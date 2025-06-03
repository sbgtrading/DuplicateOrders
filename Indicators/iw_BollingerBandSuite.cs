// 
// Copyright (C) 2006, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//

#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Cbi;
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
	/// <summary>
	/// BollingerBandSuite Bands are plotted at standard deviation levels above and below a moving average. Since standard deviation is a measure of volatility, the bands are self-adjusting: widening during volatile markets and contracting during calmer periods.
	/// </summary>
	[Description("")]
	public class BollingerBandSuite : Indicator
	{
		private int SoundBar = 0;
		private int EmailBar = 0;
		private int PopupBar = 0;
		private int MostRecentArrowDirection = 0;
//Divergence variables
		private int A = 1;
		private int BarsAgo;
		private Series<double> IndicatorZeroed;
		private int [] HighBarsAgo = new int[3]{0,0,0};
		private int [] LowBarsAgo = new int[3]{0,0,0};
		private int ThisHigh, ThisLow;
		private int QHLength = 0, QLLength = 0;
		private int DivEmailBar = 0;
		private int DivSoundBar = 0;
//=======================

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "iw Bollinger Band Suite 2.0";
				AddPlot(Brushes.Blue, "BollUpper");
				AddPlot(Brushes.White, "Main");
				AddPlot(new Stroke(Brushes.White,2f), PlotStyle.Dot, "Dots");
				AddPlot(Brushes.Red, "BollLower");
				AddPlot(Brushes.White, "Center");
				AddPlot(new Stroke(Brushes.Yellow,7f), PlotStyle.Dot, "CenterLineCross");
				AddPlot(Brushes.Transparent, "BollMidline");

				var IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", "AIBBSuite", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				IsOverlay=false;
			}
			if(State==State.DataLoaded){
				IndicatorZeroed = new Series<double>(this);//inside State.DataLoaded
			}
		}
		/// <summary>
		/// Called on each bar update event (incoming tick)
		/// </summary>
		protected override void OnBarUpdate()
		{
			if(CurrentBar<2) return;

			double CenterValue = 0;
			if(pCalcMode == BollingerBandSuite_CalculationModes.MACD_Main){
				Main[0] = MACD(this.pMACD_FastPeriod,this.pMACD_SlowPeriod,1)[0];
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.MACD_Signal){
				Main[0] = MACD(this.pMACD_FastPeriod,this.pMACD_SlowPeriod,1).Avg[0];
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.ROC){
				Main[0] = ROC(pROC_Period)[0];
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.CCI){
				Main[0] = CCI(pCCI_Period)[0];
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.AO){
				Main[0] = SMA(pAO_FastPeriod)[0] - SMA(pAO_SlowPeriod)[0];
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.TSI){
				Main[0] = TSI(pTSI_FastPeriod,pTSI_SlowPeriod)[0];
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.RSI){
				Main[0] = RSI(pRSI_Period,1)[0];
				CenterValue = 50;
			}
			else if(pCalcMode == BollingerBandSuite_CalculationModes.Stochastics){
				Main[0] = Stochastics(pStochD_Period,pStochK_Period,pStochSmooth_Period)[0];
				CenterValue = 50;
			}
			CenterLine[0]=CenterValue;
			CenterLineCross.Reset(0);
			if(Main[0]>CenterLine[0] && Main[1]<=CenterLine[1]) CenterLineCross[0] = CenterValue;
			if(Main[0]<CenterLine[0] && Main[1]>=CenterLine[1]) CenterLineCross[0] = CenterValue;
			if(Main[0]<CenterLine[0]) PlotBrushes[4][0] = this.pZeroLineBearish;
			else PlotBrushes[4][0] = this.pZeroLineBullish;

			Middle[0] = SMA(Main,this.pBollingerPeriod)[0];
			double stdDevValue = StdDev(Main, pBollingerPeriod)[0];
			Upper[0] = (Middle[0] + pBollingerStdDevs * stdDevValue);
			Lower[0] = (Middle[0] - pBollingerStdDevs * stdDevValue);


			TheDots[0] = Main[0];
			if(Main[0]<Upper[0] && Main[0]>Lower[0]){
				if(TheDots[0]<TheDots[1]) PlotBrushes[2][0] = pBrushDotsBearishInside;
				if(TheDots[0]>TheDots[1]) PlotBrushes[2][0] = pBrushDotsBullishInside;
			}else{
				if(TheDots[0]<TheDots[1]) PlotBrushes[2][0] = pBrushDotsBearishOutside;
				if(TheDots[0]>TheDots[1]) PlotBrushes[2][0] = pBrushDotsBullishOutside;
			}

			if(pShowArrows){
				string tag = string.Format("Signal_{0}",CurrentBar);
				RemoveDrawObject(tag);
				if(Main[1]>Upper[1]) MostRecentArrowDirection =  1;
				if(Main[1]<Lower[1]) MostRecentArrowDirection = -1;
				if(Main[0]>Upper[0] && MostRecentArrowDirection !=  1) Draw.ArrowUp  (this, tag, false, 0,Low[0]-TickSize,pBrushUpArrow);
				if(Main[0]<Lower[0] && MostRecentArrowDirection != -1) Draw.ArrowDown(this, tag, false, 0,High[0]+TickSize,pBrushDownArrow);
			}
			if(pLaunchPopup && PopupBar!=CurrentBar && State!=State.Historical){
				if(Main[0]>Upper[0]) {Log("Buy signal", Cbi.LogLevel.Alert); PopupBar=CurrentBar;}
				if(Main[0]<Lower[0]) {Log("Sell signal",Cbi.LogLevel.Alert); PopupBar=CurrentBar;}
			}else if(pEnableSounds && SoundBar != CurrentBar){
				try{
					if(Main[0]>Upper[0] && pBuySound.Length>0) {PlaySound(AddSoundFolder(pBuySound));  SoundBar=CurrentBar;}
					if(Main[0]<Lower[0] && pSellSound.Length>0) {PlaySound(AddSoundFolder(pSellSound)); SoundBar=CurrentBar;}
				}catch{}
			}
			if(EmailBar!=CurrentBar && pEmailAddress.Length>0){
				if(Main[0]>Upper[0]) {SendMail(pEmailAddress,"Buy signal Bollinger Suite","Generated by indicator, automatically");  EmailBar=CurrentBar;}
				if(Main[0]<Lower[0]) {SendMail(pEmailAddress,"Sell signal Bollinger Suite","Generated by indicator, automatically"); EmailBar=CurrentBar;}
			}

			if(!pEnableDivergence) return;
			IndicatorZeroed[0] = Main[0] - CenterValue;
			if(CurrentBar<pScanWidth) return;
			double PriceDiff, IndicatorDiff;
			int ThisHigh = HighestBar(Close, pScanWidth);
			int DivergenceSignal = 0;//+1 for buy, -1 for sell
			if (ThisHigh == A) {
//				if (ShowAlerts == true) {
//					Alert("MyAlrt1" + CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "High Found", MyAlert1, 10, Brushes.Black, Brushes.Yellow);  AddSoundFolder(
//				}
				
				for (int i = HighBarsAgo.Length-1; i >= 1; i--) {
					HighBarsAgo[i] = HighBarsAgo[i-1];
				}

				HighBarsAgo[0] = CurrentBar - A;

//				Draw.Dot(this, "Hdot" + CurrentBar.ToString(), true, A, High[A] + (TickSize*pMarkerDistanceFactor), pColorUpDivergence);
				DrawOnPricePanel = false;

//				Draw.Dot(this, "IHdot" + CurrentBar.ToString(), true, A, IndicatorZeroed[A], pColorUpDivergence);
				DrawOnPricePanel = true;

				if (++QHLength >= 2) {
					
					for(int i = 0; i < Math.Min(QHLength, HighBarsAgo.Length); i++) {
						BarsAgo = CurrentBar - HighBarsAgo[i];

						IndicatorDiff	= IndicatorZeroed[A] - IndicatorZeroed[BarsAgo];

						PriceDiff	= Close[A] - Close[BarsAgo];
						if (((IndicatorDiff < IndicatorDiffLimit) && (PriceDiff >= PriceDiffLimit)) || ((IndicatorDiff > IndicatorDiffLimit) && (PriceDiff <= PriceDiffLimit))) {						
								
							if ((BarsAgo - A) < pScanWidth) {
								DivergenceSignal = -1;
//								if (ShowAlerts == true) {
//									Alert("MyAlrt2" + CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "Divergence Found", MyAlert2, 10, Brushes.Black, Brushes.Yellow);  AddSoundFolder(
//								}
								
								Draw.Line(this, "high"+CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, High[BarsAgo] + (TickSize*pMarkerDistanceFactor), A, High[A] + (TickSize*pMarkerDistanceFactor), pDivLineBrush, pDivDashStyle, pDivLineWidth);								
								Draw.TriangleDown(this, CurrentBar.ToString(), true, 0, High[0] + (TickSize*pMarkerDistanceFactor), pBrushDownDivArrow);

								DrawOnPricePanel = false;	
								Draw.Line(this, "IH"+CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, IndicatorZeroed[BarsAgo]+CenterValue, A, IndicatorZeroed[A]+CenterValue, pDivLineBrush, pDivDashStyle, pDivLineWidth);
								DrawOnPricePanel = true;
							}
						}
					}
				}
			}
//---------------------------------------------
			ThisLow = LowestBar(Close, pScanWidth);
			if (ThisLow == A) {
Print(209);
				for (int i = LowBarsAgo.Length-1; i >= 1; i--) {
					LowBarsAgo[i] = LowBarsAgo[i-1];
				}

				LowBarsAgo[0] = CurrentBar - A;

//				Draw.Dot(this, "Ldot" + CurrentBar.ToString(), true, A, Low[A] - (TickSize*pMarkerDistanceFactor), pColorDownDivergence);
				DrawOnPricePanel = false;
//				Draw.Dot(this, "ILdot" + CurrentBar.ToString(), true, A, IndicatorZeroed[A], pColorDownDivergence);
				DrawOnPricePanel = true;

//				if (ShowAlerts == true) {
//					Alert("MyAlrt1" + CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "Low Found", MyAlert1, 10, Brushes.Black, Brushes.Yellow);  AddSoundFolder(
//				}

				if (++QLLength >= 2) {
					
					for(int i = 0; i < Math.Min(QLLength, LowBarsAgo.Length); i++) {

						BarsAgo = CurrentBar - LowBarsAgo[i];

						IndicatorDiff 	= IndicatorZeroed[A] - IndicatorZeroed[BarsAgo];
						PriceDiff 		= Close[A] - Close[BarsAgo];	

						if (((IndicatorDiff > IndicatorDiffLimit) && (PriceDiff <= PriceDiffLimit)) || ((IndicatorDiff < IndicatorDiffLimit) && (PriceDiff >= PriceDiffLimit))) {	

							if ((BarsAgo - A) < pScanWidth) {
								DivergenceSignal = 1;
								Draw.Line(this, "low"+CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, Low[BarsAgo] - (TickSize*pMarkerDistanceFactor), A, Low[A] - (TickSize*pMarkerDistanceFactor), pDivLineBrush, pDivDashStyle, pDivLineWidth);

								Draw.TriangleUp(this, CurrentBar.ToString(), true, 0, Low[0] - (TickSize*pMarkerDistanceFactor), pBrushUpDivArrow);

								DrawOnPricePanel = false;
								Draw.Line(this, "Ilow"+CurrentBar.ToString() + BarsAgo.ToString(), true, BarsAgo, IndicatorZeroed[BarsAgo]+CenterValue, A, IndicatorZeroed[A]+CenterValue, pDivLineBrush, pDivDashStyle, pDivLineWidth);
								DrawOnPricePanel = true;

//								if (ShowAlerts == true) {
//									Alert("MyAlrt2" + CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "Divergence Found", MyAlert2, 10, Brushes.Black, Brushes.Yellow);  AddSoundFolder(
//								}
							}
						}
					}
				}
			}
			if(DivergenceSignal!=0){
				if(pEnableDivergenceSounds && DivSoundBar != CurrentBar){
					try{
						if(DivergenceSignal== 1 && pBuySound.Length>0) {PlaySound(AddSoundFolder(pBuyDivSound));  DivSoundBar=CurrentBar;}
						if(DivergenceSignal==-1 && pSellSound.Length>0) {PlaySound(AddSoundFolder(pSellDivSound)); DivSoundBar=CurrentBar;}
					}catch{}
				}
				if(DivEmailBar!=CurrentBar && pDivEmailAddress.Length>0){
					if(DivergenceSignal== 1) {SendMail(pDivEmailAddress,"Buy divergence signal Bollinger Suite","Generated by indicator, automatically");  DivEmailBar=CurrentBar;}
					if(DivergenceSignal==-1) {SendMail(pDivEmailAddress,"Sell divergence signal Bollinger Suite","Generated by indicator, automatically"); DivEmailBar=CurrentBar;}
				}
			}

		}

//-----------------------------------------------------------------------------------------------
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
		public Series<double> Upper
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Main
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TheDots
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CenterLine
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CenterLineCross
		{
			get { return Values[5]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[6]; }
		}
		#endregion
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds\"+wav);
		}
//====================================================================

		#region Properties

		#region Divergence
		private bool pEnableDivergenceSounds = true;
		[Description("Turn-on divergence sounds?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound  enabled?",  GroupName = "Divergence")]
		public bool EnableDivergenceSounds
		{
			get { return pEnableDivergenceSounds; }
			set { pEnableDivergenceSounds = value; }
		}

		private bool pEnableDivergence = false;
		[Description("Turn-on divergence detection?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = " Enable?",  GroupName = "Divergence")]
		public bool EnableDivergence
		{
			get { return pEnableDivergence; }
			set { pEnableDivergence = value; }
		}
		private int pScanWidth = 30;
		[Description("Lookback window for divergence search")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lookback Bars",  GroupName = "Divergence")]
		public int ScanWidth
		{
			get { return pScanWidth; }
			set { pScanWidth = Math.Max(10, value); }
		}

		private double pPriceDiffLimit = 0.0;
		[Description("Price Difference Limit for divergence")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Limit of Price diff.",  GroupName = "Divergence")]
		public double PriceDiffLimit
		{
			get { return pPriceDiffLimit; }
			set { pPriceDiffLimit =value; }
		}

		private double pIndicatorDiffLimit 	= 0.0;
		[Description("Indicator Difference Limit for divergence")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Limit of Indi diff.",  GroupName = "Divergence")]
		public double IndicatorDiffLimit
		{
			get { return pIndicatorDiffLimit; }
			set { pIndicatorDiffLimit = value; }
		}

		private int pMarkerDistanceFactor = 1;
		[Description("Marker Distance Factor")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Separation",  GroupName = "Divergence")]
		public int MarkerDistanceFactor
		{
			get { return pMarkerDistanceFactor; }
			set { pMarkerDistanceFactor = Math.Max(1,value);}
		}

		private Brush pDivLineBrush = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of divergence line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line Color",  GroupName = "Divergence")]
		public Brush DivLineBrush
			{get { return pDivLineBrush; }
			 set { pDivLineBrush = value; }}
		[Browsable(false)]
		public string pDivLineBrushSerialize
		{get { return Serialize.BrushToString(pDivLineBrush); }set { pDivLineBrush = Serialize.StringToBrush(value); }		}

		private int pDivLineWidth = 1;
		[Description("Divergence Line Width")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line Width",  GroupName = "Divergence")]
		public int DivergenceLineWidth
		{
			get { return pDivLineWidth; }
			set { pDivLineWidth = Math.Max(1,value);}
		}

		private DashStyleHelper pDivDashStyle = DashStyleHelper.Dot;
		[Description("Divergence Dash Style")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash Style",  GroupName = "Divergence")]
		public DashStyleHelper DivergenceDashStyle
		{
			get { return pDivDashStyle; }
			set { pDivDashStyle = value; }
		}
		private Brush pBrushDownDivArrow = Brushes.Magenta;
		[XmlIgnore()]
		[Description("Color of sell arrow - when main line crosses below lower band")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Arrow",  GroupName = "Divergence")]
		public Brush BrushDownDivArrow
			{get { return pBrushDownDivArrow; }
			 set { pBrushDownDivArrow = value; }}
		[Browsable(false)]
		public string ColorDownDivArrowSerialize
		{get { return Serialize.BrushToString(pBrushDownDivArrow); }set { pBrushDownDivArrow = Serialize.StringToBrush(value); }		}

		private Brush pBrushUpDivArrow = Brushes.Cyan;
		[XmlIgnore()]
		[Description("Color of buy arrow - when main line crosses above lower band")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Arrow",  GroupName = "Divergence")]
		public Brush BrushUpDivArrow
			{get { return pBrushUpDivArrow; }
			 set { pBrushUpDivArrow = value; }}
		[Browsable(false)]
		public string ColorUpDivArrowSerialize
		{get { return Serialize.BrushToString(pBrushUpDivArrow); }set { pBrushUpDivArrow = Serialize.StringToBrush(value); }		}

		private string pBuyDivSound = "";
		[Description("Sound file on up divergence - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound Buy",  GroupName = "Divergence")]
		public string BuyDivSound
		{
			get { return pBuyDivSound; }
			set { pBuyDivSound = value.Trim(); }
		}
		private string pSellDivSound = "";
		[Description("Sound file on down divergence - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sound Sell",  GroupName = "Divergence")]
		public string SellDivSound
		{
			get { return pSellDivSound; }
			set { pSellDivSound = value.Trim(); }
		}
		private string pDivEmailAddress = string.Empty;
		[Description("Email address for divergence signals - leave blank to turn-off email notifications")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Email address",  GroupName = "Divergence")]
		public string DivEmailAddress
		{
			get { return pDivEmailAddress; }
			set { pDivEmailAddress = value.Trim(); }
		}
		#endregion

		#region Dot Colors
		private Brush pBrushDotsBearishInside = Brushes.Salmon;
		private Brush pBrushDotsBullishInside = Brushes.PaleGreen;
		private Brush pBrushDotsBearishOutside = Brushes.Crimson;
		private Brush pBrushDotsBullishOutside = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of bullish outside")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish outside",  GroupName = "Dot Colors")]
		public Brush BrushDotsBullishOutside
			{get { return pBrushDotsBullishOutside; }
			 set { pBrushDotsBullishOutside = value; }}
		[Browsable(false)]
		public string pBrushDotsBullishOutsideSerialize
		{get { return Serialize.BrushToString(pBrushDotsBullishOutside); }set { pBrushDotsBullishOutside = Serialize.StringToBrush(value); }		}

		[XmlIgnore()]
		[Description("Color of bearish outside")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish outside",  GroupName = "Dot Colors")]
		public Brush BrushDotsBearishOutside
			{get { return pBrushDotsBearishOutside; }
			 set { pBrushDotsBearishOutside = value; }}
		[Browsable(false)]
		public string pBrushDotsBearishOutsideSerialize
		{get { return Serialize.BrushToString(pBrushDotsBearishOutside); }set { pBrushDotsBearishOutside = Serialize.StringToBrush(value); }		}


		[XmlIgnore()]
		[Description("Color of bullish inside")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish inside",  GroupName = "Dot Colors")]
		public Brush BrushDotsBullishInside
			{get { return pBrushDotsBullishInside; }
			 set { pBrushDotsBullishInside = value; }}
		[Browsable(false)]
		public string pBrushDotsBullishInsideSerialize
		{get { return Serialize.BrushToString(pBrushDotsBullishInside); }set { pBrushDotsBullishInside = Serialize.StringToBrush(value); }		}

		[XmlIgnore()]
		[Description("Color of bearish inside")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish inside",  GroupName = "Dot Colors")]
		public Brush BrushDotsBearishInside
			{get { return pBrushDotsBearishInside; }
			 set { pBrushDotsBearishInside = value; }}
		[Browsable(false)]
		public string pBrushDotsBearishInsideSerialize
		{get { return Serialize.BrushToString(pBrushDotsBearishInside); }set { pBrushDotsBearishInside = Serialize.StringToBrush(value); }		}
		#endregion

		#region Alerts
		private Brush pZeroLineBearish = Brushes.Crimson;
		[XmlIgnore()]
		[Description("Color of zero line when Main is below it")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish Zeroline",  GroupName = "Alerts")]
		public Brush ZeroLineBearish
			{get { return pZeroLineBearish; }
			 set { pZeroLineBearish = value; }}
		[Browsable(false)]
		public string pZeroLineBearishSerialize
		{get { return Serialize.BrushToString(pZeroLineBearish); }set { pZeroLineBearish = Serialize.StringToBrush(value); }		}

		private Brush pZeroLineBullish = Brushes.Blue;
		[XmlIgnore()]
		[Description("Color of zero line when Main is above it")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish Zeroline",  GroupName = "Alerts")]
		public Brush ZeroLineBullish
			{get { return pZeroLineBullish; }
			 set { pZeroLineBullish = value; }}
		[Browsable(false)]
		public string pZeroLineBullishSerialize
		{get { return Serialize.BrushToString(pZeroLineBullish); }set { pZeroLineBullish = Serialize.StringToBrush(value); }		}

		private Brush pBrushDownArrow = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of sell arrow - when main line crosses below lower band")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell Arrow",  GroupName = "Alerts")]
		public Brush BrushDownArrow
			{get { return pBrushDownArrow; }
			 set { pBrushDownArrow = value; }}
		[Browsable(false)]
		public string ColorDownArrowSerialize
		{get { return Serialize.BrushToString(pBrushDownArrow); }set { pBrushDownArrow = Serialize.StringToBrush(value); }		}

		private Brush pBrushUpArrow = Brushes.Blue;
		[XmlIgnore()]
		[Description("Color of buy arrow - when main line crosses above lower band")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy Arrow",  GroupName = "Alerts")]
		public Brush BrushUpArrow
			{get { return pBrushUpArrow; }
			 set { pBrushUpArrow = value; }}
		[Browsable(false)]
		public string ColorUpArrowSerialize
		{get { return Serialize.BrushToString(pBrushUpArrow); }set { pBrushUpArrow = Serialize.StringToBrush(value); }		}

		private string pBuySound = "Alert2.wav";
		[Description("Sound file on up signal - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Alerts")]
		public string BuySound
		{
			get { return pBuySound; }
			set { pBuySound = value.Trim(); }
		}
		private string pSellSound = "Alert2.wav";
		[Description("Sound file on down signal - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Alerts")]
		public string SellSound
		{
			get { return pSellSound; }
			set { pSellSound = value.Trim(); }
		}
		private bool pShowArrows = true;
		[Description("Show arrows when main line crosses outside of bollinger bands")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show arrows?",  GroupName = "Alerts")]
		public bool ShowArrows
		{
			get { return pShowArrows; }
			set { pShowArrows = value; }
		}
		private bool pLaunchPopup = false;
		[Description("Launch popup window when main line crosses outside of bollinger bands")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Launch popup?",  GroupName = "Alerts")]
		public bool LaunchPopup
		{
			get { return pLaunchPopup; }
			set { pLaunchPopup = value; }
		}
		private bool pEnableSounds = false;
		[Description("Enable playing of the sound alerts when main line crosses outside of bollinger bands")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enable sounds?",  GroupName = "Alerts")]
		public bool EnableSounds
		{
			get { return pEnableSounds; }
			set { pEnableSounds = value; }
		}
		private string pEmailAddress = string.Empty;
		[Description("Email address - leave blank to turn-off email notifications")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Email address",  GroupName = "Alerts")]
		public string EmailAddress
		{
			get { return pEmailAddress; }
			set { pEmailAddress = value.Trim(); }
		}
		#endregion

		private BollingerBandSuite_CalculationModes pCalcMode = BollingerBandSuite_CalculationModes.MACD_Main;
		[Description("Basis for calculation")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Calculation Mode",  GroupName = "  Parameters")]
		public BollingerBandSuite_CalculationModes CalculationMode
		{	get { return pCalcMode; }
			set { pCalcMode = value; }
		}

		#region MACD
		private int pMACD_FastPeriod = 12;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast period",  GroupName = " Parameters - MACD")]
		public int MACD_FastPeriod
		{
			get { return pMACD_FastPeriod; }
			set { pMACD_FastPeriod = Math.Max(1, value); }
		}
		private int pMACD_SlowPeriod = 26;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow period",  GroupName = " Parameters - MACD")]
		public int MACD_SlowPeriod
		{
			get { return pMACD_SlowPeriod; }
			set { pMACD_SlowPeriod = Math.Max(1, value); }
		}
		private int pMACD_SmoothPeriod = 9;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth period",  GroupName = " Parameters - MACD")]
		public int MACD_SmoothPeriod
		{
			get { return pMACD_SmoothPeriod; }
			set { pMACD_SmoothPeriod = Math.Max(1, value); }
		}
		#endregion

		private int pRSI_Period = 14;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period",  GroupName = " Parameters - RSI")]
		public int RSIPeriod
		{
			get { return pRSI_Period; }
			set { pRSI_Period = Math.Max(1, value); }
		}
		private int pStochK_Period = 8;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "K Period",  GroupName = " Parameters - Stoch")]
		public int StochKPeriod
		{
			get { return pStochK_Period; }
			set { pStochK_Period = Math.Max(1, value); }
		}
		private int pStochD_Period = 3;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "D Period",  GroupName = " Parameters - Stoch")]
		public int StochDPeriod
		{
			get { return pStochD_Period; }
			set { pStochD_Period = Math.Max(1, value); }
		}
		private int pStochSmooth_Period = 3;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "D Period",  GroupName = " Parameters - Stoch")]
		public int StochSmoothPeriod
		{
			get { return pStochSmooth_Period; }
			set { pStochSmooth_Period = Math.Max(1, value); }
		}

		#region AO periods
		private int pAO_FastPeriod = 5;
		[Description("Fast SMA period")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast period",  GroupName = " Parameters - AO")]
		public int AOFastPeriod
		{
			get { return pAO_FastPeriod; }
			set { pAO_FastPeriod = Math.Max(1, value); }
		}
		private int pAO_SlowPeriod = 34;
		[Description("Slow SMA period")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow period",  GroupName = " Parameters - AO")]
		public int AO_SlowPeriod
		{
			get { return pAO_SlowPeriod; }
			set { pAO_SlowPeriod = Math.Max(1, value); }
		}
		#endregion

		#region TSI periods
		private int pTSI_FastPeriod = 3;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast period",  GroupName = " Parameters - TSI")]
		public int TSIFastPeriod
		{
			get { return pTSI_FastPeriod; }
			set { pTSI_FastPeriod = Math.Max(1, value); }
		}
		private int pTSI_SlowPeriod = 14;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow period",  GroupName = " Parameters - TSI")]
		public int TSI_SlowPeriod
		{
			get { return pTSI_SlowPeriod; }
			set { pTSI_SlowPeriod = Math.Max(1, value); }
		}
		#endregion

		#region Bollinger
		private int pBollingerPeriod = 20;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period",  GroupName = " Parameters - Bollinger")]
		public int BollingerPeriod
		{
			get { return pBollingerPeriod; }
			set { pBollingerPeriod = Math.Max(1, value); }
		}
		private double pBollingerStdDevs = 2.0;
		[Description("Number of standard deviations")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Std. devs",  GroupName = " Parameters - Bollinger")]
		public double BollingerStdDevs
		{
			get { return pBollingerStdDevs; }
			set { pBollingerStdDevs = Math.Max(0, value); }
		}
		#endregion

		private int pROC_Period = 10;
		[Description("Numbers of bars used for ROC calculations")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period",  GroupName = " Parameters - ROC")]
		public int ROCPeriod
		{
			get { return pROC_Period; }
			set { pROC_Period = Math.Max(1, value); }
		}
		private int pCCI_Period = 10;
		[Description("Numbers of bars used for CCI calculations")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period",  GroupName = " Parameters - CCI")]
		public int CCI_Period
		{
			get { return pCCI_Period; }
			set { pCCI_Period = Math.Max(1, value); }
		}

		#endregion
	}
}
public enum BollingerBandSuite_CalculationModes{
	MACD_Main,MACD_Signal,ROC,CCI,AO,TSI,RSI,Stochastics
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerBandSuite[] cacheBollingerBandSuite;
		public BollingerBandSuite BollingerBandSuite()
		{
			return BollingerBandSuite(Input);
		}

		public BollingerBandSuite BollingerBandSuite(ISeries<double> input)
		{
			if (cacheBollingerBandSuite != null)
				for (int idx = 0; idx < cacheBollingerBandSuite.Length; idx++)
					if (cacheBollingerBandSuite[idx] != null &&  cacheBollingerBandSuite[idx].EqualsInput(input))
						return cacheBollingerBandSuite[idx];
			return CacheIndicator<BollingerBandSuite>(new BollingerBandSuite(), input, ref cacheBollingerBandSuite);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerBandSuite BollingerBandSuite()
		{
			return indicator.BollingerBandSuite(Input);
		}

		public Indicators.BollingerBandSuite BollingerBandSuite(ISeries<double> input )
		{
			return indicator.BollingerBandSuite(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerBandSuite BollingerBandSuite()
		{
			return indicator.BollingerBandSuite(Input);
		}

		public Indicators.BollingerBandSuite BollingerBandSuite(ISeries<double> input )
		{
			return indicator.BollingerBandSuite(input);
		}
	}
}

#endregion
