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
	public class ShowBidAskLines : Indicator
	{
		private double CurrentAsk=0;
		private double CurrentBid=0;
		private DateTime TimeOfLastTick = DateTime.MinValue;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Show BidAskLines";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				LineLength_Bars					= 10;
				AddPlot(Brushes.Orange, "BidLevel");
				AddPlot(Brushes.Olive, "AskLevel");
			}
			else if (State == State.Configure)
			{
			}
		}
//==============================================================================================
        protected override void OnMarketData(MarketDataEventArgs e)
        {
			if (e.MarketDataType == MarketDataType.Ask)
			{
				if(e.Price!=CurrentAsk){
					CurrentAsk	= e.Price;
//Print("new Ask  "+e.Time.ToString());
					ForceRefresh();
				}
			}
			if (e.MarketDataType == MarketDataType.Bid)
			{
				if(e.Price!=CurrentBid){
					CurrentBid	= e.Price;
//Print("new Bid  "+e.Time.ToString());
					ForceRefresh();
				}
			}
			TimeOfLastTick = e.Time;
		}
//==============================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			var yBidPrice = chartScale.GetYByValue(CurrentBid)+1;
			var yAskPrice = chartScale.GetYByValue(CurrentAsk);
			float x = ChartPanel.W;//ChartControl.GetXByBarIndex(ChartBars, CurrentBars[0])+3;
			var barwidth = chartControl.GetBarPaintWidth(ChartBars);
			RenderTarget.DrawLine(new SharpDX.Vector2(x,yAskPrice),new SharpDX.Vector2(x-LineLength_Bars * barwidth,yAskPrice), Plots[1].BrushDX);
			RenderTarget.DrawLine(new SharpDX.Vector2(x,yBidPrice),new SharpDX.Vector2(x-LineLength_Bars * barwidth,yBidPrice), Plots[0].BrushDX);
//Print("OnRender  ");
		}
//==============================================================================================
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="LineLength_Bars", Description="Length of the bid/ask lines, 0 changes this to a dot instead of a line", Order=1, GroupName="Parameters")]
		public int LineLength_Bars
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BidLevel
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AskLevel
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ShowBidAskLines[] cacheShowBidAskLines;
		public ShowBidAskLines ShowBidAskLines(int lineLength_Bars)
		{
			return ShowBidAskLines(Input, lineLength_Bars);
		}

		public ShowBidAskLines ShowBidAskLines(ISeries<double> input, int lineLength_Bars)
		{
			if (cacheShowBidAskLines != null)
				for (int idx = 0; idx < cacheShowBidAskLines.Length; idx++)
					if (cacheShowBidAskLines[idx] != null && cacheShowBidAskLines[idx].LineLength_Bars == lineLength_Bars && cacheShowBidAskLines[idx].EqualsInput(input))
						return cacheShowBidAskLines[idx];
			return CacheIndicator<ShowBidAskLines>(new ShowBidAskLines(){ LineLength_Bars = lineLength_Bars }, input, ref cacheShowBidAskLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ShowBidAskLines ShowBidAskLines(int lineLength_Bars)
		{
			return indicator.ShowBidAskLines(Input, lineLength_Bars);
		}

		public Indicators.ShowBidAskLines ShowBidAskLines(ISeries<double> input , int lineLength_Bars)
		{
			return indicator.ShowBidAskLines(input, lineLength_Bars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ShowBidAskLines ShowBidAskLines(int lineLength_Bars)
		{
			return indicator.ShowBidAskLines(Input, lineLength_Bars);
		}

		public Indicators.ShowBidAskLines ShowBidAskLines(ISeries<double> input , int lineLength_Bars)
		{
			return indicator.ShowBidAskLines(input, lineLength_Bars);
		}
	}
}

#endregion
