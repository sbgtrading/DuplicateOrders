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
namespace NinjaTrader.NinjaScript.Indicators.Utilities
{
	public class TestLicenseSystem : Indicator
	{
		string ModuleName          = "TestLicenseSystem";
		string indicatorVersion    = "v1.0";
		string SupportEmailAddress = "support@company.com";
		bool IsDebug      = true;
		bool ValidLicense = false;
		string MachineId  = string.Empty;
		string msg        = string.Empty;
		List<string> Expected_ISTagSet = new List<string>(){"7035","6783","4970",  "12775"};//these are the Infusionsoft tag numbers that permit execution of this indicator

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description	= @"";
				Name		= "TestLicenseSystem";
				IsOverlay	= false;
				Contact_ID	= 1;
			}
			else if (State == State.DataLoaded)
			{
				MachineId = NinjaTrader.Cbi.License.MachineId;
				string plainText = string.Empty;
if(IsDebug) Print("Sending webclient request");
				var URL = string.Format("https://nstradingacademy.com/ns_scripts/IS/NS_LicenseCheck.php");
				try{
					#region -- Do the license check --
					var parameters = new System.Collections.Specialized.NameValueCollection();
					var values = new System.Collections.Generic.Dictionary<string, string>
                        {
							{ "nscustid", Contact_ID.ToString() },
							{ "custnum",  MachineId },
							{ "platform", "ninjatrader"},
							{ "version",  ModuleName+" "+indicatorVersion},
							{ "datetime", DateTime.Now.ToString()},
							{ "random",   this.Name}  
                        };
					var paramstring = string.Empty;
					foreach (var kvp in values)
					{
		                parameters.Add(kvp.Key, kvp.Value);
		                if(paramstring.Length==0)
		                    paramstring = string.Format("{0}={1}", kvp.Key, kvp.Value);
		                else
		                    paramstring = string.Format("{0}&{1}={2}", paramstring, kvp.Key, kvp.Value);
					}
					#endregion

					#region -- Create WebClient and post request --
					var ntWebClient = new System.Net.WebClient();
					ntWebClient.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
					ntWebClient.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "NeuroStreet NinjaTrader DLL");
					System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

if(IsDebug) Print(string.Concat(Environment.NewLine, "====== response below =======", Environment.NewLine));
					try
					{
						byte[] responseArray = ntWebClient.UploadValues(URL, "POST", parameters);
						plainText = System.Text.Encoding.ASCII.GetString(responseArray);
if(IsDebug) Print(string.Concat(responseArray.Length,"-char response was: ", Environment.NewLine, plainText, Environment.NewLine));
						msg = string.Empty;
		            }
		            catch (Exception er) {
if(IsDebug) Print(string.Concat(Environment.NewLine, "====== error =======", Environment.NewLine, er.ToString(), Environment.NewLine));
						msg = er.ToString();
		            }
					#endregion
					//==========================================
				}catch(Exception err){
					msg = string.Concat(msg,"===================",Environment.NewLine,err.Message);
if(IsDebug) Print(msg);
				}
if(IsDebug) Print("PlainText response from api: "+plainText);
				var TagsFromServer = plainText.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries).ToList();
				foreach(var num in TagsFromServer){
					if(Expected_ISTagSet.Contains(num)) {ValidLicense = true; break;}
				}
				if(!ValidLicense){
					Print(string.Format("Send this message to: {1}{0}This license is expired{0}{2}  {3}{0}{4}", Environment.NewLine, SupportEmailAddress, Contact_ID, ModuleName, MachineId));
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if(!ValidLicense) {
				Draw.TextFixed(this,"lictext",ModuleName+" license not valid\n"+MachineId+"  "+Contact_ID+"\nHas your NT machine id changed?  Log into your Member Site to update your NT machine id\nContact "+SupportEmailAddress+" for assistance",TextPosition.Center,Brushes.White,new SimpleFont("Arial",12),Brushes.Black,Brushes.Black,90);
				IsVisible = false;
				return;
			}else
				Draw.TextFixed(this,"lictext",ModuleName+" license is valid",TextPosition.Center,Brushes.White,new SimpleFont("Arial",12),Brushes.Black,Brushes.Black,90);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contact_ID", Description="Your customer id number", Order=1, GroupName="Parameters")]
		public int Contact_ID
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Utilities.TestLicenseSystem[] cacheTestLicenseSystem;
		public Utilities.TestLicenseSystem TestLicenseSystem(int contact_ID)
		{
			return TestLicenseSystem(Input, contact_ID);
		}

		public Utilities.TestLicenseSystem TestLicenseSystem(ISeries<double> input, int contact_ID)
		{
			if (cacheTestLicenseSystem != null)
				for (int idx = 0; idx < cacheTestLicenseSystem.Length; idx++)
					if (cacheTestLicenseSystem[idx] != null && cacheTestLicenseSystem[idx].Contact_ID == contact_ID && cacheTestLicenseSystem[idx].EqualsInput(input))
						return cacheTestLicenseSystem[idx];
			return CacheIndicator<Utilities.TestLicenseSystem>(new Utilities.TestLicenseSystem(){ Contact_ID = contact_ID }, input, ref cacheTestLicenseSystem);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Utilities.TestLicenseSystem TestLicenseSystem(int contact_ID)
		{
			return indicator.TestLicenseSystem(Input, contact_ID);
		}

		public Indicators.Utilities.TestLicenseSystem TestLicenseSystem(ISeries<double> input , int contact_ID)
		{
			return indicator.TestLicenseSystem(input, contact_ID);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Utilities.TestLicenseSystem TestLicenseSystem(int contact_ID)
		{
			return indicator.TestLicenseSystem(Input, contact_ID);
		}

		public Indicators.Utilities.TestLicenseSystem TestLicenseSystem(ISeries<double> input , int contact_ID)
		{
			return indicator.TestLicenseSystem(input, contact_ID);
		}
	}
}

#endregion
