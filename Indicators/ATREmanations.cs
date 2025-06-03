//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

public enum ATREmanations_Types {ATRmult,Ticks,Points};
// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Average True Range (ATR) is a measure of volatility. It was introduced by Welles Wilder
	/// in his book 'New Concepts in Technical Trading Systems' and has since been used as a component
	/// of many indicators and trading systems.
	/// </summary>
	public class ATREmanations : Indicator
	{
		List<double> Dists = new List<double>();
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionATR;
				Name						= "ATR Emanations";
				IsSuspendedWhileInactive	= false;
				Period						= 14;
				pType = ATREmanations_Types.ATRmult;
				pDistances = "0.7, 1.4, 3, 4, 5, 6";
				pOpacity = 50;
				pAboveMidline_Brush = Brushes.Magenta;
				pBelowMidline_Brush = Brushes.Lime;
				this.IsChartOnly = true;
				this.IsOverlay = true;

//				AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameATR);
			}
			else if (State == State.DataLoaded){
				var elem = pDistances.Split(new char[]{',',' ',';','|'}, StringSplitOptions.RemoveEmptyEntries);
				foreach(var e in elem){
					double v = 0;
					if(double.TryParse(e, out v))
						Dists.Add(v);
				}
			}
		}

		double avg = 0;
		List<double> ranges = new List<double>();
		int abar = 0;
		SortedDictionary<string,double[]> Markers = new SortedDictionary<string,double[]>();
		protected override void OnBarUpdate()
		{
			if(CurrentBar<1) return;

//			if(IsFirstTickOfBar){
//				ranges.Add(Range[1]());
//			}
		}
		SortedDictionary<string, System.Windows.Media.Brush> brushes = new SortedDictionary<string,System.Windows.Media.Brush>();
		SortedDictionary<string, SharpDX.Direct2D1.Brush> brushesDX = new SortedDictionary<string,SharpDX.Direct2D1.Brush>();
		SharpDX.Direct2D1.Brush WhiteBrushDX = null;
		SharpDX.Direct2D1.Brush AboveMidBrushDX = null;
		SharpDX.Direct2D1.Brush BelowMidBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(WhiteBrushDX!=null && !WhiteBrushDX.IsDisposed) WhiteBrushDX.Dispose(); WhiteBrushDX = null;
			if(RenderTarget!=null && ChartControl!=null)
				WhiteBrushDX = ChartControl.Properties.AxisPen.Brush.ToDxBrush(RenderTarget);
			if(AboveMidBrushDX!=null && !AboveMidBrushDX.IsDisposed) AboveMidBrushDX.Dispose(); AboveMidBrushDX = null;
			if(RenderTarget!=null){
				AboveMidBrushDX = pAboveMidline_Brush.ToDxBrush(RenderTarget);
				AboveMidBrushDX.Opacity = pOpacity/100f;
			}
			if(BelowMidBrushDX!=null && !BelowMidBrushDX.IsDisposed) BelowMidBrushDX.Dispose(); BelowMidBrushDX = null;
			if(RenderTarget!=null){
				BelowMidBrushDX = pBelowMidline_Brush.ToDxBrush(RenderTarget);
				BelowMidBrushDX.Opacity = pOpacity/100f;
			}

			var keys = brushesDX.Keys.ToArray();
			foreach(var k in keys){
				if(!Markers.ContainsKey(k)){
					brushesDX[k].Dispose();
					brushesDX[k]=null;
					brushesDX.Remove(k);
				}
			}
			foreach(var kvp in brushes){
				if(brushesDX.ContainsKey(kvp.Key) && brushesDX[kvp.Key]!=null && !brushesDX[kvp.Key].IsDisposed) brushesDX[kvp.Key].Dispose(); brushesDX[kvp.Key] = null;
				if(RenderTarget!=null){
					brushesDX[kvp.Key] = kvp.Value.ToDxBrush(RenderTarget);
					brushesDX[kvp.Key].Opacity = pOpacity/100f;
				}
			}
		}
		protected override void OnRender(Gui.Chart.ChartControl chartControl, Gui.Chart.ChartScale chartScale)
		{
			if (Bars == null || chartControl == null)
				return;
			var objects = DrawObjects.Where(o=> o.ToString().Contains("Diamond")).ToArray();
			if(objects!=null && objects.Length>0){
				SortedDictionary<string,double[]> Markers2 = new SortedDictionary<string,double[]>();
				bool NewMarker = false;
				brushes.Clear();
try{
				foreach (dynamic dob in objects) {
					Diamond o = (Diamond)dob;
					var t = o.Anchors.First().Time;
					var p = o.Anchors.First().Price;
					abar = BarsArray[0].GetBar(t);
					brushes[o.Tag] = o.AreaBrush.Clone();

					if(!Markers.ContainsKey(o.Tag)) NewMarker = true;
					else if(Markers[o.Tag].Length<6 || Markers[o.Tag][0] != p) NewMarker = true;
					Markers2[o.Tag] = new double[Dists.Count*2+2];
					Markers2[o.Tag][0] = p;
					Markers2[o.Tag][1] = abar; //center price, abar, 3 top emanations, 3 bot emanations
				}
}catch(Exception ee){Print(ee.ToString());}
				if(NewMarker){
					Markers.Clear();
					foreach(var kvp in Markers2){
						abar = Convert.ToInt32(kvp.Value[1]);
						avg = High.GetValueAt(abar) - Low.GetValueAt(abar);
						for(var i = abar-1; i>Math.Max(1,abar-Period); i--){
							avg = avg + High.GetValueAt(i) - Low.GetValueAt(i);
						}
						avg = avg/Period;
						Markers[kvp.Key] = kvp.Value;
						int j = 0;
						for(int i = 0; i<Dists.Count; i++){
							if(pType == ATREmanations_Types.ATRmult){
								Markers[kvp.Key][j+2] = kvp.Value[0] + avg*Dists[i];
								Markers[kvp.Key][j+3] = kvp.Value[0] - avg*Dists[i];
							}
							else if(pType == ATREmanations_Types.Points){
								Markers[kvp.Key][j+2] = kvp.Value[0] + Dists[i];
								Markers[kvp.Key][j+3] = kvp.Value[0] - Dists[i];
							}
							else if(pType == ATREmanations_Types.Ticks){
								Markers[kvp.Key][j+2] = kvp.Value[0] + Dists[i]*TickSize;
								Markers[kvp.Key][j+3] = kvp.Value[0] - Dists[i]*TickSize;
							}
							j++;j++;
						}
					}
				}
			}else Markers.Clear();

			SharpDX.Vector2 v1 = new SharpDX.Vector2(0, 0);
			SharpDX.Vector2 v2 = new SharpDX.Vector2(Convert.ToSingle(ChartPanel.W), 0);
			foreach(var kvp in Markers){
				v1.X = chartControl.GetXByBarIndex(ChartBars, Convert.ToInt32(kvp.Value[1]));
				v1.Y = chartScale.GetYByValue(kvp.Value[0]);
				v2.Y = v1.Y;

				if(brushesDX.ContainsKey(kvp.Key) && brushesDX[kvp.Key]!=null)	RenderTarget.DrawLine(v1, v2, brushesDX[kvp.Key]);
				else if(WhiteBrushDX!=null)										RenderTarget.DrawLine(v1, v2, WhiteBrushDX);

				for(int i = 2; i<kvp.Value.Length; i++){
					v1.Y = chartScale.GetYByValue(kvp.Value[i]);
					v2.Y = v1.Y;

					if(kvp.Value[i] > kvp.Value[0])	RenderTarget.DrawLine(v1, v2, AboveMidBrushDX);
					else							RenderTarget.DrawLine(v1, v2, BelowMidBrushDX);

//					if(brushesDX.ContainsKey(kvp.Key) && brushesDX[kvp.Key]!=null)	RenderTarget.DrawLine(v1, v2, brushesDX[kvp.Key]);
//					else if(WhiteBrushDX!=null)										RenderTarget.DrawLine(v1, v2, WhiteBrushDX);
				}
			}
		}
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 10)]
		public int Period
		{ get; set; }

		[Display(Name = "Type", GroupName = "NinjaScriptParameters", Order = 20, ResourceType = typeof(Custom.Resource))]
		public ATREmanations_Types pType
		{get;set;}

		[Display(Name = "Distances", GroupName = "NinjaScriptParameters", Order = 30, ResourceType = typeof(Custom.Resource))]
		public string pDistances
		{get;set;}

		[XmlIgnore]
		[Display(Order = 10, Name = "Lines above midline", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pAboveMidline_Brush { get; set; }
			[Browsable(false)]
			public string pAboveMidline_BrushSerialize{	get { return Serialize.BrushToString(pAboveMidline_Brush); } set { pAboveMidline_Brush = Serialize.StringToBrush(value); }}
		[XmlIgnore]
		[Display(Order = 20, Name = "Lines below midline", GroupName = "Custom Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pBelowMidline_Brush { get; set; }
			[Browsable(false)]
			public string pBelowMidline_BrushSerialize{	get { return Serialize.BrushToString(pBelowMidline_Brush); } set { pBelowMidline_Brush = Serialize.StringToBrush(value); }}

		[Display(Name = "Opacity", GroupName = "NinjaScriptParameters", Order = 30, ResourceType = typeof(Custom.Resource))]
		public int pOpacity
		{get;set;}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATREmanations[] cacheATREmanations;
		public ATREmanations ATREmanations(int period)
		{
			return ATREmanations(Input, period);
		}

		public ATREmanations ATREmanations(ISeries<double> input, int period)
		{
			if (cacheATREmanations != null)
				for (int idx = 0; idx < cacheATREmanations.Length; idx++)
					if (cacheATREmanations[idx] != null && cacheATREmanations[idx].Period == period && cacheATREmanations[idx].EqualsInput(input))
						return cacheATREmanations[idx];
			return CacheIndicator<ATREmanations>(new ATREmanations(){ Period = period }, input, ref cacheATREmanations);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATREmanations ATREmanations(int period)
		{
			return indicator.ATREmanations(Input, period);
		}

		public Indicators.ATREmanations ATREmanations(ISeries<double> input , int period)
		{
			return indicator.ATREmanations(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATREmanations ATREmanations(int period)
		{
			return indicator.ATREmanations(Input, period);
		}

		public Indicators.ATREmanations ATREmanations(ISeries<double> input , int period)
		{
			return indicator.ATREmanations(input, period);
		}
	}
}

#endregion
