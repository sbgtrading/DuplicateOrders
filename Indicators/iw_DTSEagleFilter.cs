//#define MARKET_ANALYZER_VERSION

#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
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
	public class DTS_EagleFilter : Indicator
	{

#if MARKET_ANALYZER_VERSION
		private string ModuleName = "iwDTS";
#else
		private string ModuleName = "iwTrendTrader";
#endif

		private bool LicenseValid = true;
		private bool IsDebug = false;
		System.Windows.Media.Brush[] bkgBrushes = null;

		private Series<double>	avgUp;
		private Series<double>	avgDown;
		private Series<double>	down;
		private int				period	= 21;
		private Series<double>	up;
		SMA smadown, smaup;

		//		private Brush NormalBackBrush = Brushes.Empty;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "iw DTS Eagle Filter";
				IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", ModuleName, "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Green,4), PlotStyle.Square, "Direction");
				AddPlot(System.Windows.Media.Brushes.Transparent, "Signal");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Orange,1), PlotStyle.Hash, "-");
				AddPlot(new Stroke(System.Windows.Media.Brushes.Orange,1), PlotStyle.Hash, ".");
				IsOverlay=false;
				IsAutoScale=true;
				this.PaintPriceMarkers = false;
			}
			if (State == State.DataLoaded) {
				avgUp				= new Series<double>(this);//inside State.DataLoaded
				avgDown				= new Series<double>(this);//inside State.DataLoaded
				down				= new Series<double>(this);//inside State.DataLoaded
				up					= new Series<double>(this);//inside State.DataLoaded
				smaup = SMA(up,period);
				smadown = SMA(down,period);
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
				bkgBrushes = new System.Windows.Media.Brush[4]{br1.Clone(),br2.Clone(),br3.Clone(),br4.Clone()};
			}
		}
		protected override void OnBarUpdate()
		{
int line =66;
try{
			if(ChartControl!=null){
				PlotBrushes[2][0]=bkgBrushes[2];
				PlotBrushes[3][0]=bkgBrushes[2];
			}
line=71;
			if (CurrentBar < 10) {
				down[0] = (0);
				up[0] = (0);

//                if (Period < 3)
//                    Avg[0] = (50);
				return;
			}

line=82;
			down[0] = (Math.Max(Input[1] - Input[0], 0));
			up[0] = (Math.Max(Input[0] - Input[1], 0));

			if ((CurrentBar + 1) < period) {
				return;
			}

			if ((CurrentBar + 1) == period) {
				avgDown[0] = (smadown[0]);
				avgUp[0] = (smaup[0]);
			} 
			else {
				// Rest of averages are smoothed
				avgDown[0] = ((avgDown[1] * (period - 1) + down[0]) / period);
				avgUp[0] = ((avgUp[1] * (period - 1) + up[0]) / period);
			}

			double rsi = avgDown[0] == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown[0]);

line=106;
			Values[2][0] = (2);
			Values[3][0] = (-2);
			if(rsi>50) {
line=110;
				if(ChartControl!=null){
					BackBrush = bkgBrushes[0];
					PlotBrushes[0][0] = pPLUp.Clone();
					PlotBrushes[1][0] = bkgBrushes[2];
					PlotBrushes[2][0] = bkgBrushes[2];
				}
				Signal[0] = (1);
				Values[0][0] = (1.0);
			}
			else {
line=118;
				if(ChartControl!=null){
					BackBrush = bkgBrushes[1];
					PlotBrushes[0][0] = pPLDn;
					PlotBrushes[1][0] = bkgBrushes[3];
					PlotBrushes[2][0] = bkgBrushes[3];
				}
				Signal[0] = (-1);
				Values[0][0] = (-1.0);
			}
		}catch(Exception err){Print(line+":  "+err.ToString());}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Default {get { return Values[0]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Signal {get { return Values[1]; }}

		private System.Windows.Media.Brush pPLUp = System.Windows.Media.Brushes.Green;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up",  GroupName = "Visual")]
		public System.Windows.Media.Brush PLUp{	get { return pPLUp; }	set { pPLUp = value; }		}
				[Browsable(false)]
				public string PLUpClSerialize
				{	get { return Serialize.BrushToString(pPLUp); } set { pPLUp = Serialize.StringToBrush(value); }
				}
		private System.Windows.Media.Brush pPLDn = System.Windows.Media.Brushes.Red;
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down",  GroupName = "Visual")]
		public System.Windows.Media.Brush PLDn{	get { return pPLDn; }	set { pPLDn = value; }		}
				[Browsable(false)]
				public string PLDnClSerialize
				{	get { return Serialize.BrushToString(pPLDn); } set { pPLDn = Serialize.StringToBrush(value); }
				}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DTS_EagleFilter[] cacheDTS_EagleFilter;
		public DTS_EagleFilter DTS_EagleFilter()
		{
			return DTS_EagleFilter(Input);
		}

		public DTS_EagleFilter DTS_EagleFilter(ISeries<double> input)
		{
			if (cacheDTS_EagleFilter != null)
				for (int idx = 0; idx < cacheDTS_EagleFilter.Length; idx++)
					if (cacheDTS_EagleFilter[idx] != null &&  cacheDTS_EagleFilter[idx].EqualsInput(input))
						return cacheDTS_EagleFilter[idx];
			return CacheIndicator<DTS_EagleFilter>(new DTS_EagleFilter(), input, ref cacheDTS_EagleFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DTS_EagleFilter DTS_EagleFilter()
		{
			return indicator.DTS_EagleFilter(Input);
		}

		public Indicators.DTS_EagleFilter DTS_EagleFilter(ISeries<double> input )
		{
			return indicator.DTS_EagleFilter(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DTS_EagleFilter DTS_EagleFilter()
		{
			return indicator.DTS_EagleFilter(Input);
		}

		public Indicators.DTS_EagleFilter DTS_EagleFilter(ISeries<double> input )
		{
			return indicator.DTS_EagleFilter(input);
		}
	}
}

#endregion
