#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
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
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    [Description("")]
    public class OneTwoThree : Indicator
    {

        #region Variables
        // Wizard generated variables
			private int maximumBars = 20; // Default setting for Strength
			private int minimumTicksBetween1and3 = 0;
		  	private int displayOffset = 2; // Default setting for TimeEnd
			private int pixelOffset = 10; // Default setting for TimeEnd
			private int strength = 1;
			private NinjaTrader.Gui.Tools.SimpleFont textFont1 = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 8);
			private NinjaTrader.Gui.Tools.SimpleFont textFont2 = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 8);
			private NinjaTrader.Gui.Tools.SimpleFont textFont3 = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 8);
			private Brush textBrushTops = Brushes.Red;
			private Brush textBrushBottoms = Brushes.Cyan;
		
			private ArrayList	lastLowCache;
			private ArrayList	lastHighCache;
			private List<int>	pivotLows;			
			private List<int>	pivotHighs;
			private Dictionary<int,int>	ValidPt1LowBars, ValidPt1HighBars;
			private double minimumPtsBetween1and3=0;
        // User defined variables (add any user defined variables below)
		
			private double	currentSwingLow = 0;	
			private double	currentSwingHigh = 0;
			private int		countLow = 0;	
			private int		countHigh = 0;
			private int		countPivots = 0;
			private double 	balance = 0;
			
			private bool	timeReady = false;
		
		// variables for long patterns

		// variables for short patterns

			int BullishGroup = 0;
			int BearishGroup = 0;
		
        #endregion
		private Dictionary<int,int> Pt1LLocs, Pt2LLocs;	//Key is the Group number, value is the AbsBar
		private Dictionary<int,int> Pt1HLocs, Pt2HLocs;	//Key is the Group number, value is the AbsBar
		private List<int[]> Finished123Bottoms, Finished123Tops;
		private string NL = Environment.NewLine;
		private bool TooFarApart = false;
		private bool Pt1beforePt2 = false;
		private bool Pt2beforePt3 = false;
		private bool Pt1belowPt2  = false;
		private bool Pt1beforePt3 = false;
		private bool RunInit = true;
		private bool IsBen = false;
		private int ABar_RecentBuy = -1;
		private int ABar_RecentSell = -1;

private DateTime T1 = new DateTime(2011,1,11,12,40,0);
private DateTime T2 = new DateTime(2011,1,11,13,0,0);
private bool InZone = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				var IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", "AIOneTwoThree", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				AddPlot(new Stroke(Brushes.Transparent,2), PlotStyle.Dot, "Swing High");
				AddPlot(new Stroke(Brushes.Transparent,2), PlotStyle.Dot, "Swing Low");
				AddPlot(new Stroke(Brushes.Lime,1),    PlotStyle.TriangleUp, "BuySignals");
				AddPlot(new Stroke(Brushes.Magenta,1), PlotStyle.TriangleDown, "SellSignals");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "Age");

				IsOverlay=true;
				IsAutoScale=false;
				Name = "iw OneTwoThree";
			}
			if (State == State.Configure)
			{
			}
			if (State == State.DataLoaded)
			{
				minimumPtsBetween1and3 = minimumTicksBetween1and3*TickSize;
				lastLowCache  = new ArrayList();
				lastHighCache = new ArrayList();
				pivotLows     = new List<int>(); //Contains the absbar of the low pivots
				pivotHighs    = new List<int>(); //Contains the absbar of the high pivots
				ValidPt1LowBars    = new Dictionary<int,int>();
				ValidPt1HighBars   = new Dictionary<int,int>();
				Finished123Bottoms = new List<int[]>();
				Finished123Tops    = new List<int[]>();
				Pt1LLocs = new Dictionary<int,int>();
				Pt2LLocs = new Dictionary<int,int>();
				Pt1HLocs = new Dictionary<int,int>();
				Pt2HLocs = new Dictionary<int,int>();
			}
		}
        protected override void OnBarUpdate()
        {
try{
            // Store the minimum highs necessary to determine if a high or low pivot was formed

			lastLowCache.Add(Low[0]);
			if (lastLowCache.Count > (2 * strength) + 1)
				lastLowCache.RemoveAt(0);

			lastHighCache.Add(High[0]);
			if (lastHighCache.Count > (2 * strength) + 1)
				lastHighCache.RemoveAt(0);


				timeReady = true;


// Find and store swing low and high pivots based on strength

			if (CurrentBar < (2 * strength))  // Start once we have enough data in lastHighCache / lastLowCache
				return;

			if (timeReady == true)
			{
				#region Identify pivot bars
				if (countHigh == 0)		// get and store all qualifying high pivots, by strength
				{						// strength is defined as x bars with an equal or lower high to the left and right
					bool isSwingHigh = false;
					if (High[1] > High[0])
					{
						double h = High[1];
						int i = 2;
						do
						{
							if(High[1] > High[i])	
							isSwingHigh = true;
							i = i + 1;
						}
						while (i<CurrentBar && h == High[i-1]);
					}

					if (isSwingHigh)
					{
						currentSwingHigh = High[strength];
						SwingHighPlot[strength] = (currentSwingHigh + TickSize*displayOffset);
						pivotHighs.Insert(0, CurrentBar-strength);
						#region Find Valid Pt1
						bool curvedprices = false;
						bool ValidPt1 = true;
						for(int bb = 2; bb<Math.Min(CurrentBar-1,15); bb++) {
							if(High[bb]<currentSwingHigh) curvedprices = true;
							if(curvedprices && High[bb]==currentSwingHigh) {
								ValidPt1 = false;
							} else if(High[bb]>currentSwingHigh) ValidPt1 = false;
						}
						if(ValidPt1) {
							ValidPt1HighBars[CurrentBar-strength]=0;
//	Draw.Dot(this, "D"+CurrentBar.ToString(), false, strength, High[strength]-TickSize, Color.Pink);
						}
						#endregion
						countHigh = strength;
						isSwingHigh = false;
					}
				}
				else
				{
					countHigh = countHigh - 1;
				}

				if (countLow == 0)    	// get and store all qualifying low pivots, by strength
				{						// strength is defined as x bars with an equal or higher low to the left and right
					bool isSwingLow = false;
					if (Low[1] < Low[0])
					{
						double l = Low[1];
						int i = 2;
						do
						{
							if(Low[1] < Low[i])	
							isSwingLow = true;
							i = i + 1;
						}
						while (i<CurrentBar && l == Low[i-1]);
					}

					if (isSwingLow)
					{
						currentSwingLow = Low[strength];
						SwingLowPlot[strength] = (currentSwingLow - TickSize*displayOffset);
						pivotLows.Insert(0,CurrentBar-strength);
						#region Find Valid Pt1
						bool curvedprices = false;
						bool ValidPt1 = true;
						for(int bb = 2; bb<Math.Min(CurrentBar-1,15); bb++) {
							if(Low[bb]>currentSwingLow) curvedprices = true;
							if(curvedprices && Low[bb]==currentSwingLow) {
								ValidPt1 = false;
							} else if(Low[bb]<currentSwingLow) ValidPt1 = false;
						}
						if(ValidPt1) {
							ValidPt1LowBars[CurrentBar-strength]=0;
//	Draw.Dot(this, "D"+CurrentBar.ToString(), false, strength, Low[strength]+TickSize, Color.Yellow);
						} else {
//	Draw.Dot(this, "D"+CurrentBar.ToString(), false, strength, Low[strength]+TickSize, Color.Navy);
						}
						#endregion
						countLow = strength;
						isSwingLow = false;
					}
				}
				else countLow = countLow - 1;
				#endregion

				if(IsFirstTickOfBar) {
					ValidPt1LowBars.Remove(CurrentBar-maximumBars);
					ValidPt1HighBars.Remove(CurrentBar-maximumBars);
				}
				List<int> key = new List<int>(ValidPt1LowBars.Keys);
//Print("- - - - - - -");
				if(pViewBottoms) {
					foreach(int b1a in key) {
						int b1r = CurrentBar-b1a;
		//Print("b1: "+Time[b1r].ToString()+"    "+pivotHighs.Count);
						if(Low[0]<Low[b1r] + minimumPtsBetween1and3)        ValidPt1LowBars.Remove (b1a);
						else if(pivotHighs.Count>0) {
							Pt1LLocs[b1a] = b1a;
							int b2r = CurrentBar-pivotHighs[0];
		//Print("b2: "+Time[b2r].ToString());
							int b = b2r;
		//					int b2 = CurrentBar-b1a-1;
		//					if(b2<0) continue;
							double p2 = High[b2r];
		//if(b1r>b2r) Draw.Square(this, "S"+(CurrentBar-b2r).ToString(), false, b2r, p2,Color.Red);
							int b3r = b2r-1;
							if(b3r<=0) continue; else Pt2HLocs[b1a]=CurrentBar-b2r;
							double p3 = Low[b3r];
							b = b3r-1;
							while(b>=0) {
								if(Low[b]<=p3) {p3=Low[b]; b3r=b;}
								b--;
							}
		//if(b2r>=0 && b3r>=0) Print(Time[0].ToString()+"  B2: "+Time[b2r].ToString()+"  B3: "+Time[b3r].ToString());
		//else if(b2r>=0) Print(Time[0].ToString()+"  B2: "+Time[b2r].ToString());
							if(b1r>b2r && b2r>=b3r && High[0]>p2) {
		//Print("Adding to Finished123Bottoms  "+Time[b1r].ToString());
								if(Finished123Bottoms.Count==0) {
//====================================================================
									Finished123Bottoms.Insert(0,new int[3]{b1a, CurrentBar-b2r, CurrentBar-b3r});
									Alert((CurrentBar-b3r).ToString(),Priority.High,Name+" bottom found at "+Time[b3r].ToString(), AddSoundFolder(pBottomWAV), 1, Brushes.Black, Brushes.Lime);  
									BuySignals[0] = (Low[0]);
									ABar_RecentBuy = CurrentBar;
//BackColor=Color.Green;
									if(pSendEmails) SendMail(pEmailAddress,"BUY signal 1-2-3 "+Instrument.FullName+"("+Bars.BarsPeriod.ToString()+")",Name+" bottom found at "+Time[b3r].ToString());
								}
								else if(Finished123Bottoms[0][0]!=b1a && Finished123Bottoms[0][1]!=CurrentBar-b2r && Finished123Bottoms[0][2]!=CurrentBar-b3r) {
									Finished123Bottoms.Insert(0,new int[3]{b1a, CurrentBar-b2r, CurrentBar-b3r});
//BackColor=Color.Green;
									Alert((CurrentBar-b3r).ToString(),Priority.High,Name+" bottom found at "+Time[b3r].ToString(), AddSoundFolder(pBottomWAV), 1, Brushes.Black, Brushes.Lime); 
									BuySignals[0] = (Low[0]);
									ABar_RecentBuy = CurrentBar;
									if(pSendEmails) SendMail(pEmailAddress,"BUY signal 1-2-3 "+Instrument.FullName+"("+Bars.BarsPeriod.ToString()+")",Name+" bottom found at "+Time[b3r].ToString());
								}
							}
						}
					}
				}
				if(pViewTops) {
					key = new List<int>(ValidPt1HighBars.Keys); //AbsBar, Price
					foreach(int b1a in key) {
						int b1r = CurrentBar-b1a;
						if(High[0]>High[b1r] - minimumPtsBetween1and3)        ValidPt1HighBars.Remove (b1a);
						else if(pivotLows.Count>0) {
							Pt1HLocs[b1a] = b1a;
							int b2r = CurrentBar-pivotLows[0];
							int b = b2r;
//if(InZone) Print("  Pt2L: "+Time[b2r].ToString());
							double p2 = Low[b2r];
							int b3r = b2r-1;
							if(b3r<=0) continue; else Pt2LLocs[b1a]=CurrentBar-b2r;
							double p3 = High[b3r];
//if(InZone) Print("  b1r: "+b1r+" b2r: "+b2r+" b3r: "+b3r+"   p3: "+p3+"  Low[0]: "+Low[0]+"  < p2: "+p2);
							b = b3r-1;
							while(b>=0) {
								if(High[b]>=p3) {p3=High[b]; b3r=b;}
								b--;
							}

							if(b1r>b2r && b2r>=b3r && Low[0]<p2) {
								if(Finished123Tops.Count==0) {
									Finished123Tops.Insert(0, new int[3]{b1a, CurrentBar-b2r, CurrentBar-b3r});
									Alert((CurrentBar-b3r).ToString(),Priority.High,Name+" top found at "+Time[b3r].ToString(), AddSoundFolder(pTopWAV), 1, Brushes.Black, Brushes.Magenta);  
//BackColor=Color.Red;
									SellSignals[0] = (High[0]);
									ABar_RecentSell = CurrentBar;
									if(pSendEmails) SendMail(pEmailAddress,"SELL signal 1-2-3 "+Instrument.FullName+"("+Bars.BarsPeriod.ToString()+")",Name+" top found at "+Time[b3r].ToString());
								}
								else if(Finished123Tops[0][0]!=b1a && Finished123Tops[0][1]!=CurrentBar-b2r && Finished123Tops[0][2]!=CurrentBar-b3r) {
									Finished123Tops.Insert(0, new int[3]{b1a, CurrentBar-b2r, CurrentBar-b3r});
//BackColor=Color.Red;
									Alert((CurrentBar-b3r).ToString(),Priority.High,Name+" top found at "+Time[b3r].ToString(), AddSoundFolder(pTopWAV), 1, Brushes.Black, Brushes.Magenta);  
									SellSignals[0] = (High[0]);
									ABar_RecentSell = CurrentBar;
									if(pSendEmails) SendMail(pEmailAddress,"SELL signal 1-2-3 "+Instrument.FullName+"("+Bars.BarsPeriod.ToString()+")",Name+" top found at "+Time[b3r].ToString());
								}
							}
						}
					}
				}
				if(pMarkerOnEntrySignal != OneTwoThree_MarkerType.None){
					if(BuySignals.IsValidDataPoint(0)){
						double p = Low[0]-pSeparation*TickSize;
						if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Triangle)  Draw.TriangleUp(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[2].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Arrow)   Draw.ArrowUp(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[2].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Diamond) Draw.Diamond(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[2].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Dot)         Draw.Dot(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[2].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Square)   Draw.Square(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[2].Brush);
					}
					if(SellSignals.IsValidDataPoint(0)){
						double p = High[0]+pSeparation*TickSize;
						if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Triangle) Draw.TriangleDown(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[3].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Arrow)  Draw.ArrowDown(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[3].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Diamond)  Draw.Diamond(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[3].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Dot)          Draw.Dot(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[3].Brush);
						else if(pMarkerOnEntrySignal == OneTwoThree_MarkerType.Square)    Draw.Square(this, "123_"+CurrentBar.ToString(),false,0,p,Plots[3].Brush);
					}
				}
				int age = int.MaxValue;
				if(ABar_RecentBuy > ABar_RecentSell) {
					age = CurrentBar - ABar_RecentBuy;
				}
				if(ABar_RecentSell > ABar_RecentBuy) {
					age = ABar_RecentSell - CurrentBar;
				}
				if(Math.Abs(age)<=pMaximumAge) {
					Age[0] = (age);
				} 
				else Age[0] = (0);

			}
}catch(Exception err){Print("Error: "+err.ToString());}
        }
//==============================================================================================
//		private static bool FindAllIntegers(int a, int b){
//			return a==b;
//		}
//==============================================================================================
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds",wav);
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			if (!IsVisible) return;double min = chartScale.MinValue; double max = chartScale.MaxValue;
			base.OnRender(chartControl, chartScale);
			Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
			Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = ChartBars.ToIndex;

int line = 390;
			//			base.Plot(graphics, bounds, min, max);

			if(ChartControl==null) return;
			SharpDX.DirectWrite.TextFormat txtFormat1 = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	textFont1.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)textFont1.Size);
			SharpDX.DirectWrite.TextFormat txtFormat2 = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	textFont2.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)textFont2.Size);
			SharpDX.DirectWrite.TextFormat txtFormat3 = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	textFont3.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)textFont3.Size);
			//var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "", txtFormat, ChartPanel.W, txtFormat.FontSize);
			var txtPosition = new SharpDX.Vector2();
try{
			int absbar1H=0;
			int absbar2H=0;
			int absbar3H=0;
			int absbar1L=0;
			int absbar2L=0;
			int absbar3L=0;
			List<int> Pt1Hprints, Pt2Hprints, Pt3Hprints;
			List<int> Pt1Lprints, Pt2Lprints, Pt3Lprints;

			Pt1Hprints=new List<int>();
			Pt2Hprints=new List<int>();
			Pt3Hprints=new List<int>();
			Pt1Lprints=new List<int>();
			Pt2Lprints=new List<int>();
			Pt3Lprints=new List<int>();

			foreach(int[] x in Finished123Bottoms) {
				Pt1Lprints.Add(x[0]);
				Pt2Hprints.Add(x[1]);
				Pt3Lprints.Add(x[2]);
			}
			foreach(int[] x in Finished123Tops) {
				Pt1Hprints.Add(x[0]);
				Pt2Lprints.Add(x[1]);
				Pt3Hprints.Add(x[2]);
			}
			for(int b = firstBarPainted; b<lastBarPainted; b++) {
				if(Pt1LLocs.TryGetValue(b, out absbar1L)) Pt1Lprints.Add(absbar1L);
				if(Pt2LLocs.TryGetValue(b, out absbar2L)) Pt2Lprints.Add(absbar2L);
				if(Pt1HLocs.TryGetValue(b, out absbar1H)) Pt1Hprints.Add(absbar1H);
				if(Pt2HLocs.TryGetValue(b, out absbar2H)) Pt2Hprints.Add(absbar2H);

				string txtH1 = string.Empty;
				string txtH2 = string.Empty;
				string txtH3 = string.Empty;
				string txtL1 = string.Empty;
				string txtL2 = string.Empty;
				string txtL3 = string.Empty;

				if(Pt1Hprints.Contains(b)) txtH1 = "1";
				if(Pt2Lprints.Contains(b)) txtL2 = "2";
				if(Pt3Hprints.Contains(b)) txtH3 = "3";

				if(Pt1Lprints.Contains(b)) txtL1 = "1";
				if(Pt2Hprints.Contains(b)) txtH2 = "2";
				if(Pt3Lprints.Contains(b)) txtL3 = "3";

				int stackedcount = 0;
				if(txtH1.Length>0) {
					var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txtH1, txtFormat1, (int)(ChartPanel.W*0.8), txtFormat1.FontSize);
					int halfchar = (int)txtLayout.Metrics.Width/2;
					float x = chartControl.GetXByBarIndex(ChartBars,b)-halfchar;
					float y = chartScale.GetYByValue(High.GetValueAt(b))-txtLayout.Metrics.Height-pixelOffset-txtLayout.Metrics.Height*stackedcount;
					#region DrawText and background fill
					txtPosition = new SharpDX.Vector2(x,y);
					if(pBackFill_1s) {
						var rectangleF = new SharpDX.RectangleF(txtPosition.X-1, txtPosition.Y-1, txtLayout.Metrics.Width+2, txtLayout.Metrics.Height+2);
						RenderTarget.FillRectangle(rectangleF, textColorTopsBrush.ToDxBrush(RenderTarget));
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget));
					}else
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, textColorTopsBrush.ToDxBrush(RenderTarget));
					txtLayout.Dispose();txtLayout=null;
					#endregion
					stackedcount++;
				}
				if(txtH2.Length>0) {
					var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txtH2, txtFormat2, (int)(ChartPanel.W*0.8), txtFormat2.FontSize);
					int halfchar = (int)txtLayout.Metrics.Width/2;
					float x = chartControl.GetXByBarIndex(ChartBars,b)-halfchar;
					float y = chartScale.GetYByValue(High.GetValueAt(b))-txtLayout.Metrics.Height-pixelOffset-txtLayout.Metrics.Height*stackedcount;
					#region DrawText
					txtPosition = new SharpDX.Vector2(x,y);
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, textColorBottomsBrush.ToDxBrush(RenderTarget));
					txtLayout.Dispose();txtLayout=null;
					#endregion
					stackedcount++;
				}
				if(txtH3.Length>0) {
					var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txtH3, txtFormat3, (int)(ChartPanel.W*0.8), txtFormat3.FontSize);
					int halfchar = (int)txtLayout.Metrics.Width/2;
					float x = chartControl.GetXByBarIndex(ChartBars,b)-halfchar;
					float y = chartScale.GetYByValue(High.GetValueAt(b))-txtLayout.Metrics.Height-pixelOffset-txtLayout.Metrics.Height*stackedcount;
					#region DrawText and background fill
					txtPosition = new SharpDX.Vector2(x,y);
					if(pBackFill_3s) {
						var rectangleF = new SharpDX.RectangleF(txtPosition.X-1, txtPosition.Y-1, txtLayout.Metrics.Width+2, txtLayout.Metrics.Height+2);
						RenderTarget.FillRectangle(rectangleF, textColorTopsBrush.ToDxBrush(RenderTarget));
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget));
					}else
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, textColorTopsBrush.ToDxBrush(RenderTarget));
					txtLayout.Dispose();txtLayout=null;
					#endregion
				}

				stackedcount = 0;
				if(txtL1.Length>0) {
					var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txtL1, txtFormat1, (int)(ChartPanel.W*0.8), txtFormat1.FontSize);
					int halfchar = (int)txtLayout.Metrics.Width/2;
					float x = chartControl.GetXByBarIndex(ChartBars,b)-halfchar;
					float y = chartScale.GetYByValue(Low.GetValueAt(b))+pixelOffset+txtLayout.Metrics.Height*stackedcount;
					txtPosition = new SharpDX.Vector2(x,y);
					#region DrawText and background fill
					if(pBackFill_1s) {
						var rectangleF = new SharpDX.RectangleF(txtPosition.X-1, txtPosition.Y-1, txtLayout.Metrics.Width+2, txtLayout.Metrics.Height+2);
						RenderTarget.FillRectangle(rectangleF, textColorBottomsBrush.ToDxBrush(RenderTarget));
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget));
					}else
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, textColorBottomsBrush.ToDxBrush(RenderTarget));
					txtLayout.Dispose();txtLayout=null;
					#endregion
					stackedcount++;
				}
				if(txtL2.Length>0) {
					var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txtL2, txtFormat2, (int)(ChartPanel.W*0.8), txtFormat2.FontSize);
					int halfchar = (int)txtLayout.Metrics.Width/2;
					float x = chartControl.GetXByBarIndex(ChartBars,b)-halfchar;
					float y = chartScale.GetYByValue(Low.GetValueAt(b))+pixelOffset+txtLayout.Metrics.Height*stackedcount;
					txtPosition = new SharpDX.Vector2(x,y);
					#region DrawText
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, textColorTopsBrush.ToDxBrush(RenderTarget));
					txtLayout.Dispose();txtLayout=null;
					#endregion
					stackedcount++;
				}
				if(txtL3.Length>0) {
					var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, txtL3, txtFormat3, (int)(ChartPanel.W*0.8), txtFormat3.FontSize);
					int halfchar = (int)txtLayout.Metrics.Width/2;
					float x = chartControl.GetXByBarIndex(ChartBars,b)-halfchar;
					float y = chartScale.GetYByValue(Low.GetValueAt(b))+pixelOffset+txtLayout.Metrics.Height*stackedcount;
					txtPosition = new SharpDX.Vector2(x,y);
					#region DrawText and background fill
					if(pBackFill_3s) {
						var rectangleF = new SharpDX.RectangleF(txtPosition.X-1, txtPosition.Y-1, txtLayout.Metrics.Width+2, txtLayout.Metrics.Height+2);
						RenderTarget.FillRectangle(rectangleF, textColorBottomsBrush.ToDxBrush(RenderTarget));
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget));
					}else
						RenderTarget.DrawTextLayout(txtPosition, txtLayout, textColorBottomsBrush.ToDxBrush(RenderTarget));
					txtLayout.Dispose();txtLayout=null;
					#endregion
				}
			}
}catch(Exception perr){Print(line+"  Plot error: "+perr.ToString());}
			if(txtFormat1!=null) {
				txtFormat1.Dispose();
				txtFormat1 = null;
			}
			if(txtFormat2!=null) {
				txtFormat2.Dispose();
				txtFormat2 = null;
			}
			if(txtFormat3!=null) {
				txtFormat3.Dispose();
				txtFormat3 = null;
			}
		}
//==============================================================================================
		public override string ToString() {
			return Name;
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

		#region Plots
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SwingHighPlot{get { return Values[0]; }}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SwingLowPlot {get { return Values[1]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BuySignals {get { return Values[2]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SellSignals {get { return Values[3]; }}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Age {get { return Values[4]; }}

		#endregion

		private bool pSendEmails = false;
		[Description("Send an email on each arrow signal?")]
		[Category("Alert")]
		public bool EmailEnabled
		{
			get { return pSendEmails; }
			set { pSendEmails = value; }
		}
		private string pEmailAddress = "";
		[Description("Enter a valid destination email address to receive an email on signals")]
		[Category("Alert")]
		public string EmailAddress
		{
			get { return pEmailAddress; }
			set { pEmailAddress = value; }
		}

		private string pBottomWAV = "none";
		[Description("Sound file name for a 123 Bottom...must be a valid WAV file in your Sounds folder")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Alert")]
		public string BottomWAV {
			get { return pBottomWAV; }
			set { pBottomWAV = value; }
		}

		private string pTopWAV = "none";
		[Description("Sound file name for a 123 Top...must be a valid WAV file in your Sounds folder")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Alert")]
		public string TopWAV {
			get { return pTopWAV; }
			set { pTopWAV = value; }
		}

		#region Properties
		private int pMaximumAge = 5;
		[Description("Max age of a signal displayed in the 'Age' plot")]
		[Category("Parameters")]
		public int MaximumAge
		{
			get { return pMaximumAge ; }
			set { pMaximumAge = Math.Max(0, value); }
		}
		
		[Description("Number of bars permitted between #1 and #3 points")]
		[Category("Parameters")]
		public int MaximumBars 
		{
			get { return maximumBars ; }
			set { maximumBars = Math.Max(1, value); }
		}
		
		[Description("this is the closest that the #1 and #3 points can come.  Anything closer than that disqualifies the signal")]
		[Category("Parameters")]
		public int MinimumTicksBetween1and3  
		{
			get { return minimumTicksBetween1and3  ; }
			set { minimumTicksBetween1and3  = Math.Max(0, value); }
		}

		private bool pViewTops = true;
		[Description("Show top 1-2-3 formations")]
		[Category("Parameters")]
		public bool ViewTops
		{
			get { return pViewTops; }
			set { pViewTops = value; }
		}
		private bool pViewBottoms = true;
		[Description("Show bottom 1-2-3 formations")]
		[Category("Parameters")]
		public bool ViewBottoms
		{
			get { return pViewBottoms; }
			set { pViewBottoms = value; }
		}
		#endregion

		private OneTwoThree_MarkerType pMarkerOnEntrySignal = OneTwoThree_MarkerType.None;
		[Description("Type of chart marker to print on the entry bar (BuySignals / SellSignals)")]
		[Category("MarkersOnSignals")]
		public OneTwoThree_MarkerType MarkerOnEntrySignal {
			get { return pMarkerOnEntrySignal; }
			set { pMarkerOnEntrySignal = value; }
		}

		private int pSeparation = 1;
		[Description("Number of ticks between the price bar and the MarkerOnEntrySignal markers")]
		[Category("MarkersOnSignals")]
		public int Separation {
			get { return pSeparation; }
			set { pSeparation = value; }
		}

		#region Visual
		[Description("The amount, in ticks, by which to offset the markers from the pivots.")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMarkers",  GroupName = "Visual")]
		public int DisplayOffset
		{
			get { return displayOffset; }
			set { displayOffset = Math.Max(0, value); }
		}
		
		[Description("The amount, in pixels, by which to offset the numbers from the pivots.")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetNumbers",  GroupName = "Visual")]
		public int PixelOffset
		{
			get { return pixelOffset; }
			set { pixelOffset = Math.Max(0, value); }
		}

		[Description("Choose your font style for the numbers displayed")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text Font #1",  GroupName = "Visual")]
		public NinjaTrader.Gui.Tools.SimpleFont TextFont1
		{
			get { return textFont1; }
			set { textFont1 = value; }
		}
		[Description("Choose your font style for the numbers displayed")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text Font #2",  GroupName = "Visual")]
		public NinjaTrader.Gui.Tools.SimpleFont TextFont2
		{
			get { return textFont2; }
			set { textFont2 = value; }
		}
		[Description("Choose your font style for the numbers displayed")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text Font #3",  GroupName = "Visual")]
		public NinjaTrader.Gui.Tools.SimpleFont TextFont3
		{
			get { return textFont3; }
			set { textFont3 = value; }
		}


		private bool pBackFill_1s = false;
		[Description("Fill the #1's with color")]
		[Category("Visual")]
		public bool BackFill_1s
		{
			get { return pBackFill_1s; }
			set { pBackFill_1s = value; }
		}
		private bool pBackFill_3s = true;
		[Description("Fill the #3's with color")]
		[Category("Visual")]
		public bool BackFill_3s
		{
			get { return pBackFill_3s; }
			set { pBackFill_3s = value; }
		}
		private Brush textColorTopsBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of Bar Numbers")]
		[Category("Visual")]
//		[Gui.Design.DisplayName("Text Color")]
		public Brush TextBrushTops
		{
			get { return textColorTopsBrush; }
			set { textColorTopsBrush = value; }
		}
				[Browsable(false)]
				public string TextColorTopsS
				{
					get { return Serialize.BrushToString(textColorTopsBrush); }
					set { textColorTopsBrush = Serialize.StringToBrush(value); }
				}

		private Brush textColorBottomsBrush = Brushes.Cyan;
		[XmlIgnore()]
		[Description("Color of Bar Numbers")]
		[Category("Visual")]
//		[Gui.Design.DisplayName("Text Color")]
		public Brush TextBrushBottoms
		{
			get { return textColorBottomsBrush; }
			set { textColorBottomsBrush = value; }
		}
				[Browsable(false)]
				public string TextColorBottomsS
				{
					get { return Serialize.BrushToString(textColorBottomsBrush); }
					set { textColorBottomsBrush = Serialize.StringToBrush(value); }
				}

		
		
        #endregion
    }
}
public enum OneTwoThree_MarkerType{None,Arrow,Triangle,Dot,Diamond,Square}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OneTwoThree[] cacheOneTwoThree;
		public OneTwoThree OneTwoThree()
		{
			return OneTwoThree(Input);
		}

		public OneTwoThree OneTwoThree(ISeries<double> input)
		{
			if (cacheOneTwoThree != null)
				for (int idx = 0; idx < cacheOneTwoThree.Length; idx++)
					if (cacheOneTwoThree[idx] != null &&  cacheOneTwoThree[idx].EqualsInput(input))
						return cacheOneTwoThree[idx];
			return CacheIndicator<OneTwoThree>(new OneTwoThree(), input, ref cacheOneTwoThree);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OneTwoThree OneTwoThree()
		{
			return indicator.OneTwoThree(Input);
		}

		public Indicators.OneTwoThree OneTwoThree(ISeries<double> input )
		{
			return indicator.OneTwoThree(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OneTwoThree OneTwoThree()
		{
			return indicator.OneTwoThree(Input);
		}

		public Indicators.OneTwoThree OneTwoThree(ISeries<double> input )
		{
			return indicator.OneTwoThree(input);
		}
	}
}

#endregion
