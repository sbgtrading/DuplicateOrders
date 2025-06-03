//#define CHECKAUTHORIZATION
#region Using declarations
using System;
//using System.Drawing;
//using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
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

namespace NinjaTrader.NinjaScript.Indicators
{

	[Description("Fan Multicolor Moving Average")]
	public class IW_FanMA : Indicator
	{
		private bool LicenseValid = true;
		EMA se1, se2, se3, se4, se5, se6;
		HMA sh1, sh2, sh3, sh4, sh5, sh6;
		SMA ss1, ss2, ss3, ss4, ss5, ss6;
		WMA sw1, sw2, sw3, sw4, sw5, sw6;
		VWMA svw1, svw2, svw3, svw4, svw5, svw6;
		TEMA ste1, ste2, ste3, ste4, ste5, ste6;
		LinReg slr1, slr2, slr3, slr4, slr5, slr6;
		ZLEMA sz1, sz2, sz3, sz4, sz5, sz6;
		EMA le1, le2, le3, le4, le5, le6;
		HMA lh1, lh2, lh3, lh4, lh5, lh6;
		SMA ls1, ls2, ls3, ls4, ls5, ls6;
		WMA lw1, lw2, lw3, lw4, lw5, lw6;
		VWMA lvw1, lvw2, lvw3, lvw4, lvw5, lvw6;
		TEMA lte1, lte2, lte3, lte4, lte5, lte6;
		LinReg llr1, llr2, llr3, llr4, llr5, llr6;
		ZLEMA lz1, lz2,lz3, lz4, lz5, lz6;
		bool RunInit = true;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name="iw Fan MA";
				string ExemptMachine1 = "CB15E08BE30BC80628CFF6010471FA2A";
				string ExemptMachine2 = "B0D2E9D1C802E279D3678D7DE6A33CE4";
				bool ExemptMachine = NinjaTrader.Cbi.License.MachineId==ExemptMachine1 || NinjaTrader.Cbi.License.MachineId==ExemptMachine2;
				bool IsBen = System.IO.File.Exists("c:\\222222222222.txt") && ExemptMachine;
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "IWfree7", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");
//				VendorLicense("IndicatorWarehouse", "IWFMMA", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				#region Plots
				//-------------------------------------------------------------------------------------
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "shortMA1");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "shortMA2");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "shortMA3");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "shortMA4");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "shortMA5");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "shortMA6");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "longMA1");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "longMA2");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "longMA3");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "longMA4");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "longMA5");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "longMA6");
				#endregion

				IsOverlay=true;
				//PriceTypeSupported	= true;
				ArePlotsConfigurable=true;
	
				DisplayInDataBox    = true; 
				Calculate=Calculate.OnBarClose;
				PaintPriceMarkers   = false; 
			}
			if(State == State.Configure){
				if(pMATypeST == FMMA_type.EMA) {
					se1 = EMA(pSTperiod1);
					se2 = EMA(pSTperiod2);
					se3 = EMA(pSTperiod3);
					se4 = EMA(pSTperiod4);
					se5 = EMA(pSTperiod5);
					se6 = EMA(pSTperiod6);
				}else if(pMATypeST == FMMA_type.HMA) {
					sh1 = HMA(pSTperiod1);
					sh2 = HMA(pSTperiod2);
					sh3 = HMA(pSTperiod3);
					sh4 = HMA(pSTperiod4);
					sh5 = HMA(pSTperiod5);
					sh6 = HMA(pSTperiod6);
				}else if(pMATypeST == FMMA_type.SMA) {
					ss1 = SMA(pSTperiod1);
					ss2 = SMA(pSTperiod2);
					ss3 = SMA(pSTperiod3);
					ss4 = SMA(pSTperiod4);
					ss5 = SMA(pSTperiod5);
					ss6 = SMA(pSTperiod6);
				}else if(pMATypeST == FMMA_type.WMA) {
					sw1 = WMA(pSTperiod1);
					sw2 = WMA(pSTperiod2);
					sw3 = WMA(pSTperiod3);
					sw4 = WMA(pSTperiod4);
					sw5 = WMA(pSTperiod5);
					sw6 = WMA(pSTperiod6);
				}else if(pMATypeST == FMMA_type.TEMA) {
					ste1 = TEMA(pSTperiod1);
					ste2 = TEMA(pSTperiod2);
					ste3 = TEMA(pSTperiod3);
					ste4 = TEMA(pSTperiod4);
					ste5 = TEMA(pSTperiod5);
					ste6 = TEMA(pSTperiod6);
				}else if(pMATypeST == FMMA_type.VWMA) {
					svw1 = VWMA(pSTperiod1);
					svw2 = VWMA(pSTperiod2);
					svw3 = VWMA(pSTperiod3);
					svw4 = VWMA(pSTperiod4);
					svw5 = VWMA(pSTperiod5);
					svw6 = VWMA(pSTperiod6);
				}else if(pMATypeST == FMMA_type.LinReg) {
					slr1 = LinReg(pSTperiod1);
					slr2 = LinReg(pSTperiod2);
					slr3 = LinReg(pSTperiod3);
					slr4 = LinReg(pSTperiod4);
					slr5 = LinReg(pSTperiod5);
					slr6 = LinReg(pSTperiod6);
				}else if(pMATypeST == FMMA_type.ZLEMA) {
					sz1 = ZLEMA(pSTperiod1);
					sz2 = ZLEMA(pSTperiod2);
					sz3 = ZLEMA(pSTperiod3);
					sz4 = ZLEMA(pSTperiod4);
					sz5 = ZLEMA(pSTperiod5);
					sz6 = ZLEMA(pSTperiod6);
				}
				//--
				if(pMATypeLT == FMMA_type.EMA) {
					le1 = EMA(pLTperiod1);
					le2 = EMA(pLTperiod2);
					le3 = EMA(pLTperiod3);
					le4 = EMA(pLTperiod4);
					le5 = EMA(pLTperiod5);
					le6 = EMA(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.HMA) {
					lh1 = HMA(pLTperiod1);
					lh2 = HMA(pLTperiod2);
					lh3 = HMA(pLTperiod3);
					lh4 = HMA(pLTperiod4);
					lh5 = HMA(pLTperiod5);
					lh6 = HMA(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.SMA) {
					ls1 = SMA(pLTperiod1);
					ls2 = SMA(pLTperiod2);
					ls3 = SMA(pLTperiod3);
					ls4 = SMA(pLTperiod4);
					ls5 = SMA(pLTperiod5);
					ls6 = SMA(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.WMA) {
					lw1 = WMA(pLTperiod1);
					lw2 = WMA(pLTperiod2);
					lw3 = WMA(pLTperiod3);
					lw4 = WMA(pLTperiod4);
					lw5 = WMA(pLTperiod5);
					lw6 = WMA(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.TEMA) {
					lte1 = TEMA(pLTperiod1);
					lte2 = TEMA(pLTperiod2);
					lte3 = TEMA(pLTperiod3);
					lte4 = TEMA(pLTperiod4);
					lte5 = TEMA(pLTperiod5);
					lte6 = TEMA(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.VWMA) {
					lvw1 = VWMA(pLTperiod1);
					lvw2 = VWMA(pLTperiod2);
					lvw3 = VWMA(pLTperiod3);
					lvw4 = VWMA(pLTperiod4);
					lvw5 = VWMA(pLTperiod5);
					lvw6 = VWMA(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.LinReg) {
					llr1 = LinReg(pLTperiod1);
					llr2 = LinReg(pLTperiod2);
					llr3 = LinReg(pLTperiod3);
					llr4 = LinReg(pLTperiod4);
					llr5 = LinReg(pLTperiod5);
					llr6 = LinReg(pLTperiod6);
				}else if(pMATypeLT == FMMA_type.ZLEMA) {
					lz1 = ZLEMA(pLTperiod1);
					lz2 = ZLEMA(pLTperiod2);
					lz3 = ZLEMA(pLTperiod3);
					lz4 = ZLEMA(pLTperiod4);
					lz5 = ZLEMA(pLTperiod5);
					lz6 = ZLEMA(pLTperiod6);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if(!LicenseValid) return;

			if(pMATypeST == FMMA_type.EMA) {
				#region Init dataseries
				ShortMA1[0]=(se1[0]);
				ShortMA2[0]=(se2[0]);
				ShortMA3[0]=(se3[0]);
				ShortMA4[0]=(se4[0]);
				ShortMA5[0]=(se5[0]);
				ShortMA6[0]=(se6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.HMA) {
				#region Init dataseries
				ShortMA1[0]=(sh1[0]);
				ShortMA2[0]=(sh2[0]);
				ShortMA3[0]=(sh3[0]);
				ShortMA4[0]=(sh4[0]);
				ShortMA5[0]=(sh5[0]);
				ShortMA6[0]=(sh6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.SMA) {
				#region Init dataseries
				ShortMA1[0]=(ss1[0]);
				ShortMA2[0]=(ss2[0]);
				ShortMA3[0]=(ss3[0]);
				ShortMA4[0]=(ss4[0]);
				ShortMA5[0]=(ss5[0]);
				ShortMA6[0]=(ss6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.WMA) {
				#region Init dataseries
				ShortMA1[0]=(sw1[0]);
				ShortMA2[0]=(sw2[0]);
				ShortMA3[0]=(sw3[0]);
				ShortMA4[0]=(sw4[0]);
				ShortMA5[0]=(sw5[0]);
				ShortMA6[0]=(sw6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.TEMA) {
				#region Init dataseries
				ShortMA1[0]=(ste1[0]);
				ShortMA2[0]=(ste2[0]);
				ShortMA3[0]=(ste3[0]);
				ShortMA4[0]=(ste4[0]);
				ShortMA5[0]=(ste5[0]);
				ShortMA6[0]=(ste6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.VWMA) {
				#region Init dataseries
				ShortMA1[0]=(svw1[0]);
				ShortMA2[0]=(svw2[0]);
				ShortMA3[0]=(svw3[0]);
				ShortMA4[0]=(svw4[0]);
				ShortMA5[0]=(svw5[0]);
				ShortMA6[0]=(svw6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.LinReg) {
				#region Init dataseries
				ShortMA1[0]=(slr1[0]);
				ShortMA2[0]=(slr2[0]);
				ShortMA3[0]=(slr3[0]);
				ShortMA4[0]=(slr4[0]);
				ShortMA5[0]=(slr5[0]);
				ShortMA6[0]=(slr6[0]);
				#endregion
			}
			else if(pMATypeST == FMMA_type.ZLEMA) {
				#region Init dataseries
				ShortMA1[0]=(sz1[0]);
				ShortMA2[0]=(sz2[0]);
				ShortMA3[0]=(sz3[0]);
				ShortMA4[0]=(sz4[0]);
				ShortMA5[0]=(sz5[0]);
				ShortMA6[0]=(sz6[0]);
				#endregion
			}

			if(pMATypeLT == FMMA_type.EMA) {
				#region Init dataseries
				LongMA1[0]=(le1[0]);
				LongMA2[0]=(le2[0]);
				LongMA3[0]=(le3[0]);
				LongMA4[0]=(le4[0]);
				LongMA5[0]=(le5[0]);
				LongMA6[0]=(le6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.HMA) {
				#region Init dataseries
				LongMA1[0]=(lh1[0]);
				LongMA2[0]=(lh2[0]);
				LongMA3[0]=(lh3[0]);
				LongMA4[0]=(lh4[0]);
				LongMA5[0]=(lh5[0]);
				LongMA6[0]=(lh6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.SMA) {
				#region Init dataseries
				LongMA1[0]=(ls1[0]);
				LongMA2[0]=(ls2[0]);
				LongMA3[0]=(ls3[0]);
				LongMA4[0]=(ls4[0]);
				LongMA5[0]=(ls5[0]);
				LongMA6[0]=(ls6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.WMA) {
				#region Init dataseries
				LongMA1[0]=(lw1[0]);
				LongMA2[0]=(lw2[0]);
				LongMA3[0]=(lw3[0]);
				LongMA4[0]=(lw4[0]);
				LongMA5[0]=(lw5[0]);
				LongMA6[0]=(lw6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.TEMA) {
				#region Init dataseries
				LongMA1[0]=(lte1[0]);
				LongMA2[0]=(lte2[0]);
				LongMA3[0]=(lte3[0]);
				LongMA4[0]=(lte4[0]);
				LongMA5[0]=(lte5[0]);
				LongMA6[0]=(lte6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.VWMA) {
				#region Init dataseries
				LongMA1[0]=(lvw1[0]);
				LongMA2[0]=(lvw2[0]);
				LongMA3[0]=(lvw3[0]);
				LongMA4[0]=(lvw4[0]);
				LongMA5[0]=(lvw5[0]);
				LongMA6[0]=(lvw6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.LinReg) {
				#region Init dataseries
				LongMA1[0]=(llr1[0]);
				LongMA2[0]=(llr2[0]);
				LongMA3[0]=(llr3[0]);
				LongMA4[0]=(llr4[0]);
				LongMA5[0]=(llr5[0]);
				LongMA6[0]=(llr6[0]);
				#endregion
			}
			else if(pMATypeLT == FMMA_type.ZLEMA) {
				#region Init dataseries
				LongMA1[0]=(lz1[0]);
				LongMA2[0]=(lz2[0]);
				LongMA3[0]=(lz3[0]);
				LongMA4[0]=(lz4[0]);
				LongMA5[0]=(lz5[0]);
				LongMA6[0]=(lz6[0]);
				#endregion
			}

			if (CurrentBar<2) 
				return;

			int plotnum = 0;
			#region ShortMA1
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pShortUpColor;
			else PlotBrushes[plotnum][0] = pShortDownColor;
			#endregion

			plotnum = 1;
			#region ShortMA2
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pShortUpColor;
			else PlotBrushes[plotnum][0] = pShortDownColor;
			#endregion

			plotnum = 2;
			#region ShortMA3
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pShortUpColor;
			else PlotBrushes[plotnum][0] = pShortDownColor;
			#endregion

			plotnum = 3;
			#region ShortMA4
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pShortUpColor;
			else PlotBrushes[plotnum][0] = pShortDownColor;
			#endregion

			plotnum = 4;
			#region ShortMA5
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pShortUpColor;
			else PlotBrushes[plotnum][0] = pShortDownColor;
			#endregion

			plotnum = 5;
			#region ShortMA6
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pShortUpColor;
			else PlotBrushes[plotnum][0] = pShortDownColor;
			#endregion

			plotnum = 6;
			#region LongMA1
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pLongUpColor;
			else PlotBrushes[plotnum][0] = pLongDownColor;
			#endregion

			plotnum = 7;
			#region LongMA2
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pLongUpColor;
			else PlotBrushes[plotnum][0] = pLongDownColor;
			#endregion

			plotnum = 8;
			#region LongMA3
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pLongUpColor;
			else PlotBrushes[plotnum][0] = pLongDownColor;
			#endregion

			plotnum = 9;
			#region LongMA4
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pLongUpColor;
			else PlotBrushes[plotnum][0] = pLongDownColor;
			#endregion

			plotnum = 10;
			#region LongMA5
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pLongUpColor;
			else PlotBrushes[plotnum][0] = pLongDownColor;
			#endregion

			plotnum = 11;
			#region LongMA6
			if(Values[plotnum][0] > Values[plotnum][1]) PlotBrushes[plotnum][0] = pLongUpColor;
			else PlotBrushes[plotnum][0] = pLongDownColor;
			#endregion
		}

		#region Properties
		
		#region ST average params
		private int pSTperiod1 = 3;
		private int pSTperiod2 = 5;
		private int pSTperiod3 = 8;
		private int pSTperiod4 = 10;
		private int pSTperiod5 = 12;
		private int pSTperiod6 = 15;
		[Description("Period of the short-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        STperiod1
		{
			get { return pSTperiod1; }
			set {        pSTperiod1 = value; }
		}
		[Description("Period of the short-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        STperiod2
		{
			get { return pSTperiod2; }
			set {        pSTperiod2 = value; }
		}
		[Description("Period of the short-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        STperiod3
		{
			get { return pSTperiod3; }
			set {        pSTperiod3 = value; }
		}
		[Description("Period of the short-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        STperiod4
		{
			get { return pSTperiod4; }
			set {        pSTperiod4 = value; }
		}
		[Description("Period of the short-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        STperiod5
		{
			get { return pSTperiod5; }
			set {        pSTperiod5 = value; }
		}
		[Description("Period of the short-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        STperiod6
		{
			get { return pSTperiod6; }
			set {        pSTperiod6 = value; }
		}
		#endregion
		
		#region LT average params
		private int pLTperiod1 = 30;
		private int pLTperiod2 = 35;
		private int pLTperiod3 = 40;
		private int pLTperiod4 = 45;
		private int pLTperiod5 = 50;
		private int pLTperiod6 = 60;
		[Description("Period of the long-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        LTperiod1
		{
			get { return pLTperiod1; }
			set {        pLTperiod1 = value; }
		}
		[Description("Period of the long-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        LTperiod2
		{
			get { return pLTperiod2; }
			set {        pLTperiod2 = value; }
		}
		[Description("Period of the long-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        LTperiod3
		{
			get { return pLTperiod3; }
			set {        pLTperiod3 = value; }
		}
		[Description("Period of the long-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        LTperiod4
		{
			get { return pLTperiod4; }
			set {        pLTperiod4 = value; }
		}
		[Description("Period of the long-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        LTperiod5
		{
			get { return pLTperiod5; }
			set {        pLTperiod5 = value; }
		}
		[Description("Period of the long-term moving averages")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public int        LTperiod6
		{
			get { return pLTperiod6; }
			set {        pLTperiod6 = value; }
		}
		#endregion


		private FMMA_type pMATypeLT = FMMA_type.EMA;
		[Description("Select one of the moving average types")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public FMMA_type MAType_LongTerm
		{
			get { return pMATypeLT; }
			set { pMATypeLT = value; }
		}
		private FMMA_type pMATypeST = FMMA_type.EMA;
		[Description("Select one of the moving average types")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public FMMA_type MAType_ShortTerm
		{
			get { return pMATypeST; }
			set { pMATypeST = value; }
		}

		private Brush pShortUpColor = Brushes.Lime;
		[XmlIgnore]
		[Description("Color of upward moving Short MAs")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Trend Colors")]
		public Brush ShortUpColor
		{get { return pShortUpColor; }
		set { pShortUpColor = value; }}
		[Browsable(false)]
		public string ShortUpColorDownSerialize
		{get { return Serialize.BrushToString(pShortUpColor); }set { pShortUpColor = Serialize.StringToBrush(value); }}

		private Brush pShortDownColor =Brushes.Magenta;
		[XmlIgnore]
		[Description("Color of downward moving Short MAs")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Trend Colors")]
		public Brush ShortDownColor
		{get { return pShortDownColor; }
		set { pShortDownColor = value; }}
		[Browsable(false)]
		public string ShortDownColorDownSerialize
		{get { return Serialize.BrushToString(pShortDownColor); }set { pShortDownColor = Serialize.StringToBrush(value); }}

		private Brush pLongUpColor = Brushes.DarkGreen;
		[XmlIgnore]
		[Description("Color of upward moving Long MAs")]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Trend Colors")]
		public Brush LongUpColor
		{get { return pLongUpColor; }
		set { pLongUpColor = value; }}
		[Browsable(false)]
        public string LongUpColorDownSerialize{get { return Serialize.BrushToString(pLongUpColor); }set { pLongUpColor = Serialize.StringToBrush(value); }}

		private Brush pLongDownColor = Brushes.DarkRed;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Trend Colors")]
		[Description("Color of downward moving Long MAs")]
		public Brush LongDownColor
		{	get { return pLongDownColor; }
			set { pLongDownColor = value; }
		}
        [Browsable(false)]
        public string LongDownColorDownSerialize{get { return Serialize.BrushToString(pLongDownColor); }set { pLongDownColor = Serialize.StringToBrush(value); }}
		#endregion

		#region Plots
		
		[Browsable(false),XmlIgnore()]
		public Series<double> ShortMA1 {get { return Values[0]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> ShortMA2 {get { return Values[1]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> ShortMA3 {get { return Values[2]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> ShortMA4 {get { return Values[3]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> ShortMA5 {get { return Values[4]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> ShortMA6 {get { return Values[5]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> LongMA1 {get { return Values[6]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> LongMA2 {get { return Values[7]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> LongMA3 {get { return Values[8]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> LongMA4 {get { return Values[9]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> LongMA5 {get { return Values[10]; }}

		[Browsable(false),XmlIgnore()]
		public Series<double> LongMA6 {get { return Values[11]; }}

		#endregion
	}
}
	public enum FMMA_type
	{	HMA,
		SMA,
		EMA,
		WMA,
		TEMA,
		VWMA,
		ZLEMA,
		LinReg}



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IW_FanMA[] cacheIW_FanMA;
		public IW_FanMA IW_FanMA(int sTperiod1, int sTperiod2, int sTperiod3, int sTperiod4, int sTperiod5, int sTperiod6, int lTperiod1, int lTperiod2, int lTperiod3, int lTperiod4, int lTperiod5, int lTperiod6, FMMA_type mAType_LongTerm, FMMA_type mAType_ShortTerm)
		{
			return IW_FanMA(Input, sTperiod1, sTperiod2, sTperiod3, sTperiod4, sTperiod5, sTperiod6, lTperiod1, lTperiod2, lTperiod3, lTperiod4, lTperiod5, lTperiod6, mAType_LongTerm, mAType_ShortTerm);
		}

		public IW_FanMA IW_FanMA(ISeries<double> input, int sTperiod1, int sTperiod2, int sTperiod3, int sTperiod4, int sTperiod5, int sTperiod6, int lTperiod1, int lTperiod2, int lTperiod3, int lTperiod4, int lTperiod5, int lTperiod6, FMMA_type mAType_LongTerm, FMMA_type mAType_ShortTerm)
		{
			if (cacheIW_FanMA != null)
				for (int idx = 0; idx < cacheIW_FanMA.Length; idx++)
					if (cacheIW_FanMA[idx] != null && cacheIW_FanMA[idx].STperiod1 == sTperiod1 && cacheIW_FanMA[idx].STperiod2 == sTperiod2 && cacheIW_FanMA[idx].STperiod3 == sTperiod3 && cacheIW_FanMA[idx].STperiod4 == sTperiod4 && cacheIW_FanMA[idx].STperiod5 == sTperiod5 && cacheIW_FanMA[idx].STperiod6 == sTperiod6 && cacheIW_FanMA[idx].LTperiod1 == lTperiod1 && cacheIW_FanMA[idx].LTperiod2 == lTperiod2 && cacheIW_FanMA[idx].LTperiod3 == lTperiod3 && cacheIW_FanMA[idx].LTperiod4 == lTperiod4 && cacheIW_FanMA[idx].LTperiod5 == lTperiod5 && cacheIW_FanMA[idx].LTperiod6 == lTperiod6 && cacheIW_FanMA[idx].MAType_LongTerm == mAType_LongTerm && cacheIW_FanMA[idx].MAType_ShortTerm == mAType_ShortTerm && cacheIW_FanMA[idx].EqualsInput(input))
						return cacheIW_FanMA[idx];
			return CacheIndicator<IW_FanMA>(new IW_FanMA(){ STperiod1 = sTperiod1, STperiod2 = sTperiod2, STperiod3 = sTperiod3, STperiod4 = sTperiod4, STperiod5 = sTperiod5, STperiod6 = sTperiod6, LTperiod1 = lTperiod1, LTperiod2 = lTperiod2, LTperiod3 = lTperiod3, LTperiod4 = lTperiod4, LTperiod5 = lTperiod5, LTperiod6 = lTperiod6, MAType_LongTerm = mAType_LongTerm, MAType_ShortTerm = mAType_ShortTerm }, input, ref cacheIW_FanMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IW_FanMA IW_FanMA(int sTperiod1, int sTperiod2, int sTperiod3, int sTperiod4, int sTperiod5, int sTperiod6, int lTperiod1, int lTperiod2, int lTperiod3, int lTperiod4, int lTperiod5, int lTperiod6, FMMA_type mAType_LongTerm, FMMA_type mAType_ShortTerm)
		{
			return indicator.IW_FanMA(Input, sTperiod1, sTperiod2, sTperiod3, sTperiod4, sTperiod5, sTperiod6, lTperiod1, lTperiod2, lTperiod3, lTperiod4, lTperiod5, lTperiod6, mAType_LongTerm, mAType_ShortTerm);
		}

		public Indicators.IW_FanMA IW_FanMA(ISeries<double> input , int sTperiod1, int sTperiod2, int sTperiod3, int sTperiod4, int sTperiod5, int sTperiod6, int lTperiod1, int lTperiod2, int lTperiod3, int lTperiod4, int lTperiod5, int lTperiod6, FMMA_type mAType_LongTerm, FMMA_type mAType_ShortTerm)
		{
			return indicator.IW_FanMA(input, sTperiod1, sTperiod2, sTperiod3, sTperiod4, sTperiod5, sTperiod6, lTperiod1, lTperiod2, lTperiod3, lTperiod4, lTperiod5, lTperiod6, mAType_LongTerm, mAType_ShortTerm);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IW_FanMA IW_FanMA(int sTperiod1, int sTperiod2, int sTperiod3, int sTperiod4, int sTperiod5, int sTperiod6, int lTperiod1, int lTperiod2, int lTperiod3, int lTperiod4, int lTperiod5, int lTperiod6, FMMA_type mAType_LongTerm, FMMA_type mAType_ShortTerm)
		{
			return indicator.IW_FanMA(Input, sTperiod1, sTperiod2, sTperiod3, sTperiod4, sTperiod5, sTperiod6, lTperiod1, lTperiod2, lTperiod3, lTperiod4, lTperiod5, lTperiod6, mAType_LongTerm, mAType_ShortTerm);
		}

		public Indicators.IW_FanMA IW_FanMA(ISeries<double> input , int sTperiod1, int sTperiod2, int sTperiod3, int sTperiod4, int sTperiod5, int sTperiod6, int lTperiod1, int lTperiod2, int lTperiod3, int lTperiod4, int lTperiod5, int lTperiod6, FMMA_type mAType_LongTerm, FMMA_type mAType_ShortTerm)
		{
			return indicator.IW_FanMA(input, sTperiod1, sTperiod2, sTperiod3, sTperiod4, sTperiod5, sTperiod6, lTperiod1, lTperiod2, lTperiod3, lTperiod4, lTperiod5, lTperiod6, mAType_LongTerm, mAType_ShortTerm);
		}
	}
}

#endregion
