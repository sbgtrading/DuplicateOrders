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
#endregion
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;

namespace NinjaTrader.NinjaScript.Indicators
{
    [Description("The SMMA (Smoothed Moving Average) is an indicator that shows the average value of a security's price over a period of time.")]
    public class IW_SMMA : Indicator
    {
        #region Variables
		private int		period	= 14;
		private double	smma1	= 0;
		private double	sum1	= 0;
		private double	prevsum1 = 0;
		private double	prevsmma1 = 0;
        #endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AddPlot(Brushes.Orange, "SMMA");
				Calculate=Calculate.OnBarClose;
				IsOverlay=true;
			//PriceTypeSupported	= true;
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar == Period)
			{
				sum1 = SUM(Input,Period)[0];
				smma1 = sum1/Period;
				Value[0]=(smma1);
			}
			else if (CurrentBar > Period)
			{
				if (IsFirstTickOfBar)
				{
					prevsum1 = sum1;
					prevsmma1 = smma1;
				}
				Value[0] = (prevsum1-prevsmma1+Input[0])/Period;
				sum1 = prevsum1-prevsmma1+Input[0];
				smma1 = (sum1-prevsmma1+Input[0])/Period;
				//Print(string.Concat("SMMA: ",Value[0].ToString(),"  ",CurrentBar,"  ",(State == State.Historical).ToString()));
			}
        }

        #region Properties
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
		private IW_SMMA[] cacheIW_SMMA;
		public IW_SMMA IW_SMMA(int period)
		{
			return IW_SMMA(Input, period);
		}

		public IW_SMMA IW_SMMA(ISeries<double> input, int period)
		{
			if (cacheIW_SMMA != null)
				for (int idx = 0; idx < cacheIW_SMMA.Length; idx++)
					if (cacheIW_SMMA[idx] != null && cacheIW_SMMA[idx].Period == period && cacheIW_SMMA[idx].EqualsInput(input))
						return cacheIW_SMMA[idx];
			return CacheIndicator<IW_SMMA>(new IW_SMMA(){ Period = period }, input, ref cacheIW_SMMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_SMMA IW_SMMA(int period)
		{
			return indicator.IW_SMMA(Input, period);
		}

		public Indicators.IW_SMMA IW_SMMA(ISeries<double> input , int period)
		{
			return indicator.IW_SMMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_SMMA IW_SMMA(int period)
		{
			return indicator.IW_SMMA(Input, period);
		}

		public Indicators.IW_SMMA IW_SMMA(ISeries<double> input , int period)
		{
			return indicator.IW_SMMA(input, period);
		}
	}
}

#endregion
