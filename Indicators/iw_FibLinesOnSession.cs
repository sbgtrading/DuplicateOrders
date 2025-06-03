// 
// Copyright (C) 2009, SBG Trading Corp.    www.affordableindicators.com
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//

#region Using declarations
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;

namespace NinjaTrader.NinjaScript.Indicators
{

    [Description("")]
    public class FibLinesOnSession : Indicator
    {
		private class SessionInfo {
			public DateTime StartTime;
			public DateTime StopTime;
			public double   SessionHigh;
			public double   SessionLow;
			public SessionInfo(DateTime starttime, DateTime stoptime, /*int startabar, int stopabar, */double h, double l){this.StartTime=starttime; StopTime=stoptime; /*this.StartABar=startabar; this.StopABar=stopabar; */this.SessionHigh=h; this.SessionLow=l;}
		}
		private SortedDictionary<DateTime,SessionInfo> Sessions = new SortedDictionary<DateTime,SessionInfo>();
		private DateTime WorkingDT = DateTime.MinValue;
		private DateTime SessionDT = DateTime.MinValue;
		private bool SpansMidnight = false;
		private int RMB = 0;

		private SortedDictionary<int,double> PlotIdToPct = new SortedDictionary<int,double>();
		private int starthour = 9;
		private int startminute = 30;
		private int daysago=0;
		private double pSessionLengthinhours=1.25;
		private bool RunInit=true;
		private bool ResetHiLo=true;

		private DateTime StartTime, EndTime;
//		private string FS="";
		private DateTime T1 = new DateTime(2200,4,24,0,0,0);
		private double price = double.MinValue;
		private double priorprice = double.MinValue;
		private int AlertBar = 0;
		private DateTime SpecificDateDT = DateTime.MinValue;
		private Brush IBBrush = null;
        private TextFormat txtFormat = null;

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
if(State==null)Print("FibLinesOnSession state == NULL");
else Print(State.ToString());

			if (State == State.SetDefaults)
			{
				var IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", "AIFibLinesOnSession", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				Name = "iw Fib Lines On Session v8.1";
				#region Plot definitions
				AddPlot(new Stroke(Brushes.Blue,1), PlotStyle.Dot,		 	"High");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Dot, 			"Low");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot1");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot2");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot3");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot4");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot5");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot6");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot7");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot8");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot9");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot10");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot11");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot12");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot13");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot14");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot15");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot16");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot17");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot18");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot19");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot20");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot21");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot22");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot23");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot24");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot25");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot26");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot27");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot28");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot29");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot30");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot31");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot32");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot33");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot34");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot35");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot36");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot37");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot38");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot39");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot40");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot41");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot42");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot43");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot44");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot45");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot46");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot47");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot48");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot49");
				AddPlot(new Stroke(Brushes.DarkGreen,1), PlotStyle.Hash, 		"Plot50");
				#endregion
				Calculate=Calculate.OnPriceChange;
				IsAutoScale=false;
				IsOverlay=true;
				RunInit 			= true;
			}
			
			if (State == State.Configure)
			{
				PlotIdToPct.Clear();
				#region Initialize plot
				int PlotId = 2;
				InitPlot(pFibPct1, ref PlotId, Plots.Length);
				InitPlot(pFibPct2, ref PlotId, Plots.Length);
				InitPlot(pFibPct3, ref PlotId, Plots.Length);
				InitPlot(pFibPct4, ref PlotId, Plots.Length);
				InitPlot(pFibPct5, ref PlotId, Plots.Length);
				InitPlot(pFibPct6, ref PlotId, Plots.Length);
				InitPlot(pFibPct7, ref PlotId, Plots.Length);
				InitPlot(pFibPct8, ref PlotId, Plots.Length);
				InitPlot(pFibPct9, ref PlotId, Plots.Length);
				InitPlot(pFibPct10, ref PlotId, Plots.Length);
				InitPlot(pFibPct11, ref PlotId, Plots.Length);
				InitPlot(pFibPct12, ref PlotId, Plots.Length);
				InitPlot(pFibPct13, ref PlotId, Plots.Length);
				InitPlot(pFibPct14, ref PlotId, Plots.Length);
				InitPlot(pFibPct15, ref PlotId, Plots.Length);
				InitPlot(pFibPct16, ref PlotId, Plots.Length);
				InitPlot(pFibPct17, ref PlotId, Plots.Length);
				InitPlot(pFibPct18, ref PlotId, Plots.Length);
				InitPlot(pFibPct19, ref PlotId, Plots.Length);
				InitPlot(pFibPct20, ref PlotId, Plots.Length);
				InitPlot(pFibPct21, ref PlotId, Plots.Length);
				InitPlot(pFibPct22, ref PlotId, Plots.Length);
				InitPlot(pFibPct23, ref PlotId, Plots.Length);
				InitPlot(pFibPct24, ref PlotId, Plots.Length);
				InitPlot(pFibPct25, ref PlotId, Plots.Length);
				#endregion
	 		}
			if(State==State.DataLoaded){
				if(ChartControl!=null){
					if(pIB_bkg_Opacity>0){
						IBBrush = pIB_bkg_Brush.Clone();//new SolidColorBrush((byte)(Color.FromArgb),  pIB_bkg_Brush.R, pIB_bkg_Brush.G, pIB_bkg_Brush.B);
						IBBrush.Opacity = pIB_bkg_Opacity/10.0;
						IBBrush.Freeze();
					}
					txtFormat = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	ChartControl.Properties.LabelFont.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Bold,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	pFontSize);
				}
			}
		}

		private void InitPlot(double pct, ref int PlotId, int MaxPlotId){
			if(pct>0) {
				if(pct>=1.0) {
					if(PlotId<=MaxPlotId) PlotIdToPct[PlotId] = pct; 
					PlotId++;
					if(PlotId<=MaxPlotId) PlotIdToPct[PlotId] = -pct;
				} else 
					if(PlotId<=MaxPlotId) PlotIdToPct[PlotId] = pct; 
				PlotId++;
			}
		}
			#region Plots
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> SessionHigh {get { return Values[0]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> SessionLow {get { return Values[1]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot1 {get { return Values[2]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot2 {get { return Values[3]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot3 {get { return Values[4]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot4 {get { return Values[5]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot5 {get { return Values[6]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot6 {get { return Values[7]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot7 {get { return Values[8]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot8 {get { return Values[9]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot9 {get { return Values[10]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot10 {get { return Values[11]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot11 {get { return Values[12]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot12 {get { return Values[13]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot13 {get { return Values[14]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot14 {get { return Values[15]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot15 {get { return Values[16]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot16 {get { return Values[17]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot17 {get { return Values[18]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot18 {get { return Values[19]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot19 {get { return Values[20]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot20 {get { return Values[21]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot21 {get { return Values[22]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot22 {get { return Values[23]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot23 {get { return Values[24]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot24 {get { return Values[25]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot25 {get { return Values[26]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot26 {get { return Values[27]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot27 {get { return Values[28]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot28 {get { return Values[29]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot29 {get { return Values[30]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot30 {get { return Values[31]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot31 {get { return Values[32]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot32 {get { return Values[33]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot33 {get { return Values[34]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot34 {get { return Values[35]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot35 {get { return Values[36]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot36 {get { return Values[37]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot37 {get { return Values[38]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot38 {get { return Values[39]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot39 {get { return Values[40]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot40 {get { return Values[41]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot41 {get { return Values[42]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot42 {get { return Values[43]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot43 {get { return Values[44]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot44 {get { return Values[45]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot45 {get { return Values[46]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot46 {get { return Values[47]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot47 {get { return Values[48]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot48 {get { return Values[49]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot49 {get { return Values[50]; }}
			[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
			[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
			public Series<double> Plot50 {get { return Values[51]; }}
			#endregion

		/// <summary>
		/// Called on each bar update event (incoming tick)
		/// </summary>
		protected override void OnBarUpdate()
		{
			StartTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, pStartAt.Hours, pStartAt.Minutes, pStartAt.Seconds);
			if(RunInit)
			{
				RunInit = false;
				pSessionLengthinhours = pSessionLength.TotalHours;
				if(Time[0].Ticks > StartTime.Ticks) StartTime = StartTime.AddDays(1);
				EndTime = StartTime.AddHours(pSessionLengthinhours);
				if(StartTime.Day != EndTime.Day) SpansMidnight = true;

				for(int i = 2; i<Plots.Length; i++)
					if(PlotIdToPct.ContainsKey(i))  Plots[i].Name = (PlotIdToPct[i]).ToString("0%"); else Plots[i].Name="Plot "+(i-1).ToString();

				DateTime t = Time[CurrentBar];
				while(t<NinjaTrader.Core.Globals.Now.AddDays(10)){
					if(!Sessions.ContainsKey(t.Date)) {
						DateTime st = new DateTime(t.Year, t.Month, t.Day, pStartAt.Hours, pStartAt.Minutes, pStartAt.Seconds);
						if(SpecificDateDT!=DateTime.MinValue){
							if(t.Date.Date==SpecificDateDT) Sessions[t.Date] = new SessionInfo(st,st.AddHours(this.pSessionLengthinhours),/*0,0,*/0,0);
						} else
							Sessions[t.Date] = new SessionInfo(st,st.AddHours(this.pSessionLengthinhours),/*0,0,*/0,0);
					}
					t = t.AddDays(1);
				}
//				if(Bars.BarsPeriod.ToString()=="Daily" || Bars.BarsPeriod.ToString()=="Weekly" || Bars.BarsPeriod.ToString()=="Monthly")
//					Sessions.Clear();
			}
			if(CurrentBar<5) {
				return;
			}
//			if(IsFirstTickOfBar && (Bars.BarsPeriod.ToString()=="Daily" || Bars.BarsPeriod.ToString()=="Weekly" || Bars.BarsPeriod.ToString()=="Monthly")){
//				Print(399);
//				Sessions[Time[0].Date] = new SessionInfo(Time[0], Time[1], High[0], Low[0]);
//			}
			if(IsFirstTickOfBar) { //find the current WorkingDT
				if(SpecificDateDT!=DateTime.MinValue)
					WorkingDT = SpecificDateDT;
				else{
					var WorkingDTlist = new List<DateTime>(Sessions.Keys);
					WorkingDTlist.Sort();
					if(DateTime.Compare(WorkingDTlist[0],WorkingDTlist[1])>0) WorkingDTlist.Reverse();
					WorkingDT = WorkingDTlist[0];
					for(int i=0; i<WorkingDTlist.Count; i++){
						if(DateTime.Compare(Sessions[WorkingDTlist[i]].StartTime,Time[0])>0) break;//stop searching once the StartTime is beyond the current time
						if(DateTime.Compare(WorkingDT,WorkingDTlist[i])>0) continue;//get the largest date
						WorkingDT = WorkingDTlist[i];
					}
				}
			}
//if(WorkingDT.Date==T1.Date) Print(Time[0].ToString()+"   "+WorkingDT.ToShortDateString());
			if(Time[0].Ticks > Sessions[WorkingDT].StartTime.Ticks && Time[0].Ticks <= Sessions[WorkingDT].StopTime.Ticks) {
				if(High[0] > Sessions[WorkingDT].SessionHigh || Sessions[WorkingDT].SessionHigh==0) {
					Sessions[WorkingDT].SessionHigh = High[0];
					if(pShowDotsOnSessions) 
						Draw.Dot(this, WorkingDT.ToString()+"firsthigh",false,Sessions[WorkingDT].StartTime,Sessions[WorkingDT].SessionHigh, Brushes.Pink);
				}
				if(Low[0] < Sessions[WorkingDT].SessionLow  || Sessions[WorkingDT].SessionLow==0)  
					Sessions[WorkingDT].SessionLow  = Low[0];

//if(WorkingDT.Date==T1.Date) Print("    High/Low:  "+Sessions[WorkingDT].SessionHigh+" : "+Sessions[WorkingDT].SessionLow);
			} else
//if(WorkingDT.Date==T1.Date && WorkingDT.Hour==T1.Hour) Print("    outside of session:     High/Low:  "+Sessions[WorkingDT].SessionHigh+" : "+Sessions[WorkingDT].SessionLow);

			if(Time[0].Ticks > Sessions[WorkingDT].StopTime.Ticks) {
				if(pShowDotsOnSessions)
					Draw.Dot(this, WorkingDT.ToString()+"endlow",   false,Sessions[WorkingDT].StopTime, Sessions[WorkingDT].SessionLow, Brushes.Pink);
			}
			bool c1 = Time[0].Ticks > StartTime.Ticks;
			bool c2 = Time[1].Ticks <= StartTime.Ticks;
			if((c1 && c2)) {
				Sessions[WorkingDT].SessionHigh   = High[0];
				Sessions[WorkingDT].SessionLow    = Low[0];
//if(WorkingDT.Date==T1.Date) Print(Time[0].ToString()+ "  ******    "+WorkingDT.ToShortDateString()+" Session Start: "+Sessions[WorkingDT].StartTime.ToString()+"  End: "+Sessions[WorkingDT].StopTime.ToString()+"  High: "+High[0]+"  Low: "+Low[0]);
			}
			while(Time[0].Ticks>StartTime.Ticks) StartTime = StartTime.AddDays(1);


			if(pShowSessionTime) Draw.TextFixed(this, "fibonsession","Next session Start/End: "+Sessions[WorkingDT].StartTime.ToString()+" / "+Sessions[WorkingDT].StopTime.ToString(), TextPosition.TopLeft);
var DT = new DateTime(2016,6,7,0,0,0);
//if(Sessions[WorkingDT].StartTime!=DateTime.MinValue) Draw.VerticalLine(this, Sessions[WorkingDT].StartTime.ToString(),Sessions[WorkingDT].StartTime,Color.Blue,DashStyleHelper.Dash,3);
			Values[0][0] = (Sessions[WorkingDT].SessionHigh);
			Values[1][0] = (Sessions[WorkingDT].SessionLow);
			for(int i = 2; i<Plots.Length; i++) {
				double level = 0;
				if(PlotIdToPct.ContainsKey(i)){
					level = CalcLevel(i, PlotIdToPct[i], Sessions[WorkingDT].SessionHigh, Sessions[WorkingDT].SessionLow);
					if(level!=0) Values[i][0] = (level);
				}
//				if(Plots[20].Name=="StartBar") Values[20][0] = (AbsBarOfStartTime[0]);
//				if(Plots[21].Name=="EndBar") Values[21][0] = (AbsBarOfEndTime[0]);
				if(level!=0 && price!=double.MinValue && priorprice!=double.MinValue && AlertBar!=CurrentBar){
					string WavFile = CrossedAlertOnThisLevel(i-1, price, priorprice, Values[i][0]);
					if(WavFile.Length>0){
						AlertBar = CurrentBar;
						if(pPrintToAlertWindow){
							if(WavFile != "NO SOUND") //either ALERT MSG ONLY or it's a WAV file name
								Alert(CurrentBar.ToString(),Priority.High,"Price hit "+PlotIdToPct[i]+"-fib at "+Instrument.MasterInstrument.FormatPrice(Values[i][0]),AddSoundFolder(WavFile),0,Brushes.Red,Brushes.White);
						}
						else if(WavFile != "NO SOUND" && WavFile != "ALERT MSG ONLY") //only if it's a WAV file name
							PlaySound(AddSoundFolder(WavFile));
						if(pEmailAddress.Length>0) SendMail(pEmailAddress,Instrument.FullName+": Price hit "+PlotIdToPct[i]+"-fib at "+Instrument.MasterInstrument.FormatPrice(Values[i][0]),"This email was autogenerated by the NinjaTrader indicator named:  'iw Fib Lines On Session'");
					}
				}
			}
			priorprice = price;
			price = Close[0];
		}//end of OnBarUpdate
//====================================================================
		#region Supporting
		private string CrossedAlertOnThisLevel(int Id, double price, double priorprice, double Level){
			#region CrossedAlertOnThisLevel
			string WavFile = string.Empty;
			switch (Id){
				case 1:   WavFile = this.pWav01; break;
				case 2:   WavFile = this.pWav02; break;
				case 3:   WavFile = this.pWav03; break;
				case 4:   WavFile = this.pWav04; break;
				case 5:   WavFile = this.pWav05; break;
				case 6:   WavFile = this.pWav06; break;
				case 7:   WavFile = this.pWav07; break;
				case 8:   WavFile = this.pWav08; break;
				case 9:   WavFile = this.pWav09; break;
				case 10:  WavFile = this.pWav10; break;
				case 11:  WavFile = this.pWav11; break;
				case 12:  WavFile = this.pWav12; break;
				case 13:  WavFile = this.pWav13; break;
				case 14:  WavFile = this.pWav14; break;
				case 15:  WavFile = this.pWav15; break;
				case 16:  WavFile = this.pWav16; break;
				case 17:  WavFile = this.pWav17; break;
				case 18:  WavFile = this.pWav18; break;
				case 19:  WavFile = this.pWav19; break;
				case 20:  WavFile = this.pWav20; break;
				case 21:  WavFile = this.pWav21; break;
				case 22:  WavFile = this.pWav22; break;
				case 23:  WavFile = this.pWav23; break;
				case 24:  WavFile = this.pWav24; break;
				case 25:  WavFile = this.pWav25; break;
			}
			if(WavFile.Length==0) return string.Empty;
			if(State==State.Historical){
				if(Low[0]<=Level && High[0]>=Level)  return WavFile;
				if(Close[1]<Level && High[0]>=Level) return WavFile;
				if(Close[1]>Level && Low[0]<=Level)  return WavFile;
			} else {
				if(priorprice<=Level && price>Level) return WavFile;
				if(priorprice>=Level && price<Level) return WavFile;
			}
			return string.Empty;
			#endregion
		}
//====================================================================
		private double CalcLevel(int FibId, double FibPct, double H, double L) {
			double result = 0;
			if(FibPct==0) return 0;
			double range = H-L;
			if(FibPct<1.0 && FibPct>0) 
				result = L+range*FibPct;
//				Values[FibId][0] = (); 
			else if(FibPct>0) {
				result = L+range*FibPct;
//				Values[FibId][0] = (L+TheRange*(FibPct));
			}
			else if(FibPct<0) {
				result = H+range*FibPct;
//				Values[FibId][0] = (H+TheRange*(FibPct));
			}
			return result;
		}

//====================================================================
/*		private double ToDouble(string NumStr) {
			char[] ch = NumStr.ToCharArray();
			string result = string.Empty;
			for(int i = 0; i<NumStr.Length; i++) {
				if((int)ch[i] == 45 || (int)ch[i] == 46) result = result+ch[i];
				else if((int)ch[i] >= 48 && (int)ch[i] <= 57) result = result+ch[i];
			}
			return Math.Abs(double.Parse(result));
		}
*/
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds", wav);
		}
		#endregion
//====================================================================

		/// <summary>
		/// Called when the indicator is plotted.
		/// </summary>
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
int line = 560;
			if(ChartControl == null) return;
			if (!IsVisible) return;
			double ChartMinPrice = chartScale.MinValue; double ChartMaxPrice = chartScale.MaxValue;
			
			Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
			Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			int firstBarPainted = ChartBars.FromIndex;
			int lastBarPainted = ChartBars.ToIndex;
			var txtLayout = new TextLayout(Core.Globals.DirectWriteFactory, "", txtFormat, ChartPanel.W, txtFormat.FontSize);

try{

line = 570;
			if(pShowHistoricalLevels) base.OnRender(chartControl, chartScale);
			#region Plot
			if(Sessions.Count==0) return;

//if(RMB!=Math.Min(Bars.Count-1,lastBarPainted)-(CalculateOnBarClose?1:0))
			{
			RMB = Math.Min(Bars.Count-1,lastBarPainted)-(Calculate==Calculate.OnBarClose?1:0);
//			Print("keys[0]: "+keys[0].ToShortDateString()+"   Lastbar: "+BarsArray[0].GetTime(lastbar).ToString()+"   Sessions[lasbar]: "+Sessions[BarsArray[0].GetTime(lastbar).Date].StartTime.ToString());
				if(this.SpecificDateDT!=DateTime.MinValue)
					SessionDT = SpecificDateDT;
				else {
					var keys = new List<DateTime>(from d in Sessions.Keys 
						where DateTime.Compare(Sessions[d].StartTime, BarsArray[0].GetTime(RMB))<=0
						select d);
					if(pDaysAgo==0){
						SessionDT = keys.AsQueryable().Max();
					}
					else {
line=590;
						if(pDaysAgo > keys.Count-1) return;
						if(keys.Count>1) {
							if(keys[0].Ticks < keys[1].Ticks) keys.Reverse();
						}
						SessionDT = Time.GetValueAt(RMB);
						int i = pDaysAgo;
line=597;
						try{
							while(i<keys.Count && (Sessions[keys[i]].StartTime.Ticks > SessionDT.Ticks || Sessions[keys[i]].SessionHigh==Sessions[keys[i]].SessionLow)) {
								i++;
							}
						}catch(Exception err){Print("Error "+line+":  "+err.ToString());}
line=603;
						SessionDT = keys[i];
					}
				}
			}
line=616;
			string errS = string.Empty;
			bool SessionFound = Sessions.ContainsKey(SessionDT);
			if(!SessionFound) 
				errS = "Session starting at '"+SessionDT.ToShortDateString()+"' was not found 1";
//			if(Sessions[SessionDT].SessionHigh - Sessions[SessionDT].SessionLow < TickSize*10)
//				errS = "Session starting at '"+SessionDT.ToString()+"' appears to be very small - perhaps it was not";
			if(errS.Length==0 && Sessions[SessionDT].SessionHigh == Sessions[SessionDT].SessionLow) {
				errS = "Session starting at '"+SessionDT.ToShortDateString()+"' was not found 2";
line=624;
			}
			RemoveDrawObject("wd");
			if(errS.Length>0){
				Draw.TextFixed(this, "wd", errS, TextPosition.BottomLeft);
				return;
			}
//Draw.TextFixed(this, "wd",SessionDT.ToString()+Environment.NewLine+Sessions[SessionDT].SessionHigh+":"+Sessions[SessionDT].SessionLow,TextPosition.Center);
line=633;
			int StartTimeChangeBar = Bars.GetBar(Sessions[SessionDT].StartTime);
			int EndTimeChangeBar = Bars.GetBar(Sessions[SessionDT].StopTime);
line=637;
//Print("Session start bar:  "+StartTimeChangeBar+"   End bar: "+EndTimeChangeBar);
			int EndTimePixel = chartControl.GetXByBarIndex(ChartBars,EndTimeChangeBar);

			if(pIB_bkg_Opacity>0){
				float StartTimePixel = chartControl.GetXByBarIndex(ChartBars, StartTimeChangeBar);
				float IBhighPixel    = chartScale.GetYByValue( Sessions[SessionDT].SessionHigh);
				float IBlowPixel     = chartScale.GetYByValue( Sessions[SessionDT].SessionLow);
				RenderTarget.FillRectangle(new SharpDX.RectangleF(StartTimePixel, IBhighPixel, Math.Abs(StartTimePixel-EndTimePixel)/*Math.Abs(EndTimePixel-StartTimePixel)*/, Math.Abs(IBhighPixel-IBlowPixel)),IBBrush.ToDxBrush(RenderTarget));
			}

line=649;
			int abar = Math.Min(RMB,EndTimeChangeBar);
			SharpDX.Direct2D1.Brush brush = null;
			double line_price = 0;
			for(int i = 0; i<Plots.Length; i++) {
				if(/*Plots[i].Brush != Brushes.Transparent && */Plots[i].Name.Length>0 && Values[i].IsValidDataPointAt(abar)) {
					line_price = Values[i].GetValueAt(abar);
					float lineval1 = (float)(chartScale.GetYByValue(line_price));
//if(i<=1) Print(Plots[i].Name+": "+Values[i].Get(EndTimeChangeBar)+"   "+Time[CurrentBar-EndTimeChangeBar].ToString());
					float endpoint = ChartPanel.X + ChartPanel.W;
					//ThePen = (Pen)Plots[i].Pen.Clone();//new Pen(Plots[i].Pen.Color, Plots[i].Stroke.Width);
line=657;
					brush = Plots[i].Brush.ToDxBrush(RenderTarget);
					if(i<=1)
						RenderTarget.DrawLine(new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars,StartTimeChangeBar), lineval1), new SharpDX.Vector2(endpoint, lineval1), brush);
					else
						RenderTarget.DrawLine(new SharpDX.Vector2(EndTimePixel, lineval1), new SharpDX.Vector2(endpoint, lineval1), brush);
					#region DrawTextLayout
					txtLayout   = new TextLayout(Core.Globals.DirectWriteFactory, Plots[i].Name, txtFormat, (int)(ChartPanel.W*0.8), txtFormat.FontSize);
					var txtPosition = new SharpDX.Vector2(ChartPanel.X+ChartPanel.W - txtLayout.Metrics.Width - 5f, lineval1-txtLayout.Metrics.Height-Plots[i].Width);
//					RenderTarget.FillRectangle(new SharpDX.RectangleF(txtPosition.X-1f, txtPosition.Y-1f, txtLayout.Metrics.Width+2f, txtLayout.Metrics.Height+2f), bkgBrush);
					#endregion
					RenderTarget.DrawTextLayout(txtPosition, txtLayout, brush);
//					for(int fillabar = abar; fillabar<RMB; fillabar++)
//						Values[i][CurrentBars[0]-fillabar] = line_price;
				}
			}
			if(brush!=null) {brush.Dispose();brush=null;}

line=672;

			abar = firstBarPainted;
			while (abar < lastBarPainted)
			{
				if(pShowSessionTime) {
					if(daysago>0 && StartTimeChangeBar < EndTimeChangeBar && abar == StartTimeChangeBar)
						RenderTarget.DrawLine(new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars, StartTimeChangeBar), 0), new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars, StartTimeChangeBar), ChartPanel.H),Brushes.Green.ToDxBrush(RenderTarget));
					else if(daysago==0 && abar == StartTimeChangeBar)
						RenderTarget.DrawLine(new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars, StartTimeChangeBar), 0), new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars, StartTimeChangeBar), ChartPanel.H),Brushes.Green.ToDxBrush(RenderTarget));
					if(abar == EndTimeChangeBar){
						RenderTarget.DrawLine(
							new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars, EndTimeChangeBar), chartScale.GetYByValue(Values[0].GetValueAt(EndTimeChangeBar))),
							new SharpDX.Vector2(chartControl.GetXByBarIndex(ChartBars, EndTimeChangeBar), chartScale.GetYByValue(Values[1].GetValueAt(EndTimeChangeBar))),Brushes.Red.ToDxBrush(RenderTarget));
					}
				}
				abar++;
			}
}catch(Exception exx){Print("Error: line "+line+": "+exx.ToString());}
			#endregion
			if(txtLayout!=null){
				txtLayout.Dispose();
				txtLayout = null;
			}
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
				list.Add("NO SOUND");
				list.Add("ALERT MSG ONLY");
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
		public static string ValidateEmail(string str)
		{
			if(str.Length==0) return string.Empty;
			if(System.Text.RegularExpressions.Regex.IsMatch(str, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
				return str;
			else
				return "invalid";
		}

//		private bool HideOutput = false;
		#region Properties
		private int pIB_bkg_Opacity = 4;
		[Description("Opacity of InitialBalance session background rectangle (0=transparent, 10=opaque)")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public int IBbackground_Opacity
		{
			get { return pIB_bkg_Opacity; }
			set { pIB_bkg_Opacity = Math.Max(0,Math.Min(10,value)); }
		}
		private Brush pIB_bkg_Brush = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of InitialBalance session background rectangle")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public Brush IBbackground_Brush
		{	get { return pIB_bkg_Brush; } set { pIB_bkg_Brush = value; }}
				[Browsable(false)]
				public string BkgClSerialize
				{	get { return Serialize.BrushToString(pIB_bkg_Brush); } set { pIB_bkg_Brush = Serialize.StringToBrush(value); }
				}
		

		private TimeSpan pSessionLength = new TimeSpan(1,15,0);
		[NinjaScriptProperty]
		[Description("Enter session length in hh:mm:00, maximum is 23:59:00")]
		[Category("3 Parameters")]
		public string SessionLength {
			get { return pSessionLength.ToString(); }
			set { 
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pSessionLength); 
				pSessionLength = new TimeSpan(Math.Min(23,pSessionLength.Hours), Math.Min(59,pSessionLength.Minutes), Math.Min(59,pSessionLength.Seconds));
			}
		}
		
		
//		private TimeSpan pSessionLength = new TimeSpan(1,15,0);
//		[Description("Enter session length in hh:mm:00, maximum is 23:59:00")]
//		[Category("3 Parameters")]
//		public string SessionLength
//		{
//			get { return pSessionLength.ToString(); }
//			set { TimeSpan temp;
//				if(TimeSpan.TryParse(value, out temp)) {
//					int hr = Math.Min(temp.Hours,23);
//					int min = Math.Min(temp.Minutes,59);
//					pSessionLength = new TimeSpan(hr,min,0);
//				}}
//		}

//		private TimeSpan pStartAt = new TimeSpan(9,30,0);
//		[Description("Enter start time hh:mm:00, maximum is 23:59:00")]
//		[Category("3 Parameters")]
//		public string StartAt
//		{
//			get { return pStartAt.ToString(); }
//			set { TimeSpan temp;
//				if(TimeSpan.TryParse(value, out temp)) {
//					int hr = Math.Min(temp.Hours,23);
//					int min = Math.Min(temp.Minutes,59);
//					pStartAt = new TimeSpan(hr,min,0);
//				}}
//		}
		private TimeSpan pStartAt = new TimeSpan(9,30,0);
		[NinjaScriptProperty]
		[Description("Enter session start time in hh:mm:00, maximum is 23:59:00")]
		[Category("3 Parameters")]
		public string StartAt {
			get { return pStartAt.ToString(); }
			set { 
				string t = value.ToString();
				if(!t.Contains(":")) {
					while(t.Length<6) t = "0"+t;
					char[] tarray = t.ToCharArray(0,t.Length);
					t = tarray[0].ToString()+tarray[1].ToString()+":"+tarray[2].ToString()+tarray[3].ToString()+":"+tarray[4].ToString()+tarray[5].ToString();
				}
				TimeSpan.TryParse(t, out pStartAt); 
				pStartAt = new TimeSpan(Math.Min(23,pStartAt.Hours), Math.Min(59,pStartAt.Minutes), Math.Min(59,pStartAt.Seconds));
			}
		}
		private bool pPrintToAlertWindow = true;
		[Description("Print message to the Alerts window whenever a cross occurs - NOTE:  Only levels that have a Wav file named")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public bool PrintToAlertWindow
		{
			get { return pPrintToAlertWindow; }
			set { pPrintToAlertWindow = value; }
		}

		private bool pShowDotsOnSessions = false;
		[Description("When enabled, the indicator will draw a dot at the high of the session, on the open bar of the session, and at the low of the session on the close bar of the session")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public bool ShowDotsOnSessions
		{
			get { return pShowDotsOnSessions; }
			set { pShowDotsOnSessions = value; }
		}

		private int pDaysAgo = 0;
		[NinjaScriptProperty]
		[Description("Lookback period...shows the levels from the prior day (if set to 1)")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "2 Lookback")]
		public int DaysAgo
		{
			get { return pDaysAgo; }
			set { pDaysAgo = Math.Max(0,value); }
		}
		private string pSpecificDateStr = "mm/dd/yyyy";
		
		[NinjaScriptProperty]
		[Description("Enter the specific date to use in the calculation...this would be the date that the StartAt time occurred")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "2 Lookback")]
		public string SpecificDate
		{
			get { return pSpecificDateStr; }
			set {
				string temp = pSpecificDateStr;
				DateTime dt = SpecificDateDT;
				pSpecificDateStr = value; 
				var elem = pSpecificDateStr.Split(new char[]{'/','.'});
				int mo = 0;
				int day = 0;
				int yr = 0;
				try{
					if( int.TryParse(elem[0],out mo) &&
						int.TryParse(elem[1],out day) &&
						int.TryParse(elem[2],out yr)){
							SpecificDateDT = new DateTime(yr,mo,day,0,0,0);
						}
				}catch {
					pSpecificDateStr = temp;
					SpecificDateDT = dt;
				}
			}
		}
//		[NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "BarDown", GroupName = "NinjaScriptParameters", Order = 1)]

		private bool pShowSessionTime = false;
		[Description("Show session range times, start and stop...info printed at top-left corner of chart")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public bool ShowSessionTime
		{
			get { return pShowSessionTime; }
			set { pShowSessionTime = value; }
		}

		private float pFontSize = 10f;
		[Description("Size of label text")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public float FontSize
		{
			get { return pFontSize; }
			set { pFontSize = Math.Max(5,value); }
		}

		private bool pShowHistoricalLevels = false;
		[Description("Show all historical levels")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "1 Visual")]
		public bool ShowHistoricalLevels
		{
			get { return pShowHistoricalLevels; }
			set { pShowHistoricalLevels = value; }
		}

		private string pEmailAddress = string.Empty;
		[Description("Destination email address for any FibPct value that has a WAV file specified for it, leave blank to turn-off all emails")]
		[Category("Email")]
		public string EmailAddress
		{
			get { return pEmailAddress; }
			set { pEmailAddress = ValidateEmail(value.Trim()); }
		}
		#region Audible category
		private string pWav01 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct01 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav01
		{
			get { return pWav01; }
			set { pWav01 = value.Trim(); }
		}

		private string pWav02 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct02 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav02
		{
			get { return pWav02; }
			set { pWav02 = value.Trim(); }
		}

		private string pWav03 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct03 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav03
		{
			get { return pWav03; }
			set { pWav03 = value.Trim(); }
		}

		private string pWav04 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct04 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav04
		{
			get { return pWav04; }
			set { pWav04 = value.Trim(); }
		}

		private string pWav05 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct05 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav05
		{
			get { return pWav05; }
			set { pWav05 = value.Trim(); }
		}

		private string pWav06 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct06 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav06
		{
			get { return pWav06; }
			set { pWav06 = value.Trim(); }
		}

		private string pWav07 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct07 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav07
		{
			get { return pWav07; }
			set { pWav07 = value.Trim(); }
		}

		private string pWav08 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct08 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav08
		{
			get { return pWav08; }
			set { pWav08 = value.Trim(); }
		}

		private string pWav09 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct09 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav09
		{
			get { return pWav09; }
			set { pWav09 = value.Trim(); }
		}

		private string pWav10 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct10 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav10
		{
			get { return pWav10; }
			set { pWav10 = value.Trim(); }
		}

		private string pWav11 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct11 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav11
		{
			get { return pWav11; }
			set { pWav11 = value.Trim(); }
		}
		private string pWav12 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct12 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav12
		{
			get { return pWav12; }
			set { pWav12 = value.Trim(); }
		}
		private string pWav13 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct13 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav13
		{
			get { return pWav13; }
			set { pWav13 = value.Trim(); }
		}
		private string pWav14 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct14 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav14
		{
			get { return pWav14; }
			set { pWav14 = value.Trim(); }
		}
		private string pWav15 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct15 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav15
		{
			get { return pWav15; }
			set { pWav15 = value.Trim(); }
		}
		private string pWav16 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct16 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav16
		{
			get { return pWav16; }
			set { pWav16 = value.Trim(); }
		}
		private string pWav17 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct17 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav17
		{
			get { return pWav17; }
			set { pWav17 = value.Trim(); }
		}
		private string pWav18 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct18 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav18
		{
			get { return pWav18; }
			set { pWav18 = value.Trim(); }
		}
		private string pWav19 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct19 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav19
		{
			get { return pWav19; }
			set { pWav19 = value.Trim(); }
		}
		private string pWav20 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct20 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav20
		{
			get { return pWav20; }
			set { pWav20 = value.Trim(); }
		}
		private string pWav21 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct21 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav21
		{
			get { return pWav21; }
			set { pWav21 = value.Trim(); }
		}
		private string pWav22 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct22 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav22
		{
			get { return pWav22; }
			set { pWav22 = value.Trim(); }
		}
		private string pWav23 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct23 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav23
		{
			get { return pWav23; }
			set { pWav23 = value.Trim(); }
		}
		private string pWav24 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct24 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav24
		{
			get { return pWav24; }
			set { pWav24 = value.Trim(); }
		}
		private string pWav25 = "";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Description("Alert file to play when FibPct25 is hit, leave blank to turn-off the alert on this level")]
		[Category("Audible")]
		public string Wav25
		{
			get { return pWav25; }
			set { pWav25 = value.Trim(); }
		}
		#endregion

		#region FibPcts
		private double pFibPct1 = 0.5;
		[NinjaScriptProperty]
		[Description("Fib percentage #1")]
		[Category("3 Parameters")]
		public double FibPct01
		{
			get { return pFibPct1; }
			set { pFibPct1 = Math.Max(0,value); }
		}
		private double pFibPct2 = 0.382;
		[NinjaScriptProperty]
		[Description("Fib percentage #2")]
		[Category("3 Parameters")]
		public double FibPct02
		{
			get { return pFibPct2; }
			set { pFibPct2 = Math.Max(0,value); }
		}
		private double pFibPct3 = 0.75;
		[NinjaScriptProperty]
		[Description("Fib percentage #3")]
		[Category("3 Parameters")]
		public double FibPct03
		{
			get { return pFibPct3; }
			set { pFibPct3 = Math.Max(0,value); }
		}
		private double pFibPct4 = 1.618;
		[NinjaScriptProperty]
		[Description("Fib percentage #4")]
		[Category("3 Parameters")]
		public double FibPct04
		{
			get { return pFibPct4; }
			set { pFibPct4 = Math.Max(0,value); }
		}
		private double pFibPct5 = 2.618;
		[NinjaScriptProperty]
		[Description("Fib percentage #5")]
		[Category("3 Parameters")]
		public double FibPct05
		{
			get { return pFibPct5; }
			set { pFibPct5 = Math.Max(0,value); }
		}
		private double pFibPct6 = 3.618;
		[NinjaScriptProperty]
		[Description("Fib percentage #6")]
		[Category("3 Parameters")]
		public double FibPct06
		{
			get { return pFibPct6; }
			set { pFibPct6 = Math.Max(0,value); }
		}
		private double pFibPct7 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #7")]
		[Category("3 Parameters")]
		public double FibPct07
		{
			get { return pFibPct7; }
			set { pFibPct7 = Math.Max(0,value); }
		}
		private double pFibPct8 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #8")]
		[Category("3 Parameters")]
		public double FibPct08
		{
			get { return pFibPct8; }
			set { pFibPct8 = Math.Max(0,value); }
		}
		private double pFibPct9 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #9")]
		[Category("3 Parameters")]
		public double FibPct09
		{
			get { return pFibPct9; }
			set { pFibPct9 = Math.Max(0,value); }
		}
		private double pFibPct10 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #10")]
		[Category("3 Parameters")]
		public double FibPct10
		{
			get { return pFibPct10; }
			set { pFibPct10 = Math.Max(0,value); }
		}
		private double pFibPct11 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #11")]
		[Category("3 Parameters")]
		public double FibPct11
		{
			get { return pFibPct11; }
			set { pFibPct11 = Math.Max(0,value); }
		}
		private double pFibPct12 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #12")]
		[Category("3 Parameters")]
		public double FibPct12
		{
			get { return pFibPct12; }
			set { pFibPct12 = Math.Max(0,value); }
		}
		private double pFibPct13 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #13")]
		[Category("3 Parameters")]
		public double FibPct13
		{
			get { return pFibPct13; }
			set { pFibPct13 = Math.Max(0,value); }
		}
		private double pFibPct14 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #14")]
		[Category("3 Parameters")]
		public double FibPct14
		{
			get { return pFibPct14; }
			set { pFibPct14 = Math.Max(0,value); }
		}
		private double pFibPct15 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #15")]
		[Category("3 Parameters")]
		public double FibPct15
		{
			get { return pFibPct15; }
			set { pFibPct15 = Math.Max(0,value); }
		}
		private double pFibPct16 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #16")]
		[Category("3 Parameters")]
		public double FibPct16
		{
			get { return pFibPct16; }
			set { pFibPct16 = Math.Max(0,value); }
		}
		private double pFibPct17 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #17")]
		[Category("3 Parameters")]
		public double FibPct17
		{
			get { return pFibPct17; }
			set { pFibPct17 = Math.Max(0,value); }
		}
		private double pFibPct18 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #18")]
		[Category("3 Parameters")]
		public double FibPct18
		{
			get { return pFibPct18; }
			set { pFibPct18 = Math.Max(0,value); }
		}
		private double pFibPct19 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #19")]
		[Category("3 Parameters")]
		public double FibPct19
		{
			get { return pFibPct19; }
			set { pFibPct19 = Math.Max(0,value); }
		}
		private double pFibPct20 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #20")]
		[Category("3 Parameters")]
		public double FibPct20
		{
			get { return pFibPct20; }
			set { pFibPct20 = Math.Max(0,value); }
		}
		private double pFibPct21 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #21")]
		[Category("3 Parameters")]
		public double FibPct21
		{
			get { return pFibPct21; }
			set { pFibPct21 = Math.Max(0,value); }
		}
		private double pFibPct22 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #22")]
		[Category("3 Parameters")]
		public double FibPct22
		{
			get { return pFibPct22; }
			set { pFibPct22 = Math.Max(0,value); }
		}
		private double pFibPct23 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #23")]
		[Category("3 Parameters")]
		public double FibPct23
		{
			get { return pFibPct23; }
			set { pFibPct23 = Math.Max(0,value); }
		}
		private double pFibPct24 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #24")]
		[Category("3 Parameters")]
		public double FibPct24
		{
			get { return pFibPct24; }
			set { pFibPct24 = Math.Max(0,value); }
		}
		private double pFibPct25 = 0.0;
		[NinjaScriptProperty]
		[Description("Fib percentage #25")]
		[Category("3 Parameters")]
		public double FibPct25
		{
			get { return pFibPct25; }
			set { pFibPct25 = Math.Max(0,value); }
		}
		#endregion
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FibLinesOnSession[] cacheFibLinesOnSession;
		public FibLinesOnSession FibLinesOnSession(string sessionLength, string startAt, int daysAgo, string specificDate, double fibPct01, double fibPct02, double fibPct03, double fibPct04, double fibPct05, double fibPct06, double fibPct07, double fibPct08, double fibPct09, double fibPct10, double fibPct11, double fibPct12, double fibPct13, double fibPct14, double fibPct15, double fibPct16, double fibPct17, double fibPct18, double fibPct19, double fibPct20, double fibPct21, double fibPct22, double fibPct23, double fibPct24, double fibPct25)
		{
			return FibLinesOnSession(Input, sessionLength, startAt, daysAgo, specificDate, fibPct01, fibPct02, fibPct03, fibPct04, fibPct05, fibPct06, fibPct07, fibPct08, fibPct09, fibPct10, fibPct11, fibPct12, fibPct13, fibPct14, fibPct15, fibPct16, fibPct17, fibPct18, fibPct19, fibPct20, fibPct21, fibPct22, fibPct23, fibPct24, fibPct25);
		}

		public FibLinesOnSession FibLinesOnSession(ISeries<double> input, string sessionLength, string startAt, int daysAgo, string specificDate, double fibPct01, double fibPct02, double fibPct03, double fibPct04, double fibPct05, double fibPct06, double fibPct07, double fibPct08, double fibPct09, double fibPct10, double fibPct11, double fibPct12, double fibPct13, double fibPct14, double fibPct15, double fibPct16, double fibPct17, double fibPct18, double fibPct19, double fibPct20, double fibPct21, double fibPct22, double fibPct23, double fibPct24, double fibPct25)
		{
			if (cacheFibLinesOnSession != null)
				for (int idx = 0; idx < cacheFibLinesOnSession.Length; idx++)
					if (cacheFibLinesOnSession[idx] != null && cacheFibLinesOnSession[idx].SessionLength == sessionLength && cacheFibLinesOnSession[idx].StartAt == startAt && cacheFibLinesOnSession[idx].DaysAgo == daysAgo && cacheFibLinesOnSession[idx].SpecificDate == specificDate && cacheFibLinesOnSession[idx].FibPct01 == fibPct01 && cacheFibLinesOnSession[idx].FibPct02 == fibPct02 && cacheFibLinesOnSession[idx].FibPct03 == fibPct03 && cacheFibLinesOnSession[idx].FibPct04 == fibPct04 && cacheFibLinesOnSession[idx].FibPct05 == fibPct05 && cacheFibLinesOnSession[idx].FibPct06 == fibPct06 && cacheFibLinesOnSession[idx].FibPct07 == fibPct07 && cacheFibLinesOnSession[idx].FibPct08 == fibPct08 && cacheFibLinesOnSession[idx].FibPct09 == fibPct09 && cacheFibLinesOnSession[idx].FibPct10 == fibPct10 && cacheFibLinesOnSession[idx].FibPct11 == fibPct11 && cacheFibLinesOnSession[idx].FibPct12 == fibPct12 && cacheFibLinesOnSession[idx].FibPct13 == fibPct13 && cacheFibLinesOnSession[idx].FibPct14 == fibPct14 && cacheFibLinesOnSession[idx].FibPct15 == fibPct15 && cacheFibLinesOnSession[idx].FibPct16 == fibPct16 && cacheFibLinesOnSession[idx].FibPct17 == fibPct17 && cacheFibLinesOnSession[idx].FibPct18 == fibPct18 && cacheFibLinesOnSession[idx].FibPct19 == fibPct19 && cacheFibLinesOnSession[idx].FibPct20 == fibPct20 && cacheFibLinesOnSession[idx].FibPct21 == fibPct21 && cacheFibLinesOnSession[idx].FibPct22 == fibPct22 && cacheFibLinesOnSession[idx].FibPct23 == fibPct23 && cacheFibLinesOnSession[idx].FibPct24 == fibPct24 && cacheFibLinesOnSession[idx].FibPct25 == fibPct25 && cacheFibLinesOnSession[idx].EqualsInput(input))
						return cacheFibLinesOnSession[idx];
			return CacheIndicator<FibLinesOnSession>(new FibLinesOnSession(){ SessionLength = sessionLength, StartAt = startAt, DaysAgo = daysAgo, SpecificDate = specificDate, FibPct01 = fibPct01, FibPct02 = fibPct02, FibPct03 = fibPct03, FibPct04 = fibPct04, FibPct05 = fibPct05, FibPct06 = fibPct06, FibPct07 = fibPct07, FibPct08 = fibPct08, FibPct09 = fibPct09, FibPct10 = fibPct10, FibPct11 = fibPct11, FibPct12 = fibPct12, FibPct13 = fibPct13, FibPct14 = fibPct14, FibPct15 = fibPct15, FibPct16 = fibPct16, FibPct17 = fibPct17, FibPct18 = fibPct18, FibPct19 = fibPct19, FibPct20 = fibPct20, FibPct21 = fibPct21, FibPct22 = fibPct22, FibPct23 = fibPct23, FibPct24 = fibPct24, FibPct25 = fibPct25 }, input, ref cacheFibLinesOnSession);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FibLinesOnSession FibLinesOnSession(string sessionLength, string startAt, int daysAgo, string specificDate, double fibPct01, double fibPct02, double fibPct03, double fibPct04, double fibPct05, double fibPct06, double fibPct07, double fibPct08, double fibPct09, double fibPct10, double fibPct11, double fibPct12, double fibPct13, double fibPct14, double fibPct15, double fibPct16, double fibPct17, double fibPct18, double fibPct19, double fibPct20, double fibPct21, double fibPct22, double fibPct23, double fibPct24, double fibPct25)
		{
			return indicator.FibLinesOnSession(Input, sessionLength, startAt, daysAgo, specificDate, fibPct01, fibPct02, fibPct03, fibPct04, fibPct05, fibPct06, fibPct07, fibPct08, fibPct09, fibPct10, fibPct11, fibPct12, fibPct13, fibPct14, fibPct15, fibPct16, fibPct17, fibPct18, fibPct19, fibPct20, fibPct21, fibPct22, fibPct23, fibPct24, fibPct25);
		}

		public Indicators.FibLinesOnSession FibLinesOnSession(ISeries<double> input , string sessionLength, string startAt, int daysAgo, string specificDate, double fibPct01, double fibPct02, double fibPct03, double fibPct04, double fibPct05, double fibPct06, double fibPct07, double fibPct08, double fibPct09, double fibPct10, double fibPct11, double fibPct12, double fibPct13, double fibPct14, double fibPct15, double fibPct16, double fibPct17, double fibPct18, double fibPct19, double fibPct20, double fibPct21, double fibPct22, double fibPct23, double fibPct24, double fibPct25)
		{
			return indicator.FibLinesOnSession(input, sessionLength, startAt, daysAgo, specificDate, fibPct01, fibPct02, fibPct03, fibPct04, fibPct05, fibPct06, fibPct07, fibPct08, fibPct09, fibPct10, fibPct11, fibPct12, fibPct13, fibPct14, fibPct15, fibPct16, fibPct17, fibPct18, fibPct19, fibPct20, fibPct21, fibPct22, fibPct23, fibPct24, fibPct25);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FibLinesOnSession FibLinesOnSession(string sessionLength, string startAt, int daysAgo, string specificDate, double fibPct01, double fibPct02, double fibPct03, double fibPct04, double fibPct05, double fibPct06, double fibPct07, double fibPct08, double fibPct09, double fibPct10, double fibPct11, double fibPct12, double fibPct13, double fibPct14, double fibPct15, double fibPct16, double fibPct17, double fibPct18, double fibPct19, double fibPct20, double fibPct21, double fibPct22, double fibPct23, double fibPct24, double fibPct25)
		{
			return indicator.FibLinesOnSession(Input, sessionLength, startAt, daysAgo, specificDate, fibPct01, fibPct02, fibPct03, fibPct04, fibPct05, fibPct06, fibPct07, fibPct08, fibPct09, fibPct10, fibPct11, fibPct12, fibPct13, fibPct14, fibPct15, fibPct16, fibPct17, fibPct18, fibPct19, fibPct20, fibPct21, fibPct22, fibPct23, fibPct24, fibPct25);
		}

		public Indicators.FibLinesOnSession FibLinesOnSession(ISeries<double> input , string sessionLength, string startAt, int daysAgo, string specificDate, double fibPct01, double fibPct02, double fibPct03, double fibPct04, double fibPct05, double fibPct06, double fibPct07, double fibPct08, double fibPct09, double fibPct10, double fibPct11, double fibPct12, double fibPct13, double fibPct14, double fibPct15, double fibPct16, double fibPct17, double fibPct18, double fibPct19, double fibPct20, double fibPct21, double fibPct22, double fibPct23, double fibPct24, double fibPct25)
		{
			return indicator.FibLinesOnSession(input, sessionLength, startAt, daysAgo, specificDate, fibPct01, fibPct02, fibPct03, fibPct04, fibPct05, fibPct06, fibPct07, fibPct08, fibPct09, fibPct10, fibPct11, fibPct12, fibPct13, fibPct14, fibPct15, fibPct16, fibPct17, fibPct18, fibPct19, fibPct20, fibPct21, fibPct22, fibPct23, fibPct24, fibPct25);
		}
	}
}

#endregion
