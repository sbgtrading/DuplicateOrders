#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion
/*
//=====================================================================================================
//   OpenSource code distributed under the CDDL v1.0 (http://www.opensource.org/licenses/cddl1.php)
//=====================================================================================================
//
//  Enjoy this OpenSource NinjaScript v7.0 indicator!  May it help bring you great amounts of profit!

//  If you have profited from this code, then please consider a suitable donation of any amount to its 
//  Initial Developer and/or other Contributors.  Contact information given below.

//  Also, feel free to email enhancement suggestions to:
//          Initial Developer:     SBG Trading Corp., Ben Letto, sbgtrading@yahoo.com February 22, 2008
//          NT7.0.0.14 Developer:  Ben Letto, April 24, 2010:  Converted to NT7.0.0.14
//          NT7.0.0.16 Developer:  Ben Letto, June 8, 2010:  Added line thickness parameters
//
//=====================================================================================================

The Square of 9, introduced by W.D. Gann

*****************************************************************************************************
*****************************************************************************************************
WARNING:
This indicator draws NUMEROUS horizontal and vertical lines.  The display of these graphical elements
may take significant amount of time.  Don't be surprised if it takes time to run/load this indicator.
Program logic has taken steps to reduce the processing time, but it still could take time to load.
*****************************************************************************************************
*****************************************************************************************************


IMPORTANT NOTE:

If you find ANY profitable application of this indicator and it helps you trade better, by all means
let me know via email...this way I'll know that all this work went for something profitable!

If you make a million $$$ using this indicator, just remember where you got this code from!  ;)

INSTRUCTIONS:
1)  PriceOrTime parameter must be set to either PriceAnalysis or TimeAnalysis
		This determines if the indicator draws either price levels or time markers
2)  NumOfLevels parameter lets you set the number of lines drawn (max is 30)
3)  Angle000Flag, Angle090Flag, Angle180Flag, and Angle270Flag let you turn on/off those Gann Square of 9 Progressions
	The Angle000 is the progression of numbers from the center of the Square Of 9 to the left edge
	The Angle090 is the progression of numbers from the center of the Square Of 9 to the top edge
	The Angle180 is the progression of numbers from the center of the Square Of 9 to the right edge
	The Angle270 is the progression of numbers from the center of the Square Of 9 to the bottom edge
4)  If you've selected PriceAnalysis, then you must supply an InitialPrice.  This is the price
	that lies at the center of all progressions.
5)  If you've selected TimeAnalysis, then you must supply a ZuluTime.  This time is then the
	center of all bar counts for the progressions

*/
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("Parameters", 5)]
    [CategoryOrder("Custom Visuals", 10)]

	public class SquareOf9 : Indicator
    {
        #region Variables
        // Wizard generated variables
            private double initialPrice = 1;              // Default setting for InitialPrice
            private int multiplierForPriceScale = 1;      // Default setting for MultiplierForPriceScale
            private bool angle000Flag = true;             // Default setting for angle000Flag
            private bool angle090Flag = false;            // Default setting for angle090Flag
            private bool angle180Flag = true;             // Default setting for angle180Flag
            private bool angle270Flag = false;            // Default setting for angle270Flag
			private DateTime zuluTime=DateTime.Now;

		// User defined variables (add any user defined variables below)
			private int LineId=0,i;
			//private Color Color0,Color90,Color180,Color270;
			private double MaxLevelUp=double.MinValue, MinLevelDown=double.MaxValue;
			private int[] Angle0 = new int[] {0,1,10,27,52,85,126,175,232,297,370,451,540,637,742,855,976,1105,1242,1387,1540,1701,1870,2047,2232,2425,2626,2835,3052,3277,3510,3751,4000,4257,4522,4795,5076,5365,5662,5967,6280,6601,6930,7267,7612,7965,8326,8695,9072,9457,9850,10251,10660,11077,11502,11935,12376,12825,13282,13747,14220,14701,15190,15687,16192,16705,17226,17755,18292,18837,19390,19951,20520,21097,21682,22275,22876,23485,24102,24727,25360,26001,26650,27307,27972,28645,29326,30015,30712};
			private int[] Angle90 = new int [] {0,3,14,33,60,95,138,189,248,315,390,473,564,663,770,885,1008,1139,1278,1425,1580,1743,1914,2093,2280,2475,2678,2889,3108,3335,3570,3813,4064,4323,4590,4865,5148,5439,5738,6045,6360,6683,7014,7353,7700,8055,8418,8789,9168,9555,9950,10353,10764,11183,11610,12045,12488,12939,13398,13865,14340,14823,15314,15813,16320,16835,17358,17889,18428,18975,19530,20093,20664,21243,21830,22425,23028,23639,24258,24885,25520,26163,26814,27473,28140,28815,29498,30189,30888};
			private int[] Angle180 = new int [] {0,5,18,39,68,105,150,203,264,333,410,495,588,689,798,915,1040,1173,1314,1463,1620,1785,1958,2139,2328,2525,2730,2943,3164,3393,3630,3875,4128,4389,4658,4935,5220,5513,5814,6123,6440,6765,7098,7439,7788,8145,8510,8883,9264,9653,10050,10455,10868,11289,11718,12155,12600,13053,13514,13983,14460,14945,15438,15939,16448,16965,17490,18023,18564,19113,19670,20235,20808,21389,21978,22575,23180,23793,24414,25043,25680,26325,26978,27639,28308,28985,29670,30363,31064};
			private int[] Angle270 = new int [] {0,7,22,45,76,115,162,217,280,351,430,517,612,715,826,945,1072,1207,1350,1501,1660,1827,2002,2185,2376,2575,2782,2997,3220,3451,3690,3937,4192,4455,4726,5005,5292,5587,5890,6201,6520,6847,7182,7525,7876,8235,8602,8977,9360,9751,10150,10557,10972,11395,11826,12265,12712,13167,13630,14101,14580,15067,15562,16065,16576,17095,17622,18157,18700,19251,19810,20377,20952,21535,22126,22725,23332,23947,24570,25201,25840,26487,27142,27805,28476,29155,29842,30537,31240};
			private double MAX_HIGH = Double.MinValue, MIN_LOW = Double.MaxValue;
			private int ZuluBar, NextBar0,NextBar90,NextBar180,NextBar270, PriceDigits;
			private int lastBar,firstBar;
			private double HighestPaintedPrice,LowestPaintedPrice;
			private string OutString;
			private bool RunInit = true;
			
		#endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        private void Initialize()
        {
            Calculate	= Calculate.OnBarClose;
            IsOverlay				= true;
			ZuluBar = -1;
        }

        protected override void OnStateChange()
        {
            switch (State)
            {
                case State.SetDefaults:
                    Name = "SquareOf9";
                    Description = "Gann's Square Of 9 is a method of plotting resistance/support areas and expecting time turning points.";
                    Initialize();
                    break;
             }
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
		{
			if(RunInit) {
				RunInit=false;
				PriceDigits = Math.Max(0,TickSize.ToString().Length-2);
			}
			if(priceOrTime == SquareOf9_AnalysisType.PriceAnalysis)
			{	MAX_HIGH = Math.Max(MAX_HIGH,High[0]);
				MIN_LOW = Math.Min(MIN_LOW,Low[0]);
			}
			if(priceOrTime == SquareOf9_AnalysisType.PriceAnalysis && CurrentBar == Bars.Count-2)
			{	
				MAX_HIGH = Math.Round(MAX_HIGH*1.05,PriceDigits);
				MIN_LOW = Math.Round(MIN_LOW*0.95,PriceDigits);
				if(initialPrice<MIN_LOW || initialPrice>MAX_HIGH) 
				{	Log("InitialPrice parameter MUST be between the high price and low price on this chart",NinjaTrader.Cbi.LogLevel.Information);
					if(initialPrice>MAX_HIGH) initialPrice = MAX_HIGH;
					if(initialPrice<MIN_LOW) initialPrice = MIN_LOW;
				}

				double LevelUp=-1.0,LevelDown=-1.0;
				i=1;
				do 
				{	if(angle000Flag){ LevelUp=initialPrice+Angle0[i]*TickSize*multiplierForPriceScale;    LevelDown=initialPrice-Angle0[i]*TickSize*multiplierForPriceScale;   MakePriceLine(LevelUp,LevelDown,Color0,   pThickness0Line);}
				 	if(angle090Flag){ LevelUp=initialPrice+Angle90[i]*TickSize*multiplierForPriceScale;   LevelDown=initialPrice-Angle90[i]*TickSize*multiplierForPriceScale;  MakePriceLine(LevelUp,LevelDown,Color90,  pThickness90Line);}
			 		if(angle180Flag){ LevelUp=initialPrice+Angle180[i]*TickSize*multiplierForPriceScale;  LevelDown=initialPrice-Angle180[i]*TickSize*multiplierForPriceScale; MakePriceLine(LevelUp,LevelDown,Color180, pThickness180Line);}
				 	if(angle270Flag){ LevelUp=initialPrice+Angle270[i]*TickSize*multiplierForPriceScale;  LevelDown=initialPrice-Angle270[i]*TickSize*multiplierForPriceScale; MakePriceLine(LevelUp,LevelDown,Color270, pThickness270Line);}
					i++;
					if(LevelUp<0.0 || LevelDown<0.0) break;
					if(i>=Angle0.Length) break;
					if(i>=Angle90.Length) break;
					if(i>=Angle180.Length) break;
					if(i>=Angle270.Length) break;

					if(LevelUp > MaxLevelUp)     MaxLevelUp   = LevelUp;
					if(LevelDown < MinLevelDown) MinLevelDown = LevelDown;
				}
				while (1==1);
    	    }
			if(priceOrTime==SquareOf9_AnalysisType.TimeAnaysis)
			{	if(zuluTime.CompareTo(Time[0])<0 && ZuluBar<0)  ZuluBar=CurrentBar; //set ZuluBar to time the user selected
				if(ZuluBar>0)
				{	OutString=null;
					if(angle000Flag) 
					{	i = Angle0[NextBar0]+ZuluBar;
						if(i==CurrentBar) { MakeVerticalLine(0,Color0,pThickness0Line); NextBar0++;}
						OutString=string.Concat("Zero line coming in ",(i-CurrentBar).ToString()," bars",Environment.NewLine);
					}
				 	if(angle090Flag) 
					{	i = Angle90[NextBar90]+ZuluBar;
						if(i==CurrentBar) { MakeVerticalLine(0,Color90,pThickness90Line); NextBar90++;}
						OutString=string.Concat(OutString,"90 line coming in ",(i-CurrentBar).ToString()," bars",Environment.NewLine);
					}
				 	if(angle180Flag) 
					{	i = Angle180[NextBar180]+ZuluBar;
						if(i==CurrentBar) { MakeVerticalLine(0,Color180,pThickness180Line); NextBar180++;}
						OutString=string.Concat(OutString,"180 line coming in ",(i-CurrentBar).ToString()," bars",Environment.NewLine);
					}
				 	if(angle270Flag) 
					{	i = Angle270[NextBar270]+ZuluBar;
						if(i==CurrentBar) { MakeVerticalLine(0,Color270,pThickness270Line); NextBar270++;}
						OutString=string.Concat(OutString,"270 line coming in ",(i-CurrentBar).ToString()," bars");
					}
					if(OutString.Length > 0 && pTxtLocation!=SquareOf9_TxtLoc.None) 
					{	lastBar		= Math.Min(ChartBars.ToIndex, Bars.Count - 1);
						firstBar	= (lastBar - ChartBars.FromIndex) + 1;
						int i;
						// Find highest and lowest price points
						HighestPaintedPrice = double.MinValue;
						LowestPaintedPrice  = double.MaxValue;
						for (i = firstBar; i <= lastBar && i >= 0; i++)
						{
							HighestPaintedPrice = Math.Max(HighestPaintedPrice, ChartBars.Bars.GetHigh(i));
							LowestPaintedPrice  = Math.Min(LowestPaintedPrice , ChartBars.Bars.GetLow(i));
						}
						double Outprice = (HighestPaintedPrice+LowestPaintedPrice)/2.0;
						if(ChartBars.Bars.GetClose(lastBar-1)< Outprice) Outprice = HighestPaintedPrice;
						if(ChartBars.Bars.GetClose(lastBar-1)>= Outprice) Outprice = (Outprice+LowestPaintedPrice)/2.0;

						Print(OutString);
						if(CurrentBar>10) {
//							DrawText("Info",false, OutString,10,Outprice,0,Color.Black,new Font("Arial",10,FontStyle.Italic,GraphicsUnit.Point),StringAlignment.Near,Color.White,Color.Transparent,50);
							if(pTxtLocation == SquareOf9_TxtLoc.BottomLeft)  Draw.TextFixed(this,"Info", string.Concat(OutString,Environment.NewLine,"\t"), TextPosition.BottomLeft);
							if(pTxtLocation == SquareOf9_TxtLoc.BottomRight) Draw.TextFixed(this,"Info", string.Concat(OutString,Environment.NewLine,"\t"), TextPosition.BottomRight);
							if(pTxtLocation == SquareOf9_TxtLoc.TopRight) Draw.TextFixed(this,"Info", string.Concat("\t",Environment.NewLine,OutString), TextPosition.TopRight);
							if(pTxtLocation == SquareOf9_TxtLoc.TopLeft)  Draw.TextFixed(this,"Info", string.Concat("\t",Environment.NewLine,OutString), TextPosition.TopLeft);
							if(pTxtLocation == SquareOf9_TxtLoc.Center)   Draw.TextFixed(this,"Info", OutString, TextPosition.Center);
						}
					}
				}
			}
		}
		
        #region Properties
		private SquareOf9_AnalysisType priceOrTime = SquareOf9_AnalysisType.PriceAnalysis;
        [NinjaScriptProperty]
		[Display(Description = "Calculate the SquareOf9 Price levels or Time periods?", GroupName = "Parameters", Order = 10)]
		public SquareOf9_AnalysisType PriceOrTime
        {
            get { return priceOrTime; }
            set { priceOrTime = value; }
        }
		
		private bool pGlobalizeLines = false;
        [Display(Name = "Globalize lines?", Description = "If true, then the color and thickness parameters are ignored", GroupName = "Parameters", Order = 15)]
        public bool GlobalizeLines
        {
            get { return pGlobalizeLines; }
            set { pGlobalizeLines = value; }
        }

        [NinjaScriptProperty]		
        [Display(Description = "", GroupName = "Parameters", Order = 20)]		
        public double InitialPrice
        {
            get { return initialPrice; }
            set { initialPrice = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Description = "Multiplier for scaling of price levels", GroupName = "Parameters", Order = 30)]
        public int MultiplierForPriceScale
        {
            get { return multiplierForPriceScale; }
            set { multiplierForPriceScale = value; }
        }

        [NinjaScriptProperty]
		[Display(Description = "If Time, enter the Zulu time", GroupName = "Parameters", Order = 40)]
		public DateTime ZuluTime
        {
            get { return zuluTime; }
            set { zuluTime = value; }
        }
		
        [NinjaScriptProperty]
		[Display(Name = "Angle 270 on/off", Description = "Show 270 angle lines", GroupName = "Custom Visuals", Order = 40)]
		public bool Angle270Flag
        {
            get { return angle270Flag; }
            set { angle270Flag = value; }
        }
		private Brush Color270 = Brushes.Purple;
		[XmlIgnore()]
		[Display(Name = "Color", Description = "", GroupName = "Custom Visuals", Order = 41)]
		public Brush InputAngle270Color{	get { return Color270; }	set { Color270 = value; }		}
				[Browsable(false)]
				public string A270ClSerialize
				{	get { return Serialize.BrushToString(Color270); } set { Color270 = Serialize.StringToBrush(value); }
				}

		private int pThickness270Line = 1;
        [NinjaScriptProperty]
		[Display(Name = "Thickness", GroupName = "Custom Visuals", Order = 45)]        public int Thickness270Line
        {
            get { return pThickness270Line; }
            set { pThickness270Line = value; }
        }

        [NinjaScriptProperty]
		[Display(Name = "Angle 180 on/off", Description = "Show 180 angle lines", GroupName = "Custom Visuals", Order = 30)]
		public bool Angle180Flag
        {
            get { return angle180Flag; }
            set { angle180Flag = value; }
        }
		private Brush Color180 = Brushes.Green;
		[XmlIgnore()]
		[Display(Name = "Color", Description = "", GroupName = "Custom Visuals", Order = 31)]
		public Brush InputAngle180Color{	get { return Color180; }	set { Color180 = value; }		}
				[Browsable(false)]
				public string A180ClSerialize
				{	get { return Serialize.BrushToString(Color180); } set { Color180 = Serialize.StringToBrush(value); }
				}
		private int pThickness180Line = 1;
        [NinjaScriptProperty]
		[Display(Name = "Thickness", GroupName = "Custom Visuals", Order = 35)]        public int Thickness180Line
        {
            get { return pThickness180Line; }
            set { pThickness180Line = value; }
        }
		private Brush Color90 = Brushes.Blue;
        [NinjaScriptProperty]
		[Display(Name = "Angle 090 on/off", Description = "Show 90 angle lines", GroupName = "Custom Visuals", Order = 20)]
		public bool Angle090Flag
        {
            get { return angle090Flag; }
            set { angle090Flag = value; }
        }
		[XmlIgnore()]
		[Display(Name = "Color", Description = "", GroupName = "Custom Visuals", Order = 21)]
		public Brush InputAngle090Color{	get { return Color90; }	set { Color90 = value; }		}
				[Browsable(false)]
				public string A090ClSerialize
				{	get { return Serialize.BrushToString(Color90); } set { Color90 = Serialize.StringToBrush(value); }
				}
		private int pThickness90Line = 1;
		[Display(Name = "Thickness", GroupName = "Custom Visuals", Order = 25)]
		public int Thickness090Line
        {
            get { return pThickness90Line; }
            set { pThickness90Line = value; }
        }
        [NinjaScriptProperty]
        [Display(Name = "Angle 000 on/off", Description = "Show zero angle lines", GroupName = "Custom Visuals", Order = 10)]
        public bool Angle000Flag
        {
            get { return angle000Flag; }
            set { angle000Flag = value; }
        }
		private Brush Color0 = Brushes.Red;
		[XmlIgnore()]
		[Display(Name = "Color", Description = "", GroupName = "Custom Visuals", Order = 11)]
		public Brush InputAngle000Color{	get { return Color0; }	set { Color0 = value; }		}
				[Browsable(false)]
				public string A000ClSerialize
				{	get { return Serialize.BrushToString(Color0); } set { Color0 = Serialize.StringToBrush(value); }
				}
		private int pThickness0Line = 1;
		[Display(Name = "Thickness", GroupName = "Custom Visuals", Order = 15)]
		public int Thickness000Line
        {
            get { return pThickness0Line; }
            set { pThickness0Line = value; }
        }

	
		private SquareOf9_TxtLoc pTxtLocation = SquareOf9_TxtLoc.BottomRight;
        [NinjaScriptProperty]
		[Display(Name = "Text location", Description = "Where to print out the information concerning approaching time periods", GroupName = "Custom Visuals", Order = 50)]        public SquareOf9_TxtLoc TimeInfoLocation
        {
            get { return pTxtLocation; }
            set { pTxtLocation = value; }
        }

		#endregion

//======================================================================================
		private void MakePriceLine(double Up, double Down, Brush C, int thickness)
		{
			if(Up>0.0 && Up < MAX_HIGH) {
				if(pGlobalizeLines)
					Draw.HorizontalLine(this, $"Sq9P_{LineId}", Up, true, "");
				else
					Draw.HorizontalLine(this,$"Sq9P_{LineId}", false, Up, C, DashStyleHelper.Dash,thickness);
				LineId++;
			}
			if(Down>0.0 && Down > MIN_LOW) {
				if(pGlobalizeLines)
					Draw.HorizontalLine(this, $"Sq9P_{LineId}", Down, true, "");
				else
					Draw.HorizontalLine(this,$"Sq9P_{LineId}",false,Down,C,DashStyleHelper.Dash,thickness);
				LineId++;
			}
		}
//======================================================================================
		private void MakeVerticalLine(int BarNum, Brush C, int thickness)
		{
			if(pGlobalizeLines)
				Draw.VerticalLine(this,$"Sq9T_{LineId}",BarNum, true,"");
			else
				Draw.VerticalLine(this,$"Sq9T_{LineId}",BarNum,C,DashStyleHelper.Dash,thickness);
			LineId++;
		}
//======================================================================================
	}
}
public enum SquareOf9_TxtLoc {
	None,
	TopRight,
	TopLeft,
	Center,
	BottomRight,
	BottomLeft
}
public enum SquareOf9_AnalysisType {PriceAnalysis, TimeAnaysis}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SquareOf9[] cacheSquareOf9;
		public SquareOf9 SquareOf9(SquareOf9_AnalysisType priceOrTime, double initialPrice, int multiplierForPriceScale, DateTime zuluTime, bool angle270Flag, int thickness270Line, bool angle180Flag, int thickness180Line, bool angle090Flag, bool angle000Flag, SquareOf9_TxtLoc timeInfoLocation)
		{
			return SquareOf9(Input, priceOrTime, initialPrice, multiplierForPriceScale, zuluTime, angle270Flag, thickness270Line, angle180Flag, thickness180Line, angle090Flag, angle000Flag, timeInfoLocation);
		}

		public SquareOf9 SquareOf9(ISeries<double> input, SquareOf9_AnalysisType priceOrTime, double initialPrice, int multiplierForPriceScale, DateTime zuluTime, bool angle270Flag, int thickness270Line, bool angle180Flag, int thickness180Line, bool angle090Flag, bool angle000Flag, SquareOf9_TxtLoc timeInfoLocation)
		{
			if (cacheSquareOf9 != null)
				for (int idx = 0; idx < cacheSquareOf9.Length; idx++)
					if (cacheSquareOf9[idx] != null && cacheSquareOf9[idx].PriceOrTime == priceOrTime && cacheSquareOf9[idx].InitialPrice == initialPrice && cacheSquareOf9[idx].MultiplierForPriceScale == multiplierForPriceScale && cacheSquareOf9[idx].ZuluTime == zuluTime && cacheSquareOf9[idx].Angle270Flag == angle270Flag && cacheSquareOf9[idx].Thickness270Line == thickness270Line && cacheSquareOf9[idx].Angle180Flag == angle180Flag && cacheSquareOf9[idx].Thickness180Line == thickness180Line && cacheSquareOf9[idx].Angle090Flag == angle090Flag && cacheSquareOf9[idx].Angle000Flag == angle000Flag && cacheSquareOf9[idx].TimeInfoLocation == timeInfoLocation && cacheSquareOf9[idx].EqualsInput(input))
						return cacheSquareOf9[idx];
			return CacheIndicator<SquareOf9>(new SquareOf9(){ PriceOrTime = priceOrTime, InitialPrice = initialPrice, MultiplierForPriceScale = multiplierForPriceScale, ZuluTime = zuluTime, Angle270Flag = angle270Flag, Thickness270Line = thickness270Line, Angle180Flag = angle180Flag, Thickness180Line = thickness180Line, Angle090Flag = angle090Flag, Angle000Flag = angle000Flag, TimeInfoLocation = timeInfoLocation }, input, ref cacheSquareOf9);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SquareOf9 SquareOf9(SquareOf9_AnalysisType priceOrTime, double initialPrice, int multiplierForPriceScale, DateTime zuluTime, bool angle270Flag, int thickness270Line, bool angle180Flag, int thickness180Line, bool angle090Flag, bool angle000Flag, SquareOf9_TxtLoc timeInfoLocation)
		{
			return indicator.SquareOf9(Input, priceOrTime, initialPrice, multiplierForPriceScale, zuluTime, angle270Flag, thickness270Line, angle180Flag, thickness180Line, angle090Flag, angle000Flag, timeInfoLocation);
		}

		public Indicators.SquareOf9 SquareOf9(ISeries<double> input , SquareOf9_AnalysisType priceOrTime, double initialPrice, int multiplierForPriceScale, DateTime zuluTime, bool angle270Flag, int thickness270Line, bool angle180Flag, int thickness180Line, bool angle090Flag, bool angle000Flag, SquareOf9_TxtLoc timeInfoLocation)
		{
			return indicator.SquareOf9(input, priceOrTime, initialPrice, multiplierForPriceScale, zuluTime, angle270Flag, thickness270Line, angle180Flag, thickness180Line, angle090Flag, angle000Flag, timeInfoLocation);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SquareOf9 SquareOf9(SquareOf9_AnalysisType priceOrTime, double initialPrice, int multiplierForPriceScale, DateTime zuluTime, bool angle270Flag, int thickness270Line, bool angle180Flag, int thickness180Line, bool angle090Flag, bool angle000Flag, SquareOf9_TxtLoc timeInfoLocation)
		{
			return indicator.SquareOf9(Input, priceOrTime, initialPrice, multiplierForPriceScale, zuluTime, angle270Flag, thickness270Line, angle180Flag, thickness180Line, angle090Flag, angle000Flag, timeInfoLocation);
		}

		public Indicators.SquareOf9 SquareOf9(ISeries<double> input , SquareOf9_AnalysisType priceOrTime, double initialPrice, int multiplierForPriceScale, DateTime zuluTime, bool angle270Flag, int thickness270Line, bool angle180Flag, int thickness180Line, bool angle090Flag, bool angle000Flag, SquareOf9_TxtLoc timeInfoLocation)
		{
			return indicator.SquareOf9(input, priceOrTime, initialPrice, multiplierForPriceScale, zuluTime, angle270Flag, thickness270Line, angle180Flag, thickness180Line, angle090Flag, angle000Flag, timeInfoLocation);
		}
	}
}

#endregion
