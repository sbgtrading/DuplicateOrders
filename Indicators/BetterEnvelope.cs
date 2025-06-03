//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Better Envelopes
	/// </summary>
	public class BetterEnvelope : Indicator
	{
		private EMA		ema;
		private HMA		hma;
		private SMA		sma;
		private TEMA	tema;
		private TMA		tma;
		private LinReg  lr;
		private WMA		wma;
		private double envelopePercentage;
		private int period;
		private List<double> ranges = new List<double>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMAEnvelopes;
				Name						= "Better Envelope";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				MAType						= 7;
				Period						= 14;
				EnvelopePercentage			= 1.5;
				pWidthTicks = 0;
				pOpacity = 20;

				AddPlot(Brushes.DodgerBlue,																NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddPlot(new Gui.Stroke(Brushes.DodgerBlue, DashStyleHelper.Dash, 1), PlotStyle.Line,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorMiddle);
				AddPlot(Brushes.DodgerBlue,																NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
			}
			else if (State == State.DataLoaded)
			{
				ema		= EMA(Inputs[0], Period);
				hma		= HMA(Inputs[0], Math.Max(2, Period));
				sma		= SMA(Inputs[0], Period);
				tma		= TMA(Inputs[0], Period);
				tema	= TEMA(Inputs[0], Period);
				lr		= LinReg(Inputs[0], Period);
				wma		= WMA(Inputs[0], Period);
			}
		}

		double distance = 0;
		protected override void OnBarUpdate()
		{
			if(BarsArray[0].BarsSinceNewTradingDay==1){
				var dn = Times[0][0].AddDays(1).AddMinutes(-5);
				//dn = new DateTime(dn.Year,dn.Month,dn.Day,0,0,0);
				if(pWidthTicks>0){
					var mid = (Highs[0][1]+Lows[0][1])/2;
					Draw.Rectangle(this,CurrentBars[0].ToString(),false, Times[0][0], mid-pWidthTicks*TickSize/2, dn, mid+pWidthTicks*TickSize/2, pRectOutlineBrush, pRectFillBrush, this.pOpacity, true);
				}else if(pWidthTicks >0)
					Draw.Rectangle(this,CurrentBars[0].ToString(),false, Times[0][0], Lows[0][1], dn, Highs[0][1], pRectOutlineBrush, pRectFillBrush, pOpacity, true);
			}
			double maValue = 0;

			switch (MAType)
			{
				case 1:
				{
					Middle[0] = maValue = ema[0];
					break;
				}
				case 2:
				{
					Middle[0] = maValue = hma[0];
					break;
				}
				case 3:
				{
					Middle[0] = maValue = sma[0];
					break;
				}
				case 4:
				{
					Middle[0] = maValue = tma[0];
					break;
				}
				case 5:
				{
					Middle[0] = maValue = tema[0];
					break;
				}
				case 6:
				{
					Middle[0] = maValue = wma[0];
					break;
				}
				case 7:
				{
					Middle[0] = maValue = lr[0];
					break;
				}
			}
			if(pUseATR && IsFirstTickOfBar && CurrentBar>5){
				ranges.Add(Range()[1]);
				while(ranges.Count>Period) ranges.RemoveAt(0);
				distance = ranges.Average() * EnvelopePercentage;
			}else
				distance = (maValue * EnvelopePercentage / 100.0);

			Upper[0] = maValue + distance;
			Lower[0] = maValue - distance;
		}

		#region Properties

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Envelope %",
			GroupName = "NinjaScriptParameters", Order = 10)]
		public double EnvelopePercentage
		{
			get { return envelopePercentage; }
			set { envelopePercentage = value; }
		}

		private bool pUseATR = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Use ATR?",
			GroupName = "NinjaScriptParameters", Order = 20)]
		public bool UseATR
		{
			get { return pUseATR; }
			set { pUseATR = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Minimum Opening Height (ticks)", Description="'0' to use the Hi-Lo range of the opening candle, '-1' to turn-off the rectangle completely",
			GroupName = "NinjaScriptParameters", Order = 30)]
		public int pWidthTicks
		{get;set;}

		[Range(1, 7), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MAType", GroupName = "NinjaScriptParameters", Order = 40)]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(BetterEvenlopeEnumConverter))] // Converts the int to string values
		[PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")] // Create the combo box on the property grid
		public int MAType { get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 50)]
		public int Period
		{
			get { return MAType != 2 ? period : Math.Max(2, period); }
			set { period = MAType != 2 ? value : Math.Max(2, value); }
		}
		private Brush pRectFillBrush = Brushes.Cyan;
		[XmlIgnore()]
		[Display(Name = "Rectangle Fill", GroupName = "Custom Visual", Order = 10)]
		public Brush RectFillBrush{	get { return pRectFillBrush; }	set { pRectFillBrush = value; }		}
				[Browsable(false)]
				public string pRectFillBrushSerialize
				{	get { return Serialize.BrushToString(pRectFillBrush); } set { pRectFillBrush = Serialize.StringToBrush(value); }
				}
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fill opacity", GroupName = "Custom Visual", Order = 20)]
		public int pOpacity
		{get;set;}

		private Brush pRectOutlineBrush = Brushes.Transparent;
		[XmlIgnore()]
		[Display(Name = "Rectangle Outline", GroupName = "Custom Visual", Order = 30)]
		public Brush RectOutlineBrush{	get { return pRectOutlineBrush; }	set { pRectOutlineBrush = value; }		}
				[Browsable(false)]
				public string pRectOutlineBrushSerialize
				{	get { return Serialize.BrushToString(pRectOutlineBrush); } set { pRectOutlineBrush = Serialize.StringToBrush(value); }
				}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper { get { return Values[0]; } }
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Middle { get { return Values[1]; } }
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower { get { return Values[2]; } }
		#endregion
	}

	#region BetterEvenlopeEnumConverter
	public class BetterEvenlopeEnumConverter : TypeConverter
	{
		// Set the values to appear in the combo box
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			List<string> values = new List<string>() { "EMA", "HMA", "SMA", "TMA", "TEMA", "WMA", "LinReg" };

			return new StandardValuesCollection(values);
		}

		// map the value from string to int type
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			int mATypeValue = 3;

			switch (value.ToString())
			{
				case "EMA": mATypeValue = 1; break;
				case "HMA": mATypeValue = 2; break;
				case "SMA": mATypeValue = 3; break;
				case "TMA": mATypeValue = 4; break;
				case "TEMA": mATypeValue = 5; break;
				case "WMA": mATypeValue = 6; break;
				case "LinReg": mATypeValue = 7; break;
			}
			return mATypeValue;
		}

		// map the int type to string
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			string mATypeString = "SMA";

			switch (value.ToString())
			{
				case "1": mATypeString = "EMA"; break;
				case "2": mATypeString = "HMA"; break;
				case "3": mATypeString = "SMA"; break;
				case "4": mATypeString = "TMA"; break;
				case "5": mATypeString = "TEMA"; break;
				case "6": mATypeString = "WMA"; break;
				case "7": mATypeString = "LinReg"; break;
			}
			return mATypeString;
		}

		// required interface members needed to compile
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{ return true; }

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{ return true; }

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{ return true; }

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{ return true; }
	}
	#endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterEnvelope[] cacheBetterEnvelope;
		public BetterEnvelope BetterEnvelope(double envelopePercentage, int mAType, int period)
		{
			return BetterEnvelope(Input, envelopePercentage, mAType, period);
		}

		public BetterEnvelope BetterEnvelope(ISeries<double> input, double envelopePercentage, int mAType, int period)
		{
			if (cacheBetterEnvelope != null)
				for (int idx = 0; idx < cacheBetterEnvelope.Length; idx++)
					if (cacheBetterEnvelope[idx] != null && cacheBetterEnvelope[idx].EnvelopePercentage == envelopePercentage && cacheBetterEnvelope[idx].MAType == mAType && cacheBetterEnvelope[idx].Period == period && cacheBetterEnvelope[idx].EqualsInput(input))
						return cacheBetterEnvelope[idx];
			return CacheIndicator<BetterEnvelope>(new BetterEnvelope(){ EnvelopePercentage = envelopePercentage, MAType = mAType, Period = period }, input, ref cacheBetterEnvelope);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterEnvelope BetterEnvelope(double envelopePercentage, int mAType, int period)
		{
			return indicator.BetterEnvelope(Input, envelopePercentage, mAType, period);
		}

		public Indicators.BetterEnvelope BetterEnvelope(ISeries<double> input , double envelopePercentage, int mAType, int period)
		{
			return indicator.BetterEnvelope(input, envelopePercentage, mAType, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterEnvelope BetterEnvelope(double envelopePercentage, int mAType, int period)
		{
			return indicator.BetterEnvelope(Input, envelopePercentage, mAType, period);
		}

		public Indicators.BetterEnvelope BetterEnvelope(ISeries<double> input , double envelopePercentage, int mAType, int period)
		{
			return indicator.BetterEnvelope(input, envelopePercentage, mAType, period);
		}
	}
}

#endregion
