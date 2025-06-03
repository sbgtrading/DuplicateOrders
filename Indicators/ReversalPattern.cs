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
	public class ReversalPattern : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"finds a 4 bar pattern, middle 2 bars close same direction, outer 2 bars close in opposite direction of middle 2 bars";
				Name										= "ReversalPattern";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				IsAutoScale = false;
				pBarCount = 2;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;
				AddPlot(new Stroke(Brushes.Orange,4), PlotStyle.TriangleRight, "Reversal");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			Reversal[0] = 0;
			if(CurrentBar<5) return;
			bool c1 = Median[0]>Close[0];
			bool c2 = Median[1]<Close[1];
			bool c3 = Median[2]<Close[2];
			bool c4 = Median[3]>Close[3];
			if(pBarCount == 2) {
				c1 = Median[0]<Close[0];
				c2 = Median[1]>Close[1];
				c3=true;c4=true;
			}
			if(pBarCount == 3) {
				c4=true;
			}
			if(c1 && c2 && c3 && c4){
				if(Panel==0){
					Reversal[0] = Highs[0][0]+TickSize; //buy signal
					PlotBrushes[0][0] = Brushes.Lime;
					double y = Math.Min(Lows[0][1],Lows[0][0]);//Math.Min(Lows[0][1],Math.Min(Lows[0][2],Lows[0][3]));
					Draw.Rectangle(this,CurrentBars[0].ToString(),false,Times[0][2], 
						y, Times[0][0].AddMinutes(10), Highs[0][0]+TickSize, Brushes.Blue,Brushes.Blue,3 );
				}
				else
					Reversal[0] = 1; //buy signal
			}
			c1 = Median[0]<Close[0];
			c2 = Median[1]>Close[1];
			c3 = Median[2]>Close[2];
			c4 = Median[3]<Close[3];
			if(pBarCount == 2) {
				c1 = Median[0]>Close[0];
				c2 = Median[1]<Close[1];
				c3=true;c4=true;
			}
			if(pBarCount == 3) {c4=true;}
			if(c1 && c2 && c3 && c4){
				if(Panel==0){
					Reversal[0] = Lows[0][0]-TickSize;//sell signal
					PlotBrushes[0][0] = Brushes.Magenta;
					double y = Math.Max(Highs[0][0], Highs[0][1]);//Math.Max(Highs[0][1],Math.Min(Highs[0][2],Highs[0][3]));
					Draw.Rectangle(this,CurrentBars[0].ToString(),false,Times[0][2], 
						y, Times[0][0].AddMinutes(10), Lows[0][0]-TickSize, Brushes.Red, Brushes.Red, 3);
				}
				else
					Reversal[0] = -1;//sell signal
			}
		}

		#region Properties
		[Display(Order=10, Name="Bars in pattern", GroupName="Parameters", Description="")]
		public int pBarCount
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Reversal
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ReversalPattern[] cacheReversalPattern;
		public ReversalPattern ReversalPattern()
		{
			return ReversalPattern(Input);
		}

		public ReversalPattern ReversalPattern(ISeries<double> input)
		{
			if (cacheReversalPattern != null)
				for (int idx = 0; idx < cacheReversalPattern.Length; idx++)
					if (cacheReversalPattern[idx] != null &&  cacheReversalPattern[idx].EqualsInput(input))
						return cacheReversalPattern[idx];
			return CacheIndicator<ReversalPattern>(new ReversalPattern(), input, ref cacheReversalPattern);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ReversalPattern ReversalPattern()
		{
			return indicator.ReversalPattern(Input);
		}

		public Indicators.ReversalPattern ReversalPattern(ISeries<double> input )
		{
			return indicator.ReversalPattern(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ReversalPattern ReversalPattern()
		{
			return indicator.ReversalPattern(Input);
		}

		public Indicators.ReversalPattern ReversalPattern(ISeries<double> input )
		{
			return indicator.ReversalPattern(input);
		}
	}
}

#endregion
