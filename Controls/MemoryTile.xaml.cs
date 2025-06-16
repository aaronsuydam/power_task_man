using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using PowerTaskMan.ViewModels;
using System.Numerics;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{


    public sealed partial class MemoryTile : UserControl
    {

    
        private readonly MemoryPerformanceViewModel _memoryVM;

        public MemoryTile()
        {
            InitializeComponent();
            _memoryVM = (MemoryPerformanceViewModel)App.ServiceProvider
                        .GetService(typeof(MemoryPerformanceViewModel));
            DataContext = _memoryVM;

        }

 


    }
}
