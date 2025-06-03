
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
//using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ATRForecaster : Indicator
	{
		private const int OVERBOUGHT_STATE = 2;
		private const int MIDDLE_STATE = 0;
		private const int OVERSOLD_STATE = -2;
		[Display(Name="Version", Order=10, GroupName="Setup")]
		public string ProductVersion { get { return "v1.0 11-July-2023"; } }
		private bool IsDebug        = false;

		private SortedDictionary<int,List<double>> atr = new SortedDictionary<int,List<double>>();
		//private double MinATR = double.MaxValue;
		private double AvgATR = 0;
		private double CurHigh = 0;
		private double CurLow = double.MaxValue;
		private SessionIterator sessionIterator0;
		private DateTime LaunchTime = DateTime.Now;
		private bool DeletedWarningMessage = false;
		private int FirstBarOfSession = 0;
		double ATRAvgDistance=0;
		private DateTime TradingDay=DateTime.MinValue, PriorTradingDay = DateTime.MinValue;
		private double FixedMaxUpperPrice = 0;
		private double FixedMaxLowerPrice = 0;
		private double FixedAvgUpperPrice = 0;
		private double FixedAvgLowerPrice = 0;
		private ATR Atr;
		private Brush NoMansLandBrush = Brushes.DimGray;
		double maxv = 0;
		double minv = 0;
		ParabolicSAR psar = null;

		private double ShortStopPrice = 0;
		private double LongStopPrice = 0;
		private class info {
			public DateTime EntryDT = DateTime.MinValue;
			public int    SignalABar  = 0;
			public int    EntryABar   = 0;
			public double EntryPrice  = 0;
			public double TargetPrice = 0;
			public double BETriggerPrice = 0;
			public char   Direction   = ' ';
			public string Disposition = string.Empty;
			public int    ExitABar    = -1;
			public double ExitPrice   = 0;
			public double SLPrice     = 0;
			public bool   BEStopEngaged    = false;
			public bool   TrailStopEngaged = false;
			public double LocPercentileInDailyRange = 0;
			public double Contracts = 1;
			public double PnL = 0;
			public double InitialRiskPts = 0;
			public double InitialRewardPts = 0;
			public info (char dir, double entryPrice, int entryABar, int signalABar, DateTime entryDT, double targetPrice, double slPrice, double contracts, double dayHigh, double dayLow){
				Direction=dir; EntryPrice=entryPrice; EntryABar=entryABar; SignalABar=signalABar; EntryDT = entryDT; TargetPrice = targetPrice; SLPrice = slPrice; 
				BETriggerPrice = (TargetPrice+SLPrice)/2;
				Contracts = Math.Max(1,contracts);
				LocPercentileInDailyRange = (EntryPrice-dayLow)/(dayHigh-dayLow);
				InitialRiskPts = Math.Abs(entryPrice-slPrice)*Contracts;
				InitialRewardPts = Math.Abs(targetPrice-entryPrice)*Contracts;
			}
		}
		private List<info> Trades = new List<info>();
		public override void OnCalculateMinMax()
		{
//			if(CurrentBars[0]>2){
//				try{
//					if(ATRAvgUpper.IsValidDataPointAt(CurrentBars[0]-1)) {MaxValue = ATRAvgUpper[1]; maxv = MaxValue;}
//					if(ATRAvgLower.IsValidDataPointAt(CurrentBars[0]-1)) {MinValue = ATRAvgLower[1]; minv = MinValue;}
//				}catch{};//(Exception e){Print("OnCalcMinMax: "+e.ToString());}
//			}else{
//				MaxValue=maxv;
//				MinValue=minv;
//			}
			double maxv = double.MinValue;
			double minv = double.MaxValue;
			// For performance optimization, only loop through what is viewable on the chart
			for (int index = ChartBars.FromIndex; index <= ChartBars.ToIndex; index++)
			{
				if(ATRAvgUpper.IsValidDataPointAt(index)) {maxv = Math.Max(maxv, ATRAvgUpper.GetValueAt(index));}
				if(ATRAvgLower.IsValidDataPointAt(index)) {minv = Math.Min(minv, ATRAvgLower.GetValueAt(index));}
			}
			MaxValue = maxv;
			MinValue = minv;
		}
		//=====================================================================================
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "ATR Forecaster";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				ATR_period     = 14;
				pReductionMult = 0.75;
				IsAutoScale    = false;
				pRewardRiskMult = 0;
				pDevelopingLevels = false;
				pMidOpacity = 20;
				pResetTime = ATRForecaster_ResetTimes.NewSession;
				pResetTOD = 930;
				AddPlot(new Stroke(Brushes.HotPink,2), PlotStyle.Line, "ATR Avg Upper");
				AddPlot(new Stroke(Brushes.Lime,2), PlotStyle.Line, "ATR Avg Lower");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "Midline");
				AddPlot(new Stroke(Brushes.Red,1), PlotStyle.TriangleUp, "Lower ShelfEdge");
				AddPlot(new Stroke(Brushes.Green,1), PlotStyle.TriangleDown, "Higher ShelfEdge");
				AddPlot(new Stroke(Brushes.Green,1), PlotStyle.Dot, "Avg Daily Top");
				AddPlot(new Stroke(Brushes.Red,1), PlotStyle.Dot, "Avg Daily Bottom");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "LocPct");
				AddPlot(new Stroke(Brushes.Lime,1), PlotStyle.Dot, "BuyBreakDot");//oversold area psar
				AddPlot(new Stroke(Brushes.Magenta,1), PlotStyle.Dot, "SellBreakDot");//overbought area psar
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "MktAnalyzerState");

			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("CB15E08BE30BC80628CFF6010471FA2A")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				AddDataSeries(Data.BarsPeriodType.Minute, 10);
				AddDataSeries(Data.BarsPeriodType.Minute, 1440);
				if(pDevelopingLevels) BkgDataID = 1; else BkgDataID = 2;
			}
			else if (State == State.DataLoaded)
			{
				var Acceleration				= 0.02;
				var AccelerationStep			= 0.02;
				var AccelerationMax				= 0.2;
				psar = ParabolicSAR(Acceleration, AccelerationMax, AccelerationStep);
				//Atr = ATR(BarsArray[BkgDataID], this.ATR_period);
				//stores the sessions once bars are ready, but before OnBarUpdate is called
				sessionIterator0 = new SessionIterator(BarsArray[BkgDataID]);
			}
		}
		//=====================================================================================
		#region -- Plots --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> ATRAvgUpper { get { return Values[0]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> ATRAvgLower { get { return Values[1]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> Mid { get { return Values[2]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> LShelfEdge { get { return Values[3]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> HShelfEdge { get { return Values[4]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> AvgDailyHigh { get { return Values[5]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> AvgDailyLow { get { return Values[6]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> LocPct { get { return Values[7]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BuyBreak { get { return Values[8]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> SellBreak { get { return Values[9]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MktAnalyzerState { get { return Values[10]; } }
		#endregion
		//=====================================================================================
		#region commented out
//		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
//		{
//			ConnectionStatusEventArgs eCopy = e;

//			Dispatcher.InvokeAsync(() =>
//			{
//				Print(string.Format("{1} Status: {2}",
//					Environment.NewLine,
//					eCopy.Connection.Options.Name,
//					eCopy.Status));
//			}); 
//		}
//		public override void Cleanup()
//		{
//			// Make sure to unsubscribe to the connection status subscription
//			Connection.ConnectionStatusUpdate -= OnConnectionStatusUpdate;
//		}
		#endregion
		//=====================================================================================
		char Dir = ' ';
		bool PrintResults = true;
		double pSLTicks = 10;
		bool pPermitBE = false;
		double H = -1;
		double L = double.MaxValue;
		bool isNewSession = false;
		int BkgDataID = -1;
		int timeptr = 0;
		int LastTimePtr = -1;
		IDrawingTool o = null;

		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday) return;
			if(CurrentBars[0]<2) return;

//			var objects = DrawObjects.Where(xo=>xo.ToString().Contains(".PathTool")).ToList();
//			o = null;
//			if(objects!=null){
//				for(int idx = 0; idx<objects.Count; idx++){
//					if (objects[idx].ToString().EndsWith(".PathTool")) {
//						Print(objects[idx].Tag);
//						PathTool pt = (PathTool)objects[idx];
//						var p = pt.Anchors.GetEnumerator();
//						p.Reset();
//						while(p.MoveNext()){
//							Print("BarsAgo: "+p.Current.DrawnOnBar+"   "+p.Current.Price);
//						}
//						p.Dispose();
//					}
//				}
//				Print("");
//			}
			if(BarsInProgress == 0 && IsFirstTickOfBar && pResetTime == ATRForecaster_ResetTimes.Midnight){
				if(CurrentBars[0]>1){
					PriorTradingDay = Times[0][1].Date;
					TradingDay      = Times[0][0].Date;
					if(TradingDay != PriorTradingDay) isNewSession = true;
				}
				if(isNewSession || L==double.MaxValue){
					H = Highs[0][0];
					L = Lows[0][0];
				}else{
					H = Math.Max(H,Highs[0][0]);
					L = Math.Min(L,Lows[0][0]);
				}
//				if(pDevelopingLevels)
//					timeptr = Convert.ToInt32(Math.Truncate((Times[0][0].Hour*60 + Times[0][0].Minute)/10.0));
				if(isNewSession || L==double.MaxValue){
					H = Highs[0][0];
					L = Lows[0][0];
				}else{
					H = Math.Max(H,Highs[0][0]);
					L = Math.Min(L,Lows[0][0]);
				}
				if(pDevelopingLevels){
					timeptr = Convert.ToInt32(Math.Truncate((Times[0][0].Hour*60 + Times[0][0].Minute)/10.0));
					if(timeptr != LastTimePtr){
						LastTimePtr = timeptr;
						if(!atr.ContainsKey(timeptr)) atr[timeptr]=new List<double>();
						atr[timeptr].Add(H-L);
					}else {
						if(!atr.ContainsKey(timeptr)) atr[timeptr]=new List<double>();
						atr[timeptr][atr[timeptr].Count-1] = H-L;
					}
				}else{
					if(!atr.ContainsKey(timeptr)) atr[timeptr]=new List<double>();
					atr[timeptr].Add(H-L);
				}
//Print("timeptr: "+timeptr+"   "+Times[1][0].Hour+":"+Times[1][0].Minute);
//foreach(var s in atr[timeptr])Print(s);
				AvgATR = atr[timeptr].Average();
				pSLTicks = AvgATR *0.25 / TickSize;
				ATRAvgDistance = Round2Tick(AvgATR/2.0 * pReductionMult);
			}else if(BarsInProgress==BkgDataID && IsFirstTickOfBar && pResetTime == ATRForecaster_ResetTimes.NewSession) {
//				if(){
					sessionIterator0.CalculateTradingDay(Times[BkgDataID][0],true);
					PriorTradingDay = TradingDay;
					TradingDay = sessionIterator0.GetTradingDay(Times[BkgDataID][0]);
					if(TradingDay != PriorTradingDay) isNewSession = true;
//				}
				if(isNewSession || L==double.MaxValue){
//Print("NewSession "+Times[1][0].ToString());
					H = Highs[BkgDataID][0];
					L = Lows[BkgDataID][0];
				}else{
					H = Math.Max(H,Highs[BkgDataID][0]);
					L = Math.Min(L,Lows[BkgDataID][0]);
				}
				if(pDevelopingLevels)
					timeptr = Convert.ToInt32(Math.Truncate((Times[BkgDataID][0].Hour*60 + Times[BkgDataID][0].Minute)/10.0));
				if(!atr.ContainsKey(timeptr)) atr[timeptr]=new List<double>();
				atr[timeptr].Add(H-L);
				while(atr[timeptr].Count > ATR_period) atr[timeptr].RemoveAt(0);
//Print("timeptr: "+timeptr+"   "+Times[1][0].Hour+":"+Times[1][0].Minute);
//foreach(var s in atr[timeptr])Print(s);
				AvgATR = atr[timeptr].Average();
				pSLTicks = AvgATR *0.25 / TickSize;
				ATRAvgDistance = Round2Tick(AvgATR/2.0 * pReductionMult);
				if(BarsInProgress != 0) return;
			}
			if(BarsInProgress==BkgDataID && IsFirstTickOfBar && pResetTime == ATRForecaster_ResetTimes.Custom && CurrentBars[BkgDataID]>1) {
				var tod1 = ToTime(Times[BkgDataID][1])/100;
				var tod = ToTime(Times[BkgDataID][0])/100;

				bool c1 = tod1 < pResetTOD && tod >= pResetTOD;
				bool c2 = tod < tod1 && (tod >= pResetTOD || tod1 <= pResetTOD);
				if(c1 || c2) isNewSession = true;

				if(isNewSession || L==double.MaxValue){
//Print("NewSession "+Times[1][0].ToString());
					H = Highs[BkgDataID][0];
					L = Lows[BkgDataID][0];
				}else{
					H = Math.Max(H,Highs[BkgDataID][0]);
					L = Math.Min(L,Lows[BkgDataID][0]);
				}
				if(pDevelopingLevels)
					timeptr = Convert.ToInt32(Math.Truncate((Times[BkgDataID][0].Hour*60 + Times[BkgDataID][0].Minute)/10.0));
				if(!atr.ContainsKey(timeptr)) atr[timeptr]=new List<double>();
				atr[timeptr].Add(H-L);
				while(atr[timeptr].Count > ATR_period) atr[timeptr].RemoveAt(0);
//Print("timeptr: "+timeptr+"   "+Times[1][0].Hour+":"+Times[1][0].Minute);
//foreach(var s in atr[timeptr])Print(s);
				AvgATR = atr[timeptr].Average();
				pSLTicks = AvgATR *0.25 / TickSize;
				ATRAvgDistance = Round2Tick(AvgATR/2.0 * pReductionMult);
				if(BarsInProgress != 0) return;
			}

			if(BarsInProgress == 0){
				Draw.Region(this,"nomansland",CurrentBars[0]-1, 0, LShelfEdge,HShelfEdge,Brushes.Transparent,NoMansLandBrush,pMidOpacity);
				if(isNewSession){
//if(ChartControl==null)Print(Instrument.FullName+"   New session "+Times[0][0].ToString());
					isNewSession = false;
					FirstBarOfSession = CurrentBars[0];
//Print("\n"+BarsArray[0].BarsPeriod.Value+"-minute chart, New session: "+Times[0][0].ToString()+"  Close: "+Closes[0][0]);
//if(CurrentBars[0]>2)Draw.Dot(this,Times[0][0].ToString(), false,1,Opens[0][0], Brushes.Red);
					CurHigh = Highs[0][0];
					CurLow  = Lows[0][0];
				}else{
					CurHigh = Math.Max(CurHigh, Highs[0][0]);
					CurLow  = Math.Min(CurLow, Lows[0][0]);
				}

				Mid[0] = (CurHigh + CurLow)/2.0;
				AvgDailyHigh[0] = Round2Tick(Mid[0] + AvgATR/2);
				AvgDailyLow[0]  = Round2Tick(Mid[0] - AvgATR/2);
				ATRAvgUpper[0]  = Round2Tick(Mid[0] + ATRAvgDistance);
				ATRAvgLower[0]  = Round2Tick(Mid[0] - ATRAvgDistance);
				LShelfEdge[0]   = Round2Tick(ATRAvgLower[0] + (ATRAvgUpper[0]-ATRAvgLower[0])*0.4);
				HShelfEdge[0]   = Round2Tick(ATRAvgUpper[0] - (ATRAvgUpper[0]-ATRAvgLower[0])*0.4);
				LocPct[0]       = Math.Round((Close[0] - CurLow)/(CurHigh - CurLow),2)*100;
				if(Highs[0][1] > ATRAvgUpper[1] && psar[1] < Lows[0][1]) SellBreak[1] = Instrument.MasterInstrument.RoundToTickSize(psar[1]);
				if(Lows[0][1] < ATRAvgLower[1] && psar[1] > Highs[0][1]) BuyBreak[1]  = Instrument.MasterInstrument.RoundToTickSize(psar[1]);

				int tt1 = ToTime(Times[0][1])/100;
				int tt = ToTime(Times[0][0])/100;
				if(pRewardRiskMult>0){
					int BarOfSession = CurrentBars[0] - FirstBarOfSession;
					#region -- calc trades --
					if(BarOfSession <= 2){
						LongStopPrice  = double.MinValue;
						ShortStopPrice = double.MinValue;
						Dir = ' ';
					}
//var z = Times[0][0].Day==3 && Times[0][0].Hour==0;
//if(z)Print(Times[0][0].ToString()+"   Dir: "+Dir);
					if(Dir==' '){//  && LongStopPrice!=double.MinValue){
						if(Lows[0][1] <= AvgDailyLow[2] && Closes[0][0] < LShelfEdge[1]){
							ShortStopPrice  = ATRAvgLower[1];
							Draw.Diamond(this,CurrentBars[0].ToString(),false,0,ShortStopPrice,Brushes.Red,true);
//if(z)Print("  Diamond drawn at short stop price");
						}
						else if(Highs[0][1] >= AvgDailyHigh[2] && Closes[0][0] > HShelfEdge[1]){
							LongStopPrice = ATRAvgUpper[1];
							Draw.Diamond(this,CurrentBars[0].ToString(),false,0,LongStopPrice,Brushes.Red,true);
//if(z)Print("  Diamond drawn at long stop price");
						}
					}
					
					if(Dir!='L' && tt<1600 && ATRAvgUpper.IsValidDataPoint(1) && ATRAvgLower[1]!=ATRAvgUpper[1]
						&& LongStopPrice != double.MinValue
						&& Highs[0][1] < LongStopPrice
						&& Highs[0][0] >= LongStopPrice
					){
						double entryPrice = LongStopPrice;
						LongStopPrice     = double.MinValue;
						double tgtPrice   = AvgDailyHigh[1];
						double slPrice    = entryPrice - Math.Abs(entryPrice-tgtPrice)*pRewardRiskMult;
						Trades.Insert(0, new info('L', entryPrice, CurrentBars[0], 0, Times[0][0], tgtPrice, slPrice, 1, CurHigh, CurLow));
						Dir = 'L';
						double DlrsTgt = Instrument.MasterInstrument.PointValue*(Trades[0].TargetPrice-Trades[0].EntryPrice);
						double DlrsRisk = Instrument.MasterInstrument.PointValue*(Trades[0].EntryPrice-Trades[0].SLPrice);
						Draw.Dot(this,string.Format("Tgt {0} {1}",DlrsTgt.ToString("C"),CurrentBar), false,0, Trades[0].TargetPrice, Brushes.Blue);
						Draw.Dot(this,string.Format("SL {0} {1}",DlrsRisk.ToString("C"),CurrentBar), false,0, Trades[0].SLPrice, Brushes.Red);
//if(z)Print("  LONG dot drawn, entry at "+entryPrice);
					}else
					if(Dir!='S' && tt<1600 && ATRAvgLower.IsValidDataPoint(1) && ATRAvgLower[1]!=ATRAvgUpper[1]
						&& ShortStopPrice != double.MinValue
						&& Lows[0][1] > ShortStopPrice
						&& Lows[0][0] <= ShortStopPrice
					){
						double entryPrice = ShortStopPrice;
						ShortStopPrice  = double.MinValue;
						double tgtPrice = AvgDailyLow[1];//Math.Max(Highs[0][0],Highs[0][1])+10*TickSize;//entryPrice+pSLTicks*TickSize;
						double slPrice  = entryPrice + Math.Abs(entryPrice-tgtPrice)*pRewardRiskMult;
						Trades.Insert(0, new info('S', entryPrice, CurrentBars[0], 0, Times[0][0], tgtPrice, slPrice, 1, CurHigh, CurLow));
						Dir = 'S';
						double DlrsTgt  = Instrument.MasterInstrument.PointValue*(Trades[0].EntryPrice-Trades[0].TargetPrice);
						double DlrsRisk = Instrument.MasterInstrument.PointValue*(Trades[0].SLPrice-Trades[0].EntryPrice);
						Draw.Dot(this,string.Format("Tgt {0} {1}",DlrsTgt.ToString("C"),CurrentBar), false,0, Trades[0].TargetPrice, Brushes.Blue);
						Draw.Dot(this,string.Format("SL {0} {1}",DlrsRisk.ToString("C"),CurrentBar), false,0, Trades[0].SLPrice, Brushes.Red);
//if(z)Print("  SHORT dot drawn, entry at "+entryPrice);
					}

//bool z = Times[0][0].Day==2 && Times[0][0].Month==7;
int BETicksProfitLock = 3;
					List<info> tr = Trades.Where(k=>k.ExitABar == -1 && k.EntryABar<CurrentBars[0]).ToList();
					foreach(var t in tr){
						if(t.Direction=='L'){
							if(pPermitBE){
								if(!t.BEStopEngaged && t.EntryABar < CurrentBars[0]-2 && Lows[0][0] > t.EntryPrice && Lows[0][1] > t.EntryPrice) {
									t.SLPrice = t.EntryPrice + BETicksProfitLock*TickSize;
									Draw.Dot(this,"BE"+CurrentBar.ToString(), false,0,t.SLPrice, Brushes.Yellow);
									t.BEStopEngaged = true;
								}
							}
							if(Highs[0][0] > t.TargetPrice) {
								t.ExitPrice = t.TargetPrice;
								t.ExitABar  = CurrentBars[0];
								t.PnL = Instrument.MasterInstrument.RoundToTickSize(t.ExitPrice - t.EntryPrice);
								t.Disposition = "Tgt";
								Dir = ' ';
								ShortStopPrice = double.MinValue;
								LongStopPrice = double.MinValue;
							}else if(Lows[0][0] < t.SLPrice){
								t.ExitPrice = t.SLPrice;
								t.ExitABar  = CurrentBars[0];
								t.PnL = Instrument.MasterInstrument.RoundToTickSize(t.ExitPrice - t.EntryPrice);
								if(t.BEStopEngaged) t.Disposition = "BE";
								else t.Disposition = "SL";
								Dir = ' ';
								ShortStopPrice = double.MinValue;
								LongStopPrice = double.MinValue;
							}
						}
						else if(t.Direction=='S'){
							if(pPermitBE){
								if(!t.BEStopEngaged && t.EntryABar < CurrentBars[0]-2 && Highs[0][0] < t.EntryPrice && Highs[0][1] < t.EntryPrice) {
									t.SLPrice = t.EntryPrice - BETicksProfitLock*TickSize;
									Draw.Dot(this,"BE"+CurrentBar.ToString(), false,0,t.SLPrice, Brushes.Yellow);
									t.BEStopEngaged = true;
								}
							}
							if(Lows[0][0] < t.TargetPrice) {
								t.ExitPrice = t.TargetPrice;
								t.ExitABar  = CurrentBars[0];
								t.PnL = Instrument.MasterInstrument.RoundToTickSize(t.EntryPrice - t.ExitPrice);
								t.Disposition = "Tgt";
								Dir = ' ';
								ShortStopPrice = double.MinValue;
								LongStopPrice = double.MinValue;
							}else if(Highs[0][0] > t.SLPrice){
								t.ExitPrice = t.SLPrice;
								t.ExitABar  = CurrentBars[0];
								t.PnL = Instrument.MasterInstrument.RoundToTickSize(t.EntryPrice - t.ExitPrice);
								if(t.BEStopEngaged) t.Disposition = "BE";
								else t.Disposition = "SL";
								Dir = ' ';
								ShortStopPrice = double.MinValue;
								LongStopPrice = double.MinValue;
							}
						}
						if(tt >= 1610 && tt1 < 1610 || Times[0][0].Day!=Times[0][1].Day){
							t.ExitPrice = Closes[0][0];
							t.ExitABar  = CurrentBars[0];
							if(t.Direction=='L') t.PnL = Instrument.MasterInstrument.RoundToTickSize(t.ExitPrice - t.EntryPrice);
							else if(t.Direction=='S') t.PnL = Instrument.MasterInstrument.RoundToTickSize(t.EntryPrice - t.ExitPrice);
							Dir = ' ';
							ShortStopPrice = double.MinValue;
							LongStopPrice = double.MinValue;
							t.Disposition = "EOD";
						}
					}
					#endregion
				}

				#region -- Set MktAnalyzerState --
				if(ChartControl==null){
					MktAnalyzerState[0] = MIDDLE_STATE;
					if(Highs[0][0] > ATRAvgUpper[1]){
						MktAnalyzerState[0] = OVERBOUGHT_STATE;
						if(CurrentBars[0] > BarsArray[0].Count-3) Print(Instrument.FullName+" "+Highs[0][0]+" > OB @ "+ATRAvgUpper[1]+"    "+Times[0][0].ToString());
					}
					else if(Lows[0][0] < ATRAvgLower[1]){
						MktAnalyzerState[0] = OVERSOLD_STATE;
						if(CurrentBars[0] > BarsArray[0].Count-3) Print(Instrument.FullName+" "+Lows[0][0]+" < OS @ "+ATRAvgLower[1]+"    "+Times[0][0].ToString());
					}
				}
				#endregion
			}
			if(PrintResults && CurrentBars[0] > BarsArray[0].Count-3 && pRewardRiskMult>0){
				#region -- Print trading results --
//				ClearOutputWindow();
				PrintResults   = false;
				double Wins    = 0;
				double Losses  = 0;
				double PnLDlr  = 0;
				double BEs     = 0;
				double WinDlrs = 0;
				double LossDlrs = 0;
				double BEDlrs   = 0;
				double SumRiskPts   = 0;
				double SumRewardPts = 0;
				Trades.Reverse();
				foreach(var t in Trades){
					if(t.ExitABar == -1) continue;
					PnLDlr = PnLDlr + t.PnL * Instrument.MasterInstrument.PointValue;
					SumRiskPts += t.InitialRiskPts;
					SumRewardPts += t.InitialRewardPts;
					if(t.PnL>0) Wins++;
					if(t.PnL<0) Losses++;
					if(t.Disposition == "BE") {
						BEs++;
						BEDlrs += t.PnL * Instrument.MasterInstrument.PointValue;
					}else if(t.PnL<0)//SL hit, or EOD hit in a losing position
						LossDlrs += t.PnL * Instrument.MasterInstrument.PointValue;
					else if(t.PnL>0)//win without being a BE, or an EOD hit in a profitable position
						WinDlrs += t.PnL * Instrument.MasterInstrument.PointValue;

					double wpct = Wins / (Wins+Losses);
					Print((t.PnL>0 ? "Win\t":"Los\t")+t.EntryDT.ToString()+"  \t"+t.Direction+"   \t"+t.PnL+"  "+PnLDlr.ToString("C")+"\t  "+wpct.ToString("0%")+"\t "+t.Disposition);
					if(t.ExitABar>0) Draw.Line(this,t.EntryDT.ToString(), false, t.EntryDT, t.EntryPrice, Times[0].GetValueAt(t.ExitABar), t.ExitPrice, t.PnL>0 ? Brushes.Lime:Brushes.Red, DashStyleHelper.Dash, 2);
				}
				Print("Avg trade: "+(PnLDlr / (Wins+Losses)).ToString("C"));
				Print("\nAvg risk: "+(SumRiskPts * Instrument.MasterInstrument.PointValue / Trades.Count).ToString("C"));
				Print("Avg reward: "+(SumRewardPts * Instrument.MasterInstrument.PointValue / Trades.Count).ToString("C"));
				Print("\nW/L/BE:  "+(Wins-BEs)+" / "+Losses+" / "+BEs);
				Print("Avg profitable: "+((BEDlrs+WinDlrs)/Wins).ToString("C"));
				Print("Avg loser: "+(LossDlrs/Losses).ToString("C"));
				#endregion
			}
		}
		//=====================================================================================
		private double Round2Tick(double p){ 
			long pi = 0;
			try{
				pi = Convert.ToInt64(p/TickSize); return Convert.ToDouble(pi)*TickSize;
			}catch{}
			return p;
		}
		//=====================================================================================
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Day ATR period", Order=10, GroupName="Parameters")]
		public int ATR_period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 1)]
		[Display(Name="Reduction Mult", Order=20, GroupName="Parameters", Description="Expressed as a decimal...0.6 = 60%")]
		public double pReductionMult
		{ get; set; }

		[Display(Name="RewardRisk Mult", Order=30, GroupName="Parameters", Description="Set to 0 to turn-off trades")]
		public double pRewardRiskMult
		{ get; set; }

		[Display(Name="Developing levels?", Order=40, GroupName="Parameters", Description="")]
		public bool pDevelopingLevels
		{ get; set; }

		[Display(Name="Reset time", Order=50, GroupName="Parameters", Description="")]
		public ATRForecaster_ResetTimes pResetTime
		{ get; set; }

		[Display(Name="Reset TOD", Order=60, GroupName="Parameters", Description="")]
		public int pResetTOD
		{get;set;}

		[Range(0,100)]
		[Display(Name="Midregion Opacity", Order=70, GroupName="Parameters", Description="Opacity of the midregion")]
		public int pMidOpacity
		{get;set;}
		#endregion

	}
}
public enum ATRForecaster_ResetTimes {Midnight, NewSession, Custom}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATRForecaster[] cacheATRForecaster;
		public ATRForecaster ATRForecaster(int aTR_period, double pReductionMult)
		{
			return ATRForecaster(Input, aTR_period, pReductionMult);
		}

		public ATRForecaster ATRForecaster(ISeries<double> input, int aTR_period, double pReductionMult)
		{
			if (cacheATRForecaster != null)
				for (int idx = 0; idx < cacheATRForecaster.Length; idx++)
					if (cacheATRForecaster[idx] != null && cacheATRForecaster[idx].ATR_period == aTR_period && cacheATRForecaster[idx].pReductionMult == pReductionMult && cacheATRForecaster[idx].EqualsInput(input))
						return cacheATRForecaster[idx];
			return CacheIndicator<ATRForecaster>(new ATRForecaster(){ ATR_period = aTR_period, pReductionMult = pReductionMult }, input, ref cacheATRForecaster);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATRForecaster ATRForecaster(int aTR_period, double pReductionMult)
		{
			return indicator.ATRForecaster(Input, aTR_period, pReductionMult);
		}

		public Indicators.ATRForecaster ATRForecaster(ISeries<double> input , int aTR_period, double pReductionMult)
		{
			return indicator.ATRForecaster(input, aTR_period, pReductionMult);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATRForecaster ATRForecaster(int aTR_period, double pReductionMult)
		{
			return indicator.ATRForecaster(Input, aTR_period, pReductionMult);
		}

		public Indicators.ATRForecaster ATRForecaster(ISeries<double> input , int aTR_period, double pReductionMult)
		{
			return indicator.ATRForecaster(input, aTR_period, pReductionMult);
		}
	}
}

#endregion
