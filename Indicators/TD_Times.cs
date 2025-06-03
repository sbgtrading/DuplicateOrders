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

using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class TD_Times : Indicator
	{
		private Collection<plLine> lineList = new Collection<plLine>();
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "TD_Times";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				ArePlotsConfigurable 						= false;
								
				for (int x=0;x<1000;x++)
					AddPlot(null, "Plot"+x.ToString());
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute,1);
				
				for (int x=0;x<lineList.Count;x++)
				{
					plLine line = lineList[x];
					line.plotIndex = x;
					
					Plots[x].PlotStyle = line.LineStyle;
					Plots[x].Brush = line.LineColor;
					Plots[x].Name = line.ToString();
					Plots[x].DashStyleHelper = line.LineDashStyle;
					Plots[x].Width = line.LineWidth;
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar<1)
				return;
			
			if (BarsInProgress==0)
			{
	            foreach (plLine line in lineList)
				{
					if (line.linePrice>0)
						Values[line.plotIndex][0] = line.linePrice;
				}
			}
			else
			{
				foreach (plLine line in lineList)
				{
					bool isTime = Time[0].TimeOfDay.CompareTo(line.LineTime)>=0 && Time[1].TimeOfDay.CompareTo(line.LineTime)<0;

					if (isTime)
						line.linePrice = GetPrice(line.LinePrice);
					if (line.linePrice>0)
						Values[line.plotIndex][0] = line.linePrice;
				}
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			int firstBar = ChartBars.FromIndex;
			int lastBar = Math.Min(ChartBars.ToIndex,CurrentBars[0]);
			
			for (int idx=firstBar;idx<=lastBar;idx++)
			{
				foreach (plLine line in lineList)
				{
					if (idx==CurrentBars[0] || (Values[line.plotIndex].IsValidDataPointAt(idx) && Values[line.plotIndex].IsValidDataPointAt(idx+1)))
					{
						double current = Values[line.plotIndex].GetValueAt(idx);
						double next = idx==CurrentBars[0] ? 0 : Values[line.plotIndex].GetValueAt(idx+1);	
						
						if (idx==lastBar)
							Draw.TextFixed(this,"text",idx.ToString(),TextPosition.BottomRight);
						
						if (current!=next)
						{
							using (SharpDX.DirectWrite.TextFormat textFormat1 = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat())
							{															
								string str = string.Format("{0} {1}",line.LineTime,line.LinePrice);
								using (SharpDX.DirectWrite.TextLayout textLayout1 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, str, textFormat1, ChartPanel.W, textFormat1.FontSize))										
								{
									textLayout1.MaxHeight = textLayout1.Metrics.Height;
									textLayout1.MaxWidth = textLayout1.Metrics.Width;
									
									int x = chartControl.GetXByBarIndex(ChartBars,idx);
									int y = chartScale.GetYByValue(current);
									
									y -= (int)Math.Round(textLayout1.Metrics.Height/2);
									
									SharpDX.Vector2 point = new SharpDX.Vector2(x, y);
									using (SharpDX.Direct2D1.Brush brushDX = Plots[line.plotIndex].Brush.ToDxBrush(RenderTarget))
									{
										RenderTarget.DrawTextLayout(point, textLayout1, brushDX);		
									
										SharpDX.RectangleF rect = new SharpDX.RectangleF(x,y,textLayout1.Metrics.Width+2,textLayout1.Metrics.Height);
										RenderTarget.DrawRectangle(rect, brushDX);
									}									
								}
							}
						}
					}
				}
			}				
		}
		
		private double GetPrice(PriceType priceType)
		{
			switch(priceType)
			{
				case PriceType.Open:
					return Open[0];
					break;
					
				case PriceType.High:
					return High[0];
					break;
					
				case PriceType.Low:
					return Low[0];
					break;
					
				case PriceType.Close:
					return Close[0];
					break;
					
				case PriceType.Median:
					return Median[0];
					break;
					
				case PriceType.Typical:
					return Typical[0];
					break;
			}
			return Close[0];
		}
		
		public override void CopyTo(NinjaScript ninjaScript)
		{
			base.CopyTo(ninjaScript);

			Type			newInstType					= ninjaScript.GetType();
			PropertyInfo	myListValuesPropertyInfo	= newInstType.GetProperty("LineList");
			
			if (myListValuesPropertyInfo == null)
				return;

			IList newInstMyListValues = myListValuesPropertyInfo.GetValue(ninjaScript) as IList;
			
			if (newInstMyListValues == null)
				return;

			// Since new instance could be past set defaults, clear any existing
			newInstMyListValues.Clear();
			
			foreach (plLine oldPercentWrapper in LineList)
			{
				try
				{
					object newInstance = oldPercentWrapper.AssemblyClone(Core.Globals.AssemblyRegistry.GetType(typeof(plLine).FullName));
					if (newInstance == null)
						continue;
					
					newInstMyListValues.Add(newInstance);
				}
				catch { }
			}
		}

		#region Properties
		[XmlIgnore]
        [Display(Name = "Line List", GroupName = "Parameters", Order = 1, Prompt = "1 value|{0} values|Add value...|Edit value...|Edit values...")]
        [PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")] // Allows a pop-up to be used to add values to the collection, similar to Price Levels in Drawing Tools
		[SkipOnCopyTo(true)]
        public Collection<plLine> LineList
        {
			get 
			{
				return lineList;
			}
			set
			{
				lineList = new Collection<plLine>(value.ToList());
			}
		}

        [Browsable(false)]
        public Collection<plLine> LineListS
        {
			get
			{
				return LineList;
			}
			set
			{
				LineList = value;
			}
        }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TestPlot
		{
			get { return Values[0]; }
		}
		#endregion
	}
}

#region Classes and enums
				
[TypeConverter(typeof(ExpandableObjectConverter))]
public class plLine : NotifyPropertyChangedBase, ICloneable, IComparable<plLine>
{
	#region Variables
		
	private TimeSpan lineTime;	
	private PriceType priceType;
	private Brush color;
	private PlotStyle style;
	private int width;
	private DashStyleHelper dashStyle;
	
	public int plotIndex = -1;
	public double linePrice = 0;
	
	#endregion
	
	#region Constructors
	
	public plLine()
	{
		lineTime = new TimeSpan(0,0,0);		
		priceType = PriceType.Close;
		
		color = Brushes.Blue;	
		style = PlotStyle.Line;
		width = 1;
		dashStyle = DashStyleHelper.Solid;
	}
	
	#endregion
	
	#region Methods
	
	public int CompareTo(plLine other)
	{
		return lineTime.CompareTo(other.LineTime);
	}
	
	public override string ToString()
	{
		return lineTime.ToString() + " " + priceType.ToString();
	}
	
	public object Clone()
	{
		plLine p  = new plLine();
		p.LineColor = LineColor;
		p.LineTime = LineTime;
		p.LinePrice = LinePrice;
		p.LineStyle = LineStyle;
		p.LineWidth = LineWidth;
		p.LineColor = LineColor;
		p.LineDashStyle = LineDashStyle;
		return p;
	}
	
	public object AssemblyClone(Type t)
	{
		Assembly a 				= t.Assembly;
		object line 			= a.CreateInstance(t.FullName);
		
		foreach (PropertyInfo p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
		{
			if (p.CanWrite)
				p.SetValue(line, this.GetType().GetProperty(p.Name).GetValue(this), null);
		}
		
		return line;
	}
	
	#endregion
	
	#region Properties
	
	[Category("Properties")]
	[DisplayName("01. Time")]	
	[Display(Order=1)]
	[XmlIgnore()]
	public TimeSpan LineTime
	{
		get { return lineTime; }
		set { lineTime = value; }
	}
	
	[Browsable(false)]
	public string LineTimeS
	{
		get { return lineTime.ToString(); }
		set { lineTime = TimeSpan.Parse(value); }
	}
	
	[Category("Properties")]
	[DisplayName("02. Price")]
	[Display(Order=2)]
	public PriceType LinePrice
	{
		get { return priceType; }
		set { priceType = value; }
	}		
	
	[Category("Properties")]
	[DisplayName("03. Style")]
	[Display(Order=3)]
	public PlotStyle LineStyle
	{
		get { return style; }
		set { style = value; }
	}
	
	[Category("Properties")]
	[DisplayName("04. Width")]
	[Display(Order=4)]
	public int LineWidth
	{
		get { return width; }
		set { width = value; }
	}
	
	[XmlIgnore()]
	[Description("")]
	[Category("Properties")]
	[DisplayName("05. Color")]
	[Display(Order=5)]
	public Brush LineColor
	{
		get { return color; }
		set { color = value; }
	}
	
	[Browsable(false)]
	public string LineColorS
	{
		get { return Serialize.BrushToString(color); }
		set { color = Serialize.StringToBrush(value); }
	}
	
	[Category("Properties")]
	[DisplayName("06. Dash Style")]
	[Display(Order=6)]
	public DashStyleHelper LineDashStyle
	{
		get { return dashStyle; }
		set { dashStyle = value; }
	}
	
	#endregion
}

#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TD_Times[] cacheTD_Times;
		public TD_Times TD_Times()
		{
			return TD_Times(Input);
		}

		public TD_Times TD_Times(ISeries<double> input)
		{
			if (cacheTD_Times != null)
				for (int idx = 0; idx < cacheTD_Times.Length; idx++)
					if (cacheTD_Times[idx] != null &&  cacheTD_Times[idx].EqualsInput(input))
						return cacheTD_Times[idx];
			return CacheIndicator<TD_Times>(new TD_Times(), input, ref cacheTD_Times);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TD_Times TD_Times()
		{
			return indicator.TD_Times(Input);
		}

		public Indicators.TD_Times TD_Times(ISeries<double> input )
		{
			return indicator.TD_Times(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TD_Times TD_Times()
		{
			return indicator.TD_Times(Input);
		}

		public Indicators.TD_Times TD_Times(ISeries<double> input )
		{
			return indicator.TD_Times(input);
		}
	}
}

#endregion
