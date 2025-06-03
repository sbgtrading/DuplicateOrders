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
		
		private IwJournalLync[] cacheIwJournalLync;

		
		public IwJournalLync IwJournalLync(string username, string password, string accountTag, string accountName)
		{
			return IwJournalLync(Input, username, password, accountTag, accountName);
		}


		
		public IwJournalLync IwJournalLync(ISeries<double> input, string username, string password, string accountTag, string accountName)
		{
			if (cacheIwJournalLync != null)
				for (int idx = 0; idx < cacheIwJournalLync.Length; idx++)
					if (cacheIwJournalLync[idx].Username == username && cacheIwJournalLync[idx].Password == password && cacheIwJournalLync[idx].AccountTag == accountTag && cacheIwJournalLync[idx].AccountName == accountName && cacheIwJournalLync[idx].EqualsInput(input))
						return cacheIwJournalLync[idx];
			return CacheIndicator<IwJournalLync>(new IwJournalLync(){ Username = username, Password = password, AccountTag = accountTag, AccountName = accountName }, input, ref cacheIwJournalLync);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.IwJournalLync IwJournalLync(string username, string password, string accountTag, string accountName)
		{
			return indicator.IwJournalLync(Input, username, password, accountTag, accountName);
		}


		
		public Indicators.IwJournalLync IwJournalLync(ISeries<double> input , string username, string password, string accountTag, string accountName)
		{
			return indicator.IwJournalLync(input, username, password, accountTag, accountName);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.IwJournalLync IwJournalLync(string username, string password, string accountTag, string accountName)
		{
			return indicator.IwJournalLync(Input, username, password, accountTag, accountName);
		}


		
		public Indicators.IwJournalLync IwJournalLync(ISeries<double> input , string username, string password, string accountTag, string accountName)
		{
			return indicator.IwJournalLync(input, username, password, accountTag, accountName);
		}

	}
}

#endregion
