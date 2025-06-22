using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestHQ.UI.Models;

public class SelectableItem<T> : INotifyPropertyChanged where T : class
{
    private bool _isSelected;

    public SelectableItem(T item)
    {
        Item = item;
    }

    public T Item { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}