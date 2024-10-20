using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Dispatching;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore;
using System.Collections.ObjectModel;

namespace PowerTaskMan.Services
{
    public partial class MemoryService : ObservableObject
    {
        PerformanceCounter availableMemory = new PerformanceCounter("Memory", "Available MBytes");

        CancellationTokenSource memoryMonitoring = new();

        public ObservableCollection<ISeries> MemoryUseChartSeries { get; set; } = new();
        Collection<int> memoryUseHistory = new();


        [ObservableProperty]
        int availableMemoryMB = 0;

        [ObservableProperty]
        int totalMemoryMB = 0;

        [ObservableProperty]
        int usedMemoryMB = 0;

        [ObservableProperty]
        int usedMemoryPercent = 0;

        [ObservableProperty]
        string usedMemoryMBString = "";


        public MemoryService()
        {
            ObjectQuery query = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();

            // Iterate through each memory module and display the capacity
            ulong totalCapacity = 0;

            foreach (ManagementObject result in results)
            {
                // Capacity is in bytes, so divide by (1024 * 1024 * 1024) to convert to GB
                ulong capacityBytes = (ulong)result["Capacity"];
                totalCapacity += capacityBytes;
                totalMemoryMB = (int)(totalCapacity / (1024 * 1024));

                Console.WriteLine($"Capacity: {capacityBytes / (1024 * 1024 * 1024)} GB");
            }
        }

        public void StartMemoryMonitoring(DispatcherQueue dq)
        {
            Task.Run(() =>
            {
                while (!memoryMonitoring.Token.IsCancellationRequested)
                {
                    dq.TryEnqueue(() =>
                    {
                        MemoryStatsMonitor();
                        memoryUseHistory.Add(UsedMemoryPercent);
                        UsedMemoryMBString = UsedMemoryMB.ToString() + " MB";
                        UpdateChart();
                    });
                    Debug.WriteLine("Memory Stats: " + usedMemoryMB + "MB / " + totalMemoryMB + "MB (" + usedMemoryPercent + "%)");
                    Thread.Sleep(500);
                }
            });

        }

        public void StopMemoryMonitoring()
        {
            memoryMonitoring.Cancel();
        }

        private void MemoryStatsMonitor()
        {
            AvailableMemoryMB = (int)availableMemory.NextValue();
            UsedMemoryMB = TotalMemoryMB - AvailableMemoryMB;
            UsedMemoryPercent = (int)(((double)UsedMemoryMB / (double)TotalMemoryMB) * 100);
        }


        void UpdateChart()
        {
            if (MemoryUseChartSeries.Count == 0)
            {
                MemoryUseChartSeries.Add(
                    new LineSeries<int>
                    {
                        Values = memoryUseHistory,
                        Stroke = new SolidColorPaint(SKColor.Parse("FFAD1EFE")) { StrokeThickness = 2 },
                        Fill = new SolidColorPaint(SKColor.Parse("80AD1EFE")),
                        GeometryFill = new SolidColorPaint(SKColor.Parse("FFAD1EFE")),
                        GeometryStroke = new SolidColorPaint(SKColor.Parse("FFAD1EFE")),
                        GeometrySize = 5,
                        LineSmoothness = 0.1
                    }
                );
            }
            else
            {
                MemoryUseChartSeries[0].Values = memoryUseHistory.Select(x => x).TakeLast(60).ToArray();
            }




        }

    }
}
