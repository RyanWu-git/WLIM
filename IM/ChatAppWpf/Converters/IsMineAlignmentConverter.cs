using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatAppWpf.Converters;

public sealed class IsMineAlignmentConverter : IMultiValueConverter
{
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
  {
    var senderId = values.Length > 0 ? values[0]?.ToString() : null;
    var currentUserId = values.Length > 1 ? values[1]?.ToString() : null;

    if (!string.IsNullOrWhiteSpace(senderId) && !string.IsNullOrWhiteSpace(currentUserId) && senderId == currentUserId)
    {
      return HorizontalAlignment.Right;
    }

    return HorizontalAlignment.Left;
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }
}
