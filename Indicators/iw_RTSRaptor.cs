//#define ROBERT_WILLIAMS_CUSTOM


#region Using declarations
using System;
//using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Reflection;
using System.Collections.Generic;
#endregion
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
using System.Linq;
using SBG_Raptor;

namespace NinjaTrader.NinjaScript.Indicators
{
	public enum Raptor_PermittedDirections {Both, Long, Short, Trend}
    #region -- Category Order --
    [CategoryOrder("Hawk Bars", 10)]
    [CategoryOrder("Cloud Setup", 20)]
    [CategoryOrder("Visual", 30)]
    [CategoryOrder("Presignals Visual", 40)]
    [CategoryOrder("Signal Parameters HE", 50)]
    [CategoryOrder("Signal Parameters SE", 60)]
    [CategoryOrder("Special Signals", 70)]
    [CategoryOrder("Special Signals Visual", 80)]
    [CategoryOrder("Audible", 90)]
    [CategoryOrder("1.0 Audible", 100)]
    [CategoryOrder("2.0 Audible", 110)]
    [CategoryOrder("Alert", 120)]
    [CategoryOrder("Trade Assist", 130)]
	#endregion

   // [TypeConverter("NinjaTrader.NinjaScript.Indicators.RaptorConverter")]
	public class RTS_Raptor : Indicator
	{
		private const int Brush_UpArrow_ID = 1;
		private const int Brush_DownArrow_ID = 2;
		private const int CloudCrossover_BuyBrush_ID = 3;
		private const int CloudCrossover_SellBrush_ID = 4;
		private const int SoftEdge_BuyBrush_ID = 5;
		private const int SoftEdge_SellBrush_ID = 6;
		private const int HardEdge_BuyBrush_ID = 7;
		private const int HardEdge_SellBrush_ID = 8;

		private class iwTmAtrTrail
		{
			#region Class

			#region RegInputs
			
			private int iAtrLength = 14; 
			private double iAtrMult = 2.0; 
			
			#endregion
			#region variables
			public bool bPrimed = false;
			public Series<double> sTrail;
//			public IDataSeries Close;
//			public IDataSeries Open;
			public double hHigh, lLow;
			#endregion
			public double trail = 0;
			public double trailprior = 0;
			public string Status = string.Empty;

			public iwTmAtrTrail(int atrLength, double atrMult){
				iAtrMult = atrMult;
				iAtrLength = atrLength;
			}
			
			public void OnBarUpdate(int CurrentBar, double atrValue, double Close1, double Close0, double Open0, bool NewBar)
			{	
				if(CurrentBar < iAtrLength)
					return;
				Status = string.Empty;
				// Prime
				if(!bPrimed) {
					if(CurrentBar >= iAtrLength)
					{
						bPrimed = true;
						if(Close0 > Open0){
							trail = Close0 - (atrValue * iAtrMult);
						}
						else if(Close0 < Open0){
							trail = Close0 + (atrValue * iAtrMult);
						}else{
							trail = Close0;
						}

						hHigh = lLow = Close0;
					}
					else
					{				
						trail = trailprior;
					}
					return;
				}


				if(NewBar) trailprior = trail;
				if(Close0 > trail)
				{
					if(Close1 > trailprior)
					{
						if(Close0 > hHigh)
						{
							trail = Math.Max(trail, Close0 - (atrValue * iAtrMult));
							//Status = "1134 Close above all trails and prior high  "+Close0+" - "+atrValue.ToString("0.00")+" * "+iAtrMult+" = "+trail.ToString("0.00");
						}
					}
					else
					{
						hHigh = lLow = Close0;					
						trail = Close0 - (atrValue * iAtrMult);
						//Status = "1142 Trend reversed to UP  "+Close0+" - "+atrValue.ToString("0.00")+" * "+iAtrMult+" = "+trail.ToString("0.00");
					}
				}	
				
				if(Close0 < trail)
				{
					if(Close1 < trailprior)
					{
						if(Close0 < lLow)
						{
							trail = Math.Min(trail, Close0 + (atrValue * iAtrMult));
							//Status = "1156 Close below all trails and prior low  "+Close0+" + "+atrValue.ToString("0.00")+" * "+iAtrMult+" = "+trail.ToString("0.00");
						}
					}
					else
					{
						hHigh = lLow = Close0;
						trail = Close0 + (atrValue * iAtrMult);
						//Status = "1164 Trend reversed to DOWN  "+Close0+" + "+atrValue.ToString("0.00")+" * "+iAtrMult+" = "+trail.ToString("0.00");
					}
				}

				// Update high / low
				if(Close0 > hHigh)
					hHigh = Close0;
				if(Close0 < lLow)
					lLow = Close0;
			}
			#endregion
		}

		private string ModuleName = "iwRaptor";

		private const int LONG = 1;
		private const int FLAT = 0;
		private const int SHORT = -1;
        #region Variables
		private bool RunInit = true;
		private Stochastics stoch;
		private HMA hma;
		private EMA ema;
		private ParabolicSAR psar;
        #endregion
		private int PopupBarOnSetup = -1;
		private int PopupBarOnEntry = -1;
		private int EmailBar = -1;
		private int SoundWarningBar = -1;
		private int SoundEntryBar = -1;
		private int SoundPresignalBar = -1;

		private Series<int> SetupDirection;
		private Series<double> position;
		private Series<double> position_v2;
		private int MostRecentPosition = 0;
//		private Series<double> SoftBandEdgeHigh1, SoftBandEdgeHigh2, SoftBandEdgeLow1, SoftBandEdgeLow2;
//		private Series<double> Cloud1_HE, Cloud1_SEH, Cloud1_SEL;
//		private Series<double> Cloud2_HE, Cloud2_SEH, Cloud2_SEL;
		private string ArrowTag="";
		private bool isBen = false;
		private double BuyPrice = double.MinValue;
		private double SellPrice = double.MinValue;

		private int BuyAlert_ABar = 0;
		private int BuyWarningDot_ABar = 0;
		private int SellAlert_ABar = 0;
		private int SellWarningDot_ABar = 0;
		
		private Swing swing2=null;
		private double atrSeparation = 0;
		private int cbar = 0;

		private int pos = FLAT;
		private iwTmAtrTrail atrFilter=null;
		private Brush barcolor = Brushes.DimGray;
		private int HawkDirection = FLAT;
		private string DebugString = string.Empty;
		private int ArrowBrush = -1;
		private NinjaTrader.Gui.Tools.SimpleFont LabelFont = null;
		private double EntryPrice = double.MinValue;
		private HashSet<int> SwingABarsHash = new HashSet<int>();
		private HashSet<int> PresignalSwingABarsHash = new HashSet<int>();
//		private string BuySoundFile = "";
//		private string SellSoundFile = "";

		private string PreSignalMsg = string.Empty;
		private Brush PresignalBuyBkgBrush = Brushes.Transparent;
		private Brush PresignalSellBkgBrush = Brushes.Transparent;
		private NinjaTrader.Gui.Tools.SimpleFont PresignalFont = null;
        private TextFormat txtFormat_LabelFont = null;
        private TextFormat txtFormat_BandCounterFont = null;

		private class v2SignalDataRecord{
			public int LastArrowABar = 0;
			public int ABar_CloudCrossoverSignal = 0;
			public int ABar_SoftEdgeCounterTrend = -1;
			public int SignalInteger = 0;
			public bool ExtremeHasBeenRetested = false;
		}
		private v2SignalDataRecord v2SignalData;
		private v2SignalDataRecord v2PresignalData;
		private List<double> list = new List<double>(6);

		private SharpDX.Direct2D1.Brush yellowDXBrush = null;
		private SharpDX.Direct2D1.Brush dimgrayDXBrush = null;
		private EMA ema2_13;
		private EMA ema2_21;
		private EMA ema2_55;
		private EMA ema3_13;
		private EMA ema3_21;
		private EMA ema3_55;
		private EMA hawk_EMA;
		private SMA hawk_SMA;
		private IW_HawkMA hawk_RMA;
		string BandEdgeTrendStr = "";

		private class CloudStatus {
			public string Status = string.Empty;
			#region properties
			public int TrendDirection = 0;
			public char TrendStrength = ' ';//'E' = established trend, 'S' = strong trend
			public int ABar_Crossover = -1;
			public int ABar_PriceLeftCloud = -1;
			public int ABar_HardEdgeTouch = -1;
			public double Fast;
			public double Med;
			public double Slow;
			public double SEH;
			public double SEL;
			public bool SearchForHardEdgeTouchSignal = true;
			public bool SearchForRetestExtreme = false;
			public CloudStatus(){TrendDirection = 0; ABar_Crossover = -1; ABar_PriceLeftCloud = -1;
				Fast=0; Med=0; Slow=0; SEH=0; SEL=0;
			}
			#endregion
			#region methods
			public void UpdateTrendDirection(int CurrentBar, double HE, double SEH, double SEL, double High, double Low){
				if(TrendDirection<=FLAT && HE <= SEL){
					TrendDirection = LONG;
					ABar_Crossover = CurrentBar-1;
					TrendStrength = ' ';
					SearchForRetestExtreme = true;
				}
				if(TrendDirection>=FLAT && HE >= SEH){
					TrendDirection = SHORT;
					ABar_Crossover = CurrentBar-1;
					TrendStrength = ' ';
					SearchForRetestExtreme = true;
				}
				if(Low <= SEH && High >= SEL){//if price touched or crossed the cloud
					ABar_PriceLeftCloud = CurrentBar-1;
				}
			}
			public void UpdateTrendStrength(int CurrentBar, double High, double Low){
				if(TrendDirection>0 && Low > SEH){
					TrendStrength = 'E';
				}
				else if(TrendDirection<0 && High < SEL){
					TrendStrength = 'E';
				}
//				else
//					TrendStrength = ' ';//reset trendstrength to 'none'

				if(TrendStrength == 'E' && CurrentBar - ABar_PriceLeftCloud > 20){
					TrendStrength = 'S';
				}
			}
			public void UpdateHardEdgeTouch(int CurrentBar, double h1, double h0, double l1, double l0, double edge1, double edge0){
				if(h1>=edge1 && l1<=edge1) ABar_HardEdgeTouch = CurrentBar-1;
				else if(l1<=edge1 && h0>=edge0) ABar_HardEdgeTouch = CurrentBar-1;
				else if(h1>=edge1 && l0<=edge0) ABar_HardEdgeTouch = CurrentBar-1;
			}
			#endregion
			public int CheckForHardEdgeSignal(int CurrentBar, int MostRecentPosition, int ArrowBar, bool ShowCloudCrossoverSignals, int ABar_CloudCrossoverSignal, int LONG, int SHORT, ref string ArrowTag, ref int ArrowBrush, int pHardEdge_BuyBrush, int pHardEdge_SellBrush, int TrendDirection){
				int result = 0;
				if(ShowCloudCrossoverSignals){
					if(ABar_HardEdgeTouch > ABar_CloudCrossoverSignal) SearchForHardEdgeTouchSignal = true;
					else SearchForHardEdgeTouchSignal = false;
				}else{
					if(ABar_HardEdgeTouch > ABar_Crossover) SearchForHardEdgeTouchSignal = true;
					else SearchForHardEdgeTouchSignal = false;
				}
				bool c1 = TrendStrength=='E' || TrendStrength=='S';
				if(SearchForHardEdgeTouchSignal && c1){
					if(TrendDirection==LONG && MostRecentPosition==LONG){
						ArrowTag = "HardEdgeSignal_Buy"+CurrentBar.ToString();
						ArrowBrush = pHardEdge_BuyBrush;
						ABar_HardEdgeTouch = -1;
						result = LONG;
					}
					if(TrendDirection==SHORT && MostRecentPosition==SHORT){
						ArrowTag = "HardEdgeSignal_Sell"+CurrentBar.ToString();
						ArrowBrush = pHardEdge_SellBrush;
						ABar_HardEdgeTouch = -1;
						result = SHORT;
					}
				}
				return result;
			}
		}
		private SortedDictionary<int,CloudStatus> RaptorClouds = new SortedDictionary<int,CloudStatus>();
		private class v2SignalInfo{
			public char Type;
			public double Price;
			public int Direction;
			public int ForeBrush;
			public char BackBrush=' ';
			public v2SignalInfo(char type, double price, int direction, int foreBrushId, char bkgBrushId){
				Type=type; 
				Price=price; 
				Direction=direction; 
				ForeBrush = foreBrushId;
				BackBrush = bkgBrushId;
			}
		}
		private SortedDictionary<int,v2SignalInfo> SignalLabels = new SortedDictionary<int,v2SignalInfo>();
		private double PV;
		private SBG_Raptor.TradeManager tm;
		private Account myAccount;
		private DateTime MessageTime = DateTime.MinValue;
		string Order_Reject_Message = string.Empty;
		string Log_Order_Reject_Message = string.Empty;
		string ATMStrategyNamesEmployed = "";

		//v2.4 - Fixed ATR issue...ATR was not being calculated correctly on some bar types.  Replaced ATR with SUM(Range(),14)[0]/14.  This affected the location of the 1,2,3 labels on the signal bars
		//v3.2.1 - TargetBasisStr1, 2, 3 was being initialized to "atr 2.5", it is now corrected to "UseATM"
		//v3.2.2 - Added "Trade Assist disabled" message if the current time is outside of the Start/End time parameters
		private const string VERSION = "v3.2.2 Mar.18.2025";
		[Display(Name = "Indicator Version", GroupName = "Indicator Version", Description = "Indicator Version", Order = 0)]
		public string indicatorVersion { get { return VERSION; } }

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "iw RTS Raptor "+System.Text.RegularExpressions.Regex.Match(VERSION, @"^\S+").Value;
				#region Initialize
				
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt");
				IsBen = IsBen && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", ModuleName, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(Brushes.Green,4),     PlotStyle.Hash, "BuyEntryPoint");
				AddPlot(new Stroke(Brushes.Red,4),       PlotStyle.Hash, "SellEntryPoint");
				AddPlot(new Stroke(Brushes.DarkGreen,4), PlotStyle.Dot,  "BuyWarningDot");
				AddPlot(new Stroke(Brushes.Red,4),       PlotStyle.Dot,  "SellWarningDot");
				AddPlot(new Stroke(Brushes.DarkGreen,6), PlotStyle.TriangleUp,   "BuyAlert");
				AddPlot(new Stroke(Brushes.Red,6),       PlotStyle.TriangleDown, "SellAlert");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot,        "FilterLevel");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot,        "PositionAge");
				AddPlot(new Stroke(Brushes.CadetBlue,3), PlotStyle.Line,   "BandEdge1");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "SoftBandEdgeH1");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "SoftBandEdgeL1");
				AddPlot(new Stroke(Brushes.CadetBlue,3), PlotStyle.Line,   "BandEdge2");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "SoftBandEdgeH2");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "SoftBandEdgeL2");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "BandCounter");
				Calculate = Calculate.OnPriceChange;
				IsAutoScale = false;
				IsOverlay = true;
				pATMStrategyName1 = "";
				pATMStrategyName2 = "";
				pATMStrategyName3 = "";
				SelectedAccountName = "";
				pStartTime		= 930;
				pStopTime		= 1550;
				pFlatTime		= 1650;
				pPermittedDOW	= "1,2,3,4,5";
				pShowPnLStats = false;
//				pTargetDistStr1 = "UseATM";
//				pStoplossDistStr1 = "UseATM";
//				pTargetDistStr2 = "atr 2.5";
//				pStoplossDistStr2 = "UseATM";
//				pTargetDistStr3 = "UseATM";
//				pStoplossDistStr3 = "UseATM";
				pDailyTgtDollars = 1000;
				pPermittedDirection = Raptor_PermittedDirections.Both;

				#endregion
			}
	        if (State == State.Configure)
	        {
				#region MeanRenko2 background feeds setup
				AddDataSeries(new BarsPeriod { BarsPeriodType = (BarsPeriodType) 22220, Value = pHawkBarBrickSizeTrend, Value2 = pHawkBarBrickSizeReverse });
				AddDataSeries(new BarsPeriod { BarsPeriodType = (BarsPeriodType) 22220, Value = pBand1_BrickSizeTrend, Value2 = pBand1_BrickSizeReverse });
				AddDataSeries(new BarsPeriod { BarsPeriodType = (BarsPeriodType) 22220, Value = pBand2_BrickSizeTrend, Value2 = pBand2_BrickSizeReverse });
				#endregion

				Calculate=Calculate.OnPriceChange;
				ClearOutputWindow();
				string s = string.Empty;
				string sU = pPermittedDOW.ToUpper();
				if (sU.Contains("ALL")) s = "All";
				else{
					if(sU.Contains("TODAY")) {
						if(DateTime.Now.DayOfWeek==DayOfWeek.Saturday) 
							sU = "M";
						else if(DateTime.Now.DayOfWeek==DayOfWeek.Sunday) 
							sU = "M";
						else
							sU = DateTime.Now.DayOfWeek.ToString().Substring(0,2);
						s = "Today";
					}else{
						if(sU.Contains("M") || sU.Contains("1"))   s = s+"M ";
						if(sU.Contains("TU") || sU.Contains("2"))  s = s+"Tu ";
						if(sU.Contains("W") || sU.Contains("3"))   s = s+"W ";
						if(sU.Contains("TH") || sU.Contains("4"))  s = s+"Th ";
						if(sU.Contains("F") || sU.Contains("5"))   s = s+"F ";
						if(sU.Contains("SA") || sU.Contains("6"))  s = s+"Sa ";
						if(sU.Contains("SU") || sU.Contains("0"))  s = s+"Su ";
						s = s.Trim();
					}
				}

				PV = Instrument.MasterInstrument.PointValue;
				tm = new SBG_Raptor.TradeManager(this, "Raptor", "", "","", Instrument, s, pStartTime, pStopTime, pStopTime, pShowHrOfDayPnL, int.MaxValue, int.MaxValue);
				tm.WarnIfWrongDayOfWeek = false;//do not print the warning message if the current day isn't in the Permitted DOW parameter

				#region -- SLTP for Signal --
				if(pTargetDistStr1.Trim() != "" && pStoplossDistStr1.Trim() != "")
					tm.SLTPs["1"] = new TradeManager.SLTPinfo(pStoplossDistStr1, pTargetDistStr1, pATMStrategyName1);
				if(pTargetDistStr2.Trim() != "" && pStoplossDistStr2.Trim() != "")
					tm.SLTPs["2"] = new TradeManager.SLTPinfo(pStoplossDistStr2, pTargetDistStr2, pATMStrategyName2);
				if(pTargetDistStr3.Trim() != "" && pStoplossDistStr3.Trim() != "")
					tm.SLTPs["3"] = new TradeManager.SLTPinfo(pStoplossDistStr3, pTargetDistStr3, pATMStrategyName3);
				foreach(var id in tm.SLTPs.Keys){
					tm.SLTPs[id].ATRmultTarget = 0;
					tm.SLTPs[id].ATRmultSL = 0;
					if(tm.SLTPs[id].TargetBasisStr.ToLower().Contains("atr")) {
						tm.SLTPs[id].ATRmultTarget = tm.StrToDouble(tm.SLTPs[id].TargetBasisStr);
						tm.SLTPs[id].DollarsTarget = 0;
					}else if(tm.SLTPs[id].TargetBasisStr == "UseATM" && tm.SLTPs[id].ATMname.Length > 0){
						ParseXML(id, "TP");
					}else{
						tm.SLTPs[id].TargetBasisStr = "Undefined";
					}

					if(tm.SLTPs[id].SLBasisStr.ToLower().Contains("atr")) {
						tm.SLTPs[id].ATRmultSL = tm.StrToDouble(tm.SLTPs[id].SLBasisStr);
						tm.SLTPs[id].DollarsSL = 0;
					}else if(tm.SLTPs[id].SLBasisStr == "UseATM" && tm.SLTPs[id].ATMname.Length > 0){
						ParseXML(id, "SL");
					}else{
						tm.SLTPs[id].SLBasisStr = "Undefined";
					}
				}
				#endregion

				#region Configure
				RaptorClouds[1] = new CloudStatus();//Cloud for BIP==2
				RaptorClouds[2] = new CloudStatus();//Cloud for BIP==3
				RaptorClouds[-1] = new CloudStatus();//presignal Cloud for BIP==2
				RaptorClouds[-2] = new CloudStatus();//presignal Cloud for BIP==3
				v2SignalData = new v2SignalDataRecord();
				v2PresignalData = new v2SignalDataRecord();
				LabelFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", this.pSignalLabelFontSize);
				PresignalFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", pPresignalTextSize);
				txtFormat_LabelFont = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	LabelFont.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Bold,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)LabelFont.Size);
				txtFormat_BandCounterFont = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	LabelFont.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)this.pBandCounterFontSize);
			}
			if(State == State.DataLoaded){
				SetupDirection  = new Series<int>(this);//inside State.DataLoaded
				position = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				position_v2 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				#endregion

				swing2 = Swing(BarsArray[0],2);

				SelectedAccountName = SelectedAccountName.Trim();
				#region Verify account availability
				var accts = Account.All.ToList();
				for(int i = 0; i<accts.Count; i++){
					if(accts[i].Name==SelectedAccountName){
						accts=null;
						break;
					}
				}
				if(accts==null){
					lock (Account.All)
					myAccount = Account.All.FirstOrDefault(a => a.Name == SelectedAccountName);
					CheckForExistingOrdersPositions(myAccount);
				}else{
					string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "RaptorTradeAssist.log");
					var lines = new string[]{"x"};
					if(System.IO.File.Exists(path)) lines = System.IO.File.ReadAllLines(path);
					long MostRecent = long.MaxValue;
					foreach(var line in lines){
						if(line.Contains(string.Format("'{0}'",SelectedAccountName))) {
							var elements = line.Split(new char[]{'\t'});
							if(elements.Length>0){
								var dt = DateTime.Parse(elements[0]);
								long diff = DateTime.Now.Ticks - dt.Ticks;
								if(diff<MostRecent) MostRecent=diff;
							}
						}
					}
					var ts = new TimeSpan(MostRecent);
					if(ts.TotalDays>5) System.IO.File.Delete("RaptorTradeAssist.Log");
					else if(ts.TotalMinutes > 10){
						string msg = "ERROR = account '"+SelectedAccountName+"' is not available to trade";
						Log(msg, LogLevel.Alert);
						System.IO.File.AppendAllText("RaptorTradeAssist.Log", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
					}
				}
				ATMStrategyNamesEmployed = $"#1:  '{pATMStrategyName1}'  Qty: {pSignal1Qty}\n#2:  '{pATMStrategyName2}'  Qty: {pSignal2Qty}\n#3:  '{pATMStrategyName3}'  Qty: {pSignal3Qty}";
				#endregion
				Log_Order_Reject_Message = "Your order was rejected.  Account Name '"+SelectedAccountName+"' may not be available on this datafeed connection.  Check for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				Order_Reject_Message = "Your order was rejected.\nAccount Name '"+SelectedAccountName+"' may not be available on this datafeed connection\nCheck for typo errors, or consider changing it to 'Sim101' or 'Replay101'";

				hma = HMA(BarsArray[0],19);
				ema = EMA(BarsArray[0],7);
				psar = ParabolicSAR(BarsArray[0],0.09, 0.2, 0.02);
				stoch = Stochastics(BarsArray[0],3,14,3);
				hma.Calculate  = Calculate.OnBarClose;
				ema.Calculate  = Calculate.OnBarClose;
				psar.Calculate = Calculate.OnBarClose;
				stoch.Calculate= Calculate.OnBarClose;
				swing2.Calculate = Calculate.OnBarClose;
				ema2_13 = EMA(Closes[2],13); ema2_13.Calculate = Calculate.OnPriceChange;
				ema2_21 = EMA(Closes[2],21); ema2_21.Calculate = Calculate.OnPriceChange;
				ema2_55 = EMA(Closes[2],55); ema2_55.Calculate = Calculate.OnPriceChange;
				ema3_13 = EMA(Closes[3],13); ema3_13.Calculate = Calculate.OnPriceChange;
				ema3_21 = EMA(Closes[3],21); ema3_21.Calculate = Calculate.OnPriceChange;
				ema3_55 = EMA(Closes[3],55); ema3_55.Calculate = Calculate.OnPriceChange;
				hawk_SMA = SMA(Closes[1],pBarBrushMALength);  hawk_SMA.Calculate = Calculate.OnPriceChange;
				hawk_EMA = EMA(Closes[1],pBarBrushMALength);  hawk_EMA.Calculate = Calculate.OnPriceChange;
				hawk_RMA = IW_HawkMA(Closes[1],pBarBrushMALength); hawk_RMA.Calculate = Calculate.OnPriceChange;
				if(this.pPresignalBackgroundOpacity>0){
					if(Plots[2].Brush != Brushes.Transparent){
						PresignalBuyBkgBrush = Plots[2].Brush.Clone();
						PresignalBuyBkgBrush.Opacity = this.pPresignalBackgroundOpacity*10.0f;
						PresignalBuyBkgBrush.Freeze();
					}
					if(Plots[3].Brush != Brushes.Transparent){
						PresignalSellBkgBrush = Plots[3].Brush.Clone();
						PresignalSellBkgBrush.Opacity = this.pPresignalBackgroundOpacity*10.0f;
						PresignalSellBkgBrush.Freeze();
					}
				}
                #region -- Add Custom Toolbar --
                if (!isToolBarButtonAdded && ChartControl != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ChartControl.AllowDrop = false;
                        chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                        if (chartWindow == null) return;

                        foreach (DependencyObject item in chartWindow.MainMenu) if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == (toolbarname + uID)) isToolBarButtonAdded = true;

                        if (!isToolBarButtonAdded)
                        {
                            indytoolbar = new System.Windows.Controls.Grid { Visibility = Visibility.Collapsed };

                            addToolBar();

                            chartWindow.MainMenu.Add(indytoolbar);
                            chartWindow.MainTabControl.SelectionChanged += TabSelectionChangedHandler;

                            foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items) if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem) indytoolbar.Visibility = Visibility.Visible;
                            System.Windows.Automation.AutomationProperties.SetAutomationId(indytoolbar, toolbarname + uID);
                        }
                    }));
                }
                #endregion
			}
			if (State == State.Terminated)
			{
				#region Terminated
				if (ChartControl != null)
				{
					//ChartPanel.KeyDown    -= MyKeyDownEvent;
				}
                if (chartWindow != null && indytoolbar != null)
                {
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
                        chartWindow.MainMenu.Remove(indytoolbar);
                        indytoolbar = null;

                        chartWindow.MainTabControl.SelectionChanged -= TabSelectionChangedHandler;
                        chartWindow = null;
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            chartWindow.MainMenu.Remove(indytoolbar);
                            indytoolbar = null;

                            chartWindow.MainTabControl.SelectionChanged -= TabSelectionChangedHandler;
                            chartWindow = null;
                        }));
                    }
                }
				#endregion
			}
		}
		#region -- AddToolBar __
		private List<string> GetATMNames(){
			#region -- GetATMNames --
			string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"templates","AtmStrategy");
			string search = "*.xml";

			System.IO.FileInfo[] filCustom=null;
			try{
				var dirCustom = new System.IO.DirectoryInfo(folder);
				filCustom = dirCustom.GetFiles(search);
			}catch{}

			var list = new List<string>();//new string[filCustom.Length+1];
			if(filCustom!=null){
				foreach (System.IO.FileInfo fi in filCustom)
				{
					if(!fi.Name.ToLower().StartsWith("ignore"))
						list.Add(fi.Name.Replace(".xml",string.Empty));
				}
			}
			if(list.Count>0){
				list.Sort();
			}else{
				list.Clear();
				list.Add("No templates");
			}
			return list;
			#endregion
		}
        private string toolbarname = "Raptor_TB", uID;
        private bool isToolBarButtonAdded = false;
        private NinjaTrader.Gui.Chart.Chart chartWindow;
        private System.Windows.Controls.Grid indytoolbar;
        private System.Windows.Controls.Menu MenuControlContainer;
        private System.Windows.Controls.MenuItem MenuControl;
        private System.Windows.Controls.MenuItem miStatus, miDirection, miShowPnL, miSig1Qty, miSig2Qty, miSig3Qty;
		private System.Windows.Controls.MenuItem miSig1Template, miSig2Template, miSig3Template, miRecalculate1;
		private bool IsTradingPaused = true;
		private bool IsAlwaysActive = false;
		private string DetermineNewStatusHeader(){
			#region -- DeterminNewStatusHeader --
			var str = "";
			if(miStatus.Header.ToString().Contains("ALWAYS")){
				str = "PAUSED";
				IsTradingPaused = true;
				IsAlwaysActive = false;
			}
			else if(miStatus.Header.ToString().Contains("Active")){
				str = "Active ALWAYS";
				IsAlwaysActive = true;
			}
			else if(miStatus.Header.ToString().Contains("PAUSED")){
				str = "Active";
				IsAlwaysActive = false;
				IsTradingPaused = false;
			}
			return $"Status:  {str}";
			#endregion
		}

		private void EnableDisableMenuItem(string type, System.Windows.Controls.MenuItem mi){
			if(type[0]=='D') {//disable
				mi.FontStyle = FontStyles.Italic;
				mi.IsEnabled = false;
			}else{
				mi.FontStyle = FontStyles.Normal;
				mi.IsEnabled = true;
			}
		}
		private void ParseXML(string signalID, string ValueType = "BOTH"){//, ref double qty, ref double sl_pts, ref double tp_pts){
			string fullname = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"templates","AtmStrategy",tm.SLTPs[signalID].ATMname+".xml");
			try{
				var doc = System.Xml.Linq.XDocument.Parse(System.IO.File.ReadAllText(fullname));
	
				var calcmode = doc.Descendants("CalculationMode").FirstOrDefault()?.Value;
				var mult = 1.0;
				if(calcmode == "Ticks") mult = TickSize;
				else if(calcmode == "Pips") mult = TickSize;
				else if(calcmode == "Percent") mult = Closes[0][0] /100.0;
				var brackets = doc.Descendants("Bracket").ToList();
				try{
					if(ValueType == "BOTH" || ValueType == "SL"){
						var sl_pts = double.Parse(brackets[0].Element("StopLoss").Value) * mult;
						tm.SLTPs[signalID].DollarsSL = sl_pts * PV;
						tm.SLTPs[signalID].SLBasisStr = string.Format("${0}", tm.SLTPs[signalID].DollarsSL);
					}
				}catch(Exception e1){Print("722:  "+e1.ToString());}
				try{
					if(ValueType == "BOTH" || ValueType == "TP"){
						var tp_pts = double.Parse(brackets[0].Element("Target").Value) * mult;
						tm.SLTPs[signalID].DollarsTarget = tp_pts * PV;
						tm.SLTPs[signalID].TargetBasisStr = string.Format("${0}", tm.SLTPs[signalID].DollarsTarget);
					}
				}catch(Exception e1){Print("729:  "+e1.ToString());}
			}catch(Exception e2){Print("730:  "+e2.ToString());}
			if(isBen) Print($"ParseXML signal {signalID} SL: {tm.SLTPs[signalID].SLBasisStr}  TP: {tm.SLTPs[signalID].TargetBasisStr}   {fullname}");
		}
        private void addToolBar()
        {
            int rHeight = 30;
			uID = Guid.NewGuid().ToString().Replace("-", string.Empty);
            MenuControlContainer = new System.Windows.Controls.Menu { Background = Brushes.Orange, VerticalAlignment = VerticalAlignment.Center };
            MenuControl = new System.Windows.Controls.MenuItem
            {
                BorderThickness = new Thickness(2),
                Header = "Raptor",
                BorderBrush = Brushes.Cyan,
                Foreground = Brushes.Cyan,
                Background = Brushes.DimGray,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontWeight = FontWeights.Bold,
                FontSize = 13
            };
			MenuControl.MouseEnter += delegate (object o, MouseEventArgs e){
				e.Handled = true;
				miStatus.Header = "Status:  " + (IsAlwaysActive ? "Active ALWAYS" : (IsTradingPaused ? "PAUSED": "Active"));
                miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
//				Print("Status header: "+miStatus.Header.ToString());
			};
            MenuControlContainer.Items.Add(MenuControl);

            #region Status
            miStatus = new System.Windows.Controls.MenuItem { Header = "Status:  " + (IsAlwaysActive ? "Active ALWAYS" : (IsTradingPaused ? "PAUSED": "Active")), Name = "Status" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miStatus.Click += delegate (object o, RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					miStatus.Header = DetermineNewStatusHeader();
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							ForceRefresh();
                        }));
                    }
                }
            };
            miStatus.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
                if (ChartControl != null)
                {
					miStatus.Header = DetermineNewStatusHeader();
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miStatus);
			#endregion

            #region Direction
            miDirection = new System.Windows.Controls.MenuItem { Header = "Direction: " + pPermittedDirection.ToString().ToUpper(), Name = "Direction" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miDirection.Click += delegate (object o, RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						if(pPermittedDirection == Raptor_PermittedDirections.Both) pPermittedDirection = Raptor_PermittedDirections.Short;
						else if(pPermittedDirection == Raptor_PermittedDirections.Short) pPermittedDirection = Raptor_PermittedDirections.Long;
						else if(pPermittedDirection == Raptor_PermittedDirections.Long) pPermittedDirection = Raptor_PermittedDirections.Trend;
						else if(pPermittedDirection == Raptor_PermittedDirections.Trend) pPermittedDirection = Raptor_PermittedDirections.Both;
                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(pPermittedDirection == Raptor_PermittedDirections.Both) pPermittedDirection = Raptor_PermittedDirections.Short;
							else if(pPermittedDirection == Raptor_PermittedDirections.Short) pPermittedDirection = Raptor_PermittedDirections.Long;
							else if(pPermittedDirection == Raptor_PermittedDirections.Long) pPermittedDirection = Raptor_PermittedDirections.Trend;
							else if(pPermittedDirection == Raptor_PermittedDirections.Trend) pPermittedDirection = Raptor_PermittedDirections.Both;
	                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
							InformUserAboutRecalculation();
							ForceRefresh();
                        }));
                    }
                }
            };
            miDirection.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						if(pPermittedDirection == Raptor_PermittedDirections.Both) pPermittedDirection = Raptor_PermittedDirections.Short;
						else if(pPermittedDirection == Raptor_PermittedDirections.Short) pPermittedDirection = Raptor_PermittedDirections.Long;
						else if(pPermittedDirection == Raptor_PermittedDirections.Long) pPermittedDirection = Raptor_PermittedDirections.Trend;
						else if(pPermittedDirection == Raptor_PermittedDirections.Trend) pPermittedDirection = Raptor_PermittedDirections.Both;
                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(pPermittedDirection == Raptor_PermittedDirections.Both) pPermittedDirection = Raptor_PermittedDirections.Short;
							else if(pPermittedDirection == Raptor_PermittedDirections.Short) pPermittedDirection = Raptor_PermittedDirections.Long;
							else if(pPermittedDirection == Raptor_PermittedDirections.Long) pPermittedDirection = Raptor_PermittedDirections.Trend;
							else if(pPermittedDirection == Raptor_PermittedDirections.Trend) pPermittedDirection = Raptor_PermittedDirections.Both;
	                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
							InformUserAboutRecalculation();
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miDirection);
            #endregion

            #region Signal 1 info
            miSig1Qty = new System.Windows.Controls.MenuItem { Header = $"Signal 1 qty: {pSignal1Qty}", Name = "Sig1Qty" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miSig1Qty.Click += delegate (object o, RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						pSignal1Qty++;
						miSig1Qty.Header = $"Signal 1 qty: {pSignal1Qty}";
						InformUserAboutRecalculation();
						if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							pSignal1Qty++;
							miSig1Qty.Header = $"Signal 1 qty: {pSignal1Qty}";
							InformUserAboutRecalculation();
							if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
							ForceRefresh();
                        }));
                    }
                }
            };
            miSig1Qty.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						if(e.Delta>0) pSignal1Qty++; else pSignal1Qty--;
						pSignal1Qty = Math.Max(0, pSignal1Qty);
						miSig1Qty.Header = $"Signal 1 qty: {pSignal1Qty}";
						InformUserAboutRecalculation();
						if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(e.Delta>0) pSignal1Qty++; else pSignal1Qty--;
							pSignal1Qty = Math.Max(0, pSignal1Qty);
							miSig1Qty.Header = $"Signal 1 qty: {pSignal1Qty}";
							InformUserAboutRecalculation();
							if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miSig1Qty);
            #endregion
            #region Signal 1 ATM Template
            miSig1Template = new System.Windows.Controls.MenuItem { Header = $" ATM Name: '{pATMStrategyName1}'", Name = "Sig1Template" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
			if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
            miSig1Template.Click += delegate (object o, RoutedEventArgs e)
            {
				var list = GetATMNames();
				if(list==null || list.Count==0 || list[0] == "No template") return;
				var idx = list.IndexOf(pATMStrategyName1);
				if(idx > list.Count-1) idx = 0; //go to the first element in the list
				if(idx < 0) idx = 0; //if the string wasn't found, go to the first element in the list
				else idx++;
				pATMStrategyName1 = list[idx];
				tm.SLTPs["1"].ATMname = list[idx];

                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						miSig1Template.Header = $" ATM Name: '{pATMStrategyName1}'";
						ParseXML("1");
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							miSig1Template.Header = $" ATM Name: '{pATMStrategyName1}'";
							ParseXML("1");
							InformUserAboutRecalculation();
							ForceRefresh();
                        }));
                    }
					Print("Signal 1: "+tm.SLTPs["1"].ToString());
                }
            };
            miSig1Template.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
				var list = GetATMNames();
				if(list==null || list.Count==0) return;
				var idx = list.IndexOf(pATMStrategyName1);
				if(idx > list.Count-1) idx = 0; //go to the first element in the list
				if(idx < 0) idx = 0;
				if(e.Delta > 0) idx++; else idx--;
				if(idx < 0) idx = list.Count-1;
				if(idx >= list.Count) idx = 0;
				pATMStrategyName1 = list[idx];
				tm.SLTPs["1"].ATMname = list[idx];

				if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						miSig1Template.Header = $" ATM Name: '{pATMStrategyName1}'";
						ParseXML("1");
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							miSig1Template.Header = $" ATM Name: '{pATMStrategyName1}'";
							ParseXML("1");
							InformUserAboutRecalculation();
                            ForceRefresh();
                        }));
                    }
					Print("Signal 1: "+tm.SLTPs["1"].ToString());
                }
            };
            MenuControl.Items.Add(miSig1Template);
            #endregion

            #region Signal 2 info
            miSig2Qty = new System.Windows.Controls.MenuItem { Header = $"Signal 2 qty: {pSignal2Qty}", Name = "Sig2Qty" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miSig2Qty.Click += delegate (object o, RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						pSignal2Qty++;
						miSig2Qty.Header = $"Signal 2 qty: {pSignal2Qty}";
						if(pSignal2Qty == 0) EnableDisableMenuItem("Disable", miSig2Template); else EnableDisableMenuItem("Enable", miSig2Template);
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							pSignal2Qty++;
							miSig2Qty.Header = $"Signal 2 qty: {pSignal2Qty}";
							if(pSignal2Qty == 0) EnableDisableMenuItem("Disable", miSig2Template); else EnableDisableMenuItem("Enable", miSig2Template);
							InformUserAboutRecalculation();
							ForceRefresh();
                        }));
                    }
                }
            };
            miSig2Qty.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						if(e.Delta>0) pSignal2Qty++; else pSignal2Qty--;
						pSignal2Qty = Math.Max(0, pSignal2Qty);
						miSig2Qty.Header = $"Signal 2 qty: {pSignal2Qty}";
						if(pSignal2Qty == 0) EnableDisableMenuItem("Disable", miSig2Template); else EnableDisableMenuItem("Enable", miSig2Template);
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(e.Delta>0) pSignal2Qty++; else pSignal2Qty--;
							pSignal2Qty = Math.Max(0, pSignal2Qty);
							miSig2Qty.Header = $"Signal 2 qty: {pSignal2Qty}";
							if(pSignal2Qty == 0) EnableDisableMenuItem("Disable", miSig2Template); else EnableDisableMenuItem("Enable", miSig2Template);
							InformUserAboutRecalculation();
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miSig2Qty);
            #endregion
            #region Signal 2 ATM Template
            miSig2Template = new System.Windows.Controls.MenuItem { Header = $" ATM Name: '{pATMStrategyName2}'", Name = "Sig2Template" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
			if(pSignal2Qty == 0) EnableDisableMenuItem("Disable", miSig2Template); else EnableDisableMenuItem("Enable", miSig2Template);
            miSig2Template.Click += delegate (object o, RoutedEventArgs e)
            {
				var list = GetATMNames();
				if(list==null || list.Count==0) return;
				var idx = list.IndexOf(pATMStrategyName2);
				if(idx > list.Count-1) idx = 0; //go to the first element in the list
				if(idx < 0) idx = 0; //if the string wasn't found, go to the first element in the list
				else idx++;
				pATMStrategyName2 = list[idx];
				tm.SLTPs["2"].ATMname = list[idx];

                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						miSig2Template.Header = $" ATM Name: '{pATMStrategyName2}'";
						ParseXML("2");
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							miSig2Template.Header = $" ATM Name: '{pATMStrategyName2}'";
							ParseXML("2");
							InformUserAboutRecalculation();
							ForceRefresh();
                        }));
                    }
                }
            };
            miSig2Template.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
				var list = GetATMNames();
				if(list==null || list.Count==0) return;
				var idx = list.IndexOf(pATMStrategyName2);
				if(idx > list.Count-1) idx = 0; //go to the first element in the list
				if(idx < 0) idx = 0;
				if(e.Delta > 0) idx++; else idx--;
				if(idx < 0) idx = list.Count-1;
				if(idx >= list.Count) idx = 0;
				pATMStrategyName2 = list[idx];
				tm.SLTPs["2"].ATMname = list[idx];

				if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						miSig2Template.Header = $" ATM Name: '{pATMStrategyName2}'";
						ParseXML("2");
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							miSig2Template.Header = $" ATM Name: '{pATMStrategyName2}'";
							ParseXML("2");
							InformUserAboutRecalculation();
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miSig2Template);
            #endregion

            #region Signal 3 info
            miSig3Qty = new System.Windows.Controls.MenuItem { Header = $"Signal 3 qty: {pSignal3Qty}", Name = "Sig3Qty" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miSig3Qty.Click += delegate (object o, RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						pSignal3Qty++;
						miSig3Qty.Header = $"Signal 3 qty: {pSignal3Qty}";
						if(pSignal3Qty == 0) EnableDisableMenuItem("Disable", miSig3Template); else EnableDisableMenuItem("Enable", miSig3Template);
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							pSignal3Qty++;
							miSig3Qty.Header = $"Signal 3 qty: {pSignal3Qty}";
							if(pSignal3Qty == 0) EnableDisableMenuItem("Disable", miSig3Template); else EnableDisableMenuItem("Enable", miSig3Template);
							InformUserAboutRecalculation();
							ForceRefresh();
                        }));
                    }
                }
            };
            miSig3Qty.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						if(e.Delta>0) pSignal3Qty++; else pSignal3Qty--;
						pSignal3Qty = Math.Max(0, pSignal3Qty);
						miSig3Qty.Header = $"Signal 3 qty: {pSignal3Qty}";
						if(pSignal3Qty == 0) EnableDisableMenuItem("Disable", miSig3Template); else EnableDisableMenuItem("Enable", miSig3Template);
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(e.Delta>0) pSignal3Qty++; else pSignal3Qty--;
							pSignal3Qty = Math.Max(0, pSignal3Qty);
							miSig3Qty.Header = $"Signal 3 qty: {pSignal3Qty}";
							if(pSignal3Qty == 0) EnableDisableMenuItem("Disable", miSig3Template); else EnableDisableMenuItem("Enable", miSig3Template);
							InformUserAboutRecalculation();
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miSig3Qty);
            #endregion
            #region Signal 3 ATM Template
            miSig3Template = new System.Windows.Controls.MenuItem { Header = $" ATM Name: '{pATMStrategyName3}'", Name = "Sig3Template" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
			if(pSignal3Qty == 0) EnableDisableMenuItem("Disable", miSig3Template); else EnableDisableMenuItem("Enable", miSig3Template);
            miSig3Template.Click += delegate (object o, RoutedEventArgs e)
            {
				var list = GetATMNames();
				if(list==null || list.Count==0) return;
				var idx = list.IndexOf(pATMStrategyName3);
				if(idx > list.Count-1) idx = 0; //go to the first element in the list
				if(idx < 0) idx = 0; //if the string wasn't found, go to the first element in the list
				else idx++;
				pATMStrategyName3 = list[idx];
				tm.SLTPs["3"].ATMname = list[idx];

                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						miSig3Template.Header = $" ATM Name: '{pATMStrategyName3}'";
						ParseXML("3");
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							miSig3Template.Header = $" ATM Name: '{pATMStrategyName3}'";
							ParseXML("3");
							InformUserAboutRecalculation();
							ForceRefresh();
                        }));
                    }
                }
            };
            miSig3Template.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
				var list = GetATMNames();
				if(list==null || list.Count==0) return;
				var idx = list.IndexOf(pATMStrategyName3);
				if(idx > list.Count-1) idx = 0; //go to the first element in the list
				if(idx < 0) idx = 0;
				if(e.Delta > 0) idx++; else idx--;
				if(idx < 0) idx = list.Count-1;
				if(idx >= list.Count) idx = 0;
				pATMStrategyName3 = list[idx];
				tm.SLTPs["3"].ATMname = list[idx];

				if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						miSig3Template.Header = $" ATM Name: '{pATMStrategyName3}'";
						ParseXML("3");
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							miSig3Template.Header = $" ATM Name: '{pATMStrategyName3}'";
							ParseXML("3");
							InformUserAboutRecalculation();
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miSig3Template);
            #endregion

			#region -- Recalc Stats --
			miRecalculate1 = new System.Windows.Controls.MenuItem { Header = "RE-CALCULATE Stats?", HorizontalAlignment = HorizontalAlignment.Center , Background = Brushes.Yellow, Foreground = Brushes.Black, FontWeight = FontWeights.Bold , StaysOpenOnClick = false };
			miRecalculate1.Visibility = Visibility.Collapsed;
			miRecalculate1.Click += delegate (object o, RoutedEventArgs e){
				e.Handled = true;
				ResetRecalculationUI();
				System.Windows.Forms.SendKeys.SendWait("{F5}");
			};
			MenuControl.Items.Add(miRecalculate1);
			#endregion

			MenuControl.Items.Add(new System.Windows.Controls.Separator());

            #region Show PnL stats
            miShowPnL = new System.Windows.Controls.MenuItem { Header = (pShowPnLStats ? "Hide PnL stats?":"Show PnL stats?"), Name = "ShowPnL" + uID, Foreground = Brushes.Black, FontWeight = FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miShowPnL.Click += delegate (object o, RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						pShowPnLStats = !pShowPnLStats;
						miShowPnL.Header = (pShowPnLStats ? "Hide PnL stats?":"Show PnL stats?");
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							pShowPnLStats = !pShowPnLStats;
							miShowPnL.Header = (pShowPnLStats ? "Hide PnL stats?":"Show PnL stats?");
							ForceRefresh();
                        }));
                    }
                }
            };
            miShowPnL.MouseWheel += delegate (object o, System.Windows.Input.MouseWheelEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						pShowPnLStats = !pShowPnLStats;
						miShowPnL.Header = (pShowPnLStats ? "Hide PnL stats?":"Show PnL stats?");
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							pShowPnLStats = !pShowPnLStats;
							miShowPnL.Header = (pShowPnLStats ? "Hide PnL stats?":"Show PnL stats?");
                            ForceRefresh();
                        }));
                    }
                }
            };
            MenuControl.Items.Add(miShowPnL);
            #endregion

            indytoolbar.Children.Add(MenuControlContainer);
		}
		private void InformUserAboutRecalculation(){
			miRecalculate1.Visibility = Visibility.Visible;
//			miRecalculate1.Background = Brushes.Yellow;
	//		miRecalculate1.FontWeight = FontWeights.Bold;
		//	miRecalculate1.FontStyle = FontStyles.Italic;
		}
		private void ResetRecalculationUI(){
			miRecalculate1.Visibility = Visibility.Collapsed;
//			miRecalculate1.FontWeight = FontWeights.Normal;
//			miRecalculate1.FontStyle = FontStyles.Normal;
//			miRecalculate1.Background = null;
		}
        #region private void TabSelectionChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        private void TabSelectionChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            System.Windows.Controls.TabItem tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
            if (tabItem == null) return;
            ChartTab temp = tabItem.Content as ChartTab;
            if (temp != null && indytoolbar != null)
                indytoolbar.Visibility = temp.ChartControl == ChartControl ? Visibility.Visible : Visibility.Collapsed;
        }
		#endregion
        #endregion
//=================================================================================================================================================
		private char GetContrastingBrush(System.Windows.Media.Brush b){
				System.Windows.Media.Color c = ((System.Windows.Media.SolidColorBrush)b).Color;
				byte bRed 	= c.R;
				byte bGreen = c.G;
				byte bBlue 	= c.B;
				double v = (bRed*0.2126)+(bGreen*0.7152)+(bBlue*0.0722);
				if(v>128) return 'B';
				else return 'W';
		}
//=================================================================================================================================================
		private void CheckForExistingOrdersPositions(Account myAccount){
			if(ChartControl.Properties.ChartTraderVisibility != ChartTraderVisibility.Visible){
				string msg = string.Empty;
				if(myAccount.Orders.Count>0) msg = "orders";
				if(myAccount.Positions.Count>0 && msg.Length>0) msg = msg +" and positions"; else msg = "positions";
				if(myAccount.Orders.Count>0 || myAccount.Positions.Count>0){
					MessageTime = DateTime.Now;
					Draw.TextFixed(this, "preexist", "Turn-on ChartTrader to possibly view your current "+msg, TextPosition.BottomLeft,Brushes.Black, new SimpleFont("Arial", 18), Brushes.Red, Brushes.Maroon, 90);
				}
			}
		}
//=================================================================================================================================================
		private int GetCurrentMarketPosition(){
			if(myAccount == null) return 0;
			//var mp = MarketPosition.Flat;
			int pos_size = 0;
			foreach(var ex in myAccount.Positions.Where(k=>k.Instrument == Instruments[0])) {
				pos_size += ex.Quantity * (ex.MarketPosition == MarketPosition.Short? -1 : 1);
				//mp = ex.MarketPosition;
			}
			return pos_size;
		}
		#region - BuyUsingATM -
		int EntryABar = 0;
        void BuyUsingATM(string pATMStrategyName, int Qty, string SignalID)
        {
			if(pPermittedDirection == Raptor_PermittedDirections.Trend && BandEdgeTrendStr != "UP") return;//buy signal is not permitted against down trend
			if(tm.DailyTgtAchieved) {
				Print(Times[0][0].ToShortDateString()+"  Daily PnL target achieved, no buy order submitted");
				return;
			}

			var ordertype   = OrderType.Market;
try{
			if((pPermittedDirection == Raptor_PermittedDirections.Long || pPermittedDirection == Raptor_PermittedDirections.Both || pPermittedDirection == Raptor_PermittedDirections.Trend)){
				var position_size = GetCurrentMarketPosition();
				if(position_size < 0){//we're short, reverse to long requested...exit out the shorts
					myAccount.Flatten(Instruments);
					tm.Note = "Reversing";
					tm.GoFlat(Times[0][0], Closes[0][0]);
				}
				var LongsPermittedNow = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
				if(State==State.Historical && LongsPermittedNow && tm.SLTPs.ContainsKey(SignalID)){
//Print($"Buying: #{SignalID}   current position size: {position_size}");
					double EntryPrice = Closes[0][0] + TickSize;
					double TgtPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice + (tm.SLTPs[SignalID].ATRmultTarget > 0 ? tm.SLTPs[SignalID].ATRmultTarget * atr("points", 14) : tm.SLTPs[SignalID].DollarsTarget / PV));
					double SLPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice - (tm.SLTPs[SignalID].ATRmultSL > 0 ? tm.SLTPs[SignalID].ATRmultSL * atr("points", 14) : tm.SLTPs[SignalID].DollarsSL / PV));
					Draw.Dot(this, $"tgt{CurrentBars[0]}", false, 0, TgtPrice, Brushes.LimeGreen);
					Draw.Dot(this, $"sl{CurrentBars[0]}", false, 0, SLPrice, Brushes.Magenta);
					Print($"------ {Times[0][0].ToString()} Now Long from {EntryPrice}, Tgt: {TgtPrice}, SL {SLPrice}");
					tm.AddTrade('L', Qty, EntryPrice, Times[0][0], SLPrice, TgtPrice);
				}
				if(pATMStrategyName.Trim().Length==0 || Qty==0) return;
				if(State != State.Realtime) return;
				if(SelectedAccountName.Trim().Length==0) return;
				if(!LongsPermittedNow) return;
				if(!IsAlwaysActive && IsTradingPaused) return;
				IsTradingPaused = true;
				EntryABar = CurrentBar;
				var ATMorder = myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.Buy, 
								ordertype, 
								OrderEntry.Automated, 
								TimeInForce.Day, Qty, 0, 0, string.Empty, "Entry", Core.Globals.MaxDate, null); 
				NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName.Trim(), ATMorder);
			}
}catch(Exception eorder){
	Log(Log_Order_Reject_Message, LogLevel.Warning);
	MessageTime = DateTime.Now;
	Draw.TextFixed(this,"OrderError", Order_Reject_Message, TextPosition.Center,Brushes.Magenta,new SimpleFont("Arial",14),Brushes.DimGray,Brushes.Black,100);
	ForceRefresh();
}
        }
		#endregion

		#region - SellUsingATM -
        void SellUsingATM(string pATMStrategyName, int Qty, string SignalID)
        {
			if(pPermittedDirection == Raptor_PermittedDirections.Trend && BandEdgeTrendStr != "DOWN") return;//sell signal is not permitted against up trend
			if(tm.DailyTgtAchieved) {
				Print(Times[0][0].ToShortDateString()+"  Daily PnL target achieved, no sell order submitted");
				return;
			}

			var ordertype   = OrderType.Market;
try{
			if((pPermittedDirection == Raptor_PermittedDirections.Short || pPermittedDirection == Raptor_PermittedDirections.Both || pPermittedDirection == Raptor_PermittedDirections.Trend)){
				var position_size = GetCurrentMarketPosition();
				if(position_size > 0){//we're long, reverse to short requested...exit out the longs
					myAccount.Flatten(Instruments);
					tm.Note = "Reversing";
					tm.GoFlat(Times[0][0], Closes[0][0]);
				}
				var ShortsPermittedNow = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);
				if(State==State.Historical && ShortsPermittedNow && tm.SLTPs.ContainsKey(SignalID)){
//Print($"Selling: #{SignalID}   current position size: {position_size}");
					double EntryPrice = Closes[0][0] - TickSize;
					double TgtPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice - (tm.SLTPs[SignalID].ATRmultTarget > 0 ? tm.SLTPs[SignalID].ATRmultTarget * atr("points", 14) : tm.SLTPs[SignalID].DollarsTarget / PV));
					double SLPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice + (tm.SLTPs[SignalID].ATRmultSL > 0 ? tm.SLTPs[SignalID].ATRmultSL * atr("points", 14) : tm.SLTPs[SignalID].DollarsSL / PV));
					Draw.Dot(this,$"tgt{CurrentBars[0]}", false,0, TgtPrice, Brushes.LimeGreen);
					Draw.Dot(this,$"sl{CurrentBars[0]}", false, 0, SLPrice, Brushes.Magenta);
					Print($"------ {Times[0][0].ToString()} Now Short from {EntryPrice}, Tgt: {TgtPrice}, SL {SLPrice}");
					tm.AddTrade('S', Qty, EntryPrice, Times[0][0], SLPrice, TgtPrice);
				}
				if(pATMStrategyName.Trim().Length==0 || Qty==0) return;
				if(State != State.Realtime) return;
				if(SelectedAccountName.Trim().Length==0) return;
				if(!ShortsPermittedNow) return;
				if(!IsAlwaysActive && IsTradingPaused) return;
				IsTradingPaused = true;
				EntryABar = CurrentBar;
				var ATMorder = myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.SellShort, 
								ordertype, 
								OrderEntry.Automated, 
								TimeInForce.Day, Qty, 0, 0, string.Empty, "Entry", Core.Globals.MaxDate, null); 
				NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName.Trim(), ATMorder);
			}
}catch(Exception eorder){
	Log(Log_Order_Reject_Message, LogLevel.Warning);
	MessageTime = DateTime.Now;
	Draw.TextFixed(this,"OrderError",Order_Reject_Message, TextPosition.Center,Brushes.Magenta,new SimpleFont("Arial",14),Brushes.DimGray,Brushes.Black,100);
	ForceRefresh();
}
	}
		#endregion
		private double atr(string outputtype, int period){
			period = Math.Max(1,period);
			double ranges = 0;
			int i = 0;
			for(i = 0; i<Math.Min(CurrentBar, period); i++){
				ranges += Range()[i];
			}
			if(outputtype == "points") return ranges/period;
			else return ranges/period/TickSize;
		}
		private void MyKeyDownEvent(object sender, KeyEventArgs e)
		{
			e.Handled      = true;
			if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.B)){
				this.BuyUsingATM(this.pATMStrategyName1, 1, "");
			}
			if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Space)){
				this.SellUsingATM(this.pATMStrategyName1, 1, "");
			}
		
		}
//=================================================================================================================================================
		bool LongsPermittedNow = false;
		bool ShortsPermittedNow = false;
		protected override void OnBarUpdate()
		{
int line = 1495;
try{
			if(BarsArray==null) return;
			if(CurrentBars[0] <5) return;
			if(BarsArray.Length>0 && CurrentBars[1]<5) return;
			if(BarsInProgress == 0 && IsFirstTickOfBar) position[0] = position[1];
			if(BarsInProgress == 1) {
				#region PB coloring - Hawk
line=1503;
				if(pBarBrushMAtype == iwRaptor_BarBrushingMAtype.EMA){
					if(Lows[1][0] > hawk_EMA[0]) {
						HawkDirection = LONG;
						barcolor = pPLUp;
					}
					else if(Highs[1][0] < hawk_EMA[0]) {
						HawkDirection = SHORT;
						barcolor = pPLDn;
					}
					else {
						HawkDirection = FLAT;
						barcolor = pPLNeutral;
					}
				}
line=1518;
				if(pBarBrushMAtype == iwRaptor_BarBrushingMAtype.SMA){
//					double sum = 0;
//					for(int i = 0; i<pBarBrushMALength; i++) sum = sum + Closes[1][i];
//					sum = sum / pBarBrushMALength;
					if(Lows[1][0] > hawk_SMA[0]) {
						HawkDirection = LONG;
						barcolor = pPLUp;
					}
					else if(Highs[1][0] < hawk_SMA[0]) {
						HawkDirection = SHORT;
						barcolor = pPLDn;
					}
					else {
						HawkDirection = FLAT;
						barcolor = pPLNeutral;
					}
				}
line=1536;
				if(pBarBrushMAtype == iwRaptor_BarBrushingMAtype.RMA){
					if(Lows[1][0] > hawk_RMA[0]) {
						HawkDirection = LONG;
						barcolor = pPLUp;
					}
					else if(Highs[1][0] < hawk_RMA[0]) {
						HawkDirection = SHORT;
						barcolor = pPLDn;
					}
					else {
						HawkDirection = FLAT;
						barcolor = pPLNeutral;
					}
				}
				#endregion
				return;
			}
			if(BarsInProgress==0 && IsFirstTickOfBar && ChartControl!=null) BackBrush = null;

			var IsHistorical = State == State.Historical;
			var t1   = ToTime(Times[0][1])/100;
			var t    = ToTime(Times[0][0])/100;
			var dow1 = Times[0][1].DayOfWeek;
			var dow  = Times[0][0].DayOfWeek;
			var date = Times[0][0].Date;
			tm.PrintResults(BarsArray[0].Count, CurrentBars[0], pShowPnLLines, this);
			bool TradeToday = tm.DOW.Contains(dow);
			LongsPermittedNow  = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
			ShortsPermittedNow = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);

			if(tm.CurrentDay != Times[0][0].Day)
				tm.DailyTgtAchieved = false;
			if(!tm.DailyTgtAchieved && pDailyTgtDollars>0 && tm.IsInSession && tm.GetPnLPts(Times[0][0].Date) * PV > pDailyTgtDollars){
				tm.DailyTgtAchieved = true;
			}

			if(tm.DailyTgtAchieved && date != tm.DateTgtAchieved){
				Print(Times[0][0].ToString()+"   *********** Trading finished - Daily PnL exceeded today: "+(tm.GetPnLPts(Times[0][0].Date) * PV).ToString("C0"));
				tm.DateTgtAchieved = date;
				tm.GoFlat(Times[0][0], Closes[0][0]);
			}
			if(date != tm.DateTgtAchieved){
				bool HitStartTime = TradeToday && t1 < pStartTime && t >= pStartTime;
			}
			if(tm.ExitforEOD(Times[0][0], Times[0][1], Closes[0][1])){
				tm.GoFlat(Times[0][0], Closes[0][0], CurrentBars[0], Times[0][0].ToString());
			}
			if(tm.CurrentPosition != FLAT){
				var status = tm.ExitforSLTP(Times[0][0], Highs[0][0], Lows[0][0], false);
				if(tm.CurrentPosition != FLAT) tm.UpdateMinMaxPrices(Highs[0][0], Lows[0][0], Closes[0][0]);
				if(status.Length>0 && tm.CurrentPosition==FLAT){
					if(isBen) Print("TM Status: "+status);
				}
			}

//bool ta=Times[0][0].Month==12 && Times[0][0].Day==27 && (Times[0][0].Hour==10 || Times[0][0].Hour>=33);
			#region RaptorClouds state update
			if(BarsInProgress == 2){
				RaptorClouds[1].Fast = ema2_13[0];
				RaptorClouds[1].Med  = ema2_21[0];
				RaptorClouds[1].Slow = ema2_55[0];
				return;
			}
			if(BarsInProgress == 3){
				RaptorClouds[2].Fast = ema3_13[0];
				RaptorClouds[2].Med  = ema3_21[0];
				RaptorClouds[2].Slow = ema3_55[0];
				return;
			}
//From this point on, we are in BarsInProgress == 0

			cbar = CurrentBars[0];
			atrSeparation = SUM(Range(),14)[0]/14;//atr14.GetValueAt(cbar);
//Print("atrSeparation:  "+atrSeparation);
			int bo = 0;
			SoftBandEdgeHigh1[0] = Math.Max(RaptorClouds[1].Fast,Math.Max(RaptorClouds[1].Med,RaptorClouds[1].Slow));
			SoftBandEdgeLow1[0]  = Math.Min(RaptorClouds[1].Fast,Math.Min(RaptorClouds[1].Med,RaptorClouds[1].Slow));
			BandEdge1[0]         = RaptorClouds[1].Slow;
			RaptorClouds[1].SEH  = SoftBandEdgeHigh1[bo];
			RaptorClouds[1].SEL  = SoftBandEdgeLow1[bo];
			SoftBandEdgeHigh2[0] = Math.Max(RaptorClouds[2].Fast,Math.Max(RaptorClouds[2].Med,RaptorClouds[2].Slow));
			SoftBandEdgeLow2[0]  = Math.Min(RaptorClouds[2].Fast,Math.Min(RaptorClouds[2].Med,RaptorClouds[2].Slow));
			BandEdge2[0]         = RaptorClouds[2].Slow;
			RaptorClouds[2].SEH  = SoftBandEdgeHigh2[bo];
			RaptorClouds[2].SEL  = SoftBandEdgeLow2[bo];

			var c1 = Math.Max(SoftBandEdgeHigh1[0],SoftBandEdgeLow1[0]) > BandEdge1[0];
			var c2 = Math.Max(SoftBandEdgeHigh2[0],SoftBandEdgeLow2[0]) > BandEdge2[0];
			BandEdgeTrendStr = "";
			if(c1 && c2) 
				BandEdgeTrendStr = "UP";
			else {
				c1 = Math.Min(SoftBandEdgeHigh1[0],SoftBandEdgeLow1[0]) < BandEdge1[0];
				c2 = Math.Min(SoftBandEdgeHigh2[0],SoftBandEdgeLow2[0]) < BandEdge2[0];
				if(c1 && c2) BandEdgeTrendStr = "DOWN";
			}
			list.Clear();
			list.Add(SoftBandEdgeHigh1[0]);
			list.Add(SoftBandEdgeLow1[0]);
			list.Add(BandEdge1[0]);
			list.Add(SoftBandEdgeHigh2[0]);
			list.Add(SoftBandEdgeLow2[0]);
			list.Add(BandEdge2[0]);
			BandCounter[0] = list.Max() - list.Min();
//try{
//Print("BandCounter: "+list.Max()+"   "+list.Min());
//Draw.Dot(this,"max",false,0,list.Max(),Brushes.Red);
//Draw.Dot(this,"min",false,0,list.Min(),Brushes.Red);
//}catch(Exception eeee){Print(eeee.ToString());}
			if(pBand1_Opacity>0 && pBand1_Brush!=Brushes.Transparent && pBand1_Brush!=null) {
				Draw.Region(this, "cloud1",cbar-1,0,/*Times[0][cbar],Times[0][0],*/SoftBandEdgeHigh1,SoftBandEdgeLow1,Brushes.Transparent,pBand1_Brush,pBand1_Opacity);
			}
			if(pBand2_Opacity>0 && pBand2_Brush!=Brushes.Transparent && pBand2_Brush!=null) {
				Draw.Region(this, "cloud2",cbar-1,0,/*Times[0][cbar],Times[0][0],*/SoftBandEdgeHigh2,SoftBandEdgeLow2,Brushes.Transparent,pBand2_Brush,pBand2_Opacity);
			}

//if(ta){
//	System.IO.File.AppendAllText(logfilepath,string.Concat(fileline,"\t",BarsInProgress,"\t",Time[0].ToShortTimeString(),Environment.NewLine));
//	fileline++;
//}
			if(IsFirstTickOfBar){
				RaptorClouds[1].UpdateTrendDirection(cbar, BandEdge1[bo], SoftBandEdgeHigh1[bo], SoftBandEdgeLow1[bo], Highs[0][bo], Lows[0][bo]);
				RaptorClouds[1].UpdateTrendStrength(cbar, Highs[0][bo], Lows[0][bo]);
				RaptorClouds[1].UpdateHardEdgeTouch(cbar, Highs[0][bo+1], Highs[0][bo], Lows[0][bo+1], Lows[0][bo], BandEdge1[bo+1], BandEdge1[bo]);
				RaptorClouds[2].UpdateTrendDirection(cbar, BandEdge2[bo], SoftBandEdgeHigh2[bo], SoftBandEdgeLow2[bo], Highs[0][bo], Lows[0][bo]);
				RaptorClouds[2].UpdateTrendStrength(cbar, Highs[0][bo], Lows[0][bo]);
				RaptorClouds[2].UpdateHardEdgeTouch(cbar, Highs[0][bo+1], Highs[0][bo], Lows[0][bo+1], Lows[0][bo], BandEdge2[bo+1], BandEdge2[bo]);
				RaptorClouds[-1].UpdateTrendDirection(cbar, BandEdge1[bo], SoftBandEdgeHigh1[bo], SoftBandEdgeLow1[bo], Highs[0][bo], Lows[0][bo]);
				RaptorClouds[-1].UpdateTrendStrength(cbar, Highs[0][bo], Lows[0][bo]);
				RaptorClouds[-1].UpdateHardEdgeTouch(cbar, Highs[0][bo+1], Highs[0][bo], Lows[0][bo+1], Lows[0][bo], BandEdge1[bo+1], BandEdge1[bo]);
				RaptorClouds[-2].UpdateTrendDirection(cbar, BandEdge2[bo], SoftBandEdgeHigh2[bo], SoftBandEdgeLow2[bo], Highs[0][bo], Lows[0][bo]);
				RaptorClouds[-2].UpdateTrendStrength(cbar, Highs[0][bo], Lows[0][bo]);
				RaptorClouds[-2].UpdateHardEdgeTouch(cbar, Highs[0][bo+1], Highs[0][bo], Lows[0][bo+1], Lows[0][bo], BandEdge2[bo+1], BandEdge2[bo]);
			}
			#endregion
line=1673;
			#region v1 Signal logic
			if (cbar < 50) {
				SetupDirection[0] = (FLAT);
				return;
			}
			if(ChartControl!=null){
				#region Color bars
				bool pHollowBars = true;
//				if(Bars.BarsType.DefaultChartStyle == ChartStyleType.HiLoBars) pHollowBars = false;
//				else if(BarsArray[0].BarsType.DefaultChartStyle == ChartStyleType.OHLC) pHollowBars = false;
				bool IsUpBar = Closes[0][0]>Opens[0][0];
//if(cbar>BarsArray[0].Count-10){
//	Print(IsUpBar.ToString()+"   Close: "+Closes[0][0].ToString("0.00")+"  Open: "+Opens[0][0].ToString("0.00"));
//	if(IsUpBar)Draw.Diamond(this, "D"+cbar.ToString(),false,0,Highs[0][0]+5,Color.Red);
//	else Draw.Diamond(this, "D"+cbar.ToString(),false,0,Lows[0][0]-5,Color.Red);
//}
				string x = BarsArray[0].BarsType.DefaultChartStyle.ToString().ToUpper();
				if(x.Contains("CUSTOM") || x.Contains("MEANRENKO") || BarsArray[0].BarsType.DefaultChartStyle == ChartStyleType.CandleStick) {
					if (IsUpBar) {
						CandleOutlineBrush  = barcolor; 
						BarBrush = pHollowBars?Brushes.Transparent:barcolor;
					}
					else { //down color
						CandleOutlineBrush = barcolor;
						BarBrush = barcolor;
					}
				} else {
					BarBrush  = barcolor; 
					CandleOutlineBrush = barcolor;
				}
				#endregion
			}
line=1706;

			int atrFilterDirection = FLAT;
			if(bEnableAtrFilter) {
				#region ATRFilter on chart bars
				if(atrFilter == null) {
					atrFilter = new iwTmAtrTrail(this.iAtrLength, this.iAtrMult);
				}
				atrFilter.OnBarUpdate(cbar, SUM(Range(),14)[0]/14, Close[1], Close[0], Open[0], IsFirstTickOfBar);
				if(Close[1]>atrFilter.trailprior) atrFilterDirection = LONG;
				else atrFilterDirection = SHORT;
//				if(atrFilter.Status.Length>0) 
//					Print(Time[0].ToString()+"   "+atrFilter.Status+"   trail: "+atrFilter.trail.ToString("0.00"));
				FilterLevel[0] = (atrFilter.trail);
//				Draw.Dot(this, cbar.ToString()+"h",false,0,atrFilter.hHigh,Color.Red);
//				Draw.Dot(this, cbar.ToString(),false,0,atrFilter.hHigh-(iAtrMult*SUM(Range(),14)[0]/14),Color.Blue);
				#endregion
			}

#if ROBERT_WILLIAMS_CUSTOM
			#region SessionTime
			bool InSession = false;
			if(pBeginTime1==pEndTime1) 
				InSession=true;
			else if(pUse2ndSession && pBeginTime2==pEndTime2) 
				InSession=true;
			else {
				DateTime Midnight = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day,0,0,0);
				DateTime tStart = Midnight.AddTicks(pBeginTime1.Ticks);
				DateTime tEnd = Midnight.AddTicks(pEndTime1.Ticks);
				if(tStart>tEnd){
					if(Time[0]<=tEnd) InSession = true;
					else if(Time[0]>=tStart) InSession = true;
				} else {
					if(Time[0]>=tStart && Time[0]<=tEnd) InSession = true;
				}
				if(pUse2ndSession){
					tStart = Midnight.AddTicks(pBeginTime2.Ticks);
					tEnd = Midnight.AddTicks(pEndTime2.Ticks);
					if(tStart>tEnd){
						if(Time[0]<=tEnd) InSession = true;
						else if(Time[0]>=tStart) InSession = true;
					} else {
						if(Time[0]>=tStart && Time[0]<=tEnd) InSession = true;
					}
				}
			}
			if(!InSession && ChartControl!=null){
				if(pOutOfSessionOpacity>0 && pOutOfSessionBackgroundColor!=Brushes.Transparent)
					BackBrush = Color.FromArgb((pOutOfSessionOpacity/10.0), pOutOfSessionBackgroundColor.R,pOutOfSessionBackgroundColor.G,pOutOfSessionBackgroundColor.B);
				else
					BackBrush = Color.Empty;
			}
			#endregion
#else
			bool InSession = true;
#endif
			if(HawkDirection==FLAT && pEnableYellowBarFilter){
				SetupDirection[0]=FLAT;
				if(pSwitchToV1_Signals) return;
			}

			if(InSession) SetupDirection[0] = FLAT;
			else SetupDirection[0] = (SetupDirection[1]);

			if(SoundEntryBar!=cbar && IsFirstTickOfBar) {
				BuyEntryPoint.Reset();
				SellEntryPoint.Reset();
//Print(NinjaTrader.Core.Globals.Now.ToShortTimeString()+"   1369 Resetting both Buy and Sell EntryPoint");

			}
line=1777;
			if(IsFirstTickOfBar) ArrowTag = string.Empty;//string.Concat("iwRaptor",cbar.ToString());

			BuyWarningDot.Reset();
			int offset = 0;
			c1 = psar[offset]    < Median[offset];
			c2 = ema[offset]     < Close[offset];
			bool c3 = stoch.K[offset] > stoch.D[offset];
			bool c4 = !this.bEnableAtrFilter || atrFilterDirection == LONG;
			bool c5 = !this.pEnableCounterTrendFilter || HawkDirection==LONG;
			if(InSession){
				if(c5 && (c1 && c2 || c1 && c3 || c2 && c3) && pos!=LONG && c4) {
					if(!BuyAlert.IsValidDataPoint(0)) {
						BuyWarningDot[0] = (Lows[0][0]-TickSize);
						BuyWarningDot_ABar = cbar;
					}
					SetupDirection[0] = (LONG);
				}
			}

line=1797;
			offset = 1;
			c1 = psar[offset]    < Median[offset];
			c2 = ema[offset]     < Close[offset];
			c3 = stoch.K[offset] > stoch.D[offset];
			if((c1 && c2 || c1 && c3 || c2 && c3) && pos==SHORT) pos = FLAT;
			if(InSession){
				if(c5 && pos!=LONG) {
					if(c1 && c2 && c3 && c4) {
						BuyAlert[0] = (Lows[0][0]-TickSize);
						BuyAlert_ABar = cbar;
						if(BuyPrice==double.MinValue) BuyPrice = Highs[0][offset]+TickSize;
						SellPrice = double.MinValue;
						BuyEntryPoint[0] = (BuyPrice);
					} else {
						SetupDirection[0] = (FLAT);
						BuyPrice = double.MinValue;
					}
				}
			}
			if(!c2 && SetupDirection[0]==LONG) {
				SetupDirection[0] = (FLAT);
//Print(NinjaTrader.Core.Globals.Now.ToShortTimeString()+"   1402 BuyPrice is reset");
				BuyPrice = double.MinValue;
			}

line=1823;
			SellWarningDot.Reset();
			offset = 0;
			c1 = psar[offset]    > Median[offset];
			c2 = ema[offset]     > Close[offset];
			c3 = stoch.K[offset] < stoch.D[offset];
			c4 = !this.bEnableAtrFilter || atrFilterDirection == SHORT;
			c5 = !this.pEnableCounterTrendFilter || HawkDirection==SHORT;
			if(InSession){
				if(c5 && (c1 && c2 || c1 && c3 || c2 && c3) && pos!=SHORT && c4) {
					if(!SellAlert.IsValidDataPoint(0)) {
						SellWarningDot[0] = (Lows[0][0]-TickSize);
						SellWarningDot_ABar = cbar;
					}
					SetupDirection[0] = (SHORT);
				}
			}

line=1841;
			offset = 1;
			c1 = psar[offset]    > Median[offset];
			c2 = ema[offset]     > Close[offset];
			c3 = stoch.K[offset] < stoch.D[offset];
			if((c1 && c2 || c1 && c3 || c2 && c3) && pos==LONG) pos = FLAT;

			if(InSession){
				if(c5  && pos!=SHORT) {
					if(c1 && c2 && c3 && c4) {
						SellAlert[0] = (Highs[0][0]+TickSize);
						SellAlert_ABar = cbar;
						if(SellPrice==double.MinValue) SellPrice = Lows[0][offset]-TickSize;
						BuyPrice = double.MinValue;
						SellEntryPoint[0] = (SellPrice);
					} else {
						SetupDirection[0] = (FLAT);
	//Print(NinjaTrader.Core.Globals.Now.ToShortTimeString()+"   1430 SellPrice is reset");
						SellPrice = double.MinValue;
					}
				}
			}
			if(!c2 && SetupDirection[0]==SHORT) {
				SetupDirection[0] = (FLAT);
				SellPrice = double.MinValue;
			}
line=1867;
			if(!SellAlert.IsValidDataPoint(0)) SellPrice = double.MinValue;
			if(!BuyAlert.IsValidDataPoint(0))  BuyPrice = double.MinValue;

			if(BuyPrice !=double.MinValue && SetupDirection[0]==LONG)  BuyEntryPoint[0] = (BuyPrice);
			if(SellPrice!=double.MinValue && SetupDirection[0]==SHORT) SellEntryPoint[0] = (SellPrice);

			bool drawarrow = false;
			#region Draw entry arrow
			c1 = pos!=LONG && BuyPrice !=double.MinValue;// /*SetupDirection[0] == LONG &&*/ BuyEntryPoint.IsValidDataPoint(1);
			if(c1) {
line=1878;
				if(Highs[0][0] >= BuyPrice) {
					pos = LONG;
					MostRecentPosition = LONG;
					drawarrow = true;
					EntryPrice = BuyPrice;//c1? Math.Max(BuyEntryPoint[1],Open[0]) : BuyEntryPoint[0];
					BuyWarningDot.Reset();
					SellWarningDot.Reset();
					BuyEntryPoint[0] = (BuyPrice);
					v2SignalData.LastArrowABar = cbar;
					if(this.pSwitchToV1_Signals){
						ArrowBrush = Brush_UpArrow_ID;
						ArrowTag = string.Concat("iwRaptor_buy",cbar.ToString());
					}
					position[0] = (LONG);
					BuyAlert[0] = (Lows[0][0]-TickSize*pArrowSeparation);
				}
			} else {
				c1 = pos!=SHORT && SellPrice !=double.MinValue;// /*SetupDirection[0] == SHORT &&*/ SellEntryPoint.IsValidDataPoint(1);
				if(c1) {
line=1898;
					if(Lows[0][0] <= SellPrice) {
						pos = SHORT;
						MostRecentPosition = SHORT;
						drawarrow = true;
						EntryPrice = SellPrice;//c1?Math.Min(SellEntryPoint[1],Open[0]) : SellEntryPoint[0];
						BuyWarningDot.Reset();
						SellWarningDot.Reset();
						SellEntryPoint[0] = (SellPrice);
						v2SignalData.LastArrowABar = cbar;
						if(this.pSwitchToV1_Signals){
							ArrowBrush = Brush_DownArrow_ID;
							ArrowTag = string.Concat("iwRaptor_sell",cbar.ToString());
						}
						position[0] = (SHORT);
						SellAlert[0] = (Highs[0][0]+TickSize*pArrowSeparation);
					}
				}
			}
			#endregion
			if(SoundEntryBar==cbar-1) {
				SellPrice = double.MinValue;
				BuyPrice = double.MinValue;
			}
line=1922;
//			if(!BuyAlert.IsValidDataPoint(0) && !SellAlert.IsValidDataPoint(0)) RemoveDrawObject(ArrowTag);
			if(BuyAlert_ABar == cbar)       BuyAlert[0]=(Lows[0][0]-TickSize);
			if(BuyWarningDot_ABar == cbar)  BuyWarningDot[0]=(Lows[0][0]-TickSize);
			if(SellAlert_ABar == cbar)      SellAlert[0]=(Highs[0][0]+TickSize);
			if(SellWarningDot_ABar == cbar) SellWarningDot[0]=(Highs[0][0]+TickSize);

			if(BuyEntryPoint.IsValidDataPoint(0) && !BuyEntryPoint.IsValidDataPoint(1)) {
//Print(NinjaTrader.Core.Globals.Now.ToShortTimeString()+"   1497 Setting BuyEntryPoint to prior bar value");
				BuyEntryPoint[1]=(BuyEntryPoint[0]);
//				if(pReverseArrows) {
//if(pArrowLocation==iwRaptor_ArrowLocation.OffBar)Draw.ArrowDown(this, ArrowTag,AutoScale,1,Highs[0][1]+pArrowSeparation*TickSize,pBrush_DownArrow);
//if(pArrowLocation==iwRaptor_ArrowLocation.AtEntryPrice)Draw.ArrowDown(this, ArrowTag,AutoScale,1,BuyEntryPoint[0],pBrush_DownArrow);
//				}
				//BuyAlert[0] = (1,BuyWarningDot[1]);
				BuyWarningDot.Reset(1);
			}
line=1939;
			if(SellEntryPoint.IsValidDataPoint(0) && !SellEntryPoint.IsValidDataPoint(1)) {
//Print(NinjaTrader.Core.Globals.Now.ToShortTimeString()+"   1507 Setting SellEntryPoint to prior bar value");
				SellEntryPoint[1]=(SellEntryPoint[0]);
//				if(pReverseArrows) {
//if(pArrowLocation==iwRaptor_ArrowLocation.OffBar)Draw.ArrowUp(this, ArrowTag,AutoScale,1,Lows[0][1]-pArrowSeparation*TickSize,pBrush_UpArrow);
//if(pArrowLocation==iwRaptor_ArrowLocation.AtEntryPrice)Draw.ArrowUp(this, ArrowTag,AutoScale,1,SellEntryPoint[0],pBrush_UpArrow);
//				}
				//SellAlert[0] = (1,SellWarningDot[1]);
				SellWarningDot.Reset(1);
			}
			if(BuyEntryPoint.IsValidDataPoint(1))  BuyAlert[1]=(Lows[0][1]-TickSize);
			if(SellEntryPoint.IsValidDataPoint(1)) SellAlert[1]=(Highs[0][1]+TickSize);
			if(BuyEntryPoint.IsValidDataPoint(0))  BuyAlert[0]=(Lows[0][0]-TickSize);
			if(SellEntryPoint.IsValidDataPoint(0)) SellAlert[0]=(Highs[0][0]+TickSize);
//-------------------------

			int rbar = 0;
			if(IsFirstTickOfBar){
				for(rbar = 0; rbar<cbar; rbar++){
					if(SellEntryPoint.IsValidDataPoint(rbar) && Lows[0][rbar]<=SellEntryPoint[rbar]){
						PositionAge[0] = (-(rbar+1));
						for(int x = 1;x<rbar;x++) if(!PositionAge.IsValidDataPoint(x)) PositionAge[x] = (PositionAge[x-1]+1);
						break;
					}
					if(BuyEntryPoint.IsValidDataPoint(rbar) && Highs[0][rbar]>=BuyEntryPoint[rbar]){
						PositionAge[0] = (rbar+1);
						for(int x = 1;x<rbar;x++) if(!PositionAge.IsValidDataPoint(x)) PositionAge[x] = (PositionAge[x-1]-1);
						break;
					}
				}
			}
			#endregion
line=1972;
			//start v2.0 signals
			int ArrowABar = 0;
			if(Math.Abs(v2SignalData.LastArrowABar) == cbar) ArrowABar = cbar;
			else if(IsFirstTickOfBar && Math.Abs(v2SignalData.LastArrowABar) == cbar-1) ArrowABar = cbar-1;
			if(IsFirstTickOfBar) position_v2[0] = 0;
			if(!(IsHistorical) && (BuyEntryPoint.IsValidDataPoint(0) || SellEntryPoint.IsValidDataPoint(0)) && !pSwitchToV1_Signals && ChartControl!=null){
				int PresignalDirection = FLAT;
				if(BuyWarningDot_ABar==cbar || BuyAlert_ABar==cbar || BuyPrice!=double.MinValue) PresignalDirection= LONG;
				if(SellWarningDot_ABar==cbar || SellAlert_ABar==cbar || SellPrice!=double.MinValue)PresignalDirection= SHORT;
				#region Pre-Signal calculation
				if(RaptorClouds[-1].TrendDirection != RaptorClouds[-2].TrendDirection) {
					v2PresignalData.ABar_CloudCrossoverSignal = -1;
				}
				string PresignalTag = string.Empty;
				if(!pShowCloudCrossoverSignal){
					v2PresignalData.ABar_CloudCrossoverSignal = RaptorClouds[-1].ABar_Crossover;//don't let this First crossover signal get in the way of all other signals
					RaptorClouds[-1].SearchForHardEdgeTouchSignal = true;
					RaptorClouds[-2].SearchForHardEdgeTouchSignal = true;
				}else{
					#region CloudCrossoverSignal
					if(v2PresignalData.ABar_CloudCrossoverSignal < Math.Min(RaptorClouds[-1].ABar_Crossover,RaptorClouds[-2].ABar_Crossover)){
						if(RaptorClouds[-1].TrendDirection == LONG
							&& RaptorClouds[-2].TrendDirection == LONG
							&& PresignalDirection >= 0){
								PresignalTag = "Crossover_Buy";
								ArrowBrush = CloudCrossover_BuyBrush_ID;
								v2PresignalData.ABar_CloudCrossoverSignal = cbar;
								if(this.pEnableSoundAlerts && SoundPresignalBar!=cbar){
//									BuySoundFile = this.pCloudCrossLongSound;
									SoundPresignalBar = cbar;
									Alert("presignalCloudCross",Priority.High,"Raptor Crossover BUY",AddSoundFolder(pCloudCrossLongSetupSound),1,Brushes.White,Brushes.Blue); 
								}
								if(PresignalBuyBkgBrush != Brushes.Transparent) BackBrush = PresignalBuyBkgBrush;
								PreSignalMsg = "Crossover BUY forming";
						} else if(RaptorClouds[-1].TrendDirection == SHORT
							&& RaptorClouds[-2].TrendDirection == SHORT
							&& PresignalDirection <= 0){
								PresignalTag = "Crossover_Sell";
								ArrowBrush = CloudCrossover_SellBrush_ID;
								v2PresignalData.ABar_CloudCrossoverSignal = cbar;
								if(this.pEnableSoundAlerts && SoundPresignalBar!=cbar){
//									SellSoundFile = pCloudCrossShortSound;
									SoundPresignalBar = cbar;
									Alert("presignalCloudCross",Priority.High,"Raptor Crossover SELL",AddSoundFolder(pCloudCrossShortSetupSound),1,Brushes.White,Brushes.Red);
								}
								if(PresignalSellBkgBrush != Brushes.Transparent) BackBrush = PresignalSellBkgBrush;
								PreSignalMsg = "Crossover SELL forming";
						}
					}
					#endregion
				}
line=2024;
				if(v2PresignalData.ABar_CloudCrossoverSignal>0 && v2PresignalData.ABar_CloudCrossoverSignal!=cbar){
					if(pShowSoftEdgeCounterTrendSignal){
						#region SoftEdgeCounterTrendSignal
						if(RaptorClouds[-1].SearchForRetestExtreme && RaptorClouds[-2].SearchForRetestExtreme){
							int x1 = -1, x2 = -1;
							if(RaptorClouds[-1].TrendDirection==LONG && PresignalDirection==SHORT){
line=2031;
			//if the extreme price occurs on the 2nd instance of the Swing, then this algorithm works.  If the extreme price occurs sometime prior to the 2nd swing, then this algorithm may not truly be testing the extreme price
								x2 = swing2.SwingHighBar(2,2,cbar-RaptorClouds[-1].ABar_Crossover)+(IsHistorical? 0:1);
								if(!PresignalSwingABarsHash.Contains(cbar-x2)){
									x1 = swing2.SwingHighBar(2,1,cbar-RaptorClouds[-1].ABar_Crossover)+(IsHistorical? 0:1);
									if(Math.Abs(x1-x2)>=pMinRetestBarSeparation && x1>0 && x2>0 && Math.Abs(Highs[0][x2]-Highs[0][x1])<=atrSeparation*pExtremeRetestSensitivity){
										PresignalSwingABarsHash.Add(cbar-x2);
										v2PresignalData.ABar_SoftEdgeCounterTrend = cbar;
										PresignalTag = "SoftEdge_Sell";
										ArrowBrush = SoftEdge_SellBrush_ID;
										PreSignalMsg = "SoftEdge SELL forming";//this.SignalLabels[cbar] = new v2SignalInfo('2',Highs[0][rbar]+atrseparation*pLabelSeparation,SHORT,ArrowBrush);
										if(this.pEnableSoundAlerts && SoundPresignalBar!=cbar){
//											SellSoundFile = this.pSoftEdgeShortSound;
											SoundPresignalBar = cbar;
											Alert("presignalSoftEdge",Priority.High,"Raptor SoftEdge SELL",AddSoundFolder(pSoftEdgeShortSetupSound),1,Brushes.White,Brushes.Red);
										}
									}
								}
							}else if(RaptorClouds[-1].TrendDirection==SHORT && PresignalDirection==LONG){
line=2050;
			//if the extreme price occurs on the 2nd instance of the Swing, then this algorithm works.  If the extreme price occurs sometime prior to the 2nd swing, then this algorithm may not truly be testing the extreme price
								x2 = swing2.SwingLowBar(2,2,cbar-RaptorClouds[-1].ABar_Crossover)+(IsHistorical? 0:1);
								if(!PresignalSwingABarsHash.Contains(cbar-x2)){
									x1 = swing2.SwingLowBar(2,1,cbar-RaptorClouds[-1].ABar_Crossover)+(IsHistorical? 0:1);
									if(Math.Abs(x1-x2)>=pMinRetestBarSeparation && x1>0 && x2>0 && Math.Abs(Lows[0][x2]-Lows[0][x1])<=atrSeparation*pExtremeRetestSensitivity){
										PresignalSwingABarsHash.Add(cbar-x2);
										v2PresignalData.ABar_SoftEdgeCounterTrend = cbar;
										PresignalTag = "SoftEdge_Buy";
										ArrowBrush = SoftEdge_BuyBrush_ID;
										PreSignalMsg = "SoftEdge BUY forming";//this.SignalLabels[cbar] = new v2SignalInfo('2',Lows[0][rbar]-atrseparation*pLabelSeparation,LONG,ArrowBrush);
										if(this.pEnableSoundAlerts && SoundPresignalBar!=cbar){
//											BuySoundFile = this.pSoftEdgeLongSound;
											SoundPresignalBar = cbar;
											Alert("presignalSoftEdge",Priority.High,"Raptor SoftEdge BUY",AddSoundFolder(pSoftEdgeLongSetupSound),1,Brushes.White,Brushes.Blue);
										}
									}
								}
							}
						}
						#endregion
					}
					if(pShowHardEdgeSignal){
line=2073;
						#region HardEdgeSignal
						int HardEdgeTrendDirection = FLAT;
						int Direction2 = FLAT;
						int Direction3 = FLAT;
						if(this.pHardEdgeTrendConfirmation){
							if(RaptorClouds[-1].TrendDirection==LONG && RaptorClouds[-2].TrendDirection==LONG) HardEdgeTrendDirection=LONG;
							if(RaptorClouds[-1].TrendDirection==SHORT && RaptorClouds[-2].TrendDirection==SHORT) HardEdgeTrendDirection=SHORT;
							Direction2 = RaptorClouds[-1].CheckForHardEdgeSignal(cbar, PresignalDirection, cbar, this.pShowCloudCrossoverSignal, v2PresignalData.ABar_CloudCrossoverSignal, LONG, SHORT, ref PresignalTag, ref ArrowBrush, HardEdge_BuyBrush_ID, HardEdge_SellBrush_ID, HardEdgeTrendDirection);
							Direction3 = RaptorClouds[-2].CheckForHardEdgeSignal(cbar, PresignalDirection, cbar, this.pShowCloudCrossoverSignal, v2PresignalData.ABar_CloudCrossoverSignal, LONG, SHORT, ref PresignalTag, ref ArrowBrush, HardEdge_BuyBrush_ID, HardEdge_SellBrush_ID, HardEdgeTrendDirection);
						}else{
							if(RaptorClouds[-2].TrendDirection==LONG) HardEdgeTrendDirection=LONG;
							if(RaptorClouds[-2].TrendDirection==SHORT) HardEdgeTrendDirection=SHORT;
							Direction2 = RaptorClouds[-2].CheckForHardEdgeSignal(cbar, PresignalDirection, cbar, this.pShowCloudCrossoverSignal, v2PresignalData.ABar_CloudCrossoverSignal, LONG, SHORT, ref PresignalTag, ref ArrowBrush, HardEdge_BuyBrush_ID, HardEdge_SellBrush_ID, HardEdgeTrendDirection);
						}
						if(pShowSignalLabels_v2){ 
							if(Direction2==LONG || Direction3==LONG) {
								PreSignalMsg = "HardEdge BUY forming";//this.SignalLabels.Add(cbar, new v2SignalInfo('3',Lows[0][rbar]-atrseparation*pLabelSeparation,LONG,ArrowBrush));
								if(this.pEnableSoundAlerts && SoundPresignalBar!=cbar){
//									BuySoundFile = this.pHardEdgeLongSound;
									SoundPresignalBar = cbar;
									Alert("presignalHardEdge",Priority.High,"Raptor HardEdge BUY",AddSoundFolder(pHardEdgeLongSetupSound),1,Brushes.White,Brushes.Blue);
								}
							}
							else if(Direction2==SHORT || Direction3==SHORT) {
								PreSignalMsg = "HardEdge SELL forming";//this.SignalLabels[cbar]= new v2SignalInfo('3',Highs[0][rbar]+atrseparation*pLabelSeparation,SHORT,ArrowBrush);
								if(this.pEnableSoundAlerts && SoundPresignalBar!=cbar){
//									SellSoundFile = this.pHardEdgeShortSound;
									SoundPresignalBar = cbar;
									Alert("presignalHardEdge",Priority.High,"Raptor HardEdge SELL",AddSoundFolder(pHardEdgeShortSetupSound),1,Brushes.White,Brushes.Blue);
								}
							}
						}
						#endregion
					}
				}

				if(PresignalTag.Length>0){
line=2111;
						System.Windows.Media.Brush tempbrush = null;
						switch (ArrowBrush) {
							case Brush_UpArrow_ID:   tempbrush = this.pBrush_UpArrow.Clone(); break;
							case Brush_DownArrow_ID: tempbrush = this.pBrush_DownArrow.Clone(); break;
							case CloudCrossover_BuyBrush_ID:  tempbrush = this.pCloudCrossover_BuyBrush.Clone(); break;
							case CloudCrossover_SellBrush_ID: tempbrush = this.pCloudCrossover_SellBrush.Clone(); break;
							case SoftEdge_BuyBrush_ID:  tempbrush = this.pSoftEdge_BuyBrush.Clone(); break;
							case SoftEdge_SellBrush_ID: tempbrush = this.pSoftEdge_SellBrush.Clone(); break;
							case HardEdge_BuyBrush_ID:  tempbrush = this.pHardEdge_BuyBrush.Clone(); break;
							case HardEdge_SellBrush_ID: tempbrush = this.pHardEdge_SellBrush.Clone(); break;
						}
						tempbrush.Freeze();
						if(PreSignalMsg!=string.Empty && tempbrush!=null && ChartControl != null){
							TriggerCustomEvent(o2 =>{
								if(pPresignalTextPosition == iwRaptor_TextPositionTypes.TopRight)    Draw.TextFixed(this, "PreSignalMsg",PreSignalMsg, TextPosition.TopRight,   tempbrush, PresignalFont,Brushes.White,Brushes.Black, 100);
								if(pPresignalTextPosition == iwRaptor_TextPositionTypes.TopLeft)     Draw.TextFixed(this, "PreSignalMsg",PreSignalMsg, TextPosition.TopLeft,    tempbrush, PresignalFont,Brushes.White,Brushes.Black, 100);
								if(pPresignalTextPosition == iwRaptor_TextPositionTypes.BottomRight) Draw.TextFixed(this, "PreSignalMsg",PreSignalMsg, TextPosition.BottomRight,tempbrush, PresignalFont,Brushes.White,Brushes.Black, 100);
								if(pPresignalTextPosition == iwRaptor_TextPositionTypes.BottomLeft)  Draw.TextFixed(this, "PreSignalMsg",PreSignalMsg, TextPosition.BottomLeft, tempbrush, PresignalFont,Brushes.White,Brushes.Black, 100);
								if(pPresignalTextPosition == iwRaptor_TextPositionTypes.Center)      Draw.TextFixed(this, "PreSignalMsg",PreSignalMsg, TextPosition.Center,     tempbrush, PresignalFont,Brushes.White,Brushes.Black, 100);
							},0,null);
						}

						if(pPresignalBackgroundOpacity>0) {
							if(tempbrush!=null){
								tempbrush = tempbrush.Clone();
								tempbrush.Opacity = pPresignalBackgroundOpacity/10.0;
								tempbrush.Freeze();
								BackBrushes[rbar] = tempbrush.Clone();
							}
						}
					#endregion
				}
			}
			if(ArrowABar > 0){
				rbar = cbar-ArrowABar;
				if(IsFirstTickOfBar) {
					PreSignalMsg = string.Empty;
					RemoveDrawObject("PreSignalMsg");
				}
				#region Signal calculation
				if(!pSwitchToV1_Signals){
					if(RaptorClouds[1].TrendDirection != RaptorClouds[2].TrendDirection) {
						v2SignalData.ABar_CloudCrossoverSignal = -1;
					}
					char BkgCharId = ' ';
					ArrowTag = string.Empty;
					if(!pShowCloudCrossoverSignal){
						v2SignalData.ABar_CloudCrossoverSignal = RaptorClouds[1].ABar_Crossover;//don't let this First crossover signal get in the way of all other signals
						RaptorClouds[1].SearchForHardEdgeTouchSignal = true;
						RaptorClouds[2].SearchForHardEdgeTouchSignal = true;
					}else{
						#region CloudCrossoverSignal
						if(v2SignalData.ABar_CloudCrossoverSignal < Math.Min(RaptorClouds[1].ABar_Crossover,RaptorClouds[2].ABar_Crossover)){
							if(RaptorClouds[1].TrendDirection == LONG
								&& RaptorClouds[2].TrendDirection == LONG
								&& MostRecentPosition >= 0){
									ArrowTag = "Crossover_Buy"+cbar.ToString();
									ArrowBrush = CloudCrossover_BuyBrush_ID;
									BkgCharId = GetContrastingBrush(this.pCloudCrossover_BuyBrush);
									v2SignalData.ABar_CloudCrossoverSignal = ArrowABar;
									v2SignalData.SignalInteger = 1;
									if(pShowSignalLabels_v2) {
//if(isBen) Print(ArrowTag+" #1  Price: "+(Lows[0].GetValueAt(ArrowABar)-atrSeparation*pLabelSeparation)+"   abar: "+ArrowABar);
										this.SignalLabels[ArrowABar] = new v2SignalInfo('1', Lows[0].GetValueAt(ArrowABar)-atrSeparation*pLabelSeparation, LONG, ArrowBrush, BkgCharId);
									}
									if(pSignal1Qty>0 && EntryABar != CurrentBar) BuyUsingATM(pATMStrategyName1, pSignal1Qty, Math.Abs(v2SignalData.SignalInteger).ToString());
							} else if(RaptorClouds[1].TrendDirection == SHORT
								&& RaptorClouds[2].TrendDirection == SHORT
								&& MostRecentPosition <= 0){
									ArrowTag = "Crossover_Sell"+cbar.ToString();
									ArrowBrush = CloudCrossover_SellBrush_ID;
									BkgCharId = GetContrastingBrush(pCloudCrossover_SellBrush);
									v2SignalData.ABar_CloudCrossoverSignal = ArrowABar;
									v2SignalData.SignalInteger = -1;
									if(pShowSignalLabels_v2) {
if(isBen) Print(ArrowTag+" #1  Price: "+(Highs[0].GetValueAt(ArrowABar)+atrSeparation*pLabelSeparation)+"   abar: "+ArrowABar);
										this.SignalLabels[ArrowABar] = new v2SignalInfo('1', Highs[0].GetValueAt(ArrowABar)+atrSeparation*pLabelSeparation, SHORT, ArrowBrush, BkgCharId);
									}
									if(pSignal1Qty>0 && EntryABar != CurrentBar) SellUsingATM(pATMStrategyName1, pSignal1Qty, Math.Abs(v2SignalData.SignalInteger).ToString());
							}
						}
						#endregion
					}
					if(v2SignalData.ABar_CloudCrossoverSignal>0 && v2SignalData.ABar_CloudCrossoverSignal!=ArrowABar){
						if(pShowSoftEdgeCounterTrendSignal){
							#region SoftEdgeCounterTrendSignal
							if(RaptorClouds[1].SearchForRetestExtreme && RaptorClouds[2].SearchForRetestExtreme){
								int x1 = -1, x2 = -1;
								if(RaptorClouds[1].TrendDirection==LONG && position[0]==SHORT){
				//if the extreme price occurs on the 2nd instance of the Swing, then this algorithm works.  If the extreme price occurs sometime prior to the 2nd swing, then this algorithm may not truly be testing the extreme price
									x2 = swing2.SwingHighBar(2,2,cbar-RaptorClouds[1].ABar_Crossover)+(IsHistorical? 0:1);
									if(!SwingABarsHash.Contains(cbar-x2)){
										x1 = swing2.SwingHighBar(2,1,cbar-RaptorClouds[1].ABar_Crossover)+(IsHistorical? 0:1);
										c1 = Math.Abs(x1-x2) >= pMinRetestBarSeparation;
										c2 = x1>0 && x2>0 && Math.Abs(Highs[0][x2]-Highs[0][x1]) <= atrSeparation*pExtremeRetestSensitivity;
										if(c1 && c2){
											SwingABarsHash.Add(cbar-x2);
											v2SignalData.ABar_SoftEdgeCounterTrend = ArrowABar;
											v2SignalData.SignalInteger = -2;
											ArrowTag = "SoftEdge_Sell"+cbar.ToString();
											ArrowBrush = SoftEdge_SellBrush_ID;
											BkgCharId = GetContrastingBrush(pSoftEdge_SellBrush);
											if(pShowSignalLabels_v2) {
												this.SignalLabels[ArrowABar] = new v2SignalInfo('2',Highs[0].GetValueAt(cbar)+atrSeparation*pLabelSeparation,SHORT,ArrowBrush, BkgCharId);
											}
											if(pSignal2Qty>0 && EntryABar != CurrentBar) SellUsingATM(pATMStrategyName2, pSignal2Qty, Math.Abs(v2SignalData.SignalInteger).ToString());
										}
									}
								}else if(RaptorClouds[1].TrendDirection==SHORT && position[0]==LONG){
				//if the extreme price occurs on the 2nd instance of the Swing, then this algorithm works.  If the extreme price occurs sometime prior to the 2nd swing, then this algorithm may not truly be testing the extreme price
									x2 = swing2.SwingLowBar(2,2,cbar-RaptorClouds[1].ABar_Crossover)+(IsHistorical? 0:1);
									if(!SwingABarsHash.Contains(cbar-x2)){
										x1 = swing2.SwingLowBar(2,1,cbar-RaptorClouds[1].ABar_Crossover)+(IsHistorical? 0:1);
										if(Math.Abs(x1-x2)>=pMinRetestBarSeparation && x1>0 && x2>0 && Math.Abs(Lows[0][x2]-Lows[0][x1])<=atrSeparation*pExtremeRetestSensitivity){
											SwingABarsHash.Add(cbar-x2);
											v2SignalData.ABar_SoftEdgeCounterTrend = ArrowABar;
											v2SignalData.SignalInteger = 2;
											ArrowTag = "SoftEdge_Buy"+cbar.ToString();
											ArrowBrush = SoftEdge_BuyBrush_ID;
											BkgCharId = GetContrastingBrush(pSoftEdge_BuyBrush);
											if(pShowSignalLabels_v2) {
if(isBen) Print(ArrowTag+" #2  Price: "+(Lows[0].GetValueAt(cbar)).ToString()+"-"+(atrSeparation)+"*"+(pLabelSeparation).ToString()+"   abar: "+cbar);
												this.SignalLabels[ArrowABar] = new v2SignalInfo('2',Lows[0].GetValueAt(cbar)-atrSeparation*pLabelSeparation,LONG, ArrowBrush, BkgCharId);
											}
											if(pSignal2Qty>0 && EntryABar != CurrentBar) BuyUsingATM(pATMStrategyName2, pSignal2Qty, Math.Abs(v2SignalData.SignalInteger).ToString());
										}
									}
								}
							}
							#endregion
						}
						if(pShowHardEdgeSignal){
							#region HardEdgeSignal
							int HardEdgeTrendDirection = FLAT;
							int Direction2 = FLAT;
							int Direction3 = FLAT;
							if(this.pHardEdgeTrendConfirmation){
								if(RaptorClouds[1].TrendDirection==LONG && RaptorClouds[2].TrendDirection==LONG)   HardEdgeTrendDirection=LONG;
								if(RaptorClouds[1].TrendDirection==SHORT && RaptorClouds[2].TrendDirection==SHORT) HardEdgeTrendDirection=SHORT;
								Direction2 = RaptorClouds[1].CheckForHardEdgeSignal(cbar, MostRecentPosition, ArrowABar, this.pShowCloudCrossoverSignal, v2SignalData.ABar_CloudCrossoverSignal, LONG, SHORT, ref ArrowTag, ref ArrowBrush, HardEdge_BuyBrush_ID, HardEdge_SellBrush_ID, HardEdgeTrendDirection);
								Direction3 = RaptorClouds[2].CheckForHardEdgeSignal(cbar, MostRecentPosition, ArrowABar, this.pShowCloudCrossoverSignal, v2SignalData.ABar_CloudCrossoverSignal, LONG, SHORT, ref ArrowTag, ref ArrowBrush, HardEdge_BuyBrush_ID, HardEdge_SellBrush_ID, HardEdgeTrendDirection);
							}else{
								if(RaptorClouds[2].TrendDirection==LONG) HardEdgeTrendDirection=LONG;
								if(RaptorClouds[2].TrendDirection==SHORT) HardEdgeTrendDirection=SHORT;
								Direction2 = RaptorClouds[2].CheckForHardEdgeSignal(cbar, MostRecentPosition, ArrowABar, this.pShowCloudCrossoverSignal, v2SignalData.ABar_CloudCrossoverSignal, LONG, SHORT, ref ArrowTag, ref ArrowBrush, HardEdge_BuyBrush_ID, HardEdge_SellBrush_ID, HardEdgeTrendDirection);
							}
							if(Direction2==LONG || Direction3==LONG){
								v2SignalData.SignalInteger = 3;
if(isBen) Print(ArrowABar+" #3  Price: "+(Lows[0].GetValueAt(cbar)-atrSeparation*pLabelSeparation)+"   abar: "+cbar);
								if(pShowSignalLabels_v2) {
									this.SignalLabels[ArrowABar]= new v2SignalInfo('3',Lows[0].GetValueAt(ArrowABar)-atrSeparation*pLabelSeparation, LONG, ArrowBrush, BkgCharId);
								}
								if(pSignal3Qty>0 && EntryABar != CurrentBar) BuyUsingATM(pATMStrategyName3, pSignal3Qty, Math.Abs(v2SignalData.SignalInteger).ToString());
							} else if(Direction2==SHORT || Direction3==SHORT) {
								v2SignalData.SignalInteger = -3;
if(isBen) Print(ArrowABar+" #3  Price: "+(Highs[0].GetValueAt(cbar)+atrSeparation*pLabelSeparation)+"   abar: "+cbar);
								if(pShowSignalLabels_v2) {
									this.SignalLabels[ArrowABar]= new v2SignalInfo('3',Highs[0].GetValueAt(ArrowABar)+atrSeparation*pLabelSeparation, SHORT, ArrowBrush, BkgCharId);
								}
								if(pSignal3Qty>0 && EntryABar != CurrentBar) SellUsingATM(pATMStrategyName3, pSignal3Qty, Math.Abs(v2SignalData.SignalInteger).ToString());
							}
							#endregion
						}
					}
				}
				if(ArrowTag!=string.Empty){
					System.Windows.Media.Brush tempbrush = null;
					switch (ArrowBrush) {
						case Brush_UpArrow_ID:   tempbrush = this.pBrush_UpArrow.Clone(); break;
						case Brush_DownArrow_ID: tempbrush = this.pBrush_DownArrow.Clone(); break;
						case CloudCrossover_BuyBrush_ID:  tempbrush = this.pCloudCrossover_BuyBrush.Clone(); break;
						case CloudCrossover_SellBrush_ID: tempbrush = this.pCloudCrossover_SellBrush.Clone(); break;
						case SoftEdge_BuyBrush_ID:  tempbrush = this.pSoftEdge_BuyBrush.Clone(); break;
						case SoftEdge_SellBrush_ID: tempbrush = this.pSoftEdge_SellBrush.Clone(); break;
						case HardEdge_BuyBrush_ID:  tempbrush = this.pHardEdge_BuyBrush.Clone(); break;
						case HardEdge_SellBrush_ID: tempbrush = this.pHardEdge_SellBrush.Clone(); break;
					}
					tempbrush.Freeze();
					if(MostRecentPosition==LONG){
						position_v2[0] = v2SignalData.SignalInteger;
						if(ChartControl!=null){
							TriggerCustomEvent(o2 =>{
								if(!pReverseArrows) {
									if(pArrowLocation==iwRaptor_ArrowLocation.OffBar)       Draw.ArrowUp(this, ArrowTag, IsAutoScale,rbar,Lows[0].GetValueAt(ArrowABar)-pArrowSeparation*TickSize,tempbrush);
									if(pArrowLocation==iwRaptor_ArrowLocation.AtEntryPrice) Draw.ArrowUp(this, ArrowTag, IsAutoScale,rbar,EntryPrice,tempbrush);
								}else{
									if(pArrowLocation==iwRaptor_ArrowLocation.OffBar)       Draw.ArrowDown(this, ArrowTag, IsAutoScale, rbar+1, Highs[0].GetValueAt(ArrowABar-1)+pArrowSeparation*TickSize,tempbrush);
									if(pArrowLocation==iwRaptor_ArrowLocation.AtEntryPrice) Draw.ArrowDown(this, ArrowTag, IsAutoScale, rbar+1, BuyEntryPoint.GetValueAt(ArrowABar),tempbrush);
								}
							},0,null);
							if(pBackgroundOpacity>0) {
								tempbrush = tempbrush.Clone();
								tempbrush.Opacity = pBackgroundOpacity/10.0;
								tempbrush.Freeze();
								BackBrushes[rbar] = tempbrush.Clone();
							}
						}
					} else if(MostRecentPosition==SHORT){
						position_v2[0] = v2SignalData.SignalInteger;
						if(ChartControl!=null){
							if(!pReverseArrows) {
								if(pArrowLocation==iwRaptor_ArrowLocation.OffBar)       Draw.ArrowDown(this, ArrowTag, IsAutoScale, rbar,Highs[0].GetValueAt(ArrowABar)+pArrowSeparation*TickSize,tempbrush);
								if(pArrowLocation==iwRaptor_ArrowLocation.AtEntryPrice) Draw.ArrowDown(this, ArrowTag, IsAutoScale, rbar,EntryPrice,tempbrush);
						}else{
								if(pArrowLocation==iwRaptor_ArrowLocation.OffBar)       Draw.ArrowUp(this, ArrowTag, IsAutoScale, rbar+1, Lows[0].GetValueAt(ArrowABar-1)-pArrowSeparation*TickSize,tempbrush);
								if(pArrowLocation==iwRaptor_ArrowLocation.AtEntryPrice) Draw.ArrowUp(this, ArrowTag, IsAutoScale, rbar+1, SellEntryPoint.GetValueAt(ArrowABar),tempbrush);
							}
							if(pBackgroundOpacity>0 && tempbrush!=null) {
								tempbrush = tempbrush.Clone();
								tempbrush.Opacity = pBackgroundOpacity/10.0;
								tempbrush.Freeze();
								BackBrushes[rbar] = tempbrush.Clone();
							}
						}
					}
				}
				#endregion
				#region Sound, Email and Popup alerts
				if(BuyEntryPoint.IsValidDataPoint(0) && ArrowABar!=0) {
					if(pEnableSoundAlerts && SoundEntryBar!=cbar && pBuySound.Length>0) {
						SoundEntryBar = cbar;
						Alert(cbar.ToString(),Priority.High,"iwRaptor now Long, Up Arrow printed",AddSoundFolder(pBuySound),1,Brushes.Green,Brushes.White);
					}
					if(pLaunchPopupOnEntryPrice && this.PopupBarOnEntry!=cbar && !(IsHistorical)) {
						PopupBarOnEntry = cbar;
						Log("iwRaptor LONG on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
					}
					if(pEnableEmails && pEmailAlertOnEntryPrice.Length>0 && this.EmailBar!=cbar) {
						EmailBar = cbar;
						SendMail(pEmailAlertOnSetup,"iwRaptor LONG on "+Instrument.FullName,"iwRaptor LONG on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
					}
				} 
				else if(BuyWarningDot.IsValidDataPoint(0)){
					if(pEnableSoundAlerts && SoundWarningBar!=cbar && pBuyWarningSound.Length>0) {
						SoundWarningBar = cbar;
						Alert(cbar.ToString(),Priority.High,"iwRaptor Long Warning Dot printed",AddSoundFolder(pBuyWarningSound),1,Brushes.Green,Brushes.White);
					}
					if(pLaunchPopupOnSetup && this.PopupBarOnSetup!=cbar && !(IsHistorical)) {
						PopupBarOnSetup = cbar;
						Log("iwRaptor potential BUY forming on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
					}
					if(pEnableEmails && pEmailAlertOnSetup.Length>0 && this.EmailBar!=cbar) {
						EmailBar = cbar;
						SendMail(pEmailAlertOnSetup,"iwRaptor potential BUY on "+Instrument.FullName,"iwRaptor potential BUY forming  on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
					}
				}
				if(SellEntryPoint.IsValidDataPoint(0) && ArrowABar!=0) {
					if(pEnableSoundAlerts && SoundEntryBar!=cbar && pSellSound.Length>0) {
						SoundEntryBar = cbar;
						Alert(cbar.ToString(),Priority.High,"iwRaptor now Short, Down arrow printed",AddSoundFolder(pSellSound),1,Brushes.Red,Brushes.White);
					}
					if(pLaunchPopupOnEntryPrice && this.PopupBarOnEntry!=cbar && !(IsHistorical)) {
						PopupBarOnEntry = cbar;
						Log("iwRaptor SHORT entry price on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
					}
					if(pEnableEmails && pEmailAlertOnEntryPrice.Length>0 && this.EmailBar!=cbar) {
						EmailBar = cbar;
						SendMail(pEmailAlertOnSetup,"iwRaptor SHORT on "+Instrument.FullName,"iwRaptor SHORT on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
					}
				} 
				else if(SellWarningDot.IsValidDataPoint(0)){
					if(pEnableSoundAlerts && SoundWarningBar!=cbar && pSellWarningSound.Length>0) {
						SoundWarningBar = cbar;
						Alert(cbar.ToString(),Priority.High,"iwRaptor Short Warning Dot printed",AddSoundFolder(pSellWarningSound),1,Brushes.Red,Brushes.White);
					}
					if(pLaunchPopupOnSetup && this.PopupBarOnSetup!=cbar && !(IsHistorical)) {
						PopupBarOnSetup = cbar;
						Log("iwRaptor potential SELL forming on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
					}
					if(pEnableEmails && pEmailAlertOnSetup.Length>0 && this.EmailBar!=cbar) {
						EmailBar = cbar;
						SendMail(pEmailAlertOnSetup,"iwRaptor potential SELL on "+Instrument.FullName,"iwRaptor potential SELL forming on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
					}
				}
				#endregion
			}
			if(!BuyEntryPoint.IsValidDataPoint(0) && !SellEntryPoint.IsValidDataPoint(0) && IsFirstTickOfBar){
				PreSignalMsg = string.Empty;
				RemoveDrawObject("PreSignalMsg");
				#region Reset Presignal data
				for(int r = 1; r<=2; r++){
					RaptorClouds[-r].ABar_Crossover               = RaptorClouds[r].ABar_Crossover;
					RaptorClouds[-r].ABar_HardEdgeTouch           = RaptorClouds[r].ABar_HardEdgeTouch;
					RaptorClouds[-r].ABar_PriceLeftCloud          = RaptorClouds[r].ABar_PriceLeftCloud;
					RaptorClouds[-r].Fast                         = RaptorClouds[r].Fast;
					RaptorClouds[-r].Slow                         = RaptorClouds[r].Slow;
					RaptorClouds[-r].Med                          = RaptorClouds[r].Med;
					RaptorClouds[-r].SearchForHardEdgeTouchSignal = RaptorClouds[r].SearchForHardEdgeTouchSignal;
					RaptorClouds[-r].SearchForRetestExtreme       = RaptorClouds[r].SearchForRetestExtreme;
					RaptorClouds[-r].SEH                          = RaptorClouds[r].SEH;
					RaptorClouds[-r].SEL                          = RaptorClouds[r].SEL;
					RaptorClouds[-r].TrendDirection               = RaptorClouds[r].TrendDirection;
					RaptorClouds[-r].TrendStrength                = RaptorClouds[r].TrendStrength;
				}
				v2PresignalData.ABar_CloudCrossoverSignal = v2SignalData.ABar_CloudCrossoverSignal;
				v2PresignalData.ABar_SoftEdgeCounterTrend = v2SignalData.ABar_SoftEdgeCounterTrend;
				v2PresignalData.ExtremeHasBeenRetested    = v2SignalData.ExtremeHasBeenRetested;
				v2PresignalData.LastArrowABar             = v2SignalData.LastArrowABar;
				v2PresignalData.SignalInteger             = v2SignalData.SignalInteger;
				#endregion
			}

}catch(Exception err){Print(line+":  "+err.ToString());}


		}
//==================================================================================
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Position_V1
		{	get 
			{ 
				Update();
				return position; 
			}
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Position_V2
		{	get 
			{ 
				Update();
				return position_v2;
			}
		}
//====================================================================
		private string AddSoundFolder(string wav){
			if(!wav.ToLower().EndsWith(".wav")) wav = wav+".wav";
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
		}
//====================================================================
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

				System.IO.FileInfo[] filCustom=null;
				try{
					var dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
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
		SharpDX.Direct2D1.Brush WhiteBrushDX = null;
		SharpDX.Direct2D1.Brush BlackBrushDX = null;
		SharpDX.Direct2D1.Brush pBrush_UpArrowDX = null;
		SharpDX.Direct2D1.Brush pBrush_DownArrowDX = null;
		SharpDX.Direct2D1.Brush pCloudCrossover_BuyBrushDX = null;
		SharpDX.Direct2D1.Brush pCloudCrossover_SellBrushDX = null;
		SharpDX.Direct2D1.Brush pSoftEdge_BuyBrushDX = null;
		SharpDX.Direct2D1.Brush pSoftEdge_SellBrushDX = null;
		SharpDX.Direct2D1.Brush pHardEdge_BuyBrushDX = null;
		SharpDX.Direct2D1.Brush pHardEdge_SellBrushDX = null;
		
		public override void OnRenderTargetChanged()
		{
			tm.InitializeBrushes(RenderTarget);
			#region -- ortg --
			if(dimgrayDXBrush!=null && !dimgrayDXBrush.IsDisposed) { dimgrayDXBrush.Dispose(); dimgrayDXBrush = null;}
			if(RenderTarget!=null) dimgrayDXBrush = pBandCounterBkgBrush.ToDxBrush(RenderTarget);

			if(yellowDXBrush!=null && !yellowDXBrush.IsDisposed)   { yellowDXBrush.Dispose(); yellowDXBrush = null;}
			if(RenderTarget!=null) yellowDXBrush = pBandCounterTxtBrush.ToDxBrush(RenderTarget);

			if(WhiteBrushDX!=null && !WhiteBrushDX.IsDisposed) { WhiteBrushDX.Dispose(); WhiteBrushDX = null;}
			if(RenderTarget!=null) WhiteBrushDX = Brushes.White.ToDxBrush(RenderTarget);
			if(BlackBrushDX!=null && !BlackBrushDX.IsDisposed) { BlackBrushDX.Dispose(); BlackBrushDX = null;}
			if(RenderTarget!=null) BlackBrushDX = Brushes.Black.ToDxBrush(RenderTarget);

			if(pBrush_UpArrowDX!=null && !pBrush_UpArrowDX.IsDisposed) { pBrush_UpArrowDX.Dispose(); pBrush_UpArrowDX = null;}
			if(RenderTarget!=null) pBrush_UpArrowDX = pBrush_UpArrow.ToDxBrush(RenderTarget);

			if(pBrush_DownArrowDX!=null && !pBrush_DownArrowDX.IsDisposed) { pBrush_DownArrowDX.Dispose(); pBrush_DownArrowDX = null;}
			if(RenderTarget!=null) pBrush_DownArrowDX = pBrush_DownArrow.ToDxBrush(RenderTarget);

			if(pCloudCrossover_BuyBrushDX!=null && !pCloudCrossover_BuyBrushDX.IsDisposed) { pCloudCrossover_BuyBrushDX.Dispose(); pCloudCrossover_BuyBrushDX = null;}
			if(RenderTarget!=null) pCloudCrossover_BuyBrushDX = pCloudCrossover_BuyBrush.ToDxBrush(RenderTarget);

			if(pCloudCrossover_SellBrushDX!=null && !pCloudCrossover_SellBrushDX.IsDisposed) { pCloudCrossover_SellBrushDX.Dispose(); pCloudCrossover_SellBrushDX = null;}
			if(RenderTarget!=null) pCloudCrossover_SellBrushDX = pCloudCrossover_SellBrush.ToDxBrush(RenderTarget);

			if(pSoftEdge_BuyBrushDX!=null && !pSoftEdge_BuyBrushDX.IsDisposed) { pSoftEdge_BuyBrushDX.Dispose(); pSoftEdge_BuyBrushDX = null;}
			if(RenderTarget!=null) pSoftEdge_BuyBrushDX = pSoftEdge_BuyBrush.ToDxBrush(RenderTarget);

			if(pSoftEdge_SellBrushDX!=null && !pSoftEdge_SellBrushDX.IsDisposed) { pSoftEdge_SellBrushDX.Dispose(); pSoftEdge_SellBrushDX = null;}
			if(RenderTarget!=null) pSoftEdge_SellBrushDX = pSoftEdge_SellBrush.ToDxBrush(RenderTarget);

			if(pHardEdge_SellBrushDX!=null && !pHardEdge_SellBrushDX.IsDisposed) { pHardEdge_SellBrushDX.Dispose(); pHardEdge_SellBrushDX = null;}
			if(RenderTarget!=null) pHardEdge_SellBrushDX = pHardEdge_SellBrush.ToDxBrush(RenderTarget);

			if(pHardEdge_BuyBrushDX!=null && !pHardEdge_BuyBrushDX.IsDisposed) { pHardEdge_BuyBrushDX.Dispose(); pHardEdge_BuyBrushDX = null;}
			if(RenderTarget!=null) pHardEdge_BuyBrushDX = pHardEdge_BuyBrush.ToDxBrush(RenderTarget);
			#endregion
		}
		SharpDX.Direct2D1.Brush brushDX = null;
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!IsVisible) return;
			base.OnRender(chartControl, chartScale);

			float x,y;
			if((pShowPnLStats || Keyboard.IsKeyDown(Key.LeftCtrl)) && tm.OutputLS.Count>0){
				tm.OnRender(RenderTarget, ChartPanel, tm.OutputLS, 14, 10);
			}
			if(MessageTime!=DateTime.MinValue){
				var ts = new TimeSpan(DateTime.Now.Ticks - MessageTime.Ticks);
				if(ts.Seconds > 15 || ChartControl.Properties.ChartTraderVisibility == ChartTraderVisibility.Visible) {
					RemoveDrawObject("preexist");
					RemoveDrawObject("OrderError");
					MessageTime = DateTime.MinValue;
				}
			}
			#region Plot
			if(pShowSignalLabels_v2){
				for(int abar = ChartBars.FromIndex; abar<=ChartBars.ToIndex; abar++) {
					if(SignalLabels.ContainsKey(abar)){
						var txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, SignalLabels[abar].Type.ToString(), txtFormat_LabelFont, ChartPanel.W, txtFormat_LabelFont.FontSize);
						x = chartControl.GetXByBarIndex(ChartBars, abar) - txtLayout1.Metrics.Width/2f;
						y = chartScale.GetYByValue( SignalLabels[abar].Price);
						if(SignalLabels[abar].Direction==LONG) {
							y = y-txtLayout1.Metrics.Height;
						}
						var txtPosition1 = new SharpDX.Vector2(x,y);
						#region Draw background rectangle
						if(RenderTarget!=null){
							var rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, txtLayout1.Metrics.Width+2f, txtLayout1.Metrics.Height+2f);
							RenderTarget.FillRectangle(rectangleF, SignalLabels[abar].BackBrush == 'W' ? WhiteBrushDX : BlackBrushDX);
							switch (SignalLabels[abar].ForeBrush) {
								case Brush_UpArrow_ID:            brushDX = pBrush_UpArrowDX;   break;
								case Brush_DownArrow_ID:          brushDX = pBrush_DownArrowDX; break;
								case CloudCrossover_BuyBrush_ID:  brushDX = pCloudCrossover_BuyBrushDX;  break;
								case CloudCrossover_SellBrush_ID: brushDX = pCloudCrossover_SellBrushDX; break;
								case SoftEdge_BuyBrush_ID:        brushDX = pSoftEdge_BuyBrushDX;  break;
								case SoftEdge_SellBrush_ID:       brushDX = pSoftEdge_SellBrushDX; break;
								case HardEdge_BuyBrush_ID:        brushDX = pHardEdge_BuyBrushDX;  break;
								case HardEdge_SellBrush_ID:       brushDX = pHardEdge_SellBrushDX; break;
							}
							if(brushDX!=null)
								RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, brushDX);
						}
						#endregion
						if(txtLayout1!=null){
							txtLayout1.Dispose();
							txtLayout1 = null;
						}
					}
				}
			}
			if(pShowBandCounter && RenderTarget != null){
				int RMaB = Math.Max(0,Math.Min(ChartBars.ToIndex, BarsArray[0].Count-1));
				string bandcounterstring = $"BandCounter: {(BandCounter.GetValueAt(RMaB)/TickSize).ToString("0")}-ticks";
				var txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, bandcounterstring, txtFormat_BandCounterFont, ChartPanel.W, txtFormat_BandCounterFont.FontSize);
				var txtPosition1 = new SharpDX.Vector2(100f,(float)(Math.Max(0,Math.Min(ChartPanel.H-txtLayout1.Metrics.Height-3, pBandCounterVertical))));
				#region Draw background rectangle
				var rectangleF = new SharpDX.RectangleF(txtPosition1.X-4f, txtPosition1.Y-4f, txtLayout1.Metrics.Width+8f, txtLayout1.Metrics.Height+8f);
				RenderTarget.FillRectangle(rectangleF, dimgrayDXBrush);
				#endregion
				RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, yellowDXBrush);
			}
			#endregion
			#region -- Inform user about trading status --
			var msg = "";
			if(tm.DailyTgtAchieved) msg = "Daily PnL target achieved - trading halted";
			else if(SelectedAccountName.Trim().Length == 0 || myAccount==null) msg = "TradeAssist disabled:  Please select a valid trading account name";
			else if(!LongsPermittedNow && !ShortsPermittedNow)
				msg = $"TradeAssist disabled:  {DateTime.Now.ToShortTimeString()} is outside of your Start/End time setting";
			else{
				ATMStrategyNamesEmployed = $"#1:  '{pATMStrategyName1}'  Qty: {pSignal1Qty}\n#2:  '{pATMStrategyName2}'  Qty: {pSignal2Qty}\n#3:  '{pATMStrategyName3}'  Qty: {pSignal3Qty}";
				msg = (!IsAlwaysActive && IsTradingPaused) ? "TRADING IS PAUSED" : $"{(pPermittedDirection==Raptor_PermittedDirections.Both? "Long and Shorts are" : (pPermittedDirection == Raptor_PermittedDirections.Long ? "Long ONLY is": (pPermittedDirection == Raptor_PermittedDirections.Short? "Short ONLY is":"with TREND is")))} {(IsAlwaysActive ? "ALWAYS ":"")}active on '{SelectedAccountName}' with\n{ATMStrategyNamesEmployed}";
				if(pPermittedDirection == Raptor_PermittedDirections.Trend)
					msg = $"{msg}\nTrend is {BandEdgeTrendStr}";
			}
			if(true){
				var txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, msg, txtFormat_LabelFont, ChartPanel.W, txtFormat_LabelFont.FontSize);
				x = ChartPanel.W/2 - txtLayout1.Metrics.Width/2;
				double midprice = (chartScale.MaxValue + chartScale.MinValue)/2.0;
				if(Closes[0].GetValueAt(CurrentBars[0]) < midprice)
					y = chartScale.GetYByValue((chartScale.MaxValue*3 + midprice)/4.0);
				else
					y = chartScale.GetYByValue((chartScale.MinValue*3 + midprice)/4.0);
				var txtPosition1 = new SharpDX.Vector2(x,y);
				#region Draw background rectangle and text
				if(RenderTarget!=null){
					var rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, txtLayout1.Metrics.Width+2f, txtLayout1.Metrics.Height+2f);
					RenderTarget.FillRectangle(rectangleF, WhiteBrushDX);
					if(BlackBrushDX != null)
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, BlackBrushDX);
				}
				#endregion
				if(txtLayout1!=null){
					txtLayout1.Dispose();
					txtLayout1 = null;
				}
			}
			#endregion
		}
		#region Plots
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BuyEntryPoint {get { return Values[0]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SellEntryPoint {get { return Values[1]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BuyWarningDot {get { return Values[2]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SellWarningDot {get { return Values[3]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BuyAlert {get { return Values[4]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SellAlert {get { return Values[5]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> FilterLevel {get { return Values[6]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> PositionAge {get { return Values[7]; }}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BandEdge1 {get { return Values[8]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SoftBandEdgeHigh1 {get { return Values[9]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SoftBandEdgeLow1 {get { return Values[10]; }}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BandEdge2 {get { return Values[11]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SoftBandEdgeHigh2 {get { return Values[12]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SoftBandEdgeLow2 {get { return Values[13]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BandCounter {get { return Values[14]; }}

		#endregion

		#region RobertWilliams custom
		private bool pEnableYellowBarFilter = true;
		private TimeSpan pBeginTime1 = new TimeSpan(8,30,0);
		private TimeSpan pEndTime1 = new TimeSpan(11,45,0);
		private TimeSpan pBeginTime2 = new TimeSpan(13,0,0);
		private TimeSpan pEndTime2 = new TimeSpan(16,15,0);
		private bool pUse2ndSession = false;
#if ROBERT_WILLIAMS_CUSTOM
		[Description("When true, no arrows or plot elements will be permitted to print on a bar immediately following a Yellow bar")]
		[Category("Custom")]
		public bool EnableYellowBarFilter
		{
			get { return pEnableYellowBarFilter; }
			set { pEnableYellowBarFilter = value; }
		}
		private int pOutOfSessionOpacity = 2;
		[Description("Background opacity of chart when the bar is not in a trading session")]
// 		[Category("Session Times")]
// [Gui.Design.DisplayNameAttribute(" Bkg Opacity")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Bkg Opacity",  GroupName = "Session Times")]
		public int OutOfSessionOpacity
		{
			get { return pOutOfSessionOpacity; }
			set { pOutOfSessionOpacity = Math.Max(0,Math.Min(10,value)); }
		}
		private Brush pOutOfSessionBackgroundBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of background of chart when the bar is not in a trading session")]
// 		[Category("Session Times")]
// [Gui.Design.DisplayNameAttribute(" Bkg Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Bkg Color",  GroupName = "Session Times")]
		public Brush BGBrush{	get { return pOutOfSessionBackgroundBrush; }	set { pOutOfSessionBackgroundBrush = value; }		}
		[Browsable(false)]
		public string BGClSerialize
		{	get { return Serialize.BrushToString(pOutOfSessionBackgroundColor); } set { pOutOfSessionBackgroundColor = Serialize.StringToBrush(value); }
		}
		#region Session times
		[Description("Start time of session #1 (use 24hr format)")]
// [Gui.Design.DisplayNameAttribute("#1 Session Begin")]
// 		[Category("Session Times")]
[Display(ResourceType = typeof(Custom.Resource), Name = "#1 Session Begin",  GroupName = "Session Times")]
		public string BeginTimeOfDay1 {
			get { return pBeginTime1.ToString(); }
			set { 
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pBeginTime1); 
				pBeginTime1 = new TimeSpan(Math.Min(23,pBeginTime1.Hours), Math.Min(59,pBeginTime1.Minutes), Math.Min(59,pBeginTime1.Seconds));
			}
		}
		[Description("End time of session #1 (use 24hr format)")]
// [Gui.Design.DisplayNameAttribute("#1 Session End")]
// 		[Category("Session Times")]
[Display(ResourceType = typeof(Custom.Resource), Name = "#1 Session End",  GroupName = "Session Times")]
		public string EndTimeOfDay1 {
			get { return pEndTime1.ToString(); }
			set {
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pEndTime1); 
				pEndTime1 = new TimeSpan(Math.Min(23,pEndTime1.Hours), Math.Min(59,pEndTime1.Minutes), Math.Min(59,pEndTime1.Seconds));
			}
		}
// 		[Category("Session Times")]
		[Description("")]
// [Gui.Design.DisplayNameAttribute(" Enable Session #2")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Enable Session #2",  GroupName = "Session Times")]
		public bool Use2ndSession
		{
			get { return pUse2ndSession; }
			set { pUse2ndSession = value; }
		}
		[Description("Start time of session #2 (use 24hr format)")]
// [Gui.Design.DisplayNameAttribute("#2 Session Begin")]
// 		[Category("Session Times")]
[Display(ResourceType = typeof(Custom.Resource), Name = "#2 Session Begin",  GroupName = "Session Times")]
		public string BeginTimeOfDay2 {
			get { return pBeginTime2.ToString(); }
			set {
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pBeginTime2); 
				pBeginTime2 = new TimeSpan(Math.Min(23,pBeginTime2.Hours), Math.Min(59,pBeginTime2.Minutes), Math.Min(59,pBeginTime2.Seconds));
			}
		}
		[Description("End time of session #2 (use 24hr format)")]
// [Gui.Design.DisplayNameAttribute("#2 Session End")]
// 		[Category("Session Times")]
[Display(ResourceType = typeof(Custom.Resource), Name = "#2 Session End",  GroupName = "Session Times")]
		public string EndTimeOfDay2 {
			get { return pEndTime2.ToString(); }
			set {
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pEndTime2); 
				pEndTime2 = new TimeSpan(Math.Min(23,pEndTime2.Hours), Math.Min(59,pEndTime2.Minutes), Math.Min(59,pEndTime2.Seconds));
			}
		}
		#endregion
#endif
		#endregion

		#region Properties ---------------------------------------
		[NinjaScriptProperty]
		[Range(1, 2359)]
		[Display(Order=10, Name="Start Time", GroupName="Backtester", Description="New positions will be taken after this time")]
		public int pStartTime
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 2359)]
		[Display(Order=11, Name="Stop Time", GroupName="Backtester", Description="No new positions will be taken after this time")]
		public int pStopTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 2359)]
		[Display(Order=12, Name="Flat Time", GroupName="Backtester", Description="All positions will be flattened at this time")]
		public int pFlatTime
		{ get; set; }
		
		[Display(Order=30, Name="Days of week", Description="Sunday = 0, Monday = 1, etc", GroupName="Backtester")]
		public string pPermittedDOW
		{get;set;}

		private string pTargetDistStr1 = "UseATM";
		private string pTargetDistStr2 = "UseATM";
		private string pTargetDistStr3 = "UseATM";
		private string pStoplossDistStr1 = "UseATM";
		private string pStoplossDistStr2 = "UseATM";
		private string pStoplossDistStr3 = "UseATM";
//		[NinjaScriptProperty]
//		[Display(Name="#1 Target Size and type", Description="'$100', 'atr 3.5' or 't 15' (per contract)", Order=40, GroupName="Backtester")]
//		public string pTargetDistStr1
//		{get;set;}
//		[NinjaScriptProperty]
//		[Display(Name="#1 SL Size and type", Description="'UseATM' or 'atr 3.5'", Order=41, GroupName="Backtester")]
//		public string pStoplossDistStr1
//		{get;set;}

//		[NinjaScriptProperty]
//		[Display(Name="#2 Target Size and type", Description="'UseATM' or 'atr 3.5'", Order=50, GroupName="Backtester")]
//		public string pTargetDistStr2
//		{get;set;}
//		[NinjaScriptProperty]
//		[Display(Name="#2 SL Size and type", Description="'UseATM' or 'atr 3.5'", Order=51, GroupName="Backtester")]
//		public string pStoplossDistStr2
//		{get;set;}

//		[NinjaScriptProperty]
//		[Display(Name="#3 Target Size and type", Description="'UseATM' or 'atr 3.5'", Order=60, GroupName="Backtester")]
//		public string pTargetDistStr3
//		{get;set;}
//		[NinjaScriptProperty]
//		[Display(Name="#3 SL Size and type", Description="'UseATM' or 'atr 3.5'", Order=61, GroupName="Backtester")]
//		public string pStoplossDistStr3
//		{get;set;}

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Daily Profit Stop $", Order=70, GroupName="Backtester", Description="")]
		public double pDailyTgtDollars
		{get;set;}
		
		[Display(Name="Show PnL table?", Order=80, GroupName="Backtester", Description="")]
		public bool pShowPnLStats
		{get;set;}

		[Display(Name="Show Hourly PnL", Order=81, GroupName="Backtester", Description="")]
		public bool pShowHrOfDayPnL
		{get;set;}
		
		[Display(Name="Show PnL Lines", Order=82, GroupName="Backtester", Description="")]
		public bool pShowPnLLines
		{get;set;}
		
	
		#region -- Presignals visual --
		private iwRaptor_TextPositionTypes pPresignalTextPosition = iwRaptor_TextPositionTypes.TopRight;
		[Description("Where to print the text of the pre-signal warnings")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Txt location",  GroupName = "Presignals Visual")]
		public iwRaptor_TextPositionTypes PresignalTextPosition{
			get { return pPresignalTextPosition; }
			set { pPresignalTextPosition = value; }
		}
		private int pPresignalTextSize = 14;
		[Description("Size of the presignal font")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Font size",  GroupName = "Presignals Visual")]
		public int PresignalTextSize{
			get { return pPresignalTextSize; }
			set { pPresignalTextSize = Math.Max(10, value); }
		}

		private int pPresignalBackgroundOpacity = 2;
		[Description("Colorize the background of the presignal bar?  Enter '0' to turn-off, max of 10 for fully opaque")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Background opacity",  GroupName = "Presignals Visual")]
		public int PresignalBkgOpacity{
			get { return pPresignalBackgroundOpacity; }
			set { pPresignalBackgroundOpacity = Math.Max(0,Math.Min(10,value)); }
		}
		
		private bool pSwitchToV1_Signals = false;
//		[Description("Switch to the printing of the the original (Raptor v1.0) signals")]
//		[RefreshProperties(RefreshProperties.All)]
//// 		[Category("Special Signals")]
//// [Gui.Design.DisplayNameAttribute(" Switch to v1.0 Signals")]
//[Display(ResourceType = typeof(Custom.Resource), Name = "Switch to v1.0 Signals",  GroupName = "Special Signals")]
//		public bool SwitchToV1_Signals{
//			get { return pSwitchToV1_Signals; }
//			set { pSwitchToV1_Signals = value; }
//		}
		#endregion

		#region -- Special Signals --
		private bool pShowCloudCrossoverSignal = true;
		[Description("Enable the printing of the first signal after a cloud crossover occurs (both clouds are in the same trend direction)")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "a) Show CloudCrossover Signals",  GroupName = "Special Signals")]
		public bool ShowCloudCrossoverSignal{
			get { return pShowCloudCrossoverSignal; }
			set { pShowCloudCrossoverSignal = value; }
		}

		private bool pShowSoftEdgeCounterTrendSignal = true;
		[Description("Enable the printing of the Soft Edge counter-trend signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "b) Show Soft-Edge Signals",  GroupName = "Special Signals")]
		public bool ShowSoftEdgeCounterTrendSignal{
			get { return pShowSoftEdgeCounterTrendSignal; }
			set { pShowSoftEdgeCounterTrendSignal = value; }
		}

		private bool pShowHardEdgeSignal = true;
		[Description("Enable the printing of the Hard Edge trend continuation signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "c) Show Hard-Edge Signals",  GroupName = "Special Signals")]
		public bool ShowHardEdgeSignal{
			get { return pShowHardEdgeSignal; }
			set { pShowHardEdgeSignal = value; }
		}
		#endregion
		
		#region Special Signals Visual
		private double pLabelSeparation = 2.1;
		[Description("Distance between price bar and signal label")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal Labels Separation",  GroupName = "Special Signals Visual")]
		public double LabelSeparation{
			get { return pLabelSeparation; }
			set { pLabelSeparation = value; }
		}
		
		private bool pShowSignalLabels_v2 = true;
		[Description("Print signal id on all v2.0 signals?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal Labels Show",  GroupName = "Special Signals Visual")]
		public bool ShowSignalLabels_v2{
			get { return pShowSignalLabels_v2; }
			set { pShowSignalLabels_v2 = value; }
		}
		private int pSignalLabelFontSize = 12;
		[Description("Font size (in pixels)")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal Labels FontSize",  GroupName = "Special Signals Visual")]
		public int SignalLabelFontSize{
			get { return pSignalLabelFontSize; }
			set { pSignalLabelFontSize = value; }
		}

		private Brush pCloudCrossover_BuyBrush = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of arrow and background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CloudCrossover Buy",  GroupName = "Special Signals Visual")]
		public Brush CloudCrossover_BuyBrush{	get { return pCloudCrossover_BuyBrush; }	set { pCloudCrossover_BuyBrush = value; }		}
		[Browsable(false)]
		public string CloudCrossover_BuyBrush_Serialize
		{	get { return Serialize.BrushToString(pCloudCrossover_BuyBrush); } set { pCloudCrossover_BuyBrush = Serialize.StringToBrush(value); }
		}
		private Brush pCloudCrossover_SellBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of arrow and background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CloudCrossover Sell",  GroupName = "Special Signals Visual")]
		public Brush CloudCrossover_SellBrush{	get { return pCloudCrossover_SellBrush; }	set { pCloudCrossover_SellBrush = value; }		}
		[Browsable(false)]
		public string CloudCrossover_SellBrush_Serialize
		{	get { return Serialize.BrushToString(pCloudCrossover_SellBrush); } set { pCloudCrossover_SellBrush = Serialize.StringToBrush(value); }
		}
		private Brush pSoftEdge_BuyBrush = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of arrow and background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SoftEdge Buy",  GroupName = "Special Signals Visual")]
		public Brush SoftEdge_BuyBrush{	get { return pSoftEdge_BuyBrush; }	set { pSoftEdge_BuyBrush = value; }		}
		[Browsable(false)]
		public string SoftEdgeBuyColor_Serialize
		{	get { return Serialize.BrushToString(pSoftEdge_BuyBrush); } set { pSoftEdge_BuyBrush = Serialize.StringToBrush(value); }
		}
		private Brush pSoftEdge_SellBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of arrow and background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SoftEdge Sell",  GroupName = "Special Signals Visual")]
		public Brush SoftEdge_SellBrush{	get { return pSoftEdge_SellBrush; }	set { pSoftEdge_SellBrush = value; }		}
		[Browsable(false)]
		public string SoftEdge_SellBrush_Serialize
		{	get { return Serialize.BrushToString(pSoftEdge_SellBrush); } set { pSoftEdge_SellBrush = Serialize.StringToBrush(value); }
		}
		private Brush pHardEdge_BuyBrush = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of arrow and background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HardEdge Buy",  GroupName = "Special Signals Visual")]
		public Brush HardEdge_BuyBrush{	get { return pHardEdge_BuyBrush; }	set { pHardEdge_BuyBrush = value; }		}
		[Browsable(false)]
		public string HardEdgeBuyColor_Serialize
		{	get { return Serialize.BrushToString(pHardEdge_BuyBrush); } set { pHardEdge_BuyBrush = Serialize.StringToBrush(value); }
		}
		private Brush pHardEdge_SellBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of arrow and background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HardEdge Sell",  GroupName = "Special Signals Visual")]
		public Brush HardEdge_SellBrush{	get { return pHardEdge_SellBrush; }	set { pHardEdge_SellBrush = value; }		}
		[Browsable(false)]
		public string HardEdge_SellBrush_Serialize
		{	get { return Serialize.BrushToString(pHardEdge_SellBrush); } set { pHardEdge_SellBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		#region Sounds
		private string pBuyWarningSound = "none";//"Raptor_Long_Setup.wav";
		[Category("1.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when a buy warning prints, will generate message in Alerts window")]
		public string SoundBuyWarning
		{
			get { return pBuyWarningSound; }
			set { pBuyWarningSound = value; }
		}
		private string pSellWarningSound = "none";//"Raptor_Short_Setup.wav";
		[Category("1.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when a sell warning prints, will generate message in Alerts window")]
		public string SoundSellWarning
		{
			get { return pSellWarningSound; }
			set { pSellWarningSound = value; }
		}
		private string pBuySound = "none";//"Raptor_Long.wav";
		[Category("1.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when an buy price prints, will generate message in Alerts window")]
		public string SoundBuy
		{
			get { return pBuySound; }
			set { pBuySound = value; }
		}
		private string pSellSound = "none";//"Raptor_Short.wav";
		[Category("1.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when a sell price prints, will generate message in Alerts window")]
//		[Gui.Design.DisplayName("")]
		public string SoundSell
		{
			get { return pSellSound; }
			set { pSellSound = value; }
		}
		#endregion

		private bool pEnableSoundAlerts = true;
		[Description("Enable sound alerts?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sounds Enabled",  GroupName = "Audible")]
		public bool EnableSoundAlerts {	get { return pEnableSoundAlerts; }	set { pEnableSoundAlerts = value; }		}

		#region -- Trade Assist --
		public class LoadAccountNameList : TypeConverter
		{
			#region -- LoadAccountNameList --
		    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		    {
		        if (context == null)
		        	return null; 
				
		        System.Collections.ArrayList list = new System.Collections.ArrayList();

		        for (int i = 0; i < Account.All.Count; i++)
		        {
		            if (Account.All[i].ConnectionStatus == ConnectionStatus.Connected)
		                list.Add(Account.All[i].Name);
					else NinjaScript.Log(string.Format("'{0}' connection status: {1}", Account.All[i].Name,Account.All[i].ConnectionStatus.ToString()), LogLevel.Information);
		        }
				
		        return new TypeConverter.StandardValuesCollection(list);
		    }

		    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		    { return true; }
			#endregion
		}
		[TypeConverter(typeof(LoadAccountNameList))]
		[Display(Order = 10, Name = "Account Name", GroupName = "Trade Assist", Description = "")]
		public string SelectedAccountName	{get;set;}

		internal class LoadpATMStrategyNames : StringConverter
		{
			#region LoadpATMStrategyNames
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
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"templates","AtmStrategy");
				string search = "*.xml";

				System.IO.FileInfo[] filCustom=null;
				try{
					var dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						if(!fi.Name.ToLower().StartsWith("ignore"))
							list.Add(fi.Name.Replace(".xml",string.Empty));
					}
				}
				if(list.Count>0){
					list.Sort();
					return new StandardValuesCollection(list.ToArray());
				}else{
					var arr = new string[]{"No templates"};
					return new StandardValuesCollection(arr);
				}
			}
			#endregion
		}
		[Display(Order = 20, Name = "Permitted direction", GroupName = "Trade Assist", Description = "What trade directions are permitted?  Use the UI button to change this parameter")]
		public Raptor_PermittedDirections pPermittedDirection {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadpATMStrategyNames))]
		[Display(Order = 30, Name = "Signal 1 ATM Strategy Name", GroupName = "Trade Assist", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template")]
		public string pATMStrategyName1 { get; set; }

		private int pSignal1Qty = 0;
		[Description("Set to '0' to stop trading")]
		[Display(Order = 40, Name = "Signal 1 Qty",  GroupName = "Trade Assist", ResourceType = typeof(Custom.Resource))]
		public int Signal1Qty{
			get { return pSignal1Qty; }
			set { pSignal1Qty = Math.Max(0,value); }
		}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadpATMStrategyNames))]
		[Display(Order = 50, Name = "Signal 2 ATM Strategy Name", GroupName = "Trade Assist", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template")]
		public string pATMStrategyName2 { get; set; }

		private int pSignal2Qty = 0;
		[Description("Set to '0' to stop trading")]
		[Display(Order = 60, Name = "Signal 2 Qty",  GroupName = "Trade Assist", ResourceType = typeof(Custom.Resource))]
		public int Signal2Qty{
			get { return pSignal2Qty; }
			set { pSignal2Qty = Math.Max(0,value); }
		}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadpATMStrategyNames))]
		[Display(Order = 70, Name = "Signal 3 ATM Strategy Name", GroupName = "Trade Assist", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template")]
		public string pATMStrategyName3 { get; set; }

		private int pSignal3Qty = 0;
		[Description("Set to '0' to stop trading")]
		[Display(Order = 80, Name = "Signal 3 Qty",  GroupName = "Trade Assist", ResourceType = typeof(Custom.Resource))]
		public int Signal3Qty{
			get { return pSignal3Qty; }
			set { pSignal3Qty = Math.Max(0,value); }
		}

		#endregion --------------------------------------------

		#region v2 sounds
//================
		private string pHardEdgeLongSetupSound = "none";//"Number_Three_Long_Setup.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("HardEdge Long Presignal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HardEdge Long Presignal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #3 presignal appears, will generate message in Alerts window")]
		public string HardEdgeLongSetupSound
		{
			get { return pHardEdgeLongSetupSound; }
			set { pHardEdgeLongSetupSound = value; }
		}
		private string pHardEdgeShortSetupSound = "none";//"Number_Three_Short_Setup.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("HardEdge Short Presignal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HardEdge Short Presignal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #3 presignal appears, will generate message in Alerts window")]
		public string HardEdgeShortSetupSound
		{
			get { return pHardEdgeShortSetupSound; }
			set { pHardEdgeShortSetupSound = value; }
		}
		private string pSoftEdgeLongSetupSound = "none";//"Number_Two_Long_Setup.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("SoftEdge Long Presignal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SoftEdge Long Presignal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #2 presignal appears, will generate message in Alerts window")]
		public string SoftEdgeLongSetupSound
		{
			get { return pSoftEdgeLongSetupSound; }
			set { pSoftEdgeLongSetupSound = value; }
		}
		private string pSoftEdgeShortSetupSound = "none";//"Number_Two_Short_Setup.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("SoftEdge Short Presignal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SoftEdge Short Presignal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #2 presignal appears, will generate message in Alerts window")]
		public string SoftEdgeShortSetupSound
		{
			get { return pSoftEdgeShortSetupSound; }
			set { pSoftEdgeShortSetupSound = value; }
		}
		private string pCloudCrossLongSetupSound = "none";//"Number_One_Long_Setup.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("CloudCross Long Presignal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CloudCross Long Presignal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #1 presignal appears, will generate message in Alerts window")]
		public string CloudCrossLongSetupSound
		{
			get { return pCloudCrossLongSetupSound; }
			set { pCloudCrossLongSetupSound = value; }
		}
		private string pCloudCrossShortSetupSound = "none";//"Number_One_Short_Setup.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("CloudCross Short Presignal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CloudCross Short Presignal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #1 presignal appears, will generate message in Alerts window")]
		public string CloudCrossShortSetupSound
		{
			get { return pCloudCrossShortSetupSound; }
			set { pCloudCrossShortSetupSound = value; }
		}

		private string pHardEdgeLongSound = "none";//"Number_Three_Long.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("HardEdge Long entry signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HardEdge Long entry signal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #3 signal appears, will generate message in Alerts window")]
		public string HardEdgeLongSound
		{
			get { return pHardEdgeLongSound; }
			set { pHardEdgeLongSound = value; }
		}
		private string pHardEdgeShortSound = "none";//"Number_Three_Short.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("HardEdge Short entry signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HardEdge Short entry signal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #3 signal appears, will generate message in Alerts window")]
		public string HardEdgeShortSound
		{
			get { return pHardEdgeShortSound; }
			set { pHardEdgeShortSound = value; }
		}
		private string pSoftEdgeLongSound = "none";//"Number_Two_Long.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("SoftEdge Long entry signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SoftEdge Long entry signal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #2 signal appears, will generate message in Alerts window")]
		public string SoftEdgeLongSound
		{
			get { return pSoftEdgeLongSound; }
			set { pSoftEdgeLongSound = value; }
		}
		private string pSoftEdgeShortSound = "none";//"Number_Two_Short.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("SoftEdge Short entry signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SoftEdge Short entry signal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #2 signal appears, will generate message in Alerts window")]
		public string SoftEdgeShortSound
		{
			get { return pSoftEdgeShortSound; }
			set { pSoftEdgeShortSound = value; }
		}
		private string pCloudCrossLongSound = "none";//"Number_One_Long.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("CloudCross Long entry signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CloudCross Long entry signal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #1 signal appears, will generate message in Alerts window")]
		public string CloudCrossLongSound
		{
			get { return pCloudCrossLongSound; }
			set { pCloudCrossLongSound = value; }
		}
		private string pCloudCrossShortSound = "none";//"Number_One_Short.wav";
// 		[Category("2.0 Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayNameAttribute("CloudCross Short entry signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CloudCross Short entry signal",  GroupName = "2.0 Audible")]
		[Description("WAV file to play when a #1 signal appears, will generate message in Alerts window")]
		public string CloudCrossShortSound
		{
			get { return pCloudCrossShortSound; }
			set { pCloudCrossShortSound = value; }
		}
//================
		#endregion

		#region -- Signal Parameters SE and HE --
		private double pExtremeRetestSensitivity = 1.01;
		[Description("Used for SoftEdge signals, this is the max distance (in ATRs) between the extreme price and the retest of that price.  Larger numbers mean the retest is more tolerant, smaller numbers means the retest is less tolerant")]
		[Display(Name = "Extreme Retest Sensitivity",  GroupName = "Signal Parameters SE", ResourceType = typeof(Custom.Resource))]
		public double ExtremeRetestSensitivity{
			get { return pExtremeRetestSensitivity; }
			set { pExtremeRetestSensitivity = Math.Max(0,value); }
		}
		private int pMinRetestBarSeparation = 5;
		[Description("Used for SoftEdge signals, min number of bars between extreme price swing bar and retest swing bar.  Larger numbers mean a greater separation distance is required to validate the restest, smaller numbers means a smaller number of bars is required to validate the retest (more tolerant)")]
		[Display(Name = "Min Retest bar sep",  GroupName = "Signal Parameters SE", ResourceType = typeof(Custom.Resource))]
		public int MinRetestBarSeparation{
			get { return pMinRetestBarSeparation; }
			set { pMinRetestBarSeparation = Math.Max(0,value); }
		}
		private bool pHardEdgeTrendConfirmation = false;
		[Description("Used for HardEdge signals, TrendConfirmation means both clouds must be in the same trend direction to permit #3 signals")]
		[Display(Name = "TrendConfirmation",  GroupName = "Signal Parameters HE", ResourceType = typeof(Custom.Resource))]
		public bool HardEdgeTrendConfirmation{
			get { return pHardEdgeTrendConfirmation; }
			set { pHardEdgeTrendConfirmation = value; }
		}
		#endregion

		#region Cloud Setup
		private Brush pBand1_Brush = Brushes.RoyalBlue;
		[XmlIgnore()]
		[Description("Color of background Raptor Cloud for 12-tick MeanRenko2")]
		[Display(Name = "1 Color",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public Brush Band1_Brush{	get { return pBand1_Brush; }	set { pBand1_Brush = value; }		}
		[Browsable(false)]
		public string Band1ClSerialize
		{	get { return Serialize.BrushToString(pBand1_Brush); } set { pBand1_Brush = Serialize.StringToBrush(value); }
		}
		private int pBand1_Opacity = 15;
		[Description("Opacity of background Raptor Cloud...0=transparent thru 100=opaque")]
		[Display(Name = "1 Opacity",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public int Band1_Opacity{	get { return pBand1_Opacity; }	set { pBand1_Opacity = Math.Max(0,Math.Min(100,value)); }		}

		private int pBand1_BrickSizeTrend = 12;
		[Description("Trend Brick size (in ticks) of the MeanRenko2 bar for Raptor Cloud #1")]
		[Display(Name = "1 BrickSize Trend",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public int Band1_BrickSizeTrend{	
			get { return pBand1_BrickSizeTrend; }	
			set { pBand1_BrickSizeTrend = Math.Max(1,value); }
		}
		private int pBand1_BrickSizeReverse = 12;
		[Description("Brick size (in ticks) of the MeanRenko2 bar for Raptor Cloud #1")]
		[Display(Name = "1 BrickSize Reverse",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public int Band1_BrickSizeReverse{	
			get { return pBand1_BrickSizeReverse; }	
			set { pBand1_BrickSizeReverse = Math.Max(1,value); }
		}

		private Brush pBand2_Brush = Brushes.CornflowerBlue;
		[XmlIgnore()]
		[Description("Color of background Raptor Cloud #2")]
		[Display(Name = "2 Color",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public Brush Band2_Brush{	get { return pBand2_Brush; }	set { pBand2_Brush = value; }		}
		[Browsable(false)]
		public string Band2ClSerialize
		{	get { return Serialize.BrushToString(pBand2_Brush); } set { pBand2_Brush = Serialize.StringToBrush(value); }
		}
		private int pBand2_Opacity = 15;
		[Description("Opacity of background Raptor Cloud #2...0=transparent thru 100=opaque")]
		[Display(Name = "2 Opacity",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public int Band2_Opacity{	get { return pBand2_Opacity; }	set { pBand2_Opacity = Math.Max(0,Math.Min(100,value)); }		}

		private int pBand2_BrickSizeTrend = 8;
		[Description("Trend Brick size (in ticks) of the MeanRenko2 bar for Raptor Cloud #2")]
		[Display(Name = "2 BrickSize Trend",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public int Band2_BrickSizeTrend{	
			get { return pBand2_BrickSizeTrend; }	
			set { pBand2_BrickSizeTrend = Math.Max(1,value); }
		}
		private int pBand2_BrickSizeReverse = 8;
		[Description("Brick size (in ticks) of the MeanRenko2 bar for Raptor Cloud #2")]
		[Display(Name = "2 BrickSize Reverse",  GroupName = "Cloud Setup", ResourceType = typeof(Custom.Resource))]
		public int Band2_BrickSizeReverse{	
			get { return pBand2_BrickSizeReverse; }	
			set { pBand2_BrickSizeReverse = Math.Max(1,value); }
		}
		#endregion

		private iwRaptor_BarBrushingMAtype pBarBrushMAtype = iwRaptor_BarBrushingMAtype.SMA;
//		[Description("Type of MA on MeanRenko chart - controls bar coloring")]
//		[Category("Price Bars")]
//		public iwRaptor_BarBrushingMAtype BarBrushMAtype{	get { return pBarBrushMAtype; }	set { pBarBrushMAtype = value; }		}

		private int pBarBrushMALength = 55;
//		[Description("Period of the MovingAverage on MeanRenko chart - controls bar coloring")]
//		[Category("Price Bars")]
//		public int BarBrushMALength{	get { return pBarBrushMALength; }	set { pBarBrushMALength = value; }		}

		#region -- Hawk Bars --
		private bool pEnableCounterTrendFilter = true;
//		[Description("When true, it is filtering out entry arrows that are countrary to the HawkBar bar color")]
//		[Category("Hawk Bars")]
//		public bool EnableCounterTrendFilter
//		{
//			get { return pEnableCounterTrendFilter; }
//			set { pEnableCounterTrendFilter = value; }
//		}
		private Brush pPLNeutral = Brushes.Goldenrod;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HawkBarNeutral",  GroupName = "Hawk Bars")]
		public Brush PLNeutral{	get { return pPLNeutral; }	set { pPLNeutral = value; }		}
		[Browsable(false)]
		public string PLNeutralClSerialize
		{	get { return Serialize.BrushToString(pPLNeutral); } set { pPLNeutral = Serialize.StringToBrush(value); }
		}

		private Brush pPLUp = Brushes.DodgerBlue;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HawkBarUp",  GroupName = "Hawk Bars")]
		public Brush PLUp{	get { return pPLUp; }	set { pPLUp = value; }		}
		[Browsable(false)]
		public string PLUpClSerialize
		{	get { return Serialize.BrushToString(pPLUp); } set { pPLUp = Serialize.StringToBrush(value); }
		}
		private Brush pPLDn = Brushes.Magenta;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HawkBarDown",  GroupName = "Hawk Bars")]
		public Brush PLDn{	get { return pPLDn; }	set { pPLDn = value; }		}
		[Browsable(false)]
		public string PLDnClSerialize
		{	get { return Serialize.BrushToString(pPLDn); } set { pPLDn = Serialize.StringToBrush(value); }
		}
		private int pHawkBarBrickSizeTrend = 6;
		[Description("Trend ticks of background MeanRenko2 chart - controls bar coloring")]
		[Category("Hawk Bars")]
		public int HawkBarBrickSizeTrend{	get { return pHawkBarBrickSizeTrend; }	set { pHawkBarBrickSizeTrend = value; }		}
		private int pHawkBarBrickSizeReverse = 6;
		[Description("Reversal ticks of background MeanRenko2 chart - controls bar coloring")]
		[Category("Hawk Bars")]
		public int HawkBarBrickSizeReverse{	get { return pHawkBarBrickSizeReverse; }	set { pHawkBarBrickSizeReverse = value; }		}
		#endregion

		#region ATR Filter
		private bool bEnableAtrFilter = true;
		private int iAtrLength = 14; 
		private double iAtrMult = 2.0; 

        #endregion

		#region -- Alert --
		private bool pEnableEmails = true;
		[Description("To disable all emails, set to 'false'")]
		[Category("Alert")]
		[Display(Order = 10)]
		public bool EnableEmails
		{
			get { return pEnableEmails; }
			set { pEnableEmails = value; }
		}
		private string pEmailAlertOnSetup = "";
		[Category("Alert")]
		[Description("Email address to receive warning dot alert messages")]
		[Display(Order = 20)]
		public string EmailAlertOnSetup
		{
			get { return pEmailAlertOnSetup; }
			set { pEmailAlertOnSetup = value; }
		}
		private string pEmailAlertOnEntryPrice = "";
		[Category("Alert")]
		[Description("Email address to receive an email alert message when a buy or sell entry price is established")]
		[Display(Order = 30)]
		public string EmailAlertOnEntryPrice
		{
			get { return pEmailAlertOnEntryPrice; }
			set { pEmailAlertOnEntryPrice = value; }
		}

		private bool pLaunchPopupOnEntryPrice = false;
		[Category("Alert")]
		[Description("Launch a popup message when a fill arrow prints")]
		[Display(Order = 40)]
		public bool LaunchPopupOnEntryPrice
		{
			get { return pLaunchPopupOnEntryPrice; }
			set { pLaunchPopupOnEntryPrice = value; }
		}
		private bool pLaunchPopupOnSetup = false;
		[Category("Alert")]
		[Description("Launch a popup message when a warning dot prints")]
		[Display(Order = 50)]
		public bool LaunchPopupOnSetup
		{
			get { return pLaunchPopupOnSetup; }
			set { pLaunchPopupOnSetup = value; }
		}
//		private bool pShowTradeEntryPoints = true;
//		[Description("Show TradeEntryPoint plots?")]
//		[Category("Alert")]
//		[Gui.Design.DisplayNameAttribute("Show prices at entry")]
//		public bool ShowTradeEntryPoints{	get { return pShowTradeEntryPoints; }	set { pShowTradeEntryPoints = value; }		}
		#endregion

		#region Visual
		private bool pShowBandCounter = false;
        [Display(Name = "Show BandCounter", GroupName = "Visual", Description = "Print the band width (BandCounter) in ticks to the chart", Order = 0)]
		public bool ShowBandCounter{
			get { return pShowBandCounter; }
			set { pShowBandCounter = value; }
		}
		private int pBandCounterVertical = 100;
        [Display(Name = "BandCounter Vertical", GroupName = "Visual", Description = "The vertical location of the BandCounter text, in pixels", Order = 1)]
		public int BandCounterVertical{
			get { return pBandCounterVertical; }
			set { pBandCounterVertical = value; }
		}
		private int pBandCounterFontSize = 16;
		[Description("Font size (in pixels)")]
        [Display(Name = "BandCounter FontSize", GroupName = "Visual", Description = "The size of the BandCounter text", Order = 2)]
		public int BandCounterFontSize{
			get { return pBandCounterFontSize; }
			set { pBandCounterFontSize = Math.Max(4,value); }
		}
		private Brush pBandCounterTxtBrush = Brushes.Yellow;
		[XmlIgnore()]
		[Description("Color of BandCounter text")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BandCounter Text",  GroupName = "Visual", Order = 3)]
		public Brush BandCounterTxtBrush{	get { return pBandCounterTxtBrush; }	set { pBandCounterTxtBrush = value; }		}
		[Browsable(false)]
		public string pBandCounterTxtBrush_Serialize
		{	get { return Serialize.BrushToString(pBandCounterTxtBrush); } set { pBandCounterTxtBrush = Serialize.StringToBrush(value); }
		}
		private Brush pBandCounterBkgBrush = Brushes.DimGray;
		[XmlIgnore()]
		[Description("Color of BandCounter background fill area")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BandCounter Bkg",  GroupName = "Visual", Order = 4)]
		public Brush BandCounterBkgBrush{	get { return pBandCounterBkgBrush; }	set { pBandCounterBkgBrush = value; }		}
		[Browsable(false)]
		public string pBandCounterBkgBrush_Serialize
		{	get { return Serialize.BrushToString(pBandCounterBkgBrush); } set { pBandCounterBkgBrush = Serialize.StringToBrush(value); }
		}

		private int pArrowSeparation = 2;
		[Description("Ticks between buy/sell entry arrow and price bar")]
		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("")]
		public int ArrowSeparation {	get { return pArrowSeparation; }	set { pArrowSeparation = Math.Max(0,value); }		}

		private bool pReverseArrows = false;
		[Description("Reverse arrow direction?...this also moves the arrows to the bar that had the first hash mark")]
		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("")]
		public bool ReverseArrows {	get { return pReverseArrows; }	set { pReverseArrows = value; }		}

		int pBackgroundOpacity = 2;
		[Description("Colorize the background of the signal bar?  Enter '0' to turn-off, max of 10 for fully opaque")]
		[Category("Visual")]
		public int BackgroundOpacity
		{
			get { return pBackgroundOpacity; }
			set { pBackgroundOpacity = Math.Max(0, Math.Min(10,value)); }
		}

		private Brush pBrush_UpArrow = Brushes.LimeGreen;
		[XmlIgnore()]
		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("Color_UpArrow")]
		public Brush Brush_UpArrow{	get { return pBrush_UpArrow; }	set { pBrush_UpArrow = value; }		}
		[Browsable(false)]
		public string ABAClSerialize
		{	get { return Serialize.BrushToString(pBrush_UpArrow); } set { pBrush_UpArrow = Serialize.StringToBrush(value); }
		}
		private Brush pBrush_DownArrow = Brushes.Crimson;
		[XmlIgnore()]
		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("Color_DownArrow")]
		public Brush Brush_DownArrow{	get { return pBrush_DownArrow; }	set { pBrush_DownArrow = value; }		}
		[Browsable(false)]
		public string ASAClSerialize
		{	get { return Serialize.BrushToString(pBrush_DownArrow); } set { pBrush_DownArrow = Serialize.StringToBrush(value); }
		}
		private iwRaptor_ArrowLocation pArrowLocation = iwRaptor_ArrowLocation.OffBar;
		[Description("Location of arrow, OffBar = not on the bar, but away from it.  AtEntryPrice = the price of the suggested entry")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Arrow Location",  GroupName = "Visual")]
		public iwRaptor_ArrowLocation ArrowLocation{	get { return pArrowLocation; }	set { pArrowLocation = value; }		}
		#endregion

        #endregion

	 	public class RaptorConverter : IndicatorBaseConverter // or StrategyBaseConverter
	    {
			#region RaptorConverter
	        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
	        {
	            // we need the indicator instance which actually exists on the grid
	            RTS_Raptor indicator = component as RTS_Raptor;

	            // base.GetProperties ensures we have all the properties (and associated property grid editors)
	            // NinjaTrader internal logic determines for a given indicator
	            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
	                                                                        ? base.GetProperties(context, component, attrs)
	                                                                        : TypeDescriptor.GetProperties(component, attrs);

				if (indicator == null || propertyDescriptorCollection == null)
				    return propertyDescriptorCollection;

				// These values are will be shown/hidden (toggled) based on "ShowHideToggle" bool value
				PropertyDescriptor toggle = null;

				List<string> props=null;
				if(indicator.EnableSoundAlerts == false){
					props = new List<string>(){
					//remove v1 sounds
						"SoundBuyWarning", "SoundSellWarning", "SoundBuy", "SoundSell",
					//remove v1 color params
						"Color_UpArrow", "Color_DownArrow",
					//remove v2 sounds
						"CloudCrossLongSetupSound", "CloudCrossShortSetupSound", "CloudCrossLongSound", "CloudCrossShortSound", 
						"SoftEdgeLongSetupSound", "SoftEdgeShortSetupSound", "SoftEdgeLongSound", "SoftEdgeShortSound",
						"HardEdgeLongSetupSound", "HardEdgeShortSetupSound", "HardEdgeLongSound", "HardEdgeShortSound"
					};
					foreach(var pr in props){
						try{ toggle = propertyDescriptorCollection[pr];}	catch(Exception e){indicator.Print(e.ToString());}
						if(toggle != null) propertyDescriptorCollection.Remove(toggle);
					}
					props.Clear();
				}
				//if(indicator.SwitchToV1_Signals == false)
				if(false)
				{
					props = new List<string>(){
					//remove v2 signal and color params
						"ShowCloudCrossoverSignal", "ShowSoftEdgeCounterTrendSignal", "ShowHardEdgeSignal", "LabelSeparation", "SignalLabelFontSize", 
						"CloudCrossover_BuyBrush","CloudCrossover_SellBrush",
						"HardEdge_BuyBrush", "HardEdge_SellBrush",
						"SoftEdge_BuyBrush", "SoftEdge_SellBrush",
						"ExtremeRetestSensitivity", "MinRetestBarSeparation", "HardEdgeTrendConfirmation", "SignalLabelFontSize", "ShowSignalLabels_v2", 
						"PresignalTextPosition", "PresignalTextSize", "PresignalBkgOpacity",
					//remove v2 sounds
						"CloudCrossLongSetupSound", "CloudCrossShortSetupSound", "CloudCrossLongSound", "CloudCrossShortSound", 
						"SoftEdgeLongSetupSound", "SoftEdgeShortSetupSound", "SoftEdgeLongSound", "SoftEdgeShortSound",
						"HardEdgeLongSetupSound", "HardEdgeShortSetupSound", "HardEdgeLongSound", "HardEdgeShortSound"
					};
				}else{
					props = new List<string>(){
					//remove v1 sounds
						"SoundBuyWarning", "SoundSellWarning", "SoundBuy", "SoundSell",
					//remove v1 color params
						"Color_UpArrow", "Color_DownArrow"
					};
				}
				foreach(var pr in props){
					try{ toggle = propertyDescriptorCollection[pr];}	catch(Exception e){indicator.Print(e.ToString());}
					if(toggle != null) propertyDescriptorCollection.Remove(toggle);
				}
				return propertyDescriptorCollection;
	        }

	        // Important: This must return true otherwise the type convetor will not be called
	        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	        { return true; }
			#endregion
	    }
    }
}
public enum iwRaptor_ArrowLocation{OffBar,AtEntryPrice,DoNotPlot}
public enum iwRaptor_BarBrushingMAtype{EMA,SMA,RMA}
public enum iwRaptor_TextPositionTypes{TopRight,BottomRight,Center,TopLeft,BottomLeft,None}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RTS_Raptor[] cacheRTS_Raptor;
		public RTS_Raptor RTS_Raptor(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return RTS_Raptor(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public RTS_Raptor RTS_Raptor(ISeries<double> input, int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			if (cacheRTS_Raptor != null)
				for (int idx = 0; idx < cacheRTS_Raptor.Length; idx++)
					if (cacheRTS_Raptor[idx] != null && cacheRTS_Raptor[idx].pStartTime == pStartTime && cacheRTS_Raptor[idx].pStopTime == pStopTime && cacheRTS_Raptor[idx].pFlatTime == pFlatTime && cacheRTS_Raptor[idx].pDailyTgtDollars == pDailyTgtDollars && cacheRTS_Raptor[idx].EqualsInput(input))
						return cacheRTS_Raptor[idx];
			return CacheIndicator<RTS_Raptor>(new RTS_Raptor(){ pStartTime = pStartTime, pStopTime = pStopTime, pFlatTime = pFlatTime, pDailyTgtDollars = pDailyTgtDollars }, input, ref cacheRTS_Raptor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RTS_Raptor RTS_Raptor(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RTS_Raptor(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public Indicators.RTS_Raptor RTS_Raptor(ISeries<double> input , int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RTS_Raptor(input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RTS_Raptor RTS_Raptor(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RTS_Raptor(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public Indicators.RTS_Raptor RTS_Raptor(ISeries<double> input , int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RTS_Raptor(input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}
	}
}

#endregion
