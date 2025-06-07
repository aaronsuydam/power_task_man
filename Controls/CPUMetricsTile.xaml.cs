using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerTaskMan.ViewModels;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{

    public class Metric
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
    }

    public sealed partial class CPUMetricsTile : UserControl
    {

        private readonly CPUPerformanceViewModel _cpuPerformanceViewModel;

        public CPUMetricsTile()
        {
            this.InitializeComponent();

            _cpuPerformanceViewModel = App.ServiceProvider.GetService(typeof(CPUPerformanceViewModel)) as CPUPerformanceViewModel;
        
            this.DataContext = _cpuPerformanceViewModel;
        }


       
        
         

    }
}
