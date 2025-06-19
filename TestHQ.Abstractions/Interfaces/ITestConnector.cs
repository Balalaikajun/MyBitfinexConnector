using TestHQ.Abstractions.Models;

namespace TestHQ.Abstractions.Interfaces;

/// <summary>
/// Универсальный коннектор для получения рыночных данных через REST и WebSocket.
/// </summary>
public interface ITestConnector
{
    #region Rest
    
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
    /// <param name="limit">Максимальное число трейдов.</param>
    /// <returns>Список свечей.</returns>
    Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec = 60, DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null);

    #endregion

    #region Socket

    /// <summary>
    /// Событие, возникает при получении нового трейда на покупку.
    /// </summary>
    event Action<Trade> NewBuyTrade;
    
    /// <summary>
    /// Событие, возникает при получении нового трейда на продажу.
    /// </summary>
    event Action<Trade> NewSellTrade;
    
    /// <summary>
    /// Подписка на поток трейдов по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <param name="limit">Количество трейдов изначально запрашиваемых при подписке.</param>
    void SubscribeTrades(string pair, int limit = 125);
    
    /// <summary>
    /// Отписка от потока трейдов по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    void UnsubscribeTrades(string pair);

    /// <summary>
    /// Событие, возникает при получении новой свечи.
    /// </summary>
    event Action<Candle> NewCandle;

    /// <summary>
    /// Подписка на поток свеч по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    /// <param name="periodInSec">
    /// Период изначально запрашиваемых свечей при подписке в секундах. Допустимы только фиксированные значения:
    /// 60 (1m), 300 (5m), 900 (15m), 1800 (30m), 3600 (1h), 10800 (3h), 
    /// 21600 (6h), 43200 (12h), 86400 (1D), 604800 (7D), 1209600 (14D), 2592000 (1M).
    /// См. <see href="https://docs.bitfinex.com/reference/rest-public-candles#available-candles">Bitfinex API</see> для подробностей.
    /// </param>
    /// <param name="limit">Количество свечей изначально запрашиваемых при подписке.</param>
    void SubscribeCandles(string pair, int periodInSec = 60, int? limit = null);

    /// <summary>
    /// Отписка от потока свечей по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "tBTCUSD".</param>
    void UnsubscribeCandles(string pair);

    #endregion
}