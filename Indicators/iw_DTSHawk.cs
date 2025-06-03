//#define MARKET_ANALYZER_VERSION

#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
//using System.Windows.Media.;
//using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Collections.Generic;
using System.Linq;
#endregion
using SBG_HawkDTS;

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{

	public enum Hawk_PermittedDirections {Both, Long, Short, Trend}
	public class DTS_Hawk : Indicator
	{

#if MARKET_ANALYZER_VERSION
		private string ModuleName = "iwDTS";
#else
		private string ModuleName = "iwMicroScalper";
#endif

		private bool LicenseValid = true;
		private const int LONG = 1;
		private const int FLAT = 0;
		private const int SHORT = -1;
		private string NL = Environment.NewLine;

		private SMA	slow;
		private EMA fast;
		private EMA bigtrend;
//		private Series<double> macd;
		private Series<int> PaintBar_Direction;
//		private IntSeries Triangle_Direction, MACD_Dot_Direction, PaintBar_Direction, Trade_Direction;
//		private BoolSeries ResetTriggered;
		private bool EngageTrailingStop = false;
		private int InitialEntryLongBar, InitialEntryShortBar;
		private int SoundBar = -1;
		private int PopupBar = -1;
		private int EmailBar = -1;
		private double pSlopeTicksPerBarForBreakout = 0.1;
		private double PtsPerBarForBreakout = 0;
		private Series<double> position, bar_color, macroline_color;
		private Series<double> MostRecentPosition;
		private bool RunInit = true;
		private IW_Chandelier_Stop chandelier;
		System.Windows.Media.Brush[] bkgBrushes = null;

//		private ChandelierStopClass ChandelierStop;
//		private class ChandelierStopClass {
			#region ChandelierStopClass
//			public bool   Enabled = false;
//			public double RangeMultiple = 3.000;
//			public double Length = 14.000;
//			public double wt=0, avrng1=0;
//			public double hbound=0, lbound=0,hdiff=0,ldiff=0,limit=0;
//			public int Dir = 1;
//			public int Up = 1;
//			public int Dn = -1;
//			private int priorDir = 0;
//			private int priorCurrentBar = 0;
//			private double prioravrng1 = 0;
//			public double TH=0, TL=0, avrng=0, stopdistance=0;
//			public ChandelierStopClass (double rangemultiple, double length){
//				this.RangeMultiple = rangemultiple;
//				this.Length = length;
//				wt = 2.0/(this.Length + 1.0);
//				if(length>0) this.Enabled = true; else this.Dir = 0;
//			}
//			public double Update(int CurrentBar, double H0, double H1, double L0, double L1, double C1, double C0){
//				if(!this.Enabled) return C0;
//				if(CurrentBar == priorCurrentBar) {
//					Dir = priorDir;
//					avrng1 = prioravrng1;
//				} else {
//					priorCurrentBar = CurrentBar;
//					prioravrng1 = avrng1;
//					priorDir = Dir;
//				}
//				if (TH==0 && TL==0) {
//					TH = H0;
//					TL = L0;
//					avrng = avrng1 = (TH+TL)/2;
//					hbound = 999999.0;
//				}
//				else {
//					TH = Math.Max(C1, H0);  // true high
//					TL = Math.Min(C1, L0);   // true low
//					avrng = wt * (TH-TL - avrng1) + avrng1; // EMA of true range
//					avrng1 = avrng; // save last value for next bar
//				}
//				stopdistance = RangeMultiple * avrng;
//				
//				hbound=Math.Min(H0+stopdistance,hbound);
//				lbound=Math.Max(L0-stopdistance,lbound);
//
//				// current value of limit
//				if(Dir==Dn)limit= Math.Min(limit,hbound);
//				if(Dir==Up)limit= Math.Max(limit,lbound);
//				
//				// Check For Breaches of the Stop Line
//				// If breached, Change Direction, resent lbound or hbound, change direction of trend
//				
//				//if(dir==Dn && High[0]>limit) use this condition high > Short SL 
//				if(Dir==Dn && C0>limit)
//				{	// reset lbound
//					lbound=L0-stopdistance;
//					// reset limit
//					limit=lbound;
//					// reset direction
//					Dir=Up;
//				}
//				//if(dir==Up && Low[0]<limit) use this condition Low < Long SL
//				if(Dir==Up && C0<limit)
//				{
//					hbound=H0+stopdistance;
//					limit=hbound;
//					Dir=Dn;
//				}
//
//				return(limit);
//			}
			#endregion
//		}
		
		private bool isBen = false;
		private double PV;
		string SwingLineTrendStr = "";
		private SBG_HawkDTS.TradeManager tm;
		private Account myAccount;
		private DateTime MessageTime = DateTime.MinValue;
		string Order_Reject_Message = string.Empty;
		string Log_Order_Reject_Message = string.Empty;
		string ATMStrategyNamesEmployed = "";
		private NinjaTrader.Gui.Tools.SimpleFont LabelFont = null;
		//v7.1 - TargetBasisStr1 was being initialized to "atr 2.5", it is now corrected to "UseATM"
		//v7.2 - Added "Trade Assist disabled" message if the current time is outside of the Start/End time parameters
		private const string VERSION = "v7.1 Feb.28.2025";
		[Display(Name = "Indicator Version", GroupName = "Indicator Version", Description = "Indicator Version", Order = 0)]
		public string indicatorVersion { get { return VERSION; } }
//=================================================================================================================================================
		bool IsDebug = false;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "iw DTS Hawk";
				IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", ModuleName, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				AddPlot(new Stroke(System.Windows.Media.Brushes.DarkGreen,4), PlotStyle.Dot,         "BuyWarningDot");
				AddPlot(new Stroke(System.Windows.Media.Brushes.DarkGreen,6), PlotStyle.TriangleUp,  "BuyAlert");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Red,4),       PlotStyle.Dot,         "SellWarningDot");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Red,6),       PlotStyle.TriangleDown,"SellAlert");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Green,4),     PlotStyle.Hash,        "BuyEntryPoint");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Red,4),       PlotStyle.Hash,        "SellEntryPoint");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Blue,1),      PlotStyle.Line,        "MicroTrendLine");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Blue,4),        PlotStyle.TriangleRight, "MacroTrendLine");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Transparent,1), PlotStyle.Hash,      "FilterLevel");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Yellow,1), PlotStyle.Line,      "BigTrend");
				IsOverlay=true;
				pATMStrategyName1 = "";
				SelectedAccountName = "";
				pStartTime		= 930;
				pStopTime		= 1550;
				pFlatTime		= 1650;
				pPermittedDOW	= "1,2,3,4,5";
				pShowPnLStats = false;
				//pTargetDistStr1 = "atr 2.5";
				//pStoplossDistStr1 = "UseATM";
				pDailyTgtDollars = 1000;
				pPermittedDirection = Hawk_PermittedDirections.Both;
			}
			else if (State == State.Configure){
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
				tm = new SBG_HawkDTS.TradeManager(this, "Hawk", "", "","", Instrument, s, pStartTime, pStopTime, pStopTime, pShowHrOfDayPnL, int.MaxValue, int.MaxValue);
				tm.WarnIfWrongDayOfWeek = false;//do not print the warning message if the current day isn't in the Permitted DOW parameter
				#region -- SLTP for Signal --
				if(pTargetDistStr1.Trim() != "" && pStoplossDistStr1.Trim() != "")
					tm.SLTPs["1"] = new TradeManager.SLTPinfo(pStoplossDistStr1, pTargetDistStr1, pATMStrategyName1);
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
				position = new Series<double>(this);
				MostRecentPosition = new Series<double>(this,MaximumBarsLookBack.Infinite);
				bar_color = new Series<double>(this);
				macroline_color = new Series<double>(this);
				PaintBar_Direction = new Series<int>(this);
				Calculate=Calculate.OnPriceChange;
				fast = EMA(pFastEMA);
				fast.Calculate = Calculate.OnBarClose;
				slow = SMA(pSlowSMA);
				slow.Calculate = Calculate.OnBarClose;
				if(pBigTrendPeriod>0)
					bigtrend = EMA(pBigTrendPeriod);
				//ChandelierStop = new ChandelierStopClass(pChandelierRangeMultiple, pChandelierLength);
				if(pChandelierLength>0){
					chandelier = IW_Chandelier_Stop(this.pChandelierRangeMultiple, this.pChandelierLength);
					chandelier.Calculate = Calculate.OnPriceChange;
				}
				PtsPerBarForBreakout = TickSize * pSlopeTicksPerBarForBreakout;
				hawkMA18 = IW_HawkMA(18);
				hawkMAMicro = IW_HawkMA(pMicroTLperiod);

				var br1 = Plots[4].Brush.Clone();
				br1.Opacity = pBackgroundOpacity/100.0;
				br1.Freeze();
				var br2 = Plots[5].Brush.Clone();
				br2.Opacity = pBackgroundOpacity/100.0;
				br2.Freeze();
				
				bkgBrushes = new System.Windows.Media.Brush[2]{br1.Clone(),br2.Clone()};
				bkgBrushes[0].Freeze();
				bkgBrushes[1].Freeze();

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
					string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "HawkTradeAssist.log");
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
					if(ts.TotalDays>5) System.IO.File.Delete("HawkTradeAssist.Log");
					else if(ts.TotalMinutes > 10){
						string msg = "ERROR = account '"+SelectedAccountName+"' is not available to trade";
						Log(msg, LogLevel.Alert);
						System.IO.File.AppendAllText("HawkTradeAssist.Log", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
					}
				}
				ATMStrategyNamesEmployed = $"#1:  '{pATMStrategyName1}'  Qty: {pSignal1Qty}";
				#endregion
				Log_Order_Reject_Message = "Your order was rejected.  Account Name '"+SelectedAccountName+"' may not be available on this datafeed connection.  Check for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				Order_Reject_Message = "Your order was rejected.\nAccount Name '"+SelectedAccountName+"' may not be available on this datafeed connection\nCheck for typo errors, or consider changing it to 'Sim101' or 'Replay101'";

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
        private string toolbarname = "Hawk_TB", uID;
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
						if(pPermittedDirection == Hawk_PermittedDirections.Both) pPermittedDirection = Hawk_PermittedDirections.Short;
						else if(pPermittedDirection == Hawk_PermittedDirections.Short) pPermittedDirection = Hawk_PermittedDirections.Long;
						else if(pPermittedDirection == Hawk_PermittedDirections.Long) pPermittedDirection = Hawk_PermittedDirections.Trend;
						else if(pPermittedDirection == Hawk_PermittedDirections.Trend) pPermittedDirection = Hawk_PermittedDirections.Both;
                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(pPermittedDirection == Hawk_PermittedDirections.Both) pPermittedDirection = Hawk_PermittedDirections.Short;
							else if(pPermittedDirection == Hawk_PermittedDirections.Short) pPermittedDirection = Hawk_PermittedDirections.Long;
							else if(pPermittedDirection == Hawk_PermittedDirections.Long) pPermittedDirection = Hawk_PermittedDirections.Trend;
							else if(pPermittedDirection == Hawk_PermittedDirections.Trend) pPermittedDirection = Hawk_PermittedDirections.Both;
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
						if(pPermittedDirection == Hawk_PermittedDirections.Both) pPermittedDirection = Hawk_PermittedDirections.Short;
						else if(pPermittedDirection == Hawk_PermittedDirections.Short) pPermittedDirection = Hawk_PermittedDirections.Long;
						else if(pPermittedDirection == Hawk_PermittedDirections.Long) pPermittedDirection = Hawk_PermittedDirections.Trend;
						else if(pPermittedDirection == Hawk_PermittedDirections.Trend) pPermittedDirection = Hawk_PermittedDirections.Both;
                        miDirection.Header = "Direction: " + pPermittedDirection.ToString().ToUpper();
						InformUserAboutRecalculation();
                        ForceRefresh();
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
							if(pPermittedDirection == Hawk_PermittedDirections.Both) pPermittedDirection = Hawk_PermittedDirections.Short;
							else if(pPermittedDirection == Hawk_PermittedDirections.Short) pPermittedDirection = Hawk_PermittedDirections.Long;
							else if(pPermittedDirection == Hawk_PermittedDirections.Long) pPermittedDirection = Hawk_PermittedDirections.Trend;
							else if(pPermittedDirection == Hawk_PermittedDirections.Trend) pPermittedDirection = Hawk_PermittedDirections.Both;
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
					Print("Signal: "+tm.SLTPs["1"].ToString());
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
        void BuyUsingATM(string pATMStrategyName, int Qty, string SignalID, bool verbose = false)
        {
			if(pPermittedDirection == Hawk_PermittedDirections.Trend && SwingLineTrendStr != "UP") return;//buy signal is not permitted against down trend
			if(tm.DailyTgtAchieved) {
				Print(Times[0][0].ToShortDateString()+"  Daily PnL target achieved, no buy order submitted");
				return;
			}

			var ordertype   = OrderType.Market;
try{
			if((pPermittedDirection == Hawk_PermittedDirections.Long || pPermittedDirection == Hawk_PermittedDirections.Both || pPermittedDirection == Hawk_PermittedDirections.Trend)){
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
					if(verbose) Print($"------ {Times[0][0].ToString()} Now Long from {EntryPrice}, Tgt: {TgtPrice}, SL {SLPrice}");
					tm.AddTrade('L', Qty, EntryPrice, Times[0][0], SLPrice, TgtPrice);
				}
				EntryABar = CurrentBar;
				if(pATMStrategyName.Trim().Length==0 || Qty==0) return;
				if(State != State.Realtime) return;
				if(SelectedAccountName.Trim().Length == 0) return;
				if(!LongsPermittedNow) return;
				if(!IsAlwaysActive && IsTradingPaused) return;
				IsTradingPaused = true;
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
        void SellUsingATM(string pATMStrategyName, int Qty, string SignalID, bool verbose = false)
        {
			if(pPermittedDirection == Hawk_PermittedDirections.Trend && SwingLineTrendStr != "DOWN") return;//sell signal is not permitted against up trend
			if(tm.DailyTgtAchieved) {
				Print(Times[0][0].ToShortDateString()+"  Daily PnL target achieved, no sell order submitted");
				return;
			}

			var ordertype   = OrderType.Market;
try{
			if((pPermittedDirection == Hawk_PermittedDirections.Short || pPermittedDirection == Hawk_PermittedDirections.Both || pPermittedDirection == Hawk_PermittedDirections.Trend)){
				var position_size = GetCurrentMarketPosition();
				if(position_size > 0){//we're long, reverse to short requested...exit out the longs
					myAccount.Flatten(Instruments);
					tm.Note = "Reversing";
					tm.GoFlat(Times[0][0], Closes[0][0]);
				}
				var ShortsPermittedNow = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);
				if(State==State.Historical && ShortsPermittedNow && tm.SLTPs.ContainsKey(SignalID)){
					double EntryPrice = Closes[0][0];// - TickSize;
					double TgtPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice - (tm.SLTPs[SignalID].ATRmultTarget > 0 ? tm.SLTPs[SignalID].ATRmultTarget * atr("points", 14) : tm.SLTPs[SignalID].DollarsTarget / PV));
					double SLPrice = Instrument.MasterInstrument.RoundToTickSize(EntryPrice + (tm.SLTPs[SignalID].ATRmultSL > 0 ? tm.SLTPs[SignalID].ATRmultSL * atr("points", 14) : tm.SLTPs[SignalID].DollarsSL / PV));
					Draw.Dot(this,$"tgt{CurrentBars[0]}", false,0, TgtPrice, System.Windows.Media.Brushes.LimeGreen);
					Draw.Dot(this,$"sl{CurrentBars[0]}", false, 0, SLPrice, System.Windows.Media.Brushes.Magenta);
					if(verbose) Print($"------ {Times[0][0].ToString()} Now Short from {EntryPrice}, Tgt: {TgtPrice}, SL {SLPrice}");
					tm.AddTrade('S', Qty, EntryPrice, Times[0][0], SLPrice, TgtPrice, verbose);
				}
				EntryABar = CurrentBar;
				if(pATMStrategyName.Trim().Length==0 || Qty==0) return;
				if(State != State.Realtime) return;
				if(SelectedAccountName.Trim().Length == 0) return;
				if(!ShortsPermittedNow) return;
				if(!IsAlwaysActive && IsTradingPaused) return;
				IsTradingPaused = true;
				var ATMorder = myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.SellShort, 
								ordertype, 
								OrderEntry.Automated, 
								TimeInForce.Day, Qty, 0, 0, string.Empty, "Entry", Core.Globals.MaxDate, null); 
				NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName.Trim(), ATMorder);
			}
if(verbose)Print($"  ...leaving SellUsing");
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

		bool LongsPermittedNow = false;
		bool ShortsPermittedNow = false;
		IW_HawkMA hawkMA18, hawkMAMicro;
		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;

			if(CurrentBar<5) {
				macroline_color[0] = (0);
				bar_color[0] = (0);
				return;
			}
			pResetPlots = false;
			if(IsFirstTickOfBar || pResetPlots){
				BuyWarningDot.Reset();
				BuyAlert.Reset();
				SellWarningDot.Reset();
				SellAlert.Reset();
			}
			MicroTrendLine[0] = hawkMAMicro[0];
			if(MicroTrendLine[0]>MicroTrendLine[1]) PlotBrushes[6][0] = pTrendlineUpBrush; else PlotBrushes[6][0] = pTrendlineDownBrush;
			if(ChandelierLength>0) {
//			if(ChandelierStop.Enabled) {
				//FilterLevel[0] = (ChandelierStop.Update(CurrentBar, High[0], High[1], Low[0], Low[1], Close[1], Close[0]));
				if(chandelier.Dir[0]>0)
					FilterLevel[0]=(chandelier.ChandelierLo[0]);
				else
					FilterLevel[0]=(chandelier.ChandelierHi[0]);
			}

			#region PaintBars
				if(Low[0] > slow[1]) PaintBar_Direction[0]=(LONG);
				else if(High[0] < slow[1]) PaintBar_Direction[0]=(SHORT);
				else PaintBar_Direction[0]=(FLAT);
				int barcolor = FLAT;//pNeutralBrush;
				bar_color[0] = FLAT;
				if (PaintBar_Direction[0] > 0){ //up color
					barcolor = LONG;
					bar_color[0]=(1);
				}
				else if (PaintBar_Direction[0] < 0){ //down color
					barcolor  = SHORT; 
					bar_color[0]=(-1);
				}
//Print("slow[0]: "+slow[1]+"  barcolor: "+barcolor);
			if(ChartControl!=null) {
				if(this.pUpBrush!=System.Windows.Media.Brushes.Transparent && this.pDownBrush!=System.Windows.Media.Brushes.Transparent){
					bool IsUpBar = Close[0]>Open[0];
					if(Bars.BarsType.DefaultChartStyle == ChartStyleType.CandleStick) {
						if (IsUpBar) {
							CandleOutlineBrush  = barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush); 
							if(Close[0]<=Open[0]) BarBrush = barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush);
							else BarBrush = pHollowUpBars?System.Windows.Media.Brushes.Transparent : (barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush));
						}
						else { //down color
							CandleOutlineBrush = barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush);
							if(Close[0]<=Open[0]) BarBrush = barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush);
							else BarBrush = pHollowUpBars?System.Windows.Media.Brushes.Transparent : (barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush));
						}
					} else {
						BarBrush  = barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush); 
						CandleOutlineBrush = barcolor == LONG ? pUpBrush : (barcolor == SHORT ? pDownBrush : pNeutralBrush);
					}
				}
			}
			#endregion
			
			#region MacroTrendLine
			MacroTrendLine[0]=(hawkMA18[0]);
			if(MacroTrendLine[0] > MacroTrendLine[1]) {
				PlotBrushes[MACRO_TRENDLINE_ID][0] = pMacroRisingBrush;
				macroline_color[0]=(1);
//				Direction[0]=(LONG);
			}
			else if(MacroTrendLine[0] < MacroTrendLine[1]) {
				PlotBrushes[MACRO_TRENDLINE_ID][0] = pMacroFallingBrush;
				macroline_color[0]=(-1);
//				Direction[0]=(SHORT);
			} else {
				PlotBrushes[MACRO_TRENDLINE_ID][0] = PlotBrushes[MACRO_TRENDLINE_ID][1];
				macroline_color[0]=(macroline_color[1]);
			}

			if(pSlopeTicksPerBarForBreakout > 0) {
				double slope =(MacroTrendLine[0]-MacroTrendLine[1]);
				if(slope <= PtsPerBarForBreakout && slope >= -PtsPerBarForBreakout) {
					PlotBrushes[MACRO_TRENDLINE_ID][0] = pMacroRangingBrush;
					macroline_color[0]=(0);
				}
			}
			#endregion

			bool BarCloseUp1 = Open[1]<Close[1];
			bool BarCloseDn1 = Open[1]>Close[1];
			if(!pConservativeMode) { //permit doji candles to be part of the setup signal, this is a more aggressive signal
				BarCloseUp1 = BarCloseUp1 || Open[1]==Close[1];
				BarCloseDn1 = BarCloseDn1 || Open[1]==Close[1];
			}
			#region -- Determine Trend for TradeAssist --
			if(pBigTrendPeriod>0){
				BigTrend[0] = bigtrend[0];
				if(BigTrend[0] > BigTrend[1]) SwingLineTrendStr = "UP";
				else if(BigTrend[0] < BigTrend[1]) SwingLineTrendStr = "DOWN";
			}
			#endregion

			bool PermitUpTriangles = true;
			if(fast[1] < slow[1] || (SwingLineTrendStr.Length>0 && SwingLineTrendStr != "UP")) {
				PermitUpTriangles = false;
				BuyWarningDot.Reset(1);
				BuyAlert.Reset(0);
			}
			bool PermitDownTriangles = true;
			if(fast[1] > slow[1] || (SwingLineTrendStr.Length>0 && SwingLineTrendStr != "DOWN")) {
				PermitDownTriangles = false;
				SellWarningDot.Reset(1);
				SellAlert.Reset(0);
			}
			if(this.pChandelierLength>0){
				if(chandelier.Dir[0] > 0 && PermitDownTriangles) PermitDownTriangles = false;
				if(chandelier.Dir[0] < 0 && PermitUpTriangles) PermitUpTriangles = false;
			}
			#region -- Determine entry conditions for TradeAssist --
			var t1   = ToTime(Times[0][1])/100;
			var t    = ToTime(Times[0][0])/100;
			var dow1 = Times[0][1].DayOfWeek;
			var dow  = Times[0][0].DayOfWeek;
			var date = Times[0][0].Date;
			tm.PrintResults(BarsArray[0].Count, CurrentBars[0], true, this);
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
			#endregion

			if(PermitUpTriangles && BarCloseDn1) {
				BuyWarningDot[1]=(High[1]+TickSize);
				if(pEnableSounds && this.SoundBar!=CurrentBar) {
					Alert(CurrentBar.ToString(),NinjaTrader.NinjaScript.Priority.High,"DTS_Hawk potential BUY established",AddSoundFolder(this.pUpTriangleSound),1,System.Windows.Media.Brushes.Lime,System.Windows.Media.Brushes.Black);  
					SoundBar = CurrentBar;
				}
				if(pLaunchPopupOnSetup && this.PopupBar!=CurrentBar && !(State == State.Historical)) {
					PopupBar = CurrentBar;
					Log("DTS_Hawk potential BUY established on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(pEnableEmails && pEmailAlertOnSetup.Length>0 && this.EmailBar!=CurrentBar) {
					EmailBar = CurrentBar;
					SendMail(pEmailAlertOnSetup,"DTS_Hawk potential BUY on "+Instrument.FullName,"DTS_Hawk potential BUY forming at "+SellEntryPoint[1].ToString()+NL+" on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
				}
			}
			if(PermitDownTriangles && BarCloseUp1) {
				SellWarningDot[1]=(Low[1]-TickSize);
				if(pEnableSounds && this.SoundBar!=CurrentBar) {
					Alert(CurrentBar.ToString(),NinjaTrader.NinjaScript.Priority.High,"DTS_Hawk potential SELL established",AddSoundFolder(this.pDownTriangleSound),1,System.Windows.Media.Brushes.Red,System.Windows.Media.Brushes.White); 
					SoundBar = CurrentBar;
				}
				if(pLaunchPopupOnSetup && this.PopupBar!=CurrentBar && !(State == State.Historical)) {
					PopupBar = CurrentBar;
					Log("DTS_Hawk potential SELL established on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(pEnableEmails && pEmailAlertOnSetup.Length>0 && this.EmailBar!=CurrentBar) {
					EmailBar = CurrentBar;
					SendMail(pEmailAlertOnSetup,"DTS_Hawk potential SELL on "+Instrument.FullName,"DTS_Hawk potential SELL forming at "+SellEntryPoint[1].ToString()+NL+" on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
				}
			}
			if(BuyWarningDot.IsValidDataPoint(2) && BarCloseUp1){
				BuyAlert[1]=(Low[1]-TickSize);
				BuyEntryPoint[1] = (High[1]+TickSize);
			}
			if(SellWarningDot.IsValidDataPoint(2) && BarCloseDn1){
				SellAlert[1]=(High[1]+TickSize);
				SellEntryPoint[1]=(Low[1]-TickSize);
			}
			if(!BuyAlert.IsValidDataPoint(2)  && BuyWarningDot.IsValidDataPoint(3))  {if(pViewMissedSetups) PlotBrushes[0][3]=System.Windows.Media.Brushes.DimGray; else BuyWarningDot.Reset(3);}
			if(!SellAlert.IsValidDataPoint(2) && SellWarningDot.IsValidDataPoint(3)) {if(pViewMissedSetups) PlotBrushes[2][3]=System.Windows.Media.Brushes.DimGray; else SellWarningDot.Reset(3);}
			string tag = string.Concat("Hawkarrow "+CurrentBar);
			if(BuyEntryPoint.IsValidDataPoint(1) && High[0]>=BuyEntryPoint[1]) {
				var setup = BuyEntryPoint.IsValidDataPoint(1) && !BuyEntryPoint.IsValidDataPoint(2);
				if(ChartControl!=null && setup){
					if(pBackgroundOpacity>0)
						BackBrush = bkgBrushes[0];
					if(pArrowLocation == DTS_Hawk_ArrowLocation.OffBar)       Draw.ArrowUp(this, tag, false, 0, Low[0]-TickSize*pSeparation, System.Windows.Media.Brushes.Green);
					if(pArrowLocation == DTS_Hawk_ArrowLocation.AtEntryPrice) Draw.ArrowUp(this, tag, false, 0, BuyEntryPoint[1], System.Windows.Media.Brushes.Green);
				}
				position[0] = (LONG);
				if(pSignal1Qty>0 && EntryABar != CurrentBar && setup) BuyUsingATM(pATMStrategyName1, pSignal1Qty, "1");

				if(pEnableSounds && this.SoundBar!=CurrentBar) {
					Alert(CurrentBar.ToString(),NinjaTrader.NinjaScript.Priority.High,"DTS_Hawk LONG", AddSoundFolder(this.pBuySound),1,System.Windows.Media.Brushes.Lime,System.Windows.Media.Brushes.Black); 
					SoundBar = CurrentBar;
				}
				if(pLaunchPopupOnEntryPrice && this.PopupBar!=CurrentBar && !(State == State.Historical)) {
					PopupBar = CurrentBar;
					Log("DTS_Hawk LONG at "+BuyEntryPoint[1].ToString()+NL+" on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(pEnableEmails && pEmailAlertOnEntryPrice.Length>0 && this.EmailBar!=CurrentBar) {
					EmailBar = CurrentBar;
					SendMail(pEmailAlertOnEntryPrice,"DTS_Hawk LONG on "+Instrument.FullName,"DTS_Hawk LONG at "+SellEntryPoint[1].ToString()+NL+" on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
				}
			}
			else if(SellEntryPoint.IsValidDataPoint(1) && Low[0]<=SellEntryPoint[1]) {
				var setup = SellEntryPoint.IsValidDataPoint(1) && !SellEntryPoint.IsValidDataPoint(2);
				if(ChartControl!=null && setup){
					if(pBackgroundOpacity>0)
						BackBrush = bkgBrushes[1];
					if(pArrowLocation == DTS_Hawk_ArrowLocation.OffBar)       Draw.ArrowDown(this, tag, false, 0, High[0]+TickSize*pSeparation, System.Windows.Media.Brushes.Red);
					if(pArrowLocation == DTS_Hawk_ArrowLocation.AtEntryPrice) Draw.ArrowDown(this, tag, false, 0, SellEntryPoint[1], System.Windows.Media.Brushes.Red);
				}
				position[0] = (SHORT);
				if(pSignal1Qty>0 && EntryABar != CurrentBar && setup) SellUsingATM(pATMStrategyName1, pSignal1Qty, "1");

				if(pEnableSounds && this.SoundBar!=CurrentBar) {
					Alert(CurrentBar.ToString(),NinjaTrader.NinjaScript.Priority.High,"DTS_Hawk Mkt SHORT",AddSoundFolder(this.pSellSound),1,System.Windows.Media.Brushes.Red,System.Windows.Media.Brushes.White);  
					SoundBar = CurrentBar;
				}
				if(pLaunchPopupOnEntryPrice && this.PopupBar!=CurrentBar && !(State == State.Historical)) {
					PopupBar = CurrentBar;
					Log("DTS_Hawk SHORT at "+SellEntryPoint[1].ToString()+NL+" on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString(),NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(pEnableEmails && pEmailAlertOnEntryPrice.Length>0 && this.EmailBar!=CurrentBar) {
					EmailBar = CurrentBar;
					SendMail(pEmailAlertOnEntryPrice,"DTS_Hawk SHORT on "+Instrument.FullName,"DTS_Hawk SHORT at "+SellEntryPoint[1].ToString()+NL+" on "+Instrument.FullName+" "+BarsArray[0].BarsPeriod.ToString());
				}
			}
			else RemoveDrawObject(tag);

			//if(ChartControl == null){//this section is only for Bloodhound compatibility - not for chart reading
//Print("it's null");
				if(BuyAlert.IsValidDataPoint(1) && BuyEntryPoint.IsValidDataPoint(1)) {
					BuyEntryPoint[0] = (BuyEntryPoint[1]);
					//Print("BuyEntryPoint set "+Time[0].ToString());
				}
				if(SellAlert.IsValidDataPoint(1) && SellEntryPoint.IsValidDataPoint(1)) {
					SellEntryPoint[0] = (SellEntryPoint[1]);
				}
			//}

			if(position[0]==LONG) MostRecentPosition[0] = (LONG);
			else if(position[0]==SHORT) MostRecentPosition[0] = (SHORT);
			else if(CurrentBar>1) MostRecentPosition[0] = (MostRecentPosition[1]);
			//if(BuyEntryPoint.IsValidDataPoint(0)) Print(CurrentBar.ToString()+"  BuyEntryPoint contains "+BuyEntryPoint[0].ToString());

		}
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds", wav);
		}
//====================================================================

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
			else if(!LongsPermittedNow && !ShortsPermittedNow)
				msg = $"TradeAssist disabled:  {DateTime.Now.ToShortTimeString()} is outside of your Start/End time setting";
			else if(pBigTrendPeriod<=0 && pPermittedDirection==Hawk_PermittedDirections.Trend) msg = "To use 'TREND' filter, you must set 'Big Trend Period' to a non-zero number";
			else{
				ATMStrategyNamesEmployed = $"#1:  '{pATMStrategyName1}'  Qty: {pSignal1Qty}";
				msg = (!IsAlwaysActive && IsTradingPaused) ? "TRADING IS PAUSED" : $"{(pPermittedDirection==Hawk_PermittedDirections.Both? "Long and Shorts are" : (pPermittedDirection == Hawk_PermittedDirections.Long ? "Long ONLY is": (pPermittedDirection == Hawk_PermittedDirections.Short? "Short ONLY is":"with TREND is")))} {(IsAlwaysActive ? "ALWAYS ":"")}active on '{SelectedAccountName}' with\n{ATMStrategyNamesEmployed}";
				if(pPermittedDirection == Hawk_PermittedDirections.Trend)
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


		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Position
		{
			get 
			{ 
				Update();
				return MostRecentPosition; 
			}
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Bar_Color
		{
			get 
			{ 
				Update();
				return bar_color; 
			}
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Macroline_Color
		{
			get 
			{ 
				Update();
				return macroline_color; 
			}
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
				System.IO.FileInfo[] filCustom = dirCustom.GetFiles(search);

				string[] list = new string[filCustom.Length];
				int i = 0;
				foreach (System.IO.FileInfo fi in filCustom)
				{
//					if(fi.Extension.ToLower().CompareTo(".exe")!=0 && fi.Extension.ToLower().CompareTo(".txt")!=0){
					list[i] = fi.Name;
					i++;
//					}
				}
				string[] filteredlist = new string[i];
				for(i = 0; i<filteredlist.Length; i++) filteredlist[i] = list[i];
				return new StandardValuesCollection(filteredlist);
			}
			#endregion
        }

		#region Plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BuyWarningDot {get { return Values[0]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BuyAlert {get { return Values[1]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SellWarningDot {get { return Values[2]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SellAlert {get { return Values[3]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BuyEntryPoint {get { return Values[4]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SellEntryPoint {get { return Values[5]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MicroTrendLine {get { return Values[6]; }}
		private const int MACRO_TRENDLINE_ID = 7;
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MacroTrendLine {get { return Values[7]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> FilterLevel {get { return Values[8]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BigTrend {get { return Values[9]; }}
		#endregion
		private bool pResetPlots = false;
//		[Description("Reset plots on every tick?")]
//		[Category("Reset")]
//		public bool ResetPlots{	get { return pResetPlots; }	set { pResetPlots = value; }		}

		#region Properties
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
		public Hawk_PermittedDirections pPermittedDirection {get;set;}

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
        private System.Windows.Media.Brush pButtonForeground = System.Windows.Media.Brushes.Brown;
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
        private System.Windows.Media.Brush pButtonBackground = System.Windows.Media.Brushes.Black;
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
		private string pButtonText = "Hawk";
		[Display(Order=30, Name="Button Text", GroupName="UI Pulldown")]
		public string ButtonText
		{
			get { return pButtonText; }
			set { pButtonText = value; }
		}
		#endregion

		private int pChandelierLength = 0;
		[Display(Order = 10, Name = "Chandelier Length",  GroupName = "Filter", Description="Optional Chandelier period, set to '0' to turn-off the filter", ResourceType = typeof(Custom.Resource))]
		public int ChandelierLength{	get { return pChandelierLength; }	set { pChandelierLength = Math.Max(0,value); }		}

		private double pChandelierRangeMultiple = 3.0;
		[Display(Order = 20, Name = "Chandelier Range Mult",  GroupName = "Filter", Description="Optional Chandelier range multiple", ResourceType = typeof(Custom.Resource))]
		public double ChandelierRangeMultiple{	get { return pChandelierRangeMultiple; }	set { pChandelierRangeMultiple = Math.Max(0,value); }		}

		private int pFastEMA = 14;
//		[Description("Period for the Fast EMA")]
//		[Category("Parameters")]
//		public int FastEMAperiod{	get { return pFastEMA; }	set { pFastEMA = Math.Max(1,value); }		}
		private int pSlowSMA = 55;
//		[Description("Period for the Slow SMA")]
//		[Category("Parameters")]
//		public int SlowSMAperiod{	get { return pSlowSMA; }	set { pSlowSMA = Math.Max(1,value); }		}
		private int pMicroTLperiod = 7;
//		[Description("Period of Micro Trend Line")]
//		[Category("Parameters")]
//		public int MicroTLperiod{	get { return pMicroTLperiod; }	set { pMicroTLperiod = value; }		}

		private bool pConservativeMode = true; //Conservative means doji (indecision) candles invalidate the setup
		[Display(Order = 30, Name = "Conservative Mode",  GroupName = "Filter", Description="True means signals are more conservative, False means the signals are more numerous", ResourceType = typeof(Custom.Resource))]
		public bool ConservativeMode{	get { return pConservativeMode; }	set { pConservativeMode = value; }		}

		private int pBigTrendPeriod = 50; //Used to filter out counter trend trades
		[Display(Order = 40, Name = "Big Trend Period",  GroupName = "Filter", Description="If > 0, then trades will only be in the sloping direction of the BigTrend EMA", ResourceType = typeof(Custom.Resource))]
		public int BigTrendPeriod {	get { return pBigTrendPeriod; }	set { pBigTrendPeriod =  Math.Max(0,value); }		}

		private bool pViewMissedSetups = false;

		private int pSeparation = 1;
		[Description("Distance between arrows and price bar in ticks")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Arrow Separation",  GroupName = "Visual Signal")]
		public int Separation{	get { return pSeparation; }	set { pSeparation = value; }		}

		private DTS_Hawk_ArrowLocation pArrowLocation = DTS_Hawk_ArrowLocation.OffBar;
		[Description("Location of arrow, OffBar = not on the bar, but away from it.  AtEntryPrice = the price of the suggested entry")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Arrow Location",  GroupName = "Visual Signal")]
		public DTS_Hawk_ArrowLocation ArrowLocation{	get { return pArrowLocation; }	set { pArrowLocation = value; }		}

		int pBackgroundOpacity = 20;
		[Description("Colorize the background of the signal bar?  Enter '0' to turn-off")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Background Opacity",  GroupName = "Visual Signal")]
		public int BackgroundOpacity
		{
			get { return pBackgroundOpacity; }
			set { pBackgroundOpacity = Math.Max(0, Math.Min(100,value)); }
		}
		private System.Windows.Media.Brush pMacroRisingBrush    = System.Windows.Media.Brushes.Green;
		private System.Windows.Media.Brush pMacroFallingBrush  = System.Windows.Media.Brushes.Red;
		private System.Windows.Media.Brush pMacroRangingBrush = System.Windows.Media.Brushes.Gray;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rising",  GroupName = "Macro Trendline Coloring")]
		public System.Windows.Media.Brush MacroRisingBrush{	get { return pMacroRisingBrush; }	set { pMacroRisingBrush = value; }		}
		[Browsable(false)]
		public string MacroTLRClSerialize
		{	get { return Serialize.BrushToString(pMacroRisingBrush); } set { pMacroRisingBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Falling",  GroupName = "Macro Trendline Coloring")]
		public System.Windows.Media.Brush MacroFallingBrush{	get { return pMacroFallingBrush; }	set { pMacroFallingBrush = value; }		}
		[Browsable(false)]
		public string MacroTLFClSerialize
		{	get { return Serialize.BrushToString(pMacroFallingBrush); } set { pMacroFallingBrush = Serialize.StringToBrush(value); }
		}
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Ranging",  GroupName = "Macro Trendline Coloring")]
		public System.Windows.Media.Brush MacroRangingBrush{	get { return pMacroRangingBrush; }	set { pMacroRangingBrush = value; }		}
		[Browsable(false)]
		public string MacroTLRangingClSerialize
		{	get { return Serialize.BrushToString(pMacroRangingBrush); } set { pMacroRangingBrush = Serialize.StringToBrush(value); }
		}

		#region Trend Colors
		private System.Windows.Media.Brush pTrendlineUpBrush = System.Windows.Media.Brushes.Green;
		private System.Windows.Media.Brush pTrendlineDownBrush = System.Windows.Media.Brushes.Red;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Trendline Coloring")]
// [Gui.Design.DisplayNameAttribute("Rising")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rising",  GroupName = "Trendline Coloring")]
		public System.Windows.Media.Brush TrendlineUpBrush{	get { return pTrendlineUpBrush; }	set { pTrendlineUpBrush = value; }		}
					[Browsable(false)]
					public string TLRClSerialize
					{	get { return Serialize.BrushToString(pTrendlineUpBrush); } set { pTrendlineUpBrush = Serialize.StringToBrush(value); }
					}
		[XmlIgnore()]
		[Description("")]
// 		[Category("Trendline Coloring")]
// [Gui.Design.DisplayNameAttribute("Falling")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Falling",  GroupName = "Trendline Coloring")]
		public System.Windows.Media.Brush TrendlineDownBrush{	get { return pTrendlineDownBrush; }	set { pTrendlineDownBrush = value; }		}
					[Browsable(false)]
					public string TLFClSerialize
					{	get { return Serialize.BrushToString(pTrendlineDownBrush); } set { pTrendlineDownBrush = Serialize.StringToBrush(value); }
					}
		#endregion

		#region PaintBar Coloring
		private System.Windows.Media.Brush pUpBrush = System.Windows.Media.Brushes.LimeGreen;
		private System.Windows.Media.Brush pDownBrush = System.Windows.Media.Brushes.Red;
		private System.Windows.Media.Brush pNeutralBrush = System.Windows.Media.Brushes.Goldenrod;

		[XmlIgnore()]
		[Description("")]
// 		[Category("Bar Coloring")]
// [Gui.Design.DisplayNameAttribute("No Direction")]
		[Display(Order = 20, Name = "No Direction",  GroupName = "Bar Coloring", ResourceType = typeof(Custom.Resource))]
		public System.Windows.Media.Brush NeutralBrush{	get { return pNeutralBrush; }	set { pNeutralBrush = value; }		}
				[Browsable(false)]
				public string NClSerialize
				{	get { return Serialize.BrushToString(pNeutralBrush); } set { pNeutralBrush = Serialize.StringToBrush(value); }
				}
		[XmlIgnore()]
		[Description("")]
// 		[Category("Bar Coloring")]
// [Gui.Design.DisplayNameAttribute("Up Color")]
		[Display(Order = 10, Name = "Up Color",  GroupName = "Bar Coloring", ResourceType = typeof(Custom.Resource))]
		public System.Windows.Media.Brush UpBrush{	get { return pUpBrush; }	set { pUpBrush = value; }		}
				[Browsable(false)]
				public string UClSerialize
				{	get { return Serialize.BrushToString(pUpBrush); } set { pUpBrush = Serialize.StringToBrush(value); }
				}

		[XmlIgnore()]
		[Description("")]
// 		[Category("Bar Coloring")]
// [Gui.Design.DisplayNameAttribute("Down Color")]
		[Display(Order = 30, Name = "Down Color",  GroupName = "Bar Coloring", ResourceType = typeof(Custom.Resource))]
		public System.Windows.Media.Brush DownBrush{	get { return pDownBrush; }	set { pDownBrush = value; }		}
				[Browsable(false)]
				public string DClSerialize
				{	get { return Serialize.BrushToString(pDownBrush); } set { pDownBrush = Serialize.StringToBrush(value); }
				}

		private bool pHollowUpBars = true;
		[Description("Do you want up-bars to be hollow (no color) in their candle bodies?")]
		[Display(Order = 50, Name = "Hollow bars?",  GroupName = "Bar Coloring", ResourceType = typeof(Custom.Resource))]
		public bool HollowUpBars{	get { return pHollowUpBars; }	set { pHollowUpBars = value; }		}

		#endregion

		private bool pLaunchPopupOnSetup = false;
		[Description("Launch a popup when a triangle prints (a potential entry)")]
		[Category("Alert")]
		public bool LaunchPopupOnSetup
		{
			get { return pLaunchPopupOnSetup; }
			set { pLaunchPopupOnSetup = value; }
		}
		private bool pLaunchPopupOnEntryPrice = false;
		[Description("Launch a popup when an entry price is found")]
		[Category("Alert")]
		public bool LaunchPopupOnEntryPrice
		{
			get { return pLaunchPopupOnEntryPrice; }
			set { pLaunchPopupOnEntryPrice = value; }
		}

		private bool pEnableEmails = true;
		[Description("To disable all emails, set to 'false'")]
		[Category("Alert")]
		public bool EnableEmails
		{
			get { return pEnableEmails; }
			set { pEnableEmails = value; }
		}
		private string pEmailAlertOnSetup = "";
		[Category("Alert")]
		[Description("Email address to receive an email alert message when a buy or sell is setting up")]
//		[Gui.Design.DisplayName("")]
		public string EmailAlertOnSetup
		{
			get { return pEmailAlertOnSetup; }
			set { pEmailAlertOnSetup = value; }
		}
		private string pEmailAlertOnEntryPrice = "";
		[Category("Alert")]
		[Description("Email address to receive an email alert message when a buy or sell entry price is established")]
//		[Gui.Design.DisplayName("")]
		public string EmailAlertOnEntryPrice
		{
			get { return pEmailAlertOnEntryPrice; }
			set { pEmailAlertOnEntryPrice = value; }
		}
		#region Sounds
		private string pUpTriangleSound = "Scalp_Long_Setup.wav";
		[Category("Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when an up Triangle prints, will generate message in Alerts window")]
//		[Gui.Design.DisplayName("")]
		public string SoundBuyWarning
		{
			get { return pUpTriangleSound; }
			set { pUpTriangleSound = value; }
		}
		private string pDownTriangleSound = "Scalp_Short_Setup.wav";
		[Category("Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when a down Triangle prints, will generate message in Alerts window")]
//		[Gui.Design.DisplayName("")]
		public string SoundSellWarning
		{
			get { return pDownTriangleSound; }
			set { pDownTriangleSound = value; }
		}

		private string pBuySound = "Scalp_LonRenderTarget.wav";
		[Category("Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when an buy price prints, will generate message in Alerts window")]
//		[Gui.Design.DisplayName("")]
		public string SoundBuy
		{
			get { return pBuySound; }
			set { pBuySound = value; }
		}
		private string pSellSound = "Scalp_Short.wav";
		[Category("Audible")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("WAV file to play when a sell price prints, will generate message in Alerts window")]
//		[Gui.Design.DisplayName("")]
		public string SoundSell
		{
			get { return pSellSound; }
			set { pSellSound = value; }
		}

		private bool pEnableSounds = false;
		[Description("Enable sound alerts?")]
		[Category("Audible")]
//		[Gui.Design.DisplayNameAttribute("")]
		public bool SoundEnabled {	get { return pEnableSounds; }	set { pEnableSounds = value; }		}
		#endregion

		#endregion
	}
}
public enum DTS_Hawk_ArrowLocation{OffBar,AtEntryPrice,DoNotPlot}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DTS_Hawk[] cacheDTS_Hawk;
		public DTS_Hawk DTS_Hawk(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return DTS_Hawk(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public DTS_Hawk DTS_Hawk(ISeries<double> input, int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			if (cacheDTS_Hawk != null)
				for (int idx = 0; idx < cacheDTS_Hawk.Length; idx++)
					if (cacheDTS_Hawk[idx] != null && cacheDTS_Hawk[idx].pStartTime == pStartTime && cacheDTS_Hawk[idx].pStopTime == pStopTime && cacheDTS_Hawk[idx].pFlatTime == pFlatTime && cacheDTS_Hawk[idx].pDailyTgtDollars == pDailyTgtDollars && cacheDTS_Hawk[idx].EqualsInput(input))
						return cacheDTS_Hawk[idx];
			return CacheIndicator<DTS_Hawk>(new DTS_Hawk(){ pStartTime = pStartTime, pStopTime = pStopTime, pFlatTime = pFlatTime, pDailyTgtDollars = pDailyTgtDollars }, input, ref cacheDTS_Hawk);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DTS_Hawk DTS_Hawk(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.DTS_Hawk(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public Indicators.DTS_Hawk DTS_Hawk(ISeries<double> input , int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.DTS_Hawk(input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DTS_Hawk DTS_Hawk(int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.DTS_Hawk(Input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}

		public Indicators.DTS_Hawk DTS_Hawk(ISeries<double> input , int pStartTime, int pStopTime, int pFlatTime, double pDailyTgtDollars)
		{
			return indicator.DTS_Hawk(input, pStartTime, pStopTime, pFlatTime, pDailyTgtDollars);
		}
	}
}

#endregion
