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
	public class AntoSSLQQEHybrid : Indicator
	{
		private	Series<double> tr;
//		private	Series<double> lowerband;
//		private	Series<double> upperband;
		private	Series<double> lowerk;
		private	Series<double> upperk;

		private	Series<double> emaHigh;
		private	Series<double> emaLow;

//		private	Series<double> hlv;
//		private	Series<double> hlv2;
//		private	Series<double> hlv3;

//		private	Series<double> ssldown;
//		private	Series<double> ssldown2;
//		private	Series<double> sslExit;
		private WMA wma;
		private HMA hmaHighs, hmaLows, hmaExitHigh, hmaExitLow, hmaClose;
		private JMA jmaHigh, jmaLow;
		private EMA emaTR;
		
		int p;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AntoSSLQQEHybrid";
				Calculate									= Calculate.OnBarClose;
					atr_len = 14;
//				mult = 1;
				ssl1_len = 60 ;
				
				jurik_phase = 3;
				jurik_power = 1;
				
				ssl2_len = 5 ;
				Exit_len = 5 ;
				
				usetrueRange = true;
				multy =  0.2;
				
				
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "BBMC");
				AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "upperk");
				AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "lowerk");
			}
			else if (State == State.DataLoaded)
			{
				tr = new Series<double>(this);
//				upperband = new Series<double>(this);
//				lowerband = new Series<double>(this);
				
				emaHigh = new Series<double>(this);
				emaLow = new Series<double>(this);

				upperk = new Series<double>(this);
				lowerk = new Series<double>(this);

//				hlv = new Series<double>(this);
//				hlv2 = new Series<double>(this);
//				hlv3 = new Series<double>(this);

//				ssldown = new Series<double>(this);
//				ssldown2 = new Series<double>(this);
//				sslExit = new Series<double>(this);

				wma = WMA(tr, atr_len);
				hmaHighs = HMA(High, ssl1_len);
				hmaLows = HMA(Low, ssl1_len);
				hmaExitHigh = HMA( High, Exit_len);
				hmaExitLow = HMA( Low, Exit_len);
				jmaHigh = JMA(High, jurik_phase , jurik_power, ssl1_len );
				jmaLow = JMA(Low, jurik_phase , jurik_power, ssl1_len );
				hmaClose = HMA(Close,ssl1_len);
				emaTR = EMA(tr,ssl1_len);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
//			if(CurrentBar==1){
//				hlv[0] = 0;
//				hlv2[0] = 0;
//				hlv3[0] = 0;
				
//			}
			
			if(CurrentBar < atr_len)
				return;
			
			tr[0] = Math.Max(Math.Max(High[0] -Low[0], Math.Abs(High[0]-Close[1])),Low[0]-Close[1]);
			
			
//			upperband[0] = wma[0] *mult + Close[0];
//			lowerband[0] = Close[0] - wma[0] *mult;

			emaHigh[0] = hmaHighs[0];
			emaLow[0] = hmaLows[0];

			upperk[0] =  hmaClose[0] + emaTR[0]*multy;
			lowerk[0] =  hmaClose[0] - emaTR[0]*multy;


			bool c1 = Math.Abs(Close[0]-Open[0]) > wma[0];//range of candle is larger than average range
			bool c2 = c1 || Bars.BarsType.Name.Contains("Range") || Bars.BarsType.Name.Contains("Renko");
			bool c3 = upperk[0] <= High[0] && lowerk[0] >= Low[0];
			bool c4 = c3 || upperk[0] <= High[0] && upperk[0] >= Low[0];
			bool c5 = c4 || lowerk[0] <= High[0] && lowerk[0] >= Low[0];
			if(c2 && c5 && Close[0] > upperk[0]){
				Draw.Diamond(this, "tag1"+ p.ToString(), true, 0, Low[0] - TickSize, Brushes.Lime);
				p++;
			}
			else if(c2 && c5 && Close[0] < lowerk[0] ){
				Draw.Diamond(this, "tag1"+ p.ToString(), true, 0, High[0] + TickSize, Brushes.Red);
				p++;
			}

//			hlv[0]     =  (Close[0] > emaHigh[0] ) ? 1 : (Close[0] < emaLow[0] )? -1: hlv[1];			
//			ssldown[0] =  (hlv[0] < 0 ) ?  emaHigh[0] :  emaLow[0];

//			hlv2[0]     =  (Close[0] > jmaHigh[0] ) ? 1 : (Close[0] < jmaLow[0] )? -1: hlv2[1];			
//			ssldown2[0] =  (hlv2[0] < 0 ) ?  jmaHigh[0] :  jmaLow[0];

//			hlv3[0]    =  (Close[0] > hmaExitHigh[0] ) ? 1 : (Close[0] < hmaExitLow[0] )? -1: hlv3[1];			
//			sslExit[0] =  (hlv3[0] < 0 ) ?  hmaExitHigh[0] :  hmaExitLow[0];

			Value[0] = hmaClose[0];
			upchannel[0] = upperk[0];
			lowerchannel[0] = lowerk[0];

			if(Close[0] > upperk[0]){
				PlotBrushes[0][0] = Brushes.Blue;
				PlotBrushes[1][0] = Brushes.Blue;
				PlotBrushes[2][0] = Brushes.Blue;
				BarBrush = Brushes.Blue;
			}
			else if(Close[0] < lowerk[0]){
				PlotBrushes[0][0] = Brushes.DeepPink;
				PlotBrushes[1][0] = Brushes.DeepPink;
				PlotBrushes[2][0] =  Brushes.DeepPink;
				BarBrush = Brushes.DeepPink;
			}
			else{
				PlotBrushes[0][0] = Brushes.Gray;
				PlotBrushes[1][0] = Brushes.Gray;
				PlotBrushes[2][0] = Brushes.Gray;
				BarBrush = Brushes.Gray;
				
			}
		}
		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> upchannel		{			get { return Values[1]; }		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> lowerchannel		{			get { return Values[2]; }		}

		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "atr_len", GroupName = "NinjaScriptParameters", Order = 0)]
		public int atr_len
		{ get; set; }
		
		
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "multi", GroupName = "NinjaScriptParameters", Order = 1)]
//		public int mult
//		{ get; set; }
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ssl1_len", GroupName = "NinjaScriptParameters", Order = 2)]
		public int ssl1_len
		{ get; set; }
		
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "jurik_phase", GroupName = "NinjaScriptParameters", Order = 3)]
		public int jurik_phase
		{ get; set; }
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "jurik_power", GroupName = "NinjaScriptParameters", Order = 4)]
		public int jurik_power
		{ get; set; }
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ssl2_len", GroupName = "NinjaScriptParameters", Order = 5)]
		public int ssl2_len
		{ get; set; }
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Exit_len", GroupName = "NinjaScriptParameters", Order = 6)]
		public int Exit_len
		{ get; set; }
		
		
		[NinjaScriptProperty]
        [Description("Enabled")]
        [Category("Parameters")]		
		[Display(Name="use truerange", Order=7, GroupName="Parameters")]
		public bool usetrueRange
		{ get; set; }
		
		[NinjaScriptProperty]
        [Description("TP")]
        [Category("Parameters")]		
		[Display(Name="multy", Order=8, GroupName="Parameters")]
		public double multy
		{ get; set; }
		
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AntoSSLQQEHybrid[] cacheAntoSSLQQEHybrid;
		public AntoSSLQQEHybrid AntoSSLQQEHybrid(int atr_len, int ssl1_len, int jurik_phase, int jurik_power, int ssl2_len, int exit_len, bool usetrueRange, double multy)
		{
			return AntoSSLQQEHybrid(Input, atr_len, ssl1_len, jurik_phase, jurik_power, ssl2_len, exit_len, usetrueRange, multy);
		}

		public AntoSSLQQEHybrid AntoSSLQQEHybrid(ISeries<double> input, int atr_len, int ssl1_len, int jurik_phase, int jurik_power, int ssl2_len, int exit_len, bool usetrueRange, double multy)
		{
			if (cacheAntoSSLQQEHybrid != null)
				for (int idx = 0; idx < cacheAntoSSLQQEHybrid.Length; idx++)
					if (cacheAntoSSLQQEHybrid[idx] != null && cacheAntoSSLQQEHybrid[idx].atr_len == atr_len && cacheAntoSSLQQEHybrid[idx].ssl1_len == ssl1_len && cacheAntoSSLQQEHybrid[idx].jurik_phase == jurik_phase && cacheAntoSSLQQEHybrid[idx].jurik_power == jurik_power && cacheAntoSSLQQEHybrid[idx].ssl2_len == ssl2_len && cacheAntoSSLQQEHybrid[idx].Exit_len == exit_len && cacheAntoSSLQQEHybrid[idx].usetrueRange == usetrueRange && cacheAntoSSLQQEHybrid[idx].multy == multy && cacheAntoSSLQQEHybrid[idx].EqualsInput(input))
						return cacheAntoSSLQQEHybrid[idx];
			return CacheIndicator<AntoSSLQQEHybrid>(new AntoSSLQQEHybrid(){ atr_len = atr_len, ssl1_len = ssl1_len, jurik_phase = jurik_phase, jurik_power = jurik_power, ssl2_len = ssl2_len, Exit_len = exit_len, usetrueRange = usetrueRange, multy = multy }, input, ref cacheAntoSSLQQEHybrid);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AntoSSLQQEHybrid AntoSSLQQEHybrid(int atr_len, int ssl1_len, int jurik_phase, int jurik_power, int ssl2_len, int exit_len, bool usetrueRange, double multy)
		{
			return indicator.AntoSSLQQEHybrid(Input, atr_len, ssl1_len, jurik_phase, jurik_power, ssl2_len, exit_len, usetrueRange, multy);
		}

		public Indicators.AntoSSLQQEHybrid AntoSSLQQEHybrid(ISeries<double> input , int atr_len, int ssl1_len, int jurik_phase, int jurik_power, int ssl2_len, int exit_len, bool usetrueRange, double multy)
		{
			return indicator.AntoSSLQQEHybrid(input, atr_len, ssl1_len, jurik_phase, jurik_power, ssl2_len, exit_len, usetrueRange, multy);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AntoSSLQQEHybrid AntoSSLQQEHybrid(int atr_len, int ssl1_len, int jurik_phase, int jurik_power, int ssl2_len, int exit_len, bool usetrueRange, double multy)
		{
			return indicator.AntoSSLQQEHybrid(Input, atr_len, ssl1_len, jurik_phase, jurik_power, ssl2_len, exit_len, usetrueRange, multy);
		}

		public Indicators.AntoSSLQQEHybrid AntoSSLQQEHybrid(ISeries<double> input , int atr_len, int ssl1_len, int jurik_phase, int jurik_power, int ssl2_len, int exit_len, bool usetrueRange, double multy)
		{
			return indicator.AntoSSLQQEHybrid(input, atr_len, ssl1_len, jurik_phase, jurik_power, ssl2_len, exit_len, usetrueRange, multy);
		}
	}
}

#endregion
