using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using MyBitfinexConnector.Abstractions.Interfaces;
using MyBitfinexConnector.Abstractions.Models;
using MyBitfinexConnector.UI.Interfaces;

namespace MyBitfinexConnector.UI.ViewModel;

public class TickerViewModel : INotifyPropertyChanged
{
    private readonly ITestConnector _connector;

    private string _selectedPair;

    private Ticker _ticker;

    public TickerViewModel(ITestConnector connector, IPairsService pairsService)
    {
        Ticker = new Ticker();
        _connector = connector;
        Pairs = new ObservableCollection<string>();
        SelectedPair = "BTCUSD";
        _ = LoadPairs(pairsService);
    }

    public ObservableCollection<KeyValuePair<string, string>> TickerView { get; } = new();
    public ObservableCollection<string> Pairs { get; }

    public string SelectedPair
    {
        get => _selectedPair;
        set
        {
            if (_selectedPair == value) return;
            _selectedPair = value;
            OnPropertyChanged();
            _ = LoadTicker(value);
        }
    }

    public Ticker Ticker
    {
        get => _ticker;
        set
        {
            _ticker = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private async Task LoadTicker(string pair)
    {
        Ticker = await _connector.GetTickerAsync(pair);
        OnPropertyChanged(nameof(Ticker));

        TickerView.Clear();
        if (Ticker == null) return;

        TickerView.Add(new KeyValuePair<string, string>("Last Price:", Ticker.LastPrice.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Bid:", Ticker.Bid.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Ask:", Ticker.Ask.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Bid Size:", Ticker.BidSize.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Ask Size:", Ticker.AskSize.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Изменение:", Ticker.DailyChange.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Изменение (%):",
            (Ticker.DailyChangeRelative * 100).ToString("F2") + "%"));
        TickerView.Add(new KeyValuePair<string, string>("Объём:", Ticker.Volume.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("High:", Ticker.High.ToString("F2")));
        TickerView.Add(new KeyValuePair<string, string>("Low:", Ticker.Low.ToString("F2")));
    }

    private async Task LoadPairs(IPairsService pairsService)
    {
        try
        {
            var list = await pairsService.GetPairsAsync();
            foreach (var pair in list)
                Pairs.Add(pair);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadPairs ERROR: {ex}");
            MessageBox.Show(ex.ToString(), "Ошибка загрузки пар");
        }
    }
}