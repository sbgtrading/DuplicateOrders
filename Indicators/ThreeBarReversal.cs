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
	/// Returns a value of 1 when the current close is less than the prior close after penetrating the highest high of the last n bars.
	/// </summary>
	public class ThreeBarReversal : Indicator
	{

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionKeyReversalDown;
				Name						= "ThreeBarReversal";
				Period						= 1;

				AddPlot(Brushes.DodgerBlue, NinjaTrader.Custom.Resource.KeyReversalPlot0);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 3)
				return;
			Value[0]=0;

//			if(Low[2]<Low[1] && Low[1] > Low[0] && High[0] > High[1] && Close[0] < Open[0]) Value[0] = -1;
//			if(High[2]>High[1] && High[1] < High[0] && Low[0] < Low[1] && Close[0] > Open[0]) Value[0] = 1;
			
			var c1 = Math.Abs(Open[0]-Close[0]) > 0.6 * (High[0]-Low[0]);
			if(c1){
				if(High[0] > High[1] && Low[0]<Low[1] && Close[0] < Open[0]) Value[0] = -1;
				if(Low[0] < Low[1] && High[0]>High[1] && Close[0] > Open[0]) Value[0] = 1;
//				if(Value[0]==1)
//					Print(Time[0].ToString()+"   O-C: "+(Math.Abs(Open[0]-Close[0])/TickSize).ToString()+"  H-L: "+((High[0]-Low[0])/TickSize).ToString());
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
		private ThreeBarReversal[] cacheThreeBarReversal;
		public ThreeBarReversal ThreeBarReversal(int period)
		{
			return ThreeBarReversal(Input, period);
		}

		public ThreeBarReversal ThreeBarReversal(ISeries<double> input, int period)
		{
			if (cacheThreeBarReversal != null)
				for (int idx = 0; idx < cacheThreeBarReversal.Length; idx++)
					if (cacheThreeBarReversal[idx] != null && cacheThreeBarReversal[idx].Period == period && cacheThreeBarReversal[idx].EqualsInput(input))
						return cacheThreeBarReversal[idx];
			return CacheIndicator<ThreeBarReversal>(new ThreeBarReversal(){ Period = period }, input, ref cacheThreeBarReversal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ThreeBarReversal ThreeBarReversal(int period)
		{
			return indicator.ThreeBarReversal(Input, period);
		}

		public Indicators.ThreeBarReversal ThreeBarReversal(ISeries<double> input , int period)
		{
			return indicator.ThreeBarReversal(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ThreeBarReversal ThreeBarReversal(int period)
		{
			return indicator.ThreeBarReversal(Input, period);
		}

		public Indicators.ThreeBarReversal ThreeBarReversal(ISeries<double> input , int period)
		{
			return indicator.ThreeBarReversal(input, period);
		}
	}
}

#endregion
