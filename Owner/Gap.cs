using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.Customs
{
	public abstract  class Gap
	{
        public Rect Box 
		{
			get;
			set;
		}
        public string Tag 
		{
			get;
			set;
		}
		public FillType State 
		{
			get;
			set;
		}
		public int NumberBar
		{
			get;
			set;
		}

		public Gap(string tag, double lowerPrice, double upperPrice)
		{
			this.Tag		= tag;
			this.Box		= new Rect() { LowerPrice = lowerPrice, UpperPrice = upperPrice };
			this.State		= FillType.Open;
		}


		//public abstract void Draw(NinjaTrader.NinjaScript.Indicators.Indicator owner, int currentBarNumber, int twoPreviousBarIndex, MyBar currentBar, MyBar twoPreviousBar);

		public abstract bool Closed(double close);
		public abstract bool NegativeGap(MyBar barCompare);

		public virtual void UpdateBox(MyBar barCurrent)
		{
			Box.LowerPrice = barCurrent.Close;
			State = FillType.Negative;
		}

	}

	public enum FillType
	{
		Negative,
		Closed,
		Open,
	}

}

