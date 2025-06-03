//#define SPEECH_ENABLED
// 
// Copyright (C) 2011, SBG Trading Corp.   www.affordableindicators.com
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//


#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion
#if SPEECH_ENABLED
using SpeechLib;
using System.Threading;
#endif
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
using SBG_RoadRunner;

namespace NinjaTrader.NinjaScript.Indicators
{
#if SPEECH_ENABLED
	public delegate void SpeechDelegate_RR_Lines(string str);
#endif
	public enum RoadRunner_Method {Original, Simov}
	public enum RoadRunner_PermittedDirections {Both, Long, Short, Trend}
	public class RR_Lines : Indicator
	{
//====================================================================================================
		#region SayItPlayIt methods
		private string RemoveNumbersFromString(string instr, int CharsToSkip) {
			if(instr.Length <= CharsToSkip) return instr;
			string outstr = string.Empty;
			if(CharsToSkip>0) outstr = instr.Substring(0,CharsToSkip);

			for(int i = 0+CharsToSkip; i<instr.Length; i++) {
				if((int)instr[i] >= (int)'0' && (int)instr[i] <= (int)'9') continue; else outstr = string.Concat(outstr,instr[i]);
			}
			return outstr;
		}
		private void PlayIt(string EncodedTag) {
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
				PlaySoundCustom(elements[elements.Length-1]);
				TimeOfText = NinjaTrader.Core.Globals.Now;
				AlertMsg = $"WAV file {elements[elements.Length-1]} played";
			}
		}
#if SPEECH_ENABLED
		private static void SayItThread (string WhatToSay) {
			if(WhatToSay.Length>0) {
				SpVoice voice = new SpVoice();
				// Tells the program to speak everything in the textbox using Default settings
				voice.Speak(WhatToSay, SpeechVoiceSpeakFlags.SVSFDefault);
			}
		}
		private void SayIt (string EncodedTag, double Price) {
			if(!SpeechEnabled) {
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = "Speech not available\nPut Interop.SpeechLib.DLL into 'bin\\Custom' folder and restart the platform";
				Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center,ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.LabelFont, Color.Black, ChartControl.Background, 10);
				return;
			}
			if(Historical) return;
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
				if(elements[0][0]=='/') return;
				SpeechDelegate_RR_Lines speechThread = new SpeechDelegate_RR_Lines(SayItThread);
				string SayThis = elements[elements.Length-1].ToUpper();
				if(SayThis.Contains("[SAYPRICE]")) {
					string pricestr1 = Instrument.MasterInstrument.FormatPrice(Price);
					string spokenprice = string.Empty + pricestr1[0];
					int i = 0;
					while(i<pricestr1.Length) {
						spokenprice = MakeString(new Object[]{spokenprice," ",pricestr1[i++]});
					}
					SayThis = SayThis.Replace("[SAYPRICE]", spokenprice);
				}
				if(SayThis.Contains("[TIMEFRAME]")) {
					SayThis = SayThis.Replace("[TIMEFRAME]", TimeFrameText);
				}
				if(SayThis.Contains("[INSTRUMENTNAME]")) {
					string name = Instrument.FullName.Replace('-',' ');
					name = RemoveNumbersFromString(name,1);
					string spokenphrase = string.Empty;// + name[0];
					int i = 0;
					while(i<name.Length) {
						spokenphrase = MakeString(new Object[]{spokenphrase," ",name[i++]});
					}
					SayThis = SayThis.Replace("[INSTRUMENTNAME]", spokenphrase);
				}
				speechThread.BeginInvoke(SayThis, null, null);
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = MakeString(new Object[]{"'",SayThis,"' was spoken"});
				Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center,ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.LabelFont, Color.Black, ChartControl.Background, 10);
			}
		}
#else
		private void SayIt (string EncodedTag) {}
		private void SayIt (string EncodedTag,double p) {}
#endif
		private void PlaySoundCustom(string wav){
			if(State == State.Historical) return;
			string pwav = AddSoundFolder(wav);
			if(System.IO.File.Exists(pwav))
				PlaySound(pwav);
			else if (wav != "none")
				Log(wav+" sound not found in your NT8 Sounds folder",LogLevel.Information);
		}
		#endregion
//====================================================================================================
#if SPEECH_ENABLED
		static Assembly  SpeechLib = null;
#endif
		private const int LONG = 1;
		private const int FLAT = 0;
		private const int SHORT = -1;
		private const int SIGNAL_MA1_OVER_MA2  = 1;
		private const int SIGNAL_MA1_UNDER_MA2 = -1;
		private const int BarToMA1_CROSSES_MA1_UPWARD   = 1;
		private const int BarToMA1_CROSSES_MA1_DOWNWARD = -1;
		private const int BarToMA1_BAR_ABOVE_MA1 = 2;
		private const int BarToMA1_BAR_BELOW_MA1 = -2;
		private const int BarToMA2_CROSSES_MA2_UPWARD   = 1;
		private const int BarToMA2_CROSSES_MA2_DOWNWARD = -1;
		private const int BarToMA2_BAR_ABOVE_MA2 = 2;
		private const int BarToMA2_BAR_BELOW_MA2 = -2;
		private string NL = Environment.NewLine;
		private string Subj,Body;

		private string   AlertMsg = "";
		private DateTime TimeOfText = DateTime.MinValue;
		private bool     SpeechEnabled = false;
		private string TimeFrameText = string.Empty;
		private int Direction = FLAT;
		private int EmailsThisBar = 0;
		private double zone;
		private DateTime DirectionFillRegionName=DateTime.MinValue;
		private bool StartFillRegion = false;
		private string tag = null;

		#region Variables
		// Wizard generated variables
			private int pPeriod1 = 13; // Default setting for Period1
			private int pPeriod2 = 34; // Default setting for Period2
			private RR_Lines_DataType pMA1DataType = RR_Lines_DataType.Close;
			private RR_Lines_DataType pMA2DataType = RR_Lines_DataType.Close;
			private Indicator TheMA1, TheMA2;
			private int PopupBar=0, AlertsThisBar=0;
			private bool RunInit=true;
		#endregion
		private EMA bigtrend;
		private bool isBen = false;
		private double PV;
		private Simov simov;
		string SwingLineTrendStr = "";
		private SBG_RoadRunner.TradeManager tm;
		private Account myAccount;
		private DateTime MessageTime = DateTime.MinValue;
		string Order_Reject_Message = string.Empty;
		string Log_Order_Reject_Message = string.Empty;
		string ATMStrategyNamesEmployed = "";
		private NinjaTrader.Gui.Tools.SimpleFont LabelFont = null;
		private const string VERSION = "v2 Feb.16.2025";
		[Display(Name = "Indicator Version", GroupName = "Indicator Version", Description = "Indicator Version", Order = 0)]
		public string indicatorVersion { get { return VERSION; } }

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && (
					NinjaTrader.Cbi.License.MachineId=="B0D2E9D1C802E279D3678D7DE6A33CE4" || NinjaTrader.Cbi.License.MachineId=="766C8CD2AD83CA787BCA6A2A76B2303B");
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "AIRoadRunner", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
	//				VendorLicense("IndicatorWarehouse", "AIMACrosses", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(Brushes.Blue, "RR Micro");
				AddPlot(Brushes.Blue, "RR Macro");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "Signal");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "SignalTrend");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Yellow,1), PlotStyle.Line,      "BigTrend");
	//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "SignalAge");
	//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "BarToMA1");
	//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "BarToMA2");
				Calculate=Calculate.OnPriceChange;
				IsOverlay=true;
				pDrawPnLLines = false;
				
				pLongArrowTemplateName = "Default";
				pShortArrowTemplateName = "Default";
				Name= "iw RR Lines";
				
				pATMStrategyName1 = "";
				SelectedAccountName = "";
				pStartTime		= 930;
				pStopTime		= 1550;
				pFlatTime		= 1650;
				pPermittedDOW	= "1,2,3,4,5";
				pShowPnLStats = false;
				pMethod = RoadRunner_Method.Original;
//				pTargetDistStr1 = "atr 2.5";
//				pStoplossDistStr1 = "UseATM";
				pDailyTgtDollars = 1000;
				pPermittedDirection = RoadRunner_PermittedDirections.Both;
			}
			else if (State == State.Configure)
			{
				Plots[0].DashStyleHelper = pLineDashStyleFast;
				Plots[1].DashStyleHelper = pLineDashStyleSlow;

				Plots[0].Width = pLineWidthFast;
				Plots[1].Width = pLineWidthSlow;

				Plots[0].PlotStyle = pLineStyleFast;
				Plots[1].PlotStyle = pLineStyleSlow;

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
				tm = new SBG_RoadRunner.TradeManager(this, "RoadRunner", "", "","", Instrument, s, pStartTime, pStopTime, pFlatTime, pShowHrOfDayPnL, int.MaxValue, int.MaxValue);
				tm.WarnIfWrongDayOfWeek = false;//do not print the warning message if the current day isn't in the Permitted DOW parameter
				#region -- SLTP for Signal --
				if(pTargetDistStr1.Trim() != "" && pStoplossDistStr1.Trim() != ""){
					tm.SLTPs["1"] = new TradeManager.SLTPinfo(pStoplossDistStr1, pTargetDistStr1, pATMStrategyName1);
				}
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
				LabelFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", this.pLabelFontSize);
				txtFormat_LabelFont = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(),
														 	LabelFont.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Bold,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)LabelFont.Size);
			}
			else if (State == State.DataLoaded)
			{
				zone = pZoneSize*TickSize;
				SelectedAccountName = SelectedAccountName.Trim();
				#region -- Verify account availability --
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
					string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "RoadRunnerTradeAssist.log");
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
					if(ts.TotalDays>5) System.IO.File.Delete("RoadRunnerTradeAssist.Log");
					else if(ts.TotalMinutes > 10){
						string msg = "ERROR = account '"+SelectedAccountName+"' is not available to trade";
						Log(msg, LogLevel.Alert);
						System.IO.File.AppendAllText("RoadRunnerTradeAssist.Log", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
					}
				}
				ATMStrategyNamesEmployed = $"#1:  '{pATMStrategyName1}'  Qty: {pSignal1Qty}";
				#endregion
				Log_Order_Reject_Message = "Your order was rejected.  Account Name '"+SelectedAccountName+"' may not be available on this datafeed connection.  Check for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				Order_Reject_Message = "Your order was rejected.\nAccount Name '"+SelectedAccountName+"' may not be available on this datafeed connection\nCheck for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				if(pBigTrendPeriod>0)
					bigtrend = EMA(pBigTrendPeriod);
				
				if(pMethod == RoadRunner_Method.Simov){
					simov = Simov();
				}

                #region -- Add Custom Toolbar --
                if (!isToolBarButtonAdded && ChartControl != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ChartControl.AllowDrop = false;
                        chartWindow = System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
                        if (chartWindow == null) return;

                        foreach (System.Windows.DependencyObject item in chartWindow.MainMenu) if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == (toolbarname + uID)) isToolBarButtonAdded = true;

                        if (!isToolBarButtonAdded)
                        {
                            indytoolbar = new System.Windows.Controls.Grid { Visibility = System.Windows.Visibility.Collapsed };

                            addToolBar();

                            chartWindow.MainMenu.Add(indytoolbar);
                            chartWindow.MainTabControl.SelectionChanged += TabSelectionChangedHandler;

                            foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items) if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem) indytoolbar.Visibility = System.Windows.Visibility.Visible;
                            System.Windows.Automation.AutomationProperties.SetAutomationId(indytoolbar, toolbarname + uID);
                        }
                    }));
                }
                #endregion

//				var x = Close;
//				if(pMA1DataType == RR_Lines_DataType.Open)    TheMA1 = TEMA(Open,pPeriod1);
//				if(pMA1DataType == RR_Lines_DataType.High)    TheMA1 = TEMA(High,pPeriod1);
//				if(pMA1DataType == RR_Lines_DataType.Low)     TheMA1 = TEMA(Low,pPeriod1);
//				if(pMA1DataType == RR_Lines_DataType.Median)  TheMA1 = TEMA(Median,pPeriod1);
//				if(pMA1DataType == RR_Lines_DataType.Typical) TheMA1 = TEMA(Typical,pPeriod1);
				TheMA1 = TEMA(Close,pPeriod1);

//				if(pMA2DataType == RR_Lines_DataType.Open)    x = Open;
//				if(pMA2DataType == RR_Lines_DataType.High)    x = High;
//				if(pMA2DataType == RR_Lines_DataType.Low)     x = Low;
//				if(pMA2DataType == RR_Lines_DataType.Median)  x = Median;
//				if(pMA2DataType == RR_Lines_DataType.Typical) x = Typical;

				TheMA2 = HMA(Close,pPeriod2);
			}
			else if (State == State.Terminated)
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
		private System.Collections.Generic.List<string> GetATMNames(){
			#region -- GetATMNames --
			string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"templates","AtmStrategy");
			string search = "*.xml";

			System.IO.FileInfo[] filCustom=null;
			try{
				var dirCustom = new System.IO.DirectoryInfo(folder);
				filCustom = dirCustom.GetFiles(search);
			}catch{}

			var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
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
        private string toolbarname = "RoadRunner_TB", uID;
        private bool isToolBarButtonAdded = false;
        private NinjaTrader.Gui.Chart.Chart chartWindow;
        private System.Windows.Controls.Grid indytoolbar;
        private System.Windows.Controls.Menu MenuControlContainer;
        private System.Windows.Controls.MenuItem MenuControl;
        private System.Windows.Controls.MenuItem miStatus, miDirection, miShowPnL, miSig1Qty;
		private System.Windows.Controls.MenuItem miSig1Template, miRecalculate1;
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
				mi.FontStyle = System.Windows.FontStyles.Italic;
				mi.IsEnabled = false;
			}else{
				mi.FontStyle = System.Windows.FontStyles.Normal;
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
            MenuControlContainer = new System.Windows.Controls.Menu { Background = System.Windows.Media.Brushes.Orange, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            MenuControl = new System.Windows.Controls.MenuItem
            {
                BorderThickness = new System.Windows.Thickness(2),
                Header = pButtonText,
                BorderBrush = pButtonForeground,
                Foreground = pButtonForeground,
                Background = pButtonBackground,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                FontWeight = System.Windows.FontWeights.Bold,
                FontSize = 13
            };
			MenuControl.MouseEnter += delegate (object o, System.Windows.Input.MouseEventArgs e){
				e.Handled = true;
				miStatus.Header = "Status:  " + (IsAlwaysActive ? "Active ALWAYS" : (IsTradingPaused ? "PAUSED": "Active"));
                miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
//				Print("Status header: "+miStatus.Header.ToString());
			};
            MenuControlContainer.Items.Add(MenuControl);

            #region Status
            miStatus = new System.Windows.Controls.MenuItem { Header = "Status:  " + (IsAlwaysActive ? "Active ALWAYS" : (IsTradingPaused ? "PAUSED": "Active")), Name = "Status" + uID, Foreground = System.Windows.Media.Brushes.Black, FontWeight = System.Windows.FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miStatus.Click += delegate (object o, System.Windows.RoutedEventArgs e)
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
            miDirection = new System.Windows.Controls.MenuItem { Header = "Direction: " + pPermittedDirection.ToString().ToUpper(), Name = "Direction" + uID, Foreground = System.Windows.Media.Brushes.Black, FontWeight = System.Windows.FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miDirection.Click += delegate (object o, System.Windows.RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						if(pPermittedDirection == RoadRunner_PermittedDirections.Both) pPermittedDirection = RoadRunner_PermittedDirections.Short;
						else if(pPermittedDirection == RoadRunner_PermittedDirections.Short) pPermittedDirection = RoadRunner_PermittedDirections.Long;
						else if(pPermittedDirection == RoadRunner_PermittedDirections.Long) pPermittedDirection = RoadRunner_PermittedDirections.Trend;
						else if(pPermittedDirection == RoadRunner_PermittedDirections.Trend) pPermittedDirection = RoadRunner_PermittedDirections.Both;
                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(pPermittedDirection == RoadRunner_PermittedDirections.Both) pPermittedDirection = RoadRunner_PermittedDirections.Short;
							else if(pPermittedDirection == RoadRunner_PermittedDirections.Short) pPermittedDirection = RoadRunner_PermittedDirections.Long;
							else if(pPermittedDirection == RoadRunner_PermittedDirections.Long) pPermittedDirection = RoadRunner_PermittedDirections.Trend;
							else if(pPermittedDirection == RoadRunner_PermittedDirections.Trend) pPermittedDirection = RoadRunner_PermittedDirections.Both;
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
						if(pPermittedDirection == RoadRunner_PermittedDirections.Both) pPermittedDirection = RoadRunner_PermittedDirections.Short;
						else if(pPermittedDirection == RoadRunner_PermittedDirections.Short) pPermittedDirection = RoadRunner_PermittedDirections.Long;
						else if(pPermittedDirection == RoadRunner_PermittedDirections.Long) pPermittedDirection = RoadRunner_PermittedDirections.Trend;
						else if(pPermittedDirection == RoadRunner_PermittedDirections.Trend) pPermittedDirection = RoadRunner_PermittedDirections.Both;
                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(pPermittedDirection == RoadRunner_PermittedDirections.Both) pPermittedDirection = RoadRunner_PermittedDirections.Short;
							else if(pPermittedDirection == RoadRunner_PermittedDirections.Short) pPermittedDirection = RoadRunner_PermittedDirections.Long;
							else if(pPermittedDirection == RoadRunner_PermittedDirections.Long) pPermittedDirection = RoadRunner_PermittedDirections.Trend;
							else if(pPermittedDirection == RoadRunner_PermittedDirections.Trend) pPermittedDirection = RoadRunner_PermittedDirections.Both;
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
            miSig1Qty = new System.Windows.Controls.MenuItem { Header = $"Signal qty: {pSignal1Qty}", Name = "Sig1Qty" + uID, Foreground = System.Windows.Media.Brushes.Black, FontWeight = System.Windows.FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miSig1Qty.Click += delegate (object o, System.Windows.RoutedEventArgs e)
            {
                if (ChartControl != null)
                {
					e.Handled = true;
                    if (ChartControl.Dispatcher.CheckAccess())
                    {
						pSignal1Qty++;
						miSig1Qty.Header = $"Signal qty: {pSignal1Qty}";
						InformUserAboutRecalculation();
						if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							pSignal1Qty++;
							miSig1Qty.Header = $"Signal qty: {pSignal1Qty}";
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
						miSig1Qty.Header = $"Signal qty: {pSignal1Qty}";
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
							miSig1Qty.Header = $"Signal qty: {pSignal1Qty}";
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
            miSig1Template = new System.Windows.Controls.MenuItem { Header = $" ATM Name: '{pATMStrategyName1}'", Name = "Sig1Template" + uID, Foreground = System.Windows.Media.Brushes.Black, FontWeight = System.Windows.FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
			if(pSignal1Qty == 0) EnableDisableMenuItem("Disable", miSig1Template); else EnableDisableMenuItem("Enable", miSig1Template);
            miSig1Template.Click += delegate (object o, System.Windows.RoutedEventArgs e)
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

			#region -- Recalc Stats --
			miRecalculate1 = new System.Windows.Controls.MenuItem { Header = "RE-CALCULATE Stats?", HorizontalAlignment = System.Windows.HorizontalAlignment.Center , Background = System.Windows.Media.Brushes.Yellow, Foreground = System.Windows.Media.Brushes.Black, FontWeight = System.Windows.FontWeights.Bold , StaysOpenOnClick = false };
			miRecalculate1.Visibility = System.Windows.Visibility.Collapsed;
			miRecalculate1.Click += delegate (object o, System.Windows.RoutedEventArgs e){
				e.Handled = true;
				ResetRecalculationUI();
				System.Windows.Forms.SendKeys.SendWait("{F5}");
			};
			MenuControl.Items.Add(miRecalculate1);
			#endregion

			MenuControl.Items.Add(new System.Windows.Controls.Separator());

            #region Show PnL stats
            miShowPnL = new System.Windows.Controls.MenuItem { Header = (pShowPnLStats ? "Hide PnL stats?":"Show PnL stats?"), Name = "ShowPnL" + uID, Foreground = System.Windows.Media.Brushes.Black, FontWeight = System.Windows.FontWeights.Normal, StaysOpenOnClick = true, IsCheckable = false, IsChecked = false };
            miShowPnL.Click += delegate (object o, System.Windows.RoutedEventArgs e)
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
			miRecalculate1.Visibility = System.Windows.Visibility.Visible;
//			miRecalculate1.Background = Brushes.Yellow;
	//		miRecalculate1.FontWeight = FontWeights.Bold;
		//	miRecalculate1.FontStyle = FontStyles.Italic;
		}
		private void ResetRecalculationUI(){
			miRecalculate1.Visibility = System.Windows.Visibility.Collapsed;
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
                indytoolbar.Visibility = temp.ChartControl == ChartControl ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
		#endregion
        #endregion

//=================================================================================================================================================
		private void CheckForExistingOrdersPositions(Account myAccount){
			if(ChartControl.Properties.ChartTraderVisibility != ChartTraderVisibility.Visible){
				string msg = string.Empty;
				if(myAccount.Orders.Count>0) msg = "orders";
				if(myAccount.Positions.Count>0 && msg.Length>0) msg = msg +" and positions"; else msg = "positions";
				if(myAccount.Orders.Count>0 || myAccount.Positions.Count>0){
					MessageTime = DateTime.Now;
					Draw.TextFixed(this, "preexist", "Turn-on ChartTrader to possibly view your current "+msg, TextPosition.BottomLeft, System.Windows.Media.Brushes.Black, new NinjaTrader.Gui.Tools.SimpleFont("Arial", 18), System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.Maroon, 90);
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
			if(pPermittedDirection == RoadRunner_PermittedDirections.Trend && SwingLineTrendStr != "UP") return;//buy signal is not permitted against down trend
			if(tm.DailyTgtAchieved) {
				Print(Times[0][0].ToShortDateString()+"  Daily PnL target achieved, no buy order submitted");
				return;
			}

			var ordertype   = OrderType.Market;
try{
			if((pPermittedDirection == RoadRunner_PermittedDirections.Long || pPermittedDirection == RoadRunner_PermittedDirections.Both || pPermittedDirection == RoadRunner_PermittedDirections.Trend)){
				var position_size = GetCurrentMarketPosition();
				if(position_size < 0){//we're short, reverse to long requested...exit out the shorts
					myAccount.Flatten(Instruments);
					tm.Note = "Reversing";
					tm.GoFlat(Times[0][0], Closes[0][0]);
				}
				var LongsPermittedNow = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
				if(State==State.Historical && LongsPermittedNow && tm.SLTPs.ContainsKey(SignalID)){
//Print($"Buying: #{SignalID}   current position size: {position_size}");
					double EntryPrice = Closes[0][0];// + TickSize;
					double TgtPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice + (tm.SLTPs[SignalID].ATRmultTarget > 0 ? tm.SLTPs[SignalID].ATRmultTarget * atr("points", 14) : tm.SLTPs[SignalID].DollarsTarget / PV));
					double SLPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice - (tm.SLTPs[SignalID].ATRmultSL > 0 ? tm.SLTPs[SignalID].ATRmultSL * atr("points", 14) : tm.SLTPs[SignalID].DollarsSL / PV));
					Draw.Dot(this, $"tgt{CurrentBars[0]}", false, 0, TgtPrice, System.Windows.Media.Brushes.LimeGreen);
					Draw.Dot(this, $"sl{CurrentBars[0]}", false, 0, SLPrice, System.Windows.Media.Brushes.Magenta);
					Print($"------ {Times[0][0].ToString()} Now Long from {EntryPrice}, Tgt: {TgtPrice}, SL {SLPrice}");
					tm.AddTrade('L', Qty, EntryPrice, Times[0][0], SLPrice, TgtPrice);
				}
				if(pATMStrategyName.Trim().Length==0 || Qty==0) return;
				if(State != State.Realtime) return;
				if(SelectedAccountName.Trim().Length == 0) return;
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
	Draw.TextFixed(this,"OrderError", Order_Reject_Message, TextPosition.Center, System.Windows.Media.Brushes.Magenta,new NinjaTrader.Gui.Tools.SimpleFont("Arial",14), System.Windows.Media.Brushes.DimGray, System.Windows.Media.Brushes.Black,100);
	ForceRefresh();
}
        }
		#endregion

		#region - SellUsingATM -
        void SellUsingATM(string pATMStrategyName, int Qty, string SignalID)
        {
			if(pPermittedDirection == RoadRunner_PermittedDirections.Trend && SwingLineTrendStr != "DOWN") return;//sell signal is not permitted against up trend
			if(tm.DailyTgtAchieved) {
				Print(Times[0][0].ToShortDateString()+"  Daily PnL target achieved, no sell order submitted");
				return;
			}

			var ordertype   = OrderType.Market;
try{
			if((pPermittedDirection == RoadRunner_PermittedDirections.Short || pPermittedDirection == RoadRunner_PermittedDirections.Both || pPermittedDirection == RoadRunner_PermittedDirections.Trend)){
				var position_size = GetCurrentMarketPosition();
				if(position_size > 0){//we're long, reverse to short requested...exit out the longs
					myAccount.Flatten(Instruments);
					tm.Note = "Reversing";
					tm.GoFlat(Times[0][0], Closes[0][0]);
				}
				var ShortsPermittedNow = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);
				if(State==State.Historical && ShortsPermittedNow && tm.SLTPs.ContainsKey(SignalID)){
//Print($"Selling: #{SignalID}   current position size: {position_size}");
					double EntryPrice = Closes[0][0];// - TickSize;
					double TgtPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice - (tm.SLTPs[SignalID].ATRmultTarget > 0 ? tm.SLTPs[SignalID].ATRmultTarget * atr("points", 14) : tm.SLTPs[SignalID].DollarsTarget / PV));
					double SLPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice + (tm.SLTPs[SignalID].ATRmultSL > 0 ? tm.SLTPs[SignalID].ATRmultSL * atr("points", 14) : tm.SLTPs[SignalID].DollarsSL / PV));
					Draw.Dot(this,$"tgt{CurrentBars[0]}", false,0, TgtPrice, System.Windows.Media.Brushes.LimeGreen);
					Draw.Dot(this,$"sl{CurrentBars[0]}", false, 0, SLPrice, System.Windows.Media.Brushes.Magenta);
					Print($"------ {Times[0][0].ToString()} Now Short from {EntryPrice}, Tgt: {TgtPrice}, SL {SLPrice}");
					tm.AddTrade('S', Qty, EntryPrice, Times[0][0], SLPrice, TgtPrice);
				}
				if(pATMStrategyName.Trim().Length==0 || Qty==0) return;
				if(State != State.Realtime) return;
				if(SelectedAccountName.Trim().Length == 0) return;
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
	Draw.TextFixed(this,"OrderError",Order_Reject_Message, TextPosition.Center,System.Windows.Media.Brushes.Magenta,new NinjaTrader.Gui.Tools.SimpleFont("Arial",14),System.Windows.Media.Brushes.DimGray,System.Windows.Media.Brushes.Black,100);
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
		private void MyKeyDownEvent(object sender, System.Windows.Input.KeyEventArgs e)
		{
			e.Handled      = true;
			if(System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) && System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.B)){
				this.BuyUsingATM(this.pATMStrategyName1, 1, "");
			}
			if(System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) && System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Space)){
				this.SellUsingATM(this.pATMStrategyName1, 1, "");
			}
		
		}
//=================================================================================================================================================
		private double LastEntryPrice = double.MinValue;
		private int LastEntryABar = 0;
//=================================================================================================================================================
//		private double zltema(Series<double> input, int period){
//			double TEMA1 = TEMA(input, period)[0];
//			double TEMA2 = TEMA(TEMA(input, period), period)[0];
//            return(TEMA1 + (TEMA1 - TEMA2));
//		}
//=================================================================================================================================================
		protected override void OnBarUpdate()
		{
			if(RunInit)
			{
				RunInit=false;
#if SPEECH_ENABLED
				string dll = System.IO.Path.Combine(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"bin","custom"),"interop.speechlib.dll");   //"c:\users\ben\Documents\NinjaTrader 7\"
				if(System.IO.File.Exists(dll)) {
					SpeechEnabled = true;
					SpeechLib = Assembly.LoadFile(dll);
				}
#endif
				TimeFrameText = Bars.BarsPeriod.ToString().Replace("min","minutes");
				if(Bars.BarsPeriod.Value==1) TimeFrameText = TimeFrameText.Replace("minutes","minute");
			}
//Print(331);
			if(TimeOfText != DateTime.MaxValue && ChartControl!=null) {
				TimeSpan ts = new TimeSpan(Math.Abs(TimeOfText.Ticks-NinjaTrader.Core.Globals.Now.Ticks));
				if(ts.TotalSeconds>5) RemoveDrawObject("infomsg");
				else Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center,ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.LabelFont, Brushes.Black, ChartControl.Background, 10);
			}
//Print(337);
			if(DirectionFillRegionName == DateTime.MinValue)
				DirectionFillRegionName = Time[0];

//Print(341);
			BackBrush = null;//Normal_bkgColor;
			Signal[0] = (FLAT);
//Print(346);
			if(IsFirstTickOfBar) {
				AlertsThisBar=0;
				EmailsThisBar = 0;
				if(CurrentBar>1) {
					SignalTrend[0] = (SignalTrend[1]);
					if(Signal[1]>FLAT) Direction = LONG;
					if(Signal[1]<FLAT) Direction = SHORT;
					if(Direction==LONG) SignalTrend[0] = (LONG);
					else if(Direction==SHORT) SignalTrend[0] = (SHORT);
					else SignalTrend[0] = (FLAT);
				}
			}
//Print(359);
//			if(SignalTrend[0]!=FLAT && pBackgroundOpacity>0 && pSignalTrend_BackgroundColor!=Color.Transparent) {
//				BackBrush = Color.FromArgb(this.pBackgroundOpacity*25,pSignalTrend_BackgroundColor);
//			}

//			if(CurrentBar>0) {
//				offsetFast[0] = (offsetFast[1]);
//				offsetSlow[0] = (offsetSlow[1]);
//			}
			if(CurrentBar<3) return;
			#region -- Determine Trend for TradeAssist --
			if(pBigTrendPeriod>0){
				BigTrend[0] = bigtrend[0];
				if(BigTrend[0] > BigTrend[1]) SwingLineTrendStr = "UP";
				else if(BigTrend[0] < BigTrend[1]) SwingLineTrendStr = "DOWN";
			}
			#endregion
			
			#region -- Determine entry conditions for TradeAssist --
			var t1   = ToTime(Times[0][1])/100;
			var t    = ToTime(Times[0][0])/100;
			var dow1 = Times[0][1].DayOfWeek;
			var dow  = Times[0][0].DayOfWeek;
			var date = Times[0][0].Date;
			tm.PrintResults(BarsArray[0].Count, CurrentBars[0], true, this);
			bool TradeToday = tm.DOW.Contains(dow);
			bool LongsPermittedNow  = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
			bool ShortsPermittedNow = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);

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
			#endregion

			bool CrossUp = false;
			bool CrossDown = false;
			bool CrossOutwardUp = false;
			bool CrossOutwardDown = false;
			if(pMethod == RoadRunner_Method.Original){
				if(pZoneSize == 0) {
					if(CrossAbove(TheMA1, TheMA2, 1)) CrossUp=true;
					else CrossDown = CrossBelow(TheMA1, TheMA2, 1);
				} else {
					if(TheMA1[1]-TheMA2[1] > zone && TheMA1[0]-TheMA2[0] <= zone)      CrossDown = true;
					else if(TheMA2[1]-TheMA1[1] > zone && TheMA2[0]-TheMA1[0] <= zone) CrossUp = true;
					else if(Math.Abs(TheMA1[1]-TheMA2[1]) <= zone) {//MA's are within the zone already
						if(TheMA1[0]-TheMA2[0] > zone)      CrossOutwardUp = true;
						else if(TheMA2[0]-TheMA1[0] > zone) CrossOutwardDown = true;
					}
				}
			}else{//use Simov indicator
				if(simov.Buy.IsValidDataPoint(0)) CrossUp = true;
				if(simov.Sell.IsValidDataPoint(0)) CrossDown = true;
			}

			if(pShowInwardSignals || pShowOutwardSignals){
				tag = $"RRLines{CurrentBar}";
				RemoveDrawObject(tag);
			}
			if(pShowInwardSignals || pZoneSize==0) {
				if(CrossUp) {	
					Signal[0] = (SIGNAL_MA1_OVER_MA2);

					SayIt(pSpeakOnUpwardCross,Close[0]);
					if(AlertsThisBar<pMaxAlerts && pCrossInwardUpAlertSound) {
						PlaySoundCustom(pSoundFileInwardUp);
						AlertsThisBar++;
					}
					if(pLaunchPopup && (State != State.Historical)&& PopupBar != CurrentBar) {
						Log("RR_Lines is LONG on "+Instrument.ToString()+" "+Bars.BarsPeriod.ToString(),LogLevel.Alert);
						PopupBar = CurrentBar;
					}
					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
						EmailsThisBar++;
						Subj = string.Concat(Name," is LONG on ",Instrument.ToString()," ",Bars.BarsPeriod.ToString());
						Body = string.Concat(NL,NL,"Micro has crossed ABOVE Macro");
						SendMail(emailaddress,Subj,Body);
					}
					if(pSymbolTypeInward==RR_Lines_SymbolType.Arrows)			Draw.ArrowUp(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,false, pLongArrowTemplateName);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Triangles)	Draw.TriangleUp(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pMarkerLongBrush);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Dots)		Draw.Dot(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pMarkerLongBrush);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Squares)		Draw.Square(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pMarkerLongBrush);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Diamonds)	Draw.Diamond(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pMarkerLongBrush);
					DrawPnLLine(LastEntryPrice - Closes[0][0]);
					if(pSignal1Qty>0 && EntryABar != CurrentBar) BuyUsingATM(pATMStrategyName1, pSignal1Qty, "1");
				} else if (CrossDown) {
					Signal[0] = (SIGNAL_MA1_UNDER_MA2);

					SayIt(pSpeakOnDownwardCross,Close[0]);
					if(AlertsThisBar<pMaxAlerts && pCrossInwardDownAlertSound) {
						PlaySoundCustom(pSoundFileInwardDown);
						AlertsThisBar++;
					}
					if(pLaunchPopup && (State != State.Historical)&& PopupBar != CurrentBar) {
						Log(Name+" is SHORT on "+Instrument.ToString()+" "+Bars.BarsPeriod.ToString(),LogLevel.Alert);
						PopupBar = CurrentBar;
					}
					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
						EmailsThisBar++;
						Subj = string.Concat(Name," is SHORT on ",Instrument.ToString()," ",Bars.BarsPeriod.ToString());
						Body = string.Concat(NL,NL,"Micro has crossed BELOW Macro");
						SendMail(emailaddress,Subj,Body);
					}
					if(pSymbolTypeInward==RR_Lines_SymbolType.Arrows)			Draw.ArrowDown(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,false, pShortArrowTemplateName);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Triangles)	Draw.TriangleDown(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pMarkerShortBrush);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Dots)		Draw.Dot(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pMarkerShortBrush);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Squares)		Draw.Square(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pMarkerShortBrush);
					else if(pSymbolTypeInward==RR_Lines_SymbolType.Diamonds)	Draw.Diamond(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pMarkerShortBrush);
					DrawPnLLine(Closes[0][0] - LastEntryPrice);
					if(pSignal1Qty>0 && EntryABar != CurrentBar) SellUsingATM(pATMStrategyName1, pSignal1Qty, "1");
				}
			}
			if(pShowOutwardSignals && pZoneSize > 0) {
//				if(CrossOutwardUp) {	
//					Signal[0] = (SIGNAL_MA1_OVER_MA2);

//					SayIt(pSpeakOnUpwardCross,Close[0]);
//					if(AlertsThisBar<pMaxAlerts && pCrossOutwardUpAlertSound) {
//						PlaySoundCustom(pSoundFileOutwardUp);
//						AlertsThisBar++;
//					}
//					if(pLaunchPopup && (State != State.Historical)&& PopupBar != CurrentBar) {
//						Log("RR_Lines is LONG on "+Instrument.ToString()+" "+Bars.BarsPeriod.ToString(),LogLevel.Alert);
//						PopupBar = CurrentBar;
//					}
//					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
//						EmailsThisBar++;
//						Subj = string.Concat(Name," is LONG on ",Instrument.ToString()," ",Bars.BarsPeriod.ToString());
//						Body = string.Concat(NL,NL,"Micro has crossed ABOVE Macro");
//						SendMail(emailaddress,Subj,Body);
//					}
//					if(pSymbolTypeOutward==RR_Lines_SymbolType.Arrows)			Draw.ArrowUp(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pOutwardUpBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Triangles)	Draw.TriangleUp(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pOutwardUpBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Dots)		Draw.Dot(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pOutwardUpBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Squares)	Draw.Square(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pOutwardUpBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Diamonds)	Draw.Diamond(this, tag,IsAutoScale,0,Low[0]-pSeparation*TickSize,pOutwardUpBrush);
//					DrawPnLLine(LastEntryPrice - Closes[0][0]);
//					if(pSignal1Qty>0 && EntryABar != CurrentBar) BuyUsingATM(pATMStrategyName1, pSignal1Qty, "1");
//				} else if (CrossOutwardDown) {
//					Signal[0] = (SIGNAL_MA1_UNDER_MA2);

//					SayIt(pSpeakOnDownwardCross,Close[0]);
//					if(AlertsThisBar<pMaxAlerts && pCrossOutwardDownAlertSound) {
//						PlaySoundCustom(pSoundFileOutwardDown);
//						AlertsThisBar++;
//					}
//					if(pLaunchPopup && (State != State.Historical)&& PopupBar != CurrentBar) {
//						Log(Name+" is LONG on "+Instrument.ToString()+" "+Bars.BarsPeriod.ToString(),LogLevel.Alert);
//						PopupBar = CurrentBar;
//					}
//					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
//						EmailsThisBar++;
//						Subj = string.Concat(Name," is SHORT on ",Instrument.ToString()," ",Bars.BarsPeriod.ToString());
//						Body = string.Concat(NL,NL,"Micro has crossed BELOW Macro");
//						SendMail(emailaddress,Subj,Body);
//					}
//					if(pSymbolTypeOutward==RR_Lines_SymbolType.Arrows)			Draw.ArrowDown(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pOutwardDownBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Triangles)	Draw.TriangleDown(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pOutwardDownBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Dots)		Draw.Dot(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pOutwardDownBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Squares)	Draw.Square(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pOutwardDownBrush);
//					else if(pSymbolTypeOutward==RR_Lines_SymbolType.Diamonds)	Draw.Diamond(this, tag,IsAutoScale,0,High[0]+pSeparation*TickSize,pOutwardDownBrush);
//					DrawPnLLine(Closes[0][0] - LastEntryPrice);
//					if(pSignal1Qty>0 && EntryABar != CurrentBar) SellUsingATM(pATMStrategyName1, pSignal1Qty, "1");
//				}
			}
//			if(Signal[0] != 0 && pBackgroundOpacity>0 && pSignal_BackgroundBrush!=Brushes.Transparent){
//				BackBrush = pSignal_BackgroundBrush;//Color.FromArgb(this.pBackgroundOpacity*25,pSignal_BackgroundBrush);
//			}

//			for(int x = 0; x<CurrentBar-2; x++) {
//				if(Direction==LONG)  if(TheMA1[x]<TheMA2[x]) {SignalTrendAge[0] = (x); break;}
//				if(Direction==SHORT) if(TheMA1[x]>TheMA2[x]) {SignalTrendAge[0] = (-x);break;}
//			}
			
			#region Bar to MA1 and Bar to MA2 related signals
//			BarToMA1[0] = (0);
//			BarToMA2[0] = (0);
			if(Open[0] <= TheMA1[0] && Close[0] > TheMA1[0]){
//				BarToMA1[0] = (BarToMA1_CROSSES_MA1_UPWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleOverMA1.Length>0) {
					PlaySoundCustom(pSoundFileStraddleOverMA1);
					AlertsThisBar++;
				}
			}
			else if(Open[0] >= TheMA1[0] && Close[0] < TheMA1[0]){
//				BarToMA1[0] = (BarToMA1_CROSSES_MA1_DOWNWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleUnderMA1.Length>0) {
					PlaySoundCustom(pSoundFileStraddleUnderMA1);
					AlertsThisBar++;
				}
			}
			else if(Low[1] > TheMA1[1]) {
//				BarToMA1[0] = (BarToMA1_BAR_ABOVE_MA1);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarOverMA1.Length>0) {
					PlaySoundCustom(pSoundFileBarOverMA1);
					AlertsThisBar++;
				}
			}
			else if(High[1] < TheMA1[1]) {
//				BarToMA1[0] = (BarToMA1_BAR_BELOW_MA1);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarUnderMA1.Length>0) {
					PlaySoundCustom(pSoundFileBarUnderMA1);
					AlertsThisBar++;
				}
			}
			if(Open[0] <= TheMA2[0] && Close[0] > TheMA2[0]){
//				BarToMA2[0] = (BarToMA2_CROSSES_MA2_UPWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleOverMA2.Length>0) {
					PlaySoundCustom(pSoundFileStraddleOverMA2);
					AlertsThisBar++;
				}
			}
			else if(Open[0] >= TheMA2[0] && Close[0] < TheMA2[0]){
//				BarToMA2[0] = (BarToMA2_CROSSES_MA2_DOWNWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleUnderMA2.Length>0) {
					PlaySoundCustom(pSoundFileStraddleUnderMA2);
					AlertsThisBar++;
				}
			}
			else if(Low[1] > TheMA2[1]){
//				BarToMA2[0] = (BarToMA2_BAR_ABOVE_MA2);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarOverMA2.Length>0) {
					PlaySoundCustom(pSoundFileBarOverMA2);
					AlertsThisBar++;
				}
			}
			else if(High[1] < TheMA2[1]) {
//				BarToMA2[0] = (BarToMA2_BAR_BELOW_MA2);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarUnderMA2.Length>0) {
					PlaySoundCustom(pSoundFileBarUnderMA2);
					AlertsThisBar++;
				}
			}
			#endregion
//			if(pBackgroundOpacity>0 && pBarToMA1_BackgroundColor!=Color.Transparent) {
//				if(BarToMA1[0] == BarToMA1_CROSSES_MA1_UPWARD || BarToMA1[0] == BarToMA1_CROSSES_MA1_DOWNWARD){
//					BackBrush = Color.FromArgb(this.pBackgroundOpacity*25,pBarToMA1_BackgroundColor);
//				}
//			}
//			if(pBackgroundOpacity>0 && pBarToMA2_BackgroundColor!=Color.Transparent) {
//				if(BarToMA2[0] == BarToMA2_CROSSES_MA2_UPWARD || BarToMA2[0] == BarToMA2_CROSSES_MA2_DOWNWARD){
//					BackBrush = Color.FromArgb(this.pBackgroundOpacity*25,pBarToMA2_BackgroundColor);
//				}
//			}

			Values[0][0] = (TheMA1[0]);
			Values[1][0] = (TheMA2[0]);
			if(!pShowMALines) {
				PlotBrushes[0][0] = Brushes.Transparent;
				PlotBrushes[1][0] = Brushes.Transparent;
			} else {
				if(pColoringBasis == RR_Lines_ColoringBasis.OnCrossing) {
					if(TheMA1[0]>TheMA2[1]) {
						PlotBrushes[0][0] = pLineUpBrushFast;
						PlotBrushes[1][0] = pLineUpBrushSlow;
					} else {
						PlotBrushes[0][0] = pLineDownBrushFast;
						PlotBrushes[1][0] = pLineDownBrushSlow;
					}
				} else if(pColoringBasis == RR_Lines_ColoringBasis.OnTrendChange) {
					if(TheMA1[0]>TheMA1[1]) PlotBrushes[0][0] = pLineUpBrushFast;
					else PlotBrushes[0][0] = pLineDownBrushFast;
					if(TheMA2[0]>TheMA2[1]) PlotBrushes[1][0] = pLineUpBrushSlow;
					else PlotBrushes[1][0] = pLineDownBrushSlow;
				} else if(pColoringBasis == RR_Lines_ColoringBasis.NoColorChange) {
					PlotBrushes[0][0] = pLineUpBrushFast;
					PlotBrushes[1][0] = pLineUpBrushSlow;
				}
			}
			if(this.pShowDirectionFill) {
				if(!StartFillRegion) {
					if(TheMA1[0]>TheMA2[0] && TheMA1[1]<=TheMA2[1]) StartFillRegion = true;
					if(TheMA1[0]<TheMA2[0] && TheMA1[1]>=TheMA2[1]) StartFillRegion = true;
					if(StartFillRegion) DirectionFillRegionName = Time[0];
				}
				if(StartFillRegion) {
					if(TheMA1[0]>TheMA2[0]) {
						if(TheMA1[1]<=TheMA2[1]) {
							int rbar = 0;
							while(rbar<CurrentBar) {
								DirectionFillRegionName = Time[rbar];
								if(TheMA1[rbar]<TheMA2[rbar]) break;
								rbar++;
							}
						}
						Draw.Region(this, DirectionFillRegionName.ToString(), DirectionFillRegionName, Time[0], TheMA1,TheMA2, Brushes.Transparent, pMA1overMA2FillBrush, pOpacity_MA1overMA2);
					}
					if(TheMA1[0]<TheMA2[0]) {
						if(TheMA1[1]>=TheMA2[1]) {
							int rbar = 0;
							while(rbar<CurrentBar) {
								DirectionFillRegionName = Time[rbar];
								if(TheMA1[rbar]>TheMA2[rbar]) break;
								rbar++;
							}
						}
						Draw.Region(this, DirectionFillRegionName.ToString(), DirectionFillRegionName, Time[0], TheMA1,TheMA2, Brushes.Transparent, pMA1underMA2FillBrush, pOpacity_MA1underMA2);
					}
				}
			}
		}
		private void DrawPnLLine(double NetPts){
			if(State == State.Historical && pDrawPnLLines && LastEntryPrice != double.MinValue){
				if(NetPts > 0)
					Draw.Line(this, $"RR{CurrentBars[0]}", false, Times[0].GetValueAt(LastEntryABar), LastEntryPrice, Times[0][0], Closes[0][0], Brushes.Lime, DashStyleHelper.Dash, 1);
				else
					Draw.Line(this, $"RR{CurrentBars[0]}", false, Times[0].GetValueAt(LastEntryABar), LastEntryPrice, Times[0][0], Closes[0][0], Brushes.Magenta, DashStyleHelper.Dash, 1);
			}
			LastEntryPrice = Closes[0][0];
			LastEntryABar = CurrentBars[0];
		}
		SharpDX.Direct2D1.Brush WhiteBrushDX = null;
		SharpDX.Direct2D1.Brush BlackBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			tm.InitializeBrushes(RenderTarget);
			if(WhiteBrushDX!=null && !WhiteBrushDX.IsDisposed) { WhiteBrushDX.Dispose(); WhiteBrushDX = null;}
			if(RenderTarget!=null) WhiteBrushDX = System.Windows.Media.Brushes.White.ToDxBrush(RenderTarget);
			if(BlackBrushDX!=null && !BlackBrushDX.IsDisposed) { BlackBrushDX.Dispose(); BlackBrushDX = null;}
			if(RenderTarget!=null) BlackBrushDX = System.Windows.Media.Brushes.Black.ToDxBrush(RenderTarget);
		}
        private SharpDX.DirectWrite.TextFormat txtFormat_LabelFont = null;
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!IsVisible) return;
			base.OnRender(chartControl, chartScale);
			float x,y;
			if((pShowPnLStats || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl)) && tm.OutputLS.Count>0){
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

			#region -- Inform user about trading status --
			var msg = "";
			if(tm.DailyTgtAchieved) msg = "Daily PnL target achieved - trading halted";
			else if(SelectedAccountName.Trim().Length == 0 || myAccount==null) msg = "TradeAssist disabled:  Please select a valid trading account name";
			else if(pBigTrendPeriod<=0 && pPermittedDirection==RoadRunner_PermittedDirections.Trend) msg = "To use 'TREND' filter, you must set 'Big Trend Period' to a non-zero number";
			else{
				ATMStrategyNamesEmployed = $"#1:  '{pATMStrategyName1}'  Qty: {pSignal1Qty}";
				msg = (!IsAlwaysActive && IsTradingPaused) ? "TRADING IS PAUSED" : $"{(pPermittedDirection==RoadRunner_PermittedDirections.Both? "Long and Shorts are" : (pPermittedDirection == RoadRunner_PermittedDirections.Long ? "Long ONLY is": (pPermittedDirection == RoadRunner_PermittedDirections.Short? "Short ONLY is":"with TREND is")))} {(IsAlwaysActive ? "ALWAYS ":"")}active on '{SelectedAccountName}' with\n{ATMStrategyNamesEmployed}";
				if(pPermittedDirection == RoadRunner_PermittedDirections.Trend)
					msg = $"{msg}\nTrend is {SwingLineTrendStr}";
			}
			if(true){
				var txtLayout1 = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, msg, txtFormat_LabelFont, ChartPanel.W, txtFormat_LabelFont.FontSize);
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

		#region Plots
		//==================================================================
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MicroLine
		{get { return Values[0]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MacroLine
		{get { return Values[1]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Signal
		{get { return Values[2]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SignalTrend
		{get { return Values[3]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BigTrend {get { return Values[4]; }}
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> SignalTrendAge
//		{get { return Values[4]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> BarToMA1
//		{get { return Values[5]; }}
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> BarToMA2
//		{get { return Values[6]; }}
		//==================================================================
		#endregion
		#region Plot parameters
		private int	pLineWidthFast = 2;
		[Description("Width of Micro line")]
		[Category("Plots")]
		public int LineWidthMicro
		{
			get { return pLineWidthFast; }
			set { pLineWidthFast = Math.Max(1,value); }
		}
		private int	pLineWidthSlow = 3;
		[Description("Width of Macro line")]
		[Category("Plots")]
		public int LineWidthMacro
		{
			get { return pLineWidthSlow; }
			set { pLineWidthSlow = Math.Max(1,value); }
		}
		private PlotStyle pLineStyleFast = PlotStyle.Line;
		[Description("Style of Micro line")]
		[Category("Plots")]
		public PlotStyle LineStyleMicro
		{
			get { return pLineStyleFast; }
			set { pLineStyleFast = value; }
		}
		private PlotStyle pLineStyleSlow = PlotStyle.Line;
		[Description("Style of Macro line")]
		[Category("Plots")]
		public PlotStyle LineStyleMacro
		{
			get { return pLineStyleSlow; }
			set { pLineStyleSlow = value; }
		}
		private DashStyleHelper pLineDashStyleFast = DashStyleHelper.Solid;
		[Description("DashStyle of Micro line")]
		[Category("Plots")]
		public DashStyleHelper LineDashStyleMicro
		{
			get { return pLineDashStyleFast; }
			set { pLineDashStyleFast = value; }
		}
		private DashStyleHelper pLineDashStyleSlow = DashStyleHelper.Solid;
		[Description("DashStyle of Macro line")]
		[Category("Plots")]
		public DashStyleHelper LineDashStyleMacro
		{
			get { return pLineDashStyleSlow; }
			set { pLineDashStyleSlow = value; }
		}
		private Brush pLineUpBrushFast = Brushes.Lime;
		[XmlIgnore()]
		[Description("")]
		[Category("Plots")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Micro Up",  GroupName = "Plots")]
		public Brush LupFC{	get { return pLineUpBrushFast; }	set { pLineUpBrushFast = value; }		}
		[Browsable(false)]
		public string LupFClSerialize
		{	get { return Serialize.BrushToString(pLineUpBrushFast); } set { pLineUpBrushFast = Serialize.StringToBrush(value); }
		}
		private Brush pLineDownBrushFast = Brushes.Red;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Micro Down",  GroupName = "Plots")]
		public Brush LDownFC{	get { return pLineDownBrushFast; }	set { pLineDownBrushFast = value; }		}
		[Browsable(false)]
		public string LDownFClSerialize
		{	get { return Serialize.BrushToString(pLineDownBrushFast); } set { pLineDownBrushFast = Serialize.StringToBrush(value); }
		}
		private Brush pLineUpBrushSlow = Brushes.Green;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Macro Up",  GroupName = "Plots")]
		public Brush LupSC{	get { return pLineUpBrushSlow; }	set { pLineUpBrushSlow = value; }		}
		[Browsable(false)]
		public string LupSClSerialize
		{	get { return Serialize.BrushToString(pLineUpBrushSlow); } set { pLineUpBrushSlow = Serialize.StringToBrush(value); }
		}
		private Brush pLineDownBrushSlow = Brushes.Crimson;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Macro Down",  GroupName = "Plots")]
		public Brush LDownSC{	get { return pLineDownBrushSlow; }	set { pLineDownBrushSlow = value; }		}
		[Browsable(false)]
		public string LDownSClSerialize
		{	get { return Serialize.BrushToString(pLineDownBrushSlow); } set { pLineDownBrushSlow = Serialize.StringToBrush(value); }
		}
		#endregion
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds",wav);
		}
//====================================================================

		#region Properties
		#region Background
		private int pBackgroundOpacity = 0;
//		[Description("Opacity (0-10) for Bkgrnd colors.  Set to '0' to turn-off coloring")]
//		[Gui.Design.DisplayNameAttribute("Bkgrnd Opacity")]
//		[Category("Background")]
//		public int BackgroundOpacity
//		{	get { return pBackgroundOpacity; }	set { pBackgroundOpacity = value; }	}

		private Brush pSignalTrend_BackgroundBrush = Brushes.Transparent;
//		[XmlIgnore()]
//		[Description("Colorize the background of the chart when the SignalTrend is in a signal condition")]
//		[Category("Background")]
//		[Gui.Design.DisplayNameAttribute("SignalTrend")]
//		public Brush SignalTrendBC{	get { return pSignalTrend_BackgroundBrush; }	set { pSignalTrend_BackgroundBrush = value; }		}
//		[Browsable(false)]
//		public string SignalTrendClSerialize
//		{	get { return Serialize.BrushToString(pSignalTrend_BackgroundColor); } set { pSignalTrend_BackgroundColor = Serialize.StringToBrush(value); }
//		}
		private Brush pSignal_BackgroundBrush = Brushes.Transparent;
//		[XmlIgnore()]
//		[Description("Colorize the background of the chart when the Signal is in a signal condition")]
//		[Category("Background")]
//		[Gui.Design.DisplayNameAttribute("Signal")]
//		public Brush SignalBC{	get { return pSignal_BackgroundBrush; }	set { pSignal_BackgroundBrush = value; }		}
//		[Browsable(false)]
//		public string SignalClSerialize
//		{	get { return Serialize.BrushToString(pSignal_BackgroundBrush); } set { pSignal_BackgroundBrush = Serialize.StringToBrush(value); }
//		}
		private Brush pBarToMA1_BackgroundBrush = Brushes.Transparent;
//		[XmlIgnore()]
//		[Description("Colorize the background of the chart when the BarToMA1 is in a signal condition")]
//		[Category("Background")]
//		[Gui.Design.DisplayNameAttribute("BarToMA1")]
//		public Brush B2MA1BC{	get { return pBarToMA1_BackgroundBrush; }	set { pBarToMA1_BackgroundBrush = value; }		}
//		[Browsable(false)]
//		public string B2MA1ClSerialize
//		{	get { return Serialize.BrushToString(pBarToMA1_BackgroundColor); } set { pBarToMA1_BackgroundColor = Serialize.StringToBrush(value); }
//		}
		private Brush pBarToMA2_BackgroundBrush = Brushes.Transparent;
//		[XmlIgnore()]
//		[Description("Colorize the background of the chart when the BarToMA2 is in a signal condition")]
//		[Category("Background")]
//		[Gui.Design.DisplayNameAttribute("BarToMA2")]
//		public Brush B2MA2BC{	get { return pBarToMA2_BackgroundBrush; }	set { pBarToMA2_BackgroundBrush = value; }		}
//		[Browsable(false)]
//		public string B2MA2ClSerialize
//		{	get { return Serialize.BrushToString(pBarToMA2_BackgroundColor); } set { pBarToMA2_BackgroundColor = Serialize.StringToBrush(value); }
//		}
		#endregion

		#region -- Trade Assist --
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

		[Display(Name="Show PnL Stats", Order=40, GroupName="Backtester", Description="")]
		public bool pShowPnLStats
		{get;set;}

		private string pTargetDistStr1 = "UseATM";
		private string pStoplossDistStr1 = "UseATM";

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Daily Profit Stop $", Order=70, GroupName="Backtester", Description="")]
		public double pDailyTgtDollars
		{get;set;}
		
		[Display(Name="Show Hourly PnL", Order=80, GroupName="Backtester", Description="")]
		public bool pShowHrOfDayPnL
		{get;set;}

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

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
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
		public RoadRunner_PermittedDirections pPermittedDirection {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadpATMStrategyNames))]
		[Display(Order = 30, Name = "Signal ATM Strategy Name", GroupName = "Trade Assist", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template")]
		public string pATMStrategyName1 { get; set; }

		private int pSignal1Qty = 0;
		[Description("Set to '0' to stop trading")]
		[Display(Order = 40, Name = "Signal Qty",  GroupName = "Trade Assist", ResourceType = typeof(Custom.Resource))]
		public int Signal1Qty{
			get { return pSignal1Qty; }
			set { pSignal1Qty = Math.Max(0,value); }
		}
		private int pLabelFontSize = 24;
		[Description("Font size (in pixels)")]
		[Display(Order = 50, Name = "Label FontSize",  GroupName = "Trade Assist", ResourceType = typeof(Custom.Resource))]
		public int LabelFontSize{
			get { return pLabelFontSize; }
			set { pLabelFontSize = value; }
		}
		#endregion
		#region -- UI Pulldown --
        private System.Windows.Media.Brush pButtonForeground = System.Windows.Media.Brushes.Black;
		[XmlIgnore()]
		[Display(Order=10, Name="Button Foreground", GroupName="UI Pulldown")]
		public System.Windows.Media.Brush ButtonForeground
		{
			get { return pButtonForeground; }
			set { pButtonForeground = value; }
		}
				[Browsable(false)]
				public string ButtonForegroundClSerialize
				{	get { return Serialize.BrushToString(pButtonForeground); } set { pButtonForeground = Serialize.StringToBrush(value); }
				}
        private System.Windows.Media.Brush pButtonBackground = System.Windows.Media.Brushes.Orchid;
		[XmlIgnore()]
		[Display(Order=20, Name="Button Background", GroupName="UI Pulldown")]
		public System.Windows.Media.Brush ButtonBackground
		{
			get { return pButtonBackground; }
			set { pButtonBackground = value; }
		}
				[Browsable(false)]
				public string ButtonBackgroundClSerialize
				{	get { return Serialize.BrushToString(pButtonBackground); } set { pButtonBackground = Serialize.StringToBrush(value); }
				}
		private string pButtonText = "RoadRunner";
		[Display(Order=30, Name="Button Text", GroupName="UI Pulldown")]
		public string ButtonText
		{
			get { return pButtonText; }
			set { pButtonText = value; }
		}
		#endregion
		
		string pSpeakOnUpwardCross = "/[InstrumentName] [TimeFrame] long";
		string pSpeakOnDownwardCross = "/[InstrumentName] [TimeFrame] short";
#if SPEECH_ENABLED
		[Description("Message to speak when fast MA crosses over the slow...leave blank to turn-off spoken message, or put an '/' in the front of the string")]
		[Category("Alert Voice")]
		public string SpeakOnUpwardCross
		{
			get { return pSpeakOnUpwardCross; }
			set { pSpeakOnUpwardCross = value; }
		}
		[Description("Message to speak when fast MA crosses below the slow...leave blank to turn-off spoken message, or put an '/' in the front of the string")]
		[Category("Alert Voice")]
		public string SpeakOnDownwardCross
		{
			get { return pSpeakOnDownwardCross; }
			set { pSpeakOnDownwardCross = value; }
		}
#endif
		[Display(Order = 10, Name = "Micro Period",  GroupName = "Parameters", Description="", ResourceType = typeof(Custom.Resource))]
		public int MicroPeriod
		{
			get { return pPeriod1; }
			set { pPeriod1 = Math.Max(1, value); }
		}

		[Display(Order = 20, Name = "Macro Period",  GroupName = "Parameters", Description="", ResourceType = typeof(Custom.Resource))]
		public int MacroPeriod
		{
			get { return pPeriod2; }
			set { pPeriod2 = Math.Max(1, value); }
		}
		
		[Display(Order = 30, Name = "Signal Method",  GroupName = "Parameters", Description="Original is a moving average cross, Simov is a proprietary algo filtered by volume activity", ResourceType = typeof(Custom.Resource))]
		public RoadRunner_Method pMethod
		{get; set;}

		private int pBigTrendPeriod = 50; //Used to filter out counter trend trades
		[Display(Order = 40, Name = "Big Trend Period",  GroupName = "Parameters", Description="If > 0, then trades will only be in the sloping direction of the BigTrend EMA", ResourceType = typeof(Custom.Resource))]
		public int BigTrendPeriod {	get { return pBigTrendPeriod; }	set { pBigTrendPeriod =  Math.Max(0,value); }		}

		private int pZoneSize = 0;
//		[Description("Tick size of the 'zone region' between the two MA's...set to '0' to turn-off zone calculations")]
//		[Category("Parameters")]
//		public int ValidZoneSize
//		{
//			get { return pZoneSize; }
//			set { pZoneSize = Math.Max(0, value); }
//		}
		private bool pShowInwardSignals = true;
//		[Description("Fire alerts when the separation distance between the two MA's becomes less than the zone region")]
//		[Category("Parameters")]
//		public bool ShowInwardSignals
//		{
//			get { return pShowInwardSignals; }
//			set { pShowInwardSignals = value; }
//		}
		private bool pShowOutwardSignals = false;
//		[Description("Fire alerts when the separation distance between the two MA's becomes greater than the zone region")]
//		[Category("Parameters")]
//		public bool ShowOutwardSignals
//		{
//			get { return pShowOutwardSignals; }
//			set { pShowOutwardSignals = value; }
//		}
		private int pFVolatilityPeriod = 9;
//		[Description("Volatility period for Fast VMA, used to calculate the CMO-based volatility index")]
//		[Category("Parameters VMA")]
//		public int VMA1_VolatilityPeriod
//		{
//			get { return pFVolatilityPeriod; }
//			set { pFVolatilityPeriod = Math.Max(1, value); }
//		}
		private int pSVolatilityPeriod = 9;
//		[Description("Volatility period for Slow VMA, used to calculate the CMO-based volatility index")]
//		[Category("Parameters VMA")]
//		public int VMA2_VolatilityPeriod
//		{
//			get { return pSVolatilityPeriod; }
//			set { pSVolatilityPeriod = Math.Max(1, value); }
//		}
		private int pKAMAfast1 = 2;
//		[Description("Number of bars for KAMA MA1 Fast period (between 1 and 125)")]
//		[Category("Parameters KAMA")]
//		public int KAMAfast1
//		{
//			get { return pKAMAfast1; }
//			set { pKAMAfast1 = Math.Min(125, Math.Max(1, value)); }
//		}
		private int pKAMAslow1 = 30;
//		[Description("Number of bars for KAMA MA1 Slow period (between 1 and 125)")]
//		[Category("Parameters KAMA")]
//		public int KAMAslow1
//		{
//			get { return pKAMAslow1; }
//			set { pKAMAslow1 = Math.Min(125, Math.Max(1, value)); }
//		}
		private int pKAMAfast2 = 2;
//		[Description("Number of bars for KAMA MA2 Fast period (between 1 and 125)")]
//		[Category("Parameters KAMA")]
//		public int KAMAfast2
//		{
//			get { return pKAMAfast2; }
//			set { pKAMAfast2 = Math.Min(125, Math.Max(1, value)); }
//		}
		private int pKAMAslow2 = 30;
//		[Description("Number of bars for KAMA MA2 Slow period (between 1 and 125)")]
//		[Category("Parameters KAMA")]
//		public int KAMAslow2
//		{
//			get { return pKAMAslow2; }
//			set { pKAMAslow2 = Math.Min(125, Math.Max(1, value)); }
//		}

		private int pSeparation = 2;
		[Description("Number of ticks between price bar and arrows")]
		[Category("Visual")]
		public int Separation
		{
			get { return pSeparation; }
			set { pSeparation = Math.Max(1, value); }
		}

		private int pMaxEmails = 1;
		[Description("Max number of emails per bar, useful if CalculateOnBarClose = false")]
		[Category("Alert")]
		public int MaxEmails
		{
			get { return pMaxEmails; }
			set { pMaxEmails = Math.Max(1, value); }
		}

		private int pMaxAlerts = 1;
		[Description("Max number of audio signals per bar, useful if CalculateOnBarClose = false")]
		[Category("Audible Alerts")]
		public int MaxAudioAlerts
		{
			get { return pMaxAlerts; }
			set { pMaxAlerts = Math.Max(1, value); }
		}

		private RR_Lines_ColoringBasis pColoringBasis = RR_Lines_ColoringBasis.OnCrossing;
//		[Description("When to color the MA lines?  When the two MA cross, or when the MA lines change their trend direction?")]
//		[Category("Visual")]
//		public RR_Lines_ColoringBasis ColoringBasis
//		{
//			get { return pColoringBasis; }
//			set { pColoringBasis = value; }
//		}

		private string pSoundFileInwardUp = "none";
		[Description("Sound file when micro crosses up through macro - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Audible Alerts")]
		public string RR_LongSound
		{
			get { return pSoundFileInwardUp; }
			set { pSoundFileInwardUp = value; }
		}

		private string pSoundFileInwardDown = "none";
		[Description("Sound file when micro crosses down through macro - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Audible Alerts")]
		public string RR_ShortSound
		{
			get { return pSoundFileInwardDown; }
			set { pSoundFileInwardDown = value; }
		}

		private string pSoundFileOutwardDown = "";
//		[Description("Sound file when fast crosses down through slow - it must exist in your Sounds folder in order to be played")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
//		public string SoundFileOutwardDown
//		{
//			get { return pSoundFileOutwardDown; }
//			set { pSoundFileOutwardDown = value; }
//		}
		private string pSoundFileOutwardUp = "";
//		[Description("Sound file when fast crosses up through slow - it must exist in your Sounds folder in order to be played")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
//		public string SoundFileOutwardUp
//		{
//			get { return pSoundFileOutwardUp; }
//			set { pSoundFileOutwardUp = value; }
//		}

		private RR_Lines_SymbolType pSymbolTypeInward = RR_Lines_SymbolType.Arrows;
		[Description("Symbol to draw on each MA cross, or when their separation distance is less than the zone distance")]
		[Category("Visual")]
		public RR_Lines_SymbolType SymbolType
		{
			get { return pSymbolTypeInward; }
			set { pSymbolTypeInward = value; }
		}

		private RR_Lines_SymbolType pSymbolTypeOutward = RR_Lines_SymbolType.None;
//		[Description("Symbol to draw when the separation between the two MA's is greater than the zone distance")]
//		[Category("Outward Visual (only used when ValidZoneSize > 0)")]
//		public RR_Lines_SymbolType SymbolTypeOutward
//		{
//			get { return pSymbolTypeOutward; }
//			set { pSymbolTypeOutward = value; }
//		}

		private bool pShowMALines = true;
//		[Description("Show the lines drawn by the Moving Averages?")]
//		[Category("Visual")]
//		public bool ShowMAlines
//		{
//			get { return pShowMALines; }
//			set { pShowMALines = value; }
//		}

		#region DirectionFill
		private bool pShowDirectionFill = false;
//		[Description("Fill the background between the two MA's?")]
//		[Category("DirectionFill")]
//		public bool ShowDirectionFill
//		{
//			get { return pShowDirectionFill; }
//			set { pShowDirectionFill = value; }
//		}
		private Brush pMA1overMA2FillBrush = Brushes.Green;
//		[XmlIgnore()]
//		[Description("Fill color when MA1 is above MA2")]
//		[Category("DirectionFill")]
//		public Brush Fill_MA1overMA2{	get { return pMA1overMA2FillBrush; }	set { pMA1overMA2FillBrush = value; }		}
//		[Browsable(false)]
//		public string MA1overMA2FillColorSerialize
//		{	get { return Serialize.BrushToString(pMA1overMA2FillBrush); } set { pMA1overMA2FillBrush = Serialize.StringToBrush(value); }
//		}
		private int pOpacity_MA1overMA2 = 0;
//		[Description("Opacity of the fill background color when MA1 is over MA2, 0=transparent, 10=fully opaque")]
//		[Category("DirectionFill")]
//		public int Opacity_MA1overMA2
//		{
//			get { return pOpacity_MA1overMA2; }
//			set { pOpacity_MA1overMA2 = Math.Max(0,Math.Min(10,value)); }
//		}

		private Brush pMA1underMA2FillBrush = Brushes.Red;
//		[XmlIgnore()]
//		[Description("Fill color when MA1 is below MA2")]
//		[Category("DirectionFill")]
//		public Brush Fill_MA1underMA2{	get { return pMA1underMA2FillBrush; }	set { pMA1underMA2FillBrush = value; }		}
//		[Browsable(false)]
//		public string MA1underMA2FillColorSerialize
//		{	get { return Serialize.BrushToString(pMA1underMA2FillBrush); } set { pMA1underMA2FillBrush = Serialize.StringToBrush(value); }
//		}
		private int pOpacity_MA1underMA2 = 0;
//		[Description("Opacity of the fill background color when MA1 is under MA2, 0=transparent, 10=fully opaque")]
//		[Category("DirectionFill")]
//		public int Opacity_MA1underMA2
//		{
//			get { return pOpacity_MA1underMA2; }
//			set { pOpacity_MA1underMA2 = Math.Max(0,Math.Min(10,value)); }
//		}
		#endregion

		private bool pCrossInwardDownAlertSound = true;
		[Description("Play alert sound when the Fast MA comes crosses the Slow MA inward, from above?")]
		[Category("Audible Alerts")]
		public bool RR_ShortAlertEnabled
		{
			get { return pCrossInwardDownAlertSound; }
			set { pCrossInwardDownAlertSound = value; }
		}
		private bool pCrossInwardUpAlertSound = true;
		[Description("Play alert sound when the Fast MA comes crosses the Slow MA inward, from below?")]
		[Category("Audible Alerts")]
		public bool RR_LongAlertEnabled
		{
			get { return pCrossInwardUpAlertSound; }
			set { pCrossInwardUpAlertSound = value; }
		}

		private bool pCrossOutwardDownAlertSound = false;
//		[Description("Play alert sound when the Fast MA comes crosses the Slow MA outward, from above?")]
//		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
//		public bool CrossOutwardDownAlert
//		{
//			get { return pCrossOutwardDownAlertSound; }
//			set { pCrossOutwardDownAlertSound = value; }
//		}
		private bool pCrossOutwardUpAlertSound = false;
//		[Description("Play alert sound when the Fast MA comes crosses the Slow MA outward, from below?")]
//		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
//		public bool CrossOutwardUpAlert
//		{
//			get { return pCrossOutwardUpAlertSound; }
//			set { pCrossOutwardUpAlertSound = value; }
//		}
		private string pSoundFileStraddleOverMA1 = "";
//		[Description("Sound file when the bar opens below MA1 and closes above MA1")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileStraddleOverMA1
//		{
//			get { return pSoundFileStraddleOverMA1; }
//			set { pSoundFileStraddleOverMA1 = value; }
//		}
		private string pSoundFileStraddleOverMA2 = "";
//		[Description("Sound file when the bar opens below MA1 and closes above MA2")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileStraddleOverMA2
//		{
//			get { return pSoundFileStraddleOverMA2; }
//			set { pSoundFileStraddleOverMA2 = value; }
//		}
		private string pSoundFileStraddleUnderMA1 = "";
//		[Description("Sound file when the bar opens below MA1 and closes below MA1")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileStraddleUnderMA1
//		{
//			get { return pSoundFileStraddleUnderMA1; }
//			set { pSoundFileStraddleUnderMA1 = value; }
//		}
		private string pSoundFileStraddleUnderMA2 = "";
//		[Description("Sound file when the bar opens below MA1 and closes below MA2")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileStraddleUnderMA2
//		{
//			get { return pSoundFileStraddleUnderMA2; }
//			set { pSoundFileStraddleUnderMA2 = value; }
//		}
		private string pSoundFileBarOverMA1 = "";
//		[Description("Sound file when the recently closed bar is completely above MA1")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileBarOverMA1
//		{
//			get { return pSoundFileBarOverMA1; }
//			set { pSoundFileBarOverMA1 = value; }
//		}
		private string pSoundFileBarUnderMA1 = "";
//		[Description("Sound file when the recently closed bar is completely below MA1")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileBarUnderMA1
//		{
//			get { return pSoundFileBarUnderMA1; }
//			set { pSoundFileBarUnderMA1 = value; }
//		}
		private string pSoundFileBarOverMA2 = "";
//		[Description("Sound file when the recently closed bar is completely above MA2")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileBarOverMA2
//		{
//			get { return pSoundFileBarOverMA2; }
//			set { pSoundFileBarOverMA2 = value; }
//		}
		private string pSoundFileBarUnderMA2 = "";
//		[Description("Sound file when the recently closed bar is completely below MA2")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadSoundFileList))]
//		[Category("Straddle Alert")]
//		public string SoundFileBarUnderMA2
//		{
//			get { return pSoundFileBarUnderMA2; }
//			set { pSoundFileBarUnderMA2 = value; }
//		}

		private bool pLaunchPopup = false;
		[Description("Launch a Popup window on a crossing signal?")]
		[Category("Alert")]
		public bool LaunchPopup
		{
			get { return pLaunchPopup; }
			set { pLaunchPopup = value; }
		}
		private string emailaddress = "";
		[Description("Supply your address (e.g. 'you@mail.com') for alerts, leave blank to turn-off emails")]
		[Category("Alert")]
		public string EmailAddress
		{
			get { return emailaddress; }
			set { emailaddress = value; }
		}

//		[Description("Select one of the input data types")]
//		[Category("Parameters")]
//		public RR_Lines_DataType MA1DataType
//		{
//			get { return pMA1DataType; }
//			set { pMA1DataType = value; }
//		}
//		[Description("Select one of the indicator types")]
//		[Category("Parameters")]
//		public RR_Lines_type MA1Type
//		{
//			get { return pMA1Type; }
//			set { pMA1Type = value; }
//		}
//		[Description("Select one of the input data types")]
//		[Category("Parameters")]
//		public RR_Lines_DataType MA2DataType
//		{
//			get { return pMA2DataType; }
//			set { pMA2DataType = value; }
//		}
//		[Description("Select one of the indicator types")]
//		[Category("Parameters")]
//		public RR_Lines_type MA2Type
//		{
//			get { return pMA2Type; }
//			set { pMA2Type = value; }
//		}

		#region Marker Colors
		private Brush pOutwardUpBrush = Brushes.Cyan;
//		[XmlIgnore()]
//		[Description("Color of marker when MA lines move out of their zone and the Fast is over the Slow")]
//		[Category("Outward Visual (only used when ValidZoneSize > 0)")]
//		[Gui.Design.DisplayNameAttribute("Marker Up")]
//		public Brush OutwardUpBrush{	get { return pOutwardUpBrush; }	set { pOutwardUpBrush = value; }		}
//		[Browsable(false)]
//		public string pOutwardUpBrushSerialize
//		{	get { return Serialize.BrushToString(pOutwardUpBrush); } set { pOutwardUpBrush = Serialize.StringToBrush(value); }
//		}

		private Brush pOutwardDownBrush = Brushes.Pink;
//		[XmlIgnore()]
//		[Description("Color of marker when MA lines move out of their zone and the Fast is under the Slow")]
//		[Category("Outward Visual (only used when ValidZoneSize > 0)")]
//		[Gui.Design.DisplayNameAttribute("Marker Down")]
//		public Brush OutwardDownBrush{	get { return pOutwardDownBrush; }	set { pOutwardDownBrush = value; }		}
//		[Browsable(false)]
//		public string pOutwardDownBrushSerialize
//		{	get { return Serialize.BrushToString(pOutwardDownBrush); } set { pOutwardDownBrush = Serialize.StringToBrush(value); }
//		}
		internal class LoadArrowDownTemplates : StringConverter
		{
			#region LoadArrowDownTemplates
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
				string[] paths = new string[4]{NinjaTrader.Core.Globals.UserDataDir,"templates","DrawingTool","ArrowDown"};
				string search = "*.xml";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(System.IO.Path.Combine(paths));
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("Default");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						string name = fi.Name.Replace(".xml",string.Empty);
						if(!list.Contains(name)){
							list.Add(name);
						}
					}
				}
				return new StandardValuesCollection(list.ToArray());
			}
			#endregion
		}
		internal class LoadArrowUpTemplates : StringConverter
		{
			#region LoadArrowUpTemplates
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
				string[] paths = new string[4]{NinjaTrader.Core.Globals.UserDataDir,"templates","DrawingTool","ArrowUp"};
				string search = "*.xml";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(System.IO.Path.Combine(paths));
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("Default");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						string name = fi.Name.Replace(".xml",string.Empty);
						if(!list.Contains(name)){
							list.Add(name);
						}
					}
				}
				return new StandardValuesCollection(list.ToArray());
			}
			#endregion
		}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadArrowUpTemplates))]
		[Display(Name = "Arrow LONG template", GroupName = "Visual")]
		public string pLongArrowTemplateName { get;set; }
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadArrowDownTemplates))]
		[Display(Name = "Arrow SHORT template", GroupName = "Visual")]
		public string pShortArrowTemplateName { get;set; }

		private Brush pMarkerLongBrush = Brushes.PaleGreen;
		[XmlIgnore()]
		[Description("Color of marker when Micro is over the Macro")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Marker Long")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Long",  GroupName = "Visual")]
		public Brush MarkerLongBrush{	get { return pMarkerLongBrush; }	set { pMarkerLongBrush = value; }		}
					[Browsable(false)]
					public string pMarkerLongBrushSerialize
					{	get { return Serialize.BrushToString(pMarkerLongBrush); } set { pMarkerLongBrush = Serialize.StringToBrush(value); }
					}

		private Brush pMarkerShortBrush = Brushes.Salmon;
		[XmlIgnore()]
		[Description("Color of marker when Micro is under the Macro")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Marker Short")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Short",  GroupName = "Visual")]
		public Brush MarkerShortBrush{	get { return pMarkerShortBrush; }	set { pMarkerShortBrush = value; }		}
					[Browsable(false)]
					public string pMarkerShortBrushSerialize
					{	get { return Serialize.BrushToString(pMarkerShortBrush); } set { pMarkerShortBrush = Serialize.StringToBrush(value); }
					}
		#endregion

		[Display(Name="Draw the PnL Lines?", Order=10, GroupName="PnL", Description="Show green and red PnL win/loss lines", ResourceType = typeof(Custom.Resource))]
		public bool pDrawPnLLines {get;set;}
		
		#endregion
	}
}
//	public enum RR_Lines_type
//	{
//	SMA,
//	EMA,
//	WMA,
//	TMA,
//	TEMA,
//	HMA,
//	LinReg,
//	SMMA,
//	ZeroLagEMA,
//	ZeroLagTEMA,
//	ZeroLagHATEMA,
//	KAMA,
//	VWMA,
//	VMA,
//	RMA
//	}
public enum RR_Lines_SymbolType {
	Arrows,Triangles,Diamonds,Squares,Dots,None
}
public enum RR_Lines_DataType {
	Open,High,Low,Close,Median,Typical
}
public enum RR_Lines_ColoringBasis {
	OnTrendChange, OnCrossing, NoColorChange
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RR_Lines[] cacheRR_Lines;
		public RR_Lines RR_Lines(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return RR_Lines(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public RR_Lines RR_Lines(ISeries<double> input, int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			if (cacheRR_Lines != null)
				for (int idx = 0; idx < cacheRR_Lines.Length; idx++)
					if (cacheRR_Lines[idx] != null && cacheRR_Lines[idx].pStartTime == pStartTime && cacheRR_Lines[idx].pStopTime == pStopTime && cacheRR_Lines[idx].pFlatTime == pFlatTime && cacheRR_Lines[idx].pDailyTgtDollars == pDailyTgtDollars && cacheRR_Lines[idx].EqualsInput(input))
						return cacheRR_Lines[idx];
			return CacheIndicator<RR_Lines>(new RR_Lines(){ pStartTime = pStartTime, pStopTime = pStopTime, pFlatTime = pFlatTime, pDailyTgtDollars = pDailyTgtDollars }, input, ref cacheRR_Lines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RR_Lines RR_Lines(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RR_Lines(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public Indicators.RR_Lines RR_Lines(ISeries<double> input , int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RR_Lines(input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RR_Lines RR_Lines(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RR_Lines(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public Indicators.RR_Lines RR_Lines(ISeries<double> input , int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.RR_Lines(input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}
	}
}

#endregion
