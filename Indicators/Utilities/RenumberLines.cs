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
using System.Text.RegularExpressions;

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Utilities
{
	public class RenumberLines : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RenumberLines";
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
				Directory					= @"Indicators";
				FileName					= @"test.cs";
			}
			else if (State == State.Configure)
			{
				string[] lines=null;
//				Print("UserData: "+NinjaTrader.Core.Globals.UserDataDir);
				var outlines = new List<string>();
				string path = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"bin","custom",Directory,FileName);
				if(System.IO.File.Exists(path)){
					string newfilename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),FileName);
					if(System.IO.File.Exists(newfilename)) System.IO.File.Delete(newfilename);
					System.IO.File.Copy(path, newfilename);
					lines = System.IO.File.ReadAllLines(path);
//					Regex regex = new Regex(
//					      @"\b*[\bint]*\s+line\s*=\s*(?<num>[0-9]*)\s*;.*",
//						    RegexOptions.IgnoreCase
//						    | RegexOptions.CultureInvariant
//						    | RegexOptions.Compiled
//						    );
//					for(int i = 0; i<lines.Length; i++) {
//						if(regex.IsMatch(lines[i])){//lines[i].Contains("line=")) {
//							string[] results = regex.Split(lines[i]);
//foreach(var kk in results)Print(kk);
//							foreach(var xx in results) {
//								if(xx.Length>0)
//									lines[i] = lines[i].Replace(xx,(i+1).ToString());
//							}
//						}
//					}

					string s =string.Empty;
					for(int i = 0; i<lines.Length; i++) {
						s = lines[i];
						if(s.StartsWith("line=")){
							s = lines[i].Replace(" ",string.Empty);
							var elements = s.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
							elements[0] = string.Format("line={0}", i.ToString());
							for(int e = 0; e<elements.Length; e++) elements[e] = string.Format("{0};", elements[e]);
							s = string.Concat(elements);
							//Print(lines[i]+" becomes:  "+s);
						}
						outlines.Add(s);
					}
				}
				if(lines!=null){
					if(System.IO.File.Exists(path)) System.IO.File.Delete(path);
					foreach(var s in outlines){
						System.IO.File.AppendAllText(path, string.Concat(s,Environment.NewLine));
					}
				}
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Directory", Order=1, GroupName="Parameters")]
		public string Directory
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FileName", Order=2, GroupName="Parameters")]
		public string FileName
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Utilities.RenumberLines[] cacheRenumberLines;
		public Utilities.RenumberLines RenumberLines(string directory, string fileName)
		{
			return RenumberLines(Input, directory, fileName);
		}

		public Utilities.RenumberLines RenumberLines(ISeries<double> input, string directory, string fileName)
		{
			if (cacheRenumberLines != null)
				for (int idx = 0; idx < cacheRenumberLines.Length; idx++)
					if (cacheRenumberLines[idx] != null && cacheRenumberLines[idx].Directory == directory && cacheRenumberLines[idx].FileName == fileName && cacheRenumberLines[idx].EqualsInput(input))
						return cacheRenumberLines[idx];
			return CacheIndicator<Utilities.RenumberLines>(new Utilities.RenumberLines(){ Directory = directory, FileName = fileName }, input, ref cacheRenumberLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Utilities.RenumberLines RenumberLines(string directory, string fileName)
		{
			return indicator.RenumberLines(Input, directory, fileName);
		}

		public Indicators.Utilities.RenumberLines RenumberLines(ISeries<double> input , string directory, string fileName)
		{
			return indicator.RenumberLines(input, directory, fileName);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Utilities.RenumberLines RenumberLines(string directory, string fileName)
		{
			return indicator.RenumberLines(Input, directory, fileName);
		}

		public Indicators.Utilities.RenumberLines RenumberLines(ISeries<double> input , string directory, string fileName)
		{
			return indicator.RenumberLines(input, directory, fileName);
		}
	}
}

#endregion
