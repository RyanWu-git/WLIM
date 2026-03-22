using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;

namespace ChatAppWpf.Converters;

public sealed class IsMineBrushConverter : IMultiValueConverter
{
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
  {
    var senderId = values.Length > 0 ? values[0]?.ToString() : null;
    var currentUserId = values.Length > 1 ? values[1]?.ToString() : null;

    if (!string.IsNullOrWhiteSpace(senderId) && !string.IsNullOrWhiteSpace(currentUserId) && senderId == currentUserId)
    {
      return new SolidColorBrush(Color.FromRgb(0x95, 0xEC, 0x69));
    }

    return Brushes.White;
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }
}
