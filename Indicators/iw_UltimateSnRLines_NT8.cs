
#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using System.Collections.Generic;
#endregion

using System.ComponentModel.DataAnnotations;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
//using NinjaTrader.Gui.Chart;
//using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
//using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
using SharpDX.Direct2D1;
//using SharpDX;
//using SharpDX.DirectWrite;

namespace NinjaTrader.NinjaScript.Indicators
{
	public class UltimateSnRLines : Indicator
	{

		private struct DayInfo{
			public double H, L, C;
			public DateTime DT;
			public int ABar_DT;
			public DayInfo(DateTime dt, int abar, double h, double l, double c){this.DT=dt; this.ABar_DT=abar; this.H=h; this.L=l; this.C=c;}
		}
		private bool LicenseValid = true;

		#region Variables
		private double HH=double.NaN, LL=double.NaN;

		private	Series<double> bp,r1,r2,r3,r3a,r4,r4a,r4b,r5,r5a,r6;
		private	Series<double> s1,s2,s3,s3a,s4,s4a,s4b,s5,s5a,s6;

		private DateTime StartTime =DateTime.MinValue;
		private DateTime EndTime   =DateTime.MinValue;
		private DateTime t0 = DateTime.MinValue;
		private DateTime t1 = DateTime.MinValue;
		private bool RunInit=true;
		#endregion
		private string Version = "v5.6";
// v3.2 uses a 1-minute background datafeed so that the output is common across charts of different timeframes
// v3.5 Corrects a bug in the Plot method...causing plotted lines to disappear
// v5.2 Moved to DTS Ultimate SnR Lines name
// v5.3 Corrects bug when ChartControl==null, crashing when attempting Draw.TextFixed(this, ) on error message, also added Log.Information printing logic when error occurs
// v5.4 Added public dataseries (instead of Plots), this is for Bloodhound integration
// v5.5 Added OnRenderTargetChanged() to make more efficient brushes
// v5.6 Fixed OnRenderTargetChanged() if the Plot brush is null, don't attempt to produce a DXbrush
		private string NL = Environment.NewLine;
//		private	System.Windows.Media.Color[] colors = null;//new System.Windows.Media.Color[Plots.Length];//{ Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black }; 
		private	System.Windows.Media.Brush[] zoneBrushes			= null;//new SolidColorBrush[Plots.Length];
		private	SharpDX.Direct2D1.Brush[] zoneBrushesDX			= null;
		//private	StringFormat		stringFormatFar		= new StringFormat();
		//private	StringFormat		stringFormatNear	= new StringFormat();
		private NinjaTrader.Gui.Tools.SimpleFont textFont			= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 30);
		private bool     NewDay = false;
		private SortedDictionary<double,int> LevelZones=new SortedDictionary<double,int>();
		private double   LastClosePrice = -1;
		private int      BarProcessed = -1;
		private int      AlertsThisBar = -1;
		private int      PopupBar = -1;
		private int      LastLevelIdHit = -1;
		private DateTime EmailSentAt = DateTime.MinValue;
		private DateTime LaunchedAt = DateTime.MinValue;
		private bool     EnoughData = false;
		private double   FactorAdjuster = 1, pFactor=1, dayavg=-1;
		private SortedDictionary<int,double> FiveDayAvgRange;
		private bool     IsBen = false;
		private bool	 RunPlotMethod = true;
		private string chart_sessiontemplate;

		private DateTime dtNewDay     = DateTime.MinValue;
		private static readonly string idEST = "Eastern Standard Time";
		private TimeZoneInfo tziEST   = TimeZoneInfo.FindSystemTimeZoneById(idEST);
		private TimeZoneInfo tziLocal = TimeZoneInfo.Local;
//		private bool convertTZ = true;
		private List<DayInfo> DayRanges = new List<DayInfo>();
		private string ErrorText = string.Empty;
		private SortedDictionary<string,double> Levels = new SortedDictionary<string,double>();
//		private double HH=double.MinValue, LL=double.MinValue;
        private SharpDX.DirectWrite.TextFormat txtFormat_ChartFont = null;
        private SharpDX.DirectWrite.TextFormat txtFormat_ErrorFont = null;

//private string temp_msg = string.Empty;
		private static string MakeString(object[] s, string Separator){
			System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
			for(int i = 0; i<s.Length; i++) {
				stb = stb.Append(s[i].ToString());
				if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
			}
			return stb.ToString();
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				string module = "iwUltimateSnRLines";
				string ExemptMachine1 = "CB15E08BE30BC80628CFF6010471FA2A";
				string ExemptMachine2 = "B0D2E9D1C802E279D3678D7DE6A33CE4";
				bool ExemptMachine = NinjaTrader.Cbi.License.MachineId==ExemptMachine1 || NinjaTrader.Cbi.License.MachineId==ExemptMachine2;
				IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachine;				if(!IsBen)
					VendorLicense("IndicatorWarehouse", module, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				Name = "iw DTS UltSnR Lines "+Version;
				var LineColor = Brushes.PaleGreen;
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+10");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+09");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+08");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+07");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+06");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+05");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+04");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+03");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"+02");
				AddPlot(new Stroke(Brushes.Blue,1),PlotStyle.Hash,	"+01");
				AddPlot(new Stroke(Brushes.Goldenrod,1),PlotStyle.Hash,	"0");
				AddPlot(new Stroke(Brushes.Blue,1),PlotStyle.Hash,	"-01");
				LineColor = Brushes.Plum;
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-02");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-03");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-04");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-05");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-06");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-07");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-08");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-09");
				AddPlot(new Stroke(LineColor,1),PlotStyle.Hash,	"-10");

				IsOverlay=true;
				IsAutoScale=false;
				Calculate=Calculate.OnPriceChange;
				LicenseValid = true;
				if(!IsBen) {
				} else {
					LicenseValid = true;
				}

				//FactorAdj = new SortedDictionary<int,double>();
//				FiveDayAvgRange = new SortedDictionary<int,double>();
				if(IsBen) ClearOutputWindow();
				//if(IsBen) Print(Environment.NewLine+Environment.NewLine+"FiveDayAvgRange.Count: "+FiveDayAvgRange.Count+Environment.NewLine);

			}
			if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute,1);
			}
			if (State == State.DataLoaded)
			{
IsBen=false;
				DateTimeEST();
				dtNewDay = dtNewDay.AddDays(1).AddHours(pCustomTimeOffsetHrs);
				chart_sessiontemplate = BarsArray[0].TradingHours.Name;
				if(ChartControl!=null)
					txtFormat_ChartFont = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(),
														 	ChartControl.Properties.LabelFont.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)ChartControl.Properties.LabelFont.Size);
				else
					txtFormat_ChartFont = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(),
														 	"Arial",
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	14f);
				txtFormat_ErrorFont = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(),
														 	"Arial",
														 	SharpDX.DirectWrite.FontWeight.Normal,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	20f);
				bp = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r1 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r2 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r3 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r4 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r5 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r6 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r3a= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r4a= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r4b= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				r5a= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s1 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s2 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s3 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s4 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s5 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s6 = new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s3a= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s4a= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s4b= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				s5a= new Series<double>(this,MaximumBarsLookBack.Infinite);//inside State.DataLoaded
				if(ChartControl!=null) {
					textFont = ChartControl.Properties.LabelFont;
				}
				//colors      = new System.Windows.Media.Color[Plots.Length];//{ Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black }; 
				zoneBrushes	= new System.Windows.Media.Brush[Plots.Length];
				zoneBrushesDX	= new SharpDX.Direct2D1.Brush[Plots.Length];
				for(int id = 0; id<Plots.Length; id++){
					if (Plots[id].Pen.Brush.IsFrozen) zoneBrushes[id] = Plots[id].Pen.Brush.Clone();
					zoneBrushes[id].Opacity = this.pOpacity / 10.0;
					zoneBrushes[id].Freeze();
					zoneBrushesDX[id] = null;
				}
			}

		}

		private void DateTimeEST()
		{
			DateTime dt = new DateTime( Times[0][0].Year, Times[0][0].Month, Times[0][0].Day, 0, 0, 0 );
			dtNewDay = ( idEST == tziLocal.Id )?
				dt:
				TimeZoneInfo.ConvertTime(dt, tziEST, tziLocal);
if(IsBen) Print("Converted dt "+dt.ToString()+" to dtNewDay "+dtNewDay.ToString());
			TimeSpan ts = new TimeSpan(dt.Ticks - dtNewDay.Ticks);
//			pHoursChartOffset = ts.TotalHours;
//if(IsBen) Print("Hours ChartOffset: "+pHoursChartOffset);
//			if(pHoursChartOffset != 0)
//				dt = dt.AddHours(pHoursChartOffset);
			if(pHoursTimezoneOffset != 0) {
				dtNewDay = dtNewDay.AddHours(pHoursTimezoneOffset);
//Print("  Added "+pHoursTimezoneOffset+" to dtNewDay, which is now "+dtNewDay.ToString());
			}
		}
	//============================================================================================
		protected override void OnBarUpdate()
		{
int line = 239;
//Print("240   cb1: "+CurrentBars[1]+"   cb0: "+CurrentBars[0]+"   LicenseValid: "+LicenseValid.ToString());
try{
			if(!LicenseValid || CurrentBars[1]<=2 || CurrentBars[0]<=2) return;
//if(IsBen)Print(243);
			DateTime t0 = Times[1][0];//.AddHours(pHoursChartOffset);
			DateTime t1 = Times[1][1];//.AddHours(pHoursChartOffset);
			if(RunInit) {
				RunInit = false;
				while(Times[1][0] > dtNewDay) dtNewDay = dtNewDay.AddDays(1);
if(IsBen) Print("NewDay adjusted to:  "+dtNewDay.ToString()+"    Times[1][0]: "+Times[1][0].ToString());
			}
if(IsBen && t0.DayOfWeek==DayOfWeek.Sunday && ChartControl!=null) {
	BackBrush=Brushes.Maroon;
}
line=254;
//if(IsBen && t0.DayOfWeek!=t1.DayOfWeek) Draw.VerticalLine(this, CurrentBar.ToString(),t0,Color.Yellow,DashStyleHelper.Dash,2);
//if(IsBen && Bars.BarsSinceSession==0) Draw.VerticalLine(this, CurrentBar.ToString(),t0,Color.Yellow,DashStyleHelper.Dash,2);
//if(IsBen) Draw.Text(this, CurrentBar.ToString()+"BSS",false,Bars.BarsSinceSession.ToString(),0,Low[0]-TickSize,0,Color.Black,ChartControl.Properties.LabelFont,TextAlignment.Far,Color.Yellow,Color.Yellow,10);

//if(IsBen) Print("1375 UltSnR lines   BIP = "+BarsInProgress);
if(IsBen && BarsInProgress==1) {
	TimeSpan ts = new TimeSpan(dtNewDay.Ticks-t0.Ticks);
	//if(IsBen) Print("bip=1, ts.TotalsMinutes: "+ts.TotalMinutes);
	if(ts.TotalMinutes<10 && IsBen) Print("BIP=1:   t0: "+t0.ToString()+"   dtNewDay: "+dtNewDay.ToString());
}
			if(t1 <= dtNewDay && t0 > dtNewDay && BarsInProgress==1 && BarProcessed != CurrentBars[1])
			{
line=267;
if(IsBen) Print("  *****************************    t0: "+t0.ToString()+"  t1: "+t1.ToString()+"    NewDay: "+dtNewDay.ToString());
				BarProcessed = CurrentBars[1];
				if(!double.IsNaN(HH)) {
					DateTime t2 = Times[1][2];//.AddHours(pHoursChartOffset);
					int offset = 0;
					if(!pIgnoreSundays || t2.DayOfWeek!=DayOfWeek.Sunday) {
						if(t0.DayOfWeek==DayOfWeek.Sunday) offset = 0; else offset = 1;
						DayRanges.Insert(0,new DayInfo(t1,CurrentBar,HH,LL,Closes[1][offset+1]));
if(IsBen) Draw.VerticalLine(this, CurrentBar.ToString(),t0,Brushes.Yellow,DashStyleHelper.Dash,2);
					}
				}
line=279;
//if(IsBen && DayRanges.Count>0) Draw.Text(this, CurrentBar.ToString()+"xx",false,"DayRanges[0].DayOfWeek: "+DayRanges[0].DT.DayOfWeek.ToString(),0,Highs[1][0],10,Brushes.Black,ChartControl.Properties.LabelFont,TextAlignment.Far,Color.Yellow,Color.Yellow,10);
//				while(dtNewDay.DayOfWeek==DayOfWeek.Friday) dtNewDay = dtNewDay.AddDays(1);
//if(IsBen) Print("Next NewDay is: "+dtNewDay.ToString());

				HH = double.NaN;
				FactorAdjuster = 1;
if(IsBen) Print("  Days: "+DayRanges.Count);
				double ratio = 0;
				if(DayRanges.Count>0 && pAutoFactor != UltimateSnRLines_FactorAdjType.None) {
line=289;
					double dr = 0;
					int count = 0;
					for(int bb = 0; bb<5; bb++) {
						if(bb<DayRanges.Count) {
							double r = DayRanges[bb].H-DayRanges[bb].L; 
							dr = dr + r;
if(IsBen) Print("  "+bb+":  "+DayRanges[bb].DT.ToString()+"  Range: "+r+"  H: "+DayRanges[bb].H.ToString()+"  L: "+DayRanges[bb].L.ToString());
							count++;
						}
					}
					dayavg = dr / count;
					ratio = (DayRanges[0].H-DayRanges[0].L) / dayavg;
				}
line=303;

				if(pAutoFactor == UltimateSnRLines_FactorAdjType.Simple) {
					if(ratio < 0.49) FactorAdjuster = 2.0;
					else FactorAdjuster = 1.0;
//				} else if(pAutoFactor == UltimateSnRLines_FactorAdjType.Advanced) {
//					if(ratio > 1.5) FactorAdjuster = 0.5;
//					else if(ratio>=0.5) FactorAdjuster = 1.0;
//					else if(ratio>=0.3) FactorAdjuster = 2.33 / 1.55;
//					else FactorAdjuster = 2.0;
//				} else if(pAutoFactor == UltimateSnRLines_FactorAdjType.Continuous) {
//					FactorAdjuster = Math.Abs(1.0 - (DayRanges[0].H-DayRanges[0].L) / dayavg);
				}
//if(IsBen) Draw.Text(this, CurrentBar.ToString()+"TT",false,"FactorAdj: "+FactorAdjuster+"  The ratio: "+ratio.ToString("0.000"),0,Low[0],10,Color.Black,ChartControl.Properties.LabelFont,TextAlignment.Far,Color.Red,Color.Red,10);
line=317;


//				if(!IsFirstTickOfBar) return;
	try{
//				if(CurrentBars[1]<pDaysBack+1) return;
				EnoughData = DayRanges.Count>=5;
if(IsBen && ChartControl==null) Print(Time[0].ToString()+"  EnoughData?   "+EnoughData.ToString());
				NewDay = true;
	}catch(Exception err){Print("DataFeed 1:  "+err.ToString()+NL+Bars.Count+"-bars");}
			}

line=329;
			while(BarsInProgress==1 && t0 > dtNewDay) {
				dtNewDay = dtNewDay.AddDays(1);
			}

if(!EnoughData && IsBen) BackBrush=Brushes.Pink;

line=349;
			if(pAutoFactor != UltimateSnRLines_FactorAdjType.None || dayavg>0) {
				if(double.IsNaN(HH)) {
					if(!pIgnoreSundays || t0.DayOfWeek!=DayOfWeek.Sunday) {
						HH = Highs[1][0];
						LL = Lows[1][0];
					}
				} else {
					HH = Math.Max(HH, Highs[1][0]);
					LL = Math.Min(LL, Lows[1][0]);
				}
line=361;
				if(CurrentBar<pDaysBack+1) return;
				if(!EnoughData) {
line=366;
					TimeSpan DaysOnChart = new TimeSpan(Times[1][0].Ticks-Times[1][CurrentBars[1]].Ticks);
					double tradingdays = DaysOnChart.TotalDays / 7 * 5;
line=369;
					if(tradingdays>6)
						ErrorText = string.Concat(tradingdays.ToString("0"),":",DayRanges.Count,"  '",Name,"' has experienced an internal data issue...",NL,"Please add 2 or 3 more days of data to the chart");
					else {
						ErrorText = string.Concat(tradingdays.ToString("0"),":",DayRanges.Count,"  '",Name,"' needs 5 more days of data...",NL,"1)  Go to DataSeries setup (Ctrl-F)",NL,"2)  Add 5 additional days to your 'Days to load' setting");
					}
line=375;
					return;
				}
line=378;
				pFactor = 1.55 * FactorAdjuster;
line=390;
				if(EnoughData && NewDay) {
					RemoveDrawObject("NEED_MORE_DATA");
					ErrorText = string.Empty;
					NewDay = false;
					int start = pDaysBack-1;
line=397;
					double DayClose = DayRanges[start].C;
					//double range = (Math.Max(dayavg*0.8,DayRanges[start].H-DayRanges[start].L))*1.5*pFactor;
					double range = DayRanges[start].H-DayRanges[start].L;
line=401;
					if(dayavg*0.8 > range) {
						range = dayavg*0.8*1.5*pFactor;
if(IsBen) Print("start: "+start+"   pFactor: "+(1.5*pFactor).ToString("0.000")+" * dayavg * 0.8 = " + range.ToString());
					} else {
						range = range*1.5*pFactor;
if(IsBen) Print("start: "+start+"   pFactor: "+(1.5*pFactor).ToString("0.000")+" * H-L "+DayRanges[start].H+"-"+DayRanges[start].L+" = "+range.ToString());
					}
					r6[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose + (range/0.75));
					r5[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose + range);
					r5a[0] = Instrument.MasterInstrument.RoundDownToTickSize( (r5[0]+r6[0])/2.0);
					r4[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose + range/2.0);
					r4b[0] = Instrument.MasterInstrument.RoundDownToTickSize( r5[0]-((r5[0]-r4[0])/3.0));
					r4a[0] = Instrument.MasterInstrument.RoundDownToTickSize( r4[0]+((r5[0]-r4[0])/3.0));
					r3[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose + range/4.0);
					r3a[0] = Instrument.MasterInstrument.RoundDownToTickSize( (r3[0]+r4[0])/2.0);
					r2[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose + range/6.0);
					r1[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose + range/12.0);
					s1[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose - range/12.0);
					bp[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose);//(r1+s1)/2.0);
					s2[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose - range/6.0);
					s3[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose - range/4.0);
					s4[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose - range/2.0);
					s3a[0] = Instrument.MasterInstrument.RoundDownToTickSize( (s3[0]+s4[0])/2.0);
					s5[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose - range);
					s4a[0] = Instrument.MasterInstrument.RoundDownToTickSize( s4[0] - ((s4[0]-s5[0])/3.0));
					s4b[0] = Instrument.MasterInstrument.RoundDownToTickSize( s5[0] + ((s4[0]-s5[0])/3.0));
					s6[0]  = Instrument.MasterInstrument.RoundDownToTickSize( DayClose - (range/0.75));
					s5a[0] = Instrument.MasterInstrument.RoundDownToTickSize( (s5[0]+s6[0])/2.0);
line=430;
					Levels["r6"] =r6[0];
					Levels["r5a"]=r5a[0];
					Levels["r5"] =r5[0];
					Levels["r4"]=r4[0];
					Levels["r4b"]=r4b[0];
					Levels["r4a"]=r4a[0];
					Levels["r3a"]=r3a[0];
					Levels["r3"] =r3[0];
					Levels["r2"] =r2[0];
					Levels["r1"] =r1[0];
					Levels["bp"] =bp[0];
					Levels["s1"] =s1[0];
					Levels["s2"] =s2[0];
					Levels["s3"] =s3[0];
					Levels["s3a"]=s3a[0];
					Levels["s4"] =s4[0];
					Levels["s4a"]=s4a[0];
					Levels["s4b"]=s4b[0];
					Levels["s5"] =s5[0];
					Levels["s5a"]=s5a[0];
					Levels["s6"] =s6[0];
					LevelZones.Clear();
if(IsBen) Print(line+": bp[0]: "+bp[0].ToString()+" s5a[0]: "+s5a[0].ToString());
line=454;
//Print("Date: "+Time[1].ToString());
					double[] levels = {r6[0],r5[0],r4[0],r3[0],r2[0],r1[0],bp[0],s1[0],s2[0],s3[0],s4[0],s5[0],s6[0],r5a[0],r4a[0],r4b[0],r3a[0],s5a[0],s4a[0],s4b[0],s3a[0]};
					if(pLevelStyle == UltimateSnRLines_LevelStyleType.Line || pZoneHeight==1) {
						for(int id=0;id<levels.Length;id++) LevelZones[levels[id]]=id;
					} else {
						int halfzoneticks = (int)(Math.Truncate(pZoneHeight / 2.0));
						double halfzoneprice = TickSize * halfzoneticks;
						for(int id=0;id<levels.Length;id++) {
							for(int idx = -halfzoneticks; idx<=halfzoneticks; idx++) LevelZones[levels[id]+idx*TickSize]=id;
						}
					}
				}else if(EnoughData && Levels.Count>0){
					r6[0] = Levels["r6"];
					r5[0] = Levels["r5"];
					r4[0] = Levels["r4"];
					r3[0] = Levels["r3"];
					r2[0] = Levels["r2"];
					r1[0] = Levels["r1"];
					r5a[0] = Levels["r5a"];
					r4a[0] = Levels["r4a"];
					r4b[0] = Levels["r4b"];
					r3a[0] = Levels["r3a"];
					bp[0] = Levels["bp"];
					s1[0] = Levels["s1"];
					s2[0] = Levels["s2"];
					s3[0] = Levels["s3"];
					s4[0] = Levels["s4"];
					s5[0] = Levels["s5"];
					s6[0] = Levels["s6"];
					s3a[0] = Levels["s3a"];
					s4a[0] = Levels["s4a"];
					s4b[0] = Levels["s4b"];
					s5a[0] = Levels["s5a"];
				}
line=489;
				if(!double.IsNaN(bp[0])){
					#region Public dataseries setting
//					BP[0] = (bp[0]);
//					R1[0] = (r1[0]);
//					S1[0] = (s1[0]);
//					R2[0] = (r2[0]);
//					S2[0] = (s2[0]);
//					R3[0] = (r3[0]);
//					S3[0] = (s3[0]);
//					R3a[0] = (r3a[0]);
//					S3a[0] = (s3a[0]);
//					R4[0] = (r4[0]);
//					S4[0] = (s4[0]);
//					R4a[0] = (r4a[0]);
//					S4a[0] = (s4a[0]);
//					R4b[0] = (r4b[0]);
//					S4b[0] = (s4b[0]);
//					R5[0] = (r5[0]);
//					S5[0] = (s5[0]);
//					R5a[0] = (r5a[0]);
//					S5a[0] = (s5a[0]);
//					R6[0] = (r6[0]);
//					S6[0] = (s6[0]);
					#endregion
//if(IsBen) Print("1632: bp[0]: "+bp[0].ToString());
					#region Plot setting
					P10[0]=r6[0];
					P9[0]=r5a[0];
					P8[0]=r5[0];
					P7[0]=r4b[0];
					P6[0]=r4a[0];
					P5[0]=r4[0];
					P4[0]=r3a[0];
					P3[0]=r3[0];
					P2[0]=r2[0];
					P1[0]=r1[0];
					BPoint[0] = bp[0];
					M1[0] = s1[0];
					M2[0] = s2[0];
					M3[0] = s3[0];
					M4[0] = s3a[0];
					M5[0] = s4[0];
					M6[0] = s4a[0];
					M7[0] = s4b[0];
					M8[0] = s5[0];
					M9[0] = s5a[0];
					M10[0] = s6[0];
					#endregion

					if(pUseHorizLines) {
						int id = 0;
						NinjaTrader.NinjaScript.DrawingTools.HorizontalLine hl;
						for(int idx = 0; idx<Values.Length; idx++)
							hl = Draw.HorizontalLine(this, MakeString(new Object[] {Plots[idx].Name,"USR",idx},string.Empty), Values[idx][0], Plots[idx].Brush);
					}
//					if(IsBen) {
//						Values[21][0] = (DayRanges[0].H);
//						Values[22][0] = (DayRanges[0].L);
//					}


line=551;
					if(IsFirstTickOfBar) AlertsThisBar = 0;
					if(CurrentBars[0]>=BarsArray[0].Count-2) {
						if(ErrorText.Length>0) {
							if(ChartControl!=null){
								Draw.TextFixed(this, "NEED_MORE_DATA", ErrorText, TextPosition.Center, Brushes.Black, ChartControl.Properties.LabelFont, Brushes.Red, Brushes.Red, 10);
								Log(ErrorText,Cbi.LogLevel.Information);
							} else {
								Print(ErrorText);
								Log(ErrorText,Cbi.LogLevel.Information);
							}
						}
						double ptr = Instrument.MasterInstrument.RoundToTickSize(Math.Max(LastClosePrice,Closes[1][0]));
						while(ptr>=Math.Min(LastClosePrice,Closes[1][0])) {
							if(LevelZones.ContainsKey(ptr)) {
								int id = LevelZones[ptr];
								string type = "Level hit";    
								if(pLevelStyle != UltimateSnRLines_LevelStyleType.Line) type = "Zone entered";

								string msg = string.Concat("SnRLines on ",Instrument.FullName,":  ",type," at ",ptr.ToString());
								if(AlertsThisBar == 0 && pEmailAlert.Length>0) {
									SendMail(pEmailAlert, msg, string.Concat(Environment.NewLine,"Auto-generated message from '",Name,"' indicator in NinjaTrader"));
									Draw.TextFixed(this, "EMAIL","Level Hit!  Email message sent to "+pEmailAlert,TextPosition.TopRight);
									EmailSentAt = NinjaTrader.Core.Globals.Now;
								}
								if(AlertsThisBar < pMaxSoundsPerBar && pEnableSoundAlert) {
									if(AlertsThisBar==0) Alert(CurrentBar.ToString(),NinjaTrader.NinjaScript.Priority.High, msg, AddSoundFolder(pLevelHitWAV),1,Brushes.Blue,Brushes.White);  
									else PlaySound(AddSoundFolder(pLevelHitWAV));
								}
								if(pEnablePopup && PopupBar!=CurrentBar) {
									if(id!=LastLevelIdHit) {
//										LastLevelIdHit = id;
										PopupBar = CurrentBar;
										Log(string.Concat("SnRLines ",type," at ",ptr.ToString()," on chart ",Instrument.FullName," ",Bars.BarsPeriod.ToString()),NinjaTrader.Cbi.LogLevel.Alert);
//										LaunchPopupWindow(string.Concat("SnRLines ",type," at ",ptr.ToString()), t0.ToShortTimeString(), string.Concat(Instrument.FullName," (",Bars.BarsPeriod.ToString(),")"),"");
									}
								}
								AlertsThisBar++;
								break;
							}
							ptr = ptr - TickSize;
						}
					}
					LastClosePrice = Closes[1][0];
line=595;
				}
//}catch(Exception err){Print(Bars.BarsPeriod.ToString()+" Error: "+err.ToString());}
			}
}catch(Exception err){Print(line+":  "+Environment.NewLine+err.ToString());}

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
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("none");
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

		#region Custom Plot
		private SharpDX.Direct2D1.Brush BlackBrushDX = null;
		private SharpDX.Direct2D1.Brush PinkBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(BlackBrushDX!=null && !BlackBrushDX.IsDisposed)      BlackBrushDX.Dispose();      BlackBrushDX   = null;
			if(RenderTarget!=null) {BlackBrushDX = Brushes.Black.ToDxBrush(RenderTarget);}
			if(PinkBrushDX!=null && !PinkBrushDX.IsDisposed)      PinkBrushDX.Dispose();      PinkBrushDX   = null;
			if(RenderTarget!=null) {PinkBrushDX = Brushes.Pink.ToDxBrush(RenderTarget);}
			for(int i = 0; i<zoneBrushes.Length; i++){
				if(zoneBrushesDX[i]!=null && !zoneBrushesDX[i].IsDisposed)     zoneBrushesDX[i].Dispose();      zoneBrushesDX[i] = null;
				if(RenderTarget!=null && zoneBrushes[i]!=null) {zoneBrushesDX[i] = zoneBrushes[i].ToDxBrush(RenderTarget);}
			}
			
		}
		protected override void OnRender(NinjaTrader.Gui.Chart.ChartControl chartControl, NinjaTrader.Gui.Chart.ChartScale chartScale) {
			if (!IsVisible) return;
			//double min = chartScale.MinValue; 
			double max = chartScale.MaxValue;
			base.OnRender(chartControl, chartScale);
//			SharpDX.Point PanelUpperLeftPoint	= new SharpDX.Point(ChartPanel.X, ChartPanel.Y);
//			SharpDX.Point PanelLowerRightPoint	= new SharpDX.Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			//int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = ChartBars.ToIndex;

			if(!RunPlotMethod) return;
			if(!LicenseValid) return;
			if(ChartControl==null) return;
			var txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "", txtFormat_ChartFont, ChartPanel.W, txtFormat_ChartFont.FontSize);
			var txtPosition = new SharpDX.Vector2();
int line = 648;
		try{
			if(LaunchedAt==DateTime.MinValue) LaunchedAt = NinjaTrader.Core.Globals.Now;
			TimeSpan ts = new TimeSpan(NinjaTrader.Core.Globals.Now.Ticks - LaunchedAt.Ticks);
			if(ts.TotalSeconds<15){
				var sessionInfo = (chart_sessiontemplate.StartsWith("Default ")?string.Empty : MakeString(new object[]{NL,"Session template is:   ",chart_sessiontemplate,NL,"We recommend 'Default 24 x 5' or 'Default 24 x 7'",string.Empty},string.Empty));
				var tsTotal = new TimeSpan(Bars.GetTime(0).Ticks - Bars.GetTime(CurrentBar).Ticks);
				if (Bars == null) 
				{
					string s1 = MakeString(new object[]{"Ultimate S&R Lines:  Plotting cancelled",sessionInfo,NL},string.Empty);
					string s2 = null;
					if(Instrument.MasterInstrument.InstrumentType == InstrumentType.Future)
						s2 = "You do not have any bars on this chart...check your data connection and futures contract expiry";
					else
						s2 = "You do not have any bars on this chart...check your data connection";
					#region DrawTextLayout
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, MakeString(new object[]{s1,NL,s2},string.Empty), txtFormat_ErrorFont, (int)(ChartPanel.W*0.8), txtFormat_ErrorFont.FontSize);
					txtPosition = new System.Windows.Point(40, ChartPanel.H/4).ToVector2();
					#region Draw background rectangle
					var rectangleF = new SharpDX.RectangleF(txtPosition.X-3, txtPosition.Y-3, txtLayout.Metrics.Width+6, txtLayout.Metrics.Height+6);
					RenderTarget.FillRectangle(rectangleF, BlackBrushDX);
					RenderTarget.DrawRectangle(rectangleF, PinkBrushDX);
					#endregion
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, PinkBrushDX);
					txtLayout.Dispose();
					#endregion
					return;
				}
				else if(Bars.Count==0) 
				{
					string s1 = MakeString(new object[]{"Ultimate S&R Lines:  You do not have bars on this chart",sessionInfo,NL},string.Empty);
					string s2 = "Check your data connection, and load at least 10-days of data on this chart";
					#region DrawTextLayout
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, MakeString(new object[]{s1,NL,s2},string.Empty), txtFormat_ErrorFont, (int)(ChartPanel.W*0.8), txtFormat_ErrorFont.FontSize);
					txtPosition = new System.Windows.Point(40, ChartPanel.H/4).ToVector2();
					#region Draw background rectangle
					var rectangleF = new SharpDX.RectangleF(txtPosition.X-3, txtPosition.Y-3, txtLayout.Metrics.Width+6, txtLayout.Metrics.Height+6);
					RenderTarget.FillRectangle(rectangleF, BlackBrushDX);
					RenderTarget.DrawRectangle(rectangleF, PinkBrushDX);
					#endregion
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, PinkBrushDX);
					txtLayout.Dispose();
					#endregion
					return;
				}
				else if(Math.Abs(tsTotal.TotalDays) < 10) 
				{
					string s1 = MakeString(new object[]{"Ultimate S&R Lines:",sessionInfo,NL},string.Empty);
					string s2 = MakeString(new object[]{"Only ",Math.Abs(tsTotal.TotalDays).ToString("0"),"-days on chart",NL,"Make sure there are 10 (or more) days of data here, without gaps in the data"},string.Empty);
					#region DrawTextLayout
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, MakeString(new object[]{s1,NL,s2},string.Empty), txtFormat_ErrorFont, (int)(ChartPanel.W*0.8), txtFormat_ErrorFont.FontSize);
					txtPosition = new System.Windows.Point(40, ChartPanel.H/4).ToVector2();
					#region Draw background rectangle
					var rectangleF = new SharpDX.RectangleF(txtPosition.X-3, txtPosition.Y-3, txtLayout.Metrics.Width+6, txtLayout.Metrics.Height+6);
					RenderTarget.FillRectangle(rectangleF, BlackBrushDX);
					RenderTarget.DrawRectangle(rectangleF, PinkBrushDX);
					#endregion
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, PinkBrushDX);
					txtLayout.Dispose();
					#endregion
					return;
				}
			}
			if(LaunchedAt==DateTime.MinValue) LaunchedAt = NinjaTrader.Core.Globals.Now;

			if(EmailSentAt!=DateTime.MinValue) {
				ts = new TimeSpan(NinjaTrader.Core.Globals.Now.Ticks-EmailSentAt.Ticks);
				if(ts.TotalSeconds>15) {
					RemoveDrawObject("EMAIL");
					EmailSentAt = DateTime.MinValue;
				}
			}
			float ZoneHeightPixels = Math.Abs(chartScale.GetYByValue(max+TickSize*pZoneHeight));
			float LabelX = (float)chartScale.Width;
			int y = 0;
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				if(Plots[seriesCount].Name=="YH") continue;
				if(Plots[seriesCount].Name=="YL") continue;

				y = chartScale.GetYByValue(Values[seriesCount].GetValueAt(lastBarPainted));
				if(pLevelStyle == UltimateSnRLines_LevelStyleType.Zone || pLevelStyle == UltimateSnRLines_LevelStyleType.Both) {
					int x0  = chartControl.GetXByTime(DayRanges[0].DT);
					int x1 = chartControl.GetXByBarIndex(ChartBars, lastBarPainted+1);
					RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, y-ZoneHeightPixels/2.0f, x1-x0, ZoneHeightPixels), zoneBrushesDX[seriesCount]);
				}
				if(pShowLevelLabels) {
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[seriesCount].Name, txtFormat_ChartFont, ChartPanel.W, txtFormat_ChartFont.FontSize);
					if(chartScale.Width - txtLayout.Metrics.Width*2 < LabelX) LabelX = (float)chartScale.Width - txtLayout.Metrics.Width*2;
				}
			}
			if(pShowLevelLabels) {
				#region Draw labels on each line
				for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				{
					if(Plots[seriesCount].Name=="YH") continue;
					if(Plots[seriesCount].Name=="YL") continue;

					y	= chartScale.GetYByValue(Values[seriesCount].GetValueAt(lastBarPainted));
					txtLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, Plots[seriesCount].Name, txtFormat_ChartFont, ChartPanel.W, txtFormat_ChartFont.FontSize);
					txtPosition = new System.Windows.Point(LabelX, y-txtLayout.Metrics.Height/2).ToVector2();
					#region Draw background rectangle
					var rectangleF = new SharpDX.RectangleF(txtPosition.X-3, txtPosition.Y-3, txtLayout.Metrics.Width+6, txtLayout.Metrics.Height+6);
					RenderTarget.FillRectangle(rectangleF, BlackBrushDX);
					RenderTarget.DrawRectangle(rectangleF, PinkBrushDX);
					#endregion
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, Plots[seriesCount].BrushDX);
				}
				#endregion
			}
			txtLayout.Dispose();
		}catch(Exception ex){RunPlotMethod=false; Print(line+": "+ex.ToString());}
		}
		#endregion

		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P10
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P9
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P8
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P7
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P6
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P5
		{
			get { return Values[5]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P4
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P3
		{
			get { return Values[7]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P2
		{
			get { return Values[8]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> P1
		{
			get { return Values[9]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BPoint
		{
			get { return Values[10]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M1
		{
			get { return Values[11]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M2
		{
			get { return Values[12]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M3
		{
			get { return Values[13]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M4
		{
			get { return Values[14]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M5
		{
			get { return Values[15]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M6
		{
			get { return Values[16]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M7
		{
			get { return Values[17]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M8
		{
			get { return Values[18]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M9
		{
			get { return Values[19]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> M10
		{
			get { return Values[20]; }
		}
		#endregion
		#region Public dataseries
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R6
//		{get { Update();	return r6; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R5a
//		{get { Update();	return r5a; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R5
//		{get { Update();	return r5; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R4b
//		{get { Update();	return r4b; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R4a
//		{get { Update();	return r4a; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R4
//		{get { Update();	return r4; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R3a
//		{get { Update();	return r3a; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R3
//		{get { Update();	return r3; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R2
//		{get { Update();	return r2; }}
//		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> R1
//		{get { Update();	return r1; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> BP
//		{get { Update();	return bp; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S1
//		{get { Update();	return s1; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S2
//		{get { Update();	return s2; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S3
//		{get { Update();	return s3; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S3a
//		{get { Update();	return s3a; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S4
//		{get { Update();	return s4; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S4a
//		{get { Update();	return s4a; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S4b
//		{get { Update();	return s4b; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S5
//		{get { Update();	return s5; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S5a
//		{get { Update();	return s5a; }}
//
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> S6
//		{get { Update();	return s6; }}
		#endregion


		private double pHoursTimezoneOffset = 0;
//		[Description("Hours TimeZone offset")]
//		[Category("Parameters")]
//		public double HoursTimezoneOffset
//		{
//			get { return pHoursTimezoneOffset; }
//			set { pHoursTimezoneOffset = value; }
//		}
		private double pCustomTimeOffsetHrs = 0;
		[Description("Moves the level calculation forward (+) or backward (-) in time")]
		[Category("Parameters")]
		public double CustomTimeOffsetHrs
		{
			get { return pCustomTimeOffsetHrs; }
			set { pCustomTimeOffsetHrs = value; }
		}

		private bool pIgnoreSundays = true;
//		[Description("Do you want to ignore Sundays (basically consider them as part of Monday?)")]
//		[Category("Parameters")]
//		public bool IgnoreSundays
//		{
//			get { return pIgnoreSundays; }
//			set { pIgnoreSundays = value; }
//		}

//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds",wav);
		}
//====================================================================

		#region Properties
		private string pEmailAlert = "";
		[Description("Email address to receive a message when a price level is hit")]
		[Category("Alerts")]
		public string EmailAlert
		{
			get { return pEmailAlert; }
			set { pEmailAlert = value; }
		}

		private bool pEnablePopup = false;
		[Description("Enable popup when level is hit or zone is entered?")]
		[Category("Alerts")]
		public bool EnablePopup
		{
			get { return pEnablePopup; }
			set { pEnablePopup = value; }
		}
		private bool pEnableSoundAlert = true;
		[Description("Enable sound when level is hit or zone is entered?")]
		[Category("Alerts")]
		public bool EnableSoundAlert
		{
			get { return pEnableSoundAlert; }
			set { pEnableSoundAlert = value; }
		}
		private int pMaxSoundsPerBar = 1;
		[Description("Maximum number of audible alerts per bar")]
		[Category("Alerts")]
		public int MaxSoundsPerBar
		{
			get { return pMaxSoundsPerBar; }
			set { pMaxSoundsPerBar = Math.Max(1,value); }
		}
		private string pLevelHitWAV = "none";
		[Description("Sound played whenever a level is hit, or a zone is entered.  An 'Alerts' window message is printed as well")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Alerts")]
		public string LevelHitWAV
		{
			get { return pLevelHitWAV; }
			set { pLevelHitWAV = value; }
		}

		private UltimateSnRLines_FactorAdjType pAutoFactor = UltimateSnRLines_FactorAdjType.Simple;
//		[Description("What type of auto factor adjustment based on a continuous function")]
//		[Category("Parameters")]
//		public UltimateSnRLines_FactorAdjType AutoFactorAdj
//		{
//			get { return pAutoFactor; }
//			set { pAutoFactor = value; }
//		}
		private int pDaysBack = 1;
//		[Description("If you want to get the Buy or Sell signals to plot in advance of their actual time, increase this parameter")]
//		[Category("Parameters")]
//		public int DaysBack
//		{
//			get { return pDaysBack; }
//			set { pDaysBack = Math.Max(0,value); }
//		}
		private string pSessionStartTime = "0:00";
//		[Description("Start time for the session, each day...NOTE:  Use military (24hr) time for input")]
//		[Category("Parameters")]
//		public string SessionStartTime
//		{
//			get { return pSessionStartTime; }
//			set { pSessionStartTime = value; 
//				ParameterError = !DateTime.TryParse(pSessionStartTime, out StartTime);
//			}
//		}
		private double pSessionLengthHours = 23.99;
//		[Description("Session length, in hours...6.75 is 6 hours and 45 minutes")]
//		[Category("Parameters")]
//		public double SessionLengthHours
//		{
//			get { return pSessionLengthHours; }
//			set { pSessionLengthHours = Math.Max(0, Math.Min(24-1/60,value)); }
//		}
		private int lineWidth = -1;
//		[Description("Width of lines...set to -1 to make them stretch across entire chart")]
//		[Category("Visual")]
//		public int LineWidth
//		{
//			get { return lineWidth; }
//			set { lineWidth = Math.Max(-1,value); }
//		}
		private int pOpacity = 2;
		[Description("Opacity of zones (0=transparent, 10=opaque)...only valid if LevelStyle is Zone or Both")]
		[Category("Visual")]
		public int ZoneOpacity
		{
			get { return pOpacity; }
			set { pOpacity = Math.Max(0,Math.Min(10,value)); }
		}

		private int pZoneHeight = 2;
		[Description("Height of zones (in ticks), if LevelStyle is Zone or Both")]
		[Category("Visual")]
		public int ZoneHeight
		{
			get { return pZoneHeight; }
			set { pZoneHeight = Math.Max(1,value); }
		}
		private bool pShowLevelLabels = false;
		[Description("Show or hide the labels on each SnR level")]
		[Category("Visual")]
		public bool ShowLevelLabels
		{
			get { return pShowLevelLabels; }
			set { pShowLevelLabels = value; }
		}
		private bool pUseHorizLines = false;
		[Description("Draw levels as HorizontalLines")]
		[Category("Visual")]
		public bool UseHorizLines
		{
			get { return pUseHorizLines; }
			set { pUseHorizLines = value; }
		}
		private UltimateSnRLines_LevelStyleType pLevelStyle = UltimateSnRLines_LevelStyleType.Both;
		[Description("Level appearance")]
		[Category("Visual")]
		public UltimateSnRLines_LevelStyleType LevelStyle
		{
			get { return pLevelStyle; }
			set { pLevelStyle = value; }
		}

		private bool pIncludeCurrentDay=false;
//		[Description("Do you want to include the current (active) session in the calculation of the highest and lowest values?")]
//		[Category("Parameters")]
//		public bool IncludeCurrentDay
//		{
//			get { return pIncludeCurrentDay; }
//			set { pIncludeCurrentDay = value; }
//		}
        #endregion
	}
}
public enum UltimateSnRLines_LevelStyleType {
	Line, Zone, Both
}
public enum UltimateSnRLines_FactorAdjType {
	Continuous, Simple, Advanced, None
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UltimateSnRLines[] cacheUltimateSnRLines;
		public UltimateSnRLines UltimateSnRLines()
		{
			return UltimateSnRLines(Input);
		}

		public UltimateSnRLines UltimateSnRLines(ISeries<double> input)
		{
			if (cacheUltimateSnRLines != null)
				for (int idx = 0; idx < cacheUltimateSnRLines.Length; idx++)
					if (cacheUltimateSnRLines[idx] != null &&  cacheUltimateSnRLines[idx].EqualsInput(input))
						return cacheUltimateSnRLines[idx];
			return CacheIndicator<UltimateSnRLines>(new UltimateSnRLines(), input, ref cacheUltimateSnRLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UltimateSnRLines UltimateSnRLines()
		{
			return indicator.UltimateSnRLines(Input);
		}

		public Indicators.UltimateSnRLines UltimateSnRLines(ISeries<double> input )
		{
			return indicator.UltimateSnRLines(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UltimateSnRLines UltimateSnRLines()
		{
			return indicator.UltimateSnRLines(Input);
		}

		public Indicators.UltimateSnRLines UltimateSnRLines(ISeries<double> input )
		{
			return indicator.UltimateSnRLines(input);
		}
	}
}

#endregion
