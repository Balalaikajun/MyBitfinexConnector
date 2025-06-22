using System.Text.Json;
using MyBitfinexConnector.Abstractions.Models;

namespace TestConnector.Bitfinex.Parsing;

internal static class TickerParser
{
    /// <summary>
    ///     Преобразует JSON-массив с данными тикера в объект.
    /// </summary>
    /// <param name="e">
    ///     JSON-элемент в формате массива, содержащий информацию о тикере, полученную от Bitfinex.
    ///     Ожидается массив следующей структуры:
    ///     [
    ///     BID (decimal, BID_SIZE (decimal), ASK (decimal),
    ///     ASK_SIZE (decimal), DAILY_CHANGE (decimal),
    ///     DAILY_CHANGE_PERC (decimal), LAST_PRICE (decimal),
    ///     VOLUME (decimal), HIGH (decimal), LOW (decimal)
    ///     ].
    /// </param>
    /// <param name="pair">Валютная пара.</param>
    /// <remarks>
    ///     См. описание формата: https://docs.bitfinex.com/reference/rest-public-ticker.
    /// </remarks>
    public static Ticker FromJson(JsonElement e, string pair)
    {
        return new Ticker
        {
            Pair = pair,
            Bid = e[0].GetDecimal(),
            BidSize = e[1].GetDecimal(),
            Ask = e[2].GetDecimal(),
            AskSize = e[3].GetDecimal(),
            DailyChange = e[4].GetDecimal(),
            DailyChangeRelative = e[5].GetDecimal(),
            LastPrice = e[6].GetDecimal(),
            Volume = e[7].GetDecimal(),
            High = e[8].GetDecimal(),
            Low = e[9].GetDecimal()
        };
    }
}