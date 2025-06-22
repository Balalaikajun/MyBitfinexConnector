using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyBitfinexConnector.Abstractions.Interfaces;

namespace MyBitfinexConnector.UI.ViewModel;

public class BalanceViewModel : INotifyPropertyChanged
{
    private readonly ITestConnector _connector;
    private readonly Dictionary<string, decimal> _exchangeRates = new();
    private readonly string[] _trackedCurrencies = { "BTC", "XRP", "XMR", "DSH" };
    private decimal _bTCBalance = 1;
    private decimal _dASHBalance = 30;
    private string _selectedCurrency = "USD";
    private decimal _xMRBalance = 50;
    private decimal _xRPBalance = 15000;

    public BalanceViewModel(ITestConnector connector)
    {
        _connector = connector;
        _ = CalcExchangeRates();
    }

    public decimal BTCBalance
    {
        get => _bTCBalance;
        set
        {
            if (value == _bTCBalance) return;
            _bTCBalance = value;
            OnPropertyChanged();
            CalcTotal();
        }
    }

    public decimal XRPBalance
    {
        get => _xRPBalance;
        set
        {
            if (value == _xRPBalance) return;
            _xRPBalance = value;
            OnPropertyChanged();
            CalcTotal();
        }
    }

    public decimal XMRBalance
    {
        get => _xMRBalance;
        set
        {
            if (value == _xMRBalance) return;
            _xMRBalance = value;
            OnPropertyChanged();
            CalcTotal();
        }
    }

    public decimal DASHBalance
    {
        get => _dASHBalance;
        set
        {
            if (_dASHBalance == value) return;
            _dASHBalance = value;
            OnPropertyChanged();
            CalcTotal();
        }
    }

    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (_selectedCurrency == value) return;
            _selectedCurrency = value;
            OnPropertyChanged();
            CalcTotal();
        }
    }

    public decimal TotalBalance { get; private set; }

    public ObservableCollection<string> Currencies { get; } = new()
    {
        "USD", "BTC", "XRP", "XMR", "DSH"
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task CalcExchangeRates()
    {
        foreach (var currency in _trackedCurrencies)
        {
            var pair = $"{currency}USD";
            var ticker = await _connector.GetTickerAsync(pair);
            var mid = (ticker.Bid + ticker.Ask) / 2m;
            _exchangeRates[pair] = mid;
        }

        CalcTotal();
    }

    private decimal GetRate(string from, string to)
    {
        if (from == to) return 1m;
        if (to == "USD") return _exchangeRates[$"{from}{to}"];
        return 0m;
    }

    private void CalcTotal()
    {
        var totalUSD =
            BTCBalance * GetRate("BTC", "USD") +
            XRPBalance * GetRate("XRP", "USD") +
            XMRBalance * GetRate("XMR", "USD") +
            DASHBalance * GetRate("DSH", "USD");

        TotalBalance = SelectedCurrency == "USD" ? totalUSD : totalUSD / GetRate(_selectedCurrency, "USD");

        OnPropertyChanged(nameof(TotalBalance));
    }

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}