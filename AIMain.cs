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

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private aiSIGFirstTouch[] cacheaiSIGFirstTouch;
		private aiSIGIchimokuCloud[] cacheaiSIGIchimokuCloud;
		private aiSRKeyLevels[] cacheaiSRKeyLevels;
		private aiSRPriceAction[] cacheaiSRPriceAction;

		
		public aiSIGFirstTouch aiSIGFirstTouch(int numberOfBars, int numberOfBars3)
		{
			return aiSIGFirstTouch(Input, numberOfBars, numberOfBars3);
		}

		public aiSIGIchimokuCloud aiSIGIchimokuCloud(int conversion, int bBase, int spanB)
		{
			return aiSIGIchimokuCloud(Input, conversion, bBase, spanB);
		}

		public aiSRKeyLevels aiSRKeyLevels()
		{
			return aiSRKeyLevels(Input);
		}

		public aiSRPriceAction aiSRPriceAction(int minimumPivots, int zoneWidth2, int zoneSpace)
		{
			return aiSRPriceAction(Input, minimumPivots, zoneWidth2, zoneSpace);
		}


		
		public aiSIGFirstTouch aiSIGFirstTouch(ISeries<double> input, int numberOfBars, int numberOfBars3)
		{
			if (cacheaiSIGFirstTouch != null)
				for (int idx = 0; idx < cacheaiSIGFirstTouch.Length; idx++)
					if (cacheaiSIGFirstTouch[idx].NumberOfBars == numberOfBars && cacheaiSIGFirstTouch[idx].NumberOfBars3 == numberOfBars3 && cacheaiSIGFirstTouch[idx].EqualsInput(input))
						return cacheaiSIGFirstTouch[idx];
			return CacheIndicator<aiSIGFirstTouch>(new aiSIGFirstTouch(){ NumberOfBars = numberOfBars, NumberOfBars3 = numberOfBars3 }, input, ref cacheaiSIGFirstTouch);
		}

		public aiSIGIchimokuCloud aiSIGIchimokuCloud(ISeries<double> input, int conversion, int bBase, int spanB)
		{
			if (cacheaiSIGIchimokuCloud != null)
				for (int idx = 0; idx < cacheaiSIGIchimokuCloud.Length; idx++)
					if (cacheaiSIGIchimokuCloud[idx].Conversion == conversion && cacheaiSIGIchimokuCloud[idx].BBase == bBase && cacheaiSIGIchimokuCloud[idx].SpanB == spanB && cacheaiSIGIchimokuCloud[idx].EqualsInput(input))
						return cacheaiSIGIchimokuCloud[idx];
			return CacheIndicator<aiSIGIchimokuCloud>(new aiSIGIchimokuCloud(){ Conversion = conversion, BBase = bBase, SpanB = spanB }, input, ref cacheaiSIGIchimokuCloud);
		}

		public aiSRKeyLevels aiSRKeyLevels(ISeries<double> input)
		{
			if (cacheaiSRKeyLevels != null)
				for (int idx = 0; idx < cacheaiSRKeyLevels.Length; idx++)
					if ( cacheaiSRKeyLevels[idx].EqualsInput(input))
						return cacheaiSRKeyLevels[idx];
			return CacheIndicator<aiSRKeyLevels>(new aiSRKeyLevels(), input, ref cacheaiSRKeyLevels);
		}

		public aiSRPriceAction aiSRPriceAction(ISeries<double> input, int minimumPivots, int zoneWidth2, int zoneSpace)
		{
			if (cacheaiSRPriceAction != null)
				for (int idx = 0; idx < cacheaiSRPriceAction.Length; idx++)
					if (cacheaiSRPriceAction[idx].MinimumPivots == minimumPivots && cacheaiSRPriceAction[idx].ZoneWidth2 == zoneWidth2 && cacheaiSRPriceAction[idx].ZoneSpace == zoneSpace && cacheaiSRPriceAction[idx].EqualsInput(input))
						return cacheaiSRPriceAction[idx];
			return CacheIndicator<aiSRPriceAction>(new aiSRPriceAction(){ MinimumPivots = minimumPivots, ZoneWidth2 = zoneWidth2, ZoneSpace = zoneSpace }, input, ref cacheaiSRPriceAction);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.aiSIGFirstTouch aiSIGFirstTouch(int numberOfBars, int numberOfBars3)
		{
			return indicator.aiSIGFirstTouch(Input, numberOfBars, numberOfBars3);
		}

		public Indicators.aiSIGIchimokuCloud aiSIGIchimokuCloud(int conversion, int bBase, int spanB)
		{
			return indicator.aiSIGIchimokuCloud(Input, conversion, bBase, spanB);
		}

		public Indicators.aiSRKeyLevels aiSRKeyLevels()
		{
			return indicator.aiSRKeyLevels(Input);
		}

		public Indicators.aiSRPriceAction aiSRPriceAction(int minimumPivots, int zoneWidth2, int zoneSpace)
		{
			return indicator.aiSRPriceAction(Input, minimumPivots, zoneWidth2, zoneSpace);
		}


		
		public Indicators.aiSIGFirstTouch aiSIGFirstTouch(ISeries<double> input , int numberOfBars, int numberOfBars3)
		{
			return indicator.aiSIGFirstTouch(input, numberOfBars, numberOfBars3);
		}

		public Indicators.aiSIGIchimokuCloud aiSIGIchimokuCloud(ISeries<double> input , int conversion, int bBase, int spanB)
		{
			return indicator.aiSIGIchimokuCloud(input, conversion, bBase, spanB);
		}

		public Indicators.aiSRKeyLevels aiSRKeyLevels(ISeries<double> input )
		{
			return indicator.aiSRKeyLevels(input);
		}

		public Indicators.aiSRPriceAction aiSRPriceAction(ISeries<double> input , int minimumPivots, int zoneWidth2, int zoneSpace)
		{
			return indicator.aiSRPriceAction(input, minimumPivots, zoneWidth2, zoneSpace);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.aiSIGFirstTouch aiSIGFirstTouch(int numberOfBars, int numberOfBars3)
		{
			return indicator.aiSIGFirstTouch(Input, numberOfBars, numberOfBars3);
		}

		public Indicators.aiSIGIchimokuCloud aiSIGIchimokuCloud(int conversion, int bBase, int spanB)
		{
			return indicator.aiSIGIchimokuCloud(Input, conversion, bBase, spanB);
		}

		public Indicators.aiSRKeyLevels aiSRKeyLevels()
		{
			return indicator.aiSRKeyLevels(Input);
		}

		public Indicators.aiSRPriceAction aiSRPriceAction(int minimumPivots, int zoneWidth2, int zoneSpace)
		{
			return indicator.aiSRPriceAction(Input, minimumPivots, zoneWidth2, zoneSpace);
		}


		
		public Indicators.aiSIGFirstTouch aiSIGFirstTouch(ISeries<double> input , int numberOfBars, int numberOfBars3)
		{
			return indicator.aiSIGFirstTouch(input, numberOfBars, numberOfBars3);
		}

		public Indicators.aiSIGIchimokuCloud aiSIGIchimokuCloud(ISeries<double> input , int conversion, int bBase, int spanB)
		{
			return indicator.aiSIGIchimokuCloud(input, conversion, bBase, spanB);
		}

		public Indicators.aiSRKeyLevels aiSRKeyLevels(ISeries<double> input )
		{
			return indicator.aiSRKeyLevels(input);
		}

		public Indicators.aiSRPriceAction aiSRPriceAction(ISeries<double> input , int minimumPivots, int zoneWidth2, int zoneSpace)
		{
			return indicator.aiSRPriceAction(input, minimumPivots, zoneWidth2, zoneSpace);
		}

	}
}

#endregion
