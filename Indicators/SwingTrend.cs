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
	public class SwingTrend : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "SwingTrend";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				SwingPct					= 0.5;
				pMultiplier = 1.0;
				AddPlot(new Stroke(Brushes.Orange,3), PlotStyle.Bar, "Trend");
				AddPlot(Brushes.Yellow, "Pct");
			}
			else if (State == State.Configure)
			{
			}
		}

		double ExtremePrice = 0;
		double ReversalPriceUp = double.MinValue;
		double ReversalPriceDown = double.MinValue;
		int id = 0;
		protected override void OnBarUpdate()
		{
			if(CurrentBar<4) {
				Trend[0] = 1;
				ExtremePrice = Close[0];
				return;
			}

			Trend[0] = Trend[1];

			if(pMultiplier>0){
				var factor = 100 * pMultiplier / ExtremePrice;
				if(Trend[0] == 1){
					Pct[0] = (Low[0] - ExtremePrice) * factor;
				}
				else if(Trend[0] == -1){
					Pct[0] = (High[0] - ExtremePrice) * factor;
				}
			}

			var ReversalDistance = ExtremePrice * SwingPct/100.0;
			ReversalPriceUp = ExtremePrice + ReversalDistance;
			ReversalPriceDown = ExtremePrice - ReversalDistance;
			if(Trend[0] == -1 && High[0] > ReversalPriceUp){
				Trend[0] = 1;
				id = CurrentBar;
			}
			if(Trend[0] == 1 && Low[0] < ReversalPriceDown){
				Trend[0] = -1;
				id = CurrentBar;
			}
			if(Trend[0] == 1){
				ExtremePrice = Math.Max(ExtremePrice, High[0]);
			}else if(Trend[0] == -1){
				ExtremePrice = Math.Min(ExtremePrice, Low[0]);
			}
			Draw.Dot(this,$"{id}1 ",false,0,ReversalPriceDown,Brushes.Pink);
			Draw.Dot(this,$"{id}2 ",false,0,ReversalPriceUp,Brushes.Cyan);
			if(Trend[0] < 0) PlotBrushes[0][0]=Brushes.Pink;
			if(Trend[0] > 0) PlotBrushes[0][0]=Brushes.Lime;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Swing Pct", Order=10, GroupName="Parameters", Description="1.5 = 1.5%")]
		public double SwingPct
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Multiplier", Order=20, GroupName="Parameters", Description="Expand the Pct plot for easier viewing")]
		public double pMultiplier
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Pct
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SwingTrend[] cacheSwingTrend;
		public SwingTrend SwingTrend(double swingPct, double pMultiplier)
		{
			return SwingTrend(Input, swingPct, pMultiplier);
		}

		public SwingTrend SwingTrend(ISeries<double> input, double swingPct, double pMultiplier)
		{
			if (cacheSwingTrend != null)
				for (int idx = 0; idx < cacheSwingTrend.Length; idx++)
					if (cacheSwingTrend[idx] != null && cacheSwingTrend[idx].SwingPct == swingPct && cacheSwingTrend[idx].pMultiplier == pMultiplier && cacheSwingTrend[idx].EqualsInput(input))
						return cacheSwingTrend[idx];
			return CacheIndicator<SwingTrend>(new SwingTrend(){ SwingPct = swingPct, pMultiplier = pMultiplier }, input, ref cacheSwingTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SwingTrend SwingTrend(double swingPct, double pMultiplier)
		{
			return indicator.SwingTrend(Input, swingPct, pMultiplier);
		}

		public Indicators.SwingTrend SwingTrend(ISeries<double> input , double swingPct, double pMultiplier)
		{
			return indicator.SwingTrend(input, swingPct, pMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SwingTrend SwingTrend(double swingPct, double pMultiplier)
		{
			return indicator.SwingTrend(Input, swingPct, pMultiplier);
		}

		public Indicators.SwingTrend SwingTrend(ISeries<double> input , double swingPct, double pMultiplier)
		{
			return indicator.SwingTrend(input, swingPct, pMultiplier);
		}
	}
}

#endregion
