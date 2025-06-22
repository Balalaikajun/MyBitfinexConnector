using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestHQ.Abstractions.Interfaces;

namespace TestHQ.UI.ViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    private ITestConnector _testConnector;

    public MainViewModel(TickerViewModel tickerViewModel,
        CandlesViewModel candlesViewModel,
        PairSelectorViewModel pairSelectorViewModel,
        ITestConnector testConnector,
        TradesViewModel tradesViewModel,
        BalanceViewModel balanceViewModel)
    {
        TickerViewModel = tickerViewModel;
        PairSelectorViewModel = pairSelectorViewModel;
        CandlesViewModel = candlesViewModel;
        _testConnector = testConnector;
        TradesViewModel = tradesViewModel;
        BalanceViewModel = balanceViewModel;
    }

    public TickerViewModel TickerViewModel { get; }
    public CandlesViewModel CandlesViewModel { get; }
    public PairSelectorViewModel PairSelectorViewModel { get; }
    public BalanceViewModel BalanceViewModel { get; }

    public TradesViewModel TradesViewModel { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}