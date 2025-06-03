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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ShowDateAtBottomOfChart : Indicator
	{
		private Brush outofsessionColor;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "sbg Show DateAtBottomOfChart";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;

				FormatString = @"ddd dd-MMM";
				pDateTextColor = Brushes.Yellow;
				pWinColor	 = Brushes.Lime;
				pShowPnLBox	 = false;
				pStartTime	 = 600;
				pEndTime	 = 1600;
				pCommishPerContract = 0;
			}
			else if (State == State.Configure)
			{
				outofsessionColor = Brushes.Maroon.Clone();
				outofsessionColor.Opacity = 0.3f;
				outofsessionColor.Freeze();
			}
			else if (State == State.DataLoaded){
				if (timer == null){
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						timer			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 5), IsEnabled = true };
						timer.Tick		+= OnTimerTick;
					});
				}
			}
			else if (State == State.Terminated){
				if (timer != null){
					timer.IsEnabled = false;
					timer = null;
				}
			}
		}
//		public override string ToString(){return "";}
//		public override string DisplayName { get { return ToString(); } }
		private System.Windows.Threading.DispatcherTimer timer;
		private void OnTimerTick(object sender, EventArgs e)
		{
			sum = double.MinValue;//this will force a recalc of pnl in OnRender
			ForceRefresh();
		}
		double sum = double.MinValue;
		double sumwins = 0;
		double sumlosses = 0;
		int wins = 0;
		int losses = 0;
		List<int> ABarsInSession = new List<int>();
		List<DateTime> DatesWithTrades = new List<DateTime>();
//=======================================================================================================================
		protected override void OnBarUpdate()
		{
			var tt = ToTime(Times[0][0])/100;
			if(tt > pStartTime && tt < pEndTime) ABarsInSession.Add(CurrentBars[0]);
			else BackBrushes[0] = outofsessionColor;
		}

//=======================================================================================================================
		private SharpDX.Direct2D1.Brush TextColorBrushDX;
		private SharpDX.Direct2D1.Brush OutOfSessionBrushDX;
		public override void OnRenderTargetChanged()
		{
			if(TextColorBrushDX!=null   && !TextColorBrushDX.IsDisposed)    {TextColorBrushDX.Dispose();   TextColorBrushDX=null;}
			if(RenderTarget!=null) TextColorBrushDX = pDateTextColor.ToDxBrush(RenderTarget);

			if(OutOfSessionBrushDX!=null   && !OutOfSessionBrushDX.IsDisposed)    {OutOfSessionBrushDX.Dispose();   OutOfSessionBrushDX=null;}
			if(RenderTarget!=null) {
				OutOfSessionBrushDX = Brushes.Maroon.ToDxBrush(RenderTarget); OutOfSessionBrushDX.Opacity = 0.5f;
			}
		}
		bool RecalcSessions = false;
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
try{
			var objects = DrawObjects.Where(o=> o.ToString().Contains("VerticalLine")).ToList();
			if(objects!=null && objects.Count>0){
				try{
				foreach (dynamic dob in objects) {
					IDrawingTool o = (IDrawingTool)dob;
					var L = o.Anchors.ToArray();
					int tt = ToTime(L[0].Time)/100;
					if(o.Tag.ToUpper().Contains("START") && pStartTime!= tt) {pStartTime = tt; RecalcSessions = true;}
					if(o.Tag.ToUpper().Contains("END") && pEndTime!= tt) {pEndTime = tt; RecalcSessions = true;}
				}}catch(Exception e1){Print(this.Name+"  "+e1.ToString());}
			}
			bool IsShiftKeyDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			RulerDollars rDollars;
			Ruler rRegular;
			float x=0,y=0;
			float w = chartControl.GetXByBarIndex(ChartBars, 1) - chartControl.GetXByBarIndex(ChartBars, 0);
			double commishPts = pCommishPerContract / Instrument.MasterInstrument.PointValue;
			if(CurrentBars[0] >= BarsArray[0].Count-3 && (sum==double.MinValue || IsShiftKeyDown)) {
				if(RecalcSessions){
					RecalcSessions = false;
					ABarsInSession.Clear();
					for(int abar = 0; abar< BarsArray[0].Count; abar++){
						var tt = ToTime(Times[0].GetValueAt(abar))/100;
						if(tt > pStartTime && tt < pEndTime) {
							ABarsInSession.Add(abar);
							BackBrushes.Set(abar, null);
						}
						else BackBrushes.Set(abar, outofsessionColor);
					}
				}
				objects = DrawObjects.Where(o=> o.ToString().Contains("Ruler")).ToList();
				var RulersList = new SortedDictionary<DateTime, double>();
				if(objects!=null && objects.Count>0){
					sum = 0;
					sumwins = 0;
					sumlosses = 0;
					wins = 0;
					losses = 0;
					string pWinColorStr = pWinColor.ToString();
//Print("win color: "+pWinColorStr);
					DatesWithTrades.Clear();
					DateTime EntryDT = DateTime.MinValue;
					var AllPnL = new List<double>();
					try{
					foreach (dynamic dob in objects) {
						//Print(dob.Tag+" found");
						IDrawingTool o = (IDrawingTool)dob;//DrawObjects[dob.Tag];
						var L = o.Anchors.ToArray();
						try{
							#region -- Determine if first abar of anchor is in Session, and calc days count --
							if(L[0].Time < L[1].Time){
								if(pStartTime!=pEndTime){
									var ab = Bars.GetBar(L[0].Time);
									if(!ABarsInSession.Contains(ab)) continue;
								}
								EntryDT = L[0].Time;
								if(!DatesWithTrades.Contains(EntryDT.Date))
									DatesWithTrades.Add(EntryDT.Date);
							}
							else if(L[1].Time <= L[0].Time){
								if(pStartTime!=pEndTime){
									var ab = Bars.GetBar(L[1].Time);
									if(!ABarsInSession.Contains(ab)) continue;
								}
								EntryDT = L[1].Time;
								if(!DatesWithTrades.Contains(EntryDT.Date))
									DatesWithTrades.Add(EntryDT.Date);
							}
							#endregion
							double PnL = Math.Abs(L[0].Price-L[1].Price);
							if(o.ToString().Contains("RulerDollars")){
								rDollars = (RulerDollars)o;
								var ss = rDollars.LineColor.Pen.Brush.ToString();
								RulersList[EntryDT] = PnL - commishPts;
//Print(o.Tag+":  "+ss);
								if(!ss.Contains(pWinColorStr)) RulersList[L[0].Time] = -PnL - commishPts;
							}else{
								rRegular = (Ruler)o;
								var ss = rRegular.LineColor.Pen.Brush.ToString();
								RulersList[EntryDT] = PnL - commishPts;
//Print(o.Tag+":  "+ss);
								if(!ss.Contains(pWinColorStr)) RulersList[L[0].Time] = -PnL - commishPts;
							}
						}catch(Exception e){Print(o.ToString()+"\n"+dob.Tag+" was not recognized...skipping it\n"+e.ToString());}
//Print(o.GetType().ToString());
					}
					foreach(var trade in RulersList){
//						var tt = ToTime(trade.Key)/100;
//						if(pStartTime==pEndTime || tt>=pStartTime && tt <= pEndTime){
							Print(trade.Key.ToString()+":   "+trade.Value.ToString("C"));
							sum = sum + trade.Value;
							if(trade.Value>0){
								AllPnL.Add(trade.Value);
								sumwins += trade.Value;
								wins++;
							}else{
								AllPnL.Add(trade.Value);
								sumlosses += trade.Value;
								losses++;
							}
//						}
					}
					Print("Sum: "+sum.ToString("C"));
					Print("Wins: "+wins+"  losses: "+losses);
					Print("W%: "+(wins*1.0 / (wins+losses)).ToString("0%"));
					Print("Days: "+DatesWithTrades.Count);

					RemoveDrawObject("summary");
					if(pShowPnLBox && AllPnL.Count>0){
						AllPnL.Sort();
						double winPct = wins*1.0/(wins+losses);
						double pnlTicks = sum/TickSize;
						double avgwinTks = sumwins/wins;
						double avglossTks = Math.Abs(sumlosses/losses);
						double medianTks = AllPnL[0]/TickSize;
						if(AllPnL.Count>2) medianTks = AllPnL[AllPnL.Count/2]/TickSize;
						Draw.TextFixed(this,"summary",string.Format(
//							"Trades: {0}\nWins: {1}\nLosses: {2}\n\nWin%: {3}\nPF: {4}\nExpectancy (tks): {5}\nPnL ticks {6}\nAvg Ticks/trade: {7}",
							"Trades: {0}\n{1}: Wins ({2})\n{3}: Losses\n\n{4}: PF\n\n{5}: PnL (tks)\n{6}: Avg tks/trade\n{7}: median trade (tks)\n{8}: Days",
							(wins+losses), 
							wins,
							winPct.ToString("0%"),
							losses,
							Math.Abs(sumwins/sumlosses).ToString("0.0"),//PF
							pnlTicks.ToString("0"),
							(pnlTicks/(wins+losses)).ToString("0"),
							medianTks.ToString("0"),
							DatesWithTrades.Count),
						TextPosition.TopRight, Brushes.White, new SimpleFont("Arial",14), Brushes.Black, sum>0 ? Brushes.DarkGreen:Brushes.Red, 100);
					}
					}catch(Exception e1){Print(this.Name+"  "+e1.ToString());}
				}
			}

			var txtFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout txtLayout = null;

			if(Bars.BarsType.IsIntraday){
				SharpDX.RectangleF labelRect;
				var str = "@ 2024 NinjaTrader, LLC";
				txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);
				var taglineWidth = txtLayout.Metrics.Width*1.1f;
				str = string.Format("{0}...",DateTime.Now.ToString(FormatString));
				txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);
				y = Convert.ToSingle(ChartPanel.H - txtLayout.Metrics.Height);
				DateTime tx;
				List<int> txtbars = new List<int>(){Math.Max(5,ChartBars.FromIndex)};
				var rect = new SharpDX.RectangleF(0, 0, w, ChartPanel.H);
				float priorx = chartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex) - w/2f;
				for(int abar = Math.Max(5,ChartBars.FromIndex); abar<=ChartBars.ToIndex; abar++){
//					if(pStartTime!=pEndTime && !ABarsInSession.Contains(abar)){
//						rect.X = priorx;
//						RenderTarget.FillRectangle(rect, OutOfSessionBrushDX);
//					}
//					priorx += w;
					if(Times[0].GetValueAt(abar-1).Day!=Times[0].GetValueAt(abar).Day){
						txtbars.Insert(0,abar);//youngest dates first in the list
					}
				}
				float noprint_zone = -1f;
				foreach(var abar in txtbars){//goes through list of dates from youngest to oldest
					x = chartControl.GetXByBarIndex(ChartBars, abar);
					if(x < taglineWidth)
						labelRect = new SharpDX.RectangleF(x, y-txtLayout.FontSize*2f, txtLayout.Metrics.Width*1.5f, txtLayout.FontSize+7f);
					else
						labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width*1.5f, txtLayout.FontSize+7f);
					if(noprint_zone<0) noprint_zone = labelRect.Width;
					str = Times[0].GetValueAt(abar).ToString(FormatString);
					RenderTarget.DrawText(str, txtFormat, labelRect, TextColorBrushDX);
					if(noprint_zone>0 && x < noprint_zone) break;//stop printing dates if they are too close to the left-edge
				}
			}
			txtLayout.Dispose();
			txtFormat.Dispose();
}catch(Exception ee){}//Print(this.Name+":  "+ee.ToString());}
		}
		#region Properties

		[Display(Order=1, Name="Format String", GroupName="Date Text", Description="C# format string for date")]
		public string FormatString
		{ get; set; }

		[XmlIgnore]
		[Display(Order=10, Name="Color of date text", GroupName="Date Text", Description="")]
		public Brush pDateTextColor
		{ get; set; }
			[Browsable(false)]
			public string pColorBrush_Serialize {	get { return Serialize.BrushToString(pDateTextColor); } set { pDateTextColor = Serialize.StringToBrush(value); }}

		[Display(Order=20, Name="Show PnL box", GroupName="PnL Box", Description="")]
		public bool pShowPnLBox
		{ get; set; }

		[XmlIgnore]
		[Display(Order=30, Name="Win line color", GroupName="PnL Box", Description="Color of the Ruler line for a 'winning' trade")]
		public Brush pWinColor
		{ get; set; }
			[Browsable(false)]
			public string pWinColorBrush_Serialize {	get { return Serialize.BrushToString(pWinColor); } set { pWinColor = Serialize.StringToBrush(value); }}

		[Display(Order=40, Name="Start Time", GroupName="PnL Box", Description="")]
		public int pStartTime
		{ get; set; }
		[Display(Order=50, Name="End Time", GroupName="PnL Box", Description="")]
		public int pEndTime
		{ get; set; }
		[Display(Order=60, Name="Commish/contract", GroupName="PnL Box", Description="Dollars per contract or per share")]
		public double pCommishPerContract
		{ get; set; }

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ShowDateAtBottomOfChart[] cacheShowDateAtBottomOfChart;
		public ShowDateAtBottomOfChart ShowDateAtBottomOfChart()
		{
			return ShowDateAtBottomOfChart(Input);
		}

		public ShowDateAtBottomOfChart ShowDateAtBottomOfChart(ISeries<double> input)
		{
			if (cacheShowDateAtBottomOfChart != null)
				for (int idx = 0; idx < cacheShowDateAtBottomOfChart.Length; idx++)
					if (cacheShowDateAtBottomOfChart[idx] != null &&  cacheShowDateAtBottomOfChart[idx].EqualsInput(input))
						return cacheShowDateAtBottomOfChart[idx];
			return CacheIndicator<ShowDateAtBottomOfChart>(new ShowDateAtBottomOfChart(), input, ref cacheShowDateAtBottomOfChart);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ShowDateAtBottomOfChart ShowDateAtBottomOfChart()
		{
			return indicator.ShowDateAtBottomOfChart(Input);
		}

		public Indicators.ShowDateAtBottomOfChart ShowDateAtBottomOfChart(ISeries<double> input )
		{
			return indicator.ShowDateAtBottomOfChart(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ShowDateAtBottomOfChart ShowDateAtBottomOfChart()
		{
			return indicator.ShowDateAtBottomOfChart(Input);
		}

		public Indicators.ShowDateAtBottomOfChart ShowDateAtBottomOfChart(ISeries<double> input )
		{
			return indicator.ShowDateAtBottomOfChart(input);
		}
	}
}

#endregion
