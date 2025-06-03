//#define SPEECH_ENABLED

#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#if SPEECH_ENABLED
using SpeechLib;
using System.Reflection;
using System.Threading;
#endif

#endregion

using System.ComponentModel.DataAnnotations;
//using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Input;
using NinjaTrader.Gui;
//using NinjaTrader.Gui.Tools;
//using NinjaTrader.NinjaScript;
//using NinjaTrader.Core.FloatingPoint;
//using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public delegate void SpeechDelegate_AutoTrendChannel(string str);
	/// <summary>
	/// Automatically draws a line representing the current trend and generates an alert if the trend line is broken.
	/// </summary>
	[Description("Calculates the most recent trendlines and channels and puts a dot on trendline price")]
	public class AutoTrendChannel : Indicator
	{

//====================================================================================================
		#region SayItPlayIt methods
		private void PlayIt(string EncodedTag) {
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
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
try{
			if(!SPEECH_ENABLED) {
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = "Speech not available\nPut Interop.SpeechLib.DLL into 'bin\\Custom' folder and restart the platform";
				Print(AlertMsg+"   cancelling");
				return;
			}
Print("instructed to say: "+EncodedTag);
			string[] elements = EncodedTag.Split(new char[]{':'}, StringSplitOptions.None);
			if(elements.Length>0) {
				SpeechDelegate_AutoTrendChannel speechThread = new SpeechDelegate_AutoTrendChannel(SayItThread);
				string SayThis = elements[elements.Length-1].ToUpper();
				if(SayThis.Contains("[SAYPRICE]")) {
					string pricestr1 = Instrument.MasterInstrument.FormatPrice(Price);
					string spokenprice = string.Empty + pricestr1[0];
					int i = 1;
					while(i<pricestr1.Length) {
						spokenprice = MakeString(new Object[]{spokenprice," ",pricestr1[i++]});
					}
					spokenprice = spokenprice.Replace(".","point");
					SayThis = SayThis.Replace("[SAYPRICE]", spokenprice);
				}
				SayThis = SayThis.Replace("[INSTRUMENT]", Instrument.MasterInstrument.Name);
				speechThread.BeginInvoke(SayThis, null, null);
				TimeOfText  = NinjaTrader.Core.Globals.Now;
				AlertMsg = MakeString(new Object[]{"'",SayThis,"' was spoken"});
				Print(AlertMsg);
			}
}catch(Exception err){Print("SayIt Error: "+err.ToString());}
		}
#else
		private void SayIt (string EncodedTag) {}
		private void SayIt (string EncodedTag, double Price) {}
#endif
//==================================================================
	private static string MakeString(object[] s){
		System.Text.StringBuilder stb = new System.Text.StringBuilder(null);
		for(int i = 0; i<s.Length; i++) {
			stb = stb.Append(s[i].ToString());
		}
		return stb.ToString();
	}
		#endregion
//====================================================================================================
		private string   AlertMsg="";
		private DateTime TimeOfText = DateTime.MinValue;
		private bool     SPEECH_ENABLED = false;
		private string FS = string.Empty;
#if SPEECH_ENABLED
		static System.Reflection.Assembly  SpeechLib = null;
		private string pSpeakOnUpwardBreakout = "[instrument] Upward breakout at [SayPrice] on [instrument]";
		[Description("Message to speak when topside trendline gets hit...leave blank to turn-off spoken message")]
		[Category("Alert Voice")]
		public string SpeakOnUpwardBreakout
		{
			get { return pSpeakOnUpwardBreakout; }
			set { pSpeakOnUpwardBreakout = value; }
		}
		private string pSpeakOnDownwardBreakout = "[instrument] Downward breakout at [SayPrice] on [instrument]";
		[Description("Message to speak when bottomside trendline gets hit...leave blank to turn-off spoken message")]
		[Category("Alert Voice")]
		public string SpeakOnDownwardBreakout
		{
			get { return pSpeakOnDownwardBreakout; }
			set { pSpeakOnDownwardBreakout = value; }
		}
#else
		private string pSpeakOnUpwardBreakout = "";
		private string pSpeakOnDownwardBreakout = "";
#endif
			private double trendlinePrice=0.0;
			private Brush downTrendBrush = Brushes.Red;
			private Brush upTrendBrush = Brushes.Green;
			private int signal = 0; // 0 = no signal, 1 = buy signal on down trend break, 2 = sell signal on up trend break

		private int      LineNumber = 0;
		private int      ArrowNumber = 0;
		private int      ArrowBar = -1;
		private int      AlertsThisBar = 0;
		//private int      DiamondsThisChannel = 0;
		private List<Point> UpTLsSignaled = new List<Point>();
		private List<Point> DownTLsSignaled = new List<Point>();

		private Swing swing;
		private List<Point>  UpTLs, DownTLs;
		private List<double> UpRayPrices, DownRayPrices;
		private List<int> ArrowNumbers;
		private double   PriorPrice = 0;
		private bool     InitAlertPrices = true;
		private int      EmailBar = -1;
		private int      EmailBarCL = -1;
		private double   RisingChannelHeight = 0;
		private double   FallingChannelHeight = 0;
		private int      ChannelAlertBar = 0;
		private Series<int> CLhit;
		private Series<int> CLreset;
		private Series<int> ConservativeSetupMode;
		private bool     ChannelLineHit = false;
		private int aBarAtLastCLreset =-1;
		private int aBarAtLastCLsignal = -1;
		private int aBarAtLastCLhit = -1;
		private int COBCoffset = 0;
		private int upTrendStartBarsAgo		= 0;
		private int upTrendEndBarsAgo 		= 0;
		private int upTrendOccurence 		= 1;
		private int downTrendStartBarsAgo	= 0;
		private int downTrendEndBarsAgo 	= 0;
		private int downTrendOccurence 		= 1;
		private int UpTrendStartABar = 0;
		private int UpTrendEndABar = 0;
		private int DownTrendStartABar = 0;
		private int DownTrendEndABar = 0;
		private int LastUpTrendEndABar = 0;
		private int LastDownTrendEndABar = 0;
		private List<int> ABarAtNewTL = new List<int>();
		private bool IsBen = false;

//		private int      CLhit = 0; //+1 if falling ChannelLine hit, -1 if rising ChannelLine hit

		protected override void OnStateChange(){
			if (State == State.SetDefaults)
			{
				IsBen = System.IO.File.Exists("c:\\222222222222.txt");
				IsBen = IsBen && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "AIAutoTrendChannel", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				AddPlot(new Stroke(Brushes.MediumBlue,2),  PlotStyle.Dot,   "UpTrendlinePrice");
				AddPlot(new Stroke(Brushes.Red,2),         PlotStyle.Dot,   "DownTrendlinePrice");
				AddPlot(new Stroke(Brushes.DodgerBlue,1),  PlotStyle.Block, "UpChannellinePrice");
				AddPlot(new Stroke(Brushes.Magenta,1),     PlotStyle.Block, "DownChannellinePrice");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "TrendlineSignal");
				AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Dot, "ChannellineSignal");
				//			DisplayInDataBox 	= false;
				Calculate=Calculate.OnBarClose;
				IsOverlay=true;
				IsAutoScale=false;
				//PriceTypeSupported	= false;
				string Version = "5.7";
				Name="iw AutoTrendChannel v"+Version;
				UpTLs = new List<Point>();
				DownTLs = new List<Point>();
				ArrowNumbers = new List<int>();
			}else if (State == State.Configure) {
				#region OnStartup
	#if SPEECH_ENABLED
				string dll = System.IO.Path.Combine(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir,"bin\custom"),"interop.speechlib.dll");   //"c:\users\ben\Documents\NinjaTrader 7\"
				if(System.IO.File.Exists(dll)) {
					SPEECH_ENABLED = true;
					SpeechLib = Assembly.LoadFile(dll);
				}
	#endif
				swing = Swing(strength);
				swing.Calculate = Calculate.OnBarClose;
				UpRayPrices = new List<double>();
				DownRayPrices = new List<double>();
				CLhit = new Series<int>(this);
				CLreset = new Series<int>(this);
	//			if(this.pConservativeEntryCL) ConservativeSetupMode = new Series<int>(this);
				#endregion
			}
		}
//=============================================================================================
		private bool IsValidCLSignal(int Direction, int LastAlertABar) {//Direction = -1 for a Sell search, +1 for a Buy search
			bool result = false;
			if(Direction == -1) {
				if(High[0] >= UpChannellinePrice[0]-pTouchZoneSizeCL*TickSize) {
					ChannelLineHit=true;
					aBarAtLastCLhit = CurrentBar;
				}
				if(!UpTrendlinePrice.IsValidDataPoint(1) || High[1] < UpChannellinePrice[1]-pTouchZoneSizeCL*TickSize) aBarAtLastCLreset = CurrentBar-1;

				if(pConservativeEntryCL) {
//Draw.Text(this, "reset"+CurrentBar,(CurrentBar-aBarAtLastCLreset-1).ToString(),1,High[1]+ATR(14)[0],Color.Green);
//Draw.Text(this, "signal"+CurrentBar,(CurrentBar-aBarAtLastCLsignal-1).ToString(),1,Low[1]-ATR(14)[0],Color.White);
//Draw.Text(this, "hit"+CurrentBar,(CurrentBar-aBarAtLastCLhit).ToString(),1,Low[1]-ATR(14)[0]-TickSize,Color.Red);
					if(ChannelLineHit) {
						if(aBarAtLastCLreset >= aBarAtLastCLsignal && aBarAtLastCLreset < aBarAtLastCLhit) 
						{
							if(this.Calculate==Calculate.OnBarClose &&Close[0]<Open[0])result=true;
							if(this.Calculate!=Calculate.OnBarClose &&Close[1]<Open[1])result=true;
						}
						if(result) {
							aBarAtLastCLsignal = CurrentBar;
//							BackBrush=Color.Pink;
						}
						if(High[1] < UpChannellinePrice[1]-this.pTouchZoneSizeCL*TickSize) {
							aBarAtLastCLreset = CurrentBar-1;
						}
					}
				} else {
					if(!UpTrendlinePrice.IsValidDataPoint(1) || High[1] < UpChannellinePrice[1]-this.pTouchZoneSizeCL*TickSize) CLreset[0]=(1); //could not convert else CLreset[0]=(0);
					if (CLreset[0]==1 && CLhit[0] == -1) result = true;
				}
			} else {
				if(pConservativeEntryCL) {
					if(Low[0] <= DownChannellinePrice[0]+pTouchZoneSizeCL*TickSize) {
						ChannelLineHit=true;
						aBarAtLastCLhit = CurrentBar;
					}
					if(!DownTrendlinePrice.IsValidDataPoint(1) || Low[1] > DownChannellinePrice[1]+pTouchZoneSizeCL*TickSize) aBarAtLastCLreset = CurrentBar-1;
					if(ChannelLineHit) {
						if(aBarAtLastCLreset >= aBarAtLastCLsignal && aBarAtLastCLreset < aBarAtLastCLhit) 
						{
							if(this.Calculate==Calculate.OnBarClose && Close[0]>Open[0])result=true;
							if(this.Calculate!=Calculate.OnBarClose && Close[1]>Open[1])result=true;
						}
						if(result) {
							aBarAtLastCLsignal = CurrentBar;
//							BackBrush=Color.Cyan;
						}
						if(Low[1] > DownChannellinePrice[1]+this.pTouchZoneSizeCL*TickSize) {
							aBarAtLastCLreset = CurrentBar-1;
						}
					}
				} else {
					if(!DownTrendlinePrice.IsValidDataPoint(1) || Low[1] > DownChannellinePrice[1]+this.pTouchZoneSizeCL*TickSize) CLreset[0]=(1); //could not convert else CLreset[0]=(0);
					if (CLreset[0]==1 && CLhit[0] == 1) result = true;
				}
			}
			return result;
		}
//=============================================================================================
		protected override void OnBarUpdate()
		{
			if(CurrentBar<1) return;
			if(IsFirstTickOfBar) {
				TrendlineSignal[0]=(0);
				ChannellineSignal[0]=(ChannellineSignal[1]);
				if(this.Calculate!=Calculate.OnBarClose) COBCoffset=1; else COBCoffset=0;
//				if(this.pConservativeEntryCL) ConservativeSetupMode[0]=(0);
				//CLreset[0]=(CLreset[1]);
			}
			CLhit[0]=(0);
//			if(this.pConservativeEntryCL) ConservativeSetupMode[0]=(ConservativeSetupMode[1]);
			//if(!this.pConservativeEntryCL) CLhit.Set(0) //could not convert else CLhit[0]=(CLhit[1]);

			while(ArrowNumbers.Count >= pMaxArrowsShown) {
				RemoveDrawObject(string.Concat("UArrow",ArrowNumbers[ArrowNumbers.Count-1].ToString()));
				RemoveDrawObject(string.Concat("DArrow",ArrowNumbers[ArrowNumbers.Count-1].ToString()));
				ArrowNumbers.RemoveAt(ArrowNumbers.Count-1);
			}
			if(IsFirstTickOfBar) {
				AlertsThisBar = 0;
				// Calculate up trend line
				upTrendStartBarsAgo		= 0;
				upTrendEndBarsAgo 		= 0;
				upTrendOccurence 		= 1;

//Print(Time[0].ToString());
				if(pTrendlineType != AutoTrendChannel_Type.Downward) {
					while (Low[upTrendEndBarsAgo] <= Low[upTrendStartBarsAgo]) {
						upTrendStartBarsAgo 	= swing.SwingLowBar(COBCoffset, upTrendOccurence + 1, CurrentBar);
						upTrendEndBarsAgo 		= swing.SwingLowBar(COBCoffset, upTrendOccurence, CurrentBar);
						if (upTrendStartBarsAgo < 0 || upTrendEndBarsAgo < 0)
							break;

//Print("Start/Stop:  "+Time[upTrendStartBarsAgo].ToString()+"  End: "+Time[upTrendEndBarsAgo].ToString());
						UpTrendStartABar = CurrentBar-upTrendStartBarsAgo;
						UpTrendEndABar = CurrentBar-upTrendEndBarsAgo;
//Print(upTrendOccurence+":  "+UpTrendStartABar+" : "+UpTrendEndABar);
						upTrendOccurence++;
					}
				} else upTrendStartBarsAgo=int.MaxValue;

				// Calculate down trend line	
				downTrendStartBarsAgo	= 0;
				downTrendEndBarsAgo 	= 0;
				downTrendOccurence 		= 1;

				if(pTrendlineType != AutoTrendChannel_Type.Upward) {
					while (High[downTrendEndBarsAgo] >= High[downTrendStartBarsAgo])
					{
						downTrendStartBarsAgo 		= swing.SwingHighBar(COBCoffset, downTrendOccurence + 1, CurrentBar);
						downTrendEndBarsAgo 		= swing.SwingHighBar(COBCoffset, downTrendOccurence, CurrentBar);

						if (downTrendStartBarsAgo < 0 || downTrendEndBarsAgo < 0)
							break;

						DownTrendStartABar = CurrentBar-downTrendStartBarsAgo;
						DownTrendEndABar = CurrentBar-downTrendEndBarsAgo;
						downTrendOccurence++;
					}
				} else downTrendStartBarsAgo=int.MaxValue;
			}
			if(upTrendStartBarsAgo!=int.MaxValue) {
				upTrendStartBarsAgo = CurrentBar-UpTrendStartABar;
				upTrendEndBarsAgo   = CurrentBar-UpTrendEndABar;
			}
			if(downTrendStartBarsAgo!=int.MaxValue) {
				downTrendStartBarsAgo = CurrentBar-DownTrendStartABar;
				downTrendEndBarsAgo   = CurrentBar-DownTrendEndABar;
			}

			bool ValidSignal = false;
			bool ValidSignalCL = false;
			double LowerSignalPrice = 0;
			double UpperSignalPrice = 0;
			// We have found an uptrend and the uptrend is the current trend
			if (upTrendStartBarsAgo > 0 && upTrendEndBarsAgo > 0 && upTrendStartBarsAgo < downTrendStartBarsAgo)
			{

				double startBarPrice 	= Bars.GetLow(UpTrendStartABar);
				double endBarPrice 		= Bars.GetLow(UpTrendEndABar); 

				double changePerBar 	= (endBarPrice - startBarPrice) / (Math.Abs(upTrendEndBarsAgo - upTrendStartBarsAgo));
				UpTrendlinePrice[0] = (endBarPrice+changePerBar*upTrendEndBarsAgo); //could not convert


				// Draw the up trend line
				{
					Point p = new Point(UpTrendStartABar, UpTrendEndABar);
					if(!UpTLs.Contains(p) && UpTrendStartABar >= LastUpTrendEndABar) {
#if SPEECH_ENABLED
						if(!(State == State.Historical)) {
							SayIt("SayIt:New upward trendline on [instrument]",0);
						}
#endif
						LastUpTrendEndABar = UpTrendEndABar;
						if(pEnableAudibleAlertNewTL) Alert(CurrentBar.ToString(),Priority.High,"New upward Trendline created",AddSoundFolder(pNewTrendlineWAV),1,Brushes.Green,Brushes.White);
						if(pBackBrushForNewTL != Brushes.Transparent) BackBrush = pBackBrushForNewTL;
						if(pRemoveHistoricalDots && (ABarAtNewTL.Count==0 || ABarAtNewTL[0]!=CurrentBar)) ABarAtNewTL.Insert(0,CurrentBar);
						ChannelLineHit=false;
						#region Draw trend and channel line ray
						string tag = string.Concat("UpTL ",p.X,"-",p.Y);
						if(ChartControl!=null) Draw.Ray(this, tag, false, CurrentBar-UpTrendStartABar, startBarPrice, CurrentBar-UpTrendEndABar, endBarPrice, upTrendBrush, lineDashStyle, lineWidth);
						if(pShowChannels){// && ChartControl!=null) {
							double maxp = High[0]; int relbar = 0;
							double mainlineprice = 0;
							double slope = (endBarPrice-startBarPrice) / Math.Abs(upTrendEndBarsAgo-upTrendStartBarsAgo);
							if(pChannelBasis == AutoTrendChannel_ChannelBasis.HighsLows)       {maxp=High[0];  for(int b = 0; b<upTrendStartBarsAgo; b++) {if(High[b]>=maxp)  { maxp=High[b];  relbar=b;}}}
							else if(pChannelBasis == AutoTrendChannel_ChannelBasis.Closes)     {maxp=Close[0]; for(int b = 0; b<upTrendStartBarsAgo; b++) {if(Close[b]>=maxp) { maxp=Close[b]; relbar=b;}}}
							else if(pChannelBasis == AutoTrendChannel_ChannelBasis.Opens)      {maxp=Open[0];  for(int b = 0; b<upTrendStartBarsAgo; b++) {if(Open[b]>=maxp)  { maxp=Open[b];  relbar=b;}}}
							else if(pChannelBasis == AutoTrendChannel_ChannelBasis.OpensCloses){maxp=Math.Max(Open[0],Close[0]);  for(int b = 0; b<upTrendStartBarsAgo; b++) {double px = Math.Max(Open[b],Close[b]); if(px>=maxp)  { maxp=px;  relbar=b;}}}
							int x0 = upTrendStartBarsAgo;
							int x1 = relbar;
							double y1 = maxp;
							double y0 = slope*(relbar-upTrendStartBarsAgo) + y1;
							RisingChannelHeight = (slope*(relbar) + y1) - UpTrendlinePrice[0];
							if(ChartControl!=null) Draw.Ray(this, string.Concat("C",tag), false, x0, y0, x1, y1, channelUpTrendBrush, channelDashStyle, channelLineWidth);
							ChannellineSignal[0]=(0);
							ChannellineSignal[1]=(0);
						}
						UpTLs.Add(new Point(UpTrendStartABar, UpTrendEndABar));
//Print("Adding "+string.Concat("UpTL ",p.X,"-",p.Y));
						#endregion
					}
				}

				ValidSignalCL = false;
				if(ChannellineSignal.GetValueAt(CurrentBar) > -pMaxDiamondsPerChannel && RisingChannelHeight>0 && UpTrendlinePrice.IsValidDataPointAt(CurrentBar)) {
					UpChannellinePrice[0] = (UpTrendlinePrice[0]+RisingChannelHeight); 

					if(Low.GetValueAt(CurrentBar) <= UpChannellinePrice.GetValueAt(CurrentBar)+this.pTouchZoneSizeCL*TickSize && High.GetValueAt(CurrentBar) >= UpChannellinePrice.GetValueAt(CurrentBar)-this.pTouchZoneSizeCL*TickSize) CLhit[0]=(-1);
					ValidSignalCL = IsValidCLSignal(-1, ChannelAlertBar);
//if(!ValidSignalCL) BackBrush=Color.Maroon;
					if(ChannelAlertBar != CurrentBar && ValidSignalCL) {
						//ChannelAlertBar = CurrentBar;
//===================================================================================================
						if(this.pEnableAudibleAlertCL && AlertsThisBar<pMaxAlertsPerBar) {
							AlertsThisBar++;
							string msg = string.Format("{0} Rising Channel line {1} at {2}, on {3} {4}", pAlertMsgLabel, SignalTypeToTxt(pSignalType), Instrument.MasterInstrument.FormatPrice(UpChannellinePrice[0]), Instrument.FullName, Bars.BarsPeriod.ToString());
							if(pVerboseAlertLogMessage){
								if(IsBen) Print("AlertLog message: '"+msg+"'"+Time[0].ToString());
								Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileTopCL), 1, Brushes.Black, Brushes.Red);
							}else{
								if(IsBen) Print("AlertLog message 'Up channel line broken'"+Time[0].ToString());
								Alert(CurrentBar.ToString(), Priority.High, "Up channel line broken", AddSoundFolder(pSoundFileTopCL), 1, Brushes.Black, Brushes.Red);
							}
						}
//===================================================================================================
//						if(this.EnableAudibleAlertCL) PlaySoundVerified(this.pSoundFileTopCL);
						if(EmailBarCL!=CurrentBar && pSendEmailsCL && pEmailAddressCL.Length>0) {
							EmailBarCL = CurrentBar;
							string msg = string.Format("{0} Rising Channel line {1} at {2}, on {3} {4}", this.pAlertMsgLabel, SignalTypeToTxt(pSignalType), Instrument.MasterInstrument.FormatPrice(UpChannellinePrice[0]), Instrument.FullName, Bars.BarsPeriod.ToString());
							if(IsBen) Print(Time[0].ToString()+"   Sending email to: "+pEmailAddressCL+"  msg: "+msg);
							SendFullEmailMessage(pEmailAddressCL, msg);
						}
						ChannellineSignal[0]=(ChannellineSignal[1]-1);
					}
					if (ChannelAlertBar == CurrentBar) {
						if(pConservativeEntryCL) CLreset[1]=(0);
						if(ChannellineSignal[1]>=0) ChannellineSignal[0]=(-1);
						if(pShowDiamonds && ChartControl!=null) {
							if(IsBen) Print("Drawn down diamond at "+Time[0].ToString());
							if(pConservativeEntryCL && this.Calculate!=Calculate.OnBarClose) 
								Draw.Diamond(this, "CLhit"+CurrentBar, this.IsAutoScale, 1, High[1]+TickSize, pDownDiamondBrush);
							else
								Draw.Diamond(this, "CLhit"+CurrentBar, this.IsAutoScale, 0, High[0]+TickSize, pDownDiamondBrush);
							//BackBrush = Color.Pink;
						}
					}
				}

				int barsAgo = 0;
				{
					ValidSignal = false;
//Print("Checking for uptrend linebreak  barsago: "+barsAgo+"  "+Time[upTrendEndBarsAgo].ToString());
					LowerSignalPrice = endBarPrice + (Math.Abs(upTrendEndBarsAgo - barsAgo) * changePerBar);
					UpperSignalPrice = LowerSignalPrice + this.pTouchZoneSizeTL * TickSize;
//RemoveDrawObject("HUpper");
//RemoveDrawObject("HLower");
//DrawLine(this, "HUpper",UpperSignalPrice,Color.White);
//DrawLine(this, "HLower",LowerSignalPrice,Color.Blue);
					if((pSignalType == AutoTrendChannel_SignalType.Touch || pSignalType == AutoTrendChannel_SignalType.TouchAndBreak) && 
					   Close.GetValueAt(CurrentBar) <= UpperSignalPrice &&
					   Close.GetValueAt(CurrentBar) >= LowerSignalPrice) {
						ValidSignal = true;
					}
					if((pSignalType == AutoTrendChannel_SignalType.Break  || pSignalType == AutoTrendChannel_SignalType.TouchAndBreak) && 
					   Close.GetValueAt(CurrentBar) < LowerSignalPrice) {
//DrawDot(this, "Price",false,1,LowerSignalPrice,Color.Purple);
						ValidSignal = true;
					}
//if(!ValidSignal) BackBrush=Color.DimGray;
					if (ValidSignal)
					{
						//BackBrush=Color.Pink;
						// Alert will only trigger in real-time
						if (!UpTLsSignaled.Contains(UpTLs[UpTLs.Count-1]))
						{
							UpTLsSignaled.Add(UpTLs[UpTLs.Count-1]);
							// Set the signal only if the break is on the right most bar
							TrendlineSignal[0]=(-1);
							string msg = string.Format("{0} Rising Trendline {1} at {2}, on {3} {4}", pAlertMsgLabel, SignalTypeToTxt(pSignalType), Instrument.MasterInstrument.FormatPrice(UpTrendlinePrice[0]), Instrument.FullName, Bars.BarsPeriod.ToString());
							if(pShowArrows && ChartControl!=null) {
//Print(Time[0].ToString()+"  Drawing down arrow");
								if(ArrowNumbers.Count==0 || ArrowNumbers[0]!=CurrentBar) ArrowNumbers.Insert(0,CurrentBar);
								if(IsBen) Print("Drawn down arrow at "+Time[barsAgo].ToString());
								Draw.ArrowDown(this, string.Concat("DArrow",CurrentBar), this.IsAutoScale, barsAgo, High[barsAgo] + TickSize, pDownArrowBrush);
							}
							if(pEnableAudibleAlertTL && AlertsThisBar<pMaxAlertsPerBar) {
								AlertsThisBar++;
								if(pSpeakOnUpwardBreakout.Length>0) {
									if(!(State == State.Historical)) SayIt(pSpeakOnUpwardBreakout, UpTrendlinePrice[0]);
								}
								else {
									if(pVerboseAlertLogMessage){
										if(IsBen) Print("AlertLog message: '"+msg+"'"+Time[0].ToString());
										Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileTop), 1, Brushes.Black, Brushes.Red);
									}else{
										if(IsBen) Print("AlertLog message 'Up trend line broken'"+Time[0].ToString());
										Alert(CurrentBar.ToString(), Priority.High, "Up trend line broken", AddSoundFolder(pSoundFileTop), 1, Brushes.Black, Brushes.Red);
									}
								}
							}
							if(EmailBar!=CurrentBar && pSendEmailsTL && pEmailAddressTL.Length>0) {
								EmailBar = CurrentBar;
								if(IsBen) Print(Time[0].ToString()+"   Sending email to: "+pEmailAddressTL+"  msg: "+msg);
								SendMail(pEmailAddressTL, msg, string.Format("{0}{1}{1}Email auto-generated from NinjaTrader",msg,Environment.NewLine));
							}
						}
					} else {
//Print(TrendlineSignal[0].ToString());
//if(UpTLsSignaled.Count>0) Print("  UpTLsSignaled: "+UpTLsSignaled[UpTLsSignaled.Count-1].ToString()+"   last Up ray: "+UpTLs[UpTLs.Count-1].ToString());
						if(TrendlineSignal.GetValueAt(CurrentBar)<0 && UpTLsSignaled.Contains(UpTLs[UpTLs.Count-1])) {
							int idx = UpTLsSignaled.IndexOf(UpTLs[UpTLs.Count-1]);
//Print("idx: "+idx+" out of "+UpTLsSignaled.Count);
							if(idx>0) {
//								Print("Removing UpTLs["+idx+"] : "+UpTLsSignaled[idx].ToString());
								UpTLsSignaled.RemoveAt(idx);
							}
							TrendlineSignal[0]=(0);
							if(pShowArrows && ChartControl!=null) {
								RemoveDrawObject(string.Concat("DArrow",CurrentBar));
								if(ArrowNumbers.Count>0) ArrowNumbers.RemoveAt(ArrowNumbers.Count-1);
							}
						}
					}
				}
			}
			// We have found a downtrend and the downtrend is the current trend
			if (downTrendStartBarsAgo > 0 && downTrendEndBarsAgo > 0  && upTrendStartBarsAgo > downTrendStartBarsAgo)
			{
				double startBarPrice 	= Bars.GetHigh(DownTrendStartABar);
				double endBarPrice 		= Bars.GetHigh(DownTrendEndABar);
				double changePerBar 	= (endBarPrice - startBarPrice) / (Math.Abs(downTrendEndBarsAgo - downTrendStartBarsAgo));
				DownTrendlinePrice[0]=(endBarPrice+changePerBar*downTrendEndBarsAgo); //could not convert

				// Draw the down trend line
				{
					Point p = new Point(DownTrendStartABar, DownTrendEndABar);
					if(!DownTLs.Contains(p) && DownTrendStartABar >= LastDownTrendEndABar) {
						ChannelLineHit=false;
#if SPEECH_ENABLED
						if(!(State == State.Historical)) {
							SayIt("SayIt:New downward trendline on [instrument]",0);
						}
#endif
						LastDownTrendEndABar = DownTrendEndABar;
						if(pEnableAudibleAlertNewTL) Alert(CurrentBar.ToString(),Priority.High,"New downward Trendline created",AddSoundFolder(pNewTrendlineWAV),1,Brushes.Red,Brushes.White);
						if(pBackBrushForNewTL!=Brushes.Transparent) BackBrush = pBackBrushForNewTL;
						if(pRemoveHistoricalDots && (ABarAtNewTL.Count==0 || ABarAtNewTL[0]!=CurrentBar)) ABarAtNewTL.Insert(0,CurrentBar);
						#region Draw trend and channel line ray
						string tag = string.Concat("DownTL ",p.X,"-",p.Y);
						if(ChartControl!=null) Draw.Ray(this, tag, false, downTrendStartBarsAgo, startBarPrice, downTrendEndBarsAgo, endBarPrice, downTrendBrush, lineDashStyle, lineWidth);
						if(pShowChannels){// && ChartControl!=null) {
							double minp = Low[0];  int relbar = 0;
							double mainlineprice = 0;
							double slope = (endBarPrice-startBarPrice) / Math.Abs(downTrendEndBarsAgo-downTrendStartBarsAgo);
							if(pChannelBasis == AutoTrendChannel_ChannelBasis.HighsLows)       {minp=Low[0];   for(int b = 0; b<downTrendStartBarsAgo; b++) {if(Low[b]<=minp) {minp=Low[b];   relbar=b;}}}
							else if(pChannelBasis == AutoTrendChannel_ChannelBasis.Closes)     {minp=Close[0]; for(int b = 0; b<downTrendStartBarsAgo; b++) {if(Close[b]<=minp) { minp=Close[b]; relbar=b;}}}
							else if(pChannelBasis == AutoTrendChannel_ChannelBasis.Opens)      {minp=Open[0];  for(int b = 0; b<downTrendStartBarsAgo; b++) {if(Open[b]<=minp)  { minp=Open[b];  relbar=b;}}}
							else if(pChannelBasis == AutoTrendChannel_ChannelBasis.OpensCloses){minp=Math.Min(Open[0],Close[0]);  for(int b = 0; b<downTrendStartBarsAgo; b++) {double px = Math.Min(Open[b],Close[b]); if(px<=minp)  { minp=px;  relbar=b;}}}
							int x0 = downTrendStartBarsAgo;
							int x1 = relbar;
							double y1 = minp;
							double y0 = slope*(relbar-downTrendStartBarsAgo) + y1;
							FallingChannelHeight = DownTrendlinePrice[0] - (slope*(relbar) + y1);
							if(ChartControl!=null) Draw.Ray(this, string.Concat("C",tag), false, x0, y0, x1, y1, channelDownTrendBrush, channelDashStyle, channelLineWidth);
							ChannellineSignal[0]=(0);
							ChannellineSignal[1]=(0);
						}
						DownTLs.Add(new Point(DownTrendStartABar, DownTrendEndABar));
//Print("Adding "+string.Concat("DownTL ",p.X,"-",p.Y));
						#endregion
					}
				}
				ValidSignalCL = false;
				if(ChannellineSignal.GetValueAt(CurrentBar) < pMaxDiamondsPerChannel && FallingChannelHeight>0 && DownTrendlinePrice.IsValidDataPoint(0)) {
					DownChannellinePrice[0]=(DownTrendlinePrice[0]-FallingChannelHeight);

					if(Low.GetValueAt(CurrentBar) <= DownChannellinePrice.GetValueAt(CurrentBar)+this.pTouchZoneSizeCL*TickSize && High.GetValueAt(CurrentBar) >= DownChannellinePrice.GetValueAt(CurrentBar)-this.pTouchZoneSizeCL*TickSize) CLhit[0]=(1);
					ValidSignalCL = IsValidCLSignal(1, ChannelAlertBar);
//if(!ValidSignalCL) BackBrush=Color.Maroon;
					if(pConservativeEntryCL || CLhit[1] != 1) {
						if(ChannelAlertBar != CurrentBar && ValidSignalCL) {
//							ChannelAlertBar = CurrentBar;
//===================================================
							if(this.pEnableAudibleAlertCL && AlertsThisBar<pMaxAlertsPerBar) {
								AlertsThisBar++;
								string msg = string.Format("{0} Falling Channel line {1} at {2}, on {3} {4}", pAlertMsgLabel, SignalTypeToTxt(pSignalType), Instrument.MasterInstrument.FormatPrice(DownChannellinePrice[0]), Instrument.FullName, Bars.BarsPeriod.ToString());
								if(pVerboseAlertLogMessage){
									if(IsBen) Print("AlertLog message: '"+msg+"'"+Time[0].ToString());
									Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileBottomCL), 1, Brushes.Black, Brushes.Red);
								}else{
									if(IsBen) Print("AlertLog message 'Down channel line broken'"+Time[0].ToString());
									Alert(CurrentBar.ToString(), Priority.High, "Down channel line broken", AddSoundFolder(pSoundFileBottomCL), 1, Brushes.Black, Brushes.Red);
								}
							}
//===================================================
//							if(this.EnableAudibleAlertCL) 
//								PlaySoundVerified(this.pSoundFileBottomCL);
							if(EmailBarCL!=CurrentBar && pSendEmailsCL && pEmailAddressCL.Length>0) {
								EmailBarCL = CurrentBar;
								string msg = string.Format("{0} Falling Channel line {1} at {2}, on {3} {4}",Name, SignalTypeToTxt(pSignalType), Instrument.MasterInstrument.FormatPrice(DownChannellinePrice[0]), Instrument.FullName, Bars.BarsPeriod.ToString());
								if(IsBen) Print(Time[0].ToString()+"   Sending email to: "+pEmailAddressCL+"  msg: "+msg);
								SendMail(pEmailAddressCL, msg, string.Format("{0}{1}{1}Email auto-generated from NinjaTrader",msg,Environment.NewLine));
							}
							ChannellineSignal[0] = (ChannellineSignal[1]+1); //could not convert
						}
						if (ChannelAlertBar == CurrentBar) {
							if(pConservativeEntryCL) {
								CLreset[1]=(0);
//RemoveDrawObject("signal"+(CurrentBar-1).ToString());
//Draw.Text(this, "signal"+(CurrentBar-1).ToString(),CLreset[1].ToString(),1,Low[1]-ATR(14)[0],Color.White);
							}
							if(ChannellineSignal[1]<=0) ChannellineSignal[0]=(1);
//							CLhit[0]=(0);
//							CLhit[1]=(0);
							if(pShowDiamonds && ChartControl!=null) {
								if(IsBen) Print("Drawn up diamond at "+Time[0].ToString());
								if(pConservativeEntryCL && this.Calculate!=Calculate.OnBarClose)
									Draw.Diamond(this, "CLhit"+CurrentBar,this.IsAutoScale,1,Low[1]-TickSize,pUpDiamondBrush);
								else
									Draw.Diamond(this, "CLhit"+CurrentBar,this.IsAutoScale,0,Low[0]-TickSize,pUpDiamondBrush);
//								BackBrush = Color.Pink;
							}
						}
					}
				}

				int barsAgo = 0;
				{
					ValidSignal = false;
//Print("Checking for downtrend linebreak  barsago: "+barsAgo+"  "+Time[downTrendEndBarsAgo].ToString());
					UpperSignalPrice = endBarPrice + (Math.Abs(downTrendEndBarsAgo - barsAgo) * changePerBar);
					LowerSignalPrice = UpperSignalPrice - this.pTouchZoneSizeTL * TickSize;
					if((pSignalType == AutoTrendChannel_SignalType.Touch  || pSignalType == AutoTrendChannel_SignalType.TouchAndBreak) && 
					   Close.GetValueAt(CurrentBar) <= UpperSignalPrice &&
					   Close.GetValueAt(CurrentBar) >= LowerSignalPrice) {
						ValidSignal = true;
					}
					if((pSignalType == AutoTrendChannel_SignalType.Break  || pSignalType == AutoTrendChannel_SignalType.TouchAndBreak) && 
					   Close.GetValueAt(CurrentBar) > UpperSignalPrice) {
//DrawDot(this, "Price",false,1,UpperSignalPrice,Color.Purple);
						ValidSignal = true;
					}
//if(!ValidSignal) BackBrush=Color.DimGray;

					if (ValidSignal)
					{
//						BackBrush=Color.Pink;
						// Alert will only trigger in real-time
						if (!DownTLsSignaled.Contains(DownTLs[DownTLs.Count-1]))
						{
							DownTLsSignaled.Add(DownTLs[DownTLs.Count-1]);
//Print("DownTLsSignaled: "+DownTLsSignaled[DownTLsSignaled.Count-1].ToString());
							// Set the signal only if the break is on the right most bar
							TrendlineSignal[0]=(1);
//Print(TrendlineSignal[0].ToString());
							string msg = string.Format("{0} Falling Trendline {1} at {2}, on {3} {4}",pAlertMsgLabel, SignalTypeToTxt(pSignalType), Instrument.MasterInstrument.FormatPrice(DownTrendlinePrice[0]), Instrument.FullName, Bars.BarsPeriod.ToString());
							if(pShowArrows && ChartControl!=null) {
	//Print("Drawing up arrow");
								if(ArrowNumbers.Count==0 || ArrowNumbers[0]!=CurrentBar) ArrowNumbers.Insert(0, CurrentBar);
								if(IsBen) Print("Drawn up arrow at "+Time[barsAgo].ToString());
								Draw.ArrowUp(this, string.Concat("UArrow",CurrentBar), this.IsAutoScale, barsAgo, Low[barsAgo] - TickSize, pUpArrowBrush);
							}
							if(pEnableAudibleAlertTL && AlertsThisBar<pMaxAlertsPerBar) {
								AlertsThisBar++;
								if(pSpeakOnDownwardBreakout.Length>0) {
									if(!(State == State.Historical)) SayIt(pSpeakOnDownwardBreakout, DownTrendlinePrice[0]);
								}
								else {
									if(pVerboseAlertLogMessage){
										if(IsBen) Print("AlertLog message: '"+msg+"'"+Time[0].ToString());
										Alert(CurrentBar.ToString(), Priority.High, msg, AddSoundFolder(pSoundFileBottom), 1, Brushes.Black, Brushes.Green);
									}else{
										if(IsBen) Print("AlertLog message 'Down trend line broken'"+Time[0].ToString());
										Alert(CurrentBar.ToString(), Priority.High, "Down trend line broken", AddSoundFolder(pSoundFileBottom), 1, Brushes.Black, Brushes.Green);
									}
								}
							}
							if(EmailBar!=CurrentBar && pSendEmailsTL && pEmailAddressTL.Length>0) {
								EmailBar = CurrentBar;
								if(IsBen) Print(Time[0].ToString()+"   Sending email to: "+pEmailAddressTL+"  msg: "+msg);
								SendMail(pEmailAddressTL, msg, string.Format("{0}{1}{1}Email auto-generated from NinjaTrader",msg,Environment.NewLine));
							}
						}
					} else {
//Print(TrendlineSignal[0].ToString());
//if(DownTLsSignaled.Count>0) Print("  DownTLsSignaled: "+DownTLsSignaled[DownTLsSignaled.Count-1].ToString()+"   last down ray: "+DownTLs[DownTLs.Count-1].ToString());
						if(TrendlineSignal.GetValueAt(CurrentBar)>0 && DownTLsSignaled.Contains(DownTLs[DownTLs.Count-1])) {
							int idx = DownTLsSignaled.IndexOf(DownTLs[DownTLs.Count-1]);
//Print("idx: "+idx+" out of "+DownTLsSignaled.Count);
							if(idx>0) {
//								Print("Removing DownTLs["+idx+"] : "+DownTLsSignaled[idx].ToString());
								DownTLsSignaled.RemoveAt(idx);
							}
							TrendlineSignal[0]=(0);
							if(pShowArrows && ChartControl!=null) {
								RemoveDrawObject(string.Concat("UArrow",CurrentBar));
								if(ArrowNumbers.Count>0) ArrowNumbers.RemoveAt(ArrowNumbers.Count-1);
							}
						}
					}
				}
			}
//Draw.Text(this, "clsignal"+CurrentBar,CLreset[0].ToString(),0,High[0]+ATR(14)[0],Color.Green);
//Draw.Text(this, "signal"+CurrentBar,CLhit[0].ToString(),0,Low[0]-ATR(14)[0],Color.White);
			string ray = string.Empty;
			while(UpTLs.Count + DownTLs.Count > pMaximumLines) {
				int minABar = int.MaxValue;
//Print(Time[0].ToString()+"   Removing  Ups: "+UpTLs.Count+"  Downs: "+DownTLs.Count);
				if(UpTLs.Count>0 && DownTLs.Count>0) {
					if(UpTLs[0].X < DownTLs[0].X) {
						minABar = (int)Math.Min(minABar,UpTLs[0].X);
						ray = string.Concat("UpTL ",UpTLs[0].X,"-",UpTLs[0].Y);
						RemoveDrawObject(ray);
						if(pShowChannels) RemoveDrawObject(string.Concat("C",ray)); //"C" is for Channel
//Print("   Removed  "+ray);
						UpTLs.RemoveAt(0);
					} else {
						minABar = (int)Math.Min(minABar,DownTLs[0].X);
						ray = string.Concat("DownTL ",DownTLs[0].X,"-",DownTLs[0].Y);
						RemoveDrawObject(ray);
						if(pShowChannels) RemoveDrawObject(string.Concat("C",ray)); //"C" is for Channel
//Print("   Removed  "+ray);
						DownTLs.RemoveAt(0);
					}
				} else if(UpTLs.Count>0) {
					ray = string.Concat("UpTL ",UpTLs[0].X,"-",UpTLs[0].Y);
					RemoveDrawObject(ray);
					minABar = (int)Math.Min(minABar,UpTLs[0].X);
					if(pShowChannels) RemoveDrawObject(string.Concat("C",ray)); //"C" is for Channel
//Print("   Removed  "+ray);
					UpTLs.RemoveAt(0);
				} else {
					ray = string.Concat("DownTL ",DownTLs[0].X,"-",DownTLs[0].Y);
					RemoveDrawObject(ray);
					minABar = (int)Math.Min(minABar,DownTLs[0].X);
					if(pShowChannels) RemoveDrawObject(string.Concat("C",ray)); //"C" is for Channel
//Print("   Removed  "+ray);
					DownTLs.RemoveAt(0);
				}
				if(pRemoveHistoricalDots && MaximumLines>0 && minABar != int.MaxValue) {
					int maxABar = ABarAtNewTL[this.pMaximumLines-1];
//					try{Print(Time[0].ToString()+" count: "+ABarAtNewTL.Count+"  minABar: "+minABar+"   maxABar: "+maxABar+"  at "+Bars.GetTime(maxABar).ToString());}catch{}
					for(int abar = minABar; abar<maxABar; abar++) {
						int rbar = CurrentBar-abar;
						Values[0].Reset(rbar);
						Values[1].Reset(rbar);
						Values[2].Reset(rbar);
						Values[3].Reset(rbar);
					}
					while(ABarAtNewTL.Count>MaximumLines) ABarAtNewTL.RemoveAt(ABarAtNewTL.Count-1);
				}
			}
#if SPEECH_ENABLED
			#region Speech alerts
			if(!(State == State.Historical)) {
				if(InitAlertPrices || IsFirstTickOfBar) {
					InitAlertPrices = false;
					int i = 0;
					for(i = 0; i<UpRayPrices.Count; i++) RemoveDrawObject("U"+i.ToString());
					for(i = 0; i<DownRayPrices.Count; i++) RemoveDrawObject("D"+i.ToString());
					UpRayPrices.Clear();
					DownRayPrices.Clear();
					double run=0, rise=0, slope=0;
					i = 0;
					foreach(Point r in UpTLs) {
						run = r.Y-r.X;
						rise = Low[CurrentBar-r.Y] - Low[CurrentBar-r.X];
						slope = rise/run;
						UpRayPrices.Insert(0, slope * (CurrentBar-r.Y) + Low[CurrentBar-r.Y]);
//						DrawDot(this, "U"+i.ToString(), false, 0, UpRayPrices[0], Color.Yellow);
						i++;
					}
					i=0;
					foreach(Point r in DownTLs) {
						run = r.Y-r.X;
						rise = High[CurrentBar-r.Y] - High[CurrentBar-r.X];
						slope = rise/run;
						DownRayPrices.Insert(0, slope * (CurrentBar-r.Y) + High[CurrentBar-r.Y]);
//						DrawDot(this, "D"+i.ToString(), false, 0, DownRayPrices[0], Color.Pink);
						i++;
					}
				}
				if(pSpeakOnDownwardBreakout.Length>0) {
					foreach(double p in UpRayPrices) {
						if(Close[0] < p && PriorPrice >= p) {
//							BackBrush = Color.Pink;
							SayIt(pSpeakOnDownwardBreakout, p);
						}
					}
				}
				if(pSpeakOnUpwardBreakout.Length>0) {
					foreach(double p in DownRayPrices) {
						if(Close[0] > p && PriorPrice <= p) {
//							BackBrush = Color.Cyan;
							SayIt(pSpeakOnUpwardBreakout, p);
						}
					}
				}
			}
			#endregion
#endif
			PriorPrice = Close[0];
        }
		private void SendFullEmailMessage(string addr, string verboseMsg){
			string subj = string.Empty;
			if(pEmail_SubjectContent==AutoTrendChannel_MessageContent.Verbose)        subj = verboseMsg;
			else if(pEmail_SubjectContent==AutoTrendChannel_MessageContent.Shortened) subj = verboseMsg;
			string body = string.Empty;
			if(pEmail_BodyContent==AutoTrendChannel_MessageContent.Verbose)        body = string.Format("{0}{1}{1}Email auto-generated from NinjaTrader",verboseMsg,Environment.NewLine);
			else if(pEmail_BodyContent==AutoTrendChannel_MessageContent.Shortened) body = "Email auto-generated from NinjaTrader";
			SendMail(addr, subj, body);
		}

		private string SignalTypeToTxt(AutoTrendChannel_SignalType Type){
			if(Type==AutoTrendChannel_SignalType.Break) return "broken";
			else return "touched";
		}
//		private void PlaySoundVerified(string wav){
//			if(wav.ToUpper().StartsWith("SILENT")) return;
//			string name = AddSoundFolder(wav);
//			if(System.IO.File.Exists(name)) PlaySound(name);
//			else Log("Cannot play "+wav+" sound, it is not in the Sounds folder",LogLevel.Information);
//		}
//====================================================================================================

	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
		base.OnRender(chartControl, chartScale);
//		Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
//		Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
		int firstBarPainted = ChartBars.FromIndex;
		int lastBarPainted = ChartBars.ToIndex;

			//			base.Plot(graphics,bounds,minPrice,maxPrice);
			if(TimeOfText != DateTime.MaxValue && ChartControl!=null) {
				TimeSpan ts = new TimeSpan(Math.Abs(TimeOfText.Ticks-NinjaTrader.Core.Globals.Now.Ticks));
				if(ts.TotalSeconds>5) RemoveDrawObject("infomsg");
				else Draw.TextFixed(this, "infomsg", AlertMsg, TextPosition.Center, Brushes.White, new Gui.Tools.SimpleFont("Arial",16),Brushes.Green,Brushes.Green,10);
			}
		}
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds\"+wav);
		}
//====================================================================
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

				var list = new List<string>();//new string[filCustom.Length+1];
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

		#region Plots
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> UpTrendlinePrice
		{
			get { return Values[0]; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> DownTrendlinePrice
		{
			get { return Values[1]; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> UpChannellinePrice
		{
			get { return Values[2]; }
		}
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> DownChannellinePrice
		{
			get { return Values[3]; }
		}
		/// <summary>
		/// Gets the trade signal. 0 = no signal, 1 = Buy signal on break of down trend line, -1 = Sell signal on break of up trend line
		/// </summary>
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> TrendlineSignal
		{
			get { return Values[4]; }
		}
		/// <summary>
		/// Gets the trade signal. 0 = no signal, 1 = Buy signal on break of down trend line, -1 = Sell signal on break of up trend line
		/// </summary>
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> ChannellineSignal
		{
			get { return Values[5]; }
		}
		#endregion

		#region Properties

		#region Channel Settings
		private bool pShowChannels = true;
		[NinjaScriptProperty]
		[Description("Show nearest parallel channel line?")]
		[Category("Channel Settings")]
		public bool ShowChannels
		{
			get { return pShowChannels; }
			set { pShowChannels = value; }
		}
		private AutoTrendChannel_ChannelBasis pChannelBasis = AutoTrendChannel_ChannelBasis.HighsLows;
		[NinjaScriptProperty]
		[Description("Price basis for finding location of the channel line")]
		[Category("Channel Settings")]
		public AutoTrendChannel_ChannelBasis ChannelBasis
		{
			get { return pChannelBasis; }
			set { pChannelBasis = value; }
		}
		private DashStyleHelper channelDashStyle = DashStyleHelper.Dash;
		[Description("Channel line dash style")]
		[Category("Channel Settings")]
		public DashStyleHelper ChannelDashStyle
		{
			get { return channelDashStyle; }
			set { channelDashStyle = value; }
		}
		private int channelLineWidth = 2;
		[Description("Channel line width")]
		[Category("Channel Settings")]
		public int ChannelLineWidth
		{
			get { return channelLineWidth; }
			set { channelLineWidth = Math.Max(1, value); }
		}
		private Brush channelDownTrendBrush = Brushes.Magenta;
		[XmlIgnore()]
		[Description("Color of the down channel trend line.")]
//		[Category("Channel Settings")]
//		[Gui.Design.DisplayNameAttribute("Down trend")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down trend", GroupName = "Channel Settings")]
		public Brush ChannelDownTrendBrush
		{
			get { return channelDownTrendBrush; }
			set { channelDownTrendBrush = value; }
		}
		[Browsable(false)]
		public string ChannelDownTrendColorSerialize
		{get { return Serialize.BrushToString(channelDownTrendBrush); } set { ChannelDownTrendBrush = Serialize.StringToBrush(value); }}
		
		private Brush channelUpTrendBrush = Brushes.Lime;
		[XmlIgnore()]
		[Description("Color of the up channel trend line.")]
//		[Category("Channel Settings")]
//		[Gui.Design.DisplayNameAttribute("Up trend")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up trend", GroupName = "Channel Settings")]
		public Brush ChannelUpTrendBrush
		{
			get { return channelUpTrendBrush; }
			set { channelUpTrendBrush = value; }
		}
		[Browsable(false)]
		public string ChannelUpTrendColorSerialize
		{get { return Serialize.BrushToString(channelUpTrendBrush); } set { channelUpTrendBrush = Serialize.StringToBrush(value); }}

		#endregion

		#region Trendline Alerts
		private Brush pDownArrowBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of the down alert arrow")]
//		[Category("Trendline Alert")]
//		[Gui.Design.DisplayNameAttribute("Arrow Down")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Arrow Down", GroupName = "Trendline Alert")]
		public Brush DownArrowBrush
		{
			get { return pDownArrowBrush; }
			set { pDownArrowBrush = value; }
		}
		[Browsable(false)]
		public string DownArrowColorSerialize
		{get { return Serialize.BrushToString(pDownArrowBrush); } set { pDownArrowBrush = Serialize.StringToBrush(value); }}

		private Brush pUpArrowBrush = Brushes.Blue;
		[XmlIgnore()]
		[Description("Color of the Up alert arrow")]
//		[Category("Trendline Alert")]
//		[Gui.Design.DisplayNameAttribute("Arrow Up")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Arrow Up", GroupName = "Trendline Alert")]
		public Brush UpArrowBrush
		{
			get { return pUpArrowBrush; }
			set { pUpArrowBrush = value; }
		}
		[Browsable(false)]
		public string UpArrowColorSerialize
		{get { return Serialize.BrushToString(pUpArrowBrush); } set { pUpArrowBrush = Serialize.StringToBrush(value); }}

		
		private string pSoundFileTop = "Alert3.wav";
		[Description("Default WAV file to be played when descending (resistance) trendline gets hit")]
		[Category("Trendline Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SoundFileNameTopLine
		{
			get { return pSoundFileTop; }
			set { pSoundFileTop = value; }
		}
		private string pSoundFileBottom = "Alert2.wav";
		[Description("Default WAV file to be played when the ascending (support) trendline gets hit")]
		[Category("Trendline Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SoundFileNameBottomLine
		{
			get { return pSoundFileBottom; }
			set { pSoundFileBottom = value; }
		}

		private bool pSendEmailsTL = false;
		[Description("Send an email on each arrow signal?")]
		[Category("Trendline Alert")]
		public bool EmailEnabledTL
		{
			get { return pSendEmailsTL; }
			set { pSendEmailsTL = value; }
		}
		private string pEmailAddressTL = "";
		[Description("Enter a valid destination email address to receive an email on signals")]
		[Category("Trendline Alert")]
		public string EmailAddressTL
		{
			get { return pEmailAddressTL; }
			set { pEmailAddressTL = value; }
		}
		private int pMaxArrowsShown = 100;
		[Description("Max number of up/down arrows to print on chart, showing where trendlines signals were generated")]
		[Category("Trendline Alert")]
		public int MaxArrowsShown
		{
			get { return pMaxArrowsShown; }
			set { pMaxArrowsShown = Math.Max(0, value); }
		}
		private bool pEnableAudibleAlertNewTL = true;
		[Description("Generates an audible alert when a new trendline is formed")]
		[Category("Trendline Alert")]
		public bool NewTL_EnableAudibleAlert
		{
			get { return pEnableAudibleAlertNewTL; }
			set { pEnableAudibleAlertNewTL = value; }
		}

		private string pNewTrendlineWAV = "Alert4.wav";
		[Description("WAV file to play when a new trendline has formed, also NewTL_EnableAudibleAlert must be set to true")]
		[Category("Trendline Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string NewTL_WAV
		{
			get { return pNewTrendlineWAV; }
			set { pNewTrendlineWAV = value; }
		}

		private bool pEnableAudibleAlertTL = true;
		[Description("Generates a visual and audible alert on a trend line break")]
		[Category("Trendline Alert")]
		public bool EnableAudibleAlertTL
		{
			get { return pEnableAudibleAlertTL; }
			set { pEnableAudibleAlertTL = value; }
		}

		private bool pShowArrows = true;
		[NinjaScriptProperty]
		[Description("Show the visual arrow when trendline break (or touch) signal is first generated?")]
		[Category("Trendline Alert")]
		public bool ShowArrows
		{
			get { return pShowArrows; }
			set { pShowArrows = value; }
		}
		private int pTouchZoneSizeTL = 1;
		[NinjaScriptProperty]
		[Description("Size of 'Touch' zone, in ticks.  When price comes within this zone, the TrendlineSignal will be generated (only valid when 'SignalType' is set to 'Touch'")]
		[Category("Trendline Alert")]
		public int TouchZoneSizeTL
		{
			get { return pTouchZoneSizeTL; }
			set { pTouchZoneSizeTL = Math.Max(0, value); }
		}
		#endregion
		private string pAlertMsgLabel = "AutoTL & Channel";
		[Description("This label will go on all Email subject lines and (if you have Verbose=true) also on Alert Log messages")]
		[Category("Alert message settings")]
		public string AlertMsgLabel
		{
			get { return pAlertMsgLabel; }
			set { pAlertMsgLabel = value; }
		}
		private bool pVerboseAlertLogMessage = true;
		[Description("If you want a long message sent to the Alerts Log, set to 'true'")]
		[Category("Alert message settings")]
		public bool VerboseAlertLogMessage
		{
			get { return pVerboseAlertLogMessage; }
			set { pVerboseAlertLogMessage = value; }
		}
		private AutoTrendChannel_MessageContent pEmail_SubjectContent = AutoTrendChannel_MessageContent.Verbose;
		[Description("Is the subject line content verbose, shortened or blank?")]
		[Category("Alert message settings")]
		public AutoTrendChannel_MessageContent Email_SubjectContent
		{
			get { return pEmail_SubjectContent; }
			set { pEmail_SubjectContent = value; }
		}
		private AutoTrendChannel_MessageContent pEmail_BodyContent = AutoTrendChannel_MessageContent.Verbose;
		[Description("Is the email body content verbose, shortened or blank?")]
		[Category("Alert message settings")]
		public AutoTrendChannel_MessageContent Email_BodyContent
		{
			get { return pEmail_BodyContent; }
			set { pEmail_BodyContent = value; }
		}

		#region Channelline Alerts
		private Brush pDownDiamondBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Color of the down alert Diamond")]
//		[Category("Trendline Alert")]
//		[Gui.Design.DisplayNameAttribute("Diamond Down")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Diamond Down", GroupName = "Trendline Alert")]
		public Brush DownDiamondBrush
		{
			get { return pDownDiamondBrush; }
			set { pDownDiamondBrush = value; }
		}
		[Browsable(false)]
		public string DownDiamondColorSerialize
		{get { return Serialize.BrushToString(pDownDiamondBrush); } set { pDownDiamondBrush = Serialize.StringToBrush(value); }}

		private Brush pUpDiamondBrush = Brushes.Blue;
		[XmlIgnore()]
		[Description("Color of the Up alert Diamond")]
//		[Category("Trendline Alert")]
//		[Gui.Design.DisplayNameAttribute("Diamond Up")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Diamond Up", GroupName = "Trendline Alert")]
		public Brush UpDiamondBrush
		{
			get { return pUpDiamondBrush; }
			set { pUpDiamondBrush = value; }
		}
		[Browsable(false)]
		public string UpDiamondColorSerialize
		{get { return Serialize.BrushToString(pUpDiamondBrush); } set { pUpDiamondBrush = Serialize.StringToBrush(value); }}

		private string pSoundFileTopCL = "Alert3.wav";
		[Description("Default WAV file to be played when descending (support) channel line gets hit")]
		[Category("Channelline Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SoundFileNameTopCL
		{
			get { return pSoundFileTopCL; }
			set { pSoundFileTopCL = value; }
		}
		private string pSoundFileBottomCL = "Alert2.wav";
		[Description("Default WAV file to be played when the ascending (resistance) channel line gets hit")]
		[Category("Channelline Alert")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SoundFileNameBottomCL
		{
			get { return pSoundFileBottomCL; }
			set { pSoundFileBottomCL = value; }
		}

		private int pMaxDiamondsPerChannel = 5;
		[Description("Max number of diamond signals per channel?")]
		[Category("Channelline Alert")]
		public int MaxDiamondsPerChannel
		{
			get { return pMaxDiamondsPerChannel; }
			set { pMaxDiamondsPerChannel = value; }
		}
		private bool pConservativeEntryCL = true;
		[NinjaScriptProperty]
		[Description("Print diamond signals only when a bar closes in the predicted direction of the bounce off of the Channel Line?")]
		[Category("Channelline Alert")]
		public bool ConservativeEntry
		{
			get { return pConservativeEntryCL; }
			set { pConservativeEntryCL = value; }
		}
		private bool pSendEmailsCL = false;
		[Description("Send an email on each channel line diamond signal?")]
		[Category("Channelline Alert")]
		public bool EmailEnabledCL
		{
			get { return pSendEmailsCL; }
			set { pSendEmailsCL = value; }
		}
		private string pEmailAddressCL = "";
		[Description("Enter a valid destination email address to receive an email on signals")]
		[Category("Channelline Alert")]
		public string EmailAddressCL
		{
			get { return pEmailAddressCL; }
			set { pEmailAddressCL = value; }
		}
		private int pMaxDiamondsShown = 100;
		[Description("Max number of up/down diamonds to print on chart, showing where channelline signals were generated")]
		[Category("Channelline Alert")]
		public int MaxDiamondsShown
		{
			get { return pMaxDiamondsShown; }
			set { pMaxDiamondsShown = Math.Max(0, value); }
		}
		private bool pEnableAudibleAlertCL = true;
		[Description("Generates a visual and audible alert on a trend line break")]
		[Category("Channelline Alert")]
		public bool EnableAudibleAlertCL
		{
			get { return pEnableAudibleAlertCL; }
			set { pEnableAudibleAlertCL = value; }
		}

		private bool pShowDiamonds = false;
		[Description("Show the visual diamond when channel line signal is first generated?")]
		[Category("Channelline Alert")]
		public bool ShowDiamonds
		{
			get { return pShowDiamonds; }
			set { pShowDiamonds = value; }
		}
		private int pTouchZoneSizeCL = 1;
		[NinjaScriptProperty]
		[Description("Size of 'Touch' zone, in ticks.  When price comes within this distance of the channel line, the TrendlineSignal will be generated")]
		[Category("Channelline Alert")]
		public int TouchZoneSizeCL
		{
			get { return pTouchZoneSizeCL; }
			set { pTouchZoneSizeCL = Math.Max(0, value); }
		}
		#endregion

		private Brush pBackBrushForNewTL = Brushes.Transparent;
		[XmlIgnore()]
		[Description("Color of the background of the chart when a new trendline (or channel) forms, 'Transparent' turns off the colorizing")]
//		[Category("Trendline Settings")]
//		[Gui.Design.DisplayNameAttribute("NewTL BackBrush")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NewTL BackBrush", GroupName = "Trendline Settings")]
		public Brush BackBrushForNewTL
		{
			get { return pBackBrushForNewTL; }
			set { pBackBrushForNewTL = value; }
		}
		[Browsable(false)]
		public string BackBrushForNewTLSerialize
		{get { return Serialize.BrushToString(pBackBrushForNewTL); } set { pBackBrushForNewTL = Serialize.StringToBrush(value); }}

		private AutoTrendChannel_Type pTrendlineType = AutoTrendChannel_Type.Either;
		[NinjaScriptProperty]
		[Description("Trendline type")]
		[Category("Parameters")]
		public AutoTrendChannel_Type TrendlineType
		{
			get { return pTrendlineType; }
			set { pTrendlineType = value; }
		}
		private AutoTrendChannel_SignalType pSignalType = AutoTrendChannel_SignalType.Break;
		[NinjaScriptProperty]
		[Description("Type of trendline signal, a 'Touch' is when price comes within 'TouchZone' ticks of the trendline.  'Break' is when price exceeds the trendline")]
		[Category("Parameters")]
		public AutoTrendChannel_SignalType SignalType
		{
			get { return pSignalType; }
			set { pSignalType = value; }
		}

		private int strength = 5; // Default setting for Strength
		[NinjaScriptProperty]
		[Description("Number of bars required on each side swing pivot points used to connect the trend lines")]
		[Category("Parameters")]
		public int Strength
		{
			get { return strength; }
			set { strength = Math.Max(1, value); }
		}

		private int pMaximumLines = 2;
		[NinjaScriptProperty]
		[Description("Maximum number of recent trendlines to show")]
		[Category("Parameters")]
		public int MaximumLines
		{
			get { return pMaximumLines; }
			set { pMaximumLines = Math.Max(1, value); }
		}

		private int pMaxAlertsPerBar = 1;
//		[Description("Maximum number of audible and message alerts on each bar")]
//		[Category("Parameters")]
//		public int MaxAlertsPerBar
//		{
//			get { return pMaxAlertsPerBar; }
//			set { pMaxAlertsPerBar = Math.Max(0, value); }
//		}

		private int lineWidth = 2;
		[Description("Trend line width")]
		[Category("Trendline Settings")]
		public int LineWidth
		{
			get { return lineWidth; }
			set { lineWidth = Math.Max(1, value); }
		}
		private bool pRemoveHistoricalDots = true;
		[Description("When a trendline expires (is removed), do you want to remove the historical price dots as well?")]
		[Category("Plots")]
		public bool RemoveHistoricalDots
		{
			get { return pRemoveHistoricalDots; }
			set { pRemoveHistoricalDots = value; }
		}

		[XmlIgnore()]
		[Description("Color of the down trend line.")]
//		[Category("Trendline Settings")]
//		[Gui.Design.DisplayNameAttribute("Down trend")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down trend", GroupName = "Trendline Settings")]
		public Brush DownTrendBrush
		{
			get { return downTrendBrush; }
			set { downTrendBrush = value; }
		}
		[Browsable(false)]
		public string DownTrendColorSerialize
		{get { return Serialize.BrushToString(downTrendBrush); } set { downTrendBrush = Serialize.StringToBrush(value); }}
		
		[XmlIgnore()]
		[Description("Color of the up trend line.")]
//		[Category("Trendline Settings")]
//		[Gui.Design.DisplayNameAttribute("Up trend")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up trend", GroupName = "Trendline Settings")]
		public Brush UpTrendBrush
		{
			get { return upTrendBrush; }
			set { upTrendBrush = value; }
		}
		[Browsable(false)]
		public string UpTrendColorSerialize
		{get { return Serialize.BrushToString(upTrendBrush); } set { upTrendBrush = Serialize.StringToBrush(value); }}

		private DashStyleHelper lineDashStyle = DashStyleHelper.Solid;
		[Description("Dash style of the primary trendline")]
		[Category("Trendline Settings")]
		public DashStyleHelper LineDashStyle
		{
			get { return lineDashStyle; }
			set { lineDashStyle = value; }
		}

		#endregion
    }
}
public enum AutoTrendChannel_Type {
	Upward, Downward, Either
}
public enum AutoTrendChannel_SignalType {
	Touch,Break,TouchAndBreak
}
public enum AutoTrendChannel_ChannelBasis {
	HighsLows,Closes,Opens,OpensCloses
}
public enum AutoTrendChannel_MessageContent {
	Verbose,Shortened,Blank
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AutoTrendChannel[] cacheAutoTrendChannel;
		public AutoTrendChannel AutoTrendChannel(bool showChannels, AutoTrendChannel_ChannelBasis channelBasis, bool showArrows, int touchZoneSizeTL, bool conservativeEntry, int touchZoneSizeCL, AutoTrendChannel_Type trendlineType, AutoTrendChannel_SignalType signalType, int strength, int maximumLines)
		{
			return AutoTrendChannel(Input, showChannels, channelBasis, showArrows, touchZoneSizeTL, conservativeEntry, touchZoneSizeCL, trendlineType, signalType, strength, maximumLines);
		}

		public AutoTrendChannel AutoTrendChannel(ISeries<double> input, bool showChannels, AutoTrendChannel_ChannelBasis channelBasis, bool showArrows, int touchZoneSizeTL, bool conservativeEntry, int touchZoneSizeCL, AutoTrendChannel_Type trendlineType, AutoTrendChannel_SignalType signalType, int strength, int maximumLines)
		{
			if (cacheAutoTrendChannel != null)
				for (int idx = 0; idx < cacheAutoTrendChannel.Length; idx++)
					if (cacheAutoTrendChannel[idx] != null && cacheAutoTrendChannel[idx].ShowChannels == showChannels && cacheAutoTrendChannel[idx].ChannelBasis == channelBasis && cacheAutoTrendChannel[idx].ShowArrows == showArrows && cacheAutoTrendChannel[idx].TouchZoneSizeTL == touchZoneSizeTL && cacheAutoTrendChannel[idx].ConservativeEntry == conservativeEntry && cacheAutoTrendChannel[idx].TouchZoneSizeCL == touchZoneSizeCL && cacheAutoTrendChannel[idx].TrendlineType == trendlineType && cacheAutoTrendChannel[idx].SignalType == signalType && cacheAutoTrendChannel[idx].Strength == strength && cacheAutoTrendChannel[idx].MaximumLines == maximumLines && cacheAutoTrendChannel[idx].EqualsInput(input))
						return cacheAutoTrendChannel[idx];
			return CacheIndicator<AutoTrendChannel>(new AutoTrendChannel(){ ShowChannels = showChannels, ChannelBasis = channelBasis, ShowArrows = showArrows, TouchZoneSizeTL = touchZoneSizeTL, ConservativeEntry = conservativeEntry, TouchZoneSizeCL = touchZoneSizeCL, TrendlineType = trendlineType, SignalType = signalType, Strength = strength, MaximumLines = maximumLines }, input, ref cacheAutoTrendChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AutoTrendChannel AutoTrendChannel(bool showChannels, AutoTrendChannel_ChannelBasis channelBasis, bool showArrows, int touchZoneSizeTL, bool conservativeEntry, int touchZoneSizeCL, AutoTrendChannel_Type trendlineType, AutoTrendChannel_SignalType signalType, int strength, int maximumLines)
		{
			return indicator.AutoTrendChannel(Input, showChannels, channelBasis, showArrows, touchZoneSizeTL, conservativeEntry, touchZoneSizeCL, trendlineType, signalType, strength, maximumLines);
		}

		public Indicators.AutoTrendChannel AutoTrendChannel(ISeries<double> input , bool showChannels, AutoTrendChannel_ChannelBasis channelBasis, bool showArrows, int touchZoneSizeTL, bool conservativeEntry, int touchZoneSizeCL, AutoTrendChannel_Type trendlineType, AutoTrendChannel_SignalType signalType, int strength, int maximumLines)
		{
			return indicator.AutoTrendChannel(input, showChannels, channelBasis, showArrows, touchZoneSizeTL, conservativeEntry, touchZoneSizeCL, trendlineType, signalType, strength, maximumLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AutoTrendChannel AutoTrendChannel(bool showChannels, AutoTrendChannel_ChannelBasis channelBasis, bool showArrows, int touchZoneSizeTL, bool conservativeEntry, int touchZoneSizeCL, AutoTrendChannel_Type trendlineType, AutoTrendChannel_SignalType signalType, int strength, int maximumLines)
		{
			return indicator.AutoTrendChannel(Input, showChannels, channelBasis, showArrows, touchZoneSizeTL, conservativeEntry, touchZoneSizeCL, trendlineType, signalType, strength, maximumLines);
		}

		public Indicators.AutoTrendChannel AutoTrendChannel(ISeries<double> input , bool showChannels, AutoTrendChannel_ChannelBasis channelBasis, bool showArrows, int touchZoneSizeTL, bool conservativeEntry, int touchZoneSizeCL, AutoTrendChannel_Type trendlineType, AutoTrendChannel_SignalType signalType, int strength, int maximumLines)
		{
			return indicator.AutoTrendChannel(input, showChannels, channelBasis, showArrows, touchZoneSizeTL, conservativeEntry, touchZoneSizeCL, trendlineType, signalType, strength, maximumLines);
		}
	}
}

#endregion
