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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The OrderBlocks (Simple Moving Average) is an indicator that shows the average value of a security's price over a period of time.
	/// </summary>
	public class OrderBlocks2 : Indicator
	{
		private double priorSum;
		private double sum;
		private class R{
			public int BrokenABar = 0;
			public double Top;
			public double Bot;
			public bool HighExceeded = false;
			public bool LowExceeded = false;
			public char Type = ' ';//'T' for top or 'B' for bottom
			public char Strength = ' ';//'S' for strong (buy zone in an oversold location), or 'W' for weak (buy zone in an overbought location)
			public R(double top, double bot, char type, char strength){Top = top; Bot = bot; Type=type; Strength = strength;}
		}
		SortedDictionary<int, R> Rects = new SortedDictionary<int, R>();
		private class D{
			public double Price;
			public char Type;
			public D(double price, char type){Price = price; Type=type;}
		}
		SortedDictionary<int, D> Dots = new SortedDictionary<int, D>();//when a price bar crosses into a zone, print a dot

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionSMA;
				Name						= "OrderBlocks2";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= false;
				pPeriod	= 6;
				pSignalStrength = 4;
				pDollarsRisked = 150;
				pAlertsEnabled = true;
				pWAVSupplyZone = "<inst>_SellSetup.wav";
				pWAVDemandZone = "<inst>_BuySetup.wav";
				pStrongBuyZoneBrush   = Brushes.Blue;
				pStrongBuyZoneOpacity = 30;
				pWeakBuyZoneBrush     = Brushes.Cyan;
				pWeakBuyZoneOpacity   = 10;
				pStrongSellZoneBrush  = Brushes.Crimson;
				pStrongSellZoneOpacity = 30;
				pWeakSellZoneBrush     = Brushes.DeepPink;
				pWeakSellZoneOpacity   = 10;
				pConsolidationsOpacity = 10;

				AddPlot(new Stroke(Brushes.Goldenrod,2), PlotStyle.Line, "MA");
			}
			else if (State == State.Configure)
			{
				priorSum	= 0;
				sum			= 0;
			}else if (State == State.Realtime){
//				var instr = System.IO.File.ReadAllText(@"C:\Users\Ben\Sync\UserPass\aaainfile.html");
//				instr = instr.Replace("\r",string.Empty).Replace("\n",string.Empty);
//	            string pattern = "(?<!<span[^>]*)(class\\s*=\\s*\"[^\"]*\")";//"class\\s*=\\s*\"[^\"]*\"";
//    	        string output = System.Text.RegularExpressions.Regex.Replace(instr, pattern, "");
//				System.IO.File.WriteAllText(@"C:\Users\Ben\Sync\UserPass\outstr2.html",output);
			}
		}

		double OpenPrice = 0;
		int c = 0;
		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				if (CurrentBar == 0)
					Value[0] = Typical[0];
				else
				{
					double last = Value[1] * Math.Min(CurrentBar, pPeriod);

					if (CurrentBar >= pPeriod)
						Value[0] = (last + Typical[0] - Typical[pPeriod]) / Math.Min(CurrentBar, pPeriod);
					else
						Value[0] = ((last + Typical[0]) / (Math.Min(CurrentBar, pPeriod) + 1));
				}
			}
			else
			{
				if (IsFirstTickOfBar)
					priorSum = sum;

				sum = priorSum + Typical[0] - (CurrentBar >= pPeriod ? Typical[pPeriod] : 0);
				Value[0] = sum / (CurrentBar < pPeriod ? CurrentBar + 1 : pPeriod);
			}
			if(CurrentBar>5){
				c = 0;
				bool LocationFilter = Closes[0][0] < OpenPrice;
				bool NewZoneCreated = false;
				var NewDay = Times[0][0].Day != Times[0][1].Day;
				if(NewDay) OpenPrice = Closes[0][0];
				if(Value[0] < Closes[0][0]) c++;// && Closes[0][0] > Opens[0][0];
				if(Value[1] < Closes[0][1]) c++;// && Closes[0][1] > Opens[0][1];
				if(pSignalStrength <= 2){
					if(Value[2] > Closes[0][2]) c++;
				}
				else if(pSignalStrength == 3){
					if(Value[2] < Closes[0][2] && Closes[0][2] > Medians[0][2]) c++;
					if(Value[3] > Closes[0][3]) c++;
				}
				else if(pSignalStrength == 4){
					if(Value[2] < Closes[0][2] && Closes[0][2] > Medians[0][2]) c++;
					if(Value[3] < Closes[0][3] && Closes[0][3] > Medians[0][3]) c++;
					if(Value[4] > Closes[0][4]) c++;
				}
				if(c == pSignalStrength+1){
					//Draw.Rectangle(this, (CurrentBars[0]-3).ToString(), Times[0][3], Highs[0][3], Times[0][0].AddMinutes(20), Lows[0][3], true, "Default");
					if(pDollarsRisked>0){
						var pts = pDollarsRisked/Instrument.MasterInstrument.PointValue;
						var low = Lows[0][pSignalStrength] - pts/2;
						NewZoneCreated = AddIfNotOverlapping(CurrentBars[0], low+pts, low, 'B', LocationFilter ? 'S':'W');//Rects[CurrentBars[0]] = new R(low + pts, low, 'B');
					}else{
						NewZoneCreated = AddIfNotOverlapping(CurrentBars[0], Highs[0][pSignalStrength], Lows[0][pSignalStrength], 'B', LocationFilter ? 'S':'W');//Rects[CurrentBars[0]] = new R(Highs[0][4], Lows[0][4], 'B');
					}
					if(pAlertsEnabled && NewZoneCreated) Alert(CurrentBar.ToString(), Priority.High, "New demand zone", AddSoundFolder(pWAVDemandZone), 1, Brushes.Green, Brushes.White);
				}
				NewZoneCreated = false;
				c = 0;
				LocationFilter = Closes[0][0] > OpenPrice;
				if(Value[0] > Closes[0][0]) c++;// && Closes[0][0] < Opens[0][0];
				if(Value[1] > Closes[0][1]) c++;// && Closes[0][1] < Opens[0][1];
				if(pSignalStrength <= 2){
					if(Value[2] < Closes[0][2]) c++;
				}
				else if(pSignalStrength == 3){
					if(Value[2] > Closes[0][2] && Closes[0][2] < Medians[0][2]) c++;
					if(Value[3] < Closes[0][3]) c++;
				}
				else if(pSignalStrength == 4){
					if(Value[2] > Closes[0][2] && Closes[0][2] < Medians[0][2]) c++;
					if(Value[3] > Closes[0][3] && Closes[0][3] < Medians[0][3]) c++;
					if(Value[4] < Closes[0][4]) c++;
				}
				if(c == pSignalStrength+1){
//					Draw.Rectangle(this, (CurrentBars[0]-3).ToString(), Times[0][3], Highs[0][3], Times[0][0].AddMinutes(20), Lows[0][3], true, "Crimson");
					if(pDollarsRisked>0){
						var pts = pDollarsRisked/Instrument.MasterInstrument.PointValue;
						var high = Highs[0][pSignalStrength] + pts/2;
						NewZoneCreated = AddIfNotOverlapping(CurrentBars[0], high, high-pts, 'T', LocationFilter ? 'S':'W');// Rects[CurrentBars[0]] = new R(high, high - pts, 'T');
					}else{
						NewZoneCreated = AddIfNotOverlapping(CurrentBars[0], Highs[0][pSignalStrength], Lows[0][pSignalStrength], 'T', LocationFilter ? 'S':'W');//Rects[CurrentBars[0]] = new R(Highs[0][pSignalStrength], Lows[0][pSignalStrength], 'T');
					}
					if(pAlertsEnabled && NewZoneCreated) Alert(CurrentBar.ToString(), Priority.High, "New supply zone", AddSoundFolder(pWAVSupplyZone), 1, Brushes.Red, Brushes.White);
				}
				var r = Rects.Where(k=>k.Value.BrokenABar==0).ToList();
				foreach(KeyValuePair<int,R> kvp in r){
					if(kvp.Value.Type=='T' && Highs[0][0] > kvp.Value.Top || NewDay)
						kvp.Value.BrokenABar = CurrentBars[0];
					else if(kvp.Value.Type=='B' && Lows[0][0] < kvp.Value.Bot || NewDay)
						kvp.Value.BrokenABar = CurrentBars[0];
					else if(kvp.Value.Type=='C'){
						if(!kvp.Value.HighExceeded && Highs[0][0] > kvp.Value.Top) kvp.Value.HighExceeded = true;
						if(!kvp.Value.LowExceeded && Lows[0][0] < kvp.Value.Bot) kvp.Value.LowExceeded = true;
						if(kvp.Value.LowExceeded && Highs[0][0] > kvp.Value.Top || NewDay)
							kvp.Value.BrokenABar = CurrentBars[0];
						else if(kvp.Value.HighExceeded && Lows[0][0] < kvp.Value.Bot || NewDay)
							kvp.Value.BrokenABar = CurrentBars[0];
					}
				}
				#region -- Consolidation zones --
				if(IsFirstTickOfBar && pConsolidationsOpacity>0 && CurrentBars[0]>15){
					double max = MAX(Highs[0],10)[1];
					double min = MIN(Lows[0],10)[1];
					double mid = (max + min)/2;
	//				if(CurrentBars[0] > BarsArray[0].Count-100){
	//					Draw.Dot(this,"Max"+CurrentBars[0].ToString(),false,1,max,Brushes.White);
	//					Draw.Dot(this,"Min"+CurrentBars[0].ToString(),false,1,min,Brushes.White);
	//					Draw.Dot(this,"Mid"+CurrentBars[0].ToString(),false,1,mid,Brushes.Cyan);
	//				}
					List<double> Ra = new List<double>(10){Range()[1], Range()[2], Range()[3], Range()[4], Range()[5], Range()[6], Range()[7], Range()[8], Range()[9], Range()[10]};
					double atr = Ra.Average()*pConsolidationSensitivity/10.0;
					if(mid+atr > max && mid-atr < min){
						var lmab = Rects.Where(k=>k.Value.Type=='C' && k.Value.BrokenABar == 0).Select(k=>k.Key).ToList();
						int i = -1;
						if(lmab!=null && lmab.Count>0){ i = lmab.Max();}
						if(i == -1){// || Rects.Last().Value.BrokenABar < CurrentBars[0]-pOverlapBars){
//							if(ConsolidationAlertABar < CurrentBars[0]-2 && pWAVOnConsolidation!="none"){
//								ConsolidationAlertABar = CurrentBars[0];
//								Alert(CurrentBars[0].ToString(),Priority.Medium, "Consolidation box created", AddSoundFolder(pWAVOnConsolidation), 1, Brushes.White, Brushes.Black);
//							}
//Print("Added first rect  "+Times[0][0].ToString());
							Rects[CurrentBars[0]-10] = new R(max, min, 'C', ' ');//new RectInfo(CurrentBars[0]-1, max, CurrentBars[0]-10, min));//Draw.Rectangle(this,"Rect"+CurrentBars[0].ToString(), 1,max, 10, min, Brushes.Yellow);
						}else{
//Print("max: "+max+"  Rects[i].Top: "+Rects[i].Top+"    min: "+min+"  Bot: "+Rects[i].Bot);
							if(min > Rects[i].Top || max < Rects[i].Bot){
//								max = Math.Min(Rects[i].Top, max);
//								min = Math.Max(Rects[i].Bot, min);
								//Rects.RemoveAt(Rects.Count-1);
								Rects[i].BrokenABar = CurrentBars[0]-10;
//Print("Added new rect  "+Times[0][0].ToString()+"   max/min: "+max+" / "+min);
								Rects[CurrentBars[0]-10] = new R(max, min, 'C', ' ');//new RectInfo(CurrentBars[0]-1, max, CurrentBars[0]-10, min));//Draw.Rectangle(this,"Rect"+CurrentBars[0].ToString(), 1,max, 10, min, Brushes.Yellow);
	//							Rects[i].Top = max;//.Add(new RectInfo(CurrentBars[0]-1, max, lmab, min));
	//							Rects[i].Bot = min;
							}
						}
					}
				}
				#endregion
				#region -- Signal dots --
				r = Rects.Where(k=>k.Value.BrokenABar==0 && k.Value.Type!='C').ToList();
				foreach(KeyValuePair<int,R> kvp in r){
//bool z = Times[0][0].Hour==4 && Times[0][0].Minute < 18 && Times[0][0].Minute>14 && Times[0][0].Day==17 && kvp.Value.Top==4131;
//if(z) Print("Zone starting at: "+Bars.GetTime(kvp.Key).ToString() + kvp.Value.Top+" / "+kvp.Value.Bot);
					if(     Lows[0][1] >=  kvp.Value.Top && Lows[0][0] <  kvp.Value.Top && Highs[0][0] >= kvp.Value.Top) Dots[CurrentBars[0]] = new D(Value[0], 'B');//kvp.Value.Bot);
					else if(Highs[0][1] <= kvp.Value.Bot && Highs[0][0] > kvp.Value.Bot && Lows[0][0] <=  kvp.Value.Bot) Dots[CurrentBars[0]] = new D(Value[0], 'T');//kvp.Value.Top);
				}
				#endregion
			}
		}
		private bool AddIfNotOverlapping(int cb, double high, double low, char type, char strength){
			var z = Rects.Where(k=>k.Value.BrokenABar == 0).Count(k=> high <= k.Value.Top && high >= k.Value.Bot || low <= k.Value.Top && low >= k.Value.Bot);
			if(z==0) {
				if(     type == 'B' && strength == 'S' && pStrongBuyZoneOpacity == 0)  return false;
				else if(type == 'T' && strength == 'S' && pStrongSellZoneOpacity == 0) return false;
				else if(type == 'B' && strength == 'W' && pWeakBuyZoneOpacity == 0)    return false;
				else if(type == 'T' && strength == 'W' && pWeakSellZoneOpacity == 0)   return false;
				Rects[cb] = new R(high, low, type, strength);
				return true;
			} else return false;
		}

		SharpDX.Direct2D1.Brush StrongSellBrushDX=null;
		SharpDX.Direct2D1.Brush StrongBuyBrushDX=null;
		SharpDX.Direct2D1.Brush WeakSellBrushDX=null;
		SharpDX.Direct2D1.Brush WeakBuyBrushDX=null;
		SharpDX.Direct2D1.Brush BuySignalDotBrushDX=null;
		SharpDX.Direct2D1.Brush SellSignalDotBrushDX=null;
		SharpDX.Direct2D1.Brush ConsolidationBrushDX = null;
		public override void OnRenderTargetChanged()
		{
			if(ConsolidationBrushDX!=null  && !ConsolidationBrushDX.IsDisposed)  ConsolidationBrushDX.Dispose(); ConsolidationBrushDX = null;
			if(RenderTarget!=null) {ConsolidationBrushDX = Brushes.Yellow.ToDxBrush(RenderTarget);  ConsolidationBrushDX.Opacity = pConsolidationsOpacity/100f;}

			if(StrongBuyBrushDX!=null  && !StrongBuyBrushDX.IsDisposed)  StrongBuyBrushDX.Dispose(); StrongBuyBrushDX = null;
			if(RenderTarget!=null) {StrongBuyBrushDX = pStrongBuyZoneBrush.ToDxBrush(RenderTarget);  StrongBuyBrushDX.Opacity = pStrongBuyZoneOpacity/100f;}

			if(StrongSellBrushDX!=null && !StrongSellBrushDX.IsDisposed)  StrongSellBrushDX.Dispose(); StrongSellBrushDX = null;
			if(RenderTarget!=null) {StrongSellBrushDX = pStrongSellZoneBrush.ToDxBrush(RenderTarget);  StrongSellBrushDX.Opacity = pStrongSellZoneOpacity/100f;}

			if(WeakBuyBrushDX!=null  && !WeakBuyBrushDX.IsDisposed)  WeakBuyBrushDX.Dispose();  WeakBuyBrushDX = null;
			if(RenderTarget!=null) {WeakBuyBrushDX = pWeakBuyZoneBrush.ToDxBrush(RenderTarget); WeakBuyBrushDX.Opacity = pWeakBuyZoneOpacity/100f;}

			if(WeakSellBrushDX!=null && !WeakSellBrushDX.IsDisposed)  WeakSellBrushDX.Dispose();  WeakSellBrushDX = null;
			if(RenderTarget!=null) {WeakSellBrushDX = pWeakSellZoneBrush.ToDxBrush(RenderTarget); WeakSellBrushDX.Opacity = pWeakSellZoneOpacity/100f;}

			if(BuySignalDotBrushDX!=null && !BuySignalDotBrushDX.IsDisposed)  BuySignalDotBrushDX.Dispose();  BuySignalDotBrushDX = null;
			if(RenderTarget!=null) {BuySignalDotBrushDX = Brushes.Lime.ToDxBrush(RenderTarget); BuySignalDotBrushDX.Opacity = 70f/100f;}
			if(SellSignalDotBrushDX!=null && !SellSignalDotBrushDX.IsDisposed)  SellSignalDotBrushDX.Dispose();  SellSignalDotBrushDX = null;
			if(RenderTarget!=null) {SellSignalDotBrushDX = Brushes.Magenta.ToDxBrush(RenderTarget); SellSignalDotBrushDX.Opacity = 70f/100f;}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) 
		{
			base.OnRender(chartControl, chartScale);
			if (!IsVisible) return;
			if (chartControl==null) return;
			//if (IsInHitTest) 
			{
				float x, y, height, width;
				var keys = Rects.Where(k=>k.Value.Top > chartScale.MinValue && k.Value.Bot < chartScale.MaxValue && k.Key < ChartBars.ToIndex && (k.Value.BrokenABar==0 || k.Value.BrokenABar > ChartBars.FromIndex)).Select(p=>p.Key).ToList();
				for(int i = 0; i<keys.Count; i++){
					y = chartScale.GetYByValue(Rects[keys[i]].Top);
					height = chartScale.GetYByValue(Rects[keys[i]].Bot) - y;

					x = chartControl.GetXByBarIndex(ChartBars, keys[i]);
					if(Rects[keys[i]].BrokenABar == 0) {
						x = Math.Max(0f, x);
						width = ChartPanel.W;//chartControl.GetXByBarIndex(ChartBars, BarsArray[0].Count);
					}
					else{
						width = chartControl.GetXByBarIndex(ChartBars, Rects[keys[i]].BrokenABar) - x;
					}

					if(Rects[keys[i]].Type == 'T') RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, width, height), Rects[keys[i]].Strength=='S'? StrongSellBrushDX : WeakSellBrushDX);
					else if(Rects[keys[i]].Type == 'B') RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, width, height), Rects[keys[i]].Strength=='S' ? StrongBuyBrushDX : WeakBuyBrushDX);
					else if(Rects[keys[i]].Type == 'C') RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, width, height), ConsolidationBrushDX);
				}
				var keys2 = Dots.Where(k=>k.Value.Price > chartScale.MinValue && k.Value.Price < chartScale.MaxValue && k.Key < ChartBars.ToIndex && k.Key > ChartBars.FromIndex).Select(p=>p.Key).ToList();
				var w = Plots[0].Width * 3f;
				for(int i = 0; i<keys2.Count; i++){
					x = chartControl.GetXByBarIndex(ChartBars, keys2[i]);
					y = chartScale.GetYByValue(Dots[keys2[i]].Price);
					RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y), w, w), Dots[keys2[i]].Type=='B' ? BuySignalDotBrushDX : SellSignalDotBrushDX);
				}
			}
		}
		#region Properties
		[Range(1, int.MaxValue)]
		[Display(Order = 10, Name = "MA Period", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public int pPeriod
		{ get; set; }

		[Display(Order = 20, Name = "Signal Strength", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public int pSignalStrength
		{get;set;}

		[Display(Order = 30, Name = "Dollars risked", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public double pDollarsRisked
		{ get; set; }

		[Range(0, 100)]
		[Display(Order = 40, Name = "Consolidations opacity", GroupName = "NinjaScriptParameters", ResourceType = typeof(Custom.Resource))]
		public int pConsolidationsOpacity
		{get;set;}

		private int pConsolidationSensitivity = 11;
		[Display(Order=50,    Name="Consolidation Sensitivity", GroupName="NinjaScriptParameters", Description="Sensitivity of consolidation rectangles.  Low numbers mean more rectangles.  Set to '0' to turn off all consolidation rectangles")]
        public int ConsolidationSensitivity
        {
            get { return pConsolidationSensitivity; }
            set { pConsolidationSensitivity = Math.Max(0, value); }
        }
		private int pOverlapBars = 5;
		[Display(Order=60,    Name="Consolidation Overlap", GroupName="NinjaScriptParameters", Description="")]
		public int OverlapBars
        {
            get { return pOverlapBars; }
            set { pOverlapBars = Math.Max(0, value); }
        }
		#region -- Zone Visuals --
		[XmlIgnore]
		[Display(Order = 10, Name = "Strong Buy Zone", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pStrongBuyZoneBrush
		{ get; set; }
				[Browsable(false)]
				public string pStrongBuyZoneSerializable { get { return Serialize.BrushToString(pStrongBuyZoneBrush); } set { pStrongBuyZoneBrush = Serialize.StringToBrush(value); }        }

		[Range(0, 100)]
		[Display(Order = 20, Name = "Opacity", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public int pStrongBuyZoneOpacity {get;set;}
		[XmlIgnore]
		[Display(Order = 30, Name = "Weak Buy Zone", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pWeakBuyZoneBrush
		{ get; set; }
				[Browsable(false)]
				public string pWeakBuyZoneSerializable { get { return Serialize.BrushToString(pWeakBuyZoneBrush); } set { pWeakBuyZoneBrush = Serialize.StringToBrush(value); }        }

		[Range(0, 100)]
		[Display(Order = 40, Name = "Opacity", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public int pWeakBuyZoneOpacity {get;set;}

		[XmlIgnore]
		[Display(Order = 50, Name = "Strong Sell Zone", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pStrongSellZoneBrush
		{ get; set; }
				[Browsable(false)]
				public string pStrongSellZoneSerializable { get { return Serialize.BrushToString(pStrongSellZoneBrush); } set { pStrongSellZoneBrush = Serialize.StringToBrush(value); }        }

		[Range(0, 100)]
		[Display(Order = 60, Name = "Opacity", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public int pStrongSellZoneOpacity {get;set;}
		[XmlIgnore]
		[Display(Order = 70, Name = "Weak Sell Zone", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public Brush pWeakSellZoneBrush
		{ get; set; }
				[Browsable(false)]
				public string pWeakSellZoneSerializable { get { return Serialize.BrushToString(pWeakSellZoneBrush); } set { pWeakSellZoneBrush = Serialize.StringToBrush(value); }        }

		[Range(0, 100)]
		[Display(Order = 80, Name = "Opacity", GroupName = "Zone Visuals", ResourceType = typeof(Custom.Resource))]
		public int pWeakSellZoneOpacity {get;set;}
		#endregion

		#region -- Alerts --
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav.Replace("<inst>",Instrument.MasterInstrument.Name));
		}
		internal class LoadSoundFileList : StringConverter
		{
			#region LoadSoundFileList
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				//true means show a combobox
				return true;
			}

			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
				//true will limit to list. false will show the list, 
				//but allow free-form entry
				return false;
			}

			public override System.ComponentModel.TypeConverter.StandardValuesCollection
				GetStandardValues(ITypeDescriptorContext context)
			{
				string dir = NinjaTrader.Core.Globals.InstallDir;
//				string dir = WAVDirectory;
//				if(dir.Trim().Length==0) dir = NinjaTrader.Core.Globals.InstallDir;
//				if(dir.ToLower().Contains("<default>")) dir = NinjaTrader.Core.Globals.InstallDir;
				string folder = System.IO.Path.Combine(dir, "sounds");
				string search = "*.wav";
				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
				list.Add("<inst>_BuyBreakout.wav");
				list.Add("<inst>_SellBreakout.wav");
				list.Add("<inst>_Consolidation.wav");
				if(filCustom!=null){
					foreach (System.IO.FileInfo fi in filCustom)
					{
						if(!list.Contains(fi.Name)){
							list.Add(fi.Name);
						}
					}
				}
				return new StandardValuesCollection(list.ToArray());
			}
			#endregion
        }

		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Alerts enabled", GroupName = "Alerts")]
		public bool pAlertsEnabled {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Wav on Demand zone", GroupName = "Alerts")]
		public string pWAVDemandZone {get;set;}
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, ResourceType = typeof(Custom.Resource), Name = "Wav on Supply zone", GroupName = "Alerts")]
		public string pWAVSupplyZone {get;set;}
		#endregion

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OrderBlocks2[] cacheOrderBlocks2;
		public OrderBlocks2 OrderBlocks2()
		{
			return OrderBlocks2(Input);
		}

		public OrderBlocks2 OrderBlocks2(ISeries<double> input)
		{
			if (cacheOrderBlocks2 != null)
				for (int idx = 0; idx < cacheOrderBlocks2.Length; idx++)
					if (cacheOrderBlocks2[idx] != null &&  cacheOrderBlocks2[idx].EqualsInput(input))
						return cacheOrderBlocks2[idx];
			return CacheIndicator<OrderBlocks2>(new OrderBlocks2(), input, ref cacheOrderBlocks2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OrderBlocks2 OrderBlocks2()
		{
			return indicator.OrderBlocks2(Input);
		}

		public Indicators.OrderBlocks2 OrderBlocks2(ISeries<double> input )
		{
			return indicator.OrderBlocks2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OrderBlocks2 OrderBlocks2()
		{
			return indicator.OrderBlocks2(Input);
		}

		public Indicators.OrderBlocks2 OrderBlocks2(ISeries<double> input )
		{
			return indicator.OrderBlocks2(input);
		}
	}
}

#endregion
