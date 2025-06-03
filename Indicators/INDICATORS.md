# NinjaTrader Indicators Reference

This document provides a categorized reference of the indicators available in this collection.

## Table of Contents

- [Moving Averages](#moving-averages)
- [Oscillators](#oscillators)
- [Volatility Indicators](#volatility-indicators)
- [Volume Indicators](#volume-indicators)
- [Trend Indicators](#trend-indicators)
- [Price Action Patterns](#price-action-patterns)
- [Support/Resistance Tools](#supportresistance-tools)
- [Trade Management](#trade-management)
- [ARC Trading Suite](#arc-trading-suite)
- [Utilities](#utilities)

## Moving Averages

Moving averages smooth price data to identify trends and potential support/resistance levels.

| Indicator | Description | File |
|-----------|-------------|------|
| SMA | Simple Moving Average | @SMA.cs |
| EMA | Exponential Moving Average | @EMA.cs |
| DEMA | Double Exponential Moving Average | @DEMA.cs |
| TEMA | Triple Exponential Moving Average | @TEMA.cs |
| HMA | Hull Moving Average | @HMA.cs |
| KAMA | Kaufman's Adaptive Moving Average | @KAMA.cs |
| MAMA | MESA Adaptive Moving Average | @MAMA.cs |
| VWMA | Volume Weighted Moving Average | @VWMA.cs |
| WMA | Weighted Moving Average | @WMA.cs |
| ZLEMA | Zero Lag Exponential Moving Average | @ZLEMA.cs |
| TMA | Triangular Moving Average | @TMA.cs |
| VMA | Variable Moving Average | @VMA.cs |
| IW_SMMA | Smoothed Moving Average | IW_SMMA.cs |
| IW_ZeroLagHATEMA | Zero Lag Hull Adaptive Triple EMA | IW_ZeroLagHATEMA.cs |
| IW_ZeroLagTEMA | Zero Lag Triple EMA | IW_ZeroLagTEMA.cs |
| KimM4mas | Kim's Moving Averages | KimM4mas.cs |
| MovingAverageRibbon | Multiple MAs displayed as a ribbon | MovingAverageRibbon.cs |

## Oscillators

Oscillators help identify overbought/oversold conditions and potential reversals.

| Indicator | Description | File |
|-----------|-------------|------|
| RSI | Relative Strength Index | @RSI.cs |
| CCI | Commodity Channel Index | @CCI.cs |
| Stochastics | Stochastic Oscillator | @Stochastics.cs |
| StochasticsFast | Fast Stochastic Oscillator | @StochasticsFast.cs |
| StochRSI | Stochastic RSI | @StochRSI.cs |
| MACD | Moving Average Convergence Divergence | @MACD.cs |
| PPO | Percentage Price Oscillator | @PPO.cs |
| ROC | Rate of Change | @ROC.cs |
| VROC | Volume Rate of Change | @VROC.cs |
| MFI | Money Flow Index | @MFI.cs |
| CMO | Chande Momentum Oscillator | @CMO.cs |
| WilliamsR | Williams %R | @WilliamsR.cs |
| UltimateOscillator | Ultimate Oscillator | @UltimateOscillator.cs |
| RVI | Relative Vigor Index | @RVI.cs |
| TSI | True Strength Index | @TSI.cs |
| TRIX | Triple Exponential Average | @TRIX.cs |
| FOSC | Forecast Oscillator | @FOSC.cs |
| QQESignals | Quantitative Qualitative Estimation | QQESignals.cs |
| AntoQQE | Anton's QQE Variant | AntoQQE.cs |
| AntoSSLQQEHybrid | Anton's SSL QQE Hybrid | AntoSSLQQEHybrid.cs |

## Volatility Indicators

Volatility indicators measure the rate and magnitude of price changes.

| Indicator | Description | File |
|-----------|-------------|------|
| ATR | Average True Range | @ATR.cs |
| ARC_ATR | ARC Average True Range | ARC/ARC_ATR.cs |
| Bollinger | Bollinger Bands | @Bollinger.cs |
| KeltnerChannel | Keltner Channel | @KeltnerChannel.cs |
| ChaikinVolatility | Chaikin Volatility | @ChaikinVolatility.cs |
| StdDev | Standard Deviation | @StdDev.cs |
| ChoppinessIndex | Choppiness Index | @ChoppinessIndex.cs |
| ATRdollars | ATR in dollar terms | ATRdollars.cs |
| ATREmanations | ATR Emanations | ATREmanations.cs |
| ATRForecaster | ATR Forecaster | ATRForecaster.cs |
| BetterEnvelope | Enhanced Price Envelopes | BetterEnvelope.cs |
| DonchianChannel | Donchian Channel | @DonchianChannel.cs |
| MAEnvelopes | Moving Average Envelopes | @MAEnvelopes.cs |
| PercentBands | Percentage Bands | PercentBands.cs |

## Volume Indicators

Volume indicators analyze trading volume to confirm price movements and identify potential reversals.

| Indicator | Description | File |
|-----------|-------------|------|
| OBV | On Balance Volume | @OBV.cs |
| VOL | Volume | @VOL.cs |
| VOLMA | Volume Moving Average | @VOLMA.cs |
| ChaikinMoneyFlow | Chaikin Money Flow | @ChaikinMoneyFlow.cs |
| ChaikinOscillator | Chaikin Oscillator | @ChaikinOscillator.cs |
| VolumeOscillator | Volume Oscillator | @VolumeOscillator.cs |
| VolumeProfile | Volume Profile | @VolumeProfile.cs |
| BuySellVolume | Buy/Sell Volume | @BuySellVolume.cs |
| BuySellPressure | Buy/Sell Pressure | @BuySellPressure.cs |
| VolumeUpDown | Volume Up/Down | @VolumeUpDown.cs |
| VolumeZones | Volume Zones | @VolumeZones.cs |
| BlockVolume | Block Volume | @BlockVolume.cs |
| VolumeCounter | Volume Counter | @VolumeCounter.cs |
| FootprintChart | Footprint Chart | FootprintChart.cs |
| FootPrintV2 | Footprint Chart V2 | FootPrintV2.cs |

## Trend Indicators

Trend indicators help identify the direction and strength of market trends.

| Indicator | Description | File |
|-----------|-------------|------|
| ADX | Average Directional Index | @ADX.cs |
| ADXR | Average Directional Movement Index Rating | @ADXR.cs |
| DMI | Directional Movement Index | @DMI.cs |
| DM | Directional Movement | @DM.cs |
| DMIndex | Directional Movement Index | @DMIndex.cs |
| Aroon | Aroon | @Aroon.cs |
| AroonOscillator | Aroon Oscillator | @AroonOscillator.cs |
| TrendLines | Trend Lines | @TrendLines.cs |
| IchimokuCloud | Ichimoku Cloud | IchimokuCloud.cs |
| TrendAge | Trend Age | TrendAge.cs |
| SwingTrend | Swing Trend | SwingTrend.cs |
| FractalSwing | Fractal Swing | FractalSwing.cs |

## Price Action Patterns

Price action patterns identify specific chart formations that may indicate future price movements.

| Indicator | Description | File |
|-----------|-------------|------|
| CandleStickPattern | Candlestick Pattern Recognition | @CandleStickPattern.cs |
| KeyReversalUp | Key Reversal Up Pattern | @KeyReversalUp.cs |
| KeyReversalDown | Key Reversal Down Pattern | @KeyReversalDown.cs |
| PivotReversal | Pivot Reversal | PivotReversal.cs |
| ReversalPattern | Reversal Pattern | ReversalPattern.cs |
| ThreeBarReversal | Three Bar Reversal | ThreeBarReversal.cs |
| NBarsUp | N Bars Up | @NBarsUp.cs |
| NBarsDown | N Bars Down | @NBarsDown.cs |
| Swing | Swing High/Low | @Swing.cs |
| ZigZag | ZigZag | @ZigZag.cs |
| BarPatterns | Bar Patterns | BarPatterns.cs |
| BiggestBar | Biggest Bar | BiggestBar.cs |
| BiggestBarOfDay | Biggest Bar Of Day | BiggestBarOfDay.cs |

## Support/Resistance Tools

Support and resistance tools identify potential price levels where the market may reverse or pause.

| Indicator | Description | File |
|-----------|-------------|------|
| Pivots | Pivot Points | @Pivots.cs |
| FibonacciPivots | Fibonacci Pivot Points | @FibonacciPivots.cs |
| CamarillaPivots | Camarilla Pivot Points | @CamarillaPivots.cs |
| LettoPivots | Letto Pivot Points | LettoPivots.cs |
| CurrentDayOHL | Current Day Open/High/Low | @CurrentDayOHL.cs |
| PriorDayOHLC | Prior Day Open/High/Low/Close | @PriorDayOHLC.cs |
| RoundNumbers | Round Numbers | RoundNumbers.cs |
| Darvas | Darvas Box | @Darvas.cs |
| RegressionChannel | Regression Channel | @RegressionChannel.cs |
| LettoChannels | Letto Channels | LettoChannels.cs |
| ConstantLines | Constant Lines | @ConstantLines.cs |
| PriceLine | Price Line | @PriceLine.cs |

## Trade Management

Trade management tools help with entry, exit, and position management.

| Indicator | Description | File |
|-----------|-------------|------|
| ATRTrailingStopSystem | ATR Trailing Stop System | ATRTrailingStopSystem.cs |
| EntryManager | Entry Manager | EntryManager.cs |
| SBGTradeManager | SBG Trade Manager | SBGTradeManager.cs |
| SwingAlert | Swing Alert | SwingAlert.cs |
| AlwaysIn | Always In Position | AlwaysIn.cs |
| BuyOneSellAnother | Buy One Sell Another | BuyOneSellAnother.cs |
| AutoSummarizePnL | Auto Summarize P&L | AutoSummarizePnL.cs |
| ListTrades | List Trades | ListTrades.cs |

## ARC Trading Suite

The ARC Trading Suite is a comprehensive collection of advanced indicators and algorithms.

| Category | Description | Example Files |
|----------|-------------|--------------|
| Core Indicators | Basic building blocks | ARC_ATR.cs, ARC_VMDivergences.cs |
| Pattern Recognition | Identify chart patterns | ARC_PatternFinder.cs, ARC_MWPatternFinderZigZag.cs |
| Algorithmic Trading | Trading algorithms | ARC_APSAlgo_*.cs, ARC_ATRVolTraderAlgo_*.cs |
| Volume Analysis | Volume-based indicators | ARC_AnchoredVolume.cs, ARC_VMLean.cs |
| Market Structure | Market structure analysis | ARC_StructureBoss.cs, ARC_SwingStructure.cs |
| Trade Management | Trade execution and management | ARC_TrailingStopsAndTargets.cs |
| Support/Resistance | Support/resistance levels | ARC_PivotPointBoss.cs, ARC_BigRoundNumbers.cs |

## Utilities

Utilities provide supporting functionality for analysis and development.

| Utility | Description | File |
|---------|-------------|------|
| ReadHTML | Fetch data from web sources | Utilities/ReadHTML.cs |
| PrintMessagesToChart | Display messages on chart | Utilities/PrintMessagesToChart.cs |
| RenumberLines | Renumber drawing lines | Utilities/RenumberLines.cs |
| Convert7to8 | Convert NT7 to NT8 scripts | Utilities/Convert7to8.cs |
| PlotHelper | Helper for plotting | PlotHelper.cs |
| HTMLParser | Parse HTML content | @@HTMLParser.cs |
| BarTimer | Timer for bars | @BarTimer.cs |
| NetChangeDisplay | Display net change | @NetChangeDisplay.cs |
| TimeLeftInSession | Show time left in session | TimeLeftInSession.cs |
| ShowDateAtBottomOfChart | Show date at bottom of chart | ShowDateAtBottomOfChart.cs |