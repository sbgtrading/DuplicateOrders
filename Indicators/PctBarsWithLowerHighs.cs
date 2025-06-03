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
	public class PctBarsWithLowerHighs : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"What percent of the last 'LookbackBars' number, have lower lows";
				Name										= "Pct of Bars With Lower Highs";
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
				IsSuspendedWhileInactive					= false;
				LookbackBars					= 50;
				pShowLHorHL = "LH";
				AddPlot(Brushes.Orange, "Pct");
				AddPlot(Brushes.Cyan, "Avg");
				AddLine(new Stroke(Brushes.DarkGray,3),	.5,		"50%");
			}
			else if (State == State.Configure)
			{
			}
		}

		bool ResetSignal = false;
		protected override void OnBarUpdate()
		{
			if(CurrentBar<=LookbackBars+1) return;
			if(pShowLHorHL.ToUpper()=="LH"){
				int count_LH = 0;
				for(int rb = 0; rb<LookbackBars; rb++){
					if(Highs[0][rb] < Highs[0][rb+1]) count_LH++;
				}
				Pct[0] = ((double)count_LH) / LookbackBars;
			}
			else if(pShowLHorHL.ToUpper()=="HL"){
				int count_HL = 0;
				for(int rb = 0; rb<LookbackBars; rb++){
					if(Lows[0][rb] > Lows[0][rb+1]) count_HL++;
				}
				Pct[0] = ((double)count_HL) / LookbackBars;
			}
			double sum = 0;
			int count = 0;
			for(int rb = 0; rb<LookbackBars; rb++){
				if(Pct.IsValidDataPoint(rb)) {sum = sum + Pct[rb]; count++;}
			}
			if(count>0)	Avg[0] = sum/count;
			if(Pct[0] > 0.5) ResetSignal = true;
			if(ResetSignal){
				if(Pct[1] > Avg[1] && Pct[0] <= Avg[0]) {
					BackBrushesAll[0] = pShowLHorHL.ToUpper()=="HL"? Brushes.DarkRed : Brushes.DarkGreen;
					ResetSignal = false;
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="LookbackBars", Order=1, GroupName="Parameters")]
		public int LookbackBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show LH or HL", Order=20, GroupName="Parameters")]
		public string pShowLHorHL
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Pct
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

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PctBarsWithLowerHighs[] cachePctBarsWithLowerHighs;
		public PctBarsWithLowerHighs PctBarsWithLowerHighs(int lookbackBars, string pShowLHorHL)
		{
			return PctBarsWithLowerHighs(Input, lookbackBars, pShowLHorHL);
		}

		public PctBarsWithLowerHighs PctBarsWithLowerHighs(ISeries<double> input, int lookbackBars, string pShowLHorHL)
		{
			if (cachePctBarsWithLowerHighs != null)
				for (int idx = 0; idx < cachePctBarsWithLowerHighs.Length; idx++)
					if (cachePctBarsWithLowerHighs[idx] != null && cachePctBarsWithLowerHighs[idx].LookbackBars == lookbackBars && cachePctBarsWithLowerHighs[idx].pShowLHorHL == pShowLHorHL && cachePctBarsWithLowerHighs[idx].EqualsInput(input))
						return cachePctBarsWithLowerHighs[idx];
			return CacheIndicator<PctBarsWithLowerHighs>(new PctBarsWithLowerHighs(){ LookbackBars = lookbackBars, pShowLHorHL = pShowLHorHL }, input, ref cachePctBarsWithLowerHighs);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PctBarsWithLowerHighs PctBarsWithLowerHighs(int lookbackBars, string pShowLHorHL)
		{
			return indicator.PctBarsWithLowerHighs(Input, lookbackBars, pShowLHorHL);
		}

		public Indicators.PctBarsWithLowerHighs PctBarsWithLowerHighs(ISeries<double> input , int lookbackBars, string pShowLHorHL)
		{
			return indicator.PctBarsWithLowerHighs(input, lookbackBars, pShowLHorHL);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PctBarsWithLowerHighs PctBarsWithLowerHighs(int lookbackBars, string pShowLHorHL)
		{
			return indicator.PctBarsWithLowerHighs(Input, lookbackBars, pShowLHorHL);
		}

		public Indicators.PctBarsWithLowerHighs PctBarsWithLowerHighs(ISeries<double> input , int lookbackBars, string pShowLHorHL)
		{
			return indicator.PctBarsWithLowerHighs(input, lookbackBars, pShowLHorHL);
		}
	}
}

#endregion
