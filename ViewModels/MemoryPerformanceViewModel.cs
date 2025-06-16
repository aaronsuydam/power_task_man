using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using PowerTaskMan.Common;
using PowerTaskMan.Controls;
using PowerTaskMan.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerTaskMan.ViewModels
{
    public partial class MemoryPerformanceViewModel : ObservableObject
    {
        public MemoryService memoryService { get; set; } = new();

        public int UpdateSpeedMilliseconds { get; set; } = 500;

        [ObservableProperty]
        Metric usedMemoryMB = new Metric { Name = "Used Memory", Unit = "MB", Value = "0" };

        [ObservableProperty]
        Metric availableMemory = new Metric { Name = "Available Memory", Unit = "MB", Value = "0" };

        [ObservableProperty]
        Metric totalMemory = new Metric { Name = "Total Memory", Unit = "MB", Value = "0" };

        [ObservableProperty]
        Metric usedMemoryPercent = new Metric { Name = "Used Memory Percent", Unit = "%", Value = "0" };

        private Task memory_monitor_loop;
        private CancellationTokenSource memory_cts;

        [ObservableProperty]
        List<ICoordinatePair> memoryUseChartSeries = new List<ICoordinatePair>(
        Enumerable.Range(0, 61).Select(_ => new CoordinatePair { X = 0, Y = 0 })
        );

        public MemoryPerformanceViewModel()
        {
            this.BeginMonitoring(DispatcherQueue.GetForCurrentThread());
        }

        public void BeginMonitoring(DispatcherQueue dq)
        {
            if (memory_monitor_loop != null)
            {
                return;
            }
            memory_monitor_loop = Task.Run(() =>
            {
                while (true)
                {
                    var mem_stats = this.memoryService.GetAllStats();
                    this.AvailableMemory.Value = mem_stats.AvailableMemoryMB.ToString();
                    this.TotalMemory.Value = mem_stats.TotalMemoryMB.ToString();
                    this.UsedMemoryMB.Value = mem_stats.UsedMemoryMB.ToString();
                    this.UsedMemoryPercent.Value = mem_stats.UsedMemoryPercent.ToString();
                    dq.TryEnqueue(() =>
                    {
                        UpdateMemoryChartData();
                    });
                    Task.Delay(UpdateSpeedMilliseconds).Wait();
                }
            });
        }
        void UpdateMemoryChartData()
        {
            var values = MemoryUseChartSeries.Cast<CoordinatePair>().ToList();
            values.RemoveAt(0);
            values.Add(new CoordinatePair { X = 0, Y = float.Parse(UsedMemoryPercent.Value)});
            MemoryUseChartSeries = new List<ICoordinatePair>(values);
        }
    }
}
