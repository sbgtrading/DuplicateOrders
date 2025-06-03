//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections;
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
	/// The SwingAlert indicator plots lines that represents the swing high and low points.
	/// </summary>
	public class SwingAlert : Indicator
	{
		private int			constant;
		private double		currentSwingHigh;
		private double		currentSwingLow;
		private ArrayList	lastHighCache;
		private double		lastSwingHighValue;
		private ArrayList	lastLowCache;
		private double		lastSwingLowValue;
		private int			saveCurrentBar;

		private Series<double> swingHighSeries;
		private Series<double> swingHighSwings;
		private Series<double> swingLowSeries;
		private Series<double> swingLowSwings;
		double pRiskDollars = 0;
		double pRewardDollars = 0;
		List<DayOfWeek> DOW = new List<DayOfWeek>();
		int RiskTicks = 0;
		int RewardTicks = 0;
		double RiskATRs = 0;
		double RewardATRs = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionSwing;
				Name						= "SwingAlert";
				DisplayInDataBox			= true;
				PaintPriceMarkers			= false;
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				IsAutoScale    = false;
				pStrength	   = 5;
				pRiskDollars   = 250;
				pRewardDollars = 125;
				pStartTime = 800;
				pStopTime = 1500;
				pGoFlatTime = 1600;
				sRiskAmount    = "$250";
				sRewardAmount  = "$125";
				pConsecLevels = 3;
				sDaysOfWeek = "M Tu W Th F";
				sDirection = "LS";
				pEnableSoundAlerts = false;
				pWAVOnBuyEntry = "<inst>_BuySetup.wav";
				pWAVOnSellEntry = "<inst>_BuySetup.wav";

				AddPlot(new Stroke(Brushes.DarkCyan,	2), PlotStyle.Dot, NinjaTrader.Custom.Resource.SwingHigh);
				AddPlot(new Stroke(Brushes.Goldenrod,	2), PlotStyle.Dot, NinjaTrader.Custom.Resource.SwingLow);
				AddPlot(new Stroke(Brushes.Lime,	4), PlotStyle.TriangleUp, "Buy");
				AddPlot(new Stroke(Brushes.Magenta,	4), PlotStyle.TriangleDown, "Sell");
			}
			else if (State == State.Configure)
			{
				currentSwingHigh	= 0;
				currentSwingLow		= 0;
				lastSwingHighValue	= 0;
				lastSwingLowValue	= 0;
				saveCurrentBar		= -1;
				constant			= (2 * pStrength) + 1;

				string s = string.Empty;
				string sU = sDaysOfWeek.ToUpper();
				if(sU.Contains("ALL")) s = "All";
				if(sU.Contains("M"))   s = s+"M ";
				if(sU.Contains("TU"))  s = s+"Tu ";
				if(sU.Contains("W"))   s = s+"W ";
				if(sU.Contains("TH"))  s = s+"Th ";
				if(sU.Contains("F"))   s = s+"F ";
				if(sU.Contains("SA"))  s = s+"Sa ";
				if(sU.Contains("SU"))  s = s+"Su ";
				sDaysOfWeek = s.Trim();
				sDirection = sDirection.ToUpper();
			}
			else if (State == State.DataLoaded)
			{
				RiskATRs = 0;
				RewardATRs = 0;
				PV = Instrument.MasterInstrument.PointValue;
				if(sRiskAmount.Contains("$")) pRiskDollars = Convert.ToDouble(sRiskAmount.Replace("$",""));
				else if(sRiskAmount.Contains("atr")) {pRiskDollars = 0; RiskATRs=Convert.ToDouble(sRiskAmount.Replace("atr",""));}
				else if(sRiskAmount.Contains("t")) pRiskDollars = Convert.ToDouble(sRiskAmount.Replace("t",""))*TickSize*PV;
				else if(sRiskAmount.Contains("p")) pRiskDollars = Convert.ToDouble(sRiskAmount.Replace("p",""))*PV;

				if(sRewardAmount.Contains("$")) pRewardDollars = Convert.ToDouble(sRewardAmount.Replace("$",""));
				else if(sRewardAmount.Contains("atr")) {pRewardDollars = 0; RewardATRs=Convert.ToDouble(sRewardAmount.Replace("atr",""));}
				else if(sRewardAmount.Contains("t")) pRewardDollars = Convert.ToDouble(sRewardAmount.Replace("t",""))*TickSize*PV;
				else if(sRewardAmount.Contains("p")) pRewardDollars = Convert.ToDouble(sRewardAmount.Replace("p",""))*PV;

				RiskTicks = Convert.ToInt32(pRiskDollars/PV/TickSize);
				RewardTicks = Convert.ToInt32(pRewardDollars/PV/TickSize);

				lastHighCache	= new ArrayList();
				lastLowCache	= new ArrayList();
				ClearOutputWindow();
				swingHighSeries = new Series<double>(this);
				swingHighSwings = new Series<double>(this);
				swingLowSeries	= new Series<double>(this);
				swingLowSwings	= new Series<double>(this);
				tm = new TradeManager(Instrument, sDaysOfWeek, pStartTime, pStopTime, pGoFlatTime);
			}
		}

		List<double> TheHighs = new List<double>();
		List<double> TheLows = new List<double>();
		bool c0 = true;
		bool c1 = true;
		bool c2 = true;
		bool c3 = true;
		bool c4 = true;
		bool c5 = true;
		bool cA = true;
		bool cB = true;
		double[] CurrentL = new double[3]{double.MinValue,double.MinValue,double.MinValue};
		double[] CurrentS = new double[3]{double.MinValue,double.MinValue,double.MinValue};
		int Position = 0;
		int LONG = 1;
		int SHORT = -1;
		#region -- TradeManager --
		//==============================================
		private class TradeManager : IndicatorBase{
			public Instrument instrument;
			public int StartTOD = 0;
			public int StopTOD = 0;
			public int GoFlatTOD = 0;
			public int AlertBar = 0;
			public TradeManager(Instrument inst, string sDaysOfWeek, int startAt, int stopAt, int goFlatAt){
				instrument = inst;
				StartTOD = startAt;
				StopTOD = stopAt;
				GoFlatTOD = goFlatAt;
				var sU = sDaysOfWeek.ToUpper();
				if(sU.Contains("M")  || sU.Contains("ALL")) DOW.Add(DayOfWeek.Monday);
				if(sU.Contains("TU") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Tuesday);
				if(sU.Contains("W")  || sU.Contains("ALL")) DOW.Add(DayOfWeek.Wednesday);
				if(sU.Contains("TH") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Thursday);
				if(sU.Contains("F")  || sU.Contains("ALL")) DOW.Add(DayOfWeek.Friday);
				if(sU.Contains("SA") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Saturday);
				if(sU.Contains("SU") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Sunday);
			}
			private List<DayOfWeek> DOW = new List<DayOfWeek>();
			public DateTime DTofLastLong = DateTime.MinValue;
			public DateTime DTofLastShort = DateTime.MinValue;
			public class info{
				public char Pos = ' ';
				public double EntryPrice = 0;
				public double ExitPrice  = 0;
				public double SL = 0;
				public double TP = 0;
				public double PnLPoints   = double.MinValue;
				public DateTime EntryDT   = DateTime.MinValue;
				public DateTime ExitDT    = DateTime.MinValue;
				public string Disposition = "";
				public info (char pos, double ep, DateTime et, double sl, double tp){
					Pos = pos;
					EntryPrice = ep;
					EntryDT = et;
					SL = sl;
					TP = tp;
					//NinjaTrader.Code.Output.Process(string.Format("{0} - {1}  e{2}  sl{3}  tp{4}",et.ToString(), Pos, ep, SL, TP), PrintTo.OutputTab1);
				}
			}
			public bool ResultsPrinted = false;
			public List<info> Trades = new List<info>();
			public void ExitforEOD(DateTime t0, DateTime t1, double ClosePrice){
				if(Trades.Count==0) return;
				var tt0 = ToTime(t0)/100;
				var tt1 = ToTime(t1)/100;
				if(tt1 < GoFlatTOD && tt0 >= GoFlatTOD || t0.Day!=t1.Day){
					foreach(var trade in Trades.Where(k=>k.PnLPoints==double.MinValue)){
						trade.ExitPrice = ClosePrice;
						trade.ExitDT = t0;
						trade.Disposition = "EOD";
						if(trade.Pos == 'L')      trade.PnLPoints = trade.ExitPrice - trade.EntryPrice;
						else if(trade.Pos == 'S') trade.PnLPoints = trade.EntryPrice - trade.ExitPrice;
						montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints);
					}
				}
			}
			public void ExitforSLTP(DateTime t0, double H, double L){
				if(Trades.Count==0) return;
				foreach(var trade in Trades.Where(k=>k.PnLPoints==double.MinValue)){
					if(trade.Pos == 'L'){
						if(H > trade.TP){
							trade.ExitPrice = trade.TP;
							trade.PnLPoints = trade.ExitPrice - trade.EntryPrice;
							trade.ExitDT = t0;
							trade.Disposition = "TP";
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints);
						}
						else if(L < trade.SL){
							trade.ExitPrice = trade.SL;
							trade.PnLPoints = trade.ExitPrice - trade.EntryPrice;
							trade.ExitDT = t0;
							trade.Disposition = "SL";
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints);
						}
					}
					else if(trade.Pos == 'S'){
						if(L < trade.TP){
							trade.ExitPrice = trade.TP;
							trade.PnLPoints = trade.EntryPrice - trade.ExitPrice;
							trade.ExitDT = t0;
							trade.Disposition = "TP";
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints);
						}
						else if(H > trade.SL){
							trade.ExitPrice = trade.SL;
							trade.PnLPoints = trade.EntryPrice - trade.ExitPrice;
							trade.ExitDT = t0;
							trade.Disposition = "SL";
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints);
						}
					}
				}
			}

			private MonteCarloEngine montecarlo = new MonteCarloEngine();
			private class MonteCarloEngine{
				private SortedDictionary<int, List<double>> PnLByTOD = new SortedDictionary<int, List<double>>();//TOD is time of day in 30-minute sections
				public void AddToTODList(DateTime tod, double pnl, bool InZone=false){
					int t = 0;
					if(tod.Minute<30) t = tod.Hour*100;
					else t = tod.Hour*100+30;
//NinjaTrader.Code.Output.Process(string.Format("{0} added TODList {1} ", t, pnl), PrintTo.OutputTab1);
					if(!PnLByTOD.ContainsKey(t)) PnLByTOD[t] = new List<double>(){pnl};
					else PnLByTOD[t].Add(pnl);
					if(InZone) NinjaTrader.Code.Output.Process(string.Format("{0} added TODList  {1}  {2}", tod.ToString(), t, pnl), PrintTo.OutputTab1);
				}
				public void DoMonteCarlo(int BatchesCount, double PctOfTradesPerBatch, double PV){
					var distribution = new List<double>();
					var r = new Random();
		
					var pnl_table = new List<double>();
					var list = PnLByTOD.SelectMany(k=> k.Value).ToList();
					int samples = Convert.ToInt32(list.Count * PctOfTradesPerBatch);
					for(int batch = 0; batch< BatchesCount; batch++){
						var randoms = Enumerable.Range(0, list.Count-1).OrderBy(_ => r.Next()).Take(samples).ToList();//guarantees unique random numbers in this list
						pnl_table.Clear();//calc the PnL for each batch of samples...100 batches
						for(int i = 0;i<randoms.Count; i++){//number of samples is list.Count/2
							int idx = Convert.ToInt32(randoms[i]);
							pnl_table.Add(list[idx]);

//	NinjaTrader.Code.Output.Process("***************************   List["+idx+"] was "+list[idx], PrintTo.OutputTab1);
						}
						distribution.Add(pnl_table.Sum());
					}
					var winning_iterations = distribution.Count(k=>k>0);
					NinjaTrader.Code.Output.Process("Monte Carlo("+BatchesCount+"-sims, list#: "+list.Count+"  "+samples+"-samples/sim):  You have a "+(winning_iterations*1.0/distribution.Count).ToString("0%")+ " of a positive balance", PrintTo.OutputTab1);
					NinjaTrader.Code.Output.Process("Worst Monte Carlo balance:  "+(distribution.Min()*PV).ToString("C")+"   Best: "+(distribution.Max()*PV).ToString("C")+"  Avg: "+distribution.Average().ToString("C"), PrintTo.OutputTab1);
				}
			}
			#region -- supporting methods --
//			private void Print(string s){
//				NinjaTrader.Code.Output.Process(s, PrintTo.OutputTab1);
//			}
			private string ToCurrency(double c){
				var s = c.ToString("C");
				return s.Replace(".00","");
			}
			#endregion
			public bool IsValidTimeAndDay(char Dir, DateTime t0, DateTime t1){
				if(StartTOD!=StopTOD){
					int tt0 = ToTime(t0)/100;
					int tt1 = ToTime(t1)/100;
					if(StartTOD < StopTOD){
						if(tt0 < StartTOD || tt0 >= StopTOD) return false;
					}else{
						if(tt0 < StartTOD && tt0 >= StopTOD) return false;
					}
				}
				if(Dir=='L'){
					if(DTofLastLong.Day == t0.Day) return false;
					return DOW.Contains(t0.DayOfWeek) && DTofLastLong.Day!= t0.Day;
				}else{
					if(DTofLastShort.Day == t0.Day) return false;
					return DOW.Contains(t0.DayOfWeek) && DTofLastShort.Day!= t0.Day;
				}
			}
			public void PrintResults(int BarsCount, int CurrentBar, NinjaTrader.NinjaScript.IndicatorBase context){
				#region -- PrintResults --
				if(CurrentBar > BarsCount-3 && !this.ResultsPrinted){
					this.ResultsPrinted = true;
					double pnl = 0;
					double wins = 0;
					double losses = 0;
					int TPhit = 0;
					int SLhit = 0;
					var TradeTimes    = new SortedDictionary<int,       List<double>>();//'L'/'S' and pnl points
					var DatesOfTrades = new SortedDictionary<DateTime,  List<double>>();
					var DOWOfTrades   = new SortedDictionary<DayOfWeek, List<double>>();
	
					foreach(var t in Trades){
						if(t.ExitDT == DateTime.MinValue) continue;
						pnl = pnl + t.PnLPoints;
						if(t.PnLPoints>0)
							wins++;
						else
							losses++;
						int hr = t.EntryDT.Hour*100 + (t.EntryDT.Minute<30 ? 0:30);
						if(!TradeTimes.ContainsKey(hr)) TradeTimes[hr] = new List<double>(){t.PnLPoints};
						else TradeTimes[hr].Add(t.PnLPoints);
						if(!DatesOfTrades.ContainsKey(t.EntryDT.Date)) DatesOfTrades[t.EntryDT.Date] = new List<double>(){t.PnLPoints};
						else DatesOfTrades[t.EntryDT.Date].Add(t.PnLPoints);
						if(!DOWOfTrades.ContainsKey(t.EntryDT.DayOfWeek)) DOWOfTrades[t.EntryDT.DayOfWeek] = new List<double>(){t.PnLPoints};
						else DOWOfTrades[t.EntryDT.DayOfWeek].Add(t.PnLPoints);
	
						if(true){
							Draw.Line(context, t.EntryDT.ToString(), false, t.EntryDT, t.EntryPrice, t.ExitDT, t.ExitPrice, (t.PnLPoints>0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash, 2);
						}
						if(t.ExitPrice == t.TP) TPhit++;
						if(t.ExitPrice == t.SL) SLhit++;
						Print(t.EntryDT.ToString()+"   "+t.Pos+":  "+(t.PnLPoints>0?"+":"")+instrument.MasterInstrument.FormatPrice(t.PnLPoints)+" pts,  total: "+instrument.MasterInstrument.FormatPrice(pnl)+"    "+wins+"/"+losses+"   Disosition: "+t.Disposition);
					}
					double PV = instrument.MasterInstrument.PointValue;
					Print("\nTotal "+ToCurrency(pnl*PV)+" on "+Trades.Count+"-trades  Avg trade: "+ToCurrency(PV*pnl/Trades.Count));
					Print(wins+"-Wins   "+(wins / Trades.Count).ToString("0% wins"));
					Print("\nHr of day");
					foreach(var tt in TradeTimes){
						Print(tt.Key+":   "+ToCurrency(tt.Value.Sum()*PV)+"  "+(tt.Value.Count(k=>k>0)*1.0 / tt.Value.Count).ToString("0%")+"-win  on "+tt.Value.Count+"-trades");
					}
	//				Print("\nBy Date");
	//				foreach(var tt in DatesOfTrades){
	//					Print(tt.Key.ToShortDateString()+"   PnL: "+ToCurrency(tt.Value.Sum()*PV)+"   "+(tt.Value.Count(k=>k>0)*1.0/tt.Value.Count).ToString("0%"));
	//				}
					Print("\nAvg $/day: "+ToCurrency(pnl*PV/DatesOfTrades.Count));
					Print("\nAvg Trades/day: "+(Trades.Count*1.0/DatesOfTrades.Count).ToString("0.0"));
					Print("\nBy Day of week");
					foreach(var tt in DOWOfTrades){
						Print(tt.Key.ToString().Substring(0,3)+"   PnL: "+ToCurrency(tt.Value.Sum()*PV)+"   "+(tt.Value.Count(k=>k>0)*1.0/tt.Value.Count).ToString("0%")+" ("+tt.Value.Count(k=>k>0)+"/"+tt.Value.Count+")");
					}

					montecarlo.DoMonteCarlo(100, 0.5, PV);
				}
				#endregion
			}
		}
		private TradeManager tm;
		//==============================================
		#endregion
		double TP = 0;
		double SL = 0;
		int ShortID = 0;
		int LongID = 0;
		int LastShortID = -1;
		int LastLongID = -1;
		double PV = 0;
		protected override void OnBarUpdate()
		{
			#region --- Swings ---
			double high0	= !(Input is PriceSeries || Input is Bars) ? Input[0] : High[0];
			double low0		= !(Input is PriceSeries || Input is Bars) ? Input[0] : Low[0];
			double close0	= !(Input is PriceSeries || Input is Bars) ? Input[0] : Close[0];

			if (BarsArray[0].BarsType.IsRemoveLastBarSupported && CurrentBar < saveCurrentBar)
			{
				currentSwingHigh			= HighDots.IsValidDataPoint(0) ? HighDots[0] : 0;
				currentSwingLow				= LowDots.IsValidDataPoint(0) ? LowDots[0] : 0;
				lastSwingHighValue			= swingHighSeries[0];
				lastSwingLowValue			= swingLowSeries[0];
				swingHighSeries[pStrength]	= 0;
				swingLowSeries[pStrength]	= 0;

				lastHighCache.Clear();
				lastLowCache.Clear();
				for (int barsBack = Math.Min(CurrentBar, constant) - 1; barsBack >= 0; barsBack--)
				{
					lastHighCache.Add(!(Input is PriceSeries || Input is Bars) ? Input[barsBack] : High[barsBack]);
					lastLowCache.Add(!(Input is PriceSeries || Input is Bars) ? Input[barsBack] : Low[barsBack]);
				}
				saveCurrentBar = CurrentBar;
				return;
			}

			if (saveCurrentBar != CurrentBar)
			{
				swingHighSwings[0]	= 0;	// initializing important internal
				swingLowSwings[0]	= 0;	// initializing important internal

				swingHighSeries[0]	= 0;	// initializing important internal
				swingLowSeries[0]	= 0;	// initializing important internal

				lastHighCache.Add(high0);
				if (lastHighCache.Count > constant)
					lastHighCache.RemoveAt(0);
				lastLowCache.Add(low0);
				if (lastLowCache.Count > constant)
					lastLowCache.RemoveAt(0);

				if (lastHighCache.Count == constant)
				{
					bool isSwingHigh = true;
					double swingHighCandidateValue = (double) lastHighCache[pStrength];
					for (int i=0; i < pStrength; i++)
						if (((double) lastHighCache[i]).ApproxCompare(swingHighCandidateValue) >= 0)
							isSwingHigh = false;

					for (int i=pStrength+1; i < lastHighCache.Count; i++)
						if (((double) lastHighCache[i]).ApproxCompare(swingHighCandidateValue) > 0)
							isSwingHigh = false;

					swingHighSwings[pStrength] = isSwingHigh ? swingHighCandidateValue : 0.0;
					if (isSwingHigh)
						lastSwingHighValue = swingHighCandidateValue;

					if (isSwingHigh)
					{
						currentSwingHigh = swingHighCandidateValue;
//						for (int i=0; i <= pStrength; i++)
//							HighDots[i] = currentSwingHigh;
					}
					else if (high0 > currentSwingHigh || currentSwingHigh.ApproxCompare(0.0) == 0)
					{
						currentSwingHigh = 0.0;
						HighDots[0] = close0;
						HighDots.Reset();
					}
					else
						HighDots[0] = currentSwingHigh;

					if (isSwingHigh)
					{
						for (int i=0; i<=pStrength; i++)
							swingHighSeries[i] = lastSwingHighValue;
					}
					else
					{
						swingHighSeries[0] = lastSwingHighValue;
					}
				}

				if (lastLowCache.Count == constant)
				{
					bool isSwingLow = true;
					double swingLowCandidateValue = (double) lastLowCache[pStrength];
					for (int i=0; i < pStrength; i++)
						if (((double) lastLowCache[i]).ApproxCompare(swingLowCandidateValue) <= 0)
							isSwingLow = false;

					for (int i=pStrength+1; i < lastLowCache.Count; i++)
						if (((double) lastLowCache[i]).ApproxCompare(swingLowCandidateValue) < 0)
							isSwingLow = false;

					swingLowSwings[pStrength] = isSwingLow ? swingLowCandidateValue : 0.0;
					if (isSwingLow)
						lastSwingLowValue = swingLowCandidateValue;

					if (isSwingLow)
					{
						currentSwingLow = swingLowCandidateValue;
//						for (int i=0; i <= pStrength; i++)
//							LowDots[i] = currentSwingLow;
					}
					else if (low0 < currentSwingLow || currentSwingLow.ApproxCompare(0.0) == 0)
					{
						currentSwingLow = double.MaxValue;
						LowDots[0] = close0;
						LowDots.Reset();
					}
					else
						LowDots[0] = currentSwingLow;

					if (isSwingLow)
					{
						for (int i=0; i<=pStrength; i++)
							swingLowSeries[i] = lastSwingLowValue;
					}
					else
					{
						swingLowSeries[0] = lastSwingLowValue;
					}
				}
				saveCurrentBar = CurrentBar;
			}
			else if (CurrentBar >= constant - 1)
			{
				if (lastHighCache.Count == constant && high0.ApproxCompare((double) lastHighCache[lastHighCache.Count - 1]) > 0)
					lastHighCache[lastHighCache.Count - 1] = high0;
				if (lastLowCache.Count == constant && low0.ApproxCompare((double) lastLowCache[lastLowCache.Count - 1]) < 0)
					lastLowCache[lastLowCache.Count - 1] = low0;

				if (high0 > currentSwingHigh && swingHighSwings[pStrength] > 0.0)
				{
					swingHighSwings[pStrength] = 0.0;
					for (int i = 0; i <= pStrength; i++)
					{
						HighDots[i] = close0;
						HighDots.Reset(i);
						currentSwingHigh = 0.0;
					}
				}
				else if (high0 > currentSwingHigh && currentSwingHigh.ApproxCompare(0.0) != 0)
				{
					HighDots[0] = close0;
					HighDots.Reset();
					currentSwingHigh = 0.0;
				}
				else if (high0 <= currentSwingHigh)
					HighDots[0] = currentSwingHigh;

				if (low0 < currentSwingLow && swingLowSwings[pStrength] > 0.0)
				{
					swingLowSwings[pStrength] = 0.0;
					for (int i = 0; i <= pStrength; i++)
					{
						LowDots[i] = close0;
						LowDots.Reset(i);
						currentSwingLow = double.MaxValue;
					}
				}
				else if (low0 < currentSwingLow && currentSwingLow.ApproxCompare(double.MaxValue) != 0)
				{
					LowDots.Reset();
					currentSwingLow = double.MaxValue;
				}
				else if (low0 >= currentSwingLow)
					LowDots[0] = currentSwingLow;
			}
			#endregion
//bool z = Times[0][0].Day==5 && Times[0][0].Hour==14 && Times[0][0].Month==4 && (Times[0][0].Minute>=45 && Times[0][0].Minute<=58);
			if(TheHighs.Count==0) TheHighs.Add(currentSwingHigh);
			else if(HighDots.IsValidDataPoint(0) && TheHighs[0]!= HighDots[0]) TheHighs.Insert(0, HighDots[0]);
			else if(!HighDots.IsValidDataPoint(0) && HighDots.IsValidDataPoint(1) && TheHighs[0]!= HighDots[1])	TheHighs.Insert(0, HighDots[1]);

			if(TheLows.Count==0) TheLows.Add(currentSwingLow);
			else if(LowDots.IsValidDataPoint(0) && TheLows[0]!= LowDots[0]) TheLows.Insert(0, LowDots[0]);
			else if(!LowDots.IsValidDataPoint(0) && LowDots.IsValidDataPoint(1) && TheLows[0]!= LowDots[1])	TheLows.Insert(0, LowDots[1]);

			int TT = ToTime(Times[0][0])/100;
			if(pStartTime==pStopTime)
				c4 = true;
			else{
				c4 = TT >= pStartTime && TT < pStopTime;
				if(!c4) BackBrushes[0] = Brushes.Maroon;
			}
			if(TheHighs.Count>3){
				c0 = TheHighs[0] < TheHighs[1];
				if(pConsecLevels>1)
					c1 = TheHighs[1] < TheHighs[2];
				if(pConsecLevels>2)
					c2 = TheHighs[2] < TheHighs[3];
				c3 = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1]);
				c5 = sDirection.Contains("L");
				if(c0 && c1 && c2 && c3 && c4 && c5){
					BuyTriangles[0] = TheHighs[1];
				}
			}
			if(TheLows.Count>3){
				c0 = TheLows[0] > TheLows[1];
				if(pConsecLevels>1)
					c1 = TheLows[1] > TheLows[2];
				if(pConsecLevels>2)
					c2 = TheLows[2] > TheLows[3];
				c3 = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1]);
				c5 = sDirection.Contains("S");
				if(c0 && c1 && c2 && c3 && c4 && c5){
					SellTriangles[0] = TheLows[1];
				}
			}
			if(CurrentBar>5){
				TP = 0;
				SL = 0;
				if(!BuyTriangles.IsValidDataPoint(1) && !BuyTriangles.IsValidDataPoint(0)) CurrentL = new double[3]{double.MinValue,double.MinValue,double.MinValue};
				if(!SellTriangles.IsValidDataPoint(1) && !SellTriangles.IsValidDataPoint(0)) CurrentS = new double[3]{double.MinValue,double.MinValue,double.MinValue};

				cA = !BuyTriangles.IsValidDataPoint(1) && BuyTriangles.IsValidDataPoint(0);
				cB = BuyTriangles.IsValidDataPoint(1) && BuyTriangles.IsValidDataPoint(0) && BuyTriangles[1]>BuyTriangles[0];
				if(cA || cB){
					if(LongID != CurrentBars[0]) LongID = CurrentBars[0];
				}
				if(BuyTriangles.IsValidDataPoint(1)){
					var v = BuyTriangles.GetValueAt(CurrentBars[0]-1);
					if(CurrentL[0]==double.MinValue){
						CurrentL[0] = v;
					}else if(v < CurrentL[0]){
						CurrentL[0] = v;
					}
				}
				cA = !SellTriangles.IsValidDataPoint(1) && SellTriangles.IsValidDataPoint(0);
				cB = SellTriangles.IsValidDataPoint(1) && SellTriangles.IsValidDataPoint(0) && SellTriangles[1]<SellTriangles[0];
				if(cA || cB){
					if(ShortID != CurrentBars[0]) ShortID = CurrentBars[0];
				}
				if(SellTriangles.IsValidDataPoint(1)){
					var v = SellTriangles.GetValueAt(CurrentBars[0]-1);
					if(CurrentS[0]==double.MinValue){
						CurrentS[0] = v;
					}else if(v > CurrentS[0]){
						CurrentS[0] = v;
					}
				}
//bool z = Times[0][0].Day==20 && Times[0][0].Hour==11 && Times[0][0].Minute>31 && Times[0][0].Minute<38;
				if(Highs[0][0] >= CurrentL[0] && CurrentL[0] != double.MinValue && LastLongID!=LongID) {
					LastLongID = LongID;
					if(RiskATRs>0) RiskTicks = Convert.ToInt32(atr(14) * RiskATRs/TickSize);
					if(RewardATRs>0) RewardTicks = Convert.ToInt32(atr(14) * RewardATRs/TickSize);
					SL = CurrentL[0] - RiskTicks*TickSize;
					TP = CurrentL[0] + RewardTicks*TickSize;
					Draw.Square(this,string.Format("longStop ${0} {1}", PV*RiskTicks*TickSize, CurrentBar), false,0, SL, Brushes.Red);
					Draw.Dot(this,string.Format("longTP ${0} {1}", PV*RewardTicks*TickSize, CurrentBar), false,0, TP, Brushes.Lime);
					CurrentL[0] = double.MinValue;
					if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar!=CurrentBar){
						Alert(DateTime.Now.ToString(), Priority.Medium, "SwingAlert Buy level hit at "+Instrument.MasterInstrument.FormatPrice(CurrentL[0]), AddSoundFolder(pWAVOnBuyEntry), 1, Brushes.Lime,Brushes.White);
						tm.AlertBar = CurrentBars[0];
					}
//					tm.DTofLastLong = Times[0][0];//one long trade per day
					tm.Trades.Add(new TradeManager.info('L', CurrentL[0], Times[0][0], SL, TP));
					BackBrushes[0] = Brushes.Cyan;
				}
				if(Lows[0][0] <= CurrentS[0] && CurrentS[0] != double.MinValue && LastShortID!=ShortID) {
					LastShortID = ShortID;
					if(RiskATRs>0) RiskTicks = Convert.ToInt32(atr(14) * RiskATRs/TickSize);
					if(RewardATRs>0) RewardTicks = Convert.ToInt32(atr(14) * RewardATRs/TickSize);
					SL = CurrentS[0] + RiskTicks*TickSize;
					TP = CurrentS[0] - RewardTicks*TickSize;
					Draw.Square(this,string.Format("shortStop ${0} {1}", PV*RiskTicks*TickSize, CurrentBar), false,0, SL, Brushes.Red);
					Draw.Dot(this,string.Format("shortTP ${0} {1}", PV*RewardTicks*TickSize, CurrentBar), false,0, TP, Brushes.Lime);
					CurrentS[0] = double.MinValue;
					if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar!=CurrentBar){
						Alert(DateTime.Now.ToString(), Priority.Medium, "SwingAlert Sell level hit at "+Instrument.MasterInstrument.FormatPrice(CurrentS[0]), AddSoundFolder(pWAVOnSellEntry), 1, Brushes.Magenta,Brushes.White);
						tm.AlertBar = CurrentBars[0];
					}
//					tm.DTofLastShort = Times[0][0];//one short trade per day
					tm.Trades.Add(new TradeManager.info('S', CurrentS[0], Times[0][0], SL, TP));
					BackBrushes[0] = Brushes.Magenta;
				}
				tm.ExitforEOD(Times[0][0], Times[0][1], Closes[0][1]);
				tm.ExitforSLTP(Times[0][0], Highs[0][0], Lows[0][0]);
				tm.PrintResults(Bars.Count, CurrentBars[0], this);
			}

		}
		private double atr(int period){
			int i = 1;
			double sum = 0;
			while(i<=period && i < CurrentBars[0]+1){
				sum = sum + Highs[0][i]-Lows[0][i];
				i++;
			}
			return sum/period;
		}
		#region Functions
		/// <summary>
		/// Returns the number of bars ago a swing low occurred. Returns a value of -1 if a swing low is not found within the look back period.
		/// </summary>
		/// <param name="barsAgo"></param>
		/// <param name="instance"></param>
		/// <param name="lookBackPeriod"></param>
		/// <returns></returns>
		public int SwingAlertLowBar(int barsAgo, int instance, int lookBackPeriod)
		{
			if (instance < 1)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.SwingSwingLowBarInstanceGreaterEqual, GetType().Name, instance));
			if (barsAgo < 0)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.SwingSwingLowBarBarsAgoGreaterEqual, GetType().Name, barsAgo));
			if (barsAgo >= Count)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.SwingSwingLowBarBarsAgoOutOfRange, GetType().Name, (Count - 1), barsAgo));

			Update();

			for (int idx=CurrentBar - barsAgo - pStrength; idx >= CurrentBar - barsAgo - pStrength - lookBackPeriod; idx--)
			{
				if (idx < 0)
					return -1;
				if (idx >= swingLowSwings.Count)
					continue;

				if (swingLowSwings.GetValueAt(idx).Equals(0.0))
					continue;

				if (instance == 1) // 1-based, < to be save
					return CurrentBar - idx;

				instance--;
			}

			return -1;
		}

		/// <summary>
		/// Returns the number of bars ago a swing high occurred. Returns a value of -1 if a swing high is not found within the look back period.
		/// </summary>
		/// <param name="barsAgo"></param>
		/// <param name="instance"></param>
		/// <param name="lookBackPeriod"></param>
		/// <returns></returns>
		public int SwingAlertHighBar(int barsAgo, int instance, int lookBackPeriod)
		{
			if (instance < 1)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.SwingSwingHighBarInstanceGreaterEqual, GetType().Name, instance));
			if (barsAgo < 0)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.SwingSwingHighBarBarsAgoGreaterEqual, GetType().Name, barsAgo));
			if (barsAgo >= Count)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.SwingSwingHighBarBarsAgoOutOfRange, GetType().Name, (Count - 1), barsAgo));

			Update();

			for (int idx=CurrentBar - barsAgo - pStrength; idx >= CurrentBar - barsAgo - pStrength - lookBackPeriod; idx--)
			{
				if (idx < 0)
					return -1;
				if (idx >= swingHighSwings.Count)
					continue;

				if (swingHighSwings.GetValueAt(idx).Equals(0.0))
					continue;

				if (instance <= 1) // 1-based, < to be save
					return CurrentBar - idx;

				instance--;
			}

			return -1;
		}
		#endregion

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Swing Strength", GroupName = "NinjaScriptParameters", Order = 10)]
		public int pStrength
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Risk $", GroupName = "NinjaScriptParameters", Order = 20)]
		public string sRiskAmount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Reward $", GroupName = "NinjaScriptParameters", Order = 30)]
		public string sRewardAmount
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1,3)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Consecutive Levels", GroupName = "NinjaScriptParameters", Order = 40)]
		public int pConsecLevels
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Days of week", GroupName = "NinjaScriptParameters", Order = 50)]
		public string sDaysOfWeek
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Direction", GroupName = "Strategy", Order = 10, ResourceType = typeof(Custom.Resource))]
		public string sDirection
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Start time", GroupName = "Strategy", Order = 20, ResourceType = typeof(Custom.Resource))]
		public int pStartTime
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name = "Stop time", GroupName = "Strategy", Order = 30, ResourceType = typeof(Custom.Resource))]
		public int pStopTime
		{ get; set; }
		[Display(Order = 40, Name="Trade Exit time", GroupName="Strategy",  Description="All open trades are flattened at this time", ResourceType = typeof(Custom.Resource))]
		public int pGoFlatTime
		{get;set;}
		#endregion

		#region -- Alerts --
		private string AddSoundFolder(string wav){
			wav = wav.Replace("<inst>",Instrument.MasterInstrument.Name);
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
			if(!System.IO.File.Exists(wav)) {
				Log("SwingAlert could not find wav: "+wav,LogLevel.Information);
				return "";
			}else
				return wav;
		}
		internal class LoadSoundFileList : StringConverter
		{
			#region LoadSoundFileList
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				//true means show a combobox
				return true;
			}

			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
				//true will limit to list. false will show the list, 
				//but allow free-form entry
				return false;
			}

			public override System.ComponentModel.TypeConverter.StandardValuesCollection
				GetStandardValues(ITypeDescriptorContext context)
			{
				string dir = NinjaTrader.Core.Globals.InstallDir;
//				string dir = WAVDirectory;
//				if(dir.Trim().Length==0) dir = NinjaTrader.Core.Globals.InstallDir;
//				if(dir.ToLower().Contains("<default>")) dir = NinjaTrader.Core.Globals.InstallDir;
				string folder = System.IO.Path.Combine(dir, "sounds");
				string search = "*.wav";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						if(!list.Contains(fi.Name)){
							list.Add(fi.Name);
						}
					}
				}
				return new StandardValuesCollection(list.ToArray());
			}
			#endregion
        }

		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Enable sound alerts?", GroupName = "Alerts")]
		public bool pEnableSoundAlerts {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Wav on Buy Entry", GroupName = "Alerts")]
		public string pWAVOnBuyEntry {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Wav on Sell Entry", GroupName = "Alerts")]
		public string pWAVOnSellEntry {get;set;}
		#endregion

		#region Plots
		/// <summary>
		/// Gets the high swings.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SwingAlertHigh
		{
			get
			{
				Update();
				return swingHighSeries;
			}
		}

		private Series<double> HighDots
		{
			get
			{
				Update();
				return Values[0];
			}
		}
		private Series<double> BuyTriangles
		{
			get
			{
				Update();
				return Values[2];
			}
		}

		/// <summary>
		/// Gets the low swings.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SwingAlertLow
		{
			get
			{
				Update();
				return swingLowSeries;
			}
		}

		private Series<double> LowDots
		{
			get
			{
				Update();
				return Values[1];
			}
		}
		private Series<double> SellTriangles
		{
			get
			{
				Update();
				return Values[3];
			}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SwingAlert[] cacheSwingAlert;
		public SwingAlert SwingAlert(int pStrength, string sRiskAmount, string sRewardAmount, int pConsecLevels, string sDaysOfWeek, string sDirection, int pStartTime, int pStopTime)
		{
			return SwingAlert(Input, pStrength, sRiskAmount, sRewardAmount, pConsecLevels, sDaysOfWeek, sDirection, pStartTime, pStopTime);
		}

		public SwingAlert SwingAlert(ISeries<double> input, int pStrength, string sRiskAmount, string sRewardAmount, int pConsecLevels, string sDaysOfWeek, string sDirection, int pStartTime, int pStopTime)
		{
			if (cacheSwingAlert != null)
				for (int idx = 0; idx < cacheSwingAlert.Length; idx++)
					if (cacheSwingAlert[idx] != null && cacheSwingAlert[idx].pStrength == pStrength && cacheSwingAlert[idx].sRiskAmount == sRiskAmount && cacheSwingAlert[idx].sRewardAmount == sRewardAmount && cacheSwingAlert[idx].pConsecLevels == pConsecLevels && cacheSwingAlert[idx].sDaysOfWeek == sDaysOfWeek && cacheSwingAlert[idx].sDirection == sDirection && cacheSwingAlert[idx].pStartTime == pStartTime && cacheSwingAlert[idx].pStopTime == pStopTime && cacheSwingAlert[idx].EqualsInput(input))
						return cacheSwingAlert[idx];
			return CacheIndicator<SwingAlert>(new SwingAlert(){ pStrength = pStrength, sRiskAmount = sRiskAmount, sRewardAmount = sRewardAmount, pConsecLevels = pConsecLevels, sDaysOfWeek = sDaysOfWeek, sDirection = sDirection, pStartTime = pStartTime, pStopTime = pStopTime }, input, ref cacheSwingAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SwingAlert SwingAlert(int pStrength, string sRiskAmount, string sRewardAmount, int pConsecLevels, string sDaysOfWeek, string sDirection, int pStartTime, int pStopTime)
		{
			return indicator.SwingAlert(Input, pStrength, sRiskAmount, sRewardAmount, pConsecLevels, sDaysOfWeek, sDirection, pStartTime, pStopTime);
		}

		public Indicators.SwingAlert SwingAlert(ISeries<double> input , int pStrength, string sRiskAmount, string sRewardAmount, int pConsecLevels, string sDaysOfWeek, string sDirection, int pStartTime, int pStopTime)
		{
			return indicator.SwingAlert(input, pStrength, sRiskAmount, sRewardAmount, pConsecLevels, sDaysOfWeek, sDirection, pStartTime, pStopTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SwingAlert SwingAlert(int pStrength, string sRiskAmount, string sRewardAmount, int pConsecLevels, string sDaysOfWeek, string sDirection, int pStartTime, int pStopTime)
		{
			return indicator.SwingAlert(Input, pStrength, sRiskAmount, sRewardAmount, pConsecLevels, sDaysOfWeek, sDirection, pStartTime, pStopTime);
		}

		public Indicators.SwingAlert SwingAlert(ISeries<double> input , int pStrength, string sRiskAmount, string sRewardAmount, int pConsecLevels, string sDaysOfWeek, string sDirection, int pStartTime, int pStopTime)
		{
			return indicator.SwingAlert(input, pStrength, sRiskAmount, sRewardAmount, pConsecLevels, sDaysOfWeek, sDirection, pStartTime, pStopTime);
		}
	}
}

#endregion
