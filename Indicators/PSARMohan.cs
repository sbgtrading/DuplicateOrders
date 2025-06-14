//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
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
	/// Parabolic SAR according to Stocks and Commodities magazine V 11:11 (477-479).
	/// </summary>
	public class PSARMohan : Indicator
	{
		private double			af;				// Acceleration factor
		private bool			afIncreased;
		private bool			longPosition;
		private int				prevBar;
		private double			prevSAR;
		private int				reverseBar;
		private double			reverseValue;
		private double			todaySAR;		// SAR value
		private double			xp;				// Extreme Price

		private Series<double>	afSeries;
		private Series<bool>	afIncreasedSeries;
		private Series<bool>	longPositionSeries;
		private Series<int>		prevBarSeries;
		private Series<double>	prevSARSeries;
		private Series<int>		reverseBarSeries;
		private Series<double>	reverseValueSeries;
		private Series<double>	todaySARSeries;
		private Series<double>	xpSeries;

		private ISeries<double>	high;
		private ISeries<double>	low;
		private Brush UpRegionBrush = null;
		private Brush DownRegionBrush = null;
		private DateTime ReverseTime = DateTime.MinValue;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionParabolicSAR;
				Name						= "PSAR Mohan";
				Acceleration				= 0.02;
				AccelerationStep			= 0.02;
				AccelerationMax				= 0.2;
				Calculate 					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				pUpTrendBrush = Brushes.Green;
				pDownTrendBrush = Brushes.Red;
				pBkgOpacity = 10;

				AddPlot(new Stroke(Brushes.Goldenrod, 2), PlotStyle.Line, Custom.Resource.NinjaScriptIndicatorNameParabolicSAR);
			}

			else if (State == State.Configure)
			{
				xp				= 0.0;
				af				= 0;
				todaySAR		= 0;
				prevSAR			= 0;
				reverseBar		= 0;
				reverseValue	= 0;
				prevBar			= 0;
				afIncreased		= false;
				UpRegionBrush = pUpTrendBrush.Clone();
				UpRegionBrush.Opacity = pBkgOpacity/100f;
				UpRegionBrush.Freeze();
				DownRegionBrush = pDownTrendBrush.Clone();
				DownRegionBrush.Opacity = pBkgOpacity/100f;
				DownRegionBrush.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
				{
					afSeries			= new Series<double>(this);
					afIncreasedSeries	= new Series<bool>(this);
					longPositionSeries	= new Series<bool>(this);
					prevBarSeries		= new Series<int>(this);
					prevSARSeries		= new Series<double>(this);
					reverseBarSeries	= new Series<int>(this);
					reverseValueSeries	= new Series<double>(this);
					todaySARSeries		= new Series<double>(this);
					xpSeries			= new Series<double>(this);
				}

				high	= Input is NinjaScriptBase ? Input : High;
				low		= Input is NinjaScriptBase ? Input : Low;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 3)
				return;

			if (CurrentBar == 3)
			{
				// Determine initial position
				longPosition	= high[0] > high[1];
				xp				= longPosition ? MAX(high, CurrentBar)[0] : MIN(low, CurrentBar)[0];
				af				= Acceleration;
				Value[0]		= xp + (longPosition ? -1 : 1) * ((MAX(high, CurrentBar)[0] - MIN(low, CurrentBar)[0]) * af);
				return;
			}
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported && CurrentBar < prevBar)
			{
				af				= afSeries[0];
				afIncreased		= afIncreasedSeries[0];
				longPosition	= longPositionSeries[0];
				prevBar			= prevBarSeries[0];
				prevSAR			= prevSARSeries[0];
				reverseBar		= reverseBarSeries[0];
				reverseValue	= reverseValueSeries[0];
				todaySAR		= todaySARSeries[0];
				xp				= xpSeries[0];
			}

			// Reset accelerator increase limiter on new bars
			if (afIncreased && prevBar != CurrentBar)
				afIncreased = false;

			// Current event is on a bar not marked as a reversal bar yet
			if (reverseBar != CurrentBar)
			{
				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(Value[1] + af * (xp - Value[1]));
				for (int x = 1; x <= 2; x++)
				{
					if (longPosition)
					{
						if (todaySAR > low[x])
							todaySAR = low[x];
					}
					else
					{
						if (todaySAR < high[x])
							todaySAR = high[x];
					}
				}

				// Holding long position
				if (longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || low[0] < prevSAR)
					{
						Value[0] = todaySAR;
						prevSAR = todaySAR;
					}
					else
						Value[0] = prevSAR;
					PlotBrushes[0][0] = pUpTrendBrush;

					if (high[0] > xp)
					{
						xp = high[0];
						AfIncrease();
					}
					if(CurrentBar>reverseBar){
						Draw.Region(this, reverseBar.ToString(), Times[0].GetValueAt(reverseBar), Times[0][0], Value, Lows[0], Brushes.Transparent, pUpTrendBrush, pBkgOpacity);
					}
				}

				// Holding short position
				else if (!longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || high[0] > prevSAR)
					{
						Value[0] = todaySAR;
						prevSAR = todaySAR;
					}
					else
						Value[0] = prevSAR;
					PlotBrushes[0][0] = pDownTrendBrush;

					if (low[0] < xp)
					{
						xp = low[0];
						AfIncrease();
					}
					if(CurrentBar>reverseBar){
						Draw.Region(this, reverseBar.ToString(), Times[0].GetValueAt(reverseBar), Times[0][0], Value, Highs[0], Brushes.Transparent, pDownTrendBrush, pBkgOpacity);
					}
				}
			}

			// Current event is on the same bar as the reversal bar
			else
			{
				// Only set new xp values. No increasing af since this is the first bar.
				if (longPosition && high[0] > xp)
					xp = high[0];
				else if (!longPosition && low[0] < xp)
					xp = low[0];

				Value[0] = prevSAR;

				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(longPosition ? Math.Min(reverseValue, low[0]) : Math.Max(reverseValue, high[0]));
			}

			prevBar = CurrentBar;

			// Reverse position
			if ((longPosition && (low[0] < todaySAR || low[1] < todaySAR))
				|| (!longPosition && (high[0] > todaySAR || high[1] > todaySAR)))
				Value[0] = Reverse();

			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				afSeries[0]				= af;
				afIncreasedSeries[0]	= afIncreased;
				longPositionSeries[0]	= longPosition;
				prevBarSeries[0]		= prevBar;
				prevSARSeries[0]		= prevSAR;
				reverseBarSeries[0]		= reverseBar;
				reverseValueSeries[0]	= reverseValue;
				todaySARSeries[0]		= todaySAR;
				xpSeries[0]				= xp;
			}
		}

		#region Miscellaneous
		// Only raise accelerator if not raised for current bar yet
		private void AfIncrease()
		{
			if (!afIncreased)
			{
				af			= Math.Min(AccelerationMax, af + AccelerationStep);
				afIncreased	= true;
			}
		}

		// Additional rule. SAR for today can't be placed inside the bar of day - 1 or day - 2.
		private double TodaySAR(double tSAR)
		{
			if (longPosition)
			{
				double lowestSAR = Math.Min(Math.Min(tSAR, low[0]), low[1]);
				if (low[0] > lowestSAR)
					tSAR = lowestSAR;
			}
			else
			{
				double highestSAR = Math.Max(Math.Max(tSAR, high[0]), high[1]);
				if (high[0] < highestSAR)
					tSAR = highestSAR;
			}
			return tSAR;
		}

		private double Reverse()
		{
			double tSAR = xp;

			if ((longPosition && prevSAR > low[0]) || (!longPosition && prevSAR < high[0]) || prevBar != CurrentBar)
			{
				longPosition	= !longPosition;
				reverseBar		= CurrentBar;
				reverseValue	= xp;
				af				= Acceleration;
				xp				= longPosition ? high[0] : low[0];
				prevSAR			= tSAR;
			}
			else
				tSAR = prevSAR;
			return tSAR;
		}
		#endregion

		#region Properties
		[Range(0.00, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Acceleration", GroupName = "NinjaScriptParameters", Order = 0)]
		public double Acceleration
		{ get; set; }

		[Range(0.001, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccelerationMax", GroupName = "NinjaScriptParameters", Order = 1)]
		public double AccelerationMax
		{ get; set; }

		[Range(0.001, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccelerationStep", GroupName = "NinjaScriptParameters", Order = 2)]
		public double AccelerationStep
		{ get; set; }
		
		[Range(0, 100)]
		[Display(Order = 10, Name = "Bkg opacity", GroupName = "Colors", Description="", ResourceType = typeof(Custom.Resource))]
		public int pBkgOpacity
		{ get; set; }

		[XmlIgnore]
		[Display(Order = 20, Name = "Uptrend line", GroupName = "Colors")]
		public Brush pUpTrendBrush
		{ get; set; }
				[Browsable(false)]
				public string pUpTrendBrushSerializable { get { return Serialize.BrushToString(pUpTrendBrush); } set { pUpTrendBrush = Serialize.StringToBrush(value); }        }
		[XmlIgnore]
		[Display(Order = 30, Name = "Downtrend line", GroupName = "Colors")]
		public Brush pDownTrendBrush
		{ get; set; }
				[Browsable(false)]
				public string pDownTrendBrushSerializable { get { return Serialize.BrushToString(pDownTrendBrush); } set { pDownTrendBrush = Serialize.StringToBrush(value); }        }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PSARMohan[] cachePSARMohan;
		public PSARMohan PSARMohan(double acceleration, double accelerationMax, double accelerationStep)
		{
			return PSARMohan(Input, acceleration, accelerationMax, accelerationStep);
		}

		public PSARMohan PSARMohan(ISeries<double> input, double acceleration, double accelerationMax, double accelerationStep)
		{
			if (cachePSARMohan != null)
				for (int idx = 0; idx < cachePSARMohan.Length; idx++)
					if (cachePSARMohan[idx] != null && cachePSARMohan[idx].Acceleration == acceleration && cachePSARMohan[idx].AccelerationMax == accelerationMax && cachePSARMohan[idx].AccelerationStep == accelerationStep && cachePSARMohan[idx].EqualsInput(input))
						return cachePSARMohan[idx];
			return CacheIndicator<PSARMohan>(new PSARMohan(){ Acceleration = acceleration, AccelerationMax = accelerationMax, AccelerationStep = accelerationStep }, input, ref cachePSARMohan);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PSARMohan PSARMohan(double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSARMohan(Input, acceleration, accelerationMax, accelerationStep);
		}

		public Indicators.PSARMohan PSARMohan(ISeries<double> input , double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSARMohan(input, acceleration, accelerationMax, accelerationStep);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PSARMohan PSARMohan(double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSARMohan(Input, acceleration, accelerationMax, accelerationStep);
		}

		public Indicators.PSARMohan PSARMohan(ISeries<double> input , double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSARMohan(input, acceleration, accelerationMax, accelerationStep);
		}
	}
}

#endregion
