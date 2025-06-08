using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerTaskMan.ViewModels;
using System.Collections.Generic;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{

    public partial class Metric : ObservableObject
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string value;

        [ObservableProperty]
        string unit;
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

        private void PerCoreMetricsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is Border border)
            {
                // create a fresh ThemeShadow per-item (or pull from Resources)
                var shadow = new ThemeShadow();
                // cast onto the repeater’s panel
                shadow.Receivers.Add(sender);
                border.Shadow = shadow;

                // push out along Z
                border.Translation = new Vector3(0, 0, 10);
            }
        }
    }
}
