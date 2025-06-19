namespace TestConnector.Bitfinex.Common;

public static class CandlePeriodMapper
{
    /// <summary>
    /// Словарь для соответствия секунд строковым периодам Bitfinex API.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string> SecondsToString = new Dictionary<int, string>
    {
        { 60, "1m" },
        { 300, "5m" },
        { 900, "15m" },
        { 1800, "30m" },
        { 3600, "1h" },
        { 10800, "3h" },
        { 21600, "6h" },
        { 43200, "12h" },
        { 86400, "1D" },
        { 604800, "7D" },
        { 1209600, "14D" },
        { 2592000, "1M" }
    };
    
    /// <summary>
    /// Получить строковый таймфрейм по секундам.
    /// </summary>
    public static string ToStringPeriod(int seconds)
    {
        if (!SecondsToString.TryGetValue(seconds, out var result))
            throw new ArgumentException($"Период в {seconds} секунд не поддерживается API Bitfinex.", nameof(seconds));

        return result;
    }
}