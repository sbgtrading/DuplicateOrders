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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BreakoutResearchTool : Indicator
	{
		private const int LONG = 1;
		private const int SHORT = -1;
		private const byte SUMMARY_TABLE = 1;
		private const byte EXAMINE_TABLE = 2;
		private const byte HIDE_ALL_TABLES = 3;

		#region -- class definitions --
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
			public SortedDictionary<int, double> StopLevels = new SortedDictionary<int,double>();//abar is key, stop price is double
			public info (char dir, double entryprice, int entryABar, int signalABar, DateTime entryDT, double targetPrice, double slPrice, double contracts, double dayHigh, double dayLow){
				Direction=dir; EntryPrice=entryprice; EntryABar=entryABar; SignalABar=signalABar; EntryDT = entryDT; TargetPrice = targetPrice; SLPrice = slPrice; 
				BETriggerPrice = (TargetPrice+SLPrice)/2;
				Contracts = Math.Max(1,contracts);
				LocPercentileInDailyRange = (EntryPrice-dayLow)/(dayHigh-dayLow);
				StopLevels[entryABar] = SLPrice;
			}
		}
		private SortedDictionary<int,List<info>> Trades = new SortedDictionary<int,List<info>>();//time is the key, list of trades is the value

		private class BTinfo{
			public int TimeKey = -1;
			public double WinsL     = 0;
			public double WinsS     = 0;
			public double LossesL   = 0;
			public double LossesS   = 0;
			public double PnLDlrs   = 0;
			public double WinDlrsL  = 0;
			public double WinDlrsS  = 0;
			public double LossDlrsL = 0;
			public double LossDlrsS = 0;
			public double ExpectancyDlrs   = 0;
			public double AvgLongWinDlrs   = 0;
			public double AvgShortWinDlrs  = 0;
			public double AvgLongLossDlrs  = 0;
			public double AvgShortLossDlrs = 0;
			public double TgtsHit = 0;
			public double SLsHit = 0;
			public double EODsHit = 0;

			public string LongDetails = string.Empty;
			public string ShortDetails = string.Empty;
			public string OutStr         = string.Empty;
			public List<string> Cols     = new List<string>();
//res[pct] = new double[]{kvp.Key, LongW, ShortW, LongL, ShortL, pnlLongWinners*POINT_VALUE, pnlShortWinners*POINT_VALUE, pnlLongLosers*POINT_VALUE, pnlShortLosers*POINT_VALUE};
//BestTrades[Int2TimeStr(Convert.ToInt32(kvp.Value[0]))] = new BTinfo(kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], Int2TimeStr(Convert.ToInt32(kvp.Value[0])), kvp.Value[3], kvp.Value[4],this);
			public BTinfo(double longw, double shortw, double longl, double shortl, string time, double LocPercentile, double longWinnersDlrs, double shortWinnersDlrs, double longLosersDlrs, double shortLosersDlrs, double tgtsHit, double slsHit, double eodsHit, Indicator parent){
				TimeKey   = Convert.ToInt32(time.Replace(":",string.Empty));
				WinsL     = longw;
				LossesL   = longl;
				WinsS     = shortw;
				LossesS   = shortl;
				WinDlrsL  = longWinnersDlrs;
				WinDlrsS  = shortWinnersDlrs;
				LossDlrsL = longLosersDlrs;
				LossDlrsS = shortLosersDlrs;
				PnLDlrs   = longWinnersDlrs + longLosersDlrs + shortWinnersDlrs + shortLosersDlrs;
				if(WinsL+WinsS+LossesL+LossesS > 0){
					double w = longw+shortw;
					double l = longl+shortl;
					double Longwpct  = longw+longl > 0 ? longw/(longw+longl) : 0;
					double Shortwpct = shortw+shortl > 0 ? shortw/(shortw+shortl) : 0;
					AvgLongWinDlrs   = longw>0 ? WinDlrsL/longw : 0;
					AvgLongLossDlrs  = longl>0 ? LossDlrsL/longl : 0;
					AvgShortWinDlrs  = shortw>0 ? WinDlrsS/shortw : 0;
					AvgShortLossDlrs = shortl>0 ? LossDlrsS/shortl : 0;
					double AvgWinDlrs  = (w>0) ? (WinDlrsL+WinDlrsS) / w : 0;
					double AvgLossDlrs = (l>0) ? (LossDlrsL+LossDlrsS) / l : 0;
					double wpct      = w+l>0 ? (longw+shortw) / (w+l) : 0;
					double wpctLong  = longw+longl>0 ? (longw) / (longw+longl) : 0;
					double wpctShort = shortw+shortl>0 ? (shortw) / (shortw+shortl) : 0;

					ExpectancyDlrs = wpct * AvgWinDlrs + (1.0-wpct)*AvgLossDlrs;
					double AvgDlrs = PnLDlrs/(w+l);
//					parent.Print(PnLDlrs.ToString("C")+" with "+(w+l)+"-trades = avg: "+AvgDlrs.ToString("C"));//+"    "+wpct.ToString()+": "+AvgWinDlrs.ToString("C")+"   "+(1-wpct).ToString()+" "+AvgLossDlrs.ToString("C")+"  Expectancy: "+ExpectancyDlrs.ToString("C"));
//					parent.Print("       WinDlr: "+WinnersDlrs+@"/"+w+"    LossDlr: "+LosersDlrs+@"/"+l);
					Cols.Clear();
					Cols.Add(wpct.ToString("0%"));
					Cols.Add(string.Format("{0}-{1} ({2}-{3})", w, l, tgtsHit, slsHit, eodsHit));
					Cols.Add(time);
					Cols.Add(LocPercentile==999? "N/A" : Math.Truncate(LocPercentile*10).ToString("0").Replace("0",string.Empty));//this is the location percentile
					Cols.Add(string.Format("{0}", PnLDlrs.ToString("0").Replace(".00",string.Empty)));
					Cols.Add(string.Format("{0}", AvgDlrs.ToString("C")));
					Cols.Add(string.Format("{0} {1}/{2}", wpctLong.ToString("0%"), longw, longl));
					Cols.Add(string.Format("{0}", (WinDlrsL+LossDlrsL).ToString("0").Replace(".00",string.Empty)));
					Cols.Add(string.Format("{0} {1}/{2}", wpctShort.ToString("0%"), shortw, shortl));
					Cols.Add(string.Format("{0}", (WinDlrsS+LossDlrsS).ToString("0").Replace(".00",string.Empty)));
					//Cols.Add(ExpectancyDlrs.ToString("0"));
					LongDetails = string.Format("{0} Long:{1} {2} avg: {3}", Cols[0], Cols[6], (WinDlrsL+LossDlrsL).ToString("C").Replace(".00",string.Empty), ((WinDlrsL+LossDlrsL)/(longw+longl)).ToString("C"));
					ShortDetails = string.Format("{0} Short:{1} {2} avg: {3}", Cols[0], Cols[8], (WinDlrsS+LossDlrsS).ToString("C").Replace(".00",string.Empty), ((WinDlrsS+LossDlrsS)/(shortw+shortl)).ToString("C"));
					OutStr = string.Format("{0} {1}   {2}  ${3}  avg {4} Long:{5} {6} Short:{7} {8}", Cols[0], Cols[1], Cols[2], Cols[3], Cols[5], Cols[6], Cols[7], Cols[8], Cols[9]);
				}else{
					OutStr = "no trades";
				}
			}
		}
		#endregion

		private class CloseDayCloseBar {
			public double DayClose = 0; 
			public double DiffToClose = 0;//+ means it's above the close, - means its below the close
			public CloseDayCloseBar (double dayclose) {DayClose = dayclose;}
		}
		private class BarToCloseData {
			public SortedDictionary<DateTime, CloseDayCloseBar> Data;
			public double TimesAboveClose = 0;
			public double TimesBelowClose = 0;
			public double PctAboveClose = 0;
			public BarToCloseData(DateTime dt, double DayClose){ 
				Data = new SortedDictionary<DateTime, CloseDayCloseBar>();
				Data[dt] = new CloseDayCloseBar(DayClose);
			}
		}
		private SortedDictionary<int,BarToCloseData> BarToClose = new SortedDictionary<int,BarToCloseData>();
		
		private SortedDictionary<DateTime,int[]> HOSLOS = new SortedDictionary<DateTime,int[]>();
		private int AvgHOS = 0;
		private int AvgLOS = 0;
		private string AvgHOS_str = "";
		private string AvgLOS_str = "";

		private int line=113;
		private SortedDictionary<string,BTinfo>	BestTrades = new SortedDictionary<string,BTinfo>();//key=Time, Value = trade data/description
		private SortedDictionary<int,string>	Map_Abar_ToTime = new SortedDictionary<int,string>();//key=abar, value = time str "15:45" is 15:45
		private SortedDictionary<int,double>	Map_Abar_ToTgtReduction = new SortedDictionary<int,double>();//key=abar, value = target pct reduction value for that time
		private List<int>						OutOfSessionBar = new List<int>();
		private List<DateTime>					DatesTraded = new List<DateTime>();
		private SortedDictionary<int,double>	MidRangePrice = new SortedDictionary<int,double>();
		private SortedDictionary<DateTime,double> DailySMA_20 = new SortedDictionary<DateTime,double>();
		private SortedDictionary<DateTime,double> DailyRSI_3 = new SortedDictionary<DateTime,double>();
		private List<DayOfWeek>					  ValidDaysOfWeek = new List<DayOfWeek>();
		private int     MouseABar = 0;
		private double  MousePrice = 0;
		private float   MousePricePixel = 0;
		private Account myAccount;
bool tempb = true;
		private int tnow      = 0;
		private int LastExaminedKey;
		private SortedDictionary<int,string>		SummaryTable = new SortedDictionary<int,string>();
		private SortedDictionary<int,List<string>>	ExamineTable = new SortedDictionary<int,List<string>>();
		private bool   isdebug  = false;
		private double mid    = 0;
		private double NoMansLandPts = 0;
		private double DailyH = 0;
		private double DailyL = 0;
		private double POINT_VALUE = 0;
		private byte   DisplayedContent = SUMMARY_TABLE;
		private int    AlertABar = 0;
		private int    EntryHitAlertABar = 0;
		private double avgDailyRange=0;
		private double SetEntryPrice = double.MinValue;//when the entry price is discovered in the OnRender method, this price is set and EntryLevel[0] is assigned in the OnBarUpdate method.  Plots cannot be updated in the OnRender method
		private double PriorClose = 0;
		private double SuggestedPositionSize = 1;
		private string InstrumentDescription = string.Empty;
		private bool IsRealTime = false;

		private DateTime now;
		private bool IsValidLicense = false;
private bool pExcludeNoMansLand = false;

		private class MyRSI{
			#region -- MyRSI --
			public string Status = null;
			private List<double> down = new List<double>();
			private List<double> up   = new List<double>();
			private List<double> Avg  = new List<double>();
			private List<double> avgDown = new List<double>();
			private List<double> avgUp   = new List<double>();
			private List<double> Prices  = new List<double>();
			private int Period=7;
			private int LastABar = 0;
			private double constant1 = 0;
			private double constant2 = 0;
			private double constant3 = 0;
			public MyRSI(int period){
				Period = period;
				int Smooth = 3;
				constant1 = 2.0 / (1 + Smooth);
				constant2 = (1 - (2.0 / (1 + Smooth)));
				constant3 = (Period - 1);
			}

			public double CalcRSI(int CurrentBar, double price){

				if(CurrentBar != LastABar){
					down.Insert(0,0);
					up.Insert(0,0);
					Avg.Insert(0,0);
					Prices.Insert(0,price);
					avgUp.Insert(0,0);
					avgDown.Insert(0,0);
				}
				double value0=0;
				string status = string.Empty;
try{
				Prices[0] = price;
				LastABar = CurrentBar;
				while(down.Count>Period) down.RemoveAt(down.Count-1);
				while(up.Count>Period)   up.RemoveAt(up.Count-1);
				while(Avg.Count>Period)  Avg.RemoveAt(Avg.Count-1);
				while(avgUp.Count>Period)   avgUp.RemoveAt(avgUp.Count-1);
				while(avgDown.Count>Period) avgDown.RemoveAt(avgDown.Count-1);
				while(Prices.Count>Period)  Prices.RemoveAt(Prices.Count-1);
status = "101";
				if (Prices.Count<Period)
				{
	//				if (Period < 3)	Avg[0] = 50;
					return 50;
				}
				double input0	= Prices[0];
				double input1	= Prices[1];
				down[0]			= Math.Max(input1 - input0, 0);
				up[0]			= Math.Max(input0 - input1, 0);

status = "113";
				if (CurrentBar + 1 == Period)
				{
					// First averages
					avgDown[0]	= down.Average();
					avgUp[0]	= up.Average();
				}
				else
				{
status = "122";
					// Rest of averages are smoothed
					avgDown[0]	= (avgDown[1] * constant3 + down[0]) / Period;
					avgUp[0]	= (avgUp[1] * constant3 + up[0]) / Period;
				}

status = "128";
				value0	= avgDown[0] == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown[0]);
				//Avg[0]		= constant1 * value0 + constant2 * Avg[1];
status = "132";
}catch{Status = status;}
				return value0;
			}
			#endregion --
		}
		private MyRSI dailyRSI_3=null;

//===========================================================================================================
		protected override void OnStateChange()
		{
			try{
			if (State == State.SetDefaults)
			{
				Description									= @"Calc the win/loss rate for each historical breakout bar signals";
				Name										= "Breakout Research Tool";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				IsAutoScale									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				pMinWinCount     = 100;
				pTicksTarget     = 0;
				pExamineTime     = -1;
				pRangeType = BreakoutResearchTool_RangeType.Daily;
				pTargetType = BreakoutResearchTool_TargetType.MultOfRange;
				pMultRangeTarget = 0.3;
				pSL_RangeMult = 0.1;
				pTicksSLBuffer   = 0;
				pEngageBEStop    = false;
				pEngageTrailingStop = false;
				pExcludeNoMansLand = false;
				pStartTime   = 600;
				pEndTime     = 1600;
				pGoFlatTime  = 1610;
				pIgnoreDatesCSV = "1/17/22, 7/4/22, 11/24/22, 12/25/22";
				pDaysOfWeek  = "Today";
				pMaxBarsWaitingForEntry    = 2;
				Filter_BarClosingDirection = false;
				pShowHistoricalExitLevels = true;
				pMinAvgDollarsPerTrade = 60;
				pAccountSize = 150000;
				pRiskPerTrade = 0.005;
				pMaxContracts = 1;
				pAlertWAV = "none";
				pEntryLevelHitWAV = "Alert2.wav";
				pShowTrendBars = false;
				Filter_MidDayPrice_BuyBelowSellAbove = true;
				Filter_MidDayPrice_BuyAboveSellBelow = false;
				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Hash, "TargetLevel");
				AddPlot(new Stroke(Brushes.DimGray, 2), PlotStyle.Hash, "Mid");
				AddPlot(new Stroke(Brushes.RoyalBlue, 2), PlotStyle.Hash, "EntryLevel");
				AddPlot(new Stroke(Brushes.DarkCyan, 2), PlotStyle.Hash, "Hist ExitPrice");
				isdebug = System.IO.File.Exists(@"c:\222222222222.txt");
			}
			else if (State == State.Configure){
				#region -- configure --
				AddDataSeries(BarsPeriodType.Day,1);

				dailyRSI_3 = new MyRSI(3);

				pDaysOfWeek = pDaysOfWeek.ToUpper();
				if(pDaysOfWeek.Contains("ALL") || pDaysOfWeek.Trim().Length==0)
					pDaysOfWeek = "ALL";
				else if(pDaysOfWeek != "TODAY"){
					SortedDictionary<int,byte> DOW = new SortedDictionary<int,byte>();
					if(pDaysOfWeek.Contains("1"))  DOW[0] = 1;//'1' is Sunday
					if(pDaysOfWeek.Contains("2"))  DOW[1] = 1;//'2' is Monday
					if(pDaysOfWeek.Contains("3"))  DOW[2] = 1;
					if(pDaysOfWeek.Contains("4"))  DOW[3] = 1;
					if(pDaysOfWeek.Contains("5"))  DOW[4] = 1;
					if(pDaysOfWeek.Contains("6"))  DOW[5] = 1;
					if(pDaysOfWeek.Contains("7"))  DOW[6] = 1;
					if(pDaysOfWeek.Contains("SU")) DOW[0] = 1;
					if(pDaysOfWeek.Contains("M"))  DOW[1] = 1;
					if(pDaysOfWeek.Contains("TU")) DOW[2] = 1;
					if(pDaysOfWeek.Contains("W"))  DOW[3] = 1;
					if(pDaysOfWeek.Contains("TH")) DOW[4] = 1;
					if(pDaysOfWeek.Contains("F"))  DOW[5] = 1;
					if(pDaysOfWeek.Contains("S"))  DOW[6] = 1;
					pDaysOfWeek = string.Empty;
					if(DOW.ContainsKey(0)) pDaysOfWeek = "Su";
					if(DOW.ContainsKey(1)) pDaysOfWeek = pDaysOfWeek+" M";
					if(DOW.ContainsKey(2)) pDaysOfWeek = pDaysOfWeek+" Tu";
					if(DOW.ContainsKey(3)) pDaysOfWeek = pDaysOfWeek+" W";
					if(DOW.ContainsKey(4)) pDaysOfWeek = pDaysOfWeek+" Th";
					if(DOW.ContainsKey(5)) pDaysOfWeek = pDaysOfWeek+" F";
					if(DOW.ContainsKey(6)) pDaysOfWeek = pDaysOfWeek+" Sa";
					pDaysOfWeek = pDaysOfWeek.Trim();
				}
				#endregion
			}
			else if (State == State.DataLoaded)
			{
//pStartTime   = 300;
//pEndTime     = 900;
//pGoFlatTime  = 910;

				IsValidLicense = DateTime.Now < new DateTime(2022,7,1,0,0,0);
				if(!IsValidLicense && !isdebug){
					Draw.TextFixed(this, "license", "BreakoutResearchTool license expired.\nContact Ben for new license", TextPosition.Center);
					return;
				}

				try
				{
					myAccount = this.ChartControl.OwnerChart.ChartTrader.Account;
					Print("MyAccount: "+myAccount.Name);
					Print("ConnStatus: "+myAccount.ConnectionStatus.ToString());
				}
				catch
				{
				}
ClearOutputWindow();
				var DateAndClosePrice = new SortedDictionary<DateTime, double>();
//				if(pRangeType == BreakoutResearchTool_RangeType.Daily){
					double sumR = 0;
					List<double> sma = new List<double>();
					for(int abar = 1; abar<BarsArray[1].Count; abar++) {//The [1] element is a Daily datafeed
						sumR = sumR + BarsArray[1].GetHigh(abar) - BarsArray[1].GetLow(abar);
						var dt = BarsArray[1].GetTime(abar).Date;
						DailyRSI_3[dt] = dailyRSI_3.CalcRSI(abar, BarsArray[1].GetClose(abar-1));//Use the prior close so that the current day is working off the prior close.
//Print(dt.ToString()+"  rsi: "+DailyRSI_3[dt].ToString());
						if(sma.Count>0) {
							DailySMA_20[dt] = sma.Average();//calc the average of the prior 20-bars, do not use the current day close in this averaging.
						}
						sma.Add(BarsArray[1].GetClose(abar));
						while(sma.Count>20) sma.RemoveAt(0);
					}
					avgDailyRange = sumR/BarsArray[1].Count;
//				}
				InstrumentDescription = Instrument.MasterInstrument.Description;
				if(InstrumentDescription.Contains("Futures")) InstrumentDescription = InstrumentDescription.Replace("Futures"," ("+Instrument.FullName+")");
				else InstrumentDescription = string.Format("{0}  ({1})",InstrumentDescription, Instrument.FullName);

				#region -- DataLoaded --
				if(ChartPanel!=null){
					ChartPanel.MouseMove  += OnMouseMove;
					ChartPanel.KeyUp += ChartPanel_KeyUp;
				}
				#region -- Compile list of ValidDaysOfWeek --
				if(this.pDaysOfWeek.ToUpper().Contains("ALL")){
					this.ValidDaysOfWeek.Add(DayOfWeek.Sunday);
					this.ValidDaysOfWeek.Add(DayOfWeek.Monday);
					this.ValidDaysOfWeek.Add(DayOfWeek.Tuesday);
					this.ValidDaysOfWeek.Add(DayOfWeek.Wednesday);
					this.ValidDaysOfWeek.Add(DayOfWeek.Thursday);
					this.ValidDaysOfWeek.Add(DayOfWeek.Friday);
					this.ValidDaysOfWeek.Add(DayOfWeek.Saturday);
				}else if (pDaysOfWeek.Contains("TODAY")) {
					ValidDaysOfWeek.Add(Times[0].GetValueAt(Bars.Count-1).DayOfWeek);
				}else{
					if(this.pDaysOfWeek.ToUpper().Contains("SU")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Sunday);
					}
					if(this.pDaysOfWeek.ToUpper().Contains("M")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Monday);
					}
					if(this.pDaysOfWeek.ToUpper().Contains("TU")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Tuesday);
					}
					if(this.pDaysOfWeek.ToUpper().Contains("W")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Wednesday);
					}
					if(this.pDaysOfWeek.ToUpper().Contains("TH")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Thursday);
					}
					if(this.pDaysOfWeek.ToUpper().Contains("F")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Friday);
					}
					if(this.pDaysOfWeek.ToUpper().Contains("SA")){
						this.ValidDaysOfWeek.Add(DayOfWeek.Saturday);
					}
				}
				#endregion

				POINT_VALUE = Instrument.MasterInstrument.PointValue;
				if(Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock)
					POINT_VALUE = Instrument.MasterInstrument.PointValue * 100;

int xt = Convert.ToInt32(Math.Truncate(pStartTime/100.0));
var a = new TimeSpan(xt,pStartTime-xt*100,0); 
xt = Convert.ToInt32(Math.Truncate(pEndTime/100.0));
var b = new TimeSpan(xt,pEndTime-xt*100,0); 
var hoursPerSession = b.TotalHours-a.TotalHours;

				mid = 0;
if(pExcludeNoMansLand) NoMansLandPts = 0;
				DailyH = 0;
				DailyL = 0;
				double SessionH = 0;
				double SessionL = 0;
				int priorTT = 999999;
				OutOfSessionBar.Clear();
				int BarInSession = 0;
				double BarsPerSession = hoursPerSession * 60.0 / Convert.ToDouble(Bars.BarsPeriod.Value);
				var DaysToSkip = new List<DateTime>();
				foreach(string s in pIgnoreDatesCSV.Split(new char[]{' ',','}, StringSplitOptions.RemoveEmptyEntries)){
					try{
						DaysToSkip.Add(DateTime.Parse(s).Date);
						if(DaysToSkip.Last() > Times[0].GetValueAt(1)) Print("Skipping "+DaysToSkip.Last().ToString());
					}catch{}
				}

				for(int abar = 2; abar<BarsArray[0].Count; abar++){
					var time0 = Times[0].GetValueAt(abar);
					if(time0.Date == DateTime.Now.Date) break;//don't use current day win/loss data for the calculation of predictions
//					if(pIgnoreDatesCSV.Contains(time0.ToString("M/dd/yy"))) continue;
					if(DaysToSkip.Contains(time0.Date)) continue;
					int tt = ToTime(time0)/100;
					if(!HOSLOS.ContainsKey(time0.Date)) HOSLOS[time0.Date] = new int[2]{-1,-1};
bool z = time0.Month==4 && time0.Day==24 && tt>800 && tt<1000;
					double H = Highs[0].GetValueAt(abar);
					double L = Lows[0].GetValueAt(abar);
					double C = Closes[0].GetValueAt(abar);
					double O = Opens[0].GetValueAt(abar);
					bool InSession = tt >= pStartTime && tt <= pEndTime;
					if((HOSLOS[time0.Date][0]==-1 || HOSLOS[time0.Date][1]==-1) && InSession) {
						if(HOSLOS[time0.Date][0]==-1) HOSLOS[time0.Date][0] = ToJulianMinute(tt);
						if(HOSLOS[time0.Date][1]==-1) HOSLOS[time0.Date][1] = ToJulianMinute(tt);
						SessionH = H;
						SessionL = L;
					}
					if(DailyH==0 || Bars.IsFirstBarOfSessionByIndex(abar)){//tt < priorTT){
if(z)Print("First session bar at "+time0.ToString());
						DailyH=H;
						DailyL=L;
					}else{
						if(H>SessionH) HOSLOS[time0.Date][0] = ToJulianMinute(tt);
						if(L<SessionL) HOSLOS[time0.Date][1] = ToJulianMinute(tt);
						DailyH = Math.Max(H,DailyH);
						DailyL = Math.Min(L,DailyL);
						SessionH = Math.Max(H,SessionH);
						SessionL = Math.Min(L,SessionL);
						mid = (DailyH+DailyL)/2;
if(z)Print("DailyH: "+DailyH+"  L: "+DailyL+"  mid: "+mid);
					}
					if(Filter_MidDayPrice_BuyBelowSellAbove || Filter_MidDayPrice_BuyAboveSellBelow) {
						MidRangePrice[abar]=mid;
					}

					if(tt >= pGoFlatTime && priorTT <= pGoFlatTime && ValidDaysOfWeek.Contains(time0.DayOfWeek)) {
						DateAndClosePrice[time0.Date] = C;
					}

					priorTT = tt;
					if(tt <= pStartTime || tt >= pEndTime) {
						OutOfSessionBar.Add(abar);
						BarInSession = 0;
						continue;
					}
					BarInSession++;
					double TgtReducePerBar = BarInSession / BarsPerSession;
					Map_Abar_ToTgtReduction[tt] = TgtReducePerBar;

					if(H==L) continue;//ignore key bars with no range
if(z)Print("------------------------------  "+time0.ToString());
					if(!Trades.ContainsKey(tt)) Trades[tt] = new List<info>();
					int Pos = 0;
					double range = 0;
					for(int x = abar+1; x<Bars.Count; x++){
						double xH = Highs[0].GetValueAt(x);
						double xL = Lows[0].GetValueAt(x);
//						if(xH>=H && xL<=L){
//							Trades[tt].Insert(0,new info('x',0));
//							Trades[tt][0].PnL = -(H-L);//complete loss, price broke out in both directions
//Print(x+"  immediate loss");
//							break;
//						}
						if(Pos==0 && ValidDaysOfWeek.Contains(time0.DayOfWeek)){
							if(x - abar > pMaxBarsWaitingForEntry) break;//pull your entry order after too many bars occurred
							#region -- Apply filters --
							int PermittedDir = GetPermittedDirection(C, O, mid, Medians[0].GetValueAt(abar));
							#endregion ----------------------------------------
if(z) Print("PermittedDir: "+PermittedDir);
							if(PermittedDir != int.MinValue){//if this is minvalue, then trades are not permitted
								if(pRangeType==BreakoutResearchTool_RangeType.Daily) range = avgDailyRange; else range = (H-L);

								double tdist = (pMultRangeTarget > 0? range*pMultRangeTarget : pTicksTarget*TickSize);

								if(xH > H && PermittedDir >= 0){//long or either (0 permits both long and short)
									double EntryPrice = Round2Tick(H+TickSize);
									var SLPrice   = CalculateInitialSL('L', EntryPrice, range);
									var contracts = CalculateContracts(EntryPrice, SLPrice);
									if(contracts>0){
										var time = Times[0].GetValueAt(x);
										var TgtPrice = Round2Tick(EntryPrice + (pRangeType==BreakoutResearchTool_RangeType.Daily ? GetTargetDistance(tdist/3, tdist, TgtReducePerBar) : tdist));
										Trades[tt].Insert(0,new info('L', EntryPrice, x, abar, time, TgtPrice, SLPrice, contracts, DailyH,DailyL));
if(z)Print("Going long at "+EntryPrice+"  SL at: "+SLPrice);
										Pos = LONG;
										if(!DatesTraded.Contains(time.Date)) DatesTraded.Add(time.Date);
									}
								}
								else if(xL < L && PermittedDir == SHORT){
									double EntryPrice = Round2Tick(L-TickSize);
									var SLPrice   = CalculateInitialSL('S', EntryPrice, range);
if(z)Print("Going short at "+EntryPrice+"  SL at: "+SLPrice);
									var contracts = CalculateContracts(EntryPrice, SLPrice);
									if(contracts>0){
										var time = Times[0].GetValueAt(x);
										var TgtPrice = Round2Tick(EntryPrice - (pRangeType==BreakoutResearchTool_RangeType.Daily ? GetTargetDistance(tdist/3, tdist, TgtReducePerBar) : tdist));
										Trades[tt].Insert(0,new info('S', EntryPrice, x, abar, time, TgtPrice, SLPrice, contracts, DailyH,DailyL));
										Pos = SHORT;
										if(!DatesTraded.Contains(time.Date)) DatesTraded.Add(time.Date);
									}
								}
							}
						}
						if(Pos == LONG){
							int xT = ToTime(Times[0].GetValueAt(x))/100;
							if(xT>=pGoFlatTime || Bars.IsFirstBarOfSessionByIndex(x)){//go flat at the close price of the session
								Trades[tt][0].ExitPrice = Round2Tick(Closes[0].GetValueAt(x-1));
								Trades[tt][0].ExitABar = x-1;
								Trades[tt][0].PnL = (Trades[tt][0].ExitPrice - Trades[tt][0].EntryPrice) * Trades[tt][0].Contracts;
								Trades[tt][0].Disposition = "EOD";//exit at end of day
if(z) Print("End of session hit...exiting at "+Times[0].GetValueAt(x-1)+"  pnl: "+Trades[tt][0].PnL);
								break;
							}
if(z) Print("we're long, checking for exit on bar "+Times[0].GetValueAt(x)+"  SL at "+Instrument.MasterInstrument.FormatPrice(Trades[tt][0].SLPrice)+"  Tgt at "+Instrument.MasterInstrument.FormatPrice(Trades[tt][0].TargetPrice)+"  H: "+xH);
							if(xH > Trades[tt][0].TargetPrice){
if(z) Print("we're long, Target hit! "+Times[0].GetValueAt(x));
								Trades[tt][0].ExitPrice = Trades[tt][0].TargetPrice;
								Trades[tt][0].ExitABar = x;
								Trades[tt][0].PnL = (Trades[tt][0].ExitPrice - Trades[tt][0].EntryPrice) * Trades[tt][0].Contracts;
								Trades[tt][0].Disposition = "Tgt";//exit at target
								break;
							}
							else if(xL < Trades[tt][0].SLPrice){
if(z) Print("we're long, SL HIT "+Times[0].GetValueAt(x));
								Trades[tt][0].ExitPrice = Trades[tt][0].SLPrice;
								Trades[tt][0].ExitABar = x;
								Trades[tt][0].PnL = (Trades[tt][0].ExitPrice - Trades[tt][0].EntryPrice) * Trades[tt][0].Contracts;
								Trades[tt][0].Disposition = "SL";//exit at SL
								break;
							}
//							if(pEngageBEStop && Lows[0].GetValueAt(x) > Trades[tt][0].EntryPrice){//put stop to BE when we have 1 bars low above the entry price
							if(pEngageBEStop){
								if(Highs[0].GetValueAt(x) > Trades[tt][0].BETriggerPrice){
									Trades[tt][0].SLPrice = Trades[tt][0].EntryPrice+TickSize;
									Trades[tt][0].BEStopEngaged = true;
if(z) Print(Times[0].GetValueAt(x).ToString()+"  "+tt+"  Move to BE+1 for long trade to "+Trades[tt][0].SLPrice);
									Trades[tt][0].StopLevels[x] = Trades[tt][0].SLPrice;
								}
							}
							if(x>3 && pEngageTrailingStop && Lows[0].GetValueAt(x) > Trades[tt][0].EntryPrice){// && Trades[tt][0].SLPrice < Trades[tt][0].EntryPrice){//trail the stop while it's below EntryPrice
								Trades[tt][0].SLPrice = Math.Max(Trades[tt][0].SLPrice, Math.Min(Lows[0].GetValueAt(x-2), Lows[0].GetValueAt(x-1)));
								Trades[tt][0].TrailStopEngaged = true;
if(z) Print(Times[0].GetValueAt(x).ToString()+"  "+tt+"  Trail stop up for long trade to "+Trades[tt][0].SLPrice+"   L2: "+Lows[0].GetValueAt(x-2)+"  L1: "+Lows[0].GetValueAt(x-1));
								Trades[tt][0].StopLevels[x] = Trades[tt][0].SLPrice;
							}
						}
						else if(Pos == SHORT){
							int xT = ToTime(Times[0].GetValueAt(x))/100;
							if(xT>=pGoFlatTime || Bars.IsFirstBarOfSessionByIndex(x)){//go flat at the close price of th session
								Trades[tt][0].ExitPrice = Closes[0].GetValueAt(x-1);
								Trades[tt][0].ExitABar = x-1;
								Trades[tt][0].PnL = (Trades[tt][0].EntryPrice - Trades[tt][0].ExitPrice) * Trades[tt][0].Contracts;
								Trades[tt][0].Disposition = "EOD";//exit at end of day
								break;
							}
							if(xL < Trades[tt][0].TargetPrice){
								Trades[tt][0].ExitPrice = Trades[tt][0].TargetPrice;
								Trades[tt][0].ExitABar = x;
								Trades[tt][0].PnL = (Trades[tt][0].EntryPrice - Trades[tt][0].ExitPrice) * Trades[tt][0].Contracts;
								Trades[tt][0].Disposition = "Tgt";//exit at target
								break;
							}
							else if(xH > Trades[tt][0].SLPrice){
								Trades[tt][0].ExitPrice = Trades[tt][0].SLPrice;
								Trades[tt][0].ExitABar = x;
								Trades[tt][0].PnL = (Trades[tt][0].EntryPrice - Trades[tt][0].ExitPrice) * Trades[tt][0].Contracts;
								Trades[tt][0].Disposition = "SL";//exit at SL
								break;
							}
//							if(pEngageBEStop && Highs[0].GetValueAt(x) < Trades[tt][0].EntryPrice){//put stop to BE when we have 1 bars high below the entry price
//								Trades[tt][0].SLPrice = Trades[tt][0].EntryPrice-TickSize;
//								Trades[tt][0].BEStopEngaged = true;
//							}
							if(pEngageBEStop){
								if(Lows[0].GetValueAt(x) < Trades[tt][0].BETriggerPrice){
									Trades[tt][0].SLPrice = Trades[tt][0].EntryPrice-TickSize;
									Trades[tt][0].BEStopEngaged = true;
if(z) Print(Times[0].GetValueAt(x).ToString()+"  "+tt+"  Move to BE+1 for short trade to "+Trades[tt][0].SLPrice);
									Trades[tt][0].StopLevels[x] = Trades[tt][0].SLPrice;
								}
							}
							if(x>3 && pEngageTrailingStop && Highs[0].GetValueAt(x) < Trades[tt][0].EntryPrice){// && Trades[tt][0].SLPrice > Trades[tt][0].EntryPrice){//trail the stop while it's above EntryPrice
								Trades[tt][0].SLPrice = Math.Min(Trades[tt][0].SLPrice, Math.Max(Highs[0].GetValueAt(x-2), Highs[0].GetValueAt(x-1)));
								Trades[tt][0].TrailStopEngaged = true;
if(z) Print(Times[0].GetValueAt(x).ToString()+"  "+tt+"  Trail stop down for short trade to "+Trades[tt][0].SLPrice+"   H2: "+Highs[0].GetValueAt(x-2)+"  H1: "+Highs[0].GetValueAt(x-1));
								Trades[tt][0].StopLevels[x] = Trades[tt][0].SLPrice;
							}
						}
					}
				}
line=621;
//}catch(Exception ee1){Print(line+": "+ee1.ToString());}
				#region -- calculate the average time of the HOD and LOD --
				double SumHOD = 0;
				double SumLOD = 0;
				foreach(var kvp in HOSLOS){
					SumHOD += kvp.Value[0];
					SumLOD += kvp.Value[1];
				}
				double xAvgHOS = JulianTimeToHM(SumHOD / HOSLOS.Count);
				double xAvgLOS = JulianTimeToHM(SumLOD / HOSLOS.Count);
				AvgHOS = -1;
				AvgLOS = -1;
				#region -- Find the time of the actual bar nearest to the AvgHOS and AvgLOS.  So the average times need to be rounded to actual bar times
				priorTT = -1;
				for(int abar = 1; (AvgHOS==-1 || AvgLOS==-1) && abar<BarsArray[0].Count; abar++){
					int tt = ToTime(Times[0].GetValueAt(abar))/100;
					if(priorTT!=-1){
						if(tt>=xAvgHOS && priorTT<xAvgHOS) AvgHOS = tt;
						if(tt>=xAvgLOS && priorTT<xAvgLOS) AvgLOS = tt;
					}
					priorTT = tt;
				}
				#endregion
				AvgHOS_str = Int2TimeStr(AvgHOS);
				AvgLOS_str = Int2TimeStr(AvgLOS);
//Print(c+": "+HOSLOS.Count+"   AvgHOS: "+AvgHOS_str+"  LOD: "+AvgLOS_str);
				#endregion

				var res = new SortedList<double,double[]>();
//				var examine_str = string.Empty;
				foreach(var kvp in Trades){
					double LongW  = 0;
					double LongL  = 0;
					double ShortW = 0;
					double ShortL = 0;
					double PnL    = 0;//PnL in points
					double pnlWinnersL    = 0;
					double pnlLosersL     = 0;
					double pnlWinnersS    = 0;
					double pnlLosersS     = 0;
					double contractsTotal = 0;
					double tgtsHit = 0;
					double slsHit  = 0;
					double eodsHit = 0;
//Print(kvp.Key);
					foreach(var trade in kvp.Value){
						PnL = PnL + trade.PnL;
						if(trade.Disposition=="Tgt")      tgtsHit++;
						else if(trade.Disposition=="SL" && !trade.BEStopEngaged)  slsHit++;//only count a SL hit if it's not a BE stop
						else if(trade.Disposition=="EOD") eodsHit++;
						contractsTotal += trade.Contracts;
						if(trade.PnL>0){
							if(trade.Direction == 'L'){
								LongW += trade.Contracts;
								pnlWinnersL += trade.PnL;
							}else{
								ShortW += trade.Contracts;
								pnlWinnersS += trade.PnL;
							}
						}else{
							if(trade.Direction == 'L'){
								LongL += trade.Contracts;
								pnlLosersL += trade.PnL;
							}else{
								ShortL += trade.Contracts;
								pnlLosersS += trade.PnL;
							}
						}
//Print("  "+trade.Direction+"  "+(POINT_VALUE*trade.PnL).ToString("C"));
					}
						
					if(LongW+LongL+ShortW+ShortL > 0){
						double avgTrade = PnL*POINT_VALUE / (LongW+LongL+ShortW+ShortL);
						if(PnL>0 && avgTrade>pMinAvgDollarsPerTrade){//only select profitable trade times
							var pct = ((LongW+ShortW)/(LongW+LongL+ShortW+ShortL));
							res[pct] = new double[]{kvp.Key, LongW, ShortW, LongL, ShortL, pnlWinnersL*POINT_VALUE, pnlWinnersS*POINT_VALUE, pnlLosersL*POINT_VALUE, pnlLosersS*POINT_VALUE, tgtsHit, slsHit, eodsHit};
						}
					}

//					BestTrades[Int2TimeStr(kvp.Key)] = res[pct];
//					if(pExamineTime == kvp.Key) examine_str = res[pct];
				}

//				foreach(var r in res){
//					Print(r.Key+"   wins: "+(r.Value[1]+r.Value[2])+"  losses: "+(r.Value[3]+r.Value[4])+"  pnl: "+(r.Value[5]+r.Value[6]+r.Value[7]+r.Value[8]).ToString("C"));
//				}
//string pctl_location = string.Empty;
//if(i==bt.Value.Cols.Count-1) pctl_location = BarToClose[key].PctAboveClose.ToString(": 0%"); else pctl_location = string.Empty;

				while(res.Count > pMinWinCount) res.RemoveAt(0);//remove all extra trades that are above the min requested win count
				foreach(var kvp in res){
					int key = Convert.ToInt32(kvp.Value[0]);
//try{
line=685;
					if(DateAndClosePrice.Count>0) foreach(var dayToExamine in DateAndClosePrice.Keys){
						var dt = new DateTime(dayToExamine.Year, dayToExamine.Month, dayToExamine.Day, IntToHr(key), IntToMinute(key),0);
						int abar = BarsArray[0].GetBar(dt);
						var barClose = Bars.GetClose(abar);
						if(!BarToClose.ContainsKey(key))
							BarToClose[key] = new BarToCloseData(dayToExamine, DateAndClosePrice[dayToExamine]);
						if(!BarToClose[key].Data.ContainsKey(dayToExamine)) BarToClose[key].Data[dayToExamine] = new CloseDayCloseBar(DateAndClosePrice[dayToExamine]);
						BarToClose[key].Data[dayToExamine].DiffToClose = barClose - BarToClose[key].Data[dayToExamine].DayClose;
						BarToClose[key].TimesAboveClose = BarToClose[key].Data.Values.Where(num => num.DiffToClose>0).Count();
						BarToClose[key].TimesBelowClose = BarToClose[key].Data.Values.Where(num => num.DiffToClose<0).Count();
						BarToClose[key].PctAboveClose   = BarToClose[key].TimesAboveClose / (BarToClose[key].TimesAboveClose + BarToClose[key].TimesBelowClose);
//if(key==738)Print(BarToClose[key].TimesAboveClose+"/"+BarToClose[key].TimesBelowClose+"  Mkt: "+barClose+"  DayClose: "+BarToClose[key].Data[dayToExamine].DayClose+"  % above dc: "+BarToClose[key].PctAboveClose);
					}
//					foreach(var kvp2 in BarToClose){
//						kvp2.Value.TimesAboveClose = kvp2.Value.Data.Values.Where(num => num.DiffToClose>0).Count();
//						kvp2.Value.TimesBelowClose = kvp2.Value.Data.Values.Where(num => num.DiffToClose<0).Count();
//						kvp2.Value.PctAboveClose = kvp2.Value.TimesAboveClose / (kvp2.Value.TimesAboveClose + kvp2.Value.TimesBelowClose);
//					}
					BestTrades[Int2TimeStr(Convert.ToInt32(kvp.Value[0]))] = new BTinfo(kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], Int2TimeStr(key), (BarToClose.ContainsKey(key) ? BarToClose[key].PctAboveClose-0.5:999), kvp.Value[5], kvp.Value[6], kvp.Value[7], kvp.Value[8], kvp.Value[9],kvp.Value[10],kvp.Value[11], this);
//}catch(Exception k1){Print(line+": "+k1.ToString());}
				}
				tnow = ToTime(NinjaTrader.Core.Globals.Now)/100;
				{
					string outputstr = "Top Winningest times\nTarget is "+(pTargetType == BreakoutResearchTool_TargetType.MultOfRange ? pMultRangeTarget+"x Range of key bar" : pTicksTarget+"-ticks")+ " SL is "+pSL_RangeMult+"x Range of key bar";
					if(BestTrades.Count>0){
						if(pShowTradesInOutputWindow) Print(outputstr);
						PrintExamineTime(pExamineTime);
//						foreach(var winkvp in BestTrades){
//							Print("\nTrade history for "+winkvp.Key+" on "+Instrument.FullName);
//							Print(winkvp.Value.OutStr);
//							int key = int.Parse(winkvp.Key.Replace(":",string.Empty));
//							if(key<tnow)
//								outputstr = string.Format("{0}\n   {1}", outputstr, winkvp.Value.OutStr);
//							else
//								outputstr = string.Format("{0}\n{1}", outputstr, winkvp.Value.OutStr);
//							foreach(var trade in Trades[key]){
//								Print(trade.EntryDT.ToString()+"  "+trade.Direction+"   "+Instrument.MasterInstrument.RoundToTickSize(trade.PnL)+"-pts  "+(trade.PnL*POINT_VALUE).ToString("C").Replace(".00",string.Empty));
//								if(trade.Contracts>0)
//									Print(trade.EntryDT.ToString("MM/dd/yyyy HH:mm")+"  "+trade.Direction+" risk: "+Instrument.MasterInstrument.FormatPrice(Math.Abs(trade.EntryPrice-trade.SLPrice))+"   "+Instrument.MasterInstrument.FormatPrice(trade.PnL)+"-pts  on "+trade.Contracts+"-contracts  "+(trade.PnL*POINT_VALUE).ToString("C").Replace(".00",string.Empty));
//								else
//									Print(trade.EntryDT.ToString("MM/dd/yyyy HH:mm")+"  "+trade.Direction+" risk: "+Instrument.MasterInstrument.FormatPrice(Math.Abs(trade.EntryPrice-trade.SLPrice))+"   "+Instrument.MasterInstrument.FormatPrice(trade.PnL)+"-pts  "+(trade.PnL*POINT_VALUE).ToString("C").Replace(".00",string.Empty));
//							}
//						}
					}
				}
				#endregion
			}
			else if (State == State.Realtime){
				#region -- Realtime --
//				foreach(var kvp in BestTrades) {
//					var ts  = TimeSpan.Parse(kvp.Key);
//					int key = int.Parse(kvp.Key.Replace(":",string.Empty));
//					foreach(var trade in Trades[key]){
//						var dt = new DateTime(trade.EntryDT.Year,trade.EntryDT.Month,trade.EntryDT.Day,ts.Hours, ts.Minutes,0);
////Print(dt.ToString()+"  key: "+kvp.Key);
//						int rbar = Bars.GetBar(dt);
//						//BackBrushes[rbar] = Brushes.DimGray;
//						TargetLevel[rbar] = trade.TargetPrice;
//					}
//				}
				#endregion
			}
			else if (State == State.Terminated){
				if(ChartPanel!=null){
					ChartPanel.MouseMove  -= OnMouseMove;
					ChartPanel.KeyUp -= ChartPanel_KeyUp;
				}
			}
			}catch(Exception ee){Print("BreakoutResearch: "+(State != null ? State.ToString():"") + "\n"+ee.ToString());}
		}

		private bool IsShiftPressed = false;
		private bool IsAltPressed = false;
		private bool IsCtrlPressed = false;
//		private void MyKeyUpEvent(object sender, KeyEventArgs e)
//		{
//		}
//=======================================================================================
		private int ToJulianMinute(int time){
			//input is 743 for 7:43.  Output is 7*60+43 = 463 (7:43 is the 463rd minute of the day)
			int x = (int)Math.Truncate(time/100.0);
			return x*60 + (time-x*100);
		}
		private int JulianTimeToHM(double juliant){//Julian time is between 0 and 1440, since there's 1440 minutes in a day
			double t = juliant/60;
			int h = (int)Math.Truncate(t);
			return h*100 + Convert.ToInt32((t-h)*60);
		}
//=======================================================================================
		private double Round2Tick(double p) {int tk = Convert.ToInt32(Math.Round(p/TickSize,0)); return tk*TickSize;}
//=======================================================================================
		private double CalculateInitialSL(char Direction, double entryPrice, double range){
			if(Direction=='L')
				return Round2Tick(entryPrice - pTicksSLBuffer*TickSize - (pSL_RangeMult > 0 ? range * pSL_RangeMult : 0));
			else
				return Round2Tick(entryPrice + pTicksSLBuffer*TickSize + (pSL_RangeMult > 0 ? range * pSL_RangeMult : 0));
		}
		private double CalculateContracts(double EntryPrice, double SLPrice){
			double contracts = pRiskPerTrade >= 0 ? (int)Math.Truncate(pAccountSize*pRiskPerTrade / (Math.Abs(EntryPrice-SLPrice)*POINT_VALUE)) : 0;
			//if contracts is less than 1 (like 0.6), then the risk distance is too large, contracts will be truncated to 0
			return Math.Min(pMaxContracts,contracts);
		}
		private double GetTargetDistance(double MinTgtPts, double distPts, double TgtReductionPct){
			return distPts;
			double gradient = distPts - MinTgtPts;
			return Round2Tick(Math.Max(MinTgtPts, distPts - gradient * TgtReductionPct));
		}
//=======================================================================================
//=======================================================================================
		private void PrintExamineTime(int ExamineTime){
			#region -- Print out trades at the ExamineTime --
try{
line=858;
			ExamineTable.Clear();
			ExamineTable[1] = new List<string>();
			ExamineTable[1].Add(Int2TimeStr(ExamineTime)+" No trade data found, is this time is outside of your session range?");
			if(ExamineTime>=0){
line=863;
				string s = "";
				int row = 1;
				if(pExamineTime==ExamineTime && pShowTradesInOutputWindow)  Print("\n-------  your specific examine request ---------");
				string ts = Int2TimeStr(ExamineTime);
				if(ExamineTime >=0 && !Trades.ContainsKey(ExamineTime)){
					if(pShowTradesInOutputWindow) {
						s = ("Trades did not contain your examine time of "+ts);
						Print("");
						ExamineTable[row] = new List<string>(){s};	row++;
					}
				}else{
line=875;
					s = (ts+" EXAMINED:  Trade history for -------- "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
					if(pShowTradesInOutputWindow) Print(s);
					ExamineTable[row] = new List<string>(){ts+" Trade-by-trade"};		row++;
					ExamineTable[row] = new List<string>(){"<headers>"};		row++;
					//Print(examine_str);
					double wLong = 0;
					double wShort = 0;
					double lLong = 0;
					double lShort = 0;
					double lPnL = 0;
					double sPnL = 0;
					double wProfit = 0;
					double lProfit = 0;
					var rows = new SortedDictionary<int,List<string>>();
					int startrow = row;
					foreach(var trade in Trades[ExamineTime]){
line=890;
						if(trade.Direction == 'L') lPnL += trade.PnL;
						else sPnL += trade.PnL;
						if(trade.PnL>0) {
							wProfit += trade.PnL;
							if(trade.Direction == 'L') wLong++;
							else wShort++;
						} else{
							lProfit += trade.PnL;
							if(trade.Direction == 'L') lLong++;
							else lShort++;
						}
						rows[row] = new List<string>(){
							"<data>",
							trade.Direction.ToString(), 
							trade.EntryDT.ToString("MM/dd/yyyy HH:mm"),
							trade.LocPercentileInDailyRange.ToString("0%"),
							Instrument.MasterInstrument.FormatPrice(Math.Abs(trade.EntryPrice-trade.SLPrice)), 
							Instrument.MasterInstrument.FormatPrice(trade.PnL),
							trade.Contracts.ToString(),
							(trade.PnL*POINT_VALUE).ToString("C").Replace(".00",string.Empty),
							trade.Disposition
						};
						s = (trade.EntryDT.ToString("MM/dd/yyyy HH:mm")+"  "+trade.Direction+"   pct HL: "+trade.LocPercentileInDailyRange.ToString("0%")+"    risk: "+Instrument.MasterInstrument.FormatPrice(Math.Abs(trade.EntryPrice-trade.SLPrice))+"   "+Instrument.MasterInstrument.FormatPrice(trade.PnL)+"-pts  on "+trade.Contracts+(trade.Contracts==1 ? "-contract  ": "-contracts  ")+(trade.PnL*POINT_VALUE).ToString("C").Replace(".00",string.Empty)+"  "+trade.Disposition);
						if(pShowTradesInOutputWindow) Print(s);
						row++;
					}
line=917;
					#region -- Sort results, Long trades first, short trades second in the sequence --
					row = startrow;
					foreach(var r in rows){
						if(r.Value[1]=="L") {
							ExamineTable[row] = r.Value;
							row++;
//Print(r.Value[1]+ " "+r.Value[2]);
						}
					}
					foreach(var r in rows){
						if(r.Value[1]=="S") {
							ExamineTable[row] = r.Value;
							row++;
//Print(r.Value[1]+ " "+r.Value[2]);
						}
					}
					#endregion
					ExamineTable[row] = new List<string>(){""};		row++;
					s = ((POINT_VALUE*(lPnL+sPnL)).ToString("C")+":  PnL");
					if(pShowTradesInOutputWindow) Print(s);
					ExamineTable[row] = new List<string>(){s};		row++;
					s = ((lPnL*POINT_VALUE).ToString("C")+ "  Long w/l:   "+wLong+"/"+lLong+"   "+(wLong/(wLong+lLong)).ToString("0%"));
					if(pShowTradesInOutputWindow) Print(s);
					ExamineTable[row] = new List<string>(){s};		row++;
					s = ((sPnL*POINT_VALUE).ToString("C")+ " Short w/l:   "+wShort+"/"+lShort+"   "+(wShort/(wShort+lShort)).ToString("0%"));
					if(pShowTradesInOutputWindow) Print(s);
					ExamineTable[row] = new List<string>(){s};		row++;
					s = ("overall:  "+((wLong+wShort) / (wLong+wShort+lLong+lShort)).ToString("0%"));
line=928;
					if(pShowTradesInOutputWindow) Print(s);
					ExamineTable[row] = new List<string>(){s};		row++;
					double avgWinDlrs = wProfit/(wLong+wShort)*POINT_VALUE;
					double avgLossDlrs = lProfit/(lLong+lShort)*POINT_VALUE;
					s = ("avg win: "+avgWinDlrs.ToString("C")+" avg loss: "+avgLossDlrs.ToString("C"));
line=934;
					if(pShowTradesInOutputWindow) Print(s);
					ExamineTable[row] = new List<string>(){s};		row++;
					if(pShowTradesInOutputWindow) Print("");
				}
			}
			#endregion
}catch(Exception ee){Print(line+": "+ee.ToString());}
		}
//=======================================================================================
//=======================================================================================
		private string Int2TimeStr(int t){
			string s = t.ToString();
			if(s.Length==1) return ("0:0"+s);
			if(s.Length==2) return ("0:"+s);
			if(s.Length==3)
				s = "0"+s;
			return s.Substring(0,2)+":"+s.Substring(2,2);
		}
		private int IntToHr(int t) {
			return Convert.ToInt32(Math.Truncate(t/100.0));
		}
		private int IntToMinute(int t) {
			int x = Convert.ToInt32((Math.Truncate(t/100.0) * 100));
			return t-x;
		}
//=======================================================================================
//=======================================================================================
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
//			if(!Keyboard.IsKeyUp(Key.LeftCtrl)) return;
			#region -- OnMouseMove -----------------------------------------------------------
			Point coords = e.GetPosition(ChartPanel);
			var X = ChartingExtensions.ConvertToHorizontalPixels(coords.X, ChartControl.PresentationSource);
			MouseABar = ChartBars.GetBarIdxByX(ChartControl, X);
			MousePricePixel = ChartingExtensions.ConvertToVerticalPixels(coords.Y, ChartControl.PresentationSource);

			ForceRefresh();
			#endregion -----------------------------------------------------
		}
//======================================================================================================
		private void ChartPanel_KeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) IsShiftPressed = true;
			if(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) IsAltPressed = true;
			if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) IsCtrlPressed = true;
			if (e.Key==Key.D1 && IsCtrlPressed)// && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
			{
				if(DisplayedContent == SUMMARY_TABLE)        DisplayedContent = EXAMINE_TABLE;
				else if(DisplayedContent == EXAMINE_TABLE)   DisplayedContent = HIDE_ALL_TABLES;
				else if(DisplayedContent == HIDE_ALL_TABLES) DisplayedContent = SUMMARY_TABLE;
			    ForceRefresh();
			}
			else if(e.Key==Key.D2 && IsCtrlPressed) {
				DisplayedContent = EXAMINE_TABLE;
				int tt = ToTime(BarsArray[0].GetTime(MouseABar))/100;
//Print(1019);
				PrintExamineTime(tt);
				ForceRefresh();
//Print("Contains tt: "+tt+":  "+(BarToClose.ContainsKey(tt)).ToString());
				if(BarToClose.ContainsKey(tt)){
					foreach(var kvp in BarToClose[tt].Data){
						Print("     BTC: "+kvp.Key.ToString()+"  "+kvp.Value.DiffToClose);
					}
					Print("Times above close: "+BarToClose[tt].TimesAboveClose.ToString());
					Print("Times below close: "+BarToClose[tt].TimesBelowClose.ToString());
					Print("Pct above close: "+BarToClose[tt].PctAboveClose.ToString("0%"));
				}

//				var OrdersList = new List<Order>();
//				if(Closes[0][0] < MousePrice){
//					OrdersList.Add(myAccount.CreateOrder(
//								Instrument.GetInstrument(Instrument.FullName), 
//								OrderAction.Buy, 
//								OrderType.StopLimit, 
//								OrderEntry.Automated, 
//								TimeInForce.Day, (int)Math.Max(1,Math.Min(pMaxContracts,SuggestedPositionSize)), MousePrice, MousePrice-TickSize, string.Empty, "BuyLimEntry", Core.Globals.MaxDate, null)
//					);
//				}
//				else if(Closes[0][0] > MousePrice){
//					OrdersList.Add(myAccount.CreateOrder(
//								Instrument.GetInstrument(Instrument.FullName), 
//								OrderAction.Sell, 
//								OrderType.StopLimit, 
//								OrderEntry.Automated, 
//								TimeInForce.Day, (int)Math.Max(1,Math.Min(pMaxContracts,SuggestedPositionSize)), MousePrice-TickSize, MousePrice, string.Empty, "BuyLimEntry", Core.Globals.MaxDate, null)
//					);
//				}
//					foreach(var tgt in LongTargets.Values){
//		//Print(tgt.MarkerTag+" Tgt at: "+tgt.Price.ToString()+"  qty: "+tgt.OrderQty);
//						if(tgt.OrderQty>0 && !double.IsNaN(tgt.Price)){
//							var oco_id_this_tgt = pEnableOCO ? string.Format("{0}-{1}",tgt.MarkerTag,oco_id) : string.Empty;
//							OrdersList.Add(myAccount.CreateOrder(Instrument.GetInstrument(Instrument.FullName), 
//									OrderAction.Sell, 
//									OrderType.StopMarket, 
//									OrderEntry.Automated, 
//									TimeInForce.Day, (int)tgt.OrderQty, 0, LowerSL, 
//									oco_id_this_tgt, 
//									string.Format("{0}-{1}",tgt.MarkerTag,"SL"), Core.Globals.MaxDate, null));
//							OrdersList.Add(myAccount.CreateOrder(
//									Instrument.GetInstrument(Instrument.FullName), 
//									OrderAction.Sell, 
//									OrderType.Limit, 
//									OrderEntry.Automated, 
//									TimeInForce.Day, (int)tgt.OrderQty, tgt.Price, 0, 
//									oco_id_this_tgt, 
//									tgt.MarkerTag, Core.Globals.MaxDate, null));
//						}
//					}
//				ChartControl.Dispatcher.InvokeAsync((Action)(() =>{	myAccount.Submit(OrdersList.ToArray()); 
//				}));
			}
		}
//=======================================================================================
//=======================================================================================
		protected override void OnBarUpdate()
		{
			if(BarsInProgress != 0) return;
			if(CurrentBars[0]<2 || !IsValidLicense) return;

			if(pExamineTime >=0 && ToTime(Time[0])/100 == pExamineTime && this.ValidDaysOfWeek.Contains(Time[0].DayOfWeek)) Draw.VerticalLine(this, CurrentBars[0].ToString()+"examinetime",1, Brushes.Yellow);
			if(DailyH==0 || Bars.IsFirstBarOfSession){
				DailyH=High[0];
				DailyL=Low[0];
			}else{
				DailyH = Math.Max(High[0],DailyH);
				DailyL = Math.Min(Low[0],DailyL);
				mid = (DailyH+DailyL)/2;
			}
			if(Filter_MidDayPrice_BuyBelowSellAbove || Filter_MidDayPrice_BuyAboveSellBelow) {
				MidRangePrice[CurrentBars[0]]=mid;
			}

			if(SetEntryPrice!=double.MinValue){
				EntryLevel[0] = SetEntryPrice;
				SetEntryPrice = double.MinValue;
			}

			if(IsFirstTickOfBar)
			{
				tnow = ToTime(Times[0][0])/100;//NinjaTrader.Core.Globals.Now)/100;
				Map_Abar_ToTime[CurrentBars[0]] = Int2TimeStr(ToTime(Times[0][0])/100);
				if(MidRangePrice.ContainsKey(CurrentBars[0])) Values[1][0] = this.MidRangePrice[CurrentBars[0]];
				//RemoveDrawObject("info");
				if(CurrentBar > Bars.Count-5){
					//IsRealTime = true;
					SummaryTable.Clear();
					double wins   = 0;
					double losses = 0;
					double pnl    = 0;
					double GrossWinDlrs  = 0;
					double GrossLossDlrs = 0;

					if(BestTrades.Count>0){
						foreach(var winkvp in BestTrades){
							int key = int.Parse(winkvp.Key.Replace(":",string.Empty));
							wins    = wins   + winkvp.Value.WinsL+winkvp.Value.WinsS;
							losses  = losses + winkvp.Value.LossesL + winkvp.Value.LossesS;
							pnl     = pnl    + winkvp.Value.PnLDlrs;
							GrossWinDlrs += winkvp.Value.WinDlrsL + winkvp.Value.WinDlrsS;
							GrossLossDlrs += Math.Abs(winkvp.Value.LossDlrsL + winkvp.Value.LossDlrsS);
						}
						SummaryTable[1] = ("Summary (Ctrl-1 to toggle show/hide of this table)");
						SummaryTable[2] = string.Format("{0} ({1}-{2})  {3}   {4}/trade",(wins/(wins+losses)).ToString("0%"), wins, losses, pnl.ToString("C").Replace(".00",string.Empty), (pnl/(wins+losses)).ToString("C").Replace(".00",string.Empty));
						SummaryTable[3] = string.Format("{0} trades/day over {1}-days",((wins+losses)/DatesTraded.Count).ToString("0.0"),DatesTraded.Count);
						SummaryTable[4] = string.Format("Gross$ {0}/{1} pf: {2}", GrossWinDlrs.ToString("0"), GrossLossDlrs.ToString("0"), (GrossWinDlrs/GrossLossDlrs).ToString("0.00"));
						SummaryTable[5] = "";
//					Draw.TextFixed(this,"info",outputstr,TextPosition.TopLeft,Brushes.White,new SimpleFont("Arial",12), Brushes.DimGray,Brushes.Black,100);
					}else if(IsValidLicense)
						Draw.TextFixed(this,"info","No times meet your filter criteria\nSee OutputWindow for more details",TextPosition.Center,Brushes.White,new SimpleFont("Arial",12), Brushes.DimGray,Brushes.Black,100);

					SummaryTable[6] = string.Format("Target is {0}  SL is {1} {2}", 
						(pTargetType == BreakoutResearchTool_TargetType.MultOfRange ? string.Format("{0}x Range", pMultRangeTarget) : string.Format("{0}-ticks", pTicksTarget)),
						string.Format("{0}x Range", pSL_RangeMult),
						avgDailyRange>0 ? string.Format("{0}-tks avg daily range",Math.Truncate(avgDailyRange/TickSize)) : ""
					);
					SummaryTable[7] = pMaxBarsWaitingForEntry+"-max waiting bars";
					if(this.pEngageBEStop)       SummaryTable[8] = ("BE stop ON");
					if(this.pEngageTrailingStop) SummaryTable[9] = ("2bar Trailstop ON");
					if(this.Filter_BarClosingDirection) SummaryTable[10] = ("Closing dir filter ON");
					if(this.Filter_MidDayPrice_BuyBelowSellAbove) SummaryTable[11] = ("Revert to midprice filter ON");
					else if(this.Filter_MidDayPrice_BuyAboveSellBelow) SummaryTable[11] = ("Breakout from midprice filter ON");
					if(pDaysOfWeek=="TODAY")
						SummaryTable[12] = (string.Format("DOW: {0} {1}-{2}",this.ValidDaysOfWeek[0].ToString(), Int2TimeStr(this.pStartTime), Int2TimeStr(this.pEndTime)));
					else
						SummaryTable[12] = (string.Format("DOW: {0} {1}-{2}",pDaysOfWeek, Int2TimeStr(this.pStartTime), Int2TimeStr(this.pEndTime)));
					SummaryTable[13] = "";
				}
//				if(BestTrades.ContainsKey(Int2TimeStr(tnow))) {
//					foreach(var trade in Trades[tnow])
//						TargetLevel[CurrentBars[0]-trade.EntryABar] = trade.TargetPrice;
////						Print(trade.EntryDT.ToString("MM/dd/yyyy HH:mm")+"  "+trade.Direction+"  "+Instrument.MasterInstrument.RoundToTickSize(trade.PnL)+"-pts  "+(trade.PnL*POINT_VALUE).ToString("C"));
//				}
				if(IsRealTime && EntryLevel.IsValidDataPoint(1) && EntryHitAlertABar!=CurrentBars[0] && !pEntryLevelHitWAV.Contains("none") && (PriorClose < EntryLevel[1] && Closes[0].GetValueAt(0) >= EntryLevel[1] || PriorClose > EntryLevel[1] && Closes[0].GetValueAt(0) <= EntryLevel[1])){
					EntryHitAlertABar = CurrentBars[0];
					Alert(CurrentBars[0].ToString(), Priority.High, string.Format("BRT-{0} entry price hit",(this.Filter_MidDayPrice_BuyAboveSellBelow ? "Trend":"Reversion")), AddSoundFolder(pEntryLevelHitWAV), 1, Brushes.Black, Brushes.Blue);
				}
				PriorClose = Closes[0].GetValueAt(0);
line=731;
				if(IsFirstTickOfBar && AlertABar != CurrentBars[0] && !pAlertWAV.Contains("none")){
					//Now, alert if we find a trade that should be taken today, based on the "BestTrades" results calculated through yesterday
					var BTabars = Map_Abar_ToTime.Where(k=> k.Key > CurrentBars[0]-3 && BestTrades.ContainsKey(k.Value)).Select(k=> k.Key).ToList();//select any BestTrades key times that are not recorded trades
					#region -- Play alert when a new possible trade arrives --
					int PermittedDir = int.MinValue;
					foreach(var btabar in BTabars){
line=738;
						var t0 = Times[0].GetValueAt(btabar);
						if(!ValidDaysOfWeek.Contains(t0.DayOfWeek)) continue;
						#region -- Trade setup filters --
						PermittedDir = GetPermittedDirection(Closes[0].GetValueAt(btabar), Opens[0].GetValueAt(btabar), MidRangePrice[btabar], Medians[0].GetValueAt(btabar));
						#endregion ------
						if(IsRealTime && PermittedDir != int.MinValue && btabar==CurrentBars[0]){
							AlertABar = CurrentBars[0];
							Alert(CurrentBars[0].ToString(), Priority.High, string.Format("BRT-{0} approaching {1}",
								(this.Filter_MidDayPrice_BuyAboveSellBelow ? "Trend":"Reversion"), PermittedDir == LONG ? BestTrades[Map_Abar_ToTime[btabar]].LongDetails : BestTrades[Map_Abar_ToTime[btabar]].ShortDetails), AddSoundFolder(pAlertWAV), 1, Brushes.Black, Brushes.White);
						}
					}
					#endregion
				}
//foreach(var kvp in SummaryTable)Print(kvp.Key+":  "+kvp.Value);
			}
		}
//=======================================================================================
		private SharpDX.Direct2D1.Brush MagentaBrushDX, GreenBrushDX, BlackBrushDX, CyanBrushDX, WhiteBrushDX, OrangeBrushDX;
		public override void OnRenderTargetChanged()
		{
			if(MagentaBrushDX!=null      && !MagentaBrushDX.IsDisposed)      MagentaBrushDX.Dispose();      MagentaBrushDX = null;
			if(RenderTarget != null) MagentaBrushDX = Brushes.Magenta.ToDxBrush(RenderTarget);

			if(GreenBrushDX!=null      && !GreenBrushDX.IsDisposed)      GreenBrushDX.Dispose();      GreenBrushDX = null;
			if(RenderTarget != null) GreenBrushDX = Brushes.DarkGreen.ToDxBrush(RenderTarget);

			if(BlackBrushDX!=null      && !BlackBrushDX.IsDisposed)      BlackBrushDX.Dispose();      BlackBrushDX = null;
			if(RenderTarget != null) BlackBrushDX = Brushes.Black.ToDxBrush(RenderTarget);

			if(CyanBrushDX!=null      && !CyanBrushDX.IsDisposed)      CyanBrushDX.Dispose();      CyanBrushDX = null;
			if(RenderTarget != null) CyanBrushDX = Brushes.Cyan.ToDxBrush(RenderTarget);

			if(WhiteBrushDX!=null      && !WhiteBrushDX.IsDisposed)      WhiteBrushDX.Dispose();      WhiteBrushDX = null;
			if(RenderTarget != null) WhiteBrushDX = Brushes.White.ToDxBrush(RenderTarget);

			if(OrangeBrushDX!=null      && !OrangeBrushDX.IsDisposed)      OrangeBrushDX.Dispose();      OrangeBrushDX = null;
			if(RenderTarget != null) OrangeBrushDX = Brushes.Orange.ToDxBrush(RenderTarget);
		}
//=======================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			IsRealTime=true;
			if(ChartBars==null)    return;
			if(chartControl==null) return;
			if(chartScale==null)   return;
			MousePrice = Round2Tick(chartScale.GetValueByY(Convert.ToSingle(MousePricePixel)));
			//base.OnRender(chartControl, chartScale);
try{

			var barwidth_f = 0.5f*(chartControl.GetXByBarIndex(ChartBars, 1) - chartControl.GetXByBarIndex(ChartBars, 0));
			int LMaB   = Math.Max(0,ChartBars.FromIndex);
			int RMaB   = Math.Min(ChartBars.Count-1, ChartBars.ToIndex);

//			var trades = Trades.Where(k=> BestTrades.ContainsKey(Int2TimeStr(k.Key))).Select(k=>k.Value).ToList();//select all trades that are in the BestTrades list
			var trades = Trades.Where(k=> BestTrades.ContainsKey(Int2TimeStr(k.Key))).ToDictionary(k=> k.Key, k=> k.Value);//select all trades that are in the BestTrades list
line=1242;
			int i =0;
			var v1 = new SharpDX.Vector2(0,0);
			v1.Y = 0f;
			float x0=0;
			if(pBlockOutOfSessionTimes){
				#region -- background block of out of session bars --
				var outofsessionabars = OutOfSessionBar.Where(k=>k < RMaB && k > LMaB);
				if(outofsessionabars!=null){
					var CrimsonBrushDX = Brushes.Crimson.ToDxBrush(RenderTarget);
					CrimsonBrushDX.Opacity = 0.25f;
					foreach(var abar in outofsessionabars){
						x0 = chartControl.GetXByBarIndex(ChartBars, abar-1)-barwidth_f;
						v1.X = chartControl.GetXByBarIndex(ChartBars, abar)-barwidth_f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, v1.Y, v1.X-x0, ChartPanel.H), CrimsonBrushDX);
					}
					CrimsonBrushDX.Dispose();
				}
				#endregion
			}
			if(pShowTrendBars){
				DateTime dt;
				for(int abar = LMaB; abar<RMaB; abar++){
					x0 = chartControl.GetXByBarIndex(ChartBars, abar-1)-barwidth_f;
					v1.X = chartControl.GetXByBarIndex(ChartBars, abar)-barwidth_f;
					dt = BarsArray[0].GetTime(abar).Date;
					if(DailyRSI_3.ContainsKey(dt)){
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, ChartPanel.H-30f, v1.X-x0, 9f), 
							DailyRSI_3[dt] > 50 ? GreenBrushDX : MagentaBrushDX);
					}
					if(DailySMA_20.ContainsKey(dt)){
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, ChartPanel.H-20f, v1.X-x0, 20f), 
							BarsArray[0].GetClose(abar) > DailySMA_20[dt] ? GreenBrushDX : MagentaBrushDX);
					}
				}
			}

bool pShowHOSLOS = true;
			if(pShowHOSLOS){
				float w = 0f;
				float X = 0f;
				float Y = 0f;
				//if(pShowAvgHOSLOS)
				{
					var hodabars = Map_Abar_ToTime.Where(k=> k.Key > LMaB && k.Key <= RMaB && k.Value==AvgHOS_str).Select(k=>k.Key).ToList();
					if(hodabars!=null && hodabars.Count>0){
						foreach(var b in hodabars) {
							X = chartControl.GetXByBarIndex(ChartBars, b-1);
							w = 20f;//chartControl.GetXByBarIndex(ChartBars, b+1) - X;
							Y = (float)chartScale.GetYByValue(BarsArray[0].GetHigh(b)+5*TickSize) - 10f;
							RenderTarget.FillRectangle(new SharpDX.RectangleF(X, Y, w, 10f), WhiteBrushDX);
						}
					}
					hodabars = Map_Abar_ToTime.Where(k=> k.Key > LMaB && k.Key <= RMaB && k.Value==AvgLOS_str).Select(k=>k.Key).ToList();
					if(hodabars!=null && hodabars.Count>0){
						foreach(var b in hodabars) {
							X = chartControl.GetXByBarIndex(ChartBars, b-1);
							w = 20f;//chartControl.GetXByBarIndex(ChartBars, b+1) - X;
							Y = (float)chartScale.GetYByValue(BarsArray[0].GetLow(b)-5*TickSize);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(X, Y, w, 10f), OrangeBrushDX);
						}
					}
				}
//				if(pShowEachHOSLOS){
//					foreach(var kvp in HOSLOS){
//						var ht = Int2TimeStr(kvp.Value[0]);
//						var lt = Int2TimeStr(kvp.Value[1]);
						
//					}
//					for(int abar = LMaB; abar<RMaB; abar++){
//						var dt = Times[0].GetValueAt(abar);
//						var t_str = Map_Abar_ToTime[abar];
//						if(HOSLOS.ContainsKey(dt.Date)
//					}
//				}
			}
line=797;
			v1.X = chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]);
line=799;
			//int maxSignalABar = 0;
			if(trades!=null && trades.Count>0){ // Plot all historical trades
				foreach(var tr in trades.Values){
					var x = tr.Where(p=>p.SignalABar > LMaB && p.SignalABar <= RMaB).ToList();//select all best trades that happened in chart area
					//maxSignalABar = Math.Max(maxSignalABar, tr.Max(p=>p.SignalABar));
					if(x!=null){
line=806;
						foreach(info t in x){
							v1.X = chartControl.GetXByBarIndex(ChartBars,t.SignalABar);
							v1.Y = 0f;
							#region -- Draw win/loss bar from top of chart down 100px --
							if(t.PnL>0)
								RenderTarget.FillRectangle(new SharpDX.RectangleF(v1.X - barwidth_f/2f, v1.Y, Math.Max(1,barwidth_f), 100f), GreenBrushDX);
							else if(t.PnL<0)
								RenderTarget.FillRectangle(new SharpDX.RectangleF(v1.X - barwidth_f/2f, v1.Y, Math.Max(1,barwidth_f), 100f), MagentaBrushDX);
							#endregion ------
							#region -- Draw EntryPrice oval --
							v1.Y = (float)chartScale.GetYByValue(t.EntryPrice);
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[2].Width, Plots[2].Width), Plots[2].BrushDX);
							#endregion --
							#region -- Draw historical exit levels --
							if(pShowHistoricalExitLevels){
								List<info> lvls = tr.Where(k=>k.ExitABar < t.EntryABar && t.Disposition.CompareTo("Tgt")!=0 && t.Disposition.CompareTo("SL")!=0).ToList();
								var v2 = new SharpDX.Vector2(v1.X-2f,0);
								foreach(var lvl in lvls){
									double p = lvl.Direction=='L'? lvl.ExitPrice-lvl.EntryPrice : lvl.EntryPrice-lvl.ExitPrice;
									p = t.Direction=='L' ? t.EntryPrice + p : t.EntryPrice - p;
									v2.Y = (float)chartScale.GetYByValue(p);
									RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v2, Plots[3].Width, Plots[3].Width), Plots[3].BrushDX);
								}
							}
							#endregion --
							#region -- Draw TargetPrice oval --
							v1.Y = (float)chartScale.GetYByValue(t.TargetPrice);
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), Plots[0].BrushDX);
							#endregion --
							#region -- Draw SLPrice oval --
							if(t.StopLevels.Count==0){
								v1.Y = (float)chartScale.GetYByValue(t.SLPrice);
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), 
									(t.BEStopEngaged ? OrangeBrushDX :MagentaBrushDX)
								);
							}
							#endregion --
							#region -- Draw Trailing SLPrice oval --
							if(t.StopLevels.Count>0){
								foreach(var tstop in t.StopLevels){
									v1.X = chartControl.GetXByBarIndex(ChartBars,tstop.Key);
									v1.Y = (float)chartScale.GetYByValue(tstop.Value);
									RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), 
										(t.BEStopEngaged ? OrangeBrushDX :MagentaBrushDX)
									);
								}
							}
							#endregion --
line=828;
							#region -- Draw on-bar-trade-details and exit line --
							if(MouseABar==t.SignalABar){
								var str = string.Empty;
								if(t.ExitABar>0){
									str = string.Format("{0}-{1}: {2}\n{3}-tks {4}-size\n{5}\n{6}",
											t.Direction,
											(t.PnL>0 ? "Win":"Loss"),
											Instrument.MasterInstrument.FormatPrice(t.PnL),
											(t.PnL/TickSize).ToString("0"),
											t.Contracts,
											t.Disposition=="EOD" ? "EOD" :(t.TrailStopEngaged ? "TrailStop" : (t.BEStopEngaged ? "BEstop" : t.Disposition)),
											Int2TimeStr(ToTime(Times[0].GetValueAt(MouseABar))/100));
								}else
									str = "open position";
								var textFormat = new SimpleFont("Arial",16).ToDirectWriteTextFormat();
								var textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, textFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
								var labelRect  = new SharpDX.RectangleF(v1.X+20f, MousePricePixel+20f, textLayout.Metrics.Width+2f, textLayout.Metrics.Height+2f);
line=846;
								RenderTarget.FillRectangle(labelRect, BlackBrushDX);
								if(t.PnL>0){
									if(t.ExitABar>0) RenderTarget.DrawLine(new SharpDX.Vector2(v1.X,(float)chartScale.GetYByValue(t.EntryPrice)),new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars,t.ExitABar),(float)chartScale.GetYByValue(t.ExitPrice)),Plots[0].BrushDX);
									RenderTarget.DrawText(str, textFormat, labelRect, Plots[0].BrushDX);
								}
								else{
									if(t.ExitABar>0) RenderTarget.DrawLine(new SharpDX.Vector2(v1.X,(float)chartScale.GetYByValue(t.EntryPrice)),new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars,t.ExitABar),(float)chartScale.GetYByValue(t.ExitPrice)),MagentaBrushDX);
									RenderTarget.DrawText(str, textFormat, labelRect, MagentaBrushDX);
								}
								textLayout.Dispose();
								textFormat.Dispose();
							}
							#endregion
						}
					}
				}
			}
			//Print("MaxSignalbar: "+maxSignalABar+"   "+Times[0].GetValueAt(maxSignalABar).ToString());
line=866;
//if(false && isdebug){
//	var nowst = Int2TimeStr(tnow);
//	if(!BestTrades.ContainsKey(nowst)) {
//		BestTrades[nowst] = BestTrades.First().Value;
//		BestTrades[nowst].Cols[2] = nowst;
//	}
//}
line=873;
			int PermittedDir = int.MinValue;
			SuggestedPositionSize = -999;
			//if(BarsInProgress == 0)
			{
			//Now, print the trades that should be taken today, based on the "BestTrades" results calculated through yesterday
			var BTabars = Map_Abar_ToTime.Where(k=> k.Key > LMaB && k.Key<=RMaB && BestTrades.ContainsKey(k.Value)).Select(k=> k.Key).ToList();//select any BestTrades key times that are not recorded trades
line=880;
			foreach(var btabar in BTabars){
line=882;
				var t0 = Times[0].GetValueAt(btabar);
				if(!ValidDaysOfWeek.Contains(t0.DayOfWeek)) continue;
				if(t0.Date != BarsArray[0].LastBarTime.Date) continue;
				#region -- Trade setup filters --
line=884;
				PermittedDir = GetPermittedDirection(Closes[0].GetValueAt(btabar), Opens[0].GetValueAt(btabar), MidRangePrice.ContainsKey(btabar) ? MidRangePrice[btabar] : Closes[0].GetValueAt(btabar), Medians[0].GetValueAt(btabar));
				#endregion ------
line=886;
				if(PermittedDir != int.MinValue){
line=890;
					v1.X = chartControl.GetXByBarIndex(ChartBars,btabar);
//Print("CB: "+CurrentBars[0]+"  btabar: "+btabar+"  PermittedDir: "+PermittedDir+"   "+Times[0].GetValueAt(btabar).ToShortTimeString());
					v1.Y = 0f;
					double H = Highs[0].GetValueAt(btabar);
					double L = Lows[0].GetValueAt(btabar);
					double range = H - L;
					if(pRangeType==BreakoutResearchTool_RangeType.Daily) range = avgDailyRange;
					double tdist = range * pMultRangeTarget;
					double entryPrice = 0;
					double tgtPrice = 0;
					double slPrice = 0;
					int tkey = ToTime(t0)/100;
					if(PermittedDir == LONG) {
						//Print long signal, entry level, target level, initial stop
						#region -- Draw EntryPrice oval --
						entryPrice = H+TickSize;
						SetEntryPrice = entryPrice;
						v1.Y = (float)chartScale.GetYByValue(entryPrice);
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[2].Width, Plots[2].Width), Plots[2].BrushDX);
						#endregion --
						#region -- Draw historical exit levels --
						if(pShowHistoricalExitLevels){
							var v2 = new SharpDX.Vector2(v1.X-2f,0);
							foreach(var lvl in Trades[tkey]){
								if(lvl.Disposition.CompareTo("Tgt")!=0 && lvl.Disposition.CompareTo("SL")!=0){
									double p = lvl.Direction=='L'? lvl.ExitPrice-lvl.EntryPrice : lvl.EntryPrice-lvl.ExitPrice;
									p = PermittedDir==LONG ? entryPrice + p : entryPrice - p;
									v2.Y = (float)chartScale.GetYByValue(p);
									RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v2, Plots[3].Width, Plots[3].Width), Plots[3].BrushDX);
								}
							}
						}
						#endregion --
						#region -- Draw TargetPrice oval --
						if(pTargetType == BreakoutResearchTool_TargetType.MultOfRange){
//							tgtPrice = entryPrice + tdist;
							tgtPrice = Round2Tick(entryPrice + (pRangeType==BreakoutResearchTool_RangeType.Daily ? GetTargetDistance(tdist/3, tdist, this.Map_Abar_ToTgtReduction[tkey]) : tdist));
						}else
							tgtPrice = entryPrice + pTicksTarget*TickSize;
						v1.Y = (float)chartScale.GetYByValue(tgtPrice);
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), Plots[0].BrushDX);
						#endregion --
						#region -- Draw SLPrice oval --
						slPrice = CalculateInitialSL('L', entryPrice, range);
						v1.Y = (float)chartScale.GetYByValue(slPrice);
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), MagentaBrushDX);
						#endregion --

						if(pRiskPerTrade>0) SuggestedPositionSize = CalculateContracts(EntryLevel[0], slPrice);
					}else if(PermittedDir== SHORT){
						//Print short signal, entry level, target level, initial stop
						#region -- Draw EntryPrice oval --
						entryPrice = L - TickSize;
						SetEntryPrice = entryPrice;
						v1.Y = (float)chartScale.GetYByValue(entryPrice);
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[2].Width, Plots[2].Width), Plots[2].BrushDX);
						#endregion --
						#region -- Draw historical exit levels --
						if(pShowHistoricalExitLevels){
							var v2 = new SharpDX.Vector2(v1.X-2f,0);
							foreach(var lvl in Trades[tkey]){
								if(lvl.Disposition.CompareTo("Tgt")!=0 && lvl.Disposition.CompareTo("SL")!=0){
									double p = lvl.Direction=='L'? lvl.ExitPrice-lvl.EntryPrice : lvl.EntryPrice-lvl.ExitPrice;
									p = PermittedDir==LONG ? entryPrice + p : entryPrice - p;
									v2.Y = (float)chartScale.GetYByValue(p);
									RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v2, Plots[3].Width, Plots[3].Width), Plots[3].BrushDX);
								}
							}
						}
						#endregion --
						#region -- Draw TargetPrice oval --
						if(pTargetType == BreakoutResearchTool_TargetType.MultOfRange){
//							tgtPrice = entryPrice - tdist;
							tgtPrice = Round2Tick(entryPrice - (pRangeType==BreakoutResearchTool_RangeType.Daily ? GetTargetDistance(tdist/3, tdist, this.Map_Abar_ToTgtReduction[tkey]) : tdist));
						}else
							tgtPrice = entryPrice - pTicksTarget*TickSize;
						v1.Y = (float)chartScale.GetYByValue(tgtPrice);
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), Plots[0].BrushDX);
						#endregion --
						#region -- Draw SLPrice oval --
						slPrice = CalculateInitialSL('S', entryPrice, range);
						v1.Y = (float)chartScale.GetYByValue(slPrice);
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, barwidth_f+Plots[0].Width, Plots[0].Width), MagentaBrushDX);
						#endregion --

						if(pRiskPerTrade>0) SuggestedPositionSize = CalculateContracts(EntryLevel[0], slPrice);
					}
				}
			}
			}

			float Xorigin_ofSummaryTable = 20f;
			float Yorigin_ofSummaryTable = 20f;
			v1 = new SharpDX.Vector2(Xorigin_ofSummaryTable,Yorigin_ofSummaryTable);//start summary table 20pxls from top
			SharpDX.RectangleF slineRect;
			var stextFormat = new SimpleFont("Arial",12).ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout stextLayout = null;
			if(DisplayedContent == HIDE_ALL_TABLES){
				#region -- Draw text of summary statement at top of table --
				stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, "Tailwind (Ctrl+1 to toggle show table)", stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
				slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width, stextLayout.Metrics.Height+2f);
				RenderTarget.FillRectangle(new SharpDX.RectangleF(Xorigin_ofSummaryTable, Yorigin_ofSummaryTable, stextLayout.Metrics.Width+1f, stextLayout.Metrics.Height+1f), BlackBrushDX);
				RenderTarget.DrawText("Tailwind (Ctrl+1 to toggle show table)", stextFormat, slineRect, MagentaBrushDX);
				#endregion
			}else if(DisplayedContent == EXAMINE_TABLE){
				#region -- Calculate overall dimensions of Examine table, and widths of each column in the table --
				float space_between_columns = 30f;
				string[] header = new string[9]{"<type>","L/S","Time","Loc","risk","Pnl Pts","Size","PnL$","Disposition"};
/*
trade.Direction, 
trade.EntryDT.ToString("MM/dd/yyyy HH:mm"),
"pct HL: "+trade.LocPercentileInDailyRange.ToString("0%"),
"risk: "+Instrument.MasterInstrument.FormatPrice(Math.Abs(trade.EntryPrice-trade.SLPrice)), 
Instrument.MasterInstrument.FormatPrice(trade.PnL)+"-pts  on ",
trade.Contracts+(trade.Contracts==1 ? "-contract": "-contracts"),
(trade.PnL*POINT_VALUE).ToString("C").Replace(".00",string.Empty),
trade.Disposition
*/
				float[] col_widths = new float[header.Length];
				for(int j = 1; j<header.Length; j++){
					stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, header[j], stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
					col_widths[j] = stextLayout.Metrics.Width;
				}
line=1362;
				float datatablewidth = 0;
				foreach(var bt in ExamineTable){
line=1366;
					if(bt.Value.Count==1) {
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, bt.Value[0], stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						col_widths[0] = Math.Max(col_widths[0], stextLayout.Metrics.Width);
					}else{
						i = 0;
						float rowwidth = 0;
line=1371;
						foreach(var col in bt.Value){
							if(!col.StartsWith("<data>")){
								stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, col, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
								col_widths[i] = Math.Max(col_widths[i], stextLayout.Metrics.Width + space_between_columns);
								rowwidth = rowwidth+col_widths[i];
							}
							i++;
							datatablewidth = Math.Max(datatablewidth, rowwidth);
						}
line=1379;
					}
				}
				float table_overallwidth = Math.Max(col_widths[0], datatablewidth);
				#endregion
				#region -- Draw background fill rectangle for entire table --
				float table_height = (stextLayout.Metrics.Height+1f) * (ExamineTable.Count + 1/*+1 for the header row*/);
				RenderTarget.FillRectangle(new SharpDX.RectangleF(Xorigin_ofSummaryTable, Yorigin_ofSummaryTable, table_overallwidth, table_height), BlackBrushDX);
				#endregion

				int row = ExamineTable.Keys.Min();
				for(row=ExamineTable.Keys.Min(); row<=ExamineTable.Keys.Max(); row++){
					var eline = ExamineTable[row];
					if(eline[0]=="<headers>"){
						#region -- Print column headers --
						i = 0;
						foreach(var h in header){
							if(h!="<type>"){
								stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, h, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
								slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, col_widths[i], stextLayout.Metrics.Height+1f);
								RenderTarget.DrawText(h, stextFormat, slineRect, OrangeBrushDX);
								v1.X = v1.X + col_widths[i];
							}
							i++;
						}
						v1.Y = v1.Y+stextLayout.Metrics.Height+1f;
						#endregion
					}
					else if(eline[0]=="<data>"){
						#region -- Print column data for each winning time slice --
//						var DimGrayBrushDX = Brushes.DimGray.ToDxBrush(RenderTarget);
						v1.X = Xorigin_ofSummaryTable;
						for(i = 1; i< eline.Count; i++){
							stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, eline[i], stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
							slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, col_widths[i], stextLayout.Metrics.Height+1f);
							RenderTarget.DrawText(eline[i], stextFormat, slineRect, WhiteBrushDX);
							v1.X = v1.X + col_widths[i];
						}
						#endregion
					}
					else if(eline.Count==1){
						v1.X = Xorigin_ofSummaryTable;
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, eline[0], stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width+10f, stextLayout.Metrics.Height+1f);
						RenderTarget.FillRectangle(new SharpDX.RectangleF(v1.X, v1.Y, slineRect.Width, slineRect.Height), BlackBrushDX);
						RenderTarget.DrawText(eline[0], stextFormat, slineRect, WhiteBrushDX);
					}
					v1.Y = v1.Y+stextLayout.Metrics.Height+2f;
				}
			}else if(DisplayedContent == SUMMARY_TABLE){
				#region -- Draw Summary Table --
				if(BestTrades.Count>0){
					#region -- Calculate overall dimensions of table, and widths of each column in the table --
					float space_between_columns = 30f;
					string[] header = new string[10]{"W%", "W-L(TP-SL)","Time","Loc", "PnL$","Avg/trade","Long%","Long$","Short%","Short$"};
					float[] col_widths = new float[header.Length];
					for(int j = 0; j<header.Length; j++){
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, header[j], stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						col_widths[j] = stextLayout.Metrics.Width;
					}
					foreach(var bt in BestTrades){
						i = 0;
						foreach(var col in bt.Value.Cols){
							stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, col, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
							col_widths[i] = Math.Max(col_widths[i], stextLayout.Metrics.Width + space_between_columns);
	//Print(i+":  "+bt.Value.Cols[i]+"   width "+col_widths[i]);
							i++;
						}
					}
					#endregion
					#region -- Calculate width of summary statement text at top of table --
					float table_overallwidth = col_widths.Sum();
					foreach(var LN in SummaryTable.Values){
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, LN, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						table_overallwidth = Math.Max(table_overallwidth, stextLayout.Metrics.Width+5f);
					}
					#endregion
					#region -- Draw background fill rectangle for entire table --
					float table_height = (stextLayout.Metrics.Height+1f) * (SummaryTable.Count + BestTrades.Count + 1/*+1 for the header row*/);
					RenderTarget.FillRectangle(new SharpDX.RectangleF(Xorigin_ofSummaryTable, Yorigin_ofSummaryTable, table_overallwidth, table_height), BlackBrushDX);
					#endregion
					#region -- Draw text of summary statement at top of table --
					string txt = string.Empty;
					foreach(var LN in SummaryTable.Values){
						if(LN.StartsWith("DOW:")) txt = string.Format("{0}  {1}",LN,Times[0].GetValueAt(RMaB).ToShortDateString());
						else txt = LN;
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, txt, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						slineRect  = new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width, stextLayout.Metrics.Height+2f);
						RenderTarget.DrawText(txt, stextFormat, slineRect, PermittedDir == int.MinValue ? WhiteBrushDX :(PermittedDir>0 ? GreenBrushDX : MagentaBrushDX));
						v1.Y = v1.Y+stextLayout.Metrics.Height+1f;
					}
					#endregion
					#region -- Print column headers --
					i = 0;
					foreach(var h in header){
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, h, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, col_widths[i], stextLayout.Metrics.Height+1f);
						RenderTarget.DrawText(h, stextFormat, slineRect, OrangeBrushDX);
						v1.X = v1.X + col_widths[i];
						i++;
					}
					v1.Y = v1.Y+stextLayout.Metrics.Height+1f;
					#endregion
					#region -- Print column data for each winning time slice --
					var DimGrayBrushDX = Brushes.DimGray.ToDxBrush(RenderTarget);
					v1.X = Xorigin_ofSummaryTable;
					bool IsCurrentTime = false;
					int CurrentTimeRow = 0;
					foreach(var bt in BestTrades){
						i = 0;
						int key = int.Parse(bt.Key.Replace(":",string.Empty));
						foreach(var col in bt.Value.Cols){
							stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, col, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
							slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, col_widths[i], stextLayout.Metrics.Height+1f);
							if(key<tnow)
								RenderTarget.DrawText(col, stextFormat, slineRect, DimGrayBrushDX);
							else{
								bool UseCyan = CurrentTimeRow==0 && (i==2 || (PermittedDir>0 && (i==6||i==7)) || (PermittedDir<0 && (i==8||i==9)));
//								bool UseCyan = i==2 || CurrentTimeRow==0 && (PermittedDir>0 && (i==5||i==6)) || (PermittedDir<0 && (i==7||i==8)));
								IsCurrentTime = true;
								RenderTarget.DrawText(col, stextFormat, slineRect, 
									UseCyan ? CyanBrushDX :
										(PermittedDir == int.MinValue ? WhiteBrushDX :(PermittedDir>0 ? GreenBrushDX : MagentaBrushDX))
								);
								if(IsFirstTickOfBar && LastExaminedKey != key){
									LastExaminedKey = key;
									//PrintExamineTime(key);
								}
							}

							v1.X = v1.X + col_widths[i];
	//Print(i+":  "+bt.Value.Cols[i]+"   width "+col_widths[i]);
							i++;
						}
						if(IsCurrentTime) CurrentTimeRow++;
						v1.X = Xorigin_ofSummaryTable;
						v1.Y = v1.Y+stextLayout.Metrics.Height+1f;
					}
					DimGrayBrushDX.Dispose();
					#endregion
					if(this.pMaxContracts>1 && pRiskPerTrade>0 && SuggestedPositionSize != -999){
						v1.Y = v1.Y+stextLayout.Metrics.Height+1f;
						string sugsizestr = ". NO TRADE - too much risk .";
						if(SuggestedPositionSize>=1){
							sugsizestr = string.Format(". {0}-contract{1} calculated .",SuggestedPositionSize, SuggestedPositionSize == 1 ? "s" : null);
						}
						stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, sugsizestr, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
						slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width, stextLayout.Metrics.Height+1f);
						RenderTarget.FillRectangle(new SharpDX.RectangleF(v1.X, v1.Y, slineRect.Width, slineRect.Height), WhiteBrushDX);
						RenderTarget.DrawText(sugsizestr, stextFormat, slineRect, BlackBrushDX);
					}
				}
				if(InstrumentDescription.Length>0 && stextLayout!=null){
					v1.Y = v1.Y+stextLayout.Metrics.Height+1f;
					stextFormat = new SimpleFont("Arial",24).ToDirectWriteTextFormat();
					stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, InstrumentDescription, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
					RenderTarget.FillRectangle(new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width, stextLayout.Metrics.Height), BlackBrushDX);
					RenderTarget.DrawText(InstrumentDescription, stextFormat, new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width, stextLayout.Metrics.Height), WhiteBrushDX);
				}
				#endregion -------------
			}

			if(stextFormat!=null) stextFormat.Dispose();
			if(stextLayout!=null) stextLayout.Dispose();
}catch(Exception e){Print(line+":  "+Instrument.FullName+"  "+Bars.BarsPeriod.ToString()+"  "+e.ToString());}
		}
//======================================================================================================
		private int GetPermittedDirection(double C, double O, double mid, double bar_median){
			int PermittedDir = int.MinValue;
line=1645;
			if(pExcludeNoMansLand){
				if(C <= mid+NoMansLandPts && C >= mid-NoMansLandPts) return int.MinValue;
			}
			if(Filter_BarClosingDirection && Filter_MidDayPrice_BuyBelowSellAbove){//revert back to mid price
				if(C>O && C < mid) PermittedDir = LONG;
				else if(C<O && C > mid) PermittedDir = SHORT;
			}
			else if(Filter_BarClosingDirection && Filter_MidDayPrice_BuyAboveSellBelow){//breakout away from mid price
line=1651;
				if(C>O && C > mid) PermittedDir = LONG;
				else if(C<O && C < mid) PermittedDir = SHORT;
			}
			else if(Filter_BarClosingDirection){
line=1656;
				if(C>O) PermittedDir = LONG; else PermittedDir = SHORT;
			}
			else if(Filter_MidDayPrice_BuyBelowSellAbove){//revert back to mid price
line=1660;
				if(C < mid) PermittedDir = LONG; else PermittedDir = SHORT;
			}
			else if(Filter_MidDayPrice_BuyAboveSellBelow){//breakout away from mid price
line=1664;
				if(C > mid) PermittedDir = LONG; else PermittedDir = SHORT;
			}else{
line=1667;
				PermittedDir = 0;//...permit either long or short here          C > bar_median ? LONG:SHORT;
			}
line=1670;
			return PermittedDir;
		}
//======================================================================================================
		private DateTime Now
		{	get{
				now = (Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now);
				if (now.Millisecond > 0)
					now = Core.Globals.MinDate.AddSeconds((long)Math.Floor(now.Subtract(Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
//======================================================================================================
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
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";

				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("none");
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
//======================================================================================================
		private string AddSoundFolder(string wav){
			if(Instruments.Length>1)
				wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst2>",Instruments[1].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			if(Instruments.Length>0)
				wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Target Type", Order=5, GroupName="Parameters")]
		public BreakoutResearchTool_TargetType pTargetType
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Range Type", Order=7, GroupName="Parameters", Description="What type of range is used for calculation of range-based TP and SL")]
		public BreakoutResearchTool_RangeType pRangeType {get;set;}

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Ticks Target", Order=10, GroupName="Parameters", Description="Number of ticks beyond entry price")]
		public int pTicksTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Mult Range: Target", Order=20, GroupName="Parameters", Description="Multiples of key bars' range for target")]
		public double pMultRangeTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Mult Range: SL", Order=25, GroupName="Parameters", Description="SL distance as a multiple of the key bars' range")]
		public double pSL_RangeMult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Buffer Ticks SL", Order=30, GroupName="Parameters", Description="Number of additional ticks below low of key bar for a long, or above high for a short")]
		public int pTicksSLBuffer
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Max waiting bars", Order=40, GroupName="Parameters", Description="How many bars will you permit entry orders to be active?  Cancel all entry orders if too many bars transpire without a fill")]
		public int pMaxBarsWaitingForEntry
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Days of week", Order=50, GroupName="Parameters", Description="Focus your historical data to the day of the week.  Valid values are 'TODAY', 'ALL', or 'M', 'Tu', 'W', 'Th', 'F', 'Sa', 'Su'")]
		public string pDaysOfWeek
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Engage BE stop", Order=60, GroupName="Parameters", Description="When the first bar is fully in profit, set the stop to BE")]
		public bool pEngageBEStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Engage 2-bar trailing stop", Order=70, GroupName="Parameters", Description="When the first bar is fully in profit, trail the stop by 2-bars")]
		public bool pEngageTrailingStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Closing direction filter", Order=80, GroupName="Parameters", Description="Permit buys when signal bar is up-closing, permit sells when signal bar is down-closing")]
		public bool Filter_BarClosingDirection
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Revert to midprice filter", Order=90, GroupName="Parameters", Description="Permit buys when close price is below mid-day price, permit sells when it's above mid-day price")]
		public bool Filter_MidDayPrice_BuyBelowSellAbove
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Breakout from midprice filter", Order=91, GroupName="Parameters", Description="Permit buys when close price is above mid-day price, permit sells when it's below mid-day price")]
		public bool Filter_MidDayPrice_BuyAboveSellBelow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="Entry Start", Order=100, GroupName="Parameters")]
		public int pStartTime {get;set;}

		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="Entry End", Order=110, GroupName="Parameters")]
		public int pEndTime {get;set;}

		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="Go Flat by", Order=120, GroupName="Parameters")]
		public int pGoFlatTime {get;set;}

		[NinjaScriptProperty]
		[Range(-1, 2359)]
		[Display(Name="Examine time", Order=130, GroupName="Parameters")]
		public int pExamineTime {get;set;}

		[NinjaScriptProperty]
		[Display(Name="Show hist exit levels on each trade", Order=135, GroupName="Parameters")]
		public bool pShowHistoricalExitLevels {get;set;}

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Min. Winner count ", Order=140, GroupName="Parameters")]
		public int pMinWinCount {get;set;}

		[NinjaScriptProperty]
		[Display(Name="Minimum Avg $/trade", Order=150, GroupName="Parameters")]
		public double pMinAvgDollarsPerTrade {get;set;}

		[NinjaScriptProperty]
		[Display(Name="Block out of session times", Order=160, GroupName="Parameters")]
		public bool pBlockOutOfSessionTimes {get;set;}
		
		[NinjaScriptProperty]
		[Display(Name="Ignore dates", Order=170, GroupName="Parameters", Description="m/d/y, m/d/y for dates to ignore")]
		public string pIgnoreDatesCSV {get;set;}

		[Display(Order = 10, Name = "Alert WAV", GroupName = "Alerts")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string pAlertWAV { get; set; }
		
		[Display(Order = 20, Name = "Entry Level Hit WAV", GroupName = "Alerts")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string pEntryLevelHitWAV {get;set;}

		[Display(Order = 30, Name = "Show trades detail in OutputWindow", GroupName = "Alerts")]
		public bool pShowTradesInOutputWindow {get;set;}

		[Display(Order = 40, Name = "Show Trend bars", GroupName = "Alerts", Description="Long and Short (green and magenta) trend bars at bottom of chart")]
		public bool pShowTrendBars {get;set;}

		[Display(Order = 10, Name = "Account size ($)", GroupName = "Risk parameters")]
		public double pAccountSize {get;set;}

		[Display(Order = 20, Name = "Risk % per trade", GroupName = "Risk parameters", Description="Set to '0' to turn-off risk based sizing")]
		public double pRiskPerTrade {get;set;}

		[Display(Order = 30, Name = "Max contracts", GroupName = "Risk parameters", Description="")]
		public double pMaxContracts {get;set;}
		
		#endregion

		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TargetLevel
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MidPriceOfDay
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EntryLevel
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HistExitLevels
		{
			get { return Values[3]; }
		}
		#endregion

	}
}
public enum BreakoutResearchTool_RangeType {Daily, KeyBar}
public enum BreakoutResearchTool_TargetType {Ticks, MultOfRange}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BreakoutResearchTool[] cacheBreakoutResearchTool;
		public BreakoutResearchTool BreakoutResearchTool(BreakoutResearchTool_TargetType pTargetType, BreakoutResearchTool_RangeType pRangeType, int pTicksTarget, double pMultRangeTarget, double pSL_RangeMult, int pTicksSLBuffer, int pMaxBarsWaitingForEntry, string pDaysOfWeek, bool pEngageBEStop, bool pEngageTrailingStop, bool filter_BarClosingDirection, bool filter_MidDayPrice_BuyBelowSellAbove, bool filter_MidDayPrice_BuyAboveSellBelow, int pStartTime, int pEndTime, int pGoFlatTime, int pExamineTime, bool pShowHistoricalExitLevels, int pMinWinCount, double pMinAvgDollarsPerTrade, bool pBlockOutOfSessionTimes, string pIgnoreDatesCSV)
		{
			return BreakoutResearchTool(Input, pTargetType, pRangeType, pTicksTarget, pMultRangeTarget, pSL_RangeMult, pTicksSLBuffer, pMaxBarsWaitingForEntry, pDaysOfWeek, pEngageBEStop, pEngageTrailingStop, filter_BarClosingDirection, filter_MidDayPrice_BuyBelowSellAbove, filter_MidDayPrice_BuyAboveSellBelow, pStartTime, pEndTime, pGoFlatTime, pExamineTime, pShowHistoricalExitLevels, pMinWinCount, pMinAvgDollarsPerTrade, pBlockOutOfSessionTimes, pIgnoreDatesCSV);
		}

		public BreakoutResearchTool BreakoutResearchTool(ISeries<double> input, BreakoutResearchTool_TargetType pTargetType, BreakoutResearchTool_RangeType pRangeType, int pTicksTarget, double pMultRangeTarget, double pSL_RangeMult, int pTicksSLBuffer, int pMaxBarsWaitingForEntry, string pDaysOfWeek, bool pEngageBEStop, bool pEngageTrailingStop, bool filter_BarClosingDirection, bool filter_MidDayPrice_BuyBelowSellAbove, bool filter_MidDayPrice_BuyAboveSellBelow, int pStartTime, int pEndTime, int pGoFlatTime, int pExamineTime, bool pShowHistoricalExitLevels, int pMinWinCount, double pMinAvgDollarsPerTrade, bool pBlockOutOfSessionTimes, string pIgnoreDatesCSV)
		{
			if (cacheBreakoutResearchTool != null)
				for (int idx = 0; idx < cacheBreakoutResearchTool.Length; idx++)
					if (cacheBreakoutResearchTool[idx] != null && cacheBreakoutResearchTool[idx].pTargetType == pTargetType && cacheBreakoutResearchTool[idx].pRangeType == pRangeType && cacheBreakoutResearchTool[idx].pTicksTarget == pTicksTarget && cacheBreakoutResearchTool[idx].pMultRangeTarget == pMultRangeTarget && cacheBreakoutResearchTool[idx].pSL_RangeMult == pSL_RangeMult && cacheBreakoutResearchTool[idx].pTicksSLBuffer == pTicksSLBuffer && cacheBreakoutResearchTool[idx].pMaxBarsWaitingForEntry == pMaxBarsWaitingForEntry && cacheBreakoutResearchTool[idx].pDaysOfWeek == pDaysOfWeek && cacheBreakoutResearchTool[idx].pEngageBEStop == pEngageBEStop && cacheBreakoutResearchTool[idx].pEngageTrailingStop == pEngageTrailingStop && cacheBreakoutResearchTool[idx].Filter_BarClosingDirection == filter_BarClosingDirection && cacheBreakoutResearchTool[idx].Filter_MidDayPrice_BuyBelowSellAbove == filter_MidDayPrice_BuyBelowSellAbove && cacheBreakoutResearchTool[idx].Filter_MidDayPrice_BuyAboveSellBelow == filter_MidDayPrice_BuyAboveSellBelow && cacheBreakoutResearchTool[idx].pStartTime == pStartTime && cacheBreakoutResearchTool[idx].pEndTime == pEndTime && cacheBreakoutResearchTool[idx].pGoFlatTime == pGoFlatTime && cacheBreakoutResearchTool[idx].pExamineTime == pExamineTime && cacheBreakoutResearchTool[idx].pShowHistoricalExitLevels == pShowHistoricalExitLevels && cacheBreakoutResearchTool[idx].pMinWinCount == pMinWinCount && cacheBreakoutResearchTool[idx].pMinAvgDollarsPerTrade == pMinAvgDollarsPerTrade && cacheBreakoutResearchTool[idx].pBlockOutOfSessionTimes == pBlockOutOfSessionTimes && cacheBreakoutResearchTool[idx].pIgnoreDatesCSV == pIgnoreDatesCSV && cacheBreakoutResearchTool[idx].EqualsInput(input))
						return cacheBreakoutResearchTool[idx];
			return CacheIndicator<BreakoutResearchTool>(new BreakoutResearchTool(){ pTargetType = pTargetType, pRangeType = pRangeType, pTicksTarget = pTicksTarget, pMultRangeTarget = pMultRangeTarget, pSL_RangeMult = pSL_RangeMult, pTicksSLBuffer = pTicksSLBuffer, pMaxBarsWaitingForEntry = pMaxBarsWaitingForEntry, pDaysOfWeek = pDaysOfWeek, pEngageBEStop = pEngageBEStop, pEngageTrailingStop = pEngageTrailingStop, Filter_BarClosingDirection = filter_BarClosingDirection, Filter_MidDayPrice_BuyBelowSellAbove = filter_MidDayPrice_BuyBelowSellAbove, Filter_MidDayPrice_BuyAboveSellBelow = filter_MidDayPrice_BuyAboveSellBelow, pStartTime = pStartTime, pEndTime = pEndTime, pGoFlatTime = pGoFlatTime, pExamineTime = pExamineTime, pShowHistoricalExitLevels = pShowHistoricalExitLevels, pMinWinCount = pMinWinCount, pMinAvgDollarsPerTrade = pMinAvgDollarsPerTrade, pBlockOutOfSessionTimes = pBlockOutOfSessionTimes, pIgnoreDatesCSV = pIgnoreDatesCSV }, input, ref cacheBreakoutResearchTool);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BreakoutResearchTool BreakoutResearchTool(BreakoutResearchTool_TargetType pTargetType, BreakoutResearchTool_RangeType pRangeType, int pTicksTarget, double pMultRangeTarget, double pSL_RangeMult, int pTicksSLBuffer, int pMaxBarsWaitingForEntry, string pDaysOfWeek, bool pEngageBEStop, bool pEngageTrailingStop, bool filter_BarClosingDirection, bool filter_MidDayPrice_BuyBelowSellAbove, bool filter_MidDayPrice_BuyAboveSellBelow, int pStartTime, int pEndTime, int pGoFlatTime, int pExamineTime, bool pShowHistoricalExitLevels, int pMinWinCount, double pMinAvgDollarsPerTrade, bool pBlockOutOfSessionTimes, string pIgnoreDatesCSV)
		{
			return indicator.BreakoutResearchTool(Input, pTargetType, pRangeType, pTicksTarget, pMultRangeTarget, pSL_RangeMult, pTicksSLBuffer, pMaxBarsWaitingForEntry, pDaysOfWeek, pEngageBEStop, pEngageTrailingStop, filter_BarClosingDirection, filter_MidDayPrice_BuyBelowSellAbove, filter_MidDayPrice_BuyAboveSellBelow, pStartTime, pEndTime, pGoFlatTime, pExamineTime, pShowHistoricalExitLevels, pMinWinCount, pMinAvgDollarsPerTrade, pBlockOutOfSessionTimes, pIgnoreDatesCSV);
		}

		public Indicators.BreakoutResearchTool BreakoutResearchTool(ISeries<double> input , BreakoutResearchTool_TargetType pTargetType, BreakoutResearchTool_RangeType pRangeType, int pTicksTarget, double pMultRangeTarget, double pSL_RangeMult, int pTicksSLBuffer, int pMaxBarsWaitingForEntry, string pDaysOfWeek, bool pEngageBEStop, bool pEngageTrailingStop, bool filter_BarClosingDirection, bool filter_MidDayPrice_BuyBelowSellAbove, bool filter_MidDayPrice_BuyAboveSellBelow, int pStartTime, int pEndTime, int pGoFlatTime, int pExamineTime, bool pShowHistoricalExitLevels, int pMinWinCount, double pMinAvgDollarsPerTrade, bool pBlockOutOfSessionTimes, string pIgnoreDatesCSV)
		{
			return indicator.BreakoutResearchTool(input, pTargetType, pRangeType, pTicksTarget, pMultRangeTarget, pSL_RangeMult, pTicksSLBuffer, pMaxBarsWaitingForEntry, pDaysOfWeek, pEngageBEStop, pEngageTrailingStop, filter_BarClosingDirection, filter_MidDayPrice_BuyBelowSellAbove, filter_MidDayPrice_BuyAboveSellBelow, pStartTime, pEndTime, pGoFlatTime, pExamineTime, pShowHistoricalExitLevels, pMinWinCount, pMinAvgDollarsPerTrade, pBlockOutOfSessionTimes, pIgnoreDatesCSV);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BreakoutResearchTool BreakoutResearchTool(BreakoutResearchTool_TargetType pTargetType, BreakoutResearchTool_RangeType pRangeType, int pTicksTarget, double pMultRangeTarget, double pSL_RangeMult, int pTicksSLBuffer, int pMaxBarsWaitingForEntry, string pDaysOfWeek, bool pEngageBEStop, bool pEngageTrailingStop, bool filter_BarClosingDirection, bool filter_MidDayPrice_BuyBelowSellAbove, bool filter_MidDayPrice_BuyAboveSellBelow, int pStartTime, int pEndTime, int pGoFlatTime, int pExamineTime, bool pShowHistoricalExitLevels, int pMinWinCount, double pMinAvgDollarsPerTrade, bool pBlockOutOfSessionTimes, string pIgnoreDatesCSV)
		{
			return indicator.BreakoutResearchTool(Input, pTargetType, pRangeType, pTicksTarget, pMultRangeTarget, pSL_RangeMult, pTicksSLBuffer, pMaxBarsWaitingForEntry, pDaysOfWeek, pEngageBEStop, pEngageTrailingStop, filter_BarClosingDirection, filter_MidDayPrice_BuyBelowSellAbove, filter_MidDayPrice_BuyAboveSellBelow, pStartTime, pEndTime, pGoFlatTime, pExamineTime, pShowHistoricalExitLevels, pMinWinCount, pMinAvgDollarsPerTrade, pBlockOutOfSessionTimes, pIgnoreDatesCSV);
		}

		public Indicators.BreakoutResearchTool BreakoutResearchTool(ISeries<double> input , BreakoutResearchTool_TargetType pTargetType, BreakoutResearchTool_RangeType pRangeType, int pTicksTarget, double pMultRangeTarget, double pSL_RangeMult, int pTicksSLBuffer, int pMaxBarsWaitingForEntry, string pDaysOfWeek, bool pEngageBEStop, bool pEngageTrailingStop, bool filter_BarClosingDirection, bool filter_MidDayPrice_BuyBelowSellAbove, bool filter_MidDayPrice_BuyAboveSellBelow, int pStartTime, int pEndTime, int pGoFlatTime, int pExamineTime, bool pShowHistoricalExitLevels, int pMinWinCount, double pMinAvgDollarsPerTrade, bool pBlockOutOfSessionTimes, string pIgnoreDatesCSV)
		{
			return indicator.BreakoutResearchTool(input, pTargetType, pRangeType, pTicksTarget, pMultRangeTarget, pSL_RangeMult, pTicksSLBuffer, pMaxBarsWaitingForEntry, pDaysOfWeek, pEngageBEStop, pEngageTrailingStop, filter_BarClosingDirection, filter_MidDayPrice_BuyBelowSellAbove, filter_MidDayPrice_BuyAboveSellBelow, pStartTime, pEndTime, pGoFlatTime, pExamineTime, pShowHistoricalExitLevels, pMinWinCount, pMinAvgDollarsPerTrade, pBlockOutOfSessionTimes, pIgnoreDatesCSV);
		}
	}
}

#endregion
