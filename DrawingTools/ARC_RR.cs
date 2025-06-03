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
	public class ARCRR : DrawingTool
	{
		private const int		cursorSensitivity		= 15;
		private ChartAnchor		editingAnchor;
		private double			entryPrice;
		private bool			needsRatioUpdate		= true;
		private double			ratio					= 0;
		private double			ratio1					= 1.0;
		private double			ratio2					= 1.3;
		private double			ratio3					= 2;
		private double			ratio4					= 0;
		private double			risk;
		private double			reward;
		private double			reward1;	
		private double			reward2;	
		private double			reward3;	
		private double			reward4;	
		private double			stopPrice;
		private double			targetPrice;
		private double			targetPrice1;
		private double			targetPrice2;
		private double			targetPrice3;
		private double			targetPrice4;
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
		
		[Browsable(false)]
		public ChartAnchor		RewardAnchor1			{ get; set; }
		[Browsable(false)]
		public ChartAnchor		RewardAnchor2			{ get; set; }
		[Browsable(false)]
		public ChartAnchor		RewardAnchor3			{ get; set; }
		[Browsable(false)]
		public ChartAnchor		RewardAnchor4			{ get; set; }

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


		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAnchor", GroupName = "NinjaScriptLines", Order = 3)]
		public Stroke			AnchorLineStroke 		{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRiskRewardLineStrokeEntry", GroupName = "NinjaScriptLines", Order = 6)]
		public Stroke			EntryLineStroke 		{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRiskRewardLineStrokeRisk", GroupName = "NinjaScriptLines", Order = 4)]
		public Stroke			StopLineStroke 			{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRiskRewardLineStrokeReward", GroupName = "NinjaScriptLines", Order = 5)]
		public Stroke			TargetLineStroke 		{ get; set; }
		
		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { EntryAnchor, RiskAnchor, RewardAnchor,RewardAnchor1,RewardAnchor2,RewardAnchor3,RewardAnchor4 }; } }

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsExtendLinesRight", GroupName = "NinjaScriptLines", Order = 2)]
		public bool						IsExtendedLinesRight 	{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsExtendLinesLeft", GroupName = "NinjaScriptLines", Order = 1)]
		public bool						IsExtendedLinesLeft 	{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolTextAlignment", GroupName="NinjaScriptGeneral", Order = 2)]
		public TextLocation				TextAlignment 			{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolRulerYValueDisplayUnit", GroupName = "NinjaScriptGeneral", Order = 3)]
		public ValueUnit				DisplayUnit 		{ get; set; }

		public override bool SupportsAlerts { get { return true; } }

		private void DrawDollarText(ChartAnchor anchor, Point point, double price, ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale)
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

			dollarsString = GetPriceString(price, chartBars);

			Stroke color;
			textleftPoint	= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale).X;
			textRightPoint	= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale).X;

			if (anchor == RewardAnchor)		color = TargetLineStroke;
			else if (anchor == RewardAnchor1)	color = TargetLineStroke;
			else if (anchor == RewardAnchor2)	color = TargetLineStroke;
			else if (anchor == RewardAnchor3)	color = TargetLineStroke;
			else if (anchor == RewardAnchor4)	color = TargetLineStroke;
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

		private string GetPriceString(double price, ChartBars chartBars)
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
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize).ToString("F0") :
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize).ToString("F0"),
							price > yValueEntry ?
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize * (tickSize * pointValue * Account.All[0].ForexLotSize)) :
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize * (tickSize * pointValue * Account.All[0].ForexLotSize)));
						dollarsString = dollarsString.Replace(".00",string.Empty);
					}
					else
					{
						dollarsString = string.Format("{0}={1}",
							price > yValueEntry ?
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize).ToString("F0") :
								(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize).ToString("F0"),
							price > yValueEntry ?
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize * (tickSize * pointValue)) :
								Core.Globals.FormatCurrency(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize * (tickSize * pointValue)));
						dollarsString = dollarsString.Replace(".00",string.Empty).Replace("0=$0",AttachedTo.Instrument.MasterInstrument.FormatPrice(yValueEntry));
					}
					break;
				case ValueUnit.Percent:
					dollarsString = price > yValueEntry ?
						(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / yValueEntry).ToString("P", Core.Globals.GeneralOptions.CurrentCulture) :
						(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / yValueEntry).ToString("P", Core.Globals.GeneralOptions.CurrentCulture);
					break;
				case ValueUnit.Ticks:
					dollarsString = price > yValueEntry ?
						(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(price - yValueEntry) / tickSize).ToString("F0") :
						(AttachedTo.Instrument.MasterInstrument.RoundToTickSize(yValueEntry - price) / tickSize).ToString("F0");
					break;
				case ValueUnit.Pips:
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
						// we set the anchor for the target after stop mouse down event via SetReward()
						//however we need make sure Time is in view when builiding, but always when SetRreward is used..
						RewardAnchor.Time		= EntryAnchor.Time;
						RewardAnchor.SlotIndex	= EntryAnchor.SlotIndex;
						RewardAnchor.IsEditing	= false;
						
						RewardAnchor1.Time		= EntryAnchor.Time;
						RewardAnchor1.SlotIndex	= EntryAnchor.SlotIndex;
						RewardAnchor1.IsEditing	= false;
						
						RewardAnchor2.Time		= EntryAnchor.Time;
						RewardAnchor2.SlotIndex	= EntryAnchor.SlotIndex;
						RewardAnchor2.IsEditing	= false;
						
						RewardAnchor3.Time		= EntryAnchor.Time;
						RewardAnchor3.SlotIndex	= EntryAnchor.SlotIndex;
						RewardAnchor3.IsEditing	= false;
						
						RewardAnchor4.Time		= EntryAnchor.Time;
						RewardAnchor4.SlotIndex	= EntryAnchor.SlotIndex;
						RewardAnchor4.IsEditing	= false;
						
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
				else if (RewardAnchor1.IsEditing)
					dataPoint.CopyDataValues(RewardAnchor1);
				else if (RewardAnchor2.IsEditing)
					dataPoint.CopyDataValues(RewardAnchor2);
				else if (RewardAnchor3.IsEditing)
					dataPoint.CopyDataValues(RewardAnchor3);
				else if (RewardAnchor4.IsEditing)
					dataPoint.CopyDataValues(RewardAnchor4);
			}
			else if (DrawingState == DrawingState.Editing && editingAnchor != null)
			{
				dataPoint.CopyDataValues(editingAnchor);
				if (editingAnchor != EntryAnchor)
				{
					if (editingAnchor != RewardAnchor && ratio.ApproxCompare(0) != 0)
						SetReward();
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
			targetPrice1	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor1.Price);
			targetPrice2	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor2.Price);
			targetPrice3	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor3.Price);
			targetPrice4	= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor4.Price);
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
		
			// this will be true right away to fix a restoral issue, so check if we really want to set reward
			if (needsRatioUpdate && DrawTarget)
				SetReward();
			
			ChartPanel chartPanel	= chartControl.ChartPanels[PanelIndex];
			Point entryPoint		= EntryAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point stopPoint			= RiskAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point targetPoint		= RewardAnchor.GetPoint(chartControl, chartPanel, chartScale);
			
			Point targetPoint1		= RewardAnchor1.GetPoint(chartControl, chartPanel, chartScale);
			Point targetPoint2		= RewardAnchor2.GetPoint(chartControl, chartPanel, chartScale);
			Point targetPoint3		= RewardAnchor3.GetPoint(chartControl, chartPanel, chartScale);
			Point targetPoint4		= RewardAnchor4.GetPoint(chartControl, chartPanel, chartScale);
						
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
			if (DrawTarget)
			{	
				AnchorLineStroke.RenderTarget 	= RenderTarget;
				RenderTarget.DrawLine(entryPoint.ToVector2(), targetPoint.ToVector2(), tmpBrush, AnchorLineStroke.Width, AnchorLineStroke.StrokeStyle);
				
				TargetLineStroke.RenderTarget		= RenderTarget;
				Point tp = new Point(targetPoint.X,0);
				double mult = 1;
				if(EntryAnchor.Price < RiskAnchor.Price) mult= -1;
				if(ratio1>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio1;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, chartControl, chartPanel, chartScale);
				}
				if(ratio2>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio2;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, chartControl, chartPanel, chartScale);
				}
				if(ratio3>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio3;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, chartControl, chartPanel, chartScale);
				}
				if(ratio4>0){
					var tprice = EntryAnchor.Price + mult*Math.Abs(EntryAnchor.Price-RiskAnchor.Price) * ratio4;
					var a = new ChartAnchor(RewardAnchor.Time, tprice, chartControl);
					tp = a.GetPoint(chartControl, chartPanel, chartScale);
					var targetStartVector	= new SharpDX.Vector2((float)lineStartX, (float)tp.Y);
					var targetEndVector		= new SharpDX.Vector2((float)lineEndX, (float)tp.Y);

					tmpBrush = IsInHitTest ? chartControl.SelectionBrush : TargetLineStroke.BrushDX;
					RenderTarget.DrawLine(targetStartVector, targetEndVector, tmpBrush, TargetLineStroke.Width, TargetLineStroke.StrokeStyle);
					DrawDollarText(a, tp, tprice, chartControl, chartPanel, chartScale);
				}
			}
			
			tmpBrush = IsInHitTest ? chartControl.SelectionBrush : EntryLineStroke.BrushDX;
			RenderTarget.DrawLine(entryStartVector, entryEndVector, tmpBrush, EntryLineStroke.Width, EntryLineStroke.StrokeStyle);
			DrawDollarText(EntryAnchor, entryPoint, entryPrice, chartControl, chartPanel, chartScale);

			tmpBrush = IsInHitTest ? chartControl.SelectionBrush : StopLineStroke.BrushDX;
			RenderTarget.DrawLine(stopStartVector, stopEndVector, tmpBrush, StopLineStroke.Width, StopLineStroke.StrokeStyle);
			DrawDollarText(RiskAnchor, stopPoint, stopPrice, chartControl, chartPanel, chartScale);
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= Custom.Resource.NinjaScriptDrawingToolRiskRewardDescription;
				Name						= "ARC RR";
				Ratio1 = 1.0;
				Ratio2 = 2.0;
				Ratio3 = 0;
				Ratio4 = 0;
				ratio						= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
				DisplayUnit					= ValueUnit.Price;
				AnchorLineStroke 			= new Stroke(Brushes.Black,	DashStyleHelper.Solid, 1f, 50);
				EntryLineStroke 			= new Stroke(Brushes.Yellow,	DashStyleHelper.Solid, 2f);
				StopLineStroke 				= new Stroke(Brushes.Red,	DashStyleHelper.Solid, 2f);
				TargetLineStroke 			= new Stroke(Brushes.Lime,	DashStyleHelper.Solid, 2f);
				EntryAnchor					= new ChartAnchor { IsEditing = true, DrawingTool = this };
				RiskAnchor					= new ChartAnchor { IsEditing = true, DrawingTool = this };
				RewardAnchor				= new ChartAnchor { IsEditing = true, DrawingTool = this };
				RewardAnchor1               = new ChartAnchor { IsEditing = true, DrawingTool = this };
				RewardAnchor2               = new ChartAnchor { IsEditing = true, DrawingTool = this };
				RewardAnchor3               = new ChartAnchor { IsEditing = true, DrawingTool = this };
				RewardAnchor4               = new ChartAnchor { IsEditing = true, DrawingTool = this };
                EntryAnchor.DisplayName		= Custom.Resource.NinjaScriptDrawingToolRiskRewardAnchorEntry;
				RiskAnchor.DisplayName		= Custom.Resource.NinjaScriptDrawingToolRiskRewardAnchorRisk;
				RewardAnchor.DisplayName	= Custom.Resource.NinjaScriptDrawingToolRiskRewardAnchorReward;
				
				RewardAnchor1.DisplayName	= "Reward1 Anchor";
				RewardAnchor2.DisplayName	= "Reward2 Anchor";
				RewardAnchor3.DisplayName	= "Reward3 Anchor";
				RewardAnchor4.DisplayName	= "Reward4 Anchor";
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
			
			risk 					= entryPrice - stopPrice;
			
			ratio					= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
			reward					= risk * ratio;
			
			ratio1					= Ratio1;
			reward1					= risk * ratio1;
			
			ratio2					= Ratio2;
			reward2					= risk * ratio2;
			
			ratio3				    = Ratio3;
			reward3					= risk * ratio3;
			
			ratio4					= Ratio4;
			reward4					= risk * ratio4;
			
			
			targetPrice 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward);
			targetPrice1 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward1);
			targetPrice2			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward2);
			targetPrice3 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward3);
			targetPrice4 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward4);
			
			
			
			
			RewardAnchor.Price		= targetPrice;
			RewardAnchor.IsEditing	= false;
			
			RewardAnchor1.Price		= targetPrice1;
			RewardAnchor1.IsEditing	= false;
			
			RewardAnchor2.Price		= targetPrice2;
			RewardAnchor2.IsEditing	= false;
			
			RewardAnchor3.Price		= targetPrice3;
			RewardAnchor3.IsEditing	= false;
			
			RewardAnchor4.Price		= targetPrice4;
			RewardAnchor4.IsEditing	= false;
		
			needsRatioUpdate		= false;
		}
		
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetRisk()
		{
			if (Anchors == null || AttachedTo == null)
				return;

			/*
			entryPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			targetPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor.Price);
			
			targetPrice1			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor1.Price);
			targetPrice2			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor2.Price);
			targetPrice3			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor3.Price);
			targetPrice4			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor4.Price);
		
			reward					= targetPrice - entryPrice;
			
			ratio					= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
			risk					= reward / ratio;
			stopPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice - risk);
			
			RiskAnchor.Price 		= stopPrice;
			RiskAnchor.IsEditing	= false;
		
			needsRatioUpdate		= false;
			*/
			
			entryPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(EntryAnchor.Price);
			targetPrice				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(RewardAnchor.Price);
			
			reward					= targetPrice - entryPrice;
			
			ratio					= Math.Max(Ratio1, Math.Max(Ratio2, Math.Max(Ratio3, Ratio4)));
			risk					= reward / ratio;
			
			ratio1					= Ratio1;
			reward1					= risk * ratio1;
			
			ratio2					= Ratio2;
			reward2					= risk * ratio2;
			
			ratio3				    = Ratio3;
			reward3					= risk * ratio3;
			
			ratio4					= Ratio4;
			reward4					= risk * ratio4;
			
			targetPrice1 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward1);
			targetPrice2			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward2);
			targetPrice3 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward3);
			targetPrice4 			= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice + reward4);
			
			
			stopPrice 				= AttachedTo.Instrument.MasterInstrument.RoundToTickSize(entryPrice - risk);
			
			RiskAnchor.Price 		= stopPrice;
			RiskAnchor.IsEditing	= false;
			
			RewardAnchor1.Price		= targetPrice1;
			RewardAnchor1.IsEditing	= false;
			
			RewardAnchor2.Price		= targetPrice2;
			RewardAnchor2.IsEditing	= false;
			
			RewardAnchor3.Price		= targetPrice3;
			RewardAnchor3.IsEditing	= false;
			
			RewardAnchor4.Price		= targetPrice4;
			RewardAnchor4.IsEditing	= false;
		
			needsRatioUpdate		= false;
		}
	}

	public static partial class Draw
	{
		private static ARCRR ARCRRCore(NinjaScriptBase owner, string tag,
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

			ARCRR riskReward = DrawingTool.GetByTagOrNew(owner, typeof(ARCRR), tag, templateName) as ARCRR;
			
			if (riskReward == null)
				return null;

			DrawingTool.SetDrawingToolCommonValues(riskReward, tag, isAutoScale, owner, isGlobal);

			// this is a little tricky, we use entry + (stop or target) to calculate the (target or stop) from ratio
			ChartAnchor	entryAnchor		= DrawingTool.CreateChartAnchor(owner, entryBarsAgo, entryTime, entryY);
			ChartAnchor	stopAnchor;
			ChartAnchor	targetAnchor;
			
			ChartAnchor	targetAnchor1;
			ChartAnchor	targetAnchor2;
			ChartAnchor	targetAnchor3;
			ChartAnchor	targetAnchor4;
			

//			riskReward.Ratio = ratio;
			
			if (isStop)
			{
				stopAnchor = DrawingTool.CreateChartAnchor(owner, stopBarsAgo, stopTime, stopY);
				entryAnchor.CopyDataValues(riskReward.EntryAnchor);
				entryAnchor.CopyDataValues(riskReward.RewardAnchor);
				stopAnchor.CopyDataValues(riskReward.RiskAnchor);
				riskReward.SetReward();
			}
			else 
			{
				targetAnchor = DrawingTool.CreateChartAnchor(owner, targetBarsAgo, targetTime, targetY);
				targetAnchor1 = DrawingTool.CreateChartAnchor(owner, targetBarsAgo, targetTime, targetY);
				targetAnchor2 = DrawingTool.CreateChartAnchor(owner, targetBarsAgo, targetTime, targetY);
				targetAnchor3 = DrawingTool.CreateChartAnchor(owner, targetBarsAgo, targetTime, targetY);
				targetAnchor4 = DrawingTool.CreateChartAnchor(owner, targetBarsAgo, targetTime, targetY);
				
				entryAnchor.CopyDataValues(riskReward.EntryAnchor);
				entryAnchor.CopyDataValues(riskReward.RiskAnchor);
				targetAnchor.CopyDataValues(riskReward.RewardAnchor);
				targetAnchor1.CopyDataValues(riskReward.RewardAnchor1);
				targetAnchor2.CopyDataValues(riskReward.RewardAnchor2);
				targetAnchor3.CopyDataValues(riskReward.RewardAnchor3);
				targetAnchor4.CopyDataValues(riskReward.RewardAnchor4);
				riskReward.SetRisk();
			}
			
			riskReward.SetState(State.Active);
			return riskReward;
		}

		/// <summary>
		/// Draws a risk/reward on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryTime">The time where the draw object's entry will be drawn</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the risk or reward based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <returns></returns>
		public static ARCRR ARC_RR(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime entryTime, double entryY, DateTime endTime, double endY, double ratio, bool isStop)
		{
			return isStop
				? ARCRRCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, int.MinValue, endTime, endY, 0, Core.Globals.MinDate, 0, ratio, true, false, null)
				: ARCRRCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, 0, Core.Globals.MinDate, 0, int.MinValue, endTime, endY, ratio, false, false, null);
		}

		/// <summary>
		/// Draws a risk/reward on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryBarsAgo">The starting bar (x axis coordinate) where the draw object's entry will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the risk or reward based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <returns></returns>
		public static ARCRR ARC_RR(NinjaScriptBase owner, string tag, bool isAutoScale, int entryBarsAgo, double entryY, int endBarsAgo, double endY, double ratio, bool isStop)
		{
			return isStop
				? ARCRRCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, endBarsAgo, Core.Globals.MinDate, endY, 0, Core.Globals.MinDate, 0, ratio, true, false, null)
				: ARCRRCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, 0, Core.Globals.MinDate, 0, endBarsAgo, Core.Globals.MinDate, endY, ratio, false, false, null);
		}

		/// <summary>
		/// Draws a risk/reward on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryTime">The time where the draw object's entry will be drawn</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the risk or reward based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ARCRR ARC_RR(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime entryTime, double entryY, DateTime endTime, double endY, double ratio, bool isStop, bool isGlobal, string templateName)
		{
			return isStop
				? ARCRRCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, int.MinValue, endTime, endY, 0, Core.Globals.MinDate, 0, ratio, true, isGlobal, templateName)
				: ARCRRCore(owner, tag, isAutoScale, int.MinValue, entryTime, entryY, 0, Core.Globals.MinDate, 0, int.MinValue, endTime, endY, ratio, false, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a risk/reward on a chart.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="entryBarsAgo">The starting bar (x axis coordinate) where the draw object's entry will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="entryY">The y value coordinate where the draw object's entry price will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="ratio">An int value determining the calculated ratio between the risk or reward based on the entry point</param>
		/// <param name="isStop">A bool value, when true will use the endTime/endBarsAgo and endY to set the stop, and will automatically calculate the target based off the ratio value.</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ARCRR ARC_RR(NinjaScriptBase owner, string tag, bool isAutoScale, int entryBarsAgo, double entryY, int endBarsAgo, double endY, double ratio, bool isStop, bool isGlobal, string templateName)
		{
			return isStop
				? ARCRRCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, endBarsAgo, Core.Globals.MinDate, endY, 0, Core.Globals.MinDate, 0, ratio, true, isGlobal, templateName)
				: ARCRRCore(owner, tag, isAutoScale, entryBarsAgo, Core.Globals.MinDate, entryY, 0, Core.Globals.MinDate, 0, endBarsAgo, Core.Globals.MinDate, endY, ratio, false, isGlobal, templateName);
		}
	}
}
