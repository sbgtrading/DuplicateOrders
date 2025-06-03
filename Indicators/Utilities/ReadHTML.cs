#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization; // Built into .NET Framework

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Utilities
{
	public class ReadHTML : Indicator
	{
    class MoonPhase
    {
        public string phase { get; set; }
        public string month { get; set; }
        public string day { get; set; }
        public string year { get; set; }
        public string time { get; set; }
    }

    class ApiResponse
    {
        public List<MoonPhase> phasedata { get; set; }
    }

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ReadHTML";
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
				IsSuspendedWhileInactive = true;
				AddPlot(Brushes.Orange, "Plot1");
				AddPlot(Brushes.Orange, "Plot2");
			}
			else if (State == State.DataLoaded)
			{
        string url = @"https://aa.usno.navy.mil/api/moon/phases/year?year=2030&tz=0";
        string outputFile = @"C:\Users\sbgtr\Documents\moon_phases_2030.csv";

        try
        {
            // 1. Download JSON synchronously
            string json;
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString(url);
            }
			Print("===========================");
			Print(json);
			Print("===========================");

            // 2. Parse with JavaScriptSerializer
            var serializer = new JavaScriptSerializer();
            var response = serializer.Deserialize<ApiResponse>(json);

            // 3. Format output
            var output = new System.Text.StringBuilder();
            foreach (var phase in response.phasedata)
            {
                string phaseName = phase.phase switch
                {
                    "New Moon" => "New moon",
                    "First Quarter" => "FirstQ moon",
                    "Full Moon" => "Full moon",
                    "Last Quarter" => "LastQ moon",
                    _ => null
                };

                if (phaseName != null)
                {
                    output.AppendFormat("{0}\t{1}/{2}/{3} {4}:00\n", 
                        phaseName, 
						phase.month,
                        phase.day, 
						phase.year,
                        phase.time);
                }
            }

            // 4. Save to file
            File.WriteAllText(outputFile, output.ToString());
            Print($"Success! Saved to {outputFile}");
        }
        catch (Exception ex)
        {
            Print($"Error: {ex.Message}");
        }
	}
}
		public static long ConvertToUnixTime(DateTime dateTime)
		{
			DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
			return dateTimeOffset.ToUnixTimeSeconds();
		}
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Plot1
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Plot2
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Utilities.ReadHTML[] cacheReadHTML;
		public Utilities.ReadHTML ReadHTML()
		{
			return ReadHTML(Input);
		}

		public Utilities.ReadHTML ReadHTML(ISeries<double> input)
		{
			if (cacheReadHTML != null)
				for (int idx = 0; idx < cacheReadHTML.Length; idx++)
					if (cacheReadHTML[idx] != null &&  cacheReadHTML[idx].EqualsInput(input))
						return cacheReadHTML[idx];
			return CacheIndicator<Utilities.ReadHTML>(new Utilities.ReadHTML(), input, ref cacheReadHTML);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Utilities.ReadHTML ReadHTML()
		{
			return indicator.ReadHTML(Input);
		}

		public Indicators.Utilities.ReadHTML ReadHTML(ISeries<double> input )
		{
			return indicator.ReadHTML(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Utilities.ReadHTML ReadHTML()
		{
			return indicator.ReadHTML(Input);
		}

		public Indicators.Utilities.ReadHTML ReadHTML(ISeries<double> input )
		{
			return indicator.ReadHTML(input);
		}
	}
}

#endregion
