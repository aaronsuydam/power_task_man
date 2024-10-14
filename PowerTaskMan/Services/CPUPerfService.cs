using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Dispatching;
using System;
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
    internal partial class CPUPerfService : ObservableObject
    {

        [ObservableProperty]
        UInt32 current_frequency = 0;

        Collection<int> frequencyHistory = new();


        public ObservableCollection<ISeries> FrequencyHistoryChartSeries { get; set; } = new();



        CancellationTokenSource cpu_freq;
        PerformanceCounter cpuClockCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");


        UInt32 max_freq = 0;


        public CPUPerfService()
        {
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

        public void StartCPUFreqMonitoring()
        {
            cpu_freq = new();
            Task.Run(() =>
            {
                var dq = DispatcherQueue.GetForCurrentThread();
                while (true)
                {
                    if (cpu_freq.IsCancellationRequested)
                    {
                        return;
                    }
                    int clock_speed_mhz = QueryCPUFreq();
                    frequencyHistory.Add((int)clock_speed_mhz);
                    UpdateChart();
                    Debug.WriteLine("Current Clock Speed (MHz): " + clock_speed_mhz);
                    Thread.Sleep(100);
                }
                
            });
        }

        public void StopCPUFreqMonitoring()
        {
            cpu_freq.Cancel();
        }

        public int QueryCPUFreq()
        {
            float cpuPerformance = cpuClockCounter.NextValue();
            int frequency = (int)(cpuPerformance * max_freq);
            return frequency;
        }

        void UpdateChart()
        {
            if(FrequencyHistoryChartSeries.Count == 0)
            {
                FrequencyHistoryChartSeries.Add(
                    new LineSeries<int>
                    {
                        Values = frequencyHistory
                    }
                );
            }
            else
            {
                FrequencyHistoryChartSeries[0].Values = frequencyHistory;
            }


            
           
        }


    }
}
