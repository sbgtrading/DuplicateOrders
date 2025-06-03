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
	/// The Commodity Channel Index (CCI) measures the variation of a security's price
	/// from its statistical mean. High values show that prices are unusually high
	/// compared to average prices whereas low values indicate that prices are unusually low.
	/// </summary>
	public class CCIwithAlert : Indicator
	{
		private SMA sma;
		private Brush BkgZero;
		private Brush BkgUp;
		private Brush BkgDown;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionCCI;
				Name						= "CCI with Alert";
				IsSuspendedWhileInactive	= true;
				Period						= 14;
				pDrawArrows = true;

				AddPlot(Brushes.Goldenrod,			NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameCCI);
				AddLine(Brushes.DarkGray,	140,	NinjaTrader.Custom.Resource.CCILevel2);
				AddLine(Brushes.DarkGray,	80,	NinjaTrader.Custom.Resource.CCILevel1);
				AddLine(Brushes.DarkGray,	0,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddLine(Brushes.DarkGray,	-80,	NinjaTrader.Custom.Resource.CCILevelMinus1);
				AddLine(Brushes.DarkGray,	-140,	NinjaTrader.Custom.Resource.CCILevelMinus2);
				pBkgStripeZeroCross = Brushes.White;
				pBkgStripeUpCross = Brushes.Lime;
				pBkgStripeDownCross = Brushes.Red;
				pCrossUpWAV = "none";
				pCrossDownWAV = "none";
				pCrossZeroWAV = "none";
			}
			else if (State == State.DataLoaded){
				sma  = SMA(Typical, Period);
				BkgZero = pBkgStripeZeroCross.Clone();
				BkgZero.Opacity = pBkgStripeZeroCrossOpacity/100.0;
				BkgZero.Freeze();
				BkgUp = pBkgStripeUpCross.Clone();
				BkgUp.Opacity = pBkgStripeUpCrossOpacity/100.0;
				BkgUp.Freeze();
				BkgDown = pBkgStripeDownCross.Clone();
				BkgDown.Opacity = pBkgStripeDownCrossOpacity/100.0;
				BkgDown.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
				Value[0] = 0;
			else
			{
				double mean = 0;
				double sma0 = sma[0];

				for (int idx = Math.Min(CurrentBar, Period - 1); idx >= 0; idx--)
					mean += Math.Abs(Typical[idx] - sma0);

				Value[0] = (Typical[0] - sma0) / (mean.ApproxCompare(0) == 0 ? 1 : (0.015 * (mean / Math.Min(Period, CurrentBar + 1))));
			}
			double signal = double.MinValue;
			if(CrossAbove(Values[0], 0,1) || CrossBelow(Values[0], 0,1)){
				if(ChartControl!=null) BackBrush = BkgZero;
				signal = 0;
				if(AlertBarZero!=CurrentBar){
					AlertBarZero = CurrentBar;
					if(pPopupOnZero) Log("CCI average Crosssed 0", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "CCI average Crosssed 0", AddSoundFolder(pCrossZeroWAV), 1, Brushes.Black, BkgZero);
				}
			}
			if(CrossAbove(Values[0], Lines[4].Value, 1)){
				if(ChartControl!=null) BackBrush = BkgUp;
				signal = 1;
				if(AlertBarUp!=CurrentBar){
					AlertBarUp = CurrentBar;
					if(pPopupOnUp) Log("CCI average Crosssed UP above "+Lines[4].Value+"-line ", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "CCI average Crosssed up", AddSoundFolder(pCrossUpWAV), 1, Brushes.Black, BkgUp);
				}
			}
			if(CrossBelow(Values[0], Lines[0].Value, 1)){
				if(ChartControl!=null) BackBrush = BkgDown;
				signal = -1;
				if(AlertBarDown!=CurrentBar){
					AlertBarDown = CurrentBar;
					if(pPopupOnDown) Log("CCI average Crosssed Down below "+Lines[0].Value+"-line", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "CCI average Crosssed down", AddSoundFolder(pCrossDownWAV), 1, Brushes.Black, BkgDown);
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Order = 15, Name = "Draw arrows on crossing", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public bool pDrawArrows
		{ get; set; }

		#region -- Up line cross --
		[XmlIgnore]
		[Display(Name="Up Bkg stripe", Order=11, GroupName="Signals", Description="When the CCI line crosses the Up level")]
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
		[Display(Order=13, Name="Up Crossing WAV", GroupName="Signals", Description="Sound file when CCI average line crosses Up level")]
		public string pCrossUpWAV
		{get;set;}

		[Display(Name="Up Popup alert", Order=14, GroupName="Signals", Description="Launch a popup window when the Up cross occurs?")]
		public bool pPopupOnUp
		{get;set;}
		#endregion

		#region -- Zero cross --
		[XmlIgnore]
		[Display(Name="Zero Bkg stripe", Order=20, GroupName="Signals", Description="When the CCI line crosses the Zero level")]
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
		[Display(Order=22, Name="Zero Crossing WAV", GroupName="Signals", Description="Sound file when CCI average line crosses Zero")]
		public string pCrossZeroWAV
		{get;set;}

		[Display(Name="Zero Popup alert", Order=23, GroupName="Signals", Description="Launch a popup window when the Zero-level cross occurs?")]
		public bool pPopupOnZero
		{get;set;}
		#endregion

		#region -- Down line cross --
		[XmlIgnore]
		[Display(Name="Down Bkg stripe", Order=31, GroupName="Signals", Description="When the CCI line crosses the Down level")]
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
		[Display(Order=33, Name="Down Crossing WAV", GroupName="Signals", Description="Sound file when CCI average line crosses Down level")]
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
		private CCIwithAlert[] cacheCCIwithAlert;
		public CCIwithAlert CCIwithAlert(int period, bool pDrawArrows)
		{
			return CCIwithAlert(Input, period, pDrawArrows);
		}

		public CCIwithAlert CCIwithAlert(ISeries<double> input, int period, bool pDrawArrows)
		{
			if (cacheCCIwithAlert != null)
				for (int idx = 0; idx < cacheCCIwithAlert.Length; idx++)
					if (cacheCCIwithAlert[idx] != null && cacheCCIwithAlert[idx].Period == period && cacheCCIwithAlert[idx].pDrawArrows == pDrawArrows && cacheCCIwithAlert[idx].EqualsInput(input))
						return cacheCCIwithAlert[idx];
			return CacheIndicator<CCIwithAlert>(new CCIwithAlert(){ Period = period, pDrawArrows = pDrawArrows }, input, ref cacheCCIwithAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCIwithAlert CCIwithAlert(int period, bool pDrawArrows)
		{
			return indicator.CCIwithAlert(Input, period, pDrawArrows);
		}

		public Indicators.CCIwithAlert CCIwithAlert(ISeries<double> input , int period, bool pDrawArrows)
		{
			return indicator.CCIwithAlert(input, period, pDrawArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCIwithAlert CCIwithAlert(int period, bool pDrawArrows)
		{
			return indicator.CCIwithAlert(Input, period, pDrawArrows);
		}

		public Indicators.CCIwithAlert CCIwithAlert(ISeries<double> input , int period, bool pDrawArrows)
		{
			return indicator.CCIwithAlert(input, period, pDrawArrows);
		}
	}
}

#endregion
