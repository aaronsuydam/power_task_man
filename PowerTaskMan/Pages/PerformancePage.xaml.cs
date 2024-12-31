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
using PowerTaskMan.ViewModels;
using Windows.UI.ViewManagement;
using PowerTaskMan.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace power_task_man.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public partial class PerformancePage : Page
    {
        public string CPUFrequency { get; set; } = "Something";

        public PerformanceViewModel viewModel { get; set; } = new();

        public List<Axis> MemoryChartXAxes {get; set;}
        public List<Axis> MemoryChartYAxes { get; set; }

        public List<Axis> FrequencyChartXAxes { get; set; }
        public List<Axis> FrequencyChartYAxes { get; set; }


        public PerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;

            SetupCPUChart();
            SetupMemoryChart();
            this.viewModel.BeginMonitoring(this.DispatcherQueue);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.BeginMonitoring(this.DispatcherQueue);
        }

        private void SetupCPUChart()
        {
            SKColor font_color = ThemeHelpers.GetTextThemeColor();
            var x_axis = new Axis
            {
                MaxLimit = 60,
                MinLimit = 0,
                Name = "Time (seconds)",
                NamePaint = new SolidColorPaint(font_color) { SKTypeface = SKTypeface.FromFamilyName("Aptos") }, // Title color and font

            };

            var y_axis = new Axis
            {
                MaxLimit = 6000,
                MinLimit = 1000,
                ForceStepToMin = true,
                MinStep = 2000
            };

            FrequencyChartXAxes = new List<Axis> { x_axis };
            FrequencyChartYAxes = new List<Axis> { y_axis };
        }

        private void SetupMemoryChart()
        {
            SKColor font_color = ThemeHelpers.GetTextThemeColor();
            
            var x_axis = new Axis
            {
                MaxLimit = 60,
                MinLimit = 0,
                Name = "Time (seconds)",
                NamePaint = new SolidColorPaint(font_color) { SKTypeface = SKTypeface.FromFamilyName("Aptos") }, // Title color and font

            };

            this.MemoryChartXAxes = new List<Axis> { x_axis };
            

            var y_axis = new Axis
            {
                MaxLimit = 100,
                MinLimit = 0,
            };

            this.MemoryChartYAxes = new List<Axis> { y_axis };
        }

        private void MemoryButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.BeginMonitoring(this.DispatcherQueue);
        }
    }
}
