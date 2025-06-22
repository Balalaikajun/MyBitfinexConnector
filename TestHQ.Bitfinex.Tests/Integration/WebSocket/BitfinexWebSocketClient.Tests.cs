using FluentAssertions;
using TestConnector.Bitfinex.WebSocket;
using TestHQ.Abstractions.Models;

namespace TestHQ.Bitfinex.Tests.Integration.WebSocket;

public class BitfinexWebSocketClient_Tests : IAsyncLifetime
{
    private readonly BitfinexWebSocketClient _webSocketClient;
    private const string Pair = "BTCUSD";

    public BitfinexWebSocketClient_Tests()
    {
        _webSocketClient = new BitfinexWebSocketClient();
    }

    public async Task InitializeAsync()
    {
        await _webSocketClient.ConnectAsync();
    }

    public async Task DisposeAsync()
    {
        await _webSocketClient.DisconnectAsync();
    }

    [Fact(Timeout = 15_000)]
    public async Task SubscribeTrades_ShouldReceiveTrade()
    {
        var tcs = new TaskCompletionSource<Trade>(TaskCreationOptions.RunContinuationsAsynchronously);

        _webSocketClient.NewBuyTrade += Handle;
        _webSocketClient.NewSellTrade += Handle;

        _webSocketClient.SubscribeTrades(Pair);

        using var cts = new CancellationTokenSource(10_000);
        using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            var trade = await tcs.Task;
            
            trade.Pair.Should().Be(Pair);
            trade.Price.Should().BeGreaterThan(0);
        }
        
        _webSocketClient.TryUnsubscribeTrades(Pair);
        _webSocketClient.NewBuyTrade -= Handle;
        _webSocketClient.NewSellTrade -= Handle;
        
        void Handle(Trade trade) => tcs.TrySetResult(trade);
    }
    
    [Fact(Timeout = 20_000)]
    public async Task SubscribeCandles_ShouldReceiveCandle()
    {
        var tcs = new TaskCompletionSource<Candle>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        _webSocketClient.NewCandle += Handle;
        _webSocketClient.SubscribeCandles(Pair);
        
        using var cts = new CancellationTokenSource(15_000);
        using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            var candle = await tcs.Task;
            
            candle.Pair.Should().Be(Pair);
            candle.OpenPrice.Should().BeGreaterThan(0);
        }
        
        _webSocketClient.TryUnsubscribeCandles(Pair);
        _webSocketClient.NewCandle -= Handle;
        
        void Handle(Candle candle) => tcs.TrySetResult(candle);
    }
}