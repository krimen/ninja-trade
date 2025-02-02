using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.Gui.Tools;

namespace NinjaTrader.Custom.Indicators.Customs.AddOns
{
    public class CopyTradingWindowFactory : INTTabFactory
    {
        public NTWindow CreateParentWindow()
        {
            return new CopyTradingWindow();
        }

        public NTTabPage CreateTabPage(string typeName, bool isNewWindow = false)
        {
            return new CopyTradingTab();
        }
    }
}
