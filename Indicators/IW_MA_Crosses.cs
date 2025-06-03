//#define SPEECH_ENABLED
// 
// Copyright (C) 2011, SBG Trading Corp.   www.affordableindicators.com
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//


#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion
#if SPEECH_ENABLED
using SpeechLib;
//using System.Reflection;
using System.Threading;
#endif
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;

public enum MA_Crosses_MsgLengthType {Verbose, Terse}

namespace NinjaTrader.NinjaScript.Indicators
{

#if SPEECH_ENABLED
	public delegate void SpeechDelegate_MA_Crosses(string str);
#endif
	/// <summary>
	/// Give alert when two MA's cross or come within X-ticks
	/// </summary>
	[Description("Give alert when two MA's cross or come within X-ticks")]
	public class MA_Crosses : Indicator
	{
//====================================================================================================
//====================================================================================================
		private string AddSoundFolder(string wav){
			if(wav=="NO SOUND") return "";
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds",wav);
		}
//====================================================================================================
		#region SayItPlayIt methods
		private string RemoveNumbersFromString(string instr, int CharsToSkip) {
			if(instr.Length <= CharsToSkip) return instr;
			string outstr = string.Empty;
			if(CharsToSkip>0) outstr = instr.Substring(0,CharsToSkip);

			for(int i = 0+CharsToSkip; i<instr.Length; i++) {
				if((int)instr[i] >= (int)'0' && (int)instr[i] <= (int)'9') continue; else outstr = string.Concat(outstr,instr[i]);
			}
			return outstr;
		}
		private void PlayIt(string EncodedTag) {
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
//				Alert(CurrentBar.ToString(),Priority.High,"",AddSoundFolder(elements[elements.Length-1])
				PlaySound(AddSoundFolder(elements[elements.Length-1]));
				TimeOfText = NinjaTrader.Core.Globals.Now;
				AlertMsg = MakeString(new Object[]{"WAV file ",elements[elements.Length-1]," played"});
			}
		}
#if SPEECH_ENABLED
		private static void SayItThread (string WhatToSay) {
			if(WhatToSay.Length>0) {
				SpVoice voice = new SpVoice();
				// Tells the program to speak everything in the textbox using Default settings
				voice.Speak(WhatToSay, SpeechVoiceSpeakFlags.SVSFDefault);
			}
		}
		private void SayIt (string EncodedTag, double Price) {
			if(!SpeechEnabled) {
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = "Speech not available\nPut Interop.SpeechLib.DLL into 'bin\\Custom' folder and restart the platform";
				Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center,ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.LabelFont, Color.Black, ChartControl.BackBrush, 10);
				return;
			}
			if((State == State.Historical)) return;
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
				if(elements[0][0]=='/') return;
				SpeechDelegate_MA_Crosses speechThread = new SpeechDelegate_MA_Crosses(SayItThread);
				string SayThis = elements[elements.Length-1].ToUpper();
				if(SayThis.Contains("[SAYPRICE]")) {
					string pricestr1 = Price.ToString(FS);
					string spokenprice = string.Empty + pricestr1[0];
					int i = 0;
					while(i<pricestr1.Length) {
						spokenprice = MakeString(new Object[]{spokenprice," ",pricestr1[i++]});
					}
					SayThis = SayThis.Replace("[SAYPRICE]", spokenprice);
				}
				if(SayThis.Contains("[TIMEFRAME]")) {
					SayThis = SayThis.Replace("[TIMEFRAME]", TimeFrameText);
				}
				if(SayThis.Contains("[INSTRUMENTNAME]")) {
					string name = Instrument.FullName.Replace('-',' ');
					name = RemoveNumbersFromString(name,1);
					string spokenphrase = string.Empty;// + name[0];
					int i = 0;
					while(i<name.Length) {
						spokenphrase = MakeString(new Object[]{spokenphrase," ",name[i++]});
					}
					SayThis = SayThis.Replace("[INSTRUMENTNAME]", spokenphrase);
				}
				speechThread.BeginInvoke(SayThis, null, null);
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = MakeString(new Object[]{"'",SayThis,"' was spoken"});
				Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center,ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.LabelFont, Color.Black, ChartControl.BackBrush, 10);
			}
		}
#else
		private void SayIt (string EncodedTag) {}
		private void SayIt (string EncodedTag,double p) {}
#endif
		#endregion
//====================================================================================================
	#region MakeString
	private static string MakeString(object[] s, string Separator){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		return stb.ToString();
	}
	private void PrintMakeString(object[] s, string Separator){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		Print(stb.ToString());
	}
	private void PrintMakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		Print(stb.ToString());
	}
	private void PrintMakeString(string filepath, object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		System.IO.File.AppendAllText(filepath,stb.ToString());
	}
	private void PrintMakeString(string filepath, object[] s, string Separator) {
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
			if(i<s.Length-1 && Separator.Length>0) stb = stb.Append(Separator);
		}
		System.IO.File.AppendAllText(filepath,stb.ToString());
	}
	private static string MakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		return stb.ToString();
	}
		#endregion
//====================================================================================================
#if SPEECH_ENABLED
		static Assembly  SpeechLib = null;
#endif
		#region variables
		private const int LONG = 1;
		private const int FLAT = 0;
		private const int SHORT = -1;
		private const int SIGNAL_MA1_OVER_MA2_CrossInward  = 1;
		private const int SIGNAL_MA1_OVER_MA2_CrossOutward  = 2;
		private const int SIGNAL_MA1_UNDER_MA2_CrossInward = -1;
		private const int SIGNAL_MA1_UNDER_MA2_CrossOutward = -2;
		private const int BarToMA1_CROSSES_MA1_UPWARD   = 1;
		private const int BarToMA1_CROSSES_MA1_DOWNWARD = -1;
		private const int BarToMA1_BAR_ABOVE_MA1 = 2;
		private const int BarToMA1_BAR_BELOW_MA1 = -2;
		private const int BarToMA2_CROSSES_MA2_UPWARD   = 1;
		private const int BarToMA2_CROSSES_MA2_DOWNWARD = -1;
		private const int BarToMA2_BAR_ABOVE_MA2 = 2;
		private const int BarToMA2_BAR_BELOW_MA2 = -2;
		private string    NL = Environment.NewLine;
		private string    Subj,Body;
		private bool      LicenseValid = true;

		private string   AlertMsg = "";
		private DateTime TimeOfText = DateTime.MinValue;
		private bool     SpeechEnabled = false;
		private string   FS = string.Empty;
		private string   TimeFrameText = string.Empty;
		private int      Direction = FLAT;
		private int      EmailsThisBar = 0;
		private double   zone=0;
		private Brush    Normal_bkgBrush;
		private DateTime DirectionFillRegionName=DateTime.MinValue;
		private bool     StartFillRegion = false;
		private System.Collections.Generic.SortedDictionary<string,Brush> MyBrushes = new System.Collections.Generic.SortedDictionary<string,Brush>();

		private int pPeriod1 = 10; // Default setting for Period1
		private int pPeriod2 = 20; // Default setting for Period2
		private MA_Crosses_type pMA1Type = MA_Crosses_type.EMA;
		private MA_Crosses_type pMA2Type = MA_Crosses_type.EMA;
		private MA_Crosses_DataType pMA1DataType = MA_Crosses_DataType.Close;
		private MA_Crosses_DataType pMA2DataType = MA_Crosses_DataType.Close;
		private Indicator TheMA1, TheMA2;
		private int PopupBar=0, AlertsThisBar=0;
		private bool RunInit=true;
		#endregion

		protected override void OnStateChange(){
			#region OnStateChange
			if (State == State.SetDefaults)
			{
				bool IsBen = NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 && System.IO.File.Exists("c:\\222222222222.txt");
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "AIMACrosses", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
	//				VendorLicense("IndicatorWarehouse", "AIRoadrunner", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(Brushes.Blue, "MA1");
				AddPlot(Brushes.Blue, "MA2");
				AddPlot(Brushes.Transparent, "Signal");
				AddPlot(Brushes.Transparent, "SignalTrend");
				AddPlot(Brushes.Transparent, "SignalAge");
				AddPlot(Brushes.Transparent, "BarToMA1");
				AddPlot(Brushes.Transparent, "BarToMA2");
				Calculate=Calculate.OnPriceChange;
				IsOverlay=true;
				//PriceTypeSupported	= false;
				//ArePlotsConfigurable=false;
				Name= "iw MA Cross Alerts";
			}else if(State==State.DataLoaded){
//				if(ChartControl!=null) Normal_bkgColor = ChartControl.BackBrush;
				MyBrushes["pSignal_BackgroundColor"] = pSignal_BackgroundColor.Clone();
				MyBrushes["pSignal_BackgroundColor"].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes["pSignal_BackgroundColor"].Freeze();
				MyBrushes["pBarToMA1_BackgroundColor"] = pBarToMA1_BackgroundColor.Clone();
				MyBrushes["pBarToMA1_BackgroundColor"].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes["pBarToMA1_BackgroundColor"].Freeze();
				MyBrushes["pBarToMA2_BackgroundColor"] = pBarToMA2_BackgroundColor.Clone();
				MyBrushes["pBarToMA2_BackgroundColor"].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes["pBarToMA2_BackgroundColor"].Freeze();
				
				MyBrushes["pMA1underMA2FillColor"] = pMA1underMA2FillColor.Clone();
				MyBrushes["pMA1underMA2FillColor"].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes["pMA1underMA2FillColor"].Freeze();
				
				MyBrushes["pMA1overMA2FillColor"] = pMA1overMA2FillColor.Clone();
				MyBrushes["pMA1overMA2FillColor"].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes["pMA1overMA2FillColor"].Freeze();
				
				string n = "pInwardDownColor";
				MyBrushes[n] = pInwardDownColor.Clone();
				MyBrushes[n].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes[n].Freeze();
				n = "pInwardUpColor";
				MyBrushes[n] = pInwardUpColor.Clone();
				MyBrushes[n].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes[n].Freeze();
				n = "pOutwardDownColor";
				MyBrushes[n] = pOutwardDownColor.Clone();
				MyBrushes[n].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes[n].Freeze();
				n = "pOutwardUpColor";
				MyBrushes[n] = pOutwardUpColor.Clone();
				MyBrushes[n].Opacity = this.pBackgroundOpacity/10.0;
				MyBrushes[n].Freeze();
			}
			#endregion
		}

//==================================================================
		private double zltema(ISeries<double> input, int period){
			double TEMA1 = TEMA(input, period)[0];
			double TEMA2 = TEMA(TEMA(input, period), period)[0];
            return(TEMA1 + (TEMA1 - TEMA2));
		}
//==================================================================
		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;
			if(RunInit)
			{
				RunInit=false;
				#region RunInit
#if SPEECH_ENABLED
				string dll = System.IO.Path.Combine(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"bin\custom"),"interop.speechlib.dll");   //"c:\users\ben\Documents\NinjaTrader 7\"
				if(System.IO.File.Exists(dll)) {
					SpeechEnabled = true;
					SpeechLib = Assembly.LoadFile(dll);
				}
#endif
				TimeFrameText = Bars.BarsPeriod.ToString().Replace("min","minutes");
				if(Bars.BarsPeriod.Value==1) TimeFrameText = TimeFrameText.Replace("minutes","minute");

				Plots[0].DashStyleHelper = pLineDashStyleFast;
				Plots[1].DashStyleHelper = pLineDashStyleSlow;

				Plots[0].Width = pLineWidthFast;
				Plots[1].Width = pLineWidthSlow;

				Plots[0].PlotStyle = pLineStyleFast;
				Plots[1].PlotStyle = pLineStyleSlow;

				ISeries<double> x = Close;
				if(pMA1DataType == MA_Crosses_DataType.Open)    x = Open;
				if(pMA1DataType == MA_Crosses_DataType.High)    x = High;
				if(pMA1DataType == MA_Crosses_DataType.Low)     x = Low;
				if(pMA1DataType == MA_Crosses_DataType.Median)  x = Median;
				if(pMA1DataType == MA_Crosses_DataType.Typical) x = Typical;
//				switch (pMA1Type)
//				{	case MA_Crosses_type.SMA  : TheMA1 = SMA(x,pPeriod1); break;
//					case MA_Crosses_type.EMA  : TheMA1 = EMA(x,pPeriod1); break;
//					case MA_Crosses_type.WMA  : TheMA1 = WMA(x,pPeriod1); break;
//					case MA_Crosses_type.HMA  : TheMA1 = HMA(x,pPeriod1); break;
//					case MA_Crosses_type.TEMA : TheMA1 = TEMA(x,pPeriod1); break;
//					case MA_Crosses_type.VWMA : TheMA1 = VWMA(x,pPeriod1); break;
//					case MA_Crosses_type.TSF  : TheMA1 = TSF(x,1,pPeriod1); break;
//					case MA_Crosses_type.LinReg  : TheMA1 = LinReg(pPeriod1); break;
//				}
				if(pMA1Type == MA_Crosses_type.EMA)       TheMA1 = EMA(x,pPeriod1);
				else if(pMA1Type == MA_Crosses_type.SMA)  TheMA1 = SMA(x,pPeriod1);
				else if(pMA1Type == MA_Crosses_type.HMA)  TheMA1 = HMA(x,pPeriod1);
				else if(pMA1Type == MA_Crosses_type.SMMA) TheMA1 = IW_SMMA(x, pPeriod1);
				else if(pMA1Type == MA_Crosses_type.TMA)  TheMA1 = TMA(x,pPeriod1);
				else if(pMA1Type == MA_Crosses_type.TEMA) TheMA1 = TEMA(x,pPeriod1);
				else if(pMA1Type == MA_Crosses_type.WMA)  TheMA1 = WMA(x,pPeriod1);
				else if(pMA1Type == MA_Crosses_type.LinReg)        TheMA1 = LinReg(x, pPeriod1);
				else if(pMA1Type == MA_Crosses_type.ZeroLagEMA)    TheMA1 = ZLEMA(x, pPeriod1);
				else if(pMA1Type == MA_Crosses_type.ZeroLagHATEMA) TheMA1 = IW_ZeroLagHATEMA(x, pPeriod1);
				else if(pMA1Type == MA_Crosses_type.ZeroLagTEMA)   TheMA2 = IW_ZeroLagTEMA(x, pPeriod1, -1);
				else if(pMA1Type == MA_Crosses_type.KAMA)       TheMA1 = KAMA(x, pKAMAfast1, pPeriod1, pKAMAslow1);
				else if(pMA1Type == MA_Crosses_type.VWMA)       TheMA1 = VWMA(x, pPeriod1);
				else if(pMA1Type == MA_Crosses_type.VMA)        TheMA1 = VMA(x, pPeriod1, pFVolatilityPeriod);
				else if(pMA1Type == MA_Crosses_type.RMA)        TheMA1 = IW_RMA(x, pPeriod1);

				if(pMA2DataType == MA_Crosses_DataType.Open)    x = Open;
				if(pMA2DataType == MA_Crosses_DataType.High)    x = High;
				if(pMA2DataType == MA_Crosses_DataType.Low)     x = Low;
				if(pMA2DataType == MA_Crosses_DataType.Median)  x = Median;
				if(pMA2DataType == MA_Crosses_DataType.Typical) x = Typical;
//				switch (pMA2Type)
//				{	case MA_Crosses_type.SMA  : TheMA2 = SMA(x,pPeriod2); break;
//					case MA_Crosses_type.EMA  : TheMA2 = EMA(x,pPeriod2); break;
//					case MA_Crosses_type.WMA  : TheMA2 = WMA(x,pPeriod2); break;
//					case MA_Crosses_type.HMA  : TheMA2 = HMA(x,pPeriod2); break;
//					case MA_Crosses_type.TEMA : TheMA2 = TEMA(x,pPeriod2); break;
//					case MA_Crosses_type.VWMA : TheMA2 = VWMA(x,pPeriod2); break;
//					case MA_Crosses_type.TSF  : TheMA2 = TSF(x,1,pPeriod2); break;
//					case MA_Crosses_type.LinReg  : TheMA2 = LinReg(pPeriod2); break;
//				}
				if(pMA2Type == MA_Crosses_type.EMA)       TheMA2 = EMA(x,pPeriod2);
				else if(pMA2Type == MA_Crosses_type.SMA)  TheMA2 = SMA(x,pPeriod2);
				else if(pMA2Type == MA_Crosses_type.HMA)  TheMA2 = HMA(x,pPeriod2);
				else if(pMA2Type == MA_Crosses_type.SMMA) TheMA2 = IW_SMMA(x, pPeriod2);
				else if(pMA2Type == MA_Crosses_type.TMA)  TheMA2 = TMA(x,pPeriod2);
				else if(pMA2Type == MA_Crosses_type.TEMA) TheMA2 = TEMA(x,pPeriod2);
				else if(pMA2Type == MA_Crosses_type.WMA)  TheMA2 = WMA(x,pPeriod2);
				else if(pMA2Type == MA_Crosses_type.LinReg)        TheMA2 = LinReg(x, pPeriod2);
				else if(pMA2Type == MA_Crosses_type.ZeroLagEMA)    TheMA2 = ZLEMA(x, pPeriod2);
				else if(pMA2Type == MA_Crosses_type.ZeroLagHATEMA) TheMA2 = IW_ZeroLagHATEMA(x, pPeriod2);
				else if(pMA2Type == MA_Crosses_type.ZeroLagTEMA)   TheMA2 = IW_ZeroLagTEMA(x, pPeriod2, -1);
				else if(pMA2Type == MA_Crosses_type.KAMA)       TheMA2 = KAMA(x, pKAMAfast2, pPeriod2, pKAMAslow2);
				else if(pMA2Type == MA_Crosses_type.VWMA)       TheMA2 = VWMA(x, pPeriod2);
				else if(pMA2Type == MA_Crosses_type.VMA)        TheMA2 = VMA(x, pPeriod2, pFVolatilityPeriod);
				else if(pMA2Type == MA_Crosses_type.RMA)        TheMA2 = IW_RMA(x, pPeriod2);
				#endregion
			}

			if(pZoneSize>0)
				zone = pZoneSize*TickSize;
			else if(pATRperiod>0)
				zone = ATR(pATRperiod)[0] * pATRmult;

//if(CurrentBars[0]>3){
//	Draw.Line(this,"UL",0,TheMA2[0]+zone,2,TheMA2[0]+zone,Brushes.Lime);
//	Draw.Line(this,"LL",0,TheMA2[0]-zone,2,TheMA2[0]-zone,Brushes.Pink);
//}
			if(pPrintZoneSizeToOutputWindow) Print(Times[0][0].ToString()+"   zone: "+zone.ToString());

			if(TimeOfText != DateTime.MaxValue && ChartControl!=null) {
				TimeSpan ts = new TimeSpan(Math.Abs(TimeOfText.Ticks-NinjaTrader.Core.Globals.Now.Ticks));
				if(ts.TotalSeconds>5) RemoveDrawObject("infomsg");
				else Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center,ChartControl.Properties.AxisPen.Brush, ChartControl.Properties.LabelFont, Brushes.Black, BackBrush, 10);
			}
			if(DirectionFillRegionName == DateTime.MinValue)
				DirectionFillRegionName = Time[0];


			BackBrush = null;
			Signal[0] = (FLAT);
			string tag = string.Format("ma_crosses_{0}",CurrentBar);
			RemoveDrawObject(tag);
			if(IsFirstTickOfBar) {
				AlertsThisBar=0;
				EmailsThisBar = 0;
				if(CurrentBar>1) {
					SignalTrend[0] = (SignalTrend[1]);
					if(Signal[1]>FLAT) Direction = LONG;
					if(Signal[1]<FLAT) Direction = SHORT;
					if(Direction==LONG) SignalTrend[0] = (LONG);
					else if(Direction==SHORT) SignalTrend[0] = (SHORT);
					else SignalTrend[0] = (FLAT);
				}
			}
			
			if(SignalTrend[0]!=FLAT && pBackgroundOpacity>0 && pSignalTrend_BackgroundColor.A>0) {
				BackBrush = new SolidColorBrush(Color.FromArgb((byte)(this.pBackgroundOpacity*25), pSignalTrend_BackgroundColor.R,pSignalTrend_BackgroundColor.G,pSignalTrend_BackgroundColor.B));
			}

//			if(CurrentBar>0) {
//				offsetFast[0] = (offsetFast[1]);
//				offsetSlow[0] = (offsetSlow[1]);
//			}
			if(CurrentBar<3) return;

			bool CrossUp = false;
			bool CrossDown = false;
			bool CrossOutwardUp = false;
			bool CrossOutwardDown = false;
			if(zone == 0) {
				if(CrossAbove(TheMA1, TheMA2, 1)) CrossUp=true;
				else CrossDown = CrossBelow(TheMA1, TheMA2, 1);
			} else {
				if(     TheMA1[1]-TheMA2[1] > zone && TheMA1[0]-TheMA2[0] <= zone) CrossDown = true;
				else if(TheMA2[1]-TheMA1[1] > zone && TheMA2[0]-TheMA1[0] <= zone) CrossUp = true;
				else if(Math.Abs(TheMA1[1]-TheMA2[1]) <= zone) {//MA's are within the zone already
					if(TheMA1[0]-TheMA2[0] > zone)      CrossOutwardUp = true;
					else if(TheMA2[0]-TheMA1[0] > zone) CrossOutwardDown = true;
				}
			}

			string msg = string.Empty;
			if(pShowInwardSignals || zone==0) {
				#region Show Inward cross, or all crosses if ZoneSize==0
				if(CrossUp) {	
					Signal[0] = (SIGNAL_MA1_OVER_MA2_CrossInward);

					SayIt(pSpeakOnUpwardCross,Close[0]);

					if(zone==0){
						msg = string.Format("MAC is LONG on {0}",Bars.BarsPeriod.ToString());
						if(pMsgLength==MA_Crosses_MsgLengthType.Verbose)
							msg = string.Concat(msg, " ",this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed ABOVE ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
					}else{
						msg = string.Format("MAC is LONG on {0}, IC ",Bars.BarsPeriod.ToString());
						if(pMsgLength==MA_Crosses_MsgLengthType.Verbose)
							msg = string.Concat(msg," ",this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed ABOVE ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
					}
					if(AlertsThisBar<pMaxAlerts && pCrossInwardUpAlertSound) {
						Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileInwardUp), 1, Brushes.Green, Brushes.White);
//						PlaySound(AddSoundFolder(pSoundFileInwardUp));
						AlertsThisBar++;
					}
					if(pLaunchPopup && !(State == State.Historical) && PopupBar != CurrentBar) {
						Log(msg, LogLevel.Alert);
						PopupBar = CurrentBar;
					}
					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
						EmailsThisBar++;
						msg = string.Format("MAC is LONG on {0} {1}", Instrument.ToString(), Bars.BarsPeriod.ToString());
						Subj = msg;
						Body = string.Concat(NL,NL,this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed ABOVE ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
						SendMail(emailaddress,Subj,Body);
					}
					if(pSymbolTypeInward==MA_Crosses_SymbolType.Arrows)
						Draw.ArrowUp(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pInwardUpColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Triangles)
						Draw.TriangleUp(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pInwardUpColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Dots)
						Draw.Dot(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pInwardUpColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Squares)
						Draw.Square(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pInwardUpColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Diamonds)
						Draw.Diamond(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pInwardUpColor"]);
				} else if (CrossDown) {
					Signal[0] = (SIGNAL_MA1_UNDER_MA2_CrossInward);

					if(zone==0){
						msg = string.Format("MAC is SHORT on {0}",Bars.BarsPeriod.ToString());
						if(pMsgLength==MA_Crosses_MsgLengthType.Verbose)
							msg = string.Concat(msg, " ",this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed BELOW ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
					}else{
						msg = string.Format("MAC is SHORT on {0}, IC ",Bars.BarsPeriod.ToString());
						if(pMsgLength==MA_Crosses_MsgLengthType.Verbose)
							msg = string.Concat(msg," ",this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed BELOW ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
					}
					SayIt(pSpeakOnDownwardCross,Close[0]);
					if(AlertsThisBar<pMaxAlerts && pCrossInwardDownAlertSound) {
						Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileInwardDown), 1, Brushes.Red, Brushes.White);
//						PlaySound(AddSoundFolder(pSoundFileInwardDown));
						AlertsThisBar++;
					}
					if(pLaunchPopup && !(State == State.Historical) && PopupBar != CurrentBar) {
						Log(msg, LogLevel.Alert);
						PopupBar = CurrentBar;
					}
					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
						EmailsThisBar++;
						msg = string.Format("MA_Crosses is SHORT on {0} {1}", Instrument.ToString(), Bars.BarsPeriod.ToString());
						Subj = msg;
						Body = string.Concat(NL,NL,this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed BELOW ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
						SendMail(emailaddress,Subj,Body);
					}
					if(pSymbolTypeInward==MA_Crosses_SymbolType.Arrows)
						Draw.ArrowDown(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pInwardDownColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Triangles)
						Draw.TriangleDown(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pInwardDownColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Dots)
						Draw.Dot(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pInwardDownColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Squares)
						Draw.Square(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pInwardDownColor"]);
					else if(pSymbolTypeInward==MA_Crosses_SymbolType.Diamonds)
						Draw.Diamond(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pInwardDownColor"]);
				}
				#endregion
			}
			if(pShowOutwardSignals && zone > 0) {
				#region Outward Cross
				if(CrossOutwardUp) {	
					Signal[0] = (SIGNAL_MA1_OVER_MA2_CrossOutward);

					SayIt(pSpeakOnUpwardCross,Close[0]);

					msg = string.Format("MAC is LONG on {0}, C ",Bars.BarsPeriod.ToString());
					if(pMsgLength==MA_Crosses_MsgLengthType.Verbose)
						msg = string.Concat(msg," ",this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed ABOVE ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");

					if(AlertsThisBar<pMaxAlerts && pCrossOutwardUpAlertSound) {
						Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileOutwardUp), 1, Brushes.Green, Brushes.White);
//						PlaySound(AddSoundFolder(pSoundFileOutwardUp));
						AlertsThisBar++;
					}
					if(pLaunchPopup && !(State == State.Historical) && PopupBar != CurrentBar) {
						Log(msg ,LogLevel.Alert);
						PopupBar = CurrentBar;
					}
					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
						EmailsThisBar++;
						msg = string.Format("MAC is LONG on {0} {1}", Instrument.ToString(), Bars.BarsPeriod.ToString());
						Subj = msg;
						Body = string.Concat(NL,NL,this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed ABOVE ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
						SendMail(emailaddress,Subj,Body);
					}
					if(pSymbolTypeOutward==MA_Crosses_SymbolType.Arrows)
						Draw.ArrowUp(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pOutwardUpColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Triangles
						)Draw.TriangleUp(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pOutwardUpColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Dots)
						Draw.Dot(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pOutwardUpColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Squares)
						Draw.Square(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pOutwardUpColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Diamonds)
						Draw.Diamond(this, tag,this.IsAutoScale,0,Low[0]-pSeparation*TickSize, MyBrushes["pOutwardUpColor"]);
				} else if (CrossOutwardDown) {
					Signal[0] = (SIGNAL_MA1_UNDER_MA2_CrossOutward);

					SayIt(pSpeakOnDownwardCross,Close[0]);
					msg = string.Format("MAC is SHORT on {0}, C ",Bars.BarsPeriod.ToString());
					if(pMsgLength==MA_Crosses_MsgLengthType.Verbose)
						msg = string.Concat(msg," ",this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed BELOW ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");

					if(AlertsThisBar<pMaxAlerts && pCrossOutwardDownAlertSound) {
						Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileOutwardDown), 1, Brushes.Red, Brushes.White);
//						PlaySound(AddSoundFolder(pSoundFileOutwardDown));
						AlertsThisBar++;
					}
					if(pLaunchPopup && !(State == State.Historical) && PopupBar != CurrentBar) {
						Log(msg, LogLevel.Alert);
						PopupBar = CurrentBar;
					}
					if(EmailsThisBar<pMaxEmails && emailaddress.Length>0) {
						EmailsThisBar++;
						msg = string.Format("MAC is SHORT on {0} {1}", Instrument.ToString(), Bars.BarsPeriod.ToString());
						Subj = msg;
						Body = string.Concat(NL,NL,this.pMA1Type.ToString(),"(",this.pPeriod1.ToString(),") has crossed BELOW ",this.pMA2Type.ToString(),"(",this.pPeriod2.ToString(),")");
						SendMail(emailaddress,Subj,Body);
					}
					if(pSymbolTypeOutward==MA_Crosses_SymbolType.Arrows)
						Draw.ArrowDown(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pOutwardDownColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Triangles)
						Draw.TriangleDown(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pOutwardDownColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Dots)
						Draw.Dot(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pOutwardDownColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Squares)
						Draw.Square(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pOutwardDownColor"]);
					else if(pSymbolTypeOutward==MA_Crosses_SymbolType.Diamonds)
						Draw.Diamond(this, tag,this.IsAutoScale,0,High[0]+pSeparation*TickSize, MyBrushes["pOutwardDownColor"]);
				}
				#endregion
			}
			if(Signal[0] != 0 && pBackgroundOpacity>0 && pSignal_BackgroundColor != Brushes.Transparent){
				BackBrush = MyBrushes["pSignal_BackgroundColor"];//new SolidColorBrush(Color.FromArgb((byte)(this.pBackgroundOpacity*25), pSignal_BackgroundColor.R,pSignal_BackgroundColor.G,pSignal_BackgroundColor.B));
			}

			for(int x = 0; x<CurrentBar-2; x++) {
				if(Direction==LONG)  if(TheMA1[x]<TheMA2[x]) {SignalTrendAge[0] = (x); break;}
				if(Direction==SHORT) if(TheMA1[x]>TheMA2[x]) {SignalTrendAge[0] = (-x);break;}
			}
			
			#region Bar to MA1 and Bar to MA2 related signals
			BarToMA1[0] = (0);
			BarToMA2[0] = (0);
			if(Open[0] <= TheMA1[0] && Close[0] > TheMA1[0]){
				BarToMA1[0] = (BarToMA1_CROSSES_MA1_UPWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleOverMA1.Length>0) {
					msg = string.Format("MA_Crosses Price bar is straddling above MA1 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileStraddleOverMA1), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileStraddleOverMA1));
					AlertsThisBar++;
				}
			}
			else if(Open[0] >= TheMA1[0] && Close[0] < TheMA1[0]){
				BarToMA1[0] = (BarToMA1_CROSSES_MA1_DOWNWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleUnderMA1.Length>0) {
					msg = string.Format("MA_Crosses Price bar is straddling below MA1 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileStraddleUnderMA1), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileStraddleUnderMA1));
					AlertsThisBar++;
				}
			}
			else if(Low[1] > TheMA1[1]) {
				BarToMA1[0] = (BarToMA1_BAR_ABOVE_MA1);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarOverMA1.Length>0) {
					msg = string.Format("MA_Crosses Price bar is over MA1 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileBarOverMA1), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileBarOverMA1));
					AlertsThisBar++;
				}
			}
			else if(High[1] < TheMA1[1]) {
				BarToMA1[0] = (BarToMA1_BAR_BELOW_MA1);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarUnderMA1.Length>0) {
					msg = string.Format("MA_Crosses Price bar is below MA1 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileBarUnderMA1), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileBarUnderMA1));
					AlertsThisBar++;
				}
			}
			if(Open[0] <= TheMA2[0] && Close[0] > TheMA2[0]){
				BarToMA2[0] = (BarToMA2_CROSSES_MA2_UPWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleOverMA2.Length>0) {
					msg = string.Format("MA_Crosses Price bar is straddling above MA2 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileStraddleOverMA2), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileStraddleOverMA2));
					AlertsThisBar++;
				}
			}
			else if(Open[0] >= TheMA2[0] && Close[0] < TheMA2[0]){
				BarToMA2[0] = (BarToMA2_CROSSES_MA2_DOWNWARD);
				if(AlertsThisBar<pMaxAlerts && pSoundFileStraddleUnderMA2.Length>0) {
					msg = string.Format("MA_Crosses Price bar is straddling under MA2 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileStraddleUnderMA2), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileStraddleUnderMA2));
					AlertsThisBar++;
				}
			}
			else if(Low[1] > TheMA2[1]){
				BarToMA2[0] = (BarToMA2_BAR_ABOVE_MA2);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarOverMA2.Length>0) {
					msg = string.Format("MA_Crosses Price bar is above MA2 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileBarOverMA2), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileBarOverMA2));
					AlertsThisBar++;
				}
			}
			else if(High[1] < TheMA2[1]) {
				BarToMA2[0] = (BarToMA2_BAR_BELOW_MA2);
				if(AlertsThisBar<pMaxAlerts && pSoundFileBarUnderMA2.Length>0) {
					msg = string.Format("MA_Crosses Price bar is under MA2 {0}",Bars.BarsPeriod.ToString());
					Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileBarUnderMA2), 1, Brushes.Black, Brushes.White);
//					PlaySound(AddSoundFolder(pSoundFileBarUnderMA2));
					AlertsThisBar++;
				}
			}
			#endregion
			if(pBackgroundOpacity>0 && pBarToMA1_BackgroundColor!=Brushes.Transparent) {
				if(BarToMA1[0] == BarToMA1_CROSSES_MA1_UPWARD || BarToMA1[0] == BarToMA1_CROSSES_MA1_DOWNWARD){
					BackBrush = MyBrushes["pBarToMA1_BackgroundColor"];//new SolidColorBrush(Color.FromArgb((byte)(this.pBackgroundOpacity*25), pBarToMA1_BackgroundColor.R,pBarToMA1_BackgroundColor.G,pBarToMA1_BackgroundColor.B));
				}
			}
			if(pBackgroundOpacity>0 && pBarToMA2_BackgroundColor!=Brushes.Transparent) {
				if(BarToMA2[0] == BarToMA2_CROSSES_MA2_UPWARD || BarToMA2[0] == BarToMA2_CROSSES_MA2_DOWNWARD){
					BackBrush = MyBrushes["pBarToMA2_BackgroundColor"];//new SolidColorBrush(Color.FromArgb((byte)(this.pBackgroundOpacity*25), pBarToMA2_BackgroundColor.R,pBarToMA2_BackgroundColor.G,pBarToMA2_BackgroundColor.B));
				}
			}

			Values[0][0] = (TheMA1[0]);
			Values[1][0] = (TheMA2[0]);
			if(!pShowMALines) {
				PlotBrushes[0][0] = Brushes.Transparent;
				PlotBrushes[1][0] = Brushes.Transparent;
			} else {
				if(pColoringBasis == MA_Crosses_ColoringBasis.OnCrossing) {
					if(TheMA1[0]>TheMA2[1]) {
						PlotBrushes[0][0] = pLineUpColorFast;
						PlotBrushes[1][0] = pLineUpColorSlow;
					} else {
						PlotBrushes[0][0] = pLineDownColorFast;
						PlotBrushes[1][0] = pLineDownColorSlow;
					}
				} else if(pColoringBasis == MA_Crosses_ColoringBasis.OnTrendChange) {
					if(TheMA1[0]>TheMA1[1]) PlotBrushes[0][0] = pLineUpColorFast;
					else PlotBrushes[0][0] = pLineDownColorFast;
					if(TheMA2[0]>TheMA2[1]) PlotBrushes[1][0] = pLineUpColorSlow;
					else PlotBrushes[1][0] = pLineDownColorSlow;
				} else if(pColoringBasis == MA_Crosses_ColoringBasis.NoColorChange) {
					PlotBrushes[0][0] = pLineUpColorFast;
					PlotBrushes[1][0] = pLineUpColorSlow;
				}
			}
			if(this.pShowDirectionFill) {
				#region Show Direction fill region
				if(!StartFillRegion) {
					if(TheMA1[0]>TheMA2[0] && TheMA1[1]<=TheMA2[1]) StartFillRegion = true;
					if(TheMA1[0]<TheMA2[0] && TheMA1[1]>=TheMA2[1]) StartFillRegion = true;
					if(StartFillRegion) DirectionFillRegionName = Time[0];
				}
				if(StartFillRegion) {
					if(TheMA1[0]>TheMA2[0]) {
						if(TheMA1[1]<=TheMA2[1]) {
							int rbar = 0;
							while(rbar<CurrentBar) {
								DirectionFillRegionName = Time[rbar];
								if(TheMA1[rbar]<TheMA2[rbar]) break;
								rbar++;
							}
						}
						Draw.Region(this, DirectionFillRegionName.ToString(), DirectionFillRegionName, Time[0], TheMA1,TheMA2, Brushes.Transparent, MyBrushes["pMA1overMA2FillColor"], pOpacity_MA1overMA2);
					}
					if(TheMA1[0]<TheMA2[0]) {
						if(TheMA1[1]>=TheMA2[1]) {
							int rbar = 0;
							while(rbar<CurrentBar) {
								DirectionFillRegionName = Time[rbar];
								if(TheMA1[rbar]>TheMA2[rbar]) break;
								rbar++;
							}
						}
						Draw.Region(this, DirectionFillRegionName.ToString(), DirectionFillRegionName, Time[0], TheMA1,TheMA2, Brushes.Transparent, MyBrushes["pMA1underMA2FillColor"], pOpacity_MA1underMA2);
					}
				}
				#endregion
			}

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
				GetStandardValues(ITypeDescriptorContext context)
			{
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";

				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("NO SOUND");
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

		#region Plots
		//==================================================================
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MA1
		{get { return Values[0]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MA2
		{get { return Values[1]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Signal
		{get { return Values[2]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SignalTrend
		{get { return Values[3]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SignalTrendAge
		{get { return Values[4]; }}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BarToMA1
		{get { return Values[5]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BarToMA2
		{get { return Values[6]; }}
		//==================================================================
		#endregion
		public override string ToString(){
			string type1 = MA1Type.ToString()+"("+pPeriod1.ToString()+")";
			if(MA1Type==MA_Crosses_type.VMA)  type1 = "VMA("+this.pPeriod1.ToString()+","+this.pFVolatilityPeriod.ToString()+")";
			if(MA1Type==MA_Crosses_type.KAMA) type1 = "KAMA("+this.pKAMAfast1.ToString()+","+this.pPeriod1.ToString()+","+this.pKAMAslow1.ToString()+")";
			string type2 = MA2Type.ToString()+"("+pPeriod2.ToString()+")";
			if(MA2Type==MA_Crosses_type.VMA)  type2 = "VMA("+this.pPeriod2.ToString()+","+this.pSVolatilityPeriod.ToString()+")";
			if(MA2Type==MA_Crosses_type.KAMA) type2 = "KAMA("+this.pKAMAfast2.ToString()+","+this.pPeriod2.ToString()+","+this.pKAMAslow2.ToString()+")";
			return "MA Crosses "+type1+" x "+type2;
		}
		#region Plot parameters
		private int	pLineWidthFast = 2;
		[Description("Width of Fast MA line")]
		[Category("Plots")]
		public int LineWidthFast
		{
			get { return pLineWidthFast; }
			set { pLineWidthFast = Math.Max(1,value); }
		}
		private int	pLineWidthSlow = 3;
		[Description("Width of Slow MA line")]
		[Category("Plots")]
		public int LineWidthSlow
		{
			get { return pLineWidthSlow; }
			set { pLineWidthSlow = Math.Max(1,value); }
		}
		private PlotStyle pLineStyleFast = PlotStyle.Line;
		[Description("Style of Fast MA line")]
		[Category("Plots")]
		public PlotStyle LineStyleFast
		{
			get { return pLineStyleFast; }
			set { pLineStyleFast = value; }
		}
		private PlotStyle pLineStyleSlow = PlotStyle.Line;
		[Description("Style of Slow MA line")]
		[Category("Plots")]
		public PlotStyle LineStyleSlow
		{
			get { return pLineStyleSlow; }
			set { pLineStyleSlow = value; }
		}
		private DashStyleHelper pLineDashStyleFast = DashStyleHelper.Solid;
		[Description("DashStyle of Fast MA line")]
		[Category("Plots")]
		public DashStyleHelper LineDashStyleFast
		{
			get { return pLineDashStyleFast; }
			set { pLineDashStyleFast = value; }
		}
		private DashStyleHelper pLineDashStyleSlow = DashStyleHelper.Solid;
		[Description("DashStyle of Slow MA line")]
		[Category("Plots")]
		public DashStyleHelper LineDashStyleSlow
		{
			get { return pLineDashStyleSlow; }
			set { pLineDashStyleSlow = value; }
		}
		private System.Windows.Media.Brush pLineUpColorFast = Brushes.Cyan;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast Up",  GroupName = "Plots")]
		public System.Windows.Media.Brush LupFC{	get { return pLineUpColorFast; }	set { pLineUpColorFast = value; }		}
		[Browsable(false)]
		public string LupFClSerialize
		{	get { return Serialize.BrushToString(pLineUpColorFast); } set { pLineUpColorFast = Serialize.StringToBrush(value); }
		}
		private System.Windows.Media.Brush pLineDownColorFast = Brushes.Magenta;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast Down",  GroupName = "Plots")]
		public System.Windows.Media.Brush LDownFC{	get { return pLineDownColorFast; }	set { pLineDownColorFast = value; }		}
		[Browsable(false)]
		public string LDownFClSerialize
		{	get { return Serialize.BrushToString(pLineDownColorFast); } set { pLineDownColorFast = Serialize.StringToBrush(value); }
		}
		private System.Windows.Media.Brush pLineUpColorSlow = Brushes.Blue;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow Up",  GroupName = "Plots")]
		public System.Windows.Media.Brush LupSC{	get { return pLineUpColorSlow; }	set { pLineUpColorSlow = value; }		}
		[Browsable(false)]
		public string LupSClSerialize
		{	get { return Serialize.BrushToString(pLineUpColorSlow); } set { pLineUpColorSlow = Serialize.StringToBrush(value); }
		}
		private System.Windows.Media.Brush pLineDownColorSlow = Brushes.Red;
		[XmlIgnore()]
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow Down",  GroupName = "Plots")]
		public System.Windows.Media.Brush LDownSC{	get { return pLineDownColorSlow; }	set { pLineDownColorSlow = value; }		}
		[Browsable(false)]
		public string LDownSClSerialize
		{	get { return Serialize.BrushToString(pLineDownColorSlow); } set { pLineDownColorSlow = Serialize.StringToBrush(value); }
		}
		#endregion

		
		#region Properties
		#region Background
		private int pBackgroundOpacity = 3;
		[Description("Opacity (0-10) for Bkgrnd colors.  Set to '0' to turn-off coloring")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bkgrnd Opacity",  GroupName = "Background")]
		public int BackgroundOpacity
		{	get { return pBackgroundOpacity; }	set { pBackgroundOpacity = value; }	}

		private Color pSignalTrend_BackgroundColor = Colors.Transparent;
//		[XmlIgnore()]
//		[Description("Colorize the background of the chart when the SignalTrend is in a signal condition")]
//		[Category("Background")]
//		[Gui.Design.DisplayNameAttribute("SignalTrend")]
//		public Brush SignalTrendBC{	get { return pSignalTrend_BackgroundColor; }	set { pSignalTrend_BackgroundColor = value; }		}
//		[Browsable(false)]
//		public string SignalTrendClSerialize
//		{	get { return Serialize.BrushToString(pSignalTrend_BackgroundColor); } set { pSignalTrend_BackgroundColor = Serialize.StringToBrush(value); }
//		}
		private Brush pSignal_BackgroundColor = Brushes.Transparent;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when the Signal is in a signal condition")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal",  GroupName = "Background")]
		public Brush SignalBC{	get { return pSignal_BackgroundColor; }	set { pSignal_BackgroundColor = value; }		}
		[Browsable(false)]
		public string SignalClSerialize
		{	get { return Serialize.BrushToString(pSignal_BackgroundColor); } set { pSignal_BackgroundColor = Serialize.StringToBrush(value); }
		}
		private Brush pBarToMA1_BackgroundColor = Brushes.Transparent;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when the BarToMA1 is in a signal condition")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarToMA1",  GroupName = "Background")]
		public Brush B2MA1BC{	get { return pBarToMA1_BackgroundColor; }	set { pBarToMA1_BackgroundColor = value; }		}
		[Browsable(false)]
		public string B2MA1ClSerialize
		{	get { return Serialize.BrushToString(pBarToMA1_BackgroundColor); } set { pBarToMA1_BackgroundColor = Serialize.StringToBrush(value); }
		}
		private Brush pBarToMA2_BackgroundColor = Brushes.Transparent;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when the BarToMA2 is in a signal condition")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarToMA2",  GroupName = "Background")]
		public Brush B2MA2BC{	get { return pBarToMA2_BackgroundColor; }	set { pBarToMA2_BackgroundColor = value; }		}
		[Browsable(false)]
		public string B2MA2ClSerialize
		{	get { return Serialize.BrushToString(pBarToMA2_BackgroundColor); } set { pBarToMA2_BackgroundColor = Serialize.StringToBrush(value); }
		}
		#endregion

		string pSpeakOnUpwardCross = "/[InstrumentName] [TimeFrame] long";
		string pSpeakOnDownwardCross = "/[InstrumentName] [TimeFrame] short";
#if SPEECH_ENABLED
		[Description("Message to speak when fast MA crosses over the slow...leave blank to turn-off spoken message, or put an '/' in the front of the string")]
		[Category("Alert Voice")]
		public string SpeakOnUpwardCross
		{
			get { return pSpeakOnUpwardCross; }
			set { pSpeakOnUpwardCross = value; }
		}
		[Description("Message to speak when fast MA crosses below the slow...leave blank to turn-off spoken message, or put an '/' in the front of the string")]
		[Category("Alert Voice")]
		public string SpeakOnDownwardCross
		{
			get { return pSpeakOnDownwardCross; }
			set { pSpeakOnDownwardCross = value; }
		}
#endif
        [NinjaScriptProperty]
		[Description("MA period 1")]
		[Category("Parameters")]
		public int Period1
		{
			get { return pPeriod1; }
			set { pPeriod1 = Math.Max(1, value); }
		}

        [NinjaScriptProperty]
		[Description("MA period 2")]
		[Category("Parameters")]
		public int Period2
		{
			get { return pPeriod2; }
			set { pPeriod2 = Math.Max(1, value); }
		}

		private int pZoneSize = 0;
        [NinjaScriptProperty]
        [Display(Name = "Valid Zone Size", GroupName = "Zone Parameters", Description = "Tick size of the 'zone region' between the two MA's...set to '0' to turn-off tick-based zone calculations", Order = 0)]
		public int ValidZoneSize
		{
			get { return pZoneSize; }
			set { pZoneSize = Math.Max(0, value); }
		}

		private int pATRperiod = 0;
        [NinjaScriptProperty]
        [Display(Name = "ATR period", GroupName = "Zone Parameters", Description = "Period for ATR...set to '0' to turn-off ATR-based zone calculations", Order = 10)]
		public int ATRperiod
		{
			get { return pATRperiod; }
			set { pATRperiod = Math.Max(0, value); }
		}
		private double pATRmult = 0.1;
        [NinjaScriptProperty]
        [Display(Name = "ATR mult", GroupName = "Zone Parameters", Description = "Multiplier for ATR.  If ATRperiod>0, then this multiplier will determine the zone size", Order = 20)]
		public double ATRmult
		{
			get { return pATRmult; }
			set { pATRmult = Math.Max(0, value); }
		}
		private bool pPrintZoneSizeToOutputWindow = true;
        [NinjaScriptProperty]
        [Display(Name = "Print ZoneSize", GroupName = "Zone Parameters", Description = "Print your zone sizes to the OutputWindow", Order = 30)]
		public bool PrintZoneSizeToOutputWindow
		{
			get { return pPrintZoneSizeToOutputWindow; }
			set { pPrintZoneSizeToOutputWindow = value; }
		}

		private bool pShowInwardSignals = true;
        [NinjaScriptProperty]
		[Description("Fire alerts when the separation distance between the two MA's becomes less than the zone region")]
		[Category("Parameters")]
		public bool ShowInwardSignals
		{
			get { return pShowInwardSignals; }
			set { pShowInwardSignals = value; }
		}
		private bool pShowOutwardSignals = true;
        [NinjaScriptProperty]
		[Description("Fire alerts when the separation distance between the two MA's becomes greater than the zone region")]
		[Category("Parameters")]
		public bool ShowOutwardSignals
		{
			get { return pShowOutwardSignals; }
			set { pShowOutwardSignals = value; }
		}
		private int pFVolatilityPeriod = 9;
        [NinjaScriptProperty]
		[Description("Volatility period for Fast VMA, used to calculate the CMO-based volatility index")]
		[Category("Parameters VMA")]
		public int VMA1_VolatilityPeriod
		{
			get { return pFVolatilityPeriod; }
			set { pFVolatilityPeriod = Math.Max(1, value); }
		}
		private int pSVolatilityPeriod = 9;
        [NinjaScriptProperty]
		[Description("Volatility period for Slow VMA, used to calculate the CMO-based volatility index")]
		[Category("Parameters VMA")]
		public int VMA2_VolatilityPeriod
		{
			get { return pSVolatilityPeriod; }
			set { pSVolatilityPeriod = Math.Max(1, value); }
		}
		private int pKAMAfast1 = 2;
        [NinjaScriptProperty]
		[Description("Number of bars for KAMA MA1 Fast period (between 1 and 125)")]
		[Category("Parameters KAMA")]
		public int KAMAfast1
		{
			get { return pKAMAfast1; }
			set { pKAMAfast1 = Math.Min(125, Math.Max(1, value)); }
		}
		private int pKAMAslow1 = 30;
        [NinjaScriptProperty]
		[Description("Number of bars for KAMA MA1 Slow period (between 1 and 125)")]
		[Category("Parameters KAMA")]
		public int KAMAslow1
		{
			get { return pKAMAslow1; }
			set { pKAMAslow1 = Math.Min(125, Math.Max(1, value)); }
		}
		private int pKAMAfast2 = 2;
        [NinjaScriptProperty]
		[Description("Number of bars for KAMA MA2 Fast period (between 1 and 125)")]
		[Category("Parameters KAMA")]
		public int KAMAfast2
		{
			get { return pKAMAfast2; }
			set { pKAMAfast2 = Math.Min(125, Math.Max(1, value)); }
		}
		private int pKAMAslow2 = 30;
        [NinjaScriptProperty]
		[Description("Number of bars for KAMA MA2 Slow period (between 1 and 125)")]
		[Category("Parameters KAMA")]
		public int KAMAslow2
		{
			get { return pKAMAslow2; }
			set { pKAMAslow2 = Math.Min(125, Math.Max(1, value)); }
		}

		private int pSeparation = 2;
		[Description("Number of ticks between price bar and arrows")]
		[Category("Visual")]
		public int Separation
		{
			get { return pSeparation; }
			set { pSeparation = Math.Max(1, value); }
		}

		private MA_Crosses_MsgLengthType pMsgLength = MA_Crosses_MsgLengthType.Verbose;
        [NinjaScriptProperty]
		[Description("Length of the Alert message")]
		[Category("Alert")]
		public MA_Crosses_MsgLengthType MsgLength
		{
			get { return pMsgLength; }
			set { pMsgLength = value; }
		}

		private int pMaxEmails = 1;
        [NinjaScriptProperty]
		[Description("Max number of emails per bar, useful if CalculateOnBarClose = false")]
		[Category("Alert")]
		public int MaxEmails
		{
			get { return pMaxEmails; }
			set { pMaxEmails = Math.Max(1, value); }
		}

		private int pMaxAlerts = 1;
        [NinjaScriptProperty]
		[Description("Max number of audio signals per bar, useful if CalculateOnBarClose = false")]
		[Category("Alert")]
		public int MaxAudioAlerts
		{
			get { return pMaxAlerts; }
			set { pMaxAlerts = Math.Max(1, value); }
		}

		private MA_Crosses_ColoringBasis pColoringBasis = MA_Crosses_ColoringBasis.OnCrossing;
		[Description("When to color the MA lines?  When the two MA cross, or when the MA lines change their trend direction?")]
		[Category("Visual")]
		public MA_Crosses_ColoringBasis ColoringBasis
		{
			get { return pColoringBasis; }
			set { pColoringBasis = value; }
		}

		private string pSoundFileInwardUp = "Alert3.wav";
        [NinjaScriptProperty]
		[Description("Sound file when fast crosses up through slow - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Inward Sound")]
		public string SoundFileInwardUp
		{
			get { return pSoundFileInwardUp; }
			set { pSoundFileInwardUp = value; }
		}

		private string pSoundFileInwardDown = "Alert2.wav";
        [NinjaScriptProperty]
		[Description("Sound file when fast crosses down through slow - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Inward Sound")]
		public string SoundFileInwardDown
		{
			get { return pSoundFileInwardDown; }
			set { pSoundFileInwardDown = value; }
		}

		private string pSoundFileOutwardDown = "Alert2.wav";
        [NinjaScriptProperty]
		[Description("Sound file when fast crosses down through slow - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
		public string SoundFileOutwardDown
		{
			get { return pSoundFileOutwardDown; }
			set { pSoundFileOutwardDown = value; }
		}
		private string pSoundFileOutwardUp = "Alert3.wav";
        [NinjaScriptProperty]
		[Description("Sound file when fast crosses up through slow - it must exist in your Sounds folder in order to be played")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
		public string SoundFileOutwardUp
		{
			get { return pSoundFileOutwardUp; }
			set { pSoundFileOutwardUp = value; }
		}

		private MA_Crosses_SymbolType pSymbolTypeInward = MA_Crosses_SymbolType.Arrows;
		[Description("Symbol to draw on each MA cross, or when their separation distance is less than the zone distance")]
		[Category("Inward Visual")]
		public MA_Crosses_SymbolType SymbolTypeInward
		{
			get { return pSymbolTypeInward; }
			set { pSymbolTypeInward = value; }
		}

		private MA_Crosses_SymbolType pSymbolTypeOutward = MA_Crosses_SymbolType.Triangles;
		[Description("Symbol to draw when the separation between the two MA's is greater than the zone distance")]
		[Category("Outward Visual (only used when ValidZoneSize > 0)")]
		public MA_Crosses_SymbolType SymbolTypeOutward
		{
			get { return pSymbolTypeOutward; }
			set { pSymbolTypeOutward = value; }
		}

		private bool pShowMALines = true;
		[Description("Show the lines drawn by the Moving Averages?")]
		[Category("Visual")]
		public bool ShowMAlines
		{
			get { return pShowMALines; }
			set { pShowMALines = value; }
		}

		#region DirectionFill
		private bool pShowDirectionFill = false;
		[Description("Fill the background between the two MA's?")]
		[Category("DirectionFill")]
		public bool ShowDirectionFill
		{
			get { return pShowDirectionFill; }
			set { pShowDirectionFill = value; }
		}
		private Brush pMA1overMA2FillColor = Brushes.Green;
		[XmlIgnore()]
		[Description("Fill color when MA1 is above MA2")]
		[Category("DirectionFill")]
		public Brush Fill_MA1overMA2{	get { return pMA1overMA2FillColor; }	set { pMA1overMA2FillColor = value; }		}
		[Browsable(false)]
		public string MA1overMA2FillColorSerialize
		{	get { return Serialize.BrushToString(pMA1overMA2FillColor); } set { pMA1overMA2FillColor = Serialize.StringToBrush(value); }
		}
		private int pOpacity_MA1overMA2 = 5;
		[Description("Opacity of the fill background color when MA1 is over MA2, 0=transparent, 10=fully opaque")]
		[Category("DirectionFill")]
		public int Opacity_MA1overMA2
		{
			get { return pOpacity_MA1overMA2; }
			set { pOpacity_MA1overMA2 = Math.Max(0,Math.Min(10,value)); }
		}

		private Brush pMA1underMA2FillColor = Brushes.Red;
		[XmlIgnore()]
		[Description("Fill color when MA1 is below MA2")]
		[Category("DirectionFill")]
		public Brush Fill_MA1underMA2{	get { return pMA1underMA2FillColor; }	set { pMA1underMA2FillColor = value; }		}
		[Browsable(false)]
		public string MA1underMA2FillColorSerialize
		{	get { return Serialize.BrushToString(pMA1underMA2FillColor); } set { pMA1underMA2FillColor = Serialize.StringToBrush(value); }
		}
		private int pOpacity_MA1underMA2 = 5;
		[Description("Opacity of the fill background color when MA1 is under MA2, 0=transparent, 10=fully opaque")]
		[Category("DirectionFill")]
		public int Opacity_MA1underMA2
		{
			get { return pOpacity_MA1underMA2; }
			set { pOpacity_MA1underMA2 = Math.Max(0,Math.Min(10,value)); }
		}

		#endregion

		private bool pCrossInwardDownAlertSound = true;
        [NinjaScriptProperty]
		[Description("Play alert sound when the Fast MA comes crosses the Slow MA inward, from above?")]
		[Category("Inward Sound")]
		public bool CrossInwardDownAlert
		{
			get { return pCrossInwardDownAlertSound; }
			set { pCrossInwardDownAlertSound = value; }
		}
		private bool pCrossInwardUpAlertSound = true;
        [NinjaScriptProperty]
		[Description("Play alert sound when the Fast MA comes crosses the Slow MA inward, from below?")]
		[Category("Inward Sound")]
		public bool CrossInwardUpAlert
		{
			get { return pCrossInwardUpAlertSound; }
			set { pCrossInwardUpAlertSound = value; }
		}

		private bool pCrossOutwardDownAlertSound = true;
        [NinjaScriptProperty]
		[Description("Play alert sound when the Fast MA comes crosses the Slow MA outward, from above?")]
		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
		public bool CrossOutwardDownAlert
		{
			get { return pCrossOutwardDownAlertSound; }
			set { pCrossOutwardDownAlertSound = value; }
		}
		private bool pCrossOutwardUpAlertSound = true;
        [NinjaScriptProperty]
		[Description("Play alert sound when the Fast MA comes crosses the Slow MA outward, from below?")]
		[Category("Outward Sound (only used when ValidZoneSize > 0)")]
		public bool CrossOutwardUpAlert
		{
			get { return pCrossOutwardUpAlertSound; }
			set { pCrossOutwardUpAlertSound = value; }
		}
		private string pSoundFileStraddleOverMA1 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the bar opens below MA1 and closes above MA1")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileStraddleOverMA1
		{
			get { return pSoundFileStraddleOverMA1; }
			set { pSoundFileStraddleOverMA1 = value; }
		}
		private string pSoundFileStraddleOverMA2 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the bar opens below MA1 and closes above MA2")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileStraddleOverMA2
		{
			get { return pSoundFileStraddleOverMA2; }
			set { pSoundFileStraddleOverMA2 = value; }
		}
		private string pSoundFileStraddleUnderMA1 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the bar opens below MA1 and closes below MA1")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileStraddleUnderMA1
		{
			get { return pSoundFileStraddleUnderMA1; }
			set { pSoundFileStraddleUnderMA1 = value; }
		}
		private string pSoundFileStraddleUnderMA2 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the bar opens below MA1 and closes below MA2")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileStraddleUnderMA2
		{
			get { return pSoundFileStraddleUnderMA2; }
			set { pSoundFileStraddleUnderMA2 = value; }
		}
		private string pSoundFileBarOverMA1 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the recently closed bar is completely above MA1")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileBarOverMA1
		{
			get { return pSoundFileBarOverMA1; }
			set { pSoundFileBarOverMA1 = value; }
		}
		private string pSoundFileBarUnderMA1 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the recently closed bar is completely below MA1")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileBarUnderMA1
		{
			get { return pSoundFileBarUnderMA1; }
			set { pSoundFileBarUnderMA1 = value; }
		}
		private string pSoundFileBarOverMA2 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the recently closed bar is completely above MA2")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileBarOverMA2
		{
			get { return pSoundFileBarOverMA2; }
			set { pSoundFileBarOverMA2 = value; }
		}
		private string pSoundFileBarUnderMA2 = "";
        [NinjaScriptProperty]
		[Description("Sound file when the recently closed bar is completely below MA2")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Straddle Alert")]
		public string SoundFileBarUnderMA2
		{
			get { return pSoundFileBarUnderMA2; }
			set { pSoundFileBarUnderMA2 = value; }
		}

		private bool pLaunchPopup = false;
		[Description("Launch a Popup window?")]
		[Category("Alert")]
		public bool LaunchPopup
		{
			get { return pLaunchPopup; }
			set { pLaunchPopup = value; }
		}
		private string emailaddress = "";
        [NinjaScriptProperty]
		[Description("Supply your address (e.g. 'you@mail.com') for alerts, leave blank to turn-off emails")]
		[Category("Alert")]
		public string EmailAddress
		{
			get { return emailaddress; }
			set { emailaddress = value; }
		}

		[Description("Select one of the input data types")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public MA_Crosses_DataType MA1DataType
		{
			get { return pMA1DataType; }
			set { pMA1DataType = value; }
		}
		[Description("Select one of the indicator types")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public MA_Crosses_type MA1Type
		{
			get { return pMA1Type; }
			set { pMA1Type = value; }
		}
		[Description("Select one of the input data types")]
		[Category("Parameters")]
		[NinjaScriptProperty]
		public MA_Crosses_DataType MA2DataType
		{
			get { return pMA2DataType; }
			set { pMA2DataType = value; }
		}
		[Description("Select one of the indicator types")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public MA_Crosses_type MA2Type
		{
			get { return pMA2Type; }
			set { pMA2Type = value; }
		}

		#region Marker Colors
		private Brush pOutwardUpColor = Brushes.Cyan;
		[XmlIgnore()]
		[Description("Color of marker when MA lines move out of their zone and the Fast is over the Slow")]
// 		[Category("Outward Visual (only used when ValidZoneSize > 0)")]
// [Gui.Design.DisplayNameAttribute("Marker Up")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Up",  GroupName = "Outward Visual (only used when ValidZoneSize > 0)")]
		public Brush OutwardUpColor{	get { return pOutwardUpColor; }	set { pOutwardUpColor = value; }		}
		[Browsable(false)]
		public string pOutwardUpColorSerialize
		{	get { return Serialize.BrushToString(pOutwardUpColor); } set { pOutwardUpColor = Serialize.StringToBrush(value); }
		}

		private Brush pOutwardDownColor = Brushes.Pink;
		[XmlIgnore()]
		[Description("Color of marker when MA lines move out of their zone and the Fast is under the Slow")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Down",  GroupName = "Outward Visual (only used when ValidZoneSize > 0)")]
		public Brush OutwardDownColor{	get { return pOutwardDownColor; }	set { pOutwardDownColor = value; }		}
		[Browsable(false)]
		public string pOutwardDownColorSerialize
		{	get { return Serialize.BrushToString(pOutwardDownColor); } set { pOutwardDownColor = Serialize.StringToBrush(value); }
		}

		private Brush pInwardUpColor = Brushes.Blue;
		[XmlIgnore()]
		[Description("Color of marker when MA lines move into their zone and the Fast is over the Slow")]
// 		[Category("Inward Visual")]
// [Gui.Design.DisplayNameAttribute("Marker Up")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Up",  GroupName = "Inward Visual")]
		public Brush InwardUpColor{	get { return pInwardUpColor; }	set { pInwardUpColor = value; }		}
		[Browsable(false)]
		public string pInwardUpColorSerialize
		{	get { return Serialize.BrushToString(pInwardUpColor); } set { pInwardUpColor = Serialize.StringToBrush(value); }
		}

		private Brush pInwardDownColor = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of marker when MA lines move into their zone and the Fast is under the Slow")]
// 		[Category("Inward Visual")]
// [Gui.Design.DisplayNameAttribute("Marker Down")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Down",  GroupName = "Inward Visual")]
		public Brush InwardDownColor{	get { return pInwardDownColor; }	set { pInwardDownColor = value; }		}
		[Browsable(false)]
		public string pInwardDownColorSerialize
		{	get { return Serialize.BrushToString(pInwardDownColor); } set { pInwardDownColor = Serialize.StringToBrush(value); }
		}
		#endregion

		#endregion
	}
}
	public enum MA_Crosses_type
	{
	SMA,
	EMA,
	WMA,
	TMA,
	TEMA,
	HMA,
	LinReg,
	SMMA,
	ZeroLagEMA,
	ZeroLagTEMA,
	ZeroLagHATEMA,
	KAMA,
	VWMA,
	VMA,
	RMA
	}
public enum MA_Crosses_SymbolType {
	Arrows,Triangles,Diamonds,Squares,Dots,None
}
public enum MA_Crosses_DataType {
	Open,High,Low,Close,Median,Typical
}
public enum MA_Crosses_ColoringBasis {
	OnTrendChange, OnCrossing, NoColorChange
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MA_Crosses[] cacheMA_Crosses;
		public MA_Crosses MA_Crosses(int period1, int period2, int validZoneSize, int aTRperiod, double aTRmult, bool printZoneSizeToOutputWindow, bool showInwardSignals, bool showOutwardSignals, int vMA1_VolatilityPeriod, int vMA2_VolatilityPeriod, int kAMAfast1, int kAMAslow1, int kAMAfast2, int kAMAslow2, MA_Crosses_MsgLengthType msgLength, int maxEmails, int maxAudioAlerts, string soundFileInwardUp, string soundFileInwardDown, string soundFileOutwardDown, string soundFileOutwardUp, bool crossInwardDownAlert, bool crossInwardUpAlert, bool crossOutwardDownAlert, bool crossOutwardUpAlert, string soundFileStraddleOverMA1, string soundFileStraddleOverMA2, string soundFileStraddleUnderMA1, string soundFileStraddleUnderMA2, string soundFileBarOverMA1, string soundFileBarUnderMA1, string soundFileBarOverMA2, string soundFileBarUnderMA2, string emailAddress, MA_Crosses_DataType mA1DataType, MA_Crosses_type mA1Type, MA_Crosses_DataType mA2DataType, MA_Crosses_type mA2Type)
		{
			return MA_Crosses(Input, period1, period2, validZoneSize, aTRperiod, aTRmult, printZoneSizeToOutputWindow, showInwardSignals, showOutwardSignals, vMA1_VolatilityPeriod, vMA2_VolatilityPeriod, kAMAfast1, kAMAslow1, kAMAfast2, kAMAslow2, msgLength, maxEmails, maxAudioAlerts, soundFileInwardUp, soundFileInwardDown, soundFileOutwardDown, soundFileOutwardUp, crossInwardDownAlert, crossInwardUpAlert, crossOutwardDownAlert, crossOutwardUpAlert, soundFileStraddleOverMA1, soundFileStraddleOverMA2, soundFileStraddleUnderMA1, soundFileStraddleUnderMA2, soundFileBarOverMA1, soundFileBarUnderMA1, soundFileBarOverMA2, soundFileBarUnderMA2, emailAddress, mA1DataType, mA1Type, mA2DataType, mA2Type);
		}

		public MA_Crosses MA_Crosses(ISeries<double> input, int period1, int period2, int validZoneSize, int aTRperiod, double aTRmult, bool printZoneSizeToOutputWindow, bool showInwardSignals, bool showOutwardSignals, int vMA1_VolatilityPeriod, int vMA2_VolatilityPeriod, int kAMAfast1, int kAMAslow1, int kAMAfast2, int kAMAslow2, MA_Crosses_MsgLengthType msgLength, int maxEmails, int maxAudioAlerts, string soundFileInwardUp, string soundFileInwardDown, string soundFileOutwardDown, string soundFileOutwardUp, bool crossInwardDownAlert, bool crossInwardUpAlert, bool crossOutwardDownAlert, bool crossOutwardUpAlert, string soundFileStraddleOverMA1, string soundFileStraddleOverMA2, string soundFileStraddleUnderMA1, string soundFileStraddleUnderMA2, string soundFileBarOverMA1, string soundFileBarUnderMA1, string soundFileBarOverMA2, string soundFileBarUnderMA2, string emailAddress, MA_Crosses_DataType mA1DataType, MA_Crosses_type mA1Type, MA_Crosses_DataType mA2DataType, MA_Crosses_type mA2Type)
		{
			if (cacheMA_Crosses != null)
				for (int idx = 0; idx < cacheMA_Crosses.Length; idx++)
					if (cacheMA_Crosses[idx] != null && cacheMA_Crosses[idx].Period1 == period1 && cacheMA_Crosses[idx].Period2 == period2 && cacheMA_Crosses[idx].ValidZoneSize == validZoneSize && cacheMA_Crosses[idx].ATRperiod == aTRperiod && cacheMA_Crosses[idx].ATRmult == aTRmult && cacheMA_Crosses[idx].PrintZoneSizeToOutputWindow == printZoneSizeToOutputWindow && cacheMA_Crosses[idx].ShowInwardSignals == showInwardSignals && cacheMA_Crosses[idx].ShowOutwardSignals == showOutwardSignals && cacheMA_Crosses[idx].VMA1_VolatilityPeriod == vMA1_VolatilityPeriod && cacheMA_Crosses[idx].VMA2_VolatilityPeriod == vMA2_VolatilityPeriod && cacheMA_Crosses[idx].KAMAfast1 == kAMAfast1 && cacheMA_Crosses[idx].KAMAslow1 == kAMAslow1 && cacheMA_Crosses[idx].KAMAfast2 == kAMAfast2 && cacheMA_Crosses[idx].KAMAslow2 == kAMAslow2 && cacheMA_Crosses[idx].MsgLength == msgLength && cacheMA_Crosses[idx].MaxEmails == maxEmails && cacheMA_Crosses[idx].MaxAudioAlerts == maxAudioAlerts && cacheMA_Crosses[idx].SoundFileInwardUp == soundFileInwardUp && cacheMA_Crosses[idx].SoundFileInwardDown == soundFileInwardDown && cacheMA_Crosses[idx].SoundFileOutwardDown == soundFileOutwardDown && cacheMA_Crosses[idx].SoundFileOutwardUp == soundFileOutwardUp && cacheMA_Crosses[idx].CrossInwardDownAlert == crossInwardDownAlert && cacheMA_Crosses[idx].CrossInwardUpAlert == crossInwardUpAlert && cacheMA_Crosses[idx].CrossOutwardDownAlert == crossOutwardDownAlert && cacheMA_Crosses[idx].CrossOutwardUpAlert == crossOutwardUpAlert && cacheMA_Crosses[idx].SoundFileStraddleOverMA1 == soundFileStraddleOverMA1 && cacheMA_Crosses[idx].SoundFileStraddleOverMA2 == soundFileStraddleOverMA2 && cacheMA_Crosses[idx].SoundFileStraddleUnderMA1 == soundFileStraddleUnderMA1 && cacheMA_Crosses[idx].SoundFileStraddleUnderMA2 == soundFileStraddleUnderMA2 && cacheMA_Crosses[idx].SoundFileBarOverMA1 == soundFileBarOverMA1 && cacheMA_Crosses[idx].SoundFileBarUnderMA1 == soundFileBarUnderMA1 && cacheMA_Crosses[idx].SoundFileBarOverMA2 == soundFileBarOverMA2 && cacheMA_Crosses[idx].SoundFileBarUnderMA2 == soundFileBarUnderMA2 && cacheMA_Crosses[idx].EmailAddress == emailAddress && cacheMA_Crosses[idx].MA1DataType == mA1DataType && cacheMA_Crosses[idx].MA1Type == mA1Type && cacheMA_Crosses[idx].MA2DataType == mA2DataType && cacheMA_Crosses[idx].MA2Type == mA2Type && cacheMA_Crosses[idx].EqualsInput(input))
						return cacheMA_Crosses[idx];
			return CacheIndicator<MA_Crosses>(new MA_Crosses(){ Period1 = period1, Period2 = period2, ValidZoneSize = validZoneSize, ATRperiod = aTRperiod, ATRmult = aTRmult, PrintZoneSizeToOutputWindow = printZoneSizeToOutputWindow, ShowInwardSignals = showInwardSignals, ShowOutwardSignals = showOutwardSignals, VMA1_VolatilityPeriod = vMA1_VolatilityPeriod, VMA2_VolatilityPeriod = vMA2_VolatilityPeriod, KAMAfast1 = kAMAfast1, KAMAslow1 = kAMAslow1, KAMAfast2 = kAMAfast2, KAMAslow2 = kAMAslow2, MsgLength = msgLength, MaxEmails = maxEmails, MaxAudioAlerts = maxAudioAlerts, SoundFileInwardUp = soundFileInwardUp, SoundFileInwardDown = soundFileInwardDown, SoundFileOutwardDown = soundFileOutwardDown, SoundFileOutwardUp = soundFileOutwardUp, CrossInwardDownAlert = crossInwardDownAlert, CrossInwardUpAlert = crossInwardUpAlert, CrossOutwardDownAlert = crossOutwardDownAlert, CrossOutwardUpAlert = crossOutwardUpAlert, SoundFileStraddleOverMA1 = soundFileStraddleOverMA1, SoundFileStraddleOverMA2 = soundFileStraddleOverMA2, SoundFileStraddleUnderMA1 = soundFileStraddleUnderMA1, SoundFileStraddleUnderMA2 = soundFileStraddleUnderMA2, SoundFileBarOverMA1 = soundFileBarOverMA1, SoundFileBarUnderMA1 = soundFileBarUnderMA1, SoundFileBarOverMA2 = soundFileBarOverMA2, SoundFileBarUnderMA2 = soundFileBarUnderMA2, EmailAddress = emailAddress, MA1DataType = mA1DataType, MA1Type = mA1Type, MA2DataType = mA2DataType, MA2Type = mA2Type }, input, ref cacheMA_Crosses);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MA_Crosses MA_Crosses(int period1, int period2, int validZoneSize, int aTRperiod, double aTRmult, bool printZoneSizeToOutputWindow, bool showInwardSignals, bool showOutwardSignals, int vMA1_VolatilityPeriod, int vMA2_VolatilityPeriod, int kAMAfast1, int kAMAslow1, int kAMAfast2, int kAMAslow2, MA_Crosses_MsgLengthType msgLength, int maxEmails, int maxAudioAlerts, string soundFileInwardUp, string soundFileInwardDown, string soundFileOutwardDown, string soundFileOutwardUp, bool crossInwardDownAlert, bool crossInwardUpAlert, bool crossOutwardDownAlert, bool crossOutwardUpAlert, string soundFileStraddleOverMA1, string soundFileStraddleOverMA2, string soundFileStraddleUnderMA1, string soundFileStraddleUnderMA2, string soundFileBarOverMA1, string soundFileBarUnderMA1, string soundFileBarOverMA2, string soundFileBarUnderMA2, string emailAddress, MA_Crosses_DataType mA1DataType, MA_Crosses_type mA1Type, MA_Crosses_DataType mA2DataType, MA_Crosses_type mA2Type)
		{
			return indicator.MA_Crosses(Input, period1, period2, validZoneSize, aTRperiod, aTRmult, printZoneSizeToOutputWindow, showInwardSignals, showOutwardSignals, vMA1_VolatilityPeriod, vMA2_VolatilityPeriod, kAMAfast1, kAMAslow1, kAMAfast2, kAMAslow2, msgLength, maxEmails, maxAudioAlerts, soundFileInwardUp, soundFileInwardDown, soundFileOutwardDown, soundFileOutwardUp, crossInwardDownAlert, crossInwardUpAlert, crossOutwardDownAlert, crossOutwardUpAlert, soundFileStraddleOverMA1, soundFileStraddleOverMA2, soundFileStraddleUnderMA1, soundFileStraddleUnderMA2, soundFileBarOverMA1, soundFileBarUnderMA1, soundFileBarOverMA2, soundFileBarUnderMA2, emailAddress, mA1DataType, mA1Type, mA2DataType, mA2Type);
		}

		public Indicators.MA_Crosses MA_Crosses(ISeries<double> input , int period1, int period2, int validZoneSize, int aTRperiod, double aTRmult, bool printZoneSizeToOutputWindow, bool showInwardSignals, bool showOutwardSignals, int vMA1_VolatilityPeriod, int vMA2_VolatilityPeriod, int kAMAfast1, int kAMAslow1, int kAMAfast2, int kAMAslow2, MA_Crosses_MsgLengthType msgLength, int maxEmails, int maxAudioAlerts, string soundFileInwardUp, string soundFileInwardDown, string soundFileOutwardDown, string soundFileOutwardUp, bool crossInwardDownAlert, bool crossInwardUpAlert, bool crossOutwardDownAlert, bool crossOutwardUpAlert, string soundFileStraddleOverMA1, string soundFileStraddleOverMA2, string soundFileStraddleUnderMA1, string soundFileStraddleUnderMA2, string soundFileBarOverMA1, string soundFileBarUnderMA1, string soundFileBarOverMA2, string soundFileBarUnderMA2, string emailAddress, MA_Crosses_DataType mA1DataType, MA_Crosses_type mA1Type, MA_Crosses_DataType mA2DataType, MA_Crosses_type mA2Type)
		{
			return indicator.MA_Crosses(input, period1, period2, validZoneSize, aTRperiod, aTRmult, printZoneSizeToOutputWindow, showInwardSignals, showOutwardSignals, vMA1_VolatilityPeriod, vMA2_VolatilityPeriod, kAMAfast1, kAMAslow1, kAMAfast2, kAMAslow2, msgLength, maxEmails, maxAudioAlerts, soundFileInwardUp, soundFileInwardDown, soundFileOutwardDown, soundFileOutwardUp, crossInwardDownAlert, crossInwardUpAlert, crossOutwardDownAlert, crossOutwardUpAlert, soundFileStraddleOverMA1, soundFileStraddleOverMA2, soundFileStraddleUnderMA1, soundFileStraddleUnderMA2, soundFileBarOverMA1, soundFileBarUnderMA1, soundFileBarOverMA2, soundFileBarUnderMA2, emailAddress, mA1DataType, mA1Type, mA2DataType, mA2Type);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MA_Crosses MA_Crosses(int period1, int period2, int validZoneSize, int aTRperiod, double aTRmult, bool printZoneSizeToOutputWindow, bool showInwardSignals, bool showOutwardSignals, int vMA1_VolatilityPeriod, int vMA2_VolatilityPeriod, int kAMAfast1, int kAMAslow1, int kAMAfast2, int kAMAslow2, MA_Crosses_MsgLengthType msgLength, int maxEmails, int maxAudioAlerts, string soundFileInwardUp, string soundFileInwardDown, string soundFileOutwardDown, string soundFileOutwardUp, bool crossInwardDownAlert, bool crossInwardUpAlert, bool crossOutwardDownAlert, bool crossOutwardUpAlert, string soundFileStraddleOverMA1, string soundFileStraddleOverMA2, string soundFileStraddleUnderMA1, string soundFileStraddleUnderMA2, string soundFileBarOverMA1, string soundFileBarUnderMA1, string soundFileBarOverMA2, string soundFileBarUnderMA2, string emailAddress, MA_Crosses_DataType mA1DataType, MA_Crosses_type mA1Type, MA_Crosses_DataType mA2DataType, MA_Crosses_type mA2Type)
		{
			return indicator.MA_Crosses(Input, period1, period2, validZoneSize, aTRperiod, aTRmult, printZoneSizeToOutputWindow, showInwardSignals, showOutwardSignals, vMA1_VolatilityPeriod, vMA2_VolatilityPeriod, kAMAfast1, kAMAslow1, kAMAfast2, kAMAslow2, msgLength, maxEmails, maxAudioAlerts, soundFileInwardUp, soundFileInwardDown, soundFileOutwardDown, soundFileOutwardUp, crossInwardDownAlert, crossInwardUpAlert, crossOutwardDownAlert, crossOutwardUpAlert, soundFileStraddleOverMA1, soundFileStraddleOverMA2, soundFileStraddleUnderMA1, soundFileStraddleUnderMA2, soundFileBarOverMA1, soundFileBarUnderMA1, soundFileBarOverMA2, soundFileBarUnderMA2, emailAddress, mA1DataType, mA1Type, mA2DataType, mA2Type);
		}

		public Indicators.MA_Crosses MA_Crosses(ISeries<double> input , int period1, int period2, int validZoneSize, int aTRperiod, double aTRmult, bool printZoneSizeToOutputWindow, bool showInwardSignals, bool showOutwardSignals, int vMA1_VolatilityPeriod, int vMA2_VolatilityPeriod, int kAMAfast1, int kAMAslow1, int kAMAfast2, int kAMAslow2, MA_Crosses_MsgLengthType msgLength, int maxEmails, int maxAudioAlerts, string soundFileInwardUp, string soundFileInwardDown, string soundFileOutwardDown, string soundFileOutwardUp, bool crossInwardDownAlert, bool crossInwardUpAlert, bool crossOutwardDownAlert, bool crossOutwardUpAlert, string soundFileStraddleOverMA1, string soundFileStraddleOverMA2, string soundFileStraddleUnderMA1, string soundFileStraddleUnderMA2, string soundFileBarOverMA1, string soundFileBarUnderMA1, string soundFileBarOverMA2, string soundFileBarUnderMA2, string emailAddress, MA_Crosses_DataType mA1DataType, MA_Crosses_type mA1Type, MA_Crosses_DataType mA2DataType, MA_Crosses_type mA2Type)
		{
			return indicator.MA_Crosses(input, period1, period2, validZoneSize, aTRperiod, aTRmult, printZoneSizeToOutputWindow, showInwardSignals, showOutwardSignals, vMA1_VolatilityPeriod, vMA2_VolatilityPeriod, kAMAfast1, kAMAslow1, kAMAfast2, kAMAslow2, msgLength, maxEmails, maxAudioAlerts, soundFileInwardUp, soundFileInwardDown, soundFileOutwardDown, soundFileOutwardUp, crossInwardDownAlert, crossInwardUpAlert, crossOutwardDownAlert, crossOutwardUpAlert, soundFileStraddleOverMA1, soundFileStraddleOverMA2, soundFileStraddleUnderMA1, soundFileStraddleUnderMA2, soundFileBarOverMA1, soundFileBarUnderMA1, soundFileBarOverMA2, soundFileBarUnderMA2, emailAddress, mA1DataType, mA1Type, mA2DataType, mA2Type);
		}
	}
}

#endregion
