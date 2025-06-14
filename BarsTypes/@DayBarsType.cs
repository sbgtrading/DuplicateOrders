// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class DayBarsType : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 1;

		public override string ChartLabel(DateTime time) { return time.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern); }

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) => (int) Math.Ceiling(barsPeriod.Value * barsBack * 7.0 / 4.5);

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			if (SessionIterator == null || SessionIterator.ActualTradingDayExchange == Core.Globals.MinDate) return 1;
			DateTime tradingDayBegin = SessionIterator.GetTradingDayBeginLocal(SessionIterator.ActualTradingDayExchange);
			return now > tradingDayBegin && now < SessionIterator.ActualTradingDayEndLocal ? now.Subtract(tradingDayBegin).TotalSeconds / SessionIterator.ActualTradingDayEndLocal.Subtract(tradingDayBegin).TotalSeconds : 1;
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			if (bars.Count == 0)
			{
				if (isBar || bars.TradingHours.Sessions.Count == 0)
					AddBar(bars, open, high, low, close, time.Date, volume);
				else
				{
					SessionIterator.CalculateTradingDay(time, false);
					AddBar(bars, open, high, low, close, SessionIterator.ActualTradingDayExchange, volume);
				}
			}
			else
			{
				DateTime barTime;
				if (isBar)
					barTime = time.Date;
				else
				{
					if (SessionIterator.IsNewSession(time, false))
					{
						SessionIterator.CalculateTradingDay(time, false);
						barTime = SessionIterator.ActualTradingDayExchange;
						if (barTime < bars.LastBarTime.Date)
							barTime = bars.LastBarTime.Date; // Make sure timestamps are ascending
					}
					else
						barTime = bars.LastBarTime.Date; // Make sure timestamps are ascending
				}

				if (bars.DayCount < bars.BarsPeriod.Value || isBar && bars.Count > 0 && barTime == bars.LastBarTime.Date || !isBar && bars.Count > 0 && barTime <= bars.LastBarTime.Date)
					UpdateBar(bars, high, low, close, barTime, volume);
				else
					AddBar(bars, open, high, low, close, barTime, volume);
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptBarsTypeDay;
				BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Day };
				BuiltFrom		= BarsPeriodType.Day;
				DaysToLoad		= 365;
				IsIntraday		= false;
				IsTimeBased		= true;
			}
			else if (State == State.Configure)
			{
				Name = $"{(BarsPeriod.Value == 1 ? Custom.Resource.DataBarsTypeDaily : string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeDay, BarsPeriod.Value))}{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)}" : string.Empty)}";

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}
	}
}
