using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MisPaint
{
    public class HexToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex)
                return new SolidColorBrush(Color.Parse(hex));
            return new SolidColorBrush(Colors.Black);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            return "#000000";
        }
    }

    public class IsTransparentConverter : IValueConverter
    {
        public static readonly IsTransparentConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IBrush brush && brush is SolidColorBrush solidBrush)
                return solidBrush.Color.A < 255;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
