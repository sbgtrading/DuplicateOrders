// 
// Copyright (C) 2012, SBG Trading Corp.
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//
//
//Description of inputs:
//
//1)  MaxBars
//	This indicator can be CPU intensive.  It identifies all the Fractals of specific Significance
//	over a specified period of bars.  This parameter, MaxBars, identifies that max "lookback" period.
//	Obviously, the larger the value of MaxBars, the slower this indicator will run.
//
//2)  MinimumSignificance
//	All fractals are identified by their significance, that is the number of bars before and after
//	it that do not re-touch that fractals high or low price value.  The lowest significance of any
//	fractal pivot is 1 meaning that the 1 bar before it and the 1 bar after it do not touch its
//	high or low price level.  This indicatorf dynamically identifies the significances of each
//	fractal pivot, and will disregard any fractal that has a smaller significance than specified
//	by MinimumSignificance.  Keep this number above 6 or 7.
//		
//3)  Percentile
//	When this indicator dynamically calculates the significance of each fractal pivot, there are going
//	to be a wide range of significances found.  Most fractals will be of small significance, some fractals
//	will have huge significances.  What Percentile does is control which fractals are plotted as lines.
//	The default value for Percentile is 0.1 (or 10%).  This means that only the top 10% of the most significant
//	fractal pivots are plotted.  Increasing Percentile will increase the number of less significant 
//	fractal pivot lines drawn.
//
//4)  StartOfAnalysis
//	This parameter lets you identify the first bar of the analysis period.  The earlier StartOfAnalysis is set,
//	the longer it will take for this indicator to load initially.  If StartOfAnalysis is set for the current
//	date, then this indicator will load quicker.  Basically, if you want to do a historical review of fractal pivots,
//	then move your chart to view early data and set StartOfAnalysis to that first date.  With that configuration,
//	you could then step through the data to see	how the fractal lines are drawn and removed as time progresses.  


#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Windows.Forms;
#endregion
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class FractalPivotConfluence : Indicator
    {
		private string VersionNumber = "5.0";
		private bool RunLicenseCheck = false; //runs the custom license check
        #region Variables
		private int          maxBars=5000;
		private int          significance = 20;
		private double       percentile = 0.2;
		private bool         pApplyVariableThickness = true;
		private List<int>    Flocs;
		private List<Point>  Fweight;
		private List<Point>  Levels;
		private float        MaxLevel, MinLevel;
		private Brush lineColor = Brushes.Red;
		//private Brush TintedLineColor = Brushes.Red;
		//private Brush TintedAxisColor = Brushes.Red;
		private int          FractalID;
		private bool         ParameterInputError = false;
		private bool         RunInit = true;
		private int          BarAtRecalculateTime = -1;
		private string       pRecalculateAtStr = "7:00";
		private DateTime     RecalculateTime;
		private float        KeySwitch = 0;
		//RBA is RightBarAbsolute, LBA is LeftBarAbsolute
		private int          RBA_Chart = 0, RBA_Secondary = 0, OldestBarAbs_Chart = 0, OldestBarAbs_Secondary = 0;

		private List<int>    PivotBars;
		private int          FlocsStartElement = 0;
		private int          FlocsEndElement = 0;
		
		private const int SPACE_KEY_VALUE = 32;
		private const int SHIFT_KEY_VALUE = 16;
		private int KeyValue = 0;
		private int LicenseStatus = 0;
		private long stopdate = 0;
		private DateTime TimeOfLaunch = NinjaTrader.Core.Globals.Now;

        #endregion
		private BarsPeriodType AcceptableBaseBarsPeriodType = BarsPeriodType.Tick;
		private string     instrument = string.Empty;
		private BarsPeriodType period_type = BarsPeriodType.Minute;
		private int        period_value = 1;
		private string     BaseInstrumentString = string.Empty;
		private bool       Debug=false;
		private int        BarsToGather = 0;
		private string     ErrorMsg = string.Empty;
		private int        BarAtBeginDateTime = int.MinValue, BarAtEndDateTime = int.MinValue;
		private string     BeginDateTimeInfoStr = string.Empty, EndDateTimeInfoStr = string.Empty;
		private string     NL = Environment.NewLine;
		private DateTime   SessionStartTime1, SessionEndTime1;

//		private DateTime EndAt = new DateTime(2010,5,31,23,59,0);

		private DayOfWeek DOW = DayOfWeek.Monday;
		private string RecalcType = "WEEK";
//===============================================================================================
		protected override void OnStateChange()
		{

			if (State == State.Configure)
			{
				ClearOutputWindow();
				#region Configure
				if(ErrorMsg.Length>0) {
					Print(ErrorMsg);
					Draw.TextFixed(this, "error1", ErrorMsg, TextPosition.TopLeft);
				}
				Draw.TextFixed(this, "initmsg", "...initializing FPC datafeed", TextPosition.TopLeft);
				//if(Debug) ClearOutputWindow();
				Flocs               = new List<int>();
				Fweight				= new List<Point>();
				Levels 				= new List<Point>();
					//Levels is a PointF list, the X coord is the price of the level, the Y coord is the LineCategory of the level
				PivotBars = new List<int>();
int line = 140;
				try{
					if(BarsToGather!=0) Print($"{Name} adding data: {Instruments[BarsToGather].FullName} {(BarsArray[BarsToGather].BarsPeriod.Value)} - {BarsArray[BarsToGather].BarsPeriod.BarsPeriodTypeName})");
					else Print($"{Name} using chart data: {Instruments[BarsToGather].FullName} ({BarsArray[BarsToGather].BarsPeriod.Value} - {BarsArray[BarsToGather].BarsPeriod.BarsPeriodTypeName})");
line=144;
					if(instrument.Length>0) BaseInstrumentString = string.Concat("\t\nFPC running on ",instrument,"  Period: ",period_value,"-",period_type.ToString());
					else BaseInstrumentString = string.Concat("\t\nFPC running on ",Instrument.FullName,"  Period: ",period_value,"-",period_type.ToString());
line=147;
					Calculate=Calculate.OnPriceChange;
					PaintPriceMarkers = true;
					string LicenseAdministrator = "  info@sbgtradingcorp.com  ";

line=150;
					if(!Debug && LicenseStatus <= 0) {ParameterInputError = true;return;}
	
					if(ChartControl == null)
						RBA_Chart = Bars.Count-1;
	
line=158;
				}catch(Exception osu){Print(line+"  Error "+Name+" - OnStartUp: "+osu.ToString());}
				//}
				RemoveDrawObject("initmsg");
				#endregion
				if(this.pMaxBars==0) BeginDateTimeInfoStr = "BeginDateTime set to "+this.pBeginDateTime.ToString()+NL;
				if(pEndDateTime != DateTime.MinValue) EndDateTimeInfoStr = "EndDateTime set to "+this.pEndDateTime.ToString()+NL;

				if(!pUseChartBars && ChartControl!=null) {
					AddDataSeries(AcceptableBaseBarsPeriodType, Period_Value);
					BarsToGather = 1;
				} else {
					BarsToGather = 0;
					AddDataSeries(BarsPeriodType.Minute, 60);//dummy datafeed, ignored in the code, used only because NinjaTrader requires it
				}
			}
			if (State == State.SetDefaults)
			{
				Name = "FractalPivotConfluence v"+VersionNumber;
				IsOverlay=true;
				//PriceTypeSupported	= false;
				Debug = System.IO.File.Exists(@"c:\222222222222.txt");
				RunLicenseCheck = false;
				if(!Debug) {
					VendorLicense("IndicatorWarehouse", "FractalPivotConfluence", "www.IndicatorWarehouse.com", "license@indicatorwarehouse.com");
					RunLicenseCheck = false;
					LicenseStatus = 999;
				} else {
					RunLicenseCheck = true;
				}
	        }
			if(State == State.DataLoaded){
				if(pRecalculateAtStr.Length>0) {
					try {
						var s = pRecalculateAtStr.ToLower();
						if(s.Contains("w")) RecalcType = "WEEK";
						else{
							RecalcType = "TIME";
							RecalculateTime = DateTime.Parse(pRecalculateAtStr);
							RecalculateTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day, RecalculateTime.Hour, RecalculateTime.Minute,0);
						}
					} catch {Log("Invalid Session Start time, must be in 24hr format: 'hh:mm', or enter Su/M/Tu/W/Th/F/Sa",LogLevel.Alert); ParameterInputError=true; return;}
				} else {
					RecalcType = "BAR0";
					RecalculateTime = DateTime.MaxValue;//Turns-off the RecalculateTime logic, calculations are continuous
				}
			}
		}
//===============================================================================================
//		private void MyKeyUpEvent(object sender, KeyEventArgs e) {
//			e.Handled = true;
//			try{
//				KeyValue = e.KeyValue;
//				if(KeyValue == SHIFT_KEY_VALUE) {
//					KeySwitch = KeySwitch + 1;
//					if(KeySwitch > 4) KeySwitch = 0;
//				}
//			}catch (Exception ex){ Print(string.Format(Name+" on "+Instrument.FullName+", MyKeyUpEvent Exception occured: {0}", ex.Message));}
//		}
//===============================================================================================
        protected override void OnBarUpdate()
        {	
//			Draw.TextFixed(this, "license",@"This trial version of "+Name+" will only run on data on or before "+EndAt.ToShortDateString()+".  Email bl@sbgtradingcorp.com for license info"+Environment.NewLine+".",TextPosition.BottomLeft);
//			if(Time[0].Ticks > EndAt.Ticks) {
//				return;
//			}
try{
			if(CurrentBar<10) return;
			if(ParameterInputError) return;

			if(CurrentBars[BarsToGather] < Math.Max(significance*3,15)) return;

			if(pBeginDateTime != DateTime.MinValue) {
				if(Times[BarsToGather][0] < pBeginDateTime) {
					return;
				} else {
					if(BarsInProgress == 0) Draw.VerticalLine(this, "startline", pBeginDateTime, Brushes.Pink, DashStyleHelper.Dash, 3);
					BarAtBeginDateTime = BarsArray[0].GetBar(pBeginDateTime);
				}
			}
			if(pEndDateTime != DateTime.MinValue) {
				if(Times[BarsToGather][0] > pEndDateTime) {
					BarAtEndDateTime = BarsArray[0].GetBar(pEndDateTime);
					if(BarsInProgress == 0 && Times[BarsToGather][1]<=pEndDateTime && Times[BarsToGather][0]>pEndDateTime) Draw.VerticalLine(this, "FPC_End",pEndDateTime, Brushes.Magenta, DashStyleHelper.Dash,3);
				}
				if(Times[BarsToGather][0]>pEndDateTime) return;
			}

			if(pMaxBars == 0) {
				if(BarsInProgress == BarsToGather) {
					if(pBeginDateTime != DateTime.MinValue && pEndDateTime != DateTime.MinValue) 
						maxBars = BarsArray[BarsToGather].GetBar(pEndDateTime) - BarsArray[BarsToGather].GetBar(pBeginDateTime);
					else if(pBeginDateTime != DateTime.MinValue)
						maxBars = CurrentBars[BarsToGather]-BarsArray[BarsToGather].GetBar(pBeginDateTime);
					else Draw.TextFixed(this, "error","_MaxBars was set to 0, you must now specify a BeginDateTime and, optionally, also specify EndDateTime.",TextPosition.Center);
				}
			}
			else {
				maxBars = pMaxBars;
				try{
					BarAtBeginDateTime = BarsArray[0].Count-maxBars;
					pBeginDateTime     = Times[0][maxBars];
//					if(BarsInProgress == 0) Draw.VerticalLine(this, "startline",pBeginDateTime,Color.Pink,DashStyleHelper.Dash,3);
				}catch{}
//				Print("StartDT: "+pBeginDateTime.ToString()+"  "+BarsArray[0].Count+"  maxbars: "+pMaxBars);
			}
//Print("maxBars: "+maxBars.ToString()+"  BarsAtBeginDateTime: "+BarAtBeginDateTime.ToString());
			if(BarsInProgress != BarsToGather) return;
			if(IsFirstTickOfBar) {
				double B = 0; double S = 0;
				if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) B = isHighPivot(significance, Brushes.Transparent);
				else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes) B = isCloseHighPivot(significance, Brushes.Transparent);
				else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Opens) B = isOpenHighPivot(significance, Brushes.Transparent);
	//			int B = Swing(significance).SwingHighBar(0,1,0);
				if(B>=0) AddFractal(CurrentBars[BarsToGather]-significance);

				if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) S = isLowPivot(significance, Brushes.Transparent);
				else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes) S = isCloseLowPivot(significance, Brushes.Transparent);
				else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Opens) S = isOpenLowPivot(significance, Brushes.Transparent);
	//			int S = Swing(significance).SwingLowBar(0,1,0);
				if(S>=0) AddFractal(-(CurrentBars[BarsToGather]-significance));
			}

			//Fill output DataSeries with all the levels, the terminator is a value of double.MinValue
}catch(Exception obu){Print("OBU Error "+Name+" - "+obu.ToString());}
        }
//========================================================================================================
		public List<Point> GetLevels(DateTime RecalculateTime) {
			#region GetLevels
			List<Point> lvls = null;
int line = 282;
try{
			int AbsBarAtLevelCalculation = 0;
			Update();
line=286;
			if(RecalculateTime != DateTime.MaxValue) {
				AbsBarAtLevelCalculation = BarsArray[BarsToGather].GetBar(RecalculateTime);
			} else {
				AbsBarAtLevelCalculation = CurrentBars[BarsToGather];
			}
line=290;
			int OldestBarAbs_Secondary = Math.Max(1,AbsBarAtLevelCalculation - maxBars);
line=292;
			Fweight = DetermineWeight(OldestBarAbs_Secondary, AbsBarAtLevelCalculation);
			if(Fweight==null || Fweight.Count==0) return Levels;
line=295;
			Fweight = SortHeavyToLight(Fweight);
			if(Fweight==null || Fweight.Count==0) return Levels;
line=297;
			lvls = GenerateRankHisto(Fweight);

}catch(Exception GetLevelsError){Print(string.Concat(line.ToString(),": ",Name,".GetLevels error: ",GetLevelsError.ToString()));}
			return lvls;
			#endregion
		}
//===================================================================
		private double isCloseHighPivot(int period, Brush color)
		{
			#region CloseHighPivot
			int y = 0;
			int Lvls = 0;

			//Four Matching Closes
			if(Closes[BarsToGather][period]==Closes[BarsToGather][period+1] && Closes[BarsToGather][period]==Closes[BarsToGather][period+2] && Closes[BarsToGather][period]==Closes[BarsToGather][period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Closes[BarsToGather][period+3]>Closes[BarsToGather][period+3+y] : Closes[BarsToGather][period+3]>Closes[BarsToGather][period+3+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period-y] : Closes[BarsToGather][period]>Closes[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Closes
			else if(Closes[BarsToGather][period]==Closes[BarsToGather][period+1] && Closes[BarsToGather][period]==Closes[BarsToGather][period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Closes[BarsToGather][period+2]>Closes[BarsToGather][period+2+y] : Closes[BarsToGather][period+2]>Closes[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period-y] : Closes[BarsToGather][period]>Closes[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Closes
			else if(Closes[BarsToGather][period]==Closes[BarsToGather][period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Closes[BarsToGather][period+1]>Closes[BarsToGather][period+1+y] : Closes[BarsToGather][period+1]>Closes[BarsToGather][period+1+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period-y] : Closes[BarsToGather][period]>Closes[BarsToGather][period-y])
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
					if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period+y] : Closes[BarsToGather][period]>Closes[BarsToGather][period+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period-y] : Closes[BarsToGather][period]>Closes[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Closes - First and Last Matching - Middle 2 are lower
				if(Closes[BarsToGather][period]>=Closes[BarsToGather][period+1] && Closes[BarsToGather][period]>=Closes[BarsToGather][period+2] && Closes[BarsToGather][period]==Closes[BarsToGather][period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Closes[BarsToGather][period+3]>Closes[BarsToGather][period+3+y] : Closes[BarsToGather][period+3]>Closes[BarsToGather][period+3+y])
							Lvls++;
						if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period-y] : Closes[BarsToGather][period]>Closes[BarsToGather][period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Closes - Middle is lower than two outside
				if(Closes[BarsToGather][period]>=Closes[BarsToGather][period+1] && Closes[BarsToGather][period]==Closes[BarsToGather][period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Closes[BarsToGather][period+2]>Closes[BarsToGather][period+2+y] : Closes[BarsToGather][period+2]>Closes[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]>Closes[BarsToGather][period-y] : Closes[BarsToGather][period]>Closes[BarsToGather][period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				
				return(Closes[BarsToGather][period]);
			}
			return(-1.0);
			#endregion
		}
//===================================================================
		private double isHighPivot(int period, Brush color)
		{
			#region HighPivot
			int y = 0;
			int Lvls = 0;

			//Four Matching Highs
			if(Highs[BarsToGather][period]==Highs[BarsToGather][period+1] && Highs[BarsToGather][period]==Highs[BarsToGather][period+2] && Highs[BarsToGather][period]==Highs[BarsToGather][period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Highs[BarsToGather][period+3]>Highs[BarsToGather][period+3+y] : Highs[BarsToGather][period+3]>Highs[BarsToGather][period+3+y])
						Lvls++;
					if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period-y] : Highs[BarsToGather][period]>Highs[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Highs
			else if(Highs[BarsToGather][period]==Highs[BarsToGather][period+1] && Highs[BarsToGather][period]==Highs[BarsToGather][period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Highs[BarsToGather][period+2]>Highs[BarsToGather][period+2+y] : Highs[BarsToGather][period+2]>Highs[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period-y] : Highs[BarsToGather][period]>Highs[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Highs
			else if(Highs[BarsToGather][period]==Highs[BarsToGather][period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Highs[BarsToGather][period+1]>Highs[BarsToGather][period+1+y] : Highs[BarsToGather][period+1]>Highs[BarsToGather][period+1+y])
						Lvls++;
					if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period-y] : Highs[BarsToGather][period]>Highs[BarsToGather][period-y])
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
					if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period+y] : Highs[BarsToGather][period]>Highs[BarsToGather][period+y])
						Lvls++;
					if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period-y] : Highs[BarsToGather][period]>Highs[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Highs - First and Last Matching - Middle 2 are lower
				if(Highs[BarsToGather][period]>=Highs[BarsToGather][period+1] && Highs[BarsToGather][period]>=Highs[BarsToGather][period+2] && Highs[BarsToGather][period]==Highs[BarsToGather][period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Highs[BarsToGather][period+3]>Highs[BarsToGather][period+3+y] : Highs[BarsToGather][period+3]>Highs[BarsToGather][period+3+y])
							Lvls++;
						if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period-y] : Highs[BarsToGather][period]>Highs[BarsToGather][period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Highs - Middle is lower than two outside
				if(Highs[BarsToGather][period]>=Highs[BarsToGather][period+1] && Highs[BarsToGather][period]==Highs[BarsToGather][period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Highs[BarsToGather][period+2]>Highs[BarsToGather][period+2+y] : Highs[BarsToGather][period+2]>Highs[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Highs[BarsToGather][period]>Highs[BarsToGather][period-y] : Highs[BarsToGather][period]>Highs[BarsToGather][period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{

				return(Highs[BarsToGather][period]);
			}
			return(-1.0);
			#endregion
		}
//===================================================================
		private double isOpenHighPivot(int period, Brush color)
		{
			#region OpenHighPivot
			int y = 0;
			int Lvls = 0;

			//Four Matching Opens
			if(Opens[BarsToGather][period]==Opens[BarsToGather][period+1] && Opens[BarsToGather][period]==Opens[BarsToGather][period+2] && Opens[BarsToGather][period]==Opens[BarsToGather][period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Opens[BarsToGather][period+3]>Opens[BarsToGather][period+3+y] : Opens[BarsToGather][period+3]>Opens[BarsToGather][period+3+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period-y] : Opens[BarsToGather][period]>Opens[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Opens
			else if(Opens[BarsToGather][period]==Opens[BarsToGather][period+1] && Opens[BarsToGather][period]==Opens[BarsToGather][period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Opens[BarsToGather][period+2]>Opens[BarsToGather][period+2+y] : Opens[BarsToGather][period+2]>Opens[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period-y] : Opens[BarsToGather][period]>Opens[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Opens
			else if(Opens[BarsToGather][period]==Opens[BarsToGather][period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Opens[BarsToGather][period+1]>Opens[BarsToGather][period+1+y] : Opens[BarsToGather][period+1]>Opens[BarsToGather][period+1+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period-y] : Opens[BarsToGather][period]>Opens[BarsToGather][period-y])
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
					if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period+y] : Opens[BarsToGather][period]>Opens[BarsToGather][period+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period-y] : Opens[BarsToGather][period]>Opens[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Opens - First and Last Matching - Middle 2 are lower
				if(Opens[BarsToGather][period]>=Opens[BarsToGather][period+1] && Opens[BarsToGather][period]>=Opens[BarsToGather][period+2] && Opens[BarsToGather][period]==Opens[BarsToGather][period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Opens[BarsToGather][period+3]>Opens[BarsToGather][period+3+y] : Opens[BarsToGather][period+3]>Opens[BarsToGather][period+3+y])
							Lvls++;
						if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period-y] : Opens[BarsToGather][period]>Opens[BarsToGather][period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Opens - Middle is lower than two outside
				if(Opens[BarsToGather][period]>=Opens[BarsToGather][period+1] && Opens[BarsToGather][period]==Opens[BarsToGather][period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Opens[BarsToGather][period+2]>Opens[BarsToGather][period+2+y] : Opens[BarsToGather][period+2]>Opens[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]>Opens[BarsToGather][period-y] : Opens[BarsToGather][period]>Opens[BarsToGather][period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				
				return(Opens[BarsToGather][period]);
			}
			return(-1.0);
			#endregion
		}
//===================================================================
		private double isCloseLowPivot(int period, Brush color)
		{
			#region CloseLowPivot
			int y = 0;
			int Lvls = 0;
			
			//Four Matching Closes
			if(Closes[BarsToGather][period]==Closes[BarsToGather][period+1] && Closes[BarsToGather][period]==Closes[BarsToGather][period+2] && Closes[BarsToGather][period]==Closes[BarsToGather][period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Closes[BarsToGather][period+3]<Closes[BarsToGather][period+3+y] : Closes[BarsToGather][period+3]<Closes[BarsToGather][period+3+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period-y] : Closes[BarsToGather][period]<Closes[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Closes
			else if(Closes[BarsToGather][period]==Closes[BarsToGather][period+1] && Closes[BarsToGather][period]==Closes[BarsToGather][period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Closes[BarsToGather][period+2]<Closes[BarsToGather][period+2+y] : Closes[BarsToGather][period+2]<Closes[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period-y] : Closes[BarsToGather][period]<Closes[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Closes
			else if(Closes[BarsToGather][period]==Closes[BarsToGather][period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Closes[BarsToGather][period+1]<Closes[BarsToGather][period+1+y] : Closes[BarsToGather][period+1]<Closes[BarsToGather][period+1+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period-y] : Closes[BarsToGather][period]<Closes[BarsToGather][period-y])
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
					if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period+y] : Closes[BarsToGather][period]<Closes[BarsToGather][period+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period-y] : Closes[BarsToGather][period]<Closes[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Closes - First and Last Matching - Middle 2 are lower
				if(Closes[BarsToGather][period]<=Closes[BarsToGather][period+1] && Closes[BarsToGather][period]<=Closes[BarsToGather][period+2] && Closes[BarsToGather][period]==Closes[BarsToGather][period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Closes[BarsToGather][period+3]<Closes[BarsToGather][period+3+y] : Closes[BarsToGather][period+3]<Closes[BarsToGather][period+3+y])
							Lvls++;
						if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period-y] : Closes[BarsToGather][period]<Closes[BarsToGather][period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Closes - Middle is lower than two outside
				if(Closes[BarsToGather][period]<=Closes[BarsToGather][period+1] && Closes[BarsToGather][period]==Closes[BarsToGather][period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Closes[BarsToGather][period+2]<Closes[BarsToGather][period+2+y] : Closes[BarsToGather][period+2]<Closes[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Closes[BarsToGather][period]<Closes[BarsToGather][period-y] : Closes[BarsToGather][period]<Closes[BarsToGather][period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				return(Closes[BarsToGather][period]);
			}
			return(-1.0);
			#endregion
		}
//===================================================================
		private double isLowPivot(int period, Brush color)
		{
			#region LowPivot
			int y = 0;
			int Lvls = 0;
			
			//Four Matching Lows
			if(Lows[BarsToGather][period]==Lows[BarsToGather][period+1] && Lows[BarsToGather][period]==Lows[BarsToGather][period+2] && Lows[BarsToGather][period]==Lows[BarsToGather][period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Lows[BarsToGather][period+3]<Lows[BarsToGather][period+3+y] : Lows[BarsToGather][period+3]<Lows[BarsToGather][period+3+y])
						Lvls++;
					if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period-y] : Lows[BarsToGather][period]<Lows[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Lows
			else if(Lows[BarsToGather][period]==Lows[BarsToGather][period+1] && Lows[BarsToGather][period]==Lows[BarsToGather][period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Lows[BarsToGather][period+2]<Lows[BarsToGather][period+2+y] : Lows[BarsToGather][period+2]<Lows[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period-y] : Lows[BarsToGather][period]<Lows[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Lows
			else if(Lows[BarsToGather][period]==Lows[BarsToGather][period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Lows[BarsToGather][period+1]<Lows[BarsToGather][period+1+y] : Lows[BarsToGather][period+1]<Lows[BarsToGather][period+1+y])
						Lvls++;
					if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period-y] : Lows[BarsToGather][period]<Lows[BarsToGather][period-y])
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
					if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period+y] : Lows[BarsToGather][period]<Lows[BarsToGather][period+y])
						Lvls++;
					if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period-y] : Lows[BarsToGather][period]<Lows[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Lows - First and Last Matching - Middle 2 are lower
				if(Lows[BarsToGather][period]<=Lows[BarsToGather][period+1] && Lows[BarsToGather][period]<=Lows[BarsToGather][period+2] && Lows[BarsToGather][period]==Lows[BarsToGather][period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Lows[BarsToGather][period+3]<Lows[BarsToGather][period+3+y] : Lows[BarsToGather][period+3]<Lows[BarsToGather][period+3+y])
							Lvls++;
						if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period-y] : Lows[BarsToGather][period]<Lows[BarsToGather][period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Lows - Middle is lower than two outside
				if(Lows[BarsToGather][period]<=Lows[BarsToGather][period+1] && Lows[BarsToGather][period]==Lows[BarsToGather][period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Lows[BarsToGather][period+2]<Lows[BarsToGather][period+2+y] : Lows[BarsToGather][period+2]<Lows[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Lows[BarsToGather][period]<Lows[BarsToGather][period-y] : Lows[BarsToGather][period]<Lows[BarsToGather][period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				return(Lows[BarsToGather][period]);
			}
			return(-1.0);
			#endregion
		}
//===================================================================
		private double isOpenLowPivot(int period, Brush color)
		{
			#region OpenLowPivot
			int y = 0;
			int Lvls = 0;
			
			//Four Matching Opens
			if(Opens[BarsToGather][period]==Opens[BarsToGather][period+1] && Opens[BarsToGather][period]==Opens[BarsToGather][period+2] && Opens[BarsToGather][period]==Opens[BarsToGather][period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Opens[BarsToGather][period+3]<Opens[BarsToGather][period+3+y] : Opens[BarsToGather][period+3]<Opens[BarsToGather][period+3+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period-y] : Opens[BarsToGather][period]<Opens[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Opens
			else if(Opens[BarsToGather][period]==Opens[BarsToGather][period+1] && Opens[BarsToGather][period]==Opens[BarsToGather][period+2])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Opens[BarsToGather][period+2]<Opens[BarsToGather][period+2+y] : Opens[BarsToGather][period+2]<Opens[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period-y] : Opens[BarsToGather][period]<Opens[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Opens
			else if(Opens[BarsToGather][period]==Opens[BarsToGather][period+1])
			{
				y=1;
				while(y<=period)
				{
					if(y!=period ? Opens[BarsToGather][period+1]<Opens[BarsToGather][period+1+y] : Opens[BarsToGather][period+1]<Opens[BarsToGather][period+1+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period-y] : Opens[BarsToGather][period]<Opens[BarsToGather][period-y])
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
					if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period+y] : Opens[BarsToGather][period]<Opens[BarsToGather][period+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period-y] : Opens[BarsToGather][period]<Opens[BarsToGather][period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if(Lvls<period*2)
			{
				Lvls=0;
				//Four Opens - First and Last Matching - Middle 2 are lower
				if(Opens[BarsToGather][period]<=Opens[BarsToGather][period+1] && Opens[BarsToGather][period]<=Opens[BarsToGather][period+2] && Opens[BarsToGather][period]==Opens[BarsToGather][period+3])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Opens[BarsToGather][period+3]<Opens[BarsToGather][period+3+y] : Opens[BarsToGather][period+3]<Opens[BarsToGather][period+3+y])
							Lvls++;
						if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period-y] : Opens[BarsToGather][period]<Opens[BarsToGather][period-y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls<period*2)
			{
				Lvls=0;
				//Three Opens - Middle is lower than two outside
				if(Opens[BarsToGather][period]<=Opens[BarsToGather][period+1] && Opens[BarsToGather][period]==Opens[BarsToGather][period+2])
				{
					y=1;
					while(y<=period)
					{
						if(y!=period ? Opens[BarsToGather][period+2]<Opens[BarsToGather][period+2+y] : Opens[BarsToGather][period+2]<Opens[BarsToGather][period+2+y])
						Lvls++;
					if(y!=period ? Opens[BarsToGather][period]<Opens[BarsToGather][period-y] : Opens[BarsToGather][period]<Opens[BarsToGather][period-y])
						Lvls++;
					y++;
					}
				}
			}
			if(Lvls>=period*2)
			{
				return(Opens[BarsToGather][period]);
			}
			return(-1.0);
			#endregion
		}
//============================================================================		
		private void AddFractal(int TheBar)
		{
//			if(TheBar<0) Print("Added Low pivot at "+Times[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(TheBar)].ToString()+"  "+Lows[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(TheBar)]);
//			else Print("Added High pivot at "+Times[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(TheBar)].ToString()+"  "+Highs[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(TheBar)]);
			Flocs.Add(TheBar);
		}
//====================================================================
		private List<Point> DetermineWeight(int OldestBarAbs_Secondary, int RBA_Secondary)
		{	
			#region DetermineWeight
int line = 948;
try{
			Fweight.Clear();
			FlocsStartElement = int.MinValue;
//if(Debug) Print("***********************************************************************************************************");
//if(Debug) Print("In DetermineWeight:  Flocs.Count is: "+Flocs.Count+"  CurrentBars[BarsToGather]: "+CurrentBars[BarsToGather]+"  BarsArray[BarsToGather].Count: "+BarsArray[BarsToGather].Count);
			for(int Fdex=0; Fdex<Flocs.Count; Fdex++)
			{	
line=956;
				int absbar = Math.Abs(Flocs[Fdex]);
line=958;
//if(Debug) Print("AbsBarPoint: "+absbar+"  compared with Start/End: "+OldestBarAbs_Secondary+" EndBar: "+RBA_Secondary);
line=960;
				if(absbar<OldestBarAbs_Secondary || absbar>RBA_Secondary) continue;
line=962;
				Point newpoint = new Point(Flocs[Fdex], 0); 
line=964;
//if(Debug) Print("      Adding Fweight point: "+newpoint.X.ToString()+"  time: "+Times[BarsToGather][CurrentBars[BarsToGather]-absbar].ToString());
line=966;
				Fweight.Add(newpoint);
				double P=0.0;
				int PivotBar = (int) CurrentBars[BarsToGather]-absbar;
				int PivotType = 0;
				if(Flocs[Fdex] < 0) 
				{	PivotType = -1;
line=973;
					if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) P = Lows[BarsToGather].GetValueAt(absbar);
					else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes)  P = Closes[BarsToGather].GetValueAt(absbar);
					else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Opens)   P = Opens[BarsToGather].GetValueAt(absbar);
				} else 
				{	PivotType = 1;
line=979;
					if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) P = Highs[BarsToGather].GetValueAt(absbar);
					else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes)  P = Closes[BarsToGather].GetValueAt(absbar);
					else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Opens)   P = Opens[BarsToGather].GetValueAt(absbar);
				}
line=984;
				int offset = 0;
				int weight = 0;
//bool watch = absbar == 5939;
//if(watch) Print("watched bar PivotType: "+PivotType);
				while (PivotBar-offset > 0 && CurrentBars[BarsToGather]-(PivotBar-offset) < RBA_Secondary && CurrentBars[BarsToGather]-(PivotBar+offset) > 0)
				{
line=991;
					if(PivotType > 0) //Count bars until next bar exceeds High[PivotBar +/- offset]
					{
line=994;
						offset++;
						if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) {
							if(Highs[BarsToGather].GetValueAt(absbar+offset) > P || Highs[BarsToGather].GetValueAt(absbar-offset) > P) break;
							else weight++;
						}
						else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes) {
							if(Closes[BarsToGather].GetValueAt(absbar+offset) > P || Closes[BarsToGather].GetValueAt(absbar-offset) > P) break;
							else weight++;
						}
						else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Opens) {
							if(Opens[BarsToGather].GetValueAt(absbar+offset) > P || Opens[BarsToGather].GetValueAt(absbar-offset) > P) break;
							else weight++;
						}
					} else {
line=1009;
						offset++;
						if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) {
							if(Lows[BarsToGather].GetValueAt(absbar+offset) < P || Lows[BarsToGather].GetValueAt(absbar-offset) < P) break;
							else weight++;
//if(watch) Print(Times[BarsToGather][PivotBar-offset].ToString()+"  Bar "+PivotBar+"+ new weight is "+weight);
						}
						else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes) {
							if(Closes[BarsToGather].GetValueAt(absbar+offset) < P || Closes[BarsToGather].GetValueAt(absbar-offset) < P) break;
							else weight++;
						}
						else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Opens) {
							if(Opens[BarsToGather].GetValueAt(absbar+offset) < P || Opens[BarsToGather].GetValueAt(absbar-offset) < P) break;
							else weight++;
						}
					}
				}
line=1026;
				Fweight[Fweight.Count-1] = new Point(Flocs[Fdex], weight);
			}
line=1030;
			if(Fweight.Count>0) Fweight.Sort(ComparePointsByWeight);
//if(Debug) {
//	line=1034;
//	for(int k =0; k<Fweight.Count; k++)  Print("Fweight:   bar: "+Times[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(Fweight[k].X)].ToString()+"    wgt: "+Fweight[k].Y+"  Price: "+(Fweight[k].X>0?Highs[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(Fweight[k].X)] : Lows[BarsToGather][CurrentBars[BarsToGather]-(int)Math.Abs(Fweight[k].X)]).ToString());
//}
line=1037;
			//Fweight.Reverse();
}catch(Exception DWerror){Print(string.Concat(line.ToString(),": ",Name,".DetermineWeight error: ",DWerror.ToString()));}
			return Fweight;
			#endregion
		}
//====================================================================
		private static int ComparePointsByPrice(Point a, Point b){
			try{
			if(a==null) {
				if(b==null) return 0; //0 means the two points are equal, both null
				else        return -1; //a is null and b is not null, b is "greater"
			} else {
				if(b==null) return 1; //b is null, a isn't, therefore a is "greater"
				else 		return a.X.CompareTo(b.X);
			}
			}catch(Exception err){return 0;};
		}
//====================================================================
		private static int ComparePointsByWeight(Point a, Point b){ //Larger numbers at 0 element, smaller numbers at last element
			try{
			if(a==null) {
				if(b==null) return 0; //0 means the two points are equal, both null
				else        return -1; //a is null and b is not null, b is "greater"
			} else {
				if(b==null) return 1; //b is null, a isn't, therefore a is "greater"
				else 		return -a.Y.CompareTo(b.Y);
			}
			}catch(Exception err){return 0;};
		}
//====================================================================
		private List<Point> SortHeavyToLight(List<Point> Fweight)
		{	
			//Fweight list is a subset of Flocs, it contains only the pivots that occurred between Oldest and Rightmost abs bars
			if(Fweight.Count==0) return (Fweight);
			Fweight.Sort(ComparePointsByWeight);
			if(Fweight[0].Y < Fweight[Fweight.Count-1].Y) Flocs.Reverse();
			//now, the "0"element in Flocs is bigger than the nth element
			return Fweight;
		}
//====================================================================
		private List<Point> GenerateRankHisto(List<Point> Fweight)
		{
			#region GenerateRankHisto
			int hdex = 0;
			int i = 1;
			hdex = (int) Math.Round(Fweight.Count * percentile ,0);
			hdex = Math.Max(1,hdex);
			var outmsg = $"FPC: {hdex.ToString()},{(hdex>1?"-levels":"-level")}, found";
			double WeightCategorySize = hdex/5.0;//divide all lines among 5 weight categories
			double P=0.0;
			int AdjLineWidth = 1;
			i = 0;
			MinLevel = float.MaxValue;
			MaxLevel = float.MinValue;
			Levels.Clear();
			List<double> AddedPrices = new List<double>();
//Print("Fweight.Count: "+Fweight.Count+"  i: "+i+"  hdex: "+hdex+"  percentile: "+percentile+"   RecalcTime: "+RecalculateTime.ToString());
			while(i<hdex)
			{
//				int k = CurrentBars[BarsToGather]-(int)Math.Abs(Fweight[i].X);
				int k = (int)Math.Abs(Fweight[i].X);

				if(pKeyPrices == FractalPivotConfluence_KeyPrices.HighsAndLows) {
					if(Fweight[i].X > 0)	P=Highs[BarsToGather].GetValueAt(k);
					else 					P=Lows[BarsToGather].GetValueAt(k);
				} else if(pKeyPrices == FractalPivotConfluence_KeyPrices.Closes) {
					if(Fweight[i].X > 0)	P=Closes[BarsToGather].GetValueAt(k);
					else 					P=Closes[BarsToGather].GetValueAt(k);
				} else if (pKeyPrices == FractalPivotConfluence_KeyPrices.Opens) {
					if(Fweight[i].X > 0)	P=Opens[BarsToGather].GetValueAt(k);
					else 					P=Opens[BarsToGather].GetValueAt(k);
				}
//Print("CurrentBar[BarsToGather]: "+CurrentBars[BarsToGather]+"  CurrentBar[0]: "+CurrentBars[0]);
				if(CurrentBars[BarsToGather] > 1 && !AddedPrices.Contains(P)) {
					float Category = (float) Math.Round(i/WeightCategorySize,0);
					MaxLevel = (float)Math.Max(MaxLevel, P);
					MinLevel = (float)Math.Min(MinLevel, P);
					var lvlelement = new Point((float)P, Category);
					Levels.Add(lvlelement);
					AddedPrices.Add(P);
					outmsg = $"{outmsg} {NL} {P.ToString()}";
//Print("FPC added a level: "+P.ToString()+"   Level count is: "+Levels.Count);
				}
				i++;
			}
			if(ShowMaxLevelsCount) Draw.TextFixed(this, "maxlevels",outmsg,TextPosition.TopRight);
			#endregion
			return Levels;
		}

//====================================================================
		private SharpDX.Direct2D1.Brush textBrushDX = null;
		private SharpDX.Direct2D1.Brush TintedAxisBrushDX = null;
		private SharpDX.Direct2D1.Brush TintedLineBrushDX = null;
		private SharpDX.Direct2D1.Brush lineBrushDX = null;
		private bool ShowMaxLevelsCount = false;

		public override void OnRenderTargetChanged()
		{
			if(textBrushDX!=null      && !textBrushDX.IsDisposed)      textBrushDX.Dispose();      textBrushDX   = null;
			if(RenderTarget!=null && ChartControl != null) 
				textBrushDX = ChartControl.Properties.AxisPen.Brush.ToDxBrush(RenderTarget);

			if(lineBrushDX!=null      && !lineBrushDX.IsDisposed)      lineBrushDX.Dispose();      lineBrushDX   = null;
			if(RenderTarget!=null) {
				lineBrushDX = lineColor.ToDxBrush(RenderTarget);
			}

			if(TintedLineBrushDX!=null      && !TintedLineBrushDX.IsDisposed)      TintedLineBrushDX.Dispose();      TintedLineBrushDX   = null;
			if(RenderTarget!=null) {
				TintedLineBrushDX = lineColor.ToDxBrush(RenderTarget);
				TintedLineBrushDX.Opacity = pMaxOpacity / 10f;
			}

			if(TintedAxisBrushDX!=null      && !TintedAxisBrushDX.IsDisposed)      TintedAxisBrushDX.Dispose();      TintedAxisBrushDX   = null;
			if(RenderTarget!=null && ChartControl != null) {
				TintedAxisBrushDX = ChartControl.Properties.AxisPen.Brush.ToDxBrush(RenderTarget);
				TintedAxisBrushDX.Opacity = pMaxOpacity / 10f;
			}
		}
		/// <summary>
		/// Called when the indicator is plotted.
		/// </summary>
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;
		double ChartMinPrice = chartScale.MinValue; double ChartMaxPrice = chartScale.MaxValue;
		int firstBarPainted = ChartBars.FromIndex;
		int lastBarPainted = ChartBars.ToIndex;
int line=1149;
		#region Plot
		if(ChartControl==null) return;
        var textFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
		var v1 = new SharpDX.Vector2(0,0);
		var v2 = new SharpDX.Vector2(0,0);
try{
		TimeSpan t = new TimeSpan(NinjaTrader.Core.Globals.Now.Ticks - TimeOfLaunch.Ticks);
		if(t.TotalSeconds < 10) 
			Draw.TextFixed(this, "maxbars",$"\t{NL}\t{NL}Effective MaxBars in FPC: {Math.Min(Bars.Count,maxBars).ToString()}{NL}{(BeginDateTimeInfoStr.Length>0?NL+BeginDateTimeInfoStr:string.Empty)} {(EndDateTimeInfoStr.Length>0?NL+EndDateTimeInfoStr:string.Empty)}",TextPosition.TopLeft);
		else {
			RemoveDrawObject("maxbars");
			RemoveDrawObject("maxlevels");
			ShowMaxLevelsCount = false;
		}
		int LeftPixelOfLines = 0;

		//RBA is RightBarAbsolute, LBA is LeftBarAbsolute
		try{
			if(ChartBars.ToIndex < 0) RBA_Chart = BarsArray[0].GetBar(Time[CurrentBars[0]-ChartBars.ToIndex]);
			else RBA_Chart = Math.Min(CurrentBars[0],ChartBars.ToIndex);
		}catch(Exception error){return;}

		if(BarAtBeginDateTime > firstBarPainted && BarAtBeginDateTime <= lastBarPainted && textBrushDX!=null && textBrushDX.IsValid(RenderTarget)) {
			var txtLayout	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, BeginDateTimeInfoStr+"This dashed line is the first bar in the input data", textFormat, (float)(ChartPanel.X + ChartPanel.W), 12f);
			v1.X = chartControl.GetXByBarIndex(ChartBars, BarAtBeginDateTime);
			v1.Y = chartScale.GetYByValue((ChartMaxPrice-ChartMinPrice)*0.05+ChartMinPrice);
			RenderTarget.DrawTextLayout(v1, txtLayout, textBrushDX);
		}
		if(BarAtEndDateTime>firstBarPainted && BarAtEndDateTime<=lastBarPainted && textBrushDX!=null && textBrushDX.IsValid(RenderTarget)) {
			var txtLayout	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, EndDateTimeInfoStr+"This dashed line is the last bar in the input data", textFormat, (float)(ChartPanel.X + ChartPanel.W), 12f);
			v1.X = chartControl.GetXByBarIndex(ChartBars, BarAtBeginDateTime);
			v1.Y = chartScale.GetYByValue((ChartMaxPrice-ChartMinPrice)*0.05+ChartMinPrice);
			RenderTarget.DrawTextLayout(v1, txtLayout, textBrushDX);
		}

		var TimeOfRightBar  = Times[0].GetValueAt(RBA_Chart);
		if(pEndDateTime>DateTime.MinValue && TimeOfRightBar>pEndDateTime) TimeOfRightBar = pEndDateTime;

		RBA_Secondary = Math.Max(0,BarsArray[BarsToGather].GetBar(TimeOfRightBar));
		OldestBarAbs_Secondary = Math.Max(0,RBA_Secondary - maxBars);
		var OldestDT = Times[BarsToGather].GetValueAt(OldestBarAbs_Secondary);
		OldestBarAbs_Chart = BarsArray[0].GetBar(OldestDT);

		bool RecalculateAllData  = false;
		if(pRecalculateAtStr.Length>0) {
			int ChartBarAtRecalculateTime = 0;
			if(RecalcType.CompareTo("TIME")==0){
				RecalculateTime = new DateTime(TimeOfRightBar.Year, TimeOfRightBar.Month, TimeOfRightBar.Day, RecalculateTime.Hour, RecalculateTime.Minute, RecalculateTime.Second);
				while(TimeOfRightBar.Ticks < RecalculateTime.Ticks) {
					RecalculateTime    = RecalculateTime.AddDays(-1);
				}
				if(RecalculateTime < pBeginDateTime) return;//don't draw any levels if the left-edge of levels is PRIOR to the beginning of the level calc
				int temp = BarAtRecalculateTime;
				BarAtRecalculateTime = BarsArray[BarsToGather].GetBar(RecalculateTime);
				RecalculateAllData = (temp != BarAtRecalculateTime);
				ChartBarAtRecalculateTime = BarsArray[0].GetBar(RecalculateTime);
			}else if(RecalcType.CompareTo("WEEK")==0){
				RecalculateTime = new DateTime(TimeOfRightBar.Year, TimeOfRightBar.Month, TimeOfRightBar.Day, 0, 0, 0);
				var done = false;
				var dowNow = RecalculateTime.DayOfWeek;
				while(!done){
					if(dowNow > DayOfWeek.Monday && RecalculateTime.DayOfWeek <= DayOfWeek.Monday) break;
					RecalculateTime.AddDays(-1);
					dowNow = RecalculateTime.DayOfWeek;
				}
				ChartBarAtRecalculateTime = BarsArray[0].GetBar(RecalculateTime);
			}

			LeftPixelOfLines     = chartControl.GetXByBarIndex(ChartBars, ChartBarAtRecalculateTime);
		} else {
			if(pEndDateTime > DateTime.MinValue) {
				if(pEndDateTime < TimeOfRightBar) RecalculateTime = pEndDateTime;
				else RecalculateTime = TimeOfRightBar;
			}
			if(pBeginDateTime>DateTime.MinValue) {
				OldestBarAbs_Secondary  = BarsArray[BarsToGather].GetBar(pBeginDateTime);
				OldestDT = pBeginDateTime;
			} else {
				OldestBarAbs_Secondary  = Math.Max(0,RBA_Secondary - maxBars);
				OldestDT = Times[BarsToGather].GetValueAt(OldestBarAbs_Secondary);
			}
			RecalculateAllData = true;
			LeftPixelOfLines   = 0;
//if(Debug) Print("1134 DrawingLine: "+lineColor.ToString()+"  LeftPixel: "+LeftPixelOfLines+"  ChartPanel.H: "+ChartPanel.H);
		}
		v1.X = LeftPixelOfLines;
		v1.Y = 0;
		v2.X = v1.X;
		v2.Y = ChartPanel.H;
		if(/*BarsInProgress == BarsToGather &&*/ (RecalculateAllData || Levels == null || Levels.Count == 0)) {
			Levels = GetLevels(RecalculateTime);
			if(Levels == null) return;
		}
		if(lineBrushDX!=null && lineBrushDX.IsValid(RenderTarget)){
			v1.X = v2.X = Math.Max(2,LeftPixelOfLines);
			v1.Y = chartScale.GetYByValue(MaxLevel);//MaxLevel calculated in GenerateRankHisto
			v2.Y = chartScale.GetYByValue(MinLevel);//MinLevel calculated in GenerateRankHisto
			RenderTarget.DrawLine(v1, v2, lineBrushDX, (float)Math.Max(1,ChartControl.BarWidth));
		}
		var initialAxisOpacity = TintedAxisBrushDX.Opacity;
		var initialLineOpacity = TintedLineBrushDX.Opacity;
		var opacity = 1f;
		for(int i = 0; i<Levels.Count && TintedAxisBrushDX != null && TintedAxisBrushDX.IsValid(RenderTarget) && TintedLineBrushDX != null && TintedLineBrushDX.IsValid(RenderTarget); i++) {
			TintedAxisBrushDX.Opacity = initialAxisOpacity;
			TintedLineBrushDX.Opacity = initialLineOpacity;
			if(Levels[i].X > ChartMinPrice && Levels[i].X < ChartMaxPrice) {
				float AdjLineWidth = 1;
				if(pLineMode == FractalPivotConfluence_LineMode.Width || pLineMode == FractalPivotConfluence_LineMode.Both) {
					if(Levels[i].Y <= 1) AdjLineWidth = 1+4;
					else if(Levels[i].Y == 2) AdjLineWidth = 1+3;
					else if(Levels[i].Y == 3) AdjLineWidth = 1+2;
					else if(Levels[i].Y == 4) AdjLineWidth = 1+1;
					else AdjLineWidth = 1;
				}
				if (pLineMode == FractalPivotConfluence_LineMode.Shading || pLineMode == FractalPivotConfluence_LineMode.Both) {
					double ThisOpacity = 255;
					if(Levels[i].Y == 1) ThisOpacity = 255;
					else if(Levels[i].Y == 2) ThisOpacity = 0.8 * 255;
					else if(Levels[i].Y == 3) ThisOpacity = 0.6 * 255;
					else if(Levels[i].Y == 4) ThisOpacity = 0.4 * 255;
					else ThisOpacity = 0.2 * 255;
					int alpha = Math.Min(255,(int)Math.Round(ThisOpacity,0));
					alpha = Math.Max(0, alpha);
					if(TintedLineBrushDX != null && TintedLineBrushDX.IsValid(RenderTarget)){
						if(Levels[i].X == MaxLevel || Levels[i].X == MinLevel)
							TintedLineBrushDX.Opacity = 1f;
						else
							TintedLineBrushDX.Opacity = alpha/255f;
					}
				}
				v1.X = LeftPixelOfLines;
				v1.Y = v2.Y = (float)(chartScale.GetYByValue( Levels[i].X));
				v2.X = ChartPanel.X+ChartPanel.W - KeySwitch*ChartPanel.W/5;
				if(TintedAxisBrushDX != null && TintedAxisBrushDX.IsValid(RenderTarget)){
					if(Levels[i].X == MaxLevel || Levels[i].X == MinLevel)
						RenderTarget.DrawLine(v1,v2, TintedAxisBrushDX, AdjLineWidth);
					else
						RenderTarget.DrawLine(v1,v2, TintedAxisBrushDX, AdjLineWidth);
				}
				if(pShowPrices && textBrushDX != null && textBrushDX.IsValid(RenderTarget)) {
					string pricestr = Instrument.MasterInstrument.FormatPrice(Levels[i].X);
					var txtLayout	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, pricestr, textFormat, (float)(ChartPanel.X + ChartPanel.W), 12f);
					v1.X = Math.Max(3, LeftPixelOfLines - txtLayout.Metrics.Width - 3);
					v1.Y = v1.Y+(float)AdjLineWidth/2.0f;
					RenderTarget.DrawTextLayout(v1, txtLayout, textBrushDX);
				}
			}
		}
}catch(Exception exx){Print(line+": "+Name+" "+VersionNumber+" Plot error: "+Instrument.FullName+" "+exx.ToString());}
#endregion
		}
//==================================================================
	#region MakeString
	private static string MakeString(object[] s, string Separator){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		return stb.ToString();
	}
	private void PrintMakeString(object[] s, string Separator){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		Print(stb.ToString());
	}
	private void PrintMakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		Print(stb.ToString());
	}
	private void PrintMakeString(string filepath, object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		System.IO.File.AppendAllText(filepath,stb.ToString());
	}
	private void PrintMakeString(string filepath, object[] s, string Separator) {
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		System.IO.File.AppendAllText(filepath,stb.ToString());
	}
	private static string MakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		return stb.ToString();
	}
		#endregion

//========================================================================
		public override string ToString()
		{
			return String.Format("{0}({1}, {2}, {3})", "FPC "+VersionNumber, significance, percentile, pRecalculateAtStr);
		}
		#region Properties
//===========================================================================================================

		[Description("Minimum significance or strength of Support/Resistance levels")]
		[Display(Order=10, Name = "Minimum Significance",  GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public int MinimumSignificance
		{
			get { return significance; }
			set { significance = Math.Max(1,value); }
		}

		private int pMaxBars = 5000;
		[Description("Max # of bars to analyze (on a rolling basis), enter '0' here to use the BeginDateTime instead of MaxBars")]
		[Display(Order=20, Name = "Max bars",  GroupName = "Parameters",ResourceType = typeof(Custom.Resource))]
		public int MaxBars
		{
			get { return pMaxBars; }
			set { pMaxBars = Math.Max(0,value); }
		}
		private DateTime pBeginDateTime = DateTime.MinValue;
		[Description("Instead of using MaxBars to calculate the start time, you can specify the start time explicitly here.")]
		[Display(Order=30, Name = "Begin DateTime",  GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public DateTime BeginDateTime
		{
			get { return pBeginDateTime; }
			set { pBeginDateTime = value; }
		}
		private DateTime pEndDateTime = DateTime.MinValue;
		[Description("Instead of using the current time as the end (of input data) time, you can specify the end time explicitly here.")]
		[Display(Order=40, Name = "End DateTime",  GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public DateTime EndDateTime
		{
			get { return pEndDateTime; }
			set { pEndDateTime = value; }
		}

		/// <summary>
		/// If set to true, then this indicator will ignore the secondary datafeed and use the primary (chart bars) data.  This is the recommended setting when calling the FractalPivotConfluence from within a strategy or indicator
		/// </summary>
		private bool pUseChartBars = false;
		[Description("Use chart bars for input...ignore secondary datafeed")]
		[Display(Name = "Use Chart Bars?",  GroupName = "Datafeed Config", ResourceType = typeof(Custom.Resource))]
		public bool UseChartBars
		{
			get { return pUseChartBars; }
			set { pUseChartBars = value; }
		}
		private FractalPivotConfluence_BaseTimeframes pBaseBarsPeriodType = FractalPivotConfluence_BaseTimeframes.Minute;
		[Description("Base Period Type to be used in calculation of the FPC")]
		[Category("Datafeed Config")]
		public FractalPivotConfluence_BaseTimeframes BaseBarsPeriodType
		{
			get { return pBaseBarsPeriodType; }
			set { pBaseBarsPeriodType = value; 
				switch (pBaseBarsPeriodType) {
					case FractalPivotConfluence_BaseTimeframes.Tick:   AcceptableBaseBarsPeriodType = BarsPeriodType.Tick;   break;
					case FractalPivotConfluence_BaseTimeframes.Range:  AcceptableBaseBarsPeriodType = BarsPeriodType.Range;  break;
					case FractalPivotConfluence_BaseTimeframes.Volume: AcceptableBaseBarsPeriodType = BarsPeriodType.Volume; break;
					case FractalPivotConfluence_BaseTimeframes.Second: AcceptableBaseBarsPeriodType = BarsPeriodType.Second; break;
					case FractalPivotConfluence_BaseTimeframes.Minute: AcceptableBaseBarsPeriodType = BarsPeriodType.Minute; break;
					case FractalPivotConfluence_BaseTimeframes.Day:    AcceptableBaseBarsPeriodType = BarsPeriodType.Day;    break;
					case FractalPivotConfluence_BaseTimeframes.Week:   AcceptableBaseBarsPeriodType = BarsPeriodType.Week;   break;
					case FractalPivotConfluence_BaseTimeframes.Month:  AcceptableBaseBarsPeriodType = BarsPeriodType.Month;  break;
					case FractalPivotConfluence_BaseTimeframes.Year:   AcceptableBaseBarsPeriodType = BarsPeriodType.Year;   break;
				}
			}
		}
		[Description("Period value of base instrument, must be a whole number")]
		[Display(Name = "Base Period Value",  GroupName = "Datafeed Config", ResourceType = typeof(Custom.Resource))]
		public int Period_Value
		{
			get { return period_value; }
			set { period_value = Math.Max(1,value); }
		}

		private bool pShowPrices = false;
		[Description("Show prices of each level")]
		[Display(Name = "Show Prices?",  GroupName = "Custom Visual", ResourceType = typeof(Custom.Resource))]
		public bool ShowPrices
		{
			get { return pShowPrices; }
			set { pShowPrices = value; }
		}

		private int pMaxOpacity = 3;
		[Description("Maximum opacity of horizontal lines, 0=transparent, 10=opaque")]
		[Display(Name = "Max Opacity",  GroupName = "Custom Visual", ResourceType = typeof(Custom.Resource))]
		public int MaxOpacity
		{
			get { return pMaxOpacity; }
			set { pMaxOpacity = Math.Max(0,Math.Min(10,value)); }
		}

		[Description("Fractal percentile (0.1 = 10% of the strongest pivots to be displayed)")]
		[Display(Order=50, Name = "Percentile",  GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public double Percentile
		{
			get { return percentile; }
			set { percentile = Math.Max(0.0001,value); }
		}


		[Description("Determine when, each day, to recalculate the Fractal levels (leave blank to recalculate continuously)")]
		[Display(Order=60, Name = "Recalculate at",  GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
    	public string RecalculateAtStr
    	{
        	get { return pRecalculateAtStr; }
        	set { pRecalculateAtStr = value; }
    	}

		[Description("Base color of support/resistance lines"), XmlIgnore]
        [Category("Custom Visual")]
        public Brush LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }
				[Browsable(false)]
	    		public string LineColorSerialize { get { return Serialize.BrushToString(LineColor); } set { LineColor = Serialize.StringToBrush(value); }}

		private FractalPivotConfluence_LineMode pLineMode = FractalPivotConfluence_LineMode.Width;
		[Description("Type of line mode to communicate support/resistance strength?")]
        [Category("Custom Visual")]
        public FractalPivotConfluence_LineMode LineMode
        {
            get { return pLineMode; }
            set { pLineMode = value; }
        }
		private FractalPivotConfluence_KeyPrices pKeyPrices = FractalPivotConfluence_KeyPrices.HighsAndLows;
		[Description("What to price basis to use?")]
		[Display(Order=70, Name = "Key Price",  GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
        public FractalPivotConfluence_KeyPrices KeyPrices
        {
            get { return pKeyPrices; }
            set { pKeyPrices = value; }
        }

		#endregion
	}
}
public enum FractalPivotConfluence_LineMode {
	Width,
	Shading,
	Both,
	None
}
public enum FractalPivotConfluence_KeyPrices {
	Closes, Opens, HighsAndLows
}
public enum FractalPivotConfluence_BaseTimeframes {
	Tick,Range,Volume,Second,Minute,Day,Week,Month,Year
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FractalPivotConfluence[] cacheFractalPivotConfluence;
		public FractalPivotConfluence FractalPivotConfluence()
		{
			return FractalPivotConfluence(Input);
		}

		public FractalPivotConfluence FractalPivotConfluence(ISeries<double> input)
		{
			if (cacheFractalPivotConfluence != null)
				for (int idx = 0; idx < cacheFractalPivotConfluence.Length; idx++)
					if (cacheFractalPivotConfluence[idx] != null &&  cacheFractalPivotConfluence[idx].EqualsInput(input))
						return cacheFractalPivotConfluence[idx];
			return CacheIndicator<FractalPivotConfluence>(new FractalPivotConfluence(), input, ref cacheFractalPivotConfluence);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FractalPivotConfluence FractalPivotConfluence()
		{
			return indicator.FractalPivotConfluence(Input);
		}

		public Indicators.FractalPivotConfluence FractalPivotConfluence(ISeries<double> input )
		{
			return indicator.FractalPivotConfluence(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FractalPivotConfluence FractalPivotConfluence()
		{
			return indicator.FractalPivotConfluence(Input);
		}

		public Indicators.FractalPivotConfluence FractalPivotConfluence(ISeries<double> input )
		{
			return indicator.FractalPivotConfluence(input);
		}
	}
}

#endregion
