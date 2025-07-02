using System.Windows;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace SerialNumberGeneratorApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Add configuration
        ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        IConfiguration configuration = configurationBuilder.Build();
        services.AddSingleton<IConfiguration>(configuration);
        
        // Configure Logging
        services.AddLogging();

        // Register Services
        services.AddSingleton<ISerialNumberGenerator, SerialNumberGenerator>();

        // Register ViewModels
        services.AddDbContextPool<WorkOrderContext>(options => options.UseSqlServer(
            configuration.GetConnectionString("CubeConnectionString")
        ));
        // Register Views
        services.AddSingleton<MainWindow>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        // Dispose of services if needed
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }  
}