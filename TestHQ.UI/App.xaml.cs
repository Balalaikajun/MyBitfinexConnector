using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TestConnector.Bitfinex.Connector;
using TestConnector.Bitfinex.REST;
using TestConnector.Bitfinex.WebSocket;
using TestHQ.Abstractions.Interfaces;
using TestHQ.UI.Interfaces;
using TestHQ.UI.Services;
using TestHQ.UI.ViewModel;

namespace TestHQ.UI;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var connector = ServiceProvider.GetRequiredService<ITestConnector>();

        try
        {
            await connector.ConnectAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка подключения: {ex.Message}");
            MessageBox.Show("Не удалось подключиться к WebSocket-серверу.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }


    protected override void OnExit(ExitEventArgs e)
    {
        _ = DisconnectAsync();

        base.OnExit(e);
    }

    private async Task DisconnectAsync()
    {
        var connector = ServiceProvider.GetRequiredService<ITestConnector>();

        try
        {
            await connector.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка отключения: {ex.Message}");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPairsService, PairsService>();
        services.AddSingleton<ITestConnector, BitfinexConnector>(provider =>
        {
            return new BitfinexConnector(
                new BitfinexRestClient(),
                new BitfinexWebSocketClient());
        });

        services.AddSingleton<PairSelectorViewModel>();
        services.AddSingleton<ObservableCollection<string>>(provider
            => provider.GetRequiredService<PairSelectorViewModel>().SelectedPairs);
        services.AddSingleton<TickerViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<CandlesViewModel>();
        services.AddSingleton<TradesViewModel>();
        services.AddSingleton<BalanceViewModel>();


        services.AddTransient<MainWindow>(provider =>
        {
            var vm = provider.GetRequiredService<MainViewModel>();
            var window = new MainWindow(vm);
            return window;
        });
    }
}