// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class MonthBarsType : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 1;

		public override string ChartLabel(DateTime time) => time.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.YearMonthPattern);

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) => barsPeriod.Value * barsBack * 31;

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			if (now.Date <= bars.LastBarTime.Date)
			{
				int month = now.Month;
				int daysInMonth = month == 2 ? DateTime.IsLeapYear(now.Year) ? 29 : 28 : month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12 ? 31 : 30;
				return (daysInMonth - bars.LastBarTime.Date.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.Value) / daysInMonth;
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

				if (barTime.Month <= bars.LastBarTime.Month && barTime.Year == bars.LastBarTime.Year || barTime.Year < bars.LastBarTime.Year)
				{
					if (high.ApproxCompare(bars.GetHigh(bars.Count - 1)) != 0 || low.ApproxCompare(bars.GetLow(bars.Count - 1)) != 0 || close.ApproxCompare(bars.GetClose(bars.Count - 1)) != 0 || volume > 0)
						UpdateBar(bars, high, low, close, bars.LastBarTime, volume);
				}
				else
					AddBar(bars, open, high, low, close, TimeToBarTime(barTime, bars.BarsPeriod.Value), volume);
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptBarsTypeMonth;
				BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Month };
				BuiltFrom		= BarsPeriodType.Day;
				DaysToLoad		= 5475;
				IsIntraday		= false;
				IsTimeBased		= true;
			}
			else if (State == State.Configure)
			{
				Name = $"{(BarsPeriod.Value == 1 ? Custom.Resource.DataBarsTypeMonthly : string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeMonth, BarsPeriod.Value))}{(BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty)}";

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}

		private static DateTime TimeToBarTime(DateTime time, int periodValue)
		{
			DateTime result = new(time.Year, time.Month, 1); 
			for (int i = 0; i < periodValue; i++)
				result = result.AddMonths(1);

			return result.AddDays(-1);
		}
	}
}
