//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum CurrentDayOHLCfibonacci_SessionTypes {Daily, Weekly, ThousandBarsRolling}
	/// <summary>
	/// Plots the open, high, and low values from the session starting on the current day.
	/// </summary>
	public class CurrentDayOHLCfibonacci : Indicator
	{
		private const int LONG = 1;
		private const int SHORT = -1;
		private DateTime			currentDate			=	Core.Globals.MinDate;
		private double				currentOpen			=	double.MinValue;
		private double				currentHigh			=	double.MinValue;
		private double				currentLow			=	double.MaxValue;
		private DateTime			lastDate			= 	Core.Globals.MinDate;
		private SessionIterator		sessionIterator;
		private int ABar_High = 0;
		private int ABar_Low = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionCurrentDayOHL;
				Name						= "CurrentDayOHLC Fibonacci";
				IsAutoScale					= false;
				DrawOnPricePanel			= false;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				ShowLow						= true;
				ShowHigh					= true;
				ShowOpen					= false;
				BarsRequiredToPlot			= 0;
				pEntryLinePct = 0.5;
				pTriggerLinePct = 0.62;
				pSessionType = CurrentDayOHLCfibonacci_SessionTypes.Daily;

				AddPlot(new Stroke(Brushes.Goldenrod,	DashStyleHelper.Dash, 2), PlotStyle.Square, Custom.Resource.CurrentDayOHLOpen);
				AddPlot(new Stroke(Brushes.SeaGreen,	DashStyleHelper.Dash, 2), PlotStyle.Square, Custom.Resource.CurrentDayOHLHigh);
				AddPlot(new Stroke(Brushes.Red,			DashStyleHelper.Dash, 2), PlotStyle.Square, Custom.Resource.CurrentDayOHLLow);
				AddPlot(new Stroke(Brushes.White,		DashStyleHelper.Solid, 5), PlotStyle.TriangleRight, "TriggerLine");
				AddPlot(new Stroke(Brushes.White,		DashStyleHelper.Solid, 2), PlotStyle.Line, "EntryLvl");
				AddPlot(new Stroke(Brushes.White,		DashStyleHelper.Solid, 2), PlotStyle.Line, "TgtLvl");
			}
			else if (State == State.Configure)
			{
				currentDate			= Core.Globals.MinDate;
				currentOpen			= double.MinValue;
				currentHigh			= double.MinValue;
				currentLow			= double.MaxValue;
				lastDate			= Core.Globals.MinDate;
			}
			else if (State == State.DataLoaded)
			{
				sessionIterator = new SessionIterator(Bars);
			}
			else if (State == State.Historical)
			{
				if (!Bars.BarsType.IsIntraday)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", Custom.Resource.CurrentDayOHLError, TextPosition.BottomRight);
					Log(Custom.Resource.CurrentDayOHLError, LogLevel.Error);
				}
			}
		}
		SortedDictionary<int,double> Maxs = new SortedDictionary<int,double>();
		SortedDictionary<int,double> Mins = new SortedDictionary<int,double>();
		private double SmartMax(int abar){
			while(Maxs.Count>0 && Maxs.Keys.Min() < abar) Maxs.Remove(Maxs.Keys.Min());
			if(CurrentBars[0] > 3){
				if(Highs[0][0] < Highs[0][1]) Maxs[CurrentBars[0]-1] = Highs[0][1];
			}
			return (Maxs.Count == 0 ? Highs[0][0] : Maxs.Values.Max());
		}
		private double SmartMin(int abar){
			while(Mins.Count>0 && Mins.Keys.Min() < abar) Mins.Remove(Mins.Keys.Min());
			if(CurrentBars[0] > 3){
				if(Lows[0][0] > Lows[0][1]) Mins[CurrentBars[0]-1] = Lows[0][1];
			}
			return (Mins.Count == 0 ? Lows[0][0] : Mins.Values.Min());
		}

		bool IsValidNow = false;
		bool IsTriggeredLong = false;
		bool IsTriggeredShort = false;
		int AlertABar = 0;
		double FiftyLine = 0.0;
		double Sixty8Line = 0.0;
		int SignalId = 0;
		int Trend = 0;
		protected override void OnBarUpdate()
		{
			if(CurrentBars[0] < 2) return;
			if (!Bars.BarsType.IsIntraday) return;

			lastDate 		= currentDate;
			currentDate 	= sessionIterator.GetTradingDay(Time[0]);
			
			if(pSessionType == CurrentDayOHLCfibonacci_SessionTypes.Daily){
				if (lastDate != currentDate || currentOpen == double.MinValue)
				{
					currentOpen		= Open[0];
					currentHigh		= High[0];
					currentLow		= Low[0];
					IsValidNow = true;
				}
			}
			else if(pSessionType == CurrentDayOHLCfibonacci_SessionTypes.Weekly){
				if ((int)Times[0][0].DayOfWeek < (int)Times[0][1].DayOfWeek || currentOpen == double.MinValue)
				{
					currentOpen		= Open[0];
					currentHigh		= High[0];
					currentLow		= Low[0];
					IsValidNow = true;
				}
			}
			else if(pSessionType == CurrentDayOHLCfibonacci_SessionTypes.ThousandBarsRolling){
				if (currentOpen == double.MinValue)
				{
					currentOpen		= Open[0];
					currentHigh		= High[0];
					currentLow		= Low[0];
				}else if(IsFirstTickOfBar){
					if(CurrentBars[0] > 1000) IsValidNow = true;
					int bar0 = Math.Max(1, CurrentBars[0] - 1000);
					currentHigh = SmartMax(bar0);
					currentLow = SmartMin(bar0);
				}
			}

			if(IsValidNow && CurrentBar>22){
				if(High[0] > currentHigh){
					ABar_High = CurrentBar;
					currentHigh = High[0];
				}
				if(Low[0] < currentLow){
					ABar_Low = CurrentBar;
					currentLow = Low[0];
				}
				if(ABar_High > ABar_Low && Trend <= 0){
					RemoveDrawObject("AlertTxt");
					Trend = LONG;
				}
				else if(ABar_High < ABar_Low && Trend >= 0){
					RemoveDrawObject("AlertTxt");
					Trend = SHORT;
				}
				//var z = Times[0][0].Day==31 && Times[0][0].Hour==7 && Times[0][0].Minute>32;
				if(Trend == LONG){
					FiftyLine = currentHigh - (currentHigh - currentLow) * pEntryLinePct;
					Sixty8Line = currentHigh - (currentHigh - currentLow) * pTriggerLinePct;
					if(!IsTriggeredLong && Low[0] > FiftyLine) SignalId = CurrentBars[0];
					PlotBrushes[3][0] = Brushes.Cyan;

//					if(High[1] > Sixty8Line && Low[0] <= Sixty8Line) IsTriggeredLong = true;
					if(Low[0] <= Sixty8Line) IsTriggeredLong = true;
					if(IsTriggeredLong){
						IsTriggeredShort = false;
						EntryLvl[2] = FiftyLine;
						EntryLvl[1] = FiftyLine;
						EntryLvl[0] = FiftyLine;
						PlotBrushes[4][0] = Brushes.Lime;
						TgtLvl[0] = EntryLvl[0] + Math.Abs(FiftyLine-Sixty8Line) * 3;
						TgtLvl[1] = TgtLvl[0];
						TgtLvl[2] = TgtLvl[0];
						Draw.Line(this, "50%", 20, FiftyLine, 0, FiftyLine, Brushes.Green);
						Draw.TextFixed(this, "AlertTxt",$"{Times[0][0].ToString()}  Trigger active...prepare to buy at "+Instrument.MasterInstrument.FormatPrice(FiftyLine),TextPosition.Center);
						if(High[0] >= FiftyLine){
							//Draw.Ellipse(this, $"TriggerHit{SignalId}", false, 10, Sixty8Line, 0, FiftyLine, Brushes.Green, Brushes.Lime, 20);
							IsTriggeredLong = false;
							RemoveDrawObject("AlertTxt");
							AlertABar = CurrentBar;
							Alert(CurrentBar.ToString(), Priority.High, "Buy on CDOHLCFib at "+Instrument.MasterInstrument.FormatPrice(FiftyLine), "Alert2.Wav",1,Brushes.Green,Brushes.White);
						}
					}
				}
				else if(Trend == SHORT){
					FiftyLine = currentLow + (currentHigh - currentLow) * pEntryLinePct;
					Sixty8Line = currentLow + (currentHigh - currentLow) * pTriggerLinePct;
					if(!IsTriggeredShort && High[0] < FiftyLine) SignalId = CurrentBars[0];
					PlotBrushes[3][0] = Brushes.Yellow;

//					if(High[1] >= Sixty8Line && Low[0] < Sixty8Line) IsTriggeredShort = true;
					if(High[1] >= Sixty8Line) IsTriggeredShort = true;
					if(IsTriggeredShort){
						IsTriggeredLong = false;
						EntryLvl[2] = FiftyLine;
						EntryLvl[1] = FiftyLine;
						EntryLvl[0] = FiftyLine;
						PlotBrushes[4][0] = Brushes.Magenta;
						TgtLvl[0] = EntryLvl[0] - Math.Abs(FiftyLine-Sixty8Line) * 3;
						TgtLvl[1] = TgtLvl[0];
						TgtLvl[2] = TgtLvl[0];
						Draw.Line(this, "50%", 20, FiftyLine, 0, FiftyLine, Brushes.Red);
						Draw.TextFixed(this, "AlertTxt",$"{Times[0][0].ToString()}  Trigger active...prepare to sell at "+Instrument.MasterInstrument.FormatPrice(FiftyLine),TextPosition.Center);
						if(Low[0] <= FiftyLine){
							//Draw.Ellipse(this, $"TriggerHit{SignalId}", false, 10, Sixty8Line, 0, FiftyLine, Brushes.Red, Brushes.Pink, 20);
							IsTriggeredShort = false;
							RemoveDrawObject("AlertTxt");
							AlertABar = CurrentBar;
							Alert(CurrentBar.ToString(), Priority.High, "Sell on CDOHLCFib at "+Instrument.MasterInstrument.FormatPrice(FiftyLine), "Alert2.Wav",1,Brushes.Maroon,Brushes.White);
						}
					}
				}
				if(!IsTriggeredLong && !IsTriggeredShort) RemoveDrawObject("AlertTxt");
				TriggerLvl[0] = Sixty8Line;
				Draw.Line(this, "62%", 20, Sixty8Line, 0, Sixty8Line, Brushes.White);
				if(AlertABar < CurrentBar - 20){
					RemoveDrawObject("50%");
				}

			}

			if (ShowOpen)
				CurrentOpen[0] = currentOpen;

			if (ShowHigh)
				CurrentHigh[0] = currentHigh;

			if (ShowLow)
				CurrentLow[0] = currentLow;
		}

		private SharpDX.Direct2D1.Brush TriggerLineBrushDX, BuyEntryLineBrushDX, SellEntryLineBrushDX, textBrushDX;
		public override void OnRenderTargetChanged()
		{
			#region == OnRenderTargetChanged ==
			if(TriggerLineBrushDX!=null   && !TriggerLineBrushDX.IsDisposed)    {TriggerLineBrushDX.Dispose(); TriggerLineBrushDX=null;}
			if(RenderTarget != null) {TriggerLineBrushDX     = Brushes.Yellow.ToDxBrush(RenderTarget);  TriggerLineBrushDX.Opacity = 0.5f;}
			if(textBrushDX!=null   && !textBrushDX.IsDisposed)    {textBrushDX.Dispose(); textBrushDX=null;}
			if(RenderTarget != null) {textBrushDX     = Brushes.Black.ToDxBrush(RenderTarget);}
			if(BuyEntryLineBrushDX!=null   && !BuyEntryLineBrushDX.IsDisposed)    {BuyEntryLineBrushDX.Dispose(); BuyEntryLineBrushDX=null;}
			if(RenderTarget != null) {BuyEntryLineBrushDX     = Brushes.Lime.ToDxBrush(RenderTarget);}
			if(SellEntryLineBrushDX!=null   && !SellEntryLineBrushDX.IsDisposed)    {SellEntryLineBrushDX.Dispose(); SellEntryLineBrushDX=null;}
			if(RenderTarget != null) {SellEntryLineBrushDX     = Brushes.Lime.ToDxBrush(RenderTarget);}
			#endregion
		}

		SharpDX.Vector2 v1 = new SharpDX.Vector2(0,0);
		SharpDX.Vector2 v2 = new SharpDX.Vector2(0,0);
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
            var textFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
			var textLayout	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, "Trigger Line" , textFormat, (float)(ChartPanel.X + ChartPanel.W), 12f);

			var x = chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]) + 10f;
			v2.X = x + textLayout.Metrics.Width + 10f;
			var y = chartScale.GetYByValue(Sixty8Line) - 5f;
			v1.X = x;
			v1.Y = v2.Y = y;
			RenderTarget.DrawLine(v1, v2, TriggerLineBrushDX, 20f);
			v1.X = x + 5f;
			v1.Y = y - textFormat.FontSize / 2f;
			RenderTarget.DrawTextLayout(v1, textLayout, textBrushDX);

			y = chartScale.GetYByValue(FiftyLine) - 5f;
			v1.Y = v2.Y = y;
			if(FiftyLine > Sixty8Line)
				RenderTarget.DrawLine(v1, v2, BuyEntryLineBrushDX, textLayout.Metrics.Height);
			else
				RenderTarget.DrawLine(v1, v2, SellEntryLineBrushDX, textLayout.Metrics.Height);
		}
		#region --Plots--
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentLow
		{
			get { return Values[2]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> TriggerLvl
		{
			get { return Values[3]; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> EntryLvl
		{
			get { return Values[4]; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> TgtLvl
		{
			get { return Values[5]; }
		}
		#endregion

		#region -- Properties --
		[Display(Name = "Show High", GroupName = "NinjaScriptParameters", Order = 10, ResourceType = typeof(Custom.Resource))]
		public bool ShowHigh
		{ get; set; }

		[Display(Name = "Show Low", GroupName = "NinjaScriptParameters", Order = 20, ResourceType = typeof(Custom.Resource))]
		public bool ShowLow
		{ get; set; }

		[Display(Name = "Show Open", GroupName = "NinjaScriptParameters", Order = 30, ResourceType = typeof(Custom.Resource))]
		public bool ShowOpen
		{ get; set; }
		
		[Display(Name = "Trigger Pct", GroupName = "NinjaScriptParameters", Order = 40, ResourceType = typeof(Custom.Resource))]
		public double pTriggerLinePct
		{get;set;}

		[Display(Name = "Entry Pct", GroupName = "NinjaScriptParameters", Order = 50, ResourceType = typeof(Custom.Resource))]
		public double pEntryLinePct
		{get;set;}

		[Display(Name = "Session type", GroupName = "NinjaScriptParameters", Order = 60, ResourceType = typeof(Custom.Resource))]
		public CurrentDayOHLCfibonacci_SessionTypes pSessionType
		{get;set;}

		#endregion
		
		public override string FormatPriceMarker(double price)
		{
			return Instrument.MasterInstrument.FormatPrice(price);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CurrentDayOHLCfibonacci[] cacheCurrentDayOHLCfibonacci;
		public CurrentDayOHLCfibonacci CurrentDayOHLCfibonacci()
		{
			return CurrentDayOHLCfibonacci(Input);
		}

		public CurrentDayOHLCfibonacci CurrentDayOHLCfibonacci(ISeries<double> input)
		{
			if (cacheCurrentDayOHLCfibonacci != null)
				for (int idx = 0; idx < cacheCurrentDayOHLCfibonacci.Length; idx++)
					if (cacheCurrentDayOHLCfibonacci[idx] != null &&  cacheCurrentDayOHLCfibonacci[idx].EqualsInput(input))
						return cacheCurrentDayOHLCfibonacci[idx];
			return CacheIndicator<CurrentDayOHLCfibonacci>(new CurrentDayOHLCfibonacci(), input, ref cacheCurrentDayOHLCfibonacci);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CurrentDayOHLCfibonacci CurrentDayOHLCfibonacci()
		{
			return indicator.CurrentDayOHLCfibonacci(Input);
		}

		public Indicators.CurrentDayOHLCfibonacci CurrentDayOHLCfibonacci(ISeries<double> input )
		{
			return indicator.CurrentDayOHLCfibonacci(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CurrentDayOHLCfibonacci CurrentDayOHLCfibonacci()
		{
			return indicator.CurrentDayOHLCfibonacci(Input);
		}

		public Indicators.CurrentDayOHLCfibonacci CurrentDayOHLCfibonacci(ISeries<double> input )
		{
			return indicator.CurrentDayOHLCfibonacci(input);
		}
	}
}

#endregion
