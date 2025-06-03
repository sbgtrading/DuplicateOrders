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
	public class AvgRange : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AvgRange";
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
				IsSuspendedWhileInactive					= true;
				Period					= 14;
				AddPlot(Brushes.Orange, "AvgRngPts");
				AddPlot(Brushes.Green, "AvgHCPts");
				AddPlot(Brushes.Red, "AvgLCPts");
			}
			else if (State == State.Configure)
			{
			}
		}

		List<double> ranges = new List<double>();
		List<double> rangesHC = new List<double>();
		List<double> rangesLC = new List<double>();
		protected override void OnBarUpdate()
		{
			if(CurrentBar<5) return;
			ranges.Add(Highs[0][1]-Lows[0][1]);
			rangesHC.Add(Highs[0][1]-Closes[0][1]);
			rangesLC.Add(Closes[0][1]-Lows[0][1]);
			while(ranges.Count>Period) ranges.RemoveAt(0);
			while(rangesHC.Count>Period) rangesHC.RemoveAt(0);
			while(rangesLC.Count>Period) rangesLC.RemoveAt(0);
			Values[0][0] = Instrument.MasterInstrument.RoundToTickSize(ranges.Average());
			Values[1][0] = Instrument.MasterInstrument.RoundToTickSize(rangesHC.Average());
			Values[2][0] = Instrument.MasterInstrument.RoundToTickSize(rangesLC.Average());
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRngPts
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
		private AvgRange[] cacheAvgRange;
		public AvgRange AvgRange(int period)
		{
			return AvgRange(Input, period);
		}

		public AvgRange AvgRange(ISeries<double> input, int period)
		{
			if (cacheAvgRange != null)
				for (int idx = 0; idx < cacheAvgRange.Length; idx++)
					if (cacheAvgRange[idx] != null && cacheAvgRange[idx].Period == period && cacheAvgRange[idx].EqualsInput(input))
						return cacheAvgRange[idx];
			return CacheIndicator<AvgRange>(new AvgRange(){ Period = period }, input, ref cacheAvgRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AvgRange AvgRange(int period)
		{
			return indicator.AvgRange(Input, period);
		}

		public Indicators.AvgRange AvgRange(ISeries<double> input , int period)
		{
			return indicator.AvgRange(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AvgRange AvgRange(int period)
		{
			return indicator.AvgRange(Input, period);
		}

		public Indicators.AvgRange AvgRange(ISeries<double> input , int period)
		{
			return indicator.AvgRange(input, period);
		}
	}
}

#endregion
