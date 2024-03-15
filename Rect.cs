using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NinjaTrader.Custom.Indicators.Customs
{
    public class Rect
    {
        public int Width 
        {
            get;
            set;
        }
        public Brush BorderColor 
        {
            get;
            set;
        }
        public Brush BackgroundColor 
        {
            get;
            set;
        }
        public byte Opacity 
        {
            get;
            set;
        }

        public bool OnPricePanel
        {
            get
            {
                return true;
            }
        }

        public double UpperPrice 
        {
            get;
            set;
        }
        public double LowerPrice 
        {
            get;
            set;
        }
    }
}
