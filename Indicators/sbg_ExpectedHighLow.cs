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
using SBG_ExpectedHL;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
public enum ExpectedHighLow_Level {R1S1, R2S2, R3S3, R4S4};
public enum ExpectedHighLow_Timeframe {Week, Month, TradeStartTime};
public enum ExpectedHighLow_PivotType {PriorDayHL, CamarillaPivots, FloorPivots, DailyRange};
namespace NinjaTrader.NinjaScript.Indicators
{

	public class ExpectedHighLow : Indicator
	{
		DateTime expireDT = new DateTime(2025,5,23,0,0,0);

		double StdDev1 = 0;
		bool IsStdDevMultBand = false;//this will always be false, IsStdDevMultBand was discontinued
		bool IsPointsBand = false;
		bool IsTicksBand = false;
		SBG_ExpectedHL.TradeManager tm;
		Pivots pivot;
		CamarillaPivots cpivot;
		string Rtext = "";
		string Stext = "";
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Calculate the average high-to-priordayclose and the average low-to-priordayclose, for each day of the week, and project those expected highs/lows based on yesterdays close.";
				Name										= "sbg Expected HighLow";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				IsAutoScale = false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;

				var ExemptMachines = new List<string>(){
						"B0D2E9D1C802E279D3678D7DE6A33CE4" /*Ben Laptop*/,
						"other",//name and email address of exmpted
					};
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachines.Contains(NinjaTrader.Cbi.License.MachineId);
				if(IsBen)
					expireDT = DateTime.MaxValue;
				else if(expireDT == DateTime.MaxValue)//not me and the expiration date is ignored
					VendorLicense("DayTradersAction", "SbgExpectedHighLow", "www.sbgtradingcorp.com", "support@sbgtradingcorp.com");

				pDifferentiateDOW = true;
				StdDev1	= 0.5;
				BandSizeStr = "2ticks";
				pShowDOW = true;
				pPositionsize = 1;
				pFontSize = 14;
				pPivotType = ExpectedHighLow_PivotType.FloorPivots;
				pStartTime = 600;
				pStopTime = 1600;
				pGoFlatTime = 1600;
				pDailyRangemultiplier = 1.0;
				sDaysOfWeek = "M Tu W Th F";
				pShowHrOfDayTable = false;
				pEnableSoundAlerts = false;
				pCalculateLongTrades = true;
				pCalculateShortTrades = true;
				pGlobalizeLevels = true;
				pTimeframe = ExpectedHighLow_Timeframe.TradeStartTime;
				pWAVOnBuyEntry = "none";
				pWAVOnSellEntry = "none";

				AddPlot(Brushes.OrangeRed, "ExpectedHigh");
				AddPlot(Brushes.Blue, "ExpectedHighA");
				AddPlot(Brushes.Blue, "ExpectedHighB");
				AddPlot(Brushes.MediumSpringGreen, "ExpectedLow");
				AddPlot(Brushes.DarkGreen, "ExpectedLowA");
				AddPlot(Brushes.DarkGreen, "ExpectedLowB");
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
//				AddDataSeries(Data.BarsPeriodType.Day, 1);
				StdDev1 = ToDouble(BandSizeStr);
				if(BandSizeStr.ToLower().Contains("p")) IsPointsBand = true;
				else if(BandSizeStr.ToLower().Contains("t")) IsTicksBand = true;
				else IsPointsBand = true;//IsStdDevMultBand = true;

				Stext = "YL";
				Rtext = "YH";
				if(pPivotType == ExpectedHighLow_PivotType.DailyRange) {
					Stext = "DRL"; Rtext = "DRH";
				}else{
					if(pPivotLevels == ExpectedHighLow_Level.R1S1) {Stext = "S1"; Rtext = "R1";}
					if(pPivotLevels == ExpectedHighLow_Level.R2S2) {Stext = "S2"; Rtext = "R2";}
					if(pPivotLevels == ExpectedHighLow_Level.R3S3) {Stext = "S3"; Rtext = "R3";}
					if(pPivotLevels == ExpectedHighLow_Level.R4S4) {
						if(pPivotType == ExpectedHighLow_PivotType.FloorPivots) {Stext = "S3"; Rtext = "R3";}
						else {Stext = "S4"; Rtext = "R4";}
					}
				}
				tm = new SBG_ExpectedHL.TradeManager(this, "ExpectedHighLow", "", Stext, Rtext, Instrument, sDaysOfWeek, pStartTime, pStopTime, pGoFlatTime, pShowHrOfDayTable, 1, 1);
				tm.WarnIfWrongDayOfWeek = DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday;
				pivot = Pivots(PivotRange.Daily, HLCCalculationMode.DailyBars, 0,0,0,1);
				cpivot = CamarillaPivots(PivotRange.Daily, HLCCalculationMode.DailyBars,0,0,0,1);
			}
		}
		private double ToDouble(string s){
//			var ca = s.ToCharArray();
			s = new string(s.ToCharArray()
				.Where(k => (k>='0' && k<= '9') || k=='.' || k=='-')
				.ToArray());
			double d = 0;
			if(double.TryParse(s, out d)) return d; return 0;
		}

		private class RegionInfo{
			public int LastABar;
			public double HighA;
			public double LowA;
			public double HighB;
			public double LowB;
			public RegionInfo(int cbar, double Ha, double Hb, double La, double Lb){
				LastABar = cbar; HighA = Ha; HighB = Hb; LowA = La; LowB = Lb;
			}
		}
		private SortedDictionary<int, RegionInfo> Regions = new SortedDictionary<int, RegionInfo>();
		double avgH = double.MinValue;
		double avgL = double.MinValue;
		double C1 = 0;
		double stddevH = 0;
		double stddevL = 0;
		private SortedDictionary<DateTime, double> KeyPrices = new SortedDictionary<DateTime, double>();
		private SortedDictionary<DayOfWeek, List<double>> H = new SortedDictionary<DayOfWeek, List<double>>();
		private SortedDictionary<DayOfWeek, List<double>> L = new SortedDictionary<DayOfWeek, List<double>>();
		private SortedDictionary<DayOfWeek, List<double>> DailyRanges = new SortedDictionary<DayOfWeek, List<double>>();
		private double HH = double.MinValue;
		private double LL = double.MaxValue;
		private double DailyHH = double.MinValue;
		private double DailyLL = double.MaxValue;
		DayOfWeek dow1 = DayOfWeek.Sunday;
		DayOfWeek dow = DayOfWeek.Sunday;
		int bip = 0;
//		List<double> ranges = new List<double>();
		double KeyPrice = double.MinValue;
		double PriorDayH = double.MinValue;
		double PriorDayL = double.MinValue;
		int ABar_NewSession = 0;
		bool IsInSession = false;

		bool isExpired = false;
		protected override void OnBarUpdate()
		{
			if(!isExpired && expireDT <= Times[0][0]){
				isExpired = true;
				Draw.TextFixed(this,"expired","'Expected High-Low' trial license is expired - contact me at ben@sbgtradingcorp.com to upgrade", TextPosition.Center);
			}
			if(isExpired){
				return;
			}
			if(Highs[0][0] > DailyHH) DailyHH = Highs[0][0];
			if(Lows[0][0] < DailyLL) DailyLL = Lows[0][0];

			bip = 0;
			if(CurrentBars[bip]<2) return;
//			ranges.Add(Highs[bip][0]-Lows[bip][0]);
//			while(ranges.Count>10) ranges.RemoveAt(0);
//			if(BarsInProgress==1)
			{
				dow1 = Times[bip][1].DayOfWeek;
				dow = Times[bip][0].DayOfWeek;
				if(!H.ContainsKey(dow1)) H[dow1] = new List<double>();
				if(!L.ContainsKey(dow1)) L[dow1] = new List<double>();
				if(!H.ContainsKey(dow)) H[dow] = new List<double>();
				if(!L.ContainsKey(dow)) L[dow] = new List<double>();
				if(pPivotType == ExpectedHighLow_PivotType.DailyRange){
					if(!DailyRanges.ContainsKey(dow1)) DailyRanges[dow] = new List<double>(){0};
					if(dow1 != dow){
						DailyRanges[dow1].Add(DailyHH - DailyLL);
						DailyRanges[dow1][0] = (DailyRanges[dow1].Sum() - DailyRanges[dow1][0]) / (DailyRanges[dow1].Count-1) * this.pDailyRangemultiplier / 2.0;
						DailyHH = Highs[0][0];
						DailyLL = Lows[0][0];
						//the 0 element of that list is half of the average daily range multiplied by the reduction/enlargement factor.
					}
				}
			}
//			else
			{
				bool new_session = false;
				var t1 = ToTime(Times[0][1])/100;
				var t0 = ToTime(Times[0][0])/100;
				if(pTimeframe == ExpectedHighLow_Timeframe.TradeStartTime){
					if(t1<pStartTime && t0>=pStartTime){
						new_session = true;
						if(!pDifferentiateDOW) dow = DayOfWeek.Monday;
					}
//				}else if(pTimeframe == ExpectedHighLow_Timeframe.Day && dow1 != dow){
//					new_session = true;
//					if(!pDifferentiateDOW) dow = DayOfWeek.Monday;
				}else if(pTimeframe == ExpectedHighLow_Timeframe.Week && dow1 > dow){
					new_session = true;
					dow = DayOfWeek.Monday;
				}else if(pTimeframe == ExpectedHighLow_Timeframe.Month && Times[0][0].Day < Times[0][1].Day){
					new_session = true;
					dow = DayOfWeek.Monday;
				}
				if(new_session){
					ABar_NewSession = CurrentBars[0];
				} else {
					IsInSession = true;
					if(pStartTime < pStopTime && (t0 < pStartTime || t0 > pStopTime))
						IsInSession = false;
					if(pStartTime > pStopTime && (t0 < pStartTime && t0 > pStopTime))
						IsInSession = false;
//Print($"{t0}  start {pStartTime}  stop {pStopTime}   abar: {ABar_NewSession}   {IsInSession}");
				}

				if(new_session && CurrentBars[0]>=ABar_NewSession){
//Print($"{Name}  {Times[0][0]} New session!");
					avgH = 0;
					PriorDayH = HH;
					PriorDayL = LL;
					C1 = (HH+LL)/2.0;
//Print(Times[0][0].ToString()+"  C1:  "+C1);
					KeyPrices[Times[bip][1].Date] = C1;
					if(IsStdDevMultBand){
//						stddevH = StdDeviation(H[dow], avgH) * StdDev1;
//						stddevL = StdDeviation(L[dow], avgL) * StdDev1;
//						var minimumavg = ranges.Average();
//						stddevH = Math.Max(stddevH, minimumavg);
//						stddevL = Math.Max(stddevL, minimumavg);
					}else if(IsTicksBand){
						stddevH = StdDev1 * TickSize;
						stddevL = StdDev1 * TickSize;
					}else if(IsPointsBand){
						stddevH = StdDev1;
						stddevL = StdDev1;
					}
//Print(Times[0][0].ToString()+" "+dow+"  h[dow].Count: "+H[dow].Count+"   stdevH: "+stddevH+"   L: "+stddevL+"    IsStdDevMultBand? "+IsStdDevMultBand.ToString());
					if(KeyPrices.Count>0){
						Draw.Dot(this, string.Format("{0}{1}  {2}-{3}", pPivotLevels.ToString(),CurrentBars[0].ToString(),HH,LL), false, 0, KeyPrices.Last().Value, Brushes.Yellow);
						if(pShowDOW && tm.DOW.Contains(Times[0][0].DayOfWeek)){
							Draw.Text(this, "txt"+CurrentBars[0].ToString(), false, 
								dow.ToString()+"\n"+Times[bip][0].ToShortDateString()
//									+"\navgH: "+Instrument.MasterInstrument.FormatPrice(avgH)+" avgL: "+Instrument.MasterInstrument.FormatPrice(avgL)+"\n H: "+Instrument.MasterInstrument.FormatPrice(stddevH)+" L: "+Instrument.MasterInstrument.FormatPrice(stddevL)
								, 0, KeyPrices.Last().Value, 0, Brushes.White, new SimpleFont("Arial",12), TextAlignment.Right, Brushes.Transparent, Brushes.Black,100);
						}
					}
					HH = Highs[bip][0];
					LL = Lows[bip][0];
				}else{
					HH = Math.Max(HH,Highs[bip][0]);
					LL = Math.Min(LL,Lows[bip][0]);
				}
				int offset = 0;
//				if(CurrentBars[0]>BarsArray[0].Count-5)Draw.Text(this,"XXX","你明白这个翻译吗?",15,Closes[0][0],Brushes.Yellow);
				if(avgH != double.MinValue){
					if(pPivotType == ExpectedHighLow_PivotType.PriorDayHL){
						if(PriorDayH != double.MinValue){
							ExpectedHigh[0] = PriorDayH;
							ExpectedLow[0] = PriorDayL;
						}else{
							ExpectedHigh[0] = C1 + avgH;
							ExpectedLow[0] = C1 + avgL;
						}
						if(State==State.Realtime){
							if(this.pCalculateShortTrades) Draw.Text(this,"SellLvl","YH Sell",25,ExpectedHigh[0], pGlobalizeLevels, "Default");
							if(this.pCalculateLongTrades)  Draw.Text(this,"BuyLvl","YL Buy",25,ExpectedLow[0], pGlobalizeLevels, "Default");
						}
					}else if(pPivotType == ExpectedHighLow_PivotType.FloorPivots){
						if(pPivotLevels == ExpectedHighLow_Level.R1S1){
							ExpectedHigh[0] = pivot.R1[0];
							ExpectedLow[0] = pivot.S1[0];
//Print(Times[0][0].ToString()+"   ExpectedHigh[0]: "+ExpectedHigh[0]+"  low: "+ExpectedLow[0]);
						}else if(pPivotLevels == ExpectedHighLow_Level.R2S2){
							ExpectedHigh[0] = pivot.R2[0];
							ExpectedLow[0] = pivot.S2[0];
						}else if(pPivotLevels == ExpectedHighLow_Level.R3S3){
							ExpectedHigh[0] = pivot.R3[0];
							ExpectedLow[0] = pivot.S3[0];
						}else if(pPivotLevels == ExpectedHighLow_Level.R4S4){
							ExpectedHigh[0] = pivot.R3[0];
							ExpectedLow[0] = pivot.S3[0];
						}
						if(State==State.Realtime){
							if(this.pCalculateShortTrades) Draw.Text(this,"SellLvl",$"{Rtext} sell",25,ExpectedHigh[0], pGlobalizeLevels, "Default");
							if(this.pCalculateLongTrades)  Draw.Text(this,"BuyLvl",$"{Stext} buy",25,ExpectedLow[0], pGlobalizeLevels, "Default");
						}
					}else if(pPivotType == ExpectedHighLow_PivotType.CamarillaPivots){
						if(pPivotLevels == ExpectedHighLow_Level.R1S1){
							ExpectedHigh[0] = cpivot.R1[0];
							ExpectedLow[0] = cpivot.S1[0];
						}else if(pPivotLevels == ExpectedHighLow_Level.R2S2){
							ExpectedHigh[0] = cpivot.R2[0];
							ExpectedLow[0] = cpivot.S2[0];
						}else if(pPivotLevels == ExpectedHighLow_Level.R3S3){
							ExpectedHigh[0] = cpivot.R3[0];
							ExpectedLow[0] = cpivot.S3[0];
						}else if(pPivotLevels == ExpectedHighLow_Level.R4S4){
							ExpectedHigh[0] = cpivot.R4[0];
							ExpectedLow[0] = cpivot.S4[0];
						}
						if(State==State.Realtime){
							if(this.pCalculateShortTrades) Draw.Text(this,"SellLvl",$"{Rtext} sell",25,ExpectedHigh[0], pGlobalizeLevels, "Default");
							if(this.pCalculateLongTrades)  Draw.Text(this,"BuyLvl",$"{Stext} buy",25,ExpectedLow[0], pGlobalizeLevels, "Default");
						}
					}else if(pPivotType == ExpectedHighLow_PivotType.DailyRange){
						offset = 1;
						if(!DailyRanges.ContainsKey(dow)) return;
						double midprice = (DailyHH+DailyLL)/2.0;
						ExpectedHigh[0] = midprice + DailyRanges[dow][0];
						ExpectedLow[0] = midprice - DailyRanges[dow][0];
						if(State==State.Realtime){
							if(this.pCalculateShortTrades) Draw.Text(this,"SellLvl",$"{Rtext} sell",25,ExpectedHigh[0], pGlobalizeLevels, "Default");
							if(this.pCalculateLongTrades)  Draw.Text(this,"BuyLvl",$"{Stext} buy",25,ExpectedLow[0], pGlobalizeLevels, "Default");
						}
					}
					ExpectedHighA[0] = Instrument.MasterInstrument.RoundToTickSize(ExpectedHigh[0]+stddevH);
					ExpectedHighB[0] = Instrument.MasterInstrument.RoundToTickSize(ExpectedHigh[0]-stddevH);
					ExpectedLowA[0] = Instrument.MasterInstrument.RoundToTickSize(ExpectedLow[0]+stddevL);
					ExpectedLowB[0] = Instrument.MasterInstrument.RoundToTickSize(ExpectedLow[0]-stddevL);
					var t = Times[0].GetValueAt(ABar_NewSession);
					if(IsInSession){
//						RemoveDrawObject($"regionH{ABar_NewSession}");
	//					RemoveDrawObject($"regionL{ABar_NewSession}");
						if(!Regions.ContainsKey(ABar_NewSession)) Regions[ABar_NewSession] = new RegionInfo(CurrentBars[0], ExpectedHighA[0], ExpectedHighB[0], ExpectedLowA[0], ExpectedLowB[0]);
						Regions[ABar_NewSession].LastABar = CurrentBars[0];
//Print("Regions count: "+Regions.Count);
					}else{
						//Draw.Dot(this,$"regiondotH{CurrentBars[0]}",false,Times[0][0],ExpectedHighB[0],Brushes.Red);
						//Draw.Dot(this,$"regiondotL{CurrentBars[0]}",false,Times[0][0],ExpectedLowA[0],Brushes.Red);
					}
//					var t = Times[0].GetValueAt(1);
//					Draw.Region(this, $"regionH", t, Times[0][0], ExpectedHighA, ExpectedHighB, Brushes.Transparent, Plots[0].Brush, 20);
//					Draw.Region(this, $"regionL", t, Times[0][0], ExpectedLowA, ExpectedLowB, Brushes.Transparent, Plots[3].Brush, 20);

					if(pCalculateLongTrades || pCalculateShortTrades){
						tm.ExitforEOD(Times[0][0], Times[0][1], Closes[0][1]);
						var c1 = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);
						var c2 = Highs[0][1]<ExpectedHigh[offset] && Highs[0][0] >= ExpectedHigh[offset];
						var c3 = Lows[0][1]>ExpectedHigh[offset] && Lows[0][0] <= ExpectedHigh[offset];
						if(pCalculateShortTrades && c1 && (c2 || c3)){
							if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar!=CurrentBar){
								Alert(DateTime.Now.ToString(), Priority.Medium, "ExpectedHighLow Sell level hit at "+Instrument.MasterInstrument.FormatPrice(ExpectedHigh[0]), AddSoundFolder(pWAVOnSellEntry), 1, Brushes.Magenta,Brushes.White);
								tm.AlertBar = CurrentBars[0];
							}
							tm.DTofLastShort = Times[0][0];//one short trade per day
							tm.GoFlat(Times[0][0], ExpectedHigh[offset]);
							tm.AddTrade('S', pPositionsize, ExpectedHigh[offset], Times[0][0], double.MaxValue, ExpectedLow[offset]);
							BackBrushes[0] = Brushes.Magenta;
						}
						c1 = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
						c2 = Lows[0][1]>ExpectedLow[offset] && Lows[0][0] <= ExpectedLow[offset];
						c3 = Highs[0][1]<ExpectedLow[offset] && Highs[0][0] >= ExpectedLow[offset];
						if(pCalculateLongTrades && c1 && (c2 || c3)){
							if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar!=CurrentBar){
								Alert(DateTime.Now.ToString(), Priority.Medium, "ExpectedHighLow Buy level hit at "+Instrument.MasterInstrument.FormatPrice(ExpectedLow[0]), AddSoundFolder(pWAVOnBuyEntry), 1, Brushes.Lime,Brushes.White);
								tm.AlertBar = CurrentBars[0];
							}
							tm.DTofLastLong = Times[0][0];//one long trade per day
							tm.GoFlat(Times[0][0], ExpectedLow[offset]);
							tm.AddTrade('L', pPositionsize, ExpectedLow[offset], Times[0][0], double.MinValue, ExpectedHigh[offset]);
							BackBrushes[0] = Brushes.Lime;
						}
						tm.ExitforSLTP(Times[0][0], Highs[0][0], Lows[0][0], false);
						tm.UpdateMinMaxPrices(Highs[0][0], Lows[0][0], Closes[0][0]);
						tm.PrintResults(Bars.Count, CurrentBars[0], true, this);
					}
				}
			}
		}
//		private SharpDX.Direct2D1.Brush GreenTxtBrushDX = null;
//		private SharpDX.Direct2D1.Brush MagentaTxtBrushDX = null;
//		private SharpDX.Direct2D1.Brush CyanTxtBrushDX = null;
		private SharpDX.Direct2D1.Brush FillHighBrushDX = null;
		private SharpDX.Direct2D1.Brush FillLowBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			tm.InitializeBrushes(RenderTarget);
			if(FillHighBrushDX!=null && !FillHighBrushDX.IsDisposed)      FillHighBrushDX.Dispose();      FillHighBrushDX   = null;
			if(RenderTarget!=null) {FillHighBrushDX = Plots[0].Brush.ToDxBrush(RenderTarget); FillHighBrushDX.Opacity = 0.2f;}
			if(FillLowBrushDX!=null && !FillLowBrushDX.IsDisposed)      FillLowBrushDX.Dispose();      FillLowBrushDX   = null;
			if(RenderTarget!=null) {FillLowBrushDX = Plots[3].Brush.ToDxBrush(RenderTarget); FillLowBrushDX.Opacity = 0.2f;}
		}
		int count =0;
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
            #region -- conditions to return --
            if (!IsVisible || ChartBars.ToIndex < BarsRequiredToPlot) return;
            if (Bars == null || BarsArray[0]==null || chartControl == null) return;
            if (ChartBars.FromIndex == -1 || ChartBars.ToIndex == -1) return;
            #endregion
			int RMaB = Math.Max(1, Math.Min(ChartBars.ToIndex, BarsArray[0].Count-1));
			int LMaB = Math.Max(1, Math.Min(ChartBars.FromIndex, RMaB));
			if(pPivotType != ExpectedHighLow_PivotType.DailyRange){
				var ll = Regions.Where(kvp => kvp.Key < RMaB && kvp.Value.LastABar >= LMaB).ToList();
				foreach(var kvp in ll){
					var x = chartControl.GetXByBarIndex(ChartBars, kvp.Key);
					var x1 = chartControl.GetXByBarIndex(ChartBars, kvp.Value.LastABar);
					var yT = chartScale.GetYByValue(kvp.Value.HighA);
					var yB = chartScale.GetYByValue(kvp.Value.HighB);
					RenderTarget.FillRectangle(new SharpDX.RectangleF(x,yT,x1-x,(yB-yT)), FillHighBrushDX);
					yT = chartScale.GetYByValue(kvp.Value.LowA);
					yB = chartScale.GetYByValue(kvp.Value.LowB);
					//Print($"{kvp.Key}: {x},{x1}   yt: {yT}   {kvp.Value.LowB} yb: {yB}");
					RenderTarget.FillRectangle(new SharpDX.RectangleF(x,yT,x1-x,(yB-yT)), FillLowBrushDX);
				}
			}

			if(pFontSize > 4){
				double distToUpper = Math.Abs(Closes[0].GetValueAt(CurrentBars[0]-1) - ExpectedHigh.GetValueAt(CurrentBars[0]-1));
				double distToLower = Math.Abs(Closes[0].GetValueAt(CurrentBars[0]-1) - ExpectedLow.GetValueAt(CurrentBars[0]-1));
				bool SwitchOutput = Keyboard.IsKeyDown(Key.LeftCtrl);
				if(!SwitchOutput){
					if(tm.OutputLS.Count>0){
						tm.OnRender(RenderTarget, ChartPanel, tm.OutputLS, pFontSize, pFontSize-3);
					}
				}else{
					if(distToLower < distToUpper && tm.OutputL.Count>0){
						tm.OnRender(RenderTarget, ChartPanel, tm.OutputL, pFontSize, pFontSize-3);
					}
					if(distToUpper < distToLower && tm.OutputS.Count>0){
						tm.OnRender(RenderTarget, ChartPanel, tm.OutputS, pFontSize, pFontSize-3);
					}
				}
			}
		}
		double StdDeviation(List<double> L, double mean){
			if(L.Count==0) return 0;
			double sumOfSquaresOfDifferences = L.Sum(val => (val - mean) * (val - mean));
			Print("sumofDiffs: "+sumOfSquaresOfDifferences);
			return Math.Sqrt(sumOfSquaresOfDifferences / L.Count);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0.000001,10000000)]
		[Display(Name="Position Size", Order=5, GroupName="Parameters", Description="", ResourceType = typeof(Custom.Resource))]
		public double pPositionsize
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Band Size", Order=10, GroupName="Parameters", Description="Enter '20t' for 20-ticks, enter '3.5p' for 3.5-points", ResourceType = typeof(Custom.Resource))]
		public string BandSizeStr
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Differentiate DOW", Order=20, GroupName="Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pDifferentiateDOW
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Order = 24, Name = "Pivot Type", Description="", GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public ExpectedHighLow_PivotType pPivotType
		{get;set;}

		[NinjaScriptProperty]
		[Display(Order = 25, Name = "Levels?", Description="Only used if Pivot Type is FloorPivots or CamarillaPivots", GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public ExpectedHighLow_Level pPivotLevels
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Timeframe", Order=50, GroupName="Parameters", ResourceType = typeof(Custom.Resource))]
		public ExpectedHighLow_Timeframe pTimeframe
		{get;set;}

		[NinjaScriptProperty]
		[Display(Order = 50, Name="Daily Range multiplier", GroupName="Parameters", Description="If Levels is 'DailyRange', this is the multiplier.  <1.0 is a reduction, >1.0 is an enlargement", ResourceType = typeof(Custom.Resource))]
		public double pDailyRangemultiplier
		{get;set;}

		[Display(Name="Show Current DOW", Order=60, GroupName="Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pShowDOW
		{ get; set; }
		
		[Display(Name="Globalize buy/sell levels?", Order=70, GroupName="Parameters", ResourceType = typeof(Custom.Resource))]
		public bool pGlobalizeLevels
		{get;set;}

		#region -- Strategy params --
		[Display(Name="Permit LONG trades", Order=10, GroupName="Strategy", ResourceType = typeof(Custom.Resource))]
		public bool pCalculateLongTrades
		{ get; set; }

		[Display(Name="Permit SHORT trades", Order=20, GroupName="Strategy", ResourceType = typeof(Custom.Resource))]
		public bool pCalculateShortTrades
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Order = 30, Name = "Days of week", GroupName = "Strategy", ResourceType = typeof(Custom.Resource))]
		public string sDaysOfWeek
		{ get; set; }

		[Display(Order = 40, Name="Trade Start time", GroupName="Strategy", Description="Trading is permitted after this time",  ResourceType = typeof(Custom.Resource))]
		public int pStartTime
		{get;set;}

		[Display(Order = 50, Name="Trade Stop time", GroupName="Strategy", Description="No more trades initiated after this time", ResourceType = typeof(Custom.Resource))]
		public int pStopTime
		{get;set;}

		[Display(Order = 60, Name="Trade Exit time", GroupName="Strategy",  Description="All open trades are flattened at this time", ResourceType = typeof(Custom.Resource))]
		public int pGoFlatTime
		{get;set;}

		[Display(Order = 10, Name="Show 'Hr of Day' Table?", GroupName="Strategy Visuals",  Description="", ResourceType = typeof(Custom.Resource))]
		public bool pShowHrOfDayTable
		{get;set;}
		
		[Display(Order = 20, Name="Font size", GroupName="Strategy Visuals",  Description="", ResourceType = typeof(Custom.Resource))]
		public int pFontSize
		{get;set;}

		#endregion
		#endregion

		#region -- Alerts --
		private string AddSoundFolder(string wav){
			wav = wav.Replace("<inst>",Instrument.MasterInstrument.Name);
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
			if(!System.IO.File.Exists(wav)) {
				Log("ExpectedHighLow could not find wav: "+wav,LogLevel.Information);
				return "";
			}else
				return wav;
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

		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Enable sound alerts?", GroupName = "Alerts")]
		public bool pEnableSoundAlerts {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Wav on Buy Entry", GroupName = "Alerts")]
		public string pWAVOnBuyEntry {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Wav on Sell Entry", GroupName = "Alerts")]
		public string pWAVOnSellEntry {get;set;}
		#endregion

		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExpectedHigh
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExpectedHighA
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExpectedHighB
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExpectedLow
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExpectedLowA
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExpectedLowB
		{
			get { return Values[5]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ExpectedHighLow[] cacheExpectedHighLow;
		public ExpectedHighLow ExpectedHighLow(double pPositionsize, string bandSizeStr, bool pDifferentiateDOW, ExpectedHighLow_PivotType pPivotType, ExpectedHighLow_Level pPivotLevels, ExpectedHighLow_Timeframe pTimeframe, double pDailyRangemultiplier, string sDaysOfWeek)
		{
			return ExpectedHighLow(Input, pPositionsize, bandSizeStr, pDifferentiateDOW, pPivotType, pPivotLevels, pTimeframe, pDailyRangemultiplier, sDaysOfWeek);
		}

		public ExpectedHighLow ExpectedHighLow(ISeries<double> input, double pPositionsize, string bandSizeStr, bool pDifferentiateDOW, ExpectedHighLow_PivotType pPivotType, ExpectedHighLow_Level pPivotLevels, ExpectedHighLow_Timeframe pTimeframe, double pDailyRangemultiplier, string sDaysOfWeek)
		{
			if (cacheExpectedHighLow != null)
				for (int idx = 0; idx < cacheExpectedHighLow.Length; idx++)
					if (cacheExpectedHighLow[idx] != null && cacheExpectedHighLow[idx].pPositionsize == pPositionsize && cacheExpectedHighLow[idx].BandSizeStr == bandSizeStr && cacheExpectedHighLow[idx].pDifferentiateDOW == pDifferentiateDOW && cacheExpectedHighLow[idx].pPivotType == pPivotType && cacheExpectedHighLow[idx].pPivotLevels == pPivotLevels && cacheExpectedHighLow[idx].pTimeframe == pTimeframe && cacheExpectedHighLow[idx].pDailyRangemultiplier == pDailyRangemultiplier && cacheExpectedHighLow[idx].sDaysOfWeek == sDaysOfWeek && cacheExpectedHighLow[idx].EqualsInput(input))
						return cacheExpectedHighLow[idx];
			return CacheIndicator<ExpectedHighLow>(new ExpectedHighLow(){ pPositionsize = pPositionsize, BandSizeStr = bandSizeStr, pDifferentiateDOW = pDifferentiateDOW, pPivotType = pPivotType, pPivotLevels = pPivotLevels, pTimeframe = pTimeframe, pDailyRangemultiplier = pDailyRangemultiplier, sDaysOfWeek = sDaysOfWeek }, input, ref cacheExpectedHighLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ExpectedHighLow ExpectedHighLow(double pPositionsize, string bandSizeStr, bool pDifferentiateDOW, ExpectedHighLow_PivotType pPivotType, ExpectedHighLow_Level pPivotLevels, ExpectedHighLow_Timeframe pTimeframe, double pDailyRangemultiplier, string sDaysOfWeek)
		{
			return indicator.ExpectedHighLow(Input, pPositionsize, bandSizeStr, pDifferentiateDOW, pPivotType, pPivotLevels, pTimeframe, pDailyRangemultiplier, sDaysOfWeek);
		}

		public Indicators.ExpectedHighLow ExpectedHighLow(ISeries<double> input , double pPositionsize, string bandSizeStr, bool pDifferentiateDOW, ExpectedHighLow_PivotType pPivotType, ExpectedHighLow_Level pPivotLevels, ExpectedHighLow_Timeframe pTimeframe, double pDailyRangemultiplier, string sDaysOfWeek)
		{
			return indicator.ExpectedHighLow(input, pPositionsize, bandSizeStr, pDifferentiateDOW, pPivotType, pPivotLevels, pTimeframe, pDailyRangemultiplier, sDaysOfWeek);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ExpectedHighLow ExpectedHighLow(double pPositionsize, string bandSizeStr, bool pDifferentiateDOW, ExpectedHighLow_PivotType pPivotType, ExpectedHighLow_Level pPivotLevels, ExpectedHighLow_Timeframe pTimeframe, double pDailyRangemultiplier, string sDaysOfWeek)
		{
			return indicator.ExpectedHighLow(Input, pPositionsize, bandSizeStr, pDifferentiateDOW, pPivotType, pPivotLevels, pTimeframe, pDailyRangemultiplier, sDaysOfWeek);
		}

		public Indicators.ExpectedHighLow ExpectedHighLow(ISeries<double> input , double pPositionsize, string bandSizeStr, bool pDifferentiateDOW, ExpectedHighLow_PivotType pPivotType, ExpectedHighLow_Level pPivotLevels, ExpectedHighLow_Timeframe pTimeframe, double pDailyRangemultiplier, string sDaysOfWeek)
		{
			return indicator.ExpectedHighLow(input, pPositionsize, bandSizeStr, pDifferentiateDOW, pPivotType, pPivotLevels, pTimeframe, pDailyRangemultiplier, sDaysOfWeek);
		}
	}
}

#endregion
