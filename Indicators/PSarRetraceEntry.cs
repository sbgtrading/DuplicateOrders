//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Parabolic SAR according to Stocks and Commodities magazine V 11:11 (477-479).
	/// </summary>
	public class PSarRetraceEntry : Indicator
	{
		private const int UP = 1;
		private const int DOWN = -1;
		private double 			af;				// Acceleration factor
		private bool 			afIncreased;
		private bool   			longPosition;
		private int 			prevBar;
		private double 			prevSAR;
		private int 			reverseBar;
		private double			reverseValue;
		private double 			todaySAR;		// SAR value
		private double 			xp;				// Extreme Price

		private Series<double> 	afSeries;
		private Series<bool> 	afIncreasedSeries;
		private Series<bool>   	longPositionSeries;
		private Series<int> 	prevBarSeries;
		private Series<double> 	prevSARSeries;
		private Series<int> 	reverseBarSeries;
		private Series<double>	reverseValueSeries;
		private Series<double> 	todaySARSeries;
		private Series<double> 	xpSeries;
		private double retrace_buyprice = double.MinValue;
		private double retrace_sellprice = double.MinValue;
		private Brush BoBuyBkgTintedBrush = null;
		private Brush BoSellBkgTintedBrush = null;
		private double LastValidBOEntryPrice = double.NaN;
		private int TrendDirection = 0;
		private string KeyStrokesText="";
		private string timestr = "\n00:00:00";
		private List<int> ATM_SL_Ticks = new List<int>();
		private List<int> ATM_T_Ticks = new List<int>();
		private bool InSession = false;

		protected override void OnStateChange()
		{
//if(State!=null) Print("State: "+State.ToString());
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionParabolicSAR;
				Name						= "PSarRetraceEntry";
				Acceleration				= 0.02;
				AccelerationStep			= 0.02;
				AccelerationMax				= 0.2;
				Calculate 					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				pRewardToRisk = 1.5;
				pBoBuyWAV = "SOUND OFF";
				pBoSellWAV = "SOUND OFF";
				pTrendReverseUpWAV = "SOUND OFF";
				pTrendReverseDnWAV = "SOUND OFF";
				pBoBuyBackgroundBrush = Brushes.Lime;
				pBoSellBackgroundBrush = Brushes.Magenta;
				pBkgOpacity = 10;
				pMaxSignalCount = 1;
				pSelectedAccountName = "N/A";
				pTradingQty = 2;
				pATMStrategyName = "";
				pBuyMarketKeystroke = PSarRetraceEntry_Keys.OFF;
				pSellMarketKeystroke = PSarRetraceEntry_Keys.OFF;
				pEnterTrendKeystroke = PSarRetraceEntry_Keys.Alt_Z;
				pTextFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial",12);
				pTxtPosition = TextPosition.TopLeft;
				pStartTime1 = 900;
				pEndTime1 = 1100;
				pStartTime2 = 1400;
				pEndTime2 = 1700;
				pAccountSize = 50000;
				pPctRiskPerTrade = 1.5;

				AddPlot(new Stroke(Brushes.Goldenrod, 2), PlotStyle.Dot, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameParabolicSAR);
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Hash, "BuyLvl");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Hash, "SellLvl");
				AddPlot(new Stroke(Brushes.Lime, 4), PlotStyle.TriangleRight, "BoBuyLvl");
				AddPlot(new Stroke(Brushes.Magenta, 4), PlotStyle.TriangleRight, "BoSellLvl");
			}

			else if (State == State.Configure)
			{
	 			xp				= 0.0;
		 		af				= 0;
		 		todaySAR		= 0;
		 		prevSAR			= 0;
		 		reverseBar		= 0;
		 		reverseValue	= 0;
		 		prevBar			= 0;
		 		afIncreased		= false;
				KeyStrokesText = (pEnterTrendKeystroke != PSarRetraceEntry_Keys.OFF ? string.Format("{0} TrendEntry{1}", pEnterTrendKeystroke.ToString().Replace("_","-"),Environment.NewLine) : string.Empty)
								+(pBuyMarketKeystroke  != PSarRetraceEntry_Keys.OFF ? string.Format("{0} Buy Mkt{1}",    pBuyMarketKeystroke.ToString().Replace("_","-"), Environment.NewLine) : string.Empty)
								+(pSellMarketKeystroke != PSarRetraceEntry_Keys.OFF ? string.Format("{0} Sell Mkt",      pSellMarketKeystroke.ToString().Replace("_","-")):string.Empty);
				KeyStrokesText = Environment.NewLine + KeyStrokesText.Trim();

			}
			else if (State == State.DataLoaded)
			{
				if(pATMStrategyName == null)
					pATMStrategyName = "";
				else
					pATMStrategyName = pATMStrategyName.Trim();

				if(pBoBuyBackgroundBrush != Brushes.Transparent && pBkgOpacity>0){
					BoBuyBkgTintedBrush = pBoBuyBackgroundBrush.Clone();
					BoBuyBkgTintedBrush.Opacity = pBkgOpacity/100f;
				}
				if(pBoSellBackgroundBrush != Brushes.Transparent && pBkgOpacity>0){
					BoSellBkgTintedBrush = pBoSellBackgroundBrush.Clone();
					BoSellBkgTintedBrush.Opacity = pBkgOpacity/100f;
				}
				ReversalBars.Add(-1);
				ReversalBars.Add(0);
				if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
				{
					afSeries			= new Series<double>(this);
					afIncreasedSeries	= new Series<bool>(this);
					longPositionSeries	= new Series<bool>(this, MaximumBarsLookBack.Infinite);
					prevBarSeries		= new Series<int>(this);
					prevSARSeries		= new Series<double>(this);
					reverseBarSeries	= new Series<int>(this);
					reverseValueSeries	= new Series<double>(this);
					todaySARSeries		= new Series<double>(this);
					xpSeries			= new Series<double>(this);
				}
				if(ChartPanel!=null){
					ChartPanel.KeyUp  += OnKeyUp;
					ChartPanel.KeyDown  += OnKeyDown;
					ChartPanel.MouseWheel  += OnMouseWheel;
					ChartPanel.MouseMove  += OnMouseMove;
				}
			}
			else if(State == State.Realtime){
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
//							else
//								Draw.TextFixed(this, "BottomRightText", "Waiting on data", TextPosition.BottomRight, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
						}
					}
				}
			}
			else if(State == State.Terminated){
				if(ChartPanel!=null){
					ChartPanel.KeyUp  -= OnKeyUp;
					ChartPanel.KeyDown -= OnKeyDown;
					ChartPanel.MouseWheel -= OnMouseWheel;
					ChartPanel.MouseMove  -= OnMouseMove;
				}
				if (timer != null){
					timer.IsEnabled = false;
					timer = null;
				}
			}
		}
		#region -- Timer variables and methods --
		private DateTime		now		 	= Core.Globals.Now;
		private bool			connected, hasRealtimeData;
		private SessionIterator sessionIterator;
		private System.Windows.Threading.DispatcherTimer timer;
		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Connected
				&& connectionStatusUpdate.Connection.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType))
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
			if (DisplayTime()){
				if (timer != null && !timer.IsEnabled)
					timer.IsEnabled = true;

				if (connected)
				{
//					if (hasRealtimeData)
						timestr = string.Format("\n{0}", Now.ToString("HH:mm:ss"));
				}
				else
				{
					if (timer != null){
						timer.IsEnabled = false;
						timestr = "\ntimer off";
					}
				}
			}
			ForceRefresh();
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

		private string AddSoundFolder(string wav){
			wav = wav.Replace("<inst>",Instrument.MasterInstrument.Name);
			//Print(wav);
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
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
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("SOUND OFF");
				list.Add("<inst>_BuyBreakout.wav");
				list.Add("<inst>_SellBreakout.wav");
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
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
        #region -- Plots --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PSar { get { return Values[0]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BuyLvl { get { return Values[1]; } }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> SellLvl { get { return Values[2]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BoBuyLvl { get { return Values[3]; } }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BoSellLvl { get { return Values[4]; } }
		#endregion
		//======================================================================
		private double Round2Tick(double p){
			return Instrument.MasterInstrument.RoundToTickSize(p);
		}
		//======================================================================
		private Account myAccount;
		private bool IsAltPressed = false;
		private int MouseY = 0;
		//======================================================================
		#region --- Mouse and keyboard methods ---
		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			Point coords = e.GetPosition(ChartPanel);
			MouseY = ChartingExtensions.ConvertToVerticalPixels(coords.Y, ChartControl.PresentationSource);
			//Print("MouseY: "+MouseY);
		}
		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			//e.Handled = true;
			if(Keyboard.IsKeyDown(Key.LeftAlt)) {
				e.Handled = true;
				pTradingQty = pTradingQty+ (e.Delta>0 ? 1:-1);
				pTradingQty = Math.Max(1,pTradingQty);
				if(pTextFont.Size>4){
					RemoveDrawObject("avg");
					if(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Volume || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Tick || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Range || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Renko || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak)
						Draw.TextFixed(this, "avg",
//								string.Format("10bar {0}  100bar {1}\nBar {2}\nQty: {3}\nATM: {4}\nAcct: {5}{6}", avg10.ToString("0.0"), avg100.ToString("0.0"), BarsArray[0].PercentComplete.ToString("0%"), pTradingQty, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
								string.Format("10bar {0}  100bar {1}\nBar {2}\nQty: {3}\nSugg Pos: {4}\nATM: {5}\nAcct: {6}{7}{8}", avg10.ToString("0.0"), avg100.ToString("0.0"), BarsArray[0].PercentComplete.ToString("0%"), pTradingQty, SuggestedPosition, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText, timestr), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
					else
						Draw.TextFixed(this, "avg", string.Format("Qty: {0}\nSugg Pos: {1}\nATM: {2}\nAcct: {3}{4}", pTradingQty, SuggestedPosition, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
				}
			}else if(Keyboard.IsKeyDown(Key.LeftCtrl)){
				e.Handled = true;
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
//	Print((list.Count-1)+" list:  "+list.Last());
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						list.Add(fi.Name.Replace(".xml",string.Empty).Trim());
//	Print((list.Count-1)+" list:  "+list.Last());
					}
				}
//	Print("In:  "+pATMStrategyName);
				if(pATMStrategyName.Trim()==""){
					pATMStrategyName = list[0];
				}else{
					int i = 0;
					if(e.Delta>0){
						for(i = 0; i<list.Count; i++){
//	Print("i:  "+list[i]);
							if(list[i]==pATMStrategyName.Trim())
								break;
						}
						i++;
						if(i>= list.Count) pATMStrategyName = list[0];
						else pATMStrategyName = list[i];
					}else{
						for(i = list.Count-1; i>0; i--){
//	Print(i+":  "+list[i]);
							if(list[i]==pATMStrategyName.Trim())
								break;
						}
						i--;
						if(i<0) pATMStrategyName = list.Last();
						else pATMStrategyName = list[i];
					}
					if(LastValidBOEntryPrice<0) SuggestedPosition = -Math.Abs(SuggestedPosition); else SuggestedPosition = Math.Abs(SuggestedPosition);
					if(Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Volume || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Tick || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Range || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Renko || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak)
						Draw.TextFixed(this, "avg",
//								string.Format("10bar {0}  100bar {1}\nBar {2}\nQty: {3}\nATM: {4}\nAcct: {5}{6}", avg10.ToString("0.0"), avg100.ToString("0.0"), BarsArray[0].PercentComplete.ToString("0%"), pTradingQty, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
								string.Format("10bar {0}  100bar {1}\nBar {2}\nQty: {3}\nSugg Pos: {4}\nATM: {5}\nAcct: {6}{7}{8}", avg10.ToString("0.0"), avg100.ToString("0.0"), BarsArray[0].PercentComplete.ToString("0%"), pTradingQty, SuggestedPosition, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText, timestr), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
					else
//						Draw.TextFixed(this, "avg", string.Format("Qty: {0}\nATM: {1}\nAcct: {2}{3}", pTradingQty, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
						Draw.TextFixed(this, "avg", string.Format("Qty: {0}\nSugg Pos: {1}\nATM: {2}\nAcct: {3}{4}", pTradingQty, SuggestedPosition, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
					ForceRefresh();
				}
				if(false && pATMStrategyName!="N/A" && pATMStrategyName.Trim().Length>0){
					//this isn't completely implemented - suggestion is to read all SL ticks and TP ticks on this ATM file, and plot them on the chart with a gray line or "SL" or "T", from the current market price
					string xmlname = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"templates","AtmStrategy",pATMStrategyName.Trim()+".xml");
					var lines = System.IO.File.ReadAllLines(xmlname);
					int x =0;
					ATM_SL_Ticks.Clear();
					ATM_T_Ticks.Clear();
					foreach(var line in lines){
						if(line.Contains("<StopLoss>")){
							var input = line.Replace("<StopLoss>",string.Empty).Replace("</StopLoss>",string.Empty);
							if(int.TryParse(input,out x)) ATM_SL_Ticks.Add(x);
//	var regex = new System.Text.RegularExpressions.Regex("<StopLoss>([0-9]+)</StopLoss>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
//	var x = regex.Matches("<StopLoss>15</StopLoss>");
//	foreach(System.Text.RegularExpressions.Match k in x){
//		foreach(var xx in k.Groups)
//			Print("Regex: "+k.ToString()+"  val: "+xx.ToString());
//	}
						}
						if(line.Contains("<Target>")){
							var input = line.Replace("<Target>",string.Empty).Replace("</Target>",string.Empty);
							if(int.TryParse(input,out x)) ATM_T_Ticks.Add(x);
						}
					}
				}
			}
		}
		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			//e.Handled = true;
		}
		bool LaunchedPopup = false;
//======================================================================
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
//Print("OnKeyDown");
			if(!InSession) {
				if(!LaunchedPopup)
					Log(Instrument.FullName+": Trade utility is not active during off-session time "+pStartTime1+" - "+pEndTime1+" & "+pStartTime2+" - "+pEndTime2, LogLevel.Alert);
				LaunchedPopup = true;
				return;
			}
			if(double.IsNaN(LastValidBOEntryPrice)) return;
//			if(pATMStrategyName=="N/A") return;
//Print("OnKeyDown  267");
//Print("LeftAlt:  "+Keyboard.IsKeyDown(Key.LeftAlt).ToString()+"  LeftShift: "+Keyboard.IsKeyDown(Key.LeftShift).ToString());
			if(e.Key==Key.Escape)
				IsAltPressed = false;
			else if(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)){
				//e.Handled      = true;
				bool TrendEntry = false;
				if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_A && Keyboard.IsKeyDown(Key.A)) TrendEntry = true;
				else if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_C && Keyboard.IsKeyDown(Key.C)) TrendEntry = true;
				else if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_D && Keyboard.IsKeyDown(Key.D)) TrendEntry = true;
				else if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_F && Keyboard.IsKeyDown(Key.F)) TrendEntry = true;
				else if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_S && Keyboard.IsKeyDown(Key.S)) TrendEntry = true;
				else if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_X && Keyboard.IsKeyDown(Key.X)) TrendEntry = true;
				else if(pEnterTrendKeystroke == PSarRetraceEntry_Keys.Alt_Z && Keyboard.IsKeyDown(Key.Z)) TrendEntry = true;
				if(TrendEntry){
					#region Verify account availability
					var accts = Account.All.ToList();
					for(int i = 0; i<accts.Count; i++){
						if(accts[i].Name == pSelectedAccountName){
							accts = null;
							break;
						}
					}
					if(accts == null){
						lock (Account.All)
							myAccount = Account.All.FirstOrDefault(a => a.Name == pSelectedAccountName);
//						CheckForExistingOrdersPositions(myAccount);
					}else{
						myAccount = null;
						string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "PsarRetraceEntry.log");
						var lines = new string[]{"x"};
						if(System.IO.File.Exists(path)) lines = System.IO.File.ReadAllLines(path);
						long MostRecent = long.MaxValue;
						foreach(var line in lines){
							if(line.Contains(string.Format("'{0}'", pSelectedAccountName))) {
								var elements = line.Split(new char[]{'\t'});
								if(elements.Length>0){
									var dt = DateTime.Parse(elements[0]);
									long diff = Now.Ticks - dt.Ticks;
									if(diff<MostRecent) MostRecent=diff;
								}
							}
						}
						var ts = new TimeSpan(MostRecent);
						if(ts.TotalDays>5) System.IO.File.Delete("PsarRetraceEntry.Log");
						else if(ts.TotalMinutes > 10){
							string msg = "ERROR = account '"+pSelectedAccountName+"' is not available to trade";
							Log(msg, LogLevel.Alert);
							System.IO.File.AppendAllText("PsarRetraceEntry.Log", Now.ToString()+"\t"+msg+Environment.NewLine);
						}
					}
					#endregion
//Print("BoBuy: "+BoBuyLvl.IsValidDataPoint(0).ToString()+"  BoSell: "+BoSellLvl.IsValidDataPoint(0).ToString());
					try{
						if(LastValidBOEntryPrice>0){
							double entryp = Round2Tick(LastValidBOEntryPrice);
							var otype = GetCurrentAsk() < entryp ? OrderType.StopMarket : OrderType.Limit;
//Print("Entryp: "+entryp+"   GetCurrentAsk(): "+GetCurrentAsk().ToString());
							var ATMorder = myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.Buy, 
										otype, 
										OrderEntry.Automated, 
										TimeInForce.Day, pTradingQty, entryp, entryp, string.Empty, "Entry", Core.Globals.MaxDate, null); 
//Print("Placing buy "+otype.ToString()+" order: "+ATMorder.ToString());
							NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName, ATMorder);
						}
						else if(LastValidBOEntryPrice<0){
							double entryp = Round2Tick(Math.Abs(LastValidBOEntryPrice));
//Print("Entryp: "+entryp+"   GetCurrentBid(): "+GetCurrentBid().ToString());
							var otype = GetCurrentBid() > entryp ? OrderType.StopMarket : OrderType.Limit;
							var ATMorder = myAccount.CreateOrder(
										Instrument.GetInstrument(Instrument.FullName), 
										OrderAction.Sell, 
										otype, 
										OrderEntry.Automated, 
										TimeInForce.Day, pTradingQty, entryp, entryp, string.Empty, "Entry", Core.Globals.MaxDate, null); 
//Print("Placing sell "+otype.ToString()+" order: "+ATMorder.ToString());
							NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName, ATMorder);

						}
					}catch(Exception er){Print("Order was not placed - do you have your account selected?");}
				}
				else{
					bool BuyMarket = false;
					if(this.pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_A && Keyboard.IsKeyDown(Key.A)) BuyMarket = true;
					else if(pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_C && Keyboard.IsKeyDown(Key.C)) BuyMarket = true;
					else if(pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_D && Keyboard.IsKeyDown(Key.D)) BuyMarket = true;
					else if(pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_F && Keyboard.IsKeyDown(Key.F)) BuyMarket = true;
					else if(pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_S && Keyboard.IsKeyDown(Key.S)) BuyMarket = true;
					else if(pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_X && Keyboard.IsKeyDown(Key.X)) BuyMarket = true;
					else if(pBuyMarketKeystroke == PSarRetraceEntry_Keys.Alt_Z && Keyboard.IsKeyDown(Key.Z)) BuyMarket = true;
					if(BuyMarket){
//Print("OnKeyDown  356");
						#region Verify account availability
						var accts = Account.All.ToList();
						for(int i = 0; i<accts.Count; i++){
							if(accts[i].Name == pSelectedAccountName){
								accts = null;
								break;
							}
						}
						if(accts == null){
							lock (Account.All)
								myAccount = Account.All.FirstOrDefault(a => a.Name == pSelectedAccountName);
	//						CheckForExistingOrdersPositions(myAccount);
						}else{
							myAccount = null;
							string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "PsarRetraceEntry.log");
							var lines = new string[]{"x"};
							if(System.IO.File.Exists(path)) lines = System.IO.File.ReadAllLines(path);
							long MostRecent = long.MaxValue;
							foreach(var line in lines){
								if(line.Contains(string.Format("'{0}'", pSelectedAccountName))) {
									var elements = line.Split(new char[]{'\t'});
									if(elements.Length>0){
										var dt = DateTime.Parse(elements[0]);
										long diff = DateTime.Now.Ticks - dt.Ticks;
										if(diff<MostRecent) MostRecent=diff;
									}
								}
							}
							var ts = new TimeSpan(MostRecent);
							if(ts.TotalDays>5) System.IO.File.Delete("PsarRetraceEntry.Log");
							else if(ts.TotalMinutes > 10){
								string msg = "ERROR = account '"+pSelectedAccountName+"' is not available to trade";
								Log(msg, LogLevel.Alert);
								System.IO.File.AppendAllText("PsarRetraceEntry.Log", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
							}
						}
						#endregion
						try{
							double entryp = Round2Tick(LastValidBOEntryPrice);
							var ATMorder = myAccount.CreateOrder(
											Instrument.GetInstrument(Instrument.FullName), 
											OrderAction.Buy, 
											OrderType.Market, 
											OrderEntry.Automated, 
											TimeInForce.Day, pTradingQty, entryp, entryp, string.Empty, "Entry", Core.Globals.MaxDate, null); 
//Print("Placing buy market order: "+ATMorder.ToString());
							NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName, ATMorder);
						}catch(Exception er){Print("Order was not placed - do you have your account selected?");}
					}else{
						bool SellMarket = false;
//Print("OnKeyDown  405");
						if(this.pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_A && Keyboard.IsKeyDown(Key.A)) SellMarket = true;
						else if(pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_C && Keyboard.IsKeyDown(Key.C)) SellMarket = true;
						else if(pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_D && Keyboard.IsKeyDown(Key.D)) SellMarket = true;
						else if(pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_F && Keyboard.IsKeyDown(Key.F)) SellMarket = true;
						else if(pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_S && Keyboard.IsKeyDown(Key.S)) SellMarket = true;
						else if(pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_X && Keyboard.IsKeyDown(Key.X)) SellMarket = true;
						else if(pSellMarketKeystroke == PSarRetraceEntry_Keys.Alt_Z && Keyboard.IsKeyDown(Key.Z)) SellMarket = true;
						if(SellMarket){
							#region Verify account availability
							var accts = Account.All.ToList();
							for(int i = 0; i<accts.Count; i++){
								if(accts[i].Name == pSelectedAccountName){
									accts = null;
									break;
								}
							}
							if(accts == null){
								lock (Account.All)
									myAccount = Account.All.FirstOrDefault(a => a.Name == pSelectedAccountName);
		//						CheckForExistingOrdersPositions(myAccount);
							}else{
								myAccount = null;
								string path = System.IO.Path.Combine(Core.Globals.UserDataDir, "PsarRetraceEntry.log");
								var lines = new string[]{"x"};
								if(System.IO.File.Exists(path)) lines = System.IO.File.ReadAllLines(path);
								long MostRecent = long.MaxValue;
								foreach(var line in lines){
									if(line.Contains(string.Format("'{0}'", pSelectedAccountName))) {
										var elements = line.Split(new char[]{'\t'});
										if(elements.Length>0){
											var dt = DateTime.Parse(elements[0]);
											long diff = DateTime.Now.Ticks - dt.Ticks;
											if(diff<MostRecent) MostRecent=diff;
										}
									}
								}
								var ts = new TimeSpan(MostRecent);
								if(ts.TotalDays>5) System.IO.File.Delete("PsarRetraceEntry.Log");
								else if(ts.TotalMinutes > 10){
									string msg = "ERROR = account '"+pSelectedAccountName+"' is not available to trade";
									Log(msg, LogLevel.Alert);
									System.IO.File.AppendAllText("PsarRetraceEntry.Log", DateTime.Now.ToString()+"\t"+msg+Environment.NewLine);
								}
							}
							#endregion
							try{
								double entryp = Round2Tick(Math.Abs(LastValidBOEntryPrice));
								var ATMorder = myAccount.CreateOrder(
											Instrument.GetInstrument(Instrument.FullName), 
											OrderAction.Sell, 
											OrderType.Market, 
											OrderEntry.Automated, 
											TimeInForce.Day, pTradingQty, entryp, entryp, string.Empty, "Entry", Core.Globals.MaxDate, null); 
//Print("Placing sell market order: "+ATMorder.ToString());
								NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(pATMStrategyName, ATMorder);
							}catch(Exception er){Print("Order was not placed - do you have your account selected?");}
						}
					}
				}
			}
		}
		#endregion
		private double BoLowPrice = double.NaN;
		private double BoHighPrice = double.NaN;
		private int SignalCount = 0;
		private double HighestHigh = 0;
		private double LowestLow = 0;
		private double avg10 = 0;
		private double avg100 = 0;
		private double SuggestedPosition = 0;
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 3)
				return;

			if (CurrentBar == 3)
			{
				// Determine initial position
				longPosition = High[0] > High[1];
				xp = longPosition ? MAX(High, CurrentBar)[0] : MIN(Low, CurrentBar)[0];
				af = Acceleration;
				PSar[0] = xp + (longPosition ? -1 : 1) * ((MAX(High, CurrentBar)[0] - MIN(Low, CurrentBar)[0]) * af);
				return;
			}
			else if (BarsArray[0].BarsType.IsRemoveLastBarSupported && CurrentBar < prevBar)
			{
				af				= afSeries[0];
				afIncreased		= afIncreasedSeries[0];
				longPosition	= longPositionSeries[0];
				prevBar			= prevBarSeries[0];
				prevSAR			= prevSARSeries[0];
				reverseBar		= reverseBarSeries[0];
				reverseValue	= reverseValueSeries[0];
				todaySAR		= todaySARSeries[0];
				xp				= xpSeries[0];
			}
			InSession = false;
			if(pStartTime1 == pEndTime1 || pStartTime2 == pEndTime2){
				InSession = true;
			}else{
				var t = ToTime(Now)/100;
				if(pStartTime1 != pEndTime1){
					if(t>=pStartTime1 && t<=pEndTime1) InSession = true;
				}
				if(!InSession && pStartTime2 != pEndTime2){
					if(t>=pStartTime2 && t<=pEndTime2) InSession = true;
				}
			}

			if(pTextFont.Size>4){
				if(pSelectedAccountName.Trim().Length==0){
					Draw.TextFixed(this, "avg", "You must supply a valid trading account name to proceed", pTxtPosition, Brushes.Magenta, pTextFont, Brushes.Black, Brushes.Black, 90);
				}else{
					if(CurrentBars[0] > BarsArray[0].Count-4){
						if(CurrentBars[0] > 100 && (Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Volume || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Tick || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Range || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.Renko || Bars.BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak)){
							var tks = Times[0][0] - Times[0][9];
							avg10 = tks.TotalMinutes/10;// /TimeSpan.TicksPerMinute;
//Print(tks.ToString()+"   T[0] "+Times[0][0].ToString()+"  T[9] "+Times[0][9].ToString()+"   tks.TotalMinutes: "+tks.TotalMinutes);
							tks = Times[0][0] - Times[0][99];
							avg100 = tks.TotalMinutes/100;// /TimeSpan.TicksPerMinute;
							Draw.TextFixed(this, "avg", 
								string.Format("10bar {0}  100bar {1}\nBar {2}\nQty: {3}\nSugg Pos: {4}\nATM: {5}\nAcct: {6}{7}{8}", avg10.ToString("0.0"), avg100.ToString("0.0"), BarsArray[0].PercentComplete.ToString("0%"), pTradingQty, SuggestedPosition, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText, timestr), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
						}else
							Draw.TextFixed(this, "avg", string.Format("Qty: {0}\nSugg Pos: {1}\nATM: {2}\nAcct: {3}{4}", pTradingQty, SuggestedPosition, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
//							Draw.TextFixed(this, "avg", string.Format("Qty: {0}\nATM: {1}\nAcct: {2}{3}{4}", pTradingQty, pATMStrategyName, this.pSelectedAccountName, KeyStrokesText, timestr), pTxtPosition, Brushes.DimGray, pTextFont, Brushes.Black, Brushes.Black,90);
					}
				}
			}
			// Reset accelerator increase limiter on new bars
			if (afIncreased && prevBar != CurrentBar)
				afIncreased = false;

			// Current event is on a bar not marked as a reversal bar yet
			if (reverseBar != CurrentBar)
			{
				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(PSar[1] + af * (xp - PSar[1]));
				for (int x = 1; x <= 2; x++)
				{
					if (longPosition)
					{
						if (todaySAR > Low[x])
							todaySAR = Low[x];
					}
					else
					{
						if (todaySAR < High[x])
							todaySAR = High[x];
					}
				}

				// Holding long position
				if (longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || Low[0] < prevSAR)
					{
						PSar[0] = todaySAR;
						prevSAR = todaySAR;
					}
					else
						PSar[0] = prevSAR;

					if (High[0] > xp)
					{
						xp = High[0];
						AfIncrease();
					}
				}

				// Holding short position
				else if (!longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || High[0] > prevSAR)
					{
						PSar[0] = todaySAR;
						prevSAR = todaySAR;
					}
					else
						PSar[0] = prevSAR;

					if (Low[0] < xp)
					{
						xp = Low[0];
						AfIncrease();
					}
				}
			}

			// Current event is on the same bar as the reversal bar
			else
			{
				// Only set new xp values. No increasing af since this is the first bar.
				if (longPosition && High[0] > xp)
					xp = High[0];
				else if (!longPosition && Low[0] < xp)
					xp = Low[0];

				PSar[0] = prevSAR;

				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(longPosition ? Math.Min(reverseValue, Low[0]) : Math.Max(reverseValue, High[0]));
			}

			prevBar = CurrentBar;
bool pTerminateBuySellLines = false;
			// Reverse position
			if(pTerminateBuySellLines){
				if(retrace_buyprice != double.MinValue && Low[0]<=retrace_buyprice)     retrace_buyprice = double.MinValue;
				if(retrace_sellprice != double.MinValue && High[0]>= retrace_sellprice) retrace_sellprice = double.MinValue;
			}

			if ((longPosition && (Low[0] < todaySAR || Low[1] < todaySAR))
				|| (!longPosition && (High[0] > todaySAR || High[1] > todaySAR))){
				PSar[0] = Reverse();
				retrace_buyprice = double.MinValue;
				retrace_sellprice = double.MinValue;
				if(longPosition) retrace_buyprice = Instrument.MasterInstrument.RoundToTickSize(Math.Abs(PSar[0]-PSar[1])*0.5 + Math.Min(PSar[0],PSar[1]))+TickSize;
				else retrace_sellprice = Instrument.MasterInstrument.RoundToTickSize(Math.Abs(PSar[0]-PSar[1])*0.5 + Math.Min(PSar[0],PSar[1]))-TickSize;
			}

			if(retrace_buyprice != double.MinValue)  {
				TrendDirection = UP;
				BuyLvl[0] = retrace_buyprice;
				#region -- Calclate suggested position based on PSar to SellLvl distance --
				if(PSar[0] < BuyLvl[0]){
					double stopdollar = (BuyLvl[0]-PSar[0])*Instrument.MasterInstrument.PointValue;
					double rdollar = pAccountSize * this.pPctRiskPerTrade/100.0;
					SuggestedPosition = Math.Floor(rdollar/stopdollar);
				}else SuggestedPosition = 0;
				#endregion ===============================
				if(!BuyLvl.IsValidDataPoint(1)){
					BoHighPrice = double.NaN;
					BoLowPrice  = double.NaN;
					BoBuyLvl.Reset(0);
					BoSellLvl.Reset(0);
					BackBrushes[0]=null;
					HighestHigh = Highs[0][0];
					SignalCount = 0;
					if(pTrendReverseUpWAV!="SOUND OFF")
						Alert(DateTime.Now.Ticks.ToString(), Priority.High, "PSar Up", AddSoundFolder(pTrendReverseUpWAV), 1, Brushes.Green, Brushes.White);
				}
				if(BuyLvl.IsValidDataPoint(1) && double.IsNaN(BoHighPrice) && double.IsNaN(BoLowPrice)){
					HighestHigh = Math.Max(Highs[0][1], HighestHigh);
					if(Highs[0][1] < HighestHigh){//Closes[0][0] < Opens[0][0]){
						BoHighPrice = HighestHigh;
					}
				}
				if(!double.IsNaN(BoHighPrice) && SignalCount<pMaxSignalCount){
					BoBuyLvl[1] = BoHighPrice+TickSize;
					BoBuyLvl[0] = BoBuyLvl[1];
					if(pBoBuyBackgroundBrush != Brushes.Transparent && pBkgOpacity>0)
						BackBrushes[0] = BoBuyBkgTintedBrush;
					if(Highs[0][0]>BoHighPrice){
						SignalCount++;
						BoBuyLvl.Reset(0);
						BoHighPrice = double.NaN;
						if(pBoBuyWAV!="SOUND OFF")
							Alert(DateTime.Now.Ticks.ToString(), Priority.High, "BO Buy!", AddSoundFolder(pBoBuyWAV), 1, Brushes.Green, Brushes.White);
						if(pBoBuyBackgroundBrush != Brushes.Transparent && pBkgOpacity>0)
							BackBrushes[1] = BoBuyBkgTintedBrush;
					}
				}
			}
			if(retrace_sellprice != double.MinValue){
				TrendDirection = DOWN;
				SellLvl[0] = retrace_sellprice;
				#region -- Calclate suggested position based on PSar to SellLvl distance --
//Print("sell signal  "+PSar[0]+"   SellLvl: "+SellLvl[0]);
				if(PSar[0] > SellLvl[0]){
					double stopdollar = (PSar[0] - SellLvl[0])*Instrument.MasterInstrument.PointValue;
					double rdollar = pAccountSize * this.pPctRiskPerTrade/100.0;
					SuggestedPosition = Math.Floor(rdollar/stopdollar);
//	Print(Times[0][0].ToString()+"   stopdol: "+stopdollar+"   rdollar: "+rdollar+"  result: "+SuggestedPosition);
				}else SuggestedPosition = 0;
				#endregion ===============================
				if(!SellLvl.IsValidDataPoint(1)){
					BoHighPrice = double.NaN;
					BoLowPrice  = double.NaN;
					BoBuyLvl.Reset(0);
					BoSellLvl.Reset(0);
					BackBrushes[0]=null;
					LowestLow   = Lows[0][0];
					SignalCount = 0;
					if(pTrendReverseDnWAV!="SOUND OFF")
						Alert(DateTime.Now.Ticks.ToString(), Priority.High, "PSar Dwon", AddSoundFolder(pTrendReverseDnWAV), 1, Brushes.Red, Brushes.White);
				}
				if(SellLvl.IsValidDataPoint(1) && double.IsNaN(BoHighPrice) && double.IsNaN(BoLowPrice)){
					LowestLow = Math.Min(Lows[0][1], LowestLow);
					if(Lows[0][1] > LowestLow){//Closes[0][0] > Opens[0][0]){
						BoLowPrice = LowestLow;
					}
				}
				if(!double.IsNaN(BoLowPrice) && SignalCount<pMaxSignalCount){
					BoSellLvl[1] = BoLowPrice-TickSize;
					BoSellLvl[0] = BoSellLvl[1];
					if(pBoSellBackgroundBrush != Brushes.Transparent && pBkgOpacity>0)
						BackBrushes[0] = BoSellBkgTintedBrush;
					if(Lows[0][0]<BoLowPrice){
						SignalCount++;
						BoSellLvl.Reset(0);
						BoLowPrice = double.NaN;
						if(pBoSellWAV!="SOUND OFF")
							Alert(DateTime.Now.Ticks.ToString(), Priority.High, "BO Sell!", AddSoundFolder(pBoSellWAV), 1, Brushes.Red, Brushes.White);
						if(pBoSellBackgroundBrush != Brushes.Transparent && pBkgOpacity>0)
							BackBrushes[1] = BoSellBkgTintedBrush;
					}
				}
			}
//			if(BoBuyLvl.IsValidDataPoint(1))
//				LastValidBOEntryPrice = BoBuyLvl[0];
//			if(BoSellLvl.IsValidDataPoint(1))
//				LastValidBOEntryPrice = -BoSellLvl[0];
//			if(BoBuyLvl.IsValidDataPoint(0))
//				LastValidBOEntryPrice = BoBuyLvl[0];
//			if(BoSellLvl.IsValidDataPoint(0))
//				LastValidBOEntryPrice = -BoSellLvl[0];
			

			PSar[0] = Instrument.MasterInstrument.RoundToTickSize(PSar[0]);

			#region -- Draw target lines --
			if(pRewardToRisk>0){
//				try{
				double SLdistance = 0;
				if(BuyLvl.IsValidDataPoint(0) && !BuyLvl.IsValidDataPoint(1)){
					double low = retrace_buyprice;
					int i = 2;
					var endabars = ReversalBars.Where(k=>k < CurrentBar-1).ToList();
					if(endabars!=null && endabars.Count>0){
//						while(CurrentBars[0]-i > 5 && CurrentBars[0]-i > endabars.Max()) {low = Math.Min(low,Low[i]); i++;}
//						double SLdistance = PSar[1] - low;
SLdistance = Math.Abs(PSar[0]-PSar[1]);
						Draw.ArrowLine(this, CurrentBars[0].ToString(), 1, PSar[1], 1, PSar[1] + SLdistance*pRewardToRisk, Brushes.Green);
					}
				}
				if(SellLvl.IsValidDataPoint(0) && !SellLvl.IsValidDataPoint(1)){
					double high = retrace_sellprice;
					int i = 2;
					var endabars = ReversalBars.Where(k=>k < CurrentBar-1).ToList();
					if(endabars!=null && endabars.Count>0){
//						while(CurrentBars[0]-i > 5 && CurrentBars[0]-i > endabars.Max()) {high = Math.Max(high,High[i]); i++;}
//						double SLdistance = high - PSar[1];
SLdistance = Math.Abs(PSar[0]-PSar[1]);
						Draw.ArrowLine(this, CurrentBars[0].ToString(), 1, PSar[1], 1, PSar[1] - SLdistance*pRewardToRisk, Brushes.Red);
					}
				}
//				}catch(Exception e){Print("PSarRetraceEntry e: "+e.ToString());}
			}
			#endregion

			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				afSeries[0]				= af;
				afIncreasedSeries[0]	= afIncreased;
				longPositionSeries[0]	= longPosition;
				prevBarSeries[0]		= prevBar;
				prevSARSeries[0]		= prevSAR;
				reverseBarSeries[0]		= reverseBar;
				reverseValueSeries[0]	= reverseValue;
				todaySARSeries[0]		= todaySAR;
				xpSeries[0]				= xp;
			}
		}
//=============================================================================================================
		private SharpDX.Direct2D1.Brush YellowBrushDX = null;
		private SharpDX.Direct2D1.Brush BlackBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(YellowBrushDX !=null){	YellowBrushDX.Dispose(); YellowBrushDX=null;	}
			if(RenderTarget != null){
				YellowBrushDX = Brushes.Yellow.ToDxBrush(RenderTarget);
			}
			if(BlackBrushDX !=null){	BlackBrushDX.Dispose(); BlackBrushDX=null;	}
			if(RenderTarget != null){
				BlackBrushDX = Brushes.Black.ToDxBrush(RenderTarget);
			}
		}
//======================================================================
		double MousePrice = 0;
		NinjaTrader.Gui.Tools.SimpleFont f = new NinjaTrader.Gui.Tools.SimpleFont("Arial",20);
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			base.OnRender(chartControl, chartScale);
			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(ChartPanel.W, MouseY), 9f, 2f), YellowBrushDX);
			MousePrice = chartScale.GetValueByY(MouseY);//Instrument.MasterInstrument.RoundToTickSize(chartScale.MaxValue - chartScale.MaxMinusMin * MouseY/ChartPanel.H);
			var TextFormat = pTextFont.ToDirectWriteTextFormat();
			var TextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, pTradingQty.ToString(), TextFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
			var labelRect = new SharpDX.RectangleF((ChartPanel.W-TextLayout.Metrics.Width-4f), MouseY + Convert.ToSingle(pTextFont.Size)/2f, TextLayout.Metrics.Width, Convert.ToSingle(pTextFont.Size));
			RenderTarget.DrawText(pTradingQty.ToString(), TextFormat, labelRect, YellowBrushDX);
			LastValidBOEntryPrice = TrendDirection == UP ? MousePrice : -MousePrice;
			if(!InSession){
				string txt = string.Format("Not in session - trading keystrokes are disabled {0} - {1} / {2} - {3}", pStartTime1, pEndTime1, pStartTime2, pEndTime2);
				TextFormat = f.ToDirectWriteTextFormat();
				TextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, txt, TextFormat, (float)(ChartPanel.X + ChartPanel.W),Convert.ToSingle(f.Size*2));
				labelRect = new SharpDX.RectangleF(20f, ChartPanel.H/1.5f, TextLayout.Metrics.Width, Convert.ToSingle(f.Size*2));
				labelRect.Width = labelRect.Width+40f;
				RenderTarget.FillRectangle(labelRect, BlackBrushDX);
				labelRect.Height = labelRect.Height-Convert.ToSingle(f.Size/3);
				labelRect.X = labelRect.X + 20f;
				RenderTarget.DrawText(txt, TextFormat, labelRect, YellowBrushDX);
			}
		}

//======================================================================
		#region Miscellaneous
		// Only raise accelerator if not raised for current bar yet
		private void AfIncrease()
		{
			if (!afIncreased)
			{
				af = Math.Min(AccelerationMax, af + AccelerationStep);
				afIncreased = true;
			}
		}

		// Additional rule. SAR for today can't be placed inside the bar of day - 1 or day - 2.
		private double TodaySAR(double todaySAR)
		{
			if (longPosition)
			{
				double lowestSAR = Math.Min(Math.Min(todaySAR, Low[0]), Low[1]);
				if (Low[0] > lowestSAR)
					todaySAR = lowestSAR;
			}
			else
			{
				double highestSAR = Math.Max(Math.Max(todaySAR, High[0]), High[1]);
				if (High[0] < highestSAR)
					todaySAR = highestSAR;
			}
			return todaySAR;
		}

		private List<int> ReversalBars = new List<int>();
		private double Reverse()
		{
			double todaySAR = xp;

			if ((longPosition && prevSAR > Low[0]) || (!longPosition && prevSAR < High[0]) || prevBar != CurrentBar)
			{
				longPosition = !longPosition;
				reverseBar = CurrentBar;
				ReversalBars.Add(CurrentBar);
				reverseValue = xp;
				af = Acceleration;
				xp = longPosition ? High[0] : Low[0];
				prevSAR = todaySAR;
			}
			else
				todaySAR = prevSAR;
			return todaySAR;
		}
		#endregion

		#region Properties
		#region -- Session time --
		[Range(0, 2359), NinjaScriptProperty]
		[Display(Order = 10, Name = "Start time 1", GroupName = "Session", ResourceType = typeof(Custom.Resource))]
		public int pStartTime1
		{ get; set; }
		[Range(0, 2359), NinjaScriptProperty]
		[Display(Order = 20, Name = "End time 1", GroupName = "Session", ResourceType = typeof(Custom.Resource))]
		public int pEndTime1
		{ get; set; }
		[Range(0, 2359), NinjaScriptProperty]
		[Display(Order = 30, Name = "Start time 2", GroupName = "Session", ResourceType = typeof(Custom.Resource))]
		public int pStartTime2
		{ get; set; }
		[Range(0, 2359), NinjaScriptProperty]
		[Display(Order = 40, Name = "End time 2", GroupName = "Session", ResourceType = typeof(Custom.Resource))]
		public int pEndTime2
		{ get; set; }
		#endregion

		[Range(0.00, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Acceleration", GroupName = "NinjaScriptParameters", Order = 10)]
		public double Acceleration
		{ get; set; }

		[Range(0.001, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccelerationMax", GroupName = "NinjaScriptParameters", Order = 20)]
		public double AccelerationMax
		{ get; set; }

		[Range(0.001, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccelerationStep", GroupName = "NinjaScriptParameters", Order = 30)]
		public double AccelerationStep
		{ get; set; }

		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Reward to risk", GroupName = "NinjaScriptParameters", Order = 40, Description="Set to '0' to turn off target arrows")]
		public double pRewardToRisk
		{ get; set; }

		#region -- Entry Signals --
		[Display(Order = 40, Name = "Max signal count", GroupName = "Entry Signals")]
		public int pMaxSignalCount
		{ get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		[Display(Order = 50, Name = "B.O. Buy WAV", GroupName = "Entry Signals", Description="WAV to play when breakout Buy price is hit", ResourceType = typeof(Custom.Resource))]
		public string pBoBuyWAV
		{ get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		[Display(Order = 60, Name = "B.O. Sell WAV", GroupName = "Entry Signals", Description="WAV to play when breakout sell price is hit", ResourceType = typeof(Custom.Resource))]
		public string pBoSellWAV
		{ get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		[Display(Order = 70, Name = "PSar Up WAV", GroupName = "Entry Signals", Description="WAV to play when psar goes up-trend", ResourceType = typeof(Custom.Resource))]
		public string pTrendReverseUpWAV
		{ get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		[Display(Order = 80, Name = "PSar Down WAV", GroupName = "Entry Signals", Description="WAV to play when psar goes down-trend", ResourceType = typeof(Custom.Resource))]
		public string pTrendReverseDnWAV
		{ get; set; }

		#endregion

		#region -- Racing Stripes --
		[Range(0, 100f)]
		[Display(Order = 10, Name = "Bkg brush opacity", GroupName = "Racing Stripes", Description="", ResourceType = typeof(Custom.Resource))]
		public float pBkgOpacity
		{ get; set; }

		[XmlIgnore]
		[Display(Order = 20, Name = "Buy RacingStripe", GroupName = "Racing Stripes")]
		public Brush pBoBuyBackgroundBrush
		{ get; set; }
				[Browsable(false)]
				public string pBoBuyBackgroundBrushSerializable { get { return Serialize.BrushToString(pBoBuyBackgroundBrush); } set { pBoBuyBackgroundBrush = Serialize.StringToBrush(value); }        }
		[XmlIgnore]
		[Display(Order = 30, Name = "Sell RacingStripe", GroupName = "Racing Stripes")]
		public Brush pBoSellBackgroundBrush
		{ get; set; }
				[Browsable(false)]
				public string pBoSellBackgroundBrushSerializable { get { return Serialize.BrushToString(pBoSellBackgroundBrush); } set { pBoSellBackgroundBrush = Serialize.StringToBrush(value); }        }

		#endregion

		#region -- Order params --
		//======================================================================
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
				
		        return new TypeConverter.StandardValuesCollection(list);
		    }

		    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		    { return true; }
			#endregion
		}

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

		[TypeConverter(typeof(LoadAccountNameList))]
		[Display(Name = "Account Name", GroupName = "Order Parameters", Description = "", Order = 10)]
		public string pSelectedAccountName	{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadATMStrategyNames))]
		[Display(Order = 5, Name = "ATM Strategy Name", GroupName = "Order Parameters", Description = "Optional - select which ATM Strategy name to use.  All stops and targets will be controlled by the ATM Strategy Template")]
		public string pATMStrategyName
		{get;set;}

		[Display(Order = 10, Name = "Trading qty", GroupName = "Order Parameters", Description = "")]
		public int pTradingQty
		{get;set;}

		[Display(Order = 20, Name = "Account Size", GroupName = "Order Parameters", Description = "")]
		public double pAccountSize
		{get;set;}
		[Display(Order = 21, Name = "% risk per trade", GroupName = "Order Parameters", Description = "Calc 'Sugg Pos' between entry hash and current PSar value")]
		public double pPctRiskPerTrade
		{get;set;}

		[Display(Order = 30, Name = "Key - Buy @ Market", GroupName = "Order Parameters", Description = "Keystroke to BUY now, at market")]
		public PSarRetraceEntry_Keys pBuyMarketKeystroke
		{get;set;}

		[Display(Order = 40, Name = "Key - Sell @ Market", GroupName = "Order Parameters", Description = "Keystroke to SELL now, at market")]
		public PSarRetraceEntry_Keys pSellMarketKeystroke
		{get;set;}

		[Display(Order = 50, Name = "Key - Enter Trend", GroupName = "Order Parameters", Description = "Keystroke to place limit/stop entry with trend")]
		public PSarRetraceEntry_Keys pEnterTrendKeystroke
		{get;set;}

		[Display(Order = 60,Name = "Message font", GroupName = "Order Parameters", Description = "")]
		public NinjaTrader.Gui.Tools.SimpleFont pTextFont
		{get;set;}

		[Display(Order = 70,Name = "Message Location", GroupName = "Order Parameters", Description = "")]
		public TextPosition pTxtPosition
		{get;set;}

		#endregion

		#endregion
	}
}
public enum PSarRetraceEntry_Keys {OFF, Alt_A, Alt_C, Alt_D, Alt_F, Alt_S, Alt_X, Alt_Z};

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PSarRetraceEntry[] cachePSarRetraceEntry;
		public PSarRetraceEntry PSarRetraceEntry(int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2, double acceleration, double accelerationMax, double accelerationStep)
		{
			return PSarRetraceEntry(Input, pStartTime1, pEndTime1, pStartTime2, pEndTime2, acceleration, accelerationMax, accelerationStep);
		}

		public PSarRetraceEntry PSarRetraceEntry(ISeries<double> input, int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2, double acceleration, double accelerationMax, double accelerationStep)
		{
			if (cachePSarRetraceEntry != null)
				for (int idx = 0; idx < cachePSarRetraceEntry.Length; idx++)
					if (cachePSarRetraceEntry[idx] != null && cachePSarRetraceEntry[idx].pStartTime1 == pStartTime1 && cachePSarRetraceEntry[idx].pEndTime1 == pEndTime1 && cachePSarRetraceEntry[idx].pStartTime2 == pStartTime2 && cachePSarRetraceEntry[idx].pEndTime2 == pEndTime2 && cachePSarRetraceEntry[idx].Acceleration == acceleration && cachePSarRetraceEntry[idx].AccelerationMax == accelerationMax && cachePSarRetraceEntry[idx].AccelerationStep == accelerationStep && cachePSarRetraceEntry[idx].EqualsInput(input))
						return cachePSarRetraceEntry[idx];
			return CacheIndicator<PSarRetraceEntry>(new PSarRetraceEntry(){ pStartTime1 = pStartTime1, pEndTime1 = pEndTime1, pStartTime2 = pStartTime2, pEndTime2 = pEndTime2, Acceleration = acceleration, AccelerationMax = accelerationMax, AccelerationStep = accelerationStep }, input, ref cachePSarRetraceEntry);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PSarRetraceEntry PSarRetraceEntry(int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2, double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSarRetraceEntry(Input, pStartTime1, pEndTime1, pStartTime2, pEndTime2, acceleration, accelerationMax, accelerationStep);
		}

		public Indicators.PSarRetraceEntry PSarRetraceEntry(ISeries<double> input , int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2, double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSarRetraceEntry(input, pStartTime1, pEndTime1, pStartTime2, pEndTime2, acceleration, accelerationMax, accelerationStep);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PSarRetraceEntry PSarRetraceEntry(int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2, double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSarRetraceEntry(Input, pStartTime1, pEndTime1, pStartTime2, pEndTime2, acceleration, accelerationMax, accelerationStep);
		}

		public Indicators.PSarRetraceEntry PSarRetraceEntry(ISeries<double> input , int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2, double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.PSarRetraceEntry(input, pStartTime1, pEndTime1, pStartTime2, pEndTime2, acceleration, accelerationMax, accelerationStep);
		}
	}
}

#endregion
