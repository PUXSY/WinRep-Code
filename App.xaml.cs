using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using WinRep_Code.Utilities;
using WinRep_Code.ViewModels;

namespace WinRep_Code
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<MainWindow>(Provider => new MainWindow
            {
                DataContext = Provider.GetRequiredService<MainWindowVM>()
            });
            services.AddSingleton<CustomTweaksVM>();    
            services.AddSingleton<InstallVM>();
            services.AddSingleton<ScanSettingsVM>();
            services.AddSingleton<ScanVM>();
            services.AddSingleton<TweaksVM>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}
