//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Exponential Moving Average. The Exponential Moving Average is an indicator that
	/// shows the average value of a security's price over a period of time. When calculating
	/// a moving average. The EMA applies more weight to recent prices than the SMA.
	/// </summary>
	public class WellesMA : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionEMA;
				Name						= "WellesMA";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameEMA);
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			Value[0] = (CurrentBar < 3 ? Closes[0][0] : Value[1] + (Inputs[0][1] - Value[1]) / Period);
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WellesMA[] cacheWellesMA;
		public WellesMA WellesMA(int period)
		{
			return WellesMA(Input, period);
		}

		public WellesMA WellesMA(ISeries<double> input, int period)
		{
			if (cacheWellesMA != null)
				for (int idx = 0; idx < cacheWellesMA.Length; idx++)
					if (cacheWellesMA[idx] != null && cacheWellesMA[idx].Period == period && cacheWellesMA[idx].EqualsInput(input))
						return cacheWellesMA[idx];
			return CacheIndicator<WellesMA>(new WellesMA(){ Period = period }, input, ref cacheWellesMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WellesMA WellesMA(int period)
		{
			return indicator.WellesMA(Input, period);
		}

		public Indicators.WellesMA WellesMA(ISeries<double> input , int period)
		{
			return indicator.WellesMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WellesMA WellesMA(int period)
		{
			return indicator.WellesMA(Input, period);
		}

		public Indicators.WellesMA WellesMA(ISeries<double> input , int period)
		{
			return indicator.WellesMA(input, period);
		}
	}
}

#endregion
