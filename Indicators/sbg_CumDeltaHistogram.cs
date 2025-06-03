
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
using System.Windows.Controls;
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
using System.Windows.Controls;
using NinjaTrader.Gui.Chart;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum CDeltaHisto_HistoColorBasis {PosNeg, RiseFall, Trend, AboveAvgBelowAvg, ExceedPercentile}
	public class CumDeltaHistogram : Indicator
	{
		private double		buys 	= 1;
		private double 		sells 	= 1;
		private double 		cdClose	= 1;
		private Series<Double> histo;
		

		
		private bool	isReset;
		private SMA OscSmooth,TrendFilter;
		private EMA emaSlow;

		private int 	lastBar;
		private bool 	lastInTransition;
		private List<double> HistoSizes = new List<double>();
		private Brush DefaultRisingBrush = Brushes.Yellow;
		private Brush DefaultFallingBrush = Brushes.Yellow;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name										= "sbg CDelta Histo";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;

				MaximumBarsLookBack = MaximumBarsLookBack.Infinite;

//				dxB	= new Dictionary<string, DXMediaMap>();
//				foreach (string brushName in new string[] { "s" })
//					dxB.Add(brushName, new DXMediaMap());
//				BarColorDown								= Brushes.Red;
//				BarColorUp									= Brushes.LimeGreen;
//				ShadowColor									= Brushes.Black;
//				ShadowWidth									= 1;
				pMinSize = 0;
				pSMAperiod = 9;
				pHistoPercentile = 1;
				pHistoColorBasis = CDeltaHisto_HistoColorBasis.RiseFall;
				pShowEntries = false;
				pTrendFilterPeriod = 14;
				
				AddPlot(new Stroke(Brushes.Transparent),PlotStyle.Line,"Delta");
				AddPlot(new Stroke(Brushes.Lime,3f),PlotStyle.Bar,"RisingHisto");
				AddPlot(new Stroke(Brushes.Magenta,3f),PlotStyle.Bar,"FallingHisto");
				AddPlot(new Stroke(Brushes.Orange,3f),PlotStyle.Line,"SMA");
				AddPlot(new Stroke(Brushes.White,1f),PlotStyle.Line,"Threshold+");
				AddPlot(new Stroke(Brushes.White,1f),PlotStyle.Line,"Threshold-");
				AddPlot(new Stroke(Brushes.White,1f),PlotStyle.Line,"Threshold50+");
				AddPlot(new Stroke(Brushes.White,1f),PlotStyle.Line,"Threshold50-");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
			
			else if (State == State.DataLoaded)
			{
				pHistoPercentile = Math.Max(0,Math.Min(pHistoPercentile,100));
				DefaultRisingBrush = Plots[1].Brush.Clone();
				DefaultRisingBrush.Freeze();
				DefaultFallingBrush = Plots[2].Brush.Clone();
				DefaultFallingBrush.Freeze();
				histo = new Series<double>(this);
//				emaFast = EMA(delta_close, 1);
				emaSlow = EMA(DeltaClose, 10);
				if(pSMAperiod > 0)
					OscSmooth = SMA(histo, pSMAperiod);
				if(pTrendFilterPeriod > 0)
					TrendFilter = SMA(pTrendFilterPeriod);
			}
		}
	    private double RoundToSignificantDigits(double number, int digits)
	    {
	        if (number == 0) return 0; // Special case for zero
//	        int scale = (int)Math.Floor(Math.Log10((double)Math.Abs(number))) + 1;
//	        double factor = (double)Math.Pow(10, digits - scale);
			var factor = (double)Math.Pow(10, digits - 1 - (int)Math.Floor(Math.Log10((double)Math.Abs(number))));

			var r = Math.Round(number * factor);
			var res = r/factor;
//			Print($"{number}:  factor {factor}  Round(num*fact) {r}  Result: {res}");
	        return r / factor;
	    }


		HashSet<double> sortedList = null;
		double minHisto = 0;
		double thresh = 0;
		int TradeDir = 0;
		bool UpHisto1 = false;
		bool UpHisto0 = false;
		bool DownHisto1 = false;
		bool DownHisto0 = false;
		bool ThresholdExceeded = false;
		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 5 || CurrentBars[1] < 5)
				return;
			if (BarsInProgress == 0)
			{
				int indexOffset = BarsArray[1].Count - 1 - CurrentBars[1];

				if (IsFirstTickOfBar && Calculate != Calculate.OnBarClose && (State == State.Realtime || BarsArray[0].IsTickReplay))
				{
					if (CurrentBars[0] > 0)
						SetValues(1);					
					
					if (BarsArray[0].IsTickReplay || State == State.Realtime && indexOffset == 0)
						ResetValues(false,cdClose);
				}

				SetValues(0);

				if (Calculate == Calculate.OnBarClose || (lastBar != CurrentBars[0] && (State == State.Historical || State == State.Realtime && indexOffset > 0)))
					ResetValues(false,cdClose);
				
				lastBar = CurrentBars[0];
				if(IsFirstTickOfBar){
					UpHisto1 = UpHisto0;
					DownHisto1 = DownHisto0;
				}
				int c = 0;
				histo[0] = DeltaClose[0]-emaSlow[0];
				if(pHistoColorBasis != CDeltaHisto_HistoColorBasis.ExceedPercentile){
					if(IsFirstTickOfBar) HistoSizes.Add(Math.Abs(histo[1]));
					while(HistoSizes.Count>100) HistoSizes.RemoveAt(0);
					if(HistoSizes.Count>0)
						thresh = HistoSizes.Average();
					Values[4][0] = thresh;
					Values[5][0] = -Values[4][0];
				}
				if(pHistoColorBasis == CDeltaHisto_HistoColorBasis.ExceedPercentile){
					if(histo[0]>0){
						RisingHisto[0] = histo[0];
						FallingHisto.Reset();
						UpHisto0 = true;
						DownHisto0 = false;
					}else{
						FallingHisto[0] = histo[0];
						RisingHisto.Reset();
						UpHisto0 = false;
						DownHisto0 = true;
					}
					if(CurrentBars[0]>2 && IsFirstTickOfBar){
						if(HistoSizes.Count==0) HistoSizes.Add(Math.Abs(histo[1]));
						minHisto = 0;//HistoSizes.Average()/20.0;
						if(Math.Abs(histo[1]) > minHisto)
							HistoSizes.Add(RoundToSignificantDigits(Math.Abs(histo[1]),3));
						while(HistoSizes.Count>100) HistoSizes.RemoveAt(0);
						sortedList = new HashSet<double>(HistoSizes);
//						if(sortedList[0] < sortedList.Last()){
//							c = Convert.ToInt32((sortedList.Count-1) * (100-pHistoPercentile)/100.0);
//						}else{
//							c = Convert.ToInt32((sortedList.Count-1) * pHistoPercentile/100.0);
//						}
//						c = sortedList.Count-Convert.ToInt32(pHistoPercentile);
//						c = Math.Max(0,Math.Min(c, sortedList.Count-1));
						thresh = Math.Max(0,sortedList.Max() - pHistoPercentile * sortedList.Average());
						Values[4][0] = thresh;//sortedList.ElementAt(c);
						Values[5][0] = -Values[4][0];
						Values[6][0] = thresh/2;
						Values[7][0] = -Values[6][0];
					}
//					if(Math.Abs(histo[0]) > minHisto)
//						HistoSizes[HistoSizes.Count-1] = Math.Abs(histo[0]);//RoundToSignificantDigits(Math.Abs(histo[0]),3));
					if(HistoSizes.Count>12){
						//Print($"{sortedList.Min()} to {sortedList.Max()}  threshold: {sortedList.ElementAt(c)}  hash size: {sortedList.Count}");
						if(Math.Abs(histo[0]) < thresh){
							PlotBrushes[1][0] = Brushes.DimGray;
							PlotBrushes[2][0] = Brushes.DimGray;
							UpHisto0 = false;
							DownHisto0 = false;
						}else{
							PlotBrushes[1][0] = DefaultRisingBrush;
							PlotBrushes[2][0] = DefaultFallingBrush;
						}
					}
				}else if(pHistoColorBasis == CDeltaHisto_HistoColorBasis.Trend){
					if(OscSmooth[0]>OscSmooth[1]){
						RisingHisto[0] = histo[0];
						FallingHisto.Reset();
						UpHisto0 = true;
						DownHisto0 = false;
					}else{
						FallingHisto[0] = histo[0];
						RisingHisto.Reset();
						UpHisto0 = false;
						DownHisto0 = true;
					}
				}else if(pHistoColorBasis == CDeltaHisto_HistoColorBasis.PosNeg){
					if(histo[0]>0){
						RisingHisto[0] = histo[0];
						FallingHisto.Reset();
						UpHisto0 = true;
						DownHisto0 = false;
					}else{
						FallingHisto[0] = histo[0];
						RisingHisto.Reset();
						UpHisto0 = false;
						DownHisto0 = true;
					}
				}else if(pHistoColorBasis == CDeltaHisto_HistoColorBasis.RiseFall && CurrentBar>2){
					if(histo[0]>histo[1]){
						RisingHisto[0] = histo[0];
						FallingHisto.Reset();
						UpHisto0 = true;
						DownHisto0 = false;
					}else{
						FallingHisto[0] = histo[0];
						RisingHisto.Reset();
						UpHisto0 = false;
						DownHisto0 = true;
					}
				}else if(pHistoColorBasis == CDeltaHisto_HistoColorBasis.AboveAvgBelowAvg){
					if(histo[0]>OscSmooth[0]){
						RisingHisto[0] = histo[0];
						FallingHisto.Reset();
						UpHisto0 = true;
						DownHisto0 = false;
					}else{
						FallingHisto[0] = histo[0];
						RisingHisto.Reset();
						UpHisto0 = false;
						DownHisto0 = true;
					}
				}
				if(CurrentBar > 2 && pSMAperiod > 0){
					Sma[0] = OscSmooth[0];
					if(OscSmooth[0] > OscSmooth[1]) PlotBrushes[3][0] = Brushes.Green; else PlotBrushes[3][0] = Brushes.Red;
				}
				
				if(histo[0] > Values[4][0] || histo[0] < Values[5][0])
					ThresholdExceeded = true;
				if(pShowEntries){
					if(UpHisto0 && DownHisto1 && Sma[0] < 0){
						TradeDir = 1;
					}
					else if(DownHisto0 && UpHisto1 && Sma[0] > 0){
						TradeDir = -1;
					}
					if(TradeDir==1) BackBrushes[0] = Brushes.DarkGreen;
					if(TradeDir==-1) BackBrushes[0] = Brushes.Maroon;

					bool UpClose = Closes[0][0] > Opens[0][0];
					bool DownClose = Closes[0][0] < Opens[0][0];
					if(TradeDir == 1 && UpClose && ThresholdExceeded){
						if(pTrendFilterPeriod>0 && TrendFilter[0] > Closes[0][0]){
							TradeDir = 0;
							ThresholdExceeded = false;
						}else{
							Draw.ArrowUp(this,CurrentBars[0].ToString(), false,0,Lows[0][0]-TickSize, Brushes.Lime);
							TradeDir = 0;
							ThresholdExceeded = false;
						}
					}else if(TradeDir == -1 && DownClose && ThresholdExceeded){
						if(pTrendFilterPeriod>0 && TrendFilter[0] < Closes[0][0]){
							TradeDir = 0;
							ThresholdExceeded = false;
						}else{
							Draw.ArrowDown(this,CurrentBars[0].ToString(), false,0,Highs[0][0]+TickSize, Brushes.Magenta);
							TradeDir = 0;
							ThresholdExceeded = false;
						}
					}
					string  msg = pHistoColorBasis.ToString();
					string NL = Environment.NewLine;
					if(pTrendFilterPeriod>0) msg = $"Basis:  {msg}{NL}Trend Filter:  sma({pTrendFilterPeriod})"; else msg = $"Basis: {msg}{NL}Trend Filter:  OFF";
					if(ThresholdExceeded) msg = $"{msg}{NL}Threshold exeeded";
					else msg = $"{msg}{NL}Threshold NOT exeeded";
					if(TradeDir == 1) msg = $"{msg}{NL}LONG is coming";
					else if(TradeDir == -1) msg = $"{msg}{NL}SHORT is coming";
					DrawOnPricePanel = false;
					Draw.TextFixed(this,"info",msg,TextPosition.BottomLeft);
					DrawOnPricePanel = true;
				}
			}
			else if (BarsInProgress == 1)
			{
			
				if (BarsArray[1].IsFirstBarOfSession)
					ResetValues(true,cdClose);
			
				CalculateValues(false);
			}
		}

		private void CalculateValues(bool forceCurrentBar)
		{
			int 	indexOffset 	= BarsArray[1].Count - 1 - CurrentBars[1];
			bool 	inTransition 	= State == State.Realtime && indexOffset > 1;
			if (!inTransition && lastInTransition && !forceCurrentBar && Calculate == Calculate.OnBarClose)
				CalculateValues(true);
			
			bool 	useCurrentBar 	= State == State.Historical || inTransition || Calculate != Calculate.OnBarClose || forceCurrentBar;
			int 	whatBar 		= useCurrentBar ? CurrentBars[1] : Math.Min(CurrentBars[1] + 1, BarsArray[1].Count - 1);
		
			double 	volume 			= BarsArray[1].GetVolume(whatBar);
			double	price			= BarsArray[1].GetClose(whatBar);
			
			if (price >= BarsArray[1].GetAsk(whatBar) && volume>=pMinSize)
				buys += volume;	
			else if (price <= BarsArray[1].GetBid(whatBar) && volume>=pMinSize)
				sells += volume;
			
			cdClose = buys - sells;

			lastInTransition 	= inTransition;
		}
		
		private void SetValues(int barsAgo)
		{
			DeltaClose[barsAgo] = cdClose;
		}
		
		private void ResetValues(bool isNewSession, double openlevel)
		{
			cdClose = openlevel;
				
			if (isNewSession)
			{
				cdClose = 0;
			}
			isReset = true;
		}
		
		public override string DisplayName
		{
		  get { return "CDelta Histogram"; }
		}
		
		#region Miscellaneous
	
//		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
//		{
//			foreach (KeyValuePair<string, DXMediaMap> item in dxB)
//			{
//				if (item.Value.DxBrush != null)
//					item.Value.DxBrush.Opacity = item.Value.Opacity;
//			}
//			RenderTarget.DrawLine(new SharpDX.Vector2(10,10), new SharpDX.Vector2(500,500), dxB["s"].DxBrush);
//			var v = dxB["s"].DxBrush;
//			v.Opacity = 0.5f;
//			RenderTarget.DrawLine(new SharpDX.Vector2(10,500), new SharpDX.Vector2(500,10), v);

//			base.OnRender(chartControl, chartScale);
//			barPaintWidth = Math.Max(3, 1 + 2 * ((int)ChartBars.Properties.ChartStyle.BarWidth - 1) + 2 * ShadowWidth);
	

//            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
//            {
//                if (idx - Displacement < 0 || idx - Displacement >= BarsArray[0].Count || (idx - Displacement < BarsRequiredToPlot))
//                    continue;

//                x					= ChartControl.GetXByBarIndex(ChartBars, idx);
//                y1					= chartScale.GetYByValue(delta_open.GetValueAt(idx));
//                y2					= chartScale.GetYByValue(delta_high.GetValueAt(idx));
//                y3					= chartScale.GetYByValue(delta_low.GetValueAt(idx));
//                y4					= chartScale.GetYByValue(delta_close.GetValueAt(idx));

//				reuseVector1.X		= x;
//				reuseVector1.Y		= y2;
//				reuseVector2.X		= x;
//				reuseVector2.Y		= y3;

//				RenderTarget.DrawLine(reuseVector1, reuseVector2, dxB["shadowColor"].DxBrush);

//				if (y4 == y1)
//				{
//					reuseVector1.X	= (x - barPaintWidth / 2);
//					reuseVector1.Y	= y1;
//					reuseVector2.X	= (x + barPaintWidth / 2);
//					reuseVector2.Y	= y1;

//					RenderTarget.DrawLine(reuseVector1, reuseVector2, dxB["shadowColor"].DxBrush);
//				}
//				else
//				{
//					if (y4 > y1)
//					{
//						UpdateRect(ref reuseRect, (x - barPaintWidth / 2), y1, barPaintWidth, (y4 - y1));
//						RenderTarget.FillRectangle(reuseRect, dxB["barColorDown"].DxBrush);
//					}
//					else
//					{
//						UpdateRect(ref reuseRect, (x - barPaintWidth / 2), y4, barPaintWidth, (y1 - y4));
//						RenderTarget.FillRectangle(reuseRect, dxB["barColorUp"].DxBrush);
//					}

//					UpdateRect(ref reuseRect, ((x - barPaintWidth / 2) + (ShadowWidth / 2)), Math.Min(y4, y1), (barPaintWidth - ShadowWidth + 2), Math.Abs(y4 - y1));
//					RenderTarget.DrawRectangle(reuseRect, dxB["shadowColor"].DxBrush);
//				}
//            }
//		}
//		public override void OnRenderTargetChanged()
//		{		
//			try
//			{
//				foreach (KeyValuePair<string, DXMediaMap> item in dxB)
//				{
//					if (item.Value.DxBrush != null)
//						item.Value.DxBrush.Dispose();

//					if (RenderTarget != null){
//						item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);					
//						item.Value.DxBrush.Opacity = item.Value.Opacity;
//					}
//				}
//			}
//			catch (Exception exception)
//			{
//			}
//		}

//		private void UpdateRect(ref SharpDX.RectangleF updateRectangle, float x, float y, float width, float height)
//		{
//			updateRectangle.X		= x;
//			updateRectangle.Y		= y;
//			updateRectangle.Width	= width;
//			updateRectangle.Height	= height;
//		}

//		private void UpdateRect(ref SharpDX.RectangleF rectangle, int x, int y, int width, int height)
//		{
//			UpdateRect(ref rectangle, (float)x, (float)y, (float)width, (float)height);
//		}
		#endregion
		
		#region Properties
//		[Browsable(false)]
//		public class DXMediaMap
//		{
//			public SharpDX.Direct2D1.Brush		DxBrush;
//			public System.Windows.Media.Brush	MediaBrush;
//			public float Opacity=1f;
//		}
		
//		[NinjaScriptProperty]
//		[XmlIgnore]
//		[Display(Name="BarColorDown", Order=4, GroupName= "Optics")]
//		public Brush BarColorDown
//		{
//			get { return dxB["barColorDown"].MediaBrush; }
//			set { dxB["barColorDown"].MediaBrush = value; }
//		}

//				[Browsable(false)]
//				public string BarColorDownSerializable{	get { return Serialize.BrushToString(BarColorDown); }set { BarColorDown = Serialize.StringToBrush(value); }				}

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Osc Smooth period", Order=10, GroupName= "Parameters")]
		public int pSMAperiod
		{ get; set; }
		
	
		[Display(Name="Histo Color Basis", Order=20, GroupName= "Parameters")]
		public CDeltaHisto_HistoColorBasis pHistoColorBasis
		{ get; set; }
		
		[Display(Name="Size Percentile", Order=30, GroupName= "Parameters", Description="Only used if 'Histo Color Basis' is ExceedPercentile")]
		public double pHistoPercentile
		{ get;set; }

		[Display(Name="Show Entries", Order=10, GroupName= "Signals", Description="Buy when Histo changes from red to green, sell when changes from green to red")]
		public bool pShowEntries
		{ get;set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Trend Filter period", Order=20, GroupName= "Signals")]
		public int pTrendFilterPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DeltaClose
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RisingHisto
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FallingHisto
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Sma
		{
			get { return Values[3]; }
		}
	
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Size Filter", Description="Size filtering", Order=1, GroupName="Parameters")]
		public int pMinSize
		{ get; set; }

		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CumDeltaHistogram[] cacheCumDeltaHistogram;
		public CumDeltaHistogram CumDeltaHistogram(int pSMAperiod, int pTrendFilterPeriod, int pMinSize)
		{
			return CumDeltaHistogram(Input, pSMAperiod, pTrendFilterPeriod, pMinSize);
		}

		public CumDeltaHistogram CumDeltaHistogram(ISeries<double> input, int pSMAperiod, int pTrendFilterPeriod, int pMinSize)
		{
			if (cacheCumDeltaHistogram != null)
				for (int idx = 0; idx < cacheCumDeltaHistogram.Length; idx++)
					if (cacheCumDeltaHistogram[idx] != null && cacheCumDeltaHistogram[idx].pSMAperiod == pSMAperiod && cacheCumDeltaHistogram[idx].pTrendFilterPeriod == pTrendFilterPeriod && cacheCumDeltaHistogram[idx].pMinSize == pMinSize && cacheCumDeltaHistogram[idx].EqualsInput(input))
						return cacheCumDeltaHistogram[idx];
			return CacheIndicator<CumDeltaHistogram>(new CumDeltaHistogram(){ pSMAperiod = pSMAperiod, pTrendFilterPeriod = pTrendFilterPeriod, pMinSize = pMinSize }, input, ref cacheCumDeltaHistogram);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CumDeltaHistogram CumDeltaHistogram(int pSMAperiod, int pTrendFilterPeriod, int pMinSize)
		{
			return indicator.CumDeltaHistogram(Input, pSMAperiod, pTrendFilterPeriod, pMinSize);
		}

		public Indicators.CumDeltaHistogram CumDeltaHistogram(ISeries<double> input , int pSMAperiod, int pTrendFilterPeriod, int pMinSize)
		{
			return indicator.CumDeltaHistogram(input, pSMAperiod, pTrendFilterPeriod, pMinSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CumDeltaHistogram CumDeltaHistogram(int pSMAperiod, int pTrendFilterPeriod, int pMinSize)
		{
			return indicator.CumDeltaHistogram(Input, pSMAperiod, pTrendFilterPeriod, pMinSize);
		}

		public Indicators.CumDeltaHistogram CumDeltaHistogram(ISeries<double> input , int pSMAperiod, int pTrendFilterPeriod, int pMinSize)
		{
			return indicator.CumDeltaHistogram(input, pSMAperiod, pTrendFilterPeriod, pMinSize);
		}
	}
}

#endregion
