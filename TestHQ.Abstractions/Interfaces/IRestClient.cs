using TestHQ.Abstractions.Models;

namespace TestHQ.Abstractions.Interfaces;

public interface IRestClient
{
    /// <summary>
    /// Получить трейды, по заданной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <param name="limit">Максимальное число трейдов.</param>
    /// <param name="from">Начало периода.</param>
    /// <param name="to">Конец периода.</param>
    /// <returns>Список трейдов.</returns>
    Task<IEnumerable<Trade>> GetTradesAsync(string pair, int limit = 125, DateTimeOffset? from = null, DateTimeOffset? to = null);


    /// <summary>
    /// Получить свечи, по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <param name="periodInSec">
    /// Период свечей в секундах. Допустимы только фиксированные значения:
    /// 60 (1m), 300 (5m), 900 (15m), 1800 (30m), 3600 (1h), 10800 (3h), 
    /// 21600 (6h), 43200 (12h), 86400 (1D), 604800 (7D), 1209600 (14D), 2592000 (1M).
    /// См. <see href="https://docs.bitfinex.com/reference/rest-public-candles#available-candles">Bitfinex API</see> для подробностей.
    /// </param>
    /// <param name="from">Начало периода.</param>
    /// <param name="to">Конец периода.</param>
    /// <param name="limit">Максимальное число свеч.</param>
    /// <returns>Список свечей.</returns>
    Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec = 60, DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null);
    
    /// <summary>
    /// Получить тикер по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <returns>Тикер.</returns>
    Task<Ticker> GetTickerAsync(string pair);
}