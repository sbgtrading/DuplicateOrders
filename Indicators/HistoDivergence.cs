

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
//using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui.Tools;
using SharpDX.DirectWrite;
using System.Windows.Controls;
using System.Windows.Automation;
#endregion


namespace NinjaTrader.NinjaScript.Indicators
{
    #region -- Category Order --
    [CategoryOrder("Parameters", 1)]
    [CategoryOrder("Divergence Options", 10)]
    [CategoryOrder("Divergence Parameters", 20)]
    [CategoryOrder("MACDBB Parameters", 30)]
    [CategoryOrder("SwingTrend Parameters", 40)]
    [CategoryOrder("Display Options Divergences", 50)]
    [CategoryOrder("Display Options Oscillator Panel", 60)]
    [CategoryOrder("Display Options Price Excursions", 65)]
    [CategoryOrder("Display Options Swing Trend", 70)]
    [CategoryOrder("Histogram Divergences - Plot Colors", 80)]
    [CategoryOrder("Histogram Divergences - Plot Parameters", 90)]
	[CategoryOrder("Histogram Divergences - Sounds", 95)]
    [CategoryOrder("Histogram Hidden Divergences - Plot Colors", 100)]
    [CategoryOrder("Histogram Hidden Divergences - Plot Parameters", 110)]
    [CategoryOrder("MACD Divergences - Plot Colors", 120)]
    [CategoryOrder("MACD Divergences - Plot Parameters", 130)]
    [CategoryOrder("MACD Hidden Divergences - Plot Colors", 140)]
    [CategoryOrder("MACD Hidden Divergences - Plot Parameters", 150)]
    [CategoryOrder("Plot Colors", 160)]
    [CategoryOrder("Plot Parameters", 170)]
    [CategoryOrder("Price Excursion - Plot Colors", 180)]
    [CategoryOrder("Price Excursion - Plot Parameters", 190)]
    #endregion
    public class HistoDivergence : Indicator
    {
		bool IsDebug = false;

        private const string VERSION = "v1.0";
		
        #region ---- Variables ---- 
		#region -- Bloodhound --
//		private Series<double> bearishMACDDivProjection;
//		private Series<double> bullishMACDDivProjection;
//		private Series<double> bearishHistogramDivProjection;
//		private Series<double> bullishHistogramDivProjection;
//		private Series<double> bearishMACDHiddenDivProjection;
//		private Series<double> bullishMACDHiddenDivProjection;
//		private Series<double> bearishHistogramHiddenDivProjection;
//		private Series<double> bullishHistogramHiddenDivProjection;
		private Series<double> structureBiasState;
//		private Series<double> swingHighsState;
//		private Series<double> swingLowsState;

//		private double cptBearMACDdiv, maxcptBearMACDdiv, memfirstPeakBar1;//#BLOOHOUND - added 17.02.03 - AzurITec
//		private double cptBullMACDdiv, maxcptBullMACDdiv, memfirstTroughBar1;//#BLOOHOUND - added 17.02.03 - AzurITec        
//		private double cptBearHistogramdiv, maxcptBearHistogramdiv, memfirstPeakBar2;//#BLOOHOUND - added 17.02.03 - AzurITec
//		private double cptBullHistogramdiv, maxcptBullHistogramdiv, memfirstTroughBar2;//#BLOOHOUND - added 17.02.03 - AzurITec

//		private double cptBearMACDhdiv, maxcptBearMACDhdiv, memfirstPeakBar1H;//#BLOOHOUND - added 17.02.03 - AzurITec
//		private double cptBullMACDhdiv, maxcptBullMACDhdiv, memfirstTroughBar1H;//#BLOOHOUND - added 17.02.03 - AzurITec        
//		private double cptBearHistogramhdiv, maxcptBearHistogramhdiv, memfirstPeakBar2H;//#BLOOHOUND - added 17.02.03 - AzurITec
//		private double cptBullHistogramhdiv, maxcptBullHistogramhdiv, memfirstTroughBar2H;//#BLOOHOUND - added 17.02.03 - AzurITec
		#endregion

        #region -- Structure BIAS + Swings --
        private List<int> sequence = new List<int>(3);//#STRBIAS
        private int SRType, preSRType;//#STRBIAS  
        #endregion

        #region -- velocity momo  --
        private MACD BMACD;
        private MACD MACD1;
        private MACD MACD2;
        private MACD MACD3;
        private MACD MACD4;
        private StdDev SDBB;
        #endregion

        #region -- zigzag indicator -- 
        private int lastHighIdx = 0;
        private int lastLowIdx = 0;
        private int priorSwingHighIdx = 0;
        private int priorSwingLowIdx = 0;
        private int highCount = 0;
        private int lowCount = 0;
        private int preLastHighIdx = 0;
        private int preLastLowIdx = 0;
        private double zigzagDeviation = 0.0;
        private double currentHigh = 0.0;
        private double currentLow = 0.0;
        private double swingMax = 0.0;
        private double swingMin = 0.0;
        private double preCurrentHigh = 0.0;
        private double preCurrentLow = 0.0;
        private bool addHigh = false;
        private bool updateHigh = false;
        private bool addLow = false;
        private bool updateLow = false;
        private bool drawHigherHighDot = false;
        private bool drawLowerHighDot = false;
        private bool drawDoubleTopDot = false;
        private bool drawLowerLowDot = false;
        private bool drawHigherLowDot = false;
        private bool drawDoubleBottomDot = false;
        private bool drawHigherHighLabel = false;
        private bool drawLowerHighLabel = false;
        private bool drawDoubleTopLabel = false;
        private bool drawLowerLowLabel = false;
        private bool drawHigherLowLabel = false;
        private bool drawDoubleBottomLabel = false;
        private bool drawSwingLegUp = false;
        private bool drawSwingLegDown = false;
        private bool intraBarAddHigh = false;
        private bool intraBarUpdateHigh = false;
        private bool intraBarAddLow = false;
        private bool intraBarUpdateLow = false;

        private SimpleFont labelFont = null;
        private SimpleFont swingDotFont = null;
        private int pixelOffset1 = 10;               //##HARD CODED##
        private int pixelOffset2 = 10;               //##HARD CODED##
        private Brush upColor = Brushes.LimeGreen;//##HARD CODED##
        private Brush downColor = Brushes.Red;      //##HARD CODED##
        private Brush doubleTopBottomColor = Brushes.Yellow;   //##HARD CODED##
        private string dotString = "n";              //##HARD CODED##

        private ATR avgTrueRange;
        private Series<double> swingInput;
        private Series<int> pre_swingHighType;
        private Series<int> pre_swingLowType;
        private Series<int> swingHighType;
        private Series<int> swingLowType;
        private Series<int> acceleration1;
        private Series<int> acceleration2;
        private Series<bool> upTrend;
        #endregion

        #region -- divergence indicator --
        private bool divergenceActive = true;

        private int[] refPeakBar1H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int[] refOscPeakBar1H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstOscPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstOscPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementOscPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondOscPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondOscPeakBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int peakCount1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec

        private int[] refTroughBar1H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int[] refOscTroughBar1H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstOscTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstOscTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementOscTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondOscTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondOscTroughBar1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int troughCount1H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec

        private int[] refPeakBar2H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int[] refOscPeakBar2H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstOscPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstOscPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementOscPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondOscPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondOscPeakBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int peakCount2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec

        private int[] refTroughBar2H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int[] refOscTroughBar2H = new int[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int firstOscTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorFirstOscTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int replacementOscTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int secondOscTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int priorSecondOscTroughBar2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private int troughCount2H = 0;//#HIDDENDIV - added 17.02.01 - AzurITec

        private double[] refPeakHigh1H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refPeakValue1H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstPeakHigh1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstPeakValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstPeakHigh1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstPeakValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementPeakHigh1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementPeakValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementPeakValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondPeakHigh1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondPeakValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondPeakHigh1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondPeakValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementPeakHigh1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refTroughLow1H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refTroughValue1H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstTroughLow1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstTroughValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstTroughLow1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstTroughValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementTroughLow1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementTroughValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementTroughValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementTroughLow1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondTroughLow1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondTroughValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondTroughLow1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondTroughValue1H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementPeakHigh2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementTroughLow2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refPeakHigh2H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refPeakValue2H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstPeakHigh2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstPeakValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstPeakHigh2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstPeakValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementPeakHigh2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementPeakValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementPeakValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondPeakHigh2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondPeakValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondPeakHigh2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondPeakValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refTroughLow2H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double[] refTroughValue2H = new double[10];//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstTroughLow2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double firstTroughValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstTroughLow2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorFirstTroughValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementTroughLow2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double replacementTroughValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorReplacementTroughValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondTroughLow2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double secondTroughValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondTroughLow2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec
        private double priorSecondTroughValue2H = 0.0;//#HIDDENDIV - added 17.02.01 - AzurITec

        private bool firstPeakFound1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool firstTroughFound1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivCandidatePrice1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivCandidateOsc1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBearishDivCandidatePrice1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBearishDivCandidateOsc1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivOnPrice1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivOnOsc1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivCandidatePrice1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivCandidateOsc1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBullishDivCandidatePrice1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBullishDivCandidateOsc1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivOnPrice1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivOnOsc1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearSetup1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullSetup1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawArrowDown1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawArrowUp1H = false;//#HIDDENDIV - added 17.02.01 - AzurITec

        private bool firstPeakFound2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool firstTroughFound2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivCandidatePrice2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivCandidateOsc2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBearishDivCandidatePrice2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBearishDivCandidateOsc2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivOnPrice2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearishDivOnOsc2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivCandidatePrice2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivCandidateOsc2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBullishDivCandidatePrice2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool updateBullishDivCandidateOsc2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivOnPrice2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullishDivOnOsc2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBearSetup2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawBullSetup2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawArrowDown2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec
        private bool drawArrowUp2H = false;//#HIDDENDIV - added 17.02.01 - AzurITec

        private int[] refPeakBar1 = new int[10];
        private int[] refOscPeakBar1 = new int[10];
        private int firstPeakBar1 = 0;
        private int firstOscPeakBar1 = 0;
        private int priorFirstPeakBar1 = 0;
        private int priorFirstOscPeakBar1 = 0;
        private int replacementPeakBar1 = 0;
        private int replacementOscPeakBar1 = 0;
        private int secondPeakBar1 = 0;
        private int secondOscPeakBar1 = 0;
        private int priorSecondPeakBar1 = 0;
        private int priorSecondOscPeakBar1 = 0;
        private int peakCount1 = 0;

        private int[] refTroughBar1 = new int[10];
        private int[] refOscTroughBar1 = new int[10];
        private int firstTroughBar1 = 0;
        private int firstOscTroughBar1 = 0;
        private int priorFirstTroughBar1 = 0;
        private int priorFirstOscTroughBar1 = 0;
        private int replacementTroughBar1 = 0;
        private int replacementOscTroughBar1 = 0;
        private int secondTroughBar1 = 0;
        private int secondOscTroughBar1 = 0;
        private int priorSecondTroughBar1 = 0;
        private int priorSecondOscTroughBar1 = 0;
        private int troughCount1 = 0;

        private int[] refPeakBar2 = new int[10];
        private int[] refOscPeakBar2 = new int[10];
        private int firstPeakBar2 = 0;
        private int firstOscPeakBar2 = 0;
        private int priorFirstPeakBar2 = 0;
        private int priorFirstOscPeakBar2 = 0;
        private int replacementPeakBar2 = 0;
        private int replacementOscPeakBar2 = 0;
        private int secondPeakBar2 = 0;
        private int secondOscPeakBar2 = 0;
        private int priorSecondPeakBar2 = 0;
        private int priorSecondOscPeakBar2 = 0;
        private int peakCount2 = 0;

        private int[] refTroughBar2 = new int[10];
        private int[] refOscTroughBar2 = new int[10];
        private int firstTroughBar2 = 0;
        private int firstOscTroughBar2 = 0;
        private int priorFirstTroughBar2 = 0;
        private int priorFirstOscTroughBar2 = 0;
        private int replacementTroughBar2 = 0;
        private int replacementOscTroughBar2 = 0;
        private int secondTroughBar2 = 0;
        private int secondOscTroughBar2 = 0;
        private int priorSecondTroughBar2 = 0;
        private int priorSecondOscTroughBar2 = 0;
        private int troughCount2 = 0;

        private double offsetDraw1 = 0.0;
        private double offsetDraw2 = 0.0;
        private double offsetDiv1 = 0.0;
        private double offsetDiv2 = 0.0;
        private double[] refPeakHigh1 = new double[10];
        private double[] refPeakValue1 = new double[10];
        private double firstPeakHigh1 = 0.0;
        private double firstPeakValue1 = 0.0;
        private double priorFirstPeakHigh1 = 0.0;
        private double priorFirstPeakValue1 = 0.0;
        private double replacementPeakHigh1 = 0.0;
        private double replacementPeakValue1 = 0.0;
        private double priorReplacementPeakValue1 = 0.0;
        private double secondPeakHigh1 = 0.0;
        private double secondPeakValue1 = 0.0;
        private double priorSecondPeakHigh1 = 0.0;
        private double priorSecondPeakValue1 = 0.0;
        private double[] refTroughLow1 = new double[10];
        private double[] refTroughValue1 = new double[10];
        private double firstTroughLow1 = 0.0;
        private double firstTroughValue1 = 0.0;
        private double priorFirstTroughLow1 = 0.0;
        private double priorFirstTroughValue1 = 0.0;
        private double replacementTroughLow1 = 0.0;
        private double replacementTroughValue1 = 0.0;
        private double priorReplacementTroughValue1 = 0.0;
        private double secondTroughLow1 = 0.0;
        private double secondTroughValue1 = 0.0;
        private double priorSecondTroughLow1 = 0.0;
        private double priorSecondTroughValue1 = 0.0;

        private double[] refPeakHigh2 = new double[10];
        private double[] refPeakValue2 = new double[10];
        private double firstPeakHigh2 = 0.0;
        private double firstPeakValue2 = 0.0;
        private double priorFirstPeakHigh2 = 0.0;
        private double priorFirstPeakValue2 = 0.0;
        private double replacementPeakHigh2 = 0.0;
        private double replacementPeakValue2 = 0.0;
        private double priorReplacementPeakValue2 = 0.0;
        private double secondPeakHigh2 = 0.0;
        private double secondPeakValue2 = 0.0;
        private double priorSecondPeakHigh2 = 0.0;
        private double priorSecondPeakValue2 = 0.0;
        private double[] refTroughLow2 = new double[10];
        private double[] refTroughValue2 = new double[10];
        private double firstTroughLow2 = 0.0;
        private double firstTroughValue2 = 0.0;
        private double priorFirstTroughLow2 = 0.0;
        private double priorFirstTroughValue2 = 0.0;
        private double replacementTroughLow2 = 0.0;
        private double replacementTroughValue2 = 0.0;
        private double priorReplacementTroughValue2 = 0.0;
        private double secondTroughLow2 = 0.0;
        private double secondTroughValue2 = 0.0;
        private double priorSecondTroughLow2 = 0.0;
        private double priorSecondTroughValue2 = 0.0;

        private bool drawObjectsEnabled = true;//##HARD CODED## - Set only here
        private bool hidePlots = false;//##HARD CODED## - Set only here
        private bool showArrows = true;//##HARD CODED## - Set only here

        private SimpleFont triangleFont1 = null;
        private SimpleFont setupFont1 = null;
        private SimpleFont triangleFont2 = null;
        private SimpleFont setupFont2 = null;
        private string arrowStringUp = "5";//##HARD CODED## - Set only here
        private string arrowStringDown = "6";//##HARD CODED## - Set only here
        private string setupDotString = "n";//##HARD CODED## - Set only here

        private Series<double> bbDotTrend;
        private Series<double> momoSignum;

        private Series<double> bearishCDivMACD;
        private Series<double> bullishCDivMACD;
        private Series<double> bearishCDivHistogram;
        private Series<double> bullishCDivHistogram;

        private Series<double> bearishPDivMACD;
        private Series<double> bullishPDivMACD;
        private Series<double> bearishPDivHistogram;
        private Series<double> bullishPDivHistogram;

        private Series<int> bearishTriggerCountMACDBB;
        private Series<int> bullishTriggerCountMACDBB;
        private Series<int> bearishTriggerCountHistogram;
        private Series<int> bullishTriggerCountHistogram;

        private Series<double> macdBBState;
        private Series<double> histogramState;

        private Series<double> hiddenbearishPDivMACD;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbullishPDivMACD;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbearishCDivMACD;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbullishCDivMACD;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbearishPDivHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbullishPDivHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbearishCDivHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<double> hiddenbullishCDivHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> hiddenbearishTriggerCountMACDBB;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> hiddenbullishTriggerCountMACDBB;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> hiddenbearishTriggerCountHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> hiddenbullishTriggerCountHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec

        private Series<int> bearishDivPlotSeriesMACDBB;
        private Series<int> hiddenbearishDivPlotSeriesMACDBB;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> bullishDivPlotSeriesMACDBB;
        private Series<int> hiddenbullishDivPlotSeriesMACDBB;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> bearishDivPlotSeriesHistogram;
        private Series<int> hiddenbearishDivPlotSeriesHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec
        private Series<int> bullishDivPlotSeriesHistogram;
        private Series<int> hiddenbullishDivPlotSeriesHistogram;//#HIDDENDIV - added 17.02.01 - AzurITec

        #endregion

        #region -- Box --

        private System.Windows.TextAlignment textAlignmentCenter = System.Windows.TextAlignment.Center;

        private double filterValue1 = 0.0;
        private double filterValue2 = 0.0;
        private double filterValue3 = 0.0;

        #endregion

        #region -- rj RJ5GROUP code Algorithm  --
        private SMA ExcursionSeries;
        #endregion

        #endregion

        #region -- Index Constants to change Plot order and superposition. Lower index goes below --        
        private const int BBMACDIDX = 12;
        private const int BBMACDFrameIDX = 11;
        private const int BBMACDLineIDX = 10;
        private const int AverageIDX = 9;
        private const int UpperIDX = 8;
        private const int LowerIDX = 7;
        private const int HistogramIDX = 6;
        private const int PriceExcursionUL3IDX = 5;
        private const int PriceExcursionUL2IDX = 4;
        private const int PriceExcursionUL1IDX = 3;
        private const int PriceExcursionLL1IDX = 2;
        private const int PriceExcursionLL2IDX = 1;
        private const int PriceExcursionLL3IDX = 0;
        private const int PriceExcursionMAXIDX = 14;
        private const int PriceExcursionMINIDX = 13;
        #endregion
		int line=0;

        public override string DisplayName { get { return "HistoDivergence"; } }

        #region -- Toolbar variables --
        private string toolbarname = "HDIVToolBar", uID;
        private bool isToolBarButtonAdded = false;
        private Chart chartWindow;
        private Grid indytoolbar;

        private Menu MenuControlContainer;
        private MenuItem MenuControl;

        //private ComboBox comboMTF, comboZFM, comboProfile;
        private MenuItem miDivOptions1, miDivOptions2, miDivOptions3, miDivOptions4;
        private MenuItem miDisplayOptions1, miDisplayOptions2, miDisplayOptions3, miDisplayOptions4, miDisplayOptions5, miDisplayOptions6, miDisplayOptions7;
        private MenuItem miExcursionLevels1, miExcursionLevels2, miExcursionLevels3, miMarketStructure1, miMarketStructure2;
        private ComboBox comboExcursionStyle, comboFlooding;
        private TextBox nudMTF, nudZFM, nudMST1, nudMST2, nudCRV1, nudCRV2;
        
        private Button gCmdup;
        private Button gCmddw;
        private Label gLabel;
        #endregion

        #region -- Toolbar Management Utilities --
        #region private void addToolBar()
        private void addToolBar()
        {
            gCmdup = new Button() { Margin = new Thickness(0), Padding = new Thickness(0, -5, 0, 0), Height = 10, Width = 11, MinWidth = 11, MaxWidth = 11, Content = "??", VerticalAlignment = VerticalAlignment.Bottom, FontWeight = FontWeights.Bold, FontSize = 13, BorderThickness = new Thickness(0, 1, 1, 1) };//"??" - #RJBug001
            gCmddw = new Button() { Margin = new Thickness(0), Padding = new Thickness(0, -5, 0, 0), Height = 10, Width = 11, MinWidth = 11, MaxWidth = 11, Content = "??", VerticalAlignment = VerticalAlignment.Top, FontWeight = FontWeights.Bold, FontSize = 13, BorderThickness = new Thickness(0, 1, 1, 1) };//"??" - #RJBug001
            gLabel = new Label();//#RJBug001

            MenuControlContainer = new Menu { Background = Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
            MenuControl = new MenuItem {Name="NSVMD"+uID, BorderThickness = new Thickness(2), BorderBrush = Brushes.Lime, Header = pButtonText, Foreground = Brushes.Lime, VerticalAlignment = VerticalAlignment.Stretch, FontWeight = FontWeights.Bold, FontSize = 13 };
            MenuControlContainer.Items.Add(MenuControl);

            MenuItem item;
            Separator separator;

            #region -- Divergence Options --
            MenuItem miDivOptions = new MenuItem { Header = "Divergence Options", Foreground = Brushes.Black, FontWeight = FontWeights.Normal };

            miDivOptions1 = new MenuItem { Header = UseOscHighLow ? "Adjust Divergence ON" : "Adjust Divergence OFF", Name = "btDivOptions_AdjustDiv", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDivOptions1.Click += ModifyDivOptionsSetting_Click;
            miDivOptions.Items.Add(miDivOptions1);

            miDivOptions2 = new MenuItem { Header = IncludeDoubleTopsAndBottoms ? "Include DT/DB ON" : "Include DT/DB OFF", Name = "btDivOptions_IncludeDTDB", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDivOptions2.Click += ModifyDivOptionsSetting_Click;
            miDivOptions.Items.Add(miDivOptions2);

            miDivOptions4 = new MenuItem { Header = ResetFilter ? "Reset Filter ON" : "Reset Filter OFF", Name = "btDivOptions_ResetFilter", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDivOptions4.Click += ModifyDivOptionsSetting_Click;
            miDivOptions.Items.Add(miDivOptions4);

            separator = new Separator();
            miDivOptions.Items.Add(separator);

            item = new MenuItem { Header = "        RELOAD THE CHART", Name = "btDivOptions_Reload", Foreground = Brushes.Black, StaysOpenOnClick = false };
            item.Click += ReloadChart_Click;
            miDivOptions.Items.Add(item);

            MenuControl.Items.Add(miDivOptions);
            #endregion

            #region -- MarketStructure --
            MenuItem miMarketStructure = new MenuItem { Header = "Swing Trend Parameters", Foreground = Brushes.Black, FontWeight = FontWeights.Normal };

            miMarketStructure1 = new MenuItem { Header = "Structure Swings " + (ShowZigzagLegs ? "ON" : "OFF"), Name = "btMarketStructure_trends", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miMarketStructure1.Click += MarketStructure_Click;
            miMarketStructure.Items.Add(miMarketStructure1);

            miMarketStructure2 = new MenuItem { Header = "Structure Labels " + (ShowZigzagLabels ? "ON" : "OFF"), Name = "btMarketStructure_labels", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miMarketStructure2.Click += MarketStructure_Click;
            miMarketStructure.Items.Add(miMarketStructure2);

            miMarketStructure.Items.Add(createMSTMenu(MultiplierDTB.ToString(), SwingStrength.ToString()));

            separator = new Separator();
            miMarketStructure.Items.Add(separator);

            MenuItem buttonMST = new MenuItem { Header = "RE-CALCULATE MARKET STRUCTURE", Name = "marketStructureClick", HorizontalAlignment = HorizontalAlignment.Center };
            buttonMST.Click += ReloadChart_Click;
            miMarketStructure.Items.Add(buttonMST);
            //------------------

            MenuControl.Items.Add(miMarketStructure);
            #endregion

            #region -- Display Options Divergences --
            MenuItem miDisplayOptions = new MenuItem { Header = "Display Options", Foreground = Brushes.Black, FontWeight = FontWeights.Normal };

            bool isMasterOn = ShowSetupDots || ShowDivOnPricePanel || ShowDivOnOscillatorPanel || ShowHistogramDivergences || ShowHistogramHiddenDivergences || ShowOscillatorDivergences || ShowOscillatorHiddenDivergences;
            item = new MenuItem { Header = "--- MASTER " + (isMasterOn ? "ON" : "OFF") + " ---", Name = "btDisplayOptions_Master", Foreground = Brushes.Black, StaysOpenOnClick = true };
            item.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(item);

            miDisplayOptions1 = new MenuItem { Header = "Show Div Dots " + (ShowSetupDots?"ON":"OFF"), Name = "btDisplayOptions_ShowDivDots", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions1.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions1);

            miDisplayOptions2 = new MenuItem { Header = "Show Div On Price Panel " + (ShowDivOnPricePanel ? "ON" : "OFF"), Name = "btDisplayOptions_ShowDivOnPricePanel", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions2.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions2);

            miDisplayOptions3 = new MenuItem { Header = "Show Div On Sub-Panel " + (ShowDivOnOscillatorPanel ? "ON" : "OFF"), Name = "btDisplayOptions_ShowDivOnSubPanel", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions3.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions3);

            miDisplayOptions4 = new MenuItem { Header = "Show Histogram Div " + (ShowHistogramDivergences ? "ON" : "OFF"), Name = "btDisplayOptions_ShowHistoDiv", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions4.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions4);

            miDisplayOptions5 = new MenuItem { Header = "Show Histogram Hidden Div " + (ShowHistogramHiddenDivergences ? "ON" : "OFF"), Name = "btDisplayOptions_ShowHistoHDiv", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions5.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions5);

            miDisplayOptions6 = new MenuItem { Header = "Show MACD Div " + (ShowOscillatorDivergences ? "ON" : "OFF"), Name = "btDisplayOptions_ShowMACDDiv", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions6.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions6);

            miDisplayOptions7 = new MenuItem { Header = "Show MACD Hidden Div " + (ShowOscillatorHiddenDivergences ? "ON" : "OFF"), Name = "btDisplayOptions_ShowMACDHDiv", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miDisplayOptions7.Click += ModifyDisplayOptionsSetting_Click;
            miDisplayOptions.Items.Add(miDisplayOptions7);

            separator = new Separator();
            miDisplayOptions.Items.Add(separator);

            item = new MenuItem { Header = "            RELOAD THE CHART", Name = "btDisplayOptions_Reload", Foreground = Brushes.Black, StaysOpenOnClick = false };
            item.Click += ReloadChart_Click;
            miDisplayOptions.Items.Add(item);

            MenuControl.Items.Add(miDisplayOptions);
            #endregion

            #region -- Excursion Levels --
            MenuItem miExcursionLevels = new MenuItem { Header = "Excursion Levels", Foreground = Brushes.Black, FontWeight = FontWeights.Normal };

            isMasterOn = DisplayLevel1 || DisplayLevel2 || DisplayLevel3;
            item = new MenuItem { Header = "--- MASTER " + (isMasterOn ? "ON" : "OFF") + " ---", Name = "btExcursionLevels_Master", Foreground = Brushes.Black, StaysOpenOnClick = true };
            item.Click += RefreshChart_Click;
            miExcursionLevels.Items.Add(item);

            miExcursionLevels1 = new MenuItem { Header = "Show Level 1 " + (DisplayLevel1 ? "ON" : "OFF"), Name = "btExcursionLevels_Level1", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miExcursionLevels1.Click += RefreshChart_Click;
            miExcursionLevels.Items.Add(miExcursionLevels1);

            miExcursionLevels2 = new MenuItem { Header = "Show Level 2 " + (DisplayLevel2 ? "ON" : "OFF"), Name = "btExcursionLevels_Level2", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miExcursionLevels2.Click += RefreshChart_Click;
            miExcursionLevels.Items.Add(miExcursionLevels2);

            miExcursionLevels3 = new MenuItem { Header = "Show Level 3 " + (DisplayLevel3 ? "ON" : "OFF"), Name = "btExcursionLevels_Level3", Foreground = Brushes.Black, StaysOpenOnClick = true };
            miExcursionLevels3.Click += RefreshChart_Click;
            miExcursionLevels.Items.Add(miExcursionLevels3);

            List<string> cbItems = new List<string>();
            foreach (var pt in Enum.GetValues(typeof(HistoDivergence_ExcursionStyle))) cbItems.Add(pt.ToString());            
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) });
            Label lbl1 = new Label() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0), Content = "Lines' Mode :" };
            lbl1.SetValue(Grid.ColumnProperty, 0);
            lbl1.SetValue(Grid.RowProperty, 0);
            comboExcursionStyle = new ComboBox() { Name = "comboExcursionStyle" + uID, MinWidth = 86, Width = 86, MaxWidth = 86, Margin = new Thickness(5, 0, 0, 0) };
            foreach (string cbitem in cbItems) comboExcursionStyle.Items.Add(cbitem);
            comboExcursionStyle.SelectedItem = PlotStyleLevels == PlotStyle.HLine ? HistoDivergence_ExcursionStyle.Static.ToString() : HistoDivergence_ExcursionStyle.Dynamic.ToString();
            comboExcursionStyle.SelectionChanged += ExcursionStyleValueChanged;
            comboExcursionStyle.SetValue(Grid.ColumnProperty, 1);
            comboExcursionStyle.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(lbl1);
            grid.Children.Add(comboExcursionStyle);
            miExcursionLevels.Items.Add(grid);

            MenuControl.Items.Add(miExcursionLevels);
            #endregion

            #region -- Sentiment --
            MenuItem miSentiment = new MenuItem { Header = "Sentiment Settings", Foreground = Brushes.Black, FontWeight = FontWeights.Normal };

            item = new MenuItem { Header = "Show Histogram Sentiment " + (ShowSentimentInBox ? "ON" : "OFF"), Name = "btSentiment_Sentiment", Foreground = Brushes.Black, StaysOpenOnClick = true };
            item.Click += RefreshChart_Click;
            miSentiment.Items.Add(item);

            item = new MenuItem { Header = "Show Structure Bias " + (ShowBiasInBox ? "ON" : "OFF"), Name = "btSentiment_Bias", Foreground = Brushes.Black, StaysOpenOnClick = true };
            item.Click += RefreshChart_Click;
            miSentiment.Items.Add(item);

            cbItems = new List<string>();
            foreach (var pt in Enum.GetValues(typeof(HistoDivergence_Flooding))) cbItems.Add(pt.ToString());
            grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) });
            lbl1 = new Label() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0), Content = "Flooding :" };
            lbl1.SetValue(Grid.ColumnProperty, 0);
            lbl1.SetValue(Grid.RowProperty, 0);
            comboFlooding = new ComboBox() { Name = "comboFlooding" + uID, MinWidth = 86, Width = 86, MaxWidth = 86, Margin = new Thickness(5, 0, 0, 0) };
            foreach (string cbitem in cbItems) comboFlooding.Items.Add(cbitem);
            comboFlooding.SelectedItem = BackgroundFlooding.ToString();
            comboFlooding.SelectionChanged += FloodingValueChanged;
            comboFlooding.SetValue(Grid.ColumnProperty, 1);
            comboFlooding.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(lbl1);
            grid.Children.Add(comboFlooding);
            miSentiment.Items.Add(grid);

            MenuControl.Items.Add(miSentiment);
            #endregion

            indytoolbar.Children.Add(MenuControlContainer);
        }
        #endregion

        private Grid createMSTMenu(string nudValue1, string nudValue2)
        {
            const int rHeight = 26;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(11) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rHeight / 2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rHeight / 2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rHeight / 2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rHeight / 2) });

            //line 1 - MST1
            Label lbl1 = new Label() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0), Content = "Sensitivity :" };
            lbl1.SetValue(Grid.ColumnProperty, 0);
            lbl1.SetValue(Grid.RowProperty, 0);
            lbl1.SetValue(Grid.RowSpanProperty, 2);

            nudMST1 = new TextBox() { Name = "MST1txtbox" + uID, MinWidth = 60, Width = 60, MaxWidth = 60, Height = 20, Margin = new Thickness(5, 0, 0, 0), BorderBrush = gLabel.Foreground };
            nudMST1.Text = nudValue1;
            nudMST1.KeyDown += menuTxtbox_KeyDown;
            nudMST1.TextChanged += NumericUpDownValueChanged;
            nudMST1.SetValue(Grid.ColumnProperty, 1);
            nudMST1.SetValue(Grid.ColumnSpanProperty, 2);
            nudMST1.SetValue(Grid.RowProperty, 0);
            nudMST1.SetValue(Grid.RowSpanProperty, 2);
            
            //line 2 - MST2
            Label lbl2 = new Label() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0), Content = "Strength : " };
            lbl2.SetValue(Grid.ColumnProperty, 0);
            lbl2.SetValue(Grid.RowProperty, 2);
            lbl2.SetValue(Grid.RowSpanProperty, 2);

            nudMST2 = new TextBox() { Name = "MST2txtbox" + uID, MinWidth = 50, Width = 50, MaxWidth = 50, Height = 20, Margin = new Thickness(5, 0, 0, 0), BorderBrush = gLabel.Foreground };
            nudMST2.Text = nudValue2;
            nudMST2.KeyDown += menuTxtbox_KeyDown;
            nudMST2.TextChanged += NumericUpDownValueChanged;
            nudMST2.SetValue(Grid.ColumnProperty, 1);
            nudMST2.SetValue(Grid.RowProperty, 2);
            nudMST2.SetValue(Grid.RowSpanProperty, 2);

            Button cmdup2 = new Button() { Name = "MST2cmdup" + uID, Margin = gCmdup.Margin, Padding = gCmdup.Padding, Height = gCmdup.Height, Width = gCmdup.Width, MinWidth = gCmdup.MinWidth, MaxWidth = gCmdup.MaxWidth, Content = gCmdup.Content, VerticalAlignment = gCmdup.VerticalAlignment, FontWeight = gCmdup.FontWeight, FontSize = gCmdup.FontSize, Foreground = gLabel.Foreground, Background = gLabel.Background, BorderThickness = gCmdup.BorderThickness, BorderBrush = gLabel.Foreground };
            Button cmddw2 = new Button() { Name = "MST2cmddw" + uID, Margin = gCmddw.Margin, Padding = gCmddw.Padding, Height = gCmddw.Height, Width = gCmddw.Width, MinWidth = gCmddw.MinWidth, MaxWidth = gCmddw.MaxWidth, Content = gCmddw.Content, VerticalAlignment = gCmddw.VerticalAlignment, FontWeight = gCmddw.FontWeight, FontSize = gCmddw.FontSize, Foreground = gLabel.Foreground, Background = gLabel.Background, BorderThickness = gCmddw.BorderThickness, BorderBrush = gLabel.Foreground };
            cmdup2.Click += cmdupdw_Click;
            cmdup2.SetValue(Grid.ColumnProperty, 2);
            cmdup2.SetValue(Grid.RowProperty, 2);
            cmddw2.Click += cmdupdw_Click;
            cmddw2.SetValue(Grid.ColumnProperty, 2);
            cmddw2.SetValue(Grid.RowProperty, 3);

            grid.Children.Add(lbl1);
            grid.Children.Add(nudMST1);
            grid.Children.Add(lbl2);
            grid.Children.Add(nudMST2);
            grid.Children.Add(cmdup2);
            grid.Children.Add(cmddw2);

            return grid;
        }

        //---------- Events ------------------------------------------------
        #region private void TabSelectionChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        private void TabSelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            TabItem tabItem = e.AddedItems[0] as TabItem;
            if (tabItem == null) return;
            ChartTab temp = tabItem.Content as ChartTab;
            if (temp != null && indytoolbar != null)
                indytoolbar.Visibility = temp.ChartControl == ChartControl ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region -- DoubleEditKeyPress -- 
        private void menuTxtbox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            TextBox txtSender = sender as TextBox;

            int keyVal = (int)e.Key;
            int value = -1;
            if (keyVal >= (int)Key.D0 && keyVal <= (int)Key.D9) value = keyVal - (int)Key.D0;
            else if (keyVal >= (int)Key.NumPad0 && keyVal <= (int)Key.NumPad9) value = keyVal - (int)Key.NumPad0;

            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);

            if (isNumeric || e.Key == Key.Back || (e.Key == Key.Decimal && txtSender.Name== "MST1txtbox"))
            {
                string newText = value != -1 ? value.ToString() : e.Key == Key.Decimal ? System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : "";
                int tbPosition = txtSender.SelectionStart;
                txtSender.Text = txtSender.SelectedText == "" ? txtSender.Text.Insert(tbPosition, newText) : txtSender.Text.Replace(txtSender.SelectedText, newText);
                txtSender.Select(tbPosition + 1, 0);
            }
        }
        #endregion

        #region -- DoubleEditTextChanged --
        private string currentTextEdit;
        private void NumericUpDownValueChanged(object sender, EventArgs e)
        {
            TextBox txtSender = sender as TextBox;
            if (txtSender.Text.Length > 0)
            {
                float result;
                bool isNumeric = float.TryParse(txtSender.Text, out result);

                if (isNumeric) currentTextEdit = txtSender.Text;
                else
                {
                    txtSender.Text = currentTextEdit;
                    txtSender.Select(txtSender.Text.Length, 0);
                }
                if (txtSender.Name == "MST1txtbox") MultiplierDTB = Convert.ToDouble(nudMST1.Text);
                else if (txtSender.Name == "MST2txtbox") SwingStrength = Convert.ToInt16(nudMST2.Text);
            }
        }
        #endregion

        #region private void cmdupdw_Click(object sender, RoutedEventArgs e)
        private void cmdupdw_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = sender as Button;
            if (cmd.Name.Contains("MST2cmdup")) nudMST2.Text = (Math.Min(999999999, Convert.ToInt32(nudMST2.Text) + 1)).ToString();
            else if (cmd.Name.Contains("MST2cmddw")) nudMST2.Text = (Math.Max(1, Convert.ToInt32(nudMST2.Text) - 1)).ToString();
        }
        #endregion

        #region -- MarketStructure_Click --
        private void MarketStructure_Click(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;

            //----------- Divergences Options -------------------
            if (item != null && item.Name == "btMarketStructure_trends") ShowZigzagLegs = !ShowZigzagLegs;
            else if (item != null && item.Name == "btMarketStructure_labels") ShowZigzagLabels = !ShowZigzagLabels;

            miMarketStructure1.Header = "Structure Swings " + (ShowZigzagLegs ? "ON" : "OFF");
            miMarketStructure2.Header = "Structure Labels " + (ShowZigzagLabels ? "ON" : "OFF");
        }

        #endregion

        #region -- FloodingValueChanged --
        private void FloodingValueChanged(object sender, EventArgs e)
        {
            HistoDivergence_Flooding backgroundFlooding;
            Enum.TryParse((string)comboFlooding.SelectedItem, out backgroundFlooding);
            BackgroundFlooding = backgroundFlooding;
            RefreshChart_Click(null, null);
        }
        #endregion

        #region -- ExcursionStyleValueChanged --
        private void ExcursionStyleValueChanged(object sender, EventArgs e)
        {
            PlotStyleLevels = (string)comboExcursionStyle.SelectedItem == HistoDivergence_ExcursionStyle.Static.ToString() ? PlotStyle.HLine : PlotStyle.Line;
            RefreshChart_Click(null, null);
        }
        #endregion

        #region -- RefreshChart_Click --
        private void RefreshChart_Click(object sender, EventArgs e)
        {
            if (sender != null)
            {
                MenuItem item = sender as MenuItem;

                //----------- Excursion Levels -------------------
                if (item.Name.Contains("btExcursionLevels"))
                {
                    if (item.Name == "btExcursionLevels_Level1") DisplayLevel1 = !DisplayLevel1;
                    else if (item.Name == "btExcursionLevels_Level2") DisplayLevel2 = !DisplayLevel2;
                    else if (item.Name == "btExcursionLevels_Level3") DisplayLevel3 = !DisplayLevel3;
                    else if (item.Name == "btExcursionLevels_Master")
                    {
                        bool master = ((string)(item.Header)).Contains("ON");
                        master = !master;
                        item.Header = "--- MASTER " + (master ? "ON" : "OFF") + " ---";

                        DisplayLevel1 = master;
                        DisplayLevel2 = master;
                        DisplayLevel3 = master;
                    }

                    miExcursionLevels1.Header = "Show Level 1 " + (DisplayLevel1 ? "ON" : "OFF");
                    miExcursionLevels2.Header = "Show Level 2 " + (DisplayLevel2 ? "ON" : "OFF");
                    miExcursionLevels3.Header = "Show Level 3 " + (DisplayLevel3 ? "ON" : "OFF");
                }
            }

            ChartControl.InvalidateVisual();
        }
        #endregion

        #region -- ModifyDivOptionsSetting_Click --
        private void ModifyDivOptionsSetting_Click(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;

            //----------- Divergences Options -------------------
            if (item != null && item.Name == "btDivOptions_AdjustDiv") UseOscHighLow = !UseOscHighLow;
            else if (item != null && item.Name == "btDivOptions_IncludeDTDB") IncludeDoubleTopsAndBottoms = !IncludeDoubleTopsAndBottoms;
            else if (item != null && item.Name == "btDivOptions_ResetFilter") ResetFilter = !ResetFilter;

            miDivOptions1.Header = "Adjust Divergence " + (UseOscHighLow ? "ON" : "OFF");
            miDivOptions2.Header = "Include DT/DB " + (IncludeDoubleTopsAndBottoms ? "ON" : "OFF");
            miDivOptions4.Header = "Reset Filter " + (ResetFilter ? "ON" : "OFF");
        }
        #endregion

        #region -- ReloadChart_Click --
        private void ReloadChart_Click(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;

            //----------- Market Structure -------------------
            if (item.Name == "marketStructureClick")
            {
                try
                {
                    MultiplierDTB = Math.Max(0, Convert.ToDouble(nudMST1.Text));
                    SwingStrength = Math.Max(1, Convert.ToInt32(nudMST2.Text));
                }
                catch (Exception){ }
            }

            System.Windows.Forms.SendKeys.SendWait("{F5}");
        }
        #endregion

        #region -- ModifyDisplayOptionsSetting_Click --
        private void ModifyDisplayOptionsSetting_Click(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;

            //----------- Display Options -------------------
            if (item.Name == "btDisplayOptions_ShowDivDots") ShowSetupDots = !ShowSetupDots;
            else if (item.Name == "btDisplayOptions_ShowDivOnPricePanel") ShowDivOnPricePanel = !ShowDivOnPricePanel;
            else if (item.Name == "btDisplayOptions_ShowDivOnSubPanel") ShowDivOnOscillatorPanel = !ShowDivOnOscillatorPanel;
            else if (item.Name == "btDisplayOptions_ShowHistoDiv") ShowHistogramDivergences = !ShowHistogramDivergences;
            else if (item.Name == "btDisplayOptions_ShowHistoHDiv") ShowHistogramHiddenDivergences = !ShowHistogramHiddenDivergences;
            else if (item.Name == "btDisplayOptions_ShowMACDDiv") ShowOscillatorDivergences = !ShowOscillatorDivergences;
            else if (item.Name == "btDisplayOptions_ShowMACDHDiv") ShowOscillatorHiddenDivergences = !ShowOscillatorHiddenDivergences;
            else if (item.Name == "btDisplayOptions_Master")
            {
                bool master = ((string)(item.Header)).Contains("ON");
                master = !master;
                item.Header = "--- MASTER " + (master ? "ON" : "OFF") + " ---";

                ShowSetupDots = master;
                ShowDivOnPricePanel = master;
                ShowDivOnOscillatorPanel = master;
                ShowHistogramDivergences = master;
                ShowHistogramHiddenDivergences = master;
                ShowOscillatorDivergences = master;
                ShowOscillatorHiddenDivergences = master;
            }

            //-----------Market Structure------------------ -
            else if (item.Name == "marketStructureClick")
            {
                MultiplierDTB = Convert.ToDouble(nudMST1.Text);
                SwingStrength = Convert.ToInt16(nudMST2.Text);
            }

            miDisplayOptions1.Header = "Show Div Dots " + (ShowSetupDots ? "ON" : "OFF");
            miDisplayOptions2.Header = "Show Div On Price Panel " + (ShowDivOnPricePanel ? "ON" : "OFF");
            miDisplayOptions3.Header = "Show Div On Sub-Panel " + (ShowDivOnOscillatorPanel ? "ON" : "OFF");
            miDisplayOptions4.Header = "Show Histogram Div " + (ShowHistogramDivergences ? "ON" : "OFF");
            miDisplayOptions5.Header = "Show Histogram Hidden Div " + (ShowHistogramHiddenDivergences ? "ON" : "OFF");
            miDisplayOptions6.Header = "Show MACD Div " + (ShowOscillatorDivergences ? "ON" : "OFF");
            miDisplayOptions7.Header = "Show MACD Hidden Div " + (ShowOscillatorHiddenDivergences ? "ON" : "OFF");
        }
        #endregion

        #endregion

		[STAThreadAttribute]
        protected override void OnStateChange()
        {
            #region State == State.SetDefaults  
            if (State == State.SetDefaults)
            {
                Description = @"Measures 2 components of momentum:  1) the velocity histogram measures minor cycles of Multiple timeframe momentum cycles and act as a leading guage of momentum before a trend has started  2)(Moving Average Convergence/Divergence) is a trend following momentum indicator that shows the relationship between two moving averages of prices.";
                Name = "HistoDivergence";
                ArePlotsConfigurable = false;
                DrawOnPricePanel = false;
                AreLinesConfigurable = false;
                DrawOnPricePanel = false;
                PaintPriceMarkers = false;
                ZOrder = -1;
                MaximumBarsLookBack = MaximumBarsLookBack.Infinite;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = false;
                Calculate = Calculate.OnEachTick;
                IsOverlay = false;
                DisplayInDataBox = true;

                uID = "HistoDiv"+DateTime.Now.Ticks.ToString();//prevent multiple toolbar with same name
				pButtonText = "HDiv";

                #region INIT Plots / Lines 

                //rj RJ5GROUP code Algorithm
                AddPlot(new Stroke(Brushes.Red), PlotStyle.HLine, "PriceExcursionLL3");
                AddPlot(new Stroke(Brushes.Blue), PlotStyle.HLine, "PriceExcursionLL2");
                AddPlot(new Stroke(Brushes.Black), PlotStyle.HLine, "PriceExcursionLL1");
                AddPlot(new Stroke(Brushes.Black), PlotStyle.HLine, "PriceExcursionUL1");
                AddPlot(new Stroke(Brushes.Blue), PlotStyle.HLine, "PriceExcursionUL2");
                AddPlot(new Stroke(Brushes.Red), PlotStyle.HLine, "PriceExcursionUL3");
                //rj RJ5GROUP code Algorithm

                //AddLine(Brushes.Gray, 0, "Zeroline");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Bar, "Histogram");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Line, "Lower");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Line, "Upper");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Line, "Average");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Line, "BBMACDLine");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Dot, "BBMACDFrame");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Dot, "BBMACD");

                AddPlot(new Stroke(Brushes.Black,0.1f), PlotStyle.Line, "PriceExcursionMAX");
                AddPlot(new Stroke(Brushes.Black, 0.1f), PlotStyle.Line, "PriceExcursionMIN");
                #endregion

                #region INIT Properties to Default Values 

				#region MACDBB Parameters
				BandPeriod = 10;
				Fast = 12;
				Slow = 26;
				StdDevNumber = 1.0;
				#endregion

                #region SwingTrend Parameters 
                SwingStrength = 3;
                MultiplierMD = 0.0;
                MultiplierDTB = 0.0;
                #endregion

                #region Divergence Options 
                ThisInputType = HistoDivergence_InputType.High_Low;
                UseOscHighLow = false;
                IncludeDoubleTopsAndBottoms = true;
                ResetFilter = true;
                #endregion

                #region Divergence Parameters 
                DivMaxBars = 55;
                DivMinBars = 3;
                TriggerBars = 20;
                #endregion

                #region Display Options Oscillator Panel 
                //BackgroundFloodingPanel = gztFloodPanel.None;//##MODIF## AzurITec - 18.05.2016
                ShowBiasInBox = true;
                ShowSentimentInBox = true;
                BackgroundFlooding = HistoDivergence_Flooding.None;
                #endregion

                #region Display Options Swing Trend 
                SwingDotSize = 7;
                LabelFontSize = 10;
                ShowZigzagDots = false;
                ShowZigzagLabels = false;
                ShowZigzagLegs = false;
                SwingLegWidth = 2;
                SwingLegStyle = DashStyleHelper.Solid;
                #endregion

                #region Display Options Divergences 
                ShowDivOnPricePanel = true;
                ShowDivOnOscillatorPanel = true;
                ShowOscillatorDivergences = true;
                ShowHistogramDivergences = true;
                ShowOscillatorHiddenDivergences = true;
                ShowHistogramHiddenDivergences = true;
                ShowSetupDots = true;
                #endregion
				pShowEntryPrice = true;
				

                //rj RJ5GROUP code Algorithm
                #region Display Options Price Excursions 
                DisplayLevel1 = false;
                DisplayLevel2 = true;
                DisplayLevel3 = true;
                #endregion

				OptimizeSpeed = HistoDivergence_OptimizeSpeedSettings.Max;

                #region Plot Parameters 
                DotSize = 2;
                Plot2Width = 2;
                Plot3Width = 2;
                Plot4Width = 2;
                Dash3Style = DashStyleHelper.Dot;
                Dash4Style = DashStyleHelper.Solid;
                ZerolineWidth = 1;
                ZerolineStyle = DashStyleHelper.Solid;
                MomoWidth = 6;

                DeepBullishBackgroundColor = Brushes.DarkGreen;
                BullishBackgroundColor = Brushes.Green;
                OppositeBackgroundColor = Brushes.Gray;
                BearishBackgroundColor = Brushes.Red;
                DeepBearishBackgroundColor = Brushes.DarkRed;
                BackgroundOpacity = 30;//##DEFAULT VALUE TO CHECK##
                ChannelColor = Brushes.DodgerBlue;
                ChannelOpacity = 20;
                #endregion

                #region Plot Colors 
                DotsUpRisingColor = Brushes.Green;
                DotsDownRisingColor = Brushes.Green;
                DotsDownFallingColor = Brushes.Red;
                DotsUpFallingColor = Brushes.Red;
                DotsRimColor = Brushes.Black;
                BBAverageColor = Brushes.Transparent;
                BBUpperColor = Brushes.Black;
                BBLowerColor = Brushes.Black;
                HistUpColor = Brushes.LimeGreen;
                HistDownColor = Brushes.Maroon;
                ZerolineColor = Brushes.Black;
                ConnectorColor = Brushes.White;
                #endregion

				pBuySignalRDivWAV = "<inst>_BuyEntry.wav";
				pSellSignalRDivWAV = "<inst>_SellEntry.wav";
				pBuySignalHDivWAV = "<inst>_BuyEntry.wav";
				pSellSignalHDivWAV = "<inst>_SellEntry.wav";
                //rj RJ5GROUP code Algorithm
                #region Price Excursion: Plot Colors / Parameters 
                Level1Color = Brushes.WhiteSmoke;
                Level2Color = Brushes.Blue;
                Level3Color = Brushes.Red;
                PlotStyleLevels = PlotStyle.HLine;
                DashStyleHelperLevels = DashStyleHelper.Solid;
                PlotWidthLevels = 3;
                #endregion

                #region MACD Divergences: Plot Parameters 
                DivWidth1 = 3;
                OffsetMultiplier3 = 10;
                OffsetMultiplier1 = 40;
                TriangleFontSize1 = 25;
                SetupFontSize1 = 6;
                DivWidth2 = 3;
                OffsetMultiplier4 = 30;
                OffsetMultiplier2 = 65;
                TriangleFontSize2 = 25;
                SetupFontSize2 = 6;

                HiddenDivWidth1 = 3;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenTriangleFontSize1 = 25;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenSetupFontSize1 = 6;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenDivWidth2 = 3;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenTriangleFontSize2 = 25;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenSetupFontSize2 = 6;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenOffsetMultiplier1 = 40;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenOffsetMultiplier2 = 90;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenOffsetMultiplier3 = 10;//#HIDDENDIV - added 17.02.01 - AzurITec
                HiddenOffsetMultiplier4 = 30;//#HIDDENDIV - added 17.02.01 - AzurITec
                #endregion

                #region MACD Divergences: Plot Colors 
                BearColor1 = Brushes.DarkRed;
                BullColor1 = Brushes.DarkGreen;
                ArrowDownColor1 = Brushes.DarkRed;
                ArrowUpColor1 = Brushes.DarkGreen;
                BearishSetupColor1 = Brushes.DarkRed;
                BullishSetupColor1 = Brushes.DarkGreen;

                HiddenBearColor1 = Brushes.DarkTurquoise;
                HiddenBullColor1 = Brushes.DarkBlue;
                HiddenArrowDownColor1 = Brushes.DarkTurquoise;
                HiddenArrowUpColor1 = Brushes.DarkBlue;
                HiddenBearishSetupColor1 = Brushes.DarkTurquoise;
                HiddenBullishSetupColor1 = Brushes.DarkBlue;
                #endregion

                #region Histogram Divergences: Plot Colors 
                BearColor2 = Brushes.Red;
                BullColor2 = Brushes.Lime;
                ArrowDownColor2 = Brushes.Red;
                ArrowUpColor2 = Brushes.Lime;
                BearishSetupColor2 = Brushes.Red;
                BullishSetupColor2 = Brushes.Lime;

                HiddenBearColor2 = Brushes.Cyan;
                HiddenBullColor2 = Brushes.Blue;
                HiddenArrowDownColor2 = Brushes.Cyan;
                HiddenArrowUpColor2 = Brushes.Blue;
                HiddenBearishSetupColor2 = Brushes.Cyan;
                HiddenBullishSetupColor2 = Brushes.Blue;
                #endregion

                #endregion
            }
            #endregion

            #region State == State.Configure  
            else if (State == State.Configure)
            {
				IsVisible = true;
                #region INIT Fonts  
                labelFont = new SimpleFont("Arial", LabelFontSize);
                swingDotFont = new SimpleFont("Webdings", SwingDotSize) { Bold = true };
                triangleFont1 = new SimpleFont("Webdings", TriangleFontSize1);
                triangleFont2 = new SimpleFont("Webdings", TriangleFontSize2);
                setupFont1 = new SimpleFont("Webdings", SetupFontSize1);
                setupFont2 = new SimpleFont("Webdings", SetupFontSize2);
                #endregion

                #region -- Set default to plots --
                Plots[BBMACDIDX].Width = DotSize;
                Plots[BBMACDIDX].DashStyleHelper = DashStyleHelper.Dot;
                Plots[BBMACDFrameIDX].Width = DotSize + 1;
                Plots[BBMACDFrameIDX].DashStyleHelper = DashStyleHelper.Dot;
                Plots[BBMACDLineIDX].Width = Plot2Width;
                Plots[AverageIDX].Width = Plot3Width;
                Plots[AverageIDX].DashStyleHelper = Dash3Style;
                Plots[UpperIDX].Width = Plot4Width;
                Plots[UpperIDX].DashStyleHelper = Dash4Style;
                Plots[LowerIDX].Width = Plot4Width;
                Plots[LowerIDX].DashStyleHelper = Dash4Style;
                Plots[HistogramIDX].Width = MomoWidth;
                Plots[HistogramIDX].DashStyleHelper = DashStyleHelper.Solid;

                //rj RJ5GROUP code Algorithm
                if (false)
                {
                    Plots[PriceExcursionUL3IDX].Width = PlotWidthLevels;
                    Plots[PriceExcursionUL3IDX].DashStyleHelper = DashStyleHelper.Solid;
                    Plots[PriceExcursionUL3IDX].PlotStyle = PlotStyleLevels;
                    Plots[PriceExcursionUL2IDX].Width = PlotWidthLevels;
                    Plots[PriceExcursionUL2IDX].DashStyleHelper = DashStyleHelper.Solid;
                    Plots[PriceExcursionUL2IDX].PlotStyle = PlotStyleLevels;
                    Plots[PriceExcursionUL1IDX].Width = PlotWidthLevels;
                    Plots[PriceExcursionUL1IDX].DashStyleHelper = DashStyleHelper.Solid;
                    Plots[PriceExcursionUL1IDX].PlotStyle = PlotStyleLevels;
                    Plots[PriceExcursionLL1IDX].Width = PlotWidthLevels;
                    Plots[PriceExcursionLL1IDX].DashStyleHelper = DashStyleHelper.Solid;
                    Plots[PriceExcursionLL1IDX].PlotStyle = PlotStyleLevels;
                    Plots[PriceExcursionLL2IDX].Width = PlotWidthLevels;
                    Plots[PriceExcursionLL2IDX].DashStyleHelper = DashStyleHelper.Solid;
                    Plots[PriceExcursionLL2IDX].PlotStyle = PlotStyleLevels;
                    Plots[PriceExcursionLL3IDX].Width = PlotWidthLevels;
                    Plots[PriceExcursionLL3IDX].DashStyleHelper = DashStyleHelper.Solid;
                    Plots[PriceExcursionLL3IDX].PlotStyle = PlotStyleLevels;
                }
                //rj RJ5GROUP code Algorithm

                //Lines[0].Width = ZerolineWidth;
                //Lines[0].DashStyleHelper = ZerolineStyle;
                //Lines[0].Brush = ZerolineColor;

                Plots[BBMACDFrameIDX].Brush = DotsRimColor;
                Plots[BBMACDLineIDX].Brush = ConnectorColor;
                Plots[AverageIDX].Brush = BBAverageColor;
                Plots[UpperIDX].Brush = BBUpperColor;
                Plots[LowerIDX].Brush = BBLowerColor;

                //rj RJ5GROUP code Algorithm
                Plots[PriceExcursionUL3IDX].Brush = Brushes.Transparent;
                Plots[PriceExcursionUL2IDX].Brush = Brushes.Transparent;
                Plots[PriceExcursionUL1IDX].Brush = Brushes.Transparent;
                Plots[PriceExcursionLL1IDX].Brush = Brushes.Transparent;
                Plots[PriceExcursionLL2IDX].Brush = Brushes.Transparent;
                Plots[PriceExcursionLL3IDX].Brush = Brushes.Transparent;
                
                Plots[PriceExcursionMAXIDX].Brush = Level3Color;
                Plots[PriceExcursionMINIDX].Brush = Level3Color;
                //rj RJ5GROUP code Algorithm
                #endregion
            }
            #endregion

			#region -- DataLoaded --
			else if (State == State.DataLoaded){
                #region INIT Series  
                swingInput = new Series<double>(this);
                pre_swingHighType = new Series<int>(this);
                pre_swingLowType = new Series<int>(this);
                swingHighType = new Series<int>(this);
                swingLowType = new Series<int>(this);
                acceleration1 = new Series<int>(this);
                acceleration2 = new Series<int>(this);
                upTrend = new Series<bool>(this);

                bbDotTrend = new Series<double>(this);
                momoSignum = new Series<double>(this);

                bearishPDivMACD = new Series<double>(this);
                bullishPDivMACD = new Series<double>(this);
                bearishPDivHistogram = new Series<double>(this);
                bullishPDivHistogram = new Series<double>(this);

                bearishCDivMACD = new Series<double>(this);
                bullishCDivMACD = new Series<double>(this);
                bearishCDivHistogram = new Series<double>(this);
                bullishCDivHistogram = new Series<double>(this);

                bearishTriggerCountMACDBB = new Series<int>(this);
                bullishTriggerCountMACDBB = new Series<int>(this);
                bearishTriggerCountHistogram = new Series<int>(this);
                bullishTriggerCountHistogram = new Series<int>(this);

                macdBBState = new Series<double>(this);
                histogramState = new Series<double>(this);

                hiddenbearishPDivMACD = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbullishPDivMACD = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbearishCDivMACD = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbullishCDivMACD = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbearishTriggerCountMACDBB = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbullishTriggerCountMACDBB = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec

                hiddenbearishPDivHistogram = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbullishPDivHistogram = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbearishCDivHistogram = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbullishCDivHistogram = new Series<double>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbearishTriggerCountHistogram = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                hiddenbullishTriggerCountHistogram = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec

                bearishDivPlotSeriesMACDBB = new Series<int>(this);
                hiddenbearishDivPlotSeriesMACDBB = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                bullishDivPlotSeriesMACDBB = new Series<int>(this);
                hiddenbullishDivPlotSeriesMACDBB = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                bearishDivPlotSeriesHistogram = new Series<int>(this);
                hiddenbearishDivPlotSeriesHistogram = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec
                bullishDivPlotSeriesHistogram = new Series<int>(this);
                hiddenbullishDivPlotSeriesHistogram = new Series<int>(this);//#HIDDENDIV - added 17.02.01 - AzurITec

                structureBiasState = new Series<double>(this, MaximumBarsLookBack.Infinite);//#STRBIAS
                #endregion

                #region INIT external Series  
                BMACD = MACD(Input, Fast, Slow, BandPeriod);
                SDBB = StdDev(BMACD, BandPeriod);
                MACD1 = MACD(8, 20, 20);
                MACD2 = MACD(10, 20, 20);
                MACD3 = MACD(20, 60, 20);
                MACD4 = MACD(60, 240, 20);
                avgTrueRange = ATR(256);
                ExcursionSeries = SMA(ATR(256), 65);//rj RJ5GROUP code Algorithm
                #endregion

			}
			#endregion

            #region State == State.Historical  
            else if (State == State.Historical)
            {
                #region -- Add Custom Toolbar --
                if (!isToolBarButtonAdded && ChartControl != null)
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                        if (chartWindow == null) return;

                        foreach (DependencyObject item in chartWindow.MainMenu) if (AutomationProperties.GetAutomationId(item) == (toolbarname + uID)) isToolBarButtonAdded = true;

                        if (!isToolBarButtonAdded)
                        {
                            indytoolbar = new Grid { Visibility = Visibility.Collapsed };

                            addToolBar();

                            chartWindow.MainMenu.Add(indytoolbar);
                            chartWindow.MainTabControl.SelectionChanged += TabSelectionChangedHandler;
                            
                            foreach (TabItem tab in chartWindow.MainTabControl.Items) if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem) indytoolbar.Visibility = Visibility.Visible;
                            AutomationProperties.SetAutomationId(indytoolbar, toolbarname + uID);
                        }
                    }));
                #endregion
            }
            #endregion

            #region State == State.Terminated
            else if (State == State.Terminated)
            {
				if (chartWindow != null && indytoolbar != null)
				{
					if (ChartControl.Dispatcher.CheckAccess()) {
						try{	chartWindow.MainMenu.Remove(indytoolbar);}catch{}
						indytoolbar = null;
						try{	chartWindow.MainTabControl.SelectionChanged -= TabSelectionChangedHandler;}catch{}
						chartWindow = null;
					}else{
						Dispatcher.BeginInvoke(new Action(() =>
						{
							try{	chartWindow.MainMenu.Remove(indytoolbar);}catch{}
							indytoolbar = null;
							try{	chartWindow.MainTabControl.SelectionChanged -= TabSelectionChangedHandler;}catch{}
							chartWindow = null;
						}));
					}
				}

            }
            #endregion
        }

        protected override void OnBarUpdate()
        {
line=1999;
try{
line=2009;
            #region --- Calculate VelocityMomo ---  
            double macdValue = BMACD[0];
            double average = BMACD.Avg[0];
            double stdDevValue = SDBB[0];

            BBMACD[0] = macdValue;
            BBMACDLine[0] = macdValue;
            BBMACDFrame[0] = macdValue;//used for drawing only
            Average[0] = average;
            Upper[0] = average + StdDevNumber * stdDevValue;
            Lower[0] = average - StdDevNumber * stdDevValue;
            Histogram[0] = MACD1.Diff[0] + MACD2.Diff[0] + MACD3.Diff[0] + MACD4.Diff[0];
            #endregion
            #region -- Color BB + Histo plots --    
            if (CurrentBar > 0)
            {
                #region -- Histogram --
                if (Histogram[0] > 0)
                {
                    PlotBrushes[HistogramIDX][0] = HistUpColor;
                    momoSignum[0] = 1.0;
                }
                else if (Histogram[0] < 0)
                {
                    PlotBrushes[HistogramIDX][0] = HistDownColor;
                    momoSignum[0] = -1.0;
                }
                else
                {
                    PlotBrushes[HistogramIDX][0] = PlotBrushes[HistogramIDX][1];
                    momoSignum[0] = momoSignum[1];
                }
                #endregion
                #region -- MACD --
				bbDotTrend[0] = 0.0;
                if (IsRising(BBMACD))
                {
                    if (BBMACD[0] > Upper[0])
                    {
                        bbDotTrend[0] = 2.0;
                        PlotBrushes[BBMACDIDX][0] = DotsUpRisingColor;
                    }
                    else
                    {
                        bbDotTrend[0] = 1.0;
                        PlotBrushes[BBMACDIDX][0] = DotsDownRisingColor;
                    }
                }
                else if (IsFalling(BBMACD))
                {
                    if (BBMACD[0] < Lower[0])
                    {
                        bbDotTrend[0] = -2.0;
                        PlotBrushes[BBMACDIDX][0] = DotsDownFallingColor;
                    }
                    else
                    {
                        bbDotTrend[0] = -1.0;
                        PlotBrushes[BBMACDIDX][0] = DotsUpFallingColor;
                    }
                }
                else
                {
                    bbDotTrend[0] = bbDotTrend[1];
                    PlotBrushes[BBMACDIDX][0] = PlotBrushes[BBMACDIDX][1];
                }
                #endregion
            }
            #endregion
line=2079;
//            bearishMACDDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bullishMACDDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bearishHistogramDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bullishHistogramDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bearishMACDHiddenDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bullishMACDHiddenDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bearishHistogramHiddenDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec
//            bullishHistogramHiddenDivProjection[0] = (0.0);//#BLOOHOUND - added 17.02.03 - AzurITec

line=2089;
            //rj RJ5GROUP code Algorithm
            #region --- Calculate ExcurionValue ---  
            //Excursion Algorithm
            double ExcursionValue = ExcursionSeries[0];

            momoSignum[0] = 0.0;

            //Plots
            PriceExcursionUL1[0] = ExcursionValue * 1;
            PriceExcursionLL1[0] = ExcursionValue * -1;
            PriceExcursionUL2[0] = ExcursionValue * 2;
            PriceExcursionLL2[0] = ExcursionValue * -2;
            PriceExcursionUL3[0] = ExcursionValue * 3;
            PriceExcursionLL3[0] = ExcursionValue * -3;
            if (CurrentBar > 0) PriceExcursionMAX.Reset(1);
            PriceExcursionMAX[0] = ExcursionValue * 3;
            if (CurrentBar > 0) PriceExcursionMIN.Reset(1);
            PriceExcursionMIN[0] = ExcursionValue * -3;

            #region -- Show/Hide Excursion levels --  
            if (IsFirstTickOfBar)
            {
                if (!DisplayLevel1)
                {
                    PlotBrushes[PriceExcursionUL1IDX][0] = Brushes.Transparent;
                    PlotBrushes[PriceExcursionLL1IDX][0] = Brushes.Transparent;
                }
                if (!DisplayLevel2)
                {
                    PlotBrushes[PriceExcursionUL2IDX][0] = Brushes.Transparent;
                    PlotBrushes[PriceExcursionLL2IDX][0] = Brushes.Transparent;
                }
                if (!DisplayLevel3)
                {
                    PlotBrushes[PriceExcursionUL3IDX][0] = Brushes.Transparent;
                    PlotBrushes[PriceExcursionLL3IDX][0] = Brushes.Transparent;
                }
            }
            #endregion
            
            #endregion

line=2133;
            if (ThisInputType == HistoDivergence_InputType.High_Low)
                swingInput[0] = Input[0];
            else if (ThisInputType == HistoDivergence_InputType.Close)
                swingInput[0] = Close[0];
            else if (ThisInputType == HistoDivergence_InputType.Median)
                swingInput[0] = Median[0];
            else if (ThisInputType == HistoDivergence_InputType.Typical)
                swingInput[0] = Typical[0];

line=2143;
            #region --- Calculate and Draw ZigZag + Intrabar Structure BIAS ---

            #region -- Init zigzag states --    
            if (CurrentBar < 2)
            {
line=2149;
                upTrend[0] = true;
                pre_swingHighType[0] = 0;
                pre_swingLowType[0] = 0;
                swingHighType[0] = 0;
                swingLowType[0] = 0;
            }
            #endregion

            #region else if (Calculate == Calculate.OnBarClose)
            else if (Calculate == Calculate.OnBarClose)
            {
line=2161;
                bool useHL = ThisInputType == HistoDivergence_InputType.High_Low;

                zigzagDeviation = MultiplierMD * avgTrueRange[0];
                swingMax = MAX(useHL ? High : Input, SwingStrength)[1];
                swingMin = MIN(useHL ? Low : Input, SwingStrength)[1];

                pre_swingHighType[0] = 0;
                pre_swingLowType[0] = 0;
                swingHighType[0] = 0;
                swingLowType[0] = 0;

                updateHigh = upTrend[1] && (useHL ? High[0] : swingInput[0]) > currentHigh;
                updateLow = !upTrend[1] && (useHL ? Low[0] : swingInput[0]) < currentLow;
                addHigh = !upTrend[1] && !((useHL ? Low[0] : swingInput[0]) < currentLow) && (useHL ? High[0] : swingInput[0]) > Math.Max(swingMax, currentLow + zigzagDeviation);
                addLow = upTrend[1] && !((useHL ? High[0] : swingInput[0]) > currentHigh) && (useHL ? Low[0] : swingInput[0]) < Math.Min(swingMin, currentHigh - zigzagDeviation);

                upTrend[0] = upTrend[1];

line=2180;
                #region -- New High --
                if (addHigh)
                {
                    upTrend[0] = true;
                    int lookback = CurrentBar - lastLowIdx;
                    swingLowType[lookback] = pre_swingLowType[lookback];
                    double newHigh = double.MinValue;
                    int j = 0;
                    for (int i = 0; i < CurrentBar - lastLowIdx; i++)
                    {
                        if ((useHL ? High[i] : swingInput[i]) > newHigh)
                        {
                            newHigh = (useHL ? High[i] : swingInput[i]);
                            j = i;
                        }
                    }
line=2197;
                    currentHigh = newHigh;
                    priorSwingHighIdx = lastHighIdx;
                    lastHighIdx = CurrentBar - j;
                }
                #endregion

                #region -- uptrend --
                else if (updateHigh)
                {
line=2207;
                    upTrend[0] = true;
                    if (ShowZigzagDots) RemoveDrawObject(string.Format("swingHighDot{0}", lastHighIdx));
                    if (ShowZigzagLabels) RemoveDrawObject(string.Format("swingHighLabel{0}", lastHighIdx));
                    if (ShowZigzagLegs) RemoveDrawObject(string.Format("swingLegUp{0}", lastHighIdx));
                    pre_swingHighType[CurrentBar - lastHighIdx] = 0;
                    currentHigh = (useHL ? High[0] : swingInput[0]);
                    lastHighIdx = CurrentBar;
                }
                #endregion

                #region -- New Low --
                else if (addLow)
                {
line=2221;
                    upTrend[0] = false;
                    int lookback = CurrentBar - lastHighIdx;
                    swingHighType[lookback] = pre_swingHighType[lookback];
                    double newLow = double.MaxValue;
                    int j = 0;
                    for (int i = 0; i < CurrentBar - lastHighIdx; i++)
                    {
                        if ((useHL ? Low[i] : swingInput[i]) < newLow)
                        {
                            newLow = (useHL ? Low[i] : swingInput[i]);
                            j = i;
                        }
                    }
line=2235;
                    currentLow = newLow;
                    priorSwingLowIdx = lastLowIdx;
                    lastLowIdx = CurrentBar - j;
                }
                #endregion

                #region -- dwtrend --
                else if (updateLow)
                {
line=2245;
                    upTrend[0] = false;
                    if (ShowZigzagDots) RemoveDrawObject(string.Format("swingLowDot{0}", lastLowIdx));
                    if (ShowZigzagLabels) RemoveDrawObject(string.Format("swingLowLabel{0}", lastLowIdx));
                    if (ShowZigzagLegs) RemoveDrawObject(string.Format("swingLegDown{0}", lastLowIdx));
                    pre_swingLowType[CurrentBar - lastLowIdx] = 0;
                    currentLow = (useHL ? Low[0] : swingInput[0]);
                    lastLowIdx = CurrentBar;
                }
                #endregion

                #region re-init drawing states at each new bar before calculous
                if (ShowZigzagDots)
                {
                    drawHigherHighDot = false;
                    drawLowerHighDot = false;
                    drawDoubleTopDot = false;
                    drawLowerLowDot = false;
                    drawHigherLowDot = false;
                    drawDoubleBottomDot = false;
                }

                drawHigherHighLabel = false;
                drawLowerHighLabel = false;
                drawDoubleTopLabel = false;
                drawLowerLowLabel = false;
                drawHigherLowLabel = false;
                drawDoubleBottomLabel = false;
                
                if (ShowZigzagLegs)
                {
                    drawSwingLegUp = false;
                    drawSwingLegDown = false;
                }
                #endregion

                #region -- UP || HH --
                if (addHigh || updateHigh)
                {
line=2284;
                    int priorHighCount = CurrentBar - priorSwingHighIdx;
                    int priorLowCount = CurrentBar - priorSwingLowIdx;
                    highCount = CurrentBar - lastHighIdx;
                    lowCount = CurrentBar - lastLowIdx;

                    double marginUp = (useHL ? High[priorHighCount] : swingInput[priorHighCount]) + MultiplierDTB * avgTrueRange[highCount];
                    double marginDown = (useHL ? High[priorHighCount] : swingInput[priorHighCount]) - MultiplierDTB * avgTrueRange[highCount];

line=2293;
                    // new code goes here
                    #region -- Calculate acceleration on BBMACD and Histo --
                    if (currentHigh > High[priorHighCount] && (BBMACD[highCount] > 0 && BBMACD[0] > 0) && BBMACD[highCount] > BBMACD[priorHighCount])
                        acceleration1[0] = 2;
                    else if (currentHigh <= High[priorHighCount] && (BBMACD[highCount] > 0 && BBMACD[0] > 0) && acceleration1[lowCount] == 1)
                        acceleration1[0] = 1;
                    else if (currentHigh <= High[priorHighCount] && (BBMACD[highCount] < 0 && BBMACD[0] < 0) && acceleration1[lowCount] == -2)
                        acceleration1[0] = -1;
                    else
                        acceleration1[0] = 0;
                    if (currentHigh > High[priorHighCount] && Histogram[highCount] > 0 && Histogram[0] > 0 && Histogram[highCount] > Histogram[priorHighCount])
                        acceleration2[0] = 2;
                    else if (currentHigh <= High[priorHighCount] && Histogram[highCount] > 0 && Histogram[0] > 0 && acceleration2[lowCount] == 1)
                        acceleration2[0] = 1;
                    else if (currentHigh <= High[priorHighCount] && Histogram[highCount] < 0 && Histogram[0] < 0 && acceleration2[lowCount] == -2)
                        acceleration2[0] = -1;
                    else
                        acceleration2[0] = 0;
                    #endregion
                    // end new code

line=2315;
                    #region -- Set NEW drawing states --
                    if (ShowZigzagDots)
                    {
                        if (currentHigh > marginUp)
                            drawHigherHighDot = true;
                        else if (currentHigh < marginDown)
                            drawLowerHighDot = true;
                        else
                            drawDoubleTopDot = true;
                    }

                    if (currentHigh > marginUp) drawHigherHighLabel = true;
                    else if (currentHigh < marginDown) drawLowerHighLabel = true;
                    else drawDoubleTopLabel = true;

line=2331;
                    if (ShowZigzagLegs)
                        drawSwingLegUp = true;
                    if (currentHigh > marginUp)
                        pre_swingHighType[highCount] = 3;
                    else if (currentHigh < marginDown)
                        pre_swingHighType[highCount] = 1;
                    else
                        pre_swingHighType[highCount] = 2;
                    #endregion
                }
                #endregion

                #region -- DW || LL --
                else if (addLow || updateLow)
                {
line=2347;
                    int priorLowCount = CurrentBar - priorSwingLowIdx;
                    int priorHighCount = CurrentBar - priorSwingHighIdx;
                    lowCount = CurrentBar - lastLowIdx;
                    highCount = CurrentBar - lastHighIdx;

                    double marginDown = (useHL ? Low[priorLowCount] : swingInput[priorLowCount]) - MultiplierDTB * avgTrueRange[lowCount];
                    double marginUp = (useHL ? Low[priorLowCount] : swingInput[priorLowCount]) + MultiplierDTB * avgTrueRange[lowCount];

line=2356;
                    // new code goes here
                    #region -- Calculate acceleration on BBMACD and Histo --
                    if (currentLow < Low[priorLowCount] && (BBMACD[lowCount] < 0 && BBMACD[0] < 0) && BBMACD[lowCount] < BBMACD[priorLowCount])
                        acceleration1[0] = -2;
                    else if (currentLow >= Low[priorLowCount] && (BBMACD[lowCount] < 0 && BBMACD[0] < 0) && acceleration1[highCount] == -1)
                        acceleration1[0] = -1;
                    else if (currentLow >= Low[priorLowCount] && (BBMACD[lowCount] > 0 && BBMACD[0] > 0) && acceleration1[highCount] == 2)
                        acceleration1[0] = 1;
                    else
                        acceleration1[0] = 0;
                    if (currentLow < Low[priorLowCount] && Histogram[lowCount] < 0 && Histogram[0] < 0 && Histogram[lowCount] < Histogram[priorLowCount])
                        acceleration2[0] = -2;
                    else if (currentLow >= Low[priorLowCount] && Histogram[lowCount] < 0 && Histogram[0] < 0 && acceleration2[highCount] == -1)
                        acceleration2[0] = -1;
                    else if (currentLow >= Low[priorLowCount] && Histogram[lowCount] > 0 && Histogram[0] > 0 && acceleration2[highCount] == 2)
                        acceleration2[0] = 1;
                    else
                        acceleration2[0] = 0;
                    #endregion
                    // end new code

line=2378;
                    #region -- Set NEW drawing states --
                    if (ShowZigzagDots)
                    {
                        if (currentLow < marginDown)
                            drawLowerLowDot = true;
                        else if (currentLow > marginUp)
                            drawHigherLowDot = true;
                        else
                            drawDoubleBottomDot = true;
                    }

line=2390;
                    if (currentLow < marginDown) drawLowerLowLabel = true;
                    else if (currentLow > marginUp) drawHigherLowLabel = true;
                    else drawDoubleBottomLabel = true;
                    
                    if (ShowZigzagLegs)
                        drawSwingLegDown = true;
                    if (currentLow < marginDown)
                        pre_swingLowType[lowCount] = -3;
                    else if (currentLow > marginUp)
                        pre_swingLowType[lowCount] = -1;
                    else
                        pre_swingLowType[lowCount] = -2;
                    #endregion
                }
                #endregion

                //Is it possible ??
                else
                {
line=2410;
                    if ((acceleration1[1] > 0 && BBMACD[0] > 0) || (acceleration1[1] < 0 && BBMACD[0] < 0))
                        acceleration1[0] = acceleration1[1];
                    else
                        acceleration1[0] = 0;
                    if ((acceleration2[1] > 0 && Histogram[0] > 0) || (acceleration2[1] < 0 && Histogram[0] < 0))
                        acceleration2[0] = acceleration2[1];
                    else
                        acceleration2[0] = 0;
                }
            }
            #endregion

            #region else if (IsFirstTickOfBar)
            else if (IsFirstTickOfBar)
            {
line=2426;
                bool useHL = ThisInputType == HistoDivergence_InputType.High_Low;
                zigzagDeviation = MultiplierMD * avgTrueRange[1];
                swingMax = MAX(useHL ? High : Input, SwingStrength)[2];
                swingMin = MIN(useHL ? Low : Input, SwingStrength)[2];

                pre_swingHighType[0] = 0;
                pre_swingLowType[0] = 0;
                swingHighType[0] = 0;
                swingLowType[0] = 0;

                updateHigh = upTrend[1] && (useHL ? High[1] : swingInput[1]) > currentHigh;
                updateLow = !upTrend[1] && (useHL ? Low[1] : swingInput[1]) < currentLow;
                addHigh = !upTrend[1] && !((useHL ? Low[1] : swingInput[1]) < currentLow) && (useHL ? High[1] : swingInput[1]) > Math.Max(swingMax, currentLow + zigzagDeviation);
                addLow = upTrend[1] && !((useHL ? High[1] : swingInput[1]) > currentHigh) && (useHL ? Low[1] : swingInput[1]) < Math.Min(swingMin, currentHigh - zigzagDeviation);

                upTrend[0] = upTrend[1];

line=2444;
                #region -- New High --
                if (addHigh)
                {
                    upTrend[0] = true;
                    int lookback = CurrentBar - lastLowIdx;
                    swingLowType[lookback] = pre_swingLowType[lookback];
                    double newHigh = double.MinValue;
                    int j = 0;
                    for (int i = 1; i < CurrentBar - lastLowIdx; i++)
                    {
                        if ((useHL ? High[i] : swingInput[i]) > newHigh)
                        {
                            newHigh = (useHL ? High[i] : swingInput[i]);
                            j = i;
                        }
                    }
                    currentHigh = newHigh;
                    priorSwingHighIdx = lastHighIdx;
                    lastHighIdx = CurrentBar - j;
                }
                #endregion

                #region -- UPtrend --
                else if (updateHigh)
                {
line=2470;
                    upTrend[0] = true;
                    if (ShowZigzagDots)
                        RemoveDrawObject(string.Format("swingHighDot{0}", lastHighIdx));
                    if (ShowZigzagLabels)
                        RemoveDrawObject(string.Format("swingHighLabel{0}", lastHighIdx));
                    if (ShowZigzagLegs)
                        RemoveDrawObject(string.Format("swingLegUp{0}", lastHighIdx));
                    pre_swingHighType[CurrentBar - lastHighIdx] = 0;
                    currentHigh = (useHL ? High[1] : swingInput[1]);
                    lastHighIdx = CurrentBar - 1;
                }
                #endregion

                #region -- New Low --
                else if (addLow)
                {
line=2487;
                    upTrend[0] = false;
                    int lookback = CurrentBar - lastHighIdx;
                    swingHighType[lookback] = pre_swingHighType[lookback];
                    double newLow = double.MaxValue;
                    int j = 0;
                    for (int i = 1; i < CurrentBar - lastHighIdx; i++)
                    {
                        if ((useHL ? Low[i] : swingInput[i]) < newLow)
                        {
                            newLow = (useHL ? Low[i] : swingInput[i]);
                            j = i;
                        }
                    }
                    currentLow = newLow;
                    priorSwingLowIdx = lastLowIdx;
                    lastLowIdx = CurrentBar - j;
                }
                #endregion

                #region -- DWtrend --
                else if (updateLow)
                {
line=2510;
                    upTrend[0] = false;
                    if (ShowZigzagDots)
                        RemoveDrawObject(string.Format("swingLowDot{0}", lastLowIdx));
                    if (ShowZigzagLabels)
                        RemoveDrawObject(string.Format("swingLowLabel{0}", lastLowIdx));
                    if (ShowZigzagLegs)
                        RemoveDrawObject(string.Format("swingLegDown{0}", lastLowIdx));
                    pre_swingLowType[CurrentBar - lastLowIdx] = 0;
                    currentLow = useHL ? Low[1] : swingInput[1];
                    lastLowIdx = CurrentBar - 1;
                }
                #endregion

                #region re-init drawing states at each new bar before calculous
                if (ShowZigzagDots)
                {
                    drawHigherHighDot = false;
                    drawLowerHighDot = false;
                    drawDoubleTopDot = false;
                    drawLowerLowDot = false;
                    drawHigherLowDot = false;
                    drawDoubleBottomDot = false;
                }

                drawHigherHighLabel = false;
                drawLowerHighLabel = false;
                drawDoubleTopLabel = false;
                drawLowerLowLabel = false;
                drawHigherLowLabel = false;
                drawDoubleBottomLabel = false;

                if (ShowZigzagLegs)
                {
                    drawSwingLegUp = false;
                    drawSwingLegDown = false;
                }
                #endregion

                #region -- UP || HH --
                if (addHigh || updateHigh)
                {
line=2552;
                    int priorHighCount = CurrentBar - priorSwingHighIdx;
                    highCount = CurrentBar - lastHighIdx;
                    lowCount = CurrentBar - lastLowIdx;

                    double marginUp = (useHL ? High[priorHighCount] : swingInput[priorHighCount]) + MultiplierDTB * avgTrueRange[highCount];
                    double marginDown = (useHL ? High[priorHighCount] : swingInput[priorHighCount]) - MultiplierDTB * avgTrueRange[highCount];

                    #region -- Set NEW drawing states --
                    if (ShowZigzagDots)
                    {
                        if (currentHigh > marginUp)
                            drawHigherHighDot = true;
                        else if (currentHigh < marginDown)
                            drawLowerHighDot = true;
                        else
                            drawDoubleTopDot = true;
                    }

line=2571;
                    if (currentHigh > marginUp) drawHigherHighLabel = true;
                    else if (currentHigh < marginDown) drawLowerHighLabel = true;
                    else drawDoubleTopLabel = true;
                    
                    if (ShowZigzagLegs)
                        drawSwingLegUp = true;
                    if (currentHigh > marginUp)
                        pre_swingHighType[highCount] = 3;
                    else if (currentHigh < marginDown)
                        pre_swingHighType[highCount] = 1;
                    else
                        pre_swingHighType[highCount] = 2;
                    #endregion
                }
                #endregion

                #region -- DW || LL --
                else if (addLow || updateLow)
                {
line=2591;
                    int priorLowCount = CurrentBar - priorSwingLowIdx;
                    lowCount = CurrentBar - lastLowIdx;
                    highCount = CurrentBar - lastHighIdx;

                    double marginDown = (useHL ? Low[priorLowCount] : swingInput[priorLowCount]) - MultiplierDTB * avgTrueRange[lowCount];
                    double marginUp = (useHL ? Low[priorLowCount] : swingInput[priorLowCount]) + MultiplierDTB * avgTrueRange[lowCount];

                    #region -- Set NEW drawing states --
                    if (ShowZigzagDots)
                    {
                        if (currentLow < marginDown)
                            drawLowerLowDot = true;
                        else if (currentLow > marginUp)
                            drawHigherLowDot = true;
                        else
                            drawDoubleBottomDot = true;
                    }

line=2610;
                    if (currentLow < marginDown) drawLowerLowLabel = true;
                    else if (currentLow > marginUp) drawHigherLowLabel = true;
                    else drawDoubleBottomLabel = true;
                    
                    if (ShowZigzagLegs)
                        drawSwingLegDown = true;
                    if (currentLow < marginDown)
                        pre_swingLowType[lowCount] = -3;
                    else if (currentLow > marginUp)
                        pre_swingLowType[lowCount] = -1;
                    else
                        pre_swingLowType[lowCount] = -2;
                    #endregion
                }
                #endregion
            }
            #endregion

line=2629;
            #region if (Calculate != Calculate.OnBarClose)
            if (Calculate != Calculate.OnBarClose)
            {
                bool useHL = ThisInputType == HistoDivergence_InputType.High_Low;

                intraBarUpdateHigh = upTrend[0] && (useHL ? High[0] : swingInput[0]) > currentHigh;
                intraBarUpdateLow = !upTrend[0] && (useHL ? Low[0] : swingInput[0]) < currentLow;
                intraBarAddHigh = !upTrend[0] && !((useHL ? Low[0] : swingInput[0]) < currentLow) && (useHL ? High[0] : swingInput[0]) > Math.Max(swingMax, currentLow + zigzagDeviation);
                intraBarAddLow = upTrend[0] && !((useHL ? High[0] : swingInput[0]) > currentHigh) && (useHL ? Low[0] : swingInput[0]) < Math.Min(swingMin, currentHigh - zigzagDeviation);

line=2640;
                #region -- new HH --
                if (intraBarAddHigh)
                {
                    double newHigh = double.MinValue;
                    int j = 0;
                    for (int i = 0; i < CurrentBar - lastLowIdx; i++)
                    {
                        if ((useHL ? High[i] : swingInput[i]) > newHigh)
                        {
                            newHigh = (useHL ? High[i] : swingInput[i]);
                            j = i;
                        }
                    }
                    preCurrentHigh = newHigh;
                    preLastHighIdx = CurrentBar - j;
                }
                #endregion

                #region -- uptrend --
                else if (intraBarUpdateHigh)
                {
line=2662;
                    preCurrentHigh = (useHL ? High[0] : swingInput[0]);
                    preLastHighIdx = CurrentBar;
                }
                #endregion

line=2668;
                #region -- new LL --
                if (intraBarAddLow)
                {
line=2672;
                    double newLow = double.MaxValue;
                    int j = 0;
                    for (int i = 0; i < CurrentBar - lastHighIdx; i++)
                    {
                        if ((useHL ? Low[i] : swingInput[i]) < newLow)
                        {
                            newLow = useHL ? Low[i] : swingInput[i];
                            j = i;
                        }
                    }
                    preCurrentLow = newLow;
                    preLastLowIdx = CurrentBar - j;
                }
                #endregion

                #region -- dwtrend --
                else if (intraBarUpdateLow)
                {
line=2691;
                    preCurrentLow = (useHL ? Low[0] : swingInput[0]);
                    preLastLowIdx = CurrentBar;
                }
                #endregion

                #region -- UP || HH --
                if (intraBarAddHigh || intraBarUpdateHigh)
                {
line=2700;
                    int prePriorHighCount = intraBarAddHigh ? CurrentBar - lastHighIdx : CurrentBar - priorSwingHighIdx;
                    int preHighCount = CurrentBar - preLastHighIdx;
                    int prePriorLowCount = CurrentBar - priorSwingLowIdx;
                    int preLowCount = CurrentBar - lastLowIdx;

                    #region -- Calculate acceleration on BBMACD and Histo --
                    if (preCurrentHigh > High[prePriorHighCount] && (BBMACD[preHighCount] > 0 && BBMACD[0] > 0) && BBMACD[preHighCount] > BBMACD[prePriorHighCount])
                        acceleration1[0] = 2;
                    else if (preCurrentHigh <= High[prePriorHighCount] && (BBMACD[preHighCount] > 0 && BBMACD[0] > 0) && acceleration1[preLowCount] == 1)
                        acceleration1[0] = 1;
                    else if (preCurrentHigh <= High[prePriorHighCount] && (BBMACD[preHighCount] < 0 && BBMACD[0] < 0) && acceleration1[preLowCount] == -2)
                        acceleration1[0] = -1;
                    else
                        acceleration1[0] = 0;
                    if (preCurrentHigh > High[prePriorHighCount] && Histogram[preHighCount] > 0 && Histogram[0] > 0 && Histogram[preHighCount] > Histogram[prePriorHighCount])
                        acceleration2[0] = 2;
                    else if (preCurrentHigh <= High[prePriorHighCount] && Histogram[preHighCount] > 0 && Histogram[0] > 0 && acceleration2[preLowCount] == 1)
                        acceleration2[0] = 1;
                    else if (preCurrentHigh <= High[prePriorHighCount] && Histogram[preHighCount] < 0 && Histogram[0] < 0 && acceleration2[preLowCount] == -2)
                        acceleration2[0] = -1;
                    else
                        acceleration2[0] = 0;
                    #endregion

line=2725;
                    #region ---- StructureBias RealTime ---
                    double marginUp, marginDown;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                    {
                        marginUp = High[prePriorHighCount] + MultiplierDTB * avgTrueRange[preHighCount];
                        marginDown = High[prePriorHighCount] - MultiplierDTB * avgTrueRange[preHighCount];
                    }
                    else
                    {
                        marginUp = swingInput[prePriorHighCount] + MultiplierDTB * avgTrueRange[preHighCount];
                        marginDown = swingInput[prePriorHighCount] - MultiplierDTB * avgTrueRange[preHighCount];
                    }

                    if (preCurrentHigh > marginUp) preSRType = 3;//#STRBIAS
                    else if (preCurrentHigh < marginDown) preSRType = Math.Max(preSRType, 2);//#STRBIAS
                    else preSRType = Math.Max(preSRType, 1);//#STRBIAS
                    #endregion
                }
                #endregion

                #region -- DW || LL --
                else if (intraBarAddLow || intraBarUpdateLow)
                {
line=2749;
                    int prePriorLowCount = intraBarAddLow ? CurrentBar - lastLowIdx : CurrentBar - priorSwingLowIdx;
                    int preLowCount = CurrentBar - preLastLowIdx;
                    int prePriorHighCount = CurrentBar - priorSwingHighIdx;
                    int preHighCount = CurrentBar - lastHighIdx;

                    #region -- Calculate acceleration on BBMACD and Histo --
                    if (preCurrentLow < Low[prePriorLowCount] && (BBMACD[preLowCount] < 0 && BBMACD[0] < 0) && BBMACD[preLowCount] < BBMACD[prePriorLowCount])
                        acceleration1[0] = -2;
                    else if (preCurrentLow >= Low[prePriorLowCount] && (BBMACD[preLowCount] < 0 && BBMACD[0] < 0) && acceleration1[preHighCount] == -1)
                        acceleration1[0] = -1;
                    else if (preCurrentLow >= Low[prePriorLowCount] && (BBMACD[preLowCount] > 0 && BBMACD[0] > 0) && acceleration1[preHighCount] == 2)
                        acceleration1[0] = 1;
                    else
                        acceleration1[0] = 0;
                    if (preCurrentLow < Low[prePriorLowCount] && Histogram[preLowCount] < 0 && Histogram[0] < 0 && Histogram[preLowCount] < Histogram[prePriorLowCount])
                        acceleration2[0] = -2;
                    else if (preCurrentLow >= Low[prePriorLowCount] && Histogram[preLowCount] < 0 && Histogram[0] < 0 && acceleration2[preHighCount] == -1)
                        acceleration2[0] = -1;
                    else if (preCurrentLow >= Low[prePriorLowCount] && Histogram[preLowCount] > 0 && Histogram[0] > 0 && acceleration2[preHighCount] == 2)
                        acceleration2[0] = 1;
                    else
                        acceleration2[0] = 0;
                    #endregion

line=2774;
                    #region ---- StructureBias RealTime ---
                    double marginUp, marginDown;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                    {
                        marginDown = Low[prePriorLowCount] - MultiplierDTB * avgTrueRange[preLowCount];
                        marginUp = Low[prePriorLowCount] + MultiplierDTB * avgTrueRange[preLowCount];
                    }
                    else
                    {
                        marginDown = swingInput[prePriorLowCount] - MultiplierDTB * avgTrueRange[preLowCount];
                        marginUp = swingInput[prePriorLowCount] + MultiplierDTB * avgTrueRange[preLowCount];
                    }
                    if (preCurrentLow < marginDown) preSRType = -3;//#STRBIAS
                    else if (preCurrentLow > marginUp) preSRType = Math.Min(preSRType, -2);//#STRBIAS
                    else preSRType = Math.Min(preSRType, -1);//#STRBIAS
                    #endregion
                }
                #endregion

                //Is it possible ??
                else
                {
line=2797;
                    if ((acceleration1[1] > 0 && BBMACD[0] > 0) || (acceleration1[1] < 0 && BBMACD[0] < 0))
                        acceleration1[0] = acceleration1[1];
                    else
                        acceleration1[0] = 0;
                    if ((acceleration2[1] > 0 && Histogram[0] > 0) || (acceleration2[1] < 0 && Histogram[0] < 0))
                        acceleration2[0] = acceleration2[1];
                    else
                        acceleration2[0] = 0;

                    preSRType = 0;//#STRBIAS
                }

                #region ---- StructureBias RealTime ---
                if (CurrentBar < 2) 
					structureBiasState[0] = 0;
                else
                {
line=2815;
                    if (preSRType == 0) structureBiasState[0] = structureBiasState[1];

                    #region -- Oscillation State --
                    else if (structureBiasState[1] == 0)
                    {
                        //Oscillation State
                        //Need HH/!LL/HH to go to Up Trend
                        //{NEW} !LL/High/!LL/HH to go to Up Trend
                        //Need LL/!HH/LL to go to Dw Trend
                        //{NEW} !HH/Low/!HH/LL to go to Dw Trend				
                        if (sequence.Count < 2) structureBiasState[0] = 0;
                        else if (sequence.Count < 3)
                        {
line=2829;
                            if (sequence[0] == 3 && sequence[1] != -3 && preSRType == 3) structureBiasState[0] = 1;
                            else if (sequence[0] == -3 && sequence[1] != 3 && preSRType == -3) structureBiasState[0] = -1;
                            else structureBiasState[0] = 0;
                        }
                        else
                        {
line=2836;
                            if (sequence[1] == 3 && sequence[2] != -3 && preSRType == 3) structureBiasState[0] = 1;
                            else if (sequence[1] == -3 && sequence[2] != 3 && preSRType == -3) structureBiasState[0] = -1;
                            //{NEW} HL/LH/HL/HH to go to Up Trend
                            else if (sequence[0] != -3 && sequence[1] > 0 && sequence[2] != -3 && preSRType == 3) structureBiasState[0] = 1;
                            //{NEW} LH/HL/LH/LL to go to Up Trend
                            else if (sequence[0] != 3 && sequence[1] < 0 && sequence[2] != 3 && preSRType == -3) structureBiasState[0] = -1;
                            else structureBiasState[0] = 0;
                        }
                    }
                    #endregion

                    #region -- UpTrend State --
                    else if (structureBiasState[1] > 0)
                    {
line=2851;
                        //Look at Lows only. If LL go to OSC / else {HL or DB} stay UpTrend
                        if (preSRType == -3) structureBiasState[0] = 0;
                        else structureBiasState[0] = 1;
                    }
                    #endregion

                    #region -- DwTrend State --
                    else if (structureBiasState[1] < 0)
                    {
line=2861;
                        //Look at Highs only. If HH go to OSC / else {LH or DT} stay DwTrend
                        if (preSRType == 3) structureBiasState[0] = 0;
                        else structureBiasState[0] = -1;
                    }
                    #endregion

                    else structureBiasState[0] = structureBiasState[1];
                }
                #endregion

            }
            #endregion
//if(structureBiasState[0]==1 || structureBiasState[0]==-1) Print(Times[0][0].ToString()+"  SBS: "+structureBiasState[0]);
//if(structureBiasState[0]==1)  BackBrushes[0] = Brushes.Green;
//if(structureBiasState[0]==-1) BackBrushes[0] = Brushes.Red;

			int cutoffIdx = int.MinValue;
			int cb = CurrentBar;
			if(State==State.Historical) cb = Bars.Count;
			if(OptimizeSpeed == HistoDivergence_OptimizeSpeedSettings.Min) cutoffIdx = cb-500;
			else if(OptimizeSpeed == HistoDivergence_OptimizeSpeedSettings.Max) cutoffIdx = cb-100;
			bool PrintMarkers = CurrentBar>cutoffIdx;
line=2881;
            if (Calculate == Calculate.OnBarClose || IsFirstTickOfBar)
            {
                DrawOnPricePanel = true;
                #region -- Draw zigzag --    
                if (PrintMarkers && ShowZigzagDots && ThisInputType == HistoDivergence_InputType.High_Low)
                {
line=2888;
					if(IsFirstTickOfBar && cutoffIdx>0){
						RemoveDrawObject(string.Format("swingHighDot{0}",cutoffIdx));
						RemoveDrawObject(string.Format("swingLowDot{0}",cutoffIdx));
					}
                    if (drawHigherHighDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighDot{0}", lastHighIdx), true, dotString, highCount, High[highCount], SwingDotSize / 2, upColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawLowerHighDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighDot{0}", lastHighIdx), true, dotString, highCount, High[highCount], SwingDotSize / 2, downColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawDoubleTopDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighDot{0}", lastHighIdx), true, dotString, highCount, High[highCount], SwingDotSize / 2, doubleTopBottomColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    if (drawLowerLowDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowDot{0}", lastLowIdx), true, dotString, lowCount, Low[lowCount], SwingDotSize / 2, downColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawHigherLowDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowDot{0}", lastLowIdx), true, dotString, lowCount, Low[lowCount], SwingDotSize / 2, upColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawDoubleBottomDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowDot{0}", lastLowIdx), true, dotString, lowCount, Low[lowCount], SwingDotSize / 2, doubleTopBottomColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                }
                else if (PrintMarkers && ShowZigzagDots)
                {
line=2908;
					if(IsFirstTickOfBar && cutoffIdx>0){
						RemoveDrawObject(string.Format("swingHighDot{0}",cutoffIdx));
						RemoveDrawObject(string.Format("swingLowDot{0}",cutoffIdx));
					}
                    if (drawHigherHighDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighDot{0}", lastHighIdx), true, dotString, highCount, swingInput[highCount], SwingDotSize / 2, upColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawLowerHighDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighDot{0}", lastHighIdx), true, dotString, highCount, swingInput[highCount], SwingDotSize / 2, downColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawDoubleTopDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighDot{0}", lastHighIdx), true, dotString, highCount, swingInput[highCount], SwingDotSize / 2, doubleTopBottomColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    if (drawLowerLowDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowDot{0}", lastLowIdx), true, dotString, lowCount, swingInput[lowCount], SwingDotSize / 2, downColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawHigherLowDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowDot{0}", lastLowIdx), true, dotString, lowCount, swingInput[lowCount], SwingDotSize / 2, upColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawDoubleBottomDot)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowDot{0}", lastLowIdx), true, dotString, lowCount, swingInput[lowCount], SwingDotSize / 2, doubleTopBottomColor, swingDotFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                }
                if (PrintMarkers && ShowZigzagLabels)
                {
line=2928;
					if(IsFirstTickOfBar && cutoffIdx>0){
						RemoveDrawObject(string.Format("swingHighLabel{0}",cutoffIdx));
						RemoveDrawObject(string.Format("swingLowLabel{0}",cutoffIdx));
					}
                    if (drawHigherHighLabel)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighLabel{0}", lastHighIdx), true, "HH", highCount, High[highCount], (int)(labelFont.Size) + pixelOffset1, upColor, labelFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawLowerHighLabel)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighLabel{0}", lastHighIdx), true, "LH", highCount, High[highCount], (int)(labelFont.Size) + pixelOffset1, downColor, labelFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawDoubleTopLabel)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingHighLabel{0}", lastHighIdx), true, "DT", highCount, High[highCount], (int)(labelFont.Size) + pixelOffset1, doubleTopBottomColor, labelFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    if (drawLowerLowLabel)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowLabel{0}", lastLowIdx), true, "LL", lowCount, Low[lowCount], -(int)(labelFont.Size) - pixelOffset2, downColor, labelFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawHigherLowLabel)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowLabel{0}", lastLowIdx), true, "HL", lowCount, Low[lowCount], -(int)(labelFont.Size) - pixelOffset2, upColor, labelFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                    else if (drawDoubleBottomLabel)
                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("swingLowLabel{0}", lastLowIdx), true, "DB", lowCount, Low[lowCount], -(int)(labelFont.Size) - pixelOffset2, doubleTopBottomColor, labelFont, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
                }
                if (PrintMarkers && ShowZigzagLegs && ThisInputType == HistoDivergence_InputType.High_Low)
                {
line=2948;
					if(IsFirstTickOfBar && cutoffIdx>0){
						RemoveDrawObject(string.Format("swingLegUp{0}",cutoffIdx));
						RemoveDrawObject(string.Format("swingLegDown{0}",cutoffIdx));
					}
                    if (drawSwingLegUp && !IsDebug)
   	                    TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("swingLegUp{0}", lastHighIdx), false, lowCount, Low[lowCount], highCount, High[highCount], upColor, SwingLegStyle, SwingLegWidth);},0,null);
       	            if (drawSwingLegDown && !IsDebug)
           	            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("swingLegDown{0}", lastLowIdx), false, highCount, High[highCount], lowCount, Low[lowCount], downColor, SwingLegStyle, SwingLegWidth);},0,null);
                }
                else if (PrintMarkers && ShowZigzagLegs)
                {
line=2960;
					if(IsFirstTickOfBar && cutoffIdx>0){
						RemoveDrawObject(string.Format("swingLegUp{0}",cutoffIdx));
						RemoveDrawObject(string.Format("swingLegDown{0}",cutoffIdx));
					}
                    if (drawSwingLegUp && !IsDebug)
   	                    TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("swingLegUp{0}", lastHighIdx), false, lowCount, swingInput[lowCount], highCount, swingInput[highCount], upColor, SwingLegStyle, SwingLegWidth);},0,null);
       	            if (drawSwingLegDown && !IsDebug)
           	            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("swingLegDown{0}", lastLowIdx), false, highCount, swingInput[highCount], lowCount, swingInput[lowCount], downColor, SwingLegStyle, SwingLegWidth);},0,null);
                }
                #endregion
            }

            #endregion
line=2974;

            #region -- Calculate / Draw DIV -- 

            #region --- init before having enough bars --- 
            if (CurrentBar < BarsRequiredToPlot + DivMaxBars)
            {
line=2981;
                structureBiasState[0] = 0;//#STRBIAS
                SRType = 0;//#STRBIAS
//                swingHighsState[0] = 0;//#STRBIAS
//                swingLowsState[0] = 0;//#STRBIAS

                bearishTriggerCountMACDBB[0] = 0;
                bullishTriggerCountMACDBB[0] = 0;
                bearishTriggerCountHistogram[0] = 0;
                bullishTriggerCountHistogram[0] = 0;

                hiddenbearishTriggerCountMACDBB[0] = (0);//#HIDDENDIV
                hiddenbullishTriggerCountMACDBB[0] = (0);//#HIDDENDIV
                hiddenbearishTriggerCountHistogram[0] = (0);//#HIDDENDIV
                hiddenbullishTriggerCountHistogram[0] = (0);//#HIDDENDIV

                bearishDivPlotSeriesMACDBB[0] = 0;
                bullishDivPlotSeriesMACDBB[0] = 0;
                bearishDivPlotSeriesHistogram[0] = 0;
                bullishDivPlotSeriesHistogram[0] = 0;
                hiddenbearishDivPlotSeriesMACDBB[0] = 0;//#HIDDENDIV
                hiddenbullishDivPlotSeriesMACDBB[0] = 0;//#HIDDENDIV
                hiddenbearishDivPlotSeriesHistogram[0] = 0;//#HIDDENDIV
                hiddenbullishDivPlotSeriesHistogram[0] = 0;//#HIDDENDIV

//                cptBearMACDdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBullMACDdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBearHistogramdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBullHistogramdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBearMACDhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBullMACDhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBearHistogramhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                cptBullHistogramhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
                return;
            }
            #endregion
line=3017;

            #region -- Calcul OFFSET -- 
            if (IsFirstTickOfBar)
            {
line=3022;
                offsetDraw1 = OffsetMultiplier1 * avgTrueRange[1] / 100;
                offsetDraw2 = OffsetMultiplier2 * avgTrueRange[1] / 100;
                offsetDiv1 = OffsetMultiplier3 * avgTrueRange[1] / 100;
                offsetDiv2 = OffsetMultiplier4 * avgTrueRange[1] / 100;
            }
            #endregion
line=3029;

            #region -- Calculate Divergences --
            if (Calculate == Calculate.OnBarClose)
            {
line=3034;
                #region Bearish divergences between price and BBMACD
                if (bearishTriggerCountMACDBB[1] > 0)
                {
                    priorFirstPeakBar1 = firstPeakBar1;
                    priorFirstOscPeakBar1 = firstOscPeakBar1;
                    priorFirstPeakHigh1 = firstPeakHigh1;
                    priorFirstPeakValue1 = firstPeakValue1;
                    priorReplacementPeakValue1 = replacementPeakValue1;
                    priorSecondPeakValue1 = secondPeakValue1;
                    if (BBMACD[0] > firstPeakValue1)
                    {
                        bearishTriggerCountMACDBB[0] = 0;
                        RemoveDrawObject("divBearCandidateOsc1");
                        RemoveDrawObject("divBearCandidatePrice1");
                    }
                }
                bool drawBearishDivCandidateOsc1 = false;
                bool drawBearishDivCandidatePrice1 = false;
                bool updateBearishDivCandidateOsc1 = false;
                bool updateBearishDivCandidatePrice1 = false;
                bool drawBearSetup1 = false;
                bool drawBearishDivOnOsc1 = false;
                bool drawBearishDivOnPrice1 = false;
                bool drawArrowDown1 = false;
                bool firstPeakFound1 = false;
                peakCount1 = 0;
line=3061;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (swingHighType[i] > 0)
                    {
                        refPeakBar1[peakCount1] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh1[peakCount1] = High[i];
                        else
                            refPeakHigh1[peakCount1] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && BBMACD[j - 1] > BBMACD[j])
                                j = j - 1;
                            refOscPeakBar1[peakCount1] = CurrentBar - j;
                            refPeakValue1[peakCount1] = BBMACD[j];
                            peakCount1 = peakCount1 + 1;
                        }
                        else
                        {
                            refOscPeakBar1[peakCount1] = CurrentBar - i;
                            refPeakValue1[peakCount1] = BBMACD[i];
                            peakCount1 = peakCount1 + 1;
                        }

                    }
                }
                bearishPDivMACD[0] = 0.0;
                int maxBarOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                double maxValueOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? BBMACD[1] : BBMACD[0];

line=3096;
                for (int count = 0; count < peakCount1; count++) //find smallest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh1[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        bearishPDivMACD[0] = 1.0;
                        bearishTriggerCountMACDBB[0] = TriggerBars + 1;
                        firstPeakBar1 = refPeakBar1[count];
                        firstPeakHigh1 = refPeakHigh1[count];
                        firstOscPeakBar1 = refOscPeakBar1[count];
                        firstPeakValue1 = refPeakValue1[count];
                        secondPeakBar1 = CurrentBar;
                        secondPeakHigh1 = High[0];
                        secondOscPeakBar1 = maxBarOsc;
                        secondPeakValue1 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc1");
                            drawBearishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice1");
                            drawBearishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup1 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh1[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
line=3131;
                        bearishPDivMACD[0] = 1.0;
                        bearishTriggerCountMACDBB[0] = TriggerBars + 1;
                        firstPeakBar1 = refPeakBar1[count];
                        firstPeakHigh1 = refPeakHigh1[count];
                        firstOscPeakBar1 = refOscPeakBar1[count];
                        firstPeakValue1 = refPeakValue1[count];
                        secondPeakBar1 = CurrentBar;
                        secondPeakHigh1 = swingInput[0];
                        secondOscPeakBar1 = maxBarOsc;
                        secondPeakValue1 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc1");
                            drawBearishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice1");
                            drawBearishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup1 = true;
                        break;
                    }
                }
                for (int count = peakCount1 - 1; count >= 0; count--) //find largest divergence setup
                {
line=3159;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh1[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        replacementPeakBar1 = refPeakBar1[count];
                        replacementPeakHigh1 = refPeakHigh1[count];
                        replacementOscPeakBar1 = refOscPeakBar1[count];
                        replacementPeakValue1 = refPeakValue1[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh1[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        replacementPeakBar1 = refPeakBar1[count];
                        replacementPeakHigh1 = refPeakHigh1[count];
                        replacementOscPeakBar1 = refOscPeakBar1[count];
                        replacementPeakValue1 = refPeakValue1[count];
                        break;
                    }
                }
line=3181;
                if (bearishPDivMACD[0] < 0.5)
                {
                    if (bearishTriggerCountMACDBB[1] > 0)
                    {
                        bearishTriggerCountMACDBB[0] = bearishTriggerCountMACDBB[1] - 1;
                        if (BBMACD[0] > priorSecondPeakValue1)
                        {
line=3189;
                            if (BBMACD[0] < priorFirstPeakValue1)
                            {
                                secondOscPeakBar1 = CurrentBar;
                                secondPeakValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc1");
                                    updateBearishDivCandidateOsc1 = true;
                                }
                            }
                            else if (BBMACD[0] < priorReplacementPeakValue1)
                            {
                                firstPeakBar1 = replacementPeakBar1;
                                firstPeakHigh1 = replacementPeakHigh1;
                                firstOscPeakBar1 = replacementOscPeakBar1;
                                firstPeakValue1 = replacementPeakValue1;
                                secondOscPeakBar1 = CurrentBar;
                                secondPeakValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc1");
                                    drawBearishDivCandidateOsc1 = true;
                                    RemoveDrawObject("divBearCandidatePrice1");
                                    drawBearishDivCandidatePrice1 = true;
                                }
                            }
                            else
                                bearishTriggerCountMACDBB[0] = 0;
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bearishTriggerCountMACDBB[0] > 0 && High[0] > MAX(High, CurrentBar - firstPeakBar1)[1])
                        {
line=3221;
                            secondPeakBar1 = CurrentBar;
                            secondPeakHigh1 = High[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice1");
                                updateBearishDivCandidatePrice1 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bearishTriggerCountMACDBB[0] > 0 && swingInput[0] > MAX(swingInput, CurrentBar - firstPeakBar1)[1])
                        {
line=3232;
                            secondPeakBar1 = CurrentBar;
                            secondPeakHigh1 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice1");
                                updateBearishDivCandidatePrice1 = true;
                            }
                        }
                    }
                }

line=3244;
                if (bearishTriggerCountMACDBB[0] > 0)
                {
line=3247;
                    if ((Close[0] < High[CurrentBar - secondPeakBar1]) && (BBMACD[0] < BBMACD[1]) && Close[0] < Open[0])
                    {
                        bearishCDivMACD[0] = 1.0;
                        bearishTriggerCountMACDBB[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                            drawBearishDivOnOsc1 = true;
                        if (ShowDivOnPricePanel)
                            drawBearishDivOnPrice1 = true;
                        if (showArrows)
                            drawArrowDown1 = true;
                        RemoveDrawObject("divBearCandidateOsc1");
                        RemoveDrawObject("divBearCandidatePrice1");

//                        if (firstPeakBar1 != memfirstPeakBar1) cptBearMACDdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstPeakBar1 = firstPeakBar1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearMACDdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishMACDDivProjection[0] = cptBearMACDdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else
                        bearishCDivMACD[0] = 0.0;
                }
                else
                {
line=3271;
                    bearishCDivMACD[0] = 0.0;
                    {
                        RemoveDrawObject("divBearCandidateOsc1");
                        RemoveDrawObject("divBearCandidatePrice1");
                    }
                }
                #endregion

                #region Bearish Hidden divergences between price and BBMACD #HIDDENDIV
                if (hiddenbearishTriggerCountMACDBB[1] > 0)
                {
line=3283;
                    priorFirstPeakBar1H = firstPeakBar1H;
                    priorFirstOscPeakBar1H = firstOscPeakBar1H;
                    priorFirstPeakHigh1H = firstPeakHigh1H;
                    priorFirstPeakValue1H = firstPeakValue1H;
                    priorReplacementPeakHigh1H = replacementPeakHigh1H;
                    priorReplacementPeakValue1H = replacementPeakValue1H;
                    priorSecondPeakValue1H = secondPeakValue1H;
                    priorSecondPeakHigh1H = secondPeakHigh1H;

                    double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                    if (refinput > firstPeakHigh1H)
                    {
                        hiddenbearishTriggerCountMACDBB[0] = 0;
                        RemoveDrawObject("hiddendivBearCandidateOsc1");
                        RemoveDrawObject("hiddendivBearCandidatePrice1");
                    }
                }

                #region -- reset variables --
                drawBearishDivCandidateOsc1H = false;
                drawBearishDivCandidatePrice1H = false;
                updateBearishDivCandidateOsc1H = false;
                updateBearishDivCandidatePrice1H = false;
                drawBearSetup1H = false;
                drawBearishDivOnOsc1H = false;
                drawBearishDivOnPrice1H = false;
                drawArrowDown1H = false;
                firstPeakFound1H = false;
                peakCount1H = 0;
                #endregion

                #region -- get price top and osc top --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=3318;
                    int j = 0;
                    if (swingHighType[i] > 0)
                    {
                        refPeakBar1H[peakCount1H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh1H[peakCount1H] = High[i];
                        else
                            refPeakHigh1H[peakCount1H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && BBMACD[j - 1] > BBMACD[j])
                                j = j - 1;
                            refOscPeakBar1H[peakCount1H] = CurrentBar - j;
                            refPeakValue1H[peakCount1H] = BBMACD[j];
                            peakCount1H = peakCount1H + 1;
                        }
                        else
                        {
                            refOscPeakBar1H[peakCount1H] = CurrentBar - i;
                            refPeakValue1H[peakCount1H] = BBMACD[i];
                            peakCount1H = peakCount1H + 1;
                        }

                    }
                }
                #endregion

                hiddenbearishPDivMACD[0] = 0.0;
                maxBarOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                maxValueOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? BBMACD[1] : BBMACD[0];

                for (int count = 0; count < peakCount1H; count++) //find smallest divergence setup
                {
line=3355;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh1H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh1H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar1H[count] - 2)[1] &&
                        refPeakValue1H[count] > 0 &&
                        refPeakValue1H[count] < maxValueOsc &&
                        refPeakValue1H[count] < MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1H[count] - 6))[1] &&
                        (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1H[count])[0] > 0))
                    {
line=3366;
                        hiddenbearishPDivMACD[0] = 1.0;
                        hiddenbearishTriggerCountMACDBB[0] = TriggerBars + 1;
                        firstPeakBar1H = refPeakBar1H[count];
                        firstPeakHigh1H = refPeakHigh1H[count];
                        firstOscPeakBar1H = refOscPeakBar1H[count];
                        firstPeakValue1H = refPeakValue1H[count];
                        secondPeakBar1H = CurrentBar;
                        secondPeakHigh1H = refinput[0];
                        secondOscPeakBar1H = maxBarOsc;
                        secondPeakValue1H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidateOsc1");
                            drawBearishDivCandidateOsc1H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidatePrice1");
                            drawBearishDivCandidatePrice1H = true;
                        }
                        if (ShowSetupDots) drawBearSetup1H = true;
                        break;
                    }
                }

line=3392;
                for (int count = peakCount1H - 1; count >= 0; count--) //find largest divergence setup
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh1H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh1H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar1H[count] - 2)[1] &&
                        refPeakValue1H[count] > 0 &&
                        refPeakValue1H[count] < maxValueOsc &&
                        refPeakValue1H[count] < MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1H[count] - 6))[1] &&
                        (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1H[count])[0] > 0))
                    {
                        replacementPeakBar1H = refPeakBar1H[count];
                        replacementPeakHigh1H = refPeakHigh1H[count];
                        replacementOscPeakBar1H = refOscPeakBar1H[count];
                        replacementPeakValue1H = refPeakValue1H[count];
                        break;
                    }
                }

line=3412;
                double inputref = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                if (hiddenbearishPDivMACD[0] < 0.5)
                {
                    if (hiddenbearishTriggerCountMACDBB[1] > 0)
                    {
                        hiddenbearishTriggerCountMACDBB[0] = hiddenbearishTriggerCountMACDBB[1] - 1;
                        if (inputref > priorSecondPeakHigh1H)
                        {
                            if (inputref < priorFirstPeakHigh1H)//price stays below
                            {
                                secondPeakBar1H = CurrentBar;
                                secondPeakHigh1H = inputref;
                                if (!hidePlots && ShowDivOnPricePanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                                    updateBearishDivCandidatePrice1H = true;
                                }
                            }
                            else if (inputref < priorReplacementPeakHigh1H)
                            {
                                firstPeakBar1H = replacementPeakBar1H;
                                firstPeakHigh1H = replacementPeakHigh1H;
                                firstOscPeakBar1H = replacementOscPeakBar1H;
                                firstPeakValue1H = replacementPeakValue1H;
                                secondPeakBar1H = CurrentBar;
                                secondPeakHigh1H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidateOsc1");
                                    drawBearishDivCandidateOsc1H = true;
                                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                                    drawBearishDivCandidatePrice1H = true;
                                }
                            }
                            else
                                hiddenbearishTriggerCountMACDBB[0] = 0;
                        }
                        if (hiddenbearishTriggerCountMACDBB[0] > 0 && BBMACD[0] > MAX(BBMACD, CurrentBar - firstOscPeakBar1H)[1])
                        {
                            secondOscPeakBar1H = CurrentBar;
                            secondPeakValue1H = BBMACD[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBearCandidateOsc1");
                                updateBearishDivCandidateOsc1H = true;
                            }
                        }
                    }
                }

line=3463;
                if (hiddenbearishTriggerCountMACDBB[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar1H]) && (BBMACD[0] < BBMACD[1]) && Close[0] < Open[0])
                    {
                        hiddenbearishCDivMACD[0] = 1.0;
                        hiddenbearishTriggerCountMACDBB[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                            drawBearishDivOnOsc1H = true;
                        if (ShowDivOnPricePanel)
                            drawBearishDivOnPrice1H = true;
                        if (showArrows)
                            drawArrowDown1H = true;
                        RemoveDrawObject("hiddendivBearCandidateOsc1");
                        RemoveDrawObject("hiddendivBearCandidatePrice1");

//                        if (firstPeakBar1H != memfirstPeakBar1H) cptBearMACDhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstPeakBar1H = firstPeakBar1H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearMACDhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishMACDHiddenDivProjection[0] = cptBearMACDhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else hiddenbearishCDivMACD[0] = 0.0;
                }
                else
                {
                    hiddenbearishCDivMACD[0] = 0.0;
                    RemoveDrawObject("hiddendivBearCandidateOsc1");
                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                }
                #endregion

                #region Bullish divergences between price and BBMACD
                if (bullishTriggerCountMACDBB[1] > 0)
                {
line=3497;
                    priorFirstTroughBar1 = firstTroughBar1;
                    priorFirstOscTroughBar1 = firstOscTroughBar1;
                    priorFirstTroughLow1 = firstTroughLow1;
                    priorFirstTroughValue1 = firstTroughValue1;
                    priorReplacementTroughValue1 = replacementTroughValue1;
                    priorSecondTroughValue1 = secondTroughValue1;
                    if (BBMACD[0] < firstTroughValue1)
                    {
                        bullishTriggerCountMACDBB[0] = 0;
                        RemoveDrawObject("divBullCandidateOsc1");
                        RemoveDrawObject("divBullCandidatePrice1");
                    }
                }
                bool drawBullishDivCandidateOsc1 = false;
                bool drawBullishDivCandidatePrice1 = false;
                bool updateBullishDivCandidateOsc1 = false;
                bool updateBullishDivCandidatePrice1 = false;
                bool drawBullSetup1 = false;
                bool drawBullishDivOnOsc1 = false;
                bool drawBullishDivOnPrice1 = false;
                bool drawArrowUp1 = false;
                bool firstTroughFound1 = false;
                troughCount1 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=3523;
                    int j = 0;
                    if (swingLowType[i] < 0)
                    {
                        refTroughBar1[troughCount1] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow1[troughCount1] = Low[i];
                        else
                            refTroughLow1[troughCount1] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && BBMACD[j - 1] < BBMACD[j])
                                j = j - 1;
                            refOscTroughBar1[troughCount1] = CurrentBar - j;
                            refTroughValue1[troughCount1] = BBMACD[j];
                            troughCount1 = troughCount1 + 1;
                        }
                        else
                        {
                            refOscTroughBar1[troughCount1] = CurrentBar - i;
                            refTroughValue1[troughCount1] = BBMACD[i];
                            troughCount1 = troughCount1 + 1;
                        }
                    }
                }
                bullishPDivMACD[0] = 0.0;
                int minBarOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                double minValueOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? BBMACD[1] : BBMACD[0];

                for (int count = 0; count < troughCount1; count++) //find smallest divergence setup
                {
line=3557;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow1[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        bullishPDivMACD[0] = 1.0;
                        bullishTriggerCountMACDBB[0] = TriggerBars + 1;
                        firstTroughBar1 = refTroughBar1[count];
                        firstTroughLow1 = refTroughLow1[count];
                        firstOscTroughBar1 = refOscTroughBar1[count];
                        firstTroughValue1 = refTroughValue1[count];
                        secondTroughBar1 = CurrentBar;
                        secondTroughLow1 = Low[0];
                        secondOscTroughBar1 = minBarOsc;
                        secondTroughValue1 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc1");
                            drawBullishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice1");
                            drawBullishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup1 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow1[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
line=3590;
                        bullishPDivMACD[0] = 1.0;
                        bullishTriggerCountMACDBB[0] = TriggerBars + 1;
                        firstTroughBar1 = refTroughBar1[count];
                        firstTroughLow1 = refTroughLow1[count];
                        firstOscTroughBar1 = refOscTroughBar1[count];
                        firstTroughValue1 = refTroughValue1[count];
                        secondTroughBar1 = CurrentBar;
                        secondTroughLow1 = swingInput[0];
                        secondOscTroughBar1 = minBarOsc;
                        secondTroughValue1 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc1");
                            drawBullishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice1");
                            drawBullishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup1 = true;
                        break;
                    }
                }
                for (int count = troughCount1 - 1; count >= 0; count--) //find largest divergence setup
                {
line=3618;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow1[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        replacementTroughBar1 = refTroughBar1[count];
                        replacementTroughLow1 = refTroughLow1[count];
                        replacementOscTroughBar1 = refOscTroughBar1[count];
                        replacementTroughValue1 = refTroughValue1[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow1[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        replacementTroughBar1 = refTroughBar1[count];
                        replacementTroughLow1 = refTroughLow1[count];
                        replacementOscTroughBar1 = refOscTroughBar1[count];
                        replacementTroughValue1 = refTroughValue1[count];
                        break;
                    }
                }

                if (bullishPDivMACD[0] < 0.5)
                {
line=3643;
                    if (bullishTriggerCountMACDBB[1] > 0)
                    {
                        bullishTriggerCountMACDBB[0] = bullishTriggerCountMACDBB[1] - 1;
                        if (BBMACD[0] < priorSecondTroughValue1)
                        {
                            if (BBMACD[0] > priorFirstTroughValue1)
                            {
line=3651;
                                secondOscTroughBar1 = CurrentBar;
                                secondTroughValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc1");
                                    updateBullishDivCandidateOsc1 = true;
                                }
                            }
                            else if (BBMACD[0] > priorReplacementTroughValue1)
                            {
line=3662;
                                firstTroughBar1 = replacementTroughBar1;
                                firstTroughLow1 = replacementTroughLow1;
                                firstOscTroughBar1 = replacementOscTroughBar1;
                                firstTroughValue1 = replacementTroughValue1;
                                secondOscTroughBar1 = CurrentBar;
                                secondTroughValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc1");
                                    drawBullishDivCandidateOsc1 = true;
                                    RemoveDrawObject("divBullCandidatePrice1");
                                    drawBullishDivCandidatePrice1 = true;
                                }
                            }
                            else
                                bullishTriggerCountMACDBB[0] = 0;
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bullishTriggerCountMACDBB[0] > 0 && Low[0] < MIN(Low, CurrentBar - firstTroughBar1)[1])
                        {
line=3682;
                            secondTroughBar1 = CurrentBar;
                            secondTroughLow1 = Low[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice1");
                                updateBullishDivCandidatePrice1 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bullishTriggerCountMACDBB[0] > 0 && swingInput[0] < MIN(swingInput, CurrentBar - firstTroughBar1)[1])
                        {
line=3693;
                            secondTroughBar1 = CurrentBar;
                            secondTroughLow1 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice1");
                                updateBullishDivCandidatePrice1 = true;
                            }
                        }
                    }
                }

                if (bullishTriggerCountMACDBB[0] > 0)
                {
                    if ((Close[0] > Low[CurrentBar - secondTroughBar1]) && (BBMACD[0] > BBMACD[1]) && Close[0] > Open[0])
                    {
line=3709;
                        bullishCDivMACD[0] = 1.0;
                        bullishTriggerCountMACDBB[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                            drawBullishDivOnOsc1 = true;
                        if (ShowDivOnPricePanel)
                            drawBullishDivOnPrice1 = true;
                        if (showArrows)
                            drawArrowUp1 = true;
                        RemoveDrawObject("divBullCandidateOsc1");
                        RemoveDrawObject("divBullCandidatePrice1");

//                        if (firstTroughBar1 != memfirstTroughBar1) cptBullMACDdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstTroughBar1 = firstTroughBar1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullMACDdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishMACDDivProjection[0] = cptBullMACDdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else
                        bullishCDivMACD[0] = 0.0;
                }
                else
                {
                    bullishCDivMACD[0] = 0.0;
                    {
                        RemoveDrawObject("divBullCandidateOsc1");
                        RemoveDrawObject("divBullCandidatePrice1");
                    }
                }
                #endregion

                #region Bullish Hidden divergences between price and BBMACD #HIDDENDIV
                if (hiddenbullishTriggerCountMACDBB[1] > 0)
                {
line=3742;
                    priorFirstTroughBar1H = firstTroughBar1H;
                    priorFirstOscTroughBar1H = firstOscTroughBar1H;
                    priorFirstTroughLow1H = firstTroughLow1H;
                    priorFirstTroughValue1H = firstTroughValue1H;
                    priorReplacementTroughLow1H = replacementTroughLow1H;
                    priorReplacementTroughValue1H = replacementTroughValue1H;
                    priorSecondTroughValue1H = secondTroughValue1H;
                    priorSecondTroughLow1H = secondTroughLow1H;

                    double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                    if (refinput < firstTroughLow1H)
                    {
                        hiddenbullishTriggerCountMACDBB[0] = 0;
                        RemoveDrawObject("hiddendivBullCandidateOsc1");
                        RemoveDrawObject("hiddendivBullCandidatePrice1");
                    }
                }

                #region -- reset variables --
                drawBullishDivCandidateOsc1H = false;
                drawBullishDivCandidatePrice1H = false;
                updateBullishDivCandidateOsc1H = false;
                updateBullishDivCandidatePrice1H = false;
                drawBullSetup1H = false;
                drawBullishDivOnOsc1H = false;
                drawBullishDivOnPrice1H = false;
                drawArrowUp1H = false;
                firstTroughFound1H = false;
                troughCount1H = 0;
                #endregion

                #region -- get price low and osc low --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=3777;
                    int j = 0;
                    if (swingLowType[i] < 0)
                    {
                        refTroughBar1H[troughCount1H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow1H[troughCount1H] = Low[i];
                        else
                            refTroughLow1H[troughCount1H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && BBMACD[j - 1] < BBMACD[j])
                                j = j - 1;
                            refOscTroughBar1H[troughCount1H] = CurrentBar - j;
                            refTroughValue1H[troughCount1H] = BBMACD[j];
                            troughCount1H++;
                        }
                        else
                        {
                            refOscTroughBar1H[troughCount1H] = CurrentBar - i;
                            refTroughValue1H[troughCount1H] = BBMACD[i];
                            troughCount1H++;
                        }
                    }
                }
                #endregion

                hiddenbullishPDivMACD[0] = 0.0;
                minBarOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                minValueOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? BBMACD[1] : BBMACD[0];

                for (int count = 0; count < troughCount1H; count++) //find smallest divergence setup
                {
line=3814;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow1H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow1H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar1H[count] - 2)[1] &&
                        refTroughValue1H[count] < 0 &&
                        refTroughValue1H[count] > minValueOsc &&
                        refTroughValue1H[count] > MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1H[count] - 6))[1] &&
                        (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1H[count])[0] < 0))
                    {
line=3825;
                        hiddenbullishPDivMACD[0] = 1.0;
                        hiddenbullishTriggerCountMACDBB[0] = TriggerBars + 1;
                        firstTroughBar1H = refTroughBar1H[count];
                        firstTroughLow1H = refTroughLow1H[count];
                        firstOscTroughBar1H = refOscTroughBar1H[count];
                        firstTroughValue1H = refTroughValue1H[count];
                        secondTroughBar1H = CurrentBar;
                        secondTroughLow1H = refinput[0];
                        secondOscTroughBar1H = maxBarOsc;
                        secondTroughValue1H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidateOsc1");
                            drawBullishDivCandidateOsc1H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidatePrice1");
                            drawBullishDivCandidatePrice1H = true;
                        }
                        if (ShowSetupDots) drawBullSetup1H = true;
                        break;
                    }
                }

                for (int count = troughCount1H - 1; count >= 0; count--) //find largest divergence setup
                {
line=3853;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow1H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow1H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar1H[count] - 2)[1] &&
                        refTroughValue1H[count] < 0 &&
                        refTroughValue1H[count] > minValueOsc &&
                        refTroughValue1H[count] > MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1H[count] - 6))[1] &&
                        (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1H[count])[0] < 0))
                    {
                        replacementTroughBar1H = refOscTroughBar1H[count];
                        replacementTroughLow1H = refTroughLow1H[count];
                        replacementOscTroughBar1H = refOscTroughBar1H[count];
                        replacementTroughValue1H = refTroughValue1H[count];
                        break;
                    }
                }

                inputref = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                if (hiddenbullishPDivMACD[0] < 0.5)
                {
line=3874;
                    if (hiddenbullishTriggerCountMACDBB[1] > 0)
                    {
                        hiddenbullishTriggerCountMACDBB[0] = hiddenbullishTriggerCountMACDBB[1] - 1;
                        if (inputref < priorSecondTroughLow1H)
                        {
                            if (inputref > priorFirstTroughLow1H)//price stays above
                            {
line=3882;
                                secondTroughBar1H = CurrentBar;
                                secondTroughLow1H = inputref;
                                if (!hidePlots && ShowDivOnPricePanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                                    updateBullishDivCandidatePrice1H = true;
                                }
                            }
                            else if (inputref > priorReplacementTroughLow1H)
                            {
line=3893;
                                firstTroughBar1H = replacementTroughBar1H;
                                firstTroughLow1H = replacementTroughLow1H;
                                firstOscTroughBar1H = replacementOscTroughBar1H;
                                firstTroughValue1H = replacementTroughValue1H;
                                secondTroughBar1H = CurrentBar;
                                secondTroughLow1H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidateOsc1");
                                    drawBullishDivCandidateOsc1H = true;
                                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                                    drawBullishDivCandidatePrice1H = true;
                                }
                            }
                            else hiddenbullishTriggerCountMACDBB[0] = 0;
                        }
                        if (hiddenbullishTriggerCountMACDBB[0] > 0 && BBMACD[0] < MIN(BBMACD, CurrentBar - firstOscTroughBar1H)[1])
                        {
line=3912;
                            secondOscTroughBar1H = CurrentBar;
                            secondTroughValue1H = BBMACD[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBullCandidateOsc1");
                                updateBullishDivCandidateOsc1H = true;
                            }
                        }
                    }
                }

                if (hiddenbullishTriggerCountMACDBB[0] > 0)
                {
line=3926;
                    if ((Close[0] > Low[CurrentBar - secondTroughBar1H]) && (BBMACD[0] > BBMACD[1]) && Close[0] > Open[0])
                    {
                        hiddenbullishCDivMACD[0] = 1.0;
                        hiddenbullishTriggerCountMACDBB[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel) drawBullishDivOnOsc1H = true;
                        if (ShowDivOnPricePanel) drawBullishDivOnPrice1H = true;
                        if (showArrows) drawArrowUp1H = true;
                        RemoveDrawObject("hiddendivBullCandidateOsc1");
                        RemoveDrawObject("hiddendivBullCandidatePrice1");

//                        if (firstTroughBar1H != memfirstTroughBar1H) cptBullMACDhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstTroughBar1H = firstTroughBar1H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullMACDhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishMACDHiddenDivProjection[0] = cptBullMACDhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else hiddenbullishCDivMACD[0] = 0.0;
                }
                else
                {
                    hiddenbullishCDivMACD[0] = 0.0;
                    RemoveDrawObject("hiddendivBullCandidateOsc1");
                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                }
                #endregion

                #region Bearish divergences between price and histogram
                if (bearishTriggerCountHistogram[1] > 0)
                {
line=3955;
                    priorFirstPeakBar2 = firstPeakBar2;
                    priorFirstOscPeakBar2 = firstOscPeakBar2;
                    priorFirstPeakHigh2 = firstPeakHigh2;
                    priorFirstPeakValue2 = firstPeakValue2;
                    priorReplacementPeakValue2 = replacementPeakValue2;
                    priorSecondPeakValue2 = secondPeakValue2;
                    if (Histogram[0] > firstPeakValue2)
                    {
                        bearishTriggerCountHistogram[0] = 0;
                        RemoveDrawObject("divBearCandidateOsc2");
                        RemoveDrawObject("divBearCandidatePrice2");
                    }
                }
                bool drawBearishDivCandidateOsc2 = false;
                bool drawBearishDivCandidatePrice2 = false;
                bool updateBearishDivCandidateOsc2 = false;
                bool updateBearishDivCandidatePrice2 = false;
                bool drawBearSetup2 = false;
                bool drawBearishDivOnOsc2 = false;
                bool drawBearishDivOnPrice2 = false;
                bool drawArrowDown2 = false;
                bool firstPeakFound2 = false;
                peakCount2 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=3981;
                    int j = 0;
                    if (swingHighType[i] > 0)
                    {
                        refPeakBar2[peakCount2] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh2[peakCount2] = High[i];
                        else
                            refPeakHigh2[peakCount2] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && Histogram[j - 1] > Histogram[j])
                                j = j - 1;
                            refOscPeakBar2[peakCount2] = CurrentBar - j;
                            refPeakValue2[peakCount2] = Histogram[j];
                            peakCount2 = peakCount2 + 1;
                        }
                        else
                        {
                            refOscPeakBar2[peakCount2] = CurrentBar - i;
                            refPeakValue2[peakCount2] = Histogram[i];
                            peakCount2 = peakCount2 + 1;
                        }

                    }
                }
                bearishPDivHistogram[0] = 0.0;
line=4011;
                if (UseOscHighLow && Histogram[1] > Histogram[0])
                {
                    maxBarOsc = CurrentBar - 1;
                    maxValueOsc = Histogram[1];
                }
                else
                {
                    maxBarOsc = CurrentBar;
                    maxValueOsc = Histogram[0];
                }
                for (int count = 0; count < peakCount2; count++) //find smallest divergence setup
                {
line=4024;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh2[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        bearishPDivHistogram[0] = 1.0;
                        bearishTriggerCountHistogram[0] = TriggerBars + 1;
                        firstPeakBar2 = refPeakBar2[count];
                        firstPeakHigh2 = refPeakHigh2[count];
                        firstOscPeakBar2 = refOscPeakBar2[count];
                        firstPeakValue2 = refPeakValue2[count];
                        secondPeakBar2 = CurrentBar;
                        secondPeakHigh2 = High[0];
                        secondOscPeakBar2 = maxBarOsc;
                        secondPeakValue2 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc2");
                            drawBearishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice2");
                            drawBearishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup2 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh2[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
line=4057;
                        bearishPDivHistogram[0] = 1.0;
                        bearishTriggerCountHistogram[0] = TriggerBars + 1;
                        firstPeakBar2 = refPeakBar2[count];
                        firstPeakHigh2 = refPeakHigh2[count];
                        firstOscPeakBar2 = refOscPeakBar2[count];
                        firstPeakValue2 = refPeakValue2[count];
                        secondPeakBar2 = CurrentBar;
                        secondPeakHigh2 = swingInput[0];
                        secondOscPeakBar2 = maxBarOsc;
                        secondPeakValue2 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc2");
                            drawBearishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice2");
                            drawBearishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup2 = true;
                        break;
                    }
                }
                for (int count = peakCount2 - 1; count >= 0; count--) //find largest divergence setup
                {
line=4085;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh2[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        replacementPeakBar2 = refPeakBar2[count];
                        replacementPeakHigh2 = refPeakHigh2[count];
                        replacementOscPeakBar2 = refOscPeakBar2[count];
                        replacementPeakValue2 = refPeakValue2[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh2[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        replacementPeakBar2 = refPeakBar2[count];
                        replacementPeakHigh2 = refPeakHigh2[count];
                        replacementOscPeakBar2 = refOscPeakBar2[count];
                        replacementPeakValue2 = refPeakValue2[count];
                        break;
                    }
                }
                if (bearishPDivHistogram[0] < 0.5)
                {
line=4109;
                    if (bearishTriggerCountHistogram[1] > 0)
                    {
                        bearishTriggerCountHistogram[0] = bearishTriggerCountHistogram[1] - 1;
                        if (Histogram[0] > priorSecondPeakValue2)
                        {
                            if (Histogram[0] < priorFirstPeakValue2)
                            {
line=4117;
                                secondOscPeakBar2 = CurrentBar;
                                secondPeakValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc2");
                                    updateBearishDivCandidateOsc2 = true;
                                }
                            }
                            else if (Histogram[0] < priorReplacementPeakValue2)
                            {
line=4128;
                                firstPeakBar2 = replacementPeakBar2;
                                firstPeakHigh2 = replacementPeakHigh2;
                                firstOscPeakBar2 = replacementOscPeakBar2;
                                firstPeakValue2 = replacementPeakValue2;
                                secondOscPeakBar2 = CurrentBar;
                                secondPeakValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc2");
                                    drawBearishDivCandidateOsc2 = true;
                                    RemoveDrawObject("divBearCandidatePrice2");
                                    drawBearishDivCandidatePrice2 = true;
                                }
                            }
                            else
                                bearishTriggerCountHistogram[0] = 0;
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bearishTriggerCountHistogram[0] > 0 && High[0] > MAX(High, CurrentBar - firstPeakBar2)[1])
                        {
line=4148;
                            secondPeakBar2 = CurrentBar;
                            secondPeakHigh2 = High[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice2");
                                updateBearishDivCandidatePrice2 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bearishTriggerCountHistogram[0] > 0 && swingInput[0] > MAX(swingInput, CurrentBar - firstPeakBar2)[1])
                        {
line=4159;
                            secondPeakBar2 = CurrentBar;
                            secondPeakHigh2 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice2");
                                updateBearishDivCandidatePrice2 = true;
                            }
                        }
                    }
                }

                if (bearishTriggerCountHistogram[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar2]) && (Histogram[0] < Histogram[1]) && Close[0] < Open[0])
                    {
line=4175;
                        bearishCDivHistogram[0] = 1.0;
                        bearishTriggerCountHistogram[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                            drawBearishDivOnOsc2 = true;
                        if (ShowDivOnPricePanel)
                            drawBearishDivOnPrice2 = true;
                        if (showArrows)
                            drawArrowDown2 = true;
                        RemoveDrawObject("divBearCandidateOsc2");
                        RemoveDrawObject("divBearCandidatePrice2");

//                        if (firstPeakBar2 != memfirstPeakBar2) cptBearHistogramdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstPeakBar2 = firstPeakBar2;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearHistogramdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishHistogramDivProjection[0] = cptBearHistogramdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else
                        bearishCDivHistogram[0] = 0.0;
                }
                else
                {
                    bearishCDivHistogram[0] = 0.0;
                    {
                        RemoveDrawObject("divBearCandidateOsc2");
                        RemoveDrawObject("divBearCandidatePrice2");
                    }
                }
                #endregion

                #region Bearish Hidden divergences between price and Histogram #HIDDENDIV
                if (hiddenbearishTriggerCountHistogram[1] > 0)
                {
line=4208;
                    priorFirstPeakBar2H = firstPeakBar2H;
                    priorFirstOscPeakBar2H = firstOscPeakBar2H;
                    priorFirstPeakHigh2H = firstPeakHigh2H;
                    priorFirstPeakValue2H = firstPeakValue2H;
                    priorReplacementPeakHigh2H = replacementPeakHigh2H;
                    priorReplacementPeakValue2H = replacementPeakValue2H;
                    priorSecondPeakValue2H = secondPeakValue2H;
                    priorSecondPeakHigh2H = secondPeakHigh2H;

                    double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                    if (refinput > firstPeakHigh2H)
                    {
                        hiddenbearishTriggerCountHistogram[0] = 0;
                        RemoveDrawObject("hiddendivBearCandidateOsc2");
                        RemoveDrawObject("hiddendivBearCandidatePrice2");
                    }
                }

                #region -- reset variables --
                drawBearishDivCandidateOsc2H = false;
                drawBearishDivCandidatePrice2H = false;
                updateBearishDivCandidateOsc2H = false;
                updateBearishDivCandidatePrice2H = false;
                drawBearSetup2H = false;
                drawBearishDivOnOsc2H = false;
                drawBearishDivOnPrice2H = false;
                drawArrowDown2H = false;
                firstPeakFound2H = false;
                peakCount2H = 0;
                #endregion

                #region -- get price top and osc top --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=4243;
                    int j = 0;
                    if (swingHighType[i] > 0)
                    {
                        refPeakBar2H[peakCount2H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh2H[peakCount2H] = High[i];
                        else
                            refPeakHigh2H[peakCount2H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && Histogram[j - 1] > Histogram[j])
                                j = j - 1;
                            refOscPeakBar2H[peakCount2H] = CurrentBar - j;
                            refPeakValue2H[peakCount2H] = Histogram[j];
                            peakCount2H = peakCount2H + 1;
                        }
                        else
                        {
                            refOscPeakBar2H[peakCount2H] = CurrentBar - i;
                            refPeakValue2H[peakCount2H] = Histogram[i];
                            peakCount2H = peakCount2H + 1;
                        }

                    }
                }
                #endregion

                hiddenbearishPDivHistogram[0] = 0.0;
                maxBarOsc = UseOscHighLow && Histogram[1] > Histogram[0] ? CurrentBar - 1 : CurrentBar;
                maxValueOsc = UseOscHighLow && Histogram[1] > Histogram[0] ? Histogram[1] : Histogram[0];

                for (int count = 0; count < peakCount2H; count++) //find smallest divergence setup
                {
line=4280;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh2H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh2H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar2H[count] - 2)[1] &&
                        refPeakValue2H[count] > 0 &&
                        refPeakValue2H[count] < maxValueOsc &&
                        refPeakValue2H[count] < MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2H[count] - 6))[1] &&
                        (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2H[count])[0] > 0))
                    {
                        hiddenbearishPDivHistogram[0] = 1.0;
                        hiddenbearishTriggerCountHistogram[0] = TriggerBars + 1;
                        firstPeakBar2H = refPeakBar2H[count];
                        firstPeakHigh2H = refPeakHigh2H[count];
                        firstOscPeakBar2H = refOscPeakBar2H[count];
                        firstPeakValue2H = refPeakValue2H[count];
                        secondPeakBar2H = CurrentBar;
                        secondPeakHigh2H = refinput[0];
                        secondOscPeakBar2H = maxBarOsc;
                        secondPeakValue2H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidateOsc2");
                            drawBearishDivCandidateOsc2H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidatePrice2");
                            drawBearishDivCandidatePrice2H = true;
                        }
                        if (ShowSetupDots) drawBearSetup2H = true;
                        break;
                    }
                }

                for (int count = peakCount2H - 1; count >= 0; count--) //find largest divergence setup
                {
line=4318;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh2H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh2H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar2H[count] - 2)[1] &&
                        refPeakValue2H[count] > 0 &&
                        refPeakValue2H[count] < maxValueOsc &&
                        refPeakValue2H[count] < MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2H[count] - 6))[1] &&
                        (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2H[count])[0] > 0))
                    {
                        replacementPeakBar2H = refPeakBar2H[count];
                        replacementPeakHigh2H = refPeakHigh2H[count];
                        replacementOscPeakBar2H = refOscPeakBar2H[count];
                        replacementPeakValue2H = refPeakValue2H[count];
                        break;
                    }
                }

                inputref = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                if (hiddenbearishPDivHistogram[0] < 0.5)
                {
line=4339;
                    if (hiddenbearishTriggerCountHistogram[1] > 0)
                    {
                        hiddenbearishTriggerCountHistogram[0] = hiddenbearishTriggerCountHistogram[1] - 1;
                        if (inputref > priorSecondPeakHigh2H)
                        {
                            if (inputref < priorFirstPeakHigh2H)//price stays below
                            {
                                secondPeakBar2H = CurrentBar;
                                secondPeakHigh2H = inputref;
                                if (!hidePlots && ShowDivOnPricePanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                                    updateBearishDivCandidatePrice2H = true;
                                }
                            }
                            else if (inputref < priorReplacementPeakHigh2H)
                            {
                                firstPeakBar2H = replacementPeakBar2H;
                                firstPeakHigh2H = replacementPeakHigh2H;
                                firstOscPeakBar2H = replacementOscPeakBar2H;
                                firstPeakValue2H = replacementPeakValue2H;
                                secondPeakBar2H = CurrentBar;
                                secondPeakHigh2H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidateOsc2");
                                    drawBearishDivCandidateOsc2H = true;
                                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                                    drawBearishDivCandidatePrice2H = true;
                                }
                            }
                            else
                                hiddenbearishTriggerCountHistogram[0] = 0;
                        }
                        if (hiddenbearishTriggerCountHistogram[0] > 0 && Histogram[0] > MAX(Histogram, CurrentBar - firstOscPeakBar2H)[1])
                        {
                            secondOscPeakBar2H = CurrentBar;
                            secondPeakValue2H = Histogram[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBearCandidateOsc2");
                                updateBearishDivCandidateOsc2H = true;
                            }
                        }
                    }
                }

line=4387;
                if (hiddenbearishTriggerCountHistogram[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar2H]) && (Histogram[0] < Histogram[1]) && Close[0] < Open[0])
                    {
                        hiddenbearishCDivHistogram[0] = 1.0;
                        hiddenbearishTriggerCountHistogram[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                            drawBearishDivOnOsc2H = true;
                        if (ShowDivOnPricePanel)
                            drawBearishDivOnPrice2H = true;
                        if (showArrows)
                            drawArrowDown2H = true;
                        RemoveDrawObject("hiddendivBearCandidateOsc2");
                        RemoveDrawObject("hiddendivBearCandidatePrice2");

//                        if (firstPeakBar2H != memfirstPeakBar2H) cptBearHistogramhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstPeakBar2H = firstPeakBar2H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearHistogramhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishHistogramHiddenDivProjection[0] = cptBearHistogramhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else hiddenbearishCDivHistogram[0] = 0.0;
                }
                else
                {
                    hiddenbearishCDivHistogram[0] = 0.0;
                    RemoveDrawObject("hiddendivBearCandidateOsc2");
                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                }
                #endregion

line=4418;
                #region Bullish divergences between price and histogram
                if (bullishTriggerCountHistogram[1] > 0)
                {
                    priorFirstTroughBar2 = firstTroughBar2;
                    priorFirstOscTroughBar2 = firstOscTroughBar2;
                    priorFirstTroughLow2 = firstTroughLow2;
                    priorFirstTroughValue2 = firstTroughValue2;
                    priorReplacementTroughValue2 = replacementTroughValue2;
                    priorSecondTroughValue2 = secondTroughValue2;
                    if (Histogram[0] < firstTroughValue2)
                    {
                        bullishTriggerCountHistogram[0] = 0;
                        RemoveDrawObject("divBullCandidateOsc2");
                        RemoveDrawObject("divBullCandidatePrice2");
                    }
                }
                bool drawBullishDivCandidateOsc2 = false;
                bool drawBullishDivCandidatePrice2 = false;
                bool updateBullishDivCandidateOsc2 = false;
                bool updateBullishDivCandidatePrice2 = false;
                bool drawBullSetup2 = false;
                bool drawBullishDivOnOsc2 = false;
                bool drawBullishDivOnPrice2 = false;
                bool drawArrowUp2 = false;
                bool firstTroughFound2 = false;
                troughCount2 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=4447;
                    int j = 0;
                    if (swingLowType[i] < 0)
                    {
                        refTroughBar2[troughCount2] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow2[troughCount2] = Low[i];
                        else
                            refTroughLow2[troughCount2] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && Histogram[j - 1] < Histogram[j])
                                j = j - 1;
                            refOscTroughBar2[troughCount2] = CurrentBar - j;
                            refTroughValue2[troughCount2] = Histogram[j];
                            troughCount2 = troughCount2 + 1;
                        }
                        else
                        {
                            refOscTroughBar2[troughCount2] = CurrentBar - i;
                            refTroughValue2[troughCount2] = Histogram[i];
                            troughCount2 = troughCount2 + 1;
                        }
                    }
                }
line=4475;
                bullishPDivHistogram[0] = 0.0;
                if (UseOscHighLow && Histogram[1] < Histogram[0])
                {
                    minBarOsc = CurrentBar - 1;
                    minValueOsc = Histogram[1];
                }
                else
                {
                    minBarOsc = CurrentBar;
                    minValueOsc = Histogram[0];
                }
                for (int count = 0; count < troughCount2; count++) //find smallest divergence setup
                {
line=4489;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow2[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        bullishPDivHistogram[0] = 1.0;
                        bullishTriggerCountHistogram[0] = TriggerBars + 1;
                        firstTroughBar2 = refTroughBar2[count];
                        firstTroughLow2 = refTroughLow2[count];
                        firstOscTroughBar2 = refOscTroughBar2[count];
                        firstTroughValue2 = refTroughValue2[count];
                        secondTroughBar2 = CurrentBar;
                        secondTroughLow2 = Low[0];
                        secondOscTroughBar2 = minBarOsc;
                        secondTroughValue2 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc2");
                            drawBullishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice2");
                            drawBullishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup2 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow2[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        bullishPDivHistogram[0] = 1.0;
                        bullishTriggerCountHistogram[0] = TriggerBars + 1;
                        firstTroughBar2 = refTroughBar2[count];
                        firstTroughLow2 = refTroughLow2[count];
                        firstOscTroughBar2 = refOscTroughBar2[count];
                        firstTroughValue2 = refTroughValue2[count];
                        secondTroughBar2 = CurrentBar;
                        secondTroughLow2 = swingInput[0];
                        secondOscTroughBar2 = minBarOsc;
                        secondTroughValue2 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc2");
                            drawBullishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice2");
                            drawBullishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup2 = true;
                        break;
                    }
                }
                for (int count = troughCount2 - 1; count >= 0; count--) //find largest divergence setup
                {
line=4549;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow2[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        replacementTroughBar2 = refTroughBar2[count];
                        replacementTroughLow2 = refTroughLow2[count];
                        replacementOscTroughBar2 = refOscTroughBar2[count];
                        replacementTroughValue2 = refTroughValue2[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow2[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        replacementTroughBar2 = refTroughBar2[count];
                        replacementTroughLow2 = refTroughLow2[count];
                        replacementOscTroughBar2 = refOscTroughBar2[count];
                        replacementTroughValue2 = refTroughValue2[count];
                        break;
                    }
                }

                if (bullishPDivHistogram[0] < 0.5)
                {
line=4574;
                    if (bullishTriggerCountHistogram[1] > 0)
                    {
                        bullishTriggerCountHistogram[0] = bullishTriggerCountHistogram[1] - 1;
                        if (Histogram[0] < priorSecondTroughValue2)
                        {
                            if (Histogram[0] > priorFirstTroughValue2)
                            {
                                secondOscTroughBar2 = CurrentBar;
                                secondTroughValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc2");
                                    updateBullishDivCandidateOsc2 = true;
                                }
                            }
                            else if (Histogram[0] > priorReplacementTroughValue2)
                            {
                                firstTroughBar2 = replacementTroughBar2;
                                firstTroughLow2 = replacementTroughLow2;
                                firstOscTroughBar2 = replacementOscTroughBar2;
                                firstTroughValue2 = replacementTroughValue2;
                                secondOscTroughBar2 = CurrentBar;
                                secondTroughValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc2");
                                    drawBullishDivCandidateOsc2 = true;
                                    RemoveDrawObject("divBullCandidatePrice2");
                                    drawBullishDivCandidatePrice2 = true;
                                }
                            }
                            else
                                bullishTriggerCountHistogram[0] = 0;
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bullishTriggerCountHistogram[0] > 0 && Low[0] < MIN(Low, CurrentBar - firstTroughBar2)[1])
                        {
                            secondTroughBar2 = CurrentBar;
                            secondTroughLow2 = Low[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice2");
                                updateBullishDivCandidatePrice2 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bullishTriggerCountHistogram[0] > 0 && swingInput[0] < MIN(swingInput, CurrentBar - firstTroughBar2)[1])
                        {
                            secondTroughBar2 = CurrentBar;
                            secondTroughLow2 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice2");
                                updateBullishDivCandidatePrice2 = true;
                            }
                        }
                    }
                }

                if (bullishTriggerCountHistogram[0] > 0)
                {
line=4634;
                    if ((Close[0] > Low[CurrentBar - secondTroughBar2]) && (Histogram[0] > Histogram[1]) && Close[0] > Open[0])
                    {
                        bullishCDivHistogram[0] = 1.0;
                        bullishTriggerCountHistogram[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                            drawBullishDivOnOsc2 = true;
                        if (ShowDivOnPricePanel)
                            drawBullishDivOnPrice2 = true;
                        if (showArrows)
                            drawArrowUp2 = true;
                        RemoveDrawObject("divBullCandidateOsc2");
                        RemoveDrawObject("divBullCandidatePrice2");

//                        if (firstTroughBar2 != memfirstTroughBar2) cptBullHistogramdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstTroughBar2 = firstTroughBar2;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullHistogramdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishHistogramDivProjection[0] = cptBullHistogramdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else
                        bullishCDivHistogram[0] = 0.0;
                }
                else
                {
                    bullishCDivHistogram[0] = 0.0;
                    {
                        RemoveDrawObject("divBullCandidateOsc2");
                        RemoveDrawObject("divBullCandidatePrice2");
                    }
                }
                #endregion

                #region Bullish Hidden divergences between price and Histogram #HIDDENDIV
                if (hiddenbullishTriggerCountHistogram[1] > 0)
                {
                    priorFirstTroughBar2H = firstTroughBar2H;
                    priorFirstOscTroughBar2H = firstOscTroughBar2H;
                    priorFirstTroughLow2H = firstTroughLow2H;
                    priorFirstTroughValue2H = firstTroughValue2H;
                    priorReplacementTroughLow2H = replacementTroughLow2H;
                    priorReplacementTroughValue2H = replacementTroughValue2H;
                    priorSecondTroughValue2H = secondTroughValue2H;
                    priorSecondTroughLow2H = secondTroughLow2H;

                    double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                    if (refinput < firstTroughLow2H)
                    {
                        hiddenbullishTriggerCountHistogram[0] = 0;
                        RemoveDrawObject("hiddendivBullCandidateOsc2");
                        RemoveDrawObject("hiddendivBullCandidatePrice2");
                    }
                }

                #region -- reset variables --
                drawBullishDivCandidateOsc2H = false;
                drawBullishDivCandidatePrice2H = false;
                updateBullishDivCandidateOsc2H = false;
                updateBullishDivCandidatePrice2H = false;
                drawBullSetup2H = false;
                drawBullishDivOnOsc2H = false;
                drawBullishDivOnPrice2H = false;
                drawArrowUp2H = false;
                firstTroughFound2H = false;
                troughCount2H = 0;
                #endregion

                #region -- get price top and osc top --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=4703;
                    int j = 0;
                    if (swingLowType[i] < 0)
                    {
                        refTroughBar2H[troughCount2H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow2H[troughCount2H] = Low[i];
                        else
                            refTroughLow2H[troughCount2H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && Histogram[j - 1] < Histogram[j])
                                j = j - 1;
                            refOscTroughBar2H[troughCount2H] = CurrentBar - j;
                            refTroughValue2H[troughCount2H] = Histogram[j];
                            troughCount2H++;
                        }
                        else
                        {
                            refOscTroughBar2H[troughCount2H] = CurrentBar - i;
                            refTroughValue2H[troughCount2H] = Histogram[i];
                            troughCount2H++;
                        }
                    }
                }
                #endregion

                hiddenbullishPDivHistogram[0] = 0.0;
                minBarOsc = UseOscHighLow && Histogram[1] < Histogram[0] ? CurrentBar - 1 : CurrentBar;
                minValueOsc = UseOscHighLow && Histogram[1] < Histogram[0] ? Histogram[1] : Histogram[0];

                for (int count = 0; count < troughCount2H; count++) //find smallest divergence setup
                {
line=4740;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow2H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow2H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar2H[count] - 2)[1] &&
                        refTroughValue2H[count] < 0 &&
                        refTroughValue2H[count] > minValueOsc &&
                        refTroughValue2H[count] > MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2H[count] - 6))[1] &&
                        (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2H[count])[0] < 0))
                    {
                        hiddenbullishPDivHistogram[0] = 1.0;
                        hiddenbullishTriggerCountHistogram[0] = TriggerBars + 1;
                        firstTroughBar2H = refTroughBar2H[count];
                        firstTroughLow2H = refTroughLow2H[count];
                        firstOscTroughBar2H = refOscTroughBar2H[count];
                        firstTroughValue2H = refTroughValue2H[count];
                        secondTroughBar2H = CurrentBar;
                        secondTroughLow2H = refinput[0];
                        secondOscTroughBar2H = maxBarOsc;
                        secondTroughValue2H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidateOsc2");
                            drawBullishDivCandidateOsc2H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidatePrice2");
                            drawBullishDivCandidatePrice2H = true;
                        }
                        if (ShowSetupDots) drawBullSetup2H = true;
                        break;
                    }
                }

                for (int count = troughCount2H - 1; count >= 0; count--) //find largest divergence setup
                {
line=4778;
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow2H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow2H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar2H[count] - 2)[1] &&
                        refTroughValue2H[count] < 0 &&
                        refTroughValue2H[count] > minValueOsc &&
                        refTroughValue2H[count] > MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2H[count] - 6))[1] &&
                        (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2H[count])[0] < 0))
                    {
                        replacementTroughBar2H = refOscTroughBar2H[count];
                        replacementTroughLow2H = refTroughLow2H[count];
                        replacementOscTroughBar2H = refOscTroughBar2H[count];
                        replacementTroughValue2H = refTroughValue2H[count];
                        break;
                    }
                }

                inputref = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                if (hiddenbullishPDivHistogram[0] < 0.5)
                {
line=4799;
                    if (hiddenbullishTriggerCountHistogram[1] > 0)
                    {
                        hiddenbullishTriggerCountHistogram[0] = hiddenbullishTriggerCountHistogram[1] - 1;
                        if (inputref < priorSecondTroughLow2H)
                        {
                            if (inputref > priorFirstTroughLow2H)//price stays above
                            {
                                secondTroughBar2H = CurrentBar;
                                secondTroughLow2H = inputref;
                                if (!hidePlots && ShowDivOnPricePanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                                    updateBullishDivCandidatePrice2H = true;
                                }
                            }
                            else if (inputref > priorReplacementTroughLow2H)
                            {
                                firstTroughBar2H = replacementTroughBar2H;
                                firstTroughLow2H = replacementTroughLow2H;
                                firstOscTroughBar2H = replacementOscTroughBar2H;
                                firstTroughValue2H = replacementTroughValue2H;
                                secondTroughBar2H = CurrentBar;
                                secondTroughLow2H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidateOsc2");
                                    drawBullishDivCandidateOsc2H = true;
                                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                                    drawBullishDivCandidatePrice2H = true;
                                }
                            }
                            else
                                hiddenbullishTriggerCountHistogram[0] = 0;
                        }
                        if (hiddenbullishTriggerCountHistogram[0] > 0 && Histogram[0] < MIN(Histogram, CurrentBar - firstOscTroughBar2H)[1])
                        {
                            secondOscTroughBar2H = CurrentBar;
                            secondTroughValue2H = Histogram[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBullCandidateOsc2");
                                updateBullishDivCandidateOsc2H = true;
                            }
                        }
                    }
                }

                if (hiddenbullishTriggerCountHistogram[0] > 0)
                {
line=4849;
                    if ((Close[0] > Low[CurrentBar - secondTroughBar2H]) && (Histogram[0] > Histogram[1]) && Close[0] > Open[0])
                    {
                        hiddenbullishCDivHistogram[0] = 1.0;
                        hiddenbullishTriggerCountHistogram[0] = 0;
                        if (!hidePlots && ShowDivOnOscillatorPanel) drawBullishDivOnOsc2H = true;
                        if (ShowDivOnPricePanel) drawBullishDivOnPrice2H = true;
                        if (showArrows) drawArrowUp2H = true;
                        RemoveDrawObject("hiddendivBullCandidateOsc2");
                        RemoveDrawObject("hiddendivBullCandidatePrice2");

//                        if (firstTroughBar2H != memfirstTroughBar2H) cptBullHistogramhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        memfirstTroughBar2H = firstTroughBar2H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullHistogramhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishHistogramHiddenDivProjection[0] = cptBullHistogramhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
                    else hiddenbullishCDivHistogram[0] = 0.0;
                }
                else
                {
                    hiddenbullishCDivHistogram[0] = 0.0;
                    RemoveDrawObject("hiddendivBullCandidateOsc2");
                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                }
                #endregion

                #region -- draw --
                if (drawObjectsEnabled)
                {
                    bearishDivPlotSeriesMACDBB[0] = 0;
                    bullishDivPlotSeriesMACDBB[0] = 0;
                    bearishDivPlotSeriesHistogram[0] = 0;
                    bullishDivPlotSeriesHistogram[0] = 0;
                    hiddenbearishDivPlotSeriesMACDBB[0] = 0;//#HIDDENDIV
                    hiddenbullishDivPlotSeriesMACDBB[0] = 0; ;//#HIDDENDIV
                    hiddenbearishDivPlotSeriesHistogram[0] = 0;//#HIDDENDIV
                    hiddenbullishDivPlotSeriesHistogram[0] = 0;//#HIDDENDIV

                    DrawOnPricePanel = false;
                    #region -- MACD div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowOscillatorDivergences)
                    {
line=4891;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergenceOP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergenceOP1{0}",cutoffIdx));
						}
    	                    if (drawBearishDivCandidateOsc1 && !IsDebug)
        	                    TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidateOsc1", false, CurrentBar - firstOscPeakBar1, firstPeakValue1, CurrentBar - secondOscPeakBar1, secondPeakValue1, BearColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
            	            if (updateBearishDivCandidateOsc1 && !IsDebug)
                	            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidateOsc1", false, CurrentBar - priorFirstOscPeakBar1, priorFirstPeakValue1, CurrentBar - secondOscPeakBar1, secondPeakValue1, BearColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (drawBearishDivOnOsc1)
    	                    {
line=4902;
        	                    if(!IsDebug){
           	        	            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BearishDivergenceOP1{0}", secondPeakBar1), false, CurrentBar - firstOscPeakBar1, firstPeakValue1, CurrentBar - secondOscPeakBar1, secondPeakValue1, BearColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
								}
                        	}

	   	                    if (drawBullishDivCandidateOsc1 && !IsDebug)
	       	                    TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidateOsc1", false, CurrentBar - firstOscTroughBar1, firstTroughValue1, CurrentBar - secondOscTroughBar1, secondTroughValue1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	           	            if (updateBullishDivCandidateOsc1 && !IsDebug)
	               	            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidateOsc1", false, CurrentBar - priorFirstOscTroughBar1, priorFirstTroughValue1, CurrentBar - secondOscTroughBar1, secondTroughValue1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (drawBullishDivOnOsc1)
	   	                    {
	       	                    if(!IsDebug){
          	        	            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BullishDivergenceOP1{0}", secondTroughBar1), false, CurrentBar - firstOscTroughBar1, firstTroughValue1, CurrentBar - secondOscTroughBar1, secondTroughValue1, BullColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
								}
	                        }
                    }
                    #endregion

                    #region -- MACD hidden div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowOscillatorHiddenDivergences)
                    {
line=4928;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergenceOP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergenceOP1{0}",cutoffIdx));
						}
                        //Bearish
							if (drawBearishDivCandidateOsc1H && !IsDebug)
							    TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidateOsc1", false, CurrentBar - firstOscPeakBar1H, firstPeakValue1H, CurrentBar - secondOscPeakBar1H, secondPeakValue1H, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
							if (updateBearishDivCandidateOsc1H && !IsDebug)
							    TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidateOsc1", false, CurrentBar - priorFirstOscPeakBar1H, priorFirstPeakValue1H, CurrentBar - secondOscPeakBar1H, secondPeakValue1H, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
							if (drawBearishDivOnOsc1H)
							{
							    if(!IsDebug){
							        TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBearishDivergenceOP1{0}", secondPeakBar1H), false, CurrentBar - firstOscPeakBar1H, firstPeakValue1H, CurrentBar - secondOscPeakBar1H, secondPeakValue1H, HiddenBearColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
								}
							}
							//-------
							//Bullish
							if (drawBullishDivCandidateOsc1H && !IsDebug)
							    TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidateOsc1", false, CurrentBar - firstOscTroughBar1H, firstTroughValue1H, CurrentBar - secondOscTroughBar1H, secondTroughValue1H, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
							if (updateBullishDivCandidateOsc1H && !IsDebug)
							    TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidateOsc1", false, CurrentBar - priorFirstOscTroughBar1H, priorFirstTroughValue1H, CurrentBar - secondOscTroughBar1H, secondTroughValue1H, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
							if (drawBullishDivOnOsc1H)
							{
							    if(!IsDebug){
							        TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBullishDivergenceOP1{0}", secondTroughBar1H), false, CurrentBar - firstOscTroughBar1H, firstTroughValue1H, CurrentBar - secondOscTroughBar1H, secondTroughValue1H, HiddenBullColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
								}
							}
                    }
                    #endregion

                    #region -- Histo div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowHistogramDivergences)
                    {
line=4966;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergenceOP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergenceOP2{0}",cutoffIdx));
						}
                        if (drawBearishDivCandidateOsc2 && !IsDebug)
                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidateOsc2", false, CurrentBar - firstOscPeakBar2, firstPeakValue2, CurrentBar - secondOscPeakBar2, secondPeakValue2, BearColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
                        if (updateBearishDivCandidateOsc2 && !IsDebug)
                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidateOsc2", false, CurrentBar - priorFirstOscPeakBar2, priorFirstPeakValue2, CurrentBar - secondOscPeakBar2, secondPeakValue2, BearColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
                        if (drawBearishDivOnOsc2)
                        {
                            if(!IsDebug){
                                TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BearishDivergenceOP2{0}", secondPeakBar2), false, CurrentBar - firstOscPeakBar2, firstPeakValue2, CurrentBar - secondOscPeakBar2, secondPeakValue2, BearColor2, DashStyleHelper.Solid, DivWidth2);},0,null);
							}
                        }
                        if (drawBullishDivCandidateOsc2 && !IsDebug)
                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidateOsc2", false, CurrentBar - firstOscTroughBar2, firstTroughValue2, CurrentBar - secondOscTroughBar2, secondTroughValue2, BullColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
                        if (updateBullishDivCandidateOsc2 && !IsDebug)
                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidateOsc2", false, CurrentBar - priorFirstOscTroughBar2, priorFirstTroughValue2, CurrentBar - secondOscTroughBar2, secondTroughValue2, BullColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
                        if (drawBullishDivOnOsc2)
                        {
                            if(!IsDebug){
                                TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BullishDivergenceOP2{0}", secondTroughBar2), false, CurrentBar - firstOscTroughBar2, firstTroughValue2, CurrentBar - secondOscTroughBar2, secondTroughValue2, BullColor2, DashStyleHelper.Solid, DivWidth2);},0,null);
							}
                        }
                    }
                    #endregion

                    #region -- Histo hidden div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowHistogramHiddenDivergences)
                    {
line=5001;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergenceOP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergenceOP2{0}",cutoffIdx));
						}
	                        //Bearish
	                        if (drawBearishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidateOsc2", false, CurrentBar - firstOscPeakBar2H, firstPeakValue2H, CurrentBar - secondOscPeakBar2H, secondPeakValue2H, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (updateBearishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidateOsc2", false, CurrentBar - priorFirstOscPeakBar2H, priorFirstPeakValue2H, CurrentBar - secondOscPeakBar2H, secondPeakValue2H, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (drawBearishDivOnOsc2H)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBearishDivergenceOP2{0}", secondPeakBar2H), false, CurrentBar - firstOscPeakBar2H, firstPeakValue2H, CurrentBar - secondOscPeakBar2H, secondPeakValue2H, HiddenBearColor2, DashStyleHelper.Solid, HiddenDivWidth2);},0,null);
								}
	                        }
	                        //-------
	                        //Bullish
	                        if (drawBullishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidateOsc2", false, CurrentBar - firstOscTroughBar2H, firstTroughValue2H, CurrentBar - secondOscTroughBar2H, secondTroughValue2H, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (updateBullishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidateOsc2", false, CurrentBar - priorFirstOscTroughBar2H, priorFirstTroughValue2H, CurrentBar - secondOscTroughBar2H, secondTroughValue2H, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (drawBullishDivOnOsc2H)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBullishDivergenceOP2{0}", secondTroughBar2H), false, CurrentBar - firstOscTroughBar2H, firstTroughValue2H, CurrentBar - secondOscTroughBar2H, secondTroughValue2H, HiddenBullColor2, DashStyleHelper.Solid, HiddenDivWidth2);},0,null);
								}
	                        }
                    }
                    #endregion

                    DrawOnPricePanel = true;
                    #region -- MACD div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowOscillatorDivergences)
                    {
line=5040;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergencePP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergencePP1{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidatePrice1", false, CurrentBar - firstPeakBar1, firstPeakHigh1 + offsetDiv1, CurrentBar - secondPeakBar1, secondPeakHigh1 + offsetDiv1, BearColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (updateBearishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidatePrice1", false, CurrentBar - priorFirstPeakBar1, priorFirstPeakHigh1 + offsetDiv1, CurrentBar - secondPeakBar1, secondPeakHigh1 + offsetDiv1, BearColor1, DashStyleHelper.Dash, DivWidth1);},0,null); // changed
	                        if (drawBearishDivOnPrice1){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BearishDivergencePP1{0}", secondPeakBar1), false, CurrentBar - firstPeakBar1, firstPeakHigh1 + offsetDiv1, CurrentBar - secondPeakBar1, secondPeakHigh1 + offsetDiv1, BearColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
							}

	                        if (drawBullishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidatePrice1", false, CurrentBar - firstTroughBar1, firstTroughLow1 - offsetDiv1, CurrentBar - secondTroughBar1, secondTroughLow1 - offsetDiv1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null); // changed
	                        if (updateBullishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidatePrice1", false, CurrentBar - priorFirstTroughBar1, priorFirstTroughLow1 - offsetDiv1, CurrentBar - secondTroughBar1, secondTroughLow1 - offsetDiv1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null);// changed
	                        if (drawBullishDivOnPrice1){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BullishDivergencePP1{0}", secondTroughBar1), false, CurrentBar - firstTroughBar1, firstTroughLow1 - offsetDiv1, CurrentBar - secondTroughBar1, secondTroughLow1 - offsetDiv1, BullColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowOscillatorDivergences)
                    {
line=5063;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearSetup1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullSetup1{0}",cutoffIdx));
						}
                        if (drawBearSetup1){
	                        TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BearSetup1{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw1, -SetupFontSize1, BearishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
        	            if (drawBullSetup1){
            	            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BullSetup1{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw1, SetupFontSize1, BullishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowOscillatorDivergences)
                    {
line=5077;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearTrigger1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullTrigger1{0}",cutoffIdx));
						}
                        if (drawArrowDown1){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BearTrigger1{0}", CurrentBar), true, arrowStringDown, 0, High[0] + offsetDraw1, 2 * TriangleFontSize1 / 3, ArrowDownColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                        if (drawArrowUp1){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BullTrigger1{0}", CurrentBar), true, arrowStringUp, 0, Low[0] - offsetDraw1, -2 * TriangleFontSize1 / 3, ArrowUpColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    #endregion

                    #region -- MACD hidden div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowOscillatorHiddenDivergences)
                    {
line=5094;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergencePP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergencePP1{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidatePrice1", false, CurrentBar - firstPeakBar1H, firstPeakHigh1H + offsetDiv1, CurrentBar - secondPeakBar1H, secondPeakHigh1H + offsetDiv1, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1); },0,null);// changed
	                        if (updateBearishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidatePrice1", false, CurrentBar - priorFirstPeakBar1H, priorFirstPeakHigh1H + offsetDiv1, CurrentBar - secondPeakBar1H, secondPeakHigh1H + offsetDiv1, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null); // changed
	                        if (drawBearishDivOnPrice1H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBearishDivergencePP1{0}", secondPeakBar1H), false, CurrentBar - firstPeakBar1H, firstPeakHigh1H + offsetDiv1, CurrentBar - secondPeakBar1H, secondPeakHigh1H + offsetDiv1, HiddenBearColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
							}

	                        if (drawBullishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidatePrice1", false, CurrentBar - firstTroughBar1H, firstTroughLow1H - offsetDiv1, CurrentBar - secondTroughBar1H, secondTroughLow1H - offsetDiv1, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null); // changed
	                        if (updateBullishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidatePrice1", false, CurrentBar - priorFirstTroughBar1H, priorFirstTroughLow1H - offsetDiv1, CurrentBar - secondTroughBar1H, secondTroughLow1H - offsetDiv1, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);// changed
	                        if (drawBullishDivOnPrice1H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBullishDivergencePP1{0}", secondTroughBar1H), false, CurrentBar - firstTroughBar1H, firstTroughLow1H - offsetDiv1, CurrentBar - secondTroughBar1H, secondTroughLow1H - offsetDiv1, HiddenBullColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowOscillatorHiddenDivergences)
                    {
line=5117;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearSetup1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullSetup1{0}",cutoffIdx));
						}
                        if (drawBearSetup1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBearSetup1{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw1, -SetupFontSize1, HiddenBearishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                        if (drawBullSetup1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBullSetup1{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw1, SetupFontSize1, HiddenBullishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }// 
                    if (PrintMarkers && showArrows && ShowOscillatorHiddenDivergences)
                    {
line=5131;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearTrigger1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullTrigger1{0}",cutoffIdx));
						}
                        if (drawArrowDown1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBearTrigger1{0}", CurrentBar), true, arrowStringDown, 0, High[0] + offsetDraw1, 2 * TriangleFontSize1 / 3, HiddenArrowDownColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                        if (drawArrowUp1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBullTrigger1{0}", CurrentBar), true, arrowStringUp, 0, Low[0] - offsetDraw1, -2 * TriangleFontSize1 / 3, HiddenArrowUpColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                    }
                    #endregion

                    #region -- HISTO div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowHistogramDivergences)
                    {
line=5148;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergencePP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergencePP2{0}",cutoffIdx));
						}
						if(!IsDebug)
						{
	                        if (drawBearishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidatePrice2", false, CurrentBar - firstPeakBar2, firstPeakHigh2 + offsetDiv2, CurrentBar - secondPeakBar2, secondPeakHigh2 + offsetDiv2, BearColor2, DashStyleHelper.Dash, DivWidth2); },0,null);
	                        if (updateBearishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBearCandidatePrice2", false, CurrentBar - priorFirstPeakBar2, priorFirstPeakHigh2 + offsetDiv2, CurrentBar - secondPeakBar2, secondPeakHigh2 + offsetDiv2, BearColor2, DashStyleHelper.Dash, DivWidth2); },0,null);
	                        if (drawBearishDivOnPrice2){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BearishDivergencePP2{0}", secondPeakBar2), false, CurrentBar - firstPeakBar2, firstPeakHigh2 + offsetDiv2, CurrentBar - secondPeakBar2, secondPeakHigh2 + offsetDiv2, BearColor2, DashStyleHelper.Solid, DivWidth2); },0,null);
							}
	                        if (drawBullishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidatePrice2", false, CurrentBar - firstTroughBar2, firstTroughLow2 - offsetDiv2, CurrentBar - secondTroughBar2, secondTroughLow2 - offsetDiv2, BullColor2, DashStyleHelper.Dash, DivWidth2);  },0,null);
	                        if (updateBullishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "divBullCandidatePrice2", false, CurrentBar - priorFirstTroughBar2, priorFirstTroughLow2 - offsetDiv2, CurrentBar - secondTroughBar2, secondTroughLow2 - offsetDiv2, BullColor2, DashStyleHelper.Dash, DivWidth2); },0,null);// changed
	                        if (drawBullishDivOnPrice2){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("BullishDivergencePP2{0}", secondTroughBar2), false, CurrentBar - firstTroughBar2, firstTroughLow2 - offsetDiv2, CurrentBar - secondTroughBar2, secondTroughLow2 - offsetDiv2, BullColor2, DashStyleHelper.Solid, DivWidth2); },0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowHistogramDivergences)
                    {
line=5170;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearSetup2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullSetup2{0}",cutoffIdx));
						}
                        if (drawBearSetup2){
                            TriggerCustomEvent(o1 =>{  Draw.Text(this, string.Format("BearSetup2{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw2, -SetupFontSize2, BearishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                        if (drawBullSetup2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BullSetup2{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw2, SetupFontSize2, BullishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowHistogramDivergences)
                    {
line=5184;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearTrigger2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullTrigger2{0}",cutoffIdx));
						}
                        if (drawArrowDown2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BearTrigger2{0}", CurrentBar), true, arrowStringDown, 0, High[0] + offsetDraw2, 2 * TriangleFontSize2 / 3, ArrowDownColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                        if (drawArrowUp2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("BullTrigger2{0}", CurrentBar), true, arrowStringUp, 0, Low[0] - offsetDraw2, -2 * TriangleFontSize2 / 3, ArrowUpColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                    }
                    #endregion

                    #region -- HISTO hidden div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowHistogramHiddenDivergences)
                    {
line=5201;
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergencePP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergencePP2{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidatePrice2", false, CurrentBar - firstPeakBar2H, firstPeakHigh2H + offsetDiv2, CurrentBar - secondPeakBar2H, secondPeakHigh2H + offsetDiv2, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2); },0,null); // changed
	                        if (updateBearishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBearCandidatePrice2", false, CurrentBar - priorFirstPeakBar2H, priorFirstPeakHigh2H + offsetDiv2, CurrentBar - secondPeakBar2H, secondPeakHigh2H + offsetDiv2, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2); },0,null); // changed
	                        if (drawBearishDivOnPrice2H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBearishDivergencePP2{0}", secondPeakBar2H), false, CurrentBar - firstPeakBar2H, firstPeakHigh2H + offsetDiv2, CurrentBar - secondPeakBar2H, secondPeakHigh2H + offsetDiv2, HiddenBearColor2, DashStyleHelper.Solid, HiddenDivWidth2); },0,null);
							}

	                        if (drawBullishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidatePrice2", false, CurrentBar - firstTroughBar2H, firstTroughLow2H - offsetDiv2, CurrentBar - secondTroughBar2H, secondTroughLow2H - offsetDiv2, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);  },0,null);// changed
	                        if (updateBullishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, "hiddendivBullCandidatePrice2", false, CurrentBar - priorFirstTroughBar2H, priorFirstTroughLow2H - offsetDiv2, CurrentBar - secondTroughBar2H, secondTroughLow2H - offsetDiv2, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2); },0,null);// changed
	                        if (drawBullishDivOnPrice2H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this, string.Format("hiddenBullishDivergencePP2{0}", secondTroughBar2H), false, CurrentBar - firstTroughBar2H, firstTroughLow2H - offsetDiv2, CurrentBar - secondTroughBar2H, secondTroughLow2H - offsetDiv2, HiddenBullColor2, DashStyleHelper.Solid, HiddenDivWidth2); },0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowHistogramHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearSetup2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullSetup2{0}",cutoffIdx));
						}
                        if (drawBearSetup2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBearSetup2{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw2, -SetupFontSize2, HiddenBearishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                        if (drawBullSetup2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBullSetup2{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw2, SetupFontSize2, HiddenBullishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowHistogramHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearTrigger2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullTrigger2{0}",cutoffIdx));
						}
                        if (drawArrowDown2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBearTrigger2{0}", CurrentBar), true, arrowStringDown, 0, High[0] + offsetDraw2, 2 * TriangleFontSize2 / 3, HiddenArrowDownColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                        if (drawArrowUp2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this, string.Format("hiddenBullTrigger2{0}", CurrentBar), true, arrowStringUp, 0, Low[0] - offsetDraw2, -2 * TriangleFontSize2 / 3, HiddenArrowUpColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0); },0,null);
						}
                    }
                    #endregion
                }
                #endregion
            }
            else
            {
line=5254;
                #region Bearish divergences between price and BBMACD
                bool drawBearishDivOnOsc1 = false;
                bool drawBearishDivOnPrice1 = false;
                bool drawArrowDown1 = false;
                if (IsFirstTickOfBar)
                {
                    if (bearishTriggerCountMACDBB[1] > 0)
                    {
                        if ((Close[1] < High[CurrentBar - secondPeakBar1]) && (BBMACD[1] < BBMACD[2]) && Close[1] < Open[1])
                        {
                            bearishCDivMACD[0] = (1.0);
                            bearishTriggerCountMACDBB[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                                drawBearishDivOnOsc1 = true;
                            if (ShowDivOnPricePanel)
                                drawBearishDivOnPrice1 = true;
                            if (showArrows)
                                drawArrowDown1 = true;
                            RemoveDrawObject("divBearCandidateOsc1");
                            RemoveDrawObject("divBearCandidatePrice1");
//                            maxcptBearMACDdiv++;
                        }
                        else
                            bearishCDivMACD[0] = (0.0);
                        priorFirstPeakBar1 = firstPeakBar1;
                        priorFirstOscPeakBar1 = firstOscPeakBar1;
                        priorFirstPeakHigh1 = firstPeakHigh1;
                        priorFirstPeakValue1 = firstPeakValue1;
                        priorReplacementPeakValue1 = replacementPeakValue1;
                        priorSecondPeakBar1 = secondPeakBar1;
                        priorSecondOscPeakBar1 = secondOscPeakBar1;
                        priorSecondPeakHigh1 = secondPeakHigh1;
                        priorSecondPeakValue1 = secondPeakValue1;
                        if (BBMACD[1] > firstPeakValue1)
                        {
                            bearishTriggerCountMACDBB[1] = (0);
                            RemoveDrawObject("divBearCandidateOsc1");
                            RemoveDrawObject("divBearCandidatePrice1");
                        }
                    }
                    else
                    {
                        bearishCDivMACD[0] = (0.0);
                        {
                            RemoveDrawObject("divBearCandidateOsc1");
                            RemoveDrawObject("divBearCandidatePrice1");
                        }
                    }
                    bearishPDivMACD[0] = (0.0);
                }
                if (bearishPDivMACD[0] > 0.5)
                {
                    RemoveDrawObject("divBearCandidateOsc1");
                    RemoveDrawObject("divBearCandidatePrice1");
                }
                bool drawBearishDivCandidateOsc1 = false;
                bool drawBearishDivCandidatePrice1 = false;
                bool updateBearishDivCandidateOsc1 = false;
                bool updateBearishDivCandidatePrice1 = false;
                bool drawBearSetup1 = false;
                bool invalidate = swingInput[0] <= swingInput[1] || (IncludeDoubleTopsAndBottoms && swingInput[0] < firstPeakHigh1) || (!IncludeDoubleTopsAndBottoms && swingInput[0] <= firstPeakHigh1);
                if (ThisInputType != HistoDivergence_InputType.High_Low && bearishTriggerCountMACDBB[1] > 0 && invalidate)
                {
line=5318;
                    bearishPDivMACD[0] = (0.0);
                    secondPeakBar1 = priorSecondPeakBar1;
                    secondPeakHigh1 = priorSecondPeakHigh1;
                    updateBearishDivCandidatePrice1 = true;
                }
                else if (ThisInputType != HistoDivergence_InputType.High_Low && bearishPDivMACD[0] > 0.5 && invalidate)
                {
line=5326;
                    bearishPDivMACD[0] = (0.0);
                    bearishTriggerCountMACDBB[0] = (0);
                    secondPeakBar1 = priorSecondPeakBar1;
                    secondPeakHigh1 = priorSecondPeakHigh1;
                    RemoveDrawObject("divBearCandidateOsc1");
                    RemoveDrawObject("divBearCandidatePrice1");
                }
                bool firstPeakFound1 = false;
                peakCount1 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
line=5338;
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMax = MAX(High, SwingStrength)[1];
                    else
                        swingMax = MAX(Input, SwingStrength)[1];
                    if (swingHighType[i] > 0 || (i > DivMinBars && pre_swingHighType[i] > 0 && !upTrend[0] && Low[0] < Math.Min(swingMin, currentHigh - zigzagDeviation))) // code references zigzag
                    {
                        refPeakBar1[peakCount1] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh1[peakCount1] = High[i];
                        else
                            refPeakHigh1[peakCount1] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && BBMACD[j - 1] > BBMACD[j])
                                j = j - 1;
                            refOscPeakBar1[peakCount1] = CurrentBar - j;
                            refPeakValue1[peakCount1] = BBMACD[j];
                            peakCount1 = peakCount1 + 1;
                        }
                        else
                        {
                            refOscPeakBar1[peakCount1] = CurrentBar - i;
                            refPeakValue1[peakCount1] = BBMACD[i];
                            peakCount1 = peakCount1 + 1;
                        }
                    }
                }
                int maxBarOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                double maxValueOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? BBMACD[1] : BBMACD[0];
               
                for (int count = 0; count < peakCount1; count++) //find smallest divergence setup
                {
line=5375;
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh1[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        bearishPDivMACD[0] = (1.0);
                        bearishTriggerCountMACDBB[0] = (TriggerBars + 1);
                        firstPeakBar1 = refPeakBar1[count];
                        firstPeakHigh1 = refPeakHigh1[count];
                        firstOscPeakBar1 = refOscPeakBar1[count];
                        firstPeakValue1 = refPeakValue1[count];
                        secondPeakBar1 = CurrentBar;
                        secondPeakHigh1 = High[0];
                        secondOscPeakBar1 = maxBarOsc;
                        secondPeakValue1 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc1");
                            drawBearishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice1");
                            drawBearishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup1 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh1[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        bearishPDivMACD[0] = (1.0);
                        bearishTriggerCountMACDBB[0] = (TriggerBars + 1);
                        firstPeakBar1 = refPeakBar1[count];
                        firstPeakHigh1 = refPeakHigh1[count];
                        firstOscPeakBar1 = refOscPeakBar1[count];
                        firstPeakValue1 = refPeakValue1[count];
                        secondPeakBar1 = CurrentBar;
                        secondPeakHigh1 = swingInput[0];
                        secondOscPeakBar1 = maxBarOsc;
                        secondPeakValue1 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc1");
                            drawBearishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice1");
                            drawBearishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup1 = true;
                        break;
                    }
                    else
                        bearishPDivMACD[0] = (0.0);
                }
                for (int count = peakCount1 - 1; count >= 0; count--) //find largest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh1[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        replacementPeakBar1 = refPeakBar1[count];
                        replacementPeakHigh1 = refPeakHigh1[count];
                        replacementOscPeakBar1 = refOscPeakBar1[count];
                        replacementPeakValue1 = refPeakValue1[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh1[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar1[count] - 2)[1] && refPeakValue1[count] > 0 && refPeakValue1[count] > maxValueOsc
                        && refPeakValue1[count] > MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1[count] - 6))[1] && (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1[count])[0] > 0))
                    {
                        replacementPeakBar1 = refPeakBar1[count];
                        replacementPeakHigh1 = refPeakHigh1[count];
                        replacementOscPeakBar1 = refOscPeakBar1[count];
                        replacementPeakValue1 = refPeakValue1[count];
                        break;
                    }
                }
                if (bearishPDivMACD[0] < 0.5)
                {
                    divergenceActive = true;
                    if (bearishTriggerCountMACDBB[1] > 0)
                    {
                        bearishTriggerCountMACDBB[0] = (bearishTriggerCountMACDBB[1] - 1);
                        if (BBMACD[0] > priorSecondPeakValue1)
                        {
                            if (BBMACD[0] < priorFirstPeakValue1)
                            {
                                secondOscPeakBar1 = CurrentBar;
                                secondPeakValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc1");
                                    updateBearishDivCandidateOsc1 = true;
                                    RemoveDrawObject("divBearCandidatePrice1");
                                    updateBearishDivCandidatePrice1 = true;
                                }
                            }
                            else if (BBMACD[0] < priorReplacementPeakValue1)
                            {
                                firstPeakBar1 = replacementPeakBar1;
                                firstPeakHigh1 = replacementPeakHigh1;
                                firstOscPeakBar1 = replacementOscPeakBar1;
                                firstPeakValue1 = replacementPeakValue1;
                                secondOscPeakBar1 = CurrentBar;
                                secondPeakValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc1");
                                    drawBearishDivCandidateOsc1 = true;
                                    RemoveDrawObject("divBearCandidatePrice1");
                                    drawBearishDivCandidatePrice1 = true;
                                }
                            }
                            else
                            {
                                bearishTriggerCountMACDBB[0] = (0);
                                RemoveDrawObject("divBearCandidateOsc1");
                                RemoveDrawObject("divBearCandidatePrice1");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondOscPeakBar1 = priorSecondOscPeakBar1;
                            secondPeakValue1 = priorSecondPeakValue1;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("divBearCandidateOsc1");
                                updateBearishDivCandidateOsc1 = true;
                                RemoveDrawObject("divBearCandidatePrice1");
                                updateBearishDivCandidatePrice1 = true;
                            }
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bearishTriggerCountMACDBB[0] > 0 && High[0] > MAX(High, CurrentBar - firstPeakBar1)[1])
                        {
                            secondPeakBar1 = CurrentBar;
                            secondPeakHigh1 = High[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice1");
                                if (divergenceActive)
                                    updateBearishDivCandidatePrice1 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bearishTriggerCountMACDBB[0] > 0 && swingInput[0] > MAX(swingInput, CurrentBar - firstPeakBar1)[1])
                        {
                            secondPeakBar1 = CurrentBar;
                            secondPeakHigh1 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice1");
                                if (divergenceActive)
                                    updateBearishDivCandidatePrice1 = true;
                            }
                        }
                    }
                }

                if (bearishTriggerCountMACDBB[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar1]) && (BBMACD[0] < BBMACD[1]) && Close[0] < Open[0])
                    {
//                        if (firstPeakBar1 != memfirstPeakBar1)
//                        {
//                            cptBearMACDdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBearMACDdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstPeakBar1 = firstPeakBar1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearMACDdiv = maxcptBearMACDdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishMACDDivProjection[0] = (cptBearMACDdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bearishMACDDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bearishMACDDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec

                #endregion

                #region Bearish Hidden divergences between price and BBMACD #HIDDENDIV
                drawBearishDivOnOsc1H = false;
                drawBearishDivOnPrice1H = false;
                drawArrowDown1H = false;
                drawBearSetup1H = false;

                #region -- IsFirstTickOfBar - {Reset} --
                if (IsFirstTickOfBar)
                {
                    if (hiddenbearishTriggerCountMACDBB[1] > 0)
                    {
                        if ((Close[1] < High[CurrentBar - secondPeakBar1H]) && (BBMACD[1] < BBMACD[2]) && Close[1] < Open[1])
                        {
                            hiddenbearishCDivMACD[0] = (1.0);
                            hiddenbearishTriggerCountMACDBB[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel) drawBearishDivOnOsc1H = true;
                            if (ShowDivOnPricePanel) drawBearishDivOnPrice1H = true;
                            if (showArrows) drawArrowDown1H = true;
                            RemoveDrawObject("hiddendivBearCandidateOsc1");
                            RemoveDrawObject("hiddendivBearCandidatePrice1");
//                            maxcptBearMACDhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
                        }
                        else
                            hiddenbearishCDivMACD[0] = (0.0);

                        priorFirstPeakBar1H = firstPeakBar1H;
                        priorFirstOscPeakBar1H = firstOscPeakBar1H;
                        priorFirstPeakHigh1H = firstPeakHigh1H;
                        priorFirstPeakValue1H = firstPeakValue1H;
                        priorReplacementPeakHigh1H = replacementPeakHigh1H;
                        priorReplacementPeakValue1H = replacementPeakValue1H;
                        priorSecondPeakBar1H = secondPeakBar1H;
                        priorSecondOscPeakBar1H = secondOscPeakBar1H;
                        priorSecondPeakValue1H = secondPeakValue1H;
                        priorSecondPeakHigh1H = secondPeakHigh1H;

                        double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High[1] : swingInput[1];
                        if (refinput > firstPeakHigh1H)
                        {
                            hiddenbearishTriggerCountMACDBB[1] = (0);
                            RemoveDrawObject("hiddendivBearCandidateOsc1");
                            RemoveDrawObject("hiddendivBearCandidatePrice1");
                        }
                    }
                    else
                    {
                        hiddenbearishCDivMACD[0] = (0.0);
                        RemoveDrawObject("hiddendivBearCandidateOsc1");
                        RemoveDrawObject("hiddendivBearCandidatePrice1");
                    }
                    hiddenbearishPDivMACD[0] = (0.0);
                }
                #endregion

                if (hiddenbearishPDivMACD[0] > 0.5)
                {
                    RemoveDrawObject("hiddendivBearCandidateOsc1");
                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                }

                #region -- reset variables --
                drawBearishDivCandidateOsc1H = false;
                drawBearishDivCandidatePrice1H = false;
                updateBearishDivCandidateOsc1H = false;
                updateBearishDivCandidatePrice1H = false;
                peakCount1H = 0;
                #endregion

                double refinput1H_0 = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                double refinput1H_1 = ThisInputType == HistoDivergence_InputType.High_Low ? High[1] : swingInput[1];
                bool invalidate1H = BBMACD[0] < MAX(BBMACD, CurrentBar - firstOscPeakBar1H)[1] || refinput1H_0 <= refinput1H_1 ||
                    (IncludeDoubleTopsAndBottoms && refinput1H_0 > firstPeakHigh1H) || (!IncludeDoubleTopsAndBottoms && refinput1H_0 >= firstPeakHigh1H);
                if (hiddenbearishTriggerCountMACDBB[1] > 0 && invalidate1H)
                {
                    hiddenbearishPDivMACD[0] = (0.0);
                    secondOscPeakBar1H = priorSecondOscPeakBar1H;
                    secondPeakValue1H = priorSecondPeakValue1H;
                    updateBearishDivCandidateOsc1H = true;
                }
                else if (hiddenbearishPDivMACD[0] > 0.5 && invalidate1H)
                {
                    hiddenbearishPDivMACD[0] = (0.0);
                    hiddenbearishTriggerCountMACDBB[0] = (0);
                    secondOscPeakBar1H = priorSecondOscPeakBar1H;
                    secondPeakValue1H = priorSecondPeakValue1H;
                    RemoveDrawObject("hiddendivBearCandidateOsc1");
                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                }

                #region -- get price top and osc top --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMax = MAX(High, SwingStrength)[1];
                    else
                        swingMax = MAX(Input, SwingStrength)[1];
                    if (swingHighType[i] > 0 || (i > DivMinBars && pre_swingHighType[i] > 0 && !upTrend[0] && Low[0] < Math.Min(swingMin, currentHigh - zigzagDeviation))) // code references zigzag
                    {
                        refPeakBar1H[peakCount1H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh1H[peakCount1H] = High[i];
                        else
                            refPeakHigh1H[peakCount1H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && BBMACD[j - 1] > BBMACD[j])
                                j = j - 1;
                            refOscPeakBar1H[peakCount1H] = CurrentBar - j;
                            refPeakValue1H[peakCount1H] = BBMACD[j];
                            peakCount1H = peakCount1H + 1;
                        }
                        else
                        {
                            refOscPeakBar1H[peakCount1H] = CurrentBar - i;
                            refPeakValue1H[peakCount1H] = BBMACD[i];
                            peakCount1H = peakCount1H + 1;
                        }
                    }
                }
                #endregion

                maxBarOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                maxValueOsc = UseOscHighLow && BBMACD[1] > BBMACD[0] ? BBMACD[1] : BBMACD[0];

                #region -- find smallest divergence setup --
                for (int count = 0; count < peakCount1H; count++)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh1H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh1H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar1H[count] - 2)[1] &&
                        refPeakValue1H[count] > 0 &&
                        refPeakValue1H[count] < maxValueOsc &&
                        refPeakValue1H[count] < MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1H[count] - 6))[1] &&
                        (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1H[count])[0] > 0))
                    {
                        hiddenbearishPDivMACD[0] = (1.0);
                        hiddenbearishTriggerCountMACDBB[0] = (TriggerBars + 1);
                        firstPeakBar1H = refPeakBar1H[count];
                        firstPeakHigh1H = refPeakHigh1H[count];
                        firstOscPeakBar1H = refOscPeakBar1H[count];
                        firstPeakValue1H = refPeakValue1H[count];
                        secondPeakBar1H = CurrentBar;
                        secondPeakHigh1H = refinput[0];
                        secondOscPeakBar1H = maxBarOsc;
                        secondPeakValue1H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidateOsc1");
                            drawBearishDivCandidateOsc1H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidatePrice1");
                            drawBearishDivCandidatePrice1H = true;
                        }
                        if (ShowSetupDots) drawBearSetup1H = true;
                        break;
                    }
                    else hiddenbearishPDivMACD[0] = (0.0);
                }
                #endregion

                #region -- find largest divergence setup --
                for (int count = peakCount1H - 1; count >= 0; count--)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh1H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh1H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar1H[count] - 2)[1] &&
                        refPeakValue1H[count] > 0 &&
                        refPeakValue1H[count] < maxValueOsc &&
                        refPeakValue1H[count] < MAX(BBMACD, Math.Max(1, CurrentBar - refOscPeakBar1H[count] - 6))[1] &&
                        (!ResetFilter || MIN(BBMACD, CurrentBar - refOscPeakBar1H[count])[0] > 0))
                    {
                        replacementPeakBar1H = refPeakBar1H[count];
                        replacementPeakHigh1H = refPeakHigh1H[count];
                        replacementOscPeakBar1H = refOscPeakBar1H[count];
                        replacementPeakValue1H = refPeakValue1H[count];
                        break;
                    }
                }
                #endregion

                double inputref = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                if (hiddenbearishPDivMACD[0] < 0.5)
                {
                    divergenceActive = true;
                    if (hiddenbearishTriggerCountMACDBB[1] > 0)
                    {
                        hiddenbearishTriggerCountMACDBB[0] = (hiddenbearishTriggerCountMACDBB[1] - 1);
                        if (inputref > priorSecondPeakHigh1H)
                        {
                            if (inputref < priorFirstPeakHigh1H)//price stays below
                            {
                                secondPeakBar1H = CurrentBar;
                                secondPeakHigh1H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidateOsc1");
                                    updateBearishDivCandidateOsc1H = true;
                                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                                    updateBearishDivCandidatePrice1H = true;
                                }
                            }
                            else if (inputref < priorReplacementPeakHigh1H)
                            {
                                firstPeakBar1H = replacementPeakBar1H;
                                firstPeakHigh1H = replacementPeakHigh1H;
                                firstOscPeakBar1H = replacementOscPeakBar1H;
                                firstPeakValue1H = replacementPeakValue1H;
                                secondPeakBar1H = CurrentBar;
                                secondPeakHigh1H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidateOsc1");
                                    drawBearishDivCandidateOsc1H = true;
                                    RemoveDrawObject("hiddendivBearCandidatePrice1");
                                    drawBearishDivCandidatePrice1H = true;
                                }
                            }
                            else
                            {
                                hiddenbearishTriggerCountMACDBB[0] = (0);
                                RemoveDrawObject("hiddendivBearCandidateOsc1");
                                RemoveDrawObject("hiddendivBearCandidatePrice1");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondPeakBar1H = priorSecondPeakBar1H;
                            secondPeakHigh1H = priorSecondPeakHigh1H;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBearCandidateOsc1");
                                updateBearishDivCandidateOsc1H = true;
                                RemoveDrawObject("hiddendivBearCandidatePrice1");
                                updateBearishDivCandidatePrice1H = true;
                            }
                        }
                        if (hiddenbearishTriggerCountMACDBB[0] > 0 && BBMACD[0] > MAX(BBMACD, CurrentBar - firstOscPeakBar1H)[1])
                        {
                            secondOscPeakBar1H = CurrentBar;
                            secondPeakValue1H = BBMACD[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBearCandidateOsc1");
                                if (divergenceActive) updateBearishDivCandidateOsc1H = true;
                            }
                        }
                    }
                }

                if (hiddenbearishTriggerCountMACDBB[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar1H]) && (BBMACD[0] < BBMACD[1]) && Close[0] < Open[0])
                    {
//                        if (firstPeakBar1H != memfirstPeakBar1H)
//                        {
//                            cptBearMACDhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBearMACDhdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstPeakBar1H = firstPeakBar1H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearMACDhdiv = maxcptBearMACDhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishMACDHiddenDivProjection[0] = (cptBearMACDhdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bearishMACDHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bearishMACDHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec
                #endregion

                #region Bullish divergences between price and BBMACD
                bool drawBullishDivOnOsc1 = false;
                bool drawBullishDivOnPrice1 = false;
                bool drawArrowUp1 = false;

                if (IsFirstTickOfBar)
                {
                    if (bullishTriggerCountMACDBB[1] > 0)
                    {
                        if ((Close[1] > Low[CurrentBar - secondTroughBar1]) && (BBMACD[1] > BBMACD[2]) && Close[1] > Open[1])
                        {
                            bullishCDivMACD[0] = (1.0);
                            bullishTriggerCountMACDBB[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                                drawBullishDivOnOsc1 = true;
                            if (ShowDivOnPricePanel)
                                drawBullishDivOnPrice1 = true;
                            if (showArrows)
                                drawArrowUp1 = true;
                            RemoveDrawObject("divBullCandidateOsc1");
                            RemoveDrawObject("divBullCandidatePrice1");
//                            maxcptBullMACDdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
                        }
                        else
                            bullishCDivMACD[0] = (0.0);
                        priorFirstTroughBar1 = firstTroughBar1;
                        priorFirstOscTroughBar1 = firstOscTroughBar1;
                        priorFirstTroughLow1 = firstTroughLow1;
                        priorFirstTroughValue1 = firstTroughValue1;
                        priorReplacementTroughValue1 = replacementTroughValue1;
                        priorSecondTroughBar1 = secondTroughBar1;
                        priorSecondOscTroughBar1 = secondOscTroughBar1;
                        priorSecondTroughLow1 = secondTroughLow1;
                        priorSecondTroughValue1 = secondTroughValue1;
                        if (BBMACD[1] < firstTroughValue1)
                        {
                            bullishTriggerCountMACDBB[1] = (0);
                            RemoveDrawObject("divBullCandidateOsc1");
                            RemoveDrawObject("divBullCandidatePrice1");
                        }
                    }
                    else
                    {
                        bullishCDivMACD[0] = (0.0);
                        {
                            RemoveDrawObject("divBullCandidateOsc1");
                            RemoveDrawObject("divBullCandidatePrice1");
                        }
                    }
                    bullishPDivMACD[0] = (0.0);
                }
                if (bullishPDivMACD[0] > 0.5)
                {
                    RemoveDrawObject("divBullCandidateOsc1");
                    RemoveDrawObject("divBullCandidatePrice1");
                }
                bool drawBullishDivCandidateOsc1 = false;
                bool drawBullishDivCandidatePrice1 = false;
                bool updateBullishDivCandidateOsc1 = false;
                bool updateBullishDivCandidatePrice1 = false;
                bool drawBullSetup1 = false;
                invalidate = swingInput[0] >= swingInput[1] || (IncludeDoubleTopsAndBottoms && swingInput[0] > firstTroughLow1) || (!IncludeDoubleTopsAndBottoms && swingInput[0] >= firstTroughLow1);
                if (ThisInputType != HistoDivergence_InputType.High_Low && bullishTriggerCountMACDBB[1] > 0 && invalidate)
                {
                    bullishPDivMACD[0] = (0.0);
                    secondTroughBar1 = priorSecondTroughBar1;
                    secondTroughLow1 = priorSecondTroughLow1;
                    updateBullishDivCandidatePrice1 = true;
                }
                else if (ThisInputType != HistoDivergence_InputType.High_Low && bullishPDivMACD[0] > 0.5 && invalidate)
                {
                    bullishPDivMACD[0] = (0.0);
                    bullishTriggerCountMACDBB[0] = (0);
                    secondTroughBar1 = priorSecondTroughBar1;
                    secondTroughLow1 = priorSecondTroughLow1;
                    RemoveDrawObject("divBullCandidateOsc1");
                    RemoveDrawObject("divBullCandidatePrice1");
                }
                bool firstTroughFound1 = false;
                troughCount1 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMin = MIN(Low, SwingStrength)[1];
                    else
                        swingMin = MIN(Input, SwingStrength)[1];
                    if (swingLowType[i] < 0 || (i > DivMinBars && pre_swingLowType[i] < 0 && upTrend[0] && High[0] > Math.Max(swingMax, currentLow + zigzagDeviation))) // code references zigzag 
                    {
                        refTroughBar1[troughCount1] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow1[troughCount1] = Low[i];
                        else
                            refTroughLow1[troughCount1] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && BBMACD[j - 1] < BBMACD[j])
                                j = j - 1;
                            refOscTroughBar1[troughCount1] = CurrentBar - j;
                            refTroughValue1[troughCount1] = BBMACD[j];
                            troughCount1 = troughCount1 + 1;
                        }
                        else
                        {
                            refOscTroughBar1[troughCount1] = CurrentBar - i;
                            refTroughValue1[troughCount1] = BBMACD[i];
                            troughCount1 = troughCount1 + 1;
                        }
                    }
                }
                int minBarOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                double minValueOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? BBMACD[1] : BBMACD[0];

                for (int count = 0; count < troughCount1; count++) //find smallest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow1[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        bullishPDivMACD[0] = (1.0);
                        bullishTriggerCountMACDBB[0] = (TriggerBars + 1);
                        firstTroughBar1 = refTroughBar1[count];
                        firstTroughLow1 = refTroughLow1[count];
                        firstOscTroughBar1 = refOscTroughBar1[count];
                        firstTroughValue1 = refTroughValue1[count];
                        secondTroughBar1 = CurrentBar;
                        secondTroughLow1 = Low[0];
                        secondOscTroughBar1 = minBarOsc;
                        secondTroughValue1 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc1");
                            drawBullishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice1");
                            drawBullishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup1 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow1[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        bullishPDivMACD[0] = (1.0);
                        bullishTriggerCountMACDBB[0] = (TriggerBars + 1);
                        firstTroughBar1 = refTroughBar1[count];
                        firstTroughLow1 = refTroughLow1[count];
                        firstOscTroughBar1 = refOscTroughBar1[count];
                        firstTroughValue1 = refTroughValue1[count];
                        secondTroughBar1 = CurrentBar;
                        secondTroughLow1 = swingInput[0];
                        secondOscTroughBar1 = minBarOsc;
                        secondTroughValue1 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc1");
                            drawBullishDivCandidateOsc1 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice1");
                            drawBullishDivCandidatePrice1 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup1 = true;
                        break;
                    }
                    else
                        bullishPDivMACD[0] = (0.0);
                }
                for (int count = troughCount1 - 1; count >= 0; count--) //find largest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow1[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        replacementTroughBar1 = refTroughBar1[count];
                        replacementTroughLow1 = refTroughLow1[count];
                        replacementOscTroughBar1 = refOscTroughBar1[count];
                        replacementTroughValue1 = refTroughValue1[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow1[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow1[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar1[count] - 2)[1] && refTroughValue1[count] < 0 && refTroughValue1[count] < minValueOsc
                        && refTroughValue1[count] < MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1[count] - 6))[1] && (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1[count])[0] < 0))
                    {
                        replacementTroughBar1 = refTroughBar1[count];
                        replacementTroughLow1 = refTroughLow1[count];
                        replacementOscTroughBar1 = refOscTroughBar1[count];
                        replacementTroughValue1 = refTroughValue1[count];
                        break;
                    }
                }

                if (bullishPDivMACD[0] < 0.5)
                {
                    divergenceActive = true;
                    if (bullishTriggerCountMACDBB[1] > 0)
                    {
                        bullishTriggerCountMACDBB[0] = (bullishTriggerCountMACDBB[1] - 1);
                        if (BBMACD[0] < priorSecondTroughValue1)
                        {
                            if (BBMACD[0] > priorFirstTroughValue1)
                            {
                                secondOscTroughBar1 = CurrentBar;
                                secondTroughValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc1");
                                    updateBullishDivCandidateOsc1 = true;
                                    RemoveDrawObject("divBullCandidatePrice1");
                                    updateBullishDivCandidatePrice1 = true;
                                }
                            }
                            else if (BBMACD[0] > priorReplacementTroughValue1)
                            {
                                firstTroughBar1 = replacementTroughBar1;
                                firstTroughLow1 = replacementTroughLow1;
                                firstOscTroughBar1 = replacementOscTroughBar1;
                                firstTroughValue1 = replacementTroughValue1;
                                secondOscTroughBar1 = CurrentBar;
                                secondTroughValue1 = BBMACD[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc1");
                                    drawBullishDivCandidateOsc1 = true;
                                    RemoveDrawObject("divBullCandidatePrice1");
                                    drawBullishDivCandidatePrice1 = true;
                                }
                            }
                            else
                            {
                                bullishTriggerCountMACDBB[0] = (0);
                                RemoveDrawObject("divBullCandidateOsc1");
                                RemoveDrawObject("divBullCandidatePrice1");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondOscTroughBar1 = priorSecondOscTroughBar1;
                            secondTroughValue1 = priorSecondTroughValue1;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("divBullCandidateOsc1");
                                updateBullishDivCandidateOsc1 = true;
                                RemoveDrawObject("divBullCandidatePrice1");
                                updateBullishDivCandidatePrice1 = true;
                            }
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bullishTriggerCountMACDBB[0] > 0 && Low[0] < MIN(Low, CurrentBar - firstTroughBar1)[1])
                        {
                            secondTroughBar1 = CurrentBar;
                            secondTroughLow1 = Low[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice1");
                                if (divergenceActive)
                                    updateBullishDivCandidatePrice1 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bullishTriggerCountMACDBB[0] > 0 && swingInput[0] < MIN(swingInput, CurrentBar - firstTroughBar1)[1])
                        {
                            secondTroughBar1 = CurrentBar;
                            secondTroughLow1 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice1");
                                if (divergenceActive)
                                    updateBullishDivCandidatePrice1 = true;
                            }
                        }
                    }
                }

                if (bullishTriggerCountMACDBB[0] > 0)
                {
                    if ((Close[0] > Low[CurrentBar - secondTroughBar1]) && (BBMACD[0] > BBMACD[1]) && Close[0] > Open[0])
                    {
//                        if (firstTroughBar1 != memfirstTroughBar1)
//                        {
//                            cptBullMACDdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBullMACDdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstTroughBar1 = firstTroughBar1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullMACDdiv = maxcptBullMACDdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishMACDDivProjection[0] = (cptBullMACDdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bullishMACDDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bullishMACDDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec

                #endregion

                #region Bullish Hidden divergences between price and BBMACD #HIDDENDIV
                drawBullishDivOnOsc1H = false;
                drawBullishDivOnPrice1H = false;
                drawArrowUp1H = false;
                drawBullSetup1H = false;

                #region -- IsFirstTickOfBar - {Reset} --
                if (IsFirstTickOfBar)
                {
                    if (hiddenbullishTriggerCountMACDBB[1] > 0)
                    {
                        if ((Close[1] > Low[CurrentBar - secondTroughBar1H]) && (BBMACD[1] > BBMACD[2]) && Close[1] > Open[1])
                        {
                            hiddenbullishCDivMACD[0] = (1.0);
                            hiddenbullishTriggerCountMACDBB[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel) drawBullishDivOnOsc1H = true;
                            if (ShowDivOnPricePanel) drawBullishDivOnPrice1H = true;
                            if (showArrows) drawArrowUp1H = true;
                            RemoveDrawObject("hiddendivBullCandidateOsc1");
                            RemoveDrawObject("hiddendivBullCandidatePrice1");
//                            maxcptBullMACDhdiv++;
                        }
                        else hiddenbullishCDivMACD[0] = (0.0);

                        priorFirstTroughBar1H = firstTroughBar1H;
                        priorFirstOscTroughBar1H = firstOscTroughBar1H;
                        priorFirstTroughLow1H = firstTroughLow1H;
                        priorFirstTroughValue1H = firstTroughValue1H;
                        priorReplacementTroughLow1H = replacementTroughLow1H;
                        priorReplacementTroughValue1H = replacementTroughValue1H;
                        priorSecondTroughBar1H = secondTroughBar1H;
                        priorSecondOscTroughBar1H = secondOscTroughBar1H;
                        priorSecondTroughValue1H = secondTroughValue1H;
                        priorSecondTroughLow1H = secondTroughLow1H;

                        double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low[1] : swingInput[1];
                        if (refinput < firstTroughLow1H)
                        {
                            hiddenbullishTriggerCountMACDBB[1] = (0);
                            RemoveDrawObject("hiddendivBullCandidateOsc1");
                            RemoveDrawObject("hiddendivBullCandidatePrice1");
                        }
                    }
                    else
                    {
                        hiddenbullishCDivMACD[0] = (0.0);
                        RemoveDrawObject("hiddendivBullCandidateOsc1");
                        RemoveDrawObject("hiddendivBullCandidatePrice1");
                    }
                    hiddenbullishPDivMACD[0] = (0.0);
                }
                #endregion

                if (hiddenbullishPDivMACD[0] > 0.5)
                {
                    RemoveDrawObject("hiddendivBullCandidateOsc1");
                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                }

                #region -- reset variables --
                drawBullishDivCandidateOsc1H = false;
                drawBullishDivCandidatePrice1H = false;
                updateBullishDivCandidateOsc1H = false;
                updateBullishDivCandidatePrice1H = false;
                troughCount1H = 0;
                #endregion

                refinput1H_0 = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                refinput1H_1 = ThisInputType == HistoDivergence_InputType.High_Low ? Low[1] : swingInput[1];
                invalidate1H = BBMACD[0] > MIN(BBMACD, CurrentBar - firstOscTroughBar1H)[1] || refinput1H_0 >= refinput1H_1 ||
                    (IncludeDoubleTopsAndBottoms && refinput1H_0 < firstTroughLow1H) || (!IncludeDoubleTopsAndBottoms && refinput1H_0 <= firstTroughLow1H);

                if (hiddenbullishTriggerCountMACDBB[1] > 0 && invalidate1H)
                {
                    hiddenbullishPDivMACD[0] = (0.0);
                    secondOscTroughBar1H = priorSecondOscTroughBar1H;
                    secondTroughValue1H = priorSecondTroughValue1H;
                    updateBullishDivCandidateOsc1H = true;
                }
                else if (hiddenbullishPDivMACD[0] > 0.5 && invalidate1H)
                {
                    hiddenbullishPDivMACD[0] = (0.0);
                    hiddenbullishTriggerCountMACDBB[0] = (0);
                    secondOscTroughBar1H = priorSecondOscTroughBar1H;
                    secondTroughValue1H = priorSecondTroughValue1H;
                    RemoveDrawObject("hiddendivBullCandidateOsc1");
                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                }

                #region -- get price low and osc low --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMin = MIN(Low, SwingStrength)[1];
                    else
                        swingMin = MIN(Input, SwingStrength)[1];
                    if (swingLowType[i] < 0 || (i > DivMinBars && pre_swingLowType[i] < 0 && upTrend[0] && High[0] > Math.Max(swingMax, currentLow + zigzagDeviation))) // code references zigzag
                    {
                        refTroughBar1H[troughCount1H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow1H[troughCount1H] = Low[i];
                        else
                            refTroughLow1H[troughCount1H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && BBMACD[j - 1] < BBMACD[j])
                                j = j - 1;
                            refOscTroughBar1H[troughCount1H] = CurrentBar - j;
                            refTroughValue1H[troughCount1H] = BBMACD[j];
                            troughCount1H++;
                        }
                        else
                        {
                            refOscTroughBar1H[troughCount1H] = CurrentBar - i;
                            refTroughValue1H[troughCount1H] = BBMACD[i];
                            troughCount1H++;
                        }
                    }
                }
                #endregion

                minBarOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? CurrentBar - 1 : CurrentBar;
                minValueOsc = UseOscHighLow && BBMACD[1] < BBMACD[0] ? BBMACD[1] : BBMACD[0];

                #region -- find smallest divergence setup --
                for (int count = 0; count < troughCount1H; count++)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow1H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow1H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar1H[count] - 2)[1] &&
                        refTroughValue1H[count] < 0 &&
                        refTroughValue1H[count] > minValueOsc &&
                        refTroughValue1H[count] > MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1H[count] - 6))[1] &&
                        (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1H[count])[0] < 0))
                    {
                        hiddenbullishPDivMACD[0] = (1.0);
                        hiddenbullishTriggerCountMACDBB[0] = (TriggerBars + 1);
                        firstTroughBar1H = refTroughBar1H[count];
                        firstTroughLow1H = refTroughLow1H[count];
                        firstOscTroughBar1H = refOscTroughBar1H[count];
                        firstTroughValue1H = refTroughValue1H[count];
                        secondTroughBar1H = CurrentBar;
                        secondTroughLow1H = refinput[0];
                        secondOscTroughBar1H = maxBarOsc;
                        secondTroughValue1H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidateOsc1");
                            drawBullishDivCandidateOsc1H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidatePrice1");
                            drawBullishDivCandidatePrice1H = true;
                        }
                        if (ShowSetupDots) drawBullSetup1H = true;
                        break;
                    }
                    else hiddenbullishPDivMACD[0] = (0.0);
                }
                #endregion

                #region -- find largest divergence setup --
                for (int count = troughCount1H - 1; count >= 0; count--)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow1H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow1H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar1H[count] - 2)[1] &&
                        refTroughValue1H[count] < 0 &&
                        refTroughValue1H[count] > minValueOsc &&
                        refTroughValue1H[count] > MIN(BBMACD, Math.Max(1, CurrentBar - refOscTroughBar1H[count] - 6))[1] &&
                        (!ResetFilter || MAX(BBMACD, CurrentBar - refOscTroughBar1H[count])[0] < 0))
                    {
                        replacementTroughBar1H = refOscTroughBar1H[count];
                        replacementTroughLow1H = refTroughLow1H[count];
                        replacementOscTroughBar1H = refOscTroughBar1H[count];
                        replacementTroughValue1H = refTroughValue1H[count];
                        break;
                    }
                }
                #endregion

                inputref = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                if (hiddenbullishPDivMACD[0] < 0.5)
                {
                    divergenceActive = true;
                    if (hiddenbullishTriggerCountMACDBB[1] > 0)
                    {
                        hiddenbullishTriggerCountMACDBB[0] = (hiddenbullishTriggerCountMACDBB[1] - 1);
                        if (inputref < priorSecondTroughLow1H)
                        {
                            if (inputref > priorFirstTroughLow1H)//price stays above
                            {
                                secondTroughBar1H = CurrentBar;
                                secondTroughLow1H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidateOsc1");
                                    drawBullishDivCandidateOsc1H = true;
                                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                                    drawBullishDivCandidatePrice1H = true;
                                }
                            }
                            else if (inputref > priorReplacementTroughLow1H)
                            {
                                firstTroughBar1H = replacementTroughBar1H;
                                firstTroughLow1H = replacementTroughLow1H;
                                firstOscTroughBar1H = replacementOscTroughBar1H;
                                firstTroughValue1H = replacementTroughValue1H;
                                secondTroughBar1H = CurrentBar;
                                secondTroughLow1H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidateOsc1");
                                    drawBullishDivCandidateOsc1H = true;
                                    RemoveDrawObject("hiddendivBullCandidatePrice1");
                                    drawBullishDivCandidatePrice1H = true;
                                }
                            }
                            else
                            {
                                hiddenbullishTriggerCountMACDBB[0] = (0);
                                RemoveDrawObject("hiddendivBullCandidateOsc1");
                                RemoveDrawObject("hiddendivBullCandidatePrice1");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondTroughBar1H = priorSecondTroughBar1H;
                            secondTroughLow1H = priorSecondTroughLow1H;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBullCandidateOsc1");
                                updateBullishDivCandidateOsc1H = true;
                                RemoveDrawObject("hiddendivBullCandidatePrice1");
                                updateBullishDivCandidatePrice1H = true;
                            }
                        }
                        if (hiddenbullishTriggerCountMACDBB[0] > 0 && BBMACD[0] < MIN(BBMACD, CurrentBar - firstOscTroughBar1H)[1])
                        {
                            secondOscTroughBar1H = CurrentBar;
                            secondTroughValue1H = BBMACD[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBullCandidateOsc1");
                                if (divergenceActive) updateBullishDivCandidateOsc1H = true;
                            }
                        }
                    }
                }

                if (hiddenbullishTriggerCountMACDBB[0] > 0)
                {
                    if ((Close[0] > Low[CurrentBar - secondTroughBar1H]) && (BBMACD[0] > BBMACD[1]) && Close[0] > Open[0])
                    {
//                        if (firstTroughBar1H != memfirstTroughBar1H)
//                        {
//                            cptBullMACDhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBullMACDhdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstTroughBar1H = firstTroughBar1H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullMACDhdiv = maxcptBullMACDhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishMACDHiddenDivProjection[0] = (cptBullMACDhdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bullishMACDHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bullishMACDHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec

                #endregion

                #region Bearish divergences between price and histogram
                bool drawBearishDivOnOsc2 = false;
                bool drawBearishDivOnPrice2 = false;
                bool drawArrowDown2 = false;
                if (IsFirstTickOfBar)
                {
                    if (bearishTriggerCountHistogram[1] > 0)
                    {
                        if ((Close[1] < High[CurrentBar - secondPeakBar2]) && (Histogram[1] < Histogram[2]) && Close[1] < Open[1])
                        {
                            bearishCDivHistogram[0] = (1.0);
                            bearishTriggerCountHistogram[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                                drawBearishDivOnOsc2 = true;
                            if (ShowDivOnPricePanel)
                                drawBearishDivOnPrice2 = true;
                            if (showArrows)
                                drawArrowDown2 = true;
                            RemoveDrawObject("divBearCandidateOsc2");
                            RemoveDrawObject("divBearCandidatePrice2");
//                            maxcptBearHistogramdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
                        }
                        else
                            bearishCDivHistogram[0] = (0.0);
                        priorFirstPeakBar2 = firstPeakBar2;
                        priorFirstOscPeakBar2 = firstOscPeakBar2;
                        priorFirstPeakHigh2 = firstPeakHigh2;
                        priorFirstPeakValue2 = firstPeakValue2;
                        priorReplacementPeakValue2 = replacementPeakValue2;
                        priorSecondPeakBar2 = secondPeakBar2;
                        priorSecondOscPeakBar2 = secondOscPeakBar2;
                        priorSecondPeakHigh2 = secondPeakHigh2;
                        priorSecondPeakValue2 = secondPeakValue2;
                        if (Histogram[1] > firstPeakValue2)
                        {
                            bearishTriggerCountHistogram[1] = (0);
                            RemoveDrawObject("divBearCandidateOsc2");
                            RemoveDrawObject("divBearCandidatePrice2");
                        }
                    }
                    else
                    {
                        bearishCDivHistogram[0] = (0.0);
                        {
                            RemoveDrawObject("divBearCandidateOsc2");
                            RemoveDrawObject("divBearCandidatePrice2");
                        }
                    }
                    bearishPDivHistogram[0] = (0.0);
                }
                if (bearishPDivHistogram[0] > 0.5)
                {
                    RemoveDrawObject("divBearCandidateOsc2");
                    RemoveDrawObject("divBearCandidatePrice2");
                }
                bool drawBearishDivCandidateOsc2 = false;
                bool drawBearishDivCandidatePrice2 = false;
                bool updateBearishDivCandidateOsc2 = false;
                bool updateBearishDivCandidatePrice2 = false;
                bool drawBearSetup2 = false;
                invalidate = swingInput[0] <= swingInput[1] || (IncludeDoubleTopsAndBottoms && swingInput[0] < firstPeakHigh2) || (!IncludeDoubleTopsAndBottoms && swingInput[0] <= firstPeakHigh2);
                if (ThisInputType != HistoDivergence_InputType.High_Low && bearishTriggerCountHistogram[1] > 0 && invalidate)
                {
                    bearishPDivHistogram[0] = (0.0);
                    secondPeakBar2 = priorSecondPeakBar2;
                    secondPeakHigh2 = priorSecondPeakHigh2;
                    updateBearishDivCandidatePrice2 = true;
                }
                else if (ThisInputType != HistoDivergence_InputType.High_Low && bearishPDivHistogram[0] > 0.5 && invalidate)
                {
                    bearishPDivHistogram[0] = (0.0);
                    bearishTriggerCountHistogram[0] = (0);
                    secondPeakBar2 = priorSecondPeakBar2;
                    secondPeakHigh2 = priorSecondPeakHigh2;
                    RemoveDrawObject("divBearCandidateOsc2");
                    RemoveDrawObject("divBearCandidatePrice2");
                }
                bool firstPeakFound2 = false;
                peakCount2 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMax = MAX(High, SwingStrength)[1];
                    else
                        swingMax = MAX(Input, SwingStrength)[1];
                    if (swingHighType[i] > 0 || (i > DivMinBars && pre_swingHighType[i] > 0 && !upTrend[0] && Low[0] < Math.Min(swingMin, currentHigh - zigzagDeviation))) // code references zigzag
                    {
                        refPeakBar2[peakCount2] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh2[peakCount2] = High[i];
                        else
                            refPeakHigh2[peakCount2] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && Histogram[j - 1] > Histogram[j])
                                j = j - 1;
                            refOscPeakBar2[peakCount2] = CurrentBar - j;
                            refPeakValue2[peakCount2] = Histogram[j];
                            peakCount2 = peakCount2 + 1;
                        }
                        else
                        {
                            refOscPeakBar2[peakCount2] = CurrentBar - i;
                            refPeakValue2[peakCount2] = Histogram[i];
                            peakCount2 = peakCount2 + 1;
                        }
                    }
                }
                if (UseOscHighLow && Histogram[1] > Histogram[0])
                {
                    maxBarOsc = CurrentBar - 1;
                    maxValueOsc = Histogram[1];
                }
                else
                {
                    maxBarOsc = CurrentBar;
                    maxValueOsc = Histogram[0];
                }
                for (int count = 0; count < peakCount2; count++) //find smallest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh2[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        bearishPDivHistogram[0] = (1.0);
                        bearishTriggerCountHistogram[0] = (TriggerBars + 1);
                        firstPeakBar2 = refPeakBar2[count];
                        firstPeakHigh2 = refPeakHigh2[count];
                        firstOscPeakBar2 = refOscPeakBar2[count];
                        firstPeakValue2 = refPeakValue2[count];
                        secondPeakBar2 = CurrentBar;
                        secondPeakHigh2 = High[0];
                        secondOscPeakBar2 = maxBarOsc;
                        secondPeakValue2 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc2");
                            drawBearishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice2");
                            drawBearishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup2 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh2[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        bearishPDivHistogram[0] = (1.0);
                        bearishTriggerCountHistogram[0] = (TriggerBars + 1);
                        firstPeakBar2 = refPeakBar2[count];
                        firstPeakHigh2 = refPeakHigh2[count];
                        firstOscPeakBar2 = refOscPeakBar2[count];
                        firstPeakValue2 = refPeakValue2[count];
                        secondPeakBar2 = CurrentBar;
                        secondPeakHigh2 = swingInput[0];
                        secondOscPeakBar2 = maxBarOsc;
                        secondPeakValue2 = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBearCandidateOsc2");
                            drawBearishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBearCandidatePrice2");
                            drawBearishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBearSetup2 = true;
                        break;
                    }
                    else
                        bearishPDivHistogram[0] = (0.0);
                }
                for (int count = peakCount2 - 1; count >= 0; count--) //find largest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && High[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && High[0] > refPeakHigh2[count]))
                        && High[0] > High[1] && High[0] >= MAX(High, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        replacementPeakBar2 = refPeakBar2[count];
                        replacementPeakHigh2 = refPeakHigh2[count];
                        replacementOscPeakBar2 = refOscPeakBar2[count];
                        replacementPeakValue2 = refPeakValue2[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] >= refPeakHigh2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] > refPeakHigh2[count]))
                        && swingInput[0] > swingInput[1] && swingInput[0] >= MAX(swingInput, CurrentBar - refPeakBar2[count] - 2)[1] && refPeakValue2[count] > 0 && refPeakValue2[count] > maxValueOsc
                        && refPeakValue2[count] > MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2[count] - 6))[1] && (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2[count])[0] > 0))
                    {
                        replacementPeakBar2 = refPeakBar2[count];
                        replacementPeakHigh2 = refPeakHigh2[count];
                        replacementOscPeakBar2 = refOscPeakBar2[count];
                        replacementPeakValue2 = refPeakValue2[count];
                        break;
                    }
                }
                if (bearishPDivHistogram[0] < 0.5)
                {
                    divergenceActive = true;
                    if (bearishTriggerCountHistogram[1] > 0)
                    {
                        bearishTriggerCountHistogram[0] = (bearishTriggerCountHistogram[1] - 1);
                        if (Histogram[0] > priorSecondPeakValue2)
                        {
                            if (Histogram[0] < priorFirstPeakValue2)
                            {
                                secondOscPeakBar2 = CurrentBar;
                                secondPeakValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc2");
                                    updateBearishDivCandidateOsc2 = true;
                                    RemoveDrawObject("divBearCandidatePrice2");
                                    updateBearishDivCandidatePrice2 = true;
                                }
                            }
                            else if (Histogram[0] < priorReplacementPeakValue2)
                            {
                                firstPeakBar2 = replacementPeakBar2;
                                firstPeakHigh2 = replacementPeakHigh2;
                                firstOscPeakBar2 = replacementOscPeakBar2;
                                firstPeakValue2 = replacementPeakValue2;
                                secondOscPeakBar2 = CurrentBar;
                                secondPeakValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBearCandidateOsc2");
                                    drawBearishDivCandidateOsc2 = true;
                                    RemoveDrawObject("divBearCandidatePrice2");
                                    drawBearishDivCandidatePrice2 = true;
                                }
                            }
                            else
                            {
                                bearishTriggerCountHistogram[0] = (0);
                                RemoveDrawObject("divBearCandidateOsc2");
                                RemoveDrawObject("divBearCandidatePrice2");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondOscPeakBar2 = priorSecondOscPeakBar2;
                            secondPeakValue2 = priorSecondPeakValue2;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("divBearCandidateOsc2");
                                updateBearishDivCandidateOsc2 = true;
                                RemoveDrawObject("divBearCandidatePrice2");
                                updateBearishDivCandidatePrice2 = true;
                            }
                        }
                        if (divergenceActive && ThisInputType == HistoDivergence_InputType.High_Low && bearishTriggerCountHistogram[0] > 0 && High[0] > MAX(High, CurrentBar - firstPeakBar2)[1])
                        {
                            secondPeakBar2 = CurrentBar;
                            secondPeakHigh2 = High[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice2");
                                if (divergenceActive) updateBearishDivCandidatePrice2 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bearishTriggerCountHistogram[0] > 0 && swingInput[0] > MAX(swingInput, CurrentBar - firstPeakBar2)[1])
                        {
                            secondPeakBar2 = CurrentBar;
                            secondPeakHigh2 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBearCandidatePrice2");
                                if (divergenceActive)   updateBearishDivCandidatePrice2 = true;
                            }
                        }
                    }
                }

                if (bearishTriggerCountHistogram[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar2]) && (Histogram[0] < Histogram[1]) && Close[0] < Open[0])
                    {
//                        if (firstPeakBar2 != memfirstPeakBar2)
//                        {
//                            cptBearHistogramdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBearHistogramdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstPeakBar2 = firstPeakBar2;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearHistogramdiv = maxcptBearHistogramdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishHistogramDivProjection[0] = (cptBearHistogramdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bearishHistogramDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bearishHistogramDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec
                #endregion

                #region Bearish Hidden divergences between price and Histogram #HIDDENDIV
                drawBearishDivOnOsc2H = false;
                drawBearishDivOnPrice2H = false;
                drawArrowDown2H = false;
                drawBearSetup2H = false;

                #region -- IsFirstTickOfBar - {Reset} --
                if (IsFirstTickOfBar)
                {
                    if (hiddenbearishTriggerCountHistogram[1] > 0)
                    {
                        if ((Close[1] < High[CurrentBar - secondPeakBar2H]) && (Histogram[1] < Histogram[2]) && Close[1] < Open[1])
                        {
                            hiddenbearishCDivHistogram[0] = (1.0);
                            hiddenbearishTriggerCountHistogram[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel) drawBearishDivOnOsc2H = true;
                            if (ShowDivOnPricePanel) drawBearishDivOnPrice2H = true;
                            if (showArrows) drawArrowDown2H = true;
                            RemoveDrawObject("hiddendivBearCandidateOsc2");
                            RemoveDrawObject("hiddendivBearCandidatePrice2");
//                            maxcptBearHistogramhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
                        }
                        else
                            hiddenbearishCDivHistogram[0] = (0.0);

                        priorFirstPeakBar2H = firstPeakBar2H;
                        priorFirstOscPeakBar2H = firstOscPeakBar2H;
                        priorFirstPeakHigh2H = firstPeakHigh2H;
                        priorFirstPeakValue2H = firstPeakValue2H;
                        priorReplacementPeakHigh2H = replacementPeakHigh2H;
                        priorReplacementPeakValue2H = replacementPeakValue2H;
                        priorSecondPeakBar2H = secondPeakBar2H;
                        priorSecondOscPeakBar2H = secondOscPeakBar2H;
                        priorSecondPeakValue2H = secondPeakValue2H;
                        priorSecondPeakHigh2H = secondPeakHigh2H;

                        double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High[1] : swingInput[1];
                        if (refinput > firstPeakHigh2H)
                        {
                            hiddenbearishTriggerCountHistogram[1] = (0);
                            RemoveDrawObject("hiddendivBearCandidateOsc2");
                            RemoveDrawObject("hiddendivBearCandidatePrice2");
                        }
                    }
                    else
                    {
                        hiddenbearishCDivHistogram[0] = (0.0);
                        RemoveDrawObject("hiddendivBearCandidateOsc2");
                        RemoveDrawObject("hiddendivBearCandidatePrice2");
                    }
                    hiddenbearishPDivHistogram[0] = (0.0);
                }
                #endregion

                if (hiddenbearishPDivHistogram[0] > 0.5)
                {
                    RemoveDrawObject("hiddendivBearCandidateOsc2");
                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                }

                #region -- reset variables --
                drawBearishDivCandidateOsc2H = false;
                drawBearishDivCandidatePrice2H = false;
                updateBearishDivCandidateOsc2H = false;
                updateBearishDivCandidatePrice2H = false;
                peakCount2H = 0;
                #endregion

                double refinput2H_0 = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                double refinput2H_1 = ThisInputType == HistoDivergence_InputType.High_Low ? High[1] : swingInput[1];
                bool invalidate2H = Histogram[0] < MAX(Histogram, CurrentBar - firstOscPeakBar2H)[1] || refinput2H_0 <= refinput2H_1 ||
                    (IncludeDoubleTopsAndBottoms && refinput2H_0 > firstPeakHigh2H) || (!IncludeDoubleTopsAndBottoms && refinput2H_0 >= firstPeakHigh2H);

                if (hiddenbearishTriggerCountHistogram[1] > 0 && invalidate2H)
                {
                    hiddenbearishPDivHistogram[0] = (0.0);
                    secondOscPeakBar2H = priorSecondOscPeakBar2H;
                    secondPeakValue2H = priorSecondPeakValue2H;
                    updateBearishDivCandidateOsc2H = true;
                }
                else if (hiddenbearishPDivHistogram[0] > 0.5 && invalidate2H)
                {
                    hiddenbearishPDivHistogram[0] = (0.0);
                    hiddenbearishTriggerCountHistogram[0] = (0);
                    secondOscPeakBar2H = priorSecondOscPeakBar2H;
                    secondPeakValue2H = priorSecondPeakValue2H;
                    RemoveDrawObject("hiddendivBearCandidateOsc2");
                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                }

                #region -- get price top and osc top --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMax = MAX(High, SwingStrength)[1];
                    else
                        swingMax = MAX(Input, SwingStrength)[1];
                    if (swingHighType[i] > 0 || (i > DivMinBars && pre_swingHighType[i] > 0 && !upTrend[0] && Low[0] < Math.Min(swingMin, currentHigh - zigzagDeviation))) // code references zigzag
                    {
                        refPeakBar2H[peakCount2H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refPeakHigh2H[peakCount2H] = High[i];
                        else
                            refPeakHigh2H[peakCount2H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 4) && Histogram[j - 1] > Histogram[j])
                                j = j - 1;
                            refOscPeakBar2H[peakCount2H] = CurrentBar - j;
                            refPeakValue2H[peakCount2H] = Histogram[j];
                            peakCount2H = peakCount2H + 1;
                        }
                        else
                        {
                            refOscPeakBar2H[peakCount2H] = CurrentBar - i;
                            refPeakValue2H[peakCount2H] = Histogram[i];
                            peakCount2H = peakCount2H + 1;
                        }
                    }
                }
                #endregion

                maxBarOsc = UseOscHighLow && Histogram[1] > Histogram[0] ? CurrentBar - 1 : CurrentBar;
                maxValueOsc = UseOscHighLow && Histogram[1] > Histogram[0] ? Histogram[1] : Histogram[0];

                #region -- find smallest divergence setup --
                for (int count = 0; count < peakCount2H; count++)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh2H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh2H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar2H[count] - 2)[1] &&
                        refPeakValue2H[count] > 0 &&
                        refPeakValue2H[count] < maxValueOsc &&
                        refPeakValue2H[count] < MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2H[count] - 6))[1] &&
                        (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2H[count])[0] > 0))
                    {
                        hiddenbearishPDivHistogram[0] = (1.0);
                        hiddenbearishTriggerCountHistogram[0] = (TriggerBars + 1);
                        firstPeakBar2H = refPeakBar2H[count];
                        firstPeakHigh2H = refPeakHigh2H[count];
                        firstOscPeakBar2H = refOscPeakBar2H[count];
                        firstPeakValue2H = refPeakValue2H[count];
                        secondPeakBar2H = CurrentBar;
                        secondPeakHigh2H = refinput[0];
                        secondOscPeakBar2H = maxBarOsc;
                        secondPeakValue2H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidateOsc2");
                            drawBearishDivCandidateOsc2H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBearCandidatePrice2");
                            drawBearishDivCandidatePrice2H = true;
                        }
                        if (ShowSetupDots) drawBearSetup2H = true;
                        break;
                    }
                    else hiddenbearishPDivHistogram[0] = (0.0);
                }
                #endregion

                #region -- find largest divergence setup --
                for (int count = peakCount2H - 1; count >= 0; count--)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? High : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] <= refPeakHigh2H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] < refPeakHigh2H[count])) &&
                        refinput[0] > refinput[1] &&
                        refinput[0] >= MAX(refinput, CurrentBar - refPeakBar2H[count] - 2)[1] &&
                        refPeakValue2H[count] > 0 &&
                        refPeakValue2H[count] < maxValueOsc &&
                        refPeakValue2H[count] < MAX(Histogram, Math.Max(1, CurrentBar - refOscPeakBar2H[count] - 6))[1] &&
                        (!ResetFilter || MIN(Histogram, CurrentBar - refOscPeakBar2H[count])[0] > 0))
                    {
                        replacementPeakBar2H = refPeakBar2H[count];
                        replacementPeakHigh2H = refPeakHigh2H[count];
                        replacementOscPeakBar2H = refOscPeakBar2H[count];
                        replacementPeakValue2H = refPeakValue2H[count];
                        break;
                    }
                }
                #endregion

                inputref = ThisInputType == HistoDivergence_InputType.High_Low ? High[0] : swingInput[0];
                if (hiddenbearishPDivHistogram[0] < 0.5)
                {
                    divergenceActive = true;
                    if (hiddenbearishTriggerCountHistogram[1] > 0)
                    {
                        hiddenbearishTriggerCountHistogram[0] = (hiddenbearishTriggerCountHistogram[1] - 1);
                        if (inputref > priorSecondPeakHigh2H)
                        {
                            if (inputref < priorFirstPeakHigh2H)//price stays below
                            {
                                secondPeakBar2H = CurrentBar;
                                secondPeakHigh2H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidateOsc2");
                                    updateBearishDivCandidateOsc2H = true;
                                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                                    updateBearishDivCandidatePrice2H = true;
                                }
                            }
                            else if (inputref < priorReplacementPeakHigh2H)
                            {
                                firstPeakBar2H = replacementPeakBar2H;
                                firstPeakHigh2H = replacementPeakHigh2H;
                                firstOscPeakBar2H = replacementOscPeakBar2H;
                                firstPeakValue2H = replacementPeakValue2H;
                                secondPeakBar2H = CurrentBar;
                                secondPeakHigh2H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBearCandidateOsc2");
                                    drawBearishDivCandidateOsc2H = true;
                                    RemoveDrawObject("hiddendivBearCandidatePrice2");
                                    drawBearishDivCandidatePrice2H = true;
                                }
                            }
                            else
                            {
                                hiddenbearishTriggerCountHistogram[0] = (0);
                                RemoveDrawObject("hiddendivBearCandidateOsc2");
                                RemoveDrawObject("hiddendivBearCandidatePrice2");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondPeakBar2H = priorSecondPeakBar2H;
                            secondPeakHigh2H = priorSecondPeakHigh2H;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBearCandidateOsc2");
                                updateBearishDivCandidateOsc2H = true;
                                RemoveDrawObject("hiddendivBearCandidatePrice2");
                                updateBearishDivCandidatePrice2H = true;
                            }
                        }
                        if (hiddenbearishTriggerCountHistogram[0] > 0 && Histogram[0] > MAX(Histogram, CurrentBar - firstOscPeakBar2H)[1])
                        {
                            secondOscPeakBar2H = CurrentBar;
                            secondPeakValue2H = Histogram[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBearCandidateOsc2");
                                if (divergenceActive) updateBearishDivCandidateOsc2H = true;
                            }
                        }
                    }
                }

                if (hiddenbearishTriggerCountHistogram[0] > 0)
                {
                    if ((Close[0] < High[CurrentBar - secondPeakBar2H]) && (Histogram[0] < Histogram[1]) && Close[0] < Open[0])
                    {
//                        if (firstPeakBar2H != memfirstPeakBar2H)
//                        {
//                            cptBearHistogramhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBearHistogramhdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstPeakBar2H = firstPeakBar2H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBearHistogramhdiv = maxcptBearHistogramhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bearishHistogramHiddenDivProjection[0] = (cptBearHistogramhdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bearishHistogramHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bearishHistogramHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec

                #endregion

                #region Bullish divergences between price and histogram
                bool drawBullishDivOnOsc2 = false;
                bool drawBullishDivOnPrice2 = false;
                bool drawArrowUp2 = false;
                if (IsFirstTickOfBar)
                {
                    if (bullishTriggerCountHistogram[1] > 0)
                    {
                        if ((Close[1] > Low[CurrentBar - secondTroughBar2]) && (Histogram[1] > Histogram[2]) && Close[1] > Open[1])
                        {
                            bullishCDivHistogram[0] = (1.0);
                            bullishTriggerCountHistogram[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                                drawBullishDivOnOsc2 = true;
                            if (ShowDivOnPricePanel)
                                drawBullishDivOnPrice2 = true;
                            if (showArrows)
                                drawArrowUp2 = true;
                            RemoveDrawObject("divBullCandidateOsc2");
                            RemoveDrawObject("divBullCandidatePrice2");
//                            maxcptBullHistogramdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
                        }
                        else
                            bullishCDivHistogram[0] = (0.0);
                        priorFirstTroughBar2 = firstTroughBar2;
                        priorFirstOscTroughBar2 = firstOscTroughBar2;
                        priorFirstTroughLow2 = firstTroughLow2;
                        priorFirstTroughValue2 = firstTroughValue2;
                        priorReplacementTroughValue2 = replacementTroughValue2;
                        priorSecondTroughBar2 = secondTroughBar2;
                        priorSecondOscTroughBar2 = secondOscTroughBar2;
                        priorSecondTroughLow2 = secondTroughLow2;
                        priorSecondTroughValue2 = secondTroughValue2;
                        if (Histogram[1] < firstTroughValue2)
                        {
                            bullishTriggerCountHistogram[1] = (0);
                            RemoveDrawObject("divBullCandidateOsc2");
                            RemoveDrawObject("divBullCandidatePrice2");
                        }
                    }
                    else
                    {
                        bullishCDivHistogram[0] = (0.0);
                        {
                            RemoveDrawObject("divBullCandidateOsc2");
                            RemoveDrawObject("divBullCandidatePrice2");
                        }
                    }
                    bullishPDivHistogram[0] = (0.0);
                }
                if (bullishPDivHistogram[0] > 0.5)
                {
                    RemoveDrawObject("divBullCandidateOsc2");
                    RemoveDrawObject("divBullCandidatePrice2");
                }
                bool drawBullishDivCandidateOsc2 = false;
                bool drawBullishDivCandidatePrice2 = false;
                bool updateBullishDivCandidateOsc2 = false;
                bool updateBullishDivCandidatePrice2 = false;
                bool drawBullSetup2 = false;
                invalidate = swingInput[0] >= swingInput[1] || (IncludeDoubleTopsAndBottoms && swingInput[0] > firstTroughLow2) || (!IncludeDoubleTopsAndBottoms && swingInput[0] >= firstTroughLow2);
                if (ThisInputType != HistoDivergence_InputType.High_Low && bullishTriggerCountHistogram[1] > 0 && invalidate)
                {
                    bullishPDivHistogram[0] = (0.0);
                    secondTroughBar2 = priorSecondTroughBar2;
                    secondTroughLow2 = priorSecondTroughLow2;
                    updateBullishDivCandidatePrice2 = true;
                }
                else if (ThisInputType != HistoDivergence_InputType.High_Low && bullishPDivHistogram[0] > 0.5 && invalidate)
                {
                    bullishPDivHistogram[0] = (0.0);
                    bullishTriggerCountHistogram[0] = (0);
                    secondTroughBar2 = priorSecondTroughBar2;
                    secondTroughLow2 = priorSecondTroughLow2;
                    RemoveDrawObject("divBullCandidateOsc2");
                    RemoveDrawObject("divBullCandidatePrice2");
                }
                bool firstTroughFound2 = false;
                troughCount2 = 0;
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMin = MIN(Low, SwingStrength)[1];
                    else
                        swingMin = MIN(Input, SwingStrength)[1];
                    if (swingLowType[i] < 0 || (i > DivMinBars && pre_swingLowType[i] < 0 && upTrend[0] && High[0] > Math.Max(swingMax, currentLow + zigzagDeviation))) // code references zigzag 
                    {
                        refTroughBar2[troughCount2] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow2[troughCount2] = Low[i];
                        else
                            refTroughLow2[troughCount2] = swingInput[i];
                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && Histogram[j - 1] < Histogram[j])
                                j = j - 1;
                            refOscTroughBar2[troughCount2] = CurrentBar - j;
                            refTroughValue2[troughCount2] = Histogram[j];
                            troughCount2 = troughCount2 + 1;
                        }
                        else
                        {
                            refOscTroughBar2[troughCount2] = CurrentBar - i;
                            refTroughValue2[troughCount2] = Histogram[i];
                            troughCount2 = troughCount2 + 1;
                        }
                    }
                }
                if (UseOscHighLow && Histogram[1] < Histogram[0])
                {
                    minBarOsc = CurrentBar - 1;
                    minValueOsc = Histogram[1];
                }
                else
                {
                    minBarOsc = CurrentBar;
                    minValueOsc = Histogram[0];
                }
                for (int count = 0; count < troughCount2; count++) //find smallest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow2[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        bullishPDivHistogram[0] = (1.0);
                        bullishTriggerCountHistogram[0] = (TriggerBars + 1);
                        firstTroughBar2 = refTroughBar2[count];
                        firstTroughLow2 = refTroughLow2[count];
                        firstOscTroughBar2 = refOscTroughBar2[count];
                        firstTroughValue2 = refTroughValue2[count];
                        secondTroughBar2 = CurrentBar;
                        secondTroughLow2 = Low[0];
                        secondOscTroughBar2 = minBarOsc;
                        secondTroughValue2 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc2");
                            drawBullishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice2");
                            drawBullishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup2 = true;
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow2[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        bullishPDivHistogram[0] = (1.0);
                        bullishTriggerCountHistogram[0] = (TriggerBars + 1);
                        firstTroughBar2 = refTroughBar2[count];
                        firstTroughLow2 = refTroughLow2[count];
                        firstOscTroughBar2 = refOscTroughBar2[count];
                        firstTroughValue2 = refTroughValue2[count];
                        secondTroughBar2 = CurrentBar;
                        secondTroughLow2 = swingInput[0];
                        secondOscTroughBar2 = minBarOsc;
                        secondTroughValue2 = minValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("divBullCandidateOsc2");
                            drawBullishDivCandidateOsc2 = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("divBullCandidatePrice2");
                            drawBullishDivCandidatePrice2 = true;
                        }
                        if (ShowSetupDots)
                            drawBullSetup2 = true;
                        break;
                    }
                    bullishPDivHistogram[0] = (0.0);
                }
                for (int count = troughCount2 - 1; count >= 0; count--) //find largest divergence setup
                {
                    if (ThisInputType == HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && Low[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && Low[0] < refTroughLow2[count]))
                        && Low[0] < Low[1] && Low[0] <= MIN(Low, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        replacementTroughBar2 = refTroughBar2[count];
                        replacementTroughLow2 = refTroughLow2[count];
                        replacementOscTroughBar2 = refOscTroughBar2[count];
                        replacementTroughValue2 = refTroughValue2[count];
                        break;
                    }
                    else if (ThisInputType != HistoDivergence_InputType.High_Low && ((IncludeDoubleTopsAndBottoms && swingInput[0] <= refTroughLow2[count]) || (!IncludeDoubleTopsAndBottoms && swingInput[0] < refTroughLow2[count]))
                        && swingInput[0] < swingInput[1] && swingInput[0] <= MIN(swingInput, CurrentBar - refTroughBar2[count] - 2)[1] && refTroughValue2[count] < 0 && refTroughValue2[count] < minValueOsc
                        && refTroughValue2[count] < MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2[count] - 6))[1] && (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2[count])[0] < 0))
                    {
                        replacementTroughBar2 = refTroughBar2[count];
                        replacementTroughLow2 = refTroughLow2[count];
                        replacementOscTroughBar2 = refOscTroughBar2[count];
                        replacementTroughValue2 = refTroughValue2[count];
                        break;
                    }
                }
                if (bullishPDivHistogram[0] < 0.5)
                {
                    divergenceActive = true;
                    if (bullishTriggerCountHistogram[1] > 0)
                    {
                        bullishTriggerCountHistogram[0] = (bullishTriggerCountHistogram[1] - 1);
                        if (Histogram[0] < priorSecondTroughValue2)
                        {
                            if (Histogram[0] > priorFirstTroughValue2)
                            {
                                secondOscTroughBar2 = CurrentBar;
                                secondTroughValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc2");
                                    updateBullishDivCandidateOsc2 = true;
                                    RemoveDrawObject("divBullCandidatePrice2");
                                    updateBullishDivCandidatePrice2 = true;
                                }
                            }
                            else if (Histogram[0] > priorReplacementTroughValue2)
                            {
                                firstTroughBar2 = replacementTroughBar2;
                                firstTroughLow2 = replacementTroughLow2;
                                firstOscTroughBar2 = replacementOscTroughBar2;
                                firstTroughValue2 = replacementTroughValue2;
                                secondOscTroughBar2 = CurrentBar;
                                secondTroughValue2 = Histogram[0];
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("divBullCandidateOsc2");
                                    drawBullishDivCandidateOsc2 = true;
                                    RemoveDrawObject("divBullCandidatePrice2");
                                    drawBullishDivCandidatePrice2 = true;
                                }
                            }
                            else
                            {
                                bullishTriggerCountHistogram[0] = (0);
                                RemoveDrawObject("divBullCandidateOsc2");
                                RemoveDrawObject("divBullCandidatePrice2");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondOscTroughBar2 = priorSecondOscTroughBar2;
                            secondTroughValue2 = priorSecondTroughValue2;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("divBullCandidateOsc2");
                                updateBullishDivCandidateOsc2 = true;
                                RemoveDrawObject("divBullCandidatePrice2");
                                updateBullishDivCandidatePrice2 = true;
                            }
                        }
                        if (ThisInputType == HistoDivergence_InputType.High_Low && bullishTriggerCountHistogram[0] > 0 && Low[0] < MIN(Low, CurrentBar - firstTroughBar2)[1])
                        {
                            secondTroughBar2 = CurrentBar;
                            secondTroughLow2 = Low[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice2");
                                if (divergenceActive)
                                    updateBullishDivCandidatePrice2 = true;
                            }
                        }
                        else if (ThisInputType != HistoDivergence_InputType.High_Low && bullishTriggerCountHistogram[0] > 0 && swingInput[0] < MIN(swingInput, CurrentBar - firstTroughBar2)[1])
                        {
                            secondTroughBar2 = CurrentBar;
                            secondTroughLow2 = swingInput[0];
                            if (ShowDivOnPricePanel)
                            {
                                RemoveDrawObject("divBullCandidatePrice2");
                                if (divergenceActive)
                                    updateBullishDivCandidatePrice2 = true;
                            }
                        }
                    }
                }

                if (bullishTriggerCountHistogram[0] > 0)
                {
                    if ((Close[0] > Low[CurrentBar - secondTroughBar2]) && (Histogram[0] > Histogram[1]) && Close[0] > Open[0])
                    {
//                        if (firstTroughBar2 != memfirstTroughBar2)
//                        {
//                            cptBullHistogramdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBullHistogramdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstTroughBar2 = firstTroughBar2;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullHistogramdiv = maxcptBullHistogramdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishHistogramDivProjection[0] = (cptBullHistogramdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bullishHistogramDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bullishHistogramDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec

                #endregion

                #region Bullish Hidden divergences between price and Histogram #HIDDENDIV
                drawBullishDivOnOsc2H = false;
                drawBullishDivOnPrice2H = false;
                drawArrowUp2H = false;
                drawBullSetup2H = false;

                #region -- IsFirstTickOfBar - {Reset} --
                if (IsFirstTickOfBar)
                {
                    if (hiddenbullishTriggerCountHistogram[1] > 0)
                    {
                        if ((Close[1] > Low[CurrentBar - secondTroughBar2H]) && (Histogram[1] > Histogram[2]) && Close[1] > Open[1])
                        {
                            hiddenbullishCDivHistogram[0] = (1.0);
                            hiddenbullishTriggerCountHistogram[1] = (0);
                            if (!hidePlots && ShowDivOnOscillatorPanel) drawBullishDivOnOsc2H = true;
                            if (ShowDivOnPricePanel) drawBullishDivOnPrice2H = true;
                            if (showArrows) drawArrowUp2H = true;
                            RemoveDrawObject("hiddendivBullCandidateOsc2");
                            RemoveDrawObject("hiddendivBullCandidatePrice2");
//                            maxcptBullHistogramhdiv++;//#BLOOHOUND - added 17.02.03 - AzurITec
                        }
                        else hiddenbullishCDivHistogram[0] = (0.0);

                        priorFirstTroughBar2H = firstTroughBar2H;
                        priorFirstOscTroughBar2H = firstOscTroughBar2H;
                        priorFirstTroughLow2H = firstTroughLow2H;
                        priorFirstTroughValue2H = firstTroughValue2H;
                        priorReplacementTroughLow2H = replacementTroughLow2H;
                        priorReplacementTroughValue2H = replacementTroughValue2H;
                        priorSecondTroughBar2H = secondTroughBar2H;
                        priorSecondOscTroughBar2H = secondOscTroughBar2H;
                        priorSecondTroughValue2H = secondTroughValue2H;
                        priorSecondTroughLow2H = secondTroughLow2H;

                        double refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low[1] : swingInput[1];
                        if (refinput < firstTroughLow2H)
                        {
                            hiddenbullishTriggerCountHistogram[1] = (0);
                            RemoveDrawObject("hiddendivBullCandidateOsc2");
                            RemoveDrawObject("hiddendivBullCandidatePrice2");
                        }
                    }
                    else
                    {
                        hiddenbullishCDivHistogram[0] = (0.0);
                        RemoveDrawObject("hiddendivBullCandidateOsc2");
                        RemoveDrawObject("hiddendivBullCandidatePrice2");
                    }
                    hiddenbullishPDivHistogram[0] = (0.0);
                }
                #endregion

                if (hiddenbullishPDivHistogram[0] > 0.5)
                {
                    RemoveDrawObject("hiddendivBullCandidateOsc2");
                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                }

                #region -- reset variables --
                drawBullishDivCandidateOsc2H = false;
                drawBullishDivCandidatePrice2H = false;
                updateBullishDivCandidateOsc2H = false;
                updateBullishDivCandidatePrice2H = false;
                troughCount2H = 0;
                #endregion

                refinput2H_0 = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                refinput2H_1 = ThisInputType == HistoDivergence_InputType.High_Low ? Low[1] : swingInput[1];
                invalidate2H = Histogram[0] > MIN(Histogram, CurrentBar - firstOscTroughBar2H)[1] || refinput2H_0 >= refinput2H_1 ||
                    (IncludeDoubleTopsAndBottoms && refinput2H_0 < firstTroughLow2H) || (!IncludeDoubleTopsAndBottoms && refinput2H_0 <= firstTroughLow2H);
                if (hiddenbullishTriggerCountHistogram[1] > 0 && invalidate2H)
                {
                    hiddenbullishPDivHistogram[0] = (0.0);
                    secondOscTroughBar2H = priorSecondOscTroughBar2H;
                    secondTroughValue2H = priorSecondTroughValue2H;
                    updateBullishDivCandidateOsc2H = true;
                }
                else if (hiddenbullishPDivHistogram[0] > 0.5 && invalidate2H)
                {
                    hiddenbullishPDivHistogram[0] = (0.0);
                    hiddenbullishTriggerCountHistogram[0] = (0);
                    secondOscTroughBar2H = priorSecondOscTroughBar2H;
                    secondTroughValue2H = priorSecondTroughValue2H;
                    RemoveDrawObject("hiddendivBullCandidateOsc2");
                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                }

                #region -- get price low and osc low --
                for (int i = DivMinBars; i <= DivMaxBars; i++)
                {
                    int j = 0;
                    if (ThisInputType == HistoDivergence_InputType.High_Low)
                        swingMin = MIN(Low, SwingStrength)[1];
                    else
                        swingMin = MIN(Input, SwingStrength)[1];
                    if (swingLowType[i] < 0 || (i > DivMinBars && pre_swingLowType[i] < 0 && upTrend[0] && High[0] > Math.Max(swingMax, currentLow + zigzagDeviation))) // code references zigzag
                    {
                        refTroughBar2H[troughCount2H] = CurrentBar - i;
                        if (ThisInputType == HistoDivergence_InputType.High_Low)
                            refTroughLow2H[troughCount2H] = Low[i];
                        else
                            refTroughLow2H[troughCount2H] = swingInput[i];

                        if (UseOscHighLow)
                        {
                            j = i;
                            while (j > i - Math.Min(DivMinBars, 3) && Histogram[j - 1] < Histogram[j])
                                j = j - 1;
                            refOscTroughBar2H[troughCount2H] = CurrentBar - j;
                            refTroughValue2H[troughCount2H] = Histogram[j];
                            troughCount2H++;
                        }
                        else
                        {
                            refOscTroughBar2H[troughCount2H] = CurrentBar - i;
                            refTroughValue2H[troughCount2H] = Histogram[i];
                            troughCount2H++;
                        }
                    }
                }
                #endregion

                minBarOsc = UseOscHighLow && Histogram[1] < Histogram[0] ? CurrentBar - 1 : CurrentBar;
                minValueOsc = UseOscHighLow && Histogram[1] < Histogram[0] ? Histogram[1] : Histogram[0];

                #region -- find smallest divergence setup --
                for (int count = 0; count < troughCount2H; count++)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow2H[count]) ||
                        (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow2H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar2H[count] - 2)[1] &&
                        refTroughValue2H[count] < 0 &&
                        refTroughValue2H[count] > minValueOsc &&
                        refTroughValue2H[count] > MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2H[count] - 6))[1] &&
                        (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2H[count])[0] < 0))
                    {
                        hiddenbullishPDivHistogram[0] = (1.0);
                        hiddenbullishTriggerCountHistogram[0] = (TriggerBars + 1);
                        firstTroughBar2H = refTroughBar2H[count];
                        firstTroughLow2H = refTroughLow2H[count];
                        firstOscTroughBar2H = refOscTroughBar2H[count];
                        firstTroughValue2H = refTroughValue2H[count];
                        secondTroughBar2H = CurrentBar;
                        secondTroughLow2H = refinput[0];
                        secondOscTroughBar2H = maxBarOsc;
                        secondTroughValue2H = maxValueOsc;
                        if (!hidePlots && ShowDivOnOscillatorPanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidateOsc2");
                            drawBullishDivCandidateOsc2H = true;
                        }
                        if (ShowDivOnPricePanel)
                        {
                            RemoveDrawObject("hiddendivBullCandidatePrice2");
                            drawBullishDivCandidatePrice2H = true;
                        }
                        if (ShowSetupDots) drawBullSetup2H = true;
                        break;
                    }
                    else hiddenbullishPDivHistogram[0] = (0.0);
                }
                #endregion

                #region -- find largest divergence setup --
                for (int count = troughCount2H - 1; count >= 0; count--)
                {
                    ISeries<double> refinput = ThisInputType == HistoDivergence_InputType.High_Low ? Low : swingInput;
                    if (((IncludeDoubleTopsAndBottoms && refinput[0] >= refTroughLow2H[count]) || (!IncludeDoubleTopsAndBottoms && refinput[0] > refTroughLow2H[count])) &&
                        refinput[0] < refinput[1] &&
                        refinput[0] <= MIN(refinput, CurrentBar - refTroughBar2H[count] - 2)[1] &&
                        refTroughValue2H[count] < 0 &&
                        refTroughValue2H[count] > minValueOsc &&
                        refTroughValue2H[count] > MIN(Histogram, Math.Max(1, CurrentBar - refOscTroughBar2H[count] - 6))[1] &&
                        (!ResetFilter || MAX(Histogram, CurrentBar - refOscTroughBar2H[count])[0] < 0))
                    {
                        replacementTroughBar2H = refOscTroughBar2H[count];
                        replacementTroughLow2H = refTroughLow2H[count];
                        replacementOscTroughBar2H = refOscTroughBar2H[count];
                        replacementTroughValue2H = refTroughValue2H[count];
                        break;
                    }
                }
                #endregion

                inputref = ThisInputType == HistoDivergence_InputType.High_Low ? Low[0] : swingInput[0];
                if (hiddenbullishPDivHistogram[0] < 0.5)
                {
                    divergenceActive = true;
                    if (hiddenbullishTriggerCountHistogram[1] > 0)
                    {
                        hiddenbullishTriggerCountHistogram[0] = (hiddenbullishTriggerCountHistogram[1] - 1);
                        if (inputref < priorSecondTroughLow2H)
                        {
                            if (inputref > priorFirstTroughLow2H)//price stays above
                            {
                                secondTroughBar2H = CurrentBar;
                                secondTroughLow2H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidateOsc2");
                                    drawBullishDivCandidateOsc2H = true;
                                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                                    drawBullishDivCandidatePrice2H = true;
                                }
                            }
                            else if (inputref > priorReplacementTroughLow2H)
                            {
                                firstTroughBar2H = replacementTroughBar2H;
                                firstTroughLow2H = replacementTroughLow2H;
                                firstOscTroughBar2H = replacementOscTroughBar2H;
                                firstTroughValue2H = replacementTroughValue2H;
                                secondTroughBar2H = CurrentBar;
                                secondTroughLow2H = inputref;
                                if (!hidePlots && ShowDivOnOscillatorPanel)
                                {
                                    RemoveDrawObject("hiddendivBullCandidateOsc2");
                                    drawBullishDivCandidateOsc2H = true;
                                    RemoveDrawObject("hiddendivBullCandidatePrice2");
                                    drawBullishDivCandidatePrice2H = true;
                                }
                            }
                            else
                            {
                                hiddenbullishTriggerCountHistogram[0] = (0);
                                RemoveDrawObject("hiddendivBullCandidateOsc2");
                                RemoveDrawObject("hiddendivBullCandidatePrice2");
                                divergenceActive = false;
                            }
                        }
                        else
                        {
                            secondTroughBar2H = priorSecondTroughBar2H;
                            secondTroughLow2H = priorSecondTroughLow2H;
                            if (!hidePlots && ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBullCandidateOsc2");
                                updateBullishDivCandidateOsc2H = true;
                                RemoveDrawObject("hiddendivBullCandidatePrice2");
                                updateBullishDivCandidatePrice2H = true;
                            }
                        }
                        if (hiddenbullishTriggerCountHistogram[0] > 0 && Histogram[0] < MIN(Histogram, CurrentBar - firstOscTroughBar2H)[1])
                        {
                            secondOscTroughBar2H = CurrentBar;
                            secondTroughValue2H = Histogram[0];
                            if (ShowDivOnOscillatorPanel)
                            {
                                RemoveDrawObject("hiddendivBullCandidateOsc2");
                                if (divergenceActive) updateBullishDivCandidateOsc2H = true;
                            }
                        }
                    }
                }

                if (hiddenbullishTriggerCountHistogram[0] > 0)
                {
                    if ((Close[0] > Low[CurrentBar - secondTroughBar2H]) && (Histogram[0] > Histogram[1]) && Close[0] > Open[0])
                    {
//                        if (firstTroughBar2H != memfirstTroughBar2H)
//                        {
//                            cptBullHistogramhdiv = 0;//#BLOOHOUND - added 17.02.03 - AzurITec
//                            maxcptBullHistogramhdiv = 1;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        }
//                        memfirstTroughBar2H = firstTroughBar2H;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        cptBullHistogramhdiv = maxcptBullHistogramhdiv;//#BLOOHOUND - added 17.02.03 - AzurITec
//                        bullishHistogramHiddenDivProjection[0] = (cptBullHistogramhdiv);//#BLOOHOUND - added 17.02.03 - AzurITec
                    }
//                    else bullishHistogramHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec						
                }
//                else bullishHistogramHiddenDivProjection[0] = (0);//#BLOOHOUND - added 17.02.03 - AzurITec

                #endregion

                #region -- draw --
                if (drawObjectsEnabled)
                {
                    bearishDivPlotSeriesMACDBB[0] = (0);
                    hiddenbearishDivPlotSeriesMACDBB[0] = (0);
                    bullishDivPlotSeriesMACDBB[0] = (0);
                    hiddenbullishDivPlotSeriesMACDBB[0] = (0);
                    bearishDivPlotSeriesHistogram[0] = (0);
                    hiddenbearishDivPlotSeriesHistogram[0] = (0);
                    bullishDivPlotSeriesHistogram[0] = (0);
                    hiddenbullishDivPlotSeriesHistogram[0] = (0);
                    DrawOnPricePanel = false;

                    #region -- MACD div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowOscillatorDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergenceOP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergenceOP1{0}",cutoffIdx));
						}
	                        if (drawBearishDivCandidateOsc1 && ShowOscillatorDivergences && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidateOsc1", false, CurrentBar - firstOscPeakBar1, firstPeakValue1, CurrentBar - secondOscPeakBar1, secondPeakValue1, BearColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (updateBearishDivCandidateOsc1 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidateOsc1", false, CurrentBar - priorFirstOscPeakBar1, priorFirstPeakValue1, CurrentBar - secondOscPeakBar1, secondPeakValue1, BearColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (drawBearishDivOnOsc1)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BearishDivergenceOP1{0}", priorSecondPeakBar1), false, CurrentBar - priorFirstOscPeakBar1, priorFirstPeakValue1, CurrentBar - priorSecondOscPeakBar1, priorSecondPeakValue1, BearColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
								}
	                        }
	                        if (drawBullishDivCandidateOsc1 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidateOsc1", false, CurrentBar - firstOscTroughBar1, firstTroughValue1, CurrentBar - secondOscTroughBar1, secondTroughValue1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (updateBullishDivCandidateOsc1 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidateOsc1", false, CurrentBar - priorFirstOscTroughBar1, priorFirstTroughValue1, CurrentBar - secondOscTroughBar1, secondTroughValue1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null);
	                        if (drawBullishDivOnOsc1)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BullishDivergenceOP1{0}", priorSecondTroughBar1), false, CurrentBar - priorFirstOscTroughBar1, priorFirstTroughValue1, CurrentBar - priorSecondOscTroughBar1, priorSecondTroughValue1, BullColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
								}
	                        }
                    }
                    #endregion

                    #region -- MACD HIDDEN div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowOscillatorHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergenceOP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergenceOP1{0}",cutoffIdx));
						}
	                        if (drawBearishDivCandidateOsc1H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidateOsc1", false, CurrentBar - firstOscPeakBar1H, firstPeakValue1H, CurrentBar - secondOscPeakBar1H, secondPeakValue1H, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
	                        if (updateBearishDivCandidateOsc1H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidateOsc1", false, CurrentBar - priorFirstOscPeakBar1H, priorFirstPeakValue1H, CurrentBar - secondOscPeakBar1H, secondPeakValue1H, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
	                        if (drawBearishDivOnOsc1H)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBearishDivergenceOP1{0}", priorSecondPeakBar1H), false, CurrentBar - priorFirstOscPeakBar1H, priorFirstPeakValue1H, CurrentBar - priorSecondOscPeakBar1H, priorSecondPeakValue1H, HiddenBearColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
								}
	                        }
	                        if (drawBullishDivCandidateOsc1H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidateOsc1", false, CurrentBar - firstOscTroughBar1H, firstTroughValue1H, CurrentBar - secondOscTroughBar1H, secondTroughValue1H, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
	                        if (updateBullishDivCandidateOsc1H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidateOsc1", false, CurrentBar - priorFirstOscTroughBar1H, priorFirstTroughValue1H, CurrentBar - secondOscTroughBar1H, secondTroughValue1H, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);
	                        if (drawBullishDivOnOsc1H)
	                        {
	                            if(!IsDebug){
									TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBullishDivergenceOP1{0}", priorSecondTroughBar1H), false, CurrentBar - priorFirstOscTroughBar1H, priorFirstTroughValue1H, CurrentBar - priorSecondOscTroughBar1H, priorSecondTroughValue1H, HiddenBullColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
								}
	                        }
                    }
                    #endregion

                    #region -- HISTO div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowHistogramDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergenceOP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergenceOP2{0}",cutoffIdx));
						}
	                        if (drawBearishDivCandidateOsc2 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidateOsc2", false, CurrentBar - firstOscPeakBar2, firstPeakValue2, CurrentBar - secondOscPeakBar2, secondPeakValue2, BearColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
	                        if (updateBearishDivCandidateOsc2 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidateOsc2", false, CurrentBar - priorFirstOscPeakBar2, priorFirstPeakValue2, CurrentBar - secondOscPeakBar2, secondPeakValue2, BearColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
	                        if (drawBearishDivOnOsc2)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BearishDivergenceOP2{0}", priorSecondPeakBar2), false, CurrentBar - priorFirstOscPeakBar2, priorFirstPeakValue2, CurrentBar - priorSecondOscPeakBar2, priorSecondPeakValue2, BearColor2, DashStyleHelper.Solid, DivWidth2);},0,null);
								}
	                        }
	                        if (drawBullishDivCandidateOsc2 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidateOsc2", false, CurrentBar - firstOscTroughBar2, firstTroughValue2, CurrentBar - secondOscTroughBar2, secondTroughValue2, BullColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
	                        if (updateBullishDivCandidateOsc2 && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidateOsc2", false, CurrentBar - priorFirstOscTroughBar2, priorFirstTroughValue2, CurrentBar - secondOscTroughBar2, secondTroughValue2, BullColor2, DashStyleHelper.Dash, DivWidth2);},0,null);
	                        if (drawBullishDivOnOsc2)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BullishDivergenceOP2{0}", priorSecondTroughBar2), false, CurrentBar - priorFirstOscTroughBar2, priorFirstTroughValue2, CurrentBar - priorSecondOscTroughBar2, priorSecondTroughValue2, BullColor2, DashStyleHelper.Solid, DivWidth2);},0,null);
								}
	                        }
                    }
                    #endregion

                    #region -- HISTO HIDDEN div --
                    if (PrintMarkers && !hidePlots && ShowDivOnOscillatorPanel && ShowHistogramHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergenceOP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergenceOP2{0}",cutoffIdx));
						}
	                        if (drawBearishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidateOsc2", false, CurrentBar - firstOscPeakBar2H, firstPeakValue2H, CurrentBar - secondOscPeakBar2H, secondPeakValue2H, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (updateBearishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidateOsc2", false, CurrentBar - priorFirstOscPeakBar2H, priorFirstPeakValue2H, CurrentBar - secondOscPeakBar2H, secondPeakValue2H, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (drawBearishDivOnOsc2H)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBearishDivergenceOP2{0}", priorSecondPeakBar2H), false, CurrentBar - priorFirstOscPeakBar2H, priorFirstPeakValue2H, CurrentBar - priorSecondOscPeakBar2H, priorSecondPeakValue2H, HiddenBearColor2, DashStyleHelper.Solid, HiddenDivWidth2);},0,null);
								}
	                        }

	                        if (drawBullishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidateOsc2", false, CurrentBar - firstOscTroughBar2H, firstTroughValue2H, CurrentBar - secondOscTroughBar2H, secondTroughValue2H, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (updateBullishDivCandidateOsc2H && !IsDebug)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidateOsc2", false, CurrentBar - priorFirstOscTroughBar2H, priorFirstTroughValue2H, CurrentBar - secondOscTroughBar2H, secondTroughValue2H, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);
	                        if (drawBullishDivOnOsc2H)
	                        {
	                            if(!IsDebug){
	                                TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBullishDivergenceOP2{0}", priorSecondTroughBar2H), false, CurrentBar - priorFirstOscTroughBar2H, priorFirstTroughValue2H, CurrentBar - priorSecondOscTroughBar2H, priorSecondTroughValue2H, HiddenBullColor2, DashStyleHelper.Solid, HiddenDivWidth2);},0,null);
								}
	                        }
                    }
                    #endregion

                    DrawOnPricePanel = true;
                    #region -- MACD div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowOscillatorDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergencePP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergencePP1{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidatePrice1", false, CurrentBar - firstPeakBar1, firstPeakHigh1 + offsetDiv1, CurrentBar - secondPeakBar1, secondPeakHigh1 + offsetDiv1, BearColor1, DashStyleHelper.Dash, DivWidth1); },0,null);// changed
	                        if (updateBearishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidatePrice1", false, CurrentBar - priorFirstPeakBar1, priorFirstPeakHigh1 + offsetDiv1, CurrentBar - secondPeakBar1, secondPeakHigh1 + offsetDiv1, BearColor1, DashStyleHelper.Dash, DivWidth1); },0,null);// changed
	                        if (drawBearishDivOnPrice1){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BearishDivergencePP1{0}", priorSecondPeakBar1), false, CurrentBar - priorFirstPeakBar1, priorFirstPeakHigh1 + offsetDiv1, CurrentBar - priorSecondPeakBar1, priorSecondPeakHigh1 + offsetDiv1, BearColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
							}

	                        if (drawBullishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidatePrice1", false, CurrentBar - firstTroughBar1, firstTroughLow1 - offsetDiv1, CurrentBar - secondTroughBar1, secondTroughLow1 - offsetDiv1, BullColor1, DashStyleHelper.Dash, DivWidth1); },0,null);// changed
	                        if (updateBullishDivCandidatePrice1)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidatePrice1", false, CurrentBar - priorFirstTroughBar1, priorFirstTroughLow1 - offsetDiv1, CurrentBar - secondTroughBar1, secondTroughLow1 - offsetDiv1, BullColor1, DashStyleHelper.Dash, DivWidth1);},0,null);// changed
	                        if (drawBullishDivOnPrice1){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BullishDivergencePP1{0}", priorSecondTroughBar1), false, CurrentBar - priorFirstTroughBar1, priorFirstTroughLow1 - offsetDiv1, CurrentBar - priorSecondTroughBar1, priorSecondTroughLow1 - offsetDiv1, BullColor1, DashStyleHelper.Solid, DivWidth1);},0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowOscillatorDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearSetup1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullSetup1{0}",cutoffIdx));
						}
                        if (drawBearSetup1){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BearSetup1{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw1, -SetupFontSize1, BearishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                        if (drawBullSetup1){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BullSetup1{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw1, SetupFontSize1, BullishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowOscillatorDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearTrigger1{0}",cutoffIdx-1));
							RemoveDrawObject(string.Format("BullTrigger1{0}",cutoffIdx-1));
						}
                        if (drawArrowDown1){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BearTrigger1{0}", CurrentBar-1), true, arrowStringDown, 1, High[1] + offsetDraw1, 2 * TriangleFontSize1 / 3, ArrowDownColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                        if (drawArrowUp1){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BullTrigger1{0}", CurrentBar-1), true, arrowStringUp, 1, Low[1] - offsetDraw1, -2 * TriangleFontSize1 / 3, ArrowUpColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    #endregion

                    #region -- MACD HIDDEN div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowOscillatorHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergencePP1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergencePP1{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidatePrice1", false, CurrentBar - firstPeakBar1H, firstPeakHigh1H + offsetDiv1, CurrentBar - secondPeakBar1H, secondPeakHigh1H + offsetDiv1, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1); },0,null);// changed
	                        if (updateBearishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidatePrice1", false, CurrentBar - priorFirstPeakBar1H, priorFirstPeakHigh1H + offsetDiv1, CurrentBar - secondPeakBar1H, secondPeakHigh1H + offsetDiv1, HiddenBearColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null); // changed
	                        if (drawBearishDivOnPrice1H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBearishDivergencePP1{0}", priorSecondPeakBar1H), false, CurrentBar - priorFirstPeakBar1H, priorFirstPeakHigh1H + offsetDiv1, CurrentBar - priorSecondPeakBar1H, priorSecondPeakHigh1H + offsetDiv1, HiddenBearColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
							}

	                        if (drawBullishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidatePrice1", false, CurrentBar - firstTroughBar1H, firstTroughLow1H - offsetDiv1, CurrentBar - secondTroughBar1H, secondTroughLow1H - offsetDiv1, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1); },0,null);// changed
	                        if (updateBullishDivCandidatePrice1H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidatePrice1", false, CurrentBar - priorFirstTroughBar1H, priorFirstTroughLow1H - offsetDiv1, CurrentBar - secondTroughBar1H, secondTroughLow1H - offsetDiv1, HiddenBullColor1, DashStyleHelper.Dash, HiddenDivWidth1);},0,null);// changed
	                        if (drawBullishDivOnPrice1H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBullishDivergencePP1{0}", priorSecondTroughBar1H), false, CurrentBar - priorFirstTroughBar1H, priorFirstTroughLow1H - offsetDiv1, CurrentBar - priorSecondTroughBar1H, priorSecondTroughLow1H - offsetDiv1, HiddenBullColor1, DashStyleHelper.Solid, HiddenDivWidth1);},0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowOscillatorHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearSetup1{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullSetup1{0}",cutoffIdx));
						}
                        if (drawBearSetup1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBearSetup1{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw1, -SetupFontSize1, HiddenBearishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}

                        if (drawBullSetup1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBullSetup1{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw1, SetupFontSize1, HiddenBullishSetupColor1, setupFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowOscillatorHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearTrigger1{0}",cutoffIdx-1));
							RemoveDrawObject(string.Format("hiddenBullTrigger1{0}",cutoffIdx-1));
						}
                        if (drawArrowDown1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBearTrigger1{0}", CurrentBar-1), true, arrowStringDown, 1, High[1] + offsetDraw1, 2 * TriangleFontSize1 / 3, HiddenArrowDownColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}

                        if (drawArrowUp1H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBullTrigger1{0}", CurrentBar-1), true, arrowStringUp, 1, Low[1] - offsetDraw1, -2 * TriangleFontSize1 / 3, HiddenArrowUpColor1, triangleFont1, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    #endregion

                    #region -- HISTO div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowHistogramDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearishDivergencePP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullishDivergencePP2{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidatePrice2", false, CurrentBar - firstPeakBar2, firstPeakHigh2 + offsetDiv2, CurrentBar - secondPeakBar2, secondPeakHigh2 + offsetDiv2, BearColor2, DashStyleHelper.Dash, DivWidth2);},0,null); // changed
	                        if (updateBearishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBearCandidatePrice2", false, CurrentBar - priorFirstPeakBar2, priorFirstPeakHigh2 + offsetDiv2, CurrentBar - secondPeakBar2, secondPeakHigh2 + offsetDiv2, BearColor2, DashStyleHelper.Dash, DivWidth2); },0,null);// changed
	                        if (drawBearishDivOnPrice2){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BearishDivergencePP2{0}", priorSecondPeakBar2), false, CurrentBar - priorFirstPeakBar2, priorFirstPeakHigh2 + offsetDiv2, CurrentBar - priorSecondPeakBar2, priorSecondPeakHigh2 + offsetDiv2, BearColor2, DashStyleHelper.Solid, DivWidth2);},0,null);
							}
	                        if (drawBullishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidatePrice2", false, CurrentBar - firstTroughBar2, firstTroughLow2 - offsetDiv2, CurrentBar - secondTroughBar2, secondTroughLow2 - offsetDiv2, BullColor2, DashStyleHelper.Dash, DivWidth2); },0,null);// changed
	                        if (updateBullishDivCandidatePrice2)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"divBullCandidatePrice2", false, CurrentBar - priorFirstTroughBar2, priorFirstTroughLow2 - offsetDiv2, CurrentBar - secondTroughBar2, secondTroughLow2 - offsetDiv2, BullColor2, DashStyleHelper.Dash, DivWidth2);},0,null);// changed
	                        if (drawBullishDivOnPrice2){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("BullishDivergencePP2{0}", priorSecondTroughBar2), false, CurrentBar - priorFirstTroughBar2, priorFirstTroughLow2 - offsetDiv2, CurrentBar - priorSecondTroughBar2, priorSecondTroughLow2 - offsetDiv2, BullColor2, DashStyleHelper.Solid, DivWidth2);},0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowHistogramDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearSetup2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("BullSetup2{0}",cutoffIdx));
						}
                        if (drawBearSetup2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BearSetup2{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw2, -SetupFontSize2, BearishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                        if (drawBullSetup2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BullSetup2{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw2, SetupFontSize2, BullishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowHistogramDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("BearTrigger2{0}",cutoffIdx-1));
							RemoveDrawObject(string.Format("BullTrigger2{0}",cutoffIdx-1));
						}
                        if (drawArrowDown2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BearTrigger2{0}", CurrentBar-1), true, arrowStringDown, 1, High[1] + offsetDraw2, 2 * TriangleFontSize2 / 3, ArrowDownColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
							if(pShowEntryPrice)
								TriggerCustomEvent(o1 =>{ Draw.Square(this,string.Format("SellPrice{0}", CurrentBar-1), true, 1, LowestLow(CurrentBar, firstPeakBar2), ArrowDownColor2, true);},0,null);
							if(pSellSignalRDivWAV.Length>0) SoundAlertHandler(pSellSignalRDivWAV, "Bearish divergence entry");
						}
                        if (drawArrowUp2){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("BullTrigger2{0}", CurrentBar-1), true, arrowStringUp, 1, Low[1] - offsetDraw2, -2 * TriangleFontSize2 / 3, ArrowUpColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
							if(pShowEntryPrice)
								TriggerCustomEvent(o1 =>{ Draw.Square(this,string.Format("BuyPrice{0}", CurrentBar-1), true, 1, HighestHigh(CurrentBar, firstTroughBar2), ArrowUpColor2, true);},0,null);
							if(pBuySignalRDivWAV.Length>0) SoundAlertHandler(pBuySignalRDivWAV, "Bullish divergence entry");
						}
                    }
                    #endregion

                    #region -- HISTO HIDDEN div --
                    if (PrintMarkers && ShowDivOnPricePanel && ShowHistogramHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearishDivergencePP2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullishDivergencePP2{0}",cutoffIdx));
						}
						if(!IsDebug){
	                        if (drawBearishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidatePrice2", false, CurrentBar - firstPeakBar2H, firstPeakHigh2H + offsetDiv2, CurrentBar - secondPeakBar2H, secondPeakHigh2H + offsetDiv2, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null); // changed
	                        if (updateBearishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBearCandidatePrice2", false, CurrentBar - priorFirstPeakBar2H, priorFirstPeakHigh2H + offsetDiv2, CurrentBar - secondPeakBar2H, secondPeakHigh2H + offsetDiv2, HiddenBearColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null); // changed
	                        if (drawBearishDivOnPrice2H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBearishDivergencePP2{0}", priorSecondPeakBar2H), false, CurrentBar - priorFirstPeakBar2H, priorFirstPeakHigh2H + offsetDiv2, CurrentBar - priorSecondPeakBar2H, priorSecondPeakHigh2H + offsetDiv2, HiddenBearColor2, DashStyleHelper.Solid, HiddenDivWidth2);},0,null);
							}

	                        if (drawBullishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidatePrice2", false, CurrentBar - firstTroughBar2H, firstTroughLow2H - offsetDiv2, CurrentBar - secondTroughBar2H, secondTroughLow2H - offsetDiv2, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null); // changed
	                        if (updateBullishDivCandidatePrice2H)
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,"hiddendivBullCandidatePrice2", false, CurrentBar - priorFirstTroughBar2H, priorFirstTroughLow2H - offsetDiv2, CurrentBar - secondTroughBar2H, secondTroughLow2H - offsetDiv2, HiddenBullColor2, DashStyleHelper.Dash, HiddenDivWidth2);},0,null);// changed
	                        if (drawBullishDivOnPrice2H){
	                            TriggerCustomEvent(o1 =>{ Draw.Line(this,string.Format("hiddenBullishDivergencePP2{0}", priorSecondTroughBar2H), false, CurrentBar - priorFirstTroughBar2H, priorFirstTroughLow2H - offsetDiv2, CurrentBar - priorSecondTroughBar2H, priorSecondTroughLow2H - offsetDiv2, HiddenBullColor2, DashStyleHelper.Solid, HiddenDivWidth2);},0,null);
							}
						}
                    }
                    if (PrintMarkers && ShowSetupDots && ShowHistogramHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearSetup2{0}",cutoffIdx));
							RemoveDrawObject(string.Format("hiddenBullSetup2{0}",cutoffIdx));
						}
                        if (drawBearSetup2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBearSetup2{0}", CurrentBar), true, setupDotString, 0, Low[0] - offsetDraw2, -SetupFontSize2, HiddenBearishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                        if (drawBullSetup2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBullSetup2{0}", CurrentBar), true, setupDotString, 0, High[0] + offsetDraw2, SetupFontSize2, HiddenBullishSetupColor2, setupFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
						}
                    }
                    if (PrintMarkers && showArrows && ShowHistogramHiddenDivergences)
                    {
						if(IsFirstTickOfBar && cutoffIdx>0) {
							RemoveDrawObject(string.Format("hiddenBearTrigger2{0}",cutoffIdx-1));
							RemoveDrawObject(string.Format("hiddenBullTrigger2{0}",cutoffIdx-1));
						}
                        if (drawArrowDown2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBearTrigger2{0}", CurrentBar-1), true, arrowStringDown, 1, High[1] + offsetDraw2, 2 * TriangleFontSize2 / 3, HiddenArrowDownColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
							if(pSellSignalRDivWAV.Length>0) SoundAlertHandler(pSellSignalRDivWAV, "Bearish divergence entry");
						}
                        if (drawArrowUp2H){
                            TriggerCustomEvent(o1 =>{ Draw.Text(this,string.Format("hiddenBullTrigger2{0}", CurrentBar-1), true, arrowStringUp, 1, Low[1] - offsetDraw2, -2 * TriangleFontSize2 / 3, HiddenArrowUpColor2, triangleFont2, textAlignmentCenter, Brushes.Transparent, Brushes.Transparent, 0);},0,null);
							if(pBuySignalRDivWAV.Length>0) SoundAlertHandler(pBuySignalRDivWAV, "Bullish divergence entry");
						}
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #endregion
line=7940;

            #region -- Structure BIAS --

            SRType = drawHigherHighLabel ? 3 : drawLowerHighLabel ? 2 : drawDoubleTopLabel ? 1 : drawDoubleBottomLabel ? -1 : drawHigherLowLabel ? -2 : drawLowerLowLabel ? -3 : 0;
//            swingHighsState[0] = drawHigherHighLabel ? 3 : drawLowerHighLabel ? 2 : drawDoubleTopLabel ? 1 : 0;//#SWINGS for BH
//            swingLowsState[0] = drawDoubleBottomLabel ? -1 : drawHigherLowLabel ? -2 : drawLowerLowLabel ? -3 : 0;//#SWINGS for BH	
line=7947;

            #region -- Oscillation State --
            int decay = 0;
            if (Calculate!= Calculate.OnBarClose && State!=State.Historical) decay = 1;
            if (SRType != 0 && structureBiasState[decay + 1] == 0)
            {
                #region -- update sequence ---
                //--- Same Trend --
                if (upTrend[1] == upTrend[0])
                {
                    if (sequence.Count == 0) sequence.Add(SRType);
                    else sequence[sequence.Count - 1] = SRType;
                }

                //--- Changing Trend ---
                else if (Calculate == Calculate.OnBarClose && upTrend[1] != upTrend[0])
                {
                    if (sequence.Count < 4) sequence.Add(SRType);
                    else
                    {
                        sequence[0] = sequence[1];
                        sequence[1] = sequence[2];
                        sequence[2] = sequence[3];
                        sequence[3] = SRType;
                    }
                }
                #region -- eachtick --
                else if (Calculate != Calculate.OnBarClose && upTrend[1] != upTrend[0])
                {
                    if (IsFirstTickOfBar)
                    {
                        if (sequence.Count < 4) sequence.Add(SRType);
                        else
                        {
                            sequence[0] = sequence[1];
                            sequence[1] = sequence[2];
                            sequence[2] = sequence[3];
                            sequence[3] = SRType;
                        }
                    }
                    else if (sequence.Count == 0) sequence.Add(SRType);
                    else sequence[sequence.Count - 1] = SRType;
                }
                #endregion
                #endregion
line=7993;

                //Oscillation State
                //Need HH/!LL/HH to go to Up Trend
                //{NEW} !LL/High/!LL/HH to go to Up Trend
                //Need LL/!HH/LL to go to Dw Trend
                //{NEW} !HH/Low/!HH/LL to go to Dw Trend
//Print(Times[0][0].ToString()+"   sequence count: "+sequence.Count);
                if (sequence.Count < 3) structureBiasState[decay] = 0;
                else if (sequence.Count < 4)
                {
                    if (sequence[0] == 3 && sequence[1] != -3 && sequence[2] == 3) structureBiasState[decay] = 1;
                    else if (sequence[0] == -3 && sequence[1] != 3 && sequence[2] == -3) structureBiasState[decay] = -1;
                    else structureBiasState[decay] = 0;
                }
                else
                {
//Print(Times[0][0].ToString()+"   a: "+sequence[0]+"  b: "+sequence[1]+"  c: "+sequence[2]+"  d: "+sequence[3]);
                    if (sequence[1] == 3 && sequence[2] != -3 && sequence[3] == 3) structureBiasState[decay] = 1;
                    else if (sequence[1] == -3 && sequence[2] != 3 && sequence[3] == -3) structureBiasState[decay] = -1;
                    //{NEW} HL/LH/HL/HH to go to Up Trend
                    else if (sequence[0] != -3 && sequence[1] > 0 && sequence[2] != -3 && sequence[3] == 3) structureBiasState[decay] = 1;
                    //{NEW} LH/HL/LH/LL to go to Up Trend
                    else if (sequence[0] != 3 && sequence[1] < 0 && sequence[2] != 3 && sequence[3] == -3) structureBiasState[decay] = -1;
                    else structureBiasState[decay] = 0;
//Print("   strucbiasstate["+decay+"]:  "+structureBiasState[decay]);
                }
            }
            #endregion

            #region -- UpTrend State --
            else if (SRType != 0 && structureBiasState[decay + 1] > 0)
            {
                if (IsFirstTickOfBar) sequence.Clear();
                //Look at Lows only. If LL go to OSC / else {HL or DB} stay UpTrend
                if (SRType == -3)
                {
                    structureBiasState[decay] = 0;
                    if (IsFirstTickOfBar) sequence.Add(SRType);
                    else if (sequence.Count == 0) sequence.Add(SRType);
                    else sequence[sequence.Count - 1] = SRType;
                }
                else structureBiasState[decay] = 1;
            }
            #endregion

            #region -- DwTrend State --
            else if (SRType != 0 && structureBiasState[decay + 1] < 0)
            {
                if (IsFirstTickOfBar) sequence.Clear();
                //Look at Highs only. If HH go to OSC / else {LH or DT} stay DwTrend
                if (SRType == 3)
                {
                    structureBiasState[decay] = 0;
                    if (IsFirstTickOfBar) sequence.Add(SRType);
                    else if (sequence.Count == 0) sequence.Add(SRType);
                    else sequence[sequence.Count - 1] = SRType;
                }
                else structureBiasState[decay] = -1;
            }
            #endregion

            else structureBiasState[decay] = structureBiasState[decay + 1];

            #endregion
line=8055;

            #region -- Setting filter values --  
            if (Calculate == Calculate.OnBarClose)
            {
line=8060;
                if (hiddenbullishCDivMACD[0] == 1.0)//#HIDDENDIV
                    macdBBState[0] = 2.5;
                else if (bullishCDivMACD[0] == 1.0)
                    macdBBState[0] = 2.0;
                else if (bearishCDivMACD[0] == 1.0)
                    macdBBState[0] = -2.0;
                else if (hiddenbearishCDivMACD[0] == 1.0)//#HIDDENDIV
                    macdBBState[0] = -2.5;

                else if (hiddenbullishTriggerCountMACDBB[0] > 0)//#HIDDENDIV
                    macdBBState[0] = 1.5;
                else if (bullishTriggerCountMACDBB[0] > 0)
                    macdBBState[0] = 1.0;
                else if (bearishTriggerCountMACDBB[0] > 0)
                    macdBBState[0] = -1.0;
                else if (hiddenbearishTriggerCountMACDBB[0] > 0)//#HIDDENDIV
                    macdBBState[0] = -1.5;

                else if (macdBBState[1] == 2.5 && !addLow && !updateLow)//#HIDDENDIV
                    macdBBState[0] = 2.5;
                else if (macdBBState[1] == 2.0 && !addLow && !updateLow)
                    macdBBState[0] = 2.0;
                else if (macdBBState[1] == -2.0 && !addHigh && !updateHigh)
                    macdBBState[0] = -2.0;
                else if (macdBBState[1] == -2.5 && !addHigh && !updateHigh)//#HIDDENDIV
                    macdBBState[0] = -2.5;

                else if (acceleration1[0] > 0)
                    macdBBState[0] = 3.0;
                else if (acceleration1[0] < 0)
                    macdBBState[0] = -3.0;
                else
                    macdBBState[0] = 0.0;

                //------------- Histogram -----------------
line=8096;

                if (hiddenbullishCDivHistogram[0] == 1.0)//#HIDDENDIV
                    histogramState[0] = 2.5;
                else if (bullishCDivHistogram[0] == 1.0)
                    histogramState[0] = 2.0;
                else if (bearishCDivHistogram[0] == 1.0)
                    histogramState[0] = -2.0;
                else if (hiddenbearishCDivHistogram[0] == 1.0)//#HIDDENDIV
                    histogramState[0] = -2.5;

                else if (hiddenbullishTriggerCountHistogram[0] > 0)//#HIDDENDIV
                    histogramState[0] = 1.5;
                else if (bullishTriggerCountHistogram[0] > 0)
                    histogramState[0] = 1.0;
                else if (bearishTriggerCountHistogram[0] > 0)
                    histogramState[0] = -1.0;
                else if (hiddenbearishTriggerCountHistogram[0] > 0)//#HIDDENDIV
                    histogramState[0] = -1.5;

                else if (histogramState[1] == 2.5 && !addLow && !updateLow)//#HIDDENDIV
                    histogramState[0] = 2.5;
                else if (histogramState[1] == 2.0 && !addLow && !updateLow)
                    histogramState[0] = 2.0;
                else if (acceleration2[0] > 0)
                    histogramState[0] = 3.0;
                else if (acceleration2[0] < 0)
                    histogramState[0] = -3.0;
                else if (histogramState[1] == -2.0 && !addHigh && !updateHigh)
                    histogramState[0] = -2.0;
                else if (HistogramState[1] == -2.5 && !addHigh && !updateHigh)//#HIDDENDIV
                    histogramState[0] = -2.5;
                else
                    histogramState[0] = 0.0;
            }
            else
            {
line=8133;
                //----------------- MACD --------------------
                if (hiddenbullishCDivMACD[0] == 1.0)//#HIDDENDIV
                    macdBBState[0] = 2.5;
                else if (bullishCDivMACD[0] == 1.0)
                    macdBBState[0] = 2.0;
                else if (bearishCDivMACD[0] == 1.0)
                    macdBBState[0] = -2.0;
                else if (hiddenbearishCDivMACD[0] == 1.0)//#HIDDENDIV
                    macdBBState[0] = -2.5;

                else if (hiddenbullishTriggerCountMACDBB[0] > 0)//#HIDDENDIV
                    macdBBState[0] = 1.5;
                else if (bullishTriggerCountMACDBB[0] > 0)
                    macdBBState[0] = 1.0;
                else if (bearishTriggerCountMACDBB[0] > 0)
                    macdBBState[0] = -1.0;
                else if (hiddenbearishTriggerCountMACDBB[0] > 0)//#HIDDENDIV
                    macdBBState[0] = -1.5;

                else if (macdBBState[1] == 2.5 && !intraBarAddLow && !intraBarUpdateLow)//#HIDDENDIV
                    macdBBState[0] = 2.5;
                else if (macdBBState[1] == 2.0 && !intraBarAddLow && !intraBarUpdateLow)
                    macdBBState[0] = 2.0;
                else if (macdBBState[1] == -2.0 && !intraBarAddHigh && !intraBarUpdateHigh)
                    macdBBState[0] = -2.0;
                else if (macdBBState[1] == -2.5 && !intraBarAddHigh && !intraBarUpdateHigh)//#HIDDENDIV
                    macdBBState[0] = -2.5;

                else if (acceleration1[0] > 0)
                    macdBBState[0] = 3.0;
                else if (acceleration1[0] < 0)
                    macdBBState[0] = -3.0;
                else
                    macdBBState[0] = 0.0;

                //------------- Histogram -----------------
                if (hiddenbullishCDivHistogram[0] == 1.0)//#HIDDENDIV
                    histogramState[0] = 2.5;
                if (bullishCDivHistogram[0] == 1.0)
                    histogramState[0] = 2.0;
                else if (bearishCDivHistogram[0] == 1.0)
                    histogramState[0] = -2.0;
                else if (hiddenbearishCDivHistogram[0] == 1.0)//#HIDDENDIV
                    histogramState[0] = -2.5;

                else if (hiddenbullishTriggerCountHistogram[0] > 0)//#HIDDENDIV
                    histogramState[0] = 1.5;
                else if (bullishTriggerCountHistogram[0] > 0)
                    histogramState[0] = 1.0;
                else if (bearishTriggerCountHistogram[0] > 0)
                    histogramState[0] = -1.0;
                else if (hiddenbearishTriggerCountHistogram[0] > 0)//#HIDDENDIV
                    histogramState[0] = -1.5;

                else if (histogramState[1] == 2.5 && !intraBarAddLow && !intraBarUpdateLow)//#HIDDENDIV
                    histogramState[0] = 2.5;
                else if (histogramState[1] == 2.0 && !intraBarAddLow && !intraBarUpdateLow)
                    histogramState[0] = 2.0;
                else if (histogramState[1] == -2.0 && !intraBarAddHigh && !intraBarUpdateHigh)
                    histogramState[0] = -2.0;
                else if (histogramState[1] == -2.5 && !intraBarAddHigh && !intraBarUpdateHigh)//#HIDDENDIV
                    histogramState[0] = -2.5;

                else if (acceleration2[0] > 0)
                    histogramState[0] = 3.0;
                else if (acceleration2[0] < 0)
                    histogramState[0] = -3.0;
                else
                    histogramState[0] = 0.0;
            }
            #endregion

}catch(Exception e){Print(line+":    "+e.ToString());}
        }
		private double HighestHigh(int RMaB, int LMaB){
			if(RMaB<LMaB){int tm = RMaB; RMaB=LMaB; LMaB=tm;}
			var max = Highs[0].GetValueAt(RMaB);
			for(int i = RMaB; i>= LMaB; i--) max = Math.Max(max,Highs[0].GetValueAt(i));
			return max;
		}
		private double LowestLow(int RMaB, int LMaB){
			if(RMaB<LMaB){int tm = RMaB; RMaB=LMaB; LMaB=tm;}
//if(RMaB-LMaB > 50) LMaB = RMaB-50;
//bool z = RMaB > BarsArray[0].Count-20;
			var min = Lows[0].GetValueAt(RMaB);
//if(z)Print(Times[0].GetValueAt(RMaB).ToString()+"   Lowest low at "+min);
			for(int i = RMaB; i>= LMaB; i--){
				min = Math.Min(min,Lows[0].GetValueAt(i));
//if(z)Print(Times[0].GetValueAt(i).ToString()+"   Lowest low now at "+min);
			}
			return min;
		}
		private void SoundAlertHandler(string wav, string msg){
			if(wav=="SOUND OFF") return;
			string filename = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", StripOutIllegalCharacters(wav.Replace("<inst>",Instruments[0].MasterInstrument.Name)," "));
			if(System.IO.File.Exists(filename))
				Alert(
					DateTime.Now.Millisecond.ToString(),
					Priority.High,
					msg,
					filename,
					0,
					Brushes.Gold,
					Brushes.Black);
			else
				Log("HistoDivergence alert triggered, but failed.  Could not find "+filename,NinjaTrader.Cbi.LogLevel.Information);
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
		private SharpDX.Direct2D1.Brush BullishDivergenceDXBrush, BearishDivergenceDXBrush, DataTableDXBrush, OppositeBkgDXBrush;
		private SharpDX.Direct2D1.Brush BullishBkgDXBrush, BearishBkgDXBrush, ChannelBkgDXBrush;
		private SharpDX.Direct2D1.Brush DeepBullishBkgDXBrush, DeepBearishBkgDXBrush, BullishAccelerationDXBrush, BearishAccelerationDXBrush;

		public override void OnRenderTargetChanged()
		{
			#region -- OnRenderTargetChanged --
			if(BullishBkgDXBrush!=null && !BullishBkgDXBrush.IsDisposed) {BullishBkgDXBrush.Dispose(); BullishBkgDXBrush=null;}
			if(RenderTarget!=null){
				BullishBkgDXBrush         = BullishBackgroundColor.ToDxBrush(RenderTarget);
				BullishBkgDXBrush.Opacity = BackgroundOpacity/100f;
			}

			if(BearishBkgDXBrush!=null && !BearishBkgDXBrush.IsDisposed) {BearishBkgDXBrush.Dispose(); BearishBkgDXBrush=null;}
			if(RenderTarget!=null){
				BearishBkgDXBrush         = BearishBackgroundColor.ToDxBrush(RenderTarget);
				BearishBkgDXBrush.Opacity = BackgroundOpacity/100f;
			}
			if(DeepBullishBkgDXBrush!=null && !DeepBullishBkgDXBrush.IsDisposed) {DeepBullishBkgDXBrush.Dispose(); DeepBullishBkgDXBrush=null;}
			if(RenderTarget!=null){
				DeepBullishBkgDXBrush         = DeepBullishBackgroundColor.ToDxBrush(RenderTarget);
				DeepBullishBkgDXBrush.Opacity = BackgroundOpacity/100f;
			}
			if(DeepBearishBkgDXBrush!=null && !DeepBearishBkgDXBrush.IsDisposed) {DeepBearishBkgDXBrush.Dispose(); DeepBearishBkgDXBrush=null;}
			if(RenderTarget!=null){
				DeepBearishBkgDXBrush         = DeepBearishBackgroundColor.ToDxBrush(RenderTarget);
				DeepBearishBkgDXBrush.Opacity = BackgroundOpacity/100f;
			}
			if(OppositeBkgDXBrush!=null && !OppositeBkgDXBrush.IsDisposed) {OppositeBkgDXBrush.Dispose(); OppositeBkgDXBrush=null;}
			if(RenderTarget!=null){
				OppositeBkgDXBrush         = OppositeBackgroundColor.ToDxBrush(RenderTarget);
				OppositeBkgDXBrush.Opacity = BackgroundOpacity/100f;
			}
			if(ChannelBkgDXBrush!=null && !ChannelBkgDXBrush.IsDisposed) {ChannelBkgDXBrush.Dispose(); ChannelBkgDXBrush=null;}
			if(RenderTarget!=null){
				ChannelBkgDXBrush = ChannelColor.ToDxBrush(RenderTarget);
	            ChannelBkgDXBrush.Opacity = ChannelOpacity / 100f;
			}
//			if(BullishAccelerationDXBrush!=null && !BullishAccelerationDXBrush.IsDisposed) {BullishAccelerationDXBrush.Dispose(); BullishAccelerationDXBrush=null;}
//			if(RenderTarget!=null)
//				BullishAccelerationDXBrush = BullishAccelerationColor.ToDxBrush(RenderTarget);

//			if(BearishAccelerationDXBrush!=null && !BearishAccelerationDXBrush.IsDisposed) {BearishAccelerationDXBrush.Dispose(); BearishAccelerationDXBrush=null;}
//			if(RenderTarget!=null)
//				BearishAccelerationDXBrush = BearishAccelerationColor.ToDxBrush(RenderTarget);

//			if(aa!=null && !aa.IsDisposed) {aa.Dispose(); aa=null;}
			#endregion
		}

//===================================================================================================================
        #region -- OnRender : Custom Drawing --  
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
line=8212;
            if (!IsVisible || ChartBars.ToIndex < BarsRequiredToPlot) return;
            int lastBarIndex = Math.Min(CurrentBar, ChartBars.ToIndex);//RIGHT BAR idx (absolute)
            if (lastBarIndex < BarsRequiredToPlot) return;

            int firstBarIndex = Math.Max(BarsRequiredToPlot, ChartBars.FromIndex);//LEFT BAR idx (absolute)

            SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
            RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;

            #region -- Zero Line --
            drawLine(0, 0, lastBarIndex, firstBarIndex, ZerolineColor, ZerolineStyle, ZerolineWidth, chartControl, chartScale);            
            #endregion

line=8226;
//			var BullishBkgDXBrush     = BullishBackgroundColor.ToDxBrush(RenderTarget);
//			BullishBkgDXBrush.Opacity = BackgroundOpacity/100f;
//			var BearishBkgDXBrush     = BearishBackgroundColor.ToDxBrush(RenderTarget);
//			BearishBkgDXBrush.Opacity = BackgroundOpacity/100f;
			
//			var DeepBullishBkgDXBrush     = DeepBullishBackgroundColor.ToDxBrush(RenderTarget);
//			DeepBullishBkgDXBrush.Opacity = BackgroundOpacity/100f;
//			var DeepBearishBkgDXBrush     = DeepBearishBackgroundColor.ToDxBrush(RenderTarget);
//			DeepBearishBkgDXBrush.Opacity = BackgroundOpacity/100f;

//			var OppositeBkgDXBrush     = OppositeBackgroundColor.ToDxBrush(RenderTarget);
//			OppositeBkgDXBrush.Opacity = BackgroundOpacity/100f;

//			var DataTableDXBrush = DataTableColor.ToDxBrush(RenderTarget);
//			var BullishDivergenceDXBrush = BullishDivergenceColor.ToDxBrush(RenderTarget);
//			var BearishDivergenceDXBrush = BearishDivergenceColor.ToDxBrush(RenderTarget);

line=8244;

			#region -- Draw Channel + Div on indicator panel + flooding on momo panel --
            bool drawbbchannel = CurrentBar > BarsRequiredToPlot && ChannelOpacity > 0 && !IsDebug;
            int divergenceSpan = 0;
			var ChannelBkgDXBrush = ChannelColor.ToDxBrush(RenderTarget);
            ChannelBkgDXBrush.Opacity = ChannelOpacity / 100f;
//	Print("7953  Background flooding: "+BackgroundFlooding.ToString());

            for (int i = lastBarIndex; i >= firstBarIndex; i--)
            {
                try
                {
                    #region -- draw Bollinger Channel --
                    if (drawbbchannel){
//						int x = chartControl.GetXByBarIndex(chartControl.BarsArray[0], i);
//						int y = chartScale.GetYByValue(-3);
//						RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x,y),5,5),Brushes.Yellow.ToDxBrush(RenderTarget));
//						var y0 = chartScale.GetYByValue(Upper.GetValueAt(i));
//						var y1 = chartScale.GetYByValue(Upper.GetValueAt(i - 1));
//						var y2 = chartScale.GetYByValue(Lower.GetValueAt(i - 1));
//						var y3 = chartScale.GetYByValue(Lower.GetValueAt(i));
//						var x0 = chartControl.GetXByBarIndex(chartControl.BarsArray[0], i);
//						var x1 = chartControl.GetXByBarIndex(chartControl.BarsArray[0], i - 1);
//if(i==lastBarIndex) {
//	RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x0,y0),5,5),Brushes.Yellow.ToDxBrush(RenderTarget));
//	RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x1,y1),5,5),Brushes.Yellow.ToDxBrush(RenderTarget));
//	RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x1,y2),5,5),Brushes.Yellow.ToDxBrush(RenderTarget));
//	RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(x0,y3),5,5),Brushes.Yellow.ToDxBrush(RenderTarget));
//}
                        drawRegion(
							new double[] { Upper.GetValueAt(i), Upper.GetValueAt(i-1), Lower.GetValueAt(i-1), Lower.GetValueAt(i) }, 
							new int[] { i, i-1, i-1, i }, 
							ChannelBkgDXBrush, chartControl, chartScale);//25.05.16 - beta11 code breaking change
					}
                    #endregion

                    #region -- BEAR DIV MACD --
                    if (bearishDivPlotSeriesMACDBB.IsValidDataPointAt(i))
                    {
                        divergenceSpan = bearishDivPlotSeriesMACDBB.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(BBMACD.GetValueAt(i), BBMACD.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, BearColor1, DashStyleHelper.Solid, DivWidth1, chartControl, chartScale);                        
                    }
                    #endregion

                    #region -- BEAR HIDDEN DIV MACD --
                    if (hiddenbearishDivPlotSeriesMACDBB.IsValidDataPointAt(i))
                    {
                        divergenceSpan = hiddenbearishDivPlotSeriesMACDBB.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(BBMACD.GetValueAt(i), BBMACD.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, HiddenBearColor1, DashStyleHelper.Solid, HiddenDivWidth1, chartControl, chartScale);                        
                    }
                    #endregion

                    #region -- BULL DIV MACD --
                    if (bullishDivPlotSeriesMACDBB.IsValidDataPointAt(i))
                    {
                        divergenceSpan = bullishDivPlotSeriesMACDBB.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(BBMACD.GetValueAt(i), BBMACD.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, BullColor1, DashStyleHelper.Solid, DivWidth1, chartControl, chartScale);
                    }
                    #endregion

                    #region -- BULL HIDDEN DIV MACD --
                    if (hiddenbullishDivPlotSeriesMACDBB.IsValidDataPointAt(i))
                    {
                        divergenceSpan = hiddenbullishDivPlotSeriesMACDBB.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(BBMACD.GetValueAt(i), BBMACD.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, HiddenBullColor1, DashStyleHelper.Solid, HiddenDivWidth1, chartControl, chartScale);
                    }
                    #endregion

                    #region -- BEAR DIV HISTO --
                    if (bearishDivPlotSeriesHistogram.IsValidDataPointAt(i))
                    {
                        divergenceSpan = bearishDivPlotSeriesHistogram.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(Histogram.GetValueAt(i), Histogram.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, BearColor2, DashStyleHelper.Solid, DivWidth2, chartControl, chartScale);
                    }
                    #endregion

                    #region -- BEAR HIDDEN DIV HISTO --
                    if (hiddenbearishDivPlotSeriesHistogram.IsValidDataPointAt(i))
                    {
                        divergenceSpan = hiddenbearishDivPlotSeriesHistogram.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(Histogram.GetValueAt(i), Histogram.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, HiddenBearColor2, DashStyleHelper.Solid, HiddenDivWidth2, chartControl, chartScale);
                    }
                    #endregion

                    #region -- BULL DIV HISTO --
                    if (bullishDivPlotSeriesHistogram.IsValidDataPointAt(i))
                    {
                        divergenceSpan = bullishDivPlotSeriesHistogram.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(Histogram.GetValueAt(i), Histogram.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, BullColor2, DashStyleHelper.Solid, DivWidth2, chartControl, chartScale);
                    }
                    #endregion

                    #region -- BULL HIDDEN DIV HISTO --
                    if (hiddenbullishDivPlotSeriesHistogram.IsValidDataPointAt(i))
                    {
                        divergenceSpan = hiddenbullishDivPlotSeriesHistogram.GetValueAt(i);
                        if (divergenceSpan > 0) drawLine(Histogram.GetValueAt(i), Histogram.GetValueAt(i - divergenceSpan), i-1, i - divergenceSpan-1, HiddenBullColor2, DashStyleHelper.Solid, HiddenDivWidth2, chartControl, chartScale);
                    }
                    #endregion

                    //##MODIF## AzurITec - 18.05.2016
                    #region -- flooding on momo panel only --
                    if (BackgroundFlooding != HistoDivergence_Flooding.None)
                    {
                        int[] indexes = new int[] { i, i + 1, i + 1, i };
                        double[] minmax = new double[] { chartScale.MaxValue, chartScale.MaxValue, chartScale.MinValue, chartScale.MinValue };
                        if (BackgroundFlooding == HistoDivergence_Flooding.Histogram)
                        {
                            if (Histogram.GetValueAt(i) > 0)
                                drawRegion(minmax, indexes, BullishBkgDXBrush, chartControl, chartScale, false, true);
                            else if (Histogram.GetValueAt(i) < 0)
                                drawRegion(minmax, indexes, BearishBkgDXBrush, chartControl, chartScale, false, true);
                            else if (Histogram.GetValueAt(i + 1) > 0)
                                drawRegion(minmax, indexes, BullishBkgDXBrush, chartControl, chartScale, false, true);
                            else
                                drawRegion(minmax, indexes, BearishBkgDXBrush, chartControl, chartScale, false, true);
                        }
                        else if (BackgroundFlooding == HistoDivergence_Flooding.Structure)
                        {
                            if (StructureBiasState.GetValueAt(i) > 0)
                                drawRegion(minmax, indexes, BullishBkgDXBrush, chartControl, chartScale, false, true);
                            else if (StructureBiasState.GetValueAt(i) < 0)
                                drawRegion(minmax, indexes, BearishBkgDXBrush, chartControl, chartScale, false, true);
                        }
                        else
                        {
                            if (StructureBiasState.GetValueAt(i) > 0)
                            {
                                if (Histogram.GetValueAt(i) > 0)
                                    drawRegion(minmax, indexes, DeepBullishBkgDXBrush, chartControl, chartScale, false, true);
                                else if (Histogram.GetValueAt(i) < 0)
                                    drawRegion(minmax, indexes, OppositeBkgDXBrush, chartControl, chartScale, false, true);
                                else if (Histogram.GetValueAt(i + 1) > 0)
                                    drawRegion(minmax, indexes, DeepBullishBkgDXBrush, chartControl, chartScale, false, true);
                                else
                                    drawRegion(minmax, indexes, OppositeBkgDXBrush, chartControl, chartScale, false, true);
                            }
                            else if (StructureBiasState.GetValueAt(i) < 0)
                            {
                                if (Histogram.GetValueAt(i) > 0)
                                    drawRegion(minmax, indexes, OppositeBkgDXBrush, chartControl, chartScale, false, true);
                                else if (Histogram.GetValueAt(i) < 0)
                                    drawRegion(minmax, indexes, DeepBearishBkgDXBrush, chartControl, chartScale, false, true);
                                else if (Histogram.GetValueAt(i + 1) > 0)
                                    drawRegion(minmax, indexes, OppositeBkgDXBrush, chartControl, chartScale, false, true);
                                else
                                    drawRegion(minmax, indexes, DeepBearishBkgDXBrush, chartControl, chartScale, false, true);
                            }
                            else
                            {
                                if (Histogram.GetValueAt(i) > 0)
                                    drawRegion(minmax, indexes, BullishBkgDXBrush, chartControl, chartScale, false, true);
                                else if (Histogram.GetValueAt(i) < 0)
                                    drawRegion(minmax, indexes, BearishBkgDXBrush, chartControl, chartScale, false, true);
                                else if (Histogram.GetValueAt(i + 1) > 0)
                                    drawRegion(minmax, indexes, BullishBkgDXBrush, chartControl, chartScale, false, true);
                                else
                                    drawRegion(minmax, indexes, BearishBkgDXBrush, chartControl, chartScale, false, true);
                            }
                        }
                    }
                    #endregion

                }
                catch (Exception ee) {Print("OnRender 8880 "+ee.ToString()); }//After testing, didn't find any problem. Let Try Catch to avoid crashes...
            }
            #endregion

            #region -- excursion levels --
            try
            {
                int idxMin = lastBarIndex;
                int idxMax = firstBarIndex;
                int x1;

                int idxHLineStart = idxMax;
                if (PlotStyleLevels == PlotStyle.HLine) idxMax = idxMin;

                for (int i = idxMin; i >= idxMax; i--)
                {
                    x1 = PlotStyleLevels == PlotStyle.HLine ? idxHLineStart : i - 1;
                    #region -- Excursion Levels --
                    if (DisplayLevel3)
                    {   
                        drawLine(PriceExcursionUL3.GetValueAt(i), PriceExcursionUL3.GetValueAt(i - 1), i, x1, Level3Color, DashStyleHelper.Solid, PlotWidthLevels, chartControl, chartScale);
                        drawLine(PriceExcursionLL3.GetValueAt(i), PriceExcursionLL3.GetValueAt(i - 1), i, x1, Level3Color, DashStyleHelper.Solid, PlotWidthLevels, chartControl, chartScale);
                    }
                    if (DisplayLevel2)
                    {
                        drawLine(PriceExcursionUL2.GetValueAt(i), PriceExcursionUL2.GetValueAt(i - 1), i, x1, Level2Color, DashStyleHelper.Solid, PlotWidthLevels, chartControl, chartScale);
                        drawLine(PriceExcursionLL2.GetValueAt(i), PriceExcursionLL2.GetValueAt(i - 1), i, x1, Level2Color, DashStyleHelper.Solid, PlotWidthLevels, chartControl, chartScale);
                    }
                    if (DisplayLevel1)
                    {
                        drawLine(PriceExcursionUL1.GetValueAt(i), PriceExcursionUL1.GetValueAt(i - 1), i, x1, Level1Color, DashStyleHelper.Solid, PlotWidthLevels, chartControl, chartScale);
                        drawLine(PriceExcursionLL1.GetValueAt(i), PriceExcursionLL1.GetValueAt(i - 1), i, x1, Level1Color, DashStyleHelper.Solid, PlotWidthLevels, chartControl, chartScale);
                    }
                    #endregion
                }
            }
            catch (Exception) { }//thrown if scrollback to chart first bar
            #endregion

			base.OnRender(chartControl, chartScale);

line=8679;
            RenderTarget.AntialiasMode = oldAntialiasMode;
//			ChannelBkgDXBrush.Dispose();  ChannelBkgDXBrush = null;
//			BullishBkgDXBrush.Dispose();  BullishBkgDXBrush=null;
//			BearishBkgDXBrush.Dispose();  BearishBkgDXBrush=null;
//			DeepBullishBkgDXBrush.Dispose();  DeepBullishBkgDXBrush=null;
//			DeepBearishBkgDXBrush.Dispose();  DeepBearishBkgDXBrush=null;
//			OppositeBkgDXBrush.Dispose();     OppositeBkgDXBrush=null;
//			DataTableDXBrush.Dispose();       DataTableDXBrush = null;
//			BullishDivergenceDXBrush.Dispose(); BullishDivergenceDXBrush = null;
//			BearishDivergenceDXBrush.Dispose(); BearishDivergenceDXBrush = null;
//			BullishAccelerationDXBrush.Dispose(); BullishAccelerationDXBrush=null;
//			BearishAccelerationDXBrush.Dispose(); BearishAccelerationDXBrush=null;

        }
        #endregion

        #region -- drawing functions { AzurITec } --
        //Draw Rectangle Region. x and y as pixel coordinate, w and h in pixel too.
        private void drawRectangle(double x, double y, double w, double h, SharpDX.Direct2D1.Brush dxbrush, int opacity = 100)
        {
            drawRegion(new Point[] { new Point(x, y), new Point(x + w, y), new Point(x + w, y + h), new Point(x, y + h) }, dxbrush);
        }

        private void drawRegion(Point[] points, SharpDX.Direct2D1.Brush dxbrush)
        {
            SharpDX.Vector2[] vectors = new[] { points[1].ToVector2(), points[2].ToVector2(), points[3].ToVector2() };

            SharpDX.Direct2D1.PathGeometry geo1 = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
            SharpDX.Direct2D1.GeometrySink sink1 = geo1.Open();
            sink1.BeginFigure(points[0].ToVector2(), SharpDX.Direct2D1.FigureBegin.Filled);
            sink1.AddLines(vectors);
            sink1.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
            sink1.Close();

            RenderTarget.FillGeometry(geo1, dxbrush);
            geo1.Dispose();
            sink1.Dispose();
        }

        private void drawRegion(double[] yValues, int[] xIndex, SharpDX.Direct2D1.Brush dxbrush, ChartControl chartControl, ChartScale chartScale, bool drawOnPricePanel = false, bool atMiddle = false)
        {
#if USE_WPF_COORDS
            drawRegion(new Point[]{
                    new Point(GetX0(xIndex[0], chartControl, atMiddle), drawOnPricePanel?0:ChartPanel.Y + chartScale.GetYByValueWpf(yValues[0])),
                    new Point(GetX0(xIndex[1], chartControl, atMiddle), drawOnPricePanel?0:ChartPanel.Y + chartScale.GetYByValueWpf(yValues[1])),
                    new Point(GetX0(xIndex[2], chartControl, atMiddle), drawOnPricePanel?0:ChartPanel.Y + chartScale.GetYByValueWpf(yValues[2])),
                    new Point(GetX0(xIndex[3], chartControl, atMiddle), drawOnPricePanel?0:ChartPanel.Y + chartScale.GetYByValueWpf(yValues[3]))
                },
                dxbrush
                );
#else
            drawRegion(new Point[]{
                    new Point(GetX0(xIndex[0], chartControl, atMiddle), drawOnPricePanel?0:chartScale.GetYByValue(yValues[0])),
                    new Point(GetX0(xIndex[1], chartControl, atMiddle), drawOnPricePanel?0:chartScale.GetYByValue(yValues[1])),
                    new Point(GetX0(xIndex[2], chartControl, atMiddle), drawOnPricePanel?0:chartScale.GetYByValue(yValues[2])),
                    new Point(GetX0(xIndex[3], chartControl, atMiddle), drawOnPricePanel?0:chartScale.GetYByValue(yValues[3]))
                },
                dxbrush
                );
#endif
        }

        //Draw a line between 2 points not in pixel coordinates. The x value is a relative the bar index. The y value is the numerical value (ie price / oscillator value)
        private void drawLine(double val1, double val2, int idxslot1, int idxslot2, Brush couleur, DashStyleHelper dashstyle, int width, ChartControl chartControl, ChartScale chartScale, bool drawOnPricePanel = false)
        {
            SharpDX.Direct2D1.Brush linebrush = couleur.ToDxBrush(RenderTarget);
            SharpDX.Direct2D1.DashStyle _dashstyle;
            if (!Enum.TryParse(dashstyle.ToString(), true, out _dashstyle)) _dashstyle = SharpDX.Direct2D1.DashStyle.Dash;

            SharpDX.Direct2D1.StrokeStyleProperties properties = new SharpDX.Direct2D1.StrokeStyleProperties() { DashStyle = _dashstyle };
            SharpDX.Direct2D1.StrokeStyle strokestyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, properties);

#if USE_WPF_COORDS
            Point p0 = new Point(GetX0(idxslot1, chartControl), drawOnPricePanel ? 0 : ChartPanel.Y + chartScale.GetYByValueWpf(val1));
            Point p1 = new Point(GetX0(idxslot2, chartControl), drawOnPricePanel ? 0 : ChartPanel.Y + chartScale.GetYByValueWpf(val2));
#else
            Point p0 = new Point(GetX0(idxslot1, chartControl), drawOnPricePanel ? 0 : chartScale.GetYByValue(val1));
            Point p1 = new Point(GetX0(idxslot2, chartControl), drawOnPricePanel ? 0 : chartScale.GetYByValue(val2));
#endif
            RenderTarget.DrawLine(p0.ToVector2(), p1.ToVector2(), linebrush, width, strokestyle);

            linebrush.Dispose();
            strokestyle.Dispose();
        }
        //Draw a line between 2 points in pixel coordinates.        
        private void drawLine(double x1, double x2, double y1, double y2, Brush couleur, DashStyleHelper dashstyle, int width)
        {
            SharpDX.Direct2D1.Brush linebrush = couleur.ToDxBrush(RenderTarget);
            SharpDX.Direct2D1.DashStyle _dashstyle;
            if (!Enum.TryParse(dashstyle.ToString(), true, out _dashstyle)) _dashstyle = SharpDX.Direct2D1.DashStyle.Dash;

            SharpDX.Direct2D1.StrokeStyleProperties properties = new SharpDX.Direct2D1.StrokeStyleProperties() { DashStyle = _dashstyle };
            SharpDX.Direct2D1.StrokeStyle strokestyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, properties);

            Point p0 = new Point(x1, y1);
            Point p1 = new Point(x2, y2);
            RenderTarget.DrawLine(p0.ToVector2(), p1.ToVector2(), linebrush, width, strokestyle);

            linebrush.Dispose();
            strokestyle.Dispose();
        }

        //Draw a text at {x;y} coordinates in pixel.
        private void drawstring(string text, double x, double y, SimpleFont font, Brush textBrush, SharpDX.DirectWrite.TextAlignment textAlignment, float width = 0f)
        {
            SharpDX.Direct2D1.Factory factory = new SharpDX.Direct2D1.Factory();
            Point textpoint = new Point(x, y);
            TextFormat textFormat = new TextFormat(
                Core.Globals.DirectWriteFactory,
                font.FamilySerialize,
                font.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
                font.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
                SharpDX.DirectWrite.FontStretch.Normal,
                (float)font.Size
                )
            { TextAlignment = textAlignment, WordWrapping = WordWrapping.NoWrap };
            TextLayout textLayout = new TextLayout(Core.Globals.DirectWriteFactory, text, textFormat, width == 0f ? getTextWidth(text, font) : width, float.MaxValue);

			var textBrushDX = textBrush.ToDxBrush(RenderTarget);
            RenderTarget.DrawTextLayout(textpoint.ToVector2(), textLayout, textBrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textBrushDX.Dispose(); textBrushDX=null;

            textLayout.Dispose();
            textFormat.Dispose();
        }

        private float getTextWidth(string text, SimpleFont font)
        {
            TextFormat textFormat = new TextFormat(
                Core.Globals.DirectWriteFactory,
                font.FamilySerialize,
                font.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
                font.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
                SharpDX.DirectWrite.FontStretch.Normal,
                (float)font.Size
                );
            TextLayout textLayout = new TextLayout(Core.Globals.DirectWriteFactory, text, textFormat, float.MaxValue, float.MaxValue);

            float textwidth = textLayout.Metrics.Width;

            textLayout.Dispose();
            textFormat.Dispose();

            return textwidth;
        }

        //retrieve the x coordinate in pixel of a a relative bar index.
        //##MODIF## AzurITec - 18.05.2016
        private int GetX0(int bars, ChartControl chartControl, bool atMiddle = false) { return chartControl.GetXByBarIndex(chartControl.BarsArray[0], bars) - (atMiddle ? (int)(chartControl.Properties.BarDistance / 2) : 0); }//NEW VERSION NT8        
        #endregion

        #region Properties
        
        #region -- PLOTS --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BBMACD { get { return Values[BBMACDIDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BBMACDFrame { get { return Values[BBMACDFrameIDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BBMACDLine { get { return Values[BBMACDLineIDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> Average { get { return Values[AverageIDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> Upper { get { return Values[UpperIDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> Lower { get { return Values[LowerIDX]; } }

        /// <summary>
        /// RJ5Group LLC
        /// </summary>
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> Histogram { get { return Values[HistogramIDX]; } }

        /// <summary>
        /// //rj RJ5GROUP code Algorithm
        /// </summary>
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionUL3 { get { return Values[PriceExcursionUL3IDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionUL2 { get { return Values[PriceExcursionUL2IDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionUL1 { get { return Values[PriceExcursionUL1IDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionLL1 { get { return Values[PriceExcursionLL1IDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionLL2 { get { return Values[PriceExcursionLL2IDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionLL3 { get { return Values[PriceExcursionLL3IDX]; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionMAX { get { return Values[PriceExcursionMAXIDX]; } }
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriceExcursionMIN { get { return Values[PriceExcursionMINIDX]; } }
        #endregion

        #region -- Exposed DataSeries --
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BearishMACDDivProjection { get { return bearishMACDDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BullishMACDDivProjection { get { return bullishMACDDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BearishHistogramDivProjection { get { return bearishHistogramDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BullishHistogramDivProjection { get { return bullishHistogramDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec

//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BearishMACDHiddenDivProjection { get { return bearishMACDHiddenDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BullishMACDHiddenDivProjection { get { return bullishMACDHiddenDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BearishHistogramHiddenDivProjection { get { return bearishHistogramHiddenDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> BullishHistogramHiddenDivProjection { get { return bullishHistogramHiddenDivProjection; } }//#BLOOHOUND - added 17.02.03 - AzurITec
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> StructureBiasState { get { return structureBiasState; } }//#STRBIAS
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> SwingHighsState { get { return swingHighsState; } }//#SWINGS
//        [Browsable(false)]
//        [XmlIgnore()]
//        public Series<double> SwingLowsState { get { return swingLowsState; } }//#SWINGS

        #region -- Trend Filters --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BBDotTrend { get { return bbDotTrend; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MomoSignum { get { return momoSignum; } }
        #endregion

        //rj RJ5GROUP code Algorithm
        #region -- Setup Signals --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BearishPDivMACD { get { return bearishPDivMACD; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BullishPDivMACD { get { return bullishPDivMACD; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BearishPDivHistogram { get { return bearishPDivHistogram; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BullishPDivHistogram { get { return bullishPDivHistogram; } }
        #endregion

        #region -- Divergence Signals --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BearishCDivMACD { get { return bearishCDivMACD; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BullishCDivMACD { get { return bullishCDivMACD; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BearishCDivHistogram { get { return bearishCDivHistogram; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> BullishCDivHistogram { get { return bullishCDivHistogram; } }
        #endregion

        #region -- Complex States --
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MACDBBState { get { return macdBBState; } }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> HistogramState { get { return histogramState; } }
        #endregion

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
					filCustom = dirCustom.GetFiles( search);
				}catch{}

				var list = new List<string>();//new string[filCustom.Length+1];
				list.Add("SOUND OFF");
				list.Add("<inst>_BuyEntry.wav");
				list.Add("<inst>_SellEntry.wav");
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
        [NinjaScriptProperty]
        [Display(Name = "Optimize for Speed", GroupName = "Parameters", Description = "Improve run-time performance (reduces the number of historical chart markers)", Order = 0)]
        public HistoDivergence_OptimizeSpeedSettings OptimizeSpeed { get; set; }

		#region GroupName = "MACDBB Parameters"
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period Bollinger Band", GroupName = "MACDBB Parameters", Description = "Band Period for Bollinger Band", Order = 0)]
        public int BandPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Lookback fast EMA", GroupName = "MACDBB Parameters", Description = "Period for fast EMA", Order = 1)]
        public int Fast { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Lookback slow EMA", GroupName = "MACDBB Parameters", Description = "Period for slow EMA", Order = 2)]
        public int Slow { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Std. dev. multiplier", GroupName = "MACDBB Parameters", Description = "Number of standard deviations", Order = 3)]
        public double StdDevNumber { get; set; }
        #endregion

        #region GroupName = "SwingTrend Parameters"
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Swing strength", GroupName = "SwingTrend Parameters", Description = "Number of bars used to identify a swing high or low", Order = 0)]
        public int SwingStrength { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Deviation multiplier", GroupName = "SwingTrend Parameters", Description = "Multiplier used to calculate minimum deviation as an ATR multiple", Order = 1)]
        public double MultiplierMD { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Sensitivity double tops/bottoms", GroupName = "SwingTrend Parameters", Description = "Fraction of ATR ignored when detecting double tops or bottoms", Order = 2)]
        public double MultiplierDTB { get; set; }
        #endregion

        #region GroupName = "Divergence Options"
        [NinjaScriptProperty]
        [Display(Name = "Select data input for swings", GroupName = "Divergence Options", Description = "Select data input for swing highs and lows", Order = 0)]
        public HistoDivergence_InputType ThisInputType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Adjust divergence", GroupName = "Divergence Options", Description = "Connect divergence line to oscillator peaks and troughs when possible", Order = 1)]
        public bool UseOscHighLow { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Include double tops & bottoms", GroupName = "Divergence Options", Description = "When set to true divergences may be calculated from double tops and bottoms on the price chart", Order = 2)]
        public bool IncludeDoubleTopsAndBottoms { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Reset filter", GroupName = "Divergence Options", Description = "When set to true, a zeroline cross will reset any divergence", Order = 4)]
        public bool ResetFilter { get; set; }
        #endregion

        #region GroupName = "Divergence Parameters"
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Div. max bars back", GroupName = "Divergence Parameters", Description = "Maximum distance in bars allowed for the divergence", Order = 0)]
        public int DivMaxBars { get; set; }

        [NinjaScriptProperty]
        [Range(3, int.MaxValue)]
        [Display(Name = "Div. min bars back", GroupName = "Divergence Parameters", Description = "Minimum distance in bars required for the divergence", Order = 1)]
        public int DivMinBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Max. trigger bars", GroupName = "Divergence Parameters", Description = "Maximum number of bars between divergence and trigger signal", Order = 2)]
        public int TriggerBars { get; set; }
        #endregion

        #region GroupName = "Display Options Oscillator Panel"
        [Display(Name = "Background Flooding", GroupName = "Display Options Oscillator Panel", Description = "Choose the type of background flooding to show", Order = 1)]
        public HistoDivergence_Flooding BackgroundFlooding { get; set; }

        [Display(Name = "Display Histogram Sentiment in box", GroupName = "Display Options Oscillator Panel", Description = "Option to display or remove the Histogram Sentiment in Box", Order = 2)]
        public bool ShowSentimentInBox { get; set; }

        [Display(Name = "Display Structure Bias in box", GroupName = "Display Options Oscillator Panel", Description = "Option to display or remove the Structure Bias in Box", Order = 3)]
        public bool ShowBiasInBox { get; set; }
        #endregion

        #region GroupName = "Display Options Swing Trend"
        [Range(1, double.MaxValue)]
        [Display(Name = "Dotsize swing dots", GroupName = "Display Options Swing Trend", Description = "Dotsize for swing dots representing swing highs and swing lows", Order = 0)]
        public int SwingDotSize { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Fontsize swing labels", GroupName = "Display Options Swing Trend", Description = "Dotsize for swing labels for swing highs and swing lows", Order = 1)]
        public int LabelFontSize { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width swing legs", GroupName = "Display Options Swing Trend", Description = "Select thickness of swing legs connecting swing highs and lows", Order = 2)]
        public int SwingLegWidth { get; set; }

        [Display(Name = "Show swing dots", GroupName = "Display Options Swing Trend", Description = "Show dots for swing highs and lows", Order = 3)]
        public bool ShowZigzagDots { get; set; }

        [Display(Name = "Show swing labels", GroupName = "Display Options Swing Trend", Description = "Show labels for swing highs and lows", Order = 4)]
        public bool ShowZigzagLabels { get; set; }

        [Display(Name = "Show swing legs", GroupName = "Display Options Swing Trend", Description = "Show swing legs connecting swing highs and lows", Order = 5)]
        public bool ShowZigzagLegs { get; set; }

        [Display(Name = "Dash style swing legs", GroupName = "Display Options Swing Trend", Description = "Dash style for swing legs.", Order = 6)]
        public DashStyleHelper SwingLegStyle { get; set; }
        #endregion

        #region GroupName = "Display Options Divergences"
        [Display(Name = "Show divergences on price panel", GroupName = "Display Options Divergences", Description = "Show divergences with trendlines on the price panel", Order = 0)]
        public bool ShowDivOnPricePanel { get; set; }

        [Display(Name = "Show divergences on second panel", GroupName = "Display Options Divergences", Description = "Show divergences with trendlines on the oscillator panel", Order = 1)]
        public bool ShowDivOnOscillatorPanel { get; set; }

        [Display(Name = "Show MACDBB divergences", GroupName = "Display Options Divergences", Description = "Show divergences between price and oscillator", Order = 2)]
        public bool ShowOscillatorDivergences { get; set; }

        [Display(Name = "Show MACDBB Hidden divergences", GroupName = "Display Options Divergences", Description = "Show Hidden divergences between price and oscillator", Order = 3)]
        public bool ShowOscillatorHiddenDivergences { get; set; }

        [Display(Name = "Show histogram divergences", GroupName = "Display Options Divergences", Description = "Show divergences between price and histogram", Order = 4)]
        public bool ShowHistogramDivergences { get; set; }

        [Display(Name = "Show histogram Hidden divergences", GroupName = "Display Options Divergences", Description = "Show Hidden divergences between price and histogram", Order = 5)]
        public bool ShowHistogramHiddenDivergences { get; set; }

        [Display(Name = "Show divergence setup dots", GroupName = "Display Options Divergences", Description = "Show set up dots for potential divergences", Order = 6)]
        public bool ShowSetupDots { get; set; }
		
        [Display(Name = "Show divergence entry price as a square", GroupName = "Display Options Divergences", Description = "Display the entry price for divergence signals", Order = 7)]
		public bool pShowEntryPrice {get;set;}
        #endregion

        //rj RJ5GROUP code Algorithm
        #region GroupName = "Display Options Price Excursions"
        [Display(GroupName = "Display Options Price Excursions", Description = "Display Level 1", Order = 0)]
        public bool DisplayLevel1 { get; set; }

        [Display(GroupName = "Display Options Price Excursions", Description = "Display Level 2", Order = 1)]
        public bool DisplayLevel2 { get; set; }

        [Display(GroupName = "Display Options Price Excursions", Description = "Display Level 3", Order = 2)]
        public bool DisplayLevel3 { get; set; }
        #endregion

        #region GroupName = "Plot Parameters"
        [Range(1, int.MaxValue)]
        [Display(Name = "Dot size MACD", GroupName = "Plot Parameters", Description = "Dotsize for MACD dots", Order = 0)]
        public int DotSize { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width connectors", GroupName = "Plot Parameters", Description = "Width for MACD connectors.", Order = 1)]
        public int Plot2Width { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width average", GroupName = "Plot Parameters", Description = "Width for Average of Bollinger Bands.", Order = 2)]
        public int Plot3Width { get; set; }

        [Display(Name = "Dash style average", GroupName = "Plot Parameters", Description = "DashStyleHelper for Average of Bollinger Bands.", Order = 3)]
        public DashStyleHelper Dash3Style { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width Bollinger Bands", GroupName = "Plot Parameters", Description = "Width for Bollinger Bands.", Order = 4)]
        public int Plot4Width { get; set; }

        [Display(Name = "Dash style Bollinger Bands", GroupName = "Plot Parameters", Description = "DashStyleHelper for Bollinger Bands.", Order = 5)]
        public DashStyleHelper Dash4Style { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width zeroline", GroupName = "Plot Parameters", Description = "Width for Zero Line.", Order = 6)]
        public int ZerolineWidth { get; set; }

        [Display(Name = "Dash style zeroline", GroupName = "Plot Parameters", Description = "DashStyleHelper for Zero Line.", Order = 7)]
        public DashStyleHelper ZerolineStyle { get; set; }

        [Range(0, 100)]
        [Display(Name = "Opacity channel shading", GroupName = "Plot Parameters", Description = "Opacity for shading the area between the Bollinger Bands", Order = 8)]
        public int ChannelOpacity { get; set; }

        [Range(0, 100)]
        [Display(Name = "Opacity background flooding", GroupName = "Plot Parameters", Description = "Opacity used for flooding the chart background", Order = 9)]
        public int BackgroundOpacity { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width histogram bars", GroupName = "Plot Parameters", Description = "Width for the Histogram Momo.", Order = 10)]
        public int MomoWidth { get; set; }
        #endregion

        #region GroupName = "Plot Colors"
        [XmlIgnore]
        [Display(Name = "Rising dots above channel ", GroupName = "Plot Colors", Description = "Select Color", Order = 0)]
        public Brush DotsUpRisingColor { get; set; }
        [Browsable(false)]
        public string DotsUpRisingColorSerialize
        {
            get { return Serialize.BrushToString(DotsUpRisingColor); }
            set { DotsUpRisingColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Rising dots inside/below channel ", GroupName = "Plot Colors", Description = "Select Color", Order = 1)]
        public Brush DotsDownRisingColor { get; set; }
        [Browsable(false)]
        public string DotsDownRisingColorSerialize
        {
            get { return Serialize.BrushToString(DotsDownRisingColor); }
            set { DotsDownRisingColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Falling dots below channel ", GroupName = "Plot Colors", Description = "Select Color", Order = 2)]
        public Brush DotsDownFallingColor { get; set; }
        [Browsable(false)]
        public string DotsDownFallingColorSerialize
        {
            get { return Serialize.BrushToString(DotsDownFallingColor); }
            set { DotsDownFallingColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Falling dots inside/above channel ", GroupName = "Plot Colors", Description = "Select Color", Order = 3)]
        public Brush DotsUpFallingColor { get; set; }
        [Browsable(false)]
        public string DotsUpFallingColorSerialize
        {
            get { return Serialize.BrushToString(DotsUpFallingColor); }
            set { DotsUpFallingColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Dots rim", GroupName = "Plot Colors", Description = "Select Color", Order = 4)]
        public Brush DotsRimColor { get; set; }
        [Browsable(false)]
        public string DotsRimColorSerialize
        {
            get { return Serialize.BrushToString(DotsRimColor); }
            set { DotsRimColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bollinger average", GroupName = "Plot Colors", Description = "Select Color", Order = 5)]
        public Brush BBAverageColor { get; set; }
        [Browsable(false)]
        public string BBAverageColorSerialize
        {
            get { return Serialize.BrushToString(BBAverageColor); }
            set { BBAverageColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bollinger upper band", GroupName = "Plot Colors", Description = "Select Color", Order = 6)]
        public Brush BBUpperColor { get; set; }
        [Browsable(false)]
        public string BBUpperColorSerialize
        {
            get { return Serialize.BrushToString(BBUpperColor); }
            set { BBUpperColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bollinger lower band", GroupName = "Plot Colors", Description = "Select Color", Order = 7)]
        public Brush BBLowerColor { get; set; }
        [Browsable(false)]
        public string BBLowerColorSerialize
        {
            get { return Serialize.BrushToString(BBLowerColor); }
            set { BBLowerColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Momo Histogram Hi Color", GroupName = "Plot Colors", Description = "Select Color", Order = 8)]
        public Brush HistUpColor { get; set; }
        [Browsable(false)]
        public string HistUpColorSerialize
        {
            get { return Serialize.BrushToString(HistUpColor); }
            set { HistUpColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Momo Histogram Down Color", GroupName = "Plot Colors", Description = "Select Color", Order = 9)]
        public Brush HistDownColor { get; set; }
        [Browsable(false)]
        public string HistDownColorSerialize
        {
            get { return Serialize.BrushToString(HistDownColor); }
            set { HistDownColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Zeroline", GroupName = "Plot Colors", Description = "Select Color", Order = 10)]
        public Brush ZerolineColor { get; set; }
        [Browsable(false)]
        public string ZerolineColorSerialize
        {
            get { return Serialize.BrushToString(ZerolineColor); }
            set { ZerolineColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Connector", GroupName = "Plot Colors", Description = "Select Color", Order = 11)]
        public Brush ConnectorColor { get; set; }
        [Browsable(false)]
        public string ConnectorColorSerialize
        {
            get { return Serialize.BrushToString(ConnectorColor); }
            set { ConnectorColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Channel shading", GroupName = "Plot Colors", Description = "Select Color", Order = 12)]
        public Brush ChannelColor { get; set; }
        [Browsable(false)]
        public string ChannelColorSerialize
        {
            get { return Serialize.BrushToString(ChannelColor); }
            set { ChannelColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Deep Bearish background flooding", GroupName = "Plot Colors", Description = "Select Color", Order = 17)]
        public Brush DeepBearishBackgroundColor { get; set; }
        [Browsable(false)]
        public string DeepBearishBackgroundColorSerialize
        {
            get { return Serialize.BrushToString(DeepBearishBackgroundColor); }
            set { DeepBearishBackgroundColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bearish background flooding", GroupName = "Plot Colors", Description = "Select Color", Order = 16)]
        public Brush BearishBackgroundColor { get; set; }
        [Browsable(false)]
        public string BearishBackgroundColorSerialize
        {
            get { return Serialize.BrushToString(BearishBackgroundColor); }
            set { BearishBackgroundColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Opposite background flooding", GroupName = "Plot Colors", Description = "Select Color", Order = 15)]
        public Brush OppositeBackgroundColor { get; set; }
        [Browsable(false)]
        public string OppositeBackgroundColorSerialize
        {
            get { return Serialize.BrushToString(OppositeBackgroundColor); }
            set { OppositeBackgroundColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bullish background flooding", GroupName = "Plot Colors", Description = "Select Color", Order = 14)]
        public Brush BullishBackgroundColor { get; set; }
        [Browsable(false)]
        public string BullishBackgroundColorSerialize
        {
            get { return Serialize.BrushToString(BullishBackgroundColor); }
            set { BullishBackgroundColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Deep Bullish background flooding", GroupName = "Plot Colors", Description = "Select Color", Order = 13)]
        public Brush DeepBullishBackgroundColor { get; set; }
        [Browsable(false)]
        public string DeepBullishBackgroundColorSerialize
        {
            get { return Serialize.BrushToString(DeepBullishBackgroundColor); }
            set { DeepBullishBackgroundColor = Serialize.StringToBrush(value); }
        }
        #endregion

        //rj RJ5GROUP code Algorithm
        #region Category("Price Excursion - Plot Colors")
        [XmlIgnore]
        [Display(Name = "Color level 1 ", GroupName = "Price Excursion - Plot Colors", Description = "Select Color for Level 1 lines", Order = 0)]
        public Brush Level1Color { get; set; }
        [Browsable(false)]
        public string Level1ColorSerialize
        {
            get { return Serialize.BrushToString(Level1Color); }
            set { Level1Color = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Color level 2 ", GroupName = "Price Excursion - Plot Colors", Description = "Select Color for Level 2 lines", Order = 1)]
        public Brush Level2Color { get; set; }
        [Browsable(false)]
        public string Level2ColorSerialize
        {
            get { return Serialize.BrushToString(Level2Color); }
            set { Level2Color = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Color level 3 ", GroupName = "Price Excursion - Plot Colors", Description = "Select Color for Level 3 lines", Order = 2)]
        public Brush Level3Color { get; set; }
        [Browsable(false)]
        public string Level3ColorSerialize
        {
            get { return Serialize.BrushToString(Level3Color); }
            set { Level3Color = Serialize.StringToBrush(value); }
        }
        #endregion

        //rj RJ5GROUP code Algorithm
        #region Category("Price Excursion - Plot Colors")
        [Display(Name = "Plot style for Level Lines", GroupName = "Price Excursion - Plot Parameters", Description = "PlotStyle for Level Lines", Order = 0)]
        public PlotStyle PlotStyleLevels { get; set; }

        [Display(Name = "Dash style for Level Lines", GroupName = "Price Excursion - Plot Parameters", Description = "DashStyleHelper for level lines", Order = 1)]
        public DashStyleHelper DashStyleHelperLevels { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Width for level lines", GroupName = "Price Excursion - Plot Parameters", Description = "Width for level lines", Order = 2)]
        public int PlotWidthLevels { get; set; }
        #endregion

        #region Category("MACD Divergences - Plot Parameters")
        [Range(1, int.MaxValue)]
        [Display(Name = "Width divergence lines", GroupName = "MACD Divergences - Plot Parameters", Description = "Select thickness for MACD divergence lines on both price panel and oscillator panel", Order = 0)]
        public int DivWidth1 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Divergences offset", GroupName = "MACD Divergences - Plot Parameters", Description = "ATR percentage which is used to set the distance between divergence lines and price bars", Order = 1)]
        public double OffsetMultiplier3 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Drawing objects offset", GroupName = "MACD Divergences - Plot Parameters", Description = "ATR percentage which is used to set the distance between setups/triggers drawing objects and price bars", Order = 2)]
        public double OffsetMultiplier1 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Triggers object size", GroupName = "MACD Divergences - Plot Parameters", Description = "Select the size for the triggers drawing object", Order = 3)]
        public int TriangleFontSize1 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Setups dot size", GroupName = "MACD Divergences - Plot Parameters", Description = "Select the size for the setups drawing object", Order = 4)]
        public int SetupFontSize1 { get; set; }
        #endregion

        #region Category("MACD Hidden Divergences - Plot Parameters")
        [Range(1, int.MaxValue)]
        [Display(Name = "Width Hidden divergence lines", GroupName = "MACD Hidden Divergences - Plot Parameters", Description = "Select thickness for MACD Hidden divergence lines on both price panel and oscillator panel", Order = 0)]
        public int HiddenDivWidth1 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Hidden Divergences offset", GroupName = "MACD Hidden Divergences - Plot Parameters", Description = "ATR percentage which is used to set the distance between Hidden divergence lines and price bars", Order = 1)]
        public double HiddenOffsetMultiplier3 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Drawing objects offset", GroupName = "MACD Hidden Divergences - Plot Parameters", Description = "ATR percentage which is used to set the distance between setups/triggers drawing objects and price bars", Order = 2)]
        public double HiddenOffsetMultiplier1 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Triggers object size", GroupName = "MACD Hidden Divergences - Plot Parameters", Description = "Select the size for the triggers drawing object", Order = 3)]
        public int HiddenTriangleFontSize1 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Setups dot size", GroupName = "MACD Hidden Divergences - Plot Parameters", Description = "Select the size for the setups drawing object", Order = 4)]
        public int HiddenSetupFontSize1 { get; set; }
        #endregion

        #region Category("Histogram Divergences - Plot Parameters")
        [Range(1, int.MaxValue)]
        [Display(Name = "Width divergence lines", GroupName = "Histogram Divergences - Plot Parameters", Description = "Select thickness for histogram divergence lines on both price panel and oscillator panel", Order = 0)]
        public int DivWidth2 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Divergences offset", GroupName = "Histogram Divergences - Plot Parameters", Description = "ATR percentage which is used to set the distance between divergence lines and price bars", Order = 1)]
        public double OffsetMultiplier4 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Drawing objects offset", GroupName = "Histogram Divergences - Plot Parameters", Description = "ATR precentage which is used to set the distance between setups/triggers drawing objects and price bars", Order = 2)]
        public double OffsetMultiplier2 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Triggers object size", GroupName = "Histogram Divergences - Plot Parameters", Description = "Select the size for the triggers drawing object", Order = 3)]
        public int TriangleFontSize2 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Setups dot size", GroupName = "Histogram Divergences - Plot Parameters", Description = "Select the size for the setups drawing object", Order = 4)]
        public int SetupFontSize2 { get; set; }
        #endregion

        #region Category("Histogram Hidden Divergences - Plot Parameters")
        [Range(1, int.MaxValue)]
        [Display(Name = "Width Hidden divergence lines", GroupName = "Histogram Hidden Divergences - Plot Parameters", Description = "Select thickness for histogram Hidden divergence lines on both price panel and oscillator panel", Order = 0)]
        public int HiddenDivWidth2 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Hidden Divergences offset", GroupName = "Histogram Hidden Divergences - Plot Parameters", Description = "ATR percentage which is used to set the distance between Hidden divergence lines and price bars", Order = 1)]
        public double HiddenOffsetMultiplier4 { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Drawing objects offset", GroupName = "Histogram Hidden Divergences - Plot Parameters", Description = "ATR precentage which is used to set the distance between setups/triggers drawing objects and price bars", Order = 2)]
        public double HiddenOffsetMultiplier2 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Triggers object size", GroupName = "Histogram Hidden Divergences - Plot Parameters", Description = "Select the size for the triggers drawing object", Order = 3)]
        public int HiddenTriangleFontSize2 { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Setups dot size", GroupName = "Histogram Hidden Divergences - Plot Parameters", Description = "Select the size for the setups drawing object", Order = 4)]
        public int HiddenSetupFontSize2 { get; set; }
        #endregion

        #region Category("MACD Divergences - Plot Colors")
        [XmlIgnore]
        [Display(Name = "Setups bearish trendlines", GroupName = "MACD Divergences - Plot Colors", Description = "Select color for bearish divergence lines", Order = 0)]
        public Brush BearColor1 { get; set; }
        [Browsable(false)]
        public string BearColor1Serialize
        {
            get { return Serialize.BrushToString(BearColor1); }
            set { BearColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish trendlines", GroupName = "MACD Divergences - Plot Colors", Description = "Select color for bullish divergence lines", Order = 1)]
        public Brush BullColor1 { get; set; }
        [Browsable(false)]
        public string BullColor1Serialize
        {
            get { return Serialize.BrushToString(BullColor1); }
            set { BullColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bearish objects", GroupName = "MACD Divergences - Plot Colors", Description = "Select color for bearish triggers objects", Order = 2)]
        public Brush ArrowDownColor1 { get; set; }
        [Browsable(false)]
        public string ArrowDownColor1Serialize
        {
            get { return Serialize.BrushToString(ArrowDownColor1); }
            set { ArrowDownColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bullish objects", GroupName = "MACD Divergences - Plot Colors", Description = "Select color for bullish triggers objects", Order = 3)]
        public Brush ArrowUpColor1 { get; set; }
        [Browsable(false)]
        public string ArrowUpColor1Serialize
        {
            get { return Serialize.BrushToString(ArrowUpColor1); }
            set { ArrowUpColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bearish dots", GroupName = "MACD Divergences - Plot Colors", Description = "Select color for bearish setups dots", Order = 4)]
        public Brush BearishSetupColor1 { get; set; }
        [Browsable(false)]
        public string BearishSetupColor1Serialize
        {
            get { return Serialize.BrushToString(BearishSetupColor1); }
            set { BearishSetupColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish dots", GroupName = "MACD Divergences - Plot Colors", Description = "Select color for bullish setups dots", Order = 5)]
        public Brush BullishSetupColor1 { get; set; }
        [Browsable(false)]
        public string BullishSetupColor1Serialize
        {
            get { return Serialize.BrushToString(BullishSetupColor1); }
            set { BullishSetupColor1 = Serialize.StringToBrush(value); }
        }
        #endregion

        #region Category("MACD Hidden Divergences - Plot Colors")
        [XmlIgnore]
        [Display(Name = "Setups bearish trendlines", GroupName = "MACD Hidden Divergences - Plot Colors", Description = "Select color for bearish Hidden divergence lines", Order = 0)]
        public Brush HiddenBearColor1 { get; set; }
        [Browsable(false)]
        public string HiddenBearColor1Serialize
        {
            get { return Serialize.BrushToString(HiddenBearColor1); }
            set { HiddenBearColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish trendlines", GroupName = "MACD Hidden Divergences - Plot Colors", Description = "Select color for bullish Hidden divergence lines", Order = 1)]
        public Brush HiddenBullColor1 { get; set; }
        [Browsable(false)]
        public string HiddenBullColor1Serialize
        {
            get { return Serialize.BrushToString(HiddenBullColor1); }
            set { HiddenBullColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bearish objects", GroupName = "MACD Hidden Divergences - Plot Colors", Description = "Select color for bearish triggers objects", Order = 2)]
        public Brush HiddenArrowDownColor1 { get; set; }
        [Browsable(false)]
        public string HiddenArrowDownColor1Serialize
        {
            get { return Serialize.BrushToString(HiddenArrowDownColor1); }
            set { HiddenArrowDownColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bullish objects", GroupName = "MACD Hidden Divergences - Plot Colors", Description = "Select color for bullish triggers objects", Order = 3)]
        public Brush HiddenArrowUpColor1 { get; set; }
        [Browsable(false)]
        public string HiddenArrowUpColor1Serialize
        {
            get { return Serialize.BrushToString(HiddenArrowUpColor1); }
            set { HiddenArrowUpColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bearish dots", GroupName = "MACD Hidden Divergences - Plot Colors", Description = "Select color for bearish setups dots", Order = 4)]
        public Brush HiddenBearishSetupColor1 { get; set; }
        [Browsable(false)]
        public string HiddenBearishSetupColor1Serialize
        {
            get { return Serialize.BrushToString(HiddenBearishSetupColor1); }
            set { HiddenBearishSetupColor1 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish dots", GroupName = "MACD Hidden Divergences - Plot Colors", Description = "Select color for bullish setups dots", Order = 5)]
        public Brush HiddenBullishSetupColor1 { get; set; }
        [Browsable(false)]
        public string HiddenBullishSetupColor1Serialize
        {
            get { return Serialize.BrushToString(HiddenBullishSetupColor1); }
            set { HiddenBullishSetupColor1 = Serialize.StringToBrush(value); }
        }
        #endregion

		#region Category("Histogram Divergences - Sounds")
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        [Display(Name = "Buy WAV RegularDiv", GroupName = "Histogram Divergences - Sounds", Description = "Regular Divergence WAV on a buy", Order = 10)]
        public string pBuySignalRDivWAV { get; set; }
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        [Display(Name = "Sell WAV RegularDiv", GroupName = "Histogram Divergences - Sounds", Description = "Regular Divergence WAV on a sell", Order = 20)]
        public string pSellSignalRDivWAV { get; set; }
		
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        [Display(Name = "Buy WAV HiddenDiv", GroupName = "Histogram Divergences - Sounds", Description = "Hidden Divergence WAV on a buy", Order = 30)]
		public string pBuySignalHDivWAV {get;set;}
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(LoadSoundFileList))]
        [Display(Name = "Sell WAV HiddenDiv", GroupName = "Histogram Divergences - Sounds", Description = "Hidden Divergence WAV on a sell", Order = 40)]
		public string pSellSignalHDivWAV {get;set;}
		#endregion

		#region Category("Histogram Divergences - Plot Colors")
        [XmlIgnore]
        [Display(Name = "Setups bearish trendlines", GroupName = "Histogram Divergences - Plot Colors", Description = "Select color for bearish divergence lines", Order = 0)]
        public Brush BearColor2 { get; set; }
        [Browsable(false)]
        public string BearColor2Serialize
        {
            get { return Serialize.BrushToString(BearColor2); }
            set { BearColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish trendlines", GroupName = "Histogram Divergences - Plot Colors", Description = "Select color for bullish divergence lines", Order = 1)]
        public Brush BullColor2 { get; set; }
        [Browsable(false)]
        public string BullColor2Serialize
        {
            get { return Serialize.BrushToString(BullColor2); }
            set { BullColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bearish objects", GroupName = "Histogram Divergences - Plot Colors", Description = "Select color for bearish triggers objects", Order = 2)]
        public Brush ArrowDownColor2 { get; set; }
        [Browsable(false)]
        public string ArrowDownColor2Serialize
        {
            get { return Serialize.BrushToString(ArrowDownColor2); }
            set { ArrowDownColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bullish objects", GroupName = "Histogram Divergences - Plot Colors", Description = "Select color for bullish triggers objects", Order = 3)]
        public Brush ArrowUpColor2 { get; set; }
        [Browsable(false)]
        public string ArrowUpColor2Serialize
        {
            get { return Serialize.BrushToString(ArrowUpColor2); }
            set { ArrowUpColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bearish dots", GroupName = "Histogram Divergences - Plot Colors", Description = "Select color for bearish setups dots", Order = 4)]
        public Brush BearishSetupColor2 { get; set; }
        [Browsable(false)]
        public string BearishSetupColor2Serialize
        {
            get { return Serialize.BrushToString(BearishSetupColor2); }
            set { BearishSetupColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish dots", GroupName = "Histogram Divergences - Plot Colors", Description = "Select color for bullish setups dots", Order = 5)]
        public Brush BullishSetupColor2 { get; set; }
        [Browsable(false)]
        public string BullishSetupColor2Serialize
        {
            get { return Serialize.BrushToString(BullishSetupColor2); }
            set { BullishSetupColor2 = Serialize.StringToBrush(value); }
        }
        #endregion

        #region Category("Histogram Hidden Divergences - Plot Colors")
        [XmlIgnore]
        [Display(Name = "Setups bearish trendlines", GroupName = "Histogram Hidden Divergences - Plot Colors", Description = "Select color for bearish Hidden divergence lines", Order = 0)]
        public Brush HiddenBearColor2 { get; set; }
        [Browsable(false)]
        public string HiddenBearColor2Serialize
        {
            get { return Serialize.BrushToString(HiddenBearColor2); }
            set { HiddenBearColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish trendlines", GroupName = "Histogram Hidden Divergences - Plot Colors", Description = "Select color for bullish Hidden divergence lines", Order = 1)]
        public Brush HiddenBullColor2 { get; set; }
        [Browsable(false)]
        public string HiddenBullColor2Serialize
        {
            get { return Serialize.BrushToString(HiddenBullColor2); }
            set { HiddenBullColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bearish objects", GroupName = "Histogram Hidden Divergences - Plot Colors", Description = "Select color for bearish triggers objects", Order = 2)]
        public Brush HiddenArrowDownColor2 { get; set; }
        [Browsable(false)]
        public string HiddenArrowDownColor2Serialize
        {
            get { return Serialize.BrushToString(HiddenArrowDownColor2); }
            set { HiddenArrowDownColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Triggers bullish objects", GroupName = "Histogram Hidden Divergences - Plot Colors", Description = "Select color for bullish triggers objects", Order = 3)]
        public Brush HiddenArrowUpColor2 { get; set; }
        [Browsable(false)]
        public string HiddenArrowUpColor2Serialize
        {
            get { return Serialize.BrushToString(HiddenArrowUpColor2); }
            set { HiddenArrowUpColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bearish dots", GroupName = "Histogram Hidden Divergences - Plot Colors", Description = "Select color for bearish setups dots", Order = 4)]
        public Brush HiddenBearishSetupColor2 { get; set; }
        [Browsable(false)]
        public string HiddenBearishSetupColor2Serialize
        {
            get { return Serialize.BrushToString(HiddenBearishSetupColor2); }
            set { HiddenBearishSetupColor2 = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Setups bullish dots", GroupName = "Histogram Hidden Divergences - Plot Colors", Description = "Select color for bullish setups dots", Order = 5)]
        public Brush HiddenBullishSetupColor2 { get; set; }
        [Browsable(false)]
        public string HiddenBullishSetupColor2Serialize
        {
            get { return Serialize.BrushToString(HiddenBullishSetupColor2); }
            set { HiddenBullishSetupColor2 = Serialize.StringToBrush(value); }
        }
        #endregion

		//JQ 11.26.2017
		// Bug 12782. The set right margin is causing the shifting of the SDV bar when the PP bar and the Divergence indicator
        // are all on the same chart. This could be a NT8 issue as it appears the BarMarginRight is only applied to the
		// primary bar (PP) rather than the secondary bar SDV). For now, I will disable the setrightmargin properties
		// until I have a chance to talk to NT about this issue.
		// --start--

		[Description("Button text - enter how you want the UI button to be labeled")]
		[Display(Order = 10, Name = "Button Txt",  GroupName = "Visuals", ResourceType = typeof(Custom.Resource))]
		public string pButtonText {get;set;}
		/*
        * JQ 11.12.2017
        * Added indicator version number on the property window.
        * */
        //---start--
        [Display(Name = "Indicator Version", GroupName = "Indicator Version", Description = "Indicator Version", Order = 0)]
        public string indicatorVersion { get { return VERSION; } }
        //--end--
        #endregion
    }
}
public enum HistoDivergence_InputType { High_Low, Close, Median, Typical }
public enum HistoDivergence_Flooding { None, Histogram, Structure, Both }
public enum HistoDivergence_ExcursionStyle { Static, Dynamic }
public enum HistoDivergence_OptimizeSpeedSettings {Max,Min,None}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HistoDivergence[] cacheHistoDivergence;
		public HistoDivergence HistoDivergence(HistoDivergence_OptimizeSpeedSettings optimizeSpeed, int bandPeriod, int fast, int slow, double stdDevNumber, int swingStrength, double multiplierMD, double multiplierDTB, HistoDivergence_InputType thisInputType, bool useOscHighLow, bool includeDoubleTopsAndBottoms, bool resetFilter, int divMaxBars, int divMinBars, int triggerBars)
		{
			return HistoDivergence(Input, optimizeSpeed, bandPeriod, fast, slow, stdDevNumber, swingStrength, multiplierMD, multiplierDTB, thisInputType, useOscHighLow, includeDoubleTopsAndBottoms, resetFilter, divMaxBars, divMinBars, triggerBars);
		}

		public HistoDivergence HistoDivergence(ISeries<double> input, HistoDivergence_OptimizeSpeedSettings optimizeSpeed, int bandPeriod, int fast, int slow, double stdDevNumber, int swingStrength, double multiplierMD, double multiplierDTB, HistoDivergence_InputType thisInputType, bool useOscHighLow, bool includeDoubleTopsAndBottoms, bool resetFilter, int divMaxBars, int divMinBars, int triggerBars)
		{
			if (cacheHistoDivergence != null)
				for (int idx = 0; idx < cacheHistoDivergence.Length; idx++)
					if (cacheHistoDivergence[idx] != null && cacheHistoDivergence[idx].OptimizeSpeed == optimizeSpeed && cacheHistoDivergence[idx].BandPeriod == bandPeriod && cacheHistoDivergence[idx].Fast == fast && cacheHistoDivergence[idx].Slow == slow && cacheHistoDivergence[idx].StdDevNumber == stdDevNumber && cacheHistoDivergence[idx].SwingStrength == swingStrength && cacheHistoDivergence[idx].MultiplierMD == multiplierMD && cacheHistoDivergence[idx].MultiplierDTB == multiplierDTB && cacheHistoDivergence[idx].ThisInputType == thisInputType && cacheHistoDivergence[idx].UseOscHighLow == useOscHighLow && cacheHistoDivergence[idx].IncludeDoubleTopsAndBottoms == includeDoubleTopsAndBottoms && cacheHistoDivergence[idx].ResetFilter == resetFilter && cacheHistoDivergence[idx].DivMaxBars == divMaxBars && cacheHistoDivergence[idx].DivMinBars == divMinBars && cacheHistoDivergence[idx].TriggerBars == triggerBars && cacheHistoDivergence[idx].EqualsInput(input))
						return cacheHistoDivergence[idx];
			return CacheIndicator<HistoDivergence>(new HistoDivergence(){ OptimizeSpeed = optimizeSpeed, BandPeriod = bandPeriod, Fast = fast, Slow = slow, StdDevNumber = stdDevNumber, SwingStrength = swingStrength, MultiplierMD = multiplierMD, MultiplierDTB = multiplierDTB, ThisInputType = thisInputType, UseOscHighLow = useOscHighLow, IncludeDoubleTopsAndBottoms = includeDoubleTopsAndBottoms, ResetFilter = resetFilter, DivMaxBars = divMaxBars, DivMinBars = divMinBars, TriggerBars = triggerBars }, input, ref cacheHistoDivergence);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HistoDivergence HistoDivergence(HistoDivergence_OptimizeSpeedSettings optimizeSpeed, int bandPeriod, int fast, int slow, double stdDevNumber, int swingStrength, double multiplierMD, double multiplierDTB, HistoDivergence_InputType thisInputType, bool useOscHighLow, bool includeDoubleTopsAndBottoms, bool resetFilter, int divMaxBars, int divMinBars, int triggerBars)
		{
			return indicator.HistoDivergence(Input, optimizeSpeed, bandPeriod, fast, slow, stdDevNumber, swingStrength, multiplierMD, multiplierDTB, thisInputType, useOscHighLow, includeDoubleTopsAndBottoms, resetFilter, divMaxBars, divMinBars, triggerBars);
		}

		public Indicators.HistoDivergence HistoDivergence(ISeries<double> input , HistoDivergence_OptimizeSpeedSettings optimizeSpeed, int bandPeriod, int fast, int slow, double stdDevNumber, int swingStrength, double multiplierMD, double multiplierDTB, HistoDivergence_InputType thisInputType, bool useOscHighLow, bool includeDoubleTopsAndBottoms, bool resetFilter, int divMaxBars, int divMinBars, int triggerBars)
		{
			return indicator.HistoDivergence(input, optimizeSpeed, bandPeriod, fast, slow, stdDevNumber, swingStrength, multiplierMD, multiplierDTB, thisInputType, useOscHighLow, includeDoubleTopsAndBottoms, resetFilter, divMaxBars, divMinBars, triggerBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HistoDivergence HistoDivergence(HistoDivergence_OptimizeSpeedSettings optimizeSpeed, int bandPeriod, int fast, int slow, double stdDevNumber, int swingStrength, double multiplierMD, double multiplierDTB, HistoDivergence_InputType thisInputType, bool useOscHighLow, bool includeDoubleTopsAndBottoms, bool resetFilter, int divMaxBars, int divMinBars, int triggerBars)
		{
			return indicator.HistoDivergence(Input, optimizeSpeed, bandPeriod, fast, slow, stdDevNumber, swingStrength, multiplierMD, multiplierDTB, thisInputType, useOscHighLow, includeDoubleTopsAndBottoms, resetFilter, divMaxBars, divMinBars, triggerBars);
		}

		public Indicators.HistoDivergence HistoDivergence(ISeries<double> input , HistoDivergence_OptimizeSpeedSettings optimizeSpeed, int bandPeriod, int fast, int slow, double stdDevNumber, int swingStrength, double multiplierMD, double multiplierDTB, HistoDivergence_InputType thisInputType, bool useOscHighLow, bool includeDoubleTopsAndBottoms, bool resetFilter, int divMaxBars, int divMinBars, int triggerBars)
		{
			return indicator.HistoDivergence(input, optimizeSpeed, bandPeriod, fast, slow, stdDevNumber, swingStrength, multiplierMD, multiplierDTB, thisInputType, useOscHighLow, includeDoubleTopsAndBottoms, resetFilter, divMaxBars, divMinBars, triggerBars);
		}
	}
}

#endregion
