//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.Globalization;
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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Code;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using NinjaTrader.NinjaScript.Indicators.THos;
#endregion


// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public enum TickHunterBreakEvenAutoTypes
    {
        Disabled = 0,
        Enabled = 1,
        PlusTrail1Bar = 2,
        PlusTrail2Bar = 3,
        PlusTrail3Bar = 4,
        PlusTrail5Bar = 5,
        PlusTrailMovingAverage1 = 6,
        PlusTrailMovingAverage2 = 7,
        PlusTrailMovingAverage3 = 8
    };
    /// <summary>
    /// Tick Hunter
    /// </summary>
    public class TickHunter : Indicator
    {
        private const string SystemVersion = "v1.141";
        private const string SystemName = "TickHunter";
        private const string SignalName = "TickHunter";
        private Account account = null;
        private NinjaTrader.Gui.Tools.AccountSelector accountSelector = null;
        private System.Windows.Threading.DispatcherTimer timer;
        private double lastAccountBalance = 0;
        private DateTime lastOrderOutputTime = DateTime.MinValue;
        private bool hasRanOnceFirstCycle = false;
        private bool hasDrawnButtons = false;
        private bool accountHadPositions = false;
        private RealLogger RealLogger = new RealLogger(SystemName);
        private RealInstrumentService RealInstrumentService = new RealInstrumentService();
        private RealTradeService RealTradeService = new RealTradeService();
        private RealPositionService RealPositionService = new RealPositionService();
        private RealOrderService RealOrderService = new RealOrderService();
        private readonly object ClosePositionLock = new object();
        private readonly object NewPositionLock = new object();
        private readonly object MarketOrderLock = new object();
        private readonly object PendingOrderDelayLock = new object();
        private DateTime lastCancelledPendingOrder = DateTime.MinValue;

        private System.Windows.Controls.Grid buttonGrid = null;
        private System.Windows.Controls.Button toggleAutoButton = null;
        private System.Windows.Controls.Button closeAllButton = null;
        private System.Windows.Controls.Button toggleECAButton = null;
        private System.Windows.Controls.Button BEButton = null;
        private System.Windows.Controls.Button SLButton = null;
        private System.Windows.Controls.Button TPButton = null;
        private System.Windows.Controls.Button revButton = null;
        private System.Windows.Controls.Button BuySnapButton = null;
        private System.Windows.Controls.Button SellSnapButton = null;
        private System.Windows.Controls.Button BuyPopButton = null;
        private System.Windows.Controls.Button SellPopButton = null;
        private System.Windows.Controls.Label riskInfoLabel = null;
        private const string ToggleAutoBEButtonEnabledText = "ABE+";
        private const string ToggleAutoBET5BButtonEnabledText = "AB+T5B";
        private const string ToggleAutoBET3BButtonEnabledText = "AB+T3B";
        private const string ToggleAutoBET2BButtonEnabledText = "AB+T2B";
        private const string ToggleAutoBET1BButtonEnabledText = "AB+T1B";
        private const string ToggleAutoBETM3ButtonEnabledText = "AB+TM3";
        private const string ToggleAutoBETM2ButtonEnabledText = "AB+TM2";
        private const string ToggleAutoBETM1ButtonEnabledText = "AB+TM1";
        private const string ToggleAutoBEButtonDisabledText = "ABE-";
        private const string ToggleECAButtonEnabledText = "ECATP+";
        private const string ToggleECAButtonDisabledText = "ECATP-";
        private const string HHToggleAutoButtonName = "HHToggleAutoButton";
        private const string HHCloseAllButtonName = "HHCloseAllButton";
        private const string HHToggleECAButtonName = "HHToggleECAButton";
        private const string HHBEButtonName = "HHBEButton";
        private const string HHSLButtonName = "HHSLButton";
        private const string HHTPButtonName = "HHTPButton";
        private const string HHRevButtonName = "HHRevButton";
        private const string HHBuySnapButtonName = "HHBSnapButton";
        private const string HHSellSnapButtonName = "HHSSnapButton";
        private const string HHBuyPopButtonName = "HHBPopButton";
        private const string HHSellPopButtonName = "HHSPopButton";
        private const string HHRiskInfoLabelName = "HHRILabel";
        private bool isECATPEnabled = false;
        private TickHunterBreakEvenAutoTypes currentBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.Disabled;
        private TickHunterBreakEvenAutoTypes nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.Disabled;
        private DateTime lastBreakEvenAutoChangeTime = DateTime.MinValue;
        private const int BeakEvenColorDelaySeconds = 5;


        private string HedgehogEntrySymbol1FullName = "";
        private string HedgehogEntrySymbol2FullName = "";

        private Instrument attachedInstrument = null;
        private bool attachedInstrumentIsFuture = false;
        private bool attachedInstrumentServerSupported = false;
        private double attachedInstrumentTickSize = 0;
        private int attachedInstrumentTicksPerPoint = 0;

        private EMA breakEvenMA1Buffer;
        private EMA breakEvenMA2Buffer;
        private EMA breakEvenMA3Buffer;

        private double breakEvenMA1Value;
        private double breakEvenMA2Value;
        private double breakEvenMA3Value;

        private Instrument micro1Instrument = null;
        private Instrument micro2Instrument = null;
        private Instrument micro3Instrument = null;
        private Instrument micro4Instrument = null;
        private Instrument emini1Instrument = null;
        private Instrument emini2Instrument = null;
        private Instrument emini3Instrument = null;
        private Instrument emini4Instrument = null;
        private bool instrumentsSubscribed = false;
        private double mymLastAsk = 0;
        private double mymLastBid = 0;
        private double mesLastAsk = 0;
        private double mesLastBid = 0;
        private double m2kLastAsk = 0;
        private double m2kLastBid = 0;
        private double mnqLastAsk = 0;
        private double mnqLastBid = 0;
        private const string MYMPrefix = "MYM";
        private const string MESPrefix = "MES";
        private const string M2KPrefix = "M2K";
        private const string MNQPrefix = "MNQ";

        private double ymLastAsk = 0;
        private double ymLastBid = 0;
        private double esLastAsk = 0;
        private double esLastBid = 0;
        private double rtyLastAsk = 0;
        private double rtyLastBid = 0;
        private double nqLastAsk = 0;
        private double nqLastBid = 0;
        private const string YMPrefix = "YM";
        private const string ESPrefix = "ES";
        private const string RTYPrefix = "RTY";
        private const string NQPrefix = "NQ";

        private double maxDDInDollars = 0;
        private double equityRemainingInDollars = 0;

        private bool riskInfoHasPosition = false;
        private MarketPosition riskInfoMarketPosition = MarketPosition.Flat;
        private int riskInfoQuanitiy = 0;
        private double riskInfoPositionPrice = 0;
        private double riskInfoPositionStopLossPrice = 0;

        private bool isInReplayMode = false;
        private ATR atrBuffer;
        private double atrValue = 0;

        private double previous1LowPrice = 0;
        private double previous2LowPrice = 0;
        private double previous3LowPrice = 0;
        private double previous4LowPrice = 0;
        private double previous5LowPrice = 0;
        private double previous1HighPrice = 0;
        private double previous2HighPrice = 0;
        private double previous3HighPrice = 0;
        private double previous4HighPrice = 0;
        private double previous5HighPrice = 0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = SystemName;
                Description = SystemName + " " + SystemVersion;
                Calculate = Calculate.OnPriceChange;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                IsOverlay = true;
                IsChartOnly = true;
                IsSuspendedWhileInactive = false;
                DisplayInDataBox = false;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;


                PrintTo = PrintTo.OutputTab1;

                UsePlayThroughSleepMode = true;
                AutobotEntryQuantity = 1;
                AutobotEntryQuantityMax = 0;
                SingleOrderChunkMaxQuantity = 10;
                SingleOrderChunkMinQuantity = 5;
                SingleOrderChunkDelayMilliseconds = 10;

                UseECATakeProfit = false;
                ECATakeProfitDollarsPerMicroVolume = 5;
                ECATakeProfitDollarsPerEminiVolume = 50;
                ECATakeProfitATRMultiplierPerVolume = 0.5;
                ECAStopLossMaxDDInDollars = 0;
                ECAStopLossEquityRemainingInDollars = 0;

                UseAutoPositionStopLoss = false;
                UseAutoPositionTakeProfit = false;
                AutoPositionBreakEvenType = TickHunterBreakEvenAutoTypes.Disabled;
                StopLossInitialTicks = 20;
                StopLossInitialATRMultiplier = 3;
                StopLossJumpTicks = 2;
                BreakEvenInitialTicks = 4;
                BreakEvenJumpTicks = 2;
                BreakEvenAutoTriggerTicks = 10;
                BreakEvenAutoTriggerATRMultiplier = 1.5;
                BreakEvenAutoTrailMA1Period = 10;
                BreakEvenAutoTrailMA2Period = 21;
                BreakEvenAutoTrailMA3Period = 89;
                TakeProfitInitialTicks = 45;
                TakeProfitInitialATRMultiplier = 2;
                TakeProfitJumpTicks = 20;
                PopInitialTicks = 20;
                PopInitialATRMultiplier = 0.5;
                PopJumpTicks = 2;
                SnapPopContracts = 1;
                UseSnapPositionTPSL = false;
                SnapPaddingTicks = 1;
                ATRPeriod = 21;
                RefreshTPSLPaddingTicks = 5;
                RefreshTPSLOrderDelaySeconds = 2;

                //UseGridEntry = false;
                //GridQuantity = 1;
                //GridOrderCountMax = 10;
                //GridStepTicks = 20;

                UseHedgehogEntry = false;
                HedgehogEntryBuySymbol1SellSymbol2 = true;
                HedgehogEntrySymbol1 = "MES";
                HedgehogEntrySymbol2 = "M2K";


                UseAccountInfoLogging = false;
                AccountInfoLoggingPath = @"C:\MetaTrader\AccountInfo_NT.csv";

                UsePositionProfitLogging = true;
                DebugLogLevel = 0;
                OrderWaitOutputThrottleSeconds = 1;
            }
            else if (State == State.Configure)
            {
                attachedInstrument = this.Instrument;
                attachedInstrumentIsFuture = RealInstrumentService.IsFutureInstrumentType(this.attachedInstrument);
                attachedInstrumentServerSupported = this.Instrument.MasterInstrument.IsServerSupported;
                attachedInstrumentTickSize = RealInstrumentService.GetTickSize(attachedInstrument);
                attachedInstrumentTicksPerPoint = RealInstrumentService.GetTicksPerPoint(attachedInstrumentTickSize);

                isECATPEnabled = (UseECATakeProfit);

                currentBreakEvenAutoStatus = AutoPositionBreakEvenType;

                if (this.ECAStopLossMaxDDInDollars == 0)
                    maxDDInDollars = this.ECAStopLossMaxDDInDollars;
                else
                    maxDDInDollars = this.ECAStopLossMaxDDInDollars * -1;

                equityRemainingInDollars = this.ECAStopLossEquityRemainingInDollars;

                if (attachedInstrumentServerSupported)
                {
                    if (attachedInstrumentIsFuture)
                    {
                        HedgehogEntrySymbol1FullName = HedgehogEntrySymbol1 + GetCurrentFuturesMonthYearPrefix();
                        HedgehogEntrySymbol2FullName = HedgehogEntrySymbol2 + GetCurrentFuturesMonthYearPrefix();

                        string micro1FullName = MYMPrefix + GetCurrentFuturesMonthYearPrefix();
                        string micro2FullName = MESPrefix + GetCurrentFuturesMonthYearPrefix();
                        string micro3FullName = M2KPrefix + GetCurrentFuturesMonthYearPrefix();
                        string micro4FullName = MNQPrefix + GetCurrentFuturesMonthYearPrefix();

                        string emini1FullName = YMPrefix + GetCurrentFuturesMonthYearPrefix();
                        string emini2FullName = ESPrefix + GetCurrentFuturesMonthYearPrefix();
                        string emini3FullName = RTYPrefix + GetCurrentFuturesMonthYearPrefix();
                        string emini4FullName = NQPrefix + GetCurrentFuturesMonthYearPrefix();


                        // BarsArray[0] is default of chart we are on

                        if (this.Instrument.FullName != micro1FullName)
                        {
                            ValidateInstrument(micro1FullName);
                            //AddDataSeries(micro1FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != micro2FullName)
                        {
                            ValidateInstrument(micro2FullName);
                            //AddDataSeries(micro2FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != micro3FullName)
                        {
                            ValidateInstrument(micro3FullName);
                            //AddDataSeries(micro3FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != micro4FullName)
                        {
                            ValidateInstrument(micro4FullName);
                            //AddDataSeries(micro4FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != emini1FullName)
                        {
                            ValidateInstrument(emini1FullName);
                            //AddDataSeries(emini1FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != emini2FullName)
                        {
                            ValidateInstrument(emini2FullName);
                            //AddDataSeries(emini2FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != emini3FullName)
                        {
                            ValidateInstrument(emini3FullName);
                            //AddDataSeries(emini3FullName, BarsPeriodType.Minute, 5);
                        }

                        if (this.Instrument.FullName != emini4FullName)
                        {
                            ValidateInstrument(emini4FullName);
                            //AddDataSeries(emini4FullName, BarsPeriodType.Minute, 5);
                        }

                        micro1Instrument = Instrument.GetInstrument(micro1FullName);
                        micro2Instrument = Instrument.GetInstrument(micro2FullName);
                        micro3Instrument = Instrument.GetInstrument(micro3FullName);
                        micro4Instrument = Instrument.GetInstrument(micro4FullName);

                        emini1Instrument = Instrument.GetInstrument(emini1FullName);
                        emini2Instrument = Instrument.GetInstrument(emini2FullName);
                        emini3Instrument = Instrument.GetInstrument(emini3FullName);
                        emini4Instrument = Instrument.GetInstrument(emini4FullName);

                        if (!instrumentsSubscribed)
                        {
                            if (micro1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro1Instrument, "MarketDataUpdate", MarketData_Update);
                            if (micro2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro2Instrument, "MarketDataUpdate", MarketData_Update);
                            if (micro3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro3Instrument, "MarketDataUpdate", MarketData_Update);
                            if (micro4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro4Instrument, "MarketDataUpdate", MarketData_Update);

                            if (emini1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini1Instrument, "MarketDataUpdate", MarketData_Update);
                            if (emini2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini2Instrument, "MarketDataUpdate", MarketData_Update);
                            if (emini3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini3Instrument, "MarketDataUpdate", MarketData_Update);
                            if (emini4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini4Instrument, "MarketDataUpdate", MarketData_Update);

                            instrumentsSubscribed = true;
                        }
                    }
                }

                atrValue = 0;
                ForceRefresh();
            }
            else if (State == State.DataLoaded)
            {
                RealLogger.PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
                RealLogger.PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

                if (attachedInstrumentServerSupported)
                {
                    hasRanOnceFirstCycle = false;
                    atrBuffer = ATR(ATRPeriod);

                    breakEvenMA1Buffer = EMA(Close, BreakEvenAutoTrailMA1Period);
                    breakEvenMA2Buffer = EMA(Close, BreakEvenAutoTrailMA2Period);
                    breakEvenMA3Buffer = EMA(Close, BreakEvenAutoTrailMA3Period);

                    isInReplayMode = this.Bars.IsInReplayMode;

                    if (BarsInProgress == 0 && ChartControl != null && timer == null)
                    {
                        ChartControl.Dispatcher.InvokeAsync(() =>
                        {
                            timer = new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250), IsEnabled = true };
                            WeakEventManager<System.Windows.Threading.DispatcherTimer, EventArgs>.AddHandler(timer, "Tick", OnTimerTick);
                        });
                    }

                    /*
                    if (IsStrategyAttachedToChart())
                    {
                        if (ChartControl != null)
                        {
                            if (ChartControl.Dispatcher.CheckAccess())
                            {
                                DrawButtonPanel();
                            }
                            else
                            {
                                ChartControl.Dispatcher.InvokeAsync((() =>
                                {

                                   DrawButtonPanel();
                                }));
                            }
                        }
                    }
                    */
                }
            }
            else if (State == State.Terminated)
            {
                hasRanOnceFirstCycle = false;
                hasDrawnButtons = false;

                if (attachedInstrumentServerSupported)
                {
                    UnloadAccountEvents();

                    if (attachedInstrumentIsFuture)
                    {
                        if (instrumentsSubscribed)
                        {
                            if (micro1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro1Instrument, "MarketDataUpdate", MarketData_Update);
                            if (micro2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro2Instrument, "MarketDataUpdate", MarketData_Update);
                            if (micro3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro3Instrument, "MarketDataUpdate", MarketData_Update);
                            if (micro4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro4Instrument, "MarketDataUpdate", MarketData_Update);

                            if (emini1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini1Instrument, "MarketDataUpdate", MarketData_Update);
                            if (emini2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini2Instrument, "MarketDataUpdate", MarketData_Update);
                            if (emini3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini3Instrument, "MarketDataUpdate", MarketData_Update);
                            if (emini4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini4Instrument, "MarketDataUpdate", MarketData_Update);
                            instrumentsSubscribed = false;
                        }
                    }

                    if (ChartControl != null && timer != null)
                    {
                        ChartControl.Dispatcher.InvokeAsync(() =>
                        {
                            WeakEventManager<System.Windows.Threading.DispatcherTimer, EventArgs>.RemoveHandler(timer, "Tick", OnTimerTick);
                            timer = null;
                        });
                    }

                    if (ChartControl != null)
                    {
                        if (ChartControl.Dispatcher.CheckAccess())
                        {
                            RemoveButtonPanel();
                        }
                        else
                        {
                            ChartControl.Dispatcher.InvokeAsync((() =>
                            {
                                RemoveButtonPanel();
                            }));
                        }
                    }
                }
            }
        }


        //public override void CloseStrategy(string signalName)
        //{


        // base.CloseStrategy(signalName);
        //}



        protected override void OnBarUpdate()
        {
            if (attachedInstrumentServerSupported)
            {
                RefreshAccount();

                if (CurrentBar > 5)
                {
                    previous1HighPrice = High[1];
                    previous2HighPrice = High[2];
                    previous3HighPrice = High[3];
                    previous4HighPrice = High[4];
                    previous5HighPrice = High[5];
                    previous1LowPrice = Low[1];
                    previous2LowPrice = Low[2];
                    previous3LowPrice = Low[3];
                    previous4LowPrice = Low[4];
                    previous5LowPrice = Low[5];
                }

                if (CurrentBar > BreakEvenAutoTrailMA3Period)
                {
                    breakEvenMA1Value = breakEvenMA1Buffer[1];
                    breakEvenMA2Value = breakEvenMA2Buffer[1];
                    breakEvenMA3Value = breakEvenMA3Buffer[1];
                }


                if (StopLossInitialATRMultiplier > 0 || TakeProfitInitialATRMultiplier > 0 || ECATakeProfitATRMultiplierPerVolume > 0)
                {
                    if (CurrentBar > ATRPeriod)
                    {
                        atrValue = atrBuffer[0];
                    }
                }

                RefreshObjects();


                //RealLogger.PrintOutput("atrValue[0]=" + atrValue[0].ToString());
            }
        }

        private void RefreshObjects()
        {
            if (lastBreakEvenAutoChangeTime != DateTime.MinValue && ChartControl != null)
            {
                bool fullyEnabled = lastBreakEvenAutoChangeTime <= GetDateTimeNow();

                if (fullyEnabled)
                {
                    if (toggleAutoButton != null)
                    {
                        currentBreakEvenAutoStatus = nextBreakEvenAutoStatus;
                        lastBreakEvenAutoChangeTime = DateTime.MinValue;
                        nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.Disabled;
                        ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                        {
                            toggleAutoButton.Background = Brushes.HotPink;
                        }));
                        RealLogger.PrintOutput("Activated break-even auto type " + currentBreakEvenAutoStatus);
                    }
                }
            }
        }


        private void RefreshAccount()
        {
            if (hasRanOnceFirstCycle)
            {
                Account tempAccount = GetAccount();
                if (account != null & tempAccount != account)
                {
                    hasRanOnceFirstCycle = false;
                }
            }
        }

        //protected override void OnMarketData(MarketDataEventArgs e)
        private void MarketData_Update(object sender, MarketDataEventArgs e)
        {
            bool newBidAsk = false;

            if (e.Instrument != null)
            {
                double lastPrice = 0;
                double newPrice = 0;
                if (e.MarketDataType == MarketDataType.Ask)
                {
                    lastPrice = RealInstrumentService.GetAskPrice(e.Instrument);
                    newPrice = e.Ask;

                    if (lastPrice != newPrice)
                    {
                        newBidAsk = true;
                        RealInstrumentService.SetAskPrice(e.Instrument, newPrice);
                    }
                }
                else if (e.MarketDataType == MarketDataType.Bid)
                {
                    lastPrice = RealInstrumentService.GetBidPrice(e.Instrument);
                    newPrice = e.Bid;

                    if (lastPrice != newPrice)
                    {
                        newBidAsk = true;
                        RealInstrumentService.SetBidPrice(e.Instrument, newPrice);
                    }
                }
                else if (e.MarketDataType == MarketDataType.Last)
                {
                    lastPrice = RealInstrumentService.GetLastPrice(e.Instrument);
                    newPrice = e.Last;

                    if (lastPrice != newPrice)
                    {
                        //newBidAsk = true;
                        RealInstrumentService.SetLastPrice(e.Instrument, newPrice);
                    }
                }
            }

            if (HasRanOnceFirstCycle() && account != null && newBidAsk)
            {
                if (!HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
                {
                    AttemptToClosePositionsInProfit();
                    AttemptToClosePositionsInLoss();



                    HandleSLTPRefresh("MarketDataChange");


                    if (accountHadPositions && IsAccountFlat() && !HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
                    {
                        RealLogger.PrintOutput("Account is flat...", PrintTo.OutputTab1);
                        accountHadPositions = false;
                    }

                    AttemptToEngageAutobot();

                    if (!IsAccountFlat()) accountHadPositions = true;
                }
                else
                {
                    bool readyOutputWithThrottle = (lastOrderOutputTime == DateTime.MinValue || lastOrderOutputTime >= (GetDateTimeNow()).AddSeconds(OrderWaitOutputThrottleSeconds));
                    if (readyOutputWithThrottle)
                    {
                        if (RealOrderService.InFlightOrderCache.HasElements())
                            RealLogger.PrintOutput("Waiting on " + RealOrderService.InFlightOrderCache.Count.ToString() + " in flight order(s) to be submitted...", PrintTo.OutputTab1);
                        else
                            RealLogger.PrintOutput("Waiting on active orders to clear...", PrintTo.OutputTab1);

                        lastOrderOutputTime = GetDateTimeNow();
                    }
                }

                AttemptAccountInfoLogging();
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (!hasDrawnButtons)
            {
                if (HasRanOnceFirstCycle())
                {
                    if (hasRanOnceFirstCycle && !hasDrawnButtons)
                    {
                        ForceRefresh();
                    }
                }
            }

        }
        private void ResetPendingOrderDelay()
        {
            lock (PendingOrderDelayLock)
            {
                lastCancelledPendingOrder = DateTime.MinValue;
            }
        }

        private void SetPendingOrderDelay()
        {
            lock (PendingOrderDelayLock)
            {
                lastCancelledPendingOrder = (GetDateTimeNow()).AddSeconds(RefreshTPSLOrderDelaySeconds);
            }
        }

        private DateTime GetDateTimeNow()
        {
            DateTime now;
            if (isInReplayMode)
            {
                now = NinjaTrader.Cbi.Connection.PlaybackConnection.Now;
            }
            else
            {
                now = DateTime.Now;
            }

            return now;
        }
        private bool HasDelayForPendingOrders()
        {
            bool returnFlag = false;

            lock (PendingOrderDelayLock)
            {
                bool delayPendingOrder = (lastCancelledPendingOrder >= GetDateTimeNow());

                if (delayPendingOrder)
                    returnFlag = true;
            }

            return returnFlag;
        }

        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            if (e != null)
            { 

                string keyName = e.Order.Name;
                bool isCancelPendingOrdersActivated = ((e.Order.IsStopMarket || e.Order.IsLimit) && e.Order.OrderState == OrderState.CancelSubmitted);
                bool isFilledPendingOrdersActivated = (!e.Order.IsMarket && (e.Order.OrderState == OrderState.Filled || e.Order.OrderState == OrderState.PartFilled));
                bool isRejected = e.Order.OrderState == OrderState.Rejected;
                //bool isFlat = (e.Order.IsMarket && e.Order.OrderState == OrderState.Filled && e.Order.Quantity == e.Order.Filled);

                if (isRejected)
                {
                    if (DebugLogLevel > 2) RealLogger.PrintOutput("OnOrderUpdate rejected state = " + e.Order.OrderState.ToString() + " name=" + e.Order.Name + " price=" + Math.Max(e.Order.StopPrice, e.Order.LimitPrice).ToString() + " orderaction=" + e.Order.OrderAction.ToString() + " lastPrice=" + RealInstrumentService.GetLastPrice(e.Order.Instrument).ToString() + " bidPrice=" + RealInstrumentService.GetBidPrice(e.Order.Instrument).ToString() + " askPrice=" + RealInstrumentService.GetAskPrice(e.Order.Instrument).ToString() + " instrument=" + e.Order.Instrument.FullName);
                }

                if (isCancelPendingOrdersActivated || isFilledPendingOrdersActivated)
                {
                    //RealLogger.PringOutput("OnOrderUpdate delay state = " + e.Order.OrderState.ToString() + " name=" + e.Order.Name + " quantity=" + e.Order.Quantity + " filled=" + e.Order.Filled + " orderaction=" + e.Order.OrderAction.ToString() + " position=" + Convert.ToString(positionQuantity));
                    if (DebugLogLevel > 2) RealLogger.PrintOutput("OnOrderUpdate delay state = " + e.Order.OrderState.ToString() + " name=" + e.Order.Name + " quantity=" + e.Order.Quantity + " filled=" + e.Order.Filled + " orderaction=" + e.Order.OrderAction.ToString() + " positionCount=" + Convert.ToString(RealPositionService.PositionsCount) + " instrument=" + e.Order.Instrument.FullName);
                    SetPendingOrderDelay();
                }

                if (e.Order.Filled > 0)
                {
                    int filledQuantity = RealOrderService.GetFilledOrderQuantity(e.Order);
                    MarketPosition marketPosition = ConvertOrderActionToMarketPosition(e.Order.OrderAction);
                        
                    RealPosition newPosition = RealPositionService.BuildRealPosition(e.Order.Account, e.Order.Instrument, marketPosition, filledQuantity, e.Order.AverageFillPrice);

                    RealPositionService.AddOrUpdatePosition(newPosition);

                    if (DebugLogLevel > 2)
                    {
                        RealPosition updatedPosition = null;
                        int updatedPositionQuantity = 0;
                        if (RealPositionService.TryGetByInstrumentFullName(e.Order.Instrument.FullName, out updatedPosition))
                        {
                            updatedPositionQuantity = updatedPosition.Quantity;
                        }

                        RealLogger.PrintOutput("OnOrderUpdate order filled state = " + e.Order.OrderState.ToString() + " name=" + e.Order.Name + " quantity=" + e.Order.Quantity + " filled=" + e.Order.Filled + " orderaction=" + e.Order.OrderAction.ToString() + " positionCount=" + Convert.ToString(RealPositionService.PositionsCount) + " filledquan=" + filledQuantity.ToString() + " poQuan=" + updatedPositionQuantity.ToString() + " instrument=" + e.Order.Instrument.FullName);
                    }
                }

                bool isCompletedMarketOrder = (e.Order.IsMarket && Order.IsTerminalState(e.Order.OrderState));
                bool isCompletedStopOrder = (e.Order.IsStopMarket && e.Order.OrderState == OrderState.Accepted);
                bool isCompletedLimitOrder = (e.Order.IsLimit && e.Order.OrderState == OrderState.Working);


                //RealLogger.PrintOutput("OnOrderUpdate order state = " + e.Order.OrderState.ToString() + " name=" + e.Order.Name);
                if ((isCompletedMarketOrder || isCompletedStopOrder || isCompletedLimitOrder || isRejected) && RealOrderService.InFlightOrderCache.Contains(keyName))
                {
                    //RealLogger.PrintOutput("OnOrderUpdate Removing from cache in order state = " + e.Order.OrderState.ToString() + " name=" + e.Order.Name);
                    RealOrderService.InFlightOrderCache.DeregisterUniqueId(keyName);
    
                }
                //RealLogger.PrintOutput("OnOrderUpdate cache = " + RealOrderService.InFlightOrderCache.Count.ToString());
            }
                //RealLogger.PringOutput("OnOrderUpdate Id=" + Convert.ToString(e.Order.Id) + " name=" + Convert.ToString(e.Order.Name) + " quantity=" + Convert.ToString(e.Order.Quantity) + " count = " + Convert.ToString(Account.Orders.Count));
        }

        
        //protected void OnExecutionUpdate(object sender, ExecutionEventArgs e)
        //{
        //    if (e != null)
        //    {
        //        //RealLogger.PringOutput("OnExecutionUpdate Id=" + Convert.ToString(e.OrderId) + " quanity=" + Convert.ToString(e.Quantity) + " count = " + Convert.ToString(Account.Orders.Count));
        //    }
        //}

        private void OnPositionUpdate(object sender, PositionEventArgs e)
        {
            //lock (inFlighOrderCache)
            //{
            //    string keyName = EscapeKeyName(e.Position.Instrument.FullName);
            //    if (inFlighOrderCache.ContainsKey(keyName))
            //    {
                    //inFlighOrderCache.Remove(keyName);
            //    }

                //RealLogger.PringOutput("OnPositionUpdate inFlighOrderCache.Count= " + inFlighOrderCache.Count.ToString());
            //}

            


            // Output the new position
            //NinjaTrader.Code.Output.Process(string.Format("XInstrument: {0} MarketPosition: {1} AveragePrice: {2} Quantity: {3}",
            //e.Position.Instrument.FullName, e.MarketPosition, e.AveragePrice, e.Quantity), PrintTo.OutputTab1);
        }

        private void LoadPositions()
        {
            RealPositionService.LoadPositions(account);
        }

        private string EscapeKeyName(string keyName)
        {
            string newKeyName = keyName.Replace(' ', '_');

            return newKeyName;
        }
        private void RefreshRiskInfoLabel()
        {
            if (riskInfoLabel != null)
            {
                if (riskInfoHasPosition)
                {
                    int riskInfoTicks = 0;
                    double riskInfoDollars = 0;
                    string dollarsText = "";

                    riskInfoDollars = Math.Round(GetPositionProfitWithStoLoss(attachedInstrument, riskInfoMarketPosition, riskInfoQuanitiy, riskInfoPositionPrice, riskInfoPositionStopLossPrice), 2);
                    riskInfoTicks = (int)Math.Round(((riskInfoDollars / riskInfoQuanitiy) / RealInstrumentService.GetTickValue(attachedInstrument)), MidpointRounding.ToEven);

                    if (riskInfoDollars >= 0)
                    {
                        riskInfoLabel.Foreground = Brushes.LimeGreen;
                        riskInfoLabel.Background = Brushes.Black;
                        dollarsText = "$" + riskInfoDollars.ToString("N2");
                    }
                    else
                    {
                        riskInfoLabel.Foreground = Brushes.Red;
                        riskInfoLabel.Background = Brushes.Black;
                        dollarsText = "-$" + riskInfoDollars.ToString("N2").Replace("-", "");

                    }

                    if (riskInfoPositionStopLossPrice == 0)
                    {
                        riskInfoLabel.Foreground = Brushes.White;
                        riskInfoLabel.Background = Brushes.Black;
                        riskInfoLabel.Content = "no sl";
                    }
                    else
                    {
                        riskInfoLabel.Background = Brushes.Black;
                        riskInfoLabel.Content = dollarsText + " (" + riskInfoTicks.ToString("N0") + ")";
                    }
                }
                else
                {
                    riskInfoLabel.Foreground = Brushes.White;
                    riskInfoLabel.Background = Brushes.Transparent;
                    riskInfoLabel.Content = "";
                }
            }
        }

        
        private void OnButtonClick(object sender, RoutedEventArgs re)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

            if (button == revButton && button.Name == HHRevButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("Reverse button clicked");
                string signalName = "ReverseButton";

                //CreatePositionStopLoss(signalName, attachedInstrument, OrderAction.Sell, OrderEntry.Automated, 1, 3824);

//                bool positionFound = HandleReverse(signalName);
				bool positionFound = EnterAtMarket(signalName, OrderAction.Buy);

                if (!positionFound)
                {
                    RealLogger.PrintOutput("Reverse Error: No position found for " + attachedInstrument.FullName.ToString());
                }

                return;
            }
            else if (button == closeAllButton && button.Name == HHCloseAllButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("Flat button clicked");
                string signalName = "FlattenEverythingButton";

                bool positionFound = FlattenEverything(signalName, true);

                if (!positionFound)
                {
                    RealLogger.PrintOutput("Flat Error: No position found for " + attachedInstrument.FullName.ToString());
                }
                return;
            }
            else if (button == toggleECAButton && button.Name == HHToggleECAButtonName)
            {
                if (button.Content.ToString() == ToggleECAButtonEnabledText)
                {
                    button.Content = ToggleECAButtonDisabledText;
                    button.Background = Brushes.DimGray;
                    isECATPEnabled = false;
                }
                else
                {
                    button.Content = ToggleECAButtonEnabledText;
                    button.Background = Brushes.HotPink;
                    isECATPEnabled = true;
                }

                return;
            }
            else if (button == toggleAutoButton && button.Name == HHToggleAutoButtonName)
            {
                if (button.Content.ToString() == ToggleAutoBEButtonEnabledText)
                {
                    button.Content = ToggleAutoBETM3ButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage3;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBETM3ButtonEnabledText)
                {
                    button.Content = ToggleAutoBETM2ButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage2;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBETM2ButtonEnabledText)
                {
                    button.Content = ToggleAutoBETM1ButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage1;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBETM1ButtonEnabledText)
                {
                    button.Content = ToggleAutoBET5BButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrail5Bar;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBET5BButtonEnabledText)
                {
                    button.Content = ToggleAutoBET3BButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrail3Bar;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBET3BButtonEnabledText)
                {
                    button.Content = ToggleAutoBET2BButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrail2Bar;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBET2BButtonEnabledText)
                {
                    button.Content = ToggleAutoBET1BButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.PlusTrail1Bar;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }
                else if (button.Content.ToString() == ToggleAutoBET1BButtonEnabledText)
                {
                    button.Content = ToggleAutoBEButtonDisabledText;
                    button.Background = Brushes.DimGray;
                    currentBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.Disabled;
                    lastBreakEvenAutoChangeTime = DateTime.MinValue;
                }
                else
                {
                    button.Content = ToggleAutoBEButtonEnabledText;
                    button.Background = Brushes.DimGray;
                    nextBreakEvenAutoStatus = TickHunterBreakEvenAutoTypes.Enabled;
                    lastBreakEvenAutoChangeTime = (GetDateTimeNow()).AddSeconds(BeakEvenColorDelaySeconds);
                }

                return;
            }
            else if (button == TPButton && button.Name == HHTPButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("TP+ button clicked");
                string signalName = "TP+ Button";

                bool positionFound = HandleTakeProfitPlus(signalName);

                if (!positionFound)
                {
                    RealLogger.PrintOutput("TP+ Error: No position found for " + attachedInstrument.FullName.ToString());
                }

                return;
            }
            else if (button == BEButton && button.Name == HHBEButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("BE+ button clicked");
                string signalName = "BE+ Button";

                bool positionFound = HandleBreakEvenPlus(signalName);

                if (!positionFound)
                {
                    RealLogger.PrintOutput("BE+ Error: No position found for " + attachedInstrument.FullName.ToString());
                }

                return;
            }
            else if (button == SLButton && button.Name == HHSLButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("SL+ button clicked");
                string signalName = "SL+ Button";

                bool positionFound = HandleStopLossPlus(signalName);

                if (!positionFound)
                {
                    RealLogger.PrintOutput("SL+ Error: No position found for " + attachedInstrument.FullName.ToString());
                }

                return;
            }
            else if (button == BuyPopButton && button.Name == HHBuyPopButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("Buy Pop button clicked");
                string signalName = "Pop+ Button";

                bool positionFound = HandleBuyPop(signalName);

                /*
                if (positionFound)
                {
                    RealLogger.PrintOutput("SNAP+ Error: Not supported position found for " + attachedInstrument.FullName.ToString());
                }
                */

                return;
            }
            else if (button == SellPopButton && button.Name == HHSellPopButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("Sell Pop button clicked");
                string signalName = "Pop- Button";

                bool positionFound = HandleSellPop(signalName);

                /*
                if (positionFound)
                {
                    RealLogger.PrintOutput("SNAP- Error: Not supported when position found for " + attachedInstrument.FullName.ToString());
                }
                */

                return;
            }
            else if (button == BuySnapButton && button.Name == HHBuySnapButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("Buy Snap button clicked");
                string signalName = "Snap+ Button";

                bool positionFound = HandleBuySnap(signalName);

                /*
                if (positionFound)
                {
                    RealLogger.PrintOutput("SNAP+ Error: Not supported position found for " + attachedInstrument.FullName.ToString());
                }
                */

                return;
            }
            else if (button == SellSnapButton && button.Name == HHSellSnapButtonName)
            {
                if (DebugLogLevel > 0) RealLogger.PrintOutput("Sell Snap button clicked");
                string signalName = "Snap- Button";

                bool positionFound = HandleSellSnap(signalName);

                /*
                if (positionFound)
                {
                    RealLogger.PrintOutput("SNAP- Error: Not supported when position found for " + attachedInstrument.FullName.ToString());
                }
                */

                return;
            }
        }

        private void UnloadAccountEvents()
        {
            if (account != null)
            {
                WeakEventManager<Account, OrderEventArgs>.RemoveHandler(account, "OrderUpdate", OnOrderUpdate);
                account = null;
                //WeakEventManager<Account, PositionEventArgs>.RemoveHandler(account, "PositionUpdate", OnPositionUpdate);
            }
        }

        private bool HandleReverse(string signalName)
        {
            bool positionFound = false;

            Instrument tempInstrument = null;
            int tempQuantity = 0;
            MarketPosition tempMarketPosition;
            OrderAction revOrderAction;

            int positionsCount = RealPositionService.PositionsCount;

            for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;

                if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    if (RealPositionService.IsValidPosition(position, attachedInstrument))
                    {
                        position.StoreState();
                        positionFound = true;
                        tempInstrument = position.Instrument;
                        tempQuantity = position.Quantity;
                        tempMarketPosition = position.MarketPosition;
                        revOrderAction = ConvertMarketPositionToRevOrderAction(tempMarketPosition);

                        if (!position.HasStateChanged() && !position.IsFlat())
                        {
                            FlattenEverything(signalName, true);


                            if (DebugLogLevel > 2) RealLogger.PrintOutput(signalName + " opening " + revOrderAction.ToString().ToLower() + " " + tempInstrument.FullName + " Quantity=" + tempQuantity, PrintTo.OutputTab1);
                            SubmitMarketOrder(tempInstrument, revOrderAction, OrderEntry.Automated, tempQuantity);
                        }
                        break; //only one postion per instrument so exit early
                    }
                }
            }


            return positionFound;
        }
        private bool EnterAtMarket(string signalName, OrderAction orderAction)
        {
            bool positionFound = false;

            Instrument tempInstrument = Instrument.GetInstrument(Instrument.FullName);
            int tempQuantity = 0;
            MarketPosition tempMarketPosition;

            int positionsCount = RealPositionService.PositionsCount;

            //for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;

               // if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    //if (RealPositionService.IsValidPosition(position, attachedInstrument))
                    {
						position.StoreState();
						positionFound = true;
						tempInstrument = position.Instrument;
						tempQuantity = position.Quantity;
//						tempMarketPosition = position.MarketPosition;

                        if (!position.HasStateChanged() && !position.IsFlat())
                        {
                            if (DebugLogLevel > 2) RealLogger.PrintOutput(signalName + " opening " + orderAction.ToString().ToLower() + " " + tempInstrument.FullName + " Quantity=" + tempQuantity, PrintTo.OutputTab1);
                            SubmitMarketOrder(tempInstrument, orderAction, OrderEntry.Automated, tempQuantity);
                        }
                        //break; //only one postion per instrument so exit early
                    }
                }
            }


            return positionFound;
        }

        private bool HandleBreakEvenPlus(string signalName)
        {
            double oldStopLossPrice = 0;
            int oldOrderQuantity = 0;
            double newStopLossPrice = 0;
            int tempQuantity = 0;
            bool positionFound = false;
            OrderType orderType = OrderType.Unknown;


            int positionsCount = RealPositionService.PositionsCount;

            for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;

                if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    if (RealPositionService.IsValidPosition(position, attachedInstrument))
                    {
                        position.StoreState();
                        positionFound = true;
                        tempQuantity = position.Quantity;
                        newStopLossPrice = 0;
                        oldStopLossPrice = RealOrderService.GetStopLossInfo(position.Account, position.Instrument, ConvertMarketPositionToSLOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);

                        if (DebugLogLevel > 2) RealLogger.PrintOutput("Current SL price=" + oldStopLossPrice.ToString() + " ticksize=" + attachedInstrumentTickSize.ToString() + " ticks/point=" + attachedInstrumentTicksPerPoint.ToString());
                        if (oldStopLossPrice == 0)
                        {
                            newStopLossPrice = GetInitialStopLossPrice(position.MarketPosition, position.AveragePrice);

                            if (position.MarketPosition == MarketPosition.Long)
                            {
                                double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                if (newStopLossPrice > bidPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }
                            else if (position.MarketPosition == MarketPosition.Short)
                            {
                                double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                if (newStopLossPrice < askPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }


                            if (newStopLossPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                            {
                                OrderAction orderAction = ConvertMarketPositionToSLOrderAction(position.MarketPosition);
                                double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                bool isPriceValid = RealOrderService.IsValidStopLossPrice(position.Instrument, orderAction, newStopLossPrice, lastPrice);

                                if (isPriceValid)
                                {
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("New SL price=" + newStopLossPrice.ToString());
                                    CreatePositionStopLoss(signalName, attachedInstrument, orderAction, OrderEntry.Automated, tempQuantity, newStopLossPrice);
                                }
                            }
                        }
                        else
                        {
                            if (position.MarketPosition == MarketPosition.Long)
                            {
                                newStopLossPrice = GetInitialBreakEvenStopLossPrice(position.MarketPosition, position.AveragePrice);

                                if (newStopLossPrice > position.AveragePrice && oldStopLossPrice >= newStopLossPrice)
                                {
                                    newStopLossPrice = GetStopLossPriceFromJumpTicks(position.MarketPosition, oldStopLossPrice, this.BreakEvenJumpTicks);
                                }

                                double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                if (newStopLossPrice > bidPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }
                            else if (position.MarketPosition == MarketPosition.Short)
                            {
                                newStopLossPrice = GetInitialBreakEvenStopLossPrice(position.MarketPosition, position.AveragePrice);

                                if (newStopLossPrice < position.AveragePrice && oldStopLossPrice <= newStopLossPrice)
                                {
                                    newStopLossPrice = GetStopLossPriceFromJumpTicks(position.MarketPosition, oldStopLossPrice, this.BreakEvenJumpTicks);
                                }

                                double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                if (newStopLossPrice < askPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }

                            if (newStopLossPrice != 0 && oldStopLossPrice != newStopLossPrice && !position.HasStateChanged() && !position.IsFlat())
                            {
                                OrderAction orderAction = ConvertMarketPositionToSLOrderAction(position.MarketPosition);
                                double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                bool isPriceValid = RealOrderService.IsValidStopLossPrice(position.Instrument, orderAction, newStopLossPrice, lastPrice);

                                if (isPriceValid)
                                {
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("Updated SL price=" + newStopLossPrice.ToString() + " old=" + oldStopLossPrice.ToString());
                                    UpdatePositionStopLoss(signalName, attachedInstrument, orderAction, OrderEntry.Automated, tempQuantity, newStopLossPrice);
                                }
                            }
                        }

                        break; //only one postion per instrument so exit early
                    }
                }
            }


            return positionFound;
        }

        private bool HandleStopLossPlus(string signalName, double overrideStopLossPrice = 0)
        {
            double oldStopLossPrice = 0;
            int oldOrderQuantity = 0;
            double newStopLossPrice = 0;
            bool positionFound = false;
            OrderType orderType = OrderType.Unknown;
            int tempQuantity = 0;

            int positionsCount = RealPositionService.PositionsCount;

            for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;
                if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    if (RealPositionService.IsValidPosition(position, attachedInstrument))
                    {
                        position.StoreState();
                        positionFound = true;
                        tempQuantity = position.Quantity;
                        newStopLossPrice = 0;
                        oldStopLossPrice = (overrideStopLossPrice != 0) ? overrideStopLossPrice : RealOrderService.GetStopLossInfo(position.Account, position.Instrument, ConvertMarketPositionToSLOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);

                        if (DebugLogLevel > 2) RealLogger.PrintOutput("Current SL price=" + oldStopLossPrice.ToString() + " ticksize=" + attachedInstrumentTickSize.ToString() + " ticks/point=" + attachedInstrumentTicksPerPoint.ToString());
                        if (oldStopLossPrice == 0)
                        {
                            newStopLossPrice = (overrideStopLossPrice != 0) ? overrideStopLossPrice : GetInitialStopLossPrice(position.MarketPosition, position.AveragePrice);

                            if (position.MarketPosition == MarketPosition.Long)
                            {
                                double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                if (newStopLossPrice > bidPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }
                            else if (position.MarketPosition == MarketPosition.Short)
                            {
                                double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                if (newStopLossPrice < askPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }

                            
                            if (newStopLossPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                            {
                                OrderAction orderAction = ConvertMarketPositionToSLOrderAction(position.MarketPosition);
                                double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                bool isPriceValid = RealOrderService.IsValidStopLossPrice(position.Instrument, orderAction, newStopLossPrice, lastPrice);

                                if (isPriceValid)
                                {
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("New SL price=" + newStopLossPrice.ToString());
                                    CreatePositionStopLoss(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newStopLossPrice);
                                }
                            }
                        }
                        else
                        {
                            if (position.MarketPosition == MarketPosition.Long)
                            {
                                newStopLossPrice = GetStopLossPriceFromJumpTicks(position.MarketPosition, oldStopLossPrice, this.StopLossJumpTicks);

                                double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                if (newStopLossPrice > bidPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }
                            else if (position.MarketPosition == MarketPosition.Short)
                            {
                                newStopLossPrice = GetStopLossPriceFromJumpTicks(position.MarketPosition, oldStopLossPrice, this.StopLossJumpTicks);

                                double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                if (newStopLossPrice < askPrice)
                                {
                                    newStopLossPrice = 0;
                                }
                            }
                            
                            if (newStopLossPrice != 0 && oldStopLossPrice != newStopLossPrice && !position.HasStateChanged() && !position.IsFlat())
                            {
                                OrderAction orderAction = ConvertMarketPositionToSLOrderAction(position.MarketPosition);
                                double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                bool isPriceValid = RealOrderService.IsValidStopLossPrice(position.Instrument, orderAction, newStopLossPrice, lastPrice);

                                if (isPriceValid)
                                {
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("Updated SL price=" + newStopLossPrice.ToString() + " old=" + oldStopLossPrice.ToString());
                                    UpdatePositionStopLoss(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newStopLossPrice);
                                }
                            }
                        }

                        break; //only one postion per instrument so exit early
                    }
                }
            }

            return positionFound;
        }

        private bool HandleSLTPRefresh(string signalName)
        {
            double oldStopLossPrice = 0;
            double oldTakeProfitPrice = 0;
            int oldOrderQuantity = 0;
            double newStopLossPrice = 0;
            double newTakeProfitPrice = 0;
            double triggerStopLossPrice = 0;
            bool hasPosition = false;
            bool hasStopLoss = false;
            bool hasProfitLocked = false;
            bool hasHitPriceTrigger = false;
            int tempQuantity = 0;
            OrderType orderType = OrderType.Unknown;


            if (!IsAccountFlat() && !HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements() && !HasDelayForPendingOrders())
            {
                int positionsCount = RealPositionService.PositionsCount;

                for (int index = 0; index < positionsCount; index++)
                {
                    RealPosition position = null;
                    if (RealPositionService.TryGetByIndex(index, out position))
                    {
                        if (RealPositionService.IsValidPosition(position, attachedInstrument))
                        {
                            position.StoreState();
                            tempQuantity = position.Quantity;

                            hasPosition = true;

                            MarketPosition reversedMarketPosition = MarketPosition.Flat;

                            if (position.MarketPosition == MarketPosition.Long)
                                reversedMarketPosition = MarketPosition.Short;
                            else
                                reversedMarketPosition = MarketPosition.Long;

                            if (CancelPositionSLTPOrders("SLTPRefresh-Rev", attachedInstrument, ConvertMarketPositionToSLOrderAction(reversedMarketPosition))) return hasPosition; //exit very early
                            
                            hasStopLoss = false;
                            hasProfitLocked = false;
                            hasHitPriceTrigger = false;
                            triggerStopLossPrice = 0;
                            newStopLossPrice = 0;
                            oldStopLossPrice = RealOrderService.GetStopLossInfo(position.Account, position.Instrument, ConvertMarketPositionToSLOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);

                            if (oldStopLossPrice == 0 && this.UseAutoPositionStopLoss)
                            {
                                if (DebugLogLevel > 2) RealLogger.PrintOutput("refresh SL price=" + oldStopLossPrice.ToString() + " auto=" + this.UseAutoPositionStopLoss.ToString() + " oldquan=" + oldOrderQuantity.ToString());
                                newStopLossPrice = GetInitialStopLossPrice(position.MarketPosition, position.AveragePrice);

                                if (position.MarketPosition == MarketPosition.Long)
                                {
                                    double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("auto SL create = " + newStopLossPrice.ToString() + " - bid=" + bidPrice.ToString());
                                    if (newStopLossPrice >= (Math.Min(bidPrice, lastPrice) - (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newStopLossPrice = 0;
                                    }
                                }
                                else if (position.MarketPosition == MarketPosition.Short)
                                {
                                    double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (newStopLossPrice <= (Math.Max(askPrice, lastPrice) + (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newStopLossPrice = 0;
                                    }
                                }

                                if (newStopLossPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                                {
                                    OrderAction orderAction = ConvertMarketPositionToSLOrderAction(position.MarketPosition);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    bool isPriceValid = RealOrderService.IsValidStopLossPrice(position.Instrument, orderAction, newStopLossPrice, lastPrice);

                                    if (isPriceValid)
                                    {
                                        if (DebugLogLevel > 2) RealLogger.PrintOutput("New SL price=" + newStopLossPrice.ToString());
                                        CreatePositionStopLoss(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newStopLossPrice);
                                    }
                                }
                            }
                            else if (oldOrderQuantity != tempQuantity)
                            {
                                //RealLogger.PrintOutput("Current SL price=" + oldStopLossPrice.ToString() + " ticksize=" + attachedInstrumentTickSize.ToString() + " ticks/point=" + attachedInstrumentTicksPerPoint.ToString());

                                newStopLossPrice = oldStopLossPrice;

                                if (position.MarketPosition == MarketPosition.Long)
                                {
                                    double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (newStopLossPrice >= (Math.Min(bidPrice, lastPrice) - (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newStopLossPrice = 0;
                                    }
                                }
                                else if (position.MarketPosition == MarketPosition.Short)
                                {
                                    double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (newStopLossPrice <= (Math.Max(askPrice, lastPrice) + (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newStopLossPrice = 0;
                                    }
                                }

                               

                                //RealLogger.PrintOutput("Updated SL price=" + newStopLossPrice.ToString() + " old=" + oldStopLossPrice.ToString());
                                if (newStopLossPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                                {
                                    OrderAction orderAction = ConvertMarketPositionToSLOrderAction(position.MarketPosition);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    bool isPriceValid = RealOrderService.IsValidStopLossPrice(position.Instrument, orderAction, newStopLossPrice, lastPrice);

                                    if (isPriceValid)
                                    {
                                        UpdatePositionStopLoss(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newStopLossPrice);
                                    }
                                }
                            }
                            
                            if ((BreakEvenAutoTriggerTicks > 0 || BreakEvenAutoTriggerATRMultiplier > 0) && currentBreakEvenAutoStatus != TickHunterBreakEvenAutoTypes.Disabled)
                            {
                                
                                hasStopLoss = (oldStopLossPrice > 0);

                                if (hasStopLoss)
                                {
                                    triggerStopLossPrice = GetTriggerBreakEvenStopLossPrice(position.MarketPosition, position.AveragePrice);

                                    if (position.MarketPosition == MarketPosition.Long)
                                    {
                                        if (oldStopLossPrice > position.AveragePrice)
                                        {
                                            hasProfitLocked = true;
                                        }

                                        double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                        if (triggerStopLossPrice <= bidPrice)
                                        {
                                            hasHitPriceTrigger = true;
                                        }
                                    }
                                    else if (position.MarketPosition == MarketPosition.Short)
                                    {
                                        if (oldStopLossPrice < position.AveragePrice)
                                        {
                                            hasProfitLocked = true;
                                        }

                                        double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                        if (triggerStopLossPrice >= askPrice)
                                        {
                                            hasHitPriceTrigger = true;
                                        }
                                    }
                                }

                                if (hasPosition && hasStopLoss && !hasProfitLocked && hasHitPriceTrigger && !position.HasStateChanged() && !position.IsFlat())
                                {
                                    if (DebugLogLevel > 0) RealLogger.PrintOutput("Auto BE hit trigger price of " + triggerStopLossPrice.ToString("N2"), PrintTo.OutputTab1, false);
                                    HandleBreakEvenPlus("AutoBreakEven");
                                }
                                else if (currentBreakEvenAutoStatus != TickHunterBreakEvenAutoTypes.Disabled && currentBreakEvenAutoStatus != TickHunterBreakEvenAutoTypes.Enabled && hasPosition && hasStopLoss && hasProfitLocked && !position.HasStateChanged() && !position.IsFlat())
                                {
                                    
                                    if (position.MarketPosition == MarketPosition.Long)
                                    {
                                        double entryPrice = CalculateTrailLowPrice(position.MarketPosition, false);

                                        if (entryPrice > oldStopLossPrice)
                                        {
                                            TrailBuyPositionStopLoss("AutoBreakEven");
                                        }
                                    }
                                    else if (position.MarketPosition == MarketPosition.Short)
                                    {
                                        double entryPrice = CalculateTrailHighPrice(position.MarketPosition);

                                        if (entryPrice < oldStopLossPrice)
                                        {
                                            TrailSellPositionStopLoss("AutoBreakEven");
                                        }
                                    }
                                }
                            }


                            newTakeProfitPrice = 0;
                            oldTakeProfitPrice = RealOrderService.GetTakeProfitInfo(position.Account, position.Instrument, ConvertMarketPositionToTPOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);

                            if (oldTakeProfitPrice == 0 && this.UseAutoPositionTakeProfit)
                            {
                                if (DebugLogLevel > 2) RealLogger.PrintOutput("refresh tp price=" + oldTakeProfitPrice.ToString() + " auto=" + this.UseAutoPositionTakeProfit.ToString() + " oldquan=" + oldOrderQuantity.ToString());
                                newTakeProfitPrice = GetInitialTakeProfitPrice(position.MarketPosition, position.AveragePrice);

                                if (position.MarketPosition == MarketPosition.Long)
                                {
                                    double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (newTakeProfitPrice <= (Math.Max(askPrice, lastPrice) + (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newTakeProfitPrice = 0;
                                    }
                                }
                                else if (position.MarketPosition == MarketPosition.Short)
                                {

                                    double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("auto TP create = " + newTakeProfitPrice.ToString() + " - bid=" + bidPrice.ToString());
                                    if (newTakeProfitPrice >= (Math.Min(bidPrice, lastPrice) - (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newTakeProfitPrice = 0;
                                    }
                                }

                                if (newTakeProfitPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                                {
                                    OrderAction orderAction = ConvertMarketPositionToTPOrderAction(position.MarketPosition);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    bool isPriceValid = RealOrderService.IsValidTakeProfitPrice(position.Instrument, orderAction, newTakeProfitPrice, lastPrice);

                                    if (isPriceValid)
                                    {
                                        if (DebugLogLevel > 2) RealLogger.PrintOutput("New TP price=" + newTakeProfitPrice.ToString());
                                        CreatePositionTakeProfit(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newTakeProfitPrice);
                                    }
                                }
                            }


                            if (oldOrderQuantity != tempQuantity)
                            {
                                //RealLogger.PrintOutput("Current SL price=" + oldStopLossPrice.ToString() + " ticksize=" + attachedInstrumentTickSize.ToString() + " ticks/point=" + attachedInstrumentTicksPerPoint.ToString());

                                newTakeProfitPrice = oldTakeProfitPrice;

                                if (position.MarketPosition == MarketPosition.Long)
                                {
                                    double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (newTakeProfitPrice <= (Math.Max(askPrice, lastPrice) + (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newTakeProfitPrice = 0;
                                    }
                                }
                                else if (position.MarketPosition == MarketPosition.Short)
                                {
                                    double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    if (newTakeProfitPrice >= (Math.Min(bidPrice, lastPrice) - (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
                                    {
                                        newTakeProfitPrice = 0;
                                    }
                                }

                                //RealLogger.PrintOutput("Updated SL price=" + newStopLossPrice.ToString() + " old=" + oldStopLossPrice.ToString());
                                if (newTakeProfitPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                                {
                                    OrderAction orderAction = ConvertMarketPositionToTPOrderAction(position.MarketPosition);
                                    double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                    bool isPriceValid = RealOrderService.IsValidTakeProfitPrice(position.Instrument, orderAction, newTakeProfitPrice, lastPrice);

                                    if (isPriceValid)
                                    {
                                        UpdatePositionTakeProfit(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newTakeProfitPrice);
                                    }
                                }
                            }


                            riskInfoHasPosition = true;
                            riskInfoMarketPosition = position.MarketPosition;
                            riskInfoQuanitiy = tempQuantity;
                            riskInfoPositionPrice = position.AveragePrice;
                            riskInfoPositionStopLossPrice = (newStopLossPrice == 0) ? oldStopLossPrice : newStopLossPrice;

                            RefreshRiskInfoLabel();

                            break; //only one postion per instrument so exit early
                        }
                    }
                       
                }
            }
            else if (IsAccountFlat() && !HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
            {
                //RealLogger.PrintOutput("CancelPositionSLTPOrders IsAccountFlat()=" + Convert.ToString(IsAccountFlat()));
                CancelPositionSLTPOrders("SLTPRefresh-All", attachedInstrument);
                riskInfoHasPosition = false;
                RefreshRiskInfoLabel();
            }

            return hasPosition;
        }

        private bool HandleTakeProfitPlus(string signalName, double overrideTakeProfitPrice = 0)
        {
            double oldTakeProfitPrice = 0;
            int oldOrderQuantity = 0;
            double newTakeProfitPrice = 0;
            bool positionFound = false;
            OrderType orderType = OrderType.Unknown;
            int tempQuantity = 0;

            int positionsCount = RealPositionService.PositionsCount;

            for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;
                if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    if (RealPositionService.IsValidPosition(position, attachedInstrument))
                     {
                        position.StoreState();
                        positionFound = true;
                        tempQuantity = position.Quantity;
                        newTakeProfitPrice = 0;
                        oldTakeProfitPrice = RealOrderService.GetTakeProfitInfo(position.Account, position.Instrument, ConvertMarketPositionToTPOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);

                        if (DebugLogLevel > 2) RealLogger.PrintOutput("Current TP price=" + oldTakeProfitPrice.ToString() + " ticksize=" + attachedInstrumentTickSize.ToString() + " ticks/point=" + attachedInstrumentTicksPerPoint.ToString());
                        if (oldTakeProfitPrice == 0)
                        {
                            newTakeProfitPrice = (overrideTakeProfitPrice != 0) ? overrideTakeProfitPrice : GetInitialTakeProfitPrice(position.MarketPosition, position.AveragePrice);

                            if (position.MarketPosition == MarketPosition.Long)
                            {
                                double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                if (newTakeProfitPrice < askPrice)
                                {
                                    newTakeProfitPrice = 0;
                                }
                            }
                            else if (position.MarketPosition == MarketPosition.Short)
                            {
                                double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                if (newTakeProfitPrice > bidPrice)
                                {
                                    newTakeProfitPrice = 0;
                                }
                            }

                            
                            if (newTakeProfitPrice != 0 && !position.HasStateChanged() && !position.IsFlat())
                            {
                                OrderAction orderAction = ConvertMarketPositionToTPOrderAction(position.MarketPosition);
                                double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                bool isPriceValid = RealOrderService.IsValidTakeProfitPrice(position.Instrument, orderAction, newTakeProfitPrice, lastPrice);

                                if (isPriceValid)
                                {
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("New TP price=" + newTakeProfitPrice.ToString());
                                    CreatePositionTakeProfit(signalName, position.Instrument, orderAction, OrderEntry.Manual, tempQuantity, newTakeProfitPrice);
                                }
                            }
                        }
                        else
                        {
                            if (position.MarketPosition == MarketPosition.Long)
                            {
                                newTakeProfitPrice = (overrideTakeProfitPrice != 0) ? overrideTakeProfitPrice : GetTakeProfitPriceFromJumpTicks(position.MarketPosition, oldTakeProfitPrice, this.TakeProfitJumpTicks);

                                double askPrice = RealInstrumentService.GetAskPrice(position.Instrument);
                                if (newTakeProfitPrice < askPrice)
                                {
                                    newTakeProfitPrice = 0;
                                }
                            }
                            else if (position.MarketPosition == MarketPosition.Short)
                            {
                                newTakeProfitPrice = (overrideTakeProfitPrice != 0) ? overrideTakeProfitPrice : GetTakeProfitPriceFromJumpTicks(position.MarketPosition, oldTakeProfitPrice, this.TakeProfitJumpTicks);

                                double bidPrice = RealInstrumentService.GetBidPrice(position.Instrument);
                                if (newTakeProfitPrice > bidPrice)
                                {
                                    newTakeProfitPrice = 0;
                                }

                            }

                            if (newTakeProfitPrice != 0 && oldTakeProfitPrice != newTakeProfitPrice && !position.HasStateChanged() && !position.IsFlat())
                            {
                                OrderAction orderAction = ConvertMarketPositionToTPOrderAction(position.MarketPosition);
                                double lastPrice = RealInstrumentService.GetLastPrice(position.Instrument);
                                bool isPriceValid = RealOrderService.IsValidTakeProfitPrice(position.Instrument, orderAction, newTakeProfitPrice, lastPrice);

                                if (isPriceValid)
                                {
                                    
                                    if (DebugLogLevel > 2) RealLogger.PrintOutput("Updated TP price=" + newTakeProfitPrice.ToString() + " old=" + oldTakeProfitPrice.ToString());
                                    UpdatePositionTakeProfit(signalName, position.Instrument, orderAction, OrderEntry.Automated, tempQuantity, newTakeProfitPrice);
                                }
                            }
                        }

                        break; //only one postion per instrument so exit early
                    }
                }
            }

            return positionFound;
        }

        private bool HandleSellSnap(string signalName)
        {
            double newSellSnapPrice = 0;
            bool positionFound = false;
            bool isShortPosition = false;
            bool orderFound = false;
            double oldStopLossPrice = 0;
            double takeProfitPrice = 0;
            OrderType orderType = OrderType.Unknown;

            int positionsCount = RealPositionService.PositionsCount;

            for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;
                if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    if (RealPositionService.IsValidPosition(position, attachedInstrument))
                    {
                        isShortPosition = (position.MarketPosition == MarketPosition.Short);
                        positionFound = true;

                        int oldOrderQuantity = 0;

                        oldStopLossPrice = RealOrderService.GetStopLossInfo(position.Account, position.Instrument, ConvertMarketPositionToSLOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);
                        int stopLossTicks = CalculateStopLossTicks(position.MarketPosition, position.AveragePrice, oldStopLossPrice, attachedInstrumentTickSize);
                        takeProfitPrice = CalculateTakeProfitPrice(position.MarketPosition, position.AveragePrice, stopLossTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

                        break;
                    }
                }
            }

            if (!isShortPosition && positionFound && CheckSnapPositionTPSL())
            {
                if (takeProfitPrice > 0)
                {
                    HandleTakeProfitPlus("SellSnap", takeProfitPrice);
                }
                positionFound = false;
            }
            else if (isShortPosition && positionFound && CheckSnapPositionTPSL())
            {
                TrailSellPositionStopLoss("SellSnap",  true);
            }
            else if (!positionFound || !CheckSnapPositionTPSL())
            {
                lock (account.Orders)
                {
                    foreach (Order order in account.Orders)
                    {
                        if (order.Instrument == attachedInstrument && (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Buy) || RealOrderService.IsValidSellSnapOrder(order, attachedInstrument, OrderAction.Sell)))
                        {
                            orderFound = true;

                            if (!Order.IsTerminalState(order.OrderState))
                            {
                                if (DebugLogLevel > 2) RealLogger.PrintOutput(signalName + " is cancelling pending order " + order.Instrument.FullName + " Type=" + order.OrderType.ToString());

                                try
                                {
                                    account.Cancel(new[] { order });
                                }
                                catch (Exception ex)
                                {
                                    RealLogger.PrintOutput("Exception in HandleSellSnap:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                                }
                            }

                        }
                    }
                }

                if (!orderFound)
                {
                    newSellSnapPrice = CalculateTrailLowPrice(MarketPosition.Short, true);

                    double bidPrice = RealInstrumentService.GetBidPrice(attachedInstrument);
                    if (newSellSnapPrice >= bidPrice)
                    {
                        newSellSnapPrice = 0;
                    }

                    
                    if (newSellSnapPrice != 0)
                    {
                        if (DebugLogLevel > 2) RealLogger.PrintOutput("New Snap- price=" + newSellSnapPrice.ToString());
                        CreateSellSnap(signalName, attachedInstrument, OrderAction.Sell, OrderEntry.Manual, SnapPopContracts, newSellSnapPrice);
                    }
                }

            }

            return positionFound;
        }

        private void TrailSellPositionStopLoss(string signalName, bool force1Bar = false)
        {
            double newEntryPrice = CalculateTrailHighPrice(MarketPosition.Short, force1Bar);

            double askPrice = RealInstrumentService.GetAskPrice(attachedInstrument);
            double lastPrice = RealInstrumentService.GetLastPrice(attachedInstrument);
            
            if (newEntryPrice <= (Math.Max(askPrice, lastPrice) + (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
            {
                newEntryPrice = 0;
            }

            if (newEntryPrice != 0)
            {
                //if (DebugLogLevel > 2) RealLogger.PrintOutput("New Snap- price=" + newSellSnapPrice.ToString());

                HandleStopLossPlus(signalName, newEntryPrice);
            }
        }

        private void TrailBuyPositionStopLoss(string signalName, bool force1Bar = false)
        {
            double newEntryPrice = CalculateTrailLowPrice(MarketPosition.Long, force1Bar);

            double bidPrice = RealInstrumentService.GetBidPrice(attachedInstrument);
            double lastPrice = RealInstrumentService.GetLastPrice(attachedInstrument);

            if (newEntryPrice >= (Math.Min(bidPrice, lastPrice) - (attachedInstrumentTickSize * RefreshTPSLPaddingTicks)))
            {
                newEntryPrice = 0;
            }

            if (newEntryPrice != 0)
            {
                //if (DebugLogLevel > 2) RealLogger.PrintOutput("New trail price=" + newEntryPrice.ToString());

                HandleStopLossPlus(signalName, newEntryPrice);
            }
        }

        private double CalculateTrailLowPrice(MarketPosition positionType, bool force1Bar = false)
        {
            double entryPrice = 0;

            if (force1Bar || currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail1Bar)
                entryPrice = previous1LowPrice - (attachedInstrumentTickSize * SnapPaddingTicks);
            else if ( currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail2Bar)
                entryPrice = Math.Min(previous1LowPrice, previous2LowPrice) - (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail3Bar)
                entryPrice = Math.Min(Math.Min(previous1LowPrice, previous2LowPrice), previous3LowPrice) - (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail5Bar)
                entryPrice = Math.Min(Math.Min(Math.Min(Math.Min(previous1LowPrice, previous2LowPrice), previous3LowPrice), previous4LowPrice), previous5LowPrice) - (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage1)
                entryPrice = GetBreakEvenAutoMovingAverage1Price(positionType);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage2)
                entryPrice = GetBreakEvenAutoMovingAverage2Price(positionType);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage3)
                entryPrice = GetBreakEvenAutoMovingAverage3Price(positionType);


            return entryPrice;
        }

        private double CalculateTrailHighPrice(MarketPosition positionType, bool force1Bar = false)
        {
            double entryPrice = 0;

            if (force1Bar || currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail1Bar)
                entryPrice = previous1HighPrice + (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail2Bar)
                entryPrice = Math.Max(previous1HighPrice, previous2HighPrice) + (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail3Bar)
                entryPrice = Math.Max(Math.Max(previous1HighPrice, previous2HighPrice), previous3HighPrice) + (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail5Bar)
                entryPrice = Math.Max(Math.Max(Math.Max(Math.Max(previous1HighPrice, previous2HighPrice), previous3HighPrice), previous4HighPrice), previous5HighPrice) + (attachedInstrumentTickSize * SnapPaddingTicks);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage1)
                entryPrice = GetBreakEvenAutoMovingAverage1Price(positionType);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage2)
                entryPrice = GetBreakEvenAutoMovingAverage2Price(positionType);
            else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage3)
                entryPrice = GetBreakEvenAutoMovingAverage3Price(positionType);

            return entryPrice;
        }

        private double GetBreakEvenAutoMovingAverage1Price(MarketPosition positionType)
        {
            double maValue = 0;

            if (positionType == MarketPosition.Long)
                maValue = Math.Floor(breakEvenMA1Value * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint;
            else
                maValue = Math.Ceiling(breakEvenMA1Value * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint;

            return maValue;
        }

        private double GetBreakEvenAutoMovingAverage2Price(MarketPosition positionType)
        {
            double maValue = 0;

            if (positionType == MarketPosition.Long)
                maValue = Math.Floor(breakEvenMA2Value * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint;
            else
                maValue = Math.Ceiling(breakEvenMA2Value * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint;

            return maValue;
        }

        private double GetBreakEvenAutoMovingAverage3Price(MarketPosition positionType)
        {
            double maValue = 0;

            if (positionType == MarketPosition.Long)
                maValue = Math.Floor(breakEvenMA3Value * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint;
            else
                maValue = Math.Ceiling(breakEvenMA3Value * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint;

            return maValue;
        }

        private bool CheckSnapPositionTPSL()
        {
            bool returnFlag = UseSnapPositionTPSL;

            return returnFlag;
        }

        private int CalculateStopLossTicks(MarketPosition marketPosition, double averagePrice, double stopLossPrice, double tickSize)
        {
            int stopLossTicks = 0;

            if (averagePrice > 0 && stopLossPrice > 0)
            {
                bool isBuyPosition = (marketPosition == MarketPosition.Long);

                if (isBuyPosition)
                {
                    stopLossTicks = (int)Math.Floor((stopLossPrice - averagePrice) / tickSize);
                }
                else
                {
                    stopLossTicks = (int)Math.Ceiling((averagePrice - stopLossPrice) / tickSize);
                }

                if (stopLossTicks < 0)
                {
                    stopLossTicks *= -1;
                }
            }

            return stopLossTicks;

        }

        private bool HandleBuySnap(string signalName)
        {
            double newBuySnapPrice = 0;
            bool positionFound = false;
            bool isLongPosition = false;
            bool orderFound = false;
            double oldStopLossPrice = 0;
            double takeProfitPrice = 0;
            OrderType orderType = OrderType.Unknown;

            int positionsCount = RealPositionService.PositionsCount;

            for (int index = 0; index < positionsCount; index++)
            {
                RealPosition position = null;
                if (RealPositionService.TryGetByIndex(index, out position))
                {
                    //if (position.Instrument == attachedInstrument)
                    if (RealPositionService.IsValidPosition(position, attachedInstrument))
                    {
                        isLongPosition = (position.MarketPosition == MarketPosition.Long);
                        positionFound = true;

                        int oldOrderQuantity = 0;

                        oldStopLossPrice = RealOrderService.GetStopLossInfo(position.Account, position.Instrument, ConvertMarketPositionToSLOrderAction(position.MarketPosition), out orderType, out oldOrderQuantity);
                        int stopLossTicks = CalculateStopLossTicks(position.MarketPosition, position.AveragePrice, oldStopLossPrice, attachedInstrumentTickSize);
                        takeProfitPrice = CalculateTakeProfitPrice(position.MarketPosition, position.AveragePrice, stopLossTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

                        break;
                    }
                }
            }

            if (!isLongPosition && positionFound && CheckSnapPositionTPSL())
            {
                if (takeProfitPrice > 0)
                {
                    HandleTakeProfitPlus("BuySnap", takeProfitPrice);
                }

                positionFound = false;
            }
            else if (isLongPosition && positionFound && CheckSnapPositionTPSL())
            {

                TrailBuyPositionStopLoss("BuySnap", true);
            }
            else if (!positionFound || !CheckSnapPositionTPSL())
            {
                lock (account.Orders)
                {
                    foreach (Order order in account.Orders)
                    {
                        if (order.Instrument == attachedInstrument && (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Buy) || RealOrderService.IsValidSellSnapOrder(order, attachedInstrument, OrderAction.Sell)))
                        {
                            orderFound = true;

                            if (!Order.IsTerminalState(order.OrderState))
                            {
                                if (DebugLogLevel > 2) RealLogger.PrintOutput(signalName + " is cancelling pending order " + order.Instrument.FullName + " Type=" + order.OrderType.ToString());

                                try
                                {
                                    account.Cancel(new[] { order });
                                }
                                catch (Exception ex)
                                {
                                    RealLogger.PrintOutput("Exception in HandleBuySnap:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                                }
                            }

                        }
                    }
                }

                if (!orderFound)
                {
                    newBuySnapPrice = CalculateTrailHighPrice(MarketPosition.Long, true);

                    double askPrice = RealInstrumentService.GetAskPrice(attachedInstrument);
                    if (newBuySnapPrice <= askPrice)
                    {
                        newBuySnapPrice = 0;
                    }

                    
                    if (newBuySnapPrice != 0)
                    {
                        if (DebugLogLevel > 2) RealLogger.PrintOutput("New Snap+ price=" + newBuySnapPrice.ToString());
                        CreateBuySnap(signalName, attachedInstrument, OrderAction.Buy, OrderEntry.Manual, SnapPopContracts, newBuySnapPrice);
                    }
                }

            }

            return positionFound;
        }

        private bool HandleBuyPop(string signalName)
        {
            bool buyOrderFound = false;
            bool sellOrderFound = false;
            double oldPopPrice = 0;

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (order.Instrument == attachedInstrument && (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Buy) || RealOrderService.IsValidSellSnapOrder(order, attachedInstrument, OrderAction.Sell)))
                    {
                        if (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Buy))
                        {
                            if (!Order.IsTerminalState(order.OrderState))
                            {
                                buyOrderFound = true;
                                oldPopPrice = order.StopPrice;
                             }
                        }

                        if (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Sell))
                        {
                            sellOrderFound = true;
                        }

                        if (sellOrderFound)
                        {
                            if (!Order.IsTerminalState(order.OrderState))
                            {
                                if (DebugLogLevel > 2) RealLogger.PrintOutput(signalName + " is cancelling pending order " + order.Instrument.FullName + " Type=" + order.OrderType.ToString());

                                try
                                {
                                    account.Cancel(new[] { order });
                                }
                                catch (Exception ex)
                                {
                                    RealLogger.PrintOutput("Exception in HandleBuyPop:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                                }
                            }
                        }
                    }
                }
            }


            if(!buyOrderFound && !sellOrderFound)
            {
                double bidPrice = RealInstrumentService.GetBidPrice(attachedInstrument);
                double newPopPrice = GetInitialPopPrice(MarketPosition.Long, bidPrice);

                if (newPopPrice <= bidPrice)
                {
                    newPopPrice = 0;
                }

                if (newPopPrice != 0)
                {
                    if (DebugLogLevel > 2) RealLogger.PrintOutput("New Pop+ price=" + newPopPrice.ToString());
                    CreateBuySnap(signalName, attachedInstrument, OrderAction.Buy, OrderEntry.Manual, SnapPopContracts, newPopPrice);
                }
            }
            else if (buyOrderFound && !sellOrderFound)
            {
                double bidPrice = RealInstrumentService.GetBidPrice(attachedInstrument);
                double newPopPrice = GetPopPriceFromJumpTicks(MarketPosition.Long, oldPopPrice, this.PopJumpTicks);

                if (newPopPrice <= bidPrice)
                {
                    newPopPrice = 0;
                }

                if (newPopPrice != 0)
                {
                    if (DebugLogLevel > 2) RealLogger.PrintOutput("Update Pop+ price=" + newPopPrice.ToString());
                    UpdatePopOrder(signalName, attachedInstrument, OrderAction.Buy, OrderEntry.Manual, newPopPrice);
                }
            }

            return buyOrderFound;
        }

        private bool HandleSellPop(string signalName)
        {
            bool buyOrderFound = false;
            bool sellOrderFound = false;
            double oldPopPrice = 0;

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (order.Instrument == attachedInstrument && (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Buy) || RealOrderService.IsValidSellSnapOrder(order, attachedInstrument, OrderAction.Sell)))
                    {
                        if (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Sell))
                        {
                            if (!Order.IsTerminalState(order.OrderState))
                            {
                                sellOrderFound = true;
                                oldPopPrice = order.StopPrice;
                            }
                        }

                        if (RealOrderService.IsValidBuySnapOrder(order, attachedInstrument, OrderAction.Buy))
                        {
                            buyOrderFound = true;
                        }

                        if (buyOrderFound)
                        {
                            if (!Order.IsTerminalState(order.OrderState))
                            {
                                if (DebugLogLevel > 2) RealLogger.PrintOutput(signalName + " is cancelling pending order " + order.Instrument.FullName + " Type=" + order.OrderType.ToString());

                                try
                                {
                                    account.Cancel(new[] { order });
                                }
                                catch (Exception ex)
                                {
                                    RealLogger.PrintOutput("Exception in HandleBuyPop:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                                }
                            }
                        }
                    }
                }
            }


            if (!sellOrderFound && !buyOrderFound)
            {
                double askPrice = RealInstrumentService.GetAskPrice(attachedInstrument);
                double newPopPrice = GetInitialPopPrice(MarketPosition.Short, askPrice);

                if (newPopPrice >= askPrice)
                {
                    newPopPrice = 0;
                }


                if (newPopPrice != 0)
                {
                    if (DebugLogLevel > 2) RealLogger.PrintOutput("New Pop- price=" + newPopPrice.ToString());
                    CreateSellSnap(signalName, attachedInstrument, OrderAction.Sell, OrderEntry.Manual, SnapPopContracts, newPopPrice);
                }
            }
            else if (sellOrderFound && !buyOrderFound)
            {
                double askPrice = RealInstrumentService.GetAskPrice(attachedInstrument);
                double newPopPrice = GetPopPriceFromJumpTicks(MarketPosition.Short, oldPopPrice, this.PopJumpTicks);

                if (newPopPrice >= askPrice)
                {
                    newPopPrice = 0;
                }

                if (newPopPrice != 0)
                {
                    if (DebugLogLevel > 2) RealLogger.PrintOutput("Update Pop- price=" + newPopPrice.ToString());
                    UpdatePopOrder(signalName, attachedInstrument, OrderAction.Sell, OrderEntry.Manual, newPopPrice);
                }
            }

            return sellOrderFound;
        }

        private int CalculateATRTicks(double atrValue, double atrMultiplier, int ticksPerPoint)
        {
            int atrTicks = 0;

            if (atrMultiplier > 0)
            {
                atrTicks = (int)((atrValue * ticksPerPoint) * atrMultiplier);
            }

            return atrTicks;
        }

        private double GetInitialPopPrice(MarketPosition marketPosition, double askPrice)
        {
            int popTicks = this.PopInitialTicks;
            bool allowATROverride = (this.PopInitialATRMultiplier > 0);

            if (allowATROverride)
            {
                int newATRPopTicks = CalculateATRTicks(atrValue, this.PopInitialATRMultiplier, attachedInstrumentTicksPerPoint);

                popTicks = Math.Max(newATRPopTicks, this.PopInitialTicks);
            }

            double newPopPrice = CalculatePopPrice(marketPosition, askPrice, popTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newPopPrice);

            return normalizedPrice;
        }

        private double GetPopPriceFromJumpTicks(MarketPosition marketPosition, double oldPopPrice, int jumpTicks)
        {
            double newPopPrice = 0;

            newPopPrice = CalculatePopPlusPrice(marketPosition, oldPopPrice, jumpTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newPopPrice);

            return normalizedPrice;
        }

        private double CalculatePopPrice(MarketPosition marketPosition, double price, int ticks, double tickSize, int ticksPerPoint)
        {
            double newPopPrice = 0;

            if (marketPosition == MarketPosition.Long)
            {
                newPopPrice = (Math.Ceiling(price * ticksPerPoint) / ticksPerPoint) + ((double)ticks * tickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newPopPrice = (Math.Floor(price * ticksPerPoint) / ticksPerPoint) - ((double)ticks * tickSize);
            }

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newPopPrice);

            return normalizedPrice;
        }

        private double CalculatePopPlusPrice(MarketPosition marketPosition, double price, int ticks, double tickSize, int ticksPerPoint)
        {
            double newPopPrice = 0;

            if (marketPosition == MarketPosition.Long)
            {
                newPopPrice = (Math.Ceiling(price * ticksPerPoint) / ticksPerPoint) - ((double)ticks * tickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newPopPrice = (Math.Floor(price * ticksPerPoint) / ticksPerPoint) + ((double)ticks * tickSize);
            }

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newPopPrice);

            return normalizedPrice;
        }

        private double GetInitialStopLossPrice(MarketPosition marketPosition, double averagePrice)
        {
            double newStopLossPrice = 0;
            int stopLossTicks = this.StopLossInitialTicks;
            bool allowATROverride = (this.StopLossInitialATRMultiplier > 0);

            if (allowATROverride)
            {
                int newATRStopLossTicks = CalculateATRTicks(atrValue, this.StopLossInitialATRMultiplier, attachedInstrumentTicksPerPoint);

                stopLossTicks = Math.Max(newATRStopLossTicks, this.StopLossInitialTicks);
            }

            newStopLossPrice = CalculateStopLossPrice(marketPosition, averagePrice, stopLossTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newStopLossPrice);

            return normalizedPrice;
        }

        private double GetStopLossPriceFromJumpTicks(MarketPosition marketPosition, double oldStopLossPrice, int jumpTicks)
        {
            double newStopLossPrice = 0;

            newStopLossPrice = CalculateStopLossPlusPrice(marketPosition, oldStopLossPrice, jumpTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newStopLossPrice);

            return normalizedPrice;
        }

        private double CalculateStopLossPrice(MarketPosition marketPosition, double price, int ticks, double tickSize, int ticksPerPoint)
        {
            double newStopLossPrice = 0;

            if (marketPosition == MarketPosition.Long)
            {
                newStopLossPrice = (Math.Floor(price * ticksPerPoint) / ticksPerPoint) - ((double)ticks * tickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newStopLossPrice = (Math.Ceiling(price * ticksPerPoint) / ticksPerPoint) + ((double)ticks * tickSize);
            }

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newStopLossPrice);

            return normalizedPrice;
        }

        private double CalculateStopLossPlusPrice(MarketPosition marketPosition, double price, int ticks, double tickSize, int ticksPerPoint)
        {
            double newStopLossPrice = 0;

            if (marketPosition == MarketPosition.Long)
            {
                newStopLossPrice = (Math.Ceiling(price * ticksPerPoint) / ticksPerPoint) + ((double)ticks * tickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newStopLossPrice = (Math.Floor(price * ticksPerPoint) / ticksPerPoint) - ((double)ticks * tickSize);
            }

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newStopLossPrice);

            return normalizedPrice;
        }

        private double GetInitialBreakEvenStopLossPrice(MarketPosition marketPosition, double averagePrice)
        {
            double newStopLossPrice = 0;

            if (marketPosition == MarketPosition.Long)
            {
                newStopLossPrice = (Math.Ceiling(averagePrice * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint) + ((double)this.BreakEvenInitialTicks * attachedInstrumentTickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newStopLossPrice = (Math.Floor(averagePrice * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint) - ((double)this.BreakEvenInitialTicks * attachedInstrumentTickSize);
            }

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newStopLossPrice);

            return normalizedPrice;
        }

        private double GetTriggerBreakEvenStopLossPrice(MarketPosition marketPosition, double averagePrice)
        {
            double newStopLossPrice = 0;
            int breakEventriggerTicks = this.BreakEvenAutoTriggerTicks;

            if (this.BreakEvenAutoTriggerATRMultiplier > 0)
            {
                int newATRTriggerTicks = CalculateATRTicks(atrValue, this.BreakEvenAutoTriggerATRMultiplier, attachedInstrumentTicksPerPoint);

                breakEventriggerTicks = Math.Max(newATRTriggerTicks, this.BreakEvenAutoTriggerTicks);
            }

            if (marketPosition == MarketPosition.Long)
            {
                newStopLossPrice = (Math.Ceiling(averagePrice * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint) + ((double)breakEventriggerTicks * attachedInstrumentTickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newStopLossPrice = (Math.Floor(averagePrice * attachedInstrumentTicksPerPoint) / attachedInstrumentTicksPerPoint) - ((double)breakEventriggerTicks * attachedInstrumentTickSize);
            }

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newStopLossPrice);

            return normalizedPrice;
        }

        private double GetInitialTakeProfitPrice(MarketPosition marketPosition, double averagePrice)
        {
            double newTakeProfitPrice = 0;
            int takeProfitTicks = this.TakeProfitInitialTicks;

            if (this.TakeProfitInitialATRMultiplier > 0)
            {
                int newATRTakeProfitTicks = CalculateATRTicks(atrValue, this.TakeProfitInitialATRMultiplier, attachedInstrumentTicksPerPoint);

                takeProfitTicks = Math.Max(newATRTakeProfitTicks, this.TakeProfitInitialTicks);
            }

            newTakeProfitPrice = CalculateTakeProfitPrice(marketPosition, averagePrice, takeProfitTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

            double normalizedPrice = RealInstrumentService.NormalizePrice(attachedInstrument, newTakeProfitPrice);

            return normalizedPrice;
        }

        private double GetTakeProfitPriceFromJumpTicks(MarketPosition marketPosition, double oldTakeProfitPrice, int jumpTicks)
        {
            double newTakeProfitPrice = 0;

            newTakeProfitPrice = CalculateTakeProfitPrice(marketPosition, oldTakeProfitPrice, jumpTicks, attachedInstrumentTickSize, attachedInstrumentTicksPerPoint);

            return newTakeProfitPrice;
        }

        double CalculateTakeProfitPrice(MarketPosition marketPosition, double price, int ticks, double tickSize, int ticksPerPoint)
        {
            double newTakeProfitPrice = 0;

            if (price > 0 && ticks > 0)
            {
                bool isBuyPosition = (marketPosition == MarketPosition.Long);

                if (marketPosition == MarketPosition.Long)
                {
                    newTakeProfitPrice = (Math.Ceiling(price * ticksPerPoint) / ticksPerPoint) + ((double)ticks * tickSize);
                }
                else if (marketPosition == MarketPosition.Short)
                {
                    newTakeProfitPrice = (Math.Floor(price * ticksPerPoint) / ticksPerPoint) - ((double)ticks * tickSize);
                }
            }

            return newTakeProfitPrice;
        }

        private MarketPosition ConvertOrderActionToMarketPosition(OrderAction orderAction)
        {
            MarketPosition marketPosition = MarketPosition.Flat;

            if (orderAction == OrderAction.Buy || orderAction == OrderAction.BuyToCover)
            {
                marketPosition = MarketPosition.Long;
            }
            else if (orderAction == OrderAction.Sell || orderAction == OrderAction.SellShort)
            {
                marketPosition = MarketPosition.Short;
            }
            else
            {
                RealLogger.PrintOutput("Order action type  " + orderAction.ToString() + " not supported.");
            }

            return marketPosition;
        }

        private OrderAction ConvertMarketPositionToRevOrderAction(MarketPosition marketPosition)
        {
            OrderAction orderAction = OrderAction.Buy;

            if (marketPosition == MarketPosition.Long)
            {
                orderAction = OrderAction.Sell;
            }
            else if (marketPosition == MarketPosition.Short)
            {
                orderAction = OrderAction.Buy;
            }
            else
            {
                RealLogger.PrintOutput("Market position type  " + marketPosition.ToString() + " not supported.");
            }

            return orderAction;
        }

        private OrderAction ConvertMarketPositionToSLOrderAction(MarketPosition marketPosition)
        {
            OrderAction orderAction = OrderAction.Buy;

            if (marketPosition == MarketPosition.Long)
            {
                orderAction = OrderAction.Sell;
            }
            else if (marketPosition == MarketPosition.Short)
            {
                orderAction = OrderAction.Buy;
            }
            else
            {
                RealLogger.PrintOutput("Market position type  " + marketPosition.ToString() + " not supported.");
            }

            return orderAction;
        }

        private OrderAction ConvertMarketPositionToTPOrderAction(MarketPosition marketPosition)
        {
            OrderAction orderAction = OrderAction.Buy;

            if (marketPosition == MarketPosition.Long)
            {
                orderAction = OrderAction.Sell;
            }
            else if (marketPosition == MarketPosition.Short)
            {
                orderAction = OrderAction.Buy;
            }
            else
            {
                RealLogger.PrintOutput("Market position type  " + marketPosition.ToString() + " not supported.");
            }

            return orderAction;
        }


        private double GetSellSnapInfo(Instrument instrument, OrderAction orderAction, out OrderType orderType, out int orderQuantity)
        {
            double sellSnapPrice = 0;
            orderType = OrderType.Unknown;
            orderQuantity = 0;

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (RealOrderService.IsValidSellSnapOrder(order, instrument, orderAction))
                    {
                        sellSnapPrice = order.StopPrice;
                        orderType = order.OrderType;
                        orderQuantity = order.Quantity;
                        break;
                    }
                }
            }

            return sellSnapPrice;
        }


        private void AttemptToEngageAutobot()
        {
            if (!UsePlayThroughSleepMode)
            {
                if (UseHedgehogEntry)
                {
                    lock (NewPositionLock)
                    {
                        int positionsCount = RealPositionService.PositionsCount;

                        for (int index = 0; index < positionsCount; index++)
                        {
                            RealPosition position = null;
                            if (RealPositionService.TryGetByIndex(index, out position))
                            {
                                if (IsAccountFlat() && !HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
                                {
                                    if (HedgehogEntryBuySymbol1SellSymbol2)
                                    {
                                        if (DebugLogLevel > 2) RealLogger.PrintOutput("Hedgehog Entry buy (" + HedgehogEntrySymbol1FullName + ") sell (" + HedgehogEntrySymbol2FullName + ")", PrintTo.OutputTab1);

                                        //Interlocked.Exchange(ref activeCloseOrderCount, Account.Orders.Count());
                                        Instrument buyInstrument = Instrument.GetInstrument(HedgehogEntrySymbol1FullName);

                                        SubmitMarketOrder(buyInstrument, OrderAction.Buy, OrderEntry.Automated, AutobotEntryQuantity);


                                        //Order buyMarketorder = Account.CreateOrder(buyInstrument, OrderAction.Buy, OrderType.Market, OrderEntry.Automated, TimeInForce.Day, AutobotEntryQuantity, 0, 0, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                                        //Account.Submit(new[] { buyMarketorder });

                                        //uniqueSignalKey = Guid.NewGuid().ToString();
                                        //inFlighOrderCache.Add(uniqueSignalKey, AutobotEntryQuantity);

                                        Instrument sellInstrument = Instrument.GetInstrument(HedgehogEntrySymbol2FullName);

                                        SubmitMarketOrder(sellInstrument, OrderAction.Sell, OrderEntry.Automated, AutobotEntryQuantity);

                                        //Order sellMarketorder = Account.CreateOrder(sellInstrument, OrderAction.SellShort, OrderType.Market, OrderEntry.Automated, TimeInForce.Day, AutobotEntryQuantity, 0, 0, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                                        //Account.Submit(new[] { sellMarketorder });
                                    }
                                    else
                                    {
                                        if (DebugLogLevel > 2) RealLogger.PrintOutput("Hedgehog Entry buy (" + HedgehogEntrySymbol2FullName + ") sell (" + HedgehogEntrySymbol1FullName + ")", PrintTo.OutputTab1);

                                        //uniqueSignalKey = Guid.NewGuid().ToString();
                                        //inFlighOrderCache.Add(uniqueSignalKey, AutobotEntryQuantity);

                                        //Interlocked.Exchange(ref lastOrderCount, Account.Orders.Count());

                                        Instrument buyInstrument = Instrument.GetInstrument(HedgehogEntrySymbol2FullName);

                                        SubmitMarketOrder(buyInstrument, OrderAction.Buy, OrderEntry.Automated, AutobotEntryQuantity);

                                        //Order buyMarketorder = Account.CreateOrder(buyInstrument, OrderAction.Buy, OrderType.Market, OrderEntry.Automated, TimeInForce.Day, AutobotEntryQuantity, 0, 0, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                                        //Account.Submit(new[] { buyMarketorder });

                                        //uniqueSignalKey = Guid.NewGuid().ToString();
                                        //inFlighOrderCache.Add(uniqueSignalKey, AutobotEntryQuantity);

                                        Instrument sellInstrument = Instrument.GetInstrument(HedgehogEntrySymbol1FullName);

                                        SubmitMarketOrder(sellInstrument, OrderAction.Sell, OrderEntry.Automated, AutobotEntryQuantity);

                                        //Order sellMarketorder = Account.CreateOrder(sellInstrument, OrderAction.SellShort, OrderType.Market, OrderEntry.Automated, TimeInForce.Day, AutobotEntryQuantity, 0, 0, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                                        //Account.Submit(new[] { sellMarketorder });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AttemptAccountInfoLogging()
        {
            if (UseAccountInfoLogging)
            {
                double accountBalance = Math.Round(account.Get(AccountItem.CashValue, Currency.UsDollar), 2);
                double grossRealizedPnL = Math.Round(account.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar), 2);
                double realizedPnL = Math.Round(account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar), 2);

                double accountBalanceWithNewPnL = accountBalance;
                if (grossRealizedPnL != 0) accountBalanceWithNewPnL = accountBalance - grossRealizedPnL + realizedPnL;

                if (accountBalanceWithNewPnL != lastAccountBalance)
                {
                    RealLogger.PrintOutput("Logging account information - $" + accountBalanceWithNewPnL.ToString("N2"), PrintTo.OutputTab2);
                    string content = "ACCOUNT_BALANCE,ACCOUNT_EQUITY\r\n" + Convert.ToString(accountBalanceWithNewPnL) + "," + Convert.ToString(accountBalanceWithNewPnL);
                    File.WriteAllText(AccountInfoLoggingPath, content);
                    lastAccountBalance = accountBalanceWithNewPnL;
                }
            }
        }

        private double GetPositionProfitWithStoLoss(Instrument instrument, MarketPosition marketPosition, int quantity, double averagePrice, double stopLossPrice)
        {
            double positionProfit = 0;

            double tickValue = RealInstrumentService.GetTickValue(instrument);
            double tickSize = instrument.MasterInstrument.TickSize;

            if (marketPosition == MarketPosition.Long)
            {
                positionProfit = (stopLossPrice - averagePrice) * ((tickValue * quantity) / tickSize);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                positionProfit = (averagePrice - stopLossPrice) * ((tickValue * quantity) / tickSize);
            }

            double commissionPerSide = GetCommissionPerSide(instrument);
            bool includeCommissions = (commissionPerSide > 0);

            if (includeCommissions)
            {
                positionProfit = positionProfit - (quantity * commissionPerSide * 2);
            }


            return (Math.Round(positionProfit, 2, MidpointRounding.ToEven));

        }

        private double GetPositionProfit(RealPosition position)
        {
            double positionProfit = 0;
            double totalVolume = position.Quantity;
            double averagePrice = position.AveragePrice;
            double tickValue = RealInstrumentService.GetTickValue(position.Instrument);
            double tickSize = position.Instrument.MasterInstrument.TickSize;

            if (position.MarketPosition == MarketPosition.Long)
            {
                double bid = RealInstrumentService.GetBidPrice(position.Instrument);

                positionProfit = (bid - averagePrice) * ((tickValue * totalVolume) / tickSize);
            }
            else if (position.MarketPosition == MarketPosition.Short)
            {
                double ask = RealInstrumentService.GetAskPrice(position.Instrument);

                positionProfit = (averagePrice - ask) * ((tickValue * totalVolume) / tickSize);
            }

            double commissionPerSide = GetCommissionPerSide(position.Instrument);
            bool includeCommissions = (commissionPerSide > 0);

            if (includeCommissions)
            {
                positionProfit = positionProfit - (totalVolume * commissionPerSide * 2);
            }


            return (Math.Round(positionProfit, 2, MidpointRounding.ToEven));

        }


        private bool IsMicroInstrument(Instrument instrument)
        {
            bool returnFlag = false;

            if (instrument.FullName.StartsWith(MYMPrefix) || instrument.FullName.StartsWith(MESPrefix) || instrument.FullName.StartsWith(M2KPrefix) || instrument.FullName.StartsWith(MNQPrefix))
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        private bool IsEminiInstrument(Instrument instrument)
        {
            bool returnFlag = false;

            if (instrument.FullName.StartsWith(YMPrefix) || instrument.FullName.StartsWith(ESPrefix) || instrument.FullName.StartsWith(RTYPrefix) || instrument.FullName.StartsWith(NQPrefix))
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        private double GetCommissionPerSide(Instrument instrument)
        {
            double commissionPerSide = 0;

            if (account != null && account.Commission != null && account.Commission.ByMasterInstrument.ContainsKey(instrument.MasterInstrument))
            {
                commissionPerSide = account.Commission.ByMasterInstrument[instrument.MasterInstrument].PerUnit;
            }
            else
            {
                //RealLogger.PrintOutput("ERROR: Missing commission per side for instrument '" + instrument.FullName + "'");
            }

            /*
            if (IsMicroInstrument(instrument))
            {
                commissionPerSide = MicroCommissionPerSide;
            }
            else if (IsEminiInstrument(instrument))
            {
                commissionPerSide = EminiCommissionPerSide;
            }
            else
            {
                RealLogger.PrintOutput("ERROR: Missing commission per side for instrument '" + instrument.FullName + "'");
            }
            */

            return commissionPerSide;
        }

        private void AttemptToClosePositionsInProfit()
        {
            if (isECATPEnabled || UsePositionProfitLogging)
            {
                if (!IsAccountFlat() && !HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
                {
                    int totalVolume = 0;
                    int totalMicroVolume = 0;
                    int totalEminiVolume = 0;
                    double totalUnrealizedProfitLoss = 0;
                    double unrealizedProfitLoss = 0;
                    bool hasPosition = false;
                    bool hasAttachedPosition = false;
                    int positionQuantity = 0;

                    int positionsCount = RealPositionService.PositionsCount;

                    for (int index = 0; index < positionsCount; index++)
                    {
                        RealPosition position = null;
                        if (RealPositionService.TryGetByIndex(index, out position))
                        {
                            hasPosition = true;
                            if (position.Instrument == attachedInstrument) hasAttachedPosition = true;
                            positionQuantity = position.Quantity;
                            totalVolume += positionQuantity;
                            if (IsMicroInstrument(position.Instrument)) totalMicroVolume += positionQuantity;
                            else if (IsEminiInstrument(position.Instrument)) totalEminiVolume += positionQuantity;
                            unrealizedProfitLoss = GetPositionProfit(position);
                            totalUnrealizedProfitLoss += Math.Round(unrealizedProfitLoss, 2);
                        }
                    }

                    if (hasPosition)
                    {
                        double expectedProfit = (ECATakeProfitDollarsPerMicroVolume * totalMicroVolume) + (ECATakeProfitDollarsPerEminiVolume * totalEminiVolume);
                        
                        if (ECATakeProfitATRMultiplierPerVolume > 0)
                        {
                            int atrTicks = CalculateATRTicks(atrValue, ECATakeProfitATRMultiplierPerVolume, attachedInstrumentTicksPerPoint);
                            double atrExpectedProfit = RealInstrumentService.ConvertTicksToDollars(attachedInstrument, atrTicks, totalMicroVolume) + RealInstrumentService.ConvertTicksToDollars(attachedInstrument, atrTicks, totalEminiVolume);

                            if (atrExpectedProfit > expectedProfit)
                            {
                                expectedProfit = atrExpectedProfit;
                            }
                        }

                        if (UsePositionProfitLogging && hasAttachedPosition) RealLogger.PrintOutput("Total vs Target PnL: $" + totalUnrealizedProfitLoss.ToString("N2") + " vs $" + expectedProfit.ToString("N2") + " with " + Convert.ToString(totalVolume) + " volume", PrintTo.OutputTab1, true);

                        if (isECATPEnabled)
                        {
                            if (totalUnrealizedProfitLoss >= expectedProfit)
                            {
                                FlattenEverything("EquityCloseAllTakeProfit", true);
                            }
                        }
                    }
                    
                }

            }
        }


        
        
        private void AttemptToClosePositionsInLoss()
        {
            if (IsMaxDDStopLossEnabled() || IsEquityRemainingStopLossEnabled())
            {
                if (!IsAccountFlat() && !HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
                {
                    int totalVolume = 0;
                    int totalMicroVolume = 0;
                    int totalEminiVolume = 0;
                    double totalUnrealizedProfitLoss = 0;
                    double unrealizedProfitLoss = 0;
                    int positionQuantity = 0;

                    int positionsCount = RealPositionService.PositionsCount;

                    for (int index = 0; index < positionsCount; index++)
                    {
                        RealPosition position = null;
                        if (RealPositionService.TryGetByIndex(index, out position))
                        {
                            positionQuantity = position.Quantity;
                            totalVolume += positionQuantity;
                            if (IsMicroInstrument(position.Instrument)) totalMicroVolume += positionQuantity;
                            else if (IsEminiInstrument(position.Instrument)) totalEminiVolume += positionQuantity;
                            unrealizedProfitLoss = GetPositionProfit(position);
                            totalUnrealizedProfitLoss += Math.Round(unrealizedProfitLoss, 2);
                        }
                    }

                    if (totalVolume > 0 && totalUnrealizedProfitLoss < 0)
                    {
                        //if (UseDebugLogging) RealLogger.PrintOutput("Max DD: $" + totalUnrealizedProfitLoss.ToString("N2") + " vs DD $" + maxDDInDollars.ToString("N2") + " with " + Convert.ToString(totalVolume) + " volume", PrintTo.OutputTab1, true);
                        double netLiquidationBalance = Math.Round(account.Get(AccountItem.NetLiquidation, Currency.UsDollar), 2);

                        if (IsMaxDDStopLossEnabled() && totalUnrealizedProfitLoss <= maxDDInDollars)
                        {
                            RealLogger.PrintOutput("Max DD reached: $" + totalUnrealizedProfitLoss.ToString("N2") + " ($" + maxDDInDollars.ToString("N2") + ") with " + Convert.ToString(totalVolume) + " volume", PrintTo.OutputTab1, false);

                            FlattenEverything("EquityCloseAllStopLoss", true);
                        }
                        else if (IsEquityRemainingStopLossEnabled() && netLiquidationBalance <= equityRemainingInDollars)
                        {
                            RealLogger.PrintOutput("Equity remaining reached: $" + netLiquidationBalance.ToString("N2") + " ($" + equityRemainingInDollars.ToString("N2") + ") with " + Convert.ToString(totalVolume) + " volume", PrintTo.OutputTab1, false);

                            FlattenEverything("EquityCloseAllStopLoss", true);
                        }
                    }
                }
            }
        }

        private void CreateSellSnap(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }


            OrderType orderType = OrderType.StopMarket;

            lock (account.Orders)
            {
                string uniqueSignalKey = RealOrderService.BuildSNAPUniqueId();

                RealOrderService.InFlightOrderCache.RegisterUniqueId(uniqueSignalKey);

                try
                {
                    Order stopLossorder = account.CreateOrder(instrument, orderAction, orderType, orderEntry, TimeInForce.Day, quantity, 0, price, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                    account.Submit(new[] { stopLossorder });
                }
                catch (Exception ex)
                {
                    RealLogger.PrintOutput("Exception in CreateSellSnap:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                }
            }
        }

        private void CreateBuySnap(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }

            OrderType orderType = OrderType.StopMarket;

            lock (account.Orders)
            {
                string uniqueSignalKey = RealOrderService.BuildSNAPUniqueId();

                RealOrderService.InFlightOrderCache.RegisterUniqueId(uniqueSignalKey);

                try
                {
                    Order stopLossorder = account.CreateOrder(instrument, orderAction, orderType, orderEntry, TimeInForce.Day, quantity, 0, price, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                    account.Submit(new[] { stopLossorder });
                }
                catch (Exception ex)
                {
                    RealLogger.PrintOutput("Exception in CreateBuySnap:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                }
            }
        }

        private void UpdatePopOrder(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }

            double lastPrice = RealInstrumentService.GetLastPrice(instrument);

            
            if (orderAction == OrderAction.Buy && price <= lastPrice)
            {
                RealLogger.PrintOutput("ERROR: Pop order price must be greater than last price.");
                return;
            }
            else if (orderAction == OrderAction.Sell && price >= lastPrice)
            {
                RealLogger.PrintOutput("ERROR: Pop order price must be less than last price.");
                return;
            }

            bool orderChanged = false;
            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (RealOrderService.IsValidBuySnapOrder(order, instrument, OrderAction.Buy) || RealOrderService.IsValidSellSnapOrder(order, instrument, OrderAction.Sell))
                    {
                        orderChanged = false;

                        if (order.StopPrice != price)
                        {
                            order.StopPriceChanged = price;
                            orderChanged = true;
                        }

                        if (orderChanged)
                        {

                            string keyName = order.Name;
                            if (!RealOrderService.InFlightOrderCache.Contains(keyName))
                            {
                                RealOrderService.InFlightOrderCache.RegisterUniqueId(keyName);
                                try
                                {
                                    account.Change(new[] { order });
                                }
                                catch (Exception ex)
                                {
                                    RealLogger.PrintOutput("Exception in UpdatePositionStopLoss:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                                }
                            }
                        }
                    }
                }
            }

        }


        private void CreatePositionStopLoss(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }

            double lastPrice = RealInstrumentService.GetLastPrice(instrument);
            bool isValid = RealOrderService.IsValidStopLossPrice(instrument, orderAction, price, lastPrice);
            if (orderAction == OrderAction.Buy && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Stop Loss order price must be greater than last price.");
                return;
            }
            else if (orderAction == OrderAction.Sell && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Stop Loss order price must be less than last price.");
                return;
            }

            OrderType orderType = OrderType.StopMarket;

            lock (account.Orders)
            {
                string uniqueSignalKey = RealOrderService.BuildStopLossUniqueId();

                RealOrderService.InFlightOrderCache.RegisterUniqueId(uniqueSignalKey);

                try
                {
                    Order stopLossorder = account.CreateOrder(instrument, orderAction, orderType, orderEntry, TimeInForce.Day, quantity, 0, price, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                    account.Submit(new[] { stopLossorder });
                }
                catch (Exception ex)
                {
                    RealLogger.PrintOutput("Exception in CreatePositionStopLoss:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                }
            }
        }

        private void UpdatePositionStopLoss(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }

            double lastPrice = RealInstrumentService.GetLastPrice(instrument);
            bool isValid = RealOrderService.IsValidStopLossPrice(instrument, orderAction, price, lastPrice);
            if (orderAction == OrderAction.Buy && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Stop Loss order price must be greater than last price.");
                return;
            }
            else if (orderAction == OrderAction.Sell && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Stop Loss order price must be less than last price.");
                return;
            }

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (RealOrderService.IsValidStopLossOrder(order, instrument, orderAction))
                    {
                        bool orderChanged = false;
                        if (order.Quantity != quantity)
                        {
                            order.QuantityChanged = quantity;
                            orderChanged = true;
                        }
                        if (order.StopPrice != price)
                        {
                            order.StopPriceChanged = price;
                            orderChanged = true;
                        }

                        if (orderChanged)
                        {
                            
                            string keyName = order.Name;
                            if (!RealOrderService.InFlightOrderCache.Contains(keyName))
                            {
                                RealOrderService.InFlightOrderCache.RegisterUniqueId(keyName);
                                try
                                {
                                    account.Change(new[] { order });
                                }
                                catch (Exception ex)
                                {
                                    RealLogger.PrintOutput("Exception in UpdatePositionStopLoss:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                                }
                            }
                        }
                    }
                }
            }

        }

        private void CreatePositionTakeProfit(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }

            double lastPrice = RealInstrumentService.GetLastPrice(instrument);
            bool isValid = RealOrderService.IsValidTakeProfitPrice(instrument, orderAction, price, lastPrice);
            if (orderAction == OrderAction.Buy && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Take Profit order price must be less than last price.");
                return;
            }
            else if (orderAction == OrderAction.Sell && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Take Profit order price must be greater than last price.");
                return;
            }

            OrderType orderType = OrderType.Limit;

            lock (account.Orders)
            {
                string uniqueSignalKey = RealOrderService.BuildTakeProfitUniqueId();

                RealOrderService.InFlightOrderCache.RegisterUniqueId(uniqueSignalKey);

                try
                {
                    Order takeProfitOrder = account.CreateOrder(instrument, orderAction, orderType, orderEntry, TimeInForce.Day, quantity, price, 0, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                    account.Submit(new[] { takeProfitOrder });
                }
                catch (Exception ex)
                {
                    RealLogger.PrintOutput("Exception in CreatePositionTakeProfit:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                }

            }
        }



        private bool CancelPositionSLTPOrders(string signalName, Instrument instrument, OrderAction? orderAction = null)
        {
            bool returnFlag = false;
            bool closeAll = false;
            OrderAction tempOrderAction = OrderAction.Buy;

            if (orderAction == null)
                closeAll = true;
            else if (orderAction == OrderAction.Buy)
                tempOrderAction = OrderAction.Buy;
            else if (orderAction == OrderAction.Sell)
                tempOrderAction = OrderAction.Sell;
            else
            {
                RealLogger.PrintOutput("Order action type not supported: " + Convert.ToString(orderAction));
            }

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (closeAll && (RealOrderService.IsValidStopLossOrder(order, instrument, OrderAction.Buy) || RealOrderService.IsValidStopLossOrder(order, instrument, OrderAction.Sell)
                        || RealOrderService.IsValidTakeProfitOrder(order, instrument, OrderAction.Buy) || RealOrderService.IsValidTakeProfitOrder(order, instrument, OrderAction.Sell))
                        || (!closeAll && (RealOrderService.IsValidStopLossOrder(order, instrument, tempOrderAction) || RealOrderService.IsValidTakeProfitOrder(order, instrument, tempOrderAction))))
                    {
                        try
                        {
                            account.Cancel(new[] { order });
                            returnFlag = true;
                        }
                        catch (Exception ex)
                        {
                            RealLogger.PrintOutput("Exception in CancelPositionSLTPOrders:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                        }
                    }
                }
            }

            return returnFlag;
        }

        private void UpdatePositionTakeProfit(string signalName, Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, double price)
        {
            if (orderAction != OrderAction.Buy && orderAction != OrderAction.Sell)
            {
                RealLogger.PrintOutput("ERROR: Order action of " + orderAction.ToString() + " not supported.");
                return;
            }

            double lastPrice = RealInstrumentService.GetLastPrice(instrument);
            bool isValid = RealOrderService.IsValidTakeProfitPrice(instrument, orderAction, price, lastPrice);
            if (orderAction == OrderAction.Buy && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Take Profit order price must be less than last price.");
                return;
            }
            else if (orderAction == OrderAction.Sell && !isValid)
            {
                RealLogger.PrintOutput("ERROR: Take Profit order price must be greater than last price.");
                return;
            }

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (RealOrderService.IsValidTakeProfitOrder(order, instrument, orderAction))
                    {
                        bool orderChanged = false;
                        if (order.Quantity != quantity)
                        {
                            order.QuantityChanged = quantity;
                            orderChanged = true;
                        }
                        if (order.LimitPrice != price)
                        {
                            order.LimitPriceChanged = price;
                            orderChanged = true;
                        }

                        if (orderChanged)
                        {
                            string keyName = order.Name;
                            if (!RealOrderService.InFlightOrderCache.Contains(keyName))
                            {
                                RealOrderService.InFlightOrderCache.RegisterUniqueId(keyName);
                                account.Change(new[] { order });
                            }
                        }
                    }
                }
            }
        }

        private bool FlattenEverything(string signalName, bool continueTillZeroRemainingQuantity)
        {
            bool positionFound = false;

            if (!HasActiveMarketOrders() && !RealOrderService.InFlightOrderCache.HasElements())
            {
                CloseAllAccountPendingOrders(signalName);

                if (!IsAccountFlat())
                {
                    double unrealizedProfitLoss = 0;
                    OrderAction orderAction = OrderAction.Buy;
                    int positionsCount = RealPositionService.PositionsCount;

                    for (int index = 0; index < positionsCount; index++)
                    {
                        RealPosition position = null;
                        if (RealPositionService.TryGetByIndex(index, out position))
                        {
                            positionFound = true;
                            position.StoreState();
                            unrealizedProfitLoss = GetPositionProfit(position);

                            if (position.MarketPosition == MarketPosition.Long)
                                orderAction = OrderAction.Sell;
                            else if (position.MarketPosition == MarketPosition.Short)
                                orderAction = OrderAction.Buy;

                            if (position.Quantity > 0 && !position.HasStateChanged() && !position.IsFlat())
                            {
                                if (DebugLogLevel > 0) RealLogger.PrintOutput(signalName + " closing " + position.MarketPosition.ToString() + " " + position.Instrument.FullName + " Quantity=" + position.Quantity + " Profit=" + Convert.ToString(unrealizedProfitLoss), PrintTo.OutputTab1);

                                SubmitMarketOrder(position.Instrument, orderAction, OrderEntry.Automated, position.Quantity, continueTillZeroRemainingQuantity);
                            }

                            if (!continueTillZeroRemainingQuantity) break;
                        }
                    }
                }
            }

            return positionFound;
        }

        private bool IsMaxDDStopLossEnabled()
        {
            bool returnFlag = false;

            if (maxDDInDollars < 0)
            {
                returnFlag = true;
            }

            return (returnFlag);
        }

        private bool IsEquityRemainingStopLossEnabled()
        {
            bool returnFlag = false;

            if (equityRemainingInDollars > 0)
            {
                returnFlag = true;
            }

            return (returnFlag);
        }

        private int GetRandomNumber(int maxValue)
        {
            int randomNumber = 0;

            if (maxValue == 1)
            {
                randomNumber = maxValue;
            }
            else if (maxValue > 0)
            {
                int minValue = (int)maxValue / 2; // half number to create range
                Random random = new Random();
                randomNumber = random.Next(minValue, maxValue + 1);
            }

            return randomNumber;
        }

        private void SubmitMarketOrder(Instrument instrument, OrderAction orderAction, OrderEntry orderEntry, int quantity, bool continueTillZeroRemainingQuantity = true)
        {
            string uniqueSignalKey;
            int quantityRemaining = quantity;
            int chunkedQuantity = 0;
            int cycleCount = 1;

            lock (MarketOrderLock)
            {
                if (quantityRemaining > 0)
                {
                    lock (MarketOrderLock)
                    {
                        while (quantityRemaining > 0)
                        {
                            if (quantityRemaining > this.SingleOrderChunkMaxQuantity)
                            {
                                int randomQuantity = GetRandomNumber(this.SingleOrderChunkMaxQuantity);
                                chunkedQuantity = randomQuantity;
                            }
                            else if (quantityRemaining > this.SingleOrderChunkMinQuantity)
                            {
                                int randomQuantity = GetRandomNumber(this.SingleOrderChunkMinQuantity);
                                chunkedQuantity = randomQuantity;
                            }
                            else
                            {
                                chunkedQuantity = quantityRemaining;
                            }

                            quantityRemaining -= chunkedQuantity;

                            if (cycleCount > 1 && SingleOrderChunkDelayMilliseconds > 0) Thread.Sleep(SingleOrderChunkDelayMilliseconds);

                            uniqueSignalKey = Guid.NewGuid().ToString();

                            RealOrderService.InFlightOrderCache.RegisterUniqueId(uniqueSignalKey);

                            try
                            {
                                Order marketorder = account.CreateOrder(instrument, orderAction, OrderType.Market, orderEntry, TimeInForce.Day, chunkedQuantity, 0, 0, "", uniqueSignalKey, Core.Globals.MaxDate, null);
                                account.Submit(new[] { marketorder });
                            }
                            catch (Exception ex)
                            {
                                RealLogger.PrintOutput("Exception in SubmitMarketOrder:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                            }

                            cycleCount++;

                            if (!continueTillZeroRemainingQuantity) break;
                        }
                    }
                }
            }
        }

        private bool IsAccountFlat()
        {
            bool returnFlag = true;

            returnFlag = (RealPositionService.PositionsCount == 0);

            return returnFlag;
        }

        private bool HasActiveMarketOrders()
        {
            bool hasActiveMarketOrders = false;
            bool isActiveMarketOrder = false;
            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    isActiveMarketOrder = (order.IsMarket && !Order.IsTerminalState(order.OrderState));
                    
                    if (isActiveMarketOrder)
                    {
                        hasActiveMarketOrders = true;
                        break;
                    }
                }
            }

            return hasActiveMarketOrders;
        }

        private void CloseAllAccountPendingOrders(string signalName)
        {
            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (!Order.IsTerminalState(order.OrderState))
                    {
                        if (DebugLogLevel > 0) RealLogger.PrintOutput(signalName + " is cancelling pending order " + order.Instrument.FullName + " Type=" + order.OrderType.ToString());

                        try
                        {
                            account.Cancel(new[] { order });
                        }
                        catch (Exception ex)
                        {
                            RealLogger.PrintOutput("Exception in CloseAllAccountPendingOrders:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
                        }
                    }
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            if (!hasDrawnButtons)
            {
                if (HasRanOnceFirstCycle() && IsStrategyAttachedToChart())
                {
                    DrawButtonPanel();
                    SetButtonPanelVisiblity();
                    hasDrawnButtons = true;
                }
            }
        }

        private bool HasRanOnceFirstCycle()
        {
            if (!hasRanOnceFirstCycle && attachedInstrumentServerSupported && BarsInProgress == 0 && CurrentBar > 0) //&& BarsInProgress == 0 && this.State == State.Realtime)
            {
                //RealOrderService.InFlightOrderCache.Clear();
                this.RealOrderService = new RealOrderService();
                this.RealPositionService = new RealPositionService();

                lastOrderOutputTime = DateTime.MinValue;

                LoadAccount();

                LoadPositions();

                RealLogger.PrintOutput("Max DD $" + maxDDInDollars.ToString("N2") + " / equity remaining $" + equityRemainingInDollars.ToString("N2"), PrintTo.OutputTab1);
                RealLogger.PrintOutput("Max DD $" + maxDDInDollars.ToString("N2") + " / equity remaining $" + equityRemainingInDollars.ToString("N2"), PrintTo.OutputTab2);

                RealLogger.PrintOutput("Detected commission per side: " + GetCommissionPerSide(attachedInstrument).ToString("N2") + " for " + attachedInstrument.FullName, PrintTo.OutputTab1);
                RealLogger.PrintOutput("Detected commission per side: " + GetCommissionPerSide(attachedInstrument).ToString("N2") + " for " + attachedInstrument.FullName, PrintTo.OutputTab2);

                if (UseHedgehogEntry && attachedInstrumentIsFuture)
                {
                    RealLogger.PrintOutput("Validating HedgehogEntrySymbol1...", PrintTo.OutputTab2);
                    ValidateInstrument(HedgehogEntrySymbol1FullName);

                    RealLogger.PrintOutput("Validating HedgehogEntrySymbol2...", PrintTo.OutputTab2);
                    ValidateInstrument(HedgehogEntrySymbol2FullName);
                }

               

                

                /*
                if (ChartControl.Dispatcher.CheckAccess())
                {
                    LoadAccount();
                }
                else
                {
                    ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                    {
                        LoadAccount();
                    }));
                }
                */

                /*

                if (IsStrategyAttachedToChart() && UserControlCollection.Contains(buttonGrid))
                {
                    if (ChartControl != null)
                    {
                        if (ChartControl.Dispatcher.CheckAccess())
                        {
                            SetButtonPanelVisiblity();
                        }
                        else
                        {
                            ChartControl.Dispatcher.InvokeAsync((() =>
                            {
                                SetButtonPanelVisiblity();
                            }));
                        }
                    }
                }
                */

                hasRanOnceFirstCycle = true;
            }

            return hasRanOnceFirstCycle;
        }

        private void OnAccountStatusUpdate(object sender, AccountStatusEventArgs e)

        {

            // Output the account name and status

            NinjaTrader.Code.Output.Process(string.Format("Account: {0} Status: {1}",

                 e.Account.Name, e.Status), PrintTo.OutputTab1);

        }

        private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)

        {

            // Output the account item

            NinjaTrader.Code.Output.Process(string.Format("Account: {0} AccountItem: {1} Value: {2}",

                 e.Account.Name, e.AccountItem, e.Value), PrintTo.OutputTab1);

        }

        private void OnExecutionUpdate(object sender, ExecutionEventArgs e)

        {

            // Output the execution

            NinjaTrader.Code.Output.Process(string.Format("Instrument: {0} Quantity: {1} Price: {2}",

                 e.Execution.Instrument.FullName, e.Quantity, e.Price), PrintTo.OutputTab1);

        }

        private Account GetAccount()
        {
            Account tempAccount = null;

            try
            {
                tempAccount = this.ChartControl.OwnerChart.ChartTrader.Account;
            }
            catch
            {
                //stuff exception
            }

            return tempAccount;
        }

        private void LoadAccount(Account newAccount = null)
        {
            //lock (account)
            {
                Account tempAccount = newAccount;

                if (newAccount == null)
                {
                    tempAccount = GetAccount();
                }
                else
                {
                    tempAccount = newAccount;
                }

                if (tempAccount != null && tempAccount != account)
                {
                    if (account != null) UnloadAccountEvents();

                    account = tempAccount;

                    RealLogger.PrintOutput("Found account name (" + Convert.ToString(account.DisplayName) + ")", PrintTo.OutputTab1);
                    RealLogger.PrintOutput("Found account name (" + Convert.ToString(account.DisplayName) + ")", PrintTo.OutputTab2);

                    //WeakEventManager<Account, AccountStatusEventArgs>.AddHandler(account, "AccountStatusUpdate", OnAccountStatusUpdate);
                    //WeakEventManager<Account, AccountItemEventArgs>.AddHandler(account, "AccountItemUpdate", OnAccountItemUpdate);
                    //WeakEventManager<Account, ExecutionEventArgs>.AddHandler(account, "ExecutionUpdate", OnExecutionUpdate);

                    WeakEventManager<Account, OrderEventArgs>.AddHandler(account, "OrderUpdate", OnOrderUpdate);
                    //WeakEventManager<Account, PositionEventArgs>.AddHandler(account, "PositionUpdate", OnPositionUpdate);

                }
                else if (tempAccount == null)
                {
                    RealLogger.PrintOutput("Account name not found.", PrintTo.OutputTab1);
                    RealLogger.PrintOutput("Account name not found.", PrintTo.OutputTab2);
                }
            }


            /*
            Window currentWindow = Window.GetWindow(ChartControl.Parent);
            if (currentWindow != null)
            {
                NinjaTrader.Gui.Tools.AccountSelector accountSelector = currentWindow.FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
                if (accountSelector != null)
                {
                    if (accountSelector.SelectedAccount != null)
                    {
                        RealLogger.PrintOutput("*** Found account name (" + Convert.ToString(accountSelector.SelectedAccount.DisplayName) + ")", PrintTo.OutputTab2);
                        account = accountSelector.SelectedAccount; //ChartControl.OwnerChart.ChartTrader.Account 

                        if (account != null)
                        {
                            //Account.ExecutionUpdate += OnExecutionUpdate;
                            WeakEventManager<Account, OrderEventArgs>.AddHandler(account, "OrderUpdate", OnOrderUpdate);
                            WeakEventManager<Account, PositionEventArgs>.AddHandler(account, "PositionUpdate", OnPositionUpdate);
                            //Account.OrderUpdate += OnOrderUpdate;
                            // Subscribe to position updates
                            //Account.PositionUpdate += OnPositionUpdate;

                        }
                    }
                }
                else
                    RealLogger.PrintOutput("*** Account name not found.", PrintTo.OutputTab2);

            }
            else
                RealLogger.PrintOutput("*** Account name not found and no window.", PrintTo.OutputTab2);
            */
        }

        private void SetButtonPanelVisiblity()
        {
            if (buttonGrid != null)
            {
                if (riskInfoLabel != null)
                {
                    riskInfoLabel.Visibility = Visibility.Visible;
                }
                if (revButton != null)
                {
                    revButton.Visibility = Visibility.Visible;
                }
                if (closeAllButton != null)
                {
                    closeAllButton.Visibility = Visibility.Visible;
                }
                if (toggleECAButton != null)
                {
                    toggleECAButton.Visibility = Visibility.Visible;
                }
                if (toggleAutoButton != null)
                {
                    toggleAutoButton.Visibility = Visibility.Visible;
                }
                if (TPButton != null)
                {
                    TPButton.Visibility = Visibility.Visible;
                }
                if (BEButton != null)
                {
                    BEButton.Visibility = Visibility.Visible;
                }
                if (SLButton != null)
                {
                    SLButton.Visibility = Visibility.Visible;
                }
                if (BuyPopButton != null)
                {
                    BuyPopButton.Visibility = Visibility.Visible;
                }
                if (SellPopButton != null)
                {
                    SellPopButton.Visibility = Visibility.Visible;
                }
                if (BuySnapButton != null)
                {
                    BuySnapButton.Visibility = Visibility.Visible;
                }
                if (SellSnapButton != null)
                {
                    SellSnapButton.Visibility = Visibility.Visible;
                }
            }
        }
        private bool IsStrategyAttachedToChart()
        {
            return (this.ChartBars != null);
        }

        private void RemoveButtonPanel()
        {
            if (buttonGrid != null)
            {
                if (UserControlCollection.Contains(buttonGrid))
                {
                    if (revButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(revButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(revButton);
                        revButton = null;
                    }
                    if (closeAllButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(closeAllButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(closeAllButton);
                        closeAllButton = null;
                    }
                    if (toggleECAButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(toggleECAButton, "Click", OnButtonClick);
                        buttonGrid.Children.Remove(toggleECAButton);
                        toggleECAButton = null;
                    }
                    if (toggleAutoButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(toggleAutoButton, "Click", OnButtonClick);
                        buttonGrid.Children.Remove(toggleAutoButton);
                        toggleAutoButton = null;
                    }
                    if (TPButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(TPButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(TPButton);
                        TPButton = null;
                    }
                    if (BEButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(BEButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(BEButton);
                        BEButton = null;
                    }
                    if (SLButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(SLButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(SLButton);
                        SLButton = null;
                    }

                    if (BuyPopButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(BuyPopButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(BuyPopButton);
                        BuyPopButton = null;
                    }

                    if (SellPopButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(SellPopButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(SellPopButton);
                        SellPopButton = null;
                    }

                    if (BuySnapButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(BuySnapButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(BuySnapButton);
                        BuySnapButton = null;
                    }

                    if (SellSnapButton != null)
                    {
                        WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.RemoveHandler(SellSnapButton, "Click", OnButtonClick);

                        buttonGrid.Children.Remove(SellSnapButton);
                        SellSnapButton = null;
                    }



                    buttonGrid = null;
                }
            }
        }

        private void DrawButtonPanel()
        {
            if (buttonGrid == null)
            {
                if (!UserControlCollection.Contains(buttonGrid))
                {
                    buttonGrid = new System.Windows.Controls.Grid
                    {
                        Name = "HHButtonGrid",
                        Margin = new Thickness(0, 0, 20, 0),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column3 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column4 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column5 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column6 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column7 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column8 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column9 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column10 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column11 = new System.Windows.Controls.ColumnDefinition();
                    System.Windows.Controls.ColumnDefinition column12 = new System.Windows.Controls.ColumnDefinition();

                    //column1.MaxWidth = 100; 
                    column2.MaxWidth = 70;
                    column3.MaxWidth = 70;
                    column4.MaxWidth = 70;
                    column5.MaxWidth = 100;
                    column6.MaxWidth = 70;
                    column7.MaxWidth = 70;
                    column8.MaxWidth = 70;
                    column9.MaxWidth = 70;
                    column10.MaxWidth = 70;
                    column11.MaxWidth = 70;
                    column12.MaxWidth = 70;


                    buttonGrid.ColumnDefinitions.Add(column1);
                    buttonGrid.ColumnDefinitions.Add(column2);
                    buttonGrid.ColumnDefinitions.Add(column3);
                    buttonGrid.ColumnDefinitions.Add(column4);
                    buttonGrid.ColumnDefinitions.Add(column5);
                    buttonGrid.ColumnDefinitions.Add(column6);
                    buttonGrid.ColumnDefinitions.Add(column7);
                    buttonGrid.ColumnDefinitions.Add(column8);
                    buttonGrid.ColumnDefinitions.Add(column9);
                    buttonGrid.ColumnDefinitions.Add(column10);
                    buttonGrid.ColumnDefinitions.Add(column11);
                    buttonGrid.ColumnDefinitions.Add(column12);

                    riskInfoLabel = new System.Windows.Controls.Label
                    {
                        Name = HHRiskInfoLabelName,
                        Content = "",
                        Foreground = Brushes.White,
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden,
                    };

                    revButton = new System.Windows.Controls.Button
                    {
                        Name = HHRevButtonName,
                        Content = "Rev",
                        Foreground = Brushes.White,
                        Background = Brushes.DarkOrange,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden,
                    };

                    closeAllButton = new System.Windows.Controls.Button
                    {
                        Name = HHCloseAllButtonName,
                        Content = "Flat",
                        Foreground = Brushes.White,
                        Background = Brushes.DarkGreen,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden,
                    };

                    string tempContent = (isECATPEnabled) ? ToggleECAButtonEnabledText : ToggleECAButtonDisabledText;
                    Brush tempBrush = (isECATPEnabled) ? Brushes.HotPink : Brushes.DimGray;

                    toggleECAButton = new System.Windows.Controls.Button
                    {
                        Name = HHToggleECAButtonName,
                        Content = tempContent,
                        Foreground = Brushes.White,
                        Background = tempBrush,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.Enabled)
                    {
                        tempContent = ToggleAutoBEButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail1Bar)
                    {
                        tempContent = ToggleAutoBET1BButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail2Bar)
                    {
                        tempContent = ToggleAutoBET2BButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail3Bar)
                    {
                        tempContent = ToggleAutoBET3BButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrail5Bar)
                    {
                        tempContent = ToggleAutoBET5BButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage1)
                    {
                        tempContent = ToggleAutoBETM1ButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage2)
                    {
                        tempContent = ToggleAutoBETM2ButtonEnabledText;
                    }
                    else if (currentBreakEvenAutoStatus == TickHunterBreakEvenAutoTypes.PlusTrailMovingAverage3)
                    {
                        tempContent = ToggleAutoBETM3ButtonEnabledText;
                    }
                    else
                    {
                        tempContent = ToggleAutoBEButtonDisabledText;
                    }

                    tempBrush = (currentBreakEvenAutoStatus != TickHunterBreakEvenAutoTypes.Disabled) ? Brushes.HotPink : Brushes.DimGray;

                    toggleAutoButton = new System.Windows.Controls.Button
                    {
                        Name = HHToggleAutoButtonName,
                        Content = tempContent,
                        Foreground = Brushes.White,
                        Background = tempBrush,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    TPButton = new System.Windows.Controls.Button
                    {
                        Name = HHTPButtonName,
                        Content = "TP+",
                        Foreground = Brushes.White,
                        Background = Brushes.Silver,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden

                    };

                    BEButton = new System.Windows.Controls.Button
                    {
                        Name = HHBEButtonName,
                        Content = "BE+",
                        Foreground = Brushes.White,
                        Background = Brushes.DarkCyan,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden

                    };

                    SLButton = new System.Windows.Controls.Button
                    {
                        Name = HHSLButtonName,
                        Content = "SL+",
                        Foreground = Brushes.White,
                        Background = Brushes.Silver,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    BuySnapButton = new System.Windows.Controls.Button
                    {
                        Name = HHBuySnapButtonName,
                        Content = "Snap+",
                        Foreground = Brushes.White,
                        Background = Brushes.Blue,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    SellSnapButton = new System.Windows.Controls.Button
                    {
                        Name = HHSellSnapButtonName,
                        Content = "Snap-",
                        Foreground = Brushes.White,
                        Background = Brushes.OrangeRed,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    BuyPopButton = new System.Windows.Controls.Button
                    {
                        Name = HHBuyPopButtonName,
                        Content = "Pop+",
                        Foreground = Brushes.White,
                        Background = Brushes.Blue,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    SellPopButton = new System.Windows.Controls.Button
                    {
                        Name = HHSellPopButtonName,
                        Content = "Pop-",
                        Foreground = Brushes.White,
                        Background = Brushes.OrangeRed,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 2, 0),
                        Visibility = (hasRanOnceFirstCycle) ? Visibility.Visible : Visibility.Hidden
                    };

                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(closeAllButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(revButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(toggleECAButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(toggleAutoButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(TPButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(BEButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(SLButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(BuySnapButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(SellSnapButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(BuyPopButton, "Click", OnButtonClick);
                    WeakEventManager<System.Windows.Controls.Button, RoutedEventArgs>.AddHandler(SellPopButton, "Click", OnButtonClick);

                    //closeAllButton.Click += OnButtonClick;
                    //toggleECAButton.Click += OnButtonClick;

                    System.Windows.Controls.Grid.SetColumn(riskInfoLabel, 0);
                    System.Windows.Controls.Grid.SetColumn(toggleAutoButton, 1);
                    System.Windows.Controls.Grid.SetColumn(revButton, 2);
                    System.Windows.Controls.Grid.SetColumn(closeAllButton, 3);
                    System.Windows.Controls.Grid.SetColumn(toggleECAButton, 4);
                    System.Windows.Controls.Grid.SetColumn(TPButton, 5);
                    System.Windows.Controls.Grid.SetColumn(BEButton, 6);
                    System.Windows.Controls.Grid.SetColumn(SLButton, 7);
                    System.Windows.Controls.Grid.SetColumn(BuySnapButton, 8);
                    System.Windows.Controls.Grid.SetColumn(SellSnapButton, 9);
                    System.Windows.Controls.Grid.SetColumn(BuyPopButton, 10);
                    System.Windows.Controls.Grid.SetColumn(SellPopButton, 11);

                    buttonGrid.Children.Add(riskInfoLabel);
                    buttonGrid.Children.Add(toggleAutoButton);
                    buttonGrid.Children.Add(revButton);
                    buttonGrid.Children.Add(closeAllButton);
                    buttonGrid.Children.Add(toggleECAButton);
                    buttonGrid.Children.Add(TPButton);
                    buttonGrid.Children.Add(BEButton);
                    buttonGrid.Children.Add(SLButton);
                    buttonGrid.Children.Add(BuyPopButton);
                    buttonGrid.Children.Add(SellPopButton);
                    buttonGrid.Children.Add(BuySnapButton);
                    buttonGrid.Children.Add(SellSnapButton);

                    UserControlCollection.Add(buttonGrid);
                }
            }
        }


        private void ValidateInstrument(string instrumentName)
        {
            Instrument instrument = Instrument.GetInstrument(instrumentName);
            if (instrument != null)
            {
                RealLogger.PrintOutput("Instrument =" + instrument.FullName + " Tick Size=" + Convert.ToString(instrument.MasterInstrument.TickSize) + " Tick Value=" + Convert.ToString(RealInstrumentService.GetTickValue(instrument)), PrintTo.OutputTab2);
            }
        }

        private string GetCurrentFuturesMonthYearPrefix()
        {
            string tempText = null;

			if (attachedInstrumentIsFuture)
			{
				tempText = this.attachedInstrument.FullName.Substring(this.attachedInstrument.MasterInstrument.Name.Length, 6);
			}
			return tempText;
        }

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "UseAutoPositionStopLoss", Order = 1, GroupName = "1) Order Management Settings")]
        public bool UseAutoPositionStopLoss
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseAutoPositionTakeProfit", Order = 2, GroupName = "1) Order Management Settings")]
        public bool UseAutoPositionTakeProfit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "AutoPositionBreakEvenType", Order = 3, GroupName = "1) Order Management Settings")]
        public TickHunterBreakEvenAutoTypes AutoPositionBreakEvenType
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "StopLossInitialTicks", Order = 4, GroupName = "1) Order Management Settings")]
        public int StopLossInitialTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "StopLossInitialATRMultiplier", Order = 5, GroupName = "1) Order Management Settings")]
        public double StopLossInitialATRMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "StopLossJumpTicks", Order = 6, GroupName = "1) Order Management Settings")]
        public int StopLossJumpTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BreakEvenInitialTicks", Order = 7, GroupName = "1) Order Management Settings")]
        public int BreakEvenInitialTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BreakEvenJumpTicks", Order = 8, GroupName = "1) Order Management Settings")]
        public int BreakEvenJumpTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "BreakEvenAutoTriggerTicks", Order = 9, GroupName = "1) Order Management Settings")]
        public int BreakEvenAutoTriggerTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "BreakEvenAutoTriggerATRMultiplier", Order = 10, GroupName = "1) Order Management Settings")]
        public double BreakEvenAutoTriggerATRMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BreakEvenAutoTrailMA1Period", Order = 11, GroupName = "1) Order Management Settings")]
        public int BreakEvenAutoTrailMA1Period
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BreakEvenAutoTrailMA2Period", Order = 12, GroupName = "1) Order Management Settings")]
        public int BreakEvenAutoTrailMA2Period
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BreakEvenAutoTrailMA3Period", Order = 13, GroupName = "1) Order Management Settings")]
        public int BreakEvenAutoTrailMA3Period
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "TakeProfitInitialTicks", Order = 14, GroupName = "1) Order Management Settings")]
        public int TakeProfitInitialTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "TakeProfitInitialATRMultiplier", Order = 15, GroupName = "1) Order Management Settings")]
        public double TakeProfitInitialATRMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "TakeProfitJumpTicks", Order = 16, GroupName = "1) Order Management Settings")]
        public int TakeProfitJumpTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SnapPopContracts", Order = 17, GroupName = "1) Order Management Settings")]
        public int SnapPopContracts
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseSnapPositionTPSL", Order = 18, GroupName = "1) Order Management Settings")]
        public bool UseSnapPositionTPSL
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "SnapPaddingTicks", Order = 19, GroupName = "1) Order Management Settings")]
        public int SnapPaddingTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "PopInitialTicks", Order = 20, GroupName = "1) Order Management Settings")]
        public int PopInitialTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "PopInitialATRMultiplier", Order = 21, GroupName = "1) Order Management Settings")]
        public double PopInitialATRMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "PopJumpTicks", Order = 22, GroupName = "1) Order Management Settings")]
        public int PopJumpTicks
        { get; set; }


        [NinjaScriptProperty]
        [Range(2, int.MaxValue)]
        [Display(Name = "ATRPeriod", Order = 23, GroupName = "1) Order Management Settings")]
        public int ATRPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "RefreshTPSLPaddingTicks", Order = 24, GroupName = "1) Order Management Settings")]
        public int RefreshTPSLPaddingTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "RefreshTPSLOrderDelaySeconds", Order = 25, GroupName = "1) Order Management Settings")]
        public int RefreshTPSLOrderDelaySeconds
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SingleOrderChunkMaxQuantity", Order = 26, GroupName = "1) Order Management Settings")]
        public int SingleOrderChunkMaxQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SingleOrderChunkMinQuantity", Order = 27, GroupName = "1) Order Management Settings")]
        public int SingleOrderChunkMinQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "SingleOrderChunkDelayMilliseconds", Order = 28, GroupName = "1) Order Management Settings")]
        public int SingleOrderChunkDelayMilliseconds
        { get; set; }


        [NinjaScriptProperty]
        [Display(Name = "UseECATakeProfit", Order = 1, GroupName = "2) Equity Close All Settings")]
        public bool UseECATakeProfit
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "ECATakeProfitDollarsPerMicroVolume", Order = 2, GroupName = "2) Equity Close All Settings")]
        public double ECATakeProfitDollarsPerMicroVolume
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "ECATakeProfitDollarsPerEminiVolume", Order = 3, GroupName = "2) Equity Close All Settings")]
        public double ECATakeProfitDollarsPerEminiVolume
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "ECATakeProfitATRMultiplierPerVolume", Order = 4, GroupName = "2) Equity Close All Settings")]
        public double ECATakeProfitATRMultiplierPerVolume
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "ECAStopLossMaxDDInDollars", Order = 5, GroupName = "2) Equity Close All Settings")]
        public double ECAStopLossMaxDDInDollars
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "ECAStopLossEquityRemainingInDollars", Order = 6, GroupName = "2) Equity Close All Settings")]
        public double ECAStopLossEquityRemainingInDollars
        { get; set; }

    
        [NinjaScriptProperty]
        [Display(Name = "UsePlayThroughSleepMode", Order = 1, GroupName = "3) Autobot Settings")]
        public bool UsePlayThroughSleepMode
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AutobotEntryQuantity", Order = 2, GroupName = "3) Autobot Settings")]
        public int AutobotEntryQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "AutobotEntryQuantityMax", Order = 3, GroupName = "3) Autobot Settings")]
        public int AutobotEntryQuantityMax
        { get; set; }

  
        [NinjaScriptProperty]
        [Display(Name = "UseHedgehogEntry", Order = 13, GroupName = "4) Hedgehog Settings")]
        public bool UseHedgehogEntry
        { get; set; }


        [NinjaScriptProperty]
        [Display(Name = "HedgehogEntryBuySymbol1SellSymbol2", Order = 14, GroupName = "4) Hedgehog Settings")]
        public bool HedgehogEntryBuySymbol1SellSymbol2
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "HedgehogEntrySymbol1", Order = 15, GroupName = "4) Hedgehog Settings")]
        public string HedgehogEntrySymbol1
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "HedgehogEntrySymbol2", Order = 16, GroupName = "4) Hedgehog Settings")]
        public string HedgehogEntrySymbol2
        { get; set; }


        [NinjaScriptProperty]
        [Display(Name = "UsePositionProfitLogging", Order = 1, GroupName = "5) Output Log Settings")]
        public bool UsePositionProfitLogging
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "DebugLogLevel", Order = 2, GroupName = "5) Output Log Settings")]
        public int DebugLogLevel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "OrderWaitOutputThrottleSeconds", Order = 3, GroupName = "5) Output Log Settings")]
        public int OrderWaitOutputThrottleSeconds
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseAccountInfoLogging", Order = 1, GroupName = "6) Account Logging Settings")]
        public bool UseAccountInfoLogging
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "AccountInfoLoggingPath", Order = 2, GroupName = "6) Account Logging Settings")]
        public string AccountInfoLoggingPath
        { get; set; }

        #endregion

    }
}

namespace NinjaTrader.NinjaScript.Indicators.THos
{
    public class RealUtility
    {

    }
    public class RealLogger
    {
        private string systemName = String.Empty;
        private int lastPrintOutputHashCode = 0;
        public RealLogger(string systemName)
        {
            this.systemName = systemName;
        }

        public void PrintOutput(string output, PrintTo outputTab = PrintTo.OutputTab1, bool blockDuplicateMessages = false)
        {
            if (blockDuplicateMessages)
            {
                int tempHashCode = output.GetHashCode();
                if (tempHashCode != lastPrintOutputHashCode)
                {
                    Output.Process(DateTime.Now + " " + systemName + ": " + output, outputTab);
                }
                lastPrintOutputHashCode = tempHashCode;
            }
            else
                Output.Process(DateTime.Now + " " + systemName + ": " + output, outputTab);
        }
    }
    public class RealInstrumentService
    {
        private readonly ConcurrentDictionary<string, double> askPriceCache = new ConcurrentDictionary<string, double>();
        private readonly ConcurrentDictionary<string, double> bidPriceCache = new ConcurrentDictionary<string, double>();
        private readonly ConcurrentDictionary<string, double> lastPriceCache = new ConcurrentDictionary<string, double>();
        private readonly Dictionary<double, int> tickSizeDecimalPlaceCountCache = new Dictionary<double, int>();

        private string BuildKeyName(Instrument instrument)
        {
            string keyName = instrument.FullName;

            return keyName;
        }

        public double NormalizePrice(Instrument instrument, double price)
        {
            double newPrice = 0;
            int decimalPlaces = GetTickSizeDecimalPlaces(instrument.MasterInstrument.TickSize);

            string formatText = string.Concat("N", decimalPlaces);
            string stringPriceValue = price.ToString(formatText);
            newPrice = double.Parse(stringPriceValue);

            return newPrice;
        }
        public int GetTickSizeDecimalPlaces(double tickSize)
        {
            int decimalPlaceCount = 0;

            if (tickSize < 0) return decimalPlaceCount;

            if (tickSizeDecimalPlaceCountCache.ContainsKey(tickSize))
            {
                decimalPlaceCount = tickSizeDecimalPlaceCountCache[tickSize];
            }
            else
            {
                var parts = tickSize.ToString(CultureInfo.InvariantCulture).Split('.');

                if (parts.Length < 2)
                    decimalPlaceCount = 0;
                else
                    decimalPlaceCount = parts[1].TrimEnd('0').Length;

                tickSizeDecimalPlaceCountCache.Add(tickSize, decimalPlaceCount);
            }

            return decimalPlaceCount;
        }

        public static double ConvertTicksToDollars(Instrument instrument, int ticks, int contracts)
        {
            double dollarValue = 0;

            if (ticks > 0 && contracts > 0)
            {
                double tickValue = GetTickValue(instrument);
                double tickSize = GetTickSize(instrument);

                dollarValue = tickValue * ticks * contracts;
            }

            return dollarValue;
        }
        public static double GetTickValue(Instrument instrument)
        {
            double tickValue = instrument.MasterInstrument.PointValue * instrument.MasterInstrument.TickSize;

            return tickValue;
        }

        public static int GetTicksPerPoint(double tickSize)
        {
            int tickPoint = 1;

            if (tickSize < 1)
            {
                tickPoint = (int)(1.0 / tickSize);
            }

            return (tickPoint);
        }
        public static bool IsFutureInstrumentType(Instrument instrument)
        {
            bool isFuture = (instrument.MasterInstrument.InstrumentType == InstrumentType.Future);
            return isFuture;

        }
        public static double GetTickSize(Instrument instrument)
        {
            double tickSize = instrument.MasterInstrument.TickSize;

            return (tickSize);
        }

        public double GetLastPrice(Instrument instrument)
        {
            double lastPrice = 0;
            string keyName = BuildKeyName(instrument);

            if (!lastPriceCache.TryGetValue(keyName, out lastPrice))
            {
                if (instrument.MarketData != null && instrument.MarketData.Last != null)
                {
                    lastPrice = instrument.MarketData.Last.Price;
                }
                else if (instrument.MarketData != null && instrument.MarketData.Bid != null)
                {
                    lastPrice = instrument.MarketData.Bid.Price;
                }
                else
                {
                    lastPrice = 0;
                }
            }

            return lastPrice;
        }

        public double GetAskPrice(Instrument instrument)
        {
            double askPrice = 0;
            string keyName = BuildKeyName(instrument);

            if (!askPriceCache.TryGetValue(keyName, out askPrice))
            {
                if (instrument.MarketData != null && instrument.MarketData.Last != null)
                {
                    askPrice = instrument.MarketData.Last.Ask;
                }
                else if (instrument.MarketData != null && instrument.MarketData.Ask != null)
                {
                    askPrice = instrument.MarketData.Ask.Price;
                }
                else
                {
                    askPrice = 0;
                }
            }

            return askPrice;
        }

        public double GetBidPrice(Instrument instrument)
        {
            double bidPrice = 0;
            string keyName = BuildKeyName(instrument);

            if (!bidPriceCache.TryGetValue(keyName, out bidPrice))
            {
                if (instrument.MarketData != null && instrument.MarketData.Last != null)
                {
                    bidPrice = instrument.MarketData.Last.Bid;
                }
                else if (instrument.MarketData != null && instrument.MarketData.Bid != null)
                {
                    bidPrice = instrument.MarketData.Bid.Price;
                }
                else
                {
                    bidPrice = 0;
                }
            }

            return bidPrice;
        }

        public double SetAskPrice(Instrument instrument, double askPrice)
        {
            string keyName = BuildKeyName(instrument);

            double newPrice = askPriceCache.AddOrUpdate(String.Copy(keyName), askPrice, (oldkey, oldvalue) => askPrice);

            return newPrice;
        }

        public double SetBidPrice(Instrument instrument, double bidPrice)
        {
            string keyName = BuildKeyName(instrument);

            double newPrice = bidPriceCache.AddOrUpdate(String.Copy(keyName), bidPrice, (oldkey, oldvalue) => bidPrice);

            return newPrice;
        }

        public double SetLastPrice(Instrument instrument, double lastPrice)
        {
            string keyName = BuildKeyName(instrument);

            double newPrice = lastPriceCache.AddOrUpdate(String.Copy(keyName), lastPrice, (oldkey, oldvalue) => lastPrice);

            return newPrice;
        }
    }


public class RealInFlightOrderCache
    {
        private Dictionary<string, int> inFlighOrderCache = new Dictionary<string, int>();

        public int Count
        {
            get { return inFlighOrderCache.Count; }
        }
        public bool HasElements()
        {
            bool returnFlag = false;

            returnFlag = (inFlighOrderCache.Count != 0);

            return returnFlag;
        }

        public bool Contains(string uniqueId)
        {
            bool returnFlag = inFlighOrderCache.ContainsKey(uniqueId);

            return returnFlag;
        }
        public void RegisterUniqueId(string uniqueId)
        {
            lock (inFlighOrderCache)
            {
                inFlighOrderCache.Add(uniqueId, 0);
            }
        }

        public void DeregisterUniqueId(string uniqueId)
        {
            lock (inFlighOrderCache)
            {
                inFlighOrderCache.Remove(uniqueId);
            }
        }

        public void Clear()
        {
            lock (inFlighOrderCache)
            {
                inFlighOrderCache.Clear();
            }
        }
    }

    public class RealOrderService
    {
        private Dictionary<string, int> orderPartialFillCache = new Dictionary<string, int>();
        private RealInFlightOrderCache inFlightOrderCache = new RealInFlightOrderCache();
        private const string TPOrderNamePrefix = "TP-";
        private const string SLOrderNamePrefix = "SL-";
        private const string SNAPOrderNamePrefix = "SNP-";

        public RealInFlightOrderCache InFlightOrderCache
        {
            get { return inFlightOrderCache; }
        }

        private string BuildKeyName(Order order)
        {
            string keyName = order.Id.ToString();

            return keyName;
        }

        public string BuildUniqueId()
        {
            string keyName = Guid.NewGuid().ToString();

            return keyName;
        }

        public string BuildTakeProfitUniqueId()
        {
            string keyName = TPOrderNamePrefix + Guid.NewGuid().ToString();

            return keyName;
        }

        public string BuildStopLossUniqueId()
        {
            string keyName = SLOrderNamePrefix + Guid.NewGuid().ToString();

            return keyName;
        }

        public string BuildSNAPUniqueId()
        {
            string keyName = SNAPOrderNamePrefix + Guid.NewGuid().ToString();

            return keyName;
        }

        public static double GetStopLossInfo(Account account, Instrument instrument, OrderAction orderAction, out OrderType orderType, out int orderQuantity)
        {
            double stopLossPrice = 0;
            orderType = OrderType.Unknown;
            orderQuantity = 0;

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (IsValidStopLossOrder(order, instrument, orderAction))
                    {
                        stopLossPrice = order.StopPrice;
                        orderType = order.OrderType;
                        orderQuantity = order.Quantity;
                        break;
                    }
                }
            }

            return stopLossPrice;
        }

        public static double GetTakeProfitInfo(Account account, Instrument instrument, OrderAction orderAction, out OrderType orderType, out int orderQuantity)
        {
            double takeProfitPrice = 0;
            orderType = OrderType.Unknown;
            orderQuantity = 0;

            lock (account.Orders)
            {
                foreach (Order order in account.Orders)
                {
                    if (IsValidTakeProfitOrder(order, instrument, orderAction))
                    {
                        takeProfitPrice = order.LimitPrice;
                        orderType = order.OrderType;
                        orderQuantity = order.Quantity;
                        break;
                    }
                }
            }

            return takeProfitPrice;
        }
        public void SubmitLimitOrder(Account account, Order limitOrder)
        { 
            //double price = limitOrder.
            
            //account.Submit(new[] { limitOrder });
        }

        public static bool IsValidStopLossPrice(Instrument instrument, OrderAction orderAction, double price, double lastPrice)
        {
            bool returnFlag = false;

            if (orderAction == OrderAction.Buy && price > lastPrice)
            {
                returnFlag = true;
            }
            else if (orderAction == OrderAction.Sell && price < lastPrice)
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        public static bool IsValidTakeProfitPrice(Instrument instrument, OrderAction orderAction, double price, double lastPrice)
        {
            bool returnFlag = false;

            if (orderAction == OrderAction.Buy && price < lastPrice)
            {
                returnFlag = true;
            }
            else if (orderAction == OrderAction.Sell && price > lastPrice)
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        public static bool IsValidStopLossOrder(Order order, Instrument instrument, OrderAction orderAction)
        {
            bool returnFlag = false;

            if (!Order.IsTerminalState(order.OrderState) && order.OrderAction == orderAction && order.Instrument == instrument && order.IsStopMarket && order.Name.StartsWith(SLOrderNamePrefix))
            {
                returnFlag = true;
            }

            return returnFlag;
        }
        public static bool IsValidTakeProfitOrder(Order order, Instrument instrument, OrderAction orderAction)
        {
            bool returnFlag = false;

            if (!Order.IsTerminalState(order.OrderState) && order.OrderAction == orderAction && order.Instrument == instrument && order.IsLimit && order.Name.StartsWith(TPOrderNamePrefix))
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        public static bool IsValidSellSnapOrder(Order order, Instrument instrument, OrderAction orderAction)
        {
            bool returnFlag = false;

            if (!Order.IsTerminalState(order.OrderState) && order.OrderAction == orderAction && order.Instrument == instrument && order.IsStopMarket && order.Name.StartsWith(SNAPOrderNamePrefix))
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        public static bool IsValidBuySnapOrder(Order order, Instrument instrument, OrderAction orderAction)
        {
            bool returnFlag = false;

            if (!Order.IsTerminalState(order.OrderState) && order.OrderAction == orderAction && order.Instrument == instrument && order.IsStopMarket && order.Name.StartsWith(SNAPOrderNamePrefix))
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        public int GetFilledOrderQuantity(Order order)
        {
            int quantity = order.Filled;

            if (order.OrderState == OrderState.PartFilled || order.OrderState == OrderState.Filled)
            {
                lock (orderPartialFillCache)
                {
                    string keyName = BuildKeyName(order);
                    if (orderPartialFillCache.ContainsKey(keyName))
                    {
                        int currentFilledQuantity = orderPartialFillCache[keyName];
                        
                        quantity = order.Filled - currentFilledQuantity;

                        if (order.OrderState == OrderState.Filled)
                            orderPartialFillCache.Remove(keyName);
                        else
                            orderPartialFillCache[keyName] = order.Filled;
                    }
                    else
                    {
                        if (order.OrderState == OrderState.PartFilled)
                            orderPartialFillCache[keyName] = order.Filled;
                    }
                }
            }

            return quantity;
        }
    }
    public class RealTradeService
    {

    }
    public class RealPositionService
    {
        private List<RealPosition> realPositions = new List<RealPosition>();

        public int PositionsCount
        {
            get { return realPositions.Count; }
        }

        public bool IsValidPosition(RealPosition position, Instrument instrument)
        {
            bool returnFlag = false;

            if (position.Instrument.FullName == instrument.FullName && !position.IsFlat())
            {
                returnFlag = true;
            }

            return returnFlag;
        }

        public bool TryGetByIndex(int index, out RealPosition realPosition)
        {
            bool returnFlag = false;
            realPosition = null;

            try
            {
                realPosition = realPositions.ElementAt(index);
                returnFlag = true;
            }
            catch
            { 
                //stuff exception 
            }

            return returnFlag;
        }

        public bool TryGetByInstrumentFullName(string instrumentFullName, out RealPosition realPosition)
        {
            bool returnFlag = false;
            realPosition = null;

            int realPositionsCount = PositionsCount;

            try
            {
                RealPosition tempRealPosition = null;

                for (int index = 0; index < PositionsCount; index++)
                {
                    tempRealPosition = realPositions.ElementAt(index);

                    if (tempRealPosition != null)
                    {
                        if (tempRealPosition.Instrument.FullName == instrumentFullName)
                        {
                            realPosition = tempRealPosition;
                            returnFlag = true;
                            break;
                        }
                    }
                }
                    
            }
            catch
            {
                //stuff exception 
            }

            return returnFlag;
        }

        public RealPosition BuildRealPosition(Account account, Instrument instrument, MarketPosition marketPosition, int quantity, double averagePrice)
        {
            RealPosition realPosition = new RealPosition();

            realPosition.Account = account;
            realPosition.Instrument = instrument;
            realPosition.MarketPosition = marketPosition;
            realPosition.Quantity = quantity;
            realPosition.AveragePrice = averagePrice;

            return realPosition;
        }

        private int GetNewQuantity(RealPosition existingPosition, RealPosition newPosition)
        {
            int newQuantity = 0;

            if (existingPosition.MarketPosition == MarketPosition.Long)
            {
                if (newPosition.MarketPosition == MarketPosition.Long)
                    newQuantity = existingPosition.Quantity + newPosition.Quantity;
                else
                    newQuantity = existingPosition.Quantity - newPosition.Quantity;
            }
            else if (existingPosition.MarketPosition == MarketPosition.Short)
            {
                if (newPosition.MarketPosition == MarketPosition.Long)
                    newQuantity = existingPosition.Quantity - newPosition.Quantity;
                else
                    newQuantity = existingPosition.Quantity + newPosition.Quantity;
            }

            return newQuantity;
        }
        
        private MarketPosition FlipMarketPosition(MarketPosition marketPosition)
        {
            MarketPosition newMarketPosition = marketPosition;

            if (marketPosition == MarketPosition.Long)
            {
                newMarketPosition = MarketPosition.Short;
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newMarketPosition = MarketPosition.Long;
            }

            return newMarketPosition;
        }

        public void AddOrUpdatePosition(RealPosition position)
        {
            RealPosition foundPosition = null;

            if (TryGetByInstrumentFullName(position.Instrument.FullName, out foundPosition))
            {
                MarketPosition newMarketPosition;
                int newQuantity = GetNewQuantity(foundPosition, position);

                if (newQuantity < 0)
                {
                    newQuantity *= -1; // flip to positive number

                    newMarketPosition = FlipMarketPosition(foundPosition.MarketPosition);
                }
                else
                {
                    newMarketPosition = foundPosition.MarketPosition;
                }

                if (newQuantity == 0)
                {
                    lock (realPositions)
                    {
                        realPositions.Remove(foundPosition);
                    }
                }
                else
                {
                    int quanitytSum = foundPosition.Quantity + position.Quantity;
                    double newAveragePrice = ((foundPosition.AveragePrice * foundPosition.Quantity) + (position.AveragePrice * position.Quantity)) / quanitytSum;
                    //Output.Process(GetDateTimeNow() + ": " + " tempAveragePrice=" + tempAveragePrice.ToString() + " previousAP =" + foundPosition.AveragePrice.ToString() + " newAP=" + position.AveragePrice.ToString() + " previousquan=" + foundPosition.Quantity.ToString() + " origQuan=" + position.Quantity.ToString(), PrintTo.OutputTab1);

                    double tickSize = position.Instrument.MasterInstrument.TickSize;
                    int ticksPerPoint = RealInstrumentService.GetTicksPerPoint(tickSize);

                    foundPosition.AveragePrice = newAveragePrice;
                    //Output.Process(GetDateTimeNow() + ": " + " newAveragePrice=" + newAveragePrice.ToString() + " previousAP =" + foundPosition.AveragePrice.ToString() + " newAP=" + position.AveragePrice.ToString() + " previousquan=" + foundPosition.Quantity.ToString() + " newQuan=" + newQuantity.ToString(), PrintTo.OutputTab1);

                    //foreach (Position xPosition in foundPosition.Account.Positions)
                    //{
                    //    Output.Process(GetDateTimeNow() + ": " + " xPositionAP=" + xPosition.AveragePrice.ToString(), PrintTo.OutputTab1);
                    //}

                    foundPosition.Quantity = newQuantity;
                    foundPosition.MarketPosition = newMarketPosition;
                }
            }
            else
            {
                //Output.Process(GetDateTimeNow() + ": " + " newAP=" + position.AveragePrice.ToString() + " newQuan=" + position.Quantity.ToString(), PrintTo.OutputTab1);
                lock (realPositions)
                {
                    realPositions.Add(position);
                }
            }
        }

        public void LoadPositions(Account account)
        {
            lock (account.Positions)
            {
                lock (realPositions)
                {
                    realPositions.Clear();

                    foreach (Position positionItem in account.Positions)
                    {
                        RealPosition position = BuildRealPosition(account,
                            positionItem.Instrument,
                            positionItem.MarketPosition,
                            positionItem.Quantity,
                            positionItem.AveragePrice);

                        AddOrUpdatePosition(position);
                    }
                }
            }
        }
    }
    public class RealPosition
    {
        private const int StateHasChanged = 1;
        private const int StateHasNotChanged = 0;
        private int stateChangeStatus = StateHasChanged;

        private MarketPosition marketPosition = MarketPosition.Flat;
        private double averagePrice = 0;
        private Instrument instrument = null;
        private int quantity = 0;
        private Account account = null;

        public Account Account
        {
            get
            {
                return account;
            }
            set
            {
                ChangeStateFlag();
                account = value;
            }
        }

        public MarketPosition MarketPosition
        {
            get
            {
                return marketPosition;
            }
            set
            {
                ChangeStateFlag();
                marketPosition = value;
            }
        }

        public double AveragePrice
        {
            get
            {
                return averagePrice;
            }
            set
            {
                ChangeStateFlag();
                averagePrice = value;
            }
        }

        public Instrument Instrument
        {
            get
            {
                return instrument;
            }
            set
            {
                ChangeStateFlag();
                instrument = value;
            }
        }

        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                ChangeStateFlag();
                quantity = value;
            }
        }

        public bool IsFlat()
        {
             return (marketPosition == MarketPosition.Flat || quantity == 0);
        }

        public bool HasStateChanged()
        {
             return (stateChangeStatus == StateHasChanged);
        }

        public void StoreState()
        {
            ResetStateFlag();
        }

        private void ChangeStateFlag()
        {
            Interlocked.Exchange(ref stateChangeStatus, StateHasChanged);
        }

        private void ResetStateFlag()
        {
            Interlocked.Exchange(ref stateChangeStatus, StateHasNotChanged);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TickHunter[] cacheTickHunter;
		public TickHunter TickHunter(bool useAutoPositionStopLoss, bool useAutoPositionTakeProfit, TickHunterBreakEvenAutoTypes autoPositionBreakEvenType, int stopLossInitialTicks, double stopLossInitialATRMultiplier, int stopLossJumpTicks, int breakEvenInitialTicks, int breakEvenJumpTicks, int breakEvenAutoTriggerTicks, double breakEvenAutoTriggerATRMultiplier, int breakEvenAutoTrailMA1Period, int breakEvenAutoTrailMA2Period, int breakEvenAutoTrailMA3Period, int takeProfitInitialTicks, double takeProfitInitialATRMultiplier, int takeProfitJumpTicks, int snapPopContracts, bool useSnapPositionTPSL, int snapPaddingTicks, int popInitialTicks, double popInitialATRMultiplier, int popJumpTicks, int aTRPeriod, int refreshTPSLPaddingTicks, int refreshTPSLOrderDelaySeconds, int singleOrderChunkMaxQuantity, int singleOrderChunkMinQuantity, int singleOrderChunkDelayMilliseconds, bool useECATakeProfit, double eCATakeProfitDollarsPerMicroVolume, double eCATakeProfitDollarsPerEminiVolume, double eCATakeProfitATRMultiplierPerVolume, double eCAStopLossMaxDDInDollars, double eCAStopLossEquityRemainingInDollars, bool usePlayThroughSleepMode, int autobotEntryQuantity, int autobotEntryQuantityMax, bool useHedgehogEntry, bool hedgehogEntryBuySymbol1SellSymbol2, string hedgehogEntrySymbol1, string hedgehogEntrySymbol2, bool usePositionProfitLogging, int debugLogLevel, int orderWaitOutputThrottleSeconds, bool useAccountInfoLogging, string accountInfoLoggingPath)
		{
			return TickHunter(Input, useAutoPositionStopLoss, useAutoPositionTakeProfit, autoPositionBreakEvenType, stopLossInitialTicks, stopLossInitialATRMultiplier, stopLossJumpTicks, breakEvenInitialTicks, breakEvenJumpTicks, breakEvenAutoTriggerTicks, breakEvenAutoTriggerATRMultiplier, breakEvenAutoTrailMA1Period, breakEvenAutoTrailMA2Period, breakEvenAutoTrailMA3Period, takeProfitInitialTicks, takeProfitInitialATRMultiplier, takeProfitJumpTicks, snapPopContracts, useSnapPositionTPSL, snapPaddingTicks, popInitialTicks, popInitialATRMultiplier, popJumpTicks, aTRPeriod, refreshTPSLPaddingTicks, refreshTPSLOrderDelaySeconds, singleOrderChunkMaxQuantity, singleOrderChunkMinQuantity, singleOrderChunkDelayMilliseconds, useECATakeProfit, eCATakeProfitDollarsPerMicroVolume, eCATakeProfitDollarsPerEminiVolume, eCATakeProfitATRMultiplierPerVolume, eCAStopLossMaxDDInDollars, eCAStopLossEquityRemainingInDollars, usePlayThroughSleepMode, autobotEntryQuantity, autobotEntryQuantityMax, useHedgehogEntry, hedgehogEntryBuySymbol1SellSymbol2, hedgehogEntrySymbol1, hedgehogEntrySymbol2, usePositionProfitLogging, debugLogLevel, orderWaitOutputThrottleSeconds, useAccountInfoLogging, accountInfoLoggingPath);
		}

		public TickHunter TickHunter(ISeries<double> input, bool useAutoPositionStopLoss, bool useAutoPositionTakeProfit, TickHunterBreakEvenAutoTypes autoPositionBreakEvenType, int stopLossInitialTicks, double stopLossInitialATRMultiplier, int stopLossJumpTicks, int breakEvenInitialTicks, int breakEvenJumpTicks, int breakEvenAutoTriggerTicks, double breakEvenAutoTriggerATRMultiplier, int breakEvenAutoTrailMA1Period, int breakEvenAutoTrailMA2Period, int breakEvenAutoTrailMA3Period, int takeProfitInitialTicks, double takeProfitInitialATRMultiplier, int takeProfitJumpTicks, int snapPopContracts, bool useSnapPositionTPSL, int snapPaddingTicks, int popInitialTicks, double popInitialATRMultiplier, int popJumpTicks, int aTRPeriod, int refreshTPSLPaddingTicks, int refreshTPSLOrderDelaySeconds, int singleOrderChunkMaxQuantity, int singleOrderChunkMinQuantity, int singleOrderChunkDelayMilliseconds, bool useECATakeProfit, double eCATakeProfitDollarsPerMicroVolume, double eCATakeProfitDollarsPerEminiVolume, double eCATakeProfitATRMultiplierPerVolume, double eCAStopLossMaxDDInDollars, double eCAStopLossEquityRemainingInDollars, bool usePlayThroughSleepMode, int autobotEntryQuantity, int autobotEntryQuantityMax, bool useHedgehogEntry, bool hedgehogEntryBuySymbol1SellSymbol2, string hedgehogEntrySymbol1, string hedgehogEntrySymbol2, bool usePositionProfitLogging, int debugLogLevel, int orderWaitOutputThrottleSeconds, bool useAccountInfoLogging, string accountInfoLoggingPath)
		{
			if (cacheTickHunter != null)
				for (int idx = 0; idx < cacheTickHunter.Length; idx++)
					if (cacheTickHunter[idx] != null && cacheTickHunter[idx].UseAutoPositionStopLoss == useAutoPositionStopLoss && cacheTickHunter[idx].UseAutoPositionTakeProfit == useAutoPositionTakeProfit && cacheTickHunter[idx].AutoPositionBreakEvenType == autoPositionBreakEvenType && cacheTickHunter[idx].StopLossInitialTicks == stopLossInitialTicks && cacheTickHunter[idx].StopLossInitialATRMultiplier == stopLossInitialATRMultiplier && cacheTickHunter[idx].StopLossJumpTicks == stopLossJumpTicks && cacheTickHunter[idx].BreakEvenInitialTicks == breakEvenInitialTicks && cacheTickHunter[idx].BreakEvenJumpTicks == breakEvenJumpTicks && cacheTickHunter[idx].BreakEvenAutoTriggerTicks == breakEvenAutoTriggerTicks && cacheTickHunter[idx].BreakEvenAutoTriggerATRMultiplier == breakEvenAutoTriggerATRMultiplier && cacheTickHunter[idx].BreakEvenAutoTrailMA1Period == breakEvenAutoTrailMA1Period && cacheTickHunter[idx].BreakEvenAutoTrailMA2Period == breakEvenAutoTrailMA2Period && cacheTickHunter[idx].BreakEvenAutoTrailMA3Period == breakEvenAutoTrailMA3Period && cacheTickHunter[idx].TakeProfitInitialTicks == takeProfitInitialTicks && cacheTickHunter[idx].TakeProfitInitialATRMultiplier == takeProfitInitialATRMultiplier && cacheTickHunter[idx].TakeProfitJumpTicks == takeProfitJumpTicks && cacheTickHunter[idx].SnapPopContracts == snapPopContracts && cacheTickHunter[idx].UseSnapPositionTPSL == useSnapPositionTPSL && cacheTickHunter[idx].SnapPaddingTicks == snapPaddingTicks && cacheTickHunter[idx].PopInitialTicks == popInitialTicks && cacheTickHunter[idx].PopInitialATRMultiplier == popInitialATRMultiplier && cacheTickHunter[idx].PopJumpTicks == popJumpTicks && cacheTickHunter[idx].ATRPeriod == aTRPeriod && cacheTickHunter[idx].RefreshTPSLPaddingTicks == refreshTPSLPaddingTicks && cacheTickHunter[idx].RefreshTPSLOrderDelaySeconds == refreshTPSLOrderDelaySeconds && cacheTickHunter[idx].SingleOrderChunkMaxQuantity == singleOrderChunkMaxQuantity && cacheTickHunter[idx].SingleOrderChunkMinQuantity == singleOrderChunkMinQuantity && cacheTickHunter[idx].SingleOrderChunkDelayMilliseconds == singleOrderChunkDelayMilliseconds && cacheTickHunter[idx].UseECATakeProfit == useECATakeProfit && cacheTickHunter[idx].ECATakeProfitDollarsPerMicroVolume == eCATakeProfitDollarsPerMicroVolume && cacheTickHunter[idx].ECATakeProfitDollarsPerEminiVolume == eCATakeProfitDollarsPerEminiVolume && cacheTickHunter[idx].ECATakeProfitATRMultiplierPerVolume == eCATakeProfitATRMultiplierPerVolume && cacheTickHunter[idx].ECAStopLossMaxDDInDollars == eCAStopLossMaxDDInDollars && cacheTickHunter[idx].ECAStopLossEquityRemainingInDollars == eCAStopLossEquityRemainingInDollars && cacheTickHunter[idx].UsePlayThroughSleepMode == usePlayThroughSleepMode && cacheTickHunter[idx].AutobotEntryQuantity == autobotEntryQuantity && cacheTickHunter[idx].AutobotEntryQuantityMax == autobotEntryQuantityMax && cacheTickHunter[idx].UseHedgehogEntry == useHedgehogEntry && cacheTickHunter[idx].HedgehogEntryBuySymbol1SellSymbol2 == hedgehogEntryBuySymbol1SellSymbol2 && cacheTickHunter[idx].HedgehogEntrySymbol1 == hedgehogEntrySymbol1 && cacheTickHunter[idx].HedgehogEntrySymbol2 == hedgehogEntrySymbol2 && cacheTickHunter[idx].UsePositionProfitLogging == usePositionProfitLogging && cacheTickHunter[idx].DebugLogLevel == debugLogLevel && cacheTickHunter[idx].OrderWaitOutputThrottleSeconds == orderWaitOutputThrottleSeconds && cacheTickHunter[idx].UseAccountInfoLogging == useAccountInfoLogging && cacheTickHunter[idx].AccountInfoLoggingPath == accountInfoLoggingPath && cacheTickHunter[idx].EqualsInput(input))
						return cacheTickHunter[idx];
			return CacheIndicator<TickHunter>(new TickHunter(){ UseAutoPositionStopLoss = useAutoPositionStopLoss, UseAutoPositionTakeProfit = useAutoPositionTakeProfit, AutoPositionBreakEvenType = autoPositionBreakEvenType, StopLossInitialTicks = stopLossInitialTicks, StopLossInitialATRMultiplier = stopLossInitialATRMultiplier, StopLossJumpTicks = stopLossJumpTicks, BreakEvenInitialTicks = breakEvenInitialTicks, BreakEvenJumpTicks = breakEvenJumpTicks, BreakEvenAutoTriggerTicks = breakEvenAutoTriggerTicks, BreakEvenAutoTriggerATRMultiplier = breakEvenAutoTriggerATRMultiplier, BreakEvenAutoTrailMA1Period = breakEvenAutoTrailMA1Period, BreakEvenAutoTrailMA2Period = breakEvenAutoTrailMA2Period, BreakEvenAutoTrailMA3Period = breakEvenAutoTrailMA3Period, TakeProfitInitialTicks = takeProfitInitialTicks, TakeProfitInitialATRMultiplier = takeProfitInitialATRMultiplier, TakeProfitJumpTicks = takeProfitJumpTicks, SnapPopContracts = snapPopContracts, UseSnapPositionTPSL = useSnapPositionTPSL, SnapPaddingTicks = snapPaddingTicks, PopInitialTicks = popInitialTicks, PopInitialATRMultiplier = popInitialATRMultiplier, PopJumpTicks = popJumpTicks, ATRPeriod = aTRPeriod, RefreshTPSLPaddingTicks = refreshTPSLPaddingTicks, RefreshTPSLOrderDelaySeconds = refreshTPSLOrderDelaySeconds, SingleOrderChunkMaxQuantity = singleOrderChunkMaxQuantity, SingleOrderChunkMinQuantity = singleOrderChunkMinQuantity, SingleOrderChunkDelayMilliseconds = singleOrderChunkDelayMilliseconds, UseECATakeProfit = useECATakeProfit, ECATakeProfitDollarsPerMicroVolume = eCATakeProfitDollarsPerMicroVolume, ECATakeProfitDollarsPerEminiVolume = eCATakeProfitDollarsPerEminiVolume, ECATakeProfitATRMultiplierPerVolume = eCATakeProfitATRMultiplierPerVolume, ECAStopLossMaxDDInDollars = eCAStopLossMaxDDInDollars, ECAStopLossEquityRemainingInDollars = eCAStopLossEquityRemainingInDollars, UsePlayThroughSleepMode = usePlayThroughSleepMode, AutobotEntryQuantity = autobotEntryQuantity, AutobotEntryQuantityMax = autobotEntryQuantityMax, UseHedgehogEntry = useHedgehogEntry, HedgehogEntryBuySymbol1SellSymbol2 = hedgehogEntryBuySymbol1SellSymbol2, HedgehogEntrySymbol1 = hedgehogEntrySymbol1, HedgehogEntrySymbol2 = hedgehogEntrySymbol2, UsePositionProfitLogging = usePositionProfitLogging, DebugLogLevel = debugLogLevel, OrderWaitOutputThrottleSeconds = orderWaitOutputThrottleSeconds, UseAccountInfoLogging = useAccountInfoLogging, AccountInfoLoggingPath = accountInfoLoggingPath }, input, ref cacheTickHunter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TickHunter TickHunter(bool useAutoPositionStopLoss, bool useAutoPositionTakeProfit, TickHunterBreakEvenAutoTypes autoPositionBreakEvenType, int stopLossInitialTicks, double stopLossInitialATRMultiplier, int stopLossJumpTicks, int breakEvenInitialTicks, int breakEvenJumpTicks, int breakEvenAutoTriggerTicks, double breakEvenAutoTriggerATRMultiplier, int breakEvenAutoTrailMA1Period, int breakEvenAutoTrailMA2Period, int breakEvenAutoTrailMA3Period, int takeProfitInitialTicks, double takeProfitInitialATRMultiplier, int takeProfitJumpTicks, int snapPopContracts, bool useSnapPositionTPSL, int snapPaddingTicks, int popInitialTicks, double popInitialATRMultiplier, int popJumpTicks, int aTRPeriod, int refreshTPSLPaddingTicks, int refreshTPSLOrderDelaySeconds, int singleOrderChunkMaxQuantity, int singleOrderChunkMinQuantity, int singleOrderChunkDelayMilliseconds, bool useECATakeProfit, double eCATakeProfitDollarsPerMicroVolume, double eCATakeProfitDollarsPerEminiVolume, double eCATakeProfitATRMultiplierPerVolume, double eCAStopLossMaxDDInDollars, double eCAStopLossEquityRemainingInDollars, bool usePlayThroughSleepMode, int autobotEntryQuantity, int autobotEntryQuantityMax, bool useHedgehogEntry, bool hedgehogEntryBuySymbol1SellSymbol2, string hedgehogEntrySymbol1, string hedgehogEntrySymbol2, bool usePositionProfitLogging, int debugLogLevel, int orderWaitOutputThrottleSeconds, bool useAccountInfoLogging, string accountInfoLoggingPath)
		{
			return indicator.TickHunter(Input, useAutoPositionStopLoss, useAutoPositionTakeProfit, autoPositionBreakEvenType, stopLossInitialTicks, stopLossInitialATRMultiplier, stopLossJumpTicks, breakEvenInitialTicks, breakEvenJumpTicks, breakEvenAutoTriggerTicks, breakEvenAutoTriggerATRMultiplier, breakEvenAutoTrailMA1Period, breakEvenAutoTrailMA2Period, breakEvenAutoTrailMA3Period, takeProfitInitialTicks, takeProfitInitialATRMultiplier, takeProfitJumpTicks, snapPopContracts, useSnapPositionTPSL, snapPaddingTicks, popInitialTicks, popInitialATRMultiplier, popJumpTicks, aTRPeriod, refreshTPSLPaddingTicks, refreshTPSLOrderDelaySeconds, singleOrderChunkMaxQuantity, singleOrderChunkMinQuantity, singleOrderChunkDelayMilliseconds, useECATakeProfit, eCATakeProfitDollarsPerMicroVolume, eCATakeProfitDollarsPerEminiVolume, eCATakeProfitATRMultiplierPerVolume, eCAStopLossMaxDDInDollars, eCAStopLossEquityRemainingInDollars, usePlayThroughSleepMode, autobotEntryQuantity, autobotEntryQuantityMax, useHedgehogEntry, hedgehogEntryBuySymbol1SellSymbol2, hedgehogEntrySymbol1, hedgehogEntrySymbol2, usePositionProfitLogging, debugLogLevel, orderWaitOutputThrottleSeconds, useAccountInfoLogging, accountInfoLoggingPath);
		}

		public Indicators.TickHunter TickHunter(ISeries<double> input , bool useAutoPositionStopLoss, bool useAutoPositionTakeProfit, TickHunterBreakEvenAutoTypes autoPositionBreakEvenType, int stopLossInitialTicks, double stopLossInitialATRMultiplier, int stopLossJumpTicks, int breakEvenInitialTicks, int breakEvenJumpTicks, int breakEvenAutoTriggerTicks, double breakEvenAutoTriggerATRMultiplier, int breakEvenAutoTrailMA1Period, int breakEvenAutoTrailMA2Period, int breakEvenAutoTrailMA3Period, int takeProfitInitialTicks, double takeProfitInitialATRMultiplier, int takeProfitJumpTicks, int snapPopContracts, bool useSnapPositionTPSL, int snapPaddingTicks, int popInitialTicks, double popInitialATRMultiplier, int popJumpTicks, int aTRPeriod, int refreshTPSLPaddingTicks, int refreshTPSLOrderDelaySeconds, int singleOrderChunkMaxQuantity, int singleOrderChunkMinQuantity, int singleOrderChunkDelayMilliseconds, bool useECATakeProfit, double eCATakeProfitDollarsPerMicroVolume, double eCATakeProfitDollarsPerEminiVolume, double eCATakeProfitATRMultiplierPerVolume, double eCAStopLossMaxDDInDollars, double eCAStopLossEquityRemainingInDollars, bool usePlayThroughSleepMode, int autobotEntryQuantity, int autobotEntryQuantityMax, bool useHedgehogEntry, bool hedgehogEntryBuySymbol1SellSymbol2, string hedgehogEntrySymbol1, string hedgehogEntrySymbol2, bool usePositionProfitLogging, int debugLogLevel, int orderWaitOutputThrottleSeconds, bool useAccountInfoLogging, string accountInfoLoggingPath)
		{
			return indicator.TickHunter(input, useAutoPositionStopLoss, useAutoPositionTakeProfit, autoPositionBreakEvenType, stopLossInitialTicks, stopLossInitialATRMultiplier, stopLossJumpTicks, breakEvenInitialTicks, breakEvenJumpTicks, breakEvenAutoTriggerTicks, breakEvenAutoTriggerATRMultiplier, breakEvenAutoTrailMA1Period, breakEvenAutoTrailMA2Period, breakEvenAutoTrailMA3Period, takeProfitInitialTicks, takeProfitInitialATRMultiplier, takeProfitJumpTicks, snapPopContracts, useSnapPositionTPSL, snapPaddingTicks, popInitialTicks, popInitialATRMultiplier, popJumpTicks, aTRPeriod, refreshTPSLPaddingTicks, refreshTPSLOrderDelaySeconds, singleOrderChunkMaxQuantity, singleOrderChunkMinQuantity, singleOrderChunkDelayMilliseconds, useECATakeProfit, eCATakeProfitDollarsPerMicroVolume, eCATakeProfitDollarsPerEminiVolume, eCATakeProfitATRMultiplierPerVolume, eCAStopLossMaxDDInDollars, eCAStopLossEquityRemainingInDollars, usePlayThroughSleepMode, autobotEntryQuantity, autobotEntryQuantityMax, useHedgehogEntry, hedgehogEntryBuySymbol1SellSymbol2, hedgehogEntrySymbol1, hedgehogEntrySymbol2, usePositionProfitLogging, debugLogLevel, orderWaitOutputThrottleSeconds, useAccountInfoLogging, accountInfoLoggingPath);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TickHunter TickHunter(bool useAutoPositionStopLoss, bool useAutoPositionTakeProfit, TickHunterBreakEvenAutoTypes autoPositionBreakEvenType, int stopLossInitialTicks, double stopLossInitialATRMultiplier, int stopLossJumpTicks, int breakEvenInitialTicks, int breakEvenJumpTicks, int breakEvenAutoTriggerTicks, double breakEvenAutoTriggerATRMultiplier, int breakEvenAutoTrailMA1Period, int breakEvenAutoTrailMA2Period, int breakEvenAutoTrailMA3Period, int takeProfitInitialTicks, double takeProfitInitialATRMultiplier, int takeProfitJumpTicks, int snapPopContracts, bool useSnapPositionTPSL, int snapPaddingTicks, int popInitialTicks, double popInitialATRMultiplier, int popJumpTicks, int aTRPeriod, int refreshTPSLPaddingTicks, int refreshTPSLOrderDelaySeconds, int singleOrderChunkMaxQuantity, int singleOrderChunkMinQuantity, int singleOrderChunkDelayMilliseconds, bool useECATakeProfit, double eCATakeProfitDollarsPerMicroVolume, double eCATakeProfitDollarsPerEminiVolume, double eCATakeProfitATRMultiplierPerVolume, double eCAStopLossMaxDDInDollars, double eCAStopLossEquityRemainingInDollars, bool usePlayThroughSleepMode, int autobotEntryQuantity, int autobotEntryQuantityMax, bool useHedgehogEntry, bool hedgehogEntryBuySymbol1SellSymbol2, string hedgehogEntrySymbol1, string hedgehogEntrySymbol2, bool usePositionProfitLogging, int debugLogLevel, int orderWaitOutputThrottleSeconds, bool useAccountInfoLogging, string accountInfoLoggingPath)
		{
			return indicator.TickHunter(Input, useAutoPositionStopLoss, useAutoPositionTakeProfit, autoPositionBreakEvenType, stopLossInitialTicks, stopLossInitialATRMultiplier, stopLossJumpTicks, breakEvenInitialTicks, breakEvenJumpTicks, breakEvenAutoTriggerTicks, breakEvenAutoTriggerATRMultiplier, breakEvenAutoTrailMA1Period, breakEvenAutoTrailMA2Period, breakEvenAutoTrailMA3Period, takeProfitInitialTicks, takeProfitInitialATRMultiplier, takeProfitJumpTicks, snapPopContracts, useSnapPositionTPSL, snapPaddingTicks, popInitialTicks, popInitialATRMultiplier, popJumpTicks, aTRPeriod, refreshTPSLPaddingTicks, refreshTPSLOrderDelaySeconds, singleOrderChunkMaxQuantity, singleOrderChunkMinQuantity, singleOrderChunkDelayMilliseconds, useECATakeProfit, eCATakeProfitDollarsPerMicroVolume, eCATakeProfitDollarsPerEminiVolume, eCATakeProfitATRMultiplierPerVolume, eCAStopLossMaxDDInDollars, eCAStopLossEquityRemainingInDollars, usePlayThroughSleepMode, autobotEntryQuantity, autobotEntryQuantityMax, useHedgehogEntry, hedgehogEntryBuySymbol1SellSymbol2, hedgehogEntrySymbol1, hedgehogEntrySymbol2, usePositionProfitLogging, debugLogLevel, orderWaitOutputThrottleSeconds, useAccountInfoLogging, accountInfoLoggingPath);
		}

		public Indicators.TickHunter TickHunter(ISeries<double> input , bool useAutoPositionStopLoss, bool useAutoPositionTakeProfit, TickHunterBreakEvenAutoTypes autoPositionBreakEvenType, int stopLossInitialTicks, double stopLossInitialATRMultiplier, int stopLossJumpTicks, int breakEvenInitialTicks, int breakEvenJumpTicks, int breakEvenAutoTriggerTicks, double breakEvenAutoTriggerATRMultiplier, int breakEvenAutoTrailMA1Period, int breakEvenAutoTrailMA2Period, int breakEvenAutoTrailMA3Period, int takeProfitInitialTicks, double takeProfitInitialATRMultiplier, int takeProfitJumpTicks, int snapPopContracts, bool useSnapPositionTPSL, int snapPaddingTicks, int popInitialTicks, double popInitialATRMultiplier, int popJumpTicks, int aTRPeriod, int refreshTPSLPaddingTicks, int refreshTPSLOrderDelaySeconds, int singleOrderChunkMaxQuantity, int singleOrderChunkMinQuantity, int singleOrderChunkDelayMilliseconds, bool useECATakeProfit, double eCATakeProfitDollarsPerMicroVolume, double eCATakeProfitDollarsPerEminiVolume, double eCATakeProfitATRMultiplierPerVolume, double eCAStopLossMaxDDInDollars, double eCAStopLossEquityRemainingInDollars, bool usePlayThroughSleepMode, int autobotEntryQuantity, int autobotEntryQuantityMax, bool useHedgehogEntry, bool hedgehogEntryBuySymbol1SellSymbol2, string hedgehogEntrySymbol1, string hedgehogEntrySymbol2, bool usePositionProfitLogging, int debugLogLevel, int orderWaitOutputThrottleSeconds, bool useAccountInfoLogging, string accountInfoLoggingPath)
		{
			return indicator.TickHunter(input, useAutoPositionStopLoss, useAutoPositionTakeProfit, autoPositionBreakEvenType, stopLossInitialTicks, stopLossInitialATRMultiplier, stopLossJumpTicks, breakEvenInitialTicks, breakEvenJumpTicks, breakEvenAutoTriggerTicks, breakEvenAutoTriggerATRMultiplier, breakEvenAutoTrailMA1Period, breakEvenAutoTrailMA2Period, breakEvenAutoTrailMA3Period, takeProfitInitialTicks, takeProfitInitialATRMultiplier, takeProfitJumpTicks, snapPopContracts, useSnapPositionTPSL, snapPaddingTicks, popInitialTicks, popInitialATRMultiplier, popJumpTicks, aTRPeriod, refreshTPSLPaddingTicks, refreshTPSLOrderDelaySeconds, singleOrderChunkMaxQuantity, singleOrderChunkMinQuantity, singleOrderChunkDelayMilliseconds, useECATakeProfit, eCATakeProfitDollarsPerMicroVolume, eCATakeProfitDollarsPerEminiVolume, eCATakeProfitATRMultiplierPerVolume, eCAStopLossMaxDDInDollars, eCAStopLossEquityRemainingInDollars, usePlayThroughSleepMode, autobotEntryQuantity, autobotEntryQuantityMax, useHedgehogEntry, hedgehogEntryBuySymbol1SellSymbol2, hedgehogEntrySymbol1, hedgehogEntrySymbol2, usePositionProfitLogging, debugLogLevel, orderWaitOutputThrottleSeconds, useAccountInfoLogging, accountInfoLoggingPath);
		}
	}
}

#endregion
