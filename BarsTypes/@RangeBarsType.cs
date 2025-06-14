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
	public class RangeBarsType : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 4;

		public override string ChartLabel(DateTime time) => time.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) => 1;

		public override double GetPercentComplete(Bars bars, DateTime now) => 0;

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			bool isNewSession = SessionIterator.IsNewSession(time, isBar);
			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);
			if (bars.Count == 0 || bars.IsResetOnNewTradingDay && isNewSession)
				AddBar(bars, open, high, low, close, time, volume);
			else
			{
				double		barClose	= bars.GetClose(bars.Count - 1); 
				double		barHigh		= bars.GetHigh(bars.Count - 1); 
				double		barLow		= bars.GetLow(bars.Count - 1); 
				double		tickSize	= bars.Instrument.MasterInstrument.TickSize;
				double		rangeValue	= Math.Floor(10000000.0 * bars.BarsPeriod.Value * tickSize) / 10000000.0;

				if (close.ApproxCompare(barLow + rangeValue) > 0) 
				{
					double	newClose		= barLow + rangeValue; // Every bar closes either with high or low
					if (newClose.ApproxCompare(barClose) > 0)
						UpdateBar(bars, newClose, barLow, newClose, time, 0);

					// If there's still a gap, fill with phantom bars
					double newBarOpen		= newClose + tickSize;
					while (close.ApproxCompare(newClose) > 0)
					{
						newClose			= Math.Min(close, newBarOpen + rangeValue);
						AddBar(bars, newBarOpen, newClose, newBarOpen, newClose, time, close.ApproxCompare(newClose) > 0 ? 0 : volume);
						newBarOpen			= newClose + tickSize;
					}
				}
				else if ((barHigh - rangeValue).ApproxCompare(close) > 0)
				{
					double	newClose		= barHigh - rangeValue; // Every bar closes either with high or low
					if (barClose.ApproxCompare(newClose) > 0)
						UpdateBar(bars, barHigh, newClose, newClose, time, 0);

					// if there's still a gap, fill with phantom bars
					double newBarOpen = newClose - tickSize;
					while (newClose.ApproxCompare(close) > 0)
					{
						newClose		= Math.Max(close, newBarOpen - rangeValue);
						AddBar(bars, newBarOpen, newBarOpen, newClose, newClose, time, newClose.ApproxCompare(close) > 0 ? 0 : volume);
						newBarOpen		= newClose - tickSize;
					}
				}
				else
					UpdateBar(bars, close > barHigh ? close : barHigh, close < barLow ? close : barLow, close, time, volume);
			}
			bars.LastPrice = close;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptBarsTypeRange;
				BuiltFrom		= BarsPeriodType.Tick;
				BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Range };
				DaysToLoad		= 3;
				IsIntraday		= true;
				IsTimeBased		= false;
			}
			else if (State == State.Configure)
			{
				Name = string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeRange, BarsPeriod.Value, BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)}" : string.Empty);

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}
	}
}
