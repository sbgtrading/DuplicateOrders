// 
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;

#endregion

//This namespace holds Drawing tools in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.DrawingTools
{
	/// <summary>
	/// Represents an interface that exposes information regarding a Risk Reward IDrawingTool.
	/// </summary>
	public class ARC_RiskReward : DrawingTool
	{
		private const int		cursorSensitivity		= 15;
		private ChartAnchor		editingAnchor;
		private double			entryPrice;
		private bool			needsRatioUpdate		= true;
		private bool			needsQtyUpdate			= true;
		private double			ratio					= 0;
		private double			ratio1					= 1.0;
		private double			ratio2					= 1.3;
		private double			ratio3					= 2;
		private double			ratio4					= 0;
		private double			maxrisk_dollars			= 100;
		private double			exit1_pct				= 50;
		private double			exit2_pct				= 30;
		private double			exit3_pct				= 15;
		private double			exit4_pct				= 5;
		private double			riskPts;
		private double			rewardPts;	
		private double			stopPrice;
		private double			targetPrice;
		private double			textleftPoint;
		private	double			textRightPoint;

		[Browsable(false)]
		private bool 			DrawTarget				{ get { return (RiskAnchor != null && !RiskAnchor.IsEditing) || (RewardAnchor != null && !RewardAnchor.IsEditing); } }

		[Display(Order = 1)]
		public ChartAnchor		EntryAnchor				{ get; set; }
		[Display(Order = 2)]
		public ChartAnchor		RiskAnchor				{ get; set; }
		[Browsable(false)]
		public ChartAnchor		RewardAnchor			{ get; set; }

		public override object Icon { get { return Icons.DrawRiskReward; } }

		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ratio1", GroupName="NinjaScriptGeneral", Order = 10)]
		public double Ratio1
		{
			get { return ratio1; }
			set
			{
				if (ratio1.ApproxCompare(value) == 0)
					return;
				ratio1				= value;
				needsRatioUpdate	= true;
				ratio = Math.Max(ratio1,Math.Max(ratio2, Math.Max(ratio3,ratio4)));
			}
		}
		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ratio2", GroupName="NinjaScriptGeneral", Order = 20)]
		public double Ratio2
		{
			get { return ratio2; }
			set
			{
				if (ratio2.ApproxCompare(value) == 0)
					return;
				ratio2				= value;
				needsRatioUpdate	= true;
				ratio = Math.Max(ratio1,Math.Max(ratio2, Math.Max(ratio3,ratio4)));
			}
		}
		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ratio3", GroupName="NinjaScriptGeneral", Order = 30)]
		public double Ratio3
		{
			get { return ratio3; }
			set
			{
				if (ratio3.ApproxCompare(value) == 0)
					return;
				ratio3				= value;
				needsRatioUpdate	= true;
				ratio = Math.Max(ratio1,Math.Max(ratio2, Math.Max(ratio3,ratio4)));
			}
		}
		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ratio4", GroupName="NinjaScriptGeneral", Order = 40)]
		public double Ratio4
		{
			get { return ratio4; }
			set
			{
				if (ratio4.ApproxCompare(value) == 0)
					return;
				ratio4				= value;
				needsRatioUpdate	= true;
				ratio = Math.Max(ratio1,Math.Max(ratio2, Math.Max(ratio3,ratio4)));
			}
		}
		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Max Risk $", GroupName="NinjaScriptGeneral", Order = 50)]
		public double MaxRisk_Dollars
		{
			get { return maxrisk_dollars; }
			set
			{
				maxrisk_dollars		= value;
				needsQtyUpdate	= true;
			}
		}
//		private double			maxrisk_dollars			= 100;
		[Range(0, 100)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Exit T1 %", GroupName="NinjaScriptGeneral", Order = 60)]
		public double Exit1_Pct
		{
			get { return exit1_pct; }
			set
			{
				exit1_pct = value;
				needsQtyUpdate	= true;
			}
		}
//		private double			exit1_pct				= 50;
		[Range(0, 100)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Exit T2 %", GroupName="NinjaScriptGeneral", Order = 70)]
		public double Exit2_Pct
		{
			get { return exit2_pct; }
			set
			{
				exit2_pct = value;
				needsQtyUpdate	= true;
			}
		}
//		private double			exit2_pct				= 30;
		[Range(0, 100)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Exit T3 %", GroupName="NinjaScriptGeneral", Order = 80)]
		public double Exit3_Pct
		{
			get { return exit3_pct; }
			set
			{
				exit3_pct = value;
				needsQtyUpdate	= true;
			}
		}
//		private double			exit3_pct				= 15;
		[Range(0, 100)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Exit T4 %", GroupName="NinjaScriptGeneral", Order = 90)]
		public double Exit4_Pct
		{
			get { return exit4_pct; }
			set
			{
				exit4_pct = value;
				needsQtyUpdate	= true;
			}
		}
//		private double			exit4_pct				= 5;

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAnchor", GroupName = "NinjaScriptLines", Order = 3)]
		public Stroke			AnchorLineStroke 		{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRiskRewardLineStrokeEntry", GroupName = "NinjaScriptLines", Order = 6)]
		public Stroke			EntryLineStroke 		{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRiskRewardLineStrokeRisk", GroupName = "NinjaScriptLines", Order = 4)]
		public Stroke			StopLineStroke 			{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRiskRewardLineStrokeReward", GroupName = "NinjaScriptLines", Order = 5)]
		public Stroke			TargetLineStroke 		{ get; set; }
		
		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { EntryAnchor, RiskAnchor, RewardAnchor }; } }

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsExtendLinesRight", GroupName = "NinjaScriptLines", Order = 2)]
		public bool						IsExtendedLinesRight 	{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsExtendLinesLeft", GroupName = "NinjaScriptLines", Order = 1)]
		public bool						IsExtendedLinesLeft 	{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolTextAlignment", GroupName="NinjaScriptGeneral", Order = 2)]
		public TextLocation				TextAlignment 			{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRulerYValueDisplayUnit", GroupName = "NinjaScriptGeneral", Order = 3)]
		public ValueUnit				DisplayUnit 		{ get; set; }

		public override bool SupportsAlerts { get { return true; } }

		private void DrawDollarText(ChartAnchor anchor, Point point, double price, double qty, ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale)
		{
			if (TextAlignment == TextLocation.Off)
				return;
			
			string dollarsString;
			ChartBars chartBars = GetAttachedToChartBars();
			
			// bars can be null while chart is initializing
			if (chartBars == null)
				return;
			
			// NS can change ChartAnchor price via Draw method or directly meaning we needed to resync price before drawing
			if (!IsUserDrawn)
				price = AttachedTo.Instrument.MasterInstrument.RoundToTickSize(anchor.Price);

			var str = GetPriceString(price, chartBars, qty);
			if(str.Length>0 && qty<0)
				dollarsString = string.Format("{0} @ {1}",qty, str);
			else if(str.Length>0 && qty>0)
				dollarsString = string.Format("+{0} @ {1}",qty, str);
			else if(str.Length==0 && qty<0)
				dollarsString = string.Format("{0}",qty);
			else if(str.Length==0 && qty>0)
				dollarsString = string.Format("+{0}",qty);
			else //if qty is zero, don't print any qty information
				dollarsString = str;

			Stroke color;
			textleftPoint	= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale).X;
			textRightPoint	= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale).X;

			if (anchor == RewardAnchor)		color = TargetLineStroke;
			else if (anchor == RiskAnchor)	color = StopLineStroke;
			else if (anchor == EntryAnchor)	color = EntryLineStroke;
			else							color = AnchorLineStroke;

			SimpleFont						wpfFont		= chartControl.Properties.LabelFont ?? new SimpleFont();
			SharpDX.DirectWrite.TextFormat	textFormat	= wpfFont.ToDirectWriteTextFormat();
			textFormat.TextAlignment					= SharpDX.DirectWrite.TextAlignment.Leading;
			textFormat.WordWrapping						= SharpDX.DirectWrite.WordWrapping.NoWrap;
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, dollarsString, textFormat, chartPanel.H, textFormat.FontSize);

			if (RiskAnchor.Time <= EntryAnchor.Time)
			{
				if(!IsExtendedLinesLeft && !IsExtendedLinesRight)
					switch(TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textleftPoint;								break;
						case TextLocation.InsideRight	: point.X = textRightPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = textleftPoint;								break;
						case TextLocation.ExtremeRight	: point.X = textRightPoint - textLayout.Metrics.Width;	break;
					}
				else if (IsExtendedLinesLeft && !IsExtendedLinesRight)
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textleftPoint;								break;
						case TextLocation.InsideRight	: point.X = textRightPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = chartPanel.X;								break;
						case TextLocation.ExtremeRight	: point.X = textRightPoint - textLayout.Metrics.Width;	break;
					}
				else if (!IsExtendedLinesLeft && IsExtendedLinesRight)
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textleftPoint;								break;
						case TextLocation.InsideRight	: point.X = textRightPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = textleftPoint;								break;
						case TextLocation.ExtremeRight	: point.X = chartPanel.W - textLayout.Metrics.Width;	break;
					}
				else if (IsExtendedLinesLeft && IsExtendedLinesRight)
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textleftPoint;								break;
						case TextLocation.InsideRight	: point.X = textRightPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeRight	: point.X = chartPanel.W - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = chartPanel.X;								break;
					}
			}
			else if (RiskAnchor.Time >= EntryAnchor.Time)
				if (!IsExtendedLinesLeft && !IsExtendedLinesRight)
				{
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textRightPoint;								break;
						case TextLocation.InsideRight	: point.X = textleftPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = textRightPoint;								break;
						case TextLocation.ExtremeRight	: point.X = textleftPoint - textLayout.Metrics.Width;	break;
					}
				}
				else if (IsExtendedLinesLeft && !IsExtendedLinesRight)
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textRightPoint;								break;
						case TextLocation.InsideRight	: point.X = textleftPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = chartPanel.X;								break;
						case TextLocation.ExtremeRight	: point.X = textleftPoint - textLayout.Metrics.Width;	break;
					}
				else if (!IsExtendedLinesLeft && IsExtendedLinesRight)
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textRightPoint;								break;
						case TextLocation.InsideRight	: point.X = textleftPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = textRightPoint;								break;
						case TextLocation.ExtremeRight	: point.X = chartPanel.W - textLayout.Metrics.Width;	break;
					}
				else if (IsExtendedLinesLeft && IsExtendedLinesRight)
					switch (TextAlignment)
					{
						case TextLocation.InsideLeft	: point.X = textRightPoint;								break;
						case TextLocation.InsideRight	: point.X = textleftPoint - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeRight	: point.X = chartPanel.W - textLayout.Metrics.Width;	break;
						case TextLocation.ExtremeLeft	: point.X = chartPanel.X;								break;
					}

			RenderTarget.DrawTextLayout(new SharpDX.Vector2((float)point.X, (float)point.Y), textLayout, color.BrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
		}

		public override IEnumerable<AlertConditionItem> GetAlertConditionItems()
		{
			return Anchors.Select(anchor => new AlertConditionItem	{
																		Name					= anchor.DisplayName,
																		ShouldOnlyDisplayName	= true,
																		Tag						= anchor
																	});
		}

		public override Cursor GetCursor(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, Point point)
		{
			switch (DrawingState)
			{
				case DrawingState.Building:	return Cursors.Pen;
				case DrawingState.Moving:	return IsLocked ? Cursors.No : Cursors.SizeAll;
				case DrawingState.Editing:	return IsLocked ? Cursors.No : (editingAnchor == EntryAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE);
				default:
					// draw move cursor if cursor is near line path anywhere
					Point entryAnchorPixelPoint	= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale);

					// see if we are near an anchor right away. this is is cheap so no big deal to do often
					ChartAnchor closest = GetClosestAnchor(chartControl, chartPanel, chartScale, cursorSensitivity, point);

					if (closest != null)
						return IsLocked ? Cursors.Arrow : (closest == EntryAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE);

					Point	stopAnchorPixelPoint	= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale);
					Vector	anchorsVector			= stopAnchorPixelPoint - entryAnchorPixelPoint;
	
					// see if the mouse is along one of our lines for moving
					if (MathHelper.IsPointAlongVector(point, entryAnchorPixelPoint, anchorsVector, cursorSensitivity))
						return IsLocked ? Cursors.Arrow : Cursors.SizeAll;

					if (!DrawTarget) 
						return null;

					Point	targetPoint			= RewardAnchor.GetPoint(chartControl, chartPanel, chartScale);
					Vector	targetToEntryVector	= targetPoint - entryAnchorPixelPoint;
					return MathHelper.IsPointAlongVector(point, entryAnchorPixelPoint, targetToEntryVector, cursorSensitivity) ? (IsLocked ? Cursors.Arrow : Cursors.SizeAll) : null;
			}
		}

		private string GetPriceString(double price, ChartBars chartBars, double qty)
		{
			string dollarsString;
			double yValueEntry	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			double tickSize		= AttachedTo.Instrument.MasterInstrument.TickSize;
			double pointValue	= AttachedTo.Instrument.MasterInstrument.PointValue;
			switch (DisplayUnit)
			{
				case ValueUnit.Currency:
					if (AttachedTo.Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex)
					{
						dollarsString = string.Format("{0}={1}",
							price > yValueEntry ?
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize * qty).ToString("F0") :
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize * qty).ToString("F0"),
							price > yValueEntry ?
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize * (tickSize * pointValue * qty * Account.All[0].ForexLotSize)) :
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize * (tickSize * pointValue * qty * Account.All[0].ForexLotSize)));
						dollarsString = dollarsString.Replace(".00",string.Empty);
					}
					else
					{
						dollarsString = string.Format("{0}={1}",
							price > yValueEntry ?
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize * qty).ToString("F0") :
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize * qty).ToString("F0"),
							price > yValueEntry ?
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize * (tickSize * pointValue) * qty) :
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize * (tickSize * pointValue) * qty));
						dollarsString = dollarsString.Replace(".00",string.Empty).Replace("0=$0",AttachedTo.Instrument.MasterInstrument.FormatPrice(yValueEntry));
					}
					break;
				case ValueUnit.Percent:
					dollarsString = price > yValueEntry ?
						(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / yValueEntry).ToString("P", Core.Globals.GeneralOptions.CurrentCulture) :
						(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / yValueEntry).ToString("P", Core.Globals.GeneralOptions.CurrentCulture);
					break;
				case ValueUnit.Ticks:
					if(price==yValueEntry) dollarsString = "";
					else
						dollarsString = price > yValueEntry ?
							(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize).ToString("F0") :
							(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize).ToString("F0");
					break;
				case ValueUnit.Pips:
					if(price==yValueEntry) dollarsString = "";
					else
						dollarsString = price > yValueEntry ?
							(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize / 10).ToString("F0") :
							(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize / 10).ToString("F0");
					break;
				default:
					dollarsString = chartBars.Bars.Instrument.MasterInstrument.FormatPrice(price);
					break;
			}
			return dollarsString;
		}

		public override Point[] GetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel	chartPanel	= chartControl.ChartPanels[chartScale.PanelIndex];
			Point		entryPoint	= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point		stopPoint	= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale);
						
			if (!DrawTarget)
				return new[] { entryPoint, stopPoint };

			Point targetPoint = RewardAnchor.GetPoint(chartControl, chartPanel, chartScale);
			return new[] { entryPoint, stopPoint, targetPoint };
		}

		public override bool IsAlertConditionTrue(AlertConditionItem conditionItem, Condition condition, ChartAlertValue[] values, ChartControl chartControl, ChartScale chartScale)
		{
			// dig up which anchor we are running on to determine line
			ChartAnchor chartAnchor = conditionItem.Tag as ChartAnchor;
			if (chartAnchor == null)
				return false;

			ChartPanel	chartPanel	= chartControl.ChartPanels[PanelIndex];
			double		alertY		= chartScale.GetYByValue(chartAnchor.Price);
			Point		entryPoint	= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point		stopPoint	= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point		targetPoint	= RewardAnchor.GetPoint(chartControl, chartPanel, chartScale);
			double		anchorMinX	= DrawTarget ? new[] { entryPoint.X, stopPoint.X, targetPoint.X }.Min() : new[] {entryPoint.X, stopPoint.X}.Min(); 
			double		anchorMaxX	= DrawTarget ? new[] { entryPoint.X, stopPoint.X, targetPoint.X }.Max() : new[] {entryPoint.X, stopPoint.X}.Max();
			double		lineStartX	= IsExtendedLinesLeft ? chartPanel.X : anchorMinX;
			double		lineEndX	= IsExtendedLinesRight ? chartPanel.X + chartPanel.W : anchorMaxX;

			// first thing, if our smallest x is greater than most recent bar, we have nothing to do yet.
			// do not try to check Y because lines could cross through stuff
			double firstBarX = chartControl.GetXByTime(values[0].Time);
			double firstBarY = chartScale.GetYByValue(values[0].Value);
			
			if (lineEndX < firstBarX) // bars passed our drawing tool
				return false;

			Point lineStartPoint	= new Point(lineStartX, alertY);
			Point lineEndPoint		= new Point(lineEndX, alertY);

			Point barPoint = new Point(firstBarX, firstBarY);
			// NOTE: 'left / right' is relative to if line was vertical. it can end up backwards too
			MathHelper.PointLineLocation pointLocation = MathHelper.GetPointLineLocation(lineStartPoint, lineEndPoint, barPoint);
			// for vertical things, think of a vertical line rotated 90 degrees to lay flat, where it's normal vector is 'up'
			switch (condition)
			{
				case Condition.Greater:			return pointLocation == MathHelper.PointLineLocation.LeftOrAbove;
				case Condition.GreaterEqual:	return pointLocation == MathHelper.PointLineLocation.LeftOrAbove || pointLocation == MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.Less:			return pointLocation == MathHelper.PointLineLocation.RightOrBelow;
				case Condition.LessEqual:		return pointLocation == MathHelper.PointLineLocation.RightOrBelow || pointLocation == MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.Equals:			return pointLocation == MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.NotEqual:		return pointLocation != MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.CrossAbove:
				case Condition.CrossBelow:
					Predicate<ChartAlertValue> predicate = v =>
					{
						double barX = chartControl.GetXByTime(v.Time);
						double barY = chartScale.GetYByValue(v.Value);
						Point stepBarPoint = new Point(barX, barY);
						// NOTE: 'left / right' is relative to if line was vertical. it can end up backwards too
						MathHelper.PointLineLocation ptLocation = MathHelper.GetPointLineLocation(lineStartPoint, lineEndPoint, stepBarPoint);
						if (condition == Condition.CrossAbove)
							return ptLocation == MathHelper.PointLineLocation.LeftOrAbove;
						return ptLocation == MathHelper.PointLineLocation.RightOrBelow;
					};
					return MathHelper.DidPredicateCross(values, predicate);
			}
			return false;
		}

		public override bool IsVisibleOnChart(ChartControl chartControl, ChartScale chartScale, DateTime firstTimeOnChart, DateTime lastTimeOnChart)
		{
			return DrawingState == DrawingState.Building || Anchors.Any(a => a.Time >= firstTimeOnChart && a.Time <= lastTimeOnChart);
		}

		public override void OnCalculateMinMax()
		{
			// It is important to set MinValue and MaxValue to the min/max Y values your drawing tool uses if you want it to support auto scale
			MinValue = double.MaxValue;
			MaxValue = double.MinValue;

			if (!IsVisible)
				return;

			// return min/max values only if something has been actually drawn
			if (Anchors.Any(a => !a.IsEditing))
				foreach (ChartAnchor anchor in Anchors)
				{
					if(anchor.DisplayName == RewardAnchor.DisplayName && !DrawTarget)
						continue;
					
					MinValue = Math.Min(anchor.Price, MinValue);
					MaxValue = Math.Max(anchor.Price, MaxValue);
				}
		}

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			switch(DrawingState)
			{
				case DrawingState.Building:
					if (EntryAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(EntryAnchor);
						dataPoint.CopyDataValues(RiskAnchor);
						EntryAnchor.IsEditing	= false;
						entryPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
					}
					else if (RiskAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(RiskAnchor);
						RiskAnchor.IsEditing	= false;
						stopPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RiskAnchor.Price);
						SetReward();
						SetQuantities();
						// we set the anchor for the target after stop mouse down event via SetReward()
						//however we need make sure Time is in view when builiding, but always when SetRreward is used..
						RewardAnchor.Time		= EntryAnchor.Time;
						RewardAnchor.SlotIndex	= EntryAnchor.SlotIndex;
						RewardAnchor.IsEditing	= false;
					}
					// if the anchors are no longer being edited, set the drawing state to normal and unselect the object
					if (!EntryAnchor.IsEditing && !RiskAnchor.IsEditing && !RewardAnchor.IsEditing)
					{
						DrawingState = DrawingState.Normal;
						IsSelected = false;
					}
					break;
				case DrawingState.Normal:
					Point point = dataPoint.GetPoint(chartControl, chartPanel, chartScale);
					//find which anchor has been clicked relative to the mouse point and make whichever anchor now editable
					editingAnchor = GetClosestAnchor(chartControl, chartPanel, chartScale, cursorSensitivity, point);
					if (editingAnchor != null)
					{
						editingAnchor.IsEditing = true;
						DrawingState = DrawingState.Editing;
					}
					else if (GetCursor(chartControl, chartPanel, chartScale, point) == null)
						IsSelected = false; // missed
					else
					// didnt click an anchor but on a line so start moving
						DrawingState = DrawingState.Moving;
					break;
			}
		}

		public override void OnMouseMove(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			if (IsLocked && DrawingState != DrawingState.Building || !IsVisible)
				return;

			if (DrawingState == DrawingState.Building)
			{
				if (EntryAnchor.IsEditing)
					dataPoint.CopyDataValues(EntryAnchor);
				else if (RiskAnchor.IsEditing)
					dataPoint.CopyDataValues(RiskAnchor);
				else if (RewardAnchor.IsEditing)
					dataPoint.CopyDataValues(RewardAnchor);
			}
			else if (DrawingState == DrawingState.Editing && editingAnchor != null)
			{
				dataPoint.CopyDataValues(editingAnchor);
				if (editingAnchor != EntryAnchor)
				{
					if (editingAnchor != RewardAnchor && ratio.ApproxCompare(0) != 0){
						SetReward();
						SetQuantities();
					}
					else if (ratio.ApproxCompare(0) != 0)
						SetRisk();
				}
			}
			else if (DrawingState == DrawingState.Moving)
			{
				foreach (ChartAnchor anchor in Anchors)
					anchor.MoveAnchor(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
			}
			
			entryPrice	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			stopPrice	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RiskAnchor.Price);
			targetPrice	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor.Price);
		}

		public override void OnMouseUp(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			//don't set anchors until we're done drawing
			if (DrawingState == DrawingState.Building)
				return;
			
			//set the drawing state back to normal when mouse is relased
			if (DrawingState == DrawingState.Editing || DrawingState == DrawingState.Moving)
				DrawingState = DrawingState.Normal;
			if (editingAnchor != null)
			{
				if (editingAnchor == EntryAnchor)
				{
					SetReward();
					SetQuantities();
					if (ratio.ApproxCompare(0) != 0)	
						SetRisk();
				}
				editingAnchor.IsEditing = false;
			}
			editingAnchor = null;
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (!IsVisible)
				return;
			if (Anchors.All(a => a.IsEditing))
				return;
		
			// this will be true right away to fix a restoral issue, so check if we really want to set rewardPts
			if (needsRatioUpdate && DrawTarget)
				SetReward();
			if(needsQtyUpdate && DrawTarget);
				SetQuantities();
			
			ChartPanel chartPanel	= chartControl.ChartPanels[PanelIndex];
			Point entryPoint		= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point stopPoint			= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point targetPoint		= (DrawingState == DrawingState.Building ? RiskAnchor.GetPoint(chartControl, chartPanel, chartScale) : RewardAnchor.GetPoint(chartControl, chartPanel, chartScale));

			AnchorLineStroke.RenderTarget	= RenderTarget;
			EntryLineStroke.RenderTarget	= RenderTarget;
			StopLineStroke.RenderTarget		= RenderTarget;

			// first of all, turn on anti-aliasing to smooth out our line
			RenderTarget.AntialiasMode	= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			RenderTarget.DrawLine(entryPoint.ToVector2(), stopPoint.ToVector2(), AnchorLineStroke.BrushDX, AnchorLineStroke.Width, AnchorLineStroke.StrokeStyle);

			double	anchorMinX	= DrawTarget ? new[] { entryPoint.X, stopPoint.X, targetPoint.X }.Min() : new[] {entryPoint.X, stopPoint.X}.Min(); 
			double	anchorMaxX	= DrawTarget ? new[] { entryPoint.X, stopPoint.X, targetPoint.X }.Max() : new[] {entryPoint.X, stopPoint.X}.Max();
			double	lineStartX	= IsExtendedLinesLeft ? chartPanel.X : anchorMinX;
			double	lineEndX	= IsExtendedLinesRight ? chartPanel.X + chartPanel.W : anchorMaxX;

			SharpDX.Vector2 entryStartVector	= new SharpDX.Vector2((float)lineStartX, (float)entryPoint.Y);
			SharpDX.Vector2 entryEndVector		= new SharpDX.Vector2((float)lineEndX, (float)entryPoint.Y);
			SharpDX.Vector2 stopStartVector		= new SharpDX.Vector2((float)lineStartX, (float)stopPoint.Y);
			SharpDX.Vector2 stopEndVector		= new SharpDX.Vector2((float)lineEndX, (float)stopPoint.Y);

			// don't try and draw the target stuff until we have calculated the target
			SharpDX.Direct2D1.Brush tmpBrush = IsInHitTest ? chartControl.SelectionBrush : AnchorLineStroke.BrushDX;
			double mult = 1;
			if(EntryAnchor.Price < RiskAnchor.Price) mult= -1;//going short
			if (DrawTarget)
			{	
				AnchorLineStroke.RenderTarget 	= RenderTarget;
				RenderTarget.DrawLine(entryPoint.ToVector2(), targetPoint.ToVector2(), tmpBrush, AnchorLineStroke.Width, AnchorLineStroke.StrokeStyle);
				TargetLineStroke.RenderTarget		= RenderTarget;
				Point tp = new Point(targetPoint.X, 0);
				if(ratio1>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio1;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, -t1_qty*mult, chartControl, chartPanel, chartScale);
				}
				if(ratio2>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio2;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, -t2_qty*mult, chartControl, chartPanel, chartScale);
				}
				if(ratio3>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio3;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, -t3_qty*mult, chartControl, chartPanel, chartScale);
				}
				if(ratio4>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio4;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, -t4_qty*mult, chartControl, chartPanel, chartScale);
				}
			}
			tmpBrush = IsInHitTest ? chartControl.SelectionBrush : EntryLineStroke.BrushDX;
			RenderTarget.DrawLine(entryStartVector, entryEndVector, tmpBrush, EntryLineStroke.Width, EntryLineStroke.StrokeStyle);
			if(initial_qty>0)
				DrawDollarText(EntryAnchor, entryPoint, entryPrice, initial_qty*mult, chartControl, chartPanel, chartScale);

			tmpBrush = IsInHitTest ? chartControl.SelectionBrush : StopLineStroke.BrushDX;
			RenderTarget.DrawLine(stopStartVector, stopEndVector, tmpBrush, StopLineStroke.Width, StopLineStroke.StrokeStyle);
			if(initial_qty>0)
				DrawDollarText(RiskAnchor, stopPoint, stopPrice, -initial_qty*mult, chartControl, chartPanel, chartScale);
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptDrawingToolRiskRewardDescription;
				Name						= "ARC RiskReward";
				Ratio1 = 1.0;
				Ratio2 = 1.3;
				Ratio3 = 2.0;
				Ratio4 = 0;
				ratio						= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
				MaxRisk_Dollars				= 100;
				Exit1_Pct					= 50;
				Exit2_Pct					= 30;
				Exit3_Pct					= 15;
				Exit4_Pct					= 5;
				DisplayUnit					= ValueUnit.Currency;
				AnchorLineStroke 			= new Stroke(Brushes.DarkGray,	DashStyleHelper.Solid, 1f, 50);
				EntryLineStroke 			= new Stroke(Brushes.Goldenrod,	DashStyleHelper.Solid, 2f);
				StopLineStroke 				= new Stroke(Brushes.Crimson,	DashStyleHelper.Solid, 2f);
				TargetLineStroke 			= new Stroke(Brushes.SeaGreen,	DashStyleHelper.Solid, 2f);
				EntryAnchor					= new ChartAnchor { IsEditing = true, DrawingTool = this };
				RiskAnchor					= new ChartAnchor { IsEditing = true, DrawingTool = this };
				RewardAnchor				= new ChartAnchor { IsEditing = true, DrawingTool = this };
				EntryAnchor.DisplayName		= Custom.Resource.NinjaScriptDrawingToolRiskRewardAnchorEntry;
				RiskAnchor.DisplayName		= Custom.Resource.NinjaScriptDrawingToolRiskRewardAnchorRisk;
				RewardAnchor.DisplayName	= Custom.Resource.NinjaScriptDrawingToolRiskRewardAnchorReward;
			}
			else if (State == State.Terminated)
				Dispose();
		}
		
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetReward()
		{
			if (Anchors == null || AttachedTo == null)
				return;
			
			entryPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			stopPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RiskAnchor.Price);
			riskPts 				= entryPrice - stopPrice;
			ratio					= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
			rewardPts				= riskPts * ratio;
			targetPrice 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + rewardPts);

			RewardAnchor.Price		= targetPrice;
			RewardAnchor.IsEditing	= false;

			needsRatioUpdate		= false;
		}
		private double initial_qty = 0;
		private double t1_qty = 0;
		private double t2_qty = 0;
		private double t3_qty = 0;
		private double t4_qty = 0;
//		bool PrintQtys = false;
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetQuantities()
		{
			if (Anchors == null || AttachedTo == null)
				return;
			entryPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			stopPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RiskAnchor.Price);
			initial_qty				= Math.Truncate(MaxRisk_Dollars / AttachedTo.Instrument.MasterInstrument.PointValue / Math.Abs(entryPrice-stopPrice));
			t1_qty					= Math.Round(initial_qty * exit1_pct/100.0,0);
			if(initial_qty - t1_qty>0)
				t2_qty = Math.Round(initial_qty * exit2_pct/100.0,0);
			else{
				t2_qty = 0;
				t3_qty = 0;
				t4_qty = 0;
			}
			if(initial_qty - t1_qty - t2_qty>0)
				t3_qty = Math.Round(initial_qty * exit3_pct/100.0,0);
			else{
				t2_qty = initial_qty - t1_qty;//reduce t2_qty so that t1_qty+t2_qty is 100% of the position
				t3_qty = 0;
				t4_qty = 0;
			}
			if(initial_qty - t1_qty - t2_qty - t3_qty>0)
				t4_qty = Math.Round(initial_qty * exit4_pct/100.0,0);
			else{
				t3_qty = initial_qty - t1_qty - t2_qty;//reduce t3_qty so that t1_qty+t2_qty+t3_qty is 100% of the position
				t4_qty = 0;
			}
//			if(initial_qty>0) PrintQtys = true;
			RewardAnchor.Price		= targetPrice;
			RewardAnchor.IsEditing	= false;
			needsQtyUpdate			= false;
		}
		
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetRisk()
		{
			if (Anchors == null || AttachedTo == null)
				return;

			entryPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			targetPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor.Price);
		
			rewardPts				= targetPrice - entryPrice;
			ratio					= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
			riskPts					= rewardPts / ratio;
			stopPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice - riskPts);
			
			RiskAnchor.Price 		= stopPrice;
			RiskAnchor.IsEditing	= false;
		
			needsRatioUpdate		= false;
		}
	}

	public static partial class Draw
	{
		private static ARC_RiskReward ARC_RiskRewardCore(NinjaScriptBase owner, string tag,
			bool isAutoScale, 
			int entryBarsAgo, DateTime entryTime, double entryY,
			int stopBarsAgo, DateTime stopTime, double stopY,
			int targetBarsAgo, DateTime targetTime, double targetY,
			double ratio, bool isStop, bool isGlobal, string templateName)
		{
			if (owner == null)
				throw new ArgumentException("owner");

			if (entryBarsAgo == int.MinValue && entryTime == Core.Globals.MinDate)
				throw new ArgumentException("entry value required");

			if (stopBarsAgo == int.MinValue && stopTime == Core.Globals.MinDate &&
				targetBarsAgo == int.MinValue && targetTime == Core.Globals.MinDate)
				throw new ArgumentException("a stop or target value is required");

			if (isGlobal && tag[0] != GlobalDrawingToolManager.GlobalDrawingToolTagPrefix)
				tag = string.Format("{0}{1}", GlobalDrawingToolManager.GlobalDrawingToolTagPrefix, tag);

			ARC_RiskReward riskReward = DrawingTool.GetByTagOrNew(owner, typeof(ARC_RiskReward), tag, templateName) as ARC_RiskReward;
			
			if (riskReward == null)
				return null;

			DrawingTool.SetDrawingToolCommonValues(riskReward, tag, isAutoScale, owner, isGlobal);

			// this is a little tricky, we use entry + (stop or target) to calculate the (target or stop) from ratio
			ChartAnchor	entryAnchor		= DrawingTool.CreateChartAnchor(owner, entryBarsAgo, entryTime, entryY);
			ChartAnchor	stopAnchor;
			ChartAnchor	targetAnchor;

//			riskReward.Ratio = ratio;
			
			if (isStop)
			{
				stopAnchor = DrawingTool.CreateChartAnchor(owner, stopBarsAgo, stopTime, stopY);
				entryAnchor.CopyDataValues(riskReward.EntryAnchor);
				entryAnchor.CopyDataValues(riskReward.RewardAnchor);
				stopAnchor.CopyDataValues(riskReward.RiskAnchor);
				riskReward.SetReward();
				riskReward.SetQuantities();
			}
			else 
			{
				targetAnchor = DrawingTool.CreateChartAnchor(owner, targetBarsAgo, targetTime, targetY);
				entryAnchor.CopyDataValues(riskReward.EntryAnchor);
				entryAnchor.CopyDataValues(riskReward.RiskAnchor);
				targetAnchor.CopyDataValues(riskReward.RewardAnchor);
				riskReward.SetRisk();
				riskReward.SetQuantities();
			}
			
			riskReward.SetState(State.Active);
			return riskReward;
		}

		/// <summary>
		/// Draws a riskPts/rewardPts on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryTime">The time where the draw object's entry will be drawn</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the riskPts or rewardPts based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <returns></returns>
		public static ARC_RiskReward ARC_RiskReward(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime entryTime, double entryY, DateTime endTime, double endY, double ratio, bool isStop)
		{
			return isStop
				? ARC_RiskRewardCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, int.MinValue, endTime, endY, 0, Core.Globals.MinDate, 0, ratio, isStop, false, null)
				: ARC_RiskRewardCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, 0, Core.Globals.MinDate, 0, int.MinValue, endTime, endY, ratio, isStop, false, null);
		}

		/// <summary>
		/// Draws a riskPts/rewardPts on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryBarsAgo">The starting bar (x axis coordinate) where the draw object's entry will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the riskPts or rewardPts based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <returns></returns>
		public static ARC_RiskReward ARC_RiskReward(NinjaScriptBase owner, string tag, bool isAutoScale, int entryBarsAgo, double entryY, int endBarsAgo, double endY, double ratio, bool isStop)
		{
			return isStop
				? ARC_RiskRewardCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, endBarsAgo, Core.Globals.MinDate, endY, 0, Core.Globals.MinDate, 0, ratio, isStop, false, null)
				: ARC_RiskRewardCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, 0, Core.Globals.MinDate, 0, endBarsAgo, Core.Globals.MinDate, endY, ratio, isStop, false, null);
		}

		/// <summary>
		/// Draws a riskPts/rewardPts on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryTime">The time where the draw object's entry will be drawn</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the riskPts or rewardPts based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ARC_RiskReward ARC_RiskReward(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime entryTime, double entryY, DateTime endTime, double endY, double ratio, bool isStop, bool isGlobal, string templateName)
		{
			return isStop
				? ARC_RiskRewardCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, int.MinValue, endTime, endY, 0, Core.Globals.MinDate, 0, ratio, isStop, isGlobal, templateName)
				: ARC_RiskRewardCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, 0, Core.Globals.MinDate, 0, int.MinValue, endTime, endY, ratio, isStop, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a riskPts/rewardPts on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryBarsAgo">The starting bar (x axis coordinate) where the draw object's entry will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the riskPts or rewardPts based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ARC_RiskReward ARC_RiskReward(NinjaScriptBase owner, string tag, bool isAutoScale, int entryBarsAgo, double entryY, int endBarsAgo, double endY, double ratio, bool isStop, bool isGlobal, string templateName)
		{
			return isStop
				? ARC_RiskRewardCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, endBarsAgo, Core.Globals.MinDate, endY, 0, Core.Globals.MinDate, 0, ratio, isStop, isGlobal, templateName)
				: ARC_RiskRewardCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, 0, Core.Globals.MinDate, 0, endBarsAgo, Core.Globals.MinDate, endY, ratio, isStop, isGlobal, templateName);
		}
	}
}
