
#region Using declarations
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion
using SBG;

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using SharpDX.DirectWrite;
using System.Linq;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("Parameters",	10)]
    [CategoryOrder("Level Visuals",	20)]
//    [CategoryOrder("SpecificLevels",	50)]

    /// <summary>
    /// Plots lines at user  defined values.
    /// </summary>
	[Description("")]
	public class RoundNumbers : Indicator
	{
		private bool RunInit = true;
		private Brush RegionBrush=null;
		private Brush SpecificLevel1RegionBrush = null;
		private Brush SpecificLevel2RegionBrush = null;
		private Brush SpecificLevel3RegionBrush = null;
		private Brush SpecificLevel4RegionBrush = null;
		private Brush SpecificLevel5RegionBrush = null;
		private double Interval_points = 0;
		private double LastMinPrice    = double.MaxValue;
		private double BasePrice_calibrated = double.MinValue;
		private DateTime LaunchedAt = DateTime.MinValue;
		private List<double[]> Lvls = new List<double[]>();
		private bool SoundAlertOn   = false;
		SBG.TradeManager tm;
		private double regionInPts = 0;

	protected override void OnStateChange()
	{
		#region OnStateChange
		if (State == State.SetDefaults)
		{
			string ExemptMachine1 = "766C8CD2AD83CA787BCA6A2A76B2303B";
			string ExemptMachine2 = "CB15E08BE30BC80628CFF6010471FA2A";
			bool ExemptMachine = NinjaTrader.Cbi.License.MachineId==ExemptMachine1 || NinjaTrader.Cbi.License.MachineId==ExemptMachine2;
			bool IsBen = System.IO.File.Exists("c:\\222222222222.txt");
//			if(!IsBen && !ExemptMachine)
//				VendorLicense("IndicatorWarehouse", "AIRoundNumbers", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
			AddPlot(new Stroke(Brushes.Navy,2), PlotStyle.Line, "Lvl");

			pStartTime = 600;
			pStopTime = 1600;
			pGoFlatTime = 1600;
			sDaysOfWeek = "M Tu W Th F";
			pShowHrOfDayTable = false;
			IsChartOnly=true;
			IsAutoScale=false;
			Calculate=Calculate.OnBarClose;
			DisplayInDataBox	= false;
			IsOverlay=true;
			Name = "Round Numbers";
			pEnableSoundAlerts = false;
			pCalculateLongTrades = false;
			pCalculateShortTrades = false;
			pWAVOnBuyEntry = "<inst>_BuySetup.wav";
			pWAVOnSellEntry = "<inst>_SellSetup.wav";
			pSLTicks = 4;

		}
		if (State == State.DataLoaded){
			tm = new SBG.TradeManager(this, "RoundNumbers", "", "Long", "Short", Instrument, sDaysOfWeek, pStartTime, pStopTime, pGoFlatTime, pShowHrOfDayTable, 1, 1);
			if(pIntervalType == RoundNumbers_IntervalType.Points){
				if(pIntervalInPts<TickSize*2) Interval_points = TickSize*2;
				else Interval_points = pIntervalInPts;
			}else if(pIntervalType == RoundNumbers_IntervalType.Ticks){
				Interval_points = Math.Max(2,pIntervalInTicks) * TickSize;
			}
			regionInPts = pRegionSizeInTicks*TickSize;
			RegionBrush = pRegionBrush.Clone();
			RegionBrush.Opacity = pRegionOpacity/100.0;
			RegionBrush.Freeze();

			double price = pBasePrice;
			double minPrice = double.MaxValue;
			for(int abar = 0; abar<Bars.Count-2; abar++){
				if(Bars.GetLow(abar) < minPrice) minPrice = Bars.GetLow(abar);
			}
			while(price > minPrice*0.9) price = price - Interval_points;
			while(price < Close[0]*1.1) {
				price = price + Interval_points;
				Lvls.Add(new double[3]{price, price+regionInPts, price-regionInPts});
				if(pDrawAsHorizontalLines){
					var L = Draw.HorizontalLine(this, price.ToString(),false,price,Plots[0].Brush,DashStyleHelper.Dash,2);
					L.IsLocked = true;
				}
			}
		}
		if(State==State.Realtime && LaunchedAt == DateTime.MinValue){
			SoundAlertOn = pLevelHitWAV != "none";
			LaunchedAt = DateTime.Now;
			double sum = 0;
			for(int i = 0; i<15; i++) sum = sum + Range()[i];
			double avg = sum/15;
			double avgD = 0;
			List<double> ranges = new List<double>();
			if(DailyRange.Count>0){
				foreach(var kvp in DailyRange){
					double diff = kvp.Value[0]-kvp.Value[1];
					if(diff>0) ranges.Add(diff);
				}
				avgD = ranges.Average();
			}
			Draw.TextFixed(this, "atrratio","RoundNumbers interval is "+(Interval_points/avg).ToString("0.0")+"x the avg range\nDailyRange is "+(avgD/Interval_points).ToString("0.0")+"x the interval",
					TextPosition.Center,Brushes.White,new SimpleFont("Arial",12), Brushes.Black,Brushes.Black,100);
		}
		#endregion
	}

	SortedDictionary<DateTime,double[]> DailyRange = new SortedDictionary<DateTime,double[]>();
	bool LongEnabled = false;
	bool ShortEnabled = false;
	double BuyEntry = 0;
	double BuyTP = 0;
	double SellEntry = 0;
	double SellTP = 0;
	protected override void OnBarUpdate()
	{
		if(CurrentBar>1){
			if(DailyRange.Count==0 && Times[0][0].Day!=Times[0][1].Day){
				DailyRange[Times[0][0].Date] = new double[2]{Highs[0][0], Lows[0][0]};
			}else if(DailyRange.Count>0){
				if(!DailyRange.ContainsKey(Times[0][0].Date))
					DailyRange[Times[0][0].Date] = new double[2]{Highs[0][0], Lows[0][0]};
				else{
					var H = Math.Max(Highs[0][0], DailyRange[Times[0][0].Date][0]);
					DailyRange[Times[0][0].Date][0] = H;
					var L = Math.Min(Lows[0][0], DailyRange[Times[0][0].Date][0]);
					DailyRange[Times[0][0].Date][0] = L;
				}
			}
		}
		Lvl.Reset(0);
		#region -- Create levels --
		if(IsFirstTickOfBar && Lvls.Count>0){
			if(Lvls.Last()[0] < Highs[0][0]){
				double price = Lvls.Last()[0] + Interval_points;
				Lvls.Add(new double[3]{price, price+regionInPts, price-regionInPts});
			}
			if(Lvls[0][0] > Lows[0][0]){
				double price = Lvls[0][0] - Interval_points;
				Lvls.Insert(0, new double[3]{price, price+regionInPts, price-regionInPts});
			}
		}
		#endregion

		if(SoundAlertOn){
			try{
				var lvl = Lvls.Where(x=> Highs[0][0] >= x[1] && Lows[0][0] <= x[2]).First();
				if(lvl!=null && lvl.Count()>0)
					Alert(DateTime.Now.TimeOfDay.ToString(), Priority.High, "Level hit "+Instrument.MasterInstrument.FormatPrice(lvl.First()), AddSoundFolder(pLevelHitWAV), 1, Brushes.Green, Brushes.White);
			}catch{}
		}
		if(CurrentBars[0]<5) return;
try{
		double H0 = R2T(Highs[0][0]);
		double H1 = R2T(Highs[0][1]);
		double L0 = R2T(Lows[0][0]);
		double L1 = R2T(Lows[0][1]);
		double C0 = R2T(Closes[0][0]);

		if(pCalculateLongTrades || pCalculateShortTrades){
			tm.ExitforEOD(Times[0][0], Times[0][1], Closes[0][1]);
			double Hlvl = double.MinValue;
			double Llvl = double.MinValue;
			try{
				Hlvl = Lvls.FirstOrDefault(k=>k[0] > Closes[0][3])[0];
				Llvl = Lvls.LastOrDefault(k=>k[0] < Closes[0][3])[0];
				if(tm.CurrentPosition==0){
					if(H1 < Hlvl - pSLTicks*TickSize) {SellEntry = Hlvl; SellTP = Llvl + pSLTicks*TickSize;}
					if(L1 > Llvl + pSLTicks*TickSize) {BuyEntry = Llvl; BuyTP = Hlvl - pSLTicks*TickSize;}
				}
			}catch{Hlvl = double.MinValue;}

			if(Hlvl!=double.MinValue && Llvl!=double.MinValue){
				var c1 = tm.IsValidTimeAndDay('S', Times[0][0], Times[0][1], CurrentBars[0]);
				var c2 = H1 < SellEntry && H0 >= SellEntry;
				if(pCalculateShortTrades && c1 && c2 && tm.CurrentPosition != -1 && SellEntry!=0) {
					if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar != CurrentBar){
						Alert(DateTime.Now.ToString(), Priority.Medium, "RoundNumbers Sell level hit at "+Instrument.MasterInstrument.FormatPrice(SellEntry), AddSoundFolder(pWAVOnSellEntry), 1, Brushes.Magenta,Brushes.White);
						tm.AlertBar = CurrentBars[0];
					}
//					tm.DTofLastShort = Times[0][0];//one short trade per day
					tm.AddTrade('S', SellEntry, Times[0][0], (pSLTicks>0? SellEntry+pSLTicks*TickSize:double.MaxValue), SellTP);
//Print("Added short at "+tm.Trades.Last().EntryDT.ToString()+"   Entry: "+tm.Trades.Last().EntryPrice+"  TP: "+tm.Trades.Last().TP);
					//BackBrushes[0] = Brushes.Magenta;
				}
				c1 = tm.IsValidTimeAndDay('L', Times[0][0], Times[0][1], CurrentBars[0]);
				c2 = L1 > BuyEntry && L0 <= BuyEntry;
				if(pCalculateLongTrades && c1 && c2 && tm.CurrentPosition != 1 && BuyEntry!=0) {
					if(State==State.Realtime && pEnableSoundAlerts && tm.AlertBar != CurrentBar){
						Alert(DateTime.Now.ToString(), Priority.Medium, "RoundNumbers Buy level hit at "+Instrument.MasterInstrument.FormatPrice(BuyEntry), AddSoundFolder(pWAVOnSellEntry), 1, Brushes.Magenta,Brushes.White);
						tm.AlertBar = CurrentBars[0];
					}
//					tm.DTofLastLong = Times[0][0];//one Long trade per day
					tm.AddTrade('L', BuyEntry, Times[0][0], (pSLTicks>0? BuyEntry-pSLTicks*TickSize:double.MinValue), BuyTP);
//Print("Added buy at "+tm.Trades.Last().EntryDT.ToString()+"   Entry: "+tm.Trades.Last().EntryPrice+"  TP: "+tm.Trades.Last().TP);
					//BackBrushes[0] = Brushes.Lime;
				}
			}
			tm.ExitforSLTP(Times[0][0], H0, L0, false);
			tm.UpdateMinMaxPrices(H0, L0, C0);
			tm.PrintResults(Bars.Count, CurrentBars[0], true, this);
		}
}catch(Exception e){Print("  error: "+e.ToString());}
	}
		double R2T(double p){
			return Instrument.MasterInstrument.RoundToTickSize(p);
		}
//--------------------------------------------------------------------------------------------------
		public override void OnRenderTargetChanged()
		{
			var status = tm.InitializeBrushes(RenderTarget);
			if(status.Trim().Length>0) Print("tm.InitializeBrushes: "+status);
		}
//--------------------------------------------------------------------------------------------------
		string LastStatus = "";
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
		if (!IsVisible) return;
		double minPrice = chartScale.MinValue; double maxPrice = chartScale.MaxValue;
		base.OnRender(chartControl, chartScale);

		var ts = new TimeSpan(DateTime.Now.Ticks - LaunchedAt.Ticks);
		if(ts.TotalSeconds > 5){
			RemoveDrawObject("atrratio");
		}
		try{
			float RegionHeight = pRegionSizeInTicks*2 * (chartScale.GetYByValue(0) - chartScale.GetYByValue(TickSize));
			float HalfOfRegionHeight = Convert.ToSingle((pRegionSizeType == RoundNumbers_RegionType.Ticks ? RegionHeight : pRegionSizeInPixels))/2.0f;
			double price = BasePrice_calibrated;
			if(BasePrice_calibrated == double.MinValue){//the calibrated price is the first key price level below the current min price of the chart
				//performing this calibration will optimize the hunt for key levels...instead of using the "pBasePrice", we're finding a key price closer to the market
				price = pBasePrice;
				if(LastMinPrice > chartScale.MinValue-Interval_points || LastMinPrice > chartScale.MinValue-Interval_points){
					price = pBasePrice;
					if(price>minPrice){
						while(price>minPrice) price = price - Interval_points;
					}else{
						while(price<minPrice) price = price + Interval_points;
						price = price - Interval_points;
					}
					LastMinPrice = price - Interval_points;
				}else
					price = LastMinPrice;
				BasePrice_calibrated = price;
			}
			if(price>minPrice){
				while(price>minPrice) price = price - Interval_points;//find the first price level below the min price of the chart
			}

			int x2 = ChartPanel.W;
			int y1 = 0;
			var rect = new SharpDX.RectangleF(0, 0, ChartPanel.W, (pRegionSizeType == RoundNumbers_RegionType.Ticks ? RegionHeight : pRegionSizeInPixels));
			var v1 = new SharpDX.Vector2(0,y1);
			var v2 = new SharpDX.Vector2(x2,y1);
			var regionBrush = RegionBrush.ToDxBrush(RenderTarget);
			while(price<=maxPrice) {
				price = price + Interval_points;

				y1 = chartScale.GetYByValue( price);
				if(pRegionOpacity>0){
					rect.Y = y1-HalfOfRegionHeight;
					RenderTarget.FillRectangle(rect,regionBrush);
				}
				if(!pDrawAsHorizontalLines){
					v1.Y = y1;
					v2.X = x2;
					v2.Y = y1;
					RenderTarget.DrawLine(v1, v2, Plots[0].BrushDX, Plots[0].Width);
				}
			}
			if(regionBrush!=null && !regionBrush.IsDisposed) regionBrush.Dispose(); regionBrush = null;
//			double distToUpper = Math.Abs(Closes[0].GetValueAt(CurrentBars[0]-1) - ExpectedHigh.GetValueAt(CurrentBars[0]-1));
//			double distToLower = Math.Abs(Closes[0].GetValueAt(CurrentBars[0]-1) - ExpectedLow.GetValueAt(CurrentBars[0]-1));
			bool SwitchOutput = false;//Keyboard.IsKeyDown(Key.LeftCtrl);
			if(!SwitchOutput){
				if(tm.OutputLS.Count>0){
					tm.OnRender(RenderTarget, ChartPanel, tm.OutputLS, 14, 10);
					if(tm.Status != LastStatus){
						Print("OnRender: "+tm.Status);
						LastStatus = tm.Status;
					}
				}
//			}else{
//				if(distToLower < distToUpper && tm.OutputL.Count>0){
//					tm.OnRender(RenderTarget, ChartPanel, tm.OutputL, 14, 10);
//				}
//				if(distToUpper < distToLower && tm.OutputS.Count>0){
//					tm.OnRender(RenderTarget, ChartPanel, tm.OutputS, 14, 10);
//				}
			}

} catch (Exception err){Print(err.ToString());}
		}
		//========================================================================================================

        #region Plot
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Lvl
        {
            get { return Values[0]; }
        }
		#endregion
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

				var list = new System.Collections.Generic.List<string>();//new string[filCustom.Length+1];
				list.Add("none");
				list.Add("<inst>_LevelTouched.wav");
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
		private string AddSoundFolder(string wav){
			wav = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(wav.Replace("<inst>",Instruments[0].MasterInstrument.Name)," "));
			if(!System.IO.File.Exists(wav)) Log("RoundNumbers: WAV file not found: "+wav, LogLevel.Information);
//			else Log("RoundNumbers WAV file played: "+wav, LogLevel.Information);
			//if(IsDebug) Print("Playing sound: "+wav);
			return wav;
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
		#region -- Specific Levels ==
		private double pLevel1 = 0;
		private double pLevel2 = 0;
		private double pLevel3 = 0;
		private double pLevel4 = 0;
		private double pLevel5 = 0;
//        [Description("Specific, user-defined levels to insert into chart")]
//        [Category("SpecificLevels")]
//        public double     Level1
//        {
//			get { return pLevel1; }
//			set {        pLevel1 = value; }
//        }
//        [Description("Specific, user-defined levels to insert into chart")]
//        [Category("SpecificLevels")]
//        public double     Level2
//        {
//			get { return pLevel2; }
//			set {        pLevel2 = value; }
//        }
//        [Description("Specific, user-defined levels to insert into chart")]
//        [Category("SpecificLevels")]
//        public double     Level3
//        {
//			get { return pLevel3; }
//			set {        pLevel3 = value; }
//        }
//		[Description("Specific, user-defined levels to insert into chart")]
//		[Category("SpecificLevels")]
//		public double     Level4
//		{
//			get { return pLevel4; }
//			set {        pLevel4 = value; }
//		}
//		[Description("Specific, user-defined levels to insert into chart")]
//		[Category("SpecificLevels")]
// 		public double     Level5
// 		{
//			get { return pLevel5; }
//			set {        pLevel5 = value; }
//		}
		
		private Brush pRegionLevel1Brush = Brushes.Purple;
		private Brush pRegionLevel2Brush = Brushes.Purple;
		private Brush pRegionLevel3Brush = Brushes.Purple;
		private Brush pRegionLevel4Brush = Brushes.Purple;
		private Brush pRegionLevel5Brush = Brushes.Purple;
//		[XmlIgnore()]
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Region Level1",  GroupName = "SpecificLevels")]
//		public Brush Region1_Brush{	get { return pRegionLevel1Brush; }	set { pRegionLevel1Brush = value; }		}
//					[Browsable(false)]
//					public string Region1ClSerialize
//					{	get { return Serialize.BrushToString(pRegionLevel1Brush); } set { pRegionLevel1Brush = Serialize.StringToBrush(value); }
//					}
//		[XmlIgnore()]
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Region Level2",  GroupName = "SpecificLevels")]
//		public Brush Region2_Brush{	get { return pRegionLevel2Brush; }	set { pRegionLevel2Brush = value; }		}
//					[Browsable(false)]
//					public string Region2ClSerialize
//					{	get { return Serialize.BrushToString(pRegionLevel2Brush); } set { pRegionLevel2Brush = Serialize.StringToBrush(value); }
//					}
//		[XmlIgnore()]
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Region Level3",  GroupName = "SpecificLevels")]
//		public Brush Region3_Brush{	get { return pRegionLevel3Brush; }	set { pRegionLevel3Brush = value; }		}
//					[Browsable(false)]
//					public string Region3ClSerialize
//					{	get { return Serialize.BrushToString(pRegionLevel3Brush); } set { pRegionLevel3Brush = Serialize.StringToBrush(value); }
//					}
//		[XmlIgnore()]
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Region Level4",  GroupName = "SpecificLevels")]
//		public Brush Region4_Brush{	get { return pRegionLevel4Brush; }	set { pRegionLevel4Brush = value; }		}
//					[Browsable(false)]
//					public string Region4ClSerialize
//					{	get { return Serialize.BrushToString(pRegionLevel4Brush); } set { pRegionLevel4Brush = Serialize.StringToBrush(value); }
//					}
//		[XmlIgnore()]
//		[Description("")]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Region Level5",  GroupName = "SpecificLevels")]
//		public Brush Region5_Brush{	get { return pRegionLevel5Brush; }	set { pRegionLevel5Brush = value; }		}
//					[Browsable(false)]
//					public string Region5ClSerialize
//					{	get { return Serialize.BrushToString(pRegionLevel5Brush); } set { pRegionLevel5Brush = Serialize.StringToBrush(value); }
//					}

		private int pRegion1Opacity = 4;
		private int pRegion2Opacity = 4;
		private int pRegion3Opacity = 4;
		private int pRegion4Opacity = 4;
		private int pRegion5Opacity = 4;
//		[Description("Opacity of Line1 colored region.  0 is transparent, 10 is solid")]
//		[Category("SpecificLevels")]
//		public int Region1Opacity
//		{
//			get { return pRegion1Opacity; }
//			set { pRegion1Opacity = Math.Max(0,Math.Min(10,value)); }
//		}
//		[Description("Opacity of Line2 colored region.  0 is transparent, 10 is solid")]
//		[Category("SpecificLevels")]
//		public int Region2Opacity
//		{
//			get { return pRegion2Opacity; }
//			set { pRegion2Opacity = Math.Max(0,Math.Min(10,value)); }
//		}
//		[Description("Opacity of Line3 colored region.  0 is transparent, 10 is solid")]
//		[Category("SpecificLevels")]
//		public int Region3Opacity
//		{
//			get { return pRegion3Opacity; }
//			set { pRegion3Opacity = Math.Max(0,Math.Min(10,value)); }
//		}
//		[Description("Opacity of Line4 colored region.  0 is transparent, 10 is solid")]
//		[Category("SpecificLevels")]
//		public int Region4Opacity
//		{
//			get { return pRegion4Opacity; }
//			set { pRegion4Opacity = Math.Max(0,Math.Min(10,value)); }
//		}
//		[Description("Opacity of Line5 colored region.  0 is transparent, 10 is solid")]
//		[Category("SpecificLevels")]
//		public int Region5Opacity
//		{
//			get { return pRegion5Opacity; }
//			set { pRegion5Opacity = Math.Max(0,Math.Min(10,value)); }
//		}

		#endregion

		private double pBasePrice = 0.00;
        [Description("Base price...the price from which all lines eminate at Interval values")]
        [Display(Order = 10, Name = "Key Price", GroupName = "Parameters")]
        public double BasePrice
        {
            get { return pBasePrice; }
            set { pBasePrice = value; }
        }

		private RoundNumbers_IntervalType pIntervalType = RoundNumbers_IntervalType.Points;
        [Description("Interval type")]
        [Display(Order = 30, Name = "Interval type", GroupName = "Parameters")]
        public RoundNumbers_IntervalType IntervalType
        {
            get { return pIntervalType; }
            set { pIntervalType = value; }
        }

		private double pIntervalInPts = 10;
        [Description("Interval (in points) between lines")]
        [Display(Order = 31, Name = "Interval in pts", GroupName = "Parameters")]
        public double IntervalInPts
        {
            get { return pIntervalInPts; }
            set { pIntervalInPts = Math.Max(0,value); }
        }
		private double pIntervalInTicks = 10;
        [Description("Interval (in ticks) between lines")]
        [Display(Order = 32, Name = "Interval in ticks", GroupName = "Parameters")]
        public double IntervalInTicks
        {
            get { return pIntervalInTicks; }
            set { pIntervalInTicks = Math.Max(2,value); }
        }
		private string pLevelHitWAV = "<inst>_LevelTouched.wav";
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order=10, Name="Level Hit", GroupName="Audible Alerts", Description="Sound file when a bar first hits a round number zone")]
		public string LevelHitWAV
		{
			get { return pLevelHitWAV; }
			set { pLevelHitWAV = value; }
		}
		#region -- Level Visuals --
		private Brush pRegionBrush = Brushes.Gold;
		[XmlIgnore()]
		[Description("Color of all auto-generated regions")]
		[Display(Order = 10, ResourceType = typeof(Custom.Resource), Name = "Highlight Color",  GroupName = "Level Visuals")]
		public Brush Region_Brush{	get { return pRegionBrush; }	set { pRegionBrush = value; }		}
					[Browsable(false)]
					public string RegionClSerialize
					{	get { return Serialize.BrushToString(pRegionBrush); } set { pRegionBrush = Serialize.StringToBrush(value); }
					}

		private RoundNumbers_RegionType pRegionSizeType = RoundNumbers_RegionType.Ticks;
        [Description("Size of colored highlight region around the round number lines, in ticks")]
        [Display(Order = 21, Name = "Highlight Thickness Type", GroupName = "Level Visuals")]
        public RoundNumbers_RegionType RegionSizeType
        {
            get { return pRegionSizeType; }
            set { pRegionSizeType = value; }
        }
		private int pRegionSizeInTicks = 1;
        [Description("Size of colored highlight region around the round number lines, in ticks")]
        [Display(Order = 22, Name = "Highlight Thickness Ticks", GroupName = "Level Visuals")]
        public int RegionSizeInTicks
        {
            get { return pRegionSizeInTicks; }
            set { pRegionSizeInTicks = Math.Max(0,value); }
        }
		private int pRegionSizeInPixels = 5;
        [Description("Size of colored highlight region around the round number lines, in screen pixels")]
        [Display(Order = 23, Name = "Highlight Thickness Pixels", GroupName = "Level Visuals")]
        public int RegionSizeInPixels
        {
            get { return pRegionSizeInPixels; }
            set { pRegionSizeInPixels = Math.Max(0,value); }
        }
		private int pRegionOpacity = 0;
		[Description("Opacity of colored region around the round number lines.  0 is transparent, 100 is solid")]
        [Display(Order = 30, Name = "Highlight Opacity", GroupName = "Level Visuals")]
		public int Region_Opacity
		{
			get { return pRegionOpacity; }
			set { pRegionOpacity = Math.Max(0,Math.Min(100,value)); }
		}
		private bool pDrawAsHorizontalLines = false;
        [Description("If you have graphics issues, then choose to draw levels as Horizontal Line drawing objects")]
        [Display(Order = 40, Name = "Draw as HLine Objects", GroupName = "Level Visuals")]
        public bool DrawAsHorizontalLines
        {
			get { return pDrawAsHorizontalLines; }
			set {        pDrawAsHorizontalLines = value; }
        }

        #endregion
		#region -- Strategy params --
		[Display(Name="Permit LONG trades", Order=10, GroupName="Strategy", ResourceType = typeof(Custom.Resource))]
		public bool pCalculateLongTrades
		{ get; set; }

		[Display(Name="Permit SHORT trades", Order=20, GroupName="Strategy", ResourceType = typeof(Custom.Resource))]
		public bool pCalculateShortTrades
		{ get; set; }

		[Range(0,int.MaxValue)]
		[Display(Order = 25, Name="SL (ticks)", GroupName="Strategy", Description="",  ResourceType = typeof(Custom.Resource))]
		public int pSLTicks
		{get;set;}

		[NinjaScriptProperty]
		[Display(Order = 30, Name = "Days of week", GroupName = "Strategy", ResourceType = typeof(Custom.Resource))]
		public string sDaysOfWeek
		{ get; set; }

		[Display(Order = 40, Name="Trade Start time", GroupName="Strategy", Description="Trading is permitted after this time",  ResourceType = typeof(Custom.Resource))]
		public int pStartTime
		{get;set;}

		[Display(Order = 50, Name="Trade Stop time", GroupName="Strategy", Description="No more trades initiated after this time", ResourceType = typeof(Custom.Resource))]
		public int pStopTime
		{get;set;}

		[Display(Order = 60, Name="Trade Exit time", GroupName="Strategy",  Description="All open trades are flattened at this time", ResourceType = typeof(Custom.Resource))]
		public int pGoFlatTime
		{get;set;}

		[Display(Order = 10, Name="Show 'Hr of Day' Table?", GroupName="Strategy Visuals",  Description="", ResourceType = typeof(Custom.Resource))]
		public bool pShowHrOfDayTable
		{get;set;}

		#endregion
		#region -- Alerts --
		[Display(Order = 70, ResourceType = typeof(Custom.Resource), Name = "Enable sound alerts?", GroupName = "Strategy")]
		public bool pEnableSoundAlerts {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 80, ResourceType = typeof(Custom.Resource), Name = "Wav on Buy Entry", GroupName = "Strategy")]
		public string pWAVOnBuyEntry {get;set;}

		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(Order = 90, ResourceType = typeof(Custom.Resource), Name = "Wav on Sell Entry", GroupName = "Strategy")]
		public string pWAVOnSellEntry {get;set;}
		#endregion

	}
public enum RoundNumbers_IntervalType {Points, Ticks}
public enum RoundNumbers_RegionType {Ticks, Pixels}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RoundNumbers[] cacheRoundNumbers;
		public RoundNumbers RoundNumbers(string sDaysOfWeek)
		{
			return RoundNumbers(Input, sDaysOfWeek);
		}

		public RoundNumbers RoundNumbers(ISeries<double> input, string sDaysOfWeek)
		{
			if (cacheRoundNumbers != null)
				for (int idx = 0; idx < cacheRoundNumbers.Length; idx++)
					if (cacheRoundNumbers[idx] != null && cacheRoundNumbers[idx].sDaysOfWeek == sDaysOfWeek && cacheRoundNumbers[idx].EqualsInput(input))
						return cacheRoundNumbers[idx];
			return CacheIndicator<RoundNumbers>(new RoundNumbers(){ sDaysOfWeek = sDaysOfWeek }, input, ref cacheRoundNumbers);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RoundNumbers RoundNumbers(string sDaysOfWeek)
		{
			return indicator.RoundNumbers(Input, sDaysOfWeek);
		}

		public Indicators.RoundNumbers RoundNumbers(ISeries<double> input , string sDaysOfWeek)
		{
			return indicator.RoundNumbers(input, sDaysOfWeek);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RoundNumbers RoundNumbers(string sDaysOfWeek)
		{
			return indicator.RoundNumbers(Input, sDaysOfWeek);
		}

		public Indicators.RoundNumbers RoundNumbers(ISeries<double> input , string sDaysOfWeek)
		{
			return indicator.RoundNumbers(input, sDaysOfWeek);
		}
	}
}

#endregion
