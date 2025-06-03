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
	public enum TwinRangeFilter_MarkerType {None, Arrow, Triangle, Dot, Diamond, Square}
	public class TwinRangeFilter : Indicator
	{
		private Series<double> DiffCloses;
		private Series<double> avgrng1;
		private Series<double> avgrng2;
		private Series<double> rngfilt;
		private Series<double> upward;
		private Series<double> downward;
		private Series<double> CondIni;

		private KAMA FilterEMA;
		private EMA EMA_diff1, EMA_diff2;
		private EMA EMA_avgrng1, EMA_avgrng2;
		
		private double PtsRisked = 0;
		private double PtsReward = 0;
		
		private Brush UpFloodBrush   = null;
		private Brush DownFloodBrush = null;
//		private double StopEntryPrice = 0;//for a buy signal, it's the highest high of the prior 2 bars.  For short, it's the lowest low of the prior 2 bars
		private char LastSignalDir  = ' ';
		private int LastSignalABar  = 0;
		private int AlertABar       = 0;
		private bool PrintedResults = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Twin Range Filter";
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
				IsSuspendedWhileInactive					= false;
				IsAutoScale = false;
				FilterMAPeriod = 0;
				Period1					= 27;
				Per1Mult				= 1.6;
				Period2					= 55;
				Per2Mult				= 2;
				pDollarRisk = 100;
				pRewardToRisk = 2.5;
				pUpFloodBrush       = Brushes.Lime;
				pDownFloodBrush     = Brushes.Fuchsia;
				pUpFloodOpacity     = 10;
				pDownFloodOpacity   = 10;
				pUpMarkerBrush   = Brushes.Green;
				pDownMarkerBrush = Brushes.Magenta;
				pSignalMarker = TwinRangeFilter_MarkerType.Diamond;
				pPrintToAlertsWindow = false;
				pWAVOnBuyEntry = "<inst>_BuyEntry.wav";
				pWAVOnSellEntry = "<inst>_SellEntry.wav";


				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.TriangleUp, "BuySig");
				AddPlot(new Stroke(Brushes.Fuchsia, 2), PlotStyle.TriangleDown, "SellSig");
				AddPlot(new Stroke(Brushes.Cyan, 1), PlotStyle.Line, "TrendFilter");
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Hash, "EntryPrice");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Hash, "ExitPrice");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Hash, "TargetPrice");
			}
			else if (State == State.Configure)
			{
				PtsRisked = Instrument.MasterInstrument.RoundDownToTickSize(pDollarRisk / Instrument.MasterInstrument.PointValue);
				PtsReward = Instrument.MasterInstrument.RoundDownToTickSize(PtsRisked * pRewardToRisk);

				UpFloodBrush = this.pUpFloodBrush.Clone();
				UpFloodBrush.Opacity = this.pUpFloodOpacity/100.0;
				UpFloodBrush.Freeze();
				DownFloodBrush = this.pDownFloodBrush.Clone();
				DownFloodBrush.Opacity = this.pDownFloodOpacity/100.0;
				DownFloodBrush.Freeze();

			}
			else if (State == State.DataLoaded){
				DiffCloses  = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				avgrng1     = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				avgrng2     = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				rngfilt     = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				upward      = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				downward    = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				CondIni     = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				EMA_diff1   = EMA(DiffCloses, Period1);
				EMA_diff2   = EMA(DiffCloses, Period2);
				EMA_avgrng1 = EMA(EMA_diff1, Period1*2-1);
				EMA_avgrng2 = EMA(EMA_diff2, Period2*2-1);
				if(FilterMAPeriod>0) FilterEMA = KAMA(3,FilterMAPeriod,FilterMAPeriod*2);
				ClearOutputWindow();
			}
		}
		private void DrawMarker(double Price, int BarsAgo, char Dir, TwinRangeFilter_MarkerType type){
			#region -- DrawMarker --
			string tag = string.Format("TRF {0}{1}",Dir,CurrentBar);
			if(type==TwinRangeFilter_MarkerType.Arrow){
				if(Dir=='U')
					Draw.ArrowUp(this, tag, false, BarsAgo, Price, pUpMarkerBrush);
				else
					Draw.ArrowDown(this, tag, false, BarsAgo, Price, pDownMarkerBrush);
			}
			else if(type==TwinRangeFilter_MarkerType.Triangle){
				if(Dir=='U')
					Draw.TriangleUp(this, tag, false, BarsAgo, Price, pUpMarkerBrush);
				else
					Draw.TriangleDown(this, tag, false, BarsAgo, Price, pDownMarkerBrush);
			}
			else if(type==TwinRangeFilter_MarkerType.Dot){
				if(Dir=='U')
					Draw.Dot(this, tag, false, BarsAgo, Price, pUpMarkerBrush);
				else
					Draw.Dot(this, tag, false, BarsAgo, Price, pDownMarkerBrush);
			}
			else if(type==TwinRangeFilter_MarkerType.Diamond){
				if(Dir=='U')
					Draw.Diamond(this, tag, false, BarsAgo, Price, pUpMarkerBrush);
				else
					Draw.Diamond(this, tag, false, BarsAgo, Price, pDownMarkerBrush);
			}
			else if(type==TwinRangeFilter_MarkerType.Square){
				if(Dir=='U')
					Draw.Square(this, tag, false, BarsAgo, Price, pUpMarkerBrush);
				else
					Draw.Square(this, tag, false, BarsAgo, Price, pDownMarkerBrush);
			}
			#endregion
		}
		protected override void OnBarUpdate()
		{
			rngfilt[0]  = Close[0];
			upward[0]   = 0;
			downward[0] = 0;
			CondIni[0]  = 0;

			if(CurrentBar<2) return;

			DiffCloses[0] = Math.Abs(Close[0]-Close[1]);
			if(CurrentBar<Math.Max(Period1,Period2)+1) return;

			avgrng1[0] = EMA_diff1[0];
			double smrng1 = EMA_avgrng1[0] * Per1Mult;

			avgrng2[0] = EMA_diff2[0];
			double smrng2 = EMA_avgrng2[0] * Per2Mult;

			double smrng = (smrng1+smrng2)/2.0;
#region -- original code --
/*
study(title="Twin Range Filter", overlay=true)

source = input(defval=close, title="Source")

// Smooth Average Range

per1  = input(defval=27, minval=1, title="Fast period")
mult1 = input(defval=1.6, minval=0.1, title="Fast range")

per2  = input(defval=55, minval=1, title="Slow period")
mult2 = input(defval=2, minval=0.1, title="Slow range")

smoothrng(close, period, mult) =>
    wper = period * 2 - 1
    avrng = ema(abs(close - close[1]), period)
    smoothrng = ema(avrng, wper) * mult
    smoothrng
smrng1 = smoothrng(source, per1, mult1)
smrng2 = smoothrng(source, per2, mult2)
smrng = (smrng1 + smrng2) / 2

// Range Filter

rngfilt(close, smrg) =>
    rngfilt = close
    rngfilt := 
			close > rngfilt[1] ? 
				(close - smrg < rngfilt[1] ? rngfilt[1] : close - smrg)
			: 
			     close + smrg > rngfilt[1] ? rngfilt[1] : close + smrg)
    rngfilt
filt = rngfilt(source, smrng)

upward = 0.0
upward := filt[0] > filt[1] ? upward[1] + 1 : filt < filt[1] ? 0 : upward[1]
downward = 0.0
downward := filt[0] < filt[1] ? downward[1] + 1 : filt > filt[1] ? 0 : downward[1]

hband = filt + smrng
lband = filt - smrng

longCond = bool(na)
shortCond = bool(na)
longCond := source > filt and source > source[1] and upward > 0 or source > filt and source < source[1] and upward > 0
shortCond := source < filt and source < source[1] and downward > 0 or source < filt and source > source[1] and downward > 0

CondIni = 0
CondIni := longCond ? 1 : shortCond ? -1 : CondIni[1]

long  = longCond  and CondIni[1] == -1
short = shortCond and CondIni[1] == 1

// Plotting
plotshape(long, title="Long", text="Long", style=shape.labelup, textcolor=color.black, size=size.tiny, location=location.belowbar, color=color.lime, transp=0)
plotshape(short, title="Short", text="Short", style=shape.labeldown, textcolor=color.white, size=size.tiny, location=location.abovebar, color=color.red, transp=0)

// Alerts
alertcondition(long, title="Long", message="Long")
alertcondition(short, title="Short", message="Short")
*/
#endregion
			rngfilt[0] = 0;
			if(Close[0] > rngfilt[1]) 
				rngfilt[0] = (Close[0] - smrng < rngfilt[1] ? rngfilt[1] : Close[0] - smrng);
			else  
				rngfilt[0] = (Close[0] + smrng > rngfilt[1] ? rngfilt[1] : Close[0] + smrng);

			upward[0]   = rngfilt[0] > rngfilt[1] ? upward[1]+1 : (rngfilt[0] < rngfilt[1] ? 0 : upward[1]);
			downward[0] = rngfilt[0] < rngfilt[1] ? downward[1]+1 : (rngfilt[0] > rngfilt[1] ? 0 : downward[1]);

			bool buy  = Close[0] > rngfilt[0] && Close[0] > Close[1] && upward[0]>0   || Close[0] > rngfilt[0] && Close[0] < Close[1] && upward[0] > 0;
			bool sell = Close[0] < rngfilt[0] && Close[0] < Close[1] && downward[0]>0 || Close[0] < rngfilt[0] && Close[0] > Close[1] && downward[0] > 0;
			
			CondIni[0] = buy ? 1 : (sell ? -1 : CondIni[1]);
			if(FilterMAPeriod>0){
				TrendFilter[0] = Instrument.MasterInstrument.RoundToTickSize(FilterEMA[0]);
				buy  = Close[1] > TrendFilter[1]/* && Low[0] <= MIN(Low, 4)[1]*/ && buy;
				sell = Close[1] < TrendFilter[1]/* && High[0] >= MAX(High, 4)[1]*/ && sell;
			}

			BuySig.Reset(0);
			SellSig.Reset(0);
			if(buy && CondIni[1] == -1) {
				LastSignalDir = 'B';
				LastSignalABar = CurrentBar;
				if(ChartControl != null){
					if(pSignalMarker!=TwinRangeFilter_MarkerType.None) DrawMarker(Lows[0][0]-TickSize, 0, 'U', pSignalMarker);
					else if(pSignalMarker == TwinRangeFilter_MarkerType.Triangle) BuySig[0] = Low[0]-TickSize;
					if(pUpFloodOpacity >0 && UpFloodBrush!=Brushes.Transparent)   BackBrushesAll[0] = UpFloodBrush;
				}else{
					BuySig[0] = Low[0]-TickSize;
				}
				#region -- Do Sound Alert --
				if(AlertABar!=CurrentBars[0]) {
					if(pPrintToAlertsWindow)
						Alert(CurrentBar.ToString(),pAlertPriority,"Buy on TwinRangeFilter",AddSoundFolder(pWAVOnBuyEntry),1,Brushes.Green,Brushes.White);  
					else
						PlaySound(AddSoundFolder(pWAVOnBuyEntry));
					AlertABar = CurrentBars[0];
				}
				#endregion
			}
			if(sell && CondIni[1] == 1) {
				LastSignalDir = 'S';
				LastSignalABar = CurrentBar;
				if(ChartControl != null){
					if(pSignalMarker!=TwinRangeFilter_MarkerType.None) DrawMarker(Highs[0][0]+TickSize, 0, 'D', pSignalMarker);
					else if(pSignalMarker == TwinRangeFilter_MarkerType.Triangle)   SellSig[0] = High[0]+TickSize;
					if(pDownFloodOpacity >0 && DownFloodBrush!=Brushes.Transparent) BackBrushesAll[0]  = DownFloodBrush;
				}else{
					SellSig[0] = High[0]+TickSize;
				}
				#region -- Do Sound Alert --
				if(AlertABar!=CurrentBars[0]) {
					if(pPrintToAlertsWindow)
						Alert(CurrentBar.ToString(),pAlertPriority,"Sell on TwinRangeFilter",AddSoundFolder(pWAVOnSellEntry),1,Brushes.Red,Brushes.White);  
					else
						PlaySound(AddSoundFolder(pWAVOnSellEntry));
					AlertABar = CurrentBars[0];
				}
				#endregion
			}
			if(CurrentBar>3 && CurrentBar - LastSignalABar < 4){
				StopEntryPrice.Reset(0);
				SLPrice.Reset(0);
				TgtPrice.Reset(0);
				if(LastSignalDir == 'B'){
					StopEntryPrice[0] = Math.Max(Highs[0][1], Highs[0][2]);
					#region -- Set risk and target --
					if(pDollarRisk>0){
//						Draw.Square(this, string.Format("TRF sl ${0} {1}",this.pDollarRisk,CurrentBar), false, 0, StopEntryPrice[0] - PtsRisked, Brushes.Yellow);
						SLPrice[0] = StopEntryPrice[0] - PtsRisked;
						if(this.pRewardToRisk>0) //Draw.Square(this, string.Format("TRF ${0} tgt{1}",pRewardToRisk*pDollarRisk,CurrentBar), false, 0, StopEntryPrice[0]+PtsReward, Brushes.Green);
							TgtPrice[0] = StopEntryPrice[0] + PtsReward;
					}
					#endregion
				}
				if(LastSignalDir == 'S'){
					StopEntryPrice[0] = Math.Min(Lows[0][1], Lows[0][2]);
					#region -- Set risk and target --
					if(pDollarRisk>0){
//						Draw.Square(this, string.Format("TRF sl ${0} {1}",this.pDollarRisk,CurrentBar), false, 0, StopEntryPrice[0] + PtsRisked, Brushes.Yellow);
						SLPrice[0] = StopEntryPrice[0] + PtsRisked;
						if(this.pRewardToRisk>0) //Draw.Square(this, string.Format("TRF ${0} tgt{1}",pRewardToRisk*pDollarRisk,CurrentBar), false, 0, StopEntryPrice[0]-PtsReward, Brushes.Green);
							TgtPrice[0] = StopEntryPrice[0] - PtsReward;
					}
					#endregion
				}
				if(StopEntryPrice.IsValidDataPoint(1) && LastSignalABar == CurrentBar-1 && !Trades.ContainsKey(CurrentBar)){
					double KeyPrice = Opens[0][0];
					if(LastSignalDir == 'B' && Low[0]<=KeyPrice){
						#region -- exit existing position, enter a long position --
						GoFlat('S', KeyPrice);
						int positions = Trades.Where(k=>k.Value.ExitABar==0).Count();
						if(positions==0) {
							Trades[CurrentBar] = new info('B', KeyPrice, KeyPrice - PtsRisked, KeyPrice + PtsReward);
							StopEntryPrice.Reset(0);
						}
						#endregion
					}
					else if(LastSignalDir == 'S' && High[0]>=KeyPrice){
						#region -- exit existing position, enter a short position --
						GoFlat('B', KeyPrice);
						int positions = Trades.Where(k=>k.Value.ExitABar==0).Count();
						if(positions==0) {
							Trades[CurrentBar] = new info('S', KeyPrice, KeyPrice + PtsRisked, KeyPrice - PtsReward);
							StopEntryPrice.Reset(0);
						}
						#endregion
					}
				}
			}
			var keys = Trades.Where(k=> k.Value.ExitABar==0).Select(k=>k.Key);
			if(keys!=null){
				foreach(var i in keys){
					if(FilterMAPeriod>0){
						if(Trades[i].Direction=='B') Trades[i].SLPrice = Math.Max(Trades[i].SLPrice,TrendFilter[1]);
						if(Trades[i].Direction=='S') Trades[i].SLPrice = Math.Min(Trades[i].SLPrice,TrendFilter[1]);
					}
					if(Trades[i].Direction=='B' && Low[0]  < Trades[i].SLPrice && Trades[i].SLPrice!=0) GoFlat(i, Trades[i].SLPrice);
					if(Trades[i].Direction=='B' && High[0] > Trades[i].TPPrice && Trades[i].TPPrice!=0) GoFlat(i, Trades[i].TPPrice);
					if(Trades[i].Direction=='S' && Low[0]  < Trades[i].TPPrice && Trades[i].TPPrice!=0) GoFlat(i, Trades[i].TPPrice);
					if(Trades[i].Direction=='S' && High[0] > Trades[i].SLPrice && Trades[i].SLPrice!=0) GoFlat(i, Trades[i].SLPrice);
				}
			}
			if(CurrentBar > Bars.Count-3 && !PrintedResults){
				PrintedResults = true;
				#region -- Calculate summary at end of bar processing --
					ClearOutputWindow();
					double WinCount=0;
					double LossCount=0;
					foreach(var kvp in Trades){
						if(kvp.Value.Direction=='B'){
							kvp.Value.PnL = kvp.Value.ExitPrice-kvp.Value.EntryPrice;
							if(kvp.Value.PnL>0) WinCount++; else LossCount++;
						}
						if(kvp.Value.Direction=='S'){
							kvp.Value.PnL = kvp.Value.EntryPrice-kvp.Value.ExitPrice;
							if(kvp.Value.PnL>0) WinCount++; else LossCount++;
						}
					}
					double ScalpCommissionsDollars = (WinCount+LossCount)*4;
					double PnL = 0;
					WinCount   = 0;
					LossCount  = 0;
					double profit = 0;
					DateTime today = DateTime.MinValue;
					Print(" ----- "+Name+" trade results on "+BarsArray[0].Instrument.FullName+" "+Bars.BarsPeriod.ToString());
					int WinningDay = 0;
					foreach(var kvp in Trades){
						if(kvp.Value.ExitABar<=0) continue;
						profit = kvp.Value.PnL;
						var EntryDT = Times[0].GetValueAt(kvp.Key);
						if(today.Day != EntryDT.Day) PnL = 0; today = EntryDT;
						if(EntryDT.Day == WinningDay) continue;//skip any trades that happen after the daily profit target has been achieved
						PnL = PnL + profit;
						if(profit>0) WinCount++; else LossCount++;
						double WLPct = WinCount*1.0/(WinCount+LossCount);
						double PnLDollars = PnL*Instrument.MasterInstrument.PointValue;
						var ExitDT = Times[0].GetValueAt(kvp.Value.ExitABar);
						var mins = new TimeSpan(ExitDT.Ticks-EntryDT.Ticks);
						Print(
							string.Format("{0}:\t {1}",
								EntryDT.ToString(),
								PnLDollars.ToString("C"))+
							string.Format("\t  P/L {0}-tks",(profit/TickSize).ToString("0"))+
							string.Format("\t  w/l: {0}&{1} {2}", WinCount, LossCount, WLPct.ToString("0.0%"))+
							string.Format("  {0}-minutes {1}-{2} at {3}", 
								mins.TotalMinutes.ToString("0.00"),
								kvp.Value.EntryPrice,
								kvp.Value.ExitPrice, 
								ExitDT.ToString())
						);
						if(pDailyProfitTarget>0 && PnLDollars>pDailyProfitTarget) {
							WinningDay = Times[0].GetValueAt(kvp.Key).Day;
							Print(" ----- "+Name+" Profit target ($"+pDailyProfitTarget+") hit today!");
						}
					}
					Trades.Clear();
				#endregion
			}
		}
//=======================================================================
		private void GoFlat(char Direction, double ExitPrice){
			var keys = Trades.Where(k=>k.Value.ExitABar==0 && (Direction==' ' ? true : k.Value.Direction==Direction)).Select(k=>k.Key).ToList();
			if(keys!=null && keys.Count>0) {
				foreach(var k in keys) GoFlat(k, ExitPrice);
			}
		}
		private void GoFlat(int TradeId, double ExitPrice){
			if(Trades.ContainsKey(TradeId)){
				Trades[TradeId].ExitABar  = CurrentBar;
				Trades[TradeId].ExitPrice = ExitPrice;
//if(inzone)Print(Trades[k].Direction+":  Entry: "+Trades[k].EntryPrice+"  exit: "+Trades[k].ExitPrice);
				double profit = Trades[TradeId].Direction=='B' ? Trades[TradeId].ExitPrice-Trades[TradeId].EntryPrice : Trades[TradeId].EntryPrice-Trades[TradeId].ExitPrice;
				var tag = string.Format("{0}T{1}", Trades[TradeId].Direction,TradeId.ToString());
//Print("Drawing line:  "+tag);
				if(pPrintTradePnLLines) Draw.Line(this, tag, false, Times[0].GetValueAt(TradeId), Trades[TradeId].EntryPrice, Times[0].GetValueAt(Trades[TradeId].ExitABar), ExitPrice,  profit> 0 ? Brushes.Green:Brushes.Red, DashStyleHelper.Dash,4); 
			}
		}
//trade data ----------------------------------------------------------------
		private class info {
			public double EntryPrice = 0;
			public char   Direction = ' ';
			public int    ExitABar = 0;
			public double ExitPrice = 0;
			public double SLPrice = 0;
			public double TPPrice = 0;
			public double PnL = 0;
			public info (char dir, double entryprice, double slPrice, double tpPrice){Direction=dir; EntryPrice=entryprice; SLPrice=slPrice; TPPrice=tpPrice;}
		}
		private SortedDictionary<int,info> Trades = new SortedDictionary<int,info>();
//end of trade data ----------------------------------------------------------------

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

				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_BuyEntry.wav");
				list.Add("<inst>_SellSetup.wav");
				list.Add("<inst>_SellEntry.wav");
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
		#region Properties

		#region -- Alerts --
		[Display(Name="Print to Alerts Window?", Order=10, GroupName="Alerts")]
		public bool pPrintToAlertsWindow { get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Wav on Buy Entry", GroupName = "Alerts", Order = 40)]
		public string pWAVOnBuyEntry {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Wav on Sell Entry", GroupName = "Alerts", Order = 41)]
		public string pWAVOnSellEntry {get;set;}

		private NinjaTrader.NinjaScript.Priority pAlertPriority = NinjaTrader.NinjaScript.Priority.High;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Alert priority", GroupName = "Alerts", Order = 41, Description="Set the priority for these alerts...the Alerts window sorts information based on this setting")]
		public NinjaTrader.NinjaScript.Priority AlertPriority
		{
			get { return pAlertPriority; }
			set { pAlertPriority = value; }
		}
		#endregion

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period1", Order=10, GroupName="Parameters")]
		public int Period1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1.01, double.MaxValue)]
		[Display(Name="Per1Mult", Order=20, GroupName="Parameters")]
		public double Per1Mult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period2", Order=30, GroupName="Parameters")]
		public int Period2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Per2Mult", Order=40, GroupName="Parameters")]
		public double Per2Mult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Filter MA Period", Order=60, GroupName="Parameters")]
		public int FilterMAPeriod
		{ get; set; }

		#region -- Strategy --
		[Range(0, double.MaxValue)]
		[Display(Name="Dollar Risk", Order=10, GroupName="Strategy")]
		public int pDollarRisk
		{ get; set; }

		[Range(0, double.MaxValue)]
		[Display(Name="Reward:Risk", Order=20, GroupName="Strategy")]
		public double pRewardToRisk
		{ get; set; }

		[Range(0, double.MaxValue)]
		[Display(Name="Daily profit target", Order=30, GroupName="Strategy")]
		public double pDailyProfitTarget {get;set;}

		[Display(Name="Marker Type?", Order=40, GroupName="Strategy")]
		public TwinRangeFilter_MarkerType pSignalMarker {get;set;}
		
        [Display(Order = 50, Name = "Draw PnL lines", GroupName = "Strategy", Description = "Draw lines from entry to exit")]
		public bool pPrintTradePnLLines {get;set;}

		#endregion
		

        [XmlIgnore]
        [Display(Name = "Up Marker Color", GroupName = "Visuals", Description = "Select Color", Order = 30)]
        public Brush pUpMarkerBrush { get; set; }
		        [Browsable(false)]
		        public string UpMarkerSerialize {get { return Serialize.BrushToString(pUpMarkerBrush); } set { pUpMarkerBrush = Serialize.StringToBrush(value); }}

        [XmlIgnore]
        [Display(Name = "Down Marker Color", GroupName = "Visuals", Description = "Select Color", Order = 30)]
        public Brush pDownMarkerBrush { get; set; }
		        [Browsable(false)]
		        public string DownMarkerSerialize {get { return Serialize.BrushToString(pDownMarkerBrush); } set { pDownMarkerBrush = Serialize.StringToBrush(value); }}

		#region -- Bkg Flood --
        [XmlIgnore]
        [Display(Name = "Up Flood", GroupName = "Visuals", Description = "Select Color", Order = 30)]
        public Brush pUpFloodBrush { get; set; }
		        [Browsable(false)]
		        public string UpBkgSerialize {get { return Serialize.BrushToString(pUpFloodBrush); } set { pUpFloodBrush = Serialize.StringToBrush(value); }}

        [Display(Name = "Up Flood Opacity", GroupName = "Visuals", Description = "0=transparent, 100=opaque", Order = 31)]
		public int pUpFloodOpacity {get;set;}

		[XmlIgnore]
        [Display(Name = "Down Flood", GroupName = "Visuals", Description = "Select Color", Order = 40)]
        public Brush pDownFloodBrush { get; set; }
		        [Browsable(false)]
		        public string DownBkgSerialize {get { return Serialize.BrushToString(pDownFloodBrush); } set { pDownFloodBrush = Serialize.StringToBrush(value); }}

		[Display(Name = "Down Flood Opacity", GroupName = "Visuals", Description = "0=transparent, 100=opaque", Order = 41)]
		public int pDownFloodOpacity {get;set;}
		#endregion
		#endregion

		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySig
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellSig
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendFilter
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StopEntryPrice
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SLPrice
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TgtPrice
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
		private TwinRangeFilter[] cacheTwinRangeFilter;
		public TwinRangeFilter TwinRangeFilter(int period1, double per1Mult, int period2, double per2Mult, int filterMAPeriod)
		{
			return TwinRangeFilter(Input, period1, per1Mult, period2, per2Mult, filterMAPeriod);
		}

		public TwinRangeFilter TwinRangeFilter(ISeries<double> input, int period1, double per1Mult, int period2, double per2Mult, int filterMAPeriod)
		{
			if (cacheTwinRangeFilter != null)
				for (int idx = 0; idx < cacheTwinRangeFilter.Length; idx++)
					if (cacheTwinRangeFilter[idx] != null && cacheTwinRangeFilter[idx].Period1 == period1 && cacheTwinRangeFilter[idx].Per1Mult == per1Mult && cacheTwinRangeFilter[idx].Period2 == period2 && cacheTwinRangeFilter[idx].Per2Mult == per2Mult && cacheTwinRangeFilter[idx].FilterMAPeriod == filterMAPeriod && cacheTwinRangeFilter[idx].EqualsInput(input))
						return cacheTwinRangeFilter[idx];
			return CacheIndicator<TwinRangeFilter>(new TwinRangeFilter(){ Period1 = period1, Per1Mult = per1Mult, Period2 = period2, Per2Mult = per2Mult, FilterMAPeriod = filterMAPeriod }, input, ref cacheTwinRangeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TwinRangeFilter TwinRangeFilter(int period1, double per1Mult, int period2, double per2Mult, int filterMAPeriod)
		{
			return indicator.TwinRangeFilter(Input, period1, per1Mult, period2, per2Mult, filterMAPeriod);
		}

		public Indicators.TwinRangeFilter TwinRangeFilter(ISeries<double> input , int period1, double per1Mult, int period2, double per2Mult, int filterMAPeriod)
		{
			return indicator.TwinRangeFilter(input, period1, per1Mult, period2, per2Mult, filterMAPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TwinRangeFilter TwinRangeFilter(int period1, double per1Mult, int period2, double per2Mult, int filterMAPeriod)
		{
			return indicator.TwinRangeFilter(Input, period1, per1Mult, period2, per2Mult, filterMAPeriod);
		}

		public Indicators.TwinRangeFilter TwinRangeFilter(ISeries<double> input , int period1, double per1Mult, int period2, double per2Mult, int filterMAPeriod)
		{
			return indicator.TwinRangeFilter(input, period1, per1Mult, period2, per2Mult, filterMAPeriod);
		}
	}
}

#endregion
