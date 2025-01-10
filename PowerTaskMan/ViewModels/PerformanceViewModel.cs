using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Dispatching;
using power_task_man.Services;
using PowerTaskMan.Common;
using PowerTaskMan.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerTaskMan.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject
    {
        public CPUPerfService cpuPerfService { get; set; } = new();
        public MemoryService memoryService { get; set; } = new();

        public int UpdateSpeedMilliseconds { get; set; } = 250;

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
        private Task cpu_utilization_loop;

        private CancellationTokenSource cpu_cts;
        private CancellationTokenSource memory_cts;


        [ObservableProperty]
        List<ICoordinatePair> cpuFrequencyChartSeries = new List<ICoordinatePair>(
            Enumerable.Range(0, 121).Select(_ => new CoordinatePair { X = 0, Y = 0 })
        );

        [ObservableProperty]
        List<ICoordinatePair> memoryUseChartSeries = new List<ICoordinatePair>(
            Enumerable.Range(0, 61).Select(_ => new CoordinatePair { X = 0, Y = 0 })
        );

        public ObservableCollection<ObservableCollection<ISeries>> UtilizationChartSeriesCollection { get; set; }

        public PerformanceViewModel()
        {
            UtilizationChartSeriesCollection = new ObservableCollection<ObservableCollection<ISeries>>(
                Enumerable.Range(0, cpuPerfService.cores.Count()).Select(_ => new ObservableCollection<ISeries>())
            );
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
                    Task.Delay(250).Wait();
                }
            });

            cpu_utilization_loop = Task.Run(() =>
            {
                while (true)
                {
                    cpuPerfService.UpdateUtilizations();
                    dq.TryEnqueue(() =>
                    {
                        UpdateCPUUtilizationChart();
                    });
                    Task.Delay(250).Wait();
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
            var values = MemoryUseChartSeries.Cast<CoordinatePair>().ToList();
            values.RemoveAt(0);
            values.Add(new CoordinatePair { X = 0, Y = UsedMemoryPercent });
            MemoryUseChartSeries = new List<ICoordinatePair>(values);
            UsedMemoryMBString = UsedMemoryMB.ToString() + " MB";
        }

        void UpdateCPUFrequencyChart(int new_clock_mhz)
        {
            var values = CpuFrequencyChartSeries.Cast<CoordinatePair>().ToList();
            values.RemoveAt(0);
            values.Add(new CoordinatePair { X = 0, Y = new_clock_mhz });
            CpuFrequencyChartSeries = new List<ICoordinatePair>(values);
            CurrentFrequency = new_clock_mhz.ToString() + " GHz";
        }

        void UpdateCPUUtilizationChart()
        {
            
        }
    }
}
