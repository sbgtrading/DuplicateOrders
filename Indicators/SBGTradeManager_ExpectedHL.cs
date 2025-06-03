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
namespace SBG_ExpectedHL
{
		#region -- TradeManager --
		//==============================================
		public class TradeManager
		{
			private Instrument instrument;
			public int AlertBar   = 0;
			public List<string> OutputLS = new List<string>();
			public List<string> OutputL  = new List<string>();
			public List<string> OutputS  = new List<string>();
			public bool IsInSession        = false;
			public int FirstAbarOfSession  = 0;
			public double RealizedEquity   = 0;
			public double UnrealizedEquity = 0;
			public List<DayOfWeek> DOW = new List<DayOfWeek>();
			public bool WarnIfWrongDayOfWeek = true;
			public DateTime DTofLastLong = DateTime.MinValue;
			public DateTime DTofLastShort = DateTime.MinValue;
			public SortedDictionary<int, List<double>> TradeTimesLS = new SortedDictionary<int, List<double>>(); //this contains the PnL for each timeslice, can be used on chart to assign a quality grade each 30-min time period
			public SortedDictionary<int, List<double>> TradeTimesL = new SortedDictionary<int, List<double>>(); //this contains the PnL for each timeslice, can be used on chart to assign a quality grade each 30-min time period
			public SortedDictionary<int, List<double>> TradeTimesS = new SortedDictionary<int, List<double>>(); //this contains the PnL for each timeslice, can be used on chart to assign a quality grade each 30-min time period
			public bool ResultsPrinted = false;
			public string Note = "";//temporary note added to the Disposition on an exit
			public List<info> Trades = new List<info>();
			public int CurrentPosition = 0;//Long is +1, Short is -1
			public int CurrentDay = -1;
			public bool DailyTgtAchieved = false;
			public DateTime DateTgtAchieved = DateTime.MinValue;
			public string Status = "";
			public TradeManager(IndicatorBase Indi, string strategyName, string comment, string msgLongs, string msgShorts, Instrument Inst, string DaysOfWeek, int startAt, int stopAt, int goFlatAt, bool showHrOfDayTable, int maxLongTradesPerDay, int maxShortTradesPerDay){
				StrategyName = strategyName;
				Comment = comment;
				ShowHrOfDayTable = showHrOfDayTable;
				instrument = Inst;
				indi = Indi;
				StartTOD = startAt;
				StopTOD = stopAt;
				GoFlatTOD = goFlatAt;
				MsgLongs = msgLongs.Trim();	  if(MsgLongs.Length>0) MsgLongs = " "+MsgLongs;
				MsgShorts = msgShorts.Trim(); if(MsgShorts.Length>0) MsgShorts = " "+MsgShorts;
				#region -- Set days of week --
				string sU = DaysOfWeek.ToUpper();
				if (sU.Contains("ALL")) DaysOfWeek = "All";
				else{
					if(sU.Contains("TODAY")) {
						if(DateTime.Now.DayOfWeek==DayOfWeek.Saturday) 
							sU = "M";
						else if(DateTime.Now.DayOfWeek==DayOfWeek.Sunday) 
							sU = "M";
						else
							sU = DateTime.Now.DayOfWeek.ToString().Substring(0,2);
						DaysOfWeek = "Today";
					}else{
						string s = string.Empty;
						if(sU.Contains("M")  || sU.Contains("1"))  s = s+"M ";
						if(sU.Contains("TU") || sU.Contains("2"))  s = s+"Tu ";
						if(sU.Contains("W")  || sU.Contains("3"))  s = s+"W ";
						if(sU.Contains("TH") || sU.Contains("4"))  s = s+"Th ";
						if(sU.Contains("F")  || sU.Contains("5"))  s = s+"F ";
						if(sU.Contains("SA") || sU.Contains("6"))  s = s+"Sa ";
						if(sU.Contains("SU") || sU.Contains("0"))  s = s+"Su ";
						DaysOfWeek = s.Trim();
					}
				}
				sU = DaysOfWeek.ToUpper();
				if(sU=="TODAY") DOW.Add(DateTime.Now.DayOfWeek);
				else{
					if(sU.Contains("M")  || sU.Contains("ALL")) DOW.Add(DayOfWeek.Monday);
					if(sU.Contains("TU") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Tuesday);
					if(sU.Contains("W")  || sU.Contains("ALL")) DOW.Add(DayOfWeek.Wednesday);
					if(sU.Contains("TH") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Thursday);
					if(sU.Contains("F")  || sU.Contains("ALL")) DOW.Add(DayOfWeek.Friday);
					if(sU.Contains("SA") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Saturday);
					if(sU.Contains("SU") || sU.Contains("ALL")) DOW.Add(DayOfWeek.Sunday);
				}
				#endregion

				MaxLongTradesPerDay = maxLongTradesPerDay;
				MaxShortTradesPerDay = maxShortTradesPerDay;
			}
			private int MaxLongTradesPerDay = 1;
			private int MaxShortTradesPerDay = 1;
			private string MsgLongs = "";
			private string MsgShorts = "";
			private int StartTOD  = 0;
			private int StopTOD   = 0;
			private int GoFlatTOD = 0;
			private IndicatorBase indi;
			private string StrategyName  = "";
			private string Comment         = "";
			private bool ShowHrOfDayTable  = false;
			private bool ShowDOWTable      = true;
			public class SLTPinfo{
				public double ATRmultSL = 0;
				public double DollarsSL = 0;
				public string SLBasisStr;
				public double ATRmultTarget = 2.5;
				public double DollarsTarget = 200;
				public string TargetBasisStr;
				public string ATMname;
				public SLTPinfo (string sLDistStr, string tgtDistStr, string aTMname){
					SLBasisStr = sLDistStr;
					TargetBasisStr = tgtDistStr;
					ATMname = aTMname;
				}
				public string ToString(){
					return $"'{SLBasisStr}': ATR multSL: {ATRmultSL}  DollarSL: {DollarsSL}   '{TargetBasisStr}':  ATRmultTgt: {ATRmultTarget}  DollarsTgt: {DollarsTarget}";
				}
			}
			public Dictionary<string, SLTPinfo> SLTPs = new Dictionary<string, SLTPinfo>();
			public class info{
				public char Pos = ' ';
				public double Qty = 0;
				public double EntryPrice = 0;
				public double ExitPrice  = 0;
				public double SL = 0;
				public double TP = 0;
				public double MaxPriceHigh = 0;
				public double MinPriceLow  = 0;
				public double PnLPoints    = double.MinValue;
				public DateTime EntryDT    = DateTime.MinValue;
				public DateTime ExitDT     = DateTime.MinValue;
				public string Disposition  = "";
				public info (char pos, double qty, double ep, DateTime et, double sl, double tp){
					Pos = pos;
					Qty = qty;
					EntryPrice = ep;
					EntryDT = et;
					SL = sl;
					TP = tp;
					MaxPriceHigh = ep;
					MinPriceLow = ep;
					//NinjaTrader.Code.Output.Process(string.Format("{0} - {1}  e{2}  sl{3}  tp{4}",et.ToString(), Pos, ep, SL, TP), PrintTo.OutputTab1);
				}
				public info (char pos, double ep, DateTime et, double sl, double tp){
					Pos = pos;
					Qty = 1;
					EntryPrice = ep;
					EntryDT = et;
					SL = sl;
					TP = tp;
					MaxPriceHigh = ep;
					MinPriceLow = ep;
					//NinjaTrader.Code.Output.Process(string.Format("{0} - {1}  e{2}  sl{3}  tp{4}",et.ToString(), Pos, ep, SL, TP), PrintTo.OutputTab1);
				}
			}
			private int ToTime(DateTime dt){
				return dt.Hour*100 + dt.Minute;
			}
			public void AddTrade(char Dir, double entryPrice, DateTime entryTime, double stopLossPrice, double tgtPrice){
				Trades.Add(new info(Dir, entryPrice, entryTime, stopLossPrice, tgtPrice));
				if(Dir=='L')      {this.LongTradesToday++; CurrentPosition = 1;}
				else if(Dir=='S') {this.ShortTradesToday++; CurrentPosition = -1;}
			}
			public void AddTrade(char Dir, double qty, double entryPrice, DateTime entryTime, double stopLossPrice, double tgtPrice){
				Trades.Add(new info(Dir, qty, entryPrice, entryTime, stopLossPrice, tgtPrice));
				if(Dir=='L')      {this.LongTradesToday++; CurrentPosition = 1;}
				else if(Dir=='S') {this.ShortTradesToday++; CurrentPosition = -1;}
			}
			public void GoFlat(DateTime t0, double ClosePrice, int CBar = -1, string time = ""){
				CurrentPosition = 0;
				if(Trades.Count==0) return;
				foreach(var trade in Trades.Where(k=>k.PnLPoints==double.MinValue)){
					trade.ExitPrice = ClosePrice;
					trade.ExitDT = t0;
					trade.Disposition = $"Exit {Note}";
					Note = "";
					if(trade.Pos == 'L')      trade.PnLPoints = (trade.ExitPrice - trade.EntryPrice) * trade.Qty;
					else if(trade.Pos == 'S') trade.PnLPoints = (trade.EntryPrice - trade.ExitPrice) * trade.Qty;
					RealizedEquity += trade.PnLPoints;
					if(FirstAbarOfSession == CBar && time!="") {
						Print(time+"  EOD, position closed");
					}
					UnrealizedEquity = 0;
					montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints, trade.Pos);
				}
			}
			public bool ExitforEOD(DateTime t0, DateTime t1, double ClosePrice){
				if(Trades.Count==0) return false;
				var tt0 = ToTime(t0);
				var tt1 = ToTime(t1);
				if(tt1 < GoFlatTOD && tt0 >= GoFlatTOD || t0.Day!=t1.Day){
					CurrentPosition = 0;
					LongTradesToday = 0;
					ShortTradesToday = 0;
					foreach(var trade in Trades.Where(k=>k.PnLPoints==double.MinValue)){
						trade.ExitPrice = ClosePrice;
						trade.ExitDT = t1;
						trade.Disposition = "EOD";
						if(trade.Pos == 'L')      trade.PnLPoints = (trade.ExitPrice - trade.EntryPrice) * trade.Qty;
						else if(trade.Pos == 'S') trade.PnLPoints = (trade.EntryPrice - trade.ExitPrice) * trade.Qty;
						RealizedEquity += trade.PnLPoints;
						UnrealizedEquity = 0;
						montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints, trade.Pos);
					}
					return true;
				}else return false;
			}
			public string ExitforSLTP(DateTime t0, double H, double L, bool PermitEntryExitOnSameBar){
				string result = "";
				if(Trades.Count==0) return result;
				foreach(var trade in Trades.Where(k=>k.PnLPoints==double.MinValue)){
					if(trade.EntryDT == t0 && !PermitEntryExitOnSameBar) continue;
					if(trade.Pos == 'L'){
						if(H >= trade.TP){
							trade.ExitPrice = trade.TP;
							trade.PnLPoints = (trade.ExitPrice - trade.EntryPrice) * trade.Qty;
							RealizedEquity += trade.PnLPoints;
							UnrealizedEquity = 0;
							trade.ExitDT = t0;
							trade.Disposition = "TP";
							result = string.Format("{0}|Ltx",result);
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints, trade.Pos);
						}
						else if(L <= trade.SL){
							trade.ExitPrice = trade.SL;
							trade.PnLPoints = (trade.ExitPrice - trade.EntryPrice) * trade.Qty;
							RealizedEquity += trade.PnLPoints;
							UnrealizedEquity = 0;
							trade.ExitDT = t0;
							trade.Disposition = "SL";
							result = string.Format("{0}|Lsx",result);
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints, trade.Pos);
						}
					}
					else if(trade.Pos == 'S'){
//Print($"215  Short from {trade.EntryPrice} at {trade.EntryDT.ToString()}  Low: {L}");
						if(L < trade.TP){
							trade.ExitPrice = trade.TP;
							trade.PnLPoints = (trade.EntryPrice - trade.ExitPrice) * trade.Qty;
							RealizedEquity += trade.PnLPoints;
							UnrealizedEquity = 0;
							trade.ExitDT = t0;
							trade.Disposition = "TP";
							result = string.Format("{0}|Stx",result);
//Print($"  224    {result}  {trade.PnLPoints}");
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints, trade.Pos);
						}
						else if(H > trade.SL){
							trade.ExitPrice = trade.SL;
							trade.PnLPoints = (trade.EntryPrice - trade.ExitPrice) * trade.Qty;
							RealizedEquity += trade.PnLPoints;
							UnrealizedEquity = 0;
							trade.ExitDT = t0;
							trade.Disposition = "SL";
							result = string.Format("{0}|Ssx",result);
//Print($"  235    {result}  {trade.PnLPoints}");
							montecarlo.AddToTODList(trade.EntryDT, trade.PnLPoints, trade.Pos);
						}
					}
				}
				if(Trades.Count(k=>k.ExitDT == DateTime.MinValue && k.Pos=='L') > 0) CurrentPosition = 1;
				else if(Trades.Count(k=>k.ExitDT == DateTime.MinValue && k.Pos=='S') > 0) CurrentPosition = -1;
				else CurrentPosition = 0;
				return result;
			}

			private MonteCarloEngine montecarlo = new MonteCarloEngine();
			#region -- Monte Carlo --
			private class MonteCarloEngine{
				public SortedDictionary<int, List<double>> PnLByTODLS = new SortedDictionary<int, List<double>>();//TOD is time of day in 30-minute sections
				public SortedDictionary<int, List<double>> PnLByTODL = new SortedDictionary<int, List<double>>();//TOD is time of day in 30-minute sections
				public SortedDictionary<int, List<double>> PnLByTODS = new SortedDictionary<int, List<double>>();//TOD is time of day in 30-minute sections
				public void AddToTODList(DateTime tod, double pnl, char Direction, bool InZone=false){
					int t = 0;
					if(tod.Minute<30) t = tod.Hour*100;
					else t = tod.Hour*100+30;
//NinjaTrader.Code.Output.Process(string.Format("{0} added TODList {1} ", t, pnl), PrintTo.OutputTab1);
					if(!PnLByTODLS.ContainsKey(t)) PnLByTODLS[t] = new List<double>(){pnl};
					else PnLByTODLS[t].Add(pnl);
					if(Direction=='L'){
						if(!PnLByTODL.ContainsKey(t)) PnLByTODL[t] = new List<double>(){pnl};
						else PnLByTODL[t].Add(pnl);
					}else if(Direction=='S'){
						if(!PnLByTODS.ContainsKey(t)) PnLByTODS[t] = new List<double>(){pnl};
						else PnLByTODS[t].Add(pnl);
					}
					if(InZone) NinjaTrader.Code.Output.Process(string.Format("{0} added TODList  {1}  {2}", tod.ToString(), t, pnl), PrintTo.OutputTab1);
				}
				public void DoMonteCarlo(string Type, int BatchesCount, double PctOfTradesPerBatch, double PV, List<string> Output){
					var distribution = new List<double>();
					var r = new Random();
		
					var pnl_table = new List<double>();
					var list = Type=="Both"? PnLByTODLS.SelectMany(k=> k.Value).ToList() : (Type=="L"? PnLByTODL.SelectMany(k=> k.Value).ToList(): PnLByTODS.SelectMany(k=> k.Value).ToList());
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
					var PrintString = "\n";
					Output.Add("-----");

					var s = "Monte Carlo Analysis:  "+(Type=="Both"? "Both Longs and Shorts":(Type=="L"? "Longs only":"Shorts only"));
					PrintString = PrintString + s + Environment.NewLine;
					Output.Add(s);
					
					s = string.Format(" ({0}-sims, Trades {1} taking {2}-samples/sim)", BatchesCount, list.Count, samples);
					PrintString = PrintString + s + Environment.NewLine;
					Output.Add(s+"{fontsize8}");
					
					s = string.Format(" {0} probability of a positive PnL", (winning_iterations*1.0/distribution.Count).ToString("0%"));
					PrintString = PrintString + s + Environment.NewLine;
					Output.Add(s);

					s = "";
					PrintString = PrintString + s + Environment.NewLine;
					Output.Add(s);

					s = string.Format("Worst PnL:  {0}   Best: {1}  Avg: {2}  Median: {3}", (Math.Round(distribution.Min()*PV,0)).ToString("C"), (Math.Round(distribution.Max()*PV,0)).ToString("C"), Math.Round(PV*distribution.Average(),0).ToString("C"), Math.Round(PV*distribution[distribution.Count/2],0).ToString("C")).Replace(".00",string.Empty);
					PrintString = PrintString + s + Environment.NewLine;
					Output.Add(s+"{fontsize8}");
					NinjaTrader.Code.Output.Process(PrintString, PrintTo.OutputTab1);
				}
			}
			#endregion
			private DateTime CalendarDay = DateTime.MinValue;
			#region -- supporting methods --
//			private void Print(string s){
//				NinjaTrader.Code.Output.Process(s, PrintTo.OutputTab1);
//			}
			private string ToCurrency(double c){
				if(Math.Abs(c)>10) c = Math.Round(c);
				var s = c.ToString("C");
				return s.Replace(".00",string.Empty);
			}
			private int TradableDaysCount = 0;
			private int LongTradesToday = 0;
			private int ShortTradesToday = 0;
			public bool IsValidTimeAndDay(char Dir, DateTime t0, DateTime t1, int CurrentBar){
				bool result = false;
				if(DTofLastLong.Day != t0.Day)  LongTradesToday=0;
				if(DTofLastShort.Day != t0.Day) ShortTradesToday=0;
				if(CalendarDay != t0.Date && DOW.Contains(t0.DayOfWeek)) {CalendarDay=t0.Date; TradableDaysCount++;}
				if(StartTOD!=StopTOD){
					int tt0 = ToTime(t0);
					int tt1 = ToTime(t1);
					if(StartTOD < StopTOD){
						if(tt0 < StartTOD || tt0 >= StopTOD) {IsInSession=false; return false;}
					}else{
						if(tt0 < StartTOD && tt0 >= StopTOD) {IsInSession=false; return false;}
					}
				}
				if(Dir=='L'){
					if(DTofLastLong.Day == t0.Day && LongTradesToday>=MaxLongTradesPerDay) {IsInSession=false; return false;}
					result = DOW.Contains(t0.DayOfWeek) && DTofLastLong.Day!= t0.Day;
				}else{
					if(DTofLastShort.Day == t0.Day && ShortTradesToday>=MaxShortTradesPerDay) {IsInSession=false; return false;}
					result = DOW.Contains(t0.DayOfWeek) && DTofLastShort.Day!= t0.Day;
				}
				if(result==true && IsInSession==false){
					IsInSession = true;
					FirstAbarOfSession = CurrentBar;
				}
				return result;
			}
			public void UpdateMinMaxPrices(double H, double L, double C, bool updateUnrealizedEquity=true){
				if(updateUnrealizedEquity) UnrealizedEquity = 0;
				foreach(var t in Trades.Where(k=>k.ExitDT==DateTime.MinValue)){
					t.MaxPriceHigh = Math.Max(H, t.MaxPriceHigh);
					t.MinPriceLow = Math.Min(L, t.MinPriceLow);
					if(updateUnrealizedEquity){
						UnrealizedEquity += t.Pos=='L' ? (C - t.EntryPrice) : (t.EntryPrice - C);
					}
				}
			}
			public void UpdateMinMaxPrices(info T, double H, double L){
				T.MaxPriceHigh = Math.Max(H, T.MaxPriceHigh);
				T.MinPriceLow = Math.Min(L, T.MinPriceLow);
			}
			public double GetPnLPts(DateTime nowDate){
				double result = Trades.Where(k=>k.ExitDT.Date == nowDate).Sum(k=>k.PnLPoints);
				return result;
			}
			public double GetPnLPts(){
				double result = Trades.Where(k=>k.ExitDT != DateTime.MinValue).Sum(k=>k.PnLPoints);
				return result;
			}
			#endregion
			private void Print(string s){ indi.Print(s);}
			private void Print(int L){ indi.Print(L.ToString());}
			public void PrintResults(int BarsCount, int CurrentBar, bool ShowPnLLines, NinjaTrader.NinjaScript.IndicatorBase context){
				#region -- PrintResults --
				Status = "";
				int L=342;
try{
				if(CurrentBar > BarsCount-3 && !this.ResultsPrinted){
					OutputLS.Clear();
					OutputL.Clear();
					OutputS.Clear();
					this.ResultsPrinted = true;
					L=348;
					if(this.Trades.Count==0){ 
						OutputLS.Add("No trades taken");
						OutputL.Add("No trades taken");
						OutputS.Add("No trades taken");
						Print("No trades taken"); return;
					}
					string s = StrategyName+" strategy results for: "+instrument.MasterInstrument.Name;
					OutputLS.Add(s);
					OutputL.Add(s);
					OutputS.Add(s);
					Print(s);
					L=360;
					if(Comment != ""){
						OutputLS.Add(Comment+"{fontsize8}");
						OutputL.Add(Comment+"{fontsize8}");
						OutputS.Add(Comment+"{fontsize8}");
						Print(Comment);
					}
					double pnl = 0;
					double wins = 0;
					double losses = 0;
//					int TPhit = 0;
//					int SLhit = 0;
					var DatesOfTradesLS = new SortedDictionary<DateTime,  List<double>>();
					var DatesOfTradesL = new SortedDictionary<DateTime,  List<double>>();
					var DatesOfTradesS = new SortedDictionary<DateTime,  List<double>>();
					var DOWOfTradesLS   = new SortedDictionary<DayOfWeek, List<double>>();
					var DOWOfTradesL   = new SortedDictionary<DayOfWeek, List<double>>();
					var DOWOfTradesS   = new SortedDictionary<DayOfWeek, List<double>>();

					int LongWinCount = 0;
					int ShortWinCount = 0;
					int LongCount = 0;
					int ShortCount = 0;
					double LongWinPts = 0;
					double ShortWinPts = 0;
					double LongLossPts = 0;
					double ShortLossPts = 0;
					double LongPnL = 0;
					double ShortPnL = 0;
					List<double> MFELongs = new List<double>();
					List<double> MAELongs = new List<double>();
					List<double> MFEShorts = new List<double>();
					List<double> MAEShorts = new List<double>();
					double realized_equity = 0;
					double equity_peak_pts = 0;
					double max_drawdown_pts = 0;
					L=396;
					foreach(var t in Trades){
						if(t.ExitDT == DateTime.MinValue) continue;
						pnl = pnl + t.PnLPoints;
						if(t.Pos=='L'){
							MFELongs.Add((t.MaxPriceHigh-t.EntryPrice));
							MAELongs.Add((t.EntryPrice-t.MinPriceLow));
							LongPnL = LongPnL + t.PnLPoints;
						}else{
							MFEShorts.Add((t.EntryPrice-t.MinPriceLow));
							MAEShorts.Add((t.MaxPriceHigh-t.EntryPrice));
							ShortPnL = ShortPnL + t.PnLPoints;
						}
						L=409;
						realized_equity += t.PnLPoints;
						if(realized_equity > equity_peak_pts){
							equity_peak_pts = realized_equity;
						}else{
							max_drawdown_pts = Math.Min(max_drawdown_pts , realized_equity-equity_peak_pts);
						}
						if(t.PnLPoints>0){
							wins++;
							if(t.Pos=='L') {LongCount++; LongWinCount++; LongWinPts += t.PnLPoints; LongPnL += t.PnLPoints; }
							if(t.Pos=='S') {ShortCount++; ShortWinCount++; ShortWinPts += t.PnLPoints; ShortPnL += t.PnLPoints; }
						}
						else{
							losses++;
							if(t.Pos=='L') {LongCount++; LongLossPts += t.PnLPoints; LongPnL+= t.PnLPoints; }
							if(t.Pos=='S') {ShortCount++; ShortLossPts += t.PnLPoints; ShortPnL += t.PnLPoints; }
						}
						L=426;
						int hr = t.EntryDT.Hour*100 + (t.EntryDT.Minute<30 ? 0:30);
						if(!TradeTimesLS.ContainsKey(hr)) TradeTimesLS[hr] = new List<double>(){t.PnLPoints};
						else TradeTimesLS[hr].Add(t.PnLPoints);
						if(t.Pos=='L'){
							if(!TradeTimesL.ContainsKey(hr)) TradeTimesL[hr] = new List<double>(){t.PnLPoints};
							else TradeTimesL[hr].Add(t.PnLPoints);
						}else if(t.Pos=='S'){
							if(!TradeTimesS.ContainsKey(hr)) TradeTimesS[hr] = new List<double>(){t.PnLPoints};
							else TradeTimesS[hr].Add(t.PnLPoints);
						}

						if(!DatesOfTradesLS.ContainsKey(t.EntryDT.Date)) DatesOfTradesLS[t.EntryDT.Date] = new List<double>(){t.PnLPoints};
						else DatesOfTradesLS[t.EntryDT.Date].Add(t.PnLPoints);
						if(t.Pos=='L'){
							if(!DatesOfTradesL.ContainsKey(t.EntryDT.Date)) DatesOfTradesL[t.EntryDT.Date] = new List<double>(){t.PnLPoints};
							else DatesOfTradesL[t.EntryDT.Date].Add(t.PnLPoints);
						}else if(t.Pos=='S'){
							if(!DatesOfTradesS.ContainsKey(t.EntryDT.Date)) DatesOfTradesS[t.EntryDT.Date] = new List<double>(){t.PnLPoints};
							else DatesOfTradesS[t.EntryDT.Date].Add(t.PnLPoints);
						}
						L=447;
						if(!DOWOfTradesLS.ContainsKey(t.EntryDT.DayOfWeek)) DOWOfTradesLS[t.EntryDT.DayOfWeek] = new List<double>(){t.PnLPoints};
						else DOWOfTradesLS[t.EntryDT.DayOfWeek].Add(t.PnLPoints);
						if(t.Pos=='L'){
							if(!DOWOfTradesL.ContainsKey(t.EntryDT.DayOfWeek)) DOWOfTradesL[t.EntryDT.DayOfWeek] = new List<double>(){t.PnLPoints};
							else DOWOfTradesL[t.EntryDT.DayOfWeek].Add(t.PnLPoints);
						}else if(t.Pos=='S'){
							if(!DOWOfTradesS.ContainsKey(t.EntryDT.DayOfWeek)) DOWOfTradesS[t.EntryDT.DayOfWeek] = new List<double>(){t.PnLPoints};
							else DOWOfTradesS[t.EntryDT.DayOfWeek].Add(t.PnLPoints);
						}

						if(ShowPnLLines){
							L=459;
							Draw.Line(context, 
								$"{t.EntryDT.Ticks}",
								false, t.EntryDT, t.EntryPrice, t.ExitDT, t.ExitPrice, (t.PnLPoints>0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash, 4);
						}
//						if(t.ExitPrice == t.TP) TPhit++;
//						if(t.ExitPrice == t.SL) SLhit++;
						L=466;
						s = t.EntryDT.ToString()+"   "+t.Pos+":  "+(t.PnLPoints>0?"+":"")+instrument.MasterInstrument.FormatPrice(t.PnLPoints)+" pts,  total: "+instrument.MasterInstrument.FormatPrice(pnl)+"    "+wins+"/"+losses+"   Disposition: "+t.Disposition;
						Print(s);
					}
					L=470;
					double PV = instrument.MasterInstrument.PointValue;
					OutputLS.Add(""); OutputL.Add(""); OutputS.Add(""); Print("");
					s = string.Format("Total {0} on {1}-trades   Avg trade {2} PF: {3}", ToCurrency(pnl*PV), Trades.Count, ToCurrency(PV*pnl/Trades.Count), (-(LongWinPts+ShortWinPts)/(LongLossPts+ShortLossPts)).ToString("0.0"));
					OutputLS.Add(s); 
					Print(s);

					s = string.Format("{0}-tradable days,  Avg Trades/day: {1}", TradableDaysCount, (Trades.Count*1.0/TradableDaysCount).ToString("0.0"));
					OutputLS.Add(s+"{fontsize8}");
					Print(s);
					if(LongCount>0){
						s = string.Format("{0}-tradable days,  Avg Trades/day: {1}", TradableDaysCount, (LongCount*1.0/TradableDaysCount).ToString("0.0"));
						OutputL.Add(s+"{fontsize8}");
					}
					if(ShortCount>0){
						s = string.Format("{0}-tradable days,  Avg Trades/day: {1}", TradableDaysCount, (ShortCount*1.0/TradableDaysCount).ToString("0.0"));
						OutputS.Add(s+"{fontsize8}");
					}
					s = string.Format("w/l: {0}|{1}  {2}", wins, losses, (wins / Trades.Count).ToString("0%"), Trades.Count);
					OutputLS.Add(s);
					Print(s);
					L=491;
					if(StrategyName=="ExpectedHighLow"){
						if(ShortCount>0){
							MFEShorts.Sort();
							double mfemedian = MFEShorts[MFEShorts.Count/2];
							MAEShorts.Sort();
							double maemedian = MAEShorts[MAEShorts.Count/2];
							s = string.Format("Shorts{7}:  {0}|{1}  {2}   Avg: {3}  PF: {4}   mae: {5}  mfe: {6}", ShortWinCount, (ShortCount-ShortWinCount), (ShortWinCount*1.0 / ShortCount).ToString("0%"), ToCurrency(PV*ShortPnL/ShortCount), (-(ShortWinPts)/(ShortLossPts)).ToString("0.0"), ToCurrency(PV*maemedian), ToCurrency(PV*mfemedian), MsgShorts);
							OutputLS.Add(s+(s.Contains("($")?"{red}":"{lime}")); OutputS.Add(s+(s.Contains("($")?"{red}":"{lime}"));
						}
						if(LongCount>0){
							MFELongs.Sort();
							double mfemedian = MFELongs[MFELongs.Count/2];
							MAELongs.Sort();
							double maemedian = MAELongs[MAELongs.Count/2];
							s = string.Format("Longs{7}:  {0}|{1}  {2}   Avg: {3}  PF: {4}   mae: {5}  mfe: {6}", LongWinCount, (LongCount-LongWinCount), (LongWinCount*1.0 / LongCount).ToString("0%"), ToCurrency(PV*LongPnL/LongCount), (-(LongWinPts)/(LongLossPts)).ToString("0.0"), ToCurrency(PV*maemedian), ToCurrency(PV*mfemedian), MsgLongs);
							OutputLS.Add(s+(s.Contains("($")?"{red}":"{lime}")); OutputL.Add(s+(s.Contains("($")?"{red}":"{lime}"));
						}
					}else{
						if(LongCount>0){
							MFELongs.Sort();
							double mfemedian = MFELongs[MFELongs.Count/2];
							MAELongs.Sort();
							double maemedian = MAELongs[MAELongs.Count/2];
							s = string.Format("Longs{7}:  {0}|{1}  {2}   Avg: {3}  PF: {4}   mae: {5}  mfe: {6}", LongWinCount, (LongCount-LongWinCount), (LongWinCount*1.0 / LongCount).ToString("0%"), ToCurrency(PV*LongPnL/LongCount), (-(LongWinPts)/(LongLossPts)).ToString("0.0"), ToCurrency(PV*maemedian), ToCurrency(PV*mfemedian), MsgLongs);
							OutputLS.Add(s+(s.Contains("($")?"{red}":"{lime}")); OutputL.Add(s+(s.Contains("($")?"{red}":"{lime}"));
						}
						if(ShortCount>0){
							MFEShorts.Sort();
							double mfemedian = MFEShorts[MFEShorts.Count/2];
							MAEShorts.Sort();
							double maemedian = MAEShorts[MAEShorts.Count/2];
							s = string.Format("Shorts{7}:  {0}|{1}  {2}   Avg: {3}  PF: {4}   mae: {5}  mfe: {6}", ShortWinCount, (ShortCount-ShortWinCount), (ShortWinCount*1.0 / ShortCount).ToString("0%"), ToCurrency(PV*ShortPnL/ShortCount), (-(ShortWinPts)/(ShortLossPts)).ToString("0.0"), ToCurrency(PV*maemedian), ToCurrency(PV*mfemedian), MsgShorts);
							OutputLS.Add(s+(s.Contains("($")?"{red}":"{lime}")); OutputS.Add(s+(s.Contains("($")?"{red}":"{lime}"));
						}
					}
					L=527;
					#region -- Calculate average $/day --
					if(DatesOfTradesLS.Count>0) {
						OutputLS.Add(""); Print("");
						s = "Avg $/day: "+ToCurrency(pnl*PV/DatesOfTradesLS.Count)+"  Max DD: -"+ToCurrency(Math.Abs(max_drawdown_pts*PV));
						if(s.Contains("($")){
							OutputLS.Add(s+"{red}");
						}else{
							OutputLS.Add(s+"{lime}");
						}
						Print(s);
						OutputLS.Add(""); Print("");
					}
					if(DatesOfTradesL.Count>0) {
						OutputL.Add("");
						s = string.Format("Avg $/day: {0}   Days count: {1}", ToCurrency(LongPnL*PV/DatesOfTradesL.Count), DatesOfTradesL.Count);
						if(s.Contains("($")){
							OutputL.Add(s+"{red}");
						}else{
							OutputL.Add(s+"{lime}");
						}
						OutputL.Add(""); Print("");
					}
					if(DatesOfTradesS.Count>0) {
						OutputS.Add("");
						s = string.Format("Avg $/day: {0}   Days count: {1}", ToCurrency(ShortPnL*PV/DatesOfTradesS.Count), DatesOfTradesS.Count);
						if(s.Contains("($")){
							OutputS.Add(s+"{red}");
						}else{
							OutputS.Add(s+"{lime}");
						}
						OutputS.Add("");
					}
					#endregion ----------------
					if(ShowHrOfDayTable){
						#region -- Show trades by hour of day --
						if(TradeTimesLS!=null && TradeTimesLS.Count>0){
							OutputLS.Add("-----");Print("");
							s = "Hr of day for entry";
							OutputLS.Add(s); Print(s);
							s = "Example: '930' means the trade was entered between 9:30 and 9:59";
							OutputLS.Add(s+"{fontsize8}"); Print(s);
							foreach(var tt in TradeTimesLS){
								double sum = tt.Value.Sum();
								wins = tt.Value.Count(k=>k>0);
								losses = tt.Value.Count-wins;
								tt.Value.Clear();
								tt.Value.Add(sum);//the list is cleared out, and the PnL for this timeslice is put in its place
								s = string.Format("{0}:   {1}  {2}   {3}|{4}", tt.Key, ToCurrency(sum*PV), (wins / (wins+losses)).ToString("0%"), wins, losses);
								if(s.Contains("($"))
									OutputLS.Add(s+"{fontsize8}{red}");
								else
									OutputLS.Add(s+"{fontsize8}{lime}");
								Print(s);
							}
						}
						if(TradeTimesL!=null && TradeTimesL.Count>0){
							OutputL.Add("-----");
							s = "Hr of day for LONG entry";
							OutputL.Add(s);
							foreach(var tt in TradeTimesL){
								double sum = tt.Value.Sum();
								wins = tt.Value.Count(k=>k>0);
								losses = tt.Value.Count-wins;
								tt.Value.Clear();
								tt.Value.Add(sum);//the list is cleared out, and the PnL for this timeslice is put in its place
								s = string.Format("{0}:   {1}  {2}   {3}|{4}", tt.Key, ToCurrency(sum*PV), (wins / (wins+losses)).ToString("0%"), wins, losses);
								if(s.Contains("($"))
									OutputL.Add(s+"{fontsize8}{red}");
								else
									OutputL.Add(s+"{fontsize8}{lime}");
							}
						}
						if(TradeTimesS!=null && TradeTimesS.Count>0){
							OutputS.Add("-----");
							s = "Hr of day for SHORT entry";
							OutputS.Add(s);
							foreach(var tt in TradeTimesS){
								double sum = tt.Value.Sum();
								wins = tt.Value.Count(k=>k>0);
								losses = tt.Value.Count-wins;
								tt.Value.Clear();
								tt.Value.Add(sum);//the list is cleared out, and the PnL for this timeslice is put in its place
								s = string.Format("{0}:   {1}  {2}   {3}|{4}", tt.Key, ToCurrency(sum*PV), (wins / (wins+losses)).ToString("0%"), wins, losses);
								if(s.Contains("($"))
									OutputS.Add(s+"{fontsize8}{red}");
								else
									OutputS.Add(s+"{fontsize8}{lime}");
							}
						}
						#endregion
					}
					L=619;
	//				Print("\nBy Date");
	//				foreach(var tt in DatesOfTrades){
	//					Print(tt.Key.ToShortDateString()+"   PnL: "+ToCurrency(tt.Value.Sum()*PV)+"   "+(tt.Value.Count(k=>k>0)*1.0/tt.Value.Count).ToString("0%"));
	//				}
					if(ShowDOWTable){
						L=625;
						#region -- Show trades by DOW --
						if(DOWOfTradesLS!=null && DOWOfTradesLS.Count>0){
							OutputLS.Add("-----"); Print("");
							s = "By Day of week  from "+this.StartTOD+" to "+this.StopTOD+", go flat at "+this.GoFlatTOD;
							OutputLS.Add(s); Print(s);
							foreach(var tt in DOWOfTradesLS){
								int daycount = Trades.Select(x => x.EntryDT.Date).Distinct().Count(date => date.DayOfWeek == tt.Key);
								wins = tt.Value.Count(k=>k>0);
								losses = tt.Value.Count-wins;
								s = string.Format("{0}({1})   {2}   {3}  {4}|{5}", tt.Key.ToString().Substring(0,3), daycount, ToCurrency(tt.Value.Sum()*PV), (tt.Value.Count(k=>k>0)*1.0/tt.Value.Count).ToString("0%"), wins, losses);
								if(s.Contains("($"))
									OutputLS.Add(s+"{fontsize8}{red}");
								else
									OutputLS.Add(s+"{fontsize8}{lime}");
								Print(s);
							}
						}
						if(DOWOfTradesL!=null && DOWOfTradesL.Count>0){
							OutputL.Add("-----");
							s = "By Day of week  from "+this.StartTOD+" to "+this.StopTOD+", go flat at "+this.GoFlatTOD;
							OutputL.Add(s);
							foreach(var tt in DOWOfTradesL){
								int daycount = Trades.Select(x => x.EntryDT.Date).Distinct().Count(date => date.DayOfWeek == tt.Key);
								wins = tt.Value.Count(k=>k>0);
								losses = tt.Value.Count-wins;
								s = string.Format("{0}({1})   {2}   {3}  {4}|{5}", tt.Key.ToString().Substring(0,3), daycount, ToCurrency(tt.Value.Sum()*PV), (tt.Value.Count(k=>k>0)*1.0/tt.Value.Count).ToString("0%"), wins, losses);
								if(s.Contains("($"))
									OutputL.Add(s+"{fontsize8}{red}");
								else
									OutputL.Add(s+"{fontsize8}{lime}");
							}
						}
						if(DOWOfTradesS!=null && DOWOfTradesS.Count>0){
							OutputS.Add("-----");
							s = "By Day of week  from "+this.StartTOD+" to "+this.StopTOD+", go flat at "+this.GoFlatTOD;
							OutputS.Add(s);
							foreach(var tt in DOWOfTradesS){
								int daycount = Trades.Select(x => x.EntryDT.Date).Distinct().Count(date => date.DayOfWeek == tt.Key);
								wins = tt.Value.Count(k=>k>0);
								losses = tt.Value.Count-wins;
								s = string.Format("{0}({1})   {2}   {3}  {4}|{5}", tt.Key.ToString().Substring(0,3), daycount, ToCurrency(tt.Value.Sum()*PV), (tt.Value.Count(k=>k>0)*1.0/tt.Value.Count).ToString("0%"), wins, losses);
								if(s.Contains("($"))
									OutputS.Add(s+"{fontsize8}{red}");
								else
									OutputS.Add(s+"{fontsize8}{lime}");
							}
						}
						#endregion
					}

					L=676;
					montecarlo.DoMonteCarlo("Both", 100, 0.5, PV, OutputLS);
					montecarlo.DoMonteCarlo("L", 100, 0.5, PV, OutputL);
					montecarlo.DoMonteCarlo("S", 100, 0.5, PV, OutputS);
					DatesOfTradesLS.Clear();
					DOWOfTradesLS.Clear();
					TradeTimesLS.Clear();
					DatesOfTradesL.Clear();
					DOWOfTradesL.Clear();
					TradeTimesL.Clear();
					DatesOfTradesS.Clear();
					DOWOfTradesS.Clear();
					TradeTimesS.Clear();
					MFELongs.Clear();
					MAELongs.Clear();
					MFEShorts.Clear();
					MAEShorts.Clear();
					L=693;
					montecarlo.PnLByTODLS.Clear();
					montecarlo.PnLByTODL.Clear();
					montecarlo.PnLByTODS.Clear();
				}
				#endregion
}catch(Exception e){Status = L.ToString()+"  error: "+e.ToString();}
			}
			//======================================================================================
			public double StrToDouble(string s){
				var ch = s.ToCharArray();
				string result = "";
				foreach(var c in ch) if((c>='0' && c<='9') || c=='.' || c=='-') {
					result = string.Format("{0}{1}",result,c);
				}
				if(result.Length==0) return 0;
				var doub = 0.0;
				if(!double.TryParse(result, out doub))
					double.TryParse(result.Replace(".",","),out doub);
				return doub;
			}
/*			public double StrToDouble(string s){
				var ch = s.ToCharArray();
				string result = "";
				foreach(var c in ch) if((c>='0' && c<='9') || c=='.' || c=='-') result = string.Format("{0}{1}",result,c);
				if(result.Length==0) return 0;
				return Convert.ToDouble(result);
			}*/
			//======================================================================================
			public int StrToInt(string s){
				var ch = s.ToCharArray();
				string result = "";
				foreach(var c in ch) if((c>='0' && c<='9') || c=='-') result = string.Format("{0}{1}",result,c);
				if(result.Length==0) return 0;
				return Convert.ToInt32(result);
			}
		//======================================================================================
			public Dictionary<System.Windows.Media.Brush, SharpDX.Direct2D1.Brush> BrushesDX = new Dictionary<System.Windows.Media.Brush, SharpDX.Direct2D1.Brush>();

			public string InitializeBrushes(SharpDX.Direct2D1.RenderTarget RenderTarget){
				var s = "";
				try{	if(!BrushesDX.ContainsKey(Brushes.Lime))    BrushesDX[Brushes.Lime] = null;}catch(Exception e){s = "Lime: "+e.ToString();}
				try{	if(!BrushesDX.ContainsKey(Brushes.Red))     BrushesDX[Brushes.Red] = null;}catch(Exception e){s = "Red: "+e.ToString();}
				try{	if(!BrushesDX.ContainsKey(Brushes.Magenta)) BrushesDX[Brushes.Magenta] = null;}catch(Exception e){s = "Magenta: "+e.ToString();}
				try{	if(!BrushesDX.ContainsKey(Brushes.Cyan))    BrushesDX[Brushes.Cyan] = null;}catch(Exception e){s = "Cyan: "+e.ToString();}
				try{	if(!BrushesDX.ContainsKey(Brushes.Black))   BrushesDX[Brushes.Black] = null;}catch(Exception e){s = "Black: "+e.ToString();}
				System.Windows.Media.Brush bx = Brushes.Lime;
				try{
				foreach(var b in BrushesDX.Keys.ToList()){
					bx = b;
					if(BrushesDX[b]!=null && !BrushesDX[b].IsDisposed) BrushesDX[b].Dispose();
					if(RenderTarget !=null) {
						BrushesDX[b] = b.ToDxBrush(RenderTarget);
					}
					else {
						BrushesDX[b] = null;
					}
				}
				}catch(Exception e){s = "Problem with brush: "+bx.ToString();}
				return s;
			}

			public void OnRender(SharpDX.Direct2D1.RenderTarget RenderTarget, NinjaTrader.Gui.Chart.ChartPanel ChartPanel, List<string> Output, int LargeFontSize, int SmallFontSize){
				float x = 10f;
				float y = 25f + ChartPanel.Y;
				var txtFormat14 = new NinjaTrader.Gui.Tools.SimpleFont("Arial", LargeFontSize).ToDirectWriteTextFormat();
				#region -- Print Time before we start --
				var now = DateTime.Now;
				if(!DOW.Contains(now.DayOfWeek) && WarnIfWrongDayOfWeek){
					x = Convert.ToSingle(ChartPanel.W/2);
					y = Convert.ToSingle(ChartPanel.H/2 + ChartPanel.Y);
					var s = "Wrong day of week...update/change your 'Days of week' parameter";
					var txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, s, txtFormat14, (float)(ChartPanel.X + ChartPanel.W), txtFormat14.FontSize);
					var labelRect = new SharpDX.RectangleF(x-4f, y-4f, txtLayout.Metrics.Width+16f, txtFormat14.FontSize + 9f);
					if(BrushesDX[Brushes.Black].IsValid(RenderTarget))
						RenderTarget.FillRectangle(labelRect, BrushesDX[Brushes.Black]);
					if(BrushesDX[Brushes.Red].IsValid(RenderTarget))
						RenderTarget.DrawText(s, txtFormat14, labelRect, BrushesDX[Brushes.Red]);
				}else{
					var hr = StartTOD/100; var min = StartTOD-hr*100; var futureStartTOD = new DateTime(now.Year,now.Month,now.Day,hr,min,0);
					var ts = new TimeSpan(futureStartTOD.Ticks-now.Ticks);
					if(ts.TotalSeconds>0) {
						x = Convert.ToSingle(ChartPanel.W/2);
						y = Convert.ToSingle(ChartPanel.H/2 + ChartPanel.Y);
						var s = string.Format("{0} until start {1}", ts.ToString("hh':'mm':'ss"), futureStartTOD.ToShortTimeString());
						var txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, s, txtFormat14, (float)(ChartPanel.X + ChartPanel.W), txtFormat14.FontSize);
						var labelRect = new SharpDX.RectangleF(x-4f, y-4f, txtLayout.Metrics.Width+16f, txtFormat14.FontSize + 9f);
						if(BrushesDX[Brushes.Black].IsValid(RenderTarget))
							RenderTarget.FillRectangle(labelRect, BrushesDX[Brushes.Black]);
						if(BrushesDX[Brushes.Cyan].IsValid(RenderTarget))
							RenderTarget.DrawText(s, txtFormat14, labelRect, BrushesDX[Brushes.Cyan]);
					}
				}
				#endregion
				var txtFormat8 = new NinjaTrader.Gui.Tools.SimpleFont("Arial", SmallFontSize).ToDirectWriteTextFormat();
				x = 10f;
				y = 25f + ChartPanel.Y;
				var Color = 'B';
				string L = "";
				foreach(var line in Output){
					Color = 'B';
					L = line.Replace("{fontsize8}","");
					if(L.Contains("{red}")) {Color = 'R'; L = L.Replace("{red}","");}
					else if(L.Contains("{lime}")) {Color = 'G'; L = L.Replace("{lime}","");}
					if(line=="" || line.CompareTo(Environment.NewLine)==0){
						y = y + txtFormat8.FontSize/2f;
					}else if(line.Contains("{fontsize8}")){
						var txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, L, txtFormat8, (float)(ChartPanel.X + ChartPanel.W), txtFormat8.FontSize);
						var labelRect = new SharpDX.RectangleF(x-4f, y-4f, txtLayout.Metrics.Width+16f, txtFormat8.FontSize + 9f);
						if(BrushesDX[Brushes.Black].IsValid(RenderTarget))
							RenderTarget.FillRectangle(labelRect, BrushesDX[Brushes.Black]);
						labelRect.X = x;
						labelRect.Y = y;
						if(BrushesDX[Brushes.Cyan].IsValid(RenderTarget) && BrushesDX[Brushes.Magenta].IsValid(RenderTarget) && BrushesDX[Brushes.Lime].IsValid(RenderTarget))
							RenderTarget.DrawText(L, txtFormat8, labelRect, Color=='B'? BrushesDX[Brushes.Cyan] : (Color=='R' ? BrushesDX[Brushes.Magenta] : BrushesDX[Brushes.Lime]));
						y = y + txtFormat8.FontSize+8f;
					}else{
						var txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, L, txtFormat14, (float)(ChartPanel.X + ChartPanel.W), txtFormat14.FontSize);
						var labelRect = new SharpDX.RectangleF(x-4f, y-4f, txtLayout.Metrics.Width+16f, txtFormat14.FontSize + 9f);
						if(BrushesDX[Brushes.Black].IsValid(RenderTarget))
							RenderTarget.FillRectangle(labelRect, BrushesDX[Brushes.Black]);
						labelRect.X = x;
						labelRect.Y = y;
						if(BrushesDX[Brushes.Cyan].IsValid(RenderTarget) && BrushesDX[Brushes.Magenta].IsValid(RenderTarget) && BrushesDX[Brushes.Lime].IsValid(RenderTarget))
							RenderTarget.DrawText(L, txtFormat14, labelRect, Color=='B'? BrushesDX[Brushes.Cyan] : (Color=='R' ? BrushesDX[Brushes.Magenta] : BrushesDX[Brushes.Lime]));
						y = y + txtFormat14.FontSize+8f;
					}
				}
			}
		}
		//==============================================
		#endregion
}




