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
	public class CDDivergenceUDT : Indicator
	{NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Size = 10, Bold = false };
	private OrderFlowCumulativeDelta OrderFlowCumulativeDelta1;
	private Brush	divergeCandle   = Brushes.Purple;  // Color body for Divergence Candle
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"A Line is drawn from the specified Open.";
				Name						= "CDDivergenceUDT";
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
	//			OrderFlowCumulativeDelta1				= OrderFlowCumulativeDelta(Close, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaType.BidAsk, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaPeriod.Session, 0);
				OrderFlowCumulativeDelta1				= OrderFlowCumulativeDelta(Close, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaType.UpDownTick, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaPeriod.Session, 0);
			}
				else if (State == State.DataLoaded)
			{				
			}	
		}
		
		protected override void OnBarUpdate()
		{	
			if (CurrentBars[0]<BarsRequiredToPlot)
			{return;}
			
		#region Transition Candles
			if(OrderFlowCumulativeDelta1.DeltaClose[0] > OrderFlowCumulativeDelta1.DeltaOpen[0] && Close[0] < Open[0])			//added Transition Yellow Outline
				{
					BarBrushes[0]=divergeCandle;
				}												
			if(OrderFlowCumulativeDelta1.DeltaClose[0] < OrderFlowCumulativeDelta1.DeltaOpen[0] && Close[0] > Open[0])			
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
		private BobC.CDDivergenceUDT[] cacheCDDivergenceUDT;
		public BobC.CDDivergenceUDT CDDivergenceUDT()
		{
			return CDDivergenceUDT(Input);
		}

		public BobC.CDDivergenceUDT CDDivergenceUDT(ISeries<double> input)
		{
			if (cacheCDDivergenceUDT != null)
				for (int idx = 0; idx < cacheCDDivergenceUDT.Length; idx++)
					if (cacheCDDivergenceUDT[idx] != null &&  cacheCDDivergenceUDT[idx].EqualsInput(input))
						return cacheCDDivergenceUDT[idx];
			return CacheIndicator<BobC.CDDivergenceUDT>(new BobC.CDDivergenceUDT(), input, ref cacheCDDivergenceUDT);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BobC.CDDivergenceUDT CDDivergenceUDT()
		{
			return indicator.CDDivergenceUDT(Input);
		}

		public Indicators.BobC.CDDivergenceUDT CDDivergenceUDT(ISeries<double> input )
		{
			return indicator.CDDivergenceUDT(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BobC.CDDivergenceUDT CDDivergenceUDT()
		{
			return indicator.CDDivergenceUDT(Input);
		}

		public Indicators.BobC.CDDivergenceUDT CDDivergenceUDT(ISeries<double> input )
		{
			return indicator.CDDivergenceUDT(input);
		}
	}
}

#endregion
