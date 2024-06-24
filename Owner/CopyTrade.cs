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
using NinjaTrader.Custom.Indicators.Customs;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Customs
{

    [Gui.CategoryOrder("Accounts", 1)]
    public class CopyTrade : Indicator
    {


        [TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
        [Display(Name = "Sender Account", Description = "Account main that send all orders", Order = 1, GroupName = "Accounts")]
        public string SenderAccount
        {
            get;
            set;
        }


        [TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
        [Display(Name = "Receive Account", Description = "Another account that received orders from sender account", Order = 2, GroupName = "Accounts")]
        public string ReceiverAccount
        {
            get;
            set;
        }

        private Account _sendAccount;
        private Account _receiverAccount;


        private System.Windows.Controls.Grid _grid;
        private System.Windows.Controls.Button _buttonStartCopy;
        private bool _running = false;
        private ICollection<LinkedOrder> _orders = new List<LinkedOrder>();
		private string tagAccountsLabel = "AccountsLabel";

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Copy trade between accounts";
                Name = "CopyTrade";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                PrintTo = PrintTo.OutputTab1;
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.Historical)
            {
                ClearOutputWindow();
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                    {
                        InsertComponentsWpf();
                    }));

                }
            }
            else if (State == State.DataLoaded)
            {
                lock (Account.All)
                {
                    if (!SenderAccount.IsNullOrEmpty() && !ReceiverAccount.IsNullOrEmpty())
                    {
                        _sendAccount = Account.All.FirstOrDefault(account => account.Name == SenderAccount);
                        _receiverAccount = Account.All.FirstOrDefault(account => account.Name == ReceiverAccount);
                    }

                }
            }
            else if (State == State.Terminated)
            {
                // Make sure to unsubscribe to the account item subscription
                if (_sendAccount != null)
                {
                    _sendAccount.OrderUpdate -= OnOrderUpdate;
                }
            }
        }

        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            if (e.Order.Account == _sendAccount)
            {
                Order originalOrder = e.Order;
                Print(":::: OnOrderUpdate ::::");


                if (e.OrderState == OrderState.CancelSubmitted && originalOrder.OrderEntry == OrderEntry.Manual)
                {
                    if (_orders.Count > 0 && _orders.Any())
                    {

                        LinkedOrder linkedOrder = _orders.FirstOrDefault(linked => linked.IdOriginalOrder == e.Order.Id);
                        if (linkedOrder != null)
                        {
                            Order order = linkedOrder.OrderCopied;
                            _receiverAccount.Cancel(new List<Order> { order });
                            _orders.Remove(linkedOrder);
                        }
                    }

                    Print(":::: OrderState.CancelSubmitted ::::");
                }

                if (e.OrderState == OrderState.Submitted)
                {
                    string OcoId = Guid.NewGuid().ToString("N");
                    Order order = null;

                    float ratio = 1.0f;
                    int quantity = (int)(originalOrder.Quantity * ratio);
                    OrderAction orderAction = originalOrder.IsLong ? OrderAction.Buy : OrderAction.Sell;
                    OrderType orderType = originalOrder.IsLimit ? OrderType.Limit : originalOrder.IsMarket ? OrderType.Market : OrderType.StopMarket;


                    order = _receiverAccount.CreateOrder(
                        originalOrder.Instrument,
                        orderAction,
                        orderType,
                        originalOrder.TimeInForce,
                        originalOrder.Quantity,
                        originalOrder.LimitPrice,
                        originalOrder.StopPrice,
                        originalOrder.Oco,
                        originalOrder.Name,
                        originalOrder.CustomOrder);

                    _receiverAccount.Submit(new[] { order });

                    Print("OrderState.Submitted: " + originalOrder.Id);
                    _orders.Add(new LinkedOrder { IdOriginalOrder = originalOrder.Id, OrderCopied = order });
                }

                if (e.OrderState == OrderState.ChangeSubmitted)
                {
                    Print("OrderState.ChangeSubmitted: " + originalOrder.Id);

                    LinkedOrder linkedOrder = _orders.FirstOrDefault(linked => linked.IdOriginalOrder == originalOrder.Id);
                    if (linkedOrder != null)
                    {
                        if (originalOrder.OrderType != OrderType.StopMarket)
                        {
                            linkedOrder.OrderCopied.LimitPrice = originalOrder.LimitPrice;
                            linkedOrder.OrderCopied.LimitPriceChanged = originalOrder.LimitPriceChanged;
                        }
                        else
                        {
                            linkedOrder.OrderCopied.StopPrice = originalOrder.StopPrice;
                            linkedOrder.OrderCopied.StopPriceChanged = originalOrder.StopPriceChanged;
                        }

                        _receiverAccount.Change(new[] { linkedOrder.OrderCopied });
                    }
                }
            }
        }

        private void InsertComponentsWpf()
        {
            _grid = new System.Windows.Controls.Grid
            {
                Name = "grid",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();

            _grid.ColumnDefinitions.Add(column1);

            _buttonStartCopy = new System.Windows.Controls.Button
            {
                Name = "run",
                Content = "Run",
                Foreground = Brushes.White,
                Background = Brushes.Green,
                Margin = new Thickness(0, 20, 30, 0)
            };

            _buttonStartCopy.Click += OnClickStartButton;

            _grid.Children.Add(_buttonStartCopy);
            UserControlCollection.Add(_grid);
        }

        private void OnClickStartButton(object sender, RoutedEventArgs e)
        {

            if (!_running)
            {
                ListenerCopyTrade();
            }
            else
            {
                RemoveListenerCopyTrade();
            }

            _running = !_running;
        }

        private void ListenerCopyTrade()
        {
            if (_sendAccount != null)
            {
                _buttonStartCopy.Content = "Running";
                _sendAccount.OrderUpdate += OnOrderUpdate;
				
				NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 14) { Bold = true };
				string label = $"Account sender: {_sendAccount.DisplayName}\nAccount receiver: {_receiverAccount.DisplayName}"; 
				
				Draw.TextFixed(this, tagAccountsLabel, label, TextPosition.BottomLeft, Brushes.Blue, font, Brushes.Transparent, Brushes.Transparent, 1);
            }
        }

        private void RemoveListenerCopyTrade()
        {
            // Make sure to unsubscribe to the account item subscription
            if (_sendAccount != null)
            {
                _buttonStartCopy.Content = "Run";
                _sendAccount.OrderUpdate -= OnOrderUpdate;
				
				Draw.TextFixed(this, tagAccountsLabel, string.Empty, TextPosition.BottomLeft, Brushes.Blue, null, Brushes.Transparent, Brushes.Transparent, 1);
            }
        }

        protected override void OnBarUpdate()
        {
            if (State == State.Historical)
                return;

            if (CurrentBar < 1)
                return;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Customs.CopyTrade[] cacheCopyTrade;
		public Customs.CopyTrade CopyTrade()
		{
			return CopyTrade(Input);
		}

		public Customs.CopyTrade CopyTrade(ISeries<double> input)
		{
			if (cacheCopyTrade != null)
				for (int idx = 0; idx < cacheCopyTrade.Length; idx++)
					if (cacheCopyTrade[idx] != null &&  cacheCopyTrade[idx].EqualsInput(input))
						return cacheCopyTrade[idx];
			return CacheIndicator<Customs.CopyTrade>(new Customs.CopyTrade(), input, ref cacheCopyTrade);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.CopyTrade CopyTrade()
		{
			return indicator.CopyTrade(Input);
		}

		public Indicators.Customs.CopyTrade CopyTrade(ISeries<double> input )
		{
			return indicator.CopyTrade(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.CopyTrade CopyTrade()
		{
			return indicator.CopyTrade(Input);
		}

		public Indicators.Customs.CopyTrade CopyTrade(ISeries<double> input )
		{
			return indicator.CopyTrade(input);
		}
	}
}

#endregion
