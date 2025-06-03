
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
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{

	[Description("Shows the currency value of the rightmost bar on the visible chart")]
	public class BarValueAnalyzer : Indicator
	{
        #region Variables
        // Wizard generated variables
		private string OutStr;
		private double pCurrencyPerTick = 12.5;
		private bool pShowTimeOfBar = false;
		private int tpLR = 90;
		private int tpTB = 90;
		private float			noTickTextWidth		= 0;
		private float			noTickTextHeight	= 0;
		private float			noConTextWidth		= 0;
		private float			noConTextHeight		= 0;
		private Brush			textBrush			= null;
		private NinjaTrader.Gui.Tools.SimpleFont textFont			= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 30);
		private float			textWidth			= 0;
		private float			textHeight			= 0;
		private float			tx=0,ty=0;
		private string FS = "0";
        #endregion

	protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			Name = "iw BarValueAnalyzer";
			var IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
			IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
			if(!IsDebug)
				VendorLicense("IndicatorWarehouse", "IWfree7", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
			Calculate=Calculate.OnBarClose;
			IsOverlay=true;
		}
		if(State == State.Configure){
			for(int i = 1; i<=pRoundingDigits; i++) {
				if(i == 1) FS = FS+".0"; else FS = FS+"0";
			}
		}
	}

	protected override void OnBarUpdate()
	{
	}
//=======================================================================================================

	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;double min = chartScale.MinValue; double max = chartScale.MaxValue;
		base.OnRender(chartControl, chartScale);
//		Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
//		Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
//		int firstBarPainted = ChartBars.FromIndex;
		int lastBarPainted = ChartBars.ToIndex;


		if (Bars == null || ChartControl == null)
			return;
		int bar  = Math.Min(CurrentBar,lastBarPainted);
		double H = High.GetValueAt(bar);
		double L = Low.GetValueAt(bar);
		double value = pCurrencyPerTick*Math.Round((H-L)/TickSize,0);
		if(pShowTimeOfBar)
			OutStr = Time.GetValueAt(bar).ToShortTimeString().ToLower()+" BarValue "+value.ToString(FS);
		else
			OutStr = "BarValue "+value.ToString(FS);

			// Recalculate the proper string size should the chart control object font and axis color change
		if (textBrush == null)
		{
			textBrush = ChartControl.Properties.AxisPen.Brush;
			textFont = ChartControl.Properties.LabelFont;
		}
//		SizeF size = RenderTarget.MeasureString(OutStr, textFont);
		var textFormat = new SharpDX.DirectWrite.TextFormat(
		        Core.Globals.DirectWriteFactory,
		        textFont.FamilySerialize,
		        textFont.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
		        textFont.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
		        SharpDX.DirectWrite.FontStretch.Normal,
		        (float)textFont.Size
        );
	    var textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, OutStr, textFormat, float.MaxValue, float.MaxValue);
		textWidth  = textLayout.Metrics.Width/2;
		textHeight = textLayout.Metrics.Height + 10;

		double pp = 100.0/TextPositionLR;
		tx = (float) ((double)ChartPanel.W / pp);
		pp = 100.0/TextPositionTB;
		ty = (float) ((double)ChartPanel.H / pp);
		tx = ChartPanel.X + tx - textWidth;
		ty = ChartPanel.Y + ty - textHeight;
		if(tx > ChartPanel.W-ChartPanel.X - textWidth) tx = ChartPanel.W - textWidth*2;
		if(tx < textWidth) tx = ChartPanel.X;
		if(ty > ChartPanel.H-5-textHeight) ty = ChartPanel.H-textHeight-5;
		if(ty < 5+textHeight) ty = textHeight+5;

		var txtDXBrush = textBrush.ToDxBrush(RenderTarget);
		RenderTarget.DrawTextLayout(new SharpDX.Vector2(tx, ty), textLayout, txtDXBrush);

		txtDXBrush.Dispose(); txtDXBrush = null;
		textLayout.Dispose(); textLayout = null;
		textFormat.Dispose(); textFormat = null;
	}

        #region Properties

        [Description("Enter the currency value of 1 tick.  Example:  Enter 12.5 if this is an ES chart and you want to use USD as the currency")]
        [Category("Parameters")]
        public double CurrencyPerTick
        {
            get { return pCurrencyPerTick; }
            set { pCurrencyPerTick = Math.Max(0, value); }
        }

		private int pRoundingDigits = 2;
        [Description("Enter the number of digits for rounding")]
        [Category("Parameters")]
        public int RoundingDigits
        {
            get { return pRoundingDigits; }
            set { pRoundingDigits = Math.Max(0, value); }
        }

        [Description("Show the timestamp of the bar along with its value?")]
        [Category("Parameters")]
        public bool ShowTimeOfBar
        {
            get { return pShowTimeOfBar; }
            set { pShowTimeOfBar = value; }
        }

        [Description("Text position 0=far left, 100=far right")]
        [Category("Visual")]
        public int TextPositionLR
        {
            get { return tpLR; }
            set { tpLR = Math.Min(100,Math.Max(0, value)); }
        }

        [Description("Text position 0=top, 100=bottom")]
        [Category("Visual")]
        public int TextPositionTB
        {
            get { return tpTB; }
            set { tpTB = Math.Min(100,Math.Max(0, value)); }
        }

		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BarValueAnalyzer[] cacheBarValueAnalyzer;
		public BarValueAnalyzer BarValueAnalyzer()
		{
			return BarValueAnalyzer(Input);
		}

		public BarValueAnalyzer BarValueAnalyzer(ISeries<double> input)
		{
			if (cacheBarValueAnalyzer != null)
				for (int idx = 0; idx < cacheBarValueAnalyzer.Length; idx++)
					if (cacheBarValueAnalyzer[idx] != null &&  cacheBarValueAnalyzer[idx].EqualsInput(input))
						return cacheBarValueAnalyzer[idx];
			return CacheIndicator<BarValueAnalyzer>(new BarValueAnalyzer(), input, ref cacheBarValueAnalyzer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BarValueAnalyzer BarValueAnalyzer()
		{
			return indicator.BarValueAnalyzer(Input);
		}

		public Indicators.BarValueAnalyzer BarValueAnalyzer(ISeries<double> input )
		{
			return indicator.BarValueAnalyzer(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BarValueAnalyzer BarValueAnalyzer()
		{
			return indicator.BarValueAnalyzer(Input);
		}

		public Indicators.BarValueAnalyzer BarValueAnalyzer(ISeries<double> input )
		{
			return indicator.BarValueAnalyzer(input);
		}
	}
}

#endregion
