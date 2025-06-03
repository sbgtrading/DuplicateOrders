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
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Collections.Concurrent;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
public enum IW_RiskManager_TargetDistanceTypes {Ticks, RiskMultiple}
public enum IW_RiskManager_Modes {Disabled, Pct, Fixed}
public enum IW_RiskManager_PennantLocs {Left,Right}
public enum IW_RiskManager_UIOrientation {Horizontal,Vertical}
public enum IW_RiskManager_LagTimeLocation {NoDisplay, TopLeft, TopRight, Center, BottomLeft, BottomRight}

namespace NinjaTrader.NinjaScript.Indicators
{
// Changes for v1.5 (May 2020)
//		Added ShowBuySellPriceLines - lets the user see the price of both the current bid and ask
//		Entry price line is adjusted to the CurrentAsk on a BuyMkt order, and CurrentBid on a SellMkt order
//		Added Pennant Loc (left or right)...helps reduce visual clutter on current bar
//		Added "UI Orientation" which permits the user to arrange the buttons Horizontally or Vertically
//		Added "Lag Timer"...if your platform data is lagging behind the current time, you can get a text warning on the chart
//		Added "Mode Disabled/Pct/Fixed" parameter - this will let the user define and save the default mode for the indicator at initial launch
//		Improved graphics performance, moved DX brush creation out to the OnRengerTargetChanged
//		Added "ForceRefresh()" to improve the speed response of the UI and the trade visuals
//		Checked for null UI Buttons before attempting assigning them colors and font values.  I still do not know why the buttons aren't defined prior to color assignment...UNKNOWN
//		Fixed OCO orders...when T1 was hit, it would cancel all of the stoploss order, not just the part of the position.  Bad for the remaining position!
// Changes for v1.6 (June 2020)
//		Fixed the "MinContracts" parameter...it was not permitting '0' value
//		Added "Default Entry Order Type" parameter...the user can instantly determine the entry order type default (Stop, Limit or Market)
//		Added ClickToEnterTime 5-second limitation...do not permit multiple market orders (like accidental double-click of the buy or sell button).
// Changes for v1.7 (July 2020)
//		Added new scrollwheel function on "Stop on {indi}" button.  Scrollwheel will now cycle through all the plots on the present chart, simplifying the plot selection process.
// Changes for v1.8 (Aug 2020)
//		Added "LeftShift" code while hovering over Buy or Sell button...hold down LeftShift, and use the mouse wheel to adjust the StopLoss Distance setting.
// Changes for v1.8.1 (Nov 2020)
//		Added ChartControl.InvokeAsync to order submits as per best practices in Nt documentation

	public class IW_RiskManager : Indicator
	{
		#region variables
		private const int PRICE_PANEL_ID_NUMBER = 0;
		private const int SLType_ATR = 0;
		private const int SLType_FIXEDTICKS = 1;
		private const int SLType_HLINE = 2;
		private const int SLType_INDICATOR = 3;
		private const int SLType_ATMSTRATEGY = 4;

		private const int EntryType_MARKET = 0;
		private const int EntryType_LIMIT = 1;
		private const int EntryType_STOP = 2;
		
		private const int QTY_NONE    = 0;
		private const int QTY_RISKPCT = 1;
		private const int QTY_FIXED   = 2;

		private bool isIndicatorAdded;
		private int mouseX = 0;
		private int mouseY = 0; 
		private bool AlertOnce = true;
		private DateTime MessageTime = DateTime.MinValue;
		private DateTime ClickToEnterTime = DateTime.MinValue;

		private System.Windows.Controls.Grid				chartGrid;
//		private NinjaTrader.Gui.Chart.ChartTab				chartTab;
//		private NinjaTrader.Gui.Chart.ChartTrader			chartTraderControl;
		private NinjaTrader.Gui.Chart.Chart					chartWindow;
		private System.Windows.Controls.Menu				topMenu;
		private System.Windows.Media.SolidColorBrush		controlLightGray;
		private SharpDX.Direct2D1.Brush blackDX = null;
		private SharpDX.Direct2D1.Brush BuyELineDX = null;
		private SharpDX.Direct2D1.Brush SellELineDX = null;
		private bool _init = false;
		private double CurrentAsk = 0;
		private double CurrentBid = 0;
		private bool OnMarketDataFound = false;

		private Account myAccount;

		private double DollarRisk = -1;
		private double UpperSL = double.MinValue;
		private double LowerSL = double.MaxValue;
		bool IsShiftPressed    = false;
		bool IsCtrlPressed     = false;
		string InitialStop_IndiBasisName = string.Empty;
		int nearestPlotIdx   = -1;
		int SelectedBuyEntryType  = EntryType_MARKET;
		int SelectedSellEntryType = EntryType_MARKET;
		double nearestprice  = double.MinValue;
		IndicatorBase SelectedIndi = null;
		string SelectedPlotName = string.Empty;
		double entryprice      = 0;
		double EntryLimitPrice = 0;
		double EntryStopPrice  = 0;
		double atr0 = 0;
		bool ATMStrategyEnabled = false;
		bool UseAtmStrategySL = false;
		DateTime DT_StoplossChanged = DateTime.MinValue;
		string BuySellLabel = string.Empty;
		bool ShowTicksInEllipse = false;
		//int QuantityMode = QTY_NONE;

		#region - UI buttons, menus, combobox -
		private	System.Windows.Controls.Button		Buy_Btn;
		private	System.Windows.Controls.Button		Sell_Btn;
		private System.Windows.Controls.Button		TicksFor_StopOrLimit_Btn;
		private	System.Windows.Controls.Button		PositionSize_Btn;
		private System.Windows.Controls.Button		Stoploss_Btn;
		private System.Windows.Controls.Grid   		MenuGrid;
		private System.Windows.Controls.WrapPanel   BuySellPanel;
		private System.Windows.Controls.Primitives.Thumb drag_BuySell;
		static int defaultMargin          = 5;
		double TogglePositionMarginLeft	  = 5;
		double TogglePositionMarginTop    = 5;
		double TogglePositionMarginRight  = 5;
		double TogglePositionMarginBottom = 5;
		int current_indi_id = 0;
		#endregion

		#region Target data, position size, SL type
		int UserDefinedPositionSize = 1;
		int SelectedSLType        = SLType_ATR;
		ConcurrentStack<char> EntryDirection_OnHoverQue = new ConcurrentStack<char>();
		double LongQty    = 0;
		double ShortQty   = 0;
		double LongSLPts  = 0;
		double ShortSLPts = 0;

		private class TargetData{
			public double Price    = double.NaN;
			public double OrderQty = 0;
			public double QtyPct   = 0;
			public double RRratio  = 0;
			public int Ticks  = 0;
			public string MarkerTag = "";
			public TargetData(double qty_as_pct, double rr_ratio, int ticks, string tag){
				RRratio = rr_ratio; Ticks=ticks; QtyPct = qty_as_pct/100; MarkerTag=tag;
			}
		}
		ConcurrentDictionary<int,TargetData> LongTargets = null;
		ConcurrentDictionary<int,TargetData> ShortTargets = null;
		#endregion
		#endregion
//		Object BuyLock = new Object();
//		Object SellLock = new Object();
		SharpDX.Direct2D1.Brush buy_ellipse_DX  = null;
		SharpDX.Direct2D1.Brush sell_ellipse_DX = null;
		int ATM_T1_Ticks = int.MinValue;
		int ATM_T2_Ticks = int.MinValue;
		int ATM_T3_Ticks = int.MinValue;
		int ATM_SL_Ticks = int.MinValue;
		string Order_Reject_Message = string.Empty;
		string Log_Order_Reject_Message = string.Empty;
		int line = 0;
		DateTime TimeOfLastTick = DateTime.MinValue;

		#region mouse and key events
		public void MyMouseUpEvent(object sender, MouseEventArgs e)
		{
			mouseY = (int)e.GetPosition(ChartPanel).Y;
			mouseX = (int)e.GetPosition(ChartPanel).X;
			ForceRefresh();
		}
		private void MyKeyUpEvent(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if(e.Key==Key.LeftShift || e.Key==Key.RightShift) IsShiftPressed = true;
			if(e.Key==Key.LeftCtrl || e.Key==Key.RightCtrl) IsCtrlPressed = true;
			if(IsShiftPressed || IsCtrlPressed) {
				InitialStop_IndiBasisName = string.Empty;
				nearestPlotIdx = -1;
			}
		}
		private void MyKeyDownEvent(object sender, KeyEventArgs e)
		{
			e.Handled      = true;
			IsShiftPressed = false;
			IsCtrlPressed  = false;
			InitialStop_IndiBasisName = string.Empty;
			nearestPlotIdx = -1;
		}
		#endregion
		public override string DisplayName{	get { return $"RiskManager on account {SelectedAccountName}"; }}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "iw RiskManager v1.8.3";
				//1.8.2: Dec 4 2020:  Added new LoadAccountNameList...more intelligent account selection based on connected accounts
				//1.8.3: Dec 13 2024: Corrected default/active coloring of Entrydistance button
				#region SetDefaults
				string ExemptMachine1 = "B0D2E9D1C802E279D3678D7DE6A33CE4";
				string ExemptMachine2 = "CB15E08BE30BC80628CFF6010471FA2A";
				bool ExemptMachine = NinjaTrader.Cbi.License.MachineId==ExemptMachine1 || NinjaTrader.Cbi.License.MachineId==ExemptMachine2;
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachine;
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "AIRiskManager", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

//1.2 - fixed the "Fixed size" was not operable...all orders would be placed at 1-contract.  Added the "ShowDollarsRisked" and "ShowDollarsReward" to print dollar values in the ellipses on the chart

				Description									= @"";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				pTargetDistanceType = IW_RiskManager_TargetDistanceTypes.RiskMultiple;
				pEnableOCO = true;
				pT1_Ticks = 4;
				pT2_Ticks = 8;
				pT3_Ticks = 14;
				RRT1      = 1.5;
				QtyT1Pct  = 50;
				RRT2      = 3.25;
				QtyT2Pct  = 30;
				RRT3      = 0;
				QtyT3Pct  = 0.0;
				AccountBalance = 10000;
				PctRisk    = 2.5;
				DollarRisk = 0;
				this.HLineTag = "Horiz";
				MaxContracts = 10;
				MinContracts = 1;
				InitialStop_ATRMult = 3.5;
				InitialStop_Ticks = 8;
				SelectedAccountName = "";
				EntryStopTicksOffset = 4;
				EntryLimitTicksOffset = 4;
				EntryDirection_OnHoverQue.Push(' ');
				ATMStrategyName     = "N/A";
				LineLengthF         = 150f;
				MarkerOpacity       = 50;
				ShowDollarsRisked   = true;
				ShowDollarsReward   = true;
				pMode               = IW_RiskManager_Modes.Pct;
				pPennantLocs        = IW_RiskManager_PennantLocs.Right;
				pOrientation        = IW_RiskManager_UIOrientation.Vertical;
				pDefaultOrderType  = "Market";
				pShowBuySellPriceLines = true;
				pShowBidAskDots = true;
				pLagTimerLocation   = IW_RiskManager_LagTimeLocation.BottomLeft;
				pLagWarningFont     = new SimpleFont("Arial", 14);
				pLagWarningSeconds  = 2;
				Brush_RiskButtonBkg = Brushes.Gold;
				Brush_RiskButtonText = Brushes.Black;
				Brush_SLButtonBkg = Brushes.Orange;
				Brush_SLButtonText = Brushes.Black;
				Brush_BuyButtonBkg = Brushes.Lime;
				Brush_BuyButtonText = Brushes.Black;
				Brush_SellButtonBkg = Brushes.Red;
				Brush_SellButtonText = Brushes.Black;
				Brush_EntryDistanceButtonBkg = Brushes.DodgerBlue;
				Brush_EntryDistanceButtonText = Brushes.Black;
				#endregion
			}
			#region OnStateChange
			if(State == State.Configure){
				Calculate = Calculate.OnPriceChange;
				PctRisk = Math.Round(PctRisk,1);
				Log_Order_Reject_Message = "Your order was rejected.  Account Name '"+SelectedAccountName+"' may not be available on this datafeed connection.  Check for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				Order_Reject_Message = "Your order was rejected.\nAccount Name '"+SelectedAccountName+"' may not be available on this datafeed connection\nCheck for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				if(pDefaultOrderType.Trim().Length==0){
					SelectedBuyEntryType  = EntryType_MARKET;
					SelectedSellEntryType = EntryType_MARKET;
					pDefaultOrderType = "Market";
				}else if(pDefaultOrderType.ToLower().Trim()[0]=='m'){
					SelectedBuyEntryType  = EntryType_MARKET;
					SelectedSellEntryType = EntryType_MARKET;
					pDefaultOrderType = "Market";
				}else if(pDefaultOrderType.ToLower().Trim()[0]=='s'){
					SelectedBuyEntryType  = EntryType_STOP;
					SelectedSellEntryType = EntryType_STOP;
					pDefaultOrderType = "Stop";
				}else if(pDefaultOrderType.ToLower().Trim()[0]=='l'){
					SelectedBuyEntryType  = EntryType_LIMIT;
					SelectedSellEntryType = EntryType_LIMIT;
					pDefaultOrderType = "Limit";
				}
			}
			if (State == State.DataLoaded)
			{
				#region DataLoaded
				controlLightGray = new System.Windows.Media.SolidColorBrush(Color.FromRgb(64, 63, 69));
				controlLightGray.Freeze();
				int count = 0;
				if(pTargetDistanceType == IW_RiskManager_TargetDistanceTypes.Ticks){
					if(pT1_Ticks>0 && QtyT1Pct>0) count++;
					if(pT2_Ticks>0 && QtyT2Pct>0) count++;
					if(pT3_Ticks>0 && QtyT3Pct>0) count++;
				}else{
					if(RRT1 > 0 && QtyT1Pct>0) count++;
					if(RRT2 > 0 && QtyT2Pct>0) count++;
					if(RRT3 > 0 && QtyT3Pct>0) count++;
				}
				if(count>0) {
					LongTargets = new ConcurrentDictionary<int,TargetData>();
					ShortTargets = new ConcurrentDictionary<int,TargetData>();
					LongTargets[0]  = new TargetData(this.QtyT1Pct, RRT1, pT1_Ticks, "T1Long");
					LongTargets[1]  = new TargetData(this.QtyT2Pct, RRT2, pT2_Ticks, "T2Long");
					LongTargets[2]  = new TargetData(this.QtyT3Pct, RRT3, pT3_Ticks, "T3Long");
					ShortTargets[0] = new TargetData(this.QtyT1Pct, RRT1, pT1_Ticks, "T1Short");
					ShortTargets[1] = new TargetData(this.QtyT2Pct, RRT2, pT2_Ticks, "T2Short");
					ShortTargets[2] = new TargetData(this.QtyT3Pct, RRT3, pT3_Ticks, "T3Short");
				}
				#endregion

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
					string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "RiskManager.log");
					var lines = new string[]{"x"};
					if(System.IO.File.Exists(path)) lines = System.IO.File.ReadAllLines(path);
					long MostRecent = long.MaxValue;
					foreach(var line in lines){
						if(line.Contains($"'{SelectedAccountName}'")) {
							var elements = line.Split(new char[]{'\t'});
							if(elements.Length>0){
								var dt = DateTime.Parse(elements[0]);
								long diff = DateTime.Now.Ticks - dt.Ticks;
								if(diff<MostRecent) MostRecent=diff;
							}
						}
					}
					var ts = new TimeSpan(MostRecent);
					if(ts.TotalDays>5) System.IO.File.Delete("RiskManager.Log");
					else if(ts.TotalMinutes > 10){
						string msg = "ERROR = account '"+SelectedAccountName+"' is not available to trade";
						Log(msg, LogLevel.Alert);
						System.IO.File.AppendAllText("RiskManager.Log", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
					}
				}
				#endregion
				this.ATM_SL_Ticks = int.MinValue;
				this.ATM_T1_Ticks = int.MinValue;
				this.ATM_T2_Ticks = int.MinValue;
				this.ATM_T3_Ticks = int.MinValue;
				ATMStrategyEnabled = false;
				UseAtmStrategySL = false;
				if(ATMStrategyName!="N/A" && ATMStrategyName.Length>0){
					ATMStrategyEnabled = true;
					string xmlname = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"templates","AtmStrategy",ATMStrategyName.Trim()+".xml");
					var lines = System.IO.File.ReadAllLines(xmlname);
					int x =0;
					foreach(var line in lines){
						if(line.Contains("<StopLoss>") && ATM_SL_Ticks==int.MinValue){
							var input = line.Replace("<StopLoss>",string.Empty).Replace("</StopLoss>",string.Empty);
							if(int.TryParse(input,out x)) ATM_SL_Ticks = x;
						}
						if(line.Contains("<Target>")){
							var input = line.Replace("<Target>",string.Empty).Replace("</Target>",string.Empty);
							if(ATM_T1_Ticks==int.MinValue && int.TryParse(input,out x)) ATM_T1_Ticks = x;
							else if(ATM_T2_Ticks==int.MinValue && int.TryParse(input,out x)) ATM_T2_Ticks = x;
							else if(ATM_T3_Ticks==int.MinValue && int.TryParse(input,out x)) ATM_T3_Ticks = x;
						}
					}
				}
			}
			if (State == State.Terminated)
			{
				#region Terminated
				if (ChartControl != null)
				{
					ChartPanel.MouseUp 	-= MyMouseUpEvent;
					ChartPanel.KeyUp    -= MyKeyUpEvent;
					ChartPanel.KeyDown  -= MyKeyDownEvent;
				}
				DisposeCleanUp();
				#endregion
			}
//return;
			if (State == State.Historical)
			{
line=365;
				#region Historical
				// Use an Automation ID to limit multiple instances of this indicator.
				if (ChartControl != null && !isIndicatorAdded)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
try{
line=373;
						chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
						if (chartWindow == null) return;

line=377;
						chartGrid	= chartWindow.MainTabControl.Parent as System.Windows.Controls.Grid;
						foreach (DependencyObject item in chartGrid.Children)
						{
line=381;
							if (AutomationProperties.GetAutomationId(item) == "IW_RiskManager")
							{
								isIndicatorAdded = true;
							}
						}

line=388;
						if (!isIndicatorAdded)
						{
line=391;
							AddToolBar();
line=393;
							#region -- Set background of TicksFor_StopOrLimit_Btn --
							if(TicksFor_StopOrLimit_Btn!=null){
								if(SelectedBuyEntryType != EntryType_MARKET || SelectedSellEntryType != EntryType_MARKET){
									TicksFor_StopOrLimit_Btn.Background = Brushes.Blue;
									TicksFor_StopOrLimit_Btn.Foreground = Brushes.LightGray;
								}else
									TicksFor_StopOrLimit_Btn.Background = Brushes.Navy;
									TicksFor_StopOrLimit_Btn.Foreground = Brushes.Black;
							}
							#endregion ---------------------------
line=404;
							topMenu = new System.Windows.Controls.Menu()
							{
								Background			= Brushes.Transparent,
								BorderBrush			= controlLightGray,
								Padding				= new System.Windows.Thickness(0),
								Margin				= new System.Windows.Thickness(0,-2,0,2),
								VerticalAlignment	= VerticalAlignment.Center				
							};
line=413;
							AutomationProperties.SetAutomationId(topMenu, "IW_RiskManager");
							// Begin AdvancedRiskReward OnStartUp
							if (!_init)
							{
line=418;
								ChartPanel.MouseUp += new MouseButtonEventHandler(MyMouseUpEvent);
line=420;
								ChartPanel.KeyUp   += new KeyEventHandler(MyKeyUpEvent);
line=422;
								ChartPanel.KeyDown += new KeyEventHandler(MyKeyDownEvent);
								_init = true;
							}
line=426;
						}
}catch(Exception e124){Print(line+"  e124: "+e124.ToString());}
					}));
				}
				#endregion
			}
			#endregion
		}
		//=======================================================================================
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
		//=======================================================================================
		protected override void OnBarUpdate()
		{
			if (isIndicatorAdded)
			{
				if(AlertOnce) {
					Log("'IW RiskManager' does not support multiple instances. Please remove any additional instances of this indicator.", LogLevel.Alert);
					AlertOnce = false;
				}
				return;
			}
			atr0 = ATR(14)[0];
		}
//		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs c)
//		{
//			ConnectionStatusEventArgs cCopy = c;
//			Dispatcher.InvokeAsync(() =>
//			{
//				Console.WriteLine(string.Format("Name: {0}\nBrandName: {1}\nMode: {2}\nTypeName: {3}",
//				  		cCopy.Connection.Options.Name,
//						cCopy.Connection.Options.BrandName,
//						cCopy.Connection.Options.Mode.ToString(),
//						cCopy.Connection.Options.TypeName));
//			});
//		}
		private void DebugPrint(string s){
		//	Print(s);
		}
//==============================================================================================
		#region - GUI, BUTTONS, MENUS... -
			#region - AddToolbar -
			private void AddToolBar()
			{
				// Use this.Dispatcher to ensure code is executed on the proper thread
				ChartControl.Dispatcher.InvokeAsync((Action)(() =>
				{
					// Grid already exists
					if (UserControlCollection.Contains(BuySellPanel))
					    return;
					AddControls();
					if(pOrientation == IW_RiskManager_UIOrientation.Vertical) {
						Buy_Btn.Width = 120;
						Sell_Btn.Width = 120;
						TicksFor_StopOrLimit_Btn.HorizontalContentAlignment = HorizontalAlignment.Left;
						MenuGrid.ColumnDefinitions[1].Width = new GridLength(120);
					}

					UserControlCollection.Add(BuySellPanel);
				}));
            }
            #endregion

            #region - CleanToolBar -
            private void DisposeCleanUp()
            {
                if (chartWindow != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (MenuGrid != null)
                        {
                            UserControlCollection.Remove(BuySellPanel);
                        }

                    }));
                }
            }
            #endregion

            #region - AddControlsToToolbar -
            private void AddControls()
            {
				string uID = Instrument.MasterInstrument.Name+DateTime.Now.Ticks.ToString();
				// Add a control grid which will host our custom buttons
				MenuGrid = new System.Windows.Controls.Grid
				{
					Name = "MenuGrid"+uID,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment   = VerticalAlignment.Top
				};

                #region - Columns -
				if(pOrientation == IW_RiskManager_UIOrientation.Horizontal){
	                // Define the two columns in the grid, one for each button
					var c1   = new System.Windows.Controls.ColumnDefinition();
					var c2   = new System.Windows.Controls.ColumnDefinition();
					var c3   = new System.Windows.Controls.ColumnDefinition();
					var c4   = new System.Windows.Controls.ColumnDefinition();
					var c5   = new System.Windows.Controls.ColumnDefinition();
					var c6   = new System.Windows.Controls.ColumnDefinition();
					var c7   = new System.Windows.Controls.ColumnDefinition();
					var c8   = new System.Windows.Controls.ColumnDefinition();
					var c9   = new System.Windows.Controls.ColumnDefinition();
					var c10  = new System.Windows.Controls.ColumnDefinition();
					var c11  = new System.Windows.Controls.ColumnDefinition();
					var row1 = new System.Windows.Controls.RowDefinition();
					row1.Height       = new GridLength(50);
					var rowSpacer1    = new System.Windows.Controls.RowDefinition();
					rowSpacer1.Height = new GridLength(5);
					var row2          = new System.Windows.Controls.RowDefinition();
					row2.Height       = new GridLength(50);
					var rowSpacer2    = new System.Windows.Controls.RowDefinition();
					rowSpacer2.Height = new GridLength(5);
	//				rowSpacer1.SetValue(System.Windows.Controls.WrapPanel.BackgroundProperty,Brushes.Orange);
	//				var rowSpacer3 = new System.Windows.Controls.RowDefinition();
	//				rowSpacer3.Height = new GridLength(5);
					c1.Width  = new GridLength(20);
					c2.Width  = new GridLength(110);
					c3.Width  = new GridLength(10);
					c4.Width  = new GridLength(120);
					c5.Width  = new GridLength(10);
					c6.Width  = new GridLength(90);
					c7.Width  = new GridLength(10);
					c8.Width  = new GridLength(100);
					c9.Width  = new GridLength(10);
					c10.Width = new GridLength(110);
					c11.Width = new GridLength(10);

	                // Add the columns to the Grid
					MenuGrid.RowDefinitions.Add(rowSpacer1);
					MenuGrid.RowDefinitions.Add(row1);
					MenuGrid.RowDefinitions.Add(rowSpacer2);
	//				MenuGrid.RowDefinitions.Add(row2);
	//				MenuGrid.RowDefinitions.Add(rowSpacer3);
					MenuGrid.ColumnDefinitions.Add(c1);
					MenuGrid.ColumnDefinitions.Add(c2);
					MenuGrid.ColumnDefinitions.Add(c3);
					MenuGrid.ColumnDefinitions.Add(c4);
					MenuGrid.ColumnDefinitions.Add(c5);
					MenuGrid.ColumnDefinitions.Add(c6);
					MenuGrid.ColumnDefinitions.Add(c7);
					MenuGrid.ColumnDefinitions.Add(c8);
					MenuGrid.ColumnDefinitions.Add(c9);
					MenuGrid.ColumnDefinitions.Add(c10);
					MenuGrid.ColumnDefinitions.Add(c11);
				}else{
	                // Define the two columns in the grid, one for each button
					var c1 = new System.Windows.Controls.ColumnDefinition();
					var c2 = new System.Windows.Controls.ColumnDefinition();

					var row1          = new System.Windows.Controls.RowDefinition();
					row1.Height       = new GridLength(50);
					var row2          = new System.Windows.Controls.RowDefinition();
					row2.Height       = new GridLength(50);
					var row3          = new System.Windows.Controls.RowDefinition();
					row3.Height       = new GridLength(50);
					var row4          = new System.Windows.Controls.RowDefinition();
					row4.Height       = new GridLength(50);
					var row5          = new System.Windows.Controls.RowDefinition();
					row5.Height       = new GridLength(50);

					var rowSpacer1    = new System.Windows.Controls.RowDefinition();
					rowSpacer1.Height = new GridLength(5);
					var rowSpacer2    = new System.Windows.Controls.RowDefinition();
					rowSpacer2.Height = new GridLength(5);
					var rowSpacer3    = new System.Windows.Controls.RowDefinition();
					rowSpacer3.Height = new GridLength(5);
					var rowSpacer4    = new System.Windows.Controls.RowDefinition();
					rowSpacer4.Height = new GridLength(5);
					var rowSpacer5    = new System.Windows.Controls.RowDefinition();
					rowSpacer5.Height = new GridLength(5);
					var rowSpacer6    = new System.Windows.Controls.RowDefinition();
					rowSpacer6.Height = new GridLength(5);
	//				rowSpacer1.SetValue(System.Windows.Controls.WrapPanel.BackgroundProperty,Brushes.Orange);
	//				var rowSpacer3 = new System.Windows.Controls.RowDefinition();
	//				rowSpacer3.Height = new GridLength(5);
					c1.Width = new GridLength(20);
					c2.Width = new GridLength(120);

	                // Add the columns to the Grid
					MenuGrid.RowDefinitions.Add(rowSpacer1);
					MenuGrid.RowDefinitions.Add(row1);
					MenuGrid.RowDefinitions.Add(rowSpacer2);
					MenuGrid.RowDefinitions.Add(row2);
					MenuGrid.RowDefinitions.Add(rowSpacer3);
					MenuGrid.RowDefinitions.Add(row3);
					MenuGrid.RowDefinitions.Add(rowSpacer4);
					MenuGrid.RowDefinitions.Add(row4);
					MenuGrid.RowDefinitions.Add(rowSpacer5);
					MenuGrid.RowDefinitions.Add(row5);
					MenuGrid.RowDefinitions.Add(rowSpacer6);
//					MenuGrid.RowDefinitions.Add(row6);
//					MenuGrid.RowDefinitions.Add(rowSpacer7);
					MenuGrid.ColumnDefinitions.Add(c1);
					MenuGrid.ColumnDefinitions.Add(c2);
				}

                #endregion

                #region - BuySellPanel -
				BuySellPanel = new System.Windows.Controls.WrapPanel();

				Thickness thickness = new Thickness(TogglePositionMarginLeft, TogglePositionMarginTop, TogglePositionMarginRight, TogglePositionMarginBottom);
				BuySellPanel.HorizontalAlignment = HorizontalAlignment.Left;
				BuySellPanel.VerticalAlignment   = VerticalAlignment.Top;
				BuySellPanel.Margin     = thickness;
				BuySellPanel.Background = Brushes.DimGray;


				drag_BuySell = new System.Windows.Controls.Primitives.Thumb();
				drag_BuySell.Width = 20;
				drag_BuySell.Cursor = Cursors.SizeAll;
				drag_BuySell.ToolTip = "Drag and drop anywhere on chart";
				drag_BuySell.DragDelta += OnDragDelta;
				FrameworkElementFactory fef = new FrameworkElementFactory(typeof(System.Windows.Controls.Border));
				fef.SetValue(System.Windows.Controls.Border.BackgroundProperty, Brushes.DimGray);
				drag_BuySell.Background = Brushes.Brown;
				drag_BuySell.Template = new System.Windows.Controls.ControlTemplate(typeof(System.Windows.Controls.Primitives.Thumb)) { VisualTree = fef };


				System.Windows.Controls.Grid.SetColumn(drag_BuySell, 0);
				System.Windows.Controls.Grid.SetRow(drag_BuySell, 1);
				MenuGrid.Children.Add(drag_BuySell);
				#endregion

				#region - PositionSize_Btn -
				PositionSize_Btn = new System.Windows.Controls.Button
				{
				    Name = "PositionSize_Btn"+uID,
					Content = ( pMode == IW_RiskManager_Modes.Disabled ? "Disabled"
								: (pMode == IW_RiskManager_Modes.Pct   ? (object) $"Risk {(PctRisk/100.0).ToString("0.0%").Replace(".0%","%")}\n{(this.AccountBalance*PctRisk/100.0).ToString("$0")}"
								: (object) $"Fixed size:\n{UserDefinedPositionSize}")),
					ToolTip    = "Click to cycle thru options",
					FontWeight = FontWeights.Bold,
				    FontSize   = 16,
					Width      = 200,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = Brush_RiskButtonBkg,
					Foreground = Brush_RiskButtonText
				};

				#region PositionSize events
				PositionSize_Btn.Click += delegate(System.Object o, RoutedEventArgs e)
                	{	e.Handled = true;
						var item = (System.Windows.Controls.Button)o;
						if(item.Content.ToString().Contains("Risk")) {
							pMode = IW_RiskManager_Modes.Fixed;
							item.Content = (object) $"Fixed size:\n{UserDefinedPositionSize}";
							if(Buy_Btn != null){
								Buy_Btn.Background = Brush_BuyButtonBkg;
								Buy_Btn.Foreground = Brush_BuyButtonText;
							}else DebugPrint("Buy_Btn was null! (1)");
							if(Sell_Btn != null){
								Sell_Btn.Background = Brush_SellButtonBkg;
								Sell_Btn.Foreground = Brush_SellButtonText;
							}else DebugPrint("Sell_Btn was null (1)");
						}
						else if(item.Content.ToString().Contains("Fixed")){
							pMode = IW_RiskManager_Modes.Disabled;
							item.Content = (object) "Disabled";
							if(Buy_Btn != null){
								Buy_Btn.Background = Brushes.Silver;
								Buy_Btn.Foreground = Brushes.LightGray;
							}else DebugPrint("Buy_Btn was null! (2)");
							if(Sell_Btn != null){
								Sell_Btn.Background = Brushes.Silver;
								Sell_Btn.Foreground = Brushes.LightGray;
							}else DebugPrint("Sell_Btn was null! (2)");
						}
						else if(item.Content.ToString().Contains("Disabled")) {
							pMode = IW_RiskManager_Modes.Pct;
							item.Content = (object) $"Risk {(PctRisk/100.0).ToString("0.0%").Replace(".0%","%")}\n{(this.AccountBalance*PctRisk/100.0).ToString("$0")}";
							if(Buy_Btn != null){
								Buy_Btn.Background  = Brush_BuyButtonBkg;
								Buy_Btn.Foreground  = Brush_BuyButtonText;
							}else DebugPrint("Buy_Btn was null! (3)");
							if(Sell_Btn != null){
								Sell_Btn.Background = Brush_SellButtonBkg;
								Sell_Btn.Foreground = Brush_SellButtonText;
							}else DebugPrint("Sell_Btn was null! (3)");
						}
//							item.Content = (object) string.Format("Risk {0}", PctRisk.ToString("0.0%").Replace(".0%","%"));
					};
				PositionSize_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
//Print(DateTime.Now.ToString()+"   QuantityMode: "+(QuantityMode==QTY_RISKPCT ? "RiskPct" : (QuantityMode==QTY_FIXED ? "Fixed" : QuantityMode.ToString()));
						e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						if(pMode == IW_RiskManager_Modes.Pct) {
							PctRisk        = Math.Max(0.1,Math.Min(100,PctRisk+0.1 * Math.Sign(e.Delta)));
//							pRiskStatement = PctRisk.ToString("0.0%").Replace(".0%","%");
							item.Content   = (object) $"Risk {(PctRisk/100.0).ToString("0.0%").Replace(".0%","%")}\n{(this.AccountBalance*PctRisk/100.0).ToString("$0")}";
						}
						else if(pMode == IW_RiskManager_Modes.Fixed) {
							this.UserDefinedPositionSize = Math.Max(this.MinContracts, Math.Min(this.MaxContracts, this.UserDefinedPositionSize + 1 * Math.Sign(e.Delta)));
							item.Content = (object) $"Fixed size:\n{UserDefinedPositionSize}";
						}
						ForceRefresh();
					};
				#endregion

				if(pOrientation == IW_RiskManager_UIOrientation.Horizontal){
					System.Windows.Controls.Grid.SetColumn(PositionSize_Btn, 1);
					System.Windows.Controls.Grid.SetRow(PositionSize_Btn, 1);
				}else{
					System.Windows.Controls.Grid.SetColumn(PositionSize_Btn, 1);
					System.Windows.Controls.Grid.SetRow(PositionSize_Btn, 1);
				}
				MenuGrid.Children.Add(PositionSize_Btn);
				#endregion

				#region - Stoploss_Btn -
				Stoploss_Btn = new System.Windows.Controls.Button
				{
				    Name = "Stoploss_Btn"+uID,
					Content = (object) $"Stop ATR  {InitialStop_ATRMult}",
					FontWeight = FontWeights.Bold,
				    FontSize = 16,
					Width = 200,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = Brush_SLButtonBkg,
					Foreground = Brush_SLButtonText
				};
				SelectedSLType = SLType_ATR;

				#region Stoploss Events
				Stoploss_Btn.Click += delegate(System.Object o, RoutedEventArgs e)
                	{	e.Handled = true;
						DT_StoplossChanged = DateTime.Now;
						var item = (System.Windows.Controls.Button)o;
						if(item.Content.ToString().Contains("HLine")) {
							item.Content = (object) $"Stop ticks  {InitialStop_Ticks}";
							SelectedSLType = SLType_FIXEDTICKS;
						}
						else if(item.Content.ToString().Contains("ticks")) {
							InitialStop_ATRMult = Math.Round(InitialStop_ATRMult,1);
							item.Content = (object) $"Stop ATR  {InitialStop_ATRMult}";
							SelectedSLType = SLType_ATR;
						}
						else if(item.Content.ToString().Contains("ATR")) {
							if(InitialStop_IndiBasisName.Length==0 || InitialStop_IndiBasisName.StartsWith("Horizontal Line"))
								item.Content = (object) "Stop on\n{indi}";
							else
								item.Content = (object) $"Stop on\n{InitialStop_IndiBasisName}";
							SelectedSLType = SLType_INDICATOR;
						}
						else if(item.Content.ToString().Contains("Stop on")) {
							UseAtmStrategySL = false;
							if(ATMStrategyEnabled){
								UseAtmStrategySL = true;
								item.Content = $"ATM Stop\n'{ATMStrategyName}'";
								SelectedSLType = SLType_ATMSTRATEGY;
							}else{
								item.Content = "Stop (HLine)\n'"+HLineTag+"'";
								SelectedSLType = SLType_HLINE;
							}
						}
						else if(item.Content.ToString().Contains("ATM")) {
							UseAtmStrategySL = false;
							item.Content = "Stop (HLine)\n'"+HLineTag+"'";
							SelectedSLType = SLType_HLINE;
						}
						ForceRefresh();
					};
				Stoploss_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{	e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						DT_StoplossChanged = DateTime.Now;
//						if(item.Content.ToString().Contains("ticks")) {
						if(SelectedSLType == SLType_FIXEDTICKS){
							InitialStop_Ticks = Math.Max(1,InitialStop_Ticks + Math.Sign(e.Delta));
							item.Content = (object) $"Stop ticks  {InitialStop_Ticks}";
						}
//						else if(item.Content.ToString().Contains("ATR")) {
						else if(SelectedSLType == SLType_ATR){
							InitialStop_ATRMult = Math.Round(Math.Max(0.1, InitialStop_ATRMult + 0.1 * Math.Sign(e.Delta)),1);
							item.Content = (object) $"Stop ATR  {InitialStop_ATRMult}";
						}
						else if(SelectedSLType == SLType_INDICATOR){
							//item.Content = (object) "Stop on\n{indi}";
							current_indi_id += e.Delta>0 ? 1 : -1;
							double price = 0;
							int id = 0;
							int plot_count = 0;
							#region -- get a count of all plots available --
							foreach(IndicatorBase indi in ChartControl.Indicators){
								try{
									if(indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
//										abar0 = abar - indi.Displacement;
										for(int i = 0; i<indi.Values.Length; i++){
											var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
											if(cl.IsTransparent()) continue;
											plot_count++;
										}
									}
								}catch(Exception e875){Print(line+"  e875: "+e875.ToString());}
							}
							#endregion
							if(current_indi_id > plot_count) current_indi_id = 1;
							if(current_indi_id < 1) current_indi_id = plot_count;

							int abar = CurrentBars[0];
							nearestprice = double.MinValue;
							foreach(IndicatorBase indi in ChartControl.Indicators){
								#region -- cycle through all indicators and then all plots in each indicator, looking for nearest plot --
								try{
									if(indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
										int abar0 = abar - indi.Displacement;
										for(int i = 0; i<indi.Values.Length; i++){
											var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
											if(cl.IsTransparent()) continue;
											id++;
											if(id != current_indi_id) continue;

											int adj = 0;
											try{
												if(indi.Calculate==Calculate.OnBarClose) {
													adj=1;
													abar0 = abar0-adj;
//Print(indi.Name+" ("+indi.Plots[i].Name+") is valid output value ("+indi.Plots[i]+") at this bar "+abar0+", "+Bars.GetTime(abar0).ToString());
												}
												else if(!indi.Values[i].IsValidDataPointAt(abar0)) {
//Print(indi.Name+" ("+indi.Plots[i].Name+") is not a valid output value at this bar "+abar0+", "+Bars.GetTime(abar0).ToString());
													continue;
												}
											}catch{}
											if(id == current_indi_id){
												nearestprice = indi.Values[i].GetValueAt(abar0);
												SelectedPlotName = indi.Plots[i].Name;
												nearestPlotIdx  = i;
												InitialStop_IndiBasisName = indi.Name;
												if(SelectedSLType == SLType_INDICATOR) Stoploss_Btn.Content = "Stop on\n"+InitialStop_IndiBasisName + (indi.Plots[i].Name.CompareTo(InitialStop_IndiBasisName)==0 ? string.Empty : " "+ indi.Plots[i].Name);
												SelectedIndi    = indi;
//Print("\nNearest name: "+InitialStop_IndiBasisName+"   price: "+nearestprice);
											}
										}
									}
								}catch(Exception e910){Print(line+"  e910: "+e910.ToString());}
								#endregion
							}

						}
						ForceRefresh();
					};
				#endregion

				if(pOrientation == IW_RiskManager_UIOrientation.Horizontal){
					System.Windows.Controls.Grid.SetColumn(Stoploss_Btn, 3);
					System.Windows.Controls.Grid.SetRow(Stoploss_Btn, 1);
				}else{
					System.Windows.Controls.Grid.SetColumn(Stoploss_Btn, 1);
					System.Windows.Controls.Grid.SetRow(Stoploss_Btn, 3);
				}
				MenuGrid.Children.Add(Stoploss_Btn);
				#endregion

				#region - Buy_Btn -
				//---------------------------------------------------------------------
//Print("BuyBtn created");
				Buy_Btn = new System.Windows.Controls.Button
				{
				    Name       = "Buy_Btn"+uID,
				    Content    = SelectedBuyEntryType==EntryType_MARKET ? "BUY Mkt" : (SelectedBuyEntryType==EntryType_LIMIT ? "BUY Limit" : "BUY Stop"),
					ToolTip    = "Mousewheel to cycle thru\nMkt/Stop/Limit options",
				    FontSize   = 16,
					FontWeight = FontWeights.Bold,
					Width      = 90,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = Brushes.Silver,
					Foreground = Brushes.LightGray
				};

				#region Buy_Btn Events
				//---------------------------------------------------------------------
				Buy_Btn.Click += Buy_Btn_Click;

				#region MouseEnter
				Buy_Btn.MouseEnter += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('B');
					BuySellLabel = "Buy";
					CalculateTarget_Price(entryprice, LongTargets, LongSLPts);
					CalculateTarget_OrderQty(LongTargets, LongQty);
					foreach(var k in ShortTargets.Keys)
						ShortTargets[k].Price=double.NaN;
						ForceRefresh();
				};
				#endregion
				#region MouseLeave
				Buy_Btn.MouseLeave += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('B');
					//BuySellLabel = "";
					CalculateTarget_Price(entryprice, LongTargets, LongSLPts);
					CalculateTarget_OrderQty(LongTargets, LongQty);
					foreach(var k in ShortTargets.Keys)
						ShortTargets[k].Price=double.NaN;
					ForceRefresh();
				};
				#endregion
				#region MouseWheel
				Buy_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
						e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						if(Keyboard.IsKeyDown(Key.LeftShift)){
							#region -- Adjust the stoploss distance while hovering over the Buy button --
							DT_StoplossChanged = DateTime.Now;
							if(SelectedSLType == SLType_FIXEDTICKS){
								InitialStop_Ticks = Math.Max(1,InitialStop_Ticks + Math.Sign(e.Delta));
								Stoploss_Btn.Content = (object) $"Stop ticks  {InitialStop_Ticks}";
							}
							else if(SelectedSLType == SLType_ATR){
								this.InitialStop_ATRMult = Math.Round(Math.Max(0.1, this.InitialStop_ATRMult + 0.1 * Math.Sign(e.Delta)),1);
								Stoploss_Btn.Content = (object) $"Stop ATR  {InitialStop_ATRMult}";
							}
							else if(SelectedSLType == SLType_INDICATOR){
								current_indi_id += e.Delta>0 ? 1 : -1;
								double price = 0;
								int id = 0;
								int plot_count = 0;
								#region -- get a count of all plots available --
								foreach(IndicatorBase indi in ChartControl.Indicators){
									try{
										if(indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
	//										abar0 = abar - indi.Displacement;
											for(int i = 0; i<indi.Values.Length; i++){
												var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
												if(cl.IsTransparent()) continue;
												plot_count++;
											}
										}
									}catch(Exception e875){Print(line+"  e875: "+e875.ToString());}
								}
								#endregion
								if(current_indi_id > plot_count) current_indi_id = 1;
								if(current_indi_id < 1) current_indi_id = plot_count;

								int abar = CurrentBars[0];
								nearestprice = double.MinValue;
								foreach(IndicatorBase indi in ChartControl.Indicators){
									#region -- cycle through all indicators and then all plots in each indicator, looking for nearest plot --
									try{
										if(indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
											int abar0 = abar - indi.Displacement;
											for(int i = 0; i<indi.Values.Length; i++){
												var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
												if(cl.IsTransparent()) continue;
												id++;
												if(id != current_indi_id) continue;

												int adj = 0;
												try{
													if(indi.Calculate==Calculate.OnBarClose) {
														adj=1;
														abar0 = abar0-adj;
													}
													else if(!indi.Values[i].IsValidDataPointAt(abar0)) {
														continue;
													}
												}catch{}
												if(id == current_indi_id){
													nearestprice = indi.Values[i].GetValueAt(abar0);
													SelectedPlotName = indi.Plots[i].Name;
													nearestPlotIdx  = i;
													InitialStop_IndiBasisName = indi.Name;
													if(SelectedSLType == SLType_INDICATOR) Stoploss_Btn.Content = "Stop on\n"+InitialStop_IndiBasisName + (indi.Plots[i].Name.CompareTo(InitialStop_IndiBasisName)==0 ? string.Empty : " "+ indi.Plots[i].Name);
													SelectedIndi    = indi;
												}
											}
										}
									}catch(Exception e910){Print(line+"  e910: "+e910.ToString());}
									#endregion
								}
							}
							#endregion
						}else{
							if(item.Content.ToString().Contains("Mkt")) {
								if(e.Delta>0) item.Content = (object) $"BUY Stop";
								else		  item.Content = (object) $"BUY Limit";
							}
							else if(item.Content.ToString().Contains("Stop")) {
								if(e.Delta>0) item.Content = (object) $"BUY Limit";
								else		  item.Content = (object) $"BUY Mkt";
							}
							else if(item.Content.ToString().Contains("Limit")) {
								if(e.Delta>0) item.Content = (object) $"BUY Mkt";
								else		  item.Content = (object) $"BUY Stop";
							}
							string s = item.Content.ToString();
							if     (s.Contains("Mkt"))   this.SelectedBuyEntryType = EntryType_MARKET;
							else if(s.Contains("Limit")) this.SelectedBuyEntryType = EntryType_LIMIT;
							else if(s.Contains("Stop"))  this.SelectedBuyEntryType = EntryType_STOP;
							#region -- Set background of TicksFor_StopOrLimit_Btn --
							if(SelectedBuyEntryType != EntryType_MARKET || SelectedBuyEntryType != EntryType_MARKET){
								TicksFor_StopOrLimit_Btn.Background = Brush_EntryDistanceButtonBkg;
								TicksFor_StopOrLimit_Btn.Foreground = Brush_EntryDistanceButtonText;
								if(SelectedBuyEntryType == EntryType_STOP)
									TicksFor_StopOrLimit_Btn.Content    = (object)($"Stop Entry\n{this.EntryStopTicksOffset}-tick");
								else if(SelectedBuyEntryType == EntryType_LIMIT)
									TicksFor_StopOrLimit_Btn.Content    = (object)($"Limit Entry\n{this.EntryLimitTicksOffset}-tick");
							}else{
								TicksFor_StopOrLimit_Btn.Background = Brushes.DimGray;
								TicksFor_StopOrLimit_Btn.Foreground = Brushes.LightGray;
							}
						}
						#endregion ---------------------------
						ForceRefresh();
					};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				if(pOrientation == IW_RiskManager_UIOrientation.Horizontal){
					System.Windows.Controls.Grid.SetColumn(Buy_Btn, 5);
					System.Windows.Controls.Grid.SetRow(Buy_Btn, 1);
				}else{
					System.Windows.Controls.Grid.SetColumn(Buy_Btn, 1);
					System.Windows.Controls.Grid.SetRow(Buy_Btn, 5);
				}
				MenuGrid.Children.Add(Buy_Btn);
				//---------------------------------------------------------------------
				#endregion

				#region - TicksFor_StopOrLimit_Btn -
//Print("SellBtn created");
				//---------------------------------------------------------------------
				var IsEnabled = SelectedBuyEntryType!=EntryType_MARKET || SelectedSellEntryType!=EntryType_MARKET;
				TicksFor_StopOrLimit_Btn = new System.Windows.Controls.Button
				{
				    Name       = "StopLimTicks"+uID,
				    Content    = (object)($"Stop Entry\n{EntryStopTicksOffset}-tick"),
					FontWeight = FontWeights.Bold,
				    FontSize   = 14,
					Width      = 110,
					ToolTip    = "Click to cycle thru Stop and Limit",
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = IsEnabled ? Brush_EntryDistanceButtonBkg : Brushes.DimGray,
					Foreground = IsEnabled ? Brush_EntryDistanceButtonText : Brushes.LightGray
				};

				#region TicksFor_StopOrLimit_Btn Events
				//---------------------------------------------------------------------
				#region MouseEnter
				TicksFor_StopOrLimit_Btn.Click += delegate(object o, RoutedEventArgs e){
					e.Handled=true;
					int w = 110;
					if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Stop")){
						w = 110;
						TicksFor_StopOrLimit_Btn.Content = (object)($"Limit Entry\n{this.EntryLimitTicksOffset}-tick");
					}else{
						w = 100;
						TicksFor_StopOrLimit_Btn.Content = (object)($"Stop Entry\n{this.EntryStopTicksOffset}-tick");
					}
					if(pOrientation == IW_RiskManager_UIOrientation.Horizontal) MenuGrid.ColumnDefinitions[9].Width = new GridLength(w);
					else if(pOrientation == IW_RiskManager_UIOrientation.Vertical) MenuGrid.ColumnDefinitions[1].Width = new GridLength(120);
					ForceRefresh();
				};
				#endregion
				#region MouseWheel
				TicksFor_StopOrLimit_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
						e.Handled=true;
						if(e.Delta>0){
							if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Stop")){
								EntryStopTicksOffset++;
								TicksFor_StopOrLimit_Btn.Content = (object)($"Stop Entry\n{EntryStopTicksOffset}-tick");
							}else{
								EntryLimitTicksOffset++;
								TicksFor_StopOrLimit_Btn.Content = (object)($"Limit Entry\n{EntryLimitTicksOffset}-tick");
							}
						}else{
							if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Stop")){
								EntryStopTicksOffset = Math.Max(1, EntryStopTicksOffset-1);
								TicksFor_StopOrLimit_Btn.Content = (object)($"Stop Entry\n{EntryStopTicksOffset}-tick");
							}else{
								EntryLimitTicksOffset = Math.Max(1, EntryLimitTicksOffset-1);
								TicksFor_StopOrLimit_Btn.Content = (object)($"Limit Entry\n{EntryLimitTicksOffset}-tick");
							}
						}
						ForceRefresh();
					};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				if(pOrientation == IW_RiskManager_UIOrientation.Horizontal){
//					TicksFor_StopOrLimit_Btn.HorizontalContentAlignment = HorizontalAlignment.Left;
					TicksFor_StopOrLimit_Btn.Width = 100;
					System.Windows.Controls.Grid.SetColumn(TicksFor_StopOrLimit_Btn, 7);
					System.Windows.Controls.Grid.SetRow(TicksFor_StopOrLimit_Btn, 1);
				}else{
//					TicksFor_StopOrLimit_Btn.HorizontalContentAlignment = HorizontalAlignment.Center;
					TicksFor_StopOrLimit_Btn.Width = 120;
					System.Windows.Controls.Grid.SetColumn(TicksFor_StopOrLimit_Btn, 1);
					System.Windows.Controls.Grid.SetRow(TicksFor_StopOrLimit_Btn, 7);
				}
				MenuGrid.Children.Add(TicksFor_StopOrLimit_Btn);
				//---------------------------------------------------------------------
				#endregion

				#region - Sell_Btn -
//Print("SellBtn created");
				//---------------------------------------------------------------------
				Sell_Btn = new System.Windows.Controls.Button
				{
				    Name       = "Sell_Btn"+uID,
				    Content    = SelectedSellEntryType==EntryType_MARKET ? "SELL Mkt" : (SelectedSellEntryType==EntryType_LIMIT ? "SELL Limit" : "SELL Stop"),
					ToolTip    = "Mousewheel to cycle thru\nMkt/Stop/Limit options",
					FontWeight = FontWeights.Bold,
				    FontSize   = 16,
					Width      = 100,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = Brushes.Silver,
					Foreground = Brushes.LightGray
				};

				#region Sell_Btn Events
				//---------------------------------------------------------------------
				Sell_Btn.Click += Sell_Btn_Click;

				#region MouseEnter
				Sell_Btn.MouseEnter += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('S');
					BuySellLabel = "Sell";
					CalculateTarget_Price(entryprice, ShortTargets, ShortSLPts);
					CalculateTarget_OrderQty(ShortTargets, ShortQty);
					foreach(var k in LongTargets.Keys)
						LongTargets[k].Price=double.NaN;
					ForceRefresh();
				};
				#endregion
				#region MouseLeave
				Sell_Btn.MouseLeave += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('S');
					//BuySellLabel = "";
					CalculateTarget_Price(entryprice, ShortTargets, ShortSLPts);
					CalculateTarget_OrderQty(ShortTargets, ShortQty);
					foreach(var k in LongTargets.Keys)
						LongTargets[k].Price=double.NaN;
					ForceRefresh();
				};
				#endregion
				#region MouseWheel
				Sell_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
						e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						if(Keyboard.IsKeyDown(Key.LeftShift)){
							#region -- Adjust the stoploss distance while hovering over the Buy button --
							DT_StoplossChanged = DateTime.Now;
							if(SelectedSLType == SLType_FIXEDTICKS){
								InitialStop_Ticks = Math.Max(1,InitialStop_Ticks + Math.Sign(e.Delta));
								Stoploss_Btn.Content = (object) $"Stop ticks  {InitialStop_Ticks}";
							}
							else if(SelectedSLType == SLType_ATR){
								this.InitialStop_ATRMult = Math.Round(Math.Max(0.1, this.InitialStop_ATRMult + 0.1 * Math.Sign(e.Delta)),1);
								Stoploss_Btn.Content = (object) $"Stop ATR  {InitialStop_ATRMult}";
							}
							else if(SelectedSLType == SLType_INDICATOR){
								current_indi_id += e.Delta>0 ? 1 : -1;
								double price = 0;
								int id = 0;
								int plot_count = 0;
								#region -- get a count of all plots available --
								foreach(IndicatorBase indi in ChartControl.Indicators){
									try{
										if(indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
	//										abar0 = abar - indi.Displacement;
											for(int i = 0; i<indi.Values.Length; i++){
												var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
												if(cl.IsTransparent()) continue;
												plot_count++;
											}
										}
									}catch(Exception e875){Print(line+"  e875: "+e875.ToString());}
								}
								#endregion
								if(current_indi_id > plot_count) current_indi_id = 1;
								if(current_indi_id < 1) current_indi_id = plot_count;

								int abar = CurrentBars[0];
								nearestprice = double.MinValue;
								foreach(IndicatorBase indi in ChartControl.Indicators){
									#region -- cycle through all indicators and then all plots in each indicator, looking for nearest plot --
									try{
										if(indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
											int abar0 = abar - indi.Displacement;
											for(int i = 0; i<indi.Values.Length; i++){
												var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
												if(cl.IsTransparent()) continue;
												id++;
												if(id != current_indi_id) continue;

												int adj = 0;
												try{
													if(indi.Calculate==Calculate.OnBarClose) {
														adj=1;
														abar0 = abar0-adj;
													}
													else if(!indi.Values[i].IsValidDataPointAt(abar0)) {
														continue;
													}
												}catch{}
												if(id == current_indi_id){
													nearestprice = indi.Values[i].GetValueAt(abar0);
													SelectedPlotName = indi.Plots[i].Name;
													nearestPlotIdx  = i;
													InitialStop_IndiBasisName = indi.Name;
													if(SelectedSLType == SLType_INDICATOR) Stoploss_Btn.Content = "Stop on\n"+InitialStop_IndiBasisName + (indi.Plots[i].Name.CompareTo(InitialStop_IndiBasisName)==0 ? string.Empty : " "+ indi.Plots[i].Name);
													SelectedIndi    = indi;
												}
											}
										}
									}catch(Exception e910){Print(line+"  e910: "+e910.ToString());}
									#endregion
								}
							}
							#endregion
						}else{
							if(item.Content.ToString().Contains("Mkt")) {
								if(e.Delta>0) item.Content = (object) $"SELL Stop";
								else		  item.Content = (object) $"SELL Limit";
							}
							else if(item.Content.ToString().Contains("Stop")) {
								if(e.Delta>0) item.Content = (object) $"SELL Limit";
								else		  item.Content = (object) $"SELL Mkt";
							}
							else if(item.Content.ToString().Contains("Limit")) {
								if(e.Delta>0) item.Content = (object) $"SELL Mkt";
								else		  item.Content = (object) $"SELL Stop";
							}
							string s = item.Content.ToString();
							int w = 90;
							if     (s.Contains("Mkt"))
								this.SelectedSellEntryType = EntryType_MARKET;
							else if(s.Contains("Stop")){
								this.SelectedSellEntryType = EntryType_STOP;
								w = 95;
							} else if(s.Contains("Limit")) {
								this.SelectedSellEntryType = EntryType_LIMIT;
								w = 100;
							}
							if(pOrientation == IW_RiskManager_UIOrientation.Horizontal) MenuGrid.ColumnDefinitions[7].Width = new GridLength(w);
							else if(pOrientation == IW_RiskManager_UIOrientation.Vertical) {
								Buy_Btn.Width = 120;
								Sell_Btn.Width = 120;
								MenuGrid.ColumnDefinitions[1].Width = new GridLength(120);
							}
							#region -- Set background of TicksFor_StopOrLimit_Btn --
							if(SelectedBuyEntryType != EntryType_MARKET || SelectedSellEntryType != EntryType_MARKET){
								TicksFor_StopOrLimit_Btn.Background = Brush_EntryDistanceButtonBkg;
								TicksFor_StopOrLimit_Btn.Foreground = Brush_EntryDistanceButtonText;
								if(SelectedSellEntryType == EntryType_STOP)
									TicksFor_StopOrLimit_Btn.Content = (object)($"Stop Entry\n{this.EntryStopTicksOffset}-tick");
								else if(SelectedSellEntryType == EntryType_LIMIT)
									TicksFor_StopOrLimit_Btn.Content = (object)($"Limit Entry\n{this.EntryLimitTicksOffset}-tick");
							}else{
								TicksFor_StopOrLimit_Btn.Background = Brushes.DimGray;
								TicksFor_StopOrLimit_Btn.Foreground = Brushes.LightGray;
							}
						}
						#endregion ---------------------------
						ForceRefresh();
					};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				if(pOrientation == IW_RiskManager_UIOrientation.Horizontal){
					System.Windows.Controls.Grid.SetColumn(Sell_Btn, 9);
					System.Windows.Controls.Grid.SetRow(Sell_Btn, 1);
				}else{
					System.Windows.Controls.Grid.SetColumn(Sell_Btn, 1);
					System.Windows.Controls.Grid.SetRow(Sell_Btn, 9);
				}
				MenuGrid.Children.Add(Sell_Btn);
				//---------------------------------------------------------------------
				#endregion

				BuySellPanel.Children.Add(MenuGrid);
            }
            #endregion

            #region - CONTROL HANDLERS -

			#region - Buy_Btn_Click -
            void Buy_Btn_Click(object sender, EventArgs e)
            {
				if(pMode == IW_RiskManager_Modes.Disabled) return;
				//Print("Submitting entry BUY order on '"+Instrument.FullName+"'");
				var ordertype   = OrderType.Market;
				if(SelectedBuyEntryType == EntryType_LIMIT) ordertype= OrderType.Limit;
				if(SelectedBuyEntryType == EntryType_STOP)  ordertype= OrderType.StopMarket;
				string oco_id  = $"Lx-{DateTime.Now.ToBinary()}";
				if(!pEnableOCO) oco_id=string.Empty;
	try{
				if(ATMStrategyName.Trim().Length>0 && ATMStrategyName.ToUpper()!="N/A"){
					var ATMorder = myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Buy, 
									ordertype, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,LongQty)), entryprice, entryprice, string.Empty, "Entry", Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
				} else {
					//do not permit rapid re-entry of an entry order...accidental double-clicks
					var ts = new TimeSpan(DateTime.Now.Ticks-ClickToEnterTime.Ticks);
					if(ts.TotalSeconds<5) {
						Log("2nd buy button click ignored...market entries must be no closer than 5-seconds apart", LogLevel.Information);
						return;
					}
					ClickToEnterTime = DateTime.Now;

					var OrdersList = new List<Order>();
						OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Buy, 
									ordertype, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,LongQty)), entryprice, entryprice, string.Empty, "BuyEntry", Core.Globals.MaxDate, null));
//Print("SL at: "+LowerSL.ToString());
					foreach(var tgt in LongTargets.Values){
//Print(tgt.MarkerTag+" Tgt at: "+tgt.Price.ToString()+"  qty: "+tgt.OrderQty);
						if(tgt.OrderQty>0 && !double.IsNaN(tgt.Price)){
							var oco_id_this_tgt = pEnableOCO ? $"{tgt.MarkerTag}-{oco_id}" : string.Empty;
							OrdersList.Add(myAccount.CreateOrder(Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Sell, 
									OrderType.StopMarket, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)tgt.OrderQty, 0, LowerSL, 
									oco_id_this_tgt, 
									$"{tgt.MarkerTag}-SL", Core.Globals.MaxDate, null));
							OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Sell, 
									OrderType.Limit, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)tgt.OrderQty, tgt.Price, 0, 
									oco_id_this_tgt, 
									tgt.MarkerTag, Core.Globals.MaxDate, null));
						}
					}
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>{	myAccount.Submit(OrdersList.ToArray()); 
					}));
				}
	}catch(Exception eorder){
//		Print("RiskManager error: "+eorder.ToString());
		if(LongQty==0) Log("Order Rejected - your risk parameters are calculating a zero quantity", LogLevel.Warning);
		else Log(Log_Order_Reject_Message, LogLevel.Warning);
		MessageTime = DateTime.Now;
		Draw.TextFixed(this,"OrderError", Order_Reject_Message, TextPosition.Center,Brushes.Magenta,new SimpleFont("Arial",14),Brushes.DimGray,Brushes.Black,100);
		ForceRefresh();
	}
            }
			#endregion

			#region - Sell_Btn_Click -
            void Sell_Btn_Click(object sender, EventArgs e)
            {
				if(pMode == IW_RiskManager_Modes.Disabled) return;
//				Print("Submitting entry SELL order on '"+Instrument.FullName+"'");
				var ordertype   = OrderType.Market;
				if(SelectedSellEntryType == EntryType_LIMIT) ordertype= OrderType.Limit;
				if(SelectedSellEntryType == EntryType_STOP)  ordertype= OrderType.StopMarket;
				string oco_id  = $"Sx-{DateTime.Now.ToBinary()}";
				if(!pEnableOCO) oco_id=string.Empty;
	try{
				if(ATMStrategyName.Trim().Length>0 && ATMStrategyName.ToUpper()!="N/A"){
					var ATMorder = myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.SellShort, 
									ordertype, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,ShortQty)), entryprice, entryprice, string.Empty, "Entry", Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
				} else {
//Print(1087);Print("min: "+MinContracts+"  max: "+MaxContracts+"   ShortQty: "+ShortQty+"  entryprice: "+entryprice);
					//do not permit rapid re-entry of an entry order...accidental double-clicks
					var ts = new TimeSpan(DateTime.Now.Ticks-ClickToEnterTime.Ticks);
					if(ts.TotalSeconds<5) {
						Log("2nd sell button click ignored...market entries must be no closer than 5-seconds apart", LogLevel.Information);
						return;
					}
					ClickToEnterTime = DateTime.Now;

					var OrdersList = new List<Order>();
					OrdersList.Add(myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.SellShort, 
								ordertype, 
								OrderEntry.Automated, 
								TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,ShortQty)), entryprice, entryprice, string.Empty, "SellEntry", Core.Globals.MaxDate, null));
//Print("SL at: "+LowerSL.ToString());
//Print(1096);
					foreach(var tgt in ShortTargets.Values){
//Print(tgt.MarkerTag+" Tgt at: "+tgt.Price.ToString()+"  qty: "+tgt.OrderQty);
//Print(1099);
						if(tgt.OrderQty>0 && !double.IsNaN(tgt.Price)){
//Print(1101);
							var oco_id_this_tgt = pEnableOCO ? $"{tgt.MarkerTag}-{oco_id}" : string.Empty;
//Print(1103);
							OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.BuyToCover, 
									OrderType.StopMarket, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)tgt.OrderQty, 0, UpperSL, 
									oco_id_this_tgt,
									$"{tgt.MarkerTag}-SL", Core.Globals.MaxDate, null));
							OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.BuyToCover, 
									OrderType.Limit, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)tgt.OrderQty, tgt.Price, 0, 
									oco_id_this_tgt, 
									tgt.MarkerTag, Core.Globals.MaxDate, null));
						}
					}
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>{	myAccount.Submit(OrdersList.ToArray()); 
					}));
				}
	}catch(Exception eorder){
		if(ShortQty==0) Log("Order Rejected - your risk parameters are calculating a zero quantity", LogLevel.Warning);
		else Log(Log_Order_Reject_Message, LogLevel.Warning);
		MessageTime = DateTime.Now;
		Draw.TextFixed(this,"OrderError",Order_Reject_Message, TextPosition.Center,Brushes.Magenta,new SimpleFont("Arial",14),Brushes.DimGray,Brushes.Black,100);
		ForceRefresh();
	}
            }
            #endregion

            #region - OnDragDelta -
			void OnDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
			{
			    try
			    {
					double left, top, right, bottom;
					left = top = right = bottom = defaultMargin;

					if (BuySellPanel.HorizontalAlignment == HorizontalAlignment.Left) 
						left = BuySellPanel.Margin.Left + e.HorizontalChange;
					else 
						right = BuySellPanel.Margin.Right - e.HorizontalChange;

					if (BuySellPanel.VerticalAlignment == VerticalAlignment.Top) 
						top = BuySellPanel.Margin.Top + e.VerticalChange;
					else 
						bottom = BuySellPanel.Margin.Bottom - e.VerticalChange;

					left   = Math.Max(0, left);
					top    = Math.Max(0, top);
					right  = Math.Max(0, right);
					bottom = Math.Max(0, bottom);

					BuySellPanel.Margin = new Thickness(left, top, right, bottom);
				}
				catch { }
			}
			#endregion

			#endregion

		#endregion

//==============================================================================================
		#region Properties
		internal class LoadATMStrategyNames : StringConverter
		{
			#region LoadATMStrategyNames
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

				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("N/A");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						list.Add(fi.Name.Replace(".xml",string.Empty).Trim());
					}
				}
				return new StandardValuesCollection(list.ToArray());
			}
			#endregion
		}
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
					else NinjaScript.Log($"'{Account.All[i].Name}' connection status: {Account.All[i].ConnectionStatus}", LogLevel.Information);
		        }
				
		        return new TypeConverter.StandardValuesCollection(list);
		    }

		    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		    { return true; }
			#endregion
		}
        [NinjaScriptProperty]
		[TypeConverter(typeof(LoadAccountNameList))]
		[Display(Name = "Account Name", GroupName = "Order Parameters", Description = "", Order = 10)]
		public string SelectedAccountName	{get;set;}

		[NinjaScriptProperty]
		[Display(Name = "Default Entry Order Type", GroupName = "Order Parameters", Description = "Set to Market, Limit or Stop", Order = 15)]
		public string pDefaultOrderType {get;set;}
		[NinjaScriptProperty]
		[Display(Name = "Ticks on LIMIT", GroupName = "Order Parameters", Description = "Distance (in ticks) from market price to LIMIT entry price (only used when you submit a LIMIT entry order", Order = 20)]
		public int EntryLimitTicksOffset { get; set; }
		[NinjaScriptProperty]
		[Display(Name = "Ticks on STOP", GroupName = "Order Parameters", Description = "Distance (in ticks) from market price to STOP entry price (only used when you submit a STOP entry order", Order = 30)]
		public int EntryStopTicksOffset { get; set; }
		[NinjaScriptProperty]
		[Display(Name = @"OCO enabled", GroupName = "Order Parameters", Description = @"Submit targets and SL as OCO orders", Order = 40)]
		public bool pEnableOCO {get;set;}

		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadATMStrategyNames))]
		[Display(Name = "ATM Strategy Name", GroupName = "ATM Parameters", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template", Order = 10)]
		public string ATMStrategyName { get; set; }
//		[NinjaScriptProperty]
//		[Display(Name = "Use ATM Strategy SL?", GroupName = "ATM Parameters", Description = "If enabled, it will use the SL distance defined in the ATM Strategy Template.  Otherwise, it will use the SL distance you select in the button interface on the chart", Order = 20)]
//		public bool pUseAtmStrategySL { get;set; }

		[NinjaScriptProperty]
		[Display(Name = "Target basis", GroupName = "Targets", Description = "Target distance basis - Ticks or RiskMultiple", Order = 5)]
		public IW_RiskManager_TargetDistanceTypes pTargetDistanceType { get; set; }

		[Range(0,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "T1 ticks", GroupName = "Targets", Description = "T1 distance, in ticks", Order = 10)]
		public int pT1_Ticks { get; set; }
		[Range(0,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "T2 ticks", GroupName = "Targets", Description = "T2 distance, in ticks", Order = 11)]
		public int pT2_Ticks { get; set; }
		[Range(0,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "T3 ticks", GroupName = "Targets", Description = "T3 distance, in ticks", Order = 12)]
		public int pT3_Ticks { get; set; }

		[Range(0,double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "T1 Reward:Risk", GroupName = "Targets", Description = "T1 distance as multiple of SL distance", Order = 20)]
		public double RRT1 { get; set; }

		[Range(0,double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "T2 Reward:Risk", GroupName = "Targets", Description = "T2 distance as multiple of SL distance, '0' to turn-off T2", Order = 30)]
		public double RRT2 { get; set; }

		[Range(0,double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "T3 Reward:Risk", GroupName = "Targets", Description = "T3 distance as multiple of SL distance, '0' to turn-off T3", Order = 40)]
		public double RRT3 { get; set; }

		[Range(0,100)]
		[NinjaScriptProperty]
		[Display(Name = "T1 Qty %", GroupName = "Targets", Description = "Quanity to exit at T1 as a percent of total position", Order = 50)]
		public double QtyT1Pct { get; set; }

		[Range(0,100)]
		[NinjaScriptProperty]
		[Display(Name = "T2 Qty %", GroupName = "Targets", Description = "Quanity to exit at T2 as a percent of total position", Order = 60)]
		public double QtyT2Pct { get; set; }

		[Range(0,100)]
		[NinjaScriptProperty]
		[Display(Name = "T3 Qty %", GroupName = "Targets", Description = "Quanity to exit at T3 as a percent of total position", Order = 70)]
		public double QtyT3Pct { get; set; }

//		public string pRiskStatement = "2.5%";
		[NinjaScriptProperty]
		[Range(0.1,100)]
		[Display(Name = "Risk %", GroupName = "Risk Parameters", Description = "", Order = 10)]
		public double PctRisk {get;set;}
//		{
//			#region Risk Statement get/set
//			get { return pRiskStatement; }
//			set { string s = value; 
//				Print("s: "+s);
//				if(s.Contains("%")){
//					s = KeepOnlyTheseCharacters(s, "0123456789.");
//					PctRisk = double.Parse(s)/100.0;
//					DollarRisk = -1;
//					pRiskStatement = s+"%";
//				}
//				else if(s.Contains("$")){
//					s = KeepOnlyTheseCharacters(s, "0123456789.");
//					PctRisk = -1;
//					DollarRisk = double.Parse(s);
//					pRiskStatement = "$"+s;
//				}else
//					pRiskStatement = "0.1%";
//			}
//			#endregion
//		}
		[Range(0,double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "AccountBalance", GroupName = "Risk Parameters", Description = "Account size in currency units", Order = 20)]
		public double AccountBalance { get; set; }

		[Range(1,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "MaxContracts", GroupName = "Risk Parameters", Description = "Max number of contracts permitted", Order = 30)]
		public int MaxContracts { get; set; }

		[Range(0,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "MinContracts", GroupName = "Risk Parameters", Description = "Min number of contracts permitted", Order = 35)]
		public int MinContracts { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Use HLine as SL", GroupName = "Risk Parameters", Description = "Use specific HLine", Order = 40)]
//		public bool UseHLine {get;set;}
		[NinjaScriptProperty]
		[Display(Name = "HLine Tag", GroupName = "Risk Parameters", Description = "Use specific HLine", Order = 50)]
		public string HLineTag {get;set;}

		[Range(0,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Initial Stop Ticks", GroupName = "Risk Parameters", Description = "Use specific number of ticks for the initial stop", Order = 60)]
		public int InitialStop_Ticks {get;set;}

		[Range(0.01,double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Initial Stop ATR mult", GroupName = "Risk Parameters", Description = "Use specific multiple of ATR(14) for the initial stop", Order = 70)]
		public double InitialStop_ATRMult {get;set;}

		[NinjaScriptProperty]
		[Display(Order = 5, Name = "Mode: Disabled/Pct/Fixed", GroupName = "Visual Options", Description = "Enable the indicator")]
		public IW_RiskManager_Modes pMode {get;set;}

		[Display(Order = 7, Name = "Pennant Loc", GroupName = "Visual Options", Description = "Left of current price, Right of current price")]
		public IW_RiskManager_PennantLocs pPennantLocs {get;set;}

		[Display(Order = 10, Name = "Show buy and sell line?", GroupName = "Visual Options", Description = "Shows both the current buying price and current selling price lines")]
		public bool pShowBuySellPriceLines {get;set;}
		
		[Display(Order = 17, Name = "Show bid and ask dots?", GroupName = "Visual Options", Description = "Shows both the current bid and current ask as dots immediately to the right of current price bar")]
		public bool pShowBidAskDots {get;set;}

		[Range(0,float.MaxValue)]
		[Display(Order = 20, Name = "Line length", GroupName = "Visual Options", Description = "Length (in pixels) of the order lines")]
		public float LineLengthF {get;set;}

		[Range(0,100)]
		[Display(Order = 30, Name = "Marker Opacity", GroupName = "Visual Options", Description = "Opacity of the order markers and lines (0=transparent, 100=opaque")]
		public int MarkerOpacity {get;set;}

		[Display(Order = 40, Name = "Show $ risked", GroupName = "Visual Options", Description = "Show amount risked (in currency units).")]
		public bool ShowDollarsRisked {get;set;}

		[Display(Order = 50, Name = "Show $ reward", GroupName = "Visual Options", Description = "Show amount rewarded at targets (in currency units).")]
		public bool ShowDollarsReward {get;set;}

		[Display(Order = 60, Name = "UI orientation", GroupName = "Visual Options", Description = "Horizontal or Vertical arrangement of UI buttons")]
		public IW_RiskManager_UIOrientation pOrientation {get;set;}
		
		[Display(Order = 70, Name = "Show Lag Time", GroupName = "Visual Options", Description = "Location of lag time (time since last price tick)")]
		public IW_RiskManager_LagTimeLocation pLagTimerLocation {get;set;}
		
		[Display(Order = 80, Name = "Lag Timer Font", GroupName = "Visual Options", Description = "")]
		public SimpleFont pLagWarningFont {get;set;}

		[Display(Order = 90, Name = "Lag Time seconds", GroupName = "Visual Options", Description = "Max number of seconds of lag before Lag Timer message is displayed")]
		public int pLagWarningSeconds {get;set;}
		
		[XmlIgnore]
		[Display(Order = 100, Name = "SL Button Bkg", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_SLButtonBkg {get;set;}
			[Browsable(false)]
			public string Brush_SLButtonBkg_BrushSerialize{	get { return Serialize.BrushToString(Brush_SLButtonBkg); } set { Brush_SLButtonBkg = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 110, Name = "SL Button Text", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_SLButtonText {get;set;}
			[Browsable(false)]
			public string Brush_SLButtonText_BrushSerialize{	get { return Serialize.BrushToString(Brush_SLButtonText); } set { Brush_SLButtonText = Serialize.StringToBrush(value); }}
		
		[XmlIgnore]
		[Display(Order = 120, Name = "Risk Button Bkg", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_RiskButtonBkg {get;set;}
			[Browsable(false)]
			public string Brush_RiskButtonBkg_BrushSerialize{	get { return Serialize.BrushToString(Brush_RiskButtonBkg); } set { Brush_RiskButtonBkg = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 130, Name = "Risk Button Text", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_RiskButtonText {get;set;}
			[Browsable(false)]
			public string Brush_RiskButtonText_BrushSerialize{	get { return Serialize.BrushToString(Brush_RiskButtonText); } set { Brush_RiskButtonText = Serialize.StringToBrush(value); }}
		
		[XmlIgnore]
		[Display(Order = 140, Name = "Buy Button Bkg", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_BuyButtonBkg {get;set;}
			[Browsable(false)]
			public string Brush_BuyButtonBkg_BrushSerialize{	get { return Serialize.BrushToString(Brush_BuyButtonBkg); } set { Brush_BuyButtonBkg = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 150, Name = "Buy Button Text", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_BuyButtonText {get;set;}
			[Browsable(false)]
			public string Brush_BuyButtonText_BrushSerialize{	get { return Serialize.BrushToString(Brush_BuyButtonText); } set { Brush_BuyButtonText = Serialize.StringToBrush(value); }}
		
		[XmlIgnore]
		[Display(Order = 160, Name = "EntryDistance Button Bkg", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_EntryDistanceButtonBkg {get;set;}
			[Browsable(false)]
			public string Brush_EntryDistanceButtonBkg_BrushSerialize{	get { return Serialize.BrushToString(Brush_EntryDistanceButtonBkg); } set { Brush_EntryDistanceButtonBkg = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 170, Name = "EntryDistance Button Text", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_EntryDistanceButtonText {get;set;}
			[Browsable(false)]
			public string Brush_EntryDistanceButtonText_BrushSerialize{	get { return Serialize.BrushToString(Brush_EntryDistanceButtonText); } set { Brush_EntryDistanceButtonText = Serialize.StringToBrush(value); }}
		
		[XmlIgnore]
		[Display(Order = 180, Name = "Sell Button Bkg", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_SellButtonBkg {get;set;}
			[Browsable(false)]
			public string Brush_SellButtonBkg_BrushSerialize{	get { return Serialize.BrushToString(Brush_SellButtonBkg); } set { Brush_SellButtonBkg = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 190, Name = "Sell Button Text", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_SellButtonText {get;set;}
			[Browsable(false)]
			public string Brush_SellButtonText_BrushSerialize{	get { return Serialize.BrushToString(Brush_SellButtonText); } set { Brush_SellButtonText = Serialize.StringToBrush(value); }}
		#endregion
//==============================================================================================
		private static string KeepOnlyTheseCharacters(string instr, string CharactersToKeep){
		string ret = string.Empty;
		char[] str = instr.ToCharArray(0,instr.Length);
		for(int i = 0; i<str.Length; i++) if(CharactersToKeep.Contains(str[i].ToString())) ret = string.Concat(ret,str[i].ToString());
		return ret;
	}
//==============================================================================================
		private double RoundToTick(double p){
			int x = (int)Math.Round(p/TickSize,0);
			return x * TickSize;
		}
//==============================================================================================
		private void DrawEllipse_SL(double center_price, bool OffsetUpward, SharpDX.Direct2D1.Brush brush, int abar, ChartScale scale, string Label, float LineWidth){
			float x = ChartControl.GetXByBarIndex(ChartBars, abar);
			if(pPennantLocs == IW_RiskManager_PennantLocs.Left){
				x = x - ChartControl.Properties.BarDistance * 3f;
			}else
				x = x + 70f;
			float y = scale.GetYByValue(center_price);
			if(Label.Length>0){
				#region Draw text - trade size
				brush.Opacity = MarkerOpacity/100.0f;
	            var font       = new SimpleFont("Arial",12) {Bold=true};
	            var textFormat = font.ToDirectWriteTextFormat();
				var textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, Label, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
				if(pPennantLocs == IW_RiskManager_PennantLocs.Left){
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+this.LineLengthF,y), brush, LineWidth);
				}else if(pPennantLocs == IW_RiskManager_PennantLocs.Right){
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f+this.LineLengthF,y), brush, LineWidth);
				}
				if(OffsetUpward) y = y - textFormat.FontSize-2f;
//				if(pPennantLocs == IW_RiskManager_PennantLocs.Right){
//					x = ChartControl.GetXByBarIndex(ChartBars, abar) + ChartControl.Properties.BarDistance + textLayout.Metrics.Width/2f;
//				}
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),textLayout.Metrics.Width*0.8f,textLayout.Metrics.Height*0.8f), brush);
				RenderTarget.DrawTextLayout(new SharpDX.Vector2(x - textLayout.Metrics.Width/2f,y-textLayout.Metrics.Height/2f), textLayout, blackDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				textFormat.Dispose(); textFormat=null;
				textLayout.Dispose(); textLayout=null;
				#endregion
			}else{
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),15f,8f), brush);
			}
		}
		private void DrawEllipse(double center_price, SharpDX.Direct2D1.Brush brush, int abar, ChartScale scale, string Label, float LineWidth){
			float x = ChartControl.GetXByBarIndex(ChartBars, abar)- ChartControl.Properties.BarDistance * 3f;
//			if(pPennantLocs == IW_RiskManager_PennantLocs.Left){
//				x = x - ChartControl.Properties.BarDistance * 3f;
//			}else
//				x = x + 10f;
			float y = scale.GetYByValue(center_price);
			if(Label.Length>0){
				#region Draw text - trade size
				brush.Opacity = MarkerOpacity/100.0f;
	            var font       = new SimpleFont("Arial",12) {Bold=true};
	            var textFormat = font.ToDirectWriteTextFormat();
				var textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, Label, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
				if(pPennantLocs == IW_RiskManager_PennantLocs.Left){
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+this.LineLengthF,y), brush, LineWidth);
				}else if(pPennantLocs == IW_RiskManager_PennantLocs.Right){
					x = ChartControl.GetXByBarIndex(ChartBars, abar) + ChartControl.Properties.BarDistance + textLayout.Metrics.Width;
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f+LineLengthF,y), brush, LineWidth);
				}
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),textLayout.Metrics.Width*0.8f,textLayout.Metrics.Height*0.8f), brush);
				RenderTarget.DrawTextLayout(new SharpDX.Vector2(x - textLayout.Metrics.Width/2f,y-textLayout.Metrics.Height/2f), textLayout, blackDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				textFormat.Dispose(); textFormat=null;
				textLayout.Dispose(); textLayout=null;
				#endregion
			}else{
				if(pPennantLocs == IW_RiskManager_PennantLocs.Right){
					x = ChartControl.GetXByBarIndex(ChartBars, abar) + ChartControl.Properties.BarDistance + 15f;
				}
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),15f,8f), brush);
			}
		}
//==============================================================================================
		private void CalculateTarget_OrderQty(ConcurrentDictionary<int,TargetData> targs, double PositionSize){
			double total = 0;
			for(int i = 0; i<targs.Count; i++){
				targs[i].OrderQty = Math.Round(targs[i].QtyPct * PositionSize,0);
//Print(i+" tgt    qty: "+targs[i].OrderQty+"  QtyPct: "+targs[i].QtyPct+"  PosSize: "+PositionSize);
				total=total+targs[i].OrderQty;
			}
//Print("total: "+total);
			if(total<PositionSize){
				if(PositionSize==1)
					targs[0].OrderQty=PositionSize;
				else{
					for(int i = targs.Count-1; i>0; i--){
						if(targs[i].RRratio>0) {
							targs[i].OrderQty = targs[i].OrderQty + PositionSize-total;
							break;
						}
					}
				}
			}
//Print("t1 qty: "+targs[0].OrderQty);
		}
//==============================================================================================
		private void CalculateTarget_Price(double entryprice, ConcurrentDictionary<int,TargetData> targs, double SLPts){
			DateTime t0  = Times[0].GetValueAt(CurrentBars[0]);
			double distance = 0;
			for(int i = 0; i<targs.Count; i++){
				if(ATMStrategyEnabled){
					if(i==0) distance = ATM_T1_Ticks * TickSize;
					if(i==1 && ATM_T2_Ticks!=int.MinValue) distance = ATM_T2_Ticks * TickSize;
					if(i==2 && ATM_T3_Ticks!=int.MinValue) distance = ATM_T3_Ticks * TickSize;
					LongTargets[0].Ticks = ATM_T1_Ticks;
					LongTargets[1].Ticks = ATM_T2_Ticks;
					LongTargets[2].Ticks = ATM_T3_Ticks;
					ShortTargets[0].Ticks = ATM_T1_Ticks;
					ShortTargets[1].Ticks = ATM_T2_Ticks;
					ShortTargets[2].Ticks = ATM_T3_Ticks;
				}else{
					LongTargets[0].Ticks = pT1_Ticks;
					LongTargets[1].Ticks = pT2_Ticks;
					LongTargets[2].Ticks = pT3_Ticks;
					ShortTargets[0].Ticks = pT1_Ticks;
					ShortTargets[1].Ticks = pT2_Ticks;
					ShortTargets[2].Ticks = pT3_Ticks;
					if(pTargetDistanceType == IW_RiskManager_TargetDistanceTypes.Ticks && targs[i].Ticks > 0)          distance = targs[i].Ticks  * TickSize;
					if(pTargetDistanceType == IW_RiskManager_TargetDistanceTypes.RiskMultiple && targs[i].RRratio > 0) distance = Math.Abs(SLPts) * targs[i].RRratio;
				}

				if(targs[i].MarkerTag.Contains("S")){
					targs[i].Price = RoundToTick(entryprice - distance);
//Print(targs[i].MarkerTag+"  rr: "+targs[i].RRratio+"   dist: "+distance+"   price: "+targs[i].Price);
						//Draw.Diamond(this, targs[i].MarkerTag, false, t0, targs[i].Price, Brushes.Green);
				}else if(targs[i].MarkerTag.Contains("L")){
					targs[i].Price = RoundToTick(entryprice + distance);
//Print(targs[i].MarkerTag+"  rr: "+targs[i].RRratio+"   dist: "+distance+"   price: "+targs[i].Price);
						//Draw.Diamond(this, targs[i].MarkerTag, false, t0, targs[i].Price, Brushes.Red);
				}else Print("Unknown: "+targs[i].MarkerTag+"  rr: "+targs[i].RRratio+"   dist: "+distance+"   price: "+targs[i].Price);
			}
		}
//==============================================================================================
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
try{
line=1638;
		if (!IsVisible) return;double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
		base.OnRender(chartControl, chartScale);

		if(MessageTime!=DateTime.MinValue){
			var ts = new TimeSpan(DateTime.Now.Ticks - MessageTime.Ticks);
			if(ts.Seconds > 15 || ChartControl.Properties.ChartTraderVisibility == ChartTraderVisibility.Visible) {
				RemoveDrawObject("preexist");
				RemoveDrawObject("OrderError");
				MessageTime = DateTime.MinValue;
			}
		}
		if(pLagTimerLocation != IW_RiskManager_LagTimeLocation.NoDisplay){
			#region -- Lag Timer --
			var ts = new TimeSpan(DateTime.Now.Ticks-TimeOfLastTick.Ticks);
			if(Math.Abs(ts.TotalSeconds) > pLagWarningSeconds){
				string lag_msg = $"Data is lagging by {Math.Abs(ts.TotalSeconds):0}-seconds";
				switch (pLagTimerLocation){
					case IW_RiskManager_LagTimeLocation.TopLeft:
						Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.TopLeft, Brushes.White, pLagWarningFont, Brushes.DimGray, Brushes.Maroon, 100);break;
					case IW_RiskManager_LagTimeLocation.TopRight:
						Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.TopRight, Brushes.White, pLagWarningFont, Brushes.DimGray, Brushes.Maroon, 100);break;
					case IW_RiskManager_LagTimeLocation.Center:
						Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.Center, Brushes.White, pLagWarningFont, Brushes.DimGray, Brushes.Maroon, 100);break;
					case IW_RiskManager_LagTimeLocation.BottomLeft:
						Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.BottomLeft, Brushes.White, pLagWarningFont, Brushes.DimGray, Brushes.Maroon, 100);break;
					case IW_RiskManager_LagTimeLocation.BottomRight:
						Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.BottomRight, Brushes.White, pLagWarningFont, Brushes.DimGray, Brushes.Maroon, 100);break;
				}
			}else RemoveDrawObject("LagWarning");
			#endregion
		}
line=1843;
		if(pMode == IW_RiskManager_Modes.Disabled) return;

		char EntryDirection_OnHover = ' ';
//		lock(BuyLock){
//			EntryDirection_OnHover = EntryDirection_OnHoverList[0];
//		}
//Print("\n\nmouse xy: "+mouseX+":"+mouseY);
		int abar = Bars.GetBar(ChartControl.GetTimeByX(mouseX - (int)(ChartControl.Properties.BarDistance/2)));
		mouseX   = ChartControl.GetXByBarIndex(ChartBars,abar);
		double price_clicked   = chartScale.GetValueByY(mouseY);
		int nearestRMB = -1;
		int abar0 = 0;
		double diff, price;
		double nearestdiff = double.MaxValue;
		bool IsReady       = false;
		UpperSL = double.MinValue;
		LowerSL = double.MaxValue;
line=1862;
//		Print("TradeCalc: "+DrawObjects.Count);
		if(!UseAtmStrategySL){
			#region -- Calculate nearest price based on SelectedSLType --
			if(this.SelectedSLType == SLType_HLINE){
//Print("\nHLine stop type selected");
line=1868;
				nearestprice = double.MinValue;
				foreach (dynamic hline in DrawObjects.ToList())
				{
					if (hline.ToString().EndsWith(".HorizontalLine"))
					{
//Print("HLine tag: "+hline.Tag);
						#region cycle through all lines looking for one that contains the HLineTag value
						if(hline.Tag.Contains(HLineTag)) 
						{
							price = hline.StartAnchor.Price;
							diff = Math.Abs(price-nearestprice);
							if(nearestprice == double.MinValue || diff < nearestdiff){
								nearestdiff  = diff;
								nearestprice = hline.StartAnchor.Price;
								InitialStop_IndiBasisName = hline.Name;
							}
						}
						#endregion
					}
				}
			}else if(SelectedSLType == SLType_INDICATOR){
//Print("Indicator stop type selected");
				#region Find the nearest plot the user has selected
//Print("PriceClicked: "+price_clicked);
				if(IsShiftPressed || IsCtrlPressed){//find the nearest plot the user has selected
//Print("ShiftPressed "+DateTime.Now.ToShortTimeString());
					IsShiftPressed = false;
					IsCtrlPressed = false;
					RemoveDrawObject("u");
					RemoveDrawObject("l");
					foreach(IndicatorBase indi in ChartControl.Indicators){
						#region cycle through all indicators and then all plots in each indicator, looking for nearest plot
						try{
							IsReady = indi.State==State.DataLoaded || indi.State==State.Realtime;
//Print("IsReady: "+IsReady.ToString());
//Print("IsShiftPressed: "+IsShiftPressed.ToString());
//Print("IsCtrlPressed: "+IsCtrlPressed.ToString());

							if(IsReady && indi.Name != this.Name && indi.Panel <= PRICE_PANEL_ID_NUMBER){
								abar0 = abar - indi.Displacement;
								for(int i = 0; i<indi.Values.Length; i++){
line=1910;
									var cl = (System.Windows.Media.SolidColorBrush)indi.Plots[i].Brush;
									if(cl.IsTransparent()) continue;
									int adj = 0;
									try{
line=1915;
										if(indi.Calculate==Calculate.OnBarClose) adj=1;
										else if(!indi.Values[i].IsValidDataPointAt(abar0)) continue;
									}catch{}
line=1919;
									abar0 = abar0-adj;
									price = indi.Values[i].GetValueAt(abar0);
									diff  = Math.Abs(price-price_clicked);
									if(diff < nearestdiff) {
line=1924;
										nearestdiff     = diff;
										nearestPlotIdx  = i;
										nearestprice    = price;
										SelectedPlotName = indi.Plots[i].Name;
										InitialStop_IndiBasisName = indi.Name;
										if(SelectedSLType == SLType_INDICATOR) Stoploss_Btn.Content = "Stop on\n"+InitialStop_IndiBasisName;
										SelectedIndi    = indi;
										nearestRMB      = CurrentBar-adj;
//Print("\n\nNearest name: "+InitialStop_IndiBasisName+"   price: "+nearestprice);
									}
								}
							}
						}catch(Exception e308){Print(line+"  e308: "+e308.ToString());}
						#endregion
					}
					bool c1 = SelectedIndi!=null;
					bool c2 = c1 && (SelectedIndi.State==State.DataLoaded || SelectedIndi.State==State.Realtime);
					if(nearestPlotIdx>=0 && c2){
						abar0 = abar - SelectedIndi.Displacement;
						int adj = 0;
						try{
							if(SelectedIndi.Calculate==Calculate.OnBarClose) adj=1;
						}catch{}
						abar0 = abar0-adj;
						if(SelectedIndi.Values[nearestPlotIdx].IsValidDataPointAt(abar0))
							nearestprice = SelectedIndi.Values[nearestPlotIdx].GetValueAt(abar0);
						else {
							SelectedIndi   = null;
							nearestprice   = double.MinValue;
							nearestPlotIdx = -1;
						}
						//Print("NearestPrice: "+nearestprice);
					}
				}
				#endregion
			}
			#endregion
		}

line=1964;

		if(!EntryDirection_OnHoverQue.TryPeek(out EntryDirection_OnHover))
			Print("Peek FAILED in OnRender");

		#region Determine Entry price (Market, Limit, Stop)
		if(CurrentBars[0]>0 && CurrentBars[0]<Bars.Count){
			if(OnMarketDataFound){
				if(CurrentAsk==0) CurrentAsk = Closes[0].GetValueAt(CurrentBars[0]);
				if(CurrentBid==0) CurrentBid = Closes[0].GetValueAt(CurrentBars[0]);
				if(EntryDirection_OnHover == 'B') entryprice = CurrentAsk;
				if(EntryDirection_OnHover == 'S') entryprice = CurrentBid;
			}else
				entryprice = Closes[0].GetValueAt(CurrentBars[0]);

			if(SelectedBuyEntryType == EntryType_LIMIT)
				if(EntryDirection_OnHover == 'B') entryprice = entryprice - this.EntryLimitTicksOffset * TickSize;
			if(SelectedSellEntryType == EntryType_LIMIT)
				if(EntryDirection_OnHover == 'S') entryprice = entryprice + this.EntryLimitTicksOffset * TickSize;
			if(SelectedBuyEntryType == EntryType_STOP)
				if(EntryDirection_OnHover == 'B') entryprice = entryprice + this.EntryStopTicksOffset * TickSize;
			if(SelectedSellEntryType == EntryType_STOP){
				if(EntryDirection_OnHover == 'S') entryprice = entryprice - this.EntryStopTicksOffset * TickSize;
	//			Print(EntryDirection_OnHover+":  EntryPrice: "+entryprice);
			}
		}
		#endregion

		#region Determine UpperSL and LowerSL
line=1993;
		RemoveDrawObject("info2");
		bool IsOkToEnableEntries = true;
		if(ATMStrategyEnabled && UseAtmStrategySL){
			UpperSL = RoundToTick(entryprice + ATM_SL_Ticks * TickSize);
			LowerSL = RoundToTick(entryprice - ATM_SL_Ticks * TickSize);
		}else if(SelectedSLType == SLType_HLINE){
line=2000;
			#region - HLine SL finder -
			if(nearestprice==double.MinValue){
				Draw.TextFixed(this,"info2","No HLine found containing the tag '"+HLineTag+"'"
					,TextPosition.Center,Brushes.Red, new SimpleFont("Arial",14),Brushes.Black,Brushes.Black,90);
				IsOkToEnableEntries = false;
			}else{
				double close = Closes[0].GetValueAt(CurrentBars[0]);
				UpperSL = RoundToTick(close + Math.Abs(close-nearestprice));
				LowerSL = RoundToTick(close - Math.Abs(close-nearestprice));
				//DrawEllipse(nearestprice, EntryDirection_OnHover == 'S' ? buy_ellipse_DX : sell_ellipse_DX, CurrentBars[0], chartScale, "SL", 2);
			}
			#endregion

		}else if(SelectedSLType == SLType_INDICATOR){
line=2015;
			#region - Indicator SL finder -
			if(SelectedIndi == null || nearestprice==double.MinValue || nearestprice==double.MaxValue){
				Draw.TextFixed(this,"info2","No indicator plot selected"
					,TextPosition.TopRight,Brushes.Red, new SimpleFont("Arial",14),Brushes.Black,Brushes.Black,90);
				IsOkToEnableEntries = false;
			}
			IsReady = SelectedIndi!=null && (SelectedIndi.State==State.DataLoaded || SelectedIndi.State==State.Realtime);
line=2023;
			if(IsReady && nearestprice!=double.MinValue){
				int adj = 0;
				if(SelectedIndi.Calculate==Calculate.OnBarClose) adj=1;
				abar0 = CurrentBar-adj;
				double indiprice = 0;
				for(int i = 0; i<SelectedIndi.Plots.Length; i++){
line=2030;
					if(SelectedIndi.Plots[i].Name == SelectedPlotName){
						double p = RoundToTick(SelectedIndi.Values[i].GetValueAt(abar0));
						if(p>indiprice) indiprice = p;
						if(p<indiprice) indiprice = p;
						break;
					}
				}
				double close = Closes[0].GetValueAt(CurrentBars[0]);
				UpperSL = RoundToTick(close + Math.Abs(close-indiprice));
				LowerSL = RoundToTick(close - Math.Abs(close-indiprice));
			}
			try{
line=2040;

			}catch(Exception e303){/*Print("e303: "+e303.ToString());*/}
			#endregion

		}else if(SelectedSLType == SLType_FIXEDTICKS){
line=2046;
			UpperSL = RoundToTick(entryprice + this.InitialStop_Ticks * TickSize);
			LowerSL = RoundToTick(entryprice - this.InitialStop_Ticks * TickSize);

		}else if(SelectedSLType == SLType_ATR){
line=2051;
			double atr = atr0 * this.InitialStop_ATRMult;
			UpperSL = RoundToTick(entryprice + atr);
			LowerSL = RoundToTick(entryprice - atr);
		}
line=2056;
		if(IsOkToEnableEntries){
			if(Buy_Btn != null){
				Buy_Btn.IsEnabled   = true;
				Buy_Btn.Background  = Brush_BuyButtonBkg;
				Buy_Btn.Foreground  = Brush_BuyButtonText;
			}else DebugPrint("Buy_Btn was null! (a)");
			if(Sell_Btn != null){
				Sell_Btn.IsEnabled  = true;
				Sell_Btn.Background = Brush_SellButtonBkg;
				Sell_Btn.Foreground = Brush_SellButtonText;
			}else DebugPrint("Sell_Btn was null! (a)");
		}else{
			if(Buy_Btn != null){
				Buy_Btn.IsEnabled   = false;
				Buy_Btn.Background  = Brushes.Silver;
				Buy_Btn.Foreground  = Brushes.LightGray;
			}else DebugPrint("Buy_Btn was null! (b)");
			if(Sell_Btn != null){
				Sell_Btn.IsEnabled  = false;
				Sell_Btn.Background = Brushes.Silver;
				Sell_Btn.Foreground = Brushes.LightGray;
			}else DebugPrint("Sell_Btn was null! (b)");
		}

		#endregion

line=2083;
		if(nearestprice == double.MinValue) {
			RemoveDrawObject("info");
			RemoveDrawObject("u");
			RemoveDrawObject("l");
//			return;
		}

line=2091;
		if(UpperSL == double.MinValue) {return;}
		if(LowerSL == double.MaxValue) {return;}
		double T1_DollarValue = double.MinValue;
		double T2_DollarValue = double.MinValue;
		double T3_DollarValue = double.MinValue;
		#region Print SL values
		//Draw.Dot(this, "close", false, Bars.GetTime(CurrentBar), close, Brushes.Blue);
		double sltemp = RoundToTick(UpperSL) - entryprice;
		if(sltemp!= ShortSLPts) {
			DT_StoplossChanged = DateTime.Now;
			ShortSLPts = sltemp;
		}
		sltemp = entryprice - RoundToTick(LowerSL);
		if(sltemp!= LongSLPts) {
			DT_StoplossChanged = DateTime.Now;
			LongSLPts = sltemp;
		}

		double LongSLDollarValue    = LongSLPts * Instrument.MasterInstrument.PointValue;
		double ShortSLDollarValue   = ShortSLPts * Instrument.MasterInstrument.PointValue;

		double DollarsWillingToRisk = 0;
		if(PctRisk>0)         DollarsWillingToRisk = AccountBalance * PctRisk/100.0;
		else if(DollarRisk>0) DollarsWillingToRisk = DollarRisk;
//Draw.TextFixed(this,"info2",
//	"ShortSL Pts: "+  ShortSLPts.ToString("0.00")
//	+"\nLongSL Pts: "+LongSLPts.ToString("0.00")
//	+"\nPtValue: "+   Instrument.MasterInstrument.PointValue.ToString("C")
//	+"\n$ to risk: "+ DollarsWillingToRisk.ToString("C")
//	,TextPosition.TopRight,Brushes.Red, new SimpleFont("Arial",14),Brushes.Black,Brushes.Black,90);

		string msg = string.Empty;
		if(entryprice > LowerSL){
			LongQty = 0;
			if(pMode == IW_RiskManager_Modes.Fixed)
				LongQty = this.UserDefinedPositionSize;
			else{
				if(DollarsWillingToRisk>0) LongQty = (int)Math.Floor(DollarsWillingToRisk / LongSLDollarValue);
			}
//Print("  LongQty:  "+LongQty+"   LongSLDollarValue: "+LongSLDollarValue);
			LongQty = Math.Max(this.MinContracts,Math.Min(this.MaxContracts, LongQty));
//Print("  LongQty:  "+LongQty);
//			msg = string.Format("{0} = {1}-contracts LONG", (LongQty * LongSLDollarValue).ToString("C"),LongQty);
		}
		if(entryprice < UpperSL){
line=1963;
//Print("Entry: "+entryprice+"\tUSL: "+UpperSL+"\tLSL: "+LowerSL);
			ShortQty = 0;
			if(pMode == IW_RiskManager_Modes.Fixed){
line=1968;
				ShortQty = this.UserDefinedPositionSize;
			}else{
line=1971;
				if(DollarsWillingToRisk>0) ShortQty = (int)Math.Floor(DollarsWillingToRisk / ShortSLDollarValue);
			}
line=1975;
			ShortQty = Math.Max(this.MinContracts,Math.Min(this.MaxContracts, ShortQty));
//			msg = string.Format("{0} = {1}-contracts SHORT",(ShortQty * ShortSLDollarValue).ToString("C"),ShortQty);
		}
line=1979;
		if(ShortQty==0 && LongQty==0){
line=1981;
			LongQty  = (int)Math.Floor(DollarsWillingToRisk / LongSLDollarValue);
			ShortQty = (int)Math.Floor(DollarsWillingToRisk / ShortSLDollarValue);

			LongQty  = Math.Max(this.MinContracts,Math.Min(this.MaxContracts, LongQty));
			ShortQty = Math.Max(this.MinContracts,Math.Min(this.MaxContracts, ShortQty));
//			msg = string.Format("{0} = {1}-contracts SHORT\n{4} = {5}-contracts LONG",(ShortQty * ShortSLDollarValue).ToString("C"),ShortQty,(LongQty * LongSLDollarValue).ToString("C"),LongQty);
		}
line=1994;
		if(msg.Length>0)
			Draw.TextFixed(this,"info",msg,TextPosition.Center,Brushes.Red, new SimpleFont("Arial",14),Brushes.Black,Brushes.Black,90);
		#endregion
line=2165;
		#region Draw ellipse on stoploss levels
//Draw.Dot(this,"u",false,10,UpperSL,Brushes.Red);
//Draw.Dot(this,"L",false,5,LowerSL,Brushes.Red);
line=2002;
		if(entryprice < UpperSL){
			string SL_label = "SL";
			if(EntryDirection_OnHover == 'S'){
				if(this.ShowDollarsRisked) SL_label = $"SL {RemoveInsignificantDigits((LongQty * LongSLDollarValue).ToString("C"))}";
				DrawEllipse_SL(UpperSL, SelectedSLType == SLType_HLINE, buy_ellipse_DX, CurrentBars[0], chartScale, SL_label, 2);
			} else {
				if(this.ShowDollarsRisked) SL_label = $"SL {RemoveInsignificantDigits((ShortQty * ShortSLDollarValue).ToString("C"))}";
				UpperSL = entryprice - Math.Abs(UpperSL - entryprice);
				DrawEllipse_SL(UpperSL, SelectedSLType == SLType_HLINE, sell_ellipse_DX, CurrentBars[0], chartScale, SL_label, 2);
			}
		}else{
			string SL_label = "SL";
			if(EntryDirection_OnHover == 'B'){
				if(this.ShowDollarsRisked) SL_label = $"SL {RemoveInsignificantDigits((LongQty * LongSLDollarValue).ToString("C"))}";
				DrawEllipse_SL(UpperSL, SelectedSLType == SLType_HLINE, sell_ellipse_DX, CurrentBars[0], chartScale, SL_label, 2);
			} else {
				if(this.ShowDollarsRisked) SL_label = $"SL {RemoveInsignificantDigits((ShortQty * ShortSLDollarValue).ToString("C"))}";
				UpperSL = entryprice - Math.Abs(entryprice - UpperSL);
				DrawEllipse_SL(UpperSL, SelectedSLType == SLType_HLINE, buy_ellipse_DX, CurrentBars[0], chartScale, SL_label, 2);
			}
		}
		#endregion

line=2193;
		var t0 = Time.GetValueAt(CurrentBar);
		if(CurrentBars.Length>0 && (pShowBidAskDots || pShowBuySellPriceLines)){
			#region -- ShowEntryLine, and bid dot and ask dot --
			float x = 0;
			try{
				x = ChartControl.GetXByBarIndex(ChartBars, CurrentBars[0])+3;
			}catch(Exception ek){
				x = chartControl.Properties.BarDistance * (ChartBars.ToIndex-ChartBars.FromIndex+1)+5;
				Print(line+"  error: "+ek.ToString());
			}
line=2032;
			var yBidPrice = chartScale.GetYByValue(CurrentBid);
			var yAskPrice = chartScale.GetYByValue(CurrentAsk);
			if(pShowBidAskDots){
line=2036;
				var barwidth = chartControl.GetBarPaintWidth(ChartBars);
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x+barwidth,yAskPrice-1f), 2,3), BuyELineDX);
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x+barwidth,yBidPrice-1f), 2,3), SellELineDX);
			}

			if(pShowBuySellPriceLines){
line=2043;
				if(pPennantLocs == IW_RiskManager_PennantLocs.Left){
line=2045;
					#region -- left of current bar --
					float y = chartScale.GetYByValue(entryprice);//entry price is CurrentBid or CurrentAsk
					if(EntryDirection_OnHover =='B'){
						RenderTarget.DrawLine(new SharpDX.Vector2(x,y),new SharpDX.Vector2(x-LineLengthF,y), BuyELineDX);
						if(pShowBuySellPriceLines && this.OnMarketDataFound){
							RenderTarget.DrawLine(new SharpDX.Vector2(x,yBidPrice),new SharpDX.Vector2(x-LineLengthF,yBidPrice), SellELineDX);
						}
	//					if(SelectedBuyEntryType == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - BUY entry label
						string labeltxt = SetLabelText(EntryDirection_OnHover, LongQty, SelectedBuyEntryType);
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						var textLayout  = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, labeltxt, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
						x = x - LineLengthF - textLayout.Metrics.Width-5f;
						y = y - textLayout.Metrics.Height/2f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x-5,y-5,textLayout.Metrics.Width+10f,textLayout.Metrics.Height+10f),blackDX);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y), textLayout, BuyELineDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						textFormat.Dispose(); textFormat=null;
						textLayout.Dispose(); textLayout=null;
						#endregion
					}
					else if(EntryDirection_OnHover=='S'){
						RenderTarget.DrawLine(new SharpDX.Vector2(x,y),new SharpDX.Vector2(x-LineLengthF,y), SellELineDX);
						if(pShowBuySellPriceLines && this.OnMarketDataFound){
							RenderTarget.DrawLine(new SharpDX.Vector2(x,yAskPrice),new SharpDX.Vector2(x-LineLengthF,yAskPrice), BuyELineDX);
						}
	//					if(SelectedBuyEntryType == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - SELL entry label
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						string labeltxt = SetLabelText(EntryDirection_OnHover, ShortQty, SelectedSellEntryType);
						var textLayout  = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, labeltxt, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
						x = x - LineLengthF - textLayout.Metrics.Width-5f;
						y = y - textLayout.Metrics.Height/2f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x-5,y-5,textLayout.Metrics.Width+10f,textLayout.Metrics.Height+10f),blackDX);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y), textLayout, SellELineDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						textFormat.Dispose(); textFormat=null;
						textLayout.Dispose(); textLayout=null;
						#endregion
					}
					#endregion ------------------
				}else{
line=2090;
					#region -- right of current bar --
					float y = chartScale.GetYByValue(entryprice);//entryrprice is CurrentBid or CurrentAsk
					float yOfEntryLine = y;
					if(EntryDirection_OnHover =='B'){
	//					if(SelectedBuyEntryType == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - BUY entry label
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						string labeltxt = SetLabelText(EntryDirection_OnHover, LongQty, SelectedBuyEntryType);
						var textLayout  = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, labeltxt, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
						x = Convert.ToSingle(ChartPanel.W) - textLayout.Metrics.Width-10f;
						y = y - textLayout.Metrics.Height/2f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x-10f,y-5f,textLayout.Metrics.Width+10f,textLayout.Metrics.Height+10f),blackDX);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x-5f,y), textLayout, BuyELineDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						textFormat.Dispose(); textFormat=null;
						textLayout.Dispose(); textLayout=null;
						RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yOfEntryLine),new SharpDX.Vector2(x-LineLengthF,yOfEntryLine), BuyELineDX);
						if(pShowBuySellPriceLines && this.OnMarketDataFound){
							yBidPrice = chartScale.GetYByValue(CurrentBid);
							RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yBidPrice),new SharpDX.Vector2(x-LineLengthF,yBidPrice), SellELineDX);
						}
						#endregion
					}
					else if(EntryDirection_OnHover=='S'){
	//					if(SelectedBuyEntryType == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - SELL entry label
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						string labeltxt = SetLabelText(EntryDirection_OnHover, ShortQty, SelectedSellEntryType);
						var textLayout  = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, labeltxt, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
						x = Convert.ToSingle(ChartPanel.W) - textLayout.Metrics.Width-10f;
						y = y - textLayout.Metrics.Height/2f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x-10f,y-5f,textLayout.Metrics.Width+10f,textLayout.Metrics.Height+10f),blackDX);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x-5f,y), textLayout, SellELineDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						textFormat.Dispose(); textFormat=null;
						textLayout.Dispose(); textLayout=null;
						RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yOfEntryLine),new SharpDX.Vector2(x-LineLengthF,yOfEntryLine), SellELineDX);
						if(pShowBuySellPriceLines && this.OnMarketDataFound){
							yAskPrice = chartScale.GetYByValue(CurrentAsk);
							RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yAskPrice),new SharpDX.Vector2(x-LineLengthF,yAskPrice), BuyELineDX);
						}
						#endregion
					}
					#endregion ----------------
				}
			}
			#endregion -----------------
		}
line=2313;
		CalculateTarget_Price(entryprice, LongTargets, LongSLPts);
		CalculateTarget_OrderQty(LongTargets, LongQty);
		CalculateTarget_Price(entryprice, ShortTargets, ShortSLPts);
		CalculateTarget_OrderQty(ShortTargets, ShortQty);
line=2318;

		#region Draw ellipse on target levels
		string label = string.Empty;
		var tsTgs = new TimeSpan(DateTime.Now.Ticks - this.DT_StoplossChanged.Ticks);
		if(tsTgs.Seconds % 2 == 0)
			ShowTicksInEllipse = !ShowTicksInEllipse;

		if(EntryDirection_OnHover=='S'){
//Print("  ShortTargets.Count: "+ShortTargets.Count);
			TargetData targ = null;
			for(int i = 0; i<ShortTargets.Count; i++){
				if(ShortTargets.TryGetValue(i, out targ)){
//Print((i+1).ToString()+"  @ "+targ.Price+"  "+targ.OrderQty);
					if(targ.OrderQty>0 && !double.IsNaN(targ.Price)){
						if(ShowTicksInEllipse){
							double pts_to_target = Math.Abs(entryprice - targ.Price);
							if(this.ShowDollarsReward){
								label = $"{RemoveInsignificantDigits((targ.OrderQty * Instrument.MasterInstrument.PointValue * pts_to_target).ToString("C"))}";
							}else
								label = $"{RoundToTick(pts_to_target/TickSize)}-tks";
						} else
							label = $"{targ.OrderQty}@T{(i+1)}";
						DrawEllipse(targ.Price, buy_ellipse_DX, CurrentBars[0], chartScale, label, 2);
					}
				}else Print("Could not get "+i+" short target");
			}
		}
		else if(EntryDirection_OnHover=='B'){
			TargetData targ = null;
			for(int i = 0; i<LongTargets.Count; i++){
				if(LongTargets.TryGetValue(i, out targ)){
					if(targ.OrderQty>0 && !double.IsNaN(targ.Price)){
		//Print(string.Format("T{0}  {1}-tks  qty: {2}  @ {3}",(i+1).ToString(),targ.Ticks,targ.OrderQty,targ.Price));
						if(ShowTicksInEllipse){
							double pts_to_target = Math.Abs(entryprice - targ.Price);
							if(this.ShowDollarsReward){
								label = $"{RemoveInsignificantDigits((targ.OrderQty * Instrument.MasterInstrument.PointValue * pts_to_target).ToString("C"))}";
							}else
								label = $"{RoundToTick(pts_to_target/TickSize)}-tks";
						}else
							label = $"{targ.OrderQty}@T{(i+1)}";
						DrawEllipse(targ.Price, sell_ellipse_DX, CurrentBars[0], chartScale, label, 2);
					}
				}else Print("Could not get "+i+" long target");
			}
		}
		#endregion
	}catch(Exception e){Print(line+" E: "+e.ToString()+Environment.NewLine+"at "+DateTime.Now.ToString()+" "+Instrument.MasterInstrument.Name+Environment.NewLine);}
	}
		//==================================================================================
		//==================================================================================
		private string SetLabelText(char Direction, double Qty, int SelectedEntryType){
			if(BuySellLabel == "") return string.Empty;
			string labeltxt = string.Empty;
			string BS = BuySellLabel;
			if(ATMStrategyEnabled){
				labeltxt = $"'{ATMStrategyName}' {BS} {Qty}";
			}else{
				labeltxt = $"{BS} {Qty}";
			}
			if(SelectedEntryType == EntryType_LIMIT)     labeltxt = $"{labeltxt}\nLimit";
			else if(SelectedEntryType == EntryType_STOP) labeltxt = $"{labeltxt}\nStop";
			return labeltxt;
		}
		//==================================================================================
        protected override void OnMarketData(MarketDataEventArgs e)
        {
			if (e.MarketDataType == MarketDataType.Ask)
			{
				OnMarketDataFound = true;
				CurrentAsk	= e.Price;
			}
			if (e.MarketDataType == MarketDataType.Bid)
			{
				OnMarketDataFound = true;
				CurrentBid	= e.Price;
			}
			TimeOfLastTick = e.Time;
		}
		//==================================================================================
		private string RemoveInsignificantDigits(string input){
			if(!input.Contains(".")) return input;
			var sb = new System.Text.StringBuilder(input);
			while(sb.Length>0 && sb[sb.Length-1] == '0') 
				sb = sb.Remove(sb.Length-1,1);
			if(sb[sb.Length-1] == '.') sb = sb.Remove(sb.Length-1,1);
			return sb.ToString();
		}
		//==================================================================================
		public override void OnRenderTargetChanged()
		{
			#region Brush disposal
			if(buy_ellipse_DX!=null      && !buy_ellipse_DX.IsDisposed)      buy_ellipse_DX.Dispose();      buy_ellipse_DX   = null;
			if(sell_ellipse_DX!=null     && !sell_ellipse_DX.IsDisposed)     sell_ellipse_DX.Dispose();     sell_ellipse_DX  = null;
			if(BuyELineDX!=null			 && !BuyELineDX.IsDisposed)          BuyELineDX.Dispose();          BuyELineDX   = null;
			if(SellELineDX!=null		 && !SellELineDX.IsDisposed)         SellELineDX.Dispose();         SellELineDX  = null;
			if(blackDX!=null			 && !blackDX.IsDisposed)			 blackDX.Dispose();				blackDX      = null;
			#endregion

			if(RenderTarget!=null){
				#region Brush init
				buy_ellipse_DX  = Brushes.Green.ToDxBrush(RenderTarget);
				sell_ellipse_DX = Brushes.Magenta.ToDxBrush(RenderTarget);
				BuyELineDX      = Brushes.Cyan.ToDxBrush(RenderTarget);
				SellELineDX     = Brushes.Magenta.ToDxBrush(RenderTarget);
				blackDX         = Brushes.Black.ToDxBrush(RenderTarget);
				#endregion
			}
		}
}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_RiskManager[] cacheIW_RiskManager;
		public IW_RiskManager IW_RiskManager(string selectedAccountName, string pDefaultOrderType, int entryLimitTicksOffset, int entryStopTicksOffset, bool pEnableOCO, string aTMStrategyName, IW_RiskManager_TargetDistanceTypes pTargetDistanceType, int pT1_Ticks, int pT2_Ticks, int pT3_Ticks, double rRT1, double rRT2, double rRT3, double qtyT1Pct, double qtyT2Pct, double qtyT3Pct, double pctRisk, double accountBalance, int maxContracts, int minContracts, string hLineTag, int initialStop_Ticks, double initialStop_ATRMult, IW_RiskManager_Modes pMode)
		{
			return IW_RiskManager(Input, selectedAccountName, pDefaultOrderType, entryLimitTicksOffset, entryStopTicksOffset, pEnableOCO, aTMStrategyName, pTargetDistanceType, pT1_Ticks, pT2_Ticks, pT3_Ticks, rRT1, rRT2, rRT3, qtyT1Pct, qtyT2Pct, qtyT3Pct, pctRisk, accountBalance, maxContracts, minContracts, hLineTag, initialStop_Ticks, initialStop_ATRMult, pMode);
		}

		public IW_RiskManager IW_RiskManager(ISeries<double> input, string selectedAccountName, string pDefaultOrderType, int entryLimitTicksOffset, int entryStopTicksOffset, bool pEnableOCO, string aTMStrategyName, IW_RiskManager_TargetDistanceTypes pTargetDistanceType, int pT1_Ticks, int pT2_Ticks, int pT3_Ticks, double rRT1, double rRT2, double rRT3, double qtyT1Pct, double qtyT2Pct, double qtyT3Pct, double pctRisk, double accountBalance, int maxContracts, int minContracts, string hLineTag, int initialStop_Ticks, double initialStop_ATRMult, IW_RiskManager_Modes pMode)
		{
			if (cacheIW_RiskManager != null)
				for (int idx = 0; idx < cacheIW_RiskManager.Length; idx++)
					if (cacheIW_RiskManager[idx] != null && cacheIW_RiskManager[idx].SelectedAccountName == selectedAccountName && cacheIW_RiskManager[idx].pDefaultOrderType == pDefaultOrderType && cacheIW_RiskManager[idx].EntryLimitTicksOffset == entryLimitTicksOffset && cacheIW_RiskManager[idx].EntryStopTicksOffset == entryStopTicksOffset && cacheIW_RiskManager[idx].pEnableOCO == pEnableOCO && cacheIW_RiskManager[idx].ATMStrategyName == aTMStrategyName && cacheIW_RiskManager[idx].pTargetDistanceType == pTargetDistanceType && cacheIW_RiskManager[idx].pT1_Ticks == pT1_Ticks && cacheIW_RiskManager[idx].pT2_Ticks == pT2_Ticks && cacheIW_RiskManager[idx].pT3_Ticks == pT3_Ticks && cacheIW_RiskManager[idx].RRT1 == rRT1 && cacheIW_RiskManager[idx].RRT2 == rRT2 && cacheIW_RiskManager[idx].RRT3 == rRT3 && cacheIW_RiskManager[idx].QtyT1Pct == qtyT1Pct && cacheIW_RiskManager[idx].QtyT2Pct == qtyT2Pct && cacheIW_RiskManager[idx].QtyT3Pct == qtyT3Pct && cacheIW_RiskManager[idx].PctRisk == pctRisk && cacheIW_RiskManager[idx].AccountBalance == accountBalance && cacheIW_RiskManager[idx].MaxContracts == maxContracts && cacheIW_RiskManager[idx].MinContracts == minContracts && cacheIW_RiskManager[idx].HLineTag == hLineTag && cacheIW_RiskManager[idx].InitialStop_Ticks == initialStop_Ticks && cacheIW_RiskManager[idx].InitialStop_ATRMult == initialStop_ATRMult && cacheIW_RiskManager[idx].pMode == pMode && cacheIW_RiskManager[idx].EqualsInput(input))
						return cacheIW_RiskManager[idx];
			return CacheIndicator<IW_RiskManager>(new IW_RiskManager(){ SelectedAccountName = selectedAccountName, pDefaultOrderType = pDefaultOrderType, EntryLimitTicksOffset = entryLimitTicksOffset, EntryStopTicksOffset = entryStopTicksOffset, pEnableOCO = pEnableOCO, ATMStrategyName = aTMStrategyName, pTargetDistanceType = pTargetDistanceType, pT1_Ticks = pT1_Ticks, pT2_Ticks = pT2_Ticks, pT3_Ticks = pT3_Ticks, RRT1 = rRT1, RRT2 = rRT2, RRT3 = rRT3, QtyT1Pct = qtyT1Pct, QtyT2Pct = qtyT2Pct, QtyT3Pct = qtyT3Pct, PctRisk = pctRisk, AccountBalance = accountBalance, MaxContracts = maxContracts, MinContracts = minContracts, HLineTag = hLineTag, InitialStop_Ticks = initialStop_Ticks, InitialStop_ATRMult = initialStop_ATRMult, pMode = pMode }, input, ref cacheIW_RiskManager);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_RiskManager IW_RiskManager(string selectedAccountName, string pDefaultOrderType, int entryLimitTicksOffset, int entryStopTicksOffset, bool pEnableOCO, string aTMStrategyName, IW_RiskManager_TargetDistanceTypes pTargetDistanceType, int pT1_Ticks, int pT2_Ticks, int pT3_Ticks, double rRT1, double rRT2, double rRT3, double qtyT1Pct, double qtyT2Pct, double qtyT3Pct, double pctRisk, double accountBalance, int maxContracts, int minContracts, string hLineTag, int initialStop_Ticks, double initialStop_ATRMult, IW_RiskManager_Modes pMode)
		{
			return indicator.IW_RiskManager(Input, selectedAccountName, pDefaultOrderType, entryLimitTicksOffset, entryStopTicksOffset, pEnableOCO, aTMStrategyName, pTargetDistanceType, pT1_Ticks, pT2_Ticks, pT3_Ticks, rRT1, rRT2, rRT3, qtyT1Pct, qtyT2Pct, qtyT3Pct, pctRisk, accountBalance, maxContracts, minContracts, hLineTag, initialStop_Ticks, initialStop_ATRMult, pMode);
		}

		public Indicators.IW_RiskManager IW_RiskManager(ISeries<double> input , string selectedAccountName, string pDefaultOrderType, int entryLimitTicksOffset, int entryStopTicksOffset, bool pEnableOCO, string aTMStrategyName, IW_RiskManager_TargetDistanceTypes pTargetDistanceType, int pT1_Ticks, int pT2_Ticks, int pT3_Ticks, double rRT1, double rRT2, double rRT3, double qtyT1Pct, double qtyT2Pct, double qtyT3Pct, double pctRisk, double accountBalance, int maxContracts, int minContracts, string hLineTag, int initialStop_Ticks, double initialStop_ATRMult, IW_RiskManager_Modes pMode)
		{
			return indicator.IW_RiskManager(input, selectedAccountName, pDefaultOrderType, entryLimitTicksOffset, entryStopTicksOffset, pEnableOCO, aTMStrategyName, pTargetDistanceType, pT1_Ticks, pT2_Ticks, pT3_Ticks, rRT1, rRT2, rRT3, qtyT1Pct, qtyT2Pct, qtyT3Pct, pctRisk, accountBalance, maxContracts, minContracts, hLineTag, initialStop_Ticks, initialStop_ATRMult, pMode);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_RiskManager IW_RiskManager(string selectedAccountName, string pDefaultOrderType, int entryLimitTicksOffset, int entryStopTicksOffset, bool pEnableOCO, string aTMStrategyName, IW_RiskManager_TargetDistanceTypes pTargetDistanceType, int pT1_Ticks, int pT2_Ticks, int pT3_Ticks, double rRT1, double rRT2, double rRT3, double qtyT1Pct, double qtyT2Pct, double qtyT3Pct, double pctRisk, double accountBalance, int maxContracts, int minContracts, string hLineTag, int initialStop_Ticks, double initialStop_ATRMult, IW_RiskManager_Modes pMode)
		{
			return indicator.IW_RiskManager(Input, selectedAccountName, pDefaultOrderType, entryLimitTicksOffset, entryStopTicksOffset, pEnableOCO, aTMStrategyName, pTargetDistanceType, pT1_Ticks, pT2_Ticks, pT3_Ticks, rRT1, rRT2, rRT3, qtyT1Pct, qtyT2Pct, qtyT3Pct, pctRisk, accountBalance, maxContracts, minContracts, hLineTag, initialStop_Ticks, initialStop_ATRMult, pMode);
		}

		public Indicators.IW_RiskManager IW_RiskManager(ISeries<double> input , string selectedAccountName, string pDefaultOrderType, int entryLimitTicksOffset, int entryStopTicksOffset, bool pEnableOCO, string aTMStrategyName, IW_RiskManager_TargetDistanceTypes pTargetDistanceType, int pT1_Ticks, int pT2_Ticks, int pT3_Ticks, double rRT1, double rRT2, double rRT3, double qtyT1Pct, double qtyT2Pct, double qtyT3Pct, double pctRisk, double accountBalance, int maxContracts, int minContracts, string hLineTag, int initialStop_Ticks, double initialStop_ATRMult, IW_RiskManager_Modes pMode)
		{
			return indicator.IW_RiskManager(input, selectedAccountName, pDefaultOrderType, entryLimitTicksOffset, entryStopTicksOffset, pEnableOCO, aTMStrategyName, pTargetDistanceType, pT1_Ticks, pT2_Ticks, pT3_Ticks, rRT1, rRT2, rRT3, qtyT1Pct, qtyT2Pct, qtyT3Pct, pctRisk, accountBalance, maxContracts, minContracts, hLineTag, initialStop_Ticks, initialStop_ATRMult, pMode);
		}
	}
}

#endregion
