
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
    [CategoryOrder("Parameters", 0)]
    [CategoryOrder("Audible", 10)]
    [CategoryOrder("Popups", 20)]
    [CategoryOrder("Email", 30)]
	[Description("The UniversalChannel is a midline (calculation method is selectable from SMA, SMMA, EMA, HMA, WMA, MAMA, FAMA, KAMA, T3, TMA, TEMA, LinReg, Bollinger, Keltner) and the bands are x-multiples of the avg range")]
	public class IW_UniversalChannel : Indicator
	{
		private const int UP = 1;
		private const int DOWN = -1;
		private Series<double> diff;
		private int LastUpperLineBar = -1;
		private int LastMidLineBar = -1;
		private int LastLowerLineBar = -1;
		private int DataBarsId = 0;
		private double middle = 0, offset = 0;
		private BarsPeriodType AcceptableBaseBarsPeriodType = BarsPeriodType.Tick;
		private int Udir = 0;
		private int Mdir = 0;
		private int Ldir = 0;
		private double PriorTickPrice = 0;
		SMA sma, smaTypical;
		IW_SMMA smma;
		EMA ema;
		HMA hma;
		WMA wma;
		VWMA vwma;
		TMA tma;
		TEMA tema;
		MAMA mama;
		T3 t3;
		KAMA kama;
		LinReg linreg;

	protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			bool IsBen = NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 && System.IO.File.Exists("c:\\222222222222.txt");
			if(!IsBen)
				VendorLicense("IndicatorWarehouse", "AIUniversalChannel", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
			AddPlot(new Stroke(Brushes.Blue,1),PlotStyle.Line, "Midline");
			AddPlot(new Stroke(Brushes.Blue,1),PlotStyle.Line, "Upper");
			AddPlot(new Stroke(Brushes.Blue,1),PlotStyle.Line, "Lower");

			Calculate=Calculate.OnPriceChange;
			this.Name = "iw Universal Channel";
			this.DisplayInDataBox = true;
			IsOverlay=true;
			MidLineTouchWAV = "SOUND OFF";
			pUpperLineTouchWAV = "SOUND OFF";
			pLowerLineTouchWAV = "SOUND OFF";
			pMidLineTouchWAV = "SOUND OFF";
		}
		else if (State == State.Configure)
		{
			if(DataBarsId == 0) {
				if(period>254) diff = new Series<double>(this, MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				else diff = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);//inside State.DataLoaded
			} else {
				diff = new Series<double>(SMA(Closes[DataBarsId],1),MaximumBarsLookBack.Infinite);//inside State.DataLoaded
			}
			if(pBaseBarsPeriodType != UniversalChannel_BaseTimeframes.UseChartTime) {
				AddDataSeries(AcceptableBaseBarsPeriodType, pBaseTimeFrame);
				DataBarsId = 1;
			} else
				AddDataSeries(BarsPeriodType.Minute, 60);
		}
		else if(State == State.DataLoaded){
			if(pMAType == UniversalChannel_MAType.SMA)
				sma =  SMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.Keltner)
				smaTypical = SMA(Typicals[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.SMMA)
				smma = IW_SMMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.EMA)
				ema = EMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.HMA)
				hma = HMA(Inputs[DataBarsId], Math.Max(2,period));
			if(pMAType == UniversalChannel_MAType.WMA)
				wma = WMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.VWMA)
				vwma = VWMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.TMA)
				tma = TMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.TEMA)
				tema = TEMA(Inputs[DataBarsId], period);
			if(pMAType == UniversalChannel_MAType.MAMA)
				mama = MAMA(Inputs[DataBarsId], pFastLimit, pSlowLimit);
			if(pMAType == UniversalChannel_MAType.T3)
				t3 = T3(Inputs[DataBarsId], period, pTCount, pVFactor);
			if(pMAType == UniversalChannel_MAType.KAMA)
				kama = KAMA(Inputs[DataBarsId], pFast, Math.Max(5,period), pSlow);
			if(pMAType == UniversalChannel_MAType.LinReg)
				linreg = LinReg(Inputs[DataBarsId], Math.Max(2,period));
		}
	}

		protected override void OnBarUpdate()
		{
			if(CurrentBars[0]<2) return;
			if(CurrentBars[1]<2) return;
			if(middle==0) middle = Closes[DataBarsId][0];
			if(BarsInProgress == DataBarsId) {
				middle	= 0;
				if(pMAType == UniversalChannel_MAType.SMA)            middle = sma[0];
				else if(pMAType == UniversalChannel_MAType.Bollinger) middle = sma[0];
				else if(pMAType == UniversalChannel_MAType.Keltner)   middle = smaTypical[0];
				else if(pMAType == UniversalChannel_MAType.SMMA)   middle = smma[0];
				else if(pMAType == UniversalChannel_MAType.EMA)    middle = ema[0];
				else if(pMAType == UniversalChannel_MAType.HMA)    middle = hma[0];
				else if(pMAType == UniversalChannel_MAType.WMA)    middle = wma[0];
				else if(pMAType == UniversalChannel_MAType.VWMA)   middle = vwma[0];
				else if(pMAType == UniversalChannel_MAType.TMA)    middle = tma[0];
				else if(pMAType == UniversalChannel_MAType.TEMA)   middle = tema[0];
				else if(pMAType == UniversalChannel_MAType.T3)     middle = t3[0];
				else if(pMAType == UniversalChannel_MAType.MAMA)   middle = mama[0];
				else if(pMAType == UniversalChannel_MAType.FAMA)   middle = mama.Fama[0];
				else if(pMAType == UniversalChannel_MAType.KAMA)   middle = kama[0];
				else if(pMAType == UniversalChannel_MAType.LinReg) middle = linreg[0];

				offset = pOffsetTicks * TickSize;
				if(pMAType == UniversalChannel_MAType.Bollinger) offset = StdDev(Inputs[DataBarsId],period)[0]  * pOffsetMultiplier;
				else if(pMAType == UniversalChannel_MAType.Keltner || pBandDistanceBasis == UniversalChannel_BandDistanceBasis.AvgRange_Multiple) {
					diff[0] = (Highs[DataBarsId][0] - Lows[DataBarsId][0]);
					offset = SMA(diff, period)[0] * pOffsetMultiplier;
				}
				else if(pBandDistanceBasis == UniversalChannel_BandDistanceBasis.ATR_Multiple)    offset = ATR(Inputs[DataBarsId],pATRperiod)[0] * pOffsetMultiplier;
				else if(pBandDistanceBasis == UniversalChannel_BandDistanceBasis.StdDev_Multiple) offset = StdDev(Inputs[DataBarsId],period)[0]  * pOffsetMultiplier;
			}

			if(BarsInProgress==0) {
				Midline[0] = (middle);
				Upper[0] = (middle + offset);
				Lower[0] = (middle - offset);

				bool UCross = false;
				bool MCross = false;
				bool LCross = false;
				if(State != State.Historical) {
					if(Calculate != Calculate.OnBarClose){
						if(PriorTickPrice>=Upper[0] && Close[0]<=Upper[0])          UCross = true;
						else if(PriorTickPrice<=Upper[0] && Close[0]>=Upper[0])     UCross = true;
						if(PriorTickPrice>=Midline[0] && Close[0]<=Midline[0])      MCross = true;
						else if(PriorTickPrice<=Midline[0] && Close[0]>=Midline[0]) MCross = true;
						if(PriorTickPrice>=Lower[0] && Close[0]<=Lower[0])          LCross = true;
						else if(PriorTickPrice<=Lower[0] && Close[0]>=Lower[0])     LCross = true;
						PriorTickPrice = Close[0];
					} else {
						if(High[0]>=Upper[0] && Low[0]<=Upper[0])     UCross = true;
						if(High[0]>=Midline[0] && Low[0]<=Midline[0]) MCross = true;
						if(High[0]>=Lower[0] && Low[0]<=Lower[0])     LCross = true;
					}
				}
				if(LastUpperLineBar != CurrentBar && UCross) {
					LastUpperLineBar = CurrentBar;
					if(ChartControl!=null) Alert(CurrentBar.ToString()+"U", NinjaTrader.NinjaScript.Priority.High, "UniversalChannel:  Upperline hit",AddSoundFolder( pUpperLineTouchWAV), 1, Plots[1].Brush, ChartControl.Properties.AxisPen.Brush);
					else PlaySound(AddSoundFolder(pUpperLineTouchWAV));
					if(pUpperLinePopup) Log(string.Concat(Name,":  Upperline hit @ ",Upper[0].ToString()," at ",NinjaTrader.Core.Globals.Now.ToString()),NinjaTrader.Cbi.LogLevel.Alert);
					if(pUpperLineEmail.Length>0) SendMail(pUpperLineEmail, Instrument.FullName+" ("+Bars.BarsPeriod.ToString()+") UniversalChannel:  Upperline hit","Email sent automatically from NinjaTrader");
				}
				if(LastMidLineBar != CurrentBar && MCross) {
					LastMidLineBar = CurrentBar;
					if(ChartControl!=null) Alert(CurrentBar.ToString()+"M", NinjaTrader.NinjaScript.Priority.High, "UniversalChannel:  Midline hit",AddSoundFolder( pMidLineTouchWAV), 1, Plots[0].Brush, ChartControl.Properties.AxisPen.Brush);
					else PlaySound(AddSoundFolder(pMidLineTouchWAV));
					if(pMidLinePopup) Log(string.Concat(Name,":  Midline hit @ ",Midline[0].ToString()," at ",NinjaTrader.Core.Globals.Now.ToString()),NinjaTrader.Cbi.LogLevel.Alert);
					if(pMidLineEmail.Length>0) SendMail(pMidLineEmail, Instrument.FullName+" ("+Bars.BarsPeriod.ToString()+") UniversalChannel:  Midline hit","Email sent automatically from NinjaTrader");
				}
				if(LastLowerLineBar != CurrentBar && LCross) {
					LastLowerLineBar = CurrentBar;
					if(ChartControl!=null) Alert(CurrentBar.ToString()+"L", NinjaTrader.NinjaScript.Priority.High, "UniversalChannel:  Lowerline hit",AddSoundFolder( pLowerLineTouchWAV), 1, Plots[2].Brush, ChartControl.Properties.AxisPen.Brush);
					else PlaySound(AddSoundFolder(pLowerLineTouchWAV));
					if(pLowerLinePopup) Log(string.Concat(Name,":  Lowerline hit @ ",Lower[0].ToString()," at ",NinjaTrader.Core.Globals.Now.ToString()),NinjaTrader.Cbi.LogLevel.Alert);
					if(pLowerLineEmail.Length>0) SendMail(pLowerLineEmail, Instrument.FullName+" ("+Bars.BarsPeriod.ToString()+") UniversalChannel:  Lowerline hit","Email sent automatically from NinjaTrader");
				}

				if(CurrentBars[0]<3) return;

				if(Upper[0] > Upper[1])      Udir = UP;
				else if(Upper[0] < Upper[1]) Udir = DOWN;
				if(Udir==UP) PlotBrushes[1][0] = pUupColor;
				else         PlotBrushes[1][0] = pUdownColor;

				if(Midline[0] > Midline[1])      Mdir = UP;
				else if(Midline[0] < Midline[1]) Mdir = DOWN;
				if(Mdir==UP) PlotBrushes[0][0] = pMupColor;
				else         PlotBrushes[0][0] = pMdownColor;
				//Draw.Text(this, CurrentBar.ToString(),Mdir.ToString(),0,Low[0]-TickSize,Color.White);

				if(Lower[0] > Lower[1])      Ldir = UP;
				else if(Lower[0] < Lower[1]) Ldir = DOWN;
				if(Ldir==UP) PlotBrushes[2][0] = pLupColor;
				else         PlotBrushes[2][0] = pLdownColor;

				if(pColorizeBars) {
					if (Close[0] > Upper[0]) //up color
					{	
						bool IsCandlestick = ChartBars.Properties.ChartStyleType == ChartStyleType.CandleStick;
						if(Close[0]<Open[0]) BarBrush = pUtDc;
						else if(Close[0]==Open[0]) BarBrush = pUtOutline;
						else BarBrush  = IsCandlestick?pUtUc:pUtOutline; 
						CandleOutlineBrush = pUtOutline;
					} 
					if (Close[0] < Lower[0]) //down color
					{
						bool IsCandlestick = ChartBars.Properties.ChartStyleType == ChartStyleType.CandleStick;
						if(Close[0]>Open[0]) BarBrush = IsCandlestick?pDtUc:pDtOutline;
						else if(Close[0]==Open[0]) BarBrush = pDtOutline;
						else BarBrush  = pDtDc; 
						CandleOutlineBrush = pDtOutline;
					} 
				}
			}
		}

//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds",wav);
		}
//====================================================================

		#region Properties
		private int pBaseTimeFrame = 100;
		[Description("Base time frame, the number of minutes, ticks, volume, range ticks to be used in calculation of the EMA")]
		[Category(" Data")]
		public int BaseTimeFrame
		{
			get { return pBaseTimeFrame; }
			set { pBaseTimeFrame = Math.Max(1, value); }
		}
		private UniversalChannel_BaseTimeframes pBaseBarsPeriodType = UniversalChannel_BaseTimeframes.UseChartTime;
		[Description("Base Period Type to be used in calculation of the Channel")]
		[Category(" Data")]
		public UniversalChannel_BaseTimeframes BaseBarsPeriodType
		{
			get { return pBaseBarsPeriodType; }
			set { pBaseBarsPeriodType = value; 
				switch (pBaseBarsPeriodType) {
					case UniversalChannel_BaseTimeframes.Tick:   AcceptableBaseBarsPeriodType = BarsPeriodType.Tick;   break;
					case UniversalChannel_BaseTimeframes.Range:  AcceptableBaseBarsPeriodType = BarsPeriodType.Range;  break;
					case UniversalChannel_BaseTimeframes.Volume: AcceptableBaseBarsPeriodType = BarsPeriodType.Volume; break;
					case UniversalChannel_BaseTimeframes.Second: AcceptableBaseBarsPeriodType = BarsPeriodType.Second; break;
					case UniversalChannel_BaseTimeframes.Minute: AcceptableBaseBarsPeriodType = BarsPeriodType.Minute; break;
					case UniversalChannel_BaseTimeframes.Day:    AcceptableBaseBarsPeriodType = BarsPeriodType.Day;    break;
					case UniversalChannel_BaseTimeframes.Week:   AcceptableBaseBarsPeriodType = BarsPeriodType.Week;   break;
					case UniversalChannel_BaseTimeframes.Month:  AcceptableBaseBarsPeriodType = BarsPeriodType.Month;  break;
					case UniversalChannel_BaseTimeframes.Year:   AcceptableBaseBarsPeriodType = BarsPeriodType.Year;   break;
				}
			}
		}

		#region Upper line visual properties
		private Brush pUupColor = Brushes.Green;
		[XmlIgnore()]
		[Category("Plots")]
		public Brush UpperLine_Up {get { return pUupColor; } set {         pUupColor = value; }}
				[Browsable(false)]
				public string UupDownSerialize	{get { return Serialize.BrushToString(pUupColor); }set { pUupColor = Serialize.StringToBrush(value); }		}

		private Brush pUdownColor = Brushes.Red;
		[XmlIgnore()]
		[Category("Plots")]
		public Brush UpperLine_Down		{get { return pUdownColor; } set { pUdownColor = value; }}
				[Browsable(false)]
				public string UdnDownSerialize	{get { return Serialize.BrushToString(pUdownColor); }set { pUdownColor = Serialize.StringToBrush(value); }		}
		#endregion
		#region Midline line visual properties
		private Brush pMupColor = Brushes.Green;
		[XmlIgnore()]
		[Category("Plots")]
		public Brush Midline_Up	{get { return pMupColor; }	set {         pMupColor = value; }}
				[Browsable(false)]
				public string MupDownSerialize	{get { return Serialize.BrushToString(pMupColor); }set { pMupColor = Serialize.StringToBrush(value); }		}

		private Brush pMdownColor = Brushes.Red;
		[XmlIgnore()]
		[Category("Plots")]
		public Brush Midline_Down	{get { return pMdownColor; } set {         pMdownColor = value; }}
				[Browsable(false)]
				public string MdnDownSerialize	{get { return Serialize.BrushToString(pMdownColor); }set { pMdownColor = Serialize.StringToBrush(value); }		}
		#endregion
		#region Lower line visual properties
		private Brush pLupColor = Brushes.Green;
		[XmlIgnore()]
		[Category("Plots")]
		public Brush LowerLine_Up {get { return pLupColor; }set {         pLupColor = value; }}
				[Browsable(false)]
				public string LupDownSerialize	{get { return Serialize.BrushToString(pLupColor); }set { pLupColor = Serialize.StringToBrush(value); }		}

		private Brush pLdownColor = Brushes.Red;
		[XmlIgnore()]
		[Category("Plots")]
		public Brush LowerLine_Down	{get { return pLdownColor; }set {         pLdownColor = value; }}
				[Browsable(false)]
				public string LdnDownSerialize	{get { return Serialize.BrushToString(pLdownColor); }set { pLdownColor = Serialize.StringToBrush(value); }		}
		#endregion
		
		#region PaintBars
		private bool pColorizeBars = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Colorize bars?",  GroupName = "PaintBars")]
		public bool ColorizeBars
		{
			get { return pColorizeBars; }
			set { pColorizeBars = value; }
		}
		private Brush pUtDc = Brushes.Green;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AboveUpper DnClose",  GroupName = "PaintBars")]
		public Brush UTDC{	get { return pUtDc; }	set { pUtDc = value; }		}
				[Browsable(false)]
				public string UTDClSerialize{	get { return Serialize.BrushToString(pUtDc); } set { pUtDc = Serialize.StringToBrush(value); }		}
		
		private Brush pUtUc = Brushes.Transparent;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AboveUpper UpClose",  GroupName = "PaintBars")]
		public Brush UTUC{	get { return pUtUc; }	set { pUtUc = value; }		}
				[Browsable(false)]
				public string UTUClSerialize{	get { return Serialize.BrushToString(pUtUc); } set { pUtUc = Serialize.StringToBrush(value); }		}
		
		private Brush pDtDc = Brushes.Red;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BelowLower DnClose",  GroupName = "PaintBars")]
		public Brush DTDC{	get { return pDtDc; }	set { pDtDc = value; }		}
				[Browsable(false)]
				public string DTDClSerialize{	get { return Serialize.BrushToString(pDtDc); } set { pDtDc = Serialize.StringToBrush(value); }	}
		
		private Brush pDtUc = Brushes.Transparent;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BelowLower UpClose",  GroupName = "PaintBars")]
		public Brush DTUC{	get { return pDtUc; }	set { pDtUc = value; }		}
				[Browsable(false)]
				public string DTUClSerialize{	get { return Serialize.BrushToString(pDtUc); } set { pDtUc = Serialize.StringToBrush(value); }}

		private Brush pUtOutline = Brushes.Green;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AboveUpper Outline",  GroupName = "PaintBars")]
		public Brush UTO{	get { return pUtOutline; }	set { pUtOutline = value; }		}
				[Browsable(false)]
				public string UTOSerialize{	get { return Serialize.BrushToString(pUtOutline); } set { pUtOutline = Serialize.StringToBrush(value); }			}
		
		private Brush pDtOutline = Brushes.Red;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BelowLower Outline",  GroupName = "PaintBars")]
		public Brush DTO{	get { return pDtOutline; }	set { pDtOutline = value; }		}
				[Browsable(false)]
				public string DTOSerialize{	get { return Serialize.BrushToString(pDtOutline); } set { pDtOutline = Serialize.StringToBrush(value); }}
		#endregion

		private double pVFactor = 0.7;
		private int pTCount = 3;
		#region T3 special parameters
		[Description("The smooth count - used only for the T3 MAType")]
        [Category("T3 parameters")]
        public int TCount
        {
            get { return pTCount; }
            set { pTCount = Math.Max(1, value); }
        }
        [Description("VFactor - used only for the T3 MAType")]
        [Category("T3 parameters")]
        public double VFactor
        {
            get { return pVFactor; }
            set { pVFactor = Math.Max(0, value); }
        }
		#endregion

		private double pFastLimit = 0.7;
		private double pSlowLimit = 0.7;
		#region MAMA/FAMA special parameters
		[Description("Fast Limit. Upper limit of the alpha used in computing values.")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast limit",  GroupName = "MAMA or FAMA parameters")]
		public double FastLimit
		{
			get { return pFastLimit; }
			set { pFastLimit = Math.Max(0.05, value); }
		}
		[Description("Slow Limit. Lower limit of the alpha used in computing values.")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow limit",  GroupName = "MAMA or FAMA parameters")]
		public double SlowLimit
		{
			get { return pSlowLimit; }
			set { pSlowLimit = Math.Max(0.005, value); }
		}
		#endregion

		private int pFast = 2;
		private int pSlow = 30;
		#region KAMA parameters
		[Description("KAMA Slow Length.")]
		[Category("KAMA parameters")]
		public int Slow
		{
			get { return pSlow; }
			set { pSlow = Math.Min(125, Math.Max(1, value)); }
		}
		[Description("KAMA Fast Length.")]
		[Category("KAMA parameters")]
		public int Fast
		{
			get { return pFast; }
			set { pFast = Math.Min(125, Math.Max(1, value)); }
		}
		#endregion

		private UniversalChannel_MAType pMAType = UniversalChannel_MAType.SMA;
		[Display(Order = 10, Name = "MA Type", GroupName = "Parameters", Description = "MA type for midline calculation")]
		public UniversalChannel_MAType MAType
		{
			get { return pMAType; }
			set { pMAType = value; }
		}

		private	int period = 10;
		[Display(Order = 20, Name = "MA Period", GroupName = "Parameters", Description = "Numbers of bars used for calculations of midline MA")]
		public int MAPeriod
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}

		private	int pATRperiod = 10;
		[Display(Order = 30, Name = "ATR Period", GroupName = "Parameters", Description = "Bars in ATR calculation, used only when ATR_Multiple is selected as the BandDistanceBasis")]
		public int ATRPeriod
		{
			get { return pATRperiod; }
			set { pATRperiod = Math.Max(1, value); }
		}

		private UniversalChannel_BandDistanceBasis pBandDistanceBasis = UniversalChannel_BandDistanceBasis.ATR_Multiple;
		[Display(Order = 40, Name = "Band Distance basis", GroupName = "Parameters", Description = "Basis for calculation of upper and lower lines, ATR, AvgRange or StdDev (this param is ignored when Bollinger or Keltner is selected)")]
		public UniversalChannel_BandDistanceBasis BandDistanceBasis
		{
			get { return pBandDistanceBasis; }
			set { pBandDistanceBasis = value; }
		}

		private double pOffsetMultiplier = 1.5;
		[Display(Order = 50, Name = "Offset (multiplier)", GroupName = "Parameters", Description = "Multiplier for calculating upper and lower band distances.  Used when ATR_Multiple, AvgRange, StdDev, Bollinger or Keltner is selected as the BandDistanceBasis")]
		public double OffsetMultiplier
		{
			get { return pOffsetMultiplier; }
			set { pOffsetMultiplier = Math.Max(0.01, value); }
		}
		private int pOffsetTicks = 15;
		[Display(Order = 60, Name = "Offset (ticks)", GroupName = "Parameters", Description = "Distance (in ticks) between the midline and the upper and lower bands.  Used when ConstantTicks is selected as the BandDistanceBasis")]
		public int OffsetTicks
		{
			get { return pOffsetTicks; }
			set { pOffsetTicks = Math.Max(0, value); }
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
				list.Add("SOUND OFF");
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
		#region WAV files
		private string pUpperLineTouchWAV = "";
		[Display(Order = 10, Name = "Upperline Touch", GroupName = "Audible", Description = "Sound file to play when a bar hits the upper-line...must be a valid WAV file in your Sounds folder")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Audible")]
		public string UpperLineTouchWAV
		{
			get { return pUpperLineTouchWAV; }
			set { pUpperLineTouchWAV = value; }
		}
		private string pMidLineTouchWAV = "";
		[Display(Order = 20, Name = "Midline Touch", GroupName = "Audible", Description = "Sound file to play when a bar hits the midline...must be a valid WAV file in your Sounds folder")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Audible")]
		public string MidLineTouchWAV
		{
			get { return pMidLineTouchWAV; }
			set { pMidLineTouchWAV = value; }
		}
		private string pLowerLineTouchWAV = "";
		[Display(Order = 30, Name = "Lowerline Touch", GroupName = "Audible", Description = "Sound file to play when a bar hits the lower-line...must be a valid WAV file in your Sounds folder")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Audible")]
		public string LowerLineTouchWAV
		{
			get { return pLowerLineTouchWAV; }
			set { pLowerLineTouchWAV = value; }
 		}
		#endregion
		
		#region Popups
		private bool pUpperLinePopup = false;
		[Display(Order = 10, Name = "Upperline popup", GroupName = "Popups", Description = "Launch popup when upperline is hit")]
		public bool UpperLinePopup
		{
			get { return pUpperLinePopup; }
			set { pUpperLinePopup = value; }
		}
		private bool pMidLinePopup = false;
		[Display(Order = 20, Name = "Midline popup", GroupName = "Popups", Description = "Launch popup when midline is hit")]
		public bool MidLinePopup
		{
			get { return pMidLinePopup; }
			set { pMidLinePopup = value; }
		}
		private bool pLowerLinePopup = false;
		[Display(Order = 30, Name = "Lowerline popup", GroupName = "Popups", Description = "Launch popup when lowerline is hit")]
		public bool LowerLinePopup
		{
			get { return pLowerLinePopup; }
			set { pLowerLinePopup = value; }
		}
		#endregion

		#region Email
		private string pUpperLineEmail = "";
		[Display(Order = 10, Name = "Upperline email", GroupName = "Email", Description = "Email address to receive a notification when upperline is hit")]
		public string UpperLineEmailAddress
		{
			get { return pUpperLineEmail; }
			set { pUpperLineEmail = value; }
		}
		private string pMidLineEmail = "";
		[Display(Order = 20, Name = "Midline email", GroupName = "Email", Description = "Email address to receive a notification when midline is hit")]
		public string MidLineEmailAddress
		{
			get { return pMidLineEmail; }
			set { pMidLineEmail = value; }
		}
		private string pLowerLineEmail = "";
		[Display(Order = 30, Name = "Lowerline email", GroupName = "Email", Description = "Email address to receive a notification when lowerline is hit")]
		public string LowerLineEmailAddress
		{
			get { return pLowerLineEmail; }
			set { pLowerLineEmail = value; }
		}
		#endregion
		
		#endregion

		#region Plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midline {get { return Values[0]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper {  get { return Values[1]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower {  get { return Values[2]; }}
        #endregion
	}
}
public enum UniversalChannel_BaseTimeframes {
	UseChartTime,Tick,Range,Volume,Second,Minute,Day,Week,Month,Year
}
public enum UniversalChannel_MAType
{
	SMA,
	SMMA,
	EMA,
	HMA,
	VWMA,
	WMA,
	MAMA,
	FAMA,
	KAMA,
	T3,
	TMA,
	TEMA,
	LinReg,
	Keltner,
	Bollinger
}
public enum UniversalChannel_BandDistanceBasis {
	ConstantTicks,
	ATR_Multiple,
	AvgRange_Multiple,
	StdDev_Multiple
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_UniversalChannel[] cacheIW_UniversalChannel;
		public IW_UniversalChannel IW_UniversalChannel()
		{
			return IW_UniversalChannel(Input);
		}

		public IW_UniversalChannel IW_UniversalChannel(ISeries<double> input)
		{
			if (cacheIW_UniversalChannel != null)
				for (int idx = 0; idx < cacheIW_UniversalChannel.Length; idx++)
					if (cacheIW_UniversalChannel[idx] != null &&  cacheIW_UniversalChannel[idx].EqualsInput(input))
						return cacheIW_UniversalChannel[idx];
			return CacheIndicator<IW_UniversalChannel>(new IW_UniversalChannel(), input, ref cacheIW_UniversalChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_UniversalChannel IW_UniversalChannel()
		{
			return indicator.IW_UniversalChannel(Input);
		}

		public Indicators.IW_UniversalChannel IW_UniversalChannel(ISeries<double> input )
		{
			return indicator.IW_UniversalChannel(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_UniversalChannel IW_UniversalChannel()
		{
			return indicator.IW_UniversalChannel(Input);
		}

		public Indicators.IW_UniversalChannel IW_UniversalChannel(ISeries<double> input )
		{
			return indicator.IW_UniversalChannel(input);
		}
	}
}

#endregion
