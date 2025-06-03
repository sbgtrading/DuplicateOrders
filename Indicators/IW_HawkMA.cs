#region Using declarations
using System;
using System.ComponentModel;
//using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Windows.Media;
using NinjaTrader.Gui;

#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public class IW_HawkMA: Indicator
	{
		private int period1	= 11;
		private Brush upColor = Brushes.Blue;
		private Brush downColor = Brushes.Magenta;
		private EMA  ema1;
		private EMA  ema2;
		private EMA  ema3;
		private bool init=false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				this.Name="IW_HawkMA";
				AddPlot(Brushes.Blue, "IW_HawkMA");
				Plots[0].DashStyleHelper = DashStyleHelper.Dash;

				Calculate=Calculate.OnPriceChange;
				IsOverlay=true;
				//PriceTypeSupported = true;
			}
		}

		protected override void OnBarUpdate()
		{
			if(!init)
			{
				ema1=EMA(Input,Math.Max(4,period1-1));
				ema2=EMA(ema1,period1);
				ema3=EMA(ema2,period1);
				init=true;
			}

			if (CurrentBar <= period1) return;

			TheRMA[0]=(ema3[0]);

			if(TheRMA[0]>TheRMA[1]) PlotBrushes[0][0] = upColor;
			else PlotBrushes[0][0] = downColor;

        }

		#region Properties

		[Description("Period of IW_HawkMA")]
		[Category("Parameters")]
		[NinjaScriptProperty]
		public int Period
		{
			get { return period1; }
			set { period1 = Math.Max(1, value); }
		}

//		[XmlIgnore()]
//		[Description("Color of Outline of Painted Bars")]
//		[Category("Plot Colors")]
//		[Gui.Design.DisplayNameAttribute("Bar Outline Color")]
//		public Brush BarBrushOutline
//		{
//			get { return barColorOutline; }
//			set { barColorOutline = value; }
//		}
//
//		/// <summary>
//		/// </summary>
//		[Browsable(false)]
//		public string barColorOutlineSerialize
//		{
//			get { return Serialize.BrushToString(barColorOutline); }
//			set { barColorOutline = Serialize.StringToBrush(value); }
//		}
		
		
		
		[XmlIgnore()]
		[Description("Color for Falling trend")]
		[Category("Line Colors")]
		public Brush PlotColorFalling
		{
			get { return downColor; }
			set { downColor = value; }
		}
		
		// Serialize our Color object
		[Browsable(false)]
		public string downColorSerialize
		{
			get { return Serialize.BrushToString(downColor); }
			set { downColor = Serialize.StringToBrush(value); }
		}

		[XmlIgnore()]
		[Description("Color for Rising trend")]
		[Category("Line Colors")]
		public Brush PlotColorRising
		{
			get { return upColor; }
			set { upColor = value; }
		}
		
		[Browsable(false)]
		public string upColorSerialize
		{
			get { return Serialize.BrushToString(upColor); }
			set { upColor = Serialize.StringToBrush(value); }
		}
		
		
		
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> TheRMA
		{
			get { return Values[0]; }
		}
		
		
//		[Description("ColorBars")]
//		[Category("Settings")]
//		public bool ColorBars
//		{
//			get { return colorbars; }
//			set { colorbars = value; }
//		}
		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_HawkMA[] cacheIW_HawkMA;
		public IW_HawkMA IW_HawkMA(int period)
		{
			return IW_HawkMA(Input, period);
		}

		public IW_HawkMA IW_HawkMA(ISeries<double> input, int period)
		{
			if (cacheIW_HawkMA != null)
				for (int idx = 0; idx < cacheIW_HawkMA.Length; idx++)
					if (cacheIW_HawkMA[idx] != null && cacheIW_HawkMA[idx].Period == period && cacheIW_HawkMA[idx].EqualsInput(input))
						return cacheIW_HawkMA[idx];
			return CacheIndicator<IW_HawkMA>(new IW_HawkMA(){ Period = period }, input, ref cacheIW_HawkMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_HawkMA IW_HawkMA(int period)
		{
			return indicator.IW_HawkMA(Input, period);
		}

		public Indicators.IW_HawkMA IW_HawkMA(ISeries<double> input , int period)
		{
			return indicator.IW_HawkMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_HawkMA IW_HawkMA(int period)
		{
			return indicator.IW_HawkMA(Input, period);
		}

		public Indicators.IW_HawkMA IW_HawkMA(ISeries<double> input , int period)
		{
			return indicator.IW_HawkMA(input, period);
		}
	}
}

#endregion
