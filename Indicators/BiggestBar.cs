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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum BiggestBar_Types {Range, Volume, VolPerRange}
	public class BiggestBar : Indicator
	{
		private Brush bkg;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BiggestBar";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Lookback				= 10;
				pBasis					= BiggestBar_Types.Range;
				BkgBrush				= Brushes.Orange;
				pOpacity = 20;
				AddPlot(new Stroke(Brushes.HotPink,2), PlotStyle.Bar, "Num");
			}
			else if (State == State.Configure)
			{
				bkg = BkgBrush.Clone();
				bkg.Opacity = pOpacity/100.0;
				bkg.Freeze();
			}
		}

		List<double> VolsPerTick = new List<double>();
		protected override void OnBarUpdate()
		{
			if(CurrentBar > Lookback){
				if(pBasis == BiggestBar_Types.Range){
					Value[0] = High[0]-Low[0];
					if(High[0]-Low[0] > MAX(Range(),Lookback)[1]) BackBrushes[0] = bkg;
				}else if(pBasis == BiggestBar_Types.Volume){
					Value[0] = Volume[0];
					if(Volume[0] > MAX(Volume,Lookback)[1]) BackBrushes[0] = bkg;
				}else if(pBasis == BiggestBar_Types.VolPerRange){
					if(IsFirstTickOfBar) VolsPerTick.Insert(0,Volume[1] / (High[1]==Low[1] ? 1 : High[1]-Low[1]));
					while(VolsPerTick.Count>Lookback) VolsPerTick.RemoveAt(VolsPerTick.Count-1);
					double v = Volume[0] / (High[0]==Low[0] ? 1 : High[0]-Low[0]);
					Value[0] = v;
//Print(Times[0][0].ToString()+"  Vol/tk: "+v.ToString("0")+"   Max: "+VolsPerTick.Max().ToString("0"));
					if(v > VolsPerTick.Max()) BackBrushes[0] = bkg; else BackBrushes[0] = null;
				}
			}
			Draw.TextFixed(this,"info",pBasis.ToString(),TextPosition.TopLeft);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Lookback", Order=10, GroupName="Parameters")]
		public int Lookback
		{ get; set; }

		[Display(Name="Basis", Order=20, GroupName="Parameters")]
		public BiggestBar_Types pBasis
		{get;set;}

		[XmlIgnore]
		[Display(Name="BkgBrush", Order=30, GroupName="Parameters")]
		public Brush BkgBrush
		{ get; set; }

				[Browsable(false)]
				public string BkgBrushSerializable
				{			get { return Serialize.BrushToString(BkgBrush); }			set { BkgBrush = Serialize.StringToBrush(value); }		}			
		[Range(1, 100)]
		[Display(Name="BkgBrush Opacity", Order=40, GroupName="Parameters")]
		public int pOpacity
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BiggestBar[] cacheBiggestBar;
		public BiggestBar BiggestBar(int lookback)
		{
			return BiggestBar(Input, lookback);
		}

		public BiggestBar BiggestBar(ISeries<double> input, int lookback)
		{
			if (cacheBiggestBar != null)
				for (int idx = 0; idx < cacheBiggestBar.Length; idx++)
					if (cacheBiggestBar[idx] != null && cacheBiggestBar[idx].Lookback == lookback && cacheBiggestBar[idx].EqualsInput(input))
						return cacheBiggestBar[idx];
			return CacheIndicator<BiggestBar>(new BiggestBar(){ Lookback = lookback }, input, ref cacheBiggestBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BiggestBar BiggestBar(int lookback)
		{
			return indicator.BiggestBar(Input, lookback);
		}

		public Indicators.BiggestBar BiggestBar(ISeries<double> input , int lookback)
		{
			return indicator.BiggestBar(input, lookback);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BiggestBar BiggestBar(int lookback)
		{
			return indicator.BiggestBar(Input, lookback);
		}

		public Indicators.BiggestBar BiggestBar(ISeries<double> input , int lookback)
		{
			return indicator.BiggestBar(input, lookback);
		}
	}
}

#endregion
