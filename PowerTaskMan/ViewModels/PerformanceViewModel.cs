using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using power_task_man.Services;
using PowerTaskMan.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PowerTaskMan.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject
    {
        public CPUPerfService cpuPerfService { get; set; } = new();
        public MemoryService memoryService { get; set; } = new();

        public string CPUFrequency { get; set; } = "Something";

        [ObservableProperty]
        string usedMemoryMBString = "";

        [ObservableProperty]
        int availableMemoryMB = 0;

        [ObservableProperty]
        int totalMemoryMB = 0;

        [ObservableProperty]
        int usedMemoryMB = 0;

        [ObservableProperty]
        int usedMemoryPercent = 0;


        public ObservableCollection<ISeries> MemoryUseChartSeries { get; set; } = new();

        public PerformanceViewModel()
        {
            
        }

        public void BeginMonitoring(DispatcherQueue dq)
        {

        }


        void UpdateMemoryChartData()
        {
            if (MemoryUseChartSeries.Count == 0)
            {
                MemoryUseChartSeries.Add(
                    new LineSeries<int>
                    {
                        Values = memoryService.memoryUseHistory.ToArray(),
                        Stroke = new SolidColorPaint(SKColor.Parse("FFAD1EFE")) { StrokeThickness = 2 },
                        Fill = new SolidColorPaint(SKColor.Parse("80AD1EFE")),
                        GeometryFill = new SolidColorPaint(SKColor.Parse("FFAD1EFE")),
                        GeometryStroke = new SolidColorPaint(SKColor.Parse("FFAD1EFE")),
                        GeometrySize = 5,
                        LineSmoothness = 0.1
                    }
                );
                UsedMemoryMBString = UsedMemoryMB.ToString() + " MB";

            }
            else
            {
                MemoryUseChartSeries[0].Values = memoryUseHistory.Select(x => x).TakeLast(60).ToArray();
                UsedMemoryMBString = UsedMemoryMB.ToString() + " MB";

            }




        }


    }
}



    }
}
