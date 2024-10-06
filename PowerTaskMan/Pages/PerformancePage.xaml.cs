using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.Management;
using CommunityToolkit.Mvvm.ComponentModel;
using power_task_man.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using ScottPlot;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Storage;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using System.Diagnostics;

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
        public StorageFolder local = ApplicationData.Current.LocalFolder;

        public string ImageSourcePath { get; set; }

        public string CPUFrequency { get; set; } = "Something";

        public PerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            ImageSourcePath = $"{local.Path}\\freq_plot.bmp";
            this.cpuPerfService.StartCPUFreqMonitoring();

            FrequencyPlot.Plot.DataBackground.Color = Color.FromHex("#f3f3f3");
            FrequencyPlot.Plot.FigureBackground.Color = Color.FromHex("#f3f3f3");
            var axes = FrequencyPlot.Plot.Axes.GetAxes();
         

            //axes.ElementAt(0).IsVisible = false;
            //axes.ElementAt(3).IsVisible = false;

        

            Task.Run(() =>
            {
                while (true)
                {
                    UpdateChart();
                    Thread.Sleep(1000);
                }
            });


        }

        public void UpdateChart()
        {
           



            double[] yData = cpuPerfService.Frequency_history.Select(x => (double)x).ToArray();

            // Generate xData for the recent points
            double[] xData = Enumerable.Range(0, yData.Length)
                                        .Select(i => (double)i) // Assuming the spacing is 10 units
                                        .ToArray();

          

            DispatcherQueue.TryEnqueue(() =>
            {
                if(yData.Length < 10)
                {
                    return;
                }
                try
                {
                    // Add the scatter plot
                    FrequencyPlot.Plot.Add.Scatter(xData, yData);


                    // Set axis limits
                    FrequencyPlot.Plot.Axes.SetLimitsX(0, xData.Length * 10);
                    FrequencyPlot.Plot.Axes.SetLimitsY(4, 6);
                    CPUFrequencyLabel.Text = yData.Last().ToString() + " MHz";
                    FrequencyPlot.Refresh();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
            
            
        }




    }
}
