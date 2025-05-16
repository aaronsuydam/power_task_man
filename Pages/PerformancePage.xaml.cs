using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using LiveChartsCore.SkiaSharpView;
using System.Collections.Generic;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PowerTaskMan.ViewModels;
using PowerTaskMan.Common;
using Microsoft.UI.Xaml.Automation;
using PowerTaskMan.Controls;

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

        private void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
  
            //if (sender.TryGetElement(args.Index) is FrameworkElement element)
            //{
            //    element.SetValue(AutomationProperties.NameProperty, $"Core {args.Index}");
            //    var graph = element as GraphControlWin2D;
            //    if (graph != null)
            //    {
            //        graph.Title = $"Core {args.Index} Frequency (MHz)";
            //    }
            
            //}
        }
    }
}
