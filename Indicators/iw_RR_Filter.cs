
#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
#endregion

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
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("")]
	public class RR_Filter : Indicator
	{
		#region Variables
		private Brush BackgroundBrushUpTrend = Brushes.Transparent;
		private Brush BackgroundBrushDownTrend = Brushes.Transparent;
		private int					pFast	= 8;
		private int					pSlow	= 21;
		private int					pSmooth	= 8;
		private	EMA fastma, slowma;
		private Series<double> signalline, MacdLine, difference;
		private RR_Filter_Type pCrossingType1 = RR_Filter_Type.None;
		private RR_Filter_Type pCrossingType2 = RR_Filter_Type.None;
		private bool pLaunchPopup1 = false;
		private bool pLaunchPopup2 = false;
		private bool playAlertSound1  = true;
		private bool playAlertSound2  = true;
		private string pSoundFileUp1 = "Alert3.wav";
		private string pSoundFileDown1 = "Alert2.wav";
		private string pSoundFileUp2 = "Alert3.wav";
		private string pSoundFileDown2 = "Alert2.wav";
		private bool UpArrowDrawn = false;
		private bool DownArrowDrawn = false;
		private int pMaxAudioAlerts1 = 3;
		private int pMaxAudioAlerts2 = 3;
		private int AudioAlertsCount1 = 0;
		private int AudioAlertsCount2 = 0;
		private int BarAtLastPopup1 = 0;
		private int BarAtLastPopup2 = 0;
		private int BarOfLastEmail1 = -1;
		private int BarOfLastEmail2 = -1;
		private Brush pUpBrush1 = Brushes.Transparent;
		private Brush pDownBrush1 = Brushes.Transparent;
		private Brush pUpBrush2 = Brushes.Transparent;
		private Brush pDownBrush2 = Brushes.Transparent;
		private bool RunInit = true;
		private int ArrowOffset = 0;
		#endregion
		private const int LONG = 1;
		private const int SHORT = -1;

		private EMA emaMACDLine = null;
		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			Name = "iw RR Filter";
			bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && (
				NinjaTrader.Cbi.License.MachineId=="B0D2E9D1C802E279D3678D7DE6A33CE4" || NinjaTrader.Cbi.License.MachineId=="766C8CD2AD83CA787BCA6A2A76B2303B");
			if(!IsBen)
				VendorLicense("IndicatorWarehouse", "AIRoadRunner", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
//				VendorLicense("IndicatorWarehouse", "AIMACDAlertOnCrossing", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
//			AddPlot(new Stroke(Brushes.Green,1), "MACD");
//			AddPlot(new Stroke(Brushes.DarkViolet,1), "Signal");
//			AddPlot(new Stroke(Brushes.Navy, 2), PlotStyle.Bar, "Diff");
//			AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Dot, "ArrowDirection1");
//			AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Dot, "ArrowDirection2");
//			AddLine(new Stroke(Brushes.DarkGray, DashStyleHelper.Solid, 1), 0, "Zero line");
				IsOverlay=false;
				IsAutoScale=false;
			}
			if (State == State.DataLoaded)
			{
				MacdLine = new Series<double>(this,this.MaximumBarsLookBack);//inside State.DataLoaded
				signalline = new Series<double>(this,this.MaximumBarsLookBack);//inside State.DataLoaded
				difference = new Series<double>(this,this.MaximumBarsLookBack);//inside State.DataLoaded
				if(Calculate != Calculate.OnBarClose) ArrowOffset = 1;
				emaMACDLine = EMA(MacdLine, pSmooth);
				fastma = EMA(Input, pFast);
				slowma = EMA(Input, pSlow);
			}
		}

		private void CleanupPriorBar(int SignalId, RR_Filter_Type CrossingType) {
			#region CleanupPriorBar
			//SignalId is either 1 or 2
			bool RemoveTheArrow = false;
			if(Values[2+SignalId][1] > 0) {
				if(CrossingType == RR_Filter_Type.MACDxSignal) {
					if(MacdLine[2]<=signalline[2] && MacdLine[1]<=signalline[1]) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.MACDxZero) {
					if(MacdLine[2]<=0 && MacdLine[1]<=0) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.SignalxZero) {
					if(signalline[2]<=0 && signalline[1]<=0) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.MACD_DirectionChange) {
					if(MacdLine[2]<=MacdLine[1] && MacdLine[1]<=MacdLine[0]) RemoveTheArrow = true;
					if(MacdLine[2]>=MacdLine[1] && MacdLine[1]>=MacdLine[0]) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.Signal_DirectionChange) {
					if(signalline[2]<=signalline[1] && signalline[1]<=signalline[0]) RemoveTheArrow = true;
					if(signalline[2]>=signalline[1] && signalline[1]>=signalline[0]) RemoveTheArrow = true;
				}
			}
			else if(Values[2+SignalId][1] < 0) {
				if(CrossingType == RR_Filter_Type.MACDxSignal) {
					if(MacdLine[2]>=signalline[2] && MacdLine[1]>=signalline[1]) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.MACDxZero) {
					if(MacdLine[2]>=0 && MacdLine[1]>=0) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.SignalxZero) {
					if(signalline[2]>=0 && signalline[1]>=0) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.MACD_DirectionChange) {
					if(MacdLine[2]>=MacdLine[2] && MacdLine[1]>=MacdLine[1]) RemoveTheArrow = true;
				}
				else if(CrossingType == RR_Filter_Type.Signal_DirectionChange) {
					if(signalline[2]>=signalline[2] && signalline[1]>=signalline[1]) RemoveTheArrow = true;
				}
			}
			if(RemoveTheArrow) {
				//Print("Removed the arrow which had previously printed at "+Time[1].ToString());
				RemoveDrawObject(string.Concat("macd_crossing",SignalId,(CurrentBar-1)));
			}
			#endregion
		}
		private int CheckForSignal(RR_Filter_Type CrossingType){
			#region CheckForSignal
			if(CrossingType == RR_Filter_Type.MACDxSignal) {
				if(CrossAbove(MacdLine,signalline,1))	   return LONG;
				else if(CrossBelow(MacdLine,signalline,1)) return SHORT;
			}
			else if(CrossingType == RR_Filter_Type.MACDxZero) {
				if(CrossAbove(MacdLine,0,1))	  return LONG;
				else if(CrossBelow(MacdLine,0,1)) return SHORT;
			}
			else if(CrossingType == RR_Filter_Type.SignalxZero) {
				if(CrossAbove(signalline,0,1))		return LONG;
				else if(CrossBelow(signalline,0,1))	return SHORT;
			}
			else if(CrossingType == RR_Filter_Type.MACD_DirectionChange) {
				if(MacdLine[0]>MacdLine[1]      && MacdLine[1]<=MacdLine[2]) return LONG;
				else if(MacdLine[0]<MacdLine[1] && MacdLine[1]>=MacdLine[2]) return SHORT;
			}
			else if(CrossingType == RR_Filter_Type.Signal_DirectionChange) {
				if(signalline[0]>signalline[1]      && signalline[1]<=signalline[2]) return LONG;
				else if(signalline[0]<signalline[1] && signalline[1]>=signalline[2]) return SHORT;
			}
			return 0;
			#endregion
		}
		private void FireAlert(int SignalId, int SignalDirection, bool LaunchPopup, ref int BarAtLastPopup, string SoundFileUp, string SoundFileDown, bool EnableAlertSound, int MaxAlerts, ref int AudioAlertsCount,
							   bool SendEmail, ref int BarOfLastEmail, string EmailAddress, string CrossingTypeStr, RR_Filter_GraphicLocation GraphicLocation, RR_Filter_GraphicType GraphicType, Brush MarkerUpBrush, Brush MarkerDownBrush){
			#region FireAlert
			if(LaunchPopup && BarAtLastPopup != CurrentBar) {
				if(State !=State.Historical) Log(string.Concat(Name," is ",(SignalDirection==LONG?"LONG":"SHORT")," on ",Instrument," ",Bars.BarsPeriod),LogLevel.Alert);
				BarAtLastPopup = CurrentBar;
			}
			else if(EnableAlertSound && AudioAlertsCount < MaxAlerts) {
				PlaySoundCustom(SignalDirection==LONG? SoundFileUp : SoundFileDown);
				AudioAlertsCount++;
			}
			if(SendEmail && BarOfLastEmail != CurrentBar && EmailAddress.Length>0) {
				BarOfLastEmail = CurrentBar;
				SendMail(
					EmailAddress,
					string.Concat(Name," ",CrossingTypeStr," ",(SignalDirection==LONG?"LONG":"SHORT")," cross:  ",Instrument.FullName," on ",Bars.BarsPeriod),
					string.Concat(Environment.NewLine,Environment.NewLine,"",Environment.NewLine,Environment.NewLine,"Auto-generated on ",NinjaTrader.Core.Globals.Now.ToString())
				);
			}

			if(GraphicLocation == RR_Filter_GraphicLocation.OnPriceChart) {
				DrawOnPricePanel = true;
				DrawGaphic(SignalDirection, false, SignalDirection==LONG? Low[0]-TickSize : High[0]+TickSize, GraphicType, SignalId, MarkerUpBrush, MarkerDownBrush);
			}
			else if(GraphicLocation == RR_Filter_GraphicLocation.OnMACD) {
				DrawOnPricePanel = false;
				double val = SignalDirection==LONG? Math.Min(MacdLine[0],signalline[0]) : Math.Max(MacdLine[0],signalline[0]);
//				Print(SignalDirection+"  "+Time[0].ToString()+"  val: "+val.ToString()+"   MacdLine[0]: "+MacdLine[0].ToString()+"  signalline[0]: "+signalline[0].ToString());
				DrawGaphic(SignalDirection, false, val, GraphicType, SignalId, MarkerUpBrush, MarkerDownBrush);
			}
			#endregion
		}
		private void PlaySoundCustom(string wav){
			if(State == State.Historical) return;
			string pwav = AddSoundFolder(wav);
			if(System.IO.File.Exists(pwav))
				PlaySound(pwav);
			else
				Log(wav+" sound not found in your NT8 Sounds folder",LogLevel.Information);
		}
		private void DrawGaphic(int Direction, bool Scale, double price, RR_Filter_GraphicType GraphicType, int SignalId, Brush UpBrush, Brush DownBrush) {
			#region DrawGraphic
			string tag = string.Concat("macd_crossing",SignalId,CurrentBar);

			if(Direction > 0) {
				if(GraphicType == RR_Filter_GraphicType.Arrow)
					Draw.ArrowUp(this, tag, Scale, 0, price, UpBrush);
				if(GraphicType == RR_Filter_GraphicType.Dot)
					Draw.Dot(this, tag, Scale, 0, price, UpBrush);
				if(GraphicType == RR_Filter_GraphicType.Triangle)
					Draw.TriangleUp(this, tag, Scale, 0, price, UpBrush);
			} else if(Direction < 0) {
				if(GraphicType == RR_Filter_GraphicType.Arrow)
					Draw.ArrowDown(this, tag, Scale, 0, price, DownBrush);
				if(GraphicType == RR_Filter_GraphicType.Dot)
					Draw.Dot(this, tag, Scale, 0, price, DownBrush);
				if(GraphicType == RR_Filter_GraphicType.Triangle)
					Draw.TriangleDown(this, tag, Scale, 0, price, DownBrush);
			}
			#endregion
		}
//		private double zltema(IDataSeries input, int period){
//			double TEMA1 = TEMA(input, period)[0];
//			double TEMA2 = TEMA(TEMA(input, period), period)[0];
//            return(TEMA1 + (TEMA1 - TEMA2));
//		}
		/// <summary>
		/// Calculates the indicator value(s) at the current index.
		/// </summary>
		protected override void OnBarUpdate()
		{
			if(RunInit) {
				RunInit = false;
//				if(pFastMAType   == RR_Filter_MATypes.SMA)   fastma = SMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.SMA)   slowma = SMA(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.EMA)   
//				if(pSlowMAType   == RR_Filter_MATypes.EMA)   
//				if(pFastMAType   == RR_Filter_MATypes.HMA)   fastma = HMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.HMA)   slowma = HMA(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.TMA)   fastma = TMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.TMA)   slowma = TMA(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.TEMA)  fastma = TEMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.TEMA)  slowma = TEMA(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.WMA)   fastma = WMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.WMA)   slowma = WMA(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.LinReg)fastma = LinReg(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.LinReg)slowma = LinReg(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.ZeroLagEMA) fastma = ZLEMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.ZeroLagEMA) slowma = ZLEMA(Input, pSlow);
//				if(pFastMAType   == RR_Filter_MATypes.VWMA) fastma = VWMA(Input, pFast);
//				if(pSlowMAType   == RR_Filter_MATypes.VWMA) slowma = VWMA(Input, pSlow);
//				if(pFastMAType == RR_Filter_MATypes.ZeroLagTEMA) fastma = ZL_TEMA(Input, pFast);
//				if(pSlowMAType == RR_Filter_MATypes.ZeroLagTEMA) slowma = ZL_TEMA(Input, pSlow);
				BackgroundBrushUpTrend = pUpTrendBrush.Clone();//Color.FromArgb(alpha, pUpTrendBrush.R, pUpTrendBrush.G, pUpTrendBrush.B);
				BackgroundBrushUpTrend.Opacity = pOpacityUpTrend/10.0;
				BackgroundBrushUpTrend.Freeze();
				BackgroundBrushDownTrend = pDownTrendBrush.Clone();//Color.FromArgb(alpha, pDownTrendBrush.R, pDownTrendBrush.G, pDownTrendBrush.B);
				BackgroundBrushDownTrend.Opacity = pOpacityDownTrend/10.0;
				BackgroundBrushDownTrend.Freeze();
			}


			MacdLine[0] = (fastma[0]-slowma[0]);
//			if(pSignalMAType      == RR_Filter_MATypes.SMA)  signalline[0] = (SMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.EMA)  
				signalline[0] = (emaMACDLine[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.HMA)  signalline[0] = (HMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.TMA)  signalline[0] = (TMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.TEMA) signalline[0] = (TEMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.WMA)  signalline[0] = (WMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.LinReg)signalline[0] = (LinReg(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.ZeroLagEMA) signalline[0] = (ZLEMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.ZeroLagTEMA) signalline[0] = (ZL_TEMA(MacdLine, pSmooth)[0]);
//			else if(pSignalMAType == RR_Filter_MATypes.VWMA) signalline[0] = (VWMA(MacdLine, pSmooth)[0]);

			difference[0] = (MacdLine[0] - signalline[0]);

//			if(!IsOverlay){
//				Macd[0] = (MacdLine[0]);
//				Avg[0] = (signalline[0]);
//				Diff[0] = (difference[0]);
//			} else {
//				pDrawArrowsOnMACD = false;
//			}
//			if(CurrentBar>5) {
//				if(Diff[0]>Diff[1]) 
//					PlotBrushes[2][0] = pDiffUpBrush;
//				else 
//					PlotBrushes[2][0] = pDiffDownBrush;
//			}
//			if(pBkgColorBasis != RR_Filter_BkgColorBasis.None) 
			{
//				if(pBkgColorBasis == RR_Filter_BkgColorBasis.MACD_PositiveNegative){
//					BackColor = NormalBackgroundColor;
//					if(pOpacityUpTrend>0 && MacdLine[0]>0)   BackColor = BackgroundBrushUpTrend;
//					if(pOpacityDownTrend>0 && MacdLine[0]<0) BackColor = BackgroundBrushDownTrend;
//				}
//				else if(pBkgColorBasis == RR_Filter_BkgColorBasis.MACD_RisingFalling){
//					BackColor = NormalBackgroundColor;
//					if(pOpacityUpTrend>0 && MacdLine[0]>MacdLine[1])   BackColor = BackgroundBrushUpTrend;
//					if(pOpacityDownTrend>0 && MacdLine[0]<MacdLine[1]) BackColor = BackgroundBrushDownTrend;
//				}
//				else if(pBkgColorBasis == RR_Filter_BkgColorBasis.Signal_PositiveNegative){
//					BackColor = NormalBackgroundColor;
//					if(pOpacityUpTrend>0 && signalline[0]>0)   BackColor = BackgroundBrushUpTrend;
//					if(pOpacityDownTrend>0 && signalline[0]<0) BackColor = BackgroundBrushDownTrend;
//				}
//				else if(pBkgColorBasis == RR_Filter_BkgColorBasis.Signal_RisingFalling){
//					BackColor = NormalBackgroundColor;
//					if(pOpacityUpTrend>0 && signalline[0]>signalline[1])   BackColor = BackgroundBrushUpTrend;
//					if(pOpacityDownTrend>0 && signalline[0]<signalline[1]) BackColor = BackgroundBrushDownTrend;
//				}
//				else if(pBkgColorBasis == RR_Filter_BkgColorBasis.MACD_over_Signal)
				{
//					BackBrush = null;
//					if(pOpacityUpTrend>0 && MacdLine[0]>signalline[1])   BackBrush = BackgroundBrushUpTrend;
//					if(pOpacityDownTrend>0 && MacdLine[0]<signalline[1]) BackBrush = BackgroundBrushDownTrend;
				}
			}

			if(IsFirstTickOfBar) {
				AudioAlertsCount1 = 0;
				AudioAlertsCount2 = 0;
			}

			if(CurrentBar<4) return;

			if(difference[0]>difference[1])
				BackBrush = BackgroundBrushUpTrend;
			else
				BackBrush = BackgroundBrushDownTrend;

			if(pCrossingType1 != RR_Filter_Type.None) {
				if(Calculate != Calculate.OnBarClose && IsFirstTickOfBar && State != State.Historical) CleanupPriorBar(1,pCrossingType1);
				int Signal1 = CheckForSignal(pCrossingType1);
//				ArrowDirection1[0] = (Signal1);
				if(Signal1 != 0) {
					FireAlert(1, Signal1, pLaunchPopup1, ref BarAtLastPopup1, pSoundFileUp1, pSoundFileDown1, this.playAlertSound1, this.pMaxAudioAlerts1, ref this.AudioAlertsCount1, this.pSendEmails1, ref BarOfLastEmail1, this.pEmailAddress1, this.pCrossingType1.ToString(), this.pGraphicLocation1, this.pGraphicType1, this.pUpBrush1, this.pDownBrush1);
				}
			}
			if(pCrossingType2 != RR_Filter_Type.None) {
				if(Calculate != Calculate.OnBarClose && IsFirstTickOfBar && State != State.Historical) CleanupPriorBar(2,pCrossingType2);
//				Draw.TextFixed(this, "info",pCrossingType2.ToString(),TextPosition.TopRight);
				int Signal2 = CheckForSignal(pCrossingType2);
//				ArrowDirection2[0] = (Signal2);
				if(Signal2 != 0) {
					//BackColor=Color.Pink;
					FireAlert(2, Signal2, pLaunchPopup2, ref BarAtLastPopup2, pSoundFileUp2, pSoundFileDown2, this.playAlertSound2, this.pMaxAudioAlerts2, ref this.AudioAlertsCount2, this.pSendEmails2, ref BarOfLastEmail2, this.pEmailAddress2, this.pCrossingType2.ToString(), this.pGraphicLocation2, this.pGraphicType2, this.pUpBrush2, this.pDownBrush2); 
				}
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
		/// <summary>
		/// </summary>
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> Macd
//		{
//			get { return Values[0]; }
//		}
//		
//		/// <summary>
//		/// </summary>
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> Avg
//		{
//			get { return Values[1]; }
//		}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> Diff
//		{
//			get { return Values[2]; }
//		}
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> ArrowDirection1
//		{
//			get { return Values[3]; }
//		}
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> ArrowDirection2
//		{
//			get { return Values[4]; }
//		}
		#endregion

//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds",wav);
		}
//====================================================================
//=================================================================================
		#region Properties
		[Description("Number of bars for fast MA")]
		[Category("Parameters")]
		public int P1
		{
			get { return pFast; }
			set { pFast = Math.Max(1, value); }
		}

		[Description("Number of bars for slow MA")]
		[Category("Parameters")]
		public int P2
		{
			get { return pSlow; }
			set { pSlow = Math.Max(1, value); }
		}

		[Description("Number of bars for smoothing")]
		[Category("Parameters")]
		public int P3
		{
			get { return pSmooth; }
			set { pSmooth = Math.Max(1, value); }
		}
//		private RR_Filter_MATypes pFastMAType = RR_Filter_MATypes.EMA;
//		[Description("MAType for fast average")]
//		[Category("Parameters")]
//		public RR_Filter_MATypes FastMAType
//		{
//			get { return pFastMAType; }
//			set { pFastMAType = value; }
//		}
//		
//		private RR_Filter_MATypes pSlowMAType = RR_Filter_MATypes.EMA;
//		[Description("MAType for slow average")]
//		[Category("Parameters")]
//		public RR_Filter_MATypes SlowMAType
//		{
//			get { return pSlowMAType; }
//			set { pSlowMAType = value; }
//		}
//		private RR_Filter_MATypes pSignalMAType = RR_Filter_MATypes.EMA;
//		[Description("MAType for signal average")]
//		[Category("Parameters")]
//		public RR_Filter_MATypes SignalMAType
//		{
//			get { return pSignalMAType; }
//			set { pSignalMAType = value; }
//		}

//=================================================================================
	#region Alert 1
		private RR_Filter_GraphicLocation pGraphicLocation1 = RR_Filter_GraphicLocation.OnMACD;
//		[Description("Draw arrows on PriceChart or MACD panel?")]
//		[Category("Alert 1")]
//		public RR_Filter_GraphicLocation GraphicLocation1
//		{
//			get { return pGraphicLocation1; }
//			set { pGraphicLocation1 = value; }
//		}
//[Description("Max number of audio signals per bar, useful if CalculateOnBarClose = false")]
//		[Category("Alert 1")]
//		public int MaxAudioAlerts1
//		{
//			get { return pMaxAudioAlerts1; }
//			set { pMaxAudioAlerts1 = Math.Max(0, value); }
//		}
//		
//		[Description("Play alert sound?")]
//		[Category("Alert 1")]
//		public bool PlayAlertSound1
//		{
//			get { return playAlertSound1; }
//			set { playAlertSound1 = value; }
//		}
//
//		[Description("Sound file name for an upward cross - it must exist in your Sounds folder in order to be played")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadFileList))]
//		[Category("Alert 1")]
//		public string SoundFileUp1
//		{
//			get { return pSoundFileUp1; }
//			set { pSoundFileUp1 = value; }
//		}

//		[Description("Sound file name for a downward cross - it must exist in your Sounds folder in order to be played")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadFileList))]
//		[Category("Alert 1")]
//		public string SoundFileDown1
//		{
//			get { return pSoundFileDown1; }
//			set { pSoundFileDown1 = value; }
//		}

		private bool pSendEmails1 = false;
//		[Description("Enable email sends on signals?")]
//		[Category("Alert 1")]
//		public bool SendEmails1
//		{
//			get { return pSendEmails1; }
//			set { pSendEmails1 = value; }
//		}

		private string pEmailAddress1 = "";
//		[Description("Enter a valid destination email address to receive an email on signals")]
//		[Category("Alert 1")]
//		public string EmailAddress1
//		{
//			get { return pEmailAddress1; }
//			set { pEmailAddress1 = value; }
//		}
//
//		[Description("Launch a Popup window?")]
//		[Category("Alert 1")]
//		public bool LaunchPopup1
//		{
//			get { return pLaunchPopup1; }
//			set { pLaunchPopup1 = value; }
//		}

		private RR_Filter_GraphicType pGraphicType1 = RR_Filter_GraphicType.Arrow;
//		[Description("Type of symbol on crossing")]
//		[Category("Alert 1")]
//		public RR_Filter_GraphicType GraphicType1
//		{
//			get { return pGraphicType1; }
//			set { pGraphicType1 = value; }
//		}
//
//
//		[Description("What condition to signal the alert")]
//		[Category("Alert 1")]
//		public RR_Filter_Type CrossingType1
//		{
//			get { return pCrossingType1; }
//			set { pCrossingType1 = value; }
//		}
//
//		[XmlIgnore()]
//		[Description("Color of upward marker on Signal 1")]
//		[Category("Alert 1")]
//		[Gui.Design.DisplayNameAttribute("Marker Up Color")]
//		public Brush UpBrush1{	get { return pUpBrush1; }	set { pUpBrush1 = value; }		}
//		[Browsable(false)]
//		public string UClSerialize1
//		{	get { return Serialize.BrushToString(pUpBrush1); } set { pUpBrush1 = Serialize.StringToBrush(value); }
//		}
//
//		[XmlIgnore()]
//		[Description("")]
//		[Category("Alert 1")]
//		[Gui.Design.DisplayNameAttribute("Marker Down Color")]
//		public Brush DownBrush1{	get { return pDownBrush1; }	set { pDownBrush1 = value; }		}
//		[Browsable(false)]
//		public string DClSerialize1
//		{	get { return Serialize.BrushToString(pDownBrush1); } set { pDownBrush1 = Serialize.StringToBrush(value); }
//		}
	#endregion
	#region Alert 2
		private RR_Filter_GraphicLocation pGraphicLocation2 = RR_Filter_GraphicLocation.OnMACD;
//		[Description("Draw arrows on PriceChart or MACD panel?")]
//		[Category("Alert 2")]
//		public RR_Filter_GraphicLocation GraphicLocation2
//		{
//			get { return pGraphicLocation2; }
//			set { pGraphicLocation2 = value; }
//		}
//[Description("Max number of audio signals per bar, useful if CalculateOnBarClose = false")]
//		[Category("Alert 2")]
//		public int MaxAudioAlerts2
//		{
//			get { return pMaxAudioAlerts2; }
//			set { pMaxAudioAlerts2 = Math.Max(0, value); }
//		}
//		
//		[Description("Play alert sound?")]
//		[Category("Alert 2")]
//		public bool PlayAlertSound2
//		{
//			get { return playAlertSound2; }
//			set { playAlertSound2 = value; }
//		}
//
//		[Description("Sound file name for an upward cross - it must exist in your Sounds folder in order to be played")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadFileList))]
//		[Category("Alert 2")]
//		public string SoundFileUp2
//		{
//			get { return pSoundFileUp2; }
//			set { pSoundFileUp2 = value; }
//		}
//
//		[Description("Sound file name for a downward cross - it must exist in your Sounds folder in order to be played")]
//		[RefreshProperties(RefreshProperties.All)]
//		[TypeConverter(typeof(LoadFileList))]
//		[Category("Alert 2")]
//		public string SoundFileDown2
//		{
//			get { return pSoundFileDown2; }
//			set { pSoundFileDown2 = value; }
//		}

		private bool pSendEmails2 = false;
//		[Description("Enable email sends on signals?")]
//		[Category("Alert 2")]
//		public bool SendEmails2
//		{
//			get { return pSendEmails2; }
//			set { pSendEmails2 = value; }
//		}

		private string pEmailAddress2 = "";
//		[Description("Enter a valid destination email address to receive an email on signals")]
//		[Category("Alert 2")]
//		public string EmailAddress2
//		{
//			get { return pEmailAddress2; }
//			set { pEmailAddress2 = value; }
//		}
//
//		[Description("Launch a Popup window?")]
//		[Category("Alert 2")]
//		public bool LaunchPopup2
//		{
//			get { return pLaunchPopup2; }
//			set { pLaunchPopup2 = value; }
//		}

		private RR_Filter_GraphicType pGraphicType2 = RR_Filter_GraphicType.Arrow;
//		[Description("Type of symbol on crossing")]
//		[Category("Alert 2")]
//		public RR_Filter_GraphicType GraphicType2
//		{
//			get { return pGraphicType2; }
//			set { pGraphicType2 = value; }
//		}
//
//
//		[Description("What condition to signal the alert")]
//		[Category("Alert 2")]
//		public RR_Filter_Type CrossingType2
//		{
//			get { return pCrossingType2; }
//			set { pCrossingType2 = value; }
//		}
//
//		[XmlIgnore()]
//		[Description("Color of upward marker on Signal 2")]
//		[Category("Alert 2")]
//		[Gui.Design.DisplayNameAttribute("Marker Up Color")]
//		public Brush UpBrush2{	get { return pUpBrush2; }	set { pUpBrush2 = value; }		}
//		[Browsable(false)]
//		public string UClSerialize2
//		{	get { return Serialize.BrushToString(pUpBrush2); } set { pUpBrush2 = Serialize.StringToBrush(value); }
//		}
//
//		[XmlIgnore()]
//		[Description("")]
//		[Category("Alert 2")]
//		[Gui.Design.DisplayNameAttribute("Marker Down Color")]
//		public Brush DownBrush2{	get { return pDownBrush2; }	set { pDownBrush2 = value; }		}
//		[Browsable(false)]
//		public string DClSerialize2
//		{	get { return Serialize.BrushToString(pDownBrush2); } set { pDownBrush2 = Serialize.StringToBrush(value); }
//		}
	#endregion
//=================================================================================

		//private RR_Filter_BkgColorBasis pBkgColorBasis = RR_Filter_BkgColorBasis.MACD_over_Signal;
//		[Description("Basis for coloring the background")]
//		[Category("Background Visual")]
//		public RR_Filter_BkgColorBasis BkgColorBasis
//		{
//			get { return pBkgColorBasis; }
//			set { pBkgColorBasis = value; }
//		}
		private int pOpacityUpTrend = 10;
		[Description("Opacity (0=transparent, 10=opaque) of background Up color")]
		[Category("Background Visual")]
		public int OpacityUpTrend
		{
			get { return pOpacityUpTrend; }
			set { pOpacityUpTrend = Math.Max(0, Math.Min(10,value)); }
		}
		private Brush pUpTrendBrush = Brushes.PaleGreen;
		[XmlIgnore()]
		[Description("Color of rising filter")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Color",  GroupName = "Background Visual")]
		public Brush TrendUpBrush{	get { return pUpTrendBrush; }	set { pUpTrendBrush = value; }		}
		[Browsable(false)]
		public string TrendUClSerialize
		{	get { return Serialize.BrushToString(pUpTrendBrush); } set { pUpTrendBrush = Serialize.StringToBrush(value); }
		}

		private int pOpacityDownTrend = 10;
		[Description("Opacity (0=transparent, 10=opaque) of background Down color")]
		[Category("Background Visual")]
		public int OpacityDownTrend
		{
			get { return pOpacityDownTrend; }
			set { pOpacityDownTrend = Math.Max(0, Math.Min(10,value)); }
		}
		private Brush pDownTrendBrush = Brushes.Salmon;
		[XmlIgnore()]
		[Description("Color of falling filter")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Color",  GroupName = "Background Visual")]
		public Brush TrendDownBrush{	get { return pDownTrendBrush; }	set { pDownTrendBrush = value; }		}
		[Browsable(false)]
		public string TrendDClSerialize
		{	get { return Serialize.BrushToString(pDownTrendBrush); } set { pDownTrendBrush = Serialize.StringToBrush(value); }
		}


		private Brush pDiffUpBrush = Brushes.Green;
//		[XmlIgnore()]
//		[Description("Color of MACD Diff Histo if its value has increased since last bar")]
//		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("Diff Up Color")]
//		public Brush DiffUpBrush{	get { return pDiffUpBrush; }	set { pDiffUpBrush = value; }		}
//		[Browsable(false)]
//		public string DiffUClSerialize
//		{	get { return Serialize.BrushToString(pDiffUpBrush); } set { pDiffUpBrush = Serialize.StringToBrush(value); }
//		}
		private Brush pDiffDownBrush = Brushes.Red;
//		[XmlIgnore()]
//		[Description("Color of MACD Diff Histo if its value has decreased since last bar")]
//		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("Diff Down Color")]
//		public Brush DiffDownBrush{	get { return pDiffDownBrush; }	set { pDiffDownBrush = value; }		}
//		[Browsable(false)]
//		public string DiffDClSerialize
//		{	get { return Serialize.BrushToString(pDiffDownBrush); } set { pDiffDownBrush = Serialize.StringToBrush(value); }
//		}

		#endregion
	}
}
	public enum RR_Filter_Type {
		MACDxSignal,
		MACDxZero,
		SignalxZero,
		MACD_DirectionChange,
		Signal_DirectionChange,
		None
	}
	public enum RR_Filter_GraphicType {
		Arrow,
		Dot,
		Triangle
	}
	public enum RR_Filter_GraphicLocation {
		OnPriceChart,
		OnMACD
	}
//	public enum RR_Filter_BkgColorBasis {
//		MACD_PositiveNegative,
//		Signal_PositiveNegative,
//		MACD_RisingFalling,
//		Signal_RisingFalling,
//		MACD_over_Signal,
//		None
//	}

//	public enum RR_Filter_MATypes {
////		SMA,
////		WMA,
////		HMA,
////		EMA,
////		TMA,
////		TEMA
//		SMA,
//		EMA,
//		HMA,
//		TEMA,
//		TMA,
//		WMA,
//		LinReg,
//		ZeroLagEMA,
//		ZeroLagTEMA,
//		VWMA
//	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RR_Filter[] cacheRR_Filter;
		public RR_Filter RR_Filter()
		{
			return RR_Filter(Input);
		}

		public RR_Filter RR_Filter(ISeries<double> input)
		{
			if (cacheRR_Filter != null)
				for (int idx = 0; idx < cacheRR_Filter.Length; idx++)
					if (cacheRR_Filter[idx] != null &&  cacheRR_Filter[idx].EqualsInput(input))
						return cacheRR_Filter[idx];
			return CacheIndicator<RR_Filter>(new RR_Filter(), input, ref cacheRR_Filter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RR_Filter RR_Filter()
		{
			return indicator.RR_Filter(Input);
		}

		public Indicators.RR_Filter RR_Filter(ISeries<double> input )
		{
			return indicator.RR_Filter(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RR_Filter RR_Filter()
		{
			return indicator.RR_Filter(Input);
		}

		public Indicators.RR_Filter RR_Filter(ISeries<double> input )
		{
			return indicator.RR_Filter(input);
		}
	}
}

#endregion
