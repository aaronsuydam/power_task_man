using Microsoft.UI.Xaml;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTaskMan.Common
{
    internal class ThemeHelpers
    {
        public static SKColor GetTextThemeColor()
        {
            var theme = Application.Current.RequestedTheme;
            SKColor font_color;
            if (theme == ApplicationTheme.Light)
            {
                font_color = SKColors.Black;
            }
            else
            {
                font_color = SKColors.White;
            }
            return font_color;  
        }
    }
}
