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
                Description = @"Draw Horizontal line on the low and high of day";
                Name = "LodHod";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                LodHorizontalLineColor = Brushes.Red;
                HodHorizontalLineColor = Brushes.Blue;
                LowHorizontalDashStyle = DashStyleHelper.Dash;
                HighHorizontalDashStyle = LowHorizontalDashStyle;
                LowWidth = 1;
                HighWidth = LowWidth;
                Lod = 0;
                Hod = 0;
                ShowFrequencyMarket = true;
                AverageDays = 10;
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

            if (currentLow == Lod || currentHigh == Hod)
            {
                if (ShowFrequencyMarket)
                {

                    NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 14) { Bold = true };

                    double range = (Hod - Lod);
                    double frenquency = (range * 0.10);
                    double scalping = frenquency / 2;
                    double goal = range * 0.30;

                    // Obtém dados do dia anterior usando PriorDayOHLC
                    string variation = "";
                    string avgRangeNDays = "";

                    try
                    {
                        if (PriorDayOHLC().PriorClose[0] != 0)
                        {
                            double priorClose = PriorDayOHLC().PriorClose[0];
                            double currentPrice = Close[0];

                            // Calcula variação baseada no fechamento anterior vs preço atual
                            double priceChange = currentPrice - priorClose;
                            double priceChangePercent = (priceChange / priorClose) * 100;

                            variation = $"Change = {(priceChangePercent >= 0 ? "+" : "")}{priceChangePercent.ToString("0.##")}%\n";
                        }

                        // Calcula média do range dos últimos N dias configurável
                        if (CurrentBar >= AverageDays)
                        {
                            double totalRange = 0;
                            int validDays = 0;

                            // Método simplificado usando PriorDayOHLC para cálculo preciso
                            try
                            {
                                // Usa PriorDayOHLC para o dia anterior (mais confiável)
                                if (PriorDayOHLC().PriorHigh[0] != 0 && PriorDayOHLC().PriorLow[0] != 0)
                                {
                                    double priorRange = PriorDayOHLC().PriorHigh[0] - PriorDayOHLC().PriorLow[0];
                                    totalRange += priorRange;
                                    validDays++;
                                }

                                // Para os outros dias, usa aproximação baseada em sessões
                                int daysFound = 1; // Já temos um dia (anterior)
                                for (int i = 1; i <= CurrentBar && daysFound < AverageDays; i++)
                                {
                                    if (Bars.IsFirstBarOfSessionByIndex(CurrentBar - i))
                                    {
                                        // Encontra o range aproximado do dia usando dados disponíveis
                                        double sessionHigh = High[i];
                                        double sessionLow = Low[i];

                                        // Procura por mais barras da mesma sessão para encontrar HOD/LOD
                                        for (int j = i; j <= CurrentBar && j <= i + 390; j++) // Máximo ~6.5h de trade
                                        {
                                            if (j < CurrentBar && Bars.IsFirstBarOfSessionByIndex(CurrentBar - j - 1))
                                                break; // Próxima sessão encontrada

                                            if (High[j] > sessionHigh) sessionHigh = High[j];
                                            if (Low[j] < sessionLow) sessionLow = Low[j];
                                        }

                                        double dayRange = sessionHigh - sessionLow;
                                        if (dayRange > 0) // Validação básica
                                        {
                                            totalRange += dayRange;
                                            validDays++;
                                            daysFound++;
                                        }
                                    }
                                }

                                if (validDays > 0)
                                {
                                    double avgRange = totalRange / validDays;
                                    avgRangeNDays = $"Avg Range ({AverageDays}) =  {avgRange.ToString("0.##")}\n";
                                }
                            }
                            catch (Exception ex2)
                            {
                                // Fallback se houver erro
                                avgRangeNDays = $"Avg Range ({AverageDays}): N/A\n";
                                Print("Erro no cálculo da média: " + ex2.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Se houver erro ao obter dados do dia anterior, continua sem variação
                        Print("Erro ao obter dados do dia anterior: " + ex.Message);
                    }

                    string lowLabel = $"Low = {Lod}\n";
                    string highLabel = $"High = {Hod}\n";
                    string rangeLabel = $"Range = {range}\n";
                    string frenquencyLabel = $"Frequency = {frenquency.ToString("#.##")}\n";
                    string goalScalping = $"Scalp Value = {scalping.ToString("#.##")}\n";
                    string goalofDay = $"Goal of day = {goal.ToString("#.##")}";

                    string labelCompleted = lowLabel + highLabel + rangeLabel + variation + avgRangeNDays + frenquencyLabel + goalScalping + goalofDay;

                    Draw.TextFixed(this, "RangeLabel", labelCompleted, TextPosition.TopRight, Brushes.Green, font, Brushes.Transparent, Brushes.Transparent, 1);

                }
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

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Show Frequency Market", Description = "Show Frenquency Market on Chart", Order = 5, GroupName = "Width")]
        public bool ShowFrequencyMarket
        {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "Average Days", Description = "Number of days to calculate range average", Order = 6, GroupName = "Width")]
        [Range(1, 50)]
        public int AverageDays
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
		public Customs.LodHod LodHod(Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth, bool showFrequencyMarket, int averageDays)
		{
			return LodHod(Input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth, showFrequencyMarket, averageDays);
		}

		public Customs.LodHod LodHod(ISeries<double> input, Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth, bool showFrequencyMarket, int averageDays)
		{
			if (cacheLodHod != null)
				for (int idx = 0; idx < cacheLodHod.Length; idx++)
					if (cacheLodHod[idx] != null && cacheLodHod[idx].LodHorizontalLineColor == lodHorizontalLineColor && cacheLodHod[idx].HodHorizontalLineColor == hodHorizontalLineColor && cacheLodHod[idx].LowHorizontalDashStyle == lowHorizontalDashStyle && cacheLodHod[idx].HighHorizontalDashStyle == highHorizontalDashStyle && cacheLodHod[idx].LowWidth == lowWidth && cacheLodHod[idx].HighWidth == highWidth && cacheLodHod[idx].ShowFrequencyMarket == showFrequencyMarket && cacheLodHod[idx].AverageDays == averageDays && cacheLodHod[idx].EqualsInput(input))
						return cacheLodHod[idx];
			return CacheIndicator<Customs.LodHod>(new Customs.LodHod(){ LodHorizontalLineColor = lodHorizontalLineColor, HodHorizontalLineColor = hodHorizontalLineColor, LowHorizontalDashStyle = lowHorizontalDashStyle, HighHorizontalDashStyle = highHorizontalDashStyle, LowWidth = lowWidth, HighWidth = highWidth, ShowFrequencyMarket = showFrequencyMarket, AverageDays = averageDays }, input, ref cacheLodHod);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.LodHod LodHod(Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth, bool showFrequencyMarket, int averageDays)
		{
			return indicator.LodHod(Input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth, showFrequencyMarket, averageDays);
		}

		public Indicators.Customs.LodHod LodHod(ISeries<double> input , Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth, bool showFrequencyMarket, int averageDays)
		{
			return indicator.LodHod(input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth, showFrequencyMarket, averageDays);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.LodHod LodHod(Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth, bool showFrequencyMarket, int averageDays)
		{
			return indicator.LodHod(Input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth, showFrequencyMarket, averageDays);
		}

		public Indicators.Customs.LodHod LodHod(ISeries<double> input , Brush lodHorizontalLineColor, Brush hodHorizontalLineColor, DashStyleHelper lowHorizontalDashStyle, DashStyleHelper highHorizontalDashStyle, byte lowWidth, byte highWidth, bool showFrequencyMarket, int averageDays)
		{
			return indicator.LodHod(input, lodHorizontalLineColor, hodHorizontalLineColor, lowHorizontalDashStyle, highHorizontalDashStyle, lowWidth, highWidth, showFrequencyMarket, averageDays);
		}
	}
}

#endregion
