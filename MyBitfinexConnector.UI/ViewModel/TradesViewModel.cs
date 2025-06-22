using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using MyBitfinexConnector.Abstractions.Interfaces;
using MyBitfinexConnector.Abstractions.Models;
using TestConnector.Bitfinex.Common;

namespace MyBitfinexConnector.UI.ViewModel;

public class TradesViewModel
{
    private readonly ITestConnector _connector;

    public TradesViewModel(ITestConnector connector, ObservableCollection<string> currentPairs)
    {
        _connector = connector;
        Period = CandlePeriodMapper.SecondsToString.Values.First();
        CurrentPairs = currentPairs;
        CurrentPairs.CollectionChanged += CurrentPairs_CollectionChanged;
        _connector.NewBuyTrade += trade => OnNewTrade(trade, BuyTrades);
        _connector.NewSellTrade += trade => OnNewTrade(trade, SellTrades);
    }

    public ObservableCollection<string> CurrentPairs { get; set; } = new();

    public ObservableCollection<Trade> BuyTrades { get; } = new();
    public ObservableCollection<Trade> SellTrades { get; } = new();
    public string Period { get; set; }
    public int MaxCount { get; set; } = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private void CurrentPairs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Remove
            or NotifyCollectionChangedAction.Replace
            or NotifyCollectionChangedAction.Reset)
            foreach (var pair in e.OldItems!.Cast<string>())
            {
                _connector.TryUnsubscribeTrades(pair);
                foreach (var element in SellTrades.Where(c => c.Pair == pair).ToList())
                    SellTrades.Remove(element);
                foreach (var element in BuyTrades.Where(c => c.Pair == pair).ToList())
                    BuyTrades.Remove(element);
            }

        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Replace)
            foreach (var pair in e.NewItems!.Cast<string>())
                _ = SubscribeTrades(pair).ConfigureAwait(false);
    }

    private async Task SubscribeTrades(string pair)
    {
        try
        {
            await _connector.SubscribeTrades(pair);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error subscribing to pair: {ex}");
        }
    }

    private void OnNewTrade(Trade trade, ObservableCollection<Trade> trades)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var low = 0;
            var high = trades.Count;

            while (low < high)
            {
                var mid = low + (high - low) / 2;
                if (trades[mid].Time > trade.Time) low = mid + 1;
                else high = mid;
            }

            trades.Insert(low, trade);

            if (trades.Count > MaxCount)
                trades.RemoveAt(trades.Count - 1);
        });
    }
}