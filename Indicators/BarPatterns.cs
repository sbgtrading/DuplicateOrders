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
	public class BarPatterns : Indicator
	{
		private Brush BuyStrong = Brushes.Green;
		private Brush SellStrong = Brushes.Pink;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BarPatterns";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Idnumber					= 1;
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Dot, "Signal");
			}
			else if (State == State.DataLoaded)
			{
				BuyStrong = Brushes.Green.Clone();
				BuyStrong.Opacity = 0.5;
				BuyStrong.Freeze();
				SellStrong = Brushes.Pink.Clone();
				SellStrong.Opacity = 0.5;
				SellStrong.Freeze();
			}
		}

		List<byte> Dir = new List<byte>();//0 is down, 1 is up, 2 is doji
		byte UP = 1;
		byte DOWN = 0;
		byte signal = byte.MaxValue;
		protected override void OnBarUpdate()
		{
			if(CurrentBars[0] < 1) return;
			if(IsFirstTickOfBar){
				if(Closes[0][1] < Opens[0][1])
					Dir.Insert(0, DOWN);
				else if(Closes[0][1] > Opens[0][1])
					Dir.Insert(0, UP);
			}

			signal = byte.MaxValue;
			double atr = (Range()[0] + Range()[1])/4;
			if(Dir.Count>10 && Dir[0] == UP && Dir[1] == DOWN && Dir[2] == UP && Dir[3] == UP// && Dir[5] == UP
				&& Highs[0][0] > Highs[0][2] + atr){
				if(Dir[4] == UP){
					BackBrushesAll[0] = SellStrong;
					Signal[0] = Lows[0][0]-TickSize;
					signal = DOWN;
				}
				else{
					BackBrushes[0] = Brushes.Pink;
					Signal[0] = Lows[0][0]-TickSize;
					signal = DOWN;
				}
			}
			else if(Dir.Count>10 && Dir[0] == DOWN && Dir[1] == UP && Dir[2] == DOWN && Dir[3] == DOWN// && Dir[5] == UP
				&& Lows[0][0] < Lows[0][2]-atr){
				if(Dir[4] == DOWN){
					BackBrushesAll[0] = BuyStrong;
					Signal[0] = Highs[0][0]+TickSize;
					signal = UP;
				}else{
					BackBrushes[0] = Brushes.Lime;
					Signal[0] = Highs[0][0]+TickSize;
					signal = UP;
				}
			}
			if(signal == UP) Draw.Dot(this,$"BarPatterns{CurrentBars[0]}",false,0,Highs[0][0]+TickSize, Brushes.White);
			else if(signal == DOWN) Draw.Dot(this,$"BarPatterns{CurrentBars[0]}",false,0,Lows[0][0]-TickSize, Brushes.White);

			while(Dir.Count>10) Dir.RemoveAt(Dir.Count-1);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Idnumber", Order=1, GroupName="Parameters")]
		public int Idnumber
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BarPatterns[] cacheBarPatterns;
		public BarPatterns BarPatterns(int idnumber)
		{
			return BarPatterns(Input, idnumber);
		}

		public BarPatterns BarPatterns(ISeries<double> input, int idnumber)
		{
			if (cacheBarPatterns != null)
				for (int idx = 0; idx < cacheBarPatterns.Length; idx++)
					if (cacheBarPatterns[idx] != null && cacheBarPatterns[idx].Idnumber == idnumber && cacheBarPatterns[idx].EqualsInput(input))
						return cacheBarPatterns[idx];
			return CacheIndicator<BarPatterns>(new BarPatterns(){ Idnumber = idnumber }, input, ref cacheBarPatterns);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BarPatterns BarPatterns(int idnumber)
		{
			return indicator.BarPatterns(Input, idnumber);
		}

		public Indicators.BarPatterns BarPatterns(ISeries<double> input , int idnumber)
		{
			return indicator.BarPatterns(input, idnumber);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BarPatterns BarPatterns(int idnumber)
		{
			return indicator.BarPatterns(Input, idnumber);
		}

		public Indicators.BarPatterns BarPatterns(ISeries<double> input , int idnumber)
		{
			return indicator.BarPatterns(input, idnumber);
		}
	}
}

#endregion
