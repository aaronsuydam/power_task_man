using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using PowerTaskMan.Services;
using PowerTaskMan.Common;
using PowerTaskMan.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotPerfStat.Types;

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

        [ObservableProperty]
        List<CoreFrequencyData> perCoreCPUFrequencySeries = new List<CoreFrequencyData>();

        public PerformanceViewModel()
        {
            perCoreCPUFrequencySeries = cpuPerfService.Cores.Select((core, index) =>
            {
                return new CoreFrequencyData
                {
                    FrequencyData = new List<ICoordinatePair>(
                        Enumerable.Range(0, 121).Select(_ => new CoordinatePair { X = 0, Y = 0 })
                    ),
                    CoreNumber = index
                };
            }).ToList();
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

            cpu_utilization_loop = Task.Run(() =>
            {
                while (true)
                {
                    var new_data = cpuPerfService.UpdateCPUStats();
                    dq.TryEnqueue(() =>
                    {
                        UpdateCPUUtilizationChart();
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
                _cpu_monitoring_loop(dq);
            });
        }

        private void _cpu_monitoring_loop(DispatcherQueue dq)
        {
            while (!cpu_cts.IsCancellationRequested)
            {
                var data = cpuPerfService.UpdateCPUStats();

                // Compute the average frequency across all cores
                //int clock_speed_mhz = 0;
                //foreach (var core in cpuPerfService.cores)
                //{
                //    clock_speed_mhz += (int) core.CoreFrequency / 1000000;
                //}
                //clock_speed_mhz /= cpuPerfService.cores.Count;

                f32 avg_freq = data.Select(per_core => per_core.Frequency).Average();

                int clock_speed_mhz = (int)(avg_freq / 1000 / 1000);

                Debug.WriteLine("Current Clock Speed (MHz): " + clock_speed_mhz);
                dq.TryEnqueue(() =>
                {
                    UpdateCPUFrequencyChart(clock_speed_mhz);
                    UpdatePerCoreCPUFrequencyChart(data);
                    CurrentFrequency = ((double)clock_speed_mhz / 1000).ToString() + " GHz";
                });
                Thread.Sleep(UpdateSpeedMilliseconds);
            }
        }

        public void StopCPUFreqMonitoring()
        {
            cpu_cts.Cancel();
        }

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

        void UpdatePerCoreCPUFrequencyChart(List<StreamingCorePerfData> new_data)
        {
            foreach ((CoreFrequencyData coreData, int index) in PerCoreCPUFrequencySeries.Select((value, i) => (value, i)))
            {
                var values = coreData.FrequencyData.Cast<CoordinatePair>().ToList();
                values.RemoveAt(0);
                var new_freq = new_data[index].Frequency;
                new_freq = new_freq / 1000 / 1000;
                values.Add(new CoordinatePair { X = 0, Y = new_freq});
                PerCoreCPUFrequencySeries[index].FrequencyData = new List<ICoordinatePair>(values);
            }
        }

        void UpdateCPUUtilizationChart()
        {
            
        }
    }

    
}
