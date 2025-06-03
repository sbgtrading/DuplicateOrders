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
	public enum SpecificDOW_Events {None, ADPNonFarmPay, PetroleumStatus, JoblessClaims, ChicagoPMI, DallasFedMfgSurvey, PMIMfgFinal}
	public class ViewSpecificDOW : Indicator
	{
		Brush bkg = null;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "View Specific DOW";
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
				IsSuspendedWhileInactive					= false;
				pDOW					= DayOfWeek.Thursday;
				pOpacity = 50;
				pMarkTime = 815;
				pMinutesAheadOfNYC = 0;
				pEvent = SpecificDOW_Events.None;
				AddPlot(Brushes.Orange, "DOWcolor");
			}
			else if (State == State.Configure)
			{
				bkg = Plots[0].Brush.Clone();
				bkg.Opacity = pOpacity/100.0;
				bkg.Freeze();
			}
		}

		private int AddMinutes(int event_time_NYC, int MinutesAheadOfNYC){
			int hr = Convert.ToInt32(Math.Truncate(event_time_NYC/100.0));
			int min = hr*100 - event_time_NYC;
			var dt = new DateTime(2023, 1, 1, hr, min, 0).AddMinutes(MinutesAheadOfNYC);
			return ToTime(dt)/100;
		}
		DayOfWeek dow = DayOfWeek.Monday;
		int time = 0;
		int t0=0;
		int t1=0;
		protected override void OnBarUpdate()
		{
			DateTime t = Times[0][0];
			if(CurrentBar>1){
				t0 = ToTime(t)/100;
				t1 = ToTime(Times[0][1])/100;
			}
			if(pEvent == SpecificDOW_Events.None){
				time = pMarkTime;
				dow = pDOW;
			}else if(pEvent == SpecificDOW_Events.ADPNonFarmPay){
				time = AddMinutes(815, this.pMinutesAheadOfNYC);
				dow = DayOfWeek.Wednesday;
			}else if(pEvent == SpecificDOW_Events.JoblessClaims){
				time = AddMinutes(830, this.pMinutesAheadOfNYC);
				dow = DayOfWeek.Thursday;
			}else if(pEvent == SpecificDOW_Events.ChicagoPMI){
				time = AddMinutes(945, this.pMinutesAheadOfNYC);
				var last_day = DateTime.DaysInMonth(t.Year, t.Month);
				if(t.Day==last_day)
					dow = t.DayOfWeek;
				else if(t.Day==last_day-1 && t.DayOfWeek == DayOfWeek.Friday)
					dow = t.DayOfWeek;
				else if(t.Day==last_day-2 && t.DayOfWeek == DayOfWeek.Friday)
					dow = t.DayOfWeek;
				else time = -1;
			}else if(pEvent == SpecificDOW_Events.DallasFedMfgSurvey){
				time = AddMinutes(1030, this.pMinutesAheadOfNYC);
				dow = DayOfWeek.Monday;
			}else if(pEvent == SpecificDOW_Events.PMIMfgFinal){
				if(t.Day < Times[0][1].Day){//is first work day of the month
					time = AddMinutes(945, this.pMinutesAheadOfNYC);
					dow = t.DayOfWeek;
				}else time = -1;
			}
			if(t.DayOfWeek != dow){
				BackBrush = bkg;
			}else{
				if(CurrentBar>1){
					if(t1 < time && t0 >= time) Draw.VerticalLine(this,t.ToString(),1, Brushes.Red);
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="DOW", Order=1, GroupName="Parameters")]
		public DayOfWeek pDOW
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Opacity", Order=20, GroupName="Parameters")]
		public int pOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Specific Time (chart)", Order=30, GroupName="Parameters")]
		public int pMarkTime
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Minutes ahead of NYC", Order=35, GroupName="Parameters")]
		public int pMinutesAheadOfNYC
		{ get; set; }

		[Display(Name="Specific Event", Order=40, GroupName="Parameters")]
		public SpecificDOW_Events pEvent
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DOWcolor
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ViewSpecificDOW[] cacheViewSpecificDOW;
		public ViewSpecificDOW ViewSpecificDOW(DayOfWeek pDOW, int pOpacity, int pMarkTime, int pMinutesAheadOfNYC)
		{
			return ViewSpecificDOW(Input, pDOW, pOpacity, pMarkTime, pMinutesAheadOfNYC);
		}

		public ViewSpecificDOW ViewSpecificDOW(ISeries<double> input, DayOfWeek pDOW, int pOpacity, int pMarkTime, int pMinutesAheadOfNYC)
		{
			if (cacheViewSpecificDOW != null)
				for (int idx = 0; idx < cacheViewSpecificDOW.Length; idx++)
					if (cacheViewSpecificDOW[idx] != null && cacheViewSpecificDOW[idx].pDOW == pDOW && cacheViewSpecificDOW[idx].pOpacity == pOpacity && cacheViewSpecificDOW[idx].pMarkTime == pMarkTime && cacheViewSpecificDOW[idx].pMinutesAheadOfNYC == pMinutesAheadOfNYC && cacheViewSpecificDOW[idx].EqualsInput(input))
						return cacheViewSpecificDOW[idx];
			return CacheIndicator<ViewSpecificDOW>(new ViewSpecificDOW(){ pDOW = pDOW, pOpacity = pOpacity, pMarkTime = pMarkTime, pMinutesAheadOfNYC = pMinutesAheadOfNYC }, input, ref cacheViewSpecificDOW);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ViewSpecificDOW ViewSpecificDOW(DayOfWeek pDOW, int pOpacity, int pMarkTime, int pMinutesAheadOfNYC)
		{
			return indicator.ViewSpecificDOW(Input, pDOW, pOpacity, pMarkTime, pMinutesAheadOfNYC);
		}

		public Indicators.ViewSpecificDOW ViewSpecificDOW(ISeries<double> input , DayOfWeek pDOW, int pOpacity, int pMarkTime, int pMinutesAheadOfNYC)
		{
			return indicator.ViewSpecificDOW(input, pDOW, pOpacity, pMarkTime, pMinutesAheadOfNYC);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ViewSpecificDOW ViewSpecificDOW(DayOfWeek pDOW, int pOpacity, int pMarkTime, int pMinutesAheadOfNYC)
		{
			return indicator.ViewSpecificDOW(Input, pDOW, pOpacity, pMarkTime, pMinutesAheadOfNYC);
		}

		public Indicators.ViewSpecificDOW ViewSpecificDOW(ISeries<double> input , DayOfWeek pDOW, int pOpacity, int pMarkTime, int pMinutesAheadOfNYC)
		{
			return indicator.ViewSpecificDOW(input, pDOW, pOpacity, pMarkTime, pMinutesAheadOfNYC);
		}
	}
}

#endregion
