#region Using declarations
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class SubmitOrderToTwoAccounts : Indicator
	{
		private Account account1;
		private Account account2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SubmitOrderToTwoAccounts";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AccountName = "Sim101";
				AccountName2 = "Sim102";
				
			}
			else if (State == State.DataLoaded)
			{
				lock (Account.All)
				{
		            account1 = Account.All.FirstOrDefault(a => a.Name == AccountName);
					account2 = Account.All.FirstOrDefault(a => a.Name == AccountName2);
				}
		        // Subscribe to account item and order updates
		        if (account1 != null)
				{	            
					account1.OrderUpdate 		+= OnOrderUpdate;
				}
			}
			else if(State == State.Terminated)
			{
				// Make sure to unsubscribe to the account item subscription
        		if (account1 != null)
				{
					account1.OrderUpdate 		-= OnOrderUpdate;				
				}
			}
		}
		
	    private void OnOrderUpdate(object sender, OrderEventArgs e)
	    {
			if (e.Order.Account == account1)
			{
				Order sellOrder = null;
				Order buyOrder = null;
				if (e.OrderState == OrderState.Filled && e.Order.IsLong == true)
				{
					
					buyOrder = account2.CreateOrder(Instrument, OrderAction.Buy, OrderType.Market, OrderEntry.Manual, TimeInForce.Day, 1, e.AverageFillPrice, 0, "", "buyOrder"+DateTime.Now.ToString(), DateTime.MaxValue, null);
					account2.Submit(new[] { buyOrder });
				}
				else if (e.OrderState == OrderState.Filled && e.Order.IsShort == true)
				{
					sellOrder = account2.CreateOrder(Instrument, OrderAction.Sell, OrderType.Market, OrderEntry.Manual, TimeInForce.Day, 1, e.Order.AverageFillPrice, 0, "", "sellOrder"+DateTime.Now.ToString(), DateTime.MaxValue, null);
					account2.Submit(new[] {sellOrder });
				}
			}
			
	    }
		
		protected override void OnBarUpdate()
		{
			if (State == State.Historical)
				return;
			
			if (CurrentBar < 1)
				return;	
		}
		
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string AccountName { get; set; }
		
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string AccountName2 { get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SubmitOrderToTwoAccounts[] cacheSubmitOrderToTwoAccounts;
		public SubmitOrderToTwoAccounts SubmitOrderToTwoAccounts()
		{
			return SubmitOrderToTwoAccounts(Input);
		}

		public SubmitOrderToTwoAccounts SubmitOrderToTwoAccounts(ISeries<double> input)
		{
			if (cacheSubmitOrderToTwoAccounts != null)
				for (int idx = 0; idx < cacheSubmitOrderToTwoAccounts.Length; idx++)
					if (cacheSubmitOrderToTwoAccounts[idx] != null &&  cacheSubmitOrderToTwoAccounts[idx].EqualsInput(input))
						return cacheSubmitOrderToTwoAccounts[idx];
			return CacheIndicator<SubmitOrderToTwoAccounts>(new SubmitOrderToTwoAccounts(), input, ref cacheSubmitOrderToTwoAccounts);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SubmitOrderToTwoAccounts SubmitOrderToTwoAccounts()
		{
			return indicator.SubmitOrderToTwoAccounts(Input);
		}

		public Indicators.SubmitOrderToTwoAccounts SubmitOrderToTwoAccounts(ISeries<double> input )
		{
			return indicator.SubmitOrderToTwoAccounts(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SubmitOrderToTwoAccounts SubmitOrderToTwoAccounts()
		{
			return indicator.SubmitOrderToTwoAccounts(Input);
		}

		public Indicators.SubmitOrderToTwoAccounts SubmitOrderToTwoAccounts(ISeries<double> input )
		{
			return indicator.SubmitOrderToTwoAccounts(input);
		}
	}
}

#endregion
