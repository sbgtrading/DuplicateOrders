
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
//using NinjaTrader.NinjaScript.Indicators.ARC.Sup;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Gui.CategoryOrder("Input Parameters", 10)]
	[Gui.CategoryOrder("ATR Band Parameters", 15)]
	[Gui.CategoryOrder("Display Options", 20)]
	[Gui.CategoryOrder("Data Series", 30)]
	[Gui.CategoryOrder("Set up", 40)]
	[Gui.CategoryOrder("Visual", 50)]
	[Gui.CategoryOrder("Plots", 60)]
	[Gui.CategoryOrder("System Signals", 70)]
	[Gui.CategoryOrder("Sound Alerts", 80)]
	[Gui.CategoryOrder("Version", 90)]
	public class ATRTrailingStopSystem : Indicator
	{

		#region Variables
        private int 						rangePeriod					= 10; 
		private int							displacement				= 0;
		private int							totalBarsRequiredToPlot		= 0;
		private int							shift						= 0;
		private bool 						showTriangles 				= false;
		private bool 						showPaintBars 				= false;
		private bool						showStopDots				= true;
		private bool 						showStopLine				= true;
		private bool						soundAlerts					= false;
		private bool 						gap0						= false;
		private bool 						gap1						= false;
		private bool						stoppedOut					= false;
		private bool						drawTriangleUp				= false;
		private bool						drawTriangleDown			= false;
		private bool						calculateFromPriceData		= true;
		private bool						indicatorIsOnPricePanel		= true;
		private double 						offset						= 0.0;
		private double						trailingAmount				= 0.0;
		private double						low							= 0.0;
		private double						high						= 0.0;
		private double						labelOffset					= 0.0;
		private SessionIterator				sessionIterator				= null;
		private PlotStyle 					plot0Style					= PlotStyle.Dot;
		private int 						plot1Width 					= 1;
		private PlotStyle 					plot1Style					= PlotStyle.Line;
		private Brush						upBrush						= Brushes.Cyan;
		private Brush						downBrush					= Brushes.Red;
		private Brush						upBrushUp					= Brushes.Cyan;
		private Brush						upBrushDown					= Brushes.LightSkyBlue;
		private Brush						downBrushUp					= Brushes.LightCoral;
		private Brush						downBrushDown				= Brushes.Red;
		private Brush						upBrushOutline				= Brushes.Black;
		private Brush						downBrushOutline			= Brushes.Black;
		private Brush						alertBackBrush				= Brushes.Black;
		private Brush						errorBrush					= Brushes.Black;
		private SimpleFont					dotFont						= null;
		private SimpleFont					triangleFont				= null;
		private SimpleFont					errorFont					= null;
		private int							triangleFontSize			= 10;
		private string						dotString					= "n";
		private string						triangleStringUp			= "5";
		private string						triangleStringDown			= "6";
		private string						errorText					= "The 'ATR TrailingStop System' cannot be used with a negative displacement.";
		private int							rearmTime					= 10;
		private const string				versionString				= "v 1.0";
		private Series<double>				preliminaryTrend;
		private Series<double>				trend;
		private Series<double>				currentStopLong;
		private Series<double>				currentStopShort;
		private ISeries<double>				offsetSeries;
		private ATR							barVolatility;
		private double BandATR = 0;
		private int AlertABar = 0;
		private double ContractMultiple = 1;
		#endregion

		Brush SellSig;
		Brush BuySig;
		int rsiDir = 0;
		int FLAT = 0;
		int SHORT = -1;
		int LONG = 1;
		RSI rsi;
		private bool ResultsPrinted = false;
		private class EntryExit{
			public char Direction = ' ';
			public double EntryPrice = 0;
			public DateTime EntryTime;
			public double ExitPrice = 0;
			public int ExitABar = 0;
			public double NetPts = 0;
			public EntryExit (char direction, DateTime entryTime, double entryP) { Direction = direction; EntryTime = entryTime; EntryPrice = entryP; }
		}
		private SortedDictionary<int, EntryExit> Trades = new SortedDictionary<int, EntryExit>();

		protected override void OnStateChange()
		{
if(State!=null) Print("State: "+State.ToString());
			#region OnStateChange
			if (State == State.SetDefaults)
			{
				Name						= "ATR TrailingStop System";
				Calculate					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				pBand1Mult = 1.0;
				pBand2Mult = 2;
				pBand3Mult = 3.0;
				pRoundBandToNearestTick = false;
				pBandATRPeriod = 14;
				pBand1Enabled = true;
				pBand1Brush   = Brushes.Cyan;
				pBand1Opacity = 20;
				pBand2Enabled = false;
				pBand2Brush   = Brushes.Orange;
				pBand2Opacity = 20;
				pBand3Enabled = false;
				pBand3Brush   = Brushes.Red;
				pBand3Opacity = 20;
				pSignalOpacity = 20;
				pStartTime = new TimeSpan(9,30,0);
				pEndTime = new TimeSpan(15,55,0);
				pRSIperiod = 10;

				AddPlot(new Stroke(Brushes.Gray, 2),   PlotStyle.Dot, "StopDot");	
				AddPlot(new Stroke(Brushes.Gray, 2),   PlotStyle.Line, "StopLine");
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Dot, "ReverseDot");
				AddPlot(new Stroke(Brushes.Cyan, 1),   PlotStyle.Line, "BandEdge U3");
				AddPlot(new Stroke(Brushes.Cyan, 1),   PlotStyle.Line, "BandEdge U2");
				AddPlot(new Stroke(Brushes.Cyan, 1),   PlotStyle.Line, "BandEdge U1");
				AddPlot(new Stroke(Brushes.Maroon, 1), PlotStyle.Line, "BandEdge L1");
				AddPlot(new Stroke(Brushes.Maroon, 1), PlotStyle.Line, "BandEdge L2");
				AddPlot(new Stroke(Brushes.Maroon, 1), PlotStyle.Line, "BandEdge L3");

				newUptrend					= "Silent";
				newDowntrend				= "Silent";
				potentialUptrend			= "Silent";
				potentialDowntrend			= "Silent";
			}
			else if (State == State.Configure)
			{
				displacement = Displacement;
				BarsRequiredToPlot = 2*rangePeriod;
				totalBarsRequiredToPlot = BarsRequiredToPlot + displacement;
//				Plots[0].PlotStyle = plot0Style;
//				Plots[0].Width = plot0Width;
//				Plots[1].PlotStyle = plot1Style;
//				Plots[1].Width = plot1Width;
//				Plots[2].PlotStyle = plot0Style;
//				Plots[2].Width = plot0Width;
			}
			else if (State == State.DataLoaded)
			{
				SellSig = Brushes.Red.Clone();
				SellSig.Opacity = pSignalOpacity/100.0;
				SellSig.Freeze();
				BuySig = Brushes.Lime.Clone();
				BuySig.Opacity = pSignalOpacity/100.0;
				BuySig.Freeze();
				rsi = RSI(pRSIperiod,1);
				
				if(Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency) ContractMultiple = 100;
				if(Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock) ContractMultiple = 100;
				if(Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex) ContractMultiple = 100000;

				preliminaryTrend = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				currentStopLong = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				currentStopShort = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
//				if(useModifiedATR)
//					offsetSeries = ARC_ATRModified(Inputs[0], ARC_ATRModCalcMode.Wilder, rangePeriod);
//				else
					offsetSeries = ATR(Inputs[0], rangePeriod);
				barVolatility = ATR(Closes[0], 256);
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
		    	sessionIterator = new SessionIterator(Bars);
			}	
			else if (State == State.Historical)
			{
				if(displacement < 0)
				{
					if(ChartBars != null)
					{	
						errorBrush = ChartControl.Properties.AxisPen.Brush;
						errorBrush.Freeze();
						errorFont = new SimpleFont("Arial", 24);
						indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
						DrawOnPricePanel = false;
						Draw.TextFixed(this, "error text", errorText, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					}	
					return;
				}
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;		
				if(Calculate == Calculate.OnBarClose)// && !reverseIntraBar)
					shift = displacement + 1;
				else
					shift = displacement;
				gap0 = (plot0Style == PlotStyle.Line)||(plot0Style == PlotStyle.Square);
				gap1 = (plot1Style == PlotStyle.Line)||(plot1Style == PlotStyle.Square);
				dotFont = new SimpleFont("Webdings", plot1Width + 2);
				triangleFont = new SimpleFont("Webdings", 3*triangleFontSize);
			}
			#endregion
		}

		private int RegionID = 0;
		private List<int> StopLineHitDotIDs = new List<int>();
		protected override void OnBarUpdate()
        {
			if(IsFirstTickOfBar && CurrentBar>pBandATRPeriod){
				double sum = 0;
				for(int i = 1; i<=pBandATRPeriod; i++) sum = sum + Range()[i];
				BandATR = sum/pBandATRPeriod;
				if(pRoundBandToNearestTick) BandATR = Instrument.MasterInstrument.RoundToTickSize(BandATR);
			}
			if(displacement < 0)
				return;

			bool StopDotPrinted = false;
			if (CurrentBar < 2)
			{ 
				preliminaryTrend[0] = 1.0;
				trend[0] = LONG;
				StopDot[0] = Input[0];
				StopLine[0] = Input[0];
				ReverseDot[0] = Input[0]; 
				PlotBrushes[0][0] = Brushes.Transparent;
				PlotBrushes[1][0] = Brushes.Transparent;
				PlotBrushes[2][0] = Brushes.Transparent;
				return; 
			}
			if (IsFirstTickOfBar)
			{
				if(pDrawDotOnStopLineHit && Lows[0][1] <= StopLine[1] && Highs[0][1] >= StopLine[1]){
					//Draw.Dot(this,$"ATSSDot0{CurrentBars[0]}",false, 2, Instrument.MasterInstrument.RoundToTickSize(Values[5][1]), true,"Red");
					//Draw.Dot(this,$"ATSSDot1{CurrentBars[0]}",false, 2, Instrument.MasterInstrument.RoundToTickSize(Values[6][1]),true,"Red");
					var sline = Instrument.MasterInstrument.RoundToTickSize(StopLine[1]);
					if(trend[1] < FLAT)
						Draw.Dot(this,$"ATSSDot2{CurrentBars[0]}",false, 2, sline, true, "Green");
					else
						Draw.Dot(this,$"ATSSDot2{CurrentBars[0]}",false, 2, sline, true, "Red");
					StopLineHitDotIDs.Add(CurrentBars[0]);
					while(Times[0][0].DayOfWeek >= DayOfWeek.Monday && Times[0][0].DayOfWeek <= DayOfWeek.Friday && Times[0].GetValueAt(StopLineHitDotIDs[0]).Date!=Times[0][0].Date && StopLineHitDotIDs.Count>20){
						RemoveDrawObject($"ATSSDot2{StopLineHitDotIDs[0]}");
						StopLineHitDotIDs.RemoveAt(0);
					}
					//Draw.Line(this,$"ATSSDotLine{CurrentBars[0]}",false, 2, sline, 1, sline, true, "Green");
				}
				
				offset = Math.Max(TickSize, offsetSeries[1]);
				trailingAmount = multiplier * offset;
				labelOffset = 0.3 * barVolatility[1];
				if(preliminaryTrend[1] > 0.5)
				{
					if (calculateFromPriceData)
					{
						currentStopLong[0] = Math.Max(currentStopLong[1], Math.Min(Input[1] - trailingAmount, Input[1] - TickSize));
						currentStopShort[0] = Input[1] + trailingAmount;
					}
					else
					{
						currentStopLong[0] = Math.Max(currentStopLong[1], Input[1] - trailingAmount);
						currentStopShort[0] = Input[1] + trailingAmount;
					}	
					StopDot[0] = currentStopLong[0];
					StopLine[0] = currentStopLong[0];
//					ReverseDot[0] = currentStopShort[0];
					if(showStopDots)
					{	
//						if(gap0 && preliminaryTrend[2] < -0.5)
//							PlotBrushes[0][0]= Brushes.Transparent;
//						else
						if(Lows[0][0] <= StopLine[0]){
							PlotBrushes[0][0] = upBrush;
							StopDotPrinted = true;
						}else
							PlotBrushes[0][0]= Brushes.Transparent;
					}
					else
						PlotBrushes[0][0]= Brushes.Transparent;
					if(showStopLine)
					{	
						if(gap1 && preliminaryTrend[2] < -0.5)
							PlotBrushes[1][0]= Brushes.Transparent;
						else
							PlotBrushes[1][0] = upBrush;
					}
					else
						PlotBrushes[1][0]= Brushes.Transparent;
				}
				else	
				{	
					if (calculateFromPriceData)
					{
						currentStopShort[0] = Math.Min(currentStopShort[1], Math.Max(Input[1] + trailingAmount, Input[1] + TickSize));
						currentStopLong[0] = Input[1] - trailingAmount;
					}
					else
					{
						currentStopShort[0] = Math.Min(currentStopShort[1], Input[1] + trailingAmount);
						currentStopLong[0] = Input[1] - trailingAmount;
					}
					StopDot[0] = currentStopShort[0];
					StopLine[0] = currentStopShort[0];
//					ReverseDot[0] = currentStopLong[0];
					if(showStopDots)
					{	
//						if(gap0 && preliminaryTrend[2] > 0.5)
//							PlotBrushes[0][0]= Brushes.Transparent;
//						else
						if(Highs[0][0] >= StopLine[0]){
							PlotBrushes[0][0] = downBrush;
							StopDotPrinted = true;
						}else
							PlotBrushes[0][0]= Brushes.Transparent;
					}
					else
						PlotBrushes[0][0]= Brushes.Transparent;
					if(showStopLine)
					{	
						if(gap1 && preliminaryTrend[2] > 0.5)
							PlotBrushes[1][0]= Brushes.Transparent;
						else
							PlotBrushes[1][0] = downBrush;
					}
					else
						PlotBrushes[1][0]= Brushes.Transparent;
				}
				
				if(showStopLine && CurrentBar >= BarsRequiredToPlot)
				{	
					DrawOnPricePanel = false;
//					if(plot1Style == PlotStyle.Line && reverseIntraBar) 
//					{
//						if(preliminaryTrend[1] > 0.5 && preliminaryTrend[2] < -0.5)
//							Draw.Line(this, "line" + CurrentBar, false, 1-displacement, ReverseDot[1], -displacement, StopLine[0], upBrush, DashStyleHelper.Solid, plot1Width);
//						else if(preliminaryTrend[1] < -0.5 && preliminaryTrend[2] > 0.5)
//							Draw.Line(this, "line" + CurrentBar, false, 1-displacement, ReverseDot[1], -displacement, StopLine[0], downBrush, DashStyleHelper.Solid, plot1Width);
//					}
//					else 
					if(plot1Style == PlotStyle.Square && !showStopDots) 
					{
						if(trend[1] > 0.5 && trend[2] < -0.5)
							Draw.Text(this, "dot" + CurrentBar, false, dotString, -displacement, StopLine[0], 0 , upBrush, dotFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						else if(trend[1] < -0.5 && trend[2] > 0.5)
							Draw.Text(this, "dot" + CurrentBar, false, dotString, -displacement, StopLine[0], 0 , downBrush, dotFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
					}
				}
				
				if(showPaintBars && CurrentBar >= BarsRequiredToPlot)
				{
					if (preliminaryTrend[1] > 0.5)
					{	
						if(Open[0] < Close[0])
							BarBrushes[-displacement] = upBrushUp;
						else
							BarBrushes[-displacement] = upBrushDown;
						CandleOutlineBrushes[-displacement] = upBrushOutline;
					}	
					else
					{	
						if(Open[0] < Close[0])
							BarBrushes[-displacement] = downBrushUp;
						else
							BarBrushes[-displacement] = downBrushDown;
						CandleOutlineBrushes[-displacement] = downBrushOutline;
					}
				}
				stoppedOut = false;
			}
			
//			if(reverseIntraBar) // only one trend change per bar is permitted
//			{
//				if(!stoppedOut)
//				{
//					if (preliminaryTrend[1] > 0.5 && Low[0] < currentStopLong[0])
//					{
//						preliminaryTrend[0] = -1.0;
//						stoppedOut = true;
//						if(showStopDots && !gap0)
//							PlotBrushes[2][0] = downBrush;
//					}	
//					else if (preliminaryTrend[1] < -0.5 && High[0] > currentStopShort[0])
//					{
//						preliminaryTrend[0] = 1.0;
//						stoppedOut = true;
//						if(showStopDots && !gap0)
//							PlotBrushes[2][0] = upBrush;
//					}
//					else
//						preliminaryTrend[0] = preliminaryTrend[1];
//				}
//			}
//			else 
			{
				if (preliminaryTrend[1] > 0.5 && Input[0] < currentStopLong[0])
					preliminaryTrend[0] = SHORT;
				else if (preliminaryTrend[1] < -0.5 && Input[0] > currentStopShort[0])
					preliminaryTrend[0] = LONG;
				else
					preliminaryTrend[0] = preliminaryTrend[1];
			}
			// this information can be accessed by a strategy - trend holds the confirmed trend, whereas preliminaryTrend may hold a preliminary trend only
			if(Calculate == Calculate.OnBarClose)
				trend[0] = preliminaryTrend[0];
			else if(IsFirstTickOfBar)// && !reverseIntraBar)
				trend[0] = preliminaryTrend[1];
//			else if(reverseIntraBar)
//				trend[0] = preliminaryTrend[0];
			if(trend[2] != trend[1]){
				RegionID = CurrentBars[0];
			}

			if(CurrentBar > totalBarsRequiredToPlot+3 && RegionID>0)
			{
				var newtrend = trend[0]!=trend[1];
				var starttime = BarsArray[0].GetTime(RegionID-1);
				var stoptime = BarsArray[0].GetTime(CurrentBars[0] - (newtrend ? 1:0));
				if(pBand1Enabled && pBand1Mult>0){
					var bandpts = BandATR * pBand1Mult;
					if(pShowOppositeBands){
						BandEdgeU1[0] = StopLine[0] + bandpts;
						BandEdgeL1[0] = StopLine[0] - bandpts;
						if(pBand1Opacity>0 && pBand1Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
							Draw.Region(this, string.Format("R1{0}",RegionID),starttime, stoptime, BandEdgeU1, BandEdgeL1, Brushes.Transparent, pBand1Brush, pBand1Opacity);
						}
					}else{
						if(trend[0]>0){
							BandEdgeU1[0] = StopLine[0] + bandpts;
							if(pBand1Opacity>0 && pBand1Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
								Draw.Region(this, string.Format("R1u{0}",RegionID),starttime, stoptime, BandEdgeU1, StopLine, Brushes.Transparent, pBand1Brush, pBand1Opacity);
							}
						}else{
							BandEdgeL1[0] = StopLine[0] - bandpts;
							if(pBand1Opacity>0 && pBand1Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
								Draw.Region(this, string.Format("R1L{0}",RegionID),starttime, stoptime, BandEdgeL1, StopLine, Brushes.Transparent, pBand1Brush, pBand1Opacity);
							}
						}
					}
					if(newtrend) {BandEdgeU1.Reset(1);BandEdgeL1.Reset(1);}
				}
				if(pBand2Enabled && pBand2Mult>0){
					var bandpts = BandATR * pBand2Mult;
					if(pShowOppositeBands){
						BandEdgeU2[0] = StopLine[0] + bandpts;
						BandEdgeL2[0] = StopLine[0] - bandpts;
						if(pBand2Opacity>0 && pBand2Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
							if(pBand1Enabled){
								Draw.Region(this, string.Format("R2u{0}",RegionID),starttime, stoptime, BandEdgeU2, BandEdgeU1, Brushes.Transparent, pBand2Brush, pBand2Opacity);
								Draw.Region(this, string.Format("R2L{0}",RegionID),starttime, stoptime, BandEdgeL2, BandEdgeL1, Brushes.Transparent, pBand2Brush, pBand2Opacity);
							}else{
								Draw.Region(this, string.Format("R2{0}",RegionID),starttime, stoptime, BandEdgeU2, BandEdgeL2, Brushes.Transparent, pBand2Brush, pBand2Opacity);
							}
						}
					}else{
						if(trend[0]>0){
							BandEdgeU2[0] = StopLine[0] + bandpts;
							if(pBand2Opacity>0 && pBand2Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
								if(pBand1Enabled){
									Draw.Region(this, string.Format("R2u{0}",RegionID),starttime, stoptime, BandEdgeU2, BandEdgeU1, Brushes.Transparent, pBand2Brush, pBand2Opacity);
								}else{
									Draw.Region(this, string.Format("R2{0}",RegionID),starttime, stoptime, BandEdgeU2, StopLine, Brushes.Transparent, pBand2Brush, pBand2Opacity);
								}
							}
						}else{
							BandEdgeL2[0] = StopLine[0] - bandpts;
							if(pBand2Opacity>0 && pBand2Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
								if(pBand1Enabled){
									Draw.Region(this, string.Format("R2L{0}",RegionID),starttime, stoptime, BandEdgeL2, BandEdgeL1, Brushes.Transparent, pBand2Brush, pBand2Opacity);
								}else{
									Draw.Region(this, string.Format("R2{0}",RegionID),starttime, stoptime, BandEdgeL2, StopLine, Brushes.Transparent, pBand2Brush, pBand2Opacity);
								}
							}
						}
					}
					if(newtrend) {BandEdgeU2.Reset(1);BandEdgeL2.Reset(1);}
				}
				if(pBand3Enabled && pBand3Mult>0){
					var bandpts = BandATR * pBand3Mult;
					if(pShowOppositeBands){
						BandEdgeU3[0] = StopLine[0] + bandpts;
						BandEdgeL3[0] = StopLine[0] - bandpts;
						if(pBand3Opacity>0 && pBand3Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
							if(pBand1Enabled && pBand2Enabled){
								Draw.Region(this, string.Format("R3u{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeU2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								Draw.Region(this, string.Format("R3L{0}",RegionID),starttime, stoptime, BandEdgeL3, BandEdgeL2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
							}else if(pBand1Enabled && !pBand2Enabled){
								Draw.Region(this, string.Format("R3u{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeU1, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								Draw.Region(this, string.Format("R3L{0}",RegionID),starttime, stoptime, BandEdgeL3, BandEdgeL1, Brushes.Transparent, pBand3Brush, pBand3Opacity);
							}else if(!pBand1Enabled && pBand2Enabled){
								Draw.Region(this, string.Format("R3u{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeU2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								Draw.Region(this, string.Format("R3L{0}",RegionID),starttime, stoptime, BandEdgeL3, BandEdgeL2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
							}else{
								Draw.Region(this, string.Format("R3{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeL3, Brushes.Transparent, pBand3Brush, pBand3Opacity);
							}
						}
					}else{
						if(trend[0]>0){
							BandEdgeU3[0] = StopLine[0] + bandpts;
							if(pBand3Opacity>0 && pBand3Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
								if(pBand1Enabled && pBand2Enabled){
									Draw.Region(this, string.Format("R3u{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeU2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}else if(pBand1Enabled && !pBand2Enabled){
									Draw.Region(this, string.Format("R3u{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeU1, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}else if(!pBand1Enabled && pBand2Enabled){
									Draw.Region(this, string.Format("R3u{0}",RegionID),starttime, stoptime, BandEdgeU3, BandEdgeU2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}else{
									Draw.Region(this, string.Format("R3{0}",RegionID),starttime, stoptime, BandEdgeU3, StopLine, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}
							}
						}else{
							BandEdgeL3[0] = StopLine[0] - bandpts;
							if(pBand3Opacity>0 && pBand3Brush !=Brushes.Transparent && RegionID != CurrentBars[0]){
								if(pBand1Enabled && pBand2Enabled){
									Draw.Region(this, string.Format("R3L{0}",RegionID),starttime, stoptime, BandEdgeL3, BandEdgeL2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}else if(pBand1Enabled && !pBand2Enabled){
									Draw.Region(this, string.Format("R3L{0}",RegionID),starttime, stoptime, BandEdgeL3, BandEdgeL1, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}else if(!pBand1Enabled && pBand2Enabled){
									Draw.Region(this, string.Format("R3L{0}",RegionID),starttime, stoptime, BandEdgeL3, BandEdgeL2, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}else{
									Draw.Region(this, string.Format("R3{0}",RegionID),starttime, stoptime, StopLine, BandEdgeL3, Brushes.Transparent, pBand3Brush, pBand3Opacity);
								}
							}
						}
					}
					if(newtrend) {BandEdgeU3.Reset(1);BandEdgeL3.Reset(1);}
				}
				if(pBand3Enabled){
					var bandpts = BandATR * pBand3Mult;
					BandEdgeU3[0] = StopLine[0] + bandpts;
					BandEdgeL3[0] = StopLine[0] - bandpts;
				}
				if(showPaintBars)
				{
					if(trend[shift] > 0)
					{
						if(Open[0] < Close[0])
							BarBrushes[0] = upBrushUp;
						else
							BarBrushes[0] = upBrushDown;
						CandleOutlineBrushes[0] = upBrushOutline;
					}
					else
					{	
						if(Open[0] < Close[0])
							BarBrushes[0] = downBrushUp;
						else
							BarBrushes[0] = downBrushDown;
						CandleOutlineBrushes[0] = downBrushOutline;
					}
				}
				if(showTriangles)
				{
					DrawOnPricePanel = true;
					if(Calculate == Calculate.OnBarClose)
					{	
						if(trend[displacement] > 0.5 && trend[displacement+1] < -0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringUp, 0, Low[0] - labelOffset, -triangleFontSize, upBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						else if(trend[displacement] < -0.5 && trend[displacement+1] > 0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringDown, 0, High[0] + labelOffset, triangleFontSize, downBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
					else if (IsFirstTickOfBar)	
					{
						if(trend[displacement] > 0.5 && trend[displacement+1] < -0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringUp, 1, Low[1] - labelOffset, -triangleFontSize, upBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0); 
						else if(trend[displacement] < -0.5 && trend[displacement+1] > 0.5)
							Draw.Text(this, "triangle" + CurrentBar, false, triangleStringDown, 1, High[1] + labelOffset, triangleFontSize, downBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}	
				}	
			}
			var pDrawPnLLine = false;
			#region -- RSI signal --
			var InSession = (pStartTime == pEndTime ||
							(pStartTime < pEndTime && Times[0][0].TimeOfDay >= pStartTime && Times[0][0].TimeOfDay < pEndTime) ||
							(pStartTime > pEndTime && (Times[0][0].TimeOfDay >= pStartTime || Times[0][0].TimeOfDay < pEndTime)));
			var IsAfterSession = Times[0][0].TimeOfDay >= pEndTime;
			if(rsiDir != FLAT)
				ReverseDot[0] = StopDot[0];
			if(rsiDir==LONG && StopDot[1] > High[1] && StopDot[0] <= High[0]) {
				BackBrushes[1] = BuySig;
				ReverseDot.Reset(0);
				rsiDir = FLAT;
				foreach(var kvp in Trades.Where(k=>k.Value.ExitABar == 0 && k.Value.Direction == 'S')){
					kvp.Value.ExitABar = CurrentBars[0];
					kvp.Value.ExitPrice = Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]);
					kvp.Value.NetPts = kvp.Value.EntryPrice - kvp.Value.ExitPrice;
					if(pDrawPnLLine) Draw.Line(this, $"{kvp.Key} {kvp.Value.Direction}", false, kvp.Value.EntryTime, kvp.Value.EntryPrice, Times[0][0], kvp.Value.ExitPrice, (kvp.Value.NetPts > 0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash,2);
//					Print(Times[0][0].ToString()+"  SHORT from "+kvp.Value.EntryPrice+", exit at "+Closes[0][0]);
				}
				if(InSession)
					Trades[CurrentBars[0]] = new EntryExit('L', Times[0][0], Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]));
//				Print(Times[0][0].ToString()+"  Long from "+Trades[CurrentBars[0]].EntryPrice);
			}
			if(rsiDir==SHORT && StopDot[1] < Low[1] && StopDot[0] >= Low[0]) {
				BackBrushes[1] = SellSig;
				ReverseDot.Reset(0);
				rsiDir = FLAT;
				foreach(var kvp in Trades.Where(k=>k.Value.ExitABar == 0 && k.Value.Direction == 'L')){
					kvp.Value.ExitABar = CurrentBars[0];
					kvp.Value.ExitPrice = Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]);
					kvp.Value.NetPts = kvp.Value.ExitPrice - kvp.Value.EntryPrice;
					if(pDrawPnLLine) Draw.Line(this, $"{kvp.Key} {kvp.Value.Direction}", false, kvp.Value.EntryTime, kvp.Value.EntryPrice, Times[0][0], kvp.Value.ExitPrice, (kvp.Value.NetPts > 0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash,2);
//					Print(Times[0][0].ToString()+"  LONG from "+kvp.Value.EntryPrice+", exit at "+Closes[0][0]);
				}
				if(InSession)
					Trades[CurrentBars[0]] = new EntryExit('S', Times[0][0], Instrument.MasterInstrument.RoundToTickSize(Closes[0][0]));
				//Print(Times[0][0].ToString()+"  Short from "+Trades[CurrentBars[0]].EntryPrice);
			}
			if(rsiDir == FLAT) {
				if(rsi[0] > 75 && StopDot[1] < Low[1])  rsiDir = SHORT;
				if(rsi[0] < 25 && StopDot[1] > High[1]) rsiDir = LONG;
			}else if(pCancelAt50){
				if(rsi[0]<=50 && rsi[1]>50) rsiDir=FLAT;
				if(rsi[0]>=50 && rsi[1]<50) rsiDir=FLAT;
			}
			
			if(Times[0][1].Day !=Times[0][0].Day || (!InSession && IsAfterSession)){//end of day, close out all trades
				foreach(var kvp in Trades.Where(k=>k.Value.ExitABar == 0)){
					kvp.Value.ExitABar = CurrentBars[0] - 1;
					kvp.Value.ExitPrice = Instrument.MasterInstrument.RoundToTickSize(Closes[0][1]);
					kvp.Value.NetPts = kvp.Value.Direction == 'L' ? kvp.Value.ExitPrice - kvp.Value.EntryPrice : kvp.Value.EntryPrice - kvp.Value.ExitPrice;
					if(pDrawPnLLine) Draw.Line(this, $"{kvp.Key} {kvp.Value.Direction}", false, kvp.Value.EntryTime, kvp.Value.EntryPrice, Times[0][1], kvp.Value.ExitPrice, (kvp.Value.NetPts > 0 ? Brushes.Lime:Brushes.Magenta), DashStyleHelper.Dash,2);
				}
			}
			if(CurrentBars[0] > Bars.Count - 3 && !ResultsPrinted){
				ResultsPrinted = true;
				var name = Instruments[0].MasterInstrument.Name;
				double PV = Instrument.MasterInstrument.PointValue;
				double overallPts = 0;
				double overallwins = 0;
				double overalllosses = 0;
				double overalldaycount = 0;
				var resultsDOWstr = new List<string>();
				foreach(DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek))){
					var sumPts = 0.0;
					var wins = 0;
					var losses = 0;
					var minutesTotal = 0.0;
					var lossPts = 0.0;
					var winPts = 0.0;
					var TimeTable = new SortedDictionary<int,List<double>>();

					int dowcount = 0;
					//if(Times[0][0].DayOfWeek == dow)
					{
						dowcount = Trades.Values
						.Where(v => v.EntryTime.DayOfWeek == dow)
						.Select(v => v.EntryTime.Date)
						.Distinct()
						.Count();
					}
					overalldaycount += dowcount;
					Print($"\n{name} on "+dow.ToString()+"   "+(dowcount>0 ? dowcount.ToString():""));
					if(ContractMultiple>0) Print("   Contract multiplier: "+ ContractMultiple);
					var maes = new List<double>();
					foreach (var trades in Trades.Where(k=>k.Value.EntryTime.DayOfWeek == dow && k.Value.ExitABar>0)){
						var t = trades.Value;
						double NetPts = t.NetPts * ContractMultiple;
						var tint = t.EntryTime.Hour *100 + (t.EntryTime.Minute / 60.0 > 0.5 ? 30:0);
						if(!TimeTable.ContainsKey(tint))
							TimeTable[tint] = new List<double>() {NetPts};
						else
							TimeTable[tint].Add(NetPts);

						sumPts += NetPts;
						overallPts += NetPts;
						if(NetPts>0){
							winPts += NetPts;
							overallwins += 1;
							wins += NetPts > 0 ? 1 : 0;
						}else{
							lossPts += NetPts;
							overalllosses += 1;
							losses += NetPts <= 0 ? 1 : 0;
						}
						var ticks = Times[0].GetValueAt(t.ExitABar).Ticks - t.EntryTime.Ticks;
						minutesTotal += ticks/TimeSpan.TicksPerMinute;

						#region Calculate MAEs
						if(pCalculateMAE){
							var maePrice = t.EntryPrice;
							for(int bar = trades.Key + 1; bar <= t.ExitABar; bar++){
								if(t.Direction == 'L'){
									maePrice = Math.Min(maePrice, Lows[0].GetValueAt(bar));
								}else{
									maePrice = Math.Max(maePrice, Highs[0].GetValueAt(bar));
								}
							}
							maes.Add(Math.Abs(maePrice - t.EntryPrice));
						}
						#endregion
					}
					if(wins+losses>0){
						var avgPts = sumPts / (wins+losses);
						var s = Instrument.MasterInstrument.FormatPrice(avgPts);
						Print($"  {s} avg pts/trade ({(avgPts / TickSize).ToString("0")}-ticks) = {(avgPts * PV).ToString("C0")}");
						s = (wins*1.0 / (wins+losses)).ToString("0%");
						Print($"  {wins} wins and {losses} losses   {s}");
						Print($"  {((winPts/wins) * PV).ToString("C0")} Avg win  {((lossPts / losses) * PV).ToString("C0")} Avg loss");
						s = (minutesTotal/(wins+losses)).ToString("0");
						Print($"  {s} avg mins per trade");
						#region -- Calc/Print MAE --
						if(pCalculateMAE){
							maes.Sort();
							var avg = maes.Average();
							var variance = 0.0;
							foreach(var v in maes)
								variance += Math.Pow(v-avg,2);
							Print($"  MAE median: {(PV*maes[maes.Count/2]).ToString("C0")}  avg: {(PV*avg).ToString("C0")}  stddev: {(PV*Math.Sqrt(variance/maes.Count)).ToString("C0")}");
						}
						#endregion
						int winsToday = 0;
						int lossesToday = 0;
						if(true || Times[0][0].DayOfWeek == dow){
							winPts = 0; lossPts = 0; wins = 0; losses = 0;
							var netPts = 0.0;
							foreach(var tkvp in TimeTable){
								wins = tkvp.Value.Count(k=> k>0);
								losses = tkvp.Value.Count(k=> k<=0);
								winsToday += wins;
								lossesToday += losses;
								winPts = tkvp.Value.Where(k=> k>0).Sum();
								lossPts = tkvp.Value.Where(k=> k<=0).Sum();
								netPts += winPts + lossPts;
								if(wins+losses>0){
									Print($"   {tkvp.Key}  {wins}|{losses} {(wins*1.0/(wins+losses)).ToString("0%")}   {(PV*(winPts+lossPts)).ToString("C0")}");
								}
							}
							resultsDOWstr.Add($"   {dow.ToString().Substring(0,2)}    Net: {(PV * netPts).ToString("C0")}   {(netPts * PV / dowcount).ToString("C0")}/day   {(netPts * PV / (winsToday+lossesToday)).ToString("C0")}/trade ");
							Print(resultsDOWstr.Last());
						}
					}else{
						Print("no trades taken");
					}
				}
				Print("\n");
				foreach(var sss in resultsDOWstr)
					Print(sss);
				Print($"\n  Overall Net: {(PV * overallPts).ToString("C0")}  {(overallwins / (overallwins+overalllosses)).ToString("0%")}  {(overallPts * PV / overalldaycount).ToString("C0")}/day");
				CalculateEquityCurveAndDrawdown();
				Trades.Clear();
			}
			#endregion

			if (soundAlerts && State == State.Realtime && IsConnected() && AlertABar!=CurrentBar){
				if(Calculate == Calculate.OnBarClose)// || reverseIntraBar))
				{
					if(preliminaryTrend[0] > 0.5 && preliminaryTrend[1] < -0.5 && !newUptrend.Contains("Silent"))		
					{
						Alert("New_Uptrend", Priority.Medium,"New Uptrend", AddSoundFolder(newUptrend), rearmTime, alertBackBrush, upBrush);
						AlertABar = CurrentBar;
					}
					else if(preliminaryTrend[0] < -0.5 && preliminaryTrend[1] > 0.5 && !newDowntrend.Contains("Silent"))
					{
						Alert("New_Downtrend", Priority.Medium,"New Downtrend", AddSoundFolder(newDowntrend), rearmTime, alertBackBrush, downBrush);
						AlertABar = CurrentBar;
					}
				}				
				else if(StopDotPrinted || Calculate != Calculate.OnBarClose)// && !reverseIntraBar)
				{
					if(preliminaryTrend[0] > 0.5 && preliminaryTrend[1] < -0.5 && !potentialUptrend.Contains("Silent"))
					{
						Alert("Potential_Uptrend", Priority.Medium,"Uptrend stopdot hit", AddSoundFolder(potentialUptrend), rearmTime, alertBackBrush, upBrush);
						AlertABar = CurrentBar;
					}
					else if(preliminaryTrend[0] < -0.5 && preliminaryTrend[1] > 0.5 && !potentialDowntrend.Contains("Silent"))
					{
						Alert("Potential_Downtrend", Priority.Medium,"Downtrend stopdot hit", AddSoundFolder(potentialDowntrend), rearmTime, alertBackBrush, downBrush);
						AlertABar = CurrentBar;
					}
				}
			}	
		}
		private void CalculateEquityCurveAndDrawdown(){
			double MaxEquity = 0;
			double CurrentEquity = 0;
			double DrawdownValue = 0;
			var DDs = new List<double>();
			int tradecount = 0;
			foreach(var kvp in Trades){
				CurrentEquity += kvp.Value.NetPts * Instrument.MasterInstrument.PointValue;
				//Print(CurrentEquity+"   kvp.Value.NetPts: "+kvp.Value.NetPts);
				if(CurrentEquity > MaxEquity && DrawdownValue > 0){
					//Print($"{MaxEquity.ToString("C")}  Drawdown was: {DrawdownValue:0} in {tradecount}-trades  {Bars.GetTime(kvp.Key).ToString()}");
					DDs.Add(Math.Round(DrawdownValue,0));
					DrawdownValue = 0;
					tradecount = 0;
				}else{
					tradecount++;
					var dd = Math.Abs(MaxEquity - CurrentEquity);
					if(dd > DrawdownValue){
						DrawdownValue = dd;
//						Print($"{kvp.Key}  New drawdown: "+DrawdownValue);
					}
				}
				MaxEquity = Math.Max(MaxEquity, CurrentEquity);
			}
			if(DDs.Count>1)
				DDs.Sort();
			else if(DDs.Count==0 && DrawdownValue != 0)
				DDs.Add(DrawdownValue);
			foreach(var dr in DDs){Print($"Drawdown: ${dr}");}
			if(DDs.Count>0)
				Print($"Drawdown as % of MaxEquity:  {(DDs.Max() / MaxEquity).ToString("0%")}");
			else
				Print($"Drawdown was zero");
		}
		private string AddSoundFolder(string wav){
			if(Instruments.Length>1)
				wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst2>",Instruments[1].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			if(Instruments.Length>0)
				wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
		}
		#region Plots

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StopDot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StopLine
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ReverseDot
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BandEdgeU3
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BandEdgeU2
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BandEdgeU1
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BandEdgeL1
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BandEdgeL2
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BandEdgeL3
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return trend; }
		}
		#endregion

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR period", Description = "Sets the lookback period for the average true range", GroupName = "Input Parameters", Order = 0)]
		public int RangePeriod
		{	
            get { return rangePeriod; }
            set { rangePeriod = value; }
		}

		private double multiplier = 2.5;
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR multiplier", Description = "Sets the multiplier for the average true range", GroupName = "Input Parameters", Order = 1)]
		public double Multiplier
		{	
            get { return multiplier; }
            set { multiplier = value; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Order = 5, Name = "Band ATR Period", Description = "Lookback period for the ATR for band calculation", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public int pBandATRPeriod
		{get;set;}

		[Display(Order = 6, Name = "Round to tick?", Description = "Round the band levels to the nearest tick?", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pRoundBandToNearestTick
		{get;set;}

		[Display(Order = 7, Name = "Band 1 Enabled", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pBand1Enabled
		{get;set;}

		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(Order = 10, Name = "Band 1 multiplier", Description = "Sets the multiplier for the average true range", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public double pBand1Mult
		{get;set;}	

		[XmlIgnore]
		[Display(Order = 11, Name = "Band 1 color", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public Brush pBand1Brush {get;set;}
				[Browsable(false)]
				public string pBand1BrushSerializable{get { return Serialize.BrushToString(pBand1Brush); }set { pBand1Brush = Serialize.StringToBrush(value); }}					
		[Range(0, 100)]
		[Display(Order = 12, Name = "Band 1 opacity", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public int pBand1Opacity
		{get;set;}

		[Display(Order = 15, Name = "Band 2 Enabled", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pBand2Enabled
		{get;set;}
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(Order = 20, Name = "Band 2 multiplier", Description = "Sets the multiplier for the average true range", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public double pBand2Mult
		{get;set;}

		[XmlIgnore]
		[Display(Order = 21, Name = "Band 2 color", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public Brush pBand2Brush {get;set;}
				[Browsable(false)]
				public string pBand2BrushSerializable{get { return Serialize.BrushToString(pBand2Brush); }set { pBand2Brush = Serialize.StringToBrush(value); }}					
		[Range(0, 100)]
		[Display(Order = 22, Name = "Band 2 opacity", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public int pBand2Opacity
		{get;set;}

		[Display(Order = 25, Name = "Band 3 Enabled", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pBand3Enabled
		{get;set;}
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(Order = 30, Name = "Band 3 multiplier", Description = "Sets the multiplier for the average true range", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public double pBand3Mult
		{get;set;}

		[XmlIgnore]
		[Display(Order = 31, Name = "Band 3 color", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public Brush pBand3Brush {get;set;}
				[Browsable(false)]
				public string pBand3BrushSerializable{get { return Serialize.BrushToString(pBand3Brush); }set { pBand3Brush = Serialize.StringToBrush(value); }}					
		[Range(0, 100)]
		[Display(Order = 32, Name = "Band 3 opacity", Description = "", GroupName = "ATR Band Parameters", ResourceType = typeof(Custom.Resource))]
		public int pBand3Opacity
		{get;set;}

		private bool pShowOppositeBands = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show opposite band", GroupName = "Display Options", Order = 10)]
        public bool ShowOppositeBands
        {
            get { return pShowOppositeBands; }
            set { pShowOppositeBands = value; }
        }
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show stop dots", GroupName = "Display Options", Order = 20)]
        public bool ShowStopDots
        {
            get { return showStopDots; }
            set { showStopDots = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show stop line", GroupName = "Display Options", Order = 30)]
        public bool ShowStopLine
        {
            get { return showStopLine; }
            set { showStopLine = value; }
        }
		
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show triangles", GroupName = "Display Options", Order = 40)]
        public bool ShowTriangles
        {
            get { return showTriangles; }
            set { showTriangles = value; }
        }

		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish color", Description = "Sets the color for the trailing stop", GroupName = "Plots", Order = 0)]
		public Brush UpBrush
		{ 
			get {return upBrush;}
			set {upBrush = value;}
		}
				[Browsable(false)]
				public string UpBrushSerializable{get { return Serialize.BrushToString(upBrush); }set { upBrush = Serialize.StringToBrush(value); }}					
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish color", Description = "Sets the color for the trailing stop", GroupName = "Plots", Order = 1)]
		public Brush DownBrush
		{ 
			get {return downBrush;}
			set {downBrush = value;}
		}

		[Browsable(false)]
		public string DownBrushSerializable
		{
			get { return Serialize.BrushToString(downBrush); }
			set { downBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plotstyle stop dots", Description = "Sets the plot style for the stop dots", GroupName = "Plots", Order = 2)]
		public PlotStyle Plot0Style
		{	
            get { return plot0Style; }
            set { plot0Style = value; }
		}
		
		private int plot0Width = 6;
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dot size", Description = "Sets the size for the stop dots", GroupName = "Plots", Order = 3)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plotstyle stop line", Description = "Sets the plot style for the stop line", GroupName = "Plots", Order = 4)]
		public PlotStyle Plot1Style
		{	
            get { return plot1Style; }
            set { plot1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Width stop line", Description = "Sets the width for the stop line", GroupName = "Plots", Order = 5)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
		
		[Range(1, 256)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Triangle size", Description = "Allows for adjusting the triangle size", GroupName = "Plots", Order = 6)]
		public int TriangleFontSize
		{	
            get { return triangleFontSize; }
            set { triangleFontSize = value; }
		}

		[Range(0, 100)]
		[Display(Name = "Signal Stripe Opacity", Description = "", GroupName = "System Signals", Order = 10, ResourceType = typeof(Custom.Resource))]
		public int pSignalOpacity
		{get;set;}
		
		[Display(Name = "RSI Period", GroupName = "System Signals", Order = 15, ResourceType = typeof(Custom.Resource))]
		public int pRSIperiod
		{get;set;}
		
		[Display(Name = "Start Time", GroupName = "System Signals", Order = 20, ResourceType = typeof(Custom.Resource))]
		public TimeSpan pStartTime
		{get;set;}

		[Display(Name = "End Time", GroupName = "System Signals", Order = 30, ResourceType = typeof(Custom.Resource))]
		public TimeSpan pEndTime
		{get;set;}
		
		[Display(Name = "Cancel at RSI50", Description = "Cancel a signal when RSI crosses 50 level", GroupName = "System Signals", Order = 35, ResourceType = typeof(Custom.Resource))]
		public bool pCancelAt50
		{get;set;}

		[Display(Name = "Calculate MAE?", GroupName = "System Signals", Order = 40, Description = "This can slow-down calculation speed", ResourceType = typeof(Custom.Resource))]
		public bool pCalculateMAE
		{get;set;}
		
		[Display(Name = "Global dot on stopline hit?", GroupName = "System Signals", Order = 45, Description = "When price hits StopLine, a globalized dot is drawn", ResourceType = typeof(Custom.Resource))]
		public bool pDrawDotOnStopLineHit
		{get;set;}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enable Sound alerts?", GroupName = "Sound Alerts", Order = 0)]
        public bool SoundAlerts
        {
            get { return soundAlerts; }
            set { soundAlerts = value; }
        }
		
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "New uptrend", Description = "Sound file for confirmed new uptrend", GroupName = "Sound Alerts", Order = 1)]
        public string newUptrend { get; set; }
		
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "New downtrend", Description = "Sound file for confirmed new downtrend", GroupName = "Sound Alerts", Order = 2)]
        public string newDowntrend { get; set; }
		
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Uptrend stopdot", Description = "Sound file when uptrend stopdot hit", GroupName = "Sound Alerts", Order = 3)]
        public string potentialUptrend { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Downtrend stopdot", Description = "Sound file when downtrend stopdot hit", GroupName = "Sound Alerts", Order = 4)]
        public string potentialDowntrend { get; set; }

		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rearm time", Description = "Rearm time for alerts in seconds", GroupName = "Sound Alerts", Order = 5)]
		public int RearmTime
		{	
            get { return rearmTime; }
            set { rearmTime = value; }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release#", Description = "", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
			set { ;}
		}
		#endregion

		#region Miscellaneous
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

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("Silent");
				list.Add("<inst>_BuyEntry.wav");
				list.Add("<inst>_SellEntry.wav");
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
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

		public override string FormatPriceMarker(double price)
		{
			if(indicatorIsOnPricePanel)
				return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
			else
				return base.FormatPriceMarker(price);
		}
		
		private bool IsConnected()
        {
			if ( Bars != null && Bars.Instrument.GetMarketDataConnection().PriceStatus == NinjaTrader.Cbi.ConnectionStatus.Connected
					&& sessionIterator.IsInSession(Now, true, true))
				return true;
			else
            	return false;
        }
		
		private DateTime Now
		{
          get 
			{ 
				DateTime now = (Bars.Instrument.GetMarketDataConnection().Options.Provider == NinjaTrader.Cbi.Provider.Playback ? Bars.Instrument.GetMarketDataConnection().Now : DateTime.Now); 

				if (now.Millisecond > 0)
					now = NinjaTrader.Core.Globals.MinDate.AddSeconds((long) System.Math.Floor(now.Subtract(NinjaTrader.Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATRTrailingStopSystem[] cacheATRTrailingStopSystem;
		public ATRTrailingStopSystem ATRTrailingStopSystem(int rangePeriod, double multiplier, int pBandATRPeriod, double pBand1Mult, double pBand2Mult, double pBand3Mult)
		{
			return ATRTrailingStopSystem(Input, rangePeriod, multiplier, pBandATRPeriod, pBand1Mult, pBand2Mult, pBand3Mult);
		}

		public ATRTrailingStopSystem ATRTrailingStopSystem(ISeries<double> input, int rangePeriod, double multiplier, int pBandATRPeriod, double pBand1Mult, double pBand2Mult, double pBand3Mult)
		{
			if (cacheATRTrailingStopSystem != null)
				for (int idx = 0; idx < cacheATRTrailingStopSystem.Length; idx++)
					if (cacheATRTrailingStopSystem[idx] != null && cacheATRTrailingStopSystem[idx].RangePeriod == rangePeriod && cacheATRTrailingStopSystem[idx].Multiplier == multiplier && cacheATRTrailingStopSystem[idx].pBandATRPeriod == pBandATRPeriod && cacheATRTrailingStopSystem[idx].pBand1Mult == pBand1Mult && cacheATRTrailingStopSystem[idx].pBand2Mult == pBand2Mult && cacheATRTrailingStopSystem[idx].pBand3Mult == pBand3Mult && cacheATRTrailingStopSystem[idx].EqualsInput(input))
						return cacheATRTrailingStopSystem[idx];
			return CacheIndicator<ATRTrailingStopSystem>(new ATRTrailingStopSystem(){ RangePeriod = rangePeriod, Multiplier = multiplier, pBandATRPeriod = pBandATRPeriod, pBand1Mult = pBand1Mult, pBand2Mult = pBand2Mult, pBand3Mult = pBand3Mult }, input, ref cacheATRTrailingStopSystem);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATRTrailingStopSystem ATRTrailingStopSystem(int rangePeriod, double multiplier, int pBandATRPeriod, double pBand1Mult, double pBand2Mult, double pBand3Mult)
		{
			return indicator.ATRTrailingStopSystem(Input, rangePeriod, multiplier, pBandATRPeriod, pBand1Mult, pBand2Mult, pBand3Mult);
		}

		public Indicators.ATRTrailingStopSystem ATRTrailingStopSystem(ISeries<double> input , int rangePeriod, double multiplier, int pBandATRPeriod, double pBand1Mult, double pBand2Mult, double pBand3Mult)
		{
			return indicator.ATRTrailingStopSystem(input, rangePeriod, multiplier, pBandATRPeriod, pBand1Mult, pBand2Mult, pBand3Mult);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATRTrailingStopSystem ATRTrailingStopSystem(int rangePeriod, double multiplier, int pBandATRPeriod, double pBand1Mult, double pBand2Mult, double pBand3Mult)
		{
			return indicator.ATRTrailingStopSystem(Input, rangePeriod, multiplier, pBandATRPeriod, pBand1Mult, pBand2Mult, pBand3Mult);
		}

		public Indicators.ATRTrailingStopSystem ATRTrailingStopSystem(ISeries<double> input , int rangePeriod, double multiplier, int pBandATRPeriod, double pBand1Mult, double pBand2Mult, double pBand3Mult)
		{
			return indicator.ATRTrailingStopSystem(input, rangePeriod, multiplier, pBandATRPeriod, pBand1Mult, pBand2Mult, pBand3Mult);
		}
	}
}

#endregion
