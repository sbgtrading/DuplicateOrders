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

//This namespace holds Indicators in this folder and is required. Do not change it.    russlandholdingcorp@gmail.com
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Simov : Indicator
	{
		// Define the parameters
		private int shortLength = 5;
		private int longLength = 20;
		private int signalLength = 5;
		private int volumeLength = 20;
		
		// Declare variables
		private Series<double> ergonic;
		private Series<double> signal;
		private double oscillator;
		private double averageVolume;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Simov";
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
			    AddPlot(Brushes.Aqua, "Oscillator");
			    AddPlot(new Stroke(Brushes.Transparent,1f), PlotStyle.Dot, "Buy");
			    AddPlot(new Stroke(Brushes.Transparent,1f), PlotStyle.Dot, "Sell");
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.DataLoaded)
			{
				tsi = TSI(Close, shortLength, longLength);
				ema = EMA(tsi, signalLength);
				ergonic = new Series<double>(this);
				signal = new Series<double>(this);
			}
		}
		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Oscillator
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Buy
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Sell
		{
			get { return Values[2]; }
		}
		#endregion

		private TSI tsi;
		private EMA ema;
		protected override void OnBarUpdate()
		{
		    if (CurrentBar < longLength) return;

		    ergonic[0] = tsi[0];
		    signal[0] = ema[0];
		    Values[0][0] = ergonic[0] - signal[0];
		    averageVolume = SUM(Volume, volumeLength)[0] / volumeLength;

		    // Buy and Sell conditions
		    bool buy = (ergonic[0] > 0 && ergonic[1] < 0 && Values[0][0] > Values[0][1] && ergonic[0] > signal[0] && Volume[0] > averageVolume && Close[0] > Open[0]);
		    bool sell = (ergonic[0] < 0 && ergonic[1] > 0 && Values[0][0] < Values[0][1] && ergonic[0] < signal[0] && Volume[0] > averageVolume && Close[0] < Open[0]);

		    // Plot buy and sell signals
		    if (buy){
				Buy[0] = Closes[0][0];
				if(ChartControl != null){
			    	var arrow = Draw.ArrowUp(this,"SimovBuy"+CurrentBar.ToString(), false, 0, Low[0] - 2 * TickSize, Brushes.LimeGreen);
					arrow.Size = ChartMarkerSize.Large;
				}
		    }
			if (sell){
				Sell[0] = Closes[0][0];
				if(ChartControl != null){
			        var arrow = Draw.ArrowDown(this,"SimovSell"+CurrentBar.ToString(), false, 0, High[0] + 2 * TickSize, Brushes.Magenta);
					arrow.Size = ChartMarkerSize.Large;
				}
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Simov[] cacheSimov;
		public Simov Simov()
		{
			return Simov(Input);
		}

		public Simov Simov(ISeries<double> input)
		{
			if (cacheSimov != null)
				for (int idx = 0; idx < cacheSimov.Length; idx++)
					if (cacheSimov[idx] != null &&  cacheSimov[idx].EqualsInput(input))
						return cacheSimov[idx];
			return CacheIndicator<Simov>(new Simov(), input, ref cacheSimov);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Simov Simov()
		{
			return indicator.Simov(Input);
		}

		public Indicators.Simov Simov(ISeries<double> input )
		{
			return indicator.Simov(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Simov Simov()
		{
			return indicator.Simov(Input);
		}

		public Indicators.Simov Simov(ISeries<double> input )
		{
			return indicator.Simov(input);
		}
	}
}

#endregion
