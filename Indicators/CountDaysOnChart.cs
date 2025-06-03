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
	public class CountDaysOnChart : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CountDaysOnChart";
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
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.DataLoaded)
			{
//				var str = "";
//                foreach (System.Net.NetworkInformation.NetworkInterface nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
//                {
//                    if (nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback && nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
//                    {
//                        str = (nic.GetPhysicalAddress().ToString());
//                    }
//                }
//	            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
//	            {
//	                byte[] inputBytes = Encoding.UTF8.GetBytes(str);
//	                byte[] hashBytes = md5.ComputeHash(inputBytes);
	
//	                // Convert the byte array to a hexadecimal string
//	                StringBuilder sb = new StringBuilder();
//	                foreach (byte b in hashBytes)
//	                {
//	                    sb.Append(b.ToString("x2")); // "x2" formats each byte as a two-character hexadecimal
//	                }
	
//					ClearOutputWindow();
//	                Print("Key: "+sb.ToString());
//	            }
			}
		}

		private SortedDictionary<DateTime,int> days = new SortedDictionary<DateTime,int>();
		DateTime t;
		protected override void OnBarUpdate()
		{
			if(CurrentBar<3) return;
			t = Times[0][0].Date;
			if(days.Count==0){
				while(t != DateTime.Now.Date){
					days[t] = 0;
					t = t.AddDays(1);
				}
			}
			t = Times[0][0].Date;
			if(Times[0][0].Day!=Times[0][1].Day){
				days[t] = 1;
			}else{
				int v = days[t]+1;//bar count for that day is increased
				days[t] = v;
			}
			if(CurrentBar == Bars.Count-3){
				var str = new StringBuilder(days.Count+1);
				str.AppendLine(".");
				str.AppendLine("Count days on chart indicator v1.0");
				foreach(var d in days){
					str.AppendLine(d.Key.ToShortDateString()+" ("+d.Key.DayOfWeek.ToString().Substring(0,2)+"):  \t"+d.Value+"-bars");
				}
				Print(str.ToString());
				Draw.TextFixed(this,"info",str.ToString(),TextPosition.TopLeft, Brushes.White, new SimpleFont("Courier",12), Brushes.Black, Brushes.Black,100);
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CountDaysOnChart[] cacheCountDaysOnChart;
		public CountDaysOnChart CountDaysOnChart()
		{
			return CountDaysOnChart(Input);
		}

		public CountDaysOnChart CountDaysOnChart(ISeries<double> input)
		{
			if (cacheCountDaysOnChart != null)
				for (int idx = 0; idx < cacheCountDaysOnChart.Length; idx++)
					if (cacheCountDaysOnChart[idx] != null &&  cacheCountDaysOnChart[idx].EqualsInput(input))
						return cacheCountDaysOnChart[idx];
			return CacheIndicator<CountDaysOnChart>(new CountDaysOnChart(), input, ref cacheCountDaysOnChart);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CountDaysOnChart CountDaysOnChart()
		{
			return indicator.CountDaysOnChart(Input);
		}

		public Indicators.CountDaysOnChart CountDaysOnChart(ISeries<double> input )
		{
			return indicator.CountDaysOnChart(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CountDaysOnChart CountDaysOnChart()
		{
			return indicator.CountDaysOnChart(Input);
		}

		public Indicators.CountDaysOnChart CountDaysOnChart(ISeries<double> input )
		{
			return indicator.CountDaysOnChart(input);
		}
	}
}

#endregion
