//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
	/// <summary>
	/// Plots the open, high, and low values from the session starting on the current day.
	/// </summary>
	public class SessionOHL : Indicator
	{
		private DateTime				currentDate			=	Core.Globals.MinDate;
		private double					currentOpen			=	double.MinValue;
		private double					currentHigh			=	double.MinValue;
		private double					currentLow			=	double.MaxValue;
		private DateTime				lastDate			= 	Core.Globals.MinDate;
		private Data.SessionIterator	sessionIterator;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionCurrentDayOHL;
				Name						= "SessionOHL";
				IsAutoScale					= false;
				DrawOnPricePanel			= false;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				ShowLow						= true;
				ShowHigh					= true;
				ShowOpen					= true;
				BarsRequiredToPlot			= 0;

				pStartTime = 930;
				pEndTime = 1600;
				AddPlot(new Stroke(Brushes.Goldenrod,	DashStyleHelper.Dash, 2), PlotStyle.Square, NinjaTrader.Custom.Resource.CurrentDayOHLOpen);
				AddPlot(new Stroke(Brushes.SeaGreen,	DashStyleHelper.Dash, 2), PlotStyle.Square, NinjaTrader.Custom.Resource.CurrentDayOHLHigh);
				AddPlot(new Stroke(Brushes.Red,			DashStyleHelper.Dash, 2), PlotStyle.Square, NinjaTrader.Custom.Resource.CurrentDayOHLLow);
			}
			else if (State == State.Configure)
			{
				currentDate			= Core.Globals.MinDate;
				currentOpen			= double.MinValue;
				currentHigh			= double.MinValue;
				currentLow			= double.MaxValue;
				lastDate			= Core.Globals.MinDate;
			}
			else if (State == State.DataLoaded)
			{
				sessionIterator = new SessionIterator(Bars);
			}
			else if (State == State.Historical)
			{
				if (!Bars.BarsType.IsIntraday)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", "Session OHL works on Intraday data", TextPosition.BottomRight);
					//Log(Custom.Resource.CurrentDayOHLError, LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday) return;
			if(CurrentBar<3) return;

			currentDate = sessionIterator.GetTradingDay(Time[0]);
			
			int tt1 = ToTime(Time[1])/100;
			int tt0 = ToTime(Time[0])/100;
			bool c1 = pStartTime==0 ? (tt0 < tt1 ? true : false) : false;
			if ((c1 || (tt0 >= pStartTime && tt1 < pStartTime) && currentDate != lastDate) || currentOpen == double.MinValue)
			{
				//BackBrush = Brushes.Yellow;
				lastDate 		= currentDate;
				currentOpen		= Open[0];
				currentHigh		= High[0];
				currentLow		= Low[0];
			}

			c1 = pStartTime > pEndTime && (tt0 > pStartTime || tt0 <= pEndTime) ? true:false;
			bool c2 = pStartTime < pEndTime && (tt0 > pStartTime && tt0 <= pEndTime) ? true:false;
			if(c1 || c2){
				currentHigh			= Math.Max(currentHigh, High[0]);
				currentLow			= Math.Min(currentLow, Low[0]);
			}

			if (ShowOpen)
				CurrentOpen[0] = currentOpen;

			if (ShowHigh)
				CurrentHigh[0] = currentHigh;

			if (ShowLow)
				CurrentLow[0] = currentLow;
		}

		#region Properties
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentLow
		{
			get { return Values[2]; }
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "Start time", GroupName = "NinjaScriptParameters", Order = 1)]
		public int pStartTime
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "End time", GroupName = "NinjaScriptParameters", Order = 2)]
		public int pEndTime
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowHigh", GroupName = "NinjaScriptParameters", Order = 10)]
		public bool ShowHigh
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowLow", GroupName = "NinjaScriptParameters", Order = 20)]
		public bool ShowLow
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowOpen", GroupName = "NinjaScriptParameters", Order = 30)]
		public bool ShowOpen
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SessionOHL[] cacheSessionOHL;
		public SessionOHL SessionOHL()
		{
			return SessionOHL(Input);
		}

		public SessionOHL SessionOHL(ISeries<double> input)
		{
			if (cacheSessionOHL != null)
				for (int idx = 0; idx < cacheSessionOHL.Length; idx++)
					if (cacheSessionOHL[idx] != null &&  cacheSessionOHL[idx].EqualsInput(input))
						return cacheSessionOHL[idx];
			return CacheIndicator<SessionOHL>(new SessionOHL(), input, ref cacheSessionOHL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SessionOHL SessionOHL()
		{
			return indicator.SessionOHL(Input);
		}

		public Indicators.SessionOHL SessionOHL(ISeries<double> input )
		{
			return indicator.SessionOHL(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SessionOHL SessionOHL()
		{
			return indicator.SessionOHL(Input);
		}

		public Indicators.SessionOHL SessionOHL(ISeries<double> input )
		{
			return indicator.SessionOHL(input);
		}
	}
}

#endregion
