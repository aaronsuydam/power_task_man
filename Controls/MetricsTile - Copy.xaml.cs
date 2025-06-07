using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerTaskMan.ViewModels;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{
    public sealed partial class MetricsTile : UserControl
    {

        private readonly PerformanceViewModel _performanceViewModel;

        public MetricsTile()
        {
            this.InitializeComponent();

            _performanceViewModel = App.ServiceProvider.GetService(typeof(PerformanceViewModel)) as PerformanceViewModel;
        
            this.DataContext = _performanceViewModel;
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

        public IList<Metric> SecondaryMetrics
        {
            get => (IList<Metric>)GetValue(SecondaryMetricsProperty);
            set => SetValue(SecondaryMetricsProperty, value);
        }

        public static readonly DependencyProperty SecondaryMetricsProperty =
            DependencyProperty.Register(
                nameof(SecondaryMetrics),
                typeof(IList<Metric>),
                typeof(MetricsTile),
                new PropertyMetadata(new List<Metric>(), null)
                );
        
         

    }
}
