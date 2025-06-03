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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RallyBase15min : Indicator
	{
		private const int MAJOR_BUY = 1;
		private const int MINOR_BUY = 2;
		private const int MAJOR_SELL = 3;
		private const int MINOR_SELL = 4;
		
		private string AddSoundFolder(string wav){
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", wav);
		}
		
		internal class LoadSoundFileList : StringConverter
		{
			#region
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

				var list = new List<string>();
				list.Add("SOUND OFF");
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
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Rally candle followed by a basing/consolidation candle, breakout beyond base candle";
				Name										= "RallyBase 15-min";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;
				BuyWAV					= @"SOUND OFF";
				SellWAV					= @"SOUND OFF";
				BuyWAVMinor				= @"SOUND OFF";
				SellWAVMinor			= @"SOUND OFF";
				AddPlot(new Stroke(Brushes.SpringGreen, 8), PlotStyle.TriangleUp, "Buy");
				AddPlot(new Stroke(Brushes.OrangeRed, 8), PlotStyle.TriangleDown, "Sell");
				AddPlot(new Stroke(Brushes.SpringGreen, 5), PlotStyle.TriangleUp, "BuyMinor");
				AddPlot(new Stroke(Brushes.OrangeRed, 5), PlotStyle.TriangleDown, "SellMinor");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute,15);
			}
		}

		List<double> pcts = new List<double>();
		double EntryPrice = double.MinValue;
		double SignalABar = -1;
		int SignalType = 0;//MAJOR_BUY, MINOR_BUY, MAJOR_SELL, MINOR_SELL
		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1) return;

			if(BarsInProgress == 1)//15-minute datafeed
			{
				int bip = 1;
				pcts.Insert(0, (Closes[bip][0]-Opens[bip][0])/(Highs[bip][0]-Lows[bip][0]));
				if(pcts.Count>2){
					double midprice = (Highs[bip][1]-Lows[bip][1])/3 + Lows[bip][1];
					var c1_up_rally = pcts[1] > 0.7;
					var c1_up_rally_minor = pcts[1] > 0.5;
					var c2_base = Math.Abs(pcts[0]) < 0.5;
					var c2_base_low = Lows[bip][0] > midprice;
					var rally_bigger_than_drop = Highs[bip][1] - Lows[bip][1] > Highs[bip][0] - Lows[bip][0];
					if(SignalType == MAJOR_BUY || SignalType == MINOR_BUY){
						EntryPrice = Math.Min(EntryPrice, Highs[bip][0]) + TickSize;
//						var x = Draw.Dot(this,$"B{CurrentBars[bip]}", false, 1, midprice, SignalType == MAJOR_BUY ? Brushes.Blue : Brushes.Yellow);
//						x.Size = ChartMarkerSize.Large;
					}
					if(c1_up_rally && c2_base && c2_base_low && rally_bigger_than_drop){
						var x = Draw.Dot(this,$"B{CurrentBars[bip]}", false, 1, midprice, Brushes.Blue);
						x.Size = ChartMarkerSize.Large;
						SignalType = MAJOR_BUY;
						EntryPrice = Highs[bip][0]+TickSize;
						SignalABar = CurrentBars[0];
					}
					else if(c1_up_rally_minor && c2_base && c2_base_low && rally_bigger_than_drop){
						var x = Draw.Dot(this,$"Bm{CurrentBars[bip]}", false, 1, midprice, Brushes.Yellow);
						x.Size = ChartMarkerSize.Large;
						SignalType = MINOR_BUY;
						EntryPrice = Highs[bip][0]+TickSize;
						SignalABar = CurrentBars[0];
					}
					midprice = Highs[bip][1] - (Highs[bip][1]-Lows[bip][1])/3;
					var c1_dn_rally = pcts[1] < -0.7;
					var c1_dn_rally_minor = pcts[1] < -0.5;
					var c2_base_high = Highs[bip][0] < midprice;
					if(SignalType == MAJOR_SELL || SignalType == MINOR_SELL){
						EntryPrice = Math.Max(EntryPrice, Lows[bip][0]) - TickSize;
//						var x = Draw.Dot(this,$"B{CurrentBars[bip]}", false, 1, midprice, SignalType == MAJOR_SELL ? Brushes.Blue : Brushes.Yellow);
//						x.Size = ChartMarkerSize.Large;
					}
					if(c1_dn_rally && c2_base && c2_base_high && rally_bigger_than_drop){
						var x = Draw.Dot(this,$"S{CurrentBars[bip]}", false, 1, midprice, Brushes.Blue);
						x.Size = ChartMarkerSize.Large;
						SignalType = MAJOR_SELL;
						EntryPrice = Lows[bip][0]-TickSize;
						SignalABar = CurrentBars[0];
					}
					else if(c1_dn_rally_minor && c2_base && c2_base_high && rally_bigger_than_drop){
						var x = Draw.Dot(this,$"Sm{CurrentBars[bip]}", false, 1, midprice, Brushes.Yellow);
						x.Size = ChartMarkerSize.Large;
						SignalType = MINOR_SELL;
						EntryPrice = Lows[bip][0]-TickSize;
						SignalABar = CurrentBars[0];
					}
				}
			}else{
				if(SignalType == MAJOR_BUY){
					Buy[0] = EntryPrice;
					if(Highs[0][0]>=EntryPrice) {
						SignalType = 0;
						Alert("Major Buy Signal", Priority.High, "Major Buy", AddSoundFolder(BuyWAV), 0, Brushes.SpringGreen, Brushes.Black);
					}
				}else if(SignalType == MINOR_BUY){
					BuyMinor[0] = EntryPrice;
					if(Highs[0][0]>=EntryPrice) {
						SignalType = 0;
						Alert("Minor Buy Signal", Priority.Medium, "Minor Buy", AddSoundFolder(BuyWAVMinor), 0, Brushes.SpringGreen, Brushes.Black);
					}
				}else if(SignalType == MAJOR_SELL){
					Sell[0] = EntryPrice;
					if(Lows[0][0]<=EntryPrice) {
						SignalType = 0;
						Alert("Major Sell Signal", Priority.High, "Major Sell", AddSoundFolder(SellWAV), 0, Brushes.OrangeRed, Brushes.Black);
					}
				}else if(SignalType == MINOR_SELL){
					SellMinor[0] = EntryPrice;
					if(Lows[0][0]<=EntryPrice) {
						SignalType = 0;
						Alert("Minor Sell Signal", Priority.Medium, "Minor Sell", AddSoundFolder(SellWAVMinor), 0, Brushes.OrangeRed, Brushes.Black);
					}
				}
			}
		}

		#region -- Plots --
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Buy
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Sell
		{
			get { return Values[1]; }
		}
		#endregion

		#region Properties
	
		[Display(Name="Buy Sound", Description="Sound file for major buy alerts", Order=2, GroupName="Parameters")]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string BuyWAV
		{ get; set; }
		
		[Display(Name="Sell Sound", Description="Sound file for major sell alerts", Order=3, GroupName="Parameters")]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SellWAV
		{ get; set; }
		
		[Display(Name="Buy Minor Sound", Description="Sound file for minor buy alerts", Order=4, GroupName="Parameters")]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string BuyWAVMinor
		{ get; set; }
		
		[Display(Name="Sell Minor Sound", Description="Sound file for minor sell alerts", Order=5, GroupName="Parameters")]
		[TypeConverter(typeof(LoadSoundFileList))]
		public string SellWAVMinor
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyMinor
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellMinor
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RallyBase15min[] cacheRallyBase15min;
		public RallyBase15min RallyBase15min()
		{
			return RallyBase15min(Input);
		}

		public RallyBase15min RallyBase15min(ISeries<double> input)
		{
			if (cacheRallyBase15min != null)
				for (int idx = 0; idx < cacheRallyBase15min.Length; idx++)
					if (cacheRallyBase15min[idx] != null &&  cacheRallyBase15min[idx].EqualsInput(input))
						return cacheRallyBase15min[idx];
			return CacheIndicator<RallyBase15min>(new RallyBase15min(), input, ref cacheRallyBase15min);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RallyBase15min RallyBase15min()
		{
			return indicator.RallyBase15min(Input);
		}

		public Indicators.RallyBase15min RallyBase15min(ISeries<double> input )
		{
			return indicator.RallyBase15min(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RallyBase15min RallyBase15min()
		{
			return indicator.RallyBase15min(Input);
		}

		public Indicators.RallyBase15min RallyBase15min(ISeries<double> input )
		{
			return indicator.RallyBase15min(input);
		}
	}
}

#endregion
