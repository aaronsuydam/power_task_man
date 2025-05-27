using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{

    public class Metric
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
    }

    public sealed partial class MetricsTile : UserControl
    {
        public MetricsTile()
        {
            this.InitializeComponent();
        }

        public HorizontalAlignment SecondaryMetricsTextAlignment
        {
            get => (HorizontalAlignment)GetValue(SecondaryMetricsTextAlignmentProperty);
            set => SetValue(SecondaryMetricsTextAlignmentProperty, value);
        }

        public static readonly DependencyProperty SecondaryMetricsTextAlignmentProperty =
            DependencyProperty.Register(
                nameof(SecondaryMetricsTextAlignment),
                typeof(HorizontalAlignment),
                typeof(MetricsTile),
                new PropertyMetadata(HorizontalAlignment.Stretch));
    
    }
}
