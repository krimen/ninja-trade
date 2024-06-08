using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.NinjaScript.DrawingTools;

namespace NinjaTrader.Custom.Indicators.Customs
{
    public class BearGap : Gap
    {
        public BearGap(string tag, double lowerPrice, double upperPrice ) :
            base(tag, lowerPrice, upperPrice)
        {

        }

        //public override void Draw(NinjaScript.Indicators.Indicator owner, int currentBarNumber, int twoPreviousBarIndex, MyBar currentBar, MyBar twoPreviousBar)
        //{
        //    string tag = "gapdown" + currentBarNumber;
        //    Draw.Rectangle(owner, tag, false, twoPreviousBarIndex, twoPreviousBar.Low, Box.Width, currentBar.High, Box.BorderColor, Box.BackgroundColor, Box.Opacity, Box.OnPricePanel);
        //}

        public override bool Closed(double close)
        {
            bool closed = close >= Box.UpperPrice;
            return closed;
        }

        public override bool NegativeGap(MyBar barCompare)
        {
            bool negativeGap = barCompare.Close < Box.UpperPrice && barCompare.High > Box.LowerPrice;
            return negativeGap;
        }

    }
}
