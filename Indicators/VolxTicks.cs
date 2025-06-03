//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The VolxTicks (Volume Moving Average) plots an exponential moving average (EMA) of volume.
	/// </summary>
	public class VolxTicks : Indicator
	{
		private EMA ema;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name						= "VolxTicks";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= false;
				DrawOnPricePanel			= false;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, "VolxTicks");
			}
			else if (State == State.Historical)
			{
				if (Calculate == Calculate.OnPriceChange)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnPriceChangeError, Name), TextPosition.BottomRight);
					Log(string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnPriceChangeError, Name), LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			Value[0] = Volume[0] * Range()[0]*TickSize;
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
		private VolxTicks[] cacheVolxTicks;
		public VolxTicks VolxTicks(int period)
		{
			return VolxTicks(Input, period);
		}

		public VolxTicks VolxTicks(ISeries<double> input, int period)
		{
			if (cacheVolxTicks != null)
				for (int idx = 0; idx < cacheVolxTicks.Length; idx++)
					if (cacheVolxTicks[idx] != null && cacheVolxTicks[idx].Period == period && cacheVolxTicks[idx].EqualsInput(input))
						return cacheVolxTicks[idx];
			return CacheIndicator<VolxTicks>(new VolxTicks(){ Period = period }, input, ref cacheVolxTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolxTicks VolxTicks(int period)
		{
			return indicator.VolxTicks(Input, period);
		}

		public Indicators.VolxTicks VolxTicks(ISeries<double> input , int period)
		{
			return indicator.VolxTicks(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolxTicks VolxTicks(int period)
		{
			return indicator.VolxTicks(Input, period);
		}

		public Indicators.VolxTicks VolxTicks(ISeries<double> input , int period)
		{
			return indicator.VolxTicks(input, period);
		}
	}
}

#endregion
