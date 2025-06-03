// 
// Copyright (C) 2022, Affordable Indicators, Inc. <www.affordableindicators.com>.
// Affordable Indicators, Inc. reserves the right to modify or overwrite this NinjaScript component with each release.
//





using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools; // SimpleFont
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;
using System.Globalization;

//using SharpDX;
//using SharpDX.Direct2D1;
//using SharpDX.DirectWrite;


using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Text;
using System.IO;
using System.Globalization;


using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Markup;
using System.Xml.Linq;
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;

//using NinjaTrader.NinjaScript.Indicators.;
 


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;


//This namespace holds Indicators in this folder and is required. Do not change it. 

namespace NinjaTrader.NinjaScript.Indicators
{
	
	
	
	[Gui.CategoryOrder("Indicator", -201)]
	[Gui.CategoryOrder("Parameters", -132)]
	[Gui.CategoryOrder("Signals", -131)]
	[Gui.CategoryOrder("Moving Averages", -130)]
	[Gui.CategoryOrder("Fast T3", -120)]
	[Gui.CategoryOrder("Slow T3", -118)]
	[Gui.CategoryOrder("EMA", -116)]
	
	//[Gui.CategoryOrder("MACD BB", -111)]
	

	
	//[Gui.CategoryOrder("Background Color", 4)]
	
	//[Gui.CategoryOrder("Output", 4)]
	
	
	
	[Gui.CategoryOrder("Bar Color", 21)]
	[Gui.CategoryOrder("Bar Highlight", 22)]
	
	[Gui.CategoryOrder("Bar Dots", 23)]
	
	
	
	[Gui.CategoryOrder("Arrows", 30)]
	
	
	
	 
	
	
	
	
	
    [Gui.CategoryOrder("Chart Buttons", 144)]
	
	
	
	
	
	[Gui.CategoryOrder("Visual", 156)]
	[Gui.CategoryOrder("Data Series", 165)]
	
	
		
	[Gui.CategoryOrder("Audio", 200)]
	
	
	[Gui.CategoryOrder("Setup", 9000)]
	
	
	[Gui.CategoryOrder("Plots", 10000)]
	
	
	
	
	
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.SignalsCraigPritchertConverter")]
	public class SignalsCraigPritchert : Indicator
	{
		

		private string ThisName = "SignalsCraigPritchert";
		
		
		
		
		
		SortedDictionary<int, double> HighP = new SortedDictionary<int, double>();
		SortedDictionary<int, double> LowP = new SortedDictionary<int, double>();
		
		SortedDictionary<int, double> HighLA = new SortedDictionary<int, double>();
		SortedDictionary<int, double> LowLA = new SortedDictionary<int, double>();
		
		SortedDictionary<int, double> DeleteLA = new SortedDictionary<int, double>();
		
		SortedDictionary<int, int> HighLF = new SortedDictionary<int, int>();
		SortedDictionary<int, int> LowLF = new SortedDictionary<int, int>();			
		
		Stroke ThisStroke = new Stroke(Brushes.DarkGreen, DashStyleHelper.Solid, 2);
		Stroke ThisStrokeH = new Stroke(Brushes.DarkGreen, DashStyleHelper.Solid, 2);
		
		
		private double DefaultV = 999;
		
			
	
		
		private bool IsCurrentBar = false;
		

		
//		private double PreviousHigh, PreviousLow = 0;
//		private bool HigherHighs = false;
//		private	bool HigherLows = false;
		
		//private ATR iATR;
		
		
		//private PeriodType AcceptableBasePeriodType = PeriodType.Tick;
		
		private int DS = 0;
		
		private DateTime Launched = new DateTime(2010, 1, 18);
		private TimeSpan LaunchTime = new TimeSpan(1, 2, 0, 30, 0);

		private bool ChangingThisBar = false;
	
		private int ChangeThisBar = 0;
				
//		private NonLagMA iNonLagMAFast;
//		private NonLagMA iNonLagMASlow;
		
		private EMA iEMAFast;
		private EMA iEMASlow;
		
		private HMA iHMAFast;
		private HMA iHMASlow;
		
		private T3 iT3Fast;
		private T3 iT3Slow;
		
//		private MACDBBLINES iMACDBBLINES;
		
		
//		private Series<Brush> BarBrushes1;
//		private Series<Brush> CandleOutlineBrushes1;
//		private Series<Brush> BarBrushes2;
//		private Series<Brush> CandleOutlineBrushes2;
//		private Series<Brush> BarBrushes3;
//		private Series<Brush> CandleOutlineBrushes3;	
		
		
		private Series<int> BarBrushes1i;
		private Series<int> CandleOutlineBrushes1i;
		private Series<int> BarBrushes2i;
		private Series<int> CandleOutlineBrushes2i;
		private Series<int> BarBrushes3i;
		private Series<int> CandleOutlineBrushes3i;		
		
		
		
		
		private Series<string> SignalNameLong;
		private Series<string> SignalNameShort;
	
		private Series<double> swingHighSeries;
		private Series<double> swingHighSwings;
		private Series<double> swingLowSeries;
		private Series<double> swingLowSwings;
		
		
		
        private Series<double> AllPivots;
		
		private Series<double> BodyHigh;
		private Series<double> BodyLow;
		
		private Series<double> BodyHighOK;
		private Series<double> BodyLowOK;
		
		
		private Series<double> FinalHigh;
		private Series<double> FinalLow;		
		private Series<double> FinalVolume;
		private Series<double> ThisVolume;
		
		private Series<double> BOStatus;
		
		private Series<double> TrendSTO;
		private Series<double> LastTrendChange;
	
		private Series<double> CurrentHighPrice2;
		private Series<int> CurrentHighBar2;
		private Series<double> CurrentLowPrice2;
		private Series<int> CurrentLowBar2;
		
		private Series<double> SupportLine;
		private Series<double> ResistanceLine;
		
		
		private Series<double> SignalLow;
		private Series<double> SignalHigh;		
		
		private double UpperLevel = 0;
		private double LowerLevel = 0;
		
				
		
		
		
		
		private Series<double>		HighPoint;
		private Series<double>		LowPoint;
		private Series<double>		TotalRange;		
		
		
		private Series<double>		AllBarSignalsCount;
		private Series<double>		AllBarSignals;
		private Series<double>		FilteredBarSignals;
		private Series<double>		FilteredBarSignalsCount;
	
		private Series<double>		FilteredBarSignals2;
		
		private int TotalIn = 0;
		private int Multiplier = 1;
		
		private int LastAudioBar3 = 0;
		//private MAX max;
		//private MIN min;
		
		private int BB = 0;
		

		private Point MP;

		private double dpiX = 0;
		private	double dpiY = 0;
		
		
		private double FinalXPixel = 0;
		private	double FinalYPixel = 0;		
			
		
			
		
        private Series<double> Line1;
        private Series<double> Line2;
		

        private Series<double> WickHigh;
        private Series<double> WickLow;
		
		private Series<double> MATrend;
		
		private Series<int> LastSwitchBar1;
		
        private Series<int> Direction;		
		
		private Series<double> AVTRMA;
        private Series<double> AVTR;
        private Series<double> SDDD;
		
        private Series<double> HHV1;
        private Series<double> HHV2;	
		private Series<double> HHV3;
		private Series<double> HHV4;
				
        private Series<double> LLV1;
        private Series<double> LLV2;	
		private Series<double> LLV3;
		private Series<double> LLV4;
		
		private double FinalStdDev1 = 0;
		private double FinalStdDev2 = 0;
		private double FinalStdDev3 = 0;
		
		private System.Windows.Media.Brush pShade1	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(105,105,105));
		private System.Windows.Media.Brush pShade2	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(145,145,145));
		private System.Windows.Media.Brush pShade3	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(105,105,105));
		private System.Windows.Media.Brush pShade4	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(65,65,65));
		
		private int FinalBarOpacity = 0;
		private Color TempColor;		
		
		
		private Brush UpNeutralBrush;
		private Brush DownNeutralBrush;
		private Brush UpBuyBrush;
		private Brush DownBuyBrush;
		private Brush UpSellBrush;
		private Brush DownSellBrush;
		
		private Brush Up4Brush;
		private Brush Down4Brush;		
		
		private Brush Up5Brush;
		private Brush Down5Brush;			
		
		private Brush Up6Brush;
		private Brush Down6Brush;			
		
		private Brush FinalBarBrush;
		private Brush TempBrush;
		
	
		
		SharpDX.Direct2D1.AntialiasMode oldAntialiasMode;
		
        private SharpDX.Direct2D1.Brush ChartTextBrushDX = null;
		private SharpDX.Direct2D1.Brush ChartBackgroundBrushDX = null;
		private SharpDX.Direct2D1.Brush ChartBackgroundErrorBrushDX = null;
		private SharpDX.Direct2D1.Brush ThisBrushDX = null;
	
		
		

		
		private string LicensingMessage = string.Empty;		

		private SortedDictionary<int, int> ProductIDToMachineIDs = new SortedDictionary<int, int>();
		private ConcurrentDictionary<int, List<string>> ProductIDToInstruments = new ConcurrentDictionary<int, List<string>>();
	
		
		private bool Permission = false;
		

		
		private void AddError(string eee)
		{
		
			if (pErrorMessagesEnabled)
			if (!AllErrorMessages.Contains(eee))
				AllErrorMessages.Add(eee);
			
		}
		private bool pErrorMessagesEnabled = true;		
			private List<string> AllErrorMessages = new List<string>();		
		

	
		
		// for strategy to filter signals that aren't valid
		[XmlIgnore]
		public bool DoFilterSignals = false;
		
		[XmlIgnore] public double CurrentStopPrice33 = 0;
		[XmlIgnore] public double CurrentLastPrice33 = 0;
		[XmlIgnore] public double CurrentT1Price33 = 0;
		[XmlIgnore] public double CurrentT2Price33 = 0;
		[XmlIgnore] public double CurrentT3Price33 = 0;
		
		
		
		[XmlIgnore]
		public string SendAudioAlertString = string.Empty;	
		
		

		private bool IsMarketAnalyzer = false; 
		private double preval = 0;
		private double val = 0;
		private double val2 = 0;
		private Brush		smallAreaBrush	= Brushes.Red;
		
		private int HoursFromEST = 0;
		
		private bool AllowInitialExitOrderMove = false;
		
		private double prey = 0;
		private bool FirstOne = false;		
		
		private ChartControlProperties ThisChartProperties;
		private Brush StartBackBrush;
		private Brush NewBackBrush;
		private Brush NewStatusBrush;
		

		private bool StartPriceMarkers = false;
		
		private string triup = "\u25B2";
		private string tridn = "\u25BC";
		private string checkoff = "\u2713";
		
		
	
		private SharpDX.Direct2D1.Ellipse ThisEllipse;
		
		
		[XmlIgnore]
		public double CurrentTrendStatus = 0;
		private double CurrentTrendStatusButton = 0;
		
		
		private List<int> AllTrends = new List<int>();
		private List<int> AllTrends2 = new List<int>();
		
		private List<int> IsFirstTickOfBarDone = new List<int>();
		
		
				private string LogMessageS = " | ";
		private string LogMessage = "";
		


		
	
   
		private int LastCurrentBar = 0;
		private int BarToCancelLimitOrder = 0;

		private double StopLimitOffset, NewStopPrice, NewLimitPrice, CurrentStopPrice = 0;
		private bool OrderInstrumentOK, OrderStateOK, OrderTypeOK, OrderNameOK = false;
		
		private string PreviousAccountName = string.Empty;
	
		private double LastPrice, LastAsk, LastBid = 0;

        private bool SelectPlot = false;

		private int buttonh = 27;
		
		private Brush ButtonBrush = Brushes.DarkGreen;
		private Brush ButtonBrush2 = Brushes.Black;
		
		private Brush ButtonSBrush = Brushes.DarkRed;
		private Brush ButtonSBrush2 = Brushes.Black;
		
		private Brush Button3Brush = Brushes.DimGray;
		private Brush Button3Brush2 = Brushes.Black;
		
		private double SLMPrice = 0;

		private Position ThisPosition;
		                           
	

		private bool DoLong = false;
		private bool DoShort = false;
		private bool DoClose = false;
		
		private string CurrentType = string.Empty;
			
		
		private bool SetBuyClick = false;
		private bool SetSellClick = false;
		
		SortedDictionary<int, double> AllSignalsNow = new SortedDictionary<int, double>();
		SortedDictionary<int, double> OverallTrendNow = new SortedDictionary<int, double>();
		
		private SharpDX.Direct2D1.Brush OrderLineBrushDX;
		private SharpDX.Direct2D1.Brush OrderBoxBrushDX;
		
        private SharpDX.Direct2D1.Brush Plot0BrushDX;
        private SharpDX.Direct2D1.Brush Plot1BrushDX;
        private Brush Plot0Brush;
        private Brush Plot1Brush;


        private SharpDX.RectangleF B2 = new SharpDX.RectangleF(0, 0, 0, 0);

        private bool InMenu;
        private bool InMenuP;

        private string ChartType = string.Empty;
        private int TickDirection = 1;

        private bool ButtonOff = false;

		private double CurrentMousePrice = 0;
		
		private double LastClose = 0;
		private double EntryOrderPrice = 0;
		
        private int space = 5;

        SortedDictionary<double, ButtonZ> AllButtonZ = new SortedDictionary<double, ButtonZ>();
		
		SortedDictionary<double, ButtonZ> AllButtonZ2 = new SortedDictionary<double, ButtonZ>();
		
		
        const float fontHeight = 15f;

        private int PriceDigits = 0;
        private string PriceString = string.Empty;

        private List<double> All50Levels = new List<double>();
        private List<double> All100Levels = new List<double>();

        private class ButtonZ
        {
            string iText;
            string iName;
            int iWidth;
            bool iSwitch;
            SharpDX.RectangleF iRect;
            bool iHovered;

            public string Text { get { return iText; } set { iText = value; } }
            public string Name { get { return iName; } set { iName = value; } }
            public int Width { get { return iWidth; } set { iWidth = value; } }
            public bool Switch { get { return iSwitch; } set { iSwitch = value; } }
            public SharpDX.RectangleF Rect { get { return iRect; } set { iRect = value; } }
            public bool Hovered { get { return iHovered; } set { iHovered = value; } }

        }

		private Order LongEntryOrder = null;
		private Order ShortEntryOrder = null;	
		
		private bool SaveNextOrder = false;
		private bool SaveLongOrder = false;
		private bool SaveShortOrder = false;
		
	
		[XmlIgnore] public  double CurrentSignal = 0;
		
		
		private double LastSignal = 0;
		
		[XmlIgnore] public double CurrentSignalNow = 0;
		
		private int LaunchNumber = 0;

		
		private DateTime InitialTime = DateTime.MinValue;
		
        //relplacewith auto location
        private string OIF_file_name = NinjaTrader.Core.Globals.UserDataDir + @"incoming\OIF";
		private string BuyUniqueOrderId, SellUniqueOrderId = string.Empty;
			private string UniqueStrategyId = string.Empty;
			private string UniqueOrderId = string.Empty;
			private int BarAtLaunch = -1;
			//private NinjaTrader.Cbi.LogEventCollection lec = NinjaTrader.Cbi.Globals.LogEvents;
			private string NL = Environment.NewLine;
			private DateTime ErrorMsgPrintedAt = DateTime.MinValue;
			private string ErrorMsg = string.Empty;
			private string instruction = string.Empty;
		
		[XmlIgnore] public string pAccountName = string.Empty;
		
		private string pATMName = string.Empty;	
		private string pTIF = "DAY";
		[XmlIgnore] public double pQty = 0;
		
		private string SpreadName = string.Empty;
		private string CHLI = string.Empty;
        private int news = 22;
		
		private string OCOID = string.Empty;
		
		private Series<double> FirstSignals;
		private Series<double> Signals;
		private Series<double> AllSignals;
		private Series<double> ExSignals;

		private Series<double> BuySignals;
		private Series<double> SellSignals;
		
		
		private Series<double> Signals01;
		private Series<double> Signals02;
		private Series<double> Signals03;
		private Series<double> Signals04;
		private Series<double> Signals05;
		private Series<double> Signals06;
		private Series<double> Signals07;
		private Series<double> Signals08;
		private Series<double> Signals09;
		private Series<double> Signals10;
		
		private int ThisDirection1, ThisDirection2, ThisDirection3, ThisDirection4 = 0;
		
	
	
		
	 private Series<int> Direction1;


        private SharpDX.Vector2 StartPoint = (new Point(0, 0)).ToVector2();
        private SharpDX.Vector2 EndPoint = (new Point(0, 0)).ToVector2();

        private DateTime FirstBarTime = DateTime.MinValue;

        private ChartTrader chartTrader;

		[XmlIgnore] public Account myAccount;
		

		private string subject = string.Empty;
		private string message = string.Empty;


		private int LastEmailBar = 0;
		private int LastAudioBar = 0;
		private int LastAudioBar2 = 0;


		 private Series<int> Direction2;
		
		 private Series<int> Direction3;
		 private Series<int> Direction4;		
		
		
		
		private Series<double> OverallTrend;	
		
		private Series<double> LastSignalBar; 		
      
		private Series<double> LongLine;
		private Series<double> ShortLine;			
		
		private Series<double> TrendLine;
		private Series<double> TrendStatus;
		private Series<double> TrendSlope;
		
		private Series<double> TrendLongOK;
		private Series<double> TrendShortOK;
		
		private Series<double> SignalReady;
		private Series<double> CloseReady;
		
		
		private DateTime SendExitOrderCommands = DateTime.MaxValue;

		
		private string ChartName = string.Empty;
		
		private bool FirstRender = true;
			
		      
	
		private double PreviousIsInSession = 0;
			
        private Series<double> IsInSession;			
		[XmlIgnore] public string TimeStatusString = string.Empty;
		
		
		private DateTime StartTime, EndTime;
		private DateTime StartTime2, EndTime2;
		private DateTime StartTime3, EndTime3;				

		private DateTime NextStart, NextEnd = DateTime.MaxValue;	

		
		private bool FirstRun = true;
		
		
//		private EMA iEMAFast;
//		private EMA iEMASlow;
		
			
		private EMA iEMAFastFeed1;
		private EMA iEMASlowFeed1;	
		
		private EMA iEMAFastFeed2;
		private EMA iEMASlowFeed2;	
		
		private EMA iEMAFastFeed3;
		private EMA iEMASlowFeed3;	
		
		private EMA iEMAFastFeed4;
		private EMA iEMASlowFeed4;			
		
		
		private OrderFlowCumulativeDelta cumulativeDelta;
		
		private SharpDX.Direct2D1.Brush HUDVOLColorDX = null;
		private SharpDX.Direct2D1.Brush HUDNEColorDX = null;
		private SharpDX.Direct2D1.Brush HUDUPColorDX = null;
		private SharpDX.Direct2D1.Brush HUDDNColorDX = null;
		
		private SharpDX.DirectWrite.TextLayout textLayout2 = null;
		
		private int HUDNumber = 0;
		
	

		
		
		private SharpDX.RectangleF ThisRect = new SharpDX.RectangleF(0, 0, 0, 0);
		
		private SharpDX.Direct2D1.Brush ChartBackgroundFadeBrushDX = null;
		
		private int FB = 0;
		private int PrintFB = 0;
		private int LB = 0;
		private int xt = 0;
		private int yt = 0;
		private int thistop2 = 0;
		
		private double y1 = 0;
		private double y2 = 0;
		private double y3 = 0;
		private double y4 = 0;
		private double y5 = 0;
		private double y6 = 0;	
			
		private double xL = 0;
		//private	double xL2 = 0;
		private	double xR = 0;
		private	double xW = 0;
		private	double xW2 = 0;
		
		private double HUDHeight = 0;
		private double MinRightMarginHUD = 0;
		
		private int barWidth = 0;
		private int barDistance = 0;
		
		private double x1 = 0;
		private double x2 = 0;
		private double x3 = 0;
		private double x4 = 0;	
		
		private ConcurrentDictionary<double, PriceBox> PriceX1Boxes = new ConcurrentDictionary<double, PriceBox>();
		private ConcurrentDictionary<double, PriceBox> PriceX2Boxes = new ConcurrentDictionary<double, PriceBox>();

		private class PriceBox
		{

			double top;
			double bottom;
			double height;
			
						
			public double Top { get{return top;} set{top = value; }}
			public double Bottom { get{return bottom;} set{bottom = value; }}
			public double Height { get{return height;} set{height = value; }}

		}

	
		private List<List<string>> AllStrings = new List<List<string>>();
		private List<List<int>> AllColors = new List<List<int>>();		
		
			private List<List<string>> AllColumns = new List<List<string>>();

		
		private Swing iSwing;
		
		
		// TIMER
		
		private string			timeLeft	= string.Empty;
		private DateTime		now		 	= Core.Globals.Now;
		private bool			connected,
								hasRealtimeData;
		private SessionIterator sessionIterator;

		private System.Windows.Threading.DispatcherTimer timer;

		private string BarTimerString = string.Empty;
		
		private bool	isRangeDerivate;
		
		private long volume;
		private bool isVolume, isVolumeBase;		
				
		
		private System.Windows.Threading.DispatcherTimer timer3;
		
		
		private string CellString = string.Empty;
        private SharpDX.DirectWrite.TextFormat CellFormat;
				private SharpDX.DirectWrite.TextLayout CellLayout;
		
		private int Feed1, Feed2, Feed3, Feed4 = 0;
		
		[XmlIgnore] public string SFeed1, SFeed2, SFeed3, SFeed4 = string.Empty;
		
		
	
		// exit orders
		
//		LongT1 = LongPTT1[0];
//				LongT2 = LongPTT2[0];
//				LongStopM = LongStop[0];
//				LongStopTr = LongTrailStop[0];
				
//				ShortT1 = ShortPTT1[0];
//				ShortT2 = ShortPTT2[0];
//				ShortStopM = ShortStop[0];
//				ShortStopTr = ShortTrailStop[0];
				
//				tLongT1 = TicksT1[0];
//				tLongT2 = TicksT2[0];
//				tLongStopM = TicksSL[0];
//				//LongStopTr = LongTrailStop[0];
				
//				tShortT1 = TicksT1[0];
//				tShortT2 = TicksT2[0];
//				tShortStopM = TicksSL[0];
//				//ShortStopTr = ShortTrailStop[0];
								
		
		
		
		private Series<double> BarHigh;
		private Series<double> BarLow;
	

		
		
		
		private double ThisTickSize = 0;
		
		private TextLayout textLayout;
	
		private Series<Brush> EMAColors;
		private Series<Brush> HMAColors;
		
		
		private Series<int> EMATrend1;
		private Series<int> HMATrend1;
		private Series<int> T3Trend1;
		
		private Series<int> EMATrend2;
		private Series<int> HMATrend2;	
		private Series<int> T3Trend2;	
		
		
	//	Zombie3SMI myZombie3SMI;
		
		
		protected override void OnStateChange()
		{
			
			//Print(State);
			
				
			if (State == State.SetDefaults)
			{
				

				Name = ThisName;
				Description = "";
				
				
			
				
				
					
				IsMarketAnalyzer = ChartControl == null;
	
				
                //Name						= "MouseMove";

				Calculate					= Calculate.OnPriceChange;
				
				TextFont						= new SimpleFont("Consolas",13);
				
				
				IsAutoScale					= false;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				ShowTransparentPlotsInDataBox = true;   
				
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				
				IsMarketAnalyzer = false;
				
				
			//	ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= false;
				
				
                ArePlotsConfigurable = true;
                AreLinesConfigurable = true;

	
				IsSuspendedWhileInactive	= false;
				
				Calculate				= Calculate.OnPriceChange;
				DisplayInDataBox		= true;
				DrawOnPricePanel		= false;
				IsAutoScale				= false;
				IsOverlay				= true;
				PaintPriceMarkers		= true;
				ScaleJustification		= ScaleJustification.Right;
				
			
				ShowTransparentPlotsInDataBox = true;				

//				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 1),	PlotStyle.Line, "Plot 1");
	
				
				
				
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1),	PlotStyle.Dot, "Signals");
				
				
//				AddPlot(new Stroke(Brushes.DimGray, DashStyleHelper.Solid, 1),	PlotStyle.Line, "Fast MA");
//				AddPlot(new Stroke(Brushes.DimGray, DashStyleHelper.Solid, 1), PlotStyle.Line, "Slow MA"); //3
// 		        AddPlot(new Stroke(Brushes.DimGray), PlotStyle.Line, "MA High");
				
				
				

            }				
           else  if (State == State.Configure)
		    {
				
				
				ArePlotsConfigurable = true;
				
//					            Plots[0].Width = 1;
//				Plots[1].Width = 2;
//				Plots[2].Width = 1;
//				Plots[0].DashStyleHelper = DashStyleHelper.Dot;
//				Plots[1].DashStyleHelper = DashStyleHelper.Dot;
//				Plots[2].DashStyleHelper = DashStyleHelper.Dot;
				
				
			
			
		    }
			else if (State == State.Historical)
			{
				ZOrder = -1;	
				
			}
			else if (State == State.DataLoaded)
			{

				if (Name != ThisName && Name != string.Empty)
					Name = ThisName;	
				
				
				//IsAutoScale					= false;
          
//				if (pSignalMode == "System")
//					pSignalMode = "System 1";
				
				BarHigh 		= new Series<double>(this);
				BarLow 			= new Series<double>(this);
	
				
				
//				iNonLagMAFast = NonLagMA(0,pNonLagPeriod1,0,0,false,0,Brushes.Black,Brushes.Black);
//				iNonLagMASlow = NonLagMA(0,pNonLagPeriod2,0,0,false,0,Brushes.Black,Brushes.Black);
					
//				iEMAFast = EMA(pEMAPeriod1);
//				iEMASlow = EMA(pEMAPeriod2);
				
//				iHMAFast = HMA(pHMAPeriod1);
//				iHMASlow = HMA(pHMAPeriod2);
				
				
				
//				iT3Fast = T3(pT3Period1, pT3TCount1, pT3VFactor1);
//				iT3Slow = T3(pT3Period2, pT3TCount2, pT3VFactor2);
	
				
//				iMACDBBLINES = MACDBBLINES(pMACDFast,pMACDSlow,pMACDSmooth,Brushes.Black,Brushes.Black,false,pMACDDevFactor,pMACDDevPeriod,1,Brushes.Black,false,Brushes.Black,Brushes.Black);
		
				//myZombie3SMI = Zombie3SMI("",pEMAPeriod1,pEMAPeriod2,pZRange,pSMIEMAPeriod);
				
		
	
	
								
				ThisTickSize = Instrument.MasterInstrument.TickSize;
			
					
					
	
				
				
				
				Permission = true;
			
				
		
				if (ChartControl != null)
				{
					
					ChartName = Instrument.MasterInstrument.Name+"-"+Bars.BarsPeriod.BarsPeriodTypeName.ToString().Replace(" ","");
					
				
		
				}
				
				
				EMAColors = new Series<Brush>(this, MaximumBarsLookBack.Infinite);
				HMAColors = new Series<Brush>(this, MaximumBarsLookBack.Infinite);
				
					Direction1 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					Direction2 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					Direction3 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					Direction4 = new Series<int>(this, MaximumBarsLookBack.Infinite);
				
					EMATrend1 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					HMATrend1 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					T3Trend1 = new Series<int>(this, MaximumBarsLookBack.Infinite);
				
					EMATrend2 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					HMATrend2 = new Series<int>(this, MaximumBarsLookBack.Infinite);
					T3Trend2 = new Series<int>(this, MaximumBarsLookBack.Infinite);
				
	
				
				OverallTrend = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				LastSignalBar = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				AllSignals = new Series<double>(this, MaximumBarsLookBack.Infinite);
		      	FirstSignals = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Signals = new Series<double>(this, MaximumBarsLookBack.Infinite);
                Direction = new Series<int>(this, MaximumBarsLookBack.Infinite);
				ExSignals = new Series<double>(this, MaximumBarsLookBack.Infinite);

                 BuySignals = new Series<double>(this, MaximumBarsLookBack.Infinite);
				SellSignals = new Series<double>(this, MaximumBarsLookBack.Infinite);    
				
				
                Signals01 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Signals02 = new Series<double>(this, MaximumBarsLookBack.Infinite);  				
                Signals03 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Signals04 = new Series<double>(this, MaximumBarsLookBack.Infinite);  				
                Signals05 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Signals06 = new Series<double>(this, MaximumBarsLookBack.Infinite);  	
                Signals07 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Signals08 = new Series<double>(this, MaximumBarsLookBack.Infinite);  
                Signals09 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Signals10 = new Series<double>(this, MaximumBarsLookBack.Infinite);  				
				
				
                LongLine = new Series<double>(this, MaximumBarsLookBack.Infinite);
				ShortLine = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				
                TrendLine = new Series<double>(this, MaximumBarsLookBack.Infinite);
				TrendStatus = new Series<double>(this, MaximumBarsLookBack.Infinite);
				TrendSlope = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				TrendLongOK = new Series<double>(this, MaximumBarsLookBack.Infinite);
				TrendShortOK = new Series<double>(this, MaximumBarsLookBack.Infinite);				
				
				SignalReady = new Series<double>(this, MaximumBarsLookBack.Infinite);
				CloseReady = new Series<double>(this, MaximumBarsLookBack.Infinite);				
	
				IsInSession = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				
			

			
				
				if (ChartControl != null)
				if (ChartPanel != null)
                {
					ChartPanel.MouseMove += new MouseEventHandler(OnMouseMove);
                    ChartPanel.MouseDown += new MouseButtonEventHandler(OnMouseDown);
					ChartPanel.MouseLeave += new MouseEventHandler(OnMouseLeave);
                }

				
				if (ChartControl != null)
				{
					
	                ChartType = Bars.BarsPeriod.BarsPeriodTypeName;
					
					
						if (pTradesButtonsEnabled)
						{
							AddButtonZ("TRADES", "ButtonOff", 40, ChartBars.Properties.PlotExecutions != ChartExecutionStyle.DoNotPlot);
					
						
							AddButtonZ("Blank", "", 20, false);
						}
					
						//if (pActiveButtonsEnabled)
							//AddButtonZ("Targets", "Targets", 40, pAllTargetsEnabled);	
						
						
						//AddButtonZ("Arrows", "Arrows", 40, pArrowsEnabled);
				
					
						
					//AddButtonZ(pSignalMode, pSignalMode, 20, true);	
						
						
					AddButtonZ("Blank", "", 20, false);	
						


				}


                string FS = TickSize.ToString();
                if (FS.Contains("E-"))
                {
                    FS = FS.Substring(FS.IndexOf("E-") + 2);
                    PriceDigits = int.Parse(FS);
                }
                else PriceDigits = Math.Max(0, FS.Length - 2);
                PriceString = "n" + PriceDigits;
				
				
				
				
				// TIME ZONE
				
				if (pUseTimeFilter && pUseEST)
				{
					
				
					TimeZoneInfo LocalTZ = TimeZoneInfo.Local;
					double HoursLocalMinusUTC = LocalTZ.GetUtcOffset(DateTime.Now.ToUniversalTime()).TotalHours;
					
					double HoursNYMinusUTC = -5;
						
					try
					{
					
						TimeZoneInfo EasternTZ = TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time");
						HoursNYMinusUTC = EasternTZ.GetUtcOffset(DateTime.Now.ToUniversalTime()).TotalHours;
					}
					catch
					{
						Log(Name + " - Did not successfully find US Eastern Standard Time in the list of time zones.",LogLevel.Error);
						
					}
					
					//Print(HoursNYMinusUTC);
						
						
		//			Print("CT.id: "+LocalTZ.Id+"  "+LocalTZ.GetUtcOffset(DateTime.Now.ToUniversalTime()).TotalHours);
		//			Print("ET.id: "+EasternTZ.Id+"  "+EasternTZ.GetUtcOffset(DateTime.Now.ToUniversalTime()).TotalHours);
						
						
		//			Print("UTC offset: "+TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
		//			Print("UTC time: "+TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).ToString());
					//StartTime  = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, pStartTime.Hours, pStartTime.Minutes, 0);
					HoursFromEST = (int) (HoursLocalMinusUTC - HoursNYMinusUTC);
					
					
					//Print(HoursFromEST);
				}
			
				
				
				
//		pShade1	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(105,105,105));
//		pShade2	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(145,145,145));
//		pShade3	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(105,105,105));
//		pShade4	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(65,65,65));
				
				
//				Plots[3].Brush = pShade1;
//				Plots[4].Brush = pShade2;
//				Plots[5].Brush = pShade3;
//				Plots[6].Brush = pShade4;
				

				
	
				
				Multiplier = 1;
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex)
				{
					Multiplier = 10;
				}
				
			
				
				HighPoint = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowPoint = new Series<double>(this, MaximumBarsLookBack.Infinite);
				TotalRange = new Series<double>(this, MaximumBarsLookBack.Infinite);		
		
				
			
				
				BarBrushes1i = new Series<int>(this, MaximumBarsLookBack.Infinite);
				CandleOutlineBrushes1i = new Series<int>(this, MaximumBarsLookBack.Infinite);
				BarBrushes2i = new Series<int>(this, MaximumBarsLookBack.Infinite);
				CandleOutlineBrushes2i = new Series<int>(this, MaximumBarsLookBack.Infinite);				
				BarBrushes3i = new Series<int>(this, MaximumBarsLookBack.Infinite);
				CandleOutlineBrushes3i = new Series<int>(this, MaximumBarsLookBack.Infinite);					
				
						
//				BarBrushes1 = new Series<Brush>(this, MaximumBarsLookBack.Infinite);
//				CandleOutlineBrushes1 = new Series<Brush>(this, MaximumBarsLookBack.Infinite);
//				BarBrushes2 = new Series<Brush>(this, MaximumBarsLookBack.Infinite);
//				CandleOutlineBrushes2 = new Series<Brush>(this, MaximumBarsLookBack.Infinite);				
//				BarBrushes3 = new Series<Brush>(this, MaximumBarsLookBack.Infinite);
//				CandleOutlineBrushes3 = new Series<Brush>(this, MaximumBarsLookBack.Infinite);									
				
				
				
				SignalNameLong = new Series<string>(this, MaximumBarsLookBack.Infinite);
				SignalNameShort = new Series<string>(this, MaximumBarsLookBack.Infinite);
				
					
				swingHighSeries = new Series<double>(this);
				swingHighSwings = new Series<double>(this);
				swingLowSeries	= new Series<double>(this);
				swingLowSwings	= new Series<double>(this);
								
								
				AllPivots = new Series<double>(this, MaximumBarsLookBack.Infinite);
         
				FinalHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
				FinalLow = new Series<double>(this, MaximumBarsLookBack.Infinite);
				FinalVolume = new Series<double>(this, MaximumBarsLookBack.Infinite);
				ThisVolume = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				BOStatus = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				TrendSTO = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LastTrendChange = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				CurrentHighPrice2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				CurrentHighBar2 = new Series<int>(this, MaximumBarsLookBack.Infinite);				
				CurrentLowPrice2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				CurrentLowBar2 = new Series<int>(this, MaximumBarsLookBack.Infinite);
				
				SupportLine = new Series<double>(this, MaximumBarsLookBack.Infinite);
				ResistanceLine = new Series<double>(this, MaximumBarsLookBack.Infinite);				
				
				
				
				SignalLow = new Series<double>(this, MaximumBarsLookBack.Infinite);		
				SignalHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
								
	
				
				
				
				if (!pBackSignalEnabled && !BackTrendEnabled)
				{
					pBackEnabled = false;
					
				}
					
				
				
				
//				if (pColorNeutralBrush != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrushBack01.Clone();
//					newBrush.Opacity	= pOpacity01 / 100d;
//					newBrush.Freeze();
//					pBrushBack01			= newBrush;
//				}	
				
		
				
				
//				UpNeutralBrush = new SolidColorBrush(pColorNeutralBrush) {Opacity = pBarOpacityUp/100f}; UpNeutralBrush.Freeze();
				
				
				
				
//				DownNeutralBrush = new SolidColorBrush(pColorNeutralBrush) {Opacity = pBarOpacityDown/100f}; UpNeutralBrush.Freeze();
				
				
//				UpBuyBrush = new SolidColorBrush(pColorUpBrush) {Opacity = pBarOpacitySignal/100f}; UpNeutralBrush.Freeze();
//				DownBuyBrush = new SolidColorBrush(pColorUpBrush) {Opacity = pBarOpacitySignal/100f}; UpNeutralBrush.Freeze();
//				UpSellBrush = new SolidColorBrush(pColorDownBrush) {Opacity = pBarOpacitySignal/100f}; UpNeutralBrush.Freeze();
//				DownSellBrush = new SolidColorBrush(pColorDownBrush) {Opacity = pBarOpacitySignal/100f}; UpNeutralBrush.Freeze();
				
				
				
					
	
		
//				Plots[3].Brush = pShade1;
//				Plots[4].Brush = pShade2;
//				Plots[5].Brush = pShade3;
//				Plots[6].Brush = pShade4;
				
		
				
			//	pBarColorHEnabled = false;
				
				
				Multiplier = 1;
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex)
				{
					Multiplier = 10;
				}
				
				AllBarSignalsCount = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				AllBarSignals = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				FilteredBarSignals = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				FilteredBarSignalsCount = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				
					FilteredBarSignals2 = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				
				
				
                Line1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                Line2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				
                Direction = new Series<int>(this, MaximumBarsLookBack.Infinite);
				
                BodyHighOK = new Series<double>(this, MaximumBarsLookBack.Infinite);
                BodyLowOK = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
                BodyHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
                BodyLow = new Series<double>(this, MaximumBarsLookBack.Infinite);
                WickHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
                WickLow = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				MATrend = new Series<double>(this);
				
				LastSwitchBar1 = new Series<int>(this);	
				
				
				AVTRMA = new Series<double>(this, MaximumBarsLookBack.Infinite);
                AVTR = new Series<double>(this, MaximumBarsLookBack.Infinite);
                SDDD = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
                HHV1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                HHV2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                HHV3 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                HHV4 = new Series<double>(this, MaximumBarsLookBack.Infinite);	
				
                LLV1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                LLV2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                LLV3 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                LLV4 = new Series<double>(this, MaximumBarsLookBack.Infinite);				
       			       							
				//max = MAX(High, pMinimumSizeTicks);
			//	min	= MIN(Low, pMinimumSizeTicks);
			
//				FinalStdDev1 = Math.Pow(pStdDev1/20, 2);
//				FinalStdDev2 = Math.Pow(pStdDev2/20, 2);
//				FinalStdDev3 = Math.Pow(pStdDev3/20, 2);
				
//				FinalStdDev1 = Math.Pow(pStdDev1/100f, 2);
//				FinalStdDev2 = Math.Pow(pStdDev2/100f, 2);
//				FinalStdDev3 = Math.Pow(pStdDev3/100f, 2);
				
				
//				Print(FinalStdDev1);
//				Print(FinalStdDev2);
//				Print(FinalStdDev3);				
				
				
				
				

			}
			else if ( State == State.Terminated )
			{
				
				
				
				if (ChartControl != null)
                if (ChartPanel != null)
                {
                    ChartPanel.MouseMove -= new MouseEventHandler(OnMouseMove);
                    ChartPanel.MouseDown -= new MouseButtonEventHandler(OnMouseDown);
					ChartPanel.MouseLeave -= new MouseEventHandler(OnMouseLeave);
                }
				
				
			
			
					
			}
				

			else if (State == State.Historical)
			{
			
				if (!Permission)
					return;
			
				SetZOrder(10000);
			
				
				
//				if (ChartControl != null)
//				{
//					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
//					{
//						InsertWPFControls();
//					}));
//				}
			}
				

		}

		
        private void TogglePlotExecutions()
        {

            if (ChartBars.Properties.PlotExecutions == ChartExecutionStyle.DoNotPlot)
            {

                ChartBars.Properties.PlotExecutions = ChartExecutionStyle.MarkersOnly;

            }
            else if (ChartBars.Properties.PlotExecutions == ChartExecutionStyle.MarkersOnly)
            {
                ChartBars.Properties.PlotExecutions = ChartExecutionStyle.TextAndMarker;


            }
            else
            {
                ChartBars.Properties.PlotExecutions = ChartExecutionStyle.DoNotPlot;


            }


        }

       
		
		private int ThisCurrentBar = 0;
		
		
		private void MatchSeries(Series<double> In)
        {
			
			Signals = In;
			Values[0] = In;
			
			
        }	
		
//		private void SetColor(Series<Brush> In, int plot)
//        {
			
	
//			PlotBrushes[plot] = In;
			
			
//        }	
	
		
		private void SetPlot(Series<double> In, int plot)
        {
			
	
			Values[plot] = In;
			
			
        }	
				
	
		

	
        internal void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MP = e.GetPosition(this.ChartPanel);

 
			FinalXPixel = MP.X / 100 * dpiX;
			FinalYPixel = MP.Y / 100 * dpiY;
         
           
		if (AllErrorMessages.Count > 0)
			{
				AllErrorMessages.Clear();
				ChartControl.InvalidateVisual();
				
				//myProperties.AllowSelectionDragging = PreviousDrag;
				
				return;
				
			}

			// top chart buttons
			
			
			
			
			
			bool OneDone = false;
            foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ2)
            {
                bool hoverednew = MouseIn(thisbutton.Value.Rect, 2, 2);
                string buttonn = thisbutton.Value.Text;


				

               if (hoverednew && buttonn == SFeed1)
                {
					
					//Print(buttonn);
					
					pFeed1Included = !pFeed1Included;
					OneDone = true;
					
					
					
				}
				
               if (hoverednew && buttonn == SFeed2)
                {
					
					//Print(buttonn);
					pFeed2Included = !pFeed2Included;
					OneDone = true;
					
					
				}				
				
               if (hoverednew && buttonn == SFeed3)
                {
					
					//Print(buttonn);
					pFeed3Included = !pFeed3Included;
					OneDone = true;
					
										
					
					
				}	
				
               if (hoverednew && buttonn == SFeed4)
                {
					
					//Print(buttonn);
					
					pFeed4Included = !pFeed4Included;
					OneDone = true;
					
						
					
				}
				
			}
			
			
			if (OneDone)
			{
					//CalculateTrendAndSignals();
					//UpdateButtons();
					
					//RefreshRender();
						
					return;						
			}
			
			
			
			
			if (pButtonsEnabled)
		
            foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
            {
                bool hoverednew = MouseIn(thisbutton.Value.Rect, 2, 2);
                string buttonn = thisbutton.Value.Text;



               if (hoverednew && buttonn == "TRADES")
                {
                    //pSLOffset = Math.Max(0, pSLOffset - 1);
                    
					TogglePlotExecutions();
					
					thisbutton.Value.Switch = ChartBars.Properties.PlotExecutions != ChartExecutionStyle.DoNotPlot;
					
				//	SetBack(0);
					
					this.ChartControl.InvalidateVisual();
					return;

                }               
  				else if (hoverednew && buttonn == "Arrows")
                {

                    if (pArrowsEnabled)
                    {
                        pArrowsEnabled = false;
						
					
						
                    }
                    else
                    {
                        pArrowsEnabled = true;
						
						
                    }
	
                    thisbutton.Value.Switch = pArrowsEnabled;
                    this.ChartControl.InvalidateVisual();
					
					return;

                }
				
//				else if (hoverednew && buttonn == pSignalMode)
//                {
					
//					if (pSignalMode == "EMA")
//					{
//						pSignalMode = "T3";
//						MatchSeries(Signals04);
						
//						SetPlot(iT3Fast.Values[0], 1);
						
					
						
//						if (pSignalMode2 == "Cross")
//							SetPlot(iT3Slow.Values[0], 2);
									
//						if (pSignalMode2 == "Cross")
//						{
//							SetPlotColors(T3Trend2,1,false);
//							SetPlotColors(T3Trend2,2,false);
//						}
//						else
//						{
//							SetPlotColors(T3Trend1,1,false);
//						}
						
						
//					}
					
					
//					else if (pSignalMode == "T3")
//					{
//						pSignalMode = "EMA";
//						MatchSeries(Signals01);
						
//						SetPlot(iEMAFast.Values[0], 1);
						
//						if (pSignalMode2 == "Cross")
//							SetPlot(iEMASlow.Values[0], 2);
					
						
//						if (pSignalMode2 == "Cross")
//						{
//							SetPlotColors(EMATrend2,1,false);
//							SetPlotColors(EMATrend2,2,false);
//						}
//						else
//						{
//							SetPlotColors(EMATrend1,1,false);
//						}
						
//					}
					
										
					
					
					
//					else if (pSignalMode == "HMA")
//					{
//						pSignalMode = "EMA";
//						MatchSeries(Signals01);
						
//						SetPlot(iEMAFast.Values[0], 1);
						
//						if (pSignalMode2 == "Cross")
//							SetPlot(iEMASlow.Values[0], 2);
					
						
//						if (pSignalMode2 == "Cross")
//						{
//							SetPlotColors(EMATrend2,1,false);
//							SetPlotColors(EMATrend2,2,false);
//						}
//						else
//						{
//							SetPlotColors(EMATrend1,1,false);
//						}
						
//					}
					
					
					
					
					
					
					
//					if (pSignalMode == "System 1")
//					{
//						pSignalMode = "System 2";
//						MatchSeries(Signals05);
//					}
//					else if (pSignalMode == "System 2")
//					{
//						pSignalMode = "System 3";
//						MatchSeries(Signals09);
//					}
//					else if (pSignalMode == "System 3")
//					{
//						pSignalMode = "EMA";
//						MatchSeries(Signals01);
//					}
//					else if (pSignalMode == "EMA")
//					{
//						pSignalMode = "NonLagMA";
//						MatchSeries(Signals02);
//					}					
						
//					else if (pSignalMode == "NonLagMA")
//					{
//						pSignalMode = "MACD";
//						MatchSeries(Signals03);
//					}							
//					else if (pSignalMode == "MACD")
//					{
//						pSignalMode = "System 1";
//						MatchSeries(Signals04);
//					}		
					
					
					
			
//					thisbutton.Value.Switch = true;
					
//					thisbutton.Value.Name = pSignalMode;
//					thisbutton.Value.Text = pSignalMode;
					
//                    this.ChartControl.InvalidateVisual();
					
//					return;

					
//				}
				
				
		
		
            }
			
			
			
		


        }
		

		
		internal void OnMouseLeave(object sender, MouseEventArgs e)
    	{
            this.MP = e.GetPosition(this.ChartPanel);


			FinalXPixel = MP.X / 100 * dpiX;
			FinalYPixel = MP.Y / 100 * dpiY;
			
			InMenu = false;
			
			RefreshRender();
		}
		
		private void RefreshRender()
		{
			//Print("Refresh Render " + DateTime.Now.ToString());
			
			this.ChartControl.InvalidateVisual();
		}
		
		internal void OnMouseMove(object sender, MouseEventArgs e)
    	{
			
			//return;
			
			
			if (!pButtonsEnabled)
				return;
			
            this.MP = e.GetPosition(this.ChartPanel);


			FinalXPixel = MP.X / 100 * dpiX;
			FinalYPixel = MP.Y / 100 * dpiY;
         

		
      
           	 foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
                {
                    bool hoverednew = MouseIn(thisbutton.Value.Rect, 2, 2);
                    bool hoverednow = thisbutton.Value.Hovered;

                    if (hoverednew && !hoverednow)
                    {
                        thisbutton.Value.Hovered = true;
						
                        RefreshRender();
                    }
                    if (!hoverednew && hoverednow)
                    {
                        thisbutton.Value.Hovered = false;
						
                        RefreshRender();
                    }

                }

                InMenuP = InMenu;
                InMenu = MouseIn(B2, 8, 8);
           
            	if (InMenu != InMenuP)
               	 RefreshRender();
			
			
			
			
			//e.Handled = true;
		}



		private int BB2 = 0;

	
		private SimpleFont pTextFontTime22 = new SimpleFont("Consolas", 16);
		private System.Windows.TextAlignment pta = System.Windows.TextAlignment.Center;
		
		private string dotttt = "\u25CF";
		
		protected override void OnBarUpdate()
		{
			
					
			
			
			BarHigh[0] = High[0];
			BarLow[0] = Low[0];
			
			BodyHigh[0] = Math.Max(Close[0],Open[0]);
			BodyLow[0] = Math.Min(Close[0],Open[0]);
			
			//double pPercentQ = 10;
		
			BodyHighOK[0] = BarHigh[0] - (BarHigh[0] - BarLow[0])*pPercentQ/100;
			BodyLowOK[0] = BarLow[0] + (BarHigh[0] - BarLow[0])*pPercentQ/100;
			
			
			BarBrushes[0]= null;
			CandleOutlineBrushes[0] = null;
			
			Direction[0] = 0;
			
			if (Close[0] < Open[0])
			{
				Direction[0] = -1;
				
			}
			else if (Close[0] > Open[0])
			{
				
				Direction[0] = 1;
			}
			else
			{
				if (CurrentBar == 0)
				{
					Direction[0] = 1;
				}
				else
				{
					Direction[0] = Direction[1];
				}
				
			}
			
			
				// if (Close[0] <= BodyLowOK[0])
				
				Signals[0] = 0;
			
			if (CurrentBar == 0)
				return;
			
				 if (Direction[0] == 1 && Direction[1] != 1)
				{
					
					Signals[0] = 1;
						
					if (pBarColorEnabled)
					{
						BarBrushes[0] = pColorUpBrush;
						CandleOutlineBrushes[0] = pColorUpBrush2;							
					}

				}
				else if (Direction[0] == -1 && Direction[1] != -1)
				{
					
					Signals[0] = -1;
					
					if (pBarColorEnabled)
					{					
						BarBrushes[0] = pColorDownBrush;
						CandleOutlineBrushes[0] = pColorDownBrush2; 
					}
					
				}
				else
				{
			
//					BarBrushes[0] = pColorNeutralBrush;
//					CandleOutlineBrushes[0] = pColorNeutralBrush2;
					
					
//					if (CurrentBar == 0)
//					{
//						BarBrushes[0] = pColorUpBrush;
//						CandleOutlineBrushes[0] = pColorUpBrush2;
//					}
//					else
//					{
//						BarBrushes[0] = BarBrushes[1];
//						CandleOutlineBrushes[0] = CandleOutlineBrushes[1];
//					}
					
					
				}

			
				
		
				
					Values[0][0] = Signals[0];
				
			//BarBrushes1i[0] = -1;
			
	
			
			
			
			BB2 = 1;
			
			if (pQuickAudio || Calculate == Calculate.OnBarClose)
				BB2 = 0;
							

			if (pAudioEnabled)
			{
				//if (ZoneItems[BB] != null)
				if(Signals[BB2] == 1 && LastAudioBar3 != CurrentBars[0])
				{
					Alert(CurrentBar.ToString(),Priority.High, BarsPeriod + " | " + Name, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+pWAVFileName,1,pColorBuyBrush,GetTextColor(pColorBuyBrush));
					LastAudioBar3 = CurrentBars[0];
					
					//BackBrushes[0] = Brushes.Green;
					
				}
				
				if(Signals[BB2] == -1 && LastAudioBar3 != CurrentBars[0])
				{
					Alert(CurrentBar.ToString(),Priority.High, BarsPeriod + " | " + Name, NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+pWAVFileName2,1,pColorSellBrush,GetTextColor(pColorSellBrush));
					LastAudioBar3 = CurrentBars[0];
					
					//BackBrushes[0] = Brushes.Green;
					
				}							
				
				
				
			}	
						
						
						
			
			
			
			
			
			
        }



	
	

        private void AddButtonZ(string iText, string iName, int iWidth, bool iSwitch)
        {
            ButtonZ Z = new ButtonZ();
            Z.Text = iText;
            Z.Name = iName;
            Z.Width = iWidth;
            Z.Switch = iSwitch;
            Z.Rect = new SharpDX.RectangleF(0, 0, 0, 0);
            Z.Hovered = false;

            AllButtonZ.Add(AllButtonZ.Count + 1, Z);

        }
					
        private void AddButtonZ2(string iText, string iName, int iWidth, bool iSwitch, SharpDX.RectangleF iRect)
        {
            ButtonZ Z = new ButtonZ();
            Z.Text = iText;
            Z.Name = iName;
            Z.Width = iWidth;
            Z.Switch = iSwitch;
            Z.Rect = iRect;
            Z.Hovered = false;

            AllButtonZ2.Add(AllButtonZ2.Count + 1, Z);

        }	
		
        private bool MouseIn(SharpDX.RectangleF RR, int XF, int YF)
        {
            //Print(RR.Left);
            
			if (FinalXPixel != 0)
            if (FinalXPixel >= RR.Left - XF && FinalXPixel <= RR.Right + XF && FinalYPixel >= RR.Top - YF && FinalYPixel <= RR.Bottom + YF)
                return true;
           
                return false;

        }

		
        private double RTTS(double p)
        {
			if (Instrument == null)
				return p;
			else
            	return Instrument.MasterInstrument.RoundToTickSize(p);
        }

	
		
		

	
		public Brush GetTextColor(Brush bg2)
		{
			
			//Color bg = new Pen(bg2,1).;
			Color bg = (bg2 as SolidColorBrush).Color;
			
			
			double a = 1 - ( 0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B)/255;
            if (a < 0.5)
               return Brushes.Black;
            else
               return Brushes.WhiteSmoke;
			
//		    int nThreshold = 150;
//		    int bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + 
//		                                  (bg.B * 0.114));

//		    Brush foreColor = (255 - bgDelta < nThreshold) ? Brushes.Black : Brushes.White;
//		    return foreColor;
		}

		
		
		
	public override void OnRenderTargetChanged()
		{

			if (RenderTarget != null)
			{
				pArrowUpStroke.RenderTarget = RenderTarget;
				pArrowDownStroke.RenderTarget = RenderTarget;
				
				
				
				ThisStroke.RenderTarget = RenderTarget;		
				
						

			}
			
		}
		

		
		private bool FirstRender2 = true;
				
		private ChartControlProperties myProperties;
					
		private bool PreviousDrag = false;		
			
		SharpDX.DirectWrite.TextFormat CenterText;
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
		
		
		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{		
			
			//Print("bug1");
			
			
			//try
			{
				
	
				
				if (!IsInHitTest)
			if (pVisualEnabled)			
				base.OnRender(chartControl, chartScale);
			
	
			
			
			oldAntialiasMode	= RenderTarget.AntialiasMode;
      

			if (FirstRender2)
			{
			
				//ChartBarsSwitch2(true);

				
            	myProperties = chartControl.Properties;
				//PreviousDrag = myProperties.AllowSelectionDragging;
				
				
				
				
				
				chartTrader = Window.GetWindow(ChartControl.Parent).FindFirst("ChartWindowChartTraderControl") as ChartTrader;	
				
				FirstRender2 = false;
				
				
			}
		
				
				ChartTextBrushDX = myProperties.ChartText.ToDxBrush(RenderTarget);
				ChartBackgroundBrushDX = myProperties.ChartBackground.ToDxBrush(RenderTarget);				 			
				//ChartBackgroundErrorBrushDX = Brushes.Red.ToDxBrush(RenderTarget);
							

			
			if (!IsInHitTest)
 			if (AllErrorMessages.Count > 0)
				{
				
					
//					ChartBarsSwitch2(false);
//					myProperties.AllowSelectionDragging = false;
					
					
					
				ChartBackgroundErrorBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
				ChartBackgroundErrorBrushDX.Opacity = 25/100f;
				
	            CenterText = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory,
	                "Arial",
	                SharpDX.DirectWrite.FontWeight.Normal,
	                SharpDX.DirectWrite.FontStyle.Normal,
	                SharpDX.DirectWrite.FontStretch.Normal,
	                11.0F);
				
				CenterText = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
				CenterText = new SimpleFont(ChartControl.Properties.LabelFont.Family.ToString(), 16).ToDirectWriteTextFormat();
	            CenterText.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
	            CenterText.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
	            CenterText.WordWrapping = SharpDX.DirectWrite.WordWrapping.Wrap;
				
				//CellFormat = FinalFont1.ToDirectWriteTextFormat();
				
				CenterRect = new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H);
				
				
				
				
				
					RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
					
						string txt = string.Empty;
					
					foreach (string sss in AllErrorMessages)
					txt = txt + sss + "\r\n\r\n";
				
					txt = txt + "Click here to continue.";
					//Print(text);
					RenderTarget.FillRectangle(CenterRect, ChartBackgroundBrushDX);
					RenderTarget.FillRectangle(CenterRect, ChartBackgroundErrorBrushDX);
					RenderTarget.DrawText(txt, CenterText, ExpandRect(CenterRect,-10,0), ChartTextBrushDX);
					
					
					RenderTarget.AntialiasMode = oldAntialiasMode;
					
				
				
				ChartBackgroundErrorBrushDX.Dispose();		
				CenterText.Dispose();	
				
					//Print("bug2");
					
				return;
			}
			
			
			
			

						
			
        
				
//			if (Permission && pEXYES)
//			if (pCTButtonsEnabled)
//			{
				
				
//				 if (chartTrader != null && FirstRender)
//				 {
					 
					
//			          //  ChartTraderProperties myprop = chartTrader.FindFirst("ChartTraderProperties") as NinjaTrader.Gui.Chart.ChartTraderProperties;
//						ChartTraderProperties myprop = chartTrader.Properties;
//					 myprop.AtmStrategySelectionMode = AtmStrategySelectionMode.KeepSelectedAtmStrategyTemplateOnOrderSubmission;
					 
					 
//					 myprop.ShowRealizedPnLWhenFlat = pShowReal;
					 
//					 if (chartTrader.ChartTraderVisibility == ChartTraderVisibility.Visible)
//					 {
						 
						 
//						//
					 	
//					 // Print("hello");
						 
						 
//					 	if (ChartControl != null)
//						{
//							ChartControl.Dispatcher.InvokeAsync((Action)(() =>
//							{
//								InsertWPFControls();
//								RunPNL();
//							}));
//						}
					
					
//						 FirstRender = false;
//					 }
//				 }
				 
//			}
			
			
			
			

            SharpDX.Direct2D1.Brush selectBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Yellow);
			SharpDX.Direct2D1.Brush textBrushDX2 = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.White);
			SharpDX.Direct2D1.Brush textBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.RoyalBlue);

            SharpDX.Direct2D1.Brush upBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.LimeGreen);
			SharpDX.Direct2D1.Brush downBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
			SharpDX.Direct2D1.Brush finalBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
			
            SharpDX.Direct2D1.Brush lineBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.RoyalBlue);
			SharpDX.Direct2D1.Brush longBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Green);
			SharpDX.Direct2D1.Brush blackBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Black);
				

   

           // System.Windows.Media.Brush buttonBrush = ChartControl.Properties.ChartText;
			// System.Windows.Media.Brush buttonBrush = GetTextColor(ChartControl.Properties.ChartBackground);
            SharpDX.Direct2D1.Brush buttonBrushDX = GetTextColor(ChartControl.Properties.ChartBackground).ToDxBrush(RenderTarget);

           // System.Windows.Media.Brush buttonHBrush = ChartControl.Properties.AxisPen.Brush;

			
			//System.Windows.Media.Brush buttonHBrush = GetTextColor(ChartControl.Properties.ChartBackground);
            SharpDX.Direct2D1.Brush buttonHBrushDX = GetTextColor(ChartControl.Properties.ChartBackground).ToDxBrush(RenderTarget);
			
			

            SharpDX.Direct2D1.Brush buttonFHBrushDX = ChartControl.Properties.AxisPen.Brush.ToDxBrush(RenderTarget);
			
			
			
			
           // SharpDX.Direct2D1.Brush buttonFOFFBrushDX = ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget);

			
			SharpDX.Direct2D1.Brush buttonFOFFBrushDX = areaBrush2.ToDxBrush(RenderTarget);
         
            SharpDX.Direct2D1.Brush buttonFONBrushDX = areaBrush.ToDxBrush(RenderTarget);


			
//            Plot0BrushDX = Plots[0].BrushDX;
//            Plot1BrushDX = Plots[1].BrushDX;

            //SharpDX.Direct2D1.Brush Plot0Brush = Plots[0].BrushDX;
            //SharpDX.Direct2D1.Brush Plot1Brush = Plots[1].BrushDX;

            //SharpDX.Direct2D1.StrokeStyle 
            //buttonBrushDX = new SharpDX.Direct2D1.Brush(ChartControl.Properties.AxisPen.BrushDX);
            // buttonHBrushDX = ChartControl.Properties.AxisPen.BrushDX;

  

                buttonHBrushDX.Opacity = 0.4f;
                buttonFHBrushDX.Opacity = 0.0f;          


   


            //Print(CY);
           
            //SizeF szvv = graphics.MeasureString("B", ChartControl.Font);


            //bsize2 = (int)szvv.Height + 2;
           


		

            //SharpDX.RectangleF rectangleF = new SharpDX.RectangleF( (float) MP.X-5F, 0F, 10F, 10000F );
            //SharpDX.Direct2D1.Brush textBrushDX2 = textBrushDX.

            FB = ChartBars.FromIndex;
            LB = ChartBars.ToIndex;
            BB = 0;
			
			
            int xt = 0;
            int yt = 0;

            //if (Calculate.OnBarClose)
            LB = Math.Min(CurrentBars[0], LB);
            BB = CurrentBars[0] - LB;

      
            int yh = 0;
			int yl = 0;
			int he = 0;
			
			
			
            //if (Calculate.OnBarClose)
            LB = Math.Min(CurrentBars[0], LB);
            BB = CurrentBars[0] - LB;

			// no longer relevant
			if (pBarColorHEnabled)
			{
				
	            System.Windows.Media.Brush textBrush = pColorFlatBrush;
	            SharpDX.Direct2D1.Brush textBrushDXww = textBrush.ToDxBrush(RenderTarget);
				textBrushDXww.Opacity = pOpacityR/100F;
				
				
				for (int i = FB; i <= LB; i++)
				{
				
					int sig = (int) Signals.GetValueAt(i);
					
					int BB2 = i;
					
					double vvvv = 0;
				
//					if (pSignalMode == "EMA")
//					{
//						if (Signals01.IsValidDataPointAt(BB2))
//							vvvv = Signals01.GetValueAt(BB2);
//					}
//					else if (pSignalMode == "NonLagMA")
//					{
//						if (Signals02.IsValidDataPointAt(BB2))
//							vvvv = Signals02.GetValueAt(BB2);
//					}					
//					else if (pSignalMode == "MACD")
//					{
//						if (Signals03.IsValidDataPointAt(BB2))
//							vvvv = Signals03.GetValueAt(BB2);
//					}							
//					else if (pSignalMode == "System 1")
//					{
//						if (Signals04.IsValidDataPointAt(BB2))
//							vvvv = Signals04.GetValueAt(BB2);
//					}							
//					else if (pSignalMode == "System 2")
//					{
//						if (Signals05.IsValidDataPointAt(BB2))
//							vvvv = Signals05.GetValueAt(BB2);
//					}		
//					else if (pSignalMode == "System 3")
//					{
//						if (Signals05.IsValidDataPointAt(BB2))
//							vvvv = Signals09.GetValueAt(BB2);
//					}		
					
//					if (pSignalMode == "EMA")
//					{
//						if (Signals01.IsValidDataPointAt(BB2))
//							vvvv = Signals01.GetValueAt(BB2);
//					}
				
//					else if (pSignalMode == "HMA")
//					{
//						if (Signals05.IsValidDataPointAt(BB2))
//							vvvv = Signals05.GetValueAt(BB2);
//					}		
				
				
//					else if (pSignalMode == "T3")
//					{
//						if (Signals04.IsValidDataPointAt(BB2))
//							vvvv = Signals04.GetValueAt(BB2);
//					}		
									
					
//					sig = (int)vvvv;
					
					
					//Print(i + "  " + sig);
					
					if (sig == 1 || sig == -1)
					{
						
						
						if (sig == 1)
						{
							textBrush = pColorFlatBrush;
		           			textBrushDXww = textBrush.ToDxBrush(RenderTarget);
							textBrushDXww.Opacity = pOpacityR/100F;						
						}
						if (sig == -1)
						{
							textBrush = pColorFlat2Brush;
		           			textBrushDXww = textBrush.ToDxBrush(RenderTarget);
							textBrushDXww.Opacity = pOpacityR/100F;							
							
						}
						

						
						int BarH = i;
						
						xt = chartControl.GetXByBarIndex(ChartBars, BarH);
						yh = chartScale.GetYByValue(High.GetValueAt(BarH));
						yl = chartScale.GetYByValue(Low.GetValueAt(BarH));
						he = yl-yh;
						
						int bbsd = (int) chartControl.BarWidth;
						
						int width = pHighlightSize + pHighlightSize + 1 +bbsd+bbsd;
						int height = he + pHighlightSize + pHighlightSize;
						
						SharpDX.RectangleF BodyRect = new SharpDX.RectangleF(xt-bbsd-pHighlightSize-1, yh-pHighlightSize, width, height);
						
						//Print(width);
						
						RenderTarget.FillRectangle(BodyRect, textBrushDXww);
					}
					
					
				}
				
				textBrushDXww.Dispose();
				
			}
			
			
			
			
			
			
			
			
			
	
 			Point StartPoint4	= new Point(0, 0);
			Point EndPoint4		= new Point(0, 0);
			Point TextPoint4		= new Point(0, 0);
			float x1, x2, y1, y2 = 0;		
			
			
			
			
			//Print("bug3");			
			
			
			
			// resolve issue with other windows 10 font scaling automatically
			
//				if (ChartControl != null)
//				{
//					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
//					{
									
//						if (this.ChartPanel != null)
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
						
				
//					}));
//				}
				
				
			//Print(dpiX);
			
			FinalXPixel = MP.X / 100 * dpiX;
			FinalYPixel = MP.Y / 100 * dpiY;
			
				
						
						
//           CurrentMousePrice = chartScale.MaxValue - chartScale.MaxMinusMin * (MP.Y / chartScale.Height);

//			CurrentMousePrice = RTTS(CurrentMousePrice);

            

//            double mousebar = (ChartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex) - ChartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex)) / Math.Max(1,(ChartBars.ToIndex - ChartBars.FromIndex)); //chartControl.GetBarPaintWidth(ChartBars);

//            double mousebar2 = ChartBars.FromIndex + (MP.X - ChartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex)) / mousebar;

//            int mousebar3 = (int) Math.Round(mousebar2, 0);

//            SharpDX.DirectWrite.TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Normal,
//            SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 12.0F);

//			textFormat = myProperties.LabelFont.ToDirectWriteTextFormat();
						
						
		
						string text = string.Empty;
						
            //{ TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, WordWrapping = WordWrapping.NoWrap };

            //text = "Mouse: (" + this.MP.X.ToString() + " , " + this.MP.Y.ToString() + " )" + Environment.NewLine;

//            text = CurrentMousePrice.ToString(PriceString) + " " + mousebar3.ToString() + " " + CurrentBar.ToString();
//			text = "NinjaTrader  " + CurrentMousePrice.ToString(PriceString) + " " + mousebar3.ToString() + " SLO = " + pSLOffset.ToString();
			
			
 //           SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(10F, 20F, 500, 50F);

            
//			DateTime TTT = ChartBars.GetTimeByBarIdx(chartControl,ChartBars.ToIndex);
			
//			text = text + TTT.DayOfWeek.ToString();


						
						   
						
						
//						double yt22 = chartScale.GetYByValue(chartScale.MinValue) - 17;
//            SharpDX.RectangleF BottomRect = new SharpDX.RectangleF(160, (float)yt22, 2000, 21);

            

//            SharpDX.DirectWrite.TextFormat BottomText = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory,
//                "Arial",
//                SharpDX.DirectWrite.FontWeight.Normal,
//                SharpDX.DirectWrite.FontStyle.Normal,
//                SharpDX.DirectWrite.FontStretch.Normal,
//                11.0F);

//            BottomText.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
//            BottomText.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
//            BottomText.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

//            BottomText = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();

			
			

				
					
			
////			string sep = "   |   ";
////			string trailing = "Daily Loss Trailing is enabled.  We are trailing the maximum profit for today by $ " + pDLTrailingAmount.ToString("N2") + ".";
			
////				trailing = "Daily Loss Trailing by $ " + pDLTrailingAmount.ToString("N2") + " behind the maximum profit is enabled.";
			
////			if (!pDailyPNLTrailingEnabled)
////			{
////				trailing = "Daily Loss Trailing is disabled.  We will adhere to the fixed Daily Loss of $ " + pDL.ToString("N2") + ".";
////				trailing = "Daily Loss Trailing is disabled.";
////			}
				
////			trailing = string.Empty;
			
////			text = "Daily Goal =  $ " + pDG.ToString("N2") + sep + "Daily Loss =  $ " + pDL.ToString("N2") + sep + trailing;
			
//			oldAntialiasMode	= RenderTarget.AntialiasMode;		
//			RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;			
			
//			BottomText = pTextFont2.ToDirectWriteTextFormat();
//			textBrushDX2 = Brushes.White.ToDxBrush(RenderTarget);
			
			
//			bool pTextDisplayEnabled = true;
			
//			//if (pDGEnabled && pTextDisplayEnabled) RenderTarget.DrawText(text, BottomText, BottomRect, textBrushDX2);

//			RenderTarget.AntialiasMode = oldAntialiasMode;
			
          


//			BottomText.Dispose();

			
			
			
			
			double percentorderl = 50;
			double orderlinew = ChartPanel.W / 100 * percentorderl;
			
			double yyyyy = chartScale.GetYByValue(CurrentMousePrice);
			
			
			Point startPoint	= new Point(ChartPanel.W-orderlinew, MP.Y);
			Point endPoint		= new Point(ChartPanel.W, MP.Y);
			
			startPoint	= new Point(ChartPanel.W-orderlinew, yyyyy);
			endPoint		= new Point(ChartPanel.W, yyyyy);

            SharpDX.DirectWrite.TextFormat PrintText = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory,
                "Arial",
                SharpDX.DirectWrite.FontWeight.Normal,
                SharpDX.DirectWrite.FontStyle.Normal,
                SharpDX.DirectWrite.FontStretch.Normal,
                11.0F);

            PrintText.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
            PrintText.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
            PrintText.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

            string tttgdf = string.Empty;



				
				
				
// Set text to chart label color and font
			//textFormat			= chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

//			// Loop through each Plot Values on the chart
			
//			Point endPoint1		= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y);
//			Point endPoint2		= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y);
//			Point nextPoint		= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y);			
		
				
		

		
			
			
			// START ARROWS
			
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode222 = RenderTarget.AntialiasMode;

//						bool dooooo = false;
						
//						if (dooooo)
						
			if (!IsInHitTest)
			if (pArrowsEnabled)
			{
			
				
				
				
			
				
				SharpDX.Direct2D1.Brush longBrushDX33 = pArrowUpFBrush.ToDxBrush(RenderTarget);			
				SharpDX.Direct2D1.Brush shortBrushDX = pArrowDownFBrush.ToDxBrush(RenderTarget);				
				SharpDX.Direct2D1.Brush arrowBrushDX = pArrowUpFBrush.ToDxBrush(RenderTarget);

				Stroke ThisStroke = pArrowDownStroke;

	            int FB2 = ChartBars.FromIndex;
	            int LB2 = ChartBars.ToIndex;
	            int BB2 = 0;
	            int xt3 = 0;
	            int yt2 = 0;
	            double yt223 = 0;

	            LB2 = Math.Min(CurrentBars[0], LB2);
	            BB2 = CurrentBars[0] - LB2;

				// ARROWS

				
			
				
				
				
				TextFormat	LabelText3Format			= pTextFont.ToDirectWriteTextFormat();
			
				TextLayout LabelText3Layout = new TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, "", LabelText3Format, 1000, LabelText3Format.FontSize);
			
				LabelText3Format.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
				LabelText3Format.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
				LabelText3Format.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
			
            	SharpDX.Direct2D1.Brush LabeLB2rushDX = ChartControl.Properties.ChartText.ToDxBrush(RenderTarget);							
				Point Text3Point		= new Point(0, 0);

				
				ChartPanel chartPanel	= chartControl.ChartPanels[chartScale.PanelIndex];
				SharpDX.Direct2D1.PathGeometry arrowGeo;
				
				int ooooarrow1 = 0;
				int ooootext1 = 0;				
				int ooooarrow = 0;
				int ooootext = 0;
				
				
				
			
				
				
				//Print("bug4");
				
				
				if (ChartBars != null)
				for (int i = FB2; i <= LB2; i++)
				{
									
					int BarsBack = CurrentBars[0] - i;				
					BB2 = i;
					
					string ThisSignalName = string.Empty;
					string ThisSignalNameP = string.Empty;
					double ThisSignal = 0;
					double ThisTrend = 0;
					
				
					
			
					

					
					double vvvv = 0;
				
//					if (pSignalMode == "EMA")
//					{
//						if (Signals01.IsValidDataPointAt(BB2))
//							vvvv = Signals01.GetValueAt(BB2);
//					}
//					else if (pSignalMode == "NonLagMA")
//					{
//						if (Signals02.IsValidDataPointAt(BB2))
//							vvvv = Signals02.GetValueAt(BB2);
//					}					
//					else if (pSignalMode == "MACD")
//					{
//						if (Signals03.IsValidDataPointAt(BB2))
//							vvvv = Signals03.GetValueAt(BB2);
//					}							
//					else if (pSignalMode == "System 1")
//					{
//						if (Signals04.IsValidDataPointAt(BB2))
//							vvvv = Signals04.GetValueAt(BB2);
//					}							
//					else if (pSignalMode == "System 2")
//					{
//						if (Signals05.IsValidDataPointAt(BB2))
//							vvvv = Signals05.GetValueAt(BB2);
//					}		
//					else if (pSignalMode == "System 3")
//					{
//						if (Signals05.IsValidDataPointAt(BB2))
//							vvvv = Signals09.GetValueAt(BB2);
//					}		
										
					if (pSignalMode == "EMA")
					{
						if (Signals01.IsValidDataPointAt(BB2))
							vvvv = Signals01.GetValueAt(BB2);
					}
				
					else if (pSignalMode == "HMA")
					{
						if (Signals05.IsValidDataPointAt(BB2))
							vvvv = Signals05.GetValueAt(BB2);
					}		
				
					else if (pSignalMode == "T3")
					{
						if (Signals04.IsValidDataPointAt(BB2))
							vvvv = Signals04.GetValueAt(BB2);
					}	
					
					
					
//					if (vvvv != 0)
//					Print(BB2 + "  " + vvvv);
				
					
				
					
									
						// set opacity			
//								if (pTradeTrack == "Bar")
//								{
									
//									ooooarrow1 = pNotActiveArrowOpacity;
//									ooootext1 = pNotActiveLabelOpacity;

//								}
//								else
//								{
									
//									if (pSignalMode == "All")
//									{
//										ooooarrow1 = pActiveArrowOpacity;
//										ooootext1 = pActiveLabelOpacity;
//									}
//									else if (ThisSignalNameP.Contains(pSignalMode))
//									{
//										ooooarrow1 = pActiveArrowOpacity;
//										ooootext1 = pActiveLabelOpacity;
										
//									}
//									else
//									{									
//										ooooarrow1 = pNotActiveArrowOpacity;
//										ooootext1 = pNotActiveLabelOpacity;
//									}
									
//								}
								
								
																
					ooooarrow1 = pActiveArrowOpacity;
										
					ooootext1 = pActiveLabelOpacity;	
						
								
				
					
						ooooarrow = ooooarrow1;
						ooootext = ooootext1;	
					
					
					
					
					ThisSignalName = string.Empty;
					
					if (vvvv == 1)
					{
						ThisSignalName = pSignalMode;
						ThisSignalName = pLabelBuy;
						
						
						ThisSignal = vvvv;
					}
						
					 
				
								
					
					if (ThisSignalName != string.Empty)
					{
					
//						Print(ThisSignal);
//						Print(ThisSignalName);
//						Print(ooooarrow);
//						Print(ooootext);
						
						ThisSignal = 1;
						
						xt3 = chartControl.GetXByBarIndex(ChartBars, i);
						
						int pTextOffset = 0;
						string LB22 = string.Empty;
						float newy = 0;
						float newx = 0;
						float totalarrowheight = pArrowOffset + pArrowSize + pArrowBarHeight;
							
	
						
						
          
				
						
						
						if (ThisSignal == 1)
						{

							yt2 = chartScale.GetYByValue(SignalLow.GetValueAt(BB2));
							yt223 = chartScale.GetYByValueWpf(SignalLow.GetValueAt(BB2));
							arrowBrushDX = longBrushDX33;
							ThisStroke = pArrowUpStroke;	
							
						}
						
						if (ThisSignal == -1)
						{			
							yt2 = chartScale.GetYByValue(SignalHigh.GetValueAt(BB2));
							yt223 = chartScale.GetYByValueWpf(SignalHigh.GetValueAt(BB2));
							arrowBrushDX = shortBrushDX;
							ThisStroke = pArrowDownStroke;	
							
						}	
				
					
						
					
					//Print("bug7");
						
						arrowGeo = CreateArrowGeometry(chartControl, chartPanel, chartScale, xt3, yt2, (int)ThisSignal);
						
						RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							
								
						arrowBrushDX.Opacity = ooooarrow/100f; 		
						ThisStroke.BrushDX.Opacity = ooooarrow/100f; 	
								
						RenderTarget.FillGeometry(arrowGeo, arrowBrushDX); 
						RenderTarget.DrawGeometry(arrowGeo, ThisStroke.BrushDX, ThisStroke.Width, ThisStroke.StrokeStyle);
						
							
						
														
						LabelText3Layout = new TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, LB22, LabelText3Format, 1000, 1000);

						float boxpadding = LabelText3Format.FontSize;
						
           
						float RectWidth = LabelText3Layout.Metrics.Width + (float) pTextFont.Size;
						float RectHeight = LabelText3Layout.Metrics.Height  + (float) pTextFont.Size / 2f + 1;
						
//						if (ThisSignal == 1)
//						{
//							LB22 = pLabelBuy;
//							newy = yt2 + totalarrowheight + 1 + pTextOffset;
							
//						}
						
//						if (ThisSignal == -1)
//						{
//							LB22 = pLabelSell;
//							newy = yt2 - totalarrowheight - RectHeight - 1 - pTextOffset;

//						}	
								
		
						if (ThisSignal == 1)
						{
							
							//LB22 = pLabelBuy;
							//if (pLabelsEnabled23)
							//LB22 = LB22 + " " + SignalNameLong.GetValueAt(BB2).ToString();// + " " + TrendSlope.GetValueAt(BB2).ToString();
							LB22 = ThisSignalName;
							
							//LB22 = Lows[0].GetValueAt(BB2-3).ToString() + " " + Lows[0].GetValueAt(BB2-1).ToString() ;
							
							newy = yt2 + totalarrowheight + 1 + pTextOffset;
							
						}
						
						if (ThisSignal == -1)
						{
							//LB22 = pLabelSell;
							//if (pLabelsEnabled23) 
							//LB22 = LB22 + " " + SignalNameShort.GetValueAt(BB2).ToString();// + " " + TrendSlope.GetValueAt(BB2).ToString();
							LB22 = ThisSignalName;
							
							newy = yt2 - totalarrowheight - RectHeight - 1 - pTextOffset;

						}	
								
						
						
						
						
//						LB22 = LB22 + " " + BarCount.GetValueAt(BB2).ToString().Replace("-", "");
						
						newx = xt3 - RectWidth/2 - 2;								
													
						Text3Point = new Point(newx, newy);
					
						
						
						
						SharpDX.RectangleF Text3Rect = new SharpDX.RectangleF(newx, newy, RectWidth, RectHeight);

//								{
						
//									RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
//									RenderTarget.DrawRectangle(rect2, pBrush08.BrushDX, pBrush08.Width, pBrush08.StrokeStyle);
//									RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							
//								}
						
						
						LabeLB2rushDX = ChartControl.Properties.ChartText.ToDxBrush(RenderTarget);
						LabeLB2rushDX.Opacity = ooootext/100f;
						
						if (pLabelsEnabled)
							RenderTarget.DrawText(LB22, LabelText3Format, Text3Rect, LabeLB2rushDX);
						
						RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
						
						
		
		
//						if (pExitOEnabled)
//						{
								
//							string ThisText = string.Empty;	
//							double ThisLine = TargetATRPrice.GetValueAt(BB2);
							
//							x1 = chartControl.GetXByBarIndex(ChartBars,i);
//							x2 = chartControl.GetXByBarIndex(ChartBars,i)+100;
							
								
//							ThisLine = TargetATRPrice.GetValueAt(BB2);	
								
//							y1 = chartScale.GetYByValue(ThisLine);
			
//							Point StartPoint2	= new Point(x1,y1);
//							Point EndPoint2 = new Point(x2,y1);

//							ThisStroke = pTargetStroke;
//							RenderTarget.DrawLine(StartPoint2.ToVector2(), EndPoint2.ToVector2(), ThisStroke.BrushDX, ThisStroke.Width, ThisStroke.StrokeStyle);
								
								
//							ThisText = "Target 1";							
//							LabelText3Layout = new TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, ThisText, LabelText3Format, 1000, 1000);
//							LabeLB2rushDX = ThisStroke.BrushDX;
//							LabeLB2rushDX.Opacity = 0.5f;
							
//							boxpadding = LabelText3Format.FontSize;
//							RectWidth = LabelText3Layout.Metrics.Width + (float) pTextFont.Size;
//							RectHeight = LabelText3Layout.Metrics.Height  + (float) pTextFont.Size / 2f + 1;
					
//							newy = (float) y1 - RectHeight/2;
//							newx = (float) x1 - RectWidth - 1;								

//							Text3Rect = new SharpDX.RectangleF(newx, newy, RectWidth, RectHeight);
							
//							RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
//							RenderTarget.DrawText(ThisText, LabelText3Format, Text3Rect, LabeLB2rushDX);
//						//	RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
						
							
							
								
//							ThisLine = TargetATRPrice2.GetValueAt(BB2);	
								
//							y1 = chartScale.GetYByValue(ThisLine);
			
//							StartPoint2	= new Point(x1,y1);
//							EndPoint2 = new Point(x2,y1);

//							ThisStroke = pTargetStroke;
//							RenderTarget.DrawLine(StartPoint2.ToVector2(), EndPoint2.ToVector2(), ThisStroke.BrushDX, ThisStroke.Width, ThisStroke.StrokeStyle);
								
//							ThisText = "Target 2";							
//							LabelText3Layout = new TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, ThisText, LabelText3Format, 1000, 1000);
//							LabeLB2rushDX = ThisStroke.BrushDX;
//							LabeLB2rushDX.Opacity = 0.5f;
							
//							boxpadding = LabelText3Format.FontSize;
//							RectWidth = LabelText3Layout.Metrics.Width + (float) pTextFont.Size;
//							RectHeight = LabelText3Layout.Metrics.Height  + (float) pTextFont.Size / 2f + 1;
					
//							newy = (float) y1 - RectHeight/2;
//							newx = (float) x1 - RectWidth - 1;								

//							Text3Rect = new SharpDX.RectangleF(newx, newy, RectWidth, RectHeight);
//							RenderTarget.DrawText(ThisText, LabelText3Format, Text3Rect, LabeLB2rushDX);													
								
								
						
//							ThisLine = StopATRPrice.GetValueAt(BB2);	
							
								
//							y1 = chartScale.GetYByValue(ThisLine);
			
//							StartPoint2	= new Point(x1,y1);
//							EndPoint2 = new Point(x2,y1);
							
						
						
//							ThisStroke = pStopStroke;
//							RenderTarget.DrawLine(StartPoint2.ToVector2(), EndPoint2.ToVector2(), ThisStroke.BrushDX, ThisStroke.Width, ThisStroke.StrokeStyle);
							
//							ThisText = "Stop Loss";							
//							LabelText3Layout = new TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, ThisText, LabelText3Format, 1000, 1000);
//							LabeLB2rushDX = ThisStroke.BrushDX;
//							LabeLB2rushDX.Opacity = 0.5f;
							
//							boxpadding = LabelText3Format.FontSize;
//							RectWidth = LabelText3Layout.Metrics.Width + (float) pTextFont.Size;
//							RectHeight = LabelText3Layout.Metrics.Height  + (float) pTextFont.Size / 2f + 1;
					
//							newy = (float) y1 - RectHeight/2;
//							newx = (float) x1 - RectWidth - 1;								

//							Text3Rect = new SharpDX.RectangleF(newx, newy, RectWidth, RectHeight);
//							RenderTarget.DrawText(ThisText, LabelText3Format, Text3Rect, LabeLB2rushDX);							
							
							
								
//						}
					
					
					}
					
					
					
					//ThisSignal = SellSignals.GetValueAt(BB2);	
					
//					if (SignalNameShort.GetValueAt(BB2).ToString() != string.Empty)	
//						ThisSignal = -1;
					
					//ThisSignalName = string.Empty;
					//ThisSignalName = SignalNameShort.GetValueAt(BB2).ToString();	
					
		
					
					
					
									
											// set opacity			
//								if (pTradeTrack == "Bar")
//								{
									
//									ooooarrow1 = pNotActiveArrowOpacity;
//									ooootext1 = pNotActiveLabelOpacity;

//								}
//								else
//								{
									
//									if (pSignalMode == "All")
//									{
//										ooooarrow1 = pActiveArrowOpacity;
//										ooootext1 = pActiveLabelOpacity;
//									}
//									else if (ThisSignalNameP.Contains(pSignalMode))
//									{
//										ooooarrow1 = pActiveArrowOpacity;
//										ooootext1 = pActiveLabelOpacity;
										
//									}
//									else
//									{									
//										ooooarrow1 = pNotActiveArrowOpacity;
//										ooootext1 = pNotActiveLabelOpacity;
//									}
									
//								}
								
										ooooarrow1 = pActiveArrowOpacity;
										ooootext1 = pActiveLabelOpacity;
								
//					if (!pSignal1 && ThisSignalNameP.Contains("Trend"))
//					{
//						ooooarrow = 0;
//						ooootext = 0;	
//					}
//					if (!pSignal2 && ThisSignalNameP.Contains("First Bar"))
//					{
//						ooooarrow = 0;
//						ooootext = 0;							
//					}
//					if (!pSignal3 && ThisSignalNameP.Contains("Break"))
//					{
//						ooooarrow = 0;
//						ooootext = 0;							
//					}	
					
//					if (pSignal1 && ThisSignalNameP.Contains("Trend"))
//					{
//						ooooarrow = ooooarrow1;
//						ooootext = ooootext1;	
//					}
//					if (pSignal2 && ThisSignalNameP.Contains("First Bar"))
//					{
//						ooooarrow = ooooarrow1;
//						ooootext = ooootext1;							
//					}
//					if (pSignal3 && ThisSignalNameP.Contains("Break"))
//					{
//						ooooarrow = ooooarrow1;
//						ooootext = ooootext1;							
//					}						
						
					
					
							ooooarrow = ooooarrow1;
						ooootext = ooootext1;								
					
								
						
				
					ThisSignalName = string.Empty;
					
					if (vvvv == -1)
					{
						ThisSignalName = pSignalMode;
						ThisSignalName = pLabelSell;
						ThisSignal = vvvv;
					}
						
					
					
				
					if (ThisSignalName != string.Empty)
						
					
					{
						ThisSignal = -1;
						
						xt3 = chartControl.GetXByBarIndex(ChartBars, i);
						
						int pTextOffset = 0;
						string LB22 = string.Empty;
						float newy = 0;
						float newx = 0;
						float totalarrowheight = pArrowOffset + pArrowSize + pArrowBarHeight;
							
	
						
						if (ThisSignal == 1)
						{

							yt2 = chartScale.GetYByValue(SignalLow.GetValueAt(BB2));
							yt223 = chartScale.GetYByValueWpf(SignalLow.GetValueAt(BB2));
							arrowBrushDX = longBrushDX33;
							ThisStroke = pArrowUpStroke;	
							
						}
						
						if (ThisSignal == -1)
						{			
							yt2 = chartScale.GetYByValue(SignalHigh.GetValueAt(BB2));
							yt223 = chartScale.GetYByValueWpf(SignalHigh.GetValueAt(BB2));
							arrowBrushDX = shortBrushDX;
							ThisStroke = pArrowDownStroke;	
							
						}	
						
						arrowGeo = CreateArrowGeometry(chartControl, chartPanel, chartScale, xt3, yt2, (int)ThisSignal);
						
						RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
								
						arrowBrushDX.Opacity = ooooarrow/100f; 		
						ThisStroke.BrushDX.Opacity = ooooarrow/100f; 	
						
						RenderTarget.FillGeometry(arrowGeo, arrowBrushDX); 
						RenderTarget.DrawGeometry(arrowGeo, ThisStroke.BrushDX, ThisStroke.Width, ThisStroke.StrokeStyle);
						
							
						
														
						LabelText3Layout = new TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, LB22, LabelText3Format, 1000, 1000);

						float boxpadding = LabelText3Format.FontSize;
						
           
						float RectWidth = LabelText3Layout.Metrics.Width + (float) pTextFont.Size;
						float RectHeight = LabelText3Layout.Metrics.Height  + (float) pTextFont.Size / 2f + 1;
						
//						if (ThisSignal == 1)
//						{
//							LB22 = pLabelBuy;
//							newy = yt2 + totalarrowheight + 1 + pTextOffset;
							
//						}
						
//						if (ThisSignal == -1)
//						{
//							LB22 = pLabelSell;
//							newy = yt2 - totalarrowheight - RectHeight - 1 - pTextOffset;

//						}	
								
		
						if (ThisSignal == 1)
						{
							
							//LB22 = pLabelBuy;
							//if (pLabelsEnabled23)
							LB22 = LB22 + " " + SignalNameLong.GetValueAt(BB2).ToString();// + " " + TrendSlope.GetValueAt(BB2).ToString();
							LB22 = ThisSignalName;
							
							//LB22 = Lows[0].GetValueAt(BB2-3).ToString() + " " + Lows[0].GetValueAt(BB2-1).ToString() ;
							
							newy = yt2 + totalarrowheight + 1 + pTextOffset;
							
						}
						
						if (ThisSignal == -1)
						{
							//LB22 = pLabelSell;
							//if (pLabelsEnabled23) 
							//LB22 = LB22 + " " + SignalNameShort.GetValueAt(BB2).ToString();// + " " + TrendSlope.GetValueAt(BB2).ToString();
							
					LB22 = ThisSignalName;// = SignalNameLong.GetValueAt(BB2).ToString();
							
							newy = yt2 - totalarrowheight - RectHeight - 1 - pTextOffset;

						}	
								
						
						
						
						
//						LB22 = LB22 + " " + BarCount.GetValueAt(BB2).ToString().Replace("-", "");
						
						newx = xt3 - RectWidth/2 - 2;								
													
						Text3Point = new Point(newx, newy);
					
						
						
						
						SharpDX.RectangleF Text3Rect = new SharpDX.RectangleF(newx, newy, RectWidth, RectHeight);

//								{
						
//									RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
//									RenderTarget.DrawRectangle(rect2, pBrush08.BrushDX, pBrush08.Width, pBrush08.StrokeStyle);
//									RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							
//								}
						
						
						LabeLB2rushDX = ChartControl.Properties.ChartText.ToDxBrush(RenderTarget);
						LabeLB2rushDX.Opacity = ooootext/100f;
						
						
						if (pLabelsEnabled)
							RenderTarget.DrawText(LB22, LabelText3Format, Text3Rect, LabeLB2rushDX);
						
						RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
						
	
					
					}
					
					
				}
		
				longBrushDX33.Dispose();
				shortBrushDX.Dispose();
				arrowBrushDX.Dispose();	
				
				LabelText3Format.Dispose();
				LabelText3Layout.Dispose();
				LabeLB2rushDX.Dispose();
				
			}
				
			

			
			RenderTarget.AntialiasMode = oldAntialiasMode222;
			
			// END ARROWS
			
			
			
				//Print("RenderTest 1");	
			
			
				
			
			
			
			ChartBackgroundFadeBrushDX = ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget);
			ChartBackgroundFadeBrushDX.Opacity = 80/100f;			
			
			
			thistop2 = ChartPanel.H;
			
			
			
			
				
			
		

				// table
				
				bool doddododod = false;
				
				
				if (doddododod)
				
				if (pSecondaryFeedsDisplayEnabled)
				if (pSecondaryFeedsEnabled)
				if (!IsInHitTest)
				{
					// 1.2 - SharpDX Brush Resources

					// RenderTarget commands must use a special brush resource defined in the SharpDX.Direct2D1 namespace
					// These resources exist just like you will find in the WPF/Windows.System.Media namespace
					// such as SolidColorBrushes, LienarGraidentBrushes, RadialGradientBrushes, etc.
					// To begin, we will start with the most basic "Brush" type
					// Warning:  Brush objects must be disposed of after they have been used
					
					SharpDX.Direct2D1.Brush areaBrushDx;
					//SharpDX.Direct2D1.Brush smallAreaBrushDx;
					SharpDX.Direct2D1.Brush textBrushDx;
					SharpDX.Direct2D1.Brush fillBrushDx;
					
					// for convenience, you can simply convert a WPF Brush to a DXBrush using the ToDxBrush() extension method provided by NinjaTrader
					// This is a common approach if you have a Brush property created e.g., on the UI you wish to use in custom rendering routines
					
					areaBrushDx = areaBrush.ToDxBrush(RenderTarget);
					//smallAreaBrushDx = smallAreaBrush.ToDxBrush(RenderTarget);
					textBrushDx = pColorTextBrush.ToDxBrush(RenderTarget);
					fillBrushDx = pColorTextBrush.ToDxBrush(RenderTarget);
					
				
//					pFillUpBrush.Freeze();
//					pFillDownBrush.Freeze();
//					pFillNeutralBrush.Freeze();
					
					
					// 1.6 - Simple Text Rendering

					// For rendering custom text to the Chart, there are a few ways you can approach depending on your requirements
					// The most straight forward way is to "borrow" the existing chartControl font provided as a "SimpleFont" class
					// Using the chartControl LabelFont, your custom object will also change to the user defined properties allowing 
					// your object to match different fonts if defined by user.  

					// The code below will use the chartControl Properties Label Font if it exists,
					// or fall back to a default property if it cannot obtain that value
					NinjaTrader.Gui.Tools.SimpleFont wpfFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);

					// the advantage of using a SimpleFont is they are not only very easy to describe 
					// but there is also a convenience method which can be used to convert the SimpleFont to a SharpDX.DirectWrite.TextFormat used to render to the chart
					// Warning:  TextFormat objects must be disposed of after they have been used
					SharpDX.DirectWrite.TextFormat textFormat1 = wpfFont.ToDirectWriteTextFormat();

					textFormat1			= TextFont.ToDirectWriteTextFormat();	
						
					// Once you have the format of the font, you need to describe how the font needs to be laid out
					// Here we will create a new Vector2() which draws the font according to the to top left corner of the chart (offset by a few pixels)
					SharpDX.Vector2 upperTextPoint = new SharpDX.Vector2(ChartPanel.X + 10, ChartPanel.Y + 20);
					// Warning:  TextLayout objects must be disposed of after they have been used
					SharpDX.DirectWrite.TextLayout textLayout1 =
						new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
							NinjaTrader.Custom.Resource.SampleCustomPlotUpperLeftCorner, textFormat1, ChartPanel.X + ChartPanel.W,
							textFormat1.FontSize);

					// With the format and layout of the text completed, we can now render the font to the chart

				
					FB = ChartBars.FromIndex;
		            LB = ChartBars.ToIndex;
		          
								
					AllStrings.Clear();
					List<string> TempRow = new List<string>();
					AllStrings.Add(new List<string>(TempRow));

					bool B1 = true;
					
					string UP = "Up";
					string DN = "Down";
					string NU = "Flat";
					
					string S1 = "Trend Is ";
					string S2 = "PC Is ";
					string S3 = "STO Is ";
					string S4 = "MOM Is ";
					string S5 = "DAD Is ";
					
					int N1 = 0;
					int N2 = 0;
					int N3 = 0;
					int N4 = 0;
					int N5 = 0;
					
					if (pFeed4Enabled) N1 = Direction4.GetValueAt(LB);
					if (pFeed3Enabled) N2 = Direction3.GetValueAt(LB);
					if (pFeed2Enabled) N3 = Direction2.GetValueAt(LB);
					if (pFeed1Enabled) N4 = Direction1.GetValueAt(LB);
					
//					N5 = Direction5.GetValueAt(LB);
					
//					if (pFlashEnabled)
//					{
						
//						if (R1Left.TotalSeconds <= pSecondsA1 && IsOdd((int) Math.Round(R1Left.TotalSeconds,0)))
//							N1 = 5;
//						if (R2Left.TotalSeconds <= pSecondsA2 && IsOdd((int) Math.Round(R2Left.TotalSeconds,0)))
//							N2 = 5;	
//						if (R3Left.TotalSeconds <= pSecondsA3 && IsOdd((int) Math.Round(R3Left.TotalSeconds,0)))
//							N3 = 5;
//						if (R4Left.TotalSeconds <= pSecondsA4 && IsOdd((int) Math.Round(R4Left.TotalSeconds,0)))
//							N4 = 5;
						
//					}
					
					//Print("test3");
					S1 = SFeed4;
					S2 = SFeed3;
					S3 = SFeed2;
					S4 = SFeed1;
										
					
//					string T1 = pEMAPeriod4.ToString();
//					string T2 = pEMAPeriod3.ToString();
//					string T3 = pEMAPeriod2.ToString();
//					string T4 = pEMAPeriod1.ToString();	
					
					
//					S1 = T1 + " " + AcceptableBasePeriodType4.ToString();
//					S2 = T2 + " " + AcceptableBasePeriodType3.ToString();
//					S3 = T3 + " " + AcceptableBasePeriodType2.ToString();	
//					S4 = T4 + " " + AcceptableBasePeriodType1.ToString();
					
					
//					S3 = T3 + "M"  + pCB3S + TL(R3Left) + pCB3S + VOL3.ToString();
//					S4 = T4 + "M"  + pCB3S + TL(R4Left) + pCB3S + VOL4.ToString();	
					
					
//					if (N1 == 1)
//						S1 = S1 + UP;
//					if (N2 == 1)
//						S2 = S2 + UP;
//					if (N3 == 1)
//						S3 = S3 + UP;
//					if (N4 == 1)
//						S4 = S4 + UP;
//					if (N5 == 1)
//						S5 = S5 + UP;
					
//					if (N1 == -1)
//						S1 = S1 + DN;
//					if (N2 == -1)
//						S2 = S2 + DN;
//					if (N3 == -1)
//						S3 = S3 + DN;
//					if (N4 == -1)
//						S4 = S4 + DN;
//					if (N5 == -1)
//						S5 = S5 + DN;
					
//					if (N1 == 0)
//						S1 = S1 + NU;
//					if (N2 == 0)
//						S2 = S2 + NU;
//					if (N3 == 0)
//						S3 = S3 + NU;
//					if (N4 == 0)
//						S4 = S4 + NU;
//					if (N5 == 0)
//						S5 = S5 + NU;					
					
					//if (pShowPreviousRange)
					{
						
					
						if (pFeed4Enabled) 
						{
					
							TempRow.Clear();	
							TempRow.Add("0");
	//						TempRow.Add(S1 + Environment.NewLine + "ASDSA");
							TempRow.Add(S1);		
							AllStrings.Add(new List<string>(TempRow));					
							
						}
						if (pFeed3Enabled) 
						{
							
							TempRow.Clear();	
							TempRow.Add("0");
							TempRow.Add(S2);	
							AllStrings.Add(new List<string>(TempRow));	
						}
						if (pFeed2Enabled) 
						{
							
							TempRow.Clear();	
							TempRow.Add("0");
							TempRow.Add(S3);
							AllStrings.Add(new List<string>(TempRow));	
						}
						if (pFeed1Enabled) 
						{
							TempRow.Clear();	
							TempRow.Add("0");
							TempRow.Add(S4);	
							AllStrings.Add(new List<string>(TempRow));		
						}
						
						
						
//						TempRow.Clear();	
//						TempRow.Add("0");
//						TempRow.Add(S5);
//						AllStrings.Add(new List<string>(TempRow));		
				
						
						//TempRow.Clear();	
						//TempRow.Add("0");
						//if (pShowUpperWick) TempRow.Add(UpperWickD.GetValueAt(LB-1).ToString(FS));
						//if (pShowBarRange) TempRow.Add(BarRangeD.GetValueAt(LB-1).ToString(FS));
						//if (pShowLowerWick) TempRow.Add(LowerWickD.GetValueAt(LB-1).ToString(FS));
						//AllStrings.Add(new List<string>(TempRow));						
					}

					//Print("test4");
					
					
					AllColors.Clear();
					List<int> TempRow2 = new List<int>();
					AllColors.Add(new List<int>(TempRow2));

					//if (pShowPreviousRange)
					{
					
						if (pFeed4Enabled) 
						{
					
									TempRow2.Clear();	
						TempRow2.Add(0);
						TempRow2.Add(N1);						
						AllColors.Add(new List<int>(TempRow2));	
						
						}
						if (pFeed3Enabled) 
						{
							
						TempRow2.Clear();	
						TempRow2.Add(0);
						TempRow2.Add(N2);						
						AllColors.Add(new List<int>(TempRow2));	
						
						}
						if (pFeed2Enabled) 
						{

						TempRow2.Clear();	
						TempRow2.Add(0);
						TempRow2.Add(N3);						
						AllColors.Add(new List<int>(TempRow2));		
						}
						if (pFeed1Enabled) 
						{
						
						TempRow2.Clear();	
						TempRow2.Add(0);
						TempRow2.Add(N4);						
						AllColors.Add(new List<int>(TempRow2));			
						}
						
						
				
					
					
						
//						TempRow2.Clear();	
//						TempRow2.Add(0);
//						TempRow2.Add(N5);						
//						AllColors.Add(new List<int>(TempRow2));	
						
				
						//TempRow2.Clear();	
						//TempRow2.Add(0);
						//if (pShowUpperWick) TempRow2.Add(Direction.GetValueAt(LB-1));
						//if (pShowBarRange) TempRow2.Add(Direction.GetValueAt(LB-1));
						//if (pShowLowerWick) TempRow2.Add(Direction.GetValueAt(LB-1));
						//AllColors.Add(new List<int>(TempRow2));						
					}

				
	  
					//Print("test5");
	        
					
					// 1.7 - Advanced Text Rendering

					// Font formatting and text layouts can get as complex as you need them to be
					// This example shows how to use a complete custom font unrelated to the existing user-defined chart control settings
					// Warning:  TextLayout and TextFormat objects must be disposed of after they have been used
//					SharpDX.DirectWrite.TextFormat textFormat2 =
//						new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory, "Century Gothic", FontWeight.Bold,
//							FontStyle.Italic, 32f);
						
					SharpDX.DirectWrite.TextFormat textFormat2			= TextFont.ToDirectWriteTextFormat();	


		            textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; // leading = left.
		            textFormat2.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center; // far = bottom.
		            textFormat2.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
					
					//Print("test6");
					
					string TXT11 = "asd";
					string TXT12 = "as1";
					string TXT13 = "as2";
					string TXT21 = "as2";
					string TXT22 = "as3";
					string TXT23 = "a4d";
					
					  text = "PU";
					//text.count
					
					textLayout2 =
						new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, text, textFormat2, 100, textFormat2.FontSize);

					//textLayout2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;	
					
		          
						int YCellPadding = 6;
						int XCellPadding = 12;
						int pSpaceBtwRects = pMarginB + 1;
						int TableMarginX = pPixelsFromRight + ChartPanel.X;
					
							//if (pActiveOutlineEnabled)
							{
								//yB2 = yB2 - pOrderBothOutlineStroke.Width;
								//TableMarginX = TableMarginX + (int) pOrderBothOutlineStroke.Width;
								
							}
					
					
						int TableMarginY = pPixelsFromBottom;
					
						int RectHeight = (int) textLayout2.Metrics.Height + YCellPadding;
						int RectWidth = (int) textLayout2.Metrics.Width + XCellPadding + 2;

						int RectX = ChartPanel.W  - TableMarginX;
						int RectY = ChartPanel.H  - TableMarginY;
						
					// at bottom
						int StartY = RectY + 2;	// two pixel ninja restriction at bottom of chart
					
						// top 
						//if (pTPosition == LVVolume_TablePosition.TopLeft || pTPosition == LVVolume_TablePosition.TopRight)
						StartY = TableMarginY + RectHeight - 1 + 2;
					
						bool ShowRects = true;
						
						int NumOfColumns = 0;
						int NumOfRows = 0; 
				
//						if (pShowCurrentRange)
//							NumOfColumns = NumOfColumns + 2;
//						if (pShowPreviousRange)
//							NumOfColumns = NumOfColumns + 2;
//						if (pShowUpperWick)
//							NumOfRows = NumOfRows + 1;
//						if (pShowBarRange)
//							NumOfRows = NumOfRows + 1;
//						if (pShowLowerWick)
//							NumOfRows = NumOfRows + 1;
				
					if (pFeed1Enabled) NumOfColumns = NumOfColumns + 1;
					if (pFeed2Enabled) NumOfColumns = NumOfColumns + 1;
					if (pFeed3Enabled) NumOfColumns = NumOfColumns + 1;
					if (pFeed4Enabled) NumOfColumns = NumOfColumns + 1;
							
					//Print("test7");
					
					
						//NumOfColumns = 4;
						NumOfRows = 1;
					
						int X11 = RectX;
						int Y11 = RectY;
					
						int TotalWidth = 0;
					
						int MaxWidth = 0;
							
						for (int i = NumOfRows; i >= 1; i--)
						{

							
							RectX = ChartPanel.W - TableMarginX;
								
							
							for (int j = NumOfColumns; j >= 1; j--)
							{
								
								int RectWidth2 = RectWidth;
//								if (j == 2 || j == 4)
//									RectWidth2 = pColumnWidthP;
								
								text = AllStrings[j][i];
								
								// insert dynamic width
								
								textLayout2 = new TextLayout(Globals.DirectWriteFactory, text, textFormat2, 1000, textFormat2.FontSize);	
								
								RectWidth2 = (int) textLayout2.Metrics.Width + XCellPadding;
								
								MaxWidth = Math.Max(RectWidth2, MaxWidth);
								
								if (j != NumOfColumns)
									TotalWidth = TotalWidth + RectWidth2 + pSpaceBtwRects;
								else
								{
									TotalWidth = TotalWidth + RectWidth2;
								}

							}
							
							//RectY = RectY - RectHeight;
							
						}
						
					
				//Print("test8");		
						
						//TextFormat	textFormat2			= TextFont.ToDirectWriteTextFormat();	
			
						//TextLayout textLayout2 = new TextLayout(Globals.DirectWriteFactory, "", textFormat2, 1000, textFormat2.FontSize);
					
					
						SharpDX.RectangleF			rect2			= new SharpDX.RectangleF(RectX, RectY, RectWidth, RectHeight);

						int MaxRectWidth = MaxWidth + 30; 
						
											
 
		//Print("test8");	
		
		
						for (int i = NumOfRows; i >= 1; i--)
						{

							// right side
							RectX = ChartPanel.W - TableMarginX + 0 + 2;  // - for the 2 pixels ninja doesn't allow drawing 
							
							// left side
							//if (pTPosition == LVVolume_TablePosition.TopLeft || pTPosition == LVVolume_TablePosition.BottomLeft)
							RectX = TotalWidth + TableMarginX + 1;	
							
							RectY = StartY - RectHeight;
							RectX = TableMarginX + 1;	
							
							if (AllButtonZ2 != null)
							AllButtonZ2.Clear();
							
							//Print("test9");	
							
							
							for (int j = NumOfColumns; j >= 1; j--)
							{
								
								
								
								int RectWidth2 = RectWidth;
//								if (j == 2 || j == 4)
//									RectWidth2 = pColumnWidthP;
								
								text = AllStrings[j][i];
								
								// insert dynamic width
								
								textLayout2 = new TextLayout(Globals.DirectWriteFactory, text, textFormat2, 1000, textFormat2.FontSize);	
								
								
								RectWidth2 = (int) textLayout2.Metrics.Width + XCellPadding;
								
								RectWidth2 = MaxWidth;
								
								
//								if (j != NumOfColumns)
//									RectX = RectX - RectWidth2 - pSpaceBtwRects;
//								else
									//RectX = RectX - RectWidth2;
								
								//Print("test10");	
								
								textBrushDx = pColorTextBrush.ToDxBrush(RenderTarget);
								
								//Print("test11");
								
									// these fillBrushDx lines causes render error in strategy
								
								
								
								fillBrushDx = pFillNeutralBrush.ToDxBrush(RenderTarget);							
								if (AllColors[j][i] == 1)
									fillBrushDx = pFillUpBrush.ToDxBrush(RenderTarget);								
								if (AllColors[j][i] == -1)
									fillBrushDx = pFillDownBrush.Clone().ToDxBrush(RenderTarget);
								
								//SharpDX.Direct2D1.Brush longBrushDX33 = pArrowUpFBrush.ToDxBrush(RenderTarget);	
								//Print("test13");
								
								//text = "as" + j.ToString() + "/" + i.ToString();
								
								//Print("test12");
								
								
								rect2			= new SharpDX.RectangleF(RectX, RectY, MaxRectWidth, RectHeight);
								
								fillBrushDx.Opacity = (float) pFillOpacity/100;
								if (ShowRects) RenderTarget.FillRectangle(rect2, fillBrushDx);	
								
								fillBrushDx.Opacity = (float) pOutlineOpacity/100;
								if (ShowRects) RenderTarget.DrawRectangle(rect2, fillBrushDx, 1);
								
								//Print("test14");
								
								if (AllButtonZ2 != null)
								AddButtonZ2(text,text,1,false,rect2);
								
								rect2			= ExpandRect(rect2, -10,-10,0,0);
								
								//Print("test15");
								
								textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading; // leading = left.
				         		RenderTarget.DrawText(text, textFormat2, rect2, textBrushDx);								
								
								bool IsIncluded = false;
								
								if (text == SFeed4)
									IsIncluded = pFeed4Included;
								if (text == SFeed3)
									IsIncluded = pFeed3Included;
								if (text == SFeed2)
									IsIncluded = pFeed2Included;								
								if (text == SFeed1)
									IsIncluded = pFeed1Included;
							
								
								text = "\u2713";
								textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing; // leading = left.
								
								if (IsIncluded)
								RenderTarget.DrawText(text, textFormat2, rect2, textBrushDx);	
								
								RectY = RectY + RectHeight + pSpaceBtwRects;
							}
							
							
							
						}
						
						
						
					
				
						
						
						
						
						
					// the textLayout object provides a way to measure the described font through a "Metrics" object
					// This allows you to create new vectors on the chart which are entirely dependent on the "text" that is being rendered
					// For example, we can create a rectangle that surrounds our font based off the textLayout which would dynamically change if the text used in the layout changed dynamically
					
					//int TableMargin = 5;	
						
					SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout2.Metrics.Width - TableMarginX,
						ChartPanel.Y + (ChartPanel.H - textLayout2.Metrics.Height - TableMarginY));
						
						
						
					SharpDX.RectangleF rect1 = new SharpDX.RectangleF(lowerTextPoint.X - XCellPadding, lowerTextPoint.Y - YCellPadding, textLayout2.Metrics.Width + XCellPadding,
						textLayout2.Metrics.Height + YCellPadding);

					// We can draw the Rectangle based on the TextLayout used above
					//RenderTarget.FillRectangle(rect1, smallAreaBrushDx);
					//RenderTarget.DrawRectangle(rect1, smallAreaBrushDx, 2);

					// And render the advanced text layout using the DrawTextLayout() method
					// Note:  When drawing the same text repeatedly, using the DrawTextLayout() method is more efficient than using the DrawText() 
					// because the text doesn't need to be formatted and the layout processed with each call
					//RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

					
					// 1.8 - Cleanup
					// This concludes all of the rendering concepts used in the sample
					// However - there are some final clean up processes we should always provided before we are done

					// If changed, do not forget to set the AntialiasMode back to the default value as described above as a best practice
					RenderTarget.AntialiasMode = oldAntialiasMode;

					// We also need to make sure to dispose of every device dependent resource on each render pass
					// Failure to dispose of these resources will eventually result in unnecessary amounts of memory being used on the chart
					// Although the effects might not be obvious as first, if you see issues related to memory increasing over time
					// Objects such as these should be inspected first
					areaBrushDx.Dispose();
						
						fillBrushDx.Dispose();
					//customDXBrush.Dispose();
					//gradientStopCollection.Dispose();
					//radialGradientBrush.Dispose();
					//smallAreaBrushDx.Dispose();
					textBrushDx.Dispose();
					textFormat1.Dispose();
					textFormat2.Dispose();
					textLayout1.Dispose();
					textLayout2.Dispose();
						
					textBrushDx.Dispose();
				
				}
				
				// end table
			
			
			//Print("RenderTest 4");	
	
			
            textLayout2 = new TextLayout(Core.Globals.DirectWriteFactory, tttgdf, PrintText, 10000, 10000);

         
      
			
			
			
			// new menu at bottom
			
			
			
			         float FinalH2 = textLayout2.Metrics.Height + 5;

            
	
					float buttonedgepadding = 3;
					
					//if (SomeBuy || SomeSell)
			
			
//			if (pLongEnabled || pShortEnabled)
//			if (pActiveDisplayEnabled && pActiveOutlineEnabled)
//			{
//						buttonedgepadding = buttonedgepadding + ThisStroke.Width + 2;
//			}

            B2 = new SharpDX.RectangleF(0, space - 4, 10000, pButtonSize + 2);
			B2 = new SharpDX.RectangleF(0, 0, 50, 10000);

			B2 = new SharpDX.RectangleF(0, ChartPanel.Y+ChartPanel.H-pButtonSize-buttonedgepadding-buttonedgepadding, 10000, pButtonSize + 2);

            // if (MouseIn(B2, 2, 2))

			

//			bool runit = false;
			
//			if (runit)
			
			
			
			
		
			
			
			
			
			
            float CY = (float)chartControl.CanvasRight - 48f;
			
			//CY = 30;
			
			if (!pHideMenuEnabled)
			InMenu = true;
			
			if (!IsInHitTest)
            if (pButtonsEnabled && InMenu)
                foreach (KeyValuePair<double, ButtonZ> thisbutton in AllButtonZ)
                //foreach (string xxx in AllButtons)
                {

                    string xxx = thisbutton.Value.Text;

                    string sd = xxx;



                    // szvv = graphics.MeasureString(sd, ChartControl.Font);

                    // int widdd = (int)szvv.Width + 8;


                    float widdd = 40;
                    widdd = Math.Max(pButtonSize, widdd);

                    if (thisbutton.Value.Width == 1)
                        widdd = pButtonSize;
                    else
                        widdd = thisbutton.Value.Width;


                    



                    SharpDX.DirectWrite.TextFormat ButtonText = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Normal,
                    SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 11.0F);

                    ButtonText = myProperties.LabelFont.ToDirectWriteTextFormat();

                    ButtonText.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                    ButtonText.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                    ButtonText.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

                    TextLayout textLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, thisbutton.Value.Text, ButtonText, 10000, 10000);

                   // Print(textLayout1.Metrics.Width);


                    float FinalH = textLayout1.Metrics.Height;

                    FinalH = Math.Max(pButtonSize, FinalH);

                    float FinalW = Math.Max(FinalH,textLayout1.Metrics.Width);

                    if (thisbutton.Value.Width == 1) // square buttons
                        FinalW = FinalH;
                    else if (xxx =="Blank") // spacer for blank buttons
                        FinalW = 10; 
					 else
                        FinalW = textLayout1.Metrics.Width + 8;


                   CY = CY - FinalW - space;
					
					
					//CY = CY + FinalH + space;
					
				
					
					if (xxx != "Blank")
					{
					
						
						
	//                    thisbutton.Value.Rect = new SharpDX.RectangleF(CY, space, FinalW, FinalH);
	//                    CY = CY - space;

						// switch to left side
						
						
						
						// BOTTOM LOCATION
						
						 thisbutton.Value.Rect = new SharpDX.RectangleF(CY, (float)ChartPanel.Y+ChartPanel.H-FinalH-buttonedgepadding, FinalW, FinalH);
						
						// TOP LOCATION
						
						 thisbutton.Value.Rect = new SharpDX.RectangleF(CY, 10, FinalW, FinalH);
						
						
						
						
	                    //CY = CY - widdd - space;
	                   // thisbutton.Value.Rect = new SharpDX.RectangleF(CY, space, widdd, pButtonSize);
	                   
//	                    if (xxx == "Break")
//							thisbutton.Value.Switch = pSignal3;
//						if (xxx == "First Bar")
//							thisbutton.Value.Switch = pSignal2;						
//						if (xxx == "Trend")
//							thisbutton.Value.Switch = pSignal1;
					if (xxx == "Arrows")
							thisbutton.Value.Switch = pArrowsEnabled;						


						RenderTarget.FillRectangle(thisbutton.Value.Rect, ChartBackgroundBrushDX);
	                    
	                    if (thisbutton.Value.Switch)
	                        RenderTarget.FillRectangle(thisbutton.Value.Rect, buttonFONBrushDX);
						else
							RenderTarget.FillRectangle(thisbutton.Value.Rect, buttonFOFFBrushDX);
						
						
	                    if (MouseIn(thisbutton.Value.Rect, 2, 2))
	                    {
	                        if (!thisbutton.Value.Switch)
	                            RenderTarget.FillRectangle(thisbutton.Value.Rect, buttonFHBrushDX);

	                        RenderTarget.DrawRectangle(thisbutton.Value.Rect, buttonHBrushDX, 3);

	                    }

	                    RenderTarget.DrawRectangle(thisbutton.Value.Rect, buttonBrushDX, 1);
	                    RenderTarget.DrawText(thisbutton.Value.Text, ButtonText, thisbutton.Value.Rect, buttonBrushDX);
						
						//thisbutton.Value.Rect = new SharpDX.RectangleF(CY-ChartPanel.X, space, FinalW, FinalH);
						
					}
					
					
					
                }

				
				
				

//Print("RenderTest 7");	
				
				
				
			//Print("bug1");
			
				
				
				
				
				
				
			if (ChartTextBrushDX != null) ChartTextBrushDX.Dispose();
      		if (ChartBackgroundBrushDX != null) ChartBackgroundBrushDX.Dispose();
			if (ChartBackgroundFadeBrushDX != null) ChartBackgroundFadeBrushDX.Dispose();
			if (ThisBrushDX != null) ThisBrushDX.Dispose();
							

            if (selectBrushDX != null) selectBrushDX.Dispose();

			
			//if (fillBrushDx != null) fillBrushDx.Dispose();
			
			
            if (textBrushDX2 != null) textBrushDX2.Dispose();
			if (textBrushDX != null) textBrushDX.Dispose();
            if (upBrushDX != null)  upBrushDX.Dispose();
			if (downBrushDX != null) downBrushDX.Dispose();
            if (finalBrushDX != null) finalBrushDX.Dispose();
			if (lineBrushDX != null) lineBrushDX.Dispose();
            if (longBrushDX != null) longBrushDX.Dispose();
			if (blackBrushDX != null) blackBrushDX.Dispose();		
			
			
			//	Print(Permission + " dgdadadlsl");
			
			
			if (buttonBrushDX != null) buttonBrushDX.Dispose();
            if (buttonHBrushDX != null)  buttonHBrushDX.Dispose();
			if (buttonFHBrushDX != null) buttonFHBrushDX.Dispose();
            if (buttonFOFFBrushDX != null) buttonFOFFBrushDX.Dispose();
			if (buttonFONBrushDX != null) buttonFONBrushDX.Dispose();	
				
				
			if (CellFormat != null) CellFormat.Dispose();
			if (CellLayout != null) CellLayout.Dispose();
			
			
				//Print("bug3");
			
			
			}
			//catch (Exception ex)
			{
				//if (TestRender) Print("OnRender DrawHUD: " + ThisMasterInstrument.Name + " " + ex.Message + " " + DateTime.Now.ToString("T"));
				
				
				//Log(Name + " OnRender Error: " + ex.Message, LogLevel.Information);
			}	
			
			
			
			
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
		
		
		
//		public override void OnCalculateMinMax()
//		{
			
				
//		}

		
				
		private void AdjustBarColor(bool isrt)
		{
			
//			ChartControl.Dispatcher.InvokeAsync((Action)(() =>
//			{
				
				
					
				if (pTradeTrack == "Signal")
				{
					SetColors2 (BarBrushes1i, CandleOutlineBrushes1i, CurrentBars[0], isrt);
				}
				else
				{
					
					if (pBarColorMode == "Normal")
					{
						SetColors2 (BarBrushes1i, CandleOutlineBrushes1i, CurrentBars[0], isrt);
					}
					else
					{
						SetColors2 (BarBrushes2i, CandleOutlineBrushes2i, CurrentBars[0], isrt);
					}				
				}
					
				if (!isrt)
					this.ChartControl.InvalidateVisual();
				
				
			//}));
		
			
			
			
			//TriggerCustomEvent(o =>
   			//	{
      				

					
			//	}, null);	
			
			
		}
		
		
	
    private void SetColors2 (Series<int> In, Series<int> Out, int BB, bool isrt)
        {
            int BTH = 0;

            BTH = CurrentBars[0] - 1;

            int st = 1;
            //if (!AllCOBC())
            st = 0;

			
			
            BTH = BB;

			if (isrt)
				st = BB;
			
			//Values[2].Reset();
			
            //Print(Values[0].Count);
            //Print(Values[0][0]);
            //Print(Values[0].GetValueAt(1));

//			//Print(st + "  " + BTH);
			
            for (int i = st; i <= BTH; i++)
            {
                
				//Out[i] = In[i];
				
				//Print(In.GetValueAt(CurrentBars[0] - i));
				
			//	Print(i);
					
//				BarBrushes.Set(i, Brushes.Beige);
//				CandleOutlineBrushes.Set(i, Brushes.Red);
				
				if (In.IsValidDataPointAt(i))
				{
					
					if (In.GetValueAt(i) == 1)
						BarBrushes.Set(i, UpBuyBrush);
					else if (In.GetValueAt(i) == 2)
						BarBrushes.Set(i, UpSellBrush);						
					else if (In.GetValueAt(i) == 3)
						BarBrushes.Set(i, UpNeutralBrush);						
					
					else if (In.GetValueAt(i) == -1)
						BarBrushes.Set(i, DownBuyBrush);
					else if (In.GetValueAt(i) == -2)
						BarBrushes.Set(i, DownSellBrush);						
					else if (In.GetValueAt(i) == -3)
						BarBrushes.Set(i, DownNeutralBrush);						
	
					//BarBrushes.Set(i, In.GetValueAt(i));
				}
				
				if (Out.IsValidDataPointAt(i))
				{
					if (Out.GetValueAt(i) == 1)
						CandleOutlineBrushes.Set(i, pColorUpBrush2);
					else if (Out.GetValueAt(i) == -1)
						CandleOutlineBrushes.Set(i, pColorDownBrush2);						
					else if (Out.GetValueAt(i) == 0)
						CandleOutlineBrushes.Set(i, pColorNeutralBrush2);	
					
					
					//CandleOutlineBrushes.Set(i, Out.GetValueAt(i));
				}
				
//				BarBrushes[i] = In.GetValueAt(CurrentBars[0] - i);
//				CandleOutlineBrushes[i] = Out.GetValueAt(CurrentBars[0] - i);
				
               // Values[0][i] = 0;

                //Print(i);

            }
            //ChartControl.Refresh();

        }	
	
		
		
		
		

		
		
		
		
		
		private SharpDX.Direct2D1.PathGeometry CreateArrowGeometry(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, int xt, double yt2, int dir)
		{
			

			Point startPoint			= new Point(0,0);
			Point midPoint				= new Point(100,0);
			Point endPoint 				= new Point(100,100);
				
			float aw = pArrowSize;
			float aw2 = pArrowBarWidth; // bar w
			float barh = pArrowBarHeight;
			float offset = pArrowOffset;

			float yt2f = (float) yt2;
			
			xt=xt-1; // adjust arrow to left 1 pixel so it lines up. bug?
			
			aw2 = Math.Min(aw2,aw);
			
			SharpDX.Vector2 tipPoint2			= new SharpDX.Vector2(0,0);
			SharpDX.Vector2 triLeftPoint2		= new SharpDX.Vector2(0,0);
			SharpDX.Vector2 triRightPoint2 		= new SharpDX.Vector2(0,0);
			SharpDX.Vector2 barLeftPoint12 		= new SharpDX.Vector2(0,0);
			SharpDX.Vector2 barRightPoint12 	= new SharpDX.Vector2(0,0);
			SharpDX.Vector2 barLeftPoint22 		= new SharpDX.Vector2(0,0);
			SharpDX.Vector2 barRightPoint22 	= new SharpDX.Vector2(0,0);

            float po = 0;

			if (dir == -1)
			{
				//yt = yt - offset;
				yt2f = yt2f - offset;

				tipPoint2 = new SharpDX.Vector2(xt,yt2f);
				triLeftPoint2 = new SharpDX.Vector2(xt-aw,yt2f-aw);
				triRightPoint2 = new SharpDX.Vector2(xt+aw,yt2f-aw);
                barLeftPoint12 = new SharpDX.Vector2(xt - aw2 + po, yt2f - aw);
                barRightPoint12 = new SharpDX.Vector2(xt + aw2 + po, yt2f - aw);
                barLeftPoint22 = new SharpDX.Vector2(xt - aw2 + po, yt2f - (aw + barh));
                barRightPoint22 = new SharpDX.Vector2(xt + aw2 + po, yt2f - (aw + barh));
			}
			else
			{
				//yt = yt + offset;
				yt2f = yt2f + offset;

				tipPoint2 = new SharpDX.Vector2(xt,yt2f);
				triLeftPoint2 = new SharpDX.Vector2(xt-aw,yt2f+aw);
				triRightPoint2 = new SharpDX.Vector2(xt+aw,yt2f+aw);
                barLeftPoint12 = new SharpDX.Vector2(xt - aw2 + po, yt2f + aw);
                barRightPoint12 = new SharpDX.Vector2(xt + aw2 + po, yt2f + aw);
                barLeftPoint22 = new SharpDX.Vector2(xt - aw2 + po, yt2f + (aw + barh));
                barRightPoint22 = new SharpDX.Vector2(xt + aw2 + po, yt2f + (aw + barh));
				
			}
			
			//Vector pixelAdjustVec		= new Vector(pixelAdjust, pixelAdjust);

			SharpDX.Direct2D1.PathGeometry pathGeometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
			SharpDX.Direct2D1.GeometrySink geometrySink = pathGeometry.Open();
			geometrySink.BeginFigure(tipPoint2, SharpDX.Direct2D1.FigureBegin.Filled);

			geometrySink.AddLines(new[] 
			{
//				startVec, midVec, 	// start -> mid,
//				midVec, endVec,		// mid -> top
//				endVec, startVec	// top -> start (cap it off)
			
				tipPoint2, triLeftPoint2,
				triLeftPoint2, barLeftPoint12,
				barLeftPoint12, barLeftPoint22,
				barLeftPoint22, barRightPoint22,
				barRightPoint22, barRightPoint12,
				barRightPoint12, triRightPoint2,
				triRightPoint2, tipPoint2 
			});
				
			geometrySink.EndFigure(SharpDX.Direct2D1.FigureEnd.Open);
			geometrySink.Close(); // calls dispose for you
			return pathGeometry;
			
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
		


//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public Series<double> SignalsOut
//        {
//            get { return Values[0]; }
//        }
		
//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public Series<double> MAnalyzer
//        {
//            get { return Values[3]; }
//        }	
		 
		
	
		
  	public override string FormatPriceMarker(double value)
		{
			
			return AllPriceMarker(value);
			
//			if (ChartControl == null)
//			{
//				return value.ToString();
				
//			}
//			else
//			{
//				return AllPriceMarker(value);
//			}
		
		
		}
		

		
		private string AllPriceMarker (double price)
		{
			
		//	Instrument ii = NinjaScriptBase.get_Instrument();
			
			if (Instrument == null)
				return price.ToString();
			
			double ThisTickSize = Instrument.MasterInstrument.TickSize;
			
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
		
		
		

		private bool pUseTimeFilter = false;
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Time Filter", Order = 0, Description = "")]
        public bool UseTimeFilter
        {
            get { return pUseTimeFilter; }
            set { pUseTimeFilter = value; }
        }
		
		private TimeSpan pStartTime = new TimeSpan(9,30,0);
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Start 1", GroupName = "Time Filter", Order = 1)]
		public string StartT
		{
			get { return pStartTime.Hours.ToString("0")+":"+pStartTime.Minutes.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pStartTime)) pStartTime=new TimeSpan(0,0,0); }
		}
				
		private TimeSpan pEndTime = new TimeSpan(16,0,0);
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time End 1", GroupName = "Time Filter", Order = 2)]
		public string EndT
		{
			get { return pEndTime.Hours.ToString("0")+":"+pEndTime.Minutes.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pEndTime)) pEndTime=new TimeSpan(0,0,0); }
		}			
		
		private TimeSpan pStartTime2 = new TimeSpan(0,00,0);
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Start 2", GroupName = "Time Filter", Order = 3)]
		public string StartT2
		{
			get { return pStartTime2.Hours.ToString("0")+":"+pStartTime2.Minutes.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pStartTime2)) pStartTime2=new TimeSpan(0,0,0); }
		}
				
		private TimeSpan pEndTime2 = new TimeSpan(0,00,0);
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time End 2", GroupName = "Time Filter", Order = 4)]
		public string EndT2
		{
			get { return pEndTime2.Hours.ToString("0")+":"+pEndTime2.Minutes.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pEndTime2)) pEndTime2=new TimeSpan(0,0,0); }
		}	
		
		private TimeSpan pStartTime3 = new TimeSpan(0,00,0);
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Start 3", GroupName = "Time Filter", Order = 5)]
		public string StartT3
		{
			get { return pStartTime3.Hours.ToString("0")+":"+pStartTime3.Minutes.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pStartTime3)) pStartTime3=new TimeSpan(0,0,0); }
		}
				
		private TimeSpan pEndTime3 = new TimeSpan(0,00,0);
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Time End 3", GroupName = "Time Filter", Order = 6)]
		public string EndT3
		{
			get { return pEndTime3.Hours.ToString("0")+":"+pEndTime3.Minutes.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pEndTime3)) pEndTime3=new TimeSpan(0,0,0); }
		}			
		
		private bool pUseEST = true;
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Eastern Standard Time", GroupName = "Time Filter", Description = "", Order = 10)]
        public bool UseEST
        {
            get { return pUseEST; }
            set { pUseEST = value; }
        }
				
		
		private TimeSpan pStartTimeZ = new TimeSpan(9,30,0);		
		private TimeSpan pEndTimeZ = new TimeSpan(16,0,0);
		private TimeSpan pStartTime2Z = new TimeSpan(0,00,0);	
		private TimeSpan pEndTime2Z = new TimeSpan(0,00,0);
		private TimeSpan pStartTime3Z = new TimeSpan(0,00,0);		
		private TimeSpan pEndTime3Z = new TimeSpan(0,00,0);

		
		
		private bool pSecondaryFeedsEnabled = false;
		[NinjaScriptProperty]
				[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Secondary Trend", Order = 1)]
        public bool SecondaryFeedsEnabled
        {
            get { return pSecondaryFeedsEnabled; }
            set { pSecondaryFeedsEnabled = value; }
        }	
		
		private bool pFeed1Enabled = true;
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 1 Enabled", Description = "", GroupName = "Secondary Trend", Order = 10)]
        public bool Feed1Enabled
        {
            get { return pFeed1Enabled; }
            set { pFeed1Enabled = value; }
        }	
		
		private bool pFeed2Enabled = true;
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 2 Enabled", Description = "", GroupName = "Secondary Trend", Order = 15)]
        public bool Feed2Enabled
        {
            get { return pFeed2Enabled; }
            set { pFeed2Enabled = value; }
        }			
		
		private bool pFeed3Enabled = true;
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 3 Enabled", Description = "", GroupName = "Secondary Trend", Order = 20)]
        public bool Feed3Enabled
        {
            get { return pFeed3Enabled; }
            set { pFeed3Enabled = value; }
        }	
		
		private bool pFeed4Enabled = true;
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 4 Enabled", Description = "", GroupName = "Secondary Trend", Order = 25)]
        public bool Feed4Enabled
        {
            get { return pFeed4Enabled; }
            set { pFeed4Enabled = value; }
        }			
		
		private bool pFeed1Included = true;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 1 Included", Description = "", GroupName = "Secondary Trend", Order = 10)]
        public bool Feed1Included
        {
            get { return pFeed1Included; }
            set { pFeed1Included = value; }
        }	
		
		private bool pFeed2Included = true;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 2 Included", Description = "", GroupName = "Secondary Trend", Order = 15)]
        public bool Feed2Included
        {
            get { return pFeed2Included; }
            set { pFeed2Included = value; }
        }			
		
		private bool pFeed3Included = false;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 3 Included", Description = "", GroupName = "Secondary Trend", Order = 20)]
        public bool Feed3Included
        {
            get { return pFeed3Included; }
            set { pFeed3Included = value; }
        }	
		
		private bool pFeed4Included = false;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend 4 Included", Description = "", GroupName = "Secondary Trend", Order = 25)]
        public bool Feed4Included
        {
            get { return pFeed4Included; }
            set { pFeed4Included = value; }
        }			
		
		
			private int	pEMAPeriod3 = 30;
			[NinjaScriptProperty]
			[Range(1, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), Name = "   Bars Period", Description = "", GroupName = "Secondary Trend", Order = 21)]
			public int EMAPeriod3
			{
				get { return pEMAPeriod3; }
				set { pEMAPeriod3 = value; }
			}	
			
			private int	pEMAPeriod4 = 60;
			[NinjaScriptProperty]
			[Range(1, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), Name = "   Bars Period", Description = "", GroupName = "Secondary Trend", Order = 26)]
			public int EMAPeriod4
			{
				get { return pEMAPeriod4; }
				set { pEMAPeriod4 = value; }
			}	
			
		private BarsPeriodType AcceptableBasePeriodType1 = BarsPeriodType.Minute;		
		private string pThisBarType1 = "Minute";
		[NinjaScriptProperty]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "   Bars Type", Description = "", GroupName = "Secondary Trend", Order = 12)]
		[TypeConverter(typeof(PTS))]
		public string ThisBarType1
		{
            get { return pThisBarType1; }
            set 
			{ 
				pThisBarType1 = value; 
			
				switch (pThisBarType1) 
				{
					case "Tick":   AcceptableBasePeriodType1 = BarsPeriodType.Tick; break;
					case "Volume":  AcceptableBasePeriodType1 = BarsPeriodType.Volume; break;
					case "Range": AcceptableBasePeriodType1 = BarsPeriodType.Range; break;
					case "Second": AcceptableBasePeriodType1 = BarsPeriodType.Second; break;
					case "Minute": AcceptableBasePeriodType1 = BarsPeriodType.Minute; break;
//					case "Renko": AcceptableBasePeriodType1 = BarsPeriodType.Renko; break;
//					case "Day": AcceptableBasePeriodType1 = BarsPeriodType.Day; break;
//					case "Week": AcceptableBasePeriodType1 = BarsPeriodType.Week; break;
//					case "Month": AcceptableBasePeriodType1 = BarsPeriodType.Month; break;						
						
				}	
				
			}
		}			
			
		private BarsPeriodType AcceptableBasePeriodType2 = BarsPeriodType.Minute;		
		private string pThisBarType2 = "Minute";
		[NinjaScriptProperty]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "   Bars Type", Description = "", GroupName = "Secondary Trend", Order = 17)]
		[TypeConverter(typeof(PTS))]
		public string ThisBarType2
		{
            get { return pThisBarType2; }
            set 
			{ 
				pThisBarType2 = value; 
			
				switch (pThisBarType2) 
				{
					case "Tick":   AcceptableBasePeriodType2 = BarsPeriodType.Tick; break;
					case "Volume":  AcceptableBasePeriodType2 = BarsPeriodType.Volume; break;
					case "Range": AcceptableBasePeriodType2 = BarsPeriodType.Range; break;
					case "Second": AcceptableBasePeriodType2 = BarsPeriodType.Second; break;
					case "Minute": AcceptableBasePeriodType2 = BarsPeriodType.Minute; break;
//					case "Renko": AcceptableBasePeriodType2 = BarsPeriodType.Renko; break;
//					case "Day": AcceptableBasePeriodType2 = BarsPeriodType.Day; break;
//					case "Week": AcceptableBasePeriodType2 = BarsPeriodType.Week; break;
//					case "Month": AcceptableBasePeriodType2 = BarsPeriodType.Month; break;						
						
				}	
				
			}
		}	
		
		private BarsPeriodType AcceptableBasePeriodType3 = BarsPeriodType.Minute;		
		private string pThisBarType3 = "Minute";
		[NinjaScriptProperty]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "   Bars Type", Description = "", GroupName = "Secondary Trend", Order = 22)]
		[TypeConverter(typeof(PTS))]
		public string ThisBarType3
		{
            get { return pThisBarType3; }
            set 
			{ 
				pThisBarType3 = value; 
			
				switch (pThisBarType3) 
				{
					case "Tick":   AcceptableBasePeriodType3 = BarsPeriodType.Tick; break;
					case "Volume":  AcceptableBasePeriodType3 = BarsPeriodType.Volume; break;
					case "Range": AcceptableBasePeriodType3 = BarsPeriodType.Range; break;
					case "Second": AcceptableBasePeriodType3 = BarsPeriodType.Second; break;
					case "Minute": AcceptableBasePeriodType3 = BarsPeriodType.Minute; break;
//					case "Renko": AcceptableBasePeriodType3 = BarsPeriodType.Renko; break;
//					case "Day": AcceptableBasePeriodType3 = BarsPeriodType.Day; break;
//					case "Week": AcceptableBasePeriodType3 = BarsPeriodType.Week; break;
//					case "Month": AcceptableBasePeriodType3 = BarsPeriodType.Month; break;						
						
				}	
				
			}
		}	
		
		
		private BarsPeriodType AcceptableBasePeriodType4 = BarsPeriodType.Minute;		
		private string pThisBarType4 = "Minute";
		[NinjaScriptProperty]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "   Bars Type", Description = "", GroupName = "Secondary Trend", Order = 27)]
		[TypeConverter(typeof(PTS))]
		public string ThisBarType4
		{
            get { return pThisBarType4; }
            set 
			{ 
				pThisBarType4 = value; 
			
				switch (pThisBarType4) 
				{
					case "Tick":   AcceptableBasePeriodType4 = BarsPeriodType.Tick; break;
					case "Volume":  AcceptableBasePeriodType4 = BarsPeriodType.Volume; break;
					case "Range": AcceptableBasePeriodType4 = BarsPeriodType.Range; break;
					case "Second": AcceptableBasePeriodType4 = BarsPeriodType.Second; break;
					case "Minute": AcceptableBasePeriodType4 = BarsPeriodType.Minute; break;
//					case "Renko": AcceptableBasePeriodType4 = BarsPeriodType.Renko; break;
//					case "Day": AcceptableBasePeriodType4 = BarsPeriodType.Day; break;
//					case "Week": AcceptableBasePeriodType4 = BarsPeriodType.Week; break;
//					case "Month": AcceptableBasePeriodType4 = BarsPeriodType.Month; break;						
						
				}	
				
			}
		}	
		
		
		
		

		internal class PTS : StringConverter
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
				//return new StandardValuesCollection( new String[] { "Tick", "Volume", "Range", "Second", "Minute", "Renko" } );
				return new StandardValuesCollection( new String[] { "Tick", "Volume", "Range", "Second", "Minute" } );
			}
		}	
		
		
		
		private bool pSecondaryFeedsDisplayEnabled = true;
				[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Secondary Trend Box Display", Order = 0)]
        public bool SecondaryFeedsDisplayEnabled
        {
            get { return pSecondaryFeedsDisplayEnabled; }
            set { pSecondaryFeedsDisplayEnabled = value; }
        }	
		
		
		
		
						[Display(Name="Text Font", Description="", GroupName= "Secondary Trend Box Display", Order = 1)]
			public SimpleFont TextFont
			{ get; set; }	
			
			
			private System.Windows.Media.Brush pColorTextBrush	= Brushes.Gainsboro;
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color", Description = "", GroupName =  "Secondary Trend Box Display", Order = 2)]
			public System.Windows.Media.Brush ColorTextBrush
			{
				get { return pColorTextBrush; } set { pColorTextBrush = value; }
			}
			[Browsable(false)]
			public string ColorTextBrushS
			{
				get { return Serialize.BrushToString(pColorTextBrush); } set { pColorTextBrush = Serialize.StringToBrush(value); }
			}	
		
			
//					internal class TotalMode : StringConverter
//		{
//			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
//			{
//			//true means show a combobox
//				return true;
//			}
			
//			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
//			{
//			//true will limit to list. false will show the list, but allow free-form entry
//				return true;
//			}
		
//			public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
//			{
//				return new StandardValuesCollection( new String[] {"Market", "Limit"} );
//			}
//		}	

//	private string pThisEntryType = "Market";
//		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Execution", Name = "Entry Type", Description = "",  Order = 8)]
//		[Description("")]
//		//[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(TotalMode))]
//		public string ThisEntryType
//		{
//			get { return pThisEntryType; }
//			set { pThisEntryType = value; }
//		}		
					
		
		
//			private LVVolume_TablePosition pTPosition = LVVolume_TablePosition.;
//			[NinjaScriptProperty]
//			[Display(Name = "Position", Description = "", GroupName =  "Secondary Trend Box Display", Order = 3)]
//			public LVVolume_TablePosition TPosition
//			{
//				get { return pTPosition; }
//				set { pTPosition = value; }
//			}	
			
			private int	pPixelsFromRight = 10;
			[Range(2, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Offset X (Pixels)", Description = "", GroupName =  "Secondary Trend Box Display", Order = 5)]
			public int PixelsFromRight
			{
				get { return pPixelsFromRight; }
				set { pPixelsFromRight = value; }
			}	
			
			private int	pPixelsFromBottom = 50;
			[Range(2, int.MaxValue)]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Offset Y (Pixels)", Description = "", GroupName =  "Secondary Trend Box Display", Order = 6)]
			public int PixelsFromBottom
			{
				get { return pPixelsFromBottom; }
				set { pPixelsFromBottom = value; }
			}		
			
			private int	pMarginB = 10;
//			[Range(0, int.MaxValue)]
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Margin (Pixels)", Description = "", GroupName =  "Secondary Trend Box Display", Order = 7)]
//			public int MarginB
//			{
//				get { return pMarginB; }
//				set { pMarginB = value; }
//			}					
			
			
//				private System.Windows.Media.Brush pColorTextBrush	= Brushes.Gainsboro;
//			[XmlIgnore]
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color", Description = "", GroupName =  "Secondary Trend Box Display", Order = 2)]
//			public System.Windows.Media.Brush ColorTextBrush
//			{
//				get { return pColorTextBrush; } set { pColorTextBrush = value; }
//			}
//			[Browsable(false)]
//			public string ColorTextBrushS
//			{
//				get { return Serialize.BrushToString(pColorTextBrush); } set { pColorTextBrush = Serialize.StringToBrush(value); }
//			}	
		
			
			
			
			private System.Windows.Media.Brush pFillUpBrush	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(3,128,0));
							
			
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Color Up", Description="", GroupName =  "Secondary Trend Box Display", Order = 8)]
			public System.Windows.Media.Brush FillUpBrush
			{
				get { return pFillUpBrush; } set { pFillUpBrush = value; }
			}
//			public System.Windows.Media.Brush FillUpBrush
//			{
//	            get { return pFillUpBrush; }
//	            set
//	            {
//	                pFillUpBrush = value;
//	                if (pFillUpBrush != null)
//	                {
//	                    if (pFillUpBrush.IsFrozen)
//	                        pFillUpBrush = pFillUpBrush.Clone();
//	                   // pFillUpBrush.Opacity = areaOpacity / 100d;
//	                    pFillUpBrush.Freeze();
//	                }
//	            }
//			}
			[Browsable(false)]
			public string FillUpBrushS
			{
				get { return Serialize.BrushToString(pFillUpBrush); } set { pFillUpBrush = Serialize.StringToBrush(value); }
			}	
			
				
			private System.Windows.Media.Brush pFillNeutralBrush	= Brushes.Silver;
//			[XmlIgnore]
//			[Display(ResourceType = typeof(Custom.Resource), Name = "Color Flash", GroupName =  "Secondary Trend Box Display", Order = 9)]
//			public System.Windows.Media.Brush FillNeutralBrush
//			{
//				get { return pFillNeutralBrush; } set { pFillNeutralBrush = value; }
//			}
//			[Browsable(false)]
//			public string FillNeutralBrushS
//			{
//				get { return Serialize.BrushToString(pFillNeutralBrush); } set { pFillNeutralBrush = Serialize.StringToBrush(value); }
//			}	
			
			private System.Windows.Media.Brush pFillDownBrush	= new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(204,0,0));
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Color Down", GroupName =  "Secondary Trend Box Display", Order = 10)]
			public System.Windows.Media.Brush FillDownBrush
			{
				get { return pFillDownBrush; } set { pFillDownBrush = value; }
			}
			[Browsable(false)]
			public string FillDownBrushS
			{
				get { return Serialize.BrushToString(pFillDownBrush); } set { pFillDownBrush = Serialize.StringToBrush(value); }
			}	
						
			private int	pFillOpacity = 80;
			[Range(0, 100)]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Fill Opacity (%)", Description = "", GroupName =  "Secondary Trend Box Display", Order = 11)]
			public int FillOpacity
			{
				get { return pFillOpacity; }
				set { pFillOpacity = value; }
			}	
			
			private int	pOutlineOpacity = 100;
			[Range(0, 100)]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Outline Opacity (%)", Description = "", GroupName =  "Secondary Trend Box Display", Order = 12)]
			public int OutlineOpacity
			{
				get { return pOutlineOpacity; }
				set { pOutlineOpacity = value; }
			}	
			

			private int	pColumnWidthP = 30;
			
			
		
        // INPUTS
		
			
			
		private string pTradeTrack = "Signal";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Execution", Name = "Trade Type", Description = "",  Order = 7)]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(EntryHandling2))]
		public string TradeTrack
		{
			get { return pTradeTrack; }
			set { pTradeTrack = value; }
		}	
		
		internal class EntryHandling2 : StringConverter
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
				return new StandardValuesCollection( new String[] { "Bar", "Signal"} );
			}
		}			
			
		private string pBarColorMode = "Normal";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Execution", Name = "Bar Color Type", Description = "",  Order = 7)]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(EntryHandling23))]
		public string BarColorMode
		{
			get { return pBarColorMode; }
			set { pBarColorMode = value; }
		}	
		
		internal class EntryHandling23 : StringConverter
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
				return new StandardValuesCollection( new String[] { "Normal", "Signals"} );
			}
		}			
		
		
			 
		private string pSignalMode = "T3";
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Execution", Name = "Signal Type", Description = "",  Order = 7)]
		//[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(EntryHandling2333))]
		public string SignalMode
		{
			get { return pSignalMode; }
			set { pSignalMode = value; }
		}	
		
		internal class EntryHandling2333 : StringConverter
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
				//return new StandardValuesCollection( new String[] { "EMA" ,"NonLagMA", "MACD", "System 1", "System 2", "System 3"} );
				
				return new StandardValuesCollection( new String[] { "EMA" ,"HMA"} );
				
			}
		}			

		
		
		
		
        private Brush areaBrush = Brushes.Transparent;
        private Brush textBrush = Brushes.Blue;
        //private Brush smallAreaBrush = Brushes.Red;
        private int areaOpacity = 50;
       // const float fontHeight = 30f;
	
        private Brush areaBrush2 = Brushes.Gray;
        
        private int area2Opacity = 60;		
		

        private bool pButtonsEnabled = false;
			[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Chart Buttons", Order = -1)]
        public bool ButtonsEnabled
        {
            get { return pButtonsEnabled; }
            set { pButtonsEnabled = value; }
        }
		
		
		

        private int pButtonSize = 20;
        [Range(1, int.MaxValue)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Size (Pixels)", GroupName = "Chart Buttons", Order = 1)]
        public int ButtonSize
        {
            get { return pButtonSize; }
            set { pButtonSize = value; }
        }

		

		
		
		
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "On Color", GroupName = "Chart Buttons", Order = 2)]
        public Brush AreaBrush
        {
            get { return areaBrush; }
            set
            {
                areaBrush = value;
                if (areaBrush != null)
                {
                    if (areaBrush.IsFrozen)
                        areaBrush = areaBrush.Clone();
                    areaBrush.Opacity = areaOpacity / 100d;
                    areaBrush.Freeze();
                }
            }
        }

        [Browsable(false)]
        public string AreaBrushSerialize
        {
            get { return Serialize.BrushToString(AreaBrush); }
            set { AreaBrush = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "On Opacity (%)", GroupName = "Chart Buttons", Order = 3)]
        public int AreaOpacity
        {
            get { return areaOpacity; }
            set
            {
                areaOpacity = Math.Max(0, Math.Min(100, value));
                if (areaBrush != null)
                {
                    Brush newBrush = areaBrush.Clone();
                    newBrush.Opacity = areaOpacity / 100d;
                    newBrush.Freeze();
                    areaBrush = newBrush;
                }
            }
        }
		
		
	
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Off Color", GroupName = "Chart Buttons", Order = 4)]
        public Brush AreaBrush2
        {
            get { return areaBrush2; }
            set
            {
                areaBrush2 = value;
                if (areaBrush2 != null)
                {
                    if (areaBrush.IsFrozen)
                        areaBrush2 = areaBrush2.Clone();
                    areaBrush2.Opacity = area2Opacity / 100d;
                    areaBrush2.Freeze();
                }
            }
        }

        [Browsable(false)]
        public string AreaBrushSerialize2
        {
            get { return Serialize.BrushToString(AreaBrush2); }
            set { AreaBrush2 = Serialize.StringToBrush(value); }
        }

        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Off Opacity (%)", GroupName = "Chart Buttons", Order = 5)]
        public int Area2Opacity
        {
            get { return area2Opacity; }
            set
            {
                area2Opacity = Math.Max(0, Math.Min(100, value));
                if (areaBrush2 != null)
                {
                    Brush newBrush = areaBrush2.Clone();
                    newBrush.Opacity = area2Opacity / 100d;
                    newBrush.Freeze();
                    areaBrush2 = newBrush;
                }
            }
        }

		
//       private bool pActiveButtonsEnabled = true;
//			[RefreshProperties(RefreshProperties.All)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Status Message Button Enabled", GroupName = "Chart Buttons", Order = 10)]
//		[Description("toggle display of the status message.")]
//        public bool ActiveButtonsEnabled
//        {
//            get { return pActiveButtonsEnabled; }
//            set { pActiveButtonsEnabled = value; }
//        }
				
       private bool pTradesButtonsEnabled = false;
			[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "TRADES Button Enabled", GroupName = "Chart Buttons", Order = 11)]
		[Description("toggle display of live trades.")]
        public bool TradesButtonsEnabled
        {
            get { return pTradesButtonsEnabled; }
            set { pTradesButtonsEnabled = value; }
        }
		
		
		
        private bool pHideMenuEnabled = false;
			[RefreshProperties(RefreshProperties.All)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Auto Hide Enabled", GroupName = "Chart Buttons", Order = 15)]
        public bool HideMenuEnabled
        {
            get { return pHideMenuEnabled; }
            set { pHideMenuEnabled = value; }
        }
		
		
		
		
       private bool pAllBOTSOffButtonEnabled = true;
//			[RefreshProperties(RefreshProperties.All)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "ALL BOTS OFF Button Enabled", GroupName = "Chart Buttons", Order = 12)]
//		[Description("one click turn off all bots")]
//        public bool AllBOTSOffButtonEnabled
//        {
//            get { return pAllBOTSOffButtonEnabled; }
//            set { pAllBOTSOffButtonEnabled = value; }
////        }		
		
		
		
       // [Description("number of ticks within a level to trigger an output for Market Analyer. The column will display positive numbers for R levels and negative numbers for S levels.  For example, if the row displays -2 for ES, then it means that ES  is close to the S2 level.")]

		
		
		
		
		
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
		
		
		
		
		
		// ARROW INPUTS

        private bool pArrowsEnabled = false;
//				[RefreshProperties(RefreshProperties.All)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Arrows", Order = 0)]
//        public bool ArrowsEnabled
//        {
//            get { return pArrowsEnabled; }
//            set { pArrowsEnabled = value; }
//        }
		
 		
        private float pArrowSize = 9;
        [Range(0, 1000)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Size", Description = "", GroupName = "Arrows", Order = 1)]
        public float ArrowSize
        {
            get { return pArrowSize; }
            set { pArrowSize = value; }
        }
		
        private float pArrowBarHeight = 0;
//        [Range(0, 1000)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Structure - Bar Height", Description = "", GroupName = "Arrows", Order = 2)]
//        public float ArrowBarHeight
//        {
//            get { return pArrowBarHeight; }
//            set { pArrowBarHeight = value; }
//        }
		
        private float pArrowBarWidth = 3;
//        [Range(0, 1000)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Structure - Bar Width", Description = "", GroupName = "Arrows", Order = 3)]
//        public float ArrowBarWidth
//        {
//            get { return pArrowBarWidth; }
//            set { pArrowBarWidth = value; }
//        }
		
		
		private float pArrowOffset = 12;
        [Range(0, 1000)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Offset (Pixels)", Description = "", GroupName = "Arrows", Order = 4)]
        public float ArrowOffset
        {
            get { return pArrowOffset; }
            set { pArrowOffset = value; }
        }



		private Brush pArrowUpFBrush	= Brushes.LimeGreen;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Fill Color", Description = "", GroupName = "Arrows", Order = 20)]
		public Brush ArrowUpFBrush
		{
			get { return pArrowUpFBrush; } set { pArrowUpFBrush = value; }
		}
		[Browsable(false)]
		public string ArrowUpFBrushS
		{
			get { return Serialize.BrushToString(pArrowUpFBrush); } set { pArrowUpFBrush = Serialize.StringToBrush(value); }
		}	

		private Brush pArrowDownFBrush	= Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Fill Color", Description = "", GroupName = "Arrows", Order = 22)]
		public Brush ArrowDownFBrush
		{
			get { return pArrowDownFBrush; } set { pArrowDownFBrush = value; }
		}
		[Browsable(false)]
		public string ArrowDownFBrushS
		{
			get { return Serialize.BrushToString(pArrowDownFBrush); } set { pArrowDownFBrush = Serialize.StringToBrush(value); }
		}			

        private Stroke pArrowUpStroke = new Stroke(Brushes.Green, DashStyleHelper.Solid, 1);
        [Display(ResourceType = typeof(Custom.Resource), Name = "Buy Outline Color", Description = "", GroupName = "Arrows", Order = 21)]
        public Stroke ArrowUpStroke
        {
            get { return pArrowUpStroke; }
            set { pArrowUpStroke = value; }
        }

        private Stroke pArrowDownStroke = new Stroke(Brushes.DarkRed, DashStyleHelper.Solid, 1);
        [Display(ResourceType = typeof(Custom.Resource), Name = "Sell Outline Color", Description = "", GroupName = "Arrows", Order = 23)]
        public Stroke ArrowDownStroke
        {
            get { return pArrowDownStroke; }
            set { pArrowDownStroke = value; }
        }
		
//		private Brush pArrowUpOBrush	= Brushes.DarkGreen;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Up (Outline)", Desciption = "", GroupName = "Arrows", Order = 20)]
//		public Brush ArrowUpOBrush
//		{
//			get { return pArrowUpOBrush; } set { pArrowUpOBrush = value; }
//		}
//		[Browsable(false)]
//		public string ArrowUpOBrushS
//		{
//			get { return Serialize.BrushToString(pArrowUpOBrush); } set { pArrowUpOBrush = Serialize.StringToBrush(value); }
//		}	

//		private Brush pArrowDownOBrush	= Brushes.DarkRed;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Down (Outline)", Desciption = "", GroupName = "Arrows", Order = 20)]
//		public Brush ArrowDownOBrush
//		{
//			get { return pArrowDownOBrush; } set { pArrowDownOBrush = value; }
//		}
//		[Browsable(false)]
//		public string ArrowDownOBrushS
//		{
//			get { return Serialize.BrushToString(pArrowDownOBrush); } set { pArrowDownOBrush = Serialize.StringToBrush(value); }
//		}	

		
		// LABELS		
		
        private bool pLabelsEnabled = true;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        [Display(ResourceType = typeof(Custom.Resource), Name = "Labels Enabled", Description = "", GroupName = "Arrows", Order = 51)]
        public bool LabelsEnabled
        {
            get { return pLabelsEnabled; }
            set { pLabelsEnabled = value; }
        }			
		
		private SimpleFont pTextFont = new SimpleFont("Arial", 11);
		[Display(ResourceType = typeof(Custom.Resource), Name = "Labels Font", Description = "", GroupName = "Arrows", Order = 52)]
		public SimpleFont TextFont4
        {
            get { return pTextFont; }
            set { pTextFont = value; }
        }	
		
		
		
			
		
		private int pActiveArrowOpacity = 100;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Arrows", Name = "Arrow Opacity (%)", Order = 100)]
        public int ActiveArrowOpacity
        {
            get { return pActiveArrowOpacity; }
            set { pActiveArrowOpacity = value; }
        }	
		
		private int pActiveLabelOpacity = 80;
        [Range(0, 100)]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Arrows", Name = "Label Opacity (%)", Order = 101)]
        public int ActiveLabelOpacity
        {
            get { return pActiveLabelOpacity; }
            set { pActiveLabelOpacity = value; }
        }			
		
	
		private int pNotActiveArrowOpacity = 15;
//        [Range(0, 100)]
//        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Arrows", Name = "Not Active Arrow Opacity (%)", Order = 200, Description = "")]
//		[Description("asfasdfasdsa")]
//        public int NotActiveArrowOpacity
//        {
//            get { return pNotActiveArrowOpacity; }
//            set { pNotActiveArrowOpacity = value; }
//        }	
		
		private int pNotActiveLabelOpacity = 15;
//        [Range(0, 100)]
//        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Arrows", Name = "Not Active Label Opacity (%)", Order = 201, Description = "")]
//		//[Description()]
//        public int NotActiveLabelOpacity
//        {
//            get { return pNotActiveLabelOpacity; }
//            set { pNotActiveLabelOpacity = value; }
//        }			
				
		
		private string pLabelBuy = "Buy";	
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Label", Description = "", GroupName = "Arrows", Order = 53)]
        public string LabelBuy
        {
            get { return pLabelBuy; }
            set { pLabelBuy = value; }
        }		
		
		private string pLabelSell = "Sell";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Label", Description = "", GroupName = "Arrows", Order = 54)]
        public string LabelSell
        {
            get { return pLabelSell; }
            set { pLabelSell = value; }
        }	

	
	
				
		
		
		private string pIsReferenced = "";
//		[NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Is Referenced", Description = "", GroupName = "License", Order = 100)]
//        public string IsReferenced
//        {
//            get { return pIsReferenced; }
//            set { pIsReferenced = value; }
//        }		
		
	
		
        private bool pBackSEnabled = false;
	//	[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Signal Enabled", Description = "", GroupName = "Background Color", Order = 10)]
//        public bool BackSEnabled
//        {
//            get { return pBackSEnabled; }
//            set { pBackSEnabled = value; }
//        }	
		
        private bool pBackTEnabled = true;
//	//	[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Trend Enabled", Description = "", GroupName = "Background Color", Order = 20)]
//        public bool BackTEnabled
//        {
//            get { return pBackTEnabled; }
//            set { pBackTEnabled = value; }
//        }	
		
        private bool pBackMEnabled = true;
	//	[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Time Filter Enabled", Description = "", GroupName = "Background Color", Order = 30)]
//        public bool BackMEnabled
//        {
//            get { return pBackMEnabled; }
//            set { pBackMEnabled = value; }
//        }	
		
		

		
		// BUY COLOR
		
		
		
		private Brush pBrush01	= Brushes.Green;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Signal Color", Description = "", GroupName = "Background Color", Order = 11)]
//		public Brush Brush01
//		{
//			get { return pBrush01; } set { pBrush01 = value; }
//		}
//		[Browsable(false)]
//		public string Brush01S
//		{
//			get { return Serialize.BrushToString(pBrush01); } set { pBrush01 = Serialize.StringToBrush(value); }
//		}	
		
		
//		private System.Windows.Media.Brush	pBrush01 = Brushes.Green;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Signal Color", Description = "", GroupName = "Background Color", Order = 11)]
//		public Brush Brush01
//		{
//			get { return pBrush01; }
//			set
//			{
//				pBrush01 = value;
//				if (pBrush01 != null)
//				{
//					if (pBrush01.IsFrozen)
//						pBrush01 = pBrush01.Clone();
//					pBrush01.Opacity = pOpacity01 / 100d;
//					pBrush01.Freeze();
//				}
//			}
//		}

//		[Browsable(false)]
//		public string Brush01S
//		{
//			get { return Serialize.BrushToString(Brush01); }
//			set { Brush01 = Serialize.StringToBrush(value); }
//		}

//		private int	pOpacity01 = 20;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Signal Opacity (%)", Description = "", GroupName = "Background Color", Order = 12)]
//		public int Opacity01
//		{
//			get { return pOpacity01; }
//			set
//			{
//				pOpacity01 = Math.Max(0, Math.Min(100, value));
//				if (pBrush01 != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrush01.Clone();
//					newBrush.Opacity	= pOpacity01 / 100d;
//					newBrush.Freeze();
//					pBrush01			= newBrush;
//				}
//			}
//		}
		
			
		// SELL COLOR
		
		private Brush pBrush02	= Brushes.Red;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Signal Color", Description = "", GroupName = "Background Color", Order = 13)]
//		public Brush Brush02
//		{
//			get { return pBrush02; } set { pBrush02 = value; }
//		}
//		[Browsable(false)]
//		public string Brush02S
//		{
//			get { return Serialize.BrushToString(pBrush02); } set { pBrush02 = Serialize.StringToBrush(value); }
//		}	
		
//		private System.Windows.Media.Brush	pBrush02 = Brushes.Red;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Signal Color", Description = "", GroupName = "Background Color", Order = 13)]
//		public Brush Brush02
//		{
//			get { return pBrush02; }
//			set
//			{
//				pBrush02 = value;
//				if (pBrush02 != null)
//				{
//					if (pBrush02.IsFrozen)
//						pBrush02 = pBrush02.Clone();
//					pBrush02.Opacity = pOpacity02 / 100d;
//					pBrush02.Freeze();
//				}
//			}
//		}

//		[Browsable(false)]
//		public string Brush02S
//		{
//			get { return Serialize.BrushToString(Brush02); }
//			set { Brush02 = Serialize.StringToBrush(value); }
//		}

//		private int	pOpacity02 = 20;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Signal Opacity (%)", Description = "", GroupName = "Background Color", Order = 14)]
//		public int Opacity02
//		{
//			get { return pOpacity02; }
//			set
//			{
//				pOpacity02 = Math.Max(0, Math.Min(100, value));
//				if (pBrush02 != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrush02.Clone();
//					newBrush.Opacity	= pOpacity02 / 100d;
//					newBrush.Freeze();
//					pBrush02			= newBrush;
//				}
//			}
//		}	
		


		
		// SESSION 7 COLOR
		
//		private Brush pBrush07	= Brushes.Green;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Long Trend Color", Description = "", GroupName = "Background Color", Order = 21)]
//		public Brush Brush07
//		{
//			get { return pBrush07; } set { pBrush07 = value; }
//		}
//		[Browsable(false)]
//		public string Brush07S
//		{
//			get { return Serialize.BrushToString(pBrush07); } set { pBrush07 = Serialize.StringToBrush(value); }
//		}	
		
	
		private System.Windows.Media.Brush	pBrush07 = Brushes.DodgerBlue;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Long Trend Color", Description = "", GroupName = "Background Color", Order = 21)]
//		public Brush Brush07
//		{
//			get { return pBrush07; }
//			set
//			{
//				pBrush07 = value;
//				if (pBrush07 != null)
//				{
//					if (pBrush07.IsFrozen)
//						pBrush07 = pBrush07.Clone();
//					pBrush07.Opacity = pOpacity07 / 100d;
//					pBrush07.Freeze();
//				}
//			}
//		}

//		[Browsable(false)]
//		public string Brush07S
//		{
//			get { return Serialize.BrushToString(Brush07); }
//			set { Brush07 = Serialize.StringToBrush(value); }
//		}

		private int	pOpacity07 = 10;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Long Trend Opacity (%)", Description = "", GroupName = "Background Color", Order = 22)]
//		public int Opacity07
//		{
//			get { return pOpacity07; }
//			set
//			{
//				pOpacity07 = Math.Max(0, Math.Min(100, value));
//				if (pBrush07 != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrush07.Clone();
//					newBrush.Opacity	= pOpacity07 / 100d;
//					newBrush.Freeze();
//					pBrush07			= newBrush;
//				}
//			}
//		}		
		

				
				
		// SESSION 8 COLOR
	
//		private Brush pBrush08	= Brushes.Red;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Short Trend Color", Description = "", GroupName = "Background Color", Order = 23)]
//		public Brush Brush08
//		{
//			get { return pBrush08; } set { pBrush08 = value; }
//		}
//		[Browsable(false)]
//		public string Brush08S
//		{
//			get { return Serialize.BrushToString(pBrush08); } set { pBrush08 = Serialize.StringToBrush(value); }
//		}	
		
		
		
		private System.Windows.Media.Brush	pBrush08 = Brushes.Red;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Short Trend Color", Description = "", GroupName = "Background Color", Order = 23)]
//		public Brush Brush08
//		{
//			get { return pBrush08; }
//			set
//			{
//				pBrush08 = value;
//				if (pBrush08 != null)
//				{
//					if (pBrush08.IsFrozen)
//						pBrush08 = pBrush08.Clone();
//					pBrush08.Opacity = pOpacity08 / 100d;
//					pBrush08.Freeze();
//				}
//			}
//		}

//		[Browsable(false)]
//		public string Brush08S
//		{
//			get { return Serialize.BrushToString(Brush08); }
//			set { Brush08 = Serialize.StringToBrush(value); }
//		}

		private int	pOpacity08 = 10;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Short Trend Opacity (%)", Description = "", GroupName = "Background Color", Order = 24)]
//		public int Opacity08
//		{
//			get { return pOpacity08; }
//			set
//			{
//				pOpacity08 = Math.Max(0, Math.Min(100, value));
//				if (pBrush08 != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrush08.Clone();
//					newBrush.Opacity	= pOpacity08 / 100d;
//					newBrush.Freeze();
//					pBrush08			= newBrush;
//				}
//			}
//		}		
		
		// SESSION 9 COLOR

		
		
	
//		private Brush pBrush09	= Brushes.Navy;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Filter Color", Description = "", GroupName = "Background Color", Order = 31)]
//		public Brush Brush09
//		{
//			get { return pBrush09; } set { pBrush09 = value; }
//		}
//		[Browsable(false)]
//		public string Brush09S
//		{
//			get { return Serialize.BrushToString(pBrush09); } set { pBrush09 = Serialize.StringToBrush(value); }
//		}	
				
		
		private System.Windows.Media.Brush	pBrush09 = Brushes.Navy;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Filter Color", Description = "", GroupName = "Background Color", Order = 31)]
//		public Brush Brush09
//		{
//			get { return pBrush09; }
//			set
//			{
//				pBrush09 = value;
//				if (pBrush09 != null)
//				{
//					if (pBrush09.IsFrozen)
//						pBrush09 = pBrush09.Clone();
//					pBrush09.Opacity = pOpacity09 / 100d;
//					pBrush09.Freeze();
//				}
//			}
//		}

//		[Browsable(false)]
//		public string Brush09S
//		{
//			get { return Serialize.BrushToString(Brush09); }
//			set { Brush09 = Serialize.StringToBrush(value); }
//		}

		private int	pOpacity09 = 10;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Time Filter Opacity (%)", Description = "", GroupName = "Background Color", Order = 32)]
//		public int Opacity09
//		{
//			get { return pOpacity09; }
//			set
//			{
//				pOpacity09 = Math.Max(0, Math.Min(100, value));
//				if (pBrush09 != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrush09.Clone();
//					newBrush.Opacity	= pOpacity09 / 100d;
//					newBrush.Freeze();
//					pBrush09			= newBrush;
//				}
//			}
//		}		
		
		// SESSION 10 COLOR
		
//		private System.Windows.Media.Brush	pBrush10 = Brushes.Orange;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Color", Description = "", GroupName = "Session 10", Order = 4)]
//		public Brush Brush10
//		{
//			get { return pBrush10; }
//			set
//			{
//				pBrush10 = value;
//				if (pBrush10 != null)
//				{
//					if (pBrush10.IsFrozen)
//						pBrush10 = pBrush10.Clone();
//					pBrush10.Opacity = pOpacity10 / 100d;
//					pBrush10.Freeze();
//				}
//			}
//		}

//		[Browsable(false)]
//		public string Brush10S
//		{
//			get { return Serialize.BrushToString(Brush10); }
//			set { Brush10 = Serialize.StringToBrush(value); }
//		}

//		private int	pOpacity10 = 10;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Opacity (%)", Description = "", GroupName = "Session 10", Order = 5)]
//		public int Opacity10
//		{
//			get { return pOpacity10; }
//			set
//			{
//				pOpacity10 = Math.Max(0, Math.Min(100, value));
//				if (pBrush10 != null)
//				{
//					System.Windows.Media.Brush newBrush		= pBrush10.Clone();
//					newBrush.Opacity	= pOpacity10 / 100d;
//					newBrush.Freeze();
//					pBrush10			= newBrush;
//				}
//			}
//		}		
		
		
		
		
		
		
		
		
		
		
		
		
		// EMAIL

		
//		private bool pEmailEnabled = false;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Email", Order = 1)]
//        public bool EmailEnabled
//        {
//            get { return pEmailEnabled; }
//            set { pEmailEnabled = value; }
//        }	
		
//		private bool pQuickEmail = true;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "IntraBar", GroupName = "Email", Order = 2)]
//        public bool QuickEmail
//        {
//            get { return pQuickEmail; }
//            set { pQuickEmail = value; }
//        }
		
		
//		private string pEmailAddress = @"";
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Email Address", GroupName = "Email", Order = 3)]
//        public string EmailAddress
//        {
//            get { return pEmailAddress; }
//            set { pEmailAddress = value; }
//        }
		
		
		
	
		
		
		private bool pVisualEnabled = true;
//		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Trigger Lines", Order = 3)]
//        public bool VisualEnabled
//        {
//            get { return pVisualEnabled; }
//            set { pVisualEnabled = value; }
//        }
				
		
//		private Brush pPlotUpFBrush	= Brushes.Lime;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Color", Description = "", GroupName = "Trigger Lines", Order = 20)]
//		public Brush PlotUpFBrush
//		{
//			get { return pPlotUpFBrush; } set { pPlotUpFBrush = value; }
//		}
//		[Browsable(false)]
//		public string PlotUpFBrushS
//		{
//			get { return Serialize.BrushToString(pPlotUpFBrush); } set { pPlotUpFBrush = Serialize.StringToBrush(value); }
//		}	

//		private Brush pPlotDownFBrush	= Brushes.Red;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Color", Description = "", GroupName = "Trigger Lines", Order = 22)]
//		public Brush PlotDownFBrush
//		{
//			get { return pPlotDownFBrush; } set { pPlotDownFBrush = value; }
//		}
//		[Browsable(false)]
//		public string PlotDownFBrushS
//		{
//			get { return Serialize.BrushToString(pPlotDownFBrush); } set { pPlotDownFBrush = Serialize.StringToBrush(value); }
//		}	

		
//		private int pWidth1 = 2;
//        [Range(1, int.MaxValue)]
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Width", GroupName = "Trigger Lines", Order = 30)]
//        public int Width1
//        {
//            get { return pWidth1; }
//            set { pWidth1 = value; }
//        }						
		
		
	
		
		
//		[Display(ResourceType = typeof(Custom.Resource), Name="Label Font", Description="", GroupName="Display", Order = 2)]
//		public SimpleFont TextFont
//		{ get; set; }	
		
		private int pRightPX = 0;
//		[Range(0, 1000)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Label X Offset", Description="in pixels.", GroupName = "Display", Order = 3)]
//		public int RightPX
//		{
//			get { return pRightPX; }
//			set { pRightPX= value; }
//		}	
		
		private int pShadowWidth = 0;
//		[Range(0, 100)]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Shadow Width", Description="in pixels.", GroupName = "Display", Order = 5)]
//		public int ShadowWidth
//		{
//			get { return pShadowWidth; }
//			set { pShadowWidth= value; }
//		}	
		
		
		private bool pIsLifeTime = true;
		
	
	
		
			
		private bool pAudioEnabled = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Audio", Order = 0)]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool AudioEnabled
        {
            get { return pAudioEnabled; }
            set { pAudioEnabled = value; }
        }	
		
		private bool pQuickAudio = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "IntraBar", GroupName = "Audio", Order = 1)]
        public bool QuickAudio
        {
            get { return pQuickAudio; }
            set { pQuickAudio = value; }
        }
		
		
		
	
		private string pWAVFileName = "Alert2.wav";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Bar WAV File", Description = "Sound file to play when a buy bar occurs.", GroupName = "Audio", Order = 2)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		public string WAVFileName
		{
			get { return pWAVFileName; }
			set { pWAVFileName = value; }
		}
		
		private string pWAVFileName2 = "Alert2.wav";
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Bar WAV File", Description = "Sound file to play when a sell bar occurs.", GroupName = "Audio", Order = 3)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		public string WAVFileName2
		{
			get { return pWAVFileName2; }
			set { pWAVFileName2 = value; }
		}
		
		private Brush pColorBuyBrush	= Brushes.Green;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Log Color", GroupName = "Audio", Order = 11)]
		public Brush ColorBuyBrush
		{
			get { return pColorBuyBrush; } set { pColorBuyBrush = value; }
		}
		[Browsable(false)]
		public string ColorBuyBrushS
		{
			get { return Serialize.BrushToString(pColorBuyBrush); } set { pColorBuyBrush = Serialize.StringToBrush(value); }
		}	
				
		private Brush pColorSellBrush	= Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Log Color", GroupName = "Audio", Order = 12)]
		public Brush ColorSellBrush
		{
			get { return pColorSellBrush; } set { pColorSellBrush = value; }
		}
		[Browsable(false)]
		public string ColorSellBrushS
		{
			get { return Serialize.BrushToString(pColorSellBrush); } set { pColorSellBrush = Serialize.StringToBrush(value); }
		}	
		
		// BACKGROUND
		
        private bool pBackEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Background Color", Order = 1)]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool BackEnabled
        {
            get { return pBackEnabled; }
            set { pBackEnabled = value; }
        }	
	
        private bool pColorAll = true;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Color All", Description = "", GroupName = "Background Color", Order = 2)]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool ColorAll
        {
            get { return pColorAll; }
            set { pColorAll = value; }
        }		
		
       
		
		private bool pBackSignalEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Signal Enabled", Description = "", GroupName = "Background Color", Order = 3)]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool BackSignalEnabled
        {
            get { return pBackSignalEnabled; }
            set { pBackSignalEnabled = value; }
        }			
		
		
		
		
		// BUY COLOR
		
		private System.Windows.Media.Brush	pBrushBack01 = Brushes.DodgerBlue;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Color", Description = "", GroupName = "Background Color", Order = 13)]
		public Brush BrushBack01
		{
			get { return pBrushBack01; }
			set
			{
				pBrushBack01 = value;
				if (pBrushBack01 != null)
				{
					if (pBrushBack01.IsFrozen)
						pBrushBack01 = pBrushBack01.Clone();
					pBrushBack01.Opacity = pOpacity01 / 100d;
					pBrushBack01.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string BrushBack01S
		{
			get { return Serialize.BrushToString(BrushBack01); }
			set { BrushBack01 = Serialize.StringToBrush(value); }
		}

		private int	pOpacity01 = 10;
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Opacity (%)", Description = "", GroupName = "Background Color", Order = 14)]
		public int Opacity01
		{
			get { return pOpacity01; }
			set
			{
				pOpacity01 = Math.Max(0, Math.Min(100, value));
				if (pBrushBack01 != null)
				{
					System.Windows.Media.Brush newBrush		= pBrushBack01.Clone();
					newBrush.Opacity	= pOpacity01 / 100d;
					newBrush.Freeze();
					pBrushBack01			= newBrush;
				}
			}
		}
		
		// SELL COLOR
		
		private System.Windows.Media.Brush	pBrushBack02 = Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Color", Description = "", GroupName = "Background Color", Order = 15)]
		public Brush BrushBack02
		{
			get { return pBrushBack02; }
			set
			{
				pBrushBack02 = value;
				if (pBrushBack02 != null)
				{
					if (pBrushBack02.IsFrozen)
						pBrushBack02 = pBrushBack02.Clone();
					pBrushBack02.Opacity = pOpacity02 / 100d;
					pBrushBack02.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string BrushBack02S
		{
			get { return Serialize.BrushToString(BrushBack02); }
			set { BrushBack02 = Serialize.StringToBrush(value); }
		}

		private int	pOpacity02 = 10;
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Opacity (%)", Description = "", GroupName = "Background Color", Order = 16)]
		public int Opacity02
		{
			get { return pOpacity02; }
			set
			{
				pOpacity02 = Math.Max(0, Math.Min(100, value));
				if (pBrushBack02 != null)
				{
					System.Windows.Media.Brush newBrush		= pBrushBack02.Clone();
					newBrush.Opacity	= pOpacity02 / 100d;
					newBrush.Freeze();
					pBrushBack02			= newBrush;
				}
			}
		}	
		
		// SELL COLOR
		
		private System.Windows.Media.Brush	pBrushBack03 = Brushes.DimGray;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral Color", Description = "", GroupName = "Background Color", Order = 17)]
		public Brush BrushBack03
		{
			get { return pBrushBack03; }
			set
			{
				pBrushBack03 = value;
				if (pBrushBack03 != null)
				{
					if (pBrushBack03.IsFrozen)
						pBrushBack03 = pBrushBack03.Clone();
					pBrushBack03.Opacity = pOpacity03 / 100d;
					pBrushBack03.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string BrushBack03S
		{
			get { return Serialize.BrushToString(BrushBack03); }
			set { BrushBack03 = Serialize.StringToBrush(value); }
		}

		private int	pOpacity03 = 10;
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral Opacity (%)", Description = "", GroupName = "Background Color", Order = 18)]
		public int Opacity03
		{
			get { return pOpacity03; }
			set
			{
				pOpacity03 = Math.Max(0, Math.Min(100, value));
				if (pBrushBack03 != null)
				{
					System.Windows.Media.Brush newBrush		= pBrushBack03.Clone();
					newBrush.Opacity	= pOpacity03 / 100d;
					newBrush.Freeze();
					pBrushBack03			= newBrush;
				}
			}
		}	
		
	
        private bool pBackTrendEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Trend Enabled", Description = "", GroupName = "Background Color", Order = 31)]
				[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool BackTrendEnabled
        {
            get { return pBackTrendEnabled; }
            set { pBackTrendEnabled = value; }
        }	

		
		// SELL COLOR
		
		private System.Windows.Media.Brush	pBrushBack04 = Brushes.DodgerBlue;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend Up Color", Description = "", GroupName = "Background Color", Order = 32)]
		public Brush BrushBack04
		{
			get { return pBrushBack04; }
			set
			{
				pBrushBack04 = value;
				if (pBrushBack04 != null)
				{
					if (pBrushBack04.IsFrozen)
						pBrushBack04 = pBrushBack04.Clone();
					pBrushBack04.Opacity = pOpacity04 / 100d;
					pBrushBack04.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string BrushBack04S
		{
			get { return Serialize.BrushToString(BrushBack04); }
			set { BrushBack04 = Serialize.StringToBrush(value); }
		}

		private int	pOpacity04 = 10;
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend Up Opacity (%)", Description = "", GroupName = "Background Color", Order = 33)]
		public int Opacity04
		{
			get { return pOpacity04; }
			set
			{
				pOpacity04 = Math.Max(0, Math.Min(100, value));
				if (pBrushBack04 != null)
				{
					System.Windows.Media.Brush newBrush		= pBrushBack04.Clone();
					newBrush.Opacity	= pOpacity04 / 100d;
					newBrush.Freeze();
					pBrushBack04			= newBrush;
				}
			}
		}	
		
		
		
		// SELL COLOR
		
		private System.Windows.Media.Brush	pBrushBack05 = Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend Down Color", Description = "", GroupName = "Background Color", Order = 34)]
		public Brush BrushBack05
		{
			get { return pBrushBack05; }
			set
			{
				pBrushBack05 = value;
				if (pBrushBack05 != null)
				{
					if (pBrushBack05.IsFrozen)
						pBrushBack05 = pBrushBack05.Clone();
					pBrushBack05.Opacity = pOpacity05 / 100d;
					pBrushBack05.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string BrushBack05S
		{
			get { return Serialize.BrushToString(BrushBack05); }
			set { BrushBack05 = Serialize.StringToBrush(value); }
		}

		private int	pOpacity05 = 10;
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trend Down Opacity (%)", Description = "", GroupName = "Background Color", Order = 35)]
		public int Opacity05
		{
			get { return pOpacity05; }
			set
			{
				pOpacity05 = Math.Max(0, Math.Min(100, value));
				if (pBrushBack05 != null)
				{
					System.Windows.Media.Brush newBrush		= pBrushBack05.Clone();
					newBrush.Opacity	= pOpacity05 / 100d;
					newBrush.Freeze();
					pBrushBack05			= newBrush;
				}
			}
		}	
		
		
		
		
		
        private bool pBarColorEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Bar Color", Order = -10)]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool BarColorEnabled
        {
            get { return pBarColorEnabled; }
            set { pBarColorEnabled = value; }
        }	
		

		
        private bool pBarColorFEnabled = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Fill Enabled", Description = "", GroupName = "Bar Color", Order = -9)]
//		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//        public bool BarColorFEnabled
//        {
//            get { return pBarColorFEnabled; }
//            set { pBarColorFEnabled = value; }
//        }	

		
        private bool pBarColorOEnabled = true;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Outline Enabled", Description = "", GroupName = "Bar Color", Order = -8)]
//		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
//        public bool BarColorOEnabled
//        {
//            get { return pBarColorOEnabled; }
//            set { pBarColorOEnabled = value; }
//        }	
		
		
		

		
		private int pBarOpacityUp = 20;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Bar Opacity (%)", GroupName = "Bar Color", Order = 40)]
//		[Range(0, 100)]
//		public int BarOpacityUp
//		{
//			get { return pBarOpacityUp; }
//			set { pBarOpacityUp = value; }
//		}			
		
		
		private int pBarOpacityDown = 70;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Fill Opacity (%)", GroupName = "Bar Color", Order = 41)]
//		[Range(0, 100)]
//		public int BarOpacityDown
//		{
//			get { return pBarOpacityDown; }
//			set { pBarOpacityDown = value; }
//		}					
		
		private int pBarOpacitySignal = 80;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal Opacity (%)", GroupName = "Bar Color", Order = 42)]
//		[Range(0, 100)]
//		public int BarOpacitySignal
//		{
//			get { return pBarOpacitySignal; }
//			set { pBarOpacitySignal = value; }
//		}				
		
		
        private bool pUseTP = false;
//        [Display(ResourceType = typeof(Custom.Resource), Name = "Display Ticks / Pips", Description = "", GroupName = "NinjaScriptParameters", Order = 3)]
//        public bool UseTP
//        {
//            get { return pUseTP; }
//            set { pUseTP = value; }
//        }
		
		private int pMinimumSizeTicks = 10;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Minimum Bar Size (Ticks)", GroupName = "Parameters  ", Order = 1)]
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		public int MinimumSizeTicks
//		{
//			get { return pMinimumSizeTicks; }
//			set { pMinimumSizeTicks = value; }
//		}	
		
		
	
	
		private bool pDotsEnabled = false;
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", GroupName = "Bar Dots", Order = 0, Description = "plot dots in conjuction with bar color.")]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        public bool DotsEnabled
        {
            get { return pDotsEnabled; }
            set { pDotsEnabled = value; }
        }	
		
		private int pDotSize = 16;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Size", GroupName = "Bar Dots", Order = 2)]
		[Range(1, int.MaxValue)]
		public int DotSize
		{
			get { return pDotSize; }
			set { pDotSize = value; }
		}	




		
		private Brush pDotUpBrush	= Brushes.DodgerBlue;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Color", GroupName = "Bar Dots", Order = 4)]
		public Brush DotUpBrush
		{
			get { return pDotUpBrush; } set { pDotUpBrush = value; }
		}
		[Browsable(false)]
		public string DotUpBrushS
		{
			get { return Serialize.BrushToString(pDotUpBrush); } set { pDotUpBrush = Serialize.StringToBrush(value); }
		}	
		
		private Brush pDotDownBrush	= Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Color", GroupName = "Bar Dots", Order = 5)]
		public Brush DotDownBrush
		{
			get { return pDotDownBrush; } set { pDotDownBrush = value; }
		}
		[Browsable(false)]
		public string DotDownBrushS
		{
			get { return Serialize.BrushToString(pDotDownBrush); } set { pDotDownBrush = Serialize.StringToBrush(value); }
		}	
		
				
		private int pDotOffset = 10;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Offset (Pixels)", GroupName = "Bar Dots", Order = 10)]
		[Range(1, int.MaxValue)]
		public int DotOffset
		{
			get { return pDotOffset; }
			set { pDotOffset = value; }
		}			
		
		
		
		
		
		
		
		
//		private int pDotOffset = 0;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Offset", GroupName = "Bar Dots", Order = 3)]
//		[Range(int.MinValue, int.MaxValue)]
//		public int DotOffset
//		{
//			get { return pDotOffset; }
//			set { pDotOffset = value; }
//		}	
		
	
	    private bool pBarColorHEnabled = true;
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
        [Display(ResourceType = typeof(Custom.Resource), Name = "Enabled", Description = "", GroupName = "Bar Highlight", Order = -100)]
        public bool BarColorHEnabled
        {
            get { return pBarColorHEnabled; }
            set { pBarColorHEnabled = value; }
        }	
		
		
		
		private int pHighlightSize = 5;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Size (Pixels)", GroupName = "Bar Highlight", Order = 2)]
		[Range(1, int.MaxValue)]
		public int HighlightSize
		{
			get { return pHighlightSize; }
			set { pHighlightSize = value; }
		}	
				
		private Brush pColorFlatBrush	= Brushes.DimGray;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Color", GroupName = "Bar Highlight", Order = 3)]
		public Brush ColorFlatBrush
		{
			get { return pColorFlatBrush; } set { pColorFlatBrush = value; }
		}
		[Browsable(false)]
		public string ColorFlatBrushS
		{
			get { return Serialize.BrushToString(pColorFlatBrush); } set { pColorFlatBrush = Serialize.StringToBrush(value); }
		}				
		
		
		private Brush pColorFlat2Brush	= Brushes.DimGray;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Color", GroupName = "Bar Highlight", Order = 3)]
		public Brush ColorFlat2Brush
		{
			get { return pColorFlat2Brush; } set { pColorFlat2Brush = value; }
		}
		[Browsable(false)]
		public string ColorFlat2BrushS
		{
			get { return Serialize.BrushToString(pColorFlat2Brush); } set { pColorFlat2Brush = Serialize.StringToBrush(value); }
		}				
		
		
		
		private int pOpacityR = 50;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Opacity (%)", GroupName = "Bar Highlight", Order = 4)]
		[Range(0, 100)]
		public int OpacityR
		{
			get { return pOpacityR; }
			set { pOpacityR = value; }
		}			
		
		
		
		
		
		
		
		
		
//		private int pEMAPeriod1 = 20;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast Period", GroupName = "EMA", Description="", Order = 3)]
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		public int EMAPeriod1
//		{
//			get { return pEMAPeriod1; }
//			set { pEMAPeriod1 = value; }
//		}			

//		private int pEMAPeriod2 = 50; 
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow Period", GroupName = "EMA", Description="", Order = 4)]
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		public int EMAPeriod2
//		{
//			get { return pEMAPeriod2; }
//			set { pEMAPeriod2 = value; }
//		}					
	
		
		
		private int pHMAPeriod1 = 20;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "HMA Fast Period", GroupName = "Moving Averages", Description="", Order = 5)]
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		public int HMAPeriod1
//		{
//			get { return pHMAPeriod1; }
//			set { pHMAPeriod1 = value; }
//		}			

		private int pHMAPeriod2 = 50; 
//		[Display(ResourceType = typeof(Custom.Resource), Name = "HMA Slow Period", GroupName = "Moving Averages", Description="", Order = 6)]
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		public int HMAPeriod2
//		{
//			get { return pHMAPeriod2; }
//			set { pHMAPeriod2 = value; }
//		}	
		
		
		
	
		
		private double pPercentQ = 10;
//		[Display(ResourceType = typeof(Custom.Resource), Name = "From High / Low (%)", GroupName = "Parameters", Description="", Order = 3)]
//		[Range(0, 100), NinjaScriptProperty]
//		public double PercentQ
//		{
//			get { return pPercentQ; }
//			set { pPercentQ = value; }
//		}
				
		
		
//iMACDBBLINES = MACDBBLINES(36,78,27,Brushes.Black,Brushes.Black,false,1,27,1,Brushes.Black,false,Brushes.Black,Brushes.Black);
		
//			private int	pMACDFast = 36;
//			[NinjaScriptProperty]
//			[Range(1, int.MaxValue)]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "MACD BB", Name = "MACD Fast", Description = "", Order = 1)]
//			public int MACDFast
//			{
//				get { return pMACDFast; }
//				set { pMACDFast = value; }
//			}	
			
//			private int	pMACDSlow = 78;
//			[NinjaScriptProperty]
//			[Range(1, int.MaxValue)]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "MACD BB", Name = "MACD Slow", Description = "", Order = 2)]
//			public int MACDSlow
//			{
//				get { return pMACDSlow; }
//				set { pMACDSlow = value; }
//			}	
			
//			private int	pMACDSmooth = 27;
//			[NinjaScriptProperty]
//			[Range(1, int.MaxValue)]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "MACD BB", Name = "MACD Smooth", Description = "", Order = 3)]
//			public int MACDSmooth
//			{
//				get { return pMACDSmooth; }
//				set { pMACDSmooth = value; }
//			}				
	
//			private double	pMACDDevFactor = 1;
//			[NinjaScriptProperty]
//			[Range(0, double.MaxValue)]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "MACD BB", Name = "MACD Dev Factor", Description = "", Order = 4)]
//			public double MACDDevFactor
//			{
//				get { return pMACDDevFactor; }
//				set { pMACDDevFactor = value; }
//			}			
			
//			private int	pMACDDevPeriod = 27;
//			[NinjaScriptProperty]
//			[Range(1, int.MaxValue)]
//			[Display(ResourceType = typeof(Custom.Resource), GroupName = "MACD BB", Name = "MACD Dev Period", Description = "", Order = 5)]
//			public int MACDDevPeriod
//			{
//				get { return pMACDDevPeriod; }
//				set { pMACDDevPeriod = value; }
//			}							

			
			
			
			
			
			private Brush pColorUpBrush	= Brushes.Green;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Up (Fill)", GroupName = "Bar Color", Order = 1)]
		public Brush ColorUpBrush
		{
			get { return pColorUpBrush; } set { pColorUpBrush = value; }
		}
		[Browsable(false)]
		public string ColorUpBrushS
		{
			get { return Serialize.BrushToString(pColorUpBrush); } set { pColorUpBrush = Serialize.StringToBrush(value); }
		}	
		
		private Brush pColorNeutralBrush	= Brushes.White;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Neutral (Fill)", GroupName = "Parameters", Order = 3)]
//		public Brush ColorNeutralBrush
//		{
//			get { return pColorNeutralBrush; } set { pColorNeutralBrush = value; }
//		}
//		[Browsable(false)]
//		public string ColorNeutralBrushS
//		{
//			get { return Serialize.BrushToString(pColorNeutralBrush); } set { pColorNeutralBrush = Serialize.StringToBrush(value); }
//		}	
		
		private Brush pColorDownBrush	= Brushes.DarkRed;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Down (Fill)", GroupName = "Bar Color", Order = 5)]
		public Brush ColorDownBrush
		{
			get { return pColorDownBrush; } set { pColorDownBrush = value; }
		}
		[Browsable(false)]
		public string ColorDownBrushS
		{
			get { return Serialize.BrushToString(pColorDownBrush); } set { pColorDownBrush = Serialize.StringToBrush(value); }
		}	

		
		
		
		
		private Brush pColorUpBrush2	= Brushes.LimeGreen;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Up (Outline)", GroupName = "Bar Color", Order = 2)]
		public Brush ColorUpBrush2
		{
			get { return pColorUpBrush2; } set { pColorUpBrush2 = value; }
		}
		[Browsable(false)]
		public string ColorUpBrush2S
		{
			get { return Serialize.BrushToString(pColorUpBrush2); } set { pColorUpBrush2 = Serialize.StringToBrush(value); }
		}	
		
		private Brush pColorNeutralBrush2	= Brushes.White;
//		[XmlIgnore]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Neutral (Outline)", GroupName = "Parameters", Order = 4)]
//		public Brush ColorNeutralBrush2
//		{
//			get { return pColorNeutralBrush2; } set { pColorNeutralBrush2 = value; }
//		}
//		[Browsable(false)]
//		public string ColorNeutralBrush2S
//		{
//			get { return Serialize.BrushToString(pColorNeutralBrush2); } set { pColorNeutralBrush2 = Serialize.StringToBrush(value); }
//		}	
		
		private Brush pColorDownBrush2	= Brushes.Red;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Down (Outline)", GroupName = "Bar Color", Order = 6)]
		public Brush ColorDownBrush2
		{
			get { return pColorDownBrush2; } set { pColorDownBrush2 = value; }
		}
		[Browsable(false)]
		public string ColorDownBrush2S
		{
			get { return Serialize.BrushToString(pColorDownBrush2); } set { pColorDownBrush2 = Serialize.StringToBrush(value); }
		}	
		
		
		
		
		
	}
	
	
	
	
		// Hide UserDefinedValues properties when not in use by the HLCCalculationMode.UserDefinedValues
	// When creating a custom type converter for indicators it must inherit from NinjaTrader.NinjaScript.IndicatorBaseConverter to work correctly with indicators
	
	
	
	public class SignalsCraigPritchertConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			SignalsCraigPritchert   jbb = (SignalsCraigPritchert) value;
			
			//Pivots						thisPivotsInstance			= (Pivots) value;
			
			//bool MagnetsOn = ;
			
			List<string> DeleteThese = new List<string>();
			List<string> DeleteThese2 = new List<string>();
			
			
			DeleteThese.Add("EXYES");

	
			
			
			DeleteThese.Add("IsLifeTime");
			
				DeleteThese.Add("EntriesEnabled");
				DeleteThese.Add("LongEnabled");
				DeleteThese.Add("ShortEnabled");
			DeleteThese.Add("TrendOnlyEnabled");	
			
				DeleteThese.Add("AutoEnabled");	
			
			DeleteThese.Add("SLTrailOrdersEnabled");	
			DeleteThese.Add("ExitOrdersEnabled");	
			
			

				DeleteThese.Add("BrushBack03");
				DeleteThese.Add("Opacity03");
			
			
			
			if (!jbb.BarColorHEnabled)
			{
		
				//DeleteThese.Add("BarColorHEnabled");
				DeleteThese.Add("HighlightSize");
				DeleteThese.Add("ColorFlatBrush");		
				DeleteThese.Add("ColorFlat2Brush");
				DeleteThese.Add("OpacityR");		
		
			}
			
			
			
			
		DeleteThese.Add("UseTimeFilter");	
	if (!jbb.UseTimeFilter)
			{			
				DeleteThese.Add("StartT");
				DeleteThese.Add("EndT");
				DeleteThese.Add("StartT2");
				DeleteThese.Add("EndT2");
				DeleteThese.Add("StartT3");
				DeleteThese.Add("EndT3");
				DeleteThese.Add("UseEST");				
			
		
		
			}			
			
			
			//DeleteThese.Add("AllTargetsEnabled");	
			
			
		
	//DeleteThese.Add("UseTimeFilter");	
			
			
//			if (!jbb.VisualEnabled)
//			{			
//				DeleteThese.Add("PlotUpFBrush");
//				DeleteThese.Add("PlotDownFBrush");
//				DeleteThese.Add("Width1");
		
		
//			}		
			
	
			
//			if (!jbb.IsLifeTime)
//				DeleteThese.Add("CuEnabled");
				
			
			
			
			
			
			
		
			
			

			
				DeleteThese.Add("Feed1Included");
				DeleteThese.Add("Feed2Included");
				DeleteThese.Add("Feed3Included");			
				DeleteThese.Add("Feed4Included");	
			
			
			DeleteThese.Add("SecondaryFeedsEnabled");	
		    if (!jbb.SecondaryFeedsEnabled)
			{	
//				DeleteThese.Add("EMAPeriod1");
//				DeleteThese.Add("EMAPeriod2");
				DeleteThese.Add("EMAPeriod3");
				DeleteThese.Add("EMAPeriod4");
				
				DeleteThese.Add("ThisBarType1");
				DeleteThese.Add("ThisBarType2");
				DeleteThese.Add("ThisBarType3");
				DeleteThese.Add("ThisBarType4");				
				
				DeleteThese.Add("Feed1Enabled");
				DeleteThese.Add("Feed2Enabled");
				DeleteThese.Add("Feed3Enabled");			
				DeleteThese.Add("Feed4Enabled");	
				
				DeleteThese.Add("SecondaryFeedsDisplayEnabled");
				
			}	
			
			DeleteThese.Add("IsReferenced");
			
			
		    if (!jbb.Feed1Enabled)
			{				
				DeleteThese.Add("EMAPeriod1");
				DeleteThese.Add("ThisBarType1");
			}
			
		    if (!jbb.Feed2Enabled)
			{				
				DeleteThese.Add("EMAPeriod2");
				DeleteThese.Add("ThisBarType2");
			}
			
		    if (!jbb.Feed3Enabled)
			{				
				DeleteThese.Add("EMAPeriod3");
				DeleteThese.Add("ThisBarType3");
			}	
			
		    if (!jbb.Feed4Enabled)
			{				
				DeleteThese.Add("EMAPeriod4");
				DeleteThese.Add("ThisBarType4");
			}
			
		    if (!jbb.SecondaryFeedsEnabled || !jbb.SecondaryFeedsDisplayEnabled)
			{	
				DeleteThese.Add("TextFont");
				DeleteThese.Add("ColorTextBrush");
				DeleteThese.Add("TPosition");
				DeleteThese.Add("PixelsFromRight");
				DeleteThese.Add("PixelsFromBottom");
				DeleteThese.Add("MarginB");
				DeleteThese.Add("FillUpBrush");
				DeleteThese.Add("FillNeutralBrush");
				DeleteThese.Add("FillDownBrush");
				DeleteThese.Add("FillOpacity");
				DeleteThese.Add("OutlineOpacity");
			
			}				
			
		
			
				DeleteThese.Add("SignalMode");
			
			
			
				DeleteThese.Add("BarColorMode");
			
			
			
			
				DeleteThese.Add("TradeTrack");					

	

			
		
	
	
	
		
		
			//if (!jbb.ArrowsEnabled)
			{			
				//DeleteThese.Add("MinimumDecliningLevels");
				//DeleteThese.Add("HighLowRangeFilter2");
				//DeleteThese.Add("HighLowRangeFilter");
				DeleteThese.Add("ArrowOffset");
				DeleteThese.Add("ArrowSize");
				DeleteThese.Add("ArrowUpFBrush");
				DeleteThese.Add("ArrowDownFBrush");
				DeleteThese.Add("ArrowUpStroke");
				DeleteThese.Add("ArrowDownStroke");
				DeleteThese.Add("LabelsEnabled");
				DeleteThese.Add("TextFont");
				DeleteThese.Add("LabelBuy");
				DeleteThese.Add("LabelSell");
				
				DeleteThese.Add("ArrowBarHeight");
				DeleteThese.Add("ArrowBarWidth");
								
				DeleteThese.Add("TextFont4");
		
		
				DeleteThese.Add("ActiveArrowOpacity");
				
				DeleteThese.Add("ActiveLabelOpacity");
				DeleteThese.Add("NotActiveArrowOpacity");
								
				DeleteThese.Add("NotActiveLabelOpacity");
		
						
				
		
		
			}			
						
		
		DeleteThese.Add("ButtonsEnabled");
		    if (!jbb.ButtonsEnabled)
			{	
				DeleteThese.Add("TradesButtonsEnabled");
				DeleteThese.Add("ActiveButtonsEnabled");
				DeleteThese.Add("AreaBrush");
				DeleteThese.Add("AreaOpacity");
				DeleteThese.Add("AreaBrush2");
				DeleteThese.Add("Area2Opacity");				
				DeleteThese.Add("ButtonSize");
				
				DeleteThese.Add("HideMenuEnabled");
				
				
			}				
			
			
		
		    if (!jbb.AudioEnabled)
			{	
				DeleteThese.Add("WAVFileName");
				DeleteThese.Add("WAVFileName2");
				DeleteThese.Add("QuickAudio");
				DeleteThese.Add("ColorBuyBrush");
				DeleteThese.Add("ColorSellBrush");
				
			}				
		


		    if (!jbb.LabelsEnabled)
			{	
				DeleteThese.Add("TextFont");
				DeleteThese.Add("LabelBuy");
				DeleteThese.Add("LabelSell");
				
			}				
			
				
		DeleteThese.Add("BackEnabled");
		    if (!jbb.BackEnabled)
			{	
				DeleteThese.Add("ColorAll");
				DeleteThese.Add("Brush01");
				DeleteThese.Add("Opacity01");
				DeleteThese.Add("Brush02");
				DeleteThese.Add("Opacity02");	
				DeleteThese.Add("Brush07");
				DeleteThese.Add("Opacity07");
				DeleteThese.Add("Brush08");
				DeleteThese.Add("Opacity08");					
				DeleteThese.Add("Brush09");
				DeleteThese.Add("Opacity09");
				
				
				DeleteThese.Add("BackSEnabled");					
				DeleteThese.Add("BackTEnabled");
				DeleteThese.Add("BackMEnabled");
				
				
			
				
			}				
			
			
			
			
			
			if (!jbb.BackEnabled)
			{
				
				DeleteThese.Add("ColorAll");
				DeleteThese.Add("BackSignalEnabled");
				DeleteThese.Add("BackTrendEnabled");
				
				
				DeleteThese.Add("BrushBack01");
				DeleteThese.Add("Opacity01");
				DeleteThese.Add("BrushBack02");
				DeleteThese.Add("Opacity02");
				DeleteThese.Add("BrushBack03");
				DeleteThese.Add("Opacity03");
				DeleteThese.Add("BrushBack04");
				DeleteThese.Add("Opacity04");
				DeleteThese.Add("BrushBack05");
				DeleteThese.Add("Opacity05");				
				
			}
			
			if (!jbb.BackTrendEnabled)
			{
				
		

				DeleteThese.Add("BrushBack04");
				DeleteThese.Add("Opacity04");
				DeleteThese.Add("BrushBack05");
				DeleteThese.Add("Opacity05");				
				
			}
			
			
			DeleteThese.Add("BackSignalEnabled");
			if (!jbb.BackSignalEnabled)
			{
				

				DeleteThese.Add("BrushBack01");
				DeleteThese.Add("Opacity01");
				DeleteThese.Add("BrushBack02");
				DeleteThese.Add("Opacity02");
				DeleteThese.Add("BrushBack03");
				DeleteThese.Add("Opacity03");
			
				
			}	
			 
			 
			DeleteThese.Add("DotsEnabled");
			if (!jbb.DotsEnabled)
			{
				

				DeleteThese.Add("DotSize");
				DeleteThese.Add("DotUpBrush");
				DeleteThese.Add("DotDownBrush");
				DeleteThese.Add("DotOffset");
				
			}	
			 

			if (!jbb.AudioEnabled)
			{
				

				DeleteThese.Add("QuickAudio");
				DeleteThese.Add("WAVFileName");
				DeleteThese.Add("WAVFileName2");
				DeleteThese.Add("ColorBuyBrush");
				DeleteThese.Add("ColorSellBrush");
				
			}	
			 			
			
			
			 

			if (!jbb.BarColorEnabled)
			{
				
				DeleteThese.Add("BarColorFEnabled");
				DeleteThese.Add("BarColorOEnabled");
				DeleteThese.Add("ColorUpBrush");
				DeleteThese.Add("ColorUpBrush2");
				DeleteThese.Add("ColorDownBrush");
				DeleteThese.Add("ColorDownBrush2");
				DeleteThese.Add("ColorNeutralBrush");
				DeleteThese.Add("ColorNeutralBrush2");
				DeleteThese.Add("BarOpacityUp");
				DeleteThese.Add("BarOpacityDown");
				DeleteThese.Add("BarOpacitySignal");
				
				
				DeleteThese.Add("ColorSixBrush");
				DeleteThese.Add("ColorSixBrush2");
				DeleteThese.Add("ColorFiveBrush");
				DeleteThese.Add("ColorFiveBrush2");
				DeleteThese.Add("ColorFourBrush");
				DeleteThese.Add("ColorFourBrush2");				
				
				
				
			}	
			 			
			
		
				
		
		
		
		
		
			
//      	DeleteThese2.Add("Calculate");
//			DeleteThese2.Add("Label");
//			DeleteThese2.Add("Maximum bars look back");
//			DeleteThese2.Add("Input series");
			

			
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
	}
	
	
	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SignalsCraigPritchert[] cacheSignalsCraigPritchert;
		public SignalsCraigPritchert SignalsCraigPritchert(bool useTimeFilter, string startT, string endT, string startT2, string endT2, string startT3, string endT3, bool useEST, bool secondaryFeedsEnabled, bool feed1Enabled, bool feed2Enabled, bool feed3Enabled, bool feed4Enabled, int eMAPeriod3, int eMAPeriod4, string thisBarType1, string thisBarType2, string thisBarType3, string thisBarType4)
		{
			return SignalsCraigPritchert(Input, useTimeFilter, startT, endT, startT2, endT2, startT3, endT3, useEST, secondaryFeedsEnabled, feed1Enabled, feed2Enabled, feed3Enabled, feed4Enabled, eMAPeriod3, eMAPeriod4, thisBarType1, thisBarType2, thisBarType3, thisBarType4);
		}

		public SignalsCraigPritchert SignalsCraigPritchert(ISeries<double> input, bool useTimeFilter, string startT, string endT, string startT2, string endT2, string startT3, string endT3, bool useEST, bool secondaryFeedsEnabled, bool feed1Enabled, bool feed2Enabled, bool feed3Enabled, bool feed4Enabled, int eMAPeriod3, int eMAPeriod4, string thisBarType1, string thisBarType2, string thisBarType3, string thisBarType4)
		{
			if (cacheSignalsCraigPritchert != null)
				for (int idx = 0; idx < cacheSignalsCraigPritchert.Length; idx++)
					if (cacheSignalsCraigPritchert[idx] != null && cacheSignalsCraigPritchert[idx].UseTimeFilter == useTimeFilter && cacheSignalsCraigPritchert[idx].StartT == startT && cacheSignalsCraigPritchert[idx].EndT == endT && cacheSignalsCraigPritchert[idx].StartT2 == startT2 && cacheSignalsCraigPritchert[idx].EndT2 == endT2 && cacheSignalsCraigPritchert[idx].StartT3 == startT3 && cacheSignalsCraigPritchert[idx].EndT3 == endT3 && cacheSignalsCraigPritchert[idx].UseEST == useEST && cacheSignalsCraigPritchert[idx].SecondaryFeedsEnabled == secondaryFeedsEnabled && cacheSignalsCraigPritchert[idx].Feed1Enabled == feed1Enabled && cacheSignalsCraigPritchert[idx].Feed2Enabled == feed2Enabled && cacheSignalsCraigPritchert[idx].Feed3Enabled == feed3Enabled && cacheSignalsCraigPritchert[idx].Feed4Enabled == feed4Enabled && cacheSignalsCraigPritchert[idx].EMAPeriod3 == eMAPeriod3 && cacheSignalsCraigPritchert[idx].EMAPeriod4 == eMAPeriod4 && cacheSignalsCraigPritchert[idx].ThisBarType1 == thisBarType1 && cacheSignalsCraigPritchert[idx].ThisBarType2 == thisBarType2 && cacheSignalsCraigPritchert[idx].ThisBarType3 == thisBarType3 && cacheSignalsCraigPritchert[idx].ThisBarType4 == thisBarType4 && cacheSignalsCraigPritchert[idx].EqualsInput(input))
						return cacheSignalsCraigPritchert[idx];
			return CacheIndicator<SignalsCraigPritchert>(new SignalsCraigPritchert(){ UseTimeFilter = useTimeFilter, StartT = startT, EndT = endT, StartT2 = startT2, EndT2 = endT2, StartT3 = startT3, EndT3 = endT3, UseEST = useEST, SecondaryFeedsEnabled = secondaryFeedsEnabled, Feed1Enabled = feed1Enabled, Feed2Enabled = feed2Enabled, Feed3Enabled = feed3Enabled, Feed4Enabled = feed4Enabled, EMAPeriod3 = eMAPeriod3, EMAPeriod4 = eMAPeriod4, ThisBarType1 = thisBarType1, ThisBarType2 = thisBarType2, ThisBarType3 = thisBarType3, ThisBarType4 = thisBarType4 }, input, ref cacheSignalsCraigPritchert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SignalsCraigPritchert SignalsCraigPritchert(bool useTimeFilter, string startT, string endT, string startT2, string endT2, string startT3, string endT3, bool useEST, bool secondaryFeedsEnabled, bool feed1Enabled, bool feed2Enabled, bool feed3Enabled, bool feed4Enabled, int eMAPeriod3, int eMAPeriod4, string thisBarType1, string thisBarType2, string thisBarType3, string thisBarType4)
		{
			return indicator.SignalsCraigPritchert(Input, useTimeFilter, startT, endT, startT2, endT2, startT3, endT3, useEST, secondaryFeedsEnabled, feed1Enabled, feed2Enabled, feed3Enabled, feed4Enabled, eMAPeriod3, eMAPeriod4, thisBarType1, thisBarType2, thisBarType3, thisBarType4);
		}

		public Indicators.SignalsCraigPritchert SignalsCraigPritchert(ISeries<double> input , bool useTimeFilter, string startT, string endT, string startT2, string endT2, string startT3, string endT3, bool useEST, bool secondaryFeedsEnabled, bool feed1Enabled, bool feed2Enabled, bool feed3Enabled, bool feed4Enabled, int eMAPeriod3, int eMAPeriod4, string thisBarType1, string thisBarType2, string thisBarType3, string thisBarType4)
		{
			return indicator.SignalsCraigPritchert(input, useTimeFilter, startT, endT, startT2, endT2, startT3, endT3, useEST, secondaryFeedsEnabled, feed1Enabled, feed2Enabled, feed3Enabled, feed4Enabled, eMAPeriod3, eMAPeriod4, thisBarType1, thisBarType2, thisBarType3, thisBarType4);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SignalsCraigPritchert SignalsCraigPritchert(bool useTimeFilter, string startT, string endT, string startT2, string endT2, string startT3, string endT3, bool useEST, bool secondaryFeedsEnabled, bool feed1Enabled, bool feed2Enabled, bool feed3Enabled, bool feed4Enabled, int eMAPeriod3, int eMAPeriod4, string thisBarType1, string thisBarType2, string thisBarType3, string thisBarType4)
		{
			return indicator.SignalsCraigPritchert(Input, useTimeFilter, startT, endT, startT2, endT2, startT3, endT3, useEST, secondaryFeedsEnabled, feed1Enabled, feed2Enabled, feed3Enabled, feed4Enabled, eMAPeriod3, eMAPeriod4, thisBarType1, thisBarType2, thisBarType3, thisBarType4);
		}

		public Indicators.SignalsCraigPritchert SignalsCraigPritchert(ISeries<double> input , bool useTimeFilter, string startT, string endT, string startT2, string endT2, string startT3, string endT3, bool useEST, bool secondaryFeedsEnabled, bool feed1Enabled, bool feed2Enabled, bool feed3Enabled, bool feed4Enabled, int eMAPeriod3, int eMAPeriod4, string thisBarType1, string thisBarType2, string thisBarType3, string thisBarType4)
		{
			return indicator.SignalsCraigPritchert(input, useTimeFilter, startT, endT, startT2, endT2, startT3, endT3, useEST, secondaryFeedsEnabled, feed1Enabled, feed2Enabled, feed3Enabled, feed4Enabled, eMAPeriod3, eMAPeriod4, thisBarType1, thisBarType2, thisBarType3, thisBarType4);
		}
	}
}

#endregion
