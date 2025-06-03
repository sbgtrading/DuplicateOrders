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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Plots the open, high, and low values from the session starting on the current day.
	/// </summary>
	public class CurrentDayOHLwithSizing : Indicator
	{
		private const int LONG = 1;
		private const int SHORT = -1;
		private DateTime			currentDate			=	Core.Globals.MinDate;
		private double				currentOpen			=	double.MinValue;
		private double				currentHigh			=	double.MinValue;
		private double				currentLow			=	double.MaxValue;
		private DateTime			lastDate			= 	Core.Globals.MinDate;
		private SessionIterator		sessionIterator;
		private KeltnerChannel kc;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionCurrentDayOHL;
				Name = "Current Day OHL with sizing";
				IsAutoScale					= false;
				DrawOnPricePanel			= false;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= false;
				ShowLow						= true;
				ShowHigh					= true;
				ShowOpen					= true;
				BarsRequiredToPlot			= 0;
				pRiskDollars = 500;
				pShowEntryLevels = true;

				AddPlot(new Stroke(Brushes.Goldenrod,	DashStyleHelper.Dash, 2), PlotStyle.Square, Custom.Resource.CurrentDayOHLOpen);
				AddPlot(new Stroke(Brushes.SeaGreen,	DashStyleHelper.Dash, 2), PlotStyle.Square, Custom.Resource.CurrentDayOHLHigh);
				AddPlot(new Stroke(Brushes.Red,			DashStyleHelper.Dash, 2), PlotStyle.Square, Custom.Resource.CurrentDayOHLLow);
				AddPlot(new Stroke(Brushes.Yellow,			DashStyleHelper.Dash, 2), PlotStyle.Hash, "BuyLvl");
				AddPlot(new Stroke(Brushes.Yellow,			DashStyleHelper.Dash, 2), PlotStyle.Hash, "SellLvl");
			}
			else if (State == State.Configure)
			{
				currentDate			= Core.Globals.MinDate;
				currentOpen			= double.MinValue;
				currentHigh			= double.MinValue;
				currentLow			= double.MaxValue;
				lastDate			= Core.Globals.MinDate;
			}
			else if (State == State.DataLoaded)
			{
				sessionIterator = new SessionIterator(Bars);
				kc = KeltnerChannel(1.5, 22);
				BarsAtStartup = BarsArray[0].Count;
			}
			else if (State == State.Historical)
			{
				if (!Bars.BarsType.IsIntraday)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", Custom.Resource.CurrentDayOHLError, TextPosition.BottomRight);
					Log(Custom.Resource.CurrentDayOHLError, LogLevel.Error);
				}
			}
		}

		double distL = 0;
		double distH = 1;
		int TradeDir = 0;
		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday) return;

			lastDate 		= currentDate;
			currentDate 	= sessionIterator.GetTradingDay(Time[0]);
			
			if (lastDate != currentDate || currentOpen == double.MinValue)
			{
				currentOpen		= Open[0];
				currentHigh		= High[0];
				currentLow		= Low[0];
			}

			currentHigh			= Math.Max(currentHigh, High[0]);
			currentLow			= Math.Min(currentLow, Low[0]);

			if (ShowOpen)
				CurrentOpen[0] = currentOpen;

			if (ShowHigh)
				CurrentHigh[0] = currentHigh;

			if (ShowLow)
				CurrentLow[0] = currentLow;
			distL = Math.Abs(Closes[0][0]-currentLow);
			distH = Math.Abs(Closes[0][0]-currentHigh);
			if(pShowEntryLevels && CurrentBar>1){
				if(CurrentHigh[0] > CurrentHigh[1]) TradeDir = SHORT;
				if(CurrentLow[0] < CurrentLow[1]) TradeDir = LONG;
				BuyLvl.Reset(0);
				SellLvl.Reset(0);
				if(TradeDir == LONG){
					BuyLvl[0] = Instrument.MasterInstrument.RoundToTickSize(kc.Upper[0]);
					if(BuyLvl.IsValidDataPoint(1) && High[0] > BuyLvl[1]){
						myAlert(string.Format("CDOHL Buy{0}",CurrentBar.ToString()), Priority.High, "BUY breakup", AddSoundFolder(pBuyEntryWAV), 10, Brushes.Black,Brushes.Lime);
						TradeDir = 0;
					}
				}else if(TradeDir == SHORT){
					SellLvl[0] = Instrument.MasterInstrument.RoundToTickSize(kc.Lower[0]);
					if(SellLvl.IsValidDataPoint(1) && Low[0] < SellLvl[1]){
						myAlert(string.Format("CDOHL Sell{0}",CurrentBar.ToString()), Priority.High, "SELL breakdown", AddSoundFolder(pSellEntryWAV), 10, Brushes.Black,Brushes.Magenta);
						TradeDir = 0;
					}
				}
			}
		}
		int BarsAtStartup = 0;
		private void myAlert(string id, Priority prio, string msg, string wav, int rearmSeconds, System.Windows.Media.SolidColorBrush bkgBrush, System.Windows.Media.SolidColorBrush foregroundBrush){
			if(CurrentBar>2) BackBrushes[2] = foregroundBrush;
			if(CurrentBar > BarsAtStartup && !wav.Contains("none"))
				Alert(id,prio,msg,wav,rearmSeconds,bkgBrush, foregroundBrush);
			//printDebug(string.Format("Alert: {0}   wav: {1}",msg,wav));
		}
//==============================================================================================
		SharpDX.Direct2D1.Brush LabelTextBrushDX;
        public override void OnRenderTargetChanged()
        {
			if(LabelTextBrushDX!=null   && !LabelTextBrushDX.IsDisposed)   {LabelTextBrushDX.Dispose();   LabelTextBrushDX=null;}
			if(RenderTarget!=null) LabelTextBrushDX = Brushes.Yellow.ToDxBrush(RenderTarget);
		}
		NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 14);
		SharpDX.RectangleF rect;
		SharpDX.DirectWrite.TextLayout textLayout = null;
//		SharpDX.Direct2D1.Factory factory = null;
		SharpDX.DirectWrite.TextFormat textFormat = null;

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			base.OnRender(chartControl, chartScale);
			try{
			textFormat = new SharpDX.DirectWrite.TextFormat(
				Core.Globals.DirectWriteFactory,
				font.FamilySerialize,
				font.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
				font.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
				SharpDX.DirectWrite.FontStretch.Normal,
				(float)font.Size) {	TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap };

			double size = pRiskDollars / Math.Min(distL,distH) / Instrument.MasterInstrument.PointValue;
			double pts = Math.Min(distL,distH);
			textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, string.Format("{0:0}-ticks {1} = {2}-ct",pts/TickSize,(pts*Instrument.MasterInstrument.PointValue).ToString("$0"), size.ToString("0.0")), textFormat, Convert.ToSingle(font.Size), float.MaxValue);
			float y = 0;
			if(distL < distH)
				y = chartScale.GetYByValue(currentLow);
			else
				y = chartScale.GetYByValue(currentHigh);

			rect = new SharpDX.RectangleF(2f, y, textLayout.Metrics.Width+10f, Convert.ToSingle(font.Size)+3f);
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(rect.X+1f, rect.Y+1f), textLayout, LabelTextBrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			}catch(Exception ee){Print(ee.ToString());}

			if(textLayout != null) textLayout.Dispose();
			if(textFormat != null) textFormat.Dispose();
		}
		#region Plots
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> CurrentLow
		{
			get { return Values[2]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> BuyLvl
		{
			get { return Values[3]; }
		}

		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		[XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
		public Series<double> SellLvl
		{
			get { return Values[4]; }
		}
		#endregion
		#region -- Parameters --
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
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
				list.Add("<inst>_BuyEntry.wav");
				list.Add("<inst>_SellEntry.wav");
				list.Add("<inst>_BuyBreakout.wav");
				list.Add("<inst>_SellBreakout.wav");
				list.Add("<inst>_Divergence.wav");
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
			if(wav.Trim().Length==0) return string.Empty;
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(wav.Replace("<inst>",Instruments[0].MasterInstrument.Name)," "));
//			Print(Times[0][0].ToString()+"  DivergenceSpotter Playing sound: "+wav);
			return wav;
		}
		private string StripOutIllegalCharacters(string name, string ReplacementString){
			#region strip
			char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
			string invalids = string.Empty;
			foreach(char ch in invalidPathChars){
				invalids += ch.ToString();
			}
//			Print("Invalid chars: '"+invalids+"'");
			string result = string.Empty;
			for(int c=0; c<name.Length; c++) {
				if(!invalids.Contains(name[c].ToString())) result += name[c];
				else result += ReplacementString;
			}
			return result;
			#endregion
		}
//====================================================================
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowHigh", GroupName = "NinjaScriptParameters", Order = 10)]
		public bool ShowHigh
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowLow", GroupName = "NinjaScriptParameters", Order = 20)]
		public bool ShowLow
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowOpen", GroupName = "NinjaScriptParameters", Order = 30)]
		public bool ShowOpen
		{ get; set; }

		[Display(Order = 40, Name = "ShowOpen", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public double pRiskDollars
		{ get; set; }


		#region -- Audible Alerts --
		[Display(Order = 10, Name = "Show EntryLvls", GroupName = "Audible Alerts", ResourceType = typeof(Custom.Resource))]
		public bool pShowEntryLevels
		{ get; set; }
		private string pBuyEntryWAV = "<inst>_BuyEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=20, Name="BUY entry", GroupName="Audible Alerts", Description="Sound file when BUY is found")]
        public string BuyEntryWAV
        {
            get { return pBuyEntryWAV; }
            set { pBuyEntryWAV = value; }
        }
		private string pSellEntryWAV = "<inst>_SellEntry.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=30, Name="SELL entry", GroupName="Audible Alerts", Description="Sound file when SELL is found")]
        public string SellEntryWAV
        {
            get { return pSellEntryWAV; }
            set { pSellEntryWAV = value; }
        }
		#endregion
		#endregion
		
		public override string FormatPriceMarker(double price)
		{
			return Instrument.MasterInstrument.FormatPrice(price);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CurrentDayOHLwithSizing[] cacheCurrentDayOHLwithSizing;
		public CurrentDayOHLwithSizing CurrentDayOHLwithSizing()
		{
			return CurrentDayOHLwithSizing(Input);
		}

		public CurrentDayOHLwithSizing CurrentDayOHLwithSizing(ISeries<double> input)
		{
			if (cacheCurrentDayOHLwithSizing != null)
				for (int idx = 0; idx < cacheCurrentDayOHLwithSizing.Length; idx++)
					if (cacheCurrentDayOHLwithSizing[idx] != null &&  cacheCurrentDayOHLwithSizing[idx].EqualsInput(input))
						return cacheCurrentDayOHLwithSizing[idx];
			return CacheIndicator<CurrentDayOHLwithSizing>(new CurrentDayOHLwithSizing(), input, ref cacheCurrentDayOHLwithSizing);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CurrentDayOHLwithSizing CurrentDayOHLwithSizing()
		{
			return indicator.CurrentDayOHLwithSizing(Input);
		}

		public Indicators.CurrentDayOHLwithSizing CurrentDayOHLwithSizing(ISeries<double> input )
		{
			return indicator.CurrentDayOHLwithSizing(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CurrentDayOHLwithSizing CurrentDayOHLwithSizing()
		{
			return indicator.CurrentDayOHLwithSizing(Input);
		}

		public Indicators.CurrentDayOHLwithSizing CurrentDayOHLwithSizing(ISeries<double> input )
		{
			return indicator.CurrentDayOHLwithSizing(input);
		}
	}
}

#endregion
