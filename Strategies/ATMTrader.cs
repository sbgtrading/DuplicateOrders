#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
//using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class BuyAtFixedPriceStrategy : Strategy
    {
        // User-defined input for the fixed price level
        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name = "Fixed Price Level", Order = 1, GroupName = "Parameters")]
        public double FixedPrice { get; set; }

        // ATM Strategy Template name
        private string atmStrategyTemplate = "YourATMStrategyTemplateName";

        // Variables to track the ATM strategy
        private string atmStrategyId;
        private string orderId;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "A strategy that buys 1 contract using an ATM Strategy Template when the price hits a specific fixed value and the market position is flat.";
                Name = "BuyAtFixedPriceStrategy";
                Calculate = Calculate.OnEachTick;
                IsUnmanaged = true;
            }
            else if (State == State.Configure)
            {
                // Set order and ATM strategy IDs to empty
                orderId = string.Empty;
                atmStrategyId = string.Empty;
            }
        }

		bool IsArmed = false;
        protected override void OnBarUpdate()
        {
            if (CurrentBar<5)
                return;

            if (Position.MarketPosition != MarketPosition.Flat)
				IsArmed = true;
            // Check if the current market position is flat
            if (State != State.Realtime && Position.MarketPosition == MarketPosition.Flat && !IsArmed)
            {
				IsArmed = true;
                // Check if the current price is equal to or greater than the fixed price level
				bool c1 = Close[1] < FixedPrice && Close[0] >= FixedPrice;
				bool c2 = Close[1] > FixedPrice && Close[0] <= FixedPrice;
                if (c1 || c2)
                {
                    // Generate a new order ID
                    orderId = GetAtmStrategyUniqueId();
                    atmStrategyId = GetAtmStrategyUniqueId();

                    // Submit an entry order to buy 1 contract using the ATM strategy template
                    AtmStrategyCreate(
                        OrderAction.Buy,
                        OrderType.Market,
                        1,
                        0,
                        TimeInForce.Day,
						orderId,
                        atmStrategyTemplate,
                        atmStrategyId,
                        (atmCallbackErrorCode, atmCallbackId) =>
                        {
                            if (atmCallbackErrorCode != ErrorCode.NoError)
                                Print("Error creating ATM strategy: " + atmCallbackErrorCode);
                        }
                    );
                }
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            // Add any additional logic for execution updates if needed
        }

        protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
        {
            // Add any additional logic for position updates if needed
        }
    }
}

