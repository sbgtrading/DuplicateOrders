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
	public class RectangleRepeater : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Rectangle Repeater";
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
				pWAVOnBuyLevelHit = "Alert2.wav";
				pWAVOnSellLevelHit = "Alert2.wav";
				pBuyRayStroke = new Stroke(Brushes.Cyan, DashStyleHelper.Dash, 2f);
				pSellRayStroke = new Stroke(Brushes.DeepPink, DashStyleHelper.Dash, 2f);
				//pType = RectangleTarget_Type.Reversing;
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

		List<IDrawingTool> objects = null;
//=========================================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
//			if(!ValidLicense || TerminalError){
//				return;
//			}
			if(font == null) font = new SimpleFont("Arial", (float)pWavLabelFontSize);

			objects = DrawObjects.Where(xo=>xo.ToString().Contains(".Rectangle")).ToList();
			o = null;
			int abarLeft = 0;
			int abarRight = 0;
			if(objects!=null && objects.Count>0){
				var txtFormat = font.ToDirectWriteTextFormat();
				SharpDX.RectangleF labelRect;
				SharpDX.DirectWrite.TextLayout txtLayout = null;
				AlertsMgr.DeleteOldLevels(CurrentBars[0], 1);

				for(int idx = 0; idx<objects.Count; idx++){
					if (objects[idx].ToString().EndsWith(".Rectangle")) {
						o = (IDrawingTool)objects[idx];
						tag = o.Tag.ToLower();
						if(tag.Contains("-copy") || tag.StartsWith("@")) continue;
try{

						var li = o.Anchors.ToArray();
						foreach(var anch in li) {
							if(anch.SlotIndex <= 0) continue;
							if(abarLeft == 0) abarLeft = Convert.ToInt32(anch.SlotIndex);
							else {
								abarRight = Convert.ToInt32(anch.SlotIndex);
								break;
							}
						}
}catch(Exception ee1){printDebug(line+":  "+ee1.ToString());}
					} 
				}
				if(abarLeft > 0){
Print($"{tag}  {abarLeft}  {abarRight}");

					var t0 = Times[0].GetValueAt(abarLeft);
					var t1 = Times[0].GetValueAt(abarRight);
					var day0 = Times[0].GetValueAt(1).Date;
					var timeNow = Times[0].GetValueAt(CurrentBars[0]);
					while(day0.Ticks <= DateTime.Now.Date.Ticks){
						var t00 = new DateTime(day0.Year, day0.Month, day0.Day, t0.Hour, t0.Minute, t0.Second);
						var t10 = new DateTime(day0.Year, day0.Month, day0.Day, t1.Hour, t1.Minute, t1.Second);
						if(t00.Ticks > timeNow.Ticks) break;
						if(t10.Ticks > timeNow.Ticks) break;
//						int abar0 = BarsArray[0].GetBar(t00);
//						int abar1 = BarsArray[0].GetBar(t10);
//						if(abar0 != abar1 && t0.Date != day0.Date)
						{
							double maxY = double.MinValue;
							double minY = double.MaxValue;
//							for(int b = Math.Min(abar0, abar1); b <= Math.Max(abar0, abar1); b++){
//								maxY = Math.Max(maxY, Highs[0].GetValueAt(b));
//								minY = Math.Min(minY, Lows[0].GetValueAt(b));
//							}
//							var tag0 = $"{tag}-{abar0}-copy";
//							Draw.Rectangle(this, tag0, t00, maxY, t10, minY, Brushes.Pink);
Print($"       {t00} {maxY}  {t10} {minY}");
						}
						day0.AddDays(1);
					}
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
//			if(LimeBrushDX!=null    && !LimeBrushDX.IsDisposed)     {LimeBrushDX.Dispose();    LimeBrushDX = null;}
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
//			if(RenderTarget != null) LimeBrushDX    = Brushes.Lime.ToDxBrush(RenderTarget);
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
public enum RectangleTarget_Type {Reversing,Trending}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RectangleRepeater[] cacheRectangleRepeater;
		public RectangleRepeater RectangleRepeater(float pTargMult)
		{
			return RectangleRepeater(Input, pTargMult);
		}

		public RectangleRepeater RectangleRepeater(ISeries<double> input, float pTargMult)
		{
			if (cacheRectangleRepeater != null)
				for (int idx = 0; idx < cacheRectangleRepeater.Length; idx++)
					if (cacheRectangleRepeater[idx] != null && cacheRectangleRepeater[idx].pTargMult == pTargMult && cacheRectangleRepeater[idx].EqualsInput(input))
						return cacheRectangleRepeater[idx];
			return CacheIndicator<RectangleRepeater>(new RectangleRepeater(){ pTargMult = pTargMult }, input, ref cacheRectangleRepeater);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RectangleRepeater RectangleRepeater(float pTargMult)
		{
			return indicator.RectangleRepeater(Input, pTargMult);
		}

		public Indicators.RectangleRepeater RectangleRepeater(ISeries<double> input , float pTargMult)
		{
			return indicator.RectangleRepeater(input, pTargMult);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RectangleRepeater RectangleRepeater(float pTargMult)
		{
			return indicator.RectangleRepeater(Input, pTargMult);
		}

		public Indicators.RectangleRepeater RectangleRepeater(ISeries<double> input , float pTargMult)
		{
			return indicator.RectangleRepeater(input, pTargMult);
		}
	}
}

#endregion
