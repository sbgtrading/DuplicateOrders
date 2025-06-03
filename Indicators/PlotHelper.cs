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
namespace NinjaTrader.NinjaScript//.Indicators
{
	public static class PlotHelper
	{
		public static void DrawArrowUp(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, double arrowWidth, double arrowBarWidth, double arrowHeight)
		{
			Point[] points = UpArrow(arrowWidth, arrowBarWidth, arrowHeight);
			SharpDX.Vector2[] pta = new SharpDX.Vector2[points.Length];
			for (int i=0;i<points.Length;i++)
			{
				points[i].Offset(x,y);
				pta[i] = new SharpDX.Vector2((float)points[i].X,(float)points[i].Y);
			}
			
			SharpDX.Direct2D1.PathGeometry geo1;
    		SharpDX.Direct2D1.GeometrySink sink1;
			
			geo1 = new SharpDX.Direct2D1.PathGeometry(target.Factory);
            sink1 = geo1.Open();
            sink1.BeginFigure(pta[0],new SharpDX.Direct2D1.FigureBegin());
			for (int xx=1;xx<pta.Length;xx++)			
				sink1.AddLine(pta[xx]);
            sink1.EndFigure(new SharpDX.Direct2D1.FigureEnd());
            sink1.Close();
			
			target.DrawGeometry(geo1, brush.ToDxBrush(target));
			
			geo1.Dispose();
            sink1.Dispose();
		}
		
		public static void FillArrowUp(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, double arrowWidth, double arrowBarWidth, double arrowHeight)
		{
			Point[] points = UpArrow(arrowWidth, arrowBarWidth, arrowHeight);
			SharpDX.Vector2[] pta = new SharpDX.Vector2[points.Length];
			for (int i=0;i<points.Length;i++)
			{
				points[i].Offset(x,y);
				pta[i] = new SharpDX.Vector2((float)points[i].X,(float)points[i].Y);
			}
			
			SharpDX.Direct2D1.PathGeometry geo1;
    		SharpDX.Direct2D1.GeometrySink sink1;
			
			geo1 = new SharpDX.Direct2D1.PathGeometry(target.Factory);
            sink1 = geo1.Open();
            sink1.BeginFigure(pta[0],new SharpDX.Direct2D1.FigureBegin());
			for (int xx=1;xx<pta.Length;xx++)			
				sink1.AddLine(pta[xx]);
            sink1.EndFigure(new SharpDX.Direct2D1.FigureEnd());
            sink1.Close();
			
			target.FillGeometry(geo1, brush.ToDxBrush(target));
			
			geo1.Dispose();
            sink1.Dispose();
		}
		
		public static void DrawArrowDown(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, double arrowWidth, double arrowBarWidth, double arrowHeight)
		{
			Point[] points = DownArrow(arrowWidth, arrowBarWidth, arrowHeight);
			SharpDX.Vector2[] pta = new SharpDX.Vector2[points.Length];
			for (int i=0;i<points.Length;i++)
			{
				points[i].Offset(x,y);
				pta[i] = new SharpDX.Vector2((float)points[i].X,(float)points[i].Y);
			}
			
			SharpDX.Direct2D1.PathGeometry geo1;
    		SharpDX.Direct2D1.GeometrySink sink1;
			
			geo1 = new SharpDX.Direct2D1.PathGeometry(target.Factory);
            sink1 = geo1.Open();
            sink1.BeginFigure(pta[0],new SharpDX.Direct2D1.FigureBegin());
			for (int xx=1;xx<pta.Length;xx++)			
				sink1.AddLine(pta[xx]);
            sink1.EndFigure(new SharpDX.Direct2D1.FigureEnd());
            sink1.Close();
			
			target.DrawGeometry(geo1, brush.ToDxBrush(target));
			
			geo1.Dispose();
            sink1.Dispose();
		}
		
		public static void FillArrowDown(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, double arrowWidth, double arrowBarWidth, double arrowHeight)
		{
			Point[] points = DownArrow(arrowWidth, arrowBarWidth, arrowHeight);
			SharpDX.Vector2[] pta = new SharpDX.Vector2[points.Length];
			for (int i=0;i<points.Length;i++)
			{
				points[i].Offset(x,y);
				pta[i] = new SharpDX.Vector2((float)points[i].X,(float)points[i].Y);
			}
			
			SharpDX.Direct2D1.PathGeometry geo1;
    		SharpDX.Direct2D1.GeometrySink sink1;
			
			geo1 = new SharpDX.Direct2D1.PathGeometry(target.Factory);
            sink1 = geo1.Open();
            sink1.BeginFigure(pta[0],new SharpDX.Direct2D1.FigureBegin());
			for (int xx=1;xx<pta.Length;xx++)			
				sink1.AddLine(pta[xx]);
            sink1.EndFigure(new SharpDX.Direct2D1.FigureEnd());
            sink1.Close();
			
			target.FillGeometry(geo1, brush.ToDxBrush(target));
			
			geo1.Dispose();
            sink1.Dispose();
		}
		
		public static Point[] UpArrow(double arrowWidth, double arrowBarWidth, double arrowHeight)
		{
			double halfWidth = arrowWidth/2.0;
			double width1 = (arrowWidth-arrowBarWidth)/2.0;
			double thirdHeight = arrowHeight/3.0;
			
			Point[] points = new Point[8];
			points[0] = new Point(0,0);
			points[1] = new Point(-halfWidth, thirdHeight);
			points[2] = new Point(-halfWidth+width1, thirdHeight);
			points[3] = new Point(-halfWidth+width1, arrowHeight);
			points[4] = new Point(halfWidth-width1, arrowHeight);
			points[5] = new Point(halfWidth-width1, thirdHeight);
			points[6] = new Point(halfWidth, thirdHeight);
			points[7] = new Point(0,0);
			return points;			
		}
		
		public static Point[] DownArrow(double arrowWidth, double arrowBarWidth, double arrowHeight)
		{
			double halfWidth = arrowWidth/2.0;
			double width1 = (arrowWidth-arrowBarWidth)/2.0;
			double thirdHeight = arrowHeight/3.0;
			
			Point[] points = new Point[8];
			points[0] = new Point(0,0);
			points[1] = new Point(-halfWidth, -thirdHeight);
			points[2] = new Point(-halfWidth+width1, -thirdHeight);
			points[3] = new Point(-halfWidth+width1, -arrowHeight);
			points[4] = new Point(halfWidth-width1, -arrowHeight);
			points[5] = new Point(halfWidth-width1, -thirdHeight);
			points[6] = new Point(halfWidth, -thirdHeight);
			points[7] = new Point(0,0);
			return points;				
		}
		
		public static void FillRectangle(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, float width, float height)
		{
			SharpDX.Direct2D1.Brush brushDX = brush.ToDxBrush(target);
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x,y,width,height);
			target.FillRectangle(rect, brushDX);
			brushDX.Dispose();
		}		
		
		public static void DrawRectangle(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, float width, float height)
		{
			SharpDX.Direct2D1.Brush brushDX = brush.ToDxBrush(target);
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x,y,width,height);
			target.DrawRectangle(rect, brushDX);
			brushDX.Dispose();
		}
		
		public static void DrawRectangle(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, float width, float height, float outlineWidth, SharpDX.Direct2D1.StrokeStyle style)
		{
			SharpDX.Direct2D1.Brush brushDX = brush.ToDxBrush(target);
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x,y,width,height);
			target.DrawRectangle(rect, brushDX,outlineWidth,style);
			brushDX.Dispose();
		}
			
		public static void FillEllipse(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, float xRadius, float yRadius)
		{
			SharpDX.Vector2 center = new SharpDX.Vector2(x,y);			
			SharpDX.Direct2D1.Ellipse ellipse = new SharpDX.Direct2D1.Ellipse(center, xRadius, yRadius);			
			target.FillEllipse(ellipse, brush.ToDxBrush(target));
		}
		
		public static void DrawEllipse(SharpDX.Direct2D1.RenderTarget target, Brush brush, float x, float y, float xRadius, float yRadius)
		{
			SharpDX.Vector2 center = new SharpDX.Vector2(x,y);			
			SharpDX.Direct2D1.Ellipse ellipse = new SharpDX.Direct2D1.Ellipse(center, xRadius, yRadius);			
			target.DrawEllipse(ellipse, brush.ToDxBrush(target));
		}
		
		public static void DrawText(SharpDX.Direct2D1.RenderTarget target, SimpleFont font, Brush brush, string str, float x, float y, float maxWidth)
		{
			SharpDX.Direct2D1.Brush brushDX = brush.ToDxBrush(target);
			SharpDX.DirectWrite.TextFormat textFormat1 = font.ToDirectWriteTextFormat();			
			SharpDX.Vector2 upperTextPoint = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout1 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
					str, textFormat1, maxWidth, textFormat1.FontSize);
			target.DrawTextLayout(upperTextPoint, textLayout1, brushDX);
			
			textFormat1.Dispose();
			textLayout1.Dispose();
			brushDX.Dispose();
		}
		
		public static void DrawText(SharpDX.Direct2D1.RenderTarget target, SimpleFont font, Brush brush, string str, float x, float y, float maxWidth, SharpDX.DirectWrite.TextAlignment textAlignment)
		{
			using (SharpDX.DirectWrite.TextFormat textFormat1 = font.ToDirectWriteTextFormat())
			{
				textFormat1.TextAlignment = textAlignment;
				
				SharpDX.Vector2 upperTextPoint = new SharpDX.Vector2(x, y);
				
				using (SharpDX.DirectWrite.TextLayout textLayout1 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,str, textFormat1, maxWidth, textFormat1.FontSize))										
				{
					SharpDX.Direct2D1.Brush brushDX = brush.ToDxBrush(target);
					target.DrawTextLayout(upperTextPoint, textLayout1, brushDX);
					brushDX.Dispose();
				}
			}
		}
		
		public static void DrawLine(SharpDX.Direct2D1.RenderTarget target, float x1, float y1, float x2, float y2, Brush brush, float width)
		{
			SharpDX.Vector2 startPoint = new SharpDX.Vector2(x1, y1);
			SharpDX.Vector2 endPoint = new SharpDX.Vector2(x2, y2);							
			
			target.DrawLine(startPoint,endPoint,brush.ToDxBrush(target),width);
		}
		
		public static void DrawLine(SharpDX.Direct2D1.RenderTarget target, float x1, float y1, float x2, float y2, Brush brush, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			SharpDX.Vector2 startPoint = new SharpDX.Vector2(x1, y1);
			SharpDX.Vector2 endPoint = new SharpDX.Vector2(x2, y2);							
			
			target.DrawLine(startPoint,endPoint,brush.ToDxBrush(target),width,strokeStyle);
		}
		
		public static Size MeasureTextSize(string text, FontFamily fontFamily, double fontSize, Typeface typeface, Brush brush)
	    {					
	        FormattedText ft = new FormattedText(text,
	                                             System.Globalization.CultureInfo.CurrentCulture,
	                                             FlowDirection.LeftToRight,
	                                             typeface,
	                                             fontSize,
	                                             brush);					
			
			return new Size(ft.Width, ft.Height);
	    }		
		
		public static Size MeasureTextSize(SharpDX.Direct2D1.RenderTarget target, string text, FontFamily fontFamily, double fontSize, Typeface typeface, Brush brush)
	    {
			SharpDX.Size2F dpi = target.Factory.DesktopDpi;
			double widthMult = dpi.Width/96.0;
			double heightMult = dpi.Height/96.0;
			
	        FormattedText ft = new FormattedText(text,
	                                             System.Globalization.CultureInfo.CurrentCulture,
	                                             FlowDirection.LeftToRight,
	                                             typeface,
	                                             fontSize,
	                                             brush);					
			
			return new Size(ft.Width*widthMult, ft.Height*heightMult);
	    }
		
		public static void ShadeRegion(SharpDX.Direct2D1.RenderTarget target, Point[] points, Brush geoBrush)
		{
			SharpDX.Direct2D1.PathGeometry geo = new SharpDX.Direct2D1.PathGeometry(target.Factory);
			SharpDX.Direct2D1.GeometrySink snk;

			SharpDX.Vector2[] vec = new SharpDX.Vector2[points.Length];

			for (int x=0;x<vec.Length;x++)			
				vec[x] = new SharpDX.Vector2((float)points[x].X,(float)points[x].Y);			

			snk = geo.Open();
			snk.BeginFigure(vec[0], new SharpDX.Direct2D1.FigureBegin());
			//snk.AddLines(new SharpDX.Vector2[] { vec[1], vec[2], vec[3] });
			for (int x=1;x<vec.Length;x++)
				snk.AddLine(vec[x]);
			snk.EndFigure(new SharpDX.Direct2D1.FigureEnd());
			snk.Close();
			
			target.FillGeometry(geo, geoBrush.ToDxBrush(target));

			geo.Dispose();
			snk.Dispose();
		}
	}
}
