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
public enum EntryManager_Modes {Disabled, Fixed}
public enum EntryManager_PennantLocs {Left,Right}
public enum EntryManager_LagTimeLocation {NoDisplay, TopLeft, TopRight, Center, BottomLeft, BottomRight}

namespace NinjaTrader.NinjaScript.Indicators
{
	public class EntryManager : Indicator
	{
		#region variables
		private const int PRICE_PANEL_ID_NUMBER = 0;

		private const int EntryType_MARKET = 0;
		private const int EntryType_LIMIT = 1;
		private const int EntryType_STOP = 2;
		private const int EntryType_MKTLIMIT = 3;
		
		private bool isIndicatorAdded;
		private int mouseX = 0;
		private int mouseY = 0; 
		private bool AlertOnce = true;
		private DateTime MessageTime = DateTime.MinValue;
		private DateTime ClickToEnterTime = DateTime.MinValue;

		private System.Windows.Controls.Grid				chartGrid;
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
		int SelectedBuyEntryType  = EntryType_LIMIT;
		int SelectedSellEntryType = EntryType_LIMIT;
		double nearestprice  = double.MinValue;
		IndicatorBase SelectedIndi = null;
		string SelectedPlotName = string.Empty;
		double entryprice      = 0;
		double EntryLimitPrice = 0;
		double EntryStopPrice  = 0;
		double atr0 = 0;
		bool ATMStrategyEnabled = false;
		DateTime DT_StoplossChanged = DateTime.MinValue;
		string BuySellLabel = string.Empty;
		bool ShowTicksInEllipse = false;

		#region - UI buttons, menus, combobox -
		private System.Windows.Controls.WrapPanel   BuySellPanel;
		private	System.Windows.Controls.Button		Buy_Btn;
		private	System.Windows.Controls.Button		Sell_Btn;
		private	System.Windows.Controls.Button		BuyMkt_Btn;
		private	System.Windows.Controls.Button		SellMkt_Btn;
		private	System.Windows.Controls.Button		GoFlat_Btn;
		private System.Windows.Controls.Button		TicksFor_StopOrLimit_Btn;
		private	System.Windows.Controls.Button		PositionSize_Btn;
		private System.Windows.Controls.Button		Stoploss_Btn;
		private System.Windows.Controls.Grid   		MenuGrid, BSMktGrid;
		private System.Windows.Controls.Primitives.Thumb drag_BuySell;
		static int defaultMargin          = 5;
		double TogglePositionMarginLeft	  = 5;
		double TogglePositionMarginTop    = 5;
		double TogglePositionMarginRight  = 5;
		double TogglePositionMarginBottom = 5;
		int current_indi_id = 0;
		#endregion

		int pPositionSize = 1;
		ConcurrentStack<char> EntryDirection_OnHoverQue = new ConcurrentStack<char>();
		#endregion
		SharpDX.Direct2D1.Brush buy_ellipse_DX  = null;
		SharpDX.Direct2D1.Brush sell_ellipse_DX = null;
		string Order_Reject_Message = string.Empty;
		string Log_Order_Reject_Message = string.Empty;
		int line = 0;
		DateTime TimeOfLastTick = DateTime.MinValue;
		ATR atr;

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
		public override string DisplayName{	get { return string.Format("EntryManager", this.SelectedAccountName); }}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "Entry Manager v2.2 (2023-9-28)";
				#region variables
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
				MaxContracts = 10;
				MinContracts = 1;
				pUseChartTraderAccount = true;
				SelectedAccountName   = "";
				EntryStopTicksOffset  = 4;
				EntryLimitTicksOffset = 4;
				EntryDirection_OnHoverQue.Push(' ');
				pUseChartTraderAtmStrategyName = true;
				ATMStrategyName     = "N/A";
				LineLengthF         = 150f;
				btnwidth    = 160;
				btnFontSize = 16;
				MarkerOpacity       = 50;
				pMode               = EntryManager_Modes.Fixed;
				pPennantLocs        = EntryManager_PennantLocs.Right;
				pDefaultOrderType   = "Limit";
				pShowBuySellPriceLines = true;
				pShowBidAskDots     = true;
				pLagTimerLocation   = EntryManager_LagTimeLocation.BottomLeft;
				pLagWarningFont     = new SimpleFont("Arial", 14);
				pLagWarningSeconds  = 2;
				pEntriesCount       = 1;
				Brush_RiskButtonBkg = Brushes.Gold;
				Brush_RiskButtonText = Brushes.Black;
				Brush_BuyButtonBkg   = Brushes.Lime;
				Brush_BuyButtonText  = Brushes.Black;
				Brush_SellButtonBkg  = Brushes.Red;
				Brush_SellButtonText = Brushes.Black;
				Brush_EntryDistanceButtonBkg  = Brushes.Navy;
				Brush_EntryDistanceButtonText = Brushes.Black;
				pShowGoFlatButton      = true;
				Brush_GoFlatButtonBkg  = Brushes.Red;
				Brush_GoFlatButtonText = Brushes.White;
				#endregion
			}
			#region OnStateChange
			else if(State == State.Configure){
				Calculate = Calculate.OnPriceChange;
				Log_Order_Reject_Message = "Your order was rejected.  Account Name '' may not be available on this datafeed connection.  Check for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				Order_Reject_Message = "Your order was rejected.\nAccount Name '' may not be available on this datafeed connection\nCheck for typo errors, or consider changing it to 'Sim101' or 'Replay101'";
				if(pDefaultOrderType.Trim().Length==0){
					SelectedBuyEntryType  = EntryType_MARKET;
					SelectedSellEntryType = EntryType_MARKET;
					pDefaultOrderType = "Market";
				}else if(pDefaultOrderType.Trim().ToLower().Contains("m") && pDefaultOrderType.Trim().ToLower().Contains("l")){
					SelectedBuyEntryType  = EntryType_MKTLIMIT;
					SelectedSellEntryType = EntryType_MKTLIMIT;
					pDefaultOrderType = "MktLimit";
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
			else if (State == State.DataLoaded)
			{
				#region DataLoaded
				controlLightGray = new System.Windows.Media.SolidColorBrush(Color.FromRgb(64, 63, 69));
				controlLightGray.Freeze();
				int count = 0;
				#endregion
				atr = ATR(14);
			}
			else if (State == State.Terminated)
			{
				#region Terminated
				if (ChartControl != null)
				{
					ChartPanel.MouseUp 	-= MyMouseUpEvent;
					ChartPanel.KeyUp    -= MyKeyUpEvent;
					ChartPanel.KeyDown  -= MyKeyDownEvent;
				}
				if (accountSelector != null)
					accountSelector.SelectionChanged		-= AccountSelector_SelectionChanged;

				if (atmStrategySelector != null)
					atmStrategySelector.SelectionChanged	-= AtmStrategySelector_SelectionChanged;

				DisposeCleanUp();
				#endregion
			}
			else if (State == State.Historical)
			{
				#region Historical
				// Use an Automation ID to limit multiple instances of this indicator.
				if (ChartControl != null && !isIndicatorAdded)
				{
					if(ChartControl.Dispatcher.CheckAccess()){
try{
line=373;
						chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
						if (chartWindow == null) return;

line=377;
						chartGrid	= chartWindow.MainTabControl.Parent as System.Windows.Controls.Grid;
						foreach (DependencyObject item in chartGrid.Children)
						{
line=381;
							if (AutomationProperties.GetAutomationId(item) == "EntryManager")
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
//							if(TicksFor_StopOrLimit_Btn!=null){
//								if(SelectedBuyEntryType != EntryType_MARKET || SelectedSellEntryType != EntryType_MARKET){
//									TicksFor_StopOrLimit_Btn.Background = Brushes.Blue;
//									TicksFor_StopOrLimit_Btn.Foreground = Brushes.LightGray;
//								}else{
//									TicksFor_StopOrLimit_Btn.Background = Brushes.Navy;
//									TicksFor_StopOrLimit_Btn.Foreground = Brushes.Black;
//								}
//							}
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
							AutomationProperties.SetAutomationId(topMenu, "EntryManager");
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
					}else{
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
								if (AutomationProperties.GetAutomationId(item) == "EntryManager")
								{
									isIndicatorAdded = true;
								}
							}

							if (!isIndicatorAdded)
							{
								AddToolBar();
								#region -- Set background of TicksFor_StopOrLimit_Btn --
//								if(TicksFor_StopOrLimit_Btn!=null){
//									if(SelectedBuyEntryType != EntryType_MARKET || SelectedSellEntryType != EntryType_MARKET){
//										TicksFor_StopOrLimit_Btn.Background = Brushes.Blue;
//										TicksFor_StopOrLimit_Btn.Foreground = Brushes.LightGray;
//									}else{
//										TicksFor_StopOrLimit_Btn.Background = Brushes.Navy;
//										TicksFor_StopOrLimit_Btn.Foreground = Brushes.Black;
//									}
//								}
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
								AutomationProperties.SetAutomationId(topMenu, "EntryManager");
								// Begin AdvancedRiskReward OnStartUp
								if (!_init)
								{
									ChartPanel.MouseUp += new MouseButtonEventHandler(MyMouseUpEvent);
									ChartPanel.KeyUp   += new KeyEventHandler(MyKeyUpEvent);
									ChartPanel.KeyDown += new KeyEventHandler(MyKeyDownEvent);
									_init = true;
								}
line=426;
							}
}catch(Exception e124){Print(line+"  e124: "+e124.ToString());}
						}));
					}
				}
				#endregion
			}
			else if(State==State.Realtime){
				ATMStrategyEnabled = ATMStrategyName.Trim().Length == 0 ? false : true;
				#region Verify account availability
				if(!pUseChartTraderAccount){
					var accts = Account.All.ToList();
					for(int i = 0; i<accts.Count; i++){
						if(accts[i].Name == SelectedAccountName){
							accts = null;
							break;
						}
					}
					if(accts==null){
						lock (Account.All)
						myAccount = Account.All.FirstOrDefault(a => a.Name == SelectedAccountName);
						CheckForExistingOrdersPositions(myAccount);
					}else{
						string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "EntryManager");
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
						if(ts.TotalDays>5) System.IO.File.Delete("EntryManager");
						else if(ts.TotalMinutes > 10){
							string msg = "ERROR = account '"+SelectedAccountName+"' is not available to trade";
							Log(msg, LogLevel.Alert);
							System.IO.File.AppendAllText("EntryManager", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
						}
					}
				}
				#endregion
				#region Initialize atmstrategy selection
				if(ATMStrategyName.Trim().Length==0)
					ATMStrategyName = "N/A";
				if ((pUseChartTraderAccount && accountSelector == null) || (pUseChartTraderAtmStrategyName && atmStrategySelector == null))
					FindAssignUIElementsByAutomationID();
				#endregion
			}
			#endregion
		}
		#region -- Get ChartTrader UI elements AccountName and ATMStrategyName --
		private void FindAssignUIElementsByAutomationID()
		{
			//Print("Inside FindAssignUIElementsByAutomationID");
			// the chart must exist to get any controls from it
			if (ChartControl != null)
			{
				// chart controls run on the UI thread. Dispatcher Invoke is used to access the thread.
				// Typically, the InvokeAsync() is used access the UI thread asynchronously when it is ready. However, if this information is needed immediately, use Invoke so that this blocks the NinjaScript thread from continuing until this operation is complete.
				// Beware that using Invoke improperly can result in deadlocks.
				// This example uses Invoke so that the UI control values are available as the historical data is processing

				ChartControl.Dispatcher.Invoke((Action)(() =>
				{
					// the window of the chart
					Window chartWindow = Window.GetWindow(ChartControl.Parent);

					// find the ChartTrader account selector by AutomationID
					accountSelector = null;
//Print("pUseChartTraderAccount: "+pUseChartTraderAccount.ToString());
					if(this.pUseChartTraderAccount){
//Print("467   adding event for accountSelector");
						try{
							accountSelector		= chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
							if (accountSelector != null)
							{
								accountSelector.SelectionChanged	+= AccountSelector_SelectionChanged;
								myAccount							= accountSelector.SelectedAccount;
							}
						}catch(Exception e1){
							myAccount = null;
							Print("EntryManager attempted to connect to ChartTrader AccountName...error:\n"+e1.ToString());
						}
					}

					// find the ChartTrader atmStrategy selector by AutomationID
					atmStrategySelector = null;
					if(pUseChartTraderAtmStrategyName){
						try{
							atmStrategySelector	= chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;
							if (atmStrategySelector != null)
							{
								atmStrategySelector.SelectionChanged	+= AtmStrategySelector_SelectionChanged;
//								currentAtmStrategy						= atmStrategySelector.SelectedAtmStrategy;
							}
						}catch(Exception e1){
							ATMStrategyName = "N/A";
							Print("EntryManager attempted to connect to ChartTrader ATMStrategyName...error:\n"+e1.ToString());}
					}

//					instrumentSelector	= chartWindow.FindFirst("ChartTraderControlInstrumentSelector") as InstrumentSelector;
//					if (instrumentSelector != null)
//					{
//						instrumentSelector.InstrumentChanged	+= InstrumentSelector_InstrumentChanged;
//						currentInstrument						= instrumentSelector.Instrument;
//					}

//					// find the ChartTrader quantity selector by AutomationID
//					quantitySelector	= chartWindow.FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown;
//					if (quantitySelector != null)
//					{
//						quantitySelector.ValueChanged			+= QuantitySelector_ValueChanged;
//						currentQuantity							= quantitySelector.Value;
//					}

//					tifSelector			= chartWindow.FindFirst("ChartTraderControlTIFSelector") as TifSelector;
//					if (tifSelector != null)
//					{
//						tifSelector.SelectionChanged			+= TifSelector_SelectionChanged;
//						currentTif								= tifSelector.SelectedTif;
//					}
				}));
			}
		}

		private void AtmStrategySelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var currentAtmStrategy	= atmStrategySelector.SelectedAtmStrategy;
			if (currentAtmStrategy != null){
				this.ATMStrategyName = currentAtmStrategy.DisplayName;
			}else ATMStrategyName = "N/A";
//Print("ATM name: "+ATMStrategyName);
		}
		private NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector atmStrategySelector;
//		private void GetAtmStrategyName(){
////			ATMselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlATMStrategySelector") as AtmStrategySelector;
//			if(ChartControl == null) return;
//			if(ChartControl.OwnerChart == null) return;
//			if(ChartControl.OwnerChart.ChartTrader == null) return;
//			if(ChartControl.OwnerChart.ChartTrader.AtmStrategy == null) return;
//			ATMStrategyName = ChartControl.OwnerChart.ChartTrader.AtmStrategy.DisplayName;
//		}
		NinjaTrader.Gui.Tools.AccountSelector accountSelector = null;
		Connection ChartTraderAccountConnection = null;
		// setting a class level value when the selection changes would prevent having to pause and invoke into the UI thread to fetch this later
		private void AccountSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
//Print("AccountSelector called");
			if(accountSelector!=null){
				myAccount = accountSelector.SelectedAccount;
			}
//			ChartTraderAccountConnection = (myAccount != null) ? myAccount.Connection : null;
			//SelectedAccountName = (ChartTraderAccountConnection != null) ? ChartTraderAccountConnection.Options.Name : "Unknown";
//Print("myAccount.Name: "+myAccount.Name);
		}

//		private void GetChartTraderAccount(){
//			try{
//				ChartControl.Dispatcher.Invoke((Action)(() =>
//				{
//					accountSelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector);
//					myAccount = (accountSelector != null) ? accountSelector.SelectedAccount : null;
//					accountSelector.SelectionChanged += (o, args) =>{
//						myAccount = null;
//						if (accountSelector.SelectedAccount != null)
//						{
//							myAccount = accountSelector.SelectedAccount;
//							ChartTraderAccountConnection = (myAccount != null) ? myAccount.Connection : null;
//							SelectedAccountName = (ChartTraderAccountConnection != null) ? ChartTraderAccountConnection.Options.Name : "Unknown";
//						}
//					};
//				}));​
//				ChartTraderAccountConnection = (myAccount != null) ? myAccount.Connection : null;
//				SelectedAccountName = (ChartTraderAccountConnection != null) ? ChartTraderAccountConnection.Options.Name : "Unknown";
////				Draw.TextFixed(this, "EXAMPLE", "Account " + myAccount + " on connection " + SelectedAccountName, TextPosition.Center, Brushes.Blue, new SimpleFont(), Brushes.Black, Brushes.White, 100);

//			}catch(Exception e){Print("error: "+e.ToString());}//printDebug("ChartTrader not found: "+e.ToString());}
//		}
		#endregion
		//=======================================================================================
		private void CheckForExistingOrdersPositions(Account myAccount){
			if(ChartControl.Properties.ChartTraderVisibility != ChartTraderVisibility.Visible){
				string msg = string.Empty;
				if(myAccount.Orders.Count>0) msg = "orders";
				if(myAccount.Positions.Count>0 && msg.Length>0) msg = msg +" and positions"; else msg = "positions";
				if(myAccount.Orders.Count>0 || myAccount.Positions.Count>0){
					MessageTime = DateTime.Now;
					Draw.TextFixed(this, "preexist", "Turn-on ChartTrader to possibly view your current "+msg, TextPosition.Center,Brushes.Black, new SimpleFont("Arial", 18), Brushes.Red, Brushes.Maroon, 90);
				}
			}
		}
		//=======================================================================================
		protected override void OnBarUpdate()
		{
			if (isIndicatorAdded)
			{
				if(AlertOnce) {
					Log("'EntryManager' does not support multiple instances. Please remove any additional instances of this indicator.", LogLevel.Alert);
					AlertOnce = false;
				}
				return;
			}
			atr0 = atr[0];
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
				// Grid already exists
				if (UserControlCollection.Contains(BuySellPanel))
				    return;
				AddControls();
				TicksFor_StopOrLimit_Btn.HorizontalContentAlignment = HorizontalAlignment.Left;

				UserControlCollection.Add(BuySellPanel);
            }
            #endregion

            #region - CleanToolBar -
            private void DisposeCleanUp()
            {
                if (chartWindow != null)
				{
					if(ChartControl.Dispatcher.CheckAccess()){
        	                if (MenuGrid != null)
            	            {
                	            UserControlCollection.Remove(BuySellPanel);
                    	    }
					}else{
	                    Dispatcher.BeginInvoke(new Action(() =>
    	                {
        	                if (MenuGrid != null)
            	            {
                	            UserControlCollection.Remove(BuySellPanel);
                    	    }

	                    }));
					}
                }
            }
            #endregion

            #region - AddControlsToToolbar -
            private void AddControls()
            {
				string uID = Guid.NewGuid().ToString().Replace("-",string.Empty);
				// Add a control grid which will host our custom buttons
				MenuGrid = new System.Windows.Controls.Grid
				{
					Name = "MenuGrid"+uID,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment   = VerticalAlignment.Top
				};
				BSMktGrid = new System.Windows.Controls.Grid
				{
					Name = "BSMktGrid"+uID,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment   = VerticalAlignment.Top
				};

                #region - Columns -
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
				var row6          = new System.Windows.Controls.RowDefinition();
				row6.Height       = new GridLength(50);

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
				var rowSpacer7    = new System.Windows.Controls.RowDefinition();
				rowSpacer7.Height = new GridLength(5);
				c1.Width = new GridLength(20);
				c2.Width = new GridLength(btnwidth);

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

				MenuGrid.ColumnDefinitions.Add(c1);
				MenuGrid.ColumnDefinitions.Add(c2);

				var c3 = new System.Windows.Controls.ColumnDefinition();
				var cspace = new System.Windows.Controls.ColumnDefinition();
				var c4 = new System.Windows.Controls.ColumnDefinition();
				c3.Width = new GridLength(btnwidth/2.0-1);
				cspace.Width = new GridLength(3);
				c4.Width = new GridLength(btnwidth/2.0-1);
				var row1bs          = new System.Windows.Controls.RowDefinition();
				row1bs.Height       = new GridLength(50);
				BSMktGrid.RowDefinitions.Add(row1bs);
				BSMktGrid.ColumnDefinitions.Add(c3);
				BSMktGrid.ColumnDefinitions.Add(cspace);
				BSMktGrid.ColumnDefinitions.Add(c4);

				if(pShowGoFlatButton){
					MenuGrid.RowDefinitions.Add(row5);
					MenuGrid.RowDefinitions.Add(rowSpacer6);
					MenuGrid.RowDefinitions.Add(row6);
					MenuGrid.RowDefinitions.Add(rowSpacer7);
					System.Windows.Controls.Grid.SetColumn(BSMktGrid, 1);
					System.Windows.Controls.Grid.SetRow(BSMktGrid, 11);
					MenuGrid.Children.Add(BSMktGrid);
				}else{
					MenuGrid.RowDefinitions.Add(row5);
					MenuGrid.RowDefinitions.Add(rowSpacer6);
					System.Windows.Controls.Grid.SetColumn(BSMktGrid, 1);
					System.Windows.Controls.Grid.SetRow(BSMktGrid, 9);
					MenuGrid.Children.Add(BSMktGrid);
				}

                #endregion

                #region - BuySellPanel -
				BuySellPanel = new System.Windows.Controls.WrapPanel();

				Thickness thickness = new Thickness(TogglePositionMarginLeft, TogglePositionMarginTop, TogglePositionMarginRight, TogglePositionMarginBottom);
				BuySellPanel.HorizontalAlignment = HorizontalAlignment.Left;
				BuySellPanel.VerticalAlignment   = VerticalAlignment.Top;
				BuySellPanel.Margin     = thickness;
				BuySellPanel.Background = Brushes.DimGray;
				BuySellPanel.Width = c1.Width.Value+2+btnwidth;


				drag_BuySell = new System.Windows.Controls.Primitives.Thumb();
				drag_BuySell.Width = c1.Width.Value;
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
					Content = ( pMode == EntryManager_Modes.Disabled ? "Disabled"
								: (object) string.Format("Fixed size:\n{0}",this.pPositionSize)),
					FontWeight = FontWeights.Bold,
				    FontSize   = btnFontSize,
					Width      = btnwidth,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = Brush_RiskButtonBkg,
					Foreground = Brush_RiskButtonText
				};
//				Print("Btn min width: "+PositionSize_Btn.MinWidth);
//				Print("Btn width: "+PositionSize_Btn.Width);
//				var f = new SimpleFont("Arial",32);
//				PositionSize_Btn.Width = f.Size.ConvertToHorizontalPixels(ChartControl);

				#region PositionSize events
				PositionSize_Btn.Click += delegate(System.Object o, RoutedEventArgs e)
                	{	e.Handled = true;
						var item = (System.Windows.Controls.Button)o;
						if(item.Content.ToString().Contains("Fixed")){
							pMode = EntryManager_Modes.Disabled;
							item.Content = (object) "Disabled";
							if(Buy_Btn != null){
								Buy_Btn.Background = Brushes.Silver;
								Buy_Btn.Foreground = Brushes.LightGray;
							}else DebugPrint("Buy_Btn was null! (2)");
							if(BuyMkt_Btn != null){
								BuyMkt_Btn.Background = Brushes.Silver;
								BuyMkt_Btn.Foreground = Brushes.LightGray;
							}else DebugPrint("BuyMkt_Btn was null! (2)");
							if(Sell_Btn != null){
								Sell_Btn.Background = Brushes.Silver;
								Sell_Btn.Foreground = Brushes.LightGray;
							}else DebugPrint("Sell_Btn was null! (2)");
							if(SellMkt_Btn != null){
								SellMkt_Btn.Background = Brushes.Silver;
								SellMkt_Btn.Foreground = Brushes.LightGray;
							}else DebugPrint("SellMkt_Btn was null! (2)");
						}
						else if(item.Content.ToString().Contains("Disabled")) {
							pMode = EntryManager_Modes.Fixed;
							item.Content = (object) (object) string.Format("Fixed size:\n{0}",this.pPositionSize);
							if(Buy_Btn != null){
								Buy_Btn.Background  = Brush_BuyButtonBkg;
								Buy_Btn.Foreground  = Brush_BuyButtonText;
							}else DebugPrint("Buy_Btn was null! (3)");
							if(BuyMkt_Btn != null){
								BuyMkt_Btn.Background  = Brush_BuyButtonBkg;
								BuyMkt_Btn.Foreground  = Brush_BuyButtonText;
							}else DebugPrint("BuyMkt_Btn was null! (3)");
							if(Sell_Btn != null){
								Sell_Btn.Background = Brush_SellButtonBkg;
								Sell_Btn.Foreground = Brush_SellButtonText;
							}else DebugPrint("Sell_Btn was null! (3)");
							if(SellMkt_Btn != null){
								SellMkt_Btn.Background = Brush_SellButtonBkg;
								SellMkt_Btn.Foreground = Brush_SellButtonText;
							}else DebugPrint("SellMkt_Btn was null! (3)");
						}
//							item.Content = (object) string.Format("Risk {0}", PctRisk.ToString("0.0%").Replace(".0%","%"));
					};
				PositionSize_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
//Print(DateTime.Now.ToString()+"   QuantityMode: "+(QuantityMode==QTY_RISKPCT ? "RiskPct" : (QuantityMode==QTY_FIXED ? "Fixed" : QuantityMode.ToString()));
						e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						this.pPositionSize = Math.Max(this.MinContracts, Math.Min(this.MaxContracts, this.pPositionSize + 1 * Math.Sign(e.Delta)));
						if(!item.Content.ToString().Contains("Disabled")){
							item.Content = (object) string.Format("Fixed size:\n{0}",this.pPositionSize);
							ForceRefresh();
						}
					};
				#endregion

				System.Windows.Controls.Grid.SetColumn(PositionSize_Btn, 1);
				System.Windows.Controls.Grid.SetRow(PositionSize_Btn, 1);
				MenuGrid.Children.Add(PositionSize_Btn);
				#endregion

				#region - Buy_Btn -
				//---------------------------------------------------------------------
//Print("BuyBtn created");
				Buy_Btn = new System.Windows.Controls.Button
				{
				    Name       = "Buy_Btn"+uID,
				    Content    = (SelectedBuyEntryType==EntryType_LIMIT ? string.Format("BUY Limit {0}x",pEntriesCount) : ((SelectedBuyEntryType==EntryType_MKTLIMIT || SelectedBuyEntryType==EntryType_MARKET) ? string.Format("BUY MktLimit {0}x",pEntriesCount) : "BUY Stop")),
					ToolTip    = "Mousewheel to cycle thru Stop/Limit/MktLimit options",
				    FontSize   = btnFontSize,
					FontWeight = FontWeights.Bold,
					Width      = btnwidth,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background  = Brush_BuyButtonBkg,
					Foreground  = Brush_BuyButtonText
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
					ForceRefresh();
				};
				#endregion
				#region MouseLeave
				Buy_Btn.MouseLeave += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('B');
					ForceRefresh();
				};
				#endregion
				#region MouseWheel
				Buy_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
						e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						{
//( ? "BUY MktLimit" : 
							if(SelectedBuyEntryType == EntryType_MKTLIMIT){
								if(e.Delta>0)	{item.Content = (object) string.Format("BUY Limit {0}x",pEntriesCount);SelectedBuyEntryType = EntryType_LIMIT;}
								else			{item.Content = (object) string.Format("BUY Stop");    SelectedBuyEntryType = EntryType_STOP;}
							}
//							else if(SelectedBuyEntryType == EntryType_MARKET) {
//								if(e.Delta>0) {item.Content = (object) string.Format("BUY MktLimit {0}x",pEntriesCount);SelectedBuyEntryType = EntryType_MKTLIMIT;}
//								else		  {item.Content = (object) string.Format("BUY Stop");    SelectedBuyEntryType = EntryType_STOP;}
//							}
							else if(SelectedBuyEntryType == EntryType_STOP) {
								if(e.Delta>0)	{item.Content = (object) string.Format("BUY MktLimit {0}x",pEntriesCount);SelectedBuyEntryType = EntryType_MKTLIMIT;}
								else			{item.Content = (object) string.Format("BUY Limit {0}x",pEntriesCount);SelectedBuyEntryType = EntryType_LIMIT;}
							}
							else if(SelectedBuyEntryType == EntryType_LIMIT) {
								if(e.Delta>0) {item.Content = (object) string.Format("BUY Stop");    SelectedBuyEntryType = EntryType_STOP;}
								else		  {item.Content = (object) string.Format("BUY MktLimit {0}x",pEntriesCount);SelectedBuyEntryType = EntryType_MKTLIMIT;}
							}
							#region -- Set background of TicksFor_StopOrLimit_Btn --
							if(SelectedBuyEntryType != EntryType_MARKET){
//								TicksFor_StopOrLimit_Btn.Background = Brushes.Blue;
//								TicksFor_StopOrLimit_Btn.Foreground = Brushes.LightGray;
								if(SelectedBuyEntryType == EntryType_STOP)
									TicksFor_StopOrLimit_Btn.Content    = (object)(string.Format("Stop Entry\n{0}-ticks", this.EntryStopTicksOffset));
								else if(SelectedBuyEntryType == EntryType_LIMIT || SelectedBuyEntryType == EntryType_MKTLIMIT)
									TicksFor_StopOrLimit_Btn.Content    = (object)(string.Format("Limit Entry\n{0}-ticks", this.EntryLimitTicksOffset));
							}
						}
						#endregion ---------------------------
						ForceRefresh();
					};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				System.Windows.Controls.Grid.SetColumn(Buy_Btn, 1);
				System.Windows.Controls.Grid.SetRow(Buy_Btn, 3);
				MenuGrid.Children.Add(Buy_Btn);
				//---------------------------------------------------------------------
				#endregion

				#region - TicksFor_StopOrLimit_Btn -
//Print("SellBtn created");
				//---------------------------------------------------------------------
				TicksFor_StopOrLimit_Btn = new System.Windows.Controls.Button
				{
				    Name       = "StopLimTicks"+uID,
				    Content    = (object)(string.Format("Limit Entry\n{0}-ticks", this.EntryLimitTicksOffset)),
					FontWeight = FontWeights.Bold,
				    FontSize   = btnFontSize,
					Width      = btnwidth,
					ToolTip    = "Click to cycle thru Stop and Limit and Entries",
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background = this.Brush_EntryDistanceButtonBkg,
					Foreground = this.Brush_EntryDistanceButtonText
				};

				#region TicksFor_StopOrLimit_Btn Events
				//---------------------------------------------------------------------
				#region Click
				TicksFor_StopOrLimit_Btn.Click += delegate(object o, RoutedEventArgs e){
					e.Handled=true;
					if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Stop")){
						TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Limit Entry\n{0}-ticks", this.EntryLimitTicksOffset));
					}else if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Limit")){
						TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Entries {0}", this.pEntriesCount));
					}else{
						TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Stop Entry\n{0}-ticks", this.EntryStopTicksOffset));
					}
					MenuGrid.ColumnDefinitions[1].Width = new GridLength(btnwidth);
					ForceRefresh();
				};
				#endregion
				#region MouseWheel
				TicksFor_StopOrLimit_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
				{
					e.Handled=true;
					if(e.Delta>0){
						if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Entries"))
							TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Entries {0}", ++this.pEntriesCount));
						else if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Stop"))
							TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Stop Entry\n{0}-ticks", ++this.EntryStopTicksOffset));
						else
							TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Limit Entry\n{0}-ticks", ++this.EntryLimitTicksOffset));
					}else{
						if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Entries")){
							if(pEntriesCount>1) pEntriesCount--;
							TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Entries {0}", this.pEntriesCount));
						}else if(TicksFor_StopOrLimit_Btn.Content.ToString().Contains("Stop"))
							TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Stop Entry\n{0}-ticks", --this.EntryStopTicksOffset));
						else
							TicksFor_StopOrLimit_Btn.Content = (object)(string.Format("Limit Entry\n{0}-ticks", --this.EntryLimitTicksOffset));
					}
					ForceRefresh();
				};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				TicksFor_StopOrLimit_Btn.Width = btnwidth;
				System.Windows.Controls.Grid.SetColumn(TicksFor_StopOrLimit_Btn, 1);
				System.Windows.Controls.Grid.SetRow(TicksFor_StopOrLimit_Btn, 5);
				MenuGrid.Children.Add(TicksFor_StopOrLimit_Btn);
				//---------------------------------------------------------------------
				#endregion

				#region - Sell_Btn -
//Print("SellBtn created");
				//---------------------------------------------------------------------
				Sell_Btn = new System.Windows.Controls.Button
				{
				    Name       = "Sell_Btn"+uID,
				    Content    = (SelectedSellEntryType==EntryType_LIMIT ? string.Format("SELL Limit {0}x",pEntriesCount) : ((SelectedSellEntryType==EntryType_MKTLIMIT || SelectedSellEntryType==EntryType_MARKET) ? string.Format("SELL MktLimit {0}x",pEntriesCount) : "SELL Stop")),
					ToolTip    = "Mousewheel to cycle thru Stop/Limit/MktLimit options",
					FontWeight = FontWeights.Bold,
				    FontSize   = btnFontSize,
					Width      = btnwidth,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background  = Brush_SellButtonBkg,
					Foreground  = Brush_SellButtonText
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
					ForceRefresh();
				};
				#endregion
				#region MouseLeave
				Sell_Btn.MouseLeave += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('S');
					ForceRefresh();
				};
				#endregion
				#region MouseWheel
				Sell_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
					{
						e.Handled=true;
						var item = (System.Windows.Controls.Button)o;
						{
							if(SelectedSellEntryType == EntryType_MKTLIMIT) {
								if(e.Delta>0) {item.Content = (object) string.Format("SELL Limit {0}x",pEntriesCount);SelectedSellEntryType = EntryType_LIMIT;}
								else		  {item.Content = (object) string.Format("SELL Stop");SelectedSellEntryType = EntryType_STOP;}
//							}else if(SelectedSellEntryType == EntryType_MARKET) {
//								if(e.Delta>0) {item.Content = (object) string.Format("SELL MktLimit {0}x",pEntriesCount);SelectedSellEntryType = EntryType_MKTLIMIT;}
//								else		  {item.Content = (object) string.Format("SELL Stop");SelectedSellEntryType = EntryType_STOP;}
							}
							else if(SelectedSellEntryType == EntryType_STOP) {
								if(e.Delta>0) {item.Content = (object) string.Format("SELL MktLimit {0}x",pEntriesCount);SelectedSellEntryType = EntryType_MKTLIMIT;}
								else		  {item.Content = (object) string.Format("SELL Limit {0}x",pEntriesCount);SelectedSellEntryType = EntryType_LIMIT;}
							}
							else if(SelectedSellEntryType == EntryType_LIMIT) {
								if(e.Delta>0) {item.Content = (object) string.Format("SELL Stop");SelectedSellEntryType = EntryType_STOP;}
								else		  {item.Content = (object) string.Format("SELL MktLimit {0}x",pEntriesCount);SelectedSellEntryType = EntryType_MKTLIMIT;}
							}
							#region -- Set background of TicksFor_StopOrLimit_Btn --
							if(SelectedSellEntryType != EntryType_MARKET){
//								TicksFor_StopOrLimit_Btn.Background = Brush_EntryDistanceButtonBkg;
//								TicksFor_StopOrLimit_Btn.Foreground = Brush_EntryDistanceButtonText;
								if(SelectedSellEntryType == EntryType_STOP)
									TicksFor_StopOrLimit_Btn.Content    = (object)(string.Format("Stop Entry\n{0}-ticks", this.EntryStopTicksOffset));
								else if(SelectedSellEntryType == EntryType_LIMIT || SelectedSellEntryType == EntryType_MKTLIMIT)
									TicksFor_StopOrLimit_Btn.Content    = (object)(string.Format("Limit Entry\n{0}-ticks", this.EntryLimitTicksOffset));
							}
						}
						#endregion ---------------------------
						ForceRefresh();
					};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				System.Windows.Controls.Grid.SetColumn(Sell_Btn, 1);
				System.Windows.Controls.Grid.SetRow(Sell_Btn, 7);
				MenuGrid.Children.Add(Sell_Btn);
				//---------------------------------------------------------------------
				#endregion

				#region - GoFlat_Btn -
				//---------------------------------------------------------------------
				GoFlat_Btn = new System.Windows.Controls.Button
				{
				    Name       = "GoFlat_Btn"+uID,
				    Content    = "Go Flat All",
					FontWeight = FontWeights.Bold,
				    FontSize   = btnFontSize,
					Width      = btnwidth,
					HorizontalContentAlignment = HorizontalAlignment.Left,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background  = Brush_GoFlatButtonBkg,
					Foreground  = Brush_GoFlatButtonText
				};

				#region GoFlat_Btn Events
				//---------------------------------------------------------------------
				GoFlat_Btn.Click += GoFlat_Btn_Click;
				#region MouseWheel
				GoFlat_Btn.MouseWheel += delegate(object o, MouseWheelEventArgs e)
				{
					e.Handled=true;
					var item = (System.Windows.Controls.Button)o;
					{
//( ? "BUY MktLimit" : 
						if(item.Content.ToString().Contains("All")){
							{item.Content = (object) ("Go Flat "+Instrument.MasterInstrument.Name);}
						}else{
							{item.Content = (object) "Go Flat All";}
						}
					}
					ForceRefresh();
				};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				if(pShowGoFlatButton){
					System.Windows.Controls.Grid.SetColumn(GoFlat_Btn, 1);
					System.Windows.Controls.Grid.SetRow(GoFlat_Btn, 9);
					MenuGrid.Children.Add(GoFlat_Btn);
				}
				//---------------------------------------------------------------------
				#endregion

				#region - BuyMkt_Btn -
				//---------------------------------------------------------------------
//Print("BuyBtn created");
				BuyMkt_Btn = new System.Windows.Controls.Button
				{
				    Name       = "BuyMkt_Btn"+uID,
				    Content    = "B Mkt",
				    FontSize   = btnFontSize,
					FontWeight = FontWeights.Bold,
					Width      = btnwidth/2.0-2,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background  = Brush_BuyButtonBkg,
					Foreground  = Brush_BuyButtonText
				};

				#region BuyMkt_Btn Events
				//---------------------------------------------------------------------
				BuyMkt_Btn.Click += Buy_Btn_Click;

				#region MouseEnter
				BuyMkt_Btn.MouseEnter += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('B');
					BuySellLabel = "BuyMkt";
					ForceRefresh();
				};
				#endregion
				#region MouseLeave
				BuyMkt_Btn.MouseLeave += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('B');
					ForceRefresh();
				};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				System.Windows.Controls.Grid.SetColumn(BuyMkt_Btn, 0);
				System.Windows.Controls.Grid.SetRow(BuyMkt_Btn, 0);
				BSMktGrid.Children.Add(BuyMkt_Btn);
				//---------------------------------------------------------------------
				#endregion
				#region - SellMkt_Btn -
//Print("SellBtn created");
				//---------------------------------------------------------------------
				SellMkt_Btn = new System.Windows.Controls.Button
				{
				    Name       = "SellMkt_Btn"+uID,
				    Content    = "S Mkt",
					FontWeight = FontWeights.Bold,
				    FontSize   = btnFontSize,
					Width      = btnwidth/2.0-2,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
				    VerticalContentAlignment   = VerticalAlignment.Center,
					Background  = Brush_SellButtonBkg,
					Foreground  = Brush_SellButtonText
				};

				#region SellMkt_Btn Events
				//---------------------------------------------------------------------
				SellMkt_Btn.Click += Sell_Btn_Click;

				#region MouseEnter
				SellMkt_Btn.MouseEnter += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('S');
					BuySellLabel = "SellMkt";
					ForceRefresh();
				};
				#endregion
				#region MouseLeave
				SellMkt_Btn.MouseLeave += delegate(object o, MouseEventArgs e){
					e.Handled=true;
					//char result;
					EntryDirection_OnHoverQue.Clear();
					EntryDirection_OnHoverQue.Push('S');
					ForceRefresh();
				};
				#endregion
				//---------------------------------------------------------------------
				#endregion

				System.Windows.Controls.Grid.SetColumn(SellMkt_Btn, 2);
				System.Windows.Controls.Grid.SetRow(SellMkt_Btn, 0);
				BSMktGrid.Children.Add(SellMkt_Btn);
				//---------------------------------------------------------------------
				#endregion


				#region initialize if Disabled
				if(pMode == EntryManager_Modes.Disabled){
					pMode = EntryManager_Modes.Disabled;
					if(Buy_Btn != null){
						Buy_Btn.Background = Brushes.Silver;
						Buy_Btn.Foreground = Brushes.LightGray;
					}else DebugPrint("Buy_Btn was null! (2)");
					if(BuyMkt_Btn != null){
						BuyMkt_Btn.Background = Brushes.Silver;
						BuyMkt_Btn.Foreground = Brushes.LightGray;
					}else DebugPrint("BuyMkt_Btn was null! (2)");
					if(Sell_Btn != null){
						Sell_Btn.Background = Brushes.Silver;
						Sell_Btn.Foreground = Brushes.LightGray;
					}else DebugPrint("Sell_Btn was null! (2)");
					if(SellMkt_Btn != null){
						SellMkt_Btn.Background = Brushes.Silver;
						SellMkt_Btn.Foreground = Brushes.LightGray;
					}else DebugPrint("SellMkt_Btn was null! (2)");
				}
				#endregion
				BuySellPanel.Children.Add(MenuGrid);
            }
            #endregion

			#region - Buy_Btn_Click -
            void Buy_Btn_Click(object sender, EventArgs e)
            {
				if(pMode == EntryManager_Modes.Disabled) return;
				if(myAccount == null) {Log("No account selected, no trading permitted",LogLevel.Alert);return;}
				//Print("Submitting entry BUY order on '"+Instrument.FullName+"'");
				int entry_type = SelectedBuyEntryType;
				System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
				if(b.Name.StartsWith("BuyMkt")) entry_type = EntryType_MARKET;

				var ordertype  = OrderType.Market;
				if(entry_type == EntryType_LIMIT || entry_type == EntryType_MKTLIMIT){
					ordertype  = OrderType.Limit;
					entryprice = Opens[0].GetValueAt(CurrentBars[0]) - this.EntryLimitTicksOffset * TickSize;
				}else if(entry_type == EntryType_STOP){
					ordertype  = OrderType.StopMarket;
					entryprice = Opens[0].GetValueAt(CurrentBars[0]) + this.EntryStopTicksOffset * TickSize;
				}
//				if(pUseChartTraderAtmStrategyName) GetAtmStrategyName();
	try{
				if(ATMStrategyName.Trim().Length>0 && ATMStrategyName.ToUpper()!="N/A"){
					#region -- Enter ATM Strategy orders --
					var REQUIRED_NAME = "Entry";//NT8 docs state this is a required name for ATM Strategy orders
					if(entry_type == EntryType_MKTLIMIT){
						var ATMorder = myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.Buy, 
										OrderType.Market, 
										OrderEntry.Automated, 
										TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), 0, 0, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						ATMorder = myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.Buy, 
										OrderType.Limit, 
										OrderEntry.Automated, 
										TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						var p = entryprice;
						for(int i = 2; i<=pEntriesCount; i++){
							p = p - EntryLimitTicksOffset*TickSize;
							ATMorder = myAccount.CreateOrder(
											Instrument.GetInstrument(Instrument.FullName), 
											OrderAction.Buy, 
											OrderType.Limit, 
											OrderEntry.Automated, 
											TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
							NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						}
					}else{
						var ATMorder = myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.Buy, 
										ordertype, 
										OrderEntry.Automated, 
										TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						if(entry_type == EntryType_LIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p - EntryLimitTicksOffset*TickSize;
								ATMorder = myAccount.CreateOrder(
												Instrument.GetInstrument(Instrument.FullName), 
												OrderAction.Buy, 
												OrderType.Limit, 
												OrderEntry.Automated, 
												TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
								NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
							}
						}
					}
					#endregion
				} else {
					//do not permit rapid re-entry of an entry order...accidental double-clicks
					var ts = new TimeSpan(DateTime.Now.Ticks-ClickToEnterTime.Ticks);
					if(ts.TotalSeconds<5) {
						Log("2nd buy button click ignored...market entries must be no closer than 5-seconds apart", LogLevel.Information);
						return;
					}
					ClickToEnterTime = DateTime.Now;

					var OrdersList = new List<Order>();
					if(entry_type == EntryType_MKTLIMIT){
						OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Buy, 
									OrderType.Market, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), 0, 0, string.Empty, "Buy", Core.Globals.MaxDate, null));
						OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Buy, 
									OrderType.Limit, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, "LimBuy1", Core.Globals.MaxDate, null));
						var p = entryprice;
						for(int i = 2; i<=pEntriesCount; i++){
							p = p - EntryLimitTicksOffset*TickSize;
							OrdersList.Add(myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.Buy, 
										OrderType.Limit, 
										OrderEntry.Automated, 
										TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, string.Format("LimBuy{0}",i), Core.Globals.MaxDate, null));
						}
					}else{
						OrdersList.Add(myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.Buy, 
									ordertype, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, "Buy", Core.Globals.MaxDate, null));
						if(entry_type == EntryType_LIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p - EntryLimitTicksOffset*TickSize;
								OrdersList.Add(myAccount.CreateOrder(
											Instrument.GetInstrument(Instrument.FullName), 
											OrderAction.Buy, 
											OrderType.Limit, 
											OrderEntry.Automated, 
											TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, string.Format("LimBuy{0}",i), Core.Globals.MaxDate, null));
							}
						}
					}
					if(ChartControl.Dispatcher.CheckAccess()){
						myAccount.Submit(OrdersList.ToArray());
					}else{
						ChartControl.Dispatcher.InvokeAsync((Action)(() =>{	myAccount.Submit(OrdersList.ToArray()); 
						}));
					}
				}
	}catch(Exception eorder){
		if(pPositionSize==0) Log("Order Rejected - your risk parameters are calculating a zero quantity", LogLevel.Warning);
		else Log(Log_Order_Reject_Message.Replace("''","'"+(pUseChartTraderAccount ? myAccount.Name:SelectedAccountName)+"'"), LogLevel.Warning);
		MessageTime = DateTime.Now;
		Draw.TextFixed(this,"OrderError", Order_Reject_Message.Replace("''","'"+(pUseChartTraderAccount ? myAccount.Name:SelectedAccountName)+"'"), TextPosition.Center,Brushes.Magenta,new SimpleFont("Arial",14),Brushes.DimGray,Brushes.Black,100);
		ForceRefresh();
	}
            }
			#endregion

			#region - Sell_Btn_Click -
            void Sell_Btn_Click(object sender, EventArgs e)
            {
				if(pMode == EntryManager_Modes.Disabled) return;
				if(myAccount == null) {Log("No account selected, no trading permitted",LogLevel.Alert);return;}
				int entry_type = SelectedSellEntryType;
				System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
				if(b.Name.StartsWith("SellMkt")) entry_type = EntryType_MARKET;

				var ordertype   = OrderType.Market;
				if(entry_type == EntryType_LIMIT){
					ordertype= OrderType.Limit;
					entryprice = Opens[0].GetValueAt(CurrentBars[0]) + this.EntryLimitTicksOffset * TickSize;
				}else if(entry_type == EntryType_STOP){
					ordertype= OrderType.StopMarket;
					entryprice = Opens[0].GetValueAt(CurrentBars[0]) - this.EntryStopTicksOffset * TickSize;
				}
//				if(pUseChartTraderAtmStrategyName) GetAtmStrategyName();
//Print("SELL entry_type:  "+entry_type);
	try{
				if(ATMStrategyName.Trim().Length>0 && ATMStrategyName.ToUpper()!="N/A"){
					#region ATM Strategy Orders
					var REQUIRED_NAME = "Entry";//NT8 docs state this is a required name for ATM Strategy orders
					if(entry_type == EntryType_MKTLIMIT){
						var ATMorder = myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.SellShort, 
									OrderType.Market, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), 0, 0, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						ATMorder = myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.SellShort, 
									OrderType.Limit,
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						var p = entryprice;
						for(int i = 2; i<=pEntriesCount; i++){
							p = p + EntryLimitTicksOffset*TickSize;
							ATMorder = myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.SellShort, 
										OrderType.Limit,
										OrderEntry.Automated, 
										TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
							NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						}
					}else{
						var ATMorder = myAccount.CreateOrder(
									Instrument.GetInstrument(Instrument.FullName), 
									OrderAction.SellShort, 
									ordertype, 
									OrderEntry.Automated, 
									TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
						if(entry_type == EntryType_LIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p + EntryLimitTicksOffset*TickSize;
								ATMorder = myAccount.CreateOrder(
											Instrument.GetInstrument(Instrument.FullName), 
											OrderAction.SellShort, 
											OrderType.Limit,
											OrderEntry.Automated, 
											TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, REQUIRED_NAME, Core.Globals.MaxDate, null); 
								NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(ATMStrategyName.Trim(), ATMorder);
							}
						}
					}
					#endregion
				} else {
//Print(1087);Print("min: "+MinContracts+"  max: "+MaxContracts+"   pPositionSize: "+pPositionSize+"  entryprice: "+entryprice);
					//do not permit rapid re-entry of an entry order...accidental double-clicks
					var ts = new TimeSpan(DateTime.Now.Ticks-ClickToEnterTime.Ticks);
					if(ts.TotalSeconds<5) {
						Log("2nd sell button click ignored...market entries must be no closer than 5-seconds apart", LogLevel.Information);
						return;
					}
					ClickToEnterTime = DateTime.Now;

					var OrdersList = new List<Order>();
					if(entry_type == EntryType_MKTLIMIT){
						OrdersList.Add(myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.SellShort, 
								OrderType.Market, 
								OrderEntry.Automated, 
								TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), 0, 0, string.Empty, "Sell", Core.Globals.MaxDate, null));
						OrdersList.Add(myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.SellShort, 
								OrderType.Limit, 
								OrderEntry.Automated, 
								TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, "LimSell1", Core.Globals.MaxDate, null));
						var p = entryprice;
						for(int i = 2; i<=pEntriesCount; i++){
							p = p + EntryLimitTicksOffset*TickSize;
							OrdersList.Add(myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.SellShort, 
										OrderType.Limit, 
										OrderEntry.Automated, 
										TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, string.Format("LimSell{0}",i), Core.Globals.MaxDate, null));
						}
					}else{
						OrdersList.Add(myAccount.CreateOrder(
								Instrument.GetInstrument(Instrument.FullName), 
								OrderAction.SellShort, 
								ordertype, 
								OrderEntry.Automated, 
								TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), entryprice, entryprice, string.Empty, "Sell", Core.Globals.MaxDate, null));
						if(entry_type == EntryType_LIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p + EntryLimitTicksOffset*TickSize;
								OrdersList.Add(myAccount.CreateOrder(
											Instrument.GetInstrument(Instrument.FullName), 
											OrderAction.SellShort, 
											OrderType.Limit, 
											OrderEntry.Automated, 
											TimeInForce.Day, (int)Math.Max(MinContracts,Math.Min(MaxContracts,pPositionSize)), p, p, string.Empty, string.Format("LimSell{0}",i), Core.Globals.MaxDate, null));
							}
						}
					}
					if(ChartControl.Dispatcher.CheckAccess()){
						myAccount.Submit(OrdersList.ToArray());
					}else{
						ChartControl.Dispatcher.InvokeAsync((Action)(() =>{	myAccount.Submit(OrdersList.ToArray()); 
						}));
					}
				}
	}catch(Exception eorder){
		if(pPositionSize==0) Log("Order Rejected - your risk parameters are calculating a zero quantity", LogLevel.Warning);
		else Log(Log_Order_Reject_Message.Replace("''","'"+(pUseChartTraderAccount ? myAccount.Name:SelectedAccountName)+"'"), LogLevel.Warning);
		MessageTime = DateTime.Now;
		Draw.TextFixed(this,"OrderError",Order_Reject_Message.Replace("''","'"+(pUseChartTraderAccount ? myAccount.Name:SelectedAccountName)+"'"), TextPosition.Center,Brushes.Magenta,new SimpleFont("Arial",14),Brushes.DimGray,Brushes.Black,100);
		ForceRefresh();
	}
            }
            #endregion

			#region - GoFlat_Btn_Click -
            void GoFlat_Btn_Click(object sender, EventArgs e)
            {
				if(myAccount==null) return;
				Account acct = null;
				lock (Account.All)
					acct = Account.All.FirstOrDefault(a => a.Name == myAccount.Name);
				//Collection<Instrument> instrumentsToClose = new Collection<Instrument>();
				List<Instrument> instrumentsToClose = new List<Instrument>();
				// add instruments to the collection
//Print("Btn: "+GoFlat_Btn.Content.ToString());
				if(GoFlat_Btn.Content.ToString().Contains("All")){
					foreach(var pos in acct.Positions){
						if(!instrumentsToClose.Contains(pos.Instrument)){
							instrumentsToClose.Add(pos.Instrument);
						}
					}
					foreach(var ord in acct.Orders){
//Print("order: "+ord.ToString());
						if(!instrumentsToClose.Contains(ord.Instrument)){
//Print("Symbol: "+ord.Instrument);
							instrumentsToClose.Add(ord.Instrument);
						}
					}
				}else{
					var symbol = Instrument.FullName;
					foreach(var pos in acct.Positions){
						if(pos.Instrument.FullName.CompareTo(symbol)==0){
							if(!instrumentsToClose.Contains(pos.Instrument)){
								instrumentsToClose.Add(pos.Instrument);
							}
						}
					}
					foreach(var ord in acct.Orders){
						if(ord.Instrument.FullName.CompareTo(symbol)==0){
//Print("Symbol: "+symbol);
							if(!instrumentsToClose.Contains(ord.Instrument)){
								instrumentsToClose.Add(ord.Instrument);
							}
						}
					}
				}
				if(instrumentsToClose.Count>0)
					myAccount.Flatten(instrumentsToClose);
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
					else NinjaScript.Log(string.Format("'{0}' connection status: {1}", Account.All[i].Name,Account.All[i].ConnectionStatus.ToString()), LogLevel.Information);
		        }
				
		        return new TypeConverter.StandardValuesCollection(list);
		    }

		    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		    { return true; }
			#endregion
		}
		[Display(Name = "Use ChartTrader acct?", GroupName = "Order Parameters", Description = "", Order = 5)]
		public bool pUseChartTraderAccount {get;set;}

//		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadAccountNameList))]
		[Display(Name = "Account Name", GroupName = "Order Parameters", Description = "", Order = 10)]
		public string SelectedAccountName	{get;set;}

		[Display(Name = "Default Entry Order Type", GroupName = "Order Parameters", Description = "Set to Market, Limit or Stop", Order = 15)]
		public string pDefaultOrderType {get;set;}
		[Display(Name = "Ticks on LIMIT", GroupName = "Order Parameters", Description = "Distance (in ticks) from market price to LIMIT entry price (only used when you submit a LIMIT entry order", Order = 20)]
		public int EntryLimitTicksOffset { get; set; }
		[Display(Name = "Ticks on STOP", GroupName = "Order Parameters", Description = "Distance (in ticks) from market price to STOP entry price (only used when you submit a STOP entry order", Order = 30)]
		public int EntryStopTicksOffset { get; set; }

		[Display(Name = "Use ChartTrader ATM Strategy Name?", GroupName = "ATM Parameters", Description = "", Order = 10)]
		public bool pUseChartTraderAtmStrategyName {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadATMStrategyNames))]
		[Display(Name = "ATM Strategy Name", GroupName = "ATM Parameters", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template", Order = 15)]
		public string ATMStrategyName { get; set; }

		[Range(1,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "MaxContracts", GroupName = "Risk Parameters", Description = "Max number of contracts permitted", Order = 30)]
		public int MaxContracts { get; set; }

		[Range(0,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "MinContracts", GroupName = "Risk Parameters", Description = "Min number of contracts permitted", Order = 35)]
		public int MinContracts { get; set; }

		[Range(1,int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Entries #", GroupName = "Risk Parameters", Description = "", Order = 40)]
		public int pEntriesCount { get; set; }

		[NinjaScriptProperty]
		[Display(Order = 5, Name = "Mode: Disabled/Fixed", GroupName = "Visual Options", Description = "Enable the indicator")]
		public EntryManager_Modes pMode {get;set;}

		[Display(Order = 7, Name = "Pennant Loc", GroupName = "Visual Options", Description = "Left of current price, Right of current price")]
		public EntryManager_PennantLocs pPennantLocs {get;set;}

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

		[Display(Order = 70, Name = "Show Lag Time", GroupName = "Visual Options", Description = "Location of lag time (time since last price tick)")]
		public EntryManager_LagTimeLocation pLagTimerLocation {get;set;}
		
		[Display(Order = 80, Name = "Lag Timer Font", GroupName = "Visual Options", Description = "")]
		public SimpleFont pLagWarningFont {get;set;}

		[Display(Order = 90, Name = "Lag Time warning (seconds)", GroupName = "Visual Options", Description = "Max number of seconds of lag before Lag Timer message is displayed")]
		public int pLagWarningSeconds {get;set;}

		[Range(10,int.MaxValue)]
		[Display(Order = 100, Name = "Button Width", GroupName = "Visual Options", Description = "Button width in pixels")]
		public int btnwidth {get;set;}

		[Range(6,int.MaxValue)]
		[Display(Order = 110, Name = "Button Font size", GroupName = "Visual Options", Description = "")]
		public int btnFontSize {get;set;}
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

		[Display(Order = 195, Name = "Show Go Flat?", GroupName = "Visual Options", Description = "")]
		public bool pShowGoFlatButton {get;set;}

		[XmlIgnore]
		[Display(Order = 200, Name = "GoFlat Button Bkg", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_GoFlatButtonBkg {get;set;}
			[Browsable(false)]
			public string Brush_GoFlatButtonBkg_{	get { return Serialize.BrushToString(Brush_GoFlatButtonBkg); } set { Brush_GoFlatButtonBkg = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 210, Name = "GoFlat Button Text", GroupName = "Visual Options", ResourceType = typeof(Custom.Resource))]
		public Brush Brush_GoFlatButtonText {get;set;}
			[Browsable(false)]
			public string Brush_GoFlatButtonText_{	get { return Serialize.BrushToString(Brush_GoFlatButtonText); } set { Brush_GoFlatButtonText = Serialize.StringToBrush(value); }}

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
			if(pPennantLocs == EntryManager_PennantLocs.Left){
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
				if(pPennantLocs == EntryManager_PennantLocs.Left){
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+this.LineLengthF,y), brush, LineWidth);
				}else if(pPennantLocs == EntryManager_PennantLocs.Right){
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f+this.LineLengthF,y), brush, LineWidth);
				}
				if(OffsetUpward) y = y - textFormat.FontSize-2f;
//				if(pPennantLocs == EntryManager_PennantLocs.Right){
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
//			if(pPennantLocs == EntryManager_PennantLocs.Left){
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
				if(pPennantLocs == EntryManager_PennantLocs.Left){
					if(LineWidth>0 && LineLengthF>0) 
						RenderTarget.DrawLine(new SharpDX.Vector2(x+textLayout.Metrics.Width*0.8f,y), new SharpDX.Vector2(x+this.LineLengthF,y), brush, LineWidth);
				}else if(pPennantLocs == EntryManager_PennantLocs.Right){
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
				if(pPennantLocs == EntryManager_PennantLocs.Right){
					x = ChartControl.GetXByBarIndex(ChartBars, abar) + ChartControl.Properties.BarDistance + 15f;
				}
				RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),15f,8f), brush);
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
		if(pLagTimerLocation != EntryManager_LagTimeLocation.NoDisplay){
			#region -- Lag Timer --
			var ts = new TimeSpan(DateTime.Now.Ticks-TimeOfLastTick.Ticks);
			var lag_msg = "";
			bool warning = false;
			double secondslag = ts.TotalSeconds;
			if(ts.TotalSeconds > 20) secondslag = 99;
//			if(pUseChartTraderAtmStrategyName) GetAtmStrategyName();
			if(Math.Abs(ts.TotalSeconds) > pLagWarningSeconds){
				lag_msg = string.Format("Data is lagging by {0}{1}-seconds\n{2}\n{3}", secondslag==99 ? ">":"", Math.Abs(secondslag).ToString("0.0"), this.ATMStrategyEnabled ? this.ATMStrategyName:"no ATM selected", myAccount!=null ? myAccount.Name: "No Account Selected");
				warning = true;
			}
			else
				lag_msg = string.Format("Lag is less than {0}-secs\n{1}\n{2}", pLagWarningSeconds,this.ATMStrategyEnabled?this.ATMStrategyName:"no ATM selected", myAccount!=null ? myAccount.Name: "No Account Selected");

			switch (pLagTimerLocation){
				case EntryManager_LagTimeLocation.TopLeft:
					Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.TopLeft, Brushes.White, pLagWarningFont, Brushes.DimGray, warning ? Brushes.Maroon:Brushes.Black, 100);break;
				case EntryManager_LagTimeLocation.TopRight:
					Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.TopRight, Brushes.White, pLagWarningFont, Brushes.DimGray, warning ? Brushes.Maroon:Brushes.Black, 100);break;
				case EntryManager_LagTimeLocation.Center:
					Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.Center, Brushes.White, pLagWarningFont, Brushes.DimGray, warning ? Brushes.Maroon:Brushes.Black, 100);break;
				case EntryManager_LagTimeLocation.BottomLeft:
					Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.BottomLeft, Brushes.White, pLagWarningFont, Brushes.DimGray, warning ? Brushes.Maroon:Brushes.Black, 100);break;
				case EntryManager_LagTimeLocation.BottomRight:
					Draw.TextFixed(this,"LagWarning", lag_msg, TextPosition.BottomRight, Brushes.White, pLagWarningFont, Brushes.DimGray, warning ? Brushes.Maroon:Brushes.Black, 100);break;
			}
			#endregion
		}
line=1843;
		if(pMode == EntryManager_Modes.Disabled) return;

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

		if(!EntryDirection_OnHoverQue.TryPeek(out EntryDirection_OnHover))
			Print("Peek FAILED in OnRender");

		int entry_type = EntryDirection_OnHover == 'B' ? SelectedBuyEntryType : SelectedSellEntryType;
		#region -- Determine Entry price (Market, Limit, Stop) --
		if(CurrentBars[0]>0 && CurrentBars[0]<Bars.Count){
			entryprice = Opens[0].GetValueAt(CurrentBars[0]);

			if(BuySellLabel == "BuyMkt" || (EntryDirection_OnHover == 'B' && entry_type == EntryType_MARKET)){
				entry_type = EntryType_MARKET;
				entryprice = CurrentAsk;
			}else if(BuySellLabel == "SellMkt" || (EntryDirection_OnHover == 'S' && entry_type == EntryType_MARKET)){
				entry_type = EntryType_MARKET;
				entryprice = CurrentBid;
			}else{
				if(entry_type == EntryType_LIMIT || entry_type == EntryType_MKTLIMIT){
					if(EntryDirection_OnHover == 'B') entryprice = entryprice - this.EntryLimitTicksOffset * TickSize;
					if(EntryDirection_OnHover == 'S') entryprice = entryprice + this.EntryLimitTicksOffset * TickSize;
				}else if(entry_type == EntryType_STOP){
					if(EntryDirection_OnHover == 'B') entryprice = entryprice + this.EntryStopTicksOffset * TickSize;
					if(EntryDirection_OnHover == 'S') entryprice = entryprice - this.EntryStopTicksOffset * TickSize;
				}
			}
		}
		#endregion

		if(nearestprice == double.MinValue) {
			RemoveDrawObject("info");
			RemoveDrawObject("u");
			RemoveDrawObject("l");
//			return;
		}

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
				if(pPennantLocs == EntryManager_PennantLocs.Left){
					#region -- left of current bar --
					float y = chartScale.GetYByValue(entryprice);//entry price is CurrentBid or CurrentAsk
					if(EntryDirection_OnHover == 'B'){
						RenderTarget.DrawLine(new SharpDX.Vector2(x,y),new SharpDX.Vector2(x-LineLengthF,y), BuyELineDX);
						if(entry_type == EntryType_LIMIT || entry_type == EntryType_MKTLIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p - EntryLimitTicksOffset*TickSize;
								y = chartScale.GetYByValue(p);
								RenderTarget.DrawLine(new SharpDX.Vector2(x,y),new SharpDX.Vector2(x-LineLengthF,y), BuyELineDX);
							}
						}
						if(this.OnMarketDataFound){
							RenderTarget.DrawLine(new SharpDX.Vector2(x,yBidPrice),new SharpDX.Vector2(x-LineLengthF,yBidPrice), SellELineDX);
						}
	//					if(entry_type == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - BUY entry label
						string labeltxt = SetLabelText(EntryDirection_OnHover, pPositionSize, entry_type);
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
						if(entry_type == EntryType_LIMIT || entry_type == EntryType_MKTLIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p + EntryLimitTicksOffset*TickSize;
								y = chartScale.GetYByValue(p);
								RenderTarget.DrawLine(new SharpDX.Vector2(x,y),new SharpDX.Vector2(x-LineLengthF,y), SellELineDX);
							}
						}
						if(this.OnMarketDataFound){
							RenderTarget.DrawLine(new SharpDX.Vector2(x,yAskPrice),new SharpDX.Vector2(x-LineLengthF,yAskPrice), BuyELineDX);
						}
	//					if(entry_type == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - SELL entry label
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						string labeltxt = SetLabelText(EntryDirection_OnHover, pPositionSize, entry_type);
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
					#region -- right of current bar --
					float y = chartScale.GetYByValue(entryprice);//entryrprice is CurrentBid or CurrentAsk
					float yOfEntryLine = y;
					if(EntryDirection_OnHover =='B'){
	//					if(entry_type == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - BUY entry label
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						string labeltxt = SetLabelText(EntryDirection_OnHover, pPositionSize, entry_type);
						var textLayout  = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, labeltxt, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
						x = Convert.ToSingle(ChartPanel.W) - textLayout.Metrics.Width-10f;
						y = y - textLayout.Metrics.Height/2f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x-10f,y-5f,textLayout.Metrics.Width+10f,textLayout.Metrics.Height+10f),blackDX);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x-5f,y), textLayout, BuyELineDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						textFormat.Dispose(); textFormat=null;
						textLayout.Dispose(); textLayout=null;
						RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yOfEntryLine),new SharpDX.Vector2(x-LineLengthF,yOfEntryLine), BuyELineDX);
						if(entry_type == EntryType_LIMIT || entry_type == EntryType_MKTLIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p - EntryLimitTicksOffset*TickSize;
								yOfEntryLine = chartScale.GetYByValue(p);
								RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yOfEntryLine),new SharpDX.Vector2(x-LineLengthF,yOfEntryLine), BuyELineDX);
							}
						}
						if(this.OnMarketDataFound){
							yBidPrice = chartScale.GetYByValue(CurrentBid);
							RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yBidPrice),new SharpDX.Vector2(x-LineLengthF,yBidPrice), SellELineDX);
						}
						#endregion
					}
					else if(EntryDirection_OnHover=='S'){
	//					if(entry_type == EntryType_MARKET)
	//						y = chartScale.GetYByValue(Closes[0].GetValueAt(CurrentBars[0]));//Text label drawn at the close price
						#region Draw text - SELL entry label
			            var font        = new SimpleFont("Arial",12);
			            var textFormat  = font.ToDirectWriteTextFormat();
						string labeltxt = SetLabelText(EntryDirection_OnHover, pPositionSize, entry_type);
						var textLayout  = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, labeltxt, textFormat, (float)(ChartPanel.X + ChartPanel.W), textFormat.FontSize);
						x = Convert.ToSingle(ChartPanel.W) - textLayout.Metrics.Width-10f;
						y = y - textLayout.Metrics.Height/2f;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(x-10f,y-5f,textLayout.Metrics.Width+10f,textLayout.Metrics.Height+10f),blackDX);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x-5f,y), textLayout, SellELineDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						textFormat.Dispose(); textFormat=null;
						textLayout.Dispose(); textLayout=null;
						RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yOfEntryLine),new SharpDX.Vector2(x-LineLengthF,yOfEntryLine), SellELineDX);
						if(entry_type == EntryType_LIMIT || entry_type == EntryType_MKTLIMIT){
							var p = entryprice;
							for(int i = 2; i<=pEntriesCount; i++){
								p = p + EntryLimitTicksOffset*TickSize;
								yOfEntryLine = chartScale.GetYByValue(p);
								RenderTarget.DrawLine(new SharpDX.Vector2(x-10f,yOfEntryLine),new SharpDX.Vector2(x-LineLengthF,yOfEntryLine), SellELineDX);
							}
						}
						if(this.OnMarketDataFound){
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
line=2318;

	}catch(Exception e){Print(line+" E: "+e.ToString()+Environment.NewLine+"at "+DateTime.Now.ToString()+" "+Instrument.MasterInstrument.Name+Environment.NewLine);}
	}
		//==================================================================================
		//==================================================================================
		private string SetLabelText(char Direction, double Qty, int entry_type){
			if(BuySellLabel == "") return string.Empty;
			string labeltxt = string.Empty;
			string BS = BuySellLabel;
			labeltxt = string.Format("{0} {1}", BS, Qty);
			if(entry_type == EntryType_LIMIT)     labeltxt = string.Format("{0}\nLimit", labeltxt);
			else if(entry_type == EntryType_STOP) labeltxt = string.Format("{0}\nStop", labeltxt);
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
		private EntryManager[] cacheEntryManager;
		public EntryManager EntryManager(int maxContracts, int minContracts, int pEntriesCount, EntryManager_Modes pMode)
		{
			return EntryManager(Input, maxContracts, minContracts, pEntriesCount, pMode);
		}

		public EntryManager EntryManager(ISeries<double> input, int maxContracts, int minContracts, int pEntriesCount, EntryManager_Modes pMode)
		{
			if (cacheEntryManager != null)
				for (int idx = 0; idx < cacheEntryManager.Length; idx++)
					if (cacheEntryManager[idx] != null && cacheEntryManager[idx].MaxContracts == maxContracts && cacheEntryManager[idx].MinContracts == minContracts && cacheEntryManager[idx].pEntriesCount == pEntriesCount && cacheEntryManager[idx].pMode == pMode && cacheEntryManager[idx].EqualsInput(input))
						return cacheEntryManager[idx];
			return CacheIndicator<EntryManager>(new EntryManager(){ MaxContracts = maxContracts, MinContracts = minContracts, pEntriesCount = pEntriesCount, pMode = pMode }, input, ref cacheEntryManager);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EntryManager EntryManager(int maxContracts, int minContracts, int pEntriesCount, EntryManager_Modes pMode)
		{
			return indicator.EntryManager(Input, maxContracts, minContracts, pEntriesCount, pMode);
		}

		public Indicators.EntryManager EntryManager(ISeries<double> input , int maxContracts, int minContracts, int pEntriesCount, EntryManager_Modes pMode)
		{
			return indicator.EntryManager(input, maxContracts, minContracts, pEntriesCount, pMode);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EntryManager EntryManager(int maxContracts, int minContracts, int pEntriesCount, EntryManager_Modes pMode)
		{
			return indicator.EntryManager(Input, maxContracts, minContracts, pEntriesCount, pMode);
		}

		public Indicators.EntryManager EntryManager(ISeries<double> input , int maxContracts, int minContracts, int pEntriesCount, EntryManager_Modes pMode)
		{
			return indicator.EntryManager(input, maxContracts, minContracts, pEntriesCount, pMode);
		}
	}
}

#endregion
