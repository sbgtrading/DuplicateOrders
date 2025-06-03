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
    /// HighestPriceHistogram - Tracks when the highest price of the day occurs and displays a histogram 
    /// of days with highest price by 10-minute segments.
    /// </summary>
    public class HighestPriceHistogram : Indicator
    {
        private double highestPrice;
        private DateTime highestPriceTime;
        private bool newDay;
        private int[] segmentCounts;
        private int totalSegments;
        private Dictionary<DateTime, int> daySegmentMap; // Maps day to segment index
        private DateTime lastProcessedDay = DateTime.MinValue;
        private bool histogramUpdated = false;
        private int currentDaySegment = -1; // Segment where the high occurred for current day

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Tracks when the highest price of the day occurs and displays a histogram of days with highest price by 10-minute segments";
                Name = "HighestPriceHistogram";
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
                AddPlot(new Stroke(Brushes.DarkGreen, 2), PlotStyle.Bar, "Histogram");
            }
            else if (State == State.Configure)
            {
                highestPrice = double.MinValue;
                highestPriceTime = DateTime.MinValue;
                newDay = true;
                totalSegments = 24 * 60 / MinutesPerSegment; // Total number of segments in a day
                segmentCounts = new int[totalSegments];
                daySegmentMap = new Dictionary<DateTime, int>();
                currentDaySegment = -1;
            }
        }

		DateTime currentDay = DateTime.MinValue;
        protected override void OnBarUpdate()
        {
            // Skip processing if we don't have enough bars
            if (CurrentBar < 1)
                return;

            // Check if we're in a new session
			if (Bars.IsFirstBarOfSession)
	            currentDay = Time[0].Date;
            if (currentDay != lastProcessedDay && Bars.IsFirstBarOfSession)
            {
                // Reset for new day
                highestPrice = double.MinValue;
                newDay = true;
                histogramUpdated = false;
                
                // Reset the current day's tracking
                currentDaySegment = -1;
                
                lastProcessedDay = currentDay;
            }

            // Track the highest price of the day
            if (High[0] > highestPrice)
            {
                highestPrice = High[0];
                highestPriceTime = Time[0];
                newDay = false;
                histogramUpdated = false;
                
                // Calculate which segment this high belongs to
                int minutesFromMidnight = Time[0].Hour * 60 + Time[0].Minute;
                currentDaySegment = minutesFromMidnight / MinutesPerSegment;
                
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
                
                // Count days where the highest price occurred in each segment
                foreach (var entry in daySegmentMap)
                {
                    int segmentIndex = entry.Value;
                    
                    // Ensure we don't go out of bounds
                    if (segmentIndex >= 0 && segmentIndex < segmentCounts.Length)
                        segmentCounts[segmentIndex]++;
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
                Print($"Day: {Time[0].Date:yyyy-MM-dd}");
                Print($"Highest price: {highestPrice}");
                
                if (currentDaySegment >= 0)
                {
                    int startHour = (currentDaySegment * MinutesPerSegment) / 60;
                    int startMinute = (currentDaySegment * MinutesPerSegment) % 60;
                    int endHour = ((currentDaySegment + 1) * MinutesPerSegment) / 60;
                    int endMinute = ((currentDaySegment + 1) * MinutesPerSegment) % 60;
                    
                    string timeRange = $"{startHour:D2}:{startMinute:D2}-{endHour:D2}:{endMinute:D2}";
                        
                    Print($"Today's high occurred during segment: {timeRange}");
                }
                
                Print($"Total days analyzed: {daySegmentMap.Count}");
                
                for (int i = 0; i < segmentCounts.Length; i++)
                {
                    if (segmentCounts[i] > 0)
                    {
                        int startHour = (i * MinutesPerSegment) / 60;
                        int startMinute = (i * MinutesPerSegment) % 60;
                        int endHour = ((i + 1) * MinutesPerSegment) / 60;
                        int endMinute = ((i + 1) * MinutesPerSegment) % 60;
                        
                        string timeRange = $"{startHour:D2}:{startMinute:D2}-{endHour:D2}:{endMinute:D2}";
                            
                        Print($"Segment {timeRange}: {segmentCounts[i]} days");
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
		private HighestPriceHistogram[] cacheHighestPriceHistogram;
		public HighestPriceHistogram HighestPriceHistogram(int minutesPerSegment)
		{
			return HighestPriceHistogram(Input, minutesPerSegment);
		}

		public HighestPriceHistogram HighestPriceHistogram(ISeries<double> input, int minutesPerSegment)
		{
			if (cacheHighestPriceHistogram != null)
				for (int idx = 0; idx < cacheHighestPriceHistogram.Length; idx++)
					if (cacheHighestPriceHistogram[idx] != null && cacheHighestPriceHistogram[idx].MinutesPerSegment == minutesPerSegment && cacheHighestPriceHistogram[idx].EqualsInput(input))
						return cacheHighestPriceHistogram[idx];
			return CacheIndicator<HighestPriceHistogram>(new HighestPriceHistogram(){ MinutesPerSegment = minutesPerSegment }, input, ref cacheHighestPriceHistogram);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HighestPriceHistogram HighestPriceHistogram(int minutesPerSegment)
		{
			return indicator.HighestPriceHistogram(Input, minutesPerSegment);
		}

		public Indicators.HighestPriceHistogram HighestPriceHistogram(ISeries<double> input , int minutesPerSegment)
		{
			return indicator.HighestPriceHistogram(input, minutesPerSegment);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HighestPriceHistogram HighestPriceHistogram(int minutesPerSegment)
		{
			return indicator.HighestPriceHistogram(Input, minutesPerSegment);
		}

		public Indicators.HighestPriceHistogram HighestPriceHistogram(ISeries<double> input , int minutesPerSegment)
		{
			return indicator.HighestPriceHistogram(input, minutesPerSegment);
		}
	}
}

#endregion
