using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

using Color = Windows.UI.Color;

namespace DontSleepYet.Helpers;

public class UsageToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double progress)
        {
            // Ensure progress is between 0 and 1
            progress = Math.Max(0, Math.Min(1, progress));

            Color color;

            if (progress <= 0.7)
            {
                // 0% - 70%は緑
                //color = Colors.Green;
                color = Color.FromArgb(255, 0, 180, 0);
            }
            else if (progress <= 0.85)
            {
                // 70% - 85%はオレンジへのグラデーション
                //double ratio = (progress - 0.7) / 0.15;
                //byte red = (byte)(255 * ratio);
                //byte green = (byte)(255 * (1 - ratio));
                //color = Color.FromArgb(255, red, green, 0);
                color = Color.FromArgb(255, 255, 160, 0);
            }
            else
            {
                // 85% - 100%は赤へのグラデーション
                //double ratio = (progress - 0.85) / 0.15;
                //byte green = (byte)(255 * (1 - ratio));
                //color = Color.FromArgb(255, 255, green, 0);
                color = Color.FromArgb(255, 255, 0, 0);
            }

            return new SolidColorBrush(color);
        }

        return Brushes.Transparent;
    }


    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}