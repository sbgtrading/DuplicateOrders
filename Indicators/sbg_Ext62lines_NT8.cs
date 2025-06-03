
// 
// Copyright (C) 2017, SBG Trading Corp.    www.sbgtradingcorp.com
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
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
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
using System.Linq;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Shows fib extensions of recent swings
    /// </summary>
    [Description("Shows fib extensions of recent swings")]
    public class Ext62Lines : Indicator
    {
        #region Variables
        // Wizard generated variables
            private int pSignificance = 4; // Default setting for Significance
			private int linewidth = 2;
			private bool pDrawAsTrendLines = false;
			private int pMaxAgeOfPivots = 50;
			private double pMinimumATRmultToFibA = 1.5;
			private double pMinimumATRmultToFibB = 3.2;
            private double fibA = 0.618; // Default setting for FibA
            private double fibB = 0.0; // Default setting for FibB
			private Brush fibacolor = Brushes.LimeGreen;
			private Brush fibbcolor = Brushes.Magenta;
        // User defined variables (add any user defined variables below)
			private double FractalHigh,FractalLow;
			private double PriorFractalHigh, PriorFractalLow;
 			private System.Collections.Generic.List<DateTime> Timep1H, Timep1L;
 			private System.Collections.Generic.List<double> ExtAH, ExtAL;
			private int i,j,k;

			private int pMaxBars = 1500;
			private double EMPTY = -1.0;
        #endregion
		DateTime expireDT = new DateTime(2025,4,30,0,0,0);
		System.Collections.Generic.List<double> ranges = new System.Collections.Generic.List<double>();

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
 		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "sbg Ext62Lines";
				AddPlot(new Stroke(Brushes.Cyan,5), PlotStyle.Dot, "Pivot");
				AddPlot(new Stroke(Brushes.Green,1), PlotStyle.Hash, "PivotHighFibA");
				AddPlot(new Stroke(Brushes.Magenta,1), PlotStyle.Hash, "PivotHighFibB");
				AddPlot(new Stroke(Brushes.Cyan,1), PlotStyle.Hash, "PivotLowFibA");
				AddPlot(new Stroke(Brushes.Yellow,1), PlotStyle.Hash, "PivotLowFibB");
				Calculate=Calculate.OnBarClose;
				IsOverlay=true;
				pRiskPerTrade = 300;
				pBuyDotColor = Brushes.Cyan;
				pSellDotColor = Brushes.Magenta;
				
				var ExemptMachines = new System.Collections.Generic.List<string>(){
						"B0D2E9D1C802E279D3678D7DE6A33CE4" /*Ben Laptop*/,
						"other",//name and email address of exmpted
					};
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachines.Contains(NinjaTrader.Cbi.License.MachineId);
				if(IsBen)
					expireDT = DateTime.MaxValue;
				else if(expireDT == DateTime.MaxValue)//not me and the expiration date is ignored
					VendorLicense("DayTradersAction", "SbgExt62Lines", "www.sbgtradingcorp.com", "support@sbgtradingcorp.com");

			}
			else if (State == State.DataLoaded)
			{
				Timep1H = new System.Collections.Generic.List<DateTime>();
				Timep1L = new System.Collections.Generic.List<DateTime>();
				ExtAH = new System.Collections.Generic.List<double>();
				ExtAL = new System.Collections.Generic.List<double>();
				if(ChartPanel!=null){
					ChartPanel.MouseMove  += OnMouseMove;
				}
			}
			else if(State == State.Terminated){
				if(ChartPanel!=null){
					ChartPanel.MouseMove  -= OnMouseMove;
				}
			}
		}

		int cb = 0;
		double avgRangeA = 0;
		double avgRangeB = 0;
		private class TradeSetupInfo{
			public double EntryPrice;
			public double StopPrice;
			public int StopABar;
			public TradeSetupInfo(double entryPrice, double stopPrice, int stopABar){ EntryPrice = entryPrice; StopPrice = stopPrice; StopABar = stopABar; }
		}
		System.Collections.Generic.SortedDictionary<int,TradeSetupInfo> TradeSetups = new System.Collections.Generic.SortedDictionary<int, TradeSetupInfo>();
		DateTime dt = new DateTime(2025,4,4,6,26,0);
		protected override void OnBarUpdate()
		{
bool z = Times[0][0].Day==4 && Times[0][0].Hour==6 && Times[0][0].Minute==26;
			if(IsFirstTickOfBar && CurrentBar>1){
				ranges.Add(Highs[0][1] - Lows[0][1]);
				while(ranges.Count>20) ranges.RemoveAt(0);
				avgRangeA = ranges.Average() * pMinimumATRmultToFibA;
				avgRangeB = ranges.Average() * pMinimumATRmultToFibB;
			}
			if(!pDrawAsTrendLines)
				if(CurrentBar < Significance*3) return;
			else if(CurrentBar < Math.Max(Significance*3,Bars.Count-pMaxBars)) return;
			
			int EndpointTime=-1;
			double MajorHigh,MajorLow;
			int rBarP1;
			string LineName;
			bool DrawThisExtension=false;
			int BarToTest = Significance;

			PriorFractalHigh = FractalHigh;
			PriorFractalLow = FractalLow;
			FractalHigh = High[BarToTest]+TickSize;
			FractalLow = Low[BarToTest]-TickSize;
			for(j = Significance;j>0;j--)
			{
				if(Low[BarToTest+j]<Low[BarToTest]) FractalLow = EMPTY;
				if(Low[BarToTest-j]<Low[BarToTest]) FractalLow = EMPTY;
				if(High[BarToTest+j]>High[BarToTest]) FractalHigh = EMPTY;
				if(High[BarToTest-j]>High[BarToTest]) FractalHigh = EMPTY;
			}
			if(FractalLow != EMPTY && FractalLow != PriorFractalLow)
			{
				Timep1L.Add(Time[BarToTest]);
				ExtAL.Add(EMPTY);
			} else if(FractalHigh != EMPTY && FractalHigh != PriorFractalHigh)
			{
				Timep1H.Add(Time[BarToTest]);
				ExtAH.Add(EMPTY);
			}
			int b = 0;
			for(var i = 0; i< Timep1H.Count; i++){
				b = CurrentBar - Bars.GetBar(Timep1H[i]);
				if(b > pMaxAgeOfPivots && ExtAH.Count > i) {
					if(ExtAH[i] == EMPTY){
						Pivot.Reset(b);
						var abar = Bars.GetBar(Timep1H[i]);
						if(TradeSetups.ContainsKey(abar)) TradeSetups.Remove(abar);
					}
					Timep1H.RemoveAt(i);
					ExtAH.RemoveAt(i);
				}
			}
			for(var i = 0; i< Timep1L.Count; i++){
				b = CurrentBar - Bars.GetBar(Timep1L[i]);
				if(b > pMaxAgeOfPivots && ExtAL.Count > i) {
					if(ExtAL[i] == EMPTY){
						Pivot.Reset(b);
						var abar = Bars.GetBar(Timep1L[i]);
						if(TradeSetups.ContainsKey(abar)) TradeSetups.Remove(abar);
					}
					Timep1L.RemoveAt(i);
					ExtAL.RemoveAt(i);
				}
			}

//   Search the ExtAH array for uncompleted P1high
			for (j=0; j<Timep1H.Count; j++){
				DrawThisExtension = false;
				if(ExtAH[j] == EMPTY) //found an uncompleted P1
				{
					rBarP1 = iBarShift(0, Timep1H[j], false);
					var abar = CurrentBar - rBarP1;
					MajorLow = High[rBarP1];
					for(k=rBarP1-1;k>=Math.Max(rBarP1-Math.Min(CurrentBar,pMaxAgeOfPivots),0);k--)
					{
						if(High[k] > High[rBarP1] && !DrawThisExtension) //when prices go higher than P1 level, draw the (sell) ext62 line
						{
							DrawThisExtension = true;
							EndpointTime = iBarShift(0, Time[k], false);
							break;
						}
						if(Low[k]<MajorLow){
							MajorLow=Low[k]; //found a new Low to compute the 62% extension
							if(!TradeSetups.ContainsKey(abar)) TradeSetups[abar] = new TradeSetupInfo(High[rBarP1], MajorLow, CurrentBar-k);
							else {TradeSetups[abar].StopPrice = MajorLow; TradeSetups[abar].StopABar = CurrentBar-k;}
						}
					}

					var distA = fibA*(High[rBarP1]-MajorLow);
					var distB = fibB*(High[rBarP1]-MajorLow);
					if((fibA>0.0 && distA > avgRangeA) || (fibB>0.0 && distB > avgRangeB)){
						Pivot[rBarP1] = High[rBarP1] + TickSize;
						PlotBrushes[0][rBarP1] = pBuyDotColor;
					}
					if(DrawThisExtension)
					{
						if(fibA>0.0 && distA > avgRangeA)
						{	//LineName="ExtAH_"+Time[rBarP1].ToBinary().ToString();
							LineName = $"ExtAH_{(CurrentBar-rBarP1)}";
							ExtAH[j] = High[rBarP1] + distA;
							SetTrendline(LineName, rBarP1, ExtAH[j], EndpointTime, ExtAH[j], DashStyleHelper.Solid);
						}
						if(fibB>0.0 && distB > avgRangeB)
						{	//LineName="ExtBH_"+Time[rBarP1].ToBinary().ToString();
							LineName = $"ExtBH_{(CurrentBar-rBarP1)}";
							double eb = High[rBarP1] + distB;
							SetTrendline(LineName,rBarP1,eb,EndpointTime,eb,DashStyleHelper.Solid);
						}
						DrawThisExtension = false;
					}
				}
			}
//	Search the ExtAL array for uncompleted P1low
			for (j=0;j<Timep1L.Count;j++)
			{
				DrawThisExtension=false;
				if(ExtAL[j] == EMPTY) //found an uncompleted P1
				{
					rBarP1 = iBarShift(0, Timep1L[j], false);
					var abar = CurrentBar - rBarP1;
					MajorHigh = Low[rBarP1];
					for(k = rBarP1-1; k >= Math.Max(rBarP1-Math.Min(CurrentBar,pMaxAgeOfPivots),0); k--)
					{
						if(Low[k]<Low[rBarP1] && !DrawThisExtension)
						{
							DrawThisExtension = true;
							EndpointTime = iBarShift(0,Time[k],false);
							break;
						}
						if(High[k]>MajorHigh) {
							MajorHigh = High[k];
							if(!TradeSetups.ContainsKey(abar)) TradeSetups[abar] = new TradeSetupInfo(Low[rBarP1], MajorHigh, CurrentBar-k);
							else {TradeSetups[abar].StopPrice = MajorHigh; TradeSetups[abar].StopABar = CurrentBar-k;}
						}
					}

					var distA = fibA*(MajorHigh-Low[rBarP1]);
					var distB = fibB*(MajorHigh-Low[rBarP1]);
					if((fibA>0.0 && distA > avgRangeA) || (fibB>0.0 && distB > avgRangeB)){
						Pivot[rBarP1] = Low[rBarP1] - TickSize;
						PlotBrushes[0][rBarP1] = pSellDotColor;
					}
					if(DrawThisExtension)
					{
						if(fibA>0.0 && distA > avgRangeA) 
						{	//LineName="ExtAL_"+Time[rBarP1].ToBinary().ToString();
							LineName = $"ExtAL_{(CurrentBar-rBarP1)}";
							ExtAL[j] = Low[rBarP1] - distA;
							SetTrendline(LineName, rBarP1, ExtAL[j], EndpointTime, ExtAL[j], DashStyleHelper.Solid);
						}
						if(fibB>0.0 && distB > avgRangeB) 
						{	//LineName="ExtBL_"+Time[rBarP1].ToBinary().ToString();
							LineName = $"ExtBL_{(CurrentBar-rBarP1)}";
							double eb = Low[rBarP1] - distB;
							SetTrendline(LineName, rBarP1, eb, EndpointTime, eb, DashStyleHelper.Solid);
						}
						DrawThisExtension = false;
					}
				}
			}
        }
//===================================================================
		private class LineInfo{
			public double Price;
			public int StartABar;
			public int EndABar;
			public int PlotId;
			public LineInfo(double price, int startABar, int endABar, int plotId){
				Price = price; StartABar = startABar; EndABar = endABar; PlotId = plotId;
			}
		}
		private SharpDX.Direct2D1.Brush[] LineBrushDX = new SharpDX.Direct2D1.Brush[5];
		private SharpDX.Direct2D1.Brush MagentaBrushDX = null;
		private SharpDX.Direct2D1.Brush BlackBrushDX = null;

		#region -- TargetChanged --
		public override void OnRenderTargetChanged()
		{
			if(BlackBrushDX!=null   && !BlackBrushDX.IsDisposed)    {BlackBrushDX.Dispose();   BlackBrushDX=null;}
			if(RenderTarget != null) BlackBrushDX     = Brushes.Black.ToDxBrush(RenderTarget);
			if(MagentaBrushDX!=null   && !MagentaBrushDX.IsDisposed)    {MagentaBrushDX.Dispose();   MagentaBrushDX=null;}
			if(RenderTarget != null) MagentaBrushDX     = Brushes.Magenta.ToDxBrush(RenderTarget);
			if(LineBrushDX[0]!=null   && !LineBrushDX[0].IsDisposed)    {LineBrushDX[0].Dispose();   LineBrushDX[0]=null;}
			if(RenderTarget != null) LineBrushDX[0]     = Brushes.Yellow.ToDxBrush(RenderTarget);
			if(LineBrushDX[1]!=null   && !LineBrushDX[1].IsDisposed)    {LineBrushDX[1].Dispose();   LineBrushDX[1]=null;}
			if(RenderTarget != null) LineBrushDX[1]     = Plots[1].Brush.ToDxBrush(RenderTarget);
			if(LineBrushDX[2]!=null   && !LineBrushDX[1].IsDisposed)    {LineBrushDX[2].Dispose();   LineBrushDX[2]=null;}
			if(RenderTarget != null) LineBrushDX[2]     = Plots[2].Brush.ToDxBrush(RenderTarget);
			if(LineBrushDX[3]!=null   && !LineBrushDX[2].IsDisposed)    {LineBrushDX[3].Dispose();   LineBrushDX[3]=null;}
			if(RenderTarget != null) LineBrushDX[3]     = Plots[3].Brush.ToDxBrush(RenderTarget);
			if(LineBrushDX[4]!=null   && !LineBrushDX[4].IsDisposed)    {LineBrushDX[4].Dispose();   LineBrushDX[4]=null;}
			if(RenderTarget != null) LineBrushDX[4]     = Plots[4].Brush.ToDxBrush(RenderTarget);
		}
		#endregion
		SharpDX.Vector2 v1 = new SharpDX.Vector2(0,0);
		SharpDX.Vector2 v2 = new SharpDX.Vector2(0,0);
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if(chartControl == null) return;

			int RMaB = ChartBars.ToIndex;
			int LMaB = ChartBars.FromIndex;
			var lines = L.Where(k=>k.Value.StartABar <= RMaB && k.Value.EndABar >= LMaB && k.Value.Price > chartScale.MinValue && k.Value.Price < chartScale.MaxValue);
			foreach(var line in lines){
				var id = line.Value.PlotId;
				if(Plots[id].BrushDX != null && Plots[id].BrushDX.IsValid(RenderTarget)){
					v1.X = chartControl.GetXByBarIndex(ChartBars, line.Value.StartABar);
					v2.X = chartControl.GetXByBarIndex(ChartBars, line.Value.EndABar);
					v1.Y = v2.Y = chartScale.GetYByValue(line.Value.Price);
					RenderTarget.DrawLine(v1, v2, Plots[id].BrushDX, Plots[id].Width, Plots[id].StrokeStyle);
				}else{
					Print($"Invalid BrushDX:  {line.Key}");
				}
			}
//			RemoveDrawObject("info");
			if(pRiskPerTrade > 0){
				var pRiskSummaryFont = new SimpleFont("Arial",16);
				var setups = TradeSetups.Where(k=>k.Key < MM.MouseABar && Pivot.IsValidDataPointAt(k.Key));
				if(setups==null || setups.Count()==0) return;
				var su = setups.Last();
				v1.X = chartControl.GetXByBarIndex(ChartBars, su.Key);
				v2.X = chartControl.GetXByBarIndex(ChartBars, su.Value.StopABar);
				v1.Y = chartScale.GetYByValue(su.Value.EntryPrice);
				v2.Y = chartScale.GetYByValue(su.Value.StopPrice);
				if(Plots[0].BrushDX != null && Plots[0].BrushDX.IsValid(RenderTarget))
					RenderTarget.DrawLine(v1, v2, Plots[0].BrushDX, 1);
				var dist = Math.Abs(Math.Round(Instrument.MasterInstrument.RoundToTickSize(su.Value.EntryPrice - su.Value.StopPrice) * Instrument.MasterInstrument.PointValue,0));
				var textFormat = pRiskSummaryFont.ToDirectWriteTextFormat();
				var contracts = Math.Round(pRiskPerTrade/dist-0.5,0);
				var msg = $"${pRiskPerTrade}/{dist.ToString("C0")}  {contracts}";
				var textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, msg, textFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
//				Draw.TextFixed(this,"info", $"${pRiskPerTrade}/{dist.ToString("C0")}  {Math.Round(pRiskPerTrade/dist-0.5,0)}", TextPosition.BottomRight);
				var RMx = ChartPanel.W;
				var labelRect = new SharpDX.RectangleF(RMx-textLayout.Metrics.Width-2f, ChartPanel.H-textLayout.Metrics.Height-2f, textLayout.Metrics.Width, Convert.ToSingle(pRiskSummaryFont.Size));
				RenderTarget.DrawText(msg, textFormat, labelRect, contracts == 0? MagentaBrushDX : ChartControl.Properties.AxisPen.BrushDX);
			}
		}
        #region -- MouseManager --
        private class MouseManager
        {
			public int MouseABar = -1;
            public int X = 0;
//            public int Y = 0;
		}
		private MouseManager MM = new MouseManager();
		private void OnMouseMove(object sender, MouseEventArgs e)
        {
			#region -- OnMouseMove -----------------------------------------------------------
			Point coords = e.GetPosition(ChartPanel);
			MM.X = ChartingExtensions.ConvertToHorizontalPixels(coords.X, ChartControl.PresentationSource);//+ barwidth_int;
//			MM.Y = ChartingExtensions.ConvertToVerticalPixels(coords.Y, ChartControl.PresentationSource);
			MM.MouseABar = ChartBars.GetBarIdxByX(ChartControl, MM.X);
			#endregion
		}
		#endregion

		private System.Collections.Generic.SortedDictionary<string,LineInfo> L = new System.Collections.Generic.SortedDictionary<string,LineInfo>();
		private void SetTrendline(string LineName, int StartBar, double Price1, int EndBar, double Price2, DashStyleHelper Style, bool z=false)
		{
			if(pDrawAsTrendLines) {
				if(LineName.StartsWith("ExtAH")) {
					Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[1].Brush, Style, (int)Plots[1].Width);
				}
				else if(LineName.StartsWith("ExtAL")) {
					Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[3].Brush, Style, (int)Plots[3].Width);
				}
				else if(LineName.StartsWith("ExtBH")) {
					Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[2].Brush, Style, (int)Plots[2].Width);
				}
				else if(LineName.StartsWith("ExtBL")) {
					Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[4].Brush, Style, (int)Plots[4].Width);
				}
			} else {
				if(LineName.StartsWith("ExtAH")) {
					L[LineName] = new LineInfo(Price1, CurrentBar-StartBar, CurrentBar-EndBar, 1);
				}
				else if(LineName.StartsWith("ExtAL")) {
					L[LineName] = new LineInfo(Price1, CurrentBar-StartBar, CurrentBar-EndBar, 3);
				}
				else if(LineName.StartsWith("ExtBH")) {
					L[LineName] = new LineInfo(Price1, CurrentBar-StartBar, CurrentBar-EndBar, 2);
				}
				else if(LineName.StartsWith("ExtBL")) {
					L[LineName] = new LineInfo(Price1, CurrentBar-StartBar, CurrentBar-EndBar, 4);
				}
			}
		}

//===================================================================
		private int iBarShift(int StartBar, DateTime time, bool exact) 
		{
			int i = StartBar; 

			while (Time[i].CompareTo(time) > 0 && i <= CurrentBar) 
			{
				i++;
				if(i > CurrentBar) return(-1);
			}

			if(exact)
			{ 	int c = Time[i].CompareTo(time);
				if(c != 0) return(-1);
				if(c == 0) return(i);
			}
			return(i);
		}
//===================================================================
		
        #region Plots

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Pivot
        { get { return Values[0]; } }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PivotHighFibA
        { get { return Values[1]; } }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PivotHighFibB
        { get { return Values[2]; } }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PivotLowFibA
        { get { return Values[3]; } }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PivotLowFibB
        { get { return Values[4]; } }
		#endregion

		#region -- Properties --
		[Display(Order=10, Name = "Max Risk/trade $",  GroupName = "Parameters", Description="", ResourceType = typeof(Custom.Resource))]
		public double pRiskPerTrade
		{get;set;}
		
        [Description("Max age (in bars) for valid pivots - we'll ignore any pivots older than this age")]
        [Category("Parameters")]
        public int MaxAgeOfPivots
        {
            get { return pMaxAgeOfPivots; }
            set { pMaxAgeOfPivots = Math.Max(1, value); }
        }
        [Description("Minimum strength or significance of pivots to use in calculations")]
        [Category("Parameters")]
        public int Significance
        {
            get { return pSignificance; }
            set { pSignificance = Math.Max(1, value); }
        }

		[Description("Minimum distance (in ATRs) from pivot to FibA")]
        [Category("Parameters")]
        public double MinimumATRmultToFibA
        {
            get { return pMinimumATRmultToFibA; }
            set { pMinimumATRmultToFibA = Math.Max(0.001, value); }
        }
		[Description("Minimum distance (in ATRs) from pivot to FibB")]
        [Category("Parameters")]
        public double MinimumATRmultToFibB
        {
            get { return pMinimumATRmultToFibB; }
            set { pMinimumATRmultToFibB = Math.Max(0.001, value); }
        }

		[Description("")]
        [Category("Visual")]
        public bool DrawAsTrendLines
        {
            get { return pDrawAsTrendLines; }
            set { pDrawAsTrendLines = value; }
        }

        [Description("Start bar, as measured back from the current bar")]
        [Category("Visual")]
        public int MaxBars
        {
            get { return pMaxBars; }
            set { pMaxBars = Math.Max(1, value); }
        }
		[XmlIgnore]
		[Display(Name="Buy dot", Order=110, GroupName="Custom Visual", Description="")]
		public Brush pBuyDotColor
		{get;set;}
				[Browsable(false)]
				public string BDotColorSerialize	{	get { return Serialize.BrushToString(pBuyDotColor); } set { pBuyDotColor = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Name="Sell dot", Order=120, GroupName="Custom Visual", Description="")]
		public Brush pSellDotColor
		{get;set;}
				[Browsable(false)]
				public string SDotColorSerialize	{	get { return Serialize.BrushToString(pSellDotColor); } set { pSellDotColor = Serialize.StringToBrush(value); }}

        [Description("First fibonacci extension percentage")]
        [Category("Parameters")]
        public double FibA
        {
            get { return fibA; }
            set { fibA = Math.Abs(value); }
        }

        [Description("Second fibonacci extension percentage")]
        [Category("Parameters")]
        public double FibB
        {
            get { return fibB; }
            set { fibB = Math.Abs(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Ext62Lines[] cacheExt62Lines;
		public Ext62Lines Ext62Lines()
		{
			return Ext62Lines(Input);
		}

		public Ext62Lines Ext62Lines(ISeries<double> input)
		{
			if (cacheExt62Lines != null)
				for (int idx = 0; idx < cacheExt62Lines.Length; idx++)
					if (cacheExt62Lines[idx] != null &&  cacheExt62Lines[idx].EqualsInput(input))
						return cacheExt62Lines[idx];
			return CacheIndicator<Ext62Lines>(new Ext62Lines(), input, ref cacheExt62Lines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Ext62Lines Ext62Lines()
		{
			return indicator.Ext62Lines(Input);
		}

		public Indicators.Ext62Lines Ext62Lines(ISeries<double> input )
		{
			return indicator.Ext62Lines(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Ext62Lines Ext62Lines()
		{
			return indicator.Ext62Lines(Input);
		}

		public Indicators.Ext62Lines Ext62Lines(ISeries<double> input )
		{
			return indicator.Ext62Lines(input);
		}
	}
}

#endregion
