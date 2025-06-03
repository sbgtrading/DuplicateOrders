// 
// Copyright (C) 2008, SBG Trading Corp.
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//

#region Using declarations
using System;
using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using System.Windows;
//using System.Windows.Input;
//using NinjaTrader.Gui.Tools;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SharpDX.Direct2D1;
using SharpDX;
using System.Linq;
using NinjaTrader.NinjaScript.DrawingTools;

//using NinjaTrader.NinjaScript;
//using NinjaTrader.Core.FloatingPoint;

#endregion


// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Plots the IW_InitialBalance (range of 1st hour of trading) and 25% emminations from that IB.
    /// </summary>
    [Description("Calculates and plots InitialBalance and 25% emminations from IB")]
    public class IW_InitialBalance : Indicator
    {
        #region Variables
		private string startTimeStr="9:30";
		private int openLengthInMinutes=60;
//		private System.Windows.Media.Brush color100Line= Brushes.Yellow;
//		private System.Windows.Media.Brush color75Line= Brushes.Red;
//		private System.Windows.Media.Brush color50Line= Brushes.White;
//		private System.Windows.Media.Brush color25Line= Brushes.Green;
		private double HH,LL=Double.MaxValue;
		private DateTime EndTime = DateTime.MinValue, StartTime = DateTime.MinValue, SessionEndTime = DateTime.MinValue;
		private bool ErrorFlagged = false;
		private Dictionary<DateTime,double[]> AbsBarAtSessionEnd;
//		private Pen The100Pen, The75Pen, The50Pen, The25Pen;
		private double PlotHH=0, PlotLL=0;
		private int EndSessionPixel=0, EndSessionBar=0;
		private bool UseChartData = false;
		private int DataPtr = 1;
//		private DashStyleHelper The75Pen_DashStyle;
//		private DashStyleHelper The50Pen_DashStyle;
//		private DashStyleHelper The25Pen_DashStyle;
//		private DashStyleHelper The100Pen_DashStyle;
        #endregion

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			Name = "iw IW_InitialBalance";
			bool IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
			IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
			if(!IsDebug)
				VendorLicense("IndicatorWarehouse", "AIInitialBalance", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
			AddPlot(new Stroke(System.Windows.Media.Brushes.Goldenrod,3), PlotStyle.Line, "OpenRangeHigh");
			AddPlot(new Stroke(System.Windows.Media.Brushes.Goldenrod,3), PlotStyle.Line, "OpenRangeLow");

			AddPlot(System.Windows.Media.Brushes.Orange, "Lvl1");
			AddPlot(System.Windows.Media.Brushes.Red,    "Lvl2");
			AddPlot(System.Windows.Media.Brushes.Blue,   "Lvl3");
			AddPlot(System.Windows.Media.Brushes.Green,  "Lvl4");
			AddPlot(System.Windows.Media.Brushes.DimGray, "Lvl5");
			AddPlot(System.Windows.Media.Brushes.Pink,   "Lvl6");
			AddPlot(System.Windows.Media.Brushes.Purple, "Lvl7");
			AddPlot(System.Windows.Media.Brushes.Cyan,   "Lvl8");
			AddPlot(System.Windows.Media.Brushes.Orchid, "Lvl9");
			AddPlot(System.Windows.Media.Brushes.Yellow, "Lvl10");
			AddPlot(System.Windows.Media.Brushes.Brown,  "Lvl11");
			Calculate = Calculate.OnBarClose;
			IsOverlay = true;
			//PriceTypeSupported	= false;
			IsAutoScale = false;

//			The75Pen  = new Pen(color75Line,1);  The75Pen_DashStyle  = DashStyleHelper.Dash;
//			The50Pen  = new Pen(color50Line,1);  The50Pen_DashStyle  = DashStyleHelper.Dash;
//			The25Pen  = new Pen(color25Line,1);  The25Pen_DashStyle  = DashStyleHelper.Dash;
//			The100Pen = new Pen(color100Line,1); The100Pen_DashStyle = DashStyleHelper.Solid;

 		}
		if (State == State.Configure) {
			AddDataSeries(BarsPeriodType.Minute,1);
			AbsBarAtSessionEnd = new Dictionary<DateTime,double[]>();
			EndTime        = DateTime.MinValue;
			SessionEndTime = DateTime.MinValue;
			try {StartTime = DateTime.Parse(StartTimeStr); }
			catch {Log("Invalid start time, must be in 24hr format: 'hh:mm'",LogLevel.Alert); ErrorFlagged=true; return;}
			if(pDataSource != InitialBalance_DataSource.OneMinuteData) UseChartData = true;
			else UseChartData = BarsArray[0].BarsPeriod.BarsPeriodType==BarsPeriodType.Minute && BarsArray[0].BarsPeriod.Value==1;
			if(UseChartData) DataPtr = 0;
		}
	}
		protected override void OnBarUpdate() {
			if(ErrorFlagged) return;
			if(BarsInProgress == 0 && CurrentBars[0] < 2) {
				return;
			}
			if(BarsInProgress == 1 && CurrentBars[1] < 2) {
				return;
			}
try{
			if(BarsArray[1]==null) Draw.TextFixed(this, "problem","Missing BarsArray[1]",TextPosition.Center);
			if(SessionEndTime == DateTime.MinValue) {
				if(UseChartData) {
					StartTime      = new DateTime(Times[0][0].Year,Times[0][0].Month,Times[0][0].Day,StartTime.Hour,StartTime.Minute,StartTime.Second);
					EndTime        = StartTime.AddMinutes(openLengthInMinutes);
					SessionEndTime = StartTime.AddHours(pLenOfSessionInHrs);
//Print(Times[0][0].ToString()+"   Initialized  StartTime: "+StartTime.ToString()+"  EndTime: "+EndTime.ToString());
				} else {
					StartTime      = new DateTime(Times[DataPtr][0].Year,Times[DataPtr][0].Month,Times[DataPtr][0].Day,StartTime.Hour,StartTime.Minute,StartTime.Second);
					EndTime        = StartTime.AddMinutes(openLengthInMinutes);
					SessionEndTime = StartTime.AddHours(pLenOfSessionInHrs);
//Print(Times[0][0].ToString()+"   Initialized  StartTime: "+StartTime.ToString()+"  EndTime: "+EndTime.ToString());
				}
			}

			if(BarsInProgress == 1 || UseChartData) {
				if(Times[DataPtr][0].CompareTo(StartTime) > 0) //current bar is after the Start of the session
				{
					if(Times[DataPtr][1].CompareTo(StartTime) <= 0) //if previous bar was before the Start of the session
					{
						HH = Highs[DataPtr][0];
						LL = Lows[DataPtr][0];
					}
					if(Times[DataPtr][1].CompareTo(EndTime) < 0 && Times[DataPtr][0].CompareTo(EndTime)>=0) {//if prior bar is before the end of the Open
						if(!pHideVerticalLine) Draw.Line(this, "EndOfOpen"+CurrentBars[DataPtr].ToString(), false, 0,(HH-LL)*4+HH, 0, LL-(HH-LL)*4, pColorVertLine, pVerticalLineDashStyle, pVerticalLineWidth);
					}
					if(Times[DataPtr][0].CompareTo(EndTime) <= 0) //if current bar is before the end of the OpeningRange
					{
						HH = Math.Max(HH,Highs[DataPtr][0]);
						LL = Math.Min(LL,Lows[DataPtr][0]);
					}
				}
				if(Times[DataPtr][0].Ticks > EndTime.Ticks && Times[DataPtr][1].Ticks <= EndTime.Ticks) {
//Print(Times[DataPtr][0].ToString()+"  HH: "+HH.ToString()+"   LL: "+LL.ToString());
					if(LL > 0) {
//						double range = HH-LL;

//						double y0  = range*0.75 +LL;
//						double y1  = range*0.5  +LL;
//						double y2  = range*0.25 +LL;

//						double y3  = range*3.0  +HH;
//						double y4  = range*2.75 +HH;
//						double y5  = range*2.50 +HH;
//						double y6  = range*2.25 +HH;
//						double y7  = range*2.0  +HH;
//						double y8  = range*1.75 +HH;
//						double y9  = range*1.50 +HH;
//						double y10 = range*1.25 +HH;
//						double y11 = range*1.0  +HH;
//						double y12 = range*0.75 +HH;
//						double y13 = range*0.5  +HH;
//						double y14 = range*0.25 +HH;

//						double y15 = LL-range*3.0;
//						double y16 = LL-range*2.75;
//						double y17 = LL-range*2.50;
//						double y18 = LL-range*2.25;
//						double y19 = LL-range*2.0;
//						double y20 = LL-range*1.75;
//						double y21 = LL-range*1.5;
//						double y22 = LL-range*1.25;
//						double y23 = LL-range*1.0;
//						double y24 = LL-range*0.75;
//						double y25 = LL-range*0.5;
//						double y26 = LL-range*0.25;
////	Print("Adding levels at this date:  "+EndTime.Date.ToShortDateString()+"  BarAtSessionEnd: "+CurrentBars[0]);
//						AbsBarAtSessionEnd[EndTime.Date] = new double[28]{y0,y1,y2,y3,y4,y5,y6,y7,y8,y9,y10,y11,y12,y13,y14,y15,y16,y17,y18,y19,y20,y21,y22,y23,y24,y25,y26,CurrentBars[0]};//CurrentBars[0];
					}
//					DrawLineAtEndOfOpen = true;
				}
			}
//			if(BarsInProgress == 0 && DrawLineAtEndOfOpen) {
//				DrawLineAtEndOfOpen = false;
//			}


			if(BarsInProgress == 0 && HH-LL > 0) {
				if(HH!=Double.MinValue) OR_High[0] = (HH);
				if(LL!=Double.MaxValue) OR_Low[0] = (LL);
				double range = HH-LL;
				if(pLvl1_Pct  != 0) Lvl1[0]  = (pLvl1_Pct>0 ? LL + pLvl1_Pct/100*range : LL + (100+pLvl1_Pct)/100*range);
				if(pLvl2_Pct  != 0) Lvl2[0]  = (pLvl2_Pct>0 ? LL + pLvl2_Pct/100*range : LL + (100+pLvl2_Pct)/100*range);
				if(pLvl3_Pct  != 0) Lvl3[0]  = (pLvl3_Pct>0 ? LL + pLvl3_Pct/100*range : LL + (100+pLvl3_Pct)/100*range);
				if(pLvl4_Pct  != 0) Lvl4[0]  = (pLvl4_Pct>0 ? LL + pLvl4_Pct/100*range : LL + (100+pLvl4_Pct)/100*range);
				if(pLvl5_Pct  != 0) Lvl5[0]  = (pLvl5_Pct>0 ? LL + pLvl5_Pct/100*range : LL + (100+pLvl5_Pct)/100*range);
				if(pLvl6_Pct  != 0) Lvl6[0]  = (pLvl6_Pct>0 ? LL + pLvl6_Pct/100*range : LL + (100+pLvl6_Pct)/100*range);
				if(pLvl7_Pct  != 0) Lvl7[0]  = (pLvl7_Pct>0 ? LL + pLvl7_Pct/100*range : LL + (100+pLvl7_Pct)/100*range);
				if(pLvl8_Pct  != 0) Lvl8[0]  = (pLvl8_Pct>0 ? LL + pLvl8_Pct/100*range : LL + (100+pLvl8_Pct)/100*range);
				if(pLvl9_Pct  != 0) Lvl9[0]  = (pLvl9_Pct>0 ? LL + pLvl9_Pct/100*range : LL + (100+pLvl9_Pct)/100*range);
				if(pLvl10_Pct != 0) Lvl10[0] = (pLvl10_Pct>0 ? LL + pLvl10_Pct/100*range : LL + (100+pLvl10_Pct)/100*range);
				if(pLvl11_Pct != 0) Lvl11[0] = (pLvl11_Pct>0 ? LL + pLvl11_Pct/100*range : LL + (100+pLvl11_Pct)/100*range);
			}
			if(BarsInProgress == 1 || UseChartData) 
			{
				while(Times[DataPtr][0].Ticks > SessionEndTime.Ticks) 
				{
					HH = double.MinValue;
					LL = double.MaxValue;
					StartTime      = StartTime.AddDays(1);
					EndTime        = StartTime.AddMinutes(openLengthInMinutes);
					SessionEndTime = StartTime.AddHours(pLenOfSessionInHrs);
//Print(Times[0][0].ToString()+"  StartTime: "+StartTime.ToString()+"  EndTime: "+EndTime.ToString());
				}
			}
}catch(Exception err){string s = err.ToString();  if(!s.Contains("Index was out of range."))Print(Name+": OnBarUpdate: "+err.ToString());}
		}

//===============================================================================================================================================
		SortedDictionary<string, SharpDX.Direct2D1.Brush> brushes = new SortedDictionary<string, SharpDX.Direct2D1.Brush>();
		public override void OnRenderTargetChanged()
		{
			var keys = brushes.Keys.ToList();
			foreach(var x in keys) {
				if(brushes[x]==null) continue;
				if(brushes[x].IsDisposed) continue;
				brushes[x].Dispose();
				brushes[x] = null;
			}
			for(int plot = 0; plot<Plots.Length; plot++){
				string k = Plots[plot].Brush.ToString();
				if(RenderTarget!=null && !brushes.ContainsKey(k)) brushes[k]  = Plots[plot].Brush.ToDxBrush(RenderTarget);
			}
//			if(RenderTarget!=null && !brushes.ContainsKey(The25Pen.Brush.ToString())) brushes[The25Pen.Brush.ToString()]  = The25Pen.Brush.ToDxBrush(RenderTarget);
//			if(RenderTarget!=null && !brushes.ContainsKey(The50Pen.Brush.ToString())) brushes[The50Pen.Brush.ToString()]  = The50Pen.Brush.ToDxBrush(RenderTarget);
//			if(RenderTarget!=null && !brushes.ContainsKey(The75Pen.Brush.ToString())) brushes[The75Pen.Brush.ToString()]  = The75Pen.Brush.ToDxBrush(RenderTarget);
//			if(RenderTarget!=null && !brushes.ContainsKey(The100Pen.Brush.ToString())) brushes[The100Pen.Brush.ToString()] = The100Pen.Brush.ToDxBrush(RenderTarget);
		}

//====================================================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {

			if (!IsVisible) return;double ChartMinPrice = chartScale.MinValue; double ChartMaxPrice = chartScale.MaxValue;
			if(chartControl==null) return;
			base.OnRender(chartControl, chartScale);
			int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = Math.Min(CurrentBars[0]-1,ChartBars.ToIndex);

			var font = new NinjaTrader.Gui.Tools.SimpleFont("Arial",12);
			var txtFormat = font.ToDirectWriteTextFormat();
			var txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, "-175% "+Instrument.MasterInstrument.FormatPrice(Closes[0][0]), txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
			var x = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex));
			float y = 0f;
			SharpDX.RectangleF labelRect;
			float op;
			float fontsize = Convert.ToSingle(font.Size);

			if(pShowLevelLabels){
				int id = 2;
				double Val = 0;
				if(pLvl1_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl1_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 3;
				if(pLvl2_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl2_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 4;
				if(pLvl3_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl3_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 5;
				if(pLvl4_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl4_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 6;
				if(pLvl5_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl5_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 7;
				if(pLvl6_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl6_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 8;
				if(pLvl7_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl7_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 9;
				if(pLvl8_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl8_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 10;
				if(pLvl9_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl9_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 11;
				if(pLvl10_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl10_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
				id = 12;
				if(pLvl11_Pct != 0 && Plots[id].Brush!=System.Windows.Media.Brushes.Transparent){
					Val = Values[id].GetValueAt(lastBarPainted);
					y = chartScale.GetYByValue(Val);
					labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
					RenderTarget.DrawText(string.Format("{0}% {1}",pLvl11_Pct.ToString("0"),Instrument.MasterInstrument.FormatPrice(Val)), txtFormat, labelRect, Plots[id].BrushDX);
				}
			}

//			if(pHideExtensionLines) return;
			#region Plot
try{
			int RightBarAbsolute = Math.Min(CurrentBars[0], Math.Max(0,lastBarPainted));
			if(RightBarAbsolute > CurrentBars[0]) RightBarAbsolute = CurrentBars[0];

//			DateTime dt = Time.GetValueAt(RightBarAbsolute).Date;
//			bool found = false;
//			double [] array = new double[28];
//			while(!found) {
//				{
//					if(!AbsBarAtSessionEnd.TryGetValue(dt, out array)) {
//						dt = dt.AddDays(-1);
//						continue;
//					}
//					else {
//						EndSessionBar = (int)array[27];
//					}
//				}

//				if(EndSessionBar > RightBarAbsolute) {
//					dt = dt.AddDays(-1);
//					continue;
//				}
//				else {
//					found = true;
//				}
//			}
			if(EndSessionBar>CurrentBars[0] || EndSessionBar<=0) return;
			EndSessionPixel = chartControl.GetXByBarIndex(ChartBars, EndSessionBar);
//			for(int id = 2; id<Plots.Length; id++){
//				drawline(ref chartScale, Plots[id].Pen,  EndSessionPixel,  array[id-2], ChartPanel.W, The75Pen_DashStyle);
//			}
//			double y = range*0.75+PlotLL;	drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//
//			y = range*0.5+PlotLL;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*0.25+PlotLL;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//
//			y = range*3.0+PlotHH;   drawline(ref chartScale, The100Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*2.75+PlotHH;  drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*2.50+PlotHH;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*2.25+PlotHH;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*2.0+PlotHH;	drawline(ref chartScale, The100Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*1.75+PlotHH;	drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*1.50+PlotHH;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*1.25+PlotHH;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*1.0+PlotHH;	drawline(ref chartScale, The100Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*0.75+PlotHH;	drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*0.5+PlotHH;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = range*0.25+PlotHH;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//
//			y = PlotLL-range*3.0;	drawline(ref chartScale, The100Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*2.75;	drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*2.50;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*2.25;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*2.0;	drawline(ref chartScale, The100Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*1.75;	drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*1.5;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*1.25;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*1.0;	drawline(ref chartScale, The100Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*0.75;	drawline(ref chartScale, The75Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*0.5;	drawline(ref chartScale, The50Pen, EndSessionPixel, y, ChartPanel.W);
//			y = PlotLL-range*0.25;	drawline(ref chartScale, The25Pen, EndSessionPixel, y, ChartPanel.W);
//			txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, "75%", txtFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
//			var arridx = new List<int>(){0,4,8,12,16,20,24};
//			foreach(var idx in arridx){
//				y = chartScale.GetYByValue(array[idx]);
//				labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
//				RenderTarget.DrawText("75%", txtFormat, labelRect, brushes[The75Pen.Brush.ToString()]);
//			}
//			arridx = new List<int>(){1,5,9,13,17,21,25};
//			foreach(var idx in arridx){
//				y = chartScale.GetYByValue(array[idx]);
//				labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
//				RenderTarget.DrawText("50%", txtFormat, labelRect, brushes[The50Pen.Brush.ToString()]);
//			}
//			arridx = new List<int>(){2,6,10,14,18,22,26};
//			foreach(var idx in arridx){
//				y = chartScale.GetYByValue(array[idx]);
//				labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, fontsize);
//				RenderTarget.DrawText("25%", txtFormat, labelRect, brushes[The25Pen.Brush.ToString()]);
//			}


}catch(Exception exx){Print("Error: "+Instrument.FullName+" "+exx.ToString());}
			#endregion
		}

		private void drawline(ref ChartScale cs, System.Windows.Media.Pen ThePen, int EndSessionPixel, double y, int width, DashStyleHelper dashstyle){
			int ypix = cs.GetYByValue( y);
			SharpDX.Direct2D1.DashStyle _dashstyle;
            if (!Enum.TryParse(dashstyle.ToString(), true, out _dashstyle)) _dashstyle = SharpDX.Direct2D1.DashStyle.Dash;
            SharpDX.Direct2D1.StrokeStyleProperties properties = new SharpDX.Direct2D1.StrokeStyleProperties() { DashStyle = _dashstyle };
            SharpDX.Direct2D1.StrokeStyle strokestyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, properties);
//Print("Drawing line at ypix: "+ypix+"  EndSessionPixel: "+EndSessionPixel);
			var s = ThePen.Brush.ToString();
			if(brushes.ContainsKey(s) && brushes[s]!=null && !brushes[s].IsDisposed)
				RenderTarget.DrawLine(new SharpDX.Vector2(EndSessionPixel,ypix), new SharpDX.Vector2(width,ypix), brushes[ThePen.Brush.ToString()], Convert.ToSingle(ThePen.Thickness), strokestyle);//, EndSessionPixel, ypix, width, ypix);
		}

        #region Properties
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> OR_High{ get { return Values[0]; }}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> OR_Low { get { return Values[1]; } }

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl1 { get { return Values[2]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl2 { get { return Values[3]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl3 { get { return Values[4]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl4 { get { return Values[5]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl5 { get { return Values[6]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl6 { get { return Values[7]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl7 { get { return Values[8]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl8 { get { return Values[9]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl9 { get { return Values[10]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl10 { get { return Values[11]; } }
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Lvl11 { get { return Values[12]; } }

//		private bool pHideExtensionLines = true;
//		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Hide extension lines?",  Description = "Hide lines that are above the Range High and below the Range Low", GroupName = "Custom Visuals")]
//        public bool HideExtensionLines
//        {
//            get { return pHideExtensionLines; }
//            set { pHideExtensionLines = value; }
//        }

		private bool pHideVerticalLine = false;
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Hide vertical line?",  Description = "Hide the vertical line drawn at the end of the opening range", GroupName = "Custom Visuals")]
        public bool HideVerticalLine
        {
            get { return pHideVerticalLine; }
            set { pHideVerticalLine = value; }
        }
		
		private System.Windows.Media.Brush pColorVertLine = System.Windows.Media.Brushes.Yellow;
		[XmlIgnore()]
		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Vertical line color",  Description = "Color of Vertical line", GroupName = "Custom Visuals")]
		public System.Windows.Media.Brush ColorVertLine
		{
			get { return pColorVertLine; }
			set { pColorVertLine = value; }
		}
					[Browsable(false)]
					public string ColorVertLineSerialize{get { return Serialize.BrushToString(pColorVertLine); }set { pColorVertLine = Serialize.StringToBrush(value); }}

		private int pVerticalLineWidth = 2;
		[Display(Order = 40, ResourceType = typeof(Custom.Resource), Name = "Vertical Line Width",  Description = "", GroupName = "Custom Visuals")]
        public int VerticalLineWidth
        {
            get { return pVerticalLineWidth; }
            set { pVerticalLineWidth = Math.Max(1, value); }
        }
		private DashStyleHelper pVerticalLineDashStyle = DashStyleHelper.Dot;
		[Display(Order = 50, ResourceType = typeof(Custom.Resource), Name = "Vertical Line dash style",  Description = "", GroupName = "Custom Visuals")]
        public DashStyleHelper VerticalLineDashStyle
        {
            get { return pVerticalLineDashStyle; }
            set { pVerticalLineDashStyle = value; }
        }
//==============================================
//		[XmlIgnore()]
//		[Description("Color of 100% lines.")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "100% line color",  GroupName = "Custom Visuals")]
//		public System.Windows.Media.Brush Color100Line
//		{
//			get { return color100Line; }
//			set { color100Line = value; }
//		}
//					[Browsable(false)]
//					public string Color100LineSerialize{get { return Serialize.BrushToString(color100Line); }set { color100Line = Serialize.StringToBrush(value); }}
////==============================================
//		[XmlIgnore()]
//		[Description("Color of 75% lines.")]
//// 		[Category("Custom Visuals")]
//// [Gui.Design.DisplayNameAttribute("75% line color")]
//[Display(ResourceType = typeof(Custom.Resource), Name = "75% line color",  GroupName = "Custom Visuals")]
//		public System.Windows.Media.Brush Color75Line
//		{
//			get { return color75Line; }
//			set { color75Line = value; }
//		}
//				[Browsable(false)]
//				public string Color75LineSerialize
//				{
//					get { return Serialize.BrushToString(color75Line); }
//					set { color75Line = Serialize.StringToBrush(value); }
//				}
////==============================================
//		[XmlIgnore()]
//		[Description("Color of 50% lines.")]
//// 		[Category("Custom Visuals")]
//// [Gui.Design.DisplayNameAttribute("50% line color")]
//[Display(ResourceType = typeof(Custom.Resource), Name = "50% line color",  GroupName = "Custom Visuals")]
//		public System.Windows.Media.Brush Color50Line
//		{
//			get { return color50Line; }
//			set { color50Line = value; }
//		}
//				[Browsable(false)]
//				public string Color50LineSerialize
//				{
//					get { return Serialize.BrushToString(color50Line); }
//					set { color50Line = Serialize.StringToBrush(value); }
//				}
////==============================================
//		[XmlIgnore()]
//		[Description("Color of 25% lines.")]
//// 		[Category("Custom Visuals")]
//// [Gui.Design.DisplayNameAttribute("25% line color")]
//[Display(ResourceType = typeof(Custom.Resource), Name = "25% line color",  GroupName = "Custom Visuals")]
//		public System.Windows.Media.Brush Color25Line
//		{
//			get { return color25Line; }
//			set { color25Line = value; }
//		}
//				[Browsable(false)]
//				public string Color25LineSerialize
//				{
//					get { return Serialize.BrushToString(color25Line); }
//					set { color25Line = Serialize.StringToBrush(value); }
//				}

        [Description("MAKE SURE YOU USE 24hr TIME FORMAT!")]
        [Category("Parameters")]
        public string StartTimeStr
        {
            get { return startTimeStr; }
            set { startTimeStr = value; }
        }

        [Description("")]
        [Category("Parameters")]
        public int OpenLengthInMinutes
        {
            get { return openLengthInMinutes; }
            set { openLengthInMinutes = Math.Max(0,value); }
        }
		private double pLenOfSessionInHrs = 23;
        [Description("Length of full session, in hours (max of 24).  Opening Range lines will not be drawn outside of the session.")]
        [Category("Parameters")]
        public double LenOfSessionInHrs
        {
            get { return pLenOfSessionInHrs; }
            set { pLenOfSessionInHrs = Math.Max(0,Math.Min(value,24-59/60)); }
        }

		private InitialBalance_DataSource pDataSource = InitialBalance_DataSource.OneMinuteData;
		[Description("Data source can be either the Chart Data itself, or a secondary 1-minute datafeed")]
		[Category("Parameters")]
		public InitialBalance_DataSource DataSource {
		    get { return pDataSource; }
		    set {        pDataSource = value; }
		}

		private double pLvl1_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 1 %", GroupName = "Levels", Order = 10, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl1_Pct {
		    get { return pLvl1_Pct; }
		    set {        pLvl1_Pct = value; }}

		private double pLvl2_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 2 %", GroupName = "Levels", Order = 20, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl2_Pct {
		    get { return pLvl2_Pct; }
		    set {        pLvl2_Pct = value; }}

		private double pLvl3_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 3 %", GroupName = "Levels", Order = 30, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl3_Pct {
		    get { return pLvl3_Pct; }
		    set {        pLvl3_Pct = value; }}

		private double pLvl4_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 4 %", GroupName = "Levels", Order = 40, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl4_Pct {
		    get { return pLvl4_Pct; }
		    set {        pLvl4_Pct = value; }}

		private double pLvl5_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 5 %", GroupName = "Levels", Order = 50, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl5_Pct {
		    get { return pLvl5_Pct; }
		    set {        pLvl5_Pct = value; }}

		private double pLvl6_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 6 %", GroupName = "Levels", Order = 60, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl6_Pct {
		    get { return pLvl6_Pct; }
		    set {        pLvl6_Pct = value; }}

		private double pLvl7_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 7 %", GroupName = "Levels", Order = 70, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl7_Pct {
		    get { return pLvl7_Pct; }
		    set {        pLvl7_Pct = value; }}

		private double pLvl8_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 8 %", GroupName = "Levels", Order = 80, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl8_Pct {
		    get { return pLvl8_Pct; }
		    set {        pLvl8_Pct = value; }}

		private double pLvl9_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 9 %", GroupName = "Levels", Order = 90, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl9_Pct {
		    get { return pLvl9_Pct; }
		    set {        pLvl9_Pct = value; }}

		private double pLvl10_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 10 %", GroupName = "Levels", Order = 100, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl10_Pct {
		    get { return pLvl10_Pct; }
		    set {        pLvl10_Pct = value; }}

		private double pLvl11_Pct = 0;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lvl 11 %", GroupName = "Levels", Order = 110, Description="Enter the percent value (10 = 10%) for this Level")]
		public double   Lvl11_Pct {
		    get { return pLvl11_Pct; }
		    set {        pLvl11_Pct = value; }}

		private bool pShowLevelLabels = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show labels?", GroupName = "Levels", Order = 120, Description="Show labels on each of these 11 levels")]
        public bool ShowLevelLabels
        {
            get { return pShowLevelLabels; }
            set { pShowLevelLabels = value; }
        }

		#endregion
    }
}
public enum InitialBalance_DataSource {
	ChartData, OneMinuteData
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_InitialBalance[] cacheIW_InitialBalance;
		public IW_InitialBalance IW_InitialBalance()
		{
			return IW_InitialBalance(Input);
		}

		public IW_InitialBalance IW_InitialBalance(ISeries<double> input)
		{
			if (cacheIW_InitialBalance != null)
				for (int idx = 0; idx < cacheIW_InitialBalance.Length; idx++)
					if (cacheIW_InitialBalance[idx] != null &&  cacheIW_InitialBalance[idx].EqualsInput(input))
						return cacheIW_InitialBalance[idx];
			return CacheIndicator<IW_InitialBalance>(new IW_InitialBalance(), input, ref cacheIW_InitialBalance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_InitialBalance IW_InitialBalance()
		{
			return indicator.IW_InitialBalance(Input);
		}

		public Indicators.IW_InitialBalance IW_InitialBalance(ISeries<double> input )
		{
			return indicator.IW_InitialBalance(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_InitialBalance IW_InitialBalance()
		{
			return indicator.IW_InitialBalance(Input);
		}

		public Indicators.IW_InitialBalance IW_InitialBalance(ISeries<double> input )
		{
			return indicator.IW_InitialBalance(input);
		}
	}
}

#endregion
