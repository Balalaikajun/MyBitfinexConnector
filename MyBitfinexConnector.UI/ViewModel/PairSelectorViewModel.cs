using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MyBitfinexConnector.UI.Comands;
using MyBitfinexConnector.UI.Interfaces;
using MyBitfinexConnector.UI.Models;

namespace MyBitfinexConnector.UI.ViewModel;

public class PairSelectorViewModel : INotifyPropertyChanged
{
    public PairSelectorViewModel(IPairsService pairsService)
    {
        _ = LoadPairs(pairsService);

        ResetCommand = new ReleyCommand(_ =>
        {
            foreach (var pair in SelectablePairs) pair.IsSelected = false;
        });
    }

    public ObservableCollection<SelectableItem<string>> SelectablePairs { get; } = new();

    public ObservableCollection<string> SelectedPairs { get; } = new();

    public ICommand ResetCommand { get; }

    public string SelectedSummary => string.Join(", ", SelectedPairs);

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    private async Task LoadPairs(IPairsService pairsService)
    {
        try
        {
            var list = await pairsService.GetPairsAsync();
            foreach (var pair in list)
            {
                var item = new SelectableItem<string>(pair);
                item.PropertyChanged += OnPairSelectChange;
                SelectablePairs.Add(item);
            }

            var USD = SelectablePairs.FirstOrDefault(p => p.Item == "BTCUSD");
            USD.IsSelected = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadPairs ERROR: {ex}");
            MessageBox.Show(ex.ToString(), "Ошибка загрузки пар");
        }
    }

    private void OnPairSelectChange(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SelectableItem<string>.IsSelected))
            return;

        var item = (SelectableItem<string>)sender!;
        if (item.IsSelected)
        {
            if (SelectedPairs.Contains(item.Item))
                return;

            SelectedPairs.Add(item.Item);
        }
        else
        {
            SelectedPairs.Remove(item.Item);
        }

        OnPropertyChanged(nameof(SelectedPairs));
        OnPropertyChanged(nameof(SelectedSummary));
    }
}