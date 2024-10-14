using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.Management;
using CommunityToolkit.Mvvm.ComponentModel;
using power_task_man.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Storage;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Microsoft.UI.Xaml.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace power_task_man.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public partial class PerformancePage : Page
    {

        internal CPUPerfService cpuPerfService = new();
        public string CPUFrequency { get; set; } = "Something";

        public PerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = cpuPerfService;
            this.cpuPerfService.StartCPUFreqMonitoring();     
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.cpuPerfService.StartCPUFreqMonitoring();
        }
    }
}
