using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PowerTaskMan.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{
    public sealed partial class MetricsGraphTile : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title), 
                typeof(string),
                typeof(MetricsGraphTile), 
                new PropertyMetadata(string.Empty)
                );

        public static readonly DependencyProperty PrimaryMetricInstantaneousValueProperty =
            DependencyProperty.Register(
                nameof(PrimaryMetricInstantaneousValue), 
                typeof(object),
                typeof(MetricsGraphTile),
                new PropertyMetadata(0.0)
                );

        public static readonly DependencyProperty SecondaryMetricInstantaneousValueProperty =
            DependencyProperty.Register(
                nameof(SecondaryMetricInstantaneousValue),
                typeof(object),
                typeof(MetricsGraphTile),
                new PropertyMetadata(0.0)
                );

        public static readonly DependencyProperty DataPointsProperty =
            DependencyProperty.Register(
                nameof(DataPoints),
                typeof(IList<ICoordinatePair>),
                typeof(MetricsGraphTile),
                new PropertyMetadata(new List<double>())
                );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object PrimaryMetricInstantaneousValue
        {
            get => GetValue(PrimaryMetricInstantaneousValueProperty);
            set => SetValue(PrimaryMetricInstantaneousValueProperty, value);
        }

        public object SecondaryMetricInstantaneousValue
        {
            get => GetValue(SecondaryMetricInstantaneousValueProperty);
            set => SetValue(SecondaryMetricInstantaneousValueProperty, value);
        }

        public string PrimaryMetricDisplayValue
        {
            get => PrimaryMetricInstantaneousValue?.ToString() ?? string.Empty;
        }
        public string SecondaryMetricDisplayValue
        {
            get => SecondaryMetricInstantaneousValue?.ToString() ?? string.Empty;
        }

        public IList<ICoordinatePair> DataPoints
        {
            get => (IList<ICoordinatePair>)GetValue(DataPointsProperty);
            set => SetValue(DataPointsProperty, value);
        }

        public MetricsGraphTile()
        {
            this.InitializeComponent();
        }
    }
}
