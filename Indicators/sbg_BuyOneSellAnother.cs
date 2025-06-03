#define DO_LICENSE

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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion
using System.Threading;


//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum BuyOneSellAnother_AutoState {Add, Close, Off, BuyOnly, SellOnly}
    [CategoryOrder("Parameters", 10)]
	[CategoryOrder("Audible Alerts",20)]
	[CategoryOrder("AutoTrade",30)]
	[CategoryOrder("Bkg Visuals",40)]
    [CategoryOrder("MovingAverages", 50)]
    [CategoryOrder("Trend Signal", 60)]
    [CategoryOrder("~ License ~", 1000)]
	public class BuyOneSellAnother : Indicator//, ICustomTypeDescriptor
	{
		private const int PLUS_TICK_BIP = 2;
		private const int MINUS_TICK_BIP = 3;
//		private SMA sma;
		private System.Windows.Controls.Grid MenuGrid;
		private System.Windows.Controls.Button btn_GoFlat, btn_BuySpread, btn_BuyLongInst, btn_SellLongInst, btn_BuyShortInst, btn_SellShortInst, btn_HideExtraButtons, btn_AccountName, btn_DrawAutoTradeLines;
		private System.Windows.Controls.Button btn_SellSpread, btn_ShowReversalDots, btn_ShowSpreadHistory, btn_ShowSessionTPO;
		private System.Windows.Controls.Button btn_AutoTrade;
		private string  OIF_file_path  = "";
		private int     OIFnumber      = 0;
		private int     SelectedSpreadQty = 1;
		private bool    TestSend       = false;//will not submit OIF to the "incoming" directory, instead, the OIF file will be written to MyDocuments
		private NinjaTrader.Cbi.Account myAccount;
		private bool    AutoTradeEnabled = false;
		private bool    IsShiftPressed   = false;
		private bool    IsCtrlPressed    = false;
		private bool    ShowExtraBtns    = false;
		private string  EquityRatioString = string.Empty;
		private int line = 0;
		private string AccountName = "";
		private BuyOneSellAnother_AutoState AutoState = BuyOneSellAnother_AutoState.Off;
		private int CurrentSpreadPosition = 0;
		private int CurrentLongSize = -1;
		private int CurrentShortSize = -1;
		private double CurrentPositionValue = 0;
		private double CurrentExitValue = 0;
		private string CurrentLongInst = "";
		private string CurrentShortInst = "";
		private double HLine_BuyValue = double.NaN;
		private double HLine_SellValue = double.NaN;
		private string StatusMsg = "Ready for manual entry";
		private string BuyHLineEntryType = " "; //T for touch entry, S for stop entry
		private string SellHLineEntryType = " "; //T for touch entry, S for stop entry
//		private Brush[] OriginalPlotColors = null;
		private double PlusAsk = 0;
		private double PlusBid = 0;
		private double PlusCurrent = 0;
		private double MinusAsk = 0;
		private double MinusBid = 0;
		private double MinusCurrent = 0;
		private string SettingStr_Calculate = string.Empty;
		private int MA1FirstABar = int.MaxValue;
		private int MA2FirstABar = int.MaxValue;
		private string uID =null;
		private DateTime TimeOfAutoEnabled = DateTime.MinValue;
//		private string SourceOfTrade = string.Empty;
		private int BarTickCount  = 1000000;
		private bool ValidLicense = false;
		private Chart chartWindow = null;
		private DateTime LaunchedAt = DateTime.MaxValue;
		private string btn_BuySpread_Name        = null;
		private string btn_SellSpread_Name       = null;
		private string btn_AutoTrade_Name        = null;
		private string btn_HideExtraButtons_Name = null;
		private string btn_DrawAutoTradeLines_Name  = null;
		private string btn_AccountName_Name         = null;
		private string btn_ShowReversalDots_Name    = null;
		private string btn_ShowSpreadHistory_Name = null;
		private string btn_ShowSessionTPO_Name = null;
		private string btn_GoFlat_Name           = null;
		private string btn_BuyLongInst_Name      = null;
		private string btn_BuyShortInst_Name     = null;
		private string btn_SellLongInst_Name     = null;
		private string btn_SellShortInst_Name    = null;
		private double EstimatedBuyPrice		 = 0;
		private double EstimatedSellPrice		 = 0;
		private double PLUS_POINT_VALUE			 = 0;
		private double MINUS_POINT_VALUE		 = 0;

		private string SupportEmailAddress = "bosa@sbgtradingcorp.com";
		private string ProductName = "BOSA";
		private double ProductID_Regular = 29;//unique product number for Regular version (no auto trading)
		private double ProductID_Pro     = 73;//unique product number for Pro version (no auto trading)
		private bool IsExpired           = true;
		private DateTime ExpirationDT = DateTime.MaxValue;

		private bool AreSpreadsBalanced = true;
		private bool IsDebug = false;
		private string MachineId = NinjaTrader.Cbi.License.MachineId;
		private bool TerminalError = false;
		private bool IsCalendarSpread = false;//same symbol, different months
//		private List<double> List_RecentSpreads = new List<double>();
		private class ButtonData{
			#region -- Button Data --
			public System.Windows.Controls.Button btn;
			public Brush BkgColor = Brushes.Red;
			public Brush ForeColor = Brushes.Gray;
			public bool Enabled = false;
			public string Txt = "";
			public Visibility Vis = Visibility.Visible;
//			public bool IsUpdated = false;
			public ButtonData(System.Windows.Controls.Button b){
				BkgColor = b.Background;
				ForeColor = b.Foreground;
				Enabled = b.IsEnabled;
				Txt = b.Content.ToString();
				Vis = b.Visibility;
//				IsUpdated = true;
			}
			public void Update(ChartControl ct, System.Windows.Controls.Button btn, ref bool refresh){//, dynamic parent){
//				if(IsUpdated) {
//NinjaTrader.Code.Output.Process("No updating needed for "+btn.Name, PrintTo.OutputTab1);
//					return;
//				}
				if (ct.Dispatcher.CheckAccess()) {
int line = 100;
try{
					if(btn.Foreground != ForeColor) {
						btn.Foreground = ForeColor;
						refresh = true;
//NinjaTrader.Code.Output.Process("   Updated ForeColor of "+btn.Name, PrintTo.OutputTab1);
					}
					if(btn.Background != BkgColor) {
						btn.Background = BkgColor;
						refresh = true;
//NinjaTrader.Code.Output.Process("   Updated BkgColor of "+btn.Name, PrintTo.OutputTab1);
					}
line=106;
					if(btn.IsEnabled != Enabled) {
						btn.IsEnabled = Enabled;
						refresh = true;
//NinjaTrader.Code.Output.Process("   Updated IsEnabled of "+btn.Name, PrintTo.OutputTab1);
					}
line=112;
					if(btn.Content.ToString().CompareTo(Txt)!=0) {
						btn.Content = (object)Txt; 
						refresh = true;
//NinjaTrader.Code.Output.Process("   Updated Content of "+btn.Name+" to "+btn.Content.ToString(), PrintTo.OutputTab1);
					}
line=117;
					if(btn.Visibility != Vis){
						btn.Visibility = Vis;
						refresh = true;
//NinjaTrader.Code.Output.Process("161   Updated Visibility of "+btn.Name+" to "+btn.Visibility.ToString(), PrintTo.OutputTab1);
					}
//					this.IsUpdated = true;
}catch(Exception e){NinjaTrader.Code.Output.Process(line+": error: "+e.ToString()+"\non "+btn.Name, PrintTo.OutputTab1);}
				}else{
					ct.Dispatcher.InvokeAsync((Action)(() =>
	   		        {
int line = 127;
try{
						if(btn.Foreground != ForeColor) {
							btn.Foreground = ForeColor;
//NinjaTrader.Code.Output.Process("   Updated ForeColor of "+btn.Name, PrintTo.OutputTab1);
						}
						if(btn.Background != BkgColor){
							btn.Background = BkgColor;
//NinjaTrader.Code.Output.Process("   Updated Background of "+btn.Name, PrintTo.OutputTab1);
						}
line=133;
						if(btn.IsEnabled != Enabled){
							btn.IsEnabled = Enabled;
//NinjaTrader.Code.Output.Process("   Updated IsEnabled of "+btn.Name, PrintTo.OutputTab1);
						}
line=139;
						if(btn.Content.ToString().CompareTo(Txt)!=0){
							btn.Content = (object)Txt;
//NinjaTrader.Code.Output.Process("   Updated Content of "+btn.Name+" to "+btn.Content.ToString(), PrintTo.OutputTab1);
						}
line=144;
						if(btn.Visibility != Vis){
							btn.Visibility = Vis;
//NinjaTrader.Code.Output.Process("188   Updated Visibility of "+btn.Name+" to "+btn.Visibility.ToString(), PrintTo.OutputTab1);
						}
//						this.IsUpdated = true;
}catch(Exception e){NinjaTrader.Code.Output.Process(line+": error: "+e.ToString()+"\non "+btn.Name, PrintTo.OutputTab1);}
					}));
				}
			}
			#endregion
		}
		private SortedDictionary<string,ButtonData> ButtonSettings = new SortedDictionary<string, ButtonData>();

		private SortedDictionary<DateTime,double[]> CorrelationDict = new SortedDictionary<DateTime,double[]>();
		private SortedDictionary<int,double[]> cci = new SortedDictionary<int,double[]>();
		private double OverallMaxSpreadVal = double.MinValue;
		private double OverallMinSpreadVal = double.MaxValue;
		private SortedDictionary<int,Tuple<double,double>> OverallHL = new SortedDictionary<int,Tuple<double,double>>();
		private SortedDictionary<int,char> FastMATrendSignals = new SortedDictionary<int,char>();

		private void UpdateUI(){
			if(!SelectedTab.Contains(uID)) return;
			bool refresh = false;
			if (ChartControl.Dispatcher.CheckAccess()) {
				ButtonSettings[btn_BuySpread_Name         ].Update(ChartControl, btn_BuySpread, ref refresh);
				ButtonSettings[btn_SellSpread_Name        ].Update(ChartControl, btn_SellSpread, ref refresh);
				ButtonSettings[btn_BuyLongInst_Name       ].Update(ChartControl, btn_BuyLongInst, ref refresh);
				ButtonSettings[btn_SellLongInst_Name      ].Update(ChartControl, btn_SellLongInst, ref refresh);
				ButtonSettings[btn_BuyShortInst_Name      ].Update(ChartControl, btn_BuyShortInst, ref refresh);
				ButtonSettings[btn_SellShortInst_Name     ].Update(ChartControl, btn_SellShortInst, ref refresh);
				ButtonSettings[btn_HideExtraButtons_Name  ].Update(ChartControl, btn_HideExtraButtons, ref refresh);
				ButtonSettings[btn_DrawAutoTradeLines_Name].Update(ChartControl, btn_DrawAutoTradeLines, ref refresh);
				ButtonSettings[btn_ShowReversalDots_Name  ].Update(ChartControl, btn_ShowReversalDots, ref refresh);
				ButtonSettings[btn_ShowSpreadHistory_Name ].Update(ChartControl, btn_ShowSpreadHistory, ref refresh);
				ButtonSettings[btn_ShowSessionTPO_Name    ].Update(ChartControl, btn_ShowSessionTPO, ref refresh);
				ButtonSettings[btn_AccountName_Name       ].Update(ChartControl, btn_AccountName, ref refresh);
				ButtonSettings[btn_AutoTrade_Name         ].Update(ChartControl, btn_AutoTrade, ref refresh);
				MenuGrid.InvalidateVisual();
			}else{
				ChartControl.Dispatcher.InvokeAsync((Action)(() =>
		        {
//Print("229  Show extra buttons? "+ShowExtraBtns.ToString());
					ButtonSettings[btn_BuySpread_Name         ].Update(ChartControl, btn_BuySpread, ref refresh);
					ButtonSettings[btn_SellSpread_Name        ].Update(ChartControl, btn_SellSpread, ref refresh);
					ButtonSettings[btn_GoFlat_Name            ].Update(ChartControl, btn_GoFlat, ref refresh);
					ButtonSettings[btn_BuyLongInst_Name       ].Update(ChartControl, btn_BuyLongInst, ref refresh);
					ButtonSettings[btn_SellLongInst_Name      ].Update(ChartControl, btn_SellLongInst, ref refresh);
					ButtonSettings[btn_BuyShortInst_Name      ].Update(ChartControl, btn_BuyShortInst, ref refresh);
					ButtonSettings[btn_SellShortInst_Name     ].Update(ChartControl, btn_SellShortInst, ref refresh);
					ButtonSettings[btn_HideExtraButtons_Name  ].Update(ChartControl, btn_HideExtraButtons, ref refresh);
					ButtonSettings[btn_DrawAutoTradeLines_Name].Update(ChartControl, btn_DrawAutoTradeLines, ref refresh);
					ButtonSettings[btn_ShowReversalDots_Name  ].Update(ChartControl, btn_ShowReversalDots, ref refresh);
					ButtonSettings[btn_ShowSpreadHistory_Name ].Update(ChartControl, btn_ShowSpreadHistory, ref refresh);
					ButtonSettings[btn_ShowSessionTPO_Name    ].Update(ChartControl, btn_ShowSessionTPO, ref refresh);
					ButtonSettings[btn_AccountName_Name       ].Update(ChartControl, btn_AccountName, ref refresh);
					ButtonSettings[btn_AutoTrade_Name         ].Update(ChartControl, btn_AutoTrade, ref refresh);
					MenuGrid.InvalidateVisual();
				}));
			}
		}

//		private ChartTab TabMe;
		protected override void OnStateChange()
		{
			//20-jul-22:  Changed StatusMsg to include SelectedSpreadQty multiplied by the GridSize
			//23-jul-22:  MA1dots and MA2dots bug - colors were not set correctly
			//23-jul-22:  Added custom color properties for MA1dots (up and down)
			//29-jul-22:  replaced base.Plot() with all custom
			//30-jul-22:  Fixed calculation of SpreadRange, added a 3rd element in the array, this is the Spread
			//30-jul-22:  Changed the channel bands to be based on 30-minute segments of history.  Much more dynamic...instead of averaging all timeperiods into one, keep them separate for differentiation
			//11-aug-22:  Added "Go Flat" button
			//24-aug-22:  Overall the SpreadRange dict...changed the double[3] to a class that contains a list of each tick, and calculates high and low based on that list.  The array structure was not allocating correctly and data from prior bar was carrying over into the new structure
			#region -- OnStateChange --
if(State!=null)printDebug("*******************************     BOSA State: "+State.ToString()); else printDebug("*******************************  BOSA State is null");
try{
			if (State == State.SetDefaults)
			{
				#region -- SetDefaults --
				Description						= @"";
				Name							= "BOSA (18-Dec-23)";
				Calculate						= Calculate.OnPriceChange;
				IsOverlay						= false;
				DisplayInDataBox				= true;
				DrawOnPricePanel				= false;
				DrawHorizontalGridLines			= true;
				DrawVerticalGridLines			= true;
				PaintPriceMarkers				= true;
				ScaleJustification				= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				MaximumBarsLookBack				= MaximumBarsLookBack.Infinite;
				IsSuspendedWhileInactive		= false;
				pLongContracts					= 1;
				pShortContracts					= 1;
				pShortSymbol      = "ES 06-24";
				pMaxSpreadsQty 	  = 5;
				pIsEquitySpread   = true;
				pSMAperiod		  = 200;
				pChannelResetTime = 0;
				pStartTime   = 800;
				pStopTime    = 1600;
				pExitTime    = 1600;
				pCorrelationTimeMinutes = 10;
				pGridlineSpacing       = 200;
				pAccountName           = "Sim101";
				pGridlineBrush         = Brushes.Yellow;
				pGridlineOpacity       = 0.05f;
				pProfitTargetDollars   = 0;
				pChannelReductionPct   = 0.2;
				pOuterBandsMult		   = 0.33;
				pAutoTradeOnBarClose   = false;
				pATBuyLineColor        = Brushes.Blue;
				pATSellLineColor       = Brushes.Pink;
				pATBuyLineThickness    = 3;
				pATSellLineThickness   = 3;
				pMA1period    = 50;
				pMA1LineColor = Brushes.Pink;
				pMA1LineWidth = 1f;
				pMA1UpDots    = Brushes.Lime;
				pMA1DnDots    = Brushes.Red;
				pFastMASignalDelayInBars = 0;
				pTrendSignalUpDotsBrush  = Brushes.Cyan;
				pTrendSignalDnDotsBrush  = Brushes.Sienna;
				pTrendSignalDotSize      = 6f;

				pMA2period = 0;
				pMA1type   = BuyOneSellAnother_MAtype.EMA;
				pMA2type   = BuyOneSellAnother_MAtype.EMA;
				pMA2LineColor = Brushes.Blue;
				pMA2LineWidth = 1f;

//				pUseChartTraderAccount = false;
				pShowReversalDots   = true;
				pShowSpreadHistory  = true;
				pShowSessionTPO     = true;
				pReversalDotsPeriod = 7;
				IsProVersion        = false;
//				pOutFileName = "";

				AddPlot(new Stroke(Brushes.Yellow,2),PlotStyle.Line,"Mid");
				AddPlot(new Stroke(Brushes.Red,2),   PlotStyle.Dot, "DU");
				AddPlot(new Stroke(Brushes.Green,2), PlotStyle.Dot, "DL");
				AddPlot(Brushes.Maroon, "SLU");
				AddPlot(Brushes.Maroon, "SLD");
				AddPlot(new Stroke(Brushes.Cyan,1), PlotStyle.Hash, "Spread");
				AddPlot(Brushes.DeepPink, "MA1");
				AddPlot(Brushes.OrangeRed, "MA2");
				AddPlot(new Stroke(Brushes.DodgerBlue,2), PlotStyle.Line, "HiLoBarTop");
				AddPlot(new Stroke(Brushes.DodgerBlue,2), PlotStyle.Line, "HiLoBarBot");
				#endregion
			}
			else if (State == State.Configure)
			{
				#region -- Configure --
				bool filefound = System.IO.File.Exists(@"c:\222222222222.txt");
				IsDebug = filefound && (MachineId.CompareTo("CB15E08BE30BC80628CFF6010471FA2A")==0 || MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
//GetChartId('T');
				AddDataSeries(pShortSymbol.ToUpper(), BarsArray[0].BarsPeriod.BarsPeriodType, BarsArray[0].BarsPeriod.BaseBarsPeriodValue, Data.MarketDataType.Last);
				int ticks = 1;
				if(pShortSymbol.ToUpper().StartsWith("NQ ") || pShortSymbol.ToUpper().StartsWith("ES ")) ticks = 5;
				if(pShortSymbol.ToUpper().StartsWith("YM ") || pShortSymbol.ToUpper().StartsWith("RTY ")) ticks = 5;
				AddDataSeries(BarsPeriodType.Tick, ticks);
				AddDataSeries(pShortSymbol.ToUpper(), BarsPeriodType.Tick, ticks);

				Plots[0].Name = (pSMAperiod>0 ? "Avg":"Mid");
				ChannelRanges[0]   = new List<double>();
				ChannelRanges[30]  = new List<double>();
				ChannelRanges[100] = new List<double>();
				ChannelRanges[130] = new List<double>();
				ChannelRanges[200] = new List<double>();
				ChannelRanges[230] = new List<double>();
				ChannelRanges[300] = new List<double>();
				ChannelRanges[330] = new List<double>();
				ChannelRanges[400] = new List<double>();
				ChannelRanges[430] = new List<double>();
				ChannelRanges[500] = new List<double>();
				ChannelRanges[530] = new List<double>();
				ChannelRanges[600] = new List<double>();
				ChannelRanges[630] = new List<double>();
				ChannelRanges[700] = new List<double>();
				ChannelRanges[730] = new List<double>();
				ChannelRanges[800] = new List<double>();
				ChannelRanges[830] = new List<double>();
				ChannelRanges[900] = new List<double>();
				ChannelRanges[930] = new List<double>();
				ChannelRanges[1000] = new List<double>();
				ChannelRanges[1030] = new List<double>();
				ChannelRanges[1100] = new List<double>();
				ChannelRanges[1130] = new List<double>();
				ChannelRanges[1200] = new List<double>();
				ChannelRanges[1230] = new List<double>();
				ChannelRanges[1300] = new List<double>();
				ChannelRanges[1330] = new List<double>();
				ChannelRanges[1400] = new List<double>();
				ChannelRanges[1430] = new List<double>();
				ChannelRanges[1500] = new List<double>();
				ChannelRanges[1530] = new List<double>();
				ChannelRanges[1600] = new List<double>();
				ChannelRanges[1630] = new List<double>();
				ChannelRanges[1700] = new List<double>();
				ChannelRanges[1730] = new List<double>();
				ChannelRanges[1800] = new List<double>();
				ChannelRanges[1830] = new List<double>();
				ChannelRanges[1900] = new List<double>();
				ChannelRanges[1930] = new List<double>();
				ChannelRanges[2000] = new List<double>();
				ChannelRanges[2030] = new List<double>();
				ChannelRanges[2100] = new List<double>();
				ChannelRanges[2130] = new List<double>();
				ChannelRanges[2200] = new List<double>();
				ChannelRanges[2230] = new List<double>();
				ChannelRanges[2300] = new List<double>();
				ChannelRanges[2330] = new List<double>();
				#endregion
			}
			else if (State == State.Historical){
				if(!ValidLicense) return;
				#region -- Historical --
				if (UserControlCollection.Contains(MenuGrid))
					return;
				if (ChartPanel!=null)
				{
					ChartPanel.KeyUp += MyKeyUpEvent;
					ChartPanel.KeyDown += MyKeyDownEvent;
					ChartPanel.MouseMove += ChartPanel_MouseMove;
				}else return;
				uID = Guid.NewGuid().ToString().Replace("-",string.Empty);

				if (ChartControl.Dispatcher.CheckAccess()) {
					CreateUIbuttons();
				}else{
					ChartControl.Dispatcher.InvokeAsync((() =>
					{
						CreateUIbuttons();
					}));
				}
				#endregion
			}
			else if (State == State.DataLoaded)
			{
				AlertsMgr = new AlertManager(this);
//				if(IsDebug){
//					Print("Inst0:  "+Instruments[0].FullName);
//					Print("Inst1:  "+Instruments[1].FullName);
//				}
				BuySymbol  = Instruments[0].MasterInstrument.Name;
				SellSymbol = Instruments[1].MasterInstrument.Name;

				IsCalendarSpread = (BuySymbol == SellSymbol);
				SpreadName = string.Format("BOSA: +{0} {1} - {2} {3}", this.pLongContracts, this.BuySymbol,  pShortContracts, SellSymbol);

				if(Instruments[0].FullName==Instruments[1].FullName) {
					TerminalError = true;
					Draw.TextFixed(this, "error","Choose a different instrument symbol...you cannot spread "+Instruments[0].FullName+" against "+Instruments[1].FullName,TextPosition.Center);
					return;
				}
				if(BarsArray[0].Count == 0 || BarsArray[1].Count == 0){
					Log("ERROR - Your Short Symbol parameter, '"+pShortSymbol+"', was not found.  Perhaps out of date?  Perhaps spelling issue?", LogLevel.Alert);
//					Draw.TextFixed(this, "NO DATA", "Your Symbol "+this.pShortSymbol+" is not found...perhaps wrong expiration date or wrong format", TextPosition.Center);
					return;
				}
//ClearOutputWindow();
//				printDebug("BarsArray[0].Count: "+BarsArray[0].Count+"    "+Instruments[0].FullName);
//				printDebug("BarsArray[1].Count: "+BarsArray[1].Count+"    "+Instruments[1].FullName);
//				printDebug("BarsArray[2].Count: "+BarsArray[2].Count+"    "+Instruments[2].FullName);
//				printDebug("BarsArray[3].Count: "+BarsArray[3].Count+"    "+Instruments[3].FullName);
//"6AE3F4567C2BED66891268ED85A34105"   Michael Filighera     #^}+-ih ijkc[-f[k^dk(e#g-f346
				#region -- out of date Validate license --
//				if(false && IsVerbose){
//					ValidLicense = true;
//					IsProVersion = true;
//					IsExpired = false;
//				}else{
//					ValidLicense = false;
//					IsProVersion = false;

//					var licfile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"bosa_config");
//					if(!System.IO.Directory.Exists(licfile)) System.IO.Directory.CreateDirectory(licfile);
//					licfile = System.IO.Path.Combine(licfile,"id.txt");
//					string lkey = pLicensePassword.Trim().ToUpper();

//					if(pLicensePassword.Trim().ToLower() == "enter your license password here" || string.IsNullOrEmpty(pLicensePassword)){
//						try{
//							if(System.IO.File.Exists(licfile)) {//read a locked file
//								using (var fileStream = new System.IO.FileStream(licfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
//								{
//								    using (var reader = new System.IO.StreamReader(fileStream))
//								    {
//								        lkey = reader.ReadToEnd().ToUpper();
//								    }
//								}
//								pLicensePassword = lkey.ToLower();
//							}
//						}catch{
//							lkey = pLicensePassword.Trim().ToLower();
//						}
//					}
//					else
//						if(!string.IsNullOrEmpty(pLicensePassword)) System.IO.File.WriteAllText(licfile, pLicensePassword.Trim().ToLower());

//					try{
//			            var arr = lkey.ToCharArray();
//			            int index = 0;
//						int indexer = 0;
//			            int i = 0;
//			            foreach(var c in arr)
//			            {
//			                if (c >= '0' && c <= '9')
//			                {
//								indexer = (int)c - (int)'0';
//			                    index = indexer + i;
//			                    break;
//			                }
//			                i++;
//			            }
//			            var c29 = ' ';
//			            var c2 = ' ';
//			            c29 = lkey[index+1];
//			            c2 = lkey[index+3];
//			            var c6 = ' ';
//			            var c18 = ' ';
//			            if (lkey[0] == '*')
//			            {
//			                c6 = lkey[lkey.Length - 1];
//			                c18 = lkey[lkey.Length - 2];
//			            }
//						if(lkey.Contains("]")){
//				            var loc_expirydate = lkey.IndexOf("]") + 1;
//				            var yr = 2000 + (int)lkey[loc_expirydate] - (int)' ' - indexer;
//				            var mo = (int)lkey[loc_expirydate+1] - (int)' ' - indexer;
//				            var day = (int)lkey[loc_expirydate+2] - (int)' ' - indexer;
//							var dt = new DateTime(yr,mo,day);
////if(IsVerbose)printDebug("dt: "+dt.ToString());
//							if(DateTime.Now > dt) {
//								ValidLicense = false;
//								IsExpired = true;
//							}else{
//								var ts = new TimeSpan(dt.Ticks - DateTime.Now.Ticks);
//								if(ts.TotalDays < 4){
//									LaunchedAt = DateTime.Now;
//									string msg = "License key '"+this.pLicensePassword+"' will expire on "+dt.ToShortDateString()+"\n\n   "+MachineId+"\nTo get an new license password, send your full name and machine id to bosa@sbgtradingcorp.com";
//									Log(msg, LogLevel.Information);
//									DrawTextFixed("licwarning", msg, TextPosition.Center, Brushes.White, font, Brushes.Maroon, Brushes.Maroon, 100);
//								}
//								IsExpired = false;
//					            if (lkey[0] == '*')
//					            {
////if(IsVerbose){
////	printDebug("\nMachine id: "+MachineId);
////	printDebug("c2: "+c2+"   MachineId[1]: "+MachineId[1]);
////	printDebug("c29: "+c29+"   MachineId["+(MachineId.Length-2)+"]: "+MachineId[MachineId.Length-2]);
////	printDebug("c6: "+c6+"   MachineId[6]: "+MachineId[6]);
////	printDebug("c18: "+c18+"   MachineId[18]: "+MachineId[18]);
////}
//					                if (MachineId[1] == c2 && MachineId[MachineId.Length - 2] == c29 && MachineId[6] == c6 && MachineId[18] == c18) {
//										ValidLicense = true;
//										IsProVersion = true;
//									}
//					            }
//					            else
//					            {
//					                if (MachineId[1] == c2 && MachineId[MachineId.Length - 2] == c29){
//										ValidLicense = true;
//										IsProVersion = false;
//									}
//					            }
//							}
//						}else
//							ValidLicense = false;

////					var arr = pLicensePassword.Trim().ToUpper().ToCharArray();
////					int offset = -1;
////					for(int i=0; i<arr.Length; i++){
////						//'aaa5xxxxLxFxxx   5 chars after the first number in the key is "L", that must be the second to last char in the MachineId.  Two chars later is the first char in the MachineId "F" in this example
////						if(offset==-1 && arr[i]>='0' && arr[i]<='9'){
////							offset = (int)arr[i] - (int)'0' + i;
////							if(arr[offset+2] == machid[1] && arr[offset] == machid[machid.Length-2]){
////								ValidLicense = true;
////								if(MachineId == "CB15E08BE30BC80628CFF6010471FA2A" && System.IO.File.Exists("c:\\222222222222.txt")) 
////									printDebug(MachineId+"  offset: "+offset+"   arr[offset]: "+arr[offset]+"   arr[offset+2]: "+arr[offset+2]+"  Valid: "+ValidLicense.ToString());
////								break;
////							}
////						}
////					}
//					}catch{}
//					if(!ValidLicense){
////						if(pLicensePassword.Trim() == "enter your license password here" || string.IsNullOrEmpty(pLicensePassword.Trim()))
////							Draw.TextFixed(this,"licerror","Please enter your LicensePassword into the LicensePassword parameter\n   "+MachineId+"\nSend your full name and machine id to "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,new SimpleFont("Arial",15),Brushes.Maroon,Brushes.Maroon,100);
////						else  *a1f2bbs]*0c8
//							if(pLicensePassword.Contains("enter your"))
//								Draw.TextFixed(this, "licerror","Your License key must be entered into your LicensePassword parameter\n\n   "+MachineId+"\n\nQuestions? Send your full name and machine id to "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,font,Brushes.Maroon,Brushes.Maroon,100);
//							else if(IsExpired)
//								Draw.TextFixed(this, "licerror","License key '"+this.pLicensePassword+"' is expired\n\n   "+MachineId+"\n\nQuestions? Send your full name and machine id to "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,font,Brushes.Maroon,Brushes.Maroon,100);
//							else
//								Draw.TextFixed(this, "licerror","License key '"+this.pLicensePassword+"' is not valid for this machine id\n\n   "+MachineId+"\nCheck to see if your machine id changed recently.\n\nQuestions? Send your full name and machine id to "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,font,Brushes.Maroon,Brushes.Maroon,100);
//						return;
//					}
//				}
				#endregion
#if DO_LICENSE
				string Status = "";
				IsProVersion = IsDebug;
//				if(!IsDebug) {
//					IsProVersion = false;
//					CheckLicense(ref ValidLicense, ref ExpirationDT, ref Status, false);
//				}else{
//					ValidLicense = true;
//					//IsProVersion is determined by user input
//				}
//Print("\n\n\nValid license: "+ValidLicense.ToString());
				if(!ValidLicense){
//Print("    LicPass: "+pLicensePassword);
					if(pLicensePassword.Trim().ToLower().StartsWith("<enter password here>")){
						if(!IsDebug) VendorLicense("DayTradersAction", "SbgBOSA", "www.sbgtradingcorp.com", "bosa@sbgtradingcorp.com");
						ValidLicense = true;
					}else{
						if(pLicensePassword.Contains("enter your"))
							DrawTextFixed("licerror","Your License key must be entered into your LicensePassword parameter\n\n   "+MachineId+"\n\nQuestions? Send your full name and machine id to:  "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,font,Brushes.Maroon,Brushes.Maroon,100);
						else if(IsExpired)
							DrawTextFixed("licerror","License key '"+this.pLicensePassword+"' is expired\n\n   "+MachineId+"\n\nQuestions? Send your full name and machine id to:  "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,font,Brushes.Maroon,Brushes.Maroon,100);
						else
							DrawTextFixed("licerror","License key '"+this.pLicensePassword+"' is not valid for this machine id\n\n   "+MachineId+"\nCheck to see if your machine id changed recently.\n\nQuestions? Send your full name and machine id to:  "+this.SupportEmailAddress,TextPosition.Center,Brushes.White,font,Brushes.Maroon,Brushes.Maroon,100);
						return;
					}
				}
//				if(!ValidLicense) Log(ProductName+" Lic Status: "+Status, LogLevel.Information);
#endif

				PLUS_POINT_VALUE = pLongContracts * Instruments[0].MasterInstrument.PointValue;
				MINUS_POINT_VALUE = pShortContracts * Instruments[1].MasterInstrument.PointValue;

				if(Calculate == Calculate.OnBarClose)         SettingStr_Calculate = "c_bc";
				else if(Calculate == Calculate.OnPriceChange) SettingStr_Calculate = "c_pc";
				else if(Calculate == Calculate.OnEachTick)    SettingStr_Calculate = "c_et";
				if(this.pAutoTradeOnBarClose)                 SettingStr_Calculate = "c_bc";
				if (ChartControl.Dispatcher.CheckAccess()) {
					chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
					GetCurrentSelectedAccount();
//					SourceOfTrade = uID;
					OnPositionUpdate(null,null);
				}else{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
    		        {
						chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
						GetCurrentSelectedAccount();
//						SourceOfTrade = uID;
						OnPositionUpdate(null,null);
					}));
				}
//				OriginalPlotColors = new Brush[Plots.Length];
//				for(int i = 0; i<Plots.Length; i++){
//					OriginalPlotColors[i] = Plots[i].Brush.Clone();
//					OriginalPlotColors[i].Freeze();
//				}

				#region -- Calculate Equity ratio --
				double AvgEquityRange0 = 0;
				double AvgEquityRange1 = 0;
				int daynum = 0;
				double H = double.MinValue;
				double L = double.MaxValue;
				List<double> DailyRange = new List<double>();
				for(int abar = 1; abar<BarsArray[0].Count; abar++){
					var t1    = Times[0].GetValueAt(abar-1);
					var tt1   = ToTime(t1)/100;
					var t0    = Times[0].GetValueAt(abar);
					var tt0   = ToTime(t0)/100;
					bool InSession = (pStartTime==pStopTime) || (tt0 >= pStartTime && tt0 < pStopTime);
					if(InSession){
						if(daynum==0) {
							daynum = Times[0].GetValueAt(abar).Day; 
							H = Highs[0].GetValueAt(abar);
							L = Lows[0].GetValueAt(abar);
							abar++;
						}
						if(daynum != Times[0].GetValueAt(abar).Day){
							daynum = Times[0].GetValueAt(abar).Day;
							DailyRange.Add(H-L);
							H = Highs[0].GetValueAt(abar);
							L = Lows[0].GetValueAt(abar);
						}else{
							H = Math.Max(H,Highs[0].GetValueAt(abar));
							L = Math.Min(L,Lows[0].GetValueAt(abar));
						}
					}
				}
				AvgEquityRange0 = DailyRange.Average() * Instruments[0].MasterInstrument.PointValue;

				daynum = 0;
				H = double.MinValue;
				L = double.MaxValue;
				DailyRange.Clear();
				for(int abar = 1; abar<BarsArray[1].Count; abar++){
					var t1    = Times[1].GetValueAt(abar-1);
					var tt1   = ToTime(t1)/100;
					var t0    = Times[1].GetValueAt(abar);
					var tt0   = ToTime(t0)/100;
					bool InSession = (pStartTime==pStopTime) || (tt0 >= pStartTime && tt0 < pStopTime);
					if(InSession){
						if(daynum==0) {
							daynum = Times[1].GetValueAt(abar).Day; 
							H = Highs[1].GetValueAt(abar);
							L = Lows[1].GetValueAt(abar);
							abar++;
						}
						if(daynum != Times[1].GetValueAt(abar).Day){
							daynum = Times[1].GetValueAt(abar).Day;
							DailyRange.Add(H-L);
							H = Highs[1].GetValueAt(abar);
							L = Lows[1].GetValueAt(abar);
						}else{
							H = Math.Max(H,Highs[1].GetValueAt(abar));
							L = Math.Min(L,Lows[1].GetValueAt(abar));
						}
					}
				}
				AvgEquityRange1 = DailyRange.Average() * Instruments[1].MasterInstrument.PointValue;
				var EquityRatio = Math.Max(AvgEquityRange0, AvgEquityRange1) / Math.Min(AvgEquityRange0,AvgEquityRange1);
				//string s1 = "KeyLevel "+BuySymbol+": "+AvgEquityRange0.ToString("C")+"  KeyLevel "+SellSymbol+": "+AvgEquityRange1.ToString("C");
				string s3 = "Buy "+EquityRatio.ToString("0.00")+" contracts of "+(BuySymbol==SellSymbol ? Instruments[0].FullName : BuySymbol)+" and sell 1 contract of "+(BuySymbol==SellSymbol ? Instruments[1].FullName : SellSymbol);
				if(AvgEquityRange0 > AvgEquityRange1){
					s3 = "\nBuy 1 contract of "+(BuySymbol==SellSymbol ? Instruments[0].FullName : BuySymbol)+" and sell "+EquityRatio.ToString("0.00")+" contracts of "+(BuySymbol==SellSymbol ? Instruments[1].FullName : SellSymbol);
				}
//				Draw.TextFixed(this,"DailyRangeRatio",s3, TextPosition.TopLeft);
//try{
//   printDebug("Days: "+DailyRange.Count+":  "+Instruments[0].MasterInstrument.Name+": "+AvgEquityRange0+"   "+Instruments[1].MasterInstrument.Name+": "+AvgEquityRange1);
//}catch(Exception kss){printDebug("Kss: "+kss.ToString());}
				EquityRatioString = string.Format("V$Ratio {0} {1} & 1 {2}",EquityRatio.ToString("0.0"), BuySymbol, SellSymbol);
//				string s3 = "Buy "+EquityRatio.ToString("0.00")+" contracts of "+(BuySymbol==SellSymbol ? Instruments[0].FullName : BuySymbol)+" and sell 1 contract of "+(BuySymbol==SellSymbol ? Instruments[1].FullName : SellSymbol);
				if(AvgEquityRange0 > AvgEquityRange1){
					EquityRatioString = string.Format("V$Ratio 1 {0} & {1} {2}", BuySymbol, EquityRatio.ToString("0.0"),SellSymbol);
//					s3 = "\nBuy 1 contract of "+(BuySymbol==SellSymbol ? Instruments[0].FullName : BuySymbol)+" and sell "+EquityRatio.ToString("0.00")+" contracts of "+(BuySymbol==SellSymbol ? Instruments[1].FullName : SellSymbol);
				}
				#endregion
			}
			else if (State == State.Realtime){
				if (timer == null)
				{
					lock (Connection.Connections)
					{
						if (Connection.Connections.ToList().FirstOrDefault(c => c.Status == ConnectionStatus.Connected && c.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType)) == null)
							Draw.TextFixed(this, "BottomRightText", "Not connected error", TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
						else
						{
							if (!SessionIterator.IsInSession(Now, false, true))
								Draw.TextFixed(this, "BottomRightText", "Session time error", TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
							else
								Draw.TextFixed(this, "BottomRightText", "Waiting on data", TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
						}
					}
				}

				if(pShowReversalDots){
					for(int i = pReversalDotsPeriod*2+3; i< Spread.Count-2; i++){
						try{
							CalculateTrendAge(i, Spread, cci);
						}catch(Exception e){printDebug(i+"  :  "+e.ToString());}
					}
				}
			}
			else if (State == State.Terminated)
			{
				#region == Terminated ==
				if (timer != null){
					timer.IsEnabled = false;
					timer = null;
				}

				if(ChartPanel!=null){
					ChartPanel.KeyUp -= MyKeyUpEvent;
					ChartPanel.KeyDown -= MyKeyDownEvent;
					ChartPanel.MouseMove -= ChartPanel_MouseMove;
				}
				if (ChartControl != null)
				{
					if(ChartControl.Dispatcher.CheckAccess()) {
						DisposeMenu();
					}else{
						Dispatcher.InvokeAsync(( () =>
						{
							DisposeMenu();
						}));
					}
				}
				var accts = Account.All.ToList();
				for(int i = 0; i<accts.Count; i++){
					accts[i].ExecutionUpdate -= OnExecutionUpdate;
					accts[i].PositionUpdate -= OnPositionUpdate;
				}
				#endregion
			}
			#endregion
}catch(Exception ddd){printDebug(ddd.ToString());}
		}
		#region -- License methods --
				//CB15608BE30BC80628CFF601047182AA
				//6k*-FJDLB1-FFDIGD-G129
				//1)  anything before the first '-' is ignored
				//2)  "FJDLB1" The last digit in the Mach ID is 2...this translates to "F"...so the first char in password is "F".
				//			Find the first number in the MachID, and the 2nd char after that number is '6', which translates to 'J', the next password char 'J'
				//			The next char after the '6' is '0', which is 'D'
				//			The next char after the '0' is '8' which is 'L'
				//			The next char after the '8' is 'B' which is 'B' (there's no numerical translation for 'B')
				//			The next char after the 'B' is 'E' which is '1'
				//  NOTE:  If a "RBT" is found in this machine id code...then all machine id's are valid...only restricted by the expiration date and module number
				//3)  "FFDIGD" is the expiration date  'F' is 2, 'D' is 0, 'I' is 5, 'G' is 3.  So the expiration date is 220530 (2022-May-30)
				//4)  "G129" is module identifier.  "G" is the multiplier, "G" translates to 3.  So take the 129 and divide by 3...so the Module number is 129/3 = 43.
				//0 = D, 1 = E, 2 = F, 3 = G, 4 = H, 5 = I, 6 = J, 7 = K, 8 = L, 9 = M
		private void CheckLicense(ref bool ValidLicense, ref DateTime ExpiredAt, ref string Status, bool PrintAll){
			string machid_status = null;
			string date_status = null;
			string prodid_status = null;
			Status = "";
			ExpiredAt = DateTime.MaxValue;
			try
			{
				string licfile = null;
				var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"bosa_config");
				if(!System.IO.Directory.Exists(path)) {
					System.IO.Directory.CreateDirectory(path);
					var instr = System.IO.Path.Combine(path, "ReadTheseInstructions.txt");
					System.IO.File.WriteAllLines(instr, new string[]{
						"",
						"--- IMPORTANT INSTRUCTIONS ---",
						"",
						"Do not move or remove this folder (bosa_config).",
						"That action will result in a license error for all of your BOSA related indicators",
						"",
						"Do not delete or remove any of the text files in this folder.",
						"That action will result in a license error for specific BOSA related indicators",
						"",
						"Contact "+SupportEmailAddress+" if you need further clarification"
					});
				}
				string search = ProductName+"_*.txt";
				System.IO.DirectoryInfo dirCustom =null;
				System.IO.FileInfo[] filCustom    =null;
				try{
					dirCustom = new System.IO.DirectoryInfo(path);
					filCustom = dirCustom.GetFiles(search);
				}catch{filCustom = null;}
if(PrintAll) foreach(var ff in filCustom) printDebug("License file found: "+ff.Name);

				string lpass = pLicensePassword.Trim().ToLower();
				if(lpass.Contains("enter password here") || string.IsNullOrEmpty(lpass)){
					try{
						if(filCustom!=null && filCustom.Length>0){
if(PrintAll) printDebug("file name: "+filCustom[0].Name);
							lpass = filCustom[0].Name.Replace(ProductName+"_",string.Empty).Replace(".txt",string.Empty).Trim();
						}
						pLicensePassword = lpass.ToLower();
if(PrintAll) printDebug("lpass from file: "+pLicensePassword);
					}catch{
						lpass = pLicensePassword.Trim().ToLower();
if(PrintAll) printDebug("lpass from file: "+lpass);
					}
				}else{
					licfile = System.IO.Path.Combine(path, StripOutIllegalCharacters(ProductName+"_"+pLicensePassword.Trim().ToLower()+".txt",' '));
					foreach(var f in filCustom)	{
if(PrintAll) printDebug("Deleted file: "+f.FullName);
						if(f.FullName!=licfile) System.IO.File.Delete(f.FullName);
					}
					if(!string.IsNullOrEmpty(pLicensePassword)) {
if(PrintAll) printDebug("Writing password to: "+licfile);
						System.IO.File.WriteAllText(licfile, "");
					}
				}

				string MachineId = NinjaTrader.Cbi.License.MachineId.ToLower();
			    var chararray = MachineId.ToCharArray();
			    int LastNumber = -1;
			    char CharOfLastNumber = ' ';
			    for (int i = chararray.Length - 1; i >= 0; i--) if (chararray[i] >= '0' && chararray[i] <= '9'){
					CharOfLastNumber = chararray[i];
					LastNumber = (int)CharOfLastNumber - (int)'0'; break;
					machid_status = "i = " + i + " char: " + CharOfLastNumber;
			    }
				var key = KeepTheseChars(pLicensePassword.ToLower().ToCharArray(), "abcdefghijklmnopqrstuvwxyz-0123456789");
if(PrintAll) printDebug("\nMachineID: "+MachineId);
if(PrintAll) printDebug("Key: "+key);
				var elems = key.Split(new char[] { '-' }, StringSplitOptions.None);
				int idx = -1;
if(PrintAll) printDebug("elems.Length: "+elems.Length);
				if(elems.Length<=3) IsProVersion = false;
				ValidLicense = true;
				if(elems.Length<3 || elems.Length>4) ValidLicense = false;//there must be 3 elements in the password.  Machine id, Expiration Date and Product Number
				for (int i = 1; i < elems.Length; i++){
if(PrintAll) printDebug("   elems["+i+"]: "+elems[i]);
					if (i == 1){//this is the machine id verification segment
if(PrintAll) printDebug("Checking machind id");
						if (CheckForRBT(elems[i])) {machid_status = "wildcard 'rbt' found"; printDebug("universal machid"); continue;}//"RBT" is the global machine id...it passes this machine id verification
						if (Decode(CharOfLastNumber) != elems[i][0]){
						    machid_status = CharOfLastNumber+"  CofLN: " + Decode(CharOfLastNumber) + "  elems[1]0: " + elems[i][0];
							Status = "Line 138 - "+machid_status;
if(PrintAll) printDebug("*******************  Failed at 822, machine id check: "+Status);
						    ValidLicense = false; break;//failed
						}else{
							for (int j = 0; j < chararray.Length - 1; j++) {
								if (chararray[j] >= '0' && chararray[j] <= '9') {
									idx = j + LastNumber;
									Status = "Line 144 - "+chararray[j];
									break;
								}
							}
							//machid_status = "LastNumber: " + LastNumber.ToString() + "  idx: " + idx.ToString();
							if (idx < 0){
								Status = "Line 150 - "+machid_status;
if(PrintAll) printDebug("*******************  Failed at 834, Elem[1]: "+Status);
								break;//failed
							}else{
								machid_status = string.Format("__{0} 1 {1} = {2}  |", machid_status, Decode(elems[i][1]), chararray[idx]);
								machid_status = string.Format("{0} 2 {1} = {2}  |", machid_status, Decode(elems[i][2]), chararray[idx+1]);
								machid_status = string.Format("{0} 3 {1} = {2}  |", machid_status, Decode(elems[i][3]), chararray[idx+2]);
								machid_status = string.Format("{0} 4 {1} = {2}  |", machid_status, Decode(elems[i][4]), chararray[idx+3]);
								if (Decode(elems[i][1]) != chararray[idx + 0]) { machid_status = machid_status + "*1"; Status=machid_status; ValidLicense = false;
if(PrintAll) printDebug("*******************  Failed at 851: "+machid_status);
										break; }//failed
								if (Decode(elems[i][2]) != chararray[idx + 1]) { machid_status = machid_status + "*2"; Status=machid_status; ValidLicense = false;
if(PrintAll) printDebug("*******************  Failed at 854: "+machid_status);
										break; }//failed
								if (Decode(elems[i][3]) != chararray[idx + 2]) { machid_status = machid_status + "*3"; Status=machid_status; ValidLicense = false;
if(PrintAll) printDebug("*******************  Failed at 857: "+machid_status);
										break; }//failed
								if (Decode(elems[i][4]) != chararray[idx + 3]) { machid_status = machid_status + "*4"; Status=machid_status; ValidLicense = false;
if(PrintAll) printDebug("*******************  Failed at 860: "+machid_status);
										break; }//failed
							}
						}
if(PrintAll) printDebug("  Machine id passed verification");
					}
					else if (i == 2)
					{
if(PrintAll) printDebug("Checking expiration date");
						var date = string.Format("{0}{1}", Decode(elems[i][0]), Decode(elems[i][1]));
						date_status = string.Format("{0}{1}", date_status, date);
						int yr = int.Parse(date); date = string.Empty;
						date = string.Format("{0}{1}", Decode(elems[i][2]), Decode(elems[i][3]));
						date_status = string.Format("{0}{1}", date_status, date);
						int mo = int.Parse(date); date = string.Empty;
						date = string.Format("{0}{1}", Decode(elems[i][4]), Decode(elems[i][5]));
						date_status = string.Format("{0}{1}", date_status, date);
						int day = int.Parse(date); date = string.Empty;
						ExpiredAt = new DateTime(2000 + yr, mo, day);
						date_status = string.Format("{0}  {1}", date_status, ExpiredAt.ToShortDateString());
						if(ExpiredAt < DateTime.Now) {
							Status=date_status; ValidLicense=false; 
if(PrintAll) printDebug("*******************  Failed at 879, date check: "+Status);
							break;
						}else{
							var ts = new TimeSpan(ExpiredAt.Ticks-DateTime.Now.Ticks);
if(PrintAll) printDebug("Expirationdate verified...will expire in "+ts.TotalDays+"-days");
						}
					}
					else if (i == 3 && (ProductID_Regular > 0 || ProductID_Pro > 0))
					{
if(PrintAll) printDebug("Checking product id");
						int Mult = (int)Decode(elems[i][0]) - (int)'0';
						if (Mult>0)
						{
							chararray = elems[i].ToCharArray();
							chararray[0] = '0';//so 'g129' becomes a numerical string of '0129'
							var str = new StringBuilder();
							str.Append(chararray);
							prodid_status = str.ToString();
							double productNumFromFile = double.Parse(prodid_status) / Mult;
							prodid_status = str.ToString() + " mult "+Mult+"  = "+productNumFromFile;
							string pidff = productNumFromFile.ToString();
if(PrintAll) printDebug("pidff: "+pidff);
							if (productNumFromFile>0 && !pidff.Contains(ProductID_Regular.ToString()) && !pidff.Contains(ProductID_Pro.ToString())) {ExpiredAt = DateTime.MaxValue; Status=prodid_status; ValidLicense=false; break;}
							if (productNumFromFile>0 && pidff.Contains(ProductID_Regular.ToString())) {IsProVersion = false;}
							if (productNumFromFile>0 && pidff.Contains(ProductID_Pro.ToString()))     {IsProVersion = true;}
							if(IsProVersion) printDebug("Pro Version = true"); else printDebug("Regular version = true");
						}
					}
				}
			}
			catch(Exception eee) {printDebug("Error: "+eee.ToString()); ValidLicense = false; }
if(PrintAll) printDebug(Environment.NewLine+  ProductName+"  IsValidLicense?  "+ValidLicense.ToString());
			if(!ValidLicense && PrintAll){
				printDebug("\nmachid status: "+machid_status);
				printDebug("date status: "+date_status);
				printDebug("prodid status: "+prodid_status);
			}
if(PrintAll) printDebug("end of license check, ValidLicense: "+ValidLicense.ToString());
		}
		#region -- license support --
		private bool CheckForRBT(string input){
			int a = input.IndexOf("r");
			int b = input.IndexOf("b");
			int c = input.IndexOf("t");
			return a<b || b<c;
		}
		private char Decode(char inchar){
			//0 = D, 1 = E, 2 = F, 3 = G, 4 = H, 5 = I, 6 = J, 7 = K, 8 = L, 9 = M
			if(inchar == '0') return 'd';
			if(inchar == '1') return 'e';
			if(inchar == '2') return 'f';
			if(inchar == '3') return 'g';
			if(inchar == '4') return 'h';
			if(inchar == '5') return 'i';
			if(inchar == '6') return 'j';
			if(inchar == '7') return 'k';
			if(inchar == '8') return 'l';
			if(inchar == '9') return 'm';
			if(inchar == 'd') return '0';
			if(inchar == 'e') return '1';
			if(inchar == 'f') return '2';
			if(inchar == 'g') return '3';
			if(inchar == 'h') return '4';
			if(inchar == 'i') return '5';
			if(inchar == 'j') return '6';
			if(inchar == 'k') return '7';
			if(inchar == 'l') return '8';
			if(inchar == 'm') return '9';
			return inchar;
		}
		private string KeepTheseChars(char[] chararray, string keepers){
			string result = string.Empty;
			for(int i = 0; i<chararray.Length; i++)
				if(keepers.Contains(chararray[i])) result = string.Format("{0}{1}", result,chararray[i]);
			return result;
		}
		#endregion
		#endregion
//===============================================================================================
		private string StripOutIllegalCharacters(string filename, char ReplacementChar){
			#region strip
			var invalidChars = System.IO.Path.GetInvalidFileNameChars();
			return new string(filename.Select(c => invalidChars.Contains(c) ? ReplacementChar: c).ToArray());
			#endregion
		}
//===============================================================================================
		string last_printed_line = "";
		private void printDebug(string p, int tab=1, bool full=false){ 
			if(!IsDebug) return;
			if(p.CompareTo(last_printed_line)==0) return;
			last_printed_line = p;
			try{
				if(SelectedTab.Contains(uID)) {
					if(tab == 1)
						NinjaTrader.Code.Output.Process(string.Format("{0}{1}", (full ? Now.ToString("HH:mm:ss  ") : ""), p), PrintTo.OutputTab1);
					else
						NinjaTrader.Code.Output.Process(string.Format("{0}{1}", (full ? Now.ToString("HH:mm:ss  ") : ""), p), PrintTo.OutputTab2);
				}else{
					NinjaTrader.Code.Output.Process(string.Format("{0}{1}", (full ? Now.ToString("HH:mm:ss  ") : ""), p), PrintTo.OutputTab2);
				}
			}catch(Exception kss){if(full) Print ("Kss: "+kss.ToString()); Print (p);}
		}
//====================================================================
		private void UpdateExtraButtonsText(double qty){
			#region -- UpdateExtraButtonsText --
			double dummydble = 0;
			string dummystr = "";
			double posLongLeg = 0;
			double posShortLeg = 0;
			if(qty>1){
				btn_BuyLongInst.Content   = string.Format("BUY {0} {1}",qty,Instruments[0].FullName);
				btn_SellLongInst.Content  = string.Format("SELL {0} {1}",qty,Instruments[0].FullName);
				btn_BuyShortInst.Content  = string.Format("BUY {0} {1}",qty,Instruments[1].FullName);
				btn_SellShortInst.Content = string.Format("SELL {0} {1}",qty,Instruments[1].FullName);
			}else{
				btn_BuyLongInst.Content   = string.Format("BUY {0}", Instruments[0].FullName);
				btn_SellLongInst.Content  = string.Format("SELL {0}", Instruments[0].FullName);
				btn_BuyShortInst.Content  = string.Format("BUY {0}", Instruments[1].FullName);
				btn_SellShortInst.Content = string.Format("SELL {0}", Instruments[1].FullName);
			}

			#endregion
		}

//====================================================================
		private void CreateUIbuttons(){
			#region -- CreateUIbuttons --
printDebug("------------------------ creating UI buttons");
				MenuGrid = new System.Windows.Controls.Grid
				{
					Name = "BuySellGrid_"+uID, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top
				};

				MenuGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
				MenuGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
				MenuGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
				MenuGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
				MenuGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
				MenuGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
				MenuGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
				MenuGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
				if(pSMAperiod <= 0 && IsProVersion)
					MenuGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());//4th row for Show/Hide Session TPO

				#region -- btn_GoFlat --
				btn_GoFlat_Name = "GF_"+uID;
				btn_GoFlat = new System.Windows.Controls.Button
				{
					Name = btn_GoFlat_Name, Content = "Go FLAT", Foreground = Brushes.Beige, Background = Brushes.DarkBlue,
					ToolTip = "Exit all open positions on the platform"
				};
				ButtonSettings[btn_GoFlat_Name] = new ButtonData(btn_GoFlat);

				btn_ShowReversalDots_Name = "SRD_"+uID;
				btn_ShowReversalDots = new System.Windows.Controls.Button
				{
					Name = btn_ShowReversalDots_Name, Content = pShowReversalDots ? "Hide Reversal Dots":"Show Reversal Dots", Foreground = Brushes.Black, Background = Brushes.White,
					ToolTip = "Turn on/off reversal dots"
				};
				ButtonSettings[btn_ShowReversalDots_Name] = new ButtonData(btn_ShowReversalDots);

				btn_ShowSpreadHistory_Name = "SHD_"+uID;
				btn_ShowSpreadHistory = new System.Windows.Controls.Button
				{
					Name = btn_ShowSpreadHistory_Name, Content = pShowSpreadHistory ? "Hide SpreadHistory":"Show SpreadHistory", Foreground = Brushes.Black, Background = Brushes.White,
					ToolTip = "Turn on/off historical average spread dots"
				};
				ButtonSettings[btn_ShowSpreadHistory_Name] = new ButtonData(btn_ShowSpreadHistory);

				btn_ShowSessionTPO_Name = "SSTPO_"+uID;
				btn_ShowSessionTPO = new System.Windows.Controls.Button
				{
					Name = btn_ShowSessionTPO_Name, Content = pShowSessionTPO ? "Hide Sess. TPO":"Show Sess. TPO", Foreground = Brushes.Black, Background = Brushes.White,
					ToolTip = "Use scrollwheel to expand/contract TPO size"
				};
				ButtonSettings[btn_ShowSessionTPO_Name] = new ButtonData(btn_ShowSessionTPO);

				#endregion ----------------------------------------------------------------
				#region -- btn_BuySpread --
				btn_BuySpread_Name = "BS_"+uID;
				btn_BuySpread = new System.Windows.Controls.Button
				{
					Name = btn_BuySpread_Name, Content = "BUY", Foreground = Brushes.White, Background = Brushes.Green,
					ToolTip = "Buy the spread (buy long contract, sell short contract)\nUse mousewheel to change qty"
				};
				ButtonSettings[btn_BuySpread_Name] = new ButtonData(btn_BuySpread);
				btn_BuySpread.MouseWheel += delegate(object o, MouseWheelEventArgs e){
					if(e!=null) {
						e.Handled = true;
						if(e.Delta>0){
							SelectedSpreadQty ++;
						}else{
							SelectedSpreadQty = Math.Max(1, SelectedSpreadQty-1);
						}
						if(IsProVersion){
							SelectedSpreadQty = Math.Min(pMaxSpreadsQty, SelectedSpreadQty);
						}else
							SelectedSpreadQty = 1;

						double qty = SelectedSpreadQty;
						if(AutoState == BuyOneSellAnother_AutoState.Close) {
							qty = Math.Abs(CurrentSpreadPosition);
						}
						UpdateAutoTradeButton(AutoState);
						StatusMsg = DetermineStatusMsg(AutoState);
						if(ShowExtraBtns){
							UpdateExtraButtonsText(qty);
							UpdateUI();
						}
						ForceRefresh();
					}
				};
				#endregion ----------------------------------------------------------------
				#region -- btn_SellSpread --
				btn_SellSpread_Name = "SS_"+uID;
				btn_SellSpread = new System.Windows.Controls.Button
				{
					Name = btn_SellSpread_Name, Content = "SELL", Foreground = Brushes.Black, Background = Brushes.Red,
					ToolTip = "Sell the spread (sell long contract, buy short contract)\nUse mousewheel to change qty"
				};
				ButtonSettings[btn_SellSpread_Name] = new ButtonData(btn_SellSpread);
				btn_SellSpread.MouseWheel += delegate(object o, MouseWheelEventArgs e){
					if(e!=null) {
						e.Handled = true;
						if(e.Delta>0){
							SelectedSpreadQty ++;
						}else{
							SelectedSpreadQty = Math.Max(1, SelectedSpreadQty-1);
						}
						if(IsProVersion){
							SelectedSpreadQty = Math.Min(pMaxSpreadsQty, SelectedSpreadQty);
						}else
							SelectedSpreadQty = 1;

						double qty = SelectedSpreadQty;
						if(AutoState == BuyOneSellAnother_AutoState.Close) {
							qty = Math.Abs(CurrentSpreadPosition);
						}
						UpdateAutoTradeButton(AutoState);
						StatusMsg = DetermineStatusMsg(AutoState);
						if(ShowExtraBtns){
							UpdateExtraButtonsText(qty);
							UpdateUI();
						}
						ForceRefresh();
					}
				};
				#endregion ----------------------------------------------------------------
				#region -- btn_AutoTrade --
				btn_AutoTrade_Name = "AT_"+uID;
				btn_AutoTrade = new System.Windows.Controls.Button
				{
					Name = btn_AutoTrade_Name, Content = "Auto "+AutoState.ToString(), Foreground = Brushes.Black, Background = AutoTradeEnabled ? Brushes.Orange:Brushes.Gray,
					ToolTip = "Use mousewheel to change, click to turn-off AutoTrade"
				};
				ButtonSettings[btn_AutoTrade_Name] = new ButtonData(btn_AutoTrade);
				btn_AutoTrade.MouseWheel += delegate(object o, MouseWheelEventArgs e){
					if(e!=null) {
						e.Handled = true;
						if(e.Delta>0){
							if(AutoState == BuyOneSellAnother_AutoState.Off){
								if(CurrentSpreadPosition ==0) {
									AutoState = BuyOneSellAnother_AutoState.BuyOnly;
									TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
								}else{
									AutoState = BuyOneSellAnother_AutoState.Close;
									TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
								}
							}
							else if(AutoState == BuyOneSellAnother_AutoState.BuyOnly){
								AutoState = BuyOneSellAnother_AutoState.Add;
								TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
							}
							else if(AutoState == BuyOneSellAnother_AutoState.Add){
								BottomRightTextStr = string.Empty;
								AutoState = BuyOneSellAnother_AutoState.Off;
							}
							else if(AutoState == BuyOneSellAnother_AutoState.SellOnly){
								BottomRightTextStr = string.Empty;
								AutoState = BuyOneSellAnother_AutoState.Off;
							}
							else if(AutoState == BuyOneSellAnother_AutoState.Close) {
								AutoState = BuyOneSellAnother_AutoState.BuyOnly;
								TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
							}
						}else{
							if(AutoState == BuyOneSellAnother_AutoState.Off){
								if(CurrentSpreadPosition ==0) {
									AutoState = BuyOneSellAnother_AutoState.SellOnly;
									TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
								}else{
									AutoState = BuyOneSellAnother_AutoState.Close;
									TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
								}
							}
							else if(AutoState == BuyOneSellAnother_AutoState.SellOnly){
								AutoState = BuyOneSellAnother_AutoState.Add;
								TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
							}
							else if(AutoState == BuyOneSellAnother_AutoState.Add){
								BottomRightTextStr = string.Empty;
								AutoState = BuyOneSellAnother_AutoState.Off;
							}
							else if(AutoState == BuyOneSellAnother_AutoState.BuyOnly){
								BottomRightTextStr = string.Empty;
								AutoState = BuyOneSellAnother_AutoState.Off;
							}
							else if(AutoState == BuyOneSellAnother_AutoState.Close) {
								AutoState = BuyOneSellAnother_AutoState.SellOnly;
								TimeOfAutoEnabled = DateTime.Now.AddSeconds(5);
							}
						}
						UpdateAutoTradeButton(AutoState);
						StatusMsg = DetermineStatusMsg(AutoState);
						ForceRefresh();
					}
				};
				btn_AutoTrade.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					TimeOfAutoEnabled = DateTime.Now;
					if(AutoState == BuyOneSellAnother_AutoState.Off){
						AutoState = BuyOneSellAnother_AutoState.Close;
					}else{
						BottomRightTextStr = string.Empty;
						AutoState = BuyOneSellAnother_AutoState.Off;
					}
					UpdateAutoTradeButton(AutoState);
					StatusMsg = DetermineStatusMsg(AutoState);
					GetCurrentSelectedAccount();
					OnPositionUpdate(null,null);
					ForceRefresh();
				};
				#endregion ----------------------------------------------------------------

				btn_BuyLongInst_Name = "BLI_"+uID;
				btn_BuyLongInst = new System.Windows.Controls.Button
				{
					Name = btn_BuyLongInst_Name, Content = "BUY "+Instruments[0].FullName, Foreground = Brushes.Black, Background = Brushes.Lime
				};
				ButtonSettings[btn_BuyLongInst_Name] = new ButtonData(btn_BuyLongInst);

				btn_SellLongInst_Name = "SLI_"+uID;
				btn_SellLongInst = new System.Windows.Controls.Button
				{
					Name = btn_SellLongInst_Name, Content = "SELL "+Instruments[0].FullName, Foreground = Brushes.Black, Background = Brushes.Magenta
				};
				ButtonSettings[btn_SellLongInst_Name] = new ButtonData(btn_SellLongInst);

				btn_BuyShortInst_Name = "BSI_"+uID;
				btn_BuyShortInst = new System.Windows.Controls.Button
				{
					Name = btn_BuyShortInst_Name, Content = "BUY "+Instruments[1].FullName, Foreground = Brushes.Black, Background = Brushes.Lime
				};
				ButtonSettings[btn_BuyShortInst_Name] = new ButtonData(btn_BuyShortInst);

				btn_SellShortInst_Name = "SSI_"+uID;
				btn_SellShortInst = new System.Windows.Controls.Button
				{
					Name = btn_SellShortInst_Name, Content = "SELL "+Instruments[1].FullName, Foreground = Brushes.Black, Background = Brushes.Magenta
				};
				ButtonSettings[btn_SellShortInst_Name] = new ButtonData(btn_SellShortInst);

				btn_HideExtraButtons_Name = "HEB_"+uID;
				btn_HideExtraButtons = new System.Windows.Controls.Button{
					Name = btn_HideExtraButtons_Name, Content = "Show/Hide", Foreground = Brushes.Black, Background = Brushes.White
				};
				ButtonSettings[btn_HideExtraButtons_Name] = new ButtonData(btn_HideExtraButtons);

				btn_AccountName_Name = "Acct_"+uID;
				btn_AccountName = new System.Windows.Controls.Button{
					Name = btn_AccountName_Name, Content = this.AccountName, Foreground = Brushes.Black, Background = Brushes.White
				};
				ButtonSettings[btn_AccountName_Name] = new ButtonData(btn_AccountName);

				btn_DrawAutoTradeLines_Name = "DATL_"+uID;
				btn_DrawAutoTradeLines = new System.Windows.Controls.Button{
					Name = btn_DrawAutoTradeLines_Name, Content = "Draw AT Lines", Foreground = Brushes.Black, Background = Brushes.White
				};
				ButtonSettings[btn_DrawAutoTradeLines_Name] = new ButtonData(btn_DrawAutoTradeLines);

				int qty2 = SelectedSpreadQty;
				if(AutoState == BuyOneSellAnother_AutoState.Close) {
					qty2 = Math.Abs(CurrentSpreadPosition);
				}
				btn_BuySpread.Content = string.Format("BUY {0}x", qty2);
				btn_SellSpread.Content = string.Format("SELL {0}x", qty2);

				btn_GoFlat.Click += delegate(object o, RoutedEventArgs e){
					//TestSend = true;
					OIFnumber = OIF_Submit(OIF_FlattenEverything(), OIFnumber); 
				};
				btn_BuySpread.Click   += OnButtonClick;
				btn_SellSpread.Click  += OnButtonClick;
				btn_BuyLongInst.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					int qty1 = SelectedSpreadQty;
					if(AutoState == BuyOneSellAnother_AutoState.Close) {
						qty1 = Math.Abs(CurrentSpreadPosition) * SelectedSpreadQty;
					}
					TriggerCustomEvent(o1 =>{
						TradeOneInstrument("Bclick",'B', 0, qty1);
					},0,null);
				};
				btn_SellLongInst.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					int qty1 = SelectedSpreadQty;
					if(AutoState == BuyOneSellAnother_AutoState.Close) {
						qty1 = Math.Abs(CurrentSpreadPosition) * SelectedSpreadQty;
					}
					TriggerCustomEvent(o1 =>{
						TradeOneInstrument("Sclick",'S', 0, qty1);
					},0,null);
				};
				btn_BuyShortInst.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					int qty1 = SelectedSpreadQty;
					if(AutoState == BuyOneSellAnother_AutoState.Close) {
						qty1 = Math.Abs(CurrentSpreadPosition) * SelectedSpreadQty;
					}
					TriggerCustomEvent(o1 =>{
						TradeOneInstrument("Bclick",'B', 1, qty1);
					},0,null);
				};
				btn_SellShortInst.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					int qty1 = SelectedSpreadQty;
					if(AutoState == BuyOneSellAnother_AutoState.Close) {
						qty1 = Math.Abs(CurrentSpreadPosition) * SelectedSpreadQty;
					}
					TriggerCustomEvent(o1 =>{
						TradeOneInstrument("Sclick",'S', 1, qty1);
					},0,null);
				};
				btn_HideExtraButtons.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					ShowExtraBtns = !ShowExtraBtns;
					if(ShowExtraBtns) {
						ButtonSettings[btn_HideExtraButtons_Name].ForeColor = Brushes.White;
						ButtonSettings[btn_HideExtraButtons_Name].BkgColor = Brushes.Black;
//						btn_HideExtraButtons.Foreground = Brushes.White;
//						btn_HideExtraButtons.Background = Brushes.Black;
					}else{
						ButtonSettings[btn_HideExtraButtons_Name].ForeColor = Brushes.Black;
						ButtonSettings[btn_HideExtraButtons_Name].BkgColor = Brushes.White;
//						btn_HideExtraButtons.Foreground = Brushes.Black;
//						btn_HideExtraButtons.Background = Brushes.White;
					}
					ButtonSettings[btn_BuyLongInst_Name       ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_SellLongInst_Name      ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_BuyShortInst_Name      ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_SellShortInst_Name     ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_ShowReversalDots_Name  ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_ShowSpreadHistory_Name   ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_ShowSessionTPO_Name    ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_AccountName_Name       ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
					ButtonSettings[btn_DrawAutoTradeLines_Name ].Vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
//					ButtonSettings[btn_BuyLongInst_Name        ].IsUpdated = false;
//					ButtonSettings[btn_SellLongInst_Name       ].IsUpdated = false;
//					ButtonSettings[btn_BuyShortInst_Name       ].IsUpdated = false;
//					ButtonSettings[btn_SellShortInst_Name      ].IsUpdated = false;
//					ButtonSettings[btn_ShowReversalDots_Name   ].IsUpdated = false;
//					ButtonSettings[btn_ShowSpreadHistory_Name    ].IsUpdated = false;
//					ButtonSettings[btn_ShowSessionTPO_Name     ].IsUpdated = false;
//					ButtonSettings[btn_AccountName_Name        ].IsUpdated = false;
//					ButtonSettings[btn_DrawAutoTradeLines_Name ].IsUpdated = false;
					if(ShowExtraBtns) 
						UpdateExtraButtonsText(0);

					UpdateUI();
					CalculateCurrentSpreadValue(CurrentSpreadPosition);
				};
				btn_ShowReversalDots.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					pShowReversalDots = !pShowReversalDots;
					ButtonSettings[btn_ShowReversalDots_Name].Txt = pShowReversalDots ? "Hide Reversal Dots":"Show Reversal Dots";
					UpdateUI();
					ForceRefresh();
				};
				btn_ShowSpreadHistory.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					pShowSpreadHistory = !pShowSpreadHistory;
					ButtonSettings[btn_ShowSpreadHistory_Name].Txt = pShowSpreadHistory ? "Hide SpreadHistory":"Show SpreadHistory";
					UpdateUI();
					ForceRefresh();
				};
				btn_ShowSessionTPO.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					pShowSessionTPO = !pShowSessionTPO;
					ButtonSettings[btn_ShowSessionTPO_Name].Txt = pShowSessionTPO ? "Hide Sess. TPO":"Show Sess. TPO";
					UpdateUI();
					ForceRefresh();
				};
				btn_ShowSessionTPO.MouseWheel += delegate(object o, MouseWheelEventArgs e){
					e.Handled = true;
					if(e.Delta<0) TPOsize = TPOsize * 1.2f; else TPOsize = Math.Max(0.2f,TPOsize*0.8f);
					ForceRefresh();
				};
				btn_AccountName.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
		            var list = new System.Collections.Generic.List<string>();

					int i = 0;
		            for (i = 0; i < Account.All.Count; i++)
		            {
		                if (Account.All[i].ConnectionStatus == ConnectionStatus.Connected){
//printDebug("...account: '"+Account.All[i].Name+"'");
		                    list.Add(Account.All[i].Name);
						}
		            }
					if(list.Count==0) list.Add("<no connection>");
					i = 0;
					while(i<list.Count && list[i] != pAccountName){
						i++;
					}
					i++;
					if(i>=list.Count) i = 0;
					pAccountName = list[i];
					myAccount = Account.All[i];
					btn_AccountName.Content = pAccountName;
					ButtonSettings[btn_AccountName_Name].Txt = pAccountName;
//printDebug("AccountButton click, pAccountName is: "+pAccountName);
					UpdateUI();
				};
				btn_DrawAutoTradeLines.Click += delegate(object o, RoutedEventArgs e){
					if(e!=null) e.Handled = true;
					if(btn_DrawAutoTradeLines.Content.ToString().Contains("Draw")){
						ButtonSettings[btn_DrawAutoTradeLines_Name].Txt = "Remove AT Lines";
						TriggerCustomEvent(o1 =>{
							btn_DrawAutoTradeLines.Content = ButtonSettings[btn_DrawAutoTradeLines_Name].Txt;
							string tag = string.Format("buy    {0}",uID);
							RemoveDrawObject(tag);
							HorizontalLine hl = Draw.HorizontalLine(this, tag /*string.Format("{0}{1}{2}", Now.Hour, Now.Minute, Now.Second, Now.Millisecond)*/, SLL.GetValueAt(CurrentBars[0]-1), pATBuyLineColor, DashStyleHelper.Dash, pATBuyLineThickness);
							hl.IsLocked = false;
							tag = string.Format("sell    {0}",uID);
							RemoveDrawObject(tag);
							hl = Draw.HorizontalLine(this, tag /*string.Format("{0}{1}{2}", Now.Hour, Now.Minute, Now.Second, Now.Millisecond)*/, SLU.GetValueAt(CurrentBars[0]-1), pATSellLineColor, DashStyleHelper.Dash, pATSellLineThickness);
							hl.IsLocked = false;
						},0,null);
					}else{
						ButtonSettings[btn_DrawAutoTradeLines_Name].Txt = "Draw AT Lines";
						TriggerCustomEvent(o1 =>{
							btn_DrawAutoTradeLines.Content = ButtonSettings[btn_DrawAutoTradeLines_Name].Txt;
							string tag = string.Format("buy    {0}",uID);
							RemoveDrawObject(tag);
							tag = string.Format("sell    {0}",uID);
							RemoveDrawObject(tag);
						},0,null);
					}
				};

				Visibility vis = (ShowExtraBtns ? Visibility.Visible : Visibility.Hidden);
				btn_BuyLongInst.Visibility = vis;
				btn_SellLongInst.Visibility = vis;
				btn_BuyShortInst.Visibility = vis;
				btn_SellShortInst.Visibility = vis;
				btn_ShowReversalDots.Visibility = vis;
				btn_ShowSpreadHistory.Visibility = vis;
				btn_ShowSessionTPO.Visibility = vis;
				btn_AccountName.Visibility = vis;
				btn_DrawAutoTradeLines.Visibility = vis;
				ButtonSettings[btn_BuyLongInst_Name       ].Vis = vis;
				ButtonSettings[btn_SellLongInst_Name      ].Vis = vis;
				ButtonSettings[btn_BuyShortInst_Name      ].Vis = vis;
				ButtonSettings[btn_SellShortInst_Name     ].Vis = vis;
				ButtonSettings[btn_ShowReversalDots_Name  ].Vis = vis;
				ButtonSettings[btn_ShowSpreadHistory_Name].Vis = vis;
				ButtonSettings[btn_ShowSessionTPO_Name    ].Vis = vis;
				ButtonSettings[btn_AccountName_Name       ].Vis = vis;
				ButtonSettings[btn_DrawAutoTradeLines_Name].Vis = vis;
//				ButtonSettings[btn_BuyLongInst_Name       ].IsUpdated = false;
//				ButtonSettings[btn_SellLongInst_Name      ].IsUpdated = false;
//				ButtonSettings[btn_BuyShortInst_Name      ].IsUpdated = false;
//				ButtonSettings[btn_SellShortInst_Name     ].IsUpdated = false;
//				ButtonSettings[btn_AccountName_Name       ].IsUpdated = false;
//				ButtonSettings[btn_ShowReversalDots_Name  ].IsUpdated = false;
//				ButtonSettings[btn_ShowSpreadHistory_Name   ].IsUpdated = false;
//				ButtonSettings[btn_ShowSessionTPO_Name     ].IsUpdated = false;
//				ButtonSettings[btn_DrawAutoTradeLines_Name ].IsUpdated = false;
				int col = 0;
				System.Windows.Controls.Grid.SetColumn(btn_GoFlat, col);
				System.Windows.Controls.Grid.SetColumn(btn_ShowReversalDots, col);
				System.Windows.Controls.Grid.SetColumn(btn_ShowSpreadHistory, col);
				System.Windows.Controls.Grid.SetColumn(btn_ShowSessionTPO, col);
				col = 1;
				System.Windows.Controls.Grid.SetColumn(btn_BuySpread, col);
				System.Windows.Controls.Grid.SetColumn(btn_BuyLongInst, col);
				System.Windows.Controls.Grid.SetColumn(btn_BuyShortInst, col);
				col = 2;
				System.Windows.Controls.Grid.SetColumn(btn_SellSpread, col);
				System.Windows.Controls.Grid.SetColumn(btn_SellLongInst, col);
				System.Windows.Controls.Grid.SetColumn(btn_SellShortInst, col);
				col = 3;
				System.Windows.Controls.Grid.SetColumn(btn_AutoTrade, col);
				col = 4;
				System.Windows.Controls.Grid.SetColumn(btn_HideExtraButtons, col);
				System.Windows.Controls.Grid.SetColumn(btn_AccountName, col);
				System.Windows.Controls.Grid.SetColumn(btn_DrawAutoTradeLines, col);

				int row = 0;
				System.Windows.Controls.Grid.SetRow(btn_GoFlat, row);
				System.Windows.Controls.Grid.SetRow(btn_BuySpread, row);
				System.Windows.Controls.Grid.SetRow(btn_SellSpread, row);
				System.Windows.Controls.Grid.SetRow(btn_AutoTrade, row);
				System.Windows.Controls.Grid.SetRow(btn_HideExtraButtons, row);
				row = 1;
				System.Windows.Controls.Grid.SetRow(btn_ShowSpreadHistory, row);
				System.Windows.Controls.Grid.SetRow(btn_AccountName, row);
				System.Windows.Controls.Grid.SetRow(btn_BuyLongInst, row);
				System.Windows.Controls.Grid.SetRow(btn_SellLongInst, row);
				row = 2;
				System.Windows.Controls.Grid.SetRow(btn_ShowReversalDots, row);
				System.Windows.Controls.Grid.SetRow(btn_BuyShortInst, row);
				System.Windows.Controls.Grid.SetRow(btn_SellShortInst, row);
				System.Windows.Controls.Grid.SetRow(btn_DrawAutoTradeLines, row);
				row = 3;
				System.Windows.Controls.Grid.SetRow(btn_ShowSessionTPO, row);

				MenuGrid.Children.Add(btn_GoFlat);
				MenuGrid.Children.Add(btn_BuySpread);
				MenuGrid.Children.Add(btn_SellSpread);
				MenuGrid.Children.Add(btn_HideExtraButtons);
				MenuGrid.Children.Add(btn_AccountName);
				MenuGrid.Children.Add(btn_ShowSpreadHistory);
				if(IsProVersion){
					MenuGrid.Children.Add(btn_AutoTrade);
					MenuGrid.Children.Add(btn_ShowReversalDots);
					MenuGrid.Children.Add(btn_ShowSessionTPO);
					MenuGrid.Children.Add(btn_DrawAutoTradeLines);
					MenuGrid.Children.Add(btn_BuyLongInst);
					MenuGrid.Children.Add(btn_SellLongInst);
					MenuGrid.Children.Add(btn_BuyShortInst);
					MenuGrid.Children.Add(btn_SellShortInst);
				}
				UserControlCollection.Add(MenuGrid);
			#endregion
		}
		
//================================================
		private void DisposeMenu(){
			#region Dispose Menu
			if (MenuGrid != null)
			{
				if (btn_BuySpread != null)
				{
					MenuGrid.Children.Remove(btn_BuySpread);
					btn_BuySpread.Click -= OnButtonClick;
					btn_BuySpread = null;
				}
				if (btn_SellSpread != null)
				{
					MenuGrid.Children.Remove(btn_SellSpread);
					btn_SellSpread.Click -= OnButtonClick;
					btn_SellSpread = null;
				}
				if (btn_AutoTrade != null)
				{
					MenuGrid.Children.Remove(btn_AutoTrade);
					btn_AutoTrade = null;
				}
				if(btn_BuyLongInst != null){
					MenuGrid.Children.Remove(btn_BuyLongInst);
					btn_BuyLongInst = null;
				}
				if(btn_SellLongInst != null){
					MenuGrid.Children.Remove(btn_SellLongInst);
					btn_SellLongInst = null;
				}
				if(btn_BuyShortInst != null){
					MenuGrid.Children.Remove(btn_BuyShortInst);
					btn_BuyShortInst = null;
				}
				if(btn_SellShortInst != null){
					MenuGrid.Children.Remove(btn_SellShortInst);
					btn_SellShortInst = null;
				}
				if(btn_HideExtraButtons != null){
					MenuGrid.Children.Remove(btn_HideExtraButtons);
					btn_HideExtraButtons = null;
				}
				if(btn_AccountName != null){
					MenuGrid.Children.Remove(btn_AccountName);
					btn_AccountName = null;
				}
				if(btn_DrawAutoTradeLines != null){
					MenuGrid.Children.Remove(btn_DrawAutoTradeLines);
					btn_DrawAutoTradeLines = null;
				}
			}
			#endregion
		}
//================================================
		private string SpreadName;
		public override string ToString()
		{
			string acctname = pAccountName;
//			if(pUseChartTraderAccount){
//				try{
//					var xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
//					acctname = xAlselector.SelectedAccount.ToString();
//				}catch(Exception e){}//printDebug("ChartTrader not found: "+e.ToString());}
//			}
			return string.Format("BOSA: +{0} {1} - {2} {3}  {4}\n{5}", this.pLongContracts, this.BuySymbol,  pShortContracts, SellSymbol, (pAccountName.Contains("Sim") || !IsDebug ? this.pAccountName:"LIVE"), EquityRatioString);
		}
		public override string DisplayName { get { return ToString(); } }

//protected string GetChartId(char IdType)
//{
//	string Identifier = "";
//	Chart chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
//	chartWindow.Dispatcher.InvokeAsync((Action)(() => {
//	if (IdType == 'T')
//	{
//		System.Windows.Controls.TabItem t = chartWindow.MainTabControl.SelectedItem as System.Windows.Controls.TabItem;
//		TabMe = t.Content as ChartTab;
//		Identifier = TabMe.PersistenceId;
////		printDebug("Tab id: "+Identifier);
////		printDebug("Tab.ToString(): "+TabMe.Content.ToString());
////		printDebug("Tab.Instrument: "+TabMe.Instrument.FullName);
////		printDebug("Properties: "+TabMe.Properties.ToString());
////		printDebug("TabName: "+TabMe.TabName);
//	}
//	if (IdType == 'W')
//	{
//		IWorkspacePersistence winPer = chartWindow as IWorkspacePersistence;
//		Identifier = winPer.WorkspaceOptions.PersistenceId;
//	}
//	}));

//	return Identifier;
// }
	#region OIF methods

//	Examples:
//		PLACE;G1135;ES 09-21;BUY;1;LIMIT;1400.25;0;GTC;;;;
//
//		PLACE;SIM101;ES 09-21;SELL;1;LIMIT;1400.25;0;DAY;;;;
//
//		PLACE;G1135;ES 09-21;SELL;1;MARKET;0;0;DAY;;;;
//
//		PLACE;SIM101;ES 09-21;SELL;1;STOP;0;1200.25;DAY;;;;
//
//		PLACE;G1135;ES 09-21;SELL;1;STOPLIMIT;1400.00;1400.25;DAY;OtherEntry;Entry#1;;55


		///CANCEL COMMAND
		/// CANCEL;;;;;;;;;;<ORDER ID>;;[STRATEGY ID]

		/// CANCELALLORDERS COMMAND
		/// CANCELALLORDERS;;;;;;;;;;;;

		/// CHANGE COMMAND
		/// CHANGE;;;;<QUANTITY>;;<LIMIT PRICE>;<STOP PRICE>;;;<ORDER ID>;;[STRATEGY ID]
		/// 
		/// CLOSEPOSITION COMMAND
		/// CLOSEPOSITION;<ACCOUNT>;<INSTRUMENT>;;;;;;;;;;
		/// 
		/// CLOSESTRATEGY COMMAND
		/// CLOSESTRATEGY;;;;;;;;;;;;<STRATEGY ID>
		/// 
		/// FLATTENEVERYTHING COMMAND
		/// FLATTENEVERYTHING;;;;;;;;;;;;
		/// 
		/// PLACE COMMAND
		/// PLACE;<ACCOUNT>;<INSTRUMENT>;<ACTION>;<QTY>;<ORDER TYPE>;[LIMIT PRICE];[STOP PRICE];<TIF>;[OCO ID];[ORDER ID];[STRATEGY];[STRATEGY ID]
		/// 
		/// REVERSEPOSITION COMMAND
		/// REVERSEPOSITION;<ACCOUNT>;<INSTRUMENT>;<ACTION>;<QTY>;<ORDER TYPE>;[LIMIT PRICE];[STOP PRICE];<TIF>;[OCO ID];[ORDER ID];[STRATEGY];[STRATEGY ID]


//=====================================================================
		private int OIF_Submit (string[] instruction, int OIFnumber)
		{  //returns the OIF file number
			string full_instruction = string.Empty;
			foreach(var ins in instruction){
				full_instruction = string.Concat(full_instruction, ins);
			}
			return OIF_Submit(full_instruction, OIFnumber);
		}
		private int OIF_Submit (string instruction, int OIFnumber)
		{  //returns the OIF file number
			if(OIFnumber > 500) OIFnumber=0; else OIFnumber++;
			if(TestSend) OIF_file_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			else		 OIF_file_path = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"incoming");
			string fname = System.IO.Path.Combine(OIF_file_path, StripOutIllegalCharacters(string.Format("OIF_BOSA{0}{1}{2}.txt", BuySymbol, SellSymbol, DateTime.Now.Ticks.ToString("0")),' '));
			System.IO.File.AppendAllText(fname, instruction);
//printDebug(String.Concat("OIF_Submit:  ",fname,"\n\t",instruction,Environment.NewLine));
			Log("OIF File written: "+fname, LogLevel.Information);
			return OIFnumber;
		}
//=====================================================================
		private string OIF_CancelOrder (string OrderId, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory OrderId was invalid
		// CANCEL;;;;;;;;;;<ORDER ID>;;[STRATEGY ID]
			if(OrderId.Length==0) return string.Empty;
			else
				return "CANCEL;;;;;;;;;;"+OrderId+";;"+StrategyId+Environment.NewLine;
		}

		private string OIF_CancelAllOrders ()
		{  //returns the OIF instruction
		// CANCELALLORDERS;;;;;;;;;;;;
			return "CANCELALLORDERS;;;;;;;;;;;;"+Environment.NewLine;
		}

		private string OIF_ChangeOrder (int Qty, double LimitPrice, double StopPrice, string OrderId, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
		// CHANGE;;;;<QUANTITY>;;<LIMIT PRICE>;<STOP PRICE>;;;<ORDER ID>;;[STRATEGY ID]
			if(Qty<=0 || LimitPrice < 0 || StopPrice < 0 || OrderId.Length==0) return string.Empty;
			else
				return "CHANGE;;;;"+Qty.ToString()+";;"+LimitPrice.ToString()+";"+StopPrice.ToString()+";;;"+OrderId+";;"+StrategyId+Environment.NewLine;
		}

		private string OIF_ChangeOrder (double Qty, double LimitPrice, double StopPrice, string OrderId, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
		// CHANGE;;;;<QUANTITY>;;<LIMIT PRICE>;<STOP PRICE>;;;<ORDER ID>;;[STRATEGY ID]
			if(Qty<=0 || LimitPrice < 0 || StopPrice < 0 || OrderId.Length==0) return string.Empty;
			else
				return "CHANGE;;;;"+Qty.ToString()+";;"+LimitPrice.ToString()+";"+StopPrice.ToString()+";;;"+OrderId+";;"+StrategyId+Environment.NewLine;
		}

		private string OIF_ClosePosition (string Account, string Instrument)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
		// CLOSEPOSITION;<ACCOUNT>;<INSTRUMENT>;;;;;;;;;;
			if(Account.Length == 0 || Instrument.Length==0) return string.Empty;
			else
				return "CLOSEPOSITION;"+Account+";"+Instrument+";;;;;;;;;;"+Environment.NewLine;
		}
		private string OIF_CloseStrategy (string StrategyId)
		{
		// CLOSESTRATEGY;;;;;;;;;;;;<STRATEGY ID>
			return "CLOSESTRATEGY;;;;;;;;;;;;"+StrategyId+Environment.NewLine;
		}
		private string OIF_FlattenEverything ()
		{
		// FLATTENEVERYTHING;;;;;;;;;;;;
			return "FLATTENEVERYTHING;;;;;;;;;;;;"+Environment.NewLine;
		}
		private string OIF_PlaceOrder (string Account, string Instrument, string Action, int Qty, string OrderType, double LimitPrice, double StopPrice, string TIF, string OCOId, string OrderId, string Strategy, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
			// PLACE;<ACCOUNT>;<INSTRUMENT>;<ACTION>;<QTY>;<ORDER TYPE>;[LIMIT PRICE];[STOP PRICE];<TIF>;[OCO ID];[ORDER ID];[STRATEGY];[STRATEGY ID]
			// REQUIRED INPUTS:  Account, Instrument, Action, Qty, OrderType, TIF
			// OPTIONAL INPUTS:  LimitPrice, StopPrice, OCOid, OrderId, Strategy, StrategyId
			// 
			// Instrument:  "ES 09-08" or "$GBPJPY"
			// Action:  BUY or SELL
			// OrderType: MARKET, LIMIT, STOP, STOPLIMIT
			// TIF:  GTC or DAY

			if(Qty<=0 || OrderType.Length==0 || TIF.Length==0 || Action.Length==0 || Account.Length == 0 || Instrument.Length==0) return string.Empty;
			else
			{
				Account = Account.ToUpper();
				Instrument = Instrument.ToUpper();
				Action = Action.ToUpper();
				if(Action.CompareTo("BUY") != 0 && Action.CompareTo("SELL") != 0) return string.Empty;
				OrderType = OrderType.ToUpper();
				if(OrderType.CompareTo("MARKET") != 0 && OrderType.CompareTo("LIMIT") != 0 && OrderType.CompareTo("STOP") != 0 && OrderType.CompareTo("STOPLIMIT") != 0) return string.Empty;
				TIF = TIF.ToUpper();
				if(TIF.CompareTo("GTC") != 0 && TIF.CompareTo("DAY") != 0) return string.Empty;
				return "PLACE;"+Account+";"+Instrument+";"+Action+";"+Qty.ToString()+";"+OrderType+";"+LimitPrice.ToString()+";"+StopPrice.ToString()+";"+TIF+";"+OCOId+";"+OrderId+";"+Strategy+";"+StrategyId+Environment.NewLine;
			}
		}
		private string OIF_PlaceOrder (string Account, string Instrument, string Action, double Qty, string OrderType, double LimitPrice, double StopPrice, string TIF, string OCOId, string OrderId, string Strategy, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
			// PLACE;<ACCOUNT>;<INSTRUMENT>;<ACTION>;<QTY>;<ORDER TYPE>;[LIMIT PRICE];[STOP PRICE];<TIF>;[OCO ID];[ORDER ID];[STRATEGY];[STRATEGY ID]
			// REQUIRED INPUTS:  Account, Instrument, Action, Qty, OrderType, TIF
			// OPTIONAL INPUTS:  LimitPrice, StopPrice, OCOid, OrderId, Strategy, StrategyId
			// 
			// Instrument:  "ES 09-21" or "$GBPJPY"
			// Action:  BUY or SELL
			// OrderType: MARKET, LIMIT, STOP, STOPLIMIT
			// TIF:  GTC or DAY

			if(Qty<=0 || OrderType.Length==0 || TIF.Length==0 || Action.Length==0 || Account.Length == 0 || Instrument.Length==0) return string.Empty;
			else
			{
				Account = Account.ToUpper();
				Instrument = Instrument.ToUpper();
				Action = Action.ToUpper();
				if(Action.CompareTo("BUY") != 0 && Action.CompareTo("SELL") != 0) return string.Empty;
				OrderType = OrderType.ToUpper();
				if(OrderType.CompareTo("MARKET") != 0 && OrderType.CompareTo("LIMIT") != 0 && OrderType.CompareTo("STOP") != 0 && OrderType.CompareTo("STOPLIMIT") != 0) return string.Empty;
				TIF = TIF.ToUpper();
				if(TIF.CompareTo("GTC") != 0 && TIF.CompareTo("DAY") != 0) return string.Empty;
				return "PLACE;"+Account+";"+Instrument+";"+Action+";"+Qty.ToString()+";"+OrderType+";"+LimitPrice.ToString()+";"+StopPrice.ToString()+";"+TIF+";"+OCOId+";"+OrderId+";"+Strategy+";"+StrategyId+Environment.NewLine;
			}
		}
		private string OIF_ReversePosition (string Account, string Instrument, string Action, int Qty, string OrderType, double LimitPrice, double StopPrice, string TIF, string OCOId, string OrderId, string Strategy, string StrategyId)
		{    //returns the OIF instruction, or the empty string is the mandatory fields were invalid
			// REVERSEPOSITION;<ACCOUNT>;<INSTRUMENT>;<ACTION>;<QTY>;<ORDER TYPE>;[LIMIT PRICE];[STOP PRICE];<TIF>;[OCO ID];[ORDER ID];[STRATEGY];[STRATEGY ID]
			// REQUIRED INPUTS:  Account, Instrument, Action, Qty, OrderType, TIF
			// OPTIONAL INPUTS:  LimitPrice, StopPrice, OCOid, OrderId, Strategy, StrategyId
			// 
			// Instrument:  "ES 09-08" or "$GBPJPY"
			// Action:  BUY or SELL
			// OrderType: MARKET, LIMIT, STOP, STOPLIMIT
			// TIF:  GTC or DAY

			if(Qty<=0 || OrderType.Length==0 || TIF.Length==0 || Action.Length==0 || Account.Length == 0 || Instrument.Length==0) return string.Empty;
			else
			{
				Account = Account.ToUpper();
				Instrument = Instrument.ToUpper();
				Action = Action.ToUpper();
				if(Action.CompareTo("BUY") != 0 && Action.CompareTo("SELL") != 0) return string.Empty;
				OrderType = OrderType.ToUpper();
				if(OrderType.CompareTo("MARKET") != 0 && OrderType.CompareTo("LIMIT") != 0 && OrderType.CompareTo("STOP") != 0 && OrderType.CompareTo("STOPLIMIT") != 0) return string.Empty;
				TIF = TIF.ToUpper();
				if(TIF.CompareTo("GTC") != 0 && TIF.CompareTo("DAY") != 0) return string.Empty;
				return "REVERSEPOSITION;"+Account+";"+Instrument+";"+Action+";"+Qty.ToString()+";"+OrderType+";"+LimitPrice.ToString()+";"+StopPrice.ToString()+";"+TIF+";"+OCOId+";"+OrderId+";"+Strategy+";"+StrategyId+Environment.NewLine;
			}
//=====================================================================

    }
	#endregion
		private double Round2Tick(double p, int bip){
			int x = Convert.ToInt32(p / Instruments[bip].MasterInstrument.TickSize);
			return x * Instruments[bip].MasterInstrument.TickSize;
		}
//===================                      =======================================================================================
		int xline=0;
		private void UpdateAutoTradeButton (BuyOneSellAnother_AutoState astate){
xline=913;
printDebug("1812   UpdateAutoTradeButton");
			#region -- UpdateAutoTradeButton --
			AutoState = astate;//global variable AutoState
try{
			if(astate == BuyOneSellAnother_AutoState.Off){
xline=1017;//printDebug(xline);
				AutoTradeEnabled = false;
xline=1018;//printDebug(xline);
				ButtonSettings[btn_AutoTrade_Name].Txt       = "Auto Off";
//				ButtonSettings[btn_AutoTrade_Name].IsUpdated = false;
xline=1020;//printDebug(xline);
				ButtonSettings[btn_BuySpread_Name].Enabled   = true;
//				ButtonSettings[btn_BuySpread_Name].IsUpdated = false;
xline=1021;//printDebug(xline);
				ButtonSettings[btn_SellSpread_Name].Enabled   = true;
//				ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
			}else if (astate == BuyOneSellAnother_AutoState.Close){
xline=1025;//printDebug(xline);
				if(CurrentSpreadPosition == 0){
xline=1027;//printDebug(xline);
					AutoState = BuyOneSellAnother_AutoState.Off;
					AutoTradeEnabled = false;
					ButtonSettings[btn_AutoTrade_Name].Txt        = "Auto Off";
					ButtonSettings[btn_BuySpread_Name].Enabled    = true;
					ButtonSettings[btn_SellSpread_Name].Enabled   = true;
//					ButtonSettings[btn_AutoTrade_Name].IsUpdated  = false;
//					ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//					ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
				}else{
xline=1034;//printDebug(xline);
					AutoTradeEnabled = true;
					ButtonSettings[btn_AutoTrade_Name].Txt = "Auto Close";
					if(CurrentSpreadPosition > 0){
						ButtonSettings[btn_BuySpread_Name].Enabled  = false;
						ButtonSettings[btn_SellSpread_Name].Enabled = true;
					}else{
						ButtonSettings[btn_BuySpread_Name].Enabled  = true;
						ButtonSettings[btn_SellSpread_Name].Enabled = false;
					}
//					ButtonSettings[btn_AutoTrade_Name].IsUpdated  = false;
//					ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//					ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
				}
			}else if (astate == BuyOneSellAnother_AutoState.Add){
xline=1045;//printDebug(xline);
				AutoTradeEnabled = true;
				ButtonSettings[btn_AutoTrade_Name].Txt = "Auto Add";
				if(CurrentSpreadPosition > 0){
					AutoState = BuyOneSellAnother_AutoState.Off;
					ButtonSettings[btn_BuySpread_Name].Enabled  = true;
					ButtonSettings[btn_SellSpread_Name].Enabled = false;
				}else if(CurrentSpreadPosition < 0){
					ButtonSettings[btn_BuySpread_Name].Enabled  = false;
					ButtonSettings[btn_SellSpread_Name].Enabled = true;
				}else{
					ButtonSettings[btn_BuySpread_Name].Enabled  = true;
					ButtonSettings[btn_SellSpread_Name].Enabled = true;
				}
//				ButtonSettings[btn_AutoTrade_Name].IsUpdated  = false;
//				ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//				ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
			}else if (astate == BuyOneSellAnother_AutoState.BuyOnly){
xline=1065;//printDebug(xline);
				AutoTradeEnabled = true;
				ButtonSettings[btn_AutoTrade_Name].Txt        = "Auto BUY";
				ButtonSettings[btn_BuySpread_Name].Enabled    = true;
				ButtonSettings[btn_SellSpread_Name].Enabled   = false;
//				ButtonSettings[btn_AutoTrade_Name].IsUpdated  = false;
//				ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//				ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
			}else if (astate == BuyOneSellAnother_AutoState.SellOnly){
xline=1071;//printDebug(xline);
				AutoTradeEnabled = true;
				ButtonSettings[btn_AutoTrade_Name].Txt        = "Auto SELL";
				ButtonSettings[btn_BuySpread_Name].Enabled    = false;
				ButtonSettings[btn_SellSpread_Name].Enabled   = true;
//				ButtonSettings[btn_AutoTrade_Name].IsUpdated  = false;
//				ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//				ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
			}
xline=1077;//printDebug(xline);
			ButtonSettings[btn_AutoTrade_Name].BkgColor = AutoTradeEnabled ? Brushes.Orange:Brushes.Gray;
			double qty = SelectedSpreadQty;
			if(AutoState == BuyOneSellAnother_AutoState.Close) {
				qty = Math.Abs(CurrentSpreadPosition);
			}
			#region -- change button color based on IsEnabled --
			if(ButtonSettings[btn_BuySpread_Name].Enabled){
xline=1086;//printDebug(xline);
				ButtonSettings[btn_BuySpread_Name].BkgColor  = Brushes.Green;
				ButtonSettings[btn_BuySpread_Name].Txt       = string.Format("BUY {0}x", qty);
//				ButtonSettings[btn_BuySpread_Name].IsUpdated = false;
			}else{
xline=1090;//printDebug(xline);
				ButtonSettings[btn_BuySpread_Name].BkgColor  = Brushes.DimGray;
				ButtonSettings[btn_BuySpread_Name].Txt       = "n/a";
//				ButtonSettings[btn_BuySpread_Name].IsUpdated = false;
			}
			if(ButtonSettings[btn_SellSpread_Name].Enabled){
xline=1097;//printDebug(xline);
				ButtonSettings[btn_SellSpread_Name].BkgColor  = Brushes.Red;
				ButtonSettings[btn_SellSpread_Name].Txt       = string.Format("SELL {0}x", qty);
//				ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
			}else{
xline=1101;//printDebug(xline);
				ButtonSettings[btn_SellSpread_Name].BkgColor  = Brushes.DimGray;
				ButtonSettings[btn_SellSpread_Name].Txt       = "n/a";
//				ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
			}
			#endregion
xline=1105;//printDebug(xline);
			UpdateUI();
			ForceRefresh();
}catch(Exception e2){printDebug("xLine: "+xline+":  "+BuySymbol+"-"+SellSymbol+"  UpdateAutoTradeButton error 2: "+e2.ToString());}
			#endregion
		}

		private OrderAction DetermineOrderAction(MarketPosition mp, char Direction){
			var action = OrderAction.Buy;
			if(     mp==MarketPosition.Long  && Direction=='B') action = OrderAction.Buy;
			else if(mp==MarketPosition.Long  && Direction=='S') action = OrderAction.Sell;
			else if(mp==MarketPosition.Short && Direction=='B') action = OrderAction.BuyToCover;
			else if(mp==MarketPosition.Short && Direction=='S') action = OrderAction.SellShort;
			else if(mp==MarketPosition.Flat  && Direction=='B') action = OrderAction.Buy;
			else if(mp==MarketPosition.Flat  && Direction=='S') action = OrderAction.SellShort;
			return action;
		}
//===================                  =======================================================================================
		private void TradeOneInstrument (string ID, char Direction, int InstId, int Qty){
			#region -- TradeOneInstrument --
//			SourceOfTrade = uID;
//			if (ChartControl.Dispatcher.CheckAccess()) {
				GetCurrentSelectedAccount();
				int pos_size = 0;
				var mp = MarketPosition.Flat;
				foreach(var ex in myAccount.Positions.Where(k=>k.Instrument == Instruments[InstId])) {
					pos_size = pos_size + ex.Quantity;
					mp = ex.MarketPosition;
				}

				var OrdersList = new List<Order>();
				OrderAction action = DetermineOrderAction(mp, Direction);
printDebug(Direction+":  TradeOneInstrument on "+Instruments[InstId].FullName+"  action: "+action.ToString()+"  Quantity: "+Qty);
printDebug("  Is MyAccount == null? "+(myAccount==null?" YES" : " NO"));
if(myAccount!=null){
				printDebug("Account is:  "+myAccount.Name);
				if(string.IsNullOrEmpty(ID)) ID = action.ToString();
				OrdersList.Add(myAccount.CreateOrder(
						Instrument.GetInstrument(Instruments[InstId].FullName), 
						action, 
						OrderType.Market, 
						OrderEntry.Automated, 
						TimeInForce.Day,
						Qty,
						0, 0, string.Empty, ID, Core.Globals.MaxDate, null));
				myAccount.Submit(OrdersList.ToArray());
}
				ForceRefresh();
//			}else{
//				ChartControl.Dispatcher.InvokeAsync((Action)(() =>
//    	        {
//line=529;
//					GetCurrentSelectedAccount();
//					int pos_size = 0;
//					var mp = MarketPosition.Flat;
//					foreach(var ex in myAccount.Positions.Where(k=>k.Instrument == Instruments[InstId])) {
//						pos_size = pos_size + ex.Quantity;
//						mp = ex.MarketPosition;
//					}

//					var OrdersList = new List<Order>();
//					OrderAction action = DetermineOrderAction(mp, Direction);
//printDebug(Direction+":  (dispatcher) TradeOneInstrument on "+Instruments[InstId].FullName+"  action: "+action.ToString()+"  Quantity: "+Qty);
//					OrdersList.Add(myAccount.CreateOrder(
//								Instrument.GetInstrument(Instruments[InstId].FullName), 
//								action, 
//								OrderType.Market, 
//								OrderEntry.Automated, 
//								TimeInForce.Day,
//								Qty,
//								0, 0, string.Empty, ID, Core.Globals.MaxDate, null));
//					myAccount.Submit(OrdersList.ToArray()); 
//					ForceRefresh();
//				}));
//			}
			#endregion
		}
		#region -- GoLong and GoShort execution methods
		private void GoLong_Execution(bool GoFlat, string entryID)	{
			int LongContracts = 0;
			int ShortContracts = 0;
			int qty = -1;
printDebug("GoLong_Execution...current spread position: "+CurrentSpreadPosition+"  CurrentLongSize: "+CurrentLongSize+" ShortSize: "+CurrentShortSize);
			GetCurrentSelectedAccount();
			if(myAccount == null) {printDebug("Account is NULL, no trades here");return;}
			if(GoFlat) {
printDebug("GoFlat is TRUE");
				if(CurrentLongSize <= 0){
printDebug("Recalculating current spread position since CurrentLongSize is: "+CurrentLongSize);
					bool dummy = false;
					CalculateSpreadPosition(false, ref dummy, ref CurrentLongSize, ref CurrentShortSize);
				}
				LongContracts  = CurrentLongSize;
				ShortContracts = CurrentShortSize;
printDebug("Auto close qty: Long: "+LongContracts+"  Short:"+ShortContracts);
			}else{
printDebug("GoFlat is FALSE");
//				NinjaTrader.Gui.Tools.QuantityUpDown quantitySelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as NinjaTrader.Gui.Tools.QuantityUpDown);
//				SelectedSpreadQty = quantitySelector.Value;
				LongContracts = SelectedSpreadQty * pLongContracts;
				ShortContracts = SelectedSpreadQty * pShortContracts;
printDebug("Manual qty: Long: "+LongContracts+"  Short:"+ShortContracts);
			}
line=595;
			ButtonSettings[btn_BuySpread_Name].Txt  = string.Format("BUY {0}x", qty);
			ButtonSettings[btn_SellSpread_Name].Txt = string.Format("SELL {0}x", qty);
//			ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//			ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
line=599;printDebug("Calling UpdateUI now...");
			UpdateUI();

line=600;
		//printDebug(xAlselector.SelectedAccount.ToString()+" qty: "+quantitySelector.Value);
//		EnterLong();
		#region -- OIF --
//					string cmdL = OIF_PlaceOrder(
//						xAlselector.SelectedAccount.ToString(),
//						Instruments[0].FullName,
//						"BUY",
//						pLongContracts*SelectedSpreadQty,
//						"MARKET",
//						0,
//						0,
//						"GTC",
//						string.Empty,
//						string.Empty,//"L-Spread+"+BuySymbol+"-"+SellSymbol,//orderid
//						string.Empty,//strategy
//						string.Empty);//strategy id
//					string cmdS = OIF_PlaceOrder(
//						xAlselector.SelectedAccount.ToString(),
//						Instruments[1].FullName,
//						"SELL",
//						pShortContracts*SelectedSpreadQty,
//						"MARKET",
//						0,
//						0,
//						"GTC",
//						string.Empty,
//						string.Empty,//"S-Spread+"+BuySymbol+"-"+SellSymbol,//orderid
//						string.Empty,//strategy
//						string.Empty);//strategy id
//					OIFnumber = OIF_Submit(new string[]{cmdL,cmdS}, OIFnumber);
//printDebug("L: "+cmdL);
//printDebug("S: "+cmdS);
					#endregion
line=632;
//			if(TabMe!=null){
//				entryID = string.Format("{0} {1}",TabMe.Name,entryID);
//			}
try{
			var mp = MarketPosition.Flat;
			if(CurrentSpreadPosition >0)      mp = MarketPosition.Long;
			else if(CurrentSpreadPosition >0) mp = MarketPosition.Short;
			var action = DetermineOrderAction(mp, 'B');

line=644;
printDebug("\n\nCreating OrderList:  Instruments[0]: "+(Instruments[0]==null ? "Instruments array is NULL" : Instruments[0].FullName));
printDebug("  myAccount: "+(myAccount==null? "IS NULL!":myAccount.Name));
printDebug("  Action: "+(action==null? "IS NULL!":action.ToString()));
printDebug("  Qty: "+LongContracts);
printDebug("  Core.Globals.MaxDate: "+Core.Globals.MaxDate);
printDebug("Order created....adding it to list");
			var OrdersList = new List<Order>();
			OrdersList.Add(myAccount.CreateOrder(
					Instruments[0], 
					action, 
					OrderType.Market, 
					OrderEntry.Automated, 
					TimeInForce.Day,
					LongContracts,
					0, 0, string.Empty, "B:"+entryID, Core.Globals.MaxDate, null));
line=655;printDebug("...first order is to go "+action.ToString()+" on "+Instruments[0].FullName);
			mp = MarketPosition.Flat;
			if(CurrentSpreadPosition >0)      mp = MarketPosition.Long;
			else if(CurrentSpreadPosition >0) mp = MarketPosition.Short;
			action = DetermineOrderAction(mp, 'S');
			OrdersList.Add(myAccount.CreateOrder(
					Instruments[1], 
					//OrderAction.Sell
					action, 
					OrderType.Market, 
					OrderEntry.Automated, 
					TimeInForce.Day,
					ShortContracts,
					0, 0, string.Empty, "S:"+entryID, Core.Globals.MaxDate, null));
line=656;printDebug("...second order is to go "+action.ToString()+" on "+Instruments[1]);
			myAccount.Submit(OrdersList.ToArray());
}catch(Exception eee){
	Log("Error with your LONG entry order...check your account name in the Ctrl-i dialog",LogLevel.Alert);
	Print("\n"+eee.ToString());
}
		}
		private void GoShort_Execution(bool GoFlat, string entryID){
			int LongContracts = 0;
			int ShortContracts = 0;
			int qty = -1;
			GetCurrentSelectedAccount();
			if(myAccount == null) {printDebug("Account is NULL, no trades here");return;}
line=681;
			if(GoFlat) {
//							qty = Math.Abs(CurrentSpreadPosition);
				if(CurrentLongSize <= 0){
					bool dummy = false;
					CalculateSpreadPosition(false, ref dummy, ref CurrentLongSize, ref CurrentShortSize);
				}
				LongContracts = CurrentLongSize;
				ShortContracts = CurrentShortSize;
				printDebug("Auto close qty: Long: "+LongContracts+"  Short:"+ShortContracts);
			}else{
				LongContracts = SelectedSpreadQty * pLongContracts;
				ShortContracts = SelectedSpreadQty * pShortContracts;
				printDebug("Manual qty: Long: "+LongContracts+"  Short:"+ShortContracts);
			}
line=695;//printDebug(line.ToString());
			ButtonSettings[btn_BuySpread_Name].Txt  = string.Format("BUY {0}x", qty);
			ButtonSettings[btn_SellSpread_Name].Txt = string.Format("SELL {0}x", qty);
//			ButtonSettings[btn_BuySpread_Name].IsUpdated  = false;
//			ButtonSettings[btn_SellSpread_Name].IsUpdated = false;
line=699;//printDebug(line.ToString());
			UpdateUI();
		//printDebug(xAlselector.SelectedAccount.ToString()+" qty: "+quantitySelector.Value);
			#region -- OIF --
//					string cmdS = OIF_PlaceOrder(
//						xAlselector.SelectedAccount.ToString(),
//						Instruments[0].FullName,
//						"SELL",
//						pLongContracts*SelectedSpreadQty,
//						"MARKET",
//						0,
//						0,
//						"GTC",
//						string.Empty,
//						string.Empty,//"S-Spread+"+BuySymbol+"-"+SellSymbol,//orderid
//						string.Empty,//strategy
//						string.Empty);//strategy id
//					string cmdL = OIF_PlaceOrder(
//						xAlselector.SelectedAccount.ToString(),
//						Instruments[1].FullName,
//						"BUY",
//						pShortContracts*SelectedSpreadQty,
//						"MARKET",
//						0,
//						0,
//						"GTC",
//						string.Empty,
//						string.Empty,//"L-Spread+"+BuySymbol+"-"+SellSymbol,//orderid
//						string.Empty,//strategy
//						string.Empty);//strategy id
//					OIFnumber = OIF_Submit(new string[]{cmdL,cmdS}, OIFnumber);
//printDebug("L: "+cmdL);
//printDebug("S: "+cmdS);
		#endregion
line=719;//printDebug(line.ToString());
//			if(TabMe!=null){
//				entryID = string.Format("{0} {1}",TabMe.Name,entryID);
//			}
try{
			var OrdersList = new List<Order>();
			var mp = MarketPosition.Flat;
			if(CurrentSpreadPosition >0)      mp = MarketPosition.Long;
			else if(CurrentSpreadPosition >0) mp = MarketPosition.Short;
line=725;//printDebug(line.ToString());
			var action = DetermineOrderAction(mp, 'S');
line=726;//printDebug(line.ToString()+" account: "+(myAccount==null ? "null":myAccount.Name));
			OrdersList.Add(myAccount.CreateOrder(
					Instruments[0], 
//					Instrument.GetInstrument(Instruments[0].FullName), 
					//OrderAction.SellShort, 
					action, 
					OrderType.Market, 
					OrderEntry.Automated, 
					TimeInForce.Day,
					LongContracts,
					0, 0, string.Empty, "S:"+entryID, Core.Globals.MaxDate, null));
line=750;//printDebug(line.ToString());
			mp = MarketPosition.Flat;
			if(CurrentSpreadPosition >0)      mp = MarketPosition.Long;
			else if(CurrentSpreadPosition >0) mp = MarketPosition.Short;
			action = DetermineOrderAction(mp, 'B');
			OrdersList.Add(myAccount.CreateOrder(
					Instruments[1], 
					//OrderAction.Buy, 
					action, 
					OrderType.Market, 
					OrderEntry.Automated, 
					TimeInForce.Day,
					ShortContracts,
					0, 0, string.Empty, "B:"+entryID, Core.Globals.MaxDate, null));
line=760;//printDebug(line.ToString());
			myAccount.Submit(OrdersList.ToArray());
}catch(Exception eee){
	Log("Error with your SHORT entry order...check your account name in the Ctrl-i dialog",LogLevel.Alert);
	Print("\n"+eee.ToString());
}
		}
		#endregion
//===================              ==============================================================
		private void OnButtonClick (object sender, RoutedEventArgs rea)
		{
printDebug("OnButtonClick...");
			if(rea!=null) rea.Handled = true;
			#region -- OnButtonClick --
			bool GoLong = false;
			bool GoShort = false;
			bool IsUserClick = false;
			string entryID = string.Empty;
			bool GoFlat = AutoState == BuyOneSellAnother_AutoState.Close;
//			SourceOfTrade = uID;
line=549;
printDebug("");
			if(sender is string){
				var s = (string)sender;
				if(s.StartsWith("L")) GoLong = true;
				else if(s.StartsWith("S")) GoShort = true;
				entryID = AutoState.ToString();
printDebug("OnButtonClick  sender is Auto trader "+s+"   EntryID is: '"+entryID+"'");
			}else{
line=556;
//				System.Windows.Controls.Button button = null;
				var button = sender as System.Windows.Controls.Button;
				GoShort = button == btn_SellSpread;
				GoLong  = button == btn_BuySpread;
				IsUserClick = true;
				entryID = "Click"+(GoLong? "L":"S");
				GoFlat = this.AutoState == BuyOneSellAnother_AutoState.Close; //if buy or sell was clicked while AutoClose was enabled, then the user wants to go flat right now
printDebug("OnButtonClick  sender is a button click, EntryID is '"+entryID+"'");
			}
printDebug("Conditions:");
printDebug("   Spread[0]: "+Spread[0]);
printDebug("    BuyLvl: "+HLine_BuyValue+"    SellLvl: "+HLine_SellValue);
printDebug("    Long Ask: "+PlusAsk +"   bid: "+PlusBid);
printDebug("    Short Ask: "+MinusAsk +"   bid: "+MinusBid);
			if(GoLong){
printDebug("  "+BuySymbol+" ask - "+SellSymbol+" bid = "+(PlusAsk-MinusBid));
printDebug("Go long ");
				if (ChartControl.Dispatcher.CheckAccess()) {
printDebug("This is on the UI thread");
					TriggerCustomEvent(o1 =>{
						GoLong_Execution(GoFlat, entryID);
						try{	ForceRefresh();		}catch(Exception e){printDebug("refresh issue: "+e.ToString());}
					},0,null);
				}else{
printDebug("This is on the chart thread");
					GoLong_Execution(GoFlat, entryID);
					try{	ForceRefresh();		}catch(Exception e){printDebug("refresh issue: "+e.ToString());}
				}
			}
			
			if (GoShort){
printDebug("  "+SellSymbol+" ask - "+BuySymbol+" bid = "+(MinusAsk-PlusBid));
printDebug("Go short ");
                //You have to put the stuff below within this ChartC ontrol.Dispatcher.InvokeAsync((Action)(() =>, because you are trying to access something on a differ ent thread.
				if (ChartControl.Dispatcher.CheckAccess()) {
printDebug("This is on the UI thread");
					TriggerCustomEvent(o1 =>{
						GoShort_Execution(GoFlat, entryID);
						try{	ForceRefresh();		}catch(Exception e){printDebug("refresh issue: "+e.ToString());}
					},0,null);
				}else{
printDebug("This is on the chart thread");
					GoShort_Execution(GoFlat, entryID);
					try{	ForceRefresh();		}catch(Exception e){printDebug("refresh issue: "+e.ToString());}
				}
			}
			#endregion
line=742;
		}
//===================                         ==============================================================
		private void GetCurrentSelectedAccount (){  //myAccount is a global variable
			#region -- GetCurrentSelectedAccount --
			string acctname = pAccountName;
//			if(pUseChartTraderAccount){
//				try{
//					var xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
//					acctname = xAlselector.SelectedAccount.ToString();
//				}catch(Exception e){}//printDebug("ChartTrader not found: "+e.ToString());}
//			}
			if(AccountName.CompareTo(acctname) != 0) {//if the new account name is not the current account name...find the account
				#region Verify account availability
				var accts = Account.All.ToList();
Print(string.Format("Checking user selected account: '{0}'  against available accounts of:",acctname.Trim().ToLower()));
				for(int i = 0; i<accts.Count; i++){
Print(string.Format(" {0}: '{1}'",i, accts[i].Name.ToLower()));
//					if(acctname.Trim().ToLower().StartsWith(accts[i].Name.ToLower())){
					if(acctname.Trim().ToLower() == accts[i].Name.ToLower()) {
						if(myAccount != null){
							myAccount.ExecutionUpdate -= OnExecutionUpdate;
							myAccount.PositionUpdate -= OnPositionUpdate;
							myAccount = null;
						}
						lock (Account.All){
							AccountName  = accts[i].Name;
							pAccountName = AccountName;
							myAccount    = accts[i];
							myAccount.ExecutionUpdate += OnExecutionUpdate;
							myAccount.PositionUpdate += OnPositionUpdate;
							break;
						}
					}
				}
				#endregion
			}

			if(myAccount == null || acctname.IsNullOrEmpty()){
				StatusMsg = "Could not connect to account: "+(acctname.IsNullOrEmpty() ? " *undefined* ":acctname)+" - no trades are possible";
				Log("Could not connect to account name: "+(acctname.IsNullOrEmpty() ? " *undefined* ":acctname)+", no trades possible", LogLevel.Alert);
			}
			else {
				StatusMsg = DetermineStatusMsg(AutoState);
				Print(this.ToString()+"   Trading on account:  "+AccountName);
			}
			#endregion
		}
//===================                =======================================================================================
		private void OnExecutionUpdate(object sender, ExecutionEventArgs e)//(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			Print("*************  OnExecutionUpdate: "+e.ToString());//.FullName+"  id: "+executionId+"  price: "+price+"  qty: "+qty+"  MP: "+marketPosition.ToString()+"  orderId: "+orderId+"   dt: "+time.ToString());
		}
		private void OnPositionUpdate (object sender, PositionEventArgs e)
		{
			#region -- OnPositionUpdate --
			if(e==null || (e.Position.Instrument.FullName.StartsWith(Instruments[0].FullName) || e.Position.Instrument.FullName.StartsWith(Instruments[1].FullName))){
line=1462;
try{
if(IsDebug){
	printDebug(Environment.NewLine+DateTime.Now.ToString()+"  in OnPositionUpdate  "+BuySymbol+" - "+SellSymbol);
	if(e!=null) NinjaTrader.Code.Output.Process(string.Format("Instrument: {0} {1} @ {2} AveragePrice: {3}  Operation: {4}\n   {5}", e.Position.Instrument.FullName, e.Quantity, e.MarketPosition, e.AveragePrice, e.Operation, e.ToString()), PrintTo.OutputTab1);
//	printDebug("CurrentLongInst: "+CurrentLongInst+"  CurrentShortInst: "+CurrentShortInst);
}
line=1469;
				if(e!=null) UpdateAutoTradeButton(BuyOneSellAnother_AutoState.Off);//only update AutoState if this method was called from an execution event, not from another method
line=1471;
				AreSpreadsBalanced = false;
				CurrentSpreadPosition = CalculateSpreadPosition(e!=null, ref AreSpreadsBalanced, ref CurrentLongSize, ref CurrentShortSize);
line=1474;
				if(CurrentLongSize+CurrentShortSize>0){
line=1476;
					if(AreSpreadsBalanced) CalculateCurrentSpreadValue(CurrentSpreadPosition);
					else {
						CurrentSpreadPosition = 0;
						CurrentPositionValue = 0;
					}
					if(e!=null && IsDebug) {
						printDebug("     Long "+CurrentLongInst+" at "+CurrentLongSize+"-contracts");
						printDebug("     Short "+CurrentShortInst+" at "+CurrentShortSize+"-contracts");
						printDebug("Current spread position is: "+CurrentSpreadPosition+" with a value at: "+CurrentPositionValue.ToString("0"));
					}
				}else{
//					foreach(var ex in myAccount.Executions.Where(k=> (k.Instrument.FullName.CompareTo(Instruments[0].FullName)==0 || k.Instrument.FullName.CompareTo(Instruments[1].FullName)==0) && k.IsExit)) {
//						Print("EXIT:           "+ex.Instrument.FullName+"  price: "+ex.Price);
//					}
//			double LastFillValue = CurrentPositionValue;
//			if(direction=='f'){
//				double pnl = 0;
//				if(CurrentPosition > 0) pnl = CurrentPositionValue
//				Print("PnL = 
//			}					CurrentSpreadPosition = 0;
				}
}catch(Exception e2){printDebug("Line: "+line+":  "+BuySymbol+"-"+SellSymbol+"  OnPositionUpdate error: "+e2.ToString());}
			}
//			SourceOfTrade = string.Empty;
			#endregion
		}
//====================                     =======================================================================================
		private void CalculatePositionByInstrumentName(string FullSymbol, Account acct, ref double EquityValue, ref double PosSize, ref string Statement){
			EquityValue = 0;
			PosSize = 0;
			Statement = string.Empty;
			if(myAccount!=null){
				foreach(var pos in myAccount.Positions.Where(k=>k.Instrument.FullName.CompareTo(FullSymbol)==0)) {
					PosSize += pos.Quantity;
					EquityValue += pos.Quantity*pos.GetUnrealizedProfitLoss(PerformanceUnit.Points)*pos.Instrument.MasterInstrument.PointValue;
					Statement = string.Format("{0}: {1} {2} = {3}", pos.Instrument.FullName, (pos.MarketPosition==MarketPosition.Long ? "L":"S"), PosSize, EquityValue.ToString("C"));
				}
			}
//result = result + pos.Quantity*pos.GetUnrealizedProfitLoss(PerformanceUnit.Points)*pos.Instrument.MasterInstrument.PointValue;
		}
//====================                     =======================================================================================
		private double CalculateOpenEquity (ref string infoLongLeg, ref string infoShortLeg){
			#region -- CalculatOpenEquity --
			double equityLongLeg = 0;
			double equityShortLeg = 0;
			double posLongLeg = 0;
			double posShortLeg = 0;
//printDebug("\nCalculateOpenEquity\n");
			CalculatePositionByInstrumentName(Instruments[0].FullName, myAccount, ref equityLongLeg, ref posLongLeg, ref infoLongLeg);
			CalculatePositionByInstrumentName(Instruments[1].FullName, myAccount, ref equityShortLeg, ref posShortLeg, ref infoShortLeg);

			return equityLongLeg + equityShortLeg;
			#endregion
		}
//==================                        ==============================================================
		private int CalculateSpreadPosition (bool PrintMsgs, ref bool AreSpreadsBalanced, ref int longs, ref int shorts){
			#region -- CalculateSpreadPosition --
			longs  = 0;
			shorts = 0;
			if(myAccount==null || myAccount.Positions==null) {
				AreSpreadsBalanced = true;
				return 0;
			}
line=1471;
			foreach(var pos in myAccount.Positions.Where(k=>k.Instrument.FullName.CompareTo(Instruments[0].FullName)==0)) {
				if(pos.MarketPosition == MarketPosition.Long){
					longs += pos.Quantity;
					CurrentLongInst = pos.Instrument.FullName;
				}
				else if(pos.MarketPosition == MarketPosition.Short){
					shorts += pos.Quantity;
					CurrentShortInst = pos.Instrument.FullName;
				}
				if(shorts+longs>0) printDebug("1576: "+BuySymbol+"-"+SellSymbol+"     pos: "+pos.ToString());
			}
line=1483;
			foreach(var pos in myAccount.Positions.Where(k=>k.Instrument.FullName.CompareTo(Instruments[1].FullName)==0)) {
				if(pos.MarketPosition == MarketPosition.Long){
					longs += pos.Quantity;
					CurrentLongInst = pos.Instrument.FullName;
				}
				else if(pos.MarketPosition == MarketPosition.Short){
					shorts += pos.Quantity;
					CurrentShortInst = pos.Instrument.FullName;
				}
				if(shorts+longs>0) printDebug("1588: "+BuySymbol+"-"+SellSymbol+"     pos: "+pos.ToString());
			}

line=1496;
printDebug("CalculateSpreadPosition   longs: "+longs+"   shorts: "+shorts+"  current long: "+CurrentLongInst+"   current short: "+CurrentLongInst);
			AreSpreadsBalanced = false;
			double ExpectedLongs = 0;
			double ExpectedShorts = 0;
			int MustBuyQty = 0;
			int MustSellQty = 0;
			if(CurrentLongInst == Instruments[0].FullName){//we are long on the spread when the long position symbol matches the spread long symbol
line=1503;
				AreSpreadsBalanced = longs / pLongContracts == shorts / pShortContracts;
				if(!AreSpreadsBalanced){
					ExpectedLongs  = longs % pLongContracts;
					ExpectedShorts = shorts % pShortContracts;
					if(ExpectedLongs != 0){
						int count = 0;
						while(longs>0 && ExpectedLongs != 0) {count++; longs-=1; ExpectedLongs = longs % pLongContracts;}
						MustBuyQty = count;
					}
					if(ExpectedShorts != 0){
						int count = 0;
						while(shorts>0 && ExpectedShorts != 0) {count++; shorts-=1; ExpectedShorts = shorts % pShortContracts;}
						MustSellQty = count;
					}
				}else
					CurrentSpreadPosition = longs / pLongContracts;
				if(PrintMsgs && CurrentSpreadPosition > 0) printDebug("     We are LONG the spread, quantity is: +"+longs+" -"+shorts);
			}else{
line=1522;
				AreSpreadsBalanced = longs / pShortContracts == shorts / pLongContracts;
				if(!AreSpreadsBalanced){
					ExpectedLongs  = longs % pShortContracts;
					ExpectedShorts = shorts % pLongContracts;
					if(ExpectedLongs != 0){
						int count = 0;
						while(longs>0 && ExpectedLongs != 0) {count++; longs-=1; ExpectedLongs = longs % pShortContracts;}
						MustBuyQty = count;
					}
					if(ExpectedShorts != 0){
						int count = 0;
						while(shorts>0 && ExpectedShorts != 0) {count++; shorts-=1; ExpectedShorts = shorts % pLongContracts;}
						MustSellQty = count;
					}
				}else
					CurrentSpreadPosition = -longs / pShortContracts;
				if(PrintMsgs && CurrentSpreadPosition < 0) printDebug("     We are SHORT the spread, quantity is: -"+longs+" +"+shorts);
			}
line=1541;
			if(PrintMsgs & !AreSpreadsBalanced){
				printDebug(string.Format("ERROR:  Unbalanced spread on '{0}-{1}'!  You need to:",BuySymbol,SellSymbol));
				if(MustBuyQty>0)  printDebug("   BUY "+(MustBuyQty)+"-contracts of "+CurrentLongInst);
				if(MustSellQty>0) printDebug("   SELL "+(MustSellQty)+"-contracts of "+CurrentShortInst);
			}
			return CurrentSpreadPosition;
			#endregion
		}
//=====================                           ==============================================================
		private double CalculateCurrentSpreadValue(double CurrentPosition){
			#region -- CalculateCurrentSpreadValue --
			double LongValue  = 0;
			double ShortValue = 0;
			double LongEntryPrice  = 0;
			double ShortEntryPrice = 0;
			double equity = 0;
			if(myAccount==null) return 0;

//printDebug("Current spread position: "+CurrentPosition);
			foreach(var pos in myAccount.Positions.Where(k=>
				k.Instrument.FullName.CompareTo(Instruments[0].FullName)==0 || k.Instrument.FullName.CompareTo(Instruments[1].FullName)==0)) {
					var iinfo = pos.Instrument.MasterInstrument;
//Print(iinfo.Name+" PnL: "+pos.GetUnrealizedProfitLoss(PerformanceUnit.Currency).ToString("C"));
//printDebug("Pos: "+pos.ToString());
					if(pos.MarketPosition==MarketPosition.Long){
						CurrentLongInst = pos.Instrument.FullName;
						LongEntryPrice = pos.AveragePrice;
//printDebug("   Long mkt price: "+pos.GetMarketPrice()+"   avg fill: "+pos.AveragePrice+"   PtVal: "+iinfo.PointValue+"  count: "+pos.Quantity);
						LongValue = LongValue + (pos.AveragePrice) * iinfo.PointValue * (CurrentSpreadPosition > 0 ? pLongContracts : pShortContracts);//pos.Quantity;
					}
					else if(pos.MarketPosition==MarketPosition.Short){
						CurrentShortInst = pos.Instrument.FullName;
						ShortEntryPrice = pos.AveragePrice;
//printDebug("   Short mkt price: "+pos.GetMarketPrice()+"   avg fill: "+pos.AveragePrice+"   PtVal: "+iinfo.PointValue+"  count: "+pos.Quantity);
						ShortValue = ShortValue + (pos.AveragePrice) * iinfo.PointValue * (CurrentSpreadPosition > 0 ? pShortContracts : pLongContracts);//pos.Quantity;
					}
			}
//printDebug(CurrentPosition+"  Long "+CurrentLongInst+": "+LongValue+"   Short "+CurrentShortInst+": "+ShortValue);
			var direction = (CurrentLongInst.CompareTo(Instruments[0].FullName)==0 ? 'L' : (CurrentLongInst.CompareTo(Instruments[1].FullName)==0 ? 'S' : 'f'));
			CurrentPositionValue = (direction == 'L' ? LongValue - ShortValue : (direction == 'S' ? ShortValue-LongValue : 0));
//printDebug("Spread value: "+CurrentPositionValue);
			ForceRefresh();
			return CurrentPositionValue;
			#endregion
		}
//===================            ==============================================================
		private bool BuyNow = false;
		private bool SellNow = false;
		private void MyKeyUpEvent (object sender, KeyEventArgs e)
		{
			if(e==null) return;
//			if(!IsDebug) return;
			if(e.Key==Key.LeftShift || e.Key==Key.RightShift){
				IsShiftPressed = false; e.Handled = true;}
//			if(e.Key==Key.LeftAlt   || e.Key==Key.RightAlt || e.Key!=Key.System){
//				IsCtrlPressed = false; e.Handled = true;}
//			if(e.Key==Key.LeftCtrl  || e.Key==Key.RightCtrl){
//				IsCtrlPressed = false; e.Handled = true;}
//			if(e.Key==Key.Escape){
//				IsCtrlPressed = false; e.Handled = true;}
//			if(IsCtrlPressed) {
//				printDebug(e.Key.ToString()+" pressed");
				if(e.Key == Key.D1) printDebug("1");
//				if(e.Key == Key.LeftCtrl)  {IsCtrlPressed = false; printDebug("Buy");  BuyNow = true;  SellNow = false;}
//				if(e.Key == Key.RightCtrl) {IsCtrlPressed = false; printDebug("Sell"); SellNow = true; BuyNow = false;}
//			}
		}
		private void MyKeyDownEvent (object sender, KeyEventArgs e)
		{
			if(e==null) return;
//			if(!IsDebug) return;
			if(e.Key==Key.LeftShift || e.Key==Key.RightShift){
				IsShiftPressed = true; e.Handled = true;}
//			if(e.Key==Key.LeftAlt   || e.Key==Key.RightAlt || e.Key==Key.System) IsCtrlPressed = true;
//			if(e.Key==Key.LeftCtrl  || e.Key==Key.RightCtrl) IsCtrlPressed = true;
//			if(e.Key==Key.Escape) IsCtrlPressed = false;
//			if(IsCtrlPressed) {
//				printDebug(e.Key.ToString()+" pressed");
//				if(e.Key == Key.D1) printDebug("1");
//				if(e.Key == Key.LeftCtrl)  {IsCtrlPressed = false; printDebug("Buy");  BuyNow = true;  SellNow = false;}
//				if(e.Key == Key.RightCtrl) {IsCtrlPressed = false; printDebug("Sell"); SellNow = true; BuyNow = false;}
//			}
		}
		
		public static string SelectedTab = "";
		int move_count = 0;
        private void ChartPanel_MouseMove(object sender, MouseEventArgs e)
        {
			SelectedTab = uID;
//			if(move_count>10) {
//				UpdateUI(); 
//				move_count = 0;
//			}
			move_count++;
			if(ChartControl!=null) ChartControl.InvalidateVisual();
		}
//===================       ==============================================================
		#region MA classes
		private class MyEMA {
			public List<double> Val = null;
			public string status="";
			public int StartPlottingABar = int.MaxValue;
			int Period = 1;
			int CB = 0;
			double constant1 = 0;
			double constant2 = 0;
			public MyEMA(int period){
				Period = Math.Max(1,period);
				constant1 = 2.0 / (1 + Period);
				constant2 = 1 - (2.0 / (1 + Period));
				Val = new List<double>();
			}
			public double Update(Series<double> c, int cb, int recalcBars, ref int FirstValidBar){
status="";
				while(Val.Count <= cb+5) Val.Insert(0,c[0]);
				CB = cb;
				if(recalcBars == 0){
					Val[0] = c[0] * constant1 + constant2 * (Val.Count==0 ? c[0] : Val[1]);
				}else{
					int i = recalcBars-1;
					if(Val.Count<1) Val.Insert(0, c[0]);
try{
					while(i>=0) {
						Val[i] = c[i] * constant1 + constant2 * Val[i+1];
						i--;
					}
}catch(Exception e){status = string.Format("CB: {0}  {1}",CB,e.ToString());}
				}
				if(StartPlottingABar==int.MaxValue && Val.Count>1){
					if(Val[0] > c[0] && Val[1] <= c[1]) {     StartPlottingABar = cb; FirstValidBar = cb;}
					else if(Val[0] < c[0] && Val[1] >= c[1]) {StartPlottingABar = cb; FirstValidBar = cb;}
				}
				if(StartPlottingABar != int.MaxValue) return c[0];
				return Val[0];
			}
			public double GetValueAt(int abar){
				if(abar > CB) return Val[CB];
				return Val[CB-abar];
			}
		}
		MyEMA ema1=null;
		MyEMA ema2=null;

		private class MySMA{
			int Period = 1;
			int CB = 0;
			public List<double> Val = null;
			public string status = "";
			public int StartPlottingABar = int.MaxValue;

			public MySMA(int period){
				Period = Math.Max(1,period);
				Val    = new List<double>();
			}
			public double Update(Series<double> c, int cb, int recalcBars, ref int FirstValidBar){
status = "";
try{
				if(Val.Count <= cb){
					while(Val.Count <= cb) {
						Val.Insert(0, c[0]);
					}
				}
				CB = cb;
				if(CB < Period) return c[0];
				int i = 0;
				if(recalcBars == 0){
					double sum = 0;
					for(i = 0; i<Period; i++) sum = sum + c[i];
					Val[0] = sum / Period;
				}else{
					for(int rbar = Math.Min(CB,recalcBars); rbar >= 0; rbar--){
						double sum = 0;
						for(i = rbar; i<Math.Min(CB,Period+rbar); i++) sum = sum + c[i];
						Val[rbar] = sum / Math.Min(CB,Period);
					}
				}
}catch(Exception e){status = string.Format("CB: {0}  {1}",CB,e.ToString());}
				if(StartPlottingABar==int.MaxValue && Val.Count>1){
					if(Val[0] > c[0] && Val[1] <= c[1]) {     StartPlottingABar = cb; FirstValidBar = cb;}
					else if(Val[0] < c[0] && Val[1] >= c[1]) {StartPlottingABar = cb; FirstValidBar = cb;}
				}
				if(StartPlottingABar != int.MaxValue) return c[0];
				return Val[0];
			}
		}
		MySMA sma1=null;
		MySMA sma2=null;

		private class MyWMA{
			int Period = 1;
			int RecalcBars = 0;
			int CB = 0;
			public List<double> Val = null;
			public string status = "";

			public MyWMA(int period, int recalcBars){
				Period = Math.Max(1,period);
				Val    = new List<double>(){0,0};
				RecalcBars = recalcBars;
			}
			public double Update(SortedDictionary<DateTime,double> c, int cb){
status = CB.ToString();
try{
				if(CB != cb){
					CB = cb;
					if(c.Count==0) return 0;
					Val.Insert(0, c.Values.Last());
				}
				if(c.Count==0) return Val[0];
				if(CB < Period) return Val[0];
				var keys = c.Keys.ToList();//0 element is now the most recent spread value
				keys.Reverse();
				if(RecalcBars == 0){
					int back = Math.Min(Period - 1, CB);
					double val = 0;
					int weight = 0;
status = string.Format("{0}\n   back {1}", status, back);
					for (int idx = back; idx >= 0; idx--)
					{
						double xx = c[keys[back - idx]];
						val    += (idx+1) * xx;
						weight += (idx+1);
status = string.Format("{0}\n   {1}: c{2} v{3} w{4}", status, idx, xx, val, weight);
					}
					Val[0] = val / weight;
status = string.Format("{0}\n w {1} val {2}", status, weight, Val[0]);
				}else{
					for(int rbar = Math.Min(CB,RecalcBars); rbar >= 0; rbar--){
						int back = Math.Min(Period - 1 - rbar, CB);
						double val = 0;
						int weight = 0;
						int count = Math.Min(Period - 1, CB);
						for (int idx = back; idx >= rbar; idx--)
						{
							double xx = c[keys[back - idx]];
							val    += (count + 1) * xx;
							weight += (count + 1);
							count--;
						}
						Val[rbar] = val / weight;
					}
				}
}catch(Exception e){status = string.Format("CB: {0}  {1}",CB,e.ToString());}
				return Val[0];
			}
			public double Update(Series<double> c, int cb){
status = CB.ToString();
try{
				if(CB != cb){
					Val.Insert(0, c[0]);
				}
				CB = cb;
				if(CB < Period) return Val[0];
				if(RecalcBars == 0){
					int back = Math.Min(Period - 1, CB);
					double val = 0;
					int weight = 0;
status = string.Format("{0}\n   back {1}", status, back);
					for (int idx = back; idx >= 0; idx--)
					{
						double xx = c.GetValueAt(CB - back + idx);//c[back-idx]
						val    += (idx+1) * xx;
						weight += (idx+1);
status = string.Format("{0}\n   {1}: c{2} v{3} w{4}", status, idx, xx, val, weight);
					}
					Val[0] = val / weight;
status = string.Format("{0}\n w {1} val {2}", status, weight, Val[0]);
				}else{
					for(int rbar = Math.Min(CB,RecalcBars); rbar >= 0; rbar--){
						int back = Math.Min(Period - 1 - rbar, CB);
						double val = 0;
						int weight = 0;
						int count = Math.Min(Period - 1, CB);
						for (int idx = back; idx >= rbar; idx--)
						{
							double xx = c.GetValueAt(CB - back + idx);//c[back-idx]
							val    += (count + 1) * xx;
							weight += (count + 1);
							count--;
						}
						Val[rbar] = val / weight;
					}
				}
}catch(Exception e){status = string.Format("CB: {0}  {1}",CB,e.ToString());}
				return Val[0];
			}
		}
		MyWMA wma1=null;
		MyWMA wma2=null;
		#endregion

		private char LastSignalType = ' ';
		private int SignalABar = 0;
		private void CalculateTrendAge(int cb, ISeries<double> Spread, SortedDictionary<int,double[]> cci){
			#region -- CalculateTrendAge --
			double sma14 = 0;
			for(int j = cb-pReversalDotsPeriod*2; j<cb; j++)
				sma14 = sma14+Spread.GetValueAt(j);
			sma14 = sma14/(pReversalDotsPeriod*2);
			double mean = 0;
			for (int j = cb-pReversalDotsPeriod*2; j<cb; j++)
				mean += Math.Abs(Spread.GetValueAt(j) - sma14);
			cci[cb] = new double[3];
			cci[cb][0] = (Spread.GetValueAt(cb) - sma14) / (mean.ApproxCompare(0) == 0 ? 1 : (0.015 * (mean / pReversalDotsPeriod/2) ) );
			mean = 0;
			for (int j = cb-(pReversalDotsPeriod*2+1); j<cb; j++){
				if(cci.ContainsKey(j)) mean += cci[j][0];
			}
			if(SignalABar == 0 && State == State.Realtime) SignalABar = cb-4;//first bar after a chart refresh is ignored...signals can only come on subsequent bars
			cci[cb][1] = mean/(pReversalDotsPeriod*2+1);//the [1] element of the double[] is the SMA of the [0] element
			if(cci[cb][1] < -200) {
				cci[cb][2] = 2;
				if(IsRealTime1 && IsRealTime2){
					if(SignalABar < cb-5 && LastSignalType=='B') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new long signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'B') {
						SignalABar = cb;
						LastSignalType = 'B';
//BackBrushes[CurrentBar-cb]=Brushes.Cyan;
						myAlert(string.Format("bosa Buy{0}",cb.ToString()), Priority.High, string.Format("BOSA buy #2 on {0} {1} - {2} {3}",pLongContracts,BuySymbol,pShortContracts,SellSymbol), AddSoundFolder(pEntryLongSound), 10, Brushes.Black,Brushes.Lime);
					}
				}
			}
			else if(cci[cb][1] < -100) {
				cci[cb][2] = 1;
				if(IsRealTime1 && IsRealTime2){
					if(SignalABar < cb-5 && LastSignalType=='B') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new long signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'B') {
						SignalABar = cb;
						LastSignalType = 'B';
//BackBrushes[CurrentBar-cb]=Brushes.Cyan;
						myAlert(string.Format("bosa Buy{0}",cb.ToString()), Priority.High, string.Format("BOSA buy #1 on {0} {1} - {2} {3}",pLongContracts,BuySymbol,pShortContracts,SellSymbol), AddSoundFolder(pEntryLongSound), 10, Brushes.Black,Brushes.Lime);
					}
				}
			}
			else if(cci[cb][1] > 200) {
				cci[cb][2] = -2;
				if(IsRealTime1 && IsRealTime2){
					if(SignalABar < cb-5 && LastSignalType=='S') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new short signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'S') {
						SignalABar = cb;
						LastSignalType = 'S';
//BackBrushes[CurrentBar-cb]=Brushes.Pink;
						myAlert(string.Format("bosa Sell{0}",cb.ToString()), Priority.High, string.Format("BOSA sell #2 on {0} {1} - {2} {3}",pLongContracts,BuySymbol,pShortContracts,SellSymbol), AddSoundFolder(pEntryShortSound), 10, Brushes.Black,Brushes.Magenta);
					}
				}
			}
			else if(cci[cb][1] > 100) {
				cci[cb][2] = -1;
				if(IsRealTime1 && IsRealTime2){
					if(SignalABar < cb-5 && LastSignalType=='S') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new short signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'S') {
						SignalABar = cb;
						LastSignalType = 'S';
//BackBrushes[CurrentBar-cb]=Brushes.Pink;
						myAlert(string.Format("bosa Sell{0}",cb.ToString()), Priority.High, string.Format("BOSA sell #1 on {0} {1} - {2} {3}",pLongContracts,BuySymbol,pShortContracts,SellSymbol), AddSoundFolder(pEntryShortSound), 10, Brushes.Black,Brushes.Magenta);
					}
				}
			}
			else cci[cb][2] = 0;
			#endregion
		}
//============================================================================================================================
		private string DetermineStatusMsg(BuyOneSellAnother_AutoState AutoState){
			#region -- DetermineStatusMsg --
			if(StatusMsg.Contains("Could not connect to account: ")) return StatusMsg;
			StatusMsg = string.Empty;
			char Separator = '|';

			if(AutoState == BuyOneSellAnother_AutoState.Off){
				StatusMsg = "Manual entry selected|";//+EquityRatioString;
			}else{
				if(double.IsNaN(HLine_BuyValue) && double.IsNaN(HLine_SellValue)){
					StatusMsg = "AutoTrading: Draw a HorizontalLine at your entry level, and add 'buy' or 'sell' to the tag";
				}
				else if(AutoState == BuyOneSellAnother_AutoState.Close){
					if(CurrentSpreadPosition > 0 && !double.IsNaN(HLine_SellValue))
						StatusMsg = string.Format("{0} AutoTrading: Close out the spread.{4}SELL {1}-spread{2} if value hits {3}",
							SettingStr_Calculate,
							CurrentSpreadPosition, 
							CurrentSpreadPosition > 1 ? "s":string.Empty,
							Convert.ToInt32(HLine_SellValue),
							Separator);
					else if(CurrentSpreadPosition < 0 && !double.IsNaN(HLine_BuyValue))
						StatusMsg = string.Format("{0} AutoTrading: Close out the spread.{4}BUY {1}-spread{2} if value hits {3}",
							SettingStr_Calculate,
							-CurrentSpreadPosition, 
							-CurrentSpreadPosition > 1 ? "s":string.Empty,
							Convert.ToInt32(HLine_BuyValue),
							Separator);
				}
				else if(AutoState == BuyOneSellAnother_AutoState.Add){
						if(CurrentSpreadPosition < 0 && !double.IsNaN(HLine_SellValue))
							StatusMsg = string.Format("{0} AutoTrading: Adding to the SHORT spread.{4}SELL {1}-spread{2} if value hits {3}",
								SettingStr_Calculate,
								SelectedSpreadQty, SelectedSpreadQty > 1 ? "s":string.Empty,
								Convert.ToInt32(HLine_SellValue),
								Separator);
						else if(CurrentSpreadPosition > 0 && !double.IsNaN(HLine_BuyValue))
							StatusMsg = string.Format("{0} AutoTrading: Adding to the LONG spread.{4}BUY {1}-spread{2} if value hits {3}",
								SettingStr_Calculate,
								SelectedSpreadQty, 
								SelectedSpreadQty > 1 ? "s":string.Empty,
								Convert.ToInt32(HLine_BuyValue),
								Separator);
						else if(CurrentSpreadPosition == 0 && !double.IsNaN(HLine_BuyValue) && !double.IsNaN(HLine_SellValue))
							StatusMsg = string.Format("{0} AutoTrading: Ready to:{5}BUY {1}-spread{2} if value hits {3},{5}SELL {1}-spread{2} if value hits {4}",
								SettingStr_Calculate,
								SelectedSpreadQty, 
								SelectedSpreadQty > 1 ? "s":string.Empty,
								Convert.ToInt32(HLine_BuyValue),
								Convert.ToInt32(HLine_SellValue),
								Separator);
						else if(CurrentSpreadPosition == 0 && !double.IsNaN(HLine_BuyValue))
							StatusMsg = string.Format("{0} AutoTrading: Ready to BUY {1}-spread{2} if value hits {3}",
								SettingStr_Calculate,
								SelectedSpreadQty, 
								SelectedSpreadQty > 1 ? "s":string.Empty,
								Convert.ToInt32(HLine_BuyValue));
						else if(CurrentSpreadPosition == 0 && !double.IsNaN(HLine_SellValue))
							StatusMsg = string.Format("{0} AutoTrading: Ready to SELL {1}-spread{2} if value hits {3}",
								SettingStr_Calculate,
								SelectedSpreadQty, 
								SelectedSpreadQty > 1 ? "s":string.Empty,
								Convert.ToInt32(HLine_SellValue));
				}
				else if(AutoState == BuyOneSellAnother_AutoState.BuyOnly && !double.IsNaN(HLine_BuyValue)){
					StatusMsg = string.Format("{0} AutoTrading: Ready to BUY {1}-spread{2} if value hits {3}",
						SettingStr_Calculate,
						SelectedSpreadQty, SelectedSpreadQty > 1 ? "s":string.Empty,
						Convert.ToInt32(HLine_BuyValue));
				}
				else if(AutoState == BuyOneSellAnother_AutoState.SellOnly && !double.IsNaN(HLine_SellValue)){
					StatusMsg = string.Format("{0} AutoTrading: Ready to SELL {1}-spread{2} if value hits {3}",
						SettingStr_Calculate,
						SelectedSpreadQty, SelectedSpreadQty > 1 ? "s":string.Empty,
						Convert.ToInt32(HLine_SellValue));
				}
			}
			return StatusMsg;
			#endregion
		}
//============   DrawTextFixed  ==============================================================================================
		#region -- Draw methods --
		void DrawTextFixed(string tag, string msg, TextPosition txtpos){
			if(tag!="licerror") {if(!IsRealTime1 || !IsRealTime2) return;}
			if (ChartControl.Dispatcher.CheckAccess())
				Draw.TextFixed(this,tag, msg, txtpos);
			else
				ChartControl.Dispatcher.InvokeAsync((Action)(() => {
					TriggerCustomEvent(o2 =>{
						Draw.TextFixed(this,tag, msg, txtpos);
					},0,null);
				}));
		}
		void DrawTextFixed(string tag, string msg, TextPosition txtpos, Brush foregroundBrush, SimpleFont font, Brush outlineBrush, Brush fillBrush, int opacity){
			if(tag!="licerror") {if(!IsRealTime1 || !IsRealTime2) return;}
			if (ChartControl.Dispatcher.CheckAccess())
				Draw.TextFixed(this,tag, msg, txtpos, foregroundBrush, font, outlineBrush, fillBrush, opacity);
			else
				ChartControl.Dispatcher.InvokeAsync((Action)(() => {
					TriggerCustomEvent(o2 =>{
						Draw.TextFixed(this,tag, msg, txtpos, foregroundBrush, font, outlineBrush, fillBrush, opacity);
					},0,null);
				}));
//			Draw.TextFixed("posvalue", msg, TextPosition.Center, netDollars>0 ? Brushes.Lime:Brushes.Magenta, new SimpleFont("Arial",15), Brushes.Black, Brushes.Black, 50);
		}
		#endregion
		
		
		private void AddToDiffs(int t, double val){
			ChannelRanges[t].Add(val);
		}
		private int GetChannelRangesId(DateTime t){
			return t.Hour*100 + (t.Minute < 30 ? 0:30);
		}
		private void CalcDistPctileOnAllTimeperiods(double divide){
			double temp1 = 0;
			double temp2 = 0;
			int k = 0;
//Print("CalcDistPctileOnAllTimeperiods is running");
			foreach(int t in ChannelRanges.Keys){
				if(ChannelRanges[t].Count>1){
					var r = ChannelRanges[t].Where(x=> x > 0).ToList();
					if(r.Count>1){
						r.Sort();
						if(r.First() < r.Last()) r.Reverse();
						k = Convert.ToInt32(Math.Truncate(r.Count*pChannelReductionPct));
						temp1 = r[k];
					}else if(r.Count==1)
						temp1 = r[0];
					temp2 = -temp1;
					r = ChannelRanges[t].Where(x=> x < 0).ToList();//assemble a list of negative ranges, ranges below the midline
					if(r.Count>1){
						r.Sort();
						if(r.First() > r.Last()) r.Reverse();
						k = Convert.ToInt32(Math.Truncate(r.Count*pChannelReductionPct));
						temp2 = r[k];
					}else if(r.Count==1)
						temp2 = r[0];
				}else if(ChannelRanges.Count==1){
					temp1 = ChannelRanges[t][0];
					temp2 = -temp1;
				}
				ChannelRanges[t].Clear();
				ChannelRanges[t].Add(temp1 / divide);//The positive ChannelRanges dictionary is now condensed.  The list of all individual distances is cleared and only the single percentile distance is saved
				ChannelRanges[t].Add(temp2 / divide);//The negative ChannelRanges dictionary is now condensed.  The list of all individual distances is cleared and only the single percentile distance is saved
//Print("     "+t+" pctle is "+ChannelRanges[t][0].ToString("0"));
			}
		}
		private void GetDistPctile(DateTime t, ref double Upper, ref double Lower){
			int id = GetChannelRangesId(t);
			Upper = ChannelRanges[id][0];
			Lower = ChannelRanges[id][1];
		}
		private void GetDistPctile(int id, ref double Upper, ref double Lower){
			Upper = ChannelRanges[id][0];
			Lower = ChannelRanges[id][1];
		}
//============   OnBarUpdate  ==============================================================================================
		SortedDictionary<int,List<double>> ChannelRanges   = new SortedDictionary<int,List<double>>();
		bool CalculatedDiffs = false;
		double upperDist10pctile  = 0;
		double lowerDist10pctile  = 0;
		double SLDistance    = double.MinValue;
		double keyprice   = 0;
		int AlertABar     = 0;
		string BuySymbol  = "";
		string SellSymbol = "";
		double H = 0;
		double L = 0;
		SortedDictionary<int,bool> InSession = new SortedDictionary<int,bool>();
		SortedDictionary<DateTime,double> P0 = new SortedDictionary<DateTime,double>();
		SortedDictionary<DateTime,double> P1 = new SortedDictionary<DateTime,double>();
		SortedDictionary<DateTime,double> spreadmap = new SortedDictionary<DateTime,double>();
		SortedDictionary<int,double> spread_avg = new SortedDictionary<int,double>();
		double c0  = double.MinValue;
		double c1  = double.MinValue;
		double c0t = double.MinValue;
		double c1t = double.MinValue;
		double c0val = 0;
		double c1val = 0;
		double Correlation = double.NaN;
		private bool Initialize = true;
		private double val = 0;
		private bool IsRealTime1=false;
		private bool IsRealTime2=false;
		private bool PermitHorizLineTrades = false;
		int cb0 = 0;
		double p0=double.MinValue;
		double p1=double.MinValue;
		double AskValOnPriorTick = double.MinValue;
		double BidValOnPriorTick = double.MinValue;
		private class SRdata{
			public List<double> Spread = new List<double>();
			public double Last = 0;
			public double Hi = double.MinValue;
			public double Lo = double.MaxValue;
			public SRdata(double spread){
				Spread.Add(spread);
				Last = spread;
//				Hi = spread;
//				Lo = spread;
			}
			public SRdata(double spread, double hi, double lo){
				Spread.Add(spread);
				Last = spread;
				Hi = hi;
				Lo = lo;
			}
			public void Add(double spread){
				Spread.Add(spread);
				Last = spread;
				Hi = Spread.Max();
				Lo = Spread.Min();
			}
		}
		private SortedDictionary<int,SRdata> SpreadRange = new SortedDictionary<int,SRdata>();//int is CurrentBars[0], double array is HiLoBarTop,HiLoBarBot,Spread
		private SortedDictionary<int,List<double>> DiffToKeyLevel = new SortedDictionary<int,List<double>>();//int is time "0910" for 9:10am, double is Spread-KeyLevel
		private SortedDictionary<int,SortedDictionary<int,double>> DiffToKeyLevelbyDay = new SortedDictionary<int,SortedDictionary<int,double>>();
		
		private string BottomRightTextStr = string.Empty;
		#region -- Timer variables and methods --
		private DateTime		now		 	= Core.Globals.Now;
		private bool			connected, hasRealtimeData;
		private SessionIterator sessionIterator;
		private System.Windows.Threading.DispatcherTimer timer;
		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Connected
				&& connectionStatusUpdate.Connection.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType)
				&& Bars.BarsType.IsIntraday)
			{
				connected = true;

				if (DisplayTime() && timer == null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						timer			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 5), IsEnabled = true };
						timer.Tick		+= OnTimerTick;
					});
				}
			}
			else if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Disconnected)
				connected = false;
		}

		private bool DisplayTime()
		{
			return ChartControl != null
					&& Bars != null
					&& Bars.Instrument.MarketData != null;
		}

		private void OnTimerTick(object sender, EventArgs e)
		{
			ForceRefresh();
			if (DisplayTime()){
				if (timer != null && !timer.IsEnabled)
					timer.IsEnabled = true;

				RemoveDrawObject("BottomRightText");
				if (connected)
				{
					if (SessionIterator.IsInSession(Now, false, true))
					{
						if (hasRealtimeData)
						{
							string s;
							if(BottomRightTextStr==string.Empty || !BottomRightTextStr.StartsWith("Auto trade"))
								s = Now.ToString("HH:mm:ss");
							else{
								s = string.Format("{0}\n{1}",Now.ToString("HH:mm:ss"),BottomRightTextStr);
							}

							Draw.TextFixed(this, "BottomRightText", s, TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
						}
						else
							Draw.TextFixed(this, "BottomRightText", string.Format("{0}\n{1}",BottomRightTextStr,"Waiting on data"), TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
					}
					else
						Draw.TextFixed(this, "BottomRightText", string.Format("{0}\n{1}",BottomRightTextStr,"Session time error"), TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
				}
				else
				{
					Draw.TextFixed(this, "BottomRightText", string.Format("{0}\n{1}",BottomRightTextStr,"Not connected"), TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);

					if (timer != null)
						timer.IsEnabled = false;
				}
			}
		}

		private SessionIterator SessionIterator
		{
			get
			{
				if (sessionIterator == null)
					sessionIterator = new SessionIterator(Bars);
				return sessionIterator;
			}
		}

		private DateTime Now
		{
			get
			{
				now = (Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now);

				if (now.Millisecond > 0)
					now = Core.Globals.MinDate.AddSeconds((long)Math.Floor(now.Subtract(Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
		#endregion
bool inzone=false;
		double srHigh = double.MinValue;
		double srLow  = double.MaxValue;
		SortedDictionary<int,int> Profile = new SortedDictionary<int,int>();
		int ProfileStep = 10;
		int ProfileVAH = int.MinValue;
		int ProfileVAL = int.MinValue;
		bool file_written = false;

		protected override void OnBarUpdate ()
		{

			if(!ValidLicense) return;
line = 776;
try{
			#region -- Determine StatusMsg --
			StatusMsg = DetermineStatusMsg(AutoState);
//				printDebug("Status: "+StatusMsg);
			#endregion

//			var t = new List<DateTime>{new DateTime(2021,07,30,15,23,0,0),new DateTime(2021,07,30,15,24,0,0),new DateTime(2021,07,30,15,25,0,0)};
//t.Clear();
//printDebug(Times[0][0].ToString());
			cb0 = CurrentBars[ 0];
			if(Initialize){
				Initialize = false;
				AreSpreadsBalanced = true;
				CurrentSpreadPosition = CalculateSpreadPosition(true, ref AreSpreadsBalanced, ref CurrentLongSize, ref CurrentShortSize);
			}
			if(State==State.Historical){
				if(BarsArray.Length > PLUS_TICK_BIP && (BarsInProgress == PLUS_TICK_BIP || BarsInProgress == MINUS_TICK_BIP)){
				#region -- this will use the 1-tick datafeeds to calculate the approximate range of the spread on the historical bars --
					if(BarsInProgress == PLUS_TICK_BIP){
						c0t = Round2Tick(Closes[PLUS_TICK_BIP][0],BarsInProgress);
						c0val = c0t * (pIsEquitySpread ? PLUS_POINT_VALUE : 1);
					}
					if(BarsInProgress == MINUS_TICK_BIP){
						c1t = Round2Tick(Closes[MINUS_TICK_BIP][0],BarsInProgress);
						c1val = c1t * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
					}
					if(c0t==double.MinValue || c1t==double.MinValue) return;
				#endregion
				}
//inzone = Times[0][0].Day==23 && Times[0][0].Hour==9 && Times[0][0].Minute>15 && Times[0][0].Minute<41;
				val = c0val-c1val;
				if(!SpreadRange.ContainsKey(cb0)) {
					SpreadRange[cb0] = new SRdata(val);
//Print("3051   New SpreadRange created "+Times[0][0].ToString()+"       "+SpreadName);
//if(inzone)Print(string.Format("\n\n   new SpreadRange created  {0:0}  {1:0}  {2:0}", val, SpreadRange[cb0].Hi, SpreadRange[cb0].Lo));
				} else {
					SpreadRange[cb0].Add(val);
//if(inzone)Print(string.Format("{0}              SpreadRange updated  {1:0}  max {2:0}  min {3:0}", cb0, val, SpreadRange[cb0].Hi, SpreadRange[cb0].Lo));
				}
			}
			#region -- Populate P0 dictionary and P1 dictionary on each close price for both instruments --
			if(BarsInProgress == 0){
				if(State == State.Realtime) IsRealTime1 = true;
				c0 = Round2Tick(Closes[BarsInProgress][0], BarsInProgress);
//printDebug("C0: "+c0+"           "+Times[BarsInProgress][0].ToString());
				if(IsRealTime1){//calculate current value intelligently based on the bid or ask price, depending on the current spread position
					if(CurrentSpreadPosition > 0) {
						c0 = PlusBid;
//printDebug(string.Format("{0}   Long:  OBU  PlusBid",GetCurrentBid(0)));
					}else if(CurrentSpreadPosition < 0){
						c0 = PlusAsk;
//printDebug(string.Format("{0}   Short:  OBU  PlusAsk",GetCurrentAsk(0)));
					}else{
						c0 = Round2Tick((PlusAsk+PlusBid)/2, BarsInProgress);
					}
				}
				p0 = c0 * (pIsEquitySpread ? PLUS_POINT_VALUE : 1);
				P0[Times[BarsInProgress][0]] = p0;
				if(!P1.ContainsKey(Times[BarsInProgress][0]) && P1.Count>0) P1[Times[BarsInProgress][0]] = P1.Values.Last();//seed the P1 dict to fill in any missing bars of price data
			}
line=1954;
			if(BarsInProgress == 1){
				if(State == State.Realtime) IsRealTime2 = true;
				c1 = Round2Tick(Closes[BarsInProgress][0], BarsInProgress);
//printDebug("      C1: "+c1+"           "+Times[BarsInProgress][0].ToString());
				if(IsRealTime2){//calculate current value intelligently based on the bid or ask price, depending on the current spread position
					if(CurrentSpreadPosition > 0) {
						c1 = MinusAsk;
					}else if(CurrentSpreadPosition < 0){
						c1 = MinusBid;
					}else{
						c1 = Round2Tick((MinusAsk+MinusBid)/2, BarsInProgress);
					}
				}
line=1960;
				p1 = c1 * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
				P1[Times[BarsInProgress][0]] = p1;
			}
			#endregion
			if(BarsInProgress == 0){
				#region -- Update CorrelationDict --
				if(CorrelationDict !=null && pCorrelationTimeMinutes > 0 && c0!=0 && c1!=0){
					if(CorrelationDict.Count==0) CorrelationDict[Times[0][0]] = new double[2]{c0,c1};
					var maxT = CorrelationDict.Keys.Max();
					var t = maxT.AddMinutes(pCorrelationTimeMinutes);
					if(Times[0][0] >= t){
						CorrelationDict[Times[0][0]] = new double[2]{c0,c1};
					}else{
						CorrelationDict[maxT][0] = c0;
						CorrelationDict[maxT][1] = c1;
					}
				}
				#endregion
line=1965;
//				if(CurrentBars[0] > BarsArray[0].Count-5 && !file_written && pOutFileName.Trim().Length>0){
//					file_written = true;
//					var strings = new List<string>();
//					var fullpath =System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),pOutFileName);

//					if(System.IO.File.Exists(fullpath)) System.IO.File.Delete(fullpath);
//					foreach(var x in CorrelationDict){
//						strings.Add(string.Format("{0};{1};{2}", x.Key.ToString(), x.Value[0], x.Value[1]));
//					}
//					System.IO.File.WriteAllLines(fullpath, strings.ToArray());
//				}
				while(P0.Count>15){
					P0.Remove(P0.Keys.First());
				}
				while(P1.Count>13 && P0.Count>13 && P1.Keys.First() < P0.Keys.First()){
					P1.Remove(P1.Keys.First());
				}
				#region -- Calculate spread: (P0-P1) coordinated values --
				var keys = P0.Keys.ToList();//since we're running on a minute based timeframe, coordinate the values of P1 based on P0 keys
				foreach(var k in keys){
					int abar = BarsArray[0].GetBar(k);
					int rbar = cb0-abar;
line=1978;
					if(P0.ContainsKey(k)){
						p0 = P0[k];
					}
					if(P1.ContainsKey(k)){
						p1 = P1[k];
					}
					if(p0 == double.MinValue || p1 == double.MinValue){
//if(p0==double.MinValue) printDebug(k.ToString()+"  p0 was MinValue!");
//if(p1==double.MinValue) printDebug(k.ToString()+"  p1 was MinValue!");
						if(!SpreadRange.ContainsKey(abar)){
							SpreadRange[abar] = new SRdata(val);
//printDebug("3137   New SpreadRange created "+Times[0][0].ToString()+"       "+SpreadName);
						}
					}else{
						val = p0-p1;
						if(!SpreadRange.ContainsKey(abar)){
							SpreadRange[abar] = new SRdata(val);
//printDebug("3143  New SpreadRange created "+Times[0][0].ToString()+"       "+SpreadName);
						}
//if(IsRealTime1 && IsRealTime2) printDebug("Val: "+val);
						if(Math.Abs(val) > 100 && !IsCalendarSpread)//only check for invalid val when it is larger than 100...calendar spreads are frequently less than 100
						{
							double top = (SpreadRange[abar-1].Hi + 0.1 * Math.Abs(SpreadRange[abar-1].Hi));
							double bot = (SpreadRange[abar-1].Lo - 0.1 * Math.Abs(SpreadRange[abar-1].Lo));
							if((val) > top) {
								if(IsRealTime1 && IsRealTime2) printDebug(string.Format("{0}  {1}-{2} Spread was too large at {3}  compared to max of {4} min of {5}", Times[0][rbar].ToString(), Instruments[0].FullName, Instruments[1].FullName, val.ToString("0"), top, bot));
								val = SpreadRange[abar-1].Last;
							}else if(!IsCalendarSpread && (val) < bot) {
								if(IsRealTime1 && IsRealTime2) printDebug(string.Format("{0}  {1}-{2} Spread was too small at {3}  compared to max of {4} min of {5}", Times[0][rbar].ToString(), Instruments[0].FullName, Instruments[1].FullName, val.ToString("0"), top, bot));
								val = SpreadRange[abar-1].Last;
							}
						}
						Spread[rbar] = val;//this.EliminateFalsePrices(List_RecentSpreads, val, Spread[cb0-abar-1]);
						if(pSMAperiod>0) spread_avg[abar] = val;
//if(t.Contains(k)) printDebug("  Spread:   "+Spread[cb0 - abar]);
					}
				}
				#endregion
				if(IsFirstTickOfBar){
					HiLoBarTop[0] = Spread[0];
					HiLoBarBot[0] = Spread[0];
				}
				HiLoBarTop[0] = Math.Max(HiLoBarTop[0], Spread[0]);
				HiLoBarBot[0] = Math.Min(HiLoBarBot[0], Spread[0]);
				if(!SpreadRange.ContainsKey(cb0)){
					SpreadRange[cb0] = new SRdata(Spread[0], HiLoBarTop[0], HiLoBarBot[0]);
//printDebug("3172   New SpreadRange created "+Times[0][0].ToString()+"       "+SpreadName);
				}else{
					SpreadRange[cb0].Hi = HiLoBarTop[0];
					SpreadRange[cb0].Lo = HiLoBarBot[0];
				}
				OverallMaxSpreadVal = Math.Max(HiLoBarTop[0], OverallMaxSpreadVal);
				OverallMinSpreadVal = Math.Min(HiLoBarBot[0], OverallMinSpreadVal);
				AddToOverallHL(CurrentBars[0], OverallMaxSpreadVal, OverallMinSpreadVal);
line=3034;
				if(pMA1period>0){
					#region -- Calc MA1 --
line=1992;
					if(pMA1type == BuyOneSellAnother_MAtype.EMA){
						if(ema1==null) ema1 = new MyEMA(pMA1period);
						ema1.Update(Spread, cb0, 2, ref MA1FirstABar);
//if(ema1.status.Length>0) printDebug("ema1 status: "+ema1.status);
						for(int i = 0; i<=Math.Min(cb0,Math.Min(ema1.Val.Count-1,3)); i++){
							if(ema1.StartPlottingABar < cb0) {
								MA1[i] = ema1.Val[i];
//								MA1dots[i] = MA1[i];
								//if(MA1[i] > MA1[i+1]) PlotBrushes[10][i] = pMA1UpDots; else PlotBrushes[10][i]=pMA1DnDots;
//if(z) printDebug(Times[0][0].ToString()+"  "+cb0+":   ema1["+i+"]: "+ema1.Val[i]+"  ma1: "+MA1[i]);
							}else MA1.Reset(cb0-i);
						}
					}
					else if(pMA1type == BuyOneSellAnother_MAtype.SMA){
line=2002;
						if(sma1==null) sma1 = new MySMA(pMA1period);
						sma1.Update(Spread, cb0, 2, ref MA1FirstABar);
//if(sma1.status.Length>0) printDebug("sma1 status: "+sma1.status);
						for(int i = 0; i<=Math.Min(cb0,Math.Min(sma1.Val.Count-1,2)); i++){
							if(sma1.StartPlottingABar < cb0) {
								MA1[i] = sma1.Val[i];
//								MA1dots[i] = MA1[i];
								//if(MA1[i] > MA1[i+1]) PlotBrushes[10][i] = pMA1UpDots; else PlotBrushes[10][i]=pMA1DnDots;
							}else MA1.Reset(cb0-i);
						}
					}
					#endregion
					if(pFastMASignalDelayInBars>0 && IsFirstTickOfBar){
						char last_dir = ' ';
						int count = 0;
						if(FastMATrendSignals.Count>0) last_dir = FastMATrendSignals.Last().Value;
						for(int x = 1; x <= pFastMASignalDelayInBars; x++){
							count += MA1[x] > MA1[x+1] ? 1 : (MA1[x] < MA1[x+1] ? -1 : 0);//count is incremented on rising MA1, decremented on falling MA1
						}
						if(count == pFastMASignalDelayInBars && last_dir != 'U') FastMATrendSignals[cb0-1] = 'U';
						else if(count == -pFastMASignalDelayInBars && last_dir != 'D') FastMATrendSignals[cb0-1] = 'D';
					}
				}
				if(pMA2period>0){
					#region -- Calc MA2 --
					if(pMA2type == BuyOneSellAnother_MAtype.EMA){
						if(ema2==null) ema2 = new MyEMA(pMA2period);
						ema2.Update(Spread, cb0, 3, ref MA2FirstABar);
						for(int i = 0; i<=Math.Min(cb0,Math.Min(ema2.Val.Count-1,3)); i++) {
							if(ema2.StartPlottingABar < cb0) 
								MA2[i] = ema2.Val[i];
							else MA2.Reset(cb0-i);
						}
					}
					else if(pMA2type == BuyOneSellAnother_MAtype.SMA){
						if(sma2==null) sma2 = new MySMA(pMA2period);
						sma2.Update(Spread, cb0, 2, ref MA2FirstABar);
						for(int i = 0; i<=Math.Min(cb0,Math.Min(sma2.Val.Count-1,2)); i++){
							if(sma2.StartPlottingABar < cb0) 
								MA2[i] = sma2.Val[i];
							else MA2.Reset(cb0-i);
						}
					}
					#endregion
				}
//				while(P0.Count>5){
//					P0.Remove(P0.Keys.First());
//				}
//				while(P1.Count>3 && P0.Count>3 && P1.Keys.First() < P0.Keys.First()){
//					P1.Remove(P1.Keys.First());
//				}
line=3094;
				if(pSMAperiod > 0 && spread_avg.Count>0) {
					while(spread_avg.Count > pSMAperiod) spread_avg.Remove(spread_avg.Keys.Min());
					KeyLevel[0] = spread_avg.Values.Average();
				}

				if(!AreSpreadsBalanced){
					string LongInfo  = "";
					string ShortInfo = "";
					double netDollars = CalculateOpenEquity(ref LongInfo, ref ShortInfo);
					DrawTextFixed("posvalue", string.Format("Unbalanced spread detected\n\n{0}\n{1}\nNet: {2}",LongInfo,ShortInfo,netDollars.ToString("C")), TextPosition.Center,netDollars>0 ? Brushes.Lime:Brushes.Magenta, font, Brushes.Black,Brushes.Black,50);
					CurrentSpreadPosition = CalculateSpreadPosition(false, ref AreSpreadsBalanced, ref CurrentLongSize, ref CurrentShortSize);
					if(CurrentSpreadPosition != 0)
						CalculateCurrentSpreadValue(CurrentSpreadPosition);
				}else {
					RemoveDrawObject("posvalue");
				}
			}
			int id = 0;
			if(IsFirstTickOfBar && Spread.Count>1 && KeyLevel.Count>1 && BarsInProgress == 0){
				if(SpreadRange.ContainsKey(cb0-2)) SpreadRange[cb0-2].Spread.Clear();//the tick-by-tick spread values are no longer needed, we've calculated the Hi, Lo and Last for each abar
				if(pSMAperiod>0 && !CalculatedDiffs){//if channel is a MA channel, then we have the MA values and the current spread values...so we can fill the ChannelRanges dictionary
					id = GetChannelRangesId(Times[0][0]);
					AddToDiffs(id, (Spread[0]-KeyLevel[0]));
				}
				if(State == State.Realtime && pShowReversalDots){//Calculate the TrendAge indicator dots
					CalculateTrendAge(cb0-1, Spread, cci);
				}
				if(cb0>BarsArray[0].Count-3 && !CalculatedDiffs){//after the historical data finishes loading, and the Spread and SMA dataseries are populated, calculate the channel size
					#region -- Run Once to calculate ranges --
					CalculatedDiffs = true;
					#region -- Calculate Correlations on historical bars --
					if(CorrelationDict!=null) {
						Correlation = CalculateCorrelation();
					}
					#endregion

					if(pSMAperiod > 0){
						CalcDistPctileOnAllTimeperiods(1);
//for(int abar = 10; abar<2000; abar++){
						int day = 0;
						for(int abar = 10; abar<BarsArray[0].Count-1; abar++){
//try{
							DateTime t = Times[0].GetValueAt(abar);
							GetDistPctile(t, ref upperDist10pctile, ref lowerDist10pctile);
//printDebug(Times[0].GetValueAt(abar).ToString()+"  Dist: "+Dist10pctile);
							SLDistance = (upperDist10pctile+Math.Abs(lowerDist10pctile))/2 * pOuterBandsMult;
							DU[cb0-abar]  = KeyLevel[cb0-abar] + upperDist10pctile;
							DL[cb0-abar]  = KeyLevel[cb0-abar] + lowerDist10pctile;
							SLU[cb0-abar] = DU[cb0-abar] + SLDistance;
							SLL[cb0-abar] = DL[cb0-abar] - SLDistance;

							int ttt = ToTime(t)/100;
							if(day != t.Day){//first bar of new day, save the DiffToKeyLevel averages to DiffToKeyLevelbyDay structure
								AddTo_DiffToKeyLevelbyDay(abar, ttt);
//Print("New day                  "+t.ToString());
//foreach(var kvp in DiffToKeyLevelbyDay[abar]) Print(kvp.Key+"   "+kvp.Value);
							}
							day = t.Day;
							double v = Spread.GetValueAt(abar) - KeyLevel.GetValueAt(abar);
							if(!DiffToKeyLevel.ContainsKey(ttt)){
//if(t.Day==21 || t.Day==22 || t.Day==23) Print("   adding "+ttt+"    adding "+v.ToString("N2"));
								DiffToKeyLevel[ttt] = new List<double>(){v};
							}else{
								DiffToKeyLevel[ttt].Add(v);
//if(t.Day==23) {
//	Print("   adding diff of "+v.ToString("N2")+"  new count("+ttt+"): "+DiffToKeyLevel[ttt].Count);
//	foreach(var rr in DiffToKeyLevel[ttt]) Print("     "+rr.ToString("N2"));
//}
							}
//}catch(Exception dd){printDebug(dd.ToString());}
//							#region -- Clear MA1 or MA2 if they are too far from the Spread line...reduces vertical chart scale condensation
//							var Dist10pctileX4 = Dist10pctile*4;
//							if(MA1.IsValidDataPointAt(abar)){
//								double average = MA1.GetValueAt(abar);
//								if(average < Spread.GetValueAt(abar) -Dist10pctileX4 || average > Spread.GetValueAt(abar) + Dist10pctileX4) {MA1.Reset(cb0-abar);}// MA1dots.Reset(cb0-abar);}
//							}
//							if(MA2.IsValidDataPointAt(abar)){
//								double average = MA2.GetValueAt(abar);
//								if(average < Spread.GetValueAt(abar) -Dist10pctileX4 || average > Spread.GetValueAt(abar) + Dist10pctileX4) MA2.Reset(cb0-abar);
//							}
//							#endregion
						}
					}else{
line=2091;
						var reset_prices = CalculateChannelSize(pChannelResetTime);
						CalcDistPctileOnAllTimeperiods(1);
						keyprice = double.NaN;
						H = 0;
						L = 0;
						for(int abar = 2; abar<BarsArray[0].Count-1; abar++){
							int ttt = ToTime(Times[0].GetValueAt(abar))/100;
							GetDistPctile(Times[0].GetValueAt(abar), ref upperDist10pctile, ref lowerDist10pctile);
							SLDistance = (upperDist10pctile+Math.Abs(lowerDist10pctile))/2 * pOuterBandsMult;
							var s = Spread.GetValueAt(abar);
							if(reset_prices.ContainsKey(abar)){
								keyprice = reset_prices[abar];
								H = s;
								L = H;
								AddTo_DiffToKeyLevelbyDay(abar, ttt);
							}
							if(!double.IsNaN(keyprice)){
								H = Math.Max(H, s);
								L = Math.Min(L, s);
								if(pSMAperiod==0) keyprice = (H+L)/2.0;
								KeyLevel[cb0-abar] = keyprice;
								DU[cb0-abar]  = keyprice + upperDist10pctile;
								DL[cb0-abar]  = keyprice + lowerDist10pctile;
								SLU[cb0-abar] = DU[cb0-abar] + SLDistance;
								SLL[cb0-abar] = DL[cb0-abar] - SLDistance;

								if(!DiffToKeyLevel.ContainsKey(ttt))
									DiffToKeyLevel[ttt] = new List<double>(){Spread.GetValueAt(abar) - keyprice};
								else
									DiffToKeyLevel[ttt].Add(Spread.GetValueAt(abar) - keyprice);
							}
						}
					}
					//CalculatePnL();
					if(pSMAperiod <= 0 && IsProVersion){//midline is at the open price of the day and this is ProVersion
						Profile = new SortedDictionary<int,int>();
//DateTime bt = DateTime.MinValue;
//Print("InSession.Count():  "+InSession.Count);
						foreach(var bar in SpreadRange){
//try{
//	bt = Bars.GetTime(bar.Key);
//}catch{continue;}
//var btt = ToTime(bt)/100;
//Print("   bar.Key: "+bar.Key);
							if(InSession.ContainsKey(bar.Key) && InSession[bar.Key]){
								double klvl = KeyLevel.GetValueAt(bar.Key);
								int vH = Convert.ToInt32((bar.Value.Hi - klvl)/ProfileStep)*ProfileStep;
								int vL = Convert.ToInt32(bar.Value.Lo - klvl);
//Print($"  {vH}  to {vL}  from key: {klvl.ToString()}  bar.Hi: {bar.Value.Hi}   bar.Lo: {bar.Value.Lo}");
								while(vH >= vL){
									if(!Profile.ContainsKey(vH)) Profile[vH] = 1;
									else{
										int vv = Profile[vH] + 1;
										Profile[vH] = vv;
									}
									vH = vH - ProfileStep;
								}
							}//else Print("Out of session: "+bt.ToString());
						}
						if(ProfileVAH == int.MinValue){
							var total_tpo = Profile.Sum(k => k.Value) * 0.70;
							var poclvl = Profile.Where(k => k.Value == Profile.Values.Max()).Select(k => k.Key).ToList();
//							Print("poclvl: "+poclvl[0]);
							int sum = Profile[poclvl[0]];
							ProfileVAH = poclvl[0];
							ProfileVAL = poclvl[0];
							while(sum < total_tpo){
								int pi = ProfileVAH + ProfileStep;
								if(Profile.ContainsKey(pi)){
									ProfileVAH = pi;
									sum = sum + Profile[pi];
								}
								pi = ProfileVAL - ProfileStep;
								if(Profile.ContainsKey(pi)){
									ProfileVAL = pi;
									sum = sum + Profile[pi];
								}
							}
						}
					}
					DiffToKeyLevel.Clear();//this isn't needed anymore, it's averages have been saved to DiffToKeyLevelbyDay
					#endregion
				}
line=3194;

				#region -- Gray-out the channel lines when we are outside of the session --
				var t1    = Times[0][cb0>0 ? 1:0];
				var tt1   = ToTime(t1)/100;
				var t0    = Times[0][0];
				var tt0   = ToTime(t0)/100;
				bool cc = (pStartTime==pStopTime) || (tt0 >= pStartTime && tt0 < pStopTime);
				InSession[cb0] = cc;
				
				#endregion

				id = GetChannelRangesId(Times[0][0]);
				if(ChannelRanges[id].Count>0)
				{
line=3214;
					GetDistPctile(id, ref upperDist10pctile, ref lowerDist10pctile);
					if(pSMAperiod > 0){
						DU[0]  = KeyLevel[0]+upperDist10pctile;
						DL[0]  = KeyLevel[0]+lowerDist10pctile;
					}else if(pSMAperiod < 0){
						if(tt1 < pChannelResetTime && tt0 >= pChannelResetTime ||
							pChannelResetTime==tt0 ||
							tt1 > tt0 && pChannelResetTime < tt0 && t1.Day!=t0.Day){
								keyprice = Spread[0];
						}
						KeyLevel[0] = keyprice;
						DU[0]  = KeyLevel[0] + upperDist10pctile;
						DL[0]  = KeyLevel[0] + lowerDist10pctile;
					}else{
						if(tt1 < pChannelResetTime && tt0 >= pChannelResetTime ||
							pChannelResetTime==tt0 ||
							tt1 > tt0 && pChannelResetTime < tt0 && t1.Day!=t0.Day){
								keyprice = Spread[0];
								H = keyprice;
								L = keyprice;
						}else{
							H = Math.Max(H, Spread[0]);
							L = Math.Min(L, Spread[0]);
						}
						KeyLevel[0] = (H+L)/2.0;
						DU[0]  = KeyLevel[0] + upperDist10pctile;
						DL[0]  = KeyLevel[0] + lowerDist10pctile;
					}
					SLU[0] = DU[0]+SLDistance;
					SLL[0] = DL[0]-SLDistance;
				}
				if(AlertABar != cb0 && State == State.Realtime){
					if(Spread[0] > SLU[1] && Spread[1]<=SLU[1] && pSLExitSound != "none"){
						myAlert("BOSAsl"+CurrentBar.ToString(),Priority.High,"Spread SL hit "+pLongContracts+" "+BuySymbol+" - "+pShortContracts+" "+SellSymbol, AddSoundFolder(pSLExitSound), 1, Brushes.Red,Brushes.White);
						AlertABar = cb0;
					}
					if(Spread[0] < SLL[1] && Spread[1]>=SLL[1] && pSLExitSound != "none"){
						myAlert("BOSAsl"+CurrentBar.ToString(),Priority.High,"Spread SL hit "+pLongContracts+" "+BuySymbol+" - "+pShortContracts+" "+SellSymbol, AddSoundFolder(pSLExitSound), 1, Brushes.Red,Brushes.White);
						AlertABar = cb0;
					}
//					if(Spread[0] > DU[1] && Spread[1]<=DU[1] && pEntryShortSound != "none"){
//						myAlert("BOSAshort"+CurrentBar.ToString(),Priority.High,"Spread Short hit "+pLongContracts+" "+BuySymbol+" - "+pShortContracts+" "+SellSymbol, AddSoundFolder(pEntryShortSound), 1, Brushes.Green,Brushes.White);
//						AlertABar = cb0;
//					}
//					if(Spread[0] < DL[1] && Spread[1]>=DL[1] && pEntryLongSound != "none"){
//						myAlert("BOSAlong"+CurrentBar.ToString(),Priority.High,"Spread Long hit "+pLongContracts+" "+BuySymbol+" - "+pShortContracts+" "+SellSymbol, AddSoundFolder(pEntryLongSound), 1, Brushes.Green,Brushes.White);
//						AlertABar = cb0;
//					}
					if(Spread[0] < KeyLevel[1] && Spread[1]>=KeyLevel[1] && pMidlineCrossSound != "none"){
						myAlert("BOSAmid"+CurrentBar.ToString(),Priority.High,"Equilibrium Line hit "+pLongContracts+" "+BuySymbol+" - "+pShortContracts+" "+SellSymbol, AddSoundFolder(pMidlineCrossSound), 1, Brushes.Green,Brushes.Yellow);
						AlertABar = cb0;
					}
				}
			}
			#region -- AutoTrade conditions, and Alert conditions --
//if(pAutoTradeOnBarClose) printDebug(Instruments[0].FullName+"  "+DateTime.Now.ToString()+"  TradingPermitted: "+TradingPermitted.ToString());
			if(IsProVersion && State==State.Realtime){
				if(AskValOnPriorTick == double.MinValue) AskValOnPriorTick = EstimatedBuyPrice;
				if(BidValOnPriorTick == double.MinValue) BidValOnPriorTick = EstimatedSellPrice;
				AlertsMgr.DetermineIfAlertLevelHit(AskValOnPriorTick, BidValOnPriorTick, Spread[0]);

				int offset = pAutoTradeOnBarClose ? 1 : 0;
				var ok_to_trade = AutoTradeEnabled && Spread.IsValidDataPoint(offset) && Spread.IsValidDataPoint(offset+1);
				BottomRightTextStr = string.Empty;
//printDebug("BIP: "+BarsInProgress,true);
//				RemoveDrawObject("BottomRightText");
				if(!ok_to_trade){
					if(AutoTradeEnabled) BottomRightTextStr = "Auto trade suspended - awaiting valid spread price";
				} else {
					PermitHorizLineTrades = DateTime.Now > TimeOfAutoEnabled;
					if(AutoState != BuyOneSellAnother_AutoState.Off && !PermitHorizLineTrades){
						BottomRightTextStr = "Auto trade will activate shortly";
					}else
						BottomRightTextStr = string.Empty;
					if(BarsInProgress == 0 && PermitHorizLineTrades && pAutoTradeOnBarClose ? IsFirstTickOfBar : true) {
//						RemoveDrawObject("BottomRightText");
						bool c1=false, c2=false, c3=false, c4 = false;
						var HLinePresent = !double.IsNaN(HLine_BuyValue) || !double.IsNaN(HLine_SellValue);
						double v = (HLinePresent ? HLine_BuyValue : double.NaN);
						if(!double.IsNaN(v)){
//printDebug("Buy hline price: "+v.ToString("0"), true);
							c1 = BuyHLineEntryType[0]=='S' ? EstimatedBuyPrice > v : false;
							c2 = BuyHLineEntryType[0]=='T' ? AskValOnPriorTick > v && EstimatedBuyPrice <= v || AskValOnPriorTick < v && EstimatedBuyPrice >= v : false;
						}
						if(c1 || c2 || BuyNow) {
if(pAutoTradeOnBarClose) printDebug("c 1,2, BuyNow:  "+c1.ToString()+"  "+c2.ToString()+"  "+BuyNow.ToString(), 1, true);
							BuyNow = false;
							if(AutoState == BuyOneSellAnother_AutoState.BuyOnly || 
							 AutoState == BuyOneSellAnother_AutoState.Add && CurrentSpreadPosition >=0
							) {
								AutoState = BuyOneSellAnother_AutoState.Off;
	printDebug("------------------------------------------------------------------");
	printDebug("------------------------- Going LONG -----------------  Buy "+Instruments[0].FullName+"  Sell "+Instruments[1].FullName);
	printDebug("------------------------------------------------------------------");
								Log("HLine hit on "+BuySymbol+"-"+SellSymbol+":  "+AutoState.ToString(), LogLevel.Information);
								OnButtonClick("Long", null);
								UpdateAutoTradeButton(BuyOneSellAnother_AutoState.Off);
								TriggerCustomEvent(o2 =>{
									Draw.ArrowUp(this,"AutoTradeBuy"+DateTime.Now.ToString(),false,0,Spread[0],Brushes.Lime);
								},0,null);
							}else if(AutoState == BuyOneSellAnother_AutoState.Close && CurrentSpreadPosition < 0){
								AutoState = BuyOneSellAnother_AutoState.Off;
	printDebug("------------------------------------------------------------------");
	printDebug("------------------------- Closing a SHORT -----------------  Buy "+CurrentShortSize+" "+Instruments[0].FullName+"  Sell "+CurrentLongSize+" "+Instruments[1].FullName);
	printDebug("------------------------------------------------------------------");
								Log("HLine hit, going flat on "+BuySymbol+"-"+SellSymbol+":  "+AutoState.ToString(), LogLevel.Information);
								TradeOneInstrument("Close", 'B', 0, CurrentShortSize);
								TradeOneInstrument("Close", 'S', 1, CurrentLongSize);
								TriggerCustomEvent(o2 =>{
									Draw.ArrowUp(this,"AutoTradeClose"+DateTime.Now.ToString(),false,0,Spread[0],Brushes.Lime);
								},0,null);
							}
						} else {
							v = (HLinePresent ? HLine_SellValue : double.NaN);
//printDebug("SellValue v: "+v.ToString()+"  HLinePresent? "+HLinePresent.ToString());
							if(!double.IsNaN(v)){
//printDebug("Sell hline price: "+v.ToString("0"));
								c1 = SellHLineEntryType[0]=='S' ? EstimatedSellPrice < v : false;
								c2 = SellHLineEntryType[0]=='T' ? BidValOnPriorTick > v && EstimatedSellPrice <= v || BidValOnPriorTick < v && EstimatedSellPrice >= v : false;
							}
							if(c1 || c2 || SellNow){ 
printDebug("c 1,2, SellNow:  "+c1.ToString()+"  "+c2.ToString()+"  "+SellNow.ToString());
if(c2)printDebug("c2 is true, SpreadValPriorTick = "+BidValOnPriorTick.ToString("0")+"  EstSellPrice: "+EstimatedSellPrice.ToString("0")+"  HLine: "+v.ToString("0"));
								SellNow = false;
								if(AutoState == BuyOneSellAnother_AutoState.SellOnly || 
								 AutoState == BuyOneSellAnother_AutoState.Add && CurrentSpreadPosition <=0)
								{
									AutoState = BuyOneSellAnother_AutoState.Off;
printDebug("-------------------------------------------------------------------");
printDebug("------------------------- Going SHORT -----------------  "+Instruments[0].FullName);
printDebug("-------------------------------------------------------------------");
									Log("HLine hit on "+BuySymbol+"-"+SellSymbol+":  "+AutoState.ToString(), LogLevel.Information);
									OnButtonClick("Short", null);
									UpdateAutoTradeButton(BuyOneSellAnother_AutoState.Off);
									TriggerCustomEvent(o2 =>{
										Draw.ArrowDown(this,"AutoTradeSell"+DateTime.Now.ToString(),false,0,Spread[0],Brushes.Magenta);
									},0,null);
								}else if(AutoState == BuyOneSellAnother_AutoState.Close && CurrentSpreadPosition > 0){
									AutoState = BuyOneSellAnother_AutoState.Off;
printDebug("------------------------------------------------------------------");
printDebug("------------------------- Closing a LONG -----------------  Sell "+CurrentLongSize+" "+Instruments[0].FullName+"  Buy "+CurrentShortSize+" "+Instruments[1].FullName);
printDebug("------------------------------------------------------------------");
									Log("HLine hit, going flat on "+BuySymbol+"-"+SellSymbol+":  "+AutoState.ToString(), LogLevel.Information);
									TradeOneInstrument("Close", 'S', 0, CurrentLongSize);
									TradeOneInstrument("Close", 'B', 1, CurrentShortSize);
									TriggerCustomEvent(o2 =>{
										Draw.ArrowDown(this,"AutoTradeClose"+DateTime.Now.ToString(),false,0,Spread[0],Brushes.Lime);
									},0,null);
								}
							}
						}
					}
				}
				//if(BottomRightTextStr!=string.Empty) {
//Print("BRTS: "+BottomRightTextStr);
					//DrawTextFixed("BottomRightText", BottomRightTextStr, TextPosition.BottomRight);
				//}
				AskValOnPriorTick = EstimatedBuyPrice;
				BidValOnPriorTick = EstimatedSellPrice;
			}
			#endregion
}catch(Exception e){printDebug(line+": "+BuySymbol+"-"+SellSymbol+"\n"+e.ToString());}
		}

//==============================================================================================================================================
		private void AddTo_DiffToKeyLevelbyDay(int abar, int ttt, bool print = false){
			if(!DiffToKeyLevelbyDay.ContainsKey(abar)){
				DiffToKeyLevelbyDay[abar] = new SortedDictionary<int,double>();
			}
			if(DiffToKeyLevelbyDay.Count>0){
				foreach(var dif in DiffToKeyLevel){
					double avg = dif.Value.Average();
//if(print)Print(abar+":  "+dif.Key+":  "+avg);
					DiffToKeyLevelbyDay.Last().Value[dif.Key] = avg;
				}
			}
		}
//==============================================================================================================================================
		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			Alert(id,prio,msg, wav,rearmSeconds, bkgBrush, foregroundBrush);
			printDebug(string.Format("Alert: {0}   wav: {1}",msg,wav));
		}
//==============================================================================================================================================
		private void AddToOverallHL(int cb, double max, double min){
			if(OverallHL.Count==0){
				OverallHL[cb] = new Tuple<double,double>(max, min);
			}else{
				if(max != OverallHL.Last().Value.Item1 || min != OverallHL.Last().Value.Item2){
					OverallHL[cb] = new Tuple<double,double>(max,min);
				}
			}
		}
//==============================================================================================================================================
		private SharpDX.Direct2D1.Brush txtBrushDX, BlackBrushDX, DimGrayBrushDX, LimeBrushDX, CyanBrushDX, BlueBrushDX, YellowBrushDX, GreenBrushDX, MaroonBrushDX, PosExpectedDiffBrushDX, NegExpectedDiffBrushDX;
		private SharpDX.Direct2D1.Brush MagentaBrushDX, BuyValueBrushDX, SellValueBrushDX;
		private SharpDX.Direct2D1.Brush GridBrushDX, MA1UpDotsBrushDX, MA1DnDotsBrushDX, MA1LineBrushDX, MA2LineBrushDX, TrendSignalUpDotsDXBrush, TrendSignalDnDotsDXBrush;

		public override void OnRenderTargetChanged()
		{
			#region == OnRenderTargetChanged ==
			if(BlackBrushDX!=null   && !BlackBrushDX.IsDisposed)    {BlackBrushDX.Dispose();   BlackBrushDX=null;}
			if(YellowBrushDX!=null   && !YellowBrushDX.IsDisposed)    {YellowBrushDX.Dispose();YellowBrushDX=null;}
			if(PosExpectedDiffBrushDX!=null   && !PosExpectedDiffBrushDX.IsDisposed)    {PosExpectedDiffBrushDX.Dispose();PosExpectedDiffBrushDX=null;}
			if(NegExpectedDiffBrushDX!=null   && !NegExpectedDiffBrushDX.IsDisposed)    {NegExpectedDiffBrushDX.Dispose();NegExpectedDiffBrushDX=null;}
			
			if(CyanBrushDX!=null && !CyanBrushDX.IsDisposed)  {CyanBrushDX.Dispose(); CyanBrushDX=null;}
			if(DimGrayBrushDX!=null && !DimGrayBrushDX.IsDisposed)  {DimGrayBrushDX.Dispose(); DimGrayBrushDX=null;}
			if(LimeBrushDX!=null    && !LimeBrushDX.IsDisposed)     {LimeBrushDX.Dispose();    LimeBrushDX = null;}
			if(GreenBrushDX!=null    && !GreenBrushDX.IsDisposed)   {GreenBrushDX.Dispose();   GreenBrushDX = null;}
			if(BlueBrushDX!=null    && !BlueBrushDX.IsDisposed)     {BlueBrushDX.Dispose();    BlueBrushDX = null;}
			if(MagentaBrushDX!=null && !MagentaBrushDX.IsDisposed)  {MagentaBrushDX.Dispose(); MagentaBrushDX = null;}
			if(MaroonBrushDX!=null && !MaroonBrushDX.IsDisposed)  {MaroonBrushDX.Dispose(); MaroonBrushDX = null;}
			if(txtBrushDX!=null     && !txtBrushDX.IsDisposed)      {txtBrushDX.Dispose();     txtBrushDX=null;}
			if(BuyValueBrushDX  != null && !BuyValueBrushDX.IsDisposed)  {BuyValueBrushDX.Dispose();  BuyValueBrushDX=null;}
			if(SellValueBrushDX != null && !SellValueBrushDX.IsDisposed) {SellValueBrushDX.Dispose(); SellValueBrushDX=null;}
			if(MA1UpDotsBrushDX != null && !MA1UpDotsBrushDX.IsDisposed) {MA1UpDotsBrushDX.Dispose(); MA1UpDotsBrushDX=null;}
			if(MA1DnDotsBrushDX != null && !MA1DnDotsBrushDX.IsDisposed) {MA1DnDotsBrushDX.Dispose(); MA1DnDotsBrushDX=null;}
			if(GridBrushDX != null && !GridBrushDX.IsDisposed) {GridBrushDX.Dispose(); GridBrushDX=null;}
			if(MA1LineBrushDX != null && !MA1LineBrushDX.IsDisposed) {MA1LineBrushDX.Dispose(); MA1LineBrushDX=null;}
			if(MA2LineBrushDX != null && !MA2LineBrushDX.IsDisposed) {MA2LineBrushDX.Dispose(); MA2LineBrushDX=null;}
			if(TrendSignalUpDotsDXBrush != null && !TrendSignalUpDotsDXBrush.IsDisposed) {TrendSignalUpDotsDXBrush.Dispose(); TrendSignalUpDotsDXBrush=null;}
			if(TrendSignalDnDotsDXBrush != null && !TrendSignalDnDotsDXBrush.IsDisposed) {TrendSignalDnDotsDXBrush.Dispose(); TrendSignalDnDotsDXBrush=null;}

			if(RenderTarget != null) txtBrushDX     = Brushes.Yellow.ToDxBrush(RenderTarget);
			if(RenderTarget != null) BlackBrushDX   = Brushes.Black.ToDxBrush(RenderTarget);
			if(RenderTarget != null) YellowBrushDX  = Brushes.Yellow.ToDxBrush(RenderTarget);
			if(RenderTarget != null) {PosExpectedDiffBrushDX  = Brushes.Green.ToDxBrush(RenderTarget); PosExpectedDiffBrushDX.Opacity = 0.5f;}
			if(RenderTarget != null) {NegExpectedDiffBrushDX  = Brushes.Red.ToDxBrush(RenderTarget); NegExpectedDiffBrushDX.Opacity = 0.5f;}
			if(RenderTarget != null) DimGrayBrushDX = Brushes.DimGray.ToDxBrush(RenderTarget);
			if(RenderTarget != null) CyanBrushDX   = Brushes.Cyan.ToDxBrush(RenderTarget);
			if(RenderTarget != null) LimeBrushDX    = Brushes.Lime.ToDxBrush(RenderTarget);
			if(RenderTarget != null) GreenBrushDX    = Brushes.Green.ToDxBrush(RenderTarget);
			if(RenderTarget != null) BlueBrushDX    = Brushes.Blue.ToDxBrush(RenderTarget);
			if(RenderTarget != null) MagentaBrushDX = Brushes.Magenta.ToDxBrush(RenderTarget);
			if(RenderTarget != null) MaroonBrushDX = Brushes.Maroon.ToDxBrush(RenderTarget);
			if(RenderTarget != null) BuyValueBrushDX  = Brushes.Cyan.ToDxBrush(RenderTarget);
			if(RenderTarget != null) SellValueBrushDX = Brushes.Pink.ToDxBrush(RenderTarget);
			if(RenderTarget != null) MA1UpDotsBrushDX = pMA1UpDots.ToDxBrush(RenderTarget);
			if(RenderTarget != null) MA1DnDotsBrushDX = pMA1DnDots.ToDxBrush(RenderTarget);
			if(RenderTarget != null) {GridBrushDX = pGridlineBrush.ToDxBrush(RenderTarget); GridBrushDX.Opacity = pGridlineOpacity;}
			if(RenderTarget != null) MA1LineBrushDX = pMA1LineColor.ToDxBrush(RenderTarget);
			if(RenderTarget != null) MA2LineBrushDX = pMA2LineColor.ToDxBrush(RenderTarget);
			if(RenderTarget != null) TrendSignalUpDotsDXBrush = pTrendSignalUpDotsBrush.ToDxBrush(RenderTarget);
			if(RenderTarget != null) TrendSignalDnDotsDXBrush = pTrendSignalDnDotsBrush.ToDxBrush(RenderTarget);

			#endregion
		}
//=====================                     ============================================================================
		private double CalculateCorrelation(){
			#region -- CalculateCorrelation --
			int count = 0;
			int congruent_dir_count = 0;
			double c0 = 0;
			double c1 = 0;
			if(pCorrelationTimeMinutes == 0){
				for(int abar = 0; abar<BarsArray[0].Count-1; abar++){
					try{
						var dt = BarsArray[0].GetTime(abar);
						int abar1 = BarsArray[1].GetBar(dt);
						c0 = Closes[0].GetValueAt(abar);
						c1 = Closes[1].GetValueAt(abar1);
						var o0 = Opens[0].GetValueAt(abar);
						var o1 = Opens[1].GetValueAt(abar1);
						if(     c0 > o0 && c1 > o1) {congruent_dir_count++;}// printDebug(string.Format("c0 {0} > o0 {1}  &&  c1 {2} > o1 {3}  congruentcount {4}",c0,o0,c1,o1,congruent_dir_count));}
						else if(c0 < o0 && c1 < o1) {congruent_dir_count++;}// printDebug(string.Format("c0 {0} < o0 {1}  &&  c1 {2} < o1 {3}  congruentcount {4}",c0,o0,c1,o1,congruent_dir_count));}
						else if(c0 < o0 && c1 > o1) {congruent_dir_count--;}// printDebug(string.Format("c0 {0} < o0 {1}  &&  c1 {2} > o1 {3}  congruentcount {4}",c0,o0,c1,o1,congruent_dir_count));}
						else if(c0 > o0 && c1 < o1) {congruent_dir_count--;}// printDebug(string.Format("c0 {0} > o0 {1}  &&  c1 {2} < o1 {3}  congruentcount {4}",c0,o0,c1,o1,congruent_dir_count));}
						count++;
					}catch(Exception e){printDebug("CalculateCorrelation "+e.ToString());}
				}
			}else{
				double priorc0 = 0;
				double priorc1 = 0;
				foreach(var kvp in CorrelationDict){
					if(c0!=0) priorc0 = c0;
					if(c1!=0) priorc1 = c1;
					c0 = kvp.Value[0];
					c1 = kvp.Value[1];
					if(     c0 > priorc0 && c1 > priorc1) {congruent_dir_count++;}// printDebug(string.Format("c0 {0} > pc0 {1}  &&  c1 {2} > pc1 {3}  congruentcount {4}",c0,priorc0,c1,priorc1,congruent_dir_count));}
					else if(c0 < priorc0 && c1 < priorc1) {congruent_dir_count++;}// printDebug(string.Format("c0 {0} < pc0 {1}  &&  c1 {2} < pc1 {3}  congruentcount {4}",c0,priorc0,c1,priorc1,congruent_dir_count));}
					else if(c0 < priorc0 && c1 > priorc1) {congruent_dir_count--;}// printDebug(string.Format("c0 {0} < pc0 {1}  &&  c1 {2} > pc1 {3}  congruentcount {4}",c0,priorc0,c1,priorc1,congruent_dir_count));}
					else if(c0 > priorc0 && c1 < priorc1) {congruent_dir_count--;}// printDebug(string.Format("c0 {0} > pc0 {1}  &&  c1 {2} < pc1 {3}  congruentcount {4}",c0,priorc0,c1,priorc1,congruent_dir_count));}
				}
				count = CorrelationDict.Count;
			}
			CorrelationDict.Clear();
			CorrelationDict = null;//this clears memory and stops the correlation calculation from consuming cycles
			double corr = congruent_dir_count*1.0/count;
//printDebug(string.Format("{0}-{1}:  Count: {2}  congruent: {3}  Correl: {4}", BuySymbol, SellSymbol, count, congruent_dir_count, corr.ToString("0.00%").Replace("0%","%")));
			return corr;
			#endregion
		}
//==============================        ============================================================================
		SimpleFont font = new SimpleFont("Arial",15);
		SharpDX.Vector2 v,v1,v2;
		double Spread0 = 0;
		string tag = "";
		IDrawingTool o = null;
		class AlertManager
		{
			public AlertManager(NinjaTrader.NinjaScript.IndicatorBase parent){Parent = parent;}
			public NinjaTrader.NinjaScript.IndicatorBase Parent;
			public class AlertInfo{
				public string Wav = "";
				public double Price = 0;
				public int AlertABar = 0;
				public AlertInfo(string wav, double price, int alertabar){Wav=wav; Price=price; AlertABar=alertabar;}
			}
			public SortedDictionary<string, AlertInfo> AlertLevels = new SortedDictionary<string, AlertInfo>();
			public void DeleteOldLevels(int cbar, int MaxAgeInBars){
				var keys_to_delete = new List<string>();
				foreach(var al in AlertLevels){
					if(al.Value.AlertABar < cbar-MaxAgeInBars) keys_to_delete.Add(al.Key);//turns-off this level
				}
				foreach(var al in keys_to_delete) AlertLevels.Remove(al);
			}
			private string StripOutIllegalCharacters(string name, string ReplacementString){
				#region strip
				char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
				string invalids = string.Empty;
				foreach(char ch in invalidPathChars){
					invalids += ch.ToString();
				}
	//			Print("Invalid chars: '"+invalids+"'");
				string result = string.Empty;
				for(int c=0; c<name.Length; c++) {
					if(!invalids.Contains(name[c].ToString())) result += name[c];
					else result += ReplacementString;
				}
				return result;
				#endregion
			}
			public void DetermineIfAlertLevelHit(double AskValOnPriorTick, double BidValOnPriorTick, double SpreadVal){
				var kvps = AlertLevels.Where(k=>k.Value.AlertABar<Parent.CurrentBars[0] && 
								SpreadVal > k.Value.Price &&
								BidValOnPriorTick < k.Value.Price).ToList();
				foreach(var kvp in kvps){
					double p = kvp.Value.Price;
					var wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(kvp.Value.Wav.Replace("<inst1>",Parent.Instruments[0].MasterInstrument.Name).Replace("<inst2>",Parent.Instruments[1].MasterInstrument.Name)," "));
					Parent.Alert(DateTime.Now.Ticks.ToString(), Priority.High, "BOSA Alert hit at "+Parent.Instrument.MasterInstrument.FormatPrice(p), wav, 0, Brushes.DimGray, Brushes.Black);
					kvp.Value.AlertABar = Parent.CurrentBars[0];
				}
			}
			public void AddLevel(string tag, double price, string wav, int cbar){
				if(AlertLevels.ContainsKey(tag)) AlertLevels[tag].Price = price;
				else AlertLevels[tag] = new AlertInfo(wav, price, cbar);//populate the AlertLevels dictionary
			}
		}
		private AlertManager AlertsMgr;
		private bool FirstExecutionOfOnRender = true;
		private float TPOsize = 10f;
		//----------------------------------------------------------------------------------
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if(!ValidLicense || TerminalError){
				return;
			}
			if(LaunchedAt != DateTime.MaxValue){
line=3982;
				var ts = new TimeSpan(DateTime.Now.Ticks - LaunchedAt.Ticks);
				UpdateUI(); 
				if(ts.TotalSeconds>4){
					RemoveDrawObject("licwarning");
					LaunchedAt = DateTime.MaxValue;
				}
			}
			v = new SharpDX.Vector2(0,0);
			v1 = new SharpDX.Vector2(0,0);
			v2 = new SharpDX.Vector2(0,0);
			cb0 = CurrentBars[0];
			SharpDX.Direct2D1.AntialiasMode OSM = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
try{
//			if(btn_BuySpread != null)  btn_BuySpread.Content = string.Format("BUY {0}x", SelectedSpreadQty);
//			if(btn_SellSpread != null) btn_SellSpread.Content = string.Format("SELL {0}x", SelectedSpreadQty);
line=4001;
			if(FirstExecutionOfOnRender){
				UpdateUI();
				FirstExecutionOfOnRender = false;
			}

			int RMaB = Math.Min(ChartBars.ToIndex, cb0);
			float RMx = 0;
			float x = 0f;
			float x1 = float.MinValue;
			float y = 0f;
			float y0 = 0f;
line=4013;

			#region -- Print MA lines, and the top and bottom range of historical tick data --
			for(int i = Math.Max(0,ChartBars.FromIndex); i <= ChartBars.ToIndex; i++){
				x = chartControl.GetXByBarIndex(ChartBars, i);
				v.X = x;
				v1.X = x;
				if(!IsShiftPressed){
				#region -- Draw each spread "price bar" HiLo -- (drawn first here so that the closing Spread value prints on top of these HiLo bars)
				if(SpreadRange.ContainsKey(i)){
					double sv = Spread.GetValueAt(i);
					var a = SpreadRange[i];
//printDebug(Times[0].GetValueAt(i).ToString()+"   upper "+a[0]+"  lower: "+a[1]);
					if(a.Hi==a.Lo) continue;
//if(i == ChartBars.FromIndex+2) printDebug("Top: "+a[0]+"   bot: "+a[1]);
					y0 = (float)chartScale.GetYByValue(sv);
					v.Y = y0;
					y = (float)chartScale.GetYByValue(Math.Max(sv, a.Hi));
//					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y), 2f,2f), Plots[8].BrushDX);
					v1.Y = y;
					RenderTarget.DrawLine(v1, v, Plots[8].BrushDX,2f);
					y = (float)chartScale.GetYByValue(Math.Min(sv, a.Lo));
//					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y), 2f,2f), Plots[9].BrushDX);
					RenderTarget.DrawLine(v1, v, Plots[9].BrushDX,2f);
					#region -- Show FastMATrendSignals --
					if(pFastMASignalDelayInBars>0 && FastMATrendSignals.ContainsKey(i)){
						if(FastMATrendSignals[i]=='U'){
							y = (float)chartScale.GetYByValue(a.Lo-pTrendSignalDotSize*2f);
							v.Y = y;
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, pTrendSignalDotSize, pTrendSignalDotSize), TrendSignalUpDotsDXBrush);
						}else{
							y = (float)chartScale.GetYByValue(a.Hi+pTrendSignalDotSize*2f);
							v.Y = y;
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, pTrendSignalDotSize, pTrendSignalDotSize), TrendSignalDnDotsDXBrush);
						}
					}
					#endregion
				}else
					printDebug("SpreadRange didn't contain info for bar: "+i);
				#endregion
				}

				if(x1 != float.MinValue){
					var insess = false;
					if(InSession.ContainsKey(i)) insess = InSession[i];

line=2790;
					#region -- Draw midline, top of channel, bottom of channel, SL lines, spread close prices --
					//midline of the channel
					y = (float)chartScale.GetYByValue(KeyLevel.GetValueAt(i-1));
					y0 = (float)chartScale.GetYByValue(KeyLevel.GetValueAt(i));
					v.X = x1; v.Y = y;
					v1.X = x; v1.Y = y0;
					RenderTarget.DrawLine(v1, v, insess ? Plots[0].BrushDX:DimGrayBrushDX, Plots[0].Width);
					//top of channel
					y0 = (float)chartScale.GetYByValue(DU.GetValueAt(i));
					v.X = x; v.Y = y0;
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, Plots[1].Width, Plots[1].Width), insess ? Plots[1].BrushDX:DimGrayBrushDX);
					//bottom of channel
					y0 = (float)chartScale.GetYByValue(DL.GetValueAt(i));
					v.X = x; v.Y = y0;
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, Plots[2].Width, Plots[2].Width), insess ? Plots[2].BrushDX:DimGrayBrushDX);
					//top of channel stoploss line
					y = (float)chartScale.GetYByValue(SLU.GetValueAt(i-1));
					y0 = (float)chartScale.GetYByValue(SLU.GetValueAt(i));
					v.X = x1; v.Y = y;
					v1.Y = y0;
					RenderTarget.DrawLine(v1, v, insess ? Plots[3].BrushDX:DimGrayBrushDX, Plots[3].Width);
					//bottom of channel stoploss line
					y = (float)chartScale.GetYByValue(SLL.GetValueAt(i-1));
					y0 = (float)chartScale.GetYByValue(SLL.GetValueAt(i));
					v.X = x1; v.Y = y;
					v1.Y = y0;
					RenderTarget.DrawLine(v1, v, insess ? Plots[4].BrushDX:DimGrayBrushDX, Plots[4].Width);
					//Spread line (close price of the spread)
					if(Plots[5].PlotStyle == PlotStyle.Hash){
						float w = (x-x1)/3f;
						y0 = (float)chartScale.GetYByValue(Spread.GetValueAt(i));
						v.X = x+w; v.Y = y0;
						v1.X = x-w; v1.Y = y0;
						RenderTarget.DrawLine(v1, v, Plots[5].BrushDX, Plots[5].Width);
					}else if(Plots[5].PlotStyle == PlotStyle.Dot){
						y0 = (float)chartScale.GetYByValue(Spread.GetValueAt(i));
						v.X = x; v.Y = y0;
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, Plots[5].Width, Plots[5].Width), Plots[5].BrushDX);
					}else {//all other plot styles get rendered as a line
						y = (float)chartScale.GetYByValue(Spread.GetValueAt(i-1));
						y0 = (float)chartScale.GetYByValue(Spread.GetValueAt(i));
						v.X = x1; v.Y = y;
						v1.X = x; v1.Y = y0;
						RenderTarget.DrawLine(v1, v, Plots[5].BrushDX, Plots[5].Width);
					}
					#endregion
line=2800;
					if(!IsShiftPressed){
						#region -- draw MA lines --
						if(this.pMA1period>0 && i > MA1FirstABar){
							//MA1 line
							double ma1 = MA1.GetValueAt(i-1);
							double ma0 = MA1.GetValueAt(i);
							y = (float)chartScale.GetYByValue(ma1);
							y0 = (float)chartScale.GetYByValue(ma0);
							v1.X = x; v1.Y = y0;
							v.X = x1; v.Y = y;
							RenderTarget.DrawLine(v1, v, MA1LineBrushDX, pMA1LineWidth);
							//MA1 dots
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 3f, 3f), ma0 > ma1 ? MA1UpDotsBrushDX : MA1DnDotsBrushDX);
						}
						if(this.pMA2period>0 && i > MA2FirstABar){
							y = (float)chartScale.GetYByValue(MA2.GetValueAt(i-1));
							y0 = (float)chartScale.GetYByValue(MA2.GetValueAt(i));
							v1.X = x; v1.Y = y0;
							v.X = x1; v.Y = y;
							RenderTarget.DrawLine(v1, v, Plots[7].BrushDX, Plots[7].Width);
						}
						#endregion
					}
				}
				x1 = x;
			}
			#endregion

line=4099;
			if(pShowReversalDots && IsProVersion){
				#region -- Reversal signal dots --
				SRdata a = null;
				List<KeyValuePair<int,double>> kvplist = null;
				try{
					kvplist = cci.Where(k=>k.Key > ChartBars.FromIndex && k.Key<=ChartBars.ToIndex && k.Value[2] !=0).Select(k=> new KeyValuePair<int,double>(k.Key, k.Value[2])).ToList();
				}catch(Exception rr){Print("3707  error: "+rr.ToString()); kvplist=null;}
				if(kvplist!=null){
					for(int k = 0; k<kvplist.Count; k++){
//					foreach(var kvp in kvplist) {//the new kvp is absolute bar number and signal value (+1 for buy, -1 for sell)
						x = chartControl.GetXByBarIndex(ChartBars, kvplist[k].Key);
						y = 0f;
						double svT = double.NaN;
						double svB = double.NaN;
						if(SpreadRange.ContainsKey(kvplist[k].Key)){
							a = SpreadRange[kvplist[k].Key];
							if(a.Hi==0) a.Hi = Spread.GetValueAt(kvplist[k].Key);
							if(a.Lo==0) a.Lo = Spread.GetValueAt(kvplist[k].Key);
//printDebug("a[0]: "+a[0]+"  a[1]: "+a[1]);
						}else{
							svT = HiLoBarTop.GetValueAt(kvplist[k].Key);
							svB = HiLoBarBot.GetValueAt(kvplist[k].Key);
						}
						if(kvplist[k].Value > 0) {
							//y = (float)chartScale.GetYByValue(double.IsNaN(svB) ? a.Lo : svB) + 20f;
							y = (float)chartScale.GetYByValue(Spread.GetValueAt(kvplist[k].Key));
							v.X = x; v.Y = y;
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 2f, 5f), kvplist[k].Value == 1 ?LimeBrushDX : GreenBrushDX);
						}
						if(kvplist[k].Value < 0) {
							//y = (float)chartScale.GetYByValue(double.IsNaN(svT) ? a.Hi : svT) - 20f;
							y = (float)chartScale.GetYByValue(Spread.GetValueAt(kvplist[k].Key));
							v.X = x; v.Y = y;
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 2f, 5f), kvplist[k].Value == -1 ?MagentaBrushDX : MaroonBrushDX);
						}
					}
				}
				#endregion
			}
			bool pShowOverallMinMax = true;
			#region -- show overall min/max levels --
			if(pShowOverallMinMax){
				var keys = OverallHL.Keys.Where(k=>k < RMaB).ToList();
				if(keys!=null && keys.Count>0){
					y = (float)chartScale.GetYByValue(OverallHL[keys.Max()].Item1);
					v1.X = chartControl.ChartPanels[chartScale.PanelIndex].W;
					v1.Y = y;
					v2.X = v1.X;
					v2.Y = y;
					RenderTarget.DrawLine(v1, v2, MagentaBrushDX, 5f);
					y = (float)chartScale.GetYByValue(OverallHL[keys.Max()].Item2);
					v1.Y = y;
					v2.Y = y;
					RenderTarget.DrawLine(v1, v2, LimeBrushDX, 5f);
					y = (float)chartScale.GetYByValue((OverallHL[keys.Max()].Item1+OverallHL[keys.Max()].Item2)/2);
					v1.Y = y;
					v2.Y = y;
					RenderTarget.DrawLine(v1, v2, BlueBrushDX, 2f);
				}
			}
			#endregion

			double channel_price = double.MinValue;
			if(pShowSpreadHistory){
				#region -- SpreadHistory Dots --
				//We want to print the average location of the spread to the keylevel, on each bar on the chart
				int max_key = -1;
				var keylist = DiffToKeyLevelbyDay.Where(k=>k.Key < ChartBars.FromIndex).Select(k=>k.Key).ToList();
try{
//for(int f = 0; f<keylist.Count; f++)Print("  key: "+keylist[f]+"    "+Times[0].GetValueAt(keylist[f]).ToString());
				if(keylist != null && keylist.Count>0) max_key = keylist.Max();
				int tt0 = 0;
				int last_tt = 0;
				int i = 0;
				PosExpectedDiffBrushDX.Opacity = 0.5f;
				NegExpectedDiffBrushDX.Opacity = 0.5f;
				for(i = Math.Max(0,ChartBars.FromIndex); i <= ChartBars.ToIndex; i++){
					if(DiffToKeyLevelbyDay.ContainsKey(i)){
						max_key = i;
					}
					if(max_key>=0){//max_key points to the highest DiffToKeyLevelbyDay that is prior to bar i
						tt0 = ToTime(Times[0].GetValueAt(i))/100;
						last_tt = tt0;
						if(DiffToKeyLevelbyDay[max_key].ContainsKey(tt0)){
//Print(Times[0].GetValueAt(i).ToString()+"   max_key: "+max_key+"   time: "+Times[0].GetValueAt(max_key).ToString());
							x = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(Times[0].GetValueAt(i)));
							channel_price = KeyLevel.GetValueAt(i);
							y = chartScale.GetYByValue(channel_price + DiffToKeyLevelbyDay[max_key][tt0]);
							v.X = x; v.Y = y;
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 4f, 1f), DiffToKeyLevelbyDay[max_key][tt0] > 0 ? PosExpectedDiffBrushDX : NegExpectedDiffBrushDX);
						}//else Print("DiffToKeyLevelbyDay did not contain key: "+tt0);
					}
				}
				if(CurrentBars[0] <= ChartBars.ToIndex){
					keylist = DiffToKeyLevelbyDay[max_key].Where(k=>k.Key > last_tt).Select(k=>k.Key).ToList();
					float width = (chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex) - chartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex)) / (ChartBars.ToIndex-ChartBars.FromIndex);
					PosExpectedDiffBrushDX.Opacity = 1f;
					NegExpectedDiffBrushDX.Opacity = 1f;
					foreach(var tt0key in keylist){
						x = x + width;
						y = chartScale.GetYByValue(channel_price);
						v.X = x; v.Y = y;
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 4f, 1f), Plots[0].BrushDX);
						y = chartScale.GetYByValue(channel_price + DiffToKeyLevelbyDay[max_key][tt0key]);
						v.Y = y;
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 4f, 1f), DiffToKeyLevelbyDay[max_key][tt0key] > 0 ? PosExpectedDiffBrushDX : NegExpectedDiffBrushDX);
						i++;
					}
				}
}catch(Exception kee){Print(kee.ToString());}
				#endregion
			}
			if(pShowSessionTPO && Profile.Count>0){
				channel_price = KeyLevel.GetValueAt(RMaB);
//Print(Profile.Keys.Max()+"  to  "+Profile.Keys.Min());
				foreach(var histobar in Profile){// = new SortedDictionary<int,int>();
					x = ChartPanel.W - histobar.Value/TPOsize;
					y = chartScale.GetYByValue(channel_price + histobar.Key);
					v.X = x; v.Y = y;
					if(histobar.Key <= ProfileVAH && histobar.Key >= ProfileVAL)
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 4f, 4f), CyanBrushDX);
					else
						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 2f, 2f), Plots[0].BrushDX);
				}
			}
			var txtFormat = font.ToDirectWriteTextFormat();
			SharpDX.RectangleF labelRect;
			SharpDX.DirectWrite.TextLayout txtLayout = null;

			#region -- Find drawn line segment objects (to show dollar value), and draw price levels on horizontal lines --
			double p0 = 0;
			DrawOnPricePanel = false;
			RemoveDrawObject("info");
			bool IsBuyLineFound = false;
			bool IsSellLineFound = false;
			HLine_BuyValue = double.MinValue;
			HLine_SellValue = double.MaxValue;
			double dist_NearestSellLevel = double.MaxValue;
			double dist_NearestBuyLevel = double.MaxValue;
			var NoBuys = AutoState == BuyOneSellAnother_AutoState.Off || !ButtonSettings[btn_BuySpread_Name].Enabled;//AutoState == BuyOneSellAnother_AutoState.Off || AutoState == BuyOneSellAnother_AutoState.SellOnly || AutoState == BuyOneSellAnother_AutoState.Close && CurrentSpreadPosition >=0;
			var NoSells = AutoState == BuyOneSellAnother_AutoState.Off || !ButtonSettings[btn_SellSpread_Name].Enabled;//AutoState == BuyOneSellAnother_AutoState.Off || AutoState == BuyOneSellAnother_AutoState.BuyOnly || AutoState == BuyOneSellAnother_AutoState.Close && CurrentSpreadPosition <=0;

//			var objects = DrawObjects.ToList();
			var objects = DrawObjects.Where(xo=>xo.Tag.Contains("buy") || xo.Tag.Contains("sell") ||  xo.Tag.Contains(".wav") || xo.ToString().Contains(".Line") || xo.ToString().Contains(".Triangle")).ToList();
			o = null;
			if(objects!=null){
				AlertsMgr.DeleteOldLevels(CurrentBars[0], 1);
				var nodes = new SortedDictionary<double,Point>();

				//NinjaTrader.NinjaScript.DrawingTools.Line L = null;
//printDebug("Objects.count: "+objects.Count);
line=4241;
				HLine_BuyValue = double.NaN;
				HLine_SellValue = double.NaN;
//				foreach (dynamic dob in objects) {
				float diff = 0f;
				for(int idx = 0; idx<objects.Count; idx++){
//printDebug(objects[idx].Tag+" found");
//try{
					if (objects[idx].ToString().EndsWith(".Triangle")) {
						#region -- Triangle signals --
						nodes.Clear();
						o = (IDrawingTool)objects[idx];
						tag = o.Tag.ToLower();
try{
line=4254;
						var li = o.Anchors.ToArray();
						double maxy = double.MinValue;
						double miny = double.MaxValue;
						foreach(var anch in li) {
							nodes[anch.SlotIndex] = anch.GetPoint(chartControl, ChartPanel, chartScale);
							maxy = Math.Max(maxy, nodes[anch.SlotIndex].Y);
							miny = Math.Min(miny, nodes[anch.SlotIndex].Y);
						}
						if(nodes.Count<2) printDebug("nodes was only : "+nodes.Count);
						else{
							var keys = nodes.Keys.ToList();
							x = Convert.ToSingle(nodes[keys[1]].X);
							y = Convert.ToSingle(nodes[keys[1]].Y);
							float mult = 2;
							string ttg = tag.ToUpper();
							if(ttg.Contains("RR") || ttg.Contains(".WAV")){
								var elems = ttg.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
								foreach(var el in elems){
									if(el.Contains("RR"))
										if(!float.TryParse(this.KeepTheseChars(el.ToCharArray(), "-.0123456789"), out mult)) mult=2;
									if(el.Contains(".WAV")){
										var wav = tag.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(k=> k.Contains(".wav"));
										if(wav !=null){
											double pr = chartScale.GetValueByY(Convert.ToSingle(nodes[keys[1]].Y));
											AlertsMgr.AddLevel(tag, pr, wav, CurrentBars[0]);
											txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, wav, txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
											labelRect = new SharpDX.RectangleF(10f, Convert.ToSingle(nodes[keys[1]].Y)-txtLayout.Metrics.Height-4f, txtLayout.Metrics.Width+8f, Convert.ToSingle(font.Size)+8f);
											RenderTarget.FillRectangle(labelRect, DimGrayBrushDX);
											RenderTarget.DrawText(wav, txtFormat, labelRect, BlackBrushDX);
											if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
										}
									}
								}
							}

							diff = Convert.ToSingle(Math.Abs(maxy-miny));
							v.X = x;
							if(nodes.Last().Value.Y < nodes.First().Value.Y){
								v.Y = y;
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v,13f,3f), MaroonBrushDX);
								v.Y = y+diff*mult;
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v,13f,3f), BlueBrushDX);
							}else {
								v.Y = y;
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v,13f,3f), BlueBrushDX);
								v.Y = y-diff*mult;
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v,13f,3f), MaroonBrushDX);
							}
						}
						#endregion
}catch(Exception ee1){printDebug(line+":  "+ee1.ToString());}
					} else if (objects[idx].ToString().EndsWith(".Line")) {
line=4288;
try{
						#region -- Line signals --
						o = (IDrawingTool)objects[idx];//DrawObjects[dob.Tag];
						var li = o.Anchors.ToArray();
						var abar0 = (int)li[0].SlotIndex;
						var abar1 = (int)li[1].SlotIndex;
						p0 = Math.Abs(li[1].Price - li[0].Price);
						if(p0>10) p0 = Math.Round(p0,0);

						var str = string.Format("Value: {0}", 
							(pIsEquitySpread ? (p0*SelectedSpreadQty).ToString("C").Replace(".00",string.Empty) : (p0*SelectedSpreadQty).ToString()));
						txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);

						if(abar0 < abar1) {
							x = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(li[1].Time)) - txtLayout.Metrics.Width-15f;
							y = Convert.ToSingle(li[0].GetPoint(chartControl,ChartPanel,chartScale).Y) - (li[1].Price < li[0].Price ? txtLayout.Metrics.Height*1.5f: -txtLayout.Metrics.Height*0.5f);
						} else {
							x = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(li[0].Time)) - txtLayout.Metrics.Width-15f;
							y = Convert.ToSingle(li[0].GetPoint(chartControl,ChartPanel,chartScale).Y) - (li[1].Price < li[0].Price ? txtLayout.Metrics.Height*1.5f: -txtLayout.Metrics.Height*0.5f);
						}
						labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, Convert.ToSingle(font.Size)+7f);
						RenderTarget.FillRectangle(labelRect, BlackBrushDX);
						try{
							var L = o as NinjaTrader.NinjaScript.DrawingTools.Line;
							if(RenderTarget!=null && L != null && L.Stroke != null && L.Stroke.BrushDX != null){
								if(L.Stroke.BrushDX.IsValid(RenderTarget)){
									RenderTarget.DrawText(str, txtFormat, labelRect, L.Stroke.BrushDX);
								}else{
									RenderTarget.DrawText(str, txtFormat, labelRect, txtBrushDX);
								}
							}
						}catch(Exception kp){printDebug(line+":  OnRender: "+kp.ToString());}
						if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
}catch(Exception ee1){printDebug(line+":  "+ee1.ToString());}
					}else if (objects[idx].ToString().EndsWith(".HorizontalLine")) {
line=4323;
						Spread0 = Spread.GetValueAt(CurrentBars[0]);
						o = (IDrawingTool)DrawObjects[objects[idx].Tag];
						tag = o.Tag.ToLower();
						if(tag.Contains(".wav")){
							var wav = tag.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(k=> k.Contains(".wav"));
							if(wav !=null){
								var li = o.Anchors.ToArray();
								AlertsMgr.AddLevel(tag, li[0].Price, wav, CurrentBars[0]);
								y = (float)chartScale.GetYByValue(li[0].Price);
								txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, wav, txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
								labelRect = new SharpDX.RectangleF(10f, y-1f-txtLayout.Metrics.Height, txtLayout.Metrics.Width, Convert.ToSingle(font.Size));
								RenderTarget.FillRectangle(labelRect, DimGrayBrushDX);
								RenderTarget.DrawText(wav, txtFormat, labelRect, BlackBrushDX);
								if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
							}
						}else if(AutoState != BuyOneSellAnother_AutoState.Off && PermitHorizLineTrades){
line=4340;
							this.BottomRightTextStr = string.Empty;
						//if (objects[idx].ToString().EndsWith(".HorizontalLine")) 
							{
								var li = o.Anchors.ToArray();
								if(NoBuys == false && tag.Contains("buy")){
									IsBuyLineFound = true;
									var dist = Math.Abs(Spread0-li[0].Price);
									if(dist < dist_NearestBuyLevel){
										HLine_BuyValue = li[0].Price;
										dist_NearestBuyLevel = dist;
									}
printDebug("Buy line found");
									if(tag.Contains("stop")) BuyHLineEntryType = "Stop";
									else BuyHLineEntryType = "Touch";
									var str = string.Format("{0} Buy {1} [{2}]", BuyHLineEntryType, 
										DetermineSLtagOrTargettag(CurrentSpreadPosition, 'B', HLine_BuyValue, Spread0),//HLine_BuyValue.ToString("0"), 
										((CurrentSpreadPosition==0?1:CurrentSpreadPosition) * Math.Abs(Spread0-HLine_BuyValue)).ToString("0"));
									y = (float)chartScale.GetYByValue(HLine_BuyValue);
									txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
									labelRect = new SharpDX.RectangleF(ChartPanel.W-txtLayout.Metrics.Width*3-5f, y+3f, txtLayout.Metrics.Width, Convert.ToSingle(font.Size));
									RenderTarget.FillRectangle(labelRect, BlackBrushDX);
									RenderTarget.DrawText(str, txtFormat, labelRect, txtBrushDX);
									if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
								}else if(NoSells == false && o.Tag.ToLower().Contains("sell")){
line=4364;
									IsSellLineFound = true;
printDebug("Sell line found");
									var dist = Math.Abs(Spread0-li[0].Price);
									if(dist < dist_NearestSellLevel){
										HLine_SellValue = li[0].Price;
										dist_NearestSellLevel = dist;
									}
									if(tag.Contains("stop")) SellHLineEntryType = "Stop";
									else SellHLineEntryType = "Touch";
									var str = string.Format("{0} Sell {1} [{2}]", SellHLineEntryType,
										DetermineSLtagOrTargettag(CurrentSpreadPosition, 'S', HLine_SellValue, Spread0),//HLine_SellValue.ToString("0"), 
										((CurrentSpreadPosition==0 ? 1:CurrentSpreadPosition) * Math.Abs(Spread0-HLine_SellValue)).ToString("0"));
									y = (float)chartScale.GetYByValue(HLine_SellValue);
									txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
									labelRect = new SharpDX.RectangleF(ChartPanel.W-txtLayout.Metrics.Width*3-5f, y-1f-txtLayout.Metrics.Height, txtLayout.Metrics.Width, Convert.ToSingle(font.Size));
									RenderTarget.FillRectangle(labelRect, BlackBrushDX);
									RenderTarget.DrawText(str, txtFormat, labelRect, txtBrushDX);
									if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
								}
							}
						}
					}
					#endregion
//}catch(Exception kk){printDebug(line+": "+kk.ToString());}
				}
			}
			if(!IsBuyLineFound)  HLine_BuyValue = double.NaN;
			if(!IsSellLineFound) HLine_SellValue = double.NaN;

			#endregion

			#region -- Draw value level grid lines --
			v1.X = 0; v1.Y = 0;
			v2.X = 0; v2.Y = 0;
			if(RenderTarget!=null && pGridlineSpacing>0 && pGridlineOpacity>0){
				var ptr = KeyLevel.GetValueAt(CurrentBars[0]);// keyprice;
				int line_count = 0;
				while(ptr > chartScale.MinValue) {
					ptr = ptr - pGridlineSpacing;
					line_count--;
				}
				float lasty = float.MinValue;
				float diffY = 0f;
				float w = chartControl.ChartPanels[chartScale.PanelIndex].W;
				v2.X = w;
bool pShowAsGridlines = false;
				while(ptr<chartScale.MaxValue + pGridlineSpacing+1){
					y = (float)chartScale.GetYByValue(ptr);// + chartControl.ChartPanels[chartScale.PanelIndex].Y;
					if(pShowAsGridlines){
						v1.Y = y;
						v2.Y = y;
						RenderTarget.DrawLine(v1, v2, GridBrushDX, 1f);
					}else{//paint filled regions across the chart, if lines are not requested
						if(diffY == 0f && lasty != float.MinValue) diffY = Math.Abs(y-lasty);
						lasty = y;
						if(line_count %2 == 0){
							RenderTarget.FillRectangle(new SharpDX.RectangleF(0, y, w, diffY), GridBrushDX);
						}
						line_count++;
					}
					ptr = ptr + pGridlineSpacing;
				}
			}
			#endregion

			int sp0 = Convert.ToInt32((PlusAsk-PlusBid)/Instruments[0].MasterInstrument.TickSize);
			int sp1 = Convert.ToInt32((MinusAsk-MinusBid)/Instruments[1].MasterInstrument.TickSize);
			#region -- Show spread position, value, and current bid-ask spreads --
			RMx = ChartPanel.W/2;
			double LongExitPrice = CurrentSpreadPosition > 0 ? PlusBid :PlusAsk;
			double ShortExitPrice = CurrentSpreadPosition > 0 ? MinusAsk :MinusBid;
			double equity = (CurrentSpreadPosition > 0 ? Spread.GetValueAt(CurrentBars[0]) - CurrentPositionValue : (CurrentSpreadPosition < 0 ? CurrentPositionValue - Spread.GetValueAt(CurrentBars[0]) : double.MinValue)) * Math.Abs(CurrentSpreadPosition);
			var position_str = string.Format("{0} {1}{2}", 
				CurrentSpreadPosition == 0 ? "Flat": (CurrentSpreadPosition>0 ? string.Format("Long {0}  PnL",CurrentSpreadPosition) : string.Format("Short {0}  PnL",CurrentSpreadPosition)),
				equity >= 0 ? "+$": (equity < 0 ? "-$":string.Empty),
				(CurrentSpreadPosition == 0 ? "" : Math.Abs(equity).ToString("0"))
				);
			var spreads_str = string.Format("Bid-Ask spreads   {0}:  {1}-tks     {2}:  {3}-tks", 
				(Instruments[0].FullName), 
				sp0, 
				(Instruments[1].FullName), 
				sp1
				);
//			var spreads_str = string.Format("Bid-Ask spreads   {0}:  {1}-tks     {2}:  {3}-tks", 
//				(BuySymbol==SellSymbol ? Instruments[0].FullName : BuySymbol), 
//				sp0, 
//				(BuySymbol==SellSymbol ? Instruments[1].FullName : SellSymbol), 
//				sp1
//				);
line=2970;
			txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, position_str, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);
			float xLeft = RMx-txtLayout.Metrics.Width/2f;
			labelRect = new SharpDX.RectangleF(xLeft, ChartPanel.Y+ChartPanel.H-txtLayout.Metrics.Height*2.1f, txtLayout.Metrics.Width, Convert.ToSingle(txtFormat.FontSize));
			if(CurrentSpreadPosition!=0){
				RenderTarget.FillRectangle(new SharpDX.RectangleF(labelRect.X-4f, labelRect.Y-2f, labelRect.Width+8f, labelRect.Height+8f), equity > 0 ? LimeBrushDX : MagentaBrushDX);
				RenderTarget.DrawText(position_str, txtFormat, labelRect, BlackBrushDX);
			}else{
				RenderTarget.DrawText(position_str, txtFormat, labelRect, BlackBrushDX);
			}
			txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, spreads_str, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);
			labelRect = new SharpDX.RectangleF(xLeft, ChartPanel.Y+ChartPanel.H-txtLayout.Metrics.Height, txtLayout.Metrics.Width, Convert.ToSingle(font.Size));
			RenderTarget.DrawText(spreads_str, txtFormat, labelRect, txtBrushDX);
			#endregion
line=3784;
			#region -- printDebug StatusMsg --
			if(StatusMsg.Trim().Length>0){
				var smsg = string.Format("{0}|Grid: {1}|Correlation{2}: {3}|Loc: {4}", 
					StatusMsg, 
					this.pGridlineSpacing* Math.Abs(CurrentSpreadPosition == 0 ? SelectedSpreadQty : CurrentSpreadPosition), 
					(pCorrelationTimeMinutes>0 ? string.Format(" {0}-mins", this.pCorrelationTimeMinutes) : ""),
					double.IsNaN(Correlation) ? "":(Correlation>0 ? "+":"")+ (Correlation).ToString("0.00"),
					((Spread.GetValueAt(cb0)-OverallMinSpreadVal)/(OverallMaxSpreadVal-OverallMinSpreadVal)).ToString("0%")
					);
				var elements = smsg.Split(new char[]{'|'});
				float lineheightF = txtFormat.FontSize+4f;
				y = ChartPanel.Y+ChartPanel.H - lineheightF*(elements.Length+4);// - txtLayout.Metrics.Height*3;
				foreach(var s in elements){
					if(!string.IsNullOrWhiteSpace(s)){
						txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, s, txtFormat, (ChartPanel.X + ChartPanel.W), txtFormat.FontSize);
						labelRect = new SharpDX.RectangleF(10f, y, txtLayout.Metrics.Width+4f, lineheightF);
						RenderTarget.FillRectangle(labelRect, BlackBrushDX);
						RenderTarget.DrawText(s, txtFormat, labelRect, txtBrushDX);
					}
					y = y + lineheightF;
				}
			}
			#endregion
			#region -- Show current position entry price on right-side of chart --
			if(CurrentSpreadPosition != 0){
				string spread_val = string.Format("{0} {1}", (CurrentSpreadPosition >=1 ? "Long @":(CurrentSpreadPosition <=-1 ? "Short @":"")), CurrentPositionValue.ToString("0"));
				y = (float)chartScale.GetYByValue(CurrentPositionValue);
				v1.X = ChartPanel.W-20f;
				v1.Y = y;
				v2.X = ChartPanel.W;
				v2.Y = y;
				txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, spread_val, txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
				labelRect = new SharpDX.RectangleF(Convert.ToSingle(ChartPanel.W-20f - txtLayout.Metrics.Width-2f), y - txtLayout.Metrics.Height/2f, txtLayout.Metrics.Width, txtLayout.Metrics.Height);
				RenderTarget.DrawLine(v1, v2, txtBrushDX);
				RenderTarget.FillRectangle(labelRect, BlackBrushDX);
				RenderTarget.DrawText(spread_val, txtFormat, labelRect, txtBrushDX);
			}
			#endregion
			x = ChartPanel.W-15f;
			v1.X = x;
			v1.Y = 0f;
			#region -- Show approximated bid/ask level dots on right side of chart --
			if(BuyValueBrushDX!=null) {
				y = (float)chartScale.GetYByValue(EstimatedBuyPrice);
				v1.Y = y;
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 3f,3f), BuyValueBrushDX);
			}
			if(SellValueBrushDX!=null) {
				y =(float)chartScale.GetYByValue(EstimatedSellPrice);
				v1.X = x-2f; //-2f to the left of the BuyValue dot printed above
				v1.Y = y;
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 3f,3f), SellValueBrushDX);
			}
			#endregion

			#region -- Show Volatility Channels of future time periods --
			double future_upperpctle = 0;
			double future_lowerpctle = 0;
//			GetDistPctile(BarsArray[0].GetTime(RMaB).AddMinutes(30), ref future_upperpctle, ref future_lowerpctle);//gets the pctile level 30-minutes from now
//			vect.X = ChartPanel.W-25f;
//			vect.Y = (float)chartScale.GetYByValue(future_upperpctle + KeyLevel.GetValueAt(RMaB));
//			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, 2f,2f), Plots[1].BrushDX);
////			future_pctle = GetDistPctile(BarsArray[0].GetTime(RMaB).AddMinutes(30));
//			vect.Y = (float)chartScale.GetYByValue(future_lowerpctle + KeyLevel.GetValueAt(RMaB));
//			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, 2f,2f), Plots[2].BrushDX);
			if(BarsArray[0].BarsPeriod.BarsPeriodType==BarsPeriodType.Minute){
				v.X = chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]);
				DateTime ti = DateTime.MinValue;
				float width = (chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex) - chartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex)) / (ChartBars.ToIndex-ChartBars.FromIndex);
				ti = BarsArray[0].LastBarTime.AddMinutes(BarsArray[0].BarsPeriod.Value);
				Plots[1].BrushDX.Opacity = 0.3f;
				Plots[2].BrushDX.Opacity = 0.3f;
				while(v.X < ChartPanel.W){
					v.X = v.X + width;
					GetDistPctile(ti, ref future_upperpctle, ref future_lowerpctle);//gets the pctile level x-minutes from rightmost bar
					v.Y = (float)chartScale.GetYByValue(future_upperpctle + KeyLevel.GetValueAt(RMaB));
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 2f,2f), Plots[1].BrushDX);
					v.Y = (float)chartScale.GetYByValue(future_lowerpctle + KeyLevel.GetValueAt(RMaB));
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v, 2f,2f), Plots[2].BrushDX);
					ti = ti.AddMinutes(BarsArray[0].BarsPeriod.Value);
				}
				Plots[1].BrushDX.Opacity = 1f;
				Plots[2].BrushDX.Opacity = 1f;
			}
			#endregion

			if(txtLayout != null    && !txtLayout.IsDisposed)     {txtLayout.Dispose();      txtLayout = null;}
line=4672;
}catch(Exception e){printDebug(line+": "+e.ToString()+"   "+BuySymbol+"-"+SellSymbol);}
			RenderTarget.AntialiasMode = OSM;
		}
		
		private string DetermineSLtagOrTargettag(int CurrentSpreadPosition, char LineDirection, double HLinePrice, double SpreadPriceNow){
			if(CurrentSpreadPosition == 0){
				return "Entry";
			}
			string result = "Add";
			if(CurrentSpreadPosition > 0){
				if(LineDirection == 'S'){
					if(SpreadPriceNow < HLinePrice){
						result = "Tgt";
					}else{
						result = "SL";
					}
				}
			}else if(CurrentSpreadPosition < 0){
				if(LineDirection == 'B'){
					if(SpreadPriceNow > HLinePrice){
						result = "Tgt";
					}else{
						result = "SL";
					}
				}
			}
			return result;
		}
//===========================================                    ===============================================================
		private SortedDictionary<int,double> CalculateChannelSize (int ChannelResetTime){//, ref double avg){
			#region -- CalculateChannelSize --
			double avg = 0;
			double H   = double.MinValue;
			double L   = 0;
			double orH = double.MinValue;
			double orL = 0;
			double s = 0;
			int session_stop_time = 0;
			SortedDictionary<int,double> reset_prices   = new SortedDictionary<int,double>();
			DateTime t1 = DateTime.MinValue;
			DateTime t0 = DateTime.MinValue;
			int tt1, tt0;

			for(int abar = 2; abar< BarsArray[0].Count-1; abar++){
				t1 = Times[0].GetValueAt(abar-1);
				t0 = Times[0].GetValueAt(abar);
				tt1 = ToTime(t1)/100;
				tt0 = ToTime(t0)/100;
				s = Spread.GetValueAt(abar);
				if(H!=double.MinValue){
					int id = this.GetChannelRangesId(t0);
					double mid = (H+L)/2;
					AddToDiffs(id, (s-mid));//calculate ChannelRange on every bar
				}
				#region -- Calculate daily ranges --
				if(H==double.MinValue || 
						tt1 < ChannelResetTime && tt0 >= ChannelResetTime ||
						ChannelResetTime==tt0 ||
						tt1 > tt0 && ChannelResetTime < tt0 && t1.Day!=t0.Day
					){
					reset_prices[abar] = s;
					H = s;
					L = s;
				}else{
					H = Math.Max(H, s);
					L = Math.Min(L, s);
				}
				#endregion
			}
			
//			foreach(var t in ChannelRanges.Keys){
//				if(ChannelRanges[t].Count>0)
//					avg = ChannelRanges[t].Average();
//				ChannelRanges[t].Clear();
//				ChannelRanges[t].Add(avg);//The ChannelRanges dictionary is now condensed.  The list of all individual distances is cleared and only the single average distance is saved
//			}
//foreach(var v in opening_ranges)printDebug(v.Key.ToString()+"  OR: "+v.Value);
//printDebug("KeyLevel OR: "+opening_ranges.Values.Average());
//			if(ranges.Count>0){
//				avg = ranges.Average() * (1-pChannelReductionPct);
//			}
//			else avg = -1;
			return reset_prices;
			#endregion
		}
		private int ToTimeAdd(int time, int minutes_to_add){
			time = time + minutes_to_add*100/60;
			while(time >= 2400) time = time-1200;
			return time;
		}
//==============================            ============================================================================
        protected override void OnMarketData(MarketDataEventArgs e)
        {
			#region -- OnMarketData --
			if(IsRealTime1 && IsRealTime2){
				hasRealtimeData = true;
				connected = true;
			}
			try{
line=3191;
			if(BarsInProgress == 0){
line=3193;
				if (e.MarketDataType==MarketDataType.Last && e.Last!=PlusCurrent) {
					PlusCurrent = Round2Tick(e.Last, BarsInProgress);
					CurrentExitValue = PlusCurrent * (pIsEquitySpread ? PLUS_POINT_VALUE : 1) - MinusCurrent * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
				}else if (e.MarketDataType==MarketDataType.Ask){
line=3198;
					var p = Round2Tick(e.Ask, BarsInProgress);
					if(p != PlusAsk) {
						PlusAsk = p;
//printDebug(string.Format("{0}  OMD   PlusAsk",PlusAsk));
						EstimatedBuyPrice = PlusAsk * (pIsEquitySpread ? PLUS_POINT_VALUE : 1) - MinusBid * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
					}
				}else if (e.MarketDataType==MarketDataType.Bid){
line=3206;
					var p = Round2Tick(e.Bid, BarsInProgress);
					if(p != PlusBid) {
						PlusBid = p;
//printDebug(string.Format("{0}  OMD   PlusBid",PlusBid));
						EstimatedSellPrice = PlusBid * (pIsEquitySpread ? PLUS_POINT_VALUE : 1) - MinusAsk * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
					}
				}
			}else if(BarsInProgress == 1){
line=3215;
				if (e.MarketDataType==MarketDataType.Last && e.Last != MinusCurrent) {
					MinusCurrent = Round2Tick(e.Last, BarsInProgress);
					CurrentExitValue = PlusCurrent * (pIsEquitySpread ? PLUS_POINT_VALUE : 1) - MinusCurrent * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
				}else if (e.MarketDataType==MarketDataType.Ask){
line=3220;
					var p = Round2Tick(e.Ask, BarsInProgress);
					if(p != MinusAsk) {
						MinusAsk = p;
						EstimatedSellPrice = PlusBid * (pIsEquitySpread ? PLUS_POINT_VALUE : 1) - MinusAsk * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
					}
				}else if (e.MarketDataType==MarketDataType.Bid) {
line=3227;
					var p = Round2Tick(e.Bid, BarsInProgress);
					if(p != MinusBid){
line=3230;
						MinusBid = p;
						EstimatedBuyPrice = PlusAsk * (pIsEquitySpread ? PLUS_POINT_VALUE : 1) - MinusBid * (pIsEquitySpread ? MINUS_POINT_VALUE : 1);
					}
				}
			}
			}catch(Exception ed){printDebug(line+":  "+ed.ToString());}
			#endregion
		}
//		private double EliminateFalsePrices(List<double> history, double new_val, double currMktPrice){
//			//sometimes, there's a strange bid or ask tick that skews the Spread calculation.  This smoothing function prevents the Spread from deviating by more than 5%
//			if(history.Count==0) history.Add(currMktPrice);
//			double avg = history.Average();
//			if(new_val > avg*1.05) {printDebug("---------- false price: "+new_val+"   "+(new_val/history.Last())); return history.Last();}
//			if(new_val < avg*0.95) {printDebug("---------- false price: "+new_val+"   "+(new_val/history.Last())); return history.Last();}
//			history.Add(new_val);
//			if(history.Count>3) history.RemoveAt(0);
//			return new_val;
//		}
//==========================================================================================================
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
			public info (char dir, double entryprice, int entryABar, int signalABar, DateTime entryDT, double targetPrice, double slPrice, double contracts, double dayHigh, double dayLow){
				Direction=dir; EntryPrice=entryprice; EntryABar=entryABar; SignalABar=signalABar; EntryDT = entryDT; TargetPrice = targetPrice; SLPrice = slPrice; 
				BETriggerPrice = (TargetPrice+SLPrice)/2;
				Contracts = Math.Max(1,contracts);
				LocPercentileInDailyRange = (EntryPrice-dayLow)/(dayHigh-dayLow);
			}
		}
		private SortedDictionary<int,info> Trades = new SortedDictionary<int,info>();//abar is the key, trades is the value

//===================            =======================================================================================
		private void CalculatePnL(){
			#region -- CalculatPnL --
			char Pos = ' ';
			double EntryPrice = 0;
			double SLPrice = 0;
			double TgtPrice = 0;
			bool InSession = false;
			for(int x = 2; x<BarsArray[0].Count-1; x++){
				var s1   = Spread.GetValueAt(x-1);
				var s   = Spread.GetValueAt(x);
				var t1 = Times[0].GetValueAt(x-1);
				var t0 = Times[0].GetValueAt(x);
				int tt1 = ToTime(t1)/100;
				int tt  = ToTime(t0)/100;
				if(Pos == 'L'){
					bool c1 = tt1 < pExitTime && tt >= pExitTime;
					bool c2 = tt1 < pExitTime && tt < tt1 || t0.Day!=t1.Day;
					if(c1 || c2){
						var open_trades = Trades.Where(k=>k.Value.ExitABar<0).Select(k=>k.Key).ToList();
						if(open_trades!=null && open_trades.Count>0){
							foreach(var t in open_trades){
								Trades[t].ExitPrice = c2 ? Spread.GetValueAt(x-1) : s;
								Trades[t].ExitABar = c2 ? x-1 : x;
								Trades[t].Disposition = "EOD";
								Trades[t].PnL = s - Trades[t].EntryPrice;
							}
						}
						Pos = ' ';
					}else{
						SLPrice  = SLL.GetValueAt(x-1);
						var open_trades = Trades.Where(k=>k.Value.ExitABar<0).Select(k=>k.Key).ToList();
						if(open_trades!=null && open_trades.Count>0){
							foreach(var t in open_trades){
								TgtPrice = pProfitTargetDollars > 0 ? Trades[t].EntryPrice + pProfitTargetDollars : KeyLevel.GetValueAt(x-1);
								Trades[t].TargetPrice = TgtPrice;
								Trades[t].SLPrice = SLPrice;
								if(s > TgtPrice) {
									Trades[t].ExitPrice = s;
									Trades[t].ExitABar = x;
									Trades[t].Disposition = "Tgt";
									Trades[t].PnL = s - Trades[t].EntryPrice;
									Pos = ' ';
								}
								else if(s < SLPrice) {
									Trades[t].ExitPrice = s;
									Trades[t].ExitABar = x;
									Trades[t].Disposition = "SL";
									Trades[t].PnL = s - Trades[t].EntryPrice;
									Pos = ' ';
								}
							}
						}
					}
				}
				if(Pos == 'S'){
					bool c1 = tt1 < pExitTime && tt >= pExitTime;
					bool c2 = tt1 < pExitTime && tt < tt1 || t0.Day!=t1.Day;
					if(c1 || c2){
						var open_trades = Trades.Where(k=>k.Value.ExitABar<0).Select(k=>k.Key).ToList();
						if(open_trades!=null && open_trades.Count>0){
							foreach(var t in open_trades){
								Trades[t].ExitPrice = c2 ? Spread.GetValueAt(x-1) : s;
								Trades[t].ExitABar = c2 ? x-1 : x;
								Trades[t].Disposition = "EOD";
								Trades[t].PnL = Trades[t].EntryPrice - s;
							}
						}
						Pos = ' ';
					}else{
						SLPrice  = SLU.GetValueAt(x-1);
						var open_trades = Trades.Where(k=>k.Value.ExitABar<0).Select(k=>k.Key).ToList();
						if(open_trades!=null && open_trades.Count>0){
							foreach(var t in open_trades){
								TgtPrice = pProfitTargetDollars > 0 ? Trades[t].EntryPrice + pProfitTargetDollars : KeyLevel.GetValueAt(x-1);
								Trades[t].TargetPrice = TgtPrice;
								Trades[t].SLPrice = SLPrice;
								if(s < TgtPrice) {
									Trades[t].ExitPrice = s;
									Trades[t].ExitABar = x;
									Trades[t].Disposition = "Tgt";
									Trades[t].PnL = Trades[t].EntryPrice - s;
									Pos = ' ';
								}
								else if(s > SLPrice) {
									Trades[t].ExitPrice = s;
									Trades[t].ExitABar = x;
									Trades[t].Disposition = "SL";
									Pos = ' ';
									Trades[t].PnL = Trades[t].EntryPrice - s;
								}
							}
						}
					}
				}

				if(Pos == ' ' && InSession){
					if(s1 < DL.GetValueAt(x-1) && s > DL.GetValueAt(x)){
						Pos = 'L';
						EntryPrice = Spread.GetValueAt(x);
						TgtPrice = KeyLevel.GetValueAt(x);
						SLPrice  = SLL.GetValueAt(x);
						var time = Times[0].GetValueAt(x);
						Trades[x] = (new info(Pos, EntryPrice, x, x, time, TgtPrice, SLPrice, 1, 0,0));
					}
					else if(s1 > DU.GetValueAt(x-1) && s < DU.GetValueAt(x)){
						Pos = 'S';
						EntryPrice = s;
						TgtPrice = KeyLevel.GetValueAt(x);
						SLPrice  = SLU.GetValueAt(x);
						var time = Times[0].GetValueAt(x);
						Trades[x] = (new info(Pos, EntryPrice, x, x, time, TgtPrice, SLPrice, 1, 0,0));
					}
				}
//				if(!InSession){
//					PlotBrushes[0][cb0-x] = Brushes.DimGray;
//					PlotBrushes[1][cb0-x] = Brushes.DimGray;
//					PlotBrushes[2][cb0-x] = Brushes.DimGray;
//					PlotBrushes[3][cb0-x] = Brushes.DimGray;
//					PlotBrushes[4][cb0-x] = Brushes.DimGray;
//				}
			}
			printDebug("Trades: "+Trades.Count);
			double TotPnL=0;
			double wins = 0;
			double losses = 0;
			double winDlrs = 0;
			double lossDlrs = 0;
			char wl = ' ';
			foreach(var t in Trades){
				if(t.Value.ExitABar>0){
					TotPnL += t.Value.PnL;
					if(t.Value.PnL>0) {
						wl = 'W';
						wins++;
						winDlrs += t.Value.PnL;
					}else{
						wl = 'L';
						losses++;
						lossDlrs += t.Value.PnL;
					}
					printDebug(wl+" "+t.Value.EntryDT.ToString()+"  "+t.Value.Direction+" "+t.Value.PnL.ToString("C")+"  "+Times[0].GetValueAt(t.Value.ExitABar).ToString()+" "+t.Value.Disposition);
				}
			}
			printDebug("Wins: "+wins+"  Losses: "+losses+"   W%: "+(wins/(wins+losses)).ToString("0%"));
			printDebug("Win $: "+winDlrs.ToString("C")+"  Loss $: "+lossDlrs.ToString("C")+"  PF: "+Math.Abs(winDlrs/lossDlrs).ToString("C"));
			printDebug("$/trade: "+(TotPnL / (wins+losses)).ToString("C"));
			#endregion
		}
//==========================================================================================================
//		public override string ToString(){
//			try{
//				if(Instruments==null || Instruments[0]==null || Instruments[1]==null) return "BuyOne SellAnother";
//				return "Long "+Instruments[0].FullName+" Short "+Instruments[1].FullName;
//			}catch(Exception e){printDebug(e.ToString());}
//			return "BuyOne SellAnother";
//		}

		#region Properties
//==========================================================================================================
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

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("<inst1><inst2>_BOSA_BuyEntry.wav");
				list.Add("<inst1><inst2>_BOSA_SellEntry.wav");
				list.Add("<inst>_ExitNow.wav");
				list.Add("<inst>_ExtremeHit.wav");
				list.Add("<inst>_MidlineHit.wav");
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
//====================================================================
		private string AddSoundFolder(string wav){
			wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst2>",Instruments[1].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(wav,' '));
			if(IsDebug) Print("Playing sound: "+wav);
			return wav;
		}
//====================================================================
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
	            }
				if(list.Count==0) list.Add("<no connection>");
	            return new TypeConverter.StandardValuesCollection(list);
	        }

	        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	        { return true; }
			#endregion
	    }
		#region -- MovingAverages --
		[Range(0, int.MaxValue)]
		[Display(Name="MA1 period", Order=10, GroupName="MovingAverages")]
		public int pMA1period
		{ get; set; }
		[Display(Name="MA1 type", Order=20, GroupName="MovingAverages")]
		public BuyOneSellAnother_MAtype pMA1type
		{ get; set; }
		[XmlIgnore]
		[Display(Name="Color of MA1 line", Order=21, GroupName="MovingAverages", Description="")]
		public Brush pMA1LineColor
		{ get; set; }
			[Browsable(false)]
			public string pMA1LineColorBrush_Serialize {	get { return Serialize.BrushToString(pMA1LineColor); } set { pMA1LineColor = Serialize.StringToBrush(value); }}
		[Range(1, float.MaxValue)]
		[Display(Name="MA1 line width", Order=22, GroupName="MovingAverages")]
		public float pMA1LineWidth
		{get;set;}

		[XmlIgnore]
		[Display(Name="Color of rising dot", Order=23, GroupName="MovingAverages", Description="When MA1 is rising, the dot will be this color")]
		public Brush pMA1UpDots
		{ get; set; }
			[Browsable(false)]
			public string pMA1UpDotsBrush_Serialize {	get { return Serialize.BrushToString(pMA1UpDots); } set { pMA1UpDots = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Name="Color of falling dot", Order=24, GroupName="MovingAverages", Description="When MA1 is falling, the dot will be this color")]
		public Brush pMA1DnDots
		{ get; set; }
			[Browsable(false)]
			public string pMA1DnDotsBrush_Serialize {	get { return Serialize.BrushToString(pMA1DnDots); } set { pMA1DnDots = Serialize.StringToBrush(value); }}

		[Range(0, int.MaxValue)]
		[Display(Name="MA2 period", Order=30, GroupName="MovingAverages")]
		public int pMA2period
		{ get; set; }
		[Display(Name="MA2 type", Order=31, GroupName="MovingAverages")]
		public BuyOneSellAnother_MAtype pMA2type
		{ get; set; }
		[XmlIgnore]
		[Display(Name="Color of MA2 line", Order=32, GroupName="MovingAverages", Description="")]
		public Brush pMA2LineColor
		{ get; set; }
			[Browsable(false)]
			public string pMA2LineColorBrush_Serialize {	get { return Serialize.BrushToString(pMA2LineColor); } set { pMA2LineColor = Serialize.StringToBrush(value); }}
		[Range(1, float.MaxValue)]
		[Display(Name="MA2 line width", Order=33, GroupName="MovingAverages")]
		public float pMA2LineWidth
		{get;set;}

		#endregion
		#region -- Trend Signal --
		[Range(0, int.MaxValue)]
		[Display(Name="Fast MA signal delay", Order=10, GroupName="Trend Signal", Description="Num of bars to confirm a trend signal, set to '0' to turn-off trend signals")]
		public int pFastMASignalDelayInBars
		{ get; set; }
		[XmlIgnore]
		[Display(Name="Color of up signal", Order=20, GroupName="Trend Signal", Description="When Trend Signal is up, the signal dot will be this color")]
		public Brush pTrendSignalUpDotsBrush
		{ get; set; }
			[Browsable(false)]
			public string pTrendSignalUpDotsBrush_Serialize {	get { return Serialize.BrushToString(pTrendSignalUpDotsBrush); } set { pTrendSignalUpDotsBrush = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Name="Color of down signal", Order=30, GroupName="Trend Signal", Description="When Trend Signal is down, the signal dot will be this color")]
		public Brush pTrendSignalDnDotsBrush
		{ get; set; }
			[Browsable(false)]
			public string pTrendSignalDnDotsBrush_Serialize {	get { return Serialize.BrushToString(pTrendSignalDnDotsBrush); } set { pTrendSignalDnDotsBrush = Serialize.StringToBrush(value); }}
		[Range(0, int.MaxValue)]
		[Display(Name="Size of TrendSignal dots", Order=40, GroupName="Trend Signal", Description="Trend signal dot size")]
		public float pTrendSignalDotSize
		{ get; set; }
		#endregion

//		[Display(Name="Outfile name", Order=1, GroupName="Parameters")]
//		public string pOutFileName {get;set;}

		[Range(1, int.MaxValue)]
		[Display(Name="Long Contracts", Order=10, GroupName="Parameters")]
		public int pLongContracts
		{ get; set; }

		[Display(Name="Short Symbol", Order=20, GroupName="Parameters")]
		public string pShortSymbol
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="Short Contracts", Order=30, GroupName="Parameters")]
		public int pShortContracts
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="Max Spreads Qty", Order=33, GroupName="Parameters", Description="To limit your quantity and control risk")]
		public int pMaxSpreadsQty
		{ get; set; }

//		[Display(Name="Use ChartTrader Account?", Order=34, GroupName="Parameters")]
//		public bool pUseChartTraderAccount
//		{ get; set; }

		[TypeConverter(typeof(LoadAccountNameList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Account Name", GroupName = "Parameters", Order = 35)]
		public string pAccountName {get;set;}

		[Display(Name="Equity Spread?", Order=40, GroupName="Parameters")]
		public bool pIsEquitySpread
		{ get; set; }

		[Range(-1, int.MaxValue)]
		[Display(Name="Channel period", Order=50, GroupName="Parameters", Description="Set to '0' to use session mid-price, instead of a SMA calculation")]
		public int pSMAperiod
		{ get; set; }

		[Range(0, 2359)]
		[Display(Name="Channel Reset time", Order=55, GroupName="Parameters", Description="Only used if 'Channel period' is set to '0'")]
		public int pChannelResetTime
		{ get; set; }

		[Range(0, 2359)]
		[Display(Name="Start time", Order=60, GroupName="Parameters")]
		public int pStartTime
		{ get; set; }

		[Range(0, 2359)]
		[Display(Name="Stop time", Order=61, GroupName="Parameters")]
		public int pStopTime
		{ get; set; }

		[Range(0, 2359)]
		[Display(Name="Exit time", Order=62, GroupName="Parameters")]
		public int pExitTime
		{ get; set; }

		private double pProfitTargetDollars = 0;
//		[NinjaScriptProperty]
//		[Display(Name="ProfitTarget $", Order=70, GroupName="Parameters", Description="0 turns off profit target and the midline will be the profit target")]
//		public double pProfitTargetDollars
//		{ get; set; }
		#region -- AutoTrade --
		[RefreshProperties(RefreshProperties.All)]
		[Display(Name="AssistedTrade OnBarClose", Order=10, GroupName="AutoTrade", Description="If enabled, assisted-trades will execute only on the close of the bar, otherwise assisted-trades will execute on each tick")]
		public bool pAutoTradeOnBarClose
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Color of BUY AT line", Order=20, GroupName="AutoTrade", Description="")]
		public Brush pATBuyLineColor
		{ get; set; }
			[Browsable(false)]
			public string pATBuyLineColor_Serialize {	get { return Serialize.BrushToString(pATBuyLineColor); } set { pATBuyLineColor = Serialize.StringToBrush(value); }}

		[Range(0,10)]
		[Display(Name="BUY line thickness", Order=30, GroupName="AutoTrade", Description="")]
		public int pATBuyLineThickness
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Color of SELL AT line", Order=40, GroupName="AutoTrade", Description="")]
		public Brush pATSellLineColor
		{ get; set; }
			[Browsable(false)]
			public string pATSellLineColor_Serialize {	get { return Serialize.BrushToString(pATSellLineColor); } set { pATSellLineColor = Serialize.StringToBrush(value); }}

		[Range(0,10)]
		[Display(Name="SELL line thickness", Order=50, GroupName="AutoTrade", Description="")]
		public int pATSellLineThickness
		{ get; set; }
		#endregion --------------------------------------------------------------------------------------------

		[Range(0,1)]
		[Display(Name="Channel reduction pct", Order=75, GroupName="Parameters", Description="To reduce the channel size, what percentile of the ranges will be excluded?")]
		public double pChannelReductionPct
		{ get; set; }

		[Display(Name="Outer bands multiplier", Order=80, GroupName="Parameters", Description="Multiplier for location of outer bands")]
		public double pOuterBandsMult
		{ get; set; }

		[Display(Name="Correlation Time (minutes)", Order=90, GroupName="Parameters", Description="0=uses chart timeframe, otherwise enter the number of minutes for bar data for correlation calculation")]
		public int pCorrelationTimeMinutes
		{ get; set; }

		#region -- Bkg Visuals ----------------------------------
		[Display(Name="Distance between grids", Order=10, GroupName="Bkg Visuals", Description="Point value between horiz gridlines (in equity units)")]
		public double pGridlineSpacing
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Color of grids", Order=20, GroupName="Bkg Visuals", Description="")]
		public Brush pGridlineBrush
		{ get; set; }
			[Browsable(false)]
			public string pGridlineBrush_Serialize {	get { return Serialize.BrushToString(pGridlineBrush); } set { pGridlineBrush = Serialize.StringToBrush(value); }}

		[Display(Name="Opacity of grids", Order=30, GroupName="Bkg Visuals", Description="")]
		public float pGridlineOpacity
		{ get; set; }
		
		[Display(Name="Show Reversal dots", Order=40, GroupName="Bkg Visuals", Description="Algorithm for picking trend weakness and reversals")]
		public bool pShowReversalDots
		{ get; set; }
		
		[Display(Name="Show SpreadHistory", Order=45, GroupName="Bkg Visuals", Description="Gold dots indicate average location (historical) of spread in relation to midline")]
		public bool pShowSpreadHistory
		{ get; set;}
		
		[Display(Name="Show Session TPO histo", Order=47, GroupName="Bkg Visuals", Description="If FixedValueMidline enabled, this is the TPO histo showing distribution of spread from that midline")]
		public bool pShowSessionTPO
		{get;set;}

		[Range(1,200)]
		[Display(Name="Reversal dots Period", Order=50, GroupName="Bkg Visuals", Description="")]
		public int pReversalDotsPeriod
		{ get; set; }
		#endregion =========================================================================================

		#region -- Audible Alerts --
		private string pEntryLongSound = "<inst1><inst2>_BOSA_BuyEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=10, Name="Long Cycle dot", GroupName="Audible Alerts", Description="Sound file when buy cycle dots first appear")]
		public string EntryLongSound
		{
			get { return pEntryLongSound; }
			set { pEntryLongSound = value; }
		}
		private string pEntryShortSound = "<inst1><inst2>_BOSA_SellEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=20, Name="Short Cycle dot", GroupName="Audible Alerts", Description="Sound file when sell cycle dots first appear")]
		public string EntryShortSound
		{
			get { return pEntryShortSound; }
			set { pEntryShortSound = value; }
		}
		
		private string pSLExitSound = "<inst>_ExtremeHit.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=30, Name="Extreme Level signal", GroupName="Audible Alerts", Description="Sound file when spread hits outermost band")]
		public string SLExitSound
		{
			get { return pSLExitSound; }
			set { pSLExitSound = value; }
		}

		private string pMidlineCrossSound = "<inst>_MidlineHit.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=40, Name="Midline signal", GroupName="Audible Alerts", Description="Sound file when spread line crosses the Equilibrium midline")]
		public string MidlineCrossSound
		{
			get { return pMidlineCrossSound; }
			set { pMidlineCrossSound = value; }
		}
		#endregion

		private string pLicensePassword = "<enter password here>";
		[XmlIgnore]
		[Display(Order=10, Name="License Password", GroupName="~ License ~", Description = "Contact us at bosa@sbgtradingcorp.com")]
		public string LicensePassword
		{
			get { return pLicensePassword; }
			set { pLicensePassword = value; }
		}

		[Display(Order=20, Name="Testing", GroupName="~ License ~", Description = "")]
		public bool IsProVersion
		{get;set;}
		#endregion
		
		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> KeyLevel
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DU
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DL
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SLU
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SLL
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Spread
		{
			get { return Values[5]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MA1
		{
			get { return Values[6]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MA2
		{
			get { return Values[7]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HiLoBarTop
		{
			get { return Values[8]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HiLoBarBot
		{
			get { return Values[9]; }
		}
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> MA1dots
//		{
//			get { return Values[10]; }
//		}
		#endregion

        #region Custom Property Manipulation
        private void ModifyProperties(PropertyDescriptorCollection col)
        {
            if (!IsProVersion) {
                col.Remove(col.Find("pAutoTradeOnBarClose", true));
                col.Remove(col.Find("pATBuyLineColor", true));
                col.Remove(col.Find("pATSellLineColor", true));
                col.Remove(col.Find("pATBuyLineThickness", true));
                col.Remove(col.Find("pATSellLineThickness", true));
            }
        }
        #region ICustomTypeDescriptor Members
        public PropertyDescriptorCollection GetProperties() { return TypeDescriptor.GetProperties(GetType()); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return this; }
        public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(GetType()); }
        public string GetClassName() { return TypeDescriptor.GetClassName(GetType()); }
        public string GetComponentName() { return TypeDescriptor.GetComponentName(GetType()); }
        public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(GetType()); }
        public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(GetType()); }
        public PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(GetType()); }
        public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(GetType(), editorBaseType); }
        public EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(GetType(), attributes); }
        public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(GetType()); }
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection orig = GetFilteredIndicatorProperties(TypeDescriptor.GetProperties(GetType(), attributes), this.IsOwnedByChart, this.IsCreatedByStrategy);
            PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
            orig.CopyTo(arr, 0);
            PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);
            ModifyProperties(col);
            return col;
        }
        public static PropertyDescriptorCollection GetFilteredIndicatorProperties(PropertyDescriptorCollection origProperties, bool isOwnedByChart, bool isCreatedByStrategy)
        {
            List<PropertyDescriptor> allProps = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor pd in origProperties) { allProps.Add(pd); }
            Type[] excludedTypes = new Type[] { typeof(System.Windows.Media.Brush), typeof(NinjaTrader.Gui.Stroke), typeof(System.Windows.Media.Color), typeof(System.Windows.Media.Pen) };
            Func<Type, bool> IsNotAVisualType = (Type propType) => {
                foreach (Type testType in excludedTypes) { if (testType.IsAssignableFrom(propType)) return false; }
                return true;
            };
            IEnumerable<string> baseIndProperties = from bp in typeof(IndicatorBase).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public) select bp.Name;
            IEnumerable<PropertyDescriptor> filteredProps = from p in allProps where p.IsBrowsable && (!isOwnedByChart && !isCreatedByStrategy ? (!baseIndProperties.Contains(p.Name) && p.Name != "Calculate" && p.Name != "Displacement" && IsNotAVisualType(p.PropertyType)) : true) select p;
            return new PropertyDescriptorCollection(filteredProps.ToArray());
        }
        #endregion
        #endregion

	}
}
public enum BuyOneSellAnother_MAtype {EMA,SMA}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BuyOneSellAnother[] cacheBuyOneSellAnother;
		public BuyOneSellAnother BuyOneSellAnother()
		{
			return BuyOneSellAnother(Input);
		}

		public BuyOneSellAnother BuyOneSellAnother(ISeries<double> input)
		{
			if (cacheBuyOneSellAnother != null)
				for (int idx = 0; idx < cacheBuyOneSellAnother.Length; idx++)
					if (cacheBuyOneSellAnother[idx] != null &&  cacheBuyOneSellAnother[idx].EqualsInput(input))
						return cacheBuyOneSellAnother[idx];
			return CacheIndicator<BuyOneSellAnother>(new BuyOneSellAnother(), input, ref cacheBuyOneSellAnother);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BuyOneSellAnother BuyOneSellAnother()
		{
			return indicator.BuyOneSellAnother(Input);
		}

		public Indicators.BuyOneSellAnother BuyOneSellAnother(ISeries<double> input )
		{
			return indicator.BuyOneSellAnother(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BuyOneSellAnother BuyOneSellAnother()
		{
			return indicator.BuyOneSellAnother(Input);
		}

		public Indicators.BuyOneSellAnother BuyOneSellAnother(ISeries<double> input )
		{
			return indicator.BuyOneSellAnother(input);
		}
	}
}

#endregion
