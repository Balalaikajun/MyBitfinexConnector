using TestHQ.Abstractions.Models;

namespace TestHQ.Abstractions.Interfaces;

public interface IWebSocketClient
{
    event Action<Trade> NewBuyTrade;
    
    event Action<Trade> NewSellTrade;
    
    void SubscribeTrades(string pair, int limit = 125);
    
    void UnsubscribeTrades(string pair);

    event Action<Candle> NewCandle;

    void SubscribeCandles(string pair, int periodInSec = 60, int? limit = null);

    void UnsubscribeCandles(string pair);
}