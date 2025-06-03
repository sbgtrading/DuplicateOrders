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
using System.Threading;
using HtmlAgilityPack;

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class HTMLParser : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "HTML Parser";
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
				IsSuspendedWhileInactive					= true;
				URLstring					= string.Empty;
			}
			else if (State == State.DataLoaded)
			{
				string url = "https://us.econoday.com/byweek?day=9&month=4&year=2024&cust=us&lid=0";

				try
				{
				    ParseTextFromEconoday(url).Start();
				}
				catch (Exception ex)
				{
				    Print($"Error: {ex.Message}");
				}
			}
		}
		public static async Task<string> ParseTextFromEconoday(string url)
		{
		    using var client = new System.Net.Http.HttpClient();
		    var response = await client.GetAsync(url);
		    
		    if (!response.IsSuccessStatusCode)
		    {
		        throw new Exception($"Failed to fetch data from URL: {url} (Status code: {response.StatusCode})");
		    }
		
		    var html = await response.Content.ReadAsStringAsync();
		
		    // Use HTML Agility Pack for parsing (install via NuGet)
		    var doc = new System.Net.Http.HtmlAgilityPack.HtmlDocument();
		    doc.LoadHtml(html);
		
		    // Extract text from relevant elements (adjust selectors as needed)
		    var textNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'calendar-table')]//td");
		    var parsedText = new StringBuilder();
		    foreach (var node in textNodes)
		    {
		        parsedText.AppendLine(node.InnerText.Trim());
		    }
		
		    Print( parsedText.ToString());
			return "";
		}
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="URLstring", Order=1, GroupName="Parameters")]
		public string URLstring
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HTMLParser[] cacheHTMLParser;
		public HTMLParser HTMLParser(string uRLstring)
		{
			return HTMLParser(Input, uRLstring);
		}

		public HTMLParser HTMLParser(ISeries<double> input, string uRLstring)
		{
			if (cacheHTMLParser != null)
				for (int idx = 0; idx < cacheHTMLParser.Length; idx++)
					if (cacheHTMLParser[idx] != null && cacheHTMLParser[idx].URLstring == uRLstring && cacheHTMLParser[idx].EqualsInput(input))
						return cacheHTMLParser[idx];
			return CacheIndicator<HTMLParser>(new HTMLParser(){ URLstring = uRLstring }, input, ref cacheHTMLParser);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HTMLParser HTMLParser(string uRLstring)
		{
			return indicator.HTMLParser(Input, uRLstring);
		}

		public Indicators.HTMLParser HTMLParser(ISeries<double> input , string uRLstring)
		{
			return indicator.HTMLParser(input, uRLstring);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HTMLParser HTMLParser(string uRLstring)
		{
			return indicator.HTMLParser(Input, uRLstring);
		}

		public Indicators.HTMLParser HTMLParser(ISeries<double> input , string uRLstring)
		{
			return indicator.HTMLParser(input, uRLstring);
		}
	}
}

#endregion
