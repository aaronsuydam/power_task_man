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
using System.Collections.Generic;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PowerTaskMan.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace power_task_man.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public partial class PerformancePage : Page
    {

        public CPUPerfService cpuPerfService { get; set; } = new();
        public MemoryService memoryService { get; set; } = new();
        public string CPUFrequency { get; set; } = "Something";

        public List<Axis> XAxes {get; set;}
        public List<Axis> YAxes { get; set; }


        public PerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            SetupCPUChart();
            SetupMemoryChart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.cpuPerfService.StartCPUFreqMonitoring(this.DispatcherQueue);
        }

        private void SetupCPUChart()
        {
            var x_axis = new Axis
            {
                MaxLimit = 60,
                MinLimit = 0,
                Name = "Time (seconds)",
                NamePaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Aptos") }, // Title color and font

            };

            var y_axis = new Axis
            {
                MaxLimit = 6000,
                MinLimit = 3000,
                Name = "CPU Frequency (MHz)",
                NamePaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Aptos") }, // Title color and font
            };

            //FrequencyChart.XAxes = new List<Axis> { x_axis };
            //FrequencyChart.YAxes = new List<Axis> { y_axis };
        }

        private void SetupMemoryChart()
        {
            var x_axis = new Axis
            {
                MaxLimit = 60,
                MinLimit = 0,
                Name = "Time (seconds)",
                NamePaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Aptos") }, // Title color and font

            };

            this.XAxes = new List<Axis> { x_axis };
            

            var y_axis = new Axis
            {
                MaxLimit = 100,
                MinLimit = 0,
                Name = "Memory In Use (%)",
                NamePaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Aptos") }, // Title color and font
            };

            this.YAxes = new List<Axis> { y_axis };

            MemoryChart.XAxes = new List<Axis> { x_axis };
            MemoryChart.YAxes = new List<Axis> { y_axis };
        }

        private void MemoryButtonClick(object sender, RoutedEventArgs e)
        {
            memoryService.StartMemoryMonitoring(this.DispatcherQueue);
        }
    }
}
