using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.Customs
{
    public class FactoryBar
    {
        public static MyBar Create(double low, double high, double close, double open)
        {
			//Low[position], High[position], Close[position], Open[position]
			MyBar bar = new DojiBar(low, high, close, open);

			if (open < close)
			{
				bar = new BullBar(low, high, close, open);
			}
			else if (open > close)
			{
				bar = new BearBar(low, high, close, open);
			}

			return bar;
		}
    }
}


