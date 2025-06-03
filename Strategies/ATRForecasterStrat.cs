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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class ATRForecasterStrat : Strategy
	{
		ATRForecaster af;
		SortedDictionary<DayOfWeek,List<double>> DOWRanges = new SortedDictionary<DayOfWeek,List<double>>();
		int JulianStartTime = 0;
		int JulianStopTime = 0;
		int[] MinsPerBar = new int[3]{0,0,0};
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "ATR Forecaster";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;

				pTargetMultiple = 0.5;
				pQty = 1;

				pDaysOfWeek = "12345";
				pExcludeTimes = "//930-1000, 1400-1550";
				pEnableBEStop = false;
				pStartTime = 930;
				pStopTime = 1550;
				pGoFlatTime = 1550;
				AddPlot(new Stroke(Brushes.Lime, 3),    PlotStyle.Dot, "BuyLvl1");
				AddPlot(new Stroke(Brushes.Magenta, 3), PlotStyle.Dot, "SellLvl1");
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				af = ATRForecaster(14, 0.75);
				af.pDevelopingLevels = true;

				int hr = pStartTime/100;
				int min = pStartTime - hr*100;
				JulianStartTime = hr*60+min;
				hr = this.pGoFlatTime/100;
				min = pGoFlatTime - hr*100;
				JulianStopTime = hr*60+min;

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
			}else if(State == State.DataLoaded){
				double H = double.MinValue;
				double L = double.MaxValue;
				DateTime t=DateTime.MinValue;
				DateTime t1=DateTime.MinValue;
				for(int i = 1; i<BarsArray[0].Count; i++){
					if(H!=double.MinValue){
						t = Times[0].GetValueAt(i);
						t1 = Times[0].GetValueAt(i-1);
						if(!DOWRanges.ContainsKey(t1.DayOfWeek)) DOWRanges[t1.DayOfWeek] = new List<double>();
						if(t.Day != t1.Day){
							DOWRanges[t1.DayOfWeek].Add(H-L);
							H = Highs[0].GetValueAt(i);
							L = Lows[0].GetValueAt(i);
						}
					}
					H = Math.Max(Highs[0].GetValueAt(i), H);
					L = Math.Min(Lows[0].GetValueAt(i), L);
				}
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

		int TPTicks = 0;
		void P(int i){Print(i);}
		int LongCount = 0;
		int ShortCount = 0;
		Order entryOrder = null;
		protected override void OnBarUpdate()
		{
			int bip = BarsInProgress;
			DateTime t = Times[0][0];
			var c1 = !IsExcluded(ToTime(t), t.DayOfWeek);
			double PctOfDay = 0;//Math.Max(0.1, (t.Hour*60.0 + t.Minute - JulianStartTime)/(JulianStopTime - JulianStartTime));
var z = t.Day==21 && t.Month==6;
			if(CurrentBars[bip]>2){
				if(c1 &&
						Lows[0][0] <= af.ATRAvgLower[0] && 
						Closes[0][0] > af.ATRAvgLower[0] && 
						Position.MarketPosition != MarketPosition.Long){
					entryOrder = EnterLong(pQty, "Laf");
					LongCount++;
					var ticks = Convert.ToInt32((Closes[0][0] - Lows[0][0])/TickSize) + 1;
					SetStopLoss("Laf", CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32((af.ATRAvgUpper[0]-af.ATRAvgLower[0]) * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget("Laf", CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"afTP{bip}-{CurrentBars[bip]}", true, 0, af.ATRAvgLower[0] + TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"afSL{bip}-{CurrentBars[bip]}", false, 0, Lows[0][0]-TickSize, Brushes.Magenta);
				}
				if(c1 &&
						Highs[0][0] >= af.ATRAvgUpper[0] && 
						Closes[0][0] < af.ATRAvgUpper[0] && 
						Position.MarketPosition != MarketPosition.Short){
					entryOrder = EnterShort(pQty, "Saf");
					ShortCount++;
					var ticks = Convert.ToInt32((Highs[0][0]-Closes[0][0])/TickSize) + 1;
					SetStopLoss("Saf", CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32((af.ATRAvgUpper[0]-af.ATRAvgLower[0]) * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget("Saf", CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"afTP{bip}-{CurrentBars[bip]}", true, 0, af.ATRAvgUpper[0] - TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"afSL{bip}-{CurrentBars[bip]}", false, 0, Highs[0][0]+TickSize, Brushes.Magenta);
				}
			}

			if(pEnableBEStop){
				if(Position.MarketPosition == MarketPosition.Long){
					double trigger = Instrument.MasterInstrument.RoundToTickSize(TPTicks*TickSize/3 + entryPrice);
					if(Lows[0][0] > trigger){
						SetStopLoss("Laf", CalculationMode.Ticks, 5, false);
						Draw.Diamond(this,$"afSL{bip}-{CurrentBars[bip]}", false, 0, entryPrice + TickSize*5, Brushes.Magenta);
					}
				}
				if(Position.MarketPosition == MarketPosition.Short){
					double trigger = Instrument.MasterInstrument.RoundToTickSize(entryPrice - TPTicks*TickSize/3);
					if(Highs[0][0] < trigger){
						SetStopLoss("Saf", CalculationMode.Ticks, 5, false);
						Draw.Diamond(this,$"afSL{bip}-{CurrentBars[bip]}", false, 0, entryPrice - TickSize*5, Brushes.Magenta);
					}
				}
			}
			BuyLvl1[0] = af.ATRAvgLower[0];
			SellLvl1[0] = af.ATRAvgUpper[0];

			if(Position.MarketPosition != MarketPosition.Flat && ToTime(t)/100 > pGoFlatTime){
				//Print("End of day on "+Times[0][0].ToString());
				if(Position.MarketPosition == MarketPosition.Long) ExitLong();
				if(Position.MarketPosition == MarketPosition.Short) ExitShort();
			}
			if(bip == 0 && CurrentBars[0] > BarsArray[0].Count-3){
				Print("Order counts:");
				Print(LongCount+"-long / "+ShortCount+"-short");
			}
		}

		double entryPrice = 0;
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (execution.Order.OrderState == OrderState.Filled || execution.Order.OrderState == OrderState.PartFilled || (execution.Order.OrderState == OrderState.Cancelled && execution.Order.Filled > 0))
			{
				entryPrice = execution.Order.AverageFillPrice;
			}
		}
		protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
		{
//Print(string.Format("OPU   position: {0}  mktPos {1}", position.ToString(), marketPosition.ToString()));
			if(marketPosition == MarketPosition.Flat){
//Print("OpU   Flat");
//				if(Account != null && Account.Orders != null){
//					var L = Account.Orders.ToList();
//					Print("L.Count: "+L.Count);
//foreach(var pp in L) Print(pp.ToString()+"  "+pp.Id+"  entry signal: "+(pp.Name));//!=null?pp.Order.FromEntrySignal:"order was null"));
//				}else Print("Account was null or Account.Executions is null");
//Print("OPU   FLAT  qty: "+ Account.Executions.Last().Quantity+"  $$:  "+Account.Executions.Last().Price);
			}
		}
		#region Properties
//		[NinjaScriptProperty]
//		[Range(0.00, double.MaxValue)]
//		[Display(Name="Exhaustion Mult 1", Order=10, GroupName="Parameters")]
//		public double pExhaustionMult1
//		{ get; set; }
//		[NinjaScriptProperty]
//		[Range(0, 3)]
//		[Display(Name="Timeframe 1", Order=10, GroupName="Parameters")]
//		public int pTimeframe1
//		{ get; set; }


		[NinjaScriptProperty]
		[Display(Name="Qty", Order=10, GroupName="Parameters")]
		public int pQty
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Target multiple", Order=30, GroupName="Parameters")]
		public double pTargetMultiple
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Days Of week", Order=40, GroupName="Parameters")]
		public string pDaysOfWeek
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Exclude", Order=45, GroupName="Parameters")]
		public string pExcludeTimes
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
		[Display(Name="Enable BE Stop", Order=60, GroupName="Parameters")]
		public bool  pEnableBEStop
		{get;set;}
		#endregion

		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyLvl1
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellLvl1
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyLvl2
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellLvl2
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyLvl3
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellLvl3
		{
			get { return Values[5]; }
		}
		#endregion
	}
}
