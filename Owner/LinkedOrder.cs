using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.Cbi;

namespace NinjaTrader.Custom.Indicators.Customs
{
    public class LinkedOrder
    {
        public long IdOriginalOrder { get; set; }
        public Order OrderCopied { get; set; }
    }
}
