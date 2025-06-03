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
namespace NinjaTrader.NinjaScript.Strategies
{
	public class ListTrades : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ListTrades";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				pSelectedAccountName					= @"Sim101";
				pStartOfTradingDT = DateTime.Now.AddDays(-1);
				AddPlot(Brushes.Orange, "PnL");
			}
			else if (State == State.Configure)
			{
			}
		}

		Account myAccount = null;
		protected override void OnBarUpdate()
		{
			if(CurrentBar >= Bars.Count-3){
				var accts = Account.All.ToList();
				for(int i = 0; i<accts.Count; i++){
					if(accts[i].Name == pSelectedAccountName){
						accts = null;
						break;
					}
				}
				if(accts == null){
					lock (Account.All)
						myAccount = Account.All.FirstOrDefault(a => a.Name == pSelectedAccountName);
				}
				if(myAccount==null) Print("Account is null");
				else if(myAccount!=null){
					Print("/n/nPositions: "+myAccount.Positions.Count);
					foreach(var p in myAccount.Positions.ToList()){
						Print(p.ToString()+" with PnL: "+p.GetUnrealizedProfitLoss(PerformanceUnit.Currency));
//						Print("Exec Positions:");
//						if(p.Account!=null && p.Account.ExecutionPositions!=null && p.Account.ExecutionPositions.Positions !=null){
//							var ep = p.Account.ExecutionPositions.Positions.ToList();
//							foreach(var ex in ep)
//								Print(ex.ToString());
//						}
//						Print("Transactions:");
//						var t = p.Account.Transactions.ToList();
//						foreach(var tx in t)
//							Print(tx.ToString());
					}
				}
			}
		}

		#region Properties
	    public class LoadAccountNameList : TypeConverter
	    {
			#region -- LoadAccountNameList --
	        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	        {
	            if (context == null)
	            	return null; 
				
	            System.Collections.ArrayList list = new System.Collections.ArrayList();

	            for (int i = 0; i < Account.All.Count; i++)
	            {
	                if (Account.All[i].ConnectionStatus == ConnectionStatus.Connected)
	                    list.Add(Account.All[i].Name);
	            }
				if(list.Count==0) list.Add("<no connection>");
	            return new TypeConverter.StandardValuesCollection(list);
	        }

	        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	        { return true; }
			#endregion
	    }

		[NinjaScriptProperty]
		[TypeConverter(typeof(LoadAccountNameList))]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Account Name", GroupName = "Parameters", Order = 10)]
		public string pSelectedAccountName {get;set;}

		[Display(ResourceType = typeof(Custom.Resource), Name = "Start of trading", GroupName = "Parameters", Order = 20)]
		public DateTime pStartOfTradingDT {get;set;}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PnL
		{
			get { return Values[0]; }
		}
		#endregion

	}
}







