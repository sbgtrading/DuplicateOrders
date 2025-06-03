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

namespace CustomEnums
{
	public enum FilterType
	{
		Price,
		Gaussian_Filter,
		Both,
		None
	}
}

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class LoxxGaussianFilter : Indicator
	{		
		// Create series that need to be tracked over time
		private Series<double> filtPrice;		// Track the filtered close price over time
		private Series<double> nPoleResult;		// Track nPole runction over time
		private Series<double> gfilt;			// Track filter series for recursion
		
		// Create an object to cache variables
		private double[,] cachedCoeffs;			// Cache the coefficients since they are only calculated once per the user variables
		private double cachedAlpha;				// Cache an alpha value since it is only set once per the user variables
		
		// Create an object to hold the FilterType selection
		private CustomEnums.FilterType FilterType;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Loxx gaussian filter converted from Tradingview.";
				Name										= "!AB Loxx Gaussian Filter";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.				
				IsSuspendedWhileInactive					= true;
				IsAutoScale									= false;
				FilterType									= CustomEnums.FilterType.Gaussian_Filter;
				PeriodLength								= 25;
				NumberOfPoles								= 5;
				FilterPeriod								= 10;
				FilterDeviations							= 1;
				UpBrush										= new SolidColorBrush(Color.FromArgb(155, 75, 155, 75));
				DownBrush									= new SolidColorBrush(Color.FromArgb(155, 224, 117, 117));
				
				// Create the plot object
				AddPlot(Brushes.Black, "Loxx N-Pole Gaussian Filter");
				Plots[0].Width = 3;
								
				// Freeze the brushes - this may not be necessary since the brushes are created in the set defaults section.
				UpBrush.Freeze();
				DownBrush.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				//Create series that require values to be tracked
				filtPrice				= new Series<double>(this);
				nPoleResult				= new Series<double>(this);
				
				//Cache variables - none of these values change unless the user selects new parameters, so they can be cached once
				cachedAlpha				= fn_Alpha(PeriodLength, NumberOfPoles);	// Calculate before cachedCoeffs since used in fn_MakeCoeffs
				cachedCoeffs			= fn_MakeCoeffs(PeriodLength, NumberOfPoles, cachedAlpha);	// Requires alpha to be calculated first
			}
		}
		
		#region CustomFunctions
		//NZ - Returns 0 if a value is null
		private double fn_Nz(double iValue, double iDefaultvalue = 0)
		{
			return iValue == 0 || Double.IsNaN(iValue) ? iDefaultvalue : iValue;
		}
		
		//FACTORIAL - Calculates the factorial (e.x. 5! = 120)
		public double fn_Factorial(int iNum)
		{
			return iNum == 0 ? 1 : iNum * fn_Factorial(iNum - 1);
		}
		
		//ALPHA - Calculates a smoothing factor
		public double fn_Alpha(int iPeriodLength, int iNumberOfPoles)
		{
			double w = 2.0 * Math.PI / iPeriodLength;
			double b = (1.0 - Math.Cos(w)) / (Math.Pow(Math.Sqrt(2.0), 2.0 / iNumberOfPoles) - 1.0);
			double a = -b + Math.Sqrt(b * b + 2.0 * b);
			return a;
		}
		
		//COEFFICIENTS - Generates a set of smoothing coefficients in an array format
		public double[,] fn_MakeCoeffs(int iPeriodLength, int iNumberOfPoles, double iAlpha)
		{
			double[,] coeffs = new double[iNumberOfPoles + 1, 3];
			double a = iAlpha;
		
			for (int r = 0; r <= iNumberOfPoles; r++)
			{
				double outVal = fn_Nz(fn_Factorial(iNumberOfPoles) / (fn_Factorial(iNumberOfPoles - r) * fn_Factorial(r)), 1);
				coeffs[r, 0] = outVal;
				coeffs[r, 1] = Math.Pow(a, r);
				coeffs[r, 2] = Math.Pow(1.0 - a, r);
			}
		
			return coeffs;
		}
		
		//N-POLE GAUSSIAN FILTER - Filters price based on the smoothing coefficients
		public double fn_Npolegf(ISeries<double> iSrc, int iPeriodLength, int iNumberOfPoles, double[,] iCoeffs, ISeries<double> iHistNpole)
		{
			double gfilt = iSrc[0] * iCoeffs[iNumberOfPoles, 1];
			int sign = 1;			
			for (int r = 1; r <= iNumberOfPoles; r++)
			{
				gfilt += sign * iCoeffs[r, 0] * iCoeffs[r, 2] * fn_Nz(iHistNpole[r]);
				sign *= -1;
			}	
			return gfilt;
		}
		
		//FILTERED VALUE - Filters price based on its movement relative to a dynamic standard deviation
		public double fn_Filt(ISeries<double> iSrc, int iFilterPeriod, int iFilterDeviations, double iPrevPrice)
		{
			// Set price values for transparency
			double curPrice = iSrc[0];
			double prevPrice = fn_Nz(iPrevPrice);			
			
			// Create an instance of the StdDev indicator with the parameters passed to the function
    		var stdDev = StdDev(iSrc, iFilterPeriod);
			
			// Calculate the value of the number of standard deviations x the current standard deviation value
			double stdDevRange = iFilterDeviations * stdDev[0];
			
			// If the difference between iSrc[0] and iSrc[1] is larger than the standard deviation,
			// then return the current price, otherwise return the previous price			
			double priceDiff = Math.Abs(curPrice - prevPrice);
			double filteredValue = fn_Nz(curPrice);
			if (priceDiff < stdDevRange)
			{
				filteredValue = prevPrice;				
			}
			
			// Return the appropriate value
			return filteredValue;
		}
		#endregion
		
		
		//MAIN BAR UPDATE LOGIC
		protected override void OnBarUpdate()
		{			
			// Check that we have enough bars on our chart before processing
			if(CurrentBar < Math.Max(PeriodLength, FilterPeriod) + 1) return;
						
			// Determine filtered price based on FilterPrice selection
			if (FilterType == CustomEnums.FilterType.Both || (FilterType == CustomEnums.FilterType.Price && FilterPeriod > 0))
			{
				// Use this if the price (close price) needs to be filtered
			    filtPrice[0] = fn_Filt(Close, FilterPeriod, FilterDeviations, filtPrice[1]);
			}
			else
			{
				// Just return the close price if it doesn't
			    filtPrice[0] = Close[0];
			}	
						
			// Determine the N-Pole result
			nPoleResult[0] = fn_Npolegf(filtPrice, PeriodLength, NumberOfPoles, cachedCoeffs, nPoleResult);
			
			// Determine whether to filter the N-Pole filtered result or not
			if (FilterType == CustomEnums.FilterType.Both || (FilterType == CustomEnums.FilterType.Gaussian_Filter && FilterPeriod > 0))
			{
				// Use this if the nPoleResult needs to be filtered
			    nPoleGF[0] = fn_Filt(nPoleResult, FilterPeriod, FilterDeviations, nPoleGF[1]);
			}
			else
			{
				// Just return the nPoleResult if it doesn't
			    nPoleGF[0] = nPoleResult[0];
			}
			
			// Color the Plot
			if (nPoleGF[0] > nPoleGF[1])
			{
				// Up brush if the plot is ascending
				PlotBrushes[0][0] = UpBrush;
			}
			else if (nPoleGF[0] < nPoleGF[1])
			{
				// Down brush if the plot is descending
				PlotBrushes[0][0] = DownBrush;
			} else {
				// Use the prior brush if it is not changing
				PlotBrushes[0][0] = PlotBrushes[0][1];
			}
		}		

		#region Properties
		// Enum Properties (FilterType)
		[NinjaScriptProperty]		
		[Display(Name="Filter Type", Description="Filter type", Order=1, GroupName="Parameters")]
		public CustomEnums.FilterType FilterTypeP
		{
			get { return FilterType; }
			set { FilterType = value; }
		}
		
		// Numeric Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period Length", Description="Period length", Order=2, GroupName="Parameters")]
		public int PeriodLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 9)]
		[Display(Name="Number Of Poles", Description="Number of poles", Order=3, GroupName="Parameters")]
		public int NumberOfPoles
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 3)]
		[Display(Name="Filter Deviations", Description="Filter deviations", Order=4, GroupName="Parameters")]
		public int FilterDeviations
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Filter Period", Description="Filter period", Order=5, GroupName="Parameters")]
		public int FilterPeriod
		{ get; set; }
		
		[XmlIgnore]
        [Display(Name = "Up Brush", Description = "Color for Up Brush", Order = 6, GroupName = "Parameters")]
        public Brush UpBrush
		{ get; set; }
		

        // Brush Properties
		[XmlIgnore]
        [Display(Name = "Down Brush", Description = "Color for Down Brush", Order = 7, GroupName = "Parameters")]
        public Brush DownBrush
		{ get; set; }

		
		// Plot data series
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> nPoleGF
		{ get { return Values[0]; } }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LoxxGaussianFilter[] cacheLoxxGaussianFilter;
		public LoxxGaussianFilter LoxxGaussianFilter(CustomEnums.FilterType filterTypeP, int periodLength, int numberOfPoles, int filterDeviations, int filterPeriod)
		{
			return LoxxGaussianFilter(Input, filterTypeP, periodLength, numberOfPoles, filterDeviations, filterPeriod);
		}

		public LoxxGaussianFilter LoxxGaussianFilter(ISeries<double> input, CustomEnums.FilterType filterTypeP, int periodLength, int numberOfPoles, int filterDeviations, int filterPeriod)
		{
			if (cacheLoxxGaussianFilter != null)
				for (int idx = 0; idx < cacheLoxxGaussianFilter.Length; idx++)
					if (cacheLoxxGaussianFilter[idx] != null && cacheLoxxGaussianFilter[idx].FilterTypeP == filterTypeP && cacheLoxxGaussianFilter[idx].PeriodLength == periodLength && cacheLoxxGaussianFilter[idx].NumberOfPoles == numberOfPoles && cacheLoxxGaussianFilter[idx].FilterDeviations == filterDeviations && cacheLoxxGaussianFilter[idx].FilterPeriod == filterPeriod && cacheLoxxGaussianFilter[idx].EqualsInput(input))
						return cacheLoxxGaussianFilter[idx];
			return CacheIndicator<LoxxGaussianFilter>(new LoxxGaussianFilter(){ FilterTypeP = filterTypeP, PeriodLength = periodLength, NumberOfPoles = numberOfPoles, FilterDeviations = filterDeviations, FilterPeriod = filterPeriod }, input, ref cacheLoxxGaussianFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LoxxGaussianFilter LoxxGaussianFilter(CustomEnums.FilterType filterTypeP, int periodLength, int numberOfPoles, int filterDeviations, int filterPeriod)
		{
			return indicator.LoxxGaussianFilter(Input, filterTypeP, periodLength, numberOfPoles, filterDeviations, filterPeriod);
		}

		public Indicators.LoxxGaussianFilter LoxxGaussianFilter(ISeries<double> input , CustomEnums.FilterType filterTypeP, int periodLength, int numberOfPoles, int filterDeviations, int filterPeriod)
		{
			return indicator.LoxxGaussianFilter(input, filterTypeP, periodLength, numberOfPoles, filterDeviations, filterPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LoxxGaussianFilter LoxxGaussianFilter(CustomEnums.FilterType filterTypeP, int periodLength, int numberOfPoles, int filterDeviations, int filterPeriod)
		{
			return indicator.LoxxGaussianFilter(Input, filterTypeP, periodLength, numberOfPoles, filterDeviations, filterPeriod);
		}

		public Indicators.LoxxGaussianFilter LoxxGaussianFilter(ISeries<double> input , CustomEnums.FilterType filterTypeP, int periodLength, int numberOfPoles, int filterDeviations, int filterPeriod)
		{
			return indicator.LoxxGaussianFilter(input, filterTypeP, periodLength, numberOfPoles, filterDeviations, filterPeriod);
		}
	}
}

#endregion
