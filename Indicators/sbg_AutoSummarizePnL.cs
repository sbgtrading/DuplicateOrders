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

public enum AutoSummarizePnL_SpecialKeys {LeftCtrl, RightCtrl, LeftAlt, RightAlt, LeftShift, RightShift}
public enum AutoSummarizePnL_OutputWindows {OutputWindow1, OutputWindow2, None}
//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Sbg_AutoSummarizePnL : Indicator
	{
		private Brush outofsessionColor;
		private double PV=0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "sbg Auto Summarize PnL";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;

				FormatString = @"ddd dd-MMM";
				pDateTextColor = Brushes.Yellow;
				pWinColor	 = Brushes.Lime;
				pLoserColor = Brushes.Red;
				pShowPnLBox	 = true;
				pTextLoc = TextPosition.TopLeft;
				pFlipTradeDirectionKey = AutoSummarizePnL_SpecialKeys.RightCtrl;
				pStartTime	 = 600;
				pEndTime	 = 1600;
				pBkgOpacity = 20;
				pCommishPerContract = 0;
				pPrintToOutputwindow = AutoSummarizePnL_OutputWindows.OutputWindow2;
			}
			else if (State == State.Configure)
			{
				outofsessionColor = Brushes.Maroon.Clone();
				outofsessionColor.Opacity = pBkgOpacity/100f;
				outofsessionColor.Freeze();
				PV = Instrument.MasterInstrument.PointValue;
			}
			else if (State == State.DataLoaded){
				Draw.TextFixed(this,"summary", "Sbg_AutoSummarizePnL\nNo rulers or lines found\nbetween session start/end", pTextLoc, Brushes.White, new SimpleFont("Arial",14), Brushes.Black, Brushes.Red, 50);
				if(ChartPanel!=null){
					ChartPanel.MouseUp    += OnMouseUp;
				}
				if (timer == null){
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						timer			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 5), IsEnabled = true };
						timer.Tick		+= OnTimerTick;
					});
				}
			}
			else if (State == State.Terminated){ 
				if(ChartPanel!=null){
					ChartPanel.MouseUp -= OnMouseUp;
				}
				if (timer != null){
					timer.IsEnabled = false;
					timer = null;
				}
			}
		}
		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			var clicked = false;
			if(pFlipTradeDirectionKey == AutoSummarizePnL_SpecialKeys.LeftAlt && Keyboard.IsKeyDown(Key.LeftAlt)) clicked = true;
			else if(pFlipTradeDirectionKey == AutoSummarizePnL_SpecialKeys.LeftCtrl && Keyboard.IsKeyDown(Key.LeftCtrl)) clicked = true;
			else if(pFlipTradeDirectionKey == AutoSummarizePnL_SpecialKeys.LeftCtrl && Keyboard.IsKeyDown(Key.LeftCtrl)) clicked = true;
			else if(pFlipTradeDirectionKey == AutoSummarizePnL_SpecialKeys.RightCtrl && Keyboard.IsKeyDown(Key.RightCtrl)) clicked = true;
			else if(pFlipTradeDirectionKey == AutoSummarizePnL_SpecialKeys.LeftShift && Keyboard.IsKeyDown(Key.LeftShift)) clicked = true;
			else if(pFlipTradeDirectionKey == AutoSummarizePnL_SpecialKeys.RightShift && Keyboard.IsKeyDown(Key.RightShift)) clicked = true;

			if(clicked){
				Point coords = e.GetPosition(ChartPanel);
				var X = ChartingExtensions.ConvertToHorizontalPixels(coords.X, ChartControl.PresentationSource);//+ barwidth_int;
				int ABar = ChartBars.GetBarIdxByX(ChartControl, X);
				DateTime ATime = BarsArray[0].GetTime(ABar);
				if(pWinColor == pLoserColor){
					if(pWinColor == Brushes.Magenta) pLoserColor = Brushes.Red; else pLoserColor = Brushes.Magenta;
				}
				var objects = DrawObjects.Where(o=> o.ToString().EndsWith("Ruler") || o.ToString().EndsWith("RulerDollars") || o.ToString().EndsWith(".Line")).ToList();
				if(objects!=null && objects.Count>0){
					foreach(var dob in objects){
						IDrawingTool o = (IDrawingTool)dob;
						var anch = o.Anchors.ToArray();
						bool flip = false;
						if(ATime >= anch[0].Time && ATime <= anch[1].Time) flip = true;
						else if(ATime >= anch[1].Time && ATime <= anch[0].Time) flip = true;
						if(flip){
							if(o.ToString().EndsWith("RulerDollars")){
								RulerDollars rDollars = (RulerDollars)o;
//								var ss = rDollars.LineColor.Pen.Brush.ToString();
								if(rDollars.LineColor.Pen.Brush == pWinColor)
									rDollars.LineColor.Pen = new Pen(pLoserColor, rDollars.LineColor.Pen.Thickness);
								else
									rDollars.LineColor.Pen = new Pen(pWinColor, rDollars.LineColor.Pen.Thickness);
							}else if(o.ToString().EndsWith("Ruler")){
								Ruler rRegular = (Ruler)o;
								if(rRegular.LineColor.Pen.Brush == pWinColor)
									rRegular.LineColor.Pen = new Pen(pLoserColor, rRegular.LineColor.Pen.Thickness);
								else
									rRegular.LineColor.Pen = new Pen(pWinColor, rRegular.LineColor.Pen.Thickness);
							}else if(o.ToString().EndsWith(".Line")){
								DrawingTools.Line rLine = (DrawingTools.Line)o;
								if(rLine.Stroke.Brush == pWinColor)
									rLine.Stroke.Brush = pLoserColor;
								else
									rLine.Stroke.Brush = pWinColor;
							}
						}
					}
				}
				ChartControl.InvalidateVisual();
//				ForceRefresh();
			}
		}
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
			else if(pStartTime!=pEndTime && pBkgOpacity>0) BackBrushes[0] = outofsessionColor;
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
		int pnlcount = 0;
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
int line=115;
try{
			var objects = DrawObjects.Where(o=> o.ToString().EndsWith("VerticalLine")).ToList();
			if(objects!=null && objects.Count>0){
line=118;
				foreach (dynamic dob in objects) {
					IDrawingTool o = (IDrawingTool)dob;
					var L = o.Anchors.ToArray();
					int tt = ToTime(L[0].Time)/100;
line=123;
					if(o.Tag.ToUpper().Contains("START") && pStartTime!= tt) {pStartTime = tt; RecalcSessions = true;}
					if(o.Tag.ToUpper().Contains("END") && pEndTime!= tt) {pEndTime = tt; RecalcSessions = true;}
				}
			}
			bool IsShiftKeyDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			RulerDollars rDollars;
			Ruler rRegular;
			DrawingTools.Line rLine;
			float x=0,y=0;
			float w = chartControl.GetXByBarIndex(ChartBars, 1) - chartControl.GetXByBarIndex(ChartBars, 0);
			double commishPts = pCommishPerContract / PV;
line=134;
			if(CurrentBars[0] >= BarsArray[0].Count-3 && (sum==double.MinValue || IsShiftKeyDown)) {
				pnlcount = 0;
				if(RecalcSessions){
					RecalcSessions = false;
					ABarsInSession.Clear();
					char type = ' ';
					if(pStartTime == pEndTime) type = 'A';//all bars are in session, 24-hr session
					if(pStartTime > pEndTime) type = 'B';
					if(pStartTime < pEndTime) type = 'C';
					if(type!='A'){
						for(int abar = 0; abar< BarsArray[0].Count; abar++){
line=140;
							var tt = ToTime(Times[0].GetValueAt(abar))/100;
							if(type=='B'){
								if(tt >= pStartTime || tt < pEndTime) {
									ABarsInSession.Add(abar);
									if(pBkgOpacity>0) BackBrushes.Set(abar, null);
								}
								else{
line=147;
									if(pBkgOpacity>0) BackBrushes.Set(abar, outofsessionColor);
								}
							}
							else if(type=='C'){
								if(tt >= pStartTime && tt < pEndTime) {
									ABarsInSession.Add(abar);
									if(pBkgOpacity>0) BackBrushes.Set(abar, null);
								}
								else{
line=147;
									if(pBkgOpacity>0) BackBrushes.Set(abar, outofsessionColor);
								}
							}
						}
					}
				}
				List<double> TradeLength = new List<double>();
				var MAE = new List<double>();
				var MFE = new List<double>();
				objects = DrawObjects.Where(o=> o.ToString().EndsWith(".RulerDollars") || o.ToString().EndsWith(".Ruler") || o.ToString().EndsWith(".Line")).ToList();
				var RulersList = new SortedDictionary<DateTime, double>();
				RemoveDrawObject("summary");
//if(objects==null)Print("Objects is null!"); else Print("Objects.count: "+objects.Count);
				if(objects!=null && objects.Count>0){
line=155;
					sum = 0;
					sumwins = 0;
					sumlosses = 0;
					wins = 0;
					losses = 0;
					double qty_total = 0;
					string pWinColorStr = pWinColor.ToString();
//Print("win color: "+pWinColorStr);
					DatesWithTrades.Clear();
					DateTime EntryDT = DateTime.MinValue;
line=165;
					foreach (dynamic dob in objects) {
						//Print(dob.Tag+" found");
						IDrawingTool o = (IDrawingTool)dob;//DrawObjects[dob.Tag];
						var L = o.Anchors.ToArray();
						try{
line=171;
							double EntryPrice = L[0].Price;
							double ExitPrice = L[1].Price;
							#region -- Determine if first abar of anchor is in Session, and calc days count --
							if(L[0].Time < L[1].Time){
								EntryDT = L[0].Time;
							}
							else if(L[1].Time <= L[0].Time){
								EntryDT = L[1].Time;
								EntryPrice = L[1].Price;
								ExitPrice = L[0].Price;
							}
							if(pStartTime!=pEndTime){
								var ab = Bars.GetBar(EntryDT);
								if(!ABarsInSession.Contains(ab)) continue;
							}
							if(!DatesWithTrades.Contains(EntryDT.Date))
								DatesWithTrades.Add(EntryDT.Date);
							#endregion
							double PnL = Math.Abs(EntryPrice-ExitPrice);
							var name = o.ToString();
							double qty = 1;
							if(name.EndsWith("RulerDollars")){
line=194;
								pnlcount = objects.Count;
								rDollars = (RulerDollars)o;
								if(o.Tag.Contains("qty")) qty=GetQty(o.Tag);
								RulersList[EntryDT] = (PnL - commishPts) * qty;
								qty_total += qty;
								TradeLength.Add(Math.Abs(L[0].Time.Ticks-L[1].Time.Ticks));
								var ss = rDollars.LineColor.Pen.Brush.ToString();
								if(!ss.Contains(pWinColorStr)){
									RulersList[L[0].Time] = qty*(-PnL - commishPts);
									GetMAEMFE('L', EntryPrice, ExitPrice, MAE, MFE, BarsArray[0].GetBar(L[0].Time), BarsArray[0].GetBar(L[1].Time));
								}else
									GetMAEMFE('W', EntryPrice, ExitPrice, MAE, MFE, BarsArray[0].GetBar(L[0].Time), BarsArray[0].GetBar(L[1].Time));
							}else if(name.EndsWith("Ruler")){
line=201;
								pnlcount = objects.Count;
								rRegular = (Ruler)o;
								if(o.Tag.Contains("qty")) qty=GetQty(o.Tag);
								RulersList[EntryDT] = (PnL - commishPts) * qty;
								qty_total += qty;
								TradeLength.Add(Math.Abs(L[0].Time.Ticks-L[1].Time.Ticks));
								var ss = rRegular.LineColor.Pen.Brush.ToString();
								if(!ss.Contains(pWinColorStr)){
									RulersList[L[0].Time] = qty*(-PnL - commishPts);
									GetMAEMFE('L', EntryPrice, ExitPrice, MAE, MFE, BarsArray[0].GetBar(L[0].Time), BarsArray[0].GetBar(L[1].Time));
								}else
									GetMAEMFE('W', EntryPrice, ExitPrice, MAE, MFE, BarsArray[0].GetBar(L[0].Time), BarsArray[0].GetBar(L[1].Time));
							}else if(name.EndsWith("DrawingTools.Line")){
line=201;
								pnlcount = objects.Count;
								rLine = (DrawingTools.Line)o;
								if(o.Tag.Contains("qty")) qty=GetQty(o.Tag);
								RulersList[EntryDT] = (PnL - commishPts) * qty;
								qty_total += qty;
								TradeLength.Add(Math.Abs(L[0].Time.Ticks-L[1].Time.Ticks));
								var ss = rLine.Stroke.Brush.ToString();
								if(!ss.Contains(pWinColorStr)){
									RulersList[L[0].Time] = qty*(-PnL - commishPts);
									GetMAEMFE('L', EntryPrice, ExitPrice, MAE, MFE, BarsArray[0].GetBar(L[0].Time), BarsArray[0].GetBar(L[1].Time));
								}else
									GetMAEMFE('W', EntryPrice, ExitPrice, MAE, MFE, BarsArray[0].GetBar(L[0].Time), BarsArray[0].GetBar(L[1].Time));
							}
						}catch(Exception e){Print(o.ToString()+"\n"+dob.Tag+" was not recognized...skipping it\n"+e.ToString());}
//Print(o.GetType().ToString());
					}
line=211;
					var AllPnL = new List<double>();
					foreach(var trade in RulersList){
//						var tt = ToTime(trade.Key)/100;
//						if(pStartTime==pEndTime || tt>=pStartTime && tt <= pEndTime){
							PrintIt($"{trade.Key}:   {(PV*trade.Value).ToString("C")}");
							sum = sum + trade.Value;
							if(trade.Value>0){
line=219;
								AllPnL.Add(trade.Value);
								sumwins += trade.Value;
								wins++;
							}else{
line=224;
								AllPnL.Add(trade.Value);
								sumlosses += trade.Value;
								losses++;
							}
//						}
					}
					if(AllPnL.Count>0){
						double TradeLen_Minutes = (TradeLength.Average()/TimeSpan.TicksPerMinute);
						PrintIt("");
						PrintIt($"Avg trade length (minutes): {TradeLen_Minutes.ToString("0.0")}");
						PrintIt($"Commish/slippage per contract: {pCommishPerContract.ToString("C")}");
						AllPnL.Sort();

						double winPct = wins*1.0/(wins+losses);
						double pnlTicks = sum/TickSize;
						double avgwinTks = sumwins/wins;
						double avglossTks = Math.Abs(sumlosses/losses);
						double medianTks = AllPnL[0]/TickSize;
						if(AllPnL.Count>2) medianTks = AllPnL[AllPnL.Count/2]/TickSize;
						string msg = "";
						if(qty_total != wins+losses){
							msg = string.Format(
	//							"Trades: {0}\nWins: {1}\nLosses: {2}\n\nWin%: {3}\nPF: {4}\nExpectancy (tks): {5}\nPnL ticks {6}\nAvg Ticks/trade: {7}",
								"Trades: {0}\n{1}: Wins ({2})\n{3}: Losses\n\n{4}: PF\n\n{5}: PnL (tks)\n{6}: Avg tks/contract\n{7}: Median tks/trade\n{8}: Days\n{9}-mins/trade",
								(wins+losses), 
								wins,
								winPct.ToString("0%"),
								losses,
								Math.Abs(sumwins/sumlosses).ToString("0.0"),//PF
								pnlTicks.ToString("0"),
								(pnlTicks/qty_total).ToString("0"),
								medianTks.ToString("0"),
								DatesWithTrades.Count,
								TradeLen_Minutes.ToString("0.0"));
						}else{
							msg = string.Format(
	//							"Trades: {0}\nWins: {1}\nLosses: {2}\n\nWin%: {3}\nPF: {4}\nExpectancy (tks): {5}\nPnL ticks {6}\nAvg Ticks/trade: {7}",
								"Trades: {0}\n{1}: Wins ({2})\n{3}: Losses\n\n{4}: PF\n\n{5}: PnL (tks)\n{6}: Avg tks/trade\n{7}: Median tks/trade\n{8}: Days\n{9}-mins/trade",
								(wins+losses), 
								wins,
								winPct.ToString("0%"),
								losses,
								Math.Abs(sumwins/sumlosses).ToString("0.0"),//PF
								pnlTicks.ToString("0"),
								(pnlTicks/(wins+losses)).ToString("0"),
								medianTks.ToString("0"),
								DatesWithTrades.Count,
								TradeLen_Minutes.ToString("0.0"));
						}
						PrintIt("");
						PrintIt(msg);
						if(pShowPnLBox){
							Draw.TextFixed(this,"summary",msg, pTextLoc, Brushes.White, new SimpleFont("Arial",14), Brushes.Black, sum>0 ? Brushes.DarkGreen:Brushes.Red, 100);
						}
						if(MFE.Count>0)
							PrintIt($"Avg Favorable Excursion (ticks): {Math.Round(MFE.Average()/TickSize,0)}");
						if(MAE.Count>0)
							PrintIt($"Avg Adverse Excursion (ticks): {Math.Round(MAE.Average()/TickSize,0)}");
						PrintIt("-------------------------------------------------");
						PrintIt("-------------------------------------------------\n\n\n");
					}
				}
			}
			if(pnlcount==0){
				RemoveDrawObject("summary");
				if(pShowPnLBox) Draw.TextFixed(this,"summary", "sbg_AutoSummarizePnL\nNo rulers or lines found\nbetween session start/end", pTextLoc, Brushes.White, new SimpleFont("Arial",14), Brushes.Black, Brushes.Red, 50);
			}

			var txtFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout txtLayout = null;

			if(Bars.BarsType.IsIntraday){
				SharpDX.RectangleF labelRect;
				var str = "@ 2025 NinjaTrader, LLC";
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
						labelRect = new SharpDX.RectangleF(x, y-txtLayout.FontSize*2f, txtLayout.Metrics.Width, txtLayout.FontSize+7f);
					else
						labelRect = new SharpDX.RectangleF(x, y, txtLayout.Metrics.Width, txtLayout.FontSize+7f);
					if(noprint_zone<0) noprint_zone = labelRect.Width;
					str = Times[0].GetValueAt(abar).ToString(FormatString);
					RenderTarget.DrawText(str, txtFormat, labelRect, TextColorBrushDX);
					if(noprint_zone>0 && x < noprint_zone) break;//stop printing dates if they are too close to the left-edge
				}
			}
			txtLayout.Dispose();
			txtFormat.Dispose();
}catch(Exception ddk){Print(line+":  "+ddk.ToString());}
		}
		private double GetQty(string s){
			int idx = s.IndexOf("qty");
			s = s.Substring(idx+3);
			var str = s.Split(new char[]{' ','|',':'}, StringSplitOptions.RemoveEmptyEntries);
			double qty = 1;
			if(double.TryParse(str[0], out qty)) return Math.Max(1,qty);
			return 1;
		}
		private void GetMAEMFE(char WinLoss, double EntryPrice, double ClosePrice, List<double> MAE, List<double> MFE, int ab0, int ab1){
			var LL = EntryPrice;
			var HH = EntryPrice;
			for(int ab = Math.Min(ab0,ab1)+1; ab<= Math.Max(ab0,ab1); ab++){
				LL = Math.Min(LL, Lows[0].GetValueAt(ab));
				HH = Math.Max(HH, Highs[0].GetValueAt(ab));
			}
			string Dir = "Long";
			if(WinLoss == 'W' && EntryPrice > ClosePrice) Dir = "Short";
			else if(WinLoss == 'L' && EntryPrice < ClosePrice) Dir = "Short";
			if(Dir=="Long"){
				MAE.Add(EntryPrice-LL);
				MFE.Add(HH-EntryPrice);
			}else{
				MAE.Add(HH-EntryPrice);
				MFE.Add(EntryPrice-LL);
			}
		}

		private void PrintIt(string s){
			if(pPrintToOutputwindow == AutoSummarizePnL_OutputWindows.OutputWindow1)
				PrintTo = PrintTo.OutputTab1;
			else if(pPrintToOutputwindow == AutoSummarizePnL_OutputWindows.OutputWindow2)
				PrintTo = PrintTo.OutputTab2;
			else return;
			Print(s);
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
		[Display(Order=21, Name="PnL box Location", GroupName="PnL Box", Description="")]
		public TextPosition pTextLoc
		{get;set;}

		[XmlIgnore]
		[Display(Order=30, Name="Win line color", GroupName="PnL Box", Description="Color of the Ruler line for a 'winning' trade")]
		public Brush pWinColor
		{ get; set; }
			[Browsable(false)]
			public string pWinColorBrush_Serialize {	get { return Serialize.BrushToString(pWinColor); } set { pWinColor = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(Order=30, Name="Lose line color", GroupName="PnL Box", Description="Color of the Ruler line for a 'losing' trade")]
		public Brush pLoserColor
		{ get; set; }
			[Browsable(false)]
			public string pLoserColorBrush_Serialize {	get { return Serialize.BrushToString(pLoserColor); } set { pLoserColor = Serialize.StringToBrush(value); }}

		[Display(Order=40, Name="Start Time", GroupName="PnL Box", Description="")]
		public int pStartTime
		{ get; set; }
		[Display(Order=50, Name="End Time", GroupName="PnL Box", Description="")]
		public int pEndTime
		{ get; set; }

		[Range(0,100)]
		[Display(Order=55, Name="Opacity out-of-session", GroupName="PnL Box", Description="")]
		public int pBkgOpacity
		{ get; set; }

		[Display(Order=60, Name="Commish/contract", GroupName="PnL Box", Description="Dollars per contract or per share")]
		public double pCommishPerContract
		{ get; set; }

		[Display(Order=10, Name="Key to Flip Trade", GroupName="Special Keys", Description="This key, plus a mouse-click, will change direction of a trade on the ruler")]
		public AutoSummarizePnL_SpecialKeys pFlipTradeDirectionKey
		{get;set;}
		
		[Display(Order=10, Name="Print to OutputWindow?", GroupName="PnL Box", Description="")]
		public AutoSummarizePnL_OutputWindows pPrintToOutputwindow
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Sbg_AutoSummarizePnL[] cacheSbg_AutoSummarizePnL;
		public Sbg_AutoSummarizePnL Sbg_AutoSummarizePnL()
		{
			return Sbg_AutoSummarizePnL(Input);
		}

		public Sbg_AutoSummarizePnL Sbg_AutoSummarizePnL(ISeries<double> input)
		{
			if (cacheSbg_AutoSummarizePnL != null)
				for (int idx = 0; idx < cacheSbg_AutoSummarizePnL.Length; idx++)
					if (cacheSbg_AutoSummarizePnL[idx] != null &&  cacheSbg_AutoSummarizePnL[idx].EqualsInput(input))
						return cacheSbg_AutoSummarizePnL[idx];
			return CacheIndicator<Sbg_AutoSummarizePnL>(new Sbg_AutoSummarizePnL(), input, ref cacheSbg_AutoSummarizePnL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Sbg_AutoSummarizePnL Sbg_AutoSummarizePnL()
		{
			return indicator.Sbg_AutoSummarizePnL(Input);
		}

		public Indicators.Sbg_AutoSummarizePnL Sbg_AutoSummarizePnL(ISeries<double> input )
		{
			return indicator.Sbg_AutoSummarizePnL(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Sbg_AutoSummarizePnL Sbg_AutoSummarizePnL()
		{
			return indicator.Sbg_AutoSummarizePnL(Input);
		}

		public Indicators.Sbg_AutoSummarizePnL Sbg_AutoSummarizePnL(ISeries<double> input )
		{
			return indicator.Sbg_AutoSummarizePnL(input);
		}
	}
}

#endregion
