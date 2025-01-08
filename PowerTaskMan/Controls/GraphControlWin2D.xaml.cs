using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using PowerTaskMan.Common;
using System;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;

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
            "DataLabel",
            typeof(string),
            typeof(GraphControlWin2D),
            new PropertyMetadata(String.Empty, OnDataLabelChanged));

        public static readonly DependencyProperty DataPointColorProperty = DependencyProperty.Register(
            "DataPointColor",
            typeof(Color),
            typeof(GraphControlWin2D),
            new PropertyMetadata(Colors.Black));

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register(
            "DataPoints",
            typeof(IList<ICoordinatePair>),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new List<ICoordinatePair>(), OnDataPointsPropertyChanged));

        public static readonly DependencyProperty GridLineColorProperty = DependencyProperty.Register(
            "GridLineColor",
            typeof(Color),
            typeof(GraphControlWin2D),
            new PropertyMetadata(Colors.LightGray));

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
                if(gc.ChartTitleTextBlock != null)
                {
                    gc.ChartTitleTextBlock.Text = gc.Title;
                }
            }));

        public static readonly DependencyProperty UseIndexBasedGraphingProperty = DependencyProperty.Register(
            "UseIndexBasedGraphing",
            typeof(bool),
            typeof(GraphControlWin2D),
            new PropertyMetadata(false));

        // Member Variables
        private IList<bool> refresh_request_cache = new List<bool>();

        private IList<ICoordinatePair> scaled_data_points;

        private SolidColorBrush _background_brush = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
        private SolidColorBrush _foreground_brush;

        // Constructor
        public GraphControlWin2D()
        {
            this.InitializeComponent();
            GraphControlBorderXamlRef.Translation = new System.Numerics.Vector3(0, 0, 16);
            GraphControlBorderXamlRef.Shadow = new ThemeShadow();

            GraphControlBorderXamlRef.Background = _background_brush;

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

        public Color GridLineColor
        {
            get { return (Color)GetValue(GridLineColorProperty); }
            set { SetValue(GridLineColorProperty, value); }
        }

        public Color LineColor
        {
            get { return (Color)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

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

        private static void OnDataPointsPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.DataPoints = (IList<ICoordinatePair>)e.NewValue;
            control.refresh_request_cache.Add(true);
        }

        private static void OnLineColorPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            //control.LineColor = (Color)e.NewValue;
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
            ClearCanvas(sender, args);

            if (DataPoints == null || DataPoints.Count < 2)
                return;

            // Draw axes
            var width = (float)sender.ActualWidth;
            var height = (float)sender.ActualHeight;
            var margin = AxisMargin;

            // Calculate the graph's origin
            float data_margin = AxisMargin;
            float origin_x = 0 + margin + data_margin;
            float origin_y = 0 - margin + height - data_margin;

            // If we are using index_based graphing, we need to replace those X indices
            var new_data = new List<ICoordinatePair>();

            if (UseIndexBasedGraphing)
            {
                var counter = 0;
                foreach (var point in DataPoints)
                {
                    var new_coord_pair = new CoordinatePair { X = counter, Y = point.Y };
                    new_data.Add(new_coord_pair);
                    counter++;
                }
            }
            else
            {
                new_data = DataPoints.ToList();
            }

            // What is the max and min values we are going to display?
            CoordinatePair max = new CoordinatePair { X = new_data.ElementAt(0).X, Y = new_data.ElementAt(0).Y };
            CoordinatePair min = new CoordinatePair { X = new_data.ElementAt(0).X, Y = new_data.ElementAt(0).Y };

            max.X = new_data.MaxBy(new_data => new_data.X).X;
            max.Y = new_data.MaxBy(new_data => new_data.Y).Y;

            min.X = new_data.MinBy(new_data => new_data.X).X;
            min.Y = new_data.MinBy(new_data => new_data.Y).Y;

            // Calculate intervals for the gridlines
            float xRange = max.X - min.X;
            float yRange = max.Y - min.Y;

            // If the height and or width of the canvas is more than an order of magnitude different from the data, define the scale
            // factor
            float x_scale = 1;
            float y_scale = 1;

            float effective_width = width - (2 * (margin + data_margin)); // Accounts for the margin
            float effective_height = height - (2 * (margin + data_margin)); // Accounts for the margin

            if (max.X - min.X != effective_width)
            {
                if(xRange != 0)
                    x_scale = effective_width / (max.X - min.X);
                else
                    x_scale = 1;
            }

            if (max.Y - min.Y != effective_height)
            {
                if (yRange != 0)
                    y_scale = effective_height / (max.Y - min.Y);
                else
                    y_scale = 1;
            }

       
            float x_gridline_interval_raw = xRange / 10;
            float y_gridline_interval_raw = yRange / 10;

            float x_interval = (float)DetermineAxisGridlineInterval(x_gridline_interval_raw);
            float y_interval = (float)DetermineAxisGridlineInterval(y_gridline_interval_raw);


            DrawAxesAndGridLines(sender, args, width, height, x_interval, y_interval, margin);

            // if the scale factor for either is not one, we need a new set of data points to graph,
            // and we need to subtract the minimum values from all points in the data range
            if (x_scale != 1 || y_scale != 1)
            {
                scaled_data_points = ScaleAndTranslateCoordinateData(new_data, x_scale, y_scale, min.X, min.Y);
            }
            else
            {
                scaled_data_points = DataPoints;
            }

            



            
            

            for (int i = 0; i < scaled_data_points.Count - 1; i++)
            {
                var x1 = origin_x + scaled_data_points[i].X;
                var y1 = origin_y - scaled_data_points[i].Y;
                var x2 = origin_x + scaled_data_points[i + 1].X;
                var y2 = origin_y - scaled_data_points[i + 1].Y;
                session.DrawLine(x1, y1, x2, y2, LineColor, 2);
                session.FillCircle(x1, y1, 3, DataPointColor);
                session.FillCircle(x2, y2, 3, DataPointColor);
            }
            
        }

        private List<ICoordinatePair> ScaleCoordinateData(IList<ICoordinatePair> data, float x_scale, float y_scale)
        {
            var new_data = new List<ICoordinatePair>();
            foreach (var point in data)
            {
                var newX = point.X * x_scale;
                var newY = point.Y* y_scale;
                var new_coord_pair = new CoordinatePair { X = newX, Y = newY };
                new_data.Add(new_coord_pair);
            }
            return new_data;
        }

        private List<ICoordinatePair> ScaleAndTranslateCoordinateData(IList<ICoordinatePair> data, float x_scale, float y_scale, float min_x, float min_y)
        {
            var new_data = new List<ICoordinatePair>();
            foreach (var point in data)
            {
                var newX = (point.X - min_x) * x_scale;
                var newY = (point.Y - min_y) * y_scale;
                var new_coord_pair = new CoordinatePair { X = newX, Y = newY };
                new_data.Add(new_coord_pair);
            }
            return new_data;
        }

        private void ClearCanvas(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            var backgroundColor = _background_brush.Color;
            var opaqueColor = Color.FromArgb(190, 251, 251, 251); // Remove the alpha (255 = fully opaque)
            session.Clear(opaqueColor);
        }

        private void DrawAxesAndGridLines(CanvasControl sender, CanvasDrawEventArgs args,
                                        float width, float height,
                                        float xInterval, float yInterval, 
                                        float margin)
        {
            CoordinatePair x_axis_start = new CoordinatePair { X = margin, Y = height - margin };
            CoordinatePair x_axis_end = new CoordinatePair { X = width - margin, Y = height - margin };
            CoordinatePair y_axis_start = new CoordinatePair { X = margin, Y = margin };
            CoordinatePair y_axis_end = new CoordinatePair { X = margin, Y = height - margin };

            args.DrawingSession.DrawLine(x_axis_start.X, x_axis_start.Y, x_axis_end.X, x_axis_end.Y, AxesColor, 2);
            args.DrawingSession.DrawLine(y_axis_start.X, y_axis_start.Y, y_axis_end.X, y_axis_end.Y, AxesColor, 2);

            float width_for_gl = width - (1 * margin);
            float height_for_gl = height - (1 * margin);
        

            // Draw vertical gridlines.
            for (float i = margin + xInterval; i < width_for_gl; i += xInterval)
            {
                args.DrawingSession.DrawLine(i, height_for_gl - 1, i, margin, GridLineColor);
            }

            // Draw horizontal gridlines.
            for (float i = margin + 1; i < height_for_gl; i += yInterval)
            {
                args.DrawingSession.DrawLine(margin + 1, i, width_for_gl, i, GridLineColor);
            }
        }

        double DetermineAxisGridlineInterval(double rawInterval)
        {
            if(rawInterval == 0 || rawInterval < 10)
            {
                return 20;
            }
            // 1. Calculate power of 10
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawInterval)));

            // 2. Get the fractional part relative to the magnitude
            double fraction = rawInterval / magnitude;

            // 3. Choose a "nice" interval based on the fractional part
            if (fraction <= 1) return magnitude;
            if (fraction <= 2) return 2 * magnitude;
            if (fraction <= 5) return 5 * magnitude;
            return 10 * magnitude;
        }
    }
}
