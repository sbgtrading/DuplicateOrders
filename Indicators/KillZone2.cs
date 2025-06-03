// TradingStudies.com
// info@tradingStudies.com
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Gui;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class KillZone2 : Indicator
    {
        private bool paintBars = true;
        private int period = 8;
        private Series<bool> signal;
		private DM dm;

        protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
        {
			Name = " EMini School Kill Zone";
            //VendorLicense("eMiniSchool", "eMiniSchool", "www.eminischool.com", "trade@eminischool.com");
			AddPlot(new Stroke(System.Windows.Media.Brushes.Blue, 2), PlotStyle.Dot, "Peak");
			AddPlot(new Stroke(System.Windows.Media.Brushes.Yellow, 2), PlotStyle.Dot, "Valley");
			AddPlot(new Stroke(System.Windows.Media.Brushes.LimeGreen, 2), PlotStyle.Line, "Up");
			AddPlot(new Stroke(System.Windows.Media.Brushes.Red, 2), PlotStyle.Line, "Down");
			AddPlot(new Stroke(System.Windows.Media.Brushes.LimeGreen, 2), PlotStyle.Bar, "UpHisto");
			AddPlot(new Stroke(System.Windows.Media.Brushes.Red, 2), PlotStyle.Bar, "DownHisto");
            AddLine(new Stroke(System.Windows.Media.Brushes.Black, DashStyleHelper.Solid, 1), 135, "UpperThreshold");
            AddLine(new Stroke(System.Windows.Media.Brushes.Black, DashStyleHelper.Solid, 1), 0, "LowerThreshold");
            AddLine(new Stroke(System.Windows.Media.Brushes.Black, DashStyleHelper.Solid, 1), 0, "ZeroLine");
			IsOverlay = false;
			Calculate = Calculate.OnPriceChange;
        }else if(State == State.DataLoaded){
            signal = new Series<bool>(this);
			dm = DM(period);
		}
	}

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 3)
                return;
//            if (paintBars)
 //               CandleOutlineBrush = CandleOutlineBrushes[0];
			double v0 = (dm[0] - 18) * 3.848;
			double v1 = (dm[1] - 18) * 3.848;
            if (dm.DiPlus[0] > dm.DiMinus[0])
            {
                signal[0] = (true);
                if (Plots[2].PlotStyle == PlotStyle.Line);
                    Values[2][1] = v1;
                Values[2][0] = v0;
                Values[4][0] = v0;
                if (paintBars)
                    BarBrush = Plots[2].Pen.Brush;
            }
            else
            {
                signal[0] = (false);
                if (Plots[3].PlotStyle == PlotStyle.Line);
                    Values[3][1] = v1;
                Values[3][0] = v0;
                Values[5][0] = v0;
                if (paintBars)
                    BarBrush = Plots[3].Pen.Brush;
            }
			
			if(v0 > Lines[0].Value)
			{
				Values[0][0] = (0);
				if(paintBars)
                  BarBrush = Plots[0].Pen.Brush;
			}
			else
				if(v0 < Lines[1].Value)
				{
					Values[1][0] = (0);
					if(paintBars)
					 BarBrush = Plots[1].Pen.Brush;
				}
        }

        #region Properties

//        [Description("")]
//        [Category("Parameters")]
//        public int Period
//        {
//            get { return _period; }
//            set { _period = Math.Max(1, value); }
//        }

        [Description("")]
        [Category("Visual")]
        public bool PaintBars
        {
            get { return paintBars; }
            set { paintBars = value; }
        }

        [Browsable(false)] 
        [XmlIgnore] 
        public Series<bool> Signal
        {
            get { return signal; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private KillZone2[] cacheKillZone2;
		public KillZone2 KillZone2()
		{
			return KillZone2(Input);
		}

		public KillZone2 KillZone2(ISeries<double> input)
		{
			if (cacheKillZone2 != null)
				for (int idx = 0; idx < cacheKillZone2.Length; idx++)
					if (cacheKillZone2[idx] != null &&  cacheKillZone2[idx].EqualsInput(input))
						return cacheKillZone2[idx];
			return CacheIndicator<KillZone2>(new KillZone2(), input, ref cacheKillZone2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.KillZone2 KillZone2()
		{
			return indicator.KillZone2(Input);
		}

		public Indicators.KillZone2 KillZone2(ISeries<double> input )
		{
			return indicator.KillZone2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.KillZone2 KillZone2()
		{
			return indicator.KillZone2(Input);
		}

		public Indicators.KillZone2 KillZone2(ISeries<double> input )
		{
			return indicator.KillZone2(input);
		}
	}
}

#endregion
