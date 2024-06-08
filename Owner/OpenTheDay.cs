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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Customs
{
	[Gui.CategoryOrder("HorizontalLine", 1)]
	[Gui.CategoryOrder("Colors", 2)]
	public class OpenTheDay : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "OpenTheDay";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				HorizontalLineColor 						= Brushes.CornflowerBlue;
				HorizontalDashStyle							= DashStyleHelper.Solid;
				PriceColor									= Brushes.Blue;
				LabelColor									= Brushes.Black;
				ShowLabelOpen 								= false; 
			}
			else if (State == State.Configure)
			{
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 1) return;
			
			if(Bars.IsFirstBarOfSession)
			{
				NinjaTrader.Custom.Indicators.Customs.MyBar currentBar =
					NinjaTrader.Custom.Indicators.Customs.FactoryBar.Create(Low[0], High[0], Close[0], Open[0]);


				string openOfDay = currentBar.Open.ToString();
				NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 50) { Bold = true };
				NinjaTrader.Gui.Tools.SimpleFont fontOpenText = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 14) { Bold = true };
				//Draw.TextFixed(this, "OpenOfDayPrice", "Open = " + openOfDay, TextPosition.TopRight, PriceColor, font, Brushes.Transparent, Brushes.Transparent, 1);
				
				//Draw.HorizontalLine(this, "OpenOfDayLine", currentBar.Open, HorizontalLineColor, HorizontalDashStyle, 1);
				Draw.Line(this, "OpenOfDayLine", false, 0, currentBar.Open, -100, currentBar.Open, HorizontalLineColor, HorizontalDashStyle, 1);
				
				if(ShowLabelOpen) 
				{
					//Draw.Text(this, "OpenOfDayLabel", true, "Open", -3, currentBar.Open, 10, LabelColor, fontOpenText, TextAlignment.Center, null, null, 1);
					Draw.TextFixed(this, "OpenOfDayPrice", "Open = " + openOfDay, TextPosition.TopRight, PriceColor, font, Brushes.Transparent, Brushes.Transparent, 1);
				}
			}
		}
		
		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Show Label Open", Description = "Horizontal Line on the Open", Order=1, GroupName= "HorizontalLine")]
		public bool ShowLabelOpen
		{
			get;
			set; 
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Horizontal Line Color", Description = "Horizontal Line on the Open", Order=2, GroupName= "HorizontalLine")]
		public Brush HorizontalLineColor
		{
			get;
			set; 
		}

		[NinjaScriptProperty]
		[Display(Name = "Dash Style", Description = "Style used by line horizontal", Order = 3, GroupName = "HorizontalLine")]
		public DashStyleHelper HorizontalDashStyle
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Price the Open Color", Description = "Value the open color", Order = 4, GroupName = "Colors")]
		public Brush PriceColor
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Label Color Open", Description = "Color of label on Open", Order = 5, GroupName = "Colors")]
		public Brush LabelColor
		{
			get;
			set;
		}
		

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Customs.OpenTheDay[] cacheOpenTheDay;
		public Customs.OpenTheDay OpenTheDay(bool showLabelOpen, Brush horizontalLineColor, DashStyleHelper horizontalDashStyle, Brush priceColor, Brush labelColor)
		{
			return OpenTheDay(Input, showLabelOpen, horizontalLineColor, horizontalDashStyle, priceColor, labelColor);
		}

		public Customs.OpenTheDay OpenTheDay(ISeries<double> input, bool showLabelOpen, Brush horizontalLineColor, DashStyleHelper horizontalDashStyle, Brush priceColor, Brush labelColor)
		{
			if (cacheOpenTheDay != null)
				for (int idx = 0; idx < cacheOpenTheDay.Length; idx++)
					if (cacheOpenTheDay[idx] != null && cacheOpenTheDay[idx].ShowLabelOpen == showLabelOpen && cacheOpenTheDay[idx].HorizontalLineColor == horizontalLineColor && cacheOpenTheDay[idx].HorizontalDashStyle == horizontalDashStyle && cacheOpenTheDay[idx].PriceColor == priceColor && cacheOpenTheDay[idx].LabelColor == labelColor && cacheOpenTheDay[idx].EqualsInput(input))
						return cacheOpenTheDay[idx];
			return CacheIndicator<Customs.OpenTheDay>(new Customs.OpenTheDay(){ ShowLabelOpen = showLabelOpen, HorizontalLineColor = horizontalLineColor, HorizontalDashStyle = horizontalDashStyle, PriceColor = priceColor, LabelColor = labelColor }, input, ref cacheOpenTheDay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.OpenTheDay OpenTheDay(bool showLabelOpen, Brush horizontalLineColor, DashStyleHelper horizontalDashStyle, Brush priceColor, Brush labelColor)
		{
			return indicator.OpenTheDay(Input, showLabelOpen, horizontalLineColor, horizontalDashStyle, priceColor, labelColor);
		}

		public Indicators.Customs.OpenTheDay OpenTheDay(ISeries<double> input , bool showLabelOpen, Brush horizontalLineColor, DashStyleHelper horizontalDashStyle, Brush priceColor, Brush labelColor)
		{
			return indicator.OpenTheDay(input, showLabelOpen, horizontalLineColor, horizontalDashStyle, priceColor, labelColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.OpenTheDay OpenTheDay(bool showLabelOpen, Brush horizontalLineColor, DashStyleHelper horizontalDashStyle, Brush priceColor, Brush labelColor)
		{
			return indicator.OpenTheDay(Input, showLabelOpen, horizontalLineColor, horizontalDashStyle, priceColor, labelColor);
		}

		public Indicators.Customs.OpenTheDay OpenTheDay(ISeries<double> input , bool showLabelOpen, Brush horizontalLineColor, DashStyleHelper horizontalDashStyle, Brush priceColor, Brush labelColor)
		{
			return indicator.OpenTheDay(input, showLabelOpen, horizontalLineColor, horizontalDashStyle, priceColor, labelColor);
		}
	}
}

#endregion
