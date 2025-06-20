using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using TestConnector.Bitfinex.Common;
using TestConnector.Bitfinex.Models;
using TestConnector.Bitfinex.Parsing;
using TestHQ.Abstractions.Interfaces;
using TestHQ.Abstractions.Models;
using Websocket.Client;

namespace TestConnector.Bitfinex.WebSocket;

public class BitfinexWebSocketClient : IWebSocketClient
{
    private const string TradesChannel = "trades";
    private const string CandlesChannel = "candles";
    private readonly ConcurrentDictionary<int, SubscriptionInfo> _subscriptions = new();
    private readonly WebsocketClient _webSocketClient;

    public BitfinexWebSocketClient()
    {
        var uri = new Uri("wss://api-pub.bitfinex.com/ws/2");
        _webSocketClient = new WebsocketClient(uri);

        _webSocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _webSocketClient.ReconnectionHappened.Subscribe(info =>
        {
            var restartSubs = _subscriptions.Values.ToArray();
            foreach (var subs in restartSubs)
                switch (subs.Channel)
                {
                    case TradesChannel:
                        SubscribeTrades(subs.Pair);
                        break;
                    case CandlesChannel:
                        SubscribeCandles(subs.Pair, subs.CandlesPeriod);
                        break;
                }
        });

        _webSocketClient.MessageReceived.Subscribe(OnMessage);
    }

    public event Action<Trade> NewBuyTrade;

    public event Action<Trade> NewSellTrade;

    public void SubscribeTrades(string pair)
    {
        if (!pair.StartsWith('t'))
            throw new InvalidDataException("Invalid pair");

        var msg = new
        {
            @event = "subscribe",
            channel = TradesChannel,
            symbol = pair
        };

        var json = JsonSerializer.Serialize(msg);
        _webSocketClient.Send(json);
    }

    public bool TryUnsubscribeTrades(string pair)
    {
        if (!pair.StartsWith('t'))
            throw new InvalidDataException("Invalid pair");

        var subs = _subscriptions.Values.FirstOrDefault(x =>
            x.Pair == pair && x.Channel == TradesChannel);

        if (subs == null)
            return false;

        var msg = new
        {
            @event = "unsubscribe",
            chanId = subs.ChanId
        };

        var json = JsonSerializer.Serialize(msg);

        _webSocketClient.Send(json);

        return true;
    }

    public event Action<Candle> NewCandle;


    public void SubscribeCandles(string pair, int periodInSec)
    {
        SubscribeCandles(pair, CandlePeriodMapper.ToStringPeriod(periodInSec));
    }

    public void SubscribeCandles(string pair, string period = "1m")
    {
        if (!pair.StartsWith('t'))
            throw new InvalidDataException("Invalid pair");

        var msg = new
        {
            @event = "subscribe",
            channel = CandlesChannel,
            key = $"trade:{period}:{pair}"
        };

        var json = JsonSerializer.Serialize(msg);
        _webSocketClient.Send(json);
    }


    public bool TryUnsubscribeCandles(string pair)
    {
        if (!pair.StartsWith('t'))
            throw new InvalidDataException("Invalid pair");

        var subs = _subscriptions.Values.FirstOrDefault(x =>
            x.Pair == pair && x.Channel == CandlesChannel);

        if (subs == null)
            return false;

        var msg = new
        {
            @event = "unsubscribe",
            chanId = subs.ChanId
        };

        var json = JsonSerializer.Serialize(msg);

        _webSocketClient.Send(json);

        return true;
    }

    public async Task ConnectAsync()
    {
        await _webSocketClient.Start();
    }

    public async Task DisconnectAsync()
    {
        await _webSocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Closed by user");
    }

    /// <summary>
    ///     Обработчик сообщений сервера.
    /// </summary>
    /// <param name="message">Сообщение сервера.</param>
    private void OnMessage(ResponseMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text)) return;
        using var doc = JsonDocument.Parse(message.Text);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            var evt = root.GetProperty("event").GetString();
            switch (evt)
            {
                case "subscribed":
                    HandleSubscribe(root);
                    break;
                case "unsubscribed":
                    HandleUnsubscribe(root);
                    break;
            }

            return;
        }

        if (root.ValueKind != JsonValueKind.Array) return;

        var chanId = root[0].GetInt32();

        if (!_subscriptions.TryGetValue(chanId, out var subs)) return;

        var playload = root[1];


        if (playload.ValueKind == JsonValueKind.String)
        {
            if (playload.GetString() != "tu")
                return;
            var data = root[2];
            HandleData(subs, data);
        }
        else if (playload.EnumerateArray().First().ValueKind != JsonValueKind.Array)
        {
            HandleData(subs, playload);
        }
        else if (playload.EnumerateArray().First().ValueKind == JsonValueKind.Array)
        {
            foreach (var element in playload.EnumerateArray().OrderBy(e => subs.Channel switch
                     {
                         CandlesChannel => e[0].GetInt64(),
                         TradesChannel => e[1].GetInt64()
                     }))
                HandleData(subs, element);
        }
    }

    /// <summary>
    ///     Обработчик события подписки.
    /// </summary>
    /// <param name="data">Сообщение сервера.</param>
    private void HandleSubscribe(JsonElement data)
    {
        var subs = new SubscriptionInfo
        {
            ChanId = data.GetProperty("chanId").GetInt32(),
            Channel = data.GetProperty("channel").GetString()
        };

        switch (subs.Channel)
        {
            case TradesChannel:
                subs.Pair = data.GetProperty("symbol").GetString();
                break;
            case CandlesChannel:
                var cuttedKey = data.GetProperty("key").GetString().Split(':');
                subs.Pair = cuttedKey[2];
                subs.CandlesPeriod = cuttedKey[1];
                break;
        }

        var existing = _subscriptions.Values.FirstOrDefault(x => x.Pair == subs.Pair && x.Channel == subs.Channel);
        if (existing != null)
            _subscriptions.TryRemove(existing.ChanId, out _);

        _subscriptions.TryAdd(subs.ChanId, subs);
    }

    /// <summary>
    ///     Обработчик события отписки.
    /// </summary>
    /// <param name="data">Сообщение сервера.</param>
    private void HandleUnsubscribe(JsonElement data)
    {
        var id = data.GetProperty("chanId").GetInt32();

        _subscriptions.TryRemove(id, out _);
    }

    /// <summary>
    ///     Обработчик данных.
    /// </summary>
    /// <param name="info">Информация о канале.</param>
    /// <param name="data">Данные для обработки.</param>
    private void HandleData(SubscriptionInfo info, JsonElement data)
    {
        switch (info.Channel)
        {
            case TradesChannel:
                var trade = TradeParser.FromJson(data, info.Pair);
                if (trade.Amount > 0) NewBuyTrade?.Invoke(trade);
                else NewSellTrade?.Invoke(trade);
                break;

            case CandlesChannel:
                var candle = CandleParser.FromJson(data, info.Pair);
                NewCandle?.Invoke(candle);
                break;
        }
    }
}