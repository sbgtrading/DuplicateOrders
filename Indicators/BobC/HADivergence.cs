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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.BobC
{
	public class HADivergence : Indicator
	{NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Size = 10, Bold = false };
	private NinjaTrader.NinjaScript.Indicators.BobC.HeikenAshibc2 HeikenAshibc21;
	private Brush	divergeCandle   = Brushes.Yellow;  // Color body for Divergence Candle
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"A Line is drawn from the specified Open.";
				Name						= "HADivergence";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false; 
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= false;
				IsSuspendedWhileInactive	= true;
				IsAutoScale					= false;
			}
			else if (State == State.Configure)
			{
			}
				else if (State == State.DataLoaded)
			{				
				HeikenAshibc21				= HeikenAshibc2(Close);
			}	
		}
		
		protected override void OnBarUpdate()
		{	
			if (CurrentBars[0]<BarsRequiredToPlot)
			{return;}
			
		#region Transition Candles
			if(HeikenAshibc21.HAClose[0] > HeikenAshibc21.HAOpen[0] && Close[0] <= Open[0])			//added Transition Yellow Outline
				{
					BarBrushes[0]=divergeCandle;
				}												
			if(HeikenAshibc21.HAClose[0] < HeikenAshibc21.HAOpen[0] && Close[0] >= Open[0])			
				{
					BarBrushes[0]=divergeCandle;
				}	
			#endregion
			
		}
		
		// In order to trim the indicator's label we need to override the ToString() method.
			public override string DisplayName
				{
		            get { return Name ;}
				}	

		
		#region Properties	
		[XmlIgnore]
		[Display(Name="DivergenceCandle", Description="Divergence Candle Body Color", Order=4, GroupName="Visual")]
		public Brush DivergeCandle
		{ 
			get { return divergeCandle;}
			set { divergeCandle = value;}
		}

		[Browsable(false)]
		public string DivergeCandleSerializable
		{
			get { return Serialize.BrushToString(divergeCandle); }
			set { divergeCandle = Serialize.StringToBrush(value); }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BobC.HADivergence[] cacheHADivergence;
		public BobC.HADivergence HADivergence()
		{
			return HADivergence(Input);
		}

		public BobC.HADivergence HADivergence(ISeries<double> input)
		{
			if (cacheHADivergence != null)
				for (int idx = 0; idx < cacheHADivergence.Length; idx++)
					if (cacheHADivergence[idx] != null &&  cacheHADivergence[idx].EqualsInput(input))
						return cacheHADivergence[idx];
			return CacheIndicator<BobC.HADivergence>(new BobC.HADivergence(), input, ref cacheHADivergence);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BobC.HADivergence HADivergence()
		{
			return indicator.HADivergence(Input);
		}

		public Indicators.BobC.HADivergence HADivergence(ISeries<double> input )
		{
			return indicator.HADivergence(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BobC.HADivergence HADivergence()
		{
			return indicator.HADivergence(Input);
		}

		public Indicators.BobC.HADivergence HADivergence(ISeries<double> input )
		{
			return indicator.HADivergence(input);
		}
	}
}

#endregion
