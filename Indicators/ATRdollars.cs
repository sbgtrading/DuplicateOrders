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
	public enum ATRdollars_DaysOfWeek {Mo,Tu,We,Th,Fr,All,Today}
	/// <summary>
	/// The Average True Range (ATR) is a measure of volatility. It was introduced by Welles Wilder
	/// in his book 'New Concepts in Technical Trading Systems' and has since been used as a component
	/// of many indicators and trading systems.
	/// </summary>
	public class ATRdollars : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionATR;
				Name						= "ATRdollars";
				IsSuspendedWhileInactive	= false;
				Period						= 14;
				Calculate = Calculate.OnPriceChange;

				pDOW = ATRdollars_DaysOfWeek.Today;
				AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameATR);
			}
			if(State == State.Configure){
				if(pDOW == ATRdollars_DaysOfWeek.Mo) dow = DayOfWeek.Monday;
				else if(pDOW == ATRdollars_DaysOfWeek.Tu) dow = DayOfWeek.Tuesday;
				else if(pDOW == ATRdollars_DaysOfWeek.We) dow = DayOfWeek.Wednesday;
				else if(pDOW == ATRdollars_DaysOfWeek.Th) dow = DayOfWeek.Thursday;
				else if(pDOW == ATRdollars_DaysOfWeek.Fr) dow = DayOfWeek.Friday;
			}
			if(State == State.DataLoaded && pDOW == ATRdollars_DaysOfWeek.Today){
				ranges.Clear();
				for(int ab = 0; ab<CurrentBars[0]; ab++){
					if(Times[0].GetValueAt(ab).DayOfWeek == Times[0].GetValueAt(CurrentBar).DayOfWeek){
						ranges.Add(Highs[0].GetValueAt(ab)-Lows[0].GetValueAt(ab));
						while(ranges.Count>Period) ranges.RemoveAt(0);
						avg = Instrument.MasterInstrument.PointValue * ranges.Average();
					}
				}
			}

		}

		List<double> ranges = new List<double>();
		double avg = 0;
		DayOfWeek dow = DayOfWeek.Monday; 
		protected override void OnBarUpdate()
		{
			double high0	= High[0];
			double low0		= Low[0];

			if(CurrentBar<2) return;
			if(pDOW == ATRdollars_DaysOfWeek.Today){
				if(State==State.Realtime && Times[0][0].Day != Times[0][1].Day && IsFirstTickOfBar){
					ranges.Clear();
					for(int ab = 0; ab<CurrentBars[0]; ab++){
						if(Times[0].GetValueAt(ab).DayOfWeek == Times[0][0].DayOfWeek){
							ranges.Add(Highs[0].GetValueAt(ab)-Lows[0].GetValueAt(ab));
							while(ranges.Count>Period) ranges.RemoveAt(0);
							avg = Instrument.MasterInstrument.PointValue * ranges.Average();
						}
					}
				}
			}else if(pDOW == ATRdollars_DaysOfWeek.All && IsFirstTickOfBar){
				ranges.Add(high0-low0);
				while(ranges.Count>Period) ranges.RemoveAt(0);
				avg = Instrument.MasterInstrument.PointValue * ranges.Average();
			}else if(IsFirstTickOfBar){
				if(Times[0][0].DayOfWeek == dow){
//					if(Instrument.FullName.StartsWith("SI "))Print("Silver dow: "+Times[0][0].DayOfWeek.ToString());
					ranges.Add(high0-low0);
					while(ranges.Count>Period) ranges.RemoveAt(0);
					avg = Instrument.MasterInstrument.PointValue * ranges.Average();
				}
			}
			Value[0]=avg;
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 10)]
		public int Period
		{ get; set; }
		
		[Display(Name = "DayOfWeek", GroupName = "NinjaScriptParameters", Order = 20, ResourceType = typeof(Custom.Resource))]
		public ATRdollars_DaysOfWeek pDOW
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATRdollars[] cacheATRdollars;
		public ATRdollars ATRdollars(int period)
		{
			return ATRdollars(Input, period);
		}

		public ATRdollars ATRdollars(ISeries<double> input, int period)
		{
			if (cacheATRdollars != null)
				for (int idx = 0; idx < cacheATRdollars.Length; idx++)
					if (cacheATRdollars[idx] != null && cacheATRdollars[idx].Period == period && cacheATRdollars[idx].EqualsInput(input))
						return cacheATRdollars[idx];
			return CacheIndicator<ATRdollars>(new ATRdollars(){ Period = period }, input, ref cacheATRdollars);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATRdollars ATRdollars(int period)
		{
			return indicator.ATRdollars(Input, period);
		}

		public Indicators.ATRdollars ATRdollars(ISeries<double> input , int period)
		{
			return indicator.ATRdollars(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATRdollars ATRdollars(int period)
		{
			return indicator.ATRdollars(Input, period);
		}

		public Indicators.ATRdollars ATRdollars(ISeries<double> input , int period)
		{
			return indicator.ATRdollars(input, period);
		}
	}
}

#endregion
