using System.Windows;
using MyBitfinexConnector.UI.ViewModel;

namespace MyBitfinexConnector.UI;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
    }
}