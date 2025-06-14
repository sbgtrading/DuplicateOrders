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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Relative Vigor Index measures the strength of a trend by comparing an instruments closing price to its price range. It's based on the fact that prices tend to close higher than they open in up trends, and closer lower than they open in downtrends.
	/// </summary>
	public class RelativeVigorIndex2 : Indicator
	{
		private Series<double> series1;
		private Series<double> series2;
		Brush Up,Dn;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionRelativeVigorIndex;
				Name						= "RelativeVigorIndex 2";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DrawOnPricePanel			= false;
				IsSuspendedWhileInactive	= true;
				Period 						= 10;

				AddPlot(Brushes.Green, NinjaTrader.Custom.Resource.NinjaScriptIndicatorRelativeVigorIndex);
				AddPlot(Brushes.Red, NinjaTrader.Custom.Resource.NinjaScriptIndicatorSignal);
				
				Up = Brushes.Green.Clone();
				Up.Opacity = 0.1;
				Up.Freeze();
				Dn = Brushes.Red.Clone();
				Dn.Opacity = 0.1;
				Dn.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				ClearOutputWindow();
				series1 = new Series<double>(this);
				series2 = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 3)
				return;
			
			series1[0] = ((Close[0] - Open[0]) + 2 * (Close[1] - Open[1]) + 2 * (Close[2] - Open[2]) + (Close[3] - Open[3])) / 6.0;
			series2[0] = ((High[0] - Low[0]) + 2 * (High[1] - Low[1]) + 2 * (High[2] - Low[2]) + (High[3] - Low[3])) / 6.0;
Print(Time[0].ToString()+"  "+Open[0]+" "+High[0]+" "+Low[0]+" "+Close[0]+"   series1: "+series1[0].ToString("0.00")+"    series2: "+series2[0].ToString("0.00"));
			double numerator 	= 0;
			double denominator 	= 0;

			for (int i = 0; i < Math.Min(CurrentBar, Period); i++)
			{
				numerator 	+= series1[i];
				denominator += series2[i];
			}
			if (denominator != 0)
			{
				Value[0] 	= numerator / denominator;
				Signal[0] 	= (Value[0] + 2 * Value[1] + 3 * Value[2] + Value[3]) / 7.0;
				int trend = Value[0] > Signal[0] ? 1 : -1;
				trend = Value[0] > Value[1] ? 1 : -1;
				//if(Value[0]>Signal[0]) BackBrushes[0] = Up; else BackBrushes[0] = Dn;
//				if(Value[0] > 0 && Value[1] <= 0) BackBrushes[0] = Up;
//				if(Value[0] < 0 && Value[1] >= 0) BackBrushes[0] = Dn;
				if(false){
					if(trend > 0 && Close[0] > Open[0] && Value[0]<0) BackBrushes[0] = Up;
					if(trend < 0 && Close[0] < Open[0] && Value[0]>0) BackBrushes[0] = Dn;
				}
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default { get { return Values[0]; } }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal { get { return Values[1]; } }
		
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
		private RelativeVigorIndex2[] cacheRelativeVigorIndex2;
		public RelativeVigorIndex2 RelativeVigorIndex2(int period)
		{
			return RelativeVigorIndex2(Input, period);
		}

		public RelativeVigorIndex2 RelativeVigorIndex2(ISeries<double> input, int period)
		{
			if (cacheRelativeVigorIndex2 != null)
				for (int idx = 0; idx < cacheRelativeVigorIndex2.Length; idx++)
					if (cacheRelativeVigorIndex2[idx] != null && cacheRelativeVigorIndex2[idx].Period == period && cacheRelativeVigorIndex2[idx].EqualsInput(input))
						return cacheRelativeVigorIndex2[idx];
			return CacheIndicator<RelativeVigorIndex2>(new RelativeVigorIndex2(){ Period = period }, input, ref cacheRelativeVigorIndex2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RelativeVigorIndex2 RelativeVigorIndex2(int period)
		{
			return indicator.RelativeVigorIndex2(Input, period);
		}

		public Indicators.RelativeVigorIndex2 RelativeVigorIndex2(ISeries<double> input , int period)
		{
			return indicator.RelativeVigorIndex2(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RelativeVigorIndex2 RelativeVigorIndex2(int period)
		{
			return indicator.RelativeVigorIndex2(Input, period);
		}

		public Indicators.RelativeVigorIndex2 RelativeVigorIndex2(ISeries<double> input , int period)
		{
			return indicator.RelativeVigorIndex2(input, period);
		}
	}
}

#endregion
