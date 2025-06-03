//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The VMA (Variable Moving Average, also known as VIDYA or Variable Index Dynamic Average)
	///  is an exponential moving average that automatically adjusts the smoothing weight based
	/// on the volatility of the data series. VMA solves a problem with most moving averages.
	/// In times of low volatility, such as when the price is trending, the moving average time
	///  period should be shorter to be sensitive to the inevitable break in the trend. Whereas,
	/// in more volatile non-trending times, the moving average time period should be longer to
	/// filter out the choppiness. VIDYA uses the CMO indicator for it's internal volatility calculations.
	/// Both the VMA and the CMO period are adjustable.
	/// </summary>
	public class VMASpread : Indicator
	{
		private CMO		cmo;
		private double	sc;	//Smoothing Constant
		private Brush bkg;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionVMA;
				Name						= "VMASpread";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 9;
				VolatilityPeriod			= 9;
				pSpreadThresholdPoints = 20;
				pBkgOpacity = 0;
				pAlwaysShowTextMsg = true;
				pFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);
				pTxtLoc = VMASpread_TxtLocation.BottomRight;

				AddPlot(Brushes.Red, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameVMA);
			}
			else if (State == State.Configure)
			{
				sc  = 2 / (double)(Period + 1);
				bkg = SignalBkg.Clone();
				bkg.Opacity = pBkgOpacity / 100.0;
				bkg.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				cmo = CMO(Inputs[0], VolatilityPeriod);
			}
		}

		private int SoundABar = 0;
		string msg = "";
		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				Value[0] = Input[0];
				return;
			}

			// Volatility Index
			double vi	= Math.Abs(cmo[0]) / 100;
			Value[0]	= sc * vi * Input[0] + (1 - sc * vi) * Value[1];
			double spread = Math.Abs(Value[0] - Closes[0][0]);
			msg = "";
			if(spread > pSpreadThresholdPoints){
				msg = string.Format("Spread of {0} is beyond threshold of {1}-pts",Instrument.MasterInstrument.FormatPrice(spread), pSpreadThresholdPoints);
				if(pBkgOpacity>0){
					BackBrushes[0] = bkg;
				}
				if(SoundABar!=CurrentBar && pWAValert != "Silent"){
					SoundABar = CurrentBar;
					Alert(SoundABar.ToString(), Priority.High, msg, AddSoundFolder(pWAValert), 1, Brushes.Black, Brushes.Green);
				}
			}else if(pAlwaysShowTextMsg)
				msg = string.Format("Current Spread is {0}-pts",Instrument.MasterInstrument.FormatPrice(spread));

			if(msg.Length>0){
				if(pTxtLoc == VMASpread_TxtLocation.TopLeft)          Draw.TextFixed(this,"info", msg, TextPosition.TopLeft,     Brushes.White, pFont, Brushes.Transparent, Brushes.Black,50);
				else if(pTxtLoc == VMASpread_TxtLocation.TopRight)    Draw.TextFixed(this,"info", msg, TextPosition.TopRight,    Brushes.White, pFont, Brushes.Transparent, Brushes.Black,50);
				else if(pTxtLoc == VMASpread_TxtLocation.Center)      Draw.TextFixed(this,"info", msg, TextPosition.Center,      Brushes.White, pFont, Brushes.Transparent, Brushes.Black, 50);
				else if(pTxtLoc == VMASpread_TxtLocation.BottomLeft)  Draw.TextFixed(this,"info", msg, TextPosition.BottomLeft,  Brushes.White, pFont, Brushes.Transparent, Brushes.Black,50);
				else if(pTxtLoc == VMASpread_TxtLocation.BottomRight) Draw.TextFixed(this,"info", msg, TextPosition.BottomRight, Brushes.White, pFont, Brushes.Transparent, Brushes.Black,50);
			}else RemoveDrawObject("info");
		}
		internal class LoadSoundFileList : StringConverter
		{
			#region LoadSoundFileList
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				//true means show a combobox
				return true;
			}
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
 			{
				//true will limit to list. false will show the list,
 				//but allow free-form entry
				return false;
			}
			public override System.ComponentModel.TypeConverter.StandardValuesCollection
					GetStandardValues(ITypeDescriptorContext context)  {
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";

				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("Silent");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						if(!list.Contains(fi.Name)){
							list.Add(fi.Name);
						}
					}
				}
				return new StandardValuesCollection(list.ToArray());
			}
			#endregion
		}
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
		}
//====================================================================
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 10)]
		public int Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "VolatilityPeriod", GroupName = "NinjaScriptParameters", Order = 20)]
		public int VolatilityPeriod
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Spread threshold (points)", GroupName = "NinjaScriptParameters", Order = 30)]
		public double pSpreadThresholdPoints
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Always show text message?", GroupName = "NinjaScriptParameters", Order = 35)]
		public bool pAlwaysShowTextMsg
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Text box location", GroupName = "NinjaScriptParameters", Order = 40)]
		public VMASpread_TxtLocation pTxtLoc
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Font", GroupName = "NinjaScriptParameters", Order = 41)]
		public NinjaTrader.Gui.Tools.SimpleFont pFont
		{ get; set; }

		private string pWAValert = "Silent";
        [Description("Sound to play when spread exceeds threshold")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "WAV alert", GroupName = "NinjaScriptParameters", Order = 50)]
        public string WAValert
        {
            get { return pWAValert; }
            set { pWAValert = value; }
        }

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal - Bkg opacity", GroupName = "NinjaScriptParameters", Order = 60)]
		public int pBkgOpacity
		{ get; set; }

		private Brush pSignalBkg = Brushes.Green;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when a spread threshold is exceeded?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal - Bkg Color",  GroupName = "NinjaScriptParameters", Order = 70)]
		public Brush SignalBkg{	get { return pSignalBkg; }	set { pSignalBkg = value; }		}
					[Browsable(false)]
					public string SignalBkgSerialize
					{	get { return Serialize.BrushToString(pSignalBkg); } set { pSignalBkg = Serialize.StringToBrush(value); }}
		#endregion
	}
}
public enum VMASpread_TxtLocation {None, TopLeft, TopRight, Center, BottomLeft, BottomRight}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VMASpread[] cacheVMASpread;
		public VMASpread VMASpread(int period, int volatilityPeriod)
		{
			return VMASpread(Input, period, volatilityPeriod);
		}

		public VMASpread VMASpread(ISeries<double> input, int period, int volatilityPeriod)
		{
			if (cacheVMASpread != null)
				for (int idx = 0; idx < cacheVMASpread.Length; idx++)
					if (cacheVMASpread[idx] != null && cacheVMASpread[idx].Period == period && cacheVMASpread[idx].VolatilityPeriod == volatilityPeriod && cacheVMASpread[idx].EqualsInput(input))
						return cacheVMASpread[idx];
			return CacheIndicator<VMASpread>(new VMASpread(){ Period = period, VolatilityPeriod = volatilityPeriod }, input, ref cacheVMASpread);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VMASpread VMASpread(int period, int volatilityPeriod)
		{
			return indicator.VMASpread(Input, period, volatilityPeriod);
		}

		public Indicators.VMASpread VMASpread(ISeries<double> input , int period, int volatilityPeriod)
		{
			return indicator.VMASpread(input, period, volatilityPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VMASpread VMASpread(int period, int volatilityPeriod)
		{
			return indicator.VMASpread(Input, period, volatilityPeriod);
		}

		public Indicators.VMASpread VMASpread(ISeries<double> input , int period, int volatilityPeriod)
		{
			return indicator.VMASpread(input, period, volatilityPeriod);
		}
	}
}

#endregion
