// Coded by Chelsea Bell. chelsea.bell@ninjatrader.com
#region Using declarations
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.Utilities
{
	public class ChartTraderCustomButtonsExample : Indicator
	{
		private System.Windows.Controls.RowDefinition	addedRow1, addedRow2;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid, upperButtonsGrid;
		private System.Windows.Controls.Button[]		buttonsArray;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Demonstrates adding buttons to Chart Trader";
				Name						= "ChartTrader CustomButtonsExample";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				PaintPriceMarkers			= false;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						CreateWPFControls();
					});
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						DisposeWPFControls();
					});
				}
			}
		}

		protected void Button1Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 1 Clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			ForceRefresh();
		}

		protected void Button2Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 2 Clicked", TextPosition.BottomLeft, Brushes.DarkRed, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}

		string entryID = string.Empty;
		protected void Button3Click(object sender, RoutedEventArgs e)
		{
			entryID = "RSI buy "+System.DateTime.Now.Ticks.ToString();
			if (ChartControl.Dispatcher.CheckAccess()) {
				TriggerCustomEvent(o1 =>{
					GoLong_Execution(entryID);
					try{	ForceRefresh();		}catch(System.Exception ex){print("refresh issue: "+ex.ToString());}
				},0,null);
			}else{
				GoLong_Execution(entryID);
				try{	ForceRefresh();		}catch(System.Exception ex){print("refresh issue: "+ex.ToString());}
			}
			Draw.TextFixed(this, "infobox", "Button 3 Clicked", TextPosition.BottomLeft, Brushes.DarkOrange, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}

		protected void Button4Click(object sender, RoutedEventArgs e)
		{
			string atmname = GetAtmStrategyName(0);
			var lines = System.IO.File.ReadAllLines(atmname);
			foreach(var L in lines) Print(L);

			Draw.TextFixed(this, "infobox", "Button 4 Clicked", TextPosition.BottomLeft, Brushes.CadetBlue, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}
		#region -- GoLong and GoShort execution methods
		private string AccountName = "";
		private NinjaTrader.Cbi.Account myAccount = null;
		private string  atmStrategyId			= string.Empty;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;
		private string atmname = "none";

		//=================================================================================
		private void GetCurrentSelectedAccount (){  //myAccount is a global variable
			#region -- GetCurrentSelectedAccount --
			string acctname = "";
			try{
				var xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
				acctname = xAlselector.SelectedAccount.ToString();
			}catch(System.Exception e){}//print("ChartTrader not found: "+e.ToString());}

			if(AccountName.CompareTo(acctname) != 0) {//if the new account name is not the current account name...find the account
				#region Verify account availability
				var accts = Account.All.ToList();
				for(int i = 0; i<accts.Count; i++){
					if(accts[i].Name.ToLower().CompareTo(acctname.Trim().ToLower())==0){
						if(myAccount != null){
//							myAccount.PositionUpdate -= OnPositionUpdate;
							myAccount = null;
						}
						lock (Account.All){
							AccountName  = accts[i].Name;
							myAccount    = accts[i];
//							myAccount.PositionUpdate += OnPositionUpdate;
							break;
						}
					}
				}
				#endregion
			}

			if(myAccount == null) Log("Could not connect to account name: "+acctname+", no trades possible", NinjaTrader.Cbi.LogLevel.Information);
			else print("Trading on account:  "+AccountName);
			#endregion
		}
		//=================================================================================
		private string GetAtmStrategyName(int type){
			if(ChartControl!=null && ChartControl.OwnerChart!=null && ChartControl.OwnerChart.ChartTrader.IsVisible){
				if(ChartControl.OwnerChart.ChartTrader.AtmStrategy!=null){
					Print("Selected strategy template: "+ChartControl.OwnerChart.ChartTrader.AtmStrategy.Template);
					if(type==1){
						return ChartControl.OwnerChart.ChartTrader.AtmStrategy.Template;
					}else{
						var atmdir = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "templates", "AtmStrategy");
						return System.IO.Path.Combine(atmdir, ChartControl.OwnerChart.ChartTrader.AtmStrategy.Template+".xml");
					}
				}
				else {
					Print("no template selected");
				}
			}
			return "none";
		}
		//=================================================================================
		private void print(string s){Print(s);}
		//=================================================================================
		private void GoLong_Execution(string entryID)	{
			if(orderId.Length>0) return;//do not process additional order requests while one is working

			GetCurrentSelectedAccount();
			if(myAccount == null) {print("Account is NULL, no trades here");return;}
			int Contracts = ChartControl.OwnerChart.ChartTrader.Quantity;
			Print("Going LONG: "+Contracts+"-contracts");

			var action = NinjaTrader.Cbi.OrderAction.Buy;//DetermineOrderAction(mp, 'B');
			atmname = GetAtmStrategyName(1);
			orderId = entryID;
			if(atmname == "none"){
				atmStrategyId = string.Empty;
				var OrdersList = new List<Order>();
				OrdersList.Add(myAccount.CreateOrder(
						Instrument.GetInstrument(Instruments[0].FullName), 
						action, 
						OrderType.Market, 
						OrderEntry.Automated, 
						TimeInForce.Day,
						Contracts,
						0, 0, string.Empty, orderId, Core.Globals.MaxDate, null));
				myAccount.Submit(OrdersList.ToArray());
	//}catch(Exception ee){print(line+":  "+ee.ToString());}
			}else{
				atmStrategyId = string.Format("B {0} {1} {2}{3}", Instrument.FullName, atmname, System.DateTime.Now.Minute, System.DateTime.Now.Second.ToString());//strategy id

				string cmd = OIF_PlaceOrder(
					AccountName,
					Instrument.FullName,
					"BUY",
					Contracts,
					"MARKET",
					0,//limit price
					0,//stop price
					"GTC",
					string.Empty,//oco id
					orderId,//orderid
					atmname,//strategy name
					atmStrategyId);//strategy id
				OIF_Submit(cmd, OIFnumber);
			}
		}
		private void GoShort_Execution(string entryID)	{
			if(orderId.Length>0) return;//do not process additional order requests while one is working

			GetCurrentSelectedAccount();
			if(myAccount == null) {print("Account is NULL, no trades here");return;}
			int Contracts = ChartControl.OwnerChart.ChartTrader.Quantity;
			Print("Going SHORT: "+Contracts+"-contracts");

			var action = NinjaTrader.Cbi.OrderAction.SellShort;//DetermineOrderAction(mp, 'B');
			atmname = GetAtmStrategyName(1);
			orderId = entryID;
			if(atmname == "none"){
				atmStrategyId = string.Empty;
				var OrdersList = new List<Order>();
				OrdersList.Add(myAccount.CreateOrder(
						Instrument.GetInstrument(Instruments[0].FullName), 
						action, 
						OrderType.Market, 
						OrderEntry.Automated, 
						TimeInForce.Day,
						Contracts,
						0, 0, string.Empty, orderId, Core.Globals.MaxDate, null));
				myAccount.Submit(OrdersList.ToArray());
	//}catch(Exception ee){print(line+":  "+ee.ToString());}
			}else{
				atmStrategyId = string.Format("S {0} {1} {2}{3}", Instrument.FullName, atmname, System.DateTime.Now.Minute, System.DateTime.Now.Second.ToString());//strategy id

				string cmd = OIF_PlaceOrder(
					AccountName,
					Instrument.FullName,
					"SELL",
					Contracts,
					"MARKET",
					0,//limit price
					0,//stop price
					"GTC",
					string.Empty,//oco id
					entryID,//orderid
					atmname,//strategy name
					atmStrategyId);//AtmStrategy id
				OIF_Submit(cmd, OIFnumber);
			}
		}
		#endregion
//=====================================================================
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
		int OIFnumber = 1;
		string OIF_file_path = "";
		private int OIF_Submit (string[] instruction, int OIFnumber)
		{  //returns the OIF file number
			string full_instruction = string.Empty;
			foreach(var ins in instruction){
				full_instruction = string.Format("{0}{1}",full_instruction, ins);
			}
			return OIF_Submit(full_instruction, OIFnumber);
		}
		private int OIF_Submit (string instruction, int OIFnumber)
		{  //returns the OIF file number
			if(OIFnumber > 500) OIFnumber=0; else OIFnumber++;
			OIF_file_path = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"incoming");
			string fname = System.IO.Path.Combine(OIF_file_path, string.Format("oif{0}.txt", OIFnumber.ToString("0")));
			System.IO.File.AppendAllText(fname, instruction);
			Log("OIF File written: "+fname, LogLevel.Information);
			return OIFnumber;
		}
//=====================================================================
		private string OIF_CancelOrder (string OrderId, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory OrderId was invalid
		// CANCEL;;;;;;;;;;<ORDER ID>;;[STRATEGY ID]
			if(OrderId.Length==0) return string.Empty;
			else
				return "CANCEL;;;;;;;;;;"+OrderId+";;"+StrategyId+System.Environment.NewLine;
		}

		private string OIF_CancelAllOrders ()
		{  //returns the OIF instruction
		// CANCELALLORDERS;;;;;;;;;;;;
			return "CANCELALLORDERS;;;;;;;;;;;;"+System.Environment.NewLine;
		}

		private string OIF_ChangeOrder (int Qty, double LimitPrice, double StopPrice, string OrderId, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
		// CHANGE;;;;<QUANTITY>;;<LIMIT PRICE>;<STOP PRICE>;;;<ORDER ID>;;[STRATEGY ID]
			if(Qty<=0 || LimitPrice < 0 || StopPrice < 0 || OrderId.Length==0) return string.Empty;
			else
				return "CHANGE;;;;"+Qty.ToString()+";;"+LimitPrice.ToString()+";"+StopPrice.ToString()+";;;"+OrderId+";;"+StrategyId+System.Environment.NewLine;
		}

		private string OIF_ChangeOrder (double Qty, double LimitPrice, double StopPrice, string OrderId, string StrategyId)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
		// CHANGE;;;;<QUANTITY>;;<LIMIT PRICE>;<STOP PRICE>;;;<ORDER ID>;;[STRATEGY ID]
			if(Qty<=0 || LimitPrice < 0 || StopPrice < 0 || OrderId.Length==0) return string.Empty;
			else
				return "CHANGE;;;;"+Qty.ToString()+";;"+LimitPrice.ToString()+";"+StopPrice.ToString()+";;;"+OrderId+";;"+StrategyId+System.Environment.NewLine;
		}

		private string OIF_ClosePosition (string Account, string Instrument)
		{  //returns the OIF instruction, or the empty string is the mandatory fields were invalid
		// CLOSEPOSITION;<ACCOUNT>;<INSTRUMENT>;;;;;;;;;;
			if(Account.Length == 0 || Instrument.Length==0) return string.Empty;
			else
				return "CLOSEPOSITION;"+Account+";"+Instrument+";;;;;;;;;;"+System.Environment.NewLine;
		}
		private string OIF_CloseStrategy (string StrategyId)
		{
		// CLOSESTRATEGY;;;;;;;;;;;;<STRATEGY ID>
			return "CLOSESTRATEGY;;;;;;;;;;;;"+StrategyId+System.Environment.NewLine;
		}
		private string OIF_FlattenEverything ()
		{
		// FLATTENEVERYTHING;;;;;;;;;;;;
			return "FLATTENEVERYTHING;;;;;;;;;;;;"+System.Environment.NewLine;
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
				return "PLACE;"+Account+";"+Instrument+";"+Action+";"+Qty.ToString()+";"+OrderType+";"+LimitPrice.ToString()+";"+StopPrice.ToString()+";"+TIF+";"+OCOId+";"+OrderId+";"+Strategy+";"+StrategyId+System.Environment.NewLine;
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
				return "PLACE;"+Account+";"+Instrument+";"+Action+";"+Qty.ToString()+";"+OrderType+";"+LimitPrice.ToString()+";"+StopPrice.ToString()+";"+TIF+";"+OCOId+";"+OrderId+";"+Strategy+";"+StrategyId+System.Environment.NewLine;
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
				return "REVERSEPOSITION;"+Account+";"+Instrument+";"+Action+";"+Qty.ToString()+";"+OrderType+";"+LimitPrice.ToString()+";"+StopPrice.ToString()+";"+TIF+";"+OCOId+";"+OrderId+";"+Strategy+";"+StrategyId+System.Environment.NewLine;
			}
		}
//=====================================================================

		#endregion

		protected void CreateWPFControls()
		{
			chartWindow				= Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;

			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;

			// this is the entire chart trader area grid
			chartTraderGrid			= (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;

			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid	= chartTraderGrid.Children[0] as System.Windows.Controls.Grid;

			// this grid is a grid i'm adding to a new row (at the bottom) in the grid that contains bid and ask prices and order controls (chartTraderButtonsGrid)
			upperButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(upperButtonsGrid, 3);

			upperButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			upperButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength((double)Application.Current.FindResource("MarginBase")) }); // separator column
			upperButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			// this grid is to organize stuff below
			lowerButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid, 4);

			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength((double)Application.Current.FindResource("MarginBase")) });
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			// these rows will be added later, but we can create them now so they only get created once
			addedRow1	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(31) };
			addedRow2	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };

			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;

			// all of the buttons are basically the same so to save lines of code I decided to use a loop over an array
			buttonsArray = new System.Windows.Controls.Button[4];

			for (int i = 0; i < 4; ++i)
			{
				buttonsArray[i]	= new System.Windows.Controls.Button()
				{
					Content			= string.Format("MyButton{0}", i + 1),
					Height			= 30,
					Margin			= new Thickness(0,0,0,0),
					Padding			= new Thickness(0,0,0,0),
					Style			= basicButtonStyle
				};

				// change colors of the buttons if you'd like. i'm going to change the first and fourth.
				if (i % 3 != 0)
				{
					buttonsArray[i].Background	= Brushes.Gray;
					buttonsArray[i].BorderBrush	= Brushes.DimGray;
				}
			}

			buttonsArray[0].Click += Button1Click;
			buttonsArray[1].Click += Button2Click;
			buttonsArray[2].Click += Button3Click;
			buttonsArray[3].Click += Button4Click;

			System.Windows.Controls.Grid.SetColumn(buttonsArray[1], 2);
			// add button3 to the lower grid
			System.Windows.Controls.Grid.SetColumn(buttonsArray[2], 0);
			// add button4 to the lower grid
			System.Windows.Controls.Grid.SetColumn(buttonsArray[3], 2);
			for (int i = 0; i < 2; ++i)
				upperButtonsGrid.Children.Add(buttonsArray[i]);
			for (int i = 2; i < 4; ++i)
				lowerButtonsGrid.Children.Add(buttonsArray[i]);

			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		public void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			if (buttonsArray[0] != null)
				buttonsArray[0].Click -= Button1Click;
			if (buttonsArray[0] != null)
				buttonsArray[1].Click -= Button2Click;
			if (buttonsArray[0] != null)
				buttonsArray[2].Click -= Button3Click;
			if (buttonsArray[0] != null)
				buttonsArray[3].Click -= Button4Click;

			RemoveWPFControls();
		}
		
		public void InsertWPFControls()
		{
			if (panelActive)
				return;

			// add a new row (addedRow1) for upperButtonsGrid to the existing buttons grid
//			chartTraderButtonsGrid.RowDefinitions.Add(addedRow1);
			// set our upper grid to that new panel
//			System.Windows.Controls.Grid.SetRow(upperButtonsGrid, (chartTraderButtonsGrid.RowDefinitions.Count - 1));
			// and add it to the buttons grid
//			chartTraderButtonsGrid.Children.Add(upperButtonsGrid);

			// add a new row (addedRow2) for our lowerButtonsGrid below the ask and bid prices and pnl display			
			chartTraderGrid.RowDefinitions.Add(addedRow2);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid, (chartTraderGrid.RowDefinitions.Count - 1));
			chartTraderGrid.Children.Add(lowerButtonsGrid);

			panelActive = true;
		}

		protected override void OnBarUpdate() { }

		protected void RemoveWPFControls()
		{
			if (!panelActive)
				return;

			if (chartTraderButtonsGrid != null || upperButtonsGrid != null)
			{
				chartTraderButtonsGrid.Children.Remove(upperButtonsGrid);
				chartTraderButtonsGrid.RowDefinitions.Remove(addedRow1);
			}
			
			if (chartTraderButtonsGrid != null || lowerButtonsGrid != null)
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				chartTraderGrid.RowDefinitions.Remove(addedRow2);
			}

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Utilities.ChartTraderCustomButtonsExample[] cacheChartTraderCustomButtonsExample;
		public Utilities.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample()
		{
			return ChartTraderCustomButtonsExample(Input);
		}

		public Utilities.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample(ISeries<double> input)
		{
			if (cacheChartTraderCustomButtonsExample != null)
				for (int idx = 0; idx < cacheChartTraderCustomButtonsExample.Length; idx++)
					if (cacheChartTraderCustomButtonsExample[idx] != null &&  cacheChartTraderCustomButtonsExample[idx].EqualsInput(input))
						return cacheChartTraderCustomButtonsExample[idx];
			return CacheIndicator<Utilities.ChartTraderCustomButtonsExample>(new Utilities.ChartTraderCustomButtonsExample(), input, ref cacheChartTraderCustomButtonsExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Utilities.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample()
		{
			return indicator.ChartTraderCustomButtonsExample(Input);
		}

		public Indicators.Utilities.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample(ISeries<double> input )
		{
			return indicator.ChartTraderCustomButtonsExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Utilities.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample()
		{
			return indicator.ChartTraderCustomButtonsExample(Input);
		}

		public Indicators.Utilities.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample(ISeries<double> input )
		{
			return indicator.ChartTraderCustomButtonsExample(input);
		}
	}
}

#endregion
