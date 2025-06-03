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
	public class TimeLeftInSession : Indicator
	{
		private SessionIterator sessionIterator;
		private DateTime EOS = DateTime.MinValue;
		private TimeSpan ts = TimeSpan.MinValue;
		private DateTime TimeWhenEOShappened = DateTime.MinValue;
		private int tickcount = 1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Time Left In Session";
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
				pUserDefinedEOS = "<use session template>";
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.Historical){
				sessionIterator = new SessionIterator(Bars);
				if(pUserDefinedEOS.Trim().Length>0 && !pUserDefinedEOS.ToLower().Contains("<use session template>")){
					if(!DateTime.TryParse(pUserDefinedEOS, out EOS)){
						pUserDefinedEOS = "<use session template>";
						EOS = DateTime.MinValue;
					}
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if(EOS != DateTime.MinValue && State == State.Realtime){
				EOS = new DateTime(Time[1].Year, Time[1].Month, Time[1].Day, EOS.Hour, EOS.Minute, EOS.Second);
			}
		}
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			if(State == State.Realtime){
				string endtimestr = "";
				if(EOS == DateTime.MinValue){
					sessionIterator.GetNextSession(Time.GetValueAt(CurrentBar-1), true);
					ts = new TimeSpan(sessionIterator.ActualSessionEnd.Ticks - DateTime.Now.Ticks);
					endtimestr = sessionIterator.ActualSessionEnd.ToShortTimeString();
				}else{
					ts = new TimeSpan(EOS.Ticks - DateTime.Now.Ticks);
					endtimestr = EOS.ToShortTimeString();
				}
				var MagentaBrushDX = Brushes.Magenta.ToDxBrush(RenderTarget);
				var BlackBrushDX = Brushes.Black.ToDxBrush(RenderTarget);
				var WhiteBrushDX = Brushes.White.ToDxBrush(RenderTarget);
				string msg = "";
				var stextFormat = new SimpleFont("Arial",16).ToDirectWriteTextFormat();
				if(ts.TotalMinutes<0){
					msg = "-- Session is now CLOSED --";
					if(TimeWhenEOShappened == DateTime.MinValue) TimeWhenEOShappened = DateTime.Now;
					var ts1 = new TimeSpan(DateTime.Now.Ticks - TimeWhenEOShappened.Ticks);
					if(ts1.TotalSeconds>10) msg = string.Empty;
				}
				else if(ts.TotalMinutes<10)
					msg = string.Format("Session ends at {0}, {1}-minutes from now", endtimestr, ts.TotalMinutes.ToString("0"));
				else if(ts.TotalMinutes<15)
					msg = string.Format("The current session ends in {0}-minutes", ts.TotalMinutes.ToString("0"));
				var stextLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, msg, stextFormat, (float)(ChartPanel.X + ChartPanel.W),100f);
				var v1 = new SharpDX.Vector2(Convert.ToSingle(ChartPanel.W)/2f - stextLayout.Metrics.Width/2f, 
					Convert.ToSingle(ChartPanel.H)-stextLayout.Metrics.Height*2f);
				var slineRect   = new SharpDX.RectangleF(v1.X, v1.Y, stextLayout.Metrics.Width, stextLayout.Metrics.Height+1f);

				if(msg!=string.Empty){
					if(ts.TotalMinutes<10)
						RenderTarget.FillRectangle(slineRect, MagentaBrushDX);
					else if(ts.TotalMinutes<15)
						RenderTarget.FillRectangle(slineRect, WhiteBrushDX);
					RenderTarget.DrawText(msg, stextFormat, slineRect, BlackBrushDX);
				}

				MagentaBrushDX.Dispose();
				BlackBrushDX.Dispose();
				WhiteBrushDX.Dispose();
			}
		}
		[NinjaScriptProperty]
		[Display(Name="Custom end time", Order=10, GroupName="Parameters", Description="Enter military time format: '13:45', or leave blank for using session time")]
		public string pUserDefinedEOS
		{ get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TimeLeftInSession[] cacheTimeLeftInSession;
		public TimeLeftInSession TimeLeftInSession(string pUserDefinedEOS)
		{
			return TimeLeftInSession(Input, pUserDefinedEOS);
		}

		public TimeLeftInSession TimeLeftInSession(ISeries<double> input, string pUserDefinedEOS)
		{
			if (cacheTimeLeftInSession != null)
				for (int idx = 0; idx < cacheTimeLeftInSession.Length; idx++)
					if (cacheTimeLeftInSession[idx] != null && cacheTimeLeftInSession[idx].pUserDefinedEOS == pUserDefinedEOS && cacheTimeLeftInSession[idx].EqualsInput(input))
						return cacheTimeLeftInSession[idx];
			return CacheIndicator<TimeLeftInSession>(new TimeLeftInSession(){ pUserDefinedEOS = pUserDefinedEOS }, input, ref cacheTimeLeftInSession);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TimeLeftInSession TimeLeftInSession(string pUserDefinedEOS)
		{
			return indicator.TimeLeftInSession(Input, pUserDefinedEOS);
		}

		public Indicators.TimeLeftInSession TimeLeftInSession(ISeries<double> input , string pUserDefinedEOS)
		{
			return indicator.TimeLeftInSession(input, pUserDefinedEOS);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TimeLeftInSession TimeLeftInSession(string pUserDefinedEOS)
		{
			return indicator.TimeLeftInSession(Input, pUserDefinedEOS);
		}

		public Indicators.TimeLeftInSession TimeLeftInSession(ISeries<double> input , string pUserDefinedEOS)
		{
			return indicator.TimeLeftInSession(input, pUserDefinedEOS);
		}
	}
}

#endregion
