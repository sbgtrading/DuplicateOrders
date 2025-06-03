//#define CHECKAUTHORIZATION
//#define MARKET_ANALYZER_VERSION
//#define TEMPLATE_MANAGER

#region Using declarations
using System;
//using System.Diagnostics;
using System.Drawing;
//using System.DrawinRenderTarget.Drawing2D;
using System.Windows.Media;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("")]
	public class DTS_FalconFilter : Indicator
	{

#if MARKET_ANALYZER_VERSION
		private string ModuleName = "iwDTS";
#else
		private string ModuleName = "iwSwingTrader";
#endif

		private bool LicenseValid = true;
		private bool IsDebug = false;
		System.Windows.Media.Brush[] bkgBrushes = null;
		System.Windows.Media.Brush[] plotBrushes = null;

		#region Variables
		private int				periodK	= 14;	// Kperiod
		private int				smooth	= 3;	// SlowKperiod
		private Series<double>		den;
		private Series<double>		nom;
		#endregion
		private Series<double> filterDirection;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "iw DTS Falcon Filter";
				IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", ModuleName, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Orange,4), PlotStyle.Dot, "Filter");
				AddPlot(new Stroke(System.Windows.Media.Brushes.White,1), PlotStyle.Hash, ".");
				AddPlot(new Stroke(System.Windows.Media.Brushes.White,1), PlotStyle.Hash, "-");
//			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Hash, "FDir");
//			AddLine(new Stroke(Brushes.DimGray, DashStyleHelper.Solid, 1), 0, "zero");
				IsOverlay=false;
				IsAutoScale=true;
				this.PaintPriceMarkers = false;
			}
			if (State == State.DataLoaded)
			{
				den = new Series<double>(this);//inside State.DataLoaded
				nom = new Series<double>(this);//inside State.DataLoaded
				filterDirection = new Series<double>(this);//inside State.DataLoaded
				var br1 = pPLUp.Clone();
				br1.Opacity = 0.5;
				br1.Freeze();
				var br2 = pPLDn.Clone();
				br2.Opacity = 0.5;
				br2.Freeze();
				var br3 = pPLUp.Clone();
				br3.Opacity = 0.1;
				br3.Freeze();
				var br4 = pPLDn.Clone();
				br4.Opacity = 0.1;
				br4.Freeze();
				var br5 = pPLNeutral.Clone();
				br5.Opacity = 0.5;
				br5.Freeze();
				var br6 = pPLNeutral.Clone();
				br6.Opacity = 0.1;
				br6.Freeze();
				
				plotBrushes = new System.Windows.Media.Brush[3]{br1.Clone(),br2.Clone(),br5.Clone()};
				bkgBrushes = new System.Windows.Media.Brush[3]{br3.Clone(),br4.Clone(),br6.Clone()};
			}
		}

		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;

			nom[0] = (Close[0] - MIN(Low, periodK)[0]);
            den[0] = (MAX(High, periodK)[0] - MIN(Low, periodK)[0]);

            double nomSum = SUM(nom, smooth)[0];
            double denSum = SUM(den, smooth)[0];

			double val = 0;
            if (denSum == 0) Filter[0] = (CurrentBar == 0 ? 0 : Filter[1]); 
            else {
				val = (Math.Min(100, 100 * nomSum / denSum) - 50)*2.0;
				Filter[0] = (0);
			}
			Values[1][0] = (1);
			Values[2][0] = (-1);
			if(val>0.1) {
				if(ChartControl!=null) {
					PlotBrushes[0][0] = plotBrushes[0];
					PlotBrushes[1][0] = bkgBrushes[0];
					PlotBrushes[2][0] = bkgBrushes[0];
					BackBrush = bkgBrushes[0];
				}
				filterDirection[0] = (1);
//				FDir[0] = (1);
//				Print("FD: 1");
			}
			else if(val<-0.1) {
				if(ChartControl!=null) {
					PlotBrushes[0][0] = plotBrushes[1];
					PlotBrushes[1][0] = bkgBrushes[1];
					PlotBrushes[2][0] = bkgBrushes[1];
					BackBrush = bkgBrushes[1];
				}
				filterDirection[0] = (-1);
//				FDir[0] = (-1);
//				Print("FD: -1");
			}
			else {
				if(ChartControl!=null) {
					PlotBrushes[0][0] = plotBrushes[2];
					PlotBrushes[1][0] = bkgBrushes[2];
					PlotBrushes[2][0] = bkgBrushes[2];
					BackBrush = bkgBrushes[2];
				}
				filterDirection[0] = (0);
//				FDir[0] = (0);
//				Print("FD: 0");
			}
        }
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> FilterDirection
		{
			get 
			{ 
				Update();
				return filterDirection; 
			}
		}
		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Filter
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> FDir
		{
			get { return Values[1]; }
		}
		#endregion

		#region -- Properties --
		private System.Windows.Media.Brush pPLUp = System.Windows.Media.Brushes.Green;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Up")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Up",  GroupName = "Visual")]
		public System.Windows.Media.Brush PLUp{	get { return pPLUp; }	set { pPLUp = value; }		}
				[Browsable(false)]
				public string PLUpClSerialize
				{	get { return Serialize.BrushToString(pPLUp); } set { pPLUp = Serialize.StringToBrush(value); }
				}
		private System.Windows.Media.Brush pPLDn = System.Windows.Media.Brushes.Red;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Down")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Down",  GroupName = "Visual")]
		public System.Windows.Media.Brush PLDn{	get { return pPLDn; }	set { pPLDn = value; }		}
				[Browsable(false)]
				public string PLDnClSerialize
				{	get { return Serialize.BrushToString(pPLDn); } set { pPLDn = Serialize.StringToBrush(value); }
				}
		private System.Windows.Media.Brush pPLNeutral = System.Windows.Media.Brushes.Gold;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Visual")]
// [Gui.Design.DisplayNameAttribute("Neutral")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Neutral",  GroupName = "Visual")]
		public System.Windows.Media.Brush PLNeutral{	get { return pPLNeutral; }	set { pPLNeutral = value; }		}
				[Browsable(false)]
				public string PLNeutralClSerialize
				{	get { return Serialize.BrushToString(pPLNeutral); } set { pPLNeutral = Serialize.StringToBrush(value); }
				}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DTS_FalconFilter[] cacheDTS_FalconFilter;
		public DTS_FalconFilter DTS_FalconFilter()
		{
			return DTS_FalconFilter(Input);
		}

		public DTS_FalconFilter DTS_FalconFilter(ISeries<double> input)
		{
			if (cacheDTS_FalconFilter != null)
				for (int idx = 0; idx < cacheDTS_FalconFilter.Length; idx++)
					if (cacheDTS_FalconFilter[idx] != null &&  cacheDTS_FalconFilter[idx].EqualsInput(input))
						return cacheDTS_FalconFilter[idx];
			return CacheIndicator<DTS_FalconFilter>(new DTS_FalconFilter(), input, ref cacheDTS_FalconFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DTS_FalconFilter DTS_FalconFilter()
		{
			return indicator.DTS_FalconFilter(Input);
		}

		public Indicators.DTS_FalconFilter DTS_FalconFilter(ISeries<double> input )
		{
			return indicator.DTS_FalconFilter(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DTS_FalconFilter DTS_FalconFilter()
		{
			return indicator.DTS_FalconFilter(Input);
		}

		public Indicators.DTS_FalconFilter DTS_FalconFilter(ISeries<double> input )
		{
			return indicator.DTS_FalconFilter(input);
		}
	}
}

#endregion
