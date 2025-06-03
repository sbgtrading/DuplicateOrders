
#region Using declarations
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[Description("www.IndicatorWarehouse.com  UltimateRSI provides numerous signal methods to help you know when an OB or OS or trend reversal situation occurs on the RSI")]
	public class UltimateRSI : Indicator
	{
		private bool LicenseValid = true;

		private Series<double> neg, pos;
		private bool SignalOnOB = false;
		private bool SignalOnOS = false;
		private bool RunInit = true;
		private int EmailsThisBar = 0;
		private int AlertsThisBar = 0;
		private int PopupThisBar = 0;
		private string Subj, Body, NL;
		private string InstrumentPeriodString=string.Empty;
		private string TrendUptag, TrendDowntag;
		private string SignalLineName = "RSI";

		private Series<double>					avgUp;
		private Series<double>					avgDown;
		private Series<double>					down;
		private Series<double>					up;
		private Series<double> smmaUp = null;
		private Series<double> smmaDown = null;
//		private IW_SMMA smmaUp = null;
//		private IW_SMMA smmaDown = null;
		private Brush BkgBrushTinted_OS = null;
		private Brush BkgBrushTinted_OB = null;
		private Brush BkgBrushTinted_TrendUp = null;
		private Brush BkgBrushTinted_TrendDown = null;
		private Brush BkgBrushTinted_CenterlineUp = null;
		private Brush BkgBrushTinted_CenterlineDown = null;

		/// <summary>
		/// This method is used to configure the indicator and is called once before any bar data is loaded.
		/// </summary>
		protected override void OnStateChange()
		{
			if(State==State.Historical){
				//BkgBrushTinted_OS = Color.FromArgb((byte)(pOpacityOS*25),  pOS_BackgroundColor.R, pOS_BackgroundColor.G, pOS_BackgroundColor.B);
				BkgBrushTinted_OS = this.pBkgBrushTinted_OS.Clone();
				BkgBrushTinted_OS.Opacity = this.pOpacityOS/10.0;
				BkgBrushTinted_OS.Freeze();

				//BkgBrushTinted_OB = Color.FromArgb((byte)(pOpacityOB*25),  pOB_BackgroundColor.R, pOB_BackgroundColor.G, pOB_BackgroundColor.B);
				BkgBrushTinted_OB = this.pBkgBrushTinted_OB.Clone();
				BkgBrushTinted_OB.Opacity = this.pOpacityOB/10.0;
				BkgBrushTinted_OB.Freeze();

//				BkgBrushTinted_TrendUp   = Color.FromArgb((byte)(pTrendBackgroundOpacity*25), this.pTrendUpBackgroundBrush.R,this.pTrendUpBackgroundBrush.G,this.pTrendUpBackgroundBrush.B);
				BkgBrushTinted_TrendUp = this.pTrendUpBackgroundBrush.Clone();
				BkgBrushTinted_TrendUp.Opacity = this.pTrendBackgroundOpacity/10.0;
				BkgBrushTinted_TrendUp.Freeze();

				//BkgBrushTinted_TrendDown = Color.FromArgb((byte)(pTrendBackgroundOpacity*25), this.pTrendDownBackgroundBrush.R,this.pTrendDownBackgroundBrush.G,this.pTrendDownBackgroundBrush.B);
				BkgBrushTinted_TrendDown = this.pTrendDownBackgroundBrush.Clone();
				BkgBrushTinted_TrendDown.Opacity = this.pTrendBackgroundOpacity/10.0;
				BkgBrushTinted_TrendDown.Freeze();

				BkgBrushTinted_CenterlineUp = this.pCenterlineUp_BackgroundBrush.Clone();
				BkgBrushTinted_CenterlineUp.Opacity = this.pOpacityCenterlineUp/10.0;
				BkgBrushTinted_CenterlineUp.Freeze();
				BkgBrushTinted_CenterlineDown = this.pCenterlineDown_BackgroundBrush.Clone();
				BkgBrushTinted_CenterlineDown.Opacity = this.pOpacityCenterlineDown/10.0;
				BkgBrushTinted_CenterlineDown.Freeze();
			}
			if (State == State.SetDefaults)
			{
				bool IsBen = NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 && System.IO.File.Exists("c:\\222222222222.txt");
				if(!IsBen)
					VendorLicense("IndicatorWarehouse", "IWfree7", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				Name = "iw Ultimate RSI";
				NL = Environment.NewLine;
				AddPlot(Brushes.Green, "RSI");
				AddPlot(new Stroke(Brushes.Pink,DashStyleHelper.Solid, 2), PlotStyle.Dot, "Avg");
				AddPlot(new Stroke(Brushes.Red,DashStyleHelper.Solid, 2),         PlotStyle.Dot,   "OBsignal");
				AddPlot(new Stroke(Brushes.Blue,DashStyleHelper.Solid, 2),        PlotStyle.Dot,   "OSsignal");
				AddPlot(Brushes.Transparent, "TrendSignal");

				AddLine(new Stroke(Brushes.DarkViolet, DashStyleHelper.Solid, 1), 20, "OS");
				AddLine(new Stroke(Brushes.Cyan, DashStyleHelper.Solid, 1), 50, "Midline");
				AddLine(new Stroke(Brushes.YellowGreen, DashStyleHelper.Solid, 1), 80, "OB");
				Calculate=Calculate.OnPriceChange;


				avgUp				= new Series<double>(this);
				avgDown				= new Series<double>(this);
				down				= new Series<double>(this);
				up					= new Series<double>(this);
				smmaUp				= new Series<double>(this);
				smmaDown			= new Series<double>(this);
			}
		}
//		protected override void OnStartUp () {
//		}

		protected override void OnBarUpdate()
		{
			if(RunInit) {
				RunInit = false;

				InstrumentPeriodString = Instrument.FullName+" ("+Bars.BarsPeriod.ToString()+")";
				Lines[0].Value = pOSlevel;
				Lines[2].Value = pOBlevel;
				Plots[2].Min = pOBlevel;
				Plots[3].Max = pOSlevel;
				Oscillator[0]=(0);
				Avg[0]=(0);
			}
			if(CurrentBar<RSIPeriod+1) {
				down[0]=(0);
				up[0]=(0);
				Oscillator[0]=(50);
				Avg[0]=(50);
				return;
			}

			#region Calculate RSI
			down[0]=(Math.Max(Close[1] - Close[0], 0));
			up[0]=(Math.Max(Close[0] - Close[1], 0));

			if ((CurrentBar + 1) < RSIPeriod) 
			{
				if ((CurrentBar + 1) == (RSIPeriod - 1))
					Avg[0]=(50);
				return;
			}

//			if ((CurrentBar + 1) == RSIPeriod) 
			{
				if(pCalcType == UltimateRSI_CalcType.Cutlers){
					avgDown[0]= SMA(down,RSIPeriod)[0];
					avgUp[0]=   SMA(up,RSIPeriod)[0];
				}
				else if(pCalcType == UltimateRSI_CalcType.Wilders){
					//if(smmaUp==null)
					{
						smmaUp[0] = IW_SMMA(up,RSIPeriod)[0];
						smmaDown[0] = IW_SMMA(down,RSIPeriod)[0];
						//smmaUp.CalculateOnBarClose=this.CalculateOnBarClose;
						//smmaDown.CalculateOnBarClose=this.CalculateOnBarClose;
					}
					avgDown[0]=(smmaDown[0]);
					avgUp[0]=(smmaUp[0]);
				}
				else if(pCalcType == UltimateRSI_CalcType.Exponential){
					avgDown[0]=EMA(down,RSIPeriod)[0];
					avgUp[0]=EMA(up,RSIPeriod)[0];
				}
			}  
//			else 
//			{
//				// Rest of averages are smoothed
//				avgDown.Set((avgDown[1] * (RSIPeriod - 1) + down[0]) / RSIPeriod); //could not convert
//				avgUp.Set((avgUp[1] * (RSIPeriod - 1) + up[0]) / RSIPeriod); //could not convert
//			}

			double rsi	  = avgDown[0] == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown[0]);
			double rsiAvg = (2.0 / (1 + pAvgPeriod)) * rsi + (1 - (2.0 / (1 + pAvgPeriod))) * Avg[1];
			#endregion

			if(ChartControl!=null) {
				if(this.pTrendBkgColorAllPanels || this.pOBOSBkgColorAllPanels) BackBrushAll = null;
				else BackBrush = null;
			}


			Oscillator[0]=(rsi);
			Avg[0]=(rsiAvg);
			
			bool PermitChartMarkers = CurrentBar > Bars.Count-3000;

			if(IsFirstTickOfBar){
				EmailsThisBar = 0;
				AlertsThisBar = 0;
				TrendUptag = string.Concat("TU",CurrentBar.ToString());
				TrendDowntag = string.Concat("TD",CurrentBar.ToString());
			}
			bool TrendUp = false;
			bool TrendDown = false;
			double SignalLineValue = Oscillator[0];
			double SignalLineValue1 = Oscillator[1];
			if(pSignalBasis == UltimateRSI_SignalBasis.TheAvg){
				SignalLineValue = Avg[0];
				SignalLineValue1 = Avg[1];
				SignalLineName = "Avg";
			}
			TrendSignal.Reset();
			#region Trend Logic
			#region Determine TrendUp or TrendDown
			if(pSignalBasis == UltimateRSI_SignalBasis.TheRSI){
				if(SignalLineValue>SignalLineValue1){
					TrendUp = true;
					for(int i = 1; i<CurrentBar-1; i++){
						if(Oscillator[i+1]>Oscillator[i]){
							TrendUp = true;
							break;
						}
						if(Oscillator[i+1]<Oscillator[i]){
							TrendUp = false;
							break;
						}
					}
				}
				if(SignalLineValue<SignalLineValue1){
					TrendDown = true;
					for(int i = 1; i<CurrentBar-1; i++){
						if(Oscillator[i+1]<Oscillator[i]){
							TrendDown = true;
							break;
						}
						if(Oscillator[i+1]>Oscillator[i]){
							TrendDown = false;
							break;
						}
					}
				}
			} else if(pSignalBasis == UltimateRSI_SignalBasis.TheAvg){
				if(SignalLineValue>SignalLineValue1){
					TrendUp = true;
					for(int i = 1; i<CurrentBar-1; i++){
						if(Avg[i+1]>Avg[i]){
							TrendUp = true;
							break;
						}
						if(Avg[i+1]<Avg[i]){
							TrendUp = false;
							break;
						}
					}
				}
				if(SignalLineValue<SignalLineValue1){
					TrendDown = true;
					for(int i = 1; i<CurrentBar-1; i++){
						if(Avg[i+1]<Avg[i]){
							TrendDown = true;
							break;
						}
						if(Avg[i+1]>Avg[i]){
							TrendDown = false;
							break;
						}
					}
				}
			}
			#endregion

			if(TrendDown) {
				TrendSignal[0]=(-1);
				if(pEnableTrendArrows && PermitChartMarkers) {
					if(pTrendVisualType == UltimateRSI_VisualType.Arrow) {
						Draw.ArrowDown(this, TrendDowntag,this.IsAutoScale,0,High[0]+TickSize*pTrendSeparation,pTrendDownArrowBrush);
					}
					else if(pTrendVisualType==UltimateRSI_VisualType.Dot)Draw.Dot(this, TrendDowntag,this.IsAutoScale,0,High[0]+TickSize*pTrendSeparation,pTrendDownArrowBrush);
					else if(pTrendVisualType==UltimateRSI_VisualType.Square)Draw.Square(this,TrendDowntag,this.IsAutoScale,0,High[0]+TickSize*pTrendSeparation,pTrendDownArrowBrush);
					else if(pTrendVisualType == UltimateRSI_VisualType.Triangle && PermitChartMarkers) {
						Draw.TriangleDown(this,TrendDowntag,this.IsAutoScale,0,High[0]+TickSize*pTrendSeparation,pTrendDownArrowBrush);
					}
				}
				if(ChartControl!=null && pTrendDownBackgroundBrush != null && this.pTrendBackgroundOpacity!=0) {
					if(this.pTrendBkgColorAllPanels) BackBrushAll = this.BkgBrushTinted_TrendDown;
					else BackBrush = this.BkgBrushTinted_TrendDown;
				}
				if(pPopupOnTrend && !(State == State.Historical) && PopupThisBar!=CurrentBar) {
					PopupThisBar = CurrentBar;
					Log(InstrumentPeriodString+Environment.NewLine+"UltimateRSI DOWN TREND",NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(AlertsThisBar<pMaxAlerts && pTrendDownSound.Length>0 && !pTrendDownSound.Contains("<")) {
					AlertsThisBar++;
					Alert(CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "UltimateRSI falling", AddSoundFolder(this.pTrendDownSound), 1, Brushes.Black, Brushes.Magenta);
				}

				if(EmailsThisBar<pMaxEmails && this.Email_Trend.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has fallen on ",InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has fallen",NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_Trend, Subj, Body);
				}
			}
			if(TrendUp) {
				TrendSignal[0]=(1);
				if(pEnableTrendArrows && PermitChartMarkers) {
					if(pTrendVisualType == UltimateRSI_VisualType.Arrow) {
						Draw.ArrowUp(this, TrendUptag,this.IsAutoScale,0,Low[0]-TickSize*pTrendSeparation,pTrendUpArrowBrush);
					}
					else if(pTrendVisualType==UltimateRSI_VisualType.Dot)Draw.Dot(this, TrendUptag,this.IsAutoScale,0,Low[0]-TickSize*pTrendSeparation,pTrendUpArrowBrush);
					else if(pTrendVisualType==UltimateRSI_VisualType.Square)Draw.Square(this,TrendUptag,this.IsAutoScale,0,Low[0]-TickSize*pTrendSeparation,pTrendUpArrowBrush);
					else if(pTrendVisualType == UltimateRSI_VisualType.Triangle && PermitChartMarkers) {
						Draw.TriangleUp(this,TrendUptag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation,pTrendUpArrowBrush);
					}
				}
				if(ChartControl!=null && this.pTrendUpBackgroundBrush != null && this.pTrendBackgroundOpacity!=0) {
					if(this.pTrendBkgColorAllPanels) BackBrushAll = BkgBrushTinted_TrendUp;
					else BackBrush = BkgBrushTinted_TrendUp;
				}
				if(pPopupOnTrend && !(State == State.Historical) && PopupThisBar!=CurrentBar) {
					PopupThisBar = CurrentBar;
					Log(InstrumentPeriodString+Environment.NewLine+"UltimateRSI UP TREND",NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(AlertsThisBar<pMaxAlerts && pTrendUpSound.Length>0 && !pTrendUpSound.Contains("<")) {
					AlertsThisBar++;
					Alert(CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "UltimateRSI "+SignalLineName+" rising", AddSoundFolder(this.pTrendUpSound), 1, Brushes.Black, Brushes.Cyan);
				}

				if(EmailsThisBar<pMaxEmails && pEmailAddress_Trend.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has risen ",InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has risen",NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_Trend, Subj, Body);
				}
			}
			if(SignalLineValue <= SignalLineValue1) RemoveDrawObject(TrendUptag);
			if(SignalLineValue >= SignalLineValue1) RemoveDrawObject(TrendDowntag);

			#endregion


			OBsignal[0]=(SignalLineValue);
			OSsignal[0]=(SignalLineValue);
			#region OBOS logic
			string OBOStag = string.Concat("OBOS",CurrentBar.ToString());
			if(SignalLineValue > pOBlevel) {
				if(pEnableOBArrows && PermitChartMarkers) {
					if(pOBOSVisualType == UltimateRSI_VisualType.Arrow) {
						if(pReverseOBOSArrowDirection)
							Draw.ArrowUp(this, OBOStag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation, this.pOBArrowBrush);
						else 
							Draw.ArrowDown(this, OBOStag,this.IsAutoScale,0,High[0]+TickSize*pOBOSSeparation, pOBArrowBrush);
					}
					else if(pOBOSVisualType==UltimateRSI_VisualType.Dot) Draw.Dot(this, OBOStag,this.IsAutoScale,0,High[0]+TickSize*pOBOSSeparation,pOBArrowBrush);
					else if(pOBOSVisualType==UltimateRSI_VisualType.Square) Draw.Square(this,OBOStag,this.IsAutoScale,0,High[0]+TickSize*pOBOSSeparation,pOBArrowBrush);
					else if(pOBOSVisualType== UltimateRSI_VisualType.Triangle) {
						if(pReverseOBOSArrowDirection) Draw.TriangleUp(this,OBOStag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation,pOBArrowBrush);
						else Draw.TriangleDown(this,OBOStag,this.IsAutoScale,0,High[0]+TickSize*pOBOSSeparation,pOBArrowBrush);
					}
				}
				if(ChartControl!=null && BkgBrushTinted_OB != null && pOpacityOB!=0) {
					if(pOBOSBkgColorAllPanels) BackBrushAll = BkgBrushTinted_OB;
					else BackBrush = BkgBrushTinted_OB;
				}

				if(SignalOnOB) {
					SignalOnOB = false;
					if(pPopupOnOBOS && !(State == State.Historical) && PopupThisBar!=CurrentBar) {
						PopupThisBar = CurrentBar;
						Log(InstrumentPeriodString+Environment.NewLine+"UltimateRSI above OB level of "+pOBlevel,NinjaTrader.Cbi.LogLevel.Alert);
					}
					if(AlertsThisBar<pMaxAlerts && pOBSound.Length>0 && !pOBSound.Contains("<")) {
						AlertsThisBar++;
						Alert(CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "UltimateRSI "+SignalLineName+" above OB level of "+pOBlevel, AddSoundFolder(pOBSound), 1, Brushes.Black, Brushes.White);
					}
				}
				if(EmailsThisBar<pMaxEmails && pEmailAddress_IntoOB.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed into OB on ",InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed into the OB zone, the OB level is ",pOBlevel.ToString(),NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_IntoOB, Subj, Body);
				}
			} else if(SignalLineValue < pOSlevel) {
//				if(pEnableOSArrows) OSsignal.Set(Low[0]-TickSize*pSeparation); //could not convert
				if(pEnableOSArrows && PermitChartMarkers) {
					if(pOBOSVisualType == UltimateRSI_VisualType.Arrow) {
						if(pReverseOBOSArrowDirection)
							Draw.ArrowDown(this, OBOStag,this.IsAutoScale,0,High[0]+TickSize*pOBOSSeparation,pOSArrowBrush);
						else 
							Draw.ArrowUp(this, OBOStag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation,pOSArrowBrush);
					}
					else if(pOBOSVisualType==UltimateRSI_VisualType.Dot)Draw.Dot(this, OBOStag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation,pOSArrowBrush);
					else if(pOBOSVisualType==UltimateRSI_VisualType.Square)Draw.Square(this,OBOStag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation,pOSArrowBrush);
					else if(pOBOSVisualType == UltimateRSI_VisualType.Triangle) {
						if(pReverseOBOSArrowDirection)Draw.TriangleDown(this,OBOStag,this.IsAutoScale,0,High[0]+TickSize*pOBOSSeparation,pOSArrowBrush);
						else Draw.TriangleUp(this,OBOStag,this.IsAutoScale,0,Low[0]-TickSize*pOBOSSeparation,pOSArrowBrush);
					}
				}
				if(ChartControl!=null && BkgBrushTinted_OS != null && pOpacityOS!=0) {
					if(pOBOSBkgColorAllPanels) BackBrushAll = BkgBrushTinted_OS;
					else BackBrush = BkgBrushTinted_OS;
				}
				if(SignalOnOS) {
					SignalOnOS = false;
					if(pPopupOnOBOS && !(State == State.Historical) && PopupThisBar!=CurrentBar) {
						PopupThisBar = CurrentBar;
						Log(InstrumentPeriodString+Environment.NewLine+"UltimateRSI "+SignalLineName+" below OS level of "+pOSlevel,NinjaTrader.Cbi.LogLevel.Alert);
					}
					if(AlertsThisBar<pMaxAlerts && pOSSound.Length>0 && !pOSSound.Contains("<")) {
						AlertsThisBar++;
						Alert(CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "UltimateRSI "+SignalLineName+" below OS level of "+pOSlevel, AddSoundFolder(pOSSound), 1, Brushes.Black, Brushes.White);
					}
				}
				if(EmailsThisBar<pMaxEmails && pEmailAddress_IntoOS.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed into OS on ",InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed into the OS zone, the OS level is ",pOSlevel.ToString(),NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_IntoOS, Subj, Body);
				}
			}
			if(SignalLineValue >= pOSlevel  && SignalLineValue <= pOBlevel)  RemoveDrawObject(OBOStag);
			if(SignalLineValue1 >= pOSlevel && SignalLineValue1 <= pOBlevel) RemoveDrawObject("OBOS"+(CurrentBar-1).ToString());

			if(IsFirstTickOfBar) {
				if(SignalLineValue < pOBlevel) SignalOnOB = true;
				if(SignalLineValue > pOSlevel) SignalOnOS = true;
			}
			if(SignalLineValue<pOBlevel && SignalLineValue1>=pOBlevel) {
				if(EmailsThisBar<pMaxEmails && pEmailAddress_ExitingOB.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has exited OB on ",InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has exited the OB zone, the OB level is ",pOBlevel.ToString(),NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_ExitingOB, Subj, Body);
				}
			}
			if(SignalLineValue>pOSlevel && SignalLineValue1<=pOSlevel) {
				if(EmailsThisBar<pMaxEmails && pEmailAddress_ExitingOS.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has exited OS on ",InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has exited the OS zone, the OS level is ",pOSlevel.ToString(),NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_ExitingOS, Subj, Body);
				}
			}
			#endregion

			#region Centerline logic
			string Centerlinetag = string.Concat("CLine",CurrentBar.ToString());
			if(pSignalBasis == UltimateRSI_SignalBasis.TheRSI){
			}else{
			}
			if(SignalLineValue > Lines[1].Value && SignalLineValue1 <= Lines[1].Value) {
				if(pEnableCenterlineUpArrows && PermitChartMarkers) {
					if(pCenterlineVisualType == UltimateRSI_VisualType.Arrow) {
						Draw.ArrowUp(this, Centerlinetag,this.IsAutoScale,0,Low[0]-TickSize*pCenterlineSeparation,this.pCenterlineUpArrowBrush);
					}
					else if(pCenterlineVisualType==UltimateRSI_VisualType.Dot)Draw.Dot(this, Centerlinetag,this.IsAutoScale,0,High[0]+TickSize*pCenterlineSeparation,pCenterlineUpArrowBrush);
					else if(pCenterlineVisualType==UltimateRSI_VisualType.Square)Draw.Square(this,Centerlinetag,this.IsAutoScale,0,High[0]+TickSize*pCenterlineSeparation,pCenterlineUpArrowBrush);
					else if(pCenterlineVisualType == UltimateRSI_VisualType.Triangle) {
						Draw.TriangleUp(this,Centerlinetag,this.IsAutoScale,0,Low[0]-TickSize*pCenterlineSeparation,pCenterlineUpArrowBrush);
					}
				}
				if(ChartControl!=null && pCenterlineUp_BackgroundBrush != null && pOpacityCenterlineUp!=0) {
					if(pCenterlineBkgColorAllPanels) BackBrushAll = BkgBrushTinted_CenterlineUp;
					else BackBrush = BkgBrushTinted_CenterlineUp;
				}

				if(pPopupOnCenterlineUp && !(State == State.Historical) && PopupThisBar!=CurrentBar) {
					PopupThisBar = CurrentBar;
					Log(InstrumentPeriodString+Environment.NewLine+"UltimateRSI crossed above level of "+Lines[1].Value,NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(AlertsThisBar<pMaxAlerts && pCenterlineUpSound.Length>0 && !pCenterlineUpSound.Contains("<")) {
					AlertsThisBar++;
					Alert(CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "UltimateRSI crossed above level of "+Lines[1].Value, AddSoundFolder(pCenterlineUpSound), 1, Brushes.Black, Brushes.White);
				}

				if(EmailsThisBar<pMaxEmails && pEmailAddress_Center.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed above "+Lines[1].Value.ToString("0.0"),InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed above ",Lines[1].Value.ToString("0.0"),NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_Center, Subj, Body);
				}
			}
			else if(SignalLineValue < Lines[1].Value && SignalLineValue1 >= Lines[1].Value) {
				if(pEnableCenterlineDownArrows && PermitChartMarkers) {
					if(pCenterlineVisualType == UltimateRSI_VisualType.Arrow) {
						Draw.ArrowDown(this, Centerlinetag,this.IsAutoScale,0,High[0]+TickSize*pCenterlineSeparation,pCenterlineDownArrowBrush);
					}
					else if(pCenterlineVisualType==UltimateRSI_VisualType.Dot)Draw.Dot(this, Centerlinetag,this.IsAutoScale,0,High[0]+TickSize*pCenterlineSeparation,pCenterlineDownArrowBrush);
					else if(pCenterlineVisualType==UltimateRSI_VisualType.Square)Draw.Square(this,Centerlinetag,this.IsAutoScale,0,High[0]+TickSize*pCenterlineSeparation,pCenterlineDownArrowBrush);
					else if(pCenterlineVisualType == UltimateRSI_VisualType.Triangle) {
						Draw.TriangleDown(this,Centerlinetag,this.IsAutoScale,0,High[0]+TickSize*pCenterlineSeparation,pCenterlineDownArrowBrush);
					}
				}
				if(ChartControl!=null && pCenterlineDown_BackgroundBrush != null && pOpacityCenterlineDown!=0) {
					if(pCenterlineBkgColorAllPanels) BackBrushAll = BkgBrushTinted_CenterlineDown;
					else BackBrush = BkgBrushTinted_CenterlineDown;
				}

				if(pPopupOnCenterlineDown && !(State == State.Historical) && PopupThisBar!=CurrentBar) {
					PopupThisBar = CurrentBar;
					Log(InstrumentPeriodString+Environment.NewLine+"UltimateRSI crossed below level of "+Lines[1].Value,NinjaTrader.Cbi.LogLevel.Alert);
				}
				if(AlertsThisBar<pMaxAlerts && pCenterlineDownSound.Length>0 && !pCenterlineDownSound.Contains("<")) {
					AlertsThisBar++;
					Alert(CurrentBar.ToString(), NinjaTrader.NinjaScript.Priority.High, "UltimateRSI "+SignalLineName+" crossed below level of "+Lines[1].Value, AddSoundFolder(pCenterlineDownSound), 1, Brushes.Black, Brushes.White);
				}

				if(EmailsThisBar<pMaxEmails && pEmailAddress_Center.Length>0) {
//Draw.Diamond(this, "Dia"+CurrentBar.ToString(),false,0,Low[0]-1,Color.Yellow);
					EmailsThisBar++;
					Subj = string.Concat("UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed below "+Lines[1].Value.ToString("0.0"),InstrumentPeriodString);
					Body = string.Concat(NL,NL,"UltimateRSI ",SignalLineName,"(",SignalLineValue.ToString("0.0"),") has crossed below ",Lines[1].Value.ToString("0.0"),NL,NL,"This message auto-generated by '",Name,"'");
					SendMail(pEmailAddress_Center, Subj, Body);
				}
			}
			if(SignalLineValue >= Lines[1].Value  && SignalLineValue1>=Lines[1].Value)  RemoveDrawObject(Centerlinetag);
			if(SignalLineValue <= Lines[1].Value  && SignalLineValue1<=Lines[1].Value)  RemoveDrawObject(Centerlinetag);
			#endregion
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
					GetStandardValues(ITypeDescriptorContext context)  {
				string folder = System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir,"sounds");
				string search = "*.wav";

				System.IO.DirectoryInfo dirCustom=null;
				System.IO.FileInfo[] filCustom=null;
				try{
					dirCustom = new System.IO.DirectoryInfo(folder);
					filCustom = dirCustom.GetFiles( search);
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

		#region Plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Oscillator {get { return Values[0]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Avg {get { return Values[1]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OBsignal {get { return Values[2]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OSsignal {get { return Values[3]; }}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TrendSignal {get { return Values[4]; }}
		#endregion

//====================================================================
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, @"sounds\"+wav);
		}
//====================================================================
		#region Properties

		private UltimateRSI_CalcType pCalcType = UltimateRSI_CalcType.Cutlers;
		[Description("Calculation method, see Wikipedia for details")]
		[NinjaScriptProperty]
// 		[Category("Parameters")]
// [Gui.Design.DisplayNameAttribute("\tRSI Calc Type")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "RSI Calc Type",  GroupName = "Parameters")]
		public UltimateRSI_CalcType CalcType
		{
			get { return pCalcType; }
			set { pCalcType = value; }
		}

		private int pRSIPeriod = 10;
		[Description("Numbers of bars used for the RSI")]
		[NinjaScriptProperty]
// 		[Category("Parameters")]
// [Gui.Design.DisplayNameAttribute("\tRSI period")]
[Display(ResourceType = typeof(Custom.Resource), Name = "RSI period",  GroupName = "Parameters")]
		public int RSIPeriod
		{
			get { return pRSIPeriod; }
			set { pRSIPeriod = Math.Max(1, value); }
		}

		private int pAvgPeriod = 3;
		[NinjaScriptProperty]
		[Description("Numbers of bars used for calculating the Average of the RSI")]
// 		[Category("Parameters")]
// [Gui.Design.DisplayNameAttribute("Avg Period")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Avg Period",  GroupName = "Parameters")]
		public int AvgPeriod
		{
			get { return pAvgPeriod; }
			set { pAvgPeriod = Math.Max(1, value); }
		}
		private UltimateRSI_SignalBasis pSignalBasis = UltimateRSI_SignalBasis.TheRSI;
		[Description("Which plot line to use for all Trend alerts, emails and arrows? 'None' turns-off the Trend-based alerts, emails and arrows")]
		[NinjaScriptProperty]
		[Category("Parameters")]
		public UltimateRSI_SignalBasis SignalBasis
		{
			get { return pSignalBasis; }
			set { pSignalBasis = value; }
		}
		private int pMaxAlerts = 1;
		[Description("Max number of audible Alerts per bar, useful if CalculateOnBarClose = false (set to '0' to turn off audible alerts)")]
		[Category("Alerts")]
		public int MaxAlerts
		{
			get { return pMaxAlerts; }
			set { pMaxAlerts = Math.Max(0, value); }
		}
		private int pMaxEmails = 1;
		[Description("Max number of emails per bar, useful if CalculateOnBarClose = false (set to '0' to turn off emails)")]
		[Category("Alerts")]
		public int MaxEmails
		{
			get { return pMaxEmails; }
			set { pMaxEmails = Math.Max(0, value); }
		}

		#region Trend Alert
		private string pTrendUpSound = "<enter wav name>";
        [Description("Sound when RSI or its Avg is rising...leave blank to turn-off this alert sound")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Trend Alerts")]
        public string TrendUpSound
        {
            get { return pTrendUpSound; }
            set { pTrendUpSound = value; }
        }

		private string pTrendDownSound = "<enter wav name>";
        [Description("Sound when RSI or its Avg is falling...leave blank to turn-off this alert sound")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("Trend Alerts")]
        public string TrendDownSound
        {
            get { return pTrendDownSound; }
            set { pTrendDownSound = value; }
        }
		private string pEmailAddress_Trend = "";
		[Description("Supply your address (e.g. 'you@mail.com') for Trend alerts, leave blank to turn-off emails for that signal")]
		[Category("Trend Alerts")]
		public string Email_Trend
		{
			get { return pEmailAddress_Trend; }
			set { pEmailAddress_Trend = value; }
		}
		private bool pPopupOnTrend = false;
		[Description("Launch a popup window when a trend reversal occurs")]
		[Category("Trend Alerts")]
		public bool PopupOnTrend
		{
			get { return pPopupOnTrend; }
			set { pPopupOnTrend = value; }
		}
		#endregion

		#region Trend Markers
		private UltimateRSI_VisualType pTrendVisualType = UltimateRSI_VisualType.Arrow;
		[Description("Type of visual marker on a trend change")]
// 		[Category("Trend Markers")]
// [Gui.Design.DisplayNameAttribute("Visual Type")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Visual Type",  GroupName = "Trend Markers")]
		public UltimateRSI_VisualType TrendVisualType
		{
			get { return pTrendVisualType; }
			set { pTrendVisualType = value; }
		}
		private bool pEnableTrendArrows = true;
		[Description("Enable arrows when Trend reversals occur")]
		[Category("Trend Markers")]
		public bool Trend_Enabled
		{
			get { return pEnableTrendArrows; }
			set { pEnableTrendArrows = value; }
		}
		private int pTrendSeparation = 4;
		[Description("Distance, in ticks, between trend change markers and price bar")]
// 		[Category("Trend Markers")]
// [Gui.Design.DisplayNameAttribute("Separation(ticks)")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Separation(ticks)",  GroupName = "Trend Markers")]
		public int TrendSeparation
		{
			get { return pTrendSeparation; }
			set { pTrendSeparation = Math.Abs(value); }
		}
		private Brush pTrendDownArrowBrush = Brushes.Magenta;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Trend Markers")]
// [Gui.Design.DisplayNameAttribute("Down Arrow Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Down Color",  GroupName = "Trend Markers")]
		public Brush TDAC{	get { return pTrendDownArrowBrush; }	set { pTrendDownArrowBrush = value; }		}
		[Browsable(false)]
		public string TDAClSerialize
		{	get { return Serialize.BrushToString(pTrendDownArrowBrush); } set { pTrendDownArrowBrush = Serialize.StringToBrush(value); }
		}
		private Brush pTrendUpArrowBrush = Brushes.Lime;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Trend Markers")]
// [Gui.Design.DisplayNameAttribute("Up Arrow Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Up Color",  GroupName = "Trend Markers")]
		public Brush TUAC{	get { return pTrendUpArrowBrush; }	set { pTrendUpArrowBrush = value; }		}
		[Browsable(false)]
		public string TUAClSerialize
		{	get { return Serialize.BrushToString(pTrendUpArrowBrush); } set { pTrendUpArrowBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		#region Trend Background
		private Brush pTrendUpBackgroundBrush = Brushes.Lime;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when an upward trend reversal occurs?")]
// 		[Category("Trend Background")]
// [Gui.Design.DisplayNameAttribute("Bkgrnd Up Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Bkgrnd Up Color",  GroupName = "Trend Background")]
		public Brush UpTrendBC{	get { return pTrendUpBackgroundBrush; }	set { pTrendUpBackgroundBrush = value; }		}
		[Browsable(false)]
		public string UpTrendBClSerialize
		{	get { return Serialize.BrushToString(pTrendUpBackgroundBrush); } set { pTrendUpBackgroundBrush = Serialize.StringToBrush(value); }
		}
		private Brush pTrendDownBackgroundBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when a downward trend reversal occurs?")]
// 		[Category("Trend Background")]
// [Gui.Design.DisplayNameAttribute("Bkgrnd Down Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Bkgrnd Down Color",  GroupName = "Trend Background")]
		public Brush DownTrendBC{	get { return pTrendDownBackgroundBrush; }	set { pTrendDownBackgroundBrush = value; }		}
		[Browsable(false)]
		public string DownTrendBClSerialize
		{	get { return Serialize.BrushToString(pTrendDownBackgroundBrush); } set { pTrendDownBackgroundBrush = Serialize.StringToBrush(value); }
		}
		private int pTrendBackgroundOpacity = 3;
		[Description("Opacity (0-10) for Bkgrnd colors.  Set to '0' to turn-off coloring")]
// [Gui.Design.DisplayNameAttribute("Opacity Bkgnd")]
// 		[Category("Trend Background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Opacity Bkgnd",  GroupName = "Trend Background")]
		public int TrendBackgroundOpacity
		{
			get { return pTrendBackgroundOpacity; }
			set { pTrendBackgroundOpacity = value; }
		}
		private bool pTrendBkgColorAllPanels = false;
		[Description("Change the background color of all panels?...if false, then just the indicator panel")]
// [Gui.Design.DisplayNameAttribute("All panels?")]
// 		[Category("Trend Background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "All panels?",  GroupName = "Trend Background")]
		public bool TrendBkgColorAllPanels
		{
			get { return pTrendBkgColorAllPanels; }
			set { pTrendBkgColorAllPanels = value; }
		}
		#endregion

		#region OBOS Alerts
		private double pOBlevel = 80;
		[Description("OB level")]
		[Category("OBOS Alerts")]
		public double Level_OB
		{
			get { return pOBlevel; }
			set { pOBlevel = value; }
		}
		private double pOSlevel = 20;
		[Description("OS level")]
		[Category("OBOS Alerts")]
		public double Level_OS
		{
			get { return pOSlevel; }
			set { pOSlevel = value; }
		}
		private string pOBSound = "<enter wav name>";
        [Description("Sound when oscillator is in overbought area...leave blank to turn-off this alert sound")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("OBOS Alerts")]
        public string OB_Sound
        {
            get { return pOBSound; }
            set { pOBSound = value; }
        }

		private string pOSSound = "<enter wav name>";
        [Description("Sound when oscillator is in oversold area...leave blank to turn-off this alert sound")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Category("OBOS Alerts")]
        public string OS_Sound
        {
            get { return pOSSound; }
            set { pOSSound = value; }
        }
		private string pEmailAddress_IntoOS = "";
		[Description("Supply your address (e.g. 'you@mail.com') for 'IntoOS' alerts, leave blank to turn-off emails for that signal")]
		[Category("OBOS Alerts")]
		public string Email_IntoOS
		{
			get { return pEmailAddress_IntoOS; }
			set { pEmailAddress_IntoOS = value; }
		}
		private string pEmailAddress_IntoOB = "";
		[Description("Supply your address (e.g. 'you@mail.com') for 'IntoOB' alerts, leave blank to turn-off emails for that signal")]
		[Category("OBOS Alerts")]
		public string Email_IntoOB
		{
			get { return pEmailAddress_IntoOB; }
			set { pEmailAddress_IntoOB = value; }
		}
		private string pEmailAddress_ExitingOS = "";
		[Description("Supply your address (e.g. 'you@mail.com') for 'ExitingOS' alerts, leave blank to turn-off emails for that signal")]
		[Category("OBOS Alerts")]
		public string Email_ExitingOS
		{
			get { return pEmailAddress_ExitingOS; }
			set { pEmailAddress_ExitingOS = value; }
		}
		private string pEmailAddress_ExitingOB = "";
		[Description("Supply your address (e.g. 'you@mail.com') for 'ExitingOB' alerts, leave blank to turn-off emails for that signal")]
		[Category("OBOS Alerts")]
		public string Email_ExitingOB
		{
			get { return pEmailAddress_ExitingOB; }
			set { pEmailAddress_ExitingOB = value; }
		}
		private bool pPopupOnOBOS = false;
		[Description("Launch a popup window when the SignalLine enters OB or OS (only reset when the SignalLine moves out of the OB or OS area)")]
		[Category("OBOS Alerts")]
		public bool PopupOnOBOS
		{
			get { return pPopupOnOBOS; }
			set { pPopupOnOBOS = value; }
		}
		#endregion

		#region OBOS Markers
		private UltimateRSI_VisualType pOBOSVisualType = UltimateRSI_VisualType.Triangle;
		[Description("Type of visual signal")]
// 		[Category("OBOS Markers")]
// [Gui.Design.DisplayNameAttribute("Visual Type")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Visual Type",  GroupName = "OBOS Markers")]
		public UltimateRSI_VisualType OBOSVisualType
		{
			get { return pOBOSVisualType; }
			set { pOBOSVisualType = value; }
		}
		private bool pEnableOBArrows = true;
		[Description("Enable arrows when OB condition exist")]
		[Category("OBOS Markers")]
		public bool OB_Enabled
		{
			get { return pEnableOBArrows; }
			set { pEnableOBArrows = value; }
		}
		private bool pEnableOSArrows = true;
		[Description("Enable arrows when OS condition exist")]
		[Category("OBOS Markers")]
		public bool OS_Enabled
		{
			get { return pEnableOSArrows; }
			set { pEnableOSArrows = value; }
		}
		private bool pReverseOBOSArrowDirection = false;
		[Description("Change direction of arrows and triangles")]
// 		[Category("OBOS Markers")]
// [Gui.Design.DisplayNameAttribute("Reverse Arrow Direction?")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Reverse Arrow Direction?",  GroupName = "OBOS Markers")]
		public bool Reverse_OBOSArrowDirection
		{
			get { return pReverseOBOSArrowDirection; }
			set { pReverseOBOSArrowDirection = value; }
		}
		private int pOBOSSeparation = 1;
		[Description("Distance, in ticks, between OBOS markers and price bar")]
// 		[Category("OBOS Markers")]
// [Gui.Design.DisplayNameAttribute("Separation(ticks)")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Separation(ticks)",  GroupName = "OBOS Markers")]
		public int OBOSSeparation
		{
			get { return pOBOSSeparation; }
			set { pOBOSSeparation = Math.Abs(value); }
		}
		private Brush pOBArrowBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("")]
// 		[Category("OBOS Markers")]
// [Gui.Design.DisplayNameAttribute("OB Marker Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "OB Marker Color",  GroupName = "OBOS Markers")]
		public Brush OBAC{	get { return pOBArrowBrush; }	set { pOBArrowBrush = value; }		}
		[Browsable(false)]
		public string OBAClSerialize
		{	get { return Serialize.BrushToString(pOBArrowBrush); } set { pOBArrowBrush = Serialize.StringToBrush(value); }
		}
		private Brush pOSArrowBrush = Brushes.Blue;
		[XmlIgnore()]
		[Description("")]
// 		[Category("OBOS Markers")]
// [Gui.Design.DisplayNameAttribute("OS Marker Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "OS Marker Color",  GroupName = "OBOS Markers")]
		public Brush OSAC{	get { return pOSArrowBrush; }	set { pOSArrowBrush = value; }		}
		[Browsable(false)]
		public string OSAClSerialize
		{	get { return Serialize.BrushToString(pOSArrowBrush); } set { pOSArrowBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		#region OBOS Background
		private Brush pBkgBrushTinted_OB = Brushes.Pink;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when an OB condition exists?")]
// 		[Category("OBOS Background")]
// [Gui.Design.DisplayNameAttribute("OB Bkng Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "OB Bkng Color",  GroupName = "OBOS Background")]
		public Brush OBBC{	get { return pBkgBrushTinted_OB; }	set { pBkgBrushTinted_OB = value; }		}
		[Browsable(false)]
		public string OBBClSerialize
		{	get { return Serialize.BrushToString(pBkgBrushTinted_OB); } set { pBkgBrushTinted_OB = Serialize.StringToBrush(value); }
		}
		private Brush pBkgBrushTinted_OS = Brushes.Olive;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when an OS condition exists?")]
// 		[Category("OBOS Background")]
// [Gui.Design.DisplayNameAttribute("OS Bkng Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "OS Bkng Color",  GroupName = "OBOS Background")]
		public Brush OSBC{	get { return pBkgBrushTinted_OS; }	set { pBkgBrushTinted_OS = value; }		}
		[Browsable(false)]
		public string OSBClSerialize
		{	get { return Serialize.BrushToString(pBkgBrushTinted_OS); } set { pBkgBrushTinted_OS = Serialize.StringToBrush(value); }
		}

		private int pOpacityOB = 5;
        [Description("0 is fully transparent, 10 is fully opaque")]
		[Category("OBOS Background")]
        public int OpacityOB
        {
            get { return pOpacityOB; }
            set { pOpacityOB = Math.Max(0, Math.Min(10,value)); }
        }
		private int pOpacityOS = 5;
        [Description("0 is fully transparent, 10 is fully opaque")]
		[Category("OBOS Background")]
        public int OpacityOS
        {
            get { return pOpacityOS; }
            set { pOpacityOS = Math.Max(0, Math.Min(10,value)); }
        }
		private bool pOBOSBkgColorAllPanels = false;
		[Description("Change the background color of all panels?...if false, then just the indicator panel")]
// [Gui.Design.DisplayNameAttribute("All panels?")]
// 		[Category("OBOS Background")]
[Display(ResourceType = typeof(Custom.Resource), Name = "All panels?",  GroupName = "OBOS Background")]
		public bool OBOSBkgColorAllPanels
		{
			get { return pOBOSBkgColorAllPanels; }
			set { pOBOSBkgColorAllPanels = value; }
		}
		#endregion

		#region Centerline Alerts
		private string pCenterlineDownSound = "<enter wav name>";
        [Description("Sound when RSI crosses below the centerline...leave blank to turn-off this alert sound")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Sound",  GroupName = "Centerline Alerts")]
        public string CenterlineDownSound
        {
            get { return pCenterlineDownSound; }
            set { pCenterlineDownSound = value; }
        }

		private string pCenterlineUpSound = "<enter wav name>";
        [Description("Sound when RSI crosses above the centerline...leave blank to turn-off this alert sound")]
		[RefreshProperties(RefreshProperties.All)]
		[TypeConverter(typeof(LoadSoundFileList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Sound",  GroupName = "Centerline Alerts")]
        public string CenterlineUpSound
        {
            get { return pCenterlineUpSound; }
            set { pCenterlineUpSound = value; }
        }

		private string pEmailAddress_Center = "";
		[Description("Supply your address (e.g. 'you@mail.com') for centerline crosses, leave blank to turn-off emails for that signal")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Email address",  GroupName = "Centerline Alerts")]
		public string EmailAddress_Center
		{
			get { return pEmailAddress_Center; }
			set { pEmailAddress_Center = value; }
		}
		private bool pPopupOnCenterlineDown = false;
		[Description("Launch a popup window when the RSI crosses the Centerline in an downward direction")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enable Down Popup?",  GroupName = "Centerline Alerts")]
		public bool PopupOnCenterlineDown
		{
			get { return pPopupOnCenterlineDown; }
			set { pPopupOnCenterlineDown = value; }
		}
		private bool pPopupOnCenterlineUp = false;
		[Description("Launch a popup window when the RSI crosses the Centerline in an upward direction")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Enable Up Popup?",  GroupName = "Centerline Alerts")]
		public bool PopupOnCenterlineUp
		{
			get { return pPopupOnCenterlineUp; }
			set { pPopupOnCenterlineUp = value; }
		}
		#endregion

		#region Centerline Markers
		private UltimateRSI_VisualType pCenterlineVisualType = UltimateRSI_VisualType.Triangle;
		[Description("Type of visual signal")]
// 		[Category("Centerline Markers")]
// [Gui.Design.DisplayNameAttribute("Visual Type")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Visual Type",  GroupName = "Centerline Markers")]
		public UltimateRSI_VisualType CenterlineVisualType
		{
			get { return pCenterlineVisualType; }
			set { pCenterlineVisualType = value; }
		}
		private bool pEnableCenterlineUpArrows = true;
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Enabled",  GroupName = "Centerline Markers")]
		[Description("Enable arrows when an upward cross of the centerline exists")]
		public bool CenterlineUp_Enabled
		{
			get { return pEnableCenterlineUpArrows; }
			set { pEnableCenterlineUpArrows = value; }
		}
		private bool pEnableCenterlineDownArrows = true;
		[Description("Enable arrows when a downward cross of the centerline exists")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Enabled",  GroupName = "Centerline Markers")]
		public bool CenterlineDown_Enabled
		{
			get { return pEnableCenterlineDownArrows; }
			set { pEnableCenterlineDownArrows = value; }
		}
		private int pCenterlineSeparation = 1;
		[Description("Distance, in ticks, between Centerline markers and price bar")]
// 		[Category("Centerline Markers")]
// [Gui.Design.DisplayNameAttribute("Separation(ticks)")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Separation(ticks)",  GroupName = "Centerline Markers")]
		public int CenterlineSeparation
		{
			get { return pCenterlineSeparation; }
			set { pCenterlineSeparation = Math.Abs(value); }
		}
		private Brush pCenterlineUpArrowBrush = Brushes.Blue;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Centerline Markers")]
// [Gui.Design.DisplayNameAttribute("CenterlineUp Marker Color")]
[Display(ResourceType = typeof(Custom.Resource), Name = "Up Marker Color",  GroupName = "Centerline Markers")]
		public Brush CLUpAC{	get { return pCenterlineUpArrowBrush; }	set { pCenterlineUpArrowBrush = value; }		}
		[Browsable(false)]
		public string CLUpAClSerialize
		{	get { return Serialize.BrushToString(pCenterlineUpArrowBrush); } set { pCenterlineUpArrowBrush = Serialize.StringToBrush(value); }
		}
		private Brush pCenterlineDownArrowBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("")]
// 		[Category("Centerline Markers")]
// [Gui.Design.DisplayNameAttribute("CenterlineDown Marker Color")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Marker Color",  GroupName = "Centerline Markers")]
		public Brush CLDnAC{	get { return pCenterlineDownArrowBrush; }	set { pCenterlineDownArrowBrush = value; }		}
		[Browsable(false)]
		public string CLDnAClSerialize
		{	get { return Serialize.BrushToString(pCenterlineDownArrowBrush); } set { pCenterlineDownArrowBrush = Serialize.StringToBrush(value); }
		}
		#endregion

		#region Centerline Background
		private Brush pCenterlineUp_BackgroundBrush = Brushes.Blue;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when an upward cross of the centerline occurs?")]
// 		[Category("Centerline Background")]
// [Gui.Design.DisplayNameAttribute("CL Up Bkng")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Background",  GroupName = "Centerline Background")]
		public Brush CenterlineUpBC{	get { return pCenterlineUp_BackgroundBrush; }	set { pCenterlineUp_BackgroundBrush = value; }		}
		[Browsable(false)]
		public string CLUpBClSerialize
		{	get { return Serialize.BrushToString(pCenterlineUp_BackgroundBrush); } set { pCenterlineUp_BackgroundBrush = Serialize.StringToBrush(value); }
		}
		private Brush pCenterlineDown_BackgroundBrush = Brushes.Red;
		[XmlIgnore()]
		[Description("Colorize the background of the chart when an downward cross of the centerline occurs?")]
// 		[Category("Centerline Background")]
// [Gui.Design.DisplayNameAttribute("CL Down Bkng")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Background",  GroupName = "Centerline Background")]
		public Brush CenterlineDownBC{	get { return pCenterlineDown_BackgroundBrush; }	set { pCenterlineDown_BackgroundBrush = value; }		}
		[Browsable(false)]
		public string CLDownBClSerialize
		{	get { return Serialize.BrushToString(pCenterlineDown_BackgroundBrush); } set { pCenterlineDown_BackgroundBrush = Serialize.StringToBrush(value); }
		}

		private int pOpacityCenterlineUp = 5;
        [Description("0 is fully transparent, 10 is fully opaque")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Up Opacity",  GroupName = "Centerline Background")]
        public int OpacityCenterlineUp
        {
            get { return pOpacityCenterlineUp; }
            set { pOpacityCenterlineUp = Math.Max(0, Math.Min(10,value)); }
        }
		private int pOpacityCenterlineDown = 5;
        [Description("0 is fully transparent, 10 is fully opaque")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Down Opacity",  GroupName = "Centerline Background")]
        public int OpacityCenterlineDown
        {
            get { return pOpacityCenterlineDown; }
            set { pOpacityCenterlineDown = Math.Max(0, Math.Min(10,value)); }
        }
		private bool pCenterlineBkgColorAllPanels = false;
		[Description("Change the background color of all panels?...if false, then just the indicator panel")]
// [Gui.Design.DisplayNameAttribute("All panels?")]
// 		[Category("Centerline Background")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "All panels?",  GroupName = "Centerline Background")]
		public bool CenterlineBkgColorAllPanels
		{
			get { return pCenterlineBkgColorAllPanels; }
			set { pCenterlineBkgColorAllPanels = value; }
		}
		#endregion

		#endregion
	}
}

public enum UltimateRSI_VisualType {
	Arrow, Triangle, Dot, Square, None
}
public enum UltimateRSI_CalcType {
	Cutlers,Wilders,Exponential
}
public enum UltimateRSI_SignalBasis{TheRSI,TheAvg,None}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UltimateRSI[] cacheUltimateRSI;
		public UltimateRSI UltimateRSI(UltimateRSI_CalcType calcType, int rSIPeriod, int avgPeriod, UltimateRSI_SignalBasis signalBasis)
		{
			return UltimateRSI(Input, calcType, rSIPeriod, avgPeriod, signalBasis);
		}

		public UltimateRSI UltimateRSI(ISeries<double> input, UltimateRSI_CalcType calcType, int rSIPeriod, int avgPeriod, UltimateRSI_SignalBasis signalBasis)
		{
			if (cacheUltimateRSI != null)
				for (int idx = 0; idx < cacheUltimateRSI.Length; idx++)
					if (cacheUltimateRSI[idx] != null && cacheUltimateRSI[idx].CalcType == calcType && cacheUltimateRSI[idx].RSIPeriod == rSIPeriod && cacheUltimateRSI[idx].AvgPeriod == avgPeriod && cacheUltimateRSI[idx].SignalBasis == signalBasis && cacheUltimateRSI[idx].EqualsInput(input))
						return cacheUltimateRSI[idx];
			return CacheIndicator<UltimateRSI>(new UltimateRSI(){ CalcType = calcType, RSIPeriod = rSIPeriod, AvgPeriod = avgPeriod, SignalBasis = signalBasis }, input, ref cacheUltimateRSI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UltimateRSI UltimateRSI(UltimateRSI_CalcType calcType, int rSIPeriod, int avgPeriod, UltimateRSI_SignalBasis signalBasis)
		{
			return indicator.UltimateRSI(Input, calcType, rSIPeriod, avgPeriod, signalBasis);
		}

		public Indicators.UltimateRSI UltimateRSI(ISeries<double> input , UltimateRSI_CalcType calcType, int rSIPeriod, int avgPeriod, UltimateRSI_SignalBasis signalBasis)
		{
			return indicator.UltimateRSI(input, calcType, rSIPeriod, avgPeriod, signalBasis);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UltimateRSI UltimateRSI(UltimateRSI_CalcType calcType, int rSIPeriod, int avgPeriod, UltimateRSI_SignalBasis signalBasis)
		{
			return indicator.UltimateRSI(Input, calcType, rSIPeriod, avgPeriod, signalBasis);
		}

		public Indicators.UltimateRSI UltimateRSI(ISeries<double> input , UltimateRSI_CalcType calcType, int rSIPeriod, int avgPeriod, UltimateRSI_SignalBasis signalBasis)
		{
			return indicator.UltimateRSI(input, calcType, rSIPeriod, avgPeriod, signalBasis);
		}
	}
}

#endregion
