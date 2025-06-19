using TestHQ.Abstractions.Models;

namespace TestHQ.Abstractions.Interfaces;

public interface ITestConnector
{
    #region Rest
    
    Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = 0);

    Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = 0);
    Task<IEnumerable<Trade>> GetTradesAsync(string pair, int limit, DateTimeOffset? from = null, DateTimeOffset? to = null);
    Task<IEnumerable<Trade>> GetTradesAsync(string pair, int limit = 125, DateTimeOffset? from = null, DateTimeOffset? to = null);

    Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec = 60, DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null);

    #endregion

    #region Socket

    event Action<Trade> NewBuyTrade;
    
    event Action<Trade> NewSellTrade;
    
    void SubscribeTrades(string pair, int limit = 125);
    
    void UnsubscribeTrades(string pair);

    event Action<Candle> NewCandle;

    void SubscribeCandles(string pair, int periodInSec = 60, int? limit = null);

    void UnsubscribeCandles(string pair);

    #endregion
}