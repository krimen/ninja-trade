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
	[Gui.CategoryOrder("Date", 1)]
	[Gui.CategoryOrder("Colors", 2)]
	public class GapIntraday : Indicator
	{
		private static List<NinjaTrader.Custom.Indicators.Customs.Gap> gaps;
		private int twoPreviousBarIndex = 2;
		private int widthRect = -50;
		private int _barCntr = 1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Micro gaps between bars";
				Name = "GapIntraday";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				DrawHorizontalGridLines = true;
				DrawVerticalGridLines = true;
				PaintPriceMarkers = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive = true;
				gaps = new List<NinjaTrader.Custom.Indicators.Customs.Gap>();
				LastDaySession = DateTime.Now;
				GapBullColor = Brushes.LightSkyBlue;
				GapBearColor = Brushes.Red;
				Opacity = 13;
				PrintTo = PrintTo.OutputTab1;

			}
			else if (State == State.Configure)
			{

			}

		}

		private NinjaTrader.Custom.Indicators.Customs.MyBar GetMyBar(int position = 0)
		{
			return NinjaTrader.Custom.Indicators.Customs.FactoryBar.Create(Low[position], High[position], Close[position], Open[position]);
		}

		private void CheckFilled()
		{

			List<NinjaTrader.Custom.Indicators.Customs.Gap> filled = new List<NinjaTrader.Custom.Indicators.Customs.Gap>();
			int currentBarIndex = 0;
			NinjaTrader.Custom.Indicators.Customs.MyBar currentBar = this.GetMyBar(currentBarIndex);
			List<NinjaTrader.Custom.Indicators.Customs.Gap> gapsNotFilled = gaps.ToList();


			foreach (NinjaTrader.Custom.Indicators.Customs.Gap gap in gapsNotFilled)
			{

				if (gap is NinjaTrader.Custom.Indicators.Customs.BearGap)
				{
					// if gap was closed
					if (currentBar.Close >= gap.Box.UpperPrice)
					{
						gap.State = NinjaTrader.Custom.Indicators.Customs.FillType.Closed;
						filled.Add(gap);
					}
					// if negative gap but not closed
					else if (currentBar.Close < gap.Box.UpperPrice && currentBar.High > gap.Box.LowerPrice)
					{
						int indexGapOnTheList = gaps.FindIndex(item => item.Tag == gap.Tag);
						bool founded = indexGapOnTheList != -1;
						if (founded)
						{
							gap.Box.LowerPrice = currentBar.High;
							gap.State = Custom.Indicators.Customs.FillType.Negative;
							gaps[indexGapOnTheList] = gap;
						}
					}

				}
				else if (gap is NinjaTrader.Custom.Indicators.Customs.BullGap)
				{
					// if gap was closed
					if (currentBar.Close <= gap.Box.LowerPrice)
					{
						gap.State = NinjaTrader.Custom.Indicators.Customs.FillType.Closed;
						filled.Add(gap);
					}
					else if (currentBar.Close > gap.Box.LowerPrice && currentBar.Low < gap.Box.UpperPrice)
					{
						int indexGapOnTheList = gaps.FindIndex(item => item.Tag == gap.Tag);
						bool founded = indexGapOnTheList != -1;
						if (founded)
						{
							gap.Box.UpperPrice = currentBar.Low;
							gap.State = Custom.Indicators.Customs.FillType.Negative;
							gaps[indexGapOnTheList] = gap;
						}
					}
				}

			}


			this.RemoveGaps(filled);

			List<Custom.Indicators.Customs.Gap> gapsNegatives = gaps.Where(gap => gap.State == Custom.Indicators.Customs.FillType.Negative).ToList();
			this.UpdateGaps(gapsNegatives);

		}

		private void UpdateGaps(List<Custom.Indicators.Customs.Gap> negatives)
		{
			foreach (var negative in negatives)
			{
				negative.State = NinjaTrader.Custom.Indicators.Customs.FillType.Open;

				int offSetBar = (_barCntr - negative.NumberBar) + 2; 

				if (negative is NinjaTrader.Custom.Indicators.Customs.BullGap)
				{
					Draw.Rectangle(this, negative.Tag, false, offSetBar, negative.Box.UpperPrice, negative.Box.Width, negative.Box.LowerPrice, negative.Box.BorderColor, negative.Box.BackgroundColor, negative.Box.Opacity, negative.Box.OnPricePanel); ;

				}
				else
				{
					Draw.Rectangle(this, negative.Tag, false, offSetBar, negative.Box.LowerPrice, negative.Box.Width, negative.Box.UpperPrice, negative.Box.BorderColor, negative.Box.BackgroundColor, negative.Box.Opacity, negative.Box.OnPricePanel);
				}
			}
		}

		private void RemoveGaps(List<Custom.Indicators.Customs.Gap> gapsToRemove)
		{
			gapsToRemove.ForEach(gap =>
			{
				if (DrawObjects[gap.Tag] != null)
				{
					var drawObject = DrawObjects[gap.Tag];
					RemoveDrawObject(gap.Tag);
				}

				gaps.Remove(gap);
			});
		}

		protected override void OnBarUpdate()
		{

			DateTime lastSession = LastDaySession;
			if (Time[0].Day == lastSession.Day)
			{
				if (Bars.IsFirstBarOfSession)
				{
					_barCntr = 1;
				}
				else
				{
					++_barCntr;
					//Print("Barra (" + _barCntr + ") das : " + Time[0].ToString());
				}

				if (_barCntr < 2) return;

				NinjaTrader.Custom.Indicators.Customs.MyBar currentBar = this.GetMyBar();
				NinjaTrader.Custom.Indicators.Customs.MyBar twoPreviousBar = this.GetMyBar(twoPreviousBarIndex);



				if (currentBar.Low > twoPreviousBar.High)
				{
					string tag = "gapup" + _barCntr;
					NinjaTrader.Custom.Indicators.Customs.Gap gap =
						new NinjaTrader.Custom.Indicators.Customs.BullGap(tag, twoPreviousBar.High, currentBar.Low);

					gap.Box.Width = widthRect;
					gap.Box.BorderColor = Brushes.Transparent;
					gap.Box.BackgroundColor = GapBullColor;
					gap.Box.Opacity = Opacity;
					gap.NumberBar = _barCntr;



					gaps.Add(gap);
					Draw.Rectangle(this, tag, false, twoPreviousBarIndex, currentBar.Low, gap.Box.Width, twoPreviousBar.High, gap.Box.BorderColor, gap.Box.BackgroundColor, gap.Box.Opacity, gap.Box.OnPricePanel);
				}

				else if (currentBar.High < twoPreviousBar.Low)
				{
					string tag = "gapdown" + _barCntr;

					NinjaTrader.Custom.Indicators.Customs.Gap gap =
						new NinjaTrader.Custom.Indicators.Customs.BearGap(tag, currentBar.High, twoPreviousBar.Low);

					gap.Box.Width = widthRect;
					gap.Box.BorderColor = Brushes.Transparent;
					gap.Box.BackgroundColor = GapBearColor;
					gap.Box.Opacity = Opacity;
					gap.NumberBar = _barCntr;
					gaps.Add(gap);


					Draw.Rectangle(this, tag, false, twoPreviousBarIndex, twoPreviousBar.Low, gap.Box.Width, currentBar.High, gap.Box.BorderColor, gap.Box.BackgroundColor, gap.Box.Opacity, gap.Box.OnPricePanel);
				}


				CheckFilled();
			}

		}


		//checks for gap between current candle and 2 previous candle e.g. low of current candle and high of the candle before last, this is the fair value gap.
		private void GapLogic(NinjaTrader.Custom.Indicators.Customs.MyBar currentCandle, NinjaTrader.Custom.Indicators.Customs.MyBar twoPreviosCandle)
		{
			if (currentCandle is NinjaTrader.Custom.Indicators.Customs.BullBar)
			{
				if (currentCandle.High - twoPreviosCandle.Low < 0)
				{
					double upperLimit = currentCandle.Close - (currentCandle.Close - twoPreviosCandle.Low);
					double lowerLimit = currentCandle.Close - (currentCandle.Close - currentCandle.High);
					double midlimit = (upperLimit + lowerLimit) / 2;
				}
			}
			else
			{
				if (currentCandle.Low - twoPreviosCandle.High > 0)
				{
					double upperLimit = currentCandle.Close - (currentCandle.Close - currentCandle.Low);
					double lowerLimit = currentCandle.Close - (currentCandle.Close - twoPreviosCandle.High);
					double midlimit = (upperLimit + lowerLimit) / 2;
				}
			}

		}

		#region Properties

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Last Day Session", Description = "Last Day Session ", Order = 1, GroupName = "Date")]
		public DateTime LastDaySession
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Gap Bull Color", Description = "Gap for bull bars", Order = 2, GroupName = "Colors")]
		public Brush GapBullColor
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Gap Bear Color", Description = "Gap for bull bars", Order = 3, GroupName = "Colors")]
		public Brush GapBearColor
		{
			get;
			set;
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Opacity", Description = "Opacity for Gap Color", Order = 4, GroupName = "Colors")]
		[Range(0, 100)]
		public byte Opacity
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
		private Customs.GapIntraday[] cacheGapIntraday;
		public Customs.GapIntraday GapIntraday(DateTime lastDaySession, Brush gapBullColor, Brush gapBearColor, byte opacity)
		{
			return GapIntraday(Input, lastDaySession, gapBullColor, gapBearColor, opacity);
		}

		public Customs.GapIntraday GapIntraday(ISeries<double> input, DateTime lastDaySession, Brush gapBullColor, Brush gapBearColor, byte opacity)
		{
			if (cacheGapIntraday != null)
				for (int idx = 0; idx < cacheGapIntraday.Length; idx++)
					if (cacheGapIntraday[idx] != null && cacheGapIntraday[idx].LastDaySession == lastDaySession && cacheGapIntraday[idx].GapBullColor == gapBullColor && cacheGapIntraday[idx].GapBearColor == gapBearColor && cacheGapIntraday[idx].Opacity == opacity && cacheGapIntraday[idx].EqualsInput(input))
						return cacheGapIntraday[idx];
			return CacheIndicator<Customs.GapIntraday>(new Customs.GapIntraday(){ LastDaySession = lastDaySession, GapBullColor = gapBullColor, GapBearColor = gapBearColor, Opacity = opacity }, input, ref cacheGapIntraday);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.GapIntraday GapIntraday(DateTime lastDaySession, Brush gapBullColor, Brush gapBearColor, byte opacity)
		{
			return indicator.GapIntraday(Input, lastDaySession, gapBullColor, gapBearColor, opacity);
		}

		public Indicators.Customs.GapIntraday GapIntraday(ISeries<double> input , DateTime lastDaySession, Brush gapBullColor, Brush gapBearColor, byte opacity)
		{
			return indicator.GapIntraday(input, lastDaySession, gapBullColor, gapBearColor, opacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.GapIntraday GapIntraday(DateTime lastDaySession, Brush gapBullColor, Brush gapBearColor, byte opacity)
		{
			return indicator.GapIntraday(Input, lastDaySession, gapBullColor, gapBearColor, opacity);
		}

		public Indicators.Customs.GapIntraday GapIntraday(ISeries<double> input , DateTime lastDaySession, Brush gapBullColor, Brush gapBearColor, byte opacity)
		{
			return indicator.GapIntraday(input, lastDaySession, gapBullColor, gapBearColor, opacity);
		}
	}
}

#endregion
