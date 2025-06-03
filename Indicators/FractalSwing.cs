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

using System.Net;
using System.IO;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum FractalSwing_CalcTypes {MADiff, FastMA, SlowMA}
	public class FractalSwing : Indicator
	{
		private const int LONG = 1;
		private const int SHORT = -1;

		private	SimpleFont		textFont;
		EMA ema1, ema2;
		int TrendDirection = 0;
		bool HigherLows = false;
		bool LowerHighs = false;
		List<double> HighSwings = new List<double>();
		List<double> LowSwings = new List<double>();
		int BarsAtStartup = 0;
		SortedDictionary<int,int> RangeDict = new SortedDictionary<int,int>();
		Brush hivolColor;
		List<double> ranges = new List<double>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= @"Fractal Swing";
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

				Strength					= 3;
				ArrowSize					= 15;
				TickSeparation				= 3;
				pHiVolatilityPctile = 10;
				pHiVolatilityOpacity = 10;
				UpColor						= Brushes.Green;
				DownColor					= Brushes.Red;
				pFastEMAperiod = 50;
				pSlowEMAperiod = 100;
				pHiVolatilityWAV = "none";
				pBOBoxSizeATRMults = 2;
				pShowVerticalLineAtHiVol = false;
				pCalcBasis = FractalSwing_CalcTypes.MADiff;

				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.TriangleRight, "BuyLvl");
				AddPlot(new Stroke(Brushes.Magenta, 3), PlotStyle.TriangleRight, "SellLvl");
				AddPlot(new Stroke(Brushes.White, 1), PlotStyle.Line, "FastEMA");
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "SlowEMA");
			}
			else if (State == State.Configure)
			{
				IsAutoScale = false;
				textFont  = new SimpleFont("Wingdings 3",ArrowSize);
				hivolColor = Brushes.Yellow.Clone();
				hivolColor.Opacity = pHiVolatilityOpacity/100.0;
				hivolColor.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				if(pFastEMAperiod>0)
					ema1 = EMA(pFastEMAperiod);
				if(pSlowEMAperiod>0)
					ema2 = EMA(pSlowEMAperiod);
				BarsAtStartup = BarsArray[0].Count+1;
			}
		}

		private double B = -1;
		bool WasHiVol = false;
		double AvgRange = 0;
		double threshold = 0;
		int tks = 0;
		protected override void OnBarUpdate()
		{
			if(CurrentBar < Strength*3+2) return;

			B = isHighPivot(Strength);
			if(B != -1) HighSwings.Insert(0, B);
			if(HighSwings.Count>=2){
				while(HighSwings.Count>2) HighSwings.RemoveAt(HighSwings.Count-1);
				LowerHighs = HighSwings[0] < HighSwings[1];
			}


			B = isLowPivot(Strength);
			if(B != -1) LowSwings.Insert(0, B);
			if(LowSwings.Count>=2){
				while(LowSwings.Count>2) LowSwings.RemoveAt(LowSwings.Count-1);
				HigherLows = LowSwings[0] > LowSwings[1];
			}

			if(IsFirstTickOfBar && State==State.Historical){
				ranges.Add(Highs[0][0]-Lows[0][0]);
				while(ranges.Count>14) ranges.RemoveAt(0);
				AvgRange = pBOBoxSizeATRMults * ranges.Average()/2;
			}
			if(pFastEMAperiod>0)
				FastEMA[0] = ema1[0];
			if(pSlowEMAperiod>0)
				SlowEMA[0] = ema2[0];
			if(ema1!=null && ema2!=null){
				#region -- Calculate hi-volatility regions --
				if(IsFirstTickOfBar){
					if(pCalcBasis == FractalSwing_CalcTypes.MADiff)
						tks = Convert.ToInt32(Math.Round(Math.Abs(ema1[1]-ema2[1])/TickSize,0));
					else if(pCalcBasis == FractalSwing_CalcTypes.FastMA)
						tks = Convert.ToInt32(Math.Round(Math.Abs(ema1[1]-Median[1])/TickSize,0));
					else if(pCalcBasis == FractalSwing_CalcTypes.SlowMA)
						tks = Convert.ToInt32(Math.Round(Math.Abs(Median[1]-ema2[1])/TickSize,0));
					if(State==State.Historical){
						if(!RangeDict.ContainsKey(tks)) RangeDict[tks] = 1;
						else RangeDict[tks] = RangeDict[tks]+1;
						threshold = RangeDict.Values.Sum() * (1-pHiVolatilityPctile/100.0);//so a 10 pctile is the top 10% of all tick diffs
						int sum = 0;
						foreach(var kvp in RangeDict) {
							var s = sum + kvp.Value;
							if(s > threshold) { /*Print("Found threshold! "+kvp.Key);*/threshold = kvp.Key; break;}
							//else Print(kvp.Key+": " + kvp.Value+"  sum: "+s);
							sum  = s;
						}
					}
					if(tks >= threshold){
						BackBrushes[1] = hivolColor;
						if(!WasHiVol) {
							myAlert(CurrentBars[0].ToString(),Priority.High,"Hi volatility", pHiVolatilityWAV, 1, Brushes.Yellow, Brushes.Black);
							Draw.Rectangle(this,string.Format("BObox{0}",CurrentBars[0]), Times[0][1], Closes[0][0]+AvgRange, Times[0][6], Closes[0][0]-AvgRange, Brushes.Transparent);
							Draw.TextFixed(this,"BoxSize", string.Format("BO Box size {0}\n{1}",(AvgRange*2*Instrument.MasterInstrument.PointValue).ToString("C"), Times[0][0].ToString()), TextPosition.BottomLeft); 
						}
						WasHiVol = true;
					}else WasHiVol = false;
//					if(CurrentBars[0]>BarsArray[0].Count-3)
//						foreach(var kvp in RangeDict) {Print(kvp.Key+"-tks: "+kvp.Value);}
				}
				#endregion

				var td = TrendDirection;
				if(ema1[1] > ema2[1]){
					//if(td != LONG) LowSwings.Clear();
					TrendDirection = LONG;
				}
				else if(ema1[1] < ema2[1]){
					//if(td != SHORT) HighSwings.Clear();
					TrendDirection = SHORT;
				}
			}
			if(LowerHighs && TrendDirection >= 0 && HighSwings.Count>1){
				BuyLvl[0] = HighSwings[0];
				if(!BuyLvl.IsValidDataPoint(1))
					myAlert(string.Format("FractalSwing Buy{0}",CurrentBar.ToString()), Priority.High, "FractalSwing Buy setup", AddSoundFolder(pBuySetupWAV), 10, Brushes.Black,Brushes.Lime);
			}
			if(BuyLvl.IsValidDataPoint(0) && High[0] > BuyLvl[0]) {
				BuyLvl.Reset(0);
				HighSwings.Clear();
				myAlert(string.Format("FractalSwing Buy{0}",CurrentBar.ToString()), Priority.High, "FractalSwing BUY", AddSoundFolder(pBuyEntryWAV), 10, Brushes.Black,Brushes.Lime);
			}
			if(HigherLows && TrendDirection <= 0 && LowSwings.Count>1){
				SellLvl[0] = LowSwings[0];
				if(!SellLvl.IsValidDataPoint(1))
					myAlert(string.Format("FractalSwing Sell{0}",CurrentBar.ToString()), Priority.High, "FractalSwing Sell setup", AddSoundFolder(pSellSetupWAV), 10, Brushes.Black,Brushes.Magenta);
			}
			if(SellLvl.IsValidDataPoint(0) && Low[0] < SellLvl[0]){
				SellLvl.Reset(0);
				LowSwings.Clear();
				myAlert(string.Format("FractalSwing Sell{0}",CurrentBar.ToString()), Priority.High, "FractalSwing SELL", AddSoundFolder(pSellEntryWAV), 10, Brushes.Black,Brushes.Magenta);
			}
        }

		private double isHighPivot(int period)
		{
			#region HighPivot
			int y = 0;
			int Lvls = 0;
			
			//Four Matching Highs
			if(High[period]==High[period+1] && High[period]==High[period+2] && High[period]==High[period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? High[period+3]>High[period+3+y] : High[period+3]>High[period+3+y])
						Lvls++;
					if(y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Highs
			else if(High[period]==High[period+1] && High[period]==High[period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? High[period+2]>High[period+2+y] : High[period+2]>High[period+2+y])
						Lvls++;
					if(y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Highs
			else if(High[period]==High[period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? High[period+1]>High[period+1+y] : High[period+1]>High[period+1+y])
						Lvls++;
					if(y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			//Regular Pivot
			else
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? High[period]>High[period+y] : High[period]>High[period+y])
						Lvls++;
					if(y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Highs - First and Last Matching - Middle 2 are lower
				if(High[period]>=High[period+1] && High[period]>=High[period+2] && High[period]==High[period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? High[period+3]>High[period+3+y] : High[period+3]>High[period+3+y])
							Lvls++;
						if(y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Highs - Middle is lower than two outside
				if(High[period]>=High[period+1] && High[period]==High[period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? High[period+2]>High[period+2+y] : High[period+2]>High[period+2+y])
						Lvls++;
					if(y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				//Draw.Dot(this,string.Format("SHigh{0}",CurrentBar),false,period,High[period]+TickSize*TickSeparation,UpColor);
				return(High[period]);
			}
			return(-1.0);
			#endregion
		}

		private double isLowPivot(int period)
		{
			#region LowPivot
			int y = 0;
			int Lvls = 0;
			
			//Four Matching Lows
			if(Low[period]==Low[period+1] && Low[period]==Low[period+2] && Low[period]==Low[period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Low[period+3]<Low[period+3+y] : Low[period+3]<Low[period+3+y])
						Lvls++;
					if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Lows
			else if(Low[period]==Low[period+1] && Low[period]==Low[period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Low[period+2]<Low[period+2+y] : Low[period+2]<Low[period+2+y])
						Lvls++;
					if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Lows
			else if(Low[period]==Low[period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Low[period+1]<Low[period+1+y] : Low[period+1]<Low[period+1+y])
						Lvls++;
					if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			//Regular Pivot
			else
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Low[period]<Low[period+y] : Low[period]<Low[period+y])
						Lvls++;
					if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Lows - First and Last Matching - Middle 2 are lower
				if(Low[period]<=Low[period+1] && Low[period]<=Low[period+2] && Low[period]==Low[period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Low[period+3]<Low[period+3+y] : Low[period+3]<Low[period+3+y])
							Lvls++;
						if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Lows - Middle is lower than two outside
				if(Low[period]<=Low[period+1] && Low[period]==Low[period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Low[period+2]<Low[period+2+y] : Low[period+2]<Low[period+2+y])
						Lvls++;
					if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				//Draw.Dot(this,string.Format("SLow{0}",CurrentBar),false,period, Low[period]-TickSize*TickSeparation, DownColor);
				return(Low[period]);
			}
			return(-1.0);
			#endregion
		}

//==========================================================================================================
		#region -- Sound support methods --
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
//			Print(Times[0][0].ToString()+"  FractalSwing Playing sound: "+wav);
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

		private int SoundABar = 0;
		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			if(pShowVerticalLineAtHiVol && msg == "Hi volatility") Draw.VerticalLine(this,CurrentBar.ToString(),2,Brushes.Yellow);
			if(CurrentBar > BarsAtStartup && CurrentBar > SoundABar){
				Alert(id,prio,msg,wav,rearmSeconds,bkgBrush, foregroundBrush);
				SoundABar = CurrentBar;
//				Print("FractalSwing playing sound: "+wav);
			}
			//printDebug(string.Format("Alert: {0}   wav: {1}",msg,wav));
		}
//====================================================================
		#endregion

		#region Properties
		[Range(1, int.MaxValue)]
		[Display(Name="Strength", Order=10, GroupName="Parameters")]
		public int Strength
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="ArrowSize", Order=20, GroupName="Parameters")]
		public int ArrowSize
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="Tick Separation", Order=30, GroupName="Parameters")]
		public int TickSeparation
		{ get; set; }

		[Range(0.1, double.MaxValue)]
		[Display(Name="Percentile for Hi Volatility", Order=40, GroupName="Parameters", Description="Top percentile of all MA differences.  So '10' means you'll be finding when the MA diffs exceed the top 10% of all MA diffs")]
		public double pHiVolatilityPctile
		{ get; set; }
		
		[Display(Name="Hi Volatility CalcBasis", Order=41, GroupName="Parameters")]
		public FractalSwing_CalcTypes pCalcBasis
		{get;set;}

		[Range(0, 100)]
		[Display(Name="Hi Volatility Opacity", Order=42, GroupName="Parameters")]
		public int pHiVolatilityOpacity
		{get;set;}

		[Display(Name="Show vline at Hi Volatility", Order=43, GroupName="Parameters")]
		public bool pShowVerticalLineAtHiVol
		{get;set;}

		[Range(0, 100)]
		[Display(Name="BO Box ATR mults", Order=44, GroupName="Parameters")]
		public double pBOBoxSizeATRMults
		{get;set;}

		[XmlIgnore]
		[Display(Name="Up", Order=50, GroupName="Parameters")]
		public Brush UpColor
		{ get; set; }

		[Browsable(false)]
		public string UpColorSerializable
					{get { return Serialize.BrushToString(UpColor); }set { UpColor = Serialize.StringToBrush(value); }}			

		[XmlIgnore]
		[Display(Name="Down", Order=60, GroupName="Parameters")]
		public Brush DownColor
		{ get; set; }

		[Browsable(false)]
		public string DownColorSerializable
					{get { return Serialize.BrushToString(DownColor); } set { DownColor = Serialize.StringToBrush(value); }		}		

		[Range(0, int.MaxValue)]
		[Display(Name="Fast EMA period", Order=70, GroupName="Parameters")]
		public int pFastEMAperiod
		{ get; set; }

		[Range(0, int.MaxValue)]
		[Display(Name="Slow EMA period", Order=80, GroupName="Parameters")]
		public int pSlowEMAperiod
		{ get; set; }

		#region -- Audible Alerts --
		private string pHiVolatilityWAV = "silent";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=5, Name="Entering Hi Volatility", GroupName="Audible Alerts", Description="Sound file when the MA bands show high volatility")]
        public string HiVolatilityWAV
        {
            get { return pHiVolatilityWAV; }
            set { pHiVolatilityWAV = value; }
        }
		private string pBuySetupWAV = "<inst>_BuySetup.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=10, Name="BUY setup", GroupName="Audible Alerts", Description="Sound file when buy setup is found")]
        public string BuySetupWAV
        {
            get { return pBuySetupWAV; }
            set { pBuySetupWAV = value; }
        }
		private string pSellSetupWAV = "<inst>_SellSetup.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=20, Name="SELL setup", GroupName="Audible Alerts", Description="Sound file when sell setup is found")]
        public string SellSetupWAV
        {
            get { return pSellSetupWAV; }
            set { pSellSetupWAV = value; }
        }
		private string pBuyEntryWAV = "<inst>_BuyEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=30, Name="BUY entry", GroupName="Audible Alerts", Description="Sound file when a buy entry level is hit")]
        public string BuyEntryWAV
        {
            get { return pBuyEntryWAV; }
            set { pBuyEntryWAV = value; }
        }
		private string pSellEntryWAV = "<inst>_SellEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=40, Name="SELL entry", GroupName="Audible Alerts", Description="Sound file when a sell entry level is hit")]
        public string SellEntryWAV
        {
            get { return pSellEntryWAV; }
            set { pSellEntryWAV = value; }
        }
		#endregion
		#endregion
		#region -- plots --

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyLvl
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellLvl
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FastEMA
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SlowEMA
		{
			get { return Values[3]; }
		}
		#endregion
	}	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FractalSwing[] cacheFractalSwing;
		public FractalSwing FractalSwing()
		{
			return FractalSwing(Input);
		}

		public FractalSwing FractalSwing(ISeries<double> input)
		{
			if (cacheFractalSwing != null)
				for (int idx = 0; idx < cacheFractalSwing.Length; idx++)
					if (cacheFractalSwing[idx] != null &&  cacheFractalSwing[idx].EqualsInput(input))
						return cacheFractalSwing[idx];
			return CacheIndicator<FractalSwing>(new FractalSwing(), input, ref cacheFractalSwing);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FractalSwing FractalSwing()
		{
			return indicator.FractalSwing(Input);
		}

		public Indicators.FractalSwing FractalSwing(ISeries<double> input )
		{
			return indicator.FractalSwing(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FractalSwing FractalSwing()
		{
			return indicator.FractalSwing(Input);
		}

		public Indicators.FractalSwing FractalSwing(ISeries<double> input )
		{
			return indicator.FractalSwing(input);
		}
	}
}

#endregion
