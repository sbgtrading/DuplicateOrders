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
	public class BiggestBarOfDay : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Time based chart, find the biggest bar of the day, for each day of week.";
				Name										= "BiggestBarOfDay";
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
				DayOfW					= 1;
				AddPlot(Brushes.Orange, "AvgSize");
			}
			else if (State == State.Configure)
			{
			}
		}

		bool Done = false;
		SortedDictionary<DayOfWeek, SortedDictionary<int,List<double>>> Data = new SortedDictionary<DayOfWeek, SortedDictionary<int,List<double>>>();
		protected override void OnBarUpdate()
		{
			var key = ToTime(Times[0][0])/100;//Times[0][0].Hour*60 + Times[0][0].Minute;
			var dow = Times[0][0].DayOfWeek;
			if(!Data.ContainsKey(dow)) Data[dow] = new SortedDictionary<int,List<double>>();
			if(!Data[dow].ContainsKey(key)) Data[dow][key] = new List<double>();
			Data[dow][key].Add(Highs[0][0]-Lows[0][0]);

			if(CurrentBars[0] > BarsArray[0].Count-3 && !Done){
				Done = true;
				var Result = new SortedDictionary<DayOfWeek, int>();
				foreach(var day in Data.Keys){
					foreach(var tod in Data[day].Keys){
						if(Data[day][tod].Count>0){
							var avg = Data[day][tod].Average();
							Data[day][tod].Clear();
							Data[day][tod].Add(avg);
						}
					}
					double max = 0;
					int maxkey = 0;
					foreach(var tod in Data[day].Keys){
						if(Data[day][tod][0] > max){
							maxkey = tod;
							max = Data[day][tod][0];
						}
						Result[day] = maxkey;
//						Print(day.ToString()+"   max: "+Result[day]);
					}
				}
				//foreach(var kk in Result)Print(kk.Key.ToString()+"  "+kk.Value);

				for(int rbar = CurrentBars[0]; rbar>5; rbar--){
					var tt = ToTime(Times[0][rbar])/100;
					var day = Times[0][rbar].DayOfWeek;
					if(Data[day].ContainsKey(tt)){
						AvgSize[rbar] = Math.Truncate(Data[day][tt][0]/TickSize);
						if(Result.ContainsKey(day) && tt == Result[day]) 
							BackBrushes[rbar] = Brushes.Gold;
					}
				}
			}
		}

//		private int ttime(double k){
//			double x = k/60.0;
//			double hr = Math.Truncate(x);
//			double minute = k - hr*60;
//			return Convert.ToInt32(hr*100+minute);
//		}
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DayOfW", Order=1, GroupName="Parameters")]
		public int DayOfW
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgSize
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
		private BiggestBarOfDay[] cacheBiggestBarOfDay;
		public BiggestBarOfDay BiggestBarOfDay(int dayOfW)
		{
			return BiggestBarOfDay(Input, dayOfW);
		}

		public BiggestBarOfDay BiggestBarOfDay(ISeries<double> input, int dayOfW)
		{
			if (cacheBiggestBarOfDay != null)
				for (int idx = 0; idx < cacheBiggestBarOfDay.Length; idx++)
					if (cacheBiggestBarOfDay[idx] != null && cacheBiggestBarOfDay[idx].DayOfW == dayOfW && cacheBiggestBarOfDay[idx].EqualsInput(input))
						return cacheBiggestBarOfDay[idx];
			return CacheIndicator<BiggestBarOfDay>(new BiggestBarOfDay(){ DayOfW = dayOfW }, input, ref cacheBiggestBarOfDay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BiggestBarOfDay BiggestBarOfDay(int dayOfW)
		{
			return indicator.BiggestBarOfDay(Input, dayOfW);
		}

		public Indicators.BiggestBarOfDay BiggestBarOfDay(ISeries<double> input , int dayOfW)
		{
			return indicator.BiggestBarOfDay(input, dayOfW);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BiggestBarOfDay BiggestBarOfDay(int dayOfW)
		{
			return indicator.BiggestBarOfDay(Input, dayOfW);
		}

		public Indicators.BiggestBarOfDay BiggestBarOfDay(ISeries<double> input , int dayOfW)
		{
			return indicator.BiggestBarOfDay(input, dayOfW);
		}
	}
}

#endregion
