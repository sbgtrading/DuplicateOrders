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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class TimeExtensionBloodHound : Indicator
	{
		DateTime LastCalculation = DateTime.MinValue;
		double avg_min_per_bar = 0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "TimeExtension - BloodHound";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= false;
				pLongColor					= Brushes.Green;
				pShortColor					= Brushes.Red;
				pIgnoreLineColor			= false;
				pPrintToOutputWindow		= true;
				pSecondsBetweenScans = 10;
				AddPlot(Brushes.Yellow, "Bar1");
				AddPlot(Brushes.Red,    "Bar2");
				AddPlot(Brushes.Lime,   "Bar3");
				AddPlot(Brushes.Cyan,   "Bar4");
				AddPlot(Brushes.White,  "Bar5");
			}
			else if (State == State.Configure)
			{
				LongColorTxt = pLongColor.ToString();
				ShortColorTxt = pShortColor.ToString();
			}
			else if (State == State.DataLoaded){
				var bars = new List<double>();
				if(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
					avg_min_per_bar = Bars.BarsPeriod.Value;
				else if(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Second){
					avg_min_per_bar = Bars.BarsPeriod.Value / 60.0;
				}else{//non-timebased bars, range, renko, tick, volume
					for(int b = 1; b<BarsArray[0].Count; b++){
						var ts = Times[0][0] - Times[0][1];
						bars.Add(ts.TotalMinutes);
					}
					bars.Sort();
					avg_min_per_bar = bars[Math.Max(0,bars.Count/2)];
				}
			}
		}

		string LongColorTxt = "";
		string ShortColorTxt = "";
		int num = -1;
		SortedDictionary<int,int> Results = new SortedDictionary<int,int>();
		protected override void OnBarUpdate()
		{
			if(CurrentBars[0]<4) return;
try{
			var objects = DrawObjects.ToList();
			Results.Clear();
			foreach (var dob in objects) {
				if (dob.ToString().Contains("FibonacciTimeExtensions")) {
					FibonacciTimeExtensions ti = dob as FibonacciTimeExtensions;
					var pl = ti.PriceLevels.ToList();
					var t0 = ti.Anchors.First().Time;
					var t1 = ti.Anchors.Last().Time;
					if(t0>t1){
						var temp = t0;
						t0 = t1;
						t1 = temp;
					}
//Print("                  =====  CB: "+CurrentBars[0]);
					var ts = t1-t0;
					int barscount = 0;
					int bar_t0 = 0;
					if(t0 <= Times[0][0]){
						bar_t0 = BarsArray[0].GetBar(t0);
					}else{
						var tx = t0 - Times[0][0];
						bar_t0 = CurrentBars[0] + Convert.ToInt32(Math.Round(tx.TotalMinutes / avg_min_per_bar,0));
						t0 = Times[0][0].AddMinutes((bar_t0-CurrentBars[0]) * avg_min_per_bar);
					}
					int bar_t1 = 0;
					if(t1 <= Times[0][0]){
						bar_t1 = BarsArray[0].GetBar(t1);
					}else{
						var tx = t1 - Times[0][0];
//Print("tx.TotalMins: "+tx.TotalMinutes);
//Print("avg min/bar: "+avg_min_per_bar);
						bar_t1 = CurrentBars[0] + Convert.ToInt32(Math.Round(tx.TotalMinutes / avg_min_per_bar,0));
						t1 = Times[0][0].AddMinutes((bar_t1-CurrentBars[0]) * avg_min_per_bar);
					}
//					int bar_distance = Bars.BarsPeriod.ToString();
//					Print(t0.ToString()+"  to  "+t1.ToString()+"  Long: "+pLongColor.ToString());
					foreach(var x in pl){
						num = -1;
						int val = 0;
						if(pIgnoreLineColor){
							var t = t0.AddMinutes(ts.TotalMinutes * x.Value/100.0);
							if(t<=Times[0][1]){
								continue;
							}else if(t<=Times[0][0]){
								num = CurrentBars[0];
							}else{
//Print("bar t0: "+bar_t0+"   bar t1: "+bar_t1);
//Print("x.Value: "+x.Value);
								barscount = Convert.ToInt32(Math.Round(x.Value/100.0 * (bar_t1-bar_t0),0));
								num = bar_t0 + barscount;
							}
							val = num;
						}else if(LongColorTxt.CompareTo(x.Stroke.Brush.ToString())==0){
							var t = t0.AddMinutes(ts.TotalMinutes * x.Value/100.0);
							if(t<=Times[0][1]){
								continue;
							}else if(t<=Times[0][0]){
								num = CurrentBars[0];
							}else{
								barscount = Convert.ToInt32(Math.Round(x.Value/100.0 * (bar_t1-bar_t0),0));
								num = bar_t0 + barscount;
							}
							val = num;
						}else if(ShortColorTxt.CompareTo(x.Stroke.Brush.ToString())==0){
							var t = t0.AddMinutes(ts.TotalMinutes * x.Value/100.0);
							if(t<=Times[0][1]){
								continue;
							}else if(t<=Times[0][0]){
								num = CurrentBars[0];
							}else{
								barscount = Convert.ToInt32(Math.Round(x.Value/100.0 * (bar_t1-bar_t0),0));
								num = bar_t0 + barscount;
							}
							val = -num;
						}
						if(num>0){
							Results[num] = val;
						}
					}
				}
			}
			if(pSecondsBetweenScans > 0){
				var xts = new TimeSpan(DateTime.Now.Ticks - LastCalculation.Ticks);
				if(xts.TotalSeconds < pSecondsBetweenScans) return;//calculate new plot values every pSecondsBetweenScans seconds
				LastCalculation = DateTime.Now;
			}

			int p = 0;
			while(p < Plots.Length){
				Values[p].Reset(0);
				p++;
			}
			if(Results.Count>0){
if(pPrintToOutputWindow) Print("-------  "+DateTime.Now.ToString("HH:mm:ss"));
				p = 0;
				var keys = Results.Keys.ToList();
				while(p < Plots.Length && p < keys.Count){
					if(Math.Abs(Results[keys[p]]) > CurrentBar) Values[p][0] = Results[keys[p]];
if(pPrintToOutputWindow) Print(CurrentBar+"  result "+p+":  "+Values[p][0]);
					p++;
				}
			}
}catch{};
		}

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Long Color", Description="Color for long signal lines", Order=10, GroupName="Parameters")]
		public Brush pLongColor
		{ get; set; }

		[Browsable(false)]
		public string pLongColorSerializable
		{
			get { return Serialize.BrushToString(pLongColor); }
			set { pLongColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Short Color", Description="Color for short signal lines", Order=20, GroupName="Parameters")]
		public Brush pShortColor
		{ get; set; }

		[Browsable(false)]
		public string pShortColorSerializable
		{
			get { return Serialize.BrushToString(pShortColor); }
			set { pShortColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(0,1000)]
		[Display(Name="Seconds between scans", Description="0 runs on each price change, 10 runs the scan every 10-seconds", Order=25, GroupName="Parameters")]
		public int pSecondsBetweenScans
		{get;set;}

		[NinjaScriptProperty]
		[Display(Name="Ignore Line Color", Description="Ignore the long/short colors, all bar numbers will be reported as positive numbers", Order=30, GroupName="Parameters")]
		public bool pIgnoreLineColor
		{ get; set; }
		
		[Display(Name="Print results", Description="Prints the current plot values to the output window", Order=40, GroupName="Parameters")]
		public bool pPrintToOutputWindow
		{get;set;}
		#endregion

		#region --- plots ---
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Bar1
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Bar2
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Bar3
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Bar4
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Bar5
		{
			get { return Values[4]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TimeExtensionBloodHound[] cacheTimeExtensionBloodHound;
		public TimeExtensionBloodHound TimeExtensionBloodHound(Brush pLongColor, Brush pShortColor, int pSecondsBetweenScans, bool pIgnoreLineColor)
		{
			return TimeExtensionBloodHound(Input, pLongColor, pShortColor, pSecondsBetweenScans, pIgnoreLineColor);
		}

		public TimeExtensionBloodHound TimeExtensionBloodHound(ISeries<double> input, Brush pLongColor, Brush pShortColor, int pSecondsBetweenScans, bool pIgnoreLineColor)
		{
			if (cacheTimeExtensionBloodHound != null)
				for (int idx = 0; idx < cacheTimeExtensionBloodHound.Length; idx++)
					if (cacheTimeExtensionBloodHound[idx] != null && cacheTimeExtensionBloodHound[idx].pLongColor == pLongColor && cacheTimeExtensionBloodHound[idx].pShortColor == pShortColor && cacheTimeExtensionBloodHound[idx].pSecondsBetweenScans == pSecondsBetweenScans && cacheTimeExtensionBloodHound[idx].pIgnoreLineColor == pIgnoreLineColor && cacheTimeExtensionBloodHound[idx].EqualsInput(input))
						return cacheTimeExtensionBloodHound[idx];
			return CacheIndicator<TimeExtensionBloodHound>(new TimeExtensionBloodHound(){ pLongColor = pLongColor, pShortColor = pShortColor, pSecondsBetweenScans = pSecondsBetweenScans, pIgnoreLineColor = pIgnoreLineColor }, input, ref cacheTimeExtensionBloodHound);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TimeExtensionBloodHound TimeExtensionBloodHound(Brush pLongColor, Brush pShortColor, int pSecondsBetweenScans, bool pIgnoreLineColor)
		{
			return indicator.TimeExtensionBloodHound(Input, pLongColor, pShortColor, pSecondsBetweenScans, pIgnoreLineColor);
		}

		public Indicators.TimeExtensionBloodHound TimeExtensionBloodHound(ISeries<double> input , Brush pLongColor, Brush pShortColor, int pSecondsBetweenScans, bool pIgnoreLineColor)
		{
			return indicator.TimeExtensionBloodHound(input, pLongColor, pShortColor, pSecondsBetweenScans, pIgnoreLineColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TimeExtensionBloodHound TimeExtensionBloodHound(Brush pLongColor, Brush pShortColor, int pSecondsBetweenScans, bool pIgnoreLineColor)
		{
			return indicator.TimeExtensionBloodHound(Input, pLongColor, pShortColor, pSecondsBetweenScans, pIgnoreLineColor);
		}

		public Indicators.TimeExtensionBloodHound TimeExtensionBloodHound(ISeries<double> input , Brush pLongColor, Brush pShortColor, int pSecondsBetweenScans, bool pIgnoreLineColor)
		{
			return indicator.TimeExtensionBloodHound(input, pLongColor, pShortColor, pSecondsBetweenScans, pIgnoreLineColor);
		}
	}
}

#endregion
