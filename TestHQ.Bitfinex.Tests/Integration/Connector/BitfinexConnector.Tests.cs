using FluentAssertions;
using TestConnector.Bitfinex.Common;
using TestConnector.Bitfinex.Connector;
using TestConnector.Bitfinex.REST;
using TestConnector.Bitfinex.WebSocket;
using TestHQ.Abstractions.Models;
using System.Collections.Concurrent;

namespace TestHQ.Bitfinex.Tests.Integration.Connector;

public class BitfinexConnector_Tests : IAsyncLifetime
{
    private readonly BitfinexConnector _connector;
    private static string Pair = "tBTCUSD";

    public BitfinexConnector_Tests()
    {
        _connector = new BitfinexConnector(
            new BitfinexRestClient(),
            new BitfinexWebSocketClient());
    }

    public async Task InitializeAsync()
    {
        await _connector.ConnectAsync();
    }

    public async Task DisposeAsync()
    {
        await _connector.DisconnectAsync();
    }

    #region REST

    #region GetTradesAsync

    [Fact]
    public async Task GetTradesAsync_ReturnsRealCandles()
    {
        var trades = await _connector.GetTradesAsync(Pair);

        trades.Should().NotBeEmpty();

        var trade = trades.First();

        trade.Pair.Should().Be(Pair);
    }

    [Fact]
    public async Task GetTradesAsync_LimitIsWork()
    {
        var limit = 10;

        var trades = await _connector.GetTradesAsync(Pair, limit: limit);

        trades.Should().NotBeEmpty();
        trades.Should().HaveCountLessThanOrEqualTo(limit);
    }

    [Fact]
    public async Task GetTradesAsync_FromToIsWork()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var to = DateTimeOffset.UtcNow;

        var trades = await _connector.GetTradesAsync(Pair, from: from, to: to);

        trades.Should().NotBeEmpty();
        trades.Max(c => c.Time).Should().BeBefore(to.AddMilliseconds(500));
        trades.Min(c => c.Time).Should().BeAfter(from.AddMilliseconds(-500));
    }

    [Fact]
    public async Task GetTradesAsync_LimitFromToIsWork()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var to = DateTimeOffset.UtcNow;
        var limit = 20;

        var trades = await _connector.GetTradesAsync(Pair, limit: limit, from: from, to: to);

        trades.Should().NotBeEmpty();
        trades.Should().HaveCountLessThanOrEqualTo(limit);
        trades.Max(c => c.Time).Should().BeBefore(to.AddMilliseconds(500));
        trades.Min(c => c.Time).Should().BeAfter(from.AddMilliseconds(-500));
    }

    #endregion

    #region GetCandlesAsync

    [Fact]
    public async Task GetCandlesAsync_ReturnsRealCandles()
    {
        var candles = await _connector.GetCandlesAsync(Pair);

        candles.Should().NotBeEmpty();

        var candle = candles.First();

        candle.Pair.Should().Be(Pair);
    }

    [Fact]
    public async Task GetCandlesAsync_PeriodIsWork()
    {
        var candlesFromSec = CandlePeriodMapper.SecondsToString.Values
            .Select(period => _connector.GetCandlesAsync(Pair, period, limit: 1));

        foreach (var candles in await Task.WhenAll(candlesFromSec))
        {
            candles.Should().HaveCount(1);
        }

        var candlesFromString = CandlePeriodMapper.SecondsToString.Values
            .Select(period => _connector.GetCandlesAsync(Pair, period, limit: 1));

        foreach (var candles in await Task.WhenAll(candlesFromString))
        {
            candles.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task GetCandlesAsync_LimitIsWork()
    {
        var limit = 10;

        var candles = await _connector.GetCandlesAsync(Pair, limit: limit);

        candles.Should().NotBeEmpty();
        candles.Should().HaveCountLessThanOrEqualTo(limit);
    }

    [Fact]
    public async Task GetCandlesAsync_FromToIsWork()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var to = DateTimeOffset.UtcNow;

        var candles = await _connector.GetCandlesAsync(Pair, from: from, to: to);

        candles.Should().NotBeEmpty();
        candles.Max(c => c.OpenTime).Should().BeBefore(to.AddMilliseconds(500));
        candles.Min(c => c.OpenTime).Should().BeAfter(from.AddMilliseconds(-500));
    }

    [Fact]
    public async Task GetCandlesAsync_LimitFromToIsWork()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var to = DateTimeOffset.UtcNow;
        var limit = 20;

        var candles = await _connector.GetCandlesAsync(Pair, limit: limit, from: from, to: to);

        candles.Should().NotBeEmpty();
        candles.Should().HaveCountLessThan(limit);
        candles.Max(c => c.OpenTime).Should().BeBefore(to.AddMilliseconds(500));
        candles.Min(c => c.OpenTime).Should().BeAfter(from.AddMilliseconds(-500));
    }

    #endregion


    #region GetTickerAsync

    [Fact]
    public async Task GetTickerAsync_ReturnsRealTicker()
    {
        var ticker = await _connector.GetTickerAsync(Pair);

        ticker.Pair.Should().Be(Pair);
    }

    #endregion

    #endregion

    #region WebSocket

    [Fact(Timeout = 15_000)]
    public async Task SubscribeTrades_ShouldReceiveTrade()
    {
        var tcs = new TaskCompletionSource<Trade>(TaskCreationOptions.RunContinuationsAsynchronously);
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);

        _connector.NewBuyTrade += Handle;
        _connector.NewSellTrade += Handle;

        _connector.SubscribeTrades(Pair, from: from);
        
        using var cts = new CancellationTokenSource(10_000);
        using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            var trade = await tcs.Task;

            trade.Pair.Should().Be(Pair);
            trade.Price.Should().BeGreaterThan(0);
            trade.Time.Should().BeAfter(from.AddMilliseconds(500));
        }

        _connector.TryUnsubscribeTrades(Pair);
        _connector.NewBuyTrade -= Handle;
        _connector.NewSellTrade -= Handle;

        void Handle(Trade trade) => tcs.TrySetResult(trade);
    }

    [Fact(Timeout = 20_000)]
    public async Task SubscribeCandles_ShouldReceiveCandle()
    {
        var tcs = new TaskCompletionSource<Candle>(TaskCreationOptions.RunContinuationsAsynchronously);
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);

        _connector.NewCandle += Handle;
        await _connector.SubscribeCandles(Pair, from: from);

        using var cts = new CancellationTokenSource(15_000);
        using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            var candle = await tcs.Task;

            candle.Pair.Should().Be(Pair);
            candle.OpenPrice.Should().BeGreaterThan(0);
            candle.OpenTime.Should().BeAfter(from.AddMilliseconds(500));
        }

        _connector.TryUnsubscribeCandles(Pair);
        _connector.NewCandle -= Handle;

        void Handle(Candle candle) => tcs.TrySetResult(candle);
    }

    #endregion
}