//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
	public class KimM4mas : Indicator
	{
		private EMA		ema9;
		private EMA		ema21;
		private EMA		ema51;
		private EMA		ema200;
		private HMA		hma;
		private SMA		sma;
		private TEMA	tema;
		private TMA		tma;
		private WMA		wma;
		private double envelopePercentage;
		private int period;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMAEnvelopes;
				Name						= "KimM4mas";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;

				AddPlot(Brushes.Transparent, "EMA1");
				AddPlot(Brushes.Transparent, "EMA2");
				AddPlot(Brushes.Transparent, "EMA3");
				AddPlot(new Stroke(Brushes.Transparent,3), PlotStyle.Dot, "EMA4");

				pUpBrush = Brushes.Green;
				pUpRegionOpacity = 80;

				pDownBrush = Brushes.Red;
				pDownRegionOpacity = 80;

				p51UpBrush = Brushes.Green;
				p51DownBrush = Brushes.Red;

				p200UpBrush = Brushes.Green;
				p200DownBrush = Brushes.Red;
			}
			else if (State == State.DataLoaded)
			{
				ema9		= EMA(Inputs[0], 9);
				ema21		= EMA(Inputs[0], 21);
				ema51		= EMA(Inputs[0], 51);
				ema200		= EMA(Inputs[0], 200);
			}
		}

		int UP = 1;
		int FLAT = 0;
		int DN = -1;
		int id = 0;
		int dir = 0;
		protected override void OnBarUpdate()
		{
			Ema1[0] = ema9[0];
			Ema2[0] = ema21[0];
			if(CurrentBar<3) return;

			if(Ema1[1] > Ema2[1]){
				if(dir<=FLAT){
					dir = UP;
					id = CurrentBar-1;
				}
				Draw.Region(this, id.ToString(), Time[0], Time.GetValueAt(id), Ema1, Ema2, Brushes.Transparent, pUpBrush, pUpRegionOpacity);
			}
			else if(Ema1[1] <= Ema2[1]){
				if(dir>=FLAT){
					dir = DN;
					id = CurrentBar-1;
				}
				Draw.Region(this, id.ToString(),Time[0], Time.GetValueAt(id), Ema1, Ema2, Brushes.Transparent, pDownBrush, pDownRegionOpacity);
			}

			Ema3[0] = ema51[0];
			if(Ema3[1] < Ema3[0]) PlotBrushes[2][0] = p51UpBrush;
			else PlotBrushes[2][0] = p51DownBrush;

			Ema4[0] = ema200[0];
			if(Ema4[1] < Ema4[0]) PlotBrushes[3][0] = p200UpBrush;
			else PlotBrushes[3][0] = p200DownBrush;
		}

		#region Properties
		[Range(0,100)]
		[Display(Order = 10, Name = "Up region opacity", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public int pUpRegionOpacity
		{get;set;}

		[XmlIgnore]
		[Display(Order = 11, Name = "Up Brush", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pUpBrush { get; set; }
			[Browsable(false)]
			public string pUpBrush_{	get { return Serialize.BrushToString(pUpBrush); } set { pUpBrush = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Order = 20, Name = "Down region opacity", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public int pDownRegionOpacity
		{get;set;}

		[XmlIgnore]
		[Display(Order = 21, Name = "Down Brush", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pDownBrush { get; set; }
			[Browsable(false)]
			public string pDownBrush_{	get { return Serialize.BrushToString(pDownBrush); } set { pDownBrush = Serialize.StringToBrush(value); }}

//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 2)]
//		public int Period
//		{get;set;}
		[XmlIgnore]
		[Display(Order = 30, Name = "51 ema Up Brush", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush p51UpBrush { get; set; }
			[Browsable(false)]
			public string p51UpBrush_{	get { return Serialize.BrushToString(p51UpBrush); } set { p51UpBrush = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 31, Name = "51 ema Down Brush", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush p51DownBrush { get; set; }
			[Browsable(false)]
			public string p51DownBrush_{	get { return Serialize.BrushToString(p51DownBrush); } set { p51DownBrush = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(Order = 40, Name = "200 ema Up Brush", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush p200UpBrush { get; set; }
			[Browsable(false)]
			public string p200UpBrush_{	get { return Serialize.BrushToString(p200UpBrush); } set { p200UpBrush = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 41, Name = "200 ema Down Brush", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush p200DownBrush { get; set; }
			[Browsable(false)]
			public string p200DownBrush_{	get { return Serialize.BrushToString(p200DownBrush); } set { p200DownBrush = Serialize.StringToBrush(value); }}

		#endregion

	#region plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Ema1 { get { return Values[0]; } }
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Ema2 { get { return Values[1]; } }
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Ema3 { get { return Values[2]; } }
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Ema4 { get { return Values[3]; } }
	#endregion
}

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private KimM4mas[] cacheKimM4mas;
		public KimM4mas KimM4mas()
		{
			return KimM4mas(Input);
		}

		public KimM4mas KimM4mas(ISeries<double> input)
		{
			if (cacheKimM4mas != null)
				for (int idx = 0; idx < cacheKimM4mas.Length; idx++)
					if (cacheKimM4mas[idx] != null &&  cacheKimM4mas[idx].EqualsInput(input))
						return cacheKimM4mas[idx];
			return CacheIndicator<KimM4mas>(new KimM4mas(), input, ref cacheKimM4mas);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.KimM4mas KimM4mas()
		{
			return indicator.KimM4mas(Input);
		}

		public Indicators.KimM4mas KimM4mas(ISeries<double> input )
		{
			return indicator.KimM4mas(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.KimM4mas KimM4mas()
		{
			return indicator.KimM4mas(Input);
		}

		public Indicators.KimM4mas KimM4mas(ISeries<double> input )
		{
			return indicator.KimM4mas(input);
		}
	}
}

#endregion
