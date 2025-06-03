

#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;

#endregion

// This namespace holds all indicators and is required. Do not change it.
/// Converted to NT8 8.0.19.0 64-bit By: @aligator on NinjaTrader and Furutres.IO Forums - October 02, 2019
namespace NinjaTrader.NinjaScript.Indicators
{
	public class JeffDots_Alerts7 : Indicator //JeffDots_Alerts7
	{
		#region Variables
		private int			period		 = 14;
		private int			ma_slope	 = 0;
		private double		radToDegrees = 180/Math.PI; // to convert Radians to Degrees for slope calc
		private int			angle1		 = 30;
		private int			angle2		 = 60;
		private int PriorDirection = 0;
		private int Direction = 0;
		private Indicator TheMA;
		
		//Paint MA line
		private Brush maUp 			= Brushes.Green;
		private Brush maDn 			= Brushes.Maroon;
		private Brush maSlopeUp 	= Brushes.LimeGreen;
		private Brush maSlopeDn 	= Brushes.Red;
		private Brush maFlat 		= Brushes.Gray;
		
		private	bool	alertOne 		= false;
		private	bool	enableTrendOnly = false;
		private string	longAlertInput 	= "none";
		private string	shortAlertInput = "none";
		int		resetBar;

		// Paint Bars
		private bool colorbars 			= false;
		private Brush barColorUp 		= Brushes.Green;
		private Brush barColorDown 		= Brushes.Maroon;
		private Brush barColorSlopeUp 	= Brushes.LimeGreen;
		private Brush barColorSlopeDown = Brushes.Red;
		private Brush barColorNeutral 	= Brushes.Gold;
		private Brush barColorOutline 	= Brushes.Black;
		#endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
               Name = "JeffDots_Alerts";
			   AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "MA line");
			   Plots[0].DashStyleHelper = DashStyleHelper.Dash;
			   DrawOnPricePanel = true;
			   Calculate 		= Calculate.OnBarClose;					
			   IsOverlay		= true;
             }
			else if(State == State.DataLoaded){
				if(pMAtype == JeffDots_Alerts7_matype.HMA)  TheMA = HMA(period);
				if(pMAtype == JeffDots_Alerts7_matype.SMA)  TheMA = SMA(period);
				if(pMAtype == JeffDots_Alerts7_matype.EMA)  TheMA = EMA(period);
				if(pMAtype == JeffDots_Alerts7_matype.WMA)  TheMA = WMA(period);
				if(pMAtype == JeffDots_Alerts7_matype.TEMA) TheMA = TEMA(period);
				if(pMAtype == JeffDots_Alerts7_matype.VWMA) TheMA = VWMA(period);
			}
        }

		protected override void OnBarUpdate()
		{
			if(CurrentBar<Period) return;

			if(IsFirstTickOfBar && pSeparation>=0) 
			{
				if(PriorDirection != 1 && Direction == 1)
					Draw.Dot(this,"JeffsDots_Alerts_V3"+(CurrentBar-1), true, 1, Low[1]-pSeparation*TickSize, dotColorUp);
				else if(PriorDirection != -1 && Direction == -1)
					Draw.Dot(this,"JeffsDots_Alerts_V3"+(CurrentBar-1), true, 1, High[1]+pSeparation*TickSize, dotColorDn);
				else 
					RemoveDrawObject("JeffsDots_Alerts_V3"+(CurrentBar-1));
				PriorDirection = Direction;
			}

			ma_slope = (int)(radToDegrees*(Math.Atan((TheMA[0]-(TheMA[1]+TheMA[2])/2)/1.5/TickSize)));
			
			MAline[0] = TheMA[0];
			if(IsRising(TheMA))
			{
				if(ma_slope>=angle2)
				{
					PlotBrushes[0][0] = maSlopeUp;
					Direction = 1;
					if (ColorBars)
					{ 
						CandleOutlineBrush = BarColorOutline; 
						BarBrush = BarColorSlopeUp; 
					}
				}
				else
				if (ma_slope>=angle1)
				{
					PlotBrushes[0][0] = maUp;
					Direction = 1;
					if (ColorBars)
					{ 
						CandleOutlineBrush = BarColorOutline; 
						BarBrush = BarColorUp; 
					}
				}
				else
				{	
					PlotBrushes[0][0] = maFlat;
					Direction = 0;
					if (ColorBars)
					{ 
						CandleOutlineBrush = BarColorOutline; 
						BarBrush = BarColorNeutral; 
					}
				}
			}
			else
			{	
				if(ma_slope<=-angle2)
				{
					PlotBrushes[0][0] = maSlopeDn;
					Direction = -1;
					if (ColorBars)
					{ 
						CandleOutlineBrush = BarColorOutline; 
						BarBrush = BarColorSlopeDown; 
					}
				}
				else
				if (ma_slope<=-angle1)
				{
					PlotBrushes[0][0] = maDn;
					Direction = -1;
					if (ColorBars)
					{ 
						CandleOutlineBrush = BarColorOutline; 
						BarBrush = BarColorDown; 
					}
				}
				else
				{	
					PlotBrushes[0][0] = maFlat;
					Direction = 0;
					if (ColorBars)
					{ 
						CandleOutlineBrush = BarColorOutline; 
						BarBrush = BarColorNeutral; 
					}
				}
			}
			if(pSeparation>=0) 
			{
				if(PriorDirection != 1 && Direction == 1)
				{
					Draw.Dot(this,"JeffDots_Alerts7"+CurrentBar, true, 0, Low[0]-pSeparation*TickSize, dotColorUp);
					if(alertOne && resetBar != CurrentBar)
						if(enableTrendOnly && EMA(Close, 20)[0] > EMA(Close, 50)[0] || !enableTrendOnly)
						{
							Alert("PossibleDotUp", Priority.High,"Possible Up Dot " + Period.ToString() + " chart",longAlertInput,6,dotColorUp,Brushes.White);
							resetBar = CurrentBar;
						}
				}
				else if(PriorDirection != -1 && Direction == -1)
				{
					Draw.Dot(this,"JeffDots_Alerts7"+CurrentBar, true, 0, High[0]+pSeparation*TickSize, dotColorDn);
					if(alertOne && resetBar != CurrentBar) 
						if(enableTrendOnly && EMA(Close, 20)[0] < EMA(Close, 50)[0] || !enableTrendOnly)
						{
							Alert("PossibleDotDn", Priority.High,"Possible Down Dot " + Period.ToString() + " chart",shortAlertInput,6,dotColorDn,Brushes.White);
							resetBar = CurrentBar;
						}
				}
				else RemoveDrawObject("JeffDots_Alerts7"+CurrentBar);
			}
		}

		[Browsable(false)]	
        [XmlIgnore()]	
        public Series<double> MAline
        {
            get { return Values[0]; }
        }

		#region Properties
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
		
		private JeffDots_Alerts7_matype pMAtype = JeffDots_Alerts7_matype.HMA;
		[NinjaScriptProperty]		[Display(Description = "Select an MA type", GroupName = "Parameters", Order = 1)]		public JeffDots_Alerts7_matype MAtype
		{
			get { return pMAtype; }
			set { pMAtype = value; }
		}

		[NinjaScriptProperty]
		[Display(Description = "Numbers of bars used for calculations", GroupName = "Parameters", Order = 1)]
		public int Period
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}
		
		private int pSeparation = 1;
		[NinjaScriptProperty]
		[Display(Description = "Number of ticks between the price bar and the Signal Dot.  Enter a '-1' here to turn-off the plotting of the dots on the price panel", GroupName = "Visual", Order = 1)]		public int Separation
		{
			get { return pSeparation; }
			set { pSeparation = Math.Max(-1, value); }
		}
		
		[NinjaScriptProperty]		
		[Display(Name = "Angle 1", Description = "MA angle between Flat and Sloping zones.", GroupName = "Parameters", Order = 1)]		
		public int Angle1
		{
			get { return angle1; }
			set { angle1 = Math.Max(1, value); }
		}
		
		[NinjaScriptProperty]		
		[Display(Name = "Angle 2", Description = "MA angle between Sloping and Steep zones.", GroupName = "Parameters", Order = 1)]		
		public int Angle2
		{
			get { return angle2; }
			set { angle2 = Math.Max(1, value); }
		}
		
/////////////////////////////////////////////////////////////////////////////////////		
		private Brush dotColorUp = Brushes.Green;
        [XmlIgnore()]
		[Display(Name = "08. Buy Dot", Description = "Color of a BUY dot, when the MA turns upward", GroupName = "Visual", Order = 1)]
        public Brush DotColorUp
        {get { return dotColorUp; } set { dotColorUp = value; }        }
			        [Browsable(false)]
			        public string dotColorUpSerialize
			        {get { return Serialize.BrushToString(dotColorUp); } set { dotColorUp = Serialize.StringToBrush(value); }
			        }

		private Brush dotColorDn = Brushes.Red;
        [XmlIgnore()]
		[Display(Name = "09. Sell Dot", Description = "Color of a SELL dot, when the MA turns downward", GroupName = "Visual", Order = 1)]
        public Brush DotColorDn
        {get { return dotColorDn; } set { dotColorDn = value; }        }
			        [Browsable(false)]
			        public string dotColorDnSerialize
			        {get { return Serialize.BrushToString(dotColorDn); } set { dotColorDn = Serialize.StringToBrush(value); }
			        }

		[Display(Name = "01. Color Bars?", Description = "Color price bars?", GroupName = "Visual", Order = 1)]
		public bool ColorBars
        {
            get { return colorbars; }
            set { colorbars = value; }
        }
		
        [XmlIgnore()]
        [Display(Name = "02. Up color", Description = "Color of up bars", GroupName = "Visual", Order = 1)]
        public Brush BarColorUp
        {
            get { return barColorUp; }
            set { barColorUp = value; }
        }
			        [Browsable(false)]
			        public string barColorUpSerialize { get { return Serialize.BrushToString(barColorUp); } set { barColorUp = Serialize.StringToBrush(value); }}
		
        [XmlIgnore()]
        [Display(Name = "03. Down color", Description = "Color of down bars", GroupName = "Visual", Order = 1)]
        public Brush BarColorDown
        {
            get { return barColorDown; }
            set { barColorDown = value; }
        }
			        [Browsable(false)]
			        public string barColorDownSerialize { get { return Serialize.BrushToString(barColorDown); } set { barColorDown = Serialize.StringToBrush(value); } }
        
        
        [XmlIgnore()]
        [Display(Name = "04. Slope Up color", Description = "Color of slope up bars", GroupName = "Visual", Order = 1)]
        public Brush BarColorSlopeUp
        {
            get { return barColorSlopeUp; }
            set { barColorSlopeUp = value; }
        }
			        [Browsable(false)]
			        public string barColorSlopeUpSerialize { get { return Serialize.BrushToString(barColorSlopeUp); } set { barColorSlopeUp = Serialize.StringToBrush(value); } }
		
        [XmlIgnore()]
        [Display(Name = "05. Slope Down color", Description = "Color of slope down bars", GroupName = "Visual", Order = 1)]
        public Brush BarColorSlopeDown
        {
            get { return barColorSlopeDown; }
            set { barColorSlopeDown = value; }
        }
			        [Browsable(false)]
			        public string barColorSlopeDownSerialize { get { return Serialize.BrushToString(barColorSlopeDown); } set { barColorSlopeDown = Serialize.StringToBrush(value);} }
        
        [XmlIgnore()]
        [Display(Name = "06. Neutral color", Description = "Color of neutral bars", GroupName = "Visual", Order = 1)]
        public Brush BarColorNeutral
        {
            get { return barColorNeutral; }
            set { barColorNeutral = value; }
        }
			        [Browsable(false)]
			        public string barColorNeutralSerialize { get { return Serialize.BrushToString(barColorNeutral); } set { barColorNeutral = Serialize.StringToBrush(value); } }
		
        [XmlIgnore()]
        [Display(Name = "07. Outline color", Description = "Color of bar outline", GroupName = "Visual", Order = 1)]
        public Brush BarColorOutline
        {
            get { return barColorOutline; }
            set { barColorOutline = value; }
        }
			        [Browsable(false)]
			        public string barColorOutlineSerialize { get { return Serialize.BrushToString(barColorOutline); } set { barColorOutline = Serialize.StringToBrush(value); } }
		
////////////////////////////////////////////////////////////////////////////////////////

		[XmlIgnore()]
		[Display(Name = "Up Trend color", Description = "Color of Up Trend line.", GroupName = "Plots", Order = 1)]
		public Brush MAup
		{
			get { return maUp; }
			set { maUp = value; }
		}
					[Browsable(false)]
					public string maUpSerialize {get { return Serialize.BrushToString(maUp); }set { maUp = Serialize.StringToBrush(value); }}
		
		[XmlIgnore()]
		[Display(Name = "Down Trend color", Description = "Color of Down Trend line.", GroupName = "Plots", Order = 1)]
		public Brush MAdn
		{
			get { return maDn; }
			set { maDn = value; }
		}
					[Browsable(false)]
					public string maDnSerialize {get { return Serialize.BrushToString(maDn); }set { maDn = Serialize.StringToBrush(value); }	}
		
		[XmlIgnore()]
		[Display(Name = "Upslope Trend color", Description = "Color of Upslope Trend line.", GroupName = "Plots", Order = 1)]
		public Brush MAslopeup
		{
			get { return maSlopeUp; }
			set { maSlopeUp = value; }
		}
					[Browsable(false)]
					public string maSlopeUpSerialize {get { return Serialize.BrushToString(maSlopeUp); }set { maSlopeUp = Serialize.StringToBrush(value); }	}
		
		[XmlIgnore()]
		[Display(Name = "Downslope Trend color", Description = "Color of Downslope Trend line.", GroupName = "Plots", Order = 1)]
		public Brush MAslopedn
		{
			get { return maSlopeDn; }
			set { maSlopeDn = value; }
		}
					[Browsable(false)]
					public string maSlopeDnSerialize { get { return Serialize.BrushToString(maSlopeDn); }set { maSlopeDn = Serialize.StringToBrush(value); }	}
		
		[XmlIgnore()]
		[Display(Name = "Flat Trend color", Description = "Color of Flat Trend line.", GroupName = "Plots", Order = 1)]
		public Brush MAflat
		{
			get { return maFlat; }
			set { maFlat = value; }
		}
					[Browsable(false)]
					public string maFlatSerialize {get { return Serialize.BrushToString(maFlat); }	set { maFlat = Serialize.StringToBrush(value); }}
		
////////////////////////////////////////////////////////////////////////////////////////
		
		/// Alerts
		[Display(Name = "Alert on?", Description = "Alert when a Jeff dot is forming.", GroupName = "Alerts", Order = 1)]		
		public bool AlertOne
        {
            get { return alertOne; }
            set { alertOne = value; }
        }
		
		[Display(Name = "Alert with trend only?", Description = "Alert only when with the trend.", GroupName = "Alerts", Order = 1)]		
		public bool EnableTrendOnly
        {
            get { return enableTrendOnly; }
            set { enableTrendOnly = value; }
        }
		
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Custom 'long' .wav file", Description = "Your custom .wav file for the possible long Alert. This would be a custom file you have placed in the Sounds folder in the NinjaTrader 7 folder (typically C:\\NinjaTrader 7\\sounds).", GroupName = "Alerts", Order = 1)]		
		public string LongAlertInput
        {
            get { return longAlertInput; }
            set { longAlertInput = value; }
        }
		
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Name = "Custom 'short' .wav file", Description = "Your custom .wav file for the possible short Alert. This would be a custom file you have placed in the Sounds folder in the NinjaTrader 7 folder (typically C:\\NinjaTrader 7\\sounds).", GroupName = "Alerts", Order = 1)]		
		public string ShortAlertInput
        {
            get { return shortAlertInput; }
            set { shortAlertInput = value; }
        }
		
		#endregion
	}
}
public enum JeffDots_Alerts7_matype 
{
	SMA,
	EMA,
	HMA,
	WMA,
	VWMA,
	TEMA
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JeffDots_Alerts7[] cacheJeffDots_Alerts7;
		public JeffDots_Alerts7 JeffDots_Alerts7(JeffDots_Alerts7_matype mAtype, int period, int separation, int angle1, int angle2)
		{
			return JeffDots_Alerts7(Input, mAtype, period, separation, angle1, angle2);
		}

		public JeffDots_Alerts7 JeffDots_Alerts7(ISeries<double> input, JeffDots_Alerts7_matype mAtype, int period, int separation, int angle1, int angle2)
		{
			if (cacheJeffDots_Alerts7 != null)
				for (int idx = 0; idx < cacheJeffDots_Alerts7.Length; idx++)
					if (cacheJeffDots_Alerts7[idx] != null && cacheJeffDots_Alerts7[idx].MAtype == mAtype && cacheJeffDots_Alerts7[idx].Period == period && cacheJeffDots_Alerts7[idx].Separation == separation && cacheJeffDots_Alerts7[idx].Angle1 == angle1 && cacheJeffDots_Alerts7[idx].Angle2 == angle2 && cacheJeffDots_Alerts7[idx].EqualsInput(input))
						return cacheJeffDots_Alerts7[idx];
			return CacheIndicator<JeffDots_Alerts7>(new JeffDots_Alerts7(){ MAtype = mAtype, Period = period, Separation = separation, Angle1 = angle1, Angle2 = angle2 }, input, ref cacheJeffDots_Alerts7);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JeffDots_Alerts7 JeffDots_Alerts7(JeffDots_Alerts7_matype mAtype, int period, int separation, int angle1, int angle2)
		{
			return indicator.JeffDots_Alerts7(Input, mAtype, period, separation, angle1, angle2);
		}

		public Indicators.JeffDots_Alerts7 JeffDots_Alerts7(ISeries<double> input , JeffDots_Alerts7_matype mAtype, int period, int separation, int angle1, int angle2)
		{
			return indicator.JeffDots_Alerts7(input, mAtype, period, separation, angle1, angle2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JeffDots_Alerts7 JeffDots_Alerts7(JeffDots_Alerts7_matype mAtype, int period, int separation, int angle1, int angle2)
		{
			return indicator.JeffDots_Alerts7(Input, mAtype, period, separation, angle1, angle2);
		}

		public Indicators.JeffDots_Alerts7 JeffDots_Alerts7(ISeries<double> input , JeffDots_Alerts7_matype mAtype, int period, int separation, int angle1, int angle2)
		{
			return indicator.JeffDots_Alerts7(input, mAtype, period, separation, angle1, angle2);
		}
	}
}

#endregion
