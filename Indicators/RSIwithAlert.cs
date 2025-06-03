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
	/// The RSI (Relative Strength Index) is a price-following oscillator that ranges between 0 and 100.
	/// </summary>
	public class RSIwithAlert : Indicator
	{
		private Series<double>		avgDown;
		private Series<double>		avgUp;
		private double				constant1;
		private double				constant2;
		private double				constant3;
		private Series<double>		down;
		private SMA					smaDown;
		private	SMA					smaUp;
		private Series<double>		up;
		private Brush BkgMidline;
		private Brush BkgOB;
		private Brush BkgOS;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionRSI;
				Name						= "RSI with Alert";
				IsSuspendedWhileInactive	= true;
				BarsRequiredToPlot			= 20;
				Period						= 14;
				Smooth						= 3;

				AddPlot(Brushes.DodgerBlue,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameRSI);
				AddPlot(Brushes.Goldenrod,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorAvg);

				AddLine(Brushes.DarkCyan,	30,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddLine(Brushes.DarkCyan,	70,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				
				pOBLevel = 100;
				pCrossOBWAV = "none";
				pBkgStripeOBCrossOpacity = 60;
				pBkgStripeOBCross = Brushes.Red;
				pPopupOnOB = false;
				
				pCross50WAV = "none";
				pBkgStripeMidlineCrossOpacity = 60;
				pBkgStripeMidlineCross = Brushes.Yellow;
				pPopupOn50 = false;

				pOSLevel = 0;
				pCrossOSWAV = "none";
				pBkgStripeOSCrossOpacity = 60;
				pBkgStripeOSCross = Brushes.Lime;
				pPopupOnOS = false;
			}
			else if (State == State.Configure)
			{
				constant1 = 2.0 / (1 + Smooth);
				constant2 = (1 - (2.0 / (1 + Smooth)));
				constant3 = (Period - 1);
			}
			else if (State == State.DataLoaded)
			{
				Calculate = Calculate.OnPriceChange;
				BkgMidline = pBkgStripeMidlineCross.Clone();
				BkgMidline.Opacity = pBkgStripeMidlineCrossOpacity/100.0;
				BkgMidline.Freeze();
				BkgOB = pBkgStripeOBCross.Clone();
				BkgOB.Opacity = pBkgStripeOBCrossOpacity/100.0;
				BkgOB.Freeze();
				BkgOS = pBkgStripeOSCross.Clone();
				BkgOS.Opacity = pBkgStripeOSCrossOpacity/100.0;
				BkgOS.Freeze();
				avgUp	= new Series<double>(this);
				avgDown = new Series<double>(this);
				down	= new Series<double>(this);
				up		= new Series<double>(this);
				smaDown = SMA(down, Period);
				smaUp	= SMA(up, Period);
				Lines[0].Value = pOSLevel;
				Lines[1].Value = pOBLevel;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <5)
			{
				down[0]		= 0;
				up[0]		= 0;

				if (Period < 3)
					Avg[0] = 50;

				return;
			}

			double input0	= Input[0];
			double input1	= Input[1];
			down[0]			= Math.Max(input1 - input0, 0);
			up[0]			= Math.Max(input0 - input1, 0);

			if (CurrentBar + 1 < Period)
			{
				if (CurrentBar + 1 == Period - 1)
					Avg[0] = 50;
				return;
			}

			if ((CurrentBar + 1) == Period)
			{
				// First averages
				avgDown[0]	= smaDown[0];
				avgUp[0]	= smaUp[0];
			}
			else
			{
				// Rest of averages are smoothed
				avgDown[0]	= (avgDown[1] * constant3 + down[0]) / Period;
				avgUp[0]	= (avgUp[1] * constant3 + up[0]) / Period;
			}

			double avgDown0	= avgDown[0];
			double value0	= avgDown0 == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown0);
			Default[0]		= value0;
			Avg[0]			= constant1 * value0 + constant2 * Avg[1];
			double signal = double.MinValue;
			if(CrossAbove(Avg, 50,1) || CrossBelow(Avg, 50,1)){
				BackBrush = BkgMidline;
				signal = 50;
				if(AlertBar50!=CurrentBar){
					AlertBar50 = CurrentBar;
					if(pPopupOn50) Log("RSI average Crosssed 50", LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "RSI average Crosssed 50", AddSoundFolder(pCross50WAV), 1, Brushes.Black, BkgMidline);
				}
			}
			if(pOBLevel<100 && pOBLevel>0 && (CrossAbove(Avg, pOBLevel,1) || CrossBelow(Avg, pOBLevel,1))){
				BackBrush = BkgOB;
				signal = pOBLevel;
				if(AlertBarOB!=CurrentBar){
					AlertBarOB = CurrentBar;
					if(pPopupOnOB) Log("RSI average Crosssed OB of "+pOBLevel, LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "RSI average Crosssed "+pOBLevel, AddSoundFolder(pCrossOBWAV), 1, Brushes.Black, BkgOB);
				}
			}
			if(pOSLevel<100 && pOSLevel>0 && (CrossAbove(Avg, pOSLevel,1) || CrossBelow(Avg, pOSLevel,1))){
				BackBrush = BkgOS;
				signal = pOSLevel;
				if(AlertBarOS!=CurrentBar){
					AlertBarOS = CurrentBar;
					if(pPopupOnOS) Log("RSI average Crosssed OS of "+pOSLevel, LogLevel.Alert);
					Alert(CurrentBar.ToString(),Priority.High, "RSI average Crosssed "+pOSLevel, AddSoundFolder(pCrossOSWAV), 1, Brushes.Black, BkgOS);
				}
			}
			if(signal == double.MinValue) BackBrush = null;
		}
		private int AlertBar50 = 0;
		private int AlertBarOB = 0;
		private int AlertBarOS = 0;

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
				list.Add("<inst>_Overbought.wav");
				list.Add("<inst>_Oversold.wav");
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 10)]
		public int Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 21)]
		public int Smooth
		{ get; set; }

		#region -- OB line cross --
		[NinjaScriptProperty]
		[Range(0,100)]
		[Display(Name="OB Level", Order=10, GroupName="Signals", Description="0 to 100")]
		public double pOBLevel
		{get;set;}

		[XmlIgnore]
		[Display(Name="OB Bkg stripe", Order=11, GroupName="Signals", Description="When the Avg line crosses the OB level")]
		public Brush pBkgStripeOBCross
		{ get; set; }
			[Browsable(false)]
			public string pBkgStripeOBCross_Serialize {	get { return Serialize.BrushToString(pBkgStripeOBCross); } set { pBkgStripeOBCross = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Name="OB Bkg stripe Opacity", Order=12, GroupName="Signals", Description="0 to 100")]
		public int pBkgStripeOBCrossOpacity
		{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=13, Name="OB Crossing WAV", GroupName="Signals", Description="Sound file when RSI average line crosses OB level")]
		public string pCrossOBWAV
		{get;set;}

		[Display(Name="OB Popup alert", Order=14, GroupName="Signals", Description="Launch a popup window when the OB cross occurs?")]
		public bool pPopupOnOB
		{get;set;}
		#endregion

		#region -- Midline cross --
		[XmlIgnore]
		[Display(Name="Midline Bkg stripe", Order=20, GroupName="Signals", Description="When the Avg line crosses the 50 level")]
		public Brush pBkgStripeMidlineCross
		{ get; set; }
			[Browsable(false)]
			public string pBkgStripeMidlineCross_Serialize {	get { return Serialize.BrushToString(pBkgStripeMidlineCross); } set { pBkgStripeMidlineCross = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Name="Midline Bkg stripe Opacity", Order=21, GroupName="Signals", Description="0 to 100")]
		public int pBkgStripeMidlineCrossOpacity
		{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=22, Name="Midline Crossing WAV", GroupName="Signals", Description="Sound file when RSI average line crosses 50")]
		public string pCross50WAV
		{get;set;}

		[Display(Name="Midline Popup alert", Order=23, GroupName="Signals", Description="Launch a popup window when the 50-level cross occurs?")]
		public bool pPopupOn50
		{get;set;}
		#endregion

		#region -- OS line cross --
		[NinjaScriptProperty]
		[Range(0,100)]
		[Display(Name="OS Level", Order=30, GroupName="Signals", Description="0 to 100")]
		public double pOSLevel
		{get;set;}

		[XmlIgnore]
		[Display(Name="OS Bkg stripe", Order=31, GroupName="Signals", Description="When the Avg line crosses the OS level")]
		public Brush pBkgStripeOSCross
		{ get; set; }
			[Browsable(false)]
			public string pBkgStripeOSCross_Serialize {	get { return Serialize.BrushToString(pBkgStripeOSCross); } set { pBkgStripeOSCross = Serialize.StringToBrush(value); }}

		[Range(0,100)]
		[Display(Name="OS Bkg stripe Opacity", Order=32, GroupName="Signals", Description="0 to 100")]
		public int pBkgStripeOSCrossOpacity
		{get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=33, Name="OS Crossing WAV", GroupName="Signals", Description="Sound file when RSI average line crosses OS level")]
		public string pCrossOSWAV
		{get;set;}

		[Display(Name="OS Popup alert", Order=34, GroupName="Signals", Description="Launch a popup window when the OS cross occurs?")]
		public bool pPopupOnOS
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
		private RSIwithAlert[] cacheRSIwithAlert;
		public RSIwithAlert RSIwithAlert(int period, int smooth, double pOBLevel, double pOSLevel)
		{
			return RSIwithAlert(Input, period, smooth, pOBLevel, pOSLevel);
		}

		public RSIwithAlert RSIwithAlert(ISeries<double> input, int period, int smooth, double pOBLevel, double pOSLevel)
		{
			if (cacheRSIwithAlert != null)
				for (int idx = 0; idx < cacheRSIwithAlert.Length; idx++)
					if (cacheRSIwithAlert[idx] != null && cacheRSIwithAlert[idx].Period == period && cacheRSIwithAlert[idx].Smooth == smooth && cacheRSIwithAlert[idx].pOBLevel == pOBLevel && cacheRSIwithAlert[idx].pOSLevel == pOSLevel && cacheRSIwithAlert[idx].EqualsInput(input))
						return cacheRSIwithAlert[idx];
			return CacheIndicator<RSIwithAlert>(new RSIwithAlert(){ Period = period, Smooth = smooth, pOBLevel = pOBLevel, pOSLevel = pOSLevel }, input, ref cacheRSIwithAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSIwithAlert RSIwithAlert(int period, int smooth, double pOBLevel, double pOSLevel)
		{
			return indicator.RSIwithAlert(Input, period, smooth, pOBLevel, pOSLevel);
		}

		public Indicators.RSIwithAlert RSIwithAlert(ISeries<double> input , int period, int smooth, double pOBLevel, double pOSLevel)
		{
			return indicator.RSIwithAlert(input, period, smooth, pOBLevel, pOSLevel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSIwithAlert RSIwithAlert(int period, int smooth, double pOBLevel, double pOSLevel)
		{
			return indicator.RSIwithAlert(Input, period, smooth, pOBLevel, pOSLevel);
		}

		public Indicators.RSIwithAlert RSIwithAlert(ISeries<double> input , int period, int smooth, double pOBLevel, double pOSLevel)
		{
			return indicator.RSIwithAlert(input, period, smooth, pOBLevel, pOSLevel);
		}
	}
}

#endregion
