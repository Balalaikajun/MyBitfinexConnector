using System.Text.Json;
using TestHQ.Abstractions.Models;

namespace TestConnector.Bitfinex.Parsing;

internal static class TradeParser
{
    /// <summary>
    /// Преобразует JSON-массив с данными о трейде в объект.
    /// </summary>
    /// <param name="e">
    /// JSON-массив, представляющий собой обновление трейда от Bitfinex.
    /// Ожидается массив из следующих элементов:
    /// [ ID (int), MTS (timestamp, long), AMOUNT (decimal), PRICE (decimal) ].
    /// </param>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <remarks>
    /// См. описание формата: https://docs.bitfinex.com/reference/rest-public-trades.
    /// </remarks>
    public static Trade FromJson(JsonElement e, string pair)
    {
        var amount = e[2].GetDecimal();

        return new Trade
        {
            Pair = pair,
            Price = e[3].GetDecimal(),
            Amount = amount,
            Side = amount > 0 ? "buy" : "sell",
            Time = DateTimeOffset.FromUnixTimeMilliseconds(e[1].GetInt64()),
            Id = e[0].GetInt32(),
        };
    }
} 