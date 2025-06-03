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
	public class AmbushSignals : Indicator
	{
		List<double> ranges = new List<double>();
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Similar to Algostrats.com";
				Name										= "AmbushSignals";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				IsAutoScale = true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				OvershootMult = 0.1;
				pLookbackBasis = 1;
				AddPlot(new Stroke(Brushes.Blue, 5), PlotStyle.TriangleRight, "BuySignal");
				AddPlot(new Stroke(Brushes.Crimson, 5), PlotStyle.TriangleRight, "SellSignal");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 120);
				AddDataSeries(Data.BarsPeriodType.Minute, 720);
				AddDataSeries(Data.BarsPeriodType.Minute, 1440);
			}
		}

		double buylvl = double.MinValue;
		double selllvl = double.MinValue;
		protected override void OnBarUpdate()
		{
			if(BarsInProgress == pLookbackBasis){
				if(CurrentBar<2) return;
				if(IsFirstTickOfBar) ranges.Add(Highs[pLookbackBasis][1]-Lows[pLookbackBasis][1]);
				if(CurrentBar<14) return;
				while(ranges.Count>14) ranges.RemoveAt(0);
				double overshoot = ranges.Average() * OvershootMult;
				if(ranges.Count>2){
					overshoot = Math.Max(Highs[pLookbackBasis][0],Highs[pLookbackBasis][1]) - Math.Min(Lows[pLookbackBasis][0],Lows[pLookbackBasis][1]);
					overshoot = overshoot * OvershootMult;
				}

				bool c1 = Closes[pLookbackBasis][0]>Opens[pLookbackBasis][0];//upclose
				bool c2 = Closes[pLookbackBasis][1]>Opens[pLookbackBasis][1];//upclose
				if(c1 && c2)
					selllvl = Instrument.MasterInstrument.RoundToTickSize(Highs[pLookbackBasis][0] + overshoot);
				else selllvl = double.MinValue;

				c1 = Closes[pLookbackBasis][0]<Opens[pLookbackBasis][0];//downclose
				c2 = Closes[pLookbackBasis][1]<Opens[pLookbackBasis][1];//downclose
				if(c1 && c2){
//					Print(Times[0][0].ToString()+"  Low: "+Lows[pLookbackBasis]
					buylvl = Instrument.MasterInstrument.RoundToTickSize(Lows[pLookbackBasis][0] - overshoot);
				}else buylvl = double.MinValue;
			}else{
				if(buylvl != double.MinValue)  BuySignal[0] = buylvl;
				if(selllvl != double.MinValue) SellSignal[0] = selllvl;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0.001, double.MaxValue)]
		[Display(Name="OvershootMult", Description="ATR multiple for overshoot", Order=10, GroupName="Parameters")]
		public double OvershootMult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 3)]
		[Display(Name="Lookback basis (1 or 2 or 3)", Description="1=120minutes, 2=720minutes, 3=1440minutes", Order=20, GroupName="Parameters")]
		public int pLookbackBasis
		{get;set;}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySignal
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellSignal
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
		private AmbushSignals[] cacheAmbushSignals;
		public AmbushSignals AmbushSignals(double overshootMult, int pLookbackBasis)
		{
			return AmbushSignals(Input, overshootMult, pLookbackBasis);
		}

		public AmbushSignals AmbushSignals(ISeries<double> input, double overshootMult, int pLookbackBasis)
		{
			if (cacheAmbushSignals != null)
				for (int idx = 0; idx < cacheAmbushSignals.Length; idx++)
					if (cacheAmbushSignals[idx] != null && cacheAmbushSignals[idx].OvershootMult == overshootMult && cacheAmbushSignals[idx].pLookbackBasis == pLookbackBasis && cacheAmbushSignals[idx].EqualsInput(input))
						return cacheAmbushSignals[idx];
			return CacheIndicator<AmbushSignals>(new AmbushSignals(){ OvershootMult = overshootMult, pLookbackBasis = pLookbackBasis }, input, ref cacheAmbushSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AmbushSignals AmbushSignals(double overshootMult, int pLookbackBasis)
		{
			return indicator.AmbushSignals(Input, overshootMult, pLookbackBasis);
		}

		public Indicators.AmbushSignals AmbushSignals(ISeries<double> input , double overshootMult, int pLookbackBasis)
		{
			return indicator.AmbushSignals(input, overshootMult, pLookbackBasis);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AmbushSignals AmbushSignals(double overshootMult, int pLookbackBasis)
		{
			return indicator.AmbushSignals(Input, overshootMult, pLookbackBasis);
		}

		public Indicators.AmbushSignals AmbushSignals(ISeries<double> input , double overshootMult, int pLookbackBasis)
		{
			return indicator.AmbushSignals(input, overshootMult, pLookbackBasis);
		}
	}
}

#endregion
