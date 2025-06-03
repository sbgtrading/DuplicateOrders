//
// Copyright (C) 2024
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
    /// LowestPriceHistogram - Tracks when the lowest price of the day occurs and displays a histogram 
    /// of days with lowest price by 10-minute segments.
    /// </summary>
    public class LowestPriceHistogram : Indicator
    {
        private double lowestPrice;
        private DateTime lowestPriceTime;
        private bool newDay;
        private int[] segmentCounts;
        private int totalSegments;
        private Dictionary<DateTime, int> daySegmentMap; // Maps day to segment index
        private DateTime lastProcessedDay = DateTime.MinValue;
        private bool histogramUpdated = false;
        private int currentDaySegment = -1; // Segment where the low occurred for current day

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Tracks when the lowest price of the day occurs and displays a histogram of days with lowest price by 10-minute segments";
                Name = "LowestPriceHistogram";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                MinutesPerSegment = 10;
                
                // Add plot for the histogram - users can change color through UI
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "Histogram");
            }
            else if (State == State.Configure)
            {
                lowestPrice = double.MaxValue;
                lowestPriceTime = DateTime.MinValue;
                newDay = true;
                totalSegments = 24 * 60 / MinutesPerSegment; // Total number of segments in a day
                segmentCounts = new int[totalSegments];
                daySegmentMap = new Dictionary<DateTime, int>();
                currentDaySegment = -1;
            }
        }
void p(string s){return; Print(s);}
		DateTime currentDay=DateTime.MinValue;
        protected override void OnBarUpdate()
        {
			bool z = (Time[0].Day==11 || Time[0].Day==12) && Time[0].Month==8;
            // Check if we're in a new session
			if (Bars.IsFirstBarOfSession)
	            currentDay = Time[0].Date;
            if (currentDay != lastProcessedDay && Bars.IsFirstBarOfSession)
            {
                // Reset for new day
                lowestPrice = double.MaxValue;
                newDay = true;
                histogramUpdated = false;

				// Reset the current day's tracking
                currentDaySegment = -1;
                
                lastProcessedDay = currentDay;
            }

            // Track the lowest price of the day
            if (Low[0] < lowestPrice)
            {
				if(z)Print($"{Time[0].ToString()}   New low of {Low[0]} replaces prior low of {lowestPrice}");
                lowestPrice = Low[0];
                lowestPriceTime = Time[0];
                newDay = false;
                histogramUpdated = false;
                
                // Calculate which segment this low belongs to
                int minutesFromMidnight = lowestPriceTime.Hour * 60 + lowestPriceTime.Minute;
                currentDaySegment = minutesFromMidnight / MinutesPerSegment;
				if(z)Print($"      current segment now: {currentDaySegment}");
                
                // Update the day-segment map
                if (daySegmentMap.ContainsKey(currentDay))
                    daySegmentMap[currentDay] = currentDaySegment;
                else
                    daySegmentMap.Add(currentDay, currentDaySegment);
            }

            // If this is the last bar of the session or we need to update the histogram
            if (Bars.IsLastBarOfSession)// || !histogramUpdated)
            {
                // Reset segment counts
                Array.Clear(segmentCounts, 0, segmentCounts.Length);
                
                // Count days where the lowest price occurred in each segment
                foreach (var entry in daySegmentMap)
                {
                    int segmentIndex = entry.Value;
                    
                    // Ensure we don't go out of bounds
                    if (segmentIndex >= 0 && segmentIndex < segmentCounts.Length){
                        segmentCounts[segmentIndex]++;
						if(z)Print($"    {segmentIndex} is now {segmentCounts[segmentIndex]}");
					}
                }
                
                histogramUpdated = true;
            }
            
            // Always update the plot value for the current bar based on its segment
            // Find the segment for the current bar
            int currentMinutesFromMidnight = Time[0].Hour * 60 + Time[0].Minute;
            int currentSegmentIndex = currentMinutesFromMidnight / MinutesPerSegment;
            
            // Set the value to the count for this segment
            if (currentSegmentIndex < segmentCounts.Length)
                Value[0] = segmentCounts[currentSegmentIndex];
            else
                Value[0] = 0;
            
            // Print debug information
            if (Bars.IsLastBarOfSession)
            {
                p($"Day: {Time[0].Date:yyyy-MM-dd}");
                p($"Lowest price: {lowestPrice}");
                
                if (currentDaySegment >= 0)
                {
                    int startHour = (currentDaySegment * MinutesPerSegment) / 60;
                    int startMinute = (currentDaySegment * MinutesPerSegment) % 60;
                    int endHour = ((currentDaySegment + 1) * MinutesPerSegment) / 60;
                    int endMinute = ((currentDaySegment + 1) * MinutesPerSegment) % 60;
                    
                    string timeRange = $"{startHour:D2}:{startMinute:D2}-{endHour:D2}:{endMinute:D2}";
                        
                    p($"Today's low occurred during segment: {timeRange}");
                }
                
                p($"Total days analyzed: {daySegmentMap.Count}");
                
                for (int i = 0; i < segmentCounts.Length; i++)
                {
                    if (segmentCounts[i] > 0)
                    {
                        int startHour = (i * MinutesPerSegment) / 60;
                        int startMinute = (i * MinutesPerSegment) % 60;
                        int endHour = ((i + 1) * MinutesPerSegment) / 60;
                        int endMinute = ((i + 1) * MinutesPerSegment) % 60;
                        
                        string timeRange = $"{startHour:D2}:{startMinute:D2}-{endHour:D2}:{endMinute:D2}";
                            
                        p($"Segment {timeRange}: {segmentCounts[i]} days");
                    }
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, 60)]
        [Display(Name = "Minutes Per Segment", Description = "Number of minutes per segment", Order = 1, GroupName = "Parameters")]
        public int MinutesPerSegment
        { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LowestPriceHistogram[] cacheLowestPriceHistogram;
		public LowestPriceHistogram LowestPriceHistogram(int minutesPerSegment)
		{
			return LowestPriceHistogram(Input, minutesPerSegment);
		}

		public LowestPriceHistogram LowestPriceHistogram(ISeries<double> input, int minutesPerSegment)
		{
			if (cacheLowestPriceHistogram != null)
				for (int idx = 0; idx < cacheLowestPriceHistogram.Length; idx++)
					if (cacheLowestPriceHistogram[idx] != null && cacheLowestPriceHistogram[idx].MinutesPerSegment == minutesPerSegment && cacheLowestPriceHistogram[idx].EqualsInput(input))
						return cacheLowestPriceHistogram[idx];
			return CacheIndicator<LowestPriceHistogram>(new LowestPriceHistogram(){ MinutesPerSegment = minutesPerSegment }, input, ref cacheLowestPriceHistogram);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LowestPriceHistogram LowestPriceHistogram(int minutesPerSegment)
		{
			return indicator.LowestPriceHistogram(Input, minutesPerSegment);
		}

		public Indicators.LowestPriceHistogram LowestPriceHistogram(ISeries<double> input , int minutesPerSegment)
		{
			return indicator.LowestPriceHistogram(input, minutesPerSegment);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LowestPriceHistogram LowestPriceHistogram(int minutesPerSegment)
		{
			return indicator.LowestPriceHistogram(Input, minutesPerSegment);
		}

		public Indicators.LowestPriceHistogram LowestPriceHistogram(ISeries<double> input , int minutesPerSegment)
		{
			return indicator.LowestPriceHistogram(input, minutesPerSegment);
		}
	}
}

#endregion
