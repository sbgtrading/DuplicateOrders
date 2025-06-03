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
	public class RangesByDay : Indicator
	{
		SortedDictionary<DayOfWeek, List<double>> Data = new SortedDictionary<DayOfWeek, List<double>>();
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "RangesByDay";
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
				Period					= 5;
				AddPlot(new Stroke(Brushes.Pink,5),PlotStyle.Bar, "AvgRangeSu");
				AddPlot(new Stroke(Brushes.Blue,5),PlotStyle.Bar, "AvgRangeMo");
				AddPlot(new Stroke(Brushes.Red,5),PlotStyle.Bar, "AvgRangeTu");
				AddPlot(new Stroke(Brushes.Orange,5),PlotStyle.Bar, "AvgRangeWe");
				AddPlot(new Stroke(Brushes.Green,5),PlotStyle.Bar, "AvgRangeTh");
				AddPlot(new Stroke(Brushes.Purple,5),PlotStyle.Bar, "AvgRangeFr");
				AddPlot(new Stroke(Brushes.DimGray,5),PlotStyle.Bar, "AvgRangeSa");
			}
			else if (State == State.Configure)
			{
			}
		}

		bool PrintedResults = false;
		protected override void OnBarUpdate()
		{
			var dow = Times[0][0].DayOfWeek;
Print(dow.ToString());
			if(!Data.ContainsKey(dow)) Data[dow] = new List<double>();
			Data[dow].Add(High[0]-Low[0]);
			while(Data[dow].Count>Period) Data[dow].RemoveAt(0);
			Values[(int)dow][0] = Data[dow].Average();
			if(CurrentBar > BarsArray[0].Count-3){
				if(!PrintedResults){
					foreach(var kvp in Data){
						var avg = kvp.Value.Average();
						Print(kvp.Key.ToString()+":  "+(Instrument.MasterInstrument.PointValue * avg).ToString("C0"));
					}
				}
				PrintedResults = true;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeSu
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeMo
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeTu
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeWe
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeTh
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeFr
		{
			get { return Values[5]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgRangeSa
		{
			get { return Values[6]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RangesByDay[] cacheRangesByDay;
		public RangesByDay RangesByDay(int period)
		{
			return RangesByDay(Input, period);
		}

		public RangesByDay RangesByDay(ISeries<double> input, int period)
		{
			if (cacheRangesByDay != null)
				for (int idx = 0; idx < cacheRangesByDay.Length; idx++)
					if (cacheRangesByDay[idx] != null && cacheRangesByDay[idx].Period == period && cacheRangesByDay[idx].EqualsInput(input))
						return cacheRangesByDay[idx];
			return CacheIndicator<RangesByDay>(new RangesByDay(){ Period = period }, input, ref cacheRangesByDay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RangesByDay RangesByDay(int period)
		{
			return indicator.RangesByDay(Input, period);
		}

		public Indicators.RangesByDay RangesByDay(ISeries<double> input , int period)
		{
			return indicator.RangesByDay(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RangesByDay RangesByDay(int period)
		{
			return indicator.RangesByDay(Input, period);
		}

		public Indicators.RangesByDay RangesByDay(ISeries<double> input , int period)
		{
			return indicator.RangesByDay(input, period);
		}
	}
}

#endregion
