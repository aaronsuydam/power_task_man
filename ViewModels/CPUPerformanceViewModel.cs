﻿using CommunityToolkit.Mvvm.ComponentModel;
using dotPerfStat.Types;
using Microsoft.UI.Dispatching;
using PowerTaskMan.Common;
using PowerTaskMan.Controls;
using PowerTaskMan.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PowerTaskMan.ViewModels
{
    public partial class CPUPerformanceViewModel : ObservableObject
    {

        public CPUPerfService cpuPerfService { get; private set; }
        public int UpdateSpeedMilliseconds { get; set; } = 1000;


        private Task cpu_monitor_loop;

        private CancellationTokenSource cpu_cts;

        // For now we have to use a hardcoded list of primary metrics
        // 0. Utilization 
        // 1. Current Frequency (GHz)
        // 2. Current Temperature
        public ObservableCollection<Metric> PrimaryMetrics = new ObservableCollection<Metric>
        {
            new Metric { Name = "Utilization", Value = "0%", Unit = "%" },
            new Metric { Name = "Current Frequency", Value = "0 GHz", Unit = "GHz" },
            new Metric { Name = "Current Temperature", Value = "0 °C", Unit = "°C" }
        };

        [ObservableProperty]
        List<ICoordinatePair> cpuFrequencyData = new(
            Enumerable.Range(0, 121).Select(_ => new CoordinatePair { X = 0, Y = 0 })
        );


        public ObservableCollection<PerCoreMetric> PerCoreFrequencyData;

        
        public CPUPerformanceViewModel(ICPUPerfService cpu_serv)
        {
            this.cpuPerfService = (CPUPerfService)cpu_serv;
            PerCoreFrequencyData = new ObservableCollection<PerCoreMetric>(
                cpuPerfService.Cores.Select(
                    (core, index) =>
                    {
                        return new PerCoreMetric
                        {
                            FrequencyData = new List<ICoordinatePair>(
                                Enumerable.Range(0, 31).Select(_ => new CoordinatePair { X = 0, Y = 0 })
                            ),
                            CoreNumber = index
                        };
                    }
                ).ToList()
            );

            this.StartMonitoring(DispatcherQueue.GetForCurrentThread());
        }

        public void StartMonitoring(DispatcherQueue dq)
        {
            if (cpu_monitor_loop != null)
            {
                return;
            }

            cpu_cts = new CancellationTokenSource();

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
                var newfreq = (u32)cpuPerfService.CurrentFrequency;

                Debug.WriteLine("Current Clock Speed (MHz): " + newfreq);
                dq.TryEnqueue(() =>
                {
                    UpdatePrimaryMetrics(newfreq);
                    UpdatePerCoreFrequencyData(data);
                });
                Thread.Sleep(UpdateSpeedMilliseconds);
            }
        }

        public void StopCPUFreqMonitoring()
        {
            cpu_cts.Cancel();
        }

        void UpdatePrimaryMetrics(u32 new_clock_mhz)
        {
            // Update CPU-wide frequency information
            var values = cpuFrequencyData.Cast<CoordinatePair>().ToList();
            values.RemoveAt(0);
            values.Add(new CoordinatePair { X = 0, Y = new_clock_mhz });
            cpuFrequencyData = new List<ICoordinatePair>(values);

            // Update primary statistics
            PrimaryMetrics[0].Value = cpuPerfService.CurrentUtilization.ToString() + "%";
            PrimaryMetrics[1].Value = cpuPerfService.CurrentFrequency.ToString() + " MHz";

        }

        void UpdatePerCoreFrequencyData(List<StreamingCorePerfData> new_data)
        {
            foreach ((PerCoreMetric coreData, int index) in PerCoreFrequencyData.Select((value, i) => (value, i)))
            {
                // Skip the first element to shift the list left by 1.
                var values = coreData.FrequencyData.Cast<CoordinatePair>().Skip(1).ToList();
                var new_freq = new_data[index].Frequency / 1000 / 1000;
                float newX = values.Count > 0 ? values.Last().X + 1 : 0;
                values.Add(new CoordinatePair { X = newX, Y = new_freq });
                coreData.FrequencyData = values.Cast<ICoordinatePair>().ToList();
                coreData.LatestFrequency = coreData.FrequencyData.Last();
                coreData.LatestUtilization = (int)new_data[index].UtilizationPercent;
            }
        }
    }
}

