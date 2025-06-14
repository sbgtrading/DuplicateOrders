//
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Sum shows the summation of the last n data points.
	/// </summary>
	public class SUM : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionSUM;
				Name						= Custom.Resource.NinjaScriptIndicatorNameSUM;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.DarkCyan, Custom.Resource.NinjaScriptIndicatorNameSUM);
			}
		}

		protected override void OnBarUpdate() => Value[0] = Input[0] + (CurrentBar > 0 ? Value[1] : 0) - (CurrentBar >= Period ? Input[Period] : 0);

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period { get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SUM[] cacheSUM;
		public SUM SUM(int period)
		{
			return SUM(Input, period);
		}

		public SUM SUM(ISeries<double> input, int period)
		{
			if (cacheSUM != null)
				for (int idx = 0; idx < cacheSUM.Length; idx++)
					if (cacheSUM[idx] != null && cacheSUM[idx].Period == period && cacheSUM[idx].EqualsInput(input))
						return cacheSUM[idx];
			return CacheIndicator<SUM>(new SUM(){ Period = period }, input, ref cacheSUM);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SUM SUM(int period)
		{
			return indicator.SUM(Input, period);
		}

		public Indicators.SUM SUM(ISeries<double> input , int period)
		{
			return indicator.SUM(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SUM SUM(int period)
		{
			return indicator.SUM(Input, period);
		}

		public Indicators.SUM SUM(ISeries<double> input , int period)
		{
			return indicator.SUM(input, period);
		}
	}
}

#endregion
