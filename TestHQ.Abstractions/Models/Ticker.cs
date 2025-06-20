namespace TestHQ.Abstractions.Models;

public class Ticker
{
    /// <summary>
    ///     Валютная пара.
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    ///     Цена последней наивысшей заявки на покупку.
    /// </summary>
    public decimal Bid { get; set; }

    /// <summary>
    ///     Объём 25 крупнейших заявок на покупку.
    /// </summary>
    public decimal BidSize { get; set; }

    /// <summary>
    ///     Цена последней наименьшей заявки на продажу.
    /// </summary>
    public decimal Ask { get; set; }

    /// <summary>
    ///     Объём 25 крупнейших заявок на продажу.
    /// </summary>
    public decimal AskSize { get; set; }

    /// <summary>
    ///     Изменение цены с момента вчерашнего закрытия.
    /// </summary>
    public decimal DailyChange { get; set; }

    /// <summary>
    ///     Относительное изменение цены с момента вчерашнего закрытия (в долях, например 0.05 = 5%).
    /// </summary>
    public decimal DailyChangeRelative { get; set; }

    /// <summary>
    ///     Цена последнего трейда.
    /// </summary>
    public decimal LastPrice { get; set; }

    /// <summary>
    ///     Объём торгов за сутки.
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    ///     Максимальная цена за сутки.
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    ///     Минимальная цена за сутки.
    /// </summary>
    public decimal Low { get; set; }
}