using System.Globalization;
using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.Converters
{
	public class StringNotNullOrEmptyConverter : IValueConverter
	{
		// Correção: object? (pode ser nulo)
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			var str = value as string;
			return !string.IsNullOrEmpty(str);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
	}
}