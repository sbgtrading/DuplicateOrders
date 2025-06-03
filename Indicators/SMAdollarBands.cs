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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The SMAdollarBands (Simple Moving Average) is an indicator that shows the average value of a security's price over a period of time.
	/// </summary>
	public class SMAdollarBands : Indicator
	{
		private double priorSum;
		private double sum;
		private double pts = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionSMA;
				Name						= "SMA $Bands";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				pPeriod						= 60;
				pDollarsBandSize = 100;
				pRoundToTicks = true;

				AddPlot(Brushes.Lime, "Upper");
				AddPlot(Brushes.DimGray, "Mid");
				AddPlot(Brushes.Magenta, "Lower");
				AddPlot(new Stroke(Brushes.Lime,3), PlotStyle.Dot, "MidHit1");
				AddPlot(new Stroke(Brushes.DimGray,3), PlotStyle.Dot, "MidHit2");
				AddPlot(new Stroke(Brushes.Magenta,3), PlotStyle.Dot, "MidHit3");
			}
			else if (State == State.Configure)
			{
				Plots[1].DashStyleHelper = DashStyleHelper.Dash;
				priorSum	= 0;
				sum			= 0;
				pts = Instrument.MasterInstrument.RoundToTickSize(pDollarsBandSize /2 / Instrument.MasterInstrument.PointValue);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				if (CurrentBar == 0)
					Values[1][0] = Input[0];
				else
				{
					double last = Values[1][1] * Math.Min(CurrentBar, pPeriod);

					if (CurrentBar >= pPeriod)
						Values[1][0] = (last + Input[0] - Input[pPeriod]) / Math.Min(CurrentBar, pPeriod);
					else
						Values[1][0] = ((last + Input[0]) / (Math.Min(CurrentBar, pPeriod) + 1));
				}
			}
			else
			{
				if (IsFirstTickOfBar)
					priorSum = sum;

				sum = priorSum + Input[0] - (CurrentBar >= pPeriod ? Input[pPeriod] : 0);
				var v = sum / (CurrentBar < pPeriod ? CurrentBar + 1 : pPeriod);
				Values[1][0] = pRoundToTicks ? Instrument.MasterInstrument.RoundToTickSize(v) : v;
			}
			Values[0][0] = Values[1][0] + pts;
			Values[2][0] = Values[1][0] - pts;
			if(CurrentBars[0]>3){
				if(Highs[0][0] > Values[1][1] && Lows[0][0] <= Values[1][1]) Values[4][1] = Values[1][1];
				else if(Highs[0][1] > Values[1][1] && Lows[0][0] <= Values[1][1]) Values[4][0] = Values[1][0];
				else if(Lows[0][1] < Values[1][1] && Highs[0][0] >= Values[1][1]) Values[4][0] = Values[1][0];
				if(Values[4].IsValidDataPoint(1)) {Values[3][1] = Values[0][1]; Values[5][1] = Values[2][1];}
				else if(Values[4].IsValidDataPoint(0)) {Values[3][0] = Values[0][0]; Values[5][0] = Values[2][0];}
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int pPeriod
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dollars band size", GroupName = "NinjaScriptParameters", Order = 10)]
		public int pDollarsBandSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Round 2 ticks", GroupName = "NinjaScriptParameters", Order = 20, ResourceType = typeof(Custom.Resource))]
		public bool pRoundToTicks
		{get;set;}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMAdollarBands[] cacheSMAdollarBands;
		public SMAdollarBands SMAdollarBands(int pPeriod, int pDollarsBandSize, bool pRoundToTicks)
		{
			return SMAdollarBands(Input, pPeriod, pDollarsBandSize, pRoundToTicks);
		}

		public SMAdollarBands SMAdollarBands(ISeries<double> input, int pPeriod, int pDollarsBandSize, bool pRoundToTicks)
		{
			if (cacheSMAdollarBands != null)
				for (int idx = 0; idx < cacheSMAdollarBands.Length; idx++)
					if (cacheSMAdollarBands[idx] != null && cacheSMAdollarBands[idx].pPeriod == pPeriod && cacheSMAdollarBands[idx].pDollarsBandSize == pDollarsBandSize && cacheSMAdollarBands[idx].pRoundToTicks == pRoundToTicks && cacheSMAdollarBands[idx].EqualsInput(input))
						return cacheSMAdollarBands[idx];
			return CacheIndicator<SMAdollarBands>(new SMAdollarBands(){ pPeriod = pPeriod, pDollarsBandSize = pDollarsBandSize, pRoundToTicks = pRoundToTicks }, input, ref cacheSMAdollarBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMAdollarBands SMAdollarBands(int pPeriod, int pDollarsBandSize, bool pRoundToTicks)
		{
			return indicator.SMAdollarBands(Input, pPeriod, pDollarsBandSize, pRoundToTicks);
		}

		public Indicators.SMAdollarBands SMAdollarBands(ISeries<double> input , int pPeriod, int pDollarsBandSize, bool pRoundToTicks)
		{
			return indicator.SMAdollarBands(input, pPeriod, pDollarsBandSize, pRoundToTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMAdollarBands SMAdollarBands(int pPeriod, int pDollarsBandSize, bool pRoundToTicks)
		{
			return indicator.SMAdollarBands(Input, pPeriod, pDollarsBandSize, pRoundToTicks);
		}

		public Indicators.SMAdollarBands SMAdollarBands(ISeries<double> input , int pPeriod, int pDollarsBandSize, bool pRoundToTicks)
		{
			return indicator.SMAdollarBands(input, pPeriod, pDollarsBandSize, pRoundToTicks);
		}
	}
}

#endregion
