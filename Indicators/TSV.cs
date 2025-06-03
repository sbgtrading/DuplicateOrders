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

public enum TSV_AverageType
{
	none, simple, exponential, weighted
}

public enum myTSV
{
	default_stroke, color_tsv, hide_tsv
}

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class TSV : Indicator
	{

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Time Segmented Volume";
				Name										= "TSV";
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
				IsSuspendedWhileInactive					= false;
				Up					= Brushes.Green;
				Dn					= Brushes.Red;
				pDaysLookback = 5;

				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "TSV");
				AddPlot(Brushes.Gray, "Average");
				AddLine(Brushes.Gainsboro, 0, "Zero Line");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
			}
		}

		private SortedDictionary<int,List<double>> Volumes = new SortedDictionary<int,List<double>>();
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;

			var t0 = ToTime(Times[0][0]);
			if(Volumes.ContainsKey(t0))
				Volumes[t0].Add(Volume[0]);
			else
				Volumes[t0] = new List<double>(){Volume[0]};
			while(Volumes[t0].Count > pDaysLookback) Volumes[t0].RemoveAt(0);
			double v = Volumes[t0].Average();
			Avg[0] = v;
			Default[0] = Volume[0] - v;

			switch (tsv_sel)
			{
				case myTSV.color_tsv:
					PlotBrushes[0][0] = Default[0] > 0 ? Up : Default[0] < 0 ? Dn : null;
					break;
				case myTSV.hide_tsv:
					PlotBrushes[0][0] = Brushes.Transparent;
					break;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Days lookback", Order=30, GroupName="Parameters")]
		public int pDaysLookback
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Up", Order=40, GroupName="Parameters")]
		public Brush Up
		{ get; set; }

					[Browsable(false)]
					public string UpSerializable		{get { return Serialize.BrushToString(Up); }set { Up = Serialize.StringToBrush(value); }		}			

		[XmlIgnore]
		[Display(Name="Dn", Order=50, GroupName="Parameters")]
		public Brush Dn
		{ get; set; }

					[Browsable(false)]
					public string DnSerializable		{get { return Serialize.BrushToString(Dn); }set { Dn = Serialize.StringToBrush(value); }		}	
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Options", GroupName = "Parameters", Order = 39)]
		public myTSV tsv_sel { get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
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
		private TSV[] cacheTSV;
		public TSV TSV(int pDaysLookback, myTSV tsv_sel)
		{
			return TSV(Input, pDaysLookback, tsv_sel);
		}

		public TSV TSV(ISeries<double> input, int pDaysLookback, myTSV tsv_sel)
		{
			if (cacheTSV != null)
				for (int idx = 0; idx < cacheTSV.Length; idx++)
					if (cacheTSV[idx] != null && cacheTSV[idx].pDaysLookback == pDaysLookback && cacheTSV[idx].tsv_sel == tsv_sel && cacheTSV[idx].EqualsInput(input))
						return cacheTSV[idx];
			return CacheIndicator<TSV>(new TSV(){ pDaysLookback = pDaysLookback, tsv_sel = tsv_sel }, input, ref cacheTSV);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TSV TSV(int pDaysLookback, myTSV tsv_sel)
		{
			return indicator.TSV(Input, pDaysLookback, tsv_sel);
		}

		public Indicators.TSV TSV(ISeries<double> input , int pDaysLookback, myTSV tsv_sel)
		{
			return indicator.TSV(input, pDaysLookback, tsv_sel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TSV TSV(int pDaysLookback, myTSV tsv_sel)
		{
			return indicator.TSV(Input, pDaysLookback, tsv_sel);
		}

		public Indicators.TSV TSV(ISeries<double> input , int pDaysLookback, myTSV tsv_sel)
		{
			return indicator.TSV(input, pDaysLookback, tsv_sel);
		}
	}
}

#endregion
