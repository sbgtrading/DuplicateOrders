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
	public class RSIandMACD : Strategy
	{
		private MACDwithAlert macd;
		private RSIwithAlert rsi;

		Brush OutOfSessionBrush = null;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description	= "";
				Name		= "RSI and MACD";
				MAFast		= 12;
				MASlow		= 26;
				RSIperiod = 10;
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
				macd = MACDwithAlert(MAFast, MASlow, 9, false);
				rsi = RSIwithAlert(RSIperiod,1, 75, 25);

				AddChartIndicator(macd);
				AddChartIndicator(rsi);
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
		int RSIdir = 0;
		double trigger = 0;
		double PriorSLPrice = 0;
		double entryPrice = 0;
		bool z = false;
		private void p(string s){if(!z)return; Print(s);}
		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(2,BarsRequiredToTrade))
				return;

			DateTime t = Times[0][0];
			var c1 = !IsExcluded(ToTime(t), t.DayOfWeek);
			if(!c1) {
				BackBrushes[0] = OutOfSessionBrush; 
				TradesSinceCross = 0;
				direction = FLAT;
			}else BackBrushes[0] = null;

			if(rsi[1] <= rsi.pOSLevel) {
				if(RSIdir!=LONG) TradesSinceCross = 0;
				RSIdir = LONG;
			}
			if(rsi[1] >= rsi.pOBLevel) {
				if(RSIdir!=SHORT) TradesSinceCross = 0;
				RSIdir = SHORT;
			}
			var PermittedDir = BOTH;
			if(Position.MarketPosition == MarketPosition.Short) PermittedDir = LONG;
			else if(Position.MarketPosition == MarketPosition.Long) PermittedDir = SHORT;
			if(direction == LONG && pNumTradesSameDirection <= TradesSinceCross) PermittedDir = SHORT;
			if(direction == SHORT && pNumTradesSameDirection <= TradesSinceCross) PermittedDir = LONG;

			var c2 = RSIdir == LONG && macd[1] < 0 && CrossAbove(macd, macd.Avg, 1);
			var c3 = RSIdir == SHORT && macd[1] > 0 && CrossBelow(macd, macd.Avg, 1);
			if (c1 && (PermittedDir == BOTH || PermittedDir == LONG) && c2){
				BEEngaged = false;
				SLPrice = 0;
				trigger = 0;
				SetStopLoss("E", CalculationMode.Ticks, pSLTicks, false);
				entryOrder = EnterLong(Qty,"E");
				TradeABar = CurrentBar;
				if(direction != LONG){
					direction = LONG;
					TradesSinceCross = 1;
				}else{
					TradesSinceCross++;
				}
			}
			else if (c1 && (PermittedDir == BOTH || PermittedDir == SHORT) && c3){
				BEEngaged = false;
				SLPrice = 0;
				trigger = 0;
				SetStopLoss("E", CalculationMode.Ticks, pSLTicks, false);
				entryOrder = EnterShort(Qty,"E");
				TradeABar = CurrentBar;
				if(direction != SHORT){
					direction = SHORT;
					TradesSinceCross = 1;
				}else{
					TradesSinceCross++;
				}
			}
z = false;
if(t.Day==5 && t.Month==7 && (t.Hour==14 && t.Minute>=25 && t.Minute<=33)){
	z = true;
	Print(Times[0][0].ToString());
}
			if(TradeABar == CurrentBar-1 && Position.MarketPosition != MarketPosition.Flat){
				if(Position.MarketPosition == MarketPosition.Long){
					SLPrice = entryPrice - TickSize * pSLTicks;
				}else{
					SLPrice = entryPrice + TickSize * pSLTicks;
				}
				p("    "+Position.MarketPosition.ToString()+"   Position $: "+entryPrice.ToString()+"  SLPrice: "+SLPrice);
			}

			if(pTrailStopEngaged && TradeABar < CurrentBar-3){
				if(Position.MarketPosition == MarketPosition.Long){
					SLPrice = Math.Max(SLPrice, Math.Min(Low[1], Low[2]));
				}else if(Position.MarketPosition == MarketPosition.Short){
					SLPrice = Math.Min(SLPrice, Math.Max(High[1], High[2]));
				}
			}
			else if(pBETicks > 0 && !BEEngaged && TradeABar <= CurrentBar-1 && Position.MarketPosition != MarketPosition.Flat){
				if(Position.MarketPosition == MarketPosition.Long){
					trigger = Instrument.MasterInstrument.RoundToTickSize(pBETicks*TickSize + entryPrice);
					if(Closes[0][1] > trigger && Low[0] > entryPrice){
						SLPrice = entryPrice - TickSize;
						BEEngaged = true;
					}
				}
				if(Position.MarketPosition == MarketPosition.Short){
					trigger = Instrument.MasterInstrument.RoundToTickSize(entryPrice - pBETicks*TickSize);
					if(Closes[0][1] < trigger && High[0] < entryPrice){
						SLPrice = entryPrice + TickSize;
						BEEngaged = true;
					}
				}
			}
if(z && entryOrder!=null) Print(t.ToString()+"   trigger: "+trigger.ToString()+"   entry price: "+entryPrice+"  BEEngaged: "+BEEngaged.ToString()+"  SLPrice: "+SLPrice+"   Prior SLPrice: "+PriorSLPrice);
			if(trigger!=0 && !BEEngaged && Position.MarketPosition != MarketPosition.Flat) Draw.Dot(this,$"afBET-{CurrentBars[0]}", true, 0, trigger, Brushes.Orange);
			if(Position.MarketPosition != MarketPosition.Flat){
				if(PriorSLPrice != SLPrice)
				{
					p(t.ToString()+"     SL moved to "+SLPrice);
					PriorSLPrice = SLPrice;
				}
				SLPrice = Instrument.MasterInstrument.RoundToTickSize(SLPrice);
				if(stopOrder!=null){
					stopOrder.StopPriceChanged = SLPrice;
					try{
						this.Account.Change(new[] { stopOrder });
					}catch(Exception ee){Print("Account.Change produced this: "+ee.ToString());}
				}
				else {
					if(z) Print(t.ToString()+"   Order was null, couldn't use order.StopPrice = X, "+Position.MarketPosition.ToString());
					SetStopLoss(entryOrder.Name, CalculationMode.Price, SLPrice, false);
				}
				if(SLPrice != 0)
					Draw.Diamond(this,$"afSL-{CurrentBars[0]}", false, 0, SLPrice, Brushes.Magenta);
			}

			if(Position.MarketPosition != MarketPosition.Flat && ToTime(t)/100 > pGoFlatTime){
				//Print("End of day on "+Times[0][0].ToString());
				if(Position.MarketPosition == MarketPosition.Long) ExitLong();
				if(Position.MarketPosition == MarketPosition.Short) ExitShort();
				TradesSinceCross = 0;
			}
		}
//		private Account myAccount;
		private Order entryOrder = null; // This variable holds an object representing our entry order
		private Order stopOrder = null; // This variable holds an object representing our stop loss order
		private Order targetOrder = null; // This variable holds an object representing our profit target order
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(z)Print("Order filled, price: "+price);
			/* We advise monitoring OnExecution to trigger submission of stop/target orders instead of OnOrderUpdate() since OnExecution() is called after OnOrderUpdate()
			which ensures your strategy has received the execution which is used for internal signal tracking. */
			if (entryOrder != null && entryOrder == execution.Order)
			{
				if (execution.Order.OrderState == OrderState.Filled || execution.Order.OrderState == OrderState.PartFilled || (execution.Order.OrderState == OrderState.Cancelled && execution.Order.Filled > 0))
				{
					if(entryOrder.IsLong){
						//Print("Entry order is long");
			          // Submit exit orders for partial fills
						entryPrice = execution.Order.AverageFillPrice;
						if (execution.Order.OrderState == OrderState.PartFilled)
						{
							SLPrice = entryPrice - pSLTicks * TickSize;
//							Print("Setting SL to: "+SLPrice);
//		    	           stopOrder = ExitLongStopMarket(0, true, execution.Order.Filled, SLPrice, "SL", entryOrder.Name);
//							Print("   stopOrder.Price: "+stopOrder.StopPrice);
//		        	       targetOrder = ExitLongLimit(0, true, execution.Order.Filled, entryPrice + pTargetTicks * TickSize, "Tgt", entryOrder.Name);
						}
						// Update our exit order quantities once orderstate turns to filled and we have seen execution quantities match order quantities
						else if (execution.Order.OrderState == OrderState.Filled)
						{
							SLPrice = entryPrice - pSLTicks * TickSize;
//							Print("Setting SL to: "+SLPrice);
//						   stopOrder = ExitLongStopMarket(0, true, execution.Order.Filled, SLPrice, "SL", entryOrder.Name);
//							Print("   stopOrder.Price: "+stopOrder.StopPrice);
//						   targetOrder = ExitLongLimit(0, true, execution.Order.Filled, entryPrice + pTargetTicks * TickSize, "Tgt", entryOrder.Name);
						}
//						if(stopOrder!=null) Print(Times[0][0].ToString()+ "  Stop price set to: "+stopOrder.StopPrice);
				   }else{
						//Print("Entry order is short");
			          // Submit exit orders for partial fills
						entryPrice = execution.Order.AverageFillPrice;
						if (execution.Order.OrderState == OrderState.PartFilled)
						{
							SLPrice = entryPrice + pSLTicks * TickSize;
//							Print("Setting SL to: "+SLPrice);
//		    	           stopOrder = ExitShortStopMarket(0, true, execution.Order.Filled, SLPrice, "SL", entryOrder.Name);
//							Print("   stopOrder.Price: "+stopOrder.StopPrice);
//		        	       targetOrder = ExitShortLimit(0, true, execution.Order.Filled, entryPrice - pTargetTicks * TickSize, "Tgt", entryOrder.Name);
						}
						// Update our exit order quantities once orderstate turns to filled and we have seen execution quantities match order quantities
						else if (execution.Order.OrderState == OrderState.Filled)
						{
							SLPrice = entryPrice + pSLTicks * TickSize;
//							Print("Setting SL to: "+SLPrice);
//						   stopOrder = ExitShortStopMarket(0, true, execution.Order.Filled, SLPrice, "SL", entryOrder.Name);
//							Print("   stopOrder.Price: "+stopOrder.StopPrice);
//						   targetOrder = ExitShortLimit(0, true, execution.Order.Filled, entryPrice - pTargetTicks * TickSize, "Tgt", entryOrder.Name);
						}
//						if(stopOrder!=null) Print(Times[0][0].ToString()+ "  Stop price set to: "+stopOrder.StopPrice);
				   }
		       }
		   }
		 
		  // Reset our stop order and target orders' Order objects after our position is closed. (1st Entry)
		  if ((stopOrder != null && stopOrder == execution.Order) || (targetOrder != null && targetOrder == execution.Order))
		   {
		      if (execution.Order.OrderState == OrderState.Filled || execution.Order.OrderState == OrderState.PartFilled)
		       {
		           stopOrder = null;
		           targetOrder = null;
		       }
		   }
		}
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MA Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int MAFast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MA Slow", GroupName = "NinjaScriptStrategyParameters", Order = 5)]
		public int MASlow
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "RSI period", GroupName = "NinjaScriptStrategyParameters", Order = 10)]
		public int RSIperiod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Qty", GroupName = "NinjaScriptStrategyParameters", Order = 20)]
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
