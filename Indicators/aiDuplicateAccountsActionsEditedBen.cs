// 
// Copyright (C) 2021, Affordable Indicators, Inc. <www.affordableindicators.com>.
// Affordable Indicators, Inc. reserves the right to modify or overwrite this NinjaScript component with each release.
//


// 8.0.28.0 OrderState.Suspended dpesmnt exists and atm.DisplayNameExtended doesnt exist

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
//using System.Threading.Tasks;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools; // SimpleFont
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using SharpDX.DirectWrite;
using System.Text.RegularExpressions;
using System.IO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml.Serialization;
using NinjaTrader.Data;

//This namespace holds Indicators in this folder and is required. do NOT change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	[Gui.CategoryOrder("Main Features", -30)]  
	[Gui.CategoryOrder("Account Management", -15)]
	[Gui.CategoryOrder("Account Name Adjustments", -14)]
	[Gui.CategoryOrder("Account Details Privacy", -13)]
	[Gui.CategoryOrder("Functionality", -12)]
	[Gui.CategoryOrder("Columns", -11)]
	[Gui.CategoryOrder("Duplicate Account Actions", -10)]
	[Gui.CategoryOrder("Account Risk Manager", -9)]
	[Gui.CategoryOrder("Exit Shield", -8)]
	[Gui.CategoryOrder("One Trade", -7)]
	[Gui.CategoryOrder("Smart Synchronize", -6)]
	[Gui.CategoryOrder("Rejected Order Handling", -5)]
	[Gui.CategoryOrder("Actions Column (Buttons)", -4)]
	[Gui.CategoryOrder("Total Rows", -2)]
	[Gui.CategoryOrder("Window Display", 10)]
	[Gui.CategoryOrder("Control Buttons" , 11)] 
	[Gui.CategoryOrder("Flatten Everything Button" , 12)] 
	[Gui.CategoryOrder("Refresh Positions Button" , 13)] 
	[Gui.CategoryOrder("Reset Button" , 14)] 
	[Gui.CategoryOrder("Status Messages" , 16)] 
	[Gui.CategoryOrder("Safety" , 17)] 
	[Gui.CategoryOrder("Setup", 18)]
	[Gui.CategoryOrder("Data Series", 20)]
	[Gui.CategoryOrder("Data", 21)]
	[Gui.CategoryOrder("TradingView", 22)]
	[Gui.CategoryOrder("Keyboard", 30)]
	[Gui.CategoryOrder("Advanced", 31)]
	[Gui.CategoryOrder("Orders Mode", 32)]
	[Gui.CategoryOrder("All Prop Firms", 34)]
	[Gui.CategoryOrder("Language", 36)]
	[Gui.CategoryOrder("License", 38)]
	
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.aiDuplicateAccountActionBensConverter")]
	public class aiDuplicateAccountsActionsBen : Indicator
	{

		private string ThisName = "aiDuplicateAccountsActions Ben Edited";
		
		private string APEXURL = "https://apextraderfunding.com/member/aff/go/jrwyse?c=DVUDKBFF&keyword=ninjatrader";
	
		private string APEXButton = "APEX";
		
		
		private string MarketOrderName = "AI";
		
		private string OneTradeOrderName = "One Tr";
		private string RefreshOrderName = "Ref Pos";
		
		string TheWarningMessage = "";

		private bool pShowLossOnLeft = true;
		
		private bool pSplitMiniAndMicro = false;
		
		private string pTVOCOPrefix = "TV";
		
		
	//	\u2713 check mark
		
		private string lockkkk = "\u1F512";
		private string topvisible = "\u21A7";
		private string bottomvisible = "\u21A5";
		private string inmiddle = "\u21C5";

		private string pHideAString = "\u019F";
		private string pHideCString = "$";
		
		
		
		private bool pCombineLongShort = true;
		private string LongColumnName = "Long";
				
		private bool pAllowDisconnectedAccounts = true;
		
		private bool ForceAllAccountsToBeListed = false;
	
		
		private SimpleFont FinalFont1;
		
		private DateTime RemoveMessageTime = DateTime.MinValue;
		
//		private DateTime LastSessionStartTime = DateTime.MinValue;
				
//		private DateTime LastTargetOrStopMasterFilled = DateTime.MinValue;
		
		
		[XmlIgnore] public NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector atmStrategySelector;	
		
		string AllKorean = "마스터 계정을 설정하고 카피기능을 활성화하려면 최소한 하나의 서브 계정을 추가해야 합니다. 먼저 초록색 동그라미 연결 상태를 더블 클릭하여 마스터 계정을 설정하십시오. 계좌 이름을 클릭하여 팔로워 계정으로 설정하십시오.";
		
		string MainResetConfirmation = "";
		
		
		private SortedDictionary<int, AtmStrategy> DefaultAtmStrategyNumber = new SortedDictionary<int, AtmStrategy>();
		private SortedDictionary<string, AtmStrategy> DefaultAtmStrategy = new SortedDictionary<string, AtmStrategy>();
//		private SortedDictionary<string, SortedList<int, double>> DefaultAtmStrategyPercentages = new SortedDictionary<string, SortedList<int, double>>();
		

		private bool Permission = false;
		private bool UserHasStrategy = false;
	
		
		private string LicensingMessage = string.Empty;
		

//		private bool FirstLoad = true;
		private int pMessageSeconds = 6;
		
		private double dpiX = 0;
		private	double dpiY = 0;
		
		
		private double FinalXPixel = 0;
		private	double FinalYPixel = 0;		
			
			private string audiosym = "\uD83D\uDD0A ";
					
			private string triup = "\u25B2";
			private string tridn = "\u25BC";
		
			float fasttextop = 0.20f;
			float fasttextop2 = 0.40f;
		
		
	
			
			private string DirectionAll = "All Trades";
			private string DirectionLong = "Long Only";
			private string DirectionShort = "Short Only";
		
		
//		Stroke ThisStroke = new Stroke(Brushes.DarkGreen, DashStyleHelper.Solid, 2);
		private SharpDX.RectangleF ThisRect = new SharpDX.RectangleF(0, 0, 0, 0);
		
		private SharpDX.RectangleF ClickRect = new SharpDX.RectangleF(0, 0, 0, 0);
		
		private List<Order> AllMasterAccountOrders = new List<Order>();
		
		private List<double> AllTradingViewTargetPrices = new List<double>();
		private List<double> AllTradingViewStopPrices = new List<double>();
		
		private List<Order> NewOrdersToSend = new List<Order>();
		private List<AtmStrategy> NewATMToSend = new List<AtmStrategy>();
		
		
		private List<string> RemoveATMCheck = new List<string>();
		
		private List<string> AllDuplicateAccounts = new List<string>();
		private List<string> AllCrossAccounts = new List<string>();
		private List<string> AllHideAccounts = new List<string>();
		
		
		private string SelectedATMAccount = string.Empty;
		private DateTime SelectedATMTime = DateTime.MinValue;
		
		private string SelectedMultiplierAccount = string.Empty;
		private DateTime SelectedMultiplerTime = DateTime.MinValue;
		
		private string SelectedDailyGoalAccount = string.Empty;
		private DateTime SelectedDailyGoalTime = DateTime.MinValue;		
		
		private string SelectedDailyLossAccount = string.Empty;
		private DateTime SelectedDailyLossTime = DateTime.MinValue;		
		
		private string SelectedPayoutAccount = string.Empty;
		private DateTime SelectedPayoutTime = DateTime.MinValue;		
		
		private List<string> ResubmittedOrders = new List<string>();		
		
		private string SelectedResetColumn = string.Empty;
		private DateTime SelectedResetTime = DateTime.MinValue;
		
		
		private string SelectedButtonNow = string.Empty;
		private DateTime SelectedButtonTime = DateTime.MinValue;
		
		
		private string LastAutoCloseReset = "Yes";
		private string LastAutoExitReset = "Yes";
										
		private DateTime LastSessionStart = DateTime.MaxValue;	

//			private Series<double> BarRangeD;
//			private Series<double> LowerWickD;
//			private Series<double> UpperWickD;
//			private Series<int> Direction;
		
			private bool RunFirst = true;
			private string FS = string.Empty;
		
			private List<List<string>> AllColumns = new List<List<string>>();
			private List<List<int>> AllColors = new List<List<int>>();
		
		private double CurrentLastData = 0;
		
		private string offtrendtext = "-"; //"Trend Is Off";
		
		private double ThisCount = 0;
		
		private bool LastLoop = false;
		
		private bool CloseIn = false;
		
		private int LastLevelBar = 0;
		
//		private int LB = 0;
//		private int rtcount = 0;
		
		private string PriceString = string.Empty;
		private int PriceDigits = 0;

//		private double HolidayIgnore = 0;
//		private double HolidayIgnore2 = 0;
		
		private bool AllowedToPlot = false;
		
//		SortedDictionary<string, int> MasterOrderNameToQty = new SortedDictionary<string, int>();
		
//		SortedDictionary<string, string> TradingViewExitOrders = new SortedDictionary<string, string>();
		
		
//		SortedDictionary<string, double> MoveBackStopLoss = new SortedDictionary<string, double>();
//		SortedDictionary<string, double> MoveBackTarget = new SortedDictionary<string, double>();
		
//		SortedDictionary<int, double> HighP = new SortedDictionary<int, double>();
//		SortedDictionary<int, double> LowP = new SortedDictionary<int, double>();
		

//		SortedDictionary<int, double> HighLA = new SortedDictionary<int, double>();
//		SortedDictionary<int, double> LowLA = new SortedDictionary<int, double>();
		
//		SortedDictionary<double, int> AllLines = new SortedDictionary<double, int>();
		
//		SortedDictionary<int, double> DeleteLA = new SortedDictionary<int, double>();
		
//		SortedDictionary<int, double> SwapLA = new SortedDictionary<int, double>();
		
//		SortedDictionary<int, int> HighLF = new SortedDictionary<int, int>();
//		SortedDictionary<int, int> LowLF = new SortedDictionary<int, int>();			

	
//        SortedDictionary<int, BoxC> AllBoxes = new SortedDictionary<int, BoxC>();
//		SortedDictionary<int, BoxC> AllBoxes2 = new SortedDictionary<int, BoxC>();
		
		
		private bool pEnabledTradingViewFix = true;
		
//		List<Entry2> entries = new List<Entry2>();

		private struct Entry2 {   //LIST
			public string Name;
			public string Email;
			public string MachineID;
			public string UniqueID;
			public string Module;
			public DateTime StartDate;
			public DateTime ExpireDate;
			public string Comment1;
			public string Comment2;
			public string Comment3;
			
			public Entry2(string name, string email, string machineid, string uniqueid, string module, DateTime startdate, DateTime expiredate, string comment1, string comment2, string comment3) {this.Name = name; this.Email = email; this.MachineID = machineid; this.UniqueID = uniqueid; this.Module = module; this.StartDate = startdate; this.ExpireDate = expiredate; this.Comment1 = comment1; this.Comment2 = comment2; this.Comment3 = comment3;}
		}
	
		
		string sizecolname = "Size";
		string modecoln = "Mode";
		
		private string CellString = string.Empty;
		private SharpDX.DirectWrite.TextLayout CellLayout;
		private SharpDX.DirectWrite.TextLayout CellLayoutMessages;
		private SharpDX.DirectWrite.TextLayout CellLayoutCalc;
		
        private SharpDX.DirectWrite.TextFormat CellFormat;		
		
//		private SharpDX.Direct2D1.Brush ChartBackgroundFadeBrushDX = null;
	
		private SortedDictionary<string, MasterAccountATM> FilledMasterAccountATMs = new SortedDictionary<string, MasterAccountATM>();		
				
		private List<MasterAccountATM> FilledMasterAccountATM = new List<MasterAccountATM>(); 
        public class MasterAccountATM
        {
   
            AtmStrategy iATMS;
            double iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRect;
            bool iHovered;
			Brush iBrushE;
			DateTime iStartTime;
			DateTime iExpireTime;


		
            public AtmStrategy ATMS { get { return iATMS; } set { iATMS = value; } }
            public double Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }
			public Brush BrushE { get { return iBrushE; } set { iBrushE = value; } }
			
            public DateTime StartTime { get { return iStartTime; } set { iStartTime = value; } }
			public DateTime ExpireTime { get { return iExpireTime; } set { iExpireTime = value; } }
			
			

        }
		
		
		private List<MessageAlerts> AllMessages = new List<MessageAlerts>(); 
        public class MessageAlerts
        {
   
            string iName;
            double iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRect;
            bool iHovered;
			int iBrushE;
			DateTime iStartTime;
			DateTime iExpireTime;

		
            public string Name { get { return iName; } set { iName = value; } }
            public double Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }
			public int BrushE { get { return iBrushE; } set { iBrushE = value; } }
            public DateTime StartTime { get { return iStartTime; } set { iStartTime = value; } }
			public DateTime ExpireTime { get { return iExpireTime; } set { iExpireTime = value; } }
			
			

        }
		

		
		private bool IsPendingEntryOrder(Order or)
		{
			
			
			bool istvtarget = or.Oco.Contains(pTVOCOPrefix);
	
			if (!or.Name.Contains("Target") && !or.Name.Contains("Stop") && !istvtarget)
			{
				return true;
			}
			else if (or.Account.GetPosition(or.Instrument.Id) == null)
			{
				return true;
			}
			else
			{
				return false;
			}
			
			
			
		}
			
		private void AddMessage (string msg, int sec, int be)
		{
			
			
			RemoveMessage("have been successfully");
			
			
			if (!pShowStatusMessages)
					return;
					
					
				MessageAlerts Z = new MessageAlerts();

						
							Z = new MessageAlerts();
				            Z.Name = msg;
							Z.StartTime = DateTime.Now;
							Z.ExpireTime = DateTime.Now.AddSeconds(sec);
							Z.BrushE = be;
							Z.Switch = true;
		
			
			//Print("Addmess 1");
			
				bool DuplicateMessage = false;

			
				foreach (MessageAlerts D in AllMessages)
				{
					
					if (D.Name == msg && D.Switch)
					{
						DuplicateMessage = true;
						Z.StartTime = DateTime.Now;
						Z.ExpireTime = DateTime.Now.AddSeconds(sec);
					}
						
				}
				
				//Print("Addmess 2");
				
				
				if (!DuplicateMessage)
				{
					AllMessages.Add(Z);
				}
				else
				{
					
						
				}
				//Print("Addmess 3");					

				bool refresh = true;
				
				
				
				if (msg.Contains("Daily Goal") || msg.Contains("Daily Loss") || msg.Contains("Funded Goal") || msg.Contains("was moved back") || msg.Contains("Rejected") || msg.Contains("rejection") || msg.Contains("are both disabled") || msg.Contains("all orders have been cancelled") || msg.Contains("will be reset in a moment"))   
					refresh = false;
				
				 if (msg.Contains("You can't update the '"))
					refresh = false;
				
										
									
				
				
				//Print("Addmess 4");
				
				if (refresh)
				if (ChartControl != null)
					ChartControl.InvalidateVisual();
				
				//Print("Addmess 5");
				
		}

		private void RemoveMessage (string msg)
		{
				 
				
			
				foreach (MessageAlerts D in AllMessages)
				{
					
					if (D.Name.Contains(msg))
						D.Switch = false;
						
				}
				
				
			//ChartControl.InvalidateVisual();
			
		}

		private List<string> SpeakAlerts = new List<string>();
		private List<string> SoundAlerts = new List<string>();
		
	
			
		private List<string> AllErrorMessages = new List<string>();
		
//		private SharpDX.Direct2D1.Brush StatusButtonsBrushDX = null;

		private SortedDictionary<int, int> ProductIDToMachineIDs = new SortedDictionary<int, int>();
		private ConcurrentDictionary<int, List<string>> ProductIDToInstruments = new ConcurrentDictionary<int, List<string>>();
			
//	Save all settings into list of strings
//Account name
//Account status (slave / master / none)
//Account X
//Account Crossover

		
		private string div = "|";
		
	
		private void AutoCloseNoAllAccounts()
		{
	
			//Print("AutoCloseNoAllAccounts  ");
			
		
			foreach(string s in pAllAccountData)
			{

				if (s!= string.Empty)
				{
					string accountname = s.Split('|')[0];
					
					SetAccountData("",accountname,"","","","","","","No","");
				}	
							
				
				
			}
	
			
		}
		
		
		
		
		private void UnhideAllAccounts()
		{
	
			
			AllHideAccounts.Clear();
			
	
		
			foreach(string s in pAllAccountData)
			{

				if (s!= string.Empty)
				{
					string accountname = s.Split('|')[0];
					
					SetAccountData("1",accountname,"","","","","No","","","");
				}	
							
				
				
			}
	
			
		}
		
		
		private void RemoveAccountData(string one)
		{
		
			
			
			AllDuplicateAccounts.Remove(one);
			
			// Print("REMOVE 1");
			
			AllCrossAccounts.Remove(one);
			
			// Print("REMOVE 2");
			
			AllHideAccounts.Remove(one);
			
			
		
			int founds = 0;
			int finals = 0;
			
			// Print("REMOVE 3");
			
			foreach(string s in pAllAccountData)
			{

				if (s.Contains(one))
				{
					
					
					finals = founds;
					break;
				}	
							
				founds++;
				
			}
			

			// Print("REMOVE 4");
			
			
			if (finals != 0)
			{
				
				//Print(pAllAccountData[finals]);
				
				
				pAllAccountData[finals] = "";
			
			
				
				
			}
			
			// Print("REMOVE 5");
			
			SortAccountDataInput(pAllAccountData);
			
			
			// Print("REMOVE 6");
			
			RemoveAccountSavedData(one, pAllAccountAutoLiquidate);
			RemoveAccountSavedData(one, pAllAccountDailyGoal);
			RemoveAccountSavedData(one, pAllAccountDailyLoss);
			RemoveAccountSavedData(one, pAllAccountPayouts);
			
			// Print("REMOVE 7");
			
			RemoveAccountSavedData(one, pAllAccountCashValue);
			
			// Print("REMOVE 8");
			
			RemoveAccountSavedData(one, pAllAccountNetLiquidation);
			
			// Print("REMOVE 9");
			
		}
		
		private void ResetAccountData(string one)
		{
	
			
			AllDuplicateAccounts.Clear();
			AllCrossAccounts.Clear();
			
			//AllHideAccounts.Clear();
			
			
			DisableMasterAccount();
			
			SelectedMultiplierAccount = string.Empty;
			SelectedATMAccount = string.Empty;

				
			foreach(string s in pAllAccountData)
			{

				if (s!= string.Empty)
				{
					string accountname = s.Split('|')[0];
					
					SetAccountData("2",accountname,"None","1","No","No","No","No","No","Default");
					
//					 	if (SelectedResetColumn == "Daily Goal")
//									{
//										foreach(string s in pAllAccountData)
//										if (s!= string.Empty)
//										{
//											string accountname = s.Split('|')[0];
//											RemoveAccountSavedData(accountname, pAllAccountDailyGoal);
//										}	
										
//									}	
					
					if (pResetDGDL)
					{
					
						RemoveAccountSavedData(accountname, pAllAccountDailyGoal);
						RemoveAccountSavedData(accountname, pAllAccountDailyLoss);	
						RemoveAccountSavedData(accountname, pAllAccountPayouts);	
					}
					
					
					
				}	
							
				
		
			}
			
			
			
			SortAccountDataInput(pAllAccountData);
			
			

			
			
	
			
		}
		
		// SetAccountData("","","None","1","No","No");
		
		
		
		
		int startslot = 10;
		
	
		
		
		private AtmStrategy TestATM = null;
		
		
		
		private void SortAccountDataInput (string[] aaaaaaaa)
		{
			
			//Print("SortAccountDataInput");
			
		
			List<string> AllAccounts = new List<string>();
			
			int slot = startslot;
			
			//Print("aaaaaaaa.Length" + "  " + aaaaaaaa.Length);
			
			
				for (int i=slot; i < aaaaaaaa.Length; i++)
					{
					
						if (aaaaaaaa[i] != string.Empty)
						if (!AllAccounts.Contains(aaaaaaaa[i]))
							AllAccounts.Add(aaaaaaaa[i]);
						
						
						aaaaaaaa[i] = "";
									
						
					}	
					
					
					
					
				slot = startslot;	
				
					
					AllAccounts = AllAccounts.OrderBy(q => q).ToList();
					
					
					foreach (string sssssss in AllAccounts)
					{
						
						
						
					
						
								aaaaaaaa[slot] = sssssss;
							
							
							slot++;			
								
						
					}	
						
					
					
					
					
		}
		
		private string GetAccountMode (string name)
		{
			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initialize = true;
			
			foreach(string s in pAllAccountData)
			{
				
					if (s!= string.Empty)
					{
					
						
						
						if (s.Split('|')[0] == name)
						{
							
//							Print("s: " + s);
//							Print("split: " + s.Split('|')[2]);
							if (s.Split('|').Length > 8)
								return s.Split('|')[8];
							else
								return "Default";
						}
						
					}

				
				
			}
			
			
			return "Default";
		
		}
			
				
	
		private string GetAccountMultiplier (string name)
		{
			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initializeoneaccount = true;
			
			
			foreach(string s in pAllAccountData)
			{
				
					if (s!= string.Empty)
					{
					
						
						
						if (s.Split('|')[0] == name)
						{
							initializeoneaccount = false;
							
//							Print("s: " + s);
//							Print("split: " + s.Split('|')[2]);
							
							return s.Split('|')[2];
						}
						
					}
					
				
				
			}
			
			
			if (initializeoneaccount)
			{
				
				int slot = startslot;
				
				//Print(pAllAccountData.Length);
				
				for (int i=slot; i < pAllAccountData.Length; i++)
				{
					if (pAllAccountData[i] == string.Empty)
					{
						pAllAccountData[i] = name + div + "None" + div + "1" + div + "No" + div + "No" + div + "No" + div + "No" + div + "No" + div + "Default";
						return "1";
					}
								
					
				}	
			
				
				
				SortAccountDataInput(pAllAccountData);
			
				
			}
			
			
			return "0";
		
		}

		private void SetAccountData (string id, string name, string status, string multiplier, string crossover, string fade, string hide, string peak, string autoclose, string atm)
		{
			
			//Print("id  " + id);
			
			
			int foundspot = -1;
			//int foundspot2 = 0;
			
			
			//bool initialize = true;
			
			foreach(string s in pAllAccountData)
			{
				
				foundspot++;

				if (s == string.Empty)
					continue;
				
				// initialize slave and cross accounts when indicator is loaded
				if (name == string.Empty)
				{
					
					
					//if (s!= string.Empty)
				//	{
					
					
						string accountname = s.Split('|')[0];
						
						
						
						
						
						if (s.Contains("Slave"))
						{
							//Print(s);
							//Print(s.Split('|')[0]);
							if (!AllDuplicateAccounts.Contains(accountname))
								AllDuplicateAccounts.Add(accountname);
						}
						
						//if (s.Contains("Yes"))
						if (s.Split('|')[3] == "Yes")
						{
							//Print(s);
							//Print(s.Split('|')[0]);
							if (!AllCrossAccounts.Contains(accountname))
								AllCrossAccounts.Add(accountname);
						}	
						
						if (s.Split('|').Length > 5)
						if (s.Split('|')[5] == "Yes")
						{
							if (!AllHideAccounts.Contains(accountname))
								AllHideAccounts.Add(accountname);							
							
						}
						
//						if (s.Split('|').Length > 8)
//						if (!AllATMStrategies.contrains.Split('|')[8]))
//						{
//							if (!AllHideAccounts.Contains(accountname))
//								AllHideAccounts.Add(accountname);							
							
//						}						
						
						
						
						
						
				//	}	
					
					
					
					
					
					
					
					
				}
				
				 
				// detect the account data we aneed to update
				
				//Print(name + "  " + foundspot);
				
				if (name != string.Empty)
				if (s.Contains(name) || name == "ALL")
				{
					
					//Print("name  " + name);
					
					
						
						if (name != string.Empty) // account is added and found in list
						{
							
							//Print(foundspot);
							
							string oldstring = pAllAccountData[foundspot];
							
							
							string oldname = string.Empty;
							string oldstatus = string.Empty;
							string oldmultiplier = string.Empty;
							string oldcrossover = string.Empty;
							string oldfade = string.Empty;
							string oldhide = string.Empty;
							string oldpeak = string.Empty;
							string oldautoclose = string.Empty;
							string oldatm = string.Empty;
							
							string [] split = oldstring.Split('|');

								int splitCounter = 0;
								foreach (string ss in split)
								{
									splitCounter++;
									switch(splitCounter)       
									{         
										case 1:
											oldname = ss;
											break;
										case 2:            
											oldstatus = ss;								
											break;
										case 3:            
											oldmultiplier = ss;	
											break;
										case 4:   
											oldcrossover = ss;	
											break;
										case 5:   
											oldfade = ss;	
											break;
										case 6:   
											oldhide = ss;	
											break;
										case 7:   
											oldpeak = ss;	
											break;											
										case 8:   
											oldautoclose = ss;	
											break;												
										case 9:   
											oldatm = ss;	
											break;												
											
											
											
										default:								          
											break;      
									}

								}
							
							if (oldhide == string.Empty)
								oldhide = "No";
							
							if (oldpeak == string.Empty)
								oldpeak = "No";	
							
							if (oldpeak == "0") // resolves a bug in older versions
								oldpeak = "No";	// this is the functionality that closes trades
							
							if (oldautoclose == string.Empty)
								oldautoclose = "No";								
							
							if (oldatm == string.Empty)
								oldatm = "Default";	
							
							//Print("oldpeak  " + oldpeak);
							
							
							
							
							string newstatus = oldstatus;
							string newmultiplier = oldmultiplier;
							string newcrossover = oldcrossover;
							string newfade = oldfade;	
							string newhide = oldhide;	
							string newpeak = oldpeak;	
							string newautoclose = oldautoclose;
							string newatm = oldatm;	
							
							//Print("newpeak  " + newpeak);	
									
							if (status != string.Empty)
								newstatus = status;
							
							if (multiplier != string.Empty)
								newmultiplier = multiplier;	
							
							if (crossover != string.Empty)
								newcrossover = crossover;
							
							if (fade != string.Empty)
								newfade = fade;		
							
							if (hide != string.Empty)
								newhide = hide;	
							
							if (peak != string.Empty)
								newpeak = peak;				
							
							if (autoclose != string.Empty)
								newautoclose = autoclose;	
							
							if (atm != string.Empty)
								newatm = atm;								
							
							
							
							// set account name for when adjusting a setting across all accounts
							
							string finalname = name;
							if (name == "ALL")
								finalname = oldname;
							
							pAllAccountData[foundspot] = finalname + div + newstatus + div + newmultiplier + div + newcrossover + div + newfade + div + newhide + div + newpeak + div + newautoclose + div + newatm;
							
							
							
							
							//Print(oldstatus + " " + oldmultiplier + " " + oldcrossover);
									
							//Print(pAllAccountData[foundspot]);
							
					
							
						}
								
								
								
					
				}
				
				//foundspot = foundspot + 1;
				
				//Print(foundspot);
				
				
				//if (s != string.Empty)
				//	initialize = false;
				
				
				
			}
		
			

			
			
			
			//Print("foundspot2: " + foundspot2);
			
			
		}
			
		private string GetAccountFade (string name)
		{
			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initialize = true;
			
			foreach(string s in pAllAccountData)
			{
				
					if (s!= string.Empty)
					{
					
						
						
						if (s.Split('|')[0] == name)
						{
							
//							Print("s: " + s);
//							Print("split: " + s.Split('|')[2]);
							
							return s.Split('|')[4];
						}
						
					}

				
				
			}
			
			
			return "No";
		
		}

		private void CheckForMissingATM ()
		{
			
		
			//bool initialize = false;
			
			foreach(string s in pAllAccountData)
			{
				
				
										
						if (s.Split('|').Length > 8)
						{
							string modesdd = s.Split('|')[8];
							
							if (!AllATMStrategies.Contains(modesdd) && modesdd != "Executions" && modesdd != "Orders" && modesdd != "Default")
							{							
								SetAccountData("3",s.Split('|')[0],"","","","","","","","Default");
								
							}
							
							if (modesdd == "Executions" || modesdd == "Orders")
							if (pMultiplierMode == "Quantity")
							{							
								SetAccountData("3",s.Split('|')[0],"","","","","","","","Default");
								
							}							
							
						}						
						
					
				
					
			}
			
			
			
			
		
		}

		private string GetSavedAutoClose (string name)
		{
			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initialize = false;
			
			foreach(string s in pAllAccountData)
			{
				
					if (s!= string.Empty)
					{
					
						
						
						if (s.Split('|')[0] == name)
						{
							
//							Print("s: " + s);
//							Print("split: " + s.Split('|')[2]);
							
							
							//Print(s.Split('|').Length);
							
							
							if (s.Split('|').Length <= 7)
							{
								//initialize = true;
								
							}
							else
							{
								string numberrrrr = s.Split('|')[7];
								return numberrrrr;
							}
							
								
							
							
							
							
						}
						
					}

				
				
			}
			

			
			return string.Empty;
		
		}

		private string GetSavedAccountExitSwitch (string name)
		{
			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initialize = false;
			
			foreach(string s in pAllAccountData)
			{
				
					if (s!= string.Empty)
					{
					
						
						
						if (s.Split('|')[0] == name)
						{
							
//							Print("s: " + s);
//							Print("split: " + s.Split('|')[2]);
							
							
							//Print(s.Split('|').Length);
							
							
							if (s.Split('|').Length <= 7)
							{
								//initialize = true;
								
							}
							else
							{
								
								return s.Split('|')[6];
							}
							
								
							
							
							
							
						}
						
					}

				
				
			}
			

			
			return string.Empty;
		
		}

		private void RemoveAccountSavedData (string name, string[] aaaaaaaa)
		{
		
			
			

		
			int founds = 0;
			int finals = 0;
			
			foreach(string s in aaaaaaaa)
			{

				if (s.Contains(name))
				{
					
					
					finals = founds;
					break;
				}	
							
				founds++;
				
			}
			

			//Print("finals " + finals);
			
			
			
			
			if (finals != 0)
			{
				
				
				//Print(aaaaaaaa[finals]);
				
				
				aaaaaaaa[finals] = "";
			
			
				
				
			}
			
			SortAccountDataInput(aaaaaaaa);
	
		}

		private void SetAccountSavedData (string name, string[] aaaaaaaa, string status)
		{
			
		
			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initialize = true;
			
			foreach(string s in aaaaaaaa)
			{

				// initialize slave and cross accounts when indicator is loaded
				
							
				
				// detect the account data we aneed to update
				
				
				if (s.Contains(name))
				{
					
					//Print("name  " + name);
					
					
						
						if (name != string.Empty) // account is added and found in list
						{
							
							//Print(foundspot);
							
							string oldstring = aaaaaaaa[foundspot];
							
							string oldstatus = string.Empty;
			
							string [] split = oldstring.Split('|');

								int splitCounter = 0;
								foreach (string ss in split)
								{
									splitCounter++;
									switch(splitCounter)       
									{         
										case 1:
											break;
										case 2:            
											oldstatus = ss;								
											break;
										
											
											
										default:								          
											break;      
									}

								}
							
					
							if (oldstatus == string.Empty)
								oldstatus = "0";							
								
						
							
							string newstatus = oldstatus;
					
									
							if (status != string.Empty)
								newstatus = status;
							
							
							
							aaaaaaaa[foundspot] = name + div + newstatus;
							
						
							
							
						}
								
								
								
								
					
					
					
					
					
					
					
					//foundspot2 = foundspot;
					
				}
				
				foundspot = foundspot + 1;
				
			
				
			}
		
			
				
				if (initialize)
				{
					
					int slot = startslot;
					
					//Print(pAllAccountData.Length);
					
					for (int i=slot; i < aaaaaaaa.Length; i++)
					{
						if (aaaaaaaa[i] == string.Empty)
						{
							aaaaaaaa[i] = name + div + status;
						
							break;
						}
						
						
									
						
					}	
					
					
					
					SortAccountDataInput(aaaaaaaa);
				
					
				}
				
				
				
				
				
			

			
		}

		private bool IsDailyGoalOrLossConfigured(string name)
		{
			
						double CurrentDailyGoal = GetAccountSavedData(name, pAllAccountDailyGoal);
						double CurrentDailyLoss = GetAccountSavedData(name, pAllAccountDailyLoss);			
			
			
			if (CurrentDailyGoal == -1000000000 && CurrentDailyLoss == -1000000000)
				return false;
			
			
			return true;
		}

		private double GetAccountSavedData (string name, string[] aaaaaaaa)
		{
			

			
			int foundspot = 0;
			int foundspot2 = 0;
			bool initialize = true;
			
			foreach(string s in aaaaaaaa)
			{

				// initialize slave and cross accounts when indicator is loaded
				
				
				
					
						if (s!= string.Empty)
						{
						
							
							
							if (s.Split('|')[0] == name)
							{
								initialize = false;
								
	//							Print("s: " + s);
	//							Print("split: " + s.Split('|')[2]);
								
								string numberrrrr =  s.Split('|')[1];
								
								return Convert.ToDouble(numberrrrr);
								 
							}
							
						}
						
			}
				
					
//				if (initialize)
//				{
					
//					int slot = startslot;
					
//					//Print(pAllAccountData.Length);
					
//					for (int i=slot; i < aaaaaaaa.Length; i++)
//					{
//						if (aaaaaaaa[i] == string.Empty)
//						{
//							aaaaaaaa[i] = name + div + "0";
						
//							break;
//						}
						
						
									
						
//					}	
					
					
					
//					SortAccountDataInput(aaaaaaaa);
				
					
//				}

			return -1000000000;
			
			
		}
							
		
		
		
		
 		 public class BoxC
        {
            string iText;
            string iName;
            int iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRect;
            bool iHovered;
			
			double iThisTop;
			double iThisBottom;
			
			bool iIsHighestHigh1 = false;
			bool iIsLowestLow1 = false;
			bool iIsHighestHigh2 = false;
			bool iIsLowestLow2 = false;
			
			int iEndLines;

            public string Text { get { return iText; } set { iText = value; } }
            public string Name { get { return iName; } set { iName = value; } }
            public int Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }
			
			public double ThisTop { get { return iThisTop; } set { iThisTop = value; } }
			public double ThisBottom { get { return iThisBottom; } set { iThisBottom = value; } }
			
			public int EndLines { get { return iEndLines; } set { iEndLines = value; } }
			
			public bool IsHighestHigh1 { get { return iIsHighestHigh1; } set { iIsHighestHigh1 = value; } }
			public bool IsLowestLow1 { get { return iIsLowestLow1; } set { iIsLowestLow1 = value; } }
			public bool IsHighestHigh2 { get { return iIsHighestHigh2; } set { iIsHighestHigh2 = value; } }
			public bool IsLowestLow2 { get { return iIsLowestLow2; } set { iIsLowestLow2 = value; } }
			
        }
		
		
		
		private bool AH, AL, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15 = false;
	
		private double L1, L2, L3, L4, L5, L6 = 999999;
		private double LL = 999999;
	
		private double TT, TT1, TT2 = 0;
		private double DHH = 0;
		private double DLL = 999999;
		private double DTT = 0;

	
		private double PCLOSE, PP, S1, S2, S3, R1, R2, R3 = 0;
	
		private Series<string> VLS;

		private bool FirstEnd = false;	
	
		private bool RunInit = true;
		private DateTime StartTime, EndTime, FinalTime, CurrentTime, LastBarTime, FirstTime, ExpireTime;
		private DateTime R1TTime, R2TTime, R3TTime, R4TTime, R5TTime, R6TTime, R7TTime, R8TTime;
	
		private DateTime FirstBarTime;
	
	
		private TimeSpan pAsianStart = new TimeSpan(19,00,0);
		private TimeSpan pAsianEnd = new TimeSpan(0,0,0);
	
		private TimeSpan pLondonStart = new TimeSpan(2,00,0);
		private TimeSpan pLondonEnd = new TimeSpan(5,0,0);
	
		private TimeSpan pNYStart = new TimeSpan(8,30,0);
		private TimeSpan pNYEnd = new TimeSpan(10,0,0);
	
		private TimeSpan pSwingStart = new TimeSpan(10,0,0);
		private TimeSpan pSwingEnd = new TimeSpan(11,0,0);
	
		private DateTime AsianSTime, AsianETime, LondonSTime, LondonETime, NYSTime, NYETime, SwingSTime, SwingETime;
	
		private DateTime Midnight;
	
	
		private DateTime LaunchedAt = DateTime.MinValue;
		private bool UseChartData = false;
		private double CurrentHigh = 0;
		private double CurrentLow = 9999999;
	
		private bool PlotNow = false;

		private int DS = 0;
	
		private int HoursFromEST = 0;
	
		private double HH = 0;
	
		private double DSHH, MSHH = 0;
		private double DSLL, MSLL = 9999999;
	
		private double LALL, ALL, PALL = 9999999;
		private double LAHH, AHH, PAHH = 0;
	
		private double LLL, PLLL = 9999999;
		private double LHH, PLHH = 0;
	
	
		private double PDSHH, PMSHH = 0;
		private double PDSLL, PMSLL = 9999999;

		Point startPoint	= new Point(0,0);
		Point endPoint		= new Point(0,0);			
	
		private List<TimeSpan> AllTimes = new List<TimeSpan>();
		private List<int> AllOnePrices = new List<int>();
		
		SortedDictionary<int, int> AllCounts = new SortedDictionary<int, int>();
		SortedDictionary<int, int> AllCounts2 = new SortedDictionary<int, int>();
		
		
		private Point MP;

        private SharpDX.RectangleF B2 = new SharpDX.RectangleF(0, 0, 0, 0);
		private SharpDX.RectangleF B22 = new SharpDX.RectangleF(0, 0, 0, 0);
		private SharpDX.RectangleF B23 = new SharpDX.RectangleF(0, 0, 0, 0);

        private bool InMenu;
        private bool InMenuP;
		
        private bool InMenu2;
        private bool InMenu2P;
		
        private bool InMenu3;
        private bool InMenu3P;
		
        private bool ButtonOff = false;

		private double CurrentMousePrice = 0;

        private int space = 5;

		private string CopyButtonString = "Copier";
		private string RiskButtonString = "Risk";
		
        SortedDictionary<double, ButtonZ> AllButtonZ = new SortedDictionary<double, ButtonZ>();
		SortedDictionary<double, ButtonZ> AllButtonZ2 = new SortedDictionary<double, ButtonZ>();
		SortedDictionary<double, ButtonZ> AllButtonZ3 = new SortedDictionary<double, ButtonZ>();
		
 		public class ButtonZ
        {
            string iText;
            string iName;
            int iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRect;
            bool iHovered;
			string iLocation;
			int iBrushOn = 0;
			int iBrushOff = 0;
			int iBrushOnHover = 0;
			int iBrushOffHover = 0;
			int iBrushOutlineHover = 0;

            public string Text { get { return iText; } set { iText = value; } }
            public string Name { get { return iName; } set { iName = value; } }
            public int Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }
			public string Location { get { return iLocation; } set { iLocation = value; } }
			public int BrushOn { get { return iBrushOn; } set { iBrushOn = value; } }
			public int BrushOff { get { return iBrushOff; } set { iBrushOff = value; } }
			public int BrushOnHover { get { return iBrushOnHover; } set { iBrushOnHover = value; } }
			public int BrushOffHover { get { return iBrushOffHover; } set { iBrushOffHover = value; } }
			public int BrushOutlineHover { get { return iBrushOutlineHover; } set { iBrushOutlineHover = value; } }
        }
		
	
		
 		public class Drawings
        {
            string iText;
            string iName;
            int iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRect;
            bool iHovered;
			string iLocation;
			int iThisB;
			float iThisBOpacity;

            public string Text { get { return iText; } set { iText = value; } }
            public string Name { get { return iName; } set { iName = value; } }
            public int Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }
			public string Location { get { return iLocation; } set { iLocation = value; } }
			public int ThisB { get { return iThisB; } set { iThisB = value; } }
			public float ThisBOpacity { get { return iThisBOpacity; } set { iThisBOpacity = value; } }
		}
		private List<Drawings> AllDrawings = new List<Drawings>();

		SortedDictionary<string, string> AccountNameToRandom = new SortedDictionary<string, string>();
		SortedDictionary<double, ButtonBOT> AllButtonZ5 = new SortedDictionary<double, ButtonBOT>();
		SortedDictionary<double, ButtonBOT> AllButtonZ6 = new SortedDictionary<double, ButtonBOT>();
		SortedDictionary<double, ButtonBOT> AllButtonZ7 = new SortedDictionary<double, ButtonBOT>();
		
 		public class ButtonBOT
        {
            PNLStatistics iText;
           
            SharpDX.RectangleF iRect;
    
			string iName;

            public PNLStatistics Text { get { return iText; } set { iText = value; } }
 
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
       
			public string Name { get { return iName; } set { iName = value; } }
        }		
		
		
		
	
		private List<PNLStatistics> SortedList2 = new List<PNLStatistics>();// = new SortedDictionary<double, BOTChart>();
		
		
	
									
//								int LastEntry = 1;
//							int LastDir = 1;
//							double totallongprices = 0;
//							int totallongqty = 0;
//							double totalshortprices = 0;
//							int totalshortqty = 0;
													
							
							
//							double averagelongprice = 0;
//							double averageshortprice = 0;
//							double moneyforthisinstrument = 0;
		
		
		
		private List<PNLStatistics> AllPNLStatistics2 = new List<PNLStatistics>();// = new SortedDictionary<double, BOTChart>();
		
		
	//	private List<PNLStatistics> TotalAccountPNLStatistics = new List<PNLStatistics>();// = new SortedDictionary<double, BOTChart>();
		
		
	
		
 		public class PNLStatistics
        {

			
			Instrument iInst;
			Position iPos;
			
			List<Position> iAllPositions = new List<Position>();// 
	
			
			
			List<double> iAllLongEntries = new List<double>();// 
			List<double> iAllShortEntries = new List<double>();// 
			
			int iLastPosition;
			
            string iHovered;
			string iTimeStatus;
			string iAudioStatus;
			string iMasterStatus;
			//VeritasBOT2 iIndy;
			//NinjaTrader.NinjaScript.Strategies.VeritasBOT2S iStrat;
			
			
			double iPNLU;
			double iPNLUALL;
			double iPNLR;
			double iPNLGR;
			double iPNLTotal;
			double iCommish;
			double iCashValue;
			double iNetLiquidation;
			double iPeakValue;
			double iAudioLiq;
			double iFromBlown;
			double iFromFunded;
			double iTrailingThre;
			double iFundedAmount;
			double iMaxPayout;
			
			int iPNLQ;
			int iATMQ;
			string iSignal;
			DateTime iLastUpdate;

			
			string iPrivateName;
			
			Account iAcct;
		
			int iPositionLong = 0;
			int iPositionShort = 0;
			
			//Position iAllPositionst = 0;
			
			
			
			int iPositionMicroLong = 0;
			int iPositionMicroShort = 0;			
			
			int iPendingEntry = 0;
			int iPendingExit = 0;
			
			int iLastEntry = 1;
			int iLastDir = 1;
			double iTotalLongPrices = 0;
			int iTotalLongQty = 0;
			double iTotalShortPrices = 0;
			int iTotalShortQty = 0;
									
			
			int iTotalBought = 0;
			int iTotalSold = 0;
			
			
			
			double iAverageLongPrice = 0;
			double iAverageShortPrice = 0;
			double iTotalMoney = 0;
			
			int iTotalAccounts = 0;
			int iOneTradeReady = 0;
			int iFrozenOrders = 0;
			
			int iHitGoalOrLoss = 0;
			
			public int PositionLong { get { return iPositionLong; } set { iPositionLong = value; } }
			public int PositionShort { get { return iPositionShort; } set { iPositionShort = value; } }
			
			public int PositionMicroLong { get { return iPositionMicroLong; } set { iPositionMicroLong = value; } }
			public int PositionMicroShort { get { return iPositionMicroShort; } set { iPositionMicroShort = value; } }
			
			
			public int PendingEntry { get { return iPendingEntry; } set { iPendingEntry = value; } }
			public int PendingExit { get { return iPendingExit; } set { iPendingExit = value; } }
					
			
			
			public int TotalBought { get { return iTotalBought; } set { iTotalBought = value; } }
			public int TotalSold { get { return iTotalSold; } set { iTotalSold = value; } }
			
			
			
			public Account Acct { get { return iAcct; } set { iAcct = value; } }
			
			public int LastPosition { get { return iLastPosition; } set { iLastPosition = value; } }
		
			public Instrument Inst { get { return iInst; } set { iInst = value; } }
			public Position Pos { get { return iPos; } set { iPos = value; } }	

			public List<Position> AllPositions { get { return iAllPositions; } set { iAllPositions = value; } }
			
			
            public List<double> AllLongEntries { get { return iAllLongEntries; } set { iAllLongEntries = value; } }
			public List<double> AllShortEntries { get { return iAllShortEntries; } set { iAllShortEntries = value; } }			
			
			
			
            public string Hovered { get { return iHovered; } set { iHovered = value; } }
			public string TimeStatus { get { return iTimeStatus; } set { iTimeStatus = value; } }
			public string AudioStatus { get { return iAudioStatus; } set { iAudioStatus = value; } }
			public string MasterStatus { get { return iMasterStatus; } set { iMasterStatus = value; } }
			
		
		//	public VeritasBOT2 Indy { get { return iIndy; } set { iIndy = value; } }
			//public NinjaTrader.NinjaScript.Strategies.VeritasBOT2S Strat { get { return iStrat; } set { iStrat = value; } }
			
			public double PNLU { get { return iPNLU; } set { iPNLU = value; } }
			public double PNLUALL { get { return iPNLUALL; } set { iPNLUALL = value; } }
			public double PNLR { get { return iPNLR; } set { iPNLR = value; } }
			public double PNLGR { get { return iPNLGR; } set { iPNLGR = value; } }
			public double PNLTotal { get { return iPNLTotal; } set { iPNLTotal = value; } }
			
			
			
			public double Commish { get { return iCommish; } set { iCommish = value; } }
			
			public double CashValue { get { return iCashValue; } set { iCashValue = value; } }
			public double NetLiquidation { get { return iNetLiquidation; } set { iNetLiquidation = value; } }
			public double PeakValue { get { return iPeakValue; } set { iPeakValue = value; } }
			public double AudioLiq { get { return iAudioLiq; } set { iAudioLiq = value; } }
			public double FromBlown { get { return iFromBlown; } set { iFromBlown = value; } }
			public double FromFunded { get { return iFromFunded; } set { iFromFunded = value; } }
			public double TrailingThre { get { return iTrailingThre; } set { iTrailingThre = value; } }
			public double FundedAmount { get { return iFundedAmount; } set { iFundedAmount = value; } }
			public double MaxPayout { get { return iMaxPayout; } set { iMaxPayout = value; } }
			
			
			
			
			public int PNLQ { get { return iPNLQ; } set { iPNLQ = value; } }
			public int ATMQ { get { return iATMQ; } set { iATMQ = value; } }
			public string Signals { get { return iSignal; } set { iSignal = value; } }
			public DateTime LastUpdate { get { return iLastUpdate; } set { iLastUpdate = value; } }
			
			
			
			public string PrivateName { get { return iPrivateName; } set { iPrivateName = value; } }
			
			
			
			public int LastEntry { get { return iLastEntry; } set { iLastEntry = value; } }
			public int LastDir { get { return iLastDir; } set { iLastDir = value; } }			
			public double TotalLongPrices { get { return iTotalLongPrices; } set { iTotalLongPrices = value; } }
			public int TotalLongQty { get { return iTotalLongQty; } set { iTotalLongQty = value; } }				
			public double TotalShortPrices { get { return iTotalShortPrices; } set { iTotalShortPrices = value; } }
			public int TotalShortQty { get { return iTotalShortQty; } set { iTotalShortQty = value; } }						
			
			public double AverageLongPrice { get { return iAverageLongPrice; } set { iAverageLongPrice = value; } }
			public double AverageShortPrice { get { return iAverageShortPrice; } set { iAverageShortPrice = value; } }			
			public double TotalMoney { get { return iTotalMoney; } set { iTotalMoney = value; } }
			public int TotalAccounts { get { return iTotalAccounts; } set { iTotalAccounts = value; } }
			public int OneTradeReady { get { return iOneTradeReady; } set { iOneTradeReady = value; } }
			
			public int FrozenOrders { get { return iFrozenOrders; } set { iFrozenOrders = value; } }
			public int HitGoalOrLoss { get { return iHitGoalOrLoss; } set { iHitGoalOrLoss = value; } }
			
        }

		private SortedDictionary<string, PNLStatistics> TotalAccountPNLStatistics = new SortedDictionary<string, PNLStatistics>();
		
		private SortedDictionary<string, PNLStatistics> AllPNLStatistics = new SortedDictionary<string, PNLStatistics>();
		private SortedDictionary<string, PNLStatistics> FinalPNLStatistics = new SortedDictionary<string, PNLStatistics>();
				
		
		
		private float minx = 999999;
		private	float miny = 999999;
		private bool FirstRender = true;
		
		private double ThisLastOnePrice = 0;
		
		private int StartBar1 = 0;
		private int StartBar2 = 0;
		
		private double t1,t2,t3,t4,t5,t6,t7,t8 = 0;
		private double p1,p2,p3,p4,p5,p6,p7,p8 = 0;
		private double totalboxcount = 0;
		
		private int ThisCurrentBarNow = 0;
		
		
		private System.Windows.Threading.DispatcherTimer timer2;
		private System.Windows.Threading.DispatcherTimer timer3;
		private System.Windows.Threading.DispatcherTimer timer4;
		
		private List<Window> AllWindowsNow; // = 	Globals.AllWindows;
		
		SortedDictionary<int, Window> AllWindowsNow2 = new SortedDictionary<int, Window>();
		
		private bool FirstLoadAccounts = true;
		
////		

		private bool drawSwitch;
		private bool indiSwitch;
				
	    // Define a Chart object to refer to the chart on which the indicator resides
		private Chart chartWindow;
		private NinjaTrader.Gui.Chart.ChartTab		chartTab;
	 
			private System.Windows.Controls.TabItem		tabItem;		
		
		
	    // Define a Button
	    private new System.Windows.Controls.Button btnDrawObjs;
		
	//	btnDrawObjs
		
		private new System.Windows.Controls.Button btnIndicators;
		private new System.Windows.Controls.Button btnShowTrades;
		private new System.Windows.Controls.Button btnHideWicks;
		
		private bool IsToolBarButtonAdded;
		
		List<string> AllATMStrategies = new List<string>();	
					
		
		public void CopierMainButton()
		{
			RemoveMessage(AllKorean);
						RemoveMessage("You must add at");
					
						
						RemoveMessage(AllKorean);
						RemoveMessage("카피트레이딩 작업을 활성화하려면 최소한 하나의 팔로워 계정을 추가해야 합니다. 계좌 이름을 클릭하여 팔로워 계정으로 설정하십시오.");
					
					
						if (pIsBuildMode)
						{
							
						
							if (AccountsAreConfigured())
							{
								AdjustmentRows = 0;
								pIsBuildMode = false;
								
							}
							else
							{
								
								string enddddd = "enable Duplicate Account Actions";
								//string enddddd = "'Lock' Accounts Dashboard";
								
								
								if (AllDuplicateAccounts.Count == 0 && pThisMasterAccount == string.Empty)
								{
									if (TotalAccounts != 0)
									{
										if (!IsKorean)
										AddMessage("You must set the Master account and add at least one Follower account to " + enddddd + ".  Double click the connected status dot to set the Master account.  Click the account name to set it as a Follower account.", 20, RED);
								
										if (IsKorean)
										{
											AddMessage(AllKorean, 20, RED);
										}
										
									}
									
									
									
								}
								else if (pThisMasterAccount == string.Empty)
								{
									if (!IsKorean)
									AddMessage("You must set the Master account to " + enddddd + ".  Double click the connected status dot to set the Master account.", 20, RED);
									
									if (IsKorean)
									{
										AddMessage(AllKorean, 20, RED);
									}									
									
								}
								else if (AllDuplicateAccounts.Count == 0)
								{
									if (!IsKorean)
									AddMessage("You must add at least one Follower account to " + enddddd + ".  Click the account name to set it as a Follower account.", 20, RED);
									
									if (IsKorean)
									{
										AddMessage("카피트레이딩 작업을 활성화하려면 최소한 하나의 팔로워 계정을 추가해야 합니다. 계좌 이름을 클릭하여 팔로워 계정으로 설정하십시오.", 20, RED);
									}
									
								}	
								else
								{
									
									if (CurrentMasterAccount != null)
									{
										bool IsConnectedOK = pAllowDisconnectedAccounts || CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected;

										if (IsConnectedOK)
										//if (IsConnectedOK && CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected)
										{
											
										}
										else
										{
											
											AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
											
										}
										
									}
									else
									{
										
										AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
									}
									
									
								}
								
							}
							
							
								
						}					
					
					
						if (pCopierIsEnabled)
						{
							pCopierIsEnabled = false;
							
						
						}
						else
						{
							if (!pIsBuildMode)
							{
								if (AccountsAreConfigured())
								{
	
									{
										pCopierIsEnabled = true;
										
										RemoveMessage("You are placing trades on the Master account,");
													
										
									}
								}
							}
							else
							{
							
								//AddMessage("Click the Lock button to Lock the Accounts Dashboard before enabling Duplicate Account Actions.", 20, RED);
								
							}
							
							
						}					
						
						
						
						
						
		}

		private void CopierOnOff()
					{
						
					
							string instending = "before changing the Duplicate Account Actions mode.";
					
							if (TotalPendingOrders != 0 && TotalPositions != 0)
							{
								RemoveMessage(instending);
								AddMessage("Please flatten all accounts " + instending, 10, RED);
								
							}
							else if (TotalPendingOrders != 0)
							{
								RemoveMessage(instending);
								AddMessage("Please cancel all pending orders " + instending, 10, RED);
							}
							else if (TotalPositions != 0)
							{
								RemoveMessage(instending);
								AddMessage("Please close all positions " + instending, 10, RED);
							}	
							else
							{
								
									
								
								if (pCopierMode == "Executions")
								{
									pCopierMode = "Orders";
									
									RemoveMessage("The 'Fade' column cannot");
									RemoveMessage("카피트레이딩 작업'이 '주문모드'에서 실행 중일 때는 '반대진입' 기능을 사용할 수 없습니다. '반대진입' 기능을 사용하려면 모드를 '체결모드' 로 설정해 주세요.");
									
									
									if (pIsFadeEnabled)
									{
										if (!IsKorean)
											AddMessage("The 'Fade' column cannot be used when Duplicate Account Actions is running in 'Orders' mode.  Please set the mode to 'Executions' for using the 'Fade' column.", 30, RED);
										else
											AddMessage("카피트레이딩 작업'이 '주문모드'에서 실행 중일 때는 '반대진입' 기능을 사용할 수 없습니다. '반대진입' 기능을 사용하려면 모드를 '체결모드' 로 설정해 주세요.", 30, RED);

									
										pIsFadeEnabled = false;
									
									}
									
									
									
	
								}
								else
								{

									pCopierMode = "Executions";
									
								}					
								
								if (IsKorean)
								{
									RemoveMessage("카피트레이딩 창이 '주문모드'로 업데이트되었습니다. 자세한 내용은 Members Area의 사용자 매뉴얼을 참조하십시오!");
									RemoveMessage("카피트레이딩 창이 '체결모드'로 업데이트되었습니다. 자세한 내용은 Members Area의 사용자 매뉴얼을 참조하십시오!");
									
									if (pCopierMode == "Orders")
										AddMessage("카피트레이딩 창이 '주문모드'로 업데이트되었습니다. 자세한 내용은 Members Area의 사용자 매뉴얼을 참조하십시오!", 15, RED);
									if (pCopierMode == "Executions")
										AddMessage("카피트레이딩 창이 '체결모드'로 업데이트되었습니다. 자세한 내용은 Members Area의 사용자 매뉴얼을 참조하십시오!", 15, RED);	
								}
								else
								{
								
									RemoveMessage("Duplicate Account Actions has been updated");									
									AddMessage("Duplicate Account Actions has been updated to '" + pCopierMode + "' mode.  Please see the user manual, in Members Area, for more information!", 15, GOLDENROD);
																	
									
								}

							
									
							}
							
					
					
					
						
						
						
						
						
						//btnDrawObjs.Content = pCopierMode;
				
					}

		private void btnDrawObjsClick(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button != null)
			{			
				//button.IsEnabled = true;
				CopierOnOff();
				//button.IsEnabled = false;
				
				//ChartControl.InvalidateVisual();
				
				
				//hideDrawsFunc();
				
//				pCopierIsEnabled = !pCopierIsEnabled;
				
//				if (pCopierIsEnabled)
//				{
//					btnDrawObjs.Background = Brushes.Green;
//				}
//				else
//				{
//					btnDrawObjs.Background = Brushes.Red;
//				}
				
				
				
				
			}
		}		

		private void btnDrawObjsMouseEnter(object sender, RoutedEventArgs e)
		{
			
			//Print("mouse e");

			
//			if (pCopierIsEnabled)
//			{
//				btnDrawObjs.Background = Brushes.Green;
//			}
//			else
//			{
//				btnDrawObjs.Background = Brushes.Red;
//			}
				
			e.Handled = true;
			
			
		}				
		
		private bool pAllowBothFeaturesToBeOff = true;
		
		public Brush ThisChartBackground()
		{
			//Print(myProperties.ChartBackground.ToString());
			
			//myProperties.ChartBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30,30,30)); // Active Background Dark Gray
			
			if (myProperties.ChartBackground.ToString().Contains("GradientBrush"))
			{
				//return myProperties.ChartBackground;
				string skinname = NinjaTrader.Core.Globals.GeneralOptions.Skin.ToString();
				if (skinname == "NinjaTrader Dark" || skinname == "Dark" || skinname == "Slate Dark" || skinname == "Slate Gray")
						{
							//Print("setting b");
							return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30,30,30));
							//return Brushes.Silver;
						}
						else
						{
							return Brushes.White;
						}
				
			}
			else
			{
				return myProperties.ChartBackground;
			}
			
			//Color bg = new Pen(bg2,1).;
//			Color bg = (bg2 as SolidColorBrush).Color;
			
			
//			double a = 1 - ( 0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B)/255;
//            if (a < 0.5)
//               return Brushes.Black;
//            else
//               return Brushes.White;
			
//		    int nThreshold = 150;
//		    int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + 
//		                                  (bg.B * 0.114));

//		    Brush foreColor = (255 - bgDelta < nThreshold) ? Brushes.Black : Brushes.White;
//		    return foreColor;
		}
		
		
		
		
		public Brush GetTextColor2(Brush bg2)
		{
			
			//Color bg = new Pen(bg2,1).;
			Color bg = (bg2 as SolidColorBrush).Color;
			
			
			double a = 1 - ( 0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B)/255;
            if (a < 0.5)
               return Brushes.Black;
            else
               return Brushes.White;
			
//		    int nThreshold = 150;
//		    int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + 
//		                                  (bg.B * 0.114));

//		    Brush foreColor = (255 - bgDelta < nThreshold) ? Brushes.Black : Brushes.White;
//		    return foreColor;
		}

		
		// Runs ShowWPFControls if this is the selected chart tab, other wise runs HideWPFControls()
		private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as ChartTab;
			if (chartTab == null)
				return;

//			if (TabSelected())
//				ShowWPFControls();
//			else
//				HideWPFControls();
			
			CheckTab();
			
				
			
			
		}
		
		private bool TabSelected()
		{
			if (ChartControl == null || chartWindow == null || chartWindow.MainTabControl == null)
				return false;

			bool tabSelected = false;

			if (ChartControl.ChartTab == ((chartWindow.MainTabControl.Items.GetItemAt(chartWindow.MainTabControl.SelectedIndex) as TabItem).Content as ChartTab))
				tabSelected = true;

			return tabSelected;
		}

		
		
		private void AddButtonToToolbar()
		{
				//Obtain the Chart on which the indicator is configured
				chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;
		        if (chartWindow == null)
		        {
		            Print("chartWindow == null");
		            return;
		        }
			
				chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
			
			
		//	chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			
			
			
				
				// subscribe chartwindow to keypress events
//				if (chartWindow != null)
//				{
//					chartWindow.KeyUp += OnKeyUp;
//					chartWindow.MouseLeftButtonDown += OnMouseLeftDown;					
//					chartWindow.PreviewMouseWheel += OnMouseWheel;
//					chartWindow.MouseEnter += OnMouseEnter;
//					chartWindow.MouseLeave += OnMouseLeave;
//				}
				
				// Create a style to apply to the button
		        Style btnStyle = new Style();
				
		       // btnStyle.TargetType = typeof(System.Windows.Controls.Button);
				
		        btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontSizeProperty, 13.0));
		       	//btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontFamilyProperty, new FontFamily("Arial")));
		        //btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontWeightProperty, FontWeights.Bold));
				//btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.MarginProperty, new Thickness(2, 0, 2, 0)));
				//btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.PaddingProperty, new Thickness(4, 2, 4, 2)));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, Brushes.Transparent));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.BackgroundProperty, Brushes.Transparent));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.IsEnabledProperty, false));
				//btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.HorizontalAlignmentProperty, HorizontalAlignment.Center));

				//System.Windows.Controls.Button.BorderBrushProperty = BorderBrushProperty.
				
				
				
//		<Window x:Class="SomeOrg.SomeApp.WpfInterface.MainWindow"
//        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
//        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
//        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
//        xmlns:local="clr-namespace:WpfInterface"
//        mc:Ignorable="d"
//        Title="SomeApp" Height="450" Width="800"
//        Background="#ffffffe1">
//    <Window.Resources>
//        <Style x:Key="MyButton" TargetType="Button">
//            <Setter Property="OverridesDefaultStyle" Value="True" />
//            <Setter Property="Cursor" Value="Hand" />
//            <Setter Property="Template">
//                <Setter.Value>
//                    <ControlTemplate TargetType="Button">
//                        <Border Name="border" BorderThickness="0" BorderBrush="Black" Background="{TemplateBinding Background}">
//                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center" />
//                        </Border>
//                    </ControlTemplate>
//                </Setter.Value>
//            </Setter>
//        </Style>
//    </Window.Resources>
//    <Grid>
//        <Grid x:Name="grid" HorizontalAlignment="Stretch" Height="Auto" Margin="10,10,10,10" VerticalAlignment="Stretch"
//              Width="Auto" Background="#ffffffe1"/>
//    </Grid>
//</Window>
				
				
				
				//System.Windows.Controls.Button.
				//btnStyle.Setters.
				//System.Windows.Controls.Button.mouse
				
		        // Instantiate the buttons
		        btnDrawObjs = new System.Windows.Controls.Button();
//				btnIndicators = new System.Windows.Controls.Button();
//				btnShowTrades = new System.Windows.Controls.Button();
//				btnHideWicks = new System.Windows.Controls.Button();
				
				
				
				
//				btnIndicators.Content = "Toggle Indicators";
//				btnShowTrades.Content = "Toggle Trades";
//				btnHideWicks.Content = "Toggle Wicks";
								
		        // Set Button style            
		        btnDrawObjs.Style = btnStyle;
//				btnIndicators.Style = btnStyle;
//				btnShowTrades.Style = btnStyle;
//				btnHideWicks.Style = btnStyle;
				
				
				//btnDrawObjs.Content = pCopierMode;
				
						int totalacccounts = 0;
						
						if (CurrentMasterAccount != null)
							totalacccounts = 1;
						
						totalacccounts = totalacccounts + AllDuplicateAccounts.Count;
										
				//btnDrawObjs.Content = pCopierMode + "   |   " + totalacccounts + " Accounts";
					            
//						if (ChartControl != null)
//						btnDrawObjs.Foreground = GetTextColor2(ThisChartBackground());
			
				
				// Add the Buttons to the chart's toolbar
				
				
				
				
				btnIndicators = new System.Windows.Controls.Button();
				btnIndicators.Style = btnStyle;
				btnIndicators.Content = "All Instruments";
				
				
				btnShowTrades = new System.Windows.Controls.Button();
				btnShowTrades.Width = 13;
				btnShowTrades.Visibility = Visibility.Hidden;
				
//						Print("hey");
						
//						foreach (object oooo in chartWindow.MainMenu)
//						{
						
//							Print(oooo.GetType().ToString());
							
//						}
				
				if (pRemoveIcons)
				if (pUseToolbarButtons && chartWindow!=null && chartWindow.MainMenu!=null)
				{
					//chartWindow.MainMenu.Add(btnIndicators);
					chartWindow.MainMenu.Add(btnShowTrades);	
					chartWindow.MainMenu.Add(btnDrawObjs);	
					
				}
				
//				btnShowTrades.Name = "222";
//				btnDrawObjs.Name = "333";
				
//				chartWindow.MainMenu.Add(btnIndicators);
//				chartWindow.MainMenu.Add(btnShowTrades);
//				chartWindow.MainMenu.Add(btnHideWicks);
				
				// Set button visibility
				btnDrawObjs.Visibility = Visibility.Visible;
//				btnIndicators.Visibility = Visibility.Visible;
//				btnShowTrades.Visibility = Visibility.Visible;
//				btnHideWicks.Visibility = Visibility.Visible;
				
				// Subscribe to click events
				btnDrawObjs.Click += btnDrawObjsClick;
				btnDrawObjs.MouseEnter += btnDrawObjsMouseEnter;
				
				
				
				CheckTab();
	
				
				
//				btnIndicators.Click += btnIndicatorsClick;
//				btnShowTrades.Click += btnShowTradesClick;
//		 		btnHideWicks.Click += btnHideWicksClick;
				
				// Set this value to true so it doesn't add the
				// toolbar multiple times if NS code is refreshed
		        IsToolBarButtonAdded = true;

				if (chartWindow!=null && chartWindow.MainMenu!=null)
				{
					chartWindow.MainMenu.Add(atmStrategySelector);
					atmStrategySelector.Visibility = Visibility.Collapsed;
				}
		}		

		private void CheckTab()
		{
				
				if (TabSelected())
				{
					
					if (chartWindow!=null)
						chartWindow.Caption = "Accounts Dashboard - 24. 3. 8. 1";
					btnDrawObjs.Visibility = Visibility.Visible;	
					btnShowTrades.Visibility = Visibility.Hidden;
					
				}
				else
				{
					if (chartWindow!=null)
						chartWindow.Caption = "Chart";
					btnDrawObjs.Visibility = Visibility.Collapsed;
					btnShowTrades.Visibility = Visibility.Collapsed;
				}
				
				if (!pIsCopyBasicFunctionsChecked)
				{
					btnDrawObjs.Visibility = Visibility.Collapsed;
					
				}
			
		}

		private void DisposeCleanUp()
		{
		    // remove toolbar items and unsubscribe from events
//			chartWindow.KeyUp -= OnKeyUp;
//			chartWindow.MouseLeftButtonDown -= OnMouseLeftDown;
//			chartWindow.PreviewMouseWheel -= OnMouseWheel;
//			chartWindow.MouseEnter -= OnMouseEnter;
//			chartWindow.MouseLeave -= OnMouseLeave;
						
            if (btnDrawObjs != null) 
			{
				chartWindow.MainMenu.Remove(btnDrawObjs);
				chartWindow.MainMenu.Remove(btnShowTrades);
				chartWindow.MainMenu.Remove(atmStrategySelector);
				
				
				btnDrawObjs.Click -= btnDrawObjsClick;
			
				btnDrawObjs.MouseEnter -= btnDrawObjsMouseEnter;
			}
			
						if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
						
						
//			if (btnIndicators != null) chartWindow.MainMenu.Remove(btnIndicators);
//				btnIndicators.Click -= btnIndicatorsClick;
//			if (btnShowTrades != null) chartWindow.MainMenu.Remove(btnShowTrades);
//				btnShowTrades.Click -= btnShowTradesClick;
//			if (btnHideWicks != null) chartWindow.MainMenu.Remove(btnHideWicks);
//				btnHideWicks.Click -= btnHideWicksClick;
		}
				
		
		bool FirstLoadAccountData = true;
		
		bool pUseToolbarButtons = true;
	
		string ThisCulture = string.Empty;
		bool IsKorean = false;
		
		protected override void OnStateChange()
		{
 			//#region -- OnStateChange
			//Print("543" + State);
			
			
		
			
			if (State == State.SetDefaults)
			{
				ClearOutputWindow();
				Name = ThisName;
				Description = "";
				//pIsCopyBasicFunctionsPermission = true;//ben adds this line to turn on the MAIN button
				
				

				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsAutoScale = false;
				ArePlotsConfigurable = false;
				ShowTransparentPlotsInDataBox = false;   
				
				IsSuspendedWhileInactive	= false;
				
	     
				IsChartOnly = true;
				
				//VeritasBOT2Track
			
				
				LaunchedAt = DateTime.Now; // get the time when the indicator is launched
				

			
									
					TextFont3						= new SimpleFont("Arial",12);
					TextFont3.Bold = false;
				
				
				//TextFont4						= new SimpleFont("Arial",12);
				
				//AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1),	PlotStyle.Line, "Latest One Price");
				//AddPlot(new Stroke(Brushes.Black, DashStyleHelper.Solid, 1),	PlotStyle.Line, "Low Point");
				//AddPlot(new Stroke(Brushes.Black, DashStyleHelper.Solid, 1),	PlotStyle.Line, "Mid Point");
				//AddPlot(new Stroke(Brushes.Black, DashStyleHelper.Solid, 1),	PlotStyle.Line, "Signals");

			}
			else if (State == State.Configure)
		    {
 				IsAutoScale = false;

				//AddDataSeries(BarsPeriodType.Minute, 1);

					pThisBarPeriod1 = 15;

					BarsPeriod BP = new BarsPeriod();					
					BP.BarsPeriodType = AcceptableBasePeriodType1;
					BP.Value = pThisBarPeriod1;
					
					//FinalBarsToLoad = (int) ((pNumberOfDaysToLoadMinute1 * 1440 / pThisBarPeriod1) / 7 * 5);
					//FinalBarsToLoad = (int) (pNumberOfDaysToLoadMinute1 * 1440 / pThisBarPeriod1);
					
					int FinalBarsToLoad = (int) (3 * 1440 / pThisBarPeriod1);
					
//					Print(BP);
//					Print(FinalBarsToLoad);
					
					AddDataSeries(null, BP, FinalBarsToLoad, null, true);					

		    }
			else if (State == State.Transition)
		    {

				// helps it load fast
				
				ChartControl.Dispatcher.InvokeAsync(() =>
				{
					
					GetAllPerformanceStats();
				
					ChartControl.InvalidateVisual();
					
				});
				
												
		    }
			else if (State == State.Realtime)
		    {
 				

//				ChartControl.Dispatcher.InvokeAsync(() =>
//				{
					
//					GetAllPerformanceStats();
				
//					ChartControl.InvalidateVisual();
					
//				});
				

		    }				
			else if (State == State.DataLoaded)
			{
				
//				GetTradovateCommands();

				// this is not working
				
//				StreamReader mysr = new StreamReader(Path.Combine(Core.Globals.UserDataDir, "bin", "Custom", "Indicators", "aiDuplicateAccountsActionsBen"));			
				
//				Page page = XamlReader.Load(mysr.BaseStream) as Page;

//				PropertyGrid propertyGrid;
//				propertyGrid = LogicalTreeHelper.FindLogicalNode(page, "PropertyGrid") as PropertyGrid;
				
//				for (int i = 0; i < propertyGrid.Properties.Count; i++) propertyGrid.Properties[i].IsExpanded = false;
				
				
				
				
				System.Globalization.CultureInfo currentCulture = null;
				
				try 
		        {
					currentCulture = System.Globalization.CultureInfo.CurrentCulture;
					ThisCulture = currentCulture.Name;
		        } 
		        catch (Exception e) 
		        {

		        }
				
				
				
				if (pSupportCode == "print")
					Print("ThisCulture " + ThisCulture);
			
				
				
				//ko-KR
				IsKorean = false;
				
				//ThisCulture = "ko-KR";
				
				
				if (pSelectedLanguage == "Default")		
				{
					if (ThisCulture.Contains("ko") || ThisCulture.Contains("KR"))
						IsKorean = true;
				}
				
				
				if (pSelectedLanguage == "Korean")	
					IsKorean = true;
				
				if (pSelectedLanguage == "English")	
					IsKorean = false;
				
				
				TheWarningMessage = "You must monitor positions and orders across all Follower accounts while trading the Master account.  This is your responsibility.  The process of duplicating trades across multiple accounts can be complicated based on many factors, including entry methods, exit methods, other third party software, and connections.  Always be prepared to manage positions and orders outside of the NinjaTrader Desktop Platform.  You can click this message to close it.";
	
		
				MainResetConfirmation = "again to confirm and";
				
				if (IsKorean)
				{
					
					MainResetConfirmation = "모든 계정의 설정을 초기화하려면";
				}
				
				
				
				
				//Print("Data Loaded 1a");
			
				if (!pIsCopyBasicFunctionsChecked && !pIsRiskFunctionsChecked)
				{
					AddMessage("Duplicate Account Actions and Account Risk Manager are both disabled.  You can enable the two products in the 'Main Features' section at the top of the 'aiDuplicateAccountsActionsBen' indicator Properties.", 10000, RED);
					
				}
				
				
				//Print("Data Loaded 1");
				
			
				foreach (Account acct in Account.All)
				{
					if (!AccountNameToRandom.ContainsKey(acct.Name))
						AccountNameToRandom.Add(acct.Name, RandomString(4));
					
				}

				if (ChartControl != null)
				ChartControl.Dispatcher.InvokeAsync((Action)(() =>
				{
					atmStrategySelector = new NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector();
		
		
				}));
		
				
				
				DefaultAtmStrategy.Clear();
				DefaultAtmStrategyNumber.Clear();
//				DefaultAtmStrategyPercentages.Clear();

		        string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "templates", "AtmStrategy"), "*.xml");  
		
		        foreach(string atm in files)
		        {
					string adfsfs = System.IO.Path.GetFileNameWithoutExtension(atm);
					
					//Print(adfsfs);
					
		            AllATMStrategies.Add(adfsfs);
				
		            //NinjaTrader.Code.Output.Process(System.IO.Path.GetFileNameWithoutExtension(atm), PrintTo.OutputTab1);
					
					//NinjaTrader.Code.OutputEventArgs
		        }				
			
				//pOldColumnDailyLoss = true;
				
				if (pOldColumnDailyLoss)
				
				foreach(string s in pAllAccountDailyLoss)
				{
	
					//s = s.Replace("|", "|-");
					
					
					
					if (s!= string.Empty)
					{
						string accountname = s.Split('|')[0];
						string accountnamev = s.Split('|')[1];
						
						
						if (!accountnamev.Contains("-"))
							SetAccountSavedData(accountname, pAllAccountDailyLoss, "-"+accountnamev);
					}	

					
					pOldColumnDailyLoss = false;
			
				}
				
				//Print("Data Loaded 2");
					
			
				if (pLastMultiplierMode != pMultiplierMode)
				{

					foreach(string s in pAllAccountData)
					{
		
						if (s!= string.Empty)
						{
							string accountname = s.Split('|')[0];
			
							SetAccountData("",accountname,"","1","","","","","","");
										
						}	
			
					}
								
					pLastMultiplierMode = pMultiplierMode;
					
					
				}

				pAPEXCoupon = "DVUDKBFF";
				pBulenoxCoupon = "";
				pEliteTraderFundingCoupon = "";
				pFundedFuturesNetworkCoupon = "";
				pLeeLooCoupon = "";
				pMyFundedFuturesCoupon = "";
				pTakeProfitTraderCoupon = "";
				pTradeDayCoupon = "";

				pAccountsSimEnabled = true;
				pAccountsLiveEnabled = true;
				
				if (pShowAccountsMain == "Live")
					pAccountsSimEnabled = false;
				
				
				if (pShowAccountsMain == "Sim")	
					pAccountsLiveEnabled = false;
			
				if (pShowAccountsMain != "All")
					pShowAccounts = pShowAccountsMain;
					
					
				if (!pShowAccountsButton)
				pShowAccounts = pShowAccountsMain;
				
				
				for (int i=0; i < pAllAccountData.Length; i++)
				{
					if (pAllAccountData[i] != string.Empty)
					{
						
						
						// might be no longer needed
						//pAllAccountData[i] = pAllAccountData[i].Replace("|0|", "|No|");
					
						
						// remove stored account data that is not actually in the accounts list in ninjatrader
						
						bool remove = true;
						string removes = string.Empty;
						
						foreach (Account acct in Account.All)
						{
						
							string accccnnnn = acct.Name;
								
							if (pAllAccountData[i].Contains(accccnnnn))
							{
								remove = false;
								removes = accccnnnn;
							}
						}
							
						if (remove)
						{
							pAllAccountData[i] = "";
							RemoveAccountData(removes);
						}
						
					}
					
				}
			
					
				SetWhichAccounts();
			
				//Print("Data Loaded 3");	
				
				if (pDisableOnStart)
					pCopierIsEnabled = false;
				
				if (pCopierIsEnabled)
					pIsBuildMode = false;
	
				
				
				
				if (pSelectedColumn == "–")
					pSelectedColumn = "Account";
		
				
				//Print("Data Loaded 3a");	
				
				
				UpdateMasterAccount(pThisMasterAccount);
				
				//Print(pThisMasterAccount);
				
				
				//Print("Data Loaded 3b");	
				
		 
			//	SetAccountData("4","","None","1","No","No","No","","No",""); // only for setting lists duplcate and cross, needs to be deleted and pulled from data itself
				SetAccountData("4","","","","","","","","","");
				
				//Print("Data Loaded 4");
		
				CheckForMissingATM();
				
				
	
				sizecolname = "Size";
//				if (pMultiplierMode == "Multiplier")
//					sizecolname = "x";
				
				
			
				ChartControl.Dispatcher.InvokeAsync(() =>
				{
					this.chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;		
//					chartWindow.Caption = "Accounts Dashboard - 24. 4. 5. 1";
					
//					if (!pRemoveIcons)
//					chartWindow.Caption = "Chart";
				});
				
				

				if (Name != ThisName && Name != string.Empty)
					Name = ThisName;					
				
				
										
				if (pAccountPrivacy1 == string.Empty && pAccountPrivacy2 == string.Empty && pAccountPrivacy3 == string.Empty && pAccountPrivacy4 == string.Empty && pAccountPrivacy5 == string.Empty && pAccountPrivacy6 == string.Empty && pAccountPrivacy7 == string.Empty && pAccountPrivacy8 == string.Empty && pAccountPrivacy9 == string.Empty && pAccountPrivacy10 == string.Empty && pAccountPrivacy11 == string.Empty && pAccountPrivacy12 == string.Empty)
				
					pPrivacyEnabled = false;
				
				//Print("Data Loaded 5");
			//	Without email added
				
				bool testlookup = false;
				
					
				if (!testlookup)
				{
				
				
			// checking for existing email tied to this computer
							
							
							string pFileLocation = NinjaTrader.Core.Globals.UserDataDir;
							
							if(!Directory.Exists(pFileLocation))
								Directory.CreateDirectory(pFileLocation);
							
							
							//look for file
								
	
					
						
							string pFileName = "AI";
						string MainFileName = pFileName + ".txt";
							
		
							string location2 = pFileLocation + MainFileName;	
							string final2 = pLicensingEmailAddress;
		
						string emailfound = string.Empty;
								
						string[] readText = null;
				        try 
				        {
							readText = File.ReadAllLines(location2);
							emailfound = readText[0];						
				        } 
				        catch (Exception e) 
				        {
	//			            Console.WriteLine("The process failed: {0}", e.ToString());
	//						Error = "Cannot find file ' " + path  + " '.";
	//						Print("Cannot find file ' " + path  + " '.");
				        }
						
						if (!pLicensingEmailAddress.Contains("@"))
						{
							pLicensingEmailAddress = "";
						}
						
						if (pLicensingEmailAddress == string.Empty)
							pLicensingEmailAddress = emailfound;
						
			
						if (pLicensingEmailAddress != string.Empty)
						if (pLicensingEmailAddress != emailfound || !File.Exists(location2))
						{			
							System.IO.File.WriteAllText(location2,final2);
						}
							
						
						//pLicensingEmailAddress = "";
						
			}
					
					
					

					
				// test without email being detected in website database
					
					//pLicensingEmailAddress = "kkkkk@gmail.com";
					
				
				
					//Permission = LicenseWordPress(NinjaTrader.Cbi.License.MachineId, pLicensingEmailAddress);
				
				
					Permission = true;
				
					//Permission = LicenseWordPress("FF5F819CE51BE82F981F05D59FE87B4D", "ballpenlove@naver.com");	
					
				//https://affordableindicators.com/ninjatrader.php?user=ballpenlove@naver.com&id=FF5F819CE51BE82F981F05D59FE87B4D&ind=aiDuplicateAccountsActionsBen&inst=MNQ&chart=Daily&random=2129771958

				//Print("Data Loaded 6");
					
				
//				if (pSupportCode.Contains("orders"))
//				{
				
//					pTheCopierMode = "Selection";
//					pSupportCode = "";
				
					
					
//				}
		
		
		
				AddButtonZ(AllButtonZ, "Main", "", 30, pCopierIsEnabled, "Top", LIME30, SILVER40, LIME60, SILVER70, SILVER40);
				
				// remove ability to set selection and add button at bottom to toggle
				
				
				AddButtonZ(AllButtonZ, pCopierMode, "", 2, true, "Top", TableBackgroundID, TableBackgroundID, TableBackgroundID, TableBackgroundID, SILVER40);
				
				
//				if (pTheCopierMode == "Selection")
//				{
//					pTheCopierMode = pCopierMode;
					
//				}
				
						
//				if (pTheCopierMode == "Selection")
//				{
//					AddButtonZ(AllButtonZ, pCopierMode, "", 2, true);	
//				}
//				else
//				{
//					pCopierMode = pTheCopierMode;
					
//				}

				AddButtonZ(AllButtonZ,"All Instruments", "", 2, true, "Top", TableBackgroundID, TableBackgroundID, TableBackgroundID, TableBackgroundID, SILVER40);	

				if (pShowFollowerColumnButtons)
				{
					if (pIsXColumnEnabled) AddButtonZ(AllButtonZ, "Size", "", 2, pIsXEnabled, "Bottom",           DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
					if (pIsCrossColumnEnabled) AddButtonZ(AllButtonZ, "Type", "", 2, pIsCrossEnabled, "Bottom",   DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
					if (pIsATMColumnEnabled) AddButtonZ(AllButtonZ, "Mode", "", 2, pIsATMSelectEnabled, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);	
						
					if (pIsFadeColumnEnabled) AddButtonZ(AllButtonZ, "Fade", "", 2, pIsFadeEnabled, "Bottom",     DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
				}
				else
				{
					pIsXEnabled = pIsXColumnEnabled;
					pIsCrossEnabled = pIsCrossColumnEnabled;
					pIsATMSelectEnabled = pIsATMColumnEnabled;
					pIsFadeEnabled = pIsFadeColumnEnabled;
				}

				if (!pIsXColumnEnabled) pIsXEnabled = false;
				if (!pIsCrossColumnEnabled) pIsCrossEnabled = false;
				if (!pIsATMColumnEnabled) pIsATMSelectEnabled = false;
				if (!pIsFadeColumnEnabled) pIsFadeEnabled = false;
				
				
				if (!pExitShieldFeaturesEnabled)
					pExitShieldIsEnabled = false;
				if (pExitShieldButtonEnabled)
				{
					AddButtonZ(AllButtonZ, "", "spacer", 2, false, "Bottom",                           DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);	
					AddButtonZ(AllButtonZ, "Exit Shield", "", 2, pExitShieldIsEnabled, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);	
				}
				else
				{
					pExitShieldIsEnabled = true;
				}

				AddButtonZ(AllButtonZ, "", "spacer", 2, false, "Top", DODGERBLUE, DODGERBLUE18, DODGERBLUE, DODGERBLUE18, SILVER40);	
				//AddButtonZ(AllButtonZ, "Accounts", "", 2, true);		
					
				//if (pIsRiskFunctionsEnabled)
				AddButtonZ(AllButtonZ, "Lock", "", 2, pIsBuildMode, "Top", pLockButtonOnID, pButtonOffColorID, pLockButtonOnID, pButtonOffColorID, SILVER40);		
					
				//Print("Data Loaded 8");
				//AddButtonZ(AllButtonZ, lockkkk, "", 2, IsBuildMode);			
//				if (pShowAccountsMain == "All")
//				{
//					AddButtonZ(AllButtonZ, "Live Accounts", "", 2, pAccountsLiveEnabled);
//					AddButtonZ(AllButtonZ, "Sim Accounts", "", 2, pAccountsSimEnabled);
//				}
				// hide accounts button
				
				if (pShowAccountsButton)
				if (pShowAccountsMain == "All")
					AddButtonZ(AllButtonZ, "Accounts", "", 2, true, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);	
				//AddButtonZ(AllButtonZ, "Restore", "Restore Hidden Accounts", 2, true);	
				AddButtonZ(AllButtonZ, "Reset", "", 2, true, "Top", TableBackgroundID, TableBackgroundID, TableBackgroundID, TableBackgroundID, SILVER40);
				
				AddButtonZ(AllButtonZ, "Restore", "", 2, true, "Top", TableBackgroundID, TableBackgroundID, TableBackgroundID, TableBackgroundID, SILVER40);	
				
						
				AddButtonZ(AllButtonZ, "", "spacer", 2, false, "Top", DODGERBLUE, DODGERBLUE18, DODGERBLUE, DODGERBLUE18, SILVER40);	
				
				
				
				//Print("Data Loaded 9");
				

				
				
				if (pWindowPrivacyCurrency)
					AddButtonZ(AllButtonZ, pHideCString, "", 30, pHideCurrencyIsEnabled, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
				
				if (pWindowPrivacyAccounts)
					AddButtonZ(AllButtonZ, pHideAString, "", 30, pHideAccountsIsEnabled, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
				
					
				if (!pWindowPrivacyCurrency)
					pHideCurrencyIsEnabled = false;
					
				if (!pWindowPrivacyCurrency)
					pHideAccountsIsEnabled = false;		
				
				
				if (pShowDiscountLinks)
				{
					AddButtonZ(AllButtonZ, APEXButton, "", 2, true, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
					AddButtonZ(AllButtonZ, "", "spacer", 2, false, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
				}
		
				if (pShowCopyRiskButtons)
				{
					AddButtonZ(AllButtonZ, RiskButtonString, "", 2, pIsRiskFunctionsEnabled, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);	
					AddButtonZ(AllButtonZ, CopyButtonString, "", 2, pIsCopyBasicFunctionsEnabled, "Bottom", DODGERBLUE18, SILVER40, DODGERBLUE30, SILVER70, SILVER40);
				}
				else
				{
					pIsCopyBasicFunctionsEnabled = pIsCopyBasicFunctionsChecked;
					pIsRiskFunctionsEnabled = pIsRiskFunctionsChecked;
				}
	
				
						
										//FlattenButton = "FLATTEN ALL";
										
				
				//if (pShowFlattenEverything)
				//FlattenButton = pFlattenButtonString;
				if (pShowFlattenEverything)
				{
					AddButtonZ(AllButtonZ, pFlattenButtonString, "", 2, true, "Top", pFlattenButtonBID, pFlattenButtonBID, pFlattenButtonBID, pFlattenButtonBID, SILVER40);	
				}
				
				if (pShowRefreshPositions)
				{
					AddButtonZ(AllButtonZ, pRPString, "", 2, true, "Bottom", DODGERBLUE, DODGERBLUE18, DODGERBLUE, DODGERBLUE18, SILVER40);	
				}		
				
				
				//\u2713

				//Print("Data Loaded 10");
				
				
//				if (pSupportCode.Contains("accounts"))
//					AddButtonZ(AllButtonZ, "HHHH", "", 2, pIsCopyBasicFunctionsEnabled);
				
					//AddButtonZ(AllButtonZ, ", "", 2, );
				
				
				
				
				// prevent user from turning off both copy and risk functions
				
				if (!pAllowBothFeaturesToBeOff)
				if (!pIsCopyBasicFunctionsChecked && !pIsRiskFunctionsChecked)
				{
					if (pIsCopyBasicFunctionsPermission)
						pIsCopyBasicFunctionsChecked = true;
					else
						pIsRiskFunctionsChecked = true;
					
					
					
				}
					
					
		
					if (!pIsCopyBasicFunctionsChecked)
					{
					
						pIsCopyBasicFunctionsEnabled = false;
					}			
						
						
					if (!pIsRiskFunctionsChecked)
					{
						pIsRiskFunctionsEnabled = false;
					}
				
				
					if (!pIsCopyBasicFunctionsPermission)
					{
						pCopierIsEnabled = false;
						//pIsCopyBasicFunctionsPermission = true;//ben added this to enable the Main button
						//pCopierIsEnabled = false;//ben added this line to enable to Main button
					}
					
			

				
				
					if (ChartControl != null)
					{
						
						//Print("ChartControl_PreviewMouseWheel");
						
//						ChartControl.PreviewMouseWheel -= ChartControl_PreviewMouseWheel;			
//						ChartControl.PreviewKeyDown -= ChartControl_PreviewKeyDown;
//						ChartControl.PreviewKeyUp -= ChartControl_PreviewKeyUp;
//						ChartControl.KeyDown -= ChartControl_KeyDown;
						
//						ChartControl.PreviewMouseWheel -= ChartControl_PreviewMouseWheel;
//						ChartControl.PreviewKeyDown -= ChartControl_PreviewKeyDown;
//						ChartControl.PreviewKeyUp -= ChartControl_PreviewKeyUp;
//						ChartControl.KeyDown -= ChartControl_KeyDown;
						
						//Print("ChartControl_PreviewMouseWheel");
						
					//	ChartControl.keypre
						
						ChartControl.PreviewMouseWheel += ChartControl_PreviewMouseWheel;
						
						ChartControl.PreviewKeyDown += ChartControl_PreviewKeyDown;
						ChartControl.PreviewKeyUp += ChartControl_PreviewKeyUp;
						ChartControl.KeyDown += ChartControl_KeyDown;
							
						
						//ChartControl.MouseWheel += ChartControl_MouseWheel;
						
//						NinjaTrader.Gui.Chart.Chart	cWindow = System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
//						cWindow.KeyDown += chartWindow_KeyDown;
						
		
						
						
						if (timer2 == null)
						{
							ChartControl.Dispatcher.InvokeAsync(() =>
							{
								timer2			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 20), IsEnabled = true };
								timer2.Tick		+= OnTimerTick2;
							});
						}
						
						
					if (timer3 == null)
						{
							ChartControl.Dispatcher.InvokeAsync(() =>
							{
								timer3			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250), IsEnabled = true };
								timer3.Tick		+= OnTimerTick3;
							});
						}
						
							
						
					
					if (timer4 == null)
					{
						ChartControl.Dispatcher.InvokeAsync(() =>
						{
							timer4			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100), IsEnabled = true };
							timer4.Tick		+= OnTimerTick4;
						});
					}
					
					
					
					
					
				}
					
				//Print("Data Loaded 11");
		
				if (this.ChartPanel != null)
                {
					this.ChartPanel.MouseMove += new System.Windows.Input.MouseEventHandler(this.OnMouseMove);
                    this.ChartPanel.MouseDown += new MouseButtonEventHandler(this.OnMouseDown);
					this.ChartPanel.MouseUp += new MouseButtonEventHandler(this.OnMouseUp);
					this.ChartPanel.MouseDoubleClick += new MouseButtonEventHandler(this.OnMouseDoubleClick);
					//this.ChartPanel.DragOver += new DragEventHandler(this.DragOver);
                }

				
				VLS = new Series<string>(this, MaximumBarsLookBack.Infinite);

				
				string FS = TickSize.ToString();
				if(FS.Contains("E-")) {
					FS = FS.Substring(FS.IndexOf("E-")+2);
					PriceDigits = int.Parse(FS);
				}
				else PriceDigits = Math.Max(0,FS.Length-2);
				PriceString = "#0";
				if (PriceDigits > 0)
					PriceString = PriceString + ".";
				for (int i = 1; i <= PriceDigits; i++)
					PriceString = PriceString + "0";
				
				
				//Print("Data Loaded 12");
					
			
			}
			else if (State == State.Historical)
			{
				Calculate					= Calculate.OnBarClose;
				SetZOrder(-1);
			}			
			else if (State == State.Terminated)
			{
				//#region -- terminated --
				if (ChartControl == null)
					return;
				
				
				
				
                if (this.ChartPanel != null)
                {
                    this.ChartPanel.MouseMove -= new System.Windows.Input.MouseEventHandler(this.OnMouseMove);
                    this.ChartPanel.MouseDown -= new MouseButtonEventHandler(this.OnMouseDown);
					this.ChartPanel.MouseUp -= new MouseButtonEventHandler(this.OnMouseUp);
					this.ChartPanel.MouseDoubleClick -= new MouseButtonEventHandler(this.OnMouseDoubleClick);
					//this.ChartPanel.DragOver -= DragOver;
                }
				
					
				
					if (CurrentMasterAccount != null)
					{
						
						//Print("terminate subs");
						
								CurrentMasterAccount.AccountItemUpdate -= OnAccountItemUpdate3;
								CurrentMasterAccount.ExecutionUpdate -= OnExecutionUpdate3;
								CurrentMasterAccount.OrderUpdate -= OnOrderUpdate3;
								CurrentMasterAccount.PositionUpdate -= OnPositionUpdate3;
						
					}

								
					
					
				if (ChartControl != null)
				{
					
					ChartControl.PreviewMouseWheel -= ChartControl_PreviewMouseWheel;
			
					ChartControl.PreviewKeyDown -= ChartControl_PreviewKeyDown;
					ChartControl.PreviewKeyUp -= ChartControl_PreviewKeyUp;
					ChartControl.KeyDown -= ChartControl_KeyDown;
					
					//ChartControl.MouseWheel -= ChartControl_MouseWheel;

					if (timer2 != null)
					{
						
						timer2.IsEnabled = false;
						timer2 = null;
					}
					
					
					if (timer3 != null)
					{
						
						timer3.IsEnabled = false;
						timer3 = null;
					}
					
					
					if (timer4 != null)
					{
						
						timer4.IsEnabled = false;
						timer4 = null;
					}
					
					
					
					
					
				}
				
				//#endregion				
			}

			//Print("1");
			
			//if (pUseToolbarButtons)
			if (State == State.Realtime)
			{
				//Call the custom method in State.Historical or State.Realtime to ensure it is only done when applied to a chart not when loaded in the Indicators window				
				
				//if (Permission)
				if (ChartControl != null && !IsToolBarButtonAdded)
				{
				    ChartControl.Dispatcher.InvokeAsync((Action)(() => // Use this.Dispatcher to ensure code is executed on the proper thread
				    {
						AddButtonToToolbar();
					}));
				}
			}
			
			//Print("2");
			
			
			if (State == State.Terminated)
			{
				//if (Permission)
				if (chartWindow != null)
				{
			        ChartControl.Dispatcher.InvokeAsync((Action)(() => //Dispatcher used to Assure Executed on UI Thread
			        {	
						DisposeCleanUp();
					}));
					
					//chartWindow.Caption = "Chart";
				}
				
			}
			
			//#endregion	
		}

	
		
		private bool FirstRun = true;

	
//		private void GetAllChartIndicators()

//		{

//			Print(" START SCAN --------------------------------------------------------------------------------------------------");
					
//			int totaldone = 0;
			
			
//			foreach (Window window in Globals.AllWindows.ToList())
//			{
				
				
				
//				//Window window = kvp.Value;
				
//				//check if the found window is a Chart window, if not continue looking
//				if (!(window is NinjaTrader.Gui.Chart.Chart)) continue;

//				window.Dispatcher.BeginInvoke(new Action(() =>
//				{
//					//try to cast as a Chart, if it fails it will be null
//					var foundChart = window as NinjaTrader.Gui.Chart.Chart; 
//					if (foundChart == null) return;	

////						string windowuid = foundChart.Uid.ToString();
////						Print("windowuid: " + windowuid);
					
					
//					foreach (TabItem tab in foundChart.MainTabControl.Items)
//					{
//						var foundChartTab = tab.Content as NinjaTrader.Gui.Chart.ChartTab;
						
//						if (foundChartTab == null) return;
						
							
//						foreach (IndicatorRenderBase ii in foundChartTab.ChartControl.Indicators.ToList())
//	                        {


//								string indyname = ii.Name;
								
//								//ii.Dispatcher.InvokeAsync(() =>
////EMA	iEMA = ii as EMA; 
											
								
//								//if (iEMA == null) return;
											
						
//									VeritasBOT2	iVeritasBOT2 = ii as VeritasBOT2; 
											
								
//								if (iVeritasBOT2 == null) return;
								
//									//ii.Dispatcher.BeginInvoke(new Action(() =>
//									//		{
								
									
//								            string instrumentnow = foundChartTab.ChartControl.PrimaryBars.Bars.Instrument.FullName;
//								            string datanow = foundChartTab.ChartControl.PrimaryBars.Bars.BarsPeriod.ToString();		
//												string emaperiod = indyname;
//											//	emaperiod = 	iEMA.Period.ToString();
												
//											totaldone = totaldone + 1;
													
//										//Print(totaldone.ToString() + " " + Z.Instrument + "  " + Z.Name + " ");		
//										Print(totaldone.ToString() + " " + instrumentnow + "  " + datanow + " - " + emaperiod);
								
						
//						//	}));
						
//						}
				
						
//					}
	
					
//				}));
				
				
//			}
			

//			Print(totaldone + " TOTAL INDICATORS ACROSS ALL CHARTS --------------------------------------------");
			
//		}
		
		
		
		
		
		public Brush GetTextColor(Brush bg2)
		{
			//Color bg = new Pen(bg2,1).;
			Color bg = (bg2 as SolidColorBrush).Color;
			
			if (myProperties == null)
				return Brushes.Black; 
			
			
				if (bg2.ToString() == "#00FFFFFF")
				
				//if (buttonGrid.Background == Brushes.Transparent)
				{
						string skinname = NinjaTrader.Core.Globals.GeneralOptions.Skin.ToString();
						
						//Print(skinname);
					
						if (skinname == "NinjaTrader Dark" || skinname == "Dark" || skinname == "Slate Dark" || skinname == "Slate Gray")
						{
							return Brushes.Silver;
						}
						else
						{
							return Brushes.Black;
						}
						
						
				}
				else
				{
					
						double a = 1 - ( 0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B)/255;
			            if (a < 0.5)
			               return Brushes.Black;
			            else
			               return Brushes.Silver;
					
				}
				
				//ChartTextBrushDX = Brushes.Bla

//		    int nThreshold = 150;
//		    int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + 
//		                                  (bg.B * 0.114));

//		    Brush foreColor = (255 - bgDelta < nThreshold) ? Brushes.Black : Brushes.White;
//		    return foreColor;
		}

		private void TurnBOTOnOff(PNLStatistics bbbb, string Mode, bool AllBots, bool Switch)
		{

			
		
				if (Mode == "Close")
									{
										
										if (bbbb.Pos != null)
										{
											bbbb.Pos.Close("Close Position");
										}
										else
										{
										
											foreach (Position PP in bbbb.Acct.Positions)
											{
												
												PP.Close("Close Position");
												
												
												
											}
											
											
										}
									
										
										
										//bbbb..ClosePosition();
									
									}
			
			ChartControl.Dispatcher.InvokeAsync(new Action(() =>
									{
										ChartControl.InvalidateVisual();
									
									}));
			
			
		}

		private string AllPriceMarker (Instrument iii, double price)
		{
			
		//	Instrument ii = NinjaScriptBase.get_Instrument();
			
			if (iii == null)
				return price.ToString();
			
			double ThisTickSize = iii.MasterInstrument.TickSize;
			
			double trunc = Math.Truncate(price);
			int fraction = 0;
			string priceMarker = "";
			
			
			if (price == 0 || price == 1 || price == -1)
			{
				
				return price.ToString();
			}
			else if (ThisTickSize == 0.03125) 
			{
				fraction = Convert.ToInt32(32 * Math.Abs(price - trunc));	
				if (fraction < 10)
					priceMarker = trunc.ToString() + "'0" + fraction.ToString();
				else if(fraction == 32)
				{	
					trunc = trunc + 1;
					fraction = 0;
					priceMarker = trunc.ToString() + "'0" + fraction.ToString();
				}	
				else 
					priceMarker = trunc.ToString() + "'" + fraction.ToString();
			}
			else if (ThisTickSize == 0.015625)
			{
				fraction = 5 * Convert.ToInt32(64 * Math.Abs(price - trunc));	
				if (fraction < 10)
					priceMarker = trunc.ToString() + "'00" + fraction.ToString();
				else if (fraction < 100)
					priceMarker = trunc.ToString() + "'0" + fraction.ToString();
				else if(fraction == 320)
				{	
					trunc = trunc + 1;
					fraction = 0;
					priceMarker = trunc.ToString() + "'00" + fraction.ToString();
				}	
				else	
					priceMarker = trunc.ToString() + "'" + fraction.ToString();
			}
			else if (ThisTickSize == 0.0078125)
			{
				fraction = Convert.ToInt32(Math.Truncate(2.5 * Convert.ToInt32(128 * Math.Abs(price - trunc))));	
				if (fraction < 10)
					priceMarker = trunc.ToString() + "'00" + fraction.ToString();
				else if (fraction < 100)
					priceMarker = trunc.ToString() + "'0" + fraction.ToString();
				else if(fraction == 320)
				{	
					trunc = trunc + 1;
					fraction = 0;
					priceMarker = trunc.ToString() + "'00" + fraction.ToString();
				}	
				else	
					priceMarker = trunc.ToString() + "'" + fraction.ToString();
			}
			else
			{
				priceMarker = price.ToString(NinjaTrader.Core.Globals.GetTickFormatString(ThisTickSize));
			}
			return priceMarker;
		}		
		
		private string GetDollarStringZero(string Sign, double Price)
		{
				
//					bool isInt = Price == (int)Price;
		
			
			
			bool IsNeg = Price < 0;
			
			double Price2 = Math.Abs(Price);
			
	
			
			string pr = Price2.ToString("N0");
			
//					if (isInt)
//					{
//						return Sign + pr.Substring(0, pr.Length-3);
//					}
//					else
			
			pr = Sign + pr;
			
			if (IsNeg)
				pr = "(" + pr + ")";
				//pr = "-" + pr;
				
						return pr;
						
        	
			
		}

		private string GetDollarStringHide(string Sign, double Price)
		{
				
//					bool isInt = Price == (int)Price;
		
			bool dohide = false;
			
			if (pHideCurrencyIsEnabled)
			if (pCurrencyPrivacy == "Everything")
				dohide = true;
			
			
			bool IsNeg = Price < 0;
			
			if (IsNeg && dohide)
			{
				IsNeg = false;
				Price = 0;
			}
			
			
			double Price2 = Math.Abs(Price);
			
	
			
			string pr = Price2.ToString("N2");
			
//					if (isInt)
//					{
//						return Sign + pr.Substring(0, pr.Length-3);
//					}
//					else
			
			pr = Sign + pr;
			
			if (IsNeg)
				pr = "(" + pr + ")";
				//pr = "-" + pr;
				
			
			if (dohide)
				pr = Regex.Replace(pr, "[0-9]", "–");
				
				
			
						return pr;
						
        	
			
		}

		private string GetDollarString(string Sign, double Price)
		{
				
//					bool isInt = Price == (int)Price;
		
			
			
			bool IsNeg = Price < 0;
			
			double Price2 = Math.Abs(Price);
			
	
			
			string pr = Price2.ToString("N2");
			
//					if (isInt)
//					{
//						return Sign + pr.Substring(0, pr.Length-3);
//					}
//					else
			
			pr = Sign + pr;
			
			if (IsNeg)
				pr = "(" + pr + ")";
				//pr = "-" + pr;
				
			
		
			
						return pr;
						
        	
			
		}

		private DateTime MoveOClicked = DateTime.MinValue;

		private string RemoveEndOfLiveATM(string atmfulldisplayname)
			{
				
								string input = atmfulldisplayname;
								int index = input.LastIndexOf("-"); // Character to remove "?"
								if (index > 0)
								    input = input.Substring(0, index); // This will remove all text after character ?

								//input = input.Substring(input.Length-2);

								input = input.Remove(input.Length-1);
								
								return input;				
				
			}		

		private Order OrderExists (Account ThisAccount, string ordertype, int qty, OrderAction oa)
		{
			
			
		
						foreach (Order or in ThisAccount.Orders.ToList())
								{
									if (or.OrderType != OrderType.Market && or.OrderState == OrderState.Working)
									{
										
										if (or.Quantity == qty && or.OrderAction == oa)
										{
											
										
											if (ordertype == "Limit")
												if (or.OrderType == OrderType.Limit)
													return or;
												
												if (ordertype == "Stop")
												if (or.OrderType == OrderType.StopMarket || or.OrderType == OrderType.StopLimit)
													return or;										
											
											
										}
										
										
										
									}
									
									
									
									
								}
				
			
								return null;
		
		}

		private void CancelAndReplaceOrder(Order or, string ocon, string name)
		{	
			Account aaaaaaa = or.Account;
			
			if (or.OrderType == OrderType.StopMarket)
				name = name.Replace("Target", "Stop");
			
			
			NewOrder = aaaaaaa.CreateOrder(or.Instrument, or.OrderAction, or.OrderType, or.TimeInForce, or.Quantity, or.LimitPrice, or.StopPrice, ocon, name, null);
			
			
			
			
			
			aaaaaaa.Cancel(new[] { or });		
			
			aaaaaaa.Submit(new[] { NewOrder });
			
			//Print("59 Submit 1");
			
		}

		private void PlaceTradingViewStopOrder(Order or, double StopPrice, string ocon, string name)
		{	
			Account aaaaaaa = or.Account;
			
			NewOrder = aaaaaaa.CreateOrder(or.Instrument, or.OrderAction, OrderType.StopMarket, or.TimeInForce, or.Quantity, 0, StopPrice, ocon, name, null);
			
			
			
			
			
			//aaaaaaa.Cancel(new[] { or });		
			
			aaaaaaa.Submit(new[] { NewOrder });
			
			//Print("59 Submit 2");
		}

		private void CancelOrdersAtPrice (double price)
		{
			
			
				foreach (Account acct in Account.All)
				{
						foreach (Order or in acct.Orders.ToList())
								{
									if (or.OrderType != OrderType.Market)
									{
										double orderprice = or.LimitPrice;
										
										if (or.StopPrice != 0)
											orderprice = or.StopPrice;
										
										if (orderprice == price)
										{
											acct.Cancel(new[] { or });
											
											
										}
									}
									
									
									
									
								}
				}
			
							
		} 
		
		private void CancelAllTargets (Account aaaaaaa, int totlq)
		{
			
			int cccqqq = 0;
			
				foreach (Account acct in Account.All)
				{
					if (acct == aaaaaaa)
						foreach (Order or in aaaaaaa.Orders.ToList())
								{
									if (or.OrderType != OrderType.Market)
									{
										
										OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.TriggerPending;
										
										
										
										//OrderNameOK = IsPendingEntryOrder(or);
										//OrderNameOK = true;
										
										OrderNameOK = or.Name.Contains("Target");
										
										if (OrderStateOK && OrderNameOK)
										{
											//FlattenEverythingClickedOrders++;;
											
											if (cccqqq < totlq)
												acct.Cancel(new[] { or });	
											
											cccqqq = cccqqq + or.Quantity;
										}
									}
									
								}
				}
			
							
		}
		
		private void CancelAllOrders (Account aaaaaaa)
		{
			
			
				foreach (Account acct in Account.All)
				{
					
					if (acct == aaaaaaa)
						foreach (Order or in aaaaaaa.Orders.ToList())
								{
									if (or.OrderType != OrderType.Market)
									{
										
										OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.TriggerPending;
										
										
										
										//OrderNameOK = IsPendingEntryOrder(or);
										//OrderNameOK = true;
										
										OrderNameOK = !or.Name.Contains("Target");
										
										if (OrderStateOK && OrderNameOK)
										{
											FlattenEverythingClickedOrders++;;
											acct.Cancel(new[] { or });		
										}
									}
									
								}
				}
			
							
		}
		
		private void ResetOneAccount(string accccnnnn)
		{
			
							
							if (pThisMasterAccount == accccnnnn)
							{
								
								// Print("REMOVE master accountt");
								
								
								DisableMasterAccount();
								
								
								
								
								//Print(pThisMasterAccount);
								
							}
							
							
							//Print("AllDuplicateAccounts.Remove(a");
							
							AllDuplicateAccounts.Remove(accccnnnn);
							AllHideAccounts.Remove(accccnnnn);
							
							
							//Print(accccnnnn);
							
							SetAccountData("5",accccnnnn,"None","","","","","","","");
			
							//Print(accccnnnn);
			
		}
		
		
		int NotAccountRows = 0;
		int TotalAccounts = 0;
		int TotalPositions = 0;
		int TotalPendingOrders = 0;
		int TotalStrategies = 0;
		int TotalPositionsPrev = 0;
		private DateTime PositionsZeroStartTime = DateTime.MinValue;
		
		int TotalDisplayAccounts = 0;
		int TotalFundedAccounts = 0;
		int MaximumAccountRows = 0;

		private void SetWhichAccounts()
		{

			if (pAccountsLiveEnabled && pAccountsSimEnabled)
				pShowAccounts = "All";
			
			else if (pAccountsLiveEnabled)
				pShowAccounts = "Live";
			
			else if (pAccountsSimEnabled)
				pShowAccounts = "Sim";			
			
		}

		bool NoAccountsConnected = true;
		bool NoAccountsDisconnected = true;
		
		List<string> AllConnectedAccounts = new List<string>();
		List<string> AllDisconnectedAccounts = new List<string>();
		
//		SortedDictionary<double, int> LimitPriceToTotalOrders = new SortedDictionary<double, int>();
			
//		SortedDictionary<double, DateTime> LimitPriceToTimeOutOfSync = new SortedDictionary<double, DateTime>();
		
		
		
		
		//private List<double> AllMessages = new List<MessageAlerts>(); 
		
		SortedDictionary<string, PendingDetails> RejectedAccounts = new SortedDictionary<string, PendingDetails>();	
		
		SortedDictionary<double, PendingDetails> LimitPriceToTotalOrders2 = new SortedDictionary<double, PendingDetails>();	
		
		SortedDictionary<double, PendingDetails> LimitPriceToTimeOutOfSync2 = new SortedDictionary<double, PendingDetails>();
		
		
		SortedDictionary<int, SortedDictionary<Account, List<double>>> TargetNumberToTotalOrders2 = new SortedDictionary<int, SortedDictionary<Account, List<double>>>();		
			
				
		//#region -- PendingDetails class --
        public class PendingDetails
        {
   
            Instrument iName;
            double iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRectFill;
			SharpDX.RectangleF iRectPlus;
			SharpDX.RectangleF iRectCancel;
			
			List<Instrument> iAllInstruments = new List<Instrument>();
				
            bool iHovered;
			int iDirection;
			
			DateTime iStartTime;
			DateTime iExpireTime;

			List<string> iAllOrderNames = new List<string>();
			List<string> iAllOrderTypes = new List<string>();
			
			string iLatestAction;
			
			
			
		
            public Instrument Name { get { return iName; } set { iName = value; } }
            public double Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF RectFill { get { return iRectFill; } set { iRectFill = value; } }
			public SharpDX.RectangleF RectPlus { get { return iRectPlus; } set { iRectPlus = value; } }
			public SharpDX.RectangleF RectCancel { get { return iRectCancel; } set { iRectCancel = value; } }
			
			public List<Instrument> AllInstruments { get { return iAllInstruments; } set { iAllInstruments = value; } }
			
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }
			public int Direction { get { return iDirection; } set { iDirection = value; } }
			
			
            public DateTime StartTime { get { return iStartTime; } set { iStartTime = value; } }
			public DateTime ExpireTime { get { return iExpireTime; } set { iExpireTime = value; } }
			
			public List<string> AllOrderNames { get { return iAllOrderNames; } set { iAllOrderNames = value; } }
			
			
			public List<string> AllOrderTypes { get { return iAllOrderTypes; } set { iAllOrderTypes = value; } }
			
			
			public string LatestAction { get { return iLatestAction; } set { iLatestAction = value; } }
			
			
			
			
			
        }
		//#endregion

		private bool FlattenEverythingClicked = false;
		private int FlattenEverythingClickedTotal = 0;
		private int FlattenEverythingClickedOrders = 0;
		private DateTime FlattenEverythingClickedTime = DateTime.MinValue;
		
		private bool RefreshPositionsClicked = false;
		private int RefreshPositionsClickedTotal = 0;
		private int RefreshPositionsClickedOrders = 0;
		private DateTime RefreshPositionsClickedTime = DateTime.MinValue;		
		
		
		
		private int TotalActions = 0;
		private int TotalFroze = 0;
		private int TotalFrozeSim = 0;
		private int TotalFrozeLive = 0;
		
		AtmStrategy MasterATM = null;
		private int LastMasterQty = 0;
		private double LastStopPrice = 0;
		private double LastLimitPrice = 0;
		private OrderType LastOrderType = 0;
		
				
		private bool GetOrdersbyPrice = true;
		
		//#region -- PrivateAccountName --
		private string PrivateAccountName(string aaaaa)
				{
					
					string thisrowacct2 = aaaaa;
					
						if (pPrivacyEnabled)
						{

							pAccountPrivacy1 = pAccountPrivacy1.Replace(" ", "");
							if (pAccountPrivacy1 != string.Empty)
							thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy1, pAccountReplace1);
							
							pAccountPrivacy2 = pAccountPrivacy2.Replace(" ", "");
							if (pAccountPrivacy2 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy2, pAccountReplace2);
							
							pAccountPrivacy3 = pAccountPrivacy3.Replace(" ", "");
							if (pAccountPrivacy3 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy3, pAccountReplace3);
							
							pAccountPrivacy4 = pAccountPrivacy4.Replace(" ", "");
							if (pAccountPrivacy4 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy4, pAccountReplace4);
							
							pAccountPrivacy5 = pAccountPrivacy5.Replace(" ", "");
							if (pAccountPrivacy5 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy5, pAccountReplace5);
							
							pAccountPrivacy6 = pAccountPrivacy6.Replace(" ", "");
							if (pAccountPrivacy6 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy6, pAccountReplace6);
							
							pAccountPrivacy7 = pAccountPrivacy7.Replace(" ", "");
							if (pAccountPrivacy7 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy7, pAccountReplace7);
							
							pAccountPrivacy8 = pAccountPrivacy8.Replace(" ", "");
							if (pAccountPrivacy8 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy8, pAccountReplace8);	
							
							pAccountPrivacy9 = pAccountPrivacy9.Replace(" ", "");
							if (pAccountPrivacy9 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy9, pAccountReplace9);
							
							pAccountPrivacy10 = pAccountPrivacy10.Replace(" ", "");
							if (pAccountPrivacy10 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy10, pAccountReplace10);
							
							pAccountPrivacy11 = pAccountPrivacy11.Replace(" ", "");
							if (pAccountPrivacy11 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy11, pAccountReplace11);
							
							pAccountPrivacy12 = pAccountPrivacy12.Replace(" ", "");
							if (pAccountPrivacy12 != string.Empty)
								thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy12, pAccountReplace12);							
							
							
							
							
							
							thisrowacct2 = thisrowacct2.Replace("--", "-");	
						}					
					
						return thisrowacct2;
					
				}
		//#endregion

				// .Orders.ToLis
		private bool SelectAllAccounts = false;
				
					
		//#region -- GetAllPerformanceStats --
		private void GetAllPerformanceStats()		
		{
			
			int number = 0;
			
//			Print(" ---------------------- ");
			
			
				
//										try
//										{
											
										
//			foreach (Account acct in Account.All)
//				{
//					//if (acct == aaaaaaa)
//						foreach (Order or in acct.Orders.ToList())
//								{
//									if (or.OrderType == OrderType.Market)
//									{
										
//										OrderStateOK = or.OrderState == OrderState.Filled;
										
								
										
//										//OrderNameOK = IsPendingEntryOrder(or);
//										OrderTypeOK = or.Time.Ticks > DateTime.Now.AddSeconds(-10).Ticks;
										
//										OrderNameOK = or.Name.Contains("Target");
//										OrderNameOK = true;
										
										
//										if (OrderStateOK && OrderNameOK && OrderTypeOK)
//										{
//											string timestamp = or.Time.ToString("yyyy-MM-dd HH:mm:ss.fff",
//                                            CultureInfo.InvariantCulture);
											
												
//											ATMObject = 	or.GetOwnerStrategy();	
											
//											NewATM = null;
											
//											if (ATMObject != null)
//											if (ATMObject.GetType() == typeof(AtmStrategy))
//												NewATM = (AtmStrategy) ATMObject;	
											
//											if (NewATM != null)
//											Print(acct.Name + "  " + NewATM.Id + "  " + timestamp);
											
//											number++;
//										}
//									}
									
//								}
//				}
			
												
//										}
//										catch (Exception ex)
//										{
										
//										}
										
				
	
			//
			
//			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
//                                            CultureInfo.InvariantCulture);
			
//			Print("GetAllPerformanceStats: " + timestamp);
			
			//return;
			
			 
			
			if (SelectAllAccounts)
				AllDuplicateAccounts.Clear();
			
			//AtmStrategy.All
			
			

			//Print(Strategy.All.Count);
			
//			int fsgfdgf = 0;
			
//			foreach (Object strat in Strategy.All)
//			{
					
//				AtmStrategy FinalATMStrategy2 = (AtmStrategy) strat;	
		
//				bool AllFlat = true;
//				bool TimeQ = true;
			
//				fsgfdgf++;
				
//				if (FinalATMStrategy2 != null)
//				{
					
//					Print(fsgfdgf.ToString() + ":  "  + FinalATMStrategy2.DisplayName);
//				}
								
				
				
//			}
				
				
//			foreach (Strategy strat in Strategy.All)
//			{
			
					
//				AtmStrategy FinalATMStrategy2 = null;
										
	
//				FinalATMStrategy2 = (AtmStrategy) strat;	
		
//				bool AllFlat = true;
//				bool TimeQ = true;
			
//				if (FinalATMStrategy2 != null)
//				{
					
//					Print(FinalATMStrategy2.DisplayName);
//				}
								
								
											
				
				
				
//			}
					
						
						
						
			 	if (pMultiplierMode == "Quantity" && pCopierMode == "Executions")
				{
					pCopierMode = "Orders";	

					//btnDrawObjs.Content = pCopierMode;
					
					
					if (pTheCopierMode != "Selection")
						pTheCopierMode = "Orders";	
					
								AddMessage("The 'Size' column cannot be used in 'Quantity' mode when Duplicate Account Actions is running in 'Executions' mode.  Duplicate Account Actions has been switched to 'Orders' mode.", 30, RED);
								
				}
							
							
							
			AllConnectedAccounts.Clear();
			AllDisconnectedAccounts.Clear();
			
			NoAccountsConnected = true;
			NoAccountsDisconnected = true;
			
			
			//Print("getall");
		
			
			
			double ThisTickSize = 0;
			double ThisTickDollars = 0;

			string pAccountName = "";
			

			
			
			AllPNLStatistics.Clear();
			
			
			
			FinalPNLStatistics.Clear();
			TotalAccountPNLStatistics.Clear();
			
			TotalActions = 0;
			TotalFroze = 0;
			TotalFrozeSim = 0;
			TotalFrozeLive = 0;		
				
			RefreshPositionsClickedTotal = 0;
			
			TotalAccounts = 0;
			TotalDisplayAccounts	 = 0;	
			TotalFundedAccounts	 = 0;	
		//	Print("----------------------");
			
			TotalPositions = 0;
			TotalPendingOrders = 0;
			
			List<string> LoopedAccounts = new List<string>();
			
			//bool One2222Done = false;
			
				
			AllTradingViewTargetPrices.Clear();
			AllTradingViewStopPrices.Clear();
		
				
				
			foreach (Account acct in Account.All)
			{
				
					
				if (AllConnectedAccounts.Contains(acct.Name))
					continue;
				
				
				
				if (acct.ConnectionStatus == ConnectionStatus.Connected)
					AllConnectedAccounts.Add(acct.Name);			
				
				if (acct.ConnectionStatus == ConnectionStatus.Disconnected)
					AllDisconnectedAccounts.Add(acct.Name);	
				
				
				
				
					foreach (Order or in acct.Orders.ToList())
					{
						
						//bool istvtarget = or.Oco.Contains(pTVOCOPrefix) && or.Account.GetPosition(or.Instrument.Id).MarketPosition == MarketPosition.Flat;
						bool istvtarget = or.Oco.Contains(pTVOCOPrefix);
						//bool istvtarget = or.Oco.Contains(pTVOCOPrefix);
						
						if (or.OrderType == OrderType.StopMarket && or.OrderState == OrderState.Working && istvtarget)
						{
							
							
							AllTradingViewStopPrices.Add(or.StopPrice);
						}
						
						if (or.OrderType == OrderType.Limit && or.OrderState == OrderState.Working && istvtarget)
						{
							
							
							AllTradingViewTargetPrices.Add(or.LimitPrice);
						}

					}

			}

			//LimitPriceToTotalOrders.Clear();
			GetOrdersbyPrice = false;
			if (DateTime.Now > MoveOClicked)
			{
				GetOrdersbyPrice	= true;
				
			}
			
			if (GetOrdersbyPrice)
			LimitPriceToTotalOrders2.Clear();
			
			TargetNumberToTotalOrders2.Clear();
		
						
			AtmStrategy PendingATM = null;
			
			
			// all master account orders
			
			MasterATM = null;
			LastMasterQty = 0;
			AllMasterAccountOrders.Clear();
			
//			bool dooooo = false;
			
			
//			if (dooooo)
			
			if (CurrentMasterAccount != null)
			{
				
//				Print(CurrentMasterAccount.Strategies.Count);
				
//					foreach (object aaa in CurrentMasterAccount.Strategies)
//					{								
//						//if (or.GetOwnerStrategy() == aaa)
						
//							AtmStrategy FinalATMStrategy = (AtmStrategy) aaa;	
							
//							if (FinalATMStrategy != null)
//							{
							
//								Print(FinalATMStrategy.DisplayName);
							
//							}
						
//					}			
									
													
				
					foreach (Order or in CurrentMasterAccount.Orders.ToList())
					{
						
						bool OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.TriggerPending
							|| or.OrderState == OrderState.ChangePending || or.OrderState == OrderState.ChangeSubmitted || or.OrderState == OrderState.PartFilled;
						
						if (or.OrderType != OrderType.Market)
						if (OrderStateOK)
						{
							if (IsPendingEntryOrder(or))
							{
								AllMasterAccountOrders.Add(or);
			
							}
						}

					}

			}

			RemoveATMCheck.Clear();
			
			
	
			//Print("543 Process 1");
			
			foreach (Account acct in Account.All)
			{
				
				
				RefreshPositionsClickedOrders = 0;
				
				
				//double dasfsdg = acct.Get(AccountItem.AutoLiquidatePeakBalance, Currency.UsDollar);
			
//				 foreach (var val in Enum.GetValues<AccountItem>())
//		        {
//		            double dasfsdg = acct.Get(val, Currency.UsDollar);
//		        }
		
//				foreach (AccountItem aaaaa in AccountItem)
//				{
//					double dasfsdg = acct.Get(aaaaa, Currency.UsDollar);		
//				}
				
			//	Print("AccountStatus  " + acct.AccountStatus);
				
				//acct.AccountStatus = AccountStatus.Enabled;
				
				string accccnnnn = acct.Name;
				
				
				
//				if (accccnnnn == "330690" && !One2222Done)
//				{
//					One2222Done = true;
					
//					continue;
					
					
//				}
	
					//Print(accccnnnn);
				
			

				
				
				bool AccountIsNowConnected = AllConnectedAccounts.Contains(accccnnnn);
							
				
				if (AccountIsNowConnected && acct.ConnectionStatus == ConnectionStatus.Disconnected)
				{
					//AllDuplicateAccounts.Remove(acct.Name);
					//RemoveAccountData(acct.Name);
					
					continue;
				}
				
				
				// FIXED ISSUE WITH NINJATRADER CASH ACCOUNTS BEING DISCONNECTED
				
				if (!AccountIsNowConnected)
				{
				
					if (LoopedAccounts.Contains(accccnnnn))
						continue;
					
					LoopedAccounts.Add(accccnnnn);
					
				}
					
				
				//AccountIsNowConnected = acct.ConnectionStatus == ConnectionStatus.Connected;			
				
				
				if (AllHideAccounts.Contains(accccnnnn))
					continue;
				
				if (accccnnnn == "Playback101")
					continue;
				
				if (accccnnnn == "Backtest")
					continue;		
				
						
				if (pAccountFilter != string.Empty && !accccnnnn.Contains(pAccountFilter))
				{
					
					//Print(FirstLoadAccounts + " " + accccnnnn);
					
					if (FirstLoadAccounts)
						ResetOneAccount(accccnnnn);

					continue;
				
				}				
				
				
				
				if (AccountIsNowConnected)
					NoAccountsConnected = false;
				
				//Print(acct.Name + " " + AccountHasBeenSaved(acct.Name));
				
				if (!ForceAllAccountsToBeListed)
				{
				
					if (pAllowDisconnectedAccounts)
						if (!AccountIsNowConnected)
						if (!AccountHasBeenSaved(accccnnnn))
							continue;
						
						if (!pAllowDisconnectedAccounts)
						if (!AccountIsNowConnected)
							continue;				
						
				}
						
				
					
				if (!pIsBuildMode && pHideExtraAccountsOnLock)
				{
					
					bool IsSelectedAccount = false;
					
					if (pThisMasterAccount == accccnnnn)
						IsSelectedAccount = true;
					
					if (AllDuplicateAccounts.Contains(accccnnnn))
						IsSelectedAccount = true;				
					
					if (!IsSelectedAccount)
						continue;
				}
				
				
				if (pShowAccounts == "All")
				{
					
				}
				else if (pShowAccounts == "Live")
				{
					if (acct.Provider == Provider.Simulator)
					{
						if (FirstLoadAccounts)
							ResetOneAccount(accccnnnn);

						continue;
					
					}
				}					
				else if (pShowAccounts == "Sim")
				{
					if (acct.Provider != Provider.Simulator)
					{
						if (FirstLoadAccounts)
							ResetOneAccount(accccnnnn);

						continue;
					
					}
				}				
							
		
		
				
				
				if (pThisMasterAccount != accccnnnn)
				if (SelectAllAccounts)
				{
				
					AllDuplicateAccounts.Add(accccnnnn);
					SetAccountData("",accccnnnn,"Slave","","","","","","","");
					
					
				}
				
				
				if (pThisMasterAccount != accccnnnn)
					TotalAccounts = TotalAccounts + 1;			
				
				TotalDisplayAccounts = TotalDisplayAccounts + 1;
				
				bool IsConnected = acct.ConnectionStatus == ConnectionStatus.Connected;
				
				
				
				IsConnected = AccountIsNowConnected;
				
				
	
								PNLStatistics Z = new PNLStatistics();
								Z.Inst = Instrument;
								Z.LastEntry = 1;
								Z.LastDir = 1;
								Z.TotalLongPrices = 0;
								Z.TotalLongQty = 0;
								Z.TotalShortPrices = 0;
								Z.TotalShortQty = 0;
								Z.LastPosition = 0;
		
				
				Z.PositionLong = 0;
				Z.PositionShort = 0;
				Z.PositionMicroLong = 0;
				Z.PositionMicroShort = 0;				
				
				Z.Pos = null;
				
				Z.AllPositions = new List<Position>();
				
				Z.PendingEntry = 0;
				Z.PendingExit = 0;	
				
				
				
				
				if (pShowRefreshPositions)
					foreach (Order or in acct.Orders.ToList())
					{
						
							if (or.OrderState == OrderState.Working)
							if (or.Name.Contains(RefreshOrderName))
							{
								acct.Cancel(new[] { or });
							}
							
					}				

//				bool testttttt = false;
					
//					if (testttttt)
					
	
				// to show pending orders as listed even when it is in 'Executions' mode	
					
				if (pCopierMode == "Executions")
				{
					
					
					
						
										
					foreach (Order or in acct.Orders.ToList())
					{
						
						bool OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.TriggerPending
							|| or.OrderState == OrderState.ChangePending || or.OrderState == OrderState.ChangeSubmitted || or.OrderState == OrderState.PartFilled;
						
						
						
						bool ormaster = acct == CurrentMasterAccount && pResubmitMaster;
		
						
						
						
						
						if (or.OrderType != OrderType.Market)
						if (OrderStateOK)
						{
							if (IsPendingEntryOrder(or))
							{
								
								bool isok = true;
								
								if (AllTradingViewStopPrices.Count > 0 || AllTradingViewTargetPrices.Count > 0)
								if (CurrentMasterAccount == acct)
								{
									
									
									if (or.OrderType == OrderType.StopMarket)
									if (AllTradingViewStopPrices.Contains(or.StopPrice))
										isok = false;
									
									if (or.OrderType == OrderType.Limit)
									if (AllTradingViewTargetPrices.Contains(or.LimitPrice))
										isok = false;
									
									if (acct.GetPosition(or.Instrument.Id) == null)
										isok = true;
								}
									
								if (isok)
								{
									Z.PendingEntry = Z.PendingEntry + or.Quantity;
								
									TotalPendingOrders++;
								}
								
								
								
								
								
								
					
										
											
							}
							
					
						}
						
				
					}
		
					
					
				}
				
				
//				bool dothissection = false;
						
//				if (dothissection)
				
				
				//if (pCopierMode == "Orders")
				{
					
					
					
						
										//Print("FilledMasterAccountATMs.Count: " + FilledMasterAccountATMs.Count);
						
										// if order doesn't get atm strategy placed
			
					
					
						bool isordersm = pCopierMode == "Orders";
						if (pIsATMSelectEnabled)
						{
							isordersm = true;
							
							string currentaccountmode = GetAccountMode(acct.Name);
							
							if (currentaccountmode == "Executions")
								isordersm = false;
							
							if (currentaccountmode == "Default" && pCopierMode == "Executions")
								isordersm = false;						
							
						}
						
		
						
						
						
						
						
						if (pProtectPosition && isordersm)
										if (AllDuplicateAccounts.Contains(acct.Name))
										foreach (KeyValuePair<string, MasterAccountATM> kvp in FilledMasterAccountATMs)
										{
											
//											bool isok = true;
											bool isdifferentatm = false;
											string currentaccountmode = string.Empty;
											
											if (pIsATMSelectEnabled)
											{											
												currentaccountmode = GetAccountMode(acct.Name);
												
												if (currentaccountmode != "Orders" && currentaccountmode != "Default")
													isdifferentatm = true;
													
											}
											
											string compareatm = kvp.Key;
											
											if (isdifferentatm)
												compareatm = currentaccountmode;
											
											
//											if (isok)
											if (DateTime.Now > kvp.Value.StartTime)
											{
												
												
												
												//Print(kvp.Key + "   " + kvp.Value.ExpireTime.ToString());
												
												
												if (!RemoveATMCheck.Contains(kvp.Key))												
													RemoveATMCheck.Add(kvp.Key);
										
												
												
												bool atmmissing = true;
												
												foreach (Order or in acct.Orders.ToList())
												{
													
													bool OrderStateOK = or.OrderState != OrderState.Cancelled || or.OrderState == OrderState.Filled;
													
													if (or.OrderType != OrderType.Market)
													if (OrderStateOK)
													{
														if (or.Name.Contains("Stop") && !or.Name.Contains("Stop "))
														
														{
															
															
															object aaaa = 	or.GetOwnerStrategy();
															
															AtmStrategy SlaveATM = null;
															
															if (aaaa != null)
															if (aaaa.GetType() == typeof(AtmStrategy))
																SlaveATM = (AtmStrategy) aaaa;
															//Print(SlaveATM.DisplayName);
														
															if (SlaveATM != null)
															if (SlaveATM.DisplayName.Contains(compareatm))
																atmmissing = false;
															
															
															
														}
													}
												}
					
												
												
												//Print("atmmissing " + atmmissing);
					
												if (atmmissing)
												{
													
													string thisexinstr = kvp.Value.ATMS.Instrument.FullName;
														
													Instrument SlaveExInstrument = kvp.Value.ATMS.Instrument;
													
													if (pIsCrossEnabled)
													{
															
														Instrument MicroInstrument = GetTheInstrument(thisexinstr, "Micro", false);
														Instrument MiniInstrument = GetTheInstrument(thisexinstr, "Mini", false);				
						
						
														SlaveExInstrument = MicroInstrument;
														
														if (AllCrossAccounts.Contains(acct.Name))
														{
							
															SlaveExInstrument = MiniInstrument;
														}	
													}
													
							
													Position PP = acct.GetPosition(SlaveExInstrument.Id);
													
													if (PP != null)
													{
														//Print("Protect " + acct.Name + " " + kvp.Value.ATMS.DisplayName);
														
														//Print(PP.MarketPosition);
														
														AddRejectedEvent(PP.Account.Name, "Protect", "");
														
														AtmStrategy FinalATM = ATMStrategyForFollower(PP.Account, kvp.Value.ATMS);
													
														NinjaTrader.NinjaScript.AtmStrategy.ProtectPosition(FinalATM, PP);
														
													}
												}
												
												
												
											}
											
											
											
											
										}
					
					
					
					
					
							
									
					
					
					
				
					//bool NoStops = true;
					
					// loop through all orders in each slave account
					
					// detect rejected orders and resubmit them				
										
					Order NewOrder = null;
					//AtmStrategy NewATM = null;					
							
										
					Z.FrozenOrders = 0;
										
					foreach (Order or in acct.Orders.ToList())
					{
						
						bool OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.TriggerPending
							|| or.OrderState == OrderState.ChangePending || or.OrderState == OrderState.ChangeSubmitted || or.OrderState == OrderState.PartFilled;
						
						
						
						bool ormaster = acct == CurrentMasterAccount && pResubmitMaster;
		
						
						
						if (or.OrderState == OrderState.CancelPending || or.OrderState == OrderState.ChangePending || or.OrderState == OrderState.Initialized)
						{
						
							
							
							if (or.Time.AddSeconds(2).Ticks < DateTime.Now.Ticks)
							{
								
								
								//Print(or.Account.Name + "  " + or.Instrument.MasterInstrument.Name + "  " + or.OrderState);
								
								TotalFroze++;
								
								// only show reset button for sim accounts
								if (acct.Provider == Provider.Simulator)
									Z.FrozenOrders = Z.FrozenOrders + 1;
								
								if (acct.Provider == Provider.Simulator)
									TotalFrozeSim++;
								else
									TotalFrozeLive++;
								
								
							} 
							
						}
							
						
						
						//pRejectedOrderHandling = false;
						
						if (pRejectedOrderHandling)
						if (AllDuplicateAccounts.Contains(acct.Name) || ormaster)
						{
							
							//if (acct != CurrentMasterAccount)
							{
							
								if (or.OrderState == OrderState.Rejected && or.Time > DateTime.Now.AddSeconds(-4))
								{
									if (or.OrderType == OrderType.StopMarket || or.OrderType == OrderType.StopLimit)
									if (!or.Name.Contains("Stop"))
									{
										//Print("or.Id " + or.Id);
										//Print("or.OrderId " + or.OrderId);
										
										string ottt = or.OrderType.ToString();
										
										ottt = ottt.Replace("StopLimit", "Stop Limit");
										ottt = ottt.Replace("StopMarket", "Stop");
										
										ottt = or.OrderAction + " " + ottt;
										
									
										
										if (!ResubmittedOrders.Contains(or.OrderId))
										{
											
											if (ormaster)
											{
												AddMessage("Rejected " + ottt + " order on Master account has been resubmitted as " + pRejectedSubmit + " order.", 10, GOLDENROD);
											}
											else
											{
												AddMessage("Rejected " + ottt + " orders on Follower accounts have been resubmitted as " + pRejectedSubmit + " orders.", 10, GOLDENROD);
											}
											
											//
											
											// add
											
											
											//Print("resubmit entry " + acct.Name);
											
										
											ResubmittedOrders.Add(or.OrderId);
													
											AddRejectedEvent(acct.Name, "Rejected", or.OrderId);
											
											
											
											
											
											
											
											
											
											
											
											
											
											double LimitPrice = or.StopPrice;	
											
											if (pRejectedSubmit == "Limit")
											if (pMatchStopLimit && or.OrderType == OrderType.StopLimit)
											{
												LimitPrice = or.LimitPrice;
											}
											else
											{
												if (or.OrderAction == OrderAction.Buy)
													LimitPrice = LimitPrice + pRejectedSubmitOff*or.Instrument.MasterInstrument.TickSize;
												else
													LimitPrice = LimitPrice - pRejectedSubmitOff*or.Instrument.MasterInstrument.TickSize;
													
												LimitPrice = RTTS(or.Instrument, LimitPrice);
											}
										
													
											if (pRejectedSubmit == "Market")
												NewOrder = acct.CreateOrder(or.Instrument, or.OrderAction, OrderType.Market, or.TimeInForce, or.Quantity, 0, 0, or.Oco, or.Name, null);
						
											else											
												NewOrder = acct.CreateOrder(or.Instrument, or.OrderAction, OrderType.Limit, or.TimeInForce, or.Quantity, LimitPrice, 0, or.Oco, or.Name, null);
						
											
											// moved this to outside of loop, could fix the issue
											
		//									object aaaa = 	or.GetOwnerStrategy();					
																		
		//									NewATM = (AtmStrategy) aaaa;		
												
		//									if (NewATM != null)
		//									{
											
		//										Print(NewATM.Name);
		//									}
											
											// fixing SLM with - offset order replacement
											
											
											
											if (or.CustomOrder != null && or.OrderType != OrderType.Market && or.OrderType != OrderType.MIT)
											{
												CustomOrder cccc = new CustomOrder();
												
												cccc.IsSimulatedStopEnabled = or.CustomOrder.IsSimulatedStopEnabled;
												cccc.VolumeTrigger = or.CustomOrder.VolumeTrigger;
												
										
												NewOrder.CustomOrder = cccc;
												
												
												 
											}
										
													
													
													
											
											
											
											
										}
										
										
									}
									
								}
							}
							
						
						}
						
						
						
						
						
						
						
						
						if (or.OrderType != OrderType.Market)
						if (OrderStateOK)
						{
							if (IsPendingEntryOrder(or))
							{
								
								bool isok = true;
								
								if (AllTradingViewStopPrices.Count > 0 || AllTradingViewTargetPrices.Count > 0)
								if (CurrentMasterAccount == acct)
								{
									
									
									if (or.OrderType == OrderType.StopMarket)
									if (AllTradingViewStopPrices.Contains(or.StopPrice))
										isok = false;
									
									if (or.OrderType == OrderType.Limit)
									if (AllTradingViewTargetPrices.Contains(or.LimitPrice))
										isok = false;
									
									if (acct.GetPosition(or.Instrument.Id) == null)
										isok = true;
								}
									
								if (isok)	
								{
									Z.PendingEntry = Z.PendingEntry + or.Quantity;
								
									TotalPendingOrders++;
									
								}
								
								
								
								
								
								
								
								// for ATM Strategy Planning feature, when trading master account, keep follower accounts ATM Strategy matching
								
								bool syncatmplanningchange = true;
								
								
								if (syncatmplanningchange && !pIsATMSelectEnabled)
								if (AllDuplicateAccounts.Contains(acct.Name))
								{
									
									object aaaa = 	or.GetOwnerStrategy();	
					
									AtmStrategy SlaveATM = null;
									
									if (aaaa != null)
									if (aaaa.GetType() == typeof(AtmStrategy))
										SlaveATM = (AtmStrategy) aaaa;		
									
									
									bool IsMatching = LastOrderType == or.OrderType && LastLimitPrice == or.LimitPrice && LastStopPrice == or.StopPrice;
										
								
									
									//if (!IsMatching) // master account order
									if (MasterATM == null || !IsMatching)
									{
										
										
										foreach (Order orr in AllMasterAccountOrders.ToList())
										{
											if (orr.OrderType == or.OrderType && orr.LimitPrice == or.LimitPrice && orr.StopPrice == or.StopPrice)
											{
												LastStopPrice = orr.StopPrice;
												LastLimitPrice = orr.LimitPrice;
												LastOrderType = orr.OrderType;
												
												object cccc = 	orr.GetOwnerStrategy();	
								
												//AtmStrategy MasterATM = null;
														
												//Print("setting master atm");
												
												if (cccc != null)
												if (cccc.GetType() == typeof(AtmStrategy))
												{
													MasterATM = (AtmStrategy) cccc;	
													LastMasterQty = orr.Quantity;
													//Print("setting master atm 2");
												}
									
											}
											
											
										}
										
															
										
									
		
									}
									
//								Print("MMM");
									
//									Print(MasterATM != null);
//									Print("SSS");
//									Print(SlaveATM != null);
									
									
									
									int iiii = 0;
									if (MasterATM != null && SlaveATM != null)		
									foreach( Bracket bb in MasterATM.Brackets)
												{
													
													// add quantity adjustment and detection
												

													
													
											double SlaveExMultiplier = Convert.ToDouble(GetAccountMultiplier(acct.Name));
						
											if (SlaveExMultiplier == 0)
												SlaveExMultiplier = 0.50;
											if (SlaveExMultiplier == -1)
												SlaveExMultiplier = 0.33;
											if (SlaveExMultiplier == -2)
												SlaveExMultiplier = 0.25;
											if (SlaveExMultiplier == -3)
												SlaveExMultiplier = 0.20;	
						
											int SlaveExQty = 0;
											
											if (pMultiplierMode == "Multiplier")
												SlaveExQty = (int) Math.Round(MasterExQty * SlaveExMultiplier);
											else
												SlaveExQty = (int) SlaveExMultiplier;
											
											SlaveExQty = Math.Max(SlaveExQty, 1);					
															
															
												double ratio = SlaveExMultiplier / LastMasterQty;
											
											
											
							
											
													if (pMultiplierMode == "Multiplier")
													{
														ratio = SlaveExMultiplier;
														
													}
													
													
													
													if (SlaveATM.Brackets.Length > iiii)
													{
									
														// if quantity on master account order is changed
														
														
														//double d = 5.0;

														
														//if (dddd >= 1)
														
														
														//Print("bb" + iiii);
														
															
															
														if (ratio != 1)
														{
								
															int newq = 0;
															
															int remainingq = SlaveExQty;
																
															int currentq = SlaveATM.Brackets[iiii].Quantity;
															
															if (currentq != 0)
															if (currentq != currentq*ratio)
															{	
																newq = (int) Math.Round(currentq*ratio);
																newq = Math.Max(newq,1);
																newq = Math.Min(newq,remainingq);
																
																//Print(newq);
																
																SlaveATM.Brackets[iiii].Quantity = newq;
																
																remainingq = remainingq - newq; 
															}
														
													
														}
														
										
//														if (pMultiplierMode == "Multiplier")
//														{			
//															double changed = bb.Quantity*ratio;
//															bool isInt = changed == (int)changed;
															
//															if (isInt)
//															if (SlaveATM.Brackets[iiii].Quantity != changed)
//															{
													
//																SlaveATM.Brackets[iiii].Quantity =  (int) changed;
																
//															}	
//														}
//														else
//														{
															
															
//														}
														
														// keep size of target and stop loss the same
													
//														Print(SlaveATM.Brackets[iiii].StopLoss);
//														Print(bb.StopLoss);
														
															if (SlaveATM.Brackets[iiii].StopLoss != bb.StopLoss)
															{
														
																SlaveATM.Brackets[iiii].StopLoss = bb.StopLoss;
																
															}
															if (SlaveATM.Brackets[iiii].Target != bb.Target)
															{
															
																SlaveATM.Brackets[iiii].Target = bb.Target;
													
															}
														
														
													}
													
													iiii++;
													
												}
										
										
											
								}
										
											
							}
							
							
							
							if (or.Name.Contains("Stop"))
							{
								Z.PendingExit = Z.PendingExit + or.Quantity;
								
								
								
							}		
							
							// cancel order from refresh positions button
							
							if (or.Name.Contains(RefreshOrderName) && or.OrderState == OrderState.Working)
							{
								acct.Cancel(new[] { or });
							}
							
						}
						
						if (pLimitOrderSyncFeatures)
						{
							
							
						
							OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.ChangePending || or.OrderState == OrderState.ChangeSubmitted || or.OrderState == OrderState.PartFilled;
							//OrderStateOK = or.OrderState == OrderState.Working || or.OrderState == OrderState.Accepted || or.OrderState == OrderState.PartFilled;
							
							
							if (OrderStateOK)
							//if (OrderStateOK && or.OrderType == OrderType.Limit)
							if (OrderStateOK && or.OrderType != OrderType.Market && or.OrderType != OrderType.Unknown)	
							{
							
	//							if (!LimitPriceToTotalOrders.ContainsKey(or.LimitPrice))
	//							{
	//								LimitPriceToTotalOrders.Add(or.LimitPrice, 1);
	//							}
	//							else
	//							{
	//								LimitPriceToTotalOrders[or.LimitPrice] = LimitPriceToTotalOrders[or.LimitPrice] + 1;
	//							}
								
//								bool doooooo = false;
								
								
//								if (doooooo)
								
//								if (or.Name.Contains("Target"))
//								{
//									string sdgh = or.Name.Replace("Target", "");
//									int iiii = Convert.ToInt32(sdgh);
									
//									//SortedDictionary<Account, List<string>>
//									if (!TargetNumberToTotalOrders2.ContainsKey(iiii))
//									{
//										SortedDictionary<Account, List<double>> DDDDD = new SortedDictionary<Account, List<double>>();	
										
										
//										TargetNumberToTotalOrders2.Add(iiii, DDDDD);
										
//										List<double> AllPrices = new List<double>();	
										
//										AllPrices.Add(or.LimitPrice);
										
//										if (!TargetNumberToTotalOrders2[iiii].ContainsKey(acct))
//										{
//											TargetNumberToTotalOrders2[iiii].Add(acct, AllPrices);
//										}
										
										
										
										
//									}
//									else
//									{
									
//										if (!TargetNumberToTotalOrders2[iiii].ContainsKey(acct))
//										{
//											List<double> AllPrices = new List<double>();	
//											AllPrices.Add(or.LimitPrice);
											
//											TargetNumberToTotalOrders2[iiii].Add(acct, AllPrices);
//										}										
//										else
//										{
//											if (!TargetNumberToTotalOrders2[iiii][acct].Contains(or.LimitPrice))
//												TargetNumberToTotalOrders2[iiii][acct].Add(or.LimitPrice);
											
//										}
										
//									}
//								}
								
								
							
									double orderprice = or.LimitPrice;
									string finalname = "Limit";
									
									if (or.StopPrice != 0)
										orderprice = or.StopPrice;
									
									if (or.OrderType == OrderType.MIT)
										finalname = "MIT";
									
									if (or.OrderType == OrderType.StopMarket)
										finalname = "Stop";
									
									if (or.OrderType == OrderType.StopLimit)
										finalname = "Stop Limit";
									
								//	Print(orderprice)
									
									if (GetOrdersbyPrice)
									if (!LimitPriceToTotalOrders2.ContainsKey(orderprice))
									{
										PendingDetails ZS = new PendingDetails();
		
								
										ZS = new PendingDetails();
										ZS.Name = or.Instrument;
										
		//								ZS.AllInstruments = new List<Instrument>();
		//								ZS.AllOrderNames = new List<string>();
										
										
										ZS.AllInstruments.Add(or.Instrument);
										ZS.StartTime = DateTime.Now;
										ZS.ExpireTime = DateTime.Now;
										ZS.Switch = true;
										ZS.Width = 1;
										
										ZS.Direction = 1;
										if (or.IsShort)
											ZS.Direction = -1;
										
										ZS.AllOrderNames.Add(or.Name);
										ZS.AllOrderTypes.Add(finalname);
										
										LimitPriceToTotalOrders2.Add(orderprice, ZS);
										
										
										
										
									}
									else
									{
										LimitPriceToTotalOrders2[orderprice].Width = LimitPriceToTotalOrders2[orderprice].Width + 1;
										
		//								if (LimitPriceToTotalOrders2[or.LimitPrice].AllInstruments.Count > 0)
		//									LimitPriceToTotalOrders2[or.LimitPrice].AllInstruments.Clear();
										
										if (!LimitPriceToTotalOrders2[orderprice].AllInstruments.Contains(or.Instrument))
											LimitPriceToTotalOrders2[orderprice].AllInstruments.Add(or.Instrument);
										
										if (!LimitPriceToTotalOrders2[orderprice].AllOrderNames.Contains(or.Name))
											LimitPriceToTotalOrders2[orderprice].AllOrderNames.Add(or.Name);
										
										if (!LimitPriceToTotalOrders2[orderprice].AllOrderTypes.Contains(finalname))
											LimitPriceToTotalOrders2[orderprice].AllOrderTypes.Add(finalname);									
										
										
									}							

							
							}
						} // end limit order sync storage
						
						
					}
					
					
//					bool heryes = false;
					
//					if (heryes)
					
					// submit rejected orders
					
					
					if (NewOrder != null)
					{
							
									//Print("NewOrder");
									
						bool IsMasterAccount = CurrentMasterAccount == acct;
						
						
									AtmStrategy SlaveA = null;
									
									if (LastNewOrderATM != null)
									{
										SlaveA = (AtmStrategy) LastNewOrderATM.Clone();
										
										
									}
									
									if (SlaveA == null)	
									{				
										
										
										acct.Submit(new[] { NewOrder });
														
										//Print("59 Submit 3");
										
										//acct.CreateOrder(
										
									}
									else
									{				
										
									
										//AtmStrategy SlaveA = (AtmStrategy) NewATM.Clone();	

										// adjusting ATM Strategy before the order is placed, based on various size settings
										
										
										bool doooo = false;
										
										
										//if (doooo)
										if (pIsXEnabled && !IsMasterAccount)
											SlaveA = ATMStrategyForFollower(acct, SlaveA);	
											
									
										
										
										//SlaveA.AtmStrategyCreate(
													
											NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(SlaveA, NewOrder);
										
										
										
										
										
									}
					
					
					
					
					}
					
					
					
					
					
				}
				
				Z.PNLGR = acct.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar);
				Z.PNLR = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
				//Z.PNLUALL = acct.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
				Z.PNLGR = Math.Round(Z.PNLGR, 2);
				Z.PNLR = Math.Round(Z.PNLR, 2);
				
			//	Z.PNLGR = Z.PNLGR - 110;
				
				double NewCashValue = acct.Get(AccountItem.CashValue, Currency.UsDollar);
				double NetLiquidation = acct.Get(AccountItem.NetLiquidation, Currency.UsDollar);

				Z.CashValue = Math.Round(NewCashValue, 2);
				Z.NetLiquidation = Math.Round(NetLiquidation, 2);
				
	

				
				if (Z.NetLiquidation == 0)
					Z.NetLiquidation = Z.CashValue;
			
				if (IsConnected)
				{
					if (GetAccountSavedData(acct.Name, pAllAccountCashValue) != Z.CashValue)
						SetAccountSavedData(acct.Name, pAllAccountCashValue, Z.CashValue.ToString());
					
					if (GetAccountSavedData(acct.Name, pAllAccountNetLiquidation) != Z.NetLiquidation)
						SetAccountSavedData(acct.Name, pAllAccountNetLiquidation, Z.NetLiquidation.ToString());
				
				}
				else
				{
					Z.CashValue = GetAccountSavedData(acct.Name, pAllAccountCashValue);
					Z.NetLiquidation = GetAccountSavedData(acct.Name, pAllAccountNetLiquidation);
					
					//Print(acct.Name + "  " + Z.CashValue);
					
					if (Z.CashValue == -1000000000)
					{						
						Z.CashValue = 0;
						SetAccountSavedData(acct.Name, pAllAccountCashValue, Z.CashValue.ToString());
					}
					
					if (Z.NetLiquidation == -1000000000)
					{
						Z.NetLiquidation = 0;
						SetAccountSavedData(acct.Name, pAllAccountNetLiquidation, Z.NetLiquidation.ToString());
					}

				}
				

					
				double requestedpayout = GetAccountSavedData(acct.Name, pAllAccountPayouts);
				
				if (requestedpayout != -1000000000)
					Z.NetLiquidation = Z.NetLiquidation - requestedpayout;		
				
				
				// starting coding to save 
				
				
	//				if (IsConnected)
//				if (NewCashValue != 
			
				//SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());
				
				//allpeak = GetAccountSavedData(acct.Name, pAllAccountCashValue);
				//allpeak = GetAccountSavedData(acct.Name, pAllAccountNetLiquidation);

				
				
				
				
				
				// tested, dont' do anything
				
				
				//Z.NetLiquidation = acct.Get(AccountItem.MaintenanceMargin, Currency.UsDollar);	
				
//				Z.NetLiquidation = acct.Get(AccountItem.SodLiquidatingValue, Currency.UsDollar);	
//				Z.NetLiquidation = acct.Get(AccountItem.LookAheadMaintenanceMargin, Currency.UsDollar);
//				Z.NetLiquidation = acct.Get(AccountItem.ExcessInitialMargin, Currency.UsDollar);
				
				//Print(accccnnnn+"DDSD");
				
				
				//AccountItem.
				
				Z.Acct = acct;
				
				
				string thisrowacct2 = acct.Name;
				
				
				// tradovate names are the same for display name and name
				
				if (pAccountDisplayType == "Display Name")
					thisrowacct2 = acct.DisplayName;
				
				thisrowacct2 = PrivateAccountName(thisrowacct2);
				
				
//				if (thisrowacct2 == "APEX2026360000011")
//					thisrowacct2 = "S1Apr294206479";
								
								
//						thisrowacct2 = thisrowacct2.Replace("51383", "51393"); // larry hide apex id
//						thisrowacct2 = thisrowacct2.Replace("53720", "53730"); // joel hide apex id
		
				
		
				//Z.Acct.UpdateAccountItems(DateTime.Now, true);
				
						
						
			
						
					Z.PrivateName = thisrowacct2;	
						
				
				
				
				
				Z.AudioStatus = "Z";
				Z.MasterStatus = "Z";
				
				
				if (pThisMasterAccount == acct.Name)
				{
					if (CurrentMasterAccount != null)
						if (IsAccountFlat(CurrentMasterAccount))
							IgnoreExecutionUntilFlat = false;
					
					
					Z.AudioStatus = "Master";
					Z.MasterStatus = "Master";
				}
				
				if (AllDuplicateAccounts.Contains(acct.Name))
				{
					Z.AudioStatus = "Slave";
					Z.MasterStatus = "Z";
				}
				
				
				bool manuallycalcgrossrealized = true;
				
				foreach (Execution ex in acct.Executions)
							{
								
								if (ex.Time >= LastSessionStart) 
								{
									Z.TotalBought = Z.TotalBought + ex.Quantity;
								
									if (manuallycalcgrossrealized)
										Z.Commish = Z.Commish + ex.Commission;	
								}
							}
							
					
				
			//	if (manuallycalcgrossrealized)			
				if (manuallycalcgrossrealized && Z.Commish != 0) // corrected tradovate display of commissions
				{
					Z.PNLR = Z.PNLGR - Z.Commish;
					
				}
				else
				{
					Z.Commish = Z.PNLGR - Z.PNLR;
					
					Z.Commish = Math.Max(0, Z.Commish); // after hours reset makes it appear negative commissions
				}
							
				
							
							
							
							
							
							
				Z.PNLU = 0;	
						
				
				//Print(accccnnnn+"DDSD1");
				
				
				// account exit on, risk manager on
				
				bool IsAccountExitOn = pIsRiskFunctionsEnabled && (GetSavedAutoClose(acct.Name) == "Yes");
				
				
				if (IsAccountExitOn)				
				{
					
					
					
					
					
					
				}
				
				
				if (pAllInstruments || IsAccountExitOn)
				{
					
					//Z.PNLU = acct.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
					
					if (acct.Positions != null)
					foreach (Position pp in acct.Positions)
							{
								
								//string iname = pp.Instrument.FullName;
								
							//	if (pAllInstruments || pp.Instrument == GetMini(iname) || pp.Instrument == GetMicro(iname))
								

								
								if (pSplitMiniAndMicro)
								{
									
									
								}
								else
								{
									 
									if (pp.MarketPosition == MarketPosition.Long)
										Z.PositionLong = Z.PositionLong + pp.Quantity;
									if (pp.MarketPosition == MarketPosition.Short)
										Z.PositionShort = Z.PositionShort + pp.Quantity;									
									
									Z.Pos = pp;
									Z.AllPositions.Add(pp);
									
									TotalPositions = TotalPositions + pp.Quantity;
									
								}
								
								
								
								//Print(accccnnnn+"DDSD1aaa");
								
								double lastp = pp.AveragePrice;
								
								if (pp.Instrument.MarketData.Last != null)
									lastp = pp.Instrument.MarketData.Last.Price;
								
								Z.PNLU = Z.PNLU + pp.GetUnrealizedProfitLoss(PerformanceUnit.Currency, lastp);
								Z.PNLUALL = Z.PNLUALL + pp.GetUnrealizedProfitLoss(PerformanceUnit.Currency, lastp);
							}							
						
//						Print("---------------------------");
//						Print("Z.PNLU  " + GetDollarString("$",Z.PNLU));
//						Print("Z.PNLUALL  " + GetDollarString("$",Z.PNLUALL));	
							
					
							
				}
				else
				{
					string iname = Instrument.FullName;
					Position pp = acct.GetPosition(GetTheInstrument(iname, "Mini", false).Id);
					
					if (pp != null)
					if (pp.MarketPosition != MarketPosition.Flat)
					{
													
								if (pSplitMiniAndMicro)
								{
									
									
								}
								else
								{
									
									if (pp.MarketPosition == MarketPosition.Long)
										Z.PositionLong = Z.PositionLong + pp.Quantity;
									if (pp.MarketPosition == MarketPosition.Short)
										Z.PositionShort = Z.PositionShort + pp.Quantity;
									
									Z.Pos = pp;
									Z.AllPositions.Add(pp);
									
									TotalPositions = TotalPositions + pp.Quantity;
									
								}
								
								

								
								//Print(accccnnnn+"DDSD1aaa");
								
								double lastp = pp.AveragePrice;
								
								if (pp.Instrument.MarketData.Last != null)
									lastp = pp.Instrument.MarketData.Last.Price;
								
								Z.PNLU = Z.PNLU + pp.GetUnrealizedProfitLoss(PerformanceUnit.Currency, lastp);						
						
						
					}
					
					
					pp = acct.GetPosition(GetTheInstrument(iname, "Micro", false).Id);
					
					if (pp != null)
					if (pp.MarketPosition != MarketPosition.Flat)
					{
						
								if (pSplitMiniAndMicro)
								{
									
									
								}
								else
								{
									
									if (pp.MarketPosition == MarketPosition.Long)
										Z.PositionLong = Z.PositionLong + pp.Quantity;
									if (pp.MarketPosition == MarketPosition.Short)
										Z.PositionShort = Z.PositionShort + pp.Quantity;	
									
									Z.Pos = pp;
									Z.AllPositions.Add(pp);
									
									TotalPositions = TotalPositions + pp.Quantity;
									
								}
								
								//Print(accccnnnn+"DDSD1aaa");
								
								double lastp = pp.AveragePrice;
								
								if (pp.Instrument.MarketData.Last != null)
									lastp = pp.Instrument.MarketData.Last.Price;
								
								Z.PNLU = Z.PNLU + pp.GetUnrealizedProfitLoss(PerformanceUnit.Currency, lastp);						
						
						
					}					
					
					
				}
			

				if (Z.PositionLong != 0 || Z.PositionShort != 0)
				{
					RefreshPositionsClickedTotal = RefreshPositionsClickedTotal + 1;
					RefreshPositionsClickedOrders = 1;
				}
			
				
				Z.PNLGR = Math.Round(Z.PNLGR, 2);
				Z.PNLR = Math.Round(Z.PNLR, 2);
				Z.PNLU = Math.Round(Z.PNLU, 2);
				Z.PNLUALL = Math.Round(Z.PNLUALL, 2);
				
				Z.PNLTotal = Z.PNLU + Z.PNLR;
				
				
				Z.Signals = "";
				
				
				

				string ThisRowAccount = acct.Name; //  acct.DisplayName;

			
//				if (ThisRowAccount == "APEX2026360000011")
//					ThisRowAccount = "S1Apr294206479";


				
			
				bool IsAPEXPA = ThisRowAccount.Contains("PA-APEX") || ThisRowAccount.Contains("PAAPEX");
				bool IsAPEXEval = ThisRowAccount.Contains("APEX") && !IsAPEXPA;
				
				
				// "LE-LL"
				// "WK-LL"
				bool IsLeeLooPA = ThisRowAccount.Contains("PA-LL");
				bool IsLeeLooEval = (ThisRowAccount.Contains("LL") || ThisRowAccount.Contains("LB-LL") || ThisRowAccount.Contains("LE-LL") || ThisRowAccount.Contains("WK-LL")) && !IsLeeLooPA;

				bool IsBulenoxPA = ThisRowAccount.Contains("BX-M");
				bool IsBulenoxEval = ThisRowAccount.Contains("BX") && !IsBulenoxPA;
			
				// MFFUSF (Funded from a starter Jame Lynch) && MFFUEVEX (eval Yoania) added on July 31, 2024 // my funded futures
				
					bool IsMFFPA = ThisRowAccount.Contains("MFFUSF") || ThisRowAccount.Contains("MFFUSFST") || ThisRowAccount.Contains("MFFULIVEST") || ThisRowAccount.Contains("MFFUSFEX");
					bool IsMFFEval = (ThisRowAccount.Contains("MFFUEV") || ThisRowAccount.Contains("MFFUEVST") || ThisRowAccount.Contains("MFFUEVEX"))&& !IsMFFPA;
				
				
				
				
														
					bool IsEliteTFPA = ThisRowAccount.Contains("ELITE") && ThisRowAccount.Contains("ETF");
					bool IsEliteTFEval = ThisRowAccount.Contains("ETF") && !IsEliteTFPA;
				
		
					bool IsTakeProfitTraderPA = ThisRowAccount.Contains("TAKEPROFITPRO");						
					bool IsTakeProfitTraderEval = (ThisRowAccount.Contains("TAKEPROFIT") || ThisRowAccount.Contains("TPT")) && !IsTakeProfitTraderPA;
				
					
					bool IsTickTickPA = ThisRowAccount.Contains("TTTD");
					bool IsTickTickEval = ThisRowAccount.Contains("TTT") && !IsTickTickPA;
					
											// waiting on tick tick and take profit funded naming schemes
						
						
						
					bool IsFFNPA = ThisRowAccount.Contains("FFNX");
					bool IsFFNEval = ThisRowAccount.Contains("FFN")	&& !IsFFNPA;
					
					bool IsTradeDayPA = ThisRowAccount.Contains("ELTDE");
					IsTradeDayPA = false;
					
					bool IsTradeDayEval = ThisRowAccount.Contains("ELTDE") && !IsTradeDayPA;			
						
					bool IsTopStepEval = ThisRowAccount.Contains("S1Jan") || ThisRowAccount.Contains("S1Feb") || ThisRowAccount.Contains("S1Mar") || ThisRowAccount.Contains("S1Apr") || ThisRowAccount.Contains("S1May") || ThisRowAccount.Contains("S1Jun") || 
					ThisRowAccount.Contains("S1Jul") || ThisRowAccount.Contains("S1Aug") || ThisRowAccount.Contains("S1Sep") || ThisRowAccount.Contains("S1Oct") || ThisRowAccount.Contains("S1Nov") || ThisRowAccount.Contains("S1Dec");
						
								
						// Here is what BluSky prints on NT8
		
						// BLUDERVGVPE0
						
						// Purdia is PUR11462
						
				
					//APEX Trader Funding
					//Blusky Trading
					//Bulenox Funding
					//DayTraders 	NO
					//Elite Trader Funding
					//Futures Funded Network
					//Leeloo Trading
					//Legends Trading 	NO
					//LifeUp Trading 	NO
					//MyFundedFutures 
					//Phidias 	NO
					//Purdia Capital
					//Take Profit Trader
					//TickTickTrader
					//TopStep
					//TradeDay
					//TradeFundrr 	NO
					//Tradeify
						
				
				
					bool IsBLUEval = ThisRowAccount.Contains("BLU");
					bool IsBLUPA = false;
				
					bool IsPUREval = ThisRowAccount.Contains("PUR");
					bool IsPURPA = false;
				
					
					bool IsTradeifyGEval = ThisRowAccount.Contains("TDYG");
					bool IsTradeifyAEval = ThisRowAccount.Contains("TDYA");
					bool IsTradeifyEval = IsTradeifyGEval || IsTradeifyAEval;

					bool IsTradeifyPA = ThisRowAccount.Contains("FTDYSF") || ThisRowAccount.Contains("FTDYFA") || ThisRowAccount.Contains("FTD");
				
				
					bool IsDayTradersPA = ThisRowAccount.Contains("PRO-DT-");
					bool IsDayTradersEval = !IsDayTradersPA && ThisRowAccount.Contains("DT-");
				
					bool IsLegendsEval = ThisRowAccount.Contains("TY");
					bool IsLegendsPA = false;					
	
					bool IsLifeUpEval = ThisRowAccount.Contains("LUTBACH") || ThisRowAccount.Contains("LUTEXCH");
					bool IsLifeUpPA = ThisRowAccount.Contains("LUTBARW") || ThisRowAccount.Contains("LUTEXRW");	
				
					bool IsPhidiasEval = ThisRowAccount.Contains("PP-F");
					bool IsPhidiasPA = ThisRowAccount.Contains("PP-CASH-F");				
				
					bool IsTradeFundrrEval = ThisRowAccount.Contains("TFRRP");
					bool IsTradeFundrrPA = false;
				
				
				
					//Life Up Trading	LUTBACH	Basic Plan (Challenge)
					//Life Up Trading	LUTEXCH	Expert Plan (Challenge)
					//Life Up Trading	LUTBARW	Basic Reward (Funded)
					//Life Up Trading	LUTEXRW	Expert Reward (Funded)
					//DayTraders	DT-0193-04	Evaluation
					//DayTraders	PRO-DT-0193-01	Pro Accounts
					//Phidias	PP-F50K-004275-000011	Evaluation
					//Phidias	PP-CASH-F50K-004275-000006	Cash Accounts
					//The Legends Trading	TY352626549	All Accounts
					//TradeFundrr	TFRRP	All Accounts
				
				
				// total is 18
				
					bool IsPropFirmEval = IsAPEXEval || IsLeeLooEval || IsBulenoxEval || IsMFFEval || IsEliteTFEval || IsFFNEval || IsTradeDayEval || IsTakeProfitTraderEval || IsTickTickEval || IsBLUEval || IsPUREval || IsTradeifyEval || IsDayTradersEval || IsLegendsEval || IsLifeUpEval || IsPhidiasEval || IsTradeFundrrEval;
				
					bool IsPropFirmPA = IsAPEXPA || IsLeeLooPA || IsBulenoxPA || IsMFFPA || IsEliteTFPA || IsFFNPA || IsTradeDayPA || IsTakeProfitTraderPA || IsTickTickPA || IsBLUPA || IsPURPA || IsTradeifyPA || IsDayTradersPA || IsLegendsPA || IsLifeUpPA || IsPhidiasPA || IsTradeFundrrPA;
					

											

							 			
										
										
	
				
				// top step EXPRESS - funded				
				// top step PRACTICE - eval
				
//				bool IsTopStepEval = ThisRowAccount.Contains("S1Jan") || ThisRowAccount.Contains("S1Feb") || ThisRowAccount.Contains("S1Mar") || ThisRowAccount.Contains("S1Apr") || ThisRowAccount.Contains("S1May") || ThisRowAccount.Contains("S1Jun") || 
//				ThisRowAccount.Contains("S1Jul") || ThisRowAccount.Contains("S1Aug") || ThisRowAccount.Contains("S1Sep") || ThisRowAccount.Contains("S1Oct") || ThisRowAccount.Contains("S1Nov") || ThisRowAccount.Contains("S1Dec");
				
				

				
				if (IsPropFirmPA)
					TotalFundedAccounts = TotalFundedAccounts + 1;
				
				
				Z.AudioLiq = 0;
				Z.FromBlown = 1000000;
				Z.FromFunded = 1000000;
				Z.PeakValue = 0;
				Z.TrailingThre = 0;
				Z.FundedAmount = 1000000;
				Z.MaxPayout = 0;
				
				
				
				
				
				double CurrentAccountValue = Z.CashValue - Z.Commish;
				
	
				
				
				 
				
				
				// 50000 - 53000 to pass 48000
				// 100000 6000 3000
				// 150000 - 4500
				
				
				
				
				double TrailingThreshold = 0;
				double AccountBeginningValue = 0;
				double ProfitToFund = 0;
				double ThisMaxPay = 0;
				
				double MaxAutoLiquidate = 0;
				
				double FundedAmount = 0;
				int LowEndBuffer = 1000;

				//double MyCustomNetLiquidation = Z.NetLiquidation;

	
				if (IsTopStepEval)
				{

				
					if (CurrentAccountValue > 47500-LowEndBuffer && CurrentAccountValue < 63000) // $50,000
					{
						TrailingThreshold = 2000;
						ProfitToFund = 3000;
						AccountBeginningValue = 50000;
						FundedAmount = AccountBeginningValue + ProfitToFund;
						MaxAutoLiquidate = AccountBeginningValue + 100;
					}
								
					if (CurrentAccountValue > 95500-LowEndBuffer && CurrentAccountValue < 116000) // $50,000
					{
						TrailingThreshold = 3000;
						ProfitToFund = 6000;
						AccountBeginningValue = 100000;
						FundedAmount = AccountBeginningValue + ProfitToFund;
						MaxAutoLiquidate = AccountBeginningValue + 100;
					}					
					if (CurrentAccountValue > 142000-LowEndBuffer && CurrentAccountValue < 169000)
					{
						TrailingThreshold = 4500;
						ProfitToFund = 9000;
						AccountBeginningValue = 150000;
						FundedAmount = AccountBeginningValue + ProfitToFund;
						MaxAutoLiquidate = AccountBeginningValue + 100;
					}				

						
					Z.FundedAmount = FundedAmount;
					Z.MaxPayout = ThisMaxPay;
					
					Z.TrailingThre = TrailingThreshold;
			
						
					
					
					// fixed, no trailing
				
					Z.PeakValue = AccountBeginningValue;
					SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());
					
					
					
					if (ResetNowAutoLiquidateAccount != string.Empty)
					if (ResetNowAutoLiquidateAccount == acct.Name)	
					{

						Z.PeakValue = AccountBeginningValue;
						
						
						SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());
						
						
						ResetNowAutoLiquidateAccount = string.Empty;
						
					}	
									

					double allpeak = Z.PeakValue;
						
			
					
					
					Z.AudioLiq = Math.Max(allpeak - Z.TrailingThre, AccountBeginningValue - Z.TrailingThre);
					Z.FromBlown = Math.Round(Z.NetLiquidation - Z.AudioLiq, 2);	
						

					
					
					double tddd = 0; 
									
					try
					{	
						// 8.1.2.1 this doesn't work
						
						tddd = Z.Acct.Get(AccountItem.TrailingMaxDrawdown, Currency.UsDollar);
					}
					catch
					{
					
					}

					// https://app.intercom.com/a/inbox/xrod7vc7/inbox/shared/all/conversation/183756700012325
					// discovered that column can equal to negative cash value (-100,000) at times. so fixing this issues
					
					if (tddd != 0 && tddd*-1 != Z.CashValue)
					if (Z.TotalBought == 0)
					{
						double syncpeak = Z.NetLiquidation + TrailingThreshold - tddd;
						
						if (allpeak != syncpeak)
						{
							Z.PeakValue = syncpeak;
							SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());	
						}
					}
					
					
					// we don't want to sync this all the time, because NinjaTrader column does not always match Tradovate
					
					tddd = 0;
			

					if (tddd != 0)
						Z.FromBlown = tddd;		
								
					
					

					
					
						
					Z.FromFunded = Math.Round(Z.FundedAmount - Z.NetLiquidation, 2) + pDollarsExceedFunded;	
					
				}
				
				
				
				
				
				
				if (IsPropFirmEval || IsPropFirmPA)
				{
					
					// disconnected
//						if (Z.CashValue == 0)
//						{
						
//							CurrentAccountValue = GetAccountSavedData(acct.Name, pAllAccountAutoLiquidate);
							
//						}
					
						
						
					double MyCustomNetLiquidation = CurrentAccountValue + Z.PNLUALL;
					
					// messes up net liquidation the morning after trading
					
					//Z.NetLiquidation = MyCustomNetLiquidation;	
						 
					
					requestedpayout = GetAccountSavedData(acct.Name, pAllAccountPayouts);
					
					if (requestedpayout != -1000000000)
						Z.NetLiquidation = Z.NetLiquidation - requestedpayout;								
		
					
					
					if (CurrentAccountValue > 9000-LowEndBuffer && CurrentAccountValue < 15000) // $10,000
					{
						TrailingThreshold = 1000;
						ProfitToFund = 1000;
						AccountBeginningValue = 10000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 1000;
						
						if (IsEliteTFPA || IsEliteTFEval)
						{
							TrailingThreshold = 1000;
							ProfitToFund = 1250;
						}	
						

						
						
					}					
					if (CurrentAccountValue > 22500-LowEndBuffer && CurrentAccountValue < 36500) // $25,000
					{
						TrailingThreshold = 1500;
						ProfitToFund = 1500;
						
						
						AccountBeginningValue = 25000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 1500;
						
						if (IsFFNPA || IsFFNEval)
						{
							TrailingThreshold = 1500;
							ProfitToFund = 2000;
						}		
						
//						if (IsMFFPA || IsMFFEval || IsFFNPA || IsFFNEval)
//						{
//							TrailingThreshold = 1500;
//							ProfitToFund = 2000;
//						}						
						
						if (IsBLUEval)
						{
							TrailingThreshold = 1200;
							ProfitToFund = 1500;
							MaxAutoLiquidate = AccountBeginningValue;
						}		
						
						if (IsPUREval)
						{
							TrailingThreshold = 2000;
							ProfitToFund = 2000;
							MaxAutoLiquidate = AccountBeginningValue;
						}	
						

						
					}
					if (CurrentAccountValue > 47500-LowEndBuffer && CurrentAccountValue < 63000) // $50,000
					{
						TrailingThreshold = 2500;
						ProfitToFund = 3000;
						
						
						AccountBeginningValue = 50000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 2000;
						
						
				
						
				
						
						if (ThisRowAccount.Contains("MFFUSFEX") || ThisRowAccount.Contains("MFFUEVEX"))
						{
							TrailingThreshold = 2000;
							ProfitToFund = 4000;
						}
						
						
						
						if (IsEliteTFPA || IsEliteTFEval || IsTakeProfitTraderEval || IsTakeProfitTraderPA)
						{
							TrailingThreshold = 2000;
							ProfitToFund = 3000;
						}
						
						if (IsFFNPA || IsFFNEval || IsBLUEval || IsPUREval)
						{
							TrailingThreshold = 2000;
							ProfitToFund = 3000;
						}	
				
						
//						if (IsMFFPA || IsMFFEval || IsFFNPA || IsFFNEval || IsBLUEval || IsPUREval)
//						{
//							TrailingThreshold = 2000;
//							ProfitToFund = 3000;
//						}	
						
						if (IsTradeDayEval || IsTradeDayPA)
						{
							TrailingThreshold = 2000;
							ProfitToFund = 2500;
						}							
					
						if (ThisRowAccount.Contains("LUTBA"))
						{
							TrailingThreshold = 2000;
							ProfitToFund = 2500;
						}	
						
						if (ThisRowAccount.Contains("LUTEX"))
						{
							TrailingThreshold = 2200;
							ProfitToFund = 2700;
						}	
						
						if (IsLegendsEval || IsLegendsPA)
						{
							TrailingThreshold = 2000;
							ProfitToFund = 3000;
						}		
						
						if (IsPhidiasEval || IsPhidiasPA)
						{
							TrailingThreshold = 2500;
							ProfitToFund = 4000;
						}							
						
						
						if (IsTickTickPA)
						{
							TrailingThreshold = 2000;
							ProfitToFund = 1000;
						}								
						
					}
					if (CurrentAccountValue > 72250-LowEndBuffer && CurrentAccountValue < 89250) // $75,000
					{
						TrailingThreshold = 2750;
						ProfitToFund = 4250;
						AccountBeginningValue = 75000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 2250;
						
						if (IsEliteTFPA || IsEliteTFEval || IsTakeProfitTraderEval || IsTakeProfitTraderPA)
						{
							TrailingThreshold = 2500;
							ProfitToFund = 4500;
						}		
						
				
						
						
					}				
					if (CurrentAccountValue > 95500-LowEndBuffer && CurrentAccountValue < 116000) // $100,000
					{
						TrailingThreshold = 3000;
						ProfitToFund = 6000;
						
						
						AccountBeginningValue = 100000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 2500;
						
						
						if (ThisRowAccount.Contains("MFFUSFEX") || ThisRowAccount.Contains("MFFUEVEX"))
						{
							TrailingThreshold = 3000;
							ProfitToFund = 8000;
						}
						
						if (IsMFFPA || IsMFFEval)
						{
							TrailingThreshold = 3500;
							ProfitToFund = 6000;
						}	
						
						if (IsFFNPA || IsFFNEval)
						{
							TrailingThreshold = 3600;
							ProfitToFund = 6000;
						}		
						
						if (IsTradeDayEval || IsTradeDayPA)
						{
							TrailingThreshold = 3000;
							ProfitToFund = 5000;
						}
						

						
						if (IsBLUEval)
						{
							TrailingThreshold = 2500;
							ProfitToFund = 6000;
						}						
						
						if (ThisRowAccount.Contains("STATIC"))
						if (IsEliteTFEval || IsEliteTFPA)
						{
							TrailingThreshold = 625;
							ProfitToFund = 2000;
						}
						
						
						// EVALDHETF
						
						if (ThisRowAccount.Contains("DHETF"))
						{
							TrailingThreshold = 3500;
							ProfitToFund = 5000;							
						}
						
						
						
						if (ThisRowAccount.Contains("LUTBA"))
						{
							TrailingThreshold = 3500;
							ProfitToFund = 5000;
							MaxAutoLiquidate = AccountBeginningValue;
						}	
						
						
						if (ThisRowAccount.Contains("LUTEX"))
						{
							TrailingThreshold = 3700;
							ProfitToFund = 5200;
							MaxAutoLiquidate = AccountBeginningValue;
						}	
						
						if (IsTickTickPA)
						{
							TrailingThreshold = 5000;
							ProfitToFund = 2000;
						}
							
							
						if (IsTickTickEval)
						{
							TrailingThreshold = 3500;
							ProfitToFund = 5000;
						}	
													
						
					}					
					if (CurrentAccountValue > 142000-LowEndBuffer && CurrentAccountValue < 169000) // $150,000
					{
					
							
						
						TrailingThreshold = 5000;
						ProfitToFund = 9000;
						AccountBeginningValue = 150000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 2750;
						
						
						if (ThisRowAccount.Contains("MFFUSFEX") || ThisRowAccount.Contains("MFFUEVEX"))
						{
							TrailingThreshold = 4500;
							ProfitToFund = 12000;
						}
						
						
						if (IsBulenoxEval || IsBulenoxPA)	
						{
							TrailingThreshold = 4500;
							ProfitToFund = 9000;
						}
						

						
						if (IsTradeDayEval || IsTradeDayPA)
						{
							TrailingThreshold = 4000;
							ProfitToFund = 7500;
						}
						
						if (IsTakeProfitTraderEval || IsTakeProfitTraderPA)
						{
							TrailingThreshold = 4500;
							ProfitToFund = 9000;
						}
						
						if (ThisRowAccount.Contains("STATIC"))
						if (IsEliteTFEval || IsEliteTFPA)
						{
							TrailingThreshold = 1250;
							ProfitToFund = 4000;
						}
															
						if (ThisRowAccount.Contains("LUTBA"))
						{
							TrailingThreshold = 5000;
							ProfitToFund = 8500;
							MaxAutoLiquidate = AccountBeginningValue;
						}	
						
						if (ThisRowAccount.Contains("LUTEX"))
						{
							TrailingThreshold = 5200;
							ProfitToFund = 8700;
							MaxAutoLiquidate = AccountBeginningValue;
						}	
						
						
						if (IsDayTradersEval || IsDayTradersPA)	
						{
							TrailingThreshold = 4500;
							ProfitToFund = 8500;
						}
						
						if (IsLegendsEval || IsLegendsPA)
						{
							TrailingThreshold = 4000;
							ProfitToFund = 9000;
						}				
						
						if (IsTickTickPA)
						{
							TrailingThreshold = 7500;
							ProfitToFund = 3000;
						}
						
						
					
						
							
					}				
					if (CurrentAccountValue > 239250-LowEndBuffer && CurrentAccountValue < 275000) // $250,000
					{
						TrailingThreshold = 6500;
						ProfitToFund = 15000;
						AccountBeginningValue = 250000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 3000;
						
						
						if (IsBulenoxEval || IsBulenoxPA)	
						{
							TrailingThreshold = 5500;	
						}	
							
						if (IsMFFPA || IsMFFEval || IsFFNPA || IsFFNEval)
						{
							TrailingThreshold = 6000;
							ProfitToFund = 15000;
						}
						
						if (IsTradeDayEval || IsTradeDayPA)
						{
							TrailingThreshold = 5000;
							ProfitToFund = 12000;
						}
						
						if (IsLegendsEval || IsLegendsPA)
						{
							TrailingThreshold = 4500;
							ProfitToFund = 15000;
						}						 
						
						if (IsTickTickPA)
						{
							TrailingThreshold = 12500;
							ProfitToFund = 5000;
						}						
						
						

					}				
					if (CurrentAccountValue > 286500-LowEndBuffer && CurrentAccountValue < 330000) // $300,000
					{
						
						
						TrailingThreshold = 7500;
						ProfitToFund = 20000;
						AccountBeginningValue = 300000;
						MaxAutoLiquidate = AccountBeginningValue + 100;
						ThisMaxPay = 3500;
						
						
						
						if (ThisRowAccount.Contains("LUTBA"))
						{
							TrailingThreshold = 7500;
							ProfitToFund = 17000;
						}	
						
						if (ThisRowAccount.Contains("LUTEX"))
						{
							TrailingThreshold = 7700;
							ProfitToFund = 17200;
						}	
						
						
					}		
					
					// TradeFundrr
					
					if (IsTradeFundrrPA || IsTradeFundrrEval)
					{
						TrailingThreshold = AccountBeginningValue * 0.06;
						ProfitToFund = AccountBeginningValue * 0.10;
					}						
						
						
						
					FundedAmount = AccountBeginningValue + ProfitToFund;
					
					//if (!FirstLoadAccountData)
					Z.MaxPayout = ThisMaxPay;
					
					
					Z.PeakValue = GetAccountSavedData(acct.Name, pAllAccountAutoLiquidate);
					
					
					 
					
					double SlowerNetLiquidation = acct.Get(AccountItem.NetLiquidation, Currency.UsDollar);
					double NetLiquidationForPeak = SlowerNetLiquidation;
					
			
					bool IsTrailAllowed = true;
					
					
					// END OF DAY TRAILING prop firms
					
					bool IsEliteTFEOD = ThisRowAccount.Contains("DHETF") || ThisRowAccount.Contains("EODETF");
					bool IsLifeUpBasic = ThisRowAccount.Contains("LUTBA");
					bool IsLegends = IsLegendsEval || IsLegendsPA;
					bool IsPhidias = IsPhidiasEval || IsPhidiasPA;
								
					if (IsTradeDayEval || IsTradeDayPA || IsTakeProfitTraderEval || IsTakeProfitTraderPA || IsPUREval || IsPURPA || IsMFFEval || IsMFFPA || IsEliteTFEOD || IsTradeifyPA || IsLifeUpBasic || IsLegends || IsPhidias)
					{
						IsTrailAllowed = false;
						if (Z.TotalBought == 0)
							IsTrailAllowed = true;
						
					}
					
					
					
					if (IsTrailAllowed)
					if (NetLiquidationForPeak > Z.PeakValue)
					{
						Z.PeakValue = NetLiquidationForPeak;	
						
					
						SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());
						
					}
					

		

					
					Z.TrailingThre = TrailingThreshold;

					
					if (ResetNowAutoLiquidateAccount != string.Empty)
					if (ResetNowAutoLiquidateAccount == acct.Name)	
					{
						
						Z.PeakValue = Z.NetLiquidation + Z.TrailingThre;

						SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());
						
						
						ResetNowAutoLiquidateAccount = string.Empty;
						
					}	
									
					// STATIC accounts - peak value never changes
					
					if (ThisRowAccount.Contains("STATIC"))
					{
						Z.PeakValue = AccountBeginningValue;
					}
					
					
					
					double allpeak = Z.PeakValue;
		
					
					Z.AudioLiq = Math.Max(allpeak - Z.TrailingThre, AccountBeginningValue - Z.TrailingThre);
					
					if (IsPropFirmPA)
						Z.AudioLiq = Math.Min(Z.AudioLiq, MaxAutoLiquidate);
					
					bool PAMax = IsPropFirmPA && Z.AudioLiq == MaxAutoLiquidate;

	
					
					Z.FromBlown = Math.Round(Z.NetLiquidation - Z.AudioLiq, 2);	

					
					
					
					
					double tddd = 0; 
									
					try
					{	
						// 8.1.2.1 this doesn't work
						
						tddd = Z.Acct.Get(AccountItem.TrailingMaxDrawdown, Currency.UsDollar);
					}
					catch
					{
					
					}
					
					if (tddd != 0 && tddd*-1 != Z.CashValue)
					if (Z.TotalBought == 0)
					{
						double syncpeak = Z.NetLiquidation + TrailingThreshold - tddd;
						
						if (allpeak != syncpeak)
						{
							Z.PeakValue = syncpeak;
							SetAccountSavedData(acct.Name, pAllAccountAutoLiquidate, Z.PeakValue.ToString());	
						}
					}
					
					
					// we don't want to sync this all the time, because NinjaTrader column does not always match Tradovate
					
					tddd = 0;
			

					if (tddd != 0)
						Z.FromBlown = tddd;		
					
					
					
					
						
						//acct.Get(AccountItem.SodLiquidatingValue
					
					if (Z.NetLiquidation != 0)
					{
						if (IsPropFirmEval)
						{
							Z.FundedAmount = FundedAmount;
							Z.FromFunded = Math.Round(Z.FundedAmount - Z.NetLiquidation, 2) + pDollarsExceedFunded;							
						}	
						
						if (IsPropFirmPA && pPAFundedAmouont != 0)
						{
							Z.FundedAmount = AccountBeginningValue + 100 + pPAFundedAmouont;
							Z.FromFunded = Math.Round(Z.FundedAmount - Z.NetLiquidation, 2) + pDollarsExceedFunded;	
						}
					}
					
					
					
				}
				
				//Z.FromFunded = -200;
				

				
				
				Z.OneTradeReady = 0;
				
				//if (IsAccountNoTrades && IsAccountConnected)
				
				
				
				if (Z.TotalBought == 0 && IsConnected)
				{
					
					
					
					bool IsAccountFunded = false;
					
					if (Z.FromFunded - pDollarsExceedFunded <= 0)
						IsAccountFunded = true;
					
					bool FundedOK = IsAccountFunded;
					FundedOK = true;
					
					if (Z.FromFunded == 1000000)
						FundedOK = false;
					
					bool PAs = IsPropFirmPA && pOneTradePAEnabled;
					bool Evals = FundedOK && pOneTradeEvalEnabled;
					

					
					if (PAs || Evals)											
					{
//														//Print("ThishghRowAccount   " + ThisRowAccount);
//														Print("PAs " + PAs);
//														Print("Evals " + Evals);
																				
						Z.OneTradeReady = 1;
						
						TotalActions++;
						

					}
				}
				
				
				
					if (ResetNowDailyGoalAccount != string.Empty)
					if (ResetNowDailyGoalAccount == acct.Name)	
					{
						
						RemoveAccountSavedData(ResetNowDailyGoalAccount, pAllAccountDailyGoal);
						
						if (!IsDailyGoalOrLossConfigured(ResetNowDailyGoalAccount))
							SetAccountData("",ResetNowDailyGoalAccount,"","","","","", "No","","");
						
						ResetNowDailyGoalAccount = string.Empty;
						LastClickedDailyGoalAccount = string.Empty; 
		
						
					}
						
					

					if (ResetNowDailyLossAccount != string.Empty)
					if (ResetNowDailyLossAccount == acct.Name)	
					{
						
						RemoveAccountSavedData(ResetNowDailyLossAccount, pAllAccountDailyLoss);
						
						if (!IsDailyGoalOrLossConfigured(ResetNowDailyLossAccount))
							SetAccountData("",ResetNowDailyLossAccount,"","","","","", "No","","");
						
						ResetNowDailyLossAccount = string.Empty;
						LastClickedDailyLossAccount = string.Empty; 
						
	
					}
					if (ResetNowPayoutAccount != string.Empty)
					if (ResetNowPayoutAccount == acct.Name)	
					{
						
						RemoveAccountSavedData(ResetNowPayoutAccount, pAllAccountPayouts);
						
			
						ResetNowPayoutAccount = string.Empty;
						LastClickedPayoutAccount = string.Empty; 

					}
					
					
					
					
//bool pOnMasterCloseIgnoreFollowers = !pOnMasterGoalHitCloseFollowers;					
					
					// goal closing
					
					
					
					
					
					
					
//					if (pDisableFundedFollowers)
//							{
							
//								double overfunded = Z.FromFunded - pDollarsExceedFunded;
								
//								if (overfunded <= 0)
//								{
								
//									// disable new orders from being placed when flat
									
//									IsFundedOrAtDailyGoalLoss = true;
//								}
							
//							}
							
//							if (pDisableGoalFollowers)
//							{
								
//								if (pColumnDailyGoal || pColumnDailyLoss)
//								{
									
//								//	Print("GetSavedAccountExitSwitch(acct.Name) " + GetSavedAccountExitSwitch(acct.Name) + " " + acct.Name);
									
									
//									double CurrentDailyGoal = 0;
//									double CurrentDailyLoss = 0;
									
									
											 
//									if (pColumnDailyGoal)
//									{							
//										CurrentDailyGoal = GetAccountSavedData(sssss, pAllAccountDailyGoal);	

//										if (CurrentDailyGoal != -1000000000)
//										{
//											if (Z.PNLTotal > CurrentDailyGoal - pDailyGoalLossBuffer)
//											{
//												IsFundedOrAtDailyGoalLoss = true;
//											}
//										}
//									}
											
//									if (pColumnDailyLoss)
//									{										
//										CurrentDailyLoss = GetAccountSavedData(sssss, pAllAccountDailyLoss);
		
//										if (CurrentDailyLoss != -1000000000)
//										{
//											if (Z.PNLTotal < CurrentDailyLoss + pDailyGoalLossBuffer)	
//											{		
//												IsFundedOrAtDailyGoalLoss = true;
//											}
//										}
//									}
//								}
								
								
								
								
					
				Z.HitGoalOrLoss = 0;
				
				if (pIsRiskFunctionsEnabled)
				{
					
					//Print("pIsRiskFunctionsEnabled" + pIsRiskFunctionsEnabled);
					
					double overfunded = Z.FromFunded - pDollarsExceedFunded;
							
						if (pDisableFundedFollowers)

					
								if (overfunded <= 0)
								{
								
									
									
									// disable new orders from being placed when flat
									
									Z.HitGoalOrLoss = 1;
								}
							
					
					//Print(Z.Acct.Name + " 0 " + Z.HitGoalOrLoss);
					
					if (GetSavedAutoClose(acct.Name) == "Yes")
					{
						
						//Print(Z.FromFunded + pDollarsExceedFunded);
						
						if (Z.FromFunded <= 0)
						{
								
							// do NOT flatten follower accounts when master account is flattened
							
							if (acct == CurrentMasterAccount)
								IgnoreExecutionUntilFlat = true;
							
							
									
							FlattenEverything(acct);		
							
							AddMessage("Funded Goal was executed on one or more accounts.  Your accounts should now be flat.", 30, GOLDENROD);	

							AddRejectedEvent(acct.Name, "Funded Hit", "");
							
							// turn off funded / withdraw exit
							
							SetAccountData("",acct.Name,"","","","","","","No","");	
					
							
							
							
							if (acct != CurrentMasterAccount)
							if (pDisableFundedFollowers)
							{
							
//								string clickedaccount = acct.Name;
								
//								AllDuplicateAccounts.Remove(clickedaccount);
//								SetAccountData("",clickedaccount,"None","","","","","","","");
								
								
								
								// remove follower
								
								
							}

							//if (pDisableStrategies)
								FlattenAllStrategies(acct);									
							
							
						}
						
					}

					if (pColumnDailyGoal || pColumnDailyLoss)
					{
						
					//	Print("GetSavedAccountExitSwitch(acct.Name) " + GetSavedAccountExitSwitch(acct.Name) + " " + acct.Name);
						
						double CurrentDailyGoal = 0;
						double CurrentDailyLoss = 0;
						string CurrentDailyStatus = GetSavedAccountExitSwitch(acct.Name);
						
						CurrentDailyGoal = GetAccountSavedData(acct.Name, pAllAccountDailyGoal);	
						CurrentDailyLoss = GetAccountSavedData(acct.Name, pAllAccountDailyLoss);
						
						if (Z.HitGoalOrLoss == 0)
						if (pDisableGoalFollowers)	
						{
							if (pColumnDailyGoal)
							{							
								//CurrentDailyGoal = GetAccountSavedData(acct.Name, pAllAccountDailyGoal);	
	
								if (CurrentDailyGoal != -1000000000)
								{
									if (CurrentDailyStatus == "No")
									if (Z.PNLTotal > CurrentDailyGoal - pDailyGoalLossBuffer)
									{
										Z.HitGoalOrLoss = 2;
									}
								}
							}
									
							if (pColumnDailyLoss)
							{										
								//CurrentDailyLoss = GetAccountSavedData(acct.Name, pAllAccountDailyLoss);
	
								if (CurrentDailyLoss != -1000000000)
								{
									if (CurrentDailyStatus == "No")
									if (Z.PNLTotal < CurrentDailyLoss + pDailyGoalLossBuffer)	
									{		
										if (Z.HitGoalOrLoss == 2)
											Z.HitGoalOrLoss = 23;
										else
											Z.HitGoalOrLoss = 3;
									}
								}
							}
						}
						
					//	Print(Z.Acct.Name + " 1 " + Z.HitGoalOrLoss);
						
						if (CurrentDailyStatus == "Yes")
						{
							if (Z.HitGoalOrLoss != 1)
								Z.HitGoalOrLoss = 0;
							
							//Print(Z.Acct.Name + " 2 " + Z.HitGoalOrLoss);
							
							if (pColumnDailyGoal)
							{
						
								//CurrentDailyGoal = GetAccountSavedData(acct.Name, pAllAccountDailyGoal);	
								
//								Print("CurrentDailyGoal");
//								Print(CurrentDailyGoal + "  " + acct.Name);
//								Print(Z.PNLTotal);
								
								if (CurrentDailyGoal != -1000000000)
								{
									
									
									if (Z.PNLTotal > CurrentDailyGoal)
									{
										
										if (!pOnMasterGoalHitCloseFollowers)
										{
											if (acct == CurrentMasterAccount)
												IgnoreExecutionUntilFlat = true;
										}
										else
										{
											if (pDisableFundedFollowers)
											{
//												NextCloseOrderDisableFollower = true;	
											
//												foreach(string ss in AllDuplicateAccounts)
//													SetAccountData("",ss,"None","","","","","","");
//												AllDuplicateAccounts.Clear();
												
												
											}
											
										}
										
										FlattenEverything(acct);
										
										AddMessage("Daily Goal was executed on one or more accounts.  Your accounts should now be flat.", 30, GOLDENROD);	
										
										AddRejectedEvent(acct.Name, "Daily Goal", "");
										
										// turn off daily goal / loss exit
										
										SetAccountData("",acct.Name,"","","","","","No","","");
			
										if (acct != CurrentMasterAccount)
										if (pDisableGoalFollowers)
										{
										
//											string clickedaccount = acct.Name;
											
//											AllDuplicateAccounts.Remove(clickedaccount);
//											SetAccountData("",clickedaccount,"None","","","","","","","");
											
											
											
											// remove follower
											
											
										}
							
										
										
										//if (pDisableStrategies)
											FlattenAllStrategies(acct);
											
										
											
										
									}
									
									
									
								}
							}
							
							
							if (pColumnDailyLoss)
							{
								
								//CurrentDailyLoss = GetAccountSavedData(acct.Name, pAllAccountDailyLoss);
								
//								Print("CurrentDailyLoss");
//								Print(CurrentDailyLoss + "  " + acct.Name);
//								Print(Z.PNLTotal);
								
								if (CurrentDailyLoss != -1000000000)
								{
									
									//if (Z.PNLTotal < -1*CurrentDailyLoss)
									if (Z.PNLTotal < CurrentDailyLoss)	
									{
										
										if (!pOnMasterGoalHitCloseFollowers)
										{
											if (acct == CurrentMasterAccount)
												IgnoreExecutionUntilFlat = true;
										}
										else
										{
											if (pDisableFundedFollowers)
											{
//												NextCloseOrderDisableFollower = true;	
										
									
//												foreach(string ss in AllDuplicateAccounts)
//													SetAccountData("",ss,"None","","","","","","");
//												AllDuplicateAccounts.Clear();
												
												
											}
											
										}
							
										FlattenEverything(acct);		
										
										AddMessage("Daily Loss was executed on one or more accounts.  Your accounts should now be flat.", 30, GOLDENROD);	
										
										AddRejectedEvent(acct.Name, "Daily Loss", "");
										
										// turn off daily goal / loss exit
										
										SetAccountData("",acct.Name,"","","","","","No","","");
			
										if (acct != CurrentMasterAccount)
										if (pDisableGoalFollowers)
										{
										
//											string clickedaccount = acct.Name;
											
//											AllDuplicateAccounts.Remove(clickedaccount);
//											SetAccountData("",clickedaccount,"None","","","","","","","");
											
											
											
											// remove follower
											
											
										}
										
		
										//if (pDisableStrategies)
											FlattenAllStrategies(acct);									
										
										//Print("hey loss");
										
									}								
									
									
								}
							}
								
							
						}
					
						
						
					}
				}

					if (FlattenEverythingClicked)
					{
						//if (acct == CurrentMasterAccount)
						//	IgnoreExecutionUntilFlat = true;	
						  
						FlattenEverything(acct);
						CancelAllOrders(acct);
						
						
						
						//continue;
					}
					
					if (RefreshPositionsClicked)
					{
						if (RefreshPositionsClickedOrders == 1) // only submit order for account that has a position
						{
							Instrument OneTradeInstrument = GetTheInstrument(Instrument.FullName, "Micro", false);
							
							//BuyLimitOne(1, OneTradeInstrument, acct);	
						}
						
					}				
					
					
									
				
	
				
				
								//Print(accccnnnn+"DDSD3");
				
				if (!FinalPNLStatistics.ContainsKey(accccnnnn))
					FinalPNLStatistics.Add(accccnnnn, Z);
				
			
			}

			//	Print(accccnnnn+"DDSD4");
				
			SelectAllAccounts = false;

//			if (RemoveATMCheck.Count != 0)
//			Print("RemoveATMCheck.Count: " + RemoveATMCheck.Count);
			
			foreach (string sss in RemoveATMCheck)
			{
				//Print("Removing RemoveATMCheck" + sss);
				FilledMasterAccountATMs.Remove(sss);
			}

			if (pEnabledTradingViewFix)
			{
				
				if (ResubmitTradingViewMove2)
				{
					
					//MoveOrdersAtPrice2(MovePrice,NewPrice);
			
					ResubmitTradingViewMove2 = false;
					
				}
								
				
				
				if (ResubmitTradingViewMove)
				{
					
								//MovePrice = asfsdgdf;
								//NewPrice = thisp;
					
					
					//Print("DOING ResubmitTradingViewMove");
					
					//MoveOrdersAtPrice2(MovePrice,NewPrice);
					
					ResubmitTradingViewMove = false;
					ResubmitTradingViewMove2 = true;
					
				}
				
			}
					
							// remove junk atm strategies - this could resolve the issue below					
							// Atm strategy - when increasing -1-2-3-4 after trade has been closed, this is when it begins to not work
							// not needed anymore

					
			if (TotalPositions > 0 && TotalPositionsPrev == 0)
			{
				PositionsZeroStartTime = DateTime.MinValue;
				
				//Print(PositionsZeroStartTime);
			}
				
			if (TotalPositions == 0 && TotalPositionsPrev != 0)
			{
				PositionsZeroStartTime = DateTime.Now.AddSeconds(5);
				
				//Print(PositionsZeroStartTime);
			}
						
			
			
			if (pRemoveClosedATMStrategies)
			if (DateTime.Now.Ticks < PositionsZeroStartTime.Ticks)
			{
				//Print("Running");
				
				if (pCopierMode == "Orders")
				{

						TotalStrategies = 0;
						
					
						foreach (Account acct in Account.All)
						{
						
					
							
							
							//BC.Acct.Strategies.Clear();
							
							bool ormaster = acct == CurrentMasterAccount && pResubmitMaster;
							
							
							if (ormaster || AllDuplicateAccounts.Contains(acct.Name))
							if (acct.Strategies.Count > 0)
							{
								//Print("ACCOUNT: " + acct.Name);	
								
								//Print("POS: " + acct.Positions.Count);	
								
							//	bool NoCurrentPositions = acct.Positions.Count == 0;
								
								int slot = 0;
								int slotremove = -1;
								
								
								
								AtmStrategy FinalATMStrategy2 = null;
							
								string eventt = acct.Name;
								
								foreach (object aaa in acct.Strategies)
								{								
							
									if (aaa.GetType() == typeof(AtmStrategy))
										FinalATMStrategy2 = (AtmStrategy) aaa;	
								
										bool AllFlat = true;
										bool TimeQ = true;
									
										if (FinalATMStrategy2 != null)
										{
										
											//Print(FinalATMStrategy2.DisplayName);
											
											
											foreach (Position PP in FinalATMStrategy2.Positions)
											{
//												Print(PP.MarketPosition);
//												Print(PP.Quantity);
													
													if (PP != null)
												if (PP.MarketPosition != MarketPosition.Flat)
												{
													AllFlat = false;
													
													//if (Notargets)
													
													//NinjaTrader.NinjaScript.AtmStrategy.ProtectPosition(FinalATMStrategy2, PP);
													
												}
												
												
											}
											
											//Print("FinalATMStrategy2.Executions.Count " + FinalATMStrategy2.Executions.Count);
											
											
											if (FinalATMStrategy2.Executions.Count == 0)
												TimeQ = false;
										

											int brak = 1;
											
											if (TimeQ)
											{
												//TimeQ = true;
												
												foreach( Bracket bb in FinalATMStrategy2.Brackets)
												{
													
													if (FinalATMStrategy2.GetTargetOrders(brak).Count != 0 || FinalATMStrategy2.GetStopOrders(brak).Count != 0)
													TimeQ = false;
													
													brak++;
												}
											}
											
											
											foreach (Execution EE in FinalATMStrategy2.Executions)
											{
												//Print(EE.Time);
												if (DateTime.Now.Ticks < EE.Time.AddSeconds(5).Ticks)
												{
													TimeQ = false;
												}

												break;		
											}
											
											//Print(AllFlat);
											//Print(TimeQ);
											
//											if (NoCurrentPositions)
//												AllFlat = true;
											
											if (AllFlat && TimeQ)
											{
												eventt = acct.Name + " " + FinalATMStrategy2.DisplayName;
												slotremove = slot;	
												
												TotalStrategies = TotalStrategies + 1;
												
												//Print("Removing ATM " + eventt);
												//FinalATMStrategy2.AtmStrategyClose(FinalATMStrategy2.GetAtmStrategyUniqueId());
											}
										}
										slot++;
								}			

								if (slotremove != -1)
								{				
									//Print("Removing ATM " + eventt);
									acct.Strategies.RemoveAt(slotremove);
									
								}
								
								if (TotalStrategies == 0)
								{
									//Print("TotalStrategies == 0");
									PositionsZeroStartTime = DateTime.MinValue;
								}
							}
						}
				}
			}
									
			
			TotalPositionsPrev = TotalPositions;
			
			if (TotalDisplayAccounts == 0)
			{
				//Print("total 0");
				AddMessage("Please make sure at least one connection is active to get started.  In the NinjaTrader Control Center, Connections Menu, select one of your connections.", 10000, RED);
			}
			
			
			int totalaccountsmasterandfollowers = AllDuplicateAccounts.Count + 1;	
			
		//Print("_1");
			
			if (pCopierIsEnabled)
			if (pSyncLimitEntry)
			if (pCopierMode == "Orders")
			{
			
					double remove = 0;

				
				
				
				//if (DateTime.Now > MoveOClicked)
				{
						foreach (KeyValuePair<double, PendingDetails> kvp in LimitPriceToTotalOrders2)
							{
								

								if (totalaccountsmasterandfollowers != kvp.Value.Width) // if total accounts in copier doesn't equal total orders at a price
								{
									
									bool includestarget = false;
									
									foreach (string sss in kvp.Value.AllOrderNames)
									{
										if (sss.Contains("Target") || sss.Contains("Stop"))
											includestarget = true;
									}
									
									
									if (!includestarget)
									{
									
										if (!LimitPriceToTimeOutOfSync2.ContainsKey(kvp.Key))
										{
											LimitPriceToTimeOutOfSync2.Add(kvp.Key, kvp.Value);
										
											LimitPriceToTimeOutOfSync2[kvp.Key].StartTime = DateTime.Now;
										}
										else
										{
											LimitPriceToTimeOutOfSync2[kvp.Key].Width = kvp.Value.Width;
											LimitPriceToTimeOutOfSync2[kvp.Key].AllInstruments = kvp.Value.AllInstruments;	
											LimitPriceToTimeOutOfSync2[kvp.Key].AllOrderNames = kvp.Value.AllOrderNames;	
											LimitPriceToTimeOutOfSync2[kvp.Key].AllOrderTypes = kvp.Value.AllOrderTypes;
										}
									}
									else
									{
										// if total target 1 across accounts = total positions across accounts    AND    all targets 1 prices are the same    AND    total does not equal totalaccountsmasterandfollowers
											
									}
								
								}
								else
								{
									
									remove = kvp.Key;
									
								}
								
								
							}
														
							
				if (remove != 0)
				{
					
					//Print(remove);
					
					if (LimitPriceToTimeOutOfSync2.ContainsKey(remove))
						LimitPriceToTimeOutOfSync2.Remove(remove);
					
				}
								
							

				
								
				foreach (KeyValuePair<double, PendingDetails> kvp in LimitPriceToTimeOutOfSync2)
				{
					if (!LimitPriceToTotalOrders2.ContainsKey(kvp.Key))
						remove = kvp.Key;

				}
				
				
				
				if (remove != 0)
				{
					
					//Print(remove);
					
					if (LimitPriceToTimeOutOfSync2.ContainsKey(remove))
						LimitPriceToTimeOutOfSync2.Remove(remove);
				}
				
				//RemoveMessage("Pending Orders Out Of Sync");
				
				}
				
				
				
				
				
				
			}
							
			
			FirstLoadAccounts = false;
			
			
			bool looppppp = false;
			
			
			
			//if (looppppp)
			
			//Print( "--------------------");
			
			IsOneTotal = false;
			
			//#region -- If show total rows and FinalPNLStatistics
			if (pShowTotalRows)
			foreach (KeyValuePair<string, PNLStatistics> kvp in FinalPNLStatistics)
							{
							
								
								//Print(kvp.Key);
								
								
								
								PNLStatistics ZZZ = kvp.Value;
								
//								ZZZ.AverageLongPrice = 0;
//								ZZZ.AverageShortPrice = 0;
//								ZZZ.TotalMoney = 0;
								
								//Print(ZZZ.Acct.Name);
								
								
									
//									ZZZ.TotalBought = ZZZ.TotalLongQty;
//									ZZZ.TotalSold = ZZZ.TotalShortQty;
							
									
									
									
									
									
									// account toals
									
									
									//string accname = ZZZ.Acct.Name;
								
									
									string accname = ZZZ.PrivateName;								
								
								// Louis Buono Jr
								// same as above
								
									//string accname = PrivateAccountName(ZZZ.Acct.Name);
								
								
								
									string accname2 = "";
								
								
								
									PNLStatistics Y = new PNLStatistics();
								
								
								
								
									bool Total1Condition = accname == "330690";								
									bool Total2Condition = accname.Contains("APEX");									
									bool Total3Condition = true;
								
									bool Total5Condition = true;
									bool Total6Condition = true;
									bool Total7Condition = true;
								

								
								
									string Filter1 = pTotal1Filter;
									string Filter2 = pTotal2Filter;
									string Filter3 = pTotal3Filter;
								
									//string Filter4 = pTotal4Filter;
								
									string Filter5 = pTotal5Filter;
									string Filter6 = pTotal6Filter;
									string Filter7 = pTotal7Filter;
							
								
//									if (pTotal1Filter == "Live")
//										Total1Condition = ZZZ.Acct.Provider != Provider.Simulator;

//									if (pTotal1Filter == "Simulator")
//										Total1Condition = ZZZ.Acct.Provider == Provider.Simulator;

									
									if (Filter1 == string.Empty)
										Total1Condition = false;
									else
										Total1Condition = accname.Contains(Filter1);
																			
									if (Filter2 == string.Empty)
										Total2Condition = false;
									else
										Total2Condition = accname.Contains(Filter2);
									
									if (Filter3 == string.Empty)
										Total3Condition = false;
									else
										Total3Condition = accname.Contains(Filter3);
									
									if (Filter5 == string.Empty)
										Total5Condition = false;
									else
										Total5Condition = accname.Contains(Filter5);
									
									if (Filter6 == string.Empty)
										Total6Condition = false;
									else
										Total6Condition = accname.Contains(Filter6);
									
									if (Filter7 == string.Empty)
										Total7Condition = false;
									else
										Total7Condition = accname.Contains(Filter7);
									
									
								
//									Total1Name = pTotal1Filter;
//									Total2Name = pTotal2Filter;
//									Total3Name = pTotal3Filter;
//									Total4Name = "Grand Total Display";
								
									
									
									
									
									
									Total1Name = pTotal1Name;
									Total2Name = pTotal2Name;
									Total3Name = pTotal3Name;
									Total4Name = pTotal4Name;
									Total5Name = pTotal5Name;
									Total6Name = pTotal6Name;
									Total7Name = pTotal7Name;									
									
									if (pTotal1Name == string.Empty)
										Total1Name = pTotal1Filter;
									if (pTotal2Name == string.Empty)
										Total2Name = pTotal2Filter;
									if (pTotal3Name == string.Empty)
										Total3Name = pTotal3Filter;
									
									if (pTotal4Name == string.Empty)
										Total4Name = "Total";
									
									if (pTotal5Name == string.Empty)
										Total5Name = pTotal5Filter;
									if (pTotal6Name == string.Empty)
										Total6Name = pTotal6Filter;
									if (pTotal7Name == string.Empty)
										Total7Name = pTotal7Filter;									
									
									if (IsKorean)
									{
									
										Total4Name = "계좌합산";
										
									}
									
//									Print("Total1Condition  " + Total1Condition);
//									Print("Total2Condition  " + Total2Condition);
//									Print("Total3Condition  " + Total3Condition);
									
									
									
//									IsOneTotal = Total1Condition || Total2Condition || Total3Condition;
									
									
									
									bool Total4Condition = true;
								
								
									if (!pShowGrandTotal)
										Total4Condition = false;
	
									
									
									AllTotalRows = new SortedList<string, bool>();	
									
									AllTotalRows.Add("TotalRow1", Total1Condition);
									AllTotalRows.Add("TotalRow2", Total2Condition);
									AllTotalRows.Add("TotalRow3", Total3Condition);								
									AllTotalRows.Add("TotalRow5", Total5Condition);
									AllTotalRows.Add("TotalRow6", Total6Condition);
									AllTotalRows.Add("TotalRow7", Total7Condition);
																	
									AllTotalRows.Add("TotalRow4", Total4Condition);
									
									
									
									
									
									
									
									
										foreach (KeyValuePair<string, bool> aaaaa in AllTotalRows.ToList())
										{
											

											if (aaaaa.Value)
											{
												// first total
											
												IsOneTotal = true;
												
											
												accname2 = aaaaa.Key;
											
												Y = new PNLStatistics();
												Y.Inst = null;
												Y.LastEntry = 1;
												Y.LastDir = 1;
												Y.TotalLongPrices = 0;
												Y.TotalLongQty = 0;
												Y.TotalShortPrices = 0;
												Y.TotalShortQty = 0;
												Y.LastPosition = 0;
												Y.TotalAccounts = 0;
												Y.OneTradeReady = 0;
												
												
												//Y.Acct = CurrentMasterAccount;
												Y.Acct = null;
												//Y.Commish = ZZZ.Commish;
												
					
											
												if (!TotalAccountPNLStatistics.ContainsKey(accname2))
												{
													TotalAccountPNLStatistics.Add(accname2, Y);
												}
												else
												{
													Y = TotalAccountPNLStatistics[accname2];
		
												}							
																			
												
												
												Y.TotalAccounts = Y.TotalAccounts + 1;
																				
		
												Y.PNLTotal = Y.PNLTotal + ZZZ.PNLTotal;
												Y.PNLGR = Y.PNLGR + ZZZ.PNLGR;
												Y.PNLR = Y.PNLR + ZZZ.PNLR;
												Y.PNLU = Y.PNLU + ZZZ.PNLU;
												Y.PNLQ = Y.PNLQ + ZZZ.PNLQ;
												
												
												Y.TotalBought = Y.TotalBought + ZZZ.TotalBought;
												Y.TotalSold = Y.TotalSold + ZZZ.TotalSold;
											
											
												Y.CashValue = Y.CashValue + ZZZ.CashValue;
												Y.NetLiquidation = Y.NetLiquidation + ZZZ.NetLiquidation;
												Y.PeakValue = Y.PeakValue + ZZZ.PeakValue;
												
												Y.Commish = Y.Commish + ZZZ.Commish;
											
												Y.PositionLong = Y.PositionLong + ZZZ.PositionLong; 	
												Y.PositionShort = Y.PositionShort + ZZZ.PositionShort; 
												
												//Z.AllPositions.Add(pp);
												
												Y.PositionMicroLong = Y.PositionMicroLong + ZZZ.PositionMicroLong; 	
												Y.PositionMicroShort = Y.PositionMicroShort + ZZZ.PositionMicroShort; 										
												
												
												Y.PendingEntry = Y.PendingEntry + ZZZ.PendingEntry; 	
												Y.PendingExit = Y.PendingExit + ZZZ.PendingExit; 										
											
												Y.FrozenOrders = Y.FrozenOrders + ZZZ.FrozenOrders; 
				
												Y.Signals = accname2;
												
		//										if (CurrentMasterAccount == ZZZ.Acct)
		//											Y.Acct = CurrentMasterAccount;
												
												bool IsAccountFunded = false;
												bool IsAccountIsPropFirmPA = false;
												bool IsAccountNoTrades = false;
												bool IsOneTradeReady = false;
												bool IsDisconnected = kvp.Value.Acct.ConnectionStatus == ConnectionStatus.Disconnected;
														
												IsDisconnected = AllConnectedAccounts.Contains(kvp.Value.Acct.Name);	
														
												if (kvp.Value.Acct.Name.Contains("PA-APEX") || kvp.Value.Acct.Name.Contains("PAAPEX") || kvp.Value.Acct.Name.Contains("PA-LL"))
													IsAccountIsPropFirmPA = true;
														
														if (ZZZ.FromFunded - pDollarsExceedFunded <= 0)
															IsAccountFunded = true;
																		
																	
																		
														if (ZZZ.TotalBought == 0)
															IsAccountNoTrades = true;
																
												bool FundedOK = IsAccountFunded;
												FundedOK = true;
														
												if (ZZZ.FromFunded == 1000000)
													FundedOK = false;							
														
												bool PAs = IsAccountIsPropFirmPA && pOneTradePAEnabled;
												bool Evals = FundedOK && pOneTradeEvalEnabled;
							
				
														
												IsOneTradeReady = (PAs || Evals) && IsAccountNoTrades && IsDisconnected;
												
										
												if (IsOneTradeReady)		
													Y.OneTradeReady	= Y.OneTradeReady + 1;		
												
											}
												
										}
							}
			//#endregion
		}
		//#endregion
		
		
		private string GetSpeakFromDescription(string ss)
		{
		
			string fullname = ss;
			
			fullname = fullname.Replace(" 500", "");
			fullname = fullname.Replace(" 100", "");
			fullname = fullname.Replace(" 100", "");
			fullname = fullname.Replace("E-mini", "");
			fullname = fullname.Replace(" E-mini", "");
			fullname = fullname.Replace(" Futures", "");
			
			fullname = fullname.Replace(" 2000", "");
			fullname = fullname.Replace(" Index", "");
			
			fullname = fullname.Replace("Nasdaq-100", "NASDAQ"); // MNQ
			fullname = fullname.Replace("E-micro", "Micro"); // MNQ
			fullname = fullname.Replace("($5)", ""); // YM						
			
			fullname = fullname.Replace("10-Year T-Note", "ZN"); // ZN
			 fullname = fullname.Replace("U.S. Treasury Bond", "ZB"); // ZB 

  

			
			
			return fullname;
			
		}
		private string GetSpeakFromSymbol(string ss)
		{
		
			
			string fullname = ss;
			
			if (ss == "NQ")
				fullname = "NASDAQ";
			
			if (ss == "ES")
				fullname = "S AND P";	
			
			if (ss == "YM")
				fullname = "DOW";
			
			if (ss == "CL")
				fullname = "Crude Oil";
			
			if (ss == "GC")
				fullname = "Gold";	
			
			if (ss == "GC")
				fullname = "Gold";	
			
			if (ss == "GC")
				fullname = "Gold";	
			
			return fullname;
			
		}

		private bool IsCurrentBar = false;
		
		
		//#region -- OnTimerTick --
		private void OnTimerTick4(object sender, EventArgs e)
		{
			
			if (DateTime.Now > SelectedMultiplerTime)
			{
				
				if (SelectedMultiplerTime != DateTime.MinValue)
				{
					SelectedMultiplierAccount = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedMultiplerTime = DateTime.MinValue;				
			}

			if (DateTime.Now > SelectedATMTime)
			{
				
				if (SelectedATMTime != DateTime.MinValue)
				{
					SelectedATMAccount = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedATMTime = DateTime.MinValue;				
			}

			if (DateTime.Now > SelectedDailyGoalTime)
			{
				
				if (SelectedDailyGoalTime != DateTime.MinValue)
				{
					SelectedDailyGoalAccount = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedDailyGoalTime = DateTime.MinValue;
			}			
			
			
			if (DateTime.Now > SelectedDailyLossTime)
			{
				
				if (SelectedDailyLossTime != DateTime.MinValue)
				{
					SelectedDailyLossAccount = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedDailyLossTime = DateTime.MinValue;
			}	
			
			if (DateTime.Now > SelectedPayoutTime)
			{
				
				if (SelectedPayoutTime != DateTime.MinValue)
				{
					SelectedPayoutAccount = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedPayoutTime = DateTime.MinValue;
			}

			if (DateTime.Now > SelectedResetTime)
			{
				
				if (SelectedResetTime != DateTime.MinValue)
				{
					SelectedResetColumn = string.Empty;
					SelectedButtonNowPre = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedResetTime = DateTime.MinValue;
			}				
				
			if (DateTime.Now > SelectedButtonTime)
			{
				
				if (SelectedButtonTime != DateTime.MinValue)
				{
					SelectedButtonNow = string.Empty;
					SelectedButtonNowPre = string.Empty;
					ChartControl.InvalidateVisual();
				}
				
				SelectedButtonTime = DateTime.MinValue;
			}	

			if (DateTime.Now > RemoveMessageTime)
				RemoveDrawObject("error");
			
			//Print("tick");
			
			
//			if 
			
		}
			
		
		bool StartEndingNewOrders = false;
		int NextOrderID = 0;
		
		private void OnTimerTick2 (object sender, EventArgs e)
		{
		
			// hello
		 
//			if (ChartControl != null)
//			{
//				if (isActiveTab(ChartControl))
//				{
//					btnDrawObjs.Visibility = Visibility.Visible;	
//					btnShowTrades.Visibility = Visibility.Hidden;
//				}
//				else
//				{
//					btnDrawObjs.Visibility = Visibility.Collapsed;
//					btnShowTrades.Visibility = Visibility.Collapsed;
//				}
//			}		
				
			
			return;
			
						
		
				
		
		
		}

		private bool IsInActiveWorkspace = false;			

		private void OnTimerTick3(object sender, EventArgs e)
		{
			
			//
			
//			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
//                                            CultureInfo.InvariantCulture);
			
			
//			Print("OnTimerTick3: " + timestamp);
						
			
			if (pShowStatusMessages)				
			if (AllMessages.Count > 0)
			foreach (MessageAlerts D in AllMessages.ToList())
				{
					
					if (DateTime.Now >= D.ExpireTime)
					{
						//Print("hefdsg");
						
						D.Switch = false;
						ChartControl.InvalidateVisual();
					}
						
				}
				
				
				
			IsInActiveWorkspace = WorkspaceOptions.GetActiveWorkspaceFromXml == chartWindow.GetWorkspaceName();
				
				
				
				
//			Print("WorkspaceOptions " + WorkspaceOptions.GetActiveWorkspaceFromXml);
				
//			Print("chartWindow.GetWorkspaceName() " + chartWindow.GetWorkspaceName());

				
			if (!NoAccountsConnected)
			timer3.Dispatcher.InvokeAsync(() =>
			{
				
				
				GetAllPerformanceStats();
				
				
			});
					
			return;
			
			

						
//				TriggerCustomEvent(o =>
//   				{
					
//						}, null);	
			
			
			
//			if (SoundAlerts.Count > 0)
//			{
				
//				string sss = SoundAlerts[0];
				
//				PlaySound(sss);
				
//				SoundAlerts.Remove(sss);
				
//			}
			
			
			
//			if (SpeakAlerts.Count > 0)
//			{
				
//				string sss = SpeakAlerts[0];
				
							
//				var synthesizer = new SpeechSynthesizer();
//			     synthesizer.SelectVoice(pVoiceName);
	  
	  
//				//synthesizer.prom
//								    synthesizer.SetOutputToDefaultAudioDevice();
//								    synthesizer.Speak(sss);
				
//				SpeakAlerts.Remove(sss);
				
//			}
			
			
			
			
		
			
			
		}
		//#endregion
	
		private bool ResubmitTradingViewMove = false;
		private bool ResubmitTradingViewMove2 = false;
		
		
 		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
			
			
				
					ChartControl.Dispatcher.InvokeAsync(() =>
						{
							
							//Print("hey");
							
							GetAllPerformanceStats();
							ChartControl.InvalidateVisual();
							
						});
				

		
			
//			//return;
			
//			//Print("OnConnectionStatusUpdate");
			
//			//ChartControl.Dispatcher.InvokeAsync(() =>
//			//{
			

//			//Print(connectionStatusUpdate.PriceStatus);
			
//				if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Connected
//					&& connectionStatusUpdate.Connection.InstrumentTypes.Contains(ThisMasterInstrument.InstrumentType)
					
					
////					&& Bars.BarsType.IsTimeBased
////					&& Bars.BarsType.IsIntraday
					
//					)
//				{
					
					
//					// one second
					
//					if (pBarCounterEnabled)
//					if (Bars.BarsType.IsTimeBased && Bars.BarsType.IsIntraday)
//					if (DisplayTime() && timer == null)
//					{
//						//Print("timer start");
						
//						ChartControl.Dispatcher.InvokeAsync(() =>
//						{ 
//							timer			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50), IsEnabled = true };
//							timer.Tick		+= OnTimer1Tick;
//						});
//					}
											
										
						

										
					
//					connected = true;

//				}
//				else if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Disconnected)
//				{
//					connected = false;
			
					
//					//MarketDataOK = false;
//				}
				
				
				
				
			//});
		}

		private bool MarketDataOK = false;

		protected override void OnMarketData (MarketDataEventArgs e)
		{
		
//			Print("is data");
			
//			Print(e.MarketDataType);
			
			if (e.MarketDataType == MarketDataType.Ask)
			if (!MarketDataOK)
			{
				//Print("is market data");
		
				MarketDataOK = true;
				
			}
					
			
		}

		protected override void OnBarUpdate()
		{

			//Print(Times[1][0]);
			
			// secondary data series to reset pnl
			
			int dddddd = BarsArray.Length - 1;
			
			if (CurrentBars[dddddd] > 0)
			if (BarsArray[dddddd].IsFirstBarOfSession)
				{
					if (pDisableOnNewSession)
					if (State == State.Realtime)
					{
					
						pCopierIsEnabled = false;
						
					}
						
					
					//if (CurrentBars[0] > 0)
					LastSessionStart = Times[dddddd][1];
					
					//Print(LastSessionStart);
				}	
				
				
				
				return;
			
			
			
			
			
			
			
				// reset pnl at first bar of session
			
				if (BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Day)
				{
					LastSessionStart = Times[0][0];
				}
				else if (BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Week)
				{
					LastSessionStart = Times[0][0];
				}			
				else if (BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Month)
				{
					LastSessionStart = Times[0][0];
				}	
				else if (BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Year)
				{
					LastSessionStart = Times[0][0];
				}					
				else if (BarsArray[0].IsFirstBarOfSession)
				{
					if (CurrentBars[0] > 0)
					LastSessionStart = Times[0][1];
					
				}	
	
		
						
			//CurrentLastData = Close[0];
						
			//ThisCurrentBarNow = CurrentBars[0];
		
			//IsCurrentBar = BarsArray[0].Count == CurrentBars[0]+1;
			//IsCurrentBar = State == State.Realtime;
			
			

			
            

		}

		public override string DisplayName
		{
			get
			{
					if (State == State.SetDefaults)
						return ThisName;
					else
						return Name;
			}
		
		}		

        private double RTTS(Instrument iii, double p)
        {
            return iii.MasterInstrument.RoundToTickSize(p);
        }

		private bool isActiveTab(ChartControl cControl)
		{
			if(!cControl.Properties.AreTabsVisible)
			{
				return true;
			}
			
			bool isActive = false;
			
			NinjaTrader.Gui.Chart.Chart	cWindow = System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
			
	
			
			if (cWindow != null)
			foreach(System.Windows.Controls.TabItem tab in cWindow.MainTabControl.Items)
			{
				if((tab.Content as ChartTab).ChartControl == ChartControl && tab == cWindow.MainTabControl.SelectedItem)
				{
					isActive = true;
					break;
				}
			}
			
			return isActive;
		}

		private int RED25 = 10;
		private int RED20 = 20;
		private int RED = 30;
		private int LIME = 40;
		private int LIME30 = 50;
		private int LIME60 = 55;
		private int LIME80 = 60;
		private int GOLDENROD = 70;
		private int DODGERBLUE18 = 80;
		private int DODGERBLUE30 = 81;
		private int DODGERBLUE = 90;
		private int WHITE = 100;
		private int SILVER = 110;
		private int SILVER70 = 120;
		private int SILVER40 = 130;
		private int BLACK = 140;
		private int ChartTextID = 150;
		private int ChartBackgroundID = 160;
		private int pCompMainColorID = 170;
		private int TableBackgroundID = 171;
		private int TableHighlight1ID = 172;
		private int pButtonOffColorID = 180;
		private int pCopierButtonOn2ID = 190;
		private int pLockButtonOnID = 200;
		private int pFlattenButtonBID = 210;
		private int pRefreshButtonBID = 220;
		private int pBackMasterAccountColorID = 230;
		private int pBackSlaveAccountColorID = 240;
		private int pIsFundedColorID = 250;
		private int pBackBuyColorID = 260;
		private int pBackSellColorID = 270;
		private int pTrailingGoodColorID = 290;
		private int pTrailingWarningColorID = 300;
		private int pTrailingBadColorID = 310;
		private int pTrailingBlownColorID = 320;
		private int pColorTextID = 330;
		private int pConnectedOtherID = 340;
		private int pColorTextBrushID = 350;
		private int pPositivePnLTextColorID = 360;
		private int pNegativePnLTextColorID = 361;
		private int pConnectedOnID = 370;
		private int pConnectedOffID = 380;
		private int pConnectedLostID = 390;
		private int pOrderDnOutlineStrokeID = 400;

		private int textID = 0;
		private int ThisBrushID = 0;
		private int ChartBackgroundErrorBrushID = 0;
		private int TableFinalBackgroundID = 0;
		private int StatusButtonsID = 0;
		private int ChartBackgroundFadeID = 0;

		string olds = "";
		private void p(string s){if(s!=olds) {Print(s); olds=s;}}
		private void p(int n){Print(n.ToString());}
		public override void OnRenderTargetChanged()
		{	
			if (RenderTarget != null)
			{
				pOrderDnOutlineStroke.RenderTarget = RenderTarget;
			}
			try
			{
				if(br.Count==0){
					br[RED25] = new DXMediaMap(Brushes.Red, 0.25f);
					br[RED20] = new DXMediaMap(Brushes.Red, 0.2f);
					br[RED] = new DXMediaMap(Brushes.Red);
					br[LIME] = new DXMediaMap(Brushes.Lime);
					br[LIME30] = new DXMediaMap(Brushes.Lime,.3f);
					br[LIME60] = new DXMediaMap(Brushes.Lime,.6f);
					br[LIME80] = new DXMediaMap(Brushes.Lime,.8f);
					br[GOLDENROD] = new DXMediaMap(Brushes.Goldenrod);
					br[DODGERBLUE18] = new DXMediaMap(Brushes.DodgerBlue, 0.18f);
					br[DODGERBLUE30] = new DXMediaMap(Brushes.DodgerBlue, 0.30f);
					br[DODGERBLUE] = new DXMediaMap(Brushes.DodgerBlue);
					br[WHITE] = new DXMediaMap(Brushes.White);
					br[SILVER] = new DXMediaMap(Brushes.Silver);
					br[SILVER70] = new DXMediaMap(Brushes.Silver, 0.7f);
					br[SILVER40] = new DXMediaMap(Brushes.Silver, 0.4f);
					br[BLACK] = new DXMediaMap(Brushes.Black);

					br[ChartTextID] = new DXMediaMap();
					br[ChartBackgroundID] = new DXMediaMap();
					br[pCompMainColorID] = new DXMediaMap(pCompMainColor, pCompMinOpacity/100f);
					br[TableBackgroundID] = new DXMediaMap(pCompMainColor, pCompMinOpacity/100f);
					br[TableHighlight1ID] = new DXMediaMap(pCompMainColor, pCompMinOpacity/100f);
					br[pButtonOffColorID] = new DXMediaMap(pButtonOffColor, pButtonOffOpacity/100f);
					br[pCopierButtonOn2ID] = new DXMediaMap(pCopierButtonOn2);
					br[pLockButtonOnID] = new DXMediaMap(pLockButtonOn);
					br[pFlattenButtonBID] = new DXMediaMap(pFlattenButtonB);
					br[pRefreshButtonBID ] = new DXMediaMap(pRefreshButtonB);
					br[pBackMasterAccountColorID] = new DXMediaMap(pBackMasterAccountColor, pCompMinOpacityHAcc/100f);
					br[pBackSlaveAccountColorID] = new DXMediaMap(pBackSlaveAccountColor, pCompMinOpacityHAcc/100f);
					br[pIsFundedColorID] = new DXMediaMap(pIsFundedColor);
					br[pBackBuyColorID] = new DXMediaMap(pBackBuyColor);
					br[pBackSellColorID] = new DXMediaMap(pBackSellColor);
					br[pTrailingGoodColorID] = new DXMediaMap(pTrailingGoodColor);
					br[pTrailingWarningColorID] = new DXMediaMap(pTrailingWarningColor);
					br[pTrailingBadColorID] = new DXMediaMap(pTrailingBadColor);
					br[pTrailingBlownColorID] = new DXMediaMap(pTrailingBlownColor);
					br[pColorTextID] = new DXMediaMap(pColorTextBrush);
					br[pConnectedOtherID] = new DXMediaMap(pConnectedOther);
					br[pColorTextBrushID] = new DXMediaMap(pColorTextBrush);
					br[pPositivePnLTextColorID] = new DXMediaMap(pPositivePnLTextColor);
					br[pNegativePnLTextColorID] = new DXMediaMap(pNegativePnLTextColor);
					br[pConnectedOnID] = new DXMediaMap(pConnectedOn);
					br[pConnectedOffID] = new DXMediaMap(pConnectedOff);
					br[pConnectedLostID] = new DXMediaMap(pConnectedLost);
					br[pOrderDnOutlineStrokeID] = new DXMediaMap();
				}
				
				foreach (KeyValuePair<int, DXMediaMap> item in br)
				{
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();

					if (RenderTarget != null && item.Value.MediaBrush!=null){
						item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);		
						item.Value.DxBrush.Opacity = item.Value.InitialOpacity;//reset opacity to initial value
					}
				}
			}
			catch (Exception exception)
			{
				p($"9609   {line}  {exception.ToString()}");
			}
		}

		SharpDX.Direct2D1.AntialiasMode oldAntialiasMode;
		
		private bool FirstRender2 = true;
				
		private ChartControlProperties myProperties;
					
		private bool PreviousDrag = false;		
			
		SharpDX.DirectWrite.TextFormat CenterTextFormat;
		SharpDX.RectangleF CenterRect;

		private void ChartBarsSwitch2(bool onoff)
		{
			if (ChartPanel == null)
			return;
			
			
			if (onoff)
			{
				ChartBars.Properties.ChartStyle.DownBrushDX.Opacity = 1;
				ChartBars.Properties.ChartStyle.UpBrushDX.Opacity = 1;
				ChartBars.Properties.ChartStyle.Stroke.BrushDX.Opacity = 1; // outline
				ChartBars.Properties.ChartStyle.Stroke2.BrushDX.Opacity = 1; // wick
			}
			else
			{
				ChartBars.Properties.ChartStyle.DownBrushDX.Opacity = 0;
				ChartBars.Properties.ChartStyle.UpBrushDX.Opacity = 0;				
				ChartBars.Properties.ChartStyle.Stroke.BrushDX.Opacity = 0; // outline
				ChartBars.Properties.ChartStyle.Stroke2.BrushDX.Opacity = 0; // wick
				
			}
			
			//ChartBars.Properties.AutoScale = onoff;
			
		}

		private void AddError(string eee)
		{
		
			//if (pErrorMessagesEnabled)
			if (!AllErrorMessages.Contains(eee))
				AllErrorMessages.Add(eee);
			
		}		

		//#region -- ChartControl events --
		private void ChartControl_PreviewKeyUp(object sender, KeyEventArgs e)
		{
//			if (pOneClickOrdersEnabled2 && pActivateWithKeys)
//			{
//				IsCTRLNow = Keyboard.IsKeyDown(Key.LeftCtrl);
//				IsALTNow = Keyboard.IsKeyDown(Key.LeftAlt);
				
//				InOneClick = false;
				
//				if (IsCTRLNow)
//				{
//					InOneClick = true;
//					pOneClickStatus = "Long";
//				}
//				if (IsALTNow)
//				{
//					InOneClick = true;
//					pOneClickStatus = "Short";
//				}

//				BuyClickReady = false;
//				SellClickReady = false;	
				
//				if (InOneClick)
//				if (pOneClickStatus == "Long")
//				{
//					BuyClickReady = true;
//				}
//				else
//				{
//					SellClickReady = true;	
//				}
					
//				UpdateButtons();
//				ChartControl.InvalidateVisual();
//			}
			
	
		}
		
		
		private int KeyNumber(string keyyyy)
		{
		
			if (keyyyy.Contains("1"))
				return 1;
			if (keyyyy.Contains("2"))
				return 2;			
			if (keyyyy.Contains("3"))
				return 3;				
			if (keyyyy.Contains("4"))
				return 4;				
			if (keyyyy.Contains("5"))
				return 5;	
			if (keyyyy.Contains("6"))
				return 6;				
			if (keyyyy.Contains("7"))
				return 7;				
			if (keyyyy.Contains("8"))
				return 8;				
			if (keyyyy.Contains("9"))
				return 9;	
			if (keyyyy.Contains("0"))
				return 10;	
			
			return -1;
		}
		
		// System.Windows.Forms.KeyEventArgs
		   
		
//		protected override void OnKeyDown(KeyEventArgs e)
//	    {
//	        base.OnKeyDown(e);
//	        switch (e.KeyCode)
//	        {
//	            case Keys.Left:
//	            case Keys.Right:
//	            case Keys.Up:
//	            case Keys.Down:
//	                if (e.Shift)
//	                {
	
//	                }
//	                else
//	                {
//	                }
//	                break;                
//	        }
//	    }
		
	
	
		private void ChartControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			
			if (!pKeyboardEnabled)
				return;
			
			//Print("test");
			
			string LastKey = e.Key.ToString();
			
			
			
			
			// doesn't work for up and down arrows

//			if (e.Key == Key.Up)
//			{
				
//				Print("UP");
				
//			}
			
//			switch (e.KeyCode)
//			{
				
				
//			}
			

			
			if (SelectedMultiplierAccount != string.Empty && pUseNumberForMultiplier)
			{
				SelectedMultiplerTime = DateTime.Now;
				
				//ChartControl.InvalidateVisual();
			
			
				
				double dddd = KeyNumber(LastKey);
				
				
				if (dddd != -1)
				SetAccountData("",SelectedMultiplierAccount,"",dddd.ToString(),"","","","","","");
			
				
			    e.Handled = true;
				
				SelectedMultiplierAccount = string.Empty;
				
				
				ChartControl.InvalidateVisual();
					
				
				
			}
			else
			{
						
				bool handled = false;
				
				bool doscrollalso = true;
				
				
				if (LastKey == pKeyScrollUp)
				{
					handled = DoWheel(1, doscrollalso);
				}
				if (LastKey == pKeyScrollDn)
				{
					handled = DoWheel(-1, doscrollalso);
				}
			
				if (handled)
					e.Handled = true;
	
				
				
				
			}
			
			
		
			
			
//			if (pOneClickOrdersEnabled2 && pActivateWithKeys)
//			{
//				IsCTRLNow = Keyboard.IsKeyDown(Key.LeftCtrl);
//				IsALTNow = Keyboard.IsKeyDown(Key.LeftAlt);
				
//				InOneClick = false;
				
//				if (IsCTRLNow)
//				{
//					InOneClick = true;
//					pOneClickStatus = "Long";
//				}
//				if (IsALTNow)
//				{
//					InOneClick = true;
//					pOneClickStatus = "Short";
//				}

//				BuyClickReady = false;
//				SellClickReady = false;	
				
//				if (InOneClick)
//				if (pOneClickStatus == "Long")
//				{
//					BuyClickReady = true;
//				}
//				else
//				{
//					SellClickReady = true;	
//				}
					
//				UpdateButtons();
//				ChartControl.InvalidateVisual();
				
//			}
			
			
			
//			if (!pHotKeysEnabled)
//				return;
			
			
//			bool IsCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl);

//				string LastKey = e.Key.ToString();
		
		
		
//				if (pKeyBuyMarket != string.Empty)
//				if (IsCtrlPressed && LastKey == pKeyBuyMarket)
//				{
//					BuyMarketButton();
//					e.Handled = true;
//				}
				
//				if (pKeySellMarket != string.Empty)
//				if (IsCtrlPressed && LastKey == pKeySellMarket)
//				{
//					SellMarketButton();
//					e.Handled = true;
					
//				}			
			
			
//				if (pKeyBuyClick != string.Empty)
//				if (IsCtrlPressed && LastKey == pKeyBuyClick)
//				{
//					BuyClickNow();
//					e.Handled = true;
//				}
				
//				if (pKeySellClick != string.Empty)
//				if (IsCtrlPressed && LastKey == pKeySellClick)
//				{
//					SellClickNow();
//					e.Handled = true;
					
//				}
				
//				if (pKeyOrderPanel != string.Empty)
//				if (IsCtrlPressed && LastKey == pKeyOrderPanel)
//				{
//					DoOrderPanel();
//					e.Handled = true;
					
//				}
				
//				//Print(LastKey);
						
//				if (pATMQuickButtonsEnabled)
//				{
				
//					if (pKeyATM1 != string.Empty)
//					if (IsCtrlPressed && LastKey == pKeyATM1)
//					{
//						ATM1Click();
//						e.Handled = true;
						
//					}
											
//					if (pKeyATM2 != string.Empty)
//					if (IsCtrlPressed && LastKey == pKeyATM2)
//					{
//						ATM2Click();
//						e.Handled = true;
						
//					}
												
//					if (pKeyATM3 != string.Empty)
//					if (IsCtrlPressed && LastKey == pKeyATM3)
//					{
//						ATM3Click();
//						e.Handled = true;
						
//					}
												
//					if (pKeyATM4 != string.Empty)
//					if (IsCtrlPressed && LastKey == pKeyATM4)
//					{
//						ATM4Click();
//						e.Handled = true;
						
//					}
					
//				}
											
//					if (pKeyATMReset != string.Empty)
//					if (IsCtrlPressed && LastKey == pKeyATMReset)
//					{
//						ATMToggle();
//						e.Handled = true;
						
//					}				
				
				
				
				
				
			
//			//	Print(LastKey);
			
			
				
				
//				if (e.Key == Key.Escape)
//				{
//					ResetAllActiveClickOrders();
//					e.Handled = true;
//					ChartControl.InvalidateVisual();
//					ChartControl.InvalidateVisual();
//				}
						
				
			
			
			
			//Print(e.Key.ToString());
			
			
			//if (buttonGrid.IsFocused)
			
			
			
//			if (pOrderPanelOn)
//			if (qudSelector.IsFocused)
			
			
				
//			e.Handled = true;
			
		}
		
		
		bool WatchingKeys = false;
		bool LastIsLeftCtrl = false;
		private void chartWindow_KeyDown(object sender, KeyEventArgs e)
		{
			
			Print("FFF " + e.Key.ToString());
			
		}

		private void ChartControl_KeyDown(object sender, KeyEventArgs e)
		{
			//if (buttonGrid.IsFocused)
			
			// did not work as good as the  the chart window key down
					
//			Print("KEYDOWN " + e.Key.ToString());
			
//			if (e.Key == Key.Escape)
//			{
				
				
//			}
			
//			e.Handled = true;
			
//				ResetAllActiveClickOrders();
			
//			if (pOrderPanelOn)
//			if (qudSelector.IsFocused)
			
			
				
//			e.Handled = true;
			
		}

		private int AdjustmentRows = 0;
		private int AdjustmentAmount = 1;

		private int GetATMNumber (string atmsel)
		{
		
			int tfgdfh = 0;
			
			foreach (string ssss in AllATMStrategies)
			{
				if (atmsel == ssss)
					return tfgdfh;
				
					
				tfgdfh++;
				
			}
			
			return tfgdfh;
			
		}

		private double LastDailyGoalNumber = 0;
		private double LastDailyLossNumber = 0;
		private double LastDailyPayoutNumber = 0;
		private string LastATMSelected = string.Empty;
		private double LastSizeNumber = 0;
		
		private double IgnoreNext = 0;
		
		private DateTime IgnoreNextTime = DateTime.MinValue;
		
		//#region -- DoWheel --
		private bool DoWheel (int dirmove, bool scrollwindow)
		{
				
//			private string SelectedDailyGoalAccount = string.Empty;
//		private DateTime SelectedDailyGoalTime = DateTime.MinValue;		
		
//		private string SelectedDailyLossAccount = string.Empty;
//		private DateTime SelectedDailyLossTime = DateTime.MinValue;		
		
		//	Print(dirmove);
			
			
			bool ehhhhh = false;
			

			if (SelectedDailyGoalAccount != string.Empty)
			{
				
				SelectedDailyGoalTime = DateTime.Now.AddSeconds(3);
				
				ChartControl.InvalidateVisual();

				
				double inc = pAdjustmentDollars;
				double dddd = GetAccountSavedData(SelectedDailyGoalAccount, pAllAccountDailyGoal);
				
				if (dirmove > 0)
					dddd = dddd + inc;
				else
					dddd = dddd - inc;
				
				if (pRestrictToZero)
				{
					dddd = Math.Max(0, dddd);
				}

				
				
				
				if (dddd <= 0)
					SelectedMultiplerTime = DateTime.Now;
	
				LastDailyGoalNumber = dddd;
				
				SetAccountSavedData(SelectedDailyGoalAccount,pAllAccountDailyGoal,dddd.ToString());
			
				
			    ehhhhh = true;
				
				ChartControl.InvalidateVisual();					
				
				
			}
			else if (SelectedDailyLossAccount != string.Empty)
			{
				
				SelectedDailyLossTime = DateTime.Now.AddSeconds(3);
				
				ChartControl.InvalidateVisual();
		
				
				double inc = pAdjustmentDollars;
				double dddd = GetAccountSavedData(SelectedDailyLossAccount,pAllAccountDailyLoss);
				
				if (dirmove > 0)
					dddd = dddd + inc;
				else
					dddd = dddd - inc;
				
				if (pRestrictToZero)
				{
					dddd = Math.Min(0, dddd);
				}
				
				if (dddd <= 0)
					SelectedMultiplerTime = DateTime.Now;
				
				LastDailyLossNumber = dddd;
				
				SetAccountSavedData(SelectedDailyLossAccount,pAllAccountDailyLoss,dddd.ToString());
			
				
			    ehhhhh = true;
				
				ChartControl.InvalidateVisual();				
				
				
			}
			else if (SelectedPayoutAccount != string.Empty)
			{
				
				SelectedPayoutTime = DateTime.Now.AddSeconds(3);
				
				ChartControl.InvalidateVisual();
		
				
				double inc = pAdjustmentDollars;
				double dddd = GetAccountSavedData(SelectedPayoutAccount,pAllAccountPayouts);
				
				if (dirmove > 0)
					dddd = dddd + inc;
				else
					dddd = dddd - inc;
				
				if (dddd <= 0)
					SelectedMultiplerTime = DateTime.Now;
				
				dddd = Math.Max(pAdjustmentDollars, dddd);
	
				LastDailyPayoutNumber = dddd;
				
				SetAccountSavedData(SelectedPayoutAccount,pAllAccountPayouts,dddd.ToString());

				
				GetAllPerformanceStats();			
				
			    ehhhhh = true;
				
				ChartControl.InvalidateVisual();				
				
				
			}			
			else if (SelectedMultiplierAccount != string.Empty)
			{
				SelectedMultiplerTime = DateTime.Now.AddSeconds(3);
				
				ChartControl.InvalidateVisual();
			
				string ssss = GetAccountMultiplier(SelectedMultiplierAccount);
				
				//Print(ssss);
				
				int variable = 0;

				
				double inc = 1;
				double dddd = 0;
				double.TryParse(ssss, out dddd);
				

				
				if (dirmove > 0)
					dddd = dddd + inc;
				else
					dddd = dddd - inc;
				
				if (dddd == 1)
					SelectedMultiplerTime = DateTime.Now;
				
//				if (dddd <= 0)
//					SelectedMultiplerTime = DateTime.Now;
				
				if (pMultiplierMode == "Quantity")
					dddd = Math.Max(1, dddd);
				
				dddd = Math.Max(-3, dddd);
			
				
				LastSizeNumber = dddd;
				
				SetAccountData("",SelectedMultiplierAccount,"",dddd.ToString(),"","","","","","");
			
				if (dddd == 1)
				//if (dirmove < 0 && dddd == 1)
				{
					SelectedMultiplierAccount = string.Empty;
					SelectedMultiplerTime = DateTime.Now;
					IgnoreNextTime = DateTime.Now.AddSeconds(1.5);
				}
					
				
			    ehhhhh = true;
				
				ChartControl.InvalidateVisual();
					
					
			}
			else if (SelectedATMAccount != string.Empty)
			{
				SelectedATMTime	= DateTime.Now.AddSeconds(3);
			 
				string ssss = GetAccountMode(SelectedATMAccount);
				
				
				if (dirmove > 0)
				{
					
					
				}
				
//				Print(SelectedATMAccount);
//				Print(ssss);
//				Print(dirmove);
				
				string newmode = ssss;
				
				if (ssss == "Default")
				{
					if (dirmove > 0)
					{
						if (pMultiplierMode != "Quantity")
							newmode = "Orders";
					}
					else
					{
						// first atm in list
						if (AllATMStrategies.Count > 0)
							newmode = AllATMStrategies[0];
					}
					
				}
				else if (ssss == "Orders")
				{
					if (dirmove > 0)
						newmode = "Executions";
					else
					{
						newmode = "Default";				
					}					
					
				}				
				else if (ssss == "Executions")
				{
					if (dirmove > 0)
						newmode = "Executions";
					else
					{
						newmode = "Orders";
					}					
					
				}	
				else
				{
				
					// up or down list of atms
					int currentatm = GetATMNumber(ssss);
					
					
					if (dirmove > 0)
					{
						// up one atm
						if (currentatm == 0)
						{
							newmode = "Default";
						}
						else
						{
							newmode = AllATMStrategies[currentatm-1];
						}
						
						
					}
					else
					{
						// down one atm
						
						int nexta = Math.Min(currentatm+1,AllATMStrategies.Count-1);
						
 
						
						newmode = AllATMStrategies[nexta];
						
						//newmode = "fakea";
					}	
					
					
					
				}
				

				LastATMSelected = newmode;
	
				
				SetAccountData("",SelectedATMAccount,"","","","","","","",newmode);
				
				if (pMultiplierMode == "Quantity")
				if (newmode != "Default" && newmode != "Orders" && newmode != "Executions")
				{
					
					AtmStrategy ThisOne = null;
					
					if (DefaultAtmStrategy.ContainsKey(newmode))
						ThisOne = DefaultAtmStrategy[newmode];
					
					
									
					SetAccountData("",SelectedATMAccount,"",ThisOne.EntryQuantity.ToString(),"","","","","","");
					
				}
				
					
												
													
				if (newmode != "Executions")
				{
					if (pIsFadeEnabled)	
						SetAccountData("",SelectedATMAccount,"","","","No","","","","");	
				}													
				
				
				
				if (newmode == "Default")
				{
							
						SelectedATMAccount = string.Empty;
						SelectedATMTime = DateTime.Now;	
						IgnoreNextTime = DateTime.Now.AddSeconds(1.5);
				}
				
				
				
			}
			else if (scrollwindow)
			{
				
				if (dirmove > 0)
					AdjustmentRows = AdjustmentRows - AdjustmentAmount;
				else
					AdjustmentRows = AdjustmentRows + AdjustmentAmount;			
				
				
				
				
				AdjustmentRows = Math.Min(TotalAccounts - MaximumAccountRows, AdjustmentRows);
				
				//AdjustmentRows = Math.Min(TotalAccounts - MaximumAccountRows - 1, AdjustmentRows);
				
				
				if (pShowFollowersAtTop)
					AdjustmentRows = Math.Min(TotalAccounts - MaximumAccountRows - AllDuplicateAccounts.Count, AdjustmentRows);
					
					
				
				AdjustmentRows = Math.Max(0, AdjustmentRows);
				
				
				
				
				
				//Print(TotalAccounts - MaximumAccountRows);
				
				
				ehhhhh = true;
				
				ChartControl.InvalidateVisual();
				
			}
			else
			{
			}
			return ehhhhh;
		}
		//#endregion
		
		
//		private void ChartControl_MouseWheel(object sender, MouseWheelEventArgs e)
//		{
			
//			Print(e.Delta);
			
//			if (IgnoreNextTime.Ticks > DateTime.Now.Ticks)
//			{

//				return;
				
//			}
			
			
//			int directionmove = e.Delta;
			
//			bool handled = DoWheel(directionmove, true);
			
//			if (handled)
//				e.Handled = true;
			
//		}
		

		private void ChartControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			
			//Print(e.Delta);
			
			if (IgnoreNextTime.Ticks > DateTime.Now.Ticks)
			{

				return;
				
			}
			
			
			int directionmove = e.Delta;
			
			bool handled = DoWheel(directionmove, true);
			
			if (handled)
				e.Handled = true;
			
		}
		//#endregion

		private int ThisWindowWidth;
		private int ThisWindowHeight;
		
		private int ButtonsHeight = 0;
			
		float rightoftable = 0;
					
				
		int RenderCount = 0;
		
		
		bool HasAPEXAccounts = false;
		bool HasLeeLooAccounts = false;
		
		
					
		float starty = 0;	
		string ThisMessage = "";
		float ppppppp = 12; // vertical padding in message box
		float ppppppp2 = 12; // add space between message boxes
		float leftrightspace = 2;	
		bool firstmessage = false;
		float totaladjustment = 0;			
		
		int addedtwice = 0;
		
		bool LaunchedWithStandardChartTraderOpen = false;
		
		//string MessageChartTrader = "Please create a new Chart window, and disable the standard Chart Trader, before adding the 'aiDuplicateAccountsActionsBen' indicator.";	
		string MessageChartTrader = "Please disable the standard Chart Trader.  At the top of the Chart window, click the Chart Trader icon, and select Off.";
		
		int indx = 0;
					
		
		SortedDictionary<int, double> AllRemoves = new SortedDictionary<int, double>();	
		
		SortedList<string, bool> AllTotalRows = new SortedList<string, bool>();
		
		List<string> AllTotalRowsS = new List<string>();
		
		
		bool StillIn = true;
		
		bool ATMsSaved = false;
		// buttons
		
		int LeftSpaceButtons = 0;
	
		float CY = 0;
		
		float thistop = 5;
		
		
		float nextrightside = 0;
	 
		
		float leftsidebuttonsrightx = 0;
									
		
		float lastrightside = 0;
		
		bool ATMsSaved2 = true;
		
		
		
        private SharpDX.Direct2D1.Bitmap cachedBitmap;
        private BitmapRenderTarget bitmapRenderTarget;
        private Factory dxFactory;
        private bool needsRedraw = true;		
		
		private bool AllowRedraw = true;
		int line = 10128;
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			try{
			//#region --
			//return;

//			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			
//			Print(timestamp);
			
			
		
			if (IsInHitTest) return;
						int iii = 0;
							
						#region atm strategy selector
						if (!ATMsSaved)
						if (atmStrategySelector != null)
						{
							
							//Print(atmStrategySelector.Items.Count);
							
							
							if (atmStrategySelector.Items.Count > 4)
							{
									foreach (object aaa in atmStrategySelector.Items)
									{
										
										
										
										//if (aaa.GetType() == Type.aAtmStrategy)
										AtmStrategy FinalATMStrategy = null;
										AtmStrategy ClonedATMStrategy = null;
										
										iii++;
										
										try
										{
											//FinalATMStrategy = new AtmStrategy();
											//	Person b = (Person) a.Clone();
											//object asfddsg = aaa.MemberwiseClone();
											//var cloned = asfddsg.
											
											//AtmStrategy s1 = "This is C# programming";
											
											
											FinalATMStrategy = (AtmStrategy) aaa;
											
											ClonedATMStrategy = (AtmStrategy) FinalATMStrategy.Clone();
												
										}
										catch (Exception ex)
										{
										
										}
									
															
										if (ClonedATMStrategy != null)
										{
											string displayn = ClonedATMStrategy.DisplayName;
											
										//	Print(iii + " " + displayn);
											
											if (displayn != "Custom")
											{
												
												if (!DefaultAtmStrategy.ContainsKey(displayn))
													DefaultAtmStrategy.Add(displayn, ClonedATMStrategy);
												
												if (!DefaultAtmStrategyNumber.ContainsKey(iii))
													DefaultAtmStrategyNumber.Add(iii, ClonedATMStrategy);
												
													
												//SortedList<int, double> sssss = new SortedList<int, double>();
												
												
												//Print(displayn);
//												if (!DefaultAtmStrategyPercentages.ContainsKey(displayn))
//													DefaultAtmStrategyPercentages.Add(displayn,AllTargetPercentages(ClonedATMStrategy,0,0));
												
												
												//
												
											}
											
										}
										
										//iii++;
										
										
									}		
									
									ATMsSaved = true;
							}
									
									//Print("ATMsSaved");
									
									
					}
						#endregion


			HasAPEXAccounts = false;
			HasLeeLooAccounts = false;
			
			oldAntialiasMode	= RenderTarget.AntialiasMode;
      
			
			bool dothisone = false;
						//if (dothisone)
			if (pIsCopyBasicFunctionsChecked)	
			{
				if (StillIn)
					try
					{
							
						if (!pIsCopyBasicFunctionsChecked)	
						{
							btnDrawObjs.Visibility = Visibility.Collapsed;
							btnShowTrades.Visibility = Visibility.Collapsed;
						}
								
						            
						string skinname = NinjaTrader.Core.Globals.GeneralOptions.Skin.ToString();
						
						if (skinname == "NinjaTrader Dark" || skinname == "Dark" || skinname == "Slate Dark" || skinname == "Slate Gray")
						{
							btnDrawObjs.Foreground = Brushes.WhiteSmoke;
						}
						else
						{
							btnDrawObjs.Foreground = Brushes.Black;
						}
						
				
			
						StillIn = false;
						
					}
						catch
						{
									//Print("render error " + DateTime.Now.ToLongTimeString() + " - ");
						}
							
	
					if (!StillIn)
					{
					
						int totalacccounts = 0;
						
						if (CurrentMasterAccount != null)
						{
							totalacccounts = 1;
							
						}
						
						//Print(totalacccounts);
						
						
						totalacccounts = totalacccounts + AllDuplicateAccounts.Count;
						
							//btnDrawObjs.Content = pCopierMode + "     |    " + totalacccounts + " Accounts";
						
						
						
						string ddddd = "No Selected Accounts";
						
						if (totalacccounts > 0)
							ddddd = totalacccounts + " Selected Accounts";
						
						if (IsKorean)
						{
							ddddd = ddddd.Replace("No Selected Accounts", "지정된 계좌 없음");
							ddddd = ddddd.Replace("Selected Accounts", "지정된 계좌");
							
						}
						
						
							btnDrawObjs.Content = ddddd;
						
					}
			}
					
			
			if (FirstRender2)
			{
			
				//Print("FirstRender2");
	
				//ChartBarsSwitch2(true);

				//chartWindow.Name = "Accounts Dashboard";
				//chartWindow.Caption = "Accounts Dashboard - 24. 3. 8. 1";
				//chartWindow.Caption = "Accounts Dashboard";
			
				
			
						
				myProperties = chartControl.Properties;

				//PreviousDrag = myProperties.AllowSelectionDragging;
				
				//Print("TTT1");
				
				myProperties.BarDistance = 2000;
				
	
			
					
					//if (pStandardChartTrader)
//					if (myProperties.ChartTraderVisibility != ChartTraderVisibility.Collapsed)
//					{
						
//						AddMessage(MessageChartTrader, 10000, RED);
//						LaunchedWithStandardChartTraderOpen = true;						
						
//					}
						

				
				
				if (ChartBars != null)
				{
				
					ChartBars.Properties.PlotExecutions = ChartExecutionStyle.DoNotPlot;
					
					ChartBars.Properties.ChartStyle.DownBrush = Brushes.Transparent;
					ChartBars.Properties.ChartStyle.UpBrush = Brushes.Transparent;
					ChartBars.Properties.ChartStyle.Stroke.Brush = Brushes.Transparent;
					ChartBars.Properties.ChartStyle.Stroke2.Brush = Brushes.Transparent;	
					
					ChartBars.Properties.PaintPriceMarker = false;

					ChartBars.Properties.TradingHoursVisibility = TradingHoursBreakLineVisible.Off;
					
					ChartBars.Properties.ShowGlobalDrawObjects = false;
					ChartBars.Properties.DisplayInDataBox = false;
					ChartBars.Properties.AutoScale = false;
					
					ChartBars.Properties.ScaleJustification = ScaleJustification.Overlay;
					
					//ChartBars.Properties.
					
				}
				
				
				
				
				
	
				// this causes the chat window to freeze
				// chartControl.AxisYRightWidth = 0;
				
//				foreach(ChartPanel ppp in chartControl.ChartPanels)
//				{
				
//					chartControl.
//					Print(ppp.Name);
					
//				}

				myProperties.AreHGridLinesVisible = false;
				myProperties.AreVGridLinesVisible = false;
				

				myProperties.ChartText = Brushes.Transparent;
				myProperties.AxisPen.Brush = Brushes.Transparent;
				myProperties.LabelFont = new SimpleFont("Arial",1);
	
				
				
				
//				myProperties.ChartText = Brushes.Wheat;
//				myProperties.AxisPen.Brush = Brushes.Wheat;						
//				myProperties.LabelFont = new SimpleFont("Arial",20);
				
				// doesn't actually disable it
				//myProperties.ChartTraderVisibility = ChartTraderVisibility.Collapsed;
				
				myProperties.ShowScrollBar = true;
				myProperties.ShowScrollBar = false;
				
		
//				myProperties.CrosshairStroke.Brush = Brushes.Transparent;
//				myProperties.CrosshairStroke.Brush = Brushes.Transparent;
				
				myProperties.CrosshairLabelBackground = Brushes.Transparent;
				myProperties.CrosshairLabelForeground = Brushes.Transparent;
	
				// this doesn't actually work
				myProperties.CrosshairCrosshairType = CrosshairType.Off;
				
			//	myProperties.CrosshairDrawCursorOnly = true;
					
									
				
				
				
				// myProperties.CrosshairStroke.Brush = Brushes.White;
				// myProperties.CrosshairDrawCursorOnly = false;
				
				
				
//				myProperties.CrosshairLabelBackground = Brushes.Transparent;
//				myProperties.CrosshairLabelForeground = Brushes.Transparent;
				
				
				//myProperties.TabName = "Duplicate Account Actions - All Instruments";
				
				// old code to get rid of tabs named like this 8/4/2023
				
				if (myProperties.TabName == "Duplicate Account Actions - All Instruments")
				{
					if (pAllInstruments)
						myProperties.TabName = "All Instruments";
					else	
						myProperties.TabName = Instrument.FullName + "Only";
					
					//Print(pAllInstruments);
					
				}
				
//				if (!pAllInstruments && myProperties.TabName == "All Instruments")
//				{
//					myProperties.TabName = Instrument.FullName + "Only";
//				}
				
//					if (ChartControl != null)
//					{

//						//Print("ChartControl_PreviewMouseWheel");
						
//						ChartControl.PreviewMouseWheel += ChartControl_PreviewMouseWheel;
						
//						ChartControl.PreviewKeyDown += ChartControl_PreviewKeyDown;
//						ChartControl.PreviewKeyUp += ChartControl_PreviewKeyUp;
//						ChartControl.KeyDown += ChartControl_KeyDown;
												
//					}
					
				//Print("TTT3");

				FirstRender2 = false;

				AddMessage(TheWarningMessage, 25, GOLDENROD);
				
				br[ChartTextID].MediaBrush = GetTextColor(ThisChartBackground());
				br[ChartBackgroundID].MediaBrush = ThisChartBackground();
//p($"10476   ThisChartBackground:  {ThisChartBackground().ToString()}");
			}
		
			
//			pPreviousVersionName = pPreviousVersionName.Replace("Accounts Dashboard -", "");
//			pCurrentVersionName = pCurrentVersionName.Replace("Accounts Dashboard -", "");
		
			
	
			if (pFirstLoadIsDone)
			//if (pCurrentVersionName != pPreviousVersionName || pPreviousVersionName == "") // load message every time
			{

				if (pCurrentVersionName != pPreviousVersionName || pPreviousVersionName == "")
				{
					if (!IsKorean)
						AddMessage("You have successfully updated to the latest version of the 'Accounts Dashboard' products.  Thanks for your business!", 10000, GOLDENROD);
					else
						AddMessage("Accounts Dashboard' 프로그램을 최신 버전으로 성공적으로 업데이트하셨습니다. 이용해 주셔서 감사합니다!", 10000, GOLDENROD);
						
				}
				
			}
	
			pPreviousVersionName = pCurrentVersionName;
			
			
			if (!pFirstLoadIsDone)
			{
				//AddMessage(TheWarningMessage, 10000, GOLDENROD);
			}
			
			string thisverb = "reset";
			
			
				if (SelectedResetColumn == "Auto Exit" && LastAutoExitReset == "No")
					thisverb = "enable";
				if (SelectedResetColumn == "Auto Exit" && LastAutoExitReset == "Yes")
					thisverb = "disable";	
				
				if (SelectedResetColumn == "Auto Close" && LastAutoCloseReset == "No")
					thisverb = "enable";
				if (SelectedResetColumn == "Auto Close" && LastAutoCloseReset == "Yes")
					thisverb = "disable";	
				
				

			string messs = "Click \u2713 to confirm and " + thisverb + " the '" +  SelectedResetColumn + "' column for all accounts.  Or click here to cancel.";
			
			messs = messs.Replace("Auto Close", "Funded / Withdraw Exit");
			messs = messs.Replace("Auto Exit", "Daily Goal / Daily Loss Exit");
				
			if (SelectedResetColumn != string.Empty)
			{
				RemoveMessage("column for all accounts");
				AddMessage(messs, 10000, GOLDENROD);
				
			}
			else
			{
//				SelectedResetColumn = string.Empty;
//				SelectedResetTime = DateTime.MinValue;
				RemoveMessage("column for all accounts");
			}
			
			
			messs = $"Click '{SelectedButtonNow}' again to confirm and reset all settings across all accounts.  Or click here to cancel.";
			
			if (IsKorean && SelectedButtonNow == "Reset")
				messs = "모든 계정의 설정을 초기화하려면 '초기화' 버튼을 다시 클릭하여 확인하십시오. 또는 여기를 클릭하여 취소할 수 있습니다.";
			
			
			if (SelectedButtonNow == "Restore")
			{
				messs = $"Click '{SelectedButtonNow}' again to confirm and restore {AllHideAccounts.Count.ToString()} hidden accounts.  Or click here to cancel.";
			}
			
			if (SelectedButtonNow != string.Empty)
			{
				RemoveMessage(MainResetConfirmation);
				AddMessage(messs, 10000, GOLDENROD);
				
			}
			else
			{
//				SelectedButtonNow = string.Empty;
//				SelectedButtonTime = DateTime.MinValue;	
				RemoveMessage(MainResetConfirmation);
			}			
	
			
			
			if (myProperties.ChartTraderVisibility == ChartTraderVisibility.Collapsed)
			{
				RemoveMessage(MessageChartTrader);
				pChartTraderNeedsDisabled = false;
				
					
					if (pRemoveIcons)
					{
						chartWindow.Caption = $"Accounts Dashboard - {pCurrentVersionName}";
						
						if (btnDrawObjs != null)
						{
						
							btnDrawObjs.Visibility = Visibility.Visible;
							btnShowTrades.Visibility = Visibility.Visible;
						}
						
					}
					else
					{
						chartWindow.Caption = "Chart";
						
					}
					

					
					
			}
			else
			{
				AddMessage(MessageChartTrader, 10000, RED);
				pChartTraderNeedsDisabled = true;
				chartWindow.Caption = "Chart";
						if (btnDrawObjs != null)
						{
										
				btnDrawObjs.Visibility = Visibility.Collapsed;
				btnShowTrades.Visibility = Visibility.Collapsed;
						}
				
			}
			
			//myProperties.ChartTraderVisibility = ChartTraderVisibility.Collapsed;
			
			
	
				#region remove all toolbar icons

				try
				{

						foreach (object oooo in chartWindow.MainMenu)
						{

							if (oooo.GetType() == typeof(System.Windows.Controls.Button))
							{
								System.Windows.Controls.Button bbbb = (System.Windows.Controls.Button) oooo;
								
								//Print(bbbb.Name);
								
								if (bbbb.Name != "")
								
								if (pRemoveIcons)
									bbbb.Visibility = Visibility.Collapsed;
								else
									bbbb.Visibility = Visibility.Visible;	
								
								
								
							}
							if (oooo.GetType() == typeof(System.Windows.Controls.Menu))
							{
								System.Windows.Controls.Menu bbbb = (System.Windows.Controls.Menu) oooo;
								
								//Print(System.Windows.Automation.AutomationProperties.GetAutomationId(oooo));
								
								//bbbb.items
				
								//ChartTraderNeedsDisabled
								
								
								if (pRemoveIcons)
								{
									bbbb.Visibility = Visibility.Collapsed;
					
								}
								else
								{
									bbbb.Visibility = Visibility.Visible;
								}
								

								
								
								if (pChartTraderNeedsDisabled)
								bbbb.Visibility = Visibility.Visible;
								
								
								
								
								
									foreach(ItemsControl ii in bbbb.Items)
									{
										//Print(ii.Name);
										
										if (pRemoveIcons)
										if (pChartTraderNeedsDisabled)
										if (ii.Name != "miChartTrader")
										{
											ii.Visibility = Visibility.Collapsed;
											//Print(ii.Name);
											
										}
										
										if (!pRemoveIcons)
											ii.Visibility = Visibility.Visible;
										
									}
									
								
							}		
							if (oooo.GetType() == typeof(NinjaTrader.Gui.Tools.IntervalSelector))
							{
								NinjaTrader.Gui.Tools.IntervalSelector bbbb = (NinjaTrader.Gui.Tools.IntervalSelector) oooo;
								if (pRemoveIcons)
									bbbb.Visibility = Visibility.Collapsed;
								else
									bbbb.Visibility = Visibility.Visible;								
							}	
							
//							if (oooo.GetType() == typeof(NinjaTrader.Gui.Tools.InstrumentSelector))
//							{
//								NinjaTrader.Gui.Tools.InstrumentSelector bbbb = (NinjaTrader.Gui.Tools.InstrumentSelector) oooo;
//								if (pRemoveIcons)
//									bbbb.Visibility = Visibility.Collapsed;
//								else
//									bbbb.Visibility = Visibility.Visible;								
//							}								

							
							
							//Print(oooo.GetType().ToString());
							
						}

						#region -- commented --
//					if (pRemoveIcons)
//					if (chartWindow != null)
//					if (chartWindow.MainMenu.Count != 1)
//					{
						
//						foreach (System.Windows.DependencyObject item in chartWindow.MainMenu)
//						{
//							//ChartWindowInstrumentSelector
//							//ChartWindowIntervalSelector
//							//ChartWindowMenuChartStyle
//							//ChartWindowMenuDrawingTool
//							//ChartWindowZoomInButton
//							//ChartWindowZoomOutButton
//							//ChartWindowMenuCrosshair
//							//ChartWindowDataBoxButton
//							//ChartWindowMenuChartTrader
//							//ChartWindowDataSeriesButton
//							//ChartWindowIndicatorsButton
//							//ChartWindowStrategiesButton
//							//ChartWindowPropertiesButton					
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowIntervalSelector")
								
								
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowMenuChartStyle")
//								AllRemoves.Add(indx, indx);
		
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowMenuDrawingTool")
//								AllRemoves.Add(indx, indx);
		
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowZoomInButton")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowZoomOutButton")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowMenuCrosshair")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowDataBoxButton")
//								AllRemoves.Add(indx, indx);
							
//							if (!ChartTraderNeedsDisabled)
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowMenuChartTrader")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowDataSeriesButton")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowIndicatorsButton")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowStrategiesButton")
//								AllRemoves.Add(indx, indx);
							
//							if (System.Windows.Automation.AutomationProperties.GetAutomationId(item) == "ChartWindowPropertiesButton")
//								AllRemoves.Add(indx, indx);
							
//							indx++;
							
//						}
						
					
						
//						foreach (KeyValuePair<int, double> aaaaa in AllRemoves.Reverse().ToList())
//						{
//							chartWindow.MainMenu.RemoveAt(aaaaa.Key);	
//						}
//					}
						#endregion

				}
				catch (Exception ex)
				{
					Print($"11305:   {line}  {ex.ToString()}");
				}
				#endregion	
			
			


				if (!pIsRiskFunctionsPermission)
				if (pExitShieldFeaturesEnabled)
				{
					//AddMessage("'Time Sliced Entry Orders' is available as an upgrade option. Click for more information.", 60, RED);
					
					if (pFirstLoadIsDone)
					AddMessage("'Exit Shield' features are only available with the 'Account Risk Manager' upgrade option.", 60, RED);
					
					//LicensingMessage = "'Exit Shield' features are only available with the 'Account Risk Manager' upgrade option.  Click here for more details.";
					pExitShieldFeaturesEnabled = false;
					
				}				
				
				
				if (!pIsRiskFunctionsPermission)
				if (pOneTradeAll)
				{
					//AddMessage("'Time Sliced Entry Orders' is available as an upgrade option. Click for more information.", 60, RED);
					
					if (pFirstLoadIsDone)
					AddMessage("'One Trade' features are only available with the 'Account Risk Manager' upgrade option.", 60, RED);
					
				
					pOneTradeAll = false;
					
				}				
						
				
				
				
								
				if (TotalFrozeSim != 0)
				{
					if (!IsKorean)
						AddMessage("Ghost orders are found on one or more simulated accounts.  Use the 'Reset' button in the Actions column to easily reset these accounts.", 60, RED);
					else
						AddMessage("하나 이상의 데모 계정에서 고스트 주문이 발견되었습니다. 이러한 계정을 쉽게 초기화하려면 '구분' 열에 있는 '초기화' 버튼을 사용하십시오.", 60, RED);
					
					
				}
				else
				{
					if (!IsKorean)
						RemoveMessage("Ghost orders are found on one or more simulated");
					else
						RemoveMessage("하나 이상의 데모 계정에서 고스트 주문이");
				}
				if (TotalFrozeLive != 0)
				{
				
					//AddMessage("Ghost orders are found on one or more live accounts.  Check in your external platform to make sure all orders have been cancelled.  After this, disconnect and reconnect the Connection related to this account.", 60, RED);
					AddMessage("Ghost orders are found on one or more live accounts.  Check in your external platform to make sure all orders have been cancelled.  Disconnect from all Connections.  Reconnect to all Connections.  This should resolve the issue!  If ghost orders are still remaining, then the NinjaTrader Control Center, Tools Menu, select Database Management and then Reset DB to remove the ghost orders.  Restart NinjaTrader.", 60, RED);
					//AddMessage("This live account cannot be reset.  Check in your external platform to make sure all orders have been cancelled.  Disconnect from all Connections.  In the NinjaTrader Control Center, Tools Menu, select Database Management and then Reset DB to remove the ghost orders.", 10, Brushes.Red)
				}
				else
				{
					RemoveMessage("Ghost orders are found on one or more live");
					
				}



 			if (AllErrorMessages.Count > 0)
			{
			#region AllErrorMessages	
					
					ChartBarsSwitch2(false);
					myProperties.AllowSelectionDragging = false;
					
					 
					ChartBackgroundErrorBrushID = RED25;
				
	            CenterTextFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory,
	                "Arial",
	                SharpDX.DirectWrite.FontWeight.Normal,
	                SharpDX.DirectWrite.FontStyle.Normal,
	                SharpDX.DirectWrite.FontStretch.Normal,
	                11.0F);
				
				CenterTextFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
				CenterTextFormat = new SimpleFont(ChartControl.Properties.LabelFont.Family.ToString(), 16).ToDirectWriteTextFormat();
	            CenterTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
	            CenterTextFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
	            CenterTextFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.Wrap;
				
				//CellFormat = FinalFont1.ToDirectWriteTextFormat();
				
				CenterRect = new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H);

				RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
				
					string txt = string.Empty;
				
				foreach (string sss in AllErrorMessages)
					txt = txt + sss + "\r\n\r\n";

				//Print(txt);
				if (txt.Contains("is available as an upgrade option."))
					txt = txt + "Click for more information.";
				else
					txt = txt + "Click here to continue.";
				
				
				if (txt.Contains("Hello!"))
				{
					ChartBackgroundErrorBrushID = DODGERBLUE18;
				}
				FillRectBoth(CenterRect, ChartBackgroundID);
				FillRectBoth(CenterRect, ChartBackgroundErrorBrushID);
				RenderTarget.DrawText(txt, CenterTextFormat, ExpandRect(CenterRect,-10,0), br[ChartTextID].DxBrush);

				RenderTarget.AntialiasMode = oldAntialiasMode;

//				ChartBackgroundErrorBrushDX.Dispose();		
				CenterTextFormat.Dispose();	
				
					
				#endregion
				return;
			}
			


			if (!Permission)
				return;

			
			 pFirstLoadIsDone = true;
			
	
			if (RenderCount < 20)
			{
					if (RenderCount == 10)
					{
						if (pIsEvalCloseEnabled)
						{
							//pColumnFromFund = true;
							
						}
						else
						{
						
							AutoCloseNoAllAccounts();
							
						}
					}
						
							
		
			
				RenderCount++;
				//return;
			}

			if (!IsVisible)
				return;

				if (this.ChartPanel != null)
		        if (dpiX == 0)
		        {
					PresentationSource source = PresentationSource.FromVisual(this.ChartPanel);
					
					if (source != null)
					{
		            dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
		            dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
					
		             dpiX = 100.0 * source.CompositionTarget.TransformToDevice.M11;
		            dpiY = 100.0 * source.CompositionTarget.TransformToDevice.M22;
					}
					
				}
						
			

			
			float y = 0;
			float x = 0;
			float x1 = 0;
			float x2 = 0;
			float x3 = 0;
			float x4 = 0;
			float y1 = 0;
			float y2 = 0;
			float y3 = 0;
			double vtextadjust = 0;
	


			// TABLLLLEEEEEE
			ThisBrushID = WHITE;
			TableFinalBackgroundID = WHITE;

			br[TableBackgroundID].DxBrush.Opacity = pCompMinOpacity/100f;
			TableHighlight1ID = pCompMainColorID;
			br[TableHighlight1ID].DxBrush.Opacity = (pCompMinOpacityH)/100f;	
			
			// This sample should be used along side the help guide educational resource on this topic:		
				// http://www.ninjatrader.com/support/helpGuides/nt8/en-us/?using_sharpdx_for_custom_chart_rendering.htm	

				// Default plotting in base class. Uncomment if indicators holds at least one plot 
				// in this case we would expect NOT to see the SMA plot we have as well in this sample script
				//base.OnRender(chartControl, chartScale);

				// 1.1 - SharpDX Vectors and Charting RenderTarget Coordinates

				// The SharpDX SDK uses "Vector2" objects to describe a two-dimensional point of a device (X and Y coordinates)
	
//				SharpDX.Vector2 StartPointT;
//				SharpDX.Vector2 EndPointT;
			

				// For our custom script, we need a way to determine the Chart's RenderTarget coordinates to draw our custom shapes
				// This info can be found within the NinjaTrader.Gui.ChartPanel class.  
				// You can also use various chartScale and chartControl members to calculate values relative to time and price
				// However, those concepts will not be discussed or used in this sample
				// Notes:  RenderTarget is always the full ChartPanel, so we need to be mindful which sub-ChartPanel we're dealing with
				// Always use ChartPanel X, Y, W, H - as chartScale and chartControl properties WPF units, so they can be drastically different depending on DPI set
//				StartPointT = new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y);
//				EndPointT = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);

				// These Vector2 objects are equivalent with WPF System.Windows.Media.Point and can be used interchangeably depending on your requirements
				// For convenience, NinjaTrader provides a "ToVector2()" extension method to convert from WPF Points to SharpDX.Vector2
//				SharpDX.Vector2 StartPointT1 = new System.Windows.Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H).ToVector2();
//				SharpDX.Vector2 EndPointT1 = new System.Windows.Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y).ToVector2();

				// SharpDX.Vector2 objects contain X/Y properties which are helpful to recalculate new properties based on the initial vector
//				float width = EndPointT.X - StartPointT.X;
//				float height = EndPointT.Y - StartPointT.Y;

				// Or you can recalculate a new vector from existing vector objects
//				SharpDX.Vector2 center = (StartPointT + EndPointT) / 2;
			
						
				// Tip: This check is simply added to prevent the Indicator dialog menu from opening as a user clicks on the chart
				// The default behavior is to open the Indicator dialog menu if a user double clicks on the indicator
				// (i.e, the indicator falls within the RenderTarget "hit testing") 
				// You can remove this check if you want the default behavior implemented
						
						
					// 1.2 - SharpDX Brush Resources

					// RenderTarget commands must use a special brush resource defined in the SharpDX.Direct2D1 namespace
					// These resources exist just like you will find in the WPF/Windows.System.Media namespace
					// such as SolidColorBrushes, LienarGraidentBrushes, RadialGradientBrushes, etc.
					// To begin, we will start with the most basic "Brush" type
					// Warning:  Brush objects must be disposed of after they have been used
				
				
				
//					SharpDX.Direct2D1.Brush areaBrushDx;
			
					//SharpDX.Direct2D1.Brush textBrushDx;

					// for convenience, you can simply convert a WPF Brush to a DXBrush using the ToDxBrush() extension method provided by NinjaTrader
					// This is a common approach if you have a Brush property created e.g., on the UI you wish to use in custom rendering routines
			
					textID = pColorTextBrushID;//.ToDxBrush(RenderTarget);
					
					// However - it should be noted that this conversion process can be rather expensive
					// If you have many brushes being created, and are not tied to WPF resources
					// You should rather favor creating the SharpDX Brush directly:
					// Warning:  SolidColorBrush objects must be disposed of after they have been used
//					SharpDX.Direct2D1.SolidColorBrush customDXBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget,
//						SharpDX.Color.DodgerBlue);

					// 1.3 - Using The RenderTarget
					// before executing chart commands, you have the ability to describe how the RenderTarget should render
					// for example, we can store the existing RenderTarget AntialiasMode mode
					// then update the AntialiasMode to be the quality of non-text primitives are rendered
					
					
					oldAntialiasMode = RenderTarget.AntialiasMode;
					//RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;

					// Note: The code above stores the oldAntialiasMode as a best practices
					// i.e., if you plan on changing a property of the RenderTarget, you should plan to set it back
					// This is to make sure your requirements to no interfere with the function of another script
					// Additionally smoothing has some performance impacts

					// Once you have defined all the necessary requirements for you object
					//  You can execute a command on the RenderTarget to draw specific shapes
					// e.g., we can now use the RenderTarget's DrawLine() command to render a line
					// using the start/end points and areaBrushDx objects defined before

					// Since rendering occurs in a sequential fashion, after you have executed a command 
					// you can switch a property of the RenderTarget to meet other requirements  
					// For example, we can draw a second line now which uses a different AntialiasMode 
					// and the changes render on the chart for both lines from the time they received commands
					
					//RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
					

					// 1.4 - Rendering Custom Shapes

					// SharpDX namespace consists of several shapes you can use to draw objects more complicated than lines
					// For example, we can use the RectangleF object to draw a rectangle that covers the entire chart area
					//SharpDX.RectangleF rect = new SharpDX.RectangleF(StartPointT.X, StartPointT.Y, width, height);

					// The RenderTarget consists of two commands related to Rectangles.
					// The FillRectangle() method is used to "Paint" the area of a Rectangle

					// and DrawRectangle() is used to "Paint" the outline of a Rectangle

					// Another example is an ellipse which can be used to draw circles
					// The ellipse center point can be used from the Vectors calculated earlier 
					// The width and height an absolute 100 device pixels
					// To ensure that pixel coordinates work across all DPI devices, we use the NinjaTrader ChartingExteions methods
					// Which will convert the "100" value from WPF pixels to Device Pixels both vertically and horizontally
//					int ellipseRadiusY = ChartingExtensions.ConvertToVerticalPixels(100, ChartControl.PresentationSource);
//					int ellipseRadiusX = ChartingExtensions.ConvertToHorizontalPixels(100, ChartControl.PresentationSource);
					
					//SharpDX.Direct2D1.Ellipse ellipse = new SharpDX.Direct2D1.Ellipse(center, ellipseRadiusX, ellipseRadiusY);

					// 1.5 - Complex Brush Types and Shapes
					// For this ellipse, we can use one of the more complex brush types "RadialGradientBrush"
					// Warning:  RadialGradientBrush objects must be disposed of after they have been used
//					SharpDX.Direct2D1.RadialGradientBrush radialGradientBrush;

					// However creating a RadialGradientBrush requires a few more properties than SolidColorBrush
					// First, you need to define the array gradient stops the brush will eventually use
//					SharpDX.Direct2D1.GradientStop[] gradientStops = new SharpDX.Direct2D1.GradientStop[2];

//					// With the gradientStops array, we can describe the color and position of the individual gradients
//					gradientStops[0].Color = SharpDX.Color.Goldenrod;
//					gradientStops[0].Position = 0.0f;
//					gradientStops[1].Color = SharpDX.Color.SeaGreen;
//					gradientStops[1].Position = 1.0f;

					// then declare a GradientStopCollection from our render target that uses the gradientStops array defined just before
					// Warning:  GradientStopCollection objects must be disposed of after they have been used

//					SharpDX.Direct2D1.GradientStopCollection gradientStopCollection =
//						new SharpDX.Direct2D1.GradientStopCollection(RenderTarget, gradientStops);

					// we also need to tell our RadialGradientBrush to match the size and shape of the ellipse that we will be drawing
					// for convenience, SharpDX provides a RadialGradientBrushProperties structure to help define these properties
//					SharpDX.Direct2D1.RadialGradientBrushProperties radialGradientBrushProperties =
//						new SharpDX.Direct2D1.RadialGradientBrushProperties
//						{
//							GradientOriginOffset = new SharpDX.Vector2(0, 0),
//							Center = ellipse.Point,
//							RadiusX = ellipse.RadiusY,
//							RadiusY = ellipse.RadiusY
//						};


					// we now have everything we need to create a radial gradient brush
//					var radialGradientBrush = new SharpDX.Direct2D1.RadialGradientBrush(RenderTarget, radialGradientBrushProperties,
//						gradientStopCollection);

					// Finally, we can use this radialGradientBrush to "Paint" the area of the ellipse

					// 1.6 - Simple Text Rendering

					// For rendering custom text to the Chart, there are a few ways you can approach depending on your requirements
					// The most straight forward way is to "borrow" the existing chartControl font provided as a "SimpleFont" class
					// Using the chartControl LabelFont, your custom object will also change to the user defined properties allowing 
					// your object to match different fonts if defined by user.  

					// The code below will use the chartControl Properties Label Font if it exists,
					// or fall back to a default property if it cannot obtain that value
					//NinjaTrader.Gui.Tools.SimpleFont wpfFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);

					// the advantage of using a SimpleFont is they are not only very easy to describe 
					// but there is also a convenience method which can be used to convert the SimpleFont to a SharpDX.DirectWrite.TextFormat used to render to the chart
					// Warning:  TextFormat objects must be disposed of after they have been used
					//SharpDX.DirectWrite.TextFormat textFormat1 = wpfFont.ToDirectWriteTextFormat();

//					textFormat1			= TextFont3.ToDirectWriteTextFormat();	
						
//					// Once you have the format of the font, you need to describe how the font needs to be laid out
//					// Here we will create a new Vector2() which draws the font according to the to top left corner of the chart (offset by a few pixels)
//					SharpDX.Vector2 upperTextPoint = new SharpDX.Vector2(ChartPanel.X + 10, ChartPanel.Y + 20);
//					// Warning:  TextLayout objects must be disposed of after they have been used
//					SharpDX.DirectWrite.TextLayout textLayout1 =
//						new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
//							NinjaTrader.Custom.Resource.SampleCustomPlotUpperLeftCorner, textFormat1, ChartPanel.X + ChartPanel.W,
//							textFormat1.FontSize);

//					// With the format and layout of the text completed, we can now render the font to the chart
//					Print(ChartControl ==  null);
//					Print(SortedList2.Count);
					//Print("ChartControl.Indicators " + ChartControl.Indicators.Count);	
					
					addedtwice = 0;
						

					if (ChartControl.Indicators.Count > 1)
					{
						pCopierIsEnabled = false;
						
						foreach (IndicatorBase ii in ChartControl.Indicators)
		                //foreach (IndicatorBase ii in chartTrader.TrackableIndicators)
		                {
							
							if (ii.Name == "aiDuplicateAccountsActions")
								addedtwice++;
							
							//Print(ii.Name);
							
							
							
						}
						
						if (addedtwice > 1)
							//AddMessage("You have added the 'aiDuplicateAccountsActionsBen' indicator to the Chart window more than one time.  Right click in the Chart window, click Indicators... and remove the extra instance of 'aiDuplicateAccountActions'.", 10, RED);
							AddMessage("You added the 'aiDuplicateAccountsActions' indicator to this Chart window multiple times.  Please remove all extra instances of the 'aiDuplicateAccountActions' indicator.", 10, RED);
						if (addedtwice == 1)
							AddMessage("Please remove all indicators from the Chart window except the 'aiDuplicateAccountsActions' indicator.", 10, RED);
					
					}

					bool dooooooossd = false;

					if (NonCompatibleInstrument != string.Empty)
					{
						
						AddMessage("The instrument '" + NonCompatibleInstrument + "' is not compatible with the 'Type' column.  You can reach out to our team if you think this instrument should be added.", 30, RED);
						
						NonCompatibleInstrument = string.Empty;
					}
				
					
				//Print("ThisR 6");	
					
					
				starty = 0;	
						
				foreach (KeyValuePair<double, PendingDetails> kvp in LimitPriceToTimeOutOfSync2)
				{
					TimeSpan TimeSinceLoad = DateTime.Now - kvp.Value.StartTime;
					
					int tmm = (int) TimeSinceLoad.TotalMilliseconds;
					
					
					if (tmm < 1000)
						continue;				
	
							
							//CellFormat			= myProperties.LabelFont.ToDirectWriteTextFormat();	
							CellFormat			= TextFont3.ToDirectWriteTextFormat();	
							
										
		                	CellFormat = new SimpleFont(TextFont3.Family.ToString(), TextFont3.Size+pFontSizeTopRow2).ToDirectWriteTextFormat();					
	
		
					
							//Print(ThisMessage);
							
							float leftrightpad = 8;
							float leftspace = (float) 8; // left pad of buttons	
						
							//leftspace = leftspace + leftrightspace;
							
							leftspace = pPixelsFromTop-1;
						
							//Print("F 3");
							
							
							float sw = 0;
//							sw = ThisStroke.Width;	
							
//							if (!pActiveOutlineEnabled)
//								sw = 0;
														
						
							ThisMessage = "hey";
					
							CellString = ThisMessage;
							CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat, 10000, 10000);
							
		                    float FinalH = CellLayout.Metrics.Height;
		                    float FinalW = CellLayout.Metrics.Width;
							
					
							float hh = FinalH + ppppppp;
						
							
							
							starty = starty - hh - ppppppp2; 

				}
				
				firstmessage = true;			

				if (!IsInHitTest)
					foreach (MessageAlerts D in AllMessages)
					{
						ThisMessage = D.Name;
	
						if (IsKorean)
						{
							if (ThisMessage == TheWarningMessage)
								ThisMessage = "마스터 계정을 거래하는 동안 모든 서브 계정의 포지션 및 주문을 모니터링하는 것은 트레이더의 책임입니다. 여러 계정에 거래를 카피하는 과정은 진입/청산 방법, 기타 타사 소프트웨어 및 연결 상태 등 여러 요인에 따라 복잡할 수 있습니다. 항상 NinjaTrader 데스크톱 플랫폼 외부에서도 포지션과 주문을 관리할 준비가 되어 있어야 합니다.";
							
						}
	
						if (D.Switch)
							if (ThisMessage != string.Empty)
							{
							
								
								CellFormat			= TextFont3.ToDirectWriteTextFormat();	
											
			                	CellFormat = new SimpleFont(TextFont3.Family.ToString(), TextFont3.Size+pFontSizeTopRow3).ToDirectWriteTextFormat();		
								
								
								CellString = ThisMessage;
								CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat,10000, 10000);
								
			                    float FinalH = CellLayout.Metrics.Height;
			                    float FinalW = CellLayout.Metrics.Width;
								
								float hh = FinalH + 4; // vertical padding in message box
								hh = hh; 
								
								if (firstmessage)
									starty = starty - hh - ppppppp2 - 6; // add space between message boxes
								else
									starty = starty - hh - 6;
								
								firstmessage = false;
		
									
							}
							
					}				

				totaladjustment = starty*-1;//+15;

				if (AllowRedraw)
				{
					AllDrawings.Clear();

				AllColumns.Clear();
				List<string> TempRow = new List<string>();
				List<string> TempRow2 = new List<string>();
				List<string> TempRow3 = new List<string>();
				List<string> TempRow4 = new List<string>();
				List<string> TempRow5 = new List<string>();
				List<string> TempRow6 = new List<string>();
				List<string> TempRow7 = new List<string>();
				List<string> TempRow8 = new List<string>();
				List<string> TempRow9 = new List<string>();
				List<string> TempRow10 = new List<string>();					
				List<string> TempRow11 = new List<string>();	
				List<string> TempRow12 = new List<string>();	
				List<string> TempRow13 = new List<string>();
				List<string> TempRow14 = new List<string>();
				List<string> TempRow15 = new List<string>();
				List<string> TempRow16 = new List<string>();					
				List<string> TempRow17 = new List<string>();	
				List<string> TempRow18 = new List<string>();	
				List<string> TempRow19 = new List<string>();
				List<string> TempRow20 = new List<string>();
				List<string> TempRow21 = new List<string>();					
				List<string> TempRow22 = new List<string>();	
				List<string> TempRow23 = new List<string>();
				List<string> TempRow24 = new List<string>();
				List<string> TempRow25 = new List<string>();
				List<string> TempRow26 = new List<string>();
				List<string> TempRow27 = new List<string>();
				List<string> TempRow28 = new List<string>();
				List<string> TempRow29 = new List<string>();
				List<string> TempRow30 = new List<string>();

				AllColumns.Add(new List<string>(TempRow));
				
				
				
				
					TempRow.Clear();	
					TempRow2.Clear();
					TempRow3.Clear();
					TempRow4.Clear();
					TempRow5.Clear();
					TempRow6.Clear();
					TempRow7.Clear();
					TempRow8.Clear();
					TempRow9.Clear();
					TempRow10.Clear();
					TempRow11.Clear();
					TempRow12.Clear();
					TempRow13.Clear();
					TempRow14.Clear();
					TempRow15.Clear();
					TempRow16.Clear();
					TempRow17.Clear();
					TempRow18.Clear();
					TempRow19.Clear();
					TempRow20.Clear();
					TempRow21.Clear();
					TempRow22.Clear();
					TempRow23.Clear();
					TempRow24.Clear();
					TempRow25.Clear();					
					TempRow26.Clear();
					TempRow27.Clear();
					TempRow28.Clear();
					TempRow29.Clear();
					TempRow30.Clear();					
				
					TempRow.Add("");
					TempRow2.Add("");
					TempRow3.Add("");
					TempRow4.Add("");
					TempRow5.Add("");
					TempRow6.Add("");	
					TempRow7.Add("");
					TempRow8.Add("");	
					TempRow9.Add("");
					TempRow10.Add("");	
					TempRow11.Add("");
					TempRow12.Add("");
					TempRow13.Add("");
					TempRow14.Add("");
					TempRow15.Add("");
					TempRow16.Add("");	
					TempRow17.Add("");
					TempRow18.Add("");
					TempRow19.Add("");
					TempRow20.Add("");
					TempRow21.Add("");
					TempRow22.Add("");
					TempRow23.Add("");					
					TempRow24.Add("");
					TempRow25.Add("");					
					TempRow26.Add("");
					TempRow27.Add("");
					TempRow28.Add("");
					TempRow29.Add("");
					TempRow30.Add("");
				
				
					//Print(sizecolname);
				
			
					TempRow.Add("Instrument");
					TempRow2.Add("Fade");
					TempRow3.Add("–");
					TempRow4.Add("Qty");
					TempRow5.Add("Trend");
					TempRow6.Add("Account");
					TempRow7.Add("Commissions");
					TempRow8.Add("Cash Value");	
					TempRow9.Add("Short");
					TempRow10.Add("Unrealized");
					TempRow11.Add("Gross Realized");					
					TempRow12.Add("Connected Status");
					TempRow13.Add("Realized");		
					TempRow14.Add("X");	
					TempRow15.Add("Total PNL");	
					TempRow16.Add("Net Liquidation");
					TempRow17.Add(sizecolname);		 //sizecolname			
					TempRow18.Add("Type");
					
					if (pCombineLongShort)
						LongColumnName = "Pos";
					
					TempRow19.Add(LongColumnName);
				
					TempRow20.Add("Auto Liquidate");	
					TempRow21.Add("From Closed");	
					
					
					TempRow22.Add("Auto Close");	
					TempRow23.Add("From Funded");	
					TempRow24.Add("Daily Goal");	
					TempRow25.Add("Daily Loss");							
					TempRow26.Add("Actions");
					TempRow27.Add("Auto Exit");
					TempRow28.Add("Payout");
					TempRow29.Add(modecoln);
					TempRow30.Add("Avg Price");
					
					
				
				// use to size width of column, text wont show
				
				
											
				
					TempRow.Add("");
					TempRow2.Add("");
					TempRow3.Add("");
					TempRow4.Add("");
					TempRow5.Add("");
					TempRow6.Add("");
					TempRow7.Add("");
					TempRow8.Add("");	
					TempRow9.Add("");
					TempRow10.Add("");	
					TempRow11.Add("");
					TempRow12.Add("");
					TempRow13.Add("");
					TempRow14.Add("");
					TempRow15.Add("");
					TempRow16.Add("");	
					TempRow17.Add("0.00x");
					TempRow18.Add("Micro");
					TempRow19.Add("");
					TempRow20.Add("");
					TempRow21.Add("");					
					TempRow22.Add("");	
					TempRow23.Add("");	
					TempRow24.Add("");	
					TempRow25.Add("");	
					//TempRow26.Add("");	
					
					TempRow26.Add("One Trade");
					
//						if (TotalActions != 0)
//						{
//							TempRow26.Add("One Trade");
//						}
//						else
//						{
//							TempRow26.Add("Ghost Orders [10]");
//						}
					
					
					TempRow27.Add("");
					TempRow28.Add("");
					TempRow29.Add("");
					TempRow30.Add("");					

					try
					{

						bool empty = AllPNLStatistics2 == null;

						//Print("empty  " + empty);
							
							if (!empty)
							{
								//Print(AllBOTChart2.Count + " / " +SortedList2.Count);
								
															//Print("TEST 1");
							
								
							//	Print("FinalPNLStatistics.Count " + FinalPNLStatistics.Count);
								
								
								AllPNLStatistics2.Clear();
							
								foreach(KeyValuePair<string,PNLStatistics> H in FinalPNLStatistics)
								{
									
									//DateTime CM = DateTime.Now.AddSeconds(-5);
									
									
									//if (H.Value.LastUpdate >= CM)
									//{
										
										// may want to actually delete from list of AllBOTCharts
										//AllBOTChart2.Add(H.Value);
										
								//	}
									
									AllPNLStatistics2.Add(H.Value);
									
								}
							
								
								
								//if (AllBOTChart2.Count >= SortedList2.Count)
									
								
//									Print("AllBOTChart2 " + AllBOTChart2.Count);
									
								//Print(pSelectedColumn);
								
								
								//sort by private name
								
								// o=>o.Acct.Name
								// o=>o.PrivateName
								
						
						if (pSelectedColumn == "Account")
						{
							
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.PrivateName).ToList();
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.PrivateName).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.PrivateName).ToList();

							}							
														
							
							
							
							
							
						}		
													
						else if (pSelectedColumn == "Cash Value")
						{
							
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.CashValue).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.CashValue).ThenBy(o=>o.PrivateName).ToList();
							//SortedList2 = AllPNLStatistics2.OrderByDescending(o=>o.CashValue).ThenBy(o=>o.PrivateName).ThenBy(o=>o.Name2).ThenBy(o=>o.Name).ToList();
							
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.CashValue).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.CashValue).ToList();

							}							
							
							
							
							
							
							
						}
						else if (pSelectedColumn == "Net Liquidation")
						{
							
							
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.NetLiquidation).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.NetLiquidation).ThenBy(o=>o.PrivateName).ToList();
							
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.NetLiquidation).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.NetLiquidation).ToList();

							}
							
							
							
							
							//SortedList2 = AllPNLStatistics2.OrderByDescending(o=>o.CashValue).ThenBy(o=>o.PrivateName).ThenBy(o=>o.Name2).ThenBy(o=>o.Name).ToList();
						}	
						
						else if (pSelectedColumn == "From Funded")
						{
							
							
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.FromFunded).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.FromFunded).ThenBy(o=>o.PrivateName).ToList();
							
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.FromFunded).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.FromFunded).ToList();

							}
							
							
						}							
						
						else if (pSelectedColumn == "From Closed")
						{
							
							
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.FromBlown).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.FromBlown).ThenBy(o=>o.PrivateName).ToList();
							
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.FromBlown).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.FromBlown).ToList();

							}
							
							
						}	
																
						
						else if (pSelectedColumn == "Auto Liquidate")
						{
							
							
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.AudioLiq).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.AudioLiq).ThenBy(o=>o.PrivateName).ToList();
							
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.AudioLiq).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.AudioLiq).ToList();

							}
							
							
						}	
												
						else if (pSelectedColumn == "Realized")
						{
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.PNLR).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.PNLR).ThenBy(o=>o.PrivateName).ToList();
					
							
							
							
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.PNLR).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.PNLR).ToList();

							}							
							
							
							
						}						
						else if (pSelectedColumn == "Gross Realized")
						{
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.PNLGR).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.PNLGR).ThenBy(o=>o.PrivateName).ToList();

							
							
							
								
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.PNLGR).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.PNLGR).ToList();

							}							
							
							
							
						}		
						else if (pSelectedColumn == "Unrealized")
						{
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.PNLU).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.PNLU).ThenBy(o=>o.PrivateName).ToList();

							
							
							
							
								
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.PNLU).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.PNLU).ToList();

							}								
							
							
							
						}		
						else if (pSelectedColumn == "Total PNL")
						{
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.PNLTotal).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.PNLTotal).ThenBy(o=>o.PrivateName).ToList();

							
							
							
									
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.PNLTotal).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.PNLTotal).ToList();

							}							
							
							
							
						}	
						else if (pSelectedColumn == "Commissions")
						{
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.Commish).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.Commish).ThenBy(o=>o.PrivateName).ToList();

							
							
							
									
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.Commish).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.Commish).ToList();

							}							
							
							
							
							
						}							
						else if (pSelectedColumn == "Qty")
						{
							if (pSelectedAscending)
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.TotalBought).ThenBy(o=>o.PrivateName).ToList();
							else
								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.TotalBought).ThenBy(o=>o.PrivateName).ToList();

							
							
							
									
							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.TotalBought).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.TotalBought).ToList();

							}							
							
							
							
							
							
						}	
						else if (pSelectedColumn == "Type")
						{
//							if (pSelectedAscending)
//								SortedList2 = AllPNLStatistics2.OrderBy(o=>o.TotalBought).ThenBy(o=>o.PrivateName).ToList();
//							else
//								SortedList2 = AllPNLStatistics2.OrderByDescending(o=>o.TotalBought).ThenBy(o=>o.PrivateName).ToList();

							
							
							
									
//							if (pForceAllSelectedAccountsToTop)
//							{
	
//								if (pSelectedAscending)
//									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.TotalBought).ToList();
//								else
//									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.TotalBought).ToList();

//							}							
							
							
							
							
							
						}	
						else
						{

							if (pForceAllSelectedAccountsToTop)
							{
	
								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenBy(o=>o.PrivateName).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.AudioStatus).ThenByDescending(o=>o.PrivateName).ToList();

							}							
							else
							{

								if (pSelectedAscending)
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenBy(o=>o.PrivateName).ToList();
								else
									SortedList2 = AllPNLStatistics2.OrderBy(o=>o.MasterStatus).ThenByDescending(o=>o.PrivateName).ToList();

							}		

						}
						
					}

					}
					catch
					{
						p($"{line}  render error {DateTime.Now.ToLongTimeString()} - ");
					}
							
//return;
				

					
//					Print("SortedList2 " + SortedList2.Count);
			
					
					//Print("ThisR 9");	
					
					bool FirstLoop = true;
					
	               // foreach (BOTChart thisbutton in AllBOTChart2.ToList())
					foreach (PNLStatistics thisdata in SortedList2.ToList())	
						
	                //foreach (string xxx in AllButtons)
	                {

	                    //string xxx = thisbutton.Value.Text;
						
						
						string thisrowacct = thisdata.Acct.Name;
						
						
					
						
						
						TempRow.Add(thisdata.Inst.FullName);
						
						
						//if (thisbutton.Name != null) TempRow2.Add("");
						
						TempRow3.Add("–");
						
//						if (thisbutton.Width != null) TempRow4.Add("");
									
//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
						
//						if (thisbutton.Direction != null) TempRow13.Add("");
//						if (thisbutton.Switch != null) TempRow5.Add("");
						
			
						
						
//						if (thisbutton.Hovered != null) TempRow6.Add("");
						
						
						string gaf = GetAccountFade(thisdata.Acct.Name);
						
						TempRow2.Add(gaf);
						//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
						
						TempRow4.Add(thisdata.TotalBought.ToString());
						
						//TempRow15.Add("");
						
						TempRow5.Add("");
						
						
						string thisrowacct2 = thisrowacct;
						
						thisrowacct2 = thisdata.PrivateName;
						
						
						if (pHideAccountsIsEnabled)
						{
							string t1 = Regex.Replace(thisrowacct2, "[0-9]", "–");
							thisrowacct2 = t1;
						}
							
							
//						thisrowacct2 = thisrowacct.Replace("51383", "51393"); // larry hide apex id
//						thisrowacct2 = thisrowacct.Replace("53720", "53730"); // joel hide apex id
		
							
//						if (pPrivacyEnabled)
//						{

//							pAccountPrivacy1 = pAccountPrivacy1.Replace(" ", "");
//							if (pAccountPrivacy1 != string.Empty)
//							thisrowacct2 = thisrowacct2.Replace(pAccountPrivacy1, pAccountReplace1);

							
//							thisrowacct2 = thisrowacct2.Replace("--", "-");	
//						}
								
								
						
						TempRow6.Add(thisrowacct2);
						

						string ammm = GetAccountMultiplier(thisrowacct);
						
										
	
//						if (ammm == "0")
//							ammm = "0.50";
//						if (ammm == "-1")
//							ammm = "0.33";
//						if (ammm == "-2")
//							ammm = "0.25";
//						if (ammm == "-3")
//							ammm = "0.20";
						
						
						if (ammm == "0")
							ammm = "1/2";
						if (ammm == "-1")
							ammm = "1/3";
						if (ammm == "-2")
							ammm = "1/4";
						if (ammm == "-3")
							ammm = "1/5";	
						
						
						//Print(pThisMasterAccount + "    " + thisrowacct)
						
						
						
						
						
						
						if (pThisMasterAccount != thisrowacct)
						{
						
//							if (pCrossType == "Mini")
//							{
//								if (AllCrossAccounts.Contains(thisrowacct))
//									TempRow18.Add("Micro");
//								else
//									TempRow18.Add("Mini");							
								
//							}
//							else
//							{							
								if (AllCrossAccounts.Contains(thisrowacct))
									TempRow18.Add("Mini");
								else
									TempRow18.Add("Micro");								
								
						//	}
							
							string adddddd = "";
								
							if (pMultiplierMode == "Multiplier")
								adddddd = "x";
								
								TempRow17.Add(ammm+adddddd);
							
							//Print("ThisR 9a");
							
							string ammm2 = GetAccountMode(thisrowacct);
							
							//Print("ThisR 9b");
							
							
							TempRow29.Add(ammm2);
							
							
						}
						else
						{
							TempRow18.Add("");
							TempRow17.Add("");
							TempRow29.Add("");
						}
						

						if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
						if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
						
				
//						TempRow30.Add("");
						
							if (thisdata.AllPositions != null)
							{
								
								if (thisdata.AllPositions.Count == 0)
								{
									
									TempRow30.Add("");
								}								
								else if (thisdata.AllPositions.Count == 1)
								{
									
									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
								}
								else if (thisdata.AllPositions.Count > 1)
								{
									//TempRow30.Add("MULTIPLE " +thisdata.Acct.Name);
									TempRow30.Add("Multiple");
								}
								
								
							}
													
//						if (pCombineLongShort)
//						{
//							if (thisdata.PositionShort != null) TempRow19.Add(thisdata.PositionShort.ToString());
//						}
						
							
		
		
						
						if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
						if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
						if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
						if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
						
						TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));


						string cv = GetDollarString("$", thisdata.CashValue);
						string nl = GetDollarString("$", thisdata.NetLiquidation);
										
						if (pHideCurrencyIsEnabled)
						{
//							cv = "$--------.--";
//							nl = "$--------.--";
							cv = Regex.Replace(cv, "[0-9]", "–");
							nl = Regex.Replace(nl, "[0-9]", "–");
						}
						
						
						TempRow8.Add(cv);
						TempRow16.Add(nl);
						
						
						
						
						TempRow12.Add("");
						
						if (pColumnCloseE) TempRow14.Add("X");
						
//										if (thisbutton.Indy.myAccount != null)
//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
										
							
	
						
						TempRow20.Add(GetDollarStringHide("$", thisdata.AudioLiq));
						TempRow21.Add(GetDollarStringHide("$", thisdata.FromBlown));
						
						TempRow22.Add(GetSavedAutoClose(thisdata.Acct.Name));
						
						TempRow23.Add(GetDollarStringHide("$", thisdata.FromFunded));
						
						
						
						
//						TempRow24.Add(GetDollarStringZero("$", 2000));
//						TempRow25.Add(GetDollarStringZero("$", 1000));
						
						double CurrentDailyGoal = 0;
						double CurrentDailyLoss = 0;
						double CurrentPayout = 0;
						string CurrentSwitch = "no";
						
						
						
						
						if (pIsRiskFunctionsChecked)
						{
							CurrentDailyGoal = GetAccountSavedData(thisdata.Acct.Name, pAllAccountDailyGoal);
							CurrentDailyLoss = GetAccountSavedData(thisdata.Acct.Name, pAllAccountDailyLoss);
							CurrentPayout = GetAccountSavedData(thisdata.Acct.Name, pAllAccountPayouts);
							CurrentSwitch = GetSavedAccountExitSwitch(thisdata.Acct.Name);
							
							// shorten column width
							
							if (CurrentDailyGoal == -1000000000)
								CurrentDailyGoal = 0;
							if (CurrentDailyLoss == -1000000000)
								CurrentDailyLoss = 0;							
							
						}
					
					
						if (pDailyGoalDisplayMode == "Status" && SelectedDailyGoalAccount != thisdata.Acct.Name)
						{
							
							CurrentDailyGoal = CurrentDailyGoal - thisdata.PNLTotal;
							//CurrentDailyGoal = Math.Max(0, CurrentDailyGoal);
							
							TempRow24.Add(GetDollarString("$", CurrentDailyGoal).Replace(".00",""));
							
						}
						else
						{
							TempRow24.Add(GetDollarStringZero("$", CurrentDailyGoal));
							
						}
						
						
						
						
						
						
						
						
						if (pDailyLossDisplayMode == "Status" && SelectedDailyLossAccount != thisdata.Acct.Name)
						{
							
							CurrentDailyLoss = CurrentDailyLoss + thisdata.PNLTotal;
							
							//CurrentDailyLoss = Math.Max(0, CurrentDailyLoss);
							
							
							
							TempRow25.Add(GetDollarString("$", CurrentDailyLoss).Replace(".00",""));
							
						}
						else
						{
							//Print(CurrentDailyLoss);
							
							
							TempRow25.Add(GetDollarStringZero("$", CurrentDailyLoss));
							
						}						
						
						
						
						
						
						//TempRow24.Add(GetDollarStringZero("$", CurrentDailyGoal));
						//TempRow25.Add(GetDollarStringZero("$", CurrentDailyLoss));
						
						
						TempRow27.Add(CurrentSwitch);
						
						//TempRow20.Add("");
						//TempRow21.Add("");

						
						
						TempRow28.Add(GetDollarStringZero("$", CurrentPayout));
						
						
						
//						if (pColumnProfitRequested)
//						{
							
							
//						}
						
						TempRow26.Add("");
						//TempRow27.Add("");
						//TempRow28.Add("");
						
						//
						
						
						
			
			
					}

	               // foreach (BOTChart thisbutton in AllBOTChart2.ToList())
					//foreach (PNLStatistics thisdata in TotalAccountPNLStatistics.ToList())	
						
					//bool pShowTotalRow = true;
					
					bool pSpaceBetweenRows = false;
					string totalrowname = "";
					
					bool pAddBlankBottomRow = true;
					
					
//return;
					//Print(IsOneTotal);
					
					NotAccountRows = 2;

					if (pThisMasterAccount != string.Empty)
						NotAccountRows = NotAccountRows + 1;
					
					if (pShowFollowersAtTop)
					if (AllDuplicateAccounts.Count != 0)
						NotAccountRows = NotAccountRows + AllDuplicateAccounts.Count;					

					if (IsOneTotal)
					{
					
						NotAccountRows = NotAccountRows + 1;
						
							
							
								TempRow.Add("");
								TempRow2.Add("");
								TempRow3.Add("");
								TempRow4.Add("");
								TempRow5.Add("");
								TempRow6.Add("");
								TempRow7.Add("");
								TempRow8.Add("");
								TempRow9.Add("");
								TempRow10.Add("");
								TempRow11.Add("");
								TempRow12.Add("");
								TempRow13.Add("");
								TempRow14.Add("");
								TempRow15.Add("");
								TempRow16.Add("");
								TempRow17.Add("");
								TempRow18.Add("");
								TempRow19.Add("");
								TempRow20.Add("");
								TempRow21.Add("");
								TempRow22.Add("");
								TempRow23.Add("");
								TempRow24.Add("");
								TempRow25.Add("");
								TempRow26.Add("");
								TempRow27.Add("");
								TempRow28.Add("");
								TempRow29.Add("");
								TempRow30.Add("");
						
						
					}
					
					
					
					
					
					
		
									AllTotalRowsS = new List<string>();	
									

					
					
									AllTotalRowsS.Add("TotalRow1");
									AllTotalRowsS.Add("TotalRow2");
									AllTotalRowsS.Add("TotalRow3");
									
									
									AllTotalRowsS.Add("TotalRow5");
									AllTotalRowsS.Add("TotalRow6");
									AllTotalRowsS.Add("TotalRow7");
														
									
									
									AllTotalRowsS.Add("TotalRow4");
									
									
									
									
									
//								bool show = false;	
									
//									if (show)
										
				//	Print("------------");
					
										
//return;
									foreach (string aaaaa in AllTotalRowsS)
										{
											
											totalrowname = aaaaa;
											
											
											
											
											
											//Total1Name
											
											
											if (TotalAccountPNLStatistics.ContainsKey(totalrowname))
											{
	
												string ThisRowName = "";
												
												if (totalrowname == "TotalRow1")
													ThisRowName = Total1Name;
												
												if (totalrowname == "TotalRow2")
													ThisRowName = Total2Name;													
												
												if (totalrowname == "TotalRow3")
													ThisRowName = Total3Name;
												
												if (totalrowname == "TotalRow4")
													ThisRowName = Total4Name;	
												
												if (totalrowname == "TotalRow5")
													ThisRowName = Total5Name;
												
												if (totalrowname == "TotalRow6")
													ThisRowName = Total6Name;													
												
												if (totalrowname == "TotalRow7")
													ThisRowName = Total7Name;
												
											
												NotAccountRows = NotAccountRows + 1;
					
										
												
												if (totalrowname == "TotalRow4")		
												{
													NotAccountRows = NotAccountRows + 1;
													
																
												
												
													TempRow.Add("");
													TempRow2.Add("");
													TempRow3.Add("");
													TempRow4.Add("");
													TempRow5.Add("");
													TempRow6.Add("");
													TempRow7.Add("");
													TempRow8.Add("");
													TempRow9.Add("");
													TempRow10.Add("");
													TempRow11.Add("");
													TempRow12.Add("");
													TempRow13.Add("");
													TempRow14.Add("");
													TempRow15.Add("");
													TempRow16.Add("");
													TempRow17.Add("");
													TempRow18.Add("");
													TempRow19.Add("");
													TempRow20.Add("");
													TempRow21.Add("");
													TempRow22.Add("");
													TempRow23.Add("");
													TempRow24.Add("");
													TempRow25.Add("");
													TempRow26.Add("");
													TempRow27.Add("");
													TempRow28.Add("");
													TempRow29.Add("");
													TempRow30.Add("");		
													
												}
												
												
												
												
												PNLStatistics thisdata = TotalAccountPNLStatistics[totalrowname];
													
								                //foreach (string xxx in AllButtons)
								                {
					
							                    //string xxx = thisbutton.Value.Text;
												
												
												//if (thisbutton.Name != null) TempRow2.Add("");
												
												
												
						//						if (thisbutton.Width != null) TempRow4.Add("");
															
						//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
												
						//						if (thisbutton.Direction != null) TempRow13.Add("");
						//						if (thisbutton.Switch != null) TempRow5.Add("");
												
									
												
												
						//						if (thisbutton.Hovered != null) TempRow6.Add("");
												
												
										
												
												//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
												
												
												
												TempRow6.Add(ThisRowName);
												
												
												TempRow.Add("");
												TempRow2.Add("");
												
												
												TempRow3.Add(thisdata.TotalAccounts.ToString());
												
												//TempRow4.Add("");
												TempRow5.Add("");
												//TempRow6.Add("");
												
												
												//TempRow15.Add("");
												TempRow17.Add("");
												
												
												TempRow18.Add("");
												
										
											
																		
												
												//if (thisdata.PNLQ != null) TempRow9.Add(thisdata.PNLQ.ToString());
												
												
												if (thisdata.TotalBought != null) TempRow4.Add(thisdata.TotalBought.ToString());
												
												
												
					
											if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
											if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
											if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
											if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
											
											TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));
					
					
											string cv = GetDollarString("$", thisdata.CashValue);
											string nl = GetDollarString("$", thisdata.NetLiquidation);
															
											if (pHideCurrencyIsEnabled)
											{
					//							cv = "$--------.--";
					//							nl = "$--------.--";
												cv = Regex.Replace(cv, "[0-9]", "–");
												nl = Regex.Replace(nl, "[0-9]", "–");
											}
											
											
											TempRow8.Add(cv);
											TempRow16.Add(nl);
											
											
											
											
											
												if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
												if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
					
												
					//							if (thisdata.AllPositions != null)
					//							{
					//								if (thisdata.AllPositions.Count == 1)
					//								{
														
					//									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
					//								}
					//								else if (thisdata.AllPositions.Count > 1)
					//								{
					//									TempRow30.Add("MULTIPLE");
					//								}
													
					//							}
												
									
											
												
												
												TempRow12.Add("");
												
												if (pColumnCloseE) TempRow14.Add("X");
												
						//										if (thisbutton.Indy.myAccount != null)
						//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
																
													TempRow20.Add("");
													TempRow21.Add("");
													TempRow22.Add("");
													TempRow23.Add("");
													TempRow24.Add("");
													TempRow25.Add("");
													TempRow26.Add("");
													TempRow27.Add("");
													TempRow28.Add("");
													TempRow29.Add("");
													TempRow30.Add("");
												
												}
					
													
												if (pSpaceBetweenRows)
												{
												
												
												
													TempRow.Add("");
													TempRow2.Add("");
													TempRow3.Add("");
													TempRow4.Add("");
													TempRow5.Add("");
													TempRow6.Add("");
													TempRow7.Add("");
													TempRow8.Add("");
													TempRow9.Add("");
													TempRow10.Add("");
													TempRow11.Add("");
													TempRow12.Add("");
													TempRow13.Add("");
													TempRow14.Add("");
													TempRow15.Add("");
													TempRow16.Add("");
													TempRow17.Add("");
													TempRow18.Add("");
													TempRow19.Add("");
													TempRow20.Add("");
													TempRow21.Add("");
													TempRow22.Add("");
													TempRow23.Add("");
													TempRow24.Add("");
													TempRow25.Add("");
													TempRow26.Add("");
													TempRow27.Add("");
													TempRow28.Add("");
													TempRow29.Add("");
													TempRow30.Add("");
													
													
												}
								
											
											}	// end one total row
												
																					
											
										}
					
					
					
						bool oldold = false;
										
										

						if (oldold)
						{
						
							
							totalrowname = "TotalRow1";
					
							if (TotalAccountPNLStatistics.ContainsKey(totalrowname))
							{
						
								
								
								
								
								NotAccountRows = NotAccountRows + 1;
	
									
								PNLStatistics thisdata = TotalAccountPNLStatistics[totalrowname];
									
				                //foreach (string xxx in AllButtons)
				                {
	
			                    //string xxx = thisbutton.Value.Text;
								
								
								//if (thisbutton.Name != null) TempRow2.Add("");
								
								
								
		//						if (thisbutton.Width != null) TempRow4.Add("");
											
		//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
								
		//						if (thisbutton.Direction != null) TempRow13.Add("");
		//						if (thisbutton.Switch != null) TempRow5.Add("");
								
					
								
								
		//						if (thisbutton.Hovered != null) TempRow6.Add("");
								
								
						
								
								//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
								
								
								
								TempRow6.Add(Total1Name);
								
								
								TempRow.Add("");
								TempRow2.Add("");
								
								
								TempRow3.Add(thisdata.TotalAccounts.ToString());
								
								//TempRow4.Add("");
								TempRow5.Add("");
								//TempRow6.Add("");
								
								
								//TempRow15.Add("");
								TempRow17.Add("");
								
								
								TempRow18.Add("");
								
						
							
														
								
								//if (thisdata.PNLQ != null) TempRow9.Add(thisdata.PNLQ.ToString());
								
								
								if (thisdata.TotalBought != null) TempRow4.Add(thisdata.TotalBought.ToString());
								
								
								
	
							if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
							if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
							if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
							if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
							
							TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));
	
	
							string cv = GetDollarString("$", thisdata.CashValue);
							string nl = GetDollarString("$", thisdata.NetLiquidation);
											
							if (pHideCurrencyIsEnabled)
							{
	//							cv = "$--------.--";
	//							nl = "$--------.--";
								cv = Regex.Replace(cv, "[0-9]", "–");
								nl = Regex.Replace(nl, "[0-9]", "–");
							}
							
							
							TempRow8.Add(cv);
							TempRow16.Add(nl);
							
							
							
							
							
								if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
								if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
	
								
	//							if (thisdata.AllPositions != null)
	//							{
	//								if (thisdata.AllPositions.Count == 1)
	//								{
										
	//									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
	//								}
	//								else if (thisdata.AllPositions.Count > 1)
	//								{
	//									TempRow30.Add("MULTIPLE");
	//								}
									
	//							}
								
					
							
								
								
								TempRow12.Add("");
								
								if (pColumnCloseE) TempRow14.Add("X");
								
		//										if (thisbutton.Indy.myAccount != null)
		//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
												
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
								
								}
	
									
								if (pSpaceBetweenRows)
								{
								
								
								
									TempRow.Add("");
									TempRow2.Add("");
									TempRow3.Add("");
									TempRow4.Add("");
									TempRow5.Add("");
									TempRow6.Add("");
									TempRow7.Add("");
									TempRow8.Add("");
									TempRow9.Add("");
									TempRow10.Add("");
									TempRow11.Add("");
									TempRow12.Add("");
									TempRow13.Add("");
									TempRow14.Add("");
									TempRow15.Add("");
									TempRow16.Add("");
									TempRow17.Add("");
									TempRow18.Add("");
									TempRow19.Add("");
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
									
									
								}
				
							
							}	// end one total row
								
							
						
						
						
							
							totalrowname = "TotalRow2";
					
							if (TotalAccountPNLStatistics.ContainsKey(totalrowname))
							{
								
							//	Print("NotAccountRows " + NotAccountRows);
		
								NotAccountRows = NotAccountRows + 1;
									
								PNLStatistics thisdata = TotalAccountPNLStatistics[totalrowname];
									
				                //foreach (string xxx in AllButtons)
				                {
	
			                    //string xxx = thisbutton.Value.Text;
								
								
								//if (thisbutton.Name != null) TempRow2.Add("");
								
								
								
		//						if (thisbutton.Width != null) TempRow4.Add("");
											
		//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
								
		//						if (thisbutton.Direction != null) TempRow13.Add("");
		//						if (thisbutton.Switch != null) TempRow5.Add("");
								
					
								
								
		//						if (thisbutton.Hovered != null) TempRow6.Add("");
								
								
						
								
								//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
								
								
								
								TempRow6.Add(Total2Name);
								
								
								TempRow.Add("");
								TempRow2.Add("");
								
								//TempRow3.Add("–");
								TempRow3.Add(thisdata.TotalAccounts.ToString());
									
									
									
								//TempRow4.Add("");
								TempRow5.Add("");
								//TempRow6.Add("");
								
								
								//TempRow15.Add("");
								TempRow17.Add("");
								
								
								TempRow18.Add("");
								
						
							
														
								
								//if (thisdata.PNLQ != null) TempRow9.Add(thisdata.PNLQ.ToString());
								
								
								if (thisdata.TotalBought != null) TempRow4.Add(thisdata.TotalBought.ToString());
								
								
			
							if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
							if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
							if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
							if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
							
							TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));
	
	
							string cv = GetDollarString("$", thisdata.CashValue);
							string nl = GetDollarString("$", thisdata.NetLiquidation);
											
							if (pHideCurrencyIsEnabled)
							{
	//							cv = "$--------.--";
	//							nl = "$--------.--";
								cv = Regex.Replace(cv, "[0-9]", "–");
								nl = Regex.Replace(nl, "[0-9]", "–");
							}
							
							
							TempRow8.Add(cv);
							TempRow16.Add(nl);
							
							
							
								
	//							if (thisdata.PNLU != null) TempRow10.Add(GetDollarString("$", thisdata.PNLU));
	//							if (thisdata.PNLR != null) TempRow11.Add(GetDollarString("$", thisdata.PNLR));
	//							if (thisdata.PNLGR != null) TempRow13.Add(GetDollarString("$", thisdata.PNLGR));
	//							if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarString("$", thisdata.PNLTotal));
								
								 	
							
								if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
								if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
	
												
	//							if (thisdata.AllPositions != null)
	//							{
	//								if (thisdata.AllPositions.Count == 1)
	//								{
										
	//									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
	//								}
	//								else
	//								{
	//									TempRow30.Add("MULTIPLE");
	//								}
									
	//							}
								
								
								
	//							if (thisdata.Commish != null) TempRow7.Add(GetDollarString("$", thisdata.Commish));
	
	//							if (thisdata.CashValue != null) TempRow8.Add(GetDollarString("$", thisdata.CashValue));
	//							if (thisdata.NetLiquidation != null) TempRow16.Add(GetDollarString("$", thisdata.NetLiquidation));
								
								
								
							
							
								
								
								TempRow12.Add("");
								
								if (pColumnCloseE) TempRow14.Add("X");
								
		//										if (thisbutton.Indy.myAccount != null)
		//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
												
									
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
								
								
								}
	
									
								if (pSpaceBetweenRows)
								{
								
								
								
									TempRow.Add("");
									TempRow2.Add("");
									TempRow3.Add("");
									TempRow4.Add("");
									TempRow5.Add("");
									TempRow6.Add("");
									TempRow7.Add("");
									TempRow8.Add("");
									TempRow9.Add("");
									TempRow10.Add("");
									TempRow11.Add("");
									TempRow12.Add("");
									TempRow13.Add("");
									TempRow14.Add("");
									TempRow15.Add("");
									TempRow16.Add("");
									TempRow17.Add("");
									TempRow18.Add("");
									TempRow19.Add("");
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
									
								
								
								}
				
							
							}	// end one total row
								
							
							
							
							
							

							totalrowname = "TotalRow3";
					
							if (TotalAccountPNLStatistics.ContainsKey(totalrowname))
							//if (TotalAccountPNLStatistics.Count > 0)
							{
					
					
								NotAccountRows = NotAccountRows + 1;
								
								
	
								
								PNLStatistics thisdata = TotalAccountPNLStatistics[totalrowname];
									
				                //foreach (string xxx in AllButtons)
				                {
	
			                    //string xxx = thisbutton.Value.Text;
								
								
								//if (thisbutton.Name != null) TempRow2.Add("");
								
								
								
		//						if (thisbutton.Width != null) TempRow4.Add("");
											
		//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
								
		//						if (thisbutton.Direction != null) TempRow13.Add("");
		//						if (thisbutton.Switch != null) TempRow5.Add("");
								
					
								
								
		//						if (thisbutton.Hovered != null) TempRow6.Add("");
								
								
						
								
								//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
								
								
								
								TempRow6.Add(Total3Name);
								
								
								TempRow.Add("");
								TempRow2.Add("");
								
								//TempRow3.Add("–");
								//TempRow3.Add("");
								TempRow3.Add(thisdata.TotalAccounts.ToString());
									
									
								//TempRow4.Add("");
								TempRow5.Add("");
								//TempRow6.Add("");
								
								
								//TempRow15.Add("");
								TempRow17.Add("");
								
								
								TempRow18.Add("");
								
						
							
														
								
								//if (thisdata.PNLQ != null) TempRow9.Add(thisdata.PNLQ.ToString());
								
								
								if (thisdata.TotalBought != null) TempRow4.Add(thisdata.TotalBought.ToString());
								
								
								
	
							if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
							if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
							if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
							if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
							
							TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));
	
	
							string cv = GetDollarString("$", thisdata.CashValue);
							string nl = GetDollarString("$", thisdata.NetLiquidation);
											
							if (pHideCurrencyIsEnabled)
							{
	//							cv = "$--------.--";
	//							nl = "$--------.--";
								cv = Regex.Replace(cv, "[0-9]", "–");
								nl = Regex.Replace(nl, "[0-9]", "–");
							}
							
							
							TempRow8.Add(cv);
							TempRow16.Add(nl);
							
							
								
								 	
							
								if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
								if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
					
								
	//							if (thisdata.AllPositions != null)
	//							{
	//								if (thisdata.AllPositions.Count == 1)
	//								{
										
	//									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
	//								}
	//								else
	//								{
	//									TempRow30.Add("MULTIPLE");
	//								}
									
	//							}
								
			
								
							
							
								
								
								TempRow12.Add("");
								
								if (pColumnCloseE) TempRow14.Add("X");
								
		//										if (thisbutton.Indy.myAccount != null)
		//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
												
									
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
								
								}
	
									
								if (pSpaceBetweenRows)
								{
								
								
								
									TempRow.Add("");
									TempRow2.Add("");
									TempRow3.Add("");
									TempRow4.Add("");
									TempRow5.Add("");
									TempRow6.Add("");
									TempRow7.Add("");
									TempRow8.Add("");
									TempRow9.Add("");
									TempRow10.Add("");
									TempRow11.Add("");
									TempRow12.Add("");
									TempRow13.Add("");
									TempRow14.Add("");
									TempRow15.Add("");
									TempRow16.Add("");
									TempRow17.Add("");
									TempRow18.Add("");
									TempRow19.Add("");
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
									
								
								
								}
				
							
							}	// end one total row
								
							
							
							
							
							
							totalrowname = "TotalRow5";
					
							if (TotalAccountPNLStatistics.ContainsKey(totalrowname))
							//if (TotalAccountPNLStatistics.Count > 0)
							{
					
					
								NotAccountRows = NotAccountRows + 1;
								
								
	
								
								PNLStatistics thisdata = TotalAccountPNLStatistics[totalrowname];
									
				                //foreach (string xxx in AllButtons)
				                {
	
			                    //string xxx = thisbutton.Value.Text;
								
								
								//if (thisbutton.Name != null) TempRow2.Add("");
								
								
								
		//						if (thisbutton.Width != null) TempRow4.Add("");
											
		//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
								
		//						if (thisbutton.Direction != null) TempRow13.Add("");
		//						if (thisbutton.Switch != null) TempRow5.Add("");
								
					
								
								
		//						if (thisbutton.Hovered != null) TempRow6.Add("");
								
								
						
								
								//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
								
								
								
								TempRow6.Add(Total5Name);
								
								
								TempRow.Add("");
								TempRow2.Add("");
								
								//TempRow3.Add("–");
								//TempRow3.Add("");
								TempRow3.Add(thisdata.TotalAccounts.ToString());
									
									
								//TempRow4.Add("");
								TempRow5.Add("");
								//TempRow6.Add("");
								
								
								//TempRow15.Add("");
								TempRow17.Add("");
								
								
								TempRow18.Add("");
								
						
							
														
								
								//if (thisdata.PNLQ != null) TempRow9.Add(thisdata.PNLQ.ToString());
								
								
								if (thisdata.TotalBought != null) TempRow4.Add(thisdata.TotalBought.ToString());
								
								
								
	
							if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
							if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
							if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
							if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
							
							TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));
	
	
							string cv = GetDollarString("$", thisdata.CashValue);
							string nl = GetDollarString("$", thisdata.NetLiquidation);
											
							if (pHideCurrencyIsEnabled)
							{
	//							cv = "$--------.--";
	//							nl = "$--------.--";
								cv = Regex.Replace(cv, "[0-9]", "–");
								nl = Regex.Replace(nl, "[0-9]", "–");
							}
							
							
							TempRow8.Add(cv);
							TempRow16.Add(nl);
							
							
								
								 	
							
								if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
								if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
					
								
	//							if (thisdata.AllPositions != null)
	//							{
	//								if (thisdata.AllPositions.Count == 1)
	//								{
										
	//									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
	//								}
	//								else
	//								{
	//									TempRow30.Add("MULTIPLE");
	//								}
									
	//							}
								
			
								
							
							
								
								
								TempRow12.Add("");
								
								if (pColumnCloseE) TempRow14.Add("X");
								
		//										if (thisbutton.Indy.myAccount != null)
		//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
												
									
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
								
								}
	
									
								if (pSpaceBetweenRows)
								{
								
								
								
									TempRow.Add("");
									TempRow2.Add("");
									TempRow3.Add("");
									TempRow4.Add("");
									TempRow5.Add("");
									TempRow6.Add("");
									TempRow7.Add("");
									TempRow8.Add("");
									TempRow9.Add("");
									TempRow10.Add("");
									TempRow11.Add("");
									TempRow12.Add("");
									TempRow13.Add("");
									TempRow14.Add("");
									TempRow15.Add("");
									TempRow16.Add("");
									TempRow17.Add("");
									TempRow18.Add("");
									TempRow19.Add("");
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
									
								
								
								}
				
							
							}	// end one total row
								
							
													
								
					
							
							
							
							totalrowname = "TotalRow4";
					
							if (TotalAccountPNLStatistics.ContainsKey(totalrowname))
							//if (TotalAccountPNLStatistics.Count > 0)
							{
					
					
								NotAccountRows = NotAccountRows + 1;
								NotAccountRows = NotAccountRows + 1;
								
								
								
									TempRow.Add("");
									TempRow2.Add("");
									TempRow3.Add("");
									TempRow4.Add("");
									TempRow5.Add("");
									TempRow6.Add("");
									TempRow7.Add("");
									TempRow8.Add("");
									TempRow9.Add("");
									TempRow10.Add("");
									TempRow11.Add("");
									TempRow12.Add("");
									TempRow13.Add("");
									TempRow14.Add("");
									TempRow15.Add("");
									TempRow16.Add("");
									TempRow17.Add("");
									TempRow18.Add("");
									TempRow19.Add("");
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");						
								
								
								PNLStatistics thisdata = TotalAccountPNLStatistics[totalrowname];
									
				                //foreach (string xxx in AllButtons)
				                {
	
			                    //string xxx = thisbutton.Value.Text;
								
								
								//if (thisbutton.Name != null) TempRow2.Add("");
								
								
								
		//						if (thisbutton.Width != null) TempRow4.Add("");
											
		//						if (thisbutton.TimeStatus != null) TempRow15.Add("");
								
		//						if (thisbutton.Direction != null) TempRow13.Add("");
		//						if (thisbutton.Switch != null) TempRow5.Add("");
								
					
								
								
		//						if (thisbutton.Hovered != null) TempRow6.Add("");
								
								
						
								
								//TempRow4.Add(thisdata.TotalBought.ToString() + " / " + thisdata.TotalSold.ToString());
								
								
								
								TempRow6.Add(Total4Name);
								
								
								TempRow.Add("");
								TempRow2.Add("");
								
								//TempRow3.Add("–");
								//TempRow3.Add("");
								TempRow3.Add(thisdata.TotalAccounts.ToString());
								//TempRow4.Add("");
								TempRow5.Add("");
								//TempRow6.Add("");
								
								
								//TempRow15.Add("");
								TempRow17.Add("");
								
								
								TempRow18.Add("");
								
						
							
														
								
								//if (thisdata.PNLQ != null) TempRow9.Add(thisdata.PNLQ.ToString());
								
								
								if (thisdata.TotalBought != null) TempRow4.Add(thisdata.TotalBought.ToString());
								
								
								
								
	
							if (thisdata.PNLU != null) TempRow10.Add(GetDollarStringHide("$", thisdata.PNLU));
							if (thisdata.PNLR != null) TempRow11.Add(GetDollarStringHide("$", thisdata.PNLR));
							if (thisdata.PNLGR != null) TempRow13.Add(GetDollarStringHide("$", thisdata.PNLGR));
							if (thisdata.PNLTotal != null) TempRow15.Add(GetDollarStringHide("$", thisdata.PNLTotal));
							
							TempRow7.Add(GetDollarStringHide("$", thisdata.Commish));
	
	
							string cv = GetDollarString("$", thisdata.CashValue);
							string nl = GetDollarString("$", thisdata.NetLiquidation);
											
							if (pHideCurrencyIsEnabled)
							{
	//							cv = "$--------.--";
	//							nl = "$--------.--";
								cv = Regex.Replace(cv, "[0-9]", "–");
								nl = Regex.Replace(nl, "[0-9]", "–");
							}
							
							
							TempRow8.Add(cv);
							TempRow16.Add(nl);
							
							
							
							
							
								if (thisdata.PositionLong != null) TempRow19.Add(thisdata.PositionLong.ToString());
								if (thisdata.PositionShort != null) TempRow9.Add(thisdata.PositionShort.ToString());
					
								
	//							if (thisdata.AllPositions != null)
	//							{
	//								if (thisdata.AllPositions.Count == 1)
	//								{
										
	//									TempRow30.Add(FormatPriceMarker2(thisdata.AllPositions[0].AveragePrice));
	//								}
	//								else
	//								{
	//									TempRow30.Add("MULTIPLE");
	//								}
									
	//							}
		
							
							
								
								
								TempRow12.Add("");
								
								if (pColumnCloseE) TempRow14.Add("X");
								
		//										if (thisbutton.Indy.myAccount != null)
		//											TempRow7.Add(thisbutton.Indy.myAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).ToString());
												
									
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
								
								}
	
									
								if (pSpaceBetweenRows)
								{
								
								
								
									TempRow.Add("");
									TempRow2.Add("");
									TempRow3.Add("");
									TempRow4.Add("");
									TempRow5.Add("");
									TempRow6.Add("");
									TempRow7.Add("");
									TempRow8.Add("");
									TempRow9.Add("");
									TempRow10.Add("");
									TempRow11.Add("");
									TempRow12.Add("");
									TempRow13.Add("");
									TempRow14.Add("");
									TempRow15.Add("");
									TempRow16.Add("");
									TempRow17.Add("");
									TempRow18.Add("");
									TempRow19.Add("");
									TempRow20.Add("");
									TempRow21.Add("");
									TempRow22.Add("");
									TempRow23.Add("");
									TempRow24.Add("");
									TempRow25.Add("");
									TempRow26.Add("");
									TempRow27.Add("");
									TempRow28.Add("");
									TempRow29.Add("");
									TempRow30.Add("");
									
								
								}
				
							
							}	// end one total row
							
						
						
						}
					

					if (pAddBlankBottomRow)
					{
					
						NotAccountRows = NotAccountRows + 1;
						
					
							
							
								TempRow.Add("");
								TempRow2.Add("");
								TempRow3.Add("");
								TempRow4.Add("");
								TempRow5.Add("");
								TempRow6.Add("");
								TempRow7.Add("");
								TempRow8.Add("");
								TempRow9.Add("");
								TempRow10.Add("");
								TempRow11.Add("");
								TempRow12.Add("");
								TempRow13.Add("");
								TempRow14.Add("");
								TempRow15.Add("");
								TempRow16.Add("");
								TempRow17.Add("");
								TempRow18.Add("");
								TempRow19.Add("");
								TempRow20.Add("");
								TempRow21.Add("");
								TempRow22.Add("");
								TempRow23.Add("");
								TempRow24.Add("");
								TempRow25.Add("");
								TempRow26.Add("");
								TempRow27.Add("");
								TempRow28.Add("");
								TempRow29.Add("");
								TempRow30.Add("");
						
					
					}

					//return;
					


//					}
					
						
	
					
					// change order of columns
					
					
					
					
					//AllColumns.Add(new List<string>(TempRow));
					
					
					
					//AllColumns.Add(new List<string>(TempRow2));

					
					//if (pColumnSignalE)
			
					
					//if (pColumnAudioE) 
					
//					if (pCloseButtonLocation == "None")
//					{
//						if (pCloseButtonEnabled || pShowHideButton)
//							pCloseButtonLocation = "Right";
//					}
					
					
	
					
					AllColumns.Add(new List<string>(TempRow3));
					
		
					
					//if (!NoAccountsDisconnected)
					
//					if (pShowConnectedStatus)
//					AllColumns.Add(new List<string>(TempRow12));
					
					
					
					
					// account 
					AllColumns.Add(new List<string>(TempRow6));
					
				
					
					if (pCloseButtonLocation == "Left")
					if (pCloseButtonEnabled || pShowHideButton || pCancelButtonEnabled)	
						AllColumns.Add(new List<string>(TempRow14)); // close button
				
					
					bool DoHideActions = pActionsHideColumnEnabled && TotalActions == 0 && TotalFroze == 0 && RejectedAccounts.Count == 0;
					
					bool DoEnableActions = pActionsColumnEnabled || TotalActions != 0 || TotalFroze != 0 || RejectedAccounts.Count != 0;
					
					//DoHideActions = false;
					
		
					
					
					
					if (DoEnableActions)
					if (pActionsButtonLocation == "Left")
					{
						
						if (!DoHideActions)
							AllColumns.Add(new List<string>(TempRow26));
						
					}
					
					
					if (pIsCopyBasicFunctionsEnabled)
					{
				
						if (pIsXEnabled) AllColumns.Add(new List<string>(TempRow17));
						
						if (pIsCrossEnabled) AllColumns.Add(new List<string>(TempRow18));
						
						if (pIsATMSelectEnabled) AllColumns.Add(new List<string>(TempRow29)); // atm strategies
					
						if (pIsFadeEnabled) AllColumns.Add(new List<string>(TempRow2));
						
					}
					
					
						
						
						//if (pColumnT3E) 
							
							// position
							
							AllColumns.Add(new List<string>(TempRow19));
						
						
						
					
					// bought / sold
					

					
					if (!pCombineLongShort)	AllColumns.Add(new List<string>(TempRow9));
					
					if (pShowAvgPrice) AllColumns.Add(new List<string>(TempRow30));	// pnl total
					
				
					
					if (pUnrealizedColumn) AllColumns.Add(new List<string>(TempRow10)); // TempRow10.Add("Unrealized");
					if (pRealizedColumn) AllColumns.Add(new List<string>(TempRow13)); // TempRow13.Add("Realized");	
					
					if (pGrossRealizedColumn) AllColumns.Add(new List<string>(TempRow11)); // TempRow11.Add("Gross Realized");
					
						
						
					
					if (pCashValueColumn) AllColumns.Add(new List<string>(TempRow8));	// cash value
					
					
					if (pNetLiquidationColumn) AllColumns.Add(new List<string>(TempRow16));	// net liquidation
				
					
					
					if (pIsRiskFunctionsEnabled)
					{
							
						if (pColumnFromFund)
							AllColumns.Add(new List<string>(TempRow23));	
						
						if (pIsEvalCloseEnabled)
							AllColumns.Add(new List<string>(TempRow22));	
						
					
						if (pColumnAutoLiquidate) AllColumns.Add(new List<string>(TempRow20));	// pnl total
						if (pColumnRemaining) AllColumns.Add(new List<string>(TempRow21));	// pnl total
						
						
						//TotalFundedAccounts = 5;
						
						if (pColumnProfitRequested)	
						if (TotalFundedAccounts > 0)
							
							AllColumns.Add(new List<string>(TempRow28));	// pnl total
						
						
						
						
						if (pShowLossOnLeft)
						{
							if (pColumnDailyLoss) AllColumns.Add(new List<string>(TempRow25));	// pnl total
							if (pColumnDailyGoal) AllColumns.Add(new List<string>(TempRow24));	// pnl total
							
												
							
						}
						else
						{
							if (pColumnDailyGoal) AllColumns.Add(new List<string>(TempRow24));	// pnl total
							if (pColumnDailyLoss) AllColumns.Add(new List<string>(TempRow25));	// pnl total
												
							
							
						}

						if (pColumnDailyGoal || pColumnDailyLoss)
							AllColumns.Add(new List<string>(TempRow27));
					}
					
					
					
					
					if (pQtyColumn2) AllColumns.Add(new List<string>(TempRow4)); // contacts
					
					if (pCommissionsColumn) AllColumns.Add(new List<string>(TempRow7)); // commissions
					
					if (pTotalPNLColumn) AllColumns.Add(new List<string>(TempRow15));	// pnl total
					
					
					
					if (DoEnableActions)
					if (pActionsButtonLocation == "Right")
					{
						if (!DoHideActions)
							AllColumns.Add(new List<string>(TempRow26));
					}
				
					
					if (pCloseButtonLocation == "Right")
					if (pCloseButtonEnabled || pShowHideButton || pCancelButtonEnabled)		
						AllColumns.Add(new List<string>(TempRow14)); // close button
					

					// extra columns for future
					
					//AllColumns.Add(new List<string>(TempRow5));

					//TempRow.sort
//					AllColors.Clear();
//					List<int> TempRow2 = new List<int>();
//					AllColors.Add(new List<int>(TempRow2));

					// 1.7 - Advanced Text Rendering

					// Font formatting and text layouts can get as complex as you need them to be
					// This example shows how to use a complete custom font unrelated to the existing user-defined chart control settings
					// Warning:  TextLayout and TextFormat objects must be disposed of after they have been used
//					SharpDX.DirectWrite.TextFormat textFormat2 =
//						new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory, "Century Gothic", System.Windows.FontWeight.Bold,
//							System.Windows.FontStyle.Italic, 32f);
						
					SharpDX.DirectWrite.TextFormat textFormat2 = TextFont3.ToDirectWriteTextFormat();	 

					
		            textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading; // leading = left.
		            textFormat2.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center; // far = bottom.
		            textFormat2.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
					
					string TXT11 = "asd";
					string TXT12 = "as1";
					string TXT13 = "as2";
					string TXT21 = "as2";
					string TXT22 = "as3";
					string TXT23 = "a4d";
					
					// string text = "Total Levels";
					string text = "Trade Balances";
					//text.count
					
					var textLayout2 =
						new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, text, textFormat2, 100, textFormat2.FontSize);

					textLayout2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;	
					
		          	pPixelsFromTop = 10;
					pPixelsFromLeft = pPixelsFromTop;

					pPixelsFromLeft = (int) Math.Round(pPixelsFromLeft*dpiY/100, 0);
					pPixelsFromTop = (int) Math.Round(pPixelsFromTop*dpiY/100, 0);
					
					
					int CellPaddingX = (int)pLeftRightPad*2;
					int CellPaddingY = (int)pTopBottomPad*2;
					int TableMarginX = pPixelsFromLeft;
					int TableMarginY = pTopBottomPad*-1;//pPixelsFromTop;
				
					CellPaddingX = (int) Math.Round(CellPaddingX*dpiY/100, 0);
					CellPaddingY = (int) Math.Round(CellPaddingY*dpiY/100, 0);
					//TableMarginX = (int) Math.Round(TableMarginX*dpiY/100, 0);
					TableMarginY = (int) Math.Round(TableMarginY*dpiY/100, 0);
				
				
					int RectHeight = (int) textLayout2.Metrics.Height + CellPaddingY;
					int RectWidth = (int) textLayout2.Metrics.Width + CellPaddingX;

//					
				//RectWidth = 60;
				
					int RectX = ChartPanel.W - RectWidth - TableMarginX;
					int RectY = ChartPanel.H - RectHeight - TableMarginY;
					
					int actualtablestart = pPixelsFromTop;
					
					var rect2 = new SharpDX.RectangleF(pPixelsFromLeft, pPixelsFromTop, RectWidth, RectHeight);

					bool showheaderrow = true;
					
					
					if (showheaderrow)
					{
						// insert new header row
						
						
//						rect2			= new SharpDX.RectangleF(pPixelsFromLeft, pPixelsFromTop, RectWidth, RectHeight);
		
//						FillRectBoth(ExpandRect(rect2,0,0,0,0), br[ChartBackgroundID].DxBrush);
//						FillRectBoth(ExpandRect(rect2,1,1,1,0), br[TableBackgroundID].DxBrush);
				
					
						RectX = 0 + RectWidth + TableMarginX;
						RectY = 0 + RectHeight + TableMarginY;
						

					
						actualtablestart = pPixelsFromTop + RectHeight + pPixelsFromTop;

						// buttons
						
						LeftSpaceButtons = pPixelsFromLeft;
					
						
						
						
						space = 10;
						space = (int) Math.Round(space*dpiY/100, 0);
						
						//space = 0;
						
						CY = (float)chartControl.CanvasRight - 48f; // top of chart offset from right
						CY = (float)chartControl.CanvasRight - space + 3; // bottom of chart
						CY = LeftSpaceButtons;
						
						thistop = pPixelsFromTop;
						
						//space = ChartPanel.Height - 5;
		
						
						rightoftable = Math.Min(rightoftable, (float)chartControl.CanvasRight - 2);
					
						nextrightside = rightoftable;
					 
						lastrightside = nextrightside;
					
						//nextrightside = 500;
						
						//nextrightside = Math.Min(rightoftable, (float)chartControl.CanvasRight - 2);
							//return;
		
						
						
						leftsidebuttonsrightx = 0;
															
																
								
							
//						bool hehehehe = false;
						
//							if (hehehehe)
						
						
						
			            if (pButtonsEnabled)
						{
			                foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
			                {
								if (thisbutton.Value.Location != "Top")//only print buttons above the table
									continue;

								thisbutton.Value.Rect = new SharpDX.RectangleF(0,0,0,0);

								string ButtonName = thisbutton.Value.Text;
			                    string xxx = ButtonName;

								if (!pIsCopyBasicFunctionsPermission || !pIsRiskFunctionsPermission)
								{
									if (xxx == RiskButtonString)
										continue;
														
									if (xxx == CopyButtonString)
										continue;
								}
				
								
								if (!pIsCopyBasicFunctionsChecked || !pIsRiskFunctionsChecked)
								{
									if (xxx == RiskButtonString)
										continue;
														
									if (xxx == CopyButtonString)
										continue;
								}


								if (!pIsCopyBasicFunctionsChecked || !pIsCopyBasicFunctionsEnabled || !pIsCopyBasicFunctionsPermission)
								{
									if (xxx == "Main" || xxx == "Fade" || xxx == "Size" || xxx == "Mode" || xxx == "Type" || xxx == "Orders" || xxx == "Executions")
										continue;
								}
								
								
								if (xxx == "Orders" || xxx == "Executions")
									xxx = pCopierMode;

								if (xxx == "Fade")
			                		thisbutton.Value.Switch = pIsFadeEnabled;
			
								if (xxx == "Mode")
			                		thisbutton.Value.Switch = pIsATMSelectEnabled;

								if (!pExitShieldFeaturesEnabled && xxx == "Exit Shield")
									continue;					

								if (!pIsBuildMode && xxx == "Reset")
									continue;
								
								if (!pIsBuildMode && xxx == "Accounts")
									continue;
								
								if (!pIsBuildMode && xxx == "Live Accounts")
									continue;
								
								if (!pIsBuildMode && xxx == "Sim Accounts")
									continue;
								
								if (xxx == pRPString)
								{
									int TotalAccountsInSync = 1 + AllDuplicateAccounts.Count;

									if (pShowRefreshPositions && RefreshPositionsClickedTotal != 0 && RefreshPositionsClickedTotal != TotalAccountsInSync)
										{
											//Print("RefreshPositionsClickedTotal" + RefreshPositionsClickedTotal);
											//Print("TotalAccountsInSync" + TotalAccountsInSync);
										}	
										else
										{
											continue;
										}
								}

								//if (!pIsBuildMode && pLockAccountsOnLock && xxx == "Restore")
								
								if (!pIsBuildMode && xxx == "Restore")
									continue;					
								
								if (AllHideAccounts.Count == 0 && xxx == "Restore")
									continue;
									
								if (!HasAPEXAccounts && xxx == APEXButton)
									continue;
									
								
							
								//Print(xxx);
								
								if (xxx == "Main")
								{
									xxx = "ON";
									if (!pCopierIsEnabled)
										xxx = "OFF";
									
								
								}
								
								if (xxx == "Accounts")
								{
								
												
									if (pShowAccounts == "All")
									{
										xxx = "All Accounts";
									}
									else if (pShowAccounts == "Live")
									{
										xxx = "Live Accounts";
									}					
									else if (pShowAccounts == "Sim")
									{
										xxx = "Sim Accounts";
									}				
										
									
									
								}

								if (xxx == "All Instruments")
								{
									if (!pAllInstruments)
									{
										string thischarti = Instrument.FullName;
											
										thischarti = GetTheInstrument(thischarti, "Mini", false).FullName;	
										
			//								if (!thischarti.Contains("YM") && !thischarti.Contains("MYM"))
			//									thischarti = Instrument.FullName.Replace("M", "");
											
			//								if (thischarti == "MYM")
			//									thischarti = "YM";								
											
			//								if (thischarti == "2K")
			//									thischarti = "RTY";
											
			//								if (thischarti == "FDXS")
			//									thischarti = "FDXM";
											
											xxx = thischarti;
									}
								}
			                    string sd = xxx;

			                    // szvv = graphics.MeasureString(sd, ChartControl.Font);
			
			                    // int widdd = (int)szvv.Width + 8;
				
			         
			
			                    float widdd = 50;
			                    widdd = Math.Max(pButtonSize, widdd);
			
			                    if (thisbutton.Value.Width == 1)
			                        widdd = pButtonSize;
			                    else
			                        widdd = thisbutton.Value.Width;
			
			//                    SharpDX.DirectWrite.TextFormat ButtonTextFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Normal,
			//                    SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 11.0F);
			
			//                    ButtonTextFormat = myProperties.LabelFont.ToDirectWriteTextFormat();
			
								
								//SharpDX.DirectWrite.TextFormat ButtonTextFormat = TextFont4.ToDirectWriteTextFormat();	
								 
								
								var ButtonTextFormat = new SimpleFont(TextFont3.Family.ToString(), TextFont3.Size+pFontSizeTopRow).ToDirectWriteTextFormat();					
			                    ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
			                    ButtonTextFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
			                    ButtonTextFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
			
								string sizetx = xxx;
								
								if (ButtonName == "Main")
									sizetx = "OFF";
								if (ButtonName == "Accounts")
									sizetx = "Live Accounts";

								if (xxx == "Orders" || xxx == "Executions")
									sizetx = "Executions";
								
								
								if (thisbutton.Value.Text == "All Instruments")
									sizetx = "All Instruments";
						
								
								if (IsKorean)
								{
									//#region ---
									if (sizetx == "Executions")
									{										
										sizetx = sizetx.Replace("Executions", "체결모드");
										
										xxx = xxx.Replace("Executions", "체결모드");
										xxx = xxx.Replace("Orders", "주문모드");
									}
									
									else if (sizetx == "All Instruments")
									{										
										sizetx = sizetx.Replace("All Instruments", "l Instruments");
										
										xxx = xxx.Replace("All Instruments", "모든종목");
									}									
									else if (sizetx == "OFF")
									{
										
											
										if (xxx == "Main")
										{
											xxx = "ON";
											if (!pCopierIsEnabled)
												xxx = "OFF";
											
										}									
										
									}
									else
									{
										
										sizetx = sizetx.Replace("Lock", "잠금");
										sizetx = sizetx.Replace("Reset", "초기화");
										sizetx = sizetx.Replace("Restore", "복원");										
										sizetx = sizetx.Replace(pFlattenButtonString, "모두 청산");
										
										xxx = sizetx;
									}
									//#endregion
								}								

								var textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, sizetx, ButtonTextFormat, 10000, 10000);

								float FinalH = textLayout1.Metrics.Height + (float) Math.Round(6*dpiY/100, 0);
			                    FinalH = Math.Max(pButtonSize, FinalH) + pFontSizeTopRow/2;
			                    float FinalW = 0;
			                    if (thisbutton.Value.Width == 1)
			                        FinalW = FinalH;
			                    else
			                        FinalW = textLayout1.Metrics.Width + (float) Math.Round(8*dpiY/100, 0) + 4;

								if (pFontSizeTopRow != 0)
								{
									FinalH = FinalH + pFontSizeTopRow/2;
									FinalW = FinalW + pFontSizeTopRow/0.7f;
								}
								
								
								
								ButtonsHeight = (int) FinalH;
								
								
								FinalW = Math.Max(FinalH, FinalW);
								
								
			//					FinalW = (int) Math.Round(FinalW*dpiY/100, 0);
			//					FinalH = (int) Math.Round(FinalH*dpiY/100, 0);
			//					space = (int) Math.Round(space*dpiY/100, 0);
								
								
								
								//thistop = (float) ChartPanel.H - FinalH - 1; // move to bottom of chart
								
								//Print(thistop);
								
			                    if (xxx == "")
									FinalW = 15;
								
								float nextleft = CY;
								
								if (sizetx == APEXButton || sizetx == CopyButtonString || sizetx == RiskButtonString || sizetx == pFlattenButtonString || sizetx == "모두 청산" || sizetx == pRPString || sizetx == pHideAString || sizetx == pHideCString)
								{
									// hide apex button if out of room
									if (sizetx == APEXButton)
									{
										
										float totalofthree = 0;
										 
										if (pShowCopyRiskButtons)
										if (pIsCopyBasicFunctionsChecked && pIsRiskFunctionsChecked)
										{
											
											 textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, CopyButtonString, ButtonTextFormat, 10000, 10000);
											totalofthree = totalofthree + textLayout1.Metrics.Width + 8;
											 textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, RiskButtonString, ButtonTextFormat, 10000, 10000);
											totalofthree = totalofthree + textLayout1.Metrics.Width + 8;
											
										
											
											
										}
										
										
											textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, pFlattenButtonString, ButtonTextFormat, 10000, 10000);
											totalofthree = totalofthree + textLayout1.Metrics.Width + 8;		
																					
										
										if (pShowFlattenEverything)
										{
											textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, pFlattenButtonString, ButtonTextFormat, 10000, 10000);
											totalofthree = totalofthree + textLayout1.Metrics.Width + 8;		
											
										}
										
								
										
										if (pShowRefreshPositions && RefreshPositionsClickedTotal != 0 && RefreshPositionsClickedTotal != TotalDisplayAccounts)
										{
											textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, pRPString, ButtonTextFormat, 10000, 10000);
											totalofthree = totalofthree + textLayout1.Metrics.Width + 8;		
											
										}
										
										
										
										 textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, APEXButton, ButtonTextFormat, 10000, 10000);
										totalofthree = totalofthree + textLayout1.Metrics.Width + 8;							
											
			
					                 
										
										
										if (leftsidebuttonsrightx > rightoftable - totalofthree)
											continue;
												
										
										
										
										
									}
									
									
									CY = nextrightside - FinalW;
									nextleft = nextrightside - FinalW;
									
									nextrightside = nextleft;
									 nextrightside = nextrightside - space;
									
									
				
								}
								else
								{
									leftsidebuttonsrightx = nextleft + FinalW;
									
								}
								
//								if (textLayout1 != null)
//								textLayout1.Dispose();
								
								
			                    thisbutton.Value.Rect = new SharpDX.RectangleF(nextleft, thistop, FinalW, FinalH);
								
								CY = CY + FinalW;
			                    CY = CY + space;
			
			                    //CY = CY - widdd - space;
			                   // thisbutton.Value.Rect = new SharpDX.RectangleF(CY, space, widdd, pButtonSize);
			                   
			                    minx = Math.Min(minx, CY - space);
								miny = Math.Min(miny, thistop - space);
			
								
								//Print(xxx);
								if (xxx != "") // allow spacer
								{
									//if (InMenu)
									{
							
									bool switched = thisbutton.Value.Switch;
									if (ButtonName == "Main")
										switched = pCopierIsEnabled;
									if (ButtonName == "Lock")
										switched = !pIsBuildMode;
									if (ButtonName == CopyButtonString)
										switched = pIsCopyBasicFunctionsEnabled;
									if (ButtonName == RiskButtonString)
										switched = pIsRiskFunctionsEnabled;
									if (ButtonName == pFlattenButtonString)
									{
										if (DateTime.Now < FlattenEverythingClickedTime.AddMilliseconds(pMilliSFE))
											{
												switched = false;
											}
											else
											{
												switched = true;
											}
									}	
										
										// button fills
										if (ChartControl.Indicators.Count == 1 || ChartControl.Indicators.Count == addedtwice)
										{
						                    if (MouseIn(thisbutton.Value.Rect, 2, 2))
						                    {
												RenderTarget.FillRectangle(thisbutton.Value.Rect, br[switched ? thisbutton.Value.BrushOnHover : thisbutton.Value.BrushOffHover].DxBrush);
												RenderTarget.DrawRectangle(thisbutton.Value.Rect, br[thisbutton.Value.BrushOutlineHover].DxBrush, 3);
												string tbbb = thisbutton.Value.Name;
												if(tbbb.Length>0){
													var NNNNN = new SharpDX.RectangleF(thisbutton.Value.Rect.X,thisbutton.Value.Rect.Top-20,200,20);
													ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
													RenderTarget.DrawText(tbbb, ButtonTextFormat, NNNNN, br[ChartTextID].DxBrush);
												}
											}else{
												RenderTarget.FillRectangle(thisbutton.Value.Rect, br[switched? thisbutton.Value.BrushOn : thisbutton.Value.BrushOff].DxBrush);
												RenderTarget.DrawRectangle(thisbutton.Value.Rect, br[thisbutton.Value.BrushOutlineHover].DxBrush, 1);
											}

											ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
						                    RenderTarget.DrawText(xxx, ButtonTextFormat, thisbutton.Value.Rect, br[ChartTextID].DxBrush);
										}
									}
								}
			                }
							B2 = new SharpDX.RectangleF(minx, miny, 10000, 10000);
							minx = 999999;
							miny = 999999;	
						}
					}
					actualtablestart = pPixelsFromTop + ButtonsHeight + pPixelsFromTop;
				
					if (FirstLoop)
					{
						RectY = actualtablestart;
						
						FirstLoop = false;
					}
				
					bool ShowRects = true;
					
					int NumOfColumns = 0;
					int NumOfRows = 0;

					NumOfColumns = AllColumns.Count-1;
					NumOfRows = AllColumns[1].Count-1;

					rect2			= new SharpDX.RectangleF(RectX, RectY, RectWidth, RectHeight);
					
					float MaxX = 0;
					float MaxY = 0;
					float MinX = 99999999999;
					float MinY = 99999999999;
				
					SortedDictionary<int, int> ColumnNumberToWidth = new SortedDictionary<int, int>();
					SortedDictionary<int, string> ColumnNumberToName = new SortedDictionary<int, string>();
					
					int MaximumRows = (int) Math.Floor((ChartPanel.H - RectY - 40 - totaladjustment) /  (double) RectHeight);

					MaximumRows = Math.Max(MaximumRows, NotAccountRows + 2);

					MaximumAccountRows = MaximumRows - NotAccountRows;

					if (CurrentMasterAccount != null && MaximumRows == NotAccountRows + 2)
						MaximumAccountRows = MaximumRows - NotAccountRows - 1;
					
					int ThisAutoCloseCol = 0;
					int ThisAutoExitCol = 0;
						#region -- All columns --
						for (int j = 1; j <= NumOfColumns; j++)
						{
							int RectWidth111 = 0;
							for (int row = 1; row <= NumOfRows; row++)
							{
								#region -- All rows --
								// row is row number
								// j is column number
								text = AllColumns[j][row];
								if (row == 1)
								{
									ColumnNumberToName[j] = text;
								}
								// fix direction column width to longest text
								if (j == 1)
									text = "AA";
								
								if (text == "Cash Value" || text == "Net Liquidation")
								{
									text = "$1,000,000.00";
								}									
								else if (text == "Connected Status")
								{
									text = "AA";
								}					
								
								else if (text == "Auto Close")
								{
									ThisAutoCloseCol = j;
								}
								
								if (ThisAutoCloseCol != 0 && ThisAutoCloseCol == j)
								{
									text = "A";
								}

								if (text == "Auto Exit")
								{
									//DrawText = false;
									ThisAutoExitCol = j;
									//text = "";
								}
								
								if (ThisAutoExitCol != 0 && ThisAutoExitCol == j)
								{
									text = "A";
								}

								if (text == "Daily Goal")
								if (pDailyGoalDisplayMode == "Status")
								{
									text = "From Goal";
								}

								if (text == "Daily Loss")
								if (pDailyLossDisplayMode == "Status")
								{
									text = "From Loss";
								}

								#region -- is korean --
								if (IsKorean)
								{
									text = text.Replace("Account", "계좌");
									text = text.Replace("Pos", "포지션");
									text = text.Replace("Avg Price", "평단가");
									text = text.Replace("Unrealized", "미실현손익");
									if (text == "Realized")
										text = text.Replace("Realized", "실현손익");
									text = text.Replace("Gross Realized", "실현손익");
									text = text.Replace("Cash Value", "평가예탁금");
									text = text.Replace("Net Liquidation", "계좌잔고");
									text = text.Replace("From Funded", "계좌통과기준수익");
									text = text.Replace("Auto Liquidate", "계좌청산시점");
									text = text.Replace("From Closed", "MDD까지 금액");
									text = text.Replace("Daily Goal", "당일목표수익");
									text = text.Replace("Daily Loss", "당일손실제한");
									text = text.Replace("Qty", "수량");
									text = text.Replace("Commissions", "수수료");
									text = text.Replace("Total PNL", "총손익");
									
									text = text.Replace("Size", "사이즈");
									text = text.Replace("Type", "유형");
									text = text.Replace("Mode", "모드");
									text = text.Replace("Fade", "반대진입");
									
									text = text.Replace("Actions", "구분");
									
									text = text.Replace("From Goal", "목표수익도달금액");	
									text = text.Replace("From Loss", "손실제한도달금액");	
								}
								#endregion
								text = text.Replace("From", "Daily");	
								textLayout2 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, text, textFormat2, 100, textFormat2.FontSize);
								RectWidth111 = Math.Max(RectWidth111, (int) textLayout2.Metrics.Width + CellPaddingX);
								#endregion
							}
							
							ColumnNumberToWidth[j] = RectWidth111+1;
						}
						#endregion
					int CurrentAdjustment = 0;
					int TotalAccountsDisplayed = 0;
					pAnalyticsEnabled = true;

					br[TableBackgroundID].DxBrush.Opacity = pCompMinOpacity/100f;
							
					if (SortedList2.Count > 0)
					if (pAnalyticsEnabled)
					{
						AllButtonZ5.Clear();
						AllButtonZ6.Clear();
						AllButtonZ7.Clear();
						
						string ColumnTitle = string.Empty;

						bool IsTotalRow = false;
						string ThisTotalRow = string.Empty;
						
						bool IsMasterAccountRow = false;
						bool IsSlaveAccountRow = false;
						bool IsAnyAccountRow = false;						
						string ThisRowAccount = string.Empty;
						
						bool IsEvaluationAccount = false;		
						bool IsAccountFundedPlus = false;
						bool IsAccountFunded = false;
						bool IsAccountIsPropFirmPA = false;
						bool IsAccountNoTrades = false;
						bool IsAccountBlown = false;
						bool IsAccountDone = false;	
						bool IsAccountDisconnected = false;	
						bool IsAccountConnected = false;	
						bool IsAccountConnectionLost = false;	
						
						double CurrentDailyGoal = -1000000000;
						double CurrentDailyLoss = -1000000000;
								
						bool CurrentGoalComplete = false;
						bool CurrentLossComplete = false;										
						bool CurrentGoalCompleteBuffer = false;
						bool CurrentLossCompleteBuffer = false;											
							//Print("ThisR 11a");	

						int skippedrows = 0;
						
						//#region -- Print all rows ---
						for (int row = 1; row <= NumOfRows; row++)
						{
							
							IsMasterAccountRow = false;
							IsSlaveAccountRow = false;
							IsAnyAccountRow = false;
							ThisRowAccount = string.Empty;
							
							IsEvaluationAccount = false;	
							IsAccountFundedPlus = false;
							IsAccountFunded = false;	
							IsAccountIsPropFirmPA = false;
							IsAccountNoTrades = false;	
							IsAccountBlown = false;	
							IsAccountDone = false;	
							IsAccountDisconnected = false;
							IsAccountConnected = false;
							IsAccountConnectionLost	= false;

							CurrentDailyGoal = -1000000000;
							CurrentDailyLoss = -1000000000;
									
							CurrentGoalComplete = false;
							CurrentLossComplete = false;	
							CurrentGoalCompleteBuffer = false;
							CurrentLossCompleteBuffer = false;						
							
							
							int RectWidth2 = 0;
							//RectX = ChartPanel.W - TableMarginX;
							RectX = 0 + TableMarginX;
							 
							PNLStatistics ThisBOTChart = null;
							
								//Print("ThisR 11ab");
							
							if (row >= 3)
							{
								int thisnumber = row-3;
								
								if (thisnumber < SortedList2.Count)
								ThisBOTChart = SortedList2[thisnumber];
							}
							
								//Print("ThisR 11bb");
							
									//if (ColumnTitle == "–")
								if (ThisBOTChart != null)			
									if (ThisBOTChart.Acct != null)
									{
										
										
										//Print(AdjustmentRows);
										
										// skip rows based on scrolling
										
										
										
											ThisRowAccount = ThisBOTChart.Acct.Name;
											
										
										//	Print(ThisRowAccount);
										
										
											//Print(ThisBOTChart.Acct.DisplayName);
																			
	//Print("ThisR 11b");	
				

				
					bool IsAPEXPA = ThisRowAccount.Contains("PA-APEX") || ThisRowAccount.Contains("PAAPEX");
					bool IsAPEXEval = ThisRowAccount.Contains("APEX") && !IsAPEXPA;
					
					
					// "LE-LL"
					// "WK-LL"
					bool IsLeeLooPA = ThisRowAccount.Contains("PA-LL");
					bool IsLeeLooEval = (ThisRowAccount.Contains("LL") || ThisRowAccount.Contains("LB-LL") || ThisRowAccount.Contains("LE-LL") || ThisRowAccount.Contains("WK-LL")) && !IsLeeLooPA;

					bool IsBulenoxPA = ThisRowAccount.Contains("BX-M");
					bool IsBulenoxEval = ThisRowAccount.Contains("BX") && !IsBulenoxPA;
				
				
										// MFFUSF (Funded from a starter Jame Lynch) && MFFUEVEX (eval Yoania) added on July 31, 2024
														
										
					bool IsMFFPA = ThisRowAccount.Contains("MFFUSF") || ThisRowAccount.Contains("MFFUSFST") || ThisRowAccount.Contains("MFFULIVEST") || ThisRowAccount.Contains("MFFUSFEX");
					bool IsMFFEval = (ThisRowAccount.Contains("MFFUEV") || ThisRowAccount.Contains("MFFUEVST") || ThisRowAccount.Contains("MFFUEVEX"))&& !IsMFFPA;
														
					bool IsEliteTFPA = ThisRowAccount.Contains("ELITE") && ThisRowAccount.Contains("ETF");
					bool IsEliteTFEval = ThisRowAccount.Contains("ETF") && !IsEliteTFPA;
				
					// trifid88@gmail.com
					// EVALDHETF
					// EVALEODETF
										
		
					bool IsTakeProfitTraderPA = ThisRowAccount.Contains("TAKEPROFITPRO");						
					bool IsTakeProfitTraderEval = (ThisRowAccount.Contains("TAKEPROFIT") || ThisRowAccount.Contains("TPT")) && !IsTakeProfitTraderPA;
					
					bool IsTickTickPA = ThisRowAccount.Contains("TTTD");
					bool IsTickTickEval = ThisRowAccount.Contains("TTT") && !IsTickTickPA;
					
											// waiting on tick tick and take profit funded naming schemes
						
						
						
					bool IsFFNPA = ThisRowAccount.Contains("FFNX");
					bool IsFFNEval = ThisRowAccount.Contains("FFN")	&& !IsFFNPA;
					
					bool IsTradeDayPA = ThisRowAccount.Contains("ELTDE");
					IsTradeDayPA = false;
					
					bool IsTradeDayEval = ThisRowAccount.Contains("ELTDE") && !IsTradeDayPA;			
						
					bool IsTopStepEval = ThisRowAccount.Contains("S1Jan") || ThisRowAccount.Contains("S1Feb") || ThisRowAccount.Contains("S1Mar") || ThisRowAccount.Contains("S1Apr") || ThisRowAccount.Contains("S1May") || ThisRowAccount.Contains("S1Jun") || 
					ThisRowAccount.Contains("S1Jul") || ThisRowAccount.Contains("S1Aug") || ThisRowAccount.Contains("S1Sep") || ThisRowAccount.Contains("S1Oct") || ThisRowAccount.Contains("S1Nov") || ThisRowAccount.Contains("S1Dec");
												
											
							
						// Here is what BluSky prints on NT8
		
						// BLUDERVGVPE0
						
						// Purdia is PUR11462
						

					bool IsBLUEval = ThisRowAccount.Contains("BLU");
					bool IsBLUPA = false;
					bool IsPUREval = ThisRowAccount.Contains("PUR");
					bool IsPURPA = false;
				
										
										
					bool IsTradeifyGEval = ThisRowAccount.Contains("TDYG");
					bool IsTradeifyAEval = ThisRowAccount.Contains("TDYA");
					bool IsTradeifyEval = IsTradeifyGEval || IsTradeifyAEval;

					bool IsTradeifyPA = ThisRowAccount.Contains("FTDYSF") || ThisRowAccount.Contains("FTDYFA") || ThisRowAccount.Contains("FTD");
				
				
				
					bool IsPropFirmEval = IsAPEXEval || IsLeeLooEval || IsBulenoxEval || IsMFFEval || IsEliteTFEval || IsFFNEval || IsTradeDayEval || IsTopStepEval || IsTakeProfitTraderEval || IsTickTickEval || IsBLUEval || IsPUREval || IsTradeifyEval;
				
					bool IsPropFirmPA = IsAPEXPA || IsLeeLooPA || IsBulenoxPA || IsMFFPA || IsEliteTFPA || IsFFNPA || IsTradeDayPA || IsTakeProfitTraderPA || IsTickTickPA || IsBLUPA || IsPURPA || IsTradeifyPA;

										//ben if (ThisRowAccount.Contains("APEX"))
												HasAPEXAccounts = true;	
											
											if (IsLeeLooPA || IsLeeLooEval)
												HasLeeLooAccounts = true;
																
											 
											
											
											IsEvaluationAccount = IsPropFirmEval;
										
											IsAccountIsPropFirmPA = IsPropFirmPA;	
											
											
											
											if (pIsRiskFunctionsEnabled)
											{
												
												
												if (IsEvaluationAccount)
												if (ThisBOTChart != null)
												{
													if (ThisBOTChart.FromFunded <= 0)
														IsAccountFundedPlus = true;
													
													if (ThisBOTChart.FromFunded - pDollarsExceedFunded <= 0)
														IsAccountFunded = true;
													
													if (ThisBOTChart.FromBlown <= 100)
														IsAccountBlown = true;
													
													

														
												}
											
												if (IsAccountIsPropFirmPA && pPAFundedAmouont != 0)
												{
													
													if (ThisBOTChart.FromFunded <= 0)
														IsAccountFundedPlus = true;
													
													if (ThisBOTChart.FromFunded - pDollarsExceedFunded <= 0)
														IsAccountFunded = true;
													
													if (ThisBOTChart.FromBlown <= 100)
														IsAccountBlown = true;													
													
													
												}
												
											
												if (pColumnDailyGoal)
												{
												
													CurrentDailyGoal = GetAccountSavedData(ThisRowAccount, pAllAccountDailyGoal);			
													CurrentGoalComplete = ThisBOTChart.PNLTotal > CurrentDailyGoal && CurrentDailyGoal != -1000000000;
													CurrentGoalCompleteBuffer = ThisBOTChart.HitGoalOrLoss == 2 || ThisBOTChart.HitGoalOrLoss == 23;
												}
												if (pColumnDailyLoss)
												{
												
													CurrentDailyLoss = GetAccountSavedData(ThisRowAccount, pAllAccountDailyLoss);			
													CurrentLossComplete = ThisBOTChart.PNLTotal < CurrentDailyLoss && CurrentDailyLoss != -1000000000;
													CurrentLossCompleteBuffer = ThisBOTChart.HitGoalOrLoss == 3 || ThisBOTChart.HitGoalOrLoss == 23;
												}															
										
											}
											
											
												
											if (ThisBOTChart.TotalBought == 0)
												IsAccountNoTrades = true;
												
											IsAccountDone = IsAccountFundedPlus || IsAccountBlown;
						
											IsAccountDisconnected = ThisBOTChart.Acct.ConnectionStatus == ConnectionStatus.Disconnected;		
										//	IsAccountDisconnected = !AllConnectedAccounts.Contains(ThisBOTChart.Acct.Name);	
											
											
											
											IsAccountConnected = AllConnectedAccounts.Contains(ThisBOTChart.Acct.Name);	
										
											IsAccountConnectionLost	= ThisBOTChart.Acct.ConnectionStatus == ConnectionStatus.ConnectionLost;
											
											
											if (ThisRowAccount != string.Empty)
												IsAnyAccountRow = true;
											
											if (ThisRowAccount == pThisMasterAccount)
												IsMasterAccountRow = true;
											

											
										
//											if (IsMasterAccountRow)
//											{
//												Print(ThisRowAccount);
//											}
											
											
											
					//Print("ThisR 12");	
//											if (IsMasterAccountRow && !NoAccountsConnected && ThisBOTChart.Acct.ConnectionStatus == ConnectionStatus.Disconnected)
//											{
												
//												//DisableMasterAccount();
//											}
											
											
											if (AllDuplicateAccounts.Contains(ThisRowAccount))
												IsSlaveAccountRow = true;
											
											
											
											
											
											bool putattop = !IsMasterAccountRow;
										
											if (pShowFollowersAtTop)
												putattop = !IsMasterAccountRow && !IsSlaveAccountRow;
									
										
											if (IsAnyAccountRow)
											//if (ThisBOTChart.Signals == "") // is an account row
											if (putattop) // is not the master account
											{
												
												
												
												
												if (CurrentAdjustment < AdjustmentRows)
												{
													CurrentAdjustment = CurrentAdjustment + 1;
													continue;
													
													
												}
												
												
												
												TotalAccountsDisplayed = TotalAccountsDisplayed + 1;
												
												if (TotalAccountsDisplayed > MaximumAccountRows)
													continue;
												
												
											}
									}

							// j is column number   
							#region -- Print all columns in this row --
							for (int j = 1; j <= NumOfColumns; j++)
							{
								RectX = RectX + RectWidth2;
								int ttt = NumOfColumns-j + 1;
								ttt = j;
								RectWidth2 = ColumnNumberToWidth[j];
								ColumnTitle = ColumnNumberToName[j];

								//Print(ColumnTitle + "    " + row + " 1");
								//RectX = RectX - RectWidth2;
								//Print("ThisR 11.1");
								text = AllColumns[j][row];

								if (ColumnTitle == "Account") // first and second columns		
								{
									if (text == Total1Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow1"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow1"];
									
									
									if (text == Total2Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow2"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow2"];
										
									if (text == Total3Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow3"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow3"];
										
									if (text == Total4Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow4"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow4"];
										
										
									if (text == Total5Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow5"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow5"];
										
									if (text == Total6Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow6"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow6"];
										
									if (text == Total7Name)
										if (TotalAccountPNLStatistics.ContainsKey("TotalRow7"))
										ThisBOTChart = TotalAccountPNLStatistics["TotalRow7"];										

								}
								 //
								
								if (ThisBOTChart != null)
								{
									IsTotalRow = ThisBOTChart.Signals != "";
									ThisTotalRow = ThisBOTChart.Signals;
								}

								//Print(ColumnTitle + "    " + row + " 2");
								
								if (row == 1)
								{
									ColumnTitle = text;

									if (text == "Daily Goal")
									if (pDailyGoalDisplayMode == "Status")
									{
										text = "From Goal";
									}
									if (text == "Daily Loss")
									if (pDailyLossDisplayMode == "Status")
									{
										text = "From Loss";
									}		
									
//									Print(row + "  " + text);
									
									if (IsKorean)
									{
										#region ---
										text = text.Replace("Account", "계좌");
										text = text.Replace("Pos", "포지션");
										text = text.Replace("Avg Price", "평단가");
										text = text.Replace("Unrealized", "미실현손익");
										if (text == "Realized")
											text = text.Replace("Realized", "실현손익");
										text = text.Replace("Gross Realized", "실현손익");
										text = text.Replace("Cash Value", "평가예탁금");
										text = text.Replace("Net Liquidation", "계좌잔고");
										text = text.Replace("From Funded", "계좌통과기준수익");
										text = text.Replace("Auto Liquidate", "계좌청산시점");
										text = text.Replace("From Closed", "MDD까지 금액");
										text = text.Replace("Daily Goal", "당일목표수익");
										text = text.Replace("Daily Loss", "당일손실제한");
										text = text.Replace("Qty", "수량");
										text = text.Replace("Commissions", "수수료");
										text = text.Replace("Total PNL", "총손익");
										
										text = text.Replace("Size", "사이즈");
										text = text.Replace("Type", "유형");
										text = text.Replace("Mode", "모드");
										text = text.Replace("Fade", "반대진입");
										
										text = text.Replace("Actions", "구분");
										
										text = text.Replace("From Goal", "목표수익도달금액");	
										text = text.Replace("From Loss", "손실제한도달금액");	
										#endregion
									}
								}
								//Print("ThisR 11.2");
								//bool DrawCell = true;
								bool DrawText = true;	
								bool CellIsButton = false;
								bool CellIsButtonBorder = false;
//								bool HighlightThisCell = true;
								//if (text != "")
								{
									textID = ChartTextID;
									rect2 = new SharpDX.RectangleF(RectX, RectY, RectWidth2, RectHeight);
									SharpDX.RectangleF TextRect = rect2;
									SharpDX.RectangleF ClickRect = ExpandRect(rect2,0,-1,0,-1);

									if (pHighlightHoverCells)
									if (j == 1)
									{
										AddButtonZ6(AllButtonZ6,ThisBOTChart,ExpandRect(ClickRect,2000,2000,0,0), "Hover", row);	
									}

									bool HighlightThisRow = false;
									if (pHighlightHoverCells)
									{
										//Print(HoveredRow);
										HighlightThisRow = HoveredRow == row;
										if (row == 1)
											HighlightThisRow = true;
									}
									
										//Print("ThisR 11.31");	
									
									//Print(HoveredRow);
									
									textFormat2 = TextFont3.ToDirectWriteTextFormat();	

									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading; // leading = left.
						            textFormat2.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center; // far = bottom.
						            textFormat2.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
									
								
									
									rect2 = new SharpDX.RectangleF(RectX, RectY, RectWidth2, RectHeight);
									
							
									TextRect = MoveRect(TextRect,1*CellPaddingX/2, 0);
									TextRect = ResizeRect (TextRect, -1*CellPaddingX, 0);
									
									MaxX = Math.Max(MaxX, rect2.X + rect2.Width);
									MaxY = Math.Max(MaxY, rect2.Y + rect2.Height);
									MinX = Math.Min(MinX, rect2.X);
									MinY = Math.Min(MinY, rect2.Y);	

									//if (ShowRects) RenderTarget.DrawRectangle(rect2, customDXBrush, 1);		
							
										//Print("ThisR 11.32");	
									 
																	
									TableFinalBackgroundID = TableBackgroundID;
									br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacity/100f;
									
									//Print("ThisR 11.33");	
									
									
									if (HighlightThisRow)
									if (!IsMasterAccountRow && !IsSlaveAccountRow)
									{
										br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacity+pHighlightAmount)/100f;

									}
		
									bool IsFillColorCell = false;
									
	
									//Print(ColumnTitle + "    " + row + " 3");
									
									
								//		bool NoExtraAccounts = TotalDisplayAccounts <= MaximumAccountRows;
										
									
								//Print("ThisR 11.3");	
									
								if (row == 1) // first row only
								{
									
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.

									//if (ColumnTitle != "Fade" && ColumnTitle != "x" && ColumnTitle != "Auto Close" && ColumnTitle != "Auto Exit" && ColumnTitle != "X" && ColumnTitle != "Actions" && ColumnTitle != "–" && ColumnTitle != "Connected Status")
									if (ColumnTitle != "Fade" && ColumnTitle != sizecolname && ColumnTitle != modecoln && ColumnTitle != "Type" && ColumnTitle != "Auto Close" && ColumnTitle != "Auto Exit" && ColumnTitle != "X" && ColumnTitle != "Actions" && ColumnTitle != "–" && ColumnTitle != "Connected Status")
									{
										AddButtonZ5(AllButtonZ7,ThisBOTChart,ClickRect, ColumnTitle);	
										CellIsButton = true;
									}
								}
								//Print("ThisR 11.4");
								if (row == 2) // second row
								{

									text = "";

									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.

									if (ColumnTitle == "Fade" || ColumnTitle == sizecolname || ColumnTitle == modecoln || ColumnTitle == "Type" || ColumnTitle == "Daily Goal" || ColumnTitle == "Daily Loss" || ColumnTitle == "Payout" || ColumnTitle == "Account" || ColumnTitle == "Auto Close" || ColumnTitle == "Auto Exit")
									{
										AddButtonZ5(AllButtonZ7,ThisBOTChart,ClickRect, "Reset " + ColumnTitle);	
										CellIsButton = true;
									}

									if (ColumnTitle == SelectedResetColumn)
									{
										
										text = "\u2713";
										br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityH/100f;		
											
										if (HighlightThisRow)
										{
											br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacityH+pHighlightAmount)/100f;
										}											
										
									}
								}
	
							if (ColumnTitle == "Instrument") // first and second columns
							{
							}

							if (IsAnyAccountRow && !IsMasterAccountRow)
							{
								if (ColumnTitle == sizecolname) // first and second columns
								{
	
									//Print(sizecolname);
									//
									
									AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, sizecolname);	
									CellIsButton = true;
								}			
											
								
								if (ColumnTitle == modecoln) // first and second columns
								{
				
									AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Mode");	
									CellIsButton = true;
								}		
								
								
								if (ColumnTitle == "Fade") // first and second columns
								{
				
									AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Fade");	
									CellIsButton = true;
								}		
								
									
								
								if (ColumnTitle == "Type") // first and second columns
								{
									
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
			
									
									
									
//										if (!AllCrossAccounts.Contains(ThisRowAccount))
//											br[textID].DxBrush.Opacity = fasttextop;

									
									
//									if (text == pCrossType)
//										br[textID].DxBrush.Opacity = fasttextop;
										
							
										
										AddButtonZ5(AllButtonZ5, ThisBOTChart, ClickRect, "Type");	
										CellIsButton = true;
								
									
								}							
										
								
							}

							//Print("ThisR 11.5");
								
								if (pIsCopyBasicFunctionsEnabled)
								{
									if (IsMasterAccountRow)
									{
										TableFinalBackgroundID = pBackMasterAccountColorID;
										br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityHAcc/100f;
									}
									else if (IsSlaveAccountRow)
									{
										TableFinalBackgroundID = pBackSlaveAccountColorID;
										br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityHAcc/100f;
									}
								}
								if (HighlightThisRow)
								{
									br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacityHAcc+pHighlightAmount)/100f;
								}

							if (ColumnTitle == "Auto Exit") // first and second columns
								if (row == 1)
									DrawText = false;

							if (IsAnyAccountRow)
							{
								if (ColumnTitle == "Daily Goal") // first and second columns
								{
									if (CurrentDailyGoal == -1000000000)
									{
										DrawText = false;
									}
									else
									{
										if (CurrentGoalCompleteBuffer)
										{
											TableFinalBackgroundID = pIsFundedColorID;
											br[TableFinalBackgroundID].DxBrush.Opacity = 20/100f;
										}
									}
								
									
									if (pDailyGoalDisplayMode == "Status")
									{
										if (DrawText)
										{
											AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Daily Goal");	
											CellIsButton = true;		
										}
									}
									else
									{
										AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Daily Goal");	
										CellIsButton = true;													
//										AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Daily Goal");	
//										CellIsButton = true;
										
										if (DrawText)
										{
										
											if (SelectedDailyGoalAccount == ThisRowAccount)
											{
												br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityH/100f;		
												
													
												if (HighlightThisRow)
												{
													br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacityH+pHighlightAmount)/100f;	
													
												}		
											
											}
											
										}
										
										
									}
								}			
												
								if (ColumnTitle == "Daily Loss") // first and second columns
								{
									
									
									if (CurrentDailyLoss == -1000000000)
									{
										DrawText = false;
									}
									else
									{
									
										if (CurrentLossCompleteBuffer)
										{
										
											TableFinalBackgroundID = RED20;
											
											
										}
											
									}
									
									
							
									
//									AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Daily Loss");	
//									CellIsButton = true;								
									
									if (pDailyLossDisplayMode == "Status")
									{
										if (DrawText)
										{

											AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Daily Loss");	
											CellIsButton = true;		
										}
																				
										
										
									}
									else
									{
												
										AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Daily Loss");	
										CellIsButton = true;
										
										if (DrawText)
										{
										
											if (SelectedDailyLossAccount == ThisRowAccount)
											{
											
												
												br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityH/100f;		
												
													
												if (HighlightThisRow)
												{
													br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacityH+pHighlightAmount)/100f;
													
												}		
											
											}
											
										}
									}
								}		
								
								if (ColumnTitle == "Auto Exit") // first and second columns
								{
									
									//if (row == 1)
									
									
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
									//Print(ThisRowAccount);
										if (text == "No" && (CurrentGoalComplete || CurrentLossComplete))
										{
											
											DrawText = false;
//											Print(ThisRowAccount + " 1 ");
//											Print(CurrentLossComplete);

										}
										else if (IsDailyGoalOrLossConfigured(ThisRowAccount))
										//if (IsDailyGoalOrLossConfigured(ThisRowAccount) || text == "Yes")
										{
											    AddButtonZ5(AllButtonZ5, ThisBOTChart, ClickRect, "Auto Exit");	
												CellIsButton = true;
												if (text == "No")
												{
													
													br[textID].DxBrush.Opacity = fasttextop;
													
												}
												text = "\u2713";
										}
										else
										{
											DrawText = false;
										}
								}	

								if (ColumnTitle == "Payout") // first and second columns
								{
									
									if (GetAccountSavedData(ThisRowAccount, pAllAccountPayouts) == -1000000000)
										DrawText = false;
												
										AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Payout");	
										CellIsButton = true;
										
										if (DrawText)
										{
									
											if (SelectedPayoutAccount == ThisRowAccount)
											{
											
												
												br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityH/100f;		
												
													
												if (HighlightThisRow)
												{
													br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacityH+pHighlightAmount)/100f;	
													
												}		
											
											}
											
										}
								}										
							}
							// copier - create buttons and highlights for setting master and slave accounts

							if (ColumnTitle == "Account") // first and second columns
							{
								if (pIsCopyBasicFunctionsEnabled)
								if (pIsBuildMode || !pLockAccountsOnLock)
								if (pThisMasterAccount != ThisRowAccount)
								if (IsAnyAccountRow)
								{
									AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Account");	
									CellIsButton = true;
								}
									
							}

							if (ColumnTitle == "–") 
							{
								textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.

								if (IsAnyAccountRow)
								{
									bool NotConnectedAccount = !NoAccountsConnected && IsAccountDisconnected;
					
									if (NotConnectedAccount)
										NoAccountsDisconnected = false;
										
									
									if (text == "–")
										TextRect = MoveRect(TextRect, 0, -1);
									
									//if (pIsCopyBasicFunctionsEnabled && !NotConnectedAccount && (pIsBuildMode || !pLockAccountsOnLock))
									if (pIsCopyBasicFunctionsEnabled && (pIsBuildMode || !pLockAccountsOnLock))
									{

																				
																	
//													if (text == "–")
//														TextRect = MoveRect(TextRect, 0, -1);
											
											AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "–");	
											CellIsButton = true;
										
									}
									else
									{
										
								
											DrawText = false;
										
										
									}
									
									
									if (pShowConnectedStatus)
									if (IsAnyAccountRow)
									{
									
										
										
										if (IsAccountDisconnected)
										{
											//DrawText = true;
											
											 
											
											text = "DIS";
											
//												if (!NoAccountsConnected)
//												{
//													AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Disconnected");	
//													CellIsButton = true;
//												}
											
										}
										
										
										if (IsAccountConnected)
											text = "CON";
										
										if (IsAccountConnectionLost)
											text = "COLOST";
									}										
										
									
									
//											if (!pShowConnectedStatus)
//											if (NotConnectedAccount)
//											{
//												//DrawText = true;
										
										
										
//												text = "DIS";
										
//												AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Disconnected");	
//												CellIsButton = true;
//											}
								}
								
								
							}

								if (ColumnTitle == "Connected Status") 
									{
								
										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.

										
										
										if (IsAnyAccountRow)
										{
										
											bool NotConnectedAccount = IsAccountDisconnected;
							

											
											if (NotConnectedAccount)
											{
												//DrawText = true;
												
												
												
												text = "DIS";
												
//												if (!NoAccountsConnected)
//												{
//													AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Disconnected");	
//													CellIsButton = true;
//												}
												
											}
											
											
											if (IsAccountConnected)
												text = "CON";
											
											if (IsAccountConnectionLost)
												text = "COLOST";
												
										}
										else
										{
											
											DrawText = false;
										}
										
										
									}								

									//Print(ColumnTitle + "    " + row + " 5");

								if (ColumnTitle == "Avg Price") // first and second columns
								{
									
									
										
												if (text == "Multiple")
												{
													
													br[textID].DxBrush.Opacity = fasttextop;
													
												}
									
									
								}
								if (ColumnTitle == "Actions") // first and second columns
									{
										
										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
										//DrawText = false;
										AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "TestAction");	

										if (MarketDataOK)
										{
											if (IsTotalRow)
											{
												
												if (pIsRiskFunctionsChecked && pOneTradeAll)
												if (pOneTradeShow != "Accounts")
												{
												
													if (ThisBOTChart != null)
													if (ThisBOTChart.FrozenOrders != 0)
													{
														
															text = "Reset [" + ThisBOTChart.FrozenOrders.ToString() + "]";
																					
															AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Ghost Orders");	
															CellIsButton = true;												
															CellIsButtonBorder = true;
														
															br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacity+pHighlightAmount)/100f;	
														
													}
													
													
													if (pIsRiskFunctionsChecked && pOneTradeAll)
													if (ThisBOTChart != null)
													if (ThisBOTChart.OneTradeReady != 0)
													{
															text = "One Tr [" + ThisBOTChart.OneTradeReady.ToString() + "]";
																					
															AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "One Trade");	
															CellIsButton = true;												
															CellIsButtonBorder = true;
															br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacity+pHighlightAmount)/100f;	
													}
												}
											}
											else
											{
												
				
												//Print(ThisTotalRow); 
												
												//if (!IsAccountDisconnected)
												
												//if (pIsRiskFunctionsChecked && pOneTradeAll)
												//if (pOneTradeShow != "Accounts")
												{
													if (ThisBOTChart != null)
													if (ThisBOTChart.FrozenOrders != 0)
													{
														
															text = "Reset [" + ThisBOTChart.FrozenOrders.ToString() + "]";
																					
															AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Ghost Orders");	
															CellIsButton = true;												
															CellIsButtonBorder = true;
														
															br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacity+pHighlightAmount)/100f;	
														
													}
												}
												
												
												AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Frozen");	
												
												
												
												
												if (pIsRiskFunctionsChecked && pOneTradeAll)
												if (pOneTradeShow != "Totals")
												{												
													if (ThisBOTChart != null)
													if (ThisBOTChart.OneTradeReady == 1)
													{
																	text = "One Tr";
																						
																AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "One Trade");	
																CellIsButton = true;												
																CellIsButtonBorder = true;
																
																br[TableFinalBackgroundID].DxBrush.Opacity = (pCompMinOpacity+pHighlightAmount)/100f;													
														
													}
												}
												
												
												
												
												
												
//												Print(ThisBOTChart.PrivateName);
//												Print(RejectedAccounts.Count);
												if (ThisBOTChart != null)
												if (RejectedAccounts.ContainsKey(ThisBOTChart.PrivateName))
												{
													
//													Print(ThisBOTChart.PrivateName);
													
													if (RejectedAccounts[ThisBOTChart.PrivateName].StartTime > DateTime.Now.AddSeconds(-10))
													{
														text = RejectedAccounts[ThisBOTChart.PrivateName].LatestAction;
														br[textID].DxBrush.Opacity = fasttextop2;
													}
													else
													{
														RejectedAccounts.Remove(ThisBOTChart.PrivateName);
													}
													
												}
//											
												
											}
										}
									}

								if (ColumnTitle == "Fade") // account column
								{
									
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
									
									
									if (IsMasterAccountRow)
									{
										DrawText = false;
										
									}
									
										if (text == "No")
											{
												
												br[textID].DxBrush.Opacity = fasttextop;
												
											}
								}	

								if (ColumnTitle == modecoln) // account column
								{
									if (IsAnyAccountRow)
									if (SelectedATMAccount == ThisRowAccount)
									{
										br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityH/100f;		
									}
									if (text == "Default")
									{
										br[textID].DxBrush.Opacity = fasttextop;
									}
//									//if (!text.Contains("x"))
//									{
//										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.				
//									}
								}

								if (ColumnTitle == sizecolname) // account column
								{
								
											if (IsAnyAccountRow)
											if (SelectedMultiplierAccount == ThisRowAccount)
											{
											
												
												br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacityH/100f;		
												
												
											
											}
											
											if (text == "1x" || text == "1")
											{
												
												br[textID].DxBrush.Opacity = fasttextop;
												
											}
											
											
											//if (!text.Contains("x"))
											{
												textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.				
										
								
											}
								}

								//Print(ColumnTitle + "    " + row + " 6");
								
								if (ColumnTitle == "Qty") // trend column
								{
										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.

								}

								if (ColumnTitle == "Trend") // trend column
								{
									
									if (ThisBOTChart != null)
									{
										
										TableFinalBackgroundID = pBackBuyColorID;
										br[TableFinalBackgroundID].DxBrush.Opacity = pCompMinOpacity/100f;
									}
									

								}

								// add close buttons
								if (ColumnTitle == "X") // trend column
								{
									
									//DrawCell = false;
									
									
									
									
									DrawText = false;
									
									if (ThisBOTChart != null)			
									//if (ThisBOTChart.Acct != null)
									{
		

										
										//pShowHideButton
										//if (pCloseButtonEnabled)
										//if (!pIsBuildMode)
										//if (pThisMasterAccount == rowaccount)
										
										bool ShowCloseButton = pCloseButtonEnabled && (ThisBOTChart.PositionLong != 0 || ThisBOTChart.PositionShort != 0);
										bool ShowPendingOrdersCloseButton = pCancelButtonEnabled && ThisBOTChart.PendingEntry != 0;
										
										if (ShowPendingOrdersCloseButton)
										{
											
											textID = GOLDENROD;
											
											//IsFillColorCell = true;
											//TableFinalBackgroundBrushDX = pBackSellColor.ToDxBrush(RenderTarget);
											//TableFinalBackgroundBrushDX.Opacity = pCompPositionOpacity/100f;	
											
											DrawText = true;
											
											
											AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Pending");	
											CellIsButton = true;											
											
										}
										else if (ShowCloseButton)											
										{
											
											
											
											
											textID = RED;
											
											//IsFillColorCell = true;
											//TableFinalBackgroundBrushDX = pBackSellColor.ToDxBrush(RenderTarget);
											//TableFinalBackgroundBrushDX.Opacity = pCompPositionOpacity/100f;	
											
											DrawText = true;
											
											
											AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Close");	
											CellIsButton = true;
										}
										else
										{
											if (pShowHideButton)
											{
											
												if (pIsBuildMode || !pLockHideAccountsOnLock)
												//if (pThisMasterAccount != rowaccount && !AllDuplicateAccounts.Contains(rowaccount))
												
												if (ThisRowAccount != string.Empty)
												if (pThisMasterAccount != ThisRowAccount || !pIsCopyBasicFunctionsEnabled)
													
												{
													DrawText = true;
													
													AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Hide");	
													CellIsButton = true;
												}
											}
										}

				
									
									}
								}

								if (ColumnTitle == LongColumnName) // qty column
								{
									
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
									
									if (ThisBOTChart != null)
									{
										int pos = ThisBOTChart.PositionLong;
										
										if (pCombineLongShort)
										if (pos == 0)
											pos = ThisBOTChart.PositionShort*-1;
										
										text = pos.ToString().Replace("-", "");
										
										if (pos >= 1)
										{
//											IsFillColorCell = true;											
//											TableFinalBackgroundBrushDX = pBackSellColor.ToDxBrush(RenderTarget);
//											TableFinalBackgroundBrushDX.Opacity = pCompPositionOpacity/100f;	
											
											
											IsFillColorCell = true;
											TableFinalBackgroundID = pBackBuyColorID;
											br[TableFinalBackgroundID].DxBrush.Opacity = pCompPositionOpacity/100f;	
											
											
											//TableFinalBackgroundBrushDX.Color = new SharpDX.Color4(pBackBuyColor.R / 255f, pBackBuyColor.G / 255f, pBackBuyColor.B / 255f, 1f);
											
											var solidColor = ((SolidColorBrush)pBackBuyColor).Color;
											var dxColor = new SharpDX.Color4(solidColor.R / 255f, solidColor.G / 255f, solidColor.B / 255f, 1f);

											
													
											if (HighlightThisRow)
												br[TableFinalBackgroundID].DxBrush.Opacity = (pCompPositionOpacity+pHighlightAmount)/100f;	
											
										}
									
									
									
										else if (pos <= -1)
										{
											
											IsFillColorCell = true;											
											TableFinalBackgroundID = pBackSellColorID;
											br[TableFinalBackgroundID].DxBrush.Opacity = pCompPositionOpacity/100f;
											
											if (HighlightThisRow)
												br[TableFinalBackgroundID].DxBrush.Opacity = (pCompPositionOpacity+pHighlightAmount)/100f;												
										}	
										
										
										
										
										else
										{
											
											br[textID].DxBrush.Opacity = fasttextop;
											
											text = "–";
										}
										
									}
								}								
								if (ColumnTitle == "Short") // qty column
								{
									
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
									
									if (ThisBOTChart != null)
									{
										
										
										int pos = ThisBOTChart.PositionShort;
										
										
										if (pos >= 1)											
										{
											IsFillColorCell = true;											
											TableFinalBackgroundID = pBackSellColorID;
											br[TableFinalBackgroundID].DxBrush.Opacity = pCompPositionOpacity/100f;	
			
											
											if (HighlightThisRow)
												br[TableFinalBackgroundID].DxBrush.Opacity = (pCompPositionOpacity+pHighlightAmount)/100f;
											
										}
									
										
									
										else if (pos <= -1)										
										{
											
											IsFillColorCell = true;											
											TableFinalBackgroundID = pBackSellColorID;
											br[TableFinalBackgroundID].DxBrush.Opacity = pCompPositionOpacity/100f;	
											
											if (HighlightThisRow)
												br[TableFinalBackgroundID].DxBrush.Opacity = (pCompPositionOpacity+pHighlightAmount)/100f;	


										}	
										
										else
										{
											
											br[textID].DxBrush.Opacity = fasttextop;
											
											text = "0";
											
//											if (pShowATMQWhenFlat)
//											{
//												text = ThisBOTChart.ATMQ.ToString();
//											}
//											else
//											{
//												br[textID].DxBrush.Opacity = 0.0f;
//											}
											
											
										}
										
									}
									
	
									
								}
								if (ColumnTitle == "Commissions" || ColumnTitle == "Cash Value" || ColumnTitle == "Net Liquidation" || ColumnTitle == "Auto Liquidate" || ColumnTitle == "From Closed" || ColumnTitle == "From Funded"|| ColumnTitle == "Daily Goal"|| ColumnTitle == "Daily Loss") // unrealized column
								{
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing; // leading = left.
								}

								if (pIsRiskFunctionsEnabled)
								{
									if (ColumnTitle == "Auto Liquidate" || ColumnTitle == "From Closed")
									{
										//HighlightThisCell = false;
	//									if (ThisBOTChart != null)
	//									if (ThisBOTChart.Acct != null)
										//	Print(ThisBOTChart.Acct.Name);
										if (ThisBOTChart != null)	
										if (IsAnyAccountRow)
										{
											if (ThisBOTChart.FromBlown == 1000000)
											{
												if (ColumnTitle == "From Closed")
													DrawText = false;
												if (ColumnTitle == "Auto Liquidate" && ThisBOTChart.AudioLiq == 0)
													DrawText = false;											
											}
											else
											if (ThisBOTChart.AudioLiq != 0)
											{
												IsFillColorCell = true;	
												double PercentFromHigh = ThisBOTChart.FromBlown / ThisBOTChart.TrailingThre * 100;

												//Print(PercentFromHigh);
												if (pFillFundedCells)
												{
													TableFinalBackgroundID = pTrailingGoodColorID;
													
													if (PercentFromHigh < pPercentWarning1)
													{
														TableFinalBackgroundID = pTrailingWarningColorID;
													}
													if (PercentFromHigh < pPercentWarning2)
													{
														TableFinalBackgroundID = pTrailingBadColorID;
													}
													if (PercentFromHigh <= 0)
													{
														TableFinalBackgroundID = pTrailingBlownColorID;
													}											
													br[TableFinalBackgroundID].DxBrush.Opacity = 0.2f;
												}
												
													
												if (HighlightThisRow)
													br[TableFinalBackgroundID].DxBrush.Opacity = (20+pHighlightAmount)/100f;

												if (ColumnTitle == "Auto Liquidate" && !IsAccountFunded)
												{
													AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Auto Liquidate");	
													CellIsButton = true;
												}
											}
											else
											{
												DrawText = false;
											}
										}
									}
									if (ColumnTitle == "Auto Close") // first and second columns
									{
										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
										if (IsAnyAccountRow && (IsEvaluationAccount || pPAFundedAmouont != 0))
										{
											if (IsAccountBlown && text == "Yes")
											{
												SetAccountData("",ThisRowAccount,"","","","","","","No","");
											}
											
											else if (IsAccountDone && text == "No")
											{
															
												DrawText = false;
												//text = "";	
											}
											else if (text == "No" && ThisBOTChart.FromBlown == 1000000)
											{
												DrawText = false;
											}
											else
											{
												AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Auto Close");	
												CellIsButton = true;
	
												if (text != "Yes")
												{
													br[textID].DxBrush.Opacity = fasttextop;
												}
												text = "\u2713";
											}
										}
										else
										{
											if (row != 2)
												DrawText = false;
										}
									}	
									if (ColumnTitle == "Net Liquidation" || ColumnTitle == "Cash Value" || ColumnTitle == "From Funded" || ColumnTitle == "Auto Close")
									{
										if (ThisBOTChart != null)	
										if (IsAnyAccountRow)
										{
											if (ThisBOTChart.FundedAmount != 1000000)
											{
												bool isffff = ThisBOTChart.NetLiquidation > ThisBOTChart.FundedAmount;
												//isffff = true;
												isffff = IsAccountFunded;

												if (isffff)
												{
													IsFillColorCell = true;	
										
													TableFinalBackgroundID = pIsFundedColorID;
													br[TableFinalBackgroundID].DxBrush.Opacity = 0.2f;
													
													if (HighlightThisRow)
														br[TableFinalBackgroundID].DxBrush.Opacity = (20+pHighlightAmount)/100f;
												}
														
												if (ColumnTitle == "From Funded")
												{
													
													if (ThisBOTChart.PositionLong == 0 && ThisBOTChart.PositionShort == 0)
													{
														if (IsAccountFunded)
															text = "FUNDED";
													}
													else
													{
														
														if (IsAccountFundedPlus)
															text = "FUNDED";
													}
												}
											}
											else
											{
												if (ColumnTitle == "From Funded")
												{
													//text = "";
													DrawText = false;
												}
											}
										}
									}
								}

								if (ColumnTitle == "Total PNL" || ColumnTitle == "Unrealized" || ColumnTitle == "Realized" || ColumnTitle == "Gross Realized") // unrealized column
								{
									textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing; // leading = left.
									
									
									bool ignorecolor = false;
									
									if (pHideCurrencyIsEnabled)
										if (pCurrencyPrivacy == "Everything")
											ignorecolor = true;
										
										
										
									if (!ignorecolor)				
									if (ThisBOTChart != null)
									{
									
										double moneyv = 0;
										
										
										if (ColumnTitle == "Realized")
											moneyv = ThisBOTChart.PNLGR;
																				
										if (ColumnTitle == "Gross Realized")
											moneyv = ThisBOTChart.PNLR;
										
										
										if (ColumnTitle == "Total PNL")
											moneyv = ThisBOTChart.PNLTotal;
																				
										if (ColumnTitle == "Unrealized")
											moneyv = ThisBOTChart.PNLU;
										
										//Print(ColumnTitle + "   " + moneyv);
									
										if (moneyv > 0)											
										{
											textID = pPositivePnLTextColorID;
										}
										if (moneyv < 0)	
										{
											textID = pNegativePnLTextColorID;
										}	
									}
								}									
								
								bool BottomAccountIsVisible = AdjustmentRows + MaximumAccountRows >= TotalAccounts;
								
								if (pShowFollowersAtTop)
									BottomAccountIsVisible = AdjustmentRows + MaximumAccountRows >= TotalAccounts - AllDuplicateAccounts.Count;
								
								
								bool TopAccountIsVisible = AdjustmentRows == 0;
								
								bool doit = false;
								
								//if (doit)
									
								bool scrollingneed = TotalAccounts > MaximumAccountRows && (!TopAccountIsVisible || !BottomAccountIsVisible);
								bool topleftcella = scrollingneed && row == 1 && j == 1;	
								
								bool isftttt = row == 1 && j == 1 && pForceAllSelectedAccountsToTop;

									if (topleftcella)
									{
										text = ">";	
										
										DrawText = true;
	
										AddButtonZ5(AllButtonZ7,ThisBOTChart,ClickRect, "JUMP");	
										CellIsButton = true;

										double FinalTextSize1 = TextFont3.Size + 3;
										FinalFont1 = new SimpleFont(TextFont3.Family.ToString(), FinalTextSize1);	

										textFormat2 = FinalFont1.ToDirectWriteTextFormat();	
										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
										
										if (TotalAccounts > MaximumAccountRows)
										{
											text = "\u21F5";
											text = inmiddle;
											
										}
										if (TopAccountIsVisible)
										{
											text = topvisible;
										}
										
//										Print(AdjustmentRows + MaximumAccountRows);
//										Print("TotalAccounts " + TotalAccounts);
										
									
										if (BottomAccountIsVisible)
										{
											text = bottomvisible;
										}
									}
									else if (row == 1 && j == 1)
									{
										AddButtonZ5(AllButtonZ7,ThisBOTChart,ClickRect, "ForceToTop");
										CellIsButton = true;
									}
									bool selcol = pSelectedColumn == ColumnTitle && row == 1;
									if (selcol || topleftcella || isftttt)
										{
											TableHighlight1ID = pCompMainColorID;
											br[TableHighlight1ID].DxBrush.Opacity = (pCompMinOpacityH)/100f;	
											
											
											TableBackgroundID = TableHighlight1ID;
											//TableHighlight1BrushDX.Opacity = TableHighlight1BrushDX.Opacity + (pHighlightAmount)/100f;	
										}
									//if (DrawCell)
									{
										oldAntialiasMode = RenderTarget.AntialiasMode;
										RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;

										int sretcg = 6; // size of top and bottom horizontal line indicator
										
										sretcg = (int) Math.Round(sretcg*dpiY/100, 0);
										
										int asadfgh = 3 + AdjustmentRows;
										string rowaccount = "";
										
										int nusfsdg =  3 + TotalAccounts;
										int test = 3 + AdjustmentRows + MaximumAccountRows - 1;

										//Print("nusfsdg " + nusfsdg + "   test " + test);
										
										int add = 0;
										
//										Print(pThisMasterAccount);
										if (pThisMasterAccount != string.Empty){
											add = 1;
											if(row==3)
												FillRectBoth(ExpandRect(rect2,0,0,0,0), pBackMasterAccountColorID);
											else FillRectBoth(ExpandRect(rect2,1,1,1,0), TableBackgroundID);
										}
										else FillRectBoth(ExpandRect(rect2,1,1,1,0), TableBackgroundID);
//p($"{pThisMasterAccount}  opacity: {br[ChartBackgroundID].DxBrush.Opacity}   {br[ChartBackgroundID].MediaBrush}");

										if (HoveredRow != 2)
										if (!TopAccountIsVisible && row == 2)
										{
											FillRectBoth(ExpandRect(rect2,1,1,sretcg-rect2.Height+2,0), TableBackgroundID);
										}
										
										//Print("TotalAccounts " + TotalAccounts);
										
										if (!BottomAccountIsVisible && row == 3 + TotalAccounts + add)
										{
											FillRectBoth(ExpandRect(rect2,1,1,1,sretcg-rect2.Height+1), TableBackgroundID);
										}
												
										if ((pSelectedColumn == ColumnTitle || (scrollingneed && j == 1) || (pForceAllSelectedAccountsToTop && j == 1)) && row == 2)
										{
											int sre2tcg = 2; // siz
											
											sre2tcg = (int) Math.Round(sre2tcg*dpiY/100, 0);
											
											FillRectBoth(ExpandRect(rect2,1,1,1,sre2tcg-rect2.Height+1), TableBackgroundID);
										}
						
										// highlight for master and follower accounts
										
										if (row != 1)
										if (br[TableFinalBackgroundID].DxBrush != br[TableBackgroundID].DxBrush)
										{
//p($"{row}  Printing  {br[ChartBackgroundID].MediaBrush} {br[ChartBackgroundID].DxBrush.Opacity} opacity");
											//FillRectBoth(ExpandRect(rect2,0,-1,0,-1), ChartBackgroundID); //gray box, why?
											FillRectBoth(ExpandRect(rect2,0,-1,0,-1), TableFinalBackgroundID);
										}
										if (IsAnyAccountRow)
										if (ColumnTitle == "Daily Goal")
										if (pDailyGoalDisplayMode == "Status")
										if (DrawText)
										{
											double dg = GetAccountSavedData(ThisRowAccount, pAllAccountDailyGoal);
											double totalw = (double) rect2.Width;
											double percent = 100;
											
											percent = ThisBOTChart.PNLTotal / dg * 100;
											
										
											//Print(percent);
											// percent view
											
											if (pShowPercentOnGoalLoss)
											text = Math.Round(percent,0).ToString()+" %";
											

											//if (percent > 100)
											text = text.Replace("$0", "GOAL");	
											
											if (ThisBOTChart.PNLTotal < 0)
												text = "–";
											
											totalw = totalw*percent/100;
											totalw = Math.Min(totalw, rect2.Width);
											totalw = Math.Max(totalw, 0);
										
											SharpDX.RectangleF ProgressRect = new SharpDX.RectangleF(rect2.Right-(float)totalw,rect2.Top,(float)totalw-1,rect2.Height);
					
											if (pShowLossOnLeft)
												ProgressRect = new SharpDX.RectangleF(rect2.Left,rect2.Top,(float)totalw-1,rect2.Height);
											
											if (ThisBOTChart.PNLTotal > 0)
											{
												if (HighlightThisRow)
													br[TableBackgroundID].DxBrush.Opacity = br[TableBackgroundID].DxBrush.Opacity+pHighlightAmount/100f;	

												FillRectBoth(ExpandRect(rect2,0,0,0,0), ChartBackgroundID);//gray box, why?
												FillRectBoth(ExpandRect(rect2,1,1,1,0), TableBackgroundID);

												TableFinalBackgroundID = pTrailingGoodColorID;
												
												br[TableFinalBackgroundID].DxBrush.Opacity = 0.2f;

												if (HighlightThisRow)
													br[TableFinalBackgroundID].DxBrush.Opacity = br[TableFinalBackgroundID].DxBrush.Opacity+pHighlightAmount/100f;

												FillRectBoth(ExpandRect(ProgressRect,0,0,0,0), ChartBackgroundID);
												FillRectBoth(ExpandRect(ProgressRect,0,0,0,0), TableFinalBackgroundID);
											
												if (percent < 80)
													TableFinalBackgroundID = TableBackgroundID;
											}
										}

										if (IsAnyAccountRow)
										if (ColumnTitle == "Daily Loss")
										if (pDailyLossDisplayMode == "Status")
										if (DrawText)
										{
											double dg = GetAccountSavedData(ThisRowAccount, pAllAccountDailyLoss);
										
											double totalw = (double) rect2.Width-1;
											double percent = 100;
											
											percent = -1*ThisBOTChart.PNLTotal / dg * 100;

											// percent view
											
											if (pShowPercentOnGoalLoss)
												text = Math.Round(percent,0).ToString()+" %";
											
											
											
											//if (percent > 100)
											text = text.Replace("$0", "LOSS");
											
												if (ThisBOTChart.PNLTotal > 0)
													text = "–";
												
											totalw = totalw*percent/100;
											totalw = Math.Min(totalw, rect2.Width);
											totalw = Math.Max(totalw, 0);
											
											//SharpDX.RectangleF ProgressRect = new SharpDX.RectangleF(rect2.Right-(float)totalw,rect2.Top,(float)totalw,rect2.Height);
											
											var ProgressRect = new SharpDX.RectangleF(rect2.Left,rect2.Top,(float)totalw-1,rect2.Height);
											
											if (pShowLossOnLeft)
												ProgressRect = new SharpDX.RectangleF(rect2.Right-(float)totalw,rect2.Top,(float)totalw-1,rect2.Height);											
											

											if (totalw != 0)
											if (ThisBOTChart.PNLTotal < 0)
											{
												if (HighlightThisRow)
													br[TableBackgroundID].DxBrush.Opacity = br[TableBackgroundID].DxBrush.Opacity+pHighlightAmount/100f;	

												FillRectBoth(ExpandRect(rect2,0,0,0,0), ChartBackgroundID);
												FillRectBoth(ExpandRect(rect2,1,1,1,0), TableBackgroundID);

												TableFinalBackgroundID = pTrailingBlownColorID;
												
												br[TableFinalBackgroundID].DxBrush.Opacity = 0.2f;
											
												if (HighlightThisRow)
													br[TableFinalBackgroundID].DxBrush.Opacity = br[TableFinalBackgroundID].DxBrush.Opacity+pHighlightAmount/100f;

												FillRectBoth(ExpandRect(ProgressRect,0,0,0,0), ChartBackgroundID);
												FillRectBoth(ExpandRect(ProgressRect,0,0,0,0), TableFinalBackgroundID);
												
												if (percent < 80)
													TableFinalBackgroundID = TableBackgroundID;
											}
										}
										if (CellIsButtonBorder)
										if (!MouseIn(ClickRect,0,0))
										{
											br[TableFinalBackgroundID].DxBrush.Opacity = 0.08f;
											RenderTarget.DrawRectangle(ExpandRect(ClickRect,-1,0,-1,0), br[TableFinalBackgroundID].DxBrush, 1);
											br[TableFinalBackgroundID].DxBrush.Opacity = 0.14f;	
											RenderTarget.DrawRectangle(ExpandRect(ClickRect,-2,-1,-2,-1), br[TableFinalBackgroundID].DxBrush, 1);											
										}										

										// always show text for second row buttons if mouse is in that row

										if (pHighlightHoverButtons && CellIsButton)
										{
											if (HoveredRow == 2 && row == 2)
											{
												if (ColumnTitle == SelectedResetColumn)
												{
													text = "\u2713";//checkmark
												}
												else
												{
													text = "Reset";	
													
													if (ColumnTitle == "Auto Exit" || ColumnTitle == "Auto Close")
													{
														text = "\u2713";//checkmark
													}

													if (ColumnTitle == "Account" && AllDuplicateAccounts.Count == 0)
														text = "Select All";

													if (IsKorean)
													{
													
														text = text.Replace("Reset", "초기화");
														text = text.Replace("Select All", "모두 선택");
													}		

													br[textID].DxBrush.Opacity = fasttextop;
													
												}
												textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
											}

											if (MouseIn(ClickRect,0,0))
											{
												ThisBrushID = TableFinalBackgroundID;
												br[ThisBrushID].DxBrush.Opacity = pButtonHighlightO/100f;
												RenderTarget.DrawRectangle(ExpandRect(ClickRect,-1,0,-1,0), br[ThisBrushID].DxBrush, 3);
											}
										}

										rightoftable = 0;
										rightoftable = Math.Max(rightoftable,(int)rect2.Right+1);

										RenderTarget.AntialiasMode = oldAntialiasMode;
									}

									if (text == "\u2713")//checkmark
									{
										SimpleFont FinalFont1 = new SimpleFont(TextFont3.Family.ToString(), (int)TextFont3.Size + 3);

										TextRect = MoveRect(TextRect, 0, -1);

										textFormat2 = FinalFont1.ToDirectWriteTextFormat();	
										textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
							            textFormat2.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center; // far = bottom.
							            textFormat2.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
									} 

									bool connnnn = pShowConnectedStatus && ColumnTitle == "–";
									if (IsAnyAccountRow)
									if (connnnn)
									{
										float ellllll = 6;
										ellllll = (float) Math.Floor(TextRect.Height/4f);
										ellllll = Math.Max(5, ellllll);
										
										int shiftup = 0;
										if (TextRect.Height == 24 || TextRect.Height == 23 || TextRect.Height == 22)
											shiftup = 1;
										
										if (ColumnTitle == "–")
											shiftup = 0;
											
//										Print("ColumnTitle " + ColumnTitle);
//										Print("TextRect.Height " + TextRect.Height);
//										Print("TextRect.Top " + TextRect.Top);
//										Print("shiftup " + shiftup);

										Point MP2 = new Point(TextRect.Left + TextRect.Width/2, TextRect.Top + TextRect.Height/2 + 1 - shiftup);
											//AddButtonZ5(AllButtonZ5,ThisBOTChart,ClickRect, "Disconnected");	
											//CellIsButton = true;
										
										var el = new SharpDX.Direct2D1.Ellipse(MP2.ToVector2(), 5, 5);

										//selectBrushDX = 
			                            //br[textID].DxBrush.Opacity = 0.5f;

										oldAntialiasMode	= RenderTarget.AntialiasMode;
						
							
										RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
										textID = pConnectedOtherID;
										
										
										if (text == "CON")
										textID = pConnectedOnID;
							

										if (text == "DIS")
										textID = pConnectedOffID;
										

										if (text == "COLOST")
										textID = pConnectedLostID;
										
										
										
										//IsAccountConnectionLost
										
	
										el = new SharpDX.Direct2D1.Ellipse(MP2.ToVector2(), ellllll, ellllll);										
										br[textID].DxBrush.Opacity = 0.2f;										
			                            RenderTarget.FillEllipse(el, br[textID].DxBrush);									
										br[textID].DxBrush.Opacity = 0.6f;
			                            RenderTarget.DrawEllipse(el, br[textID].DxBrush);
										
										
										el = new SharpDX.Direct2D1.Ellipse(MP2.ToVector2(), ellllll-1, ellllll-1);
										br[textID].DxBrush.Opacity = 0.3f;										
			                            RenderTarget.FillEllipse(el, br[textID].DxBrush);
										
										
										el = new SharpDX.Direct2D1.Ellipse(MP2.ToVector2(), ellllll-2, ellllll-2);
										br[textID].DxBrush.Opacity = 0.4f;									
			                            RenderTarget.FillEllipse(el, br[textID].DxBrush);	
										
										
										el = new SharpDX.Direct2D1.Ellipse(MP2.ToVector2(), ellllll-3, ellllll-3);
										br[textID].DxBrush.Opacity = 0.5f;										
			                            RenderTarget.FillEllipse(el, br[textID].DxBrush);
										RenderTarget.AntialiasMode = oldAntialiasMode;

										DrawText = false;
									}
									if (DrawText)
									{
										oldAntialiasMode = RenderTarget.AntialiasMode;
										RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
										br[textID].DxBrush.Opacity = br[textID].InitialOpacity;
										RenderTarget.DrawText(text, textFormat2, TextRect, br[textID].DxBrush);
										RenderTarget.AntialiasMode = oldAntialiasMode;
									}
								}
							}//end of loop through columns
							#endregion
							RectY = RectY + RectHeight;
						}
						//#endregion
					}

						TableFinalBackgroundID = TableBackgroundID;
						rect2			= new SharpDX.RectangleF(MinX, MinY, MaxX - MinX+1, MaxY - MinY+1);
						RenderTarget.DrawRectangle(rect2, br[TableFinalBackgroundID].DxBrush, 1);

					// the textLayout object provides a way to measure the described font through a "Metrics" object
					// This allows you to create new vectors on the chart which are entirely dependent on the "text" that is being rendered
					// For example, we can create a rectangle that surrounds our font based off the textLayout which would dynamically change if the text used in the layout changed dynamically
					
					//int TableMargin = 5;	
						
					SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout2.Metrics.Width - TableMarginX,
						ChartPanel.Y + (ChartPanel.H - textLayout2.Metrics.Height - TableMarginY));
						
						
						
//					SharpDX.RectangleF rect1 = new SharpDX.RectangleF(lowerTextPoint.X - CellPadding, lowerTextPoint.Y - CellPadding, textLayout2.Metrics.Width + CellPadding,
//						textLayout2.Metrics.Height + CellPadding);

	

					// And render the advanced text layout using the DrawTextLayout() method
					// Note:  When drawing the same text repeatedly, using the DrawTextLayout() method is more efficient than using the DrawText() 
					// because the text doesn't need to be formatted and the layout processed with each call
					//RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

					
					// 1.8 - Cleanup
					// This concludes all of the rendering concepts used in the sample
					// However - there are some final clean up processes we should always provided before we are done

					// If changed, do NOT forget to set the AntialiasMode back to the default value as described above as a best practice
					RenderTarget.AntialiasMode = oldAntialiasMode;

					// We also need to make sure to dispose of every device dependent resource on each render pass
					// Failure to dispose of these resources will eventually result in unnecessary amounts of memory being used on the chart
					// Although the effects might not be obvious as first, if you see issues related to memory increasing over time
					// Objects such as these should be inspected first
					
//					areaBrushDx.Dispose();
					//customDXBrush.Dispose();
//					gradientStopCollection.Dispose();
//					radialGradientBrush.Dispose();
				
					//textBrushDx.Dispose();
					//textFormat1.Dispose();
					
					textFormat2.Dispose();
					
					
					//textLayout1.Dispose();
					textLayout2.Dispose();

//					Print("ChartPanel.W  " + ChartPanel.W);
//					Print("chartScale.Width  " + chartScale.Width);
					
				
					
					ThisWindowWidth = RectX + RectWidth;
					ThisWindowHeight = RectY + RectHeight + pPixelsFromTop + ButtonsHeight + pPixelsFromTop + pPixelsFromTop + 70;
					
					//Print(TableMarginX);
					
					ThisWindowWidth = ThisWindowWidth + (int) chartControl.AxisYRightWidth;// - TableMarginX;
					
					
					ThisWindowHeight = ThisWindowHeight + 0;
					
					
					if (pAutoSizeWindow)
					if (ThisWindowWidth > chartWindow.Width)
					ChartControl.Dispatcher.InvokeAsync(new Action(() =>
					{
						
						chartWindow.Width = ThisWindowWidth;
						//chartWindow.Height = ThisWindowHeight;
										
					}));				
					
					
		
					
					
					if (pShowBorderWarning)
					if (pCopierIsEnabled)
					{
//						ThisStroke = pOrderDnOutlineStroke;
						br[pOrderDnOutlineStrokeID].DxBrush = pOrderDnOutlineStroke.BrushDX;
						// left
						
						ThisRect = new SharpDX.RectangleF(0, 0, pOrderDnOutlineStroke.Width, ChartPanel.H);
						FillRectBoth(ThisRect, pOrderDnOutlineStrokeID);
						
						// right
						ThisRect = new SharpDX.RectangleF(ChartPanel.W-pPixelsFromTop, 0, pPixelsFromTop, ChartPanel.H);
						FillRectBoth(ThisRect, ChartBackgroundID);
						
						ThisRect = new SharpDX.RectangleF(ChartPanel.W-pOrderDnOutlineStroke.Width, 0, pOrderDnOutlineStroke.Width, ChartPanel.H);
						FillRectBoth(ThisRect, pOrderDnOutlineStrokeID);				
						// top
						
						ThisRect = new SharpDX.RectangleF(ChartPanel.X, 0, ChartPanel.W, pOrderDnOutlineStroke.Width);
						FillRectBoth(ThisRect, pOrderDnOutlineStrokeID);
				
						// bottom
						ThisRect = new SharpDX.RectangleF(ChartPanel.X, ChartPanel.H-pOrderDnOutlineStroke.Width, ChartPanel.W, pOrderDnOutlineStroke.Width);
						FillRectBoth(ThisRect, pOrderDnOutlineStrokeID);
					}
				// buttons
				LeftSpaceButtons = pPixelsFromLeft;
				space = 6;
				space = (int) Math.Round(space*dpiY/100, 0);
				
				
				CY = (float)chartControl.CanvasRight - 48f; // top of chart offset from right
				CY = (float)chartControl.CanvasRight - space + 3; // bottom of chart
				CY = LeftSpaceButtons;
				
				thistop = 5;
				
				//space = ChartPanel.Height - 5;

				
				rightoftable = Math.Min(rightoftable, (float)chartControl.CanvasRight - 2);
			
				nextrightside = rightoftable;
			 
				
				//nextrightside = Math.Min(rightoftable, (float)chartControl.CanvasRight - 2);
					//return;

				
				
				leftsidebuttonsrightx = 0;
				
			//InMenu = true;
				
			
					//Print("ThisR 16");	
					
	
            if (pButtonsEnabled)
			{

				TableBackgroundID = pCompMainColorID;
				br[TableBackgroundID].DxBrush.Opacity = pCompMinOpacity/100f;
				
                foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
                //foreach (string xxx in AllButtons)
                {
					if (thisbutton.Value.Location != "Bottom")
						continue;

					thisbutton.Value.Rect = new SharpDX.RectangleF(0,0,0,0);
					
					string ButtonName = thisbutton.Value.Text;
                    string xxx = ButtonName;
					//pIsCopyBasicFunctionsEnabled
					if (!pIsCopyBasicFunctionsPermission || !pIsRiskFunctionsPermission)
					{
						if (xxx == RiskButtonString)
							continue;
											
						if (xxx == CopyButtonString)
							continue;
					}

					if (!pIsCopyBasicFunctionsChecked || !pIsRiskFunctionsChecked)
					{
						if (xxx == RiskButtonString)
							continue;
											
						if (xxx == CopyButtonString)
							continue;
								
					}
					
					
					if (!pIsCopyBasicFunctionsChecked || !pIsCopyBasicFunctionsEnabled || !pIsCopyBasicFunctionsPermission)
					{
					
						
						
						if (xxx == "Main" || xxx == "Fade" || xxx == "Size" || xxx == "Mode" || xxx == "Type" || xxx == "Orders" || xxx == "Executions") 
							continue;
											
					
					}
					
					
					if (xxx == "Orders" || xxx == "Executions")
						xxx = pCopierMode;
					
				
					if (xxx == "Fade")
                		thisbutton.Value.Switch = pIsFadeEnabled;

					if (xxx == "Mode")
                		thisbutton.Value.Switch = pIsATMSelectEnabled;					
					
					
					if (!pExitShieldFeaturesEnabled && xxx == "Exit Shield")
						continue;					
					
				
					
					if (!pIsBuildMode && xxx == "Reset")
						continue;
					
					if (!pIsBuildMode && xxx == "Accounts")
						continue;
					
					if (!pIsBuildMode && xxx == "Live Accounts")
						continue;
					
					if (!pIsBuildMode && xxx == "Sim Accounts")
						continue;
					
					if (xxx == pRPString)
					{
					
						int TotalAccountsInSync = 1 + AllDuplicateAccounts.Count;
						
						
						if (pShowRefreshPositions && RefreshPositionsClickedTotal != 0 && RefreshPositionsClickedTotal != TotalAccountsInSync)
							{
								//Print("RefreshPositionsClickedTotal" + RefreshPositionsClickedTotal);
								//Print("TotalAccountsInSync" + TotalAccountsInSync);
							}	
							else
							{
								continue;
							}
					
					}
					
					//if (!pIsBuildMode && pLockAccountsOnLock && xxx == "Restore")
					
					
					
					
					
					
					if (!pIsBuildMode && xxx == "Restore")
						continue;					
					
					if (AllHideAccounts.Count == 0 && xxx == "Restore")
						continue;
						
					if (!HasAPEXAccounts && xxx == APEXButton)
						continue;
						
					//Print(xxx);
//					if (xxx == "Main")
//					{
//						xxx = "ON";
//						if (!pCopierIsEnabled)
//							xxx = "OFF";
//					}
					
					
					if (xxx == "Accounts")
					{
					
									
						if (pShowAccounts == "All")
						{
							xxx = "All Accounts";
						}
						else if (pShowAccounts == "Live")
						{
							xxx = "Live Accounts";
						}					
						else if (pShowAccounts == "Sim")
						{
							xxx = "Sim Accounts";
						}				
							
						
						
					}
					
					
					
					if (xxx == "All Instruments")
					{
						if (!pAllInstruments)
						{
						
						
							string thischarti = Instrument.FullName;
								
							thischarti = GetTheInstrument(thischarti, "Mini", false).FullName;	
							
//								if (!thischarti.Contains("YM") && !thischarti.Contains("MYM"))
//									thischarti = Instrument.FullName.Replace("M", "");
								
//								if (thischarti == "MYM")
//									thischarti = "YM";								
								
//								if (thischarti == "2K")
//									thischarti = "RTY";
								
//								if (thischarti == "FDXS")
//									thischarti = "FDXM";
								
								xxx = thischarti;
						}
								
								
					}
					
                    string sd = xxx;

					float widdd = 50;
                    widdd = Math.Max(pButtonSize, widdd);

                    if (thisbutton.Value.Width == 1)
                        widdd = pButtonSize;
                    else
                        widdd = thisbutton.Value.Width;

					SharpDX.DirectWrite.TextFormat ButtonTextFormat = new SimpleFont(TextFont3.Family.ToString(), TextFont3.Size+pFontSizeB).ToDirectWriteTextFormat();					

					ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                    ButtonTextFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                    ButtonTextFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

					string sizetx = xxx;
					
					if (ButtonName == "Main")
						sizetx = "OFF";
					if (ButtonName == "Accounts")
						sizetx = "Live Accounts";
					
					
					
					if (IsKorean)
					{
					
						sizetx = sizetx.Replace("Size", "사이즈");
						sizetx = sizetx.Replace("Type", "유형");
						sizetx = sizetx.Replace("Mode", "모드");
						sizetx = sizetx.Replace("Fade", "반대진입");
						
						xxx = sizetx;
					}
					
				
								
			                    TextLayout textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, sizetx, ButtonTextFormat, 10000, 10000);
			
			                   // Print(textLayout1.Metrics.Width);
			
			
			                    float FinalH = textLayout1.Metrics.Height + (float) Math.Round(6*dpiY/100, 0);
			
			                    FinalH = Math.Max(pButtonSize, FinalH) + pFontSizeB/2 + 3;
			

								
								
			                    float FinalW = 0;
							
			                    if (thisbutton.Value.Width == 1)
			                        FinalW = FinalH;
			                    else
			                        FinalW = textLayout1.Metrics.Width + (float) Math.Round(8*dpiY/100, 0) + 4;
								
								
			
								
								if (pFontSizeB != 0)
								{
									FinalH = FinalH + pFontSizeB/2;
									FinalW = FinalW + pFontSizeB/0.7f;
								}
								
								
								
								ButtonsHeight = (int) FinalH;
								
								
								FinalW = Math.Max(FinalH, FinalW);
								
								
			//					FinalW = (int) Math.Round(FinalW*dpiY/100, 0);
			//					FinalH = (int) Math.Round(FinalH*dpiY/100, 0);
			//					space = (int) Math.Round(space*dpiY/100, 0);
								
					
					
					
					thistop = (float) ChartPanel.H - FinalH - 1; // move to bottom of chart
					
					//Print(thistop);


                    if (xxx == "")
						FinalW = 15;
					
					float nextleft = CY;
					
					if (sizetx == APEXButton || sizetx == CopyButtonString || sizetx == RiskButtonString || sizetx == pFlattenButtonString || sizetx == pRPString || sizetx == pHideAString || sizetx == pHideCString  )
					{
						// hide apex button if out of room
						if (sizetx == APEXButton)
						{
							
							float totalofthree = 0;
							 
							if (pShowCopyRiskButtons)
							if (pIsCopyBasicFunctionsChecked && pIsRiskFunctionsChecked)
							{
								
								 textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, CopyButtonString, ButtonTextFormat, 10000, 10000);
								totalofthree = totalofthree + textLayout1.Metrics.Width + 8;
								 textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, RiskButtonString, ButtonTextFormat, 10000, 10000);
								totalofthree = totalofthree + textLayout1.Metrics.Width + 8;
								
							
								
								
							}
							
							if (pShowFlattenEverything)
							{
								textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, pFlattenButtonString, ButtonTextFormat, 10000, 10000);
								totalofthree = totalofthree + textLayout1.Metrics.Width + 8;		
								
							}
							
							if (pShowRefreshPositions && RefreshPositionsClickedTotal != 0 && RefreshPositionsClickedTotal != TotalDisplayAccounts)
							{
								textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, pRPString, ButtonTextFormat, 10000, 10000);
								totalofthree = totalofthree + textLayout1.Metrics.Width + 8;		
								
							}
							
							
							
							 textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, APEXButton, ButtonTextFormat, 10000, 10000);
							totalofthree = totalofthree + textLayout1.Metrics.Width + 8;							
								

		                 
							
							
							if (leftsidebuttonsrightx > rightoftable - totalofthree)
								continue;
									
							
							
							
							
						}
						
						
						CY = nextrightside - FinalW;
						nextleft = nextrightside - FinalW;
						
						nextrightside = nextleft;
						 nextrightside = nextrightside - space;
						
						
	
					}
					else
					{
						leftsidebuttonsrightx = nextleft + FinalW;
						
					}
					
//					if (textLayout1 != null)
//					textLayout1.Dispose();
					
					
                    thisbutton.Value.Rect = new SharpDX.RectangleF(nextleft, thistop, FinalW, FinalH);
					
					CY = CY + FinalW;
                    CY = CY + space;

                    //CY = CY - widdd - space;
                   // thisbutton.Value.Rect = new SharpDX.RectangleF(CY, space, widdd, pButtonSize);
                   
                    minx = Math.Min(minx, CY - space);
					miny = Math.Min(miny, thistop - space);

					
					if (xxx != "") // allow spacer
					{
											
						//buttonFONID = ChartBackgroundID;//ThisChartBackground().ToDxBrush(RenderTarget);
						//br[buttonFONID].DxBrush.Opacity = 0.9f;
						
						//buttonFOFFID = pButtonOffColorID;
						//br[buttonFOFFID].DxBrush.Opacity = 0.5f;
								
						
						//if (InMenu)
						{
		           
							
							
						if (ButtonName == "Main")
						{
//							thisbutton.Value.BrushOn = pCopierButtonOn2ID;
							//buttonFONID = pCopierButtonOn2ID;
//							br[buttonFONID].DxBrush.Opacity = 0.5f;
	
						}
						if (ButtonName == "Lock")
						{
//							thisbutton.Value.BrushOn = pLockButtonOnID;
							//buttonFONID = pLockButtonOnID;
							//buttonFOFFID = pButtonOffColorID;
//							br[buttonFONID].DxBrush.Opacity = 0.5f;							
//										buttonFOFFBrushDX = pLockButtonOn.ToDxBrush(RenderTarget);
//										buttonFOFFBrushDX.Opacity = 0.5f;
//										buttonFONBrushDX = pButtonOffColor.ToDxBrush(RenderTarget);
//										buttonFONBrushDX.Opacity = 0.5f;	
						}									
						if (ButtonName == pFlattenButtonString)
						{
//							thisbutton.Value.BrushOn = pFlattenButtonBID;
							//buttonFONID = pFlattenButtonBID;
//							br[buttonFONID].DxBrush.Opacity = 0.5f;
	
						}							
						
						if (ButtonName == pRPString)
						{
//							thisbutton.Value.BrushOn = pRefreshButtonBID;
							//buttonFONID = pRefreshButtonBID;
//							br[buttonFONID].DxBrush.Opacity = 0.5f;
	
						}							 

						bool switched = thisbutton.Value.Switch;
						if (ButtonName == "Main")
							switched = pCopierIsEnabled;
						if (ButtonName == "Lock")
							switched = !pIsBuildMode;	         				
						if (ButtonName == CopyButtonString)
							switched = pIsCopyBasicFunctionsEnabled;
						if (ButtonName == RiskButtonString)
							switched = pIsRiskFunctionsEnabled;							

						if (ButtonName == pFlattenButtonString)
						{
							if (DateTime.Now < FlattenEverythingClickedTime.AddMilliseconds(pMilliSFE))
								{
									switched = false;
								}
								else
								{
									switched = true;
								}
							
					
	
						}	
										if (ChartControl.Indicators.Count == 1 || ChartControl.Indicators.Count == addedtwice)
										{
						                    if (MouseIn(thisbutton.Value.Rect, 2, 2))
						                    {
												RenderTarget.FillRectangle(thisbutton.Value.Rect, br[switched? thisbutton.Value.BrushOnHover : thisbutton.Value.BrushOffHover].DxBrush);
												RenderTarget.DrawRectangle(thisbutton.Value.Rect, br[thisbutton.Value.BrushOutlineHover].DxBrush, 3);
												string tbbb = thisbutton.Value.Name;
												if(tbbb.Length>0){
													var NNNNN = new SharpDX.RectangleF(thisbutton.Value.Rect.X,thisbutton.Value.Rect.Top-20,200,20);
													ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
													RenderTarget.DrawText(tbbb, ButtonTextFormat, NNNNN, br[ChartTextID].DxBrush);
												}
											}else{
												RenderTarget.FillRectangle(thisbutton.Value.Rect, br[switched? thisbutton.Value.BrushOn : thisbutton.Value.BrushOff].DxBrush);
												RenderTarget.DrawRectangle(thisbutton.Value.Rect, br[thisbutton.Value.BrushOutlineHover].DxBrush, 1);
											}

//											ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
//						                    RenderTarget.DrawText(xxx, ButtonTextFormat, thisbutton.Value.Rect, br[ChartTextID].DxBrush);

											// button fills
										#region -- old handler --
										/*
										if (ChartControl.Indicators.Count == 1 || ChartControl.Indicators.Count == addedtwice)
										{
											FillRectBoth(thisbutton.Value.Rect, ChartBackgroundID);

											if (switched)
											{
						                       // FillRectBoth(thisbutton.Value.Rect, TableBackgroundBrushDX);
												ThisBrushID = TableBackgroundID;
											}
											else
											{
												//  FillRectBoth(thisbutton.Value.Rect, buttonFOFFBrushDX);
												ThisBrushID = buttonFOFFID;
											}
											
											FillRectBoth(thisbutton.Value.Rect, ThisBrushID);
											
											
						                    if (MouseIn(thisbutton.Value.Rect, 2, 2))
						                    {
												FillRectBoth(thisbutton.Value.Rect, ThisBrushID);
//												if (switched)
//												{
			
//							                        FillRectBoth(thisbutton.Value.Rect, TableBackgroundBrushDX);
													
												
//												}
//												else
//												{
													
			
//								                        FillRectBoth(thisbutton.Value.Rect, buttonFOFFBrushDX);
														
												
											
//												}										
												
											}
		
				
											RenderTarget.DrawRectangle(ExpandRect(thisbutton.Value.Rect,0,0), br[ChartBackgroundID].DxBrush, 1);
											
										
											RenderTarget.DrawRectangle(ExpandRect(thisbutton.Value.Rect,0,0), br[ThisBrushID].DxBrush, 1);										
											RenderTarget.DrawRectangle(ExpandRect(thisbutton.Value.Rect,0,0), br[ThisBrushID].DxBrush, 1);
										
									
											
						                    if (MouseIn(thisbutton.Value.Rect, 2, 2))
						                    {
						                     
						                        RenderTarget.DrawRectangle(thisbutton.Value.Rect, br[WHITE].DxBrush, 3);
				
												string tbbb = thisbutton.Value.Name;
												//tbbb = "safasg";
												SharpDX.RectangleF NNNNN = new SharpDX.RectangleF(thisbutton.Value.Rect.X,thisbutton.Value.Rect.Top-20,200,20);
												
												ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
												
												RenderTarget.DrawText(tbbb, ButtonTextFormat, NNNNN, br[SILVER].DxBrush);
						                    }
											*/
											#endregion
									
											int ddddd = 0;
											
											if (xxx == pHideCString || xxx == pHideAString)
											{
												SimpleFont FinalFont1 = new SimpleFont(TextFont3.Family.ToString(), (int)TextFont3.Size +pFontSizeB+ 2);
												
												ddddd = 0;
												
												ButtonTextFormat = FinalFont1.ToDirectWriteTextFormat();	
							                    ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
							                    ButtonTextFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
							                    ButtonTextFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
	
											} 
											ButtonTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
						                    RenderTarget.DrawText(xxx, ButtonTextFormat, MoveRect(thisbutton.Value.Rect, 0, ddddd), br[ChartTextID].DxBrush);
										}
						}
					}
                }
				
				 
				B2 = new SharpDX.RectangleF(minx, miny, 10000, 10000);

				minx = 999999;
				miny = 999999;	
				
				
//				buttonBrushDX.Dispose();
//	            buttonHBrushDX.Dispose();
//				buttonFHBrushDX.Dispose();
//	            buttonFOFFBrushDX.Dispose();
//				buttonFONBrushDX.Dispose();
								
				
					
				

			}
				//TableBackgroundBrushDX
					
			
			
			
					
			
				//ThisRect = new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H);
				//RenderTarget.DrawRectangle(ExpandRect(ThisRect,-1,-1,-1,-1), ThisStroke.BrushDX, ThisStroke.Width, ThisStroke.StrokeStyle);
				
				
				
				//Print("messages");
				
			
				//Print("R 1");
			
			
				starty = 0;
				starty = ChartPanel.H + 1;
			
			
				starty = thistop;
			
			
				
			
			//Print("R 2");	
			
				ChartBackgroundFadeID = ChartBackgroundID;//ThisChartBackground().ToDxBrush(RenderTarget);
				br[ChartBackgroundFadeID].DxBrush.Opacity = 0.5f;
				ThisBrushID = ChartTextID;
			
					
			
			
			
			
			
			// this section is causing the window to really slow down
			
			
			
//				bool asdfgfdasfg = false;
			
//				if (asdfgfdasfg)
				
		
				foreach (KeyValuePair<double, PendingDetails> kvp in LimitPriceToTimeOutOfSync2)
				{
			

					
					
					TimeSpan TimeSinceLoad = DateTime.Now - kvp.Value.StartTime;
					
					int tmm = (int) TimeSinceLoad.TotalMilliseconds;
					
					//Print(tmm);
					
				
						//if (DateTime.Now > MoveOClicked) // at least one second since clicked, or never clicked
						{
							if (tmm < 1000) // at least one second click since created, avoid these displaying when orders are getting placed
							continue;						
						}
					
					
					
//					if (tmm < 1000)
//						continue;
					
					
					string timeLeft = (TimeSinceLoad.Ticks < 0
								? "0:00"
								: TimeSinceLoad.Hours.ToString("00") + ":" + TimeSinceLoad.Minutes.ToString("00") + ":" + TimeSinceLoad.Seconds.ToString("00"));
							
							if (TimeSinceLoad.TotalHours < 1 && TimeSinceLoad.Ticks >= 0)
								timeLeft = TimeSinceLoad.Minutes.ToString("0") + ":" + TimeSinceLoad.Seconds.ToString("00");
							
							
							
							
					double instprice = 0;
								
					if (kvp.Value.Name.MarketData.Last != null)
					{
						instprice = kvp.Value.Name.MarketData.Last.Price;
					}
					double pointsfromfill = instprice - kvp.Key;
					
					
							
					if (kvp.Value.Direction == -1)
					{
						pointsfromfill = kvp.Key - instprice;
					}
					
					string finalnumberdistance = FormatPriceMarker2(pointsfromfill);
					
					int ticksfromfill = (int) (pointsfromfill / 	kvp.Value.Name.MasterInstrument.TickSize);	
						
					//finalnumberdistance = ticksfromfill.ToString();
					
				
					string price = FormatPriceMarker2(kvp.Key);
					string totaliiii = string.Empty;
					string ordername = string.Empty;
					
					string totalorders = kvp.Value.Width.ToString() + " Orders";
					
						foreach (Instrument ii in kvp.Value.AllInstruments)
						{
						
							//string instrument = GetTheInstrument(kvp.Value.Name.FullName, "Mini").FullName;
							
							string instrument = ShortIN(ii.FullName);		
							
						 
							
							
							if (totaliiii == string.Empty)
								totaliiii = instrument;
							else
								totaliiii = totaliiii + " & " + instrument;
							
							if (kvp.Value.AllInstruments.Count == 2)
							{
								totaliiii = ShortIN(GetTheInstrument(ii.FullName, "Mini", false).FullName) + " & " + ShortIN(GetTheInstrument(ii.FullName, "Micro", false).FullName);
								
							}
							
						}
					
						ordername = "Limit";
	
						foreach (string ii in kvp.Value.AllOrderTypes)
						{
							//if (
						
							ordername = ii;
						}					
						
						
						foreach (string ii in kvp.Value.AllOrderNames)
						{
							//if (
						
							if (ii.Contains("Target"))
								ordername = ii.Replace("Target", "Target ");
						}
							
					
							
							
					ThisMessage = ordername + "  |  " + totaliiii + "  |  " + price + "  |  " + totalorders + "  |  " + timeLeft + "  |  " + finalnumberdistance;
					
							
						
						
					StatusButtonsID = RED;
					if (kvp.Value.Direction == 1)
						StatusButtonsID = LIME;
					
					if (ordername.Contains("Target"))
						StatusButtonsID = LIME;
					
					
//					if (ordername != string.Empty)
//					{
						
						
//					}
						
				
					
					//AddMessage("Pending Orders Out Of Sync" + kvp.Key + "   " + kvp.Value, 6, GOLDENROD);
					
					
					//Print("FILL REST OF LIMIT ORDERS " + kvp.Key + "   " + kvp.Value);
					
	//ThisStroke = pOrderBothOutlineStroke;

							
							//Print("F 1");
						
							ThisBrushID = StatusButtonsID;
							br[ThisBrushID].DxBrush.Opacity = 0.3f;
							
			
							//Print("F 2");
							
							//CellFormat			= myProperties.LabelFont.ToDirectWriteTextFormat();	
							CellFormat			= TextFont3.ToDirectWriteTextFormat();	
					
											
		                	CellFormat = new SimpleFont(TextFont3.Family.ToString(), TextFont3.Size+pFontSizeTopRow2).ToDirectWriteTextFormat();		
					
		                    CellFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
		                    CellFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
		                    CellFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.Wrap;
						
							
							//Print(ThisMessage);
							
							float leftrightpad = 8;
							float leftspace = (float) 8; // left pad of buttons	
						
							//leftspace = leftspace + leftrightspace;
							
							leftspace = pPixelsFromTop-1;
						
							//Print("F 3");
							
							
							float sw = 0;
//							sw = ThisStroke.Width;	
							
//							if (!pActiveOutlineEnabled)
//								sw = 0;
														
							float totalwww = ChartPanel.W-sw-sw-leftspace-leftrightpad-leftrightpad;//-leftrightspace-leftrightspace;
							
					
							totalwww = Math.Min(totalwww, rightoftable - leftspace - leftrightpad - leftrightpad);
							
					
							CellString = ThisMessage;
							CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat, totalwww, 10000);
							
		                    float FinalH = CellLayout.Metrics.Height;
		                    float FinalW = CellLayout.Metrics.Width;
							
					
							float hh = FinalH + ppppppp;
						
							
							
							starty = starty - hh - ppppppp2; 
							
				
							
							//Print("F 4");
							
							
							//ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw, ChartPanel.H-sw-hh, ChartPanel.W-sw-sw, hh);
							ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw+leftspace+leftrightpad, starty, totalwww, hh);
							
							ThisRect = ExpandRect(ThisRect,leftrightpad,leftrightpad,0,0);
							// add rectangles for buttons
					
							//D.Rect = ThisRect;
							FillRectBoth(ThisRect, ChartBackgroundID);
							FillRectBoth(ExpandRect(ThisRect,2,2,2,2), ChartBackgroundFadeID);
							FillRectBoth(ThisRect, ThisBrushID);
					
							//Print("F 5");
							//int sidetxtpad = 8;
							//ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw+leftspace+sidetxtpad, 0+sw+1, totalwww-sidetxtpad*2, hh);
							ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw+leftspace+leftrightpad, starty, totalwww, hh);
							
							//ThisBrushDX = br[ChartTextID].DxBrush;
							RenderTarget.DrawText(CellString, CellFormat, ThisRect, br[ChartTextID].DxBrush);	
					
							// ALL BUTTONS	
					
							ThisBrushID = StatusButtonsID;
							br[ThisBrushID].DxBrush.Opacity = 0.3f;
					
							CellFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
					
							float spacebetweenbuttons = 25;
					
							float expandv = 3;
					
												// new button
							
							float sppppp = leftspace*2+leftrightpad*2;
							float newleft = FinalW + sppppp + spacebetweenbuttons;
							
						
							// start buttons
					
					
						
							CellString = "Fill";
							CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat, 10000, 10000);
					
		                    FinalH = CellLayout.Metrics.Height;
		                    FinalW = CellLayout.Metrics.Width;			
					
							ThisRect = new SharpDX.RectangleF(newleft, starty-expandv, FinalW+leftspace*2, hh-1+expandv*2);
							kvp.Value.RectFill = ThisRect;
							ClickRect = ThisRect;

							FillRectBoth(ThisRect, ChartBackgroundID);
							FillRectBoth(ExpandRect(ThisRect,2,2,2,2), ChartBackgroundFadeID);
							FillRectBoth(ThisRect, ThisBrushID);	
							RenderTarget.DrawRectangle(ExpandRect(ThisRect,0,-1), br[ThisBrushID].DxBrush);
					
							ThisRect = MoveRect(ThisRect,0,1);
					
							br[ChartTextID].DxBrush.Opacity = 1f;
							RenderTarget.DrawText(CellString, CellFormat, ThisRect, br[ChartTextID].DxBrush);
					
							if (pHighlightHoverButtons)
							if (MouseIn(ClickRect,0,0))
							{
								ThisBrushID = ChartTextID;
								br[ThisBrushID].DxBrush.Opacity = pButtonHighlightO/100f;

								RenderTarget.DrawRectangle(ExpandRect(ClickRect,-1,-1,-2,-2), br[ThisBrushID].DxBrush, 3);	
							}						
					
					
					
					
						// button 2
					
					
							ThisBrushID = StatusButtonsID;
							br[ThisBrushID].DxBrush.Opacity = 0.3f;					
					
							newleft = ThisRect.Right + spacebetweenbuttons;


							CellString = "+1";
							CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat, 10000, 10000);

		                    FinalH = CellLayout.Metrics.Height;
		                    FinalW = CellLayout.Metrics.Width;			


							ThisRect = new SharpDX.RectangleF(newleft, starty-expandv, FinalW+leftspace*2, hh-1+expandv*2);
							kvp.Value.RectPlus = ThisRect;
							ClickRect = ThisRect;

							FillRectBoth(ThisRect, ChartBackgroundID);
							FillRectBoth(ExpandRect(ThisRect,2,2,2,2), ChartBackgroundFadeID);
							FillRectBoth(ThisRect, ThisBrushID);	
							RenderTarget.DrawRectangle(ExpandRect(ThisRect,0,-1), br[ThisBrushID].DxBrush);

							ThisRect = MoveRect(ThisRect,0,1);
					
							br[ChartTextID].DxBrush.Opacity = 1f;
							RenderTarget.DrawText(CellString, CellFormat, ThisRect, br[ChartTextID].DxBrush);	



							if (pHighlightHoverButtons)
							if (MouseIn(ClickRect,0,0))
							{

								ThisBrushID = ChartTextID;
								br[ThisBrushID].DxBrush.Opacity = pButtonHighlightO/100f;
							
								RenderTarget.DrawRectangle(ExpandRect(ClickRect,-1,-1,-2,-2), br[ThisBrushID].DxBrush, 3);
								
								
							}		
					
					
					
							// button 3
					
					
					
							ThisBrushID = StatusButtonsID;
							br[ThisBrushID].DxBrush.Opacity = 0.3f;
					
							newleft = ThisRect.Right + spacebetweenbuttons;
					
					
							CellString = "Cancel";
							CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat, 10000, 10000);
					
		                    FinalH = CellLayout.Metrics.Height;
		                    FinalW = CellLayout.Metrics.Width;			
					
							ThisRect = new SharpDX.RectangleF(newleft, starty-expandv, FinalW+leftspace*2, hh-1+expandv*2);
							kvp.Value.RectCancel = ThisRect;
							ClickRect = ThisRect;
							
							
							FillRectBoth(ThisRect, ChartBackgroundID);
							FillRectBoth(ExpandRect(ThisRect,2,2,2,2), ChartBackgroundFadeID);
							FillRectBoth(ThisRect, ThisBrushID);
							RenderTarget.DrawRectangle(ExpandRect(ThisRect,0,-1), br[ThisBrushID].DxBrush);
					
					
							ThisRect = MoveRect(ThisRect,0,1);
					
							br[ChartTextID].DxBrush.Opacity = 1f;
							RenderTarget.DrawText(CellString, CellFormat, ThisRect, br[ChartTextID].DxBrush);	
					
							
					
							if (pHighlightHoverButtons)
							if (MouseIn(ClickRect,0,0))
							{

								ThisBrushID = ChartTextID;
								br[ThisBrushID].DxBrush.Opacity = pButtonHighlightO/100f;
							
								RenderTarget.DrawRectangle(ExpandRect(ClickRect,-1,-1,-2,-2), br[ThisBrushID].DxBrush, 3);
								
								
							}					
					
					
					
					
					
					
							//Print("F 6");
							
//							StartPoint	= new Point(ThisRect.Right, ThisRect.Bottom);							
//							EndPoint = new Point(ThisRect.Right-ThisRect.Height, ThisRect.Top);								

							
//							RenderTarget.DrawLine(StartPoint.ToVector2(), EndPoint.ToVector2(), br[ChartBackgroundID].DxBrush, ThisStroke.Width, ThisStroke.StrokeStyle);
										
//							StartPoint	= new Point(ThisRect.Right, ThisRect.Top);							
//							EndPoint = new Point(ThisRect.Right-ThisRect.Height, ThisRect.Bottom);								
							
//							RenderTarget.DrawLine(StartPoint.ToVector2(), EndPoint.ToVector2(), br[ChartBackgroundID].DxBrush, ThisStroke.Width, ThisStroke.StrokeStyle);
								
								
							
							
							
							
							
							
							
							
							
							
//							ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw, starty, ChartPanel.W-sw-sw, hh);
							
							
//							// show at the top of the chart
							
////							ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw, starty, ChartPanel.W-sw-sw, hh);
							
////							starty = starty + hh;
							
							
							
//							FillRectBoth(ThisRect,br[ChartBackgroundID].DxBrush);
//							FillRectBoth(ExpandRect(ThisRect,0,0,2,0),br[ChartBackgroundFadeID].DxBrush);
//							FillRectBoth(ThisRect, br[ThisBrushID].DxBrush);	
							
							
//							ThisBrushID = ChartTextID;
//							RenderTarget.DrawText(CellString, CellFormat, ThisRect, br[ThisBrushID].DxBrush);					
					
					
					
					
					
					
					
					
				}
				
				
//				if (StatusButtonsBrushDX != null)
//				StatusButtonsBrushDX.Dispose();
				
				
				firstmessage = true;
			
				if (!IsInHitTest)
				foreach (MessageAlerts D in AllMessages)
				{
					
					ThisMessage = D.Name;
					//Print(ThisMessage);
					
					
				
				
					if (IsKorean)
					{
						if (ThisMessage == TheWarningMessage)
							ThisMessage = "마스터 계정을 거래하는 동안 모든 서브 계정의 포지션 및 주문을 모니터링하는 것은 트레이더의 책임입니다. 여러 계정에 거래를 카피하는 과정은 진입/청산 방법, 기타 타사 소프트웨어 및 연결 상태 등 여러 요인에 따라 복잡할 수 있습니다. 항상 NinjaTrader 데스크톱 플랫폼 외부에서도 포지션과 주문을 관리할 준비가 되어 있어야 합니다.";
						
					}
				
					
					
					
					
						if (D.Switch)
						if (ThisMessage != string.Empty)
						{
						
							//ThisStroke = pOrderBothOutlineStroke;

							
							//Print("F 1");

							ThisBrushID = D.BrushE;
							br[ThisBrushID].DxBrush.Opacity = 0.3f;
							
			
							//Print("F 2");
							
							//CellFormat			= myProperties.LabelFont.ToDirectWriteTextFormat();	
							CellFormat			= TextFont3.ToDirectWriteTextFormat();	
							
												
		                	CellFormat = new SimpleFont(TextFont3.Family.ToString(), TextFont3.Size+pFontSizeTopRow3).ToDirectWriteTextFormat();		
							
		                    CellFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
		                    CellFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
		                    CellFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.Wrap;
						
							
							//Print(ThisMessage);
							
							float leftrightpad = 8;
							float leftspace = (float) 3;	
							leftspace = leftspace + leftrightspace;
							

							
							leftspace = pPixelsFromTop-1;
						
							//Print("F 3");
							
							
							float sw = 0;
//							sw = ThisStroke.Width;	
							
//							if (!pActiveOutlineEnabled)
//								sw = 0;
								
							
							//Print("F 3");
							
							
							float totalwww = ChartPanel.W-sw-sw-leftspace-leftrightpad-leftrightpad;//-leftrightspace-leftrightspace;
							
							
							if (rightoftable != 0)
								totalwww = Math.Min(totalwww, rightoftable - leftspace - leftrightpad - leftrightpad);
							
							//totalwww = rightoftable - leftspace - leftrightpad - leftrightpad;
							
							//Print("F 4a");
							
						
							CellString = ThisMessage;
							
							//Print(CellString);
							
							//CellFormat			= TextFont3.ToDirectWriteTextFormat();	
							
							//CellFormat			= TextFont3.ToDirectWriteTextFormat();	
							
							
				
				
							CellLayout = new TextLayout(Core.Globals.DirectWriteFactory, CellString, CellFormat, totalwww, 10000);
								 
							
							
							
							
							//Print("F 4b");
							
		                    float FinalH = CellLayout.Metrics.Height;
		                    float FinalW = CellLayout.Metrics.Width;
							
									//Print("F 4c");
							
							float hh = FinalH + 4; // vertical padding in message box
							hh = hh; 
							
							if (firstmessage)
								starty = starty - hh - ppppppp2; // add space between message boxes
							else
								starty = starty - hh - 6;
							
							firstmessage = false;
							
					
							
							//ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw, ChartPanel.H-sw-hh, ChartPanel.W-sw-sw, hh);
							ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw+leftspace+leftrightpad, starty, totalwww, hh);
							ThisRect = ExpandRect(ThisRect,leftrightpad,leftrightpad,0,0);

							//FillRectBoth(ThisRect, ChartBackgroundID);  //why?
							//FillRectBoth(ExpandRect(ThisRect,2,2,2,2), ChartBackgroundFadeID); //why?
							FillRectBoth(ThisRect, ThisBrushID);
							
							
							//Print("F 5");
							
							
							D.Rect = ThisRect;
							
							int sidetxtpad = 4;
							
							//ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw+leftspace+sidetxtpad, 0+sw+1, totalwww-sidetxtpad*2, hh);
							ThisRect = new SharpDX.RectangleF(ChartPanel.X+sw+leftspace+leftrightpad, starty, totalwww, hh);
							
							ThisBrushID = ChartTextID;
							
							
							if (MouseIn(D.Rect,0,0))
							{

								br[ThisBrushID].DxBrush.Opacity = 0.5f;
								
								
							}					
							
							
							
							
							RenderTarget.DrawText(CellString, CellFormat, ThisRect, br[ThisBrushID].DxBrush);	
							
							
							if (MouseIn(D.Rect,0,0))
							{

								br[ThisBrushID].DxBrush.Opacity = 1f;
								
								CellFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;
								//CellFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Far;
								
								string dismess = "Click To Dismiss";
								

								
								if (CellString.Contains("column for all accounts"))
									dismess = "Click To Cancel";
								
								if (CellString.Contains(MainResetConfirmation))
									dismess = "Click To Cancel";								
								
								
								if (CellString.Contains("'Account Risk Manager'"))
									dismess = "Click To Learn More";
								
								if (CellString.Contains("disable the standard Chart Trader"))
									dismess = "";
								
								
								if (IsKorean)
								{
									dismess = dismess.Replace("Click To Dismiss", "클릭하고 닫으시면 됩니다");
									dismess = dismess.Replace("Click To Learn More", "더 알아보기 클릭");
									dismess = dismess.Replace("Click To Cancel", "취소 클릭");
								}
								
								
								RenderTarget.DrawText(dismess, CellFormat, ThisRect, br[ThisBrushID].DxBrush);	
								
								
								
							}		
						}
						}
					}
					else
					{
						p("Drawings count: "+AllDrawings.Count);
						// draw everything	
						foreach(Drawings DD in AllDrawings)
						{
							br[DD.ThisB].DxBrush.Opacity = DD.ThisBOpacity;
							RenderTarget.FillRectangle(DD.Rect, br[DD.ThisB].DxBrush); // new
						}
					}
				AllowRedraw = true;

			FirstLoadAccountData = false;
			
				
			if (lastrightside == 0)
				ChartControl.InvalidateVisual();
			
			
//			bitmapRenderTarget.EndDraw();
		
//        cachedBitmap?.Dispose();
//       	cachedBitmap = bitmapRenderTarget.Bitmap;
			
		//#endregion	
			}catch(Exception e){
				Print($"{line}   {e.ToString()}");
			}
		}//end of OnRender 

		
		
		private string ShortIN (string sss)
		{
			string[] tokens = sss.Split(' ');
			return tokens[0];		
		}
      		
		private string FormatPriceMarker2(double price)
		{
			
			return Instrument.MasterInstrument.FormatPrice(price,true);
			
			
			// old code
			
			if (ChartControl == null)
			{
				
				return Math.Round(price,0).ToString();
				//Print("safs4");
			}
			else
			{
				//Print("safs1");
				return Instrument.MasterInstrument.FormatPrice(price,true);
				
				//Print("safs2");
				//Print("safs");
					
					
				double trunc = Math.Truncate(price);
				int fraction = Convert.ToInt32(320 * Math.Abs(price - trunc) - 0.0001); // rounding down for ZF and ZT
				string priceMarker = "";
				if (TickSize == 0.03125) 
				{
					fraction = fraction/10;
					if (fraction < 10)
						priceMarker = trunc.ToString() + "'0" + fraction.ToString();
					else 
						priceMarker = trunc.ToString() + "'" + fraction.ToString();
				}
				else if (TickSize == 0.015625 || TickSize == 0.0078125)
				{
					if (fraction < 10)
						priceMarker = trunc.ToString() + "'00" + fraction.ToString();
					else if (fraction < 100)
						priceMarker = trunc.ToString() + "'0" + fraction.ToString();
					else	//
						priceMarker = trunc.ToString() + "'" + fraction.ToString();
				}
				else
					priceMarker = price.ToString(PriceString);
				return priceMarker;			
			}
			
		}		
		
		
		
		
			
		private SharpDX.RectangleF MoveRect (SharpDX.RectangleF RR, float xe, float ye)
		{
			
			SharpDX.RectangleF FF = new SharpDX.RectangleF(RR.X+xe, RR.Y+ye, RR.Width, RR.Height);
				
			return FF;
			
		}
		
		private SharpDX.RectangleF ExpandRect (SharpDX.RectangleF RR, float left, float right, float top, float bottom)
		{
			
			SharpDX.RectangleF FF = new SharpDX.RectangleF(RR.X-left, RR.Y-top, RR.Width+left+right, RR.Height+top+bottom);
				
			return FF;
			
		}
		
		private SharpDX.RectangleF ExpandRect (SharpDX.RectangleF RR, float xe, float ye)
		{
			
			SharpDX.RectangleF FF = new SharpDX.RectangleF(RR.X-xe, RR.Y-ye, RR.Width+xe*2, RR.Height+ye*2);
				
			return FF;
			
		}	

		private SharpDX.RectangleF ResizeRect (SharpDX.RectangleF RR, float xe, float ye)
		{
			
			SharpDX.RectangleF FF = new SharpDX.RectangleF(RR.X, RR.Y, RR.Width+xe, RR.Height+ye);
				
			return FF;
			
		}			
	
		private string LastClickedAccount = string.Empty;
		private string LastClickedAutoLiquidateAccount = string.Empty;
		private string LastClickedDailyGoalAccount = string.Empty;
		private string LastClickedDailyLossAccount = string.Empty;
		private string LastClickedPayoutAccount = string.Empty;
		
		
		private string ResetNowAutoLiquidateAccount = string.Empty;
		private string ResetNowDailyGoalAccount = string.Empty;
		private string ResetNowDailyLossAccount = string.Empty;
		private string ResetNowPayoutAccount = string.Empty;
		
		private void DisableMasterAccount()
		{
		
				pThisMasterAccount = string.Empty;
				CurrentMasterAccount = null;
			
				pIsBuildMode = true;
				pCopierIsEnabled = false;
		}
		
		private void DisableFollowerAccounts()
		{
			AllDuplicateAccounts.Clear();
			
		
			foreach(string s in pAllAccountData)
			{

				if (s!= string.Empty)
				{
					string accountname = s.Split('|')[0];
					
					SetAccountData("",accountname,"None","","","","","","","");
				}	
							
				
				
			}
	
			
		}		
		
						
		private bool AccountHasBeenSaved(string name)
		{
			
			
		
			foreach(string s in pAllAccountData)
			{

				if (s!= string.Empty)
				{
					string accountname = s.Split('|')[0];
					
					if (	accountname == name)
						return true;
				}	
							
				
				
			}
			
			return false;
			
		}		
		
		
		
		
		
        internal void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			
//				private string ResetNowDailyGoalAccount = string.Empty;
//		private string ResetNowDailyLossAccount = string.Empty;
		
			
			if (LastClickedDailyGoalAccount != string.Empty)
			{
				
				ResetNowDailyGoalAccount = LastClickedDailyGoalAccount;
				LastClickedDailyGoalAccount = "";
				
			}
			
			if (LastClickedDailyLossAccount != string.Empty)
			{
				ResetNowDailyLossAccount = LastClickedDailyLossAccount;	
				LastClickedDailyLossAccount = "";
				
			}
			
			if (LastClickedPayoutAccount != string.Empty)
			{
				ResetNowPayoutAccount = LastClickedPayoutAccount;	
				LastClickedPayoutAccount = "";
				
			}			
				
			
			
			// reset mode column on double click
			
			if (SelectedATMAccount != string.Empty)
			{				
				SetAccountData("",SelectedATMAccount,"","","","","","","","Default");

			}	
			
			
			
			if (LastClickedAutoLiquidateAccount != string.Empty)
			
			{
				ResetNowAutoLiquidateAccount = LastClickedAutoLiquidateAccount;
				
				LastClickedAutoLiquidateAccount = string.Empty;
				
			}
			
			
			if (LastClickedAccount != string.Empty)
			
			{
				
				SelectedMultiplierAccount = string.Empty;
				SelectedATMAccount = string.Empty;
				
				
				if (LastClickedAccount == pThisMasterAccount)
				{
					DisableMasterAccount();
					//Print("SFSD");
				}
				else
				{
					
					RemoveMessage("You must set the");
					RemoveMessage(AllKorean);
					
					pThisMasterAccount = LastClickedAccount;
					AllDuplicateAccounts.Remove(pThisMasterAccount);
					
					//Print("AllDuplicateAccounts.Remove(a");
					
					
					AllCrossAccounts.Remove(pThisMasterAccount);
					SetAccountData("",pThisMasterAccount,"None","1","No","No","No","","","");
				}
				
				
				 
				
				
				//Print(pThisMasterAccount);
			
            	UpdateMasterAccount(pThisMasterAccount);
				
//				if (CurrentMasterAccount != null)
//				if (!IsAccountFlat(CurrentMasterAccount))
//				{
//					DisableMasterAccount();
//				}
				
				// necessary to immediately recalc
				GetAllPerformanceStats();
				
				
			}
			
			ChartControl.InvalidateVisual();
		}
		
		
		
		[XmlIgnore] public Account CurrentMasterAccount = null;
		
		private Account PreviousMasterAccount = null;
		
	
		private void FlattenInstrument(Account aaaaa, Instrument iiii)
		{
			
			
				
			List<Instrument> AllInstruments = new List<Instrument>();
		
		
			foreach (Position P in aaaaa.Positions)
			{
				if (P.MarketPosition != MarketPosition.Flat) // added so that command isn't submitted for flat positions, and therefor in the log
				{
					if (P.Instrument == iiii)
					{
						AllInstruments.Add(P.Instrument);
						//FlattenEverythingClickedTotal++;
					}
				}
			}
			
			if (AllInstruments.Count > 0)
				aaaaa.Flatten(AllInstruments);
			
		}
		
		
		private void FlattenAllStrategies(Account aaaaa)
		{
			
			
				
			List<Instrument> AllInstruments = new List<Instrument>();
		
					
			foreach (object aaa in aaaaa.Strategies.ToList())
			{								

				//aaa.CloseStrategy(null);
		
				//Print(aaa.GetType().ToString());
				
				//Strategy ssss = null;
				
				//if (aaa.GetType() == typeof(Strategy))
			//	{
					StrategyBase ssss = (StrategyBase) aaa;
					
//					if (ssss != null)
//					Print("NN " + ssss.Name);
					
			//	}
				
				if (ssss != null)
				{
				
//					Print(ssss.Name);
//					Print(ssss.Instrument);
					
					//ssss.CloseStrategy(null);
					
					AllInstruments.Add(ssss.Instrument);
					FlattenEverythingClickedTotal++;
				}
				
				
			}		
			
			
			
			
			if (AllInstruments.Count > 0)
				aaaaa.Flatten(AllInstruments);
			
		}
		
		
		
		
		
		private void FlattenEverything(Account aaaaa)
		{
			
			
				
			List<Instrument> AllInstruments = new List<Instrument>();
		
		
			foreach (Position P in aaaaa.Positions)
			{
				if (P.MarketPosition != MarketPosition.Flat) // added so that command isn't submitted for flat positions, and therefor in the log
				{
					AllInstruments.Add(P.Instrument);
					FlattenEverythingClickedTotal++;
				}
			}
			
			if (AllInstruments.Count > 0)
				aaaaa.Flatten(AllInstruments);
			
		}
		
		
		
		
		
		
		private Account AccountFromName(string name)
		{
			Account aaaa = null;
			
			
				foreach (Account acct in Account.All)
				{
					
					if (acct.ConnectionStatus == ConnectionStatus.Connected)
					if (acct.Name == name)
					{
						
						aaaa = acct;
					
						
					}
		
				}
				
			return aaaa;
		}
		
		
		private void UpdateMasterAccount(string inputaccount)     
		{
			
			if (inputaccount == string.Empty)
			{
				//CurrentMasterAccount = null;
				return;
				
			}
			
			Account aaaa = null;
			
				
		
				foreach (Account acct in Account.All)
				{
					
					string accccnnnn = acct.Name;
					
					if (AllHideAccounts.Contains(accccnnnn))
						continue;
					
					if (accccnnnn == "Playback101")
						continue;
					
					if (accccnnnn == "Backtest")
						continue;			
					

					
					if (acct.Name == inputaccount)
					if (acct.ConnectionStatus == ConnectionStatus.Connected) // added to avoid connected error
					{
						
					
						
//						Print(inputaccount + "  " + acct.Name);
						
//						double addaa =  acct.Get(AccountItem.CashValue, Currency.UsDollar);
						
//						Print(addaa);
						
						aaaa = acct;
						
					
						
					}
					
					
					
				}
               				
				if (aaaa != null)
				{
					
					
					//Print("adding events");
					
					
//					Print("aaaa.MaxOrderSize " + aaaa.MaxOrderSize);
//					Print("aaaa.MaxPositionSize " + aaaa.MaxPositionSize);
			
					
					
					
					CurrentMasterAccount = aaaa;

					if (PreviousMasterAccount != null)
					{
						PreviousMasterAccount.AccountItemUpdate -= OnAccountItemUpdate3;
						PreviousMasterAccount.ExecutionUpdate -= OnExecutionUpdate3;
						PreviousMasterAccount.OrderUpdate -= OnOrderUpdate3;
						PreviousMasterAccount.PositionUpdate -= OnPositionUpdate3;
					}

								
					
					// Subscribe to new account subscriptions
					//aaaa.AccountStatusUpdate -= OnAccountStatusUpdate3;				
					CurrentMasterAccount.AccountItemUpdate   += OnAccountItemUpdate3;
					CurrentMasterAccount.ExecutionUpdate     += OnExecutionUpdate3;
					CurrentMasterAccount.OrderUpdate         += OnOrderUpdate3;
					CurrentMasterAccount.PositionUpdate      += OnPositionUpdate3;
					
					PreviousMasterAccount = CurrentMasterAccount;			
					
				}
				else
				{
					
					CurrentMasterAccount = null;
					if (PreviousMasterAccount != null)
					{
						PreviousMasterAccount.AccountItemUpdate -= OnAccountItemUpdate3;
						PreviousMasterAccount.ExecutionUpdate -= OnExecutionUpdate3;
						PreviousMasterAccount.OrderUpdate -= OnOrderUpdate3;
						PreviousMasterAccount.PositionUpdate -= OnPositionUpdate3;
						
					}					
					
				}

		
			
			
		}
		
		
		private string NonCompatibleInstrument = string.Empty;
	
		private Instrument GetTheInstrument (string thisexinstr, string miniormicro, bool showerror)
		{
			
			//Print(thisexinstr);
			
	
			string miniinstrument = string.Empty;
			string microinstrument = string.Empty;
			bool IsMicro = false;
			
		
						
			if (thisexinstr.Contains("FDAX"))
			{
				
				//microinstrument = thisexinstr.Replace("FDAX", "FDXS");
				//microinstrument = thisexinstr.Replace("FDAX", "FDXM");
				
				

				
 				//miniinstrument = thisexinstr;
				
				miniinstrument = thisexinstr.Replace("FDAX", pFDAXMini);
				microinstrument = thisexinstr.Replace("FDAX", pFDAXMicro);				
				
				
				
			}	
			else if (thisexinstr.Contains("FDXM") || thisexinstr.Contains("FDXS"))
			{
				microinstrument = thisexinstr.Replace("FDXM", "FDXS");
 				miniinstrument = thisexinstr.Replace("FDXS", "FDXM");	
				
				
			}				
			else if (thisexinstr.Contains("FESX") || thisexinstr.Contains("FSXE"))
			{
				microinstrument = thisexinstr.Replace("FESX", "FSXE");
 				miniinstrument = thisexinstr.Replace("FSXE", "FESX");	
				
				
			}				
			else if (thisexinstr.Contains("RTY") || thisexinstr.Contains("M2K"))
			{
				microinstrument = thisexinstr.Replace("RTY", "M2K");
 				miniinstrument = thisexinstr.Replace("M2K", "RTY");	
				
				
			}			
			else if (thisexinstr.Contains("ES") || thisexinstr.Contains("MES"))
			{				
				microinstrument = thisexinstr.Replace("ES", "MES");
				microinstrument = microinstrument.Replace("MMES", "MES");
				
 				miniinstrument = thisexinstr.Replace("MES", "ES");	
				
				
			}	
			else if (thisexinstr.Contains("YM") || thisexinstr.Contains("MYM"))
			{				
				microinstrument = thisexinstr.Replace("YM", "MYM");
				microinstrument = microinstrument.Replace("MMYM", "MYM");
				
 				miniinstrument = thisexinstr.Replace("MYM", "YM");	
				
				
			}	
			else if (thisexinstr.Contains("NQ") || thisexinstr.Contains("MNQ"))
			{				
				microinstrument = thisexinstr.Replace("NQ", "MNQ");
				microinstrument = microinstrument.Replace("MMNQ", "MNQ");
				
 				miniinstrument = thisexinstr.Replace("MNQ", "NQ");	
				
				
			}				
			else if (thisexinstr.Contains("CL") || thisexinstr.Contains("MCL"))
			{				
				microinstrument = thisexinstr.Replace("CL", "MCL");
				microinstrument = microinstrument.Replace("MMCL", "MCL");
				
 				miniinstrument = thisexinstr.Replace("MCL", "CL");	
				
				
			}				
			else if (thisexinstr.Contains("GC") || thisexinstr.Contains("MGC"))
			{
				microinstrument = thisexinstr.Replace("GC", "MGC");
				microinstrument = microinstrument.Replace("MMGC", "MGC");
				
 				miniinstrument = thisexinstr.Replace("MGC", "GC");	
				
				
			}				
			else if (thisexinstr.Contains("SI") || thisexinstr.Contains("SIL"))
			{
				microinstrument = thisexinstr.Replace("SI", "SIL");
				microinstrument = microinstrument.Replace("SILL", "SIL");
				
 				miniinstrument = thisexinstr.Replace("SIL", "SI");	
				
				
			}	
			else if (thisexinstr.Contains("6A"))
			{
				microinstrument = thisexinstr.Replace("6A", "M6A");
				microinstrument = microinstrument.Replace("MM", "M");
				
 				miniinstrument = thisexinstr.Replace("M6A", "6A");		

				
			}
			else if (thisexinstr.Contains("6B"))
			{
				microinstrument = thisexinstr.Replace("6B", "M6B");
				microinstrument = microinstrument.Replace("MM", "M");
				
 				miniinstrument = thisexinstr.Replace("M6B", "6B");		

				
			}		
			else if (thisexinstr.Contains("6E"))
			{
				microinstrument = thisexinstr.Replace("6E", "M6E");
				microinstrument = microinstrument.Replace("MM", "M");
				
 				miniinstrument = thisexinstr.Replace("M6E", "6E");		

				
			}				
			else
			{

				
				
			//	Cross Order Replikanto Instruments:

			//	Pre defined: ES ↔ MES, NQ ↔ MNQ, YM ↔ MYM, 6A ↔ M6A, 6B ↔ M6B, 6E ↔ M6E, 6J ↔ M6J, 6S ↔ M6S, GC ↔ MGC, ICD ↔ MICD, IJY ↔ MIJY, ISF ↔ MISF, NF ↔ MNF, RTY ↔ M2K, CL ↔ MCL, NG ↔ QG, RB ↔ QU, HO ↔ QH, ZS ↔ XK, ZC ↔ XC, ZW ↔ XW, FDXA ↔ FDXM, HG ↔ MHG…
				
				
				if (pIsCrossEnabled && showerror)
				{
					NonCompatibleInstrument = thisexinstr;
				
				}
				
				
//				if (thisexinstr.Contains("M"))
//				{
//					if (!thisexinstr.Contains("YM"))
//					{				
//						IsMicro = true;
//					}
//					else
//					{
//						if (thisexinstr.Contains("MYM"))
//							IsMicro = true;	
//					}			
//				}	
				
				
//				if (IsMicro)
//				{
//						microinstrument = thisexinstr;										
//						miniinstrument = thisexinstr.Substring(1); // remove first character
					
//				}
//				else
//				{
//						microinstrument = "M"+ thisexinstr; // add M				
//						miniinstrument = thisexinstr;				
					
//				}
						
				return Instrument.GetInstrument(thisexinstr);
				
			}
			
			
			//Print(microinstrument);
			
		
			
			

			Instrument MicroInstrument = Instrument.GetInstrument(microinstrument);
 			Instrument MiniInstrument = Instrument.GetInstrument(miniinstrument);			
			
			
			if (miniormicro == "Mini")
				return MiniInstrument;	
			else
				return MicroInstrument;	
			
			
			
		}
	
	
		
		
		
		bool IgnoreExecutionUntilFlat = false;
		bool NextCloseOrderDisableFollower = false;
		
		private void OnPositionUpdate3 (object sender, PositionEventArgs e)
	    {	
		
			
			
			//Print("OnPositionUpdate3 " + IgnoreExecutionUntilFlat);
			
			// this did not work
			
//			if (e.MarketPosition == MarketPosition.Flat)
//			{
//				IgnoreExecutionUntilFlat = false;
				
				//Print(e.MarketPosition + "  " + IgnoreExecutionUntilFlat);
//			}
			
		}
		

		private int pMilliSFE = 5000;
		
		
		private string messageoff = "You are placing trades on the Master account, while Duplicate Account Actions is disabled.  Click the button in the top left corner of this window, to begin duplicating trades across Follower accounts.  It will say 'ON' to indicate that it is ready.";
		
	
		
				private bool OrderInstrumentOK, OrderStateOK, OrderTypeOK, OrderNameOK = false;
		private double NewStopPrice, NewLimitPrice = 0;
		
		
		private double RTTS2(Instrument iii, double p)
        {
            return iii.MasterInstrument.RoundToTickSize(p);
			
			//return p;
        }


		private static Random random = new Random();
		
		public static string RandomString(int length)
		{
			//const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		    const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
		    return new string(Enumerable.Repeat(chars, length)
		        .Select(s => s[random.Next(s.Length)]).ToArray());
		}	

		private SortedDictionary<string, int> TotalAPI = new SortedDictionary<string, int>();
		

		// https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?connection_class.htm
		
		
		
    private Connection Connect(string connectionName)
    {
        // Output the execution
        try
        {
              // Get the configured account connection
              ConnectOptions connectOptions = null;
              lock (Core.Globals.ConnectOptions)
                  connectOptions = Core.Globals.ConnectOptions.FirstOrDefault(o => o.Name == connectionName);
 
              if (connectOptions == null)
              {
                  NinjaTrader.Code.Output.Process("Could not connect. No connection found.", PrintTo.OutputTab1);
                  return null;
              }
 
              // If connection is not already connected, connect.
              lock (Connection.Connections)
                  if (Connection.Connections.FirstOrDefault(c => c.Options.Name == connectionName) == null)
                  {
                        Connection connect = Connection.Connect(connectOptions);
 
                        // Only return connection if successfully connected
                        if (connect.Status == ConnectionStatus.Connected)
                            return connect;
                        else
                            return null;
                  }
 
              return null;
        }
        catch (Exception error)
        {
              NinjaTrader.Code.Output.Process("Connect exception: " + error.ToString(), PrintTo.OutputTab1);
              return null;
        }
    }
	
	
	
		
		
		double OldStopLossPrice = 0;		
		bool SkipNextChangeSubmittedCommand = false;
		
		bool LongExitOK = false;
		bool ShortExitOK = false;
		
		string LastAddATMName = string.Empty;
		
		//List<List<string>> AddOnATMAdjustExitOrders = new List<List<string>>();
		
			
		//private SortedDictionary<string, List<string>> AddOnATMAdjustExitOrders = new SortedDictionary<string, List<string>>();
		
		private SortedDictionary<string, SortedList<string, List<string>>> AddOnATMAdjustExitOrders = new SortedDictionary<string, SortedList<string, List<string>>>();
		
		
		
		bool SetLastAddATMName = false;
		
		
		private void OnOrderUpdate3 (object sender, OrderEventArgs  e)
		{
			
			
		}
		
		
		
		private DateTime LastMarketEntry = DateTime.MinValue;
		
		bool CloseHappened = false;
		
		//Order or = null;
						
							
		Order NewOrder = null;
		
		
				
		object ATMObject = 	null;
		AtmStrategy NewATM = null;
		
		int MasterExQty = 0;
		
		AtmStrategy LastNewOrderATM = null;
		
		private bool IsAccountFlatAndNoPendingOrders(string sssss, string colname)
		{
			
	
						if (FinalPNLStatistics.ContainsKey(sssss))
						{
							PNLStatistics Z = FinalPNLStatistics[sssss];
						
							
							//Print(sssss);
							
								if (Z.AllPositions.Count == 0 && Z.PendingEntry == 0)
								{
									
									return true;
								}
								else
								{
									RemoveMessage("You can't update the '");
									AddMessage("You can't update the '" + colname + "' Adjustment column for '" + sssss + "' while there is an active position or pending entry orders.", 10, RED);
									
									
									return false;
									
								}
								
								
							
							
						}
						
						
						return true;
						
		}
		
		
		private bool IsAccountFinishedAndFlat(string sssss)
		{
			
	
				if (pIsRiskFunctionsChecked)
						if (FinalPNLStatistics.ContainsKey(sssss))
						{
							PNLStatistics Z = FinalPNLStatistics[sssss];
						
							if (Z.HitGoalOrLoss != 0)
							{
							
								if (Z.AllPositions.Count == 0)
								{
									
									return true;
								}								
								
								
							}
							
						}
						
						
						return false;
						
		}
		
				
//		private bool IsAccountFinishedAndFlat(string sssss)
//		{
			
//			bool IsFundedOrAtDailyGoalLoss = false;
			
//				if (pIsRiskFunctionsChecked)
//						if (FinalPNLStatistics.ContainsKey(sssss))
//						{
//							PNLStatistics Z = FinalPNLStatistics[sssss];
						
//							if (pDisableFundedFollowers)
//							{
							
//								double overfunded = Z.FromFunded - pDollarsExceedFunded;
								
//								if (overfunded <= 0)
//								{
								
//									// disable new orders from being placed when flat
									
//									IsFundedOrAtDailyGoalLoss = true;
//								}
							
//							}
							
//							if (pDisableGoalFollowers)
//							{
								
//								if (pColumnDailyGoal || pColumnDailyLoss)
//								{
									
//								//	Print("GetSavedAccountExitSwitch(acct.Name) " + GetSavedAccountExitSwitch(acct.Name) + " " + acct.Name);
									
									
//									double CurrentDailyGoal = 0;
//									double CurrentDailyLoss = 0;
									
									
											 
//									if (pColumnDailyGoal)
//									{							
//										CurrentDailyGoal = GetAccountSavedData(sssss, pAllAccountDailyGoal);	

//										if (CurrentDailyGoal != -1000000000)
//										{
//											if (Z.PNLTotal > CurrentDailyGoal - pDailyGoalLossBuffer)
//											{
//												IsFundedOrAtDailyGoalLoss = true;
//											}
//										}
//									}
											
//									if (pColumnDailyLoss)
//									{										
//										CurrentDailyLoss = GetAccountSavedData(sssss, pAllAccountDailyLoss);
		
//										if (CurrentDailyLoss != -1000000000)
//										{
//											if (Z.PNLTotal < CurrentDailyLoss + pDailyGoalLossBuffer)	
//											{		
//												IsFundedOrAtDailyGoalLoss = true;
//											}
//										}
//									}
//								}
										
//							}
						
							
							
							
//							//Print(sssss + "   " + overfunded + "  " + kvppp.AllPositions.Count);
							
							
//							if (IsFundedOrAtDailyGoalLoss)
//							if (Z.AllPositions.Count == 0)
//							{
								
//								return true;
//							}
								
					
//						}
						
						
//						return false;
						
//		}
		
		
	 	private void AddRejectedEvent(string acccccct, string pRejectedSubmit, string iddddd)
		{
			
			
						
													string pan = PrivateAccountName(acccccct);
													
													if (!RejectedAccounts.ContainsKey(pan))
													{
														PendingDetails ZS = new PendingDetails();
						
												
														ZS = new PendingDetails();
														ZS.LatestAction = pRejectedSubmit;
														ZS.StartTime = DateTime.Now;
														ZS.AllOrderNames.Add(iddddd);
		
														
														RejectedAccounts.Add(pan, ZS);
													}		
													else
													{
													
														
														RejectedAccounts[pan].LatestAction = pRejectedSubmit;
														RejectedAccounts[pan].StartTime = DateTime.Now;
														RejectedAccounts[pan].AllOrderNames.Add(iddddd);
														
													}
													
			
			
			
		}
													
													
										
		private AtmStrategy ATMStrategyForFollower(Account aaaaaaa, AtmStrategy SlaveA)
		{
			
		
										if (SlaveA.Brackets.Length > 1)
										{
											
											double SlaveExMultiplier = Convert.ToDouble(GetAccountMultiplier(aaaaaaa.Name));
						
											if (SlaveExMultiplier == 0)
												SlaveExMultiplier = 0.50;
											if (SlaveExMultiplier == -1)
												SlaveExMultiplier = 0.33;
											if (SlaveExMultiplier == -2)
												SlaveExMultiplier = 0.25;
											if (SlaveExMultiplier == -3)
												SlaveExMultiplier = 0.20;	
						
											int SlaveExQty = 0;
											
											if (pMultiplierMode == "Multiplier")
												SlaveExQty = (int) Math.Round(MasterExQty * SlaveExMultiplier);
											else
												SlaveExQty = (int) SlaveExMultiplier;
											
											SlaveExQty = Math.Max(SlaveExQty, 1);
							
							
											
													double ratio = SlaveExMultiplier / MasterExQty;
															
											
													if (pMultiplierMode == "Multiplier")
													{
														ratio = SlaveExMultiplier;
														
													}
													
													if (ratio != 1)
													{
														
														
														//Print(ratio);
														
																											
														
	//													foreach( Bracket bb in SlaveA.Brackets)
	//														totalqqqqq = totalqqqqq + bb.Quantity;
														
														int newq = 0;
														
														int remainingq = SlaveExQty;
														foreach( Bracket bb in SlaveA.Brackets)
														{			
																if (bb.Quantity != 0 && bb.Quantity != bb.Quantity*ratio)
																{	
																	newq = (int) Math.Round(bb.Quantity*ratio);
																	newq = Math.Max(newq,1);
																	newq = Math.Min(newq,remainingq);
																	
																	//Print(newq);
																	
																	bb.Quantity = newq;
																	
																	remainingq = remainingq - newq; 
																}
														}	
														
													}
													
													
//														//	if (SlaveExMultiplier >= 1)
//													foreach( Bracket bb in SlaveA.Brackets)
//													{			
														
//															double changed = bb.Quantity*SlaveExMultiplier;
//															bool isInt = changed == (int)changed;
															
//														if (isInt)
//															if (bb.Quantity != changed)
//															{
													
//																bb.Quantity = (int) changed;
																
//															}	
															
															
////															if (bb.Quantity != 0)
////															if (bb.Quantity != bb.Quantity*SlaveExMultiplier)
////															{														
////																bb.Quantity = bb.Quantity*(int)SlaveExMultiplier;
////															}
//													}	
		
										
										}
			
			
			return SlaveA;
			
			
		}
													
	
		
		private Order PreviousChangePendingOrder = null;
		private double PreviousChangePrice = 0;
		
		private double MovePrice = 0;
		private double NewPrice = 0;
		
		private int SuspendedCount = 0;
		
		private Order TradingViewExitOrder1 = null;
		private Order TradingViewExitOrder2 = null;
		
		private int IgnoreCommands = 0;

		
		
		
		private void OnExecutionUpdate3 (object sender, ExecutionEventArgs e)
		{
	
		
			
			
		}
		
	
	
		 private void OnAccountItemUpdate3 (object sender, AccountItemEventArgs e)
	    {
//			Print(e.Account.Name);
//			Print(e.AccountItem.ToString());
//			Print(e.Value);
			
			//Print("OnAccountItemUpdate3");

	    }
	
		private void OnAccountStatusUpdate3 (object sender, AccountStatusEventArgs e)
		{
			//	Print("OnAccountStatusUpdate3");
		}
		 
 
	
		
				
		
		
		private bool IsAccountFlat(Account thisacct)
		{
			if (thisacct == null)
				return true;
			
			if (pAllInstruments)
			{
				if (thisacct.Positions.Count != 0)
					return false;
				
			}
			else
			{
				
				string iname = Instrument.FullName;
				

				
				
				if (thisacct.GetPosition(GetTheInstrument(iname, "Micro", false).Id) != null)
					return false;
				
				if (thisacct.GetPosition(GetTheInstrument(iname, "Mini", false).Id) != null)
					return false;					
				
										
			}
			
		
			return true;
		}
		
		
		
		
		private bool IsMasterAccountFlat()
		{
			if (CurrentMasterAccount == null)
				return true;
			
			if (pAllInstruments)
			{
				if (CurrentMasterAccount.Positions.Count == 0)
					return true;
				
			}
			else
			{
				if (CurrentMasterAccount.GetPosition(Instrument.Id) == null)
				//if (CurrentMasterAccount.GetPosition(Instrument.Id).MarketPosition == MarketPosition.Flat)
					return true;
				
			}
			
		
			return false;
		}
	
				
		private bool AccountsAreConfigured()
		{
		
			if (pThisMasterAccount != string.Empty && AllDuplicateAccounts.Count > 0)
			{
				UpdateMasterAccount(pThisMasterAccount);
				
				
				
				
				if (CurrentMasterAccount != null)
				{
					bool IsConnectedOK = pAllowDisconnectedAccounts || CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected;
					
					//Print("IsConnectedOK" + "  " +IsConnectedOK);
					
					if (IsConnectedOK)
					// if (IsConnectedOK && CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected)
					{
						return true;
					}
					else
					{
//						string enddddd = "enable Duplicate Account Actions";
//						AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
						
					}
					
					//Print("CurrentMasterAccount.ConnectionStatus" + "  " +CurrentMasterAccount.ConnectionStatus);
					
				}
				// master account not connected, error message
			}
			
			return false;
			
		}		
		
				
		private void ResetAllTimers()
		{
		
			SelectedResetColumn = string.Empty;
			SelectedResetColumnPre = string.Empty;
			SelectedResetTime = DateTime.MinValue;
			
			SelectedMultiplierAccount = string.Empty;
			SelectedMultiplerTime = DateTime.MinValue;
			
			SelectedATMAccount = string.Empty;
			SelectedATMTime = DateTime.MinValue;
			
			SelectedDailyLossAccount = string.Empty;
			SelectedDailyLossTime = DateTime.MinValue;

			SelectedDailyGoalAccount = string.Empty;
			SelectedDailyGoalTime = DateTime.MinValue;
			
			SelectedPayoutAccount = string.Empty;
			SelectedPayoutTime = DateTime.MinValue;	
						
			SelectedButtonNow = string.Empty;
			SelectedButtonNowPre = string.Empty;
			SelectedButtonTime = DateTime.MinValue;	
			
			IgnoreNextTime = DateTime.MinValue;	
			
		}
		
        internal void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
			// IsDragging = false;
			
		}
		
		
		
		
        internal void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
			//Print("MouseDown");
			
			

			try
			{
				
				if (e.ChangedButton == MouseButton.Right)
				{
	
					
					return;
				}
						
			}
			catch (Exception ex)
			{
			
				
				
			}
	
		
			
		
			//ResetAllTimers();
			
			
			
            this.MP = e.GetPosition(this.ChartPanel);

 
			FinalXPixel = MP.X / 100 * dpiX;
			FinalYPixel = MP.Y / 100 * dpiY;
 
			
			if (AllErrorMessages.Count > 0)
			{
				
				foreach(string s in AllErrorMessages)
				{
					if (s.Contains("You haven't purchased"))
						System.Diagnostics.Process.Start("https://affordableindicators.com/ninjatrader/indicators/accounts-dashboard-suite/");
					else if (s.Contains("'Duplicate Account Actions'"))
						System.Diagnostics.Process.Start("https://affordableindicators.com/ninjatrader/indicators/duplicate-account-actions/");
					else if (s.Contains("'Account Risk Manager'"))
						System.Diagnostics.Process.Start("https://affordableindicators.com/ninjatrader/indicators/account-risk-manager/");
				}
				
				
							
				
				AllErrorMessages.Clear();
				ChartControl.InvalidateVisual();
				return;
				
			}
			
			
			
			//return;
			
           // IsDragging = false;

		
					
			//List<string> DeleteThis = new List<string>();	
				
				string doneeee = string.Empty;
				string url = string.Empty;
				
				foreach (MessageAlerts D in AllMessages.ToList())
				{
					if (MouseIn(D.Rect,0,0))// || MouseIn(thisbutton.Value.Rect,exmbl,exmbl))
						{
							
							if (D.Switch)
							{
//								if (D.Name.Contains("'Essential Chart Trader Tools'"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/essential-chart-trader-tools/";
//								else if (D.Name.Contains("'Close Bar Entry Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/close-bar-entry-orders/";
//								else if (D.Name.Contains("'Price Based Entry Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/price-based-entry-orders/";
//								else if (D.Name.Contains("'Bracket Entry Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/bracket-entry-orders/";								
//								else if (D.Name.Contains("'Iceberg Entry Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/iceberg-entry-orders/";									
//								else if (D.Name.Contains("'Time Sliced Entry Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/time-sliced-entry-orders/";
//								else if (D.Name.Contains("'Signal Entry Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/signal-entry-orders/";								
//								else if (D.Name.Contains("'Advanced Exit Orders Management' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/advanced-stop-loss-management/";	
//								else if (D.Name.Contains("'Order Flow Entry Orders' is available as") && !D.Name.Contains("Volumetric"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/order-flow-entry-orders/";								
//								else if (D.Name.Contains("'Limit If Touched Orders' is available as"))
//									url = "https://affordableindicators.com/ninjatrader/enhanced-chart-trader/limit-if-touched-orders/";									
								
								
								 
								if (D.Name.Contains("'Account Risk Manager'"))
									System.Diagnostics.Process.Start("https://affordableindicators.com/ninjatrader/indicators/account-risk-manager/");								
									
								if (D.Name.Contains("column for all accounts"))
								{
									SelectedResetColumn = string.Empty;
									SelectedResetColumnPre = string.Empty;
									SelectedResetTime = DateTime.MinValue;
				
									
								}
								
								if (D.Name.Contains(MainResetConfirmation))
								{
									SelectedButtonNow = string.Empty;
									SelectedButtonNowPre = string.Empty;
									SelectedButtonTime = DateTime.MinValue;	
				
									//Print("		SelectedButtonNow");	
									
									
								}
								
								doneeee = D.Name;
							
								//Print(doneeee);
								
							}
							
						}
					
				}
				
				if (url != string.Empty)
				{
					
					System.Diagnostics.Process.Start(url);
					
				}
				
				string MessageChartTrader = "sadf";
				
					if (doneeee != string.Empty)
					{
						if (doneeee != MessageChartTrader) // no not allow this to be deleted ever
						RemoveMessage(doneeee);	
					
						
						ChartControl.InvalidateVisual();
					
						return;
					}
					
					
			
			
			// row titles
			
			double deleteeee = -1;
					
					
					
			foreach (KeyValuePair<double, PendingDetails> kvp in LimitPriceToTimeOutOfSync2)
			{
					
				bool hoverednew = MouseIn(kvp.Value.RectCancel, 0, 0);	
				
				double price = kvp.Key;
				
				if (hoverednew)
				{
					CancelOrdersAtPrice(price);
					
					deleteeee = price;
					
					//Print("RectCancel");
					continue;	
				}
					
				hoverednew = MouseIn(kvp.Value.RectFill, 0, 0);	
					
				if (hoverednew)
				{
					
					//FillOrdersAtPrice(price);
					
					//Print("RectFill");
					
					continue;
				}
				
				hoverednew = MouseIn(kvp.Value.RectPlus, 0, 0);	
					
				if (hoverednew)
				{
					
						
							
							
							
							
							
					//MoveOrdersAtPrice(price);
					
					//Print("RectPlus");
					continue;	
				}			
					
			}
			
			if (deleteeee != -1)
			{
				LimitPriceToTimeOutOfSync2.Remove(deleteeee);
				return;
			}
			
			
			// RIGHT chart buttons
								
			LastClickedAccount = string.Empty;
			LastClickedAutoLiquidateAccount = string.Empty;
			LastClickedDailyGoalAccount = string.Empty;
			LastClickedDailyLossAccount = string.Empty;
			LastClickedPayoutAccount = string.Empty;
				
			 
		
			
            foreach (KeyValuePair<double, ButtonBOT> thisbutton in AllButtonZ7)
            {
				
				bool hoverednew = MouseIn(thisbutton.Value.Rect, 0, 0);
				
				PNLStatistics BC = thisbutton.Value.Text;
				
				if (hoverednew)
				{
					//Print("------------------------------------------------------------ CLICKED " + BC.Instrument + "   " + BC.Name);
				
					
				//	Print(thisbutton.Value.Name);
					
					string columns = thisbutton.Value.Name;
					
		
					
					
						if (columns.Contains("Reset "))
						{
							
							string coltoreset = columns.Replace("Reset ", "");
						
//							Print("Reset");						
//							Print(coltoreset);
							
							
							
							
							if (SelectedResetColumn == string.Empty)
							{
								
								ResetAllTimers();
								
								SelectedResetColumn = coltoreset;
								SelectedResetTime = DateTime.Now.AddSeconds(20);
								
								SelectedResetColumnPre = SelectedResetColumn;

							}
							else
							{
								if (SelectedResetColumn == coltoreset)
								{
									// reset confirmed, do it
									if (SelectedResetColumn == "Auto Exit")
									{
							
										if (LastAutoExitReset == "No")
											LastAutoExitReset = "Yes";
										else
											LastAutoExitReset = "No";
										
													
										foreach (KeyValuePair<double, ButtonBOT> thisbutton2 in AllButtonZ5)
							            {
											
											//bool hoverednew = true;
											
											if (thisbutton2.Value.Text == null)
												continue;
											
											
											PNLStatistics BC2 = thisbutton2.Value.Text;
											
											string function = thisbutton2.Value.Name;
											string clickedaccount = "";
											
										
												if (BC2.Acct != null)
													clickedaccount = BC2.Acct.Name;
												
											
									
												if (function == "Auto Exit")
												{
												
												
														

														
													
				
													SetAccountData("7",clickedaccount,"","","","","", LastAutoExitReset,"","");
																	
												
														
														
												}		
												
											
												
												
										}			
										
										
									}
									if (SelectedResetColumn == "Auto Close")
									{
				
												
										if (LastAutoCloseReset == "No")
											LastAutoCloseReset = "Yes";
										else
											LastAutoCloseReset = "No";
											
										
													
													
													
										foreach (KeyValuePair<double, ButtonBOT> thisbutton2 in AllButtonZ5)
							            {
											
											//bool hoverednew = true;
											
											if (thisbutton2.Value.Text == null)
												continue;
											
											
											PNLStatistics BC2 = thisbutton2.Value.Text;
											
											string function = thisbutton2.Value.Name;
											string clickedaccount = "";
											
										
												if (BC2.Acct != null)
													clickedaccount = BC2.Acct.Name;
												
											
												
												
												if (function == "Auto Close")
												{
												
								
												
													SetAccountData("",clickedaccount,"","","","","","",LastAutoCloseReset,"");
													
														
														
												}						
											
												
												
										}
										
										
									}									
									
									if (SelectedResetColumn == "Size")
									{
										SetAccountData("","ALL","","1","","","","","","");
										
									}
									if (SelectedResetColumn == "Mode")
									{
										SetAccountData("","ALL","","","","","","","","Default");
										
									}									
									if (SelectedResetColumn == "Type")
									{
										AllCrossAccounts.Clear();
										SetAccountData("","ALL","","","No","","","","","");
										
									}
									if (SelectedResetColumn == "Fade")
									{
										SetAccountData("","ALL","","","","No","","","","");
										
									}									
									if (SelectedResetColumn == "Daily Loss")
									{
										foreach(string s in pAllAccountData)
										if (s!= string.Empty)
										{
											string accountname = s.Split('|')[0];
											RemoveAccountSavedData(accountname, pAllAccountDailyLoss);
										}	
										
									}									
									if (SelectedResetColumn == "Daily Goal")
									{
										foreach(string s in pAllAccountData)
										if (s!= string.Empty)
										{
											string accountname = s.Split('|')[0];
											RemoveAccountSavedData(accountname, pAllAccountDailyGoal);
										}	
										
									}							
									if (SelectedResetColumn == "Payouts")
									{
										foreach(string s in pAllAccountData)
										if (s!= string.Empty)
										{
											string accountname = s.Split('|')[0];
											RemoveAccountSavedData(accountname, pAllAccountPayouts);
										}	
										
									}							
									if (SelectedResetColumn == "Account")
									{
										if (AllDuplicateAccounts.Count == 0)
										{
											SelectAllAccounts = true;
											GetAllPerformanceStats();
											
										}
										else
										{
											AllDuplicateAccounts.Clear();
											SetAccountData("","ALL","None","","","","","","","");
											
											DisableMasterAccount();											
											
										}
										

																					
									}																		
	
									
									
									
									SelectedResetColumn = string.Empty;
									SelectedResetTime = DateTime.MinValue;
								
									
									
								}
								else
								{
									
									ResetAllTimers();
									
									SelectedResetColumn = coltoreset;
									SelectedResetTime = DateTime.Now.AddSeconds(10);
									
								
									
								}
								
								
								
							}
							
							
							ChartControl.InvalidateVisual();
		
							
							
							//pSelectedColumn = thisbutton.Value.Name;
						}
						
						else if (columns == "ForceToTop")
						{
							
							pForceAllSelectedAccountsToTop = !pForceAllSelectedAccountsToTop;
							
							
							//Print("ForceToTop");
						
							
							
						}
						
						else if (columns == "JUMP")
						{
							
							if (AdjustmentRows == 0)
							{
								AdjustmentRows = TotalAccounts - MaximumAccountRows;
								
								if (pShowFollowersAtTop)
									AdjustmentRows = TotalAccounts - MaximumAccountRows - AllDuplicateAccounts.Count;	
								
							}
							else
							{
								AdjustmentRows = 0;
							}
							
							//Print("AdjustmentRows  " + AdjustmentRows);
							
							
						}
						else if (columns == "Daily Goal")
						{
							
							if (pDailyGoalDisplayMode == "Update")
							{
								pDailyGoalDisplayMode = "Status";
							}
							else
							{
								pDailyGoalDisplayMode = "Update";
							}
							
						}
						else if (columns == "Daily Loss")
						{
							
							if (pDailyLossDisplayMode == "Update")
							{
								pDailyLossDisplayMode = "Status";
							}
							else
							{
								pDailyLossDisplayMode = "Update";
							}
							
						}						
						else
						{
							
							if (pSelectedColumn == columns)
							{
								//pSelectedColumn = thisbutton.Value.Name;
								
								pSelectedAscending = !pSelectedAscending;
							}
							else
							{
								//Print(pSelectedColumn);
								
								//if (columns != "Fade" && columns != "x" && columns != "Type" && columns != "Auto Close")
								{
								
									pSelectedColumn = thisbutton.Value.Name;
									
								}
								//pSelectedAscending = true;
							}
						
							//Print(pSelectedColumn);
							
						}
						
						
						
						
					
					
				
					
	
					ChartControl.InvalidateVisual();
					return;
					
				}
				
			
			}
			
			
			
		
		
			
			
            foreach (KeyValuePair<double, ButtonBOT> thisbutton in AllButtonZ5)
            {
				
				bool hoverednew = MouseIn(thisbutton.Value.Rect, 0, 0);
				
				if (thisbutton.Value.Text == null)
					continue;
				
				
				PNLStatistics BC = thisbutton.Value.Text;
				
				string function = thisbutton.Value.Name;
				string clickedaccount = "";
				
				
				if (hoverednew)
				{
					//Print("------------------------------------------------------------ CLICKED " + BC.Instrument + "   " + BC.Name);
				
					if (BC.Acct != null)
						clickedaccount = BC.Acct.Name;
					
					
					//Print("function   " + function);
					
					//Print(BC.Signals);
					
					if (function == "Close")
					{
					
						//Print("Close");
						
						if (BC.Signals == "")
						//if (BC.Acct != null)
						{
							//Print("FlattenEverything " + BC.Acct.Name);
							
							if (BC.Acct == CurrentMasterAccount)
								IgnoreExecutionUntilFlat = true;
							
							
							
							FlattenEverything(BC.Acct);							
							
						}
						else // is total row
						{
							
							
							foreach (KeyValuePair<string, PNLStatistics> kvp in FinalPNLStatistics)
							{
							
								
								//Print(kvp.Key);
								
								
								
							//	Print(BC.Signals);
								
								
								PNLStatistics ZZZ = kvp.Value;

									
									//string accname = ZZZ.Acct.Name;
									string accname = ZZZ.PrivateName;
								
								
									string accname2 = "";
								
								
								
									PNLStatistics Y = new PNLStatistics();
								
								
									bool Total1Condition = false;
									bool Total2Condition = false;
									bool Total3Condition = false;
									bool Total4Condition = false;
									bool Total5Condition = false;
									bool Total6Condition = false;
									bool Total7Condition = false;
								

									string Filter1 = pTotal1Filter;
									string Filter2 = pTotal2Filter;
									string Filter3 = pTotal3Filter;
									string Filter5 = pTotal5Filter;
									string Filter6 = pTotal6Filter;
									string Filter7 = pTotal7Filter;

																	
									if (Filter1 == string.Empty)
										Total1Condition = false;
									else
										Total1Condition = accname.Contains(Filter1);
																			
									if (Filter2 == string.Empty)
										Total2Condition = false;
									else
										Total2Condition = accname.Contains(Filter2);
									
									if (Filter3 == string.Empty)
										Total3Condition = false;
									else
										Total3Condition = accname.Contains(Filter3);
									
									if (Filter5 == string.Empty)
										Total5Condition = false;
									else
										Total5Condition = accname.Contains(Filter5);
									
									if (Filter6 == string.Empty)
										Total6Condition = false;
									else
										Total6Condition = accname.Contains(Filter6);
									
									if (Filter7 == string.Empty)
										Total7Condition = false;
									else
										Total7Condition = accname.Contains(Filter7);
									

									Total4Condition = true;
									

									
									if (ZZZ.Acct == CurrentMasterAccount)
										IgnoreExecutionUntilFlat = true;		
									
								
									
					
									AllTotalRows = new SortedList<string, bool>();	
									
									AllTotalRows.Add("TotalRow1", Total1Condition);
									AllTotalRows.Add("TotalRow2", Total2Condition);
									AllTotalRows.Add("TotalRow3", Total3Condition);
									AllTotalRows.Add("TotalRow5", Total5Condition);
									AllTotalRows.Add("TotalRow6", Total6Condition);
									AllTotalRows.Add("TotalRow7", Total7Condition);
																	
									AllTotalRows.Add("TotalRow4", Total4Condition);
									

									foreach (KeyValuePair<string, bool> aaaaa in AllTotalRows.ToList())
									{
										
										bool ThisCondition = aaaaa.Value;
										string ThisTitle = aaaaa.Key;

										if (ThisTitle == BC.Signals && ThisCondition)
										{
											FlattenEverything(ZZZ.Acct);
										}
										
									}
									
			
									
									
									
							}
							
						}
				
						
					}
					
					if (function == "Pending")
					{
					
						//Print("Close");
						
						if (BC.Signals == "")
						//if (BC.Acct != null)
						{
							//Print("FlattenEverything " + BC.Acct.Name);
							
							
							
								foreach (Order or in BC.Acct.Orders.ToList())
								{
									if (IsPendingEntryOrder(or))
									{
										BC.Acct.Cancel(new[] { or });		
									}
									
								}
								
												
							
						}
						else // is total row
						{
							
							
							foreach (KeyValuePair<string, PNLStatistics> kvp in FinalPNLStatistics)
							{
							
								
								//Print(kvp.Key);
								
								
								
							//	Print(BC.Signals);
								
								
								PNLStatistics ZZZ = kvp.Value;

									
									//string accname = ZZZ.Acct.Name;
									string accname = ZZZ.PrivateName;
								
									string accname2 = "";
								
								
								
									PNLStatistics Y = new PNLStatistics();
								
								
									bool Total1Condition = false;
									bool Total2Condition = false;
									bool Total3Condition = false;
									bool Total4Condition = false;
									bool Total5Condition = false;
									bool Total6Condition = false;
									bool Total7Condition = false;
								

									string Filter1 = pTotal1Filter;
									string Filter2 = pTotal2Filter;
									string Filter3 = pTotal3Filter;
									string Filter5 = pTotal5Filter;
									string Filter6 = pTotal6Filter;
									string Filter7 = pTotal7Filter;

																	
									if (Filter1 == string.Empty)
										Total1Condition = false;
									else
										Total1Condition = accname.Contains(Filter1);
																			
									if (Filter2 == string.Empty)
										Total2Condition = false;
									else
										Total2Condition = accname.Contains(Filter2);
									
									if (Filter3 == string.Empty)
										Total3Condition = false;
									else
										Total3Condition = accname.Contains(Filter3);
									
									if (Filter5 == string.Empty)
										Total5Condition = false;
									else
										Total5Condition = accname.Contains(Filter5);
									
									if (Filter6 == string.Empty)
										Total6Condition = false;
									else
										Total6Condition = accname.Contains(Filter6);
									
									if (Filter7 == string.Empty)
										Total7Condition = false;
									else
										Total7Condition = accname.Contains(Filter7);
									

									Total4Condition = true;
									
									
									
								
									
									if (ZZZ.Acct == CurrentMasterAccount)
										IgnoreExecutionUntilFlat = true;		
									
									
									
								
					
									AllTotalRows = new SortedList<string, bool>();	
									
									AllTotalRows.Add("TotalRow1", Total1Condition);
									AllTotalRows.Add("TotalRow2", Total2Condition);
									AllTotalRows.Add("TotalRow3", Total3Condition);
									AllTotalRows.Add("TotalRow5", Total5Condition);
									AllTotalRows.Add("TotalRow6", Total6Condition);
									AllTotalRows.Add("TotalRow7", Total7Condition);
																	
									AllTotalRows.Add("TotalRow4", Total4Condition);
									

									foreach (KeyValuePair<string, bool> aaaaa in AllTotalRows.ToList())
									{
										
										bool ThisCondition = aaaaa.Value;
										string ThisTitle = aaaaa.Key;

										if (ThisTitle == BC.Signals && ThisCondition)
										{
											foreach (Order or in ZZZ.Acct.Orders.ToList())
											{
												if (IsPendingEntryOrder(or))
												{
													ZZZ.Acct.Cancel(new[] { or });		
												}	
											}
										}
																			
									}
									
																			
									
							
									
									
									
							}
							
						}
				
						
					}
					
					
					
					if (function == "Hide")
					{
					

						if (BC.Acct != null)
						{
							
							
							//Print(BC.Acct.Name);
							
							
							bool IsAccountDisconnected = !AllConnectedAccounts.Contains(BC.Acct.Name);
							//bool IsAccountDisconnected = BC.Acct.ConnectionStatus == ConnectionStatus.Disconnected;				
														
							
							//Print(IsAccountDisconnected);
							
							//Print(clickedaccount);
							
							if (IsAccountDisconnected)
							{
								
								RemoveAccountData(clickedaccount); 
								
							}
							else
							{
					
							
								AllHideAccounts.Add(clickedaccount);
								SetAccountData("",clickedaccount,"","","","","Yes","","","");
								AllDuplicateAccounts.Remove(clickedaccount);
								SetAccountData("",clickedaccount,"None","","","","","","","");

					
								
							}
							
						}
			
						
						GetAllPerformanceStats();
						
						
						AdjustmentRows = Math.Min(TotalAccounts - MaximumAccountRows - 1, AdjustmentRows);
						
				
						if (pShowFollowersAtTop)
							AdjustmentRows = Math.Min(TotalAccounts - MaximumAccountRows - AllDuplicateAccounts.Count - 1, AdjustmentRows);
					
											
						AdjustmentRows = Math.Max(0, AdjustmentRows);
					
						
						
						
					}	
				
					if (function == "Auto Exit")
					{
					
					
							
						string asdfgdh = GetSavedAccountExitSwitch(clickedaccount);
							
							
						if (asdfgdh == "No")
							asdfgdh = "Yes";
						else
							asdfgdh = "No";
							
				
						//Print(asdfgdh + "  -  " + clickedaccount);	
					
								
								SetAccountData("7",clickedaccount,"","","","","", asdfgdh,"","");
										
					
							
							
					}		
					
					
					if (function == "Auto Close")
					{
					
					
							
						//Print(clickedaccount + "  1");
						
			
						
						string asdfgdh = GetSavedAutoClose(clickedaccount);
							
							
						if (asdfgdh == "No")
							asdfgdh = "Yes";
						else
							asdfgdh = "No";
							
				
					
									SetAccountData("",clickedaccount,"","","","","","",asdfgdh,"");
								
									//Print(clickedaccount + "  2");		
					
							
							
					}						
				
					
					if (function == "TestAction")
					{
						
						
					}
					
					if (function == "Ghost Orders")
					{
							
						string asfsdgfd = BC.PrivateName;
						
						if (BC.Signals == "")
						{
							
							if (BC.Acct.Provider == Provider.Simulator)
							{
								AddMessage("The simulated account " + asfsdgfd + " will be reset in a moment.", 10, GOLDENROD);
								BC.Acct.ResetSimulationAccount(true);
								
								
							}
							else
							{
							}
															
						
							
						}
						else
						{
	
							foreach (KeyValuePair<string, PNLStatistics> kvp in FinalPNLStatistics)
							{
							
								
								//Print(kvp.Key);
								
								
								
							//	Print(BC.Signals);
								
							
								bool IsOneTradeReady = false;
							
			
								IsOneTradeReady = kvp.Value.FrozenOrders > 0;
								
								
								
								PNLStatistics ZZZ = kvp.Value;

									
									//string accname = ZZZ.Acct.Name;
									string accname = ZZZ.PrivateName;
								
								
									string accname2 = "";
								
								
								
									PNLStatistics Y = new PNLStatistics();
								
								
								
									bool Total1Condition = false;
									bool Total2Condition = false;
									bool Total3Condition = false;
									bool Total4Condition = false;
									bool Total5Condition = false;
									bool Total6Condition = false;
									bool Total7Condition = false;
								

									string Filter1 = pTotal1Filter;
									string Filter2 = pTotal2Filter;
									string Filter3 = pTotal3Filter;
									string Filter5 = pTotal5Filter;
									string Filter6 = pTotal6Filter;
									string Filter7 = pTotal7Filter;

																	
									if (Filter1 == string.Empty)
										Total1Condition = false;
									else
										Total1Condition = accname.Contains(Filter1);
																			
									if (Filter2 == string.Empty)
										Total2Condition = false;
									else
										Total2Condition = accname.Contains(Filter2);
									
									if (Filter3 == string.Empty)
										Total3Condition = false;
									else
										Total3Condition = accname.Contains(Filter3);
									
									if (Filter5 == string.Empty)
										Total5Condition = false;
									else
										Total5Condition = accname.Contains(Filter5);
									
									if (Filter6 == string.Empty)
										Total6Condition = false;
									else
										Total6Condition = accname.Contains(Filter6);
									
									if (Filter7 == string.Empty)
										Total7Condition = false;
									else
										Total7Condition = accname.Contains(Filter7);
									

									Total4Condition = true;
									
									
									
									
									
									if (ZZZ.Acct == CurrentMasterAccount)
										IgnoreExecutionUntilFlat = true;		
									
								
									AllTotalRows = new SortedList<string, bool>();	
									
									AllTotalRows.Add("TotalRow1", Total1Condition);
									AllTotalRows.Add("TotalRow2", Total2Condition);
									AllTotalRows.Add("TotalRow3", Total3Condition);
									AllTotalRows.Add("TotalRow5", Total5Condition);
									AllTotalRows.Add("TotalRow6", Total6Condition);
									AllTotalRows.Add("TotalRow7", Total7Condition);
																	
									AllTotalRows.Add("TotalRow4", Total4Condition);
									

									foreach (KeyValuePair<string, bool> aaaaa in AllTotalRows.ToList())
									{
										
										bool ThisCondition = aaaaa.Value;
										string ThisTitle = aaaaa.Key;

										if (ThisTitle == BC.Signals && ThisCondition && IsOneTradeReady)
										{
											
											if (ZZZ.Acct.Provider == Provider.Simulator)
											{
												AddMessage("All simulated accounts with ghost orders will be reset in a moment.", 10, GOLDENROD);
												ZZZ.Acct.ResetSimulationAccount(true);
											}
											else
											{
											}
										}
										
									}
									
								
									
									
									
							}
							
							
							
						}
				
						
						//Print(BC.Acct);
						
						
						
						// testing clicks in actions column, doesn't highlight the button like the others
						
					}
					
					if (function == "One Trade")
					{

						Instrument OneTradeInstrument = GetTheInstrument(Instrument.FullName, "Micro", false);
						
						
						//OneTradeInstrument = GetTheInstrument("MES 12-23", "Micro");
						
						
						
						if (BC.Signals == "")
						//if (BC.Acct != null)
						{
				
							
							
								

								//BuyOne(1, OneTradeInstrument, BC.Acct);	
								//SellOne(1, OneTradeInstrument, BC.Acct);
									
							
							
								
							//Print(clickedaccount + "  1");
							
				
											
							
						}
						else // is total row
						{
							
							
							foreach (KeyValuePair<string, PNLStatistics> kvp in FinalPNLStatistics)
							{
							
								
								//Print(kvp.Key);
								
								
								
							//	Print(BC.Signals);
								
								bool IsAccountFunded = false;
								bool IsAccountIsPropFirmPA = false;
								bool IsAccountNoTrades = false;
								bool IsOneTradeReady = false;
								bool IsDisconnected = kvp.Value.Acct.ConnectionStatus == ConnectionStatus.Disconnected;
								
								IsDisconnected = AllConnectedAccounts.Contains(kvp.Value.Acct.Name);
								
								
								
								if (kvp.Value.Acct.Name.Contains("PA-APEX") || kvp.Value.Acct.Name.Contains("PAAPEX") || kvp.Value.Acct.Name.Contains("PA-LL"))
									IsAccountIsPropFirmPA = true;
								
								
								
								if (kvp.Value.FromFunded - pDollarsExceedFunded <= 0)
									IsAccountFunded = true;
												
											
												
								if (kvp.Value.TotalBought == 0)
									IsAccountNoTrades = true;
												
								
								bool FundedOK = IsAccountFunded;
								FundedOK = true;
								
								if (kvp.Value.FromFunded == 1000000)
									FundedOK = false;
								
								bool PAs = IsAccountIsPropFirmPA && pOneTradePAEnabled;
								bool Evals = FundedOK && pOneTradeEvalEnabled;
			
								IsOneTradeReady = (PAs || Evals) && IsAccountNoTrades && IsDisconnected;
								
								
								
								PNLStatistics ZZZ = kvp.Value;

									
									//string accname = ZZZ.Acct.Name;
									string accname = ZZZ.PrivateName;
								
								
									string accname2 = "";
								
								
								
									PNLStatistics Y = new PNLStatistics();
								
								
								
								
									bool Total1Condition = false;
									bool Total2Condition = false;
									bool Total3Condition = false;
									bool Total4Condition = false;
									bool Total5Condition = false;
									bool Total6Condition = false;
									bool Total7Condition = false;
								

									string Filter1 = pTotal1Filter;
									string Filter2 = pTotal2Filter;
									string Filter3 = pTotal3Filter;
									string Filter5 = pTotal5Filter;
									string Filter6 = pTotal6Filter;
									string Filter7 = pTotal7Filter;

																	
									if (Filter1 == string.Empty)
										Total1Condition = false;
									else
										Total1Condition = accname.Contains(Filter1);
															
									
//									Print(accname);
//									Print(BC.Signals);
//									Print(Total1Condition);
									
									
									
									
									if (Filter2 == string.Empty)
										Total2Condition = false;
									else
										Total2Condition = accname.Contains(Filter2);
									
									if (Filter3 == string.Empty)
										Total3Condition = false;
									else
										Total3Condition = accname.Contains(Filter3);
									
									if (Filter5 == string.Empty)
										Total5Condition = false;
									else
										Total5Condition = accname.Contains(Filter5);
									
									if (Filter6 == string.Empty)
										Total6Condition = false;
									else
										Total6Condition = accname.Contains(Filter6);
									
									if (Filter7 == string.Empty)
										Total7Condition = false;
									else
										Total7Condition = accname.Contains(Filter7);
									

									Total4Condition = true;
									
									
									
									
									
									if (ZZZ.Acct == CurrentMasterAccount)
										IgnoreExecutionUntilFlat = true;		
									
								
								
									AllTotalRows = new SortedList<string, bool>();	
									
									AllTotalRows.Add("TotalRow1", Total1Condition);
									AllTotalRows.Add("TotalRow2", Total2Condition);
									AllTotalRows.Add("TotalRow3", Total3Condition);
									AllTotalRows.Add("TotalRow5", Total5Condition);
									AllTotalRows.Add("TotalRow6", Total6Condition);
									AllTotalRows.Add("TotalRow7", Total7Condition);
																	
									AllTotalRows.Add("TotalRow4", Total4Condition);
									

									foreach (KeyValuePair<string, bool> aaaaa in AllTotalRows.ToList())
									{
										
										bool ThisCondition = aaaaa.Value;
										string ThisTitle = aaaaa.Key;

										
										
										
										if (ThisTitle == BC.Signals && ThisCondition && IsOneTradeReady)
										{
											//BuyOne(1, OneTradeInstrument, ZZZ.Acct);	
											//SellOne(1, OneTradeInstrument, ZZZ.Acct);
																		
											//Print("One Trade " + ZZZ.Acct);
										}
										
									}									
									
					
									
									
									
							}
							
						}
						
						
			
							
							
					}						
					
					
					
					
					
//					if (function == "Close All")
//					{
//						//if (pmast
//						FlattenEverything(BC.Acct);
				
						
//					}
									
				
				
					if (function == "–")
					{
						
						
						if (BC.Acct != null)
						{
						
							
								
								LastClickedAccount = clickedaccount;
							
							
						}
						
					}
					
	
					if (function == "Disconnected")
					{
						
						 
						if (BC.Acct != null)
						{
							//Print("clickedaccount   " + clickedaccount);
							
							
							RemoveAccountData(clickedaccount);
							
						}
						
					}
					
					
					
					if (function == "Auto Liquidate")
					{
						
						if (BC.Acct != null)
						{
						
							LastClickedAutoLiquidateAccount = BC.Acct.Name;
							
						}
						
						
						

					}
					
					
					
						
					if (function == "Account")
					{
						
						if (BC.Acct != null)
						{
						
						
							
							
							
//							if (pIsBuildMode || !pLockAccountsOnLock)
//							if (pThisMasterAccount != clickedaccount)
							{
								
								//Print(clickedaccount + "   " + AllDuplicateAccounts.Contains(clickedaccount));
								
								
								
								
								if (AllDuplicateAccounts.Contains(clickedaccount))
								{
									AllDuplicateAccounts.Remove(clickedaccount);
									
									SetAccountData("",clickedaccount,"None","","","","","","","");
									
								}
								else
								{
									AllDuplicateAccounts.Add(clickedaccount);
									
									//Print(clickedaccount + "1   " + AllDuplicateAccounts.Contains(clickedaccount));
									
									
									
									SetAccountData("",clickedaccount,"Slave","","","","","","","");
									
									
									
									
									
									
								}
							}
								
							//Print("AllDuplicateAccounts.  " + AllDuplicateAccounts.Count);
							
						}
						
					}
					
				
					
					
					
					
					if (function == "Fade")
					{
						
						if (BC.Acct != null)
						//if (IsAccountFlat(BC.Acct))
							if (IsAccountFlatAndNoPendingOrders(clickedaccount, function))	
						{
						
							
							
							string lastfade = GetAccountFade(clickedaccount);
							
							
							if (pThisMasterAccount != clickedaccount)
							{
							
								{
									if (lastfade == "Yes")
									{
										SetAccountData("",clickedaccount,"","","","No","","","","");
										
										
										if (pIsATMSelectEnabled)
										//if (pIsATMSelectEnabled && GetAccountMode(clickedaccount) == "Executions")
										{
											SetAccountData("",clickedaccount,"","","","","","","","Default");
											
											
										}
										
									}
									else
									{
										SetAccountData("",clickedaccount,"","","","Yes","","","","");
										
										if (pIsATMSelectEnabled)
										{
											SetAccountData("",clickedaccount,"","","","","","","","Executions");
											
											
										}
										
										
									}
								}
							}
						
							
						}
						
					}	
					
					
					
					if (function == "Type")
					{
						
						if (BC.Acct != null)
							if (IsAccountFlatAndNoPendingOrders(clickedaccount, function))	
						//if (IsAccountFlat(BC.Acct))
						{
						
						
							if (pThisMasterAccount != clickedaccount)
							{
							
								{
									if (AllCrossAccounts.Contains(clickedaccount))
									{
										AllCrossAccounts.Remove(clickedaccount);
										SetAccountData("",clickedaccount,"","","No","","","","","");
									}
									else
									{
										AllCrossAccounts.Add(clickedaccount);
										SetAccountData("",clickedaccount,"","","Yes","","","","","");
										
										
										
										SetAccountData("",clickedaccount,"","1","","","","","","");
										
										
										
									}
								}
							}
						
							
						}
						
					}	
						
						
					if (function == "Mode")
					{
						
						if (BC.Acct != null)
						if (IsAccountFlatAndNoPendingOrders(clickedaccount, function))
						{
							
							
								
							
							
							string current = GetAccountMode(SelectedATMAccount);

//							if (current != "Default")
//								LastATMSelected = current;							
													
							
							if (SelectedATMAccount == clickedaccount)
							{

								//if (current == "Default")		
									SetAccountData("",SelectedATMAccount,"","","","","","","",LastATMSelected);
								
								LastATMSelected = GetAccountMode(SelectedATMAccount);
								
								SelectedATMAccount = string.Empty;
								SelectedATMTime = DateTime.MinValue;
								
								 												
							}
							
							else if (pThisMasterAccount != clickedaccount)
							{
								
								ResetAllTimers();
								
								SelectedATMAccount = clickedaccount;
								SelectedATMTime = DateTime.Now.AddSeconds(3);

										// set to previous
				
//								if (current == "Default")		
//									SetAccountData("",SelectedATMAccount,"","","","","","","",LastATMSelected);
								
							}
							

							
						}
						
					}	
					
					

					if (function == sizecolname)
					{
						
						if (BC.Acct != null)
						if (IsAccountFlatAndNoPendingOrders(clickedaccount, function))	
						//if (IsAccountFlat(BC.Acct))
						{
						
							double current = Convert.ToDouble(GetAccountMultiplier(SelectedMultiplierAccount));

//							if (current != 1)
//								LastSizeNumber = current;							
							
							if (SelectedMultiplierAccount == clickedaccount)
							{

								//if (current == 1)		
									SetAccountData("",SelectedMultiplierAccount,"",LastSizeNumber.ToString(),"","","","","","");	
								
								LastSizeNumber = Convert.ToDouble(GetAccountMultiplier(SelectedMultiplierAccount));
								
								SelectedMultiplierAccount = string.Empty;
								SelectedMultiplerTime = DateTime.MinValue;
							}
							
							else if (pThisMasterAccount != clickedaccount)
							{
								
								ResetAllTimers();
								
								SelectedMultiplierAccount = clickedaccount;
								SelectedMultiplerTime = DateTime.Now.AddSeconds(3);
				
								// set to previous
					
//								if (current == 1)		
//									SetAccountData("",SelectedMultiplierAccount,"",LastSizeNumber.ToString(),"","","","","","");			
					
								
							}
							

							
						}
						
					}			
					
					
					
					
					double pInitialGoal = 100;
					double pInitialLoss = -100;
					
					double ThisInitial = 0;
	


					if (function == "Daily Goal")
					{
						
						if (BC.Acct != null)
						//if (IsAccountFlat(BC.Acct))
						{
							
//							if (LastClickedDailyGoalAccount == "Finish")
//							{
//								LastClickedDailyGoalAccount = string.Empty;
//								continue;
								
//							}
							
							double current = GetAccountSavedData(clickedaccount, pAllAccountDailyGoal);
							
							if (current != -1000000000)
								LastDailyGoalNumber = current;
							
							if (pDailyGoalDisplayMode == "Status")// && current != 0)
							{
								
								
								pShowPercentOnGoalLoss = !pShowPercentOnGoalLoss;
								
							}
							else
							{
								
								LastClickedDailyGoalAccount = clickedaccount;							
								
								ThisInitial = pInitialGoal;
								
								if (LastDailyGoalNumber != 0)
									ThisInitial = LastDailyGoalNumber;
						
								if (current == -1000000000)
								{
									SetAccountSavedData(clickedaccount, pAllAccountDailyGoal, ThisInitial.ToString());
																
								}
					
			
														
								if (SelectedDailyGoalAccount == clickedaccount)
								{
									SelectedDailyGoalAccount = string.Empty;
									SelectedDailyGoalTime = DateTime.MinValue;
								}
								
								else// if (pThisMasterAccount != clickedaccount)
								{
									ResetAllTimers();
									
									SelectedDailyGoalAccount = clickedaccount;
									SelectedDailyGoalTime = DateTime.Now.AddSeconds(3);
																
								}
												
								
								
								
							}
							
							
							
					
						
							
						}
						
					}	
					
				if (function == "Daily Loss")
					{
						
						if (BC.Acct != null)
						//if (IsAccountFlat(BC.Acct))
						{
							//Print(LastClickedDailyLossAccount);
							
							double current = GetAccountSavedData(clickedaccount, pAllAccountDailyLoss);
							
							if (current != -1000000000)
								LastDailyLossNumber = current;
							
							
							if (pDailyLossDisplayMode == "Status") // && current != 0)
							{
								
								pShowPercentOnGoalLoss = !pShowPercentOnGoalLoss;
								
								
							}
							else
							{

								
								LastClickedDailyLossAccount = clickedaccount;
								
								ThisInitial = pInitialLoss;
								
								if (LastDailyLossNumber != 0)
									ThisInitial = LastDailyLossNumber;
								
								if (current == -1000000000)
								{
									SetAccountSavedData(clickedaccount, pAllAccountDailyLoss, ThisInitial.ToString());
						
								}
															
								if (SelectedDailyLossAccount == clickedaccount)
								{
									SelectedDailyLossAccount = string.Empty;
									SelectedDailyLossTime = DateTime.MinValue;
								}
								
								else// if (pThisMasterAccount != clickedaccount)
								{
									ResetAllTimers();
									
									SelectedDailyLossAccount = clickedaccount;
									SelectedDailyLossTime = DateTime.Now.AddSeconds(3);
																
								}
								

								//Print(SelectedDailyLossAccount);
			
														
									
								
								
								
							}
							
						
								
															
						
							
						}
						
					}						
					

	
					if (function == "Payout")
					{
						
						if (BC.Acct != null)
						//if (IsAccountFlat(BC.Acct))
						{
							//Print(LastClickedDailyLossAccount);
							
							double current = GetAccountSavedData(clickedaccount, pAllAccountPayouts);
							
							
							if (current != -1000000000)
								LastDailyPayoutNumber = current;
														
//							if (pDailyLossDisplayMode == "Status") // && current != 0)
//							{
								
//								pShowPercentOnGoalLoss = !pShowPercentOnGoalLoss;
								
								
//							}
//							else
//							{
		
							
								LastClickedPayoutAccount = clickedaccount;

								// adjust to be max payout for certain accounts
								double initialamount = 100;
							
								if (BC.MaxPayout != 0)
									initialamount = BC.MaxPayout;
							
								ThisInitial = initialamount;
								
								if (LastDailyPayoutNumber != 0)
									ThisInitial = LastDailyPayoutNumber;								
							
								
								
								
								if (current == -1000000000)
								{
									SetAccountSavedData(clickedaccount, pAllAccountPayouts, ThisInitial.ToString());
						
								}
															
								if (SelectedPayoutAccount == clickedaccount)
								{
									SelectedPayoutAccount = string.Empty;
									SelectedPayoutTime = DateTime.MinValue;
								}
								
								else// if (pThisMasterAccount != clickedaccount)
								{
									ResetAllTimers();
									
									SelectedPayoutAccount = clickedaccount;
									SelectedPayoutTime = DateTime.Now.AddSeconds(3);
																
								}
								

								//Print(SelectedPayoutAccount);
							
			
														
									
								
								
								
//							}
							
						
								
															
						
							
						}
						
					}						
					
					
					
					
				
					ChartControl.Dispatcher.InvokeAsync(() =>
						{
							
							
							GetAllPerformanceStats();
							GetAllPerformanceStats();
							ChartControl.InvalidateVisual();
							
						});
				

				
					
					
				}
			}
			
			
			
	//		Print(LastClickedAccount);
			
//            foreach (KeyValuePair<double, ButtonBOT> thisbutton in AllButtonZ6)
//            {
				
//				bool hoverednew = MouseIn(thisbutton.Value.Rect, 0, 0);
				
//				PNLStatistics BC = thisbutton.Value.Text;
				
			
//			}
			
			
			
			
			
			
			
			
			
            foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
            {
				
				
				
				
                bool hoverednew = MouseIn(thisbutton.Value.Rect, 2, 2);
                string buttonn = thisbutton.Value.Text;

				
				bool RedoAccountsTable = false;
				
				
				
			
				
				
				if (hoverednew && buttonn == "All Instruments")
                {
					
					if (!pIsCopyBasicFunctionsEnabled && !pIsRiskFunctionsEnabled)
					{
						pAllInstruments = !pAllInstruments;
					}
					
					
					
					if (!pIsCopyBasicFunctionsEnabled)
						return;
					
					string instending = "before changing the instrument detection status.";
					
					if (TotalPendingOrders != 0 && TotalPositions != 0)
					{
						RemoveMessage(instending);
						AddMessage("Please flatten all accounts " + instending, 10, RED);
						
					}
					else if (TotalPendingOrders != 0)
					{
						RemoveMessage(instending);
						AddMessage("Please cancel all pending orders " + instending, 10, RED);
					}
					else if (TotalPositions != 0)
					{
						RemoveMessage(instending);
						AddMessage("Please close all positions " + instending, 10, RED);
					}	
					else
					{
						
						pAllInstruments = !pAllInstruments;
						
						string miniii = GetTheInstrument(Instrument.FullName, "Mini", false).FullName;
						string microii = GetTheInstrument(Instrument.FullName, "Micro", false).FullName;
						
						RemoveMessage("Instrument detection status");
						
						RemoveMessage("종목 감지 상태가 업데이트되어");
						RemoveMessage("모든 종목에 대한 거래 주");
						
						if (pAllInstruments)
						{
							if (!IsKorean)
							{
								AddMessage("Instrument detection status has been updated to track orders across all instruments.", 15, GOLDENROD);
							}
							else
							{
								AddMessage("모든 종목에 대한 거래 주문을 위해 종목 감지 상태가 업데이트되었습니다.", 15, GOLDENROD);
								
							}
							
						}
						else
						{
							if (!IsKorean)
							{
								if (miniii != microii)
									AddMessage("Instrument detection status has been updated to track orders for '" + miniii + "' and '" + microii + "' instruments only.", 15, GOLDENROD);
								else
									AddMessage("Instrument detection status has been updated to track orders for '" + miniii + "' only.", 15, GOLDENROD);
							}
							else
							{
								if (miniii != microii)
									AddMessage("종목 감지 상태가 업데이트되어 '" + miniii + "' 및 '" + microii + "' 종목에 대한 주문만 추적합니다.", 15, GOLDENROD);
								else
									AddMessage("종목 감지 상태가 업데이트되어 '" + miniii + "' 종목에 대한 주문만 추적합니다.", 15, GOLDENROD);								
								
							}							

						}
						
//						if (!pCopierIsEnabled)
//						{
						
//							pAllInstruments = !pAllInstruments;
							
							
//						}
//						else
//						{
//							// error message
//						}
												
					}
					
					

					
					
                    thisbutton.Value.Switch = true;
                   
				
				}	
												
		
				
				if (hoverednew && buttonn == pHideAString)
                {	
					
					pHideAccountsIsEnabled = !pHideAccountsIsEnabled;
					thisbutton.Value.Switch = pHideAccountsIsEnabled;
					
										
					
				}
				if (hoverednew && buttonn == pHideCString)
                {	
					pHideCurrencyIsEnabled = !pHideCurrencyIsEnabled;
					thisbutton.Value.Switch = pHideCurrencyIsEnabled;
					
					
				}
				
				
				if (hoverednew && buttonn == "Main")
                {
					
						
						
					CopierMainButton();
					
                    thisbutton.Value.Switch = pCopierIsEnabled;
                    
					//RedoAccountsTable = true;
				
				}	
				
				
					
			
			
				if (hoverednew && buttonn == "Exit Shield")
                {
					
					pExitShieldIsEnabled = !pExitShieldIsEnabled;
					
					thisbutton.Value.Switch = pExitShieldIsEnabled;
					
				}
				
				
				
				if (hoverednew && buttonn == "Lock")
                {
					
					RemoveMessage("You must set the");
					RemoveMessage("You must add at");	
					
					
					RemoveMessage("카피트레이딩 작업을 활성화하려면 최소한 하나의 팔로워 계정을 추가해야 합니다. 계좌 이름을 클릭하여 팔로워 계정으로 설정하십시오.");

			
					
					if (!pAffiliateLinkDone1 && HasAPEXAccounts)
					{
						//System.Diagnostics.Process.Start(APEXURL);
						pAffiliateLinkDone1 = true;
						 
					}
			
					if (!pAffiliateLinkDone2 && HasLeeLooAccounts)
					{
						//System.Diagnostics.Process.Start(APEXURL);
						pAffiliateLinkDone2 = true;
						 
					}			
			
				
						if (pIsBuildMode)
						{
							
							
							if (AccountsAreConfigured())
							{
								AdjustmentRows = 0;
								pIsBuildMode = false;
								
							}
							else
							{
								
								string enddddd = "'Lock' Accounts Dashboard";
								

								if (AllDuplicateAccounts.Count == 0 && pThisMasterAccount == string.Empty)
								{
									if (TotalAccounts != 0)
									{
										if (!IsKorean)
										AddMessage("You must set the Master account and add at least one Follower account to " + enddddd + ".  Double click the connected status dot to set the Master account.  Click the account name to set it as a Follower account.", 20, RED);
								
										if (IsKorean)
										{
											AddMessage(AllKorean, 20, RED);
										}
										
									}
									
									
									
								}
								else if (pThisMasterAccount == string.Empty)
								{
									if (!IsKorean)
									AddMessage("You must set the Master account to " + enddddd + ".  Double click the connected status dot to set the Master account.", 20, RED);
									
									if (IsKorean)
									{
										AddMessage(AllKorean, 20, RED);
									}									
									
								}
								else if (AllDuplicateAccounts.Count == 0)
								{
									if (!IsKorean)
									AddMessage("You must add at least one Follower account to " + enddddd + ".  Click the account name to set it as a Follower account.", 20, RED);
									
									if (IsKorean)
									{
										AddMessage("카피트레이딩 작업을 활성화하려면 최소한 하나의 팔로워 계정을 추가해야 합니다. 계좌 이름을 클릭하여 팔로워 계정으로 설정하십시오.", 20, RED);
									}									
									
								}	
								else
								{
									
									if (CurrentMasterAccount != null)
									{
										bool IsConnectedOK = pAllowDisconnectedAccounts || CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected;

										if (IsConnectedOK)
										//if (IsConnectedOK && CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected)
										{
											
										}
										else
										{
											
											AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
											
										}
										
									}
									else
									{
										
										AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
									}
									
									
								}
								
								
								
								
//								if (AllDuplicateAccounts.Count == 0 && pThisMasterAccount == string.Empty)
//								{
//									if (TotalAccounts != 0)
//									AddMessage("You must set the Master account and add at least one Follower account to " + enddddd + ".  Double click the connected status dot to set the Master account.  Click the account name to set it as a Follower account.", 10, RED);
//								}
//								else if (pThisMasterAccount == string.Empty)
//								{
//									AddMessage("You must set the Master account to " + enddddd + ".  Double click the connected status dot to set the Master account.", 10, RED);
									
//								}
//								else if (AllDuplicateAccounts.Count == 0)
//								{
//									AddMessage("You must add at least one Follower account to " + enddddd + ".  Click the account name to set it as a Follower account.", 10, RED);
									
//								}	
//								else
//								{
									
//									if (CurrentMasterAccount != null)
//									{
//										bool IsConnectedOK = pAllowDisconnectedAccounts || CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected;

//										if (IsConnectedOK && CurrentMasterAccount.ConnectionStatus == ConnectionStatus.Connected)
//										//if (IsConnectedOK)
//										{
											
//										}
//										else
//										{
											
//											AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
											
//										}
										
//									}
//									else
//									{
										
//										AddMessage("Your Master account must be connected to " + enddddd + ".", 10, RED);
//									}
									
									
//								}
								
							}
							
								
						}
						else
						{
					
							
							//if (IsMasterAccountFlat())
								pIsBuildMode = !pIsBuildMode;
								pCopierIsEnabled = false;
					
						}		
						
						
	
					//pIsBuildMode = !pIsBuildMode;
					
                    thisbutton.Value.Switch = pIsBuildMode;

					RedoAccountsTable = true;
				}	
				
				
				
				
				
				
				

				if (hoverednew && buttonn == pCopierMode)
                {
					
					//Print("Coper mode");
				
					CopierOnOff();
			
					thisbutton.Value.Text = pCopierMode;
					
					RedoAccountsTable = true;
					
					
					
					
					//pCopierIsEnabled = !pCopierIsEnabled;
					
					 
					//thisbutton.Value.Text = pCopierMode;
                    //thisbutton.Value.Switch = true;
                    
					
					
					//	 thisbutton.Value.Switch = pIsFadeEnabled;
					
				
				}					
				
				
				
		
				
				if (hoverednew && buttonn == RiskButtonString)
                {
					
//					if (!pIsRiskFunctionsPermission)
//						return;
					
					
						pIsRiskFunctionsEnabled = !pIsRiskFunctionsEnabled;
							
						
						if (!pAllowBothFeaturesToBeOff)
						if (!pIsCopyBasicFunctionsEnabled)
							pIsCopyBasicFunctionsEnabled = true;					
					
					
                    thisbutton.Value.Switch = pIsRiskFunctionsEnabled;	
					
						
					if (pIsRiskFunctionsEnabled)
					{
						//pIsBuildMode = true;
						
						if (!pIsCopyBasicFunctionsEnabled)
							pAllInstruments = true;
						
						//DisableFollowerAccounts();
						//DisableMasterAccount();
						
						RedoAccountsTable = true;
						
					}
					
					
				}
				
			
				
				if (hoverednew && buttonn == CopyButtonString)
                {
			
					
//					if (!pIsCopyBasicFunctionsPermission)
//						return;
			
					
					pIsCopyBasicFunctionsEnabled = !pIsCopyBasicFunctionsEnabled;
							
						
					if (!pIsCopyBasicFunctionsEnabled)
					{
						pCopierIsEnabled = false;
						
						if (!pAllowBothFeaturesToBeOff)
						if (!pIsRiskFunctionsEnabled)
							pIsRiskFunctionsEnabled = true;
					}
					
					
					if (!pIsCopyBasicFunctionsEnabled)
					{
						
						pCopierIsEnabled = false;
						//pIsBuildMode = true;
						
						if (pIsRiskFunctionsEnabled)
							pAllInstruments = true;
						
						//DisableFollowerAccounts();
						//DisableMasterAccount();
						
						RedoAccountsTable = true;
						
						
					}
					
					
					
                    thisbutton.Value.Switch = pIsCopyBasicFunctionsEnabled;	
				
				}
				
				if (hoverednew && buttonn == pRPString)
                {
					
					//Print("Refresh Positions Button");
					
                    thisbutton.Value.Switch = true;	
				
					RefreshPositionsClicked = true;
					RedoAccountsTable = true;	
					
					
				}
								
		
				
				if (hoverednew && buttonn == pFlattenButtonString)
                {
					if (DateTime.Now < FlattenEverythingClickedTime.AddMilliseconds(pMilliSFE))
					{
						
					}
					else
					{
						//Print("FLATTTTTTT");
						
						FlattenEverythingClicked = true;
						FlattenEverythingClickedTotal = 0;
						FlattenEverythingClickedOrders = 0;
						FlattenEverythingClickedTime = DateTime.Now;
						
						RedoAccountsTable = true;	
						
						
						//AddMessage("Your accounts should now be flat.  If NinjaTrader is still displaying positions in this window, or the Positions tab in NinjaTrader Control Center, please double check your true positions in Rithmic, Tradovate, or other external connection.", 30, RED);	
					
					}					
					

					
					
                    thisbutton.Value.Switch = true;	
				
				}
				
				
				
				
				
				
				// weird issue with these buttons, clicking at the same time
						
				if (hoverednew)
				if (buttonn == "Restore" || buttonn == "Reset")
				{
					
							if (SelectedButtonNow == string.Empty)
							{
								
								ResetAllTimers();
								
								SelectedButtonNow = buttonn;
								SelectedButtonTime = DateTime.Now.AddSeconds(10); 

							}
							else
							{
								
								
							//	Print(SelectedButtonNow);
								
								
								
								if (SelectedButtonNow == buttonn)
								{
									// do the action
									
									
									
									if (buttonn == "Restore")
									{
										UnhideAllAccounts();
										RedoAccountsTable = true;	
										
										SelectedButtonNow = string.Empty;
										SelectedButtonTime = DateTime.MinValue;	
										
										RemoveMessage(MainResetConfirmation);
										
										if (!IsKorean)
											AddMessage("All hidden accounts have been successfully restored.", 6, GOLDENROD);
										else
											AddMessage("All hidden accounts have been successfully restored.", 6, GOLDENROD);
										
									}
									if (buttonn == "Reset")
									{
										
										ResetAccountData("");
										RedoAccountsTable = true;	
										
										SelectedButtonNow = string.Empty;
										SelectedButtonTime = DateTime.MinValue;	
										
										RemoveMessage(MainResetConfirmation);
										
										if (!IsKorean)
											AddMessage("All columns and settings across all accounts have been successfully reset.", 6, GOLDENROD);
										else
											AddMessage("모든 계정의 세로열과 설정이 성공적으로 초기화되었습니다.", 6, GOLDENROD);
										
										
										
										
									}									
									
									
								}
								else
								{
									
									ResetAllTimers();
									
									SelectedButtonNow = buttonn;
									SelectedButtonTime = DateTime.Now.AddSeconds(10);
								
								}

							}
							
								
				}
		

							
							

//				if (hoverednew && buttonn == "Restore")
//				{
				
//					//Print("Restore");
				
//					UnhideAllAccounts();
					
					
//					RedoAccountsTable = true;
//				}			
										
	
//				else if (hoverednew && buttonn == "Reset")
//                {
					
//					//Print("Reset");
					
//					ResetAccountData("");
					
//					RedoAccountsTable = true;
					
//                   // thisbutton.Value.Switch = true;
                
				
//				}	
							
				
				
				
				
				if (hoverednew && buttonn == "Accounts")
                {
					
					if (pShowAccounts == "All")
					{
						pShowAccounts = "Live";
						pAccountsLiveEnabled = true;
						pAccountsSimEnabled = false;
					}
					else if (pShowAccounts == "Live")
					{
						pShowAccounts = "Sim";
						pAccountsLiveEnabled = false;
						pAccountsSimEnabled = true;						
					}					
					else if (pShowAccounts == "Sim")
					{
						pShowAccounts = "All";
						pAccountsLiveEnabled = true;
						pAccountsSimEnabled = true;					
					}						
					
					//pShowAccountsMain = pShowAccounts
					
					
					
					FirstLoadAccounts = true;
					
                    thisbutton.Value.Switch = true;
                   
					
					RedoAccountsTable = true;
					
				}	
								
				if (hoverednew && buttonn == "Live Accounts")
                {
					
				
					
					pAccountsLiveEnabled = !pAccountsLiveEnabled;
					
					if (!pAccountsSimEnabled)
						pAccountsLiveEnabled = true;
					
					SetWhichAccounts();
					FirstLoadAccounts = true;
					
					
					RedoAccountsTable = true;
					
                   thisbutton.Value.Switch = pAccountsLiveEnabled;
                
				
				}	
								
				if (hoverednew && buttonn == "Sim Accounts")
                {
					
			
					
					
					pAccountsSimEnabled = !pAccountsSimEnabled;
					
					if (!pAccountsLiveEnabled)
						pAccountsSimEnabled = true;
					
					SetWhichAccounts();
					FirstLoadAccounts = true;
					
					RedoAccountsTable = true;
					
                   thisbutton.Value.Switch = pAccountsSimEnabled;
                
				
				}	
				
				
				
	
				if (hoverednew && buttonn == "Mode")
                {
					
					pIsATMSelectEnabled = !pIsATMSelectEnabled;
				
					if (!pIsATMSelectEnabled && pCopierMode == "Orders")
						pIsFadeEnabled = false;		
					
					//AddMessage("XXXXX " + DateTime.Now.ToString(), 20, GOLDENROD);
					
                    thisbutton.Value.Switch = pIsATMSelectEnabled;
                    
	
					
						if (pIsFadeEnabled)
						if (pIsATMSelectEnabled)
						{
							
							foreach(string s in pAllAccountData)
							{
								if (s!= string.Empty)
								{
									string accountname = s.Split('|')[0];
									
									string oldfade = s.Split('|')[4];
									string oldex = s.Split('|')[8];
									
									
									if (oldex != "Executions")
									if (oldfade == "Yes")
									{
										SetAccountData("",accountname,"","","","No","","","","");
										
										//AddMessage("The 'Mode' column must be set to 'Executions' for the 'Fade' to be set 'Yes'.", 6, RED);
										
									}
									
									
									
								}
										
									
									
							}
							
				
						}	
						
						
				}
				
				
				
	
				if (hoverednew && buttonn == "Size")
                {
					
					pIsXEnabled = !pIsXEnabled;
					
					//AddMessage("XXXXX " + DateTime.Now.ToString(), 20, GOLDENROD);
					
                    thisbutton.Value.Switch = pIsXEnabled;
                    
				
				}	
				
				
				if (hoverednew && buttonn == "Fade")
                {
					
					
					RemoveMessage("The 'Fade' column cannot");
					RemoveMessage("카피트레이딩 작업'이 '주문모드'에서 실행 중일 때는 '반대진입' 기능을 사용할 수 없습니다. '반대진입' 기능을 사용하려면 모드를 '체결모드' 로 설정해 주세요.");
					
					
					bool restrict = false;				
					if (pCopierMode == "Orders")
					{
						if (!pIsATMSelectEnabled)
							restrict = true;
					}
					
					if (restrict)
					{
						if (!IsKorean)
							AddMessage("The 'Fade' column cannot be used when Duplicate Account Actions is running in 'Orders' mode.  Please set this to 'Executions' mode to begin using the 'Fade' column, or turn on the 'Mode' column.", 30, RED);
						else
							AddMessage("카피트레이딩 작업'이 '주문모드'에서 실행 중일 때는 '반대진입' 기능을 사용할 수 없습니다. '반대진입' 기능을 사용하려면 모드를 '체결모드' 로 설정해 주세요.", 30, RED);
						
						pIsFadeEnabled = false;
					}
					else
					{
						pIsFadeEnabled = !pIsFadeEnabled;
					}
					
//					if (pIsFadeEnabled)
//					if (pIsATMSelectEnabled)
//					{
						
//						foreach(string s in pAllAccountData)
//						{
//							if (s!= string.Empty)
//							{
//								string accountname = s.Split('|')[0];
								
//								string oldfade = s.Split('|')[4];
								
								
//								if (oldfade == "Yes")
//									SetAccountData("",accountname,"","","","","","","","Executions");
								
								
								
//							}
									
								
								
//						}
							
//					}
						if (pIsFadeEnabled)
						if (pIsATMSelectEnabled)
						{
							
							foreach(string s in pAllAccountData)
							{
								if (s!= string.Empty)
								{
									string accountname = s.Split('|')[0];
									
									string oldfade = s.Split('|')[4];
									
									string oldex = "Default";
									
									if (s.Split('|').Length > 8)
										oldex = s.Split('|')[8];
									
									if (oldex != "Executions")
									if (oldfade == "Yes")
									{
										SetAccountData("",accountname,"","","","No","","","","");
										
										//AddMessage("The 'Mode' column must be set to 'Executions' for the 'Fade' to be set 'Yes'.", 6, Brushes.Red);
										
									}
									
									
									
								}
										
									
									
							}
							
				
						}	
					
					
					
					
					
					
                    thisbutton.Value.Switch = pIsFadeEnabled;
                  
				
				}	
								
				
				if (hoverednew && buttonn == "Type")
                {
					
					pIsCrossEnabled = !pIsCrossEnabled;
					
					
					
                    thisbutton.Value.Switch = pIsCrossEnabled;
                  
				
				}	
								
			
					
					if (hoverednew && buttonn == APEXButton)
					{
						
						url = "https://apextraderfunding.com/member/aff/go/jrwyse?c=DVUDKBFF&keyword=button";									
								   
								
						//Print(url);
								 
								
						
						System.Diagnostics.Process.Start(url);
						
					}	
					
					

			
                   
				
				
				
				
				

				// added on 7/14/2023 only one button can be clicked at a time anyways
				
				if (hoverednew)
				{
				
					if (RedoAccountsTable)
					{
						AdjustmentRows = 0;
				 		GetAllPerformanceStats();
						
						//Print(FlattenEverythingClickedTotal);
						
						if (RefreshPositionsClicked)
						{
							RefreshPositionsClicked = false;
							
							if (pShowFEMessage)
								AddMessage("Refresh Positions was executed.  Your positions in this window, or in the Control Center, Positions tab, should now be refreshed with Rithmic, Tradovate, or other external connection.", 30, GOLDENROD);
							else
								AddMessage("Refresh Positions was executed.  Your positions should now be refreshed with the external connection.", 30, GOLDENROD);	
						}
						
						
						if (FlattenEverythingClicked)
						if (FlattenEverythingClickedTotal > 0 || FlattenEverythingClickedOrders > 0)
						{
						
							if (pShowFEMessage)
							{
								AddMessage("All accounts visible in this window should now be flat.  If NinjaTrader is still displaying positions in this window, or in the Control Center, Positions tab, please double check your true positions in Rithmic, Tradovate, or other external connection.", 30, RED);	
							}
							else
							{
								
								
								if (!IsKorean)
									AddMessage("Flatten Everything was executed.  All positions are closed and all pending orders are cancelled.", 30, RED);	
								else
									AddMessage("모든 포지션이 청산되었습니다. 감지된 포지션이나 대기 중인 주문이 없습니다", 6, RED);
								
								}

						}
						else
						{
							if (pShowFEMessage)
							{
								if (!IsKorean)
									AddMessage("Flatten Everything was executed.  No positions or pending orders were detected.", 6, RED);
								else
									AddMessage("모든 포지션이 청산되었습니다. 감지된 포지션이나 대기 중인 주문이 없습니다", 6, RED);
									
							}
							FlattenEverythingClickedTime = DateTime.MinValue;
						}
						
						
						FlattenEverythingClicked = false;
						
						
					}
				
					ChartControl.InvalidateVisual();
					break;
				}
				

				
			}
			
			
			if (SelectedButtonNow == SelectedButtonNowPre)
			{
				SelectedButtonNow = string.Empty;
				SelectedButtonTime = DateTime.MinValue;	
				
				ChartControl.InvalidateVisual();				
			}

			SelectedButtonNowPre = SelectedButtonNow;
				
			 
			if (SelectedResetColumn == SelectedResetColumnPre)
			{
				SelectedResetColumn = string.Empty;
				SelectedResetTime = DateTime.MinValue;	
				
				ChartControl.InvalidateVisual();				
			}
			
			SelectedResetColumnPre = SelectedResetColumn;
		
			
			
//			Print("SelectedResetColumn  " + SelectedResetColumn);
		}
		
		
		
		
		
		private string SelectedButtonNowPre = string.Empty;
		private string SelectedResetColumnPre = string.Empty;
		
		
		
		
		private int HoveredRow = 0;
		private int HoveredRowP = 0;
		
		private int HoveredButton = 0;
		private int HoveredButtonP = 0;		
		private int HoveredButton2 = 0;
		private int HoveredButton2P = 0;	
		private int HoveredButton3 = 0;
		private int HoveredButton3P = 0;			
		
		internal void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    	{
			
			
//			chartWindow.Focus();		
//			ChartControl.Focus();
			
			
            this.MP = e.GetPosition(this.ChartPanel);
			
			FinalXPixel = MP.X / 100 * dpiX;
			FinalYPixel = MP.Y / 100 * dpiY;
         
			
			bool DoRefreshAtEnd = false;
			
			
			//Print("FinalXPixel "+ FinalXPixel);
			
			//int ThisHoveredButton = 0;
			
			
			
			

			// row titles
			HoveredButton3 = 0;
			
			foreach (KeyValuePair<double, PendingDetails> kvp in LimitPriceToTimeOutOfSync2)
			{
					
				bool hoverednew = MouseIn(kvp.Value.RectCancel, 0, 0);	
				
				double price = kvp.Key;
				
				
				
				if (hoverednew)
				{
					HoveredButton3++;
					//CancelOrdersAtPrice(price);
					
					
					//Print("RectCancel");
					break;	
				}
					
				hoverednew = MouseIn(kvp.Value.RectFill, 0, 0);	
					
				if (hoverednew)
				{
					HoveredButton3++;
					//FillOrdersAtPrice(price);
					
					//Print("RectFill");
					
					break;
				}
				
				hoverednew = MouseIn(kvp.Value.RectPlus, 0, 0);	
					
				if (hoverednew)
				{
					HoveredButton3++;
					
					//MoveOrdersAtPrice(price);
					
					//Print("RectPlus");
					break;	
				}			
					
			}
			
				
				
				foreach (MessageAlerts D in AllMessages.ToList())
				{
					if (MouseIn(D.Rect,0,0))// || MouseIn(thisbutton.Value.Rect,exmbl,exmbl))
						{
							
							if (D.Switch)
							{
								HoveredButton3++;
								
								//doneeee = D.Name;
							}
							
							
						}
					
				}
				
				
				
			
		
			if (HoveredButton3P != HoveredButton3)
				DoRefreshAtEnd = true;
				
			HoveredButton3P = HoveredButton3;	
				
		
			
			
			HoveredButton = 0;
			
			if (pHighlightHoverButtons)
            foreach (KeyValuePair<double, ButtonBOT> thisbutton in AllButtonZ5)
            {
				
				bool hoverednew = MouseIn(thisbutton.Value.Rect, 0, 0);
				
				HoveredButton++;
			
				if (hoverednew)
				{
					//HoveredButton = ThisHoveredButton;
					break;
				}
	
			
			}
		
			if (HoveredButtonP != HoveredButton)
				DoRefreshAtEnd = true;
				
			HoveredButtonP = HoveredButton;	
				
		
			
			
			

			//ThisHoveredButton = 0;
			HoveredButton2 = 0;

			if (pHighlightHoverButtons)
            foreach (KeyValuePair<double, ButtonBOT> thisbutton in AllButtonZ7)
            {
				
				bool hoverednew = MouseIn(thisbutton.Value.Rect, 0, 0);
				
				HoveredButton2++;
			
				if (hoverednew)
				{
					//HoveredButton2 = ThisHoveredButton;
					break;
				}
	
			
			}
		
			if (HoveredButton2P != HoveredButton2)
				DoRefreshAtEnd = true;
				
			HoveredButton2P = HoveredButton2;	
			
			
			
			
					
			
			//int ThisHoveredRow = 0;
			 
			HoveredRow = 0;
			bool NoRow = true;
			
			if (pHighlightHoverCells)
			if (FinalXPixel <= rightoftable)
			foreach (KeyValuePair<double, ButtonBOT> thisbutton in AllButtonZ6)
           // foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ6)
                {
						
					
					//Print(thisbutton.Value.Rect.Top);
					
					
                    bool hoverednew = MouseIn(thisbutton.Value.Rect, 0, 0);
                  //  bool hoverednow = thisbutton.Value.Hovered;

					//HoveredRow = HoveredRow + 1;
					
					HoveredRow = (int) thisbutton.Key;
					
                    if (hoverednew)
                    {
                       
						NoRow = false;
						break;
						
					}
					
					
						
					

                }
				
				if (NoRow)
					HoveredRow = 0;
				
				if (HoveredRowP != HoveredRow)
				{
					DoRefreshAtEnd = true;
				}
				
			HoveredRowP = HoveredRow;	
				
				
			
            foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
                {
                    bool hoverednew = MouseIn(thisbutton.Value.Rect, 2, 2);
                    bool hoverednow = thisbutton.Value.Hovered;

                    if (hoverednew && !hoverednow)
                    {
                        thisbutton.Value.Hovered = true;
                        ChartControl.InvalidateVisual();
						
						//Print("ref");
                    }
                    if (!hoverednew && hoverednow)
                    {
                        thisbutton.Value.Hovered = false;
                        ChartControl.InvalidateVisual();
						
						//Print("ref");
						
                    }

                }

                InMenuP = InMenu;
                InMenu = MouseIn(B2, 8, 8);
            
				if (InMenu != InMenuP)
					DoRefreshAtEnd = true;
				
				if (DoRefreshAtEnd)
				{
					
               	
					ChartControl.InvalidateVisual();
				}
	
		}
		
        private bool MouseIn(SharpDX.RectangleF RR, int XF, int YF)
        {
            //Print(RR.Left);
            
			if (FinalXPixel != 0)
            if (FinalXPixel >= RR.Left - XF && FinalXPixel <= RR.Right + XF && FinalYPixel >= RR.Top - YF && FinalYPixel <= RR.Bottom + YF)
                return true;
           
                return false;

        }

		//private void DrawOrders (SharpDX.Direct2D1.Brush LastPriceBrushDX, double CurrentLast)
		
		private void FillRectBoth(SharpDX.RectangleF RR, int BrushZ)
		{
			RenderTarget.FillRectangle(RR, br[BrushZ].DxBrush); // new
//			AddDrawing(AllDrawings,"FillRectangle", RR, BrushZ, br[BrushZ].DxBrush.Opacity);
		}
		
		
	
       private void AddDrawing( List<Drawings> ThisList, string iText, SharpDX.RectangleF Rect2, int BrushZ, float Opacity)
        {
            Drawings Z = new Drawings();
            Z.Text = iText;
           // Z.Name = iName;
            //Z.Width = iWidth;
           // Z.Switch = iSwitch;
            Z.Rect = Rect2;
           // Z.Hovered = false;
			//Z.Location = iLocation;
			Z.ThisB = BrushZ;
			Z.ThisBOpacity = Opacity;
            ThisList.Add(Z);
        }
			
		
		
		
		
       private void AddButtonZ( SortedDictionary<double, ButtonZ> ThisList, string iText, string iName, int iWidth, bool iSwitch, string iLocation, int iOnColor, int iOffColor,
			int iOnHoverColor, int iOffHoverColor, int iOutlineColor)
        {
            ButtonZ Z = new ButtonZ();
            Z.Text = iText;
            Z.Name = iName;
            Z.Width = iWidth;
            Z.Switch = iSwitch;
            Z.Rect = new SharpDX.RectangleF(0, 0, 0, 0);
            Z.Hovered = false;
			Z.Location = iLocation;
			Z.BrushOn = iOnColor;
			Z.BrushOff = iOffColor;
			Z.BrushOnHover = iOnHoverColor;
			Z.BrushOffHover = iOffHoverColor;
			Z.BrushOutlineHover = iOutlineColor;
//p($"added buzzonz:  text {iText}  name {iName}   On:{Z.BrushOn}  Off:{Z.BrushOff}");
            ThisList.Add(ThisList.Count + 1, Z);

        }
						
       private void AddButtonZ5( SortedDictionary<double, ButtonBOT> ThisList, PNLStatistics BOTC, SharpDX.RectangleF zzre, string sss)
        {
            ButtonBOT Z = new ButtonBOT();
            Z.Text = BOTC;
     		Z.Name = sss;
            Z.Rect = zzre;
           

            ThisList.Add(ThisList.Count + 1, Z);

        }			
		
       private void AddButtonZ6( SortedDictionary<double, ButtonBOT> ThisList, PNLStatistics BOTC, SharpDX.RectangleF zzre, string sss, int iiii)
        {
            ButtonBOT Z = new ButtonBOT();
            Z.Text = BOTC;
     		Z.Name = sss;
            Z.Rect = zzre;
           

            //ThisList.Add(ThisList.Count + 1, Z);
			
			ThisList.Add(iiii, Z);

        }			
		
						
//		public override string FormatPriceMarker(double price)
//		{
			
//			return "";
//		}			
		
		public override void OnCalculateMinMax()
		{
			
			// shortens width of y axis area
			
			MaxValue = 1;
			MinValue = 2;
			
			
//			try
//			{
				
				

//				Print(MinValue);
//				Print(MaxValue);
				
				
				
				
				
//				if (!Permission)
//					return;
				
//					// make sure to always start fresh values to calculate new min/max values
//				double tmpMin = double.MaxValue;
//				double tmpMax = double.MinValue;
//				 
				
				
//			}
			
				
//			catch (Exception ex)
//			{
//				//if (TestRender) Print("OnCalculateMinMax: " + ex.Message + " ");
				
//			}
			
			
		}
		
		
		#region -- Properties and Parameters --

		internal class EXM : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				//return new StandardValuesCollection( new String[] {"None", "End Of Day", "Forever"} );
				return new StandardValuesCollection( new String[] {"None", "End Of Day"} );
			}
		}	
		
		
		

		
        private int pThisBarPeriod1 = 1;
//        [Range(1, int.MaxValue)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Minute Bars", GroupName = "Minute Data 1", Order = 32)]
//        public int ThisBarPeriod1
//        {
//            get { return pThisBarPeriod1; }
//            set { pThisBarPeriod1 = value; }
//        }		



		private BarsPeriodType AcceptableBasePeriodType1 = BarsPeriodType.Minute;

		private Dictionary<int, DXMediaMap> br = new Dictionary<int, DXMediaMap>();
		[Browsable(false)]
		public class DXMediaMap
		{
			public SharpDX.Direct2D1.Brush		DxBrush = null;
			public System.Windows.Media.Brush	MediaBrush = Brushes.White;
			public float InitialOpacity = 1f;
			public DXMediaMap(){
			}
			public DXMediaMap(Brush b){
				MediaBrush = b;
			}
			public DXMediaMap(Brush b, float opacity = 1f){
				MediaBrush = b;
				InitialOpacity = opacity;
			}
		}


		private string pSelectedColumn = "Account";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Label", Description = "", GroupName = "High / Low Labels", Order = 4)]
        public string SelectedColumn
        {
            get { return pSelectedColumn; }
            set { pSelectedColumn = value; }
        }	




		
		
		
			private TimeSpan pCustomETime = new TimeSpan(7,30,0);
		private TimeSpan pCustomTime = new TimeSpan(7,30,0);
//		[Description("Enter the start time of the range.")]
//		[Gui.Design.DisplayNameAttribute("\r\rTime Begin")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		public string TimeBegin
//		{
//			get { return pStartTime.Hours.ToString("0")+":"+pStartTime.Minutes.ToString("00");}
//			set { if(!TimeSpan.TryParse(value, out pStartTime)) pStartTime=new TimeSpan(0,0,0); }
//		}
		
		private TimeSpan pStartTime = new TimeSpan(9,30,0);
//		[Description("Enter the start time of the range.")]
//		[Gui.Design.DisplayNameAttribute("\r\rTime Begin")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		public string TimeBegin
//		{
//			get { return pStartTime.Hours.ToString("0")+":"+pStartTime.Minutes.ToString("00");}
//			set { if(!TimeSpan.TryParse(value, out pStartTime)) pStartTime=new TimeSpan(0,0,0); }
//		}

		private TimeSpan pEndTime = new TimeSpan(16,15,0);
//		[Description("Enter the end time of the range.")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("Time End")]
//		public string TimeEnd
//		{
//			get { return pEndTime.Hours.ToString("0")+":"+pEndTime.Minutes.ToString("00");}
//			set { if(!TimeSpan.TryParse(value, out pEndTime)) pEndTime=new TimeSpan(0,0,0); }
//		}
		
//		private TimeSpan pR1Time = new TimeSpan(10,35,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 1")]
//		public string R1Time
//		{
//			get { return pR1Time.Hours.ToString("0")+":"+pR1Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR1Time)) pR1Time=new TimeSpan(0,0,0); }
//		}
		

		private TimeSpan pR2Time = new TimeSpan(11,15,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 2")]
//		public string R2Time
//		{
//			get { return pR2Time.Hours.ToString("0")+":"+pR2Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR2Time)) pR2Time=new TimeSpan(0,0,0); }
//		}
		
		private TimeSpan pR3Time = new TimeSpan(12,20,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 3")]
//		public string R3Time
//		{
//			get { return pR3Time.Hours.ToString("0")+":"+pR3Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR3Time)) pR3Time=new TimeSpan(0,0,0); }
//		}
		
		private TimeSpan pR4Time = new TimeSpan(13,25,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 4")]
//		public string R4Time
//		{
//			get { return pR4Time.Hours.ToString("0")+":"+pR4Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR4Time)) pR4Time=new TimeSpan(0,0,0); }
//		}
		
		private TimeSpan pR5Time = new TimeSpan(14,05,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 5")]
//		public string R5Time
//		{
//			get { return pR5Time.Hours.ToString("0")+":"+pR5Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR5Time)) pR5Time=new TimeSpan(0,0,0); }
//		}
				
		private TimeSpan pR6Time = new TimeSpan(15,10,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 6")]
//		public string R6Time
//		{
//			get { return pR6Time.Hours.ToString("0")+":"+pR6Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR6Time)) pR6Time=new TimeSpan(0,0,0); }
//		}
		
		private TimeSpan pR7Time = new TimeSpan(16,10,0);
//		[Description("Minutes and Seconds")]
//		[GridCategory("\t\t\t\t\t\t\tParameters")]
//		[Gui.Design.DisplayNameAttribute("\rTime 7")]
//		public string R7Time
//		{
//			get { return pR7Time.Hours.ToString("0")+":"+pR7Time.Minutes.ToString("00"); }
//			set { if(!TimeSpan.TryParse(value, out pR7Time)) pR7Time=new TimeSpan(0,0,0); }
//		}
		
		private TimeSpan pR8Time = new TimeSpan(17,10,0);
		
		private bool pShowBorderWarning = false;
		
		
		
		private bool pAnalyticsEnabled = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Window Display", Order = -100)]
//        public bool AnalyticsEnabled
//        {
//            get { return pAnalyticsEnabled; }
//            set { pAnalyticsEnabled = value; }
//        }	
		
		
		private bool pLimitOrderSyncFeatures = true;
		
		private bool pChartTraderNeedsDisabled = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Rejected Order Handling", Order = 0)]
    //   [RefreshProperties(RefreshProperties.All)]
		public bool ChartTraderNeedsDisabled
        {
            get { return pChartTraderNeedsDisabled; }
            set { pChartTraderNeedsDisabled = value; }
        }	
			
		
		
		private bool pWindowPrivacyAccounts = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Name Button Enabled", GroupName = "Account Details Privacy", Order = 0)]
     //  [RefreshProperties(RefreshProperties.All)]
		public bool WindowPrivacyAccounts
        {
            get { return pWindowPrivacyAccounts; }
            set { pWindowPrivacyAccounts = value; }
        }	
		
		private bool pWindowPrivacyCurrency = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Values Button Enabled", GroupName = "Account Details Privacy", Order = 2)]
     //  [RefreshProperties(RefreshProperties.All)]
		public bool WindowPrivacyCurrency
        {
            get { return pWindowPrivacyCurrency; }
            set { pWindowPrivacyCurrency = value; }
        }	
				
		
		
		
			
		private string pCurrencyPrivacy = "Balances";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Details Privacy", Name = "Values To Hide", Description = "",  Order = 82)]
		[TypeConverter(typeof(BodsfgfhyMode2233423212))]
		public string CurrencyPrivacy
		{
			get { return pCurrencyPrivacy; }
			set { pCurrencyPrivacy = value; }
		}					
						
		
		
		

		internal class BodsfgfhyMode2233423212 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Balances", "Everything"} );
			}
		}			
			
		
		
		
		
		
		
		
		
		private bool pShowAccountsButton = false;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Accounts Type Button", GroupName = "Functionality", Order = -100)]
//        public bool ShowAccountsButton
//        {
//            get { return pShowAccountsButton; }
//            set { pShowAccountsButton = value; }
//        }		
		
		
		
		private bool pRejectedOrderHandling = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Rejected Order Handling", Order = 0)]
       [RefreshProperties(RefreshProperties.All)]
		public bool RejectedOrderHandling
        {
            get { return pRejectedOrderHandling; }
            set { pRejectedOrderHandling = value; }
        }	
		
		private bool pCheckBeforeSubmitting = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Check Before Submit", GroupName = "Rejected Order Handling", Order = 2)]
		public bool CheckBeforeSubmitting
        {
            get { return pCheckBeforeSubmitting; }
            set { pCheckBeforeSubmitting = value; }
        }			
	
			
		
		private string pRejectedSubmit = "Limit";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "New Order Type", GroupName = "Rejected Order Handling", Order = 5)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(pReaasdfjectedSubmit))]
		public string RejectedSubmit
		{
			get { return pRejectedSubmit; }
			set { pRejectedSubmit = value; }
		}
		

        private int pRejectedSubmitOff = 0;
        [Range(-10, int.MaxValue)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Limit Offset (Ticks)", Description = "", GroupName = "Rejected Order Handling", Order = 6)]
        public int RejectedSubmitOff
        {
            get { return pRejectedSubmitOff; }
            set { pRejectedSubmitOff = value; }
        }		
		
		private bool pMatchStopLimit = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Match Stop Limit Offset", GroupName = "Rejected Order Handling", Order = 7)]
		public bool MatchStopLimit
        {
            get { return pMatchStopLimit; }
            set { pMatchStopLimit = value; }
        }	
				
		
		
		private bool pResubmitMaster = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Master Account Tracking", GroupName = "Rejected Order Handling", Order = 10)]

		public bool ResubmitMaster
        {
            get { return pResubmitMaster; }
            set { pResubmitMaster = value; }
        }	
		
		
		
		
		
		
		internal class pReaasdfjectedSubmit : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Limit", "Market"} );
			}
		}	
							
		
		
		
		
		private bool pSyncLimitEntry = false;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "All Entry Orders", GroupName = "Smart Synchronize", Order = -100)]
//        public bool SyncLimitEntry
//        {
//            get { return pSyncLimitEntry; }
//            set { pSyncLimitEntry = value; }
//        }		
		
	private int pFontSizeTopRow2 = 0;
//        [Range(int.MinValue, int.MaxValue)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Font Size Adjustment", GroupName = "Smart Synchronize", Order = 31)]
//        public int FontSizeTopRow2
//        {
//            get { return pFontSizeTopRow2; }
//            set { pFontSizeTopRow2 = value; }
//        }		
						
		
		
		
		private bool pExitShieldFeaturesEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Exit Shield", Order = 1)]
		[RefreshProperties(RefreshProperties.All)]
        public bool ExitShieldFeaturesEnabled
        {
            get { return pExitShieldFeaturesEnabled; }
            set { pExitShieldFeaturesEnabled = value; }
        }	
		
		
		
		private bool pExitShieldButtonEnabled = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Button Enabled", GroupName = "Exit Shield", Order = 2)]
        public bool ExitShieldButtonEnabled
        {
            get { return pExitShieldButtonEnabled; }
            set { pExitShieldButtonEnabled = value; }
        }		
		
		
			
		
		
		
		private bool pExitShieldStopLoss = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss Enabled", GroupName = "Exit Shield", Order = 3)]
        public bool ExitShieldStopLoss
        {
            get { return pExitShieldStopLoss; }
            set { pExitShieldStopLoss = value; }
        }		
		
		
		private bool pExitShieldProfitTarget = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target Enabled", GroupName = "Exit Shield", Order = 4)]
        public bool ExitShieldProfitTarget
        {
            get { return pExitShieldProfitTarget; }
            set { pExitShieldProfitTarget = value; }
        }			
				
	
		
		
		private bool pUseOriginalLocation = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Keep Original Location", GroupName = "Exit Shield", Order = 5)]
        public bool UseOriginalLocation
        {
            get { return pUseOriginalLocation; }
            set { pUseOriginalLocation = value; }
        }		
		
		private bool pExitShieldMessages = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Status Messages", GroupName = "Exit Shield", Order = 100)]
        public bool ExitShieldMessages
        {
            get { return pExitShieldMessages; }
            set { pExitShieldMessages = value; }
        }	
		
		
		
	
		private bool pExitShieldIsEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Main Enabled", GroupName = "Exit Shield", Order = 100)]
        public bool ExitShieldIsEnabled
        {
            get { return pExitShieldIsEnabled; }
            set { pExitShieldIsEnabled = value; }
        }					
		
		
		
		private bool pHideAccountsIsEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "CopierIsEnabled", GroupName = "Main Switch", Order = -100)]
        public bool HideAccountsIsEnabled
        {
            get { return pHideAccountsIsEnabled; }
            set { pHideAccountsIsEnabled = value; }
        }		
		
		private bool pHideCurrencyIsEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "CopierIsEnabled", GroupName = "Main Switch", Order = -100)]
        public bool HideCurrencyIsEnabled
        {
            get { return pHideCurrencyIsEnabled; }
            set { pHideCurrencyIsEnabled = value; }
        }				
		
		
		
		private bool pCopierIsEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "CopierIsEnabled", GroupName = "Main Switch", Order = -100)]
        public bool CopierIsEnabled
        {
            get { return pCopierIsEnabled; }
            set { pCopierIsEnabled = value; }
        }		

		private bool pSelectedAscending = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "All Instruments", Order = -100)]
        public bool SelectedAscending
        {
            get { return pSelectedAscending; }
            set { pSelectedAscending = value; }
        }		

		
		private bool pAllInstruments = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "AllInstruments", GroupName = "All Instruments", Order = -100)]
        public bool AllInstruments
        {
            get { return pAllInstruments; }
            set { pAllInstruments = value; }
        }		

		
		
		
		
		private bool pIsCrossEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "IsCrossEnabled", GroupName = "All Instruments", Order = -100)]
        public bool IsCrossEnabled
        {
            get { return pIsCrossEnabled; }
            set { pIsCrossEnabled = value; }
        }		

		private bool pIsXEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "IsXEnabled", GroupName = "All Instruments", Order = -100)]
        public bool IsXEnabled
        {
            get { return pIsXEnabled; }
            set { pIsXEnabled = value; }
        }			
		
		private bool pIsATMSelectEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "pIsATMSelectEnabled", GroupName = "All Instruments", Order = -100)]
        public bool IsATMSelectEnabled
        {
            get { return pIsATMSelectEnabled; }
            set { pIsATMSelectEnabled = value; }
        }			

		private bool pIsFadeEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "IsFadeEnabled", GroupName = "All Instruments", Order = -100)]
        public bool IsFadeEnabled
        {
            get { return pIsFadeEnabled; }
            set { pIsFadeEnabled = value; }
        }			
				
		
		
		
		
		
		
		private bool pIsBuildMode = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "IsFadeEnabled", GroupName = "All Instruments", Order = -100)]
        public bool IsBuildMode
        {
            get { return pIsBuildMode; }
            set { pIsBuildMode = value; }
        }				
		
	

//			private bool pIsCopyBasicFunctionsChecked = true;
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Duplicate Account Actions", Order = -9)]
//	        public bool IsCopyBasicFunctionsChecked
//	        {
//	            get { return pIsCopyBasicFunctionsChecked; }
//	            set { pIsCopyBasicFunctionsChecked = value; }
//	        }			
		
			
			
			
			
		
	
	
		private bool pIsXColumnEnabled = true;
		//[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Size Adjustment Enabled", Order = 5)]
        public bool IsXColumnEnabled
        {
            get { return pIsXColumnEnabled; }
            set { pIsXColumnEnabled = value; }
        }			
		
	
		
		internal class BodyMode22332331 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Multiplier", "Quantity"} );
			}
		}	
		
		private string pMultiplierMode = "Multiplier";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Size Calculation",  Order = 6, Description = "")]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode22332331))]
		public string MultiplierMode
		{
			get { return pMultiplierMode; }
			set { pMultiplierMode = value; }
		}
		
		
		private string pLastMultiplierMode = "Multiplier";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "-------",  Order = 6, Description = "")]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode22332331))]
		public string LastMultiplierMode
		{
			get { return pLastMultiplierMode; }
			set { pLastMultiplierMode = value; }
		}		
		
		

		private bool pIsATMColumnEnabled = true;
		//[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Mode Adjustment Enabled", Order = 10, Description = "")]
        public bool IsATMColumnEnabled
        {
            get { return pIsATMColumnEnabled; }
            set { pIsATMColumnEnabled = value; }
        }			
		
		

		private string pTheCopierMode = "Executions";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Default Mode", Order = 12, Description = "")]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(DGDisplayModeSDS))]
		public string TheCopierMode
		{
			get { return pTheCopierMode; }
			set { pTheCopierMode = value; }
		}
		
		
		
		
		internal class DGDisplayModeSDS : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Executions", "Orders"} );
			}
		}	
					
					
		
		
		private bool pIsCrossColumnEnabled = true;
		//[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Type Adjustment Enabled", Order = 30, Description = "")]
        public bool IsCrossColumnEnabled
        {
            get { return pIsCrossColumnEnabled; }
            set { pIsCrossColumnEnabled = value; }
        }		

		
		
		private string pFDAXMini = "FDXM";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "FDAX - Mini", Description = "",  Order = 31)]
		[TypeConverter(typeof(FDAXMiniOptions))]
		public string FDAXMini
		{
			get { return pFDAXMini; }
			set { pFDAXMini = value; }
		}					
						
	
		internal class FDAXMiniOptions : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"FDAX", "FDXM"} );
			}
		}			
		
		private string pFDAXMicro = "FDXS";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "FDAX - Micro", Description = "",  Order = 33)]
		[TypeConverter(typeof(FDAXMicroOptions))]
		public string FDAXMicro
		{
			get { return pFDAXMicro; }
			set { pFDAXMicro = value; }
		}					
						
	
		internal class FDAXMicroOptions : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"FDXM", "FDXS"} );
			}
		}			
		
		
		
		
		
		
	//instr.Replace("FDAX", "FDXS");
		
		
		
		
		private string pCrossType = "Micro";
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Default Contract Type", Order = 31, Description = "")]
//		//[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(BodyMode22323))]
//		public string CrossType
//		{
//			get { return pCrossType; }
//			set { pCrossType = value; }
//		}

		internal class BodyMode22323 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Mini", "Micro"} );
			}
		}	
		
		
	
		
		
		private bool pIsFadeColumnEnabled = true;
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Duplicate Account Actions", Name = "Fade Adjustment Enabled", Order = 50)]
        public bool IsFadeColumnEnabled
        {
            get { return pIsFadeColumnEnabled; }
            set { pIsFadeColumnEnabled = value; }
        }			
			
		
		

		
		
		
		
			private bool pShowStatusMessages = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Status Messages", Order = 0)]
        public bool ShowStatusMessages
        {
            get { return pShowStatusMessages; }
            set { pShowStatusMessages = value; }
        }
		
		private int pFontSizeTopRow3 = 0;
        [Range(int.MinValue, int.MaxValue)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Font Size Adjustment", GroupName = "Status Messages", Order = 31)]
        public int FontSizeTopRow3
        {
            get { return pFontSizeTopRow3; }
            set { pFontSizeTopRow3 = value; }
        }		
				
		
		
		
		
		
		private bool pForceAllSelectedAccountsToTop = false; //always show master account and follower accounts 
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Functionality", Name = "Move Selected Accounts To Top", Description = "any account selected as master or follower account will move to the top of the list", Order = -10)]
        public bool ForceAllSelectedAccountsToTop
        {
            get { return pForceAllSelectedAccountsToTop; }
            set { pForceAllSelectedAccountsToTop = value; }
        }
				
		
	
		private string pCloseButtonLocation = "Left";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Functionality", Name = "Hide | Close | Cancel Location", Description = "",  Order = 1)]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodsfgfhyMode2233423))]
		public string CloseButtonLocation
		{
			get { return pCloseButtonLocation; }
			set { pCloseButtonLocation = value; }
		}		
		
		private bool pShowHideButton = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Functionality", Name = "Hide Button Enabled", Description = "", Order = 2)]
        public bool ShowHideButton
        {
            get { return pShowHideButton; }
            set { pShowHideButton = value; }
        }	
		
		private bool pCloseButtonEnabled = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Functionality", Name = "Close Button Enabled", Description = "", Order = 3)]
        public bool CloseButtonEnabled
        {
            get { return pCloseButtonEnabled; }
            set { pCloseButtonEnabled = value; }
        }
		

		private bool pCancelButtonEnabled = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Functionality", Name = "Cancel Button Enabled", Description = "", Order = 4)]
        public bool CancelButtonEnabled
        {
            get { return pCancelButtonEnabled; }
            set { pCancelButtonEnabled = value; }
        }
		
	
		private bool pOneTradeAll = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "One Trade", Name = "Enabled", Description = "show one trade button features to trade in and out, satisfying the requirement for a day traded with prop firms", Order = 70)]
		[RefreshProperties(RefreshProperties.All)]
        public bool OneTradeAll
        {
            get { return pOneTradeAll; }
            set { pOneTradeAll = value; }
        }			
		
		
		private bool pOneTradeEvalEnabled = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "One Trade", Name = "Evaluation Accounts", Description = "show one trade button for Evaluation accounts, when Qty for the current day = 0", Order = 80)]
        public bool OneTradeEvalEnabled
        {
            get { return pOneTradeEvalEnabled; }
            set { pOneTradeEvalEnabled = value; }
        }					

	
		
		private bool pOneTradePAEnabled = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "One Trade", Name = "Performance Accounts", Description = "show one trade button for Performance accounts, when Qty for the current day = 0", Order = 81)]
        public bool OneTradePAEnabled
        {
            get { return pOneTradePAEnabled; }
            set { pOneTradePAEnabled = value; }
        }				
		
		
		
			
		private string pOneTradeShow = "Totals";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "One Trade", Name = "One Trade - Rows", Description = "",  Order = 82)]
		[TypeConverter(typeof(BodsfgfhyMode223342322))]
		public string OneTradeShow
		{
			get { return pOneTradeShow; }
			set { pOneTradeShow = value; }
		}					
						
		
		
		

		internal class BodsfgfhyMode223342322 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Accounts", "Totals", "Both"} );
			}
		}			
			
	
		
		
		
		
		private bool pHideExtraAccountsOnLock = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Lock - Remove Extra Accounts", Description = "hide all extra accounts that are not configured as a master account or follower account when the lock button is clicked", GroupName = "Functionality", Order = 11)]
        public bool HideExtraAccountsOnLock
        {
            get { return pHideExtraAccountsOnLock; }
            set { pHideExtraAccountsOnLock = value; }
        }	
						
	
		private bool pLockAccountsOnLock = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Lock - Account Master / Follower Status", Description = "lock the master and follower status of all accounts when the lock button is clicked", GroupName = "Functionality", Order = 12)]
        public bool LockAccountsOnLock
        {
            get { return pLockAccountsOnLock; }
            set { pLockAccountsOnLock = value; }
        }	
				
	
	
		private bool pLockHideAccountsOnLock = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Lock  - Account Hidden Status", Description = "lock the hidden status of all accounts when the lock button is clicked", GroupName = "Functionality", Order = 13)]
        public bool LockHideAccountsOnLock
        {
            get { return pLockHideAccountsOnLock; }
            set { pLockHideAccountsOnLock = value; }
        }	
		
		
		private bool pShowFollowersAtTop = false;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Visible - Follower Accounts", Description = "make sure that follower accounts are always visible in the table of accounts.", GroupName = "Functionality", Order = 14)]
//        public bool ShowFollowersAtTop
//        {
//            get { return pShowFollowersAtTop; }
//            set { pShowFollowersAtTop = value; }
//        }	
		
		private bool pResetDGDL = true;
//		[RefreshProperties(RefreshProperties.All)]
//		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Reset Button", Name = "Delete Daily Goal & Daily Loss", Description = "", Order = 1)]
//        public bool ResetDGDL
//        {
//            get { return pResetDGDL; }
//            set { pResetDGDL = value; }
//        }			

		private bool pShowRefreshPositions = true;
		[RefreshProperties(RefreshProperties.All)]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Refresh Positions Button", Name = "Enabled", Description = "", Order = 1)]
        public bool ShowRefreshPositions
        {
            get { return pShowRefreshPositions; }
            set { pShowRefreshPositions = value; }
        }	
		
		private string pRPString = "Refresh Positions";
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Refresh Positions Button", Name = "Label", Description = "", Order = 2)]
        public string RPString
        {
            get { return pRPString; }
            set { pRPString = value; }
        }
		
		
		private bool pKeyboardEnabled = true;
		[RefreshProperties(RefreshProperties.All)]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Keyboard", Name = "Enabled", Description = "", Order = 1)]
        public bool KeyboardEnabled
        {
            get { return pKeyboardEnabled; }
            set { pKeyboardEnabled = value; }
        }	
		
		private string pKeyScrollUp = "NumPad8";
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Keyboard", Name = "Scroll Up", Description = "", Order = 3)]
        public string KeyScrollUp
        {
            get { return pKeyScrollUp; }
            set { pKeyScrollUp = value; }
        }	
		
		
		private string pKeyScrollDn = "NumPad2";
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Keyboard", Name = "Scroll Down", Description = "", Order = 4)]
        public string KeyScrollDn
        {
            get { return pKeyScrollDn; }
            set { pKeyScrollDn = value; }
        }	
		
		
		private bool pUseNumberForMultiplier = true;

		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Keyboard", Name = "Size Column Enabled", Description = "when a cell is clicked in the size column, using number keys from 1-9 or 0=10 immediately sets the number. otherwise, using the Scroll Up and Scroll Down keys will move the number up or down.", Order = 10)]
        public bool UseNumberForMultiplier
        {
            get { return pUseNumberForMultiplier; }
            set { pUseNumberForMultiplier = value; }
        }
		
		
		
		

		
		private Brush pRefreshButtonB = Brushes.SaddleBrown;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Refresh Positions Button", Name = "Color", Order = 12)]
		public Brush RefreshButtonB
		{
			get { return pRefreshButtonB; } set { pRefreshButtonB = value; }
		}
		[Browsable(false)]
		public string RefreshButtonBS
		{
			get { return Serialize.BrushToString(pRefreshButtonB); } set { pRefreshButtonB = Serialize.StringToBrush(value); }
		}		
		

	
		private bool pShowFRPMessage = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Refresh Positions Button", Name = "Status Message Enabled", Description = "", Order = 13)]
        public bool ShowFRPMessage
        {
            get { return pShowFRPMessage; }
            set { pShowFRPMessage = value; }
        }				
		
		
		
		
		
		private bool pShowFlattenEverything = true;
		[RefreshProperties(RefreshProperties.All)]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Flatten Everything Button", Name = "Enabled", Description = "", Order = 1)]
        public bool ShowFlattenEverything
        {
            get { return pShowFlattenEverything; }
            set { pShowFlattenEverything = value; }
        }

	
		
		private string pFlattenButtonString = "FLATTEN ALL";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Label", Description = "", GroupName = "Flatten Everything Button", Order = 2)]
        public string FLString
        {
            get { return pFlattenButtonString; }
            set { pFlattenButtonString = value; }
        }	
		
			
		
		private Brush pFlattenButtonB = Brushes.Maroon;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color", GroupName = "Flatten Everything Button", Order = 12)]
		public Brush FlattenButtonB
		{
			get { return pFlattenButtonB; } set { pFlattenButtonB = value; }
		}
		[Browsable(false)]
		public string FlattenButtonBS
		{
			get { return Serialize.BrushToString(pFlattenButtonB); } set { pFlattenButtonB = Serialize.StringToBrush(value); }
		}	
		

				
		
	
		private bool pShowFEMessage = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Flatten Everything Button", Name = "Status Message Enabled", Description = "", Order = 13)]
        public bool ShowFEMessage
        {
            get { return pShowFEMessage; }
            set { pShowFEMessage = value; }
        }		
		
		
	
		
		private bool pShowCopyRiskButtons = false;
//		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Functionality", Name = "Flatten Everything Enabled", Description = "", Order = 3)]
//        public bool ShowFlattenEverything
//        {
//            get { return pShowFlattenEverything; }
//            set { pShowFlattenEverything = value; }
//        }
		
					
	
		private bool pPrivacyEnabled = false;
			[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "enabled privacy features for account name display", GroupName = "Account Name Adjustments", Order = 0)]
        public bool PrivacyEnabled
        {
            get { return pPrivacyEnabled; }
            set { pPrivacyEnabled = value; }
        }	
		
		private string pAccountPrivacy1 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 1", Description = "", GroupName = "Account Name Adjustments", Order = 18)]
        public string AccountPrivacy1
        {
            get { return pAccountPrivacy1; }
            set { pAccountPrivacy1 = value; }
        }		
				
		private string pAccountReplace1 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 1", Description = "", GroupName = "Account Name Adjustments", Order = 19)]
        public string AccountReplace1
        {
            get { return pAccountReplace1; }
            set { pAccountReplace1 = value; }
        }			
		
		private string pAccountPrivacy2 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 2", Description = "", GroupName = "Account Name Adjustments", Order = 28)]
        public string AccountPrivacy2
        {
            get { return pAccountPrivacy2; }
            set { pAccountPrivacy2 = value; }
        }		
				
		private string pAccountReplace2 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 2", Description = "", GroupName = "Account Name Adjustments", Order = 29)]
        public string AccountReplace2
        {
            get { return pAccountReplace2; }
            set { pAccountReplace2 = value; }
        }										
							
		private string pAccountPrivacy3 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 3", Description = "", GroupName = "Account Name Adjustments", Order = 38)]
        public string AccountPrivacy3
        {
            get { return pAccountPrivacy3; }
            set { pAccountPrivacy3 = value; }
        }		
				
		private string pAccountReplace3 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 3", Description = "", GroupName = "Account Name Adjustments", Order = 39)]
        public string AccountReplace3
        {
            get { return pAccountReplace3; }
            set { pAccountReplace3 = value; }
        }										
	
		private string pAccountPrivacy4 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 4", Description = "", GroupName = "Account Name Adjustments", Order = 48)]
        public string AccountPrivacy4
        {
            get { return pAccountPrivacy4; }
            set { pAccountPrivacy4 = value; }
        }		
				
		private string pAccountReplace4 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 4", Description = "", GroupName = "Account Name Adjustments", Order = 49)]
        public string AccountReplace4
        {
            get { return pAccountReplace4; }
            set { pAccountReplace4 = value; }
        }				
		
		private string pAccountPrivacy5 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 5", Description = "", GroupName = "Account Name Adjustments", Order = 58)]
        public string AccountPrivacy5
        {
            get { return pAccountPrivacy5; }
            set { pAccountPrivacy5 = value; }
        }		
				
		private string pAccountReplace5 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 5", Description = "", GroupName = "Account Name Adjustments", Order = 59)]
        public string AccountReplace5
        {
            get { return pAccountReplace5; }
            set { pAccountReplace5 = value; }
        }	
		
		private string pAccountPrivacy6 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 6", Description = "", GroupName = "Account Name Adjustments", Order = 68)]
        public string AccountPrivacy6
        {
            get { return pAccountPrivacy6; }
            set { pAccountPrivacy6 = value; }
        }		
				
		private string pAccountReplace6 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 6", Description = "", GroupName = "Account Name Adjustments", Order = 69)]
        public string AccountReplace6
        {
            get { return pAccountReplace6; }
            set { pAccountReplace6 = value; }
        }										
	
		private string pAccountPrivacy7 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 7", Description = "", GroupName = "Account Name Adjustments", Order = 78)]
        public string AccountPrivacy7
        {
            get { return pAccountPrivacy7; }
            set { pAccountPrivacy7 = value; }
        }		
				
		private string pAccountReplace7 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 7", Description = "", GroupName = "Account Name Adjustments", Order = 79)]
        public string AccountReplace7
        {
            get { return pAccountReplace7; }
            set { pAccountReplace7 = value; }
        }				
		
		private string pAccountPrivacy8 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 8", Description = "", GroupName = "Account Name Adjustments", Order = 88)]
        public string AccountPrivacy8
        {
            get { return pAccountPrivacy8; }
            set { pAccountPrivacy8 = value; }
        }		
				
		private string pAccountReplace8 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 8", Description = "", GroupName = "Account Name Adjustments", Order = 89)]
        public string AccountReplace8
        {
            get { return pAccountReplace8; }
            set { pAccountReplace8 = value; }
        }
	
		
		private string pAccountPrivacy9 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 9", Description = "", GroupName = "Account Name Adjustments", Order = 91)]
        public string AccountPrivacy9
        {
            get { return pAccountPrivacy9; }
            set { pAccountPrivacy9 = value; }
        }		
							
		private string pAccountReplace9 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 9", Description = "", GroupName = "Account Name Adjustments", Order = 92)]
        public string AccountReplace9
        {
            get { return pAccountReplace9; }
            set { pAccountReplace9 = value; }
        }	
		
		private string pAccountPrivacy10 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 10", Description = "", GroupName = "Account Name Adjustments", Order = 101)]
        public string AccountPrivacy10
        {
            get { return pAccountPrivacy10; }
            set { pAccountPrivacy10 = value; }
        }		
				
		private string pAccountReplace10 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 10", Description = "", GroupName = "Account Name Adjustments", Order = 102)]
        public string AccountReplace10
        {
            get { return pAccountReplace10; }
            set { pAccountReplace10 = value; }
        }										
	
		private string pAccountPrivacy11 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 11", Description = "", GroupName = "Account Name Adjustments", Order = 111)]
        public string AccountPrivacy11
        {
            get { return pAccountPrivacy11; }
            set { pAccountPrivacy11 = value; }
        }		
				
		private string pAccountReplace11 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 11", Description = "", GroupName = "Account Name Adjustments", Order = 119)]
        public string AccountReplace11
        {
            get { return pAccountReplace11; }
            set { pAccountReplace11 = value; }
        }				
		
		private string pAccountPrivacy12 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Remove 12", Description = "", GroupName = "Account Name Adjustments", Order = 121)]
        public string AccountPrivacy12
        {
            get { return pAccountPrivacy12; }
            set { pAccountPrivacy12 = value; }
        }		
				
		private string pAccountReplace12 = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Replace 12", Description = "", GroupName = "Account Name Adjustments", Order = 129)]
        public string AccountReplace12
        {
            get { return pAccountReplace12; }
            set { pAccountReplace12 = value; }
        }
	
		
		
		
		
		
		//IsAPEXEval || IsLeeLooEval || IsBulenoxEval || IsMFFEval || IsEliteTFEval || IsFFNEval || IsTradeDayEval || IsTopStepEval || IsTakeProfitTraderEval || IsTickTickEval || IsBLUEval || IsPUREval;
		
		 
		
	
		private string pAPEXCoupon = "DVUDKBFF";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Apex Trader Funding", Description = "", GroupName = "All Prop Firms", Order = 10)]
        public string APEXCoupon
        {
            get { return pAPEXCoupon; }
            set { pAPEXCoupon = value; }
        }	
		
		private string pBluSkyCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BluSky Trading Company", Description = "", GroupName = "All Prop Firms", Order = 12)]
        public string BluSkyCoupon
        {
            get { return pBluSkyCoupon; }
            set { pBluSkyCoupon = value; }
        }			
	
		private string pBulenoxCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bulenox", Description = "", GroupName = "All Prop Firms", Order = 14)]
        public string BulenoxCoupon
        {
            get { return pBulenoxCoupon; }
            set { pBulenoxCoupon = value; }
        }
	
		private string pDayTradersCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DayTraders", Description = "", GroupName = "All Prop Firms", Order = 15)]
        public string DayTradersCoupon
        {
            get { return pDayTradersCoupon; }
            set { pDayTradersCoupon = value; }
        }	
		
		
		private string pEliteTraderFundingCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Elite Trader Funding", Description = "", GroupName = "All Prop Firms", Order = 16)]
        public string EliteTraderFundingCoupon
        {
            get { return pEliteTraderFundingCoupon; }
            set { pEliteTraderFundingCoupon = value; }
        }	
		
		private string pFundedFuturesNetworkCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Funded Futures Network", Description = "", GroupName = "All Prop Firms", Order = 18)]
        public string FundedFuturesNetworkCoupon
        {
            get { return pFundedFuturesNetworkCoupon; }
            set { pFundedFuturesNetworkCoupon = value; }
        }	
			
		private string pLeeLooCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "LeeLoo Trading", Description = "", GroupName = "All Prop Firms", Order = 19)]
        public string LeeLooCoupon
        {
            get { return pLeeLooCoupon; }
            set { pLeeLooCoupon = value; }
        }	
		
		private string pTheLegendsTradingCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Legends Trading", Description = "", GroupName = "All Prop Firms", Order = 20)]
        public string TheLegendsTradingCoupon
        {
            get { return pTheLegendsTradingCoupon; }
            set { pTheLegendsTradingCoupon = value; }
        }	
		
		private string pLifeUpTradingCoupon = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "LifeUp Trading", Description = "", GroupName = "All Prop Firms", Order = 21)]
        public string LifeUpTradingCoupon
        {
            get { return pLifeUpTradingCoupon; }
            set { pLifeUpTradingCoupon = value; }
        }
		
		private string pMyFundedFuturesCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MyFundedFutures", Description = "", GroupName = "All Prop Firms", Order = 22)]
        public string MyFundedFuturesCoupon
        {
            get { return pMyFundedFuturesCoupon; }
            set { pMyFundedFuturesCoupon = value; }
        }			
	
	
		
		private string pPhidiasCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Phidias", Description = "", GroupName = "All Prop Firms", Order = 23)]
        public string PhidiasCoupon
        {
            get { return pPhidiasCoupon; }
            set { pPhidiasCoupon = value; }
        }			
		
	
		
		
		private string pPurdiaCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PurdiaCapital", Description = "", GroupName = "All Prop Firms", Order = 24)]
        public string PurdiaCoupon
        {
            get { return pPurdiaCoupon; }
            set { pPurdiaCoupon = value; }
        }			
				
		private string pTakeProfitTraderCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Take Profit Trader", Description = "", GroupName = "All Prop Firms", Order = 26)]
        public string TakeProfitTraderCoupon
        {
            get { return pTakeProfitTraderCoupon; }
            set { pTakeProfitTraderCoupon = value; }
        }			

	

		
		
		
		// TickTickTrader: Trade It Your Way, Tick by Tick
		
		private string pTickTickCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TickTickTrader", Description = "", GroupName = "All Prop Firms", Order = 28)]
        public string TickTickCoupon
        {
            get { return pTickTickCoupon; }
            set { pTickTickCoupon = value; }
        }	
		
		
		private string pTopStepCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Topstep", Description = "", GroupName = "All Prop Firms", Order = 30)]
        public string TopStepCoupon
        {
            get { return pTopStepCoupon; }
            set { pTopStepCoupon = value; }
        }	
		
		private string pTradeDayCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradeDay", Description = "", GroupName = "All Prop Firms", Order = 32)]
        public string TradeDayCoupon
        {
            get { return pTradeDayCoupon; }
            set { pTradeDayCoupon = value; }
        }			
	
		private string pTradeFundrrCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradeFundrr", Description = "", GroupName = "All Prop Firms", Order = 33)]
        public string TradeFundrrCoupon
        {
            get { return pTradeFundrrCoupon; }
            set { pTradeFundrrCoupon = value; }
        }		
		
		private string pTradeifyCoupon = "";
		//[ReadOnly(true)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Tradeify", Description = "", GroupName = "All Prop Firms", Order = 34)]
        public string TradeifyCoupon
        {
            get { return pTradeifyCoupon; }
            set { pTradeifyCoupon = value; }
        }			
	
	
		

		private bool pShowDiscountLinks = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Coupon Display Enabled", Description = "show buttons that link to prop firm websites", GroupName = "All Prop Firms", Order = 100)]
        public bool ShowDiscountLinks
        {
            get { return pShowDiscountLinks; }
            set { pShowDiscountLinks = value; }
        }		
					
								
		
//		private bool pShowLiveAccounts = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "LIVE", Order = -100)]
//        public bool ShowLiveAccounts
//        {
//            get { return pShowLiveAccounts; }
//            set { pShowLiveAccounts = value; }
//        }			
		
//		private bool pShowSimAccounts = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "SIM", Order = -100)]
//        public bool ShowSimAccounts
//        {
//            get { return pShowSimAccounts; }
//            set { pShowSimAccounts = value; }
//        }			
		
	
		private bool pActionsColumnEnabled = true;
//		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Actions Column (Buttons)", Name = "Enabled", Description = "", Order = 7)]
//     	[RefreshProperties(RefreshProperties.All)]
//		public bool ActionsColumnEnabled
//        {
//            get { return pActionsColumnEnabled; }
//            set { pActionsColumnEnabled = value; }
//        }		
		
		
		
	private bool pActionsHideColumnEnabled = true;
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Actions - Hide When Empty", Description = "hide the actions column when there is no action displayed or available", Order = 8)]
		public bool ActionsHideColumnEnabled
        {
            get { return pActionsHideColumnEnabled; }
            set { pActionsHideColumnEnabled = value; }
        }		
		
		
		
		private string pActionsButtonLocation = "Left";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Actions - Location", Description = "",  Order = 9)]
		[TypeConverter(typeof(BodsfgfhyMode2233423))]
		public string ActionsButtonLocation
		{
			get { return pActionsButtonLocation; }
			set { pActionsButtonLocation = value; }
		}					
						
		
		
		

		internal class BodsfgfhyMode2233423 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Left", "Right"} );
			}
		}			
			
	
					
		
		
		

		
							
			//[NinjaScriptProperty]
			[Display(Name="Text Font", Description="", GroupName = "Window Display", Order = 17)]
			public SimpleFont TextFont3
			{ get; set; }	
			
					
		
	
		
		private Brush pCompMainColor = Brushes.SteelBlue;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Main Color", Order = 18)]
		public Brush CompMainColor
		{
			get { return pCompMainColor; } set { pCompMainColor = value; }
		}
		[Browsable(false)]
		public string CompMainColorS
		{
			get { return Serialize.BrushToString(pCompMainColor); } set { pCompMainColor = Serialize.StringToBrush(value); }
		}
	
		private int pCompMinOpacity = 25;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Main Opacity (%)", Order = 21)]
        public int CompMinOpacity
        {
            get { return pCompMinOpacity; }
            set { pCompMinOpacity = value; }
        }	
		

	
	
		private int pCompMinOpacityH = 50;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Highlight Opacity (%)", Order = 22)]
        public int CompMinOpacityH
        {
            get { return pCompMinOpacityH; }
            set { pCompMinOpacityH = value; }
        }	
		
		
		
		
//		private Brush pCopierButtonOff = Brushes.Gray;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Copier Off", Order = 40)]
//		public Brush CopierButtonOff
//		{
//			get { return pCopierButtonOff; } set { pCopierButtonOff = value; }
//		}
//		[Browsable(false)]
//		public string CopierButtonOffS
//		{
//			get { return Serialize.BrushToString(pCopierButtonOff); } set { pCopierButtonOff = Serialize.StringToBrush(value); }
//		}		
		
		
		private	Brush pBackMasterAccountColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30,30,30));
		//private Brush pBackMasterAccountColor = Brushes.Sienna;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Master Account Color", Order = 40)]
		public Brush BackMasterAccountColor
		{
			get { return pBackMasterAccountColor; } set { pBackMasterAccountColor = value; }
		}
		[Browsable(false)]
		public string BackMasterAccountColorS
		{
			get { return Serialize.BrushToString(pBackMasterAccountColor); } set { pBackMasterAccountColor = Serialize.StringToBrush(value); }
		}
		
		
		private Brush pBackSlaveAccountColor = Brushes.DarkGray;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Slave Account Color", Order = 41)]
		public Brush BackSlaveAccountColor
		{
			get { return pBackSlaveAccountColor; } set { pBackSlaveAccountColor = value; }
		}
		[Browsable(false)]
		public string BackSlaveAccountColorS
		{
			get { return Serialize.BrushToString(pBackSlaveAccountColor); } set { pBackSlaveAccountColor = Serialize.StringToBrush(value); }
		}
		
	
		
		
	private int pCompMinOpacityHAcc = 25;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Account Opacity (%)", Order = 42)]
        public int CompMinOpacityHAcc
        {
            get { return pCompMinOpacityHAcc; }
            set { pCompMinOpacityHAcc = value; }
        }	
				

		private Brush pBackBuyColor = Brushes.Green;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Long Position Color", Order = 50)]
		public Brush BackBuyColor
		{
			get { return pBackBuyColor; } set { pBackBuyColor = value; }
		}
		[Browsable(false)]
		public string BackBuyColorS
		{
			get { return Serialize.BrushToString(pBackBuyColor); } set { pBackBuyColor = Serialize.StringToBrush(value); }
		}
		
		
		private Brush pBackSellColor = Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Short Position Color", Order = 51)]
		public Brush BackSellColor
		{
			get { return pBackSellColor; } set { pBackSellColor = value; }
		}
		[Browsable(false)]
		public string BackSellColorS
		{
			get { return Serialize.BrushToString(pBackSellColor); } set { pBackSellColor = Serialize.StringToBrush(value); }
		}
		
		
		
		
		

		private Brush pPositivePnLTextColor = Brushes.LimeGreen;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "PNL Text Color Positive", Order = 52)]
		public Brush PositivePnLTextColor
		{
			get { return pPositivePnLTextColor; } set { pPositivePnLTextColor = value; }
		}
		[Browsable(false)]
		public string PositivePnLTextColorS
		{
			get { return Serialize.BrushToString(pPositivePnLTextColor); } set { pPositivePnLTextColor = Serialize.StringToBrush(value); }
		}
		
		
		private Brush pNegativePnLTextColor = Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "PNL Text Color Negative", Order = 53)]
		public Brush NegativePnLTextColor
		{
			get { return pNegativePnLTextColor; } set { pNegativePnLTextColor = value; }
		}
		[Browsable(false)]
		public string NegativePnLTextColorS
		{
			get { return Serialize.BrushToString(pNegativePnLTextColor); } set { pNegativePnLTextColor = Serialize.StringToBrush(value); }
		}
		
		
	
		
		

		private int pCompPositionOpacity = 40;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Position Opacity (%)", Order = 54)]
        public int CompPOSOpacity
        {
            get { return pCompPositionOpacity; }
            set { pCompPositionOpacity = value; }
        }		
		
	
		private Brush pConnectedOn = Brushes.LimeGreen;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Connected Status (Yes)", Order = 55)]
		public Brush ConnectedOn
		{
			get { return pConnectedOn; } set { pConnectedOn = value; }
		}
		[Browsable(false)]
		public string pConnectedOnS
		{
			get { return Serialize.BrushToString(pConnectedOn); } set { pConnectedOn = Serialize.StringToBrush(value); }
		}
				
	
		private Brush pConnectedOff = Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Connected Status (No)", Order = 56)]
		public Brush ConnectedOff
		{
			get { return pConnectedOff; } set { pConnectedOff = value; }
		}
		[Browsable(false)]
		public string pConnectedOffS
		{
			get { return Serialize.BrushToString(pConnectedOff); } set { pConnectedOff = Serialize.StringToBrush(value); }
		}
				
		
		
		private Brush pConnectedLost = Brushes.Orange;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Connected Status (Lost)", Order = 57)]
		public Brush ConnectedLost
		{
			get { return pConnectedLost; } set { pConnectedLost = value; }
		}
		[Browsable(false)]
		public string ConnectedLostS
		{
			get { return Serialize.BrushToString(pConnectedLost); } set { pConnectedLost = Serialize.StringToBrush(value); }
		}
		
		
	
		private Brush pConnectedOther = Brushes.Gold;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Connected Status (Other)", Order = 58)]
		public Brush ConnectedOther
		{
			get { return pConnectedOther; } set { pConnectedOther = value; }
		}
		[Browsable(false)]
		public string pConnectedOtherS
		{
			get { return Serialize.BrushToString(pConnectedOther); } set { pConnectedOther = Serialize.StringToBrush(value); }
		}
						
		
		
						
		
	
		private bool pHighlightHoverCells = true;
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Highlight Cells Enabled", Order = 60)]
        public bool HighlightHoverCells
        {
            get { return pHighlightHoverCells; }
            set { pHighlightHoverCells = value; }
        }		
		
			
			

			private int	pHighlightAmount = 10;
			[Range(int.MinValue, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Highlight Cells Opacity (%)", Order = 61)]
			public int HighlightAmount
			{
				get { return pHighlightAmount; }
				set { pHighlightAmount = value; }
			}	
			
			
	
		private bool pHighlightHoverButtons = true;
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Highlight Hover Buttons", Order = 62)]
        public bool HighlightHoverButtons
        {
            get { return pHighlightHoverButtons; }
            set { pHighlightHoverButtons = value; }
        }		
				
		private int pButtonHighlightO = 40;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Button Highlight Opacity (%)", Order = 63)]
        public int ButtonHighlightO
        {
            get { return pButtonHighlightO; }
            set { pButtonHighlightO = value; }
        }
								
			
			
			
			
						
				
		private int pLeftRightPad = 8;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Left / Right Cell Padding (Pixels)", Order = 210)]
        public int LeftRightPad
        {
            get { return pLeftRightPad; }
            set { pLeftRightPad = value; }
        }	
		
		private int pTopBottomPad = 5;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Top / Bottom Cell Padding (Pixels)", Order = 211)]
        public int TopBottomPad
        {
            get { return pTopBottomPad; }
            set { pTopBottomPad = value; }
        }	
		
		
		
		private bool pRemoveIcons = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Remove Chart Window Header", GroupName = "Window Display", Order = 1000, Description = "removes the 'Chart' text, data selection, and other icons in the header of the window.")]
        public bool RemoveIcons
        {
            get { return pRemoveIcons; }
            set { pRemoveIcons = value; }
        }	
		
		
		private bool pAutoSizeWindow = false;
//        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Automatic Window Sizing", Order = 300)]
//        public bool AutoSizeWindow
//        {
//            get { return pAutoSizeWindow; }
//            set { pAutoSizeWindow = value; }
//        }

		
			
//			private int	pAutoSizeX = 0;
//			[Range(int.MinValue, int.MaxValue), NinjaScriptProperty]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Offset X (Pixels)", Order = 301)]
//			public int AutoSizeX
//			{
//				get { return pAutoSizeX; }
//				set { pAutoSizeX = value; }
//			}	
		
		
	
		
		
	
		private bool pAffiliateLinkDone1 = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Affiliate Link Done", GroupName = "Hidden", Order = -100)]
        public bool AffiliateLinkDone1
        {
            get { return pAffiliateLinkDone1; }
            set { pAffiliateLinkDone1 = value; }
        }	
		
		
		private bool pAffiliateLinkDone2 = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Affiliate Link Done", GroupName = "Hidden", Order = -100)]
        public bool AffiliateLinkDone2
        {
            get { return pAffiliateLinkDone2; }
            set { pAffiliateLinkDone2 = value; }
        }		
		
		
		
		

	
		private bool pFirstLoadIsDone = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Affiliate Link Done", GroupName = "Hidden", Order = -100)]
        public bool FirstLoadIsDone
        {
            get { return pFirstLoadIsDone; }
            set { pFirstLoadIsDone = value; }
        }		
					
		
	
		
		
			
			
			
			
			
			
			
			
			
			
	
			private bool pIsCopyBasicFunctionsEnabled = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "All Copy Enabled", Description = "", GroupName = "Hidden", Order = -9)]
	        public bool IsCopyBasicFunctionsEnabled
	        {
	            get { return pIsCopyBasicFunctionsEnabled; }
	            set { pIsCopyBasicFunctionsEnabled = value; }
	        }			
							
		private bool pIsRiskFunctionsEnabled = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "All Risk Enabled", Description = "", GroupName = "Hidden", Order = -9)]
	        public bool IsRiskFunctionsEnabled
	        {
	            get { return pIsRiskFunctionsEnabled; }
	            set { pIsRiskFunctionsEnabled = value; }
	        }	
			
					
	
			private bool pIsCopyBasicFunctionsPermission = false;
			[Display(ResourceType = typeof(Custom.Resource), Name = "All Copy Permission", Description = "", GroupName = "Hidden", Order = -9)]
	        public bool IsCopyBasicFunctionsPermission
	        {
	            get { return pIsCopyBasicFunctionsPermission; }
	            set { pIsCopyBasicFunctionsPermission = value; }
	        }			
							
			private bool pIsRiskFunctionsPermission = false;
			[Display(ResourceType = typeof(Custom.Resource), Name = "All Risk Permission", Description = "", GroupName = "Hidden", Order = -9)]
	        public bool IsRiskFunctionsPermission
	        {
	            get { return pIsRiskFunctionsPermission; }
	            set { pIsRiskFunctionsPermission = value; }
	        }	
					
			
		

	
			private bool pIsCopyBasicFunctionsChecked = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Duplicate Account Actions", Description = "", GroupName = "Main Features", Order = 10)]
	        public bool IsCopyBasicFunctionsChecked
	        {
	            get { return pIsCopyBasicFunctionsChecked; }
	            set { pIsCopyBasicFunctionsChecked = value; }
	        }			
				
			
			private bool pIsRiskFunctionsChecked = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Account Risk Manager", Description = "", GroupName = "Main Features", Order = 20)]
	        public bool IsRiskFunctionsChecked
	        {
	            get { return pIsRiskFunctionsChecked; }
	            set { pIsRiskFunctionsChecked = value; }
	        }	
			
			
			
			
		
			private bool pFillFundedCells = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Funded Accounts - Fill Enabled", Description = "", GroupName = "Account Risk Manager", Order = 10)]
	        public bool FillFundedCells
	        {
	            get { return pFillFundedCells; }
	            set { pFillFundedCells = value; }
	        }	
						
			
			private Brush pIsFundedColor = Brushes.LimeGreen;
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Background Color (Funded)", Order = 20)]
			public Brush IsFundedColor
			{
				get { return pIsFundedColor; } set { pIsFundedColor = value; }
			}
			[Browsable(false)]
			public string IsFundedColorS
			{
				get { return Serialize.BrushToString(pIsFundedColor); } set { pIsFundedColor = Serialize.StringToBrush(value); }
			}	
		
		
		
		
	
			private bool pColumnFromFund = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - From Funded", Description = "", GroupName = "Account Risk Manager", Order = 30)]
	        public bool ColumnFromFund
	        {
	            get { return pColumnFromFund; }
	            set { pColumnFromFund = value; }
	        }			
			
					
			private bool pIsEvalCloseEnabled = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Funded / Withdraw Exit", Description = "", GroupName = "Account Risk Manager", Order = 40)]
	        public bool IsEvalCloseEnabled
	        {
	            get { return pIsEvalCloseEnabled; }
	            set { pIsEvalCloseEnabled = value; }
	        }			
								
			

			
			private int	pDollarsExceedFunded = 50;
			[Range(int.MinValue, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Evaluation Accounts Funded + ($)", Description = "amount of dollars beyond the profit goal to consider an account funded", Order = 50)]
			public int DollarsExceedFunded
			{
				get { return pDollarsExceedFunded; }
				set { pDollarsExceedFunded = value; }
			}		
			
			
			
			private int	pPAFundedAmouont = 0;
			[Range(int.MinValue, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Performance Accounts Withdraw + ($)", Description = "the amount in dollars, above auto liquidate, that is the goal to start getting payouts in  performance accounts",Order = 50)]
			public int PAFundedAmouont
			{
				get { return pPAFundedAmouont; }
				set { pPAFundedAmouont = value; }
			}		
					
				private bool pDisableFundedFollowers = true; //always show master account and follower accounts 
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Funded / Withdraw Exit - Disable Followers", Description = "when an account hits funded status, it will keep Follower status, but further trades will be ignored.", Order = 51)]
	        public bool DisableFundedFollowers
	        {
	            get { return pDisableFundedFollowers; }
	            set { pDisableFundedFollowers = value; }
	        }
				
		
			
			
			private bool pColumnAutoLiquidate = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Auto Liquidate", Description = "", GroupName = "Account Risk Manager", Order = 60)]
	        public bool ColumnAutoLiquidate
	        {
	            get { return pColumnAutoLiquidate; }
	            set { pColumnAutoLiquidate = value; }
	        }			
							
											
			private bool pColumnRemaining = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - From Closed", Description = "", GroupName = "Account Risk Manager", Order = 70)]
	        public bool ColumnRemaining
	        {
	            get { return pColumnRemaining; }
	            set { pColumnRemaining = value; }
	        }			
														
			
		
	
			
	
			private bool pFillTrailingCells = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Auto Liquidate - Fill Enabled", Description = "", GroupName = "Account Risk Manager", Order = 71)]
	        public bool FillTrailingCells
	        {
	            get { return pFillTrailingCells; }
	            set { pFillTrailingCells = value; }
	        }			
			
		
	
		
		
		
			private int	pPercentWarning1 = 60;
			[Range(0, 100)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Warning 1 (%)", Order = 72)]
			public int PercentWarning1
			{
				get { return pPercentWarning1; }
				set { pPercentWarning1 = value; }
			}	
			
			private int	pPercentWarning2 = 30;
			[Range(0, 100)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Warning 2 (%)", Order = 73)]
			public int PercentWarning2
			{
				get { return pPercentWarning2; }
				set { pPercentWarning2 = value; }
			}	
				
		
		private Brush pTrailingGoodColor = Brushes.LimeGreen;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Background Color (Good)", Order = 74)]
		public Brush TrailingGoodColor
		{
			get { return pTrailingGoodColor; } set { pTrailingGoodColor = value; }
		}
		[Browsable(false)]
		public string TrailingGoodColorS
		{
			get { return Serialize.BrushToString(pTrailingGoodColor); } set { pTrailingGoodColor = Serialize.StringToBrush(value); }
		}
		
			
		
		private Brush pTrailingWarningColor = Brushes.Yellow;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Background Color (Warning 1)", Order = 75)]
		public Brush TrailingWarningColor
		{
			get { return pTrailingWarningColor; } set { pTrailingWarningColor = value; }
		}
		[Browsable(false)]
		public string TrailingWarningColorS
		{
			get { return Serialize.BrushToString(pTrailingWarningColor); } set { pTrailingWarningColor = Serialize.StringToBrush(value); }
		}
		

		private Brush pTrailingBadColor = Brushes.Orange;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Background Color (Warning 2)", Order = 76)]
		public Brush TrailingBadColor
		{
			get { return pTrailingBadColor; } set { pTrailingBadColor = value; }
		}
		[Browsable(false)]
		public string TrailingBadColorS
		{
			get { return Serialize.BrushToString(pTrailingBadColor); } set { pTrailingBadColor = Serialize.StringToBrush(value); }
		}
				
		private Brush pTrailingBlownColor = Brushes.Brown;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Background Color (Closed)", Order = 77)]
		public Brush TrailingBlownColor
		{
			get { return pTrailingBlownColor; } set { pTrailingBlownColor = value; }
		}
		[Browsable(false)]
		public string TrailingBlownColorS
		{
			get { return Serialize.BrushToString(pTrailingBlownColor); } set { pTrailingBlownColor = Serialize.StringToBrush(value); }
		}		
		
		
								
			
		
		
			
	
			private bool pColumnDailyGoal = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Daily Goal", Description = "", GroupName = "Account Risk Manager", Order = 79)]
	        public bool ColumnDailyGoal
	        {
	            get { return pColumnDailyGoal; }
	            set { pColumnDailyGoal = value; }
	        }			
				
	
			private bool pColumnDailyLoss = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Daily Loss", Description = "", GroupName = "Account Risk Manager", Order = 80)]
	        public bool ColumnDailyLoss
	        {
	            get { return pColumnDailyLoss; }
	            set { pColumnDailyLoss = value; }
	        }			

			private bool pColumnProfitRequested = false;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Payout", Description = "use to enter the profit requested to subtract it from your net liquidation", GroupName = "Account Risk Manager", Order = 81)]
	        public bool ColumnProfitRequested
	        {
	            get { return pColumnProfitRequested; }
	            set { pColumnProfitRequested = value; }
	        }			
			
			
		
			private bool pShowPercentOnGoalLoss = false;
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Daily Loss", Description = "", GroupName = "Account Risk Manager", Order = 81)]
//	        public bool ShowPercentOnGoalLoss
//	        {
//	            get { return pShowPercentOnGoalLoss; }
//	            set { pShowPercentOnGoalLoss = value; }
//	        }			

					
	
		
			
			
			
			private bool pOnMasterGoalHitCloseFollowers = false;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Daily Goal / Loss  - Master Account Lead", Description = "When checked, Daily Goal or Daily Loss configured for the Master account, when hit, will also affect Follower accounts.", GroupName = "Account Risk Manager", Order = 82)]
	        public bool OnMasterGoalHitCloseFollowers
	        {
	            get { return pOnMasterGoalHitCloseFollowers; }
	            set { pOnMasterGoalHitCloseFollowers = value; }
	        }		
			
			
			private bool pDisableGoalFollowers = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Daily Goal / Loss  - Disable Followers", Description = "when checked, any account that is flattened as Daily Goal / Loss t will keep Follower status, but further trades will be ignored.", GroupName = "Account Risk Manager", Order = 83)]
	        public bool DisableGoalFollowers // "when an account hits funded status, it will keep Follower status, but further trades will be ignored.
	        {  // is hit will be deactivated from Follower account status.
	            get { return pDisableGoalFollowers; }
	            set { pDisableGoalFollowers = value; }
	        }					
		
				private int	pDailyGoalLossBuffer = 20;
			[Range(int.MinValue, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Daily Goal / Loss - Adjustment", Description = "adjustment, in dollars, when considering Daily Goal / Loss to be hit for future trades.", Order = 84)]
			public int DailyGoalLossBuffer
			{
				get { return pDailyGoalLossBuffer; }
				set { pDailyGoalLossBuffer = value; }
			}		
			
			private int	pAdjustmentDollars = 100;
			[Range(int.MinValue, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Risk Manager", Name = "Daily Goal / Loss - Increment", Description = "adjustment, in dollars, when scrolling mouse wheel.", Order = 85)]
			public int AdjustmentDollars
			{
				get { return pAdjustmentDollars; }
				set { pAdjustmentDollars = value; }
			}		
			
			private bool pRestrictToZero = true;
			[Display(ResourceType = typeof(Custom.Resource), Name = "Daily Goal / Loss  - Restrict At Zero", Description = "When checked, Daily Goal must be greater than or equal to 0 and Daily Loss must be less than or equal to 0.", GroupName = "Account Risk Manager", Order = 100)]
	        public bool RestrictToZero
	        {
	            get { return pRestrictToZero; }
	            set { pRestrictToZero = value; }
	        }							
			
		
			
			
			
		
		private string pDailyGoalDisplayMode = "Update";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Type Default Contract", GroupName = "Duplicate Account Actions", Order = 31 )]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(DGDisplayMode))]
		public string DailyGoalDisplayMode
		{
			get { return pDailyGoalDisplayMode; }
			set { pDailyGoalDisplayMode = value; }
		}

			
		
		private string pDailyLossDisplayMode = "Update";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Type Default Contract", GroupName = "Duplicate Account Actions", Order = 31 )]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(DGDisplayMode))]
		public string DailyLossDisplayMode
		{
			get { return pDailyLossDisplayMode; }
			set { pDailyLossDisplayMode = value; }
		}
		
		
		
		
		internal class DGDisplayMode : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Status", "Update"} );
			}
		}	
		
		
		
		
		
		private string pSelectedLanguage = "Default";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Language", GroupName = "Language", Order = 31 )]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(SelectedLanguageMode))]
		public string SelectedLanguage
		{
			get { return pSelectedLanguage; }
			set { pSelectedLanguage = value; }
		}
		
		
		
		
		internal class SelectedLanguageMode : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Default", "English", "Korean"} );
			}
		}	
		
		
		
		
		
		
		
			
			private System.Windows.Media.Brush pColorTextBrush	= Brushes.Silver;
//			[XmlIgnore]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "Window Display", Name = "Text Color", Description = "", Order = 30)]
//			public System.Windows.Media.Brush ColorTextBrush
//			{
//				get { return pColorTextBrush; } set { pColorTextBrush = value; }
//			}
//			[Browsable(false)]
//			public string ColorTextBrushS
//			{
//				get { return Serialize.BrushToString(pColorTextBrush); } set { pColorTextBrush = Serialize.StringToBrush(value); }
//			}	
	
	
			
			
			private int	pPixelsFromLeft = 10;
//			[Range(0, int.MaxValue), NinjaScriptProperty]
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Offset X (Pixels)", Description = "", GroupName = "Window Display", Order = 50)]
//			public int PixelsFromLeft
//			{
//				get { return pPixelsFromLeft; }
//				set { pPixelsFromLeft = value; }
//			}	
			
			private int	pPixelsFromTop = 10;
//			[Range(0, int.MaxValue), NinjaScriptProperty]
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Offset Y (Pixels)", Description = "", GroupName = "Window Display", Order = 61)]
//			public int PixelsFromTop
//			{
//				get { return pPixelsFromTop; }
//				set { pPixelsFromTop = value; }
//			}		
			
			private int	pColumnWidthP = 60;
//			[Range(0, int.MaxValue), NinjaScriptProperty]
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Width (Pixels)", Description = "", GroupName = "Display", Order = 7)]
//			public int ColumnWidthP
//			{
//				get { return pColumnWidthP; }
//				set { pColumnWidthP = value; }
//			}
			
		
			
			
			  private bool pButtonsEnabled = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Chart Buttons", Order = 1)]
//        public bool ButtonsEnabled
//        {
//            get { return pButtonsEnabled; }
//            set { pButtonsEnabled = value; }
//        }


		
//        private bool pShowBars = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Split Bid / Ask", GroupName = "Buttons", Order = 1)]
//        public bool ShowBars
//        {
//            get { return pShowBars; }
//            set { pShowBars = value; }
//        }			
		
				
			// These are WPF Brushes which are pushed and exposed to the UI by default
			// And allow users to configure a custom value of their choice
			// We will later convert the user defined brush from the UI to SharpDX Brushes for rendering purposes
			private System.Windows.Media.Brush	pButtonOffColor = System.Windows.Media.Brushes.Gray;
			private System.Windows.Media.Brush	textBrush = System.Windows.Media.Brushes.DodgerBlue;
			private int							pButtonOffOpacity= 20;
			//private SMA							mySma;
		
		

		// 
		
		
		private bool pShowFollowerColumnButtons = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Follower Adjustment Column Buttons Enabled", Description = "show the Fade, Size, and Type buttons for quickly turning those columns on and off", GroupName = "Control Buttons", Order = 0)]
        public bool ShowFollowerColumnButtons
        {
            get { return pShowFollowerColumnButtons; }
            set { pShowFollowerColumnButtons = value; }
        }			

		private Brush pCopierButtonOn2 = Brushes.LimeGreen;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Copier Is On - Color", GroupName = "Control Buttons", Order = 1)]
		public Brush CopierButtonOn2
		{
			get { return pCopierButtonOn2; } set { pCopierButtonOn2 = value; }
		}
		[Browsable(false)]
		public string CopierButtonOn2S
		{
			get { return Serialize.BrushToString(pCopierButtonOn2); } set { pCopierButtonOn2 = Serialize.StringToBrush(value); }
		}		
		
		private Brush pLockButtonOn = Brushes.Maroon;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lock Is On - Color", GroupName = "Control Buttons", Order = 2)]
		public Brush LockButtonOn
		{
			get { return pLockButtonOn; } set { pLockButtonOn = value; }
		}
		[Browsable(false)]
		public string LockButtonOnS
		{
			get { return Serialize.BrushToString(pLockButtonOn); } set { pLockButtonOn = Serialize.StringToBrush(value); }
		}	
		
	
		
		
		
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Button Off - Color", GroupName = "Control Buttons", Order = 20)]
        public Brush AreaBrush
        {
            get { return pButtonOffColor; }
            set
            {
                pButtonOffColor = value;
                if (pButtonOffColor != null)
                {
                    if (pButtonOffColor.IsFrozen)
                        pButtonOffColor = pButtonOffColor.Clone();
                    pButtonOffColor.Opacity = pButtonOffOpacity / 100d;
                    pButtonOffColor.Freeze();
                }
            }
        }

        [Browsable(false)]
        public string AreaBrushSerialize {
            get { return Serialize.BrushToString(AreaBrush); }
            set { AreaBrush = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Button Off Opacity (%)", GroupName = "Control Buttons", Order = 21)]
        public int AreaOpacity
        {
            get { return pButtonOffOpacity; }
            set
            {
                pButtonOffOpacity = Math.Max(0, Math.Min(100, value));
                if (pButtonOffColor != null)
                {
                    Brush newBrush = pButtonOffColor.Clone();
                    newBrush.Opacity = pButtonOffOpacity / 100d;
                    newBrush.Freeze();
                    pButtonOffColor = newBrush;
                }
            }
        }

				
		//[NinjaScriptProperty]
//			[Display(Name="Text Font", Description="", GroupName = "Control Buttons", Order = 31)]
//			public SimpleFont TextFont4
//			{ get; set; }	
					
   
      	private int pFontSizeTopRow = 2;
        [Range(int.MinValue, int.MaxValue)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Font Size Adjustment (Top)", GroupName = "Control Buttons", Order = 31)]
        public int FontSizeTopRow
        {
            get { return pFontSizeTopRow; }
            set { pFontSizeTopRow = value; }
        }    
		
      	private int pFontSizeB = 0;
        [Range(int.MinValue, int.MaxValue)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Font Size Adjustment (Bottom)", GroupName = "Control Buttons", Order = 32)]
        public int FontSizeB
        {
            get { return pFontSizeB; }
            set { pFontSizeB = value; }
        }
		
		
		
        private int pButtonSize = 18;
//        [Range(1, int.MaxValue), NinjaScriptProperty]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Size (Pixels)", GroupName = "Buttons", Order = 1)]
//        public int ButtonSize
//        {
//            get { return pButtonSize; }
//            set { pButtonSize = value; }
//        }
		
		

	
		private string pLicensingEmailAddress = "";
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "License", Name = "Email Address", Order = 54, Description = "")]
        public string LicensingEmailAddress
        {
            get { return pLicensingEmailAddress; }
            set { pLicensingEmailAddress = value; }
        }			
		

		

		
		private string pThisMasterAccount = "";
	
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "License", Name = "Email Address", Order = 54, Description = "")]
        public string ThisMasterAccount
        {
            get { return pThisMasterAccount; }
            set { pThisMasterAccount = value; }
        }		
		
		
		
		
		
		private bool pColumnAudioE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Audio Switch", Description = "", GroupName = "Columns", Order = 1)]
        public bool ColumnAudioE
        {
            get { return pColumnAudioE; }
            set { pColumnAudioE = value; }
        }	
			
		private bool pColumnEMAE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMAs", Description = "", GroupName = "Columns", Order = 9)]
        public bool ColumnEMAE
        {
            get { return pColumnEMAE; }
            set { pColumnEMAE = value; }
        }			
		
		
		private bool pColumnTimeStatusE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Filter Button", Description = "", GroupName = "Columns", Order = 10)]
        public bool ColumnTimeStatusE
        {
            get { return pColumnTimeStatusE; }
            set { pColumnTimeStatusE = value; }
        }		
		
				
						
		private bool pColumnTrendStatusE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend Filter Button", Description = "", GroupName = "Columns", Order = 12)]
        public bool ColumnTrendStatusE
        {
            get { return pColumnTrendStatusE; }
            set { pColumnTrendStatusE = value; }
        }

	
		
		
		private bool pColumnSignalE = true;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Signals (+)", Description = "", GroupName = "Columns", Order = 0)]
//        public bool ColumnSignalE
//        {
//            get { return pColumnSignalE; }
//            set { pColumnSignalE = value; }
//        }

		private bool pColumnLastE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Last Price", Description = "", GroupName = "Columns", Order = 20)]
        public bool ColumnLastE
        {
            get { return pColumnLastE; }
            set { pColumnLastE = value; }
        }			

		private bool pColumnEntryE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Entry Price", Description = "", GroupName = "Columns", Order = 21)]
        public bool ColumnEntryE
        {
            get { return pColumnEntryE; }
            set { pColumnEntryE = value; }
        }			
		
		private bool pColumnStopE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Price", Description = "", GroupName = "Columns", Order = 22)]
        public bool ColumnStopE
        {
            get { return pColumnStopE; }
            set { pColumnStopE = value; }
        }			
		
		
	

		private bool pColumnT1E = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Target 1", Description = "", GroupName = "Columns", Order = 23)]
        public bool ColumnT1E
        {
            get { return pColumnT1E; }
            set { pColumnT1E = value; }
        }				
				
		private bool pColumnT2E = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Target 2", Description = "", GroupName = "Columns", Order = 24)]
        public bool ColumnT2E
        {
            get { return pColumnT2E; }
            set { pColumnT2E = value; }
        }			
			
		private bool pColumnT3E = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Target 3", Description = "", GroupName = "Columns", Order = 25)]
        public bool ColumnT3E
        {
            get { return pColumnT3E; }
            set { pColumnT3E = value; }
        }			
				
		
		
		
		private bool pShowAllPriceColE = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show All Prices", Description = "", GroupName = "Columns", Order = 20)]
        public bool ShowAllPriceColE
        {
            get { return pShowAllPriceColE; }
            set { pShowAllPriceColE = value; }
        }			
		
		
		
 		
		private bool pColumnCloseE = true;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Close Position Button", Description = "", GroupName = "Columns", Order = 50)]
//        public bool ColumnCloseE
//        {
//            get { return pColumnCloseE; }
//            set { pColumnCloseE = value; }
//        }	
		
		private bool pShowATMQWhenFlat = true;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Show ATM Qty When Flat", Description = "", GroupName = "Columns", Order = 51)]
//        public bool ShowATMQWhenFlat
//        {
//            get { return pShowATMQWhenFlat; }
//            set { pShowATMQWhenFlat = value; }
//        }			
	
		
//		private bool pAlertEnabled = true;
//		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Signal Alerts", Order = 0)]
//        public bool AlertEnabled
//        {
//            get { return pAlertEnabled; }
//            set { pAlertEnabled = value; }
//        }	
		
//		private bool pQuickAudio = true;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "IntraBar", GroupName = "Audio", Order = 1)]
//        public bool QuickAudio
//        {
//            get { return pQuickAudio; }
//            set { pQuickAudio = value; }
//        }

		
		
				
										//if (pCloseButtonEnabled)
		
	
		
		
		private bool pShowAvgPrice = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Average Price", Description = "", GroupName = "Columns", Order = 10)]
        public bool ShowAvgPrice
        {
            get { return pShowAvgPrice; }
            set { pShowAvgPrice = value; }
        }				
		
		
		private bool pUnrealizedColumn = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Unrealized", Description = "", GroupName = "Columns", Order = 11)]
        public bool UnrealizedColumn
        {
            get { return pUnrealizedColumn; }
            set { pUnrealizedColumn = value; }
        }				
		
		private bool pRealizedColumn = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Realized", Description = "", GroupName = "Columns", Order = 12)]
        public bool RealizedColumn
        {
            get { return pRealizedColumn; }
            set { pRealizedColumn = value; }
        }				
						
		private bool pGrossRealizedColumn = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Gross Realized", Description = "", GroupName = "Columns", Order = 13)]
        public bool GrossRealizedColumn
        {
            get { return pGrossRealizedColumn; }
            set { pGrossRealizedColumn = value; }
        }				
				
		
		private bool pCashValueColumn = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Cash Value", Description = "", GroupName = "Columns", Order = 15)]
        public bool CashValueColumn
        {
            get { return pCashValueColumn; }
            set { pCashValueColumn = value; }
        }						
		
	private bool pNetLiquidationColumn = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Net Liquidation", Description = "", GroupName = "Columns", Order = 16)]
        public bool NetLiquidationColumn
        {
            get { return pNetLiquidationColumn; }
            set { pNetLiquidationColumn = value; }
        }						
				
	private bool pQtyColumn2 = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Qty", Description = "", GroupName = "Columns", Order = 18)]
        public bool QtyColumn2
        {
            get { return pQtyColumn2; }
            set { pQtyColumn2 = value; }
        }						
							
	
		
		private bool pCommissionsColumn = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Commissions", Description = "", GroupName = "Columns", Order = 20)]
        public bool CommissionsColumn
        {
            get { return pCommissionsColumn; }
            set { pCommissionsColumn = value; }
        }		
		
		private bool pTotalPNLColumn = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total PNL", Description = "", GroupName = "Columns", Order = 22)]
        public bool TotalPNLColumn
        {
            get { return pTotalPNLColumn; }
            set { pTotalPNLColumn = value; }
        }		
		
		
		
	
		private bool pShowConnectedStatus = true;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Connected Status", Description = "", GroupName = "Columns", Order = 2)]
//        public bool ShowConnectedStatus
//        {
//            get { return pShowConnectedStatus; }
//            set { pShowConnectedStatus = value; }
//        }		
		
		
		
		private bool pAlertLogEnabled = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Alerts Log Enabled", Description = "", GroupName = "Signal Alerts", Order = 1)]
        public bool AlertLogEnabled
        {
            get { return pAlertLogEnabled; }
            set { pAlertLogEnabled = value; }
        }		
		
		private bool pAudioEnabled = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Audio Enabled", Description = "", GroupName = "Signal Alerts", Order = 2)]
        public bool AudioEnabled
        {
            get { return pAudioEnabled; }
            set { pAudioEnabled = value; }
        }		
		
		
		private string pWAVFileName = "Alert2.wav";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Signal WAV", Description = "Sound file to play when a buy signal occurs.", GroupName = "Signal Alerts", Order = 5)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		public string WAVFileName
		{
			get { return pWAVFileName; }
			set { pWAVFileName = value; }
		}

		private Brush pArrowUpFBrush	= Brushes.Green;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Signal Background", Description = "", GroupName = "Signal Alerts", Order = 6)]
		public Brush ArrowUpFBrush
		{
			get { return pArrowUpFBrush; } set { pArrowUpFBrush = value; }
		}
		[Browsable(false)]
		public string ArrowUpFBrushS
		{
			get { return Serialize.BrushToString(pArrowUpFBrush); } set { pArrowUpFBrush = Serialize.StringToBrush(value); }
		}	
		
		private string pWAVFileName2 = "Alert2.wav";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Signal WAV", Description = "Sound file to play when a sell signal occurs.", GroupName = "Signal Alerts", Order = 7)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		public string WAVFileName2
		{
			get { return pWAVFileName2; }
			set { pWAVFileName2 = value; }
		}

		private Brush pArrowDownFBrush	= Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Signal Background", Description = "", GroupName = "Signal Alerts", Order = 8)]
		public Brush ArrowDownFBrush
		{
			get { return pArrowDownFBrush; } set { pArrowDownFBrush = value; }
		}
		[Browsable(false)]
		public string ArrowDownFBrushS
		{
			get { return Serialize.BrushToString(pArrowDownFBrush); } set { pArrowDownFBrush = Serialize.StringToBrush(value); }
		}			
		
		
		
		
		
		
		internal class LoadFileList : StringConverter
		{
			#region LoadFileList
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
		

		private bool pSpeechEnabled = true;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Voice Enabled", Description = "", GroupName = "Signal Alerts", Order = 20)]
        public bool SpeechEnabled
        {
            get { return pSpeechEnabled; }
            set { pSpeechEnabled = value; }
        }	
		
		
//		private string pVoiceName = "Microsoft Zira Desktop";
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Voice", Description = "", GroupName = "Signal Alerts", Order = 21)]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadFileList2))]
//		public string VoiceName
//		{
//			get { return pVoiceName; }
//			set { pVoiceName = value; }
//		}

		//private string pThisIndyName = "T T P BOT";
		private string pThisIndyName = "BOT";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal Name", Description = "", GroupName = "Signal Alerts", Order = 30)]
		public string ThisIndyName
		{
			get { return pThisIndyName; }
			set { pThisIndyName = value; }
		}
		
		private string pVoiceMode = "Name";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Signal Alerts", Name = "Instrument Mode", Description = "",  Order = 31)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode))]
		public string VoiceMode
		{
			get { return pVoiceMode; }
			set { pVoiceMode = value; }
		}

		internal class BodyMode : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Name", "Symbol"} );
			}
		}	
		
		
		
		private string Total1Name = string.Empty;
		private string Total2Name = string.Empty;
		private string Total3Name = string.Empty;
		private string Total4Name = string.Empty;
		private string Total5Name = string.Empty;
		private string Total6Name = string.Empty;
		private string Total7Name = string.Empty;
		
		private bool IsOneTotal = false;
		
		

		private string pCurrentVersionName = "25. 2. 5. 1";
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Version", GroupName = "Version", Order = 3, Description = "")]
//        public string CurrentVersionName
//        {
//            get { return pCurrentVersionName; }
//            set { pCurrentVersionName = value; }
//        }		
				
		private string pPreviousVersionName = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Version", GroupName = "Version", Order = 3, Description = "")]
        public string PreviousVersionName
        {
            get { return pPreviousVersionName; }
            set { pPreviousVersionName = value; }
        }		
		
		
		
		private bool pShowTotalRows = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Total Rows", Order = -100)]
		[RefreshProperties(RefreshProperties.All)]
        public bool ShowTotalRows
        {
            get { return pShowTotalRows; }
            set { pShowTotalRows = value; }
        }
		
	
		private string pTotal1Name = "PA-APEX";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 1 Label", GroupName = "Total Rows", Order = 3, Description = "")]
        public string Total1Name2
        {
            get { return pTotal1Name; }
            set { pTotal1Name = value; }
        }		
		
		
		private string pTotal2Name = "APEX";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 2 Label", GroupName = "Total Rows", Order = 5, Description = "")]
        public string Total2Name2
        {
            get { return pTotal2Name; }
            set { pTotal2Name = value; }
        }		
		
		
		private string pTotal3Name = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 3 Label", GroupName = "Total Rows", Order = 7, Description = "")]
        public string Total3Name2
        {
            get { return pTotal3Name; }
            set { pTotal3Name = value; }
        }		
		
		private string pTotal5Name = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 4 Label", GroupName = "Total Rows", Order = 9, Description = "")]
        public string Total5Name2
        {
            get { return pTotal5Name; }
            set { pTotal5Name = value; }
        }		
		
		
		private string pTotal6Name = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 5 Label", GroupName = "Total Rows", Order = 11, Description = "")]
        public string Total6Name2
        {
            get { return pTotal6Name; }
            set { pTotal6Name = value; }
        }		
		
		
		private string pTotal7Name = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 6 Label", GroupName = "Total Rows", Order = 13, Description = "")]
        public string Total7Name2
        {
            get { return pTotal7Name; }
            set { pTotal7Name = value; }
        }		
		
		
		
		private string pTotal1Filter = "PA-APEX";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 1 Account Filter", GroupName = "Total Rows", Order = 4, Description = "")]
        public string Total1Filter
        {
            get { return pTotal1Filter; }
            set { pTotal1Filter = value; }
        }		
		
		
		private string pTotal2Filter = "APEX";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 2 Account Filter", GroupName = "Total Rows", Order = 6, Description = "")]
        public string Total2Filter
        {
            get { return pTotal2Filter; }
            set { pTotal2Filter = value; }
        }		
		
		
		private string pTotal3Filter = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 3 Account Filter", GroupName = "Total Rows", Order = 8, Description = "")]
        public string Total3Filter
        {
            get { return pTotal3Filter; }
            set { pTotal3Filter = value; }
        }		
			
	
		
		private string pTotal5Filter = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 4 Account Filter", GroupName = "Total Rows", Order = 10, Description = "")]
        public string Total5Filter
        {
            get { return pTotal5Filter; }
            set { pTotal5Filter = value; }
        }		
		
		
		private string pTotal6Filter = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 5 Account Filter", GroupName = "Total Rows", Order = 12, Description = "")]
        public string Total6Filter
        {
            get { return pTotal6Filter; }
            set { pTotal6Filter = value; }
        }		
		
		private string pTotal7Filter = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Total 6 Account Filter", GroupName = "Total Rows", Order = 14, Description = "")]
        public string Total7Filter
        {
            get { return pTotal7Filter; }
            set { pTotal7Filter = value; }
        }	
		
		
		
		
		private bool pShowGrandTotal = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Grand Total Enabled", GroupName = "Total Rows", Order = 20)]
        public bool ShowGrandTotal
        {
            get { return pShowGrandTotal; }
            set { pShowGrandTotal = value; }
        }		
		
		private string pTotal4Name = "Total";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Grand Total Label", Description = "", GroupName = "Total Rows", Order = 21)]
        public string Total4Name2
        {
            get { return pTotal4Name; }
            set { pTotal4Name = value; }
        }		
				
		
		
		
		private bool pDisableOnStart = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Disable On Load", GroupName = "Safety", Order = 10, Description = "Duplicate Account Actions will be disabled every time the NinjaScript is reloaded in this Chart window.")]
        public bool DisableOnStart
        {
            get { return pDisableOnStart; }
            set { pDisableOnStart = value; }
        }	
		
						
		private bool pDisableOnNewSession = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Disable On New Session", GroupName = "Safety", Order = 20, Description = "Duplicate Account Actions will be disabled every time a connection is active and a new session begins.")]
        public bool DisableOnNewSession
        {
            get { return pDisableOnNewSession; }
            set { pDisableOnNewSession = value; }
        }			
		
		private bool pDisableInActiveWorkspace = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Disable Inactive Workspace", GroupName = "Safety", Order = 30, Description = "Duplicate Account Actions will be disabled when it is in a workspace that is open, but not active.")]
        public bool DisableInActiveWorkspace
        {
            get { return pDisableInActiveWorkspace; }
            set { pDisableInActiveWorkspace = value; }
        }		
	
		
		private string pButtonName = "COPY";
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Name", Description = "", GroupName = "Button", Order = 4)]
//        public string ButtonName
//        {
//            get { return pButtonName; }
//            set { pButtonName = value; }
//        }		

	
	
		
	
        private Stroke pOrderDnOutlineStroke = new Stroke(Brushes.Red, DashStyleHelper.Solid, 3);
//        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Chart Window Display", Name = "Chart Sell Active Outline", Order = 72)]
//        public Stroke OrderDnOutlineStroke
//        {
//            get { return pOrderDnOutlineStroke; }
//            set { pOrderDnOutlineStroke = value; }
//        }
	
		
		
	

		internal class BodyMode2232331 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Executions", "Orders"} );
			}
		}	
		
		private string pCopierMode = "Executions";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "", Name = "",  Order = -10, Description = "")]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode2232331))]
		public string CopierMode
		{
			get { return pCopierMode; }
			set { pCopierMode = value; }
		}
		
		
		
		
	private string pShowAccountsMain = "All";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Accounts To Display", GroupName = "Account Management", Order = -10, Description = "")]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode223231))]
		public string ShowAccountsMain
		{
			get { return pShowAccountsMain; }
			set { pShowAccountsMain = value; }
		}
		
		
	
	
		private string pAccountDisplayType = "Name";
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Display Name", GroupName = "Account Management", Order = 0, Description = "set 'Display Name' to show the exact name for accounts as seen in the NinjaTrader Control Center, Accounts tab.")]
		
//		[TypeConverter(typeof(BodyMode2232356789))]
//		public string AccountDisplayType
//		{
//			get { return pAccountDisplayType; }
//			set { pAccountDisplayType = value; }
//		}

		internal class BodyMode2232356789 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Display Name", "Name"} );
			}
		
			
			
		}		
		
			
	private string pAccountFilter = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Main Account Filter", Description = "only display accounts with the included string.", GroupName = "Account Management", Order = 1)]
        public string AccountFilter
        {
            get { return pAccountFilter; }
            set { pAccountFilter = value; }
        }		
		
		
		
			
		
		private string pShowAccounts = "All";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Name = "Show Accounts", Order = 31, Description = "")]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode223231))]
		public string ShowAccounts
		{
			get { return pShowAccounts; }
			set { pShowAccounts = value; }
		}
		
		
						
		private bool pAccountsSimEnabled = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Disable On Load", GroupName = "Safety", Order = 20)]
        public bool AccountsSimEnabled
        {
            get { return pAccountsSimEnabled; }
            set { pAccountsSimEnabled = value; }
        }	
				
						
		private bool pAccountsLiveEnabled = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Disable On Load", GroupName = "Safety", Order = 20)]
        public bool AccountsLiveEnabled
        {
            get { return pAccountsLiveEnabled; }
            set { pAccountsLiveEnabled = value; }
        }			
		
	
		
	

		internal class BodyMode223231 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"All", "Live", "Sim"} );
			}
		}	
		
		
		
		
		
		
		
        private double pProtectSec = 3;
        [Range(1, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Protect ATM Strategy (Seconds)", GroupName = "Advanced", Order = 131)]
        public double ProtectSec
        {
            get { return pProtectSec; }
            set { pProtectSec = value; }
        }
		
		
		
		
		private string pSupportCode = "";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Support Codes", Description = "", GroupName = "Advanced", Order = 10)]
        public string SupportCode
        {
            get { return pSupportCode; }
            set { pSupportCode = value; }
        }		
			

			
		private string[] pAllAccountData = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Advanced", Name = "Accounts - Settings", Order = 20, Description = "")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountData
        {
            get { return pAllAccountData; }
            set { pAllAccountData = value; }
        }	
	
		private string[] pAllAccountCashValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Advanced", Name = "Accounts - Cash Value", Order = 21, Description = "")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountCashValue
        {
            get { return pAllAccountCashValue; }
            set { pAllAccountCashValue = value; } 
        }	
	
		private string[] pAllAccountNetLiquidation = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Advanced", Name = "Accounts - Net Liquidation", Order = 22, Description = "")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountNetLiquidation
        {
            get { return pAllAccountNetLiquidation; }
            set { pAllAccountNetLiquidation = value; } 
        }	
		
		
		private bool pRemoveClosedATMStrategies = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Remove Closed ATM Strategy", GroupName = "Advanced", Order = 129, Description = "Duplicate Account Actions will detect when an ATM Strategy was not submitted correctly in the Follower accounts, and resubmit it.")]
//        public bool RemoveClosedATMStrategies
//        {
//            get { return pRemoveClosedATMStrategies; }
//            set { pRemoveClosedATMStrategies = value; }
//        }
		
		
		private bool pProtectPosition = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Protect ATM Strategy", GroupName = "Advanced", Order = 130, Description = "Duplicate Account Actions will detect when an ATM Strategy was not submitted correctly in the Follower accounts, and resubmit it.")]
//        public bool ProtectPosition
//        {
//            get { return pProtectPosition; }
//            set { pProtectPosition = value; }
//        }
		


		
		
		
		
		
		private string[] pAllAccountAutoLiquidate = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Management", Name = "Auto Liquidate Peak Balance ($)", Order = 20, Description = "")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountAutoLiquidate
        {
            get { return pAllAccountAutoLiquidate; }
            set { pAllAccountAutoLiquidate = value; } 
        }	
		
		private string[] pAllAccountDailyGoal = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Management", Name = "Daily Goal ($)", Order = 22, Description = "Make more precise adjustments to the daily goal for each account.")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountDailyGoal
        {
            get { return pAllAccountDailyGoal; }
            set { pAllAccountDailyGoal = value; } 
        }	
		
			
			
			
		private string[] pAllAccountDailyLoss = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Management", Name = "Daily Loss ($)", Order = 24, Description = "Make more precise adjustments to the daily loss for each account.")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountDailyLoss
        {
            get { return pAllAccountDailyLoss; }
            set { pAllAccountDailyLoss = value; } 
        }	
				
		private string[] pAllAccountPayouts = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""};
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Account Management", Name = "Payout Requested ($)", Order = 25, Description = "Make more precise adjustments to the payout requested for each account.")]
		//[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public string[] AllAccountPayouts
        {
            get { return pAllAccountPayouts; }
            set { pAllAccountPayouts = value; } 
        }			
		

		private bool pOldColumnDailyLoss = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Column - Daily Loss", Description = "", GroupName = "Account Risk Manager", Order = 81)]
        public bool OldColumnDailyLoss
        {
            get { return pOldColumnDailyLoss; }
            set { pOldColumnDailyLoss = value; }
        }			
		

		
		
		private string pEventToSend = "Execution Update";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Advanced", Name = "Event", Description = "",  Order = 31)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BodyMode2233423))]
		public string EventToSend
		{
			get { return pEventToSend; }
			set { pEventToSend = value; }
		}

		internal class BodyMode2233423 : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"Execution Update", "Order Update"} );
			}
		}			
		
		

			

		private bool pEnableTradingViewDetections = true;
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "TradingView", Name = "Enabled", Description = "",  Order = 20)]
       [RefreshProperties(RefreshProperties.All)]
		public bool EnableTradingViewDetections
        {
            get { return pEnableTradingViewDetections; }
            set { pEnableTradingViewDetections = value; }
        }		
				
		
		private string pTradingViewExitDetection = "Limit";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Exit Order Detection", GroupName = "TradingView", Order = 31 )]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(DGDisplfdghjayMode))]
		public string TradingViewExitDetection
		{
			get { return pTradingViewExitDetection; }
			set { pTradingViewExitDetection = value; }
		}
		
				
		private bool pMatchStopLossPrices = true;
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "TradingView", Name = "Match Original Stop Loss", Description = "",  Order = 60)]
        public bool MatchStopLossPrices
        {
            get { return pMatchStopLossPrices; }
            set { pMatchStopLossPrices = value; }
        }			
		#endregion

		
		internal class DGDisplfdghjayMode : StringConverter
		{
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
			//true means show a combobox
				return true;
			}
			
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
			//true will limit to list. false will show the list, but allow free-form entry
				return true;
			}
		
			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return new StandardValuesCollection( new String[] {"None", "Bracket", "Limit"} );
			}
		}	
		
		
		

		
//		private SimpleFont pTextFont222 = new SimpleFont("Segoe UI Emoji", 13);
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Labels Font", Description = "", GroupName = "Arrows", Order = 52)]
//		public SimpleFont TextFont4
//        {
//            get { return pTextFont; }
//            set { pTextFont = value; }
//        }	
				
			//[ NinjaScriptProperty ]
//			[Browsable(false)]
//			[ Display (Name = "Input Series", Order = 0 , GroupName= "Data Series" )]
//			public string InputUI
//			{get; set; }			
			
			
			
			
	}
	
		
	
		// Hide UserDefinedValues properties when not in use by the HLCCalculationMode.UserDefinedValues
	// When creating a custom type converter for indicators it must inherit from NinjaTrader.NinjaScript.IndicatorBaseConverter to work correctly with indicators
	public class aiDuplicateAccountActionBensConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		//#region -- Converter
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			
			
			aiDuplicateAccountsActionsBen   jbb = (aiDuplicateAccountsActionsBen) value;
			
	
		
			
			//Pivots						thisPivotsInstance			= (Pivots) value;
			
			//bool MagnetsOn = ;
			
			List<string> DeleteThese = new List<string>();
			List<string> DeleteThese2 = new List<string>();
			
			
			
			
			DeleteThese2.Add("TheCopierMode");
			DeleteThese2.Add("CopierMode");
			
			
			
			DeleteThese2.Add("ForceAllSelectedAccountsToTop");
			
			
			DeleteThese2.Add("ChartTraderNeedsDisabled");
			
			
			
			
			// uncomment for troubleshooting
			
			if (jbb.SupportCode.Contains("cash"))
			{
				
				
				
			}
			else
			{
				
				
	
		
	
			
				if (jbb.TheCopierMode == "Executions")
				{
					DeleteThese2.Add("SubmitATMOnEntry");
					
				}
				
				
				if (!jbb.SupportCode.Contains("accounts"))
				{
					DeleteThese2.Add("AllAccountData");
					DeleteThese.Add("AllAccountCashValue");
					DeleteThese.Add("AllAccountNetLiquidation");
				}
	
		
				
				
				if (!jbb.SupportCode.Contains("events"))
					DeleteThese2.Add("EventToSend");			
							
				
			}
		
				

				DeleteThese2.Add("DailyGoalDisplayMode");
			DeleteThese2.Add("DailyLossDisplayMode");
			
			DeleteThese2.Add("AffiliateLinkDone1");
			DeleteThese2.Add("AffiliateLinkDone2");
			DeleteThese2.Add("FirstLoadIsDone");
			
			
			
		
			
			if (!jbb.EnableTradingViewDetections)
			{	
				
				DeleteThese.Add("TradingViewExitDetection");
				DeleteThese.Add("MatchStopLossPrices");
				
			}
			
		
		
			if (!jbb.OneTradeAll)
			{	
				
				DeleteThese.Add("OneTradeEvalEnabled");
				DeleteThese.Add("OneTradePAEnabled");
				DeleteThese.Add("OneTradeShow");				
				
			}
		
			//if (!jbb.AlertEnabled)
			{			
			
//				DeleteThese.Add("AlertLogEnabled");
//				DeleteThese.Add("AudioEnabled");
//				DeleteThese.Add("WAVFileName");
//				DeleteThese.Add("ArrowUpFBrush");	
//				DeleteThese.Add("WAVFileName2");
//				DeleteThese.Add("ArrowDownFBrush");	
//				DeleteThese.Add("SpeechEnabled");	
//				DeleteThese.Add("VoiceName");
//				DeleteThese.Add("ThisIndyName");			
//				DeleteThese.Add("VoiceMode");	
				
			}
			

		
			DeleteThese.Add("ShowPercentOnGoalLoss");
			
			
			
			DeleteThese.Add("IsCopyBasicFunctionsPermission");
			DeleteThese.Add("IsRiskFunctionsPermission");
			DeleteThese.Add("IsCopyBasicFunctionsEnabled");
			DeleteThese.Add("IsRiskFunctionsEnabled");			
			
	
		
			DeleteThese2.Add("LastMultiplierMode");
			
//			if (!jbb.IsXColumnEnabled)
//			{
//				DeleteThese.Add("MultiplierMode");
//			}
		
			
			
//			if (!jbb.IsCrossColumnEnabled)
//			{
//				DeleteThese.Add("CrossType");		
//			}
			
	
	
		
			if (!jbb.RejectedOrderHandling)
			{
				
				
				DeleteThese.Add("CheckBeforeSubmitting");
				DeleteThese.Add("RejectedSubmit");
				DeleteThese.Add("ResubmitMaster");
				DeleteThese.Add("RejectedSubmitOff");
				DeleteThese.Add("MatchStopLimit");					
				
				
			}
			else
			{
				if (jbb.RejectedSubmit != "Limit")
				{
					DeleteThese.Add("RejectedSubmitOff");
					DeleteThese.Add("MatchStopLimit");					
					
				}
				
			}
			
	
		
					DeleteThese.Add("CurrentVersionName");
					DeleteThese.Add("PreviousVersionName");			

			
			
//			if (!jbb.ActionsColumnEnabled)
//			{
				
				
//				DeleteThese.Add("ActionsHideColumnEnabled");
//				DeleteThese.Add("ActionsButtonLocation");
//				DeleteThese.Add("OneTradeEvalEnabled");
//				DeleteThese.Add("OneTradePAEnabled");				
//				DeleteThese.Add("OneTradeShow");	
				
//			}
	
		
			
			
			
			DeleteThese.Add("OldColumnDailyLoss");
			
			if (!jbb.PrivacyEnabled)
			{
				
				DeleteThese.Add("AccountPrivacy1");
				DeleteThese.Add("AccountReplace1");
				DeleteThese.Add("AccountPrivacy2");
				DeleteThese.Add("AccountReplace2");
				DeleteThese.Add("AccountPrivacy3");
				DeleteThese.Add("AccountReplace3");	
				DeleteThese.Add("AccountPrivacy4");
				DeleteThese.Add("AccountReplace4");
				DeleteThese.Add("AccountPrivacy5");
				DeleteThese.Add("AccountReplace5");	
				DeleteThese.Add("AccountPrivacy6");
				DeleteThese.Add("AccountReplace6");	
				DeleteThese.Add("AccountPrivacy7");
				DeleteThese.Add("AccountReplace7");
				DeleteThese.Add("AccountPrivacy8");
				DeleteThese.Add("AccountReplace8");					
				DeleteThese.Add("AccountPrivacy9");
				DeleteThese.Add("AccountReplace9");	
				DeleteThese.Add("AccountPrivacy10");
				DeleteThese.Add("AccountReplace10");	
				DeleteThese.Add("AccountPrivacy11");
				DeleteThese.Add("AccountReplace11");
				DeleteThese.Add("AccountPrivacy12");
				DeleteThese.Add("AccountReplace12");					
				
			}
				
			
						
		
			
		if (!jbb.ShowFlattenEverything)
			{			
			
				DeleteThese.Add("FLString");
				DeleteThese.Add("FlattenButtonB");
				DeleteThese.Add("ShowFEMessage");
				
			}									
	
			
			
		DeleteThese.Add("ExitShieldIsEnabled");	
			
		if (!jbb.ExitShieldFeaturesEnabled)
			{			
			
				DeleteThese.Add("ExitShieldButtonEnabled");
				DeleteThese.Add("ExitShieldStopLoss");
				DeleteThese.Add("ExitShieldProfitTarget");
				DeleteThese.Add("UseOriginalLocation");
				DeleteThese.Add("ExitShieldMessages");
				
				
			}									
	
			
			
	
		
				
		
			
			if (!jbb.ShowRefreshPositions)
			{			
			
				DeleteThese.Add("RPString");
				DeleteThese.Add("RefreshButtonB");
				DeleteThese.Add("ShowFRPMessage");
				
			}				
		
				
			if (!jbb.KeyboardEnabled)
			{			
			
				DeleteThese.Add("KeyScrollUp");
				DeleteThese.Add("KeyScrollDn");
				DeleteThese.Add("UseNumberForMultiplier");
				
				
			}	
			
			
		
			
			if (!jbb.ShowTotalRows)
			{			
			
				DeleteThese.Add("Total1Filter");
				DeleteThese.Add("Total2Filter");
				DeleteThese.Add("Total3Filter");
				DeleteThese.Add("Total5Filter");
				DeleteThese.Add("Total6Filter");
				DeleteThese.Add("Total7Filter");				
				DeleteThese.Add("ShowGrandTotal");	
	
				DeleteThese.Add("Total1Name2");	
				DeleteThese.Add("Total2Name2");	
				DeleteThese.Add("Total3Name2");	
				DeleteThese.Add("Total4Name2");	
				DeleteThese.Add("Total5Name2");	
				DeleteThese.Add("Total6Name2");	
				DeleteThese.Add("Total7Name2");					
				
			}
					
	
		
	
			if (!jbb.IsRiskFunctionsPermission || !jbb.IsRiskFunctionsChecked)
			{
				
				DeleteThese.Add("AllAccountAutoLiquidate");	
				DeleteThese.Add("AllAccountDailyGoal");	
				DeleteThese.Add("AllAccountDailyLoss");	
				DeleteThese.Add("AllAccountPayouts");	
				
			}
			
			
			
				
			
			DeleteThese2.Add("ColumnAudioE");
	
			DeleteThese2.Add("ColumnEMAE");
			DeleteThese2.Add("ColumnEntryE");
			DeleteThese2.Add("ColumnTimeStatusE");			
			DeleteThese2.Add("ColumnTrendStatusE");		
		
		
			DeleteThese2.Add("ColumnLastE");
			DeleteThese2.Add("ColumnEntryE");
			DeleteThese2.Add("ColumnStopE");			
			DeleteThese2.Add("ColumnT1E");
			DeleteThese2.Add("ColumnT2E");
			DeleteThese2.Add("ColumnT3E");		
			
		
		
		
			
			
			
	
		
			DeleteThese2.Add("AlertEnabled");
			DeleteThese2.Add("AlertLogEnabled");
			DeleteThese2.Add("AudioEnabled");			
			DeleteThese2.Add("WAVFileName");
			DeleteThese2.Add("WAVFileName2");
			DeleteThese2.Add("ArrowUpFBrush");
			DeleteThese2.Add("ArrowDownFBrush");			
			DeleteThese2.Add("SpeechEnabled");
			DeleteThese2.Add("ThisIndyName");
			DeleteThese2.Add("VoiceMode");
		
		
				
			DeleteThese2.Add("HideAccountsIsEnabled");			
			DeleteThese2.Add("HideCurrencyIsEnabled");			
			
			
			DeleteThese2.Add("CopierIsEnabled");			
			DeleteThese2.Add("SelectedAscending");
			DeleteThese2.Add("AllInstruments");
			DeleteThese2.Add("IsCrossEnabled");			
			DeleteThese2.Add("IsXEnabled");
			DeleteThese2.Add("IsATMSelectEnabled");
			
			
			DeleteThese2.Add("IsFadeEnabled");
			DeleteThese2.Add("IsBuildMode");
			
			
			
			DeleteThese2.Add("SelectedAccount");
			DeleteThese2.Add("ThisMasterAccount");			
			
			DeleteThese2.Add("ShowAccounts");
		
		
			DeleteThese2.Add("AccountsLiveEnabled");
			DeleteThese2.Add("AccountsSimEnabled");
			
			
			
		
		
		DeleteThese2.Add("DisplayType");
			DeleteThese2.Add("SelectedColumn");
		DeleteThese2.Add("ShowAllPriceColE");
			
			
			
			DeleteThese.Add("IsAutoScale");
			DeleteThese.Add("Displacement");
			DeleteThese.Add("DisplayInDataBox");
			DeleteThese.Add("Panel");
			DeleteThese.Add("PaintPriceMarkers");
			DeleteThese.Add("ScaleJustification");
			DeleteThese.Add("IsVisible");	
			DeleteThese.Add("MaximumBarsLookBack");
			DeleteThese.Add("Name");
			
			
			//DeleteThese.Add("Input");
			
			
      	DeleteThese2.Add("Calculate");
//			DeleteThese2.Add("Label");
//			DeleteThese2.Add("Maximum bars look back");
			DeleteThese2.Add("Input series");
			

			
			if (DeleteThese.Count == 0 && DeleteThese2.Count == 0)
				return propertyDescriptorCollection;

			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				
				
				if (DeleteThese.Contains(thisDescriptor.Name))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				
				else if (DeleteThese2.Contains(thisDescriptor.DisplayName))	
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				
				else if (thisDescriptor.Category == "Data Series")	
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else
					adjusted.Add(thisDescriptor);
				
			}
			return adjusted;
			
			
		
			
		}
		//#endregion
	}
	
	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private aiDuplicateAccountsActionsBen[] cacheaiDuplicateAccountsActionsBen;
		public aiDuplicateAccountsActionsBen aiDuplicateAccountsActionsBen(double protectSec)
		{
			return aiDuplicateAccountsActionsBen(Input, protectSec);
		}

		public aiDuplicateAccountsActionsBen aiDuplicateAccountsActionsBen(ISeries<double> input, double protectSec)
		{
			if (cacheaiDuplicateAccountsActionsBen != null)
				for (int idx = 0; idx < cacheaiDuplicateAccountsActionsBen.Length; idx++)
					if (cacheaiDuplicateAccountsActionsBen[idx] != null && cacheaiDuplicateAccountsActionsBen[idx].ProtectSec == protectSec && cacheaiDuplicateAccountsActionsBen[idx].EqualsInput(input))
						return cacheaiDuplicateAccountsActionsBen[idx];
			return CacheIndicator<aiDuplicateAccountsActionsBen>(new aiDuplicateAccountsActionsBen(){ ProtectSec = protectSec }, input, ref cacheaiDuplicateAccountsActionsBen);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.aiDuplicateAccountsActionsBen aiDuplicateAccountsActionsBen(double protectSec)
		{
			return indicator.aiDuplicateAccountsActionsBen(Input, protectSec);
		}

		public Indicators.aiDuplicateAccountsActionsBen aiDuplicateAccountsActionsBen(ISeries<double> input , double protectSec)
		{
			return indicator.aiDuplicateAccountsActionsBen(input, protectSec);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.aiDuplicateAccountsActionsBen aiDuplicateAccountsActionsBen(double protectSec)
		{
			return indicator.aiDuplicateAccountsActionsBen(Input, protectSec);
		}

		public Indicators.aiDuplicateAccountsActionsBen aiDuplicateAccountsActionsBen(ISeries<double> input , double protectSec)
		{
			return indicator.aiDuplicateAccountsActionsBen(input, protectSec);
		}
	}
}

#endregion
