using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using PowerTaskMan.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace power_task_man.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public partial class PerformancePage : Page
    {
        public string CPUFrequency { get; set; } = "Something";

        public PerformanceViewModel viewModel { get; set; } = new();


        public PerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;

            this.viewModel.BeginMonitoring(this.DispatcherQueue);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.BeginMonitoring(this.DispatcherQueue);
        }

      
        private void MemoryButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.BeginMonitoring(this.DispatcherQueue);
        }

        private void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
  
            //if (sender.TryGetElement(args.Index) is FrameworkElement element)
            //{
            //    element.SetValue(AutomationProperties.NameProperty, $"Core {args.Index}");
            //    var graph = element as GraphControlWin2D;
            //    if (graph != null)
            //    {
            //        graph.Title = $"Core {args.Index} Frequency (MHz)";
            //    }
            
            //}
        }
    }
}
