using FluentAssertions;
using TestConnector.Bitfinex.Common;
using TestConnector.Bitfinex.REST;
using TestHQ.Abstractions.Models;

namespace TestHQ.Bitfinex.Tests.Integration.REST;

public class BitfinexRestClient_Tests
{
    private readonly BitfinexRestClient _client;

    private static string Pair = "tBTCUSD";

    public BitfinexRestClient_Tests()
    {
        _client = new BitfinexRestClient();
    }

    #region GetTradesAsync

    [Fact]
    public async Task GetTradesAsync_ReturnsRealCandles()
    {
        var trades = await _client.GetTradesAsync(Pair);

        trades.Should().NotBeEmpty();

        var trade = trades.First();

        trade.Pair.Should().Be(Pair);
    }
    
    [Fact]
    public async Task GetTradesAsync_LimitIsWork()
    {
        var limit = 10;

        var trades = await _client.GetTradesAsync(Pair, limit: limit);

        trades.Should().NotBeEmpty();
        trades.Should().HaveCountLessThanOrEqualTo(limit);
    }

    [Fact]
    public async Task GetTradesAsync_FromToIsWork()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var to = DateTimeOffset.UtcNow;

        var trades = await _client.GetTradesAsync(Pair, from: from, to: to);

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

        var trades = await _client.GetTradesAsync(Pair, limit: limit, from: from, to: to);

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
        var candles = await _client.GetCandlesAsync(Pair);

        candles.Should().NotBeEmpty();

        var candle = candles.First();

        candle.Pair.Should().Be(Pair);
    }

    [Fact]
    public async Task GetCandlesAsync_PeriodIsWork()
    {
        var candlesFromSec = CandlePeriodMapper.SecondsToString.Values
            .Select(period => _client.GetCandlesAsync(Pair, period, limit: 1));

        foreach (var candles in await Task.WhenAll(candlesFromSec))
        {
            candles.Should().HaveCount(1);
        }

        var candlesFromString = CandlePeriodMapper.SecondsToString.Values
            .Select(period => _client.GetCandlesAsync(Pair, period, limit: 1));

        foreach (var candles in await Task.WhenAll(candlesFromString))
        {
            candles.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task GetCandlesAsync_LimitIsWork()
    {
        var limit = 10;

        var candles = await _client.GetCandlesAsync(Pair, limit: limit);

        candles.Should().NotBeEmpty();
        candles.Should().HaveCountLessThanOrEqualTo(limit);
    }

    [Fact]
    public async Task GetCandlesAsync_FromToIsWork()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var to = DateTimeOffset.UtcNow;

        var candles = await _client.GetCandlesAsync(Pair, from: from, to: to);

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

        var candles = await _client.GetCandlesAsync(Pair, limit: limit, from: from, to: to);

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
        var ticker = await _client.GetTickerAsync(Pair);

        ticker.Pair.Should().Be(Pair);
    }

    #endregion
}