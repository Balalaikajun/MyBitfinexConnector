using System.Text.Json;
using TestHQ.Abstractions.Models;

namespace TestConnector.Bitfinex.Parsing;

internal static class CandleParser
{
    /// <summary>
    /// Преобразует JSON-массив со свечой в объект.
    /// </summary>
    /// <param name="e">
    /// JSON-массив, представляющий собой одну свечу от Bitfinex в формате:
    /// [ MTS (timestamp, long), OPEN, CLOSE, HIGH, LOW, VOLUME ].
    /// </param>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <remarks>
    /// Документация Bitfinex: https://docs.bitfinex.com/reference/rest-public-candles.
    /// </remarks>
    public static Candle FromJson(JsonElement e, string pair) => new Candle
    {
        Pair = pair,
        OpenPrice = e[1].GetDecimal(),
        ClosePrice = e[2].GetDecimal(),
        HighPrice = e[3].GetDecimal(),
        LowPrice = e[4].GetDecimal(),
        Volume = e[5].GetDecimal(),
        OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(e[0].GetInt64()),
    };
}