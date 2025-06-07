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
using static System.Collections.Specialized.BitVector32;

namespace PowerTaskMan.Controls
{    public class AxisStyle
    {
        public Color Color { get; set; } = Colors.Gray;
        public double Margin { get; set; } = 20.0f;
        public bool ShowAxes { get; set; } = true;
    }

    public class DataPointStyle
    {
        public Brush PointColor { get; set; } = new SolidColorBrush(Colors.Black);
        public bool ShowPoints { get; set; } = true;
        public bool ShowLines { get; set; } = true;
        public double PointRadius { get; set; } = 3.0f;
        public double LineThickness { get; set; } = 2.0f;
    }    public class GridStyle
    {
        public Color LineColor { get; set; } = Colors.LightGray;
        public bool ShowGridLines { get; set; } = true;
    }

    public class LineStyle
    {
        public Brush LineColor { get; set; } = new SolidColorBrush(Colors.LightGray);
        public double LineThickness { get; set; } = 1.0f;
    }

    public static class DefaultStyles
    {        public static AxisStyle axis = new AxisStyle
        {
            Color = Colors.Black,
            Margin = 10,
            ShowAxes = true
        };

        public static DataPointStyle datapoint = new DataPointStyle
        {
            PointColor = new SolidColorBrush(Colors.LightGray),
            ShowLines = true,
            ShowPoints = true,
            PointRadius = 1.0f
        };        public static LineStyle line = new LineStyle
        {
            LineColor = new SolidColorBrush(Colors.LightGray),
            LineThickness = 1.0f,
        };

        public static GridStyle grid = new GridStyle
        {
            LineColor = Colors.LightGray,
            ShowGridLines = true
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

        public static readonly DependencyProperty GridlineCustomizationProperty = DependencyProperty.Register(
            nameof(GridCustomization),
            typeof(LineStyle),
            typeof(GraphControlWin2D),
            new PropertyMetadata(new LineStyle(), OnGridCustomizationPropertyChanged)
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
        private SolidColorBrush _background_brush = new SolidColorBrush(Colors.Transparent);
        private SolidColorBrush _text_brush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        private SolidColorBrush _gridline_brush = new SolidColorBrush(Colors.LightGray);
        // Constructor
        public GraphControlWin2D()
        {
            this.InitializeComponent();
            GraphControlBorderXamlRef.Translation = new System.Numerics.Vector3(0, 0, 16);
            GraphControlBorderXamlRef.Shadow = new ThemeShadow();
            //GraphControlBorderXamlRef.Background = _background_brush;

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
            IList<ICoordinatePair> coordinates,
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
                                               IList<ICoordinatePair> coordinates,
                                               DataPointStyle dp_style,
                                               LineStyle line_style)
        {
            var line_brush = Win2DHelpers.XamlBrushToICanvasBrush(session, line_style.LineColor);
            var point_brush = Win2DHelpers.XamlBrushToICanvasBrush(session, dp_style.PointColor);
            // Draw all lines
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var x1 = coordinates[i].X;
                var y1 = coordinates[i].Y;
                var x2 = coordinates[i + 1].X;
                var y2 = coordinates[i + 1].Y;
                session.DrawLine(x1, y1, x2, y2, line_brush, (float)line_style.LineThickness);
            }

            // Draw all points
            foreach (ICoordinatePair cp in coordinates)
            {
                session.FillCircle(cp.X, cp.Y, (float)dp_style.PointRadius, point_brush);
            }
        }

        private static void DrawLinesOnly(CanvasDrawingSession session, IList<ICoordinatePair> coordinates, DataPointStyle dp_style, LineStyle line_style)
        {
            var line_brush = Win2DHelpers.XamlBrushToICanvasBrush(session, line_style.LineColor);
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var x1 = coordinates[i].X;
                var y1 = coordinates[i].Y;
                var x2 = coordinates[i + 1].X;
                var y2 = coordinates[i + 1].Y;
                session.DrawLine(x1, y1, x2, y2, line_brush, (float)line_style.LineThickness);
            }
        }

        private static void DrawPointsOnly(CanvasDrawingSession session, IList<ICoordinatePair> coordinates, DataPointStyle customization, LineStyle line_style)
        {
            var point_brush = Win2DHelpers.XamlBrushToICanvasBrush(session, customization.PointColor);
            foreach (ICoordinatePair cp in coordinates)
            {
                session.FillCircle(cp.X, cp.Y, (float)customization.PointRadius, point_brush);
            }
        }

        private static void NoDrawAction(CanvasDrawingSession session, IList<ICoordinatePair> coordinates, DataPointStyle customization, LineStyle line_style)
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
            if (max.X == min.X)
            {
                max.X += 1;
            }
            if (max.Y == min.Y)
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

            var textFormat = new CanvasTextFormat
            {
                FontSize = 12, // Set font size to 24 points
                FontFamily = "Arial", // Optional: Set font family
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center
            };

            float width_of_y_labels = MeasureTextWidth(sender.Device, max_y.ToString(), textFormat);



            // Draw axes
            var width = (float)sender.ActualWidth;
            var height = (float)sender.ActualHeight;
            var margin = (float)this.AxisCustomization.Margin + width_of_y_labels;
            // Calculate the graph's origin
            float origin_x = 0 + margin;
            float origin_y = 0 - margin + height;
            CoordinatePair origin = new CoordinatePair { X = origin_x, Y = origin_y };



            // If the height and or width of the canvas is more than an order of magnitude different from the data, define the scale
            // factor
            (float x_scale, float y_scale) = DetermineScalingFactor(height, width, margin);

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

            // Translate data points to be relative to the origin.
            IList<ICoordinatePair> to_draw = new List<ICoordinatePair>();
            foreach (ICoordinatePair cp in scaled_data_points)
            {
                CoordinatePair drawable_point = new CoordinatePair
                {
                    X = origin_x + cp.X,
                    Y = origin_y - cp.Y
                };
                to_draw.Add(drawable_point);
            }

            var linebrush = Win2DHelpers.XamlBrushToICanvasBrush(session, LineCustomization.LineColor);
            var pointbrush = Win2DHelpers.XamlBrushToICanvasBrush(session, DataPointCustomization.PointColor);
            //for (int i = 0; i < scaled_data_points.Count - 1; i++)
            //{
            //    var x1 = origin_x + scaled_data_points[i].X;
            //    var y1 = origin_y - scaled_data_points[i].Y;
            //    var x2 = origin_x + scaled_data_points[i + 1].X;
            //    var y2 = origin_y - scaled_data_points[i + 1].Y;
            //    session.DrawLine(x1, y1, x2, y2, linebrush, 2);
            //    session.FillCircle(x1, y1, 3, pointbrush);
            //    session.FillCircle(x2, y2, 3, pointbrush);
            //}

            DrawAction draw_action = GetDrawAction();
            draw_action(session, to_draw, this.DataPointCustomization, this.LineCustomization);

        }

        private List<ICoordinatePair> ScaleCoordinateData(IList<ICoordinatePair> data, float x_scale, float y_scale)
        {
            var new_data = new List<ICoordinatePair>();
            foreach (var point in data)
            {
                var newX = point.X * x_scale;
                var newY = point.Y * y_scale;
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
            float left = margin;
            float right = width - margin;
            float top = margin;
            float bottom = height - margin;

            CoordinatePair origin = new CoordinatePair { X = left, Y = bottom };
            CoordinatePair x_axis_end = new CoordinatePair { X = right, Y = bottom };
            CoordinatePair y_axis_end = new CoordinatePair { X = left, Y = top };

            float x_step = xIntervalDataUnits * xScale;
            float y_step = yIntervalDataUnits * yScale;

            var textFormat = new CanvasTextFormat
            {
                FontSize = 12, // Set font size to 24 points
                FontFamily = "Arial", // Optional: Set font family
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                VerticalAlignment = CanvasVerticalAlignment.Center
            };            // Draw vertical gridlines.
            int v_gl_count = (int)Math.Floor((right - left) / x_step);
            var line_brush = Win2DHelpers.XamlBrushToICanvasBrush(args.DrawingSession, this._gridline_brush);

            // Only draw gridlines if ShowGridLines is true
            if (GridCustomization.ShowGridLines)
            {
                for (int i = 1; i < v_gl_count + 1; i++)
                {
                    float x = left + i * x_step;
                    args.DrawingSession.DrawLine(x, bottom, x, top, line_brush);

                    // Only draw axis labels if ShowAxes is true
                    if (AxisCustomization.ShowAxes)
                    {
                        float dataValue = xMin + (i * xIntervalDataUnits);
                        args.DrawingSession.DrawText(dataValue.ToString("F0"), x, bottom + (margin / 2), _text_brush.Color, textFormat);
                    }
                }

                // Draw horizontal gridlines.
                int h_gl_count = (int)Math.Floor((bottom - top) / y_step);
                for (int i = 1; i < h_gl_count + 1; i++)
                {
                    float y = bottom - i * y_step;
                    args.DrawingSession.DrawLine(left, y, right, y, line_brush);

                    // Only draw axis labels if ShowAxes is true
                    if (AxisCustomization.ShowAxes)
                    {
                        float dataValue = yMin + (i * yIntervalDataUnits); // yMax at the top, yMin at the bottom
                        args.DrawingSession.DrawText(dataValue.ToString("F0"), left - (margin / 2), y, _text_brush.Color, textFormat);
                    }
                }
            }
            else if (AxisCustomization.ShowAxes)
            {
                // If gridlines are hidden but axes are shown, still draw the axis labels
                for (int i = 1; i < v_gl_count + 1; i++)
                {
                    float x = left + i * x_step;
                    float dataValue = xMin + (i * xIntervalDataUnits);
                    args.DrawingSession.DrawText(dataValue.ToString("F0"), x, bottom + (margin / 2), _text_brush.Color, textFormat);
                }

                int h_gl_count = (int)Math.Floor((bottom - top) / y_step);
                for (int i = 1; i < h_gl_count + 1; i++)
                {
                    float y = bottom - i * y_step;
                    float dataValue = yMin + (i * yIntervalDataUnits);
                    args.DrawingSession.DrawText(dataValue.ToString("F0"), left - (margin / 2), y, _text_brush.Color, textFormat);
                }
            }

            // Only draw axes if ShowAxes is true
            if (AxisCustomization.ShowAxes)
            {
                args.DrawingSession.DrawLine(
                    origin.X, origin.Y,
                    x_axis_end.X, x_axis_end.Y,
                    AxisCustomization.Color, 2);

                args.DrawingSession.DrawLine(
                    origin.X, origin.Y,
                    y_axis_end.X, y_axis_end.Y,
                    AxisCustomization.Color, 2);
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
        private (float, float) DetermineScalingFactor(float height, float width, float margin)
        {
            float x_scale = 1;
            float y_scale = 1;
            float effective_width = width - (2 * margin); // Accounts for the margin
            float effective_height = height - (2 * margin); // Accounts for the margin

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
        private float MeasureTextWidth(CanvasDevice device, string text, CanvasTextFormat fmt)
        {
            using var layout = new CanvasTextLayout(device, text, fmt, 1000, 1000);
            return (float)layout.DrawBounds.Width;
        }
    }
}
