#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endregion

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
	public class SwingWaveCalculator : Indicator
	{
		private bool LicenseValid = true;

        #region Variables
//		private Graphics GRAPHICS = null;
//		private Rectangle BOUNDS = null;
//		private double MAX_PRICE = 0;
//		private double MIN_PRICE = 0;
		private string FS = null;
		private List<ChartObject> ObjList;
//		private List<ChartObject> LinesList;
//		private List<ChartObject> TextList;
		private bool RunInit = true;
		private int AbsBarOldestDot = int.MaxValue;
		private NinjaTrader.Gui.Tools.SimpleFont TxtFont=null, DotTxtFont=null;
//		private float TextHeight = -1;
		private string NL = Environment.NewLine;
		private string tab = "     ";
		private string StatisticsHeader = string.Empty;
		private Brush DotIdTextBrush, DotIdBackgroundBrush;
		private Brush TextBrush, TextBackgroundBrush;
		private Pen LinePen = null;
        #endregion
		#region ZigZag variables
		#region Constants
		private const int ESC_KEY_VALUE = 27;
		#endregion
        #region Variables
		double			currentZigZagHigh	= 0;
		double			currentZigZagLow	= 0;
		DeviationType	deviationType		= DeviationType.Percent;
		double			deviationValue		= 0.05;
		Series<double>		zigZagHighZigZags; 
		Series<double>		zigZagLowZigZags; 
		Series<double>		zigZagHighSeries; 
		Series<double>		zigZagLowSeries; 
		int				lastSwingIdx		= -1;
		double			lastSwingPrice		= 0.0;
		int				trendDir			= 0; // 1 = trend up, -1 = trend down, init = 0
		bool			useHighLow			= true;

        #endregion
		SortedDictionary<int,NinjaTrader.NinjaScript.DrawingTools.Dot> DotDir=null;
		string msg = string.Empty;
		string separation_string;
		List<int> LowBarsFound = new List<int>();
		List<int> HighBarsFound = new List<int>();
		string tag = string.Empty;
		ZigZag zz;
		#endregion

		private int LastPopupBar = 0;
		private int LastAudibleBar = 0;
		private int AudibleAlertsThisBar = 0;
		private int BarsCountAtStartup = 0;
		private int IgnoredDots =0;
//		private double AvgUps = 0;
//		private double AvgDowns = 0;
//		private double StdDevUps = 0;
//		private double StdDevDowns = 0;
//		private double AvgBoth = 0;
//		private double StdDev = 0;
		private Brush NormalBackColor;
		private List<double> UpDiffList   = new List<double>();
		private List<double> DownDiffList = new List<double>();
		private List<double> BarsList = new List<double>();
		private List<double> BarsHtoH = new List<double>();
		private List<double> BarsLtoL = new List<double>();
		private string stats1, stats2, stats3, stats4;
		private long SessionLengthTicks = TimeSpan.TicksPerDay;
		private double SessionLengthHrs = 24;


	protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			Name = "iw SwingWaveCalculator";
			if(!System.IO.File.Exists("c:\\222222222222.txt"))
				VendorLicense("IndicatorWarehouse", "AISwingWaveCalculator", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "CurrentSwingDistance");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "AvgUp");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "StdDevUp");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "AvgDn");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "StdDevDn");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "Avg");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "StdDev");
			Calculate=Calculate.OnBarClose;
			IsAutoScale=false;
			IsOverlay=true;
			IsChartOnly=true;
		}
		if (State == State.Configure)
		{
			RunInit = true;	
			ObjList = new List<ChartObject>();
//			LinesList = new List<ChartObject>();
//			TextList = new List<ChartObject>();
//			DotIdTextBrush = new SolidColorBrush(pDTextColor); DotIdTextBrush.Freeze();
//			DotIdBackgroundBrush = new SolidColorBrush(pDFillColor); DotIdBackgroundBrush.Freeze(); 
//			TextBrush = new SolidColorBrush(pTextColor); TextBrush.Freeze();
//			TextBackgroundBrush = new SolidColorBrush(pFillColor); TextBackgroundBrush.Freeze();
			LinePen=null;
			if(pLineWidth>0){
				LinePen = new Pen(Brushes.Transparent, pLineWidth);
				//LinePen.DashStyle = DashStyleHelper.Dash;
			}
			#region zigzag startup
			DotDir = new SortedDictionary<int,NinjaTrader.NinjaScript.DrawingTools.Dot>();
			zigZagHighSeries	= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
			zigZagHighZigZags	= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
			zigZagLowSeries		= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
			zigZagLowZigZags	= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded

			separation_string = string.Concat(pSeparation,"_");
			tag = separation_string+"ZZdon";
			zz = ZigZag(deviationType, deviationValue, useHighLow);
			#endregion
			NormalBackColor = ChartControl.Properties.ChartBackground;
//			Print("BeginTimeTicks: "+pBeginTime1.Ticks);
//			Print("EndTimeTicks: "+pEndTime1.Ticks);
			if(this.pBeginTime1 == this.pEndTime1) SessionLengthTicks = TimeSpan.TicksPerDay;
			else if(this.pBeginTime1 > this.pEndTime1) {
				long t = TimeSpan.TicksPerDay - (pBeginTime1.Ticks - pEndTime1.Ticks);
				SessionLengthTicks = t;
			}
			else {
				long t = pEndTime1.Ticks - pBeginTime1.Ticks;
				SessionLengthTicks = t;
			}
			IsAutoScale = false;
			SessionLengthHrs = (double)SessionLengthTicks / TimeSpan.TicksPerHour;
			//Print("SessionLengthHrs: "+SessionLengthTicks+"   "+SessionLengthHrs.ToString("0.00-hrs"));
			//DateTime dt = new DateTime(NinjaTrader.Core.Globals.Now.Ticks + TimeSpan.TicksPerHour);
			//if(dt > NinjaTrader.Core.Globals.Now && dt <= NinjaTrader.Core.Globals.Now.AddHours(SessionLengthHrs)) Print("Yes"); else Print("No");
		}
	}

		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;

			if(pEnableZigZag) {
				#region ZigZag OnBarUpdate
				if(IsFirstTickOfBar) this.AudibleAlertsThisBar = 0;
				if(BarsCountAtStartup == 0) BarsCountAtStartup = Bars.Count;
				if (CurrentBar < 2) {// need 3 bars to calculate Low/High
					zigZagHighSeries[0] = (0);
					zigZagHighZigZags[0] = (0);
					zigZagLowSeries[0] = (0);
					zigZagLowZigZags[0] = (0);
					return;
				}

				int lowbar = zz.LowBar(0, 1, 1000);
				int abslowbar = -1;
				if(lowbar>0) {
					abslowbar = CurrentBar-lowbar;
					if(LowBarsFound.Count==0) LowBarsFound.Insert(0,abslowbar);
					else if(abslowbar != LowBarsFound[0]) LowBarsFound.Insert(0,abslowbar);
				}
				int highbar = zz.HighBar(0, 1, 1000);
				int abshighbar = -1;
				if(highbar>0) {
					abshighbar = CurrentBar-highbar;
					if(HighBarsFound.Count==0) HighBarsFound.Insert(0,abshighbar);
					else if(abshighbar != HighBarsFound[0]) HighBarsFound.Insert(0,abshighbar);
				}

				if(Time[0].Ticks < pDotsStartTime.Ticks) return;
				if(abshighbar>abslowbar && HighBarsFound.Count>1) {
					if(LowBarsFound.Count>0 && HighBarsFound[1]>LowBarsFound[0]) RemoveDrawObject(string.Concat(tag,HighBarsFound[1]));
					var aaa=Draw.Dot(this, string.Format("{0}{1}", tag, HighBarsFound[0]), true, highbar, High[highbar]+pSeparation*TickSize, Plots[0].Pen.Brush);
					aaa.IsLocked=false;
					if((State != State.Historical)&& CurrentBar>BarsCountAtStartup+1) {
						if(LastAudibleBar != CurrentBar-highbar) {
							if(pEnableSoundOnDot) {
								if(AudibleAlertsThisBar<pMaxAudibleAlertsPerBar) {
									PlaySound(AddSoundFolder(pNewHighWAV));
									AudibleAlertsThisBar++;
									LastAudibleBar = 0;//this enables the audible to play more than once on this signal
								}
								if(AudibleAlertsThisBar>=pMaxAudibleAlertsPerBar) LastAudibleBar = CurrentBar-highbar;
							}
						}
						if(pEnablePopupOnDot && LastPopupBar != CurrentBar-highbar) Log("SwingWaveCalculator"+NL+"New high dot printed on "+NL+Instrument.FullName+NL+Bars.BarsPeriod.ToString(),LogLevel.Alert);
						LastPopupBar = CurrentBar-highbar;
					}
				}
				if(abslowbar>abshighbar && LowBarsFound.Count>1) {
					if(HighBarsFound.Count>0 && LowBarsFound[1]>HighBarsFound[0]) RemoveDrawObject(string.Concat(tag,LowBarsFound[1]));
					NinjaTrader.NinjaScript.DrawingTools.Dot aaa=Draw.Dot(this, string.Format("{0}{1}",tag,LowBarsFound[0]), true, lowbar, Low[lowbar]-pSeparation*TickSize, Plots[0].Pen.Brush);
					aaa.IsLocked=false;
					if((State != State.Historical)&& CurrentBar>BarsCountAtStartup+1) {
						if(LastAudibleBar != CurrentBar-lowbar) {
							if(pEnableSoundOnDot) {
								if(AudibleAlertsThisBar<pMaxAudibleAlertsPerBar) {
									PlaySound(AddSoundFolder(pNewLowWAV));
									AudibleAlertsThisBar++;
									LastAudibleBar = 0;//this enables the audible to play more than once on this signal
								}
								if(AudibleAlertsThisBar>=pMaxAudibleAlertsPerBar) LastAudibleBar = CurrentBar-lowbar;
							}
						}
						if(pEnablePopupOnDot && LastPopupBar != CurrentBar-lowbar) Log("SwingWaveCalculator"+NL+"New low dot on "+NL+Instrument.FullName+NL+Bars.BarsPeriod.ToString(),LogLevel.Alert);
						LastPopupBar = CurrentBar-lowbar;
					}
				}
			#endregion
			}

			try {
				if (CurrentBar < 1) return;
				
				if(RunInit) {
					if(ChartControl == null) return;
					RunInit     = false;
					IgnoredDots = 0;

					TxtFont = (NinjaTrader.Gui.Tools.SimpleFont)ChartControl.Properties.LabelFont.Clone();
					TxtFont.Size = (int)pTxtSize;
//					TxtFont = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.Family.ToString(), (int)pTxtSize);
					DotTxtFont = (NinjaTrader.Gui.Tools.SimpleFont)ChartControl.Properties.LabelFont.Clone();
					DotTxtFont.Size = (int)pDotTxtSize;
//					DotTxtFont = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.Family.ToString(), (int) pDotTxtSize);
//					int PriceDigits = 0;
//					FS = TickSize.ToString();
//					if(FS.Contains("E-")) {
//						FS = FS.Substring(FS.IndexOf("E-")+2);
//						PriceDigits = int.Parse(FS);
//					}
//					else PriceDigits = Math.Max(0,FS.Length-2);

//					if(PriceDigits==0) FS="0";
//					if(PriceDigits==1) FS="0.0";
//					if(PriceDigits==2) FS="0.00";
//					if(PriceDigits==3) FS="0.000";
//					if(PriceDigits==4) FS="0.0000";
//					if(PriceDigits==5) FS="0.00000";
//					if(PriceDigits==6) FS="0.000000";
//					if(PriceDigits==7) FS="0.0000000";
//					if(PriceDigits>=8) FS="0.00000000";
					//Print("ChartObjectCount= " + this.ChartControl.ChartObjects.Count);
				}
//				if(Time[0].Day==27 && Time[0].Month==7) {
//					if(CurrentBar % 20 ==0) Draw.Dot(this, "xxx"+CurrentBar.ToString(),false,0,Close[0],Color.Red);
//				}
//				AvgUp[0] = (AvgUps);
//				AvgDn[0] = (AvgDowns);
//				StdDevUp[0] = (StdDevUps);
//				StdDevDn[0] = (StdDevDowns);
//				Avg[0] = (AvgBoth);
//				OneStdDev[0] = (StdDev);
			}
			catch( Exception e) {
				Print("SwingWaveCalculator EXCEPTION on " + Instrument +  e.ToString() );
				Print(e.Message);
				Alert(this.GetType().Name + Instrument.FullName,Priority.High,"Instrument=" + Instrument + "   **************EXCEPTION" + e.ToString(),AddSoundFolder("Alert2.wav"), 60, Brushes.Red,Brushes.White);
			}
		}
//=========================================================================================================================
		private double GrabPrice(int relbar, ChartObject dot) {
			double p = 0;
			if(pWhichPrice == SwingWaveCalculator_NearestPrice.NearestHighLow) {
				var d = (IDrawingTool)dot;
				p = d.Anchors.First().Price;
				double dh = Math.Abs(High[relbar]-p);
				double dl = Math.Abs(Low[relbar]-p);
				if(dh<dl) return High[relbar];
				else return Low[relbar];
			}
			if(pWhichPrice == SwingWaveCalculator_NearestPrice.NearestClose) {
				return Close[relbar];
			}
			return(p);
		}
//=========================================================================================================================
		public void GetStatistics(int InternalControlNumber, int AbsBarAtCalculation, ref double CurrentSwingDistance, ref double Avg, ref double StdDev,ref double AvgUps, ref double AvgDowns, ref double StdDevUps, ref double StdDevDowns) {
//			Update();
try{
			bool IsInternal = false;
			if(InternalControlNumber == 442789909) IsInternal = true;

			bool isManualDot = false;
			bool isAutoDot = false;
			ObjList.Clear();
//			LinesList.Clear();
//			TextList.Clear();
//			if(TextHeight == -1) {
//  //				SizeF size = graphics.MeasureString("X", DotTxtFont);
//				TextHeight = size.Height;
//			}
			IgnoredDots = 0;
			List<int> BarsWithSquares = new List<int>();
			if(pEnableBreakOnSquare) {
			    foreach(ChartObject CO in this.ChartControl.ChartObjects) 
				{
//A Square on the chart will break-up the zigzag...it's a zigzag terminator
				   	if ( CO is Square)
					{
						Square sq = (Square)CO;
						//Print("Dot Value= " + CO.Y.ToString("0.00")+"  Time: "+Time[CurrentBar-CO.StartBar].ToString()+"   Name: "+CO.ToString());
						sq.Anchor.Price = Math.Round(CO.Y/TickSize,0)*TickSize;
						IgnoredDots++;
						bool valid = pDotSource == SwingWaveCalculator_DotOrigination.Manually;// && CO.Persist;
						valid = valid || pDotSource == SwingWaveCalculator_DotOrigination.Programmatically;// && !CO.Persist;
						valid = valid || pDotSource == SwingWaveCalculator_DotOrigination.Both;
						valid = valid && sq.Anchor.DrawnOnBar <= AbsBarAtCalculation;
						if(valid) {
							ObjList.Add(CO);
							BarsWithSquares.Add(sq.Anchor.DrawnOnBar);
							AbsBarOldestDot = Math.Min(AbsBarOldestDot, sq.Anchor.DrawnOnBar);
							IgnoredDots--;
						}
						//Print(Environment.NewLine);
					}
				}
			}
		    foreach(ChartObject CO in this.ChartControl.ChartObjects) 
			{
//Print("ChartObjectType: " + CO.GetType().Name+"  "+CO.Tag );
			   	if ( CO is Dot){
					var d = (Dot)CO;
					if(!BarsWithSquares.Contains(d.Anchor.DrawnOnBar))
					{
						CO.Y = Math.Round(CO.Y/TickSize,0)*TickSize;
						NinjaTrader.NinjaScript.DrawingTools.Dot dot = (NinjaTrader.NinjaScript.DrawingTools.Dot) CO;
						IgnoredDots++;
						isManualDot = pDotSource == SwingWaveCalculator_DotOrigination.Manually && CO.Persist;
						if(isManualDot && this.EnableManualDotColorFilter && pManualKeyDotColor!=dot.Color) isManualDot = false;
						isAutoDot = pDotSource == SwingWaveCalculator_DotOrigination.Programmatically && !CO.Persist;
						if(isAutoDot && this.EnableAutoDotColorFilter && pAutoKeyDotColor!=dot.Color) isAutoDot = false;
						bool valid = (isManualDot || isAutoDot) || pDotSource == SwingWaveCalculator_DotOrigination.Both;
						valid = valid && CO.StartBar <= AbsBarAtCalculation;
						if(valid && SessionLengthHrs<24) {
							DateTime start = new DateTime(dot.Time.Year,dot.Time.Month,dot.Time.Day,pBeginTime1.Hours,pBeginTime1.Minutes,pBeginTime1.Seconds);
							DateTime end = start.AddHours(SessionLengthHrs);
							if(dot.Time >= start && dot.Time < end) 
								valid = true;
							else {
								start = new DateTime(dot.Time.Year,dot.Time.Month,dot.Time.Day,pBeginTime1.Hours,pBeginTime1.Minutes,pBeginTime1.Seconds).AddDays(-1);
								end = start.AddHours(SessionLengthHrs);
								if(dot.Time >= start && dot.Time < end) 
									valid = true; 
								else 
									valid = false;
							}
						}
						if(valid) {
//Print("Dot Value= " + CO.Y.ToString(FS)+"  Time: "+Time[CurrentBar-CO.StartBar].ToString()+"   Name: "+CO.ToString()+"  Persist: "+CO.Persist.ToString()+"  Selectable: "+CO.Selectable.ToString());
							ObjList.Add(CO);
							AbsBarOldestDot = Math.Min(AbsBarOldestDot, CO.StartBar);
							IgnoredDots--;
						}
						//Print(Environment.NewLine);
					}
				}
			}
			if(!pShowStatistics) IgnoredDots=0;
			ObjList.Sort(CompareObjectsByBar);

			double LeftPrice  = 0;
			int LeftBar       = 0;
			double RightPrice = 0;
			int RightBar      = 0;
			UpDiffList.Clear();
			DownDiffList.Clear();
			BarsList.Clear();
			BarsHtoH.Clear();
			BarsLtoL.Clear();

			double TotalUpDiffs   = 0;
//			int UpDiffsCount      = 0;
			double TotalDownDiffs = 0;
//			int DownDiffsCount    = 0;
			int DotId = 1;
			for(int i = 0; i<ObjList.Count; i++) {
//Print("Dot Value= " + ObjList[i].Y.ToString(FS)+"  Time: "+ObjList[i].Time.ToString()+"  Persist: "+ObjList[i].Persist.ToString()+"  Selectable: "+ObjList[i].Selectable.ToString());
				if(LeftBar == 0) {
					LeftBar   = CurrentBar-ObjList[i].StartBar;
					LeftPrice = GrabPrice(LeftBar, ObjList[i]);
					DotId = 1;
					if(IsInternal && pShowDotId && GRAPHICS != null) DrawDotId(LeftBar, ObjList[i].Y, DotId);
				} else {
					RightBar   = CurrentBar-ObjList[i].StartBar;
					RightPrice = GrabPrice(RightBar, ObjList[i]);
					DotId++;
					if(IsInternal && pShowDotId && GRAPHICS != null) DrawDotId(RightBar, ObjList[i].Y, DotId);
//Print("CurrentBar >= ObjListEnd.StartBar: "+CurrentBar+" >= "+ObjList[ObjList.Count-1].StartBar);
					if(CurrentBar+1>=ObjList[ObjList.Count-1].StartBar) {
						int halfbar      = (LeftBar+RightBar)/2;
						double halfprice = (LeftPrice+RightPrice)/2.0;
						double ptsdiff   = RightPrice-LeftPrice;
						string msg       = string.Empty;
						int tickdiff     = (int)(Math.Round(ptsdiff/TickSize,0));
						long voldiff     = 0;

Brush LineColor = Brushes.Transparent;
						if(pDnLineColor!=Color.Transparent && pDnLineColor!=Color.Empty && pDnLineColor!=ChartControl.Properties.ChartBackground) 
							LineColor = pDnLineColor;

						if(tickdiff>0) {
							if(pUpLineColor!=Color.Transparent && pUpLineColor!=Color.Empty && pUpLineColor!=ChartControl.Properties.ChartBackground) LineColor = pUpLineColor;
							else LineColor = Brushes.Transparent;
						}


						if(IsInternal && LinePen!=null && GRAPHICS != null) {
							LinePen.Color = LineColor;
//							Draw.Line(this, "swcl"+i.ToString(), false, LeftBar, LeftPrice, RightBar, RightPrice, pLineColor, DashStyleHelper.Dash, pLineWidth);
							float x0 = (float)chartControl.GetXByBarIndex(ChartBars,Bars, CurrentBar-LeftBar);
							float y0 = (float)chartScale.GetYByValue( LeftPrice);
							float x1 = (float)chartControl.GetXByBarIndex(ChartBars,Bars, CurrentBar-RightBar);
							float y1 = (float)chartScale.GetYByValue( RightPrice);
							GRAPHICS.Draw.Line(this, LinePen, x0, y0, x1, y1);
						}

						if(pOutputType == SwingWaveCalculator_OutputType.Ticks) {
							msg = tickdiff.ToString();
							if(tickdiff>0) { TotalUpDiffs += tickdiff;      UpDiffList.Add(tickdiff);   }
							if(tickdiff<0) { TotalDownDiffs += tickdiff;  DownDiffList.Add(-tickdiff);}
							CurrentSwingDistance = tickdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.Points) {
							msg = Instrument.MasterInstrument.FormatPrice(ptsdiff);
							if(ptsdiff>0) { TotalUpDiffs += ptsdiff;      UpDiffList.Add(ptsdiff);   }
							if(ptsdiff<0) { TotalDownDiffs += ptsdiff;  DownDiffList.Add(-ptsdiff);}
							CurrentSwingDistance = ptsdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.DollarValue) {
							msg = (ptsdiff*Instrument.MasterInstrument.PointValue).ToString("$0");
							if(ptsdiff>0) { TotalUpDiffs += ptsdiff;      UpDiffList.Add(ptsdiff*Instrument.MasterInstrument.PointValue);   }
							if(ptsdiff<0) { TotalDownDiffs += ptsdiff;  DownDiffList.Add(-ptsdiff*Instrument.MasterInstrument.PointValue);}
							CurrentSwingDistance = ptsdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.Bars) {
							msg = ((int)Math.Abs(RightBar-LeftBar)).ToString();
							int bardiff = Math.Abs(LeftBar-RightBar);
							if(ptsdiff>0) { TotalUpDiffs += bardiff;      UpDiffList.Add(bardiff);  BarsList.Add(bardiff);}
							if(ptsdiff<0) { TotalDownDiffs += bardiff;  DownDiffList.Add(bardiff);  BarsList.Add(-bardiff);}
//Print("UpDiffList.Count: "+UpDiffList.Count+"  Down: "+DownDiffList.Count);
							CurrentSwingDistance = bardiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.TicksAndBars) {
							msg = string.Concat(tickdiff.ToString("0")," / ",((int)Math.Abs(RightBar-LeftBar)).ToString());
							if(tickdiff>0) { TotalUpDiffs += tickdiff;      UpDiffList.Add(tickdiff);   }
							if(tickdiff<0) { TotalDownDiffs += tickdiff;  DownDiffList.Add(-tickdiff);}
							CurrentSwingDistance = tickdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.PointsAndBars) {
							msg = string.Concat(Instrument.MasterInstrument.FormatPrice(ptsdiff)," / ",((int)Math.Abs(RightBar-LeftBar)).ToString());
							if(ptsdiff>0) { TotalUpDiffs += ptsdiff;      UpDiffList.Add(ptsdiff);   }
							if(ptsdiff<0) { TotalDownDiffs += ptsdiff;  DownDiffList.Add(-ptsdiff);}
							CurrentSwingDistance = ptsdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.DollarsAndBars) {
							msg = string.Concat((ptsdiff*Instrument.MasterInstrument.PointValue).ToString("$0")," / ",((int)Math.Abs(RightBar-LeftBar)).ToString());
							if(ptsdiff>0) { TotalUpDiffs += ptsdiff;      UpDiffList.Add(ptsdiff*Instrument.MasterInstrument.PointValue);   }
							if(ptsdiff<0) { TotalDownDiffs += ptsdiff;  DownDiffList.Add(-ptsdiff*Instrument.MasterInstrument.PointValue);}
							CurrentSwingDistance = ptsdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.Volume) {
							voldiff = CalculateVolumeDiff(LeftBar, RightBar);
							msg = voldiff.ToString("0");
							if(RightPrice<LeftPrice) voldiff=-voldiff;
							if(voldiff>0) { TotalUpDiffs += voldiff;      UpDiffList.Add(voldiff);   }
							if(voldiff<0) { TotalDownDiffs += voldiff;  DownDiffList.Add(-voldiff);}
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.VolumeAndBars) {
							voldiff = CalculateVolumeDiff(LeftBar, RightBar);
							msg = string.Concat(voldiff.ToString("0")," / ",((int)Math.Abs(RightBar-LeftBar)).ToString());
							if(RightPrice<LeftPrice) voldiff=-voldiff;
							if(voldiff>0) { TotalUpDiffs += voldiff;      UpDiffList.Add(voldiff);   }
							if(voldiff<0) { TotalDownDiffs += voldiff;  DownDiffList.Add(-voldiff);}
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.TicksAndVolume) {
							voldiff = CalculateVolumeDiff(LeftBar, RightBar);
							msg = string.Concat(tickdiff.ToString("0")," / ",voldiff.ToString("0"));
							if(tickdiff>0) { TotalUpDiffs += tickdiff;      UpDiffList.Add(tickdiff);   }
							if(tickdiff<0) { TotalDownDiffs += tickdiff;  DownDiffList.Add(-tickdiff);}
							CurrentSwingDistance = tickdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.PointsAndVolume) {
							voldiff = CalculateVolumeDiff(LeftBar, RightBar);
							msg = string.Concat(Instrument.MasterInstrument.FormatPrice(ptsdiff)," / ",voldiff.ToString("0"));
							if(ptsdiff>0) { TotalUpDiffs += ptsdiff;      UpDiffList.Add(ptsdiff);   }
							if(ptsdiff<0) { TotalDownDiffs += ptsdiff;  DownDiffList.Add(-ptsdiff);}
							CurrentSwingDistance = ptsdiff;
						}
						else if(pOutputType == SwingWaveCalculator_OutputType.DollarsAndVolume) {
							voldiff = CalculateVolumeDiff(LeftBar, RightBar);
							msg = string.Concat((ptsdiff*Instrument.MasterInstrument.PointValue).ToString("$0")," / ",voldiff.ToString("0"));
							if(ptsdiff>0) { TotalUpDiffs += ptsdiff;      UpDiffList.Add(ptsdiff*Instrument.MasterInstrument.PointValue);   }
							if(ptsdiff<0) { TotalDownDiffs += ptsdiff;  DownDiffList.Add(-ptsdiff*Instrument.MasterInstrument.PointValue);}
							CurrentSwingDistance = ptsdiff;
						}
//Print(msg);
//						Draw.Text(this, "swct"+i.ToString(), false, msg, halfbar, halfprice, 0, pTextColor, TxtFont, TextAlignment.Center, Brushes.Transparent, pFillColor, 10);
						if(pOutputType != SwingWaveCalculator_OutputType.None && IsInternal && GRAPHICS != null) {
//  							SizeF size = GRAPHICS.MeasureString(msg, TxtFont);
							float x,y;
							if(pSeparationDistance<0) {
								x = (float)chartControl.GetXByBarIndex(ChartBars,Bars, CurrentBar-halfbar)-txtLayout.Metrics.Width/2;
								y = (float)chartScale.GetYByValue( halfprice);
							} else {
								x = (float)chartControl.GetXByBarIndex(ChartBars,Bars, ObjList[i].StartBar) - txtLayout.Metrics.Width - 4;
								y = (float)chartScale.GetYByValue( ObjList[i].Y) - (CurrentSwingDistance < 0 ? -pSeparationDistance : size.Height+pSeparationDistance);
							}
							TextBackgroundBrush.Color = LineColor;
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x,y,txtLayout.Metrics.Width, size.Height), TextBackgroundBrushDX);
//							GRAPHICS.FillRectangle(TextBackgroundBrush, x, y, txtLayout.Metrics.Width, size.Height);
							RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y), txtLayout, msg, TxtFont, TextBrush, x, y);
						}
						LeftBar   = RightBar;
						LeftPrice = RightPrice;
					}
				}
				if(pEnableBreakOnSquare && ObjList[i] is NinjaTrader.Gui.Chart.ChartSquare) LeftBar=0;
				//Print("Obj["+i+"]:  bar: "+ObjList[i].Persist);
			}
			if(pShowStatistics) {
//				string numformat = "0.0";
				string info1 = "";
				if(IgnoredDots>0) info1 = MakeString(new Object[]{NL,NL,IgnoredDots,"-dots have been ignored"});
				IgnoredDots=0;
//CurrentSwingDistance
				if(pOutputType == SwingWaveCalculator_OutputType.Bars) {
					StatisticsHeader = "All statistics in bars";
					AvgUps      = UpDiffList.Count>0?   TotalUpDiffs   / UpDiffList.Count : double.PositiveInfinity;
					AvgDowns    = DownDiffList.Count>0? TotalDownDiffs / DownDiffList.Count : double.PositiveInfinity;
					StdDevUps   = Deviation(UpDiffList, AvgUps);
					StdDevDowns = Deviation(DownDiffList, AvgDowns);
					Avg         = (TotalUpDiffs + TotalDownDiffs) / (UpDiffList.Count + DownDiffList.Count);
					int UpDiffCount = UpDiffList.Count;
					foreach(double bd in DownDiffList) UpDiffList.Add(Math.Abs(bd));
					StdDev      = Deviation(UpDiffList, Avg);
					double bh = 0, bl=0;
					foreach(double bdiff in BarsList) {
						if(bdiff>0) {
							if(bl!=0) BarsHtoH.Add(bdiff + bl);
							bh = bdiff;
						}
						else if(bdiff<0) {
							if(bh!=0) BarsLtoL.Add(-bdiff + bh);
							bl = -bdiff;
						}
					}
					if(IsInternal) {
						stats1 = MakeString(new Object[]{StatisticsHeader,NL,UpDiffCount," Up moves:",NL,tab,"avg: ",Instrument.MasterInstrument.FormatPrice(AvgUps),NL,tab,"StdDev: ",Instrument.MasterInstrument.FormatPrice(StdDevUps),info1});
						stats2 = MakeString(new Object[]{DownDiffList.Count," Down moves:",NL,tab,"avg: ",Instrument.MasterInstrument.FormatPrice(AvgDowns),NL,tab,"StdDev: ",Instrument.MasterInstrument.FormatPrice(StdDevDowns)});
						stats3 = MakeString(new Object[]{"Net ups and downs:",NL,tab,"avg: ",Instrument.MasterInstrument.FormatPrice(Avg),NL,tab,"StdDev: ",Instrument.MasterInstrument.FormatPrice(StdDev)});
						stats4 = MakeString(new Object[]{"HtoH avg: ",(BarsHtoH.Count>0?BarsHtoH.Average().ToString("0.0"):"N/A"),NL,"LtoL avg: ",(BarsLtoL.Count>0?BarsLtoL.Average().ToString("0.0"):"N/A")});
						stats1 = MakeString(new Object[]{stats1,NL,stats2,NL,stats3,NL,NL,stats4,info1});
						Draw.TextFixed(this, "stats",stats1, pStatsLocation,ChartControl.Properties.AxisPen.Brush,ChartControl.Properties.LabelFont, NormalBackColor, NormalBackColor, 10);
						//Print(stats1);
					}
				} else {
					if(pOutputType == SwingWaveCalculator_OutputType.Ticks)               StatisticsHeader = "All statistics in ticks";
					else if(pOutputType == SwingWaveCalculator_OutputType.Points)        {StatisticsHeader = "All statistics in points"; }
					else if(pOutputType == SwingWaveCalculator_OutputType.TicksAndBars)   StatisticsHeader = "All statistics in ticks";
					else if(pOutputType == SwingWaveCalculator_OutputType.PointsAndBars) {StatisticsHeader = "All statistics in points"; }
					AvgUps   = TotalUpDiffs / UpDiffList.Count;
					AvgDowns = TotalDownDiffs / DownDiffList.Count;
					Avg      = (TotalUpDiffs - TotalDownDiffs) / (UpDiffList.Count + DownDiffList.Count);
					StdDevUps   = Deviation(UpDiffList, AvgUps);
					StdDevDowns = Deviation(DownDiffList, AvgUps);
					int UpDiffCount = UpDiffList.Count;
					foreach(double diff in DownDiffList) {UpDiffList.Add(diff);}
					StdDev      = Deviation(UpDiffList, Avg);
					if(IsInternal) {
						stats1 = MakeString(new Object[]{StatisticsHeader,NL,UpDiffCount," Up moves:",NL,tab,"avg: ",Instrument.MasterInstrument.FormatPrice(AvgUps),NL,tab,"StdDev: ",Instrument.MasterInstrument.FormatPrice(StdDevUps)});
						stats2 = MakeString(new Object[]{DownDiffList.Count," Down moves:",NL,tab,"avg: ",Instrument.MasterInstrument.FormatPrice(AvgDowns),NL,tab,"StdDev: ",Instrument.MasterInstrument.FormatPrice(StdDevDowns)});
						stats3 = MakeString(new Object[]{"Net ups and downs:",NL,tab,"avg: ",Instrument.MasterInstrument.FormatPrice(Avg),NL,tab,"StdDev: ",Instrument.MasterInstrument.FormatPrice(StdDev)});
						stats1 = MakeString(new Object[]{stats1,NL,stats2,NL,stats3,info1});
						Draw.TextFixed(this, "stats",stats1, pStatsLocation,ChartControl.Properties.AxisPen.Brush,ChartControl.Properties.LabelFont, NormalBackColor, NormalBackColor, 10);
						//Print(stats1);
					}
				}
			}


} catch( Exception e) {
	Print("SwingWaveCalculator GetStatistics EXCEPTION on " + Instrument +  e.ToString() );
	Print(e.Message);
	Alert(this.GetType().Name + Instrument.FullName,Priority.High,"Instrument=" + Instrument + "   **************EXCEPTION" + e.ToString(),AddSoundFolder("Alert2.wav"), 60, Brushes.Red,Brushes.White);
}
		}
//==================================================================
		private long CalculateVolumeDiff(int LeftBarRel, int RightBarRel){
			long vol = 0;//(long)Volume[RightBarRel];
			for(int b = RightBarRel; b<LeftBarRel; b++) {
				vol = vol + (long)Volume[b];
			}
			return vol;
		}
		private SharpDX.Direct2D1.Brush TextBackgroundBrushDX = null;
		private SharpDX.Direct2D1.Brush DotIdTextBrushDX = null;
		private SharpDX.Direct2D1.Brush DotIdBackgroundBrushDX = null;
		private SharpDX.Direct2D1.Brush TextBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(TextBrushDX!=null && !TextBrushDX.IsDisposed) TextBrushDX.Dispose(); TextBrushDX=null;
			if(RenderTarget!= null)
				TextBrushDX = pTextColor.ToDxBrush(RenderTarget);

			TextBackgroundBrush = new SolidColorBrush(pFillColor); TextBackgroundBrush.Freeze();
			if(TextBackgroundBrushDX!=null && !TextBackgroundBrushDX.IsDisposed) TextBackgroundBrushDX.Dispose(); TextBackgroundBrushDX=null;
			if(RenderTarget!= null)
				TextBackgroundBrushDX = TextBackgroundBrush.ToDxBrush(RenderTarget);

			if(DotIdTextBrushDX!=null && !DotIdTextBrushDX.IsDisposed) DotIdTextBrushDX.Dispose(); DotIdTextBrushDX=null;
			if(RenderTarget!= null)
				DotIdTextBrushDX = pDTextColor.ToDxBrush(RenderTarget);

			if(DotIdBackgroundBrushDX!=null && !DotIdBackgroundBrushDX.IsDisposed) DotIdBackgroundBrushDX.Dispose(); DotIdBackgroundBrushDX=null;
			if(RenderTarget!= null)
				DotIdBackgroundBrushDX = pDFillColor.ToDxBrush(RenderTarget);
		}
//==================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!IsVisible) return;double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
			base.OnRender(chartControl, chartScale);
			Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
			Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = ChartBars.ToIndex;

			if(pEnableZigZag) {
				#region zigzag Plot method
			GetNinjaTrader.NinjaScript.DrawingTools.Dots(ref DotDir);
			if (Bars == null || ChartControl == null || DotDir == null)
				return;

//			GraphicsPath	path		= new GraphicsPath();
			int				barWidth	= ChartControl.chartControl.GetBarPaintWidth(ChartBars);;

			bool linePlotted = false;
			List<NinjaTrader.NinjaScript.DrawingTools.Dot> objs = new List<NinjaTrader.NinjaScript.DrawingTools.Dot>(DotDir.Values);
			double priorValue = 0;
			double value = 0;

			for(int obj = 0; obj<objs.Count; obj++) {
				if(value != 0) priorValue = value;

				bool isHigh	= objs[obj].Y >= Median[objs[obj].BarsAgo];
				bool isLow	= objs[obj].Y <= Median[objs[obj].BarsAgo];
				if(isHigh) value = High[objs[obj].BarsAgo];
				if(isLow) value = Low[objs[obj].BarsAgo];

				if (obj > 0)
				{
					int x0 = chartControl.GetXByBarIndex(ChartBars,BarsArray[0], CurrentBar-objs[obj-1].BarsAgo);//(int) (ChartControl.CanvasRight - ChartControl.Properties.BarMarginRightRight - barWidth / 2 - (ChartControl.BarsPainted - 1) * ChartControl.Properties.BarDistance + lastCount * ChartControl.Properties.BarDistance) + 1;
					int x1 = chartControl.GetXByBarIndex(ChartBars,BarsArray[0], CurrentBar-objs[obj].BarsAgo);//(int) (ChartControl.CanvasRight - ChartControl.Properties.BarMarginRightRight - barWidth / 2 - (ChartControl.BarsPainted - 1) * ChartControl.Properties.BarDistance + count * ChartControl.Properties.BarDistance) + 1;
					int y0 = chartScale.GetYByValue( min)) * ChartPanel.H);
					int y1 = chartScale.GetYByValue( min)) * ChartPanel.H);

//					path.AddLine(x0, y0, x1, y1);
					linePlotted = true;
				}
			}

//			SmoothingMode oldSmoothingMode = RenderTarget.SmoothingMode;
//			RenderTarget.SmoothingMode = SmoothingMode.AntiAlias;
//			RenderTarget.DrawPath(Plots[0].Pen, path);
//			RenderTarget.SmoothingMode = oldSmoothingMode;

			if (!linePlotted) Draw.TextFixed(this, "ErrorMsg", "'"+Name+"' can not plot any values since the deviation value is too large. Please reduce it.", TextPosition.BottomRight);
			else RemoveDrawObject("ErrorMsg");

			#endregion
			}

//			GRAPHICS = graphics;
//			BOUNDS = bounds;
//			MIN_PRICE = minPrice;
//			MAX_PRICE = maxPrice;
			double CurrentSwingDistance=0, Avg=0, StdDev=0, AvgUps=0, AvgDowns=0, StdDevUps=0, StdDevDowns=0;
			GetStatistics((int)442789909, Bars.Count, ref CurrentSwingDistance, ref Avg, ref StdDev, ref AvgUps, ref AvgDowns, ref StdDevUps, ref StdDevDowns);

        }
//==================================================================
		private void GetDots(ref SortedDictionary<int,NinjaTrader.NinjaScript.DrawingTools.Dot> DotDir) {
			//Runs through all the objects on the chart and puts the Arrows and Dots and Squares into a SortedDictionary
			#region GetDots

//			int abar = 0;
			bool WarningInfoPrinted = false;
			try{
			if(DotDir==null) DotDir = new SortedDictionary<int,NinjaTrader.NinjaScript.DrawingTools.Dot>();
			DotDir.Clear();
			if(this.ChartControl==null || this.ChartControl.ChartObjects==null) {Print("No ChartControl...exiting GetNinjaTrader.NinjaScript.DrawingTools.Dots");return;}
			foreach(ChartObject CO in this.ChartControl.ChartObjects) 
			{

				if(CO==null) continue;
				if(CO.Tag.IndexOf(separation_string)!=0) continue;

			   	if ( CO is NinjaTrader.Gui.Chart.ChartDot) {
					NinjaTrader.NinjaScript.DrawingTools.Dot idot = (NinjaTrader.NinjaScript.DrawingTools.Dot)CO;
					DotDir[CO.StartBar] = idot;
				}
			}

			} catch( Exception e) {
//				Print(abar+": "+e.Message);
			}
			#endregion
		}
//==================================================================
		private double Deviation(List<double> nums, double avg){
			double sum = 0;
			for(int i = 0; i<nums.Count; i++) {
				sum = Math.Pow(nums[i]-avg,2)+sum;
			}
			sum = sum / (nums.Count-1);
			return Math.Sqrt(sum);
		}
//==================================================================
		private void DrawDotId(int RelBar, double DotPrice, int DotId){
			double avg = Median[RelBar];
			if(RelBar < CurrentBar - 3) {
				avg = avg + Median[RelBar+1] + Median[RelBar+2];
				avg = avg / 3.0;
			}
//  			SizeF size = GRAPHICS.MeasureString(DotId.ToString(), DotTxtFont);
			float x = 0, y=0;
			if(DotPrice < avg) { //Draw.Text(this, "dotid"+DotId.ToString(), false, DotId.ToString(), RelBar+1, Math.Min(Low[RelBar],Math.Min(avg,DotPrice))-TickSize*pSeparation, -(int)TextHeight, pDTextColor, DotTxtFont, TextAlignment.Near, Brushes.Transparent, pDFillColor, 10);
				x = (float)chartControl.GetXByBarIndex(ChartBars,Bars, CurrentBar-RelBar)-txtLayout.Metrics.Width/2;
				y = (float)chartScale.GetYByValue( DotPrice-TickSize*pDotIdSeparation)+5;
			} else {
//				Draw.Text(this, "dotid"+DotId.ToString(), false, DotId.ToString(), RelBar+1, Math.Max(High[RelBar],Math.Max(avg,DotPrice))+TickSize*pSeparation, (int)TextHeight, pDTextColor, DotTxtFont, TextAlignment.Near, Brushes.Transparent, pDFillColor, 10);
				x = (float)chartControl.GetXByBarIndex(ChartBars,Bars, CurrentBar-RelBar)-txtLayout.Metrics.Width/2;
				y = (float)chartScale.GetYByValue( DotPrice+TickSize*pDotIdSeparation)-size.Height-5;
			}
			var rectF = new SharpDX.RectangleF(x,y,txtLayout.Metrics.Width, size.Height);
			RenderTarget.FillRectangle(rectF, DotIdBackgroundBrushDX);
//			GRAPHICS.FillRectangle(DotIdBackgroundBrush, x, y, txtLayout.Metrics.Width, size.Height);
			RenderTarget.DrawText(DotId.ToString(), txtFormat, rectF, DotIdTextBrushDX);
//			GRAPHICS.DrawTextLayout(new SharpDX.Vector2(,)DotId.ToString(), DotTxtFont, DotIdTextBrush, x, y);
		}
//==================================================================
		private static int CompareObjectsByBar(ChartObject a, ChartObject b){
			if(a==null) {
				if(b==null) return 0; //0 means the two points are equal, both null
				else        return -1; //a is null and b is not null, b is "greater"
			} else {
				if(b==null) return 1; //b is null, a isn't, therefore a is "greater"
				else 		return a.StartBar.CompareTo(b.StartBar);
			}
		}
//==================================================================
	private static string MakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		return stb.ToString();
	}
//==================================================================
		#region plots
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> CurrentSwingDistance
//		{get { return Values[0]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> AvgUp
//		{get { return Values[1]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> StdDevUp
//		{get { return Values[2]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> AvgDn
//		{get { return Values[3]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> StdDevDn
//		{get { return Values[4]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> Avg
//		{get { return Values[5]; }}
//
//		[Browsable(false)]
//		[XmlIgnore()]
//		public Series<double> OneStdDev
//		{get { return Values[6]; }}
		#endregion
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
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds",wav);
		}
//====================================================================

		#region Properties

        #region ZigZag

		private bool pEnableZigZag = false;
        [Description("Enable auto-calculation of zigzag nodes")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName(" Enable ZigZag")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Enable ZigZag",  GroupName = "ZZ Parameters")]
        public bool EnableZigZag
        {
            get { return pEnableZigZag; }
            set { pEnableZigZag = value; }
        }

        [Description("Deviation in percent or points regarding on the deviation type")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName("Deviation value")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Deviation value",  GroupName = "ZZ Parameters")]
        public double DeviationValue
        {
            get { return deviationValue; }
            set { deviationValue = Math.Max(0.0, value); }
        }

        [Description("Type of the deviation value")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName("Deviation type")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Deviation type",  GroupName = "ZZ Parameters")]
        public DeviationType DeviationType
        {
            get { return deviationType; }
            set { deviationType = value; }
        }

		private bool pEnablePopupOnDot = false;
        [Description("Enable popup window when a new zigzag node is found on live data")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName("Popup Enabled")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Popup Enabled",  GroupName = "ZZ Parameters")]
        public bool EnablePopupOnDot
        {
            get { return pEnablePopupOnDot; }
            set { pEnablePopupOnDot = value; }
        }

		private int pMaxAudibleAlertsPerBar = 1;
        [Description("Max number of times an audible alert will play on a zigzag node")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName("Sound MaxAlertsPerBar")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Sound MaxAlertsPerBar",  GroupName = "ZZ Parameters")]
        public int MaxAudibleAlertsPerBar
        {
            get { return pMaxAudibleAlertsPerBar; }
            set { pMaxAudibleAlertsPerBar = Math.Max(0,value); }
        }

		private bool pEnableSoundOnDot = false;
        [Description("Enable sound alert when a new zigzag node is found on live data")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName("Sound Enabled")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Sound Enabled",  GroupName = "ZZ Parameters")]
        public bool EnableSoundOnDot
        {
            get { return pEnableSoundOnDot; }
            set { pEnableSoundOnDot = value; }
        }

		private string pNewHighWAV = "";
        [Description("Sound WAV to play when a new high zigzag node is found on live data")]
//         [Category("ZZ Parameters")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayName("Sound New High")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Sound New High",  GroupName = "ZZ Parameters")]
        public string NewHighWAV
        {
            get { return pNewHighWAV; }
            set { pNewHighWAV = value.Trim(); }
        }

		private string pNewLowWAV = "";
        [Description("Sound WAV to play when a new low zigzag node is found on live data")]
//         [Category("ZZ Parameters")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
// [Gui.Design.DisplayName("Sound New Low")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Sound New Low",  GroupName = "ZZ Parameters")]
        public string NewLowWAV
        {
            get { return pNewLowWAV; }
            set { pNewLowWAV = value.Trim(); }
        }

        [Description("If true, high and low instead of selected price type is used to plot indicator.")]
//         [Category("ZZ Parameters")]
// [Gui.Design.DisplayName("Use high and low")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Use high and low",  GroupName = "ZZ Parameters")]
		[RefreshProperties(RefreshProperties.All)]
        public bool UseHighLow
        {
            get { return useHighLow; }
            set 
			{ 
				useHighLow			= value; 
				//PriceTypeSupported	= !value;
			}
        }

//		[XmlIgnore()]
//		[Description("Default color for nodes on automatically drawn zigzag")]
//		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("ZZNode Color")]
//		public Brush ZZNC{get { return pZZNodeColor; }	set { pZZNodeColor = value; }}
//		[Browsable(false)]
//		public string DClSerialize {get { return Serialize.BrushToString(pZZNodeColor); } set { pZZNodeColor = Serialize.StringToBrush(value); }}
// Brush pZZNodeColor = Brushes.Goldenrod;

		int pSeparation = 2;

        #endregion


		#region Dot Filter
		#region Session times
		private TimeSpan pBeginTime1 = new TimeSpan(0,0,0);
		[Description("Start time of session (use 24hr format)")]
// [Gui.Design.DisplayNameAttribute("Session Begin")]
//         [Category("Dot Filter")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Session Begin",  GroupName = "Dot Filter")]
		public string BeginTimeOfDay1 {
			get { return pBeginTime1.ToString(); }
			set { 
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pBeginTime1); 
				pBeginTime1 = new TimeSpan(Math.Min(23,pBeginTime1.Hours), Math.Min(59,pBeginTime1.Minutes), Math.Min(59,pBeginTime1.Seconds));
			}
		}
		private TimeSpan pEndTime1 = new TimeSpan(0,0,0);
		[Description("End time of session (use 24hr format)")]
// [Gui.Design.DisplayNameAttribute("Session End")]
//         [Category("Dot Filter")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Session End",  GroupName = "Dot Filter")]
		public string EndTimeOfDay1 {
			get { return pEndTime1.ToString(); }
			set {
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pEndTime1); 
				pEndTime1 = new TimeSpan(Math.Min(23,pEndTime1.Hours), Math.Min(59,pEndTime1.Minutes), Math.Min(59,pEndTime1.Seconds));
			}
		}
		#endregion
		private DateTime pDotsStartTime = NinjaTrader.Core.Globals.Now.AddDays(-20);
        [Description("Start datetime of the Dots and Squares")]
//         [Category("Dot Filter")]
// [Gui.Design.DisplayName("Start Time")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Start Time",  GroupName = "Dot Filter")]
        public DateTime DotsStart
        {
            get { return pDotsStartTime; }
            set { pDotsStartTime = value; }
        }

		private DateTime pDotsStopTime = NinjaTrader.Core.Globals.Now.AddDays(1);
        [Description("Stop datetime of the Dots and Squares")]
//         [Category("Dot Filter")]
// [Gui.Design.DisplayName("Stop Time")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Time",  GroupName = "Dot Filter")]
        public DateTime DotsStop
        {
            get { return pDotsStopTime; }
            set { pDotsStopTime = value; }
        }
		private bool pEnableBreakOnSquare = false;
		[Description("Do you want to have a Square represent a break in the zigzag sequence?")]
		[Category("Dot Filter")]
		public bool EnableBreakOnSquare
		{
			get { return pEnableBreakOnSquare; }
			set { pEnableBreakOnSquare = value; }
		}

		private Brush pAutoKeyDotColor = Brushes.Pink;
		[XmlIgnore()]
		[Description("Which auto-generated dots do you want to use?  All other colors on auto-generated dots will be ignored")]
// 		[Category("Dot Filter")]
// [Gui.Design.DisplayNameAttribute("Auto Dot Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Auto Dot Color",  GroupName = "Dot Filter")]
		public Brush AutoKeyDotColor{	get { return pAutoKeyDotColor; }	set { pAutoKeyDotColor = value; }		}
		[Browsable(false)]
		public string AutoKeyDotClSerialize
		{	get { return Serialize.BrushToString(pAutoKeyDotColor); } set { pAutoKeyDotColor = Serialize.StringToBrush(value); }
		}
		private bool pEnableAutoDotColorFilter = false;
		[Description("Enable selection of auto-generated dots based on their color?")]
		[Category("Dot Filter")]
		public bool EnableAutoDotColorFilter
		{
			get { return pEnableAutoDotColorFilter; }
			set { pEnableAutoDotColorFilter = value; }
		}

		private Brush pManualKeyDotColor = Brushes.Pink;
		[XmlIgnore()]
		[Description("Which manual-generated dots do you want to use?  All other colors on manual-generated dots will be ignored")]
// 		[Category("Dot Filter")]
// [Gui.Design.DisplayNameAttribute("Manual Dot Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Manual Dot Color",  GroupName = "Dot Filter")]
		public Brush ManualKeyDotColor{	get { return pManualKeyDotColor; }	set { pManualKeyDotColor = value; }		}
		[Browsable(false)]
		public string ManualKeyDotClSerialize
		{	get { return Serialize.BrushToString(pManualKeyDotColor); } set { pManualKeyDotColor = Serialize.StringToBrush(value); }
		}
		private bool pEnableManualDotColorFilter = false;
		[Description("Enable selection of manual-generated dots based on their color?")]
		[Category("Dot Filter")]
		public bool EnableManualDotColorFilter
		{
			get { return pEnableManualDotColorFilter; }
			set { pEnableManualDotColorFilter = value; }
		}
		#endregion

		private TextPosition pStatsLocation = TextPosition.TopLeft;
		[Description("Location of the statistics output")]
		[Category("Visual")]
		public TextPosition StatisticsLocation {	get { return pStatsLocation; }	set { pStatsLocation = value; }}

		private Brush pTextColor = Brushes.White;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Text Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color",  GroupName = "Visual")]
		public Brush TC{	get { return pTextColor; }	set { pTextColor = value; }		}
		[Browsable(false)]
		public string TClSerialize
		{	get { return Serialize.BrushToString(pTextColor); } set { pTextColor = Serialize.StringToBrush(value); }
		}
		private Brush pFillColor = Brushes.Red;
//		[XmlIgnore()]
//		[Description("")]
//		[Category("Visual")]
//		[Gui.Design.DisplayNameAttribute("Fill Color")]
//		public Brush FC{	get { return pFillColor; }	set { pFillColor = value; }		}
//		[Browsable(false)]
//		public string FClSerialize
//		{	get { return Serialize.BrushToString(pFillColor); } set { pFillColor = Serialize.StringToBrush(value); }
//		}

		private Brush pUpLineColor = Brushes.DarkGreen;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Line Color Up")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Line Color Up",  GroupName = "Visual")]
		public Brush ULC{	get { return pUpLineColor; }	set { pUpLineColor = value; }		}
		[Browsable(false)]
		public string ULClSerialize
		{	get { return Serialize.BrushToString(pUpLineColor); } set { pUpLineColor = Serialize.StringToBrush(value); }
		}

		private Brush pDnLineColor = Brushes.DarkRed;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Line Color Down")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Line Color Down",  GroupName = "Visual")]
		public Brush DLC{	get { return pDnLineColor; }	set { pDnLineColor = value; }		}
		[Browsable(false)]
		public string DLClSerialize
		{	get { return Serialize.BrushToString(pDnLineColor); } set { pDnLineColor = Serialize.StringToBrush(value); }
		}

		private SwingWaveCalculator_DotOrigination pDotSource = SwingWaveCalculator_DotOrigination.Both;
		[Description("Select which dots are to be considered by this indicator")]
		[Category("Parameters")]
		public SwingWaveCalculator_DotOrigination DotSource
		{
			get { return pDotSource; }
			set { pDotSource = value; }
		}

		private SwingWaveCalculator_NearestPrice pWhichPrice = SwingWaveCalculator_NearestPrice.NearestHighLow;
		[Description("Select which prices are to be the nodes of the lines")]
		[Category("Parameters")]
		public SwingWaveCalculator_NearestPrice WhichPrice
		{
			get { return pWhichPrice; }
			set { pWhichPrice = value; }
		}

		private bool pShowStatistics = true;
		[Description("Prints out (to the chart) statistical information on all distances")]
		[Category("Visual")]
		public bool ShowStatistics
		{
			get { return pShowStatistics; }
			set { pShowStatistics = value; }
		}

		private float pSeparationDistance = -1;
		[Description("Distance between the swing statistics data and the rightmost node of that swing, -1 to put the text in the middle of the swing line")]
		[Category("Visual")]
		public float SeparationDistance
		{
			get { return pSeparationDistance; }
			set { pSeparationDistance = Math.Max(-1,value); }
		}

		private int pLineWidth = 2;
		[Description("Width of lines in pixels, enter a '0' to turn-off lines")]
		[Category("Visual")]
		public int LineWidthPixels
		{
			get { return pLineWidth; }
			set { pLineWidth = Math.Max(0,value); }
		}

		private float pTxtSize = 10;
		[Description("Size of text in font points")]
		[Category("Visual")]
		public float TextSize
		{
			get { return pTxtSize; }
			set { pTxtSize = Math.Max(1,value); }
		}

		private bool pShowDotId = false;
		[Description("Show Dot number near each Dot?")]
		[Category("Dot Id")]
		public bool _ShowDotId
		{
			get { return pShowDotId; }
			set { pShowDotId = value; }
		}

		private float pDotTxtSize = 14;
		[Description("Size of Dot number, in font points")]
		[Category("Dot Id")]
		public float DotTextSize
		{
			get { return pDotTxtSize; }
			set { pDotTxtSize = Math.Max(1,value); }
		}

		private Brush pDTextColor = Brushes.Orange;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Dot Id")]
// [Gui.Design.DisplayNameAttribute("Text Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color",  GroupName = "Dot Id")]
		public Brush DTC{	get { return pDTextColor; }	set { pDTextColor = value; }		}
		[Browsable(false)]
		public string DTClSerialize
		{	get { return Serialize.BrushToString(pDTextColor); } set { pDTextColor = Serialize.StringToBrush(value); }
		}
		private Brush pDFillColor = Brushes.Blue;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Dot Id")]
// [Gui.Design.DisplayNameAttribute("Fill Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Fill Color",  GroupName = "Dot Id")]
		public Brush DFC{	get { return pDFillColor; }	set { pDFillColor = value; }		}
		[Browsable(false)]
		public string DFClSerialize
		{	get { return Serialize.BrushToString(pDFillColor); } set { pDFillColor = Serialize.StringToBrush(value); }
		}

		private int pDotIdSeparation = 1;
		[Description("Separation, in ticks, between price bar and Dot Id number")]
		[Category("Dot Id")]
		public int DotIdSeparation
		{
			get { return pDotIdSeparation; }
			set { pDotIdSeparation = Math.Max(0,value); }
		}

		private SwingWaveCalculator_OutputType pOutputType = SwingWaveCalculator_OutputType.TicksAndBars;
		[Description("Type of output")]
		[Category("Parameters")]
		public SwingWaveCalculator_OutputType OutputType
		{
			get { return pOutputType; }
			set { pOutputType = value; }
		}
        #endregion
	}
}
public enum SwingWaveCalculator_DotOrigination {
	Manually, Programmatically, Both, Neither
}
public enum SwingWaveCalculator_NearestPrice {
	DotPrice, NearestHighLow, NearestClose
}

public enum SwingWaveCalculator_OutputType {
	Ticks, Points, Bars, DollarValue, TicksAndBars, PointsAndBars, DollarsAndBars, Volume, TicksAndVolume, PointsAndVolume, DollarsAndVolume, VolumeAndBars, None
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SwingWaveCalculator[] cacheSwingWaveCalculator;
		public SwingWaveCalculator SwingWaveCalculator()
		{
			return SwingWaveCalculator(Input);
		}

		public SwingWaveCalculator SwingWaveCalculator(ISeries<double> input)
		{
			if (cacheSwingWaveCalculator != null)
				for (int idx = 0; idx < cacheSwingWaveCalculator.Length; idx++)
					if (cacheSwingWaveCalculator[idx] != null &&  cacheSwingWaveCalculator[idx].EqualsInput(input))
						return cacheSwingWaveCalculator[idx];
			return CacheIndicator<SwingWaveCalculator>(new SwingWaveCalculator(), input, ref cacheSwingWaveCalculator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SwingWaveCalculator SwingWaveCalculator()
		{
			return indicator.SwingWaveCalculator(Input);
		}

		public Indicators.SwingWaveCalculator SwingWaveCalculator(ISeries<double> input )
		{
			return indicator.SwingWaveCalculator(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SwingWaveCalculator SwingWaveCalculator()
		{
			return indicator.SwingWaveCalculator(Input);
		}

		public Indicators.SwingWaveCalculator SwingWaveCalculator(ISeries<double> input )
		{
			return indicator.SwingWaveCalculator(input);
		}
	}
}

#endregion
