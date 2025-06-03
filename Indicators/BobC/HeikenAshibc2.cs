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
using SharpDX;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.BobC
{
	// 04-27-2018 Changed line 220 from: chartControl.GetXByBarIndex(chartControl.BarsArray[0], idx); to: chartControl.GetXByBarIndex(ChartBars, idx); to
	// ensure compatibility when used in any panel on the chart.
	// Changed default shadow from black to dimgray.
	public class HeikenAshibc2 : Indicator
	{
        private Brush	barColorDown	= Brushes.Red;
        private Brush	barColorUp      = Brushes.Lime;
        private Brush	shadowColor     = Brushes.DimGray;  // changed 4-27-2018 (was black which did not work on black background)
        private Brush	divergeCandle   = Brushes.Yellow;  // Color body for Divergence Candle
		private Brush	divergeUpColor  = Brushes.Green;  // Color outline Down Divergence
		private Brush	divergeDnColor  = Brushes.Red;  // Color outline Up Divergence
        private int     shadowWidth     = 1;
		private bool	soundAlert		= false;
	
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"HeikenAshi technique discussed in the article 'Using Heiken-Ashi Technique' in February 2004 issue of TASC magazine.";
				Name								= "HeikenAshibc2";
				Calculate							= Calculate.OnPriceChange;
				IsOverlay							= true;
				DisplayInDataBox					= false;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= false;
				
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= false;
				BarsRequiredToPlot					= 1;
				armtime								=30;
				AddPlot(Brushes.Gray, "HAOpen");
				AddPlot(Brushes.Transparent, "HAHigh");
				AddPlot(Brushes.Transparent, "HALow");
				AddPlot(Brushes.Gray, "HAClose");
	
				
				
			}
		}

		protected override void OnBarUpdate()
		{

		#region HA Candle Calculations	
			//Clear out regular candles
			BarBrushes[0] = Brushes.Transparent;
		//	CandleOutlineBrushes[0] = Brushes.Transparent;

			if (CurrentBar == 0)
            {				
                HAOpen[0] 	=	Open[0];
                HAHigh[0] 	=	High[0];
                HALow[0]	=	Low[0];
                HAClose[0]	=	Close[0];
                return;
            }

            HAClose[0]	=	((Open[0] + High[0] + Low[0] + Close[0]) * 0.25); // Calculate the HAclose (average OHLC)
            HAOpen[0]	=	((HAOpen[1] + HAClose[1]) * 0.5); // Calculate the HAopen  (Avg HA O HA Cl
            HAHigh[0]	=	(Math.Max(High[0], HAOpen[0])); // Calculate the HAhigh
            HALow[0]	=	(Math.Min(Low[0], HAOpen[0])); // Calculate the HAlow				
		#endregion
			
		#region Transition Candles
			if(HAClose[0]>HAOpen[0]&&Close[0]<=Open[0])			//added Transition Yellow Outline
				{
					CandleOutlineBrushes[0] = divergeDnColor;
					BarBrushes[0]=divergeCandle;
				if(SoundAlert==true)	
					{
					Alert("myAlert", Priority.High, "Reached threshold", this.audioFileAlert, armtime, Brushes.Black, Brushes.Green);
			//		Alert("myAlert", Priority.Medium, "Reached threshold", NinjaTrader.Core.Globals.InstallDir+@"\sounds\broadheadSound.wav", 120, Brushes.Black, Brushes.Yellow);
			
					}
				}												
			if(HAClose[0]<HAOpen[0]&&Close[0]>=Open[0])			
				{
					CandleOutlineBrushes[0] = divergeUpColor;
					BarBrushes[0]=divergeCandle;
				if(SoundAlert==true)	
					{
					Alert("myAlert", Priority.High, "Reached threshold", this.audioFileAlert, armtime, Brushes.Black, Brushes.Green);
				//	Alert("myAlert", Priority.Medium, "Reached threshold", NinjaTrader.Core.Globals.InstallDir+@"\sounds\broadheadSound.wav", 120, Brushes.Black, Brushes.Yellow);
				
					}
				}												
		}
		#endregion
		
		#region Shorten Display Name	// In order to trim the indicator's label we need to override the ToString() method.
			public override string DisplayName
				{
		            get { return Name ;}
				}	
		#endregion
				
		#region Properties
		[XmlIgnore]
		[Display(Name="BarColorDown", Description="Color of Down bars", Order=2, GroupName="Visual")]
		public Brush BarColorDown
		{ 
			get { return barColorDown;}
			set { barColorDown = value;}
		}

		[Browsable(false)]
		public string BarColorDownSerializable
		{
			get { return Serialize.BrushToString(barColorDown); }
			set { barColorDown = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="BarColorUp", Description="Color of Up bars", Order=1, GroupName="Visual")]
		public Brush BarColorUp
		{ 
			get { return barColorUp;}
			set { barColorUp = value;}
		}

		[Browsable(false)]
		public string BarColorUpSerializable
		{
			get { return Serialize.BrushToString(barColorUp); }
			set { barColorUp = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="ShadowColor", Description="Wick/tail color", Order=3, GroupName="Visual")]
		public Brush ShadowColor
		{ 
			get { return shadowColor;}
			set { shadowColor = value;}
		}

		[Browsable(false)]
		public string ShadowColorSerializable
		{
			get { return Serialize.BrushToString(shadowColor); }
			set { shadowColor = Serialize.StringToBrush(value); }
		}			
		
		[XmlIgnore]
		[Display(Name="DivergenceCandle", Description="Divergence Candle Body Color", Order=4, GroupName="Visual")]
		public Brush DivergeCandle
		{ 
			get { return divergeCandle;}
			set { divergeCandle = value;}
		}

		[Browsable(false)]
		public string DivergeCandleSerializable
		{
			get { return Serialize.BrushToString(divergeCandle); }
			set { divergeCandle = Serialize.StringToBrush(value); }
		}
		
		
		[XmlIgnore]
		[Display(Name="DivergenceUpColor", Description="Divergence Up Wick/tail color", Order=5, GroupName="Visual")]
		public Brush DivergeUpColor
		{ 
			get { return divergeUpColor;}
			set { divergeUpColor = value;}
		}

		[Browsable(false)]
		public string DivergeUpColorSerializable
		{
			get { return Serialize.BrushToString(divergeUpColor); }
			set { divergeUpColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="DivergenceDnColor", Description="Divergence Down Wick/tail color", Order=6, GroupName="Visual")]
		public Brush DivergeDnColor
		{ 
			get { return divergeDnColor;}
			set { divergeDnColor = value;}
		}

		[Browsable(false)]
		public string DivergeDnColorSerializable
		{
			get { return Serialize.BrushToString(divergeDnColor); }
			set { divergeDnColor = Serialize.StringToBrush(value); }
		}			

		[Range(1, int.MaxValue)]
		[Display(Name="ShadowWidth", Description="Shadow (tail/wick) width", Order=7, GroupName="Visual")]
		public int ShadowWidth
		{ 
			get { return shadowWidth;}
			set { shadowWidth = value;}
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HALow
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAClose
		{
			get { return Values[3]; }
		}
		
		
		[Display(Name="Play Sound Alert", Description="Play wav file",Order=11, GroupName="Audible")]
		public bool SoundAlert
		{ 
			get { return soundAlert;}
			set { soundAlert = value;}
		}
			
		
		[Display(Name="Audio file for transition alerts", Order=12, GroupName="Audible")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string audioFileAlert
		{ get; set; }
		
		
		[Display(Name="Audio Re-Arm Time", Order=13, GroupName="Audible")]
		public int armtime
		{ get; set; }
		
		#endregion
	
		#region Miscellaneous

       	public override void OnCalculateMinMax()
        {
            base.OnCalculateMinMax();
			
            if (Bars == null || ChartControl == null)
                return;

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                double tmpHigh 	= 	HAHigh.GetValueAt(idx);
                double tmpLow 	= 	HALow.GetValueAt(idx);
				
                if (tmpHigh != 0 && tmpHigh > MaxValue)
                    MaxValue = tmpHigh;
                if (tmpLow != 0 && tmpLow < MinValue)
                    MinValue = tmpLow;										
            }
        }		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			
            if (Bars == null || ChartControl == null)
                return;			

            int barPaintWidth = Math.Max(3, 1 + 2 * ((int)ChartControl.BarWidth - 1) + 2 * shadowWidth);

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                if (idx - Displacement < 0 || idx - Displacement >= BarsArray[0].Count || ( idx - Displacement < BarsRequiredToPlot)) 
                    continue;
		
                double valH = HAHigh.GetValueAt(idx);
                double valL = HALow.GetValueAt(idx);
                double valC = HAClose.GetValueAt(idx);
                double valO = HAOpen.GetValueAt(idx);
                int x  = chartControl.GetXByBarIndex(ChartBars, idx);  //was chartControl.BarsArray[0]
                int y1 = chartScale.GetYByValue(valO);
                int y2 = chartScale.GetYByValue(valH);
                int y3 = chartScale.GetYByValue(valL);
                int y4 = chartScale.GetYByValue(valC);

				SharpDX.Direct2D1.Brush	shadowColordx 	= shadowColor.ToDxBrush(RenderTarget);  // prepare for the color to use
                var xy2 = new Vector2(x, y2);
                var xy3 = new Vector2(x, y3);
                RenderTarget.DrawLine(xy2, xy3, shadowColordx, shadowWidth);	

                if (y4 == y1)
				    RenderTarget.DrawLine( new Vector2( x - barPaintWidth / 2, y1),  new Vector2( x + barPaintWidth / 2, y1), shadowColordx, shadowWidth);
                else
                {
                    if (y4 > y1)
					{
						SharpDX.Direct2D1.Brush	barColorDowndx 	= barColorDown.ToDxBrush(RenderTarget);  // prepare for the color to use						
                        RenderTarget.FillRectangle( new RectangleF(x - barPaintWidth / 2, y1, barPaintWidth, y4 - y1), barColorDowndx);
						barColorDowndx.Dispose();
					}
                    else
					{
						SharpDX.Direct2D1.Brush	barColorUpdx 	= barColorUp.ToDxBrush(RenderTarget);  // prepare for the color to use
                        RenderTarget.FillRectangle( new RectangleF(x - barPaintWidth / 2, y4, barPaintWidth, y1 - y4),barColorUpdx);
						barColorUpdx.Dispose();
					}
                     RenderTarget.DrawRectangle( new RectangleF( x - barPaintWidth / 2 + (float)shadowWidth / 2,
                       Math.Min(y4, y1), barPaintWidth - (float)shadowWidth, Math.Abs(y4 - y1)), shadowColordx, shadowWidth);
				}	
				shadowColordx.Dispose();	
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
		private BobC.HeikenAshibc2[] cacheHeikenAshibc2;
		public BobC.HeikenAshibc2 HeikenAshibc2()
		{
			return HeikenAshibc2(Input);
		}

		public BobC.HeikenAshibc2 HeikenAshibc2(ISeries<double> input)
		{
			if (cacheHeikenAshibc2 != null)
				for (int idx = 0; idx < cacheHeikenAshibc2.Length; idx++)
					if (cacheHeikenAshibc2[idx] != null &&  cacheHeikenAshibc2[idx].EqualsInput(input))
						return cacheHeikenAshibc2[idx];
			return CacheIndicator<BobC.HeikenAshibc2>(new BobC.HeikenAshibc2(), input, ref cacheHeikenAshibc2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BobC.HeikenAshibc2 HeikenAshibc2()
		{
			return indicator.HeikenAshibc2(Input);
		}

		public Indicators.BobC.HeikenAshibc2 HeikenAshibc2(ISeries<double> input )
		{
			return indicator.HeikenAshibc2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BobC.HeikenAshibc2 HeikenAshibc2()
		{
			return indicator.HeikenAshibc2(Input);
		}

		public Indicators.BobC.HeikenAshibc2 HeikenAshibc2(ISeries<double> input )
		{
			return indicator.HeikenAshibc2(input);
		}
	}
}

#endregion
