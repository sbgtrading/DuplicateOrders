// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using System;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class OhlcStyle : ChartStyle, ISubModeProvider
	{
		private object icon;

		public override int GetBarPaintWidth(int barWidth) => 3 * barWidth;

		public override object Icon => icon ??= Gui.Tools.Icons.ChartOHLC;

		public OhlcMode Mode { get; set; }
		
		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars					bars			= chartBars.Bars;
			float					lineWidth		= (float) Math.Max(1, BarWidth);
			Vector2					point0			= new();
			Vector2					point1			= new();
			Vector2					point2			= new();
			Vector2					point3			= new();
			Vector2					point4			= new();
			Vector2					point5			= new();

			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				SharpDX.Direct2D1.Brush	overriddenBrush	= chartControl.GetBarOverrideBrush(chartBars, idx);
				double		closeValue					= bars.GetClose(idx);
				double		highValue					= bars.GetHigh(idx);
				double		lowValue					= bars.GetLow(idx);
				double		openValue					= bars.GetOpen(idx);
				int			close						= chartScale.GetYByValue(closeValue);
				int			high						= chartScale.GetYByValue(highValue);
				int			low							= chartScale.GetYByValue(lowValue);
				int			open						= chartScale.GetYByValue(openValue);
				float		x							= chartControl.GetXByBarIndex(chartBars, idx);

				point0.X								= point1.X = x;
				point0.Y								= Math.Min(high, low) - lineWidth * 0.5f;
				point1.Y								= Math.Max(high, low) + lineWidth * 0.5f;

				SharpDX.Direct2D1.Brush	b				= overriddenBrush ?? (closeValue >= openValue ? UpBrushDX : DownBrushDX);

				if (b is not SharpDX.Direct2D1.SolidColorBrush)
					TransformBrush(b, new RectangleF(point0.X - lineWidth * 1.5f, point0.Y, lineWidth * 3, point1.Y - point0.Y));

				RenderTarget.DrawLine(point0, point1, b, lineWidth);

				if (!Equals(Mode, OhlcMode.HiLo))
				{
					point2.X = x + lineWidth * 1.5f;
					point2.Y = close;
					point3.X = x;
					point3.Y = close;

					RenderTarget.DrawLine(point2, point3, b, lineWidth);

					if (Equals(Mode, OhlcMode.OHLC))
					{
						point4.X = x - lineWidth * 1.5f;
						point4.Y = open;
						point5.X = x;
						point5.Y = open;

						RenderTarget.DrawLine(point4, point5, b, lineWidth);
					}
				}
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleOHLC;
				ChartStyleType	= ChartStyleType.OHLC;
				Mode			= OhlcMode.OHLC;
				BarWidth		= 2;
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("Stroke", true));
				Properties.Remove(Properties.Find("Stroke2", true));

				SetPropertyName("BarWidth",		Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStyleOhlcUpBarsColor);
				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStyleOhlcDownBarsColor);
			}
		}

		public void SetSubmode(object mode)
		{
			if (mode is OhlcMode ohlcMode)
				Mode = ohlcMode;
		}

		public override System.Collections.Generic.IEnumerable<object> SubModes { get { foreach (object value in Enum.GetValues(typeof (OhlcMode))) yield return value; } }
	}

	public enum OhlcMode { OHLC, HLC, HiLo }
}
