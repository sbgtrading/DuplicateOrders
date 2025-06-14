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
	public class RenkoBarsType : BarsType
	{
		private			double			offset;
		private			double			renkoHigh;
		private			double			renkoLow;

		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 2;

		public override string ChartLabel(DateTime time) => time.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);

		public override int GetInitialLookBackDays(BarsPeriod period, TradingHours tradingHours, int barsBack) => 3;

		public override double GetPercentComplete(Bars bars, DateTime now) => 0;

		public override bool IsRemoveLastBarSupported => true;

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			offset = bars.BarsPeriod.Value * bars.Instrument.MasterInstrument.TickSize;
			bool isNewSession = SessionIterator.IsNewSession(time, isBar);
			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);

			if (bars.Count == 0 || bars.IsResetOnNewTradingDay && isNewSession)
			{
				if (bars.Count > 0)
				{
					// Close out last bar in session and set open == close
					double		lastBarClose	= bars.GetClose(bars.Count - 1);
					DateTime	lastBarTime		= bars.GetTime(bars.Count - 1);
					long		lastBarVolume	= bars.GetVolume(bars.Count - 1);
					RemoveLastBar(bars);
					AddBar(bars, lastBarClose, lastBarClose, lastBarClose, lastBarClose, lastBarTime, lastBarVolume);
				}

				renkoHigh	= close + offset;
				renkoLow	= close - offset;

				isNewSession = SessionIterator.IsNewSession(time, isBar);
				if (isNewSession)
					SessionIterator.GetNextSession(time, isBar);

				AddBar(bars, close, close, close, close, time, volume);
				bars.LastPrice = close;

				return;
			}

			double		barOpen		= bars.GetOpen(bars.Count - 1);
			double		barHigh		= bars.GetHigh(bars.Count - 1);
			double		barLow		= bars.GetLow(bars.Count - 1);
			long		barVolume	= bars.GetVolume(bars.Count - 1);
			DateTime	barTime		= bars.GetTime(bars.Count - 1);

			if (renkoHigh.ApproxCompare(0.0) == 0 || renkoLow.ApproxCompare(0.0) == 0)
			{
				if (bars.Count == 1)
				{
					renkoHigh	= barOpen + offset;
					renkoLow	= barOpen - offset;
				}
				else if (bars.GetClose(bars.Count - 2) > bars.GetOpen(bars.Count - 2))
				{
					renkoHigh	= bars.GetClose(bars.Count - 2) + offset;
					renkoLow	= bars.GetClose(bars.Count - 2) - offset * 2;
				}
				else
				{
					renkoHigh	= bars.GetClose(bars.Count - 2) + offset * 2;
					renkoLow	= bars.GetClose(bars.Count - 2) - offset;
				}
			}

			if (close.ApproxCompare(renkoHigh) >= 0)
			{
				if (barOpen.ApproxCompare(renkoHigh - offset) != 0
					|| barHigh.ApproxCompare(Math.Max(renkoHigh - offset, renkoHigh)) != 0
					|| barLow.ApproxCompare(Math.Min(renkoHigh - offset, renkoHigh)) != 0)
				{
					RemoveLastBar(bars);
					AddBar(bars, renkoHigh - offset, Math.Max(renkoHigh - offset, renkoHigh), Math.Min(renkoHigh - offset, renkoHigh), renkoHigh, barTime, barVolume);
				}

				renkoLow	= renkoHigh - 2.0 * offset;
				renkoHigh	+= offset;

				isNewSession = SessionIterator.IsNewSession(time, isBar);
				if (isNewSession)
					SessionIterator.GetNextSession(time, isBar);

				while (close.ApproxCompare(renkoHigh) >= 0)	// Add empty bars to fill gap if price jumps
				{
					AddBar(bars, renkoHigh - offset, Math.Max(renkoHigh - offset, renkoHigh), Math.Min(renkoHigh - offset, renkoHigh), renkoHigh, time, 0);
					renkoLow	= renkoHigh - 2.0 * offset;
					renkoHigh += offset;
				}

				// Add final partial bar
				AddBar(bars, renkoHigh - offset, Math.Max(renkoHigh - offset, close), Math.Min(renkoHigh - offset, close), close, time, volume);
			}
			else
				if (close.ApproxCompare(renkoLow) <= 0)
				{
					if (barOpen.ApproxCompare(renkoLow + offset) != 0
						|| barHigh.ApproxCompare(Math.Max(renkoLow + offset, renkoLow)) != 0
						|| barLow.ApproxCompare(Math.Min(renkoLow + offset, renkoLow)) != 0)
					{
						RemoveLastBar(bars);
						AddBar(bars, renkoLow + offset, Math.Max(renkoLow + offset, renkoLow), Math.Min(renkoLow + offset, renkoLow), renkoLow, barTime, barVolume);
					}

					renkoHigh	= renkoLow + 2.0 * offset;
					renkoLow	-= offset;

					isNewSession = SessionIterator.IsNewSession(time, isBar);
					if (isNewSession)
						SessionIterator.GetNextSession(time, isBar);

					while (close.ApproxCompare(renkoLow) <= 0)	// Add empty bars to fill gap if price jumps
					{
						AddBar(bars, renkoLow + offset, Math.Max(renkoLow + offset, renkoLow), Math.Min(renkoLow + offset, renkoLow), renkoLow, time, 0);
						renkoHigh	= renkoLow + 2.0 * offset;
						renkoLow	-= offset;
					}

					// Add final partial bar
					AddBar(bars, renkoLow + offset, Math.Max(renkoLow + offset, close), Math.Min(renkoLow + offset, close), close, time, volume);
				}
				else
					UpdateBar(bars, close, close, close, time, volume);

			bars.LastPrice	= close;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name				= Custom.Resource.NinjaScriptBarsTypeRenko;
				BarsPeriod			= new BarsPeriod { BarsPeriodType = BarsPeriodType.Renko };
				BuiltFrom			= BarsPeriodType.Tick;
				DaysToLoad			= 3;
				DefaultChartStyle	= Gui.Chart.ChartStyleType.OpenClose;
				IsIntraday			= true;
				IsTimeBased			= false;
			}
			else if (State == State.Configure)
			{
				Name				= string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeRenko, BarsPeriod.Value);

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));

				SetPropertyName("Value", Custom.Resource.NinjaScriptBarsTypeRenkoBrickSize);
			}
		}
	}
}
