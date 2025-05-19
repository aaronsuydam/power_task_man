using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using PowerTaskMan.Common;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace power_task_man.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverviewPage : Page, INotifyPropertyChanged
    {
        // Default data for graph
        private IList<ICoordinatePair> _dataPoints;
        public IList<ICoordinatePair> DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataPoints)));
            }
        }
        public OverviewPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            DataPoints = new List<ICoordinatePair>
            {
                new CoordinatePair { X = 0, Y = 0 },
                new CoordinatePair { X = 1, Y = 1 },
                new CoordinatePair { X = 2, Y = 2 },
                new CoordinatePair { X = 3, Y = 3 },
                new CoordinatePair { X = 4, Y = 4 },
                new CoordinatePair { X = 5, Y = 5 },
                new CoordinatePair { X = 6, Y = 6 },
                new CoordinatePair { X = 7, Y = 7 },
                new CoordinatePair { X = 8, Y = 8 },
                new CoordinatePair { X = 9, Y = 9 },
                new CoordinatePair { X = 10, Y = 10 }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void DataChange_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var newDataPoints = new List<ICoordinatePair>();
            for (int i = 0; i < 1000; i++)
            {
                newDataPoints.Add(new CoordinatePair { X = i, Y = (float)(Math.Sin(i) * 10 + 10) });
                DataPoints = new List<ICoordinatePair>(newDataPoints); // Update DataPoints to trigger property-changed callback
                await Task.Delay(50); // Wait for 1 second before adding the next point
            }
        }
    }
}
