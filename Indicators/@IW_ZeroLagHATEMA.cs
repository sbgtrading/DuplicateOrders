// 
// Copyright (C) 2008, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//

#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Windows.Media;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Zero-Lagging Heiken-Ashi TEMA
	/// </summary>
	[Description("Zero-Lagging Heiken-Ashi TEMA")]
	public class IW_ZeroLagHATEMA : Indicator
	{
        #region Variables
		private int period = 14; // Default setting for Period
		private Series<double> haC;
		private Series<double> haO;
        #endregion

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AddPlot(Brushes.Orange, "Zero-Lagging Heiken-Ashi TEMA");
			
			
				Calculate=Calculate.OnBarClose;
				IsOverlay=true;
				//PriceTypeSupported	= false;
			}

			if (State == State.DataLoaded)
			{
				haC = new Series<double>(this);
				haO = new Series<double>(this);
			}
		}

		/// <summary>
		/// Called on each bar update event (incoming tick)
		/// </summary>
		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				haC[0]=(0);
				haO[0]=(0);
				return;
			}

			if (CurrentBar < 1)
				return;

			haO[0]=((((Open[1] + High[1] + Low[1] + Close[1]) / 4) + haO[1]) / 2);
			haC[0]=((((Open[0] + High[0] + Low[0] + Close[0]) / 4) + haO[0] + Math.Max(High[0], haO[0]) + Math.Min(Low[0], haO[0])) / 4);
			
			double TEMA1 = TEMA(haC, Period)[0];
			double TEMA2 = TEMA(TEMA(haC, Period), Period)[0];
			ZeroHATEMA[0]=(TEMA1 + (TEMA1 - TEMA2));
		}

        #region Properties
	[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
	[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
	public Series<double> ZeroHATEMA
	{
		get { return Values[0]; }
	}

	[Description("Numbers of bars used for calculations")]
	[Category("Parameters")]
	[NinjaScriptProperty]
	public int Period
	{
		get { return period; }
		set { period = Math.Max(1, value); }
	}
        #endregion
    }
}


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_ZeroLagHATEMA[] cacheIW_ZeroLagHATEMA;
		public IW_ZeroLagHATEMA IW_ZeroLagHATEMA(int period)
		{
			return IW_ZeroLagHATEMA(Input, period);
		}

		public IW_ZeroLagHATEMA IW_ZeroLagHATEMA(ISeries<double> input, int period)
		{
			if (cacheIW_ZeroLagHATEMA != null)
				for (int idx = 0; idx < cacheIW_ZeroLagHATEMA.Length; idx++)
					if (cacheIW_ZeroLagHATEMA[idx] != null && cacheIW_ZeroLagHATEMA[idx].Period == period && cacheIW_ZeroLagHATEMA[idx].EqualsInput(input))
						return cacheIW_ZeroLagHATEMA[idx];
			return CacheIndicator<IW_ZeroLagHATEMA>(new IW_ZeroLagHATEMA(){ Period = period }, input, ref cacheIW_ZeroLagHATEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_ZeroLagHATEMA IW_ZeroLagHATEMA(int period)
		{
			return indicator.IW_ZeroLagHATEMA(Input, period);
		}

		public Indicators.IW_ZeroLagHATEMA IW_ZeroLagHATEMA(ISeries<double> input , int period)
		{
			return indicator.IW_ZeroLagHATEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_ZeroLagHATEMA IW_ZeroLagHATEMA(int period)
		{
			return indicator.IW_ZeroLagHATEMA(Input, period);
		}

		public Indicators.IW_ZeroLagHATEMA IW_ZeroLagHATEMA(ISeries<double> input , int period)
		{
			return indicator.IW_ZeroLagHATEMA(input, period);
		}
	}
}

#endregion
