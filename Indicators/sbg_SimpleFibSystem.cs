
// 
// Copyright (C) 2022, SBG Trading Corp.    www.sbgtradingcorp.com
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
using System.Collections.Generic;
using System.Linq;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("Parameters", 10)]
    [CategoryOrder("Consolidations", 20)]
    [CategoryOrder("Visual", 30)]
    [CategoryOrder("Global Lines", 40)]
    public class SimpleFibSystem : Indicator
    {
        #region Variables
			private double         FractalHigh,FractalLow;
			private double         PriorFractalHigh, PriorFractalLow;
 			private List<DateTime> Timep1H,Timep1L;
 			private List<double>   ExtAH, ExtAL;
			private int            i,j,k,iP1H,iP1L,LowsIteration=1,HighsIteration=1, NumOfLines=0;

			private int pMaxBars = 0;
			private double EMPTY = double.MinValue;
			private int FirstABar = 0;
        #endregion
		DateTime expireDT = new DateTime(2025,4,30,0,0,0);

		Brush RacingStripeUp = null;
		Brush RacingStripeDown = null;
		Brush BullishPivotLineBrush = null;
		Brush BearishPivotLineBrush = null;
		private ATR atr;
 		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "sbg SimpleFib System";
				AddPlot(new Stroke(Brushes.Cyan,4),    PlotStyle.Dot,  "Pivot");
				AddPlot(new Stroke(Brushes.Green,3),   PlotStyle.Line, "HighFibA");
				AddPlot(new Stroke(Brushes.Magenta,1), PlotStyle.Line, "HighFibB");
				AddPlot(new Stroke(Brushes.Cyan,3),    PlotStyle.Line, "LowFibA");
				AddPlot(new Stroke(Brushes.Yellow,1),  PlotStyle.Line, "LowFibB");
				AddPlot(new Stroke(Brushes.Lime,5),    PlotStyle.TriangleUp,  "BuyPivot");
				AddPlot(new Stroke(Brushes.Pink,5),    PlotStyle.TriangleDown,  "SellPivot");
				Calculate = Calculate.OnPriceChange;
				IsOverlay = true;
				IsAutoScale = false;

				var ExemptMachines = new List<string>(){
						"B0D2E9D1C802E279D3678D7DE6A33CE4" /*Ben Laptop*/,
						"other",//name and email address of exmpted
					};
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachines.Contains(NinjaTrader.Cbi.License.MachineId);
				if(IsBen)
					expireDT = DateTime.MaxValue;
				else if(expireDT == DateTime.MaxValue)//not me and the expiration date is ignored
					VendorLicense("DayTradersAction", "SbgSimpleFib", "www.sbgtradingcorp.com", "support@sbgtradingcorp.com");

				pConsolLinesColor = Brushes.Cyan;
//				pOnlyOnConfirmationTriangles = true;
				pShowConfirmationTriangles = false;
				pWAVOnBuyDotEntry = "none";
				pWAVOnSellDotEntry = "none";
				pWAVOnConsolidation = "none";
				pWAVOnConsolBreakoutUp = "<inst>_BuyBreakout.wav";
				pWAVOnConsolBreakoutDown = "<inst>_SellBreakout.wav";
				pDrawConsolidations = true;
				pFibAHigh_Template = "Default";
				pFibALow_Template  = "Default";
				pFibBHigh_Template = "Default";
				pFibBLow_Template  = "Default";
				pBullishBreakoutBrush = Brushes.Cyan;
				pBearishBreakoutBrush = Brushes.Brown;
				pBullishBreakoutOpacity = 10;
				pBearishBreakoutOpacity = 10;
				pBullishPivotLineOpacity = 50;
				pBearishPivotLineOpacity = 50;
				pPivotLineWidth = 3;
				pAgedOutColor = Brushes.Bisque;
				pAggressiveSignals = false;
				pSeparationTicks = 3;
				pMinSwingDistanceMult = 3.5;
			}
			if(State == State.Configure){
				Plots[1].DashStyleHelper = DashStyleHelper.Dash;
				Plots[2].DashStyleHelper = DashStyleHelper.Dash;
				Plots[3].DashStyleHelper = DashStyleHelper.Dash;
				Plots[4].DashStyleHelper = DashStyleHelper.Dash;
				IsAutoScale = false;
			}
			if (State == State.DataLoaded)
			{
				atr = ATR(Closes[0],14);
				RacingStripeUp = pBullishBreakoutBrush.Clone();
				RacingStripeUp.Opacity = pBullishBreakoutOpacity/100f;
				RacingStripeUp.Freeze();
				RacingStripeDown = pBearishBreakoutBrush.Clone();
				RacingStripeDown.Opacity = pBearishBreakoutOpacity/100f;
				RacingStripeDown.Freeze();

				if(pBullishPivotLineOpacity>0){
					BullishPivotLineBrush = Plots[5].Brush.Clone();
					BullishPivotLineBrush.Opacity = pBullishPivotLineOpacity/100f;
					BullishPivotLineBrush.Freeze();
				}
				if(pBearishPivotLineOpacity>0){
					BearishPivotLineBrush = Plots[6].Brush.Clone();
					BearishPivotLineBrush.Opacity = pBearishPivotLineOpacity/100f;
					BearishPivotLineBrush.Freeze();
				}

				Timep1H = new List<DateTime>();
				Timep1L = new List<DateTime>();
				ExtAH   = new List<double>();
				ExtAL   = new List<double>();

				var wav = AddSoundFolder(pWAVOnBuyDotEntry);
				bool c1 = pWAVOnBuyDotEntry!="none" && System.IO.File.Exists(wav);
//Print("buy sound: "+wav+"   c1: "+c1.ToString());
				wav = AddSoundFolder(pWAVOnBuyDotEntry);
				bool c2 = pWAVOnSellDotEntry!="none" && System.IO.File.Exists(wav);
//Print("sell sound: "+wav+"   c2: "+c2.ToString());
				IsSoundEnabled = c1 || c2;
			}
		}

		private class DotsInfo{
			public double Price;
			public double ConfirmationPrice = double.MinValue;
			public bool IsConfirmed = false;
			public DotsInfo(double p){Price = p;}
		}
		SortedDictionary<int, DotsInfo> BuyDots = new SortedDictionary<int,DotsInfo>();
		SortedDictionary<int, DotsInfo> SellDots = new SortedDictionary<int,DotsInfo>();

		SortedDictionary<int, double> BuyPivots  = new SortedDictionary<int,double>();
		SortedDictionary<int, double> SellPivots = new SortedDictionary<int,double>();

		bool IsSoundEnabled = false;
		int SellArrowABar = 0;
		int BuyArrowABar = 0;
		double ArrowPrice = 0;
		string arrowtag = "";
		int ConsolidationAlertABar = 0;
		int ConsolidationBOAlertABar = 0;
		private List<int> TagsList = new List<int>();
		protected override void OnBarUpdate()
		{
			FirstABar = pMaxBars==0? 0 : Bars.Count - pMaxBars;
			if(CurrentBar < Math.Max(Significance*3,FirstABar)) return;

			if(State == State.Historical){
				ConsolidationBOAlertABar = CurrentBars[0];
				ConsolidationAlertABar = CurrentBars[0];
				if(Rects.Count>0)
					Rects.Last().Status = 'X';
			}
			int    rBarEndPoint=-1;
			double MajorHigh,MajorLow;
			int    rbarP1;
			string LineName;
			bool   DrawThisExtension=false;
			int    BarToTest = Significance;

			PriorFractalHigh = FractalHigh;				PriorFractalLow = FractalLow;
			FractalHigh = High[BarToTest]+TickSize;		FractalLow = Low[BarToTest]-TickSize;
			if(pAggressiveSignals){
				if(Low[BarToTest+1] < Low[BarToTest]) FractalLow = EMPTY;
				if(Low[BarToTest+2] < Low[BarToTest]) FractalLow = EMPTY;
				if(Low[BarToTest+3] < Low[BarToTest]) FractalLow = EMPTY;
				if(High[BarToTest+1] > High[BarToTest]) FractalHigh = EMPTY;
				if(High[BarToTest+2] > High[BarToTest]) FractalHigh = EMPTY;
				if(High[BarToTest+3] > High[BarToTest]) FractalHigh = EMPTY;
			}
			for(j=Significance;j>0;j--)
			{
				if(!pAggressiveSignals){
					if(Low[BarToTest+j]  < Low[BarToTest])  FractalLow  = EMPTY;
					if(High[BarToTest+j] > High[BarToTest]) FractalHigh = EMPTY;
				}
				if(Low[BarToTest-j]  < Low[BarToTest])  FractalLow  = EMPTY;
				if(High[BarToTest-j] > High[BarToTest]) FractalHigh = EMPTY;
			}

			bool c1 = true;
			if(SellDots.Count>0 && SellDots.Last().Value.Price==FractalLow) c1 = false;
			bool c2 = true;
			if(BuyDots.Count>0 && BuyDots.Last().Value.Price==FractalHigh) c2 = false;
			if(FractalLow != EMPTY && c1)
			{
				#region -- If this new pivot is already a confirmed sell (if price action is above the prior swing high) --
//				int x = 0;
//				if(BuyDots.Count>0) x = BuyDots.Keys.Max()-CurrentBars[0];
//				for(int rbar = 1; rbar > x && BuyDots.Count>0; rbar++){
//					if(Highs[0][rbar] > BuyDots[x]){
//						SellPivot[rbar-1] = FractalLow;
//						break;
//					}
//				}
				#endregion
				//c1 = BuyDots.Count>0 && BuyDots.Last().Value.Price - SellDots.Last().Value.Price > atr[0]*pMinSwingDistanceMult;
				//if(c1)
				{
					Timep1L.Add(Time[BarToTest]);
					//Pivot[BarToTest] = (FractalLow);
					int abar = CurrentBars[0]-BarToTest;
					SellDots[abar] = new DotsInfo(FractalLow);
					if(SellDots.Count>=2){
						SellDots[abar].ConfirmationPrice = Highs[0][BarToTest];
						if(pShowConfirmationTriangles){
							for(int rbar = BarToTest+1; !SellDots.ContainsKey(CurrentBars[0]-rbar); rbar++)
								SellDots[abar].ConfirmationPrice = Math.Max(SellDots[abar].ConfirmationPrice, Highs[0][rbar]);
							for(int rbar = 0; rbar > SellDots.Keys.Max(); rbar++)//when a sell dot is created, it might be immediately confirmed based on price action after the swing bar
								if(Highs[0][rbar] > SellDots[abar].ConfirmationPrice) {
									SellDots[abar].IsConfirmed = true;
									SellPivot[0] = SellDots[abar].Price;
									SellPivots[abar] = SellDots[abar].Price;
								}
						}
					}
					ExtAL.Add(EMPTY);
				}
			} 
			else if(FractalHigh != EMPTY && c2)
			{
				#region -- If this new pivot is already a confirmed buy (if price action is below the prior swing low) --
//				int x = 0;
//				if(SellDots.Count>0) x = SellDots.Keys.Max()-CurrentBars[0];
//				for(int rbar = 1; rbar > x && SellDots.Count>0; rbar++){
//					if(Lows[0][rbar] < SellDots[x]){
//						BuyPivot[rbar-1] = FractalHigh;
//						break;
//					}
//				}
				#endregion
				//c1 = SellDots.Count>0 && BuyDots.Last().Value.Price - SellDots.Last().Value.Price > atr[0]*pMinSwingDistanceMult;
				//if(c1)
				{
					Timep1H.Add(Time[BarToTest]);
					//Pivot[BarToTest] = (FractalHigh);
					int abar = CurrentBars[0]-BarToTest;
					BuyDots[abar] = new DotsInfo(FractalHigh);
					if(BuyDots.Count>=2){
						BuyDots[abar].ConfirmationPrice = Lows[0][BarToTest];
						if(pShowConfirmationTriangles){
							for(int rbar = BarToTest+1; !BuyDots.ContainsKey(CurrentBars[0]-rbar); rbar++)
								BuyDots[abar].ConfirmationPrice = Math.Min(BuyDots[abar].ConfirmationPrice, Lows[0][rbar]);
							for(int rbar = 0; rbar > BuyDots.Keys.Max(); rbar++)//when a buy dot is created, it might be immediately confirmed based on price action after the swing bar
								if(Lows[0][rbar] < BuyDots[abar].ConfirmationPrice) {
									BuyDots[abar].IsConfirmed = true;
									BuyPivot[0] = BuyDots[abar].Price;
									BuyPivots[abar] = BuyDots[abar].Price;
								}
						}
					}
					ExtAH.Add(EMPTY);
				}
			}
			if(IsFirstTickOfBar){
				var keys = BuyDots.Keys.Where(k=>k<CurrentBars[0]-pMaxAgeOfPivots).ToList();
				foreach(var k in keys){
					//Draw.Line(this,keys[i].ToString(), CurrentBars[0]-keys[i], BuyDots[keys[i]], 0, BuyDots[keys[i]],Brushes.Purple);
					PlotBrushes[0][CurrentBars[0]-keys[i]] = pAgedOutColor;
					BuyDots.Remove(k);
				}
				keys = SellDots.Keys.Where(k=>k<CurrentBars[0]-pMaxAgeOfPivots).ToList();
				foreach(var k in keys){
					PlotBrushes[0][CurrentBars[0]-keys[i]] = pAgedOutColor;
					SellDots.Remove(k);
				}
			}
//bool z = Instrument.FullName.StartsWith("MNQ ") && CurrentBars[0] > BarsArray[0].Count-100;
//if(z)Print(Environment.NewLine+ Times[0][0].ToString()+"   BuyDots: "+BuyDots.Count+"  SellDots: "+SellDots.Count);
			#region -- Change pivot dots to BuyPivot or SellPivot signals --
			if(pShowConfirmationTriangles){
				//When market moves below the confirmation price of a Buy dot, then that buy level becomes confirmed as a potential reversal breakout level
				var kvp = BuyDots.Where(p=> !p.Value.IsConfirmed).ToList();
//int unconfirmedcount = kvp==null ? 0 : kvp.Count;
				foreach(var k in kvp){
					if(Highs[0][0] > k.Value.Price) k.Value.IsConfirmed = true;//if it's not confirmed and price moves above the dot, then this dot is invalidated and should never obtain confirmed status
					else if(Lows[0][0] < k.Value.ConfirmationPrice){
						k.Value.IsConfirmed = true;
//						BuyPivot[CurrentBars[0]-k.Key-1] = k.Value.Price;
						BuyPivot[0] = k.Value.Price;
						BuyPivots[CurrentBars[0]] = k.Value.Price;
					}
				}
				//When market moves above the confirmation price of a Sell dot, then that sell level becomes confirmed as a potential reversal breakout level
				kvp = SellDots.Where(p=> !p.Value.IsConfirmed).ToList();
//Print(Times[0][0].ToString()+"  BuyDots count: "+unconfirmedcount+":   SellDots: "+(kvp==null ? 0:kvp.Count));
				foreach(var k in kvp){
					if(Lows[0][0] < k.Value.Price) k.Value.IsConfirmed = true;//if it's not confirmed and price moves below the dot, then this dot is invalidated and should never obtain confirmed status
					else if(Highs[0][0] > k.Value.ConfirmationPrice){
						k.Value.IsConfirmed = true;
//						SellPivot[CurrentBars[0]-k.Key-1] = k.Value.Price;
						SellPivot[0] = k.Value.Price;
						SellPivots[CurrentBars[0]] = k.Value.Price;
					}
				}
			}
			#endregion -----------------------------------------------------

			if(IsSoundEnabled || pDrawMarkersPivotBreaks){
				arrowtag = string.Empty;
				var keys = new List<int>();
				//if(pOnlyOnConfirmationTriangles)
				keys = BuyDots.Keys.ToList();
				for(int i = 0; i<keys.Count; i++){
					if(High[0] >= BuyDots[keys[i]].Price && High[1] <= BuyDots[keys[i]].Price){
						if(State==State.Realtime && IsSoundEnabled) Alert(DateTime.Now.ToString(), Priority.Medium, "SimpleFib Buy level hit at "+Instrument.MasterInstrument.FormatPrice(BuyDots[keys[i]].Price), AddSoundFolder(this.pWAVOnBuyDotEntry), 1, Brushes.Green,Brushes.White);
//Print("Buylevel exceeded at "+BuyDots[keys[i]].ToString());
						BuyDots.Remove(keys[i]);
						BuyArrowABar = CurrentBars[0];
						if(!TagsList.Contains(BuyArrowABar)){
							TagsList.Add(BuyArrowABar);
							arrowtag = $"SFSBuyp {BuyArrowABar}";
							ArrowPrice = Low[0]+TickSize;
							if(pBullishBreakoutOpacity>0) BackBrushes[0] = RacingStripeUp;
						}
					}
				}
				keys = SellDots.Keys.ToList();
				for(int i = 0; i<keys.Count; i++){
					if(Low[0] <= SellDots[keys[i]].Price && Low[1] >= SellDots[keys[i]].Price){
						if(State==State.Realtime && IsSoundEnabled) Alert(DateTime.Now.ToString(), Priority.Medium, "SimpleFib Sell level hit at "+Instrument.MasterInstrument.FormatPrice(SellDots[keys[i]].Price), AddSoundFolder(this.pWAVOnSellDotEntry), 1, Brushes.Green,Brushes.White);
//Print("Selllevel exceeded at "+SellDots[keys[i]].ToString());
						SellDots.Remove(keys[i]);
						SellArrowABar = CurrentBars[0];
						if(!TagsList.Contains(SellArrowABar)){
							TagsList.Add(SellArrowABar);
							arrowtag = $"SFSSellp {SellArrowABar}";
							ArrowPrice = High[0]-TickSize;
							if(pBearishBreakoutOpacity>0) BackBrushes[0] = RacingStripeDown;
						}
					}
				}
				if(pDrawMarkersPivotBreaks){
					if(arrowtag != string.Empty) {
						if(BuyArrowABar == CurrentBars[0] && Low[0] < ArrowPrice){
//	Print(Times[0][0].ToString()+"  Buy arrow");
							try{
								RemoveDrawObject(arrowtag);
								int rbar = CurrentBars[0] - BuyArrowABar;
								ArrowPrice = Low[0];
								Draw.ArrowUp(this, arrowtag, false, 0, ArrowPrice-TickSize*pSeparationTicks, Brushes.Lime);
							}catch{}
						}
						if(SellArrowABar == CurrentBars[0] && High[0] > ArrowPrice){
//	Print(Times[0][0].ToString()+"  Sell arrow");
							try{
								RemoveDrawObject(arrowtag);
								int rbar = CurrentBars[0] - SellArrowABar;
								ArrowPrice = High[0];
								Draw.ArrowDown(this, arrowtag, false, 0, ArrowPrice+TickSize*pSeparationTicks, Brushes.Red);
							}catch{}
						}
					}
				}
			}
			if(Timep1L.Count > pMaxAgeOfPivots) Timep1L.RemoveAt(0);
			if(Timep1H.Count > pMaxAgeOfPivots) Timep1H.RemoveAt(0);
			if(ExtAL.Count   > pMaxAgeOfPivots) ExtAL.RemoveAt(0);
			if(ExtAH.Count   > pMaxAgeOfPivots) ExtAH.RemoveAt(0);

//   Search the ExtAH array for uncompleted P1high
			for (j=0; j<Timep1H.Count; j++)
			{
				DrawThisExtension=false;
				if(ExtAH[j] == EMPTY) //found an uncompleted P1
				{
					rbarP1 = CurrentBar - Bars.GetBar(Timep1H[j]);//iBarShift(0, Timep1H[j], false);
					MajorLow = High[rbarP1];
					c1 = High[rbarP1]-MajorLow > atr[0]*pMinSwingDistanceMult;
					if(c1) Pivot[rbarP1] = MajorLow;
					for(k=rbarP1-1; k>=Math.Max(rbarP1-Math.Min(CurrentBar,pMaxAgeOfPivots),0); k--)
					{
						if(High[k]>High[rbarP1] && !DrawThisExtension) //when prices go higher than P1 level, draw the red (sell) SimpleFib line
						{
							DrawThisExtension = true;
							rBarEndPoint = CurrentBar-Bars.GetBar(Time[k]);//iBarShift(0,Time[k],false);
							break;
						}
						if(Low[k]<MajorLow) MajorLow=Low[k]; //found a new Low to compute the 62% extension
					}
					if(DrawThisExtension)
					{
						if(c1){
							if(fibA!=-999) 
							{
								//LineName="ExtAH_"+Time[rbarP1].ToBinary().ToString();
								LineName = $"ExtAH_{Timep1H[j].ToString()}";
								ExtAH[j]= Instrument.MasterInstrument.RoundToTickSize(High[rbarP1]+fibA*(High[rbarP1]-MajorLow));
								SetTrendline(LineName, rbarP1, ExtAH[j], rBarEndPoint, ExtAH[j], DashStyleHelper.Solid);
							}
							if(fibB!=-999) 
							{
								//LineName="ExtBH_"+Time[rbarP1].ToBinary().ToString();
								LineName = $"ExtBH_{Timep1H[j].ToString()}";
								double eb = Instrument.MasterInstrument.RoundToTickSize(High[rbarP1]+fibB*(High[rbarP1]-MajorLow));
								SetTrendline(LineName,rbarP1,eb,rBarEndPoint,eb,DashStyleHelper.Solid);
							}
						}
						DrawThisExtension=false;
					}
				}
			}
//	Search the ExtAL array for uncompleted P1low
			for (j=0; j<Timep1L.Count; j++)
			{
				DrawThisExtension=false;
				if(ExtAL[j] == EMPTY) //found an uncompleted P1
				{
					rbarP1 = CurrentBar - Bars.GetBar(Timep1L[j]);//iBarShift(0, Timep1L[j], false);
					MajorHigh = Low[rbarP1];
					c1 = MajorHigh-Low[rbarP1] > atr[0]*pMinSwingDistanceMult;
					if(c1) Pivot[rbarP1] = MajorHigh;
					for(k=rbarP1-1; k>=Math.Max(rbarP1-Math.Min(CurrentBar,pMaxAgeOfPivots),0); k--)
					{
						if(Low[k]<Low[rbarP1] && !DrawThisExtension)
						{
							DrawThisExtension = true;
							rBarEndPoint = CurrentBar-Bars.GetBar(Time[k]);//iBarShift(0,Time[k],false);
							break;
						}
						if(High[k]>MajorHigh) MajorHigh=High[k];
					}
					if(DrawThisExtension)
					{
						if(c1){
							if(fibA!=-999) 
							{
								//LineName="ExtAL_"+Time[rbarP1].ToBinary().ToString();
								LineName = $"ExtAL_{Timep1L[j].ToString()}";
								ExtAL[j]= Instrument.MasterInstrument.RoundToTickSize(Low[rbarP1]-fibA*(MajorHigh-Low[rbarP1]));
								SetTrendline(LineName, rbarP1, ExtAL[j], rBarEndPoint, ExtAL[j], DashStyleHelper.Solid);
							}
							if(fibB!=-999) 
							{
								//LineName="ExtBL_"+Time[rbarP1].ToBinary().ToString();
								LineName = $"ExtBL_{Timep1L[j].ToString()}";
								double eb = Instrument.MasterInstrument.RoundToTickSize(Low[rbarP1]-fibB*(MajorHigh-Low[rbarP1]));
								SetTrendline(LineName, rbarP1,eb,rBarEndPoint,eb,DashStyleHelper.Solid);
							}
						}
						DrawThisExtension=false;
					}
				}
			}
			if(IsFirstTickOfBar && pDrawConsolidations && CurrentBars[0]>15){
				double max = MAX(Highs[0],10)[1];
				double min = MIN(Lows[0],10)[1];
				double mid = (max + min)/2;
//				if(CurrentBars[0] > BarsArray[0].Count-100){
//					Draw.Dot(this,"Max"+CurrentBars[0].ToString(),false,1,max,Brushes.White);
//					Draw.Dot(this,"Min"+CurrentBars[0].ToString(),false,1,min,Brushes.White);
//					Draw.Dot(this,"Mid"+CurrentBars[0].ToString(),false,1,mid,Brushes.Cyan);
//				}
				List<double> R = new List<double>(10){Range()[1], Range()[2], Range()[3], Range()[4], Range()[5], Range()[6], Range()[7], Range()[8], Range()[9], Range()[10]};
				double atr = R.Average()*pConsolidationSensitivity/10.0;
				if(mid+atr > max && mid-atr < min){
					if(Rects.Count==0 || Rects.Last().RMaB < CurrentBars[0]-pOverlapBars){
						if(ConsolidationAlertABar < CurrentBars[0]-2 && pWAVOnConsolidation!="none"){
							ConsolidationAlertABar = CurrentBars[0];
							Alert(CurrentBars[0].ToString(),Priority.Medium, "Consolidation box created", AddSoundFolder(pWAVOnConsolidation), 1, Brushes.White, Brushes.Black);
						}
						Rects.Add(new RectInfo(CurrentBars[0]-1, max, CurrentBars[0]-10, min));//Draw.Rectangle(this,"Rect"+CurrentBars[0].ToString(), 1,max, 10, min, Brushes.Yellow);
					}else{
						int lmab = Rects.Last().LMaB;
						max = Math.Min(Rects.Last().MaxPrice, max);
						min = Math.Max(Rects.Last().MinPrice, min);
						//Rects.RemoveAt(Rects.Count-1);
						Rects.Add(new RectInfo(CurrentBars[0]-1, max, lmab, min));
					}
				}
			}
			if(Rects.Count>0 && State == State.Realtime){
				if(pWAVOnConsolBreakoutUp != "none"){
					if(Lows[0][0] > Rects.Last().MaxPrice && Rects.Last().Status !='X'){ 
						if(Rects.Last().Status!='H'){//if it's ' ' or 'L';
							if(ConsolidationBOAlertABar < CurrentBars[0]-2)
								Alert(CurrentBars[0].ToString(),Priority.Medium, "Consolidation break upward", AddSoundFolder(pWAVOnConsolBreakoutUp), 1, Brushes.White, Brushes.Black);
							ConsolidationBOAlertABar = CurrentBars[0];
						}
						if(Rects.Last().Status==' ') Rects.Last().Status = 'H';//if it has no breakout alerts yet, then flag it as having a High breakout
						else if(Rects.Last().Status=='L') Rects.Last().Status = 'X';//if it was low breakout already, then both sides have been signaled, this zone is dead to breakout alerts
					}
				}
				if(pWAVOnConsolBreakoutDown != "none"){
					if(Highs[0][0] < Rects.Last().MinPrice && Rects.Last().Status != 'X'){ 
						if(Rects.Last().Status != 'L'){//if it's ' ' or 'H';
							if(ConsolidationBOAlertABar < CurrentBars[0]-2)
								Alert(CurrentBars[0].ToString(),Priority.Medium, "Consolidation break downward", AddSoundFolder(pWAVOnConsolBreakoutDown), 1, Brushes.White, Brushes.Black);
							ConsolidationBOAlertABar = CurrentBars[0];
						}
						if(Rects.Last().Status==' ') Rects.Last().Status = 'L';//if it has no breakout alerts yet, then flag it as having a High breakout
						else if(Rects.Last().Status=='H') Rects.Last().Status = 'X';//if it was low breakout already, then both sides have been signaled, this zone is dead to breakout alerts
					}
				}
			}
        }
		private class RectInfo{
			public int LMaB = 0;
			public int RMaB = 0;
			public double MaxPrice = 0;
			public double MinPrice = 0;
			public char Status = ' ';
			public RectInfo(int rmab, double max, int lmab, double min){
				RMaB = rmab; LMaB = lmab; MaxPrice=max; MinPrice=min;
			}
			
		}
		private List<RectInfo> Rects = new List<RectInfo>();
//===================================================================

		private struct LineData{
			public int StartABar;
			public int EndABar;
			public double Price;
			public int PlotId;
			public LineData(int startBar, double price, int endBar, int plotId){StartABar=Math.Min(endBar,startBar); EndABar = Math.Max(startBar,endBar); Price=price; PlotId=plotId;}
		}
		private List<LineData> Lines = new List<LineData>();
		private void SetTrendline(string LineName, int StartBar, double Price1, int EndBar, double Price2, DashStyleHelper Style)
		{
			if(pDrawAsTrendLines) {
//				if(pGlobalTrendlines)
//				{
					if(LineName.StartsWith("ExtAH")) {
						if (!ChartControl.Dispatcher.CheckAccess()){
							TriggerCustomEvent(o1 =>{
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibAHigh_Template);},0,null);
						}else
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibAHigh_Template);
					}
					else if(LineName.StartsWith("ExtAL")) {
						if (!ChartControl.Dispatcher.CheckAccess()){
							TriggerCustomEvent(o1 =>{
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibALow_Template);},0,null);
						}else
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibALow_Template);
					}
					else if(LineName.StartsWith("ExtBH")) {
						if (!ChartControl.Dispatcher.CheckAccess()){
							TriggerCustomEvent(o1 =>{
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibBHigh_Template);},0,null);
						}else
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibBHigh_Template);
					}
					else if(LineName.StartsWith("ExtBL")) {
						if (!ChartControl.Dispatcher.CheckAccess()){
							TriggerCustomEvent(o1 =>{
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibBLow_Template);},0,null);
						}else
								Draw.Line(this, LineName, false, (StartBar), Price1, (EndBar), Price1, pGlobalTrendlines, pFibBLow_Template);
					}
//				}else{
//					if(LineName.StartsWith("ExtAH")) {
//						TriggerCustomEvent(o1 =>{
//							Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[1].Brush, Style, (int)Plots[1].Width);},0,null);
//					}
//					else if(LineName.StartsWith("ExtAL")) {
//						TriggerCustomEvent(o1 =>{
//							Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[3].Brush, Style, (int)Plots[3].Width);},0,null);
//					}
//					else if(LineName.StartsWith("ExtBH")) {
//						TriggerCustomEvent(o1 =>{
//							Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[2].Brush, Style, (int)Plots[2].Width);},0,null);
//					}
//					else if(LineName.StartsWith("ExtBL")) {
//						TriggerCustomEvent(o1 =>{
//							Draw.Line(this, LineName, false, StartBar, Price1, EndBar, Price2, Plots[4].Brush, Style, (int)Plots[4].Width);},0,null);
//					}
//				}
			} else {
				if(StartBar > EndBar) {
					int temp = EndBar;
					EndBar = StartBar;
					StartBar = temp;
				}
				if(LineName.StartsWith("ExtAH")) {
					Lines.Add(new LineData(CurrentBar-StartBar, Price1, CurrentBar-EndBar, 1));
//					if(PivotHighFibA[1] == Price1) PivotHighFibA[0] = (Price1);
//					else {
//						for (int i=StartBar; i<=EndBar; i++) PivotHighFibA[i] = (Price1);
//					}
				}
				else if(LineName.StartsWith("ExtAL")) {
					Lines.Add(new LineData(CurrentBar-StartBar, Price1, CurrentBar-EndBar, 3));
//					if(PivotLowFibA[1] == Price1) PivotLowFibA[0] = (Price1);
//					else {
//						for (int i=StartBar; i<=EndBar; i++) PivotLowFibA[i] = (Price1);
//					}
				}
				else if(LineName.StartsWith("ExtBH")) {
					Lines.Add(new LineData(CurrentBar-StartBar, Price1, CurrentBar-EndBar, 2));
//					if(PivotHighFibB[1] == Price1) PivotHighFibB[0] = (Price1);
//					else {
//						for (int i=StartBar; i<=EndBar; i++) PivotHighFibB[i] = (Price1);
//					}
				}
				else if(LineName.StartsWith("ExtBL")) {
					Lines.Add(new LineData(CurrentBar-StartBar, Price1, CurrentBar-EndBar, 4));
//					if(PivotLowFibB[1] == Price1) PivotLowFibB[0] = (Price1);
//					else {
//						for (int i=StartBar; i<=EndBar; i++) PivotLowFibB[i] = (Price1);
//					}
				}
			}
		}
//===================================================================
		private SharpDX.Direct2D1.Brush BullishPivotLineBrushDX = null;
		private SharpDX.Direct2D1.Brush BearishPivotLineBrushDX = null;
		private SharpDX.Direct2D1.Brush boxBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(BullishPivotLineBrushDX != null){
				if(!BullishPivotLineBrushDX.IsDisposed) BullishPivotLineBrushDX.Dispose();
				BullishPivotLineBrushDX=null;	
			}
			if(BullishPivotLineBrush==null && pBullishPivotLineOpacity>0){
				BullishPivotLineBrush = Plots[5].Brush.Clone();
				BullishPivotLineBrush.Opacity = pBullishPivotLineOpacity/100f;
				BullishPivotLineBrush.Freeze();
			}
			if(RenderTarget != null && BullishPivotLineBrush!=null) BullishPivotLineBrushDX = BullishPivotLineBrush.ToDxBrush(RenderTarget);

			if(BearishPivotLineBrushDX != null){
				if(!BearishPivotLineBrushDX.IsDisposed) BearishPivotLineBrushDX.Dispose();
				BearishPivotLineBrushDX=null;
			}
			if(BearishPivotLineBrush==null && pBearishPivotLineOpacity>0){
				BearishPivotLineBrush = Plots[6].Brush.Clone();
				BearishPivotLineBrush.Opacity = pBearishPivotLineOpacity/100f;
				BearishPivotLineBrush.Freeze();
			}
			if(RenderTarget != null && BearishPivotLineBrush!=null) BearishPivotLineBrushDX = BearishPivotLineBrush.ToDxBrush(RenderTarget);

			if(boxBrushDX != null){
				if(!boxBrushDX.IsDisposed) boxBrushDX.Dispose();
				boxBrushDX = null;
			}if(RenderTarget != null) boxBrushDX = pConsolLinesColor.ToDxBrush(RenderTarget);
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
            #region -- conditions to return --
            if (!IsVisible || ChartBars.ToIndex < BarsRequiredToPlot) return;
            if (Bars == null || ChartControl == null) return;
            if (ChartBars.FromIndex == -1 || ChartBars.ToIndex == -1) return;
			if (IsInHitTest) return;
            #endregion
			SharpDX.Direct2D1.AntialiasMode OSM = RenderTarget.AntialiasMode;

			float x, y, x1, y1;
			SharpDX.Vector2 v1, v2;

			base.OnRender(chartControl, chartScale);
			v1 = new SharpDX.Vector2(0,0);
			v2 = new SharpDX.Vector2(0,0);
			if(BuyPivots.Count>0 && BullishPivotLineBrush!=Brushes.Transparent && pBullishPivotLineOpacity>0){
				#region -- draw bullish pivot lines --
				x = 0; y = 0; x1 = 0; y1 = 0;
				int count = 0;
				foreach(var piv in BuyPivots.Where(k=>k.Key>ChartBars.FromIndex-50)){
//Print(count+"  Buy Piv: "+piv.Key+"  "+piv.Value+"   RT!=null: "+(RenderTarget!=null).ToString()+"  BrushDX!=null: "+(BullishPivotLineBrushDX!=null).ToString());
					if(count>=2 && RenderTarget!=null && BullishPivotLineBrushDX!=null){
//Print("     drawing it");
						v1.Y = v2.Y;
						RenderTarget.DrawLine(v1, v2, BullishPivotLineBrushDX, pPivotLineWidth);
					}
					x1 = x;
					y1 = y;
					v2.X = x;
					v2.Y = y;
					x = chartControl.GetXByBarIndex(ChartBars, piv.Key);
					y = Convert.ToSingle(chartScale.GetYByValue(piv.Value));
					v1.X = x;
					v1.Y = y;
					count++;
				}
				if(BullishPivotLineBrushDX != null){
					if(count>=2 && RenderTarget!=null){
						v1.Y = v2.Y;
						RenderTarget.DrawLine(v1, v2, BullishPivotLineBrushDX, pPivotLineWidth);
					}
					if(BuyPivots.Count>0){
						#region -- draw a horizontal line from the last pivot marker, to the right side of the chart --
						v1.X = chartControl.GetXByBarIndex(ChartBars, BuyPivots.Last().Key);
						v1.Y = Convert.ToSingle(chartScale.GetYByValue(BuyPivots.Last().Value));
						v2.Y = v1.Y;
						v2.X = ChartPanel.W;
						RenderTarget.DrawLine(v1, v2, BullishPivotLineBrushDX, pPivotLineWidth);//horizontal line from last pivot triangle
						#endregion
					}
				}
				#endregion
				#region -- draw bearish pivot lines --
				x = 0; y = 0; x1 = 0; y1 = 0;
				count = 0;
				foreach(var piv in SellPivots.Where(k=>k.Key>ChartBars.FromIndex-50)){
//Print("Sell Piv: "+piv.Key+"  "+piv.Value);
					if(count>=2 && RenderTarget!=null && BearishPivotLineBrushDX!=null){
						v1.Y = v2.Y;
						RenderTarget.DrawLine(v1, v2, BearishPivotLineBrushDX, pPivotLineWidth);
					}
					x1 = x;
					y1 = y;
					v2.X = x;
					v2.Y = y;
					x = chartControl.GetXByBarIndex(ChartBars, piv.Key);
					y = Convert.ToSingle(chartScale.GetYByValue(piv.Value));
					v1.X = x;
					v1.Y = y;
					count++;
				}
				if(BearishPivotLineBrushDX != null){
					if(count>=2 && RenderTarget!=null){
						v1.Y = v2.Y;
						RenderTarget.DrawLine(v1, v2, BearishPivotLineBrushDX, pPivotLineWidth);
					}
					if(SellPivots.Count>0){
						#region -- draw a horizontal line from the last pivot marker, to the right side of the chart --
						v1.X = chartControl.GetXByBarIndex(ChartBars, SellPivots.Last().Key);
						v1.Y = Convert.ToSingle(chartScale.GetYByValue(SellPivots.Last().Value));
						v2.Y = v1.Y;
						v2.X = ChartPanel.W;
						RenderTarget.DrawLine(v1, v2, BearishPivotLineBrushDX, pPivotLineWidth);//horizontal line from last pivot triangle
						#endregion
					}
				}
				#endregion
			}
			if(Lines==null || Lines.Count==0) return;
			var L = Lines.Where(k=>k.EndABar > ChartBars.FromIndex && k.StartABar < ChartBars.ToIndex);
			if(L==null) return;
			foreach(var line in L){
				y = Convert.ToSingle(chartScale.GetYByValue(line.Price));
				x = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, line.StartABar));
				x1 = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, line.EndABar));
				v1.X = x;
				v1.Y = y;
				v2.X = x1;
				v2.Y = y;
				if(RenderTarget!=null)
					RenderTarget.DrawLine(v1, v2, Plots[line.PlotId].BrushDX, Plots[line.PlotId].Width, Plots[line.PlotId].StrokeStyle);
			}

			if(pConsolLinesColor != Brushes.Transparent && pConsolidationSensitivity>0 && pDrawConsolidations && boxBrushDX!=null){
				var rects = Rects.Where(k=>k.LMaB <= ChartBars.ToIndex && k.RMaB >= ChartBars.FromIndex && k.MaxPrice >= chartScale.MinValue && k.MinPrice <= chartScale.MaxValue).ToList();
				if(rects!=null && rects.Count>0){
//if(rects!=null) Print("rects.Count?: "+rects.Count+"  brushDX==null: "+(boxBrushDX==null).ToString());
//					var tFormat = font.ToDirectWriteTextFormat();
//					var tLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, Instrument.MasterInstrument.FormatPrice(Closes[0][0]), zonePricesTextFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
//					v1 = new SharpDX.Vector2(0f,0f);
//					v2 = new SharpDX.Vector2(0f,0f);
					for(int i = 0; i<rects.Count; i++){
						x1 = chartControl.GetXByBarIndex(ChartBars, rects[i].RMaB);//rmab
						x  = chartControl.GetXByBarIndex(ChartBars, rects[i].LMaB);//lmab
						y  = Convert.ToSingle(chartScale.GetYByValue(rects[i].MaxPrice));//max price
						y1 = Convert.ToSingle(chartScale.GetYByValue(rects[i].MinPrice));//min price
//						RenderTarget.DrawRectangle(new SharpDX.RectangleF(x,y, x1-x, y1-y), boxBrushDX);
						boxBrushDX.Opacity = 1f;
						v1.X = x;
						v1.Y = y;
						v2.X = x1;
						v2.Y = y;
						if(RenderTarget!=null)
							RenderTarget.DrawLine(v1, v2, boxBrushDX);
						v1.Y = y1;
						v2.Y = y1;
						if(RenderTarget!=null)
							RenderTarget.DrawLine(v1, v2, boxBrushDX);
						boxBrushDX.Opacity = 0.1f;
						v1.X = x;
						v1.Y = y;
						v2.X = x;
						v2.Y = y1;
						if(RenderTarget!=null)
							RenderTarget.DrawLine(v1, v2, boxBrushDX);
						v1.X = x1;
						v2.X = x1;
						if(RenderTarget!=null)
							RenderTarget.DrawLine(v1, v2, boxBrushDX);
					}
				}
			}
            RenderTarget.AntialiasMode = OSM;
		}
//===================================================================

		internal class LoadRayTemplates : StringConverter
		{
			#region LoadRayTemplates
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
				string[] paths = new string[4]{NinjaTrader.Core.Globals.UserDataDir,"templates","DrawingTool","Line"};
//				HLtemplates_folder = ;
				string search = "*.xml";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(System.IO.Path.Combine(paths));
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("Default");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						string name = fi.Name.Replace(".xml",string.Empty);
						if(!list.Contains(name)){
							list.Add(name);
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

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> BuyPivot
        { get { return Values[5]; } }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> SellPivot
        { get { return Values[6]; } }
		#endregion
		
		#region Properties
        private int  pSignificance = 6; // Default setting for Significance
		//private int  linewidth = 2;
		private bool pDrawAsTrendLines = false;
		private int  pMaxAgeOfPivots= 50;
        private double fibA = 0.618; // Default setting for FibA
        private double fibB = 1.618; // Default setting for FibB

		[Display(Order=10,    Name="Max age of pivots", GroupName="Parameters", Description="Max age (in bars) for valid pivots - we'll ignore any pivots older than this age")]
        public int MaxAgeOfPivots
        {
            get { return pMaxAgeOfPivots; }
            set { pMaxAgeOfPivots = Math.Max(1, value); }
        }
		[Display(Order=20,    Name="Swing significance", GroupName="Parameters", Description="Minimum strength or significance of pivots to use in calculations")]
        public int Significance
        {
            get { return pSignificance; }
            set { pSignificance = Math.Max(1, value); }
        }

		[Display(Order=25,    Name="Min ATR mult", GroupName="Parameters", Description="")]
		public double pMinSwingDistanceMult {get;set;}

		[Display(Order=30,    Name="Fib A pct", GroupName="Parameters", Description="First fibonacci extension percentage")]
        public double FibA
        {
            get { return fibA; }
            set { fibA = value; }
        }

		[Display(Order=40,    Name="Fib B pct", GroupName="Parameters", Description="Second fibonacci extension percentage")]
        public double FibB
        {
            get { return fibB; }
            set { fibB = value; }
        }

		[Display(Order=50,    Name="Aggressive Signals", GroupName="Parameters", Description="Increase the number of pivots by reducing the fractal pivot calculation to only 3 bars prior to the pivot bar")]
		public bool pAggressiveSignals {get;set;}

		[Display(Order=60,    Name="Confirmation Triangles?", GroupName="Parameters", Description="Confirmed swings are swings where price moved far away from them to surpass the prior swing value")]
		public bool pShowConfirmationTriangles {get;set;}

		[Display(Order=10,    Name="Draw as Trendlines?", GroupName="Visual", Description="")]
        public bool DrawAsTrendLines
        {
            get { return pDrawAsTrendLines; }
            set { pDrawAsTrendLines = value; }
        }

		[Display(Order=20,    Name="Max bars lookback", GroupName="Visual", Description="Start bar, as measured back from the current bar, set to '0' to contain all bars on chart")]
        public int MaxBars
        {
            get { return pMaxBars; }
            set { pMaxBars = Math.Max(0, value); }
        }

		private int pConsolidationSensitivity = 11;
		[Display(Order=30,    Name="Consolidation Sensitivity", GroupName="Visual", Description="Sensitivity of consolidation rectangles.  Low numbers mean more rectangles.  Set to '0' to turn off all rectangles")]
        public int ConsolidationSensitivity
        {
            get { return pConsolidationSensitivity; }
            set { pConsolidationSensitivity = Math.Max(0, value); }
        }

		[XmlIgnore]
		[Display(Order = 60, Name = "Aged out color", GroupName = "Racing Stripes", Description="Color of pivot dots that are older than the 'Max age of pivots' setting")]
		public Brush pAgedOutColor
		{ get; set; }
				[Browsable(false)]
				public string pAgedOutColorSerializable { get { return Serialize.BrushToString(pAgedOutColor); } set { pAgedOutColor = Serialize.StringToBrush(value); }        }

		private bool pGlobalTrendlines = true;
		[Display(Order = 5, Name = "Globalize trendlines?", GroupName = "Global Lines", ResourceType = typeof(Custom.Resource), Description="Good for mult-chart analysis, trendlines will be global")]
		public bool GlobalTrendlines
		{
            get { return pGlobalTrendlines; }
            set { pGlobalTrendlines = value; }
		}
		[Display(Order = 10, Name = "FibAHigh Template", GroupName = "Global Lines", ResourceType = typeof(Custom.Resource))]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadRayTemplates))]
		public string pFibAHigh_Template { get; set; }

		[Display(Order = 20, Name = "FibBHigh Template", GroupName = "Global Lines", ResourceType = typeof(Custom.Resource))]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadRayTemplates))]
		public string pFibBHigh_Template { get; set; }

		[Display(Order = 30, Name = "FibALow Template", GroupName = "Global Lines", ResourceType = typeof(Custom.Resource))]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadRayTemplates))]
		public string pFibALow_Template { get; set; }

		[Display(Order = 40, Name = "FibBLow Template", GroupName = "Global Lines", ResourceType = typeof(Custom.Resource))]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadRayTemplates))]
		public string pFibBLow_Template { get; set; }

		#region -- PivotLines --
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish opacity", GroupName = "Pivot Lines", Order = 20)]
		public int pBullishPivotLineOpacity {get;set;}

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish opacity", GroupName = "Pivot Lines", Order = 40)]
		public int pBearishPivotLineOpacity {get;set;}
		
		[Range(1, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line width", GroupName = "Pivot Lines", Order = 50)]
		public int pPivotLineWidth {get;set;}
		#endregion

		#region -- Racing Stripes --
		[XmlIgnore]
		[Display(Order = 10, Name = "Bullish breakout", GroupName = "Racing Stripes")]
		public Brush pBullishBreakoutBrush
		{ get; set; }
				[Browsable(false)]
				public string BullishBreakoutBrushSerializable { get { return Serialize.BrushToString(pBullishBreakoutBrush); } set { pBullishBreakoutBrush = Serialize.StringToBrush(value); }        }

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bullish opacity", GroupName = "Racing Stripes", Order = 20)]
		public int pBullishBreakoutOpacity {get;set;}

		[XmlIgnore]
		[Display(Order = 30, Name = "Bearish breakout", GroupName = "Racing Stripes")]
		public Brush pBearishBreakoutBrush
		{ get; set; }
				[Browsable(false)]
				public string BearishBreakoutBrushSerializable { get { return Serialize.BrushToString(pBearishBreakoutBrush); } set { pBearishBreakoutBrush = Serialize.StringToBrush(value); }        }

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bearish opacity", GroupName = "Racing Stripes", Order = 40)]
		public int pBearishBreakoutOpacity {get;set;}
		#endregion
		#region -- Consolidations parameters --
		private bool pDrawConsolidations = false;
		[Display(Order=10,    Name="Draw Consolidations?", GroupName="Consolidations", Description="Draws the horizontal lines demarking a consolidation zone")]
        public bool DrawConsolidations
        {
            get { return pDrawConsolidations; }
            set { pDrawConsolidations = value; }
        }
		[XmlIgnore]
        [Display(Order = 20,    Name = "Consolidation Lines Color", GroupName = "Consolidations", Description = "")]
        public Brush pConsolLinesColor { get; set; }
        [Browsable(false)]
        public string ConsolLinesColorSerialize
        {
            get { return Serialize.BrushToString(pConsolLinesColor); }
            set { pConsolLinesColor = Serialize.StringToBrush(value); }
        }
		private int pOverlapBars = 5;
		[Display(Order=30,    Name="Consolidation Overlap (bars)", GroupName="Consolidations", Description="")]
		public int OverlapBars
        {
            get { return pOverlapBars; }
            set { pOverlapBars = Math.Max(0, value); }
        }
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 40, ResourceType = typeof(Custom.Resource), Name = "Consolidation created", GroupName = "Consolidations")]
		public string pWAVOnConsolidation {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 50, ResourceType = typeof(Custom.Resource), Name = "Consolidation B.O. up", GroupName = "Consolidations")]
		public string pWAVOnConsolBreakoutUp {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 60, ResourceType = typeof(Custom.Resource), Name = "Consolidation B.O. down", GroupName = "Consolidations")]
		public string pWAVOnConsolBreakoutDown {get;set;}
		#endregion

		#region -- Alerts --
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav.Replace("<inst>",Instrument.MasterInstrument.Name));
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
				string dir = NinjaTrader.Core.Globals.InstallDir;
//				string dir = WAVDirectory;
//				if(dir.Trim().Length==0) dir = NinjaTrader.Core.Globals.InstallDir;
//				if(dir.ToLower().Contains("<default>")) dir = NinjaTrader.Core.Globals.InstallDir;
				string folder = System.IO.Path.Combine(dir, "sounds");
				string search = "*.wav";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
				list.Add("<inst>_BuyBreakout.wav");
				list.Add("<inst>_SellBreakout.wav");
				list.Add("<inst>_Consolidation.wav");
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

//		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Alerts only on confirmed triangles", GroupName = "Alerts")]
//		public bool pOnlyOnConfirmationTriangles {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Wav on Buy Dot Hit", GroupName = "Alerts")]
		public string pWAVOnBuyDotEntry {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Wav on Sell Dot Hit", GroupName = "Alerts")]
		public string pWAVOnSellDotEntry {get;set;}

		[Display(Order = 40, Name = "Draw entry arrows?", Description = "Draw arrows when price breaks a pivot level", GroupName = "Alerts")]
		public bool pDrawMarkersPivotBreaks {get;set;}

		[Display(Order = 450, Name = "Separation ticks", GroupName = "Alerts", Description="Distance between bar and entry arrow")]
		public int pSeparationTicks {get;set;}

		#endregion
		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleFibSystem[] cacheSimpleFibSystem;
		public SimpleFibSystem SimpleFibSystem()
		{
			return SimpleFibSystem(Input);
		}

		public SimpleFibSystem SimpleFibSystem(ISeries<double> input)
		{
			if (cacheSimpleFibSystem != null)
				for (int idx = 0; idx < cacheSimpleFibSystem.Length; idx++)
					if (cacheSimpleFibSystem[idx] != null &&  cacheSimpleFibSystem[idx].EqualsInput(input))
						return cacheSimpleFibSystem[idx];
			return CacheIndicator<SimpleFibSystem>(new SimpleFibSystem(), input, ref cacheSimpleFibSystem);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleFibSystem SimpleFibSystem()
		{
			return indicator.SimpleFibSystem(Input);
		}

		public Indicators.SimpleFibSystem SimpleFibSystem(ISeries<double> input )
		{
			return indicator.SimpleFibSystem(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleFibSystem SimpleFibSystem()
		{
			return indicator.SimpleFibSystem(Input);
		}

		public Indicators.SimpleFibSystem SimpleFibSystem(ISeries<double> input )
		{
			return indicator.SimpleFibSystem(input);
		}
	}
}

#endregion
