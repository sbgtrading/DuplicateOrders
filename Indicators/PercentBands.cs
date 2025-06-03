//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Plots the open, high, and low values from the session starting on the current day.
	/// </summary>
	public class PercentBands : Indicator
	{
		private const int BEYOND_B = 1;
		private const int REVERSAL = 2;
		private const int STUCK = 3;
		private DateTime			currentDate			=	Core.Globals.MinDate;
		private double				currentOpen			=	double.MinValue;
		private DateTime			lastDate			= 	Core.Globals.MinDate;
		private SessionIterator		sessionIterator;
		private List<double> Pcts = new List<double>();
		private double H=0;
		private double L=0;
		private SortedDictionary<DateTime, double[]> DailyHLPct = new SortedDictionary<DateTime, double[]>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "";
				Name						= "Percent Bands";
				IsAutoScale					= false;
				DrawOnPricePanel			= true;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				BarsRequiredToPlot			= 0;
				pPercentagesStr = "0.5, 1.2, 2";
				pMultiplier = 1;
				pRegionOpacity = 10;

				AddPlot(new Stroke(Brushes.Red,		DashStyleHelper.Dash, 1), PlotStyle.Cross, "PctC High");
				AddPlot(new Stroke(Brushes.Magenta,	DashStyleHelper.Dash, 1), PlotStyle.Cross, "PctB High");
				AddPlot(new Stroke(Brushes.Goldenrod,	DashStyleHelper.Dash, 1), PlotStyle.Cross, "PctA High");
				AddPlot(new Stroke(Brushes.Goldenrod,	DashStyleHelper.Dash, 1), PlotStyle.Cross, "PctA Low");
				AddPlot(new Stroke(Brushes.Green,	DashStyleHelper.Dash, 1), PlotStyle.Cross, "PctB Low");
				AddPlot(new Stroke(Brushes.Lime,	DashStyleHelper.Dash, 1), PlotStyle.Cross, "PctC Low");
				AddPlot(new Stroke(Brushes.White,	DashStyleHelper.Dash, 2), PlotStyle.Dot, "Avg High");
				AddPlot(new Stroke(Brushes.White,	DashStyleHelper.Dash, 2), PlotStyle.Dot, "Avg Low");
			}
			else if (State == State.Configure)
			{
				currentDate			= Core.Globals.MinDate;
				currentOpen			= double.MinValue;
				lastDate			= Core.Globals.MinDate;
				Plots[0].DashStyleHelper = DashStyleHelper.Solid;
				Plots[0].PlotStyle = PlotStyle.Dot;
				Plots[0].Width = 1;
				Plots[1].DashStyleHelper = DashStyleHelper.Solid;
				Plots[1].PlotStyle = PlotStyle.Dot;
				Plots[1].Width = 1;
				Plots[2].DashStyleHelper = DashStyleHelper.Solid;
				Plots[2].PlotStyle = PlotStyle.Dot;
				Plots[2].Width = 1;
				Plots[3].DashStyleHelper = DashStyleHelper.Solid;
				Plots[3].PlotStyle = PlotStyle.Dot;
				Plots[3].Width = 1;
				Plots[4].DashStyleHelper = DashStyleHelper.Solid;
				Plots[4].PlotStyle = PlotStyle.Dot;
				Plots[5].DashStyleHelper = DashStyleHelper.Solid;
				Plots[5].PlotStyle = PlotStyle.Dot;
			}
			else if (State == State.DataLoaded)
			{
				sessionIterator = new SessionIterator(Bars);
				var s = pPercentagesStr.Split(new char[]{',','|', ' '}, StringSplitOptions.None);
				foreach(var v in s){
					double val = 0;
					if(double.TryParse(v, out val))
						Pcts.Add(val * pMultiplier);
				}
				Pcts.Sort();
				if(Pcts.Count>1){
					if(Pcts[0] > Pcts.Max()) Pcts.Reverse();
				}
			}
			else if (State == State.Historical)
			{
				if (!Bars.BarsType.IsIntraday)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", Custom.Resource.CurrentDayOHLError, TextPosition.BottomRight);
					Log(Custom.Resource.CurrentDayOHLError, LogLevel.Error);
				}
			}
		}

		bool PrintedResults = false;
		SortedDictionary<DayOfWeek, List<double>> TimeInExtreme = new SortedDictionary<DayOfWeek, List<double>>();
		SortedDictionary<DayOfWeek, List<double>> TimeAboveMid = new SortedDictionary<DayOfWeek, List<double>>();
		SortedDictionary<DayOfWeek, List<double>> TimeInMid = new SortedDictionary<DayOfWeek, List<double>>();
		SortedDictionary<DateTime, int> AboveBCloseBelowB = new SortedDictionary<DateTime, int>();
		SortedDictionary<DateTime, int> BelowBCloseAboveB = new SortedDictionary<DateTime, int>();
		double AvgHighPrice = double.MinValue;
		double AvgLowPrice = double.MinValue;
		double devH = double.MinValue;
		double devL = double.MinValue;
		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday) {
				Draw.TextFixed(this,"error","PercentBands only works on intraday data", TextPosition.Center);
				return;
			}

			lastDate 		= currentDate;
			currentDate 	= sessionIterator.GetTradingDay(Time[0]);
			
			if (lastDate != currentDate)
			{
//Print("Adding new date: "+lastDate.ToShortDateString()+"   time: "+Times[0][0].ToString());
				if(currentOpen != double.MinValue){
					if(false)
						DailyHLPct[lastDate.Date] = new double[2]{(H-currentOpen)/currentOpen,(currentOpen-L)/currentOpen};
					else{
						double range = H-L;
						double mid = (H+L)/2.0;
						DailyHLPct[lastDate.Date] = new double[2]{(H-mid)/mid,(mid-L)/mid};
					}
				}
				double sumH = 0;
				double sumL = 0;
				double AvgHighPct = 0;
				double AvgLowPct = 0;
				devH = double.MinValue;
				devL = double.MinValue;
				var DOWdata = DailyHLPct.Where(k=>k.Key.DayOfWeek == currentDate.DayOfWeek).Select(k=>k.Value).ToList();
				AvgHighPrice = double.MinValue;
				AvgLowPrice = double.MinValue;
				if(DOWdata!=null && DOWdata.Count>0){
//if(currentDate.Day==10) Print("May 10th **************************   DOWdata.Count: "+DOWdata.Count);
					foreach(var v in DOWdata){
						sumH += v[0];
						sumL += v[1];
					}
					AvgHighPct += sumH/DOWdata.Count;
					AvgHighPct += sumL/DOWdata.Count;
					AvgHighPrice = Open[0] + sumH*Open[0] / DOWdata.Count;
					AvgLowPrice = Open[0] - sumL*Open[0] / DOWdata.Count;
					sumH = 0;//variance for the highs
					sumL = 0;//variance for the lows
					foreach(var v in DOWdata){
						sumH += Math.Pow(v[0]-AvgHighPct,2);
						sumL += Math.Pow(v[1]-AvgLowPct,2);
					}
					devH = Math.Sqrt(sumH/DOWdata.Count) * Open[0] * this.pMultiplier;
					devL = Math.Sqrt(sumL/DOWdata.Count) * Open[0] * this.pMultiplier;
				}
				currentOpen = Open[0];
				H = High[0];
				L = Low[0];
//				Draw.VerticalLine(this,CurrentBar.ToString(),Times[0][0],Brushes.Yellow);
			}else{
				H = Math.Max(H,High[0]);
				L = Math.Min(L,Low[0]);
			}
			if(AvgHighPrice != double.MinValue){
				AvgHigh[0] = AvgHighPrice;
				AvgLow[0] = AvgLowPrice;
			}

			if(currentOpen > double.MinValue){
				if(pRegionOpacity>0 && CurrentBar>1 && Values[0].IsValidDataPoint(1)){
					Draw.Region(this,"sellzone",Times[0].GetValueAt(0), Times[0].GetValueAt(CurrentBar-1), Values[0], Values[2], Brushes.Transparent, Brushes.Red, pRegionOpacity);
					Draw.Region(this,"buyzone",Times[0].GetValueAt(0), Times[0].GetValueAt(CurrentBar-1), Values[3], Values[5], Brushes.Transparent, Brushes.Green, pRegionOpacity);
				}
//				if(Pcts.Count>0){
//					var diff = currentOpen * Pcts[0]/100.0;
//					PctAHigh[0] = currentOpen + diff;
//					PctALow[0] = currentOpen - diff;
////Print(PctAHigh[0]+" / "+PctALow[0]);
//				}
//				if(Pcts.Count>1){
//					var diff = currentOpen * Pcts[1]/100.0;
//					PctBHigh[0] = currentOpen + diff;
//					PctBLow[0] = currentOpen - diff;
//				}
//				if(Pcts.Count>2){
//					var diff = currentOpen * Pcts[2]/100.0;
//					PctCHigh[0] = currentOpen + diff;
//					PctCLow[0] = currentOpen - diff;
//				}
//if(currentDate.Day==10) Print("**************************   AvgHighPrice: "+AvgHighPrice);
				if(AvgHighPrice != double.MinValue){
					PctBHigh[0] = AvgHigh[0];
					PctBLow[0] = AvgLow[0];
					if(devH!=double.MinValue){
						PctCHigh[0] = PctBHigh[0] + devH;
						PctAHigh[0] = PctBHigh[0] - devH;
						PctALow[0] = PctBLow[0] + devL;
						PctCLow[0] = PctBLow[0] - devL;
					}
				}

				if(CurrentBar > 3){
					var dow = Times[0][0].DayOfWeek;
					if(!TimeAboveMid.ContainsKey(dow)) TimeAboveMid[dow] = new List<double>();
					if(!TimeInExtreme.ContainsKey(dow)) TimeInExtreme[dow] = new List<double>();
					if(!TimeInMid.ContainsKey(dow)) TimeInMid[dow] = new List<double>();
					var ts = new TimeSpan(Times[0][1].Ticks - Times[0][2].Ticks);
					if(Bars.BarsPeriod.BarsPeriodType==BarsPeriodType.Minute) ts = new TimeSpan(0,Bars.BarsPeriod.Value,0);
					if(Closes[0][1] > PctAHigh[0] && Closes[0][1] <= PctBHigh[0]) TimeAboveMid[dow].Add(ts.TotalMinutes);
					else if(Closes[0][1] < PctALow[0] && Closes[0][1] >= PctBLow[0]) TimeAboveMid[dow].Add(ts.TotalMinutes);
					else if(Closes[0][1] > PctBHigh[0]) TimeInExtreme[dow].Add(ts.TotalMinutes);
					else if(Closes[0][1] < PctBLow[0]) TimeInExtreme[dow].Add(ts.TotalMinutes);
					else if(Bars.BarsSinceNewTradingDay>1) TimeInMid[dow].Add(ts.TotalMinutes);
					var d = Times[0][1].Date;
					if(Closes[0][0] > PctBHigh[0] && !AboveBCloseBelowB.ContainsKey(d)) {AboveBCloseBelowB[d] = BEYOND_B;}//Print(d.DayOfWeek.ToString()+"   above B");}
					if(Closes[0][0] < PctBLow[0] && !BelowBCloseAboveB.ContainsKey(d))  {BelowBCloseAboveB[d] = BEYOND_B;}//Print(d.DayOfWeek.ToString()+"   below B");}
					if(Bars.IsLastBarOfSession){
						//BackBrushes[0] = Brushes.Yellow;
						if(AboveBCloseBelowB.ContainsKey(d) && Closes[0][1] < PctBHigh[1]) {AboveBCloseBelowB[d] = REVERSAL;}//Print(d.DayOfWeek.ToString()+"   Reversed out of B");}
						else if(AboveBCloseBelowB.ContainsKey(d) && Closes[0][1] >= PctBHigh[1]) {AboveBCloseBelowB[d] = STUCK;}//Print(d.DayOfWeek.ToString()+"   Stuck in B");}
						if(BelowBCloseAboveB.ContainsKey(d) && Closes[0][1] > PctBLow[1]) {BelowBCloseAboveB[d] = REVERSAL;}//Print(d.DayOfWeek.ToString()+"   Reversed out of B");}
						else if(BelowBCloseAboveB.ContainsKey(d) && Closes[0][1] <= PctBLow[1]) {BelowBCloseAboveB[d] = STUCK;}//Print(d.DayOfWeek.ToString()+"   Stuck in B");}
					}
				}
			}
			if(CurrentBar > Bars.Count-3 && !PrintedResults){
				string msg = "Time beyond B on "+Times[0][0].DayOfWeek.ToString();
				PrintedResults = true;
				Print("---------------------------------");
				Print(Instrument.MasterInstrument.Name+"  "+Bars.BarsPeriod.ToString());
				double max = 0;
				Print("--- Time between A and B: ");
				#region -- calculate max time difference, to check for errors --
				if(Bars.BarsPeriod.BarsPeriodType != BarsPeriodType.Minute) {
					foreach(var kvp in TimeAboveMid){
						foreach(var kk in kvp.Value) if(kk>max) max = kk;
					}
					Print("   max diff was: "+max);
				}
				#endregion
				foreach(var kvp in TimeAboveMid){
					var sum = kvp.Value.Sum();
					var sumInMid = TimeInMid[kvp.Key].Sum();
					Print(kvp.Key+":   "+sum.ToString("0.0")+"  of  "+sumInMid.ToString("0.0")+" in mid area,  "+(sum/(sum+sumInMid)).ToString("0.0%"));
				}
				Print("--- Time beyond B: ");
				#region -- calculate max time difference, to check for errors --
				if(Bars.BarsPeriod.BarsPeriodType != BarsPeriodType.Minute) {
					max = 0;
					foreach(var kvp in TimeInExtreme){
						foreach(var kk in kvp.Value) if(kk>max) max = kk;
					}
					Print("   max diff was: "+max);
				}
				#endregion
				foreach(var kvp in TimeInExtreme){
					var sum = kvp.Value.Sum();
					var sumInMid = TimeInMid[kvp.Key].Sum();
					Print(kvp.Key+":   "+sum.ToString("0.0")+"  of  "+sumInMid.ToString("0.0")+" in mid area,  "+(sum/(sum+sumInMid)).ToString("0.0%"));
					if(kvp.Key==Times[0][0].DayOfWeek) {
						string s = sum.ToString("0.0")+"  of  "+sumInMid.ToString("0.0")+" in mid area,  "+(sum/(sum+sumInMid)).ToString("0.0%");
						msg = msg + "\n "+s;
						int count = 0;
						int ReversalCount = 0;
						int StuckCount = 0;
						foreach(var dkvp in AboveBCloseBelowB){
							//if(dkvp.Key.DayOfWeek == Times[0][0].DayOfWeek)
							{
								count++;
								Print(dkvp.Key.DayOfWeek.ToString()+":  "+dkvp.Value);
								if(dkvp.Value==REVERSAL){
									ReversalCount++;
									Print("   Reversed out of B");
								}
								if(dkvp.Value==STUCK){
									StuckCount++;
									Print("   Stuck in B");
								}
							}
						}
						msg = msg + "\n  Reversed down out of B: "+ReversalCount+"  Stuck: "+StuckCount;
						count = 0;
						ReversalCount = 0;
						StuckCount = 0;
						foreach(var dkvp in BelowBCloseAboveB){
							//if(dkvp.Key.DayOfWeek == Times[0][0].DayOfWeek)
							{
								count++;
								Print(dkvp.Key.DayOfWeek.ToString()+":  "+dkvp.Value);
								if(dkvp.Value==REVERSAL){
									ReversalCount++;
									Print("   Reversed out of B");
								}
								if(dkvp.Value==STUCK){
									StuckCount++;
									Print("   Stuck in B");
								}
							}
						}
						msg = msg + "\n  Reversed up out of B: "+ReversalCount+"  Stuck: "+StuckCount;
						Draw.TextFixed(this,"info",msg, TextPosition.BottomLeft);
					}
				}
				Print("--- Total Time outside of mid: ");
				#region -- calculate max time difference, to check for errors --
				if(Bars.BarsPeriod.BarsPeriodType != BarsPeriodType.Minute) {
					max = 0;
					foreach(var kvp in TimeInMid){
						foreach(var kk in kvp.Value) if(kk>max) max = kk;
					}
					Print("   max diff of time in mid was: "+max);
				}
				#endregion
				foreach(var kvp in TimeInExtreme){
					var sum = kvp.Value.Sum() + TimeAboveMid[kvp.Key].Sum();
					var sumInMid = TimeInMid[kvp.Key].Sum();
					Print(kvp.Key+":   "+sum.ToString("0.0")+"  of  "+sumInMid.ToString("0.0")+" in mid area,  "+(sum/(sum+sumInMid)).ToString("0.0%"));
				}
			}

		}

		#region Plots
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PctCHigh
		{
			get { return Values[0]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PctBHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PctAHigh
		{
			get { return Values[2]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PctALow
		{
			get { return Values[3]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PctBLow
		{
			get { return Values[4]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PctCLow
		{
			get { return Values[5]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> AvgHigh
		{
			get { return Values[6]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> AvgLow
		{
			get { return Values[7]; }
		}
		#endregion
		#region Properties

		[Display(Order = 10, Name = "Pct (3)", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public string  pPercentagesStr
		{ get; set; }

		[Display(Order = 20, Name = "Multiplier", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public double  pMultiplier
		{ get; set; }
		
		[Display(Order = 30, Name = "Region Opacity", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public int pRegionOpacity
		{get;set;}

		#endregion
		
		public override string FormatPriceMarker(double price)
		{
			return Instrument.MasterInstrument.FormatPrice(price);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PercentBands[] cachePercentBands;
		public PercentBands PercentBands()
		{
			return PercentBands(Input);
		}

		public PercentBands PercentBands(ISeries<double> input)
		{
			if (cachePercentBands != null)
				for (int idx = 0; idx < cachePercentBands.Length; idx++)
					if (cachePercentBands[idx] != null &&  cachePercentBands[idx].EqualsInput(input))
						return cachePercentBands[idx];
			return CacheIndicator<PercentBands>(new PercentBands(), input, ref cachePercentBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PercentBands PercentBands()
		{
			return indicator.PercentBands(Input);
		}

		public Indicators.PercentBands PercentBands(ISeries<double> input )
		{
			return indicator.PercentBands(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PercentBands PercentBands()
		{
			return indicator.PercentBands(Input);
		}

		public Indicators.PercentBands PercentBands(ISeries<double> input )
		{
			return indicator.PercentBands(input);
		}
	}
}

#endregion
