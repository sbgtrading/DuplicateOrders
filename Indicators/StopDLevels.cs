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
public enum StopDLevels_Types {Daily, Weekly, Monthly, Quarterly, Yearly}
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("Parameters", 10)]
    [CategoryOrder("HL Region", 20)]
    [CategoryOrder("Labels", 30)]
    [CategoryOrder("Indicator Version", 200)]
	public class StopDLevels : Indicator
	{
        private Bars bars24hr = null;
        private Bars barsYr = null;
		SortedDictionary<DateTime, double[]> PeriodResults = new SortedDictionary<DateTime, double[]>();
        [Display(Name = "Indicator Version", GroupName = "Indicator Version", Description = "Indicator Version", Order = 0)]
        public string indicatorVersion { get { return "Beta v1.4 (1-Aug-2023)"; } }
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= @"";
				Name					= "StopDLevels";
				Calculate				= Calculate.OnBarClose;
				IsOverlay				= true;
				DisplayInDataBox		= true;
				DrawOnPricePanel		= true;
				DrawHorizontalGridLines	= false;
				DrawVerticalGridLines	= false;
				PaintPriceMarkers		= true;
				IsAutoScale				= false;
				ScaleJustification		= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive = false;
				pStartTime				 = 930;
				pStopTime				 = 1600;
				pType					 = StopDLevels_Types.Daily;
				pHLRegionOpacity    = 10;
				pMonthsOfBackdata = 6;
				pFillRegionBrush    = Brushes.DimGray;
				pRegionOutlineBrush = Brushes.Transparent;

				pFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial",12);
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "PHigh");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "PLow");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "PMid");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "PClose");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "P50PctAbove");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "P100PctAbove");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "P50PctBelow");
				AddPlot(new Stroke(Brushes.ForestGreen,1), PlotStyle.Hash, "P100PctBelow");
			}
			else if (State == State.Configure)
			{
				for(int i = 0; i<Plots.Length; i++){
					brushes.Add(null);
				}

//				AddDataSeries(Data.BarsPeriodType.Minute, 30);
				Plots[2].DashStyleHelper = DashStyleHelper.Dash;//mid
				Plots[3].DashStyleHelper = DashStyleHelper.Dot;//Close
				Plots[4].DashStyleHelper = DashStyleHelper.Dash;//50+
				Plots[6].DashStyleHelper = DashStyleHelper.Dash;//50-
				ClearOutputWindow();

				List<TradingHours> th = TradingHours.All.ToList();
//foreach(var sss in th)Print(sss.Name);
				var fullday = th.FirstOrDefault(k=>k.Name.Contains("Default 24 x 5"));
//Print(fullday.Name+":  "+fullday.ToString());
				#region -- Get 2 yearly bars --
				BarsRequest barsRequest = new BarsRequest(Bars.Instrument, 3);
				barsRequest.BarsPeriod = new BarsPeriod { BarsPeriodType = BarsPeriodType.Year, MarketDataType = Bars.BarsPeriod.MarketDataType, Value = 1 };
				//---- Request the bars ----
				bool doWait = true;
				barsYr = null;
				bool error = false;
				barsRequest.Request(new Action<BarsRequest, NinjaTrader.Cbi.ErrorCode, string>((bars, errorCode, errorMessage) =>
				{
				    if (errorCode != NinjaTrader.Cbi.ErrorCode.NoError) { doWait = false; error = true; }
				    else if (bars.Bars == null || bars.Bars.Count == 0) { doWait = false; error = true; }
					if(!error){
					    barsYr = bars.Bars;
					    doWait = false;
					}
				}));

				while (doWait) { System.Threading.Thread.Sleep(10); }//made synchrone cause need request to finish before continuing
				if(error || barsYr==null) Log("StopDLevels data missing/unavailable", LogLevel.Error);
else Print("Yr bars: "+barsYr.Count+"   "+barsYr.GetTime(0).ToString());
				#endregion
				#region -- Get 6-months of 30-minute bars --
				barsRequest = new BarsRequest(Bars.Instrument, DateTime.Now.AddMonths(-pMonthsOfBackdata), DateTime.Now) {TradingHours = fullday, IsSplitAdjusted = false/*Bars.IsSplitAdjusted*/, IsDividendAdjusted = Bars.IsDividendAdjusted };
				barsRequest.BarsPeriod = new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, MarketDataType = Bars.BarsPeriod.MarketDataType, Value = 30 };
				//---- Request the bars ----

				doWait = true;
				bars24hr = null;
				error = false;
				barsRequest.Request(new Action<BarsRequest, NinjaTrader.Cbi.ErrorCode, string>((bars, errorCode, errorMessage) =>
				{
				    if (errorCode != NinjaTrader.Cbi.ErrorCode.NoError) { doWait = false; error = true; }
				    else if (bars.Bars == null || bars.Bars.Count == 0) { doWait = false; error = true; }
					if(!error){
					    bars24hr = bars.Bars;
					    doWait = false;
					}
				}));

				while (doWait) { System.Threading.Thread.Sleep(10); }//made synchrone cause need request to finish before continuing
				if(error || bars24hr==null) Log("StopDLevels data missing/unavailable", LogLevel.Error);
else Print("Total 30-minute bars loaded: "+bars24hr.Count+"  First date: "+bars24hr.GetTime(0).ToString()+"  "+BarsArray[0].TradingHours.Name);
				#endregion
				DateTime active_period_dt = DateTime.MinValue;
				for(int b = 1; b<bars24hr.Count; b++){
					var t = bars24hr.GetTime(b);
					var t1 = bars24hr.GetTime(b-1);
					tint  = ToTime(t)/100;
					tint1 = ToTime(t1)/100;
					#region -- Is this the last day of the week? --
					if(pType == StopDLevels_Types.Weekly && PeriodData.Count>0) {
						var dow = PeriodData.Last().Value.DOW;
						bool c1 = !PeriodData.Last().Value.IsLastDayOfWeek && t.DayOfWeek == DayOfWeek.Friday   && dow != DayOfWeek.Friday;
						bool c2 = !PeriodData.Last().Value.IsLastDayOfWeek && t.DayOfWeek == DayOfWeek.Saturday && dow < DayOfWeek.Saturday;
						bool c3 = !PeriodData.Last().Value.IsLastDayOfWeek && t.DayOfWeek == DayOfWeek.Sunday;
	//					bool c4 = t.DayOfWeek == DayOfWeek.Monday   && dow > DayOfWeek.Monday;
						if(c1 || c2 || c3){// || c4) {
							IsLastDayOfWeek = true;
						}
					}
					#endregion
					#region -- Is this the last day of the month? --
					if(t1.Month != t.Month && PeriodData.Count>0){
						IsLastDayOfMonth = true;
					}
					#endregion
					#region -- Is this the last day of the quarter? --
					if(pType == StopDLevels_Types.Quarterly && PeriodData.Count>0 && IsLastDayOfMonth && !PeriodData.Last().Value.IsLastDayOfQuarter){
						IsLastDayOfMonth = false;
						var tx = t1.Month;
						if(tx == 3 || tx == 6 || tx == 9 || tx == 12){//March, June, Sept or Dec
	//						IsLastDayOfQuarter = true;
							PeriodData.Last().Value.IsLastDayOfQuarter = true;
						}
					}
					#endregion
					#region -- Is this the last day of the year? --
					if(PeriodData.Count>0 && t1.Year != t.Year){
						IsLastDayOfYear = true;
						PeriodData.Last().Value.IsLastDayOfYear = true;
						pC = PeriodData.Last().Value.C;
					}
					#endregion
					InSession = false;
					if(pStartTime < pStopTime && tint > pStartTime && tint <= pStopTime) InSession = true;
					else if(pStartTime > pStopTime && (tint > pStartTime || tint <= pStopTime)) InSession = true;
					if(InSession){
						SessionBars[b] = true;
					}
					if(InSession){
//bool z = (t.Day==18 || t.Day==17) && t.Month==7;
//Print("In session now: "+t.ToString()+"  PeriodData.Count: "+PeriodData.Count);
						double h = bars24hr.GetHigh(b);
						double l = bars24hr.GetLow(b);
						double c = bars24hr.GetClose(b);
						if(!SessionBars.ContainsKey(b-1) && t.DayOfWeek!=DayOfWeek.Saturday && t.DayOfWeek!=DayOfWeek.Sunday){//first bar of a new session
							StartABar = b;
//if(z) Print("New day started "+t.ToString());
							active_period_dt = t;
							PeriodData[StartABar] = new PeriodHLC(h, l, c);
							PeriodData[StartABar].DOW = t.DayOfWeek;
							PeriodData[StartABar].Date = t;
						}else if(PeriodData.ContainsKey(StartABar)){
							active_period_dt = t;
//if(z && bars24hr.GetHigh(b) > PeriodData[StartABar].H) Print(t.ToString()+"  new high found at "+bars24hr.GetHigh(b));
							PeriodData[StartABar].H = Math.Max(PeriodData[StartABar].H, h);
//if(z && bars24hr.GetLow(b) < PeriodData[StartABar].L) Print(t.ToString()+"  new low found at "+bars24hr.GetLow(b));
							PeriodData[StartABar].L = Math.Min(PeriodData[StartABar].L, l);
							PeriodData[StartABar].C = c;
//Print("   ...day continuing    "+t.ToString()+"  high: "+h);
						}
					}//else Print("Not in session: "+t.ToString());

					if(SessionBars.ContainsKey(b-1) && InSession==false){//first out of session bar of a new session
						if(pType == StopDLevels_Types.Daily){
							#region -- Daily calculator --
							pH = PeriodData.Last().Value.H;
							pL = PeriodData.Last().Value.L;
							pC = PeriodData.Last().Value.C;
//Print("    day: "+t.Day+"    Current H: "+pH+"   L: "+pL+"  Date: "+PeriodData.Last().Value.Date);

							if(pType == StopDLevels_Types.Daily){
								if(!PeriodResults.ContainsKey(active_period_dt)) PeriodResults[active_period_dt] = new double[3]{pH,pL,pC};
								else{
									PeriodResults[active_period_dt][0] = pH;
									PeriodResults[active_period_dt][1] = pL;
									PeriodResults[active_period_dt][2] = pC;
								}
							}
							#endregion
						}else if(pType == StopDLevels_Types.Weekly && IsLastDayOfWeek){
							#region -- Week calculator --
							PeriodData.Last().Value.IsLastDayOfWeek = IsLastDayOfWeek;
							IsLastDayOfWeek = false;
							pH = PeriodData.Last().Value.H;
							pL = PeriodData.Last().Value.L;
							pC = PeriodData.Last().Value.C;
							var k = PeriodData.Keys.ToList();
	//Print("------------------------- "+PeriodData.Last().Value.DOW.ToString()+"  Date: "+bars24hr.GetTime(PeriodData.Keys.Max()).ToString());
	//Print("Inital C: "+pC+"   Initial H: "+pH+"   Initial L: "+pL);
//	BackBrushes[0] = Brushes.Blue;
							if(k!=null && k.Count>1){
								k.Reverse();//the latest dates are first in the list
								for(int i = 1; i<k.Count; i++){
	//Print("   checking "+bars24hr.GetTime(k[i]).DayOfWeek.ToString()+" "+bars24hr.GetTime(k[i]).ToString());
									if(PeriodData[k[i]].IsLastDayOfWeek){
	//Print(" EOW found at "+bars24hr.GetTime(k[i]).ToString());
										break;//stop iterating when you hit a prior end of week day
									}
									pH = Math.Max(pH,PeriodData[k[i]].H);//find week high
									pL = Math.Min(pL,PeriodData[k[i]].L);//find week low
	//Print("    day: "+bars24hr.GetTime(k[i]).Day+"    Current H: "+pH+"   L: "+pL);
								}
							}
							UpdateActivePeriodResults(active_period_dt, pH, pL, pC);
							#endregion
						}else if(pType == StopDLevels_Types.Monthly && IsLastDayOfMonth){
							#region -- Month calculator --
							PeriodData.Last().Value.IsLastDayOfMonth = true;
							IsLastDayOfMonth = false;
							pH = PeriodData.Last().Value.H;
							pL = PeriodData.Last().Value.L;
							pC = PeriodData.Last().Value.C;
//Print("------------------------- "+PeriodData.Last().Value.DOW.ToString()+"  Date: "+bars24hr.GetTime(PeriodData.Keys.Max()).ToString());
//Print("Inital C: "+pC+"   Initial H: "+pH+"   Initial L: "+pL);
//BackBrushes[0] = Brushes.Blue;
							var k = PeriodData.Keys.ToList();
							if(k!=null && k.Count>0){
								k.Reverse();//the latest dates are first in the list
								for(int i = 1; i<k.Count; i++){
									if(PeriodData[k[i]].IsLastDayOfMonth){
//Print(" EOM found at "+bars24hr.GetTime(k[i]).ToString());
										break;//stop iterating when you hit a prior end of week day
									}
									pH = Math.Max(pH,PeriodData[k[i]].H);//find week high
									pL = Math.Min(pL,PeriodData[k[i]].L);//find week low
//Print("    day: "+bars24hr.GetTime(k[i]).Day+"    Current H: "+pH+"   L: "+pL);
								}
							}
							UpdateActivePeriodResults(active_period_dt, pH, pL, pC);
							#endregion
						}else if(pType == StopDLevels_Types.Quarterly && PeriodData.Last().Value.IsLastDayOfQuarter){
							#region -- Quarter calculation --
							pH = PeriodData.Last().Value.H;
							pL = PeriodData.Last().Value.L;
							pC = PeriodData.Last().Value.C;
//	Print("------------------------- "+PeriodData.Last().Value.DOW.ToString()+"  Date: "+bars24hr.GetTime(PeriodData.Keys.Max()).ToString());
//	Print("Inital C: "+pC+"   Initial H: "+pH+"   Initial L: "+pL);
//	BackBrushes[0] = Brushes.Blue;
							var k = PeriodData.Keys.ToList();
							if(k!=null && k.Count>0){
								k.Reverse();//the latest dates are first in the list
								for(int i = 1; i<k.Count; i++){
									if(PeriodData[k[i]].IsLastDayOfQuarter){
//	Print(" EOQ found at "+bars24hr.GetTime(k[i]).ToString());
										break;//stop iterating when you hit a prior end of week day
									}
									pH = Math.Max(pH,PeriodData[k[i]].H);//find week high
									pL = Math.Min(pL,PeriodData[k[i]].L);//find week low
//	Print("    day: "+bars24hr.GetTime(k[i]).Day+"    Current H: "+pH+"   L: "+pL);
								}
							}
							UpdateActivePeriodResults(active_period_dt, pH, pL, pC);
							#endregion
						}else if(pType == StopDLevels_Types.Yearly && PeriodData.Last().Value.IsLastDayOfYear){
							#region -- Yearly calculation --
//	Print("------------------------- "+PeriodData.Last().Value.DOW.ToString()+"  Date: "+bars24hr.GetTime(PeriodData.Keys.Max()).ToString());
//	Print("Inital C: "+pC+"   Initial H: "+pH+"   Initial L: "+pL);
//	BackBrushes[0] = Brushes.Blue;
							pH = double.MinValue;
							pL = double.MaxValue;
							int yr = t.Year - 1;
							var k = PeriodData.Where(z=>z.Value.Date.Year == yr).Select(z=>z.Key).ToList();
							if(k!=null && k.Count>0){
								for(int i = 0; i<k.Count; i++){
									pH = Math.Max(pH,PeriodData[k[i]].H);//find high
									pL = Math.Min(pL,PeriodData[k[i]].L);//find low
								}
							}
							#endregion
						}
					}
	//				IsLastDayOfQuarter = false;
					IsLastDayOfYear = false;
				}
			}
            else if (State == State.Terminated)
            {
                if (bars24hr != null) bars24hr.Dispose();
			}
		}
		//============================================================================================
		private void UpdateActivePeriodResults(DateTime dt, double pH, double pL, double pC){
			if(!PeriodResults.ContainsKey(dt)) PeriodResults[dt] = new double[3]{pH,pL,pC};
			else{
				PeriodResults[dt][0] = pH;
				PeriodResults[dt][1] = pL;
				PeriodResults[dt][2] = pC;
			}
		}
		//============================================================================================
		private class PeriodHLC{
			public double H = 0;
			public double L = 0;
			public double C = 0;
			public DayOfWeek DOW = DayOfWeek.Sunday;
			public DateTime Date = DateTime.MinValue;
			public bool IsLastDayOfWeek = false;
			public bool IsLastDayOfMonth = false;
			public bool IsLastDayOfQuarter = false;
			public bool IsLastDayOfYear = false;
			public PeriodHLC(double h, double l, double c){H=h; L=l; C=c;}
		}
		SortedDictionary<int, PeriodHLC> PeriodData = new SortedDictionary<int, PeriodHLC>();

		bool InSession = false;
		SortedDictionary<int,bool>  SessionBars = new SortedDictionary<int,bool>();
		int StartABar = 0;
		double pH = 0;
		double pL = 0;
		double pC = 0;
		double pHigh = 0;
		double pLow = 0;
		double pClose = 0;
		double pMid = 0;
		double p50Up = 0;
		double p100Up = 0;
		double p50Dn = 0;
		double p100Dn = 0;
		int tint = 0;
		int tint1 = 0;
		bool IsLastDayOfWeek = false;
		bool IsLastDayOfMonth = false;
//		bool IsLastDayOfQuarter = false;
		bool IsLastDayOfYear = false;
		int DevelopingStartABar = -1;
		int RegionID = -1;
		protected override void OnBarUpdate()
		{
			if(CurrentBars[0] <3) return;
			if(pType == StopDLevels_Types.Yearly) {
				#region -- Yearly levels, use year bars --
				pHigh  = barsYr.GetHigh(barsYr.Count-2);
				pLow   = barsYr.GetLow(barsYr.Count-2);
				pClose = barsYr.GetClose(barsYr.Count-2);
				double temp = (pHigh-pLow)/2;
				pMid   = pLow  + temp;
				p50Up  = pHigh + temp;
				p50Dn  = pLow  - temp;
				p100Up = p50Up + temp;
				p100Dn = p50Dn - temp;
				var yr = barsYr.GetTime(barsYr.Count-2).Year;
//Print(barsYr.GetTime(barsYr.Count-2).ToString()+"  H/L: "+pHigh+" / "+pLow);
				if(RegionID==-1 && Times[0].GetValueAt(0).Year > yr) RegionID = 0;
				else if(Times[0][1].Year == yr && Times[0][0].Year > yr) RegionID = CurrentBars[0];
				#endregion
			}else if(pStartTime > pStopTime && pType == StopDLevels_Types.Daily) {
				#region -- daily levels, calculate developing --
				var t  = Times[0][0];
				var t1 = Times[0][1];
				tint   = ToTime(t)/100;
				tint1  = ToTime(t1)/100;
				if(tint >= pStopTime && tint1 > pStopTime && BarsArray[0].TradingHours.Name.Contains("RTH")){
					DevelopingStartABar = -1;
					var L = PeriodResults.Where(k=>k.Key <= Times[0][0]).ToList();
					if(L==null || L.Count==0) return;
					KeyValuePair<DateTime,double[]> priorresults = PeriodResults.LastOrDefault(k=>k.Key <= Times[0][0]);
					RegionID = BarsArray[0].GetBar(priorresults.Key);
					pHigh  = priorresults.Value[0];
					pLow   = priorresults.Value[1];
					pClose = priorresults.Value[2];
				}else{
					if(tint > pStartTime && tint1 <= pStartTime){
						DevelopingStartABar = CurrentBars[0];
						RegionID = CurrentBars[0];
						pHigh = Highs[0][0];
						pLow = Lows[0][0];
					}
					else if(tint < pStopTime || tint > pStartTime){
						pHigh = Math.Max(pHigh, Highs[0][0]);
						pLow = Math.Min(pLow, Lows[0][0]);
					}
				}
				if(pHigh != 0){
					double temp = (pHigh-pLow)/2;
					pMid   = pLow  + temp;
					p50Up  = pHigh + temp;
					p50Dn  = pLow  - temp;
					p100Up = p50Up + temp;
					p100Dn = p50Dn - temp;
				}
				#endregion
			}else if(BarsArray[0].IsFirstBarOfSession || ((int)BarsPeriod.BarsPeriodType > 4 && (int)BarsPeriod.BarsPeriodType < 9)){
				var L = PeriodResults.Where(k=>k.Key <= Times[0][0]).ToList();
				if(L==null || L.Count==0) return;
				KeyValuePair<DateTime,double[]> priorresults = PeriodResults.LastOrDefault(k=>k.Key <= Times[0][0]);
				RegionID = BarsArray[0].GetBar(priorresults.Key);

				pHigh  = priorresults.Value[0];
				pLow   = priorresults.Value[1];
				pClose = priorresults.Value[2];
				double temp = (pHigh-pLow)/2;
				pMid   = pLow  + temp;
				p50Up  = pHigh + temp;
				p50Dn  = pLow  - temp;
				p100Up = p50Up + temp;
				p100Dn = p50Dn - temp;
			}
			if(pHigh != 0 && RegionID>-1){
				PHigh[0]  = pHigh;
				PLow[0]   = pLow;
				PClose[0] = pClose;
				PMid[0]   = pMid;
//Print(Times[0][0].ToString()+"  "+pHigh);
				P50PctAbove[0]  = p50Up;
				P50PctBelow[0]  = p50Dn;
				P100PctAbove[0] = p100Up;
				P100PctBelow[0] = p100Dn;
				if(DevelopingStartABar>0 && pType == StopDLevels_Types.Daily){//backpaint historical developing levels based on current High and Low values
					for(int b = 1; b<(CurrentBars[0]-DevelopingStartABar+1); b++){
						PHigh[b]  = pHigh;
						PLow[b]   = pLow;
						PClose[b] = pClose;
						PMid[b]   = pMid;
						P50PctAbove[b]  = p50Up;
						P50PctBelow[b]  = p50Dn;
						P100PctAbove[b] = p100Up;
						P100PctBelow[b] = p100Dn;
					}
				}
			}
			if(pHLRegionOpacity>0 && RegionID>-1){
				Draw.Region(this,string.Format("StopDLevels{0}",RegionID), CurrentBars[0]-RegionID, 0, PHigh, PLow, pRegionOutlineBrush, pFillRegionBrush, pHLRegionOpacity);
			}
		}
//==============================================================================================
//		private SharpDX.Direct2D1.Brush BuyELBrushDX = null;
//		private SharpDX.Direct2D1.Brush SellELBrushDX = null;
//		public override void OnRenderTargetChanged()
//		{
//			if(BuyELBrushDX!=null   && !BuyELBrushDX.IsDisposed)    {BuyELBrushDX.Dispose();   BuyELBrushDX=null;}
//			if(RenderTarget!=null) BuyELBrushDX = BuyLine_Stroke.Brush.ToDxBrush(RenderTarget);

//			if(SellELBrushDX!=null   && !SellELBrushDX.IsDisposed)    {SellELBrushDX.Dispose();   SellELBrushDX=null;}
//			if(RenderTarget!=null) SellELBrushDX = SellLine_Stroke.Brush.ToDxBrush(RenderTarget);
//		}
		public override void OnRenderTargetChanged()
		{
			for(int i = 0; i<Plots.Length; i++){
				if(brushes[i]!=null && !brushes[i].IsDisposed){
					brushes[i].Dispose();
					brushes[i] = null;
				}
			}
			if(RenderTarget!=null){
				for(int i = 0; i<Plots.Length; i++){
					brushes[i] = Plots[i].Brush.ToDxBrush(RenderTarget);
				}
			}
		}

		SharpDX.RectangleF labelRect;
		SharpDX.DirectWrite.TextLayout txtLayout = null;
		List<SharpDX.Direct2D1.Brush> brushes = new List<SharpDX.Direct2D1.Brush>();
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			if(!pShowLabels) return;

			float y  = 0;
			float xL = 0;
			var txtFormat = pFont.ToDirectWriteTextFormat();
			string msg = string.Format("P{0}-High", pType.ToString()[0]);
			double p = pH;
			int rmab = Math.Max(1,Math.Min(CurrentBars[0],ChartBars.ToIndex));
			for(int i = 0; i<Plots.Length; i++){
				p = Values[i].GetValueAt(rmab);
				if(i==0)      {msg = string.Format("P{0}-High", pType.ToString()[0]);}
				else if(i==1) {msg = string.Format("P{0}-Low", pType.ToString()[0]); }
				else if(i==2) {msg = string.Format("P{0}-Mid", pType.ToString()[0]); }
				else if(i==3) {msg = string.Format("P{0}-Close", pType.ToString()[0]);}
				else if(i==4) {msg = string.Format("P{0}+50", pType.ToString()[0]);  }
				else if(i==5) {msg = string.Format("P{0}+100", pType.ToString()[0]); }
				else if(i==6) {msg = string.Format("P{0}-50", pType.ToString()[0]);  }
				else if(i==7) {msg = string.Format("P{0}-100", pType.ToString()[0]); }
				txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, msg, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);
				y = chartScale.GetYByValue(p);
				//xL = ChartControl.GetXByBarIndex(ChartBars, rmab);
				xL = ChartPanel.W-txtLayout.Metrics.Width-10f;
				txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, msg, txtFormat, ChartPanel.X + ChartPanel.W, txtFormat.FontSize);
				labelRect = new SharpDX.RectangleF(xL, y-txtLayout.Metrics.Height/2f, txtLayout.Metrics.Width, Convert.ToSingle(pFont.Size));
				RenderTarget.DrawText(msg, txtFormat, labelRect, brushes[i]);
			}
		}
//==============================================================================================
		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Start Time", Order=10, GroupName="Parameters")]
		public int pStartTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Stop Time", Order=20, GroupName="Parameters")]
		public int pStopTime
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Type", Description="Daily, Weekly, Monthly, Yearly", Order=30, GroupName="Parameters")]
		public StopDLevels_Types pType
		{ get; set; }
		
		[Display(Name="Months of backdata", Description="How many months of levels do you want calculated?", Order=40, GroupName="Parameters")]
		public int pMonthsOfBackdata
		{ get;set; }
		
		[Display(Name="Show labels?", Description="", Order=10, GroupName="Labels")]
		public bool pShowLabels
		{get;set;}
		[Display(Order = 20, Name = "Label font", GroupName = "Labels", ResourceType = typeof(Custom.Resource))]
		public NinjaTrader.Gui.Tools.SimpleFont pFont
		{get;set;}
		
		[Range(0,100)]
		[Display(Order = 10, Name = "Opacity", GroupName = "HL Region", ResourceType = typeof(Custom.Resource))]
		public int pHLRegionOpacity
		{get;set;}

		[XmlIgnore]
		[Display(Name="Region Fill", Order=20, GroupName="HL Region")]
		public Brush pFillRegionBrush
		{ get; set; }

		[Browsable(false)]
		public string RegionFillBrush_
					{get { return Serialize.BrushToString(pFillRegionBrush); }set { pFillRegionBrush = Serialize.StringToBrush(value); }}			

		[XmlIgnore]
		[Display(Name="Region Outline", Order=30, GroupName="HL Region")]
		public Brush pRegionOutlineBrush
		{ get; set; }
				[Browsable(false)]
				public string RegionOutlineBrush_
					{get { return Serialize.BrushToString(pRegionOutlineBrush); } set { pRegionOutlineBrush = Serialize.StringToBrush(value); }		}		

		#endregion

		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PHigh
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PLow
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PMid
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PClose
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P50PctAbove
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P100PctAbove
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P50PctBelow
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P100PctBelow
		{
			get { return Values[7]; }
		}
		#endregion
	}
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private StopDLevels[] cacheStopDLevels;
		public StopDLevels StopDLevels(int pStartTime, int pStopTime, StopDLevels_Types pType)
		{
			return StopDLevels(Input, pStartTime, pStopTime, pType);
		}

		public StopDLevels StopDLevels(ISeries<double> input, int pStartTime, int pStopTime, StopDLevels_Types pType)
		{
			if (cacheStopDLevels != null)
				for (int idx = 0; idx < cacheStopDLevels.Length; idx++)
					if (cacheStopDLevels[idx] != null && cacheStopDLevels[idx].pStartTime == pStartTime && cacheStopDLevels[idx].pStopTime == pStopTime && cacheStopDLevels[idx].pType == pType && cacheStopDLevels[idx].EqualsInput(input))
						return cacheStopDLevels[idx];
			return CacheIndicator<StopDLevels>(new StopDLevels(){ pStartTime = pStartTime, pStopTime = pStopTime, pType = pType }, input, ref cacheStopDLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.StopDLevels StopDLevels(int pStartTime, int pStopTime, StopDLevels_Types pType)
		{
			return indicator.StopDLevels(Input, pStartTime, pStopTime, pType);
		}

		public Indicators.StopDLevels StopDLevels(ISeries<double> input , int pStartTime, int pStopTime, StopDLevels_Types pType)
		{
			return indicator.StopDLevels(input, pStartTime, pStopTime, pType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.StopDLevels StopDLevels(int pStartTime, int pStopTime, StopDLevels_Types pType)
		{
			return indicator.StopDLevels(Input, pStartTime, pStopTime, pType);
		}

		public Indicators.StopDLevels StopDLevels(ISeries<double> input , int pStartTime, int pStopTime, StopDLevels_Types pType)
		{
			return indicator.StopDLevels(input, pStartTime, pStopTime, pType);
		}
	}
}

#endregion
