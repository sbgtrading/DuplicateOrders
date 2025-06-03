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
	public class ExhaustionBounce : Strategy
	{
		AmbushSignals ams1;
		AmbushSignals ams2;
		AmbushSignals ams3;
		SortedDictionary<DayOfWeek,List<double>> DOWRanges = new SortedDictionary<DayOfWeek,List<double>>();
		int JulianStartTime = 0;
		int JulianStopTime = 0;
		int[] MinsPerBar = new int[3]{0,0,0};
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Exhaustion Bounce";
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
				pExhaustionMult1		= 0.1;
				pExhaustionMult2		= 0.1;
				pExhaustionMult3		= 0.1;
				pTargetMultiple = 0.5;
				pTimeframe1 = 1;
				pTimeframe2 = 2;
				pTimeframe3 = 3;
				pDaysOfWeek = "12345";
				pExcludeTimes = "//930-1000, 1400-1550";
				pEnableBEStop = false;
				pStartTime = 930;
				pStopTime = 1550;
				pGoFlatTime = 1550;
				AddPlot(new Stroke(Brushes.Lime, 3),    PlotStyle.Dot, "BuyLvl1");
				AddPlot(new Stroke(Brushes.Magenta, 3), PlotStyle.Dot, "SellLvl1");
				AddPlot(new Stroke(Brushes.Green, 3),   PlotStyle.Dot, "BuyLvl2");
				AddPlot(new Stroke(Brushes.Red, 3),     PlotStyle.Dot, "SellLvl2");
				AddPlot(new Stroke(Brushes.Cyan, 3),    PlotStyle.Dot, "BuyLvl3");
				AddPlot(new Stroke(Brushes.Pink, 3),    PlotStyle.Dot, "SellLvl3");
			}
			else if (State == State.Configure)
			{
				MinsPerBar[0] = 60;
				MinsPerBar[1] = 360;
				MinsPerBar[2] = 1440;
				AddDataSeries(Data.BarsPeriodType.Minute, MinsPerBar[0]);
				AddDataSeries(Data.BarsPeriodType.Minute, MinsPerBar[1]);
				AddDataSeries(Data.BarsPeriodType.Minute, MinsPerBar[2]);

				ClearOutputWindow();
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
				if(pTimeframe1>0)
					sg1 = new SignalGenerator(pTimeframe1, MinsPerBar[pTimeframe1-1]+"-mins", pExhaustionMult1, TickSize);
				if(pTimeframe2>0)
					sg2 = new SignalGenerator(pTimeframe2, MinsPerBar[pTimeframe2-1]+"-mins", pExhaustionMult2, TickSize);
				if(pTimeframe3>0)
					sg3 = new SignalGenerator(pTimeframe3, MinsPerBar[pTimeframe3-1]+"-mins", pExhaustionMult3, TickSize);
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
		private class SignalGenerator{
			public List<double> ranges = new List<double>();
			public int DataId = -1;
			public  double BuyLvl = 0;
			public  double SellLvl = 0;
			private double Overshoot;
			private double TickSize;
			public int LongCount = 0;
			public int ShortCount = 0;
			public string TimeDescription = "";
			public  double NO_VALUE = double.MinValue;
			public SignalGenerator(int dataId, string timeDesc, double overshoot, double ticksize) {
				TimeDescription = timeDesc;
				DataId = dataId;
				Overshoot = overshoot;
				TickSize = ticksize;
			}
			private double RoundToTickSize(double p){int x = Convert.ToInt32(p/TickSize); return x*TickSize;}

			public void Update(int CurrentBar, bool IsFirstTickOfBar, double H1, double L1, double H0, double L0, double C1, double C0, double O1, double O0){
				if(IsFirstTickOfBar) ranges.Add(H1-L1);
				if(CurrentBar<5) return;
				while(ranges.Count>14) ranges.RemoveAt(0);
				double overshoot = ranges.Average() * Overshoot;
				if(ranges.Count>2){
					overshoot = Math.Max(H1,H0) - Math.Min(L1,L0);
					overshoot = overshoot * Overshoot;
				}
				bool c1 = C0>O0;//upclose
				bool c2 = C1>O1;//upclose
				if(c1 && c2)
					SellLvl = RoundToTickSize(H0 + overshoot);
				else SellLvl = NO_VALUE;

				c1 = C0<O0;//downclose
				c2 = C1<O1;//downclose
				if(c1 && c2)
					BuyLvl = RoundToTickSize(L0 - overshoot);
				else BuyLvl = NO_VALUE;
			}
		}
		SignalGenerator sg1 = null;
		SignalGenerator sg2 = null;
		SignalGenerator sg3 = null;
		int TPTicks = 0;
		Order[] entryOrder = new Order[4]{null,null,null,null};
		void P(int i){Print(i);}
		protected override void OnBarUpdate()
		{
			if(Position.MarketPosition==MarketPosition.Flat){
				entryOrder[0]=null;
				entryOrder[1]=null;
				entryOrder[2]=null;
				entryOrder[3]=null;
			}
			int bip = BarsInProgress;
			DateTime t = Times[0][0];
			var c1 = !IsExcluded(ToTime(t), t.DayOfWeek);
			double PctOfDay = 0;//Math.Max(0.1, (t.Hour*60.0 + t.Minute - JulianStartTime)/(JulianStopTime - JulianStartTime));
var z = t.Day==21 && t.Month==6;
			#region -- Timeframe 1 --
			if(sg1!=null && CurrentBars[bip]>2){
				if(bip==sg1.DataId) sg1.Update(CurrentBars[bip], IsFirstTickOfBar, Highs[bip][1], Lows[bip][1], Highs[bip][0], Lows[bip][0], Closes[bip][1], Closes[bip][0], Opens[bip][1], Opens[bip][0]);
				if(sg1.BuyLvl != sg1.NO_VALUE && c1 &&
						Lows[0][0] <= sg1.BuyLvl && 
						Closes[0][0] > sg1.BuyLvl && 
						Position.MarketPosition != MarketPosition.Long){
					entryOrder[bip] = EnterLong($"L{sg1.DataId}");
					sg1.LongCount++;
					var ticks = Convert.ToInt32((Closes[0][0] - Lows[0][0])/TickSize) + 1;
					SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32(DOWRanges[t.DayOfWeek].Average() * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget(CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"ExBounceTP{bip}-{CurrentBars[bip]}", true, 0, sg1.BuyLvl + TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Lows[0][0]-TickSize, Brushes.Magenta);
				}
				if(sg1.SellLvl != sg1.NO_VALUE && c1 &&
						Highs[0][0] >= sg1.SellLvl && 
						Closes[0][0] < sg1.SellLvl && 
						Position.MarketPosition != MarketPosition.Short){
					entryOrder[bip] = EnterShort($"S{sg1.DataId}");
					sg1.ShortCount++;
					var ticks = Convert.ToInt32((Highs[0][0]-Closes[0][0])/TickSize) + 1;
					SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32(DOWRanges[t.DayOfWeek].Average() * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget(CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"ExBounceTP{bip}-{CurrentBars[bip]}", true, 0, sg1.SellLvl - TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Highs[0][0]+TickSize, Brushes.Magenta);
				}
			}
			#endregion -- Timeframe 1 End --
			#region -- Timeframe 2 --
			if(sg2!=null && CurrentBars[bip]>2){
				if(bip==sg2.DataId) sg2.Update(CurrentBars[bip], IsFirstTickOfBar, Highs[bip][1], Lows[bip][1], Highs[bip][0], Lows[bip][0], Closes[bip][1], Closes[bip][0], Opens[bip][1], Opens[bip][0]);
				if(sg2.BuyLvl != sg2.NO_VALUE && c1 &&
						Lows[0][0] <= sg2.BuyLvl && 
						Closes[0][0] > sg2.BuyLvl && 
						DOW.Contains(t.DayOfWeek) &&
						Position.MarketPosition != MarketPosition.Long){
					entryOrder[bip] = EnterLong($"L{sg2.DataId}");
					sg2.LongCount++;
					var ticks = Convert.ToInt32((Closes[0][0] - Lows[0][0])/TickSize) + 1;
					SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32(DOWRanges[t.DayOfWeek].Average() * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget(CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"ExBounceTP{bip}-{CurrentBars[bip]}", true, 0, sg2.BuyLvl + TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Lows[0][0]-TickSize, Brushes.Magenta);
				}
				if(sg2.SellLvl != sg2.NO_VALUE && c1 &&
						Highs[0][0] >= sg2.SellLvl && 
						Closes[0][0] < sg2.SellLvl && 
						DOW.Contains(Times[0][0].DayOfWeek) &&
						Position.MarketPosition != MarketPosition.Short){
					entryOrder[bip] = EnterShort($"S{sg2.DataId}");
					sg2.ShortCount++;
					var ticks = Convert.ToInt32((Highs[0][0]-Closes[0][0])/TickSize) + 1;
					SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32(DOWRanges[t.DayOfWeek].Average() * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget(CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"ExBounceTP{bip}-{CurrentBars[bip]}", true, 0, sg2.SellLvl - TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Highs[0][0]+TickSize, Brushes.Magenta);
				}
			}
			#endregion -- Timeframe 2 End --
			#region -- Timeframe 3 --
			if(sg3!=null && CurrentBars[bip]>2){
				if(bip==sg3.DataId) sg3.Update(CurrentBars[bip], IsFirstTickOfBar, Highs[bip][1], Lows[bip][1], Highs[bip][0], Lows[bip][0], Closes[bip][1], Closes[bip][0], Opens[bip][1], Opens[bip][0]);
				if(sg3.BuyLvl != sg3.NO_VALUE && c1 &&
						Lows[0][0] <= sg3.BuyLvl && 
						Closes[0][0] > sg3.BuyLvl && 
						DOW.Contains(t.DayOfWeek) &&
						Position.MarketPosition != MarketPosition.Long){
					entryOrder[bip] = EnterLong($"L{sg3.DataId}");
					sg3.LongCount++;
					var ticks = Convert.ToInt32((Closes[0][0] - Lows[0][0])/TickSize) + 1;
					SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32(DOWRanges[t.DayOfWeek].Average() * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget(CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"ExBounceTP{bip}-{CurrentBars[bip]}", true, 0, sg3.BuyLvl + TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Lows[0][0]-TickSize, Brushes.Magenta);
				}
				if(sg3.SellLvl != sg3.NO_VALUE && c1 &&
						Highs[0][0] >= sg3.SellLvl && 
						Closes[0][0] < sg3.SellLvl && 
						DOW.Contains(Times[0][0].DayOfWeek) &&
						Position.MarketPosition != MarketPosition.Short){
					entryOrder[bip] = EnterShort($"S{sg3.DataId}");
					sg3.ShortCount++;
					var ticks = Convert.ToInt32((Highs[0][0]-Closes[0][0])/TickSize) + 1;
					SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, ticks, false);
					TPTicks = Convert.ToInt32(DOWRanges[t.DayOfWeek].Average() * (1-PctOfDay)*pTargetMultiple/TickSize);
					SetProfitTarget(CalculationMode.Ticks, TPTicks);
					Draw.Dot(this,$"ExBounceTP{bip}-{CurrentBars[bip]}", true, 0, sg3.SellLvl - TPTicks*TickSize, Brushes.Lime);
					Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Highs[0][0]+TickSize, Brushes.Magenta);
				}
			}
			#endregion -- Timeframe 3 End --
			if(pEnableBEStop){
				if(Position.MarketPosition == MarketPosition.Long){
					double trigger = Instrument.MasterInstrument.RoundToTickSize(TPTicks*TickSize/3 + Position.AveragePrice);
					if(Lows[0][0] > trigger && entryOrder[bip]!=null){
						SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, 5, false);
						Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Position.AveragePrice + TickSize*5, Brushes.Magenta);
					}
				}
				if(Position.MarketPosition == MarketPosition.Short){
					double trigger = Instrument.MasterInstrument.RoundToTickSize(Position.AveragePrice - TPTicks*TickSize/3);
					if(Highs[0][0] < trigger && entryOrder[bip]!=null){
						SetStopLoss(entryOrder[bip].Name, CalculationMode.Ticks, 5, false);
						Draw.Diamond(this,$"ExBounceSL{bip}-{CurrentBars[bip]}", false, 0, Position.AveragePrice - TickSize*5, Brushes.Magenta);
					}
				}
			}
			if(sg1!=null){
				if(sg1.BuyLvl != sg1.NO_VALUE) BuyLvl1[0] = sg1.BuyLvl;
				if(sg1.SellLvl != sg1.NO_VALUE) SellLvl1[0] = sg1.SellLvl;
			}
			if(sg2!=null){
				if(sg2.BuyLvl != sg2.NO_VALUE) BuyLvl2[0] = sg2.BuyLvl;
				if(sg2.SellLvl != sg2.NO_VALUE) SellLvl2[0] = sg2.SellLvl;
			}
			if(sg3!=null){
				if(sg3.BuyLvl != sg3.NO_VALUE) BuyLvl3[0] = sg3.BuyLvl;
				if(sg3.SellLvl != sg3.NO_VALUE) SellLvl3[0] = sg3.SellLvl;
			}

			if(Position.MarketPosition != MarketPosition.Flat && ToTime(t)/100 > pGoFlatTime){
				//Print("End of day on "+Times[0][0].ToString());
				if(Position.MarketPosition == MarketPosition.Long) ExitLong();
				if(Position.MarketPosition == MarketPosition.Short) ExitShort();
			}
			if(bip == 0 && CurrentBars[0] > BarsArray[0].Count-3){
				Print("Order counts:");
				if(sg1!=null) Print("Timeframe 1:   "+sg1.LongCount+"-long / "+sg1.ShortCount+"-short");
				if(sg2!=null) Print("Timeframe 2:   "+sg2.LongCount+"-long / "+sg2.ShortCount+"-short");
				if(sg3!=null) Print("Timeframe 3:   "+sg3.LongCount+"-long / "+sg3.ShortCount+"-short");
			}
		}

		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
//Print(string.Format("OEU:  mktPos: {0}  ExecName {1}   isExit? {2}", marketPosition, execution.Name, execution.IsExit.ToString()));
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
		[NinjaScriptProperty]
		[Range(0.00, double.MaxValue)]
		[Display(Name="Exhaustion Mult 1", Order=10, GroupName="Parameters")]
		public double pExhaustionMult1
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0, 3)]
		[Display(Name="Timeframe 1", Order=10, GroupName="Parameters")]
		public int pTimeframe1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.00, double.MaxValue)]
		[Display(Name="Exhaustion Mult 2", Order=11, GroupName="Parameters")]
		public double pExhaustionMult2
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0, 3)]
		[Display(Name="Timeframe 2", Order=11, GroupName="Parameters")]
		public int pTimeframe2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.00, double.MaxValue)]
		[Display(Name="Exhaustion Mult 3", Order=12, GroupName="Parameters")]
		public double pExhaustionMult3
		{ get; set; }
		[NinjaScriptProperty]
		[Range(0, 3)]
		[Display(Name="Timeframe 3", Order=12, GroupName="Parameters")]
		public int pTimeframe3
		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, 3)]
//		[Display(Name="Signal Timeframe", Order=20, GroupName="Parameters")]
//		public int pSigTimeframe
//		{ get; set; }

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
