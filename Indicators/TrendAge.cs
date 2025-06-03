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
	public class TrendAge : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TrendAge";
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
				pCycleDotPeriod					= 7;
				AddPlot(Brushes.Orange, "TA");
				AddPlot(Brushes.Blue, "TA2");
			}
			else if (State == State.Configure)
			{
			}
		}
		private SortedDictionary<int,double[]> cci = new SortedDictionary<int,double[]>();
		private char LastSignalType = ' ';
		private int SignalABar = 0;
		protected override void OnBarUpdate()
		{
			if(CurrentBar < pCycleDotPeriod * 2) return;

			CalculateTrendAge(CurrentBar, Closes[0], cci);
			if(cci.ContainsKey(CurrentBar)){
				TA[0] = cci[CurrentBar][0];
				TA2[0] = cci[CurrentBar][1];
			}
		}
		
		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			Alert(id,prio,msg,wav,rearmSeconds,bkgBrush, foregroundBrush);
		}
		
		private void CalculateTrendAge(int cb, ISeries<double> Prices, SortedDictionary<int,double[]> cci){
			#region -- CalculateTrendAge --
			int period = pCycleDotPeriod * 2;
			double sma14 = 0;
			for(int j = cb-period; j<cb; j++)
				sma14 = sma14+Prices.GetValueAt(j);
			sma14 = sma14/(period);
			double mean = 0;
			for (int j = cb-period; j<cb; j++)
				mean += Math.Abs(Prices.GetValueAt(j) - sma14);
			cci[cb] = new double[3];
		    cci[cb][0] = (Prices.GetValueAt(cb) - sma14) / (mean.ApproxCompare(0) == 0 ? 1 : (0.015 * (mean / pCycleDotPeriod/2) ) );
			mean = 0;
			for (int j = cb-(period+1); j<cb; j++){
				if(cci.ContainsKey(j)) mean += cci[j][0];
			}
			if(SignalABar == 0 && State == State.Realtime) SignalABar = cb-4;//first bar after a chart refresh is ignored...signals can only come on subsequent bars
			cci[cb][1] = mean/(period+1);//the [1] element of the double[] is the SMA of the [0] element
//Print(cci[cb][0].ToString("0.00")+"  "+cci[cb][1].ToString("0.00"));
			if(cci[cb][1] < -200) {
				cci[cb][2] = 2;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType=='B') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new long signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'B') {
						SignalABar = cb;
						LastSignalType = 'B';
//BackBrushes[CurrentBar-cb]=Brushes.Cyan;
						myAlert(string.Format("Buy{0}",cb.ToString()), Priority.High, "Buy #2", "", 0, Brushes.Black,Brushes.Lime);
					}
				}
			}
			else if(cci[cb][1] < -100) {
				cci[cb][2] = 1;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType=='B') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new long signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'B') {
						SignalABar = cb;
						LastSignalType = 'B';
//BackBrushes[CurrentBar-cb]=Brushes.Cyan;
						myAlert(string.Format("Buy{0}",cb.ToString()), Priority.High, "Buy #1", "", 0, Brushes.Black,Brushes.Lime);
					}
				}
			}
			else if(cci[cb][1] > 200) {
				cci[cb][2] = -2;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType=='S') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new short signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'S') {
						SignalABar = cb;
						LastSignalType = 'S';
//BackBrushes[CurrentBar-cb]=Brushes.Pink;
						myAlert(string.Format("Sell{0}",cb.ToString()), Priority.High, "Sell", "", 0, Brushes.Black,Brushes.Magenta);
					}
				}
			}
			else if(cci[cb][1] > 100) {
				cci[cb][2] = -1;
				if(State == State.Realtime){
					if(SignalABar < cb-5 && LastSignalType=='S') LastSignalType= ' ';//if we haven't had a signal in 5 bars, then reset the LastSignalType to nil.  This permits new short signals to generate alerts
					if(SignalABar != CurrentBar && LastSignalType != 'S') {
						SignalABar = cb;
						LastSignalType = 'S';
//BackBrushes[CurrentBar-cb]=Brushes.Pink;
						myAlert(string.Format("Sell{0}",cb.ToString()), Priority.High, "Sell #1", "", 0, Brushes.Black,Brushes.Magenta);
					}
				}
			}
			else cci[cb][2] = 0;
			#endregion
		}


		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Cycle Dot Period", Order=10, GroupName="Parameters")]
		public int pCycleDotPeriod
		{ get; set; }
		#endregion

		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TA
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TA2
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TrendAge[] cacheTrendAge;
		public TrendAge TrendAge(int pCycleDotPeriod)
		{
			return TrendAge(Input, pCycleDotPeriod);
		}

		public TrendAge TrendAge(ISeries<double> input, int pCycleDotPeriod)
		{
			if (cacheTrendAge != null)
				for (int idx = 0; idx < cacheTrendAge.Length; idx++)
					if (cacheTrendAge[idx] != null && cacheTrendAge[idx].pCycleDotPeriod == pCycleDotPeriod && cacheTrendAge[idx].EqualsInput(input))
						return cacheTrendAge[idx];
			return CacheIndicator<TrendAge>(new TrendAge(){ pCycleDotPeriod = pCycleDotPeriod }, input, ref cacheTrendAge);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TrendAge TrendAge(int pCycleDotPeriod)
		{
			return indicator.TrendAge(Input, pCycleDotPeriod);
		}

		public Indicators.TrendAge TrendAge(ISeries<double> input , int pCycleDotPeriod)
		{
			return indicator.TrendAge(input, pCycleDotPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TrendAge TrendAge(int pCycleDotPeriod)
		{
			return indicator.TrendAge(Input, pCycleDotPeriod);
		}

		public Indicators.TrendAge TrendAge(ISeries<double> input , int pCycleDotPeriod)
		{
			return indicator.TrendAge(input, pCycleDotPeriod);
		}
	}
}

#endregion
