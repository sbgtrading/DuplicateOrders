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
	public class QQESignals : Indicator
	{
		RSI rsi;
		EMA rsima, MaAtrRsi, dar;
		Series<double> AtrRsi;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "QQESignals";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				pRSIperiod					= 14;
				pRSISmoothingPeriod			= 5;
				pQQE_FastFactor				= 4.238;
				pThreshold					= 10;
				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Dot, "BuySignal");
				AddPlot(new Stroke(Brushes.OrangeRed, 2), PlotStyle.Dot, "SellSignal");
			}
			else if (State == State.Configure)
			{
				Calculate = Calculate.OnBarClose;
				rsi = RSI(pRSIperiod, pRSISmoothingPeriod);
				AtrRsi = new Series<double>(this,MaximumBarsLookBack.TwoHundredFiftySix);
				MaAtrRsi = EMA(AtrRsi, pRSIperiod*2 - 1);
				dar = EMA(MaAtrRsi, pRSIperiod*2 - 1);
			}else if (State == State.DataLoaded){
			}
		}
/*
RSI_Period = input(14, title='RSI Length')
SF = input(5, title='RSI Smoothing')
pQQE_FastFactor = input(4.238, title='Fast QQE Factor')
ThreshHold = input(10, title="Thresh-hold")

src = close
Wilders_Period = RSI_Period * 2 - 1

Rsi = rsi(src, RSI_Period)
RsiMa = ema(Rsi, SF)
AtrRsi = abs(RsiMa[1] - RsiMa)
MaAtrRsi = ema(AtrRsi, Wilders_Period)
dar = ema(MaAtrRsi, Wilders_Period) * pQQE_FastFactor

longband = 0.0
shortband = 0.0
trend = 0

DeltaFastAtrRsi = dar[0] * pQQE_FastFactor
newshortband = RsiMa[0] + DeltaFastAtrRsi
newlongband = RsiMa[0] - DeltaFastAtrRsi
longband := RsiMa[1] > longband[1] and RsiMa[0] > longband[1] ? max(longband[1], newlongband) : newlongband
shortband := RsiMa[1] < shortband[1] and RsiMa[0] < shortband[1] ? min(shortband[1], newshortband) : newshortband
cross_1 = cross(longband[1], RsiMa)
trend := cross(RsiMa, shortband[1]) ? 1 : cross_1 ? -1 : nz(trend[1], 1)
FastAtrRsiTL = trend == 1 ? longband : shortband

// Find all the QQE Crosses

QQExlong = 0
QQExlong := nz(QQExlong[1])
QQExshort = 0
QQExshort := nz(QQExshort[1])
QQExlong := FastAtrRsiTL < RsiMa[0] ? QQExlong + 1 : 0
QQExshort := FastAtrRsiTL > RsiMa[0] ? QQExshort + 1 : 0

//Conditions

qqeLong = QQExlong == 1 ? FastAtrRsiTL[1] - 50 : na
qqeShort = QQExshort == 1 ? FastAtrRsiTL[1] - 50 : na

// Plotting

plotshape(qqeLong, title="QQE long", text="Long", textcolor=color.white, style=shape.labelup, location=location.belowbar, color=color.green, transp=0, size=size.tiny)
plotshape(qqeShort, title="QQE short", text="Short", textcolor=color.white, style=shape.labeldown, location=location.abovebar, color=color.red, transp=0, size=size.tiny)
		*/
		double longband1 = 0;
		double shortband1 = 0;
		double longband = 0.0;
		double shortband = 0.0;
		int trend = 0;
		int trend1 = 0;
		double FastAtrRsiTL = 0;
		double FastAtrRsiTL1 = 0;
		int QQExlong = 0;
		int QQExshort = 0;
		protected override void OnBarUpdate()
		{
			if(CurrentBar<1) return;
			AtrRsi[0] = Math.Abs(rsi.Avg[1] - rsi.Avg[0]);
			if(IsFirstTickOfBar){
				longband1 = longband;
				shortband1 = shortband;
				trend1 = trend;
				FastAtrRsiTL1 = FastAtrRsiTL;
			}
			trend = 0;

			double DeltaFastAtrRsi = dar[0] * pQQE_FastFactor;
			double newshortband = rsi.Avg[0] + DeltaFastAtrRsi;
			double newlongband = rsi.Avg[0] - DeltaFastAtrRsi;
			longband = rsi.Avg[1] > longband1 && rsi.Avg[0] > longband1 ? Math.Max(longband1, newlongband) : newlongband;
			shortband = rsi.Avg[1] < shortband1 && rsi.Avg[0] < shortband1 ? Math.Min(shortband1, newshortband) : newshortband;
			bool cross_1 = longband1< rsi.Avg[1] && longband > rsi.Avg[0];
			cross_1 = cross_1 || longband1 > rsi.Avg[1] && longband < rsi.Avg[0];
			bool cross_2 = shortband1< rsi.Avg[1] && shortband > rsi.Avg[0];
			cross_2 = cross_2 || shortband1 > rsi.Avg[1] && shortband < rsi.Avg[0];
			trend = cross_2 ? 1 : cross_1 ? -1 : trend1;
			FastAtrRsiTL = trend == 1 ? longband : shortband;

			QQExlong  = FastAtrRsiTL < rsi.Avg[0] ? QQExlong + 1 : 0;
			QQExshort = FastAtrRsiTL > rsi.Avg[0] ? QQExshort + 1 : 0;

			double signal = QQExlong == 1 ? FastAtrRsiTL1 - 50 : double.MinValue;
			if(signal!= double.MinValue && signal >= 0) BuySignal[0] = Low[0];

			signal = QQExshort == 1 ? FastAtrRsiTL1 - 50 : double.MinValue;
			if(signal!= double.MinValue && signal >= 0) SellSignal[0] = High[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="pRSIperiod", Order=1, GroupName="Parameters")]
		public int pRSIperiod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="pRSISmoothingPeriod", Order=2, GroupName="Parameters")]
		public int pRSISmoothingPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="pQQE_FastFactor", Order=3, GroupName="Parameters")]
		public double pQQE_FastFactor
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="pThreshold", Order=4, GroupName="Parameters")]
		public int pThreshold
		{ get; set; }

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
		private QQESignals[] cacheQQESignals;
		public QQESignals QQESignals(int pRSIperiod, int pRSISmoothingPeriod, double pQQE_FastFactor, int pThreshold)
		{
			return QQESignals(Input, pRSIperiod, pRSISmoothingPeriod, pQQE_FastFactor, pThreshold);
		}

		public QQESignals QQESignals(ISeries<double> input, int pRSIperiod, int pRSISmoothingPeriod, double pQQE_FastFactor, int pThreshold)
		{
			if (cacheQQESignals != null)
				for (int idx = 0; idx < cacheQQESignals.Length; idx++)
					if (cacheQQESignals[idx] != null && cacheQQESignals[idx].pRSIperiod == pRSIperiod && cacheQQESignals[idx].pRSISmoothingPeriod == pRSISmoothingPeriod && cacheQQESignals[idx].pQQE_FastFactor == pQQE_FastFactor && cacheQQESignals[idx].pThreshold == pThreshold && cacheQQESignals[idx].EqualsInput(input))
						return cacheQQESignals[idx];
			return CacheIndicator<QQESignals>(new QQESignals(){ pRSIperiod = pRSIperiod, pRSISmoothingPeriod = pRSISmoothingPeriod, pQQE_FastFactor = pQQE_FastFactor, pThreshold = pThreshold }, input, ref cacheQQESignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.QQESignals QQESignals(int pRSIperiod, int pRSISmoothingPeriod, double pQQE_FastFactor, int pThreshold)
		{
			return indicator.QQESignals(Input, pRSIperiod, pRSISmoothingPeriod, pQQE_FastFactor, pThreshold);
		}

		public Indicators.QQESignals QQESignals(ISeries<double> input , int pRSIperiod, int pRSISmoothingPeriod, double pQQE_FastFactor, int pThreshold)
		{
			return indicator.QQESignals(input, pRSIperiod, pRSISmoothingPeriod, pQQE_FastFactor, pThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.QQESignals QQESignals(int pRSIperiod, int pRSISmoothingPeriod, double pQQE_FastFactor, int pThreshold)
		{
			return indicator.QQESignals(Input, pRSIperiod, pRSISmoothingPeriod, pQQE_FastFactor, pThreshold);
		}

		public Indicators.QQESignals QQESignals(ISeries<double> input , int pRSIperiod, int pRSISmoothingPeriod, double pQQE_FastFactor, int pThreshold)
		{
			return indicator.QQESignals(input, pRSIperiod, pRSISmoothingPeriod, pQQE_FastFactor, pThreshold);
		}
	}
}

#endregion
