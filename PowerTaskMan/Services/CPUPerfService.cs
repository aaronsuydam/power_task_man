using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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

        [ObservableProperty]
        List<UInt32> frequency_history = new();



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
                while (true)
                {
                    if (cpu_freq.IsCancellationRequested)
                    {
                        return;
                    }
                    int clock_speed_mhz = QueryCPUFreq();
                    frequency_history.Add((uint)clock_speed_mhz);
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
    }
}
