using TestHQ.Abstractions.Models;

namespace TestHQ.Abstractions.Interfaces;

public interface IWebSocketClient
{
    /// <summary>
    ///     Событие, возникает при получении нового трейда на покупку.
    /// </summary>
    event Action<Trade> NewBuyTrade;

    /// <summary>
    ///     Событие, возникает при получении нового трейда на продажу.
    /// </summary>
    event Action<Trade> NewSellTrade;

    /// <summary>
    ///     Подписка на поток трейдов по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "BTCUSD".</param>
    void SubscribeTrades(string pair);

    /// <summary>
    ///     Отписка от потока трейдов по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "BTCUSD".</param>
    bool TryUnsubscribeTrades(string pair);

    /// <summary>
    ///     Событие, возникает при получении новой свечи.
    /// </summary>
    event Action<Candle> NewCandle;

    /// <summary>
    ///     Подписка на поток свеч по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "BTCUSD".</param>
    /// <param name="periodInSec">
    ///     Период изначально запрашиваемых свечей при подписке в секундах. Допустимы только фиксированные значения:
    ///     60 (1m), 300 (5m), 900 (15m), 1800 (30m), 3600 (1h), 10800 (3h),
    ///     21600 (6h), 43200 (12h), 86400 (1D), 604800 (7D), 1209600 (14D), 2592000 (1M).
    ///     См. <see href="https://docs.bitfinex.com/reference/rest-public-candles#available-candles">Bitfinex API</see> для
    ///     подробностей.
    /// </param>
    void SubscribeCandles(string pair, int periodInSec);

    /// <summary>
    ///     Подписка на поток свеч по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "BTCUSD".</param>
    /// <param name="period">
    ///     Период изначально запрашиваемых свечей при подписке в секундах. Допустимы только фиксированные значения:
    ///     "1m", "5m", "15m", "30m", "1h", "3h",
    ///     "6h", "12h", "1D", "7D", "14D", "1M".
    ///     См. <see href="https://docs.bitfinex.com/reference/rest-public-candles#available-candles">Bitfinex API</see> для
    ///     подробностей.
    /// </param>
    void SubscribeCandles(string pair, string period = "1m");

    /// <summary>
    ///     Отписка от потока свечей по заданной валютной паре.
    /// </summary>
    /// <param name="pair">Валютная пара, например "BTCUSD".</param>
    bool TryUnsubscribeCandles(string pair);

    /// <summary>
    ///     Запуск соединения с сервером.
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    ///     Закрытие соединения с сервером.
    /// </summary>
    Task DisconnectAsync();
}