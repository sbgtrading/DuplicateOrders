#region Using declarations
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript;
using System.Windows.Media;
#endregion
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public class IW_Chandelier_Stop : Indicator
    {
        private double rangeMultiple = 3.000; // Default setting for RangeMultiple
        private int eMALength = 14; // Default setting for EMALength
		private double wt, avrng1;
		private bool RunInit = true;
		public Series<double> TrueRange;
		public Series<double> hbound, lbound;

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AddPlot(new Stroke(Brushes.DeepPink,1), PlotStyle.Line, "Hi");
				AddPlot(new Stroke(Brushes.DeepPink,1), PlotStyle.Line, "Lo");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "Dir");
				Calculate=Calculate.OnBarClose;
				IsOverlay=true;
				wt = 2.0/(eMALength + 1.0); // EMA weight
			}
			if (State == State.DataLoaded)
	        {
				TrueRange = new Series<double>(this);
				hbound = new Series<double>(this);
				lbound = new Series<double>(this);
			}
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			Calculate=Calculate.OnBarClose;
int line = 45;
			double TH=0, TL=0, stopdistance=0;
			try{
			if (RunInit || CurrentBar<4) {
				RunInit = false;
				hbound[0] = TH = High[0];
				lbound[0] = TL = Low[0];
				TrueRange[0] = TH-TL;
				Dir[0] = 0;
//CalculateOnBarClose=true;//WILLNOTWORKIFSETTOFALSE
			}
			else {
				TH = Math.Max(Close[1], High[0]);  // true high
				TL = Math.Min(Close[1], Low[0]);   // true low
				TrueRange[0] = TH-TL;
//				TrueRange[0] = wt * (TH-TL - TrueRange[1]) + TrueRange[1]; // EMA of true range
			}
			stopdistance = rangeMultiple * EMA(TrueRange,eMALength)[0];
			if(CurrentBar>=3){
				if(IsFirstTickOfBar) {
					Dir[0] = Dir[1];
//					hbound[0] = hbound[1];
//					hbound[0] = Math.Min(hbound[1], TL+stopdistance);
//					lbound[0] = lbound[1];
//					lbound[0] = Math.Max(lbound[1], TH-stopdistance);
				}
				hbound[0] = Math.Min(hbound[1], TL+stopdistance);
				lbound[0] = Math.Max(lbound[1], TH-stopdistance);
				if(Dir[0]>0) hbound[0] = TL+stopdistance;
				if(Dir[0]<0) lbound[0] = TH-stopdistance;
				if(this.Calculate != Calculate.OnBarClose || !IsFirstTickOfBar){
					if (Dir[0]<=0 && High[0] >= hbound[0]) {
//						lbound[0] = TH - stopdistance;
						Dir[0] = 1; // market is up
					}
//					else 
//						lbound[0] = Math.Max(lbound[1], TH-stopdistance);

					if (Dir[0]>=0 && Low[0] <= lbound[0]) {
//						hbound[0] = TL + stopdistance;
						Dir[0] = -1; // market is down
					}
//					else 
//						hbound[0] = Math.Min(hbound[1], TL+stopdistance);
				}

//Print(Time[0].ToString()+"  dir: "+Dir[0].ToString()+"  HB: "+hbound.ToString()+"  LB: "+lbound.ToString());
				if(Dir[0]<0) {
					ChandelierHi[0]=(hbound[0]);
					ChandelierLo.Reset();
//BackBrush = Color.Pink;
				}
				if(Dir[0]>0) {
//BackBrush = Color.Cyan;
					ChandelierLo[0]=(lbound[0]);
					ChandelierHi.Reset();
				}
			}
			}catch(Exception err){Print("IW_Chandelier_Stop: "+err.ToString());}			
        }

        #region Plots
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> ChandelierHi
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> ChandelierLo
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Dir
        {
            get { return Values[2]; }
        }
		#endregion

        #region Properties
        [Description("Size of stop in average range multiples")]
        [Category("Parameters")]
		[NinjaScriptProperty]
        public double RangeMultiple
        {
            get { return rangeMultiple; }
            set { rangeMultiple = Math.Max(0.000, value); }
        }

        [Description("Length of moving average of ranges")]
        [Category("Parameters")]
		[NinjaScriptProperty]
        public int EMALength
        {
            get { return eMALength; }
            set { eMALength = Math.Max(1, value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_Chandelier_Stop[] cacheIW_Chandelier_Stop;
		public IW_Chandelier_Stop IW_Chandelier_Stop(double rangeMultiple, int eMALength)
		{
			return IW_Chandelier_Stop(Input, rangeMultiple, eMALength);
		}

		public IW_Chandelier_Stop IW_Chandelier_Stop(ISeries<double> input, double rangeMultiple, int eMALength)
		{
			if (cacheIW_Chandelier_Stop != null)
				for (int idx = 0; idx < cacheIW_Chandelier_Stop.Length; idx++)
					if (cacheIW_Chandelier_Stop[idx] != null && cacheIW_Chandelier_Stop[idx].RangeMultiple == rangeMultiple && cacheIW_Chandelier_Stop[idx].EMALength == eMALength && cacheIW_Chandelier_Stop[idx].EqualsInput(input))
						return cacheIW_Chandelier_Stop[idx];
			return CacheIndicator<IW_Chandelier_Stop>(new IW_Chandelier_Stop(){ RangeMultiple = rangeMultiple, EMALength = eMALength }, input, ref cacheIW_Chandelier_Stop);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_Chandelier_Stop IW_Chandelier_Stop(double rangeMultiple, int eMALength)
		{
			return indicator.IW_Chandelier_Stop(Input, rangeMultiple, eMALength);
		}

		public Indicators.IW_Chandelier_Stop IW_Chandelier_Stop(ISeries<double> input , double rangeMultiple, int eMALength)
		{
			return indicator.IW_Chandelier_Stop(input, rangeMultiple, eMALength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_Chandelier_Stop IW_Chandelier_Stop(double rangeMultiple, int eMALength)
		{
			return indicator.IW_Chandelier_Stop(Input, rangeMultiple, eMALength);
		}

		public Indicators.IW_Chandelier_Stop IW_Chandelier_Stop(ISeries<double> input , double rangeMultiple, int eMALength)
		{
			return indicator.IW_Chandelier_Stop(input, rangeMultiple, eMALength);
		}
	}
}

#endregion
