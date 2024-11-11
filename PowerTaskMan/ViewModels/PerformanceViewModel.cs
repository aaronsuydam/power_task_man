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
using System.Diagnostics;
using System.Threading;

namespace PowerTaskMan.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject
    {
        public CPUPerfService cpuPerfService { get; set; } = new();
        public MemoryService memoryService { get; set; } = new();

        public int UpdateSpeedMilliseconds { get; set; } = 500;

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

        [ObservableProperty]
        string currentFrequency = "0.0 GHz";




        private Task memory_monitor_loop;
        private Task cpu_monitor_loop;

        private CancellationTokenSource cpu_cts;
        private CancellationTokenSource memory_cts;


        public ObservableCollection<ISeries> FrequencyHistoryChartSeries { get; set; } = new();

        public ObservableCollection<ISeries> MemoryUseChartSeries { get; set; } = new();

        public PerformanceViewModel()
        {
            
        }

        public void BeginMonitoring(DispatcherQueue dq)
        {
            if (memory_monitor_loop != null || cpu_monitor_loop != null)
            {
                return;
            }
            memory_monitor_loop = Task.Run(() =>
            {
                while (true)
                {
                    var mem_stats = this.memoryService.GetAllStats();
                    this.AvailableMemoryMB = mem_stats.AvailableMemoryMB;
                    this.TotalMemoryMB = mem_stats.TotalMemoryMB;
                    this.UsedMemoryMB = mem_stats.UsedMemoryMB;
                    this.UsedMemoryPercent = mem_stats.UsedMemoryPercent;
                    dq.TryEnqueue(() =>
                    {
                        UpdateMemoryChartData();
                    });
                    Task.Delay(500).Wait();
                }
            });

            StartCPUFreqMonitoring(dq);
        }

        public void StartCPUFreqMonitoring(DispatcherQueue dq)
        {
            if (cpu_monitor_loop != null)
            {
                return;
            }

            cpu_cts = new();

            cpu_monitor_loop = Task.Run(() =>
            {

                while (true)
                {
                    if (cpu_cts.IsCancellationRequested)
                    {
                        return;
                    }

                    cpuPerfService.UpdateFrequencies();
                    int clock_speed_mhz = (int)(cpuPerfService.cores[0].CoreFrequency / 1000 / 1000);

                    Debug.WriteLine("Current Clock Speed (MHz): " + clock_speed_mhz);
                    dq.TryEnqueue(() =>
                    {
                        UpdateCPUFrequencyChart(clock_speed_mhz);
                        CurrentFrequency = ((double)clock_speed_mhz / 1000).ToString() + " GHz";
                    });
                    Thread.Sleep(UpdateSpeedMilliseconds);
                }

            });
        }

        //public void StopCPUFreqMonitoring()
        //{
        //    cpu_freq.Cancel();
        //}



        void UpdateMemoryChartData()
        {
            if (MemoryUseChartSeries.Count == 0)
            {
                MemoryUseChartSeries.Add(
                    new LineSeries<int>
                    {
                        Values = new int[60],
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
                var values = MemoryUseChartSeries[0].Values.Cast<int>().ToList();
                values.RemoveAt(0);
                values.Add(UsedMemoryPercent);
                MemoryUseChartSeries[0].Values = values;
                UsedMemoryMBString = UsedMemoryMB.ToString() + " MB";
            }

        }

        void UpdateCPUFrequencyChart(int new_clock_mhz)
        {
            if (FrequencyHistoryChartSeries.Count == 0)
            {
                FrequencyHistoryChartSeries.Add(
                    new LineSeries<int>
                    {
                        Values = new int[60],
                        Stroke = new SolidColorPaint(SKColor.Parse("2196f3")) { StrokeThickness = 2 },
                        Fill = null,
                        GeometryFill = new SolidColorPaint(SKColor.Parse("2196f3")),
                        GeometryStroke = new SolidColorPaint(SKColor.Parse("2196f3")),
                        GeometrySize = 5,
                        LineSmoothness = 0.1
                    }
                );
            }
            else
            {
                var values = FrequencyHistoryChartSeries[0].Values.Cast<int>().ToList();
                values.RemoveAt(0);
                values.Add(new_clock_mhz);
                FrequencyHistoryChartSeries[0].Values = values;
            }




        }

    }
}
