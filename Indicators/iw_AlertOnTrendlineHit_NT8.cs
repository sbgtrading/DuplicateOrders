//#define SPEECHENABLED
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
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#if SPEECHENABLED
using SpeechLib;
using System.Reflection;
using System.Threading;
#endif
#endregion
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Gui;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
//using System.Speech.Synthesis;
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("Parameters", 10)]
    [CategoryOrder("Alert", 20)]
    [CategoryOrder("Visuals", 30)]

#if SPEECHENABLED
		public delegate void SpeechDelegate_AlertOnTrendlineHit(string str);
		[Description("Speak a phrase, play alert WAV and/or draw a chart marker when a manually drawn trendline is hit")]
#else
		[Description("Play alert WAV and/or draw a chart marker when a manually drawn trendline is hit")]
#endif
//		[Gui.Design.DisplayName("Flux Alert on trendline hit")]
//		public class FluxAlertOnTrendlineHit : Indicator
		public class AlertOnTrendlineHit : Indicator
		{

		private class TheLine {
			public string Tag="";
			public string Type = "Ray";
			public int StartBar=0;
			public int EndBar=1;
			public double StartPrice=0;
			public double EndPrice=0;
			public double CurrentUpperAlertPrice = 0;
			public double CurrentLinePrice = 0;
			public double CurrentLowerAlertPrice = 0;
			public Color ColorOfLine = Colors.Transparent;
			public TheLine(string tag, string drawType, int startbar, int endbar, double startprice, double endprice, System.Windows.Media.Color colorofline) {this.Tag=tag;this.Type=drawType;this.StartBar=startbar;this.EndBar=endbar;this.StartPrice=startprice;this.EndPrice=endprice; this.ColorOfLine=colorofline;}
		}
		#region Variables
			// Wizard generated variables
			private bool RunInit = true;
			private string CurrentSoundFileName="Alert2.wav";
			private int EmailBar = -1;
			private double priorprice = 0, price = 0;
			private string TagOfCrossedLine = null;
			private int CrossDirection = 0;
			private string NewMsg = null;
			private Dictionary<string,double> Lines = new Dictionary<string,double>();
			private string[] Msgs = new String[3]{string.Empty,string.Empty,string.Empty};
			//private Pen ThePen;
//			private StringFormat	stringFormat	= new StringFormat();
			private DateTime TimeOfText = DateTime.MaxValue;
			private double NearestAbove = double.MaxValue;
			private double NearestBelow = double.MaxValue;
			private string NearestAboveTag = null;
			private string NearestBelowTag = null;
			private float lineval1=0, lineval2=0, lineval2zupper=0, lineval2zlower=0;
			private string AlertMsg = "";
			private bool SpeechEnabled = false;

			private string FS = null;
		#endregion
		private int CrossingsThisBar = 0;
		private string NearestUpperWAV = string.Empty;
		private string NearestLowerWAV = string.Empty;
		private List<TheLine> TheCO = new List<TheLine>();
		private Series<double> input;
		private int PopupBar = -1;
		private string Id = @"N/A";
		private List<string> TagsOfLinesHit = new List<string>();

#if SPEECHENABLED
		static Assembly SpeechLib = null;
#endif
		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt");
				IsBen = IsBen && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "AIAlertOnTrendlineHit", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				Calculate=Calculate.OnPriceChange;
				IsChartOnly=true;
				IsOverlay=true;
				IsAutoScale=false;
				Name = "iw Alert on trendline hit v4.7";
			}

			else if (State == State.Configure) {
	#if SPEECHENABLED
				string dll = System.IO.Path.Combine(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"bin\\custom"),"interop.speechlib.dll");   //"c:\users\ben\Documents\NinjaTrader 7\"
				if(System.IO.File.Exists(dll)) {
					SpeechEnabled = true;
					SpeechLib = Assembly.LoadFile(dll);
				}
	#endif
				if(Instrument!=null && Bars!=null) Id = MakeString(new Object[]{Instrument.FullName.ToString()," (",Bars.BarsPeriod.ToString(),")"});
//				if(ChartControl == null) return;
	//			this.ChartControl.ChartPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(MouseUpEvent);	
//				ThePen    = new Pen(pLineBrush, 2);
//				textBrush = new SolidColorBrush(pLineBrush); textBrush.Freeze();
//				textBrushDistantLevel = new SolidColorBrush((byte)(Color.FromArgb), pLineBrush.R,pLineBrush.G,pLineBrush.B);
//				stringFormat.Alignment = TextAlignment.Near;
//				stringFormat.LineAlignment = TextAlignment.Near;

//				if(pDataSource == DataSource_AlertOnTrendlineHit.Price) {
//					int PriceDigits = 0;
//					FS = TickSize.ToString();
//					if(FS.Contains("E-")) {
//						FS = FS.Substring(FS.IndexOf("E-")+2);
//						PriceDigits = int.Parse(FS);
//					}
//					else PriceDigits = Math.Max(0,FS.Length-2);

//					if(PriceDigits==0) FS="0";
//					if(PriceDigits==1) FS="0.0";
//					if(PriceDigits==2) FS="0.00";
//					if(PriceDigits==3) FS="0.000";
//					if(PriceDigits==4) FS="0.0000";
//					if(PriceDigits==5) FS="0.00000";
//					if(PriceDigits==6) FS="0.000000";
//					if(PriceDigits==7) FS="0.0000000";
//					if(PriceDigits>=8) FS="0.00000000";
//				} else 
//					FS = "0";
				NearestUpperWAV = this.DefaultSoundFileName;
				NearestLowerWAV = this.DefaultSoundFileName;
			}
			else if(State==State.DataLoaded){
				input = new Series<double>(this);//inside State.DataLoaded
			}
		}
 		protected override void OnBarUpdate()
		{
			if(pDataSource == DataSource_AlertOnTrendlineHit.Price)			input[0] = (Close[0]); 
			else if(pDataSource == DataSource_AlertOnTrendlineHit.Volume)	input[0] = (Volume[0]);
			else input[0] = Input[0];

			if(IsFirstTickOfBar) {
				CrossingsThisBar = 0;
				TagsOfLinesHit.Clear();
			}
//			if(IsFirstTickOfBar && !Historical){
//				using (SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer())
//				{
//					synth.Volume = 70;
//					synth.Speak("First tick of bar number "+CurrentBar.ToString());
//				}
//			}
			if(Calculate == Calculate.OnBarClose) priorprice=input[1];
			else priorprice = price;
			price = input[0];
			if(price != priorprice) {
				AlertOnTrendlineHit_Engine(ref NearestUpperWAV, ref NearestLowerWAV);
			}

			if(TimeOfText != DateTime.MaxValue) {
				TimeSpan ts = new TimeSpan(Math.Abs(TimeOfText.Ticks-NinjaTrader.Core.Globals.Now.Ticks));
				if(ts.TotalSeconds>5) RemoveDrawObject("infomsg");
				else Draw.TextFixed(this, "infomsg",AlertMsg,TextPosition.Center);
			}
			
		}

//	public void MouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e) { 
//		#region MouseUpEvent
//			ChartControl.ChartPanel.Invalidate();
//		#endregion
//	}
//====================================================================================================
		#region SayItPlayIt methods
		private void PlayIt(string EncodedTag) {
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
				string wavname = elements[elements.Length-1].ToLower().Trim();
				if(wavname.Length>0){
					if(!wavname.Contains(".wav")) wavname = wavname + ".wav";
					if(pPrintToAlertsWindow)
						Alert(CurrentBar.ToString(),Priority.High,NewMsg,AddSoundFolder(wavname),1,Brushes.Blue,Brushes.White);  
					else
						PlaySound(AddSoundFolder(wavname));
				}
				TimeOfText = NinjaTrader.Core.Globals.Now;
				AlertMsg = MakeString(new Object[]{"WAV file ",wavname," played"});
			}
		}
#if SPEECHENABLED
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
				return;
			}
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
				SpeechDelegate_AlertOnTrendlineHit speechThread = new SpeechDelegate_AlertOnTrendlineHit(SayItThread);
				string SayThis = elements[elements.Length-1].ToUpper();
				if(SayThis.Contains("[SAYPRICE]")) {
					string pricestr1 = Instrument.MasterInstrument.FormatPrice(Price);
					string spokenprice = string.Empty + pricestr1[0];
					int i = 0;
					while(i<pricestr1.Length) {
						spokenprice = MakeString(new Object[]{spokenprice," ",pricestr1[i++]});
						spokenprice = spokenprice.Replace(".","point");
					}
					SayThis = SayThis.Replace("[SAYPRICE]", spokenprice);
				}
				speechThread.BeginInvoke(SayThis, null, null);
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = MakeString(new Object[]{"'",SayThis,"' was spoken"});
			}
		}
#else
		private void SayIt (string EncodedTag) {}
		private void SayIt (string EncodedTag, double Price) {}
#endif
		#endregion
//====================================================================================================
		private void AlertOnTrendlineHit_Engine(ref string NearestUpperWAV, ref string NearestLowerWAV){

try{
			double CrossedAt = double.MinValue;
			TheCO.Clear();
			double p1 = 0;
			double p2 = 0;
			DateTime t1 = DateTime.MinValue;
			DateTime t2 = DateTime.MinValue;
			int b1 = 0;
			int b2 = 0;
			foreach (dynamic CO in DrawObjects.ToList())
			{
				bool IsHLine = CO.ToString().Contains(".HorizontalLine");
				bool IsLine = CO.ToString().Contains(".ExtendedLine") || CO.ToString().Contains(".Line");
				bool IsRay = CO.ToString().Contains(".Ray");
				if(pTrendlineTag.Length>0 && (IsHLine || IsLine || IsRay)) {
					if(!CO.Tag.Contains(pTrendlineTag)) continue;
				}
				if(IsRay) {
					p1 = CO.StartAnchor.Price;
					t1 = CO.StartAnchor.Time;
					b1 = Bars.GetBar(t1);
					p2 = CO.EndAnchor.Price;
					t2 = CO.EndAnchor.Time;
					b2 = Bars.GetBar(t2);
					double slope = (p2 - p1)/Math.Abs(b1-b2);
					bool permit1 = slope>0  && (pFilterDirection == TrendDirection_AlertOnTrendlineHit.Up || pFilterDirection == TrendDirection_AlertOnTrendlineHit.UpAndHorizontal);
					bool permit2 = slope<0  && (pFilterDirection == TrendDirection_AlertOnTrendlineHit.Down || pFilterDirection == TrendDirection_AlertOnTrendlineHit.DownAndHorizontal);
					bool permit3 = slope==0 && (pFilterDirection == TrendDirection_AlertOnTrendlineHit.UpAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.DownAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.Horizontal);
					if(permit1 || permit2 || permit3 || pFilterDirection == TrendDirection_AlertOnTrendlineHit.All)
						TheCO.Add(new TheLine(CO.Tag, CO.Name, b1, b2, p1, p2, Colors.Red));
				}
				if(IsHLine) {
					p1 = CO.StartAnchor.Price;
					t1 = CO.StartAnchor.Time;
					b1 = Bars.GetBar(t1);
					bool permit1 = pFilterDirection == TrendDirection_AlertOnTrendlineHit.Horizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.UpAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.DownAndHorizontal;
					if(permit1 || pFilterDirection == TrendDirection_AlertOnTrendlineHit.All)
						TheCO.Add(new TheLine(CO.Tag, CO.Name, CurrentBar-1, CurrentBar-2, p1, p1, Colors.Red));
				}
				if(IsLine) {
					p1 = CO.StartAnchor.Price;
					t1 = CO.StartAnchor.Time;
					b1 = Bars.GetBar(t1);

					p2 = CO.EndAnchor.Price;
					t2 = CO.EndAnchor.Time;
					b2 = Bars.GetBar(t2);

					double slope = (p2 - p1)/Math.Abs(b1-b2);
					bool permit1 = slope>0 && (pFilterDirection == TrendDirection_AlertOnTrendlineHit.Up || pFilterDirection == TrendDirection_AlertOnTrendlineHit.UpAndHorizontal);
					bool permit2 = slope<0 && (pFilterDirection == TrendDirection_AlertOnTrendlineHit.Down || pFilterDirection == TrendDirection_AlertOnTrendlineHit.DownAndHorizontal);
					bool permit3 = slope==0 && (pFilterDirection == TrendDirection_AlertOnTrendlineHit.UpAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.DownAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.Horizontal);
					bool permit4 = true;
					if(CO.ToString().EndsWith(".Line")) permit4 = Math.Max(b1,b2)>=CurrentBar;
					if(permit4 && (permit1 || permit2 || permit3 || pFilterDirection == TrendDirection_AlertOnTrendlineHit.All)){
						TheCO.Add(new TheLine(CO.Tag, CO.ToString(), b1, b2, p1, p2, Colors.Red));
					}
				}
			}
			if(pFilterDirection == TrendDirection_AlertOnTrendlineHit.Horizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.UpAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.DownAndHorizontal || pFilterDirection == TrendDirection_AlertOnTrendlineHit.All) {
				if(this.pAlertLevel1>-9999) TheCO.Add(new TheLine("AlertLevel1", "Line", 0, 1, this.pAlertLevel1, this.pAlertLevel1, Colors.Transparent));
				if(this.pAlertLevel2>-9999) TheCO.Add(new TheLine("AlertLevel2", "Line", 0, 1, this.pAlertLevel2, this.pAlertLevel2, Colors.Transparent));
				if(this.pAlertLevel3>-9999) TheCO.Add(new TheLine("AlertLevel3", "Line", 0, 1, this.pAlertLevel3, this.pAlertLevel3, Colors.Transparent));
			}

			NearestAbove = double.MaxValue;
			NearestBelow = double.MaxValue;
			NearestAboveTag = null;
			NearestBelowTag = null;

			foreach(TheLine CO in TheCO) 
			{
try{
//Print(MakeString(new Object[]{CO.NinjaTrader.NinjaScript.DrawingTools.ToString()," found on ",CO.GetType(),", with a tag of: ",CO.Tag}));
				if (CO.Type.Contains("Ray"))            CO.CurrentLinePrice = CalculateDistance(input[0], CO.StartBar, CO.EndBar, CO.StartPrice, CO.EndPrice, ref NearestAbove, ref NearestBelow, "Ray ", CO.Tag, ref NearestAboveTag, ref NearestBelowTag);
				if (CO.Type.Contains("HorizontalLine")) CO.CurrentLinePrice = CalculateDistance(input[0], CO.StartBar, CO.StartBar-1, CO.StartPrice, CO.StartPrice, ref NearestAbove, ref NearestBelow, "HLine ", CO.Tag, ref NearestAboveTag, ref NearestBelowTag);
				if (CO.Type.Contains("ExtendedLine"))   CO.CurrentLinePrice = CalculateDistance(input[0], CO.StartBar, CO.EndBar, CO.StartPrice, CO.EndPrice, ref NearestAbove, ref NearestBelow, "Line ", CO.Tag, ref NearestAboveTag, ref NearestBelowTag);
				if (CO.Type.Contains("Line"))           CO.CurrentLinePrice = CalculateDistance(input[0], CO.StartBar, CO.EndBar, CO.StartPrice, CO.EndPrice, ref NearestAbove, ref NearestBelow, "Line ", CO.Tag, ref NearestAboveTag, ref NearestBelowTag);
				CO.CurrentUpperAlertPrice = CO.CurrentLinePrice + ZoneSizePts;
				CO.CurrentLowerAlertPrice = CO.CurrentLinePrice - ZoneSizePts;
}catch(Exception err){Print(Name+":  Engine2: "+err.ToString());}
			}
			TagOfCrossedLine = string.Empty;
			foreach (TheLine L in TheCO) {
				if(priorprice <= L.CurrentLowerAlertPrice && price > L.CurrentLowerAlertPrice && !TagsOfLinesHit.Contains(L.Tag)) {
					TagsOfLinesHit.Add(L.Tag);
					CrossDirection = 1;
					if(L.Tag.StartsWith("AlertLevel"))
						TagOfCrossedLine = L.Tag;
					else {
						if(L.StartPrice < L.EndPrice)
							TagOfCrossedLine = "Up Trendline";
						else if(L.StartPrice > L.EndPrice)
							TagOfCrossedLine = "Down Trendline";
						else
							TagOfCrossedLine = L.Type.ToString();//L.Tag;
					}
					CrossedAt = L.CurrentLowerAlertPrice;
	//Print(NinjaTrader.Core.Globals.Now.ToString()+" above Crossed UP, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
				}
				if(/*pDataSource == DataSource_AlertOnTrendlineHit.Price &&*/ priorprice>=L.CurrentUpperAlertPrice && price<L.CurrentUpperAlertPrice && !TagsOfLinesHit.Contains(L.Tag)) {
					CrossDirection = -1;
					if(L.Tag.StartsWith("AlertLevel"))
						TagOfCrossedLine = L.Tag;
					else {
						if(L.StartPrice < L.EndPrice)
							TagOfCrossedLine = "Up Trendline";
						else if(L.StartPrice > L.EndPrice)
							TagOfCrossedLine = "Down Trendline";
						else
							TagOfCrossedLine = L.Type.ToString();//L.Tag;
					}
					CrossedAt = L.CurrentUpperAlertPrice;
	//Print(NinjaTrader.Core.Globals.Now.ToString()+" above Crossed DOWN, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
				}
			}
			if(TheCO.Count>0 && TagOfCrossedLine.Length>0){//CrossingNow(price, priorprice, NearestAbove, NearestBelow, NearestAboveTag, NearestBelowTag, ref CrossedAt)) {
				NewMsg = MakeString(new Object[]{Id," has hit '",TagOfCrossedLine,"' at ",Instrument.MasterInstrument.FormatPrice(CrossedAt)});
				CrossingsThisBar++;
				if((int)pSignalType >=9 && (int)pSignalType <=13 && CrossingsThisBar <= pMaxAlertsPerBar && State != State.Historical) {
					PopupBar = CurrentBar;
					Log(string.Concat(NinjaTrader.Core.Globals.Now.ToString(),"  ",NewMsg),LogLevel.Alert);
				}
				else if(CrossingsThisBar <= pMaxAlertsPerBar) { 
					if((int)pSignalType>=4) {
						if(TagOfCrossedLine.ToLower().Contains("say")) SayIt(TagOfCrossedLine, price);
						else if(TagOfCrossedLine.ToLower().Contains("play")) PlayIt(TagOfCrossedLine);
						else if(pSoundFileName.Length>0){
							if(pPrintToAlertsWindow)
								Alert(NinjaTrader.Core.Globals.Now.ToString(),NinjaTrader.NinjaScript.Priority.High,Bars.BarsPeriod.ToString()+": AlertOnTrendline hit at "+Instrument.MasterInstrument.FormatPrice(CrossedAt),AddSoundFolder(pSoundFileName),1,Brushes.Blue,Brushes.White);  
							else
								PlaySound(AddSoundFolder(pSoundFileName));
						}
					}
				}
				DrawSymbol((int)pSignalType);

				if(EmailBar+pEmailFrequency < CurrentBar && pSendEmails && NewMsg!=null) {
					SendMail(pEmailAddress, MakeString(new Object[]{"AlertOnTrendlineHit on ",Id," at ",Instrument.MasterInstrument.FormatPrice(CrossedAt)}),NewMsg);
					EmailBar = CurrentBar;
					TimeOfText = NinjaTrader.Core.Globals.Now;
					AlertMsg = MakeString(new Object[]{"Email message sent to ",pEmailAddress});
				}
			}
			if(NewMsg != null) {
				if(pTextMsgPosition != TextPosition_AlertOnTrendlineHit_local.None) {
					Print(NewMsg);

					Msgs[2] = Msgs[1];
					Msgs[1] = Msgs[0];
					Msgs[0] = NewMsg+Environment.NewLine;
					NewMsg = null;

					if(pTextMsgPosition == TextPosition_AlertOnTrendlineHit_local.TopRight)
						Draw.TextFixed(this, "MsgList",MakeString(new Object[]{Msgs[0],Msgs[1],Msgs[2]}), TextPosition.TopRight);
					else if(pTextMsgPosition == TextPosition_AlertOnTrendlineHit_local.TopLeft)
						Draw.TextFixed(this, "MsgList",MakeString(new Object[]{Environment.NewLine,Msgs[0],Msgs[1],Msgs[2]}), TextPosition.TopLeft);
					else if(pTextMsgPosition == TextPosition_AlertOnTrendlineHit_local.BottomRight)
						Draw.TextFixed(this, "MsgList",MakeString(new Object[]{Msgs[0],Msgs[1],Msgs[2]}), TextPosition.BottomRight);
					else if(pTextMsgPosition == TextPosition_AlertOnTrendlineHit_local.BottomLeft)
						Draw.TextFixed(this, "MsgList",MakeString(new Object[]{Msgs[0],Msgs[1],Msgs[2],Environment.NewLine,"\t"}), TextPosition.BottomLeft);
					else if(pTextMsgPosition == TextPosition_AlertOnTrendlineHit_local.Center)
						Draw.TextFixed(this, "MsgList",MakeString(new Object[]{Msgs[0],Msgs[1],Msgs[2]}), TextPosition.Center);
				}
			}


			//for(int k = 0; k<Lines.Count; k++) Draw.Dot(this, k+"dot",false,0,Lines[k],Color.Yellow);
			string[] elements;
			try{
				elements = NearestAboveTag.ToLower().Split(new char[]{':'}, StringSplitOptions.None);
				if(elements.Length>1 && NearestAboveTag.Contains("playit")) NearestUpperWAV = elements[elements.Length-1];
				else NearestUpperWAV = this.pSoundFileName;
				if(NearestUpperWAV.StartsWith(".wav")) NearestUpperWAV = string.Empty;
			}catch{}
//Print("NearestBelowTag: "+NearestBelowTag);
			try{
				elements = NearestBelowTag.ToLower().Split(new char[]{':'}, StringSplitOptions.None);
				if(elements.Length>1 && NearestBelowTag.Contains("playit")) NearestLowerWAV = elements[elements.Length-1];
				else NearestLowerWAV = this.pSoundFileName;
				if(NearestLowerWAV.StartsWith(".wav")) NearestLowerWAV = string.Empty;
			}catch{}

}catch(Exception err1){Print(Name+":  Engine1: "+err1.ToString());}
		}

//====================================================================================================
		private double CalculateDistance(double close, int startabs, int endabs, double startprice, double endprice, ref double NearestAbove, ref double NearestBelow, string LineType, string Tag, ref string NearestAboveTag, ref string NearestBelowTag){
			if(startprice<=0) return 0;
			if(endabs<startabs) {
				int temp = endabs;
				endabs = startabs;
				startabs = temp;
				double tempp = endprice;
				endprice = startprice;
				startprice = tempp;
			}
			double slope = (endprice - startprice) / (endabs-startabs);
//			double linepricenow = (CurrentBar - endabs) * slope + endprice;
			double linepricenow = (CurrentBar - startabs) * slope + startprice;
			Lines[Tag] = linepricenow;
//Print("Lines["+Tag+"] = "+linepricenow.ToString());
			if(linepricenow > close) {
				double distance = linepricenow - close;
				if(distance < NearestAbove) {
					NearestAbove = distance;
					NearestAboveTag = Tag;
//Print("   NearestAbove "+Tag+"  "+distance);
				}
			}
			if(linepricenow < close) {
				double distance = close - linepricenow;
				if(distance < NearestBelow) {
					NearestBelow = distance;
					NearestBelowTag = Tag;
//Print("   NearestBelow "+Tag+"  "+distance);
				}
//Draw.TextFixed(this, "nearestbelow","Nearest line ("+LineType+":"+Tag+") below price: "+linepricenow.ToString()+"  distance: "+NearestBelow.ToString(),TextPosition.TopLeft);
			}
			return linepricenow;
		}
//====================================================================================================
		private bool CrossingNow (double currentprice, double priorprice, double NearestAbove, double NearestBelow, string NearestAboveTag, string NearestBelowTag, ref double CrossedAt) {
			TagOfCrossedLine = string.Empty;
			
			//Dictionary<string,double> TempLines = Lines;
			foreach (KeyValuePair<string,double> kv in /*Temp*/Lines) {
				double lineprice = kv.Value-ZoneSizePts;
				if(priorprice>lineprice) lineprice = kv.Value+ZoneSizePts;

				if(priorprice<=lineprice && currentprice>lineprice) {
					CrossDirection = 1;
					TagOfCrossedLine = kv.Key;
					CrossedAt = lineprice;
	//Print(NinjaTrader.Core.Globals.Now.ToString()+" above Crossed UP, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
					return true;
				}
				if(pDataSource == DataSource_AlertOnTrendlineHit.Price && priorprice>=lineprice && currentprice<lineprice) {
					CrossDirection = -1;
					TagOfCrossedLine = kv.Key;
					CrossedAt = lineprice;
	//Print(NinjaTrader.Core.Globals.Now.ToString()+" above Crossed DOWN, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
					return true;
				}

//				if(pDataSource == DataSource_AlertOnTrendlineHit.Price && priorprice>=lineprice && currentprice<lineprice) {
//					CrossDirection = -1;
//					TagOfCrossedLine = kv.Key;
//					CrossedAt = lineprice;
//	//Print(NinjaTrader.Core.Globals.Now.ToString()+" below Crossed DOWN, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
//					return true;
//				}
//				if(priorprice<=lineprice && currentprice>lineprice) {
//					CrossDirection = 1;
//					TagOfCrossedLine = kv.Key;
//					CrossedAt = lineprice;
//	//Print(NinjaTrader.Core.Globals.Now.ToString()+" below Crossed UP, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
//					return true;
//				}
			}
			return false;
		}
		#region commented out
		/*
		private bool CrossingNow (double currentprice, double priorprice, double NearestAbove, double NearestBelow, string NearestAboveTag, string NearestBelowTag, ref double CrossedAt) {
			TagOfCrossedLine = null;
			double lineprice = input[0]+NearestAbove;

			if(priorprice<=lineprice && currentprice>lineprice) {
				CrossDirection = 1;
				TagOfCrossedLine = NearestAboveTag;
				CrossedAt = lineprice;
Print(NinjaTrader.Core.Globals.Now.ToString()+" above Crossed UP, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
				return true;
			}
			if(priorprice>=lineprice && currentprice<lineprice) {
				CrossDirection = -1;
				TagOfCrossedLine = NearestAboveTag;
				CrossedAt = lineprice;
Print(NinjaTrader.Core.Globals.Now.ToString()+" above Crossed DOWN, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
				return true;
			}

			lineprice = input[0]-NearestBelow;
			if(priorprice>=lineprice && currentprice<lineprice) {
				CrossDirection = -1;
				TagOfCrossedLine = NearestBelowTag;
				CrossedAt = lineprice;
Print(NinjaTrader.Core.Globals.Now.ToString()+" below Crossed DOWN, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
				return true;
			}
			if(priorprice<=lineprice && currentprice>lineprice) {
				CrossDirection = 1;
				TagOfCrossedLine = NearestBelowTag;
				CrossedAt = lineprice;
Print(NinjaTrader.Core.Globals.Now.ToString()+" below Crossed UP, prior tick: "+priorprice+" line="+lineprice+"  price="+currentprice);
				return true;
			}
			return false;
		}*/
		#endregion
//====================================================================================================
		private void DrawSymbol(int AlertType) {
			string id = string.Format("AOTH {0} {1}", CrossDirection>0?"Up":"Down", CurrentBar);
			if(CrossDirection == 1) { //Upward crossing, put symbol on Low price
				if(AlertType == 0 || AlertType == 5 || AlertType == 10)      Draw.Diamond(this, id, false, 0, Low[0]-TickSize, pUpBrush);
				else if(AlertType == 1 || AlertType == 6 || AlertType == 11) Draw.TriangleUp(this, id, false, 0, Low[0]-TickSize, pUpBrush);
				else if(AlertType == 2 || AlertType == 7 || AlertType == 12) Draw.ArrowUp(this, id, false, 0, Low[0]-TickSize, pUpBrush);
				else if(AlertType == 3 || AlertType == 8 || AlertType == 13) Draw.Dot(this, id, false, 0, Low[0]-TickSize, pUpBrush);
			}
			else if(CrossDirection == -1) { //Downward crossing, put symbol on Low price
				if(AlertType == 0 || AlertType == 5 || AlertType == 10)      Draw.Diamond(this, id, false, 0, High[0]+TickSize, pDownBrush);
				else if(AlertType == 1 || AlertType == 6 || AlertType == 11) Draw.TriangleDown(this, id, false, 0, High[0]+TickSize, pDownBrush);
				else if(AlertType == 2 || AlertType == 7 || AlertType == 12) Draw.ArrowDown(this, id, false, 0, High[0]+TickSize, pDownBrush);
				else if(AlertType == 3 || AlertType == 8 || AlertType == 13) Draw.Dot(this, id, false, 0, High[0]+TickSize, pDownBrush);
			}
		}
//====================================================================================================
	private static string MakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		return stb.ToString();
	}
//====================================================================================================
		public override string ToString()
		{
			string tags = string.Empty;
			if(this.pTrendlineTag.Length>0) tags = " on '"+this.pTrendlineTag+"' tagged lines";
			if(pDataSource == DataSource_AlertOnTrendlineHit.Volume)
				return "AlertOnTrendlineHit - Volume alerts"+tags;
			else if(pDataSource == DataSource_AlertOnTrendlineHit.Price)
				return "AlertOnTrendlineHit - Price alerts"+tags;
			else return "AlertOnTrendlineHit - Indicator alerts"+tags;
		}
//====================================================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
		}
//====================================================================================================

		#region Properties
		internal class LoadFileList : StringConverter
		{
			#region LoadFileList
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

		private TrendDirection_AlertOnTrendlineHit pFilterDirection = TrendDirection_AlertOnTrendlineHit.All;
		[Description("Engage only Upward or Downward sloping trendlines?  'Horizontal' for flat, non-sloped trendlines, and 'All' means you engage upward and downward and horizontal trendlines")]
		[Category("Parameters")]
		public TrendDirection_AlertOnTrendlineHit FilterDirection
		{
			get { return pFilterDirection; }
			set { pFilterDirection = value; }
		}

		private DataSource_AlertOnTrendlineHit pDataSource = DataSource_AlertOnTrendlineHit.Price;
		[Description("")]
		[Category("Parameters")]
		public DataSource_AlertOnTrendlineHit DataSource
		{
			get { return pDataSource; }
			set { pDataSource = value; }
		}

		private string pTrendlineTag = string.Empty;
		[Description("Enter a single letter of the alphabet...and the indicator will pay attention ONLY to trendlines that contain the this letter in their 'Tag' field")]
		[Category("Parameters")]
		public string TrendlineTag
		{
			get { return pTrendlineTag; }
			set { 	string result = value;
					result = result.Trim();
					char[] ch = result.ToCharArray();
					result = string.Empty;
					foreach(char c in ch) {
						if((c>='a' && c<='z') || (c>='A' && c<='Z')){
							result = string.Concat(result,c);
						}
					}
					pTrendlineTag = result;
				}
		}

		private double pAlertLevel1 = 0;
		[Description("Optional hardcoded alert level, enter '-9999' to disengage this alert")]
		[Category("Parameters")]
		public double AlertLevel1
		{
			get { return pAlertLevel1; }
			set { pAlertLevel1 = value; }
		}

		private double pAlertLevel2 = 0;
		[Description("Optional hardcoded alert level, enter '-9999' to disengage this alert")]
		[Category("Parameters")]
		public double AlertLevel2
		{
			get { return pAlertLevel2; }
			set { pAlertLevel2 = value; }
		}

		private double pAlertLevel3 = 0;
		[Description("Optional hardcoded alert level, enter '-9999' to disengage this alert")]
		[Category("Parameters")]
		public double AlertLevel3
		{
			get { return pAlertLevel3; }
			set { pAlertLevel3 = value; }
		}

		#region Alerts		

		private double pZoneSizePts = 0;
		[Description("Zone size, number of points above and below the levels that will enable the trigger of the alert")]
		[Category("Alert")]
		public double ZoneSizePts
		{
			get { return pZoneSizePts; }
			set { pZoneSizePts = Math.Max(0,value); }
		}

		private bool pPrintToAlertsWindow = true;
		[Description("Print alert message to Alerts window?")]
		[Category("Alert")]
		public bool PrintToAlertsWindow
		{
			get { return pPrintToAlertsWindow; }
			set { pPrintToAlertsWindow = value; }
		}

		private int pMaxAlertsPerBar = 1;
		[Description("Maximum times an alert can be played on the same bar, if CalculateOnBarClose=true then MaxAlertsPerBar will be fixed at 1")]
		[Category("Alert")]
		public int MaxAlertsPerBar
		{
			get { return pMaxAlertsPerBar; }
			set { pMaxAlertsPerBar = Math.Max(0,value); }
		}

		private string pSoundFileName = "none";
		[Description("Default WAV file to be played when line gets hit")]
		[Category("Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadFileList))]
		public string DefaultSoundFileName
		{
			get { return pSoundFileName; }
			set { pSoundFileName = value; 
//				string wavname = pSoundFileName.ToLower();
//				if(!wavname.Contains(".wav")) pSoundFileName = pSoundFileName.Trim() + ".wav";
			}
		}

		private int pEmailFrequency = 0;
		[Description("What's the minimum number of bars between consecutive email sends.  0 = once on each bar, 1 = skip a bar, 2 = skip 2 bars, etc")]
		[Category("Alert")]
		public int EmailFrequency
		{
			get { return pEmailFrequency; }
			set { pEmailFrequency = value; }
		}

		private AlertOnTrendlineHit_AlertType pSignalType = AlertOnTrendlineHit_AlertType.SoundOnly;
		[Description("Signal type")]
		[Category("Alert")]
		public AlertOnTrendlineHit_AlertType SignalType
		{
			get { return pSignalType; }
			set { pSignalType = value; }
		}
		private bool pSendEmails = false;
		[Description("Send an email on each arrow signal?")]
		[Category("Alert")]
		public bool EmailEnabled
		{
			get { return pSendEmails; }
			set { pSendEmails = value; }
		}
		private string pEmailAddress = "";
		[Description("Enter a valid destination email address to receive an email on signals")]
		[Category("Alert")]
		public string EmailAddress
		{
			get { return pEmailAddress; }
			set { pEmailAddress = value; }
		}
		#endregion

		private Brush pLineBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of alert line for levels near to current price")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line color",  GroupName = "Visuals")]
		public Brush LineC{	get { return pLineBrush; }	set { pLineBrush = value; }		}
		[Browsable(false)]
		public string LineClSerialize
		{	get { return Serialize.BrushToString(pLineBrush); } set { pLineBrush = Serialize.StringToBrush(value); }
		}
		
		private Brush pUpBrush = Brushes.Green;
		[XmlIgnore()]
		[Description("Color of the selected chart marker when price crosses upward thru an alert level")]
// 		[Category("Visuals")]
// [Gui.Design.DisplayNameAttribute("Marker Up color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Up color",  GroupName = "Visuals")]
		public Brush UC{	get { return pUpBrush; }	set { pUpBrush = value; }		}
		[Browsable(false)]
		public string UClSerialize
		{	get { return Serialize.BrushToString(pUpBrush); } set { pUpBrush = Serialize.StringToBrush(value); }
		}
		
		private Brush pDownBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of the selected chart marker when price crosses downward thru an alert level")]
// 		[Category("Visuals")]
// [Gui.Design.DisplayNameAttribute("Marker Down color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Marker Down color",  GroupName = "Visuals")]
		public Brush DC{	get { return pDownBrush; }	set { pDownBrush = value; }		}
		[Browsable(false)]
		public string DClSerialize
		{	get { return Serialize.BrushToString(pDownBrush); } set { pDownBrush = Serialize.StringToBrush(value); }
		}

		private string pLineLabel = "[price]*[wav]";
		[Description("What label to put on each level?  [price] is an optional placeholder for the current level price, [wav] is an optional placeholder for the wav file name, '*' (asterisk) whenever you want a new-line inserted")]
		[Category("Visuals")]
		public string LineLabel
		{
			get { return pLineLabel; }
			set { pLineLabel = value;}
		}

		private bool pShowAlertLevels = true;
		[Description("Show price levels of only the nearest rays and extended lines?")]
		[Category("Visuals")]
		public bool ShowAlertLevels
		{
			get { return pShowAlertLevels; }
			set { pShowAlertLevels = value; }
		}

		private bool pShowAll = true;
		[Description("Show all current price alert levels, even the ones off the chart?")]
		[Category("Visuals")]
		public bool ShowAll
		{
			get { return pShowAll; }
			set { pShowAll = value; }
		}

		private TextPosition_AlertOnTrendlineHit_local pTextMsgPosition = TextPosition_AlertOnTrendlineHit_local.None;
		[Description("Location of 'line hit' notifications")]
		[Category("Visuals")]
		public TextPosition_AlertOnTrendlineHit_local TextMsgPosition
		{
			get { return pTextMsgPosition; }
			set { pTextMsgPosition = value; }
		}

		private int pLineLengthPixels = 70;
		[Description("Length of lines in pixels")]
		[Category("Visuals")]
		public int LineLengthPixels
		{
			get { return pLineLengthPixels; }
			set { pLineLengthPixels = Math.Max(1,value); }
		}

		#endregion

//====================================================================================================
		private string GetDescription(int AlertType, string SoundFileName) {
			if(AlertType == 4) return SoundFileName;
			else if(AlertType == 0) return "Diamond";
			else if(AlertType == 5) return MakeString(new Object[]{SoundFileName, " & Diamond"});
			else if(AlertType == 1) return "Triangle";
			else if(AlertType == 6) return MakeString(new Object[]{SoundFileName, " & Triangle"});
			else if(AlertType == 2) return "Arrow";
			else if(AlertType == 7) return MakeString(new Object[]{SoundFileName, " & Arrow"});
			else if(AlertType == 3) return "Dot";
			else if(AlertType == 8) return MakeString(new Object[]{SoundFileName, " & Dot"});
			else if(AlertType == 9) return "Popup";
			else if(AlertType == 10) return "Popup & Diamond";
			else if(AlertType == 11) return "Popup & Triangle";
			else if(AlertType == 12) return "Popup & Arrow";
			else if(AlertType == 13) return "Popup & Dot";
			return "Error - unknown type";
		}
//====================================================================================================
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;
		double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
		//base.OnRender(chartControl, chartScale);
		//Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
		//Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
		//int firstBarPainted = ChartBars.FromIndex;
		//int lastBarPainted = ChartBars.ToIndex;

		int line = 978;
		TextFormat textFormat		= new TextFormat(Core.Globals.DirectWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Normal,
											SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, (float)ChartControl.Properties.LabelFont.Size) 
											{ TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, WordWrapping = WordWrapping.NoWrap };

		TextLayout textLayout		= null;
		var NearLineBrush = pLineBrush.ToDxBrush(RenderTarget);
		var tempbrush = pLineBrush.Clone();
		tempbrush.Opacity = 100.0; tempbrush.Freeze();
		var DistantLineBrush = tempbrush.ToDxBrush(RenderTarget);
		var LineBrush = NearLineBrush;

try{
		if(ChartControl==null) return;
//if(CalculateOnBarClose)priorprice=input[1];
//			else priorprice = price;
//			price = input[0];
//			if(price!=priorprice) {
//				AlertOnTrendlineHit_Engine(ref NearestUpperWAV, ref NearestLowerWAV);
//			}
		if(pDataSource == DataSource_AlertOnTrendlineHit.Volume) return;
		if(!pShowAlertLevels) return;
//			if(pShowAll) {
//				Print("levels: ");
//				foreach(var CO in TheCO){
//					Print("Tag: "+CO.Tag+"   "+CO.CurrentPrice);
//				}
//			}
//Print("Upper: "+NearestUpperWAV+"  Lower: "+NearestLowerWAV);
		int LeftmostPixelOfLines = ChartPanel.W-pLineLengthPixels;//chartControl.GetXByBarIndex(BarSelected);//-ChartControl.BarSpace*5;
		float endpoint = ChartPanel.X+ChartPanel.W;
		bool skip = false;


		if(!pShowAll) {//show only the alert levels nearest to the current price
			if(NearestAbove != double.MaxValue) {
	//Print("Nearest above: "+NearestAbove);
				double p = input[0]+NearestAbove;
				if(p > maxPrice) {
					p = maxPrice;
					LineBrush = DistantLineBrush;
				}
				if(!skip) {
					lineval1 = chartScale.GetYByValue(p);//(float)(GetYPos(p, bounds, minPrice, maxPrice));
					RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval1), new SharpDX.Vector2(endpoint, lineval1), LineBrush);
					lineval2zupper = chartScale.GetYByValue(p+ZoneSizePts);//(float)GetYPos(p+ZoneSizePts, bounds, minPrice, maxPrice));
					lineval2zlower = chartScale.GetYByValue(p-ZoneSizePts);//(float)GetYPos(p-ZoneSizePts, bounds, minPrice, maxPrice));
					RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zupper), new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zlower),LineBrush);
					if(pLineLabel.Length>0) {
						string desc = pLineLabel.Replace("[price]",(input[0]+NearestAbove).ToString()).Replace("[wav]",(int)pSignalType>=4?NearestUpperWAV:string.Empty).Replace("*",Environment.NewLine);
	//						string desc = MakeString(new Object[]{"Line at ",(input[0]+NearestAbove).ToString(FS)});
						//SizeF size = RenderTarget.MeasureString(desc, ChartControl.Properties.LabelFont);
						textLayout = new TextLayout(Core.Globals.DirectWriteFactory, desc, textFormat, ChartPanel.W, (float)ChartControl.Properties.LabelFont.Size);
						float x = endpoint - textLayout.Metrics.Width - 5;
						float y = lineval1 - textLayout.Metrics.Height - 2;
						if(price != maxPrice) y = lineval1;
			//Print("Drawing text '"+desc+"' at xy: "+x+" / "+y);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y), textLayout, LineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
//						RenderTarget.DrawString(desc, ChartControl.Properties.LabelFont, Brush, x, lineval1, stringFormat);
					}
				}
			}

			LineBrush = NearLineBrush;
			skip = false;
			if(NearestBelow != double.MaxValue) {
//Print("Nearest below: "+NearestBelow);
				double p = input[0]-NearestBelow;
				if(p < minPrice) {
					p = minPrice;
					LineBrush = DistantLineBrush;
				}
				if(!skip) {
					lineval2 = chartScale.GetYByValue(p);//(float)GetYPos(p, bounds, minPrice, maxPrice));
					RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval2), new SharpDX.Vector2(endpoint, lineval2), LineBrush);
					lineval2zupper = chartScale.GetYByValue(p+ZoneSizePts);//(float)GetYPos(p+ZoneSizePts, bounds, minPrice, maxPrice));
					lineval2zlower = chartScale.GetYByValue(p-ZoneSizePts);//(float)GetYPos(p-ZoneSizePts, bounds, minPrice, maxPrice));
					RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zupper), new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zlower),LineBrush);
					if(pLineLabel.Length>0) {
						string desc = pLineLabel.Replace("[price]",(input[0]-NearestBelow).ToString()).Replace("[wav]",(int)pSignalType>=4?NearestLowerWAV:string.Empty).Replace("*",Environment.NewLine);
//						string desc = MakeString(new Object[]{"Line at ",(input[0]+NearestAbove).ToString(FS)});
						textLayout = new TextLayout(Core.Globals.DirectWriteFactory, desc, textFormat, ChartPanel.W, (float)ChartControl.Properties.LabelFont.Size);
						float x = endpoint - textLayout.Metrics.Width - 5;
						float y = lineval2 - textLayout.Metrics.Height - 2;
						if(price != minPrice) y = lineval2;
		//Print("Drawing text '"+desc+"' at xy: "+x+" / "+y);
						RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y), textLayout, LineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
//							RenderTarget.DrawString(desc, ChartControl.Properties.LabelFont, Brush, x, lineval2, stringFormat);
					}
				}
			}
		} else if(pShowAll) {
			foreach(TheLine L in TheCO) {
				lineval2 = chartScale.GetYByValue(L.CurrentLinePrice);//(float)GetYPos(L.CurrentLinePrice, bounds, minPrice, maxPrice));
				RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval2), new SharpDX.Vector2(endpoint, lineval2), LineBrush);
				lineval2zupper = chartScale.GetYByValue(L.CurrentUpperAlertPrice);//(float)GetYPos(L.CurrentUpperAlertPrice, bounds, minPrice, maxPrice));
				lineval2zlower = chartScale.GetYByValue(L.CurrentLowerAlertPrice);//(float)GetYPos(L.CurrentLowerAlertPrice, bounds, minPrice, maxPrice));
				RenderTarget.DrawLine(new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zupper), new SharpDX.Vector2(LeftmostPixelOfLines, lineval2zlower), LineBrush);
				if(pLineLabel.Length>0) {
					string desc = pLineLabel.ToLower().Replace("[price]",(L.CurrentLinePrice).ToString());
					if(pSoundFileName!=".wav") desc = desc.Replace("[wav]",(int)pSignalType>=4?pSoundFileName:string.Empty);
					else desc = desc.Replace("[wav]",string.Empty);
					if(desc.Contains("*")) desc = desc.Replace("*",Environment.NewLine);
//						string desc = MakeString(new Object[]{"Line at ",(input[0]+NearestAbove).ToString(FS)});
					textLayout = new TextLayout(Core.Globals.DirectWriteFactory, desc, textFormat, ChartPanel.W, (float)ChartControl.Properties.LabelFont.Size);
					float x = endpoint - textLayout.Metrics.Width - 5;
					float y = lineval2 - textLayout.Metrics.Height - 2;
					if(price != minPrice) y = lineval2;
	//Print("Drawing text '"+desc+"' at xy: "+x+" / "+y);
					RenderTarget.DrawTextLayout(new SharpDX.Vector2(x,y), textLayout, LineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
//						RenderTarget.DrawString(desc, ChartControl.Properties.LabelFont, Brush, x, lineval2, stringFormat);
				}
			}
		}

}catch(Exception err){Print(line+" "+Name+" PlotError: "+err.ToString());}
		if(textLayout!=null){
			textLayout.Dispose();
			textLayout = null;
		}
		if(textFormat!=null){
			textFormat.Dispose();
			textFormat = null;
		}
		if(NearLineBrush!=null){
			NearLineBrush.Dispose();
			NearLineBrush = null;
		}
		if(DistantLineBrush!=null){
			DistantLineBrush.Dispose();
			DistantLineBrush = null;
		}
		if(LineBrush!=null){
			LineBrush.Dispose();
			LineBrush = null;
		}
	}

		//========================================================================================================
    //    protected override void OnTermination()
  //      {
//			if (this.ChartControl != null) {
//				this.ChartControl.ChartPanel.MouseUp -= MouseUpEvent;
	//		}
      //  }
    }
}


	public enum AlertOnTrendlineHit_AlertType {
		Diamond=0,
		Triangle=1,
		Arrow=2,
		Dot=3,
		SoundOnly=4,
		SoundAndDiamond=5,
		SoundAndTriangle=6,
		SoundAndArrow=7,
		SoundAndDot=8,
		PopupOnly=9,
		PopupAndDiamond=10,
		PopupAndTriangle =11,
		PopupAndArrow =12,
		PopupAndDot =13
	}
	public enum DataSource_AlertOnTrendlineHit{
		Volume,Price,Input
	}
	public enum TrendDirection_AlertOnTrendlineHit{
		Up, Down, Horizontal, UpAndHorizontal, DownAndHorizontal, All
	}
	public enum TextPosition_AlertOnTrendlineHit_local {
		TopRight,
		BottomRight,
		TopLeft,
		BottomLeft,
		Center,
		None
	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlertOnTrendlineHit[] cacheAlertOnTrendlineHit;
		public AlertOnTrendlineHit AlertOnTrendlineHit()
		{
			return AlertOnTrendlineHit(Input);
		}

		public AlertOnTrendlineHit AlertOnTrendlineHit(ISeries<double> input)
		{
			if (cacheAlertOnTrendlineHit != null)
				for (int idx = 0; idx < cacheAlertOnTrendlineHit.Length; idx++)
					if (cacheAlertOnTrendlineHit[idx] != null &&  cacheAlertOnTrendlineHit[idx].EqualsInput(input))
						return cacheAlertOnTrendlineHit[idx];
			return CacheIndicator<AlertOnTrendlineHit>(new AlertOnTrendlineHit(), input, ref cacheAlertOnTrendlineHit);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlertOnTrendlineHit AlertOnTrendlineHit()
		{
			return indicator.AlertOnTrendlineHit(Input);
		}

		public Indicators.AlertOnTrendlineHit AlertOnTrendlineHit(ISeries<double> input )
		{
			return indicator.AlertOnTrendlineHit(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlertOnTrendlineHit AlertOnTrendlineHit()
		{
			return indicator.AlertOnTrendlineHit(Input);
		}

		public Indicators.AlertOnTrendlineHit AlertOnTrendlineHit(ISeries<double> input )
		{
			return indicator.AlertOnTrendlineHit(input);
		}
	}
}

#endregion
