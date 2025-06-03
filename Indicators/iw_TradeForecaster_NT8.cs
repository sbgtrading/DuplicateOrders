//#define CHECKAUTHORIZATION
//#define TEMPLATE_MANAGER

#region Using declarations
using System;
//using System.IO;
using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
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
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    [Description("")]
	public class TradeForecaster : Indicator
    {

//====================================================================================================
	#region MakeString
	private static string MakeString(object[] s, string Separator){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		return stb.ToString();
	}
	private void PrintMakeString(object[] s, string Separator){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		Print(stb.ToString());
	}
	private void PrintMakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		Print(stb.ToString());
	}
	private void PrintMakeString(string filepath, object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		System.IO.File.AppendAllText(filepath,stb.ToString());
	}
	private void PrintMakeString(string filepath, object[] s, string Separator) {
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		System.IO.File.AppendAllText(filepath,stb.ToString());
	}
	private static string MakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		return stb.ToString();
	}
		#endregion
//====================================================================================================
		private bool SessionTemplateChecked = false;

			#region Variables
			// Wizard generated variables
			private int    pWeeksToGoBack = 1;

			private int    MaxWeeksAccessible = 0;
			private string DataShortageMsg = string.Empty;

			private bool   pDrawLineLastWeek = true;
			private string pMarketOpenTimeStr="8:30";
			private DateTime   MarketOpenTime =DateTime.MinValue;
			private DateTime   MarketOpenTimeMinus75minutes =DateTime.MinValue;
			private double     TodayOpen=double.MinValue; //, ma, ma1, ma0, p;
			private bool       RunInit=true;

			private Series<double> Histo;
			private SortedDictionary<DateTime,double> TradeForecasterCore = new SortedDictionary<DateTime,double>();
			private double     HistoResult = 0;
			private int        relativebar = 0;

			private static DateTime PredictionTimeToSearchFor;

			private DateTime   EarliestDay = DateTime.MinValue;
			private CCI cci=null;
			#endregion


		private DateTime TimeAtLaunch = NinjaTrader.Core.Globals.Now;
		private DateTime TimeOfError  = DateTime.MinValue;
		private string ErrorMsg = "", debug_file=string.Empty, NL=Environment.NewLine;
		private bool Debug = false;
		private System.Windows.TextAlignment	msgAlignment = new System.Windows.TextAlignment();
//		private NinjaTrader.Gui.Tools.SimpleFont textFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial",12,FontStyle.Bold, GraphicsUnit.Point);
//		private NinjaTrader.Gui.Tools.SimpleFont textFontSmall = new NinjaTrader.Gui.Tools.SimpleFont("Arial",8,FontStyle.Italic, GraphicsUnit.Point);
		private bool InZone = false;
		private int line = 0;
//		private bool MktAnalyzerDataSaved = false;
		private bool RemoveDaysAgoMsg = false;
		//private TemporaryMessageManager TempMsgs = null;
		private DateTime ThisTimeLastWeek = DateTime.MinValue;
//		private int BarOfSession = -1;
		private double nt = 0, mt = 0, lt = 0;
		private int weekptrOffset = 1;  //if we have a holiday (missing chunk of data)...then go back an additional day
		private string[] ResultsMsg = new String[2]{null,null};
		private float[] MinutesRemaining = new float[3]{0,0,0}; //[0] is the TrendTrader, [1] is the SwingTrader, [2] is the Scalp
		//private Brush TrendTraderBarBrush;
		//private Brush SwingTraderBarBrush;
		//private Brush ScalpBarBrush;
//		private NinjaTrader.Gui.Tools.SimpleFont MessageFont;
		private string TrendTraderBarLabel = "Trend";
		private string SwingTraderBarLabel = "Swing";
		private string ScalpBarLabel       = "Scalp";
		private float MinBarWidth;
		private float MaxBarWidth;
		private float BarGroupHeight;
		private int CurrentSystem = -1;
//		private List<string> LogFileInfo = new List<string>();
		private Series<double> currentSuggestion, nextSuggestion;
        private TextFormat txtFormat_LabelFont = null;
		private int ErrorCount=0;

		private DateTime ttt = new DateTime(2012,4,12,23,59,0);

//====================================================================
		protected override void OnStateChange()
		{
			#region OnStateChange
			if (State == State.SetDefaults)
			{
//ClearOutputWindow();
				Name = "iw DTS TradeForecaster v6.6";
				//v6.0 added the CurrentSuggestion and NextSuggestion, and made it compatible with MarketAnalyzer
				//v6.1 verified compatibility with BloodHound
				//v6.2 moved to NT8
			
				Debug = System.IO.File.Exists("c:\\222222222222.txt");
				Debug = Debug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!Debug)
					VendorLicense("IndicatorWarehouse", "AITradeForecaster", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(new System.Windows.Media.SolidColorBrush(Color.FromArgb(255,255,64,0)), 1),  PlotStyle.Dot, "Scalp");
				AddPlot(new Stroke(new System.Windows.Media.SolidColorBrush(Color.FromArgb(255,204,0,0)), 1),   PlotStyle.Dot, "Swing");
				AddPlot(new Stroke(new System.Windows.Media.SolidColorBrush(Color.FromArgb(255,0,107,179)), 1), PlotStyle.Dot, "Trend");
				AddLine(new Stroke(new System.Windows.Media.SolidColorBrush(Colors.DarkGray), DashStyleHelper.Solid, 1), 0, "Zero line");

				IsAutoScale=false;
				Calculate=Calculate.OnPriceChange;

				IsOverlay=true;
			//PriceTypeSupported	= false;
				DrawOnPricePanel    = true;
				PaintPriceMarkers   = false;
//ChartOnly=true;
			//Print(Name," leaving Initialize()");
	        }
//====================================================================
			if (State == State.Configure) {
				AddDataSeries(BarsPeriodType.Minute,1);

				//if(ChartControl!=null) TempMsgs = new TemporaryMessageManager(ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.AxisPen.Brush);

				//TrendTraderBarBrush = Plots[2].Brush.Clone(); TrendTraderBarBrush.Freeze();
				//SwingTraderBarBrush = Plots[1].Brush.Clone(); SwingTraderBarBrush.Freeze();
				//ScalpBarBrush       = Plots[0].Brush.Clone(); ScalpBarBrush.Freeze();
			}
			if (State == State.DataLoaded) {
				Histo = new Series<double>(SMA(BarsArray[1], 1), MaximumBarsLookBack.Infinite);
//				Histo			  = new Series<double>(SMA(Closes[1],1),MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				currentSuggestion = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				nextSuggestion    = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				txtFormat_LabelFont = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	pLabelFont.Family.ToString(),
															pLabelFont.Bold? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
													     	pLabelFont.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)pLabelFont.Size);
			}
			#endregion
		}
//====================================================================
		#region Support methods
private void Printf(string msg) {
	//File.AppendAllText(FileName, msg,Environment.NewLine);
}
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds\"+wav);
		}
//====================================================================
		private Brush ContrastBrush(Brush inputBrush){
				System.Windows.Media.Color c = ((System.Windows.Media.SolidColorBrush)inputBrush).Color;
				byte bRed 	= c.R;
				byte bGreen = c.G;
				byte bBlue 	= c.B;
				double v = (bRed*0.2126)+(bGreen*0.7152)+(bBlue*0.0722);
				if(v>128) return Brushes.Black;
				else return Brushes.White;
		}
		#endregion
//====================================================================
//====================================================================
		/// <summary>
		/// Called on each bar update event (incoming tick)
		/// </summary>
		protected override void OnBarUpdate() {
			//if(BarsInProgress == 0) return;
			line=226;
try{
			if(ErrorMsg.Length>0) {
				//DrawOnPricePanel = false;
				//if(TempMsgs!=null) TempMsgs.Add(string.Empty, ErrorMsg, TextPosition.Center,30);
//				Draw.TextFixed(this, "error1", ErrorMsg, TextPosition.Center);
				//Print(ErrorMsg); 
				//Alert("error",Priority.High,ErrorMsg,AddSoundFolder("Alert3.wav"),1000,Brushes.Red, Brushes.Black);  
			}
			if(Instrument==null) return;

			if(CurrentBars[0]<5) {
				return;
			}
			if(CurrentBars[1]<5) {
				RunInit = true;
				return;
			}

line=245;
			TimeSpan SinceLaunch = new TimeSpan(Math.Abs(NinjaTrader.Core.Globals.Now.Ticks - TimeAtLaunch.Ticks));
			if(SinceLaunch.TotalSeconds>20) {
				RemoveDaysAgoMsg = true;
				RemoveDrawObject("info");
				RemoveDrawObject("info1");
				RemoveDrawObject("info2");
			}
//			if(this.Panel<=0) {
//				IsAutoScale=false;
//			} else 
//				IsAutoScale=true;

			if(RunInit) {
				RunInit = false;
//ClearOutputWindow();

				#region RunInit

				MarketOpenTime = new DateTime(Times[1][0].Year, Times[1][0].Month,Times[1][0].Day, MarketOpenTime.Hour, MarketOpenTime.Minute,0,0);
				MarketOpenTimeMinus75minutes = MarketOpenTime.AddMinutes(-75);

				TimeSpan ts = new TimeSpan(Math.Abs(Times[1][0].Ticks-NinjaTrader.Core.Globals.Now.Ticks));
				if(ts.TotalDays<pMaxWeeks*7 && ChartControl!=null) {
					var BackBrush = ContrastBrush(ChartControl.Properties.AxisPen.Brush);
					DataShortageMsg = MakeString(new Object[]{"ERROR - Not enough bars:  Data begins ",ts.TotalDays.ToString("0"),"-days ago, please add more data so you have at least 25 days."});
//					if(TempMsgs!=null) {
//						if(pMsgLoc == MMv2_PredictionLocType.TopLeft) TempMsgs.Add(string.Empty, DataShortageMsg, TextPosition.TopRight, 30, Backcolor);
//						else TempMsgs.Add(string.Empty, DataShortageMsg, TextPosition.TopLeft, 30, Backcolor);
//					}
					Draw.TextFixed(this, "permanent",DataShortageMsg,TextPosition.TopLeft);
				} else {
//					if(TempMsgs!=null) {
//						if(pMsgLoc == MMv2_PredictionLocType.TopLeft) TempMsgs.Add(string.Empty, MakeString(new object []{"TradeForecaster data begins at: ",Times[1][0]}), TextPosition.TopRight,10);
//						else TempMsgs.Add(string.Empty, MakeString(new object []{"TradeForecaster data begins at: ",Times[1][0]}), TextPosition.TopLeft,10);
//					}
					Draw.TextFixed(this, "info2",MakeString(new Object[]{"Historical data began at: ",Times[1][0]}),TextPosition.BottomLeft);
				}

				#endregion
//				Draw.TextFixed(this, "barcount","CB[0]: "+BarsArray[0].Count+"   CB[1]: "+BarsArray[1].Count+"  Times[1][0]: "+Times[1][0].ToString(),TextPosition.Center);
			}
line=287;

			if(DataShortageMsg.Length>0) {
				Alert("permanent1",NinjaTrader.NinjaScript.Priority.High,MakeString(new Object[]{"Add more data to your ",Instrument.FullName," chart"}), AddSoundFolder("Alert2.wav"),100, Brushes.Red, Brushes.White);  
			}
line=292;
//Printf("416");
//if(Times[1][0]>ttt) Print(Environment.NewLine+Times[1][0].ToString());
			if(BarsInProgress==1) {
//				if(!SessionTemplateChecked) {
//					SessionTemplateChecked = true;
//					if(!BarsArray[1].Session.TemplateName.Contains("24/7") && !BarsArray[1].Session.TemplateName.Contains("24/5"))
//						Log("."+Environment.NewLine+"TradeForecaster Session Template warning"+Environment.NewLine+Environment.NewLine+"Session template on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString()+" is set to '"+BarsArray[1].Session.TemplateName+"'"+Environment.NewLine+"It is recommended you run TradeForecaster on a chart whose dataseries is set to either the 'Default 24/5' or 'Defaul 24/7' session template.  Change the session template by hitting 'Ctrl-F'",LogLevel.Alert);
//				}
				HistoResult = FillHisto(0, Times[1][0], Times[1][1]);
				//LogFileInfo.Add(string.Concat(Times[1][0].ToString(),"  ",HistoResult.ToString("0.0000"),Environment.NewLine));
				Histo[0] = (HistoResult);
				return;
			}

line=303;
			int lwb = BarsArray[1].GetBar(Times[1][0].AddDays(-7*weekptrOffset));
			int rbarLastWeek = CurrentBars[1]-lwb;
//Print(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}","t0: ",Times[0][0].ToString(),"   rbarLastWeek: ",rbarLastWeek,"   ",CurrentBars[1]," - ",lwb,"   ",Bars.GetTime(CurrentBars[1]-rbarLastWeek).ToString()));
			int rbar = 0;
//			if(Times[1][0]>MarketOpenTimeMinus75minutes && Times[1][0]<=this.MarketOpenTime) {
//				if((byte)(pShowResetZone) BackColor=Color.FromArgb), ChartControl.Properties.AxisPen.Brush.R,ChartControl.Properties.AxisPen.Brush.G,ChartControl.Properties.AxisPen.Brush.B);
//				Scalp.Reset();
//				SwingTrader.Reset();
//				TrendTrader.Reset();
//			} else 
			if(rbarLastWeek>0 && BarsInProgress==0) {
				double baseval = 0;
//if(Times[1][0]>ttt) Print(weekptrOffset+"   "+Times[1][rbarLastWeek].ToString()+"    "+rbarLastWeek);
				CalculatePlots(ref rbar, rbarLastWeek, ref nt, ref mt, ref lt);
				double avg = Math.Abs((nt+mt+lt)/3.0);
				if(nt>avg) Scalp[0] = (nt-avg);
				if(mt>avg) SwingTrader[0] = (mt-avg);
				if(lt>avg) TrendTrader[0] = (lt-avg);
//if(Times[1][0]>ttt) Print("   TrendTrader: "+TrendTrader[0].ToString());

line=622;
				double init_nt = Math.Abs(nt);//-avg;
				double init_mt = Math.Abs(mt);//-avg;
				double init_lt = Math.Abs(lt);//-avg;
				string plotvalStr = null;//nt.ToString()+Environment.NewLine+mt.ToString()+Environment.NewLine+lt.ToString();
				ResultsMsg[0]="N/A";
				ResultsMsg[1]=null;
line=630;
				MinutesRemaining[0] = float.MaxValue; //TrendTrader minutes remaining
				MinutesRemaining[1] = float.MaxValue; //SwingTrader minutes remaining
				MinutesRemaining[2] = float.MaxValue; //Scalp minutes remaining

line=635;
				rbar = 1;
				nt = init_nt;
				mt = init_mt;
				lt = init_lt;
//Print(String.Format("{0}{1}","t[1][0]: ",Times[1][0].ToString()));
line=645;
				int PriorSystem = CurrentSystem;
				if(nt > mt && nt > lt) {MinutesRemaining[2] = 0; CurrentSystem = 2;}//Print("   Scalp is chosen 0");}
				if(mt > nt && mt > lt) {MinutesRemaining[1] = 0; CurrentSystem = 1;}//Print("   swing is chosen 0");}
				if(lt > mt && lt > nt) {MinutesRemaining[0] = 0; CurrentSystem = 0;}//Print("   trend is chosen 0");}
line=655;
				while(MinutesRemaining[0]==float.MaxValue || MinutesRemaining[1]==float.MaxValue || MinutesRemaining[2]==float.MaxValue) {
					if(CalculatePlots(ref rbar, rbarLastWeek, ref nt, ref mt, ref lt)) break;
//if(Times[1][0].Day==10 && Times[1][0].Hour>10) Print("354:  "+rbar+":  nt: "+nt.ToString("0.00")+"   mt: "+mt.ToString("0.00")+"   lt: "+lt.ToString("0.00"));
					if(nt > mt && nt > lt && MinutesRemaining[2] == float.MaxValue) {MinutesRemaining[2] = rbar;}//Print("    Scalp is at "+rbar);}
					if(mt > lt && mt > nt && MinutesRemaining[1] == float.MaxValue) {MinutesRemaining[1] = rbar;}//Print("    swing is at "+rbar);}
					if(lt > nt && lt > mt && MinutesRemaining[0] == float.MaxValue) {MinutesRemaining[0] = rbar;}//Print("    trend is at "+rbar);}
					rbar++;
				}
line=665;

				string TrendMinutes = MinutesRemaining[0]==float.MaxValue?"N/A":MinutesRemaining[0].ToString("0-mins");
				string SwingMinutes = MinutesRemaining[1]==float.MaxValue?"N/A":MinutesRemaining[1].ToString("0-mins");
				string ScalpMinutes = MinutesRemaining[2]==float.MaxValue?"N/A":MinutesRemaining[2].ToString("0-mins");
				currentSuggestion[0] = (-999);
				nextSuggestion[0] = (-999);
line=675;
				if(MinutesRemaining[0] < MinutesRemaining[1] && MinutesRemaining[0] < MinutesRemaining[2]) {
line=677;
					ResultsMsg[0] = "Trend is suggested ";
					currentSuggestion[0] = (CurrentSystem);
					if(MinutesRemaining[1] < MinutesRemaining[2]){
						ResultsMsg[1] = "Swing is suggested in approx "+SwingMinutes;
						nextSuggestion[0] = (1);
					}else{
						ResultsMsg[1] = "Scalp is suggested in approx "+ScalpMinutes;
						nextSuggestion[0] = (2);
					}
				}
				else if(MinutesRemaining[1] < MinutesRemaining[0] && MinutesRemaining[1] < MinutesRemaining[2]) {
line=685;
					ResultsMsg[0] = "Swing is suggested ";
					currentSuggestion[0] = (CurrentSystem);
					if(MinutesRemaining[0] < MinutesRemaining[2]){
						ResultsMsg[1] = "Trend is suggested in approx "+TrendMinutes;
						nextSuggestion[0] = (0);
					}else{
						ResultsMsg[1] = "Scalp is suggested in approx "+ScalpMinutes;
						nextSuggestion[0] = (2);
					}
				}
				else {
line=695;
					ResultsMsg[0] = "Scalp is suggested ";
					currentSuggestion[0] = (CurrentSystem);
					if(MinutesRemaining[0] < MinutesRemaining[1]){
						ResultsMsg[1] = "Trend is suggested in approx "+TrendMinutes;
						nextSuggestion[0] = (0);
					}else{
						ResultsMsg[1] = "Swing is suggested in approx "+SwingMinutes;
						nextSuggestion[0] = (1);
					}
				}

line=705;
				if((State != State.Historical)&& CurrentSystem != -1 && PriorSystem != CurrentSystem) {
					if(pEnableSound && pSuggestionChangedWAV.Length>0) Alert("change",Priority.High,"TradeForecaster changed: "+ResultsMsg[0], AddSoundFolder(pSuggestionChangedWAV),1,Brushes.Yellow,Brushes.Black); 
					if(pLaunchPopup) Log(Instrument.FullName+" TradeForecaster changed: "+ResultsMsg[0], LogLevel.Alert);
					if(pSendEmails && pEmailAddress.Length>0) SendMail(pEmailAddress,"TradeForecaster on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString()+" changed: "+ResultsMsg[0], "TradeForecaster changed: "+ResultsMsg[0]+Environment.NewLine+"Message auto-generated from your NinjaTrader platform");
				}
//				Print("Current: "+currentSuggestion[0]);
//				Print("Next: "+nextSuggestion[0]);
//Print("Scalp[0]: "+Scalp[0].ToString());
//Print("Swing[0]: "+SwingTrader[0].ToString());
//Print("Trend[0]: "+TrendTrader[0].ToString());
//Print("   Trend: "+TrendMinutes);
//Print("   Swing: "+SwingMinutes);
//Print("   Scalp: "+ScalpMinutes);//Print("");
line=720;
				if(ChartControl!=null){
					if(pMsgLoc == MMv2_PredictionLocType.TopLeft)          Draw.TextFixed(this, "results",ResultsMsg[0]+Environment.NewLine+ResultsMsg[1]+Environment.NewLine+plotvalStr,TextPosition.TopLeft);
					else if(pMsgLoc == MMv2_PredictionLocType.TopRight)    Draw.TextFixed(this, "results",ResultsMsg[0]+Environment.NewLine+ResultsMsg[1]+Environment.NewLine+plotvalStr,TextPosition.TopRight);
					else if(pMsgLoc == MMv2_PredictionLocType.BottomLeft)  Draw.TextFixed(this, "results",ResultsMsg[0]+Environment.NewLine+ResultsMsg[1]+Environment.NewLine+plotvalStr,TextPosition.BottomLeft);
					else if(pMsgLoc == MMv2_PredictionLocType.BottomRight) Draw.TextFixed(this, "results",ResultsMsg[0]+Environment.NewLine+ResultsMsg[1]+Environment.NewLine+plotvalStr,TextPosition.BottomRight);
					else if(pMsgLoc == MMv2_PredictionLocType.Center)      Draw.TextFixed(this, "results",ResultsMsg[0]+Environment.NewLine+ResultsMsg[1]+Environment.NewLine+plotvalStr,TextPosition.Center);
				}
			}

}catch(Exception x){
	TimeOfError = NinjaTrader.Core.Globals.Now; 
	ErrorMsg = MakeString(new Object[]{line,":  TradeForecaster Error @ bar ",Times[1][0], " ",x}); 
	if(ErrorCount<1){
		string s = x.ToString();
		if(ChartControl!=null) {
			if(s.Contains("Memory")) {
				s= MakeString(new Object[]{"Possible memory error occurred.",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
				//Draw.TextFixed(this, "cbcerror",MakeString(new Object[]{"Possible memory error occurred.",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"}),TextPosition.Center);
			}
			else if(s.Contains("ArgumentOutOfRange")) 
				s=string.Empty;
				//Draw.TextFixed(this, "cbcerror",MakeString(new Object[]{"ArgumentOutOfRange",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"}),TextPosition.Center);
			else {
				s=MakeString(new Object[]{"TradeForecaster Error",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
				//Draw.TextFixed(this, "cbcerror",MakeString(new Object[]{"TradeForecaster Error",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"}),TextPosition.Center);
			}
			if(s.Length>0) Draw.TextFixed(this, "cbcerror",s, TextPosition.Center);
		} else {
			if(s.Contains("Memory")) {
				s = MakeString(new Object[]{"Possible memory error occurred.",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
				//PrintMakeString(new Object[]{"Possible memory error occurred.",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
			}
			else if(s.Contains("ArgumentOutOfRange")) 
				s=string.Empty;
				//PrintMakeString(new Object[]{"ArgumentOutOfRange",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
			else {
				s = MakeString(new Object[]{"TradeForecaster Error",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
				//PrintMakeString(new Object[]{"TradeForecaster Error",Environment.NewLine,"See OutputWindow for more details",Environment.NewLine,"Please close down NinjaTrader and reopen"});
			}
			if(s!=string.Empty) Print(ErrorMsg);
		}
	}
	ErrorCount++;
}

        }
//===========================================================================================================
		private bool CalculatePlots(ref int rbar, int rbarLastWeek, ref double nt, ref double mt, ref double lt) {
			#region CalculatePlots
			double sum = 0, avg=0;
line=1545;
//if(Times[1][0]>ttt) Print("rbarLastWeek: "+rbarLastWeek+"   "+Times[1][rbarLastWeek].ToString());
			for(int i = rbar; i<15+rbar; i++){
//				if(rbarLastWeek-i>=0)
					sum += Histo[rbarLastWeek-i];
//if(Times[1][0]>ttt) Print(i+":  "+Histo[rbarLastWeek-i].ToString());
			}
			nt = (sum / 15);

			sum = 0;
line=1554;
			for(int i = 15+rbar; i<45+rbar; i++){
//				if(rbarLastWeek-i>=0)
					sum += Histo[rbarLastWeek-i];
//if(Times[1][0]>ttt) Print(i+":  "+Histo[rbarLastWeek-i].ToString());
//if(Times[1][0]>ttt) Print("\t"+i+": "+Histo[rbarLastWeek-i].ToString("0.000")+"  sum: "+sum.ToString());
			}
			mt = (sum / 30);

			sum = 0;
line=1563;
//Print(string.Format("{0}{1}","1563:   Histo.Count: ",Histo.Count));
			for(int i = 30+rbar; i<90+rbar; i++){
				int r= rbarLastWeek-i;
//				if(rbarLastWeek-i>=0)
//if(r<10) Print(String.Format("{0}{1}{2}{3}{4}{5}","    rbarLastWeek: ",rbarLastWeek,"  i: ",i,"   r-i= ",(rbarLastWeek-i).ToString()));
					sum += Histo[rbarLastWeek-i];
			}
			lt = (sum / 60);

			//avg = Math.Abs((nt+mt+lt)/3.0);
			nt = Math.Abs(nt);//-avg;
			mt = Math.Abs(mt);//-avg;
			lt = Math.Abs(lt);//-avg;
			if(rbarLastWeek-rbar <= 0) {
				rbar = int.MinValue;
				return true;//true means the search is done
			}
			return false;//false means it's ok to continue the search
			#endregion
		}
//===========================================================================================================

	private double FillHisto(int relativebar, DateTime TimeThisBar, DateTime TimePriorBar){
		#region FillHisto
		bool CollectThisDaysData = false;

string infomsg = string.Empty;

		double HistoResult=0;
		double p=0;

		if((TimeThisBar.Ticks >= MarketOpenTime.Ticks && TimePriorBar.Ticks < MarketOpenTime.Ticks)){// || TimeThisBar.Ticks > MarketOpenTime.Ticks) {
			DateTime PriorMarketOpenTime = MarketOpenTime;
			MarketOpenTime = new DateTime(TimeThisBar.Year, TimeThisBar.Month, TimeThisBar.Day, MarketOpenTime.Hour, MarketOpenTime.Minute, 0,0);
			TodayOpen = Closes[1][relativebar];
			MarketOpenTimeMinus75minutes = MarketOpenTime.AddMinutes(-75).AddDays(1);
			//LogFileInfo.Add(string.Concat(TimeThisBar.ToString(),"  MarketOpen at: ",MarketOpenTime.ToString(),"  Price: "+TodayOpen,Environment.NewLine));
		}

		while(TimeThisBar.Ticks>MarketOpenTime.Ticks) MarketOpenTime = MarketOpenTime.AddDays(1);

		if(TodayOpen < 0) return(0);
		ThisTimeLastWeek = DateTime.MinValue;

		p = (Median[relativebar]-TodayOpen)/TickSize;

		TradeForecasterCore[Times[1][relativebar]] = p;
		if(EarliestDay == DateTime.MinValue) EarliestDay = Times[1][0];
		if(relativebar<Bars.Count-3 && Times[1][relativebar].Day != Times[1][relativebar+1].Day) {
			DateTime CutOffDate = Times[1][relativebar+1].AddDays(-7*(pMaxWeeks*2));
			List<DateTime> keys = new List<DateTime>(TradeForecasterCore.Keys);
			foreach(DateTime ct in keys) {
				if(ct<CutOffDate) TradeForecasterCore.Remove(ct);
			}
		}

//Print(Times[1][0].ToString(),"  TradeForecasterCore: ",p.ToString());
		int weekptr = pWeeksToGoBack;
		int averager = 1;
		double CoreTotal = p;
		int priorbarabs = CurrentBars[1];
		DateTime t;

		int BarOfLastNotice = -1;
		bool done = false;
		while(averager < pMaxWeeks && priorbarabs>0) {
			t = TimeThisBar.AddDays(-7*weekptr);
//if(Times[1][relativebar]>ttt) Print(string.Concat(" time(-7*",weekptr,"):  ",t.ToString(),"   averager: ",averager));
			priorbarabs = Bars.GetBar(t);
			if(priorbarabs<=0) break;
			int priorbarR = CurrentBars[1]-priorbarabs;
			if(TradeForecasterCore.ContainsKey(t)) {
//if(Times[1][relativebar]>ttt) Print(string.Concat("\tTradeForecasterCore retrieved: ",TradeForecasterCore[t]));
				if(ThisTimeLastWeek == DateTime.MinValue) ThisTimeLastWeek = t;
//coretotals = string.Concat(coretotals,TradeForecasterCore[t]," + ");
				CoreTotal = CoreTotal + TradeForecasterCore[t];
				if(Debug) infomsg = MakeString(new Object[]{infomsg,",",TradeForecasterCore[t],",",t});
				averager++;
			}
			else if(priorbarabs>2) {
				int priorbarR2;
				if(Times[1][priorbarR] < t) {
					priorbarR2 = priorbarR;
					priorbarR--;
				}
				else priorbarR2 = priorbarR+1;
				TimeSpan ts = new TimeSpan(Math.Abs(Times[1][priorbarR2].Ticks - Times[1][priorbarR].Ticks));
				if(ThisTimeLastWeek == DateTime.MinValue) ThisTimeLastWeek = Times[1][priorbarR2];

				if(ts.TotalMinutes<=5) {
					double TradeForecasterCoreR2 = double.MinValue;
					double TradeForecasterCoreR  = double.MinValue;
					if(!TradeForecasterCore.TryGetValue(Times[1][priorbarR2],out TradeForecasterCoreR2)) TradeForecasterCoreR2 = 0;
					if(!TradeForecasterCore.TryGetValue(Times[1][priorbarR],out TradeForecasterCoreR))   TradeForecasterCoreR = 0;
					double r = (TradeForecasterCoreR + TradeForecasterCoreR2) / 2.0;
//coretotals = string.Concat(coretotals,r," + ");
//if(Times[1][relativebar]>ttt) Print(string.Concat("\tSynthesized(",ts.TotalMinutes,") TradeForecasterCore: ",r));
					CoreTotal = CoreTotal + r;
					averager++;
				} else {
					//Print(Times[1][relativebar].ToString()+"  Discrepancy in times:  Requested date: "+Times[1][priorbarR2].ToString()+"   "+Times[1][priorbarR].ToString()+"  ts.mins: "+ts.TotalMinutes);
//if(Times[1][relativebar]>ttt) Print(string.Concat("\tWk:",weekptr,"  ",t.ToString(),"  ",Times[1][priorbarR2].ToString(),"-",Times[1][priorbarR].ToString(),"  Skipping this datapoint, ts=",ts.TotalMinutes));
					
				}
			}
			if(weekptr<= pMaxWeeks && weekptr > MaxWeeksAccessible && BarOfLastNotice!=CurrentBars[1]) {
				MaxWeeksAccessible = weekptr;//-pWeeksToGoBack;
				BarOfLastNotice = CurrentBars[1];
				double price = SMA(50)[0];
				if(Closes[1][0]<price) price = price * 1.005;
				else price = price * 0.995;
//				Draw.Text(this, MakeString(new Object[]{"WA",BarOfLastNotice}), false, MakeString(new Object[]{"TradeForecaster now has ",MaxWeeksAccessible,(MaxWeeksAccessible==1?" week":" weeks")," of accessible data as of",Times[1][0],"  ."}), 2, price, 0, Color.Red, textFontSmall, TextAlignment.Far, Color.Red, Color.White, 9);
			}
			TimeSpan weekscount = new TimeSpan(Times[1][0].Ticks - EarliestDay.Ticks);
			if(weekscount.Days / 7.0 > pMaxWeeks) MaxWeeksAccessible = (int)Math.Truncate(weekscount.Days/7.0);
			weekptr++;
		}
		weekptrOffset = weekptr-2;
		if(averager>0) HistoResult = CoreTotal / averager;
		else HistoResult = CoreTotal;

//if(Times[1][relativebar]>ttt) Print(string.Concat("\tHistoResult: ",HistoResult));
		#endregion
		return (HistoResult);
	}
	
	private SharpDX.Direct2D1.Brush BlackDX = null;
	public override void OnRenderTargetChanged()
	{
		#region Brush disposal
		if(BlackDX !=null      && !BlackDX.IsDisposed)      BlackDX.Dispose();      BlackDX   = null;
		#endregion

		if(RenderTarget!=null){
			#region Brush init
			BlackDX         = Brushes.Black.ToDxBrush(RenderTarget);
			#endregion
		}
	}
//====================================================================
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
		Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
		Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
		int firstBarPainted = ChartBars.FromIndex;
		int lastBarPainted = ChartBars.ToIndex;
//Print("BIP: "+BarsInProgress);
		//if(BarsInProgress!=0) return;
		if(ChartControl==null) return;
		var txtLayoutScalp = new TextLayout(Core.Globals.DirectWriteFactory, ScalpBarLabel, txtFormat_LabelFont, ChartPanel.W, txtFormat_LabelFont.FontSize);
		var txtLayoutSwing = new TextLayout(Core.Globals.DirectWriteFactory, SwingTraderBarLabel, txtFormat_LabelFont, ChartPanel.W, txtFormat_LabelFont.FontSize);
		var txtLayoutTrend = new TextLayout(Core.Globals.DirectWriteFactory, TrendTraderBarLabel, txtFormat_LabelFont, ChartPanel.W, txtFormat_LabelFont.FontSize);
try{
//			if(TempMsgs!=null) {
//				TempMsgs.ExpireAndUpdate(NinjaTrader.Core.Globals.Now);
//				if(DataShortageMsg.Length>0) MessageFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial",14); else MessageFont = ChartControl.Properties.LabelFont;
//				TempMsgs.PrintGraphics(ref graphics, MessageFont, ref bounds);
//			}
		if(DataShortageMsg.Length>0) {
			Print("DataShortageMsg: "+DataShortageMsg+"....   exiting OnRender");
			return;
		}

//Print("TradeForecaster panel: "+this.Panel); //0=price panel, 1=first subpanel
		if(this.Panel>0){
			base.OnRender(chartControl, chartScale);
		}
			MinBarWidth = 3+Math.Max(txtLayoutScalp.Metrics.Width,Math.Max(txtLayoutSwing.Metrics.Width,txtLayoutTrend.Metrics.Width));
			MaxBarWidth = Math.Max((float)ChartPanel.W/6f,MinBarWidth * 5f);
			BarGroupHeight = (txtLayoutScalp.Metrics.Height + 2) * 3;
			float x = ChartPanel.X + 10;
			float y = 0;
			if(this.Panel>0) y = ChartPanel.Y + (float)ChartPanel.H/2.0f - BarGroupHeight / 2.0f + pHistoX;
			else y = txtLayoutScalp.Metrics.Height+ ChartPanel.Y + 40 + pHistoX;
//Print("Trend: "+MinutesRemaining[0]);
//Print("Swing: "+MinutesRemaining[1]);
//Print("Scalp: "+MinutesRemaining[2]);Print("");
			SharpDX.RectangleF rectangleF;// = new SharpDX.RectangleF();//x, y, txtLayoutTrend.Metrics.Width+2f, txtLayoutTrend.Metrics.Height+2f);
			if(MinutesRemaining[0] == 0) {
				rectangleF = new SharpDX.RectangleF(x, y, MaxBarWidth, txtLayoutTrend.Metrics.Height+2f);
			}else if(MinutesRemaining[0] >= MinutesRemaining[1] && MinutesRemaining[0] >= MinutesRemaining[2]){
				rectangleF = new SharpDX.RectangleF(x, y, MinBarWidth, txtLayoutTrend.Metrics.Height+2f);
			}else {
				float minuterange = Math.Abs(MinutesRemaining[1]-MinutesRemaining[2]);
//Print("Minute range: "+minuterange);
				float ratio = 1-MinutesRemaining[0] / minuterange;
//Print("ratio: "+ratio);
				rectangleF = new SharpDX.RectangleF(x, y, MaxBarWidth*ratio, txtLayoutTrend.Metrics.Height+2f);
			}
			RenderTarget.FillRectangle(rectangleF, Plots[2].BrushDX);
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y+1), txtLayoutTrend, BlackDX);

			y = y + txtLayoutSwing.Metrics.Height+3;
			if(MinutesRemaining[1] == 0) 
				rectangleF = new SharpDX.RectangleF(x, y, MaxBarWidth, txtLayoutSwing.Metrics.Height+2f);
			else if(MinutesRemaining[1] >= MinutesRemaining[0] && MinutesRemaining[1] >= MinutesRemaining[2])
				rectangleF = new SharpDX.RectangleF(x, y, MinBarWidth, txtLayoutSwing.Metrics.Height+2f);
			else {
				float minuterange = Math.Abs(MinutesRemaining[0]-MinutesRemaining[2]);
				float ratio = 1-MinutesRemaining[1] / minuterange;
				rectangleF = new SharpDX.RectangleF(x, y, MaxBarWidth*ratio, txtLayoutSwing.Metrics.Height+2f);
			}
			RenderTarget.FillRectangle(rectangleF, Plots[1].BrushDX);
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y+1), txtLayoutSwing, BlackDX);

			y = y + txtLayoutSwing.Metrics.Height+3;
			if(MinutesRemaining[2] == 0) 
				rectangleF = new SharpDX.RectangleF(x, y, MaxBarWidth, txtLayoutScalp.Metrics.Height+2f);
			else if(MinutesRemaining[2] >= MinutesRemaining[0] && MinutesRemaining[2] >= MinutesRemaining[1])
				rectangleF = new SharpDX.RectangleF(x, y, MinBarWidth, txtLayoutScalp.Metrics.Height+2f);
			else {
				float minuterange = Math.Abs(MinutesRemaining[0]-MinutesRemaining[1]);
				float ratio = 1-MinutesRemaining[2] / minuterange;
				rectangleF = new SharpDX.RectangleF(x, y, MaxBarWidth*ratio, txtLayoutSwing.Metrics.Height+2f);
			}
			RenderTarget.FillRectangle(rectangleF, Plots[0].BrushDX);
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y+1), txtLayoutScalp, BlackDX);
}catch(Exception err) {Draw.TextFixed(this, "errorerror","Please take screenshot and send to support@indicatorwarehouse.com"+Environment.NewLine+err.ToString(),TextPosition.Center);}
			if(txtLayoutScalp!=null){
				txtLayoutScalp.Dispose();
				txtLayoutScalp = null;
			}
			if(txtLayoutSwing!=null){
				txtLayoutSwing.Dispose();
				txtLayoutSwing = null;
			}
			if(txtLayoutTrend!=null){
				txtLayoutTrend.Dispose();
				txtLayoutTrend = null;
			}

		}

//===========================================================================================================
		public override string ToString()
		{
			string n=string.Empty;
			return "iw TradeForecaster("+this.MarketOpen_Time+")";
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
				string folder = System.IO.Path.Combine(Core.Globals.InstallDir,"sounds");
				string search = "*.wav";
				System.IO.DirectoryInfo dirCustom = new System.IO.DirectoryInfo(folder);
				string[] filteredlist = new string[1];
				if(!dirCustom.Exists) {
					filteredlist[0]= "unavailable";
					return new StandardValuesCollection(filteredlist);;
				}
				System.IO.FileInfo[] filCustom = dirCustom.GetFiles(search);

				string[] list = new string[filCustom.Length];
				int i = 0;
				foreach (System.IO.FileInfo fi in filCustom)
				{
					list[i] = fi.Name;
					i++;
				}
				filteredlist = new string[i];
				for(i = 0; i<filteredlist.Length; i++) filteredlist[i] = list[i];
				return new StandardValuesCollection(filteredlist);
			}
			#endregion
		}

		#region Plots
//		private const int SignalPlotId = 0;
//		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//		public Series<double> Signal
//		{
//			get { return Values[SignalPlotId]; }
//		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Scalp
		{
			get { return Values[0]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SwingTrader
		{
			get { return Values[1]; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> TrendTrader
		{
			get { return Values[2]; }
		}

//		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//		public Series<double> UL
//		{
//			get { return Values[4]; }
//		}
//		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//		public Series<double> LL
//		{
//			get { return Values[5]; }
//		}

		#endregion
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentSuggestion //2==Scalp, 1==Swing, 0==Trend
		{
			get { 
				Update();
				return currentSuggestion; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> NextSuggestion //2==Scalp, 1==Swing, 0==Trend
		{
			get { 
				Update();
				return nextSuggestion; }
		}
		#region Properties
		private bool pLaunchPopup = false;
		[Description("Launch a Popup window when the TradeForecaster suggestion changes?")]
		[Category("Alert")]
		public bool LaunchPopup
		{
			get { return pLaunchPopup; }
			set { pLaunchPopup = value; }
		}

		private bool pSendEmails = false;
		[Description("Send an email whenever the TradeForecaster suggestion changes?")]
		[Category("Alert")]
		public bool SendEmails
		{
			get { return pSendEmails; }
			set { pSendEmails = value; }
		}
		private string pEmailAddress = "";
		[Description("Supply your address (e.g. 'you@mail.com') for receiving change notices")]
		[Category("Alert")]
		public string EmailAddress
		{
			get { return pEmailAddress; }
			set { pEmailAddress = value; }
		}
		private string pSuggestionChangedWAV = "Alert2.wav";
		[Description("WAV file for when a new system is suggested")]
		[Category("Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SuggestionChangedWAV
		{
			get { return pSuggestionChangedWAV; }
			set { pSuggestionChangedWAV = value; }
		}
		private bool pEnableSound = true;
		[Description("Enable alert sound when TradeForecaster suggestion changes?")]
		[Category("Alert")]
		public bool EnableSound
		{
			get { return pEnableSound; }
			set { pEnableSound = value; }
		}

		private SimpleFont pLabelFont = new SimpleFont("Arial",14);
		[Category("Visual")]
		public SimpleFont LabelFont
		{
			get { return pLabelFont; }
			set { pLabelFont = value; }
		}
		private MMv2_PredictionLocType pMsgLoc = MMv2_PredictionLocType.TopLeft;
		[Description("Location of count-down messages")]
		[Category("Visual")]
		public MMv2_PredictionLocType MsgLocation
		{
			get { return pMsgLoc; }
			set { pMsgLoc = value; }
		}
		private int pHistoX = 50;
		[Description("Vertical X Location of colored histos")]
		[Category("Visual")]
		public int HistoX
		{
			get { return pHistoX; }
			set { pHistoX = value; }
		}

		[Description("Market open time")]
		[Category("Parameters")]
		public string MarketOpen_Time
		{
			get { return pMarketOpenTimeStr; }
			set {
				pMarketOpenTimeStr = value;
				try {
					MarketOpenTime = DateTime.Parse(pMarketOpenTimeStr);} 
				catch {
					Log("Invalid MarketOpenTime time, must be in 24hr format: 'hh:mm'",LogLevel.Alert);
					MarketOpenTime = new DateTime(2024,1,1,8,30,0,0);
					pMarketOpenTimeStr = "8:30";}
				 }
		}
		private bool pShowResetZone = false;
//		[Description("The ResetZone is the 1:15 prior to market open.  No signals are available during this time")]
//		[Category("Parameters")]
//		public bool ShowResetZone
//		{
//			get { return pShowResetZone; }
//			set { pShowResetZone = value; }
//		}
		private int pMethodNumber = 0;
//		[Description("")]
//		[Category("Parameters")]
//		public int MethodNumber
//		{
//			get { return pMethodNumber; }
//			set { pMethodNumber = Math.Max(0,Math.Min(2,value)); }
//		}


		private int pMaxWeeks = 3;

		#endregion
    }
//==================================================================

}

	public enum MMv2_PredictionLocType {
		TopRight,
		TopLeft,
		BottomRight,
		BottomLeft,
		Center,
		None
	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeForecaster[] cacheTradeForecaster;
		public TradeForecaster TradeForecaster()
		{
			return TradeForecaster(Input);
		}

		public TradeForecaster TradeForecaster(ISeries<double> input)
		{
			if (cacheTradeForecaster != null)
				for (int idx = 0; idx < cacheTradeForecaster.Length; idx++)
					if (cacheTradeForecaster[idx] != null &&  cacheTradeForecaster[idx].EqualsInput(input))
						return cacheTradeForecaster[idx];
			return CacheIndicator<TradeForecaster>(new TradeForecaster(), input, ref cacheTradeForecaster);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeForecaster TradeForecaster()
		{
			return indicator.TradeForecaster(Input);
		}

		public Indicators.TradeForecaster TradeForecaster(ISeries<double> input )
		{
			return indicator.TradeForecaster(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeForecaster TradeForecaster()
		{
			return indicator.TradeForecaster(Input);
		}

		public Indicators.TradeForecaster TradeForecaster(ISeries<double> input )
		{
			return indicator.TradeForecaster(input);
		}
	}
}

#endregion
