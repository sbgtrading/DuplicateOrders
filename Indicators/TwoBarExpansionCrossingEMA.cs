
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
//using NinjaTrader.NinjaScript.Indicators.ARC.Sup;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Gui.CategoryOrder("Input Parameters", 10)]
	[Gui.CategoryOrder("Signals", 20)]
	[Gui.CategoryOrder("Sound Alerts", 30)]
	[Gui.CategoryOrder("Up/Down Text", 40)]
	[Gui.CategoryOrder("Ticks Text", 45)]
	[Gui.CategoryOrder("Visual", 50)]
	[Gui.CategoryOrder("Plots", 60)]
//	[Gui.CategoryOrder("Paint Bars", 70)]
	public class TwoBarExpansionCrossingEMA : Indicator
	{

		private int AlertABar = 0;
		private EMA ma;
		protected override void OnStateChange()
		{
			#region OnStateChange
			if (State == State.SetDefaults)
			{
				Name						= "Two Bar Expansion across EMA";
				Calculate					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				ArePlotsConfigurable		= false;


				AddPlot(new Stroke(Brushes.Gray, 1),  PlotStyle.Line, "EMA");	
				AddPlot(new Stroke(Brushes.Green, 4), PlotStyle.Dot, "Up Signal");
				AddPlot(new Stroke(Brushes.Red, 4),   PlotStyle.Dot, "Down Signal");

				pUpWAV		= "Silent";
				pDownWAV	= "Silent";
				pUpBkgBrush = Brushes.Green;
				pUpBkgOpacity = 10;
				pDownBkgBrush = Brushes.Red;
				pDownBkgOpacity = 10;

				pShowCustomText = false;
				pFontCustomText = new SimpleFont("Arial",8);
				pUpText = "";
				pDownText = "";

				pShowTicksOnBar = false;
				pFontTicksCount = new SimpleFont("Arial",12);
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				pathUpWAV = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds", pUpWAV);
				pathDownWAV = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds", pDownWAV);

				upBkgBrush = pUpBkgBrush.Clone();
				upBkgBrush.Opacity = pUpBkgOpacity/100f;
				upBkgBrush.Freeze();
				downBkgBrush = pDownBkgBrush.Clone();
				downBkgBrush.Opacity = pDownBkgOpacity/100f;
				downBkgBrush.Freeze();
				
				ma = EMA(this.pEMAperiod);
			}	
			else if (State == State.Historical)
			{
			}
			#endregion
		}

		private string pathUpWAV = "";
		private string pathDownWAV = "";
		private int SIGNAL = 0;
		private int NONE = 0;
		private int UP = 1;
		private int DOWN = -1;
		private Brush upBkgBrush = Brushes.Transparent;
		private Brush downBkgBrush = Brushes.Transparent;
		protected override void OnBarUpdate()
        {
			if(CurrentBar<4) return;
			TheMA[0] = ma[0];

			double tks = Math.Round(Math.Abs(Close[0]-Open[0]) / TickSize,0);
			bool c1 = Math.Max(1,Math.Abs(Close[1]-Open[1])/TickSize) * this.pMultiplier < tks;
			bool c2 = (Close[1] > ma[1] && Close[0] < ma[0]);
			SIGNAL = NONE;
			if(c1 && c2) SIGNAL = DOWN;
			c2 = (Close[1] < ma[1] && Close[0] > ma[0]);
			if(c1 && c2) SIGNAL = UP;

			BackBrushes[0] = null;
			if(ChartControl!=null && c1){
				if(Close[0] < Close[1] && pDownBkgOpacity>0) BackBrushes[0] = downBkgBrush;
				else if(Close[0] > Close[1] && pUpBkgOpacity>0) BackBrushes[0] = upBkgBrush;
			}

			UpSignal.Reset(0);
			DownSignal.Reset(0);
			if(pShowTicksOnBar)	RemoveDrawObject(string.Format("Ticks {0}",CurrentBar));
			if(pShowCustomText)	RemoveDrawObject(string.Format("CustomTxt {0}",CurrentBar));
			if(SIGNAL == UP){
				UpSignal[0] = Lows[0][0]-TickSize;
				if(ChartControl!=null){
					if(pShowTicksOnBar) Draw.Text(this, string.Format("Ticks {0}",CurrentBar),false, string.Format("{0}-tks",tks), 1, Medians[0][0], 0, Brushes.White, pFontTicksCount, TextAlignment.Right, Brushes.Black, Brushes.Black,100);
					if(pShowCustomText) Draw.Text(this, string.Format("CustomTxt {0}",CurrentBar),false, pUpText, 1, Closes[0][0], 0, Brushes.White, pFontCustomText, TextAlignment.Right, Brushes.Black, Brushes.Black,100);
				}
			}
			else if(SIGNAL == DOWN){
				DownSignal[0] = Highs[0][0]+TickSize;
				if(ChartControl!=null){
					if(pShowTicksOnBar) Draw.Text(this, string.Format("Ticks {0}",CurrentBar),false, string.Format("{0}-tks",tks), 1, Medians[0][0], 0, Brushes.White, pFontTicksCount, TextAlignment.Right, Brushes.Black, Brushes.Black,100);
					if(pShowCustomText) Draw.Text(this, string.Format("CustomTxt {0}",CurrentBar),false, pDownText, 1, Closes[0][0], 0, Brushes.White, pFontCustomText, TextAlignment.Right, Brushes.Black, Brushes.Black,100);
				}
			}
			if (soundAlerts && State == State.Realtime && AlertABar!=CurrentBar){
				if(Calculate == Calculate.OnBarClose)// || reverseIntraBar))
				{
					if(SIGNAL == UP && !pUpWAV.Contains("Silent"))		
					{
						Alert("New_Uptrend", Priority.Medium,"New Uptrend", pathUpWAV, rearmTime, Brushes.Lime, Brushes.White);
						AlertABar = CurrentBar;
					}
					else if(SIGNAL == DOWN && !pDownWAV.Contains("Silent"))
					{
						Alert("New_Downtrend", Priority.Medium,"New Downtrend", pathDownWAV, rearmTime, Brushes.Red, Brushes.White);
						AlertABar = CurrentBar;
					}
				}				
			}	
		}

		#region Plots

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TheMA
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpSignal
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DownSignal
		{
			get { return Values[2]; }
		}

		#endregion

		#region Properties
		private int pEMAperiod = 14;
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "EMA period", Description = "Sets the lookback period for the moving average", GroupName = "Input Parameters", Order = 10, ResourceType = typeof(Custom.Resource))]
		public int EMAperiod
		{
            get { return pEMAperiod; }
            set { pEMAperiod = value; }
		}

		private double pMultiplier = 2.5;
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(Name = "BarSize multiplier", Description = "Sets the multiplier for the 2nd bar", GroupName = "Input Parameters", Order = 20, ResourceType = typeof(Custom.Resource))]
		public double Multiplier
		{
            get { return pMultiplier; }
            set { pMultiplier = value; }
		}

		[XmlIgnore]
		[Display(Order = 11, Name = "Up Bkg color", Description = "", GroupName = "Signals", ResourceType = typeof(Custom.Resource))]
		public Brush pUpBkgBrush {get;set;}
				[Browsable(false)]
				public string pUpBkgBrushSerializable{get { return Serialize.BrushToString(pUpBkgBrush); }set { pUpBkgBrush = Serialize.StringToBrush(value); }}					
		[Range(0, 100)]
		[Display(Order = 12, Name = "Up Bkg opacity", Description = "", GroupName = "Signals", ResourceType = typeof(Custom.Resource))]
		public int pUpBkgOpacity
		{get;set;}


		[XmlIgnore]
		[Display(Order = 21, Name = "Down Bkg color", Description = "", GroupName = "Signals", ResourceType = typeof(Custom.Resource))]
		public Brush pDownBkgBrush {get;set;}
				[Browsable(false)]
				public string pDownBkgBrushSerializable{get { return Serialize.BrushToString(pDownBkgBrush); }set { pDownBkgBrush = Serialize.StringToBrush(value); }}					
		[Range(0, 100)]
		[Display(Order = 22, Name = "Down Bkg opacity", Description = "", GroupName = "Signals", ResourceType = typeof(Custom.Resource))]
		public int pDownBkgOpacity
		{get;set;}

		[Display(Order = 5, Name = "Show Custom Text?", Description = "Enable/disable custom text", GroupName = "Up/Down Text", ResourceType = typeof(Custom.Resource))]
		public bool pShowCustomText
		{get;set;}
		[Display(Order = 10, Name = "Text Font", Description = "", GroupName = "Up/Down Text", ResourceType = typeof(Custom.Resource))]
		public SimpleFont pFontCustomText
		{get;set;}
		[Display(Order = 20, Name = "Up Text", Description = "Custom text to print on an UP signal", GroupName = "Up/Down Text", ResourceType = typeof(Custom.Resource))]
		public string pUpText
		{get;set;}
		[Display(Order = 30, Name = "Down Text", Description = "Custom text to print on an DOWN signal", GroupName = "Up/Down Text", ResourceType = typeof(Custom.Resource))]
		public string pDownText
		{get;set;}

		[Display(Order = 5, Name = "Show Custom Text?", Description = "Enable/disable custom text", GroupName = "Ticks Text", ResourceType = typeof(Custom.Resource))]
		public bool pShowTicksOnBar
		{get;set;}
		[Display(Order = 10, Name = "Text Font", Description = "", GroupName = "Ticks Text", ResourceType = typeof(Custom.Resource))]
		public SimpleFont pFontTicksCount
		{get;set;}

		private bool soundAlerts = false;
		[Display(Name = "Enable Sound alerts?", GroupName = "Sound Alerts", Order = 10, ResourceType = typeof(Custom.Resource))]
        public bool SoundAlerts
        {
            get { return soundAlerts; }
            set { soundAlerts = value; }
        }
		
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Uptrend WAV", Description = "Sound file for confirmed new uptrend", GroupName = "Sound Alerts", Order = 20, ResourceType = typeof(Custom.Resource))]
        public string pUpWAV { get; set; }

        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Downtrend WAV", Description = "Sound file for confirmed new downtrend", GroupName = "Sound Alerts", Order = 30, ResourceType = typeof(Custom.Resource))]
        public string pDownWAV { get; set; }

		private int rearmTime = 10;
		[Range(1, int.MaxValue)]
		[Display(Name = "Rearm time", Description = "Rearm time for alerts in seconds", GroupName = "Sound Alerts", Order = 40, ResourceType = typeof(Custom.Resource))]
		public int RearmTime
		{
            get { return rearmTime; }
            set { rearmTime = value; }
		}
		
		#endregion

		#region Miscellaneous
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

//		public override string FormatPriceMarker(double price)
//		{
//			if(indicatorIsOnPricePanel)
//				return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
//			else
//				return base.FormatPriceMarker(price);
//		}
		
//		private bool IsConnected()
//        {
//			if ( Bars != null && Bars.Instrument.GetMarketDataConnection().PriceStatus == NinjaTrader.Cbi.ConnectionStatus.Connected
//					&& sessionIterator.IsInSession(Now, true, true))
//				return true;
//			else
//            	return false;
//        }
		
		private DateTime Now
		{
          get 
			{ 
				DateTime now = (Bars.Instrument.GetMarketDataConnection().Options.Provider == NinjaTrader.Cbi.Provider.Playback ? Bars.Instrument.GetMarketDataConnection().Now : DateTime.Now); 

				if (now.Millisecond > 0)
					now = NinjaTrader.Core.Globals.MinDate.AddSeconds((long) System.Math.Floor(now.Subtract(NinjaTrader.Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TwoBarExpansionCrossingEMA[] cacheTwoBarExpansionCrossingEMA;
		public TwoBarExpansionCrossingEMA TwoBarExpansionCrossingEMA(int eMAperiod, double multiplier)
		{
			return TwoBarExpansionCrossingEMA(Input, eMAperiod, multiplier);
		}

		public TwoBarExpansionCrossingEMA TwoBarExpansionCrossingEMA(ISeries<double> input, int eMAperiod, double multiplier)
		{
			if (cacheTwoBarExpansionCrossingEMA != null)
				for (int idx = 0; idx < cacheTwoBarExpansionCrossingEMA.Length; idx++)
					if (cacheTwoBarExpansionCrossingEMA[idx] != null && cacheTwoBarExpansionCrossingEMA[idx].EMAperiod == eMAperiod && cacheTwoBarExpansionCrossingEMA[idx].Multiplier == multiplier && cacheTwoBarExpansionCrossingEMA[idx].EqualsInput(input))
						return cacheTwoBarExpansionCrossingEMA[idx];
			return CacheIndicator<TwoBarExpansionCrossingEMA>(new TwoBarExpansionCrossingEMA(){ EMAperiod = eMAperiod, Multiplier = multiplier }, input, ref cacheTwoBarExpansionCrossingEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TwoBarExpansionCrossingEMA TwoBarExpansionCrossingEMA(int eMAperiod, double multiplier)
		{
			return indicator.TwoBarExpansionCrossingEMA(Input, eMAperiod, multiplier);
		}

		public Indicators.TwoBarExpansionCrossingEMA TwoBarExpansionCrossingEMA(ISeries<double> input , int eMAperiod, double multiplier)
		{
			return indicator.TwoBarExpansionCrossingEMA(input, eMAperiod, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TwoBarExpansionCrossingEMA TwoBarExpansionCrossingEMA(int eMAperiod, double multiplier)
		{
			return indicator.TwoBarExpansionCrossingEMA(Input, eMAperiod, multiplier);
		}

		public Indicators.TwoBarExpansionCrossingEMA TwoBarExpansionCrossingEMA(ISeries<double> input , int eMAperiod, double multiplier)
		{
			return indicator.TwoBarExpansionCrossingEMA(input, eMAperiod, multiplier);
		}
	}
}

#endregion
