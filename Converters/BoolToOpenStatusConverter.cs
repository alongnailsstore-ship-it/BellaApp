using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Converters
{
	public class BoolToOpenStatusConverter : IValueConverter
	{
		// Adicionado '?' em object para aceitar nulos e corrigir o Warning
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is bool isOpen)
			{
				return isOpen ? "Aberto" : "Fechado";
			}
			return "Fechado";
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return null;
		}
	}
}