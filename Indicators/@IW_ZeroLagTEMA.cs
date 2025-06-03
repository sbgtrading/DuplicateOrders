

#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Gui.Chart;
using System.Windows.Media;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Zero-Lagging TEMA
    /// </summary>
    [Description("Zero-Lagging TEMA")]
    public class IW_ZeroLagTEMA : Indicator
    {
        #region Variables
		private int pPeriod = 14; // Default setting for Period
		private int pSmooth = 10;
        #endregion

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "Zero-Lagging TEMA");
				AddPlot(new Stroke(Brushes.Yellow, 1), PlotStyle.Line, "Zero-Lagging TEMA smoothed");

				Calculate=Calculate.OnPriceChange;
				IsOverlay=true;
			}
			//PriceTypeSupported	= false;
		}

		/// <summary>
		/// Called on each bar update event (incoming tick)
		/// </summary>
		protected override void OnBarUpdate()
		{

			if (CurrentBar < 1)
				return;

			double TEMA3 = TEMA(Input, pPeriod)[0];
			double TEMA4 = TEMA(TEMA(Input, pPeriod), pPeriod)[0];
			ZLTEMA[0]=(TEMA3 + (TEMA3 - TEMA4));

			if(pSmooth>0)
				ZLTEMASmooth[0]=(HMA(Values[0], pSmooth)[0]);

		}

        #region Plots
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> ZLTEMA
        {
            get { return Values[0]; }
        }
		
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> ZLTEMASmooth
        {
            get { return Values[1]; }
        }
        #endregion
		

        #region Properties
		[Description("Numbers of bars used for IW_ZeroLagTEMA calculations")]
        [Category("Parameters")]
		[NinjaScriptProperty]
        public int ZLTEMAperiod
        {
            get { return pPeriod; }
            set { pPeriod = Math.Max(1, value); }
        }

		[Description("Numbers of bars used for IW_ZeroLagTEMA smoothing calculations using an HMA, enter a '0' to disengage the smoothing")]
        [Category("Parameters")]
		[NinjaScriptProperty]
        public int ZLTEMASmoothPeriod
        {
            get { return pSmooth; }
            set { pSmooth = Math.Max(-1, value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_ZeroLagTEMA[] cacheIW_ZeroLagTEMA;
		public IW_ZeroLagTEMA IW_ZeroLagTEMA(int zLTEMAperiod, int zLTEMASmoothPeriod)
		{
			return IW_ZeroLagTEMA(Input, zLTEMAperiod, zLTEMASmoothPeriod);
		}

		public IW_ZeroLagTEMA IW_ZeroLagTEMA(ISeries<double> input, int zLTEMAperiod, int zLTEMASmoothPeriod)
		{
			if (cacheIW_ZeroLagTEMA != null)
				for (int idx = 0; idx < cacheIW_ZeroLagTEMA.Length; idx++)
					if (cacheIW_ZeroLagTEMA[idx] != null && cacheIW_ZeroLagTEMA[idx].ZLTEMAperiod == zLTEMAperiod && cacheIW_ZeroLagTEMA[idx].ZLTEMASmoothPeriod == zLTEMASmoothPeriod && cacheIW_ZeroLagTEMA[idx].EqualsInput(input))
						return cacheIW_ZeroLagTEMA[idx];
			return CacheIndicator<IW_ZeroLagTEMA>(new IW_ZeroLagTEMA(){ ZLTEMAperiod = zLTEMAperiod, ZLTEMASmoothPeriod = zLTEMASmoothPeriod }, input, ref cacheIW_ZeroLagTEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_ZeroLagTEMA IW_ZeroLagTEMA(int zLTEMAperiod, int zLTEMASmoothPeriod)
		{
			return indicator.IW_ZeroLagTEMA(Input, zLTEMAperiod, zLTEMASmoothPeriod);
		}

		public Indicators.IW_ZeroLagTEMA IW_ZeroLagTEMA(ISeries<double> input , int zLTEMAperiod, int zLTEMASmoothPeriod)
		{
			return indicator.IW_ZeroLagTEMA(input, zLTEMAperiod, zLTEMASmoothPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_ZeroLagTEMA IW_ZeroLagTEMA(int zLTEMAperiod, int zLTEMASmoothPeriod)
		{
			return indicator.IW_ZeroLagTEMA(Input, zLTEMAperiod, zLTEMASmoothPeriod);
		}

		public Indicators.IW_ZeroLagTEMA IW_ZeroLagTEMA(ISeries<double> input , int zLTEMAperiod, int zLTEMASmoothPeriod)
		{
			return indicator.IW_ZeroLagTEMA(input, zLTEMAperiod, zLTEMASmoothPeriod);
		}
	}
}

#endregion
