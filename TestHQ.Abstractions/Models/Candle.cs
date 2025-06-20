namespace TestHQ.Abstractions.Models;

public class Candle
{
    /// <summary>
    ///     Валютная пара.
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    ///     Цена открытия.
    /// </summary>
    public decimal OpenPrice { get; set; }

    /// <summary>
    ///     Цена закрытия.
    /// </summary>
    public decimal ClosePrice { get; set; }

    /// <summary>
    ///     Максимальная цена.
    /// </summary>
    public decimal HighPrice { get; set; }

    /// <summary>
    ///     Минимальная цена.
    /// </summary>
    public decimal LowPrice { get; set; }

    // TotalPrice был убран из-за отсутствия его в API Bitfinex,
    // а для его корректного расчёта требуется сложная логика
    // /// <summary>
    // /// Partial (Общая сумма сделок).
    // /// </summary>
    // public decimal TotalPrice { get; set; }

    /// <summary>
    ///     Partial (Общий объем).
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    ///     Время открытия
    /// </summary>
    public DateTimeOffset OpenTime { get; set; }

    // Убрано из-за отсутствия в API
    // /// <summary>
    // /// Время закрытия
    // /// </summary>
    // public DateTimeOffset CloseTime { get; set; }
}