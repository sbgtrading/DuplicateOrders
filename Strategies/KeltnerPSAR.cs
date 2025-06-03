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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class KeltnerPSAR : Strategy
	{
	    private SMA sma;
	    private EMA ema;
	    private ATR atr;
	    private ParabolicSAR psar;
	    private double dir0;
	    private double upper;
	    private double lower;
	    private bool buySignal;
	    private bool sellSignal;
		private Order o;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "KeltnerPSAR";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;

	            qtyPct = 0.01;
	            targetMultiplier = 1.0;
    	        targetAtrPeriod = 14;
        	    channelPeriod = 60;
            	channelMultiplier = 3.0;
	            channelAtrPeriod = 60;
    	        //useExponentialMA = true;
        	    psarStart = 0.0025;
            	psarAccel = 0.015;
	            psarMax = 0.2;
            	IsOverlay = true;
	            AddPlot(new Stroke(Brushes.Orange), PlotStyle.Line, "Upper");
    	        AddPlot(new Stroke(Brushes.Orange), PlotStyle.Line, "Lower");
        	    AddPlot(new Stroke(Brushes.Yellow), PlotStyle.Line, "PSAR");
	        }
    	    else if (State == State.DataLoaded)
        	{
	            sma = SMA(channelPeriod);
    	        ema = EMA(channelPeriod);
        	    atr = ATR(targetAtrPeriod);
            	psar = ParabolicSAR(psarStart, psarAccel, psarMax);
	        }
		}
	    protected override void OnBarUpdate()
	    {
	        if (CurrentBar < channelPeriod)
	            return;
	
	        dir0 = (High[0] > upper ? 1 : (Low[0] < lower ? -1 : dir0));
	
	        double channelRange = atr[0] * channelMultiplier;
	        upper = ema[0] + channelRange;
	        lower = ema[0] - channelRange;
	
	        // Logic for setting the buy and sell signals based on PSAR and conditions
	        sellSignal = dir0 < 0 && psar[1] < Low[1] && psar[0] > High[0];
	        buySignal = dir0 > 0 && psar[1] > High[1] && psar[0] < Low[0];

			if(Position.MarketPosition == MarketPosition.Flat){
				this.SetProfitTarget(CalculationMode.Ticks, Math.Round(atr[0]*100,0)/TickSize);
			}
//			if(o!=null){
//				if(o.FromEntrySignal == "PsarLE" && targetMultiplier>0){
//					this.SetProfitTarget("PsarLE", CalculationMode.Ticks, Instrument.MasterInstrument.RoundToTickSize(atr[0] * targetMultiplier)/TickSize);
//				}
//				if(o.FromEntrySignal == "PsarSE" && targetMultiplier>0){
//					this.SetProfitTarget("PsarSE", CalculationMode.Ticks, Instrument.MasterInstrument.RoundToTickSize(atr[0] * targetMultiplier)/TickSize);
//				}
//			}
			if (buySignal)
	        {
				if(Position!=null && Position.MarketPosition==MarketPosition.Short){
					ExitShort("ParSE");
//					this.SetProfitTarget(CalculationMode.Ticks, Math.Round(atr[0]*100,0)/TickSize);
				}
				var q = Math.Max(1,Convert.ToInt32(qtyPct * Account.MinimumCashValue / (3 * atr[0])));
	            EnterLong(q, "ParLE");
				ExitLongLimit(Close[0] + Instrument.MasterInstrument.RoundToTickSize(atr[0] * targetMultiplier), "ParLE");
				Print(Times[0][0].ToString()+"  Going long: "+q);
	        }
	
	        if (sellSignal)
	        {
				if(Position!=null && Position.MarketPosition==MarketPosition.Long){
					ExitLong("ParLE");
//					this.SetProfitTarget(CalculationMode.Ticks, Math.Round(atr[0]*100,0)/TickSize);
				}
				var q = Math.Max(1,Convert.ToInt32(qtyPct * Account.MinimumCashValue / (3 * atr[0])));
	            EnterShort(Convert.ToInt32(qtyPct * Account.MinimumCashValue / (3 * atr[0])), "ParSE");
				ExitShortLimit(Close[0] - Instrument.MasterInstrument.RoundToTickSize(atr[0] * targetMultiplier), "ParSE");
				Print(Times[0][0].ToString()+"  Going short: "+q);
	        }
	
	    }
		#region Parameters

		[Display(Name = "Channel Multiplier", GroupName = "NinjaScriptStrategyParameters", Order = 10)]
		public double channelMultiplier { get; set; }
		[Display(Name = "Qty Pct", GroupName = "NinjaScriptStrategyParameters", Order = 20)]
		public double qtyPct { get; set; }
		[Display(Name = "Target Multiplier", GroupName = "NinjaScriptStrategyParameters", Order = 30)]
		public double targetMultiplier { get; set; }
		[Display(Name = "Target Atr Period", GroupName = "NinjaScriptStrategyParameters", Order = 40)]
		public int targetAtrPeriod{ get; set; }
		[Display(Name = "Channel Period", GroupName = "NinjaScriptStrategyParameters", Order = 50)]
		public int channelPeriod{ get; set; }
		[Display(Name = "Channel Atr Period", GroupName = "NinjaScriptStrategyParameters", Order = 60)]
		public int channelAtrPeriod{ get; set; }
		[Display(Name = "Psar Start", GroupName = "NinjaScriptStrategyParameters", Order = 70)]
		public double psarStart{ get; set; }
		[Display(Name = "Psar Accel", GroupName = "NinjaScriptStrategyParameters", Order = 80)]
		public double psarAccel{ get; set; }
		[Display(Name = "Psar Max", GroupName = "NinjaScriptStrategyParameters", Order = 90)]
		public double psarMax{ get; set; }

		#endregion
	}
/*
//@version=5
strategy("KelterPSAR", overlay=false, pyramiding = 1)

qty = input.float(0.01, "Qty Pct", minval=-100, maxval=100, tooltip="Pct of your running equity to risk on each trade, initial risk distance is fixed at 3xATR(14)")
s1 = input.session(title="Session1", defval="0930-1100", options=["24x7", "0700-1100", "0930-1100", "1300-1600", "1600-2100"])
s2 = input.session(title="Session2", defval="1300-1600", options=["24x7", "0700-1100", "0930-1100", "1300-1600", "1600-2100"])
tgtmult = input.float(1.0, title="Tgt mult", minval=0, tooltip="Tgt distance is a multiple of ATR(Tgt Period), set to '0' to turn-off targets completely")
tgtatrperiod = input.int(14, title="Tgt Period", minval=1, tooltip="ATR period used for setting target")
channelperiod = input.int(60, minval=1, tooltip="Channel period")
channelmult = input.float(3.0, "Channel Mult")
channelatrperiod = input.int(60, "Channel ATR period")
src = input(close, title="Source")
exp = input(true, "Use Exponential MA")
enum bands_style
    atr = "Average True Range"
    tr = "True Range"
    r = "Range"
BandsStyle = input.enum(bands_style.atr, title="Bands Style")
psar_Start = input.float(0.0025, title="psar Start")
psar_Accel = input.float(0.015, title="psar Accel")
psar_Max = input.float(0.2, title="psar Max")
int dir0 = 0
int time_found = -1
float size = 0
BarInSession(sess) => time(timeframe.period, sess) != 0
esma(source, length)=>
	s1 = ta.sma(source, length)
	e1 = ta.ema(source, length)
	exp ? e1 : s1
ma = esma(src, channelperiod)
rangema = BandsStyle == bands_style.tr ? ta.rma(ta.tr(true), channelperiod) : BandsStyle == bands_style.atr ? ta.atr(channelatrperiod) : ta.rma(high - low, channelperiod)
upper = ma + rangema * channelmult
lower = ma - rangema * channelmult
dir0 := (high[0]>upper[0] ? 1 : (low[0]<lower[0]  ? -1 : dir0[1]))
u = plot(upper, color=#0094FF, title="Upper")
plot(ma, color=#0094FF, title="Basis")
l = plot(lower, color=#0094FF, title="Lower")

c = (dir0 == 1 ? color.new(color.green,80) : color.new(color.red,80))
fill(u, l, color=c, title="Background")

p = ta.sar(psar_Start,psar_Accel,psar_Max)
plot(p, color=color.yellow, title="psar")
sell = dir0 <0 and p[1]<low[1] and p[0]>high[0] //and high[0]>=ma[1]
buy  = dir0 >0 and p[1]>high[1] and p[0]<low[0] //and low[0]<=ma[1]
//strategy.entry(id, long, qty, limit, stop, oca_name, oca_type, comment, when, alert_message)
if(buy) 
    strategy.cancel("ParSE")
if(sell) 
    strategy.cancel("ParLE")
if(qty<0)
    size := math.abs(qty)
else
    size := math.max(1, qty*strategy.equity / (3*ta.atr(14)*syminfo.pointvalue))

bool insession = BarInSession(s1) or BarInSession(s2)
strategy.entry("ParLE", strategy.long, size, stop=p[1], when=buy and insession, comment="ParLE")
strategy.entry("ParSE", strategy.short, size, stop=p[1], when=sell and insession, comment="ParSE")
strategy.exit("exit","ParSE",stop = math.min(p,upper),limit=strategy.position_avg_price- (tgtmult==0?10000 : tgtmult)*ta.atr(tgtatrperiod))
strategy.exit("exit","ParLE",stop = math.max(p,lower),limit=strategy.position_avg_price+ (tgtmult==0?10000 : tgtmult)*ta.atr(tgtatrperiod))

if(false and time_found == -1)
    var label1 = label.new(bar_index, low, text="Hello, world!", style=label.style_circle)
    label.set_x(label1, 0)
    label.set_xloc(label1, time, xloc.bar_time)
    label.set_color(label1, color.red)
    label.set_size(label1, size.large)


*/
}
