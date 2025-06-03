//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The VROC (Volume Rate-of-Change) shows whether or not a volume trend is
	/// developing in either an up or down direction. It is similar to the ROC
	/// indicator, but is applied to volume instead.
	/// </summary>
	public class Test : Indicator
	{
		private Series<double>	smaVolume;
		private SMA				sma;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionVROC;
				Name						= "Add Lines Test";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= false;
				DrawOnPricePanel			= false;
				Period						= 14;
				Smooth						= 3;

				AddPlot(Brushes.Goldenrod,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameVROC);
				AddLine(Brushes.DarkGray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
			}

			else if (State == State.DataLoaded)
			{
	            string filename = @"C:\Users\sbgtr\Documents\Raptor 3\RaptorDemo_13Nov2024\1113 secondhalf.srt";
	            int incrementValue = 699;
	
	            string[] lines = System.IO.File.ReadAllLines(filename);
	
	            var newLines = new List<string>();
	            int? currentGroupNumber = null;
	
				TimeSpan ThirtyMinutes = new TimeSpan(0,30,0);
				int count = 0;
	            foreach (string line in lines)
	            {
	                if (line.Trim().Length>0 && int.TryParse(line.Trim(), out int groupNumber))
	                {
	                    currentGroupNumber = groupNumber + incrementValue;
	                    if (currentGroupNumber.HasValue)
	                    {
	                        newLines.Add(currentGroupNumber.Value.ToString());
							Print("Number increased to "+currentGroupNumber.Value.ToString());
	                    }
	                }
	                else if (line.Contains("-->"))//(count-1) % 4 == 0)
	                {
						var times = line.Split(new string[]{"-->"}, StringSplitOptions.RemoveEmptyEntries);
						var h = times[0].Split(new char[]{':',','})[0];
						var m = times[0].Split(new char[]{':',','})[1];
						var s = times[0].Split(new char[]{':',','})[2];
						var ms1 = times[0].Split(new char[]{':',','})[3];
						var ts1 = new TimeSpan(Convert.ToInt32(h),Convert.ToInt32(m),Convert.ToInt32(s),Convert.ToInt32(ms1));
						h = times[1].Split(new char[]{':',','})[0];
						m = times[1].Split(new char[]{':',','})[1];
						s = times[1].Split(new char[]{':',','})[2];
						var ms2 = times[1].Split(new char[]{':',','})[3];
						var ts2 = new TimeSpan(Convert.ToInt32(h),Convert.ToInt32(m),Convert.ToInt32(s),Convert.ToInt32(ms2));
						ts1 = ts1.Add(ThirtyMinutes);
						ts2 = ts2.Add(ThirtyMinutes);
                        newLines.Add(ts1.ToString(@"hh\:mm\:ss")+","+ms1+" --> "+ts2.ToString(@"hh\:mm\:ss")+","+ms2);
						Print("times: "+newLines.Last());
	                }
	                else
	                {
	                    newLines.Add(line);
	                }
	            }
	
	            filename = @"C:\Users\sbgtr\Documents\Raptor 3\RaptorDemo_13Nov2024\1113 secondhalf_shifted.srt";
	            System.IO.File.WriteAllLines(filename, newLines);

			}
			else if (State == State.Historical)
			{
			}
		}

		protected override void OnBarUpdate()
		{
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Smooth
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Test[] cacheTest;
		public Test Test(int period, int smooth)
		{
			return Test(Input, period, smooth);
		}

		public Test Test(ISeries<double> input, int period, int smooth)
		{
			if (cacheTest != null)
				for (int idx = 0; idx < cacheTest.Length; idx++)
					if (cacheTest[idx] != null && cacheTest[idx].Period == period && cacheTest[idx].Smooth == smooth && cacheTest[idx].EqualsInput(input))
						return cacheTest[idx];
			return CacheIndicator<Test>(new Test(){ Period = period, Smooth = smooth }, input, ref cacheTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Test Test(int period, int smooth)
		{
			return indicator.Test(Input, period, smooth);
		}

		public Indicators.Test Test(ISeries<double> input , int period, int smooth)
		{
			return indicator.Test(input, period, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Test Test(int period, int smooth)
		{
			return indicator.Test(Input, period, smooth);
		}

		public Indicators.Test Test(ISeries<double> input , int period, int smooth)
		{
			return indicator.Test(input, period, smooth);
		}
	}
}

#endregion
