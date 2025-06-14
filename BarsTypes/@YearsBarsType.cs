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
	public class YearBarsType : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 1;

		public override string ChartLabel(DateTime time) => time.ToString("yyyy", System.Globalization.CultureInfo.InvariantCulture);

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) => barsPeriod.Value * barsBack * 365;

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			if (now.Date <= bars.LastBarTime.Date)
			{							
				double daysInYear = DateTime.IsLeapYear(now.Year) ? 366 : 365;
				return (daysInYear - bars.LastBarTime.Date.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.Value) / daysInYear;
			}
			return 1;
		}
		
		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			if (bars.Count == 0)
			{
				if (isBar || bars.TradingHours.Sessions.Count == 0)
					AddBar(bars, open, high, low, close, TimeToBarTime(time, bars.BarsPeriod.Value), volume);
				else
				{
					SessionIterator.CalculateTradingDay(time, false);
					AddBar(bars, open, high, low, close, TimeToBarTime(SessionIterator.ActualTradingDayExchange, bars.BarsPeriod.Value), volume);
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

				if (barTime.Year <= bars.LastBarTime.Year)
					UpdateBar(bars, high, low, close, bars.LastBarTime, volume);
				else
					AddBar(bars, open, high, low, close, TimeToBarTime(barTime, bars.BarsPeriod.Value), volume);
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.BarsPeriodTypeNameYear;
				BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Year };
				BuiltFrom		= BarsPeriodType.Day;
				DaysToLoad		= 15000;
				IsIntraday		= false;
				IsTimeBased		= true;
			}
			else if (State == State.Configure)
			{
				Name = $"{(BarsPeriod.Value == 1 ? Custom.Resource.DataBarsTypeYearly : string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeYear, BarsPeriod.Value))}{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)}" : string.Empty)}";

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}

		private static DateTime TimeToBarTime(DateTime time, int periodValue)
		{
			DateTime result = new(time.Year, 1, 1); 
			for (int i = 0; i < periodValue; i++)
				result = result.AddYears(1);

			return result.AddDays(-1);
		}
	}
}
