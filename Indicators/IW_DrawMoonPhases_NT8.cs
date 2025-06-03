// 
// Copyright (C) 2011, SBG Trading Corp.    www.affordableindicators.com
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//


#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Media;
using System.IO;
#endregion

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
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("Shows the precise time of Full moons and New moons.  The output 'Signal' will contain a +1 for a Full moon, a +2 for a New moon, and zero at all other times")]
	public class DrawMoonPhases : Indicator
	{
		private List<DateTime> NewTimes, FullTimes, FirstQTimes, LastQTimes;
		private int MoonId = 0;
		private bool RunInit = true;
		private bool Debug = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Debug = System.IO.File.Exists("c:\\222222222222.txt");
				Debug = Debug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!Debug)
					VendorLicense("IndicatorWarehouse", "AIDrawMoonPhases", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(Brushes.Transparent, "Signal");
				Calculate=Calculate.OnBarClose;
	 			//PriceTypeSupported	= false;
				IsOverlay=true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				IsAutoScale=false;
				Name = "iw DrawMoonPhases";
			}

			if (State == State.Configure)
			{
				NewTimes    = new List<DateTime>();
				FullTimes   = new List<DateTime>();
				FirstQTimes = new List<DateTime>();
				LastQTimes  = new List<DateTime>();
				IsAutoScale=false;
				Calculate=Calculate.OnBarClose;
			}
		}

		private static string MakeString(object[] s){
			System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
			for(int i = 0; i<s.Length; i++) {
				stb = stb.Append(s[i].ToString());
			}
			return stb.ToString();
		}

		protected override void OnBarUpdate()
		{
			if(RunInit) {
				RunInit = false;
				List<string> MoonPhase = new List<string>();
				List<DateTime> MoonPhaseDate = new List<DateTime>();
if(Debug) Print("Reading: "+pInPath);
				if(!File.Exists(pInPath)) {
if(Debug) Print("File doesn't exist");
					Draw.TextFixed(this, "nofile",MakeString(new Object[]{"InputFile (",pInPath,") was not found",Environment.NewLine,"."}), TextPosition.BottomLeft);
				} else {
					string readText = File.ReadAllText(pInPath);
					string [] CompleteFile = readText.Split(new Char [] {'\r','\n'},StringSplitOptions.RemoveEmptyEntries);
if(Debug) Print("File exists, contains "+CompleteFile.Length+"-lines");
					DateTime t=DateTime.MinValue;
					foreach (string FullLine in CompleteFile) {
						if(!FullLine.Contains("//") && FullLine.Length!=0) { //skip comment lines
if(Debug) Print("Line: "+FullLine);
							string [] element = FullLine.Split(new Char[] {',','\t'});
							if(element.Length<2) break;
							for(int e = 1; e<element.Length; e++){
								if(element[0].ToUpper().Contains("FULL") && DateTime.TryParse(element[e], out t)) {
									MoonPhase.Add("Full");
									MoonPhaseDate.Add(t.AddMinutes(pChartTimeMinusUTCInMinutes));
if(Debug) Print("adding a full moon at: "+t.ToString());
								}
								else if(element[0].ToUpper().Contains("NEW") && DateTime.TryParse(element[e], out t)) {
									MoonPhase.Add("New");
									MoonPhaseDate.Add(t.AddMinutes(pChartTimeMinusUTCInMinutes));
								}
								else if(element[0].ToUpper().Contains("FIRST") && DateTime.TryParse(element[e], out t)) {
									MoonPhase.Add("FirstQuarter");
									MoonPhaseDate.Add(t.AddMinutes(pChartTimeMinusUTCInMinutes));
								}
								else if(element[0].ToUpper().Contains("LAST") && DateTime.TryParse(element[e], out t)) {
									MoonPhase.Add("LastQuarter");
									MoonPhaseDate.Add(t.AddMinutes(pChartTimeMinusUTCInMinutes));
								}
							}
						}
					}
				}
				bool Finished = false;
				while(!Finished && MoonId < MoonPhase.Count)
				{
					if(pDrawFullMoon && MoonPhase[MoonId].Contains("Full")) {
						FullTimes.Add(MoonPhaseDate[MoonId]);
						//if(FullTimes[FullTimes.Count-1].Ticks > NinjaTrader.Core.Globals.Now.AddYears(1).Ticks) Finished = true;
						Printt(MakeString(new Object[] {MoonPhase[MoonId],", ",FullTimes[FullTimes.Count-1],Environment.NewLine}));
					}

					if(pDrawNewMoon && MoonPhase[MoonId].Contains("New")) {
						NewTimes.Add(MoonPhaseDate[MoonId]);
						//if(NewTimes[NewTimes.Count-1].Ticks > NinjaTrader.Core.Globals.Now.AddYears(1).Ticks) Finished = true;
						Printt(MakeString(new Object[] {MoonPhase[MoonId],", ",NewTimes[NewTimes.Count-1],Environment.NewLine}));
					}

					if(pDrawFirstQuarter && MoonPhase[MoonId].Contains("FirstQuarter")) {
						FirstQTimes.Add(MoonPhaseDate[MoonId]);
						//if(FirstQTimes[FirstQTimes.Count-1].Ticks > NinjaTrader.Core.Globals.Now.AddYears(1).Ticks) Finished = true;
						Printt(MakeString(new Object[] {MoonPhase[MoonId],", ",FirstQTimes[FirstQTimes.Count-1],Environment.NewLine}));
					}

					if(pDrawLastQuarter && MoonPhase[MoonId].Contains("LastQuarter")) {
						LastQTimes.Add(MoonPhaseDate[MoonId]);
						//if(LastQTimes[LastQTimes.Count-1].Ticks > NinjaTrader.Core.Globals.Now.AddYears(1).Ticks) Finished = true;
						Printt(MakeString(new Object[] {MoonPhase[MoonId],", ",LastQTimes[LastQTimes.Count-1],Environment.NewLine}));
					}
					MoonId++;
					Finished = MoonId >= MoonPhase.Count;
				}
				MoonPhase.Clear();
				MoonPhaseDate.Clear();
			}


			DateTime NextEvent = DateTime.MaxValue;
			string NextEventName = "";
			Signal[0] = (0);
			double Separation = ATR(14)[0]*pSeparationMultiple;
			if(CurrentBar>5)
			{
				for(MoonId = 0; MoonId < FullTimes.Count; MoonId++) {
					if(FullTimes[MoonId]>NinjaTrader.Core.Globals.Now && FullTimes[MoonId]<NextEvent) {
						NextEvent = FullTimes[MoonId];
						NextEventName = "Full moon";
					}
					if(Time[0] >= FullTimes[MoonId] && Time[1] < FullTimes[MoonId]) {
//						Draw.VerticalLine(this, "Full"+CurrentBar, 0, pFullMoonBrush, DashStyleHelper.Dash, pLineWidth);
						Draw.Line(this, MakeString(new Object[] {"Full",CurrentBar}),false, 0, Low[0]-Separation, 0, 0, pFullMoonBrush, DashStyleHelper.Dash, pLineWidth);
						FullTimes.RemoveAt(MoonId);
						Signal[0] = (1);
//						TheHigh = High[1];
//						TheLow = Low[1];
//						if(TheHigh-TheLow < 30*TickSize) {
//							double Midline = (TheHigh+TheLow)/2.0;
//							TheHigh = Midline + 15*TickSize;
//							TheLow = Midline - 15*TickSize;
//						}
						break;
					}
				}

				for(MoonId = 0; MoonId < NewTimes.Count; MoonId++) {
					if(NewTimes[MoonId]>NinjaTrader.Core.Globals.Now && NewTimes[MoonId]<NextEvent) {
						NextEvent = NewTimes[MoonId];
						NextEventName = "New moon";
					}
					if(Time[0] >= NewTimes[MoonId] && Time[1] < NewTimes[MoonId]) {
//						Draw.VerticalLine(this, "New"+CurrentBar, 0, pNewMoonBrush, DashStyleHelper.Dash, pLineWidth);
						Draw.Line(this, MakeString(new Object[] {"New",CurrentBar}),false, 0, Low[0]-Separation, 0, 0, pNewMoonBrush, DashStyleHelper.Dash, pLineWidth);
						Signal[0] = (2);
						NewTimes.RemoveAt(MoonId);
//						TheHigh = High[1];
//						TheLow = Low[1];
//						if(TheHigh-TheLow < 30*TickSize) {
//							double Midline = (TheHigh+TheLow)/2.0;
//							TheHigh = Midline + 15*TickSize;
//							TheLow = Midline - 15*TickSize;
//						}
						break;
					}
				}

				for(MoonId = 0; MoonId < LastQTimes.Count; MoonId++) {
					if(LastQTimes[MoonId]>NinjaTrader.Core.Globals.Now && LastQTimes[MoonId]<NextEvent) {
						NextEvent = LastQTimes[MoonId];
						NextEventName = "Last Quarter moon";
					}
					if(Time[0] >= LastQTimes[MoonId] && Time[1] < LastQTimes[MoonId]) {
//						Draw.VerticalLine(this, "Last"+CurrentBar, 0, pLastQuarterBrush, DashStyleHelper.Dash, pLineWidth);
						Draw.Line(this, MakeString(new Object[] {"Last",CurrentBar}),false, 0, Low[0]-Separation, 0, 0, pLastQuarterBrush, DashStyleHelper.Dash, pLineWidth);
						Signal[0] = (3);
						LastQTimes.RemoveAt(MoonId);
//						TheHigh = High[1];
//						TheLow = Low[1];
//						if(TheHigh-TheLow < 30*TickSize) {
//							double Midline = (TheHigh+TheLow)/2.0;
//							TheHigh = Midline + 15*TickSize;
//							TheLow = Midline - 15*TickSize;
//						}
						break;
					}
				}

				for(MoonId = 0; MoonId < FirstQTimes.Count; MoonId++) {
					if(FirstQTimes[MoonId]>NinjaTrader.Core.Globals.Now && FirstQTimes[MoonId]<NextEvent) {
						NextEvent = FirstQTimes[MoonId];
						NextEventName = "First Quarter moon";
					}
					if(Time[0] >= FirstQTimes[MoonId] && Time[1] < FirstQTimes[MoonId]) {
						Draw.Line(this, MakeString(new Object[] {"First",CurrentBar}),false, 0, Low[0]-Separation, 0, 0, pFirstQuarterBrush, DashStyleHelper.Dash, pLineWidth);
						Signal[0] = (4);
						FirstQTimes.RemoveAt(MoonId);
//						TheHigh = High[1];
//						TheLow = Low[1];
//						if(TheHigh-TheLow < 30*TickSize) {
//							double Midline = (TheHigh+TheLow)/2.0;
//							TheHigh = Midline + 15*TickSize;
//							TheLow = Midline - 15*TickSize;
//						}
						break;
					}
				}
				if(CurrentBar>=Bars.Count-2 && NextEvent!=DateTime.MaxValue && pShowNextEventTime) {
					string msg = MakeString(new Object[] {"Next event is a ",NextEventName," at ",NextEvent.ToString()});
					Draw.TextFixed(this, "nextevent",msg, TextPosition.TopRight);
					Print(MakeString(new Object[] {"DrawMoonPhases indicator on ",Instrument.FullName,":  ",msg}));
				}
//				BarHigh[0] = (TheHigh);
//				BarLow[0] = (TheLow);
//				TargetHigh[0] = (TheHigh + 2.5 * (TheHigh-TheLow));
//				TargetLow[0] = (TheLow - 2.5 * (TheHigh-TheLow));
			}
        }
		//======================================================================================
		private void Printt(string msg) {
			if(pSendToOutputWindow) Print(msg);
		}
		//======================================================================================

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> Signal
		{
			get { return Values[0]; }
		}


        #region Properties
		private int pLineWidth = 2;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line Width",  GroupName = "Visual")]
		public int LineWidth
		{	get { return pLineWidth; }
			set { pLineWidth = Math.Max(1,value);}
		}
		private double pSeparationMultiple = 0.1;
		[Description("Distance between low of bar and the top of the signal line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line separation",  GroupName = "Visual")]
		public double SeparationMultiple
		{	get { return pSeparationMultiple; }
			set { pSeparationMultiple = Math.Max(0,value);}
		}

		private int pChartTimeMinusUTCInMinutes = -240;
		[Description("Enter the number of minutes by taking the Current Local Clock Time minus the Current UTC clock time.  Ex. If you're in NY, Eastern Time is typically 4-hrs behind UTC, therefore enter -240")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Minutes ahead of UTC",  GroupName = "Parameters")]
		public int ChartTimeMinusUTCInMinutes
		{	get { return pChartTimeMinusUTCInMinutes; }
			set { pChartTimeMinusUTCInMinutes = value;}
		}

		private string pInPath = Core.Globals.UserDataDir.ToString()+"lunar_phase_dates.csv";
		[Description("Input data file name, in comma separated value format")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Input file",  GroupName = "Input")]
		public string InputFile
		{	get { return pInPath; }
			set { pInPath = value;}
		}

		private bool pShowNextEventTime = true;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Time of next event?",  GroupName = "Output")]
		public bool ShowNextEventTime
		{	get { return pShowNextEventTime; }
			set { pShowNextEventTime = value;}
		}

		private bool pSendToOutputWindow = true;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Send status and data to Output Window?",  GroupName = "Output")]
		public bool SendToOutputWindow
		{	get { return pSendToOutputWindow; }
			set { pSendToOutputWindow = value;}
		}

		private bool pDrawFullMoon = true;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Full Moons",  GroupName = "Parameters")]
		public bool DrawFullMoons
		{	get { return pDrawFullMoon; }
			set { pDrawFullMoon = value;}
		}

		private bool pDrawNewMoon = true;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show New Moons",  GroupName = "Parameters")]
		public bool DrawNewMoons
		{	get { return pDrawNewMoon; }
			set { pDrawNewMoon = value;}
		}

		private bool pDrawFirstQuarter = true;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show FirstQuarter",  GroupName = "Parameters")]
		public bool DrawFirstQuarter
		{	get { return pDrawFirstQuarter; }
			set { pDrawFirstQuarter = value;}
		}

		private bool pDrawLastQuarter = true;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show LastQuarter",  GroupName = "Parameters")]
		public bool DrawLastQuarter
		{	get { return pDrawLastQuarter; }
			set { pDrawLastQuarter = value;}
		}

		private Brush pFullMoonBrush = Brushes.Yellow;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color FullMoon",  GroupName = "Visual")]
		public Brush FMC{	get { return pFullMoonBrush; }	set { pFullMoonBrush = value; }		}
		[Browsable(false)]
		public string FMClSerialize
		{	get { return Serialize.BrushToString(pFullMoonBrush); } set { pFullMoonBrush = Serialize.StringToBrush(value); }
		}

		private Brush pNewMoonBrush = Brushes.DimGray;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color NewMoon",  GroupName = "Visual")]
		public Brush NMC{	get { return pNewMoonBrush; }	set { pNewMoonBrush = value; }		}
		[Browsable(false)]
		public string NMClSerialize
		{	get { return Serialize.BrushToString(pNewMoonBrush); } set { pNewMoonBrush = Serialize.StringToBrush(value); }
		}

		private Brush pFirstQuarterBrush = Brushes.Goldenrod;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color FirstQuarter",  GroupName = "Visual")]
		public Brush FqMC{	get { return pFirstQuarterBrush; }	set { pFirstQuarterBrush = value; }		}
		[Browsable(false)]
		public string FqMClSerialize
		{	get { return Serialize.BrushToString(pFirstQuarterBrush); } set { pFirstQuarterBrush = Serialize.StringToBrush(value); }
		}

		private Brush pLastQuarterBrush = Brushes.ForestGreen;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color LastQuarter",  GroupName = "Visual")]
		public Brush LqMC{	get { return pLastQuarterBrush; }	set { pLastQuarterBrush = value; }		}
		[Browsable(false)]
		public string LqMClSerialize
		{	get { return Serialize.BrushToString(pLastQuarterBrush); } set { pLastQuarterBrush = Serialize.StringToBrush(value); }
		}

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DrawMoonPhases[] cacheDrawMoonPhases;
		public DrawMoonPhases DrawMoonPhases()
		{
			return DrawMoonPhases(Input);
		}

		public DrawMoonPhases DrawMoonPhases(ISeries<double> input)
		{
			if (cacheDrawMoonPhases != null)
				for (int idx = 0; idx < cacheDrawMoonPhases.Length; idx++)
					if (cacheDrawMoonPhases[idx] != null &&  cacheDrawMoonPhases[idx].EqualsInput(input))
						return cacheDrawMoonPhases[idx];
			return CacheIndicator<DrawMoonPhases>(new DrawMoonPhases(), input, ref cacheDrawMoonPhases);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DrawMoonPhases DrawMoonPhases()
		{
			return indicator.DrawMoonPhases(Input);
		}

		public Indicators.DrawMoonPhases DrawMoonPhases(ISeries<double> input )
		{
			return indicator.DrawMoonPhases(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DrawMoonPhases DrawMoonPhases()
		{
			return indicator.DrawMoonPhases(Input);
		}

		public Indicators.DrawMoonPhases DrawMoonPhases(ISeries<double> input )
		{
			return indicator.DrawMoonPhases(input);
		}
	}
}

#endregion
