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
	public class SecondsPerBar : Indicator
	{
		//SMA avg;
		double SecLimit = 0;
		bool IsRangeBar = false;

		List<double> minpbar = new List<double>();
		SortedDictionary<int, List<int>> ValleysCounts = new SortedDictionary<int,List<int>>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description		= @"";
				Name			= "Seconds Per Bar";
				Calculate			= Calculate.OnEachTick;
				IsOverlay			= false;
				DisplayInDataBox		= true;
				DrawOnPricePanel		= true;
				PaintPriceMarkers		= true;
				ScaleJustification		= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= false;
				Multiplier			= 60;
				pAvgPeriod		= 100;
				MaxSeconds		= 1000;

				AddPlot(new Stroke(Brushes.Maroon, 3), PlotStyle.Bar, @"Time/Bar");
				AddPlot(Brushes.DimGray, "Avg");
			}
			else if (State == State.Configure)
			{
				DrawOnPricePanel   = true;
				Plots[0].AutoWidth = true;
				//avg = SMA(MinPerBar, pAvgPeriod);
				SecLimit = MaxSeconds/60;
			}
			else if (State == State.DataLoaded)
			{
				IsRangeBar = BarsArray[0].BarsPeriod.ToString().IndexOf("Range") >= 0;
			}
		}


		double averageMinPerBar = 0;
		protected override void OnBarUpdate()
		{
			if(CurrentBar<6) return;
			var ts = new TimeSpan(Times[0][0].Ticks - Times[0][1].Ticks);
			MinPerBar[0] = ts.TotalMinutes * Multiplier;
			if(BarsArray[0].IsFirstBarOfSession) MinPerBar[0] = 0;
			if(Times[0][0].Day != Times[0][1].Day) MinPerBar[0] = 0;
			if(CurrentBars[0] > pAvgPeriod+5 && IsFirstTickOfBar){
				minpbar.Clear();
				for(int i = 0; i<pAvgPeriod; i++) minpbar.Add(MinPerBar[i]);
				averageMinPerBar = minpbar.Average();
			}
			Avg[0] = averageMinPerBar;
//			if(ts.TotalSeconds > SecLimit) BackBrushes[0] = Brushes.Red;

//			int index = (int)Math.Truncate(Avg[1]*100);
//			if(Avg[3]>Avg[2] && Avg[4]>Avg[3] && Avg[2] < Avg[1]){
//				if(!ValleysCounts.ContainsKey(index)) ValleysCounts[index] = new List<int>(){1};
////				ValleysCounts[index].Add(1);
//			}
//			var total_peaks = ValleysCounts.SelectMany(k=>k.Value).ToList();
//			if(total_peaks!=null && total_peaks.Count>0){
//				var keys = ValleysCounts.Keys.ToList();
//				int count = 0;
//				List<int> qualified_keys = new List<int>();
//				foreach (var k in keys){
//					count = count + ValleysCounts[k].Count;
//					if(count>total_peaks.Count*0.2) break;
//					else qualified_keys.Add(k);
//				}
//				if(qualified_keys!=null && qualified_keys.Count>0){
//					int avg_key = (int)Math.Round(qualified_keys.Average(),0);
//					if(index < avg_key) BackBrushesAll[0] = Brushes.DimGray;
//				}
//			}
			//if(MinPerBar[2] > 3 * MinPerBar[1] && MinPerBar[1]>Avg[1]) BackBrushesAll[1]=Brushes.Yellow;
		}

//end of trade data ----------------------------------------------------------------
		#region Properties
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multiplier", Order=1, GroupName="Parameters", Description="Set to '1' for minute calculation, set to '60' for seconds calc")]
		public double Multiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Avg Period", Order=5, GroupName="Parameters", Description="")]
		public int pAvgPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Warning (seconds)", Order=10, GroupName="Parameters", Description="Max number of seconds for a warning...background color will turn red if a bar finishes faster than this seconds setting")]
		public double MaxSeconds
		{ get; set; }


		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MinPerBar
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Values[1]; }
		}
		#endregion
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SecondsPerBar[] cacheSecondsPerBar;
		public SecondsPerBar SecondsPerBar(double multiplier, int pAvgPeriod, double maxSeconds)
		{
			return SecondsPerBar(Input, multiplier, pAvgPeriod, maxSeconds);
		}

		public SecondsPerBar SecondsPerBar(ISeries<double> input, double multiplier, int pAvgPeriod, double maxSeconds)
		{
			if (cacheSecondsPerBar != null)
				for (int idx = 0; idx < cacheSecondsPerBar.Length; idx++)
					if (cacheSecondsPerBar[idx] != null && cacheSecondsPerBar[idx].Multiplier == multiplier && cacheSecondsPerBar[idx].pAvgPeriod == pAvgPeriod && cacheSecondsPerBar[idx].MaxSeconds == maxSeconds && cacheSecondsPerBar[idx].EqualsInput(input))
						return cacheSecondsPerBar[idx];
			return CacheIndicator<SecondsPerBar>(new SecondsPerBar(){ Multiplier = multiplier, pAvgPeriod = pAvgPeriod, MaxSeconds = maxSeconds }, input, ref cacheSecondsPerBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SecondsPerBar SecondsPerBar(double multiplier, int pAvgPeriod, double maxSeconds)
		{
			return indicator.SecondsPerBar(Input, multiplier, pAvgPeriod, maxSeconds);
		}

		public Indicators.SecondsPerBar SecondsPerBar(ISeries<double> input , double multiplier, int pAvgPeriod, double maxSeconds)
		{
			return indicator.SecondsPerBar(input, multiplier, pAvgPeriod, maxSeconds);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SecondsPerBar SecondsPerBar(double multiplier, int pAvgPeriod, double maxSeconds)
		{
			return indicator.SecondsPerBar(Input, multiplier, pAvgPeriod, maxSeconds);
		}

		public Indicators.SecondsPerBar SecondsPerBar(ISeries<double> input , double multiplier, int pAvgPeriod, double maxSeconds)
		{
			return indicator.SecondsPerBar(input, multiplier, pAvgPeriod, maxSeconds);
		}
	}
}

#endregion
