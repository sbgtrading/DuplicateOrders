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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SampleMACrossOverWithQty : Strategy
	{
		private WellesMA maFast;
		private WellesMA maSlow;

		Brush OutOfSessionBrush = null;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description	= NinjaTrader.Custom.Resource.NinjaScriptStrategyDescriptionSampleMACrossOver;
				Name		= "SampleMACrossOverWithQty";
				Fast		= 10;
				Slow		= 25;
				Qty = 1;
				pStartTime = 930;
				pStopTime = 1550;
				pGoFlatTime = 1550;
				pExcludeTimes = "";
				pNumTradesSameDirection = 1;
				pTargetTicks = 30;
				pSLTicks = 10;
				pBETicks = 0;
				pTrailStopEngaged = false;
				pDaysOfWeek = "12345";

				// This strategy has been designed to take advantage of performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.DataLoaded)
			{
				maFast = WellesMA(Fast);
				maSlow = WellesMA(Slow);

				maFast.Plots[0].Brush = Brushes.Goldenrod;
				maSlow.Plots[0].Brush = Brushes.SeaGreen;

				AddChartIndicator(maFast);
				AddChartIndicator(maSlow);
				SetStopLoss(CalculationMode.Ticks, pSLTicks);
				SetProfitTarget(CalculationMode.Ticks, pTargetTicks);
				
				var s = pDaysOfWeek.ToUpper().Trim();
				if(s.Contains("0") || s=="ALL") DOW.Add(DayOfWeek.Sunday);
				if(s.Contains("1") || s=="ALL" || s=="WEEKDAY") DOW.Add(DayOfWeek.Monday);
				if(s.Contains("2") || s=="ALL" || s=="WEEKDAY") DOW.Add(DayOfWeek.Tuesday);
				if(s.Contains("3") || s=="ALL" || s=="WEEKDAY") DOW.Add(DayOfWeek.Wednesday);
				if(s.Contains("4") || s=="ALL" || s=="WEEKDAY") DOW.Add(DayOfWeek.Thursday);
				if(s.Contains("5") || s=="ALL" || s=="WEEKDAY") DOW.Add(DayOfWeek.Friday);
				if(s.Contains("6") || s=="ALL") DOW.Add(DayOfWeek.Saturday);
				
				ExcludedTimesList.Add(Tuple.Create(pStopTime*100, pStartTime*100));
				string ex = pExcludeTimes;
				if(ex.Contains("//")) ex = ex.Substring(0, ex.IndexOf("//"));
				while(ex.Contains(" -")) ex = ex.Replace(" -","-");
				while(ex.Contains("- ")) ex = ex.Replace("- ","-");
				var range = ex.Split(new char[]{' ',','},StringSplitOptions.RemoveEmptyEntries);// = "930-1000, 1400-1550";
				foreach(var tt in range){
					var startstop = tt.Split(new char[]{'-'});
					var start = Convert.ToInt32(startstop[0]);
					var stop = Convert.ToInt32(startstop[1]);
					ExcludedTimesList.Add(Tuple.Create(start*100,stop*100));
				}
				OutOfSessionBrush = Brushes.Crimson.Clone();
				OutOfSessionBrush.Opacity = 0.5;
				OutOfSessionBrush.Freeze();
				ClearOutputWindow();
			}
		}
		List<DayOfWeek> DOW = new List<DayOfWeek>();
		List<Tuple<int,int>> ExcludedTimesList = new List<Tuple<int,int>>();
		private bool IsExcluded(int t, DayOfWeek dow){
			if(!DOW.Contains(dow)) return true;
			foreach(var e in ExcludedTimesList){
				if(e.Item1 > e.Item2){
					//Print(t+" session:   "+e.Item1+" to "+e.Item2);
					if(t > e.Item1) return true;
					if(t < e.Item2) return true;
				}else if(t >= e.Item1 && t <= e.Item2) return true;
			}
			return false;
		}
		
		int TradesSinceCross = 0;
		int direction = 0;
		int LONG = 1;
		int FLAT = 0;
		int SHORT = -1;
		int BOTH = 999;
		bool BEEngaged = false;
		double SLPrice = 0;
		int TradeABar = 0;
		int SessionID = -1;
		double trigger = 0;
		private void p(string s){return; Print(s);}
		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(2,BarsRequiredToTrade))
				return;
			if(Position.MarketPosition == MarketPosition.Flat){
				BEEngaged = false;
				SLPrice = 0;
				trigger = 0;
			}

			DateTime t = Times[0][0];
			var c1 = !IsExcluded(ToTime(t), t.DayOfWeek);
			if(!c1) {
				BackBrushes[0] = OutOfSessionBrush; 
				TradesSinceCross = 0;
				direction = FLAT;
			}else BackBrushes[0] = null;

			if(direction == LONG && maFast[0] < maSlow[0]) TradesSinceCross = 0;
			if(direction == SHORT && maFast[0] > maSlow[0]) TradesSinceCross = 0;
			var PermittedDir = BOTH;
			if(direction == LONG && pNumTradesSameDirection <= TradesSinceCross) PermittedDir = SHORT;
			if(direction == SHORT && pNumTradesSameDirection <= TradesSinceCross) PermittedDir = LONG;
			var PriorSLPrice = SLPrice;

			var c2 = Closes[0][1] <= maSlow[1] && Closes[0][0] > maSlow[0];
			var c3 = Closes[0][1] >= maSlow[1] && Closes[0][0] < maSlow[0];
			if (c1 && (PermittedDir == BOTH || PermittedDir == LONG) && maFast[1] > maSlow[1] && Closes[0][0]>=Medians[0][0] && c2){
				SetStopLoss(CalculationMode.Ticks, pSLTicks);
				EnterLong(Qty);
				TradeABar = CurrentBar;
				if(direction != LONG){
					direction = LONG;
					TradesSinceCross = 1;
				}else{
					TradesSinceCross++;
				}
			}
			else if (c1 && (PermittedDir == BOTH || PermittedDir == SHORT) && maFast[1] < maSlow[1] && Closes[0][0]<=Medians[0][0] && c3){
				SetStopLoss(CalculationMode.Ticks, pSLTicks);
				EnterShort(Qty);
				TradeABar = CurrentBar;
				if(direction != SHORT){
					direction = SHORT;
					TradesSinceCross = 1;
				}else{
					TradesSinceCross++;
				}
			}
if(t.Day==11 && t.Month==7 && (t.Hour==13 || t.Hour==14)){
	Print(t.ToString()+"   c3: "+c3.ToString()+"  TradesSinceCross: "+TradesSinceCross+"   PermittedDir: "+PermittedDir);
}
			if(TradeABar == CurrentBar-1 && Position.MarketPosition != MarketPosition.Flat){
				if(Position.MarketPosition == MarketPosition.Long){
					SLPrice = Position.AveragePrice - TickSize * pSLTicks;
				}else{
					SLPrice = Position.AveragePrice + TickSize * pSLTicks;
				}
				p("    "+Position.MarketPosition.ToString()+"   Position $: "+Position.AveragePrice.ToString()+"  SLPrice: "+SLPrice);
			}

			if(pTrailStopEngaged && TradeABar < CurrentBar-2){
				if(Position.MarketPosition == MarketPosition.Long){
					SLPrice = Math.Max(SLPrice, maSlow[0]);
				}else if(Position.MarketPosition == MarketPosition.Short){
					SLPrice = Math.Min(SLPrice, maSlow[0]);
				}
			}
			else if(pBETicks > 0 && !BEEngaged){
				if(Position.MarketPosition == MarketPosition.Long){
					trigger = Instrument.MasterInstrument.RoundToTickSize(pBETicks*TickSize + Position.AveragePrice);
					if(Closes[0][0] > trigger){
						if(!BEEngaged){
							SLPrice = Position.AveragePrice - TickSize;
							SetStopLoss(CalculationMode.Price, SLPrice);
							BEEngaged = true;
						}
					}
				}
				if(Position.MarketPosition == MarketPosition.Short){
					trigger = Instrument.MasterInstrument.RoundToTickSize(Position.AveragePrice - pBETicks*TickSize);
					if(Closes[0][0] < trigger){
						if(!BEEngaged){
							SLPrice = Position.AveragePrice + TickSize;
							SetStopLoss(CalculationMode.Price, SLPrice);
							BEEngaged = true;
						}
					}
				}
			}
			if(trigger!=0 && !BEEngaged && Position.MarketPosition != MarketPosition.Flat) Draw.Dot(this,$"afBET-{CurrentBars[0]}", true, 0, trigger, Brushes.Orange);
			if(Position.MarketPosition != MarketPosition.Flat){
				if(PriorSLPrice != SLPrice) {
					SLPrice = Instrument.MasterInstrument.RoundToTickSize(SLPrice);
					SetStopLoss(CalculationMode.Price, SLPrice);
					p(t.ToString()+"     SL moved to "+SLPrice);
				}
				Draw.Diamond(this,$"afSL-{CurrentBars[0]}", false, 0, SLPrice, Brushes.Magenta);
			}

			if(Position.MarketPosition != MarketPosition.Flat && ToTime(t)/100 > pGoFlatTime){
				//Print("End of day on "+Times[0][0].ToString());
				if(Position.MarketPosition == MarketPosition.Long) ExitLong();
				if(Position.MarketPosition == MarketPosition.Short) ExitShort();
				TradesSinceCross = 0;
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 5)]
		public int Slow
		{ get; set; }
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Qty", GroupName = "NinjaScriptStrategyParameters", Order = 10)]
		public int Qty
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Exclusion times", Order=45, GroupName="Parameters")]
		public string pExcludeTimes
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Days Of week", Order=40, GroupName="Parameters")]
		public string pDaysOfWeek
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Start time", Order=50, GroupName="Parameters")]
		public int pStartTime
		{get; set;}

		[NinjaScriptProperty]
		[Display(Name="Stop time", Order=51, GroupName="Parameters")]
		public int pStopTime
		{get; set;}

		[NinjaScriptProperty]
		[Display(Name="Go flat time", Order=52, GroupName="Parameters")]
		public int pGoFlatTime
		{get; set;}
		
		[NinjaScriptProperty]
		[Display(Name="Ticks target", Order=70, GroupName="Parameters")]
		public int  pTargetTicks
		{get;set;}
		
		[NinjaScriptProperty]
		[Display(Name="Ticks SL", Order=80, GroupName="Parameters")]
		public int  pSLTicks
		{get;set;}
		
		[NinjaScriptProperty]
		[Display(Name="Ticks BE trigger", Order=90, GroupName="Parameters")]
		public int  pBETicks
		{get;set;}
		
		[NinjaScriptProperty]
		[Display(Name="Trail on slow ma?", Order=95, GroupName="Parameters")]
		public bool pTrailStopEngaged
		{get;set;}
		
		[NinjaScriptProperty]
		[Display(Name="# trades in same direction", Order=100, GroupName="Parameters")]
		public int  pNumTradesSameDirection
		{get;set;}
		#endregion
	}
}
