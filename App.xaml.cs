using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PowerTaskMan.Services;
using System;
using PowerTaskMan.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        private IServiceCollection services = new ServiceCollection();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            services.AddSingleton<Window, MainWindow>();
            services.AddSingleton<ICPUPerfService, CPUPerfService>();
            services.AddSingleton<CPUPerformanceViewModel>();

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = ServiceProvider.GetRequiredService<Window>();
            m_window.Activate();
        }

        private Window m_window;
    }
}
