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

public class CandlesViewModel : INotifyPropertyChanged
{
    private readonly ITestConnector _connector;

    public CandlesViewModel(ITestConnector connector, ObservableCollection<string> currentPairs)
    {
        _connector = connector;
        Period = CandlePeriodMapper.SecondsToString.Values.First();
        CurrentPairs = currentPairs;
        CurrentPairs.CollectionChanged += CurrentPairs_CollectionChanged;
        _connector.NewCandle += OnNewCandle;
    }

    public ObservableCollection<string> CurrentPairs { get; set; } = new();

    public ObservableCollection<Candle> Candles { get; } = new();
    public string Period { get; set; }
    public int MaxCount { get; set; } = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private void CurrentPairs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Replace
            || e.Action == NotifyCollectionChangedAction.Reset)
            foreach (var pair in e.OldItems!.Cast<string>())
            {
                _connector.TryUnsubscribeCandles(pair);
                foreach (var element in Candles.Where(c => c.Pair == pair).ToList())
                    Candles.Remove(element);
            }

        if (e.Action == NotifyCollectionChangedAction.Add
            || e.Action == NotifyCollectionChangedAction.Replace)
            foreach (var pair in e.NewItems!.Cast<string>())
                _ = SubscribeCandles(pair).ConfigureAwait(false);
    }

    private async Task SubscribeCandles(string pair)
    {
        try
        {
            await _connector.SubscribeCandles(pair, Period);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error subscribing to pair: {ex}");
        }
    }

    private void OnNewCandle(Candle candle)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var low = 0;
            var high = Candles.Count;

            while (low < high)
            {
                var mid = low + (high - low) / 2;
                if (Candles[mid].OpenTime > candle.OpenTime) low = mid + 1;
                else high = mid;
            }

            Candles.Insert(low, candle);

            if (Candles.Count > MaxCount)
                Candles.RemoveAt(Candles.Count - 1);
        });
    }
}