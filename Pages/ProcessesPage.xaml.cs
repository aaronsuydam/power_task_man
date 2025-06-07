using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PowerTaskMan.Controls;
using PowerTaskMan.Common;
using PowerTaskMan.ViewModels;
using PowerTaskMan;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace power_task_man.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProcessesPage : Page
    {
        public IList<ICoordinatePair> perfmod_freq;
        public CPUPerformanceViewModel _vm;
        public ProcessesPage()
        {
            this.InitializeComponent();
            _vm = App.ServiceProvider.GetService(typeof(CPUPerformanceViewModel)) as CPUPerformanceViewModel;
            perfmod_freq = _vm.CpuFrequencyData;
        }

        public List<Metric> Metrics = [
            new Metric { Name = "CPU", Value = "0.0", Unit = "%" },
            new Metric { Name = "Memory", Value = "0.0", Unit = "MB" },
            new Metric { Name = "Disk", Value = "0.0", Unit = "%" },
            new Metric { Name = "Network", Value = "0.0", Unit = "KB/s" }
        ];
    }
}
