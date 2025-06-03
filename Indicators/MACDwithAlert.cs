//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The MACD crossing alerter
	/// </summary>
	public class MACDwithAlert : Indicator
	{
		private Brush BkgZero;
		private Brush BkgUp;
		private Brush BkgDown;
		EMA emaF;
		EMA emaS;
		EMA emaSmooth;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMACD;
				Name						= "MACD with Alert";
				IsSuspendedWhileInactive	= false;
				DrawOnPricePanel = true;
				BarsRequiredToPlot			= 20;
				FastPeriod					= 12;
				SlowPeriod					= 26;
				SmoothPeriod				= 9;
				pDrawArrows = true;

				AddPlot(Brushes.DodgerBlue,		"MACD");
				AddPlot(Brushes.Goldenrod,		"Smooth");
				AddLine(Brushes.DarkCyan,	0,	"Zero");

				pCrossUpWAV = "<inst>_BuyBreakout.wav";
				pBkgStripeUpCrossOpacity = 60;
				pBkgStripeUpCross = Brushes.Lime;
				pPopupOnUp = false;
				
				pCrossZeroWAV = "none";
				pBkgStripeZeroCrossOpacity = 60;
				pBkgStripeZeroCross = Brushes.Yellow;
				pPopupOnZero = false;

				pCrossDownWAV = "<inst>_SellBreakout.wav";
				pBkgStripeDownCrossOpacity = 60;
				pBkgStripeDownCross = Brushes.Red;
				pPopupOnDown = false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				BkgZero = pBkgStripeZeroCross.Clone();
				BkgZero.Opacity = pBkgStripeZeroCrossOpacity/100.0;
				BkgZero.Freeze();
				BkgUp = pBkgStripeUpCross.Clone();
				BkgUp.Opacity = pBkgStripeUpCrossOpacity/100.0;
				BkgUp.Freeze();
				BkgDown = pBkgStripeDownCross.Clone();
				BkgDown.Opacity = pBkgStripeDownCrossOpacity/100.0;
				BkgDown.Freeze();
				emaF = EMA(FastPeriod);
				emaS = EMA(SlowPeriod);
				emaSmooth	= EMA(Default, SmoothPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			Default[0]		= emaF[0]-emaS[0];
			Avg[0]			= emaSmooth[0];
			double signal = double.MinValue;
			if(CrossAbove(Default, 0,1) || CrossBelow(Default, 0,1)){
				if(ChartControl!=null) BackBrush = BkgZero;
				signal = 0;
				if(AlertBarZero!=CurrentBar){
					AlertBarZero = CurrentBar;
					if(pPopupOnZero) Log("MACD average Crosssed 0", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "MACD average Crosssed 0", AddSoundFolder(pCrossZeroWAV), 1, Brushes.Black, BkgZero);
				}
			}
			if(CrossAbove(Default, Avg,1)){
				if(ChartControl!=null) BackBrush = BkgUp;
				signal = 1;
				if(AlertBarUp!=CurrentBar){
					AlertBarUp = CurrentBar;
					if(pPopupOnUp) Log("MACD average Crosssed UP above Avg line ", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "MACD average Crosssed up", AddSoundFolder(pCrossUpWAV), 1, Brushes.Black, BkgUp);
				}
			}
			if(CrossBelow(Default, Avg,1)){
				if(ChartControl!=null) BackBrush = BkgDown;
				signal = -1;
				if(AlertBarDown!=CurrentBar){
					AlertBarDown = CurrentBar;
					if(pPopupOnDown) Log("MACD average Crosssed Down below Avg line", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "MACD average Crosssed down", AddSoundFolder(pCrossDownWAV), 1, Brushes.Black, BkgDown);
				}
			}
			if(ChartControl!=null){
				if(signal == double.MinValue) {
					BackBrush = null;
					RemoveDrawObject($"up{CurrentBar}");
					RemoveDrawObject($"down{CurrentBar}");
				}else if(signal == 1 && pDrawArrows){
					Draw.ArrowUp(this, $"up{CurrentBar}", false, 0, Low[0]-TickSize, Brushes.Lime);
				}else if(signal == -1 && pDrawArrows){
					Draw.ArrowDown(this, $"down{CurrentBar}", false, 0, High[0]+TickSize, Brushes.Magenta);
				}
			}
		}
		private int AlertBarZero = 0;
		private int AlertBarUp = 0;
		private int AlertBarDown = 0;

		#region plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Avg
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Default
		{
			get { return Values[0]; }
		}
		#endregion
		#region Properties
//==========================================================================================================
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
				GetStandardValues(ITypeDescriptorContext context)
			{
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("<inst>_BuyBreakout.wav");
				list.Add("<inst>_SellBreakout.wav");
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
			if(Instruments.Length>1)
				wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst2>",Instruments[1].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			else if(Instruments.Length>0)
				wav = wav.Replace("<inst1>",Instruments[0].MasterInstrument.Name).Replace("<inst>",Instruments[0].MasterInstrument.Name);
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(wav,' '));
			return wav;
		}
//===============================================================================================
		private string StripOutIllegalCharacters(string filename, char ReplacementChar){
			#region strip
			var invalidChars = System.IO.Path.GetInvalidFileNameChars();
			return new string(filename.Select(c => invalidChars.Contains(c) ? ReplacementChar: c).ToArray());
			#endregion
		}
//====================================================================
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast Period", GroupName = "NinjaScriptParameters", Order = 10)]
		public int FastPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Order = 11, Name = "Slow Period", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public int SlowPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Order = 12, Name = "Smooth Period", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public int SmoothPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Order = 15, Name = "Draw arrows on crossing", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public bool pDrawArrows
		{ get; set; }

		#region -- Up line cross --
		[XmlIgnore]
		[Display(Name="Up Bkg stripe", Order=11, GroupName="Signals", Description="When the MACD line crosses over the Signal line")]
		public Brush pBkgStripeUpCross
		{ get; set; }
			[Browsable(false)]
			public string pBkgStripeUpCross_Serialize {	get { return Serialize.BrushToString(pBkgStripeUpCross); } set { pBkgStripeUpCross = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Name="Up Bkg stripe Opacity", Order=12, GroupName="Signals", Description="0 to 100")]
		public int pBkgStripeUpCrossOpacity
		{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=13, Name="Up Crossing WAV", GroupName="Signals", Description="Sound file when MACD line crosses above the Signal line")]
		public string pCrossUpWAV
		{get;set;}

		[Display(Name="Up Popup alert", Order=14, GroupName="Signals", Description="Launch a popup window when the Up cross occurs?")]
		public bool pPopupOnUp
		{get;set;}
		#endregion

		#region -- Zero cross --
		[XmlIgnore]
		[Display(Name="Zero Bkg stripe", Order=20, GroupName="Signals", Description="When the MACD line crosses the zero line")]
		public Brush pBkgStripeZeroCross
		{ get; set; }
			[Browsable(false)]
			public string pBkgStripeZeroCross_Serialize {	get { return Serialize.BrushToString(pBkgStripeZeroCross); } set { pBkgStripeZeroCross = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Name="Zero Bkg stripe Opacity", Order=21, GroupName="Signals", Description="0 to 100")]
		public int pBkgStripeZeroCrossOpacity
		{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=22, Name="Zero Crossing WAV", GroupName="Signals", Description="Sound file when MACD average line crosses Zero")]
		public string pCrossZeroWAV
		{get;set;}

		[Display(Name="Zero Popup alert", Order=23, GroupName="Signals", Description="Launch a popup window when the Zero-level cross occurs?")]
		public bool pPopupOnZero
		{get;set;}
		#endregion

		#region -- Down line cross --
		[XmlIgnore]
		[Display(Name="Down Bkg stripe", Order=31, GroupName="Signals", Description="When the MACD line crosses under the Signal line")]
		public Brush pBkgStripeDownCross
		{ get; set; }
			[Browsable(false)]
			public string pBkgStripeDownCross_Serialize {	get { return Serialize.BrushToString(pBkgStripeDownCross); } set { pBkgStripeDownCross = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Name="Down Bkg stripe Opacity", Order=32, GroupName="Signals", Description="0 to 100")]
		public int pBkgStripeDownCrossOpacity
		{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=33, Name="Down Crossing WAV", GroupName="Signals", Description="Sound file when MACD average line crosses below the Signal line")]
		public string pCrossDownWAV
		{get;set;}

		[Display(Name="Down Popup alert", Order=34, GroupName="Signals", Description="Launch a popup window when the Down cross occurs?")]
		public bool pPopupOnDown
		{get;set;}
		#endregion
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDwithAlert[] cacheMACDwithAlert;
		public MACDwithAlert MACDwithAlert(int fastPeriod, int slowPeriod, int smoothPeriod, bool pDrawArrows)
		{
			return MACDwithAlert(Input, fastPeriod, slowPeriod, smoothPeriod, pDrawArrows);
		}

		public MACDwithAlert MACDwithAlert(ISeries<double> input, int fastPeriod, int slowPeriod, int smoothPeriod, bool pDrawArrows)
		{
			if (cacheMACDwithAlert != null)
				for (int idx = 0; idx < cacheMACDwithAlert.Length; idx++)
					if (cacheMACDwithAlert[idx] != null && cacheMACDwithAlert[idx].FastPeriod == fastPeriod && cacheMACDwithAlert[idx].SlowPeriod == slowPeriod && cacheMACDwithAlert[idx].SmoothPeriod == smoothPeriod && cacheMACDwithAlert[idx].pDrawArrows == pDrawArrows && cacheMACDwithAlert[idx].EqualsInput(input))
						return cacheMACDwithAlert[idx];
			return CacheIndicator<MACDwithAlert>(new MACDwithAlert(){ FastPeriod = fastPeriod, SlowPeriod = slowPeriod, SmoothPeriod = smoothPeriod, pDrawArrows = pDrawArrows }, input, ref cacheMACDwithAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDwithAlert MACDwithAlert(int fastPeriod, int slowPeriod, int smoothPeriod, bool pDrawArrows)
		{
			return indicator.MACDwithAlert(Input, fastPeriod, slowPeriod, smoothPeriod, pDrawArrows);
		}

		public Indicators.MACDwithAlert MACDwithAlert(ISeries<double> input , int fastPeriod, int slowPeriod, int smoothPeriod, bool pDrawArrows)
		{
			return indicator.MACDwithAlert(input, fastPeriod, slowPeriod, smoothPeriod, pDrawArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDwithAlert MACDwithAlert(int fastPeriod, int slowPeriod, int smoothPeriod, bool pDrawArrows)
		{
			return indicator.MACDwithAlert(Input, fastPeriod, slowPeriod, smoothPeriod, pDrawArrows);
		}

		public Indicators.MACDwithAlert MACDwithAlert(ISeries<double> input , int fastPeriod, int slowPeriod, int smoothPeriod, bool pDrawArrows)
		{
			return indicator.MACDwithAlert(input, fastPeriod, slowPeriod, smoothPeriod, pDrawArrows);
		}
	}
}

#endregion
