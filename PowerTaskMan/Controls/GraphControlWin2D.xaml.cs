using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using PowerTaskMan.Common;

namespace PowerTaskMan.Controls
{
    public sealed partial class GraphControlWin2D : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty AxesColorProperty = DependencyProperty.Register(
            "AxesColor",
            typeof(Color),
            typeof(GraphControlWin2D),
            new PropertyMetadata(Colors.Gray, OnAxesColorPropertyChanged));

        public static readonly DependencyProperty AxisMarginProperty = DependencyProperty.Register(
            "AxisMargin",
            typeof(float),
            typeof(GraphControlWin2D),
            new PropertyMetadata(10.0f, OnAxisMarginPropertyChanged));

        public static readonly DependencyProperty DataLabelProperty = DependencyProperty.Register(
            nameof(DataLabel),
            typeof(string),
            typeof(GraphControlWin2D),
            new PropertyMetadata(null, OnDataLabelChanged));

        public static readonly DependencyProperty DataPointColorProperty = DependencyProperty.Register(
            "DataPointColor",
            typeof(Color),
            typeof(GraphControlWin2D),
            new PropertyMetadata(Colors.Black, OnDataPointColorPropertyChanged));

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register(
            "DataPoints",
            typeof(IList<ICoordinatePair>),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new List<ICoordinatePair>(), OnDataPointsPropertyChanged));

        public static readonly DependencyProperty LineColorProperty = DependencyProperty.Register(
            "LineColor",
            typeof(Color),
            typeof(GraphControlWin2D),
            new PropertyMetadata(Colors.Black, OnLineColorPropertyChanged));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(GraphControlWin2D),
            new PropertyMetadata(null, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                var gc = d as GraphControlWin2D;
                gc.Title = e.NewValue?.ToString() ?? "";
            }));

        public static readonly DependencyProperty UseIndexBasedGraphingProperty = DependencyProperty.Register(
            "UseIndexBasedGraphing",
            typeof(bool),
            typeof(GraphControlWin2D),
            new PropertyMetadata(false));

        // Member Variables
        private IList<bool> refresh_request_cache = new List<bool>();

        // Constructor
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

        // Properties
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

        public string DataLabel
        {
            get => (string)GetValue(DataLabelProperty);
            set => SetValue(DataLabelProperty, value);
        }

        public Color DataPointColor
        {
            get { return (Color)GetValue(DataPointColorProperty); }
            set { SetValue(DataPointColorProperty, value); }
        }

        public IList<ICoordinatePair> DataPoints
        {
            get { return (IList<ICoordinatePair>)GetValue(DataPointsProperty); }
            set { SetValue(DataPointsProperty, value); }
        }

        public Color LineColor
        {
            get { return (Color)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        public string Title { get; set; } = "Graph";

        public bool UseIndexBasedGraphing
        {
            get { return (bool)GetValue(UseIndexBasedGraphingProperty); }
            set { SetValue(UseIndexBasedGraphingProperty, value); }
        }

        // Member Methods
        public void Invalidate()
        {
            Canvas.Invalidate();
        }

        private static void OnAxesColorPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.AxesColor = (Color)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnAxisMarginPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.AxisMargin = (float)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnDataLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gc = d as GraphControlWin2D;
            gc.SetDataLabelText(e.NewValue?.ToString() ?? "");
        }

        private static void OnDataPointColorPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.DataPointColor = (Color)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnDataPointsPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.DataPoints = (IList<ICoordinatePair>)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnLineColorPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.LineColor = (Color)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private void SetDataLabelText(string v)
        {
            if (DataLabelTextBlock != null)
            {
                DataLabelTextBlock.Text = v;
            }
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

            // Draw the data points and lines
            if (UseIndexBasedGraphing)
            {
                var data = new List<ICoordinatePair>();
                for (int i = 0; i < DataPoints.Count; i++)
                {
                    data.Add(new CoordinatePair { X = i, Y = DataPoints[i].Y });
                }

                for (int i = 0; i < data.Count - 1; i++)
                {
                    var x1 = origin_x + (data[i].X * 2);
                    var y1 = origin_y - data[i].Y;
                    var x2 = origin_x + (data[i + 1].X * 2);
                    var y2 = origin_y - data[i + 1].Y;
                    session.DrawLine(x1, y1, x2, y2, LineColor, 2);
                    session.FillCircle(x1, y1, 3, DataPointColor);
                    session.FillCircle(x2, y2, 3, DataPointColor);
                }
            }
            else
            {
                for (int i = 0; i < DataPoints.Count - 1; i++)
                {
                    var x1 = origin_x + DataPoints[i].X;
                    var y1 = origin_y - DataPoints[i].Y;
                    var x2 = origin_x + DataPoints[i + 1].X;
                    var y2 = origin_y - DataPoints[i + 1].Y;
                    session.DrawLine(x1, y1, x2, y2, LineColor, 2);
                    session.FillCircle(x1, y1, 3, DataPointColor);
                    session.FillCircle(x2, y2, 3, DataPointColor);
                }
            }
        }
    }
}
