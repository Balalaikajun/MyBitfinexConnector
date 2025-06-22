using System.Collections.Concurrent;
using TestConnector.Bitfinex.Common;
using TestHQ.Abstractions.Interfaces;
using TestHQ.Abstractions.Models;

namespace TestConnector.Bitfinex.Connector;

public class BitfinexConnector : ITestConnector
{
    private readonly IRestClient _restClient;
    private readonly IWebSocketClient _webSocketClient;
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastCandleOffsetByPair = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastTradeOffsetByPair = new();

    public BitfinexConnector(IRestClient restClient, IWebSocketClient webSocketClient)
    {
        _restClient = restClient;
        _webSocketClient = webSocketClient;

        _webSocketClient.NewBuyTrade += OnNewTrade;
        _webSocketClient.NewSellTrade += OnNewTrade;
        _webSocketClient.NewCandle += OnNewCandle;
    }

    #region Rest

    public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int limit = 125, DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        return await _restClient.GetTradesAsync(pair, limit, from, to);
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec,
        DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null)
    {
        return await GetCandlesAsync(pair, CandlePeriodMapper.ToStringPeriod(periodInSec), from, to, limit);
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, string periodInSec = "1m",
        DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null)
    {
        return await _restClient.GetCandlesAsync(pair, periodInSec, from, to, limit);
    }

    public async Task<Ticker> GetTickerAsync(string pair)
    {
        return await _restClient.GetTickerAsync(pair);
    }

    #endregion

    #region Socket

    #region Trade

    public event Action<Trade> NewBuyTrade;

    public event Action<Trade> NewSellTrade;

    public async Task SubscribeTrades(string pair, int limit = 125, DateTimeOffset? from = null)
    {
        var history = await GetTradesAsync(pair, from: from, limit: limit);

        foreach (var trade in history.OrderBy(c => c.Id)) OnNewTrade(trade);

        _webSocketClient.SubscribeTrades(pair);
    }

    public bool TryUnsubscribeTrades(string pair)
    {
        _lastTradeOffsetByPair.TryRemove(pair, out _);
        return _webSocketClient.TryUnsubscribeTrades(pair);
    }

    private void OnNewTrade(Trade trade)
    {
        var last = _lastTradeOffsetByPair.GetValueOrDefault(trade.Pair, DateTimeOffset.MinValue);
        if (trade.Time <= last)
            return;

        _lastTradeOffsetByPair[trade.Pair] = trade.Time;
        if (trade.Amount > 0) NewBuyTrade?.Invoke(trade);
        else NewSellTrade?.Invoke(trade);
    }

    #endregion

    #region Candle

    public event Action<Candle> NewCandle;

    public async Task SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, int? limit = null)
    {
        await SubscribeCandles(pair, CandlePeriodMapper.ToStringPeriod(periodInSec), from, limit);
    }

    public async Task SubscribeCandles(string pair, string period = "1m", DateTimeOffset? from = null,
        int? limit = null)
    {
        var history = await GetCandlesAsync(pair, period, from, limit: limit);

        foreach (var candle in history.OrderBy(c => c.OpenTime)) OnNewCandle(candle);

        _webSocketClient.SubscribeCandles(pair, period);
    }

    public bool TryUnsubscribeCandles(string pair)
    {
        _lastCandleOffsetByPair.TryRemove(pair, out _);
        return _webSocketClient.TryUnsubscribeCandles(pair);
    }

    public async Task ConnectAsync()
    {
        await _webSocketClient.ConnectAsync();
    }

    public async Task DisconnectAsync()
    {
        await _webSocketClient.DisconnectAsync();
    }

    private void OnNewCandle(Candle candle)
    {
        if (_lastCandleOffsetByPair.TryGetValue(candle.Pair, out var lastCandle))
        {
            if (candle.OpenTime <= lastCandle)
                return;
            _lastCandleOffsetByPair[candle.Pair] = candle.OpenTime;
        }
        else
        {
            _lastCandleOffsetByPair.TryAdd(candle.Pair, candle.OpenTime);
        }

        NewCandle?.Invoke(candle);
    }

    #endregion

    #endregion
}