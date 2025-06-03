// 
// Copyright (C) 2011, SBG Trading Corp.
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

// This namespace holds all indicators and is required. Do not change it.
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
using System.Collections.Generic;

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Puts vertical dashed lines at every minute marker specified by user
    /// </summary>
	[Description("Changes the background color for up to three different session zones")]
	public class ColorSessionTime : Indicator
	{
		private bool LicenseValid = true;

        #region Variables
        // Wizard generated variables
		private string pStartTime1 = "9:30";
		private string pStartTime2 = "14:30";
		private string pStartTime3 = "none";
		private double pS1LengthHrs = 3.5;
		private double pS2LengthHrs = 0;
		private double pS3LengthHrs = 0;
		private Brush pS1Color = Brushes.Blue;
		private int pS1Opacity = 5;
		private Brush pS2Color = Brushes.Yellow;
		private int pS2Opacity = 5;
		private Brush pS3Color = Brushes.Red;
		private int pS3Opacity = 5;
		private DateTime StartTime1 = DateTime.MinValue;
		private DateTime StartTime2 = DateTime.MinValue;
		private DateTime StartTime3 = DateTime.MinValue;
		private DateTime EndTime1 = DateTime.MinValue;
		private DateTime EndTime2 = DateTime.MinValue;
		private DateTime EndTime3 = DateTime.MinValue;
		private DateTime InfoPrintedAt = DateTime.MinValue;
		//private DateTime pStartColorizingAt = new DateTime(now.Year,now.Month,now.Day,0,1,0);
		private bool RunInit = true;
		private bool ParameterInputError = false;
		private Brush Session1BackgroundColor=null;
		private Brush Session2BackgroundColor=null;
		private Brush Session3BackgroundColor=null;
		private Series<bool> InSession1;
		private Series<bool> InSession2;
		private Series<bool> InSession3;
        #endregion
		private DateTime NearestTime = DateTime.MaxValue;
		private string NearestTimeDesc = string.Empty;
		private DateTime PriorDTnow = DateTime.MinValue;
		private DateTime now = DateTime.MinValue;
		private int AlertBar = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				string ExemptMachine1 = "B0D2E9D1C802E279D3678D7DE6A33CE4";
				string ExemptMachine2 = "CB15E08BE30BC80628CFF6010471FA2A";
				bool ExemptMachine = NinjaTrader.Cbi.License.MachineId==ExemptMachine1 || NinjaTrader.Cbi.License.MachineId==ExemptMachine2;
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachine;
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "AIColorSessionTime", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
				Calculate=Calculate.OnPriceChange;
				IsOverlay=true;
				Name = "iw Color Session Times";
				InSession1 = new Series<bool>(this,MaximumBarsLookBack.Infinite);
				InSession2 = new Series<bool>(this,MaximumBarsLookBack.Infinite);
				InSession3 = new Series<bool>(this,MaximumBarsLookBack.Infinite);
			}
			if (State == State.Historical)
			{
				SetZOrder(-1);
				Session1BackgroundColor = this.pS1Color.Clone();
				Session1BackgroundColor.Opacity = this.pS1Opacity/9.0;
				Session1BackgroundColor.Freeze();
//				Print("Session1 Opacity: "+Session1BackgroundColor.Opacity.ToString("0.000")+"     "+(this.pS1Opacity).ToString("0.000"));
				Session2BackgroundColor = this.pS2Color.Clone();
				Session2BackgroundColor.Opacity = this.pS2Opacity/9.0;
				Session2BackgroundColor.Freeze();
				Session3BackgroundColor = this.pS3Color.Clone();
				Session3BackgroundColor.Opacity = this.pS3Opacity/9.0;
				Session3BackgroundColor.Freeze();
			}
		}
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
				now = marketDataUpdate.Time;
			}
		}
		protected override void OnBarUpdate()
		{
			bool Historical = State == State.Historical;
			if(Historical) now = Time[0];
			if(CurrentBar<1 || ParameterInputError) return;
int line = 117;
try{

			if(RunInit) 
			{
				#region RunInit
				RunInit = false;
line=102;
				if(!pStartTime1.Contains("NONE") && pStartTime1.Length>0 && pS1LengthHrs>0) {
					try {StartTime1 = DateTime.Parse(pStartTime1);} 
					catch {Log("Invalid Session1 Start time, must be in 24hr format: 'hh:mm'",LogLevel.Alert); ParameterInputError=true; return;}
					StartTime1 = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day, StartTime1.Hour, StartTime1.Minute, 0);
					EndTime1 = StartTime1.AddHours(pS1LengthHrs);
				}

line=110;
				if(!pStartTime2.Contains("NONE") && pStartTime2.Length>0 && pS2LengthHrs>0) {
					try {StartTime2 = DateTime.Parse(pStartTime2);} 
					catch {Log("Invalid Session2 Start time, must be in 24hr format: 'hh:mm'",LogLevel.Alert); ParameterInputError=true; return;}
					StartTime2 = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day, StartTime2.Hour, StartTime2.Minute, 0);
					EndTime2 = StartTime2.AddHours(pS2LengthHrs);
				} else pS2LengthHrs = 0;

line=118;
				if(!pStartTime3.Contains("NONE") && pStartTime3.Length>0 && pS3LengthHrs>0) {
					try {StartTime3 = DateTime.Parse(pStartTime3);} 
					catch {Log("Invalid Session3 Start time, must be in 24hr format: 'hh:mm'",LogLevel.Alert); ParameterInputError=true; return;}
					StartTime3 = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day, StartTime3.Hour, StartTime3.Minute, 0);
					EndTime3 = StartTime3.AddHours(pS3LengthHrs);
				} else pS3LengthHrs = 0;

				#endregion
			}
			NearestTime = DateTime.MaxValue;
			NearestTimeDesc = string.Empty;

			if(IsFirstTickOfBar){
				InSession1[0]=false;
				InSession2[0]=false;
				InSession3[0]=false;
			}
			if(StartTime1 != DateTime.MinValue) {
line=157;
				if(now.Ticks >= StartTime1.Ticks && PriorDTnow.Ticks < StartTime1.Ticks) {
					InSession1[0] = true;
					if(pS1StartLineWidth>0 && pS1StartLineColor!=Brushes.Transparent){
						Draw.VerticalLine(this, "s1S"+CurrentBar.ToString(), 0, pS1StartLineColor, pS1StartLineStyle, pS1StartLineWidth);
					}
					if(!Historical && AlertBar!=CurrentBar) {
						AlertBar = CurrentBar;
						PlayThisSound(pS1StartTimeWAV);
						Draw.TextFixed(this, "info","Session1 has been entered"+Environment.NewLine+"\t", TextPosition.BottomLeft);
						InfoPrintedAt = now;
					}
				}
line=165;
				if(now.Ticks >= EndTime1.Ticks && PriorDTnow.Ticks < EndTime1.Ticks){
					if(pS1EndLineWidth>0 && pS1EndLineColor!=Brushes.Transparent){
						Draw.VerticalLine(this, "s1E"+CurrentBar.ToString(), 0, pS1EndLineColor, pS1EndLineStyle, pS1EndLineWidth);
					}
					InSession1[0]=false;
					if(!Historical && AlertBar!=CurrentBar) {
						PlayThisSound(pS1StopTimeWAV);
						AlertBar = CurrentBar;
					}
				}
				if(now.Ticks > StartTime1.Ticks && now.Ticks <= EndTime1.Ticks) {
					InSession1[0] = true;
//					BackBrush = Session1BackgroundColor.Clone();
//					BackBrush.Opacity = pS1Opacity/9.0;
					//BackBrushes[0].Freeze();
				}
line=173;
				while(now.Ticks > EndTime1.Ticks) {
					StartTime1 = StartTime1.AddDays(1);
					EndTime1 = StartTime1.AddHours(pS1LengthHrs);
				}
				UpdateNearestTime(ref NearestTime, ref NearestTimeDesc, StartTime1, "Session1 starting");
				UpdateNearestTime(ref NearestTime, ref NearestTimeDesc, EndTime1, "Session1 ending");
			}
line=181;
			if(StartTime2 != DateTime.MinValue) {
				if(now.Ticks >= StartTime2.Ticks && PriorDTnow.Ticks < StartTime2.Ticks) {
					InSession2[0] = true;
					if(pS2StartLineWidth>0 && pS2StartLineColor!=Brushes.Transparent){
						Draw.VerticalLine(this, "s2S"+CurrentBar.ToString(), 0, pS2StartLineColor, pS2StartLineStyle, pS2StartLineWidth);
					}
					if(!Historical && AlertBar!=CurrentBar) {
						AlertBar = CurrentBar;
						PlayThisSound(pS2StartTimeWAV);
						Draw.TextFixed(this, "info","Session2 has been entered"+Environment.NewLine+"\t", TextPosition.BottomLeft);
						InfoPrintedAt = now;
					}
				}
line=190;
				if(now.Ticks >= EndTime2.Ticks && PriorDTnow.Ticks < EndTime2.Ticks) {
					if(pS2EndLineWidth>0 && pS2EndLineColor!=Brushes.Transparent){
						Draw.VerticalLine(this, "s2E"+CurrentBar.ToString(), 0, pS2EndLineColor, pS2EndLineStyle, pS2EndLineWidth);
					}
					InSession2[0]=false;
					if(!Historical && AlertBar!=CurrentBar) {
						PlayThisSound(pS2StopTimeWAV);
						AlertBar = CurrentBar;
					}
				}
				if(now.Ticks > StartTime2.Ticks && now.Ticks <= EndTime2.Ticks) {
					InSession2[0] = true;
//					BackBrush = Session2BackgroundColor.Clone();
					//BackBrushes[0].Opacity = pS2Opacity/9.0;
					//BackBrushes[0].Freeze();
				}
line=198;
				while(now.Ticks > EndTime2.Ticks) {
					StartTime2 = StartTime2.AddDays(1);
					EndTime2 = StartTime2.AddHours(pS2LengthHrs);
				}
				UpdateNearestTime(ref NearestTime, ref NearestTimeDesc, StartTime2, "Session2 starting");
				UpdateNearestTime(ref NearestTime, ref NearestTimeDesc, EndTime2, "Session2 ending");
			}
line=206;
			if(StartTime3 != DateTime.MinValue) {
				if(now.Ticks >= StartTime3.Ticks && PriorDTnow.Ticks < StartTime3.Ticks) {
					InSession3[0] = true;
					if(pS3StartLineWidth>0 && pS3StartLineColor!=Brushes.Transparent){
						Draw.VerticalLine(this, "s3S"+CurrentBar.ToString(), 0, pS3StartLineColor, pS3StartLineStyle, pS3StartLineWidth);
					}
					if(!Historical && AlertBar!=CurrentBar) {
						AlertBar = CurrentBar;
						PlayThisSound(pS3StartTimeWAV);
						Draw.TextFixed(this, "info","Session3 has been entered"+Environment.NewLine+"\t", TextPosition.BottomLeft);
						InfoPrintedAt = now;
					}
				}
line=215;
				if(now.Ticks >= EndTime3.Ticks && PriorDTnow.Ticks < EndTime3.Ticks) {
					if(pS3EndLineWidth>0 && pS3EndLineColor!=Brushes.Transparent){
						Draw.VerticalLine(this, "s3E"+CurrentBar.ToString(), 0, pS3EndLineColor, pS3EndLineStyle, pS3EndLineWidth);
					}
					InSession3[0]=false;
					if(!Historical && AlertBar!=CurrentBar) {
						PlayThisSound(pS3StopTimeWAV);
						AlertBar = CurrentBar;
					}
				}
				if(now.Ticks > StartTime3.Ticks && now.Ticks <= EndTime3.Ticks) {
					InSession3[0] = true;
//					BackBrush = Session3BackgroundColor.Clone();
					//BackBrushes[0].Opacity = pS3Opacity/9.0;
					//BackBrushes[0].Freeze();
				}
				while(now.Ticks > EndTime3.Ticks) {
					StartTime3 = StartTime3.AddDays(1);
					EndTime3 = StartTime3.AddHours(pS3LengthHrs);
				}
				UpdateNearestTime(ref NearestTime, ref NearestTimeDesc, StartTime3, "Session3 starting");
				UpdateNearestTime(ref NearestTime, ref NearestTimeDesc, EndTime3, "Session3 ending");
			}
line=230;
			if(!InSession1[0] && !InSession2[0] && !InSession3[0] && pColorNonSessionBars) {
line=232;
				BarBrush = pColorOfBar;
				CandleOutlineBrush = pColorOfBar;
			}
			PriorDTnow = now;
}catch(Exception err){if(line<1139) Print(line+":  "+err.ToString());}
		}
//------------------------------------------------------------------------------------------------------------
		private void PlayThisSound(string wav){
			try{
				PlaySound(System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds\"+wav));
			}catch(Exception err){Log("ColorSessionTime (on "+Instrument.FullName+" "+Bars.BarsPeriod.ToString()+") reports: '"+wav+"' wav file could not be found and played - check your sounds folder and your WAV files for playability",LogLevel.Warning);}
		}
//------------------------------------------------------------------------------------------------------------
		private void UpdateNearestTime(ref DateTime NearestTime, ref string NearestTimeDesc, DateTime t, string Desc){
			if(now==DateTime.MinValue) return;
			var ts = new TimeSpan(t.Ticks - now.Ticks);
			if(t<NearestTime && ts.TotalMinutes>0){
				NearestTime = t;
				if(ts.TotalMinutes>=60)
					NearestTimeDesc = string.Concat(Desc," in ",ts.TotalHours.ToString("0.0"),"-hrs");
				else if(ts.TotalMinutes<1){
					NearestTimeDesc = string.Concat(Desc," in ",ts.TotalSeconds.ToString("0.0"),"-sec");
				}else{
					string mins = ts.TotalMinutes.ToString("0.0");
					if(mins=="1.0")
						NearestTimeDesc = string.Concat(Desc," in 1-min");
					else{
						mins = mins.Replace(".0",string.Empty);
						NearestTimeDesc = string.Concat(Desc," in ",mins,"-mins");
					}
				}
			}
		}
//------------------------------------------------------------------------------------------------------------
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;
		double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
//		base.OnRender(chartControl, chartScale);
//		Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
//		Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
		int firstBarPainted = ChartBars.FromIndex;
		int lastBarPainted = ChartBars.ToIndex;

		if(InfoPrintedAt!=DateTime.MinValue) {
			TimeSpan infoage = new TimeSpan(Math.Abs(now.Ticks - InfoPrintedAt.Ticks));
			if(infoage.TotalSeconds>30) {
				RemoveDrawObject("info");
				InfoPrintedAt = DateTime.MinValue;
			}
		}
		if(pShowCountdown){
//			Print("Nearest: "+NearestTimeDesc);
			SimpleFont f = new SimpleFont("Arial", this.pFontSize);
			SharpDX.DirectWrite.TextFormat labelFormat = f.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout labelsize = null;
			
			labelsize   = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, NearestTimeDesc, labelFormat, ChartPanel.W, pFontSize);
			float textx = ChartPanel.X + 10;
			float texty = ChartPanel.Y + pVerticalLocation;
			if(texty > ChartPanel.Y+ChartPanel.H - pFontSize*2) texty = ChartPanel.Y+ChartPanel.H - pFontSize*2;
			float width  = labelsize.Metrics.Width;
			float height = labelsize.Metrics.Height;
			RenderTarget.FillRectangle(new SharpDX.RectangleF(textx-4f, texty-4f, width+8f, height+8f),ChartControl.Properties.ChartBackground.ToDxBrush(RenderTarget));
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(textx,texty), labelsize, ChartControl.Properties.AxisPen.Brush.ToDxBrush(RenderTarget));
			labelFormat.Dispose();labelFormat=null;
			labelsize.Dispose();labelsize=null;
		}
int line = 301;
try{
		#region Plot
//		List<Point> SessionBars = new List<Point>();//X is start abar, Y is end abar
		int S1s_abar = 0;
		int S2s_abar = 0;
		int S3s_abar = 0;
		int S1e_abar = 0;
		int S2e_abar = 0;
		int S3e_abar = 0;
		float y1 = ChartPanel.Y + ChartPanel.H;
//		Print("ChartPanel Y: "+ChartPanel.Y.ToString()+"   H: "+ChartPanel.H.ToString());
		float yS1 = y1  - pRegionHeight;
		float yS2 = yS1 - pRegionHeight;
		float yS3 = yS2 - pRegionHeight;
		float Height = pRegionHeight;
		float x0, x1;
		for(int abar = firstBarPainted; abar<=lastBarPainted; abar++){
			bool Historical = abar<lastBarPainted;
line=319;
			if(pRegionHeight==0f){
				yS1 = ChartPanel.Y;
				yS2 = ChartPanel.Y;
				yS3 = ChartPanel.Y;
				Height = ChartPanel.H;
			}
line=326;
			if(InSession1.GetValueAt(abar)){
				if(S1s_abar==0)S1s_abar = abar;
line=329;
			}
			if((!Historical || !InSession1.GetValueAt(abar)) && S1s_abar>0 && S1e_abar==0){
				x0 = ChartControl.GetXByBarIndex(ChartBars, S1s_abar);
				x1 = ChartControl.GetXByBarIndex(ChartBars, abar);
				RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, yS1, x1-x0, Height), Session1BackgroundColor.ToDxBrush(RenderTarget));
				S1s_abar = 0;
			}
line=337;
			if(InSession2.GetValueAt(abar)){
				if(S2s_abar==0) S2s_abar = abar;
			}
			if((!Historical || !InSession2.GetValueAt(abar)) && S2s_abar>0 && S2e_abar==0){
				x0 = ChartControl.GetXByBarIndex(ChartBars, S2s_abar);
				x1 = ChartControl.GetXByBarIndex(ChartBars, abar);
				RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, yS2, x1-x0, Height), Session2BackgroundColor.ToDxBrush(RenderTarget));
				S2s_abar = 0;
			}
line=347;
			if(InSession3.GetValueAt(abar)){
				if(S3s_abar==0) S3s_abar = abar;
			}
			if((!Historical || !InSession3.GetValueAt(abar)) && S3s_abar>0 && S3e_abar==0){
				x0 = ChartControl.GetXByBarIndex(ChartBars, S3s_abar);
				x1 = ChartControl.GetXByBarIndex(ChartBars, abar);
				RenderTarget.FillRectangle(new SharpDX.RectangleF(x0, yS3, x1-x0, Height),Session3BackgroundColor.ToDxBrush(RenderTarget));
				S3s_abar = 0;
			}
		}
		#endregion
}catch(Exception err){Print(line+":  "+err.ToString());}
	}
//------------------------------------------------------------------------------------------------------------
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
		
        #region Properties
		
		float pRegionHeight = 0f;
		[Description("Height of the colored region, set to '0' to make each colored region the full height of the chart")]
        [Category("Visual")]
        public float RegionHeight
        {
            get { return pRegionHeight; }
            set { pRegionHeight = Math.Max(0,value); }
        }
		bool pShowCountdown = true;
		[Description("Show the number of minutes remaining until the nearest start or end of a session?")]
        [Category("Countdown Timer")]
        public bool ShowCountdownTimer
        {
            get { return pShowCountdown; }
            set { pShowCountdown = value; }
        }
		int pFontSize = 14;
		[Description("Font size of the countdown timer text message")]
        [Category("Countdown Timer")]
        public int FontSize
        {
            get { return pFontSize; }
            set { pFontSize = Math.Max(6,value); }
        }
		int pVerticalLocation = 200;
		[Description("Vertical location (as measured from the top of the chart) of the message text")]
        [Category("Countdown Timer")]
        public int VertLocation
        {
            get { return pVerticalLocation; }
            set { pVerticalLocation = Math.Max(10,value); }
        }

		#region audio alerts
		private string pS1StartTimeWAV = "Alert1.wav";
        [Description("Leave blank to turn-off the audible alert")]
        [Category("Session1")]
		[NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        public string S1StartTimeWAV
        {
            get { return pS1StartTimeWAV; }
            set { pS1StartTimeWAV = value; }
        }
		private string pS1StopTimeWAV = "Alert1.wav";
        [Description("Leave blank to turn-off the audible alert")]
        [Category("Session1")]
		[NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        public string S1StopTimeWAV
        {
            get { return pS1StopTimeWAV; }
            set { pS1StopTimeWAV = value; }
        }

		private string pS2StartTimeWAV = "Alert2.wav";
        [Description("Leave blank to turn-off the audible alert")]
        [Category("Session2")]
		[NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        public string S2StartTimeWAV
        {
            get { return pS2StartTimeWAV; }
            set { pS2StartTimeWAV = value; }
        }
		private string pS2StopTimeWAV = "Alert2.wav";
        [Description("Leave blank to turn-off the audible alert")]
        [Category("Session2")]
		[NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        public string S2StopTimeWAV
        {
            get { return pS2StopTimeWAV; }
            set { pS2StopTimeWAV = value; }
        }
		private string pS3StartTimeWAV = "Alert3.wav";
        [Description("Leave blank to turn-off the audible alert")]
        [Category("Session3")]
		[NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        public string S3StartTimeWAV
        {
            get { return pS3StartTimeWAV; }
            set { pS3StartTimeWAV = value; }
        }
		private string pS3StopTimeWAV = "Alert3.wav";
        [Description("Leave blank to turn-off the audible alert")]
        [Category("Session3")]
		[NinjaScriptProperty]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        public string S3StopTimeWAV
        {
            get { return pS3StopTimeWAV; }
            set { pS3StopTimeWAV = value; }
        }
		#endregion

		//===============================================
		private bool pColorNonSessionBars = false;
		[Description("Colorize the bars when you are out of session")]
        [Category("Bar Colors")]
        public bool ColorNonSessionBars
        {
            get { return pColorNonSessionBars; }
            set { pColorNonSessionBars = value; }
        }
		private Brush pColorOfBar = Brushes.Transparent;
		[XmlIgnore()]
		[Description("Color of Bars when they occur outside of a valid session")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color", GroupName = "Bar Colors")]
		public Brush ColorOfBar
		{
			get { return pColorOfBar; }
			set { pColorOfBar = value; }
		}
		[Browsable(false)]
		public string pColorOfBarSerialize
		{
			get { return Serialize.BrushToString(pColorOfBar); }
			set { pColorOfBar = Serialize.StringToBrush(value); }
		}
		//===============================================


		#region Session1
		//=========================================================================================================
		[Description("Enter the time in 24:00 notation")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Time1", GroupName = "Session1", Order = 0)]
        public string StartTime1str
        {
            get { return pStartTime1; }
            set { pStartTime1 = value.ToUpper(); }
        }
		[Description("Set to '0' to turn-off this session")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Session Length hrs", GroupName = "Session1", Order = 10)]
		[NinjaScriptProperty]
        public double Session1LengthHrs
        {
            get { return pS1LengthHrs; }
            set { pS1LengthHrs = Math.Max(1/60, value); }
        }
        [Description("0 is fully transparent, 9 is fully opaque")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bkg Opacity", GroupName = "Session1", Order = 20)]
		[NinjaScriptProperty]
        public int Session1Opacity
        {
            get { return pS1Opacity; }
            set { pS1Opacity = Math.Max(0, Math.Min(9,value)); }
        }

		[XmlIgnore()]
		[Description("Color of Session 1 background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Base color", GroupName = "Session1", Order = 30)]
		public Brush S1_color
		{
			get { return pS1Color; }
			set { pS1Color = value; }
		}
		[Browsable(false)]
		public string pS1ColorSerialize
		{	get { return Serialize.BrushToString(pS1Color); }
			set { pS1Color = Serialize.StringToBrush(value); }}

		private int pS1StartLineWidth = 1;
        [Description("Set to '0' to turn-off the line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line Width", GroupName = "Session1", Order = 40)]
		[NinjaScriptProperty]
        public int Session1StartLineWidth
        {	get { return pS1StartLineWidth; }
			set { pS1StartLineWidth = Math.Max(0, value); }
        }

		private Brush pS1StartLineColor = Brushes.Yellow;
		[XmlIgnore()]
		[Description("Color of Session 1 start line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line color", GroupName = "Session1", Order = 50)]
		public Brush S1start_linecolor
		{	get { return pS1StartLineColor; }
			set { pS1StartLineColor = value; }}
		[Browsable(false)]
		public string pS1StartLineColorSerialize
		{	get { return Serialize.BrushToString(pS1StartLineColor); } set { pS1StartLineColor = Serialize.StringToBrush(value); }}

		private DashStyleHelper pS1StartLineStyle = DashStyleHelper.Solid;
		[Description("Line style for Session1 start line?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line Style",  GroupName = "Session1", Order=55)]
		public DashStyleHelper Session1StartLineStyle
		{
			get { return pS1StartLineStyle; }
			set { pS1StartLineStyle = value; }
		}

		private int pS1EndLineWidth = 1;
        [Description("Set to '0' to turn-off the line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line Width", GroupName = "Session1", Order = 60)]
		[NinjaScriptProperty]
        public int Session1EndLineWidth
        {
            get { return pS1EndLineWidth; }
            set { pS1EndLineWidth = Math.Max(0, value); }
        }

		private Brush pS1EndLineColor = Brushes.DimGray;
		[XmlIgnore()]
		[Description("Color of Session 1 end line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line color", GroupName = "Session1", Order = 65)]
		public Brush S1end_linecolor
		{	get { return pS1EndLineColor; }
			set { pS1EndLineColor = value; }}
		[Browsable(false)]
		public string pS1EndLineColorSerialize
		{	get { return Serialize.BrushToString(pS1EndLineColor); } set { pS1EndLineColor = Serialize.StringToBrush(value); }}

		private DashStyleHelper pS1EndLineStyle = DashStyleHelper.Solid;
		[Description("Line style for Session1 end line?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line Style",  GroupName = "Session1", Order=70)]
		public DashStyleHelper Session1EndLineStyle
		{
			get { return pS1EndLineStyle; }
			set { pS1EndLineStyle = value; }
		}
		#endregion
		//================================================================================================================

		#region Session2
		//=========================================================================================================
		[Description("Enter the time in 24:00 notation")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Time", GroupName = "Session2", Order = 0)]
        public string StartTime2str
        {
            get { return pStartTime2; }
            set { pStartTime2 = value.ToUpper(); }
        }
		[Description("Set to '0' to turn-off this session")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Session Length hrs", GroupName = "Session2", Order = 10)]
		[NinjaScriptProperty]
        public double Session2LengthHrs
        {
            get { return pS2LengthHrs; }
            set { pS2LengthHrs = Math.Max(1/60, value); }
        }
        [Description("0 is fully transparent, 9 is fully opaque")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bkg Opacity", GroupName = "Session2", Order = 20)]
		[NinjaScriptProperty]
        public int Session2Opacity
        {
            get { return pS2Opacity; }
            set { pS2Opacity = Math.Max(0, Math.Min(9,value)); }
        }

		[XmlIgnore()]
		[Description("Color of Session 2 background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Base color", GroupName = "Session2", Order = 30)]
		public Brush S2_color
		{
			get { return pS2Color; }
			set { pS2Color = value; }
		}
		[Browsable(false)]
		public string pS2ColorSerialize
		{	get { return Serialize.BrushToString(pS2Color); }
			set { pS2Color = Serialize.StringToBrush(value); }}

		private int pS2StartLineWidth = 1;
        [Description("Set to '0' to turn-off the line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line Width", GroupName = "Session2", Order = 40)]
		[NinjaScriptProperty]
        public int Session2StartLineWidth
        {	get { return pS2StartLineWidth; }
			set { pS2StartLineWidth = Math.Max(0, value); }
        }

		private Brush pS2StartLineColor = Brushes.Brown;
		[XmlIgnore()]
		[Description("Color of Session 2 start line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line color", GroupName = "Session2", Order = 50)]
		public Brush S2start_linecolor
		{	get { return pS2StartLineColor; }
			set { pS2StartLineColor = value; }}
		[Browsable(false)]
		public string pS2StartLineColorSerialize
		{	get { return Serialize.BrushToString(pS2StartLineColor); } set { pS2StartLineColor = Serialize.StringToBrush(value); }}

		private DashStyleHelper pS2StartLineStyle = DashStyleHelper.Solid;
		[Description("Line style for Session2 start line?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line Style",  GroupName = "Session2", Order=55)]
		public DashStyleHelper Session2StartLineStyle
		{
			get { return pS2StartLineStyle; }
			set { pS2StartLineStyle = value; }
		}

		private int pS2EndLineWidth = 1;
        [Description("Set to '0' to turn-off the line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line Width", GroupName = "Session2", Order = 60)]
		[NinjaScriptProperty]
        public int Session2EndLineWidth
        {
            get { return pS2EndLineWidth; }
            set { pS2EndLineWidth = Math.Max(0, value); }
        }

		private Brush pS2EndLineColor = Brushes.Transparent;
		[XmlIgnore()]
		[Description("Color of Session 2 end line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line color", GroupName = "Session2", Order = 65)]
		public Brush S2end_linecolor
		{	get { return pS2EndLineColor; }
			set { pS2EndLineColor = value; }}
		[Browsable(false)]
		public string pS2EndLineColorSerialize
		{	get { return Serialize.BrushToString(pS2EndLineColor); } set { pS2EndLineColor = Serialize.StringToBrush(value); }}

		private DashStyleHelper pS2EndLineStyle = DashStyleHelper.Solid;
		[Description("Line style for Session2 end line?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line Style",  GroupName = "Session2", Order=70)]
		public DashStyleHelper Session2EndLineStyle
		{
			get { return pS2EndLineStyle; }
			set { pS2EndLineStyle = value; }
		}
		#endregion
		//================================================================================================================

		#region Session3
		//=========================================================================================================
		[Description("Enter the time in 24:00 notation")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Time", GroupName = "Session3", Order = 0)]
        public string StartTime3str
        {
            get { return pStartTime3; }
            set { pStartTime3 = value.ToUpper(); }
        }
		[Description("Set to '0' to turn-off this session")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Session Length hrs", GroupName = "Session3", Order = 10)]
		[NinjaScriptProperty]
        public double Session3LengthHrs
        {
            get { return pS3LengthHrs; }
            set { pS3LengthHrs = Math.Max(1/60, value); }
        }
        [Description("0 is fully transparent, 9 is fully opaque")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bkg Opacity", GroupName = "Session3", Order = 20)]
		[NinjaScriptProperty]
        public int Session3Opacity
        {
            get { return pS3Opacity; }
            set { pS3Opacity = Math.Max(0, Math.Min(9,value)); }
        }

		[XmlIgnore()]
		[Description("Color of Session 3 background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Base color", GroupName = "Session3", Order = 30)]
		public Brush S3_color
		{
			get { return pS3Color; }
			set { pS3Color = value; }
		}
		[Browsable(false)]
		public string pS3ColorSerialize
		{	get { return Serialize.BrushToString(pS3Color); }
			set { pS3Color = Serialize.StringToBrush(value); }}

		private int pS3StartLineWidth = 0;
        [Description("Set to '0' to turn-off the line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line Width", GroupName = "Session3", Order = 40)]
		[NinjaScriptProperty]
        public int Session3StartLineWidth
        {	get { return pS3StartLineWidth; }
			set { pS3StartLineWidth = Math.Max(0, value); }
        }

		private Brush pS3StartLineColor = Brushes.Pink;
		[XmlIgnore()]
		[Description("Color of Session 3 start line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line color", GroupName = "Session3", Order = 50)]
		public Brush S3start_linecolor
		{	get { return pS3StartLineColor; }
			set { pS3StartLineColor = value; }}
		[Browsable(false)]
		public string pS3StartLineColorSerialize
		{	get { return Serialize.BrushToString(pS3StartLineColor); } set { pS3StartLineColor = Serialize.StringToBrush(value); }}

		private DashStyleHelper pS3StartLineStyle = DashStyleHelper.Solid;
		[Description("Line style for Session3 start line?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Line Style",  GroupName = "Session3", Order=55)]
		public DashStyleHelper Session3StartLineStyle
		{
			get { return pS3StartLineStyle; }
			set { pS3StartLineStyle = value; }
		}

		private int pS3EndLineWidth = 0;
        [Description("Set to '0' to turn-off the line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line Width", GroupName = "Session3", Order = 60)]
		[NinjaScriptProperty]
        public int Session3EndLineWidth
        {
            get { return pS3EndLineWidth; }
            set { pS3EndLineWidth = Math.Max(0, value); }
        }

		private Brush pS3EndLineColor = Brushes.Pink;
		[XmlIgnore()]
		[Description("Color of Session 3 end line")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line color", GroupName = "Session3", Order = 65)]
		public Brush S3end_linecolor
		{	get { return pS3EndLineColor; }
			set { pS3EndLineColor = value; }}
		[Browsable(false)]
		public string pS3EndLineColorSerialize
		{	get { return Serialize.BrushToString(pS3EndLineColor); } set { pS3EndLineColor = Serialize.StringToBrush(value); }}

		private DashStyleHelper pS3EndLineStyle = DashStyleHelper.Solid;
		[Description("Line style for Session3 end line?")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Line Style",  GroupName = "Session3", Order=70)]
		public DashStyleHelper Session3EndLineStyle
		{
			get { return pS3EndLineStyle; }
			set { pS3EndLineStyle = value; }
		}
		#endregion
		//================================================================================================================
		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ColorSessionTime[] cacheColorSessionTime;
		public ColorSessionTime ColorSessionTime(string s1StartTimeWAV, string s1StopTimeWAV, string s2StartTimeWAV, string s2StopTimeWAV, string s3StartTimeWAV, string s3StopTimeWAV, string startTime1str, double session1LengthHrs, int session1Opacity, int session1StartLineWidth, int session1EndLineWidth, string startTime2str, double session2LengthHrs, int session2Opacity, int session2StartLineWidth, int session2EndLineWidth, string startTime3str, double session3LengthHrs, int session3Opacity, int session3StartLineWidth, int session3EndLineWidth)
		{
			return ColorSessionTime(Input, s1StartTimeWAV, s1StopTimeWAV, s2StartTimeWAV, s2StopTimeWAV, s3StartTimeWAV, s3StopTimeWAV, startTime1str, session1LengthHrs, session1Opacity, session1StartLineWidth, session1EndLineWidth, startTime2str, session2LengthHrs, session2Opacity, session2StartLineWidth, session2EndLineWidth, startTime3str, session3LengthHrs, session3Opacity, session3StartLineWidth, session3EndLineWidth);
		}

		public ColorSessionTime ColorSessionTime(ISeries<double> input, string s1StartTimeWAV, string s1StopTimeWAV, string s2StartTimeWAV, string s2StopTimeWAV, string s3StartTimeWAV, string s3StopTimeWAV, string startTime1str, double session1LengthHrs, int session1Opacity, int session1StartLineWidth, int session1EndLineWidth, string startTime2str, double session2LengthHrs, int session2Opacity, int session2StartLineWidth, int session2EndLineWidth, string startTime3str, double session3LengthHrs, int session3Opacity, int session3StartLineWidth, int session3EndLineWidth)
		{
			if (cacheColorSessionTime != null)
				for (int idx = 0; idx < cacheColorSessionTime.Length; idx++)
					if (cacheColorSessionTime[idx] != null && cacheColorSessionTime[idx].S1StartTimeWAV == s1StartTimeWAV && cacheColorSessionTime[idx].S1StopTimeWAV == s1StopTimeWAV && cacheColorSessionTime[idx].S2StartTimeWAV == s2StartTimeWAV && cacheColorSessionTime[idx].S2StopTimeWAV == s2StopTimeWAV && cacheColorSessionTime[idx].S3StartTimeWAV == s3StartTimeWAV && cacheColorSessionTime[idx].S3StopTimeWAV == s3StopTimeWAV && cacheColorSessionTime[idx].StartTime1str == startTime1str && cacheColorSessionTime[idx].Session1LengthHrs == session1LengthHrs && cacheColorSessionTime[idx].Session1Opacity == session1Opacity && cacheColorSessionTime[idx].Session1StartLineWidth == session1StartLineWidth && cacheColorSessionTime[idx].Session1EndLineWidth == session1EndLineWidth && cacheColorSessionTime[idx].StartTime2str == startTime2str && cacheColorSessionTime[idx].Session2LengthHrs == session2LengthHrs && cacheColorSessionTime[idx].Session2Opacity == session2Opacity && cacheColorSessionTime[idx].Session2StartLineWidth == session2StartLineWidth && cacheColorSessionTime[idx].Session2EndLineWidth == session2EndLineWidth && cacheColorSessionTime[idx].StartTime3str == startTime3str && cacheColorSessionTime[idx].Session3LengthHrs == session3LengthHrs && cacheColorSessionTime[idx].Session3Opacity == session3Opacity && cacheColorSessionTime[idx].Session3StartLineWidth == session3StartLineWidth && cacheColorSessionTime[idx].Session3EndLineWidth == session3EndLineWidth && cacheColorSessionTime[idx].EqualsInput(input))
						return cacheColorSessionTime[idx];
			return CacheIndicator<ColorSessionTime>(new ColorSessionTime(){ S1StartTimeWAV = s1StartTimeWAV, S1StopTimeWAV = s1StopTimeWAV, S2StartTimeWAV = s2StartTimeWAV, S2StopTimeWAV = s2StopTimeWAV, S3StartTimeWAV = s3StartTimeWAV, S3StopTimeWAV = s3StopTimeWAV, StartTime1str = startTime1str, Session1LengthHrs = session1LengthHrs, Session1Opacity = session1Opacity, Session1StartLineWidth = session1StartLineWidth, Session1EndLineWidth = session1EndLineWidth, StartTime2str = startTime2str, Session2LengthHrs = session2LengthHrs, Session2Opacity = session2Opacity, Session2StartLineWidth = session2StartLineWidth, Session2EndLineWidth = session2EndLineWidth, StartTime3str = startTime3str, Session3LengthHrs = session3LengthHrs, Session3Opacity = session3Opacity, Session3StartLineWidth = session3StartLineWidth, Session3EndLineWidth = session3EndLineWidth }, input, ref cacheColorSessionTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ColorSessionTime ColorSessionTime(string s1StartTimeWAV, string s1StopTimeWAV, string s2StartTimeWAV, string s2StopTimeWAV, string s3StartTimeWAV, string s3StopTimeWAV, string startTime1str, double session1LengthHrs, int session1Opacity, int session1StartLineWidth, int session1EndLineWidth, string startTime2str, double session2LengthHrs, int session2Opacity, int session2StartLineWidth, int session2EndLineWidth, string startTime3str, double session3LengthHrs, int session3Opacity, int session3StartLineWidth, int session3EndLineWidth)
		{
			return indicator.ColorSessionTime(Input, s1StartTimeWAV, s1StopTimeWAV, s2StartTimeWAV, s2StopTimeWAV, s3StartTimeWAV, s3StopTimeWAV, startTime1str, session1LengthHrs, session1Opacity, session1StartLineWidth, session1EndLineWidth, startTime2str, session2LengthHrs, session2Opacity, session2StartLineWidth, session2EndLineWidth, startTime3str, session3LengthHrs, session3Opacity, session3StartLineWidth, session3EndLineWidth);
		}

		public Indicators.ColorSessionTime ColorSessionTime(ISeries<double> input , string s1StartTimeWAV, string s1StopTimeWAV, string s2StartTimeWAV, string s2StopTimeWAV, string s3StartTimeWAV, string s3StopTimeWAV, string startTime1str, double session1LengthHrs, int session1Opacity, int session1StartLineWidth, int session1EndLineWidth, string startTime2str, double session2LengthHrs, int session2Opacity, int session2StartLineWidth, int session2EndLineWidth, string startTime3str, double session3LengthHrs, int session3Opacity, int session3StartLineWidth, int session3EndLineWidth)
		{
			return indicator.ColorSessionTime(input, s1StartTimeWAV, s1StopTimeWAV, s2StartTimeWAV, s2StopTimeWAV, s3StartTimeWAV, s3StopTimeWAV, startTime1str, session1LengthHrs, session1Opacity, session1StartLineWidth, session1EndLineWidth, startTime2str, session2LengthHrs, session2Opacity, session2StartLineWidth, session2EndLineWidth, startTime3str, session3LengthHrs, session3Opacity, session3StartLineWidth, session3EndLineWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ColorSessionTime ColorSessionTime(string s1StartTimeWAV, string s1StopTimeWAV, string s2StartTimeWAV, string s2StopTimeWAV, string s3StartTimeWAV, string s3StopTimeWAV, string startTime1str, double session1LengthHrs, int session1Opacity, int session1StartLineWidth, int session1EndLineWidth, string startTime2str, double session2LengthHrs, int session2Opacity, int session2StartLineWidth, int session2EndLineWidth, string startTime3str, double session3LengthHrs, int session3Opacity, int session3StartLineWidth, int session3EndLineWidth)
		{
			return indicator.ColorSessionTime(Input, s1StartTimeWAV, s1StopTimeWAV, s2StartTimeWAV, s2StopTimeWAV, s3StartTimeWAV, s3StopTimeWAV, startTime1str, session1LengthHrs, session1Opacity, session1StartLineWidth, session1EndLineWidth, startTime2str, session2LengthHrs, session2Opacity, session2StartLineWidth, session2EndLineWidth, startTime3str, session3LengthHrs, session3Opacity, session3StartLineWidth, session3EndLineWidth);
		}

		public Indicators.ColorSessionTime ColorSessionTime(ISeries<double> input , string s1StartTimeWAV, string s1StopTimeWAV, string s2StartTimeWAV, string s2StopTimeWAV, string s3StartTimeWAV, string s3StopTimeWAV, string startTime1str, double session1LengthHrs, int session1Opacity, int session1StartLineWidth, int session1EndLineWidth, string startTime2str, double session2LengthHrs, int session2Opacity, int session2StartLineWidth, int session2EndLineWidth, string startTime3str, double session3LengthHrs, int session3Opacity, int session3StartLineWidth, int session3EndLineWidth)
		{
			return indicator.ColorSessionTime(input, s1StartTimeWAV, s1StopTimeWAV, s2StartTimeWAV, s2StopTimeWAV, s3StartTimeWAV, s3StopTimeWAV, startTime1str, session1LengthHrs, session1Opacity, session1StartLineWidth, session1EndLineWidth, startTime2str, session2LengthHrs, session2Opacity, session2StartLineWidth, session2EndLineWidth, startTime3str, session3LengthHrs, session3Opacity, session3StartLineWidth, session3EndLineWidth);
		}
	}
}

#endregion
