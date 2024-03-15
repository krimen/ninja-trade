using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.NinjaScript.DrawingTools;

namespace NinjaTrader.Custom.Indicators.Customs
{
    public class BullGap : Gap
    {
        public BullGap(string tag, double lowerPrice, double upperPrice) :
            base(tag, lowerPrice, upperPrice)
        {

        }

        //public override void Draw(NinjaScript.Indicators.Indicator owner, int currentBarNumber, int twoPreviousBarIndex, MyBar currentBar, MyBar twoPreviousBar)
        //{
        //    string tag = "gapup" + currentBarNumber;
        //    Draw.Rectangle(owner, tag, false, twoPreviousBarIndex, currentBar.Low, Box.Width, twoPreviousBar.High, Box.BorderColor, Box.BackgroundColor, Box.Opacity, Box.OnPricePanel);
        //}

        public override bool Closed(double close)
        {
            bool closed = close <= Box.LowerPrice;
            return closed;
        }

        public override bool NegativeGap(MyBar barCompare)
        {
            bool negativeGap = barCompare.Close > Box.LowerPrice && barCompare.Low < Box.UpperPrice;
            return negativeGap;
        }
    }
}


