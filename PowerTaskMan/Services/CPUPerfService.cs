using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Dispatching;
using SkiaSharp;
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
        string currentFrequency = "";

        Collection<int> frequencyHistory = new();

        [ObservableProperty]
        int updateSpeedMilliseconds = 500;

        private Task CPUFrequencyMonitoringLoop;


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

        public void StartCPUFreqMonitoring(DispatcherQueue dq)
        {
            if(CPUFrequencyMonitoringLoop != null)
            {
                return;
            }

            cpu_freq = new();

            CPUFrequencyMonitoringLoop = Task.Run(() =>
            {
        
                while (true)
                {
                    if (cpu_freq.IsCancellationRequested)
                    {
                        return;
                    }
                    int clock_speed_khz = QueryCPUFreq();
                    if(clock_speed_khz != 0)
                    {
                        frequencyHistory.Add((int)clock_speed_khz);
                    }
                 
                    UpdateChart();
                
                    Debug.WriteLine("Current Clock Speed (MHz): " + clock_speed_khz);
                    dq.TryEnqueue(() =>
                    {
                        CurrentFrequency = ((double) (clock_speed_khz/1000)/1000).ToString() + " GHz";
                    });
                    Thread.Sleep(UpdateSpeedMilliseconds);
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
            int frequency = (int)(cpuPerformance * max_freq * 10); // Frequency is in kHz
            return frequency;
        }

        void UpdateChart()
        {
            if(FrequencyHistoryChartSeries.Count == 0)
            {
                FrequencyHistoryChartSeries.Add(
                    new LineSeries<int>
                    {
                        Values = frequencyHistory,
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
                FrequencyHistoryChartSeries[0].Values = frequencyHistory.Select(x => x / 1000).TakeLast(60).ToArray();
            }


            
           
        }


    }
}
