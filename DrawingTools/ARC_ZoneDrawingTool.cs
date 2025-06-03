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
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
#endregion

namespace NinjaTrader.NinjaScript.DrawingTools
{
	/// <summary>
	/// Represents an interface that exposes information regarding a Fibonacci Retracements IDrawingTool.
	/// </summary>
	public class POCtool : FibonacciLevels
	{
		public override object Icon { get { return Icons.DrawFbRetracement; } }

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsExtendLinesRight", GroupName = "NinjaScriptLines")]
		public bool 					IsExtendedLinesRight 	{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsExtendLinesLeft", GroupName = "NinjaScriptLines")]
		public bool 					IsExtendedLinesLeft 	{ get; set; }
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolFibonacciRetracementsTextLocation", GroupName = "NinjaScriptGeneral")]
		public TextLocation				TextLocation { get; set; }
		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Max Risk $", GroupName="NinjaScriptGeneral", Order = 50)]
		public double MaxRisk_Dollars
		{
			get { return maxrisk_dollars; }
			set
			{
				maxrisk_dollars		= value;
			}
		}
		private double maxrisk_dollars = 100;
		private double qty = 0;

		protected bool CheckAlertRetracementLine(Condition condition, Point lineStartPoint, Point lineEndPoint,
													ChartControl chartControl, ChartScale chartScale, ChartAlertValue[] values)
		{
			// not completely drawn yet?
			if (Anchors.Count(a => a.IsEditing) > 1)
				return false;

			if (values[0].ValueType == ChartAlertValueType.StaticTime)
			{
				int checkX = chartControl.GetXByTime(values[0].Time);
				return lineStartPoint.X >= checkX || lineEndPoint.X >= checkX;
			}

			double firstBarX	= chartControl.GetXByTime(values[0].Time);
			double firstBarY	= chartScale.GetYByValue(values[0].Value);
			Point barPoint		= new Point(firstBarX, firstBarY);

			 // bars passed our drawing tool line
			if (lineEndPoint.X < firstBarX)
				return false;	

			// bars not yet to our drawing tool line
			if (lineStartPoint.X > firstBarX)
				return false;

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
						if (v.Time == Core.Globals.MinDate)
							return false;
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

		protected void DrawPriceLevelText(ChartPanel chartPanel, ChartScale chartScale, double minX, double maxX, double y, double price, PriceLevel priceLevel)
		{
			if (TextLocation == TextLocation.Off || priceLevel == null || priceLevel.Stroke == null || priceLevel.Stroke.BrushDX == null)
				return;
			
			// make a rectangle that sits right at our line, depending on text alignment settings
			SimpleFont						wpfFont		= chartPanel.ChartControl.Properties.LabelFont ?? new SimpleFont();
			SharpDX.DirectWrite.TextFormat	textFormat	= wpfFont.ToDirectWriteTextFormat();
			textFormat.TextAlignment					= SharpDX.DirectWrite.TextAlignment.Leading;
			textFormat.WordWrapping						= SharpDX.DirectWrite.WordWrapping.NoWrap;
			
			double diff = EndAnchor.Price - StartAnchor.Price;
			qty	= Math.Round(MaxRisk_Dollars / AttachedTo.Instrument.MasterInstrument.PointValue / Math.Abs(diff),0);

			string str		= GetPriceString(price, priceLevel);
			string dollarsString = string.Empty;
			if(str.Length>0 && qty<0)
				dollarsString = string.Format("{0} @ {1}",qty, str);
			else if(str.Length>0 && qty>0)
				dollarsString = string.Format("+{0} @ {1}",qty, str);
			else if(str.Length==0 && qty<0)
				dollarsString = string.Format("{0}",qty);
			else if(str.Length==0 && qty>0)
				dollarsString = string.Format("+{0}",qty);
			else //if qty is zero, don't print any qty information
				dollarsString = str + " Max risk too small";

			// when using extreme alignments, give a few pixels of padding on the text so we dont end up right on the edge
			const double	edgePadding	= 2f;
			float			layoutWidth	= (float)Math.Abs(maxX - minX); // always give entire available width for layout
			// dont use max x for max text width here, that can break inside left/right when extended lines are on
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, dollarsString, textFormat, layoutWidth, textFormat.FontSize);

			double drawAtX;

			if (IsExtendedLinesLeft && TextLocation == TextLocation.ExtremeLeft)
				drawAtX = chartPanel.X + edgePadding;
			else if (IsExtendedLinesRight && TextLocation == TextLocation.ExtremeRight)
				drawAtX = chartPanel.X + chartPanel.W - textLayout.Metrics.Width;
			else
			{
				if (TextLocation == TextLocation.InsideLeft || TextLocation == TextLocation.ExtremeLeft )
					drawAtX = minX - 1;
				else
					drawAtX = maxX - 1 - textLayout.Metrics.Width;
			}

			// we also move our y value up by text height so we draw label above line like NT7.
			RenderTarget.DrawTextLayout(new SharpDX.Vector2((float)drawAtX, (float)(y - textFormat.FontSize - edgePadding)),  textLayout, priceLevel.Stroke.BrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

			textFormat.Dispose();
			textLayout.Dispose();
		}
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetQuantities()
		{
			if (Anchors == null || AttachedTo == null)
				return;
			double diff = EndAnchor.Price - StartAnchor.Price;
			qty	= Math.Round(MaxRisk_Dollars / AttachedTo.Instrument.MasterInstrument.PointValue / Math.Abs(diff),0);
		}

		public override Cursor GetCursor(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, Point point)
		{
			switch (DrawingState)
			{
				case DrawingState.Building:	return Cursors.Pen;
				case DrawingState.Moving:	return IsLocked ? Cursors.No : Cursors.SizeAll;
				case DrawingState.Editing:
					if (IsLocked)
						return Cursors.No;
					return editingAnchor == StartAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE;
				default:
					// draw move cursor if cursor is near line path anywhere
					Point startAnchorPixelPoint	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);

					ChartAnchor closest = GetClosestAnchor(chartControl, chartPanel, chartScale, CursorSensitivity, point);
					if (closest != null)
					{
						if (IsLocked)
							return null;
						return closest == StartAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE;
					}

					Vector	totalVector	= EndAnchor.GetPoint(chartControl, chartPanel, chartScale) - startAnchorPixelPoint;
					return MathHelper.IsPointAlongVector(point, startAnchorPixelPoint, totalVector, CursorSensitivity) ? 
						IsLocked ? Cursors.Arrow : Cursors.SizeAll :
						null;
			}
		}

		// Item1 = leftmost point, Item2 rightmost point of line
		protected Tuple<Point, Point> GetPriceLevelLinePoints(PriceLevel priceLevel, ChartControl chartControl, ChartScale chartScale, bool isInverted)
		{
			ChartPanel chartPanel	= chartControl.ChartPanels[PanelIndex];
			Point anchorStartPoint 	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point anchorEndPoint 	= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);
			double totalPriceRange 	= EndAnchor.Price - StartAnchor.Price;
			// dont forget user could start/end draw backwards
			double anchorMinX 		= Math.Min(anchorStartPoint.X, anchorEndPoint.X);
			double anchorMaxX 		= Math.Max(anchorStartPoint.X, anchorEndPoint.X);
			double lineStartX		= IsExtendedLinesLeft ? chartPanel.X : anchorMinX;
			double lineEndX 		= IsExtendedLinesRight ? chartPanel.X + chartPanel.W : anchorMaxX;
			double levelY			= priceLevel.GetY(chartScale, StartAnchor.Price, totalPriceRange, isInverted);
			return new Tuple<Point, Point>(new Point(lineStartX, levelY), new Point(lineEndX, levelY));
		}

		private string GetPriceString(double price, PriceLevel priceLevel)
		{
//			return string.Empty;
			// note, dont use MasterInstrument.FormatPrice() as it will round value to ticksize which we do not want
			string priceStr	= price.ToString(Core.Globals.GetTickFormatString(AttachedTo.Instrument.MasterInstrument.TickSize));
			string str		= string.Format("{0}", priceStr);
			return str;
		}

		public override Point[] GetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel chartPanel = chartControl.ChartPanels[PanelIndex];
			
			Point startPoint 	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point endPoint 		= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point midPoint		= new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
			
			return new[] { startPoint, midPoint, endPoint };
		}

		
		public override bool IsAlertConditionTrue(AlertConditionItem conditionItem, Condition condition, ChartAlertValue[] values, ChartControl chartControl, ChartScale chartScale)
		{
			PriceLevel priceLevel = conditionItem.Tag as PriceLevel;
			if (priceLevel == null)
				return false;
			Tuple<Point, Point>	plp = GetPriceLevelLinePoints(priceLevel, chartControl, chartScale, true);
			return CheckAlertRetracementLine(condition, plp.Item1, plp.Item2, chartControl, chartScale, values);
		}

		public override bool IsVisibleOnChart(ChartControl chartControl, ChartScale chartScale, DateTime firstTimeOnChart, DateTime lastTimeOnChart)
		{
			if (DrawingState == DrawingState.Building)
				return true;

			DateTime minTime = Core.Globals.MaxDate;
			DateTime maxTime = Core.Globals.MinDate;
			foreach (ChartAnchor anchor in Anchors)
			{
				if (anchor.Time < minTime)
					minTime = anchor.Time;
				if (anchor.Time > maxTime)
					maxTime = anchor.Time;
			}

			if (!IsExtendedLinesLeft && !IsExtendedLinesRight)
				return new[]{minTime,maxTime}.Any(t => t >= firstTimeOnChart && t <= lastTimeOnChart) || (minTime < firstTimeOnChart && maxTime > lastTimeOnChart);

			return true;
		}

		public override void OnCalculateMinMax()
		{
			MinValue = double.MaxValue;
			MaxValue = double.MinValue;

			if (!IsVisible)
				return;

			// make sure *something* is drawn yet, but dont blow up if editing just a single anchor
			if (Anchors.All(a => a.IsEditing))
				return;

			double totalPriceRange 	= EndAnchor.Price - StartAnchor.Price;
			double startPrice = StartAnchor.Price;// + yPriceOffset;
			foreach (PriceLevel priceLevel in PriceLevels.Where(pl => pl.IsVisible && pl.Stroke != null))
			{
				double levelPrice	= startPrice + (1 - priceLevel.Value/100) * totalPriceRange;
				MinValue = Math.Min(MinValue, levelPrice);
				MaxValue = Math.Max(MaxValue, levelPrice);
			}
		}

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			switch (DrawingState)
			{
				case DrawingState.Building:
					if (StartAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(StartAnchor);
						// give end anchor something to start with so we dont try to render it with bad values right away
						dataPoint.CopyDataValues(EndAnchor);
						StartAnchor.IsEditing = false;
					}
					else if (EndAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(EndAnchor);
						EndAnchor.IsEditing = false;
					}
					
					// is initial building done (both anchors set)
					if (!StartAnchor.IsEditing && !EndAnchor.IsEditing)
					{
						DrawingState = DrawingState.Normal;
						IsSelected = false; 
					}
					break;
				case DrawingState.Normal:
					Point point = dataPoint.GetPoint(chartControl, chartPanel, chartScale);
					editingAnchor = GetClosestAnchor(chartControl, chartPanel, chartScale, CursorSensitivity, point);
					if (editingAnchor != null)
					{
						editingAnchor.IsEditing = true;
						DrawingState = DrawingState.Editing;
					}
					else if (editingAnchor == null || IsLocked)
					{
						// or if they didnt click particulary close to either, move (they still clicked close to our line)
						// set it to moving even if locked so we know to change cursor
						if (GetCursor(chartControl, chartPanel, chartScale, point) != null)
							DrawingState = DrawingState.Moving;
						else
							IsSelected = false;
					}
					break;
			}
		}
		
		public override void OnMouseMove(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			if (IsLocked && DrawingState != DrawingState.Building)
				return;

			if (DrawingState == DrawingState.Building)
			{
				// start anchor will not be editing here because we start building as soon as user clicks, which
				// plops down a start anchor right away
				if (EndAnchor.IsEditing)
					dataPoint.CopyDataValues(EndAnchor);
			}
			else if (DrawingState == DrawingState.Editing && editingAnchor != null)
				dataPoint.CopyDataValues(editingAnchor);
			else if (DrawingState == DrawingState.Moving)
				foreach (ChartAnchor anchor in Anchors)
					anchor.MoveAnchor(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
		}
		
		public override void OnMouseUp(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			// simply end whatever moving
			if (DrawingState == DrawingState.Editing || DrawingState == DrawingState.Moving)
				DrawingState = DrawingState.Normal;
			if (editingAnchor != null)
				editingAnchor.IsEditing = false;
			editingAnchor = null;
		}
		
		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// nothing is drawn yet
			if (Anchors.All(a => a.IsEditing))
				return;
			
			RenderTarget.AntialiasMode			= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			ChartPanel chartPanel				= chartControl.ChartPanels[PanelIndex];
			// get x distance of the line, this will be basis for our levels
			// unless extend left/right is also on
			Point anchorStartPoint 				= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point anchorEndPoint 				= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);
			
			AnchorLineStroke.RenderTarget		= RenderTarget;
			
			SharpDX.Direct2D1.Brush tmpBrush	= IsInHitTest ? chartControl.SelectionBrush : AnchorLineStroke.BrushDX;
			RenderTarget.DrawLine(anchorStartPoint.ToVector2(), anchorEndPoint.ToVector2(), tmpBrush, AnchorLineStroke.Width, AnchorLineStroke.StrokeStyle);
			
			// if we're doing a hit test pass, dont draw price levels at all, we dont want those to count for 
			// hit testing (match NT7)
			if (IsInHitTest || PriceLevels == null || !PriceLevels.Any())
				return;
			
			SetAllPriceLevelsRenderTarget();

			Point	lastStartPoint	= new Point(0, 0);
			Stroke	lastStroke		= null;

			foreach (PriceLevel priceLevel in PriceLevels.Where(pl => pl.IsVisible && pl.Stroke != null).OrderBy(p=>p.Value))
			{
				Tuple<Point, Point>	plp = GetPriceLevelLinePoints(priceLevel, chartControl, chartScale, true);

				// align to full pixel to avoid unneeded aliasing
				double strokePixAdj =	(priceLevel.Stroke.Width % 2.0).ApproxCompare(0) == 0 ? 0.5d : 0d;
				Vector pixelAdjustVec = new Vector(strokePixAdj, strokePixAdj);
			
				RenderTarget.DrawLine((plp.Item1 + pixelAdjustVec).ToVector2(), (plp.Item2 + pixelAdjustVec).ToVector2(),
										priceLevel.Stroke.BrushDX, priceLevel.Stroke.Width, priceLevel.Stroke.StrokeStyle);

				if (lastStroke == null)
					lastStroke = new Stroke();
				else
				{
					SharpDX.RectangleF borderBox = new SharpDX.RectangleF((float)lastStartPoint.X, (float)lastStartPoint.Y,
						(float)(plp.Item2.X + strokePixAdj - lastStartPoint.X), (float)(plp.Item2.Y - lastStartPoint.Y));

					RenderTarget.FillRectangle(borderBox, lastStroke.BrushDX);
				}

				priceLevel.Stroke.CopyTo(lastStroke);
				lastStroke.Opacity	= PriceLevelOpacity;
				lastStartPoint		= plp.Item1 + pixelAdjustVec;
			}

			// Render price text after background colors have rendered so the price text is on top
			foreach (PriceLevel priceLevel in PriceLevels.Where(pl => pl.IsVisible && pl.Stroke != null))
			{
				Tuple<Point, Point> plp = GetPriceLevelLinePoints(priceLevel, chartControl, chartScale, true);
				// dont always draw the text at min/max x the line renders at, pass anchor min max
				// in case text alignment is not extreme
				float	plPixAdjust		= (priceLevel.Stroke.Width % 2.0).ApproxCompare(0) == 0 ? 0.5f : 0f;
				double	anchorMinX		= Math.Min(anchorStartPoint.X, anchorEndPoint.X);
				double	anchorMaxX		= Math.Max(anchorStartPoint.X, anchorEndPoint.X) + plPixAdjust;

				double totalPriceRange 	= EndAnchor.Price - StartAnchor.Price;
				double price			= priceLevel.GetPrice(StartAnchor.Price, totalPriceRange, true);
				DrawPriceLevelText(chartPanel, chartScale, anchorMinX, anchorMaxX, plp.Item1.Y, price, priceLevel);
			}
		}
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AnchorLineStroke 			= new Stroke(Brushes.Orange, DashStyleHelper.Solid, 1f, 50);
				Name 						= "ARC Zone tool";
				PriceLevelOpacity			= 5;
				StartAnchor					= new ChartAnchor { IsEditing = true, DrawingTool = this };
				EndAnchor					= new ChartAnchor { IsEditing = true, DrawingTool = this };
				StartAnchor.DisplayName		= Custom.Resource.NinjaScriptDrawingToolAnchorStart;
				EndAnchor.DisplayName		= Custom.Resource.NinjaScriptDrawingToolAnchorEnd;
//				TextLocation = TextLocation.Off;
				IsExtendedLinesRight = true;
			}
			else if (State == State.Configure)
			{
				if (PriceLevels.Count == 0)
				{
					PriceLevels.Add(new PriceLevel(0,		Brushes.Orange));
					PriceLevels.Add(new PriceLevel(100,		Brushes.Orange));
				}
			}
			else if (State == State.Terminated)
				Dispose();
		}
	}
	public static partial class Draw
	{

		/// <summary>
		/// Draws a fibonacci retracement.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static POCtool PocTool(NinjaScriptBase owner, string tag, bool isAutoScale, 
			int startBarsAgo, double startY, int endBarsAgo, double endY, bool isGlobal, string templateName)
		{
			return FibonacciCore<POCtool>(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, 
				Core.Globals.MinDate, endY, isGlobal, templateName);
		}
	}
}
