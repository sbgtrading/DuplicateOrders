//#define CHECKAUTHORIZATION
//#define MARKET_ANALYZER_VERSION
//#define TEMPLATE_MANAGER

#region Using declarations
using System;
using System.Windows.Media;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("")]
	public class DTS_HawkFilter : Indicator
	{

#if MARKET_ANALYZER_VERSION
		private string ModuleName = "iwDTS";
#else
		private string ModuleName = "iwMicroScalper";
#endif

		private bool LicenseValid = true;
//		private System.Windows.Media.Brush tempbrush = null;
		private bool IsDebug = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", ModuleName, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Green,4), PlotStyle.Bar, "FilterDirection");
				IsOverlay=false;
				Name = "iw DTS Hawk Filter";
			}
		}
		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;
			if(Close[0]>SMA(period)[0]) {
				Values[0][0] = (1);
				if(ChartControl!=null){
					BackBrush = pTrendUpBrush;
					PlotBrushes[0][0] = pTrendUpBrush;
				}
			} else {
				Values[0][0] = (-1);
				if(ChartControl!=null){
					BackBrush = pTrendDownBrush;
					PlotBrushes[0][0] = pTrendDownBrush;
				}
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Default {get { return Values[0]; }}

		#region Trend Colors
		private System.Windows.Media.Brush pTrendUpBrush = System.Windows.Media.Brushes.Green;
		private System.Windows.Media.Brush pTrendDownBrush = System.Windows.Media.Brushes.Red;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Rising",  GroupName = "Trend Coloring")]
		public System.Windows.Media.Brush TrendUpBrush{	get { return pTrendUpBrush; }	set { pTrendUpBrush = value; }		}
				[Browsable(false)]
				public string TLRClSerialize
				{	get { return Serialize.BrushToString(pTrendUpBrush); } set { pTrendUpBrush = Serialize.StringToBrush(value); }				}

		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Falling",  GroupName = "Trend Coloring")]
		public System.Windows.Media.Brush TrendDownBrush{	get { return pTrendDownBrush; }	set { pTrendDownBrush = value; }		}
				[Browsable(false)]
				public string TLFClSerialize
				{	get { return Serialize.BrushToString(pTrendDownBrush); } set { pTrendDownBrush = Serialize.StringToBrush(value); }				}
		#endregion

		private int	period = 55;
//		[Description("Numbers of bars used for calculations")]
//		[Category("Parameters")]
//		public int Period
//		{
//			get { return period; }
//			set { period = Math.Max(1, value); }
//		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DTS_HawkFilter[] cacheDTS_HawkFilter;
		public DTS_HawkFilter DTS_HawkFilter()
		{
			return DTS_HawkFilter(Input);
		}

		public DTS_HawkFilter DTS_HawkFilter(ISeries<double> input)
		{
			if (cacheDTS_HawkFilter != null)
				for (int idx = 0; idx < cacheDTS_HawkFilter.Length; idx++)
					if (cacheDTS_HawkFilter[idx] != null &&  cacheDTS_HawkFilter[idx].EqualsInput(input))
						return cacheDTS_HawkFilter[idx];
			return CacheIndicator<DTS_HawkFilter>(new DTS_HawkFilter(), input, ref cacheDTS_HawkFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DTS_HawkFilter DTS_HawkFilter()
		{
			return indicator.DTS_HawkFilter(Input);
		}

		public Indicators.DTS_HawkFilter DTS_HawkFilter(ISeries<double> input )
		{
			return indicator.DTS_HawkFilter(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DTS_HawkFilter DTS_HawkFilter()
		{
			return indicator.DTS_HawkFilter(Input);
		}

		public Indicators.DTS_HawkFilter DTS_HawkFilter(ISeries<double> input )
		{
			return indicator.DTS_HawkFilter(input);
		}
	}
}

#endregion
