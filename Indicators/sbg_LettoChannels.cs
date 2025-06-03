
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum LettoChannels_PredictionMarkers {dots, histo, none};

	[CategoryOrder("License", 10)]
	[CategoryOrder("Parameters", 20)]
	[CategoryOrder("Custom Visuals", 25)]
	[CategoryOrder("Alerts", 30)]
	public class LettoChannels : Indicator
	{
		private const int UPPER_BAND_HIT = 1;
		private const int LOWER_BAND_HIT = -1;
		private double constant1;
		private double constant2;
		private string SupportEmail = "support@sbgtradingcorp.com";
//E3BBBFBFC393403BFB80975D2D701C8D is Charles Sasser
//6B23684B285F341B1442066ABAC80DAA is Joseph Toussaint, valid thru 2/28/2025
		bool IsDebug = false;
		bool ValidLicense = false;
		bool LicServerPinged = false;
		DateTime LaunchedAt = DateTime.Now;
		private SortedDictionary<int,double[]> cci = new SortedDictionary<int,double[]>();
		Brush BandHit_BkgStripe_Green;
		Brush BandHit_BkgStripe_Red;
		List<DayOfWeek> ValidDaysOfWeek = new List<DayOfWeek>{DayOfWeek.Sunday,DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday};
		bool IsValidDaysAllOrToday = false;

		private string KeepAlphabetic(string input)
		{
		    return System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z]", "");
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name				 = "sbg LettoChannels v1.4";
				//v1.3 - changed the logic of the background stripe printing, to reduce the number of times it printed
				//v1.4 - added custom visuals category, and the optional OB/OS bands
				IsOverlay			 = true;
				pMAperiod			 = 100;
				pEquidistantChannel = false;
				pMinutes = 30;
				pDaysOfWeek = "All";
				pChannelReductionPct = 0.1;
				pOuterBandsMult		 = 0.2;
				pExpectationMarkers = LettoChannels_PredictionMarkers.dots;
				pExpectationPeriod = 7;
				pCycleDotPeriod = 7;
				pCycleDotSeparation = 1;
				pPositiveExpectationColor = Brushes.Lime;
				pPositiveExpectationOpacity = 0.5;
				pNegativeExpectationColor = Brushes.Magenta;
				pNegativeExpectationOpacity = 0.5;
				pFuturePosExpectationOpacity = 0.6;
				pFutureNegExpectationOpacity = 0.6;
				pGlobalizeDiamonds = true;
				pGlobalizeRectangles = false;
				pMaxMarkersCount = 10;
				pUsePriceDiff = true;
				pEnableCycleAlerts = false;
				pCycleSell1 = "none";
				pCycleSell2 = "none";
				pCycleBuy1 = "none";
				pCycleBuy2 = "none";
				pShowOBOSbands = true;
				pOpacityYellowZone = 0.7f;

				pEnableBandAlerts = true;
				pUpperBandHit = "none";
				pLowerBandHit = "none";
				pDaysBackForRectangles = 5;
				pRectangleStartDate = DateTime.Now.AddDays(-1);
				pRectangleTemplateName = "Default";
				pDiamondTemplateName = "Default";

				AddPlot(new Stroke(Brushes.Yellow,2),PlotStyle.Line,"Mid");
				AddPlot(new Stroke(Brushes.Red,2),   PlotStyle.Dot, "PercentileUpper");
				AddPlot(new Stroke(Brushes.Green,2), PlotStyle.Dot, "PercentileLower");
				AddPlot(Brushes.Maroon, "SLU");
				AddPlot(Brushes.Maroon, "SLD");
				AddPlot(new Stroke(Brushes.Green,2), PlotStyle.Dot, "CycleStrongBuy");
				AddPlot(new Stroke(Brushes.Lime,2), PlotStyle.Dot, "CycleBuy");
				AddPlot(new Stroke(Brushes.Maroon,2), PlotStyle.Dot, "CycleStrongSell");
				AddPlot(new Stroke(Brushes.Magenta,2), PlotStyle.Dot, "CycleSell");
				AddPlot(new Stroke(Brushes.Cyan,1), PlotStyle.Dot, "Max");
				AddPlot(new Stroke(Brushes.Cyan,1), PlotStyle.Dot, "Min");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "MktAnalyzer");
				AddPlot(new Stroke(Brushes.Transparent,5), PlotStyle.Dot, "StratEntryPrice");
				AddPlot(new Stroke(Brushes.Transparent,5), PlotStyle.Dot, "StratSLPrice");
				AddPlot(new Stroke(Brushes.Transparent,5), PlotStyle.Dot, "StratT1Price");
				AddPlot(new Stroke(Brushes.Transparent,5), PlotStyle.Dot, "StratT2Price");

				var machid = NinjaTrader.Cbi.License.MachineId;
				IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (machid.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || machid.CompareTo("xxx")==0);
				var ValidMachIDs = new List<string>(){"B0D2E9D1C802E279D3678D7DE6A33CE4","CB15E08BE30BC80628CFF6010471FA2A"};
				if(machid.CompareTo("6B23684B285F341B1442066ABAC80DAA")==0 && DateTime.Now < new DateTime(2025,2,28) || //Joseph Toussaint genitous@gmail.com  free trial
				  (machid.CompareTo("38E81B0FC6ED3C2536E7C164321724D9")==0)){//Matt Narramore (box977@narramore.us) - $379 paid forlifetime license with 100% moneyback after 1yr
					ValidLicense = true;
				  }else if(ValidMachIDs.Contains(machid))
					ValidLicense = true;
				else {
					if(pLicenseID==null) pLicenseID = string.Empty;
					if(pLicenseID.Trim().Length==0 && !IsDebug){
						VendorLicense("DayTradersAction", "SbgLettoChannels", "www.sbgtradingcorp.com", "support@sbgtradingcorp.com");
						LicServerPinged = true;
						//VendorLicense("IndicatorWarehouse", "SbgLettoChannels", "www.IndicatorWarehouse.com", "License@LettoChannels.com");
					}
				}
			}
			else if (State == State.Configure)
			{
				if(!LicServerPinged && pLicenseID!=null){
					var yrstr = "";
					var monthstr = "";
					var daystr = "";
					if(pLicenseID.Trim().Length>0){
						ValidLicense = false;
						var s = KeepAlphabetic(pLicenseID);
						if(s.Length>0){
							var array = s.ToCharArray();
							foreach(var c in array){
								int v = 0;
								if(c >= 'f' && c <= 'z') v = 0;//all lowercase letters greater than e, are zero in value
								if(c >= 'a' && c <= 'e') v = (int)c-96+26;//lowercase letters a,b,c,d,e are for numbers 27, 28, 29, 30 and 31
								if(c >= 'A' && c <= 'Z') v = (int)c-64;//uppercase letters are for numbers 1,2,3,4,5,6,7,8,9,10...26
								if(monthstr.Length==0) monthstr = v.ToString();
								else if(yrstr.Length<4) yrstr += v.ToString();
								else if(daystr.Length<2) daystr += v.ToString();
							}
						}
						//month number is first, year numbers are next, and day number is last
						//April = D,   July is G,   Sept is I   Dec is L
						//2025 = TY or TBE or BmBE or BgY   2026 is TZ or TBF or BrZ
						//27th day is   a or BG
						//4*D6TzzB-Eb
					
					
						var DaysToExpiry = int.MinValue;
						TimeSpan tss;
						try{
							var dt = new DateTime(Int32.Parse(yrstr), Int32.Parse(monthstr), Int32.Parse(daystr),0,0,0);
							Print("LettoChannels good thru: "+dt.ToString());
							tss = new TimeSpan(dt.Ticks - DateTime.Now.Ticks);
							DaysToExpiry = tss.Days + 1;
							if(DaysToExpiry > 0){
								ValidLicense = true;
								if(DaysToExpiry == 1)
									Draw.TextFixed(this, "lic", $"License valid for 1-day...contact {SupportEmail}", TextPosition.BottomLeft, Brushes.White,new NinjaTrader.Gui.Tools.SimpleFont("Arial",16),Brushes.Black,Brushes.DarkGreen,100);
								else if(DaysToExpiry < 5)
									Draw.TextFixed(this, "lic", $"License valid for {DaysToExpiry}-days...contact {SupportEmail}", TextPosition.BottomLeft, Brushes.White,new NinjaTrader.Gui.Tools.SimpleFont("Arial",16),Brushes.Black,Brushes.DarkGreen,100);
							}
						}catch(Exception ex){
							Draw.TextFixed(this, "licErr", $"License key is not valid - contact {SupportEmail}", TextPosition.Center, Brushes.Red,new NinjaTrader.Gui.Tools.SimpleFont("Arial",16),Brushes.Black,Brushes.Black,100);
							ValidLicense = false;
						}
					}
					if(!ValidLicense){
						Draw.TextFixed(this, "licErr", $"License expired - contact {SupportEmail}", TextPosition.Center, Brushes.Red,new NinjaTrader.Gui.Tools.SimpleFont("Arial",16),Brushes.Black,Brushes.Black,100);
						IsVisible = false;
					}else{
						IsVisible = true;
					}
				}
				constant1 = 2.0 / (1 + pMAperiod);
				constant2 = 1 - (2.0 / (1 + pMAperiod));
				#region -- ChannelRanges init --
				int i = 0;
				int prior=-1;
				TimeSpan ts = new TimeSpan(0,0,0);
				while(i<2400){
					ChannelRanges[i]   = new List<double>();
					ts = ts.Add(new TimeSpan(0,pMinutes,0));
					i = ts.Hours*100 + ts.Minutes;
					if(i < prior) break;
					prior = i;
				}
				#endregion
				var str = pDaysOfWeek.ToUpper();
				if(str.Contains("TODAY")){
					IsValidDaysAllOrToday = true;
					ValidDaysOfWeek.Clear();
					ValidDaysOfWeek.Add(DateTime.Now.DayOfWeek);
				}else if(!str.Contains("ALL")){
					ValidDaysOfWeek.Clear();
					if(str.Contains("0") || str.Contains("SU")) ValidDaysOfWeek.Add(DayOfWeek.Sunday);
					if(str.Contains("1") || str.Contains("M")) ValidDaysOfWeek.Add(DayOfWeek.Monday);
					if(str.Contains("2") || str.Contains("TU")) ValidDaysOfWeek.Add(DayOfWeek.Tuesday);
					if(str.Contains("3") || str.Contains("W")) ValidDaysOfWeek.Add(DayOfWeek.Wednesday);
					if(str.Contains("4") || str.Contains("TH")) ValidDaysOfWeek.Add(DayOfWeek.Thursday);
					if(str.Contains("5") || str.Contains("F")) ValidDaysOfWeek.Add(DayOfWeek.Friday);
					if(str.Contains("6") || str.Contains("SA")) ValidDaysOfWeek.Add(DayOfWeek.Saturday);
				}
			}
			else if (State == State.DataLoaded)
			{
				if(!pUsePriceDiff) ScaleMult = 5; ScaleMult = 1;

				BandHit_BkgStripe_Green = Brushes.Green.Clone();
				BandHit_BkgStripe_Green.Opacity = pBandHit_BkgStripeOpacity;
				BandHit_BkgStripe_Green.Freeze();
				BandHit_BkgStripe_Red = Brushes.Red.Clone();
				BandHit_BkgStripe_Red.Opacity = pBandHit_BkgStripeOpacity;
				BandHit_BkgStripe_Red.Freeze();
				
				RectStartDate = pDaysBackForRectangles>0 ? DateTime.Now.AddDays(-pDaysBackForRectangles):pRectangleStartDate;

//				if(pCycleDotPeriod>0){
//					for(int i = pCycleDotPeriod*2+3; i< Close.Count-2; i++){
//						try{
//							CalculateTrendAge(i, Input, cci);
//						}catch(Exception e){Print(i+"  :  "+e.ToString());}
//					}
//				}
			}
		}
		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TheMA
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PercentileUpper
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PercentileLower
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SLU
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SLL
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CycleStrongBuy
		{
			get { return Values[5]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CycleBuy
		{
			get { return Values[6]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CycleStrongSell
		{
			get { return Values[7]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CycleSell
		{
			get { return Values[8]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MaxP
		{
			get { return Values[9]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MinP
		{
			get { return Values[10]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MktAnalyzer
		{
			get { return Values[11]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StratEntryPrice
		{
			get { return Values[12]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StratSLPrice
		{
			get { return Values[13]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StratT1Price
		{
			get { return Values[14]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StratT2Price
		{
			get { return Values[15]; }
		}
		#endregion ------

		#region -- TrendAge and Cycle Alerts --
		private int LastSignalType = 0;
		private int SignalABar = 0;
		private void CalculateTrendAge(int cb, ISeries<double> Price, SortedDictionary<int,double[]> cci){
			#region -- CalculateTrendAge --
			int period = pCycleDotPeriod * 2;
			double sma14 = 0;
			for(int j = cb-period; j<cb; j++)
				sma14 = sma14+Price.GetValueAt(j);
			sma14 = sma14/(period);
			double mean = 0;
			for (int j = cb-period; j<cb; j++)
				mean += Math.Abs(Price.GetValueAt(j) - sma14);
			cci[cb] = new double[3];
		    cci[cb][0] = (Price.GetValueAt(cb) - sma14) / (mean.ApproxCompare(0) == 0 ? 1 : (0.015 * (mean / pCycleDotPeriod/2) ) );
			mean = 0;
			for (int j = cb-(period+1); j<cb; j++){
				if(cci.ContainsKey(j)) mean += cci[j][0];
			}
			int StrongLvl = 160;
			int WeakLvl = 110;
			if(SignalABar == 0 && State == State.Realtime) SignalABar = cb-4;//first bar after a chart refresh is ignored...signals can only come on subsequent bars
			cci[cb][1] = mean/(period+1);//the [1] element of the double[] is the SMA of the [0] element
//Print(cci[cb][0].ToString("0.00")+"  "+cci[cb][1].ToString("0.00"));
			if(cci[cb][1] < -StrongLvl) {
				cci[cb][2] = 2;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType>0) LastSignalType = 0;//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new long signals to generate alerts
				}
			}
			else if(cci[cb][1] < -WeakLvl) {
				cci[cb][2] = 1;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType>0) LastSignalType = 0;//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new long signals to generate alerts
				}
			}
			else if(cci[cb][1] > StrongLvl) {
				cci[cb][2] = -2;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType<0) LastSignalType = 0;//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new short signals to generate alerts
				}
			}
			else if(cci[cb][1] > WeakLvl) {
				cci[cb][2] = -1;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType<0) LastSignalType = 0;//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new short signals to generate alerts
				}
			}
			else cci[cb][2] = 0;
			#endregion
		}
		private void CycleAlertLauncher(int sig, int cb){
			if((sig == 2 || sig == 1) && SignalABar != CurrentBar) {
				SignalABar = cb;
				if(sig == 1 && LastSignalType != 1) myAlert($"wBuy{cb}", Priority.High, "LettoChannels Weak Cycle Buy", pCycleBuy1, 0, Brushes.Black,Brushes.Lime);
				if(sig == 2 && LastSignalType != 2) myAlert($"sBuy{cb}", Priority.High, "LettoChannels Strong Cycle Buy", pCycleBuy2, 0, Brushes.Black,Brushes.Lime);
				LastSignalType = sig;
//Print(Times[0][0].ToString()+"  sig: "+sig);
			}
			else if((sig == -2 || sig == -1) && SignalABar != CurrentBar) {
				SignalABar = cb;
				if(sig == -1 && LastSignalType != -1) myAlert($"wSell{cb}", Priority.High, "LettoChannels Weak Cycle Sell", pCycleSell1, 0, Brushes.Black,Brushes.Magenta);
				if(sig == -2 && LastSignalType != -2) myAlert($"sSell{cb}", Priority.High, "LettoChannels Strong Cycle Sell", pCycleSell2, 0, Brushes.Black,Brushes.Magenta);
				LastSignalType = sig;
//Print(Times[0][0].ToString()+"  sig: "+sig);
			}
		}
		#endregion

		#region -- Supporting Methods --
		private void AddToChannelRanges(int t, double val){
			if(pEquidistantChannel)
				ChannelRanges[t].Add(Math.Abs(val));
			else
				ChannelRanges[t].Add(val);
		}
		private int GetChannelRangesId(DateTime t){
			int m = Convert.ToInt32(Math.Floor(t.Minute*1.0 / pMinutes)) * pMinutes;
			int x = t.Hour*100 +m;
			return x;
		}
		private void CalcDistPctileOnAllTimeperiods(double divide, DateTime dt1){
			double temp1 = 0;
			double temp2 = 0;
			double hi_p = 0;
			double low_p = 0;
			int k = 0;
//Print("CalcDistPctileOnAllTimeperiods is running");
			foreach(int t in ChannelRanges.Keys){
				temp1 = 0;
				if(ChannelRanges[t].Count>1){
					var r = ChannelRanges[t].Where(x=> x > 0).ToList();
					if(r.Count>1){
						r.Sort();
						if(r.First() < r.Last()) r.Reverse();
						k = Convert.ToInt32(Math.Truncate(r.Count*pChannelReductionPct));
						temp1 = r[k];
					}else if(r.Count==1){
						temp1 = r[0];
					}else{//r.Count is zero
						r.Add(0);
					}
					hi_p = r.Max();
					low_p = -hi_p;
					temp2 = -temp1;
					if(!pEquidistantChannel){
						r = ChannelRanges[t].Where(x=> x < 0).ToList();//assemble a list of negative ranges, ranges below the midline
						if(r.Count>1){
							r.Sort();
							if(r.First() > r.Last()) r.Reverse();
							k = Convert.ToInt32(Math.Truncate(r.Count*pChannelReductionPct));
							temp2 = r[k];
							low_p = r.Min();
//							low_p = r.First();
						}else if(r.Count==1){
							temp2 = r[0];
							low_p = r[0];
						}else{//r.Count is zero
							temp2 = 0;
//							low_p = 0;
						}
					}
				}else if(ChannelRanges.Count==1){
					temp1 = ChannelRanges[t][0];
					temp2 = -temp1;
				}
//				ChannelRanges[t].Clear();
				dt1 = dt1.Date;
				if(!ChannelHistory.ContainsKey(dt1))
					ChannelHistory[dt1] = new SortedDictionary<int,List<double>>();
				if(!ChannelHistory[dt1].ContainsKey(t))
					ChannelHistory[dt1][t] = new List<double>();
				ChannelHistory[dt1][t].Add(temp1 / divide);//The positive ChannelRanges dictionary is now condensed.  The list of all individual distances is cleared and only the single percentile distance is saved
				ChannelHistory[dt1][t].Add(temp2 / divide);//The negative ChannelRanges dictionary is now condensed.  The list of all individual distances is cleared and only the single percentile distance is saved
				ChannelHistory[dt1][t].Add(hi_p);
				ChannelHistory[dt1][t].Add(low_p);
//Print("     "+t+" pctle is "+ChannelRanges[t][0].ToString("0"));
			}
		}
		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			Alert(id,prio,msg,wav,rearmSeconds,bkgBrush, foregroundBrush);
		}
		private void GetDistPctile(DateTime dt, ref double Upper, ref double Lower, ref double MaxPrice, ref double MinPrice){
			int id   = GetChannelRangesId(dt);
			if(ChannelHistory.ContainsKey(dt.Date) && ChannelHistory[dt.Date].ContainsKey(id)){
				Upper    = ChannelHistory[dt.Date][id][0];
				Lower    = ChannelHistory[dt.Date][id][1];
				MaxPrice = ChannelHistory[dt.Date][id][2];
				MinPrice = ChannelHistory[dt.Date][id][3];
			}
		}
//		private void AddTo_DiffToTheMAbyDay(DateTime dt, SortedDictionary<int,List<double>> Diffs, bool print = false){
//			if(!DiffToTheMAbyDay.ContainsKey(dt)){
//				DiffToTheMAbyDay[dt] = new SortedDictionary<int,double>();
//			}
//			if(DiffToTheMAbyDay.Count>0){
//				foreach(var dif in Diffs){
//					double avg = dif.Value.Average();
////if(print)Print(abar+":  "+dif.Key+":  "+avg);
//					DiffToTheMAbyDay.Last().Value[dif.Key] = avg;
//				}
//			}
//		}
		#endregion

		#region -- Data types and BandHit rectangle method --
		private class ListAndAvg{
			public double Avg = 0;
			public int ABar = 0;
			public List<double> L = new List<double>();
			public void CalcAvg(int period){
				while(L.Count>period) {
					L.RemoveAt(0);
				}
				Avg = L.Average();
			}
		}
		private SortedDictionary<int, ListAndAvg> 
			DiffToTheMA			= new SortedDictionary<int, ListAndAvg>();//int is time "0910" for 9:10am, double is Close-TheMA
		private SortedDictionary<int,List<double>> 
			ChannelRanges		= new SortedDictionary<int,List<double>>();
		private SortedDictionary<DateTime, SortedDictionary<int,List<double>>> 
			ChannelHistory		= new SortedDictionary<DateTime, SortedDictionary<int,List<double>>>();
		private SortedDictionary<int, Tuple<double,double,int>>
			Historical_D2MA		= new SortedDictionary<int, Tuple<double,double,int>>();  //each abar has an average DiffToTheMA value, Item1 is current bar avg dist, Item2 is avg dist from prior day, Item3 is the abar of the prior day

		bool CalculatedDiffs = false;
		int id = 0;
		double SLDistance  = 0;
		DateTime t = DateTime.MinValue;
		bool ErrorOnce = true;
		double upperDist10pctile = 0;
		double lowerDist10pctile = 0;
		double maxp = 0;
		double minp = 0;
		private int BandSignalABar = 0;
		private class BandHitInfo{
			public int EndABar = 0;
			public bool IsClosedOut = false;
			public double PctilePrice = 0;
			public double SLPrice = 0;
			public BandHitInfo(int endABar, double pctilePrice, double slPrice){	EndABar = endABar;	PctilePrice = pctilePrice;	SLPrice = slPrice;}
		}
		private SortedDictionary<int, BandHitInfo> BandHitRect = new SortedDictionary<int, BandHitInfo>();
		private void BandHit_Add(int cb0, double BandPrice, double SLPrice){
			if(BandHitRect.Count>0 && BandHitRect.Last().Value.EndABar >= cb0-5) return;
			if(!BandHitRect.ContainsKey(cb0-1)){
				BandHitRect[cb0-1] = new BandHitInfo(cb0, BandPrice, SLPrice);
			}
		}
		private DateTime RectStartDate;
		private List<int> MarkerIDs = new List<int>();

		private void BandHit_UpdateRect(bool FirstTick, int cb0, double BandPrice, double SLPrice, double MidLinePrice){
			if(ChartControl==null || !FirstTick || BandHitRect.Count == 0) {return;}
			var CrossedMidline = Lows[0][0] <= MidLinePrice && Highs[0][1] >= MidLinePrice;
			if(!CrossedMidline) CrossedMidline = Highs[0][0] >= MidLinePrice && Lows[0][1] <= MidLinePrice;
			if(!BandHitRect.Last().Value.IsClosedOut && (BandHitRect.Last().Value.EndABar < cb0-5 || CrossedMidline)) {BandHitRect.Last().Value.IsClosedOut = true; return;}

			var key = BandHitRect.Keys.Max();
			var tag = $"{Instruments[0].MasterInstrument.Name} {key}";
			RemoveDrawObject(tag);
			if(BandPrice < SLPrice){//this is an upperband hit
				BandHitRect.Last().Value.EndABar = cb0;
				BandHitRect.Last().Value.PctilePrice = Math.Min(BandPrice, BandHitRect.Last().Value.PctilePrice);
				BandHitRect.Last().Value.SLPrice = Math.Max(SLPrice, BandHitRect.Last().Value.SLPrice);
			}else{//this is a lowerband hit
				BandHitRect.Last().Value.EndABar = cb0;
				BandHitRect.Last().Value.PctilePrice = Math.Max(BandPrice, BandHitRect.Last().Value.PctilePrice);
				BandHitRect.Last().Value.SLPrice = Math.Min(SLPrice, BandHitRect.Last().Value.SLPrice);
			}
			if(pRectangleTemplateName.Length>0 && pRectangleTemplateName!="none"){
				var r = Draw.Rectangle(this, tag, Times[0].GetValueAt(key), BandHitRect[key].PctilePrice, Times[0].GetValueAt(CurrentBars[0]), BandHitRect[key].SLPrice, pGlobalizeRectangles, pRectangleTemplateName);
				r.IsLocked = true;
				if(!MarkerIDs.Contains(key))
					MarkerIDs.Add(key);
			}
			if(pDiamondTemplateName.Length>0 && pDiamondTemplateName!="none"){
//				BandHitRect.Last().Value.IsClosedOut = true;
				//Print(Times[0][0].ToString()+"   Drawing diamond  "+tag);
				Draw.Diamond(this, tag+"d%P", false, Times[0].GetValueAt(key), BandHitRect[key].PctilePrice, pGlobalizeDiamonds, pDiamondTemplateName);
				Draw.Diamond(this, tag+"dSL", false, Times[0].GetValueAt(key), BandHitRect[key].SLPrice, pGlobalizeDiamonds, pDiamondTemplateName);
				if(!MarkerIDs.Contains(key)){
					MarkerIDs.Add(key);
				}
			}
			while(MarkerIDs.Count > pMaxMarkersCount){
				RemoveDrawObject(tag);//the rectangle
				RemoveDrawObject(tag+"d%P");
				RemoveDrawObject(tag+"dSL");
				MarkerIDs.RemoveAt(0);
			}
		}
		#endregion

		bool alertTriggerSet = true;
		List<double> Ranges = new List<double>();
		double RangesAvg = 1;
		char WhichBandWasHit = ' ';
		bool BandAudibleAlertEnabled = true;
		protected override void OnBarUpdate()
		{
//return;
//			if(!LicServerPinged && !ValidLicense && ErrorOnce){
//				ErrorOnce = false;
//				Log($"License error:  LettoChannels not licensed for this machine id\nContact {SupportEmail} for details",LogLevel.Alert);
//			}
//			if(!ValidLicense && !IsDebug && ErrorOnce) {
//				ErrorOnce = false;
//				Log($"License error:  LettoChannels not licensed for this machine id\nContact {SupportEmail} for details",LogLevel.Alert);
//			}
//			if(!ErrorOnce){
//				return;
//			}
			int cb0 = CurrentBars[0];
			TheMA[0] = (cb0 < 5 ? Input[0] : Input[0] * constant1 + constant2 * TheMA[1]);

			if(pCycleDotPeriod>0 && cb0>pCycleDotPeriod*2.1){
				CalculateTrendAge(cb0-1, Input, cci);
				if(cci.ContainsKey(cb0-1)) {
					int sig = (int)cci[cb0-1][2];
					//if(sig == 2 || sig == -2) Print("------------------   Sig: "+sig);
					if(sig == -2) {CycleStrongSell[0] = High[0]+TickSize*pCycleDotSeparation; CycleAlertLauncher(sig, cb0);} else CycleStrongSell.Reset(0);
					if(sig == -1) {CycleSell[0]       = High[0]+TickSize*pCycleDotSeparation; CycleAlertLauncher(sig, cb0);} else CycleSell.Reset(0);
					if(sig == 2)  {CycleStrongBuy[0]  = Low[0]-TickSize*pCycleDotSeparation; CycleAlertLauncher(sig, cb0);}  else CycleStrongBuy.Reset(0);
					if(sig == 1)  {CycleBuy[0]        = Low[0]-TickSize*pCycleDotSeparation; CycleAlertLauncher(sig, cb0);}  else CycleBuy.Reset(0);
				}
			}
			var ValidDay = ValidDaysOfWeek.Contains(Times[0][0].DayOfWeek);
			if(!ValidDay) {
				PlotBrushes[0][0] = Brushes.Transparent;
				return;
			}
			id = GetChannelRangesId(Times[0][0]);
			AddToChannelRanges(id, (Input[0]-TheMA[0]));
			if(cb0 > 3 && Times[0][1].Date!=Times[0][0].Date){
				CalcDistPctileOnAllTimeperiods(1, Times[0][0].Date);//convert current CalculatedDiffs and generates predictions for today
			}
			if(IsFirstTickOfBar && cb0>1){
				if(!pUsePriceDiff && CurrentBars[0] > BarsArray[0].Count-5){
					if(Ranges.Count == 0){
						for(int i = 1; i<Math.Min(CurrentBars[0],30); i++)
							Ranges.Add(Range()[i]);
						Ranges.Add(Range()[1]);
						while(Ranges.Count>30) Ranges.RemoveAt(0);
						RangesAvg = Ranges.Average();
					}
				}
			//Calculate the Close-MA difference, save it for this time
				int ttt = ToTime(Times[0].GetValueAt(cb0-1))/100;//problem:  there will be gaps in the DiffToTheMA dictionary keys on Range/Renko/Tick bars
				double v = pUsePriceDiff ? Input.GetValueAt(cb0-1) - TheMA.GetValueAt(cb0-1) : (Input.GetValueAt(cb0-1) > TheMA.GetValueAt(cb0-1) ? 1:-1);
				if(!DiffToTheMA.ContainsKey(ttt)) DiffToTheMA[ttt] = new ListAndAvg();
				var prior_avg = DiffToTheMA[ttt].Avg;//this is the prior average, before considering the current bars Diff value
				var prior_abar = DiffToTheMA[ttt].ABar;
				DiffToTheMA[ttt].L.Add(v);
				DiffToTheMA[ttt].CalcAvg(this.pExpectationPeriod);
				DiffToTheMA[ttt].ABar = cb0-1;
				Historical_D2MA[cb0-1] = new Tuple<double,double,int>( DiffToTheMA[ttt].Avg, prior_avg, prior_abar );//used for plotting historical bars.
			}

			if(ChannelHistory.ContainsKey(Times[0][0].Date))
			{
				GetDistPctile(Times[0][0], ref upperDist10pctile, ref lowerDist10pctile, ref maxp, ref minp);
				PercentileUpper[0]   = TheMA[0] + upperDist10pctile;
				PercentileLower[0]   = TheMA[0] + lowerDist10pctile;
				SLDistance = (upperDist10pctile+Math.Abs(lowerDist10pctile))/2 * pOuterBandsMult;
				SLU[0]  = PercentileUpper[0] + SLDistance;
				SLL[0]  = PercentileLower[0] - SLDistance;

				MktAnalyzer[0] = 0;
				if(PercentileUpper.IsValidDataPoint(0) && Highs[0][0] > PercentileUpper[0]){
					MktAnalyzer[0] = 1;
//					WhichBandWasHit = 'U';
					if(Historical_D2MA.ContainsKey(CurrentBars[0]-1) && Historical_D2MA[CurrentBars[0]-1].Item1 < 0) MktAnalyzer[0] = 2;
				}
				else if(PercentileLower.IsValidDataPoint(0) && Lows[0][0] < PercentileLower[0]){
					MktAnalyzer[0] = -1;
//					WhichBandWasHit = 'L';
					if(Historical_D2MA.ContainsKey(CurrentBars[0]-1) && Historical_D2MA[CurrentBars[0]-1].Item1 > 0) MktAnalyzer[0] = -2;
				}

				
				int alertType = 0;
				if(MktAnalyzer[0] > 0){
					if(Times[0][0] > RectStartDate && ChartControl!=null && pEnableBandAlerts){
						BandHit_Add(cb0, PercentileUpper[0], SLU[0]);
						BandHit_UpdateRect(IsFirstTickOfBar, cb0, PercentileUpper[0], Math.Max(Highs[0][0], SLU[0]), TheMA[0]);
					}
					if(alertTriggerSet && BandSignalABar < cb0-1){
						BandSignalABar = cb0;
						alertType = UPPER_BAND_HIT;
						if(ChartControl!=null && pEnableBandAlerts && BandAudibleAlertEnabled){
							myAlert($"UBand{cb0}", Priority.High, Name+" UBand Hit", this.pUpperBandHit, 0, Brushes.Black, Brushes.Magenta);
							BandAudibleAlertEnabled = false;
						}
					}
				}
				else if(MktAnalyzer[0] < 0){
					if(Times[0][0] > RectStartDate && ChartControl!=null && pEnableBandAlerts){
						BandHit_Add(cb0, PercentileLower[0], SLL[0]);
						BandHit_UpdateRect(IsFirstTickOfBar, cb0, PercentileLower[0], Math.Min(Lows[0][0], SLL[0]), TheMA[0]);
					}
					if(alertTriggerSet && BandSignalABar < cb0-1){
						BandSignalABar = cb0;
						alertType = LOWER_BAND_HIT;
						if(ChartControl!=null && pEnableBandAlerts && BandAudibleAlertEnabled){
							myAlert($"LBand{cb0}", Priority.High, Name+" LBand Hit", this.pLowerBandHit, 0, Brushes.Black, Brushes.Lime);
							BandAudibleAlertEnabled = false;
						}
					}
				}
				if(!BandAudibleAlertEnabled && Lows[0][1] > PercentileLower[1] && Highs[0][1] < PercentileUpper[1]) {
					BandAudibleAlertEnabled = true;
				}

				var CloseDirection = 'D';
				if(Closes[0][0] > Opens[0][0]) CloseDirection = 'U';
				if(alertType == UPPER_BAND_HIT && CloseDirection == 'D'){
					alertTriggerSet = false;
					if(pBandHit_BkgStripeOpacity > 0 && ChartControl != null)
						BackBrush = BandHit_BkgStripe_Red;
					StratEntryPrice[0] = Closes[0][0];
					StratSLPrice[0] = MAX(Highs[0], 4)[0];
					StratT1Price[0] = TheMA[0];
					StratT2Price[0] = PercentileLower[0];
				}else if(alertType == LOWER_BAND_HIT && CloseDirection == 'U'){
					alertTriggerSet = false;
					if(pBandHit_BkgStripeOpacity > 0 && ChartControl != null)
						BackBrush = BandHit_BkgStripe_Green;
					StratEntryPrice[0] = Closes[0][0];
					StratSLPrice[0] = MIN(Lows[0], 4)[0];
					StratT1Price[0] = TheMA[0];
					StratT2Price[0] = PercentileUpper[0];
				}
				if(Closes[0][1] < PercentileUpper[1] && Closes[0][1] > PercentileLower[1]) alertTriggerSet = true;

				MaxP[0] = TheMA[0] + maxp;
				MinP[0] = TheMA[0] + minp;
			}
		}

		#region -- OnRender --
		private double ScaleMult = 1;
		private SharpDX.Direct2D1.Brush ExpectedDiffBrushDX, PosExpectedDiffBrushDX, NegExpectedDiffBrushDX, DimGrayBrushDX, YellowBrushDX;
		public override void OnRenderTargetChanged()
		{
			if(ChartControl == null) return;
			#region == OnRenderTargetChanged ==
			if(YellowBrushDX!=null   && !YellowBrushDX.IsDisposed)    {YellowBrushDX.Dispose();YellowBrushDX=null;}
			if(DimGrayBrushDX!=null   && !DimGrayBrushDX.IsDisposed)    {DimGrayBrushDX.Dispose();DimGrayBrushDX=null;}
			if(ExpectedDiffBrushDX!=null   && !ExpectedDiffBrushDX.IsDisposed)    {ExpectedDiffBrushDX.Dispose();ExpectedDiffBrushDX=null;}
			if(PosExpectedDiffBrushDX!=null   && !PosExpectedDiffBrushDX.IsDisposed)    {PosExpectedDiffBrushDX.Dispose();PosExpectedDiffBrushDX=null;}
			if(NegExpectedDiffBrushDX!=null   && !NegExpectedDiffBrushDX.IsDisposed)    {NegExpectedDiffBrushDX.Dispose();NegExpectedDiffBrushDX=null;}
			if(RenderTarget != null) {YellowBrushDX     = Brushes.Yellow.ToDxBrush(RenderTarget);  YellowBrushDX.Opacity = pOpacityYellowZone;}
			if(RenderTarget != null) {DimGrayBrushDX     = Brushes.DimGray.ToDxBrush(RenderTarget);  DimGrayBrushDX.Opacity = 0.95f;}
			if(RenderTarget != null) {ExpectedDiffBrushDX     = Brushes.Gold.ToDxBrush(RenderTarget);  ExpectedDiffBrushDX.Opacity = 0.5f;}
			if(RenderTarget != null) {PosExpectedDiffBrushDX  = pPositiveExpectationColor.ToDxBrush(RenderTarget); PosExpectedDiffBrushDX.Opacity = (float)pPositiveExpectationOpacity;}
			if(RenderTarget != null) {NegExpectedDiffBrushDX  = pNegativeExpectationColor.ToDxBrush(RenderTarget); NegExpectedDiffBrushDX.Opacity = (float)pNegativeExpectationOpacity;}
			#endregion
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
//return;
			if(chartControl == null) return;
			if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Up))
				ScaleMult++;
			if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Down))
				ScaleMult--;

			ScaleMult = Math.Max(1,ScaleMult);

			if(LaunchedAt != DateTime.MinValue){
				var ts = new TimeSpan(DateTime.Now.Ticks - LaunchedAt.Ticks);
				if(ts.TotalSeconds > 10) {
					RemoveDrawObject("lic");
					LaunchedAt = DateTime.MinValue;
				}
			}

			base.OnRender(chartControl, chartScale);
			float x  = 0f;
			float x1 = float.MinValue;
			float y  = 0f;
			float y0 = 0f;
int line = 768;
			float width = 1f;
			if(CurrentBar <= ChartBars.ToIndex && ChartBars.ToIndex-ChartBars.FromIndex>0){
				width = (chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex) - chartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex)) / (ChartBars.ToIndex-ChartBars.FromIndex);
			}
line=774;
			float histo_width = Math.Max(2, width - 3);
			if(pExpectationMarkers != LettoChannels_PredictionMarkers.none && PosExpectedDiffBrushDX!=null && NegExpectedDiffBrushDX!=null){
				//We want to print the average location of the spread to the MA, on each bar on the chart
				var max_key = ToTime(Times[0].GetValueAt(ChartBars.ToIndex))/100;
				var v1 = new SharpDX.Vector2(0,0);
				var v2 = new SharpDX.Vector2(0,0);
try{
				int tt0 = 0;
				int last_tt = 0;
				int i = 0;
				int dayold_abar = -1;
				double ma_price = 0;
line=784;
				if(pShowOBOSbands){
					for(i = Math.Max(0,ChartBars.FromIndex); i <= ChartBars.ToIndex; i++){
						v1.X = v2.X = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(Times[0].GetValueAt(i)))-2f;
						double h = Highs[0].GetValueAt(i);
						double l = Lows[0].GetValueAt(i);
						double pu = PercentileUpper.GetValueAt(i);
						double pl = PercentileLower.GetValueAt(i);
						double maxp = MaxP.GetValueAt(i);
						double minp = MinP.GetValueAt(i);
						if(h < maxp){
							v1.Y = chartScale.GetYByValue(maxp);
							if(h < pu){
								v2.Y = chartScale.GetYByValue(Math.Max(pu,h))-3f;
								RenderTarget.DrawLine(v1, v2, h > pu ? YellowBrushDX : DimGrayBrushDX, 1f);
							}else{
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f,4f), YellowBrushDX);
								v2.Y = chartScale.GetYByValue(pu);
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v2, 4f,4f), YellowBrushDX);
							}
						}
						if(l > MinP.GetValueAt(i)){
							v1.Y = chartScale.GetYByValue(minp);
							if(l > pl){
								v2.Y = chartScale.GetYByValue(Math.Min(pl, l))+3f;
								RenderTarget.DrawLine(v1, v2, l < pl ? YellowBrushDX:DimGrayBrushDX, 1f);
							}else{
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f,4f), YellowBrushDX);
								v2.Y = chartScale.GetYByValue(pl);
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v2, 4f,4f), YellowBrushDX);
							}
						}
						if(l > pu){
							v1.Y = chartScale.GetYByValue(pu);
//							v2.Y = chartScale.GetYByValue(Math.Min(maxp,l))+3f;
//							RenderTarget.DrawLine(v1, v2, YellowBrushDX, 3f);
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f,4f), YellowBrushDX);
						}else if(h < pl){
							v1.Y = chartScale.GetYByValue(pl);
//							v2.Y = chartScale.GetYByValue(Math.Max(minp,h))-3f;
//							RenderTarget.DrawLine(v1, v2, YellowBrushDX, 3f);
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f,4f), YellowBrushDX);
						}
					}
				}
				PosExpectedDiffBrushDX.Opacity = (float)pPositiveExpectationOpacity;
				NegExpectedDiffBrushDX.Opacity = (float)pNegativeExpectationOpacity;
				bool HistoThisBar = false;
				for(i = Math.Max(0,ChartBars.FromIndex); i <= ChartBars.ToIndex; i++){
					HistoThisBar = IsValidDaysAllOrToday || ValidDaysOfWeek.Contains(Times[0].GetValueAt(i).DayOfWeek);
					tt0 = ToTime(Times[0].GetValueAt(i))/100;
					last_tt = tt0;
line=792;
					if(HistoThisBar && Historical_D2MA.ContainsKey(i)){//DiffToTheMA.ContainsKey(tt0)){
						ma_price = TheMA.GetValueAt(i);
						dayold_abar = Historical_D2MA[i].Item3;
						x = chartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(Times[0].GetValueAt(i))) - 1f;
						y = chartScale.GetYByValue(ma_price + Historical_D2MA[i].Item2 * RangesAvg * ScaleMult);//plot the avg from 1-day prior to the current bar plotted...this was the predicted average
						v1.X = x;
						v1.Y = y;
						if(pExpectationMarkers == LettoChannels_PredictionMarkers.dots){
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f, 3f), Historical_D2MA[i].Item2 > 0 ? PosExpectedDiffBrushDX : NegExpectedDiffBrushDX);
						}else if(pExpectationMarkers == LettoChannels_PredictionMarkers.histo){
							var y1 = chartScale.GetYByValue(ma_price);
							v1.X = v2.X = x-1;
							v2.Y = y1;
							RenderTarget.DrawLine(v1, v2, Historical_D2MA[i].Item2 > 0 ? PosExpectedDiffBrushDX : NegExpectedDiffBrushDX, 2f);
						}
					}//else Print("DiffToTheMAbyDay did not contain key: "+tt0);
				}
				
line=808;
				float hwidth = 0f;
				HistoThisBar = IsValidDaysAllOrToday || ValidDaysOfWeek.Contains(Times[0].GetValueAt(i).DayOfWeek);
				if(CurrentBar <= ChartBars.ToIndex && HistoThisBar){
					ma_price = TheMA.GetValueAt(CurrentBar);
					var y1 = chartScale.GetYByValue(ma_price);
					v2.Y = y1;
					int bar_count = 0;
					while(x < ChartPanel.W){
						if(bar_count < 6){
							PosExpectedDiffBrushDX.Opacity = (float)pPositiveExpectationOpacity;
							NegExpectedDiffBrushDX.Opacity = (float)pNegativeExpectationOpacity;
							hwidth = 2f;
						}else{
							PosExpectedDiffBrushDX.Opacity = (float)pFuturePosExpectationOpacity;
							NegExpectedDiffBrushDX.Opacity = (float)pFutureNegExpectationOpacity;
							hwidth = histo_width;
						}
line=824;
						bar_count++;
						x = x + width;
						v1.Y = chartScale.GetYByValue(ma_price);
						if(Historical_D2MA.ContainsKey(dayold_abar)){
							v1.X = x;
							RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f, 1f), Plots[0].BrushDX);
							v1.Y = chartScale.GetYByValue(ma_price + Historical_D2MA[dayold_abar].Item1 * RangesAvg * ScaleMult);
							if(pExpectationMarkers == LettoChannels_PredictionMarkers.dots){
								RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(v1, 4f, 1f), Historical_D2MA[dayold_abar].Item1 > 0 ? PosExpectedDiffBrushDX : NegExpectedDiffBrushDX);
							}else if(pExpectationMarkers == LettoChannels_PredictionMarkers.histo){
								v1.X = v2.X = x-1;
								RenderTarget.DrawLine(v1, v2, Historical_D2MA[dayold_abar].Item1 > 0 ? PosExpectedDiffBrushDX : NegExpectedDiffBrushDX, hwidth);
							}
						}
						dayold_abar++;
					}
				}
}catch(Exception kee){Print(line+":  "+kee.ToString());}
			}
			#region -- Show Volatility Channels of future time periods --
			if(BarsArray[0].BarsPeriod.BarsPeriodType==BarsPeriodType.Minute){
				double future_upperpctle = 0;
				double future_lowerpctle = 0;
				double future_maxp = 0;
				double future_minp = 0;
				var vect = new SharpDX.Vector2(0,0);
//			GetDistPctile(BarsArray[0].GetTime(RMaB).AddMinutes(30), ref future_upperpctle, ref future_lowerpctle);//gets the pctile level 30-minutes from now
//			vect.X = ChartPanel.W-25f;
//			vect.Y = (float)chartScale.GetYByValue(future_upperpctle + KeyLevel.GetValueAt(RMaB));
//			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, 2f,2f), Plots[1].BrushDX);
////			future_pctle = GetDistPctile(BarsArray[0].GetTime(RMaB).AddMinutes(30));
//			vect.Y = (float)chartScale.GetYByValue(future_lowerpctle + KeyLevel.GetValueAt(RMaB));
//			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, 2f,2f), Plots[2].BrushDX);
				vect.X = chartControl.GetXByBarIndex(ChartBars, CurrentBars[0]);
				DateTime ti = DateTime.MinValue;
				ti = BarsArray[0].LastBarTime.AddMinutes(BarsArray[0].BarsPeriod.Value);
				Plots[1].BrushDX.Opacity = 0.5f;
				Plots[2].BrushDX.Opacity = 0.5f;
				Plots[9].BrushDX.Opacity = 0.5f;
				Plots[10].BrushDX.Opacity = 0.5f;
				int RMaB = Math.Min(ChartBars.ToIndex, CurrentBars[0]);
				while(vect.X < ChartPanel.W){
					vect.X = vect.X + width;
					GetDistPctile(ti, ref future_upperpctle, ref future_lowerpctle, ref future_maxp, ref future_minp);//gets the pctile level x-minutes from rightmost bar
					vect.Y = (float)chartScale.GetYByValue(future_upperpctle + TheMA.GetValueAt(RMaB));
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, 4f,4f), Plots[1].BrushDX);
					vect.Y = (float)chartScale.GetYByValue(future_lowerpctle + TheMA.GetValueAt(RMaB));
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, 4f,4f), Plots[2].BrushDX);
					vect.Y = (float)chartScale.GetYByValue(future_maxp + TheMA.GetValueAt(RMaB));
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, Plots[9].Width,Plots[9].Width), Plots[9].BrushDX);
					vect.Y = (float)chartScale.GetYByValue(future_minp + TheMA.GetValueAt(RMaB));
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(vect, Plots[10].Width, Plots[10].Width), Plots[10].BrushDX);
					ti = ti.AddMinutes(BarsArray[0].BarsPeriod.Value);
				}
				Plots[1].BrushDX.Opacity = 1f;
				Plots[2].BrushDX.Opacity = 1f;
				Plots[9].BrushDX.Opacity = 1f;
				Plots[10].BrushDX.Opacity = 1f;
			}
			#endregion
		}
		#endregion

		#region Parameters
		[NinjaScriptProperty]
		[Display(Name = "License ID", Description = "Contact license@sbgtradingcorp.com for license ID", GroupName = "License", Order = 0, ResourceType = typeof(Custom.Resource))]
		public string pLicenseID
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MA period", GroupName = "Parameters", Order = 0)]
		public int pMAperiod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Day of week", Order=5, GroupName="Parameters", Description="'All' or 'Today' or 0=Sunday, 1=Monday.  Or use Su M Tu W Th F Sa")]
		public string pDaysOfWeek
		{get;set;}

		[NinjaScriptProperty]
		[Display(Name="Equidistant channel?", Order=10, GroupName="Parameters", Description="Equidistant channel is based on absolute value differences of price to the MA.  Turning off this setting, you get historical differences, which can lead to channel levels that are tuned to actual history")]
		public bool pEquidistantChannel
		{get;set;}

		[NinjaScriptProperty]
		[Display(Name="Timeslice", Order=15, GroupName="Parameters", Description="Number of minutes in each timeslice")]
		public int pMinutes
		{get;set;}

		[Range(0,1)]
		[NinjaScriptProperty]
		[Display(Name="Channel reduction pct", Order=75, GroupName="Parameters", Description="To reduce the channel size, what percentile of the ranges will be excluded?")]
		public double pChannelReductionPct
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Outer bands multiplier", Order=80, GroupName="Parameters", Description="Multiplier for location of outer bands")]
		public double pOuterBandsMult
		{ get; set; }
		
		[Range(0,200)]
		[Display(Name="Cycle Dots Period", Order=85, GroupName="Parameters", Description="Set to '0' to turn-off cycle dots")]
		public int pCycleDotPeriod
		{ get; set; }

		[Range(0,200)]
		[Display(Name="Cycle Dots tick separation", Order=86, GroupName="Parameters", Description="Distance between price bar and cycle dot")]
		public int pCycleDotSeparation
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Tendency Period (days)", Order=90, GroupName="Parameters", Description="Lookback period of tendency dots/histo")]
		public int pExpectationPeriod
		{ get; set; }
		#endregion
		
		#region -- Custom Visuals --
		[Display(Name="Tendency Markers?", Order=100, GroupName="Custom Visuals", Description="Show tendency dots/histo/none?")]
		public LettoChannels_PredictionMarkers pExpectationMarkers
		{ get; set; }

		[Display(Name="Tendency based on points?", Order=105, GroupName="Custom Visuals", Description="Tendency histo based on points?  If not, it will be based on counts")]
		public bool pUsePriceDiff
		{get;set;}
		
		[XmlIgnore]
		[Display(Name="Positive Tendency", Order=110, GroupName="Custom Visuals", Description="Color")]
		public Brush pPositiveExpectationColor
		{get;set;}
				[Browsable(false)]
				public string PEDClSerialize	{	get { return Serialize.BrushToString(pPositiveExpectationColor); } set { pPositiveExpectationColor = Serialize.StringToBrush(value); }}

		[Range(0,1)]
		[Display(Name="Past Pos. Tendency Opacity", Order=120, GroupName="Custom Visuals", Description="Signals behind current bar, opacity from 0 to 1")]
		public double pPositiveExpectationOpacity
		{get;set;}
		
		[Range(0,1)]
		[Display(Name="Future Pos. Tendency Opacity", Order=121, GroupName="Custom Visuals", Description="Signals ahead of current bar, opacity from 0 to 1")]
		public double pFuturePosExpectationOpacity
		{get;set;}

		[XmlIgnore]
		[Display(Name="Negative Tendency", Order=130, GroupName="Custom Visuals", Description="Color")]
		public Brush pNegativeExpectationColor
		{get;set;}
				[Browsable(false)]
				public string NEDClSerialize	{	get { return Serialize.BrushToString(pNegativeExpectationColor); } set { pNegativeExpectationColor = Serialize.StringToBrush(value); }}
		
		[Range(0,1)]
		[Display(Name="Past Neg. Tendency Opacity", Order=140, GroupName="Custom Visuals", Description="Signals behind current bar, opacity from 0 to 1")]
		public double pNegativeExpectationOpacity
		{get;set;}

		[Range(0,1)]
		[Display(Name="Future Neg. Tendency Opacity", Order=150, GroupName="Custom Visuals", Description="Signals ahead of current bar, opacity from 0 to 1")]
		public double pFutureNegExpectationOpacity
		{get;set;}
		
		[Display(Name="Show OB/OS bands?", Order=160, GroupName="Custom Visuals", Description="Region outside of the 90/10 lines")]
		public bool pShowOBOSbands
		{get;set;}
		
		[Display(Name="Opacity Yellow Zone", Order=170, GroupName="Custom Visuals", Description="Opacity of yellow histo bars showing OB OS signals")]
		public float pOpacityYellowZone
		{get;set;}

		#region -- Alerts --
		private string AddSoundFolder(string wav){
			if(wav == "none") return "";
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav.Replace("<inst>",Instrument.MasterInstrument.Name));
			if(!System.IO.File.Exists(wav)) {
				Log($"LettoChannels could not find wav: {wav}",LogLevel.Information);
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
				list.Add("<inst>_LettoChannel_OS.wav");
				list.Add("<inst>_LettoChannel_OB.wav");
				list.Add("<inst>_LettoChannel_UpperBand.wav");
				list.Add("<inst>_LettoChannel_LowerBand.wav");
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

		[Display(Order = 10, Name = "Enable Cycle alerts?", GroupName = "Alerts")]
		public bool pEnableCycleAlerts {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, Name = "Cycle Strong Sell", GroupName = "Alerts")]
		public string pCycleSell2 {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Cycle Sell", GroupName = "Alerts")]
		public string pCycleSell1 {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 40, ResourceType = typeof(Custom.Resource), Name = "Cycle Buy", GroupName = "Alerts")]
		public string pCycleBuy1 {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 50, ResourceType = typeof(Custom.Resource), Name = "Cycle Strong Buy", GroupName = "Alerts")]
		public string pCycleBuy2 {get;set;}

		[Display(Order = 60, ResourceType = typeof(Custom.Resource), Name = "Enable Band alerts?", GroupName = "Alerts")]
		public bool pEnableBandAlerts {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 70, ResourceType = typeof(Custom.Resource), Name = "UpperBand Hit", GroupName = "Alerts")]
		public string pUpperBandHit {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 80, ResourceType = typeof(Custom.Resource), Name = "LowerBand Hit", GroupName = "Alerts")]
		public string pLowerBandHit {get;set;}
		
		[Range(0,1)]
		[Display(Order = 90, Name = "Opacity bkg stripe", GroupName = "Alerts")]
		public double pBandHit_BkgStripeOpacity {get;set;}
		
		[Display(Order = 93, Name = "Max Marker count", Description="For better performance, limit the number of historical markers drawn (rectangles and diamonds)", GroupName = "Alerts")]
		public int pMaxMarkersCount
		{get;set;}

		[Display(Order = 95, Name = "Days back for markers", Description="Set to 0 to use 'Marker Start' date, otherwise a non-zero number of calendar days back from today", GroupName = "Alerts")]
		public int pDaysBackForRectangles {get;set;}
		[Display(Order = 100, Name = "Marker Start", GroupName = "Alerts")]
		public DateTime pRectangleStartDate {get;set;}
		
		internal class LoadRectTemplates : StringConverter
		{
			#region LoadRectTemplates
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
				string[] paths = new string[4]{NinjaTrader.Core.Globals.UserDataDir,"templates","DrawingTool","Rectangle"};
				string search = "*.xml";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(System.IO.Path.Combine(paths));
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("none");
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
		internal class LoadDiamondTemplates : StringConverter
		{
			#region LoadDiamondTemplates
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
				string[] paths = new string[4]{NinjaTrader.Core.Globals.UserDataDir,"templates","DrawingTool","Diamond"};
				string search = "*.xml";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(System.IO.Path.Combine(paths));
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("none");
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
		
		[Display(Order = 110, Name = "Globalize Rects", GroupName = "Alerts")]
		public bool pGlobalizeRectangles {get;set;}
		
		[Display(Order = 111, Name = "Rect template name", GroupName = "Alerts")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadRectTemplates))]
		public string pRectangleTemplateName {get;set;}
		
		[Display(Order = 120, Name = "Globalize Diamonds", GroupName = "Alerts")]
		public bool pGlobalizeDiamonds {get;set;}

		[Display(Order = 121, Name = "Diamond template name", GroupName = "Alerts")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadDiamondTemplates))]
		public string pDiamondTemplateName {get;set;}
		
		#endregion
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LettoChannels[] cacheLettoChannels;
		public LettoChannels LettoChannels(string pLicenseID, int pMAperiod, string pDaysOfWeek, bool pEquidistantChannel, int pMinutes, double pChannelReductionPct, double pOuterBandsMult, int pExpectationPeriod)
		{
			return LettoChannels(Input, pLicenseID, pMAperiod, pDaysOfWeek, pEquidistantChannel, pMinutes, pChannelReductionPct, pOuterBandsMult, pExpectationPeriod);
		}

		public LettoChannels LettoChannels(ISeries<double> input, string pLicenseID, int pMAperiod, string pDaysOfWeek, bool pEquidistantChannel, int pMinutes, double pChannelReductionPct, double pOuterBandsMult, int pExpectationPeriod)
		{
			if (cacheLettoChannels != null)
				for (int idx = 0; idx < cacheLettoChannels.Length; idx++)
					if (cacheLettoChannels[idx] != null && cacheLettoChannels[idx].pLicenseID == pLicenseID && cacheLettoChannels[idx].pMAperiod == pMAperiod && cacheLettoChannels[idx].pDaysOfWeek == pDaysOfWeek && cacheLettoChannels[idx].pEquidistantChannel == pEquidistantChannel && cacheLettoChannels[idx].pMinutes == pMinutes && cacheLettoChannels[idx].pChannelReductionPct == pChannelReductionPct && cacheLettoChannels[idx].pOuterBandsMult == pOuterBandsMult && cacheLettoChannels[idx].pExpectationPeriod == pExpectationPeriod && cacheLettoChannels[idx].EqualsInput(input))
						return cacheLettoChannels[idx];
			return CacheIndicator<LettoChannels>(new LettoChannels(){ pLicenseID = pLicenseID, pMAperiod = pMAperiod, pDaysOfWeek = pDaysOfWeek, pEquidistantChannel = pEquidistantChannel, pMinutes = pMinutes, pChannelReductionPct = pChannelReductionPct, pOuterBandsMult = pOuterBandsMult, pExpectationPeriod = pExpectationPeriod }, input, ref cacheLettoChannels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LettoChannels LettoChannels(string pLicenseID, int pMAperiod, string pDaysOfWeek, bool pEquidistantChannel, int pMinutes, double pChannelReductionPct, double pOuterBandsMult, int pExpectationPeriod)
		{
			return indicator.LettoChannels(Input, pLicenseID, pMAperiod, pDaysOfWeek, pEquidistantChannel, pMinutes, pChannelReductionPct, pOuterBandsMult, pExpectationPeriod);
		}

		public Indicators.LettoChannels LettoChannels(ISeries<double> input , string pLicenseID, int pMAperiod, string pDaysOfWeek, bool pEquidistantChannel, int pMinutes, double pChannelReductionPct, double pOuterBandsMult, int pExpectationPeriod)
		{
			return indicator.LettoChannels(input, pLicenseID, pMAperiod, pDaysOfWeek, pEquidistantChannel, pMinutes, pChannelReductionPct, pOuterBandsMult, pExpectationPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LettoChannels LettoChannels(string pLicenseID, int pMAperiod, string pDaysOfWeek, bool pEquidistantChannel, int pMinutes, double pChannelReductionPct, double pOuterBandsMult, int pExpectationPeriod)
		{
			return indicator.LettoChannels(Input, pLicenseID, pMAperiod, pDaysOfWeek, pEquidistantChannel, pMinutes, pChannelReductionPct, pOuterBandsMult, pExpectationPeriod);
		}

		public Indicators.LettoChannels LettoChannels(ISeries<double> input , string pLicenseID, int pMAperiod, string pDaysOfWeek, bool pEquidistantChannel, int pMinutes, double pChannelReductionPct, double pOuterBandsMult, int pExpectationPeriod)
		{
			return indicator.LettoChannels(input, pLicenseID, pMAperiod, pDaysOfWeek, pEquidistantChannel, pMinutes, pChannelReductionPct, pOuterBandsMult, pExpectationPeriod);
		}
	}
}

#endregion
