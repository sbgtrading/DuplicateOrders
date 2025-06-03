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


//Developed by Ben Letto (ben.letto@sbgtradingcorp.com)
//   June 23, 2023


namespace NinjaTrader.NinjaScript.Indicators
{
	public class DivergenceSpotterV3 : Indicator
	{
/*
This new version had updated internals (all basis indicators are instantiated, as per the best practices for NT8), plus addition of:

1)  Trade Entry Price - indicator now calculates a support/resistance break price level, for confirmation of a divergence trade.
On a short divergence signal, the lowest price low is found on the price bars between the two basis indicator high peaks.  This is a support level that, if broken, may be considered a trade entry.  The indicator draws a down triangle at this price level.

On a long divergence signal, the highest price low is found on the price bars between the two basis indicator low peaks.  This is a resistance level that, if broken, may be considered a trade entry.  The indicator draws an up triangle at this price level

2)  Sound alerts - One set of WAV files for when a new divergence setup is forming, and a second set of WAV files for when price confirms the divergence setup and pushes beyond the specific entry level (calculated as described above and marked on the chart as an up or down pointing triangle chart marker)

3)  2 new basis indicator types are available.  "MACDHisto" and "MultiMACD".
There are now 16 different basis indicators available.  The MACD now has 3 variations.  MACDLine, which is the macd line, otherwise known as the "main" line (the result of subtracting the slow moving EMA from the fast moving EMA.  Also, we now have the MACDHisto, which is the difference between the MACD main line, and the MACD Signal (or smoothed) line.
And a 3rd option is the MultiMACD.  This is a conglomerate histogram composed of the histograms of 4 different slow moving MACD histograms.  Those 4 MACD's are an 8,20,20, a 10,20,20, a 20,20,20 and a 60,240,20.
*/
		private DivergenceSpotter_IndicatorType indicatorType; // Default setting for IndicatorType
        private int fastPeriod; // Default setting for FastPeriod
        private int slowPeriod; // Default setting for SlowPeriod
        private int signalPeriod; // Default setting for SignalPeriod
		private int lookbackPeriod; //If "0", then lookback to first instance of indicator crossing 0 line
											//if ">0", then lookback only that amount of bars
		private int barSensitivity; //small values increase divergences, large values decrease
										  //ideal value is 50% of lookbackPeriod
		private int bkgopacity;
		private int LastSignal;
		private int BarOfLastSignal;

		// User defined variables (add any user defined variables below)
		private int bardiff, i;
		private bool DoneSearching;
		private Series<double> TheIndicator;
		MACD macd;
		TRIX trix;
		StochasticsFast stochf;
		StochRSI stochrsi;
		CCI cci;
		BOP bop;
		CMO cmo;
		ChaikinMoneyFlow cmf;
		Momentum mom;
		MFI mfi;
		RSI rsi;
		RelativeVigorIndex relative_vigor;
		ROC roc;
		Stochastics sto;
		FisherTransform ftf;
		MACD MACD1;
		MACD MACD2;
		MACD MACD3;
		EMA ema1, ema2;
		string Desc = "DivergenceSpotter v3";
		int BarsAtStartup = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				#region -- SetDefaults --
				Description									= @"";
				Name										= "DivergenceSpotter v3";
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
				IsAutoScale 								= false;
				
				indicatorType = DivergenceSpotter_IndicatorType.MACDLine; // Default setting for IndicatorType
		        fastPeriod = 12; // Default setting for FastPeriod
		        slowPeriod = 26; // Default setting for SlowPeriod
		        signalPeriod = 9; // Default setting for SignalPeriod
				lookbackPeriod = 0; //If "0", then lookback to first instance of indicator crossing 0 line
													//if ">0", then lookback only that amount of bars
				barSensitivity=3; //small values increase divergences, large values decrease
												  //ideal value is 50% of lookbackPeriod
				bkgopacity = 3;
				LastSignal = 0;
				BarOfLastSignal=-10;

				// User defined variables (add any user defined variables below)
				bardiff=0;
				DoneSearching = true; 
				pEMA1period = 50;
				pEMA2period = 100;
				
				pSignalPlotOnEntryTriangles = true;
				pSignalPlotOnPotentialDots = false;

				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "BuySignal");
				AddPlot(new Stroke(Brushes.DeepPink, 2), PlotStyle.Dot, "SellSignal");
				AddPlot(new Stroke(Brushes.Lime, 4), PlotStyle.TriangleUp, "BuyLevel");
				AddPlot(new Stroke(Brushes.Magenta, 4), PlotStyle.TriangleDown, "SellLevel");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Dot, "Signal");
				#endregion
			}
			else if (State == State.DataLoaded)
			{
				bkgopacity = 3;
				BarsAtStartup = BarsArray[0].Count;
				#region -- DataLoaded --
				if(pEMA1period > 0 && pEMA2period > 0){
					ema1 = EMA(pEMA1period);
					ema2 = EMA(pEMA2period);
				}else {ema1 = null; ema2 = null;}

				TheIndicator = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Desc = "DivSpot "+BarsPeriod.ToString();
				if(indicatorType == DivergenceSpotter_IndicatorType.MACDLine){
					macd = MACD(fastPeriod,slowPeriod,signalPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter MACD({0},{1})",fastPeriod,slowPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.MACDHisto){
					macd = MACD(fastPeriod,slowPeriod,signalPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter MACD histo({0},{1},{2})",fastPeriod,slowPeriod,signalPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.MultiMACD){
	                MACD1 = MACD(8, 20, 20);
	                MACD2 = MACD(10, 20, 20);
	                MACD3 = MACD(20, 60, 20);
	                macd = MACD(60, 240, 20);
					if(ChartControl !=null) Desc = "DivSpotter MultiMACD";
				}

				else if(indicatorType == DivergenceSpotter_IndicatorType.TRIX){
					trix = TRIX(slowPeriod,signalPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter TRIX({0},{1})",slowPeriod,signalPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.StochRSI){
					stochrsi = StochRSI(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter StoRSI({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.CCI){
					cci = CCI(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter CCI({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.BOP){
					bop = BOP(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter BOP({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.CMO){
					cmo = CMO(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter CMO({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.ChaikinMoneyFlow){
					cmf = ChaikinMoneyFlow(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter ChaikinMF({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.Momentum){
					mom = Momentum(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter Momentum({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.MFI){
					mfi = MFI(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter MFI({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.RSI){
					rsi = RSI(fastPeriod,signalPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter RSI({0},{1})",fastPeriod,signalPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.ROC){
					roc = ROC(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter ROC({0})",fastPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.Fisher){
					ftf = FisherTransform(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter Fisher({0})",fastPeriod);
				}
	            else if (indicatorType == DivergenceSpotter_IndicatorType.StochSlowD){
					sto = Stochastics(slowPeriod, fastPeriod, signalPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter Stoch.D({0},{1},{2})",slowPeriod,fastPeriod,signalPeriod);
				}
				else if(indicatorType == DivergenceSpotter_IndicatorType.StochFastK){
					stochf = StochasticsFast(slowPeriod, fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter StochF.K({0},{1})",slowPeriod,fastPeriod);
				}
	            else if (indicatorType == DivergenceSpotter_IndicatorType.StochFastD){
					stochf = StochasticsFast(slowPeriod, fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter StochF.D({0},{1})",slowPeriod,fastPeriod);
				}
				else if (indicatorType == DivergenceSpotter_IndicatorType.RelVigorIndex){
					relative_vigor = RelativeVigorIndex(fastPeriod);
					if(ChartControl !=null) Desc = string.Format("DivSpotter RelVigorIdx({0})",fastPeriod);
				}
				#endregion
			}
		}
        //public override string DisplayName { get { return Desc; } }

		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			if(CurrentBar > BarsAtStartup)
				Alert(id,prio,msg,wav,rearmSeconds,bkgBrush, foregroundBrush);
			//printDebug(string.Format("Alert: {0}   wav: {1}",msg,wav));
		}

		double LongEntryPrice = 0;
		double ShortEntryPrice = 0;
		int LONG = 1;
		int SHORT = -1;
		int LastSignalPlotValue = 0;
		DateTime TimeOfLastSignalPlot = DateTime.MinValue;
		protected override void OnBarUpdate()
		{
			int LowestBarIndexPossible = Math.Max(fastPeriod,Math.Max(slowPeriod,lookbackPeriod));
			if(CurrentBar < 1+LowestBarIndexPossible) return;
			#region -- Calculate indicator value --
			if(indicatorType == DivergenceSpotter_IndicatorType.MACDLine){
				TheIndicator[0] = macd[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.MACDHisto){
				TheIndicator[0] = macd.Diff[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.MultiMACD){
				TheIndicator[0] = MACD1.Diff[0] + MACD2.Diff[0] + MACD3.Diff[0] + macd.Diff[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.TRIX){
				TheIndicator[0] = trix[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.StochRSI){
				TheIndicator[0] = stochrsi[0]-0.5;
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.CCI){
				TheIndicator[0] = cci[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.BOP){
				TheIndicator[0] = bop[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.CMO){
				TheIndicator[0] = cmo[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.ChaikinMoneyFlow){
				TheIndicator[0] = cmf[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.Momentum){
				TheIndicator[0] = mom[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.MFI){
				TheIndicator[0] = mfi[0] - 50;
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.RSI){
				TheIndicator[0] = rsi[0]-50; //signalPeriod is here, but is ignored in the divergence calc
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.ROC){
				TheIndicator[0] = roc[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.RelVigorIndex){
				TheIndicator[0] = relative_vigor[0];
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.Fisher){
				TheIndicator[0] = ftf[0];
			}
            else if (indicatorType == DivergenceSpotter_IndicatorType.StochSlowD){
				TheIndicator[0] = sto.D[0] - 50;
			}
			else if(indicatorType == DivergenceSpotter_IndicatorType.StochFastK){
				TheIndicator[0] = stochf.K[0]-50;
			}
            else if (indicatorType == DivergenceSpotter_IndicatorType.StochFastD){
				TheIndicator[0] = stochf.D[0] - 50;
			}
			#endregion

			double Hprice = double.MinValue;
			double Lprice = High[0];
			double Hindicator = TheIndicator[0];
			double Lindicator = TheIndicator[0];
			int HPriceBar = -1;
			int LPriceBar = -1;
			int LIndicatorBar = -1;
			int HIndicatorBar = -1;
			i=1;
			DoneSearching=false;
			while(!DoneSearching)
			{
				if(High[i]>Hprice) //a new high in price was found, record it
				{	Hprice = High[i];
					HPriceBar = i; //this is the bar number of the highest price in the lookbackPeriod
				}
			 	if(Low[i]<Lprice) //a new low in price was found, record it
				{	Lprice = Low[i];
					LPriceBar = i;//this is the bar number of the lowest price in the lookbackPeriod
				}
				if(TheIndicator[i]>Hindicator) //a new high in the indicator was found, record it
				{	Hindicator = TheIndicator[i];
					HIndicatorBar = i;//this is the bar number of the highest indicator value in the lookbackPeriod
				}
				if(TheIndicator[i]<Lindicator) //a new low in the indicator was found, record it
				{	Lindicator = TheIndicator[i];
					LIndicatorBar = i;//this is the bar number of the lowest indicator value in the lookbackPeriod
				}
				i++;

				if(i>CurrentBar-LowestBarIndexPossible-1) DoneSearching=true; //limit of search reached
				if(lookbackPeriod==0)//then find the last time the indicator crossed the "0" line
				{	if(TheIndicator[i] * TheIndicator[i+1] < 0.0) DoneSearching=true;
				}
				else //terminate the search when i has exceeded the lookbackPeriod
				{	if(i > lookbackPeriod) DoneSearching=true;
				}
			}

			if(IsFirstTickOfBar){
				SellSignal.Reset(0);
				BuySignal.Reset(0);
			}
			int FilterDirection = 0;
			if(ema1!=null && ema2!=null){
				if(ema1[0] > ema2[0]) FilterDirection = LONG;
				else FilterDirection = SHORT;
			}
			Signal[0] = 0;
			BackBrush = null;
			if(CurrentBar - BarOfLastSignal > 10) LastSignal=0;
			if(TheIndicator[0]>0) //look for sell divergence since Indicator is above ZERO
			{
				bardiff = HIndicatorBar-HPriceBar; //How many bars separated the highs?
				if(bardiff>barSensitivity) {
					if(LPriceBar>0 && HIndicatorBar>0 && pShowEntryLevels && FilterDirection <= 0){
						var p = Low[0];
						for(int j = 1; j<HIndicatorBar; j++){
							if(p > Low[j]){
								LPriceBar = j;
								p = Low[j];
							}
						}
						ShortEntryPrice = p;
						SellLevel[0] = p;
						if(pSignalPlotOnPotentialDots){
							Signal[0] = SHORT;
							if(State==State.Realtime){
								LastSignalPlotValue = SHORT;
								if(TimeOfLastSignalPlot == DateTime.MinValue) TimeOfLastSignalPlot = DateTime.Now;
							}
						}
//						Draw.TriangleDown(this,string.Format("SELL price {0}",CurrentBar-LPriceBar),false,Times[0][0], p, Brushes.Magenta);
						if(!SellSignal.IsValidDataPoint(1)) myAlert(string.Format("DivSpot Sell{0}",CurrentBar.ToString()), Priority.High, "SELL divergence potential", AddSoundFolder(pSellSetupWAV), 10, Brushes.Black,Brushes.Magenta);
					}
					SellSignal[0] = Low[0]-Math.Max((High[0]-Low[0])/2,5*TickSize);
					if(LastSignal!=-1 && bkgopacity>0) BackBrush = new SolidColorBrush(Color.FromArgb((byte)Math.Round(255.0*bkgopacity/10,0),255,51,153));
					LastSignal = -1;
					BarOfLastSignal = CurrentBar;
				}
			} else //look for Buy divergence since Indicator is below ZERO
			{
				bardiff = LIndicatorBar-LPriceBar; //How many bars separated the lows?
				if(bardiff>barSensitivity) {
					if(HPriceBar>0 && LIndicatorBar>0 && pShowEntryLevels && FilterDirection >= 0){
						var p = High[0];
						for(int j = 1; j<LIndicatorBar; j++){
							if(p < High[j]){
								HPriceBar = j;
								p = High[j];
							}
						}
						LongEntryPrice = p;
						BuyLevel[0] = p;
						if(pSignalPlotOnPotentialDots){
							Signal[0] = LONG;
							if(State==State.Realtime){
								LastSignalPlotValue = LONG;
								if(TimeOfLastSignalPlot == DateTime.MinValue) TimeOfLastSignalPlot = DateTime.Now;
							}
						}
//						Draw.TriangleUp(this,string.Format("BUY price {0}",CurrentBar-HPriceBar),false,Times[0][0], p, Brushes.Lime);
						if(!BuySignal.IsValidDataPoint(1)) myAlert(string.Format("DivSpot Buy{0}",CurrentBar.ToString()), Priority.High, "BUY divergence potential", AddSoundFolder(pBuySetupWAV), 10, Brushes.Black,Brushes.Lime);
					}
					BuySignal[0] = High[0]+Math.Max((High[0]-Low[0])/2,5*TickSize);
					if(LastSignal!=1 && bkgopacity>0) BackBrush = new SolidColorBrush(Color.FromArgb((byte)Math.Round(255.0*bkgopacity/10,0),0,0,255));
					LastSignal = 1;
					BarOfLastSignal = CurrentBar;
				}
			}
			if(pShowEntryLevels){
				if(LongEntryPrice !=0 && pBuyEntryWAV.CompareTo("none")!=0){
					if(High[0]>=LongEntryPrice){
						myAlert(string.Format("DivSpot LONG{0}",CurrentBar.ToString()), Priority.High, "BUY ENTRY", AddSoundFolder(pBuyEntryWAV), 10, Brushes.Black,Brushes.Lime);
						if(pSignalPlotOnEntryTriangles){
							if(State==State.Realtime){
								LastSignalPlotValue = LONG;
								if(TimeOfLastSignalPlot == DateTime.MinValue) TimeOfLastSignalPlot = DateTime.Now;
							}
							Signal[0] = LONG;
						}
//						Print(Times[0][0].ToString()+ "  DivSpotter LONG at "+LongEntryPrice);
						LongEntryPrice = 0;
					}
				}
				if(ShortEntryPrice !=0 && pSellEntryWAV.CompareTo("none")!=0){
					if(Low[0]<=ShortEntryPrice){
						myAlert(string.Format("DivSpot SHORT{0}",CurrentBar.ToString()), Priority.High, "SELL ENTRY", AddSoundFolder(pSellEntryWAV), 10, Brushes.Black,Brushes.Magenta);
						if(pSignalPlotOnEntryTriangles){
							if(State==State.Realtime){
								LastSignalPlotValue = SHORT;
								if(TimeOfLastSignalPlot == DateTime.MinValue) TimeOfLastSignalPlot = DateTime.Now;
							}
							Signal[0] = SHORT;
						}
//						Print(Times[0][0].ToString()+ "  DivSpotter SHORT at "+ShortEntryPrice);
						ShortEntryPrice = 0;
					}
				}
			}
			#region -- Continually hit the Signal plot with it's last value, for 30 seconds --
			if(TimeOfLastSignalPlot != DateTime.MinValue){
				var ts = new TimeSpan(DateTime.Now.Ticks - TimeOfLastSignalPlot.Ticks);
				if(ts.TotalSeconds < 30){
					Signal[0] = LastSignalPlotValue;
				}else{
					TimeOfLastSignalPlot = DateTime.MinValue;
				}
			}
			#endregion

		}

		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySignal{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellSignal{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyLevel{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellLevel{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal{
			get { return Values[4]; }
		}
		#endregion

		#region Properties
//==========================================================================================================
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
				list.Add("<inst>_SellSetup.wav");
				list.Add("<inst>_BuyEntry.wav");
				list.Add("<inst>_SellEntry.wav");
				list.Add("<inst>_BuyBreakout.wav");
				list.Add("<inst>_SellBreakout.wav");
				list.Add("<inst>_Divergence.wav");
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
			if(wav.Trim().Length==0) return string.Empty;
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(wav.Replace("<inst>",Instruments[0].MasterInstrument.Name)," "));
//			Print(Times[0][0].ToString()+"  DivergenceSpotter Playing sound: "+wav);
			return wav;
		}
		private string StripOutIllegalCharacters(string name, string ReplacementString){
			#region strip
			char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
			string invalids = string.Empty;
			foreach(char ch in invalidPathChars){
				invalids += ch.ToString();
			}
//			Print("Invalid chars: '"+invalids+"'");
			string result = string.Empty;
			for(int c=0; c<name.Length; c++) {
				if(!invalids.Contains(name[c].ToString())) result += name[c];
				else result += ReplacementString;
			}
			return result;
			#endregion
		}
//====================================================================

		private string pBuySetupWAV = "<inst>_BuySetup.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=10, Name="BUY setup", GroupName="Audible Alerts", Description="Sound file when divergence is found")]
        public string BuySetupWAV
        {
            get { return pBuySetupWAV; }
            set { pBuySetupWAV = value; }
        }
		private string pSellSetupWAV = "<inst>_SellSetup.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=20, Name="SELL setup", GroupName="Audible Alerts", Description="Sound file when divergence is found")]
        public string SellSetupWAV
        {
            get { return pSellSetupWAV; }
            set { pSellSetupWAV = value; }
        }
		private string pBuyEntryWAV = "<inst>_BuyEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=30, Name="BUY entry", GroupName="Audible Alerts", Description="Sound file when divergence is found")]
        public string BuyEntryWAV
        {
            get { return pBuyEntryWAV; }
            set { pBuyEntryWAV = value; }
        }
		private string pSellEntryWAV = "<inst>_SellEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=40, Name="SELL entry", GroupName="Audible Alerts", Description="Sound file when divergence is found")]
        public string SellEntryWAV
        {
            get { return pSellEntryWAV; }
            set { pSellEntryWAV = value; }
        }

		[Display(Order=10, Name="Indicator type", GroupName="Parameters", Description="Basis for the divergence signals")]
        public DivergenceSpotter_IndicatorType IndicatorType
        {
            get { return indicatorType; }
            set { indicatorType = value; }
        }

		[Display(Order=20, Name="Period Fast", GroupName="Parameters", Description="Used on all indicators, and is the %K value in Stochs")]
        public int PeriodFast
        {
            get { return fastPeriod; }
            set { fastPeriod = Math.Max(1, value); }
        }

		[Display(Order=30, Name="Period Slow", GroupName="Parameters", Description="Used on MACD and as the %D value in Stochs")]
        public int PeriodSlow
        {
            get { return slowPeriod; }
            set { slowPeriod = Math.Max(1, value); }
        }

		[Display(Order=40, Name="Period Signal", GroupName="Parameters", Description="Used on the MACD indicator and the StochSlowD")]
        public int PeriodSignal
        {
            get { return signalPeriod; }
            set { signalPeriod = Math.Max(1, value); }
        }

		[Display(Order=50, Name="Lookback Period", GroupName="Parameters", Description="Determines how far back to look for divergence, a '0' value means since the last crossing of the Zero line")]
        public int LookbackPeriod
        {
            get { return lookbackPeriod; }
            set { lookbackPeriod = Math.Max(0, value); }
        }
		[Display(Order=60, Name="Bar Sensitivity", GroupName="Parameters", Description="Low values look for tighter matchups, high values look for loose peaks matchup")]
        public int BarSensitivity
        {
            get { return barSensitivity; }
            set { barSensitivity = Math.Max(1, value); }
        }
		[Range(0,int.MaxValue)]
		[Display(Order=70, Name="EMA Filter Period Fast", GroupName="Parameters", Description="Set to '0' to turn off the filter")]
		public int pEMA1period {get;set;}

		[Range(0,int.MaxValue)]
		[Display(Order=80, Name="EMA Filter Period Slow", GroupName="Parameters", Description="Set to '0' to turn off the filter")]
		public int pEMA2period {get;set;}

		[Display(Order=10, Name="Signal on Potential Dots", GroupName="Signal Plot", Description="Signal is triggered on potential (setup) dots")]
		public bool pSignalPlotOnPotentialDots {get;set;}
		[Display(Order=20, Name="Signal on Entry Triangles", GroupName="Signal Plot", Description="Signal is triggered on potential (setup) dots")]
		public bool pSignalPlotOnEntryTriangles {get;set;}

		[XmlIgnore()]
		[Range(0,100)]
		[Description("Opacity of background colors ('0' to disengage background colorizing, 10 is max opacity)")]
		[Category("Visual background")]
		public int Opacity
		{
			get { return bkgopacity; }
			set { bkgopacity = Math.Min(10,Math.Max(0,value)); }
		}

		private bool pShowEntryLevels = true;
		[XmlIgnore()]
		[Description("Show the buy/sell 'triangles' giving you meaningful resistance or support levels for confirmation of a divergence breakout")]
		[Category("Parameters")]
		public bool ShowEntryLevels
		{
			get { return pShowEntryLevels; }
			set { pShowEntryLevels = value; }
		}
		#endregion

	}
}

    	public enum DivergenceSpotter_IndicatorType
        {   MACDLine=1,MACDHisto=2,MultiMACD=3,
			TRIX=4,
            CCI=5,
            BOP=6,
            CMO=7,
            ChaikinMoneyFlow=8,
            Momentum=9,
            MFI=10,
            RSI=11,
            ROC=12,
            StochSlowD=13,
            StochFastD=14,
            StochFastK=15,
            StochRSI=16,
			Fisher=17,
			RelVigorIndex=18
        }

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DivergenceSpotterV3[] cacheDivergenceSpotterV3;
		public DivergenceSpotterV3 DivergenceSpotterV3()
		{
			return DivergenceSpotterV3(Input);
		}

		public DivergenceSpotterV3 DivergenceSpotterV3(ISeries<double> input)
		{
			if (cacheDivergenceSpotterV3 != null)
				for (int idx = 0; idx < cacheDivergenceSpotterV3.Length; idx++)
					if (cacheDivergenceSpotterV3[idx] != null &&  cacheDivergenceSpotterV3[idx].EqualsInput(input))
						return cacheDivergenceSpotterV3[idx];
			return CacheIndicator<DivergenceSpotterV3>(new DivergenceSpotterV3(), input, ref cacheDivergenceSpotterV3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DivergenceSpotterV3 DivergenceSpotterV3()
		{
			return indicator.DivergenceSpotterV3(Input);
		}

		public Indicators.DivergenceSpotterV3 DivergenceSpotterV3(ISeries<double> input )
		{
			return indicator.DivergenceSpotterV3(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DivergenceSpotterV3 DivergenceSpotterV3()
		{
			return indicator.DivergenceSpotterV3(Input);
		}

		public Indicators.DivergenceSpotterV3 DivergenceSpotterV3(ISeries<double> input )
		{
			return indicator.DivergenceSpotterV3(input);
		}
	}
}

#endregion
