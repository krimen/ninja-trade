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
    public class EarlyLowHighAnalysis18Bars : Indicator
    {
        private int barsFromSessionStart = 0;
        private double sessionOpen = 0;
        private double sessionLow = 0;
        private double sessionHigh = 0;
        private bool minFormedInFirst18Bars = false;
        private bool maxFormedInFirst18Bars = false;

        // SessionIterator para controlar sessões regulares apenas
        private SessionIterator sessionIterator;

        // Flag para controlar se o histórico já foi processado
        private bool historicalProcessed = false;

        // Contadores para estatísticas - ALGORITMO SIMPLIFICADO
        private List<bool> bullishDaysHistory = new List<bool>();
        private List<bool> bearishDaysHistory = new List<bool>();
        private List<bool> earlyLowHistory = new List<bool>();
        private List<bool> earlyHighHistory = new List<bool>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Analyzes how often lows/highs are formed in the first N bars on bullish/bearish days";
                Name = "EarlyLowHighAnalysis18Bars";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 1;  // Reduzido para aparecer mais cedo

                // Propriedades padrão - ALGORITMO SIMPLIFICADO
                AnalysisDays = 4;  // Padrão 4 dias, mas usuário pode alterar
                EarlyBarsLimit = 18; // Bull: após 18ª barra
                BearEarlyBarsLimit = 19; // Bear: após 19ª barra  
                RegularSessionOnly = true;
                ShowStatistics = true;
                StatsFontSize = 12;
                StatsTextColor = Brushes.Yellow;
                PrintTo = PrintTo.OutputTab1;
            }
            else if (State == State.Configure)
            {
                // DINÂMICO: Calcula baseado em AnalysisDays
                // Cada dia da sessão regular US = ~81 barras (9:30-16:00 em 5min)
                int barrasNecessarias = AnalysisDays * 81 + 50; // Margem de segurança
                BarsRequiredToPlot = Math.Min(barrasNecessarias, 1500); // Limita a 1500 para não sobrecarregar

                Print($"Configurando para {AnalysisDays} dias = {barrasNecessarias} barras (limitado a {BarsRequiredToPlot})");
                Print($"Cálculo: {AnalysisDays} dias × 81 barras + 50 margem = {barrasNecessarias} barras");
            }
            else if (State == State.DataLoaded)
            {
                // Inicializa o SessionIterator para sessão regular apenas
                sessionIterator = new SessionIterator(Bars);

                // Limpa históricos ao carregar dados para evitar problemas
                bullishDaysHistory.Clear();
                bearishDaysHistory.Clear();
                earlyLowHistory.Clear();
                earlyHighHistory.Clear();

                string sessionTypeMsg = RegularSessionOnly ? "APENAS SESSÃO REGULAR" : "TODAS AS SESSÕES";
                Print($"Históricos limpos ao carregar dados - processamento será em tempo real - {sessionTypeMsg}");
                Print($"Processamento histórico será executado quando houver dados suficientes...");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1) return;

            // DINÂMICO: Calcula baseado em AnalysisDays
            // Precisa de pelo menos 80% das barras necessárias antes de processar
            int barrasIdeais = AnalysisDays * 81;
            int barrasMinimas = Math.Max(100, (int)(barrasIdeais * 0.8)); // Mínimo 80% do ideal, nunca menos que 100
            if (!historicalProcessed && CurrentBar > barrasMinimas)
            {
                Print($"Executando processamento histórico agora com {CurrentBar + 1} barras disponíveis (mínimo: {barrasMinimas})...");
                ProcessHistoricalSessions();
                historicalProcessed = true;
            }
            else if (!historicalProcessed && CurrentBar <= barrasMinimas)
            {
                // Debug para mostrar quantas barras temos
                if (CurrentBar % 50 == 0)
                {
                    Print($"Aguardando mais dados... Atual: {CurrentBar + 1} barras (mínimo: {barrasMinimas})");
                }
            }

            if (Bars.IsFirstBarOfSession)
            {
                // DEBUG: Mostra quando nova sessao e detectada
                if (historicalProcessed)
                {
                    Print($"NOVA SESSAO detectada na barra {CurrentBar + 1} - {Time[0]:dd/MM/yyyy HH:mm}");
                    if (sessionOpen != 0)
                    {
                        Print($"   Sessao anterior tinha {barsFromSessionStart} barras");
                    }
                }
                // Analisa a sessão anterior se existir E se já processou o histórico
                if (sessionOpen != 0 && barsFromSessionStart > 0 && historicalProcessed)
                {
                    // Usa o close da última barra da sessão anterior (barra anterior à atual)
                    double lastSessionClose = Close[1]; // Barra anterior
                    bool wasBullishDay = lastSessionClose > sessionOpen;
                    bool wasBearishDay = lastSessionClose < sessionOpen;
                    bool hadEarlyLow = minFormedInFirst18Bars;
                    bool hadEarlyHigh = maxFormedInFirst18Bars;

                    DateTime sessionDate = Time[1].Date;
                    string dayOfWeek = sessionDate.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));

                    Print($"Sessão finalizada TEMPO REAL: {sessionDate:dd/MM/yyyy} ({dayOfWeek}) - Open={sessionOpen:F2}, Close={lastSessionClose:F2}, Tipo={(wasBullishDay ? "Bullish" : (wasBearishDay ? "Bearish" : "Doji"))}");
                    Print($"Early Low: {hadEarlyLow}, Early High: {hadEarlyHigh}, Barras da sessão: {barsFromSessionStart}");

                    // Adiciona ao histórico
                    if (wasBullishDay)
                    {
                        bullishDaysHistory.Add(true);
                        earlyLowHistory.Add(hadEarlyLow);
                        Print($"+ Bullish TEMPO REAL adicionado. Total bullish: {bullishDaysHistory.Count}");
                    }
                    else if (wasBearishDay)
                    {
                        bearishDaysHistory.Add(true);
                        earlyHighHistory.Add(hadEarlyHigh);
                        Print($"+ Bearish TEMPO REAL adicionado. Total bearish: {bearishDaysHistory.Count}");
                    }

                    // Mantém apenas os últimos N dias TOTAL (histórico + tempo real)
                    int totalDays = bullishDaysHistory.Count + bearishDaysHistory.Count;

                    while (totalDays > AnalysisDays)
                    {
                        // Remove sempre o mais antigo (primeiro da lista)
                        if (bullishDaysHistory.Count > 0 && (bearishDaysHistory.Count == 0 || bullishDaysHistory.Count >= bearishDaysHistory.Count))
                        {
                            bullishDaysHistory.RemoveAt(0);
                            earlyLowHistory.RemoveAt(0);
                            Print($">>> Removido dia bullish mais antigo. Restam {bullishDaysHistory.Count} bullish");
                        }
                        else if (bearishDaysHistory.Count > 0)
                        {
                            bearishDaysHistory.RemoveAt(0);
                            earlyHighHistory.RemoveAt(0);
                            Print($">>> Removido dia bearish mais antigo. Restam {bearishDaysHistory.Count} bearish");
                        }
                        totalDays = bullishDaysHistory.Count + bearishDaysHistory.Count;
                        Print($">>> Total de dias após limpeza: {totalDays}");
                    }
                }

                // Reset para nova sessão
                barsFromSessionStart = 1; // Começa em 1 porque já estamos na primeira barra da sessão
                sessionOpen = Open[0];
                sessionLow = Low[0];
                sessionHigh = High[0];
                minFormedInFirst18Bars = true;
                maxFormedInFirst18Bars = true;

                if (historicalProcessed)
                {
                    Print($"Nova sessão iniciada: Open={sessionOpen:F2} em {Time[0]:dd/MM/yyyy HH:mm} - Barra da sessão: {barsFromSessionStart}, Barra global: {CurrentBar + 1}");
                }
            }
            else
            {
                barsFromSessionStart++;
            }

            // Verifica se nova mínima foi formada
            if (Low[0] < sessionLow)
            {
                sessionLow = Low[0];
                if (barsFromSessionStart > EarlyBarsLimit)
                {
                    minFormedInFirst18Bars = false;
                }
            }

            // Verifica se nova máxima foi formada
            if (High[0] > sessionHigh)
            {
                sessionHigh = High[0];
                if (barsFromSessionStart > BearEarlyBarsLimit)  // Usa BearEarlyBarsLimit para máximas
                {
                    maxFormedInFirst18Bars = false;
                }
            }

            // Debug da contagem de barras a cada 10 barras
            if (barsFromSessionStart % 10 == 0 && historicalProcessed)
            {
                Print($"Check: Barra da sessao {barsFromSessionStart} (barra global {CurrentBar + 1}) - Low: {sessionLow:F2}, High: {sessionHigh:F2} - {Time[0]:HH:mm}");
            }

            // Debug extra para rastrear o problema
            if (barsFromSessionStart > 85 && historicalProcessed)
            {
                Print($"ATENCAO: Sessao com {barsFromSessionStart} barras! Data: {Time[0]:dd/MM/yyyy HH:mm}");
            }

            // Exibe estatísticas
            if (ShowStatistics && (bullishDaysHistory.Count > 0 || bearishDaysHistory.Count > 0))
            {
                if (barsFromSessionStart % 20 == 0 || barsFromSessionStart == 1)
                {
                    DisplayStatistics();
                }
            }
        }

        private bool IsInRegularSession()
        {
            // Abordagem mais simples e confiável
            // Se o NinjaTrader está configurado para mostrar apenas dados RTH (Regular Trading Hours),
            // então todas as barras já são filtradas
            // Caso contrário, podemos fazer uma verificação básica de horário

            // Para a maioria dos casos, se você configurar seu gráfico para RTH apenas,
            // não precisa desta verificação. Mas mantemos por segurança.

            try
            {
                // Verifica se estamos usando dados RTH ou ETH
                // Se o instrumento tem sessão definida, usa ela
                if (Instrument != null)
                {
                    // Simplesmente retorna true - assumindo que o usuário configurou
                    // o template de sessão corretamente para RTH apenas
                    return true;
                }

                return true;
            }
            catch
            {
                return true; // Em caso de erro, processa normalmente
            }
        }

        private void ProcessHistoricalSessions()
        {
            try
            {
                Print($"ALGORITMO SIMPLIFICADO - Analisando ultimos {AnalysisDays} DIAS UTEIS");
                Print($"Horario de analise: 09:30-16:00 (Sessao Regular US) | Bull<={EarlyBarsLimit} | Bear<={BearEarlyBarsLimit}");
                Print($"FILTRO: Apenas Segunda a Sexta (pula fins de semana)");

                // DINÂMICO: Verifica se temos dados suficientes baseado em AnalysisDays
                int barrasIdeais = AnalysisDays * 81;
                int barrasMinimas = Math.Max(50, (int)(barrasIdeais * 0.6)); // Mínimo 60% do ideal

                Print($"Barras necessárias: {barrasIdeais} ideais, {barrasMinimas} mínimas");

                if (CurrentBar < barrasMinimas)
                {
                    Print($"ATENÇÃO: Dados insuficientes! Barras disponíveis: {CurrentBar + 1}, Mínimo necessário: {barrasMinimas}");
                    Print($"Para {AnalysisDays} dias é recomendado ter pelo menos {barrasIdeais} barras históricas.");
                    Print($"Continuando mesmo assim, mas resultados podem ser limitados...");
                }

                // USA DATA REAL DO SISTEMA, não da barra
                DateTime hoje = DateTime.Now.Date;
                Print($"DEBUG: Data atual do sistema: {hoje:dd/MM/yyyy} ({hoje.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"))})");

                // PRIMEIRO: Descobre quais datas temos disponíveis nos dados históricos
                HashSet<DateTime> datasDisponiveis = new HashSet<DateTime>();
                DateTime dataMin = DateTime.MaxValue;
                DateTime dataMax = DateTime.MinValue;

                for (int i = CurrentBar; i >= 0; i--)
                {
                    DateTime dataBarr = Time[i].Date;
                    datasDisponiveis.Add(dataBarr);
                    if (dataBarr < dataMin) dataMin = dataBarr;
                    if (dataBarr > dataMax) dataMax = dataBarr;
                }

                Print($"DADOS HISTORICOS DISPONIVEIS: {dataMin:dd/MM/yyyy} até {dataMax:dd/MM/yyyy} ({datasDisponiveis.Count} dias únicos)");

                // Lista as últimas 10 datas para debug
                var ultimasDatas = datasDisponiveis.OrderByDescending(x => x).Take(10);
                Print("Ultimas 10 datas nos dados:");
                foreach (var data in ultimasDatas)
                {
                    string nome = data.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                    Print($"  -> {data:dd/MM/yyyy} ({nome})");
                }

                // AGORA: Pega os últimos N DIAS ÚTEIS que realmente EXISTEM nos dados
                List<DateTime> diasParaAnalisar = new List<DateTime>();
                DateTime currentDate = hoje.AddDays(-1); // Começa de ONTEM
                int diasAdicionados = 0;
                int tentativas = 0;
                int maxTentativas = 30; // Evita loop infinito

                Print($"DEBUG: Procurando {AnalysisDays} dias úteis nos dados históricos...");

                while (diasAdicionados < AnalysisDays && tentativas < maxTentativas)
                {
                    tentativas++;

                    // Verifica se é dia útil E se temos dados para este dia
                    if (currentDate.DayOfWeek >= DayOfWeek.Monday && currentDate.DayOfWeek <= DayOfWeek.Friday)
                    {
                        if (datasDisponiveis.Contains(currentDate))
                        {
                            diasParaAnalisar.Add(currentDate);
                            diasAdicionados++;
                            Print($"DEBUG: Dia util #{diasAdicionados}: {currentDate:dd/MM/yyyy} ({currentDate.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"))}) - TEM DADOS");
                        }
                        else
                        {
                            Print($"DEBUG: {currentDate:dd/MM/yyyy} ({currentDate.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"))}) - SEM DADOS HISTORICOS");
                        }
                    }
                    else
                    {
                        Print($"DEBUG: Pulando fim de semana: {currentDate:dd/MM/yyyy} ({currentDate.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"))})");
                    }

                    currentDate = currentDate.AddDays(-1); // Volta um dia
                }

                if (diasAdicionados < AnalysisDays)
                {
                    Print($"AVISO: Solicitados {AnalysisDays} dias, mas apenas {diasAdicionados} disponíveis nos dados históricos!");
                }

                // Ordena do mais antigo para o mais novo para processamento cronológico
                diasParaAnalisar.Reverse();

                Print($"{AnalysisDays} DIAS UTEIS selecionados para analise (do mais antigo ao mais novo):");
                foreach (var dia in diasParaAnalisar)
                {
                    string nome = dia.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                    Print($"  -> {dia:dd/MM/yyyy} ({nome})");
                }

                foreach (var diaAnalise in diasParaAnalisar)
                {
                    AnalyseSingleDay(diaAnalise);
                }

                int totalDias = bullishDaysHistory.Count + bearishDaysHistory.Count;
                Print($"RESULTADO: {totalDias} dias analisados ({bullishDaysHistory.Count} bulls, {bearishDaysHistory.Count} bears)");

            }
            catch (Exception ex)
            {
                Print($"Erro: {ex.Message}");
            }
        }

        private void AnalyseSingleDay(DateTime targetDate)
        {
            try
            {
                Print($"\n=== ANALISANDO DIA {targetDate:dd/MM/yyyy} ===");

                // PRIMEIRO: Conta TODAS as barras deste dia (sem filtro de horário)
                List<int> allDayBars = new List<int>();
                for (int i = CurrentBar; i >= 0; i--)
                {
                    if (Time[i].Date == targetDate)
                    {
                        allDayBars.Add(i);
                    }
                }

                Print($"Total de barras do dia {targetDate:dd/MM/yyyy}: {allDayBars.Count}");

                if (allDayBars.Count == 0)
                {
                    Print($"{targetDate:dd/MM/yyyy}: NENHUMA barra encontrada para este dia!");
                    return;
                }

                // Mostra horários das primeiras e últimas barras para debug
                allDayBars = allDayBars.OrderByDescending(x => x).ToList(); // Ordem cronológica
                Print($"Primeira barra: {Time[allDayBars[0]]:HH:mm:ss} - Última barra: {Time[allDayBars.Last()]:HH:mm:ss}");

                // AGORA: Filtra apenas o range 09:30-16:00 (Sessão Regular US)
                List<int> dayBars = new List<int>();

                for (int i = CurrentBar; i >= 0; i--)
                {
                    if (Time[i].Date == targetDate)
                    {
                        TimeSpan horario = Time[i].TimeOfDay;
                        // 09:30 até 16:00 (Sessão Regular Americana)
                        if (horario >= new TimeSpan(9, 30, 0) && horario <= new TimeSpan(16, 0, 0))
                        {
                            dayBars.Add(i);
                        }
                    }
                }

                Print($"Barras no range 09:30-16:00: {dayBars.Count}");

                // Relaxa o critério: mínimo 5 barras em vez de 20
                if (dayBars.Count < 5)
                {
                    Print($"{targetDate:dd/MM/yyyy}: Poucas barras no range 09:30-16:00 ({dayBars.Count}) - Precisa mínimo 5");

                    // Debug: mostra algumas barras do dia para entender o horário
                    Print("Primeiras 10 barras do dia (com horários):");
                    for (int i = 0; i < Math.Min(10, allDayBars.Count); i++)
                    {
                        int barIdx = allDayBars[i];
                        Print($"  Barra {i + 1}: {Time[barIdx]:HH:mm:ss} - O:{Open[barIdx]:F2} H:{High[barIdx]:F2} L:{Low[barIdx]:F2} C:{Close[barIdx]:F2}");
                    }
                    return;
                }

                // Ordena em ordem cronológica (maior índice = primeira barra)
                dayBars = dayBars.OrderByDescending(x => x).ToList();

                Print($"Range para análise: {Time[dayBars[0]]:HH:mm} até {Time[dayBars.Last()]:HH:mm}");

                // Primeira e última barra do range 09:30-16:00
                double dayOpen = Open[dayBars[0]];          // Primeiro Open (09:30)
                double dayClose = Close[dayBars.Last()];    // Último Close (16:00)

                // Determina Bull/Bear baseado no range 09:30-16:00
                bool isBullDay = dayClose > dayOpen;
                bool isBearDay = dayClose < dayOpen;

                if (!isBullDay && !isBearDay)
                {
                    Print($"{targetDate:dd/MM/yyyy}: Doji (O:{dayOpen:F2}=C:{dayClose:F2}) - Pulando");
                    return;
                }

                // Encontra mínima e máxima do DIA INTEIRO (não só 10:30-17:05)
                double dayLow = double.MaxValue;
                double dayHigh = double.MinValue;

                for (int i = CurrentBar; i >= 0; i--)
                {
                    if (Time[i].Date == targetDate)
                    {
                        if (Low[i] < dayLow) dayLow = Low[i];
                        if (High[i] > dayHigh) dayHigh = High[i];
                    }
                }

                string tipo = isBullDay ? "BULL" : "BEAR";
                string nome = targetDate.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
                Print($"{tipo} {targetDate:dd/MM/yyyy} ({nome}) - O:{dayOpen:F2} C:{dayClose:F2} | L:{dayLow:F2} H:{dayHigh:F2}");

                // Verifica Early Low (Bull) ou Early High (Bear)
                bool earlyPattern = false;

                if (isBullDay)
                {
                    // Bull: verifica se mínima foi formada APÓS a EarlyBarsLimit barra
                    for (int barNum = EarlyBarsLimit; barNum < dayBars.Count; barNum++)
                    {
                        int barIndex = dayBars[barNum];
                        if (Math.Abs(Low[barIndex] - dayLow) < 0.01) // Esta barra fez a mínima
                        {
                            earlyPattern = false; // Mínima foi APÓS EarlyBarsLimit barra
                            Print($"   Minima {dayLow:F2} formada na {barNum + 1}ª barra ({Time[barIndex]:HH:mm}) - APOS {EarlyBarsLimit}ª");
                            break;
                        }
                    }

                    // Se chegou aqui sem quebrar, mínima foi nas primeiras EarlyBarsLimit barras
                    if (earlyPattern == false)
                    {
                        // Verifica se realmente foi nas primeiras EarlyBarsLimit
                        for (int barNum = 0; barNum < Math.Min(EarlyBarsLimit, dayBars.Count); barNum++)
                        {
                            int barIndex = dayBars[barNum];
                            if (Math.Abs(Low[barIndex] - dayLow) < 0.01)
                            {
                                earlyPattern = true;
                                Print($"   Minima {dayLow:F2} formada na {barNum + 1}ª barra ({Time[barIndex]:HH:mm}) - PRIMEIRAS {EarlyBarsLimit}");
                                break;
                            }
                        }
                    }

                    bullishDaysHistory.Add(true);
                    earlyLowHistory.Add(earlyPattern);
                    Print($"   Bull adicionado: Early Low = {earlyPattern}");
                }
                else if (isBearDay)
                {
                    // Bear: verifica se máxima foi formada APÓS a BearEarlyBarsLimit barra  
                    for (int barNum = BearEarlyBarsLimit; barNum < dayBars.Count; barNum++)
                    {
                        int barIndex = dayBars[barNum];
                        if (Math.Abs(High[barIndex] - dayHigh) < 0.01) // Esta barra fez a máxima
                        {
                            earlyPattern = false; // Máxima foi APÓS BearEarlyBarsLimit barra
                            Print($"   Maxima {dayHigh:F2} formada na {barNum + 1}ª barra ({Time[barIndex]:HH:mm}) - APOS {BearEarlyBarsLimit}ª");
                            break;
                        }
                    }

                    // Se chegou aqui sem quebrar, máxima foi nas primeiras BearEarlyBarsLimit barras
                    if (earlyPattern == false)
                    {
                        // Verifica se realmente foi nas primeiras BearEarlyBarsLimit
                        for (int barNum = 0; barNum < Math.Min(BearEarlyBarsLimit, dayBars.Count); barNum++)
                        {
                            int barIndex = dayBars[barNum];
                            if (Math.Abs(High[barIndex] - dayHigh) < 0.01)
                            {
                                earlyPattern = true;
                                Print($"   Maxima {dayHigh:F2} formada na {barNum + 1}ª barra ({Time[barIndex]:HH:mm}) - PRIMEIRAS {BearEarlyBarsLimit}");
                                break;
                            }
                        }
                    }

                    bearishDaysHistory.Add(true);
                    earlyHighHistory.Add(earlyPattern);
                    Print($"   Bear adicionado: Early High = {earlyPattern}");
                }

                Print($"=== FIM ANALISE {targetDate:dd/MM/yyyy} ===\n");

            }
            catch (Exception ex)
            {
                Print($"Erro analisando {targetDate:dd/MM/yyyy}: {ex.Message}");
            }
        }

        private bool IsFirstBarOfSessionForBar(int barsAgo)
        {
            if (barsAgo >= CurrentBar) return false;

            try
            {
                // Compara datas para detectar mudança de sessão
                DateTime currentBarDate = Time[barsAgo].Date;
                DateTime previousBarDate = barsAgo < CurrentBar ? Time[barsAgo + 1].Date : currentBarDate.AddDays(-1);

                // Detecta mudança de sessão considerando fins de semana
                // Se a diferença entre datas for > 1 dia OU mudança de dia útil
                TimeSpan dateDiff = currentBarDate - previousBarDate;

                // Considera nova sessão se:
                // 1. Mudança de data simples (dia seguinte)
                // 2. Gap de fim de semana (diferença > 1 dia)
                // 3. Segunda após sexta (considerando fins de semana)
                bool isNewSession = dateDiff.Days >= 1;

                // Log apenas de sessões realmente importantes para não poluir
                if (isNewSession && barsAgo <= 10) // Só loga as 10 barras mais recentes
                {
                    Print($"Nova sessão detectada em {currentBarDate:dd/MM/yyyy} (gap: {dateDiff.Days} dias)");
                }

                return isNewSession;
            }
            catch (Exception ex)
            {
                Print($"Erro na detecção de sessão para barra {barsAgo}: {ex.Message}");
                return false;
            }
        }

        private bool IsInRegularSessionForBar(int barsAgo)
        {
            // Por simplicidade, assume que está na sessão regular se o filtro está desabilitado
            // ou se estamos processando dados históricos
            return true;
        }

        private void DisplayStatistics()
        {
            try
            {
                // Força a aplicação do limite antes de exibir
                while (bullishDaysHistory.Count > AnalysisDays)
                {
                    bullishDaysHistory.RemoveAt(0);
                    earlyLowHistory.RemoveAt(0);
                }
                while (bearishDaysHistory.Count > AnalysisDays)
                {
                    bearishDaysHistory.RemoveAt(0);
                    earlyHighHistory.RemoveAt(0);
                }

                int totalBullishDays = bullishDaysHistory.Count;
                int daysWithEarlyLow = earlyLowHistory.Count(x => x);

                int totalBearishDays = bearishDaysHistory.Count;
                int daysWithEarlyHigh = earlyHighHistory.Count(x => x);

                double lowPercentage = totalBullishDays > 0 ? (double)daysWithEarlyLow / totalBullishDays * 100 : 0;
                double highPercentage = totalBearishDays > 0 ? (double)daysWithEarlyHigh / totalBearishDays * 100 : 0;

                int totalDaysAnalyzed = totalBullishDays + totalBearishDays;

                NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("Arial", StatsFontSize) { Bold = true };

                string sessionType = RegularSessionOnly ? " - REGULAR SESSION ONLY" : " - ALL SESSIONS";
                string statsText = $"\nALGORITMO SIMPLIFICADO\n" +
                                 $"Range: 09:30-16:00 (US) | Bull>={EarlyBarsLimit} | Bear>={BearEarlyBarsLimit}\n" +
                                 $"Ultimos {AnalysisDays} dias | Analisados: {totalDaysAnalyzed}\n\n" +
                                 $"BULL DAYS: {totalBullishDays}\n" +
                                 $"Early Lows: {daysWithEarlyLow} ({lowPercentage:F1}%)\n\n" +
                                 $"BEAR DAYS: {totalBearishDays}\n" +
                                 $"Early Highs: {daysWithEarlyHigh} ({highPercentage:F1}%)\n\n" +
                                 $"SESSAO ATUAL:\n" +
                                 $"Barras: {barsFromSessionStart} | Global: {CurrentBar + 1}\n" +
                                 $"Open: {sessionOpen:F2} | {Time[0]:HH:mm}\n" +
                                 $"Early Low: {(minFormedInFirst18Bars ? "SIM" : "NAO")} | Early High: {(maxFormedInFirst18Bars ? "SIM" : "NAO")}";

                Print($"Stats: {totalDaysAnalyzed} dias | Bulls: {totalBullishDays} | Bears: {totalBearishDays} | Barra sessao: {barsFromSessionStart}");

                Draw.TextFixed(this, "EarlyLowStats", statsText, TextPosition.TopLeft,
                    StatsTextColor, font, Brushes.Transparent, Brushes.Transparent, 5);
            }
            catch (Exception ex)
            {
                Print("Erro nas estatísticas: " + ex.Message);
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, 50)]  // Dinâmico: usuário pode escolher quantos dias analisar
        [Display(Name = "Analysis Days", Description = "Número de DIAS ÚTEIS para analisar (ex: 4, 10, 20) - pula fins de semana", Order = 1, GroupName = "Parameters")]
        public int AnalysisDays
        {
            get; set;
        }

        [NinjaScriptProperty]
        [Range(15, 25)]
        [Display(Name = "Bull Early Bars", Description = "Bull day: após quantas barras (padrão 18)", Order = 2, GroupName = "Parameters")]
        public int EarlyBarsLimit
        {
            get; set;
        }

        [NinjaScriptProperty]
        [Range(15, 25)]
        [Display(Name = "Bear Early Bars", Description = "Bear day: após quantas barras (padrão 19)", Order = 3, GroupName = "Parameters")]
        public int BearEarlyBarsLimit
        {
            get; set;
        }

        [NinjaScriptProperty]
        [Display(Name = "Regular Session Only", Description = "Analisa 09:30-16:00 (Sessão Regular US) para determinar Bull/Bear", Order = 4, GroupName = "Parameters")]
        public bool RegularSessionOnly
        {
            get; set;
        }

        [NinjaScriptProperty]
        [Display(Name = "Show Statistics", Description = "Display statistics on chart", Order = 5, GroupName = "Display")]
        public bool ShowStatistics
        {
            get; set;
        }

        [NinjaScriptProperty]
        [Range(8, 20)]
        [Display(Name = "Font Size", Description = "Font size for statistics display", Order = 6, GroupName = "Display")]
        public int StatsFontSize
        {
            get; set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Text Color", Description = "Color for statistics text", Order = 7, GroupName = "Display")]
        public Brush StatsTextColor
        {
            get; set;
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Customs.EarlyLowHighAnalysis18Bars[] cacheEarlyLowHighAnalysis18Bars;
		public Customs.EarlyLowHighAnalysis18Bars EarlyLowHighAnalysis18Bars(int analysisDays, int earlyBarsLimit, int bearEarlyBarsLimit, bool regularSessionOnly, bool showStatistics, int statsFontSize, Brush statsTextColor)
		{
			return EarlyLowHighAnalysis18Bars(Input, analysisDays, earlyBarsLimit, bearEarlyBarsLimit, regularSessionOnly, showStatistics, statsFontSize, statsTextColor);
		}

		public Customs.EarlyLowHighAnalysis18Bars EarlyLowHighAnalysis18Bars(ISeries<double> input, int analysisDays, int earlyBarsLimit, int bearEarlyBarsLimit, bool regularSessionOnly, bool showStatistics, int statsFontSize, Brush statsTextColor)
		{
			if (cacheEarlyLowHighAnalysis18Bars != null)
				for (int idx = 0; idx < cacheEarlyLowHighAnalysis18Bars.Length; idx++)
					if (cacheEarlyLowHighAnalysis18Bars[idx] != null && cacheEarlyLowHighAnalysis18Bars[idx].AnalysisDays == analysisDays && cacheEarlyLowHighAnalysis18Bars[idx].EarlyBarsLimit == earlyBarsLimit && cacheEarlyLowHighAnalysis18Bars[idx].BearEarlyBarsLimit == bearEarlyBarsLimit && cacheEarlyLowHighAnalysis18Bars[idx].RegularSessionOnly == regularSessionOnly && cacheEarlyLowHighAnalysis18Bars[idx].ShowStatistics == showStatistics && cacheEarlyLowHighAnalysis18Bars[idx].StatsFontSize == statsFontSize && cacheEarlyLowHighAnalysis18Bars[idx].StatsTextColor == statsTextColor && cacheEarlyLowHighAnalysis18Bars[idx].EqualsInput(input))
						return cacheEarlyLowHighAnalysis18Bars[idx];
			return CacheIndicator<Customs.EarlyLowHighAnalysis18Bars>(new Customs.EarlyLowHighAnalysis18Bars(){ AnalysisDays = analysisDays, EarlyBarsLimit = earlyBarsLimit, BearEarlyBarsLimit = bearEarlyBarsLimit, RegularSessionOnly = regularSessionOnly, ShowStatistics = showStatistics, StatsFontSize = statsFontSize, StatsTextColor = statsTextColor }, input, ref cacheEarlyLowHighAnalysis18Bars);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Customs.EarlyLowHighAnalysis18Bars EarlyLowHighAnalysis18Bars(int analysisDays, int earlyBarsLimit, int bearEarlyBarsLimit, bool regularSessionOnly, bool showStatistics, int statsFontSize, Brush statsTextColor)
		{
			return indicator.EarlyLowHighAnalysis18Bars(Input, analysisDays, earlyBarsLimit, bearEarlyBarsLimit, regularSessionOnly, showStatistics, statsFontSize, statsTextColor);
		}

		public Indicators.Customs.EarlyLowHighAnalysis18Bars EarlyLowHighAnalysis18Bars(ISeries<double> input , int analysisDays, int earlyBarsLimit, int bearEarlyBarsLimit, bool regularSessionOnly, bool showStatistics, int statsFontSize, Brush statsTextColor)
		{
			return indicator.EarlyLowHighAnalysis18Bars(input, analysisDays, earlyBarsLimit, bearEarlyBarsLimit, regularSessionOnly, showStatistics, statsFontSize, statsTextColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Customs.EarlyLowHighAnalysis18Bars EarlyLowHighAnalysis18Bars(int analysisDays, int earlyBarsLimit, int bearEarlyBarsLimit, bool regularSessionOnly, bool showStatistics, int statsFontSize, Brush statsTextColor)
		{
			return indicator.EarlyLowHighAnalysis18Bars(Input, analysisDays, earlyBarsLimit, bearEarlyBarsLimit, regularSessionOnly, showStatistics, statsFontSize, statsTextColor);
		}

		public Indicators.Customs.EarlyLowHighAnalysis18Bars EarlyLowHighAnalysis18Bars(ISeries<double> input , int analysisDays, int earlyBarsLimit, int bearEarlyBarsLimit, bool regularSessionOnly, bool showStatistics, int statsFontSize, Brush statsTextColor)
		{
			return indicator.EarlyLowHighAnalysis18Bars(input, analysisDays, earlyBarsLimit, bearEarlyBarsLimit, regularSessionOnly, showStatistics, statsFontSize, statsTextColor);
		}
	}
}

#endregion
