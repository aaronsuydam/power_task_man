using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTaskMan.Common
{
    public class CPUCore
    {
        public int CoreNumber { get; set; } = 0;

        /// <summary>
        /// The frequency of the core in Hz
        /// </summary>
        public UInt64 CoreFrequency { get; set; } = 0;
        public UInt64 CoreUtilizationPercent { get; set; } = 0;
        public UInt64 CoreUtilizationPercentKernel { get; set; } = 0;
        public UInt64 CoreUtilizationPercentUser { get; set; } = 0;

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
