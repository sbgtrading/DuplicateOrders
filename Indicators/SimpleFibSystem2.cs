
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
using System.Collections.Generic;
using System.Linq;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("Parameters", 10)]
    [CategoryOrder("Visual", 20)]
    [CategoryOrder("Global Lines", 30)]
    public class SimpleFibSystem2 : Indicator
    {
        #region Variables
			private double         FractalHigh,FractalLow;
			private double         PriorFractalHigh, PriorFractalLow;
// 			private List<DateTime> Timep1H,Timep1L;
// 			private List<double>   ExtAH, ExtAL;
			private int            i,j,k,iP1H,iP1L,LowsIteration=1,HighsIteration=1, NumOfLines=0;

			private int pMaxBars = 0;
			private double EMPTY = double.MinValue;
			private int FirstABar = 0;
        #endregion

		Brush RacingStripeUp = null;
		Brush RacingStripeDown = null;
 		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "SimpleFib System 2";
				AddPlot(new Stroke(Brushes.Blue,3),    PlotStyle.Dot,  "Pivot");
				AddPlot(new Stroke(Brushes.Green,3),   PlotStyle.Line, "HighFibA");
				AddPlot(new Stroke(Brushes.Magenta,1), PlotStyle.Line, "HighFibB");
				AddPlot(new Stroke(Brushes.Blue,3),    PlotStyle.Line, "LowFibA");
				AddPlot(new Stroke(Brushes.Yellow,1),  PlotStyle.Line, "LowFibB");
				Calculate = Calculate.OnPriceChange;
				IsOverlay = true;
				pBoxColor = Brushes.Cyan;
				pWAVOnBuySetup = "none";
				pWAVOnSellSetup = "none";
				pWAVOnConsolidation = "none";
				pDrawConsolidations = false;
				pFibAHigh_Template = "Default";
				pFibALow_Template  = "Default";
				pFibBHigh_Template = "Default";
				pFibBLow_Template  = "Default";
				pBullishBreakoutBrush = Brushes.Cyan;
				pBearishBreakoutBrush = Brushes.Brown;
				pBullishBreakoutOpacity = 10;
				pBearishBreakoutOpacity = 10;
				pAgedOutColor = Brushes.Bisque;
				pAggressiveSignals = false;
				pSeparationTicks = 3;
			}
			if(State == State.Configure){
				Plots[1].DashStyleHelper = DashStyleHelper.Dash;
				Plots[2].DashStyleHelper = DashStyleHelper.Dash;
				Plots[3].DashStyleHelper = DashStyleHelper.Dash;
				Plots[4].DashStyleHelper = DashStyleHelper.Dash;
			}
			if (State == State.DataLoaded)
			{
				RacingStripeUp = pBullishBreakoutBrush.Clone();
				RacingStripeUp.Opacity = pBullishBreakoutOpacity/100f;
				RacingStripeUp.Freeze();
				RacingStripeDown = pBearishBreakoutBrush.Clone();
				RacingStripeDown.Opacity = pBearishBreakoutOpacity/100f;
				RacingStripeDown.Freeze();

//				Timep1H = new List<DateTime>();
//				Timep1L = new List<DateTime>();
//				ExtAH   = new List<double>();
//				ExtAL   = new List<double>();

				var wav = AddSoundFolder(pWAVOnBuySetup);
				bool c1 = pWAVOnBuySetup!="none" && System.IO.File.Exists(wav);
//Print("buy sound: "+wav+"   c1: "+c1.ToString());
				wav = AddSoundFolder(pWAVOnBuySetup);
				bool c2 = pWAVOnSellSetup!="none" && System.IO.File.Exists(wav);
//Print("sell sound: "+wav+"   c2: "+c2.ToString());
				IsSoundEnabled = c1 || c2;
			}
		}

		private class Setup{
			public double SwingPrice = 0;
			public double CurrentExtentPrice = 0;
			public int ConfirmABar = -1;
			public double Lvl1 = double.MinValue;
			public double Lvl2 = double.MinValue;
			public Setup(double swingPrice, double currentExtentPrice){
				SwingPrice = swingPrice;
				CurrentExtentPrice = currentExtentPrice;
			}
		}
		SortedDictionary<int, Setup> BuyDots = new SortedDictionary<int, Setup>();
		SortedDictionary<int, Setup> SellDots = new SortedDictionary<int, Setup>();
		bool IsSoundEnabled = false;
		int SellArrowABar = 0;
		int BuyArrowABar = 0;
		double ArrowPrice = 0;
		string arrowtag = "";
		int ConsolidationAlertABar = 0;
		protected override void OnBarUpdate()
		{
			FirstABar = pMaxBars==0? 0 : Bars.Count - pMaxBars;
			if(CurrentBar < Math.Max(Significance*3,FirstABar)) return;

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
			if(SellDots.Count>0 && SellDots.Last().Value.SwingPrice == FractalLow) c1 = false;
			bool c2 = true;
			if(BuyDots.Count>0 && BuyDots.Last().Value.SwingPrice == FractalHigh) c2 = false;
			if(FractalLow != EMPTY && c1)
			{
//				Timep1L.Add(Time[BarToTest]);
				Pivot[BarToTest] = (FractalLow);
				SellDots[CurrentBars[0]-BarToTest] = new Setup(FractalLow, MAX(Highs[0], BarToTest)[0]);
				//ExtAL.Add(EMPTY);
			} 
			else if(FractalHigh != EMPTY && c2)
			{
//				Timep1H.Add(Time[BarToTest]);
				Pivot[BarToTest] = (FractalHigh);
				BuyDots[CurrentBars[0]-BarToTest] = new Setup(FractalHigh, MIN(Lows[0], BarToTest)[0]);
				//ExtAH.Add(EMPTY);
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
			if(IsSoundEnabled || pDrawMarkersPivotBreaks){
				var keys = BuyDots.Keys.ToList();
				for(int i = 0; i<keys.Count; i++){
					if(High[0] >= BuyDots[keys[i]].SwingPrice && High[1] <= BuyDots[keys[i]].SwingPrice){
						if(State==State.Realtime && IsSoundEnabled) Alert(DateTime.Now.ToString(), Priority.Medium, "SimpleFib Buy level hit at "+Instrument.MasterInstrument.FormatPrice(BuyDots[keys[i]].SwingPrice), AddSoundFolder(this.pWAVOnBuySetup), 1, Brushes.Green,Brushes.White);
//Print("Buylevel exceeded at "+BuyDots[keys[i]].ToString());
						BuyDots.Remove(keys[i]);
						BuyArrowABar = CurrentBars[0];
						arrowtag = "BuyExt62 "+BuyArrowABar.ToString();
						ArrowPrice = Low[0]+TickSize;
						if(pBullishBreakoutOpacity>0) BackBrushes[0] = RacingStripeUp;
					}
				}
				keys = SellDots.Keys.ToList();
				for(int i = 0; i<keys.Count; i++){
					if(Low[0] <= SellDots[keys[i]].SwingPrice && Low[1] >= SellDots[keys[i]].SwingPrice){
						if(State==State.Realtime && IsSoundEnabled) Alert(DateTime.Now.ToString(), Priority.Medium, "SimpleFib Sell level hit at "+Instrument.MasterInstrument.FormatPrice(SellDots[keys[i]].SwingPrice), AddSoundFolder(this.pWAVOnSellSetup), 1, Brushes.Green,Brushes.White);
//Print("Selllevel exceeded at "+SellDots[keys[i]].ToString());
						SellDots.Remove(keys[i]);
						SellArrowABar = CurrentBars[0];
						arrowtag = "SellExt62 "+SellArrowABar.ToString();
						ArrowPrice = High[0]-TickSize;
						if(pBearishBreakoutOpacity>0) BackBrushes[0] = RacingStripeDown;
					}
				}
				if(pDrawMarkersPivotBreaks){
					if(BuyArrowABar == CurrentBars[0] && Low[0] < ArrowPrice){
//	Print(Times[0][0].ToString()+"  Buy arrow");
						RemoveDrawObject(arrowtag);
						int rbar = CurrentBars[0] - BuyArrowABar;
						ArrowPrice = Low[0];
						Draw.ArrowUp(this,arrowtag,false, 0, ArrowPrice-TickSize*pSeparationTicks, Brushes.Lime);
					}
					if(SellArrowABar == CurrentBars[0] && High[0] > ArrowPrice){
//	Print(Times[0][0].ToString()+"  Sell arrow");
						RemoveDrawObject(arrowtag);
						int rbar = CurrentBars[0] - SellArrowABar;
						ArrowPrice = High[0];
						Draw.ArrowDown(this,arrowtag,false, 0, ArrowPrice+TickSize*pSeparationTicks, Brushes.Red);
					}
				}
			}
//			if(Timep1L.Count > pMaxAgeOfPivots) Timep1L.RemoveAt(0);
//			if(Timep1H.Count > pMaxAgeOfPivots) Timep1H.RemoveAt(0);
//			if(ExtAL.Count   > pMaxAgeOfPivots) ExtAL.RemoveAt(0);
//			if(ExtAH.Count   > pMaxAgeOfPivots) ExtAH.RemoveAt(0);

			var kvp = BuyDots.Where(k=>k.Value.ConfirmABar==-1).ToList();
			foreach(var k in kvp){
				k.Value.CurrentExtentPrice = Math.Min(k.Value.CurrentExtentPrice, Lows[0][0]);
				if(Highs[0][0] > k.Value.SwingPrice) k.Value.ConfirmABar = CurrentBars[0];
				if(fibA != -999){
					k.Value.Lvl1 = Instrument.MasterInstrument.RoundToTickSize(k.Value.SwingPrice + fibA*(k.Value.SwingPrice - k.Value.CurrentExtentPrice));
					LineName=string.Format("ExtAL_{0}", k.Key.ToString());
					SetTrendline(LineName, k.Key, k.Value.Lvl1, k.Value.ConfirmABar<=0? CurrentBars[0] : k.Value.ConfirmABar, k.Value.Lvl1, DashStyleHelper.Solid);
				}
				if(fibB != -999){
					k.Value.Lvl2 = Instrument.MasterInstrument.RoundToTickSize(k.Value.SwingPrice + fibB*(k.Value.SwingPrice - k.Value.CurrentExtentPrice));
					LineName=string.Format("ExtBL_{0}", k.Key.ToString());
					SetTrendline(LineName, k.Key, k.Value.Lvl2, k.Value.ConfirmABar<=0? CurrentBars[0] : k.Value.ConfirmABar, k.Value.Lvl2, DashStyleHelper.Solid);
				}
			}
			kvp = SellDots.Where(k=>k.Value.ConfirmABar==-1).ToList();
			foreach(var k in kvp){
				k.Value.CurrentExtentPrice = Math.Max(k.Value.CurrentExtentPrice, Highs[0][0]);
				if(Lows[0][0] < k.Value.SwingPrice) k.Value.ConfirmABar = CurrentBars[0];
				if(fibA != -999){
					k.Value.Lvl1 = Instrument.MasterInstrument.RoundToTickSize(k.Value.SwingPrice - fibA*(k.Value.CurrentExtentPrice - k.Value.SwingPrice));
					LineName=string.Format("ExtAL_{0}", k.Key.ToString());
					SetTrendline(LineName, k.Key, k.Value.Lvl1, k.Value.ConfirmABar<=0? CurrentBars[0] : k.Value.ConfirmABar, k.Value.Lvl1, DashStyleHelper.Solid);
				}
				if(fibB != -999){
					k.Value.Lvl2 = Instrument.MasterInstrument.RoundToTickSize(k.Value.SwingPrice - fibB*(k.Value.CurrentExtentPrice - k.Value.SwingPrice));
					LineName=string.Format("ExtBL_{0}", k.Key.ToString());
					SetTrendline(LineName, k.Key, k.Value.Lvl2, k.Value.ConfirmABar<=0? CurrentBars[0] : k.Value.ConfirmABar, k.Value.Lvl2, DashStyleHelper.Solid);
				}
			}
			/*
//   Search the ExtAH array for uncompleted P1high
			for (j=0; j<Timep1H.Count; j++)
			{
				DrawThisExtension=false;
				if(ExtAH[j] == EMPTY) //found an uncompleted P1
				{
					rbarP1 = CurrentBar - Bars.GetBar(Timep1H[j]);//iBarShift(0, Timep1H[j], false);
					MajorLow = High[rbarP1];
					for(k=rbarP1-1; k>=Math.Max(rbarP1-Math.Min(CurrentBar,pMaxAgeOfPivots),0); k--)
					{
						if(High[k]>High[rbarP1] && !DrawThisExtension) //when prices go higher than P1 level, draw the red (sell) SimpleFib line
						{
							DrawThisExtension=true;
							rBarEndPoint = CurrentBar-Bars.GetBar(Time[k]);//iBarShift(0,Time[k],false);
							break;
						}
						if(Low[k]<MajorLow) MajorLow=Low[k]; //found a new Low to compute the 62% extension
					}
					if(DrawThisExtension)
					{	
						if(fibA!=-999) 
						{
							//LineName="ExtAH_"+Time[rbarP1].ToBinary().ToString();
							LineName=string.Format("ExtAH_{0}", Timep1H[j].ToString());
							ExtAH[j]= Instrument.MasterInstrument.RoundToTickSize(High[rbarP1]+fibA*(High[rbarP1]-MajorLow));
							SetTrendline(LineName, rbarP1, ExtAH[j], rBarEndPoint, ExtAH[j], DashStyleHelper.Solid);
						}
						if(fibB!=-999) 
						{
							//LineName="ExtBH_"+Time[rbarP1].ToBinary().ToString();
							LineName=string.Format("ExtBH_{0}", Timep1H[j].ToString());
							double eb = Instrument.MasterInstrument.RoundToTickSize(High[rbarP1]+fibB*(High[rbarP1]-MajorLow));
							SetTrendline(LineName,rbarP1,eb,rBarEndPoint,eb,DashStyleHelper.Solid);
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
						if(fibA!=-999) 
						{
							//LineName="ExtAL_"+Time[rbarP1].ToBinary().ToString();
							LineName=string.Format("ExtAL_{0}", Timep1L[j].ToString());
							ExtAL[j]= Instrument.MasterInstrument.RoundToTickSize(Low[rbarP1]-fibA*(MajorHigh-Low[rbarP1]));
							SetTrendline(LineName, rbarP1, ExtAL[j], rBarEndPoint, ExtAL[j], DashStyleHelper.Solid);
						}
						if(fibB!=-999) 
						{
							//LineName="ExtBL_"+Time[rbarP1].ToBinary().ToString();
							LineName=string.Format("ExtBL_{0}", Timep1L[j].ToString());
							double eb = Instrument.MasterInstrument.RoundToTickSize(Low[rbarP1]-fibB*(MajorHigh-Low[rbarP1]));
							SetTrendline(LineName, rbarP1,eb,rBarEndPoint,eb,DashStyleHelper.Solid);
						}
						DrawThisExtension=false;
					}
				}
			}
			*/
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
					if(Rects.Count==0 || Rects.Last().Item1 < CurrentBars[0]-pOverlapBars){
						if(ConsolidationAlertABar < CurrentBars[0]-2 && pWAVOnConsolidation!="none"){
							ConsolidationAlertABar = CurrentBars[0];
							Alert(CurrentBars[0].ToString(),Priority.Medium, "Consolidation box created", AddSoundFolder(pWAVOnConsolidation), 1, Brushes.White, Brushes.Black);
						}
						Rects.Add(new Tuple<int,double,int,double>(CurrentBars[0]-1, max, CurrentBars[0]-10, min));//Draw.Rectangle(this,"Rect"+CurrentBars[0].ToString(), 1,max, 10, min, Brushes.Yellow);
					}else{
						int lmab = Rects.Last().Item3;
						max = Math.Min(Rects.Last().Item2, max);
						min = Math.Max(Rects.Last().Item4, min);
						//Rects.RemoveAt(Rects.Count-1);
						Rects.Add(new Tuple<int,double,int,double>(CurrentBars[0]-1, max, lmab, min));
					}
				}
			}
        }
		private List<Tuple<int,double,int,double>> Rects = new List<Tuple<int,double,int,double>>();
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
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
            #region -- conditions to return --
            if (!IsVisible || ChartBars.ToIndex < BarsRequiredToPlot) return;
            if (Bars == null || ChartControl == null) return;
            if (ChartBars.FromIndex == -1 || ChartBars.ToIndex == -1) return;
			if (IsInHitTest) return;
            #endregion
			base.OnRender(chartControl, chartScale);
			if(Lines==null || Lines.Count==0) return;
			var L = Lines.Where(k=>k.EndABar > ChartBars.FromIndex && k.StartABar < ChartBars.ToIndex);
			if(L==null) return;
			float x,y, x1, y1;
			foreach(var line in L){
				y = Convert.ToSingle(chartScale.GetYByValue(line.Price));
				x = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, line.StartABar));
				x1 = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, line.EndABar));

				RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x1,y), Plots[line.PlotId].BrushDX, Plots[line.PlotId].Width, Plots[line.PlotId].StrokeStyle);
			}
			if(pBoxColor != Brushes.Transparent && pConsolidationSensitivity>0 && pDrawConsolidations){
				var rects = Rects.Where(k=>k.Item3 <= ChartBars.ToIndex && k.Item1 >= ChartBars.FromIndex && k.Item2 >= chartScale.MinValue && k.Item4 <= chartScale.MaxValue).ToList();
				if(rects!=null && rects.Count>0){
//					var tFormat = font.ToDirectWriteTextFormat();
//					var tLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, Instrument.MasterInstrument.FormatPrice(Closes[0][0]), zonePricesTextFormat, (float)(ChartPanel.X + ChartPanel.W),12f);
					var brush = pBoxColor.ToDxBrush(RenderTarget);
					SharpDX.Vector2 v1 = new SharpDX.Vector2(0f,0f);
					SharpDX.Vector2 v2 = new SharpDX.Vector2(0f,0f);
					for(int i = 0; i<rects.Count; i++){
						x1 = chartControl.GetXByBarIndex(ChartBars, rects[i].Item1);//rmab
						x = chartControl.GetXByBarIndex(ChartBars, rects[i].Item3);//lmab
						y = Convert.ToSingle(chartScale.GetYByValue(rects[i].Item2));//max price
						y1 = Convert.ToSingle(chartScale.GetYByValue(rects[i].Item4));//min price
//						RenderTarget.DrawRectangle(new SharpDX.RectangleF(x,y, x1-x, y1-y), brush);
						brush.Opacity = 1f;
						v1.X = x;
						v1.Y = y;
						v2.X = x1;
						v2.Y = y;
						RenderTarget.DrawLine(v1, v2, brush);
						v1.Y = y1;
						v2.Y = y1;
						RenderTarget.DrawLine(v1, v2, brush);
						brush.Opacity = 0.1f;
						v1.X = x;
						v1.Y = y;
						v2.X = x;
						v2.Y = y1;
						RenderTarget.DrawLine(v1, v2, brush);
						v1.X = x1;
						v2.X = x1;
						RenderTarget.DrawLine(v1, v2, brush);
					}
					brush.Dispose();
					brush = null;
				}
			}
			foreach(var k in BuyDots){
				y = Convert.ToSingle(chartScale.GetYByValue(k.Value.Lvl1));
				x = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, k.Key));
				x1 = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, k.Value.ConfirmABar <= 0? CurrentBars[0] : k.Value.ConfirmABar));
				RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x1,y), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);
				y = Convert.ToSingle(chartScale.GetYByValue(k.Value.Lvl2));
				RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x1,y), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);
			}
			foreach(var k in SellDots){
				y = Convert.ToSingle(chartScale.GetYByValue(k.Value.Lvl1));
				x = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, k.Key));
				x1 = Convert.ToSingle(chartControl.GetXByBarIndex(ChartBars, k.Value.ConfirmABar <= 0? CurrentBars[0] : k.Value.ConfirmABar));
				RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x1,y), Plots[3].BrushDX, Plots[3].Width, Plots[3].StrokeStyle);
				y = Convert.ToSingle(chartScale.GetYByValue(k.Value.Lvl2));
				RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x1,y), Plots[4].BrushDX, Plots[4].Width, Plots[4].StrokeStyle);
			}
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
		#endregion
		
		#region Properties
        private int  pSignificance = 4; // Default setting for Significance
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

		private bool pDrawConsolidations = false;
		[Display(Order=25,    Name="Draw Consolidations?", GroupName="Visual", Description="")]
        public bool DrawConsolidations
        {
            get { return pDrawConsolidations; }
            set { pDrawConsolidations = value; }
        }

		private int pConsolidationSensitivity = 11;
		[Display(Order=30,    Name="Consolidation Sensitivity", GroupName="Visual", Description="Sensitivity of consolidation rectangles.  Low numbers mean more rectangles.  Set to '0' to turn off all rectangles")]
        public int ConsolidationSensitivity
        {
            get { return pConsolidationSensitivity; }
            set { pConsolidationSensitivity = Math.Max(0, value); }
        }

		[XmlIgnore]
        [Display(Order = 40,    Name = "Consolidation Box Color", GroupName = "Visual", Description = "")]
        public Brush pBoxColor { get; set; }
        [Browsable(false)]
        public string BoxColorSerialize
        {
            get { return Serialize.BrushToString(pBoxColor); }
            set { pBoxColor = Serialize.StringToBrush(value); }
        }
		private int pOverlapBars = 5;
		[Display(Order=50,    Name="Consolidation Overlap", GroupName="Visual", Description="")]
		public int OverlapBars
        {
            get { return pOverlapBars; }
            set { pOverlapBars = Math.Max(0, value); }
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

		#region -- Racing Stipes --
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

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Wav on Buy Dot Hit", GroupName = "Alerts")]
		public string pWAVOnBuySetup {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Wav on Sell Dot Hit", GroupName = "Alerts")]
		public string pWAVOnSellSetup {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Wav on Consolidation", GroupName = "Alerts")]
		public string pWAVOnConsolidation {get;set;}

		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Draw entry arrows?", GroupName = "Alerts")]
		public bool pDrawMarkersPivotBreaks {get;set;}

		[Display(Order = 40, ResourceType = typeof(Custom.Resource), Name = "Separation ticks", GroupName = "Alerts", Description="Distance between bar and entry arrow")]
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
		private SimpleFibSystem2[] cacheSimpleFibSystem2;
		public SimpleFibSystem2 SimpleFibSystem2()
		{
			return SimpleFibSystem2(Input);
		}

		public SimpleFibSystem2 SimpleFibSystem2(ISeries<double> input)
		{
			if (cacheSimpleFibSystem2 != null)
				for (int idx = 0; idx < cacheSimpleFibSystem2.Length; idx++)
					if (cacheSimpleFibSystem2[idx] != null &&  cacheSimpleFibSystem2[idx].EqualsInput(input))
						return cacheSimpleFibSystem2[idx];
			return CacheIndicator<SimpleFibSystem2>(new SimpleFibSystem2(), input, ref cacheSimpleFibSystem2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleFibSystem2 SimpleFibSystem2()
		{
			return indicator.SimpleFibSystem2(Input);
		}

		public Indicators.SimpleFibSystem2 SimpleFibSystem2(ISeries<double> input )
		{
			return indicator.SimpleFibSystem2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleFibSystem2 SimpleFibSystem2()
		{
			return indicator.SimpleFibSystem2(Input);
		}

		public Indicators.SimpleFibSystem2 SimpleFibSystem2(ISeries<double> input )
		{
			return indicator.SimpleFibSystem2(input);
		}
	}
}

#endregion
