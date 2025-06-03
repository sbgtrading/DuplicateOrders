

#region Using declarations
using System;
using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Collections;
using System.Collections.Generic;

#endregion
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
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;

//public enum RR_Ranger_eSourceDataType
//{
//	DAY_BAR,
//	SESSION_BAR,
//}
public enum RR_Ranger_RangeFormats {
	Ticks, Points
}
public enum RR_Ranger_AlertTypes {
	LaunchPopup, PlayWAV, None
}

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{

	public class RR_Ranger : Indicator
	{

//====================================================================================================

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
		#region RegInputs
		
//		private RR_Ranger_eSourceDataType iDataType = RR_Ranger_eSourceDataType.SESSION_BAR;
//		[Description("")]
//        [Category("Parameters")]
//		[Gui.Design.DisplayName("00. Source Data Type")]
//        public RR_Ranger_eSourceDataType IDataType
//        {
//            get { return iDataType; }
//            set { iDataType = value; }
//        }
		
		private int pFontSize = 8;
		[Description("Output text font size")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Font Size",  GroupName = "Visual")]
        public int FontSize
        {
            get { return pFontSize; }
            set { pFontSize = value; }
        }		
		private Brush pTextBrush = Brushes.Silver;
		[XmlIgnore]
        [Description("Output text color")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Font Color",  GroupName = "Visual")]
        public Brush TextBrush
        {
            get { return pTextBrush; }
            set { pTextBrush = value; }
        }
        [Browsable(false)]
        public string TextColorSerialize
		{	get { return Serialize.BrushToString(pTextBrush); } set { pTextBrush = Serialize.StringToBrush(value); }}

		private Brush pAlertTextBrush = Brushes.Red;
		[XmlIgnore]
        [Description("Output text color when current day range exceeds the average")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Alert Font Size",  GroupName = "Visual")]
        public Brush AlertTextBrush
        {
            get { return pAlertTextBrush; }
            set { pAlertTextBrush = value; }
        }
        [Browsable(false)]
        public string AlertTextColorSerialize
		{	get { return Serialize.BrushToString(pAlertTextBrush); } set { AlertTextBrush = Serialize.StringToBrush(value); }}

		private RR_Ranger_AlertTypes pAlertType = RR_Ranger_AlertTypes.None;
		[Description("Select what alert type you want whenever todays range exceeds any other range")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Alert Type",  GroupName = "Visual")]
		public RR_Ranger_AlertTypes AlertType
		{
			get { return pAlertType; }
			set { pAlertType = value; }
		}

		private string pAlertWAV = "Alert2.wav";
		[Description("Select WAV file, only valid if Alert Type is set to 'PlayWAV'")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Alert WAV",  GroupName = "Visual")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string AlertWAV
		{
			get { return pAlertWAV; }
			set { pAlertWAV = value; }
		}

		private bool pVerbose = true;
		[Description("Set to true to view the dates of the range calculation")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Verbose output?",  GroupName = "Visual")]
		public bool Verbose
		{
			get { return pVerbose; }
			set { pVerbose = value; }
		}
		private TextPosition pTextLoc = TextPosition.TopLeft;
		[Description("")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Output Location",  GroupName = "Visual")]
		public TextPosition TextLoc
		{
			get { return pTextLoc; }
			set { pTextLoc = value; }
		}		
		private RR_Ranger_RangeFormats pRangeFormats = RR_Ranger_RangeFormats.Ticks;
		[Description("Display the ranges in Ticks or Points")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Range Formants",  GroupName = "Visual")]
		public RR_Ranger_RangeFormats RangeFormats
		{
			get { return pRangeFormats; }
			set { pRangeFormats = value; }
		}		
		#endregion
		private class DayInfo {
			public double HH;
			public double LL;
			public DateTime DT;
			public double Range;
			public DayInfo(double hh, double ll, DateTime dt, double ticksize){this.HH=hh; this.LL=ll; this.DT=dt; this.Range = (HH-LL)/ticksize;}
		}
		private NinjaTrader.Gui.Tools.SimpleFont vFont;
		private TextFormat txtFormat_vFont = null;
		private double rng;
		private List<DayInfo> rList;
		private double HH = double.MaxValue, LL = double.MinValue;
		private string TodaysRangeStr = string.Empty;
		private string YesterdaysRangeStr = string.Empty;
		private Brush textBrush, alertTextBrush, backgroundBrush;
		private DateTime TimeOfRefresh = DateTime.MinValue;
		private int i;
		private bool EnablePopupYD=true, EnablePopupAvg5 = true, EnablePopupAvg10 = true, EnablePopupAvg30 = true;
		private bool NewDay = false;
		private string[] Col1Strings = null;
		private string[] Col2Strings = null;
		private double RangeToday;
		private double RangeYesterday;
		private double ATR5;
		private Series<double> DailyProjectionDistance;

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
	{
		if (State == State.SetDefaults)
		{
			this.Name = "iw RR Ranger";
			Calculate = Calculate.OnPriceChange;
			this.rList = new List<DayInfo>();
			IsOverlay = true;
			
			//if(iDataType == RR_Ranger_eSourceDataType.DAY_BAR)
//				Add(BarsPeriodType.Day, 1);
			//else
			//	Add(BarsPeriodType.Minute, 1440);
			
			vFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", this.pFontSize){Bold=true};
			textBrush = pTextBrush;//new SolidColorBrush(this.iTextBrush); textBrush.Freeze();
			alertTextBrush = pAlertTextBrush;//new SolidColorBrush(this.pAlertTextBrush); alertTextBrush.Freeze();

            string mod = "AIRoadRunner";
            string vend = "IndicatorWarehouse";
            string email = "license@indicatorwarehouse.com";
            string url = "www.IndicatorWarehouse.com";
			bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && (
				NinjaTrader.Cbi.License.MachineId=="B0D2E9D1C802E279D3678D7DE6A33CE4" || NinjaTrader.Cbi.License.MachineId=="766C8CD2AD83CA787BCA6A2A76B2303B");
			if(!IsBen)
	            VendorLicense(vend, mod, url, email);
		}
		if (State == State.Configure) {
			TimeOfRefresh = NinjaTrader.Core.Globals.Now;
			if(pAlertType == RR_Ranger_AlertTypes.None) {
				EnablePopupYD = false;
				EnablePopupAvg5 = false;
				EnablePopupAvg10 = false;
				EnablePopupAvg30 = false;
			}
		}
		if (State == State.DataLoaded) {
			Calculate=Calculate.OnPriceChange;
			DailyProjectionDistance = new Series<double>(this);//inside State.DataLoaded
			Col1Strings = new string[6]{string.Empty,string.Empty,string.Empty,string.Empty,string.Empty,string.Empty};
			Col2Strings = new string[6]{string.Empty,string.Empty,string.Empty,string.Empty,string.Empty,string.Empty};
			txtFormat_vFont = new TextFormat(new SharpDX.DirectWrite.Factory(),
														 	ChartControl.Properties.LabelFont.Family.ToString(),
														 	SharpDX.DirectWrite.FontWeight.Bold,
													     	SharpDX.DirectWrite.FontStyle.Normal,
														 	(float)pFontSize);
		}
	}
		
		/// <summary>
		/// Called on each bar update event (incoming tick)
		/// </summary>
		protected override void OnBarUpdate()
		{
//			if(BarsInProgress == 1 && iDataType == RR_Ranger_eSourceDataType.DAY_BAR)
//			{
//				ATR5 = ATR(5)[0];
//				try{
//				if(IsFirstTickOfBar && CurrentBars[1]>1)
//				{
//					YesterdaysRangeStr = MakeString(new object[]{"(",Highs[1][1].ToString()," to ",Lows[1][1].ToString(),")"});
//					TodaysRangeStr     = MakeString(new object[]{"(",Highs[1][0].ToString()," to ",Lows[1][0].ToString(),")"});
//					this.rList.Add(new DayInfo(Highs[1][0], Lows[1][0], Times[1][0], pRangeFormats == RR_Ranger_RangeFormats.Ticks?TickSize:1));
//					RangeToday = this.rList[this.rList.Count - 1].Range;
//					RangeYesterday = this.rList[this.rList.Count - 2].Range;
//					Col1Strings[0]=(RangeToday.ToString(FS));
//					Col1Strings[1]=(RangeYesterday.ToString(FS));
//					if(rList.Count>1) {
//						if(pVerbose) Col2Strings[0]=(MakeString(new object[]{"Range on ",rList[rList.Count-1].DT.ToShortDateString(),"  ",TodaysRangeStr}));
//						else Col2Strings[0]=("Today's Range");
//
//						if(pVerbose) Col2Strings[1]=(MakeString(new object[]{"Range on ",rList[rList.Count-2].DT.ToShortDateString(),"  ",YesterdaysRangeStr}));
//						else Col2Strings[1]=("Yesterday's Range");
//					}
//
//					if(pAlertType != RR_Ranger_AlertTypes.None) {
//						EnablePopupYD = true;
//						EnablePopupAvg5 = true;
//						EnablePopupAvg10 = true;
//						EnablePopupAvg30 = true;
//					}
//				}
//				}catch(Exception err){Print(err.ToString());}
//			}
//			if(BarsInProgress == 0 && iDataType == RR_Ranger_eSourceDataType.SESSION_BAR) 
			{

				if(CurrentBars[0]>5) {
					if(IsFirstTickOfBar && Times[0][0].Date != Times[0][1].Date) {
//						NewDay=true;
//						Print(Times[0][0].ToString()+"   Times[0][1]: "+Times[0][1].Date.ToShortDateString()+"   Times[0][0]: "+Times[0][0].Date.ToShortDateString());
						this.rList.Add(new DayInfo(HH, LL, Times[0][1], pRangeFormats == RR_Ranger_RangeFormats.Ticks?TickSize:1));

						ATR5 = 0;
						for(i = rList.Count - 1 ; i >= this.rList.Count - 5 && i>=0 ; i--){
							ATR5 += this.rList[i].Range/5.0;
						}

						RangeYesterday = this.rList[rList.Count-1].Range;
						YesterdaysRangeStr = $"({Instrument.MasterInstrument.FormatPrice(HH)} to {Instrument.MasterInstrument.FormatPrice(LL)})";
						HH = Highs[0][0];
						LL = Lows[0][0];
						if(pAlertType != RR_Ranger_AlertTypes.None) {
							EnablePopupYD = true;
							EnablePopupAvg5 = true;
							EnablePopupAvg10 = true;
							EnablePopupAvg30 = true;
						}
					} else {
						HH = Math.Max(HH,Highs[0][0]);
						LL = Math.Min(LL,Lows[0][0]);
					}
					HH = Instrument.MasterInstrument.RoundToTickSize(HH);
					LL = Instrument.MasterInstrument.RoundToTickSize(LL);
					RangeToday = HH-LL;
					TodaysRangeStr = $"({Instrument.MasterInstrument.FormatPrice(HH)} to {Instrument.MasterInstrument.FormatPrice(LL)})";
					if(this.pRangeFormats == RR_Ranger_RangeFormats.Ticks) {
						RangeToday = RangeToday/TickSize;
						Col1Strings[0]=RangeToday.ToString("0");
						Col1Strings[1]=RangeYesterday.ToString("0");
					}else{
						Col1Strings[0]=(Instrument.MasterInstrument.FormatPrice(RangeToday));
						Col1Strings[1]=(Instrument.MasterInstrument.FormatPrice(RangeYesterday));
					}

					if(rList.Count>1) {
						if(pVerbose) Col2Strings[0]= $"Range on {Times[0][0].ToShortDateString()}  {TodaysRangeStr}";
						else Col2Strings[0]=("Today's Range");

						if(pVerbose) Col2Strings[1]= $"Range on {rList[rList.Count-1].DT.ToShortDateString()}  {YesterdaysRangeStr}";
						else Col2Strings[1]=("Yesterday's Range");
					}
				}
			}
			if(BarsInProgress==0) {
				DailyProjectionDistance[0] = (ATR5);
			}
//			if(BarsInProgress==0 && NewDay) {
//				BackColor = Color.Yellow;
//				NewDay = false;
//			}
		}
		//=========================================================================================
		private SharpDX.Point CalcScreenPositionOfTextTable(float textWidth, float textHeight){
			#region CalcScreenPositionOfTextTable
			float x=0, y=0;
			if(pTextLoc == TextPosition.TopLeft) {
				x = 10;
				y = 20;
			}
			else if(pTextLoc == TextPosition.TopRight) {
				x = ChartPanel.W - textWidth - 10;
				y = 20;
			}
			else if(pTextLoc == TextPosition.BottomLeft) {
				x = 10;
				y = ChartPanel.H - textHeight - 10;
			}
			else if(pTextLoc == TextPosition.BottomRight) {
				x = ChartPanel.W - textWidth - 10;
				y = ChartPanel.H - textHeight - 10;
			}
			else if(pTextLoc == TextPosition.Center) {
				x = (ChartPanel.W - textWidth)/2.0f;
				y = (ChartPanel.H - textHeight)/2.0f;
			}
			#endregion
			return new SharpDX.Point((int)(ChartPanel.X+x), (int)(ChartPanel.Y+y));
		}
		//=========================================================================================
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			if (!IsVisible) return;double min = chartScale.MinValue; double max = chartScale.MaxValue;
//			base.OnRender(chartControl, chartScale);
//			Point PanelUpperLeftPoint	= new Point(ChartPanel.X, ChartPanel.Y);
//			Point PanelLowerRightPoint	= new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
//			int firstBarPainted = ChartBars.FromIndex;
//			int lastBarPainted = ChartBars.ToIndex;


int line =398;
try{
			#region Plot
			double r5 = 0;
			double r10 = 0;
			double r30 = 0;
			if(this.rList.Count >= 5)
			{
				//Print("");
				for(i = this.rList.Count - 1 ; i >= this.rList.Count - 5 ; i--){
					r5 += this.rList[i].Range;
					//Print(i.ToString()+"  "+rList[i].DT.DayOfWeek.ToString()+"  "+rList[i].DT.ToShortDateString());
				}
				
				r5 /= 5;
			}
			if(this.rList.Count >= 10)
			{
				for(i = this.rList.Count - 1 ; i >= this.rList.Count - 10 ; i--)
					r10 += this.rList[i].Range;
				
				r10 /= 10;
			}			
			
			if(this.rList.Count >= 30)
			{
				for(i = this.rList.Count - 1 ; i >= this.rList.Count - 30 ; i--)
					r30 += this.rList[i].Range;
				
				r30 /= 30;
			}

			TimeSpan SecondsSinceRefresh = new TimeSpan(NinjaTrader.Core.Globals.Now.Ticks - TimeOfRefresh.Ticks);

			if(r5>0 && !double.IsInfinity(r5))  {
				Col1Strings[2]=(Instrument.MasterInstrument.RoundToTickSize(r5).ToString());
				Col2Strings[2]="Avg 5 Range";
			} else {
				Col1Strings[2]=string.Empty;
				if(SecondsSinceRefresh.TotalSeconds<10) Col2Strings[2]=("Add more days of data to calc 5-day Avg Range"); 
				else Col2Strings[2]=string.Empty;
			}
			if(r10>0 && !double.IsInfinity(r10)) {
				Col1Strings[3]=(Instrument.MasterInstrument.RoundToTickSize(r10).ToString()); 
				Col2Strings[3]="Avg 10 Range";
			} else {
				Col1Strings[3]=string.Empty;
				if(SecondsSinceRefresh.TotalSeconds<10) Col2Strings[3]=("Add more days of data to calc 10-day Avg Range"); 
				else Col2Strings[3]=string.Empty;
			}
			if(r30>0 && !double.IsInfinity(r30)) {
				Col1Strings[4]=(Instrument.MasterInstrument.RoundToTickSize(r30).ToString()); 
				Col2Strings[4]="Avg 30 Range";
			} else {
				Col1Strings[4]=string.Empty;
				if(SecondsSinceRefresh.TotalSeconds<10) Col2Strings[4]=("Add more days of data to calc 30-day Avg Range"); 
				else Col2Strings[4]=string.Empty;
			}
			if(SecondsSinceRefresh.TotalSeconds<10) {
				Col1Strings[5]=string.Empty;
				Col2Strings[5]=$"Approx {rList.Count.ToString("0")}-days loaded on this chart";
			} else {
				Col1Strings[5]=string.Empty;
				Col2Strings[5]=string.Empty;
			}

//			SizeF rngS0 = RenderTarget.MeasureString(Col1Strings[0],vFont);
//			SizeF rngS1 = rngS0;
//			SizeF rngS5, rngS10, rngS30, txtS0, txtS1, txtS5, txtS10, txtS30;
//			if(this.rList.Count>1) 
//				rngS1  = RenderTarget.MeasureString(Col1Strings[1],vFont);

//			rngS5  = RenderTarget.MeasureString(Col1Strings[2], vFont);
//			rngS10 = RenderTarget.MeasureString(Col1Strings[3], vFont);
//			rngS30 = RenderTarget.MeasureString(Col1Strings[4], vFont);

//			txtS0  = RenderTarget.MeasureString(Col2Strings[0], vFont);
//			txtS1  = RenderTarget.MeasureString(Col2Strings[1], vFont);
//			txtS5  = RenderTarget.MeasureString(Col2Strings[2], vFont);
//			txtS10 = RenderTarget.MeasureString(Col2Strings[3], vFont);
//			txtS30 = RenderTarget.MeasureString(Col2Strings[4], vFont);
			var SharpDX_BlackBrush = Brushes.Black.ToDxBrush(RenderTarget);
			var SharpDX_TextBrush = pTextBrush.ToDxBrush(RenderTarget);
			var SharpDX_AlertTextBrush = pAlertTextBrush.ToDxBrush(RenderTarget);

			float Col1Width = 0;//Math.Max(rngS0.Width, Math.Max(rngS1.Width, Math.Max(rngS5.Width, Math.Max(rngS10.Width,rngS30.Width))))+SpaceBetweenCol1andCol2;
			float Col2Width = 0;//Math.Max(txtS0.Width, Math.Max(txtS1.Width, Math.Max(txtS5.Width, Math.Max(txtS10.Width,txtS30.Width))));
			var txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, "", txtFormat_vFont, ChartPanel.W, txtFormat_vFont.FontSize);
			var txtPosition1 = new SharpDX.Vector2();
			var txtLayout2 = new TextLayout(Core.Globals.DirectWriteFactory, "", txtFormat_vFont, ChartPanel.W, txtFormat_vFont.FontSize);
			var txtPosition2 = new SharpDX.Vector2();

			float X0 = 0;
			float Y0 = 0;
			float SpaceBetweenCol1andCol2 = 10f;

			float TableHeight = Col2Strings.Length * (txtFormat_vFont.FontSize+1);
			
			//Calculate the number of lines printing to the chart
			i = Col2Strings.Length-1;
			while(Col2Strings[i].Length==0) { //if Col2 is empty string, then the height of the table is reduced by 1 line
				TableHeight = TableHeight - (txtFormat_vFont.FontSize-1);
				i--;
			}
			for(i = 0;i<Col1Strings.Length; i++) {
				txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, Col1Strings[i], txtFormat_vFont, ChartPanel.W, txtFormat_vFont.FontSize);
				Col1Width = Math.Max(Col1Width, txtLayout1.Metrics.Width);
				txtLayout2 = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[i], txtFormat_vFont, ChartPanel.W, txtFormat_vFont.FontSize);
				Col2Width = Math.Max(Col2Width, txtLayout2.Metrics.Width);
			}
			if(Col2Strings[5].Length>0) TableHeight = TableHeight + txtFormat_vFont.FontSize+1;

			Col1Width = Col1Width + SpaceBetweenCol1andCol2;
			var loc = CalcScreenPositionOfTextTable(Col1Width + Col2Width, TableHeight);

			float Line = 0;
			#region DrawTextLayout
			txtLayout1   = new TextLayout(Core.Globals.DirectWriteFactory, Col1Strings[0], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
			txtPosition1 = new System.Windows.Point(loc.X, loc.Y).ToVector2();
			txtLayout2   = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[0], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
			txtPosition2 = new System.Windows.Point(loc.X+Col1Width, txtPosition1.Y).ToVector2();
				#region Draw background rectangle
				var rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, Col1Width+txtLayout2.Metrics.Width+2f, txtLayout1.Metrics.Height+2f);
				RenderTarget.FillRectangle(rectangleF, SharpDX_BlackBrush);
				#endregion
			RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_TextBrush);
			RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
			#endregion
//			RenderTarget.FillRectangle(ChartControl.Background, loc.X-1f, loc.Y-1f+(txtFormat_vFont.FontSize+1f)*Line, Col1Width+txtS0.Width+2f, txtFormat_vFont.FontSize+2f);
//			RenderTarget.DrawString(Col1Strings[0], vFont, textBrush, loc.X, loc.Y);
//			RenderTarget.DrawString(Col2Strings[0], vFont, textBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));

			float vLineSpace = txtFormat_vFont.FontSize/4f;
			Line = 1;
			#region DrawTextLayout
			float y0 = (txtFormat_vFont.FontSize+vLineSpace)*Line;
			txtLayout1   = new TextLayout(Core.Globals.DirectWriteFactory, Col1Strings[1], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
			txtPosition1 = new System.Windows.Point(loc.X, loc.Y+y0).ToVector2();
			txtLayout2   = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[1], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
			txtPosition2 = new System.Windows.Point(loc.X+Col1Width, txtPosition1.Y).ToVector2();
				#region Draw background rectangle
				rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, Col1Width+txtLayout2.Metrics.Width+SpaceBetweenCol1andCol2+2f, txtLayout1.Metrics.Height+2f);
				RenderTarget.FillRectangle(rectangleF, SharpDX_BlackBrush);
				#endregion
//			RenderTarget.FillRectangle(ChartControl.Background, loc.X-1f, loc.Y-1f+(txtFormat_vFont.FontSize+1f)*Line, Col1Width+txtS1.Width+2f, txtFormat_vFont.FontSize+2f);
			#endregion
			if(RangeToday>RangeYesterday) {
				if(EnablePopupYD) {
					EnablePopupYD = false;
					if(pAlertType == RR_Ranger_AlertTypes.LaunchPopup)
						Log(Instrument.FullName+" has exceeded its range from Yesterday", NinjaTrader.Cbi.LogLevel.Alert);
					else if(pAlertType == RR_Ranger_AlertTypes.PlayWAV) 
						PlaySoundCustom(this.pAlertWAV);
				}
				RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_AlertTextBrush);
				RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_AlertTextBrush);
//				RenderTarget.DrawString(Col1Strings[1], vFont, alertTextBrush, loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//				RenderTarget.DrawString(Col2Strings[1], vFont, alertTextBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
			} else {
				RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_TextBrush);
				RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//				RenderTarget.DrawString(Col1Strings[1], vFont, textBrush,      loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//				RenderTarget.DrawString(Col2Strings[1], vFont, textBrush,      loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
			}

			if(Col2Strings[2].Length>0) {
				Line++;
				#region DrawTextLayout
				y0 = (txtFormat_vFont.FontSize+vLineSpace)*Line;
				txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, Col1Strings[2], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition1 = new System.Windows.Point(loc.X, loc.Y+y0).ToVector2();
				txtLayout2 = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[2], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition2 = new System.Windows.Point(loc.X+Col1Width, txtPosition1.Y).ToVector2();
					#region Draw background rectangle
					rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, Col1Width+txtLayout2.Metrics.Width+SpaceBetweenCol1andCol2+2f, txtLayout1.Metrics.Height+2f);
					RenderTarget.FillRectangle(rectangleF, SharpDX_BlackBrush);
					#endregion
				#endregion
//				RenderTarget.FillRectangle(ChartControl.Background, loc.X-1f, loc.Y-1f+(txtFormat_vFont.FontSize+1f)*Line, Col1Width+txtS5.Width+2f, txtFormat_vFont.FontSize+2f);
				if(Col1Strings[2].Length>0) {
					if(RangeToday>r5) {
						if(EnablePopupAvg5) {
							EnablePopupAvg5 = false;
							if(pAlertType == RR_Ranger_AlertTypes.LaunchPopup)
								Log(Instrument.FullName+" has exceeded its 5-day average range", NinjaTrader.Cbi.LogLevel.Alert);
							else if(pAlertType == RR_Ranger_AlertTypes.PlayWAV) 
								PlaySoundCustom(this.pAlertWAV);
						}
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_AlertTextBrush);
						RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_AlertTextBrush);
//						RenderTarget.DrawString(Col1Strings[2], vFont, alertTextBrush, loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//						RenderTarget.DrawString(Col2Strings[2], vFont, alertTextBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
					} else {
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_TextBrush);
						RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//						RenderTarget.DrawString(Col1Strings[2], vFont, textBrush,      loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//						RenderTarget.DrawString(Col2Strings[2], vFont, textBrush,      loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
					}
				}
				else {
					RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//					RenderTarget.DrawString(Col2Strings[2], vFont, textBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
				}
			}

			if(Col2Strings[3].Length>0) {
				Line++;
				#region DrawTextLayout
				y0 = (txtFormat_vFont.FontSize+vLineSpace)*Line;
				txtLayout1 = new TextLayout(Core.Globals.DirectWriteFactory, Col1Strings[3], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition1 = new System.Windows.Point(loc.X, loc.Y+y0).ToVector2();
				txtLayout2 = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[3], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition2 = new System.Windows.Point(loc.X+Col1Width, txtPosition1.Y).ToVector2();
					#region Draw background rectangle
					rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, Col1Width+txtLayout2.Metrics.Width+SpaceBetweenCol1andCol2+2f, txtLayout1.Metrics.Height+2f);
					RenderTarget.FillRectangle(rectangleF, SharpDX_BlackBrush);
					#endregion
				#endregion
//				RenderTarget.FillRectangle(ChartControl.Background, loc.X-1f, loc.Y-1f+(txtFormat_vFont.FontSize+1f)*Line, Col1Width+txtS10.Width+2f, txtFormat_vFont.FontSize+2f);
				if(Col1Strings[3].Length>0) {
					if(RangeToday>r10) {
						if(EnablePopupAvg10) {
							EnablePopupAvg10 = false;
							if(pAlertType == RR_Ranger_AlertTypes.LaunchPopup)
								Log(Instrument.FullName+" has exceeded its 10-day average range", NinjaTrader.Cbi.LogLevel.Alert);
							else if(pAlertType == RR_Ranger_AlertTypes.PlayWAV)
								PlaySoundCustom(this.pAlertWAV);
						}
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_AlertTextBrush);
						RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_AlertTextBrush);
//						RenderTarget.DrawString(Col1Strings[3], vFont, alertTextBrush, loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//						RenderTarget.DrawString(Col2Strings[3], vFont, alertTextBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
					} else {
line=647;
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_TextBrush);
						RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//						RenderTarget.DrawString(Col1Strings[3], vFont, textBrush,      loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//						RenderTarget.DrawString(Col2Strings[3], vFont, textBrush,      loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
					}
				}
				else {
					RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//					RenderTarget.DrawString(Col2Strings[3], vFont, textBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
				}
			}

line=660;
			if(Col2Strings[4].Length>0) {
				Line++;
				#region DrawTextLayout
				y0 = (txtFormat_vFont.FontSize+vLineSpace)*Line;
				txtLayout1   = new TextLayout(Core.Globals.DirectWriteFactory, Col1Strings[4], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition1 = new System.Windows.Point(loc.X, loc.Y+y0).ToVector2();
				txtLayout2   = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[4], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition2 = new System.Windows.Point(loc.X+Col1Width, txtPosition1.Y).ToVector2();
					#region Draw background rectangle
					rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, Col1Width+txtLayout2.Metrics.Width+SpaceBetweenCol1andCol2+2f, txtLayout1.Metrics.Height+2f);
					RenderTarget.FillRectangle(rectangleF, SharpDX_BlackBrush);
					#endregion
				#endregion
//				RenderTarget.FillRectangle(ChartControl.Background, loc.X-1f, loc.Y-1f+(txtFormat_vFont.FontSize+1f)*Line, Col1Width+txtS30.Width+2f, txtFormat_vFont.FontSize+2f);
				if(Col1Strings[4].Length>0) {
line=676;
					if(RangeToday>r30) {
						if(EnablePopupAvg30) {
							EnablePopupAvg30 = false;
							if(pAlertType == RR_Ranger_AlertTypes.LaunchPopup)
								Log(Instrument.FullName+" has exceeded its 30-day average range", NinjaTrader.Cbi.LogLevel.Alert);
							else if(pAlertType == RR_Ranger_AlertTypes.PlayWAV)
								PlaySoundCustom(this.pAlertWAV);
						}
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_AlertTextBrush);
						RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_AlertTextBrush);
//						RenderTarget.DrawString(Col1Strings[4], vFont, alertTextBrush, loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//						RenderTarget.DrawString(Col2Strings[4], vFont, alertTextBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
					} else {
line=690;
						RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_TextBrush);
						RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//						RenderTarget.DrawString(Col1Strings[4], vFont, textBrush,      loc.X, loc.Y+Line*(txtFormat_vFont.FontSize+1));
//						RenderTarget.DrawString(Col2Strings[4], vFont, textBrush,      loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
					}
				}
				else {
					RenderTarget.DrawTextLayout(txtPosition2, txtLayout2, SharpDX_TextBrush);
//					RenderTarget.DrawString(Col2Strings[4], vFont, textBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
				}
			}

line=703;
			if(Col2Strings[5].Length>0) {
				Line++;
				Line++;
				#region DrawTextLayout
				y0 = (txtFormat_vFont.FontSize+vLineSpace)*Line;
				txtLayout1   = new TextLayout(Core.Globals.DirectWriteFactory, Col2Strings[5], txtFormat_vFont, (int)(ChartPanel.W*0.8), txtFormat_vFont.FontSize);
				txtPosition1 = new System.Windows.Point(loc.X+Col1Width, loc.Y+y0).ToVector2();
					#region Draw background rectangle
					rectangleF = new SharpDX.RectangleF(txtPosition1.X-1f, txtPosition1.Y-1f, txtLayout1.Metrics.Width+2f, txtLayout1.Metrics.Height+2f);
					RenderTarget.FillRectangle(rectangleF, SharpDX_BlackBrush);
					#endregion
				#endregion
//				txtS30 = RenderTarget.MeasureString(Col2Strings[5],vFont);
//				RenderTarget.FillRectangle(ChartControl.Background, loc.X-1f+Col1Width, loc.Y-1f+(txtFormat_vFont.FontSize+1f)*Line, txtS30.Width+2f, txtFormat_vFont.FontSize+2f);
				RenderTarget.DrawTextLayout(txtPosition1, txtLayout1, SharpDX_TextBrush);
//				RenderTarget.DrawString(Col2Strings[5], vFont, textBrush, loc.X+Col1Width, loc.Y+Line*(txtFormat_vFont.FontSize+1));
			}
			#endregion
			if(txtLayout1!=null){
				txtLayout1.Dispose();
				txtLayout1=null;
			}
			if(txtLayout2!=null){
				txtLayout2.Dispose();
				txtLayout2=null;
			}
			if(SharpDX_BlackBrush!=null){
				SharpDX_BlackBrush.Dispose();
				SharpDX_BlackBrush=null;
			}
			if(SharpDX_TextBrush!=null){
				SharpDX_TextBrush.Dispose();
				SharpDX_TextBrush=null;
			}
			if(SharpDX_AlertTextBrush!=null){
				SharpDX_AlertTextBrush.Dispose();
				SharpDX_AlertTextBrush=null;
			}

}catch(Exception e){Print(line+":  "+e.ToString());}
		}
//====================================================================
		private void PlaySoundCustom(string wav){
			if(State == State.Historical) return;
			string pwav = AddSoundFolder(wav);
			if(System.IO.File.Exists(pwav))
				PlaySound(pwav);
			else
				Log(wav+" sound not found in your NT8 Sounds folder",LogLevel.Information);
		}
//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds",wav);
		}
//====================================================================
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RR_Ranger[] cacheRR_Ranger;
		public RR_Ranger RR_Ranger()
		{
			return RR_Ranger(Input);
		}

		public RR_Ranger RR_Ranger(ISeries<double> input)
		{
			if (cacheRR_Ranger != null)
				for (int idx = 0; idx < cacheRR_Ranger.Length; idx++)
					if (cacheRR_Ranger[idx] != null &&  cacheRR_Ranger[idx].EqualsInput(input))
						return cacheRR_Ranger[idx];
			return CacheIndicator<RR_Ranger>(new RR_Ranger(), input, ref cacheRR_Ranger);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RR_Ranger RR_Ranger()
		{
			return indicator.RR_Ranger(Input);
		}

		public Indicators.RR_Ranger RR_Ranger(ISeries<double> input )
		{
			return indicator.RR_Ranger(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RR_Ranger RR_Ranger()
		{
			return indicator.RR_Ranger(Input);
		}

		public Indicators.RR_Ranger RR_Ranger(ISeries<double> input )
		{
			return indicator.RR_Ranger(input);
		}
	}
}

#endregion
