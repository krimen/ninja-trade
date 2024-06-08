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
	[Gui.CategoryOrder("Colors", 1)]
	[Gui.CategoryOrder("Width", 2)]
	public class LodHod : Indicator
	{
        private double Lod 
		{
			get; 
			set;
		}

        private double Hod 
		{
			get;
			set;
		}
		
		private string TagLodLine => "LowOfDayLine";
		private string TagHodLine => "HighOfDayLine";
		
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Draw Horizontal line on the low and high of day";
				Name										= "LodHod";
				Calculate = Calculate.OnEachTick;
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
				LodHorizontalLineColor						= Brushes.Black;
				HodHorizontalLineColor						= Brushes.Black; 
				LowHorizontalDashStyle 						= DashStyleHelper.Solid;
				HighHorizontalDashStyle 					= LowHorizontalDashStyle;
				LowWidth 									= 1;
				HighWidth 									= LowWidth;
				Lod											= 0;
				Hod											= 0;
				PrintTo = PrintTo.OutputTab1;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1) return;

			double currentLow = Low[0];
			double currentHigh = High[0];

			if (Bars.IsFirstBarOfSession)
            {
				Lod = currentLow;
				Hod = currentHigh;
				Draw.Line(this, TagLodLine, false, 0, Lod, -100, Lod, LodHorizontalLineColor, LowHorizontalDashStyle, LowWidth);
				Draw.Line(this, TagHodLine, false, 0, Hod, -100, Hod, HodHorizontalLineColor, HighHorizontalDashStyle, HighWidth);
			}

			if (currentLow < Lod)
			{
				Lod = currentLow;
				Draw.Line(this, TagLodLine, false, 0, Lod, -100, Lod, LodHorizontalLineColor, LowHorizontalDashStyle, LowWidth);
				
			}

			if (currentHigh > Hod)
			{
				Hod = currentHigh;
				Draw.Line(this, TagHodLine, false, 0, Hod, -100, Hod, HodHorizontalLineColor, HighHorizontalDashStyle, HighWidth);
			}
			
			if(currentLow == Lod || currentHigh == Hod) 
			{
				NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 14) { Bold = true };
				
				double range  	  = (Hod - Lod);
				double frenquency = (range*0.10);
				double scalping   = frenquency / 2;
				double goal 	  = range * 0.30;
				string lowLabel   = $"Low = {Lod}\n" ;
				string highLabel  = $"High = {Hod}\n";
				string rangeLabel = $"Range = {range}\n";
				string frenquencyLabel = $"Frequency = {frenquency.ToString("#.##")}\n";
				string goalScalping = $"Scalp Value = {scalping.ToString("#.##")}\n";
				string goalofDay = $"Goal of day = {goal.ToString("#.##")}";
				
				string labelCompleted = lowLabel + highLabel + rangeLabel + frenquencyLabel + goalScalping + goalofDay;
				
				Draw.TextFixed(this, "RangeLabel", labelCompleted, TextPosition.TopRight, Brushes.Green, font, Brushes.Transparent, Brushes.Transparent, 1);
			}
			

//			Print("LOW (" + Lod + ") das : " + Time[0].ToString());
//			Print("HIGH (" + Hod + ") das : " + Time[0].ToString());

			
		}

		#region Properties

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Low of day Color", Description = "Horizontal Line on the Low", Order = 1, GroupName = "Colors")]
		public Brush LodHorizontalLineColor
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "High of day Color", Description = "Horizontal Line on the High", Order = 2, GroupName = "Colors")]
		public Brush HodHorizontalLineColor
		{
			get;
			set;
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Low Dash Style", Description = "Style used by line horizontal", Order = 3, GroupName = "Colors")]
		public DashStyleHelper LowHorizontalDashStyle
		{
			get;
			set;
		}
		
		[NinjaScriptProperty]
		[Display(Name = "High Dash Style", Description = "Style used by line horizontal", Order = 4, GroupName = "Colors")]
		public DashStyleHelper HighHorizontalDashStyle
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Low Width", Description = "Width Horizontal Line on The Low", Order = 3, GroupName = "Width")]
		[Range(1, 100)]
		public byte LowWidth
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "High Width", Description = "Width Horizontal Line on The Low", Order = 4, GroupName = "Width")]
		[Range(1, 100)]
		public byte HighWidth
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
		private Customs.LodHod[] cacheLodHod;
		public Customs.LodHod LodHod(Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth)
		{
			return LodHod(Input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth);
		}

		public Customs.LodHod LodHod(ISeries<double> input, Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth)
		{
			if (cacheLodHod != null)
				for (int idx = 0; idx < cacheLodHod.Length; idx++)
					if (cacheLodHod[idx] != null && cacheLodHod[idx].LodHorizontalLineColor == lodHorizontalLineColor && cacheLodHod[idx].HodHorizontalLineColor == hodHorizontalLineColor && cacheLodHod[idx].LowHorizontalDashStyle == lowHorizontalDashStyle && cacheLodHod[idx].HighHorizontalDashStyle == highHorizontalDashStyle && cacheLodHod[idx].LowWidth == lowWidth && cacheLodHod[idx].HighWidth == highWidth && cacheLodHod[idx].EqualsInput(input))
						return cacheLodHod[idx];
			return CacheIndicator<Customs.LodHod>(new Customs.LodHod(){ LodHorizontalLineColor = lodHorizontalLineColor, HodHorizontalLineColor = hodHorizontalLineColor, LowHorizontalDashStyle = lowHorizontalDashStyle, HighHorizontalDashStyle = highHorizontalDashStyle, LowWidth = lowWidth, HighWidth = highWidth }, input, ref cacheLodHod);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.LodHod LodHod(Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth)
		{
			return indicator.LodHod(Input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth);
		}

		public Indicators.Customs.LodHod LodHod(ISeries<double> input , Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth)
		{
			return indicator.LodHod(input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.LodHod LodHod(Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth)
		{
			return indicator.LodHod(Input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth);
		}

		public Indicators.Customs.LodHod LodHod(ISeries<double> input , Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth)
		{
			return indicator.LodHod(input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth);
		}
	}
}

#endregion
