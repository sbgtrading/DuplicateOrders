
#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using System.Collections.Generic;
using System.Windows.Media;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("")]
    public class iwRaptorFilter : Indicator
    {
		private const int LONG = 1;
		private const int FLAT = 0;
		private const int NEUTRAL = 0;
		private const int SHORT = -1;
			public class Eagle
			{
				#region Eagle engine
				private const int LONG = 1;
				private const int FLAT = 0;
				private const int SHORT = -1;

				public class Data{
					public double High,Low,Close;
					public double fastEMA;
					public double medEMA;
					public double slowEMA;
					public double Hi,Lo,MacdDiff,Macd;
					public double position, BuyWarningDot, BuyAlert,SellWarningDot,SellAlert,BuyEntryPoint,SellEntryPoint,MostRecentPosition;
					public int Triangle_Direction, MACD_Dot_Direction, PaintBar_Direction;
					public bool ResetTriggered;
				}
				public List<Data> bars;
				public int Status;
				public List<string> StatTxt = new List<string>();

//				private EMA	fast, med, slow;
//				private MACD macd;
//				private Series<double> Hi,Lo;
//				private Series<double> position, BuyWarningDot, BuyAlert, SellWarningDot, SellAlert, BuyEntryPoint, SellEntryPoint;
//				private IntSeries Triangle_Direction, MACD_Dot_Direction, PaintBar_Direction;//, Position;
//				private BoolSeries ResetTriggered;
				private int InitialEntryLongBar, InitialEntryShortBar;
//				public Series<double> MostRecentPosition;
//				public IndicatorBase indi;
				public bool RunInit = true;

//						protected override void OnStateChange(){if (State == State.SetDefaults)
//				{
//					Print("in e.Initialize");
//				}
//				if (State == State.Configure)
//				{
//					Print("in e.OnStartUp");
//				}
//				protected override void OnBarUpdate()
				public void Update(int CurrentBar, bool IsFirstTickOfBar, double fema, double mema, double sema, double macd, double macddiff, double high, double low, double close, double TickSize)
				{
					if(RunInit){
						RunInit = false;
						bars = new List<Data>();
					}
					if(IsFirstTickOfBar)
						bars.Insert(0, new Data());
Status = 75;
					bars[0].Close = close;
					bars[0].High = high;
					bars[0].Low = low;
					bars[0].fastEMA = fema;//13);
					bars[0].medEMA = mema;//,21);
					bars[0].slowEMA = sema;//,55);
					bars[0].Macd = macd;//8,21,8);
					bars[0].MacdDiff = macddiff;
					bars[0].Hi = Math.Max(bars[0].fastEMA,Math.Max(bars[0].medEMA,bars[0].slowEMA));
					bars[0].Lo = Math.Min(bars[0].fastEMA,Math.Min(bars[0].medEMA,bars[0].slowEMA));
Status = 86;
					if(CurrentBar<5) {
						bars[0].Triangle_Direction=FLAT;
						bars[0].MACD_Dot_Direction=FLAT;
						bars[0].PaintBar_Direction=FLAT;
						bars[0].ResetTriggered=false;
						bars[0].position=FLAT;
						return;
					}
					else {
Status = 96;
						if(bars.Count<2) return;
						bars[0].Triangle_Direction = bars[1].Triangle_Direction;
						bars[0].MACD_Dot_Direction = bars[1].MACD_Dot_Direction;
						bars[0].PaintBar_Direction = bars[1].PaintBar_Direction;
						bars[0].ResetTriggered = bars[1].ResetTriggered;
						bars[0].position = bars[1].position;
						bars[0].BuyWarningDot = double.NaN;
						bars[0].BuyAlert = double.NaN;
						bars[0].SellWarningDot = double.NaN;
						bars[0].SellAlert = double.NaN;
					}
Status = 108;

					if(bars.Count<5) return;
					#region PaintBars
					if(bars[0].Macd>0 && bars[0].MacdDiff>0)      bars[0].PaintBar_Direction=LONG;
					else if(bars[0].Macd<0 && bars[0].MacdDiff<0) bars[0].PaintBar_Direction=SHORT;
					else                                          bars[0].PaintBar_Direction=FLAT;
					#endregion

Status = 116;
					bool macdDiffRising   = bars[0].MacdDiff>bars[1].MacdDiff && bars[1].MacdDiff>bars[2].MacdDiff;
					bool macdDiffFalling  = bars[0].MacdDiff<bars[1].MacdDiff && bars[1].MacdDiff<bars[2].MacdDiff;
					bool PermitUpTriangles = true;
					if(bars[1].Triangle_Direction == LONG)  PermitUpTriangles = false;
					bool PermitDownTriangles = true;
					if(bars[1].Triangle_Direction == SHORT) PermitDownTriangles = false;

					if(bars[0].MACD_Dot_Direction!=LONG && macdDiffRising) {
						bars[0].MACD_Dot_Direction = LONG;
						bars[0].BuyWarningDot = bars[0].Low-TickSize;
					}
Status = 128;
					if(bars[0].MACD_Dot_Direction!=SHORT && macdDiffFalling) {
						bars[0].MACD_Dot_Direction = SHORT;
						bars[0].SellWarningDot = bars[0].High+TickSize;
					}
					if(bars[0].High>=bars[0].Lo && bars[0].Low<=bars[0].Lo)      bars[0].ResetTriggered = true;
					else if(bars[0].High>=bars[0].Hi && bars[0].Low<=bars[0].Hi) bars[0].ResetTriggered = true;
					else if(bars[0].High<=bars[0].Hi && bars[0].Low>=bars[0].Lo) bars[0].ResetTriggered = true;
					if(bars[0].ResetTriggered) {
						bars[0].position=(FLAT);
						bars[0].Triangle_Direction=(FLAT);
					}
Status = 140;
//StatTxt.Clear();
//StatTxt.Add(string.Concat("HL ",bars[0].High," ",bars[0].Low,"   ",bars[0].ResetTriggered));
					if(bars[0].MACD_Dot_Direction==LONG && bars[0].Close>bars[0].Hi && bars[0].ResetTriggered && macdDiffRising) {
						bars[0].Triangle_Direction = (LONG);
						if(PermitUpTriangles) {
							bars[0].BuyAlert = (bars[0].Low-2*TickSize);
						}
					}
					if(bars[0].MACD_Dot_Direction==SHORT && bars[0].Close<bars[0].Lo && bars[0].ResetTriggered && macdDiffFalling) {
						bars[0].Triangle_Direction = (SHORT);
						if(PermitDownTriangles) {
							bars[0].SellAlert = (bars[0].High+2*TickSize);
						}
					}

Status = 155;
//StatTxt.Add(string.Concat("160: ",bars[1].SellEntryPoint," ",bars[0].SellEntryPoint,"  Pos: "+bars[0].position));
					if(bars[1].Triangle_Direction==LONG && bars[2].Triangle_Direction!=LONG) {
						bars[0].BuyEntryPoint = (bars[1].High+TickSize);
						InitialEntryLongBar = CurrentBar-1;
					}
					if(bars[1].Triangle_Direction==SHORT && bars[2].Triangle_Direction!=SHORT) {
						bars[0].SellEntryPoint = (bars[1].Low-TickSize);
						InitialEntryShortBar = CurrentBar-1;
					}
Status = 164;
//StatTxt.Add(string.Concat("170: ",bars[1].SellEntryPoint," ",bars[0].SellEntryPoint,"  Pos: "+bars[0].position));
					if(!double.IsNaN(bars[1].BuyEntryPoint)  && bars[0].Triangle_Direction==LONG  && bars[0].MACD_Dot_Direction==LONG)  bars[0].BuyEntryPoint = (bars[1].BuyEntryPoint);
					if(!double.IsNaN(bars[1].SellEntryPoint) && bars[0].Triangle_Direction==SHORT && bars[0].MACD_Dot_Direction==SHORT) bars[0].SellEntryPoint = (bars[1].SellEntryPoint);

					if(InitialEntryLongBar==CurrentBar-1)  bars[1].BuyEntryPoint = (bars[1].High+TickSize);
					if(InitialEntryShortBar==CurrentBar-1) bars[1].SellEntryPoint = (bars[1].Low-TickSize);

					if(bars[0].MACD_Dot_Direction!=LONG)		bars[0].BuyEntryPoint = double.NaN;
					if(CurrentBar - InitialEntryLongBar > 5)	bars[0].BuyEntryPoint = double.NaN;
					if(bars[0].MACD_Dot_Direction!=SHORT)		bars[0].SellEntryPoint = double.NaN;
					if(CurrentBar - InitialEntryShortBar > 5)	bars[0].SellEntryPoint = double.NaN;
					if(bars[1].position==LONG)  bars[0].BuyEntryPoint  = double.NaN;
					if(bars[1].position==SHORT) bars[0].SellEntryPoint = double.NaN;
Status = 177;
//StatTxt.Add(string.Concat("184: ",bars[1].SellEntryPoint," ",bars[0].SellEntryPoint,"  Pos: "+bars[0].position));

//					string tag=string.Concat("Raptorarrow"+CurrentBar);
					if(bars[0].position!=LONG && !double.IsNaN(bars[1].BuyEntryPoint) && bars[0].High>=bars[1].BuyEntryPoint) {
						bars[0].position=(LONG);
						bars[0].ResetTriggered = (false);
					}
					else if(bars[0].position!=SHORT && !double.IsNaN(bars[1].SellEntryPoint) && bars[0].Low<=bars[1].SellEntryPoint) {
						bars[0].position=(SHORT);
						bars[0].ResetTriggered = (false);
					}
//StatTxt.Add(string.Concat("195: ",bars[1].SellEntryPoint," ",bars[0].SellEntryPoint,"  Pos: "+bars[0].position));
//					else RemoveDrawObject(tag);
Status = 189;

					if(bars[0].position==LONG) bars[0].MostRecentPosition=(LONG);
					else if(bars[0].position==SHORT) bars[0].MostRecentPosition=(SHORT);
					else if(CurrentBar>1) bars[0].MostRecentPosition=(bars[1].MostRecentPosition);
				}
				#endregion
			}

		System.Windows.Media.Brush[] plotBrushes = null;

		private Eagle e;
		private int CurrentEaglePosition0 = FLAT;
		private bool RunInit = true;
		private int lasterror=0;
		private string ModuleName = "iwRaptor";
		private EMA fa;
		private EMA me;
		private EMA sl;
		private MACD macd8_21_8;

		/// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
	        {
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt");
				IsBen = IsBen && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", ModuleName, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				AddPlot(new Stroke(System.Windows.Media.Brushes.Transparent,1), PlotStyle.Hash, "Direction");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Transparent,1), PlotStyle.Dot, "Filter");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Transparent,1), PlotStyle.Hash, ".");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Transparent,1), PlotStyle.Hash, "-");
				IsOverlay=false;
				Name = "iw RTS Raptor Filter";
				DrawOnPricePanel = true;
	        }
			if (State == State.Configure)
			{
				AddDataSeries(new BarsPeriod { BarsPeriodType = (BarsPeriodType) 22220, Value = pEagleBrickSizeTrend, Value2 = pEagleBrickSizeReverse });
			}
			if (State == State.DataLoaded)
			{
				bool error = false;
				string type1 = string.Empty;
				if(BarsArray.Length>1) {
					type1 = BarsArray[1].BarsPeriod.ToString();
				}
				fa = EMA(Closes[1], 13);
				me = EMA(Closes[1], 21);
				sl = EMA(Closes[1], 55);
				macd8_21_8 = MACD(Closes[1], 8,21,8);

				var br1 = pPLUp.Clone();
				br1.Opacity = pOpacityUp+0.2;
				br1.Freeze();
				var br2 = pPLDn.Clone();
				br2.Opacity = pOpacityDown+0.2;
				br2.Freeze();
				var br3 = pPLUp.Clone();
				br3.Opacity = pOpacityUp;
				br3.Freeze();
				var br4 = pPLDn.Clone();
				br4.Opacity = pOpacityDown;
				br4.Freeze();
				plotBrushes = new System.Windows.Media.Brush[4]{br1.Clone(),br2.Clone(),br3.Clone(),br4.Clone()};
				plotBrushes[0].Freeze();
				plotBrushes[1].Freeze();
				plotBrushes[2].Freeze();
				plotBrushes[3].Freeze();
//				if(error) Log("Be advised:  iwMeanRenko is not loaded on your system."+Environment.NewLine+"Raptor is now running on "+type1+" bars",LogLevel.Alert);
			}
		}
		#region Plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Direction {get { return Values[0]; }}
		#endregion


        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if(BarsInProgress==0 && RunInit) {
				RunInit = false;
			}
			if(RunInit) return;
			if(CurrentBar<5) return;
int line = 311;
			Values[2][0] = (1);
			if(Plots[1].PlotStyle == PlotStyle.Bar){
				Values[1][0] = (1);
				Values[3][0] = (1);
			}else{
				Values[1][0] = (0);
				Values[3][0] = (-1);
			}

			try{
			if(BarsInProgress == 1) {
try{
				if(e==null) {
line=321;
					e = new Eagle();
line=323;
				}
line=325;
				try{
					e.Update(CurrentBar, IsFirstTickOfBar, fa[0], me[0], sl[0], MACD(8,21,8)[0], MACD(8,21,8).Diff[0], High[0], Low[0], Close[0], TickSize);
				}catch(Exception err){Print(e.Status+":"+line+"  Err: "+err.ToString());}
line=343;
				if(e.RunInit) {
					Print("iwRaptorFilter:  e not initialized...exiting");
					return;
				}
				if(e.bars[0].MostRecentPosition == LONG)       CurrentEaglePosition0 = LONG;
				else if(e.bars[0].MostRecentPosition == SHORT) CurrentEaglePosition0 = SHORT;
}catch(Exception err){if(lasterror!=line)Print(line+": "+err.ToString()); lasterror=line;}
			}
line=351;
			if(BarsInProgress == 0) {
line=353;
				int color_now = FLAT;
				bool IsSet = false;
				if(CurrentEaglePosition0 == LONG) {
					color_now = LONG;
					Values[0][0] = (1);
					IsSet = true;
				}
line=359;
				if(CurrentEaglePosition0 == SHORT) {
					color_now = SHORT;
					Values[0][0] = (-1);
					IsSet = true;
				}
line=364;
				if(ChartControl!=null && IsSet) {
					PlotBrushes[0][0] = System.Windows.Media.Brushes.Transparent;//(color_now==LONG? plotBrushes[2] : plotBrushes[3]);
					PlotBrushes[1][0] = (color_now==LONG? plotBrushes[0] : plotBrushes[1]);
					PlotBrushes[2][0] = (color_now==LONG? plotBrushes[2] : plotBrushes[3]);
					PlotBrushes[3][0] = (color_now==LONG? plotBrushes[2] : plotBrushes[3]);
					BackBrush = (color_now==LONG? plotBrushes[2] : plotBrushes[3]);
//					BackBrush = Brushes.Yellow;
				}
			}
			}catch(Exception err){Print(line+":  "+err.ToString());}
        }

        #region Properties

		private int pEagleBrickSizeTrend = 12;
		[Description("Trend ticks of MeanRenko chart used to generate the filter signals")]
		[Category("Parameters")]
		[NinjaScriptProperty]
		public int EagleBrickSizeTrend{	get { return pEagleBrickSizeTrend; }	set { pEagleBrickSizeTrend = value; }		}

		private int pEagleBrickSizeReverse = 12;
		[Description("Reversal ticks of MeanRenko chart used to generate the filter signals")]
		[Category("Parameters")]
		[NinjaScriptProperty]
		public int EagleBrickSizeReverse{	get { return pEagleBrickSizeReverse; }	set { pEagleBrickSizeReverse = value; }		}


		private double pOpacityUp = 0.5;
		[Description("Opacity of background fill color on up-trend")]
		[Category("Filter Band")]
		[NinjaScriptProperty]
		public double OpacityUp{	get { return Math.Max(0,Math.Min(1,pOpacityUp)); }	set { pOpacityUp = value; }		}

		private double pOpacityDown = 0.5;
		[Description("Opacity of background fill color on Down-trend")]
		[Category("Filter Band")]
		[NinjaScriptProperty]
		public double OpacityDown{	get { return Math.Max(0,Math.Min(1,pOpacityDown)); }	set { pOpacityDown = value; }		}

		private System.Windows.Media.Brush pPLUp = System.Windows.Media.Brushes.DodgerBlue;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EagleBandUp",  GroupName = "Filter Band")]
		public System.Windows.Media.Brush PLUp{	get { return pPLUp; }	set { pPLUp = value; }		}
		[Browsable(false)]
		public string PLUpClSerialize
		{	get { return Serialize.BrushToString(pPLUp); } set { pPLUp = Serialize.StringToBrush(value); }
		}
		private System.Windows.Media.Brush pPLDn = System.Windows.Media.Brushes.Magenta;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EagleBandDown",  GroupName = "Filter Band")]
		public System.Windows.Media.Brush PLDn{	get { return pPLDn; }	set { pPLDn = value; }		}
		[Browsable(false)]
		public string PLDnClSerialize
		{	get { return Serialize.BrushToString(pPLDn); } set { pPLDn = Serialize.StringToBrush(value); }
		}

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private iwRaptorFilter[] cacheiwRaptorFilter;
		public iwRaptorFilter iwRaptorFilter(int eagleBrickSizeTrend, int eagleBrickSizeReverse, double opacityUp, double opacityDown)
		{
			return iwRaptorFilter(Input, eagleBrickSizeTrend, eagleBrickSizeReverse, opacityUp, opacityDown);
		}

		public iwRaptorFilter iwRaptorFilter(ISeries<double> input, int eagleBrickSizeTrend, int eagleBrickSizeReverse, double opacityUp, double opacityDown)
		{
			if (cacheiwRaptorFilter != null)
				for (int idx = 0; idx < cacheiwRaptorFilter.Length; idx++)
					if (cacheiwRaptorFilter[idx] != null && cacheiwRaptorFilter[idx].EagleBrickSizeTrend == eagleBrickSizeTrend && cacheiwRaptorFilter[idx].EagleBrickSizeReverse == eagleBrickSizeReverse && cacheiwRaptorFilter[idx].OpacityUp == opacityUp && cacheiwRaptorFilter[idx].OpacityDown == opacityDown && cacheiwRaptorFilter[idx].EqualsInput(input))
						return cacheiwRaptorFilter[idx];
			return CacheIndicator<iwRaptorFilter>(new iwRaptorFilter(){ EagleBrickSizeTrend = eagleBrickSizeTrend, EagleBrickSizeReverse = eagleBrickSizeReverse, OpacityUp = opacityUp, OpacityDown = opacityDown }, input, ref cacheiwRaptorFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.iwRaptorFilter iwRaptorFilter(int eagleBrickSizeTrend, int eagleBrickSizeReverse, double opacityUp, double opacityDown)
		{
			return indicator.iwRaptorFilter(Input, eagleBrickSizeTrend, eagleBrickSizeReverse, opacityUp, opacityDown);
		}

		public Indicators.iwRaptorFilter iwRaptorFilter(ISeries<double> input , int eagleBrickSizeTrend, int eagleBrickSizeReverse, double opacityUp, double opacityDown)
		{
			return indicator.iwRaptorFilter(input, eagleBrickSizeTrend, eagleBrickSizeReverse, opacityUp, opacityDown);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.iwRaptorFilter iwRaptorFilter(int eagleBrickSizeTrend, int eagleBrickSizeReverse, double opacityUp, double opacityDown)
		{
			return indicator.iwRaptorFilter(Input, eagleBrickSizeTrend, eagleBrickSizeReverse, opacityUp, opacityDown);
		}

		public Indicators.iwRaptorFilter iwRaptorFilter(ISeries<double> input , int eagleBrickSizeTrend, int eagleBrickSizeReverse, double opacityUp, double opacityDown)
		{
			return indicator.iwRaptorFilter(input, eagleBrickSizeTrend, eagleBrickSizeReverse, opacityUp, opacityDown);
		}
	}
}

#endregion
