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
using SBG;

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AlwaysIn : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Always-In";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive = false;
				pRangeSizeStr	= "atr 2.5";
				pStartTime		= 930;
				pStopTime		= 1550;
				pPermittedDOW	= "1,2,3,4,5";
				pTargetDistStr = "t 8";
				pDailyTgtDollars = 1000;
				AddPlot(new Stroke(Brushes.YellowGreen, 3), PlotStyle.Bar, "Equity");
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "OpenEquity");
				AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Dot, "Direction");
				AddLine(Brushes.DarkGray, 0, Custom.Resource.NinjaScriptIndicatorZeroLine);
			}
			else if (State == State.Configure)
			{
				PV = Instrument.MasterInstrument.PointValue;
				ClearOutputWindow();
				string s = string.Empty;
				string sU = pPermittedDOW.ToUpper();
				if (sU.Contains("ALL")) s = "All";
				else{
					if(sU.Contains("TODAY")) {
						if(DateTime.Now.DayOfWeek==DayOfWeek.Saturday) 
							sU = "M";
						else if(DateTime.Now.DayOfWeek==DayOfWeek.Sunday) 
							sU = "M";
						else
							sU = DateTime.Now.DayOfWeek.ToString().Substring(0,2);
						s = "Today";
					}else{
						if(sU.Contains("M") || sU.Contains("1"))   s = s+"M ";
						if(sU.Contains("TU") || sU.Contains("2"))  s = s+"Tu ";
						if(sU.Contains("W") || sU.Contains("3"))   s = s+"W ";
						if(sU.Contains("TH") || sU.Contains("4"))  s = s+"Th ";
						if(sU.Contains("F") || sU.Contains("5"))   s = s+"F ";
						if(sU.Contains("SA") || sU.Contains("6"))  s = s+"Sa ";
						if(sU.Contains("SU") || sU.Contains("0"))  s = s+"Su ";
						s = s.Trim();
					}
				}

				tm = new SBG.TradeManager(this, "AlwaysIn", "", "","", Instrument, s, pStartTime, pStopTime, pStopTime, pShowHrOfDayPnL, int.MaxValue, int.MaxValue);

				if(pRangeSizeStr.ToLower().Contains("a")) {
					ATRmultRange = tm.StrToDouble(pRangeSizeStr);
					TksRange = 0;
					RangeBasisStr = ATRmultRange+"x ATR";
				}else if(pRangeSizeStr.ToLower().Contains("$")) {
					double DollarsRange = tm.StrToDouble(pRangeSizeStr);
					TksRange = Convert.ToInt32(DollarsRange / PV / TickSize);
					ATRmultRange = 0;
					RangeBasisStr = string.Format("${0}", DollarsRange);
				}
				else{
					TksRange = tm.StrToInt(pRangeSizeStr);
					ATRmultRange = 0;
					RangeBasisStr = TksRange+"-ticks";
				}

				tm.SLTPs["1"] = new TradeManager.SLTPinfo("", pTargetDistStr, "");
				if(pTargetDistStr.ToLower().Contains("a")) {
					tm.SLTPs["1"].ATRmultTarget = tm.StrToDouble(pTargetDistStr);
					tm.SLTPs["1"].DollarsTarget = 0;
					tm.SLTPs["1"].TargetBasisStr = tm.SLTPs["1"].ATRmultTarget+"x ATR based tgt";
				}else if(pTargetDistStr.ToLower().Contains("$")) {
					tm.SLTPs["1"].DollarsTarget = tm.StrToDouble(pTargetDistStr);
					tm.SLTPs["1"].ATRmultTarget = 0;
					tm.SLTPs["1"].TargetBasisStr = string.Format("${0}", tm.SLTPs["1"].DollarsTarget);
				}
				else{
					int tks = tm.StrToInt(pTargetDistStr);
					tm.SLTPs["1"].DollarsTarget = tks * TickSize * PV;
					tm.SLTPs["1"].ATRmultTarget = 0;
					tm.SLTPs["1"].TargetBasisStr = tks+"-ticks";
				}
			}
			else if(State == State.DataLoaded){
				atr = ATR(14);
				double p = 0;
				if(pRoundNumberTicks>0){
					while(p < 1.2 * Closes[0].GetValueAt(BarsArray[0].Count-3)){
						if(p > 0.8 * Closes[0].GetValueAt(BarsArray[0].Count-3)){
							RoundNumbers.Add(p);
						}
						p = p + pRoundNumberTicks*TickSize;
					}
				}
			}
		}
		SBG.TradeManager tm;
		//======================================================================================

		ATR atr;
		double PV = 0;
		int LONG  = 1;
		int SHORT = -1;
		int FLAT  = 0;
		double EntryPrice   = double.MinValue;
		int TksRange		= 3;
		double ATRmultRange	= 2.5;

		//double DollarsTarget = 200;
		//double ATRmultTarget = 2.5;

		double LongEntry = double.MinValue;
		double ShortEntry = double.MinValue;
		double TgtPrice = 0;
		double equity_today_pts = 0;
		int tradestartabar = 0;
		bool Reset = false;
		#region -- Supporting methods --
		private void Printit(string s){
			//if(Times[0][0].Day==16) 
				Print(s);
		}
		private double CalcTgt(int Pos, double EntryPrice, double CurrentClosedEquity){
			double t1 = 0;
			double t2 = 0;
			if(tm.SLTPs["1"].DollarsTarget>0)
				t2 = Instrument.MasterInstrument.RoundToTickSize(tm.SLTPs["1"].DollarsTarget / PV);
			else if(tm.SLTPs["1"].ATRmultTarget>0)
				t2 = Instrument.MasterInstrument.RoundToTickSize(atr[0]*tm.SLTPs["1"].ATRmultTarget);

			if(Pos==LONG)
				return Instrument.MasterInstrument.RoundToTickSize(EntryPrice + Math.Max(t1,t2));
			else 
				return Instrument.MasterInstrument.RoundToTickSize(EntryPrice - Math.Max(t1,t2));
		}
		#endregion

		//DateTime DateTgtAchieved = DateTime.MinValue;
		int LastEntryTime     = -1;
		//bool DailyTgtAchieved = false;
		string RangeBasisStr  = "";
		//string TargetBasisStr = "";
//		bool BigBar           = false;
		List<double> ranges = new List<double>();
		List<double> RoundNumbers = new List<double>();
		string BuyLineTag = "";
		string SellLineTag = "";

		protected override void OnBarUpdate()
		{
			if(CurrentBar<5) return;

			if(Equity[1] < 0)      PlotBrushes[0][1] = Brushes.Firebrick;
			else if(Equity[1] > 0) PlotBrushes[0][1] = Brushes.YellowGreen;
			else                   PlotBrushes[0][1] = null;
			if(Equity[2] < 0)      PlotBrushes[0][2] = Brushes.Firebrick;
			else if(Equity[2] > 0) PlotBrushes[0][2] = Brushes.YellowGreen;
			else                   PlotBrushes[0][2] = null;

			var t1   = ToTime(Times[0][1])/100;
			var t    = ToTime(Times[0][0])/100;
			var dow1 = Times[0][1].DayOfWeek;
			var dow  = Times[0][0].DayOfWeek;
			var date = Times[0][0].Date;
			tm.PrintResults(Bars.Count, CurrentBars[0], true, this);

			var ts = new TimeSpan(Times[0][0].Ticks - Times[0][1].Ticks);
			bool SlowEnough = true;//ts.TotalSeconds >= 3;
			bool TradeToday = tm.DOW.Contains(dow);
			bool LongsPermitted  = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
			bool ShortsPermitted = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);

			if(tm.CurrentDay != Times[0][0].Day)
				tm.DailyTgtAchieved = false;
			else if(pDailyTgtDollars>0 && tm.IsInSession && tm.GetPnLPts(Times[0][0].Date) * PV > pDailyTgtDollars)
				tm.DailyTgtAchieved = true;

			if(tm.DailyTgtAchieved && date != tm.DateTgtAchieved){
				tm.DateTgtAchieved = date;
				tm.GoFlat(Times[0][0], Closes[0][0]);
				Equity[1] = tm.GetPnLPts(Times[0][0].Date) * PV;
				Equity[0] = 0;
			}
			if(tm.DailyTgtAchieved){
				return;
				Equity[0] = 0;
			}
			if(date != tm.DateTgtAchieved){
				if(IsFirstTickOfBar){
					ranges.Add(Highs[0][2]-Lows[0][2]);
					while(ranges.Count>10) ranges.RemoveAt(0);
//					if(Highs[0][1]-Lows[0][1] > ranges.Average()) BigBar = true;
				}
				bool HitStartTime = TradeToday && t1 < pStartTime && t >= pStartTime;
				if(SlowEnough && HitStartTime || Reset)
				{
					if(HitStartTime){
						Draw.VerticalLine(this,"VL"+CurrentBar.ToString(), Times[0][0], Brushes.Lime);
					}
					if(tm.IsInSession){
//						if(tradestartabar>0 && LongEntry!=double.MinValue && ShortEntry!=double.MinValue){
//							Draw.Line(this,string.Format("bl{0}", tradestartabar), CurrentBars[0]-tradestartabar, LongEntry, 1, LongEntry, Brushes.Cyan);
//							Draw.Line(this,string.Format("sl{0}", tradestartabar), CurrentBars[0]-tradestartabar, ShortEntry, 1, ShortEntry, Brushes.Red);
//						}
						tm.CurrentDay = Times[0][0].Day;
						CalcLongShortEntryPrices(pRoundNumberTicks, ref LongEntry, ref ShortEntry);
						if(LongEntry!=double.MinValue){
							Draw.Dot(this,string.Format("b{0}", CurrentBars[0]), false,0, LongEntry, Brushes.Cyan);
							Draw.Dot(this,string.Format("s{0}", CurrentBars[0]), false,0, ShortEntry, Brushes.Red);
							Reset = false;
							BuyLineTag = string.Format("bl{0}", CurrentBars[0]);
							SellLineTag = string.Format("sl{0}", CurrentBars[0]);
						}
						tradestartabar = CurrentBars[0];
					}
				}
			}
			if(!tm.IsInSession){
				PlotBrushes[0][0] = Brushes.DimGray;
			}else{
				Equity[0] = tm.GetPnLPts(Times[0][0].Date) * PV;
			}

			if(tm.ExitforEOD(Times[0][0], Times[0][1], Closes[0][1])){
				tm.GoFlat(Times[0][0], Closes[0][0]);
				Equity[1] = tm.GetPnLPts(Times[0][0].Date) * PV;
			}
			if(tm.FirstAbarOfSession == CurrentBars[0]) {
//				Equity[1] = tm.RealizedEquity;
				Printit(Times[0][0].ToString()+"  EOD, position closed");
				Equity[0] = 0;
			}

			if(tm.CurrentDay != 0 && CurrentBars[0]>tradestartabar && LongEntry!=double.MinValue){
//bool z = day==30;
//if(z) Print(Times[0][0].ToString()+"  pos: "+tm.CurrentPosition+"   LongEntry: "+LongEntry+"  "+LongsPermitted.ToString()+"   ShortEntry: "+ShortEntry+"  "+ShortsPermitted.ToString());
				if(Highs[0][0] > LongEntry && tm.CurrentPosition != LONG && LongsPermitted){
					if(tm.CurrentPosition == SHORT){//exit the short and go long
						tm.GoFlat(Times[0][0], LongEntry);
						Equity[0] = tm.GetPnLPts(Times[0][0].Date) * PV;
						Printit(Times[0][0].ToString()+"  reversing, short closed, added PnL of ");
					}
					if(SlowEnough){
						EntryPrice = LongEntry;
						TgtPrice = CalcTgt(LONG, EntryPrice, Equity[0]);
						Draw.Dot(this,string.Format("tgt{0}", CurrentBars[0]), false,0, TgtPrice, Brushes.LimeGreen);
						Printit("------  Now LONG, Tgt: "+TgtPrice);
						LastEntryTime = ToTime(Times[0][0]);
//						if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar!=CurrentBar){
//							Alert(DateTime.Now.ToString(), Priority.Medium, "ExpectedHighLow Sell level hit at "+Instrument.MasterInstrument.FormatPrice(ExpectedHigh[0]), AddSoundFolder(pWAVOnSellEntry), 1, Brushes.Magenta,Brushes.White);
//							tm.AlertBar = CurrentBars[0];
//						}
						tm.AddTrade('L', LongEntry, Times[0][0], ShortEntry, TgtPrice);
//						BackBrushes[0] = Brushes.Cyan;
					}
				}
				else if(Lows[0][0] < ShortEntry && tm.CurrentPosition != SHORT && ShortsPermitted){
					if(tm.CurrentPosition == LONG){//exit the long and go short
						tm.GoFlat(Times[0][0], ShortEntry);
						Equity[0] = tm.GetPnLPts(Times[0][0].Date) * PV;
						Printit(Times[0][0].ToString()+"  reversing, long closed, added PnL of ");
					}
					if(SlowEnough){
						EntryPrice = ShortEntry;
						TgtPrice = CalcTgt(SHORT, EntryPrice, Equity[0]);
						Draw.Dot(this,string.Format("tgt{0}", CurrentBars[0]), false,0, TgtPrice, Brushes.LimeGreen);
						Printit("------  Now SHORT, Tgt: "+TgtPrice);
						LastEntryTime = ToTime(Times[0][0]);
//						if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar!=CurrentBar){
//							Alert(DateTime.Now.ToString(), Priority.Medium, "ExpectedHighLow Sell level hit at "+Instrument.MasterInstrument.FormatPrice(ExpectedHigh[0]), AddSoundFolder(pWAVOnSellEntry), 1, Brushes.Magenta,Brushes.White);
//							tm.AlertBar = CurrentBars[0];
//						}
						tm.AddTrade('S', ShortEntry, Times[0][0], LongEntry, TgtPrice);
//						BackBrushes[0] = Brushes.Magenta;
					}
				}
				if(tm.CurrentPosition != FLAT){
					Direction[0] = 0;
					if(tm.CurrentPosition==LONG) PlotBrushes[2][0] = Brushes.Cyan; else PlotBrushes[2][0] = Brushes.Magenta;

					var status = tm.ExitforSLTP(Times[0][0], Highs[0][0], Lows[0][0], false);
					if(tm.CurrentPosition!=FLAT) tm.UpdateMinMaxPrices(Highs[0][0], Lows[0][0], Closes[0][0]);
					if(tm.CurrentPosition==LONG) PlotBrushes[2][0] = Brushes.Green;
					else if(tm.CurrentPosition==SHORT) PlotBrushes[2][0] = Brushes.Red;
					else PlotBrushes[2][0] = Brushes.Transparent;
					if(status.Length>0 && tm.CurrentPosition==FLAT){
						Reset = true;
					}

					OpenEquity[0] = tm.UnrealizedEquity * PV;
				}

			}
			if((!SlowEnough || LongEntry==double.MinValue || Reset) && tm.CurrentPosition == FLAT){
//if(Times[0][0].Day==13 && Times[0][0].Hour==11 && Times[0][0].Minute>=48)Print(Times[0][0].ToString());
				CalcLongShortEntryPrices(pRoundNumberTicks, ref LongEntry, ref ShortEntry);
				if(LongEntry!=double.MinValue){
					Draw.Dot(this,string.Format("b{0}", CurrentBars[0]), false,0, LongEntry, Brushes.Cyan);
					Draw.Dot(this,string.Format("s{0}", CurrentBars[0]), false,0, ShortEntry, Brushes.Red);
					Reset = false;
					BuyLineTag = string.Format("bl{0}", CurrentBars[0]);
					SellLineTag = string.Format("sl{0}", CurrentBars[0]);
					tradestartabar = CurrentBars[0];
				}
			}
			if(tm.IsInSession && BuyLineTag!=""){
				RemoveDrawObject(BuyLineTag);
				RemoveDrawObject(SellLineTag);
				Draw.Line(this, BuyLineTag, CurrentBars[0]-tradestartabar, LongEntry, 1, LongEntry, Brushes.Cyan);
				Draw.Line(this, SellLineTag, CurrentBars[0]-tradestartabar, ShortEntry, 1, ShortEntry, Brushes.Red);
			}
		}
		private void CalcLongShortEntryPrices(int RoundnumTicks, ref double LongEntry, ref double ShortEntry){
			LongEntry = double.MinValue;
			ShortEntry = double.MinValue;
			if(RoundnumTicks == 0){
				if(ATRmultRange>0){
					LongEntry = Closes[0][0] + atr[0]*ATRmultRange;
					ShortEntry = Closes[0][0] - atr[0]*ATRmultRange;
				}else{
					LongEntry = Closes[0][0] + TksRange*TickSize;
					ShortEntry = Closes[0][0] - TksRange*TickSize;
				}
			}else{
				var rn = RoundNumbers.Where(k=>k >= Lows[0][0]).ToList();
				if(rn!=null && rn.Count>0){
//if(Times[0][0].Day==13 && Times[0][0].Hour==11 && Times[0][0].Minute>=48)Print(Times[0][0].ToString()+ "   "+rn[0]);
					if(Highs[0][1] < rn[0] && Highs[0][0] >= rn[0] || Lows[0][1] > rn[0] && Lows[0][0] <= rn[0]){
//						BackBrushesAll[0] = Brushes.Yellow;
						if(ATRmultRange>0){
							LongEntry = rn[0] + atr[0]*ATRmultRange;
							ShortEntry = rn[0] - atr[0]*ATRmultRange;
						}else{
							LongEntry = rn[0] + TksRange*TickSize;
							ShortEntry = rn[0] - TksRange*TickSize;
						}
//if(Times[0][0].Day==13 && Times[0][0].Hour==11 && Times[0][0].Minute>=48)Print(Times[0][0].ToString()+ "   LongEntry "+LongEntry);
					}
				}
			}
		}
		public override void OnRenderTargetChanged()
		{
			tm.InitializeBrushes(RenderTarget);
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

			if(tm.OutputLS.Count>0){
				tm.OnRender(RenderTarget, ChartPanel, tm.OutputLS, 14, 10);
			}
//			if(distToLower < distToUpper && tm.OutputL.Count>0){
//				tm.OnRender(RenderTarget, ChartPanel, tm.OutputL, 14, 10);
//			}
//			if(distToUpper < distToLower && tm.OutputS.Count>0){
//				tm.OnRender(RenderTarget, ChartPanel, tm.OutputS, 14, 10);
//			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Order=10, Name="Range Size and type", Description="'$100', 'atr 3.5' or 't 15'", GroupName="Parameters")]
		public string pRangeSizeStr
		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="Tks Range", Order=10, GroupName="Parameters")]
//		public int TksRange
//		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(0, double.MaxValue)]
//		[Display(Name="ATR Mult Range", Order=11, GroupName="Parameters", Description="")]
//		public double ATRmultRange
//		{get;set;}

		[Range(1, 2359)]
		[Display(Order=20, Name="Start Time", GroupName="Parameters")]
		public int pStartTime
		{ get; set; }
		
		[Range(1, 2359)]
		[Display(Order=21, Name="Stop Time", GroupName="Parameters")]
		public int pStopTime
		{ get; set; }
		
		[Display(Order=40, Name="Days of week", GroupName="Parameters")]
		public string pPermittedDOW
		{get;set;}

		[NinjaScriptProperty]
		[Display(Order=50, Name="Round  numbers (ticks)", Description="Enter '0' to not use round number levels", GroupName="Parameters")]
		public int pRoundNumberTicks
		{get;set;}

		[Display(Name="Target Size and type", Description="'$100', 'atr 3.5' or 't 15'", Order=10, GroupName="Target")]
		public string pTargetDistStr
		{get;set;}

//		[NinjaScriptProperty]
//		[Range(0, double.MaxValue)]
//		[Display(Name="Target $", Order=10, GroupName="Target")]
//		public double DollarsTarget
//		{get;set;}
//		[NinjaScriptProperty]
//		[Range(0, double.MaxValue)]
//		[Display(Name="Target ATR Mult", Order=20, GroupName="Target", Description="")]
//		public double ATRmultTarget
//		{get;set;}

		[Range(0, double.MaxValue)]
		[Display(Name="Daily Profit Stop $", Order=30, GroupName="Target", Description="")]
		public double pDailyTgtDollars
		{get;set;}
		
		[Display(Name="Show Hourly PnL", Order=10, GroupName="Display", Description="")]
		public bool pShowHrOfDayPnL
		{get;set;}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Equity
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OpenEquity
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Direction
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlwaysIn[] cacheAlwaysIn;
		public AlwaysIn AlwaysIn(string pRangeSizeStr, int pRoundNumberTicks)
		{
			return AlwaysIn(Input, pRangeSizeStr, pRoundNumberTicks);
		}

		public AlwaysIn AlwaysIn(ISeries<double> input, string pRangeSizeStr, int pRoundNumberTicks)
		{
			if (cacheAlwaysIn != null)
				for (int idx = 0; idx < cacheAlwaysIn.Length; idx++)
					if (cacheAlwaysIn[idx] != null && cacheAlwaysIn[idx].pRangeSizeStr == pRangeSizeStr && cacheAlwaysIn[idx].pRoundNumberTicks == pRoundNumberTicks && cacheAlwaysIn[idx].EqualsInput(input))
						return cacheAlwaysIn[idx];
			return CacheIndicator<AlwaysIn>(new AlwaysIn(){ pRangeSizeStr = pRangeSizeStr, pRoundNumberTicks = pRoundNumberTicks }, input, ref cacheAlwaysIn);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlwaysIn AlwaysIn(string pRangeSizeStr, int pRoundNumberTicks)
		{
			return indicator.AlwaysIn(Input, pRangeSizeStr, pRoundNumberTicks);
		}

		public Indicators.AlwaysIn AlwaysIn(ISeries<double> input , string pRangeSizeStr, int pRoundNumberTicks)
		{
			return indicator.AlwaysIn(input, pRangeSizeStr, pRoundNumberTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlwaysIn AlwaysIn(string pRangeSizeStr, int pRoundNumberTicks)
		{
			return indicator.AlwaysIn(Input, pRangeSizeStr, pRoundNumberTicks);
		}

		public Indicators.AlwaysIn AlwaysIn(ISeries<double> input , string pRangeSizeStr, int pRoundNumberTicks)
		{
			return indicator.AlwaysIn(input, pRangeSizeStr, pRoundNumberTicks);
		}
	}
}

#endregion
