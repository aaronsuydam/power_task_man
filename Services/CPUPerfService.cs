using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using dotPerfStat;
using dotPerfStat.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerTaskMan.Common;
using System.Linq;

namespace PowerTaskMan.Services
{
    
    
    public interface ICPUPerfService
    {
        public float CurrentFrequency { get; set; }
        public u8 CurrentUtilization { get; set; }
        public List<WinCPUCore> Cores { get; }
        public List<StreamingCorePerfData> UpdateCPUStats();
    }
    
    public partial class CPUPerfService : ICPUPerfService
    {

        private List<WinCPUCore> _cores = new();
        private IObserver<IStreamingCorePerfData> _perfDataObserver;
        private List<IDisposable> _corePerfSubscriptions;
        
        Collection<int> frequencyHistory = new();

        CancellationTokenSource cpu_freq;

        public List<WinCPUCore> Cores => _cores;
        public float CurrentFrequency { get; set; } = 0.0f;
        public u8 CurrentUtilization { get; set; } = 0;

        UInt32 max_freq = 0;

        public CPUPerfService()
        {
            int core_count = Environment.ProcessorCount;

            for (int i = 0; i < core_count; i++)
            {
                WinCPUCore newCore = new((u8)i);
                _cores.Add(newCore);
            }

        }

        /// <summary>
        /// Updates the frequency for each core in the system.
        /// </summary>
        /// <returns></returns>
        public List<StreamingCorePerfData> UpdateCPUStats()
        {
            List<StreamingCorePerfData> newStats = new();
            foreach (var core in Cores)
            {
                var updated_core_stats = core.Update();
                newStats.Add(updated_core_stats);
            }
            CurrentFrequency = newStats.Select(coredata => coredata.Frequency).Average() / 1000000.0f; // Convert to GHz
            CurrentFrequency = newStats.Select(coredata => (float)coredata.UtilizationPercent).Average(); // Average utilization percent
            return newStats;
        }

      



    }
}
