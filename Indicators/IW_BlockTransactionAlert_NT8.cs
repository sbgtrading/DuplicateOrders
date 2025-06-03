// 
// Copyright (C) 2011, SBG Trading Corp.    www.affordableindicators.com
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.



#region Using declarations
using System;
//using System.IO;
using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
//using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Linq;
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
    [CategoryOrder("Parameters", 10)]
    [CategoryOrder("Audible", 20)]
    [CategoryOrder("Audible HighAlert", 30)]
    [CategoryOrder("Custom Visual", 40)]
    [CategoryOrder("Messages Visual", 50)]
    [CategoryOrder("Visual", 60)]

	[Description("Plays an alert when a transaction volume meets or exceeds the specified minimum")]
	public class BlockTransactionAlert : Indicator
	{
		private class RectRecord {
			public int StartABar;
			public int EndABar;
			public double LowPrice;
			public double HighPrice;
			//public Brush TextBrush;
			public RectRecord(int startabar, int endabar, double highprice, double lowprice)/*, Brush outlinecolor, Brush fillcolor, Brush axiscolor)*/{ this.StartABar=startabar; this.EndABar=endabar; this.HighPrice=highprice; this.LowPrice=lowprice;
//				if(outlinecolor!=Brushes.Transparent) 
//					this.TextBrush = outlinecolor.Clone();
//				else if(fillcolor!=Brushes.Transparent) 
//					this.TextBrush = fillcolor.Clone();
//				else this.TextBrush = axiscolor.Clone();
//				TextBrush.Freeze();
			}
		}
		private class MessageRecord{
			public string Msg;
			public long Tick;
			public sbyte Loc;
			public long Vol;
			public double Price;
			public MessageRecord(DateTime t, sbyte loc, long vol, string InstDesc, double price, string price_str, bool IncludeTimestamp){
				Tick = t.Ticks;
				Loc = loc;
				Vol = vol;
				Price = price;
				if(IncludeTimestamp){
					if(loc==2)  Msg = String.Format("{5} Above Ask:  {0}{1}{2}{3}{4}",t.ToLongTimeString().ToLower(),"\t  Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==-2) Msg = String.Format("{5} Below Bid:  {0}{1}{2}{3}{4}",t.ToLongTimeString().ToLower(),"\t  Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==1)  Msg = String.Format("{5} At Ask:   {0}{1}{2}{3}{4}",t.ToLongTimeString().ToLower(),"\t  Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==-1) Msg = String.Format("{5} At Bid:   {0}{1}{2}{3}{4}",t.ToLongTimeString().ToLower(),"\t  Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==0)  Msg = String.Format("{5} Between:  {0}{1}{2}{3}{4}",t.ToLongTimeString().ToLower(),"\t  Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
				} else {
					if(loc==2)  Msg = String.Format("{4} Above Ask:  {0}{1}{2}{3}","Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==-2) Msg = String.Format("{4} Below Bid:  {0}{1}{2}{3}","Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==1)  Msg = String.Format("{4} At Ask:   {0}{1}{2}{3}","Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==-1) Msg = String.Format("{4} At Bid:   {0}{1}{2}{3}","Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
					if(loc==0)  Msg = String.Format("{4} Between:  {0}{1}{2}{3}","Vol: ",vol.ToString(),"     on ",InstDesc,price_str);
				}
			}
		}
		private class BlockRecord {
			public DateTime T;
			public sbyte Location;
			public double Price;
			public long Size;
			public BlockRecord(DateTime t, sbyte location, double price, long size){this.T=t; this.Location=location; this.Price=price; this.Size=size;}
		}
		private int pMinBlockVolume = 100;
		private int pHighAlertBlockVolume = 99999999;
		private bool pPrintToOutputWindow = false;
		private string pAlertSoundFile = "none";
		private string pAskAlertSoundFile = "Alert2.wav";
		private string pBidAlertSoundFile = "Alert2.wav";
		private bool PlaySoundBetween = false;
		private bool PlaySoundAtAsk = true;
		private bool PlaySoundAtBid = true;
		private string pAlertSoundFileHA = "none";
		private string pAskAlertSoundFileHA = "none";
		private string pBidAlertSoundFileHA = "none";
		private bool PlaySoundBetweenHA = false;
		private bool PlaySoundAtAskHA = false;
		private bool PlaySoundAtBidHA = false;
		private int AlertCounter = 0;
		private DateTime now=DateTime.MinValue;

		private int ptr = 0;
		private double askPrice = 0, bidPrice = 0;
		private List<MessageRecord> Messages = new List<MessageRecord>();
		private List<string> TextOnScreen = new List<string>();
		private List<string> TextTags = new List<string>();
		private List<BlockRecord>   RecentBlocks = new List<BlockRecord>();
		private List<long>   RecentBlockSizes = new List<long>();
		private SortedDictionary<int,List<BlockRecord>> History = new SortedDictionary<int,List<BlockRecord>>();
		private double MinimumBlockVolume = 0;
		private double AvgBlockVolume = 0;
		private string screenmsg = string.Empty;
		private Gui.Tools.SimpleFont txtFont=null;
		private long LastVolumeTotal = 0;
		private DateTime LaunchedAt;
		private bool DoubleDiamondFound = false;
		private SortedDictionary<int,string> DiamondsDict = new SortedDictionary<int,string>();
		private SortedDictionary<string,RectRecord> RectDict = new SortedDictionary<string,RectRecord>();
		private SortedDictionary<int,long> VolOnEachBar = new SortedDictionary<int,long>();
		private Brush TextBrushAtAsk, TextBrushAtBid, TextBrushBetween;
		private float DotWidth, HalfDotWidth;
		private int IgnoredTransactions = 0,CountedTransactions = 0;
		private string NL = Environment.NewLine;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				var IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", "AIBlockTransactionAlert", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				IsOverlay=true;
				DisplayInDataBox = true;
				IsAutoScale=false;
				PaintPriceMarkers = false;
				Calculate=Calculate.OnPriceChange;
//			Name = "iw BlockTransactionAlert v2.1";
//			Name = "iw BlockTransactionAlert v2.2";//removed the DotsOnBlockPrice plot, added DotSize parameter, fixed MinVolume filter bug
//			Name = "iw BlockTransactionAlert v2.3";//added parameters "MessagesContainAge", "ColorizeMessages" and "MessagesContainTimestamp"
//			Name = "iw BlockTransactionAlert v2.3.1";//attempted to correct bug where some dots were not printing while their Message was printing (got rid of RecentBlocks, adding data to History structure moved into the OnMarketUpdate method)
//			Name = "iw BlockTransactionAlert v2.3.2";//added buy/sell pressure background fill to the on-bar volume text
//				v2.3.3  cleaned up DX brush creation, added "Freeze()" after ContrastBrush() calls, fixed "ColorizeMessages" (it wasn't)
//				v2.3.4  Cleaned up parameter names, added CategoryOrder statements, attached FontSize to the block volume numbers
				Name = "iw BlockTransactionAlert v2.3.4";
			}
			if(State == State.DataLoaded){
				History.Clear();
				this.MinimumBlockVolume = this.pMinBlockVolume;
				if(ChartControl!=null) {
					txtFont =  (SimpleFont)ChartControl.Properties.LabelFont.Clone();
					txtFont.Size = pFontSize;
				}

				LaunchedAt = NinjaTrader.Core.Globals.Now;

				string wavname = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,@"sounds", pAlertSoundFile);
				PlaySoundBetween = System.IO.File.Exists(wavname);
				wavname = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,@"sounds", pAskAlertSoundFile);
				PlaySoundAtAsk = System.IO.File.Exists(wavname);
				wavname = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,@"sounds", pBidAlertSoundFile);
				PlaySoundAtBid = System.IO.File.Exists(wavname);

				wavname = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,@"sounds", pAlertSoundFileHA);
				PlaySoundBetweenHA = System.IO.File.Exists(wavname);
				wavname = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,@"sounds", pAskAlertSoundFileHA);
				PlaySoundAtAskHA = System.IO.File.Exists(wavname);
				wavname = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,@"sounds", pBidAlertSoundFileHA);
				PlaySoundAtBidHA = System.IO.File.Exists(wavname);
				
				DotWidth = (pDotSize);
				HalfDotWidth = DotWidth/2.0f;
			}
		}
		protected override void OnBarUpdate()
		{
//			if(State == State.Historical) {
//				return;
//			}
			if(IsFirstTickOfBar) {
				ptr = 0;
				History[CurrentBar] = new List<BlockRecord>();
				if(History.ContainsKey(CurrentBar-1) && History[CurrentBar-1].Count==0) History.Remove(CurrentBar-1);
			}
			if(bidPrice!=0 && askPrice!=0) RemoveDrawObject("nodata");
		}
		private void GetDrawObjects(ref SortedDictionary<int,string> DiamondsDict, ref SortedDictionary<string,RectRecord> RectDict) {
			//Runs through all the objects on the chart and puts the Arrows and Dots and Squares into a SortedDictionary
			#region GetDrawObjects
			DoubleDiamondFound = false;
			try{
				if(this.ChartControl==null || this.DrawObjects==null) {Print("No ChartControl...exiting GetDrawObjects");return;}

				if(DiamondsDict==null) DiamondsDict = new SortedDictionary<int,string>(); 
				else DiamondsDict.Clear();
				if(RectDict==null) RectDict = new SortedDictionary<string,RectRecord>(); 
				else RectDict.Clear();

				foreach (IDrawingTool CO in DrawObjects) {
					if(CO is NinjaTrader.NinjaScript.DrawingTools.Diamond) {
						var d = (NinjaTrader.NinjaScript.DrawingTools.Diamond)CO;
						int abar = Bars.GetBar(d.Anchor.Time);
						if(DiamondsDict.ContainsKey(abar)) {
							DoubleDiamondFound = true;
//Print("Second diamond on this bar is found");
						}
						else DiamondsDict[abar] = CO.Tag;
//Print(CO.Tag+"  Diamonds found: "+DiamondsDict.Count+"   price: "+d.Anchor.Price+"    DoubleDiamondFound: "+DoubleDiamondFound.ToString());
					}
					if(CO is NinjaTrader.NinjaScript.DrawingTools.Rectangle) {
						NinjaTrader.NinjaScript.DrawingTools.Rectangle r = (NinjaTrader.NinjaScript.DrawingTools.Rectangle)CO;
						int lmb = Math.Min(Bars.GetBar(r.StartAnchor.Time),Bars.GetBar(r.EndAnchor.Time));
						int rmb = Math.Max(Bars.GetBar(r.StartAnchor.Time),Bars.GetBar(r.EndAnchor.Time));
						RectDict[r.Tag] = new RectRecord(lmb, rmb, Math.Max(r.StartAnchor.Price,r.EndAnchor.Price),Math.Min(r.StartAnchor.Price,r.EndAnchor.Price));//, r.OutlineStroke.Pen.Brush, r.AreaBrush, ChartControl.Properties.AxisPen.Brush);
//Print("r.lmb: "+lmb+"  rmb: "+rmb+"   rect count: "+RectDict.Count);
					}
				}

			} catch( Exception e) {
//				PrintStatusMsgOnBar(abar, string.Concat(Name," GetDrawObjects EXCEPTION on " , Instrument ,  e.ToString()) );
				Alert(this.GetType().Name + Instrument.FullName,NinjaTrader.NinjaScript.Priority.High,"Instrument=" + Instrument + "   **************EXCEPTION" + e.ToString(),AddSoundFolder("Alert2.wav"), 60, Brushes.Red,Brushes.White);  
			}
			return;
			#endregion
		}
//===========================================================================================================
	private System.Windows.Media.Brush ContrastBrush(System.Windows.Media.Brush cl){
		System.Windows.Media.SolidColorBrush solidBrush = cl as System.Windows.Media.SolidColorBrush;

		byte bRed   = solidBrush.Color.R;
		byte bGreen = solidBrush.Color.G;
		byte bBlue  = solidBrush.Color.B;
		double c = (bRed*0.2126)+(bGreen*0.7152)+(bBlue*0.0722);
		if(c>128) return Brushes.Black.Clone();
		else return Brushes.White.Clone();
	}
//===========================================================================================================
		private SharpDX.Direct2D1.Brush BackgroundDXBrush, TextDXBrushBetween, TextDXBrushAtAsk, TextDXBrushAtBid, WhiteDXBrush, 
				BlackDXBrush, DotDXBrushAtAsk, DotDXBrushAtBid, DotDXBrushBetween, NetVolTextDXBrush;
        public override void OnRenderTargetChanged()
        {
int line=252;
			try{
			if(BackgroundDXBrush != null && !BackgroundDXBrush.IsDisposed){
				BackgroundDXBrush.Dispose();
				BackgroundDXBrush = null;
			}
line=258;
			if(TextDXBrushBetween != null && !TextDXBrushBetween.IsDisposed){
				TextDXBrushBetween.Dispose();
				TextDXBrushBetween = null;
			}
line=263;
			if(TextDXBrushAtAsk != null){
				TextDXBrushAtAsk.Dispose();
				TextDXBrushAtAsk = null;
			}
line=268;
			if(TextDXBrushAtBid != null && !TextDXBrushAtBid.IsDisposed){
				TextDXBrushAtBid.Dispose();
				TextDXBrushAtBid = null;
			}
line=273;
			if(WhiteDXBrush != null && !WhiteDXBrush.IsDisposed){
				WhiteDXBrush.Dispose();
				WhiteDXBrush = null;
			}
line=278;
			if(BlackDXBrush != null && !BlackDXBrush.IsDisposed){
				BlackDXBrush.Dispose();
				BlackDXBrush = null;
			}
line=283;
			if(DotDXBrushAtAsk != null && !DotDXBrushAtAsk.IsDisposed){
				DotDXBrushAtAsk.Dispose();
				DotDXBrushAtAsk = null;
			}
line=288;
			if(DotDXBrushAtBid != null && !DotDXBrushAtBid.IsDisposed){
				DotDXBrushAtBid.Dispose();
				DotDXBrushAtBid = null;
			}
line=293;
			if(DotDXBrushBetween != null && !DotDXBrushBetween.IsDisposed){
				DotDXBrushBetween.Dispose();
				DotDXBrushBetween = null;
			}
//			if(RenderTarget!=null)
//				NetVolTextDXBrush = Brushes.White.ToDxBrush(RenderTarget);
			if(RenderTarget!=null && ChartControl!=null)
				BackgroundDXBrush = ChartControl.Background.ToDxBrush(RenderTarget);
line=302;
			if(TextBrushBetween==null || !TextBrushBetween.IsFrozen){
				TextBrushBetween = ContrastBrush(pDotBrushBetween);
				TextBrushBetween.Freeze();
			}
			if(TextBrushAtAsk==null || !TextBrushAtAsk.IsFrozen){
				TextBrushAtAsk = ContrastBrush(pDotBrushAtAsk);
				TextBrushAtAsk.Freeze();
			}
			if(TextBrushAtBid==null || !TextBrushAtBid.IsFrozen){
				TextBrushAtBid = ContrastBrush(pDotBrushAtBid);
				TextBrushAtBid.Freeze();
			}

			if(RenderTarget!=null && TextBrushBetween!=null && TextBrushBetween.IsFrozen)
				TextDXBrushBetween = TextBrushBetween.ToDxBrush(RenderTarget);
line=305;
			if(RenderTarget!=null && TextBrushAtAsk!=null && TextBrushAtAsk.IsFrozen)
				TextDXBrushAtAsk = TextBrushAtAsk.ToDxBrush(RenderTarget);
line=308;
			if(RenderTarget!=null && TextBrushAtBid!=null && TextBrushAtBid.IsFrozen)
				TextDXBrushAtBid = TextBrushAtBid.ToDxBrush(RenderTarget);
line=311;
			if(RenderTarget!=null)
				WhiteDXBrush = Brushes.White.ToDxBrush(RenderTarget);
line=314;
			if(RenderTarget!=null)
				BlackDXBrush = Brushes.Black.ToDxBrush(RenderTarget);
line=317;
			if(RenderTarget!=null)
				DotDXBrushAtAsk = pDotBrushAtAsk.ToDxBrush(RenderTarget);
line=320;
			if(RenderTarget!=null)
				DotDXBrushAtBid = pDotBrushAtBid.ToDxBrush(RenderTarget);
line=323;
			if(RenderTarget!=null)
				DotDXBrushBetween = pDotBrushBetween.ToDxBrush(RenderTarget);
			}catch(Exception ee){Print(line+"  ----"+ee.ToString());}
		}
//===========================================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!IsVisible) return;

			base.OnRender(chartControl, chartScale);
			if(ChartControl==null) return;

int line = 341;
try{
			int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = ChartBars.ToIndex;

			float barSpace = ChartControl.Properties.BarDistance / 4.0f;
			int RMB = Math.Min(Bars.Count-1, lastBarPainted);
			int LMB = Math.Max(0, firstBarPainted);
			SharpDX.Vector2     TxtVector;
			TextFormat TxtFormat = new TextFormat(Core.Globals.DirectWriteFactory, ChartPanel.FontFamily.ToString(), SharpDX.DirectWrite.FontWeight.Normal,
											SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, pFontSize) 
											{ TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, WordWrapping = WordWrapping.NoWrap };
			TextLayout	TxtLayout = new TextLayout(Core.Globals.DirectWriteFactory, "X", TxtFormat, ChartPanel.W, TxtFormat.FontSize);
line=350;
			TimeSpan ts = new TimeSpan(NinjaTrader.Core.Globals.Now.Ticks - LaunchedAt.Ticks);
			if(ts.TotalSeconds>pSecondsBeforeWarningMessage) {
				RemoveDrawObject("nodata");
				if(bidPrice==0 && askPrice==0)
					Draw.TextFixed(this, "nodata", string.Concat(Name," warning:",NL,"No market data is being received",NL,"Are you connected to a data feed?",NL,"If you are connected to a datafeed, then hit 'F5' to restart the indicator", NL,"If this problem persists, consider restarting the NT platform"),TextPosition.Center, Brushes.Black, ChartControl.Properties.LabelFont,Brushes.Black,Brushes.Red,100);
				else {
					if(CountedTransactions==0)
						Draw.TextFixed(this, "nodata",string.Concat(Name," warning:",NL,"Currently, no transactions meet your minimum volume parameters",NL,"You may need to lower your minimum volume requirements",NL,"# of transactions ignored due to insufficient volume: ",IgnoredTransactions.ToString()),TextPosition.Center,Brushes.Black, ChartControl.Properties.LabelFont, Brushes.Black,Brushes.Red,100);
					else RemoveDrawObject("nodata");
				}
			}
			if(pAveragingPeriod>0) {
				if(RecentBlockSizes.Count<pAveragingPeriod) 
					Draw.TextFixed(this, "accumulating", "...collecting data for calculating average volume.  Block transactions will display shortly", TextPosition.BottomLeft);
				else
					RemoveDrawObject("accumulating");
			}
				if(pAveragingPeriod>0 && RecentBlockSizes.Count>=this.pAveragingPeriod)
					screenmsg = string.Format("Avg:  {0}{1} = {2}{3}", 
										AvgBlockVolume.ToString("0.0"), 
										(this.pAveragingBufferPct>0 ? string.Format(" + {0}% buffer",pAveragingBufferPct) : string.Empty), 
										MinimumBlockVolume.ToString("0.0"),
										NL);
				else if(CountedTransactions==0)
					screenmsg = string.Format("Number of transactions that are less than the min volume requirement: (0)",IgnoredTransactions);
				else {
					RemoveDrawObject("screenmsg");
					screenmsg = string.Empty;
				}

line=378;
				if(Messages.Count > 0 && pMessageCount > 0) {
					if(!pColorizeMessages){
						if(this.pMessagesContainAge){
							for(int i = 0; i<Math.Min(pMessageCount,Messages.Count); i++) {
								ts = new TimeSpan(now.Ticks - Messages[i].Tick);
								string age_str = string.Empty;
								if(ts.TotalSeconds>299) {
									if(ts.Minutes<60) age_str = ts.Minutes+":"+ts.Seconds.ToString("00");
									else age_str = ts.Hours+":"+ts.Minutes+":"+ts.Seconds.ToString("00");
								}
								else age_str = ts.TotalSeconds.ToString("0");
								screenmsg = String.Format("{0}{1}{2}  age: {3}", screenmsg, NL, Messages[i].Msg, age_str);
							}
						} else {
							for(int i = 0; i<Math.Min(pMessageCount,Messages.Count); i++) {
								screenmsg = String.Format("{0}{1}{2}", screenmsg, NL, Messages[i].Msg);
							}
						}
						RemoveDrawObject("screenmsg");
						Draw.TextFixed(this, "screenmsg", screenmsg, pMessageLocation, ChartControl.Properties.AxisPen.Brush, txtFont, ChartControl.Background, ChartControl.Background,10);
					} else {
line=399;
						float MaxWidth = 0;
						float TotalHeight = 0;
						//SizeF size = new SizeF(0f,0f);
						while(TextOnScreen.Count<Messages.Count) TextOnScreen.Add("");
						for(int i = 0; i<Math.Min(pMessageCount,Messages.Count); i++) {
							if(this.pMessagesContainAge){
								ts = new TimeSpan(now.Ticks - Messages[i].Tick);
								string age_str = string.Empty;
								if(ts.TotalSeconds>299) {
									if(ts.Minutes<60) age_str = ts.Minutes+":"+ts.Seconds.ToString("00");
									else age_str = ts.Hours+":"+ts.Minutes+":"+ts.Seconds.ToString("00");
								}
								else age_str = ts.TotalSeconds.ToString("0");
								TextOnScreen[i] = String.Format("{0}  age: {1}", Messages[i].Msg, age_str);
							} else {
								TextOnScreen[i] = Messages[i].Msg;
							}
							TxtLayout	= new TextLayout(Core.Globals.DirectWriteFactory, TextOnScreen[i], TxtFormat, ChartPanel.W, TxtFormat.FontSize);
							MaxWidth    = Math.Max(MaxWidth,TxtLayout.Metrics.Width+2f);
							TotalHeight = TotalHeight + TxtLayout.Metrics.Height+2f;
						}
line=405;
						float x = ChartPanel.X;
						float y = ChartPanel.Y + TxtLayout.Metrics.Height*2;
						if(pMessageLocation==TextPosition.TopRight) 
							x = ChartPanel.W-MaxWidth;
						else if(pMessageLocation==TextPosition.BottomRight) {
							x = ChartPanel.W - MaxWidth;
							y = ChartPanel.H - TotalHeight - TxtLayout.Metrics.Height;
						}
						else if(pMessageLocation==TextPosition.BottomLeft) 
							y = ChartPanel.H - TotalHeight - TxtLayout.Metrics.Height;
						else if(pMessageLocation==TextPosition.Center) {
							x = ChartPanel.W/2f - MaxWidth/2f;
							y = ChartPanel.H/2f - TotalHeight/2f;
						}
						for(int i = 0; i<TextOnScreen.Count && i<Messages.Count; i++) {
							TxtLayout	= new TextLayout(Core.Globals.DirectWriteFactory, TextOnScreen[i], TxtFormat, ChartPanel.W, TxtFormat.FontSize);
							TxtVector	= new SharpDX.Vector2(x, y);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y-1f, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height+2f), BackgroundDXBrush);
							switch (Messages[i].Loc) {
								case 0:  if(DotDXBrushBetween!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, DotDXBrushBetween, SharpDX.Direct2D1.DrawTextOptions.NoSnap);break;
								case 2:
								case 1:  if(DotDXBrushAtAsk!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, DotDXBrushAtAsk, SharpDX.Direct2D1.DrawTextOptions.NoSnap);break;
								case -1:
								case -2: if(DotDXBrushAtBid!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, DotDXBrushAtBid, SharpDX.Direct2D1.DrawTextOptions.NoSnap);break;
							}
							y = y + TxtLayout.Metrics.Height+2f;
						}
					}
				}
line=452;
				while(Messages.Count-1 > pMessageCount) Messages.RemoveAt(Messages.Count-1);
//}catch(Exception e1){Print(line+":  "+e1.ToString());}

line=456;
			long Total = 0;
//try{
			string s;

			var BarsWithData = from n in History.Keys
				where n>= LMB && n <= RMB
			    select n;
			if(pShowTotalsOnDiamond || pShowSummaryOnRectangle) {
				GetDrawObjects(ref DiamondsDict, ref RectDict);
				VolOnEachBar.Clear();
			}
line=468;
			foreach(int abar in BarsWithData) {
				var TotalsAtPrice = new SortedDictionary<double,long>();
				for(int ptr = 0; ptr<History[abar].Count; ptr++) {
					double Price = History[abar][ptr].Price;
					long TotalSize = 0, size=0;
					//s=null;
					if(!TotalsAtPrice.ContainsKey(Price)) {
						for(int i = 0; i<History[abar].Count; i++) {
							if(History[abar][i].Price==Price) {
								if(pCalcVolumeWithoutDirection) size = History[abar][i].Size;
								else size = History[abar][i].Size * Math.Sign(History[abar][i].Location);
								TotalSize += size;
								//s = string.Concat(s," ",i,":",size.ToString());
							}
						}
line=484;
						TotalsAtPrice[Price] = TotalSize;
						if(VolOnEachBar!=null) {
							if(VolOnEachBar.ContainsKey(abar)) {
								long curvol = VolOnEachBar[abar];
								VolOnEachBar[abar] = curvol + TotalSize;
							} else
								VolOnEachBar[abar] = TotalSize;
						}
					}
				}
				foreach(double price in TotalsAtPrice.Keys) {
line=496;
					if(pShowTotalsOnHistorical || DoubleDiamondFound) {
//						s = MakeLabel(ref RenderTarget, abar, price, TotalsAtPrice[price], barSpace, TxtLayout, TxtFormat);
						#region MakeLabel
						string label=null;
						//try{
							label = TotalsAtPrice[price].ToString("0");
							TxtLayout	= new TextLayout(Core.Globals.DirectWriteFactory, label, TxtFormat, ChartPanel.W, TxtFormat.FontSize);
							float x = ChartControl.GetXByBarIndex(ChartBars, abar)-TxtLayout.Metrics.Width-1 - barSpace;
							if(abar==Bars.Count-1) x = x + TxtLayout.Metrics.Width+barSpace*4;
							float y = chartScale.GetYByValue(price) - TxtLayout.Metrics.Height/2f;
							TxtVector	= new SharpDX.Vector2(x, y);
line=508;
							if(!pColorizeVolumeBackgrounds){
								if(WhiteDXBrush!=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), WhiteDXBrush);
								if(BlackDXBrush!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, BlackDXBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
							} else {
								if(TotalsAtPrice[price]>0) {
									if(DotDXBrushAtAsk!=null)  RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), DotDXBrushAtAsk);
									if(TextDXBrushAtAsk!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, TextDXBrushAtAsk, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
								}
								else {
									if(DotDXBrushAtBid!=null)  RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), DotDXBrushAtBid);
									if(TextDXBrushAtBid!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, TextDXBrushAtBid, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
								}
							}
						//}catch(Exception err){Print("MakeLabel:   rbar: "+(CurrentBar-abar).ToString()+NL+err.ToString());}
						#endregion
					} else if(pShowTotalsOnDiamond && DiamondsDict.ContainsKey(abar)){
line=525;
//						s = MakeLabel(ref RenderTarget, abar, price, TotalsAtPrice[price], barSpace, TxtLayout, TxtFormat);
						#region MakeLabel
						string label=null;
						//try{
							label = TotalsAtPrice[price].ToString("0");
							TxtLayout	= new TextLayout(Core.Globals.DirectWriteFactory, label, TxtFormat, ChartPanel.W, TxtFormat.FontSize);
							float x = ChartControl.GetXByBarIndex(ChartBars, abar)-TxtLayout.Metrics.Width-1 - barSpace;
							if(abar==Bars.Count-1) x = x + TxtLayout.Metrics.Width+barSpace*4;
							float y = chartScale.GetYByValue(price) - TxtLayout.Metrics.Height/2f;
							TxtVector	= new SharpDX.Vector2(x, y);
							if(!pColorizeVolumeBackgrounds){
								if(WhiteDXBrush !=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), WhiteDXBrush);
								if(BlackDXBrush !=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, BlackDXBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
							} else {
								if(TotalsAtPrice[price]>0) {
									if(DotDXBrushAtAsk  !=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), DotDXBrushAtAsk);
									if(TextDXBrushAtAsk !=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, TextDXBrushAtAsk, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
								}
								else {
									if(DotDXBrushAtBid  !=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), DotDXBrushAtBid);
									if(TextDXBrushAtBid !=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, TextDXBrushAtBid, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
								}
							}
						//}catch(Exception err){Print("MakeLabel:   rbar: "+(CurrentBar-abar).ToString()+NL+err.ToString());}
						#endregion
					} else if(pShowTotalsOnCurrentBar && abar==CurrentBar){
line=552;
//						s = MakeLabel(ref RenderTarget, abar, price, TotalsAtPrice[price], barSpace, TxtLayout, TxtFormat);
						#region MakeLabel
						string label=null;
						//try{
							label = TotalsAtPrice[price].ToString("0");
							TxtLayout	= new TextLayout(Core.Globals.DirectWriteFactory, label, TxtFormat, ChartPanel.W, TxtFormat.FontSize);
							float x = ChartControl.GetXByBarIndex(ChartBars, abar)-TxtLayout.Metrics.Width-1 - barSpace;
							if(abar==Bars.Count-1) x = x + TxtLayout.Metrics.Width+barSpace*4;
							float y = chartScale.GetYByValue( price) - TxtLayout.Metrics.Height/2f;
							TxtVector	= new SharpDX.Vector2(x, y);
							if(!pColorizeVolumeBackgrounds){
								if(WhiteDXBrush !=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), WhiteDXBrush);
								if(BlackDXBrush !=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, BlackDXBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
							} else {
								if(TotalsAtPrice[price]>0) {
									if(DotDXBrushAtAsk !=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), DotDXBrushAtAsk);
									if(TextBrushAtAsk !=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, TextDXBrushAtAsk, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
								}
								else {
									if(DotDXBrushAtBid !=null) RenderTarget.FillRectangle(new SharpDX.RectangleF(x-1f,y, TxtLayout.Metrics.Width+2f, TxtLayout.Metrics.Height), DotDXBrushAtBid);
									if(TextBrushAtBid !=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, TextDXBrushAtBid, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
								}
							}
						//}catch(Exception err){Print("MakeLabel:   rbar: "+(CurrentBar-abar).ToString()+NL+err.ToString());}
						#endregion
					}
					if(pShowDotsOnHistorical || abar==CurrentBar || DoubleDiamondFound){
line=580;
						if(TotalsAtPrice[price]>0) {
							if(DotDXBrushAtAsk!=null)   RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(ChartControl.GetXByBarIndex(ChartBars,abar)-DotWidth-2, chartScale.GetYByValue(price)-HalfDotWidth),DotWidth,DotWidth), DotDXBrushAtAsk);
						}else if(TotalsAtPrice[price]<0){
							if(DotDXBrushAtBid!=null)   RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(ChartControl.GetXByBarIndex(ChartBars,abar)-DotWidth-2, chartScale.GetYByValue(price)-HalfDotWidth),DotWidth,DotWidth), DotDXBrushAtBid);
						}else{
							if(DotDXBrushBetween!=null) RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(ChartControl.GetXByBarIndex(ChartBars,abar)-DotWidth-2, chartScale.GetYByValue(price)-HalfDotWidth),DotWidth,DotWidth), DotDXBrushBetween);
						}
					}
				}
			}
			if(VolOnEachBar!=null) {
line=591;
				try{
					NetVolTextDXBrush=null;
					foreach(KeyValuePair<string,RectRecord> kvp in RectDict) {
						if(kvp.Value.EndABar >= LMB && kvp.Value.StartABar <= RMB) {
							NinjaTrader.NinjaScript.DrawingTools.Rectangle r = (NinjaTrader.NinjaScript.DrawingTools.Rectangle)DrawObjects[kvp.Key];
							if(r.OutlineStroke.Brush != Brushes.Transparent) 
								NetVolTextDXBrush = r.OutlineStroke.Brush.ToDxBrush(RenderTarget);
							else if(r.AreaBrush != Brushes.Transparent) 
								NetVolTextDXBrush = r.AreaBrush.ToDxBrush(RenderTarget);
							else NetVolTextDXBrush = ChartControl.Properties.AxisPen.Brush.ToDxBrush(RenderTarget);

							long rectsum = 0;
							long barsize = 0;
							for(int i = kvp.Value.StartABar; i<=kvp.Value.EndABar; i++) {
								if(VolOnEachBar.TryGetValue(i,out barsize)) rectsum = rectsum+barsize;
							}
line=608;
							int x = chartControl.GetXByBarIndex(ChartBars, Math.Max(LMB,Math.Min(RMB,kvp.Value.StartABar)));
							int y = chartScale.GetYByValue( kvp.Value.LowPrice);
							string msg = string.Format("Net: {0}",rectsum.ToString());

							TxtLayout	= new TextLayout(Core.Globals.DirectWriteFactory, msg, TxtFormat, ChartPanel.W, TxtFormat.FontSize);
							TxtVector	= new SharpDX.Vector2(x,y);
							if(NetVolTextDXBrush!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, NetVolTextDXBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
							x = chartControl.GetXByBarIndex(ChartBars, Math.Min(RMB,Math.Max(LMB,kvp.Value.EndABar))) - (int)TxtLayout.Metrics.Width;
							y = chartScale.GetYByValue( kvp.Value.HighPrice) - (int)TxtLayout.Metrics.Height-1;
							TxtVector	= new SharpDX.Vector2(x,y);
							if(NetVolTextDXBrush!=null) RenderTarget.DrawTextLayout(TxtVector, TxtLayout, NetVolTextDXBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
line=620;
							if(NetVolTextDXBrush!=null) {NetVolTextDXBrush.Dispose(); NetVolTextDXBrush=null;}
						}
					}
				}catch(Exception rectError){Print(line+":  "+rectError.ToString());}
			}
}catch(Exception e2){Print(line+":  "+e2.ToString());}
		}
//==================================================================================================
		protected override void OnMarketData(MarketDataEventArgs e)
		{
			//Print("OMData "+e.Time.ToString());
			#region OnMarketData
			try{
			now = e.Time;
			if (Bars.IsTickReplay)
			{
				Print(string.Format("{0}  TickReplay  {1}  {2}",e.Time.ToString(), e.Ask, +e.Bid));
			}
			double Price = Instrument.MasterInstrument.RoundDownToTickSize(e.Price);
			if (e.MarketDataType == MarketDataType.Last) {
				askPrice = Instrument.MasterInstrument.RoundDownToTickSize(e.Ask);
				bidPrice = Instrument.MasterInstrument.RoundDownToTickSize(e.Bid);

				long vol = e.Volume;
				sbyte loc = 0;
				if(pAveragingPeriod>0) {
					if(vol>=pSizeFilter) {
						this.RecentBlockSizes.Add(vol);
						double buffer = 0;
						if(RecentBlockSizes.Count>=pAveragingPeriod) {
							while(RecentBlockSizes.Count>pAveragingPeriod) RecentBlockSizes.RemoveAt(0);
							AvgBlockVolume = RecentBlockSizes.Average();
							MinimumBlockVolume = AvgBlockVolume + AvgBlockVolume * (pAveragingBufferPct/100.0);
						}
						CountedTransactions++;
					} else {
						IgnoredTransactions++;
						return;
					}
					if(vol < MinimumBlockVolume) return;
				} else {
					if(vol < pMinBlockVolume) {
						IgnoredTransactions++;
						return;
					} else
						CountedTransactions++;
				}

				//if(pAveragingPeriod<=0 && vol>pHighAlertBlockVolume && !PlaySoundBetweenHA && !PlaySoundAtAskHA && !PlaySoundAtBidHA) return;
				AlertCounter++;
				if(IsFirstTickOfBar) AlertCounter = 0;
				string desc = string.Empty;
				if(Price>askPrice) desc = " above Ask";
				else if(Price<bidPrice) desc = " below Bid";
				else if(Price==askPrice) desc = " at Ask";
				else if(Price==bidPrice) desc = " at Bid";

				bool c1 = Price > askPrice || Price < bidPrice;
				bool c3 = pBlockType == BlockTransactionAlert_BlockType.OutsideOfSpread;

				if(c1 && c3) {
					#region If price is outside of Bid/Ask spread and BlockType == OutsideOfSpread
					loc = (sbyte)(Price>askPrice ? 2:-2);
					if(PlaySoundBetweenHA && vol >= this.pHighAlertBlockVolume) {
						if(loc>0) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.High,string.Format("HA: {0}{1}  of {2}",vol.ToString(), desc, Price), AddSoundFolder(pAskAlertSoundFileHA),0, pDotBrushAtAsk, TextBrushAtAsk);  
						else      Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.High,string.Format("HA: {0}{1}  of {2}",vol.ToString(), desc, Price), AddSoundFolder(pBidAlertSoundFileHA),0, pDotBrushAtBid, TextBrushAtBid);  
					}
					else if(PlaySoundBetween && (!PlaySoundBetweenHA || vol < this.pHighAlertBlockVolume)) {
						if(loc>0) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.Medium,string.Format("{0}{1}  of {2}", vol.ToString(), desc, Price), AddSoundFolder(pAskAlertSoundFile),0, pDotBrushAtAsk, TextBrushAtAsk);
						else      Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.Medium,string.Format("{0}{1}  of {2}", vol.ToString(), desc, Price), AddSoundFolder(pBidAlertSoundFile),0, pDotBrushAtBid, TextBrushAtBid);
					}
					Messages.Insert(0,new MessageRecord(e.Time, loc, vol, Instrument.FullName, Price, Instrument.MasterInstrument.FormatPrice(Price),pMessagesContainTimestamp));
					#endregion
				}
				else {
					#region If price is at or above the ask
					bool c2 = pBlockType == BlockTransactionAlert_BlockType.All;
					c1 = Price >= askPrice;
					c3 = pBlockType == BlockTransactionAlert_BlockType.AsksOnly;
					if(c1 && (c2 || c3)) {
						loc = (sbyte)(Price>askPrice ? 2:1);
						if(PlaySoundAtAskHA && vol >= this.pHighAlertBlockVolume) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.High,string.Format("HA: {0}{1}  of {2}", vol.ToString(), desc, Price), AddSoundFolder(pAskAlertSoundFileHA),0, pDotBrushAtAsk, TextBrushAtAsk);
						else if(PlaySoundAtAsk && (!PlaySoundAtAskHA || vol < this.pHighAlertBlockVolume)) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.Medium,string.Format("{0}{1}  of {2}", vol.ToString(), desc, Price), AddSoundFolder(pAskAlertSoundFile),0, pDotBrushAtAsk, TextBrushAtAsk);
						Messages.Insert(0,new MessageRecord(e.Time, loc, vol, Instrument.FullName, Price, Instrument.MasterInstrument.FormatPrice(Price),pMessagesContainTimestamp));
					}
					#endregion
					else {
						#region If price is at or below the bid
						c1 = Price <= bidPrice;
						c3 = pBlockType == BlockTransactionAlert_BlockType.BidsOnly;
						if(c1 && (c2 || c3)) {
							loc = (sbyte)(Price<bidPrice ? -2:-1);
							if(PlaySoundAtBidHA && vol >= this.pHighAlertBlockVolume) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.High,string.Format("HA: {0}{1}  of {2}", vol.ToString(), desc, Price),AddSoundFolder(pBidAlertSoundFileHA),0, pDotBrushAtBid, TextBrushAtBid);
							else if(PlaySoundAtBid && (!PlaySoundAtBidHA || vol < this.pHighAlertBlockVolume)) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.Medium,string.Format("{0}{1}  of {2}", vol.ToString(), desc, Price),AddSoundFolder(pBidAlertSoundFile),0, pDotBrushAtBid, TextBrushAtBid);
							Messages.Insert(0,new MessageRecord(e.Time, loc, vol, Instrument.FullName, Price, Instrument.MasterInstrument.FormatPrice(Price),pMessagesContainTimestamp));
						}
						#endregion
						else {
							#region If price is at or between the bid and ask prices
							c1 = Price>=bidPrice && Price<=askPrice;
							c3 = pBlockType == BlockTransactionAlert_BlockType.BetweenOnly;
							if(c1 && (c2 || c3)) {
								loc = 0;
								if(PlaySoundBetweenHA && vol >= this.pHighAlertBlockVolume) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.High,string.Format("HA: {0} between at {1}", vol.ToString(), Price),AddSoundFolder(pAlertSoundFileHA),0,Brushes.Cyan,Brushes.Black);
								else if(PlaySoundBetween && (!PlaySoundBetweenHA || vol < this.pHighAlertBlockVolume)) Alert(AlertCounter.ToString(),NinjaTrader.NinjaScript.Priority.Medium,string.Format("{0} between at {2}", vol.ToString(), Price), AddSoundFolder(pAlertSoundFile),0,Brushes.Cyan,Brushes.Black);
								Messages.Insert(0,new MessageRecord(e.Time, loc, vol, Instrument.FullName, Price, Instrument.MasterInstrument.FormatPrice(Price),pMessagesContainTimestamp));
							}
							#endregion
						}
					}
				}
				int abar = Math.Min(Bars.GetBar(e.Time),CurrentBar);
				int rbar = CurrentBar-abar;
				if(!History.ContainsKey(abar)) History[abar] = new List<BlockRecord>();
				History[abar].Add(new BlockRecord(e.Time, loc, Price, vol));
				if(pPrintToOutputWindow && Messages.Count>0) Print(Messages[0].Msg);
			}
			}catch{};//(Exception err){Print("OnMarketData: "+err.ToString());}
			#endregion
		}
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

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
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
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds", wav);
		}
//====================================================================
//===============================================================================
		#region Properties
		#region -- Parameters --
		private BlockTransactionAlert_BlockType pBlockType = BlockTransactionAlert_BlockType.All;
		[Display(Name = "Block Type", GroupName = "Parameters", Description = "Types of block transactions to find...where do they occur?", Order = 5)]
        public BlockTransactionAlert_BlockType BlockType
        {
            get { return pBlockType; }
            set { pBlockType = value; }
        }

		[Display(Name = "Min Block Volume", GroupName = "Parameters", Description = "Min transaction volume for alert, only used if AveragingPeriod = 0", Order = 10)]
        public int MinBlockVolume
        {
            get { return pMinBlockVolume; }
            set { pMinBlockVolume = value;}
        }

		private int pAveragingPeriod = 0;
		[Display(Name = "Averaging Period", GroupName = "Parameters", Description = "Use average block size for calculation of MinBlockVolume?  This is the averaging period, enter '0' to turn-off averaging calculation and use the fixed MinBlockVolume value", Order = 20)]
        public int AveragingPeriod
        {
            get { return pAveragingPeriod; }
            set { pAveragingPeriod = Math.Max(0,value); }
        }

		private long pSizeFilter = 10;
		[Display(Name = "Averaging Size Filter", GroupName = "Parameters", Description = "All blocks below the SizeFilter will be ignored and not calculated in the Average", Order = 30)]
        public long AveragingSizeFilter
        {
            get { return pSizeFilter; }
            set { pSizeFilter = Math.Max(0,value); }
        }

		private double pAveragingBufferPct = 150;
		[Display(Name = "Averaging Buffer %", GroupName = "Parameters", Description = "If AveragingPeriod > 0, this 'AveragingBufferPct' will increase the Average Block Size by X-percent.  120 = 120%", Order = 40)]
        public double AveragingBufferPct
        {
            get { return pAveragingBufferPct; }
            set { pAveragingBufferPct = Math.Max(0,value); }
        }
		#endregion

		#region -- Audible --
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Alert above Ask", GroupName = "Audible", Description = "WAV file for alert when Block is transacted at or above the Ask", Order = 00)]
        public string AskAlertSoundFile
        {
            get { return pAskAlertSoundFile; }
            set { pAskAlertSoundFile = value; }
        }
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Alert below Bid", GroupName = "Audible", Description = "WAV file for alert when Block is transacted at or below the Bid", Order = 00)]
        public string BidAlertSoundFile
        {
            get { return pBidAlertSoundFile; }
            set { pBidAlertSoundFile = value; }
        }
		[Display(Name = "Alert between Bid/Ask", GroupName = "Audible", Description = "WAV file for alert when Block is transacted between the Ask and the Bid", Order = 00)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
        public string AlertSoundFile
        {
            get { return pAlertSoundFile; }
            set { pAlertSoundFile = value; }
        }
		#endregion

		#region -- Audible HighAlert --
		[Display(Name = "HighAlert min block volume", GroupName = "Audible HighAlert", Description = "High alert volume, only used if AveragingPeriod = 0", Order = 00)]
        public int HighAlertBlockVolume
        {
            get { return pHighAlertBlockVolume; }
            set { pHighAlertBlockVolume = value;}
        }
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Alert above Ask", GroupName = "Audible HighAlert", Description = "WAV file for alert when HighAlert Block is transacted at or above the Ask", Order = 10)]
        public string AskAlertSoundFileHA
        {
            get { return pAskAlertSoundFileHA; }
            set { pAskAlertSoundFileHA = value; }
        }
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Alert below Bid", GroupName = "Audible HighAlert", Description = "WAV file for alert when Block is transacted at or below the Bid", Order = 20)]
        public string BidAlertSoundFileHA
        {
            get { return pBidAlertSoundFileHA; }
            set { pBidAlertSoundFileHA = value; }
        }
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Alert between Bid/Ask", GroupName = "Audible HighAlert", Description = "WAV file for alert when Block is transacted between the Bid/Ask", Order = 30)]
        public string AlertSoundFileHA
        {
            get { return pAlertSoundFileHA; }
            set { pAlertSoundFileHA = value; }
        }
		#endregion

		#region -- Custom Visual --
		private int pSecondsBeforeWarningMessage = 15;
		[Display(Name = "# seconds before warning msg", GroupName = "Custom Visual", Description = "Number of seconds before a data shortage warning message is displayed", Order = 900)]
        public int SecondsBeforeWarningMessage
        {
            get { return pSecondsBeforeWarningMessage; }
            set { pSecondsBeforeWarningMessage = Math.Max(1,Math.Min(100000,value)); }
        }

		private float pFontSize = 6;
		[Display(Name = "Font size", GroupName = "Custom Visual", Description = "Font size, in typeface points", Order = 10)]
        public float FontSize
        {
            get { return pFontSize; }
            set { pFontSize = Math.Max(1,value); }
        }
		private bool pColorizeVolumeBackgrounds = true;
		[Display(Name = "Colorize Vol Background?", GroupName = "Custom Visual", Description = "Colorize the background of the on-bar volumes based on Ask or Bid color, otherwise backgrounds will be white", Order = 20)]
        public bool ColorizeVolumeBackgrounds
        {
            get { return pColorizeVolumeBackgrounds; }
            set { pColorizeVolumeBackgrounds = value; }
        }
		private float pDotSize = 2f;
		[Display(Name = "Dot Size", GroupName = "Custom Visual", Description = "Size of the dots on the transaction price", Order = 30)]
        public float DotSize
        {
            get { return pDotSize; }
            set { pDotSize = Math.Max(1,value); }
        }

		private bool pShowSummaryOnRectangle = true;
		[Display(Name = "Show summary on Rectangles?", GroupName = "Custom Visual", Description = "Automatically display Block summary information whenever you draw a rectangle?", Order = 40)]
        public bool ShowSummaryOnRectangle
        {
            get { return pShowSummaryOnRectangle; }
            set { pShowSummaryOnRectangle = value; }
        }

		private bool pShowDotsOnHistorical = true;
		[Display(Name = "Show Dots on historical?", GroupName = "Custom Visual", Description = "Automatically display Block record dots on all historical bars?", Order = 50)]
        public bool ShowDotsOnHistorical
        {
            get { return pShowDotsOnHistorical; }
            set { pShowDotsOnHistorical = value; }
        }
		private bool pShowTotalsOnHistorical = false;
		[Display(Order = 60, Name = "Show Totals on Historical?", GroupName = "Custom Visual", Description = "Automatically display Block total counts on all historical bars?")]
        public bool ShowTotalsOnHistorical
        {
            get { return pShowTotalsOnHistorical; }
            set { pShowTotalsOnHistorical = value; }
        }
		private bool pCalcVolumeWithoutDirection = false;
		[Display(Order = 70, Name = "Calc Vol without Direction?", GroupName = "Custom Visual", Description = "If true, then 'bid' and 'ask' volume will both be positive.  If false, then 'bid' volume is negative and 'ask' volume is positive")]
        public bool CalcVolumeWithoutDirection
        {
            get { return pCalcVolumeWithoutDirection; }
            set { pCalcVolumeWithoutDirection = value; }
        }

		private bool pShowTotalsOnCurrentBar = true;
		[Display(Order = 80, Name = "Show Totals on Current Bar?", GroupName = "Custom Visual", Description = "Show the transaction totals on the current bar?")]
        public bool ShowTotalsOnCurrentBar
        {
            get { return pShowTotalsOnCurrentBar; }
            set { pShowTotalsOnCurrentBar = value; }
        }
		private bool pShowTotalsOnDiamond = true;
		[Display(Order = 90, Name = "Show Totals on Diamonds?", GroupName = "Custom Visual", Description = "Show the transaction totals on any bar that has a Diamond on it?")]
        public bool ShowTotalsOnDiamond
        {
            get { return pShowTotalsOnDiamond; }
            set { pShowTotalsOnDiamond = value; }
        }
		private Brush pDotBrushAtAsk = Brushes.Blue;
		[XmlIgnore()]
		[Display(Order = 100, Name = "Color Dot at Ask", GroupName = "Custom Visual", Description = "Color of the dot when the block transacted at or above the Ask price")]
        public Brush DotBrushAtAsk
        {
            get { return pDotBrushAtAsk; }
            set { pDotBrushAtAsk = value; }
        }
				[Browsable(false)]
				public string DCAASerialize{	get { return Serialize.BrushToString(pDotBrushAtAsk); } set { pDotBrushAtAsk = Serialize.StringToBrush(value); }}

		private Brush pDotBrushAtBid = Brushes.Red;
		[XmlIgnore()]
		[Display(Order = 110, Name = "Color Dot below Bid", GroupName = "Custom Visual", Description = "Color of the dot when the block transacted at or below the Bid price")]
        public Brush DotBrushAtBid
        {
            get { return pDotBrushAtBid; }
            set { pDotBrushAtBid = value; }
        }
				[Browsable(false)]
				public string DCABSerialize{	get { return Serialize.BrushToString(pDotBrushAtBid); } set { pDotBrushAtBid = Serialize.StringToBrush(value); }}

		private Brush pDotBrushBetween = Brushes.Yellow;
		[XmlIgnore()]
		[Display(Order = 120, Name = "Color Dot Between Bid/Ask", GroupName = "Custom Visual", Description = "Color of the dot when the block transacted between the Bid and Ask prices")]
        public Brush DotBrushBetween
        {
            get { return pDotBrushBetween; }
            set { pDotBrushBetween = value; }
        }
				[Browsable(false)]
				public string DCBBASerialize{	get { return Serialize.BrushToString(pDotBrushBetween); } set { pDotBrushBetween = Serialize.StringToBrush(value); }}

		#endregion

		#region -- Messages Visual --
		private bool pColorizeMessages = false;
		[Display(Order = 10, Name = "Colorize Messages", GroupName = "Messages Visual", Description = "Colorize the Messages text based on Ask, Bid, or Between color")]
        public bool ColorizeMessages
        {
            get { return pColorizeMessages; }
            set { pColorizeMessages = value; }
        }
		private TextPosition pMessageLocation = TextPosition.TopLeft;
		[Display(Order = 20, Name = "Message Location", GroupName = "Messages Visual", Description = "Location of messages to printed to the chart")]
        public TextPosition MessageLocation
        {
            get { return pMessageLocation; }
            set { pMessageLocation = value; }
        }

		[Display(Order = 30, Name = "Print to output window?", GroupName = "Messages Visual", Description = "Send a printed record to the Output Window?")]
        public bool PrintToOutputWindow
        {
            get { return pPrintToOutputWindow; }
            set { pPrintToOutputWindow = value; }
        }
		private int pMessageCount = 1;
		[Display(Order = 40, Name = "Message Count", GroupName = "Messages Visual", Description = "Number of messages to print to the chart...enter '0' to disable message printing")]
        public int MessageCount
        {
            get { return pMessageCount; }
            set { pMessageCount = value; }
        }
		private bool pMessagesContainTimestamp = true;
		[Display(Order = 50, Name = "Show Timestamp?", GroupName = "Messages Visual", Description = "Do you want the Messages (on screen) to contain a timestamp?")]
        public bool MessagesContainTimestamp
        {
            get { return pMessagesContainTimestamp; }
            set { pMessagesContainTimestamp = value; }
        }
		private bool pMessagesContainAge = false;
		[Display(Order = 60, Name = "Messages contain age?", GroupName = "Messages Visual", Description = "Do you want the Messages (on screen) to show the age of the transaction (in seconds, then after 5-minutes, it will be shown as mm:ss)?")]
        public bool MessagesContainAge
        {
            get { return pMessagesContainAge; }
            set { pMessagesContainAge = value; }
        }
		#endregion
		#endregion
	}
}
public enum BlockTransactionAlert_BlockType {
	BidsOnly, AsksOnly, BidsAndAsks, BetweenOnly, OutsideOfSpread, All
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BlockTransactionAlert[] cacheBlockTransactionAlert;
		public BlockTransactionAlert BlockTransactionAlert()
		{
			return BlockTransactionAlert(Input);
		}

		public BlockTransactionAlert BlockTransactionAlert(ISeries<double> input)
		{
			if (cacheBlockTransactionAlert != null)
				for (int idx = 0; idx < cacheBlockTransactionAlert.Length; idx++)
					if (cacheBlockTransactionAlert[idx] != null &&  cacheBlockTransactionAlert[idx].EqualsInput(input))
						return cacheBlockTransactionAlert[idx];
			return CacheIndicator<BlockTransactionAlert>(new BlockTransactionAlert(), input, ref cacheBlockTransactionAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BlockTransactionAlert BlockTransactionAlert()
		{
			return indicator.BlockTransactionAlert(Input);
		}

		public Indicators.BlockTransactionAlert BlockTransactionAlert(ISeries<double> input )
		{
			return indicator.BlockTransactionAlert(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BlockTransactionAlert BlockTransactionAlert()
		{
			return indicator.BlockTransactionAlert(Input);
		}

		public Indicators.BlockTransactionAlert BlockTransactionAlert(ISeries<double> input )
		{
			return indicator.BlockTransactionAlert(input);
		}
	}
}

#endregion
