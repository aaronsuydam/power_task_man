using System;
using System.Diagnostics;

namespace PowerTaskMan.Common
{
    public class CPUCore
    {
        /// <summary>
        /// The frequency of the core in Hz
        /// </summary>
        public UInt64 CoreFrequency { get; set; } = 0;
        public float CoreUtilizationPercent { get; set; } = 0;
        public float CoreUtilizationPercentKernel { get; set; } = 0;
        public float CoreUtilizationPercentUser { get; set; } = 0;
        public int CoreNumber { get; set; } = 0;

        public PerformanceCounter frequency;
        public PerformanceCounter utilization;

        public CPUCore(int coreNumber)
        {
            CoreNumber = coreNumber;
            CoreFrequency = 0;
            CoreUtilizationPercent = 0;

            string counter_core_id = "0," + CoreNumber.ToString();

            frequency = new PerformanceCounter("Processor Information", "% Processor Performance", counter_core_id);
            utilization = new PerformanceCounter("Processor Information", "% Processor Time", counter_core_id);
        }

        public override string ToString()
        {
            return $"Core {CoreNumber}: {CoreFrequency} MHz, {CoreUtilizationPercent}%";
        }
    }
}
