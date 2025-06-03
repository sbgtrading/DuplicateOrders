//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Parabolic SAR according to Stocks and Commodities magazine V 11:11 (477-479).
	/// </summary>
	public class PSARSI : Indicator
	{
		private double			af;				// Acceleration factor
		private bool			afIncreased;
		private bool			longPosition;
		private int				prevBar;
		private double			prevSAR;
		private int				reverseBar;
		private double			reverseValue;
		private double			todaySAR;		// SAR value
		private double			xp;				// Extreme Price

		private Series<double>	afSeries;
		private Series<bool>	afIncreasedSeries;
		private Series<bool>	longPositionSeries;
		private Series<int>		prevBarSeries;
		private Series<double>	prevSARSeries;
		private Series<int>		reverseBarSeries;
		private Series<double>	reverseValueSeries;
		private Series<double>	todaySARSeries;
		private Series<double>	xpSeries;
		private Series<double>	psar;
		private Brush SellSig, BuySig;

		private ISeries<double>	high;
		private ISeries<double>	low;
		private RSI rsi;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionParabolicSAR;
				Name						= "PSARSI";
				Acceleration				= 0.02;
				AccelerationStep			= 0.02;
				AccelerationMax				= 0.2;
				RSIperiod = 10;
				RSIOB     = 75;
				RSIOS     = 25;
				Calculate 					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				pBuySigColor = Brushes.Lime;
				pBuySigOpacity = 50;
				pSellSigColor = Brushes.Magenta;
				pSellSigOpacity = 50;
				pStartTime = new TimeSpan(9,30,0);
				pEndTime = new TimeSpan(15,55,0);

				AddPlot(new Stroke(Brushes.Orange, 4), PlotStyle.Dot, Custom.Resource.NinjaScriptIndicatorNameParabolicSAR);
			}
			else if (State == State.Configure)
			{
				xp				= 0.0;
				af				= 0;
				todaySAR		= 0;
				prevSAR			= 0;
				reverseBar		= 0;
				reverseValue	= 0;
				prevBar			= 0;
				afIncreased		= false;
			}
			else if (State == State.DataLoaded)
			{
				ClearOutputWindow();
				SellSig = pSellSigColor.Clone();
				SellSig.Opacity = pSellSigOpacity/100.0;
				SellSig.Freeze();
				BuySig = pBuySigColor.Clone();
				BuySig.Opacity = pBuySigOpacity/100.0;
				BuySig.Freeze();
				if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
				{
					afSeries			= new Series<double>(this);
					afIncreasedSeries	= new Series<bool>(this);
					longPositionSeries	= new Series<bool>(this);
					prevBarSeries		= new Series<int>(this);
					prevSARSeries		= new Series<double>(this);
					reverseBarSeries	= new Series<int>(this);
					reverseValueSeries	= new Series<double>(this);
					todaySARSeries		= new Series<double>(this);
					xpSeries			= new Series<double>(this);
				}
				psar			= new Series<double>(this);

				high	= Input is NinjaScriptBase ? Input : High;
				low		= Input is NinjaScriptBase ? Input : Low;
				rsi = RSI(RSIperiod,1);
			}
		}

		private bool ResultsPrinted = false;
		private class EntryExit{
			public char Direction = ' ';
			public double EntryPrice = 0;
			public DateTime EntryTime;
			public double ExitPrice = 0;
			public int ExitABar = 0;
			public double NetPts = 0;
			public EntryExit (char direction, DateTime entryTime, double entryP) { Direction = direction; EntryTime = entryTime; EntryPrice = entryP; }
		}
		private Dictionary<int, EntryExit> Trades = new Dictionary<int, EntryExit>();
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 3)
				return;

			if (CurrentBar == 3)
			{
				// Determine initial position
				longPosition= high[0] > high[1];
				xp			= longPosition ? MAX(high, CurrentBar)[0] : MIN(low, CurrentBar)[0];
				af			= Acceleration;
				psar[0]		= xp + (longPosition ? -1 : 1) * ((MAX(high, CurrentBar)[0] - MIN(low, CurrentBar)[0]) * af);
				return;
			}
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported && CurrentBar < prevBar)
			{
				af				= afSeries[0];
				afIncreased		= afIncreasedSeries[0];
				longPosition	= longPositionSeries[0];
				prevBar			= prevBarSeries[0];
				prevSAR			= prevSARSeries[0];
				reverseBar		= reverseBarSeries[0];
				reverseValue	= reverseValueSeries[0];
				todaySAR		= todaySARSeries[0];
				xp				= xpSeries[0];
			}

			// Reset accelerator increase limiter on new bars
			if (afIncreased && prevBar != CurrentBar)
				afIncreased = false;

			// Current event is on a bar not marked as a reversal bar yet
			if (reverseBar != CurrentBar)
			{
				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(psar[1] + af * (xp - psar[1]));
				for (int x = 1; x <= 2; x++)
				{
					if (longPosition)
					{
						if (todaySAR > low[x])
							todaySAR = low[x];
					}
					else
					{
						if (todaySAR < high[x])
							todaySAR = high[x];
					}
				}

				// Holding long position
				if (longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || low[0] < prevSAR)
					{
						psar[0] = todaySAR;
						prevSAR = todaySAR;
					}
					else
						psar[0] = prevSAR;

					if (high[0] > xp)
					{
						xp = high[0];
						AfIncrease();
					}
				}

				// Holding short position
				else if (!longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || high[0] > prevSAR)
					{
						psar[0] = todaySAR;
						prevSAR = todaySAR;
					}
					else
						psar[0] = prevSAR;

					if (low[0] < xp)
					{
						xp = low[0];
						AfIncrease();
					}
				}
			}

			// Current event is on the same bar as the reversal bar
			else
			{
				// Only set new xp values. No increasing af since this is the first bar.
				if (longPosition && high[0] > xp)
					xp = high[0];
				else if (!longPosition && low[0] < xp)
					xp = low[0];

				psar[0] = prevSAR;

				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(longPosition ? Math.Min(reverseValue, low[0]) : Math.Max(reverseValue, high[0]));
			}

			prevBar = CurrentBar;

			// Reverse position
			if ((longPosition && (low[0] < todaySAR || low[1] < todaySAR))
				|| (!longPosition && (high[0] > todaySAR || high[1] > todaySAR)))
				psar[0] = Reverse();
			
			#region -- RSI signal --
			var InSession = (pStartTime == pEndTime ||
							(pStartTime < pEndTime && Times[0][0].TimeOfDay >= pStartTime && Times[0][0].TimeOfDay < pEndTime) ||
							(pStartTime > pEndTime && (Times[0][0].TimeOfDay >= pStartTime || Times[0][0].TimeOfDay < pEndTime)));
			var IsAfterSession = Times[0][0].TimeOfDay >= pEndTime;
			if(rsiDir != FLAT)
				Value[0] = psar[0];
			if(rsiDir==LONG && psar[1] > high[1] && psar[0] <= high[0]){
				BackBrushes[1] = BuySig;
				Value.Reset(0);
				rsiDir = FLAT;
				foreach(var kvp in Trades.Where(k=>k.Value.ExitABar == 0 && k.Value.Direction == 'S')){
					kvp.Value.ExitABar = CurrentBars[0];
					kvp.Value.ExitPrice = Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]);
					kvp.Value.NetPts = kvp.Value.EntryPrice - kvp.Value.ExitPrice;
					Draw.Line(this, $"{kvp.Key} {kvp.Value.Direction}", false, kvp.Value.EntryTime, kvp.Value.EntryPrice, Times[0][0], kvp.Value.ExitPrice, (kvp.Value.NetPts > 0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash,2);
				}
				if(InSession)
					Trades[CurrentBars[0]] = new EntryExit('L', Times[0][0], Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]));
			}
			if(rsiDir==SHORT && psar[1] < low[1] && psar[0] >= low[0]){
				BackBrushes[1] = SellSig;
				Value.Reset(0);
				rsiDir = FLAT;
				foreach(var kvp in Trades.Where(k=>k.Value.ExitABar == 0 && k.Value.Direction == 'L')){
					kvp.Value.ExitABar = CurrentBars[0];
					kvp.Value.ExitPrice = Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]);
					kvp.Value.NetPts = kvp.Value.ExitPrice - kvp.Value.EntryPrice;
					Draw.Line(this, $"{kvp.Key} {kvp.Value.Direction}", false, kvp.Value.EntryTime, kvp.Value.EntryPrice, Times[0][0], kvp.Value.ExitPrice, (kvp.Value.NetPts > 0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash,2);
//					Print(Times[0][0].ToString()+"  LONG from "+kvp.Value.EntryPrice+", exit at "+Closes[0][0]);
				}
				if(InSession)
					Trades[CurrentBars[0]] = new EntryExit('S', Times[0][0], Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]));
				//Print(Times[0][0].ToString()+"  Short from "+Trades[CurrentBars[0]].EntryPrice);
			}
			if(rsiDir == FLAT){
				if(rsi[0] > RSIOB && psar[1] < low[1])  rsiDir = SHORT;
				if(rsi[0] < RSIOS && psar[1] > high[1]) rsiDir = LONG;
			}else if(pCancelAt50){
				if(rsi[0]<=50 && rsi[1]>50) rsiDir=FLAT;
				if(rsi[0]>=50 && rsi[1]<50) rsiDir=FLAT;
			}
			if(Times[0][1].Day !=Times[0][0].Day ||  (!InSession && IsAfterSession)){//end of day, close out all trades
				foreach(var kvp in Trades.Where(k=>k.Value.ExitABar == 0)){
					kvp.Value.ExitABar = CurrentBars[0] - 1;
					kvp.Value.ExitPrice = Instrument.MasterInstrument.RoundToTickSize(Closes[0][1]);
					kvp.Value.NetPts = kvp.Value.Direction == 'L' ? kvp.Value.ExitPrice - kvp.Value.EntryPrice : kvp.Value.EntryPrice - kvp.Value.ExitPrice;
					Draw.Line(this, $"{kvp.Key} {kvp.Value.Direction}", false, kvp.Value.EntryTime, kvp.Value.EntryPrice, Times[0][1], kvp.Value.ExitPrice, (kvp.Value.NetPts > 0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash,2);
				}
			}
			
			if(CurrentBars[0] > Bars.Count - 3 && !ResultsPrinted){
				ResultsPrinted = true;
				var name = Instruments[0].MasterInstrument.Name;
				double PV = Instrument.MasterInstrument.PointValue;
				double overallPts = 0;
				double overallwins = 0;
				double overalllosses = 0;
				double overalldaycount = 0;
				var resultsDOWstr = new List<string>();
				foreach(DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek))){
					var sumPts = 0.0;
					var wins = 0;
					var losses = 0;
					var minutesTotal = 0.0;
					var lossPts = 0.0;
					var winPts = 0.0;
					var TimeTable = new SortedDictionary<int,List<double>>();

					int dowcount = 0;
					//if(Times[0][0].DayOfWeek == dow)
					{
						dowcount = Trades.Values
						.Where(v => v.EntryTime.DayOfWeek == dow)
						.Select(v => v.EntryTime.Date)
						.Distinct()
						.Count();
					}
					overalldaycount += dowcount;
					Print($"\n{name} on "+dow.ToString()+"   "+(dowcount>0 ? dowcount.ToString():""));
					var maes = new List<double>();
					foreach (var trades in Trades.Where(k=>k.Value.EntryTime.DayOfWeek == dow && k.Value.ExitABar>0)){
						var t = trades.Value;
						var tint = t.EntryTime.Hour *100 + (t.EntryTime.Minute / 60.0 > 0.5 ? 30:0);
						if(!TimeTable.ContainsKey(tint))
							TimeTable[tint] = new List<double>() {t.NetPts};
						else
							TimeTable[tint].Add(t.NetPts);

						sumPts += t.NetPts;
						overallPts += t.NetPts;
						if(t.NetPts>0){
							winPts += t.NetPts;
							overallwins += 1;
							wins += t.NetPts > 0 ? 1 : 0;
						}else{
							lossPts += t.NetPts;
							overalllosses += 1;
							losses += t.NetPts <= 0 ? 1 : 0;
						}
						var ticks = Times[0].GetValueAt(t.ExitABar).Ticks - t.EntryTime.Ticks;
						minutesTotal += ticks/TimeSpan.TicksPerMinute;

						#region Calculate MAEs
						if(pCalculateMAE){
							var maePrice = t.EntryPrice;
							for(int bar = trades.Key + 1; bar <= t.ExitABar; bar++){
								if(t.Direction == 'L'){
									maePrice = Math.Min(maePrice, Lows[0].GetValueAt(bar));
								}else{
									maePrice = Math.Max(maePrice, Highs[0].GetValueAt(bar));
								}
							}
							maes.Add(Math.Abs(maePrice - t.EntryPrice));
						}
						#endregion
					}
					if(wins+losses>0){
						var avgPts = sumPts / (wins+losses);
						var s = Instrument.MasterInstrument.FormatPrice(avgPts);
						Print($"  {s} avg pts/trade ({(avgPts / TickSize).ToString("0")}-ticks) = {(avgPts * PV).ToString("C0")}");
						s = (wins*1.0 / (wins+losses)).ToString("0%");
						Print($"  {wins} wins and {losses} losses   {s}");
						Print($"  {((winPts/wins) * PV).ToString("C0")} Avg win  {((lossPts / losses) * PV).ToString("C0")} Avg loss");
						s = (minutesTotal/(wins+losses)).ToString("0");
						Print($"  {s} avg mins per trade");
						#region -- Calc/Print MAE --
						if(pCalculateMAE){
							maes.Sort();
							var avg = maes.Average();
							var variance = 0.0;
							foreach(var v in maes)
							variance += Math.Pow(v-avg,2);
							Print($"  MAE median: {(PV*maes[maes.Count/2]).ToString("C0")}  avg: {(PV*avg).ToString("C0")}  stddev: {(PV*Math.Sqrt(variance/maes.Count)).ToString("C0")}");
						}
						#endregion
						if(true || Times[0][0].DayOfWeek == dow){
							winPts = 0; lossPts = 0; wins = 0; losses = 0;
							var netPts = 0.0;
							foreach(var tkvp in TimeTable){
								wins = tkvp.Value.Count(k=> k>0);
								losses = tkvp.Value.Count(k=> k<=0);
								winPts = tkvp.Value.Where(k=> k>0).Sum();
								lossPts = tkvp.Value.Where(k=> k<=0).Sum();
								netPts += winPts + lossPts;
								if(wins+losses>0){
									Print($"   {tkvp.Key}  {wins}|{losses} {(wins*1.0/(wins+losses)).ToString("0%")}   {(PV*(winPts+lossPts)).ToString("C0")}");
								}
							}
							resultsDOWstr.Add($"       Net: {(PV * netPts).ToString("C0")}   {(netPts * PV / dowcount).ToString("C0")}/day  {dow}");
							Print(resultsDOWstr.Last());
						}
					}else{
						Print("no trades taken");
					}
				}
				Print("\n");
				foreach(var sss in resultsDOWstr)
					Print(sss);
				Print($"\n  Overall Net: {(PV * overallPts).ToString("C0")}  {(overallwins / (overallwins+overalllosses)).ToString("0%")}  {(overallPts * PV / overalldaycount).ToString("C0")}/day");
				Trades.Clear();
			}
			#endregion

			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				afSeries[0]				= af;
				afIncreasedSeries[0]	= afIncreased;
				longPositionSeries[0]	= longPosition;
				prevBarSeries[0]		= prevBar;
				prevSARSeries[0]		= prevSAR;
				reverseBarSeries[0]		= reverseBar;
				reverseValueSeries[0]	= reverseValue;
				todaySARSeries[0]		= todaySAR;
				xpSeries[0]				= xp;
			}
		}
		int rsiDir = 0;
		int FLAT =0;
		int SHORT = -1;
		int LONG = 1;

		#region Miscellaneous
		// Only raise accelerator if not raised for current bar yet
		private void AfIncrease()
		{
			if (!afIncreased)
			{
				af			= Math.Min(AccelerationMax, af + AccelerationStep);
				afIncreased	= true;
			}
		}

		// Additional rule. SAR for today can't be placed inside the bar of day - 1 or day - 2.
		private double TodaySAR(double tSAR)
		{
			if (longPosition)
			{
				double lowestSAR = Math.Min(Math.Min(tSAR, low[0]), low[1]);
				if (low[0] > lowestSAR)
					tSAR = lowestSAR;
			}
			else
			{
				double highestSAR = Math.Max(Math.Max(tSAR, high[0]), high[1]);
				if (high[0] < highestSAR)
					tSAR = highestSAR;
			}
			return tSAR;
		}

		private double Reverse()
		{
			double tSAR = xp;

			if ((longPosition && prevSAR > low[0]) || (!longPosition && prevSAR < high[0]) || prevBar != CurrentBar)
			{
				longPosition	= !longPosition;
				reverseBar		= CurrentBar;
				reverseValue	= xp;
				af				= Acceleration;
				xp				= longPosition ? high[0] : low[0];
				prevSAR			= tSAR;
			}
			else
				tSAR = prevSAR;
			return tSAR;
		}
		#endregion

		#region Properties
		[Range(0.00, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Acceleration", GroupName = "NinjaScriptParameters", Order = 0)]
		public double Acceleration
		{ get; set; }

		[Range(0.001, double.MaxValue), NinjaScriptProperty]
		[Display(Name = "AccelerationMax", GroupName = "NinjaScriptParameters", Order = 10, ResourceType = typeof(Custom.Resource))]
		public double AccelerationMax
		{ get; set; }

		[Range(0.001, double.MaxValue), NinjaScriptProperty]
		[Display(Name = "AccelerationStep", GroupName = "NinjaScriptParameters", Order = 20, ResourceType = typeof(Custom.Resource))]
		public double AccelerationStep
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "RSI Period", GroupName = "NinjaScriptParameters", Order = 30, ResourceType = typeof(Custom.Resource))]
		public int RSIperiod
		{get;set;}
		
		[Range(0, 100), NinjaScriptProperty]
		[Display(Name = "RSI OB", GroupName = "NinjaScriptParameters", Order = 31, ResourceType = typeof(Custom.Resource))]
		public int RSIOB
		{get;set;}
		
		[Range(0, 100), NinjaScriptProperty]
		[Display(Name = "RSI OS", GroupName = "NinjaScriptParameters", Order = 32, ResourceType = typeof(Custom.Resource))]
		public int RSIOS
		{get;set;}
		
		[Display(Name = "Buy Sig Bkg", GroupName = "Signal", Order = 10, ResourceType = typeof(Custom.Resource))]
		public Brush pBuySigColor
		{get;set;}
		
		[Range(0, 100)]
		[Display(Name = "Buy Sig Opacity", GroupName = "Signal", Order = 11, ResourceType = typeof(Custom.Resource))]
		public int pBuySigOpacity
		{get;set;}
		
		[Display(Name = "Sell Sig Bkg", GroupName = "Signal", Order = 20, ResourceType = typeof(Custom.Resource))]
		public Brush pSellSigColor
		{get;set;}

		[Range(0, 100)]
		[Display(Name = "Sell Sig Opacity", GroupName = "Signal", Order = 21, ResourceType = typeof(Custom.Resource))]
		public int pSellSigOpacity
		{get;set;}

		[Display(Name = "Start Time", GroupName = "Signal", Order = 31, ResourceType = typeof(Custom.Resource))]
		public TimeSpan pStartTime
		{get;set;}

		[Display(Name = "End Time", GroupName = "Signal", Order = 41, ResourceType = typeof(Custom.Resource))]
		public TimeSpan pEndTime
		{get;set;}

		[Display(Name = "Cancel at RSI50", Description = "Cancel a signal when RSI crosses 50 level", GroupName = " Signal", Order = 45, ResourceType = typeof(Custom.Resource))]
		public bool pCancelAt50
		{get;set;}

		[Display(Name = "Calculate MAE?", GroupName = "Signal", Order = 50, Description = "This can slow-down calculation speed", ResourceType = typeof(Custom.Resource))]
		public bool pCalculateMAE
		{get;set;}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PSARSI[] cachePSARSI;
		public PSARSI PSARSI(double acceleration, double accelerationMax, double accelerationStep, int rSIperiod, int rSIOB, int rSIOS)
		{
			return PSARSI(Input, acceleration, accelerationMax, accelerationStep, rSIperiod, rSIOB, rSIOS);
		}

		public PSARSI PSARSI(ISeries<double> input, double acceleration, double accelerationMax, double accelerationStep, int rSIperiod, int rSIOB, int rSIOS)
		{
			if (cachePSARSI != null)
				for (int idx = 0; idx < cachePSARSI.Length; idx++)
					if (cachePSARSI[idx] != null && cachePSARSI[idx].Acceleration == acceleration && cachePSARSI[idx].AccelerationMax == accelerationMax && cachePSARSI[idx].AccelerationStep == accelerationStep && cachePSARSI[idx].RSIperiod == rSIperiod && cachePSARSI[idx].RSIOB == rSIOB && cachePSARSI[idx].RSIOS == rSIOS && cachePSARSI[idx].EqualsInput(input))
						return cachePSARSI[idx];
			return CacheIndicator<PSARSI>(new PSARSI(){ Acceleration = acceleration, AccelerationMax = accelerationMax, AccelerationStep = accelerationStep, RSIperiod = rSIperiod, RSIOB = rSIOB, RSIOS = rSIOS }, input, ref cachePSARSI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PSARSI PSARSI(double acceleration, double accelerationMax, double accelerationStep, int rSIperiod, int rSIOB, int rSIOS)
		{
			return indicator.PSARSI(Input, acceleration, accelerationMax, accelerationStep, rSIperiod, rSIOB, rSIOS);
		}

		public Indicators.PSARSI PSARSI(ISeries<double> input , double acceleration, double accelerationMax, double accelerationStep, int rSIperiod, int rSIOB, int rSIOS)
		{
			return indicator.PSARSI(input, acceleration, accelerationMax, accelerationStep, rSIperiod, rSIOB, rSIOS);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PSARSI PSARSI(double acceleration, double accelerationMax, double accelerationStep, int rSIperiod, int rSIOB, int rSIOS)
		{
			return indicator.PSARSI(Input, acceleration, accelerationMax, accelerationStep, rSIperiod, rSIOB, rSIOS);
		}

		public Indicators.PSARSI PSARSI(ISeries<double> input , double acceleration, double accelerationMax, double accelerationStep, int rSIperiod, int rSIOB, int rSIOS)
		{
			return indicator.PSARSI(input, acceleration, accelerationMax, accelerationStep, rSIperiod, rSIOB, rSIOS);
		}
	}
}

#endregion
