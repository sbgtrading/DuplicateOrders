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
	public class DifferenceTimeToClosePrice : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Difference Time-To-ClosePrice";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				pStartTime = 830;
				pCloseTime = 1555;
				pSpecificTime = -1;
				pRiskDollars = 150;
				pReduction = 80;
				AddPlot(Brushes.Cyan, "AvgDiff");
				AddPlot(Brushes.Red, "Upper");
				AddPlot(Brushes.Red, "Lower");
				AddPlot(Brushes.Red, "TgtUp");
				AddPlot(Brushes.Red, "TgtDown");
				AddLine(Brushes.SlateBlue,	0,	"Zero");
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				PV = Instrument.MasterInstrument.PointValue;
				AddDataSeries(BarsPeriodType.Minute,1);
			}
			else if (State == State.Realtime){
//				foreach(var kvp in MFE){
//					if(kvp.Value.Count>0){
//						var avg = kvp.Value.Average();
//						MFE[kvp.Key].Clear();
//						MFE[kvp.Key].Add(avg);//average MFE is stored in this dictionary
//					}
//				}
			}
		}

//		private class DayData {
//			public int PeakTOD   = 0;
//			public double AvgPts = 0;
//			public double MFE    = 0;
//			public double AvgFE  = 0;
//		}
//		SortedDictionary<DayOfWeek,DayData> Dailies = new SortedDictionary<DayOfWeek,DayData>();
		double PV = 0;
		int tint  = 0;
		int tint1 = 0;
		int MaxTime   = 0;
		int StartABar = -1;
		int CloseABar = 0;
		double BuyPrice  = 0;
		double SellPrice = 0;
		double UpperTarget = 0;
		double LowerTarget = 0;
		double MFEpts = 0;
		int AlertBar = 0;
		SortedDictionary<DayOfWeek, SortedDictionary<int,List<double>>> diffs = new SortedDictionary<DayOfWeek, SortedDictionary<int,List<double>>>();
		SortedDictionary<DayOfWeek, List<double>> MFE = new SortedDictionary<DayOfWeek, List<double>>();
		SortedDictionary<DayOfWeek, SortedDictionary<int,double>> sd = new SortedDictionary<DayOfWeek, SortedDictionary<int,double>>();
		SortedDictionary<DayOfWeek, SortedDictionary<int,double>> AvgByDOW = new SortedDictionary<DayOfWeek, SortedDictionary<int,double>>();
		SortedDictionary<DayOfWeek, Tuple<int,double>> PeakTOD = new SortedDictionary<DayOfWeek, Tuple<int,double>>();
		SortedDictionary<int,bool>  SessionBars = new SortedDictionary<int,bool>();
		int DayOfLastRectangle = 0;
		protected override void OnBarUpdate()
		{
			var DataID = 1;
			if(CurrentBars[0]<2) return;
			if(CurrentBars[DataID]<2) return;
			var t = Times[DataID][0];
			tint  = ToTime(t)/100;
			tint1 = ToTime(Times[DataID][1])/100;
			bool InSession = false;
			int StartTime  = pStartTime;
			if(pSpecificTime>-1)
				StartTime = pSpecificTime;
			if(StartTime == pCloseTime) InSession = true;
			else{
				if(StartTime < pCloseTime && tint >= StartTime && tint <= pCloseTime) InSession = true;
				else if(StartTime > pCloseTime && tint >= pCloseTime && tint <= StartTime) InSession = true;
			}
			if(IsFirstTickOfBar && InSession){
				SessionBars[CurrentBars[DataID]]=true;
			}

			if(!diffs.ContainsKey(t.DayOfWeek)){
				diffs[t.DayOfWeek]    = new SortedDictionary<int,List<double>>();
				AvgByDOW[t.DayOfWeek] = new SortedDictionary<int, double>();
				sd[t.DayOfWeek]       = new SortedDictionary<int, double>();
				MFE[t.DayOfWeek]      = new List<double>();
				PeakTOD[t.DayOfWeek]  = new Tuple<int,double>(0,-1);
			}
			bool c1 = tint1 < StartTime;
			bool c2 = tint >= StartTime;
			bool c3 = false;
			if(c1 && c2)
				StartABar = CurrentBars[DataID];
			c1 = tint1 < pCloseTime;
			c2 = tint >= pCloseTime;
			if(c1 && c2)
				CloseABar = CurrentBars[DataID];
			if(tint < tint1 && tint1 < StartTime && tint < StartTime) {StartABar = CurrentBars[DataID]; MFEpts=0;}
			int shift = 0;
			if(tint < tint1 && tint1 < pCloseTime && tint < pCloseTime) {CloseABar = CurrentBars[DataID]; tint=tint1; shift = 1;}//if the pCloseTime is after the market close, and the next bar is in the morning
//			var TradeTimeSpan = TimeSpan.Parse(FormatTime(PeakTOD[t.DayOfWeek].Item1));
//			var TradeTimeABar = BarsArray[DataID].GetBar(new DateTime(Times[DataID][1].Year, Times[DataID][1].Month, Times[DataID][1].Day, TradeTimeSpan.Hours, TradeTimeSpan.Minutes,0));
			if(BarsInProgress == DataID && State==State.Historical && (CloseABar==CurrentBars[DataID] || shift==1)){
				int a = shift;
Print(BarsInProgress+":  "+Times[DataID][0].ToString().Replace(":00 "," ")+"  PeakTime is: "+FormatTime(PeakTOD[t.DayOfWeek].Item1));
				MFEpts = 0;
				while(CurrentBars[DataID]-a >= Math.Max(1,StartABar-1) && Times[DataID].GetValueAt(CurrentBars[DataID]-a).Day == t.Day){
					int b = CurrentBars[DataID]-a;
					if(SessionBars.ContainsKey(b)){
						var tx = Times[DataID].GetValueAt(b);
						var minsdiff = Times[DataID][0] - tx;
						double tdiff = minsdiff.TotalMinutes;
						if(minsdiff.TotalMinutes<10) tdiff = 10;
						tint = ToTime(tx)/100;
						if(!diffs[tx.DayOfWeek].ContainsKey(tint)) diffs[tx.DayOfWeek][tint] = new List<double>();
						double d = Math.Abs(Closes[DataID].GetValueAt(b) - Closes[DataID][0]);
						diffs[tx.DayOfWeek][tint].Add(d);
//if(!SessionBars.ContainsKey(b-1))
	Print(tx.ToString().Replace(":00 "," ")+":  diff: "+(d/TickSize).ToString("0")+"  count of diffs: "+diffs[tx.DayOfWeek][tint].Count);
						if(BuyPrice != 0 && SellPrice !=0){
							MFEpts = Math.Max(Highs[DataID].GetValueAt(b) - BuyPrice, MFEpts);
//							if(SellPrice-Lows[DataID].GetValueAt(b) > MFEpts)
//								Print("SellPrice: "+SellPrice+"   Lows: "+Lows[DataID].GetValueAt(b));
							MFEpts = Math.Max(SellPrice - Lows[DataID].GetValueAt(b), MFEpts);
						}
					}
					a++;
				}
//Print(Times[0][1].ToString()+"  sell price: "+SellPrice+"   mfe: "+Math.Round(MFEpts/TickSize,0).ToString());
				if(MFEpts>0){
					if(!MFE.ContainsKey(t.DayOfWeek)){
						MFE[t.DayOfWeek]=new List<double>();
					}
					MFE[t.DayOfWeek].Add(MFEpts);
				}
			}
//var z = t.Day==8 && t.Month==5 && t.Hour==13;
			tint1 = ToTime(Times[DataID][1])/100;
			tint  = ToTime(Times[DataID][0])/100;
			if(diffs[t.DayOfWeek].ContainsKey(tint) && diffs[t.DayOfWeek][tint].Count>0){
				Values[0][0] = diffs[t.DayOfWeek][tint].Average();
				double variance = 0;
				foreach(var val in diffs[t.DayOfWeek][tint]){
					variance += Math.Pow(val - Values[0][0],2);
				}
				sd[t.DayOfWeek][tint] = Math.Sqrt(variance/diffs[t.DayOfWeek][tint].Count);
				Upper[0] = Values[0][0] + sd[t.DayOfWeek][tint];
				Lower[0] = Values[0][0] - sd[t.DayOfWeek][tint];

				AvgByDOW[t.DayOfWeek][tint] = (Values[0][0]);
				if(pSpecificTime>-1){
					c1 = tint1 <= pSpecificTime;
					c2 = tint >= pSpecificTime;
					c3 = false;
//if(z)Print(t.ToString()+"  c1: "+c1.ToString()+"  c2: "+c2.ToString()+"    tint1: "+tint1+"   0: "+tint);
					if(c1 && c2){
						PeakTOD[t.DayOfWeek] = new Tuple<int,double>(pSpecificTime, AvgByDOW[t.DayOfWeek][tint]);
//if(z)Print(t.ToString()+"   peak today found");
					}
				}else{
					c1 = false;
					c2 = false;
					if(CurrentBar == CloseABar){
						double maxV = -1;
						MaxTime = tint;
//Print(t.ToString());
						foreach(var kvp in AvgByDOW[t.DayOfWeek]){
							if(kvp.Value > maxV) {
//Print("New max at "+kvp.Key);
								maxV = kvp.Value;
								MaxTime = kvp.Key;
							}
						}
						PeakTOD[t.DayOfWeek] = new Tuple<int,double>(MaxTime, maxV);
					}
					c3 = tint1< PeakTOD[t.DayOfWeek].Item1 && tint >= PeakTOD[t.DayOfWeek].Item1;
				}
//if(z)Print(BarsInProgress+":  "+t.ToString()+"   peak today: "+PeakTOD[t.DayOfWeek].Item1+"    tint: "+tint+"   tint1: "+tint1);
				if(BarsInProgress==1 && DayOfLastRectangle != Times[DataID][0].Day && ((c1&&c2) || c3)) {
					BackBrushes[0] = Brushes.Yellow;
					if(State !=State.Historical && AlertBar!=CurrentBar){
						myAlert(string.Format("DTTC{0}",CurrentBar.ToString()), Priority.High, "DTTC OptimumTime", AddSoundFolder(pAlertWAV), 10, Brushes.Black,Brushes.Lime);
						AlertBar = CurrentBar;
					}
					DrawOnPricePanel = true;
					BuyPrice = Closes[DataID][0] + pRiskDollars/2/PV;
					SellPrice = Closes[DataID][0] - pRiskDollars/2/PV;
					Draw.Rectangle(this,"Rect"+CurrentBar.ToString(), false, Times[DataID][0], SellPrice, new DateTime(Times[DataID][0].Year, Times[DataID][0].Month, Times[DataID][0].Day,23,59,0), BuyPrice, Brushes.Transparent,Brushes.Green,20);
					DayOfLastRectangle = Times[DataID][0].Day;
					double reduction =  pReduction/100.0;
					UpperTarget = BuyPrice + Values[0][0] * reduction;
					LowerTarget = SellPrice - Values[0][0] * reduction;
					Draw.Square(this,string.Format("Avgtop {0} {1}%",CurrentBar,pReduction),false,0,UpperTarget,Brushes.Green);
					Draw.Square(this,string.Format("Avgbot {0} {1}%",CurrentBar,pReduction),false,0,LowerTarget,Brushes.Green);
					if(MFE.ContainsKey(t.DayOfWeek)){
						var mfedist = MFE[t.DayOfWeek].Count>0 ? MFE[t.DayOfWeek].Average() : 0;
						Draw.Dot(this,string.Format("AFEtop {0} {1}%",CurrentBar,pReduction),false,0,BuyPrice + mfedist * reduction, Brushes.Green);
						Draw.Dot(this,string.Format("AFEbot {0} {1}%",CurrentBar,pReduction),false,0,SellPrice - mfedist * reduction, Brushes.Green);
					}
					DrawOnPricePanel = false;
					Draw.Text(this, string.Format("diffttcp{0}",CurrentBar),false, t.DayOfWeek.ToString().Substring(0,3)+": "+tint.ToString(), 2, (Values[0][0]+Upper[0])/2, 0,Brushes.White, new SimpleFont("Arial",12),TextAlignment.Right,Brushes.Black,Brushes.Black,100);
					var s = string.Empty;
					foreach(var ss in PeakTOD){
						if(PeakTOD[ss.Key].Item2>0)
							s = string.Format("{0}\n{1}\t{2}-tks ({3}):\t@{4}\tavgFE: {5}",s,
								ss.Key.ToString().Substring(0,3), 
								(PeakTOD[ss.Key].Item2/TickSize).ToString("0"), 
								(PeakTOD[ss.Key].Item2*PV).ToString("C0"), 
								FormatTime(ss.Value.Item1), 
								MFE[ss.Key].Count>0 ? (MFE[ss.Key].Average()*PV).ToString("C0") : "N/A"
							);
					}
					RemoveDrawObject("sum_diffttcp");
					Draw.TextFixed(this, "sum_diffttcp", s,TextPosition.TopLeft,Brushes.DimGray,new SimpleFont("Arial",12),Brushes.Black,Brushes.Black,100);
				}
//				TgtUpper[0] = UpperTarget;
//				TgtLower[0] = LowerTarget;
			}
		}
		private string FormatTime(int t){
			double hr = Math.Truncate(t/100.0);
			double min = t - hr*100;
			if(min>9)
				return string.Format("{0}:{1}", hr,min);
			else
				return string.Format("{0}:0{1}", hr,min);
		}
		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			Alert(id,prio,msg,wav,rearmSeconds,bkgBrush, foregroundBrush);
			//printDebug(string.Format("Alert: {0}   wav: {1}",msg,wav));
		}
//==========================================================================================================
		#region Sound support methods
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
			//Print(Times[0][0].ToString()+"  Diff Time-to-ClosePrice Playing sound: "+wav);
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
		#endregion --------------------------
		#region Properties
		private string pAlertWAV = "Alert2.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=10, Name="Optimum Time Alert", GroupName="Audible Alerts", Description="")]
        public string AlertWAV
        {
            get { return pAlertWAV; }
            set { pAlertWAV = value; }
        }

		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="Start Time", Order=5, GroupName="Parameters")]
		public int pStartTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 2359)]
		[Display(Name="Close Time", Order=10, GroupName="Parameters")]
		public int pCloseTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-1, 2359)]
		[Display(Name="Specific Time", Order=20, GroupName="Parameters")]
		public int pSpecificTime
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Risk Rectangle", Order=30, Description="Dollars for the height of the Risk Rectangle", GroupName="Parameters")]
		public double pRiskDollars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Target reduction %", Order=40, Description="Targets are based on a percentage of the average distance", GroupName="Parameters")]
		public int pReduction
		{get;set;}
		#endregion

		#region plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgDiff
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TgtUpper
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TgtLower
		{
			get { return Values[4]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DifferenceTimeToClosePrice[] cacheDifferenceTimeToClosePrice;
		public DifferenceTimeToClosePrice DifferenceTimeToClosePrice(int pStartTime, int pCloseTime, int pSpecificTime, double pRiskDollars, int pReduction)
		{
			return DifferenceTimeToClosePrice(Input, pStartTime, pCloseTime, pSpecificTime, pRiskDollars, pReduction);
		}

		public DifferenceTimeToClosePrice DifferenceTimeToClosePrice(ISeries<double> input, int pStartTime, int pCloseTime, int pSpecificTime, double pRiskDollars, int pReduction)
		{
			if (cacheDifferenceTimeToClosePrice != null)
				for (int idx = 0; idx < cacheDifferenceTimeToClosePrice.Length; idx++)
					if (cacheDifferenceTimeToClosePrice[idx] != null && cacheDifferenceTimeToClosePrice[idx].pStartTime == pStartTime && cacheDifferenceTimeToClosePrice[idx].pCloseTime == pCloseTime && cacheDifferenceTimeToClosePrice[idx].pSpecificTime == pSpecificTime && cacheDifferenceTimeToClosePrice[idx].pRiskDollars == pRiskDollars && cacheDifferenceTimeToClosePrice[idx].pReduction == pReduction && cacheDifferenceTimeToClosePrice[idx].EqualsInput(input))
						return cacheDifferenceTimeToClosePrice[idx];
			return CacheIndicator<DifferenceTimeToClosePrice>(new DifferenceTimeToClosePrice(){ pStartTime = pStartTime, pCloseTime = pCloseTime, pSpecificTime = pSpecificTime, pRiskDollars = pRiskDollars, pReduction = pReduction }, input, ref cacheDifferenceTimeToClosePrice);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DifferenceTimeToClosePrice DifferenceTimeToClosePrice(int pStartTime, int pCloseTime, int pSpecificTime, double pRiskDollars, int pReduction)
		{
			return indicator.DifferenceTimeToClosePrice(Input, pStartTime, pCloseTime, pSpecificTime, pRiskDollars, pReduction);
		}

		public Indicators.DifferenceTimeToClosePrice DifferenceTimeToClosePrice(ISeries<double> input , int pStartTime, int pCloseTime, int pSpecificTime, double pRiskDollars, int pReduction)
		{
			return indicator.DifferenceTimeToClosePrice(input, pStartTime, pCloseTime, pSpecificTime, pRiskDollars, pReduction);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DifferenceTimeToClosePrice DifferenceTimeToClosePrice(int pStartTime, int pCloseTime, int pSpecificTime, double pRiskDollars, int pReduction)
		{
			return indicator.DifferenceTimeToClosePrice(Input, pStartTime, pCloseTime, pSpecificTime, pRiskDollars, pReduction);
		}

		public Indicators.DifferenceTimeToClosePrice DifferenceTimeToClosePrice(ISeries<double> input , int pStartTime, int pCloseTime, int pSpecificTime, double pRiskDollars, int pReduction)
		{
			return indicator.DifferenceTimeToClosePrice(input, pStartTime, pCloseTime, pSpecificTime, pRiskDollars, pReduction);
		}
	}
}

#endregion
