
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;


namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class IwMeanRenko : NinjaTrader.Data.BarsType
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"";
				Name = "IW MeanRenko";
				bool IsDebug = System.IO.File.Exists("c:\\222222222222.txt");
				IsDebug = IsDebug && (NinjaTrader.Cbi.License.MachineId.CompareTo("B0D2E9D1C802E279D3678D7DE6A33CE4")==0 || NinjaTrader.Cbi.License.MachineId.CompareTo("766C8CD2AD83CA787BCA6A2A76B2303B")==0);
				if(!IsDebug)
					VendorLicense("IndicatorWarehouse", "AIMeanRenko", "www.IndicatorWarehouse.com", "License@indicatorwarehouse.com");

				this.BarsPeriod = new NinjaTrader.Data.BarsPeriod { BarsPeriodType = (NinjaTrader.Data.BarsPeriodType)22220, BarsPeriodTypeName = "IwMeanRenko", Value = 8, Value2 = 6 };
				BuiltFrom = NinjaTrader.Data.BarsPeriodType.Tick;
				DaysToLoad = 5;
				IsIntraday = true;
			}
			else if (State == State.Configure)
			{
				Name = "IwMeanRenko {" + BarsPeriod.Value.ToString() + ";" + BarsPeriod.Value2.ToString() + "}" + (BarsPeriod.MarketDataType != NinjaTrader.Data.MarketDataType.Last ? " - " + BarsPeriod.MarketDataType : string.Empty);

				Properties.Remove(Properties.Find("ReversalType", true));
				Properties.Remove(Properties.Find("BaseBarsPeriodType", true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue", true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType", true));

				SetPropertyName("Value", "Trend");
				SetPropertyName("Value2", "Reversal");
			}
			#region --- DO NOT REMOVE : For Later Use if no change in NT8 ---
//            else if (State == State.Configure)
//            {
//                string newStr = BarsPeriod.PointAndFigurePriceType == PointAndFigurePriceType.Close ? "Off" : "On";
//                Name = "NS_RenkoBT {" + BarsPeriod.Value.ToString() + ";" + BarsPeriod.Value2.ToString() + "}" + (BarsPeriod.MarketDataType != MarketDataType.Last ? " - " + BarsPeriod.MarketDataType : string.Empty);

//                Properties.Remove(Properties.Find("ReversalType", true));
//                Properties.Remove(Properties.Find("BaseBarsPeriodType", true));
                
//                SetPropertyName("Value", "Bar Size (ticks)");
//                SetPropertyName("Value2", "Open Offset (ticks)");
//                SetPropertyName("BaseBarsPeriodValue", "Reversal Size (ticks)");
                
//                SetPropertyName("PointAndFigurePriceType", "Backtest Mode");
//                NS_BacktestModePropertyDescriptor btDescriptor = new NS_BacktestModePropertyDescriptor(Properties.Find("PointAndFigurePriceType", true), BarsPeriod, BarsPeriod.PointAndFigurePriceType);                                
//                Properties.Add(btDescriptor);                
//                Properties.Remove(Properties.Find("PointAndFigurePriceType", true));
//                //Properties["PointAndFigurePriceType"].SetValue(BarsPeriod, newStr);
//            }
			#endregion
		}

        #region ##### generic #####
        public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) { return 3; }
        public override void ApplyDefaultBasePeriodValue(NinjaTrader.Data.BarsPeriod period) { }
        public override void ApplyDefaultValue(NinjaTrader.Data.BarsPeriod period)
        {
            period.BarsPeriodTypeName = "IwMeanRenko";
            period.Value = 8;
            period.Value2 = 8;
        }
        public override string ChartLabel(System.DateTime time) { return time.ToString("T", Core.Globals.GeneralOptions.CurrentCulture); }
        public override double GetPercentComplete(NinjaTrader.Data.Bars bars, System.DateTime now) { return 0; }
//        public override bool   IsRemoveLastBarSupported { get { return true; } }
        #endregion

        private double trendOffset=0;
		private double reversalOffset=0;
		private double barOpen =0;
		private double barMax =0;
		private double barMin =0;
		private double barDirection =0;
		private bool   maxExceeded = false;
		private bool   minExceeded = false;
		private double fakeOpen = 0;

        private double RndToTickSize(double value, NinjaTrader.Data.Bars bars) { 
			int ti = (int)System.Math.Round(value / bars.Instrument.MasterInstrument.TickSize,0);
			return ti * bars.Instrument.MasterInstrument.TickSize; 
		}

        protected override void OnDataPoint(NinjaTrader.Data.Bars bars, double open, double high, double low, double close, System.DateTime time, long volume, bool isBar, double bid, double ask)
        {
            if (SessionIterator == null) SessionIterator = new NinjaTrader.Data.SessionIterator(bars);//Code Breaking Change - From Beta 9
            bool isNewSession = SessionIterator.IsNewSession(time, isBar);//Code Breaking Change - From Beta 9 
            if (isNewSession) SessionIterator.CalculateTradingDay(time, isBar);//Code Breaking Change - From Beta 9 

			var tickSize = bars.Instrument.MasterInstrument.TickSize;
			//### First Bar
			if (bars.Count == 0 || isNewSession)
			{
				//### Parse Long Param Specification
//				if ( bars.Period.Value >= 1000000 ) {
//					int d; string str = bars.Period.Value.ToString("000000000");
//					d=0; Int32.TryParse(str.Substring(0,3), out d); bars.Period.Value  = d;
//					d=0; Int32.TryParse(str.Substring(3,3), out d); bars.Period.Value2 = d;
//				}

				trendOffset    = bars.BarsPeriod.Value  * tickSize;
				reversalOffset = bars.BarsPeriod.Value2 * tickSize;

				barOpen = close;
				barMax  = barOpen + (trendOffset * barDirection);
				barMin  = barOpen - (trendOffset * barDirection);

				AddBar(bars, barOpen, barOpen, barOpen, barOpen, time, volume);
			}
			//### Subsequent Bars
			else
			{
				//Data.Bar bar = (Bar)bars.Get(bars.Count - 1);

				maxExceeded  = bars.Instrument.MasterInstrument.Compare(close, barMax) >= 0 ? true : false;
				minExceeded  = bars.Instrument.MasterInstrument.Compare(close, barMin) <= 0 ? true : false;

				//### Defined Range Exceeded?
				if ( maxExceeded || minExceeded )
				{
					double thisClose = maxExceeded ? System.Math.Min(close, barMax) : minExceeded ? System.Math.Max(close, barMin) : close;
					barDirection     = maxExceeded ? 1 : minExceeded ? -1 : 0;

					//### Close Current Bar
					UpdateBar(bars, 
							(maxExceeded ? thisClose : bars.GetHigh(bars.Count - 1)), 
							(minExceeded ? thisClose : bars.GetLow(bars.Count - 1)), 
							thisClose, time, volume);

					fakeOpen = 0.5 * (bars.GetClose(bars.Count - 1) + bars.GetOpen(bars.Count - 1));		//### Fake Open is halfway down the bar
					int ti = (int)(fakeOpen/tickSize);
					fakeOpen = ti * tickSize;
					//### Add New Bar
					barMax  = fakeOpen + ((barDirection>0 ? trendOffset : reversalOffset) );
					barMin  = fakeOpen - ((barDirection>0 ? reversalOffset : trendOffset) );
					AddBar(bars, 
							fakeOpen, 
							(maxExceeded ? thisClose : fakeOpen), 
							(minExceeded ? thisClose : fakeOpen), 
							close, time, volume);
				}
				//### Current Bar Still Developing
				else
				{
					UpdateBar(bars, 
							(close > bars.GetHigh(bars.Count - 1) ? close : bars.GetHigh(bars.Count - 1)), 
							(close < bars.GetLow(bars.Count - 1) ? close : bars.GetLow(bars.Count - 1)), 
							close, time, volume);
				}
			}
			bars.LastPrice = close;
        }
    }

    #region -- DO NOT REMOVE : For later use if no change in NT8 --
    /*Custom property descriptor used to display the backtest mode in the property grid*/
    //#region public class NS_BacktestModePropertyDescriptor : PropertyDescriptor
    //public class NS_BacktestModePropertyDescriptor : PropertyDescriptor
    //{
    //    private readonly PropertyDescriptor pParent;

    //    #region -- Ctor. --
    //    public NS_BacktestModePropertyDescriptor(PropertyDescriptor parent, object component, object value)
    //        : base(parent, createNewSetofAttributes(parent.Attributes, new TypeConverterAttribute(typeof(NS_BacktestModeConverter))))
    //    {
    //        pParent = parent;
    //        pParent.SetValue(component, value);
    //    }

    //    protected NS_BacktestModePropertyDescriptor(MemberDescriptor descriptor) : base(descriptor) { }
    //    protected NS_BacktestModePropertyDescriptor(MemberDescriptor descriptor, Attribute[] attributes) : base(descriptor, attributes) { }
    //    protected NS_BacktestModePropertyDescriptor(string name, Attribute[] attributes) : base(name, attributes) { }
    //    #endregion

    //    public override bool CanResetValue(object component) { return pParent.CanResetValue(component); }
    //    public override object GetValue(object component)
    //    {
    //        return pParent.GetValue(component);
    //    }
    //    public override void ResetValue(object component) { pParent.ResetValue(component); }
    //    public override void SetValue(object component, object value) { pParent.SetValue(component, (string)value == "Off" ? PointAndFigurePriceType.Close : PointAndFigurePriceType.HighsAndLows); }
    //    public override bool ShouldSerializeValue(object component) { return pParent.ShouldSerializeValue(component); }

    //    public override Type ComponentType { get { return typeof(NS_BacktestModeConverter); } }
    //    public override bool IsReadOnly { get { return pParent.IsReadOnly; } }
    //    public override Type PropertyType { get { return pParent.PropertyType; } }

    //    public override string DisplayName { get { return ""; } }//doesn't work
    //    public override string Description { get { return ""; } }//doesn't work

    //    #region -- Static method --
    //    private static Attribute[] createNewSetofAttributes(AttributeCollection originals, Attribute newType)
    //    {
    //        List<Attribute> newAtts = new List<Attribute>();
    //        foreach (Attribute att in originals)
    //        {
    //            if (!newType.GetType().IsAssignableFrom(att.GetType())) { newAtts.Add(att); }
    //        }
    //        newAtts.Add(newType);
    //        return newAtts.ToArray();
    //    }
    //    #endregion
    //}
    //#endregion

    //public enum NS_BacktestMode { Off, On }

    ///*This class is a converter used in the property grid to display the backtestmode enum*/
    //#region public class NS_BacktestModeConverter : EnumConverter
    //public class NS_BacktestModeConverter : EnumConverter
    //{
    //    public NS_BacktestModeConverter() : base(typeof(NS_BacktestMode)) { }

    //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    //    {
    //        if ((sourceType != typeof(NS_BacktestMode)) && (sourceType != typeof(string))) return base.CanConvertFrom(context, sourceType);
    //        return true;
    //    }

    //    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    //    {
    //        if ((destinationType != typeof(NS_BacktestMode)) && (destinationType != typeof(string))) return base.CanConvertTo(context, destinationType);
    //        return true;
    //    }

    //    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    //    {
    //        Type type = value.GetType();
    //        if (type == typeof(string)) return ((string)value).ToUpper() == "Off" ? NS_BacktestMode.Off : NS_BacktestMode.On;
    //        if (type == typeof(NS_BacktestMode)) return ((NS_BacktestMode)value).ToString();
    //        return base.ConvertFrom(context, culture, value);
    //    }

    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    //    {
    //        Type type = value.GetType();
    //        if ((type == typeof(string)) && (destinationType == typeof(NS_BacktestMode))) return ((string)value).ToUpper() == "OFF" ? NS_BacktestMode.Off : NS_BacktestMode.On;
    //        if ((type == typeof(NS_BacktestMode)) && (destinationType == typeof(string))) return ((NS_BacktestMode)value).ToString();
            
    //        return base.ConvertTo(context, culture, value, destinationType);
    //    }
    //}
    //#endregion
    
    #endregion
}

#region -- DO NOT REMOVE : For later use if no change in NT8 --
namespace NinjaTrader.Data
{
    //[CategoryOrder(typeof(Resource), "NinjaScriptDataSeries", 2)]
    //[TypeConverter(typeof(BarsPeriodConverter))]
    //public class BarsPeriodb : BarsPeriod
    //{
    //    [Display(ResourceType = typeof(Resource), Name = "TimeFilterValue", GroupName = "NinjaScriptDataSeries", Order = 3)]
    //    [Range(1, int.MaxValue)]
    //    public int TimeFilterValue { get; set; }
    //}

    //public class BarsPeriodbConverter : BarsPeriodConverter
    //{
    //    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
    //    {
    //        PropertyDescriptorCollection properties = base.GetProperties(context, component, attrs);
            
    //        NS_BacktestModePropertyDescriptor btDescriptor = new NS_BacktestModePropertyDescriptor(properties.Find("PointAndFigurePriceType", true));
    //        properties.Add(btDescriptor);

    //        properties.Remove(properties.Find("PointAndFigurePriceType", true));

    //        return properties;
    //    }
    //}
}
#endregion


