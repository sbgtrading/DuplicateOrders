#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
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

using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
#endregion


///Derived from the jtEconNews2a Indicator. 

/// 04-22-2019 - Changed logic that select brush color by impact to account for unexpected impact (caused object error on current brush)
/// 03-15-2021 - Changed URL for news
/// 05/15/2023 - Added extra sound alerts and custom sound file.  Customized text sizes, colors, and background color.  Reduced number of hits on website.  Plus a lot more.
/// 08/30/2023 - Added display news after event time and active news color display options.  Fixed font display.  Move display down if Show date range is on.  Broke code into more granular methods. 
 
namespace NinjaTrader.NinjaScript.Indicators
{

	[Gui.CategoryOrder("News Filters", 0)]
	[Gui.CategoryOrder("Alerts", 	20)]
	[Gui.CategoryOrder("Visuals",	40)]
//	[Gui.CategoryOrder("Fonts", 	50)]
	[Gui.CategoryOrder("Signals",	60)]
	[Gui.CategoryOrder("Debug", 	100)]

	public class NewsEcon : Indicator
	{
		private bool	debug	= false, 	advDebug	= false,		newsDebug	= true,		newsListDebug	= false,		newsXMLListDebug	= false;

		#region Vars
		
		// Default internal parameters and limits
		private const int maxAlertWarningMinutesBefore	= 120;
		private const int alertRearmSeconds				= 60*60*12; // 12hrs.  To prevent multiple Alerts Log entries caused by chart reloads.
		private const double minutesToUpdateNews		= 45.0;
		private string fullImminentSoundFileName 		= NinjaTrader.Core.Globals.InstallDir+@"sounds\News_Event_imminent.wav";
		private const float	textStartCoordX				= 45f;
		private 	  float	textStartCoordY				= 23f;
		private static string	indiName				= "News (Econ) 2c";


		private	SimpleFont titleFont 	= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Bold = false };
		private	SimpleFont defaultFont	= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12) { Bold = false };
		private	SimpleFont alertFont 	= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 13) { Bold = true, Italic = true };
	
		private System.Windows.Media.Brush colorHeader			= Brushes.Gold;
		private System.Windows.Media.Brush colorImpactHigh		= Brushes.White;
		private System.Windows.Media.Brush colorImpactMed		= Brushes.Lime;
		private System.Windows.Media.Brush colorImpactLow		= Brushes.MediumSeaGreen;
		private System.Windows.Media.Brush colorImpactOther 	= Brushes.Gray;	
		private System.Windows.Media.Brush colorBackground 		= Brushes.Black;	
		private System.Windows.Media.Brush colorAlertLogText	= Brushes.Lime;
		private System.Windows.Media.Brush colorAlertLogBg		= Brushes.Black;
		private System.Windows.Media.Brush colorActiveEvent		= Brushes.OrangeRed;
		private System.Windows.Media.Brush rectBackgroundColor;


		//private const string ffNewsUrl = @"http://cdn-nfs.faireconomy.media/ff_calendar_thisweek.xml";   // 04-09-20919, was: http://www.forexfactory.com/ffcal_week_this.xml
		private const string ffNewsUrl		= @"http://nfs.faireconomy.media/ff_calendar_thisweek.xml";  // changed 03/15/2021
		private static string TITLE_TXT 	= indiName+" - News courtesy of FairEconomy.media (all times are local)";
		private const string TIME_TXT 		= "Time";
		private const string IMPACT_TXT 	= "Impact";
		private const string DESC_TXT 		= "News Description  (prev/forecast)";
		private const string FX_TXT 		= "FX   "+DESC_TXT;
		private const string NO_NEWS_TXT 	= "No News Events to list.";
		private const float TIME_PAD		= 12;
		private const float IMPACT_PAD		= 12;
		private const float FX_PAD			= 12;
		private const float DESC_PAD		= 12;
		private const float	offsetBgRectangleX	= 5f,	offsetBgRectangleY		= 3f;
		//				Offset percentage from Top Left corner.
		private double	headerFontSizeIncr	= 2;	// offsetPercentX	= 0.04,		offsetPercentY	= 0.06,	
		private float widestTimeCol 		= 0;
		private float widestImpactCol		= 0;
		private float widestDescCol 		= 0;
		private float totalHeight			= 0;
		private float rectWidth				= 0;
		
		private int lastNewsPtr			= -1,			newsItemPtr 		= 0,		historicalBarCount	= 0,					sendAlertsOnImpactTypeValue,	displayEventImpactValue;
		private DateTime nextNewsUpdate = DateTime.MinValue,							nextCheck			= DateTime.MaxValue,	startOfWeek					= DateTime.MaxValue;
		private DateTime firstEventTime	= DateTime.	MinValue,							lastEventTime		= DateTime.MinValue;
		private string lastLoadError	= "",	fullPendingSoundFileName	= "";
		private bool printDebug			= false, 		isRealtime			= false,	isDataLoaded		= false,				isHistorical				= true;
		private bool playSoundAlert		= false, 		isNewsLoaded		= false, 	isListBuilt			= false,				setActiveColor				= true;
//		private Tuple<DateTime, int>	_dicMultiEventAlertCounter	= new Tuple<DateTime, int>(DateTime.MinValue, 0);

//		private ArrayList 					_alstAllEvents 					= new ArrayList();
		private	List<TextLine>				_lstTextLine;		
		private NewsEvent[] 				_arrNewsEvents					= null,		_arrAllEvents		= null;
		private Dictionary<DateTime, int>	_dicMultiEventAlertCounter		= new Dictionary<DateTime, int>();
		

		// Must specify that culture that should be used to parse the ForexFactory date/time data.
		private CultureInfo ffDateTimeCulture = CultureInfo.CreateSpecificCulture("en-US");
	
		#endregion


		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				#region SetDefaults
				Description					= @"Formerly jtEcon News v2a";
				Name						= indiName;
//				Calculate					= Calculate.OnBarClose;
				IsChartOnly					= true;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= false;
//				IsSuspendedWhileInactive	= false;
				
				NewsPeriod					= EnumNewsEconNewsPeriod.Next_24Hrs;
				MaxNewsItems 				= 10;					
				USOnlyEvents 				= true;
				DisplayEventImpact 			= EnumNewsEconEventImpact.High_Medium;
				NewsRefeshInterval 			= 12;
				
				SendAlertsOnImpactType		= EnumNewsEconAlertImpact.High_only;
				PendingSoundFileNameOnly	= "News_Event_pending.wav"; //"Alert1.wav";
				AlertWarningMinutesBefore	= 15;
				DisplayNewsMinutesAfter		= 20;
								
				Use24HrTimeFormat 			= false;
				ShowBackground				= true;
				BackgroundOpacity			= 50;
				
				#endregion
			}
			else if (State == State.Configure)
			{
				#region Configure
				if (advDebug) Print(Name+": State."+State+" - Started ");
				
				printDebug			= Debug;
				advDebug			= Debug && advDebug;
				newsDebug			= newsDebug;
				newsListDebug		= Debug && newsListDebug;
				newsXMLListDebug	= Debug && newsXMLListDebug;
				
				displayEventImpactValue		= Math.Abs(Convert.ToInt32(DisplayEventImpact) );
				sendAlertsOnImpactTypeValue	= Math.Abs(Convert.ToInt32(SendAlertsOnImpactType) );
				
				setActiveColor				= colorActiveEvent.ToString() != Brushes.Transparent.ToString();
					if (newsListDebug) Print(Name+": State."+State+": \t sendAlertsOnImpactTypeValue = "+sendAlertsOnImpactTypeValue+" \t setActiveColor: "+setActiveColor);
				#endregion
			}
			else if (State == State.DataLoaded)
			{
				#region DataLoaded
				if (advDebug) Print(Name+": State."+State+" - Started ");
				isDataLoaded		= true;
				
				rectBackgroundColor 		= BackgroundColor.Clone();
				rectBackgroundColor.Opacity = BackgroundOpacity*0.01;
				rectBackgroundColor.Freeze();
				
				_lstTextLine 		= new List<TextLine>();	
				
				// Pending sound file  Plays before event time.
				fullPendingSoundFileName = NinjaTrader.Core.Globals.InstallDir+@"sounds\"+PendingSoundFileNameOnly;
				if (!File.Exists(fullPendingSoundFileName))
				{
					fullPendingSoundFileName = NinjaTrader.Core.Globals.InstallDir+@"sounds\Alert4.wav";
					if (advDebug) Print(Name+": State."+State+" --  The sound file '"+PendingSoundFileNameOnly+"' was not found.  Check spelling.  Using NT sound file 'Alert4.wav' instead.");
				}
				// Imminent sound file.  Plays at event time.
				if (!File.Exists(fullImminentSoundFileName))	fullImminentSoundFileName = NinjaTrader.Core.Globals.InstallDir+@"sounds\Alert1.wav";
				
				if (advDebug) Print(Name+": State."+State+" - END:  fullPendingSoundFileName:  "+fullPendingSoundFileName);
				#endregion
			}
			else if (State == State.Historical)
			{
				#region Historical
				if (advDebug) Print(Name+": State."+State+" - Started ");
				isHistorical 	= true;
				DateTime now	= DateTime.Now;
				int hr			= NewsRefeshInterval + (now.Minute > 44 ? 1 : 0);
				
				nextNewsUpdate	= DateTime.Parse(now.Date.AddHours( (double)now.Hour).AddMinutes( 45.0 +(hr*60) ).ToString() );// Do updates exactly at 45 minutes after the hour.	//now.AddHours(NewsRefeshInterval).AddSeconds(59-now.Second);
				if (advDebug) Print(Name+": State."+State+":  nextNewsUpdate = "+nextNewsUpdate);

				int daysDiff	= (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
				startOfWeek		= now.AddDays(-1 * daysDiff).Date;
				
				if (advDebug) Print(Name+": State."+State+":  .Now = "+DateTime.Now+" \t daysDiff = "+daysDiff+" \t startOfWeek = "+startOfWeek);
				
				nextCheck 		= startOfWeek.AddMinutes(1); //	nextCheck 		= now.AddSeconds(59-now.Second);
				if (advDebug) Print(Name+": State."+State+":  nextCheck = "+nextCheck+" \t nextNewsUpdate = "+nextNewsUpdate+" \t .Now = "+DateTime.Now+" \t Time[0]: "+Time0()+" \t CurrentBar = "+CurrentBar);
				
				if (advDebug) Print(Name+": State."+State+":  Initial LoadNews() and ProcessList()...");
				LoadNews(225);

				#endregion
			}
			else if (State == State.Transition)
			{
				if (advDebug) Print(Name+": State."+State+":  CurrentBar = "+CurrentBar);
				isHistorical	= false;
				if(ChartControl.Properties.ShowDateRange)
					textStartCoordY += textStartCoordY;
				if (advDebug) Print(Name+": State."+State+" - END:  nextCheck = "+nextCheck+" \t nextNewsUpdate = "+nextNewsUpdate+" \t .Now = "+DateTime.Now+" \t Time[0] = "+Time0()+" \t CurrentBar = "+CurrentBar);
			}
			else if (State == State.Realtime)
			{
				if (advDebug) Print(Name+": State."+State+":  CurrentBar = "+CurrentBar);
				isRealtime		= true;
			}
		}


		protected override void OnBarUpdate()
		{

			if(isHistorical && Time[0] < startOfWeek)	return;
				if (printDebug) Print("OnBarUpdate:  State."+State+"  \t Time[0]: "+Time0()+" \t nextCheck: "+nextCheck+")  \t\t\t CurrentBar: "+CurrentBar);
			
			
			//  only need to update at most every minute since this is the max granularity of the 
			//	 date/time of the news events.  This saves a little wear and tear.
			if (Time[0] >= nextCheck)
			{
if (printDebug) Print(String.Format("OnBarUpdate:  nextNewsUpdate = {0} \t nextNewsUpdate+NewsRefeshInterval = {1} \t DateTime.Now = {2}",nextNewsUpdate.ToShortTimeString(), nextNewsUpdate.AddHours(NewsRefeshInterval).ToShortTimeString(), DateTime.Now.ToShortTimeString() ));
				
				// download the news data every news refresh interval (not bar interval).
				if (isRealtime && Time[0] >= nextNewsUpdate)
				{
					nextNewsUpdate = nextNewsUpdate.AddHours(NewsRefeshInterval);
					if (printDebug) Print("OnBarUpdate:   New nextNewsUpdate = "+nextNewsUpdate+"  \t\t\t\t CurrentBar: "+CurrentBar);
					LoadNews(262);
				}
				ProcessList();

				// Only need to rebuild list at most once every minute
				nextCheck = Time[0].AddMinutes(1); // isRealtime ? Time[0].AddMinutes(1) : DateTime.Now.AddMinutes(1);
				if (printDebug) Print("OnBarUpdate:   New nextCheck = "+nextCheck+"  \t\t\t\t CurrentBar: "+CurrentBar);
			}

			//  SOUND ONLY ALERTS  --------------------------
			#region SOUND ALERTS
				if (printDebug) Print("OnBarUpdate:   Check Sound Alerts:  "+(playSoundAlert && isListBuilt) );
			if (playSoundAlert && isListBuilt)
			{
				TimeSpan timeDiff 				= firstEventTime - Time[0]; //DateTime.Now;
				double timeDiffMinutes			= Math.Round(timeDiff.TotalMinutes, 1);
				double halfAlertWarningMinutes	= Math.Round( (AlertWarningMinutesBefore*0.5), 1);
				if (printDebug) Print(String.Format("OnBarUpdate:   Play Sound testing.  \t timeDiffMinutes = {0} \t halfAlertWarningMinutes = {1} \t multiEAC[{2}] = {3} \t Time[0] = {4} \t\t CurrentBar: {5}",
														timeDiffMinutes, halfAlertWarningMinutes, firstEventTime.ToShortTimeString(), 
														(_dicMultiEventAlertCounter.ContainsKey(firstEventTime)? _dicMultiEventAlertCounter[firstEventTime].ToString():"Null"), 
														Time[0], CurrentBar ));

				//  play sound at halfway mark, if 'Notify X Minutes' is 10 minutes or more
				if (AlertWarningMinutesBefore >= 10 && _dicMultiEventAlertCounter.ContainsKey(firstEventTime) && _dicMultiEventAlertCounter[firstEventTime] == 1
					&& timeDiffMinutes <= halfAlertWarningMinutes+0.5 && timeDiffMinutes >= halfAlertWarningMinutes-0.5)
				{
					PlaySound(fullPendingSoundFileName);
					_dicMultiEventAlertCounter[firstEventTime] = 2;
					
					if (printDebug) Print(String.Format("OnBarUpdate:     Play sound halfway for {0} event.  Time: {1}  \t Counter = {2} \t\t CurrentBar: {3}", 
														_arrNewsEvents[newsItemPtr].ToShortString(), Time[0].ToShortTimeString(), _dicMultiEventAlertCounter[firstEventTime], CurrentBar));
				}
				//  play sound at event time or just before
				else if(_dicMultiEventAlertCounter.ContainsKey(firstEventTime) && _dicMultiEventAlertCounter[firstEventTime] >= 1 && _dicMultiEventAlertCounter[firstEventTime] < 3
						&& timeDiffMinutes <= 0.5 && timeDiffMinutes >= -0.5)
				{
					PlaySound(fullImminentSoundFileName);
					_dicMultiEventAlertCounter[firstEventTime] = 3;
					
					if (printDebug) Print(String.Format("OnBarUpdate:     Play final sound for {0} event.  Time: {1}  \t Counter = {2} \t\t CurrentBar: {3}", 
														_arrNewsEvents[newsItemPtr].ToShortString(), Time[0].ToShortTimeString(), _dicMultiEventAlertCounter[firstEventTime], CurrentBar));
					playSoundAlert = false;
				}
			}
			#endregion
			
			if (printDebug) { Print("OnBarUpdate:  END -------------------------------  "+CurrentBar);  Print(""); }
		}


		#region  OnRender
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			
			 //Call base.OnRender() to ensure all base behavior is performed
            base.OnRender(chartControl, chartScale);
	
			widestTimeCol 	= 0;				
			widestImpactCol	= 0;				
			widestDescCol	= 0;				
			totalHeight 	= 0;
			
					#region Debugging
//					Vector2	point0	= new Vector2();
//					Vector2	point1	= new Vector2();
//					System.Windows.Media.Brush	br1	= System.Windows.Media.Brushes.Yellow;
//					Vector2	point2	= new Vector2();
//					Vector2	point3	= new Vector2();
//					System.Windows.Media.Brush	br2	= System.Windows.Media.Brushes.White;
					#endregion
			
            // Instatiate a factory, which is required for the next step
            SharpDX.DirectWrite.Factory factory = new SharpDX.DirectWrite.Factory();
				
			SimpleFont headerFont			= new NinjaTrader.Gui.Tools.SimpleFont(defaultFont.FamilySerialize, defaultFont.Size +headerFontSizeIncr) { Bold = true };	

			TextFormat format_ToUse;
			TextFormat format_TitleFont 	= titleFont.ToDirectWriteTextFormat();
			TextFormat format_HeaderFont 	= headerFont.ToDirectWriteTextFormat();
			TextFormat format_DefaultFont 	= defaultFont.ToDirectWriteTextFormat();
			TextFormat format_AlertFont		= alertFont.ToDirectWriteTextFormat();
			
			int maxWidth					= ChartPanel.W; //+ ChartPanel.X;
			RenderTarget.AntialiasMode 		= SharpDX.Direct2D1.AntialiasMode.Aliased;
			
			SharpDX.DirectWrite.TextLayout textLayout_Title = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, TITLE_TXT, format_TitleFont , maxWidth, format_TitleFont.FontSize);
			
			float lineSpaceOffsetAP 		= textLayout_Title.Metrics.Height;			// Gets hight of TITLE_TXT
			rectWidth						= textLayout_Title.Metrics.Width +DESC_PAD;	// Use TITLE_TXT width incase there is No News to list
			
			int rectStartingCoordX 	= (int)(textStartCoordX - offsetBgRectangleX);
			int rectStartingCoordY 	= (int)(textStartCoordY - offsetBgRectangleY);
			float textRoamingCoordY	= textStartCoordY + lineSpaceOffsetAP + Math.Min(5f, lineSpaceOffsetAP/2f);// + lineSpaceOffsetAP;
			totalHeight				= textRoamingCoordY - textStartCoordY;
			
			SharpDX.Vector2 textPoint	= new SharpDX.Vector2(textStartCoordX, textStartCoordY);


			#region Calc Rectangle size
			
			SharpDX.DirectWrite.TextLayout textLayout1;

			for(int i = 0; i < _lstTextLine.Count() ; i++)
			{	 
				format_ToUse = i==0 ? format_HeaderFont : _lstTextLine[i].impactValue >= sendAlertsOnImpactTypeValue ? format_AlertFont : format_DefaultFont;
					
				textLayout1 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, _lstTextLine[i].timeColumn.text, format_ToUse, maxWidth,  _lstTextLine[i].font.TextFormatHeight);
				float f 	= textLayout1.Metrics.Width;
				if (f > widestTimeCol)		widestTimeCol = f;
			
				textLayout1 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, _lstTextLine[i].impactColumn.text, format_ToUse, maxWidth,  _lstTextLine[i].font.TextFormatHeight);
				f 			= textLayout1.Metrics.Width;
				if (f > widestImpactCol)	widestImpactCol = f;

				textLayout1 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, _lstTextLine[i].descColumn.text, format_ToUse, maxWidth,  _lstTextLine[i].font.TextFormatHeight);
				f 			= textLayout1.Metrics.Width;
				if (f > widestDescCol)		widestDescCol = f;
				
				totalHeight += textLayout1.Metrics.Height;
				
			}
			rectWidth 					= Math.Max(rectWidth, (widestTimeCol+TIME_PAD +widestImpactCol+IMPACT_PAD +widestDescCol+DESC_PAD) ); // Use TITLE_TXT incase there is No News to list
			totalHeight					+= offsetBgRectangleY +offsetBgRectangleY;
			float textImpactPointX		= textStartCoordX + widestTimeCol + TIME_PAD;
			float textDescriptionPointX	= textImpactPointX + widestImpactCol + IMPACT_PAD;
			#endregion
				

			// Draw Rect. Backgound first
			if(ShowBackground && rectBackgroundBrushDX!=null && rectBackgroundBrushDX.IsValid(RenderTarget))
			{//																	X1		 ,			Y1		  ,		  Width		,		Height
				var recBackground = new SharpDX.Rectangle(rectStartingCoordX, rectStartingCoordY, (int) rectWidth, (int) totalHeight );
				RenderTarget.FillRectangle(recBackground, rectBackgroundBrushDX);
			}
			// Draw Title text second
			if(colorHeaderBrushDX!=null && colorHeaderBrushDX.IsValid(RenderTarget))
				RenderTarget.DrawTextLayout(textPoint, textLayout_Title, colorHeaderBrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);	

			#region Draw the rest of the text
			if(lastLoadError!=null)
				_lstTextLine[0].timeColumn.text = lastLoadError;

			for(int i = 0; i < _lstTextLine.Count() ; i++)
			{
				format_ToUse = i==0 ? format_HeaderFont : _lstTextLine[i].impactValue >= sendAlertsOnImpactTypeValue ? format_AlertFont : format_DefaultFont;
				
				// TIME_TXT text
				SharpDX.DirectWrite.TextLayout textLayout3 	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, _lstTextLine[i].timeColumn.text, format_ToUse, maxWidth, _lstTextLine[i].font.TextFormatHeight);
				if(_lstTextLine[i].BrushDX!=null && _lstTextLine[i].BrushDX.IsValid(RenderTarget)){
					textPoint 									= new SharpDX.Vector2(textStartCoordX, textRoamingCoordY); //(startPointX2, textStartCoordY);
					RenderTarget.DrawTextLayout(textPoint, textLayout3,  _lstTextLine[i].BrushDX,	SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				
				// IMPACT_TXT text
					textPoint 									= new SharpDX.Vector2(textImpactPointX, textRoamingCoordY); //(startPointX2, textStartCoordY);
					SharpDX.DirectWrite.TextLayout textLayout4	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,  _lstTextLine[i].impactColumn.text, format_ToUse, maxWidth,  _lstTextLine[i].font.TextFormatHeight);
					RenderTarget.DrawTextLayout(textPoint, textLayout4, 	 _lstTextLine[i].BrushDX,	SharpDX.Direct2D1.DrawTextOptions.NoSnap);

				// DESCRIPTION text
					textPoint 									= new SharpDX.Vector2(textDescriptionPointX, textRoamingCoordY); //(startPointX2, textStartCoordY);
					SharpDX.DirectWrite.TextLayout textLayout5	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,  _lstTextLine[i].descColumn.text, format_ToUse, maxWidth,  _lstTextLine[i].font.TextFormatHeight);
					RenderTarget.DrawTextLayout(textPoint, textLayout5,  _lstTextLine[i].BrushDX,	SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				}
				
				textRoamingCoordY += textLayout3.Metrics.Height;
//						point0.Y = point1.Y	= textPoint.Y; //textRoamingCoordY; //textStartCoordY;
//						RenderTarget.DrawLine(point0, point1, br1.ToDxBrush(RenderTarget), 1.0f);
			}
			#endregion


			format_TitleFont.Dispose();	//10/28/2019
			format_HeaderFont.Dispose(); //10/28/2019
			format_DefaultFont.Dispose(); //10/28/2019
			format_AlertFont.Dispose(); //10/28/2019
		}
		#endregion

		private SharpDX.Direct2D1.Brush rectBackgroundBrushDX, colorHeaderBrushDX;
		public override void OnRenderTargetChanged()
		{
			if(ChartControl == null) return;
			#region == OnRenderTargetChanged ==
			if(rectBackgroundBrushDX!=null   && !rectBackgroundBrushDX.IsDisposed)    {rectBackgroundBrushDX.Dispose();rectBackgroundBrushDX=null;}
			if(RenderTarget != null) {rectBackgroundBrushDX     = rectBackgroundColor.ToDxBrush(RenderTarget);}

			if(colorHeaderBrushDX!=null   && !colorHeaderBrushDX.IsDisposed)    {colorHeaderBrushDX.Dispose();colorHeaderBrushDX=null;}
			if(RenderTarget != null) {colorHeaderBrushDX     = colorHeader.ToDxBrush(RenderTarget);}
			
			for(int i = 0; i < _lstTextLine.Count() ; i++)
			{
				if(_lstTextLine[i].BrushDX!=null && !_lstTextLine[i].BrushDX.IsDisposed) {_lstTextLine[i].BrushDX.Dispose(); _lstTextLine[i].BrushDX = null;}
				if(RenderTarget!=null) {_lstTextLine[i].BrushDX = _lstTextLine[i].brush.ToDxBrush(RenderTarget);}
			}

			#endregion
		}

		#region  Functions
		private void ProcessList()
		{
			CreateList();
			
			if (printDebug) Print("ProcessList()...   Time.Now = "+DateTime.Now+"  \t State: "+State+"  \t Time[0]: "+Time0()+" \t\t CurrentBar: "+CurrentBar);

			_lstTextLine 		= new System.Collections.Generic.List<TextLine>();	// Reset the list
			TextLine line 		= new TextLine(defaultFont, colorHeader);
			newsItemPtr = -1;  // this will indicate that there are no pending items at this time.
			
			if (!isNewsLoaded || _arrNewsEvents.IsNullOrEmpty() || _arrNewsEvents.Length <= 0)
			{
				isListBuilt			= false;
				headerFontSizeIncr	= 0.0;
				// Display No News text
				SetNoNewsText();
				
				if (printDebug) Print("ProcessList()... EXITING!!!  No List to process. \t newsItemPtr = "+newsItemPtr);
				return;
			}

			headerFontSizeIncr	= 2.0;
			// add headers
			line.timeColumn 	= new TextColumn(TIME_PAD, TIME_TXT);
			line.impactColumn	= new TextColumn(IMPACT_PAD, IMPACT_TXT);
			line.descColumn 	= USOnlyEvents ? new TextColumn(DESC_PAD, DESC_TXT) : new TextColumn(DESC_PAD, FX_TXT);
			_lstTextLine.Add(line);
			
			// set pointer to the first "pending" news item in the list based on current datetime.
			for(int x = 0; x < _arrNewsEvents.Length ; x++)
			{
				NewsEvent item = _arrNewsEvents[x];
				if (item.DateTimeLocal >= Time[0].AddMinutes(-DisplayNewsMinutesAfter) ) // DateTime.Now )
				{
					newsItemPtr = x;		break;
				}
			}
			
			if (newsItemPtr == -1)
			{
				isListBuilt = false;
				headerFontSizeIncr	= 0.0;
				// Display No News text
				SetNoNewsText();
				
				if (printDebug) Print("Building List... EXITING.  No news events to process. \t\t newsItemPtr: "+newsItemPtr);
				return;
			}
			
//			int lineCnt = 0;
			
			// limit the number of events to be displayed.
			int maxNewsEventLength	= Math.Min(newsItemPtr+MaxNewsItems, _arrNewsEvents.Count());
			bool isFirstItem		= true;
			if (printDebug) Print("  Start For-Loop.   maxNewsEventLength = "+maxNewsEventLength+" \t newsItemPtr = "+newsItemPtr+" \tFirst event: "+_arrNewsEvents[newsItemPtr].DateTimeLocal.ToShortTimeString()+", "+_arrNewsEvents[newsItemPtr].Country+", "+_arrNewsEvents[newsItemPtr].Title );
			// Assemble list to be displayed.
			for (int x = newsItemPtr; x < maxNewsEventLength; x++) // x < _arrNewsEvents.Length; x++)
			{
//				lineCnt++;
//				// limit the number of pending events to be displayed.
//				if (lineCnt > MaxNewsItems) break;
				
				NewsEvent item = _arrNewsEvents[x];
				
				int valueOut = -1;
				string mEACDateTimeLocalValueOrNull = (_dicMultiEventAlertCounter.TryGetValue(item.DateTimeLocal, out valueOut) ? valueOut.ToString() : "No entry");
//				string mEACDateTimeLocalValueOrNull = (_dicMultiEventAlertCounter.ContainsKey(item.DateTimeLocal)? _dicMultiEventAlertCounter[item.DateTimeLocal].ToString():"No entry");


				#region Format .DateTimeLoca for display
//					if (printDebug) Print("    Format .DateTimeLoca for display");
				string tempTime = "";
				if (NewsPeriod != EnumNewsEconNewsPeriod.Todays_News)
					tempTime = item.DateTimeLocal.ToString("M/d "); //		tempTime = item.DateTimeLocal.ToString("MM/dd ") + tempTime;

				if (Use24HrTimeFormat)
					tempTime += item.DateTimeLocal.ToString("HH:mm", ffDateTimeCulture);
				else 
					tempTime += item.DateTimeLocal.ToString("hh:mm", ffDateTimeCulture) + item.DateTimeLocal.ToString("tt", ffDateTimeCulture).ToLower();
//					tempTime += item.DateTimeLocal.ToString("hh:mm tt", ffDateTimeCulture);
				#endregion

				
				#region  Set Priority & Displayed Text Color
//					if (printDebug) Print("    Set Priority");
				
				Priority alertPriority 					= Priority.Low;
//				int alertImpact							= 0;
				System.Windows.Media.Brush  lineBrush	= colorImpactOther;
				
				if (item.ImpactValue == 3) // (item.Impact.ToUpper() == "HIGH")
				{
					lineBrush		= colorImpactHigh;
					alertPriority	= Priority.High;
//					alertImpact 	= 3;
				} 
				else if (item.ImpactValue == 2) // (item.Impact.ToUpper() == "MEDIUM")
				{
					lineBrush		= colorImpactMed;
					alertPriority	= Priority.Medium;
//					alertImpact 	= 2;
				} 
				else if (item.ImpactValue == 1) // (item.Impact.ToUpper() == "LOW")
				{
					lineBrush		= colorImpactLow;
//					alertImpact 	= 1;
				}
				#endregion


				#region  Process Alerts

				if (printDebug) Print("    Process Alerts. \t x = "+x);
				
				#region  isFirstItem
				if(isFirstItem)
				{
					firstEventTime	= item.DateTimeLocal;
					if (printDebug) Print("      1.0 Set _dicMultiEventAlertCounter \t firstEventTime = "+firstEventTime);
					// Update sound counter to different event time
												//	Not Null  &  Event time has changed
					if(!_dicMultiEventAlertCounter.IsNullOrEmpty() && !_dicMultiEventAlertCounter.ContainsKey(firstEventTime))		
					{
						_dicMultiEventAlertCounter.Clear();
						_dicMultiEventAlertCounter.Add(firstEventTime, 0);
					}//							Is Null, set an event time
					else if(_dicMultiEventAlertCounter.IsNullOrEmpty())
							_dicMultiEventAlertCounter.Add(firstEventTime, 0);
					else
					{
						if (printDebug) Print("      1.1 DID NOT update _dicMultiEventAlertCounter");
						if (printDebug) Print("         .IsNullOrEmpty() = "+_dicMultiEventAlertCounter.IsNullOrEmpty()+" \t\t\t\t\t\t\t .Count() = "+(_dicMultiEventAlertCounter.Count() ) );
					}
					
					mEACDateTimeLocalValueOrNull = (_dicMultiEventAlertCounter.ContainsKey(item.DateTimeLocal)? _dicMultiEventAlertCounter[item.DateTimeLocal].ToString():"No entry");
//					if (printDebug) Print("    Updated _dicMultiEventAlertCounter...");
					if (printDebug) Print("      1.2 _dicMultiEventAlertCounter[ "+firstEventTime.ToShortTimeString()+" ] = "+(mEACDateTimeLocalValueOrNull)+" \t\t .Count() = "+(_dicMultiEventAlertCounter.Count() ) );
				}
				#endregion


				double timeDiff 		= (item.DateTimeLocal - Time[0]).TotalMinutes; // Math.Min(0.0, (item.DateTimeLocal - Time[0]).TotalMinutes);
				
				SimpleFont tempFont	= defaultFont;
				if (printDebug) Print("      2.0 timeDiff: "+timeDiff+" \t AlertWarningMinutesBefore = "+AlertWarningMinutesBefore+" \t firstEventTime: "+firstEventTime+" \t .ImpactValue: "+item.ImpactValue );

				
				#region  Sound & Alerts Log
				if (isRealtime && item.ImpactValue >= sendAlertsOnImpactTypeValue && timeDiff <= AlertWarningMinutesBefore )
				{
					if (printDebug) Print("        Send Alert.");
					
					tempFont	= alertFont;
					string id	= "", message = "";
					
					if(USOnlyEvents)
					{
						id = "EconNewsAlert_"+tempTime+item.Title;
						message = String.Format( "News Alert pending at {0},  {1}", item.DateTimeLocal.ToShortTimeString(), item.Title);
					}
					else					
					{
						id = "EconNewsAlert_"+tempTime+item.Country+item.Title;
						message = String.Format( "News Alert pending at {0},  {1}, {2}", item.DateTimeLocal.ToShortTimeString(), item.Country, item.Title);
					}
						
			    	Alert( id,  alertPriority, message,	fullPendingSoundFileName,  alertRearmSeconds,  colorAlertLogBg,  colorAlertLogText);
					
					if (printDebug) Print("          3.0 Test incrementing _dicMultiEventAlertCounter[ "+item.DateTimeLocal.ToShortTimeString()+" ] = "+(mEACDateTimeLocalValueOrNull) );
					// Update sound counter value to 1
					if(_dicMultiEventAlertCounter.ContainsKey(item.DateTimeLocal) && _dicMultiEventAlertCounter[item.DateTimeLocal] == 0)  //  item.DateTimeLocal == _arrNewsEvents[x+1].DateTimeLocal && 
					{
						_dicMultiEventAlertCounter[item.DateTimeLocal] = 1;
						if (printDebug) Print("          3.1 _dicMultiEventAlertCounter[ "+item.DateTimeLocal.ToShortTimeString()+" ] updated to: "+_dicMultiEventAlertCounter[item.DateTimeLocal]);
					}
					else if (printDebug) Print("          3.2 _dicMultiEventAlertCounter Value not changed."); // \t mEAIdx = "+mEAIdx);
					
					mEACDateTimeLocalValueOrNull = (_dicMultiEventAlertCounter.TryGetValue(item.DateTimeLocal, out valueOut) ? valueOut.ToString() : "No entry");
					//mEACDateTimeLocalValueOrNull = (_dicMultiEventAlertCounter.ContainsKey(item.DateTimeLocal)? _dicMultiEventAlertCounter[item.DateTimeLocal].ToString():"No entry");

					playSoundAlert	= true;
					if (printDebug) Print("       END Alert() \t _dicMultiEventAlertCounter[ "+(item.DateTimeLocal.ToShortTimeString())+" ] Value = "+mEACDateTimeLocalValueOrNull+" \t playSoundAlert: "+playSoundAlert);
				}
				else if (printDebug) Print("       Alert() SKIPPED \t isRealtime: "+isRealtime.ToString().ToUpper()+" \t playSoundAlert: "+playSoundAlert);
				#endregion  -------------

				
				#endregion  -------------


				#region  Collect events for display
				
				if(timeDiff <= 0 && setActiveColor)
					lineBrush		= colorActiveEvent;
				
				line 				= new TextLine(tempFont, lineBrush);
				line.timeColumn 	= new TextColumn(TIME_PAD, tempTime);
				line.impactColumn	= new TextColumn(IMPACT_PAD, item.Impact);
				line.impactValue	= item.ImpactValue;
				
				string templine = null;
				if (item.Previous.Trim().Length == 0 && item.Forecast.Trim().Length == 0)
					templine = string.Format("{0}{1}", USOnlyEvents?"":item.Country+": " , item.Title);
				else 
					templine = string.Format("{0}{1} ({2}/{3})", USOnlyEvents?"":item.Country+": " , item.Title, item.Previous, item.Forecast  );
				
				line.descColumn 	= new TextColumn(DESC_PAD, templine);
				_lstTextLine.Add(line);
				#endregion
				
				isFirstItem		= false;
				if (printDebug)	Print("    END Process Alerts --  "+item.ToShortString()+"\t\t multiEAC[ "+firstEventTime.ToShortTimeString()+" ] = "+(_dicMultiEventAlertCounter.ContainsKey(firstEventTime)? _dicMultiEventAlertCounter[firstEventTime].ToString():"No Key") ); //+" \t\t firstEventTime: "+firstEventTime);
			}
			if (printDebug) { Print("  Exit For-Loop... \t firstEventTime: "+firstEventTime);	}
			
			lastNewsPtr	= newsItemPtr;
			isListBuilt	= true;
//			if (printDebug)	{ Print("Building List... DONE. \t CurrentBar: "+CurrentBar);	}//	Print("");	}
		}


		private void CreateList()
		{
			if (newsDebug) Print("CreateList()...   _arrAllEvents = "+_arrAllEvents.Count()+", \t Time.Now: "+DateTime.Now+"  \t State: "+State+"  \t Time[0]: "+Time0()+" \t\t CurrentBar: "+CurrentBar);
			if (!isNewsLoaded)
			{
				if (printDebug) Print("CreateList()... EXITING!!!  No List to process. \t CurrentBar: "+CurrentBar);
				return;
			}
			
			
			ArrayList tempDisplayList	= new ArrayList();
//			ArrayList tempAlertList		= new ArrayList();
				
			// filter events based on settings...
			DateTime startTime			= isRealtime ? DateTime.Now : Time[0]; //  DateTime.Now.AddMinutes(-1) : Time[0].AddMinutes(-1);
			DateTime endTime			= startTime.AddHours(24);
			DateTime displayStartTime	= startTime.AddMinutes(-DisplayNewsMinutesAfter);
			
			if(newsListDebug) Print("   (isRealtime || IsLastHistoricalBar() ) = ( "+isRealtime+" || "+IsLastHistoricalBar()+" )" );
			if(newsListDebug) Print("   && _arrAllEvents[i].DateTimeLocal >= displayStartTime = "+_arrAllEvents[0].DateTimeLocal+" >= "+displayStartTime+" \t = "+(_arrAllEvents[0].DateTimeLocal >= displayStartTime) );
			if(newsListDebug) Print("     || (NewsPeriod == EnumNewsEconNewsPeriod.Next_24Hrs && _arrAllEvents[i].DateTimeLocal < endTime) = ( "+(NewsPeriod == EnumNewsEconNewsPeriod.Next_24Hrs)+" && "+_arrAllEvents[0].DateTimeLocal+" < "+endTime+" \t = "+(NewsPeriod == EnumNewsEconNewsPeriod.Next_24Hrs && _arrAllEvents[0].DateTimeLocal < endTime) );
			
			for(int i =0; i < _arrAllEvents.Count(); i++)
			{
				// filter news events based on various property settings...
				if( ( (isRealtime || IsLastHistoricalBar())
						&& _arrAllEvents[i].DateTimeLocal >= displayStartTime 
						&& (NewsPeriod == EnumNewsEconNewsPeriod.All_Week																// Show All events.
							|| (NewsPeriod == EnumNewsEconNewsPeriod.Todays_News && _arrAllEvents[i].DateTimeLocal.Date < endTime.Date)	// NewsPeriod clips events at midnight.
							|| (NewsPeriod == EnumNewsEconNewsPeriod.Next_24Hrs && _arrAllEvents[i].DateTimeLocal < endTime) )			// NewsPeriod clips events after 24hrs.
					)
					|| (isHistorical && _arrAllEvents[i].DateTimeLocal >= startTime && _arrAllEvents[i].DateTimeLocal < endTime) )
				{
					if(newsListDebug) Print("   Adding to tempDisplayList,  "+_arrAllEvents[i].ToShortString() );
					tempDisplayList.Add(_arrAllEvents[i]);
					
				}
				else
				{				}
			}
			if(newsListDebug) Print("   List size:  tempDisplayList = "+tempDisplayList.Count); // +" \t tempAlertList = "+tempAlertList.Count);
			
			_arrNewsEvents = (NewsEvent[])tempDisplayList.ToArray(typeof(NewsEvent));
//			_arrNewsEvents = (NewsEvent[])tempAlertList.ToArray(typeof(NewsEvent));
			if (newsDebug && advDebug) Print("CreateList()  DONE."); //     Time[0]: "+Time0()+" \t CurrentBar: "+CurrentBar);
		}


		private void LoadNews(int line)
		{
			lastLoadError = null;
			var newsDoc = new XmlDocument();
			try 
			{
				if (newsDebug)
				{
					Print("\r\nLoadNews()...   Time.Now = "+DateTime.Now+"  \t State: "+State+"  \t Time[0]: "+Time0()+" \t\t CurrentBar: "+CurrentBar);
				}

		        string filePath = "";
				string folder = NinjaTrader.Core.Globals.UserDataDir;
				try{
					var dirCustom = new System.IO.DirectoryInfo(folder);
					var filCustom = dirCustom.GetFiles("xmlNews*.xml");
					var fileDate = DateTime.Now.AddDays(-10);
					foreach(var xfile in filCustom){
						if(xfile.CreationTime > fileDate && xfile.CreationTime > DateTime.Now.AddHours(-NewsRefeshInterval)){
							fileDate = xfile.CreationTime;
							filePath = xfile.FullName;
							Print("xml File found: "+filePath+"  creation: "+xfile.CreationTime.ToString());
						}
					}
				}catch{}

				// add a random query string to defeat server side caching.
				string urltweak = ffNewsUrl; //  IS IGNORED BY WEBSITE >> + "?x=" + Convert.ToString(DateTime.Now.Ticks);
				if(filePath.Length>0){
					if (newsDebug) Print("Loading news from file: " + filePath);
					newsDoc.Load(filePath);
				}else{
					if (newsDebug) Print("Loading news from URL: " + urltweak);

					HttpWebRequest newsReq = (HttpWebRequest)HttpWebRequest.Create(urltweak);
					newsReq.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Reload);

					// fetch the xml doc from the web server
					using (HttpWebResponse newsResp = (HttpWebResponse)newsReq.GetResponse())
					{
						// check that we got a valid reponse
						if (newsResp != null && newsResp.StatusCode == HttpStatusCode.OK){
							// read the response stream into and xml document
							Stream receiveStream = newsResp.GetResponseStream();
							Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
							StreamReader readStream = new StreamReader( receiveStream, encode );
							
							string xmlString = readStream.ReadToEnd();
											if (newsXMLListDebug) Print("  RAW http response: " + xmlString);
							
							newsDoc.LoadXml(xmlString);
	
							if (newsDebug) Print("  XML news event node count: " + newsDoc.DocumentElement.ChildNodes.Count );
						}
						// handle unexpected scenarios...
						else if (newsResp == null) 
								throw new Exception("   Web response was null.");
							else 
								throw new Exception("   Web response status code = " + newsResp.StatusCode.ToString());
						if(newsDoc != null){
							try{
								var dirCustom = new System.IO.DirectoryInfo(folder);
								var filCustom = dirCustom.GetFiles("xmlNews*.xml");
								Print("----------------------------------------");
								foreach(var xfile in filCustom) {
									Print("Deleting file: "+xfile.FullName);
									System.IO.File.Delete(xfile.FullName);
								}
							}catch{}
					        filePath = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, $"xmlNews {DateTime.Now.ToBinary()}.xml");
					        try
					        {
					            // Save the XmlDocument to the specified file
					            using (StreamWriter writer = new StreamWriter(filePath, false))
					                newsDoc.Save(writer);
					        }
					        catch (Exception ex)
					        {
								Print($"An error occurred: {ex.Message}");
					        }
						}
					}
				}
				// build collection of events
				ArrayList list	= new ArrayList();
				int itemId 		= 0;
				// filter events based on settings...
				DateTime startTime	= DateTime.Now.AddMinutes(-1);
				DateTime endTime	= startTime.AddDays(1);

				#region  CONVERT XML data to ArrayList
						//(XmlNode xmlNode in newsDoc.DocumentElement.ChildNodes)
						//foreach(XmlNode xmlNode in newsDoc.DocumentElement.ChildNodes)
				for(int i =1; i < newsDoc.DocumentElement.ChildNodes.Count; i++)
				{
					NewsEvent newsEvent = new NewsEvent();
					newsEvent.Time 		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("time").InnerText;

					if (string.IsNullOrEmpty(newsEvent.Time)) continue;  // ignore tentative events!

					newsEvent.Date 		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("date").InnerText;
								
					// assembly and convert event date/time to local time.
//							if (printDebug) Print(string.Format("About to parse \t\t Date: {0}, Time: {1}", newsEvent.Date, newsEvent.Time));
					DateTime dtLondon		= DateTime.SpecifyKind(DateTime.Parse( newsEvent.Date + " " + newsEvent.Time, ffDateTimeCulture), DateTimeKind.Utc);
					newsEvent.DateTimeLocal = dtLondon.ToLocalTime();
					if (newsXMLListDebug) Print(String.Format("    Succesfully parsed \t DateTime: {0}  to local time: {1}.", dtLondon, newsEvent.DateTimeLocal) );

					// filter news events based on various property settings...
						newsEvent.ID 		= ++itemId;
//								if (printDebug) Print("   Filter news events.  ("+newsEvent.DateTimeLocal+" >= "+);
					
						newsEvent.Country		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("country").InnerText;
						if (USOnlyEvents && newsEvent.Country != "USD") continue;

						newsEvent.Impact		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("impact").InnerText;
						newsEvent.ImpactValue	= GetImpactValue(newsEvent.Impact);
						if (newsXMLListDebug) Print(String.Format("    displayEventImpactValue = {0}  >  .ImpactValue = {1}.", displayEventImpactValue, newsEvent.ImpactValue) );
						if (displayEventImpactValue > newsEvent.ImpactValue) continue;

						newsEvent.Forecast		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("forecast").InnerText;
						newsEvent.Previous		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("previous").InnerText;
						newsEvent.Title 		= newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("title").InnerText;

						list.Add(newsEvent);
						if (newsListDebug) Print("      Added: "+ newsEvent.ToString());
				}
				#endregion  ----------

				_arrAllEvents = (NewsEvent[])list.ToArray(typeof(NewsEvent));
				if (newsDebug) Print("  Added a total of "+_arrAllEvents.Count()+" events to _arrAllEvents.");
			} 
			catch (Exception ex)
			{
				Print("LoadNews error in EconNews2b.  "+ex.ToString());
				Log("LoadNews error in EconNews2b.  "+ex.ToString(),LogLevel.Information);
				lastLoadError = ex.Message;
				isNewsLoaded  = false;
				if (printDebug) Print("LoadNews()  ERROR.");
				return;
			}
			
			isNewsLoaded = true;
			if (newsDebug) Print("LoadNews()  DONE.     DateTime.Now: "+DateTime.Now+" \t CurrentBar: "+CurrentBar);
		}
		#endregion


		#region Helper functions
		private static DateTime ParseDateFromString(string s)
		{
			//Used to parse news columns
				string[] formats= { "yyyyMMdd","MM/dd/yyyy","MM-dd-yyyy","M-dd-yyyy", "M-d-yyyy", "MM-d-yyyy",
				"yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd",
				"M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", 	"MM/dd/yyyy hh:mm:ss tt", 	"yyyy-MM-dd hh:mm:ss" ,	
				//Got from news parser, probably good to have just added them.				
				"M/d/yyyy","M/d/yy","MM/dd/yy","MM/dd/yyyy","yy/MM/dd","yyyy-MM-dd",
				"dd-MMM-yy","dddd, MMMM d, yyyy","dddd, MMMM dd, yyyy","MMMM dd, yyyy","dddd, dd MMMM, yyyy","dd MMMM, yyyy",
				"dddd, MMMM d, yyyy h:mm tt","dddd, MMMM d, yyyy hh:mm tt","dddd, MMMM d, yyyy H:mm",
				"dddd, MMMM d, yyyy HH:mm","dddd, MMMM dd, yyyy h:mm tt","dddd, MMMM dd, yyyy hh:mm tt",
				"dddd, MMMM dd, yyyy H:mm","dddd, MMMM dd, yyyy HH:mm","MMMM dd, yyyy h:mm tt",
				"MMMM dd, yyyy hh:mm tt","MMMM dd, yyyy H:mm","MMMM dd, yyyy HH:mm","dddd, dd MMMM, yyyy h:mm tt",
				"dddd, dd MMMM, yyyy hh:mm tt","dddd, dd MMMM, yyyy H:mm","dddd, dd MMMM, yyyy HH:mm","dd MMMM, yyyy h:mm tt",
				"dd MMMM, yyyy hh:mm tt","dd MMMM, yyyy H:mm","dd MMMM, yyyy HH:mm","dddd, MMMM d, yyyy h:mm:ss tt","dddd, MMMM d, yyyy hh:mm:ss tt",
				"dddd, MMMM d, yyyy H:mm:ss","dddd, MMMM d, yyyy HH:mm:ss","dddd, MMMM dd, yyyy h:mm:ss tt","dddd, MMMM dd, yyyy hh:mm:ss tt",
				"dddd, MMMM dd, yyyy H:mm:ss","dddd, MMMM dd, yyyy HH:mm:ss","MMMM dd, yyyy h:mm:ss tt","MMMM dd, yyyy hh:mm:ss tt",
				"MMMM dd, yyyy H:mm:ss","MMMM dd, yyyy HH:mm:ss","dddd, dd MMMM, yyyy h:mm:ss tt",
				"dddd, dd MMMM, yyyy hh:mm:ss tt","dddd, dd MMMM, yyyy H:mm:ss","dddd, dd MMMM, yyyy HH:mm:ss","dd MMMM, yyyy h:mm:ss tt",
				"dd MMMM, yyyy hh:mm:ss tt","dd MMMM, yyyy H:mm:ss","dd MMMM, yyyy HH:mm:ss","M/d/yyyy h:mm tt","M/d/yyyy hh:mm tt","M/d/yyyy H:mm",
				"M/d/yyyy HH:mm","M/d/yy h:mm tt","M/d/yy hh:mm tt","M/d/yy H:mm","M/d/yy HH:mm","MM/dd/yy h:mm tt",
				"MM/dd/yy hh:mm tt","MM/dd/yy H:mm","MM/dd/yy HH:mm","MM/dd/yyyy h:mm tt",
				"MM/dd/yyyy hh:mm tt","MM/dd/yyyy H:mm","MM/dd/yyyy HH:mm","yy/MM/dd h:mm tt",
				"yy/MM/dd hh:mm tt","yy/MM/dd H:mm","yy/MM/dd HH:mm","yyyy-MM-dd h:mm tt","yyyy-MM-dd hh:mm tt",
				"yyyy-MM-dd H:mm","yyyy-MM-dd HH:mm","dd-MMM-yy h:mm tt","dd-MMM-yy hh:mm tt","dd-MMM-yy H:mm","dd-MMM-yy HH:mm",
				"M/d/yyyy h:mm:ss tt","M/d/yyyy hh:mm:ss tt","M/d/yyyy H:mm:ss",
				"M/d/yyyy HH:mm:ss","M/d/yy h:mm:ss tt","M/d/yy hh:mm:ss tt","M/d/yy H:mm:ss","M/d/yy HH:mm:ss",
				"MM/dd/yy h:mm:ss tt","MM/dd/yy hh:mm:ss tt","MM/dd/yy H:mm:ss","MM/dd/yy HH:mm:ss","MM/dd/yyyy h:mm:ss tt",
				"MM/dd/yyyy hh:mm:ss tt","MM/dd/yyyy H:mm:ss","MM/dd/yyyy HH:mm:ss","yy/MM/dd h:mm:ss tt",
				"yy/MM/dd hh:mm:ss tt","yy/MM/dd H:mm:ss","yy/MM/dd HH:mm:ss","yyyy-MM-dd h:mm:ss tt","yyyy-MM-dd hh:mm:ss tt",
				"yyyy-MM-dd H:mm:ss","yyyy-MM-dd HH:mm:ss","dd-MMM-yy h:mm:ss tt","dd-MMM-yy hh:mm:ss tt","dd-MMM-yy H:mm:ss",
				"dd-MMM-yy HH:mm:ss","MMMM dd","MMMM dd","yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK","yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
				"ddd, dd MMM yyyy HH':'mm':'ss 'GMT'","ddd, dd MMM yyyy HH':'mm':'ss 'GMT'","yyyy'-'MM'-'dd'T'HH':'mm':'ss",
				"h:mm tt","hh:mm tt","H:mm","HH:mm","h:mm:ss tt","hh:mm:ss tt","H:mm:ss","HH:mm:ss",
				"yyyy'-'MM'-'dd HH':'mm':'ss'Z'","dddd, MMMM d, yyyy h:mm:ss tt","dddd, MMMM d, yyyy hh:mm:ss tt","dddd, MMMM d, yyyy H:mm:ss",
				"dddd, MMMM d, yyyy HH:mm:ss","dddd, MMMM dd, yyyy h:mm:ss tt",
				"dddd, MMMM dd, yyyy hh:mm:ss tt","dddd, MMMM dd, yyyy H:mm:ss","dddd, MMMM dd, yyyy HH:mm:ss","MMMM dd, yyyy h:mm:ss tt",
				"MMMM dd, yyyy hh:mm:ss tt","MMMM dd, yyyy H:mm:ss","MMMM dd, yyyy HH:mm:ss","dddd, dd MMMM, yyyy h:mm:ss tt",
				"dddd, dd MMMM, yyyy hh:mm:ss tt","dddd, dd MMMM, yyyy H:mm:ss", "dddd, dd MMMM, yyyy HH:mm:ss","dd MMMM, yyyy h:mm:ss tt",
				"dd MMMM, yyyy hh:mm:ss tt","dd MMMM, yyyy H:mm:ss", 
				"dd MMMM, yyyy HH:mm:ss", "MMMM yyyy", "MMMM, yyyy", "MMMM yyyy","MMMM, yyyy"};
			
				
				DateTime dateFortmated;					

				//Having a try catch in here was changing our results!!!  Cause I had to return something so I returned datetime.price.
				dateFortmated = DateTime.ParseExact(s, formats, new CultureInfo("en-GB"), DateTimeStyles.None);
				return dateFortmated;		
		}


		private void SetNoNewsText()
		{
			if (printDebug) Print("   SetNoNewsText() \t DateTime.Now: "+DateTime.Now);
			
			_lstTextLine		= new System.Collections.Generic.List<TextLine>();	// Reset the list
			TextLine line 		= new TextLine(defaultFont, colorHeader);
			
			// Display No News text
			if(lastLoadError.Length>0)
				line.timeColumn = new TextColumn(0f, lastLoadError);
			else
				line.timeColumn = new TextColumn(0f, NO_NEWS_TXT);
			line.impactColumn	= new TextColumn(0f, "");
			line.descColumn 	= new TextColumn(0f, "");
			_lstTextLine.Add(line);
		}
		
		
//		private String Time0()
//		{	return CurrentBar > -1 ? Time[0] : "Invalid.    ";	}
		
		private DateTime Time0()
		{	return CurrentBar > -1 ? Time[0] : DateTime.MinValue;	}
		
		
		private bool IsLastHistoricalBar()
		{
			if(Calculate == Calculate.OnBarClose)	// When CalculateOnBarClose = true:
				return (isHistorical && Count-2 == CurrentBar); //Is last bar
			else
				return (isHistorical && Count-1 == CurrentBar);	//is last bar.
		}
		
		
		private int	GetImpactValue(String _impact)
		{
				if (_impact.ToUpper() == "LOW")
				{	return 1;				} 
				else if (_impact.ToUpper() == "MEDIUM")
				{	return 2;				} 
				else if (_impact.ToUpper() == "HIGH")
				{	return 3;				}
				else
					return 0;
		}

		
		public override string DisplayName
		{	get 
			{
				if(isDataLoaded)	return string.Format("{0}({1}{2})", indiName, USOnlyEvents? "US Only, " : "", NewsPeriod.ToString());
				else				return indiName; 
		}	}
		#endregion


		
		#region Properties
		
		#region News Filters
		[NinjaScriptProperty]
		[Display(Name="News Period to Show:", Description="How far into the future to show news events?",					GroupName = "News Filters", Order=0)]
		public EnumNewsEconNewsPeriod NewsPeriod
		{ get; set; }


		[NinjaScriptProperty]
		[Display(Name="Max Items to Show",Description="Max number of news events to display?",								GroupName = "News Filters", Order=2)]
		public int MaxNewsItems
		{ get; set; }

	 
		[NinjaScriptProperty]
		[Display(Name="Only US Events", Description="Show only US news events?", 											GroupName = "News Filters", Order=4)]
		public bool USOnlyEvents
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Display News Priority for", Description="Show Low priority news events?", 									GroupName = "News Filters", Order=6)]
		public EnumNewsEconEventImpact DisplayEventImpact
//		public bool DisplayEventImpact
		{ get; set; }
		
		
		[Range(1, 24), NinjaScriptProperty]
		[Display(Name="News Refesh Interval (Hrs)",Description="How often to download news events from website in hours.",	GroupName = "News Filters", Order=8)]
		public int NewsRefeshInterval
		{ get; set; }
		
        #endregion

// ----- Alerts ----------------------------------------------------------------------------------------------------------
		#region Alerts
 		[NinjaScriptProperty]
		[Display(Name="Send Alert for this Impact Level", Description="Play sound & show event in the Alerts Log window for events of this Impact level or higher.", 			GroupName="Alerts", Order= 0)]
		public EnumNewsEconAlertImpact SendAlertsOnImpactType
		{ get; set; }


		[Range(0, maxAlertWarningMinutesBefore), NinjaScriptProperty]
		[Display(Name="Notify X minutes before", Description="Number of minutes to warn before the news event time.", 																		GroupName="Alerts", Order= 2)]
		public int AlertWarningMinutesBefore
		{ get; set; }
		
	
		[NinjaScriptProperty]
		[Display(Name="Pending Alert sound file", Description="Sound file (.wav) to play for pending event notification.", 																	GroupName="Alerts", Order= 4)]
		public string PendingSoundFileNameOnly
		{ get; set; }


		[Range(0, 300), NinjaScriptProperty]
		[Display(Name="Display News for X minutes afterwards", Description="Number of minutes to keep displaying the news title on the chart after the event has passed.",		GroupName="Alerts", Order= 6)]
		public int DisplayNewsMinutesAfter
		{ get; set; }
		
        #endregion

// ----- Visuals ----------------------------------------------------------------------------------------------------------
		#region Visuals
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="High Priority color", Description = "High Priority text color", 								GroupName = "Visuals", Order = 30)]
		public    System.Windows.Media.Brush HighPriorityColor
		{
			get{return colorImpactHigh;}
			set{colorImpactHigh = value;}
		}
		[Browsable(false)]
		public string HighPriorityColorSerialize
		{
			get{return Serialize.BrushToString(colorImpactHigh);}
			set{colorImpactHigh = Serialize.StringToBrush(value);}
		}
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Medium Priority color", Description = "Medium Priority text color", 							GroupName = "Visuals", Order = 32)]
		public    System.Windows.Media.Brush MediumPriorityColor
		{
			get{return colorImpactMed;}
			set{colorImpactMed = value;}
		}
		[Browsable(false)]
		public string MediumPriorityColorSerialize
		{
			get{return Serialize.BrushToString(colorImpactMed);}
			set{colorImpactMed = Serialize.StringToBrush(value);}
		}
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Low Priority color", Description = "Low Priority text color", 								GroupName = "Visuals", Order = 34)]
		public    System.Windows.Media.Brush LowPriorityColor
		{
			get{return colorImpactLow;}
			set{colorImpactLow = value;}
		}
		[Browsable(false)]
		public string LowPriorityColorSerialize
		{
			get{return Serialize.BrushToString(colorImpactLow);}
			set{colorImpactLow = Serialize.StringToBrush(value);}
		}
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Active Event color", Description = "Set a special color for active ongoing event(s).  Use Transparent to not change color.", GroupName = "Visuals", Order = 36)]
		public    System.Windows.Media.Brush ActiveEventColor
		{
			get{return colorActiveEvent;}
			set{colorActiveEvent = value;}
		}
		[Browsable(false)]
		public string ActiveEventColorSerialize
		{
			get{return Serialize.BrushToString(colorActiveEvent);}
			set{colorActiveEvent = Serialize.StringToBrush(value);}
		}
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Header color", Description = "Set the text color for the column headers.", 							GroupName = "Visuals", Order = 38)]
		public    System.Windows.Media.Brush HeaderColor
		{
			get{return colorHeader;}
			set{colorHeader = value;}
		}
		[Browsable(false)]
		public string HeaderColorSerialize
		{
			get{return Serialize.BrushToString(colorHeader);}
			set{colorHeader = Serialize.StringToBrush(value);}
		}
		
		
		[NinjaScriptProperty]
		[Display(Name="Apply Background", Description="Place a background under the text to block the chart behind.", 		GroupName="Visuals", Order=40)]
		public bool ShowBackground
		{ get; set; }
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Background color", Description = "Choose the Background color", 										GroupName = "Visuals", Order = 42)]
		public    System.Windows.Media.Brush BackgroundColor
		{
			get{return colorBackground;}
			set{colorBackground = value;}
		}
		[Browsable(false)]
		public string BackgroundColorSerialize
		{
			get{return Serialize.BrushToString(colorBackground);}
			set{colorBackground = Serialize.StringToBrush(value);}
		}


		[Range(1, 100), NinjaScriptProperty]
		[Display(Name="Background Opacity %", Description="1 = Nearly transparent.  100 = Opaque//solid.", 					GroupName="Visuals", Order=44)]
		public int BackgroundOpacity
		{ get; set; }


		[NinjaScriptProperty]
		[Display(Name="Use 24hr format", Description="Display time as, AM/PM (off) or 24Hr military (on).", 				GroupName="Visuals", Order=46)]
		public bool Use24HrTimeFormat
		{ get; set; }
		
		
// ---------------------------------------------------------------------------------------------------------------
//		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Default Event Font", Description = "Default Font size & style.", 						GroupName = "Visuals", Order = 50)]
		public SimpleFont DefaultFont
		{
			get{return defaultFont;}
			set{defaultFont = value;}
		}
		
//		[Browsable(false)]	
//		public string DefaultFontSerialize 
//		{
//		    get { return defaultFont.FamilySerialize;}
//		    set { defaultFont = new SimpleFont(value, DefaultFontSizeSerialize); }
//		}
		
//		[Browsable(false)]	
//		public double DefaultFontSizeSerialize 
//		{
//		    get { return DefaultFont.Size;}
//		    set {DefaultFont.Size = value; }
//		}
		

//		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="Alert Event Font", Description = "Font to display events that send Alerts, as selected in the Alerts section.", GroupName = "Visuals", Order = 52)]
		public SimpleFont AlertFont
		{
			get{return alertFont;}
			set{alertFont = value;}
		}
		
//		[Browsable(false)]	
//		public string WarningFontSerialize 
//		{
//		    get { return alertFont.FamilySerialize;}
//		    set { alertFont = new SimpleFont(value, WarningFontSizeSerialize); }
//		}

//		[Browsable(false)]	
//		public double WarningFontSizeSerialize 
//		{
//		    get { return AlertFont.Size;}
//		    set {AlertFont.Size = value; }
//		}
		
        #endregion
		
// ----- Debug ----------------------------------------------------------------------------------------------------------
		[NinjaScriptProperty]
		[Display(Name="Enable Debug messaging", Description="Send Debug info to the Output window.", GroupName="Debug", Order=100)]
		public bool Debug
		{
			get{return debug;}
			set{debug = value;}
		}
		
        #endregion



		#region Text Parsing classes
		private class TextColumn 
		{			
			public TextColumn(float padding, string text)
			{
				this.padding 	= padding;
				this.text		= text;
			}
			public float padding;
			public string text;	
		}

		private class TextLine 
		{
			public TextLine(SimpleFont  font,    System.Windows.Media.Brush brush)
			{	
				this.font = font;
				this.brush = brush.Clone();
				this.brush.Freeze();
			}
			public TextColumn	timeColumn;
			public TextColumn	impactColumn;
			public TextColumn	descColumn;
			public SimpleFont	font;
			public System.Windows.Media.Brush brush;
			public SharpDX.Direct2D1.Brush BrushDX = null;
			public int			impactValue		= 0;
		}

//		public class NewsEvent : ICloneable
		private class NewsEvent : ICloneable
		{
			public int ID;
			public string	Title;
			public string	Country;
			public string	Date;
			public string	Time;
			public string	Impact;
			public int		ImpactValue;
			public string	Forecast;
			public string	Previous;
			[XmlIgnore()]
			public DateTime DateTimeLocal;
//			[XmlIgnore()]
//			public int AlertCount = 0;
			
			public override string ToString()
			{
				return string.Format("ID: {0}  DTLocal: {8} Imp: {5} FX: {2}, Title: {1}   D: {3}, T: {4}  Prev: {7}, Forecast: {6}, ImpVal: {7}",
									  String.Format("{0,-3}", ID+","),  String.Format("{0,-36}", Title+","),  Country,  Date, String.Format("{0,-8}", Time+","), 
									  String.Format("{0,-8}", Impact+","), Forecast, Previous, String.Format("{0,-22}", DateTimeLocal+","), ImpactValue );
			}
			
			public string ToShortString()
			{
				return string.Format("NewsEvent element: {0} {1}: {2}", String.Format("{0,-8}", DateTimeLocal.ToShortTimeString()+""),  Country, String.Format("{0,-36}", Title+",") );  
			}

			public object Clone()
			{
				return this.MemberwiseClone();
			}
			
		}
		#endregion
	}
}


#region Global Enums
public enum EnumNewsEconAlertImpact
{
    High_only	= -3,
    High_Medium	= -2,
	All			= -1,
	None 		= 0
}

public enum EnumNewsEconEventImpact
{
    High_only	= -3,
    High_Medium	= -2,
	All			= -1
}

public enum EnumNewsEconNewsPeriod
{
    Todays_News	= 1,
    Next_24Hrs	= 2,
	All_Week	= 3
}
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NewsEcon[] cacheNewsEcon;
		public NewsEcon NewsEcon(EnumNewsEconNewsPeriod newsPeriod, int maxNewsItems, bool uSOnlyEvents, EnumNewsEconEventImpact displayEventImpact, int newsRefeshInterval, EnumNewsEconAlertImpact sendAlertsOnImpactType, int alertWarningMinutesBefore, string pendingSoundFileNameOnly, int displayNewsMinutesAfter, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush activeEventColor, System.Windows.Media.Brush headerColor, bool showBackground, System.Windows.Media.Brush backgroundColor, int backgroundOpacity, bool use24HrTimeFormat, SimpleFont defaultFont, SimpleFont alertFont, bool debug)
		{
			return NewsEcon(Input, newsPeriod, maxNewsItems, uSOnlyEvents, displayEventImpact, newsRefeshInterval, sendAlertsOnImpactType, alertWarningMinutesBefore, pendingSoundFileNameOnly, displayNewsMinutesAfter, highPriorityColor, mediumPriorityColor, lowPriorityColor, activeEventColor, headerColor, showBackground, backgroundColor, backgroundOpacity, use24HrTimeFormat, defaultFont, alertFont, debug);
		}

		public NewsEcon NewsEcon(ISeries<double> input, EnumNewsEconNewsPeriod newsPeriod, int maxNewsItems, bool uSOnlyEvents, EnumNewsEconEventImpact displayEventImpact, int newsRefeshInterval, EnumNewsEconAlertImpact sendAlertsOnImpactType, int alertWarningMinutesBefore, string pendingSoundFileNameOnly, int displayNewsMinutesAfter, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush activeEventColor, System.Windows.Media.Brush headerColor, bool showBackground, System.Windows.Media.Brush backgroundColor, int backgroundOpacity, bool use24HrTimeFormat, SimpleFont defaultFont, SimpleFont alertFont, bool debug)
		{
			if (cacheNewsEcon != null)
				for (int idx = 0; idx < cacheNewsEcon.Length; idx++)
					if (cacheNewsEcon[idx] != null && cacheNewsEcon[idx].NewsPeriod == newsPeriod && cacheNewsEcon[idx].MaxNewsItems == maxNewsItems && cacheNewsEcon[idx].USOnlyEvents == uSOnlyEvents && cacheNewsEcon[idx].DisplayEventImpact == displayEventImpact && cacheNewsEcon[idx].NewsRefeshInterval == newsRefeshInterval && cacheNewsEcon[idx].SendAlertsOnImpactType == sendAlertsOnImpactType && cacheNewsEcon[idx].AlertWarningMinutesBefore == alertWarningMinutesBefore && cacheNewsEcon[idx].PendingSoundFileNameOnly == pendingSoundFileNameOnly && cacheNewsEcon[idx].DisplayNewsMinutesAfter == displayNewsMinutesAfter && cacheNewsEcon[idx].HighPriorityColor == highPriorityColor && cacheNewsEcon[idx].MediumPriorityColor == mediumPriorityColor && cacheNewsEcon[idx].LowPriorityColor == lowPriorityColor && cacheNewsEcon[idx].ActiveEventColor == activeEventColor && cacheNewsEcon[idx].HeaderColor == headerColor && cacheNewsEcon[idx].ShowBackground == showBackground && cacheNewsEcon[idx].BackgroundColor == backgroundColor && cacheNewsEcon[idx].BackgroundOpacity == backgroundOpacity && cacheNewsEcon[idx].Use24HrTimeFormat == use24HrTimeFormat && cacheNewsEcon[idx].DefaultFont == defaultFont && cacheNewsEcon[idx].AlertFont == alertFont && cacheNewsEcon[idx].Debug == debug && cacheNewsEcon[idx].EqualsInput(input))
						return cacheNewsEcon[idx];
			return CacheIndicator<NewsEcon>(new NewsEcon(){ NewsPeriod = newsPeriod, MaxNewsItems = maxNewsItems, USOnlyEvents = uSOnlyEvents, DisplayEventImpact = displayEventImpact, NewsRefeshInterval = newsRefeshInterval, SendAlertsOnImpactType = sendAlertsOnImpactType, AlertWarningMinutesBefore = alertWarningMinutesBefore, PendingSoundFileNameOnly = pendingSoundFileNameOnly, DisplayNewsMinutesAfter = displayNewsMinutesAfter, HighPriorityColor = highPriorityColor, MediumPriorityColor = mediumPriorityColor, LowPriorityColor = lowPriorityColor, ActiveEventColor = activeEventColor, HeaderColor = headerColor, ShowBackground = showBackground, BackgroundColor = backgroundColor, BackgroundOpacity = backgroundOpacity, Use24HrTimeFormat = use24HrTimeFormat, DefaultFont = defaultFont, AlertFont = alertFont, Debug = debug }, input, ref cacheNewsEcon);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NewsEcon NewsEcon(EnumNewsEconNewsPeriod newsPeriod, int maxNewsItems, bool uSOnlyEvents, EnumNewsEconEventImpact displayEventImpact, int newsRefeshInterval, EnumNewsEconAlertImpact sendAlertsOnImpactType, int alertWarningMinutesBefore, string pendingSoundFileNameOnly, int displayNewsMinutesAfter, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush activeEventColor, System.Windows.Media.Brush headerColor, bool showBackground, System.Windows.Media.Brush backgroundColor, int backgroundOpacity, bool use24HrTimeFormat, SimpleFont defaultFont, SimpleFont alertFont, bool debug)
		{
			return indicator.NewsEcon(Input, newsPeriod, maxNewsItems, uSOnlyEvents, displayEventImpact, newsRefeshInterval, sendAlertsOnImpactType, alertWarningMinutesBefore, pendingSoundFileNameOnly, displayNewsMinutesAfter, highPriorityColor, mediumPriorityColor, lowPriorityColor, activeEventColor, headerColor, showBackground, backgroundColor, backgroundOpacity, use24HrTimeFormat, defaultFont, alertFont, debug);
		}

		public Indicators.NewsEcon NewsEcon(ISeries<double> input , EnumNewsEconNewsPeriod newsPeriod, int maxNewsItems, bool uSOnlyEvents, EnumNewsEconEventImpact displayEventImpact, int newsRefeshInterval, EnumNewsEconAlertImpact sendAlertsOnImpactType, int alertWarningMinutesBefore, string pendingSoundFileNameOnly, int displayNewsMinutesAfter, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush activeEventColor, System.Windows.Media.Brush headerColor, bool showBackground, System.Windows.Media.Brush backgroundColor, int backgroundOpacity, bool use24HrTimeFormat, SimpleFont defaultFont, SimpleFont alertFont, bool debug)
		{
			return indicator.NewsEcon(input, newsPeriod, maxNewsItems, uSOnlyEvents, displayEventImpact, newsRefeshInterval, sendAlertsOnImpactType, alertWarningMinutesBefore, pendingSoundFileNameOnly, displayNewsMinutesAfter, highPriorityColor, mediumPriorityColor, lowPriorityColor, activeEventColor, headerColor, showBackground, backgroundColor, backgroundOpacity, use24HrTimeFormat, defaultFont, alertFont, debug);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NewsEcon NewsEcon(EnumNewsEconNewsPeriod newsPeriod, int maxNewsItems, bool uSOnlyEvents, EnumNewsEconEventImpact displayEventImpact, int newsRefeshInterval, EnumNewsEconAlertImpact sendAlertsOnImpactType, int alertWarningMinutesBefore, string pendingSoundFileNameOnly, int displayNewsMinutesAfter, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush activeEventColor, System.Windows.Media.Brush headerColor, bool showBackground, System.Windows.Media.Brush backgroundColor, int backgroundOpacity, bool use24HrTimeFormat, SimpleFont defaultFont, SimpleFont alertFont, bool debug)
		{
			return indicator.NewsEcon(Input, newsPeriod, maxNewsItems, uSOnlyEvents, displayEventImpact, newsRefeshInterval, sendAlertsOnImpactType, alertWarningMinutesBefore, pendingSoundFileNameOnly, displayNewsMinutesAfter, highPriorityColor, mediumPriorityColor, lowPriorityColor, activeEventColor, headerColor, showBackground, backgroundColor, backgroundOpacity, use24HrTimeFormat, defaultFont, alertFont, debug);
		}

		public Indicators.NewsEcon NewsEcon(ISeries<double> input , EnumNewsEconNewsPeriod newsPeriod, int maxNewsItems, bool uSOnlyEvents, EnumNewsEconEventImpact displayEventImpact, int newsRefeshInterval, EnumNewsEconAlertImpact sendAlertsOnImpactType, int alertWarningMinutesBefore, string pendingSoundFileNameOnly, int displayNewsMinutesAfter, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush activeEventColor, System.Windows.Media.Brush headerColor, bool showBackground, System.Windows.Media.Brush backgroundColor, int backgroundOpacity, bool use24HrTimeFormat, SimpleFont defaultFont, SimpleFont alertFont, bool debug)
		{
			return indicator.NewsEcon(input, newsPeriod, maxNewsItems, uSOnlyEvents, displayEventImpact, newsRefeshInterval, sendAlertsOnImpactType, alertWarningMinutesBefore, pendingSoundFileNameOnly, displayNewsMinutesAfter, highPriorityColor, mediumPriorityColor, lowPriorityColor, activeEventColor, headerColor, showBackground, backgroundColor, backgroundOpacity, use24HrTimeFormat, defaultFont, alertFont, debug);
		}
	}
}

#endregion
