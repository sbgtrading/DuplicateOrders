#region Using declarations
using System;
using System.Drawing;
using System.Collections;
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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DirectWrite;
//using System.Windows.Forms;

using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
#endregion

#region Enums
public enum FootPrintBarEnum
{
	BidAsk,
	VolumeDelta
}
public enum FootPrintBarColorEnum
{
	Saturation,
	VolumeBar,
	Solid,
	None
}
public enum ClosePriceEnum
{
	TextColor,
	Rectangle,
	None
}
public enum HighestVolumeEnum
{
	Rectangle,
	None
}
#endregion
	
//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class FootPrintChart : Indicator
	{
		#region Variable and Structure Declarations
 
		private int barSpacing = 100;
		private int barWidth = 43;
		private bool setChartProperties = true;
		private string displayName = null;
		
		FootPrintBarEnum footPrintBarType = FootPrintBarEnum.BidAsk;
		FootPrintBarColorEnum footPrintBarColor = FootPrintBarColorEnum.Saturation;
		ClosePriceEnum closePriceIndicator = ClosePriceEnum.TextColor;
		HighestVolumeEnum highestVolumeIndicator = HighestVolumeEnum.Rectangle;
		NinjaTrader.Gui.Tools.SimpleFont textFont = new NinjaTrader.Gui.Tools.SimpleFont("Consolas", 12);

        double fontOffset = 0.0;
        double rectangleOffset = 0.0;
		
        GlyphTypeface gtf = new GlyphTypeface();
        TextFormat footPrintBarTextFormat = null;

        public class ABV
        {
            public double Price { get; set; }
            public double askVolume { get; set; }
            public double bidVolume { get; set; }
			public double Volume { get; set; }
            public int Id { get; set; }
        }
				
		public struct BidAskVolume
		{
			public double currentVolume;
			public double askVolume;
			public double bidVolume;
			
			public BidAskVolume(double cv, double av, double bv)
			{
				currentVolume = cv;
				askVolume = av;
				bidVolume = bv;
			}
		}
		
		private List<ABV> baseBAVList = new List<ABV>();
		private Dictionary<double, BidAskVolume> bidAskVolume = new Dictionary<double, BidAskVolume>();
        
		double tmpAskVolume;
		double tmpBidVolume;
		double tmpCurrentVolume;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Displays bid/ask volumes next to candlestick bars.";
				Name								= "FootPrintChart";
				Calculate							= Calculate.OnEachTick;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= ScaleJustification.Right;
				IsAutoScale 						= true;
				IsSuspendedWhileInactive			= true;
				
				// default color values
				TotalDeltaDownBrush 					= System.Windows.Media.Brushes.IndianRed;
				TotalDeltaUpBrush 						= System.Windows.Media.Brushes.LimeGreen;
				TotalDeltaFontBrush 					= System.Windows.Media.Brushes.Black;
				FootPrintBarClosePriceBrush		 		= System.Windows.Media.Brushes.Gold;
				FootPrintBarHighestVolumeBrush 			= System.Windows.Media.Brushes.Wheat;
				FootPrintBarTextBrush 					= System.Windows.Media.Brushes.WhiteSmoke;
				FootPrintBarUpBrush 					= System.Windows.Media.Brushes.LimeGreen;
				FootPrintBarDownBrush 					= System.Windows.Media.Brushes.Red;
				
				// default boolean values
				ShowTotalDelta 				= true;				
				ShowCandleStickBars 		= true;
				ShowBodyBar					= false;
				ShowWicks 					= true;
			}
			else if (State == State.Configure)
			{
                //  freeze custom brushes
//                TotalDeltaDownBrush.Freeze();
//                TotalDeltaUpBrush.Freeze();
//                TotalDeltaFontBrush.Freeze();
//                FootPrintBarClosePriceBrush.Freeze();
//                FootPrintBarHighestVolumeBrush.Freeze();
//                FootPrintBarTextBrush.Freeze();
//                FootPrintBarUpBrush.Freeze();
//                FootPrintBarDownBrush.Freeze();

                setChartProperties = true;

                // create a better display name
                displayName = Name + String.Format(" ({0}, {1} {2}, BarType: {3}, Color Type: {4})", Instrument.FullName, BarsPeriod.Value, BarsPeriod.BarsPeriodType.ToString(), footPrintBarType.ToString(), footPrintBarColor.ToString());
			}
            else if (State == State.Terminated)
            {
                // disposing of text format
                if (footPrintBarTextFormat != null)
                    footPrintBarTextFormat.Dispose();
            }
		}

		public override string DisplayName
        {
            get { return (displayName != null ? displayName : Name); }
        }

		protected override void OnMarketData(MarketDataEventArgs e)
		{
            try
            {
                if (e.MarketDataType == MarketDataType.Last)
                {
                    if (IsFirstTickOfBar)
                    {
                        if (bidAskVolume.Count > 0)
                        {
                            foreach (KeyValuePair<double, BidAskVolume> kvp in bidAskVolume)
                            {
                                ABV tmp = new ABV();
                                tmp.Id = CurrentBar - 1;
                                tmp.Price = kvp.Key;
                                tmp.askVolume = kvp.Value.askVolume;
                                tmp.bidVolume = kvp.Value.bidVolume;
                                tmp.Volume = kvp.Value.currentVolume;
                                baseBAVList.Add(tmp);
                            }
                        }

                        bidAskVolume.Clear();
                    }

                    if (e.Price >= e.Ask)
                    {
                        if (bidAskVolume.ContainsKey(e.Price))
                        {
                            tmpBidVolume = bidAskVolume[e.Price].bidVolume;
                            tmpAskVolume = bidAskVolume[e.Price].askVolume + e.Volume;
                            tmpCurrentVolume = bidAskVolume[e.Price].currentVolume;

                            bidAskVolume[e.Price] = new BidAskVolume(tmpCurrentVolume, tmpAskVolume, tmpBidVolume);
                        }
                        else
                        {
                            tmpAskVolume = e.Volume;
                            tmpBidVolume = 0;
                            tmpCurrentVolume = e.Volume;

                            bidAskVolume.Add(e.Price, new BidAskVolume(tmpCurrentVolume, tmpAskVolume, tmpBidVolume));
                        }
                    }
                    else if (e.Price <= e.Bid)
                    {
                        if (bidAskVolume.ContainsKey(e.Price))
                        {

                            tmpBidVolume = bidAskVolume[e.Price].bidVolume + e.Volume;
                            tmpAskVolume = bidAskVolume[e.Price].askVolume;
                            tmpCurrentVolume = bidAskVolume[e.Price].currentVolume;

                            bidAskVolume[e.Price] = new BidAskVolume(tmpCurrentVolume, tmpAskVolume, tmpBidVolume);
                        }
                        else
                        {
                            tmpAskVolume = 0;
                            tmpBidVolume = e.Volume;
                            tmpCurrentVolume = e.Volume;

                            bidAskVolume.Add(e.Price, new BidAskVolume(tmpCurrentVolume, tmpAskVolume, tmpBidVolume));
                        }
                    }
                }
            }
            catch (Exception ex) {
				Print("OnMarketData failure FootPrint chart: " + ex.ToString());
			}
        }

		public override void OnCalculateMinMax()
		{			 
			// exit if ChartBars has not yet been initialized
			if (ChartBars == null)
				return;
			
            // int barLowPrice and barHighPrice to min and max double vals
            double barLowPrice = double.MaxValue;
            double barHighPrice = double.MinValue;

            // loop through the bars visable on the chart
            for (int index = ChartBars.FromIndex; index <= ChartBars.ToIndex; index++)
            {
                // get min/max of bar high/low values
                barLowPrice = Math.Min(barLowPrice, Low.GetValueAt(index));
                barHighPrice = Math.Max(barHighPrice, High.GetValueAt(index));
            }
			
			// number of ticks between high/low price
			double priceTicks = (barHighPrice - barLowPrice) * (1 / TickSize);
			
			// number of ticks on the chart panel based on the chart panel height and text font size
			double panelTicks = ChartPanel.H / (textFont.Size + 4);
			
			// number of ticks we'll need to add to high and low price to auto adjust the chart
			double ticksNeeded = (panelTicks - priceTicks) / 2;

			// calc min and max chart prices
			MinValue = barLowPrice - (ticksNeeded * TickSize);
            MaxValue = barHighPrice + (ticksNeeded * TickSize);
		}

//=============================================================================================================
		SharpDX.Direct2D1.Brush FootPrintBarUpBrushDX = null;
		SharpDX.Direct2D1.Brush FootPrintBarDownBrushDX = null;
		SharpDX.Direct2D1.Brush TotalDeltaDownBrushDX = null;
		SharpDX.Direct2D1.Brush TotalDeltaUpBrushDX = null;
		SharpDX.Direct2D1.Brush TotalDeltaFontBrushDX = null;
		SharpDX.Direct2D1.Brush FootPrintBarClosePriceBrushDX = null;
		SharpDX.Direct2D1.Brush FootPrintBarHighestVolumeBrushDX = null;
		SharpDX.Direct2D1.Brush FootPrintBarTextBrushDX = null;
		SharpDX.Direct2D1.Brush CandleStickUpBrushDX = null;
		SharpDX.Direct2D1.Brush CandleStickDownBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(TotalDeltaUpBrushDX!=null && !TotalDeltaUpBrushDX.IsDisposed) {TotalDeltaUpBrushDX.Dispose(); TotalDeltaUpBrushDX=null;}
			if(RenderTarget!=null) TotalDeltaUpBrushDX = TotalDeltaUpBrush.ToDxBrush(RenderTarget);
			if(TotalDeltaDownBrushDX!=null && !TotalDeltaDownBrushDX.IsDisposed) {TotalDeltaDownBrushDX.Dispose(); TotalDeltaDownBrushDX=null;}
			if(RenderTarget!=null) TotalDeltaDownBrushDX = TotalDeltaDownBrush.ToDxBrush(RenderTarget);

			if(FootPrintBarClosePriceBrushDX!=null && !FootPrintBarClosePriceBrushDX.IsDisposed) {FootPrintBarClosePriceBrushDX.Dispose(); FootPrintBarClosePriceBrushDX=null;}
			if(RenderTarget!=null) FootPrintBarClosePriceBrushDX = FootPrintBarClosePriceBrush.ToDxBrush(RenderTarget);

			if(FootPrintBarHighestVolumeBrushDX!=null && !FootPrintBarHighestVolumeBrushDX.IsDisposed) {FootPrintBarHighestVolumeBrushDX.Dispose(); FootPrintBarHighestVolumeBrushDX=null;}
			if(RenderTarget!=null) FootPrintBarHighestVolumeBrushDX = FootPrintBarHighestVolumeBrush.ToDxBrush(RenderTarget);

			if(FootPrintBarUpBrushDX!=null && !FootPrintBarUpBrushDX.IsDisposed) {FootPrintBarUpBrushDX.Dispose(); FootPrintBarUpBrushDX=null;}
			if(RenderTarget!=null) FootPrintBarUpBrushDX = FootPrintBarUpBrush.ToDxBrush(RenderTarget);
			if(FootPrintBarDownBrushDX!=null && !FootPrintBarDownBrushDX.IsDisposed) {FootPrintBarDownBrushDX.Dispose(); FootPrintBarDownBrushDX=null;}
			if(RenderTarget!=null) FootPrintBarDownBrushDX = FootPrintBarDownBrush.ToDxBrush(RenderTarget);

			if(FootPrintBarTextBrushDX!=null && !FootPrintBarTextBrushDX.IsDisposed) {FootPrintBarTextBrushDX.Dispose(); FootPrintBarTextBrushDX=null;}
			if(RenderTarget!=null) FootPrintBarTextBrushDX = FootPrintBarTextBrush.ToDxBrush(RenderTarget);

			if(ChartBars!=null){
				if(CandleStickUpBrushDX!=null && !CandleStickUpBrushDX.IsDisposed) {CandleStickUpBrushDX.Dispose(); CandleStickUpBrushDX=null;}
				if(RenderTarget!=null) CandleStickUpBrushDX = ChartBars.Properties.ChartStyle.UpBrush.ToDxBrush(RenderTarget);
				if(CandleStickDownBrushDX!=null && !CandleStickDownBrushDX.IsDisposed) {CandleStickDownBrushDX.Dispose(); CandleStickDownBrushDX=null;}
				if(RenderTarget!=null) CandleStickDownBrushDX = ChartBars.Properties.ChartStyle.DownBrush.ToDxBrush(RenderTarget);
			}
		}
//=============================================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
            try
            {
                // set the bar spacing, chart width, and some vars, we only want to do this once and not on every render
                if (setChartProperties)
                {
                    chartControl.Properties.BarDistance = barSpacing;
                    chartControl.BarWidth = barWidth;

                    // create the TextFormat structure for the footprint bars
                    footPrintBarTextFormat = new TextFormat(new SharpDX.DirectWrite.Factory(),
                                                                 textFont.Family.ToString(),
                                                                 textFont.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
                                                                 textFont.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
                                                                 (float)textFont.Size);
                    
                    System.Windows.Media.Typeface t_face = new System.Windows.Media.Typeface(new System.Windows.Media.FontFamily(textFont.Family.ToString()), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                    t_face.TryGetGlyphTypeface(out gtf);

                    // chart drawing starts from the current price line down, so for our footprint rectangles we need to offset our footprint bar drawings up a bit so they align with the price marker
                    fontOffset = (gtf.CapsHeight * textFont.Size) * 0.6 * 1.8;
                    rectangleOffset = (gtf.CapsHeight * textFont.Size) * 1.8;

                    setChartProperties = false;
                }

                // vectors and rectangle for the candlestick bars
                Vector2 point0 = new Vector2();
                Vector2 point1 = new Vector2();
                SharpDX.RectangleF rect = new SharpDX.RectangleF();

                for (int chartBarIndex = ChartBars.FromIndex; chartBarIndex <= ChartBars.ToIndex; chartBarIndex++)
                {
                    // current bar prices
                    double barClosePrice = ChartBars.Bars.GetClose(chartBarIndex);
                    double barOpenPrice = ChartBars.Bars.GetOpen(chartBarIndex);
                    double barHighPrice = ChartBars.Bars.GetHigh(chartBarIndex);
                    double barLowPrice = ChartBars.Bars.GetLow(chartBarIndex);

                    // current bar X and Y points
                    int x = chartControl.GetXByBarIndex(ChartBars, chartBarIndex) - (int)chartControl.BarWidth;
                    float barX = chartControl.GetXByBarIndex(ChartBars, chartBarIndex);
                    float barOpenY = chartScale.GetYByValue(barOpenPrice);
                    float barCloseY = chartScale.GetYByValue(barClosePrice);
                    float barHighY = chartScale.GetYByValue(barHighPrice);
                    float barLowY = chartScale.GetYByValue(barLowPrice);

                    // get the ABV list for this chart bar
                    IEnumerable<ABV> currentBAVList = baseBAVList.Where(p => p.Id == chartBarIndex);

                    // init totalDelta and maxVolumePrice
                    double totalDelta = 0d;
                    double maxVolumePrice = 0d;

                    #region FootPrint Bars
                    if (chartBarIndex == ChartBars.Count - 1)
                    {
                        double maxVolume = double.MinValue;
                        double barVolume = 0;
                        double barDelta = 0;
                        double maxAskVolume = double.MinValue;
                        double maxBidVolume = double.MinValue;

                        foreach (KeyValuePair<double, BidAskVolume> kvp in bidAskVolume)
                        {
                            if ((kvp.Value.askVolume + kvp.Value.bidVolume) > maxVolume)
                            {
                                maxVolume = kvp.Value.askVolume + kvp.Value.bidVolume;
                                maxVolumePrice = kvp.Key;
                            }

                            if (footPrintBarType == FootPrintBarEnum.BidAsk)
                            {
                                maxBidVolume = Math.Max(maxBidVolume, kvp.Value.bidVolume);
                                maxAskVolume = Math.Max(maxAskVolume, kvp.Value.askVolume);
                            }
                            else
                            {
                                maxBidVolume = Math.Max(maxBidVolume, (kvp.Value.askVolume + kvp.Value.bidVolume));
                                maxAskVolume = Math.Max(maxAskVolume, (kvp.Value.askVolume - kvp.Value.bidVolume));
                            }
                        }

                        foreach (KeyValuePair<double, BidAskVolume> kvp in bidAskVolume)
                        {
                            int y = chartScale.GetYByValue(kvp.Key) - (int)(fontOffset);

                            // create totalDelta, currentVolume, and delta values
                            totalDelta += kvp.Value.askVolume;
                            totalDelta -= kvp.Value.bidVolume;
                            barVolume = kvp.Value.askVolume + kvp.Value.bidVolume;
                            barDelta = kvp.Value.askVolume - kvp.Value.bidVolume;

                            // determine the bar opacity
                            double curr_percent = 100 * (barVolume / maxVolume);
                            double curr_opacity = Math.Round((curr_percent / 100) * 0.8, 1);
                            curr_opacity = curr_opacity == 0 ? 0.1 : curr_opacity;

                            // set the color based on the volume direction
                            char footPrintBarBrush = 'U';
                            if (kvp.Value.askVolume < kvp.Value.bidVolume)
                                footPrintBarBrush = 'D';

                            if (FootPrintBarColor == FootPrintBarColorEnum.VolumeBar)
                            {
                                double ratioAsk = 0d;
                                double ratioBid = 0d;

                                if (maxAskVolume != 0)
                                    ratioAsk = 1f - (kvp.Value.askVolume / maxAskVolume);

                                if (maxBidVolume != 0)
                                    ratioBid = 1f - (kvp.Value.bidVolume / maxBidVolume);

                                // determine the width of the rectangle based on the bid/ask volume
                                double width = (chartControl.BarWidth - (chartControl.BarWidth * ratioBid)) + (chartControl.BarWidth - (chartControl.BarWidth * ratioAsk));

                                RenderTarget.FillRectangle(new SharpDX.RectangleF(x + (float)(chartControl.BarWidth * ratioBid), y, (float)(width), (float)(rectangleOffset)), footPrintBarBrush=='U'? FootPrintBarUpBrushDX : FootPrintBarDownBrushDX);
                            }
                            else if (FootPrintBarColor == FootPrintBarColorEnum.Saturation)
                            {
                                FootPrintBarUpBrushDX.Opacity = (float)curr_opacity;
                                FootPrintBarDownBrushDX.Opacity = (float)curr_opacity;
                                RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, (float)(chartControl.BarWidth * 2), (float)(rectangleOffset)), footPrintBarBrush=='U'? FootPrintBarUpBrushDX : FootPrintBarDownBrushDX);
                            }
                            else if (FootPrintBarColor == FootPrintBarColorEnum.Solid)
                            {
                                RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, (float)(chartControl.BarWidth * 2), (float)(rectangleOffset)), footPrintBarBrush=='U'? FootPrintBarUpBrushDX : FootPrintBarDownBrushDX);
                            }
                            FootPrintBarUpBrushDX.Opacity = 100f;
                            FootPrintBarDownBrushDX.Opacity = 100f;

                            // create the bid/ask or volume/delta strings to show on the chart
                            string bidStr = null;
                            string askStr = null;
                            if (footPrintBarType == FootPrintBarEnum.BidAsk)
                            {
                                bidStr = kvp.Value.bidVolume.ToString();
                                askStr = kvp.Value.askVolume.ToString();
                            }
                            else
                            {
                                bidStr = barVolume.ToString();
                                askStr = barDelta.ToString();
                            }

                            // draw the bid footprint bar string
                            footPrintBarTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;
                            if (kvp.Key == barClosePrice && closePriceIndicator == ClosePriceEnum.TextColor)
                                RenderTarget.DrawText(bidStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX - 5 - (float)chartControl.BarWidth, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarClosePriceBrushDX);
                            else
                                RenderTarget.DrawText(bidStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX - 5 - (float)chartControl.BarWidth, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarTextBrushDX);

                            // draw the ask footprint bar string
                            footPrintBarTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
                            if (kvp.Key == barClosePrice && closePriceIndicator == ClosePriceEnum.TextColor)
                                RenderTarget.DrawText(askStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX + 5, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarClosePriceBrushDX);
                            else
                                RenderTarget.DrawText(askStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX + 5, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarTextBrushDX);
                        }
                    }
                    else
                    {
                        double maxVolume = double.MinValue; ;
                        double barVolume = 0d;
                        double barDelta = 0d;
                        double maxAskVolume = double.MinValue;
                        double maxBidVolume = double.MinValue;

                        foreach (ABV t in currentBAVList)
                        {
                            if ((t.askVolume + t.bidVolume) > maxVolume)
                            {
                                maxVolume = t.askVolume + t.bidVolume;
                                maxVolumePrice = t.Price;
                            }

                            if (footPrintBarType == FootPrintBarEnum.BidAsk)
                            {
                                maxBidVolume = Math.Max(maxBidVolume, t.bidVolume);
                                maxAskVolume = Math.Max(maxAskVolume, t.askVolume);
                            }
                            else
                            {
                                maxBidVolume = Math.Max(maxBidVolume, (t.bidVolume + t.askVolume));
                                maxAskVolume = Math.Max(maxAskVolume, (t.askVolume - t.askVolume));
                            }
                        }

                        foreach (ABV t in currentBAVList)
                        {
                            // y value of where our drawing will start minus our fontOffset
                            int y = chartScale.GetYByValue(t.Price) - (int)(fontOffset);

                            // sum up the totalDelta, currentVolume, and delta values
                            totalDelta += t.askVolume;
                            totalDelta -= t.bidVolume;
                            barVolume = t.askVolume + t.bidVolume;
                            barDelta = t.askVolume - t.bidVolume;

                            double curr_percent = 100 * (barVolume / maxVolume);
                            double curr_opacity = Math.Round((curr_percent / 100) * 0.8, 1);
                            curr_opacity = curr_opacity == 0 ? 0.1 : curr_opacity;

                            // set the color based on the volume direction
                            char footPrintBarBrush = 'U';

                            if (t.askVolume < t.bidVolume)
                                footPrintBarBrush = 'D';

                            // draw the background color
                            if (FootPrintBarColor == FootPrintBarColorEnum.VolumeBar) {

                                double ratioAsk = 0;
                                double ratioBid = 0;

                                if (maxAskVolume != 0)
                                    ratioAsk = 1f - (t.askVolume / maxAskVolume);

                                if (maxBidVolume != 0)
                                    ratioBid = 1f - (t.bidVolume / maxBidVolume);

                                // determine the width of the rectangle based on the bid/ask volume
                                double width = (chartControl.BarWidth - (chartControl.BarWidth * ratioBid)) + (chartControl.BarWidth - (chartControl.BarWidth * ratioAsk));

                                RenderTarget.FillRectangle(new SharpDX.RectangleF(x + (float)(chartControl.BarWidth * ratioBid), y, (float)(width), (float)(rectangleOffset)), footPrintBarBrush=='U'? FootPrintBarUpBrushDX:FootPrintBarDownBrushDX);
                            }
                            else if (FootPrintBarColor == FootPrintBarColorEnum.Saturation)
                            {
                                FootPrintBarUpBrushDX.Opacity = (float)curr_opacity;
                                FootPrintBarDownBrushDX.Opacity = (float)curr_opacity;
                                RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, (float)(chartControl.BarWidth * 2), (float)(rectangleOffset)), footPrintBarBrush=='U'? FootPrintBarUpBrushDX:FootPrintBarDownBrushDX);
                            }
                            else if (FootPrintBarColor == FootPrintBarColorEnum.Solid)
                            {
                                RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, (float)(chartControl.BarWidth * 2), (float)(rectangleOffset)), footPrintBarBrush=='U'? FootPrintBarUpBrushDX:FootPrintBarDownBrushDX);
                            }
                            FootPrintBarUpBrushDX.Opacity = 100f;
                            FootPrintBarDownBrushDX.Opacity = 100f;

                            // create the bid/ask or volume/delta strings to show on the chart
                            string bidStr = null;
                            string askStr = null;
                            if (footPrintBarType == FootPrintBarEnum.BidAsk)
                            {
                                bidStr = t.bidVolume.ToString();
                                askStr = t.askVolume.ToString();
                            }
                            else
                            {
                                bidStr = barVolume.ToString();
                                askStr = barDelta.ToString();
                            }

                            // draw the bid footprint bar string
                            footPrintBarTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;
                            if (t.Price == barClosePrice && closePriceIndicator == ClosePriceEnum.TextColor)
                                RenderTarget.DrawText(bidStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX - 5 - (float)chartControl.BarWidth, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarClosePriceBrushDX);
                            else
                                RenderTarget.DrawText(bidStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX - 5 - (float)chartControl.BarWidth, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarTextBrushDX);

                            // draw the ask footprint bar string
                            footPrintBarTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading;
                            if (t.Price == barClosePrice && closePriceIndicator == ClosePriceEnum.TextColor)
                                RenderTarget.DrawText(askStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX + 5, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarClosePriceBrushDX);
                            else
                                RenderTarget.DrawText(askStr, footPrintBarTextFormat, new SharpDX.RectangleF(barX + 5, y, (float)chartControl.BarWidth, (float)(rectangleOffset)),
                                    FootPrintBarTextBrushDX);
                        }
                    }
                    #endregion

                    #region CandleStick Bars
                    if (ShowCandleStickBars || ShowBodyBar)
                    {
                        if (Math.Abs(barOpenY - barCloseY) < 0.0000001)
                        {
                            // draw doji bar if no movement between open and close
                            if (ShowCandleStickBars)
                            {
                                point0.X = x - 10;
                                point0.Y = barCloseY;
                                point1.X = x - 1;
                                point1.Y = barCloseY;

                                RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke.BrushDX, 1);
                            }

                            if (ShowBodyBar)
                            {
                                point0.X = barX - 3;
                                point0.Y = barCloseY;
                                point1.X = barX + 3;
                                point1.Y = barCloseY;

                                RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke.BrushDX, 1);
                            }
                        }
                        else if(CandleStickUpBrushDX!=null && CandleStickDownBrushDX!=null)
                        {
                            // set the candlestick color based on open and close price
                            char candleStickBrush = barClosePrice >= barOpenPrice ? 'U' : 'D';
                            if (ShowCandleStickBars)
                            {
                                rect.X = barX - (int)chartControl.BarWidth - 7;
                                rect.Y = Math.Min(barCloseY, barOpenY);
                                rect.Width = 4;
                                rect.Height = Math.Max(barOpenY, barCloseY) - Math.Min(barCloseY, barOpenY);

                                // draw the candlestick
                                RenderTarget.FillRectangle(rect, candleStickBrush == 'U' ? CandleStickUpBrushDX : CandleStickDownBrushDX);

                                // draw the candlestick outline color, the color and width come from the main chart properties
                                RenderTarget.DrawRectangle(rect, ChartBars.Properties.ChartStyle.Stroke.BrushDX, ChartBars.Properties.ChartStyle.Stroke.Width);

                            }

                            if (ShowBodyBar)
                            {
                                rect.X = barX - (int)chartControl.BarWidth;
                                rect.Y = Math.Min(barCloseY, barOpenY) - ((float)rectangleOffset / 2);
                                rect.Width = (float)(chartControl.BarWidth * 2);
                                rect.Height = (Math.Max(barOpenY, barCloseY) - Math.Min(barCloseY, barOpenY)) + (float)rectangleOffset;

                                // draw the candlestick outline color, the color and width come from the main chart properties
                                RenderTarget.DrawRectangle(rect, candleStickBrush == 'U' ? CandleStickUpBrushDX : CandleStickDownBrushDX, 2);
                            }
                        }

                        if (ShowWicks)
                        {
                            // high wick
                            if (barHighY < Math.Min(barOpenY, barCloseY))
                            {
                                if (ShowCandleStickBars)
                                {
                                    point0.X = barX - (float)(chartControl.BarWidth + 5);
                                    point0.Y = barHighY;
                                    point1.X = barX - (float)(chartControl.BarWidth + 5);
                                    point1.Y = Math.Min(barOpenY, barCloseY);

                                    // draw the high wick, the color and width come from the main chart properties
                                    RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke2.BrushDX, ChartBars.Properties.ChartStyle.Stroke2.Width);
                                }

                                if (ShowBodyBar)
                                {
                                    point0.X = barX;
                                    point0.Y = barHighY;
                                    point1.X = barX;

                                    if (Math.Abs(barOpenY - barCloseY) < 0.0000001)
                                        point1.Y = Math.Max(barOpenY, barCloseY);
                                    else
                                        point1.Y = Math.Max(barOpenY, barCloseY) + ((float)rectangleOffset / 2);

                                    // draw the high wick, the color and width come from the main chart properties
                                    RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke2.BrushDX, ChartBars.Properties.ChartStyle.Stroke2.Width);
                                }
                            }

                            // low wick
                            if (barLowY > Math.Max(barOpenY, barCloseY))
                            {
                                if (ShowCandleStickBars)
                                {
                                    point0.X = barX - (float)(chartControl.BarWidth + 5);
                                    point0.Y = barLowY;
                                    point1.X = barX - (float)(chartControl.BarWidth + 5);
                                    point1.Y = Math.Max(barOpenY, barCloseY);

                                    // draw the low wick, the color and width come from the main chart properties
                                    RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke2.BrushDX, ChartBars.Properties.ChartStyle.Stroke2.Width);
                                }

                                if (ShowBodyBar)
                                {
                                    point0.X = barX;
                                    point0.Y = barLowY;
                                    point1.X = barX;

                                    if (Math.Abs(barOpenY - barCloseY) < 0.0000001)
                                        point1.Y = Math.Max(barOpenY, barCloseY);
                                    else
                                        point1.Y = Math.Max(barOpenY, barCloseY) + ((float)rectangleOffset / 2);

                                    // draw the low wick, the color and width come from the main chart properties
                                    RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke2.BrushDX, ChartBars.Properties.ChartStyle.Stroke2.Width);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Draw Separator
                    // create a point to draw a vertical line in the footprint bar
                    if (!ShowBodyBar)
                    {
                        point0.X = barX;
                        point0.Y = barHighY - (float)fontOffset;
                        point1.X = barX;
                        point1.Y = barLowY + (float)fontOffset;

                        // draw the line separator, the color and width come from the main chart properties
                        RenderTarget.DrawLine(point0, point1, ChartBars.Properties.ChartStyle.Stroke2.BrushDX, ChartBars.Properties.ChartStyle.Stroke2.Width);
                    }
                    #endregion

                    #region Highest Volume Rectangle
                    if (highestVolumeIndicator == HighestVolumeEnum.Rectangle)
                        RenderTarget.DrawRectangle(new SharpDX.RectangleF(x, (chartScale.GetYByValue(maxVolumePrice) - (float)(fontOffset)), (float)(chartControl.BarWidth * 2), (float)(rectangleOffset)), FootPrintBarHighestVolumeBrushDX, 2);
                    #endregion

                    #region Total Delta Footer
                    if (ShowTotalDelta)
                    {
						char deltaBrush = 'U';
                        // set the background color based on the delta number
                        if (totalDelta < 0)
                            deltaBrush = 'D';

                        float xBar = chartControl.GetXByBarIndex(ChartBars, chartBarIndex - 1) + (float)(chartControl.BarWidth + 13);
                        RenderTarget.FillRectangle(new SharpDX.RectangleF(xBar, (float)(ChartPanel.H - 12), (float)(chartControl.BarWidth * 2), (float)rectangleOffset), deltaBrush == 'U' ? TotalDeltaUpBrushDX : TotalDeltaDownBrushDX);

                        // draw the total delta string
                        footPrintBarTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                        RenderTarget.DrawText(totalDelta.ToString(), footPrintBarTextFormat, new SharpDX.RectangleF(xBar, (float)(ChartPanel.H - 12), (float)(chartControl.BarWidth * 2), (float)rectangleOffset),
                                TotalDeltaFontBrushDX, DrawTextOptions.None, MeasuringMode.GdiClassic);
                    }
                    #endregion

                }
            }
            catch (Exception ex) {
				Print("OnRender failure FootPrint chart: " + ex.ToString());
			}
        }

		protected override void OnBarUpdate()
		{
			if (!Bars.IsTickReplay)
				Draw.TextFixed(this, "warning msg", "WARNING: Tick Replay must be enabled for FootPrintChart to display historical values.", TextPosition.TopRight);

			BarBrushes[0] = System.Windows.Media.Brushes.Transparent;
			CandleOutlineBrushes[0] = System.Windows.Media.Brushes.Transparent;
		}
		
		#region Properties
		#region Chart Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Bar Spacing", Description="Sets the space between the footprint bars.", Order=1, GroupName="1. Chart Properties")]
		public int BarSpacing
		{
			get { return barSpacing; }
			set {barSpacing = Math.Max(1, value);}
		}
	
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Bar Width", Description="Sets the width of the footprint bars.", Order=2, GroupName="1. Chart Properties")]
		public int BarWidth
		{
			get { return barWidth; }
			set {barWidth = Math.Max(1, value);}
		}
				
	    [NinjaScriptProperty]
	    [Display(Name = "Bar Font", Description = "Text font for the chart bars.", Order = 4, GroupName = "1. Chart Properties")]
	    public NinjaTrader.Gui.Tools.SimpleFont TextFont
	    {
	        get { return textFont; }
	        set { textFont = value; }
	    }
		#endregion
		
		#region CandleStick Properties
		[NinjaScriptProperty]
		[Display(Name="Show Left Bars", Order=1, Description = "Show the CandleStick Bars.", GroupName="2. CandleStick Bar Properties")]
		public bool ShowCandleStickBars
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Outline in FootPrint Bar", Order=2, Description = "Shows a candlestick outline in the FootPrint bar body.", GroupName="2. CandleStick Bar Properties")]
		public bool ShowBodyBar
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Bar Wicks", Order=3, Description = "Show High/Low Wicks", GroupName="2. CandleStick Bar Properties")]
		public bool ShowWicks
		{ get; set; }
		
		#endregion
		
		#region FootPrint Bar Properties
		[NinjaScriptProperty]
		[Display(Name="Bar Type", Description="Shows either Bid/Ask volume or Volume/Delta", Order=1, GroupName = "3. FootPrint Bar Properties")]
		public FootPrintBarEnum FootPrintBarType
		{
			get { return footPrintBarType; }
			set { footPrintBarType = value; }
		}

		[NinjaScriptProperty]
		[Display(Name="Bar Color", Description="Shows either a volume bar or a saturation color backgroud.", Order=2, GroupName = "3. FootPrint Bar Properties")]
		public FootPrintBarColorEnum FootPrintBarColor
		{
			get { return footPrintBarColor; }
			set { footPrintBarColor = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color", Description = "Color for the text string.", Order = 3, GroupName = "3. FootPrint Bar Properties")]
		public System.Windows.Media.Brush FootPrintBarTextBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string FootPrintBarTextBrushSerialize
		{
			get { return Serialize.BrushToString(FootPrintBarTextBrush); }
   			set { FootPrintBarTextBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Volume Up Bar Color", Description = "Color for the up volume bars.", Order = 4, GroupName = "3. FootPrint Bar Properties")]
		public System.Windows.Media.Brush FootPrintBarUpBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string FootPrintBarUpBrushSerialize
		{
			get { return Serialize.BrushToString(FootPrintBarUpBrush); }
   			set { FootPrintBarUpBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Volume Down Bar Color", Description = "Color for the down volume bars.", Order = 5, GroupName = "3. FootPrint Bar Properties")]
		public System.Windows.Media.Brush FootPrintBarDownBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string FootPrintBarDownBrushSerialize
		{
			get { return Serialize.BrushToString(FootPrintBarDownBrush); }
   			set { FootPrintBarDownBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Close Indicator", Description="Indicates the close price in text or rectangle.", Order=6, GroupName = "3. FootPrint Bar Properties")]
		public ClosePriceEnum ClosePriceIndicator
		{
			get { return closePriceIndicator; }
			set { closePriceIndicator = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Close Indicator Color", Description = "Color for the close indicator.", Order = 7, GroupName = "3. FootPrint Bar Properties")]
		public System.Windows.Media.Brush FootPrintBarClosePriceBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string FootPrintBarClosePriceColorSerialize
		{
			get { return Serialize.BrushToString(FootPrintBarClosePriceBrush); }
   			set { FootPrintBarClosePriceBrush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name="High Volume Indicator", Description="Indicates the high volume in text or rectangle.", Order=8, GroupName = "3. FootPrint Bar Properties")]
		public HighestVolumeEnum HighVolumeIndicator
		{
			get { return highestVolumeIndicator; }
			set { highestVolumeIndicator = value; }
		}
		
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Highest Volume Indicator Color", Description = "Color for the high volume rectangle.", Order = 9, GroupName = "3. FootPrint Bar Properties")]
		public System.Windows.Media.Brush FootPrintBarHighestVolumeBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string FootPrintBarHighestVolumeBrushSerialize
		{
			get { return Serialize.BrushToString(FootPrintBarHighestVolumeBrush); }
   			set { FootPrintBarHighestVolumeBrush = Serialize.StringToBrush(value); }
		}		
		#endregion
				
		#region Total Delta Properties
		[NinjaScriptProperty]
		[Display(Name="Show Total Delta", Order=1, GroupName="4. Total Delta Properties")]
		public bool ShowTotalDelta
		{ get; set; }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color", Order = 2, GroupName = "4. Total Delta Properties")]
		public System.Windows.Media.Brush TotalDeltaFontBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string DeltaFontColorSerialize
		{
			get { return Serialize.BrushToString(TotalDeltaFontBrush); }
   			set { TotalDeltaFontBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Background Color", Order = 3, GroupName = "4. Total Delta Properties")]
		public System.Windows.Media.Brush TotalDeltaUpBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string TotalDeltaUpBrushSerialize
		{
			get { return Serialize.BrushToString(TotalDeltaUpBrush); }
   			set { TotalDeltaUpBrush = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Background Color", Order = 4, GroupName = "4. Total Delta Properties")]
		public System.Windows.Media.Brush TotalDeltaDownBrush		
		{ get; set; }
		
		[Browsable(false)]
		public string TotalDeltaDownBrushSerialize
		{
			get { return Serialize.BrushToString(TotalDeltaDownBrush); }
   			set { TotalDeltaDownBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FootPrintChart[] cacheFootPrintChart;
		public FootPrintChart FootPrintChart(int barSpacing, int barWidth, NinjaTrader.Gui.Tools.SimpleFont textFont, bool showCandleStickBars, bool showBodyBar, bool showWicks, FootPrintBarEnum footPrintBarType, FootPrintBarColorEnum footPrintBarColor, ClosePriceEnum closePriceIndicator, HighestVolumeEnum highVolumeIndicator, bool showTotalDelta)
		{
			return FootPrintChart(Input, barSpacing, barWidth, textFont, showCandleStickBars, showBodyBar, showWicks, footPrintBarType, footPrintBarColor, closePriceIndicator, highVolumeIndicator, showTotalDelta);
		}

		public FootPrintChart FootPrintChart(ISeries<double> input, int barSpacing, int barWidth, NinjaTrader.Gui.Tools.SimpleFont textFont, bool showCandleStickBars, bool showBodyBar, bool showWicks, FootPrintBarEnum footPrintBarType, FootPrintBarColorEnum footPrintBarColor, ClosePriceEnum closePriceIndicator, HighestVolumeEnum highVolumeIndicator, bool showTotalDelta)
		{
			if (cacheFootPrintChart != null)
				for (int idx = 0; idx < cacheFootPrintChart.Length; idx++)
					if (cacheFootPrintChart[idx] != null && cacheFootPrintChart[idx].BarSpacing == barSpacing && cacheFootPrintChart[idx].BarWidth == barWidth && cacheFootPrintChart[idx].TextFont == textFont && cacheFootPrintChart[idx].ShowCandleStickBars == showCandleStickBars && cacheFootPrintChart[idx].ShowBodyBar == showBodyBar && cacheFootPrintChart[idx].ShowWicks == showWicks && cacheFootPrintChart[idx].FootPrintBarType == footPrintBarType && cacheFootPrintChart[idx].FootPrintBarColor == footPrintBarColor && cacheFootPrintChart[idx].ClosePriceIndicator == closePriceIndicator && cacheFootPrintChart[idx].HighVolumeIndicator == highVolumeIndicator && cacheFootPrintChart[idx].ShowTotalDelta == showTotalDelta && cacheFootPrintChart[idx].EqualsInput(input))
						return cacheFootPrintChart[idx];
			return CacheIndicator<FootPrintChart>(new FootPrintChart(){ BarSpacing = barSpacing, BarWidth = barWidth, TextFont = textFont, ShowCandleStickBars = showCandleStickBars, ShowBodyBar = showBodyBar, ShowWicks = showWicks, FootPrintBarType = footPrintBarType, FootPrintBarColor = footPrintBarColor, ClosePriceIndicator = closePriceIndicator, HighVolumeIndicator = highVolumeIndicator, ShowTotalDelta = showTotalDelta }, input, ref cacheFootPrintChart);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FootPrintChart FootPrintChart(int barSpacing, int barWidth, NinjaTrader.Gui.Tools.SimpleFont textFont, bool showCandleStickBars, bool showBodyBar, bool showWicks, FootPrintBarEnum footPrintBarType, FootPrintBarColorEnum footPrintBarColor, ClosePriceEnum closePriceIndicator, HighestVolumeEnum highVolumeIndicator, bool showTotalDelta)
		{
			return indicator.FootPrintChart(Input, barSpacing, barWidth, textFont, showCandleStickBars, showBodyBar, showWicks, footPrintBarType, footPrintBarColor, closePriceIndicator, highVolumeIndicator, showTotalDelta);
		}

		public Indicators.FootPrintChart FootPrintChart(ISeries<double> input , int barSpacing, int barWidth, NinjaTrader.Gui.Tools.SimpleFont textFont, bool showCandleStickBars, bool showBodyBar, bool showWicks, FootPrintBarEnum footPrintBarType, FootPrintBarColorEnum footPrintBarColor, ClosePriceEnum closePriceIndicator, HighestVolumeEnum highVolumeIndicator, bool showTotalDelta)
		{
			return indicator.FootPrintChart(input, barSpacing, barWidth, textFont, showCandleStickBars, showBodyBar, showWicks, footPrintBarType, footPrintBarColor, closePriceIndicator, highVolumeIndicator, showTotalDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FootPrintChart FootPrintChart(int barSpacing, int barWidth, NinjaTrader.Gui.Tools.SimpleFont textFont, bool showCandleStickBars, bool showBodyBar, bool showWicks, FootPrintBarEnum footPrintBarType, FootPrintBarColorEnum footPrintBarColor, ClosePriceEnum closePriceIndicator, HighestVolumeEnum highVolumeIndicator, bool showTotalDelta)
		{
			return indicator.FootPrintChart(Input, barSpacing, barWidth, textFont, showCandleStickBars, showBodyBar, showWicks, footPrintBarType, footPrintBarColor, closePriceIndicator, highVolumeIndicator, showTotalDelta);
		}

		public Indicators.FootPrintChart FootPrintChart(ISeries<double> input , int barSpacing, int barWidth, NinjaTrader.Gui.Tools.SimpleFont textFont, bool showCandleStickBars, bool showBodyBar, bool showWicks, FootPrintBarEnum footPrintBarType, FootPrintBarColorEnum footPrintBarColor, ClosePriceEnum closePriceIndicator, HighestVolumeEnum highVolumeIndicator, bool showTotalDelta)
		{
			return indicator.FootPrintChart(input, barSpacing, barWidth, textFont, showCandleStickBars, showBodyBar, showWicks, footPrintBarType, footPrintBarColor, closePriceIndicator, highVolumeIndicator, showTotalDelta);
		}
	}
}

#endregion
