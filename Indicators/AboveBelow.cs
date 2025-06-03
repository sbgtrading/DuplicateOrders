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
	public class AboveBelow : Indicator
	{
		private List<int> time_keys = new List<int>();
		private double MAX_PCT = 0.02;
		private double MIN_PCT = -0.02;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AboveBelow";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				pStartTime					= 930;
				pEndTime					= 1600;
				pFont = new SimpleFont("Arial", 14);
				AddPlot(Brushes.Orange, "Test");
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				AddDataSeries(BarsPeriodType.Minute, 5);
				int hr = pStartTime/100;
				int minute = pStartTime - hr*100;
				var tstart = new DateTime(2024, 1, 1, hr, minute, 0);
				hr = pEndTime/100;
				minute = pEndTime - hr*100;
				int day = 1 + (pStartTime > pEndTime ? 1:0);
				var tend = new DateTime(2024, 1, day, hr, minute, 0);
				tstart.AddMinutes(15);
				if(tstart <= tend){
					while(tstart < tend){
						time_keys.Add(ToTime(tstart)/100);
						tstart = tstart.AddMinutes(15);
					}
				}
				else if(tstart > tend){
					var tptr = new DateTime(2024, 1, 1, 0,0,0);
					while(tptr <= tend){
						time_keys.Add(ToTime(tptr)/100);
						tptr = tptr.AddMinutes(15);
					}
					tptr = new DateTime(2024, 1, 1, 23,59,59);
					while(tstart <= tptr){
						time_keys.Add(ToTime(tstart)/100);
						tstart = tstart.AddMinutes(15);
					}
				}
				Data[DayOfWeek.Monday] = new Dictionary<double, SortedDictionary<int, List<double>>>();
				Data[DayOfWeek.Tuesday] = new Dictionary<double, SortedDictionary<int, List<double>>>();
				Data[DayOfWeek.Wednesday] = new Dictionary<double, SortedDictionary<int, List<double>>>();
				Data[DayOfWeek.Thursday] = new Dictionary<double, SortedDictionary<int, List<double>>>();
				Data[DayOfWeek.Friday] = new Dictionary<double, SortedDictionary<int, List<double>>>();
				for(double x = MIN_PCT; x <= MAX_PCT; x=x+0.0004){
					var xx = Math.Round(x,4);
					Data[DayOfWeek.Monday][xx]    = new SortedDictionary<int, List<double>>();
					foreach(var k in time_keys)
						Data[DayOfWeek.Monday][xx][k] = new List<double>();

					Data[DayOfWeek.Tuesday][xx]   = new SortedDictionary<int, List<double>>();
					foreach(var k in time_keys)
						Data[DayOfWeek.Tuesday][xx][k] = new List<double>();

					Data[DayOfWeek.Wednesday][xx] = new SortedDictionary<int, List<double>>();
					foreach(var k in time_keys)
						Data[DayOfWeek.Wednesday][xx][k] = new List<double>();

					Data[DayOfWeek.Thursday][xx]  = new SortedDictionary<int, List<double>>();
					foreach(var k in time_keys)
						Data[DayOfWeek.Thursday][xx][k] = new List<double>();

					Data[DayOfWeek.Friday][xx]    = new SortedDictionary<int, List<double>>();
					foreach(var k in time_keys)
						Data[DayOfWeek.Friday][xx][k] = new List<double>();
				}
			}
		}

		int startabar = 0;
		double sess_start_price = 0;
		DateTime start_time = DateTime.MinValue;
		SortedDictionary<DateTime, double> sess_start_prices = new SortedDictionary<DateTime, double>();
		private Dictionary<DayOfWeek, Dictionary<double, SortedDictionary<int, List<double>>>> Data = new Dictionary<DayOfWeek, Dictionary<double, SortedDictionary<int, List<double>>>>();
		protected override void OnBarUpdate()
		{
			if(BarsInProgress==1 && CurrentBars[1]>0){
				var t = Times[1][1];
				var t1 = ToTime(t)/100;
				t = Times[1][0];
				var dow = (pAllDaysTogether ? DayOfWeek.Monday : t.DayOfWeek);
				var t0 = ToTime(t)/100;
				if(t1 < pStartTime && t0 >= pStartTime){
					startabar = CurrentBars[1];
					sess_start_price = Closes[1][0];
					sess_start_prices[t.Date] = Closes[1][0];
					start_time = t;
				}
var z = t.DayOfWeek == DayOfWeek.Monday;
				if(t1 < pEndTime && t0 >= pEndTime){
					var touched_pct_levels = new List<double>();
					for(int i = startabar; i <= CurrentBars[1]; i++){//march forward from the bar at open of session, see which bars hit which levels
						double H1 = Highs[1].GetValueAt(i-1);
						double H = Highs[1].GetValueAt(i);
						double L1 = Lows[1].GetValueAt(i-1);
						double L = Lows[1].GetValueAt(i);
						t0 = ToTime(Times[1].GetValueAt(i))/100;
						t0 = time_keys.Where(k=> k<=t0).Last();
						var phigh = (H-sess_start_price)/sess_start_price;
						var plow = (L-sess_start_price)/sess_start_price;
						var pct_in_range = Data[dow].Keys.Where(k=>k >= plow && k <= phigh).ToList();
						if(pct_in_range!=null && pct_in_range.Count>0){
							foreach(var _pct in pct_in_range){
								double trigger_price = sess_start_price * (1+_pct);
								if(H1 < trigger_price && H >= trigger_price && L < trigger_price && !touched_pct_levels.Contains(_pct)){
									touched_pct_levels.Add(_pct);
if(z) Print($"{t0} went above the {_pct} high lvl of {trigger_price.ToString("0.00")}");
//if(Data[dow][_pct].ContainsKey(t0))
									Data[dow][_pct][t0].Add(Closes[1][0] - trigger_price);
//else Print("138  Key not found: "+t0);
								}
								if(L1 > trigger_price && H > trigger_price && L <= trigger_price && !touched_pct_levels.Contains(_pct)){
									touched_pct_levels.Add(_pct);
if(z) Print($"{t0} went below the {_pct} low lvl of {trigger_price.ToString("0.00")}");
//if(Data[dow][-_pct].ContainsKey(t0))
									Data[dow][_pct][t0].Add(Closes[1][0] - trigger_price);
//else Print("145  Key not found: "+t0);
								}
							}
						}
					}
				}
			}
			if(CurrentBars[0] > BarsArray[0].Count-4){
//				var dow = DayOfWeek.Wednesday;
//				foreach(var _pct in Data[dow].Keys){
//					var s1 = false;
//					foreach(var kvp in Data[dow][_pct]){
//						var s2 = false;
//						if(kvp.Value.Count>0){
//							if(!s1) Print($"{_pct}"); s1 = true;
//							if(!s2) Print($"  time {kvp.Key}"); s2 = true;
//							foreach(var points in kvp.Value){
//								Print($"       {(points>0?'+':' ')}{Instrument.MasterInstrument.FormatPrice(points)}");
//							}
//						}
//					}
//				}
			}
		}

		private class TheLine {
			public string Tag="";
			public string Type = "Ray";
			public int StartBar=0;
			public int EndBar=1;
			public double Price=0;
			public Color ColorOfLine = Colors.Transparent;
			public TheLine(string tag, string drawType, int startbar, int endbar, double price, System.Windows.Media.Color colorofline) {this.Tag=tag;this.Type=drawType;this.StartBar=startbar;this.EndBar=endbar;this.Price=price; this.ColorOfLine=colorofline;}
		}
		
		private SharpDX.Direct2D1.Brush WhiteBrushDX=null;
		private SharpDX.Direct2D1.Brush YellowBrushDX=null;
		public override void OnRenderTargetChanged()
		{
			if(WhiteBrushDX!=null && !WhiteBrushDX.IsDisposed) WhiteBrushDX.Dispose(); WhiteBrushDX = null;
			if(RenderTarget!=null) WhiteBrushDX = Brushes.White.ToDxBrush(RenderTarget);

			if(YellowBrushDX!=null && !YellowBrushDX.IsDisposed) YellowBrushDX.Dispose(); YellowBrushDX = null;
			if(RenderTarget!=null) YellowBrushDX = Brushes.Yellow.ToDxBrush(RenderTarget);
		}

		private List<TheLine> TheCO = new List<TheLine>();
		double px_per_minute = 10;
		SharpDX.Vector2 v1, v2;
		int ypx = 0;
		int xpx = 0;
		double _pct = double.MinValue;
		List<double> _pcts = null;
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			if(RenderTarget==null || WhiteBrushDX==null) return;
			var TextFormat = this.pFont.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout Layout;//     = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, Instrument.MasterInstrument.FormatPrice(Closes[0][0]), TextFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
			TheCO.Clear();
			int pxRMB = chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]);
			long timeTicksRMB = Time.GetValueAt(CurrentBars[0]).Ticks;
			var ttt = timeTicksRMB - Time.GetValueAt(CurrentBars[0]-1).Ticks;
			if(ttt>0)
				px_per_minute = (pxRMB-chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]-1))/(ttt/TimeSpan.TicksPerMinute);

			double p1 = 0;
			double p2 = 0;
			DateTime t1 = DateTime.MinValue;
			DateTime t2 = DateTime.MinValue;
			int b1 = 0;
			int b2 = 0;
			v1 = new SharpDX.Vector2(0,0);
			v2 = new SharpDX.Vector2(0,0);
			var objs = DrawObjects.Where(k=> k.ToString().Contains(".HorizontalLine")).ToList();//Only horizontal lines are valid for ARC_LineAlert
			foreach (dynamic CO in objs)
			{
				if(CO.ToString().Contains(".HorizontalLine")){
					if(pTrendlineTag.Length>0 && !CO.Tag.ToLower().Contains(pTrendlineTag.ToLower())) continue;
					p1 = CO.StartAnchor.Price;
					t1 = BarsArray[0].GetTime(ChartBars.FromIndex);
					var leftmost_dt_in_view = t1;
					var currentbar_time = BarsArray[0].GetTime(CurrentBars[0]);
					if(pShowHistoricalPctLevel && _pct!=double.MinValue){
						if(sess_start_prices.ContainsKey(currentbar_time.Date)){
							var dow = pAllDaysTogether ? DayOfWeek.Monday : t1.DayOfWeek;
							if(_pcts!=null && _pcts.Count>0){
								if(sess_start_prices.ContainsKey(leftmost_dt_in_view.Date)){
									var open_price = sess_start_prices[leftmost_dt_in_view.Date];
									v1.Y = chartScale.GetYByValue(open_price);
									v1.X = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(ToTime(leftmost_dt_in_view, start_time.Hour, start_time.Minute)));
									v2.Y = chartScale.GetYByValue(open_price * (1+_pct));
									v2.X = v1.X + 10;
									RenderTarget.DrawLine(v1, v2, YellowBrushDX, 10f);
									foreach(var timekvp in Data[dow][_pct]){
										if(timekvp.Value.Count == 0) continue;
										xpx = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(ToTime(t1, timekvp.Key)));
		
										var sum_netpts = timekvp.Value.Sum();
										var abovesL = timekvp.Value.Where(k=> k > 0).ToList();
										double aboves = 0;
										if(abovesL!=null || abovesL.Count > 0) aboves = abovesL.Count;
										double abovespct = timekvp.Value.Count == 0 ? 0 : aboves / timekvp.Value.Count;
										var str = Instrument.MasterInstrument.FormatPrice(sum_netpts);
										str = $"{str}\n{abovespct.ToString("0%")} {timekvp.Value.Count}\n{timekvp.Key}";
										Layout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, TextFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
										var labelRect = new SharpDX.RectangleF(xpx, v2.Y, Layout.Metrics.Width, Layout.Metrics.Height);
		//Print(str+"   at "+xpx+"/"+ypx);
										RenderTarget.DrawText(str, TextFormat, labelRect, WhiteBrushDX);
									}
								}
							}
						}
					}
//					b1 = BarsArray[0].GetBar(t1);
					TheCO.Add(new TheLine(CO.Tag, CO.Name, CurrentBar-1, CurrentBar-2, p1, Colors.Red));
					if(sess_start_prices.ContainsKey(currentbar_time.Date)){
						var dow = pAllDaysTogether ? DayOfWeek.Monday : currentbar_time.DayOfWeek;
						_pct = (p1 - sess_start_prices[currentbar_time.Date])/sess_start_prices[currentbar_time.Date];//_pct is calculated from the day to the drawn horizontal line level
						_pcts = Data[dow].Keys.Where(k=> (_pct<0 ? k >= _pct : k <= _pct)).ToList();
						if(_pcts!=null && _pcts.Count>0){
							ypx = chartScale.GetYByValue(p1);
							_pct = _pct < 0 ? _pcts[0] : _pcts.Last();//adjust _pct to an actual key value, not a floating point calculated value
							foreach(var timekvp in Data[dow][_pct]){
								if(timekvp.Value.Count == 0) continue;
								t1 = ToTime(currentbar_time, timekvp.Key);
								int abar = BarsArray[0].GetBar(t1);
								if(t1 < currentbar_time)
									xpx = chartControl.GetXByBarIndex(ChartBars, abar);
								else{
									double minutesadvance = (t1.Ticks - timeTicksRMB) / TimeSpan.TicksPerMinute;
									xpx = pxRMB + (int)(minutesadvance * px_per_minute);
								}

								var sum_netpts = timekvp.Value.Sum();
								var abovesL = timekvp.Value.Where(k=> k > 0).ToList();
								double aboves = 0;
								if(abovesL!=null || abovesL.Count > 0) aboves = abovesL.Count;
								double abovespct = timekvp.Value.Count == 0 ? 0 : aboves / timekvp.Value.Count;
								var str = Instrument.MasterInstrument.FormatPrice(sum_netpts);
								str = $"{str}\n{abovespct.ToString("0%")} {timekvp.Value.Count}\n{timekvp.Key}";
								Layout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, TextFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
								var labelRect = new SharpDX.RectangleF(xpx, ypx, Layout.Metrics.Width, Layout.Metrics.Height);
//Print(str+"   at "+xpx+"/"+ypx);
								RenderTarget.DrawText(str, TextFormat, labelRect, WhiteBrushDX);
							}
						}
					}
				}
			}
		}
		private DateTime ToTime(DateTime originalT, int x){
			var hr = Convert.ToInt32(Math.Truncate(x/100.0));
			var minute = x - hr*100;
			return new DateTime(originalT.Year, originalT.Month, originalT.Day, hr, minute, 0);
		}
		private DateTime ToTime(DateTime originalT, int hr, int minute){
			return new DateTime(originalT.Year, originalT.Month, originalT.Day, hr, minute, 0);
		}
		#region Properties
		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="Start Time", Order=10, GroupName="Parameters")]
		public int pStartTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="End Time", Order=20, GroupName="Parameters")]
		public int pEndTime
		{ get; set; }

		[Display(Name="Font", Order=30, GroupName="Parameters")]
		public SimpleFont pFont
		{ get; set; }

		[Display(Name="Show historical _pct level?", Order=40, GroupName="Parameters")]
		public bool pShowHistoricalPctLevel
		{get;set;}

		[Display(Name="Lump together all days?", Order=50, GroupName="Parameters")]
		public bool pAllDaysTogether
		{get;set;}

		private string pTrendlineTag = "Horiz";
		[Description("Enter a specific tag name or id (case insensitive)...and the indicator will pay attention ONLY to horizontal lines that contain the this tag/id in their 'Tag' field.  If you leave this parameter blank, then all trendlines will be recognized")]
		[Category("Parameters")]
		public string TrendlineTag
		{
			get { return pTrendlineTag; }
			set { 	string result = value;
					if(string.IsNullOrEmpty(result.Trim())){
						pTrendlineTag = string.Empty;
					}else{
						pTrendlineTag = result;
					}
				}
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Test
		{
			get { return Values[0]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AboveBelow[] cacheAboveBelow;
		public AboveBelow AboveBelow(int pStartTime, int pEndTime)
		{
			return AboveBelow(Input, pStartTime, pEndTime);
		}

		public AboveBelow AboveBelow(ISeries<double> input, int pStartTime, int pEndTime)
		{
			if (cacheAboveBelow != null)
				for (int idx = 0; idx < cacheAboveBelow.Length; idx++)
					if (cacheAboveBelow[idx] != null && cacheAboveBelow[idx].pStartTime == pStartTime && cacheAboveBelow[idx].pEndTime == pEndTime && cacheAboveBelow[idx].EqualsInput(input))
						return cacheAboveBelow[idx];
			return CacheIndicator<AboveBelow>(new AboveBelow(){ pStartTime = pStartTime, pEndTime = pEndTime }, input, ref cacheAboveBelow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AboveBelow AboveBelow(int pStartTime, int pEndTime)
		{
			return indicator.AboveBelow(Input, pStartTime, pEndTime);
		}

		public Indicators.AboveBelow AboveBelow(ISeries<double> input , int pStartTime, int pEndTime)
		{
			return indicator.AboveBelow(input, pStartTime, pEndTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AboveBelow AboveBelow(int pStartTime, int pEndTime)
		{
			return indicator.AboveBelow(Input, pStartTime, pEndTime);
		}

		public Indicators.AboveBelow AboveBelow(ISeries<double> input , int pStartTime, int pEndTime)
		{
			return indicator.AboveBelow(input, pStartTime, pEndTime);
		}
	}
}

#endregion
