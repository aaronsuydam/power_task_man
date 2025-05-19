using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace PowerTaskMan.Common
{
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value; // Not used, so we can leave this unchanged
        }
    }

    class Win2DHelpers
    {
        public static ICanvasBrush XamlBrushToICanvasBrush(CanvasDrawingSession session, Brush xamlBrush)
        {
            if (xamlBrush is SolidColorBrush solidColorBrush)
            {
                return new CanvasSolidColorBrush(session, solidColorBrush.Color);
            }
            else if (xamlBrush is LinearGradientBrush linearGradientBrush)
            {
                var stops = linearGradientBrush.GradientStops.Select(gs => new CanvasGradientStop
                {
                    Position = (float)gs.Offset,
                    Color = gs.Color
                }).ToArray();

                var cg = new CanvasLinearGradientBrush(session, stops);
                cg.StartPoint = new System.Numerics.Vector2((float)linearGradientBrush.StartPoint.X, (float)linearGradientBrush.StartPoint.Y);
                cg.EndPoint = new System.Numerics.Vector2((float)linearGradientBrush.EndPoint.X, (float)linearGradientBrush.EndPoint.Y);
                return cg;
            }
            // Add more cases for other types of brushes as needed
            throw new NotSupportedException($"Error in {nameof(XamlBrushToICanvasBrush)}: Unsupported brush type");
        }
    }
}
