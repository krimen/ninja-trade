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

namespace NinjaTrader.Custom.Indicators.Customs
{
	public abstract class MyBar
	{
		public double Low { get; set; }
		public double High { get; set; }
		public double Close { get; set; }
		public double Open { get; set; }

		public MyBar(double low, double high, double close, double open)
		{
			this.Low = low;
			this.High = high;
			this.Close = close;
			this.Open = open;
		}

		/*public virtual void Draw(Indicator indicator)
		{

		}*/

		protected virtual bool IsOutside(MyBar previousBar)
		{
			bool outsideBar = ((this.High >= previousBar.High && this.Low < previousBar.Low) || (this.Low <= previousBar.Low && this.High > previousBar.High));
			return outsideBar;
		}

		protected virtual bool IsInside(MyBar previousBar)
		{
			bool insideBar = (this.High <= previousBar.High && this.Low >= previousBar.Low);
			return insideBar;
		}

		public abstract SolidColorBrush? ResolveColor(MyBar barCompare);
		
		public virtual string ToString(MyBar barCompare) 
		{
			if(IsInside(barCompare))
			{
				return "i";
			}
			else if (IsOutside(barCompare))
			{
				return "O";
			}
			
			return string.Empty;
		}
		
		public virtual bool GapOnOpen(MyBar barCompare)
		{
			bool hasGap = this.Open > barCompare.Close || this.Open < barCompare.Close;
			return hasGap;
		}

	}

	public class BullBar : MyBar
	{
		public BullBar(double low, double high, double close, double open) : base(low, high, close, open)
		{

		}

		public override string ToString()
		{
			return "Bull Bar";
		}

		public override SolidColorBrush? ResolveColor(MyBar barCompare)
		{
			if(IsInside(barCompare))
            {
				return Brushes.GreenYellow; 
			}
			else if(IsOutside(barCompare))
            {
				return Brushes.SteelBlue;

			}

			return null;
		}
		
		
	}

	public class BearBar : MyBar
	{
		public BearBar(double low, double high, double close, double open) : base(low, high, close, open)
		{

		}

		public override string ToString()
		{
			return "Bear";
		}

		public override SolidColorBrush? ResolveColor(MyBar barCompare)
		{
			if (IsInside(barCompare))
			{
				return Brushes.Salmon;
			}
			else if (IsOutside(barCompare))
			{
				return Brushes.Magenta;

			}

			return null;
		}
	}

	public class DojiBar : MyBar
	{
		public DojiBar(double low, double high, double close, double open) : base(low, high, close, open)
		{

		}

		public override string ToString()
		{
			return "Doji Bar";
		}

		public override SolidColorBrush? ResolveColor(MyBar barCompare)
		{
			return null;
		}
	}
}






