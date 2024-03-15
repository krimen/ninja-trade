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
	public class InsideOutsideBar : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "InsideOutsideBar";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			int currentBarPosition = 0;
			int previousBarPosition = 1;
			SolidColorBrush? color = null;

			if (CurrentBar < 1) return;
			
			NinjaTrader.Custom.Indicators.Customs.MyBar currentBar = GetMyBar(currentBarPosition);
			NinjaTrader.Custom.Indicators.Customs.MyBar previousBar = GetMyBar(previousBarPosition);
			
			

			this.DrawLabelFor(currentBar, previousBar);
			this.UpdateCandleOutlineBrushs(currentBar, previousBar);
			
		}
		
		private void DrawLabelFor(NinjaTrader.Custom.Indicators.Customs.MyBar currentBar, NinjaTrader.Custom.Indicators.Customs.MyBar previousBar)
		{
			string descriptionOfBar = currentBar.ToString(previousBar);
			if(descriptionOfBar != string.Empty)
			{
				NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 14) { Size = 14, Bold = true };
				Draw.Text(this, "tag"+ CurrentBar.ToString(), true, descriptionOfBar,0,  currentBar.High, 10, Brushes.Blue, font, TextAlignment.Center, null, null, 1);
			}
		}
		
		private void UpdateCandleOutlineBrushs(NinjaTrader.Custom.Indicators.Customs.MyBar currentBar, NinjaTrader.Custom.Indicators.Customs.MyBar previousBar)
		{
			if(currentBar.GapOnOpen(previousBar))
			{
				//CandleOutlineBrush = Brushes.Blue;
			}
		}

		private NinjaTrader.Custom.Indicators.Customs.MyBar GetMyBar(int position = 0)
		{
			return NinjaTrader.Custom.Indicators.Customs.FactoryBar.Create(Low[position], High[position], Close[position], Open[position]);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Customs.InsideOutsideBar[] cacheInsideOutsideBar;
		public Customs.InsideOutsideBar InsideOutsideBar()
		{
			return InsideOutsideBar(Input);
		}

		public Customs.InsideOutsideBar InsideOutsideBar(ISeries<double> input)
		{
			if (cacheInsideOutsideBar != null)
				for (int idx = 0; idx < cacheInsideOutsideBar.Length; idx++)
					if (cacheInsideOutsideBar[idx] != null &&  cacheInsideOutsideBar[idx].EqualsInput(input))
						return cacheInsideOutsideBar[idx];
			return CacheIndicator<Customs.InsideOutsideBar>(new Customs.InsideOutsideBar(), input, ref cacheInsideOutsideBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.InsideOutsideBar InsideOutsideBar()
		{
			return indicator.InsideOutsideBar(Input);
		}

		public Indicators.Customs.InsideOutsideBar InsideOutsideBar(ISeries<double> input )
		{
			return indicator.InsideOutsideBar(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.InsideOutsideBar InsideOutsideBar()
		{
			return indicator.InsideOutsideBar(Input);
		}

		public Indicators.Customs.InsideOutsideBar InsideOutsideBar(ISeries<double> input )
		{
			return indicator.InsideOutsideBar(input);
		}
	}
}

#endregion
