//#define CHECKAUTHORIZATION
// 
// Copyright (C) 2011, SBG Trading Corp.    www.affordableindicators.com
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//


#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
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

namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Plots the high and low of a specific, prior time range
	/// </summary>
	[Description("Plots the high and low of a specific, past time range for the current day")]
	public class HiLoOfTimeRange_withPivotLevels : Indicator
	{
		private bool LicenseValid = true;
		private const int LONG = 1;
		private const int FLAT = 0;
		private const int SHORT = -1;
		#region Variables
		// Wizard generated variables
		private int    pSeparation = 5;
		private bool   pShowPivotLevels = false;
		private bool   pShowBreakoutLabel = true;
		private bool   pExcludeWeekends = true;
		private bool   pShowHistoricalBreakouts = false;
		private string pDownLabelPrefix = "Short @ ";
		private string pUpLabelPrefix = "Long @ ";
		private string pWAValert = "Alert2.wav";
		private bool   OkToPlaySound = false;
		private int    LastSignal = FLAT;
		private int    position = FLAT;
		private bool   SearchForEntry = false;
		// User defined variables (add any user defined variables below)
		private int SessionId=0, LastSessionId = -1, StartBar=0, EndBar=0;
		private DateTime StartTime=DateTime.MinValue, EndTime=DateTime.MinValue;
		private DateTime StopAlertsTime=DateTime.MinValue;
		private double h=0.0;
		private double l=0.0;
		private double ThePP=0, TheR1=0, TheS1=0, TheR2=0, TheS2=0, TheR3=0, TheS3=0;
		private int SessionsCount = 0, SessionOfLastBreakout = -1;
		private bool RunInit = true;
		#endregion
		private Series<int> BarAtLabels = null;
		private SortedDictionary<int,double> BarAtHighBreakout = new SortedDictionary<int,double>();
		private SortedDictionary<int,double> BarAtLowBreakout = new SortedDictionary<int,double>();
		private int width2=int.MinValue;
		private int width3, width4, width5, width6, width7, width8, width0,width1, height, halfheight;
		private Brush TxtBackBrush;
		private DateTime t1 = DateTime.MinValue;
		private DateTime t0 = DateTime.MinValue;
		private double targetprice = double.MinValue;
		private double entryprice = double.MinValue;
		private double stoplossprice = double.MinValue;
		private double StoplossDistance = 0;
		private int AlertBar = -1;
		private double EntryDistance = 0;
		private int LosingTradesToday = 0;
		private int WinningTradesToday = 0;
		private int TradesToday = 0;
		private int MinLabelWidth = 0;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				this.MaximumBarsLookBack = MaximumBarsLookBack.Infinite;
				Name = "iw HiLoOfTimeRange (with pivot levels)";
				bool IsDebug = NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 && System.IO.File.Exists("c:\\222222222222.txt");
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", "AIHiLoOfTimeRange", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(Brushes.MediumSlateBlue,2), PlotStyle.Hash, "TheHigh");
				AddPlot(new Stroke(Brushes.HotPink, 2),  PlotStyle.Hash, "TheLow");
				AddPlot(new Stroke(Brushes.Maroon,1),    PlotStyle.Hash, "R3");
				AddPlot(new Stroke(Brushes.Red,1),       PlotStyle.Hash, "R2");
				AddPlot(new Stroke(Brushes.Magenta,1),   PlotStyle.Hash, "R1");
				AddPlot(new Stroke(Brushes.Blue,1),      PlotStyle.Hash, "PP");
				AddPlot(new Stroke(Brushes.Lime,1),      PlotStyle.Hash, "S1");
				AddPlot(new Stroke(Brushes.Green,1),     PlotStyle.Hash, "S2");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, "S3");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, "TargetPrice");
				Calculate = Calculate.OnPriceChange;
				IsAutoScale=false;
				IsOverlay=true;
				//PriceTypeSupported	= false;
			}
			if (State == State.Configure)
			{
				Plots[2].DashStyleHelper = DashStyleHelper.Dash;
				Plots[3].DashStyleHelper = DashStyleHelper.Dash;
				Plots[4].DashStyleHelper = DashStyleHelper.Dash;
//				Plots[5].DashStyleHelper = DashStyleHelper.Dash;
				Plots[6].DashStyleHelper = DashStyleHelper.Dash;
				Plots[7].DashStyleHelper = DashStyleHelper.Dash;
				Plots[8].DashStyleHelper = DashStyleHelper.Dash;
			}
			if(State==State.DataLoaded){
				EntryDistance = pEntryTicksBeyondRange * TickSize;
				StoplossDistance = this.pExitTicks * TickSize;
				BarAtLabels       = new Series<int>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				TxtBackBrush  = pTextBackgroundBrush.Clone(); 
				TxtBackBrush.Opacity = this.pTextBkgndOpacity;
				TxtBackBrush.Freeze();
			}
		}
		public override string ToString(){
			if(pShowPivotLevels) return "HiLoOfTimeRange (with pivot levels) "+TimeBegin+" - "+TimeEnd;
			else return "HiLoOfTimeRange "+TimeBegin+" - "+TimeEnd;
		}
		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;
//if(CurrentBar<100) Print(Time[0].ToString()+"  TheHigh: "+TheHigh[0].ToString()+"   "+TheHigh.IsValidDataPoint(0));
			if(CurrentBar<10) return;
			if(RunInit) {
//IsAutoScale=false;
				#region RunInit
				RunInit = false;
				StartTime      = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, pStartTime.Hours, pStartTime.Minutes, pStartTime.Seconds);
				EndTime        = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, pEndTime.Hours,   pEndTime.Minutes,   pEndTime.Seconds);
				if(EndTime<StartTime) EndTime = EndTime.AddDays(1);
				StopAlertsTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, pStopTime.Hours,  pStopTime.Minutes,  pStopTime.Seconds);
				if(StopAlertsTime<EndTime) StopAlertsTime = StopAlertsTime.AddDays(1);
				if(StopAlertsTime > StartTime.AddDays(1)) StopAlertsTime = StartTime.AddMinutes(-1);
				#endregion
			}
//			StartTime      = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, startTimeHr, startTimeMinute,0);
//			EndTime        = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, endTimeHr, endTimeMinute,0);
//			StopAlertsTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, pStopAlertsHr, pStopAlertsMinute,0);

			t1 = t0==DateTime.MinValue? Time[0] : t0;
			t0 = (State == State.Historical || Time[0].Ticks<NinjaTrader.Core.Globals.Now.Ticks-TimeSpan.TicksPerHour)?Time[0]:NinjaTrader.Core.Globals.Now;
//Draw.TextFixed(this, "Times",t1.ToString()+" / "+t0.ToString(), TextPosition.BottomRight);
			bool InSession = Time[0].Ticks > EndTime.Ticks && Time[0].Ticks < StopAlertsTime.Ticks;
			OkToPlaySound = InSession && AlertBar!=CurrentBar;
			while(t0>StopAlertsTime) {
				StartTime = StartTime.AddDays(1);
				EndTime = EndTime.AddDays(1);
				StopAlertsTime = StopAlertsTime.AddDays(1);
//				if(LastSignal==LONG) Draw.Diamond(this, "DD"+CurrentBar,false,0,Close[0],Color.Maroon);
//				if(LastSignal==FLAT) Draw.Diamond(this, "DD"+CurrentBar,false,0,Close[0],Color.DarkGreen);
//				if(LastSignal==SHORT) Draw.Diamond(this, "DD"+CurrentBar,false,0,Close[0],Color.Cyan);
				CheckForExit("EOD");
			}
			if(pExcludeWeekends) {
				if(Time[0].DayOfWeek == DayOfWeek.Saturday ||Time[0].DayOfWeek == DayOfWeek.Sunday) return;
			}
//Print(Time[0].ToString()+"   Start: "+StartTime.ToString()+"   End time: "+EndTime.ToString());
			if(OkToPlaySound) BarAtLabels[0] = (BarAtLabels[1]); else BarAtLabels[0] = (-1);
			if(t0.Ticks > EndTime.Ticks && t1.Ticks <= EndTime.Ticks) {
				StartBar = Bars.GetBar(StartTime);
				EndBar = CurrentBar - 1;
				SessionId = ToDay(Time[0]);
//				if(LastSessionId==-1) LastSessionId = SessionId;
				if(CurrentBar == StartBar){
					h = High[0]+EntryDistance;
					l = Low[0]-EntryDistance;
				}else if(StartBar != EndBar) {
					h = MAX(High,CurrentBar-StartBar)[1]+EntryDistance;
					l = MIN(Low,CurrentBar-StartBar)[1]-EntryDistance;
				}
				if(pShowRangeBox) {
					Draw.Line(this, "diag1"+SessionId, false, CurrentBar-StartBar, h, CurrentBar-EndBar,   l, Brushes.Blue,  DashStyleHelper.Dot, 1);
					Draw.Line(this, "diag2"+SessionId, false, CurrentBar-StartBar, l, CurrentBar-EndBar,   h, Brushes.Blue,  DashStyleHelper.Dot, 1);
					Draw.Line(this, "vert1"+SessionId, false, CurrentBar-EndBar,   l, CurrentBar-EndBar,   h, Brushes.Green, DashStyleHelper.Dot, 1);
					Draw.Line(this, "vert2"+SessionId, false, CurrentBar-StartBar, l, CurrentBar-StartBar, h, Brushes.Red,   DashStyleHelper.Dot, 1);
				}
				if(pShowPivotLevels) {
					ThePP = (h+l+Close[0])/3.0;
					TheS1 = 2*ThePP-h;
					TheR1 = 2*ThePP-l;
					TheS2 = ThePP-(TheR1-TheS1);
					TheR2 = ThePP+(TheR1-TheS1);
					TheS3 = TheS1-(h-l);
					TheR3 = TheR1+(h-l);
					string tag = string.Empty;
					if(pShowHistoricalBreakouts) tag = string.Concat(tag, SessionsCount.ToString());
					BarAtLabels[0] = (CurrentBar);
				}
//				SessionsCount++;
				LastSignal = FLAT;
				position = FLAT;
				targetprice = double.MinValue;
			}

			if(InSession){
				TheHigh[0] = (h);
				TheLow[0] = (l);
			}
			if(OkToPlaySound) {
				if(pShowPivotLevels && ThePP>0 && InSession) {
					PP[0] = (ThePP);
					R1[0] = (TheR1);
					R2[0] = (TheR2);
					R3[0] = (TheR3);
					S1[0] = (TheS1);
					S2[0] = (TheS2);
					S3[0] = (TheS3);
				}
			}
//			if(SessionsCount == SessionOfLastBreakout) {
//				if(High[0] < TheHigh[0] && High[0] > TheLow[0])  SessionsCount++;
//				if(Low[0]  > TheLow[0]  && Low[0]  < TheHigh[0]) SessionsCount++;
//			}
//			if(BarAtHighBreakout.IsValidDataPoint(1)) BarAtHighBreakout[0] = (BarAtHighBreakout[1]);
//			if(BarAtLowBreakout.IsValidDataPoint(1)) BarAtLowBreakout[0] = (BarAtLowBreakout[1]);
			if(SessionId != LastSessionId) //if the end of the timeframe has already arrived
			{
				SearchForEntry = true;
				LastSessionId = SessionId;
				LosingTradesToday = 0;
				TradesToday = 0;
				WinningTradesToday = 0;
			}
			if(InSession) 
			{
				CheckForExit("TP SL");
				bool EntryValid = false;
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.ClosesBeyond) EntryValid = Close[1] > TheHigh[0]+EntryDistance;
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.Touches)      EntryValid = High[0] >= TheHigh[0]+EntryDistance;
				EntryValid = EntryValid && TradesToday<2 && LosingTradesToday<=1 && WinningTradesToday==0;
				if(EntryValid) {
					if(LastSignal != LONG) {
						LastSignal = LONG;
						if(pShowArrows && TheHigh.IsValidDataPoint(0) && this.pEntryTicksBeyondRange>-999) {
							EnterTrade(LosingTradesToday, ref TradesToday, LONG, ref entryprice, ref targetprice, ref stoplossprice);
						}
						if(OkToPlaySound) {PlaySoundCustom(pWAValert);AlertBar=CurrentBar;}
						position = LONG;
					}
					SearchForEntry = false;
				}
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.ClosesBeyond) EntryValid = Close[1] < TheLow[0]-EntryDistance;
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.Touches)      EntryValid = Low[0] <= TheLow[0]-EntryDistance;
				EntryValid = EntryValid && TradesToday<2 && LosingTradesToday<=1 && WinningTradesToday==0;
				if(EntryValid) {
					if(LastSignal != SHORT) {
						LastSignal = SHORT;
						if(pShowArrows && TheHigh.IsValidDataPoint(0) && this.pEntryTicksBeyondRange>-999) {
							EnterTrade(LosingTradesToday, ref TradesToday, SHORT, ref entryprice, ref targetprice, ref stoplossprice);
						}
						if(OkToPlaySound) {PlaySoundCustom(pWAValert);AlertBar=CurrentBar;}
						position = SHORT;
					}
					SearchForEntry = false;
				}
				if(targetprice!=double.MinValue) TargetPrice[0] = (targetprice);
				CheckForExit("TP SL");
			} else {
				TargetPrice.Reset();
			}
		}
		
//====================================================================
		private void PlaySoundCustom(string wav){
			string path = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds");
			string fname = System.IO.Path.Combine(path,wav);
			if(System.IO.File.Exists(fname)) PlaySound(fname);
			else
				Log("'"+wav+"' does not exist in "+path+"...no sound was played",LogLevel.Information);
		}
//====================================================================
//=========================================================================================================
		private void EnterTrade(int LosingTradesToday, ref int TradesToday, int Direction, ref double EntryPrice, ref double TargetPrice, ref double StoplossPrice) {
			if(LosingTradesToday>=2) return;
			if(TradesToday>=2) return;
			string ExitTag = "PositionExit"+CurrentBar.ToString();
			if(Direction==SHORT) {
				string tag = "TradeArrowDn"+CurrentBar.ToString();
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.ClosesBeyond) {
					RemoveDrawObject(ExitTag);
					TradesToday++;
					EntryPrice = Close[1];
					var arr = Draw.ArrowDown(this, tag, IsAutoScale,1,EntryPrice,Brushes.Red);
					arr.IsLocked = false;
					if(LosingTradesToday!=0) arr = Draw.ArrowDown(this, "2nd"+tag, IsAutoScale,1,EntryPrice,Brushes.Red);
					arr.IsLocked = false;
				}
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.Touches) {
					RemoveDrawObject(ExitTag);
					TradesToday++;
					EntryPrice = TheLow[0]-EntryDistance;
					var arr=Draw.ArrowDown(this, tag,IsAutoScale,0,EntryPrice,Brushes.Red);
					arr.IsLocked = false;
					if(LosingTradesToday!=0) arr = Draw.ArrowDown(this, "2nd"+tag,IsAutoScale,0,EntryPrice,Brushes.Red);
					arr.IsLocked = false;
				}
				StoplossPrice = pExitTicks<0? double.MinValue : EntryPrice + this.pExitTicks * TickSize;
				TargetPrice = pTargetTicks<0? double.MinValue : EntryPrice - this.pTargetTicks * TickSize;
				if(pShowBreakoutLabel) BarAtLowBreakout[CurrentBar] = EntryPrice;
			} else {
				string tag = "TradeArrowUp"+CurrentBar.ToString();
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.ClosesBeyond) {
					RemoveDrawObject(ExitTag);
					TradesToday++;
					EntryPrice = Close[1];
					var arr=Draw.ArrowUp(this, tag,IsAutoScale,1,EntryPrice,Brushes.Green);
					arr.IsLocked = false;
					if(LosingTradesToday!=0) arr = Draw.ArrowUp(this, "2nd"+tag,IsAutoScale,1,EntryPrice,Brushes.Green);
					arr.IsLocked = false;
				}
				if(pSignalType == HiLoOfTimeRange_withPivotLevels_SignalType.Touches) {
					RemoveDrawObject(ExitTag);
					TradesToday++;
					EntryPrice = TheHigh[0]+EntryDistance;
					var arr = Draw.ArrowUp(this, tag,IsAutoScale,0,EntryPrice,Brushes.Green);
					arr.IsLocked = false;
					if(LosingTradesToday!=0) arr = Draw.ArrowUp(this, "2nd"+tag,IsAutoScale,0,EntryPrice,Brushes.Green);
					arr.IsLocked = false;
				}
				TargetPrice = pTargetTicks<0? double.MinValue : EntryPrice + this.pTargetTicks * TickSize;
				StoplossPrice = pExitTicks<0? double.MinValue : EntryPrice - this.pExitTicks * TickSize;
				if(pShowBreakoutLabel) BarAtHighBreakout[CurrentBar] = EntryPrice;
			}
		}

//====================================================================================================================
		private void CheckForExit(string TypeOfExit) {

			if(pShowArrows) 
			{
				if(TypeOfExit.Contains("TP SL")) 
				{
					string ExitTag = "PositionExit"+CurrentBar.ToString();
					if(TargetPrice.IsValidDataPoint(0)) 
					{
						#region Handle TP and SL hits
						if(position == LONG) {
							if(High[0] > targetprice && targetprice != double.MinValue) {
BackBrush = Brushes.LightGreen; //Print(Time[0].ToString()+"  Printing square at: "+targetprice);
								var sq=Draw.Square(this, ExitTag,IsAutoScale,0,targetprice,Brushes.Yellow);
								sq.IsLocked = false;
								targetprice   = double.MinValue;
								stoplossprice = double.MinValue;
								position = FLAT;
								WinningTradesToday++;
							}
						} else if(position == SHORT) {
//	Print("position: "+position.ToString()+"   "+targetprice.ToString());
							if(Low[0] < targetprice && targetprice != double.MinValue) {
BackBrush = Brushes.LightGreen; //Print(Time[0].ToString()+"  Printing square at: "+targetprice);
								var sq=Draw.Square(this, ExitTag,IsAutoScale,0,targetprice,Brushes.Yellow);
								sq.IsLocked = false;
								targetprice   = double.MinValue;
								stoplossprice = double.MinValue;
								position = FLAT;
								WinningTradesToday++;
							}
						}
					}
					if(stoplossprice != double.MinValue) {
						if(position == LONG) {
							if(Low[0] < stoplossprice) {
	BackBrush=Brushes.Pink; //Print(Time[0].ToString()+"  Printing square at: "+targetprice);
								var sq=Draw.Square(this, ExitTag,IsAutoScale,0,stoplossprice,Brushes.Yellow);
								sq.IsLocked = false;
								targetprice   = double.MinValue;
								stoplossprice = double.MinValue;
								position = FLAT;
								LosingTradesToday++;
							}
						}
						else if(position == SHORT) {
							if(High[0] > stoplossprice) {
	BackBrush=Brushes.Pink; //Print(Time[0].ToString()+"  Printing square at: "+targetprice);
								var sq=Draw.Square(this, ExitTag,IsAutoScale,0,stoplossprice,Brushes.Yellow);
								sq.IsLocked = false;
								targetprice   = double.MinValue;
								stoplossprice = double.MinValue;
								position = FLAT;
								LosingTradesToday++;
							}
						}
						#endregion
//						if(position == FLAT) TargetPrice.Reset();
//						else if(targetprice!=double.MinValue) TargetPrice[0] = (targetprice);
					}
				} else {
					#region Handle EndOfDay exit
					if(position == LONG) {
						if(High[0] > targetprice && targetprice != double.MinValue) {
	BackBrush=Brushes.LightGreen;
							var sq=Draw.Square(this, "PositionExit"+CurrentBar,IsAutoScale,0,targetprice,Brushes.Yellow);
							sq.IsLocked = false;
						} else {
	BackBrush=Brushes.Pink;
							var sq=Draw.Square(this, "PositionExit"+CurrentBar,IsAutoScale,0,Close[0],Brushes.Yellow);
							sq.IsLocked = false;
						}
						targetprice   = double.MinValue;
						stoplossprice = double.MinValue;
						position = FLAT;
					}
					if(position == SHORT) {
						if(Low[0] < targetprice && targetprice != double.MinValue) {
	BackBrush=Brushes.LightGreen; //Print(Time[0].ToString()+"  Printing square at: "+targetprice);
							var sq=Draw.Square(this, "PositionExit"+CurrentBar,IsAutoScale,0,targetprice,Brushes.Yellow);
							sq.IsLocked = false;
						} else {
	BackBrush=Brushes.Pink; //Print(Time[0].ToString()+"  Printing square at: "+targetprice);
							var sq=Draw.Square(this, "PositionExit"+CurrentBar,IsAutoScale,0,Close[0],Brushes.Yellow);
							sq.IsLocked = false;
						}
						targetprice   = double.MinValue;
						stoplossprice = double.MinValue;
						position = FLAT;
					}
					#endregion
//						if(position == FLAT) TargetPrice.Reset();
//						else if(targetprice!=double.MinValue) TargetPrice[0] = (targetprice);
				}
			}
		}
//====================================================================================================================
		SharpDX.Direct2D1.Brush TxtBackBrushDX=null, LongTextBrushDX=null, ShortTextBrushDX=null;
        public override void OnRenderTargetChanged()
        {
			try{
				if(TxtBackBrushDX!=null)  {TxtBackBrushDX.Dispose();   TxtBackBrushDX=null;}
				if(LongTextBrushDX!=null)  {LongTextBrushDX.Dispose();   LongTextBrushDX=null;}
				if(ShortTextBrushDX!=null)  {ShortTextBrushDX.Dispose();   ShortTextBrushDX=null;}
				if(RenderTarget!=null) TxtBackBrushDX = TxtBackBrush.ToDxBrush(RenderTarget);
				if(RenderTarget!=null) LongTextBrushDX = this.pLongTextBrush.ToDxBrush(RenderTarget);
				if(RenderTarget!=null) ShortTextBrushDX = this.pShortTextBrush.ToDxBrush(RenderTarget);
			}catch(Exception ee){Print("iw_HighLowOfTimeRange: "+ee.ToString());}
		}
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			if (!IsVisible) return;
			base.OnRender(chartControl, chartScale);
			int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = ChartBars.ToIndex;
			SharpDX.DirectWrite.TextFormat txtFormat = TextFont.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout txtLayout = null;
int line=460;
			try{
				if(ChartControl==null) return;
				if(BarAtLabels==null) return;
				int RMB = Math.Max(0,Math.Min(CurrentBar-1, lastBarPainted));
				//if(BarAtLabels[CurrentBar-RMB]==-1) return;
line=467;
				#region Initialize the Widths of the Plot Names
				if(width2==int.MinValue) {
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "SessionHigh", txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width0 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "SessionLow", txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width1 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[2].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width2 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[3].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width3 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[4].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width4 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[5].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width5 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[6].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width6 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[7].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width7 = (int)txtLayout.Metrics.Width;
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[8].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
					width8 = (int)txtLayout.Metrics.Width;
					MinLabelWidth = Math.Max(width2,Math.Max(width3,Math.Max(width4,Math.Max(width5,Math.Max(width6,Math.Max(width7,Math.Max(width8,Math.Max(width0,width1))))))));
					height = (int)(txtLayout.Metrics.Height);
					halfheight = (int)(txtLayout.Metrics.Height/2.0);
				}
				#endregion
line=493;

				int x = chartControl.GetXByBarIndex(ChartBars,BarAtLabels.GetValueAt(RMB)-1);
//				if(x<MinLabelWidth) x=MinLabelWidth+5;
				if(x<ChartPanel.X) x=ChartPanel.X;
//	Draw.TextFixed(this, "x",Time[CurrentBar-BarAtLabels[CurrentBar-RMB]].ToString(), TextPosition.Center);
				if(pShowPivotLevelLabels){// && PP.IsValidDataPoint(RMB)) {
line=500;
					int y0 = chartScale.GetYByValue( TheHigh.GetValueAt(RMB))-halfheight;
					int y1 = chartScale.GetYByValue( TheLow.GetValueAt(RMB))-halfheight;
					int y2 = chartScale.GetYByValue( R3.GetValueAt(RMB))-halfheight;
					int y3 = chartScale.GetYByValue( R2.GetValueAt(RMB))-halfheight;
					int y4 = chartScale.GetYByValue( R1.GetValueAt(RMB))-halfheight;
					int y5 = chartScale.GetYByValue( PP.GetValueAt(RMB))-halfheight;
					int y6 = chartScale.GetYByValue( S1.GetValueAt(RMB))-halfheight;
					int y7 = chartScale.GetYByValue( S2.GetValueAt(RMB))-halfheight;
					int y8 = chartScale.GetYByValue( S3.GetValueAt(RMB))-halfheight;
//Print("RMB: "+RMB+"  CB: "+CurrentBar+"   x: "+x+"  y2: "+y2+"  y5: "+y5);
					if(y2 != y5){
						int x2 = Math.Max(ChartPanel.X,x-width2-1);
						int x3 = Math.Max(ChartPanel.X,x-width3-1);
						int x4 = Math.Max(ChartPanel.X,x-width4-1);
						int x5 = Math.Max(ChartPanel.X,x-width5-1);
						int x6 = Math.Max(ChartPanel.X,x-width6-1);
						int x7 = Math.Max(ChartPanel.X,x-width7-1);
						int x8 = Math.Max(ChartPanel.X,x-width8-1);
//						x2 = x;
//						x3 = x;
//						x4 = x;
//						x5 = x;
//						x6 = x;
//						x7 = x;
//						x8 = x;
line=525;
						if(pTextBkgndOpacity>0 && TxtBackBrushDX!=null && !TxtBackBrushDX.IsDisposed) {
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x2, y2, width2, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x3, y3, width3, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x4, y4, width4, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x5, y5, width5, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x6, y6, width6, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x7, y7, width7, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x8, y8, width8, height), TxtBackBrushDX);
						}
line=535;
//Print("Plots[2].Name: "+Plots[2].Name+"  x2: "+x2+"  y2: "+y2);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[2].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x2,y2), txtLayout, Plots[2].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[3].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x3,y3), txtLayout, Plots[3].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[4].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x4,y4), txtLayout, Plots[4].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[5].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x5,y5), txtLayout, Plots[5].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[6].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x6,y6), txtLayout, Plots[6].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[7].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x7,y7), txtLayout, Plots[7].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[8].Name, txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x8,y8), txtLayout, Plots[8].BrushDX);
line=550;
					}
					if(y0 != y1){
						int x0 = Math.Max(ChartPanel.X,x-width0-1);
						int x1 = Math.Max(ChartPanel.X,x-width1-1);
//						x0 = x;
//						x1 = x;
						if(pTextBkgndOpacity>0 && TxtBackBrushDX!=null && !TxtBackBrushDX.IsDisposed) {
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, y0, width0, height), TxtBackBrushDX);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x1, y1, width1, height), TxtBackBrushDX);
						}
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "SessionHigh", txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x0,y0), txtLayout, Plots[0].BrushDX);
						txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "SessionLow", txtFormat, ChartPanel.W, (float)pTextFont.Size);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x1,y1), txtLayout, Plots[1].BrushDX);
					}
				}
line=567;

				if(pShowBreakoutLabel) {
					double hp=0, lp=0;
					foreach(KeyValuePair<int,double> kvp in BarAtHighBreakout) {
						int absbar = kvp.Key;
						if(absbar>firstBarPainted && absbar<RMB) {
							int relbar = CurrentBar-absbar;
							hp = kvp.Value;
							string txt = pUpLabelPrefix+hp.ToString();
							//SizeF size = RenderTarget.MeasureString(txt,pTextFont);
							txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txt, txtFormat, ChartPanel.W, (float)pTextFont.Size);
							int xb = chartControl.GetXByBarIndex(ChartBars,absbar)-(int)txtLayout.Metrics.Width-(int)ChartControl.Properties.BarDistance*2;
							int yb = chartScale.GetYByValue(hp)-(int)txtLayout.Metrics.Height-2;
//Print("breakout High absbar: "+absbar+"  "+Time[relbar].ToString()+"   "+hp+"   "+txt+"   x/y: "+xb+"/"+yb);
							if(TxtBackBrushDX!=null && !TxtBackBrushDX.IsDisposed) RenderTarget.FillRectangle(new SharpDX.RectangleF(xb, yb, txtLayout.Metrics.Width, txtLayout.Metrics.Height), TxtBackBrushDX);
							if(LongTextBrushDX!=null && !LongTextBrushDX.IsDisposed) RenderTarget.DrawTextLayout(new SharpDX.Vector2(xb,yb), txtLayout, LongTextBrushDX);
//							RenderTarget.DrawString(txt, pTextFont, new SolidColorBrush(pLongTextBrush), xb, yb);
						}
					}
line=587;

					foreach(KeyValuePair<int,double> kvp in BarAtLowBreakout) {
						int absbar = kvp.Key;
						if(absbar>firstBarPainted && absbar<RMB) {
							int relbar = CurrentBar-absbar;
							lp = kvp.Value;
							string txt = pDownLabelPrefix+lp.ToString();
//							SizeF size = RenderTarget.MeasureString(txt,pTextFont);
							txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txt, txtFormat, ChartPanel.W, (float)pTextFont.Size);
							int xb = chartControl.GetXByBarIndex(ChartBars,absbar)-(int)txtLayout.Metrics.Width-(int)ChartControl.Properties.BarDistance*2;
							int yb = chartScale.GetYByValue(lp);
							if(TxtBackBrushDX!=null && !TxtBackBrushDX.IsDisposed) RenderTarget.FillRectangle(new SharpDX.RectangleF(xb, yb, txtLayout.Metrics.Width, txtLayout.Metrics.Height), TxtBackBrushDX);
							if(ShortTextBrushDX!=null && !ShortTextBrushDX.IsDisposed) RenderTarget.DrawTextLayout(new SharpDX.Vector2(xb,yb), txtLayout, ShortTextBrushDX);
						}
					}
				}
line=604;

			} catch(Exception err) {Print("Line "+line+Environment.NewLine+err.ToString());}
		}
//===========================================================================================================
//	private System.Windows.Media.Brush ContrastBrush(System.Windows.Media.Brush cl){
//		System.Windows.Media.SolidColorBrush solidBrush = cl as System.Windows.Media.SolidColorBrush;

//		byte bRed   = solidBrush.Color.R;
//		byte bGreen = solidBrush.Color.G;
//		byte bBlue  = solidBrush.Color.B;
//		double c = (bRed*0.2126)+(bGreen*0.7152)+(bBlue*0.0722);
//		if(c>128) return Brushes.Black;
//		else return Brushes.White;
//	}
//===========================================================================================================

        #region Plots
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> TheHigh
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> TheLow
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> R3
        {
            get { return Values[2]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> R2
        {
            get { return Values[3]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> R1
        {
            get { return Values[4]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PP
        {
            get { return Values[5]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> S1
        {
            get { return Values[6]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> S2
        {
            get { return Values[7]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> S3
        {
            get { return Values[8]; }
        }
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> TargetPrice
        {
            get { return Values[9]; }
        }
		#endregion
		
		#region Properties

		[Display(GroupName="Visual", Description="Number of ticks the line of text is away from the high or low entry price")]
        public int Separation
        {
            get { return pSeparation; }
            set { pSeparation = Math.Max(0, value); }
        }

		private bool pShowRangeBox = true;
		[Display(GroupName="Visual", Description="Show (or hide) the box that identifies the range being calculated")]
        public bool ShowRangeBox
        {
            get { return pShowRangeBox; }
            set { pShowRangeBox = value; }
        }

		[Display(GroupName="Alert", Description="Leave blank to turn-off audio warning")]
        public string AlertFileName
        {
            get { return pWAValert; }
            set { pWAValert = value; }
        }
		private bool pShowArrows = true;
		[Display(GroupName="Strategy", Description="Show (or hide) buy and sell arrows at breakout")]
        public bool ShowArrows
        {
            get { return pShowArrows; }
            set { pShowArrows = value; }
        }
		private int pEntryTicksBeyondRange = 1;
		[Display(GroupName="Strategy", Description="Number of ticks for entry price, beyond high and low of range, enter '-999' to turn-off the entrys")]
        public int EntryTicksBeyondRange
        {
            get { return pEntryTicksBeyondRange; }
            set { pEntryTicksBeyondRange = Math.Max(-999,value); }
        }
		private int pTargetTicks = 10;
		[Display(GroupName="Strategy", Description="Number of ticks for target exit, taken from the entry price, enter '-1' to turn-off the targets")]
        public int TargetTicks
        {
            get { return pTargetTicks; }
            set { pTargetTicks = Math.Max(-1,value); }
        }

		private int pExitTicks = 10;
		[Display(GroupName="Strategy", Description="Number of ticks for stoploss exit, taken from the entry price, enter '-1' to turn-off the stoploss")]
        public int ExitTicks
        {
            get { return pExitTicks; }
            set { pExitTicks = Math.Max(-1,value); }
        }
		private HiLoOfTimeRange_withPivotLevels_SignalType pSignalType = HiLoOfTimeRange_withPivotLevels_SignalType.ClosesBeyond;
		[Display(GroupName="Strategy", Description="What constitutes a valid signal?  When a bar closes beyond the entry price?  Or when a bar touches the entry price?")]
        public HiLoOfTimeRange_withPivotLevels_SignalType SignalType
        {
            get { return pSignalType; }
            set { pSignalType = value; }
        }
		[Display(GroupName="Visual", Description="Prefix to add to each upside breakout")]
        public string LabelPrefixUpside
        {
            get { return pUpLabelPrefix; }
            set { pUpLabelPrefix = value; }
        }
		[Display(GroupName="Visual", Description="Prefix to add to each downside breakout")]
        public string LabelPrefixDownside
        {
            get { return pDownLabelPrefix; }
            set { pDownLabelPrefix = value; }
        }
		
		private int    pTextBkgndOpacity = 10;
		[Display(GroupName="Visual", Description="0 is fully transparent, '10' is opaque")]
        public int TextBkgndOpacity
        {
            get { return pTextBkgndOpacity; }
            set { pTextBkgndOpacity = Math.Max(0, Math.Min(10,value)); }
        }

		private Brush pTextBackgroundBrush = Brushes.White;
		[XmlIgnore()]
		[Display(GroupName="Visual", Description="")]
        public Brush TextBackgroundBrush
        {
            get { return pTextBackgroundBrush; }
            set { pTextBackgroundBrush = value; }
        }
					[Browsable(false)]
					public string TBClSerialize
					{	get { return Serialize.BrushToString(pTextBackgroundBrush); } set { pTextBackgroundBrush = Serialize.StringToBrush(value); }
					}
		private NinjaTrader.Gui.Tools.SimpleFont pTextFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial",12);
		[Display(GroupName="Visual", Description="")]
        public NinjaTrader.Gui.Tools.SimpleFont TextFont
        {
            get { return pTextFont; }
            set { pTextFont = value; }
        }
		
		#region Colors
		private Brush  pLongTextBrush = Brushes.Green;
		[XmlIgnore()]
		[Display(GroupName="Visual", Description="")]
        public Brush TextBrushUpside
        {
            get { return pLongTextBrush; }
            set { pLongTextBrush = value; }
        }
					[Browsable(false)]
					public string LClSerialize
					{	get { return Serialize.BrushToString(pLongTextBrush); } set { pLongTextBrush = Serialize.StringToBrush(value); }
					}
		private Brush  pShortTextBrush = Brushes.Red;
		[XmlIgnore()]
		[Display(GroupName="Visual", Description="")]
        public Brush TextBrushDownside
        {
            get { return pShortTextBrush; }
            set { pShortTextBrush = value; }
        }
					[Browsable(false)]
					public string SClSerialize
					{	get { return Serialize.BrushToString(pShortTextBrush); } set { pShortTextBrush = Serialize.StringToBrush(value); }
					}

		#endregion

		#region Times
		private TimeSpan pStartTime = new TimeSpan(8,0,0);
		[Description("Enter the time at the start of the range (use 24-hr clock where 15:30:00 is 3:30pm)")]
		[Category("Parameters")]
		public string TimeBegin
		{
			get { return pStartTime.Hours.ToString("0")+":"+pStartTime.Minutes.ToString("00")+":"+pStartTime.Seconds.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pStartTime)) pStartTime = new TimeSpan(0,0,0); 
				  DateTime dt;
				  if(!DateTime.TryParse(value, out dt)) pStartTime = new TimeSpan(0,0,0);
			}
		}

		private TimeSpan pEndTime = new TimeSpan(10,30,0);
		[Description("Enter the time at the end of the range (use 24-hr clock where 15:30:00 is 3:30pm)")]
		[Category("Parameters")]
		public string TimeEnd
		{
			get { return pEndTime.Hours.ToString("0")+":"+pEndTime.Minutes.ToString("00")+":"+pEndTime.Seconds.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pEndTime)) pEndTime=new TimeSpan(0,0,0); 
				  DateTime dt;
				  if(!DateTime.TryParse(value, out dt)) pEndTime = new TimeSpan(0,0,0);
			}
		}

		private TimeSpan pStopTime = new TimeSpan(12,0,0);
		[Description("Enter the time to stop the alerts (use 24-hr clock where 15:30:00 is 3:30pm)")]
		[Category("Parameters")]
		public string TimeStop
		{
			get { return pStopTime.Hours.ToString("0")+":"+pStopTime.Minutes.ToString("00")+":"+pStopTime.Seconds.ToString("00"); }
			set { if(!TimeSpan.TryParse(value, out pStopTime)) pStopTime=new TimeSpan(0,0,0); 
				  DateTime dt;
				  if(!DateTime.TryParse(value, out dt)) pStopTime = new TimeSpan(0,0,0);
			}
		}
		#endregion

		[Display(GroupName="Visual", Description="")]
        public bool ShowPivotLevels
        {
            get { return pShowPivotLevels; }
            set { pShowPivotLevels = value; }
        }
		private bool pShowPivotLevelLabels = true;
		[Display(GroupName="Visual", Description="")]
        public bool ShowPivotLevelLabels
        {
            get { return pShowPivotLevelLabels; }
            set { pShowPivotLevelLabels = value; }
        }

		[Display(GroupName="Visual", Description="Show the price at the breakout?")]
        public bool ShowBreakoutLabel
        {
            get { return pShowBreakoutLabel; }
            set { pShowBreakoutLabel = value; }
        }
		[Display(GroupName="Visual", Description="Exclude weekends?")]
        public bool ExcludeWeekends
        {
            get { return pExcludeWeekends; }
            set { pExcludeWeekends = value; }
        }
//        [Description("Puts a price label on each and every breakout throughout history...or just on the most recent breakout")]
//        [Category("Visual")]
//        public bool ShowHistoricalBreakouts
//        {
//            get { return pShowHistoricalBreakouts; }
//            set { pShowHistoricalBreakouts = value; }
//        }
        #endregion
    }
}
public enum HiLoOfTimeRange_withPivotLevels_SignalType {
	ClosesBeyond, Touches
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HiLoOfTimeRange_withPivotLevels[] cacheHiLoOfTimeRange_withPivotLevels;
		public HiLoOfTimeRange_withPivotLevels HiLoOfTimeRange_withPivotLevels()
		{
			return HiLoOfTimeRange_withPivotLevels(Input);
		}

		public HiLoOfTimeRange_withPivotLevels HiLoOfTimeRange_withPivotLevels(ISeries<double> input)
		{
			if (cacheHiLoOfTimeRange_withPivotLevels != null)
				for (int idx = 0; idx < cacheHiLoOfTimeRange_withPivotLevels.Length; idx++)
					if (cacheHiLoOfTimeRange_withPivotLevels[idx] != null &&  cacheHiLoOfTimeRange_withPivotLevels[idx].EqualsInput(input))
						return cacheHiLoOfTimeRange_withPivotLevels[idx];
			return CacheIndicator<HiLoOfTimeRange_withPivotLevels>(new HiLoOfTimeRange_withPivotLevels(), input, ref cacheHiLoOfTimeRange_withPivotLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HiLoOfTimeRange_withPivotLevels HiLoOfTimeRange_withPivotLevels()
		{
			return indicator.HiLoOfTimeRange_withPivotLevels(Input);
		}

		public Indicators.HiLoOfTimeRange_withPivotLevels HiLoOfTimeRange_withPivotLevels(ISeries<double> input )
		{
			return indicator.HiLoOfTimeRange_withPivotLevels(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HiLoOfTimeRange_withPivotLevels HiLoOfTimeRange_withPivotLevels()
		{
			return indicator.HiLoOfTimeRange_withPivotLevels(Input);
		}

		public Indicators.HiLoOfTimeRange_withPivotLevels HiLoOfTimeRange_withPivotLevels(ISeries<double> input )
		{
			return indicator.HiLoOfTimeRange_withPivotLevels(input);
		}
	}
}

#endregion
