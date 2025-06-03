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
	public class TBEma : Indicator
	{
		private double _smoothFactor;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionEMA;
				Name						= "TBEma";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameEMA);
			}
			else if (State == State.Configure)
			{
				_smoothFactor = 2.0 / (Period + 1);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar<2)
				Value[0] = Input[0];
			else
				Value[0] = _smoothFactor * Input[0] + (1 - _smoothFactor) * Value[1];
			if(CurrentBar>Bars.Count-5){
				var s = string.Format("{0} {1}\n{2} {3}\n{4} {5}\n{6} {7}\n{8} {9}",
					Times[0][0].ToShortTimeString(), Range()[0],
					Times[0][1].ToShortTimeString(), Range()[1],
					Times[0][2].ToShortTimeString(), Range()[2],
					Times[0][3].ToShortTimeString(), Range()[3],
					Times[0][4].ToShortTimeString(), Range()[4]);
				Draw.TextFixed(this,"info",s, TextPosition.Center);
			}
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
		private TBEma[] cacheTBEma;
		public TBEma TBEma(int period)
		{
			return TBEma(Input, period);
		}

		public TBEma TBEma(ISeries<double> input, int period)
		{
			if (cacheTBEma != null)
				for (int idx = 0; idx < cacheTBEma.Length; idx++)
					if (cacheTBEma[idx] != null && cacheTBEma[idx].Period == period && cacheTBEma[idx].EqualsInput(input))
						return cacheTBEma[idx];
			return CacheIndicator<TBEma>(new TBEma(){ Period = period }, input, ref cacheTBEma);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TBEma TBEma(int period)
		{
			return indicator.TBEma(Input, period);
		}

		public Indicators.TBEma TBEma(ISeries<double> input , int period)
		{
			return indicator.TBEma(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TBEma TBEma(int period)
		{
			return indicator.TBEma(Input, period);
		}

		public Indicators.TBEma TBEma(ISeries<double> input , int period)
		{
			return indicator.TBEma(input, period);
		}
	}
}

#endregion
