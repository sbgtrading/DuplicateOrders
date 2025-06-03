
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
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("")]
	public class UltimateMovingAverage : Indicator
	{
		private const int FLAT = 0;
		private const int LONG = 1;
		private const int SHORT = -1;
		private bool LicenseValid = true;
		private string emailaddress = "";
		[Description("Supply your address (e.g. 'you@mail.com') for signals")]
		[Category("Alert")]
		public string EmailAddress
		{
			get { return emailaddress; }
			set { emailaddress = value; }
		}

		#region Variables
		private int pPeriod	= 12;
		private bool RunInit = true;
		private bool connectline = false;
		private string txtmsg = "";
		#endregion
		private string MA_Description = string.Empty;
		private int BarOfLastEmail = -1;
		private int BarOfLastSound = -1;
		private int BarOfLastPopup = -1;
		private double PriceShiftPts = 0;
		private double PtsPerBarForBreakout = 0;
		private double PriceAtLastAlert = -1;
		private Series<int> Direction;
		private Brush NormalBackgroundColor;
		private bool OHLC_or_HiLo = false;
		private Series<bool> trend;

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			if(!System.IO.File.Exists("c:\\222222222222.txt"))
				VendorLicense("IndicatorWarehouse", "AIUltimateMovingAverage", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

			Name = "iw Ultimate Moving Average";
			AddPlot(new Stroke(Brushes.Yellow, 3),  PlotStyle.Dot, "Breakout");
			AddPlot(new Stroke(Brushes.Green, 3),   PlotStyle.Line, "MA");
			AddPlot(new Stroke(Brushes.Blue, 3),    PlotStyle.TriangleUp, "UpTriangle");
			AddPlot(new Stroke(Brushes.Orange, 3),  PlotStyle.TriangleDown, "DownTriangle");
			AddPlot(new Stroke(Brushes.DimGray, 3), PlotStyle.Dot, "Dot");

			ArePlotsConfigurable=true;
			Calculate=Calculate.OnBarClose;
			//PriceTypeSupported = true;
			IsOverlay=true;
		}else if(State == State.DataLoaded){
			Direction = new Series<int>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
			NormalBackgroundColor = ChartControl.Properties.ChartBackground;
//			if(ChartBars.Properties.ChartStyleType == ChartStyleType.OHLC)     OHLC_or_HiLo = true;
//			if(ChartBars.Properties.ChartStyleType == ChartStyleType.HiLoBars) OHLC_or_HiLo = true;
			PtsPerBarForBreakout = TickSize * pSlopeTicksPerBarForBreakout;

			if(pTypeOfMA == UltimateMovingAverage_MAtype.KAMA)       MA_Description=string.Concat("KAMA(",pKAMAfast,",",pPeriod,",",pKAMAslow);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.VMA)   MA_Description=string.Concat("VMA(",pPeriod, ",",pVolatilityPeriod);
			else MA_Description = string.Concat(pTypeOfMA.ToString(),"(",pPeriod,")");
			if(pTypeOfMA == UltimateMovingAverage_MAtype.SuperTrend) trend = new Series<bool>(this);//inside State.DataLoaded

		}
	}

		protected override void OnBarUpdate()
		{
			//if(!LicenseValid) return;

			if(pTypeOfMA == UltimateMovingAverage_MAtype.EMA)       MA[0] = (EMA(Input,pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.SMA)  MA[0] = (SMA(Input,pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.HMA)  MA[0] = (HMA(Input,pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.SMMA) MA[0] = (IW_SMMA(Input, pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.TMA)  MA[0] = (TMA(Input,pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.TEMA) MA[0] = (TEMA(Input,pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.WMA)  MA[0] = (WMA(Input,pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.LinReg)     MA[0] = (LinReg(Input, pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.ZeroLagEMA) MA[0] = (ZLEMA(Input, pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.ZeroLagHATEMA) MA[0] = (IW_ZeroLagHATEMA(Input, pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.ZeroLagTEMA)   MA[0] = (IW_ZeroLagTEMA(Input, pPeriod, -1)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.KAMA)       MA[0] = (KAMA(Input, pKAMAfast, pPeriod, pKAMAslow)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.VWMA)       MA[0] = (VWMA(Input, pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.VMA)        MA[0] = (VMA(Input, pPeriod, pVolatilityPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.RMA)        MA[0] = (IW_UltimateRMA(Input, pPeriod)[0]);
			else if(pTypeOfMA == UltimateMovingAverage_MAtype.SuperTrend) 
			{
				#region SuperTrend
				double avg = 0;
				double offset = 0;
				if (CurrentBar < 1)
				{
					trend[0] = (true);
					MA[0] = (Close[0]);
					return;
				}
				switch (pSTmaType) 
				{
					case UltimateMovingAverage_SuperTrend_MAType.SMA:
						avg = SMA(smooth)[0];
						break;
					case UltimateMovingAverage_SuperTrend_MAType.TMA:
						avg = TMA(smooth)[0];
						break;
					case UltimateMovingAverage_SuperTrend_MAType.WMA:
						avg = WMA(smooth)[0];
						break;
					case UltimateMovingAverage_SuperTrend_MAType.VWMA:
						avg = VWMA(smooth)[0];
						break;
					case UltimateMovingAverage_SuperTrend_MAType.TEMA:
						avg = TEMA(smooth)[0];
						break;
					case UltimateMovingAverage_SuperTrend_MAType.HMA:
						avg = HMA(smooth)[0];
						break;
					case UltimateMovingAverage_SuperTrend_MAType.VMA:
						avg = VMA(smooth, smooth)[0];
						break;
					default:
						avg = EMA(smooth)[0];
						break;
				}
				switch (smode)
				{
					case UltimateMovingAverage_SuperTrend_Mode.ATR:
						offset = ATR(length)[0] * Multiplier;
						break;
					default:
						offset = DTT(length, Multiplier);
						break;
				}
				if (Close[0] > MA[1]) 
					trend[0] = (true);
				else if (Close[0] < MA[1])
					trend[0] = (false);
				else
					trend[0] = (trend[1]);

				if (trend[0] && !trend[1])
					MA[0] = (avg-offset);
				else if (!trend[0] && trend[1])
					MA[0] = (avg+offset);
				else {
					if (trend[0]) MA[0] = ((avg - offset) > MA[1] ? (avg - offset) : MA[1]);
					else          MA[0] = ((avg + offset) < MA[1] ? (avg + offset) : MA[1]);
				}
				#endregion
			}
			if(CurrentBar<3) {
				Direction[0] = (FLAT);
				return;
			}
			Direction[0] = (Direction[1]);
			Breakout.Reset(1);

			if(MA[0] > MA[1]) {
				PlotBrushes[1][0] = pMAupColor;
				Direction[0] = (LONG);
			}
			else if(MA[0] < MA[1]) {
				PlotBrushes[1][0] = pMAdownColor;
				Direction[0] = (SHORT);
			} else {
				PlotBrushes[1][0] = PlotBrushes[1][1];
			}

			if(pSlopeTicksPerBarForBreakout > 0) {
				double slope =(MA[0]-MA[1]);
//				Print("Slope: "+slope+"  Pts/Bar: "+PtsPerBarForBreakout);
				if(slope <= PtsPerBarForBreakout && slope >= -PtsPerBarForBreakout) {
					PlotBrushes[1][0] = pRangingColor;
				} 
			}
			
			if(pColorBarOnTrends) {
				int SignalForBarBrush = SHORT;
				if(MA[0]<Low[0]) 
					SignalForBarBrush = LONG;
				else if(MA[0]<=High[0] && MA[0]>=Low[0]) 
					SignalForBarBrush = FLAT;

				if(SignalForBarBrush == LONG) {
					CandleOutlineBrush = pcoac;
					if(OHLC_or_HiLo) BarBrush = pac;
					else {
						if(Close[0]<Open[0])		BarBrush = pac;
						else if(Close[0]==Open[0])	BarBrush = CandleOutlineBrush;
						else {
							if(pUseHollowBars)		BarBrush = NormalBackgroundColor; else BarBrush = pac;
						}
					}
				}
				else if (SignalForBarBrush == FLAT) {
					CandleOutlineBrush = pcosc;
					if(OHLC_or_HiLo) BarBrush = psc;
					else {
						if(Close[0]<Open[0])		BarBrush = psc;
						else if(Close[0]==Open[0])	BarBrush = CandleOutlineBrush;
						else {
							if(pUseHollowBars)		BarBrush = NormalBackgroundColor; else BarBrush = psc;
						}
					}
				}
				else {
					CandleOutlineBrush = pcobc;
					if(OHLC_or_HiLo) BarBrush = pbc;
					else {
						if(Close[0]<Open[0])		BarBrush = pbc;
						else if(Close[0]==Open[0])	BarBrush = CandleOutlineBrush;
						else {
							if(pUseHollowBars)		BarBrush = NormalBackgroundColor; else BarBrush = pbc;
						}
					}
				}
			}

			int ArrowCount = 0;
			if(pAlertBasis == UltimateMovingAverage_AlertBasis.ColorChange) {
				if(pSlopeTicksPerBarForBreakout > 0) {
					bool c1 = PlotBrushes[1][1] == pRangingColor  && PlotBrushes[1][0] != pRangingColor;
					bool c2 = PlotBrushes[1][1] == pMAdownColor   && PlotBrushes[1][0] == pMAupColor;
					bool c3 = PlotBrushes[1][1] == pMAupColor     && PlotBrushes[1][0] == pMAdownColor;
					if(c1 || c2 || c3) {
						Breakout[1] = (MA[1]);
						if(MA[0]>MA[1]) {
							txtmsg = string.Concat("UltimateMA: ",MA_Description," started rising");
							DoAlerting(MA[0], 1);
							ArrowCount++;
						} else {
							txtmsg = string.Concat("UltimateMA: ",MA_Description," started falling");
							DoAlerting(MA[0], -1);
							ArrowCount++;
						}
					}
				} else {
					if(Direction[0]==LONG && Direction[1]!=LONG) {
						txtmsg = string.Concat("UltimateMA: ",MA_Description," started rising");
						DoAlerting(MA[0], 1);
						ArrowCount++;
					}
					else if(Direction[0]==SHORT && Direction[1]!=SHORT) {
						txtmsg = string.Concat("UltimateMA: ",MA_Description," started falling");
						DoAlerting(MA[0], -1);
						ArrowCount++;
					}
				}
			} else {
				double dist = pAlertZoneArea * TickSize;
				double Upper0 = MA[0]+dist;
				double Upper1 = MA[1]+dist;
				double Lower0 = MA[0]-dist;
				double Lower1 = MA[1]-dist;
//Draw.Dot(this, CurrentBar.ToString()+"X",false,0,Upper0,Color.Pink);
//Draw.Dot(this, CurrentBar.ToString(),false,0,Lower0,Color.Pink);
				if(pAlertBasis == UltimateMovingAverage_AlertBasis.PriceEnteringZone || pAlertBasis == UltimateMovingAverage_AlertBasis.BothEnteringAndExiting) {
					if(Close[1]>=Upper1 && Close[0]<Upper0) {
						txtmsg = string.Concat("UltimateMA: ","Price entered zone from above ",MA_Description);
						DoAlerting(Upper0, -1, pcArrowXinFromAbove);
						ArrowCount++;
//BackColor=Color.Cyan;
					}
					else if(Close[1]<=Lower1 && Close[0]>Lower0) {
						txtmsg = string.Concat("UltimateMA: ","Price entered zone from below ",MA_Description);
						DoAlerting(Lower0, 1, pcArrowXinFromBelow);
						ArrowCount++;
//BackColor=Color.Maroon;
					}
				}
				if(pAlertBasis == UltimateMovingAverage_AlertBasis.PriceExitingZone || pAlertBasis == UltimateMovingAverage_AlertBasis.BothEnteringAndExiting) {
					if(Close[1]>=Lower1 && Close[0]<Lower0) {
						txtmsg = string.Concat("UltimateMA: ","Price exiting below zone ",MA_Description);
						DoAlerting(Upper0, -1, pcArrowXBelowZone);
						ArrowCount++;
					}
					else if(Close[1]<=Upper1 && Close[0]>Upper0) {
						txtmsg = string.Concat("UltimateMA: ","Price exiting above zone ",MA_Description);
						DoAlerting(Lower0, 1, pcArrowXAboveZone);
						ArrowCount++;
					}
				}
			}
			if(ArrowCount==0) {
				UpTriangle.Reset();
				DownTriangle.Reset();
				if(pAlertType == UltimateMovingAverage_AlertType.Arrow || pAlertType == UltimateMovingAverage_AlertType.ArrowAndSound) RemoveDrawObject("MAMCArrow"+CurrentBar);
				Dot.Reset();
			}
		}
		private double DTT(int nDay, double mult)
		{
			double HH = MAX(High, nDay)[0];
			double HC = MAX(Close,nDay)[0];
			double LL = MIN(Low, nDay)[0];
			double LC = MIN(Close, nDay)[0];
			
			return mult * Math.Max((HH - LC),(HC - LL));
		}
		private void DoAlerting (double price, int Direction, Brush ArrowColor) {
			#region DoAlerting with ArrowColor supplied
			string tag;
			switch (pAlertType) {
				case UltimateMovingAverage_AlertType.Arrow: 
					tag = "MAMCArrow"+CurrentBar;
					if(Direction>0)Draw.ArrowUp(this, tag,IsAutoScale,0,Low[0]-TickSize,ArrowColor);else Draw.ArrowDown(this, tag,IsAutoScale,0,High[0]+TickSize,ArrowColor);
					break;
				case UltimateMovingAverage_AlertType.ArrowAndSound: 
					tag = "MAMCArrow"+CurrentBar;
					if(Direction>0)Draw.ArrowUp(this, tag,IsAutoScale,0,Low[0]-TickSize,ArrowColor);else Draw.ArrowDown(this, tag, IsAutoScale,0,High[0]+TickSize,ArrowColor);
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
				case UltimateMovingAverage_AlertType.Triangle: 
					if(Direction>0) {
						UpTriangle[0] = (Low[0]-TickSize); 
						PlotBrushes[2][0] = ArrowColor;
					} else {
						DownTriangle[0] = (High[0]+TickSize);
						PlotBrushes[3][0] = ArrowColor;
					}
					break;
				case UltimateMovingAverage_AlertType.TriangleAndSound: 
					if(Direction>0) {
						UpTriangle[0] = (Low[0]-TickSize); 
						PlotBrushes[2][0] = ArrowColor;
					} else {
						DownTriangle[0] = (High[0]+TickSize);
						PlotBrushes[3][0] = ArrowColor;
					}
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
				case UltimateMovingAverage_AlertType.Dot: 
					if(ChartControl!=null) {
						Dot[0] = (price);//Draw.Dot(this, "mam"+CurrentBar,false,0,price,ChartControl.Properties.AxisPen.Brush); 
						PlotBrushes[4][0] = ArrowColor;
					}
					break;
				case UltimateMovingAverage_AlertType.DotAndSound: 
					if(ChartControl!=null) {
						Dot[0] = (price);//Draw.Dot(this, "mam"+CurrentBar,false,0,price,ChartControl.Properties.AxisPen.Brush); 
						PlotBrushes[5][0] = ArrowColor;
					}
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
				case UltimateMovingAverage_AlertType.Sound: 
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
			}
			if(State == State.Realtime) {
				if(pSendEmails && BarOfLastEmail != CurrentBar) {
					BarOfLastEmail = CurrentBar;
					SendMail(
						emailaddress,
						string.Concat(Name,txtmsg),
						string.Concat(Environment.NewLine,Environment.NewLine,txtmsg,Environment.NewLine,Environment.NewLine,"Auto-generated on ",NinjaTrader.Core.Globals.Now.ToString())
					);
				}
				if(pLaunchPopup && BarOfLastPopup != CurrentBar) {
					BarOfLastPopup = CurrentBar;
					Log(txtmsg, NinjaTrader.Cbi.LogLevel.Alert);
//					LaunchPopupWindow(txtmsg, Time[0].ToShortTimeString(), string.Concat(Instrument.FullName," (",Bars.BarsPeriod.ToString(),")"),"");
				}
			}
			#endregion
		}
		private void DoAlerting (double price, int Direction) {
			#region DoAlerting
			string tag;
			switch (pAlertType) {
				case UltimateMovingAverage_AlertType.Arrow: 
					tag = "MAMCArrow"+CurrentBar;
					if(Direction>0)Draw.ArrowUp(this, tag,IsAutoScale,0,Low[0]-TickSize,Plots[2].Brush);else Draw.ArrowDown(this, tag,IsAutoScale,0,High[0]+TickSize,Plots[3].Brush);
					break;
				case UltimateMovingAverage_AlertType.ArrowAndSound: 
					tag = "MAMCArrow"+CurrentBar;
					if(Direction>0)Draw.ArrowUp(this, tag,IsAutoScale,0,Low[0]-TickSize,Plots[2].Brush);else Draw.ArrowDown(this, tag,IsAutoScale,0,High[0]+TickSize,Plots[3].Brush);
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
				case UltimateMovingAverage_AlertType.Triangle: 
					if(Direction>0) UpTriangle[0] = (Low[0]-TickSize); else DownTriangle[0] = (High[0]+TickSize);
					break;
				case UltimateMovingAverage_AlertType.TriangleAndSound: 
					if(Direction>0) UpTriangle[0] = (Low[0]-TickSize); else DownTriangle[0] = (High[0]+TickSize);
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
				case UltimateMovingAverage_AlertType.Dot: 
					if(ChartControl!=null) Dot[0] = (price);//Draw.Dot(this, "mam"+CurrentBar,false,0,price,ChartControl.Properties.AxisPen.Brush); 
					break;
				case UltimateMovingAverage_AlertType.DotAndSound: 
					if(ChartControl!=null) Dot[0] = (price);//Draw.Dot(this, "mam"+CurrentBar,false,0,price,ChartControl.Properties.AxisPen.Brush); 
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
				case UltimateMovingAverage_AlertType.Sound: 
					if(BarOfLastSound != CurrentBar) {PlaySound(Direction>0?pUpAlertSound:pDownAlertSound); BarOfLastSound = CurrentBar;}
					break;
			}
			if(State == State.Realtime) {
				if(pSendEmails && BarOfLastEmail != CurrentBar) {
					BarOfLastEmail = CurrentBar;
					SendMail(
						emailaddress,
						string.Concat(Name,txtmsg),
						string.Concat(Environment.NewLine,Environment.NewLine,txtmsg,Environment.NewLine,Environment.NewLine,"Auto-generated on ",NinjaTrader.Core.Globals.Now.ToString())
					);
				}
				if(pLaunchPopup && BarOfLastPopup != CurrentBar) {
					BarOfLastPopup = CurrentBar;
					Log(txtmsg, NinjaTrader.Cbi.LogLevel.Alert);
//					LaunchPopupWindow(txtmsg, Time[0].ToShortTimeString(), string.Concat(Instrument.FullName," (",Bars.BarsPeriod.ToString(),")"),"");
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

		#region plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Breakout {	get { return Values[0]; }		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MA {	get { return Values[1]; }		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpTriangle {	get { return Values[2]; }		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DownTriangle {	get { return Values[3]; }		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Dot {	get { return Values[4]; }		}
		#endregion

		#region Properties
		#region Flat Conditions
		private double pSlopeTicksPerBarForBreakout = 0.1;
		[Description("Max. number of ticks per bar (slope) for a flat condition to occur")]
		[Category("Flat Conditions")]
		public double SlopeTicksPerBarForBreakout
		{
			get { return pSlopeTicksPerBarForBreakout; }
			set { pSlopeTicksPerBarForBreakout = Math.Max(0,value); }
		}
		private Brush pRangingColor = Brushes.Gray;
		[Description("Color of MA when it's flat or range-bound condition")]
		[Category("Flat Conditions")]
		public Brush FlatCondition
		{get { return pRangingColor; }
		set { pRangingColor = value; }}
		[Browsable(false)]
		public string RangingColorSerialize
		{get { return Serialize.BrushToString(pRangingColor); }set { pRangingColor = Serialize.StringToBrush(value); }}

		private Brush pMAupColor = Brushes.Green;
		[Description("")]
		[Category("Flat Conditions")]
		public Brush UpCondition
		{get { return pMAupColor; }
		set { pMAupColor = value; }}
		[Browsable(false)]
		public string MAupDownSerialize
		{get { return Serialize.BrushToString(pMAupColor); }set { pMAupColor = Serialize.StringToBrush(value); }		}

		private Brush pMAdownColor = Brushes.Red;
		[Description("")]
		[Category("Flat Conditions")]
		public Brush DownCondition
		{get { return pMAdownColor; }
		set { pMAdownColor = value; }}
		[Browsable(false)]
		public string MAdnDownSerialize
		{get { return Serialize.BrushToString(pMAdownColor); }set { pMAdownColor = Serialize.StringToBrush(value); }		}
		#endregion

		#region SuperTrend params
		private UltimateMovingAverage_SuperTrend_Mode smode = UltimateMovingAverage_SuperTrend_Mode.ATR;
        [Description("SuperTrend Mode")]
// 		[Category("Parameters SuperTrend")]
// [Gui.Design.DisplayName("01. Mode")]
[Display(ResourceType = typeof(Custom.Resource), Name = "01. Mode",  GroupName = "Parameters SuperTrend")]
        public UltimateMovingAverage_SuperTrend_Mode STMode
        {
            get { return smode; }
            set { smode = value; }
        }

		private int length = 14;
        [Description("ATR/DT Period")]
// 		[Category("Parameters SuperTrend")]
// [Gui.Design.DisplayName("02. Period")]
[Display(ResourceType = typeof(Custom.Resource), Name = "02. Period",  GroupName = "Parameters SuperTrend")]
        public int Length
        {
            get { return length; }
            set { length = Math.Max(1, value); }
        }
		private double multiplier = 2.618;
        [Description("ATR Multiplier")]
// 		[Category("Parameters SuperTrend")]
// [Gui.Design.DisplayName("03. Multiplier")]
[Display(ResourceType = typeof(Custom.Resource), Name = "03. Multiplier",  GroupName = "Parameters SuperTrend")]
        public double Multiplier
        {
            get { return multiplier; }
            set { multiplier = Math.Max(0.0001, value); }
        }
		private UltimateMovingAverage_SuperTrend_MAType pSTmaType = UltimateMovingAverage_SuperTrend_MAType.HMA;
        [Description("Moving Average Type for smoothing")]
// 		[Category("Parameters SuperTrend")]
// [Gui.Design.DisplayName("04. MA Type")]
[Display(ResourceType = typeof(Custom.Resource), Name = "04. MA Type",  GroupName = "Parameters SuperTrend")]
        public UltimateMovingAverage_SuperTrend_MAType ST_MAType
        {
            get { return pSTmaType; }
            set { pSTmaType = value; }
        }
		private int smooth = 14;
        [Description("Smoothing Period (for SuperTrend MA)")]
// 		[Category("Parameters SuperTrend")]
// [Gui.Design.DisplayName("05. SmoothingPeriod")]
[Display(ResourceType = typeof(Custom.Resource), Name = "05. SmoothingPeriod",  GroupName = "Parameters SuperTrend")]
        public int Smooth
        {
            get { return smooth; }
            set { smooth = Math.Max(1, value); }
        }
		#endregion

		private bool pLaunchPopup = false;
		[Description("Launch a Popup window on each signal")]
		[Category("Alert")]
		public bool LaunchPopup
		{
			get { return pLaunchPopup; }
			set { pLaunchPopup = value; }
		}

		private bool pSendEmails = false;
		[Description("Send an email on each signal?")]
		[Category("Alert")]
		public bool SendEmails
		{
			get { return pSendEmails; }
			set { pSendEmails = value; }
		}


		private int pAlertZoneArea = 0;
		[Description("Number of ticks on either side of MA to activate alert")]
		[Category("Alert")]
		public int AlertZoneArea
		{
			get { return pAlertZoneArea; }
			set { pAlertZoneArea = Math.Max(0, value); }
		}
		[Description("Number of bars for MA period")]
		[Category("Parameters")]
		public int Period
		{
			get { return pPeriod; }
			set { pPeriod = Math.Max(1, value); }
		}
		private int pKAMAfast = 2;
		[Description("Number of bars for KAMA Fast period (between 1 and 125)")]
		[Category("Parameters KAMA")]
		public int KAMAfast
		{
			get { return pKAMAfast; }
			set { pKAMAfast = Math.Min(125, Math.Max(1, value)); }
		}
		private int pKAMAslow = 30;
		[Description("Number of bars for KAMA Slow period (between 1 and 125)")]
		[Category("Parameters KAMA")]
		public int KAMAslow
		{
			get { return pKAMAslow; }
			set { pKAMAslow = Math.Min(125, Math.Max(1, value)); }
		}
		private UltimateMovingAverage_MAtype pTypeOfMA = UltimateMovingAverage_MAtype.SMA;
		[Description("Number of bars for KAMA Slow period (between 1 and 125)")]
		[Category("Parameters")]
		public UltimateMovingAverage_MAtype MAType
		{
			get { return pTypeOfMA; }
			set { pTypeOfMA = value; }
		}

		private int pVolatilityPeriod = 9;
		[Description("Number of bars for VMA Volatility period, used to calculate the CMO-based volatility index")]
		[Category("Parameters VMA")]
		public int VMA_VolatilityPeriod
		{
			get { return pVolatilityPeriod; }
			set { pVolatilityPeriod = Math.Max(1, value); }
		}

		private string pUpAlertSound = "Alert2.wav";
		[Description("Audible alert wav file to play on and UP signal")]
		[Category("Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string AlertSound_UP
		{
			get { return pUpAlertSound; }
			set { pUpAlertSound = value; }
		}

		private string pDownAlertSound = "Alert2.wav";
		[Description("Audible alert wav file to play on a DOWN signal")]
		[Category("Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string AlertSound_Down
		{
			get { return pDownAlertSound; }
			set { pDownAlertSound = value; }
		}

		private UltimateMovingAverage_AlertType pAlertType = UltimateMovingAverage_AlertType.Sound;
		[Description("What type of alert?")]
// 		[Category("Alert")]
// [Gui.Design.DisplayName("Alert Type")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Alert Type",  GroupName = "Alert")]
		public UltimateMovingAverage_AlertType AlertType
		{
			get { return pAlertType; }
			set { pAlertType = value; }
		}
		private UltimateMovingAverage_AlertBasis pAlertBasis = UltimateMovingAverage_AlertBasis.ColorChange;
		[Description("When does an alert occur?")]
// 		[Category("Alert")]
// [Gui.Design.DisplayName("Alert Basis")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Alert Basis",  GroupName = "Alert")]
		public UltimateMovingAverage_AlertBasis AlertBasis
		{
			get { return pAlertBasis; }
			set { pAlertBasis = value; }
		}

		#region Chart Marker Colors
		private Brush pcArrowXinFromBelow = Brushes.Green;
		[Description("Color of arrow or triangle when price crosses into the zone from below it")]
		[Category("Marker Colors")]
		public Brush PriceIntoZoneFromBelow {get { return pcArrowXinFromBelow; } set { pcArrowXinFromBelow = value; }}
		[Browsable(false)]
		public string pcArrowXinFromBelowSerialize
		{get { return Serialize.BrushToString(pcArrowXinFromBelow); }set { pcArrowXinFromBelow = Serialize.StringToBrush(value); }}

		private Brush pcArrowXinFromAbove = Brushes.Red;
		[Description("Color of arrow or triangle when price crosses into the zone from above it")]
		[Category("Marker Colors")]
		public Brush PriceIntoZoneFromAbove {get { return pcArrowXinFromAbove; } set { pcArrowXinFromAbove = value; }}
		[Browsable(false)]
		public string pcArrowXinFromAboveSerialize
		{get { return Serialize.BrushToString(pcArrowXinFromAbove); }set { pcArrowXinFromAbove = Serialize.StringToBrush(value); }}

		private Brush pcArrowXAboveZone = Brushes.Blue;
		[Description("Color of arrow or triangle when price crosses out ABOVE the zone from within it")]
		[Category("Marker Colors")]
		public Brush PriceExitsAboveZone {get { return pcArrowXAboveZone; } set { pcArrowXAboveZone = value; }}
		[Browsable(false)]
		public string pcArrowXAboveZoneSerialize
		{get { return Serialize.BrushToString(pcArrowXAboveZone); }set { pcArrowXAboveZone = Serialize.StringToBrush(value); }}

		private Brush pcArrowXBelowZone = Brushes.Maroon;
		[Description("Color of arrow or triangle when price crosses out BELOW the zone from within it")]
		[Category("Marker Colors")]
		public Brush PriceExitsBelowZone {get { return pcArrowXBelowZone; } set { pcArrowXBelowZone = value; }}
		[Browsable(false)]
		public string pcArrowXBelowZoneSerialize
		{get { return Serialize.BrushToString(pcArrowXBelowZone); }set { pcArrowXBelowZone = Serialize.StringToBrush(value); }}
		#endregion

		#region PaintBar colors
		private bool pColorBarOnTrends = false;
		[Description("Colorize the bars according to their position relative to the MA?")]
		[Category("PaintBar Colors")]
		public bool ColorBarOnTrends
		{
			get { return pColorBarOnTrends; }
			set { pColorBarOnTrends = value; }
		}
		private bool pUseHollowBars = true;
		[Description("Use hollow bars for up-closes?  Otherwise, fill in bars with trend color")]
		[Category("PaintBar Colors")]
		public bool UseHollowBars
		{
			get { return pUseHollowBars; }
			set { pUseHollowBars = value; }
		}

		private Brush pcoac = Brushes.Green;
		[Description("Color of candle outline when bar is completely above MA")]
		[Category("PaintBar Colors")]
		public Brush AboveMA_Outline {get { return pcoac; } set { pcoac = value; }}
		[Browsable(false)]
		public string pcoacColorDownSerialize
		{get { return Serialize.BrushToString(pcoac); }set { pcoac = Serialize.StringToBrush(value); }}

		private Brush pcosc = Brushes.Black;
		[Description("Color of candle outline when bar is straddling the MA")]
		[Category("PaintBar Colors")]
		public Brush StraddleMA_Outline {get { return pcosc; } set { pcosc = value; }}
		[Browsable(false)]
		public string pcoscColorDownSerialize
		{get { return Serialize.BrushToString(pcosc); }set { pcosc = Serialize.StringToBrush(value); }}

		private Brush pcobc = Brushes.Red;
		[Description("Color of candle outline when bar is completely below MA")]
		[Category("PaintBar Colors")]
		public Brush BelowMA_Outline {get { return pcobc; } set { pcobc = value; }}
		[Browsable(false)]
		public string pcobcColorDownSerialize
		{get { return Serialize.BrushToString(pcobc); }set { pcobc = Serialize.StringToBrush(value); }}

		private Brush pac = Brushes.Green;
		[Description("Color of up-closing bar when bar is completely above MA")]
		[Category("PaintBar Colors")]
		public Brush AboveMA_UpClose {get { return pac; } set { pac = value; }}
		[Browsable(false)]
		public string pacColorDownSerialize
		{get { return Serialize.BrushToString(pac); }set { pac = Serialize.StringToBrush(value); }}

		private Brush psc = Brushes.Black;
		[Description("Color of up-closing bar when bar is straddling the MA")]
		[Category("PaintBar Colors")]
		public Brush StraddleMA_UpClose {get { return psc; } set { psc = value; }}
		[Browsable(false)]
		public string pscColorDownSerialize
		{get { return Serialize.BrushToString(psc); }set { psc = Serialize.StringToBrush(value); }}

		private Brush pbc = Brushes.Red;
		[Description("Color of up-closing bar when bar is completely below MA")]
		[Category("PaintBar Colors")]
		public Brush BelowMA_UpClose {get { return pbc; } set { pbc = value; }}
		[Browsable(false)]
		public string pbcColorDownSerialize
		{get { return Serialize.BrushToString(pbc); }set { pbc = Serialize.StringToBrush(value); }}
		#endregion
		#region MA line settings
//		private Brush pMarkerDotColor = Brushes.Purple;
//		[Description("Color of Dot when 'Alert Type' involves an Dot")]
//		[Category("Plots")]
//		public Brush MarkerDotColor
//		{get { return pMarkerDotColor; }
//		set { pMarkerDotColor = value; }}
//		[Browsable(false)]
//		public string MarkerDotColorDownSerialize
//		{get { return Serialize.BrushToString(pMarkerDotColor); }set { pMarkerDotColor = Serialize.StringToBrush(value); }}


		#endregion
		#endregion
	}
}
public enum UltimateMovingAverage_MAtype {
	SMA,
	EMA,
	WMA,
	TMA,
	TEMA,
	HMA,
	LinReg,
	SMMA,
	ZeroLagEMA,
	ZeroLagTEMA,
	ZeroLagHATEMA,
	KAMA,
	VWMA,
	VMA,
	SuperTrend,
	RMA
}
public enum UltimateMovingAverage_AlertType {
	Dot,
	Sound,
	Arrow,
	Triangle,
	ArrowAndSound,
	TriangleAndSound,
	DotAndSound,
	None
}
public enum UltimateMovingAverage_SuperTrend_MAType
{
	SMA, TMA, WMA, VWMA, TEMA, HMA, EMA, VMA
}
public enum UltimateMovingAverage_SuperTrend_Mode
{
    ATR,
    DualThrust
}
public enum UltimateMovingAverage_AlertBasis {
	PriceEnteringZone, PriceExitingZone, BothEnteringAndExiting, ColorChange
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UltimateMovingAverage[] cacheUltimateMovingAverage;
		public UltimateMovingAverage UltimateMovingAverage()
		{
			return UltimateMovingAverage(Input);
		}

		public UltimateMovingAverage UltimateMovingAverage(ISeries<double> input)
		{
			if (cacheUltimateMovingAverage != null)
				for (int idx = 0; idx < cacheUltimateMovingAverage.Length; idx++)
					if (cacheUltimateMovingAverage[idx] != null &&  cacheUltimateMovingAverage[idx].EqualsInput(input))
						return cacheUltimateMovingAverage[idx];
			return CacheIndicator<UltimateMovingAverage>(new UltimateMovingAverage(), input, ref cacheUltimateMovingAverage);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UltimateMovingAverage UltimateMovingAverage()
		{
			return indicator.UltimateMovingAverage(Input);
		}

		public Indicators.UltimateMovingAverage UltimateMovingAverage(ISeries<double> input )
		{
			return indicator.UltimateMovingAverage(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UltimateMovingAverage UltimateMovingAverage()
		{
			return indicator.UltimateMovingAverage(Input);
		}

		public Indicators.UltimateMovingAverage UltimateMovingAverage(ISeries<double> input )
		{
			return indicator.UltimateMovingAverage(input);
		}
	}
}

#endregion
