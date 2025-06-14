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
	public class PercentR : Indicator
	{
		private MAX max;
		private MIN min;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "PercentR";
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
				Length					= 20;
				AddPlot(Brushes.Red, "R");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				max = MAX(High,Length);
				min = MIN(Low,Length);
			}
		}

		protected override void OnBarUpdate()
		{
			double hh = max[0];
			double divisor = hh-min[0];
			if (divisor!=0)
				R[0] = 100-((hh-Close[0])/divisor)*100;
			else
				R[0] = 0;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Order=1, GroupName="Parameters")]
		public int Length
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> R
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
		private PercentR[] cachePercentR;
		public PercentR PercentR(int length)
		{
			return PercentR(Input, length);
		}

		public PercentR PercentR(ISeries<double> input, int length)
		{
			if (cachePercentR != null)
				for (int idx = 0; idx < cachePercentR.Length; idx++)
					if (cachePercentR[idx] != null && cachePercentR[idx].Length == length && cachePercentR[idx].EqualsInput(input))
						return cachePercentR[idx];
			return CacheIndicator<PercentR>(new PercentR(){ Length = length }, input, ref cachePercentR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PercentR PercentR(int length)
		{
			return indicator.PercentR(Input, length);
		}

		public Indicators.PercentR PercentR(ISeries<double> input , int length)
		{
			return indicator.PercentR(input, length);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PercentR PercentR(int length)
		{
			return indicator.PercentR(Input, length);
		}

		public Indicators.PercentR PercentR(ISeries<double> input , int length)
		{
			return indicator.PercentR(input, length);
		}
	}
}

#endregion
