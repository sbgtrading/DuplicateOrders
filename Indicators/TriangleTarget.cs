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
	public class TriangleTarget : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Triangle Target";
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
				IsSuspendedWhileInactive					= true;
				pTargMult		= 2;
				pWavLabelFontSize = 12;
				pAudibleAlertsEnabled = false;
				pShowLevelAsLines = true;
				pWAVOnBuyLevelHit = "Alert2.wav";
				pWAVOnSellLevelHit = "Alert2.wav";
				pBuyRayStroke = new Stroke(Brushes.Cyan, DashStyleHelper.Dash, 2f);
				pSellRayStroke = new Stroke(Brushes.DeepPink, DashStyleHelper.Dash, 2f);
				pType = TriangleTarget_Type.Auto;
//				pBrushBuyTriangle = Brushes.Transparent;
//				pBrushSellTriangle = Brushes.Transparent;
				AddPlot(Brushes.Orange, "T1");
			}
			else if (State == State.DataLoaded)
			{
				AlertsMgr = new AlertManager(this);
			}
			else if (State == State.Realtime)
			{
				ForceRefresh();
			}
		}

		string lastmsg = "";
		private void Printit(string s){
			if(s.CompareTo(lastmsg)!=0) {Print(s); lastmsg = s;}
		}
		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
		string tag = "";
		IDrawingTool o = null;
		int line=0;
		private void printDebug(string s){
			return;
			Print(s);
		}
		class AlertManager
		{
			public AlertManager(NinjaTrader.NinjaScript.IndicatorBase parent){Parent = parent;}
			public NinjaTrader.NinjaScript.IndicatorBase Parent;
			public class AlertInfo{
				public string Wav = "";
				public double Price = 0;
				public int AlertABar = 0;
				public AlertInfo(string wav, double price, int alertabar){Wav=wav; Price=price; AlertABar=alertabar;}
			}
			public SortedDictionary<string, AlertInfo> AlertLevels = new SortedDictionary<string, AlertInfo>();
			public void DeleteOldLevels(int cbar, int MaxAgeInBars){
				var keys_to_delete = new List<string>();
				foreach(var al in AlertLevels){
					if(al.Value.AlertABar < cbar-MaxAgeInBars) keys_to_delete.Add(al.Key);//turns-off this level
				}
				foreach(var al in keys_to_delete) AlertLevels.Remove(al);
			}
			private string StripOutIllegalCharacters(string name, string ReplacementString){
				#region strip
				char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
				string invalids = string.Empty;
				foreach(char ch in invalidPathChars){
					invalids += ch.ToString();
				}
	//			Print("Invalid chars: '"+invalids+"'");
				string result = string.Empty;
				for(int c=0; c<name.Length; c++) {
					if(!invalids.Contains(name[c].ToString())) result += name[c];
					else result += ReplacementString;
				}
				return result;
				#endregion
			}
			public void DetermineIfAlertLevelHit(double AskValOnPriorTick, double BidValOnPriorTick, double SpreadVal){
				var kvps = AlertLevels.Where(k=>k.Value.AlertABar<Parent.CurrentBars[0] && 
								SpreadVal > k.Value.Price &&
								BidValOnPriorTick < k.Value.Price).ToList();
				foreach(var kvp in kvps){
					double p = kvp.Value.Price;
Parent.Print("Alert level at "+p);
					var wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(kvp.Value.Wav.Replace("<inst1>",Parent.Instruments[0].MasterInstrument.Name).Replace("<inst2>",Parent.Instruments[1].MasterInstrument.Name)," "));
					Parent.Alert(DateTime.Now.Ticks.ToString(), Priority.High, "triangle Alert hit at "+Parent.Instrument.MasterInstrument.FormatPrice(p), wav, 0, Brushes.DimGray, Brushes.Black);
					kvp.Value.AlertABar = Parent.CurrentBars[0];
				}
			}
			public void AddLevel(string tag, double price, string wav, int cbar){
				if(AlertLevels.ContainsKey(tag)) AlertLevels[tag].Price = price;
				else AlertLevels[tag] = new AlertInfo(wav, price, cbar);//populate the AlertLevels dictionary
			}
		}
		private AlertManager AlertsMgr;
//=========================================================================================================
		private string KeepTheseChars(char[] chararray, string keepers){
			string result = string.Empty;
			for(int i = 0; i<chararray.Length; i++)
				if(keepers.Contains(chararray[i])) result = string.Format("{0}{1}", result,chararray[i]);
			return result;
		}
//=========================================================================================================
		SimpleFont font = null;
		private SharpDX.Direct2D1.Brush txtBrushDX, BlackBrushDX, DimGrayBrushDX, LimeBrushDX, CyanBrushDX, BlueBrushDX, YellowBrushDX, GreenBrushDX, MaroonBrushDX;

//=========================================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
//			if(!ValidLicense || TerminalError){
//				return;
//			}
			if(font == null) font = new SimpleFont("Arial", (float)pWavLabelFontSize);

			var objects = DrawObjects.Where(xo=>xo.ToString().Contains(".Triangle") || xo.ToString().Contains(".Ray")).ToList();
			o = null;
			if(objects!=null && objects.Count>0){
				var txtFormat = font.ToDirectWriteTextFormat();
				SharpDX.RectangleF labelRect = new SharpDX.RectangleF(0,0,0,0);;
				SharpDX.DirectWrite.TextLayout txtLayout = null;
				AlertsMgr.DeleteOldLevels(CurrentBars[0], 1);
				var nodes = new SortedDictionary<double,Point>();

				//NinjaTrader.NinjaScript.DrawingTools.Line L = null;
//printDebug("Objects.count: "+objects.Count);
//				foreach (dynamic dob in objects) {
				float diff = 0f;
				for(int idx = 0; idx<objects.Count; idx++){
//printDebug(objects[idx].Tag+" found");
//try{
					var AutoType = ' ';
					if (objects[idx].ToString().EndsWith(".Triangle")) {
						#region -- Triangle signals --
						nodes.Clear();
						o = (IDrawingTool)objects[idx];
						tag = o.Tag.ToLower();
try{
line=177;
						var li = o.Anchors.ToArray();
						double maxy = double.MinValue;
						double miny = double.MaxValue;
						double maxx = double.MinValue;
						double minx = double.MaxValue;
						double minxSlot = double.MaxValue;
						double maxxSlot = double.MinValue;
						foreach(var anch in li) {
							nodes[anch.SlotIndex] = anch.GetPoint(chartControl, ChartPanel, chartScale);
							maxy = Math.Max(maxy, nodes[anch.SlotIndex].Y);
							miny = Math.Min(miny, nodes[anch.SlotIndex].Y);
							if(nodes[anch.SlotIndex].X > maxx){
								maxx = nodes[anch.SlotIndex].X;
								maxxSlot = anch.SlotIndex;
							}
							if(nodes[anch.SlotIndex].X < minx){
								minx = nodes[anch.SlotIndex].X;
								minxSlot = anch.SlotIndex;
							}
						}
						if(pType == TriangleTarget_Type.Auto){
							if(nodes[maxxSlot].Y > miny && nodes[maxxSlot].Y < maxy) AutoType='T';
							if(nodes[minxSlot].Y > miny && nodes[minxSlot].Y < maxy) AutoType='R';
						}

						if(nodes.Count<2) printDebug("TriangleTarget on "+Instrument.MasterInstrument.Name+" nodes was only : "+nodes.Count);
						else{
							var keys = nodes.Keys.ToList();
							float x = Convert.ToSingle(nodes[keys[1]].X);
							float y = Convert.ToSingle(nodes[keys[1]].Y);
							float mult = pTargMult;
							string ttg = tag.ToUpper();
							var dir = nodes.Last().Value.Y > nodes.First().Value.Y ? 'U' : 'D';

							if(ttg.Contains("RR") || (ttg.Contains(".WAV") || this.pAudibleAlertsEnabled)) {
								var elems = ttg.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
								foreach(var el in elems){
									if(el.Contains("RR"))
										if(!float.TryParse(this.KeepTheseChars(el.ToCharArray(), "-.0123456789"), out mult)) mult=2;
									if(el.Contains(".WAV")){//pWAVOnBuyLevelHit
										var wav = tag.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(k=> k.Contains(".wav"));
										if(wav !=null){
											double pr = chartScale.GetValueByY(Convert.ToSingle(nodes[keys[1]].Y));
											AlertsMgr.AddLevel(tag, pr, wav, CurrentBars[0]);
											txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, wav, txtFormat, (float)(ChartPanel.X + ChartPanel.W), pWavLabelFontSize);
											labelRect = new SharpDX.RectangleF(10f, Convert.ToSingle(nodes[keys[1]].Y)-txtLayout.Metrics.Height-5f, txtLayout.Metrics.Width+10f, Convert.ToSingle(font.Size)+10f);
											RenderTarget.FillRectangle(labelRect, DimGrayBrushDX);
											RenderTarget.DrawText(wav, txtFormat, labelRect, BlackBrushDX);
											if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
										}
									} else {
										string wav = null;
										if(pAudibleAlertsEnabled && dir == 'U' && pWAVOnBuyLevelHit!="none"){
											wav = pWAVOnBuyLevelHit;
										}else if(pAudibleAlertsEnabled && dir == 'D' && pWAVOnBuyLevelHit!="none"){
											wav = pWAVOnSellLevelHit;
										}
										if(wav !=null){
											wav = wav.Replace("<inst>",Instrument.MasterInstrument.Name);
											double pr = chartScale.GetValueByY(Convert.ToSingle(nodes[keys[1]].Y));
											AlertsMgr.AddLevel(tag, pr, wav, CurrentBars[0]);
											txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, wav, txtFormat, (float)(ChartPanel.X + ChartPanel.W), pWavLabelFontSize);
											labelRect = new SharpDX.RectangleF(10f, Convert.ToSingle(nodes[keys[1]].Y)-txtLayout.Metrics.Height-5f, txtLayout.Metrics.Width+10f, Convert.ToSingle(font.Size)+10f);
											RenderTarget.FillRectangle(labelRect, DimGrayBrushDX);
											RenderTarget.DrawText(wav, txtFormat, labelRect, BlackBrushDX);
											if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
										}
									}
								}
							}
							diff = Convert.ToSingle(Math.Abs(maxy-miny));
							labelRect.X = Convert.ToSingle(minx);
							labelRect.Y = y;
							if(pShowLevelAsLines){
								if(dir == 'D'){
									if(pType == TriangleTarget_Type.Reversing || AutoType == 'R'){
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), MaroonBrushDX);
										y = y + diff*mult;
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), BlueBrushDX);
									}else if(pType == TriangleTarget_Type.Trending || AutoType == 'T'){
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), BlueBrushDX);
										y = y - diff*mult;
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), MaroonBrushDX);
									}
								}else {
									if(pType == TriangleTarget_Type.Reversing || AutoType == 'R'){
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), BlueBrushDX);
										y = y - diff*mult;
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), MaroonBrushDX);
									}else if(pType == TriangleTarget_Type.Trending || AutoType == 'T'){
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), MaroonBrushDX);
										y = y + diff*mult;
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.DrawLine(new SharpDX.Vector2(x,y), new SharpDX.Vector2(x+ChartPanel.W,y), BlueBrushDX);
									}
								}
							}else{
								if(dir == 'D'){
									if(pType == TriangleTarget_Type.Reversing || AutoType == 'R'){
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),13f,3f), MaroonBrushDX);
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y+diff*mult),13f,3f), BlueBrushDX);
									}else if(pType == TriangleTarget_Type.Trending || AutoType == 'T'){
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),13f,3f), BlueBrushDX);
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y-diff*mult),13f,3f), MaroonBrushDX);
									}
								}else {
									if(pType == TriangleTarget_Type.Reversing || AutoType == 'R'){
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),13f,3f), BlueBrushDX);
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y-diff*mult),13f,3f), MaroonBrushDX);
									}else if(pType == TriangleTarget_Type.Trending || AutoType == 'T'){
										if(MaroonBrushDX!=null && MaroonBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),13f,3f), MaroonBrushDX);
										if(BlueBrushDX!=null && BlueBrushDX.IsValid(RenderTarget))
											RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y+diff*mult),13f,3f), BlueBrushDX);
									}
								}
							}
							var pShowRiskDollars = true;
							if(pShowRiskDollars){
								var dollars = Instrument.MasterInstrument.PointValue * Instrument.MasterInstrument.RoundToTickSize(Math.Abs(chartScale.GetValueByY(diff) - chartScale.GetValueByY(0)));
								string s = dollars.ToString("C").Replace(".00",string.Empty);
								txtLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, s, txtFormat, (float)(ChartPanel.X + ChartPanel.W), pWavLabelFontSize);
								labelRect.Width = txtLayout.Metrics.Width+10f;
								labelRect.Height = Convert.ToSingle(font.Size)+20f;
								labelRect.X = labelRect.X - labelRect.Width;
								if(DimGrayBrushDX!=null && DimGrayBrushDX.IsValid(RenderTarget))
									RenderTarget.FillRectangle(labelRect, DimGrayBrushDX);
								if(LimeBrushDX!=null && LimeBrushDX.IsValid(RenderTarget)){
									labelRect.X += 5f;
									labelRect.Y += 5f;
									RenderTarget.DrawText(s, txtFormat, labelRect, LimeBrushDX);
								}
								if(txtLayout != null && !txtLayout.IsDisposed){txtLayout.Dispose(); txtLayout = null;}
							}
						}
}catch(Exception ee1){printDebug(line+":  "+ee1.ToString());}
						#endregion
					} 
					if (objects[idx].ToString().EndsWith(".Ray")) {
						try{
							o = (IDrawingTool)objects[idx];
							if(true || !o.IsSelected){
								tag = o.Tag.ToLower();
								var li = o.Anchors.ToArray();
								o.Anchors.Last().Price = o.Anchors.First().Price;
								Ray r = (Ray)objects[idx];
								if(Medians[0].GetValueAt(Convert.ToInt32(o.Anchors.First().SlotIndex)) > o.Anchors.First().Price)
									r.Stroke = pSellRayStroke;
								else
									r.Stroke = pBuyRayStroke;
							}
						}catch(Exception ee1){Printit(string.Format("TriangleTarget error ({0}): {1}",Instrument.MasterInstrument.Name, ee1.ToString()));}
					}
//}catch(Exception kk){printDebug(line+": "+kk.ToString());}
				}
			}
		}
//=========================================================================================================
		public override void OnRenderTargetChanged()
		{
			#region == OnRenderTargetChanged ==
			if(BlackBrushDX!=null   && !BlackBrushDX.IsDisposed)    {BlackBrushDX.Dispose();   BlackBrushDX=null;}
//			if(YellowBrushDX!=null   && !YellowBrushDX.IsDisposed)    {YellowBrushDX.Dispose();YellowBrushDX=null;}
//			if(PosExpectedDiffBrushDX!=null   && !PosExpectedDiffBrushDX.IsDisposed)    {PosExpectedDiffBrushDX.Dispose();PosExpectedDiffBrushDX=null;}
//			if(NegExpectedDiffBrushDX!=null   && !NegExpectedDiffBrushDX.IsDisposed)    {NegExpectedDiffBrushDX.Dispose();NegExpectedDiffBrushDX=null;}

//			if(CyanBrushDX!=null && !CyanBrushDX.IsDisposed)  {CyanBrushDX.Dispose(); CyanBrushDX=null;}
			if(DimGrayBrushDX!=null && !DimGrayBrushDX.IsDisposed)  {DimGrayBrushDX.Dispose(); DimGrayBrushDX=null;}
			if(LimeBrushDX!=null    && !LimeBrushDX.IsDisposed)     {LimeBrushDX.Dispose();    LimeBrushDX = null;}
//			if(GreenBrushDX!=null    && !GreenBrushDX.IsDisposed)   {GreenBrushDX.Dispose();   GreenBrushDX = null;}
			if(BlueBrushDX!=null    && !BlueBrushDX.IsDisposed)     {BlueBrushDX.Dispose();    BlueBrushDX = null;}
//			if(MagentaBrushDX!=null && !MagentaBrushDX.IsDisposed)  {MagentaBrushDX.Dispose(); MagentaBrushDX = null;}
			if(MaroonBrushDX!=null && !MaroonBrushDX.IsDisposed)  {MaroonBrushDX.Dispose(); MaroonBrushDX = null;}
//			if(txtBrushDX!=null     && !txtBrushDX.IsDisposed)      {txtBrushDX.Dispose();     txtBrushDX=null;}
//			if(BuyValueBrushDX  != null && !BuyValueBrushDX.IsDisposed)  {BuyValueBrushDX.Dispose();  BuyValueBrushDX=null;}
//			if(SellValueBrushDX != null && !SellValueBrushDX.IsDisposed) {SellValueBrushDX.Dispose(); SellValueBrushDX=null;}
//			if(MA1UpDotsBrushDX != null && !MA1UpDotsBrushDX.IsDisposed) {MA1UpDotsBrushDX.Dispose(); MA1UpDotsBrushDX=null;}
//			if(MA1DnDotsBrushDX != null && !MA1DnDotsBrushDX.IsDisposed) {MA1DnDotsBrushDX.Dispose(); MA1DnDotsBrushDX=null;}
//			if(GridBrushDX != null && !GridBrushDX.IsDisposed) {GridBrushDX.Dispose(); GridBrushDX=null;}
//			if(MA1LineBrushDX != null && !MA1LineBrushDX.IsDisposed) {MA1LineBrushDX.Dispose(); MA1LineBrushDX=null;}
//			if(MA2LineBrushDX != null && !MA2LineBrushDX.IsDisposed) {MA2LineBrushDX.Dispose(); MA2LineBrushDX=null;}
//			if(TrendSignalUpDotsDXBrush != null && !TrendSignalUpDotsDXBrush.IsDisposed) {TrendSignalUpDotsDXBrush.Dispose(); TrendSignalUpDotsDXBrush=null;}
//			if(TrendSignalDnDotsDXBrush != null && !TrendSignalDnDotsDXBrush.IsDisposed) {TrendSignalDnDotsDXBrush.Dispose(); TrendSignalDnDotsDXBrush=null;}

//			if(RenderTarget != null) txtBrushDX     = Brushes.Yellow.ToDxBrush(RenderTarget);
			if(RenderTarget != null) BlackBrushDX   = Brushes.Black.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) YellowBrushDX  = Brushes.Yellow.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) {PosExpectedDiffBrushDX  = Brushes.Green.ToDxBrush(RenderTarget); PosExpectedDiffBrushDX.Opacity = 0.5f;}
//			if(RenderTarget != null) {NegExpectedDiffBrushDX  = Brushes.Red.ToDxBrush(RenderTarget); NegExpectedDiffBrushDX.Opacity = 0.5f;}
			if(RenderTarget != null) DimGrayBrushDX = Brushes.DimGray.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) CyanBrushDX   = Brushes.Cyan.ToDxBrush(RenderTarget);
			if(RenderTarget != null) LimeBrushDX    = Brushes.Lime.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) GreenBrushDX    = Brushes.Green.ToDxBrush(RenderTarget);
			if(RenderTarget != null) BlueBrushDX    = Brushes.Blue.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) MagentaBrushDX = Brushes.Magenta.ToDxBrush(RenderTarget);
			if(RenderTarget != null) MaroonBrushDX = Brushes.Maroon.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) BuyValueBrushDX  = Brushes.Blue.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) SellValueBrushDX = Brushes.Pink.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) MA1UpDotsBrushDX = pMA1UpDots.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) MA1DnDotsBrushDX = pMA1DnDots.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) {GridBrushDX = pGridlineBrush.ToDxBrush(RenderTarget); GridBrushDX.Opacity = pGridlineOpacity;}
//			if(RenderTarget != null) MA1LineBrushDX = pMA1LineColor.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) MA2LineBrushDX = pMA2LineColor.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) TrendSignalUpDotsDXBrush = pTrendSignalUpDotsBrush.ToDxBrush(RenderTarget);
//			if(RenderTarget != null) TrendSignalDnDotsDXBrush = pTrendSignalDnDotsBrush.ToDxBrush(RenderTarget);

			#endregion
		}
//=========================================================================================================
//======================================================================================================
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
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";

				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles(search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("<inst>_Buy.wav");
				list.Add("<inst>_Sell.wav");
				list.Add("<inst>_BuySetup.wav");
				list.Add("<inst>_SellSetup.wav");
				list.Add("<inst>_BuyEntry.wav");
				list.Add("<inst>_SellEntry.wav");
				list.Add("none");
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
//======================================================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav.Replace("<inst>",Instrument.MasterInstrument.Name));
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, float.MaxValue)]
		[Display(Name="Targ Mult", Order=10, GroupName="Parameters")]
		public float pTargMult
		{ get; set; }
//		[Display(Order = 30, ResourceType = typeof(Custom.Resource), Name = "Color of buy triangle", GroupName = "Parameters")]
//		public Brush pBrushBuyTriangle {get;set;}
//		[Display(Order = 40, ResourceType = typeof(Custom.Resource), Name = "Color of sell triangle", GroupName = "Parameters")]
//		public Brush pBrushSellTriangle {get;set;}

		[Display(Order = 20, Name = "Triangle type", GroupName = "Parameters", ResourceType = typeof(Custom.Resource))]
		public TriangleTarget_Type pType
		{get;set;}

		[Display(Order = 5, Name = "Audible alerts enabled?", GroupName = "Alerts", ResourceType = typeof(Custom.Resource))]
		public bool pAudibleAlertsEnabled {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 10, Name = "Wav on Buy level hit", GroupName = "Alerts", ResourceType = typeof(Custom.Resource))]
		public string pWAVOnBuyLevelHit {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 20, Name = "Wav on Sell level hit", GroupName = "Alerts", ResourceType = typeof(Custom.Resource))]
		public string pWAVOnSellLevelHit {get;set;}

		[Display(Order = 30, Name = "Wav label size", GroupName = "Alerts", ResourceType = typeof(Custom.Resource))]
		public int pWavLabelFontSize
		{get;set;}
		
		[Display(Order = 10, Name = "Buy ray",Description = "Color of a ray at the top of a buy triangle",GroupName = "Visuals", ResourceType = typeof(Resource))]
		public Stroke pBuyRayStroke { get; set; }

		[Display(Order = 20, Name = "Sell ray",Description = "Color of a ray at the bottom of a sell triangle",GroupName = "Visuals", ResourceType = typeof(Resource))]
		public Stroke pSellRayStroke { get; set; }

		[Display(Order = 30, Name = "Show levels as lines?",Description = "Entry/Target lines?  If not, then small ellipses will print instead",GroupName = "Visuals", ResourceType = typeof(Resource))]
		public bool pShowLevelAsLines { get; set; }

//				[Browsable(false)]
//				public string pBuyRayBrushSerialize{get { return NinjaTrader.Gui.Serialize.BrushToString(pBuyRayBrush); }set { pBuyRayBrush = NinjaTrader.Gui.Serialize.StringToBrush(value); }}
//		[XmlIgnore]
//		[Display(Order = 10, Name = "Buy ray",Description = "Color of a ray at the top of a buy triangle",GroupName = "Visuals", ResourceType = typeof(Resource))]
//		public Brush pBuyRayBrush { get; set; }
//				[Browsable(false)]
//				public string pBuyRayBrushSerialize{get { return NinjaTrader.Gui.Serialize.BrushToString(pBuyRayBrush); }set { pBuyRayBrush = NinjaTrader.Gui.Serialize.StringToBrush(value); }}
//		[XmlIgnore]
//		[Display(Order = 20, Name = "Buy ray",Description = "Color of a ray at the top of a buy triangle",GroupName = "Visuals", ResourceType = typeof(Resource))]
//		public Brush pBuyRayBrush { get; set; }
//				[Browsable(false)]
//				public string pBuyRayBrushSerialize{get { return NinjaTrader.Gui.Serialize.BrushToString(pBuyRayBrush); }set { pBuyRayBrush = NinjaTrader.Gui.Serialize.StringToBrush(value); }}


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Target
		{
			get { return Values[0]; }
		}
		#endregion

	}
}
public enum TriangleTarget_Type {Reversing,Trending,Auto}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TriangleTarget[] cacheTriangleTarget;
		public TriangleTarget TriangleTarget(float pTargMult)
		{
			return TriangleTarget(Input, pTargMult);
		}

		public TriangleTarget TriangleTarget(ISeries<double> input, float pTargMult)
		{
			if (cacheTriangleTarget != null)
				for (int idx = 0; idx < cacheTriangleTarget.Length; idx++)
					if (cacheTriangleTarget[idx] != null && cacheTriangleTarget[idx].pTargMult == pTargMult && cacheTriangleTarget[idx].EqualsInput(input))
						return cacheTriangleTarget[idx];
			return CacheIndicator<TriangleTarget>(new TriangleTarget(){ pTargMult = pTargMult }, input, ref cacheTriangleTarget);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TriangleTarget TriangleTarget(float pTargMult)
		{
			return indicator.TriangleTarget(Input, pTargMult);
		}

		public Indicators.TriangleTarget TriangleTarget(ISeries<double> input , float pTargMult)
		{
			return indicator.TriangleTarget(input, pTargMult);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TriangleTarget TriangleTarget(float pTargMult)
		{
			return indicator.TriangleTarget(Input, pTargMult);
		}

		public Indicators.TriangleTarget TriangleTarget(ISeries<double> input , float pTargMult)
		{
			return indicator.TriangleTarget(input, pTargMult);
		}
	}
}

#endregion
