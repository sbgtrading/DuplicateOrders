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

// Lime to Blue 2022/01/14 mrr

namespace NinjaTrader.NinjaScript.Indicators
{
// transcription from trading view : medRedas	
//Original Script > @DonovanWall && Version > @guikroth from trading view


	public class RangeFilterBuyAndSell : Indicator
	{
		
		private Series<double> diff;
		private Series<double> smoothDiff;
		private Series<double> upward;
		private Series<double> downward;
		private Brush BullBrush;
		private Brush BearBrush;
		private int condInit = 0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RangeFilterBuyAndSell";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				BullBrush						= Brushes.Blue;
				BearBrush						= Brushes.Red;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period					= 100;
				Multiplier					= 3;
				ShowArrows= true;
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Range");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Dot,1), PlotStyle.Line, "RangeU");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Dot,1), PlotStyle.Line, "RangeL");
				
				
				HighlightChart					= true;
				
				BullChart						= Brushes.DarkBlue;
				BearChart						= Brushes.DarkRed;
				
				BullOpacity						= 44;
				BearOpacity						= 44;
			}
			else if (State == State.DataLoaded)
			{
				
				diff = new Series<double>(this);
				smoothDiff = new Series<double>(this);
				upward = new Series<double>(this);
				downward = new Series<double>(this);
				
				Range[0] = 0;
				upward[0] = 0;
				downward[0] = 0;
			}
			else if (State == State.Configure)
			{
			///https://ninjatrader.com/support/forum/forum/ninjatrader-8/indicator-development/1090657-issue-with-backbrush
				
				Brush tempB = BullChart.Clone(); //Copy the brush into a temporary brush
				tempB.Opacity = BullOpacity	 / 100.0; // set the opacity
				tempB.Freeze(); // freeze the temp brush
				BullChart = tempB; // assign the temp brush value to BullChart.
				
				
				Brush tempS = BearChart.Clone(); //Copy the brush into a temporary brush
				tempS.Opacity = BearOpacity / 100.0; // set the opacity
				tempS.Freeze(); // freeze the temp brush
				BearChart = tempS; // assign the temp brush value to BearChart.
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
			{
			   	diff[0] = 0;
				return;
			} else {
				diff[0] = Math.Abs(Close[0] - Close[1]);
			}
			
			BackBrush = null;
			
			double sr = smoothRange();
			Range[0] = rangFilter(sr);

			upward[0] = Range[0] > Range[1] ? nz(upward,1) + 1 : Range[0] < Range[1] ? 0 : nz(upward,1);
			
			downward[0] = Range[0] < Range[1] ? nz(downward,1) + 1 : Range[0] > Range[1] ? 0 : nz(downward,1);
			
			RangeU[0] = Range[0] + sr;
			RangeL[0] = Range[0] - sr;
			
			PlotBrushes[0][0] = upward[0] > 0 ? BullBrush : downward[0] > 0 ? BearBrush : Brushes.Orange;
			
			Draw.Region(this, "range", CurrentBar, 0, RangeU, RangeL, null, Brushes.CornflowerBlue, 6);
			
			

			if(ShowArrows) {
				bool longCond = Close[0] > Range[0] && Close[0] > Close[1] && upward[0] > 0 || 
   				 Close[0] > Range[0] &&  Close[0] < Close[1] &&  upward[0] > 0;
			
				bool shortCond = Close[0] < Range[0] && Close[0] < Close[1] && downward[0] > 0 || 
	  				 Close[0] < Range[0] && Close[0] > Close[1] && downward[0] > 0;
				
				if(longCond && condInit <= 0 ) {
					condInit = 1;
					Draw.ArrowUp(this, CurrentBar.ToString(), true, 0, RangeL[0] - TickSize, BullBrush);
				}
				if(shortCond && condInit >= 0 ) {
					condInit = -1;
					Draw.ArrowDown(this, CurrentBar.ToString(), true, 0, RangeU[0] + TickSize, BearBrush);
				}
			}
			
			#region TradeSimple Added
			
			if (HighlightChart)
			{
				if (upward[0] > 0)
				{
					BackBrush = BullChart;
				}
				
				if (downward[0] > 0 )
				{
					BackBrush = BearChart;
				}
			}
			
			#endregion
			
			
			
		}
		
		protected double smoothRange() {
			
			int doublePer = Period*2-1;
			double avrng = EMA(diff,Period)[0];
			smoothDiff[0] = avrng;
			return EMA(smoothDiff,doublePer)[0] * Multiplier ;
		}
		protected double nz(Series<double> serie,int idx) {
			if(serie.Count<idx+1) {
				return 0;
			} else {
				return serie[idx];
			}
		}
		
		protected double rangFilter(double r) {

			double x= Close[0];
			double rngfilt =   x > nz(Range,1) ? x - r < nz(Range,1) ? nz(Range,1) : x - r : 
      			 x + r > nz(Range,1) ? nz(Range,1) : x + r;
			return rngfilt;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multiplier", Order=2, GroupName="Parameters")]
		public double Multiplier
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShowArrows", Order=3, GroupName="Entry")]
		public bool ShowArrows
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Range
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RangeU
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RangeL
		{
			get { return Values[2]; }
		}
		
		
		#region TradeSimple Added
	
		[NinjaScriptProperty]
		[Display(Name="HighlightChart", Order=0, GroupName="1. Highlight Chart")]
		public bool HighlightChart
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "Bull Color ", GroupName="1. Highlight Chart", Order=1)]
		public Brush BullChart
		{ get; set; }

		[Browsable(false)]
		public string BullChartSerialize
		{
			get { return Serialize.BrushToString(BullChart); }
   			set { BullChart = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Bull Opacity", Order=2, GroupName="1. Highlight Chart")]
		public int BullOpacity
		{ get; set; }
		
		
		
		[XmlIgnore()]
		[Display(Name = "Bear Color", GroupName="1. Highlight Chart", Order=3)]
		public Brush BearChart
		{ get; set; }

		[Browsable(false)]
		public string BearChartSerialize
		{
			get { return Serialize.BrushToString(BearChart); }
   			set { BearChart = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name="Bear Opacity", Order=4, GroupName="1. Highlight Chart")]
		public int BearOpacity
		{ get; set; }
		
		#endregion
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RangeFilterBuyAndSell[] cacheRangeFilterBuyAndSell;
		public RangeFilterBuyAndSell RangeFilterBuyAndSell(int period, double multiplier, bool showArrows, bool highlightChart, int bullOpacity, int bearOpacity)
		{
			return RangeFilterBuyAndSell(Input, period, multiplier, showArrows, highlightChart, bullOpacity, bearOpacity);
		}

		public RangeFilterBuyAndSell RangeFilterBuyAndSell(ISeries<double> input, int period, double multiplier, bool showArrows, bool highlightChart, int bullOpacity, int bearOpacity)
		{
			if (cacheRangeFilterBuyAndSell != null)
				for (int idx = 0; idx < cacheRangeFilterBuyAndSell.Length; idx++)
					if (cacheRangeFilterBuyAndSell[idx] != null && cacheRangeFilterBuyAndSell[idx].Period == period && cacheRangeFilterBuyAndSell[idx].Multiplier == multiplier && cacheRangeFilterBuyAndSell[idx].ShowArrows == showArrows && cacheRangeFilterBuyAndSell[idx].HighlightChart == highlightChart && cacheRangeFilterBuyAndSell[idx].BullOpacity == bullOpacity && cacheRangeFilterBuyAndSell[idx].BearOpacity == bearOpacity && cacheRangeFilterBuyAndSell[idx].EqualsInput(input))
						return cacheRangeFilterBuyAndSell[idx];
			return CacheIndicator<RangeFilterBuyAndSell>(new RangeFilterBuyAndSell(){ Period = period, Multiplier = multiplier, ShowArrows = showArrows, HighlightChart = highlightChart, BullOpacity = bullOpacity, BearOpacity = bearOpacity }, input, ref cacheRangeFilterBuyAndSell);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RangeFilterBuyAndSell RangeFilterBuyAndSell(int period, double multiplier, bool showArrows, bool highlightChart, int bullOpacity, int bearOpacity)
		{
			return indicator.RangeFilterBuyAndSell(Input, period, multiplier, showArrows, highlightChart, bullOpacity, bearOpacity);
		}

		public Indicators.RangeFilterBuyAndSell RangeFilterBuyAndSell(ISeries<double> input , int period, double multiplier, bool showArrows, bool highlightChart, int bullOpacity, int bearOpacity)
		{
			return indicator.RangeFilterBuyAndSell(input, period, multiplier, showArrows, highlightChart, bullOpacity, bearOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RangeFilterBuyAndSell RangeFilterBuyAndSell(int period, double multiplier, bool showArrows, bool highlightChart, int bullOpacity, int bearOpacity)
		{
			return indicator.RangeFilterBuyAndSell(Input, period, multiplier, showArrows, highlightChart, bullOpacity, bearOpacity);
		}

		public Indicators.RangeFilterBuyAndSell RangeFilterBuyAndSell(ISeries<double> input , int period, double multiplier, bool showArrows, bool highlightChart, int bullOpacity, int bearOpacity)
		{
			return indicator.RangeFilterBuyAndSell(input, period, multiplier, showArrows, highlightChart, bullOpacity, bearOpacity);
		}
	}
}

#endregion
