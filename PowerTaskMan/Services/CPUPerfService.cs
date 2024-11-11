﻿using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Dispatching;
using PowerTaskMan.Common;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace power_task_man.Services
{
    public partial class CPUPerfService
    {
        Collection<int> frequencyHistory = new();

        public ObservableCollection<ISeries> FrequencyHistoryChartSeries { get; set; } = new();

        CancellationTokenSource cpu_freq;

        public List<CPUCore> cores;

        UInt32 max_freq = 0;

        public CPUPerfService()
        {
            int core_count = Environment.ProcessorCount;

            cores = new List<CPUCore>();
            for (int i = 0; i < core_count; i++)
            {
                cores.Add(new CPUCore(i));
            }

            // Retrieve the maximum clock speed of the cpu from WMI and store it
            // Create a ManagementObjectSearcher to query Win32_Processor
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select MaxClockSpeed, CurrentClockSpeed, Name from Win32_Processor");

            foreach (ManagementObject obj in searcher.Get())
            {
                Debug.WriteLine("Processor Name: " + obj["Name"]);
                Debug.WriteLine("Max Clock Speed: " + obj["MaxClockSpeed"] + " MHz");
                Debug.WriteLine("Current Clock Speed: " + obj["CurrentClockSpeed"] + " MHz");
                Debug.WriteLine("---------------------------------------");
                max_freq = (uint)obj["MaxClockSpeed"];
            }
        }

        /// <summary>
        /// Returns a list of the per-core CPU frequencies in kHz
        /// </summary>
        /// <returns></returns>
        public void UpdateFrequencies()
        {

            foreach (var core in cores)
            {
                float cpuPerformance = core.frequency.NextValue();
                UInt64 frequency_hz = ((UInt64)(cpuPerformance * max_freq * 10 * 1000)); // Frequency is in kHz
                core.CoreFrequency = frequency_hz;

                float utilization = core.utilization.NextValue();
                core.CoreUtilizationPercent = (UInt64)utilization;
            }

            

            // Return the result as a list of frequencies for each core (if needed)
        }



    }
}
