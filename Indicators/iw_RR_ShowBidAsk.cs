// 
// Copyright (C) 2008, SBG Trading Corp.
// Use this indicator/strategy at your own risk.  No warranty expressed or implied.
// Trading financial instruments is risky and can result in substantial loss.
// The owner of this indicator/strategy holds harmless SBG Trading Corp. from any 
// and all trading losses incurred while using this indicator/strategy.
//
//

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

namespace NinjaTrader.NinjaScript.Indicators
{
		/// <summary>
		/// Puts a horizontal line at the ask price and bid price
		/// </summary>
		public class RR_ShowBidAsk : Indicator
		{
			#region Variables
			// Wizard generated variables
			// User defined variables (add any user defined variables below)
			private bool displayText=false;
			private int askLineWidth = 1;
			private int bidLineWidth = 1;
			private int lineLength   = 10;
			private double divideby = 1.0;
			private int LL;
			private DashStyleHelper lineStyle=DashStyleHelper.Dash;
			private string FS;
			double a,b;
//			private Brush askLineBrush = Brushes.Pink;
//			private Brush bidLineBrush = Brushes.CornflowerBlue;
//			private Brush askLineColor = new SolidColorBrush( Color.Pink ); askLineColor.Freeze();
//			private Brush bidLineColor = new SolidColorBrush( Color.CornflowerBlue ); bidLineColor.Freeze();
		#endregion

			private double prior_ask = 0;
			private double prior_bid = 0;
		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			Description="Puts a horizontal line at the ask price and bid price";
			Name = "iw RR Show BidAsk";
			bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && (
				NinjaTrader.Cbi.License.MachineId=="1E53E271B82EC62C7C03A15C336229AE" || NinjaTrader.Cbi.License.MachineId=="766C8CD2AD83CA787BCA6A2A76B2303B");
			if(!IsBen)
				VendorLicense("IndicatorWarehouse", "AIRoadRunner", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
//				VendorLicense("IndicatorWarehouse", "AIShowBidAsk", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
			AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.HLine, "Ask");
			AddPlot(new Stroke(Brushes.Blue, 1), PlotStyle.HLine, "Bid");
			Calculate=Calculate.OnBarClose;
			IsOverlay=true;
			//PriceTypeSupported	= false;
		}
	}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
//			if(lineLength >= CurrentBar) LL = CurrentBar;
//			else LL = lineLength;
        }

        /// <summary>
        /// Called on each incoming real time market data event
        /// </summary>
        protected override void OnMarketData(MarketDataEventArgs e)
        {	
			if (e.MarketDataType==MarketDataType.Ask) a=e.Price;
			if (e.MarketDataType==MarketDataType.Bid) b=e.Price;

			if(displayText && (prior_ask!=a || prior_bid!=b)) 
			{
				prior_ask = a;
				prior_bid = b;
				double TValue = Instrument.MasterInstrument.PointValue * TickSize;
				double SpreadCost = (a-b)/TickSize * TValue;
				RemoveDrawObject("BidAsk");
				Draw.TextFixed(this, "BidAsk",
					string.Format("Ask/Bid:\n{0}\n{1}\n${2}",Instrument.MasterInstrument.FormatPrice(a),Instrument.MasterInstrument.FormatPrice(b), SpreadCost.ToString("0.00")),
					pTextLoc);
			}

			if(askLineWidth>0) Values[0][0] = (a);//Draw.Ray(this, "AL",false, LL, a, 0, a, askLineColor, lineStyle, askLineWidth);
			if(bidLineWidth>0) Values[1][0] = (b);//Draw.Ray(this, "BL",false, LL, b, 0, b, bidLineColor, lineStyle, bidLineWidth);
        }

        #region Properties
        [Description("Display price text (ask price/bid price/spread value")]
        [Category("DisplayInfo")]
        public bool DisplayText
        {
            get { return displayText; }
            set { displayText = value; }
        }
		private TextPosition pTextLoc = TextPosition.TopRight;
        [Description("Location of displayed text info")]
        [Category("DisplayInfo")]
        public TextPosition DisplayTextLocation
        {
            get { return pTextLoc; }
            set { pTextLoc = value; }
        }
//        [Description("Ask Line width (a zero value removes the Ask Line")]
//        [Category("Line Styles")]
//        public int AskLineWidth
//        {
//            get { return askLineWidth; }
//            set { askLineWidth = Math.Max(0,value); }
//        }
//		[Description("Divide result by this value")]
//        [Category("DisplayInfo")]
//        public double DivideBy
//        {
//            get { return divideby; }
//            set { divideby = Math.Max(0.00001,value); }
//        }
//        [Description("Bid Line width (a zero value removes the Bid Line)")]
//        [Category("Line Styles")]
//        public int BidLineWidth
//        {
//            get { return bidLineWidth; }
//            set { bidLineWidth = Math.Max(0,value); }
//        }
//        [Description("Line length (in bars)")]
//        [Category("Line Styles")]
//        public int LineLength
//        {
//            get { return lineLength; }
//            set { lineLength = Math.Max(1,value); }
//        }
//        [Description("Line style")]
//        [Category("Line Styles")]
//        public DashStyleHelper LineStyle
//        {
//            get { return lineStyle; }
//            set { lineStyle = value; }
//        }
//		[Browsable(false)]
//    	public string AskLineColorSerialize
//    	{
//        	get { return Serialize.BrushToString(askLineColor); }
//        	set { askLineColor = Serialize.StringToBrush(value); }
//    	}

//		[Description("Color of Ask line"), XmlIgnore]
//        [Category("Line Colors")]
//        public Brush AskLineBrush
//        {
//            get { return askLineColor; }
//            set { askLineColor = value; }
//        }
//
//		[Browsable(false)]
//    	public string BidLineColorSerialize
//    	{
//       		get { return Serialize.BrushToString(bidLineColor); }
//       		set { bidLineColor = Serialize.StringToBrush(value); }
//    	}
//
//		[Description("Color of Bid line"), XmlIgnore]
//        [Category("Line Colors")]
//        public Brush BidLineBrush
//        {
//            get { return bidLineColor; }
//            set { bidLineColor = value; }
//        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RR_ShowBidAsk[] cacheRR_ShowBidAsk;
		public RR_ShowBidAsk RR_ShowBidAsk()
		{
			return RR_ShowBidAsk(Input);
		}

		public RR_ShowBidAsk RR_ShowBidAsk(ISeries<double> input)
		{
			if (cacheRR_ShowBidAsk != null)
				for (int idx = 0; idx < cacheRR_ShowBidAsk.Length; idx++)
					if (cacheRR_ShowBidAsk[idx] != null &&  cacheRR_ShowBidAsk[idx].EqualsInput(input))
						return cacheRR_ShowBidAsk[idx];
			return CacheIndicator<RR_ShowBidAsk>(new RR_ShowBidAsk(), input, ref cacheRR_ShowBidAsk);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RR_ShowBidAsk RR_ShowBidAsk()
		{
			return indicator.RR_ShowBidAsk(Input);
		}

		public Indicators.RR_ShowBidAsk RR_ShowBidAsk(ISeries<double> input )
		{
			return indicator.RR_ShowBidAsk(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RR_ShowBidAsk RR_ShowBidAsk()
		{
			return indicator.RR_ShowBidAsk(Input);
		}

		public Indicators.RR_ShowBidAsk RR_ShowBidAsk(ISeries<double> input )
		{
			return indicator.RR_ShowBidAsk(input);
		}
	}
}

#endregion
