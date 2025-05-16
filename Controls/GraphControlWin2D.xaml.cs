using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PowerTaskMan.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace PowerTaskMan.Controls
{
    public class AxisStyle
    {
        public Color Color { get; set; } = Colors.Gray;
        public double Margin { get; set; } = 20.0f;
    }

    public class DataPointStyle
    {
        public Color PointColor { get; set; } = Colors.Black;
        public bool ShowPoints { get; set; } = true;
        public bool ShowLines { get; set; } = true;
        public double PointRadius { get; set; } = 3.0f;
        public double LineThickness { get; set; } = 2.0f;
    }

    public class GridStyle
    {
        public Color LineColor { get; set; } = Colors.LightGray;
    }

    public static class DefaultStyles
    {
        public static AxisStyle axis = new AxisStyle
        {
            Color = Colors.Black,
            Margin = 0
        };

        public static DataPointStyle datapoint = new DataPointStyle
        {
            PointColor = Colors.LightGray,
            ShowLines = true,
            ShowPoints = true,
            PointRadius = 1.0f
        };

        public static LineStyle line = new LineStyle
        {
            LineColor = Colors.LightGray,
            LineThickness = 1.0f,
        };
    }

    public sealed partial class GraphControlWin2D : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty AxisCustomizationProperty = DependencyProperty.Register(
            "AxisCustomization",
            typeof(AxisStyle),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new AxisStyle(), OnAxisCustomizationPropertyChanged));

        public static readonly DependencyProperty DataPointCustomizationProperty = DependencyProperty.Register(
            "DataPointCustomization",
            typeof(DataPointStyle),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new DataPointStyle(), OnDataPointCustomizationPropertyChanged));

        public static readonly DependencyProperty GridCustomizationProperty = DependencyProperty.Register(
            "GridCustomization",
            typeof(GridStyle),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new GridStyle(), OnGridCustomizationPropertyChanged));

        public static readonly DependencyProperty DataLabelProperty = DependencyProperty.Register(
            "DataLabel",
            typeof(string),
            typeof(GraphControlWin2D),
            new PropertyMetadata(string.Empty, OnDataLabelChanged));

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register(
            "DataPoints",
            typeof(IList<ICoordinatePair>),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new List<ICoordinatePair>(), OnDataPointsPropertyChanged));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(GraphControlWin2D),
            new PropertyMetadata(null, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                var gc = d as GraphControlWin2D;
                gc.Title = e.NewValue?.ToString() ?? "";
                if (gc.ChartTitleTextBlock != null)
                {
                    gc.ChartTitleTextBlock.Text = gc.Title;
                }
            }));

        public static readonly DependencyProperty UseIndexBasedGraphingProperty = DependencyProperty.Register(
            "UseIndexBasedGraphing",
            typeof(bool),
            typeof(GraphControlWin2D),
            new PropertyMetadata(false));

        public static readonly DependencyProperty LineCustomizationProperty = DependencyProperty.Register(
          nameof(LineCustomization),
          typeof(LineStyle),
          typeof(GraphControlWin2D),
          new PropertyMetadata(new LineStyle(), OnLineCustomizationPropertyChanged)
        );

        // Member Variables
        private IList<bool> refresh_request_cache = new List<bool>();
        private float xRange;
        private float yRange;
        private float min_x;
        private float min_y;
        private float max_x;
        private float max_y;
        private IList<ICoordinatePair> _dataPoints;
        private IList<ICoordinatePair> scaled_data_points;
        private bool notinit = true;
        private SolidColorBrush _background_brush = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
        private SolidColorBrush _text_brush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];

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
                        DispatcherQueue.TryEnqueue(() =>
                        {
                                _determine_data_range(); // Must be called before Canvas.Invalidate() to ensure the range has been updated
                                Canvas.Invalidate();
                        });
                    }
                }
            });
        }


        /// Public Properties
        public AxisStyle AxisCustomization
        {
            get { return (AxisStyle)GetValue(AxisCustomizationProperty); }
            set { SetValue(AxisCustomizationProperty, value); }
        }

        public DataPointStyle DataPointCustomization
        {
            get { return (DataPointStyle)GetValue(DataPointCustomizationProperty); }
            set { SetValue(DataPointCustomizationProperty, value); }
        }

        public GridStyle GridCustomization
        {
            get { return (GridStyle)GetValue(GridCustomizationProperty); }
            set { SetValue(GridCustomizationProperty, value); }
        }

        public string DataLabel
        {
            get => (string)GetValue(DataLabelProperty);
            set => SetValue(DataLabelProperty, value);
        }

        public IList<ICoordinatePair> DataPoints
        {
            get { return (IList<ICoordinatePair>)GetValue(DataPointsProperty); }
            set { SetValue(DataPointsProperty, value); }
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

        public LineStyle LineCustomization
        {
            get => (LineStyle)GetValue(LineCustomizationProperty);
            set => SetValue(LineCustomizationProperty, value);
        }

        // Member Methods
        // Drawing action delegates
        private delegate void DrawAction(
            CanvasDrawingSession session, 
            List<(float X, float Y)> coordinates, 
            DataPointStyle customization, 
            LineStyle line_style
            );

        private DrawAction GetDrawAction()
        {
            // Select a drawing strategy based on current settings - this happens once per draw call
            if (this.DataPointCustomization.ShowLines && DataPointCustomization.ShowPoints)
                return DrawLinesAndPoints;
            else if (DataPointCustomization.ShowLines)
                return DrawLinesOnly;
            else if (DataPointCustomization.ShowPoints)
                return DrawPointsOnly;
            else
                return NoDrawAction;
        }

        // Specialized drawing methods with no branching in their hot paths
        private static void DrawLinesAndPoints(CanvasDrawingSession session, 
                                               List<(float X, float Y)> coordinates, 
                                               DataPointStyle dp_style,
                                               LineStyle line_style)
        {
            // Draw all lines
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var (x1, y1) = coordinates[i];
                var (x2, y2) = coordinates[i + 1];
                session.DrawLine(x1, y1, x2, y2, customization.LineColor, (float)customization.LineThickness);
            }

            // Draw all points
            foreach (var (x, y) in coordinates)
            {
                session.FillCircle(x, y, (float)customization.PointRadius, customization.PointColor);
            }
        }

        private static void DrawLinesOnly(CanvasDrawingSession session, List<(float X, float Y)> coordinates, DataPointStyle dp_style, LineStyle line_style)
        {
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var (x1, y1) = coordinates[i];
                var (x2, y2) = coordinates[i + 1];
                session.DrawLine(x1, y1, x2, y2, customization.LineColor, (float)customization.LineThickness);
            }
        }

        private static void DrawPointsOnly(CanvasDrawingSession session, List<(float X, float Y)> coordinates, DataPointStyle customization, LineStyle line_style)
        {
            foreach (var (x, y) in coordinates)
            {
                session.FillCircle(x, y, (float)customization.PointRadius, customization.PointColor);
            }
        }

        private static void NoDrawAction(CanvasDrawingSession session, List<(float X, float Y)> coordinates, DataPointStyle customization, LineStyle line_style)
        {
            // Do nothing
        }

        public void Invalidate()
        {
            Canvas.Invalidate();
        }

        private static void OnAxisCustomizationPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.refresh_request_cache.Add(true);
        }

        private static void OnDataPointCustomizationPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
            control.refresh_request_cache.Add(true);
        }

        private static void OnGridCustomizationPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            var control = (GraphControlWin2D)dobj;
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
            var originalData = (IList<ICoordinatePair>)e.NewValue;

            if (control.UseIndexBasedGraphing)
            {
                var counter = 0;
                for (int i = 0; i < originalData.Count; i++)
                {
                    originalData[i] = new CoordinatePair { X = counter, Y = originalData[i].Y };
                    counter++;
                }
            }

            control.min_x = originalData.MinBy(new_data => new_data.X).X;
            control.min_y = originalData.MinBy(new_data => new_data.Y).Y;
            control.max_x = originalData.MaxBy(new_data => new_data.X).X;
            control.max_y = originalData.MaxBy(new_data => new_data.Y).Y;
            control.notinit = false;
            control.refresh_request_cache.Add(true);
        }

        private static void OnLineCustomizationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (GraphControlWin2D)d;
            ctrl.refresh_request_cache.Add(true);
        }

        private void SetDataLabelText(string v)
        {
            if (DataLabelTextBlock != null)
            {
                DataLabelTextBlock.Text = v;
            }
        }

        private void _determine_data_range()
        {
            if (DataPoints == null || DataPoints.Count < 2)
                return;
            // What is the max and min values we are going to display?
            CoordinatePair max = new CoordinatePair { X = DataPoints.ElementAt(0).X, Y = DataPoints.ElementAt(0).Y };
            CoordinatePair min = new CoordinatePair { X = DataPoints.ElementAt(0).X, Y = DataPoints.ElementAt(0).Y };

            max.X = DataPoints.MaxBy(DataPoints => DataPoints.X).X;
            max.Y = DataPoints.MaxBy(DataPoints => DataPoints.Y).Y;

            min.X = DataPoints.MinBy(DataPoints => DataPoints.X).X;
            min.Y = DataPoints.MinBy(DataPoints => DataPoints.Y).Y;

            // Calculate intervals for the gridlines, ensuring they are never 0;
            if(max.X == min.X)
            {
                max.X += 1;
            }
            if(min.Y == min.Y)
            {
                max.Y += 1;
            }

            xRange = max.X - min.X;
            yRange = max.Y - min.Y;

        }


        void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            ClearCanvas(sender, args);

            if (DataPoints == null || DataPoints.Count < 2 || this.notinit)
                return;

            if (xRange == 0 || yRange == 0)
                return;

            // Draw axes
            var width = (float)sender.ActualWidth;
            var height = (float)sender.ActualHeight;
            var margin = (float)this.AxisCustomization.Margin;

            // Calculate the graph's origin
            float data_margin = margin;
            float origin_x = 0 + margin;
            float origin_y = 0 - margin + height;

    

            // If the height and or width of the canvas is more than an order of magnitude different from the data, define the scale
            // factor
            (float x_scale, float y_scale) = DetermineScalingFactor(height, width, margin, data_margin);

            float x_interval = DetermineAxisGridlineInterval(this.xRange);
            float y_interval = DetermineAxisGridlineInterval(this.yRange);


            DrawAxesAndGridLines(sender, args, 
                                 width, height,
                                 x_interval, y_interval,
                                 x_scale, y_scale,
                                 this.min_x, this.min_y,
                                 margin);

            // if the scale factor for either is not one, we need a new set of data points to graph,
            // and we need to subtract the minimum values from all points in the data range
            if (x_scale != 1 || y_scale != 1)
            {
                scaled_data_points = ScaleAndTranslateCoordinateData(DataPoints, x_scale, y_scale);
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
                session.DrawLine(x1, y1, x2, y2, DataPointCustomization.LineColor, 2);
                session.FillCircle(x1, y1, 3, DataPointCustomization.PointColor);
                session.FillCircle(x2, y2, 3, DataPointCustomization.PointColor);
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

        private List<ICoordinatePair> ScaleAndTranslateCoordinateData(IList<ICoordinatePair> data, float x_scale, float y_scale)
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
            var a = backgroundColor.A;
            var b = backgroundColor.B;
            var g = backgroundColor.G;
            var r = backgroundColor.R;
            var rgb = Color.FromArgb(a, r, g, b); // Remove the alpha (255 = fully opaque)
            session.Clear(rgb);
        }

        private void DrawAxesAndGridLines(CanvasControl sender, CanvasDrawEventArgs args,
                                        float width, float height,
                                        float xIntervalDataUnits, float yIntervalDataUnits,
                                        float xScale, float yScale,
                                        float xMin, float yMin,
                                        float margin)
        {
            CoordinatePair x_axis_start = new CoordinatePair { X = margin, Y = height - margin };
            CoordinatePair x_axis_end = new CoordinatePair { X = width - margin, Y = height - margin };
            CoordinatePair y_axis_start = new CoordinatePair { X = margin, Y = margin };
            CoordinatePair y_axis_end = new CoordinatePair { X = margin, Y = height - margin };

            args.DrawingSession.DrawLine(x_axis_start.X, x_axis_start.Y, x_axis_end.X, x_axis_end.Y, AxisCustomization.Color, 2);
            args.DrawingSession.DrawLine(y_axis_start.X, y_axis_start.Y, y_axis_end.X, y_axis_end.Y, AxisCustomization.Color, 2);

            float width_for_gl = width - (1 * margin);
            float height_for_gl = height;

            float x_gridlines_graphics_interval = xIntervalDataUnits * xScale;
            float y_gridlines_graphics_interval = yIntervalDataUnits * yScale;

            var textFormat = new CanvasTextFormat
            {
                FontSize = 12, // Set font size to 24 points
                FontFamily = "Arial", // Optional: Set font family
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center
            };


            // Draw vertical gridlines.
            float gridline_counter = 0;
            for (float i = margin; i < width_for_gl; i += x_gridlines_graphics_interval)
            {
                args.DrawingSession.DrawLine(i, height_for_gl - 1, i, margin, GridCustomization.LineColor);
                float dataValue = xMin + (gridline_counter * xIntervalDataUnits);
                args.DrawingSession.DrawText(dataValue.ToString("F0"), i, height_for_gl - 10, _text_brush.Color, textFormat);
                gridline_counter++;
            }

            gridline_counter = 0;

            // Draw horizontal gridlines.
            for (float i = y_axis_end.Y; i > margin; i -= y_gridlines_graphics_interval)
            {
                args.DrawingSession.DrawLine(margin + 1, i, width_for_gl, i, GridCustomization.LineColor);
                float dataValue = yMin + (gridline_counter * yIntervalDataUnits); // yMax at the top, yMin at the bottom
                args.DrawingSession.DrawText(dataValue.ToString("F0"), margin - 5, i, _text_brush.Color, textFormat);
                gridline_counter++;
            }
        }

        /// <summary>
        /// Determines the <b>data units</b> interval for the gridlines on a particular axis.
        /// </summary>
        /// <param name="data_range"></param>
        /// <returns></returns>
        float DetermineAxisGridlineInterval(float data_range, float more_or_less_gridlines = 1)
        { 
            // 1. Calculate power of 10
            double nearest_power_ten = Math.Floor(Math.Log10(data_range));
            double next_lowest_power_ten = nearest_power_ten - 1;

            float magnitude = (float)Math.Pow(10, next_lowest_power_ten);

            // 2. Get the fractional part relative to the magnitude
            float fraction = (data_range / magnitude) * more_or_less_gridlines;

            // 3. Choose a "nice" interval based on the fractional part
            float interval;
            if (fraction <= 3) interval = 3.0f;
            else if (fraction <= 6) interval = (float)Math.Round(fraction);
            else interval = 6.0f;

            // 4. Determine max and min intervals
            float max_interval = data_range / 10.0f; // Maximum allowable interval for 15 gridlines
            float min_interval = data_range / 3.0f;  // Minimum allowable interval for 3 gridlines

            // Ensure min_interval is never greater than max_interval
            if (min_interval > max_interval)
            {
                min_interval = max_interval;
            }

            // 5. Clamp the interval between min and max
            return Math.Clamp(interval, min_interval, max_interval);
        }

        /// <summary>
        /// Determines the scaling factor for the x and y axes.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="margin"></param>
        /// <param name="data_margin"></param>
        /// <returns>xScale, yScale</returns>
        private (float, float) DetermineScalingFactor(float height, float width, float margin, float data_margin)
        {
            float x_scale = 1;
            float y_scale = 1;
            float effective_width = width - (2 * (margin + data_margin)); // Accounts for the margin
            float effective_height = height - (2 * (margin + data_margin)); // Accounts for the margin

            if (xRange != effective_width)
            {
                if (xRange != 0)
                    x_scale = effective_width / xRange;
                else
                    x_scale = 1;
            }

            if (yRange != effective_height)
            {
                if (yRange != 0)
                    y_scale = effective_height / yRange;
                else
                    y_scale = 1;
            }

            return (x_scale, y_scale);
        }
    }
}
