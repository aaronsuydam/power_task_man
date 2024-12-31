using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using PowerTaskMan.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PowerTaskMan.Controls
{
    public sealed partial class GraphControlWin2D : UserControl
    {
        public static readonly DependencyProperty AxesColorProperty = DependencyProperty.Register(
            "AxesColor", 
            typeof(Color),
            typeof(GraphControlWin2D),
            new PropertyMetadata(Colors.Gray, OnAxesColorPropertyChanged));

        // Axis Margin from edge of control dep prop
        public static readonly DependencyProperty AxisMarginProperty = DependencyProperty.Register(
            "AxisMargin",
            typeof(float),
            typeof(GraphControlWin2D),
            new PropertyMetadata(10.0f, OnAxisMarginPropertyChanged));

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register(
            "DataPoints",
            typeof(IList<ICoordinatePair>),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new List<ICoordinatePair>(), OnDataPointsPropertyChanged));

        public static readonly DependencyProperty UseIndexBasedGraphingProperty = DependencyProperty.Register(
            "UseIndexBasedGraphing",
            typeof(bool),
            typeof(GraphControlWin2D),
            new PropertyMetadata(false));

        public Color AxesColor
        {
            get { return (Color)GetValue(AxesColorProperty); }
            set { SetValue(AxesColorProperty, value); }
        }
        public float AxisMargin
        {
            get { return (float)GetValue(AxisMarginProperty); }
            set { SetValue(AxisMarginProperty, value); }
        }

        // Data points
        public IList<ICoordinatePair> DataPoints
        {
            get { return (IList<ICoordinatePair>)GetValue(DataPointsProperty); }
            set { SetValue(DataPointsProperty, value); }
        }

        public bool UseIndexBasedGraphing
        {
            get { return (bool)GetValue(UseIndexBasedGraphingProperty); }
            set { SetValue(UseIndexBasedGraphingProperty, value); }
        }

        private IList<bool> refresh_request_cache = new List<bool>();
           
   

        public GraphControlWin2D()
        {
            this.InitializeComponent();

            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(10).Wait();
                    if (refresh_request_cache.Count > 0)
                    {
                        refresh_request_cache.Clear();
                        Canvas.Invalidate();
                    }
                }
            });
        }

        private static void OnAxesColorPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.AxesColor = (Color)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnDataPointsPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.DataPoints = (IList<ICoordinatePair>)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnAxisMarginPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.AxisMargin = (float)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        public void Invalidate()
        {
            Canvas.Invalidate();
        }

        void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;

            // Clear the canvas
            session.Clear(Colors.White);

            if (DataPoints == null || DataPoints.Count < 2)
                return;

            // Draw axes
            var width = (float)sender.ActualWidth;
            var height = (float)sender.ActualHeight;
            var margin = AxisMargin;
            session.DrawLine(margin, margin, margin, height - margin, AxesColor, 2);
            session.DrawLine(margin, height - margin, width - margin, height - margin, AxesColor, 2);

            // Calculate the graph's origin
            float origin_x = 0 + margin;
            float origin_y = 0 - margin + height;

            // This control will support two modes, one being the default mode where the data points are drawn relative to the origin of the graph
            // and users can specify the x and y values of the data points. The other mode will be where the x values of the data points are the
            // indices of the data points in the list and the y values are the values of the data points in the list. This second mode will be 
            // active when UseIndexBasedGraphing is set to true.
            if (UseIndexBasedGraphing)
            {
                // Create a new list, this will be the actual data that we will graph.
                var data = new List<ICoordinatePair>();
                for (int i = 0; i < DataPoints.Count; i++)
                {
                    data.Add(new CoordinatePair { X = i, Y = DataPoints[i].Y });
                }

                for (int i = 0; i < data.Count - 1; i++)
                {
                    var x1 = origin_x + data[i].X;
                    var y1 = origin_y - data[i].Y;
                    var x2 = origin_x + data[i + 1].X;
                    var y2 = origin_y - data[i + 1].Y;
                    session.DrawLine(x1, y1, x2, y2, Colors.Black, 2);
                }
            }
            else
            { 
                // Draw the data points relative to the origin of the graph.
                for (int i = 0; i < DataPoints.Count - 1; i++)
                {
                    var x1 = origin_x + DataPoints[i].X;
                    var y1 = origin_y - DataPoints[i].Y;
                    var x2 = origin_x + DataPoints[i + 1].X;
                    var y2 = origin_y - DataPoints[i + 1].Y;
                    session.DrawLine(x1, y1, x2, y2, Colors.Black, 2);
                }
            }
        }
    }
}
