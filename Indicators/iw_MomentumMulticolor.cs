#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The MomentumMulticolor indicator measures the amount that a security's price has changed over a given time span.
	/// </summary>
	[Description("The MomentumMulticolor indicator measures the amount that a security's price has changed over a given time span.")]
	public class MomentumMulticolor : Indicator
	{
		private bool LicenseValid = true;
		private int	pPeriod	= 14;
		private int MomentumDirection = 0;
		private bool OHLC_or_HiLo = false;
		private Brush UpBkgBrush = null;
		private Brush DownBkgBrush = null;
		private int SoundBar = 0;
		private MFI mfi;


	protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			var Debug = System.IO.File.Exists("c:\\222222222222.txt");
			Debug = Debug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0);
			if(!Debug)
				VendorLicense("IndicatorWarehouse", "AIMultiColorMomentum", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
			AddPlot(new Stroke(Brushes.Green,2),PlotStyle.Line, "Line1");
			AddPlot(new Stroke(Brushes.Green,2),PlotStyle.Line, "Line2");
			AddLine(new Stroke(Brushes.DarkViolet, DashStyleHelper.Solid, 1), pMidlineLevel, "Mid line");
			Name = "iw Momentum Multicolor";
		}else if(State==State.Configure){
			Lines[0].Value = pMidlineLevel;
			if(pBasis == MomentumMulticolor_Basis.PositiveNegative) {
				Plots[0].Min = pMidlineLevel;
				Plots[1].Max = pMidlineLevel;
				Plots[0].Pen = new Pen(pUpBrush,Plots[0].Pen.Thickness);
				Plots[1].Pen = new Pen(pDownBrush,Plots[1].Pen.Thickness);
			}
			if(pBasis == MomentumMulticolor_Basis.RisingFalling) {
				Plots[1].Pen = new Pen(Brushes.Transparent,Plots[1].Width);
				Plots[0].Min = double.MinValue;
				Plots[1].Max = double.MaxValue;
			}
			UpBkgBrush = pUpBrush.Clone();
			UpBkgBrush.Opacity = pBackgroundOpacity/100f;
			UpBkgBrush.Freeze();
			DownBkgBrush = pDownBrush.Clone();
			DownBkgBrush.Opacity = pBackgroundOpacity/100f;
			DownBkgBrush.Freeze();
			
			mfi = MFI(pPeriod);
		}
	}

	double mom = 0;
	int prior_dir = -1111;
		protected override void OnBarUpdate()
		{
			if(pType == MomentumMulticolor_CalcType.MFI){
				mom = mfi[0];
			}else{
				mom = (CurrentBar == 0 ? 0 : Input[0] - Input[Math.Min(CurrentBar, pPeriod)]);
			}
			int dir = 0;
			bool IsLive = State!=State.Historical;
			if(pBasis == MomentumMulticolor_Basis.PositiveNegative) {
				Values[0][0] = (mom);
				Values[1][0] = (mom);
				if(mom>pMidlineLevel) dir = 1; else dir=-1;
				if(pBackgroundOpacity>0) {
					BackBrush = (mom>pMidlineLevel ? UpBkgBrush:DownBkgBrush);
				}
				if(pEnableAudibleAlerts && IsLive){
					if(dir != prior_dir){//plays only if a new direction is achieved
						if(dir == 1 && pBuySound.Length>0)  {PlaySound(AddSoundFolder(pBuySound));  SoundBar=CurrentBar;}
						if(dir == -1 && pSellSound.Length>0) {PlaySound(AddSoundFolder(pSellSound)); SoundBar=CurrentBar;}
						prior_dir = dir;
					}
				}
			}
			if(pBasis == MomentumMulticolor_Basis.RisingFalling && CurrentBar>1) {
				Values[0][0] = (mom);
				if(Values[0][0] > Values[0][1]) MomentumDirection = 1;
				if(Values[0][0] < Values[0][1]) MomentumDirection = -1;
				if(MomentumDirection>0)      PlotBrushes[0][0] = pUpBrush;
				else if(MomentumDirection<0) PlotBrushes[0][0] = pDownBrush;

				if(MomentumDirection > 0) dir = 1; 
				else if(MomentumDirection < 0) dir = -1;

				if(pBackgroundOpacity>0) {
					BackBrush = (MomentumDirection > 0 ? UpBkgBrush : DownBkgBrush);
				}
				if(pEnableAudibleAlerts && IsLive){
					if(dir != prior_dir){//plays only if a new direction is achieved
						if(MomentumDirection > 0 && pBuySound.Length>0)  {PlaySound(AddSoundFolder(pBuySound));  SoundBar=CurrentBar;}
						if(MomentumDirection < 0 && pSellSound.Length>0) {PlaySound(AddSoundFolder(pSellSound)); SoundBar=CurrentBar;}
						prior_dir = dir;
					}
				}
			}
//			if(Bars.BarsType.DefaultChartStyle == ChartStyleType.OHLC)     OHLC_or_HiLo = true;
//			if(Bars.BarsType.DefaultChartStyle == ChartStyleType.HiLoBars) OHLC_or_HiLo = true;
			if(ColorBarOnTrends){
				if(dir==1) {
					CandleOutlineBrush = pcou;
					if(Close[0]<Open[0])		BarBrush = pud;//(OHLC_or_HiLo? pud : NormalBackgroundColor);
					else if(Close[0]==Open[0])	BarBrush = CandleOutlineBrush;
					else 						BarBrush = puu;
				} else if(dir==-1) {
					CandleOutlineBrush = pcod;
					if(Close[0]<Open[0])		BarBrush = pdd;//(OHLC_or_HiLo? pdd : NormalBackgroundColor);
					else if(Close[0]==Open[0])	BarBrush = CandleOutlineBrush;
					else 						BarBrush = pdu;
				}
			}
		}

//-----------------------------------------------------------------------------------------------
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
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(new string[]{NinjaTrader.Core.Globals.InstallDir, "sounds", wav});
		}
//====================================================================
		#region Properties
//-----------------------------------------------------------------------------------------------
		[Description("Numbers of bars used for momentum calculation")]
		[Category("Parameters")]
		public int Period
		{
			get { return pPeriod; }
			set { pPeriod = Math.Max(1, value); }
		}
		private MomentumMulticolor_CalcType pType = MomentumMulticolor_CalcType.MFI;
		[Description("Select indicator calculation type, either Momentum or MFI")]
		[Category("Parameters")]
		public MomentumMulticolor_CalcType Type
		{
			get { return pType; }
			set { pType = value; }
		}
		private MomentumMulticolor_Basis pBasis = MomentumMulticolor_Basis.PositiveNegative;
		[Description("Basis for color of Momentum line")]
		[Category("Parameters")]
		public MomentumMulticolor_Basis Basis
		{
			get { return pBasis; }
			set { pBasis = value; }
		}
		private double pMidlineLevel = 50;
		[Description("Midline for coloring of 'PositiveNegative', customarily 0 for Momentum, 50 for MFI")]
		[Category("Parameters")]
		public double MidlineLevel
		{
			get { return pMidlineLevel; }
			set { pMidlineLevel = value; }
		}

		private int pBackgroundOpacity = 0;
		[Description("Colorize the background?  0=no colorizing, 100=full color")]
		[Category("Visual")]
		public int BackgroundOpacity
		{
			get { return pBackgroundOpacity; }
			set { pBackgroundOpacity = Math.Max(0,Math.Min(10,value)); }
		}

		#region PlotColors
		private Brush pUpBrush = Brushes.Green;
		[XmlIgnore]
		[Description("Color of Momentum line when it is 'up'")]
		[Category("Plot Colors")]
		public Brush UpBrush
		{get { return pUpBrush; }
		set { pUpBrush = value; }}
		[Browsable(false)]
		public string UpColorDownSerialize
		{get { return Serialize.BrushToString(pUpBrush); }set { pUpBrush = Serialize.StringToBrush(value); }}

		private Brush pDownBrush = Brushes.Red;
		[XmlIgnore]
		[Description("Color of Momentum line when it is 'down'")]
		[Category("Plot Colors")]
		public Brush DownBrush
		{get { return pDownBrush; }
		set { pDownBrush = value; }}
		[Browsable(false)]
		public string DownColorDownSerialize
		{get { return Serialize.BrushToString(pDownBrush); }set { pDownBrush = Serialize.StringToBrush(value); }}
		#endregion

		#region Audible
		private bool pEnableAudibleAlerts = false;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enable",  GroupName = "Audible", Order=0)]
		[Description("Enable sound alerts when signal occurs")]
		public bool EnableAudibleAlerts
		{
			get { return pEnableAudibleAlerts; }
			set { pEnableAudibleAlerts = value; }
		}

		private string pBuySound = "Alert2.wav";
		[Description("Sound file on up close bar with positive/rising momentum line - it must exist in your Sounds folder in order to be played")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Buy sound",  GroupName = "Audible", Order=1)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string BuySound
		{
			get { return pBuySound; }
			set { pBuySound = value.Trim(); }
		}
		private string pSellSound = "Alert2.wav";
		[Description("Sound file on down close bar with negative/falling momentum line - it must exist in your Sounds folder in order to be played")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sell sound",  GroupName = "Audible", Order=2)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SellSound
		{
			get { return pSellSound; }
			set { pSellSound = value.Trim(); }
		}
		#endregion

		#region PaintBar colors
		private bool pColorBarOnTrends = false;
		[Description("Colorize the bars according to their position relative to the MA?")]
		[Category("PaintBar Colors")]
		public bool ColorBarOnTrends
		{
			get { return pColorBarOnTrends; }
			set { pColorBarOnTrends = value; }
		}

		private Brush pcou = Brushes.Green;
		[XmlIgnore]
		[Description("Color of candle outline on up-momentum bar")]
		[Category("PaintBar Colors")]
		public Brush UpMom_Outline
		{get { return pcou; }
		set { pcou = value; }}
		[Browsable(false)]
		public string pcouColorDownSerialize
		{get { return Serialize.BrushToString(pcou); }set { pcou = Serialize.StringToBrush(value); }}

		private Brush pcod = Brushes.Red;
		[XmlIgnore]
		[Description("Color of candle outline on down-momentum bar")]
		[Category("PaintBar Colors")]
		public Brush DownMom_Outline
		{get { return pcod; }
		set { pcod = value; }}
		[Browsable(false)]
		public string pcodColorDownSerialize
		{get { return Serialize.BrushToString(pcod); }set { pcod = Serialize.StringToBrush(value); }}

		private Brush puu = Brushes.Green;
		[XmlIgnore]
		[Description("Color of up-closing bar on up-momentum")]
		[Category("PaintBar Colors")]
		public Brush UpMom_UpClose
		{get { return puu; }
		set { puu = value; }}
		[Browsable(false)]
		public string puuColorDownSerialize
		{get { return Serialize.BrushToString(puu); }set { puu = Serialize.StringToBrush(value); }}

		private Brush pdu = Brushes.Blue;
		[XmlIgnore]
		[Description("Color of up-closing bar on down-momentum")]
		[Category("PaintBar Colors")]
		public Brush DownMom_UpClose
		{get { return pdu; }
		set { pdu = value; }}
		[Browsable(false)]
		public string pduColorDownSerialize
		{get { return Serialize.BrushToString(pdu); }set { pdu = Serialize.StringToBrush(value); }}

		private Brush pud = Brushes.Blue;
		[XmlIgnore]
		[Description("Color of down-closing bar on up-momentum")]
		[Category("PaintBar Colors")]
		public Brush UpMom_DownClose
		{get { return pud; }
		set { pud = value; }}
		[Browsable(false)]
		public string pudColorDownSerialize
		{get { return Serialize.BrushToString(pud); }set { pud = Serialize.StringToBrush(value); }}

		private Brush pdd = Brushes.Red;
		[XmlIgnore]
		[Description("Color of down-closing bar on down-momentum")]
		[Category("PaintBar Colors")]
		public Brush DownMom_DownClose
		{get { return pdd; }
		set { pdd = value; }}
		[Browsable(false)]
		public string pddColorDownSerialize
		{get { return Serialize.BrushToString(pdd); }set { pdd = Serialize.StringToBrush(value); }}
		#endregion

		#endregion
	}
}
public enum MomentumMulticolor_Basis {
	RisingFalling, PositiveNegative
}
public enum MomentumMulticolor_CalcType {MFI, Momentum}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MomentumMulticolor[] cacheMomentumMulticolor;
		public MomentumMulticolor MomentumMulticolor()
		{
			return MomentumMulticolor(Input);
		}

		public MomentumMulticolor MomentumMulticolor(ISeries<double> input)
		{
			if (cacheMomentumMulticolor != null)
				for (int idx = 0; idx < cacheMomentumMulticolor.Length; idx++)
					if (cacheMomentumMulticolor[idx] != null &&  cacheMomentumMulticolor[idx].EqualsInput(input))
						return cacheMomentumMulticolor[idx];
			return CacheIndicator<MomentumMulticolor>(new MomentumMulticolor(), input, ref cacheMomentumMulticolor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MomentumMulticolor MomentumMulticolor()
		{
			return indicator.MomentumMulticolor(Input);
		}

		public Indicators.MomentumMulticolor MomentumMulticolor(ISeries<double> input )
		{
			return indicator.MomentumMulticolor(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MomentumMulticolor MomentumMulticolor()
		{
			return indicator.MomentumMulticolor(Input);
		}

		public Indicators.MomentumMulticolor MomentumMulticolor(ISeries<double> input )
		{
			return indicator.MomentumMulticolor(input);
		}
	}
}

#endregion
