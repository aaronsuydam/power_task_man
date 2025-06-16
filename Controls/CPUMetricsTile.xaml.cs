using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using PowerTaskMan.ViewModels;
using System;
using System.Numerics;
using Microsoft.UI;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{

    public partial class Metric : ObservableObject
    {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        string value;

        [ObservableProperty]
        string unit;
    }

    public sealed partial class CPUMetricsTile : UserControl
    {

        private Compositor _compositor;
        private ContainerVisual _containerVisual;
        private SpriteVisual _spotlightVisual;
        private CompositionRadialGradientBrush _radialBrush;
        private readonly CPUPerformanceViewModel _cpuVM;

        public CPUMetricsTile()
        {
            InitializeComponent();
            _cpuVM = (CPUPerformanceViewModel)App.ServiceProvider
                       .GetService(typeof(CPUPerformanceViewModel))
                     as CPUPerformanceViewModel;
            DataContext = _cpuVM;

            //PointerEntered += OnPointerEntered;
            //PointerExited += OnPointerExited;
            //PointerMoved += OnPointerMoved;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize Composition spotlight
            var rootVisual = ElementCompositionPreview.GetElementVisual(RootGrid);
            _compositor = rootVisual.Compositor;
            _containerVisual = _compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(RootGrid, _containerVisual);

            _spotlightVisual = _compositor.CreateSpriteVisual();
            _spotlightVisual.Size = new Vector2(80, 80);
            _spotlightVisual.Opacity = 0f;

            _radialBrush = _compositor.CreateRadialGradientBrush();
            _radialBrush.CenterPoint = new Vector2(0.5f, 0.5f);
            _radialBrush.EllipseRadius = new Vector2(0.5f, 0.5f);
            CompositionColorGradientStop stop1 = _compositor.CreateColorGradientStop();
            stop1.Color = Colors.Blue;
            stop1.Offset = 0f;

            CompositionColorGradientStop stop2 = _compositor.CreateColorGradientStop();
            stop2.Color = Colors.Red;
            stop2.Offset = 0f;
            _radialBrush.ColorStops.Add(stop1);
            _radialBrush.ColorStops.Add(stop2);

            _spotlightVisual.Brush = _radialBrush;
            _containerVisual.Children.InsertAtTop(_spotlightVisual);
        }

        //private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    var pt = e.GetCurrentPoint(this).Position;
        //    _spotlightVisual.Offset = new Vector3(
        //        (float)pt.X - _spotlightVisual.Size.X / 2,
        //        (float)pt.Y - _spotlightVisual.Size.Y / 2,
        //        0);

        //    Debug.WriteLine($"Pointer position: X:{pt.X}, Y:{pt.Y}");
        //}

        //private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    var fadeIn = _compositor.CreateScalarKeyFrameAnimation();
        //    fadeIn.InsertKeyFrame(1f, 0.4f);
        //    fadeIn.Duration = TimeSpan.FromMilliseconds(1000);
        //    _spotlightVisual.StartAnimation(nameof(_spotlightVisual.Opacity), fadeIn);
        //    Debug.WriteLine("Animating");
        //}

        //private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    var fadeOut = _compositor.CreateScalarKeyFrameAnimation();
        //    fadeOut.InsertKeyFrame(1f, 0f);
        //    fadeOut.Duration = TimeSpan.FromMilliseconds(1000);
        //    _spotlightVisual.StartAnimation(nameof(_spotlightVisual.Opacity), fadeOut);
        //}


        private void PerCoreMetricsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is Border border)
            {
                // create a fresh ThemeShadow per-item (or pull from Resources)
                var shadow = new ThemeShadow();
                // cast onto the repeater’s panel
                shadow.Receivers.Add(sender);
                border.Shadow = shadow;

                // push out along Z
                border.Translation = new Vector3(0, 0, 10);
            }
        }
    }
}
