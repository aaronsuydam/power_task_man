using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTaskMan.Common
{
    public partial class PerCoreMetric : ObservableObject
    {
        [ObservableProperty]
        public List<ICoordinatePair> frequencyData;

        [ObservableProperty]
        public List<int> utilizationData;

        public int CoreNumber { get; set; }

        [ObservableProperty]
        public ICoordinatePair latestFrequency;

        [ObservableProperty]
        public int latestUtilization;
    }
}
