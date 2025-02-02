using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;

namespace NinjaTrader.Custom.Indicators.Customs.AddOns
{
    public class CopyTradingTab : NTTabPage
    {
        private Account masterAccount;

        private Account account;
        public CopyTradingTab()
        {
            // Find our Sim101 account
            lock (Account.All)
                account = Account.All.FirstOrDefault(a => a.Name == "Sim101");

            // Subscribe to execution updates
            if (account != null)
                account.ExecutionUpdate += OnExecutionUpdate;
        }

        /* This method is fired as new executions come in, an existing execution is amended
        (e.g. by the broker's back office), or an execution is removed (e.g. by the broker's back office) */
        private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
        {
            //Order order = e.MarketPosition
            // Output the execution
            NinjaTrader.Code.Output.Process(string.Format("Instrument: {0} Quantity: {1} Price: {2}",
                e.Execution.Instrument.FullName, e.Quantity, e.Price), PrintTo.OutputTab1);
        }

        // Called by TabControl when tab is being removed or window is closed
        public override void Cleanup()
        {
            // Make sure to unsubscribe to the execution subscription
            if (account != null)
                account.ExecutionUpdate -= OnExecutionUpdate;
        }

        protected override string GetHeaderPart(string variable)
        {
            //throw new NotImplementedException();
            return string.Empty;
        }

        protected override void Restore(XElement element)
        {
            //throw new NotImplementedException();
        }

        protected override void Save(XElement element)
        {
            //throw new NotImplementedException();
        }
    }
}
