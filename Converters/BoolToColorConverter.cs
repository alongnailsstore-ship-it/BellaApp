using System.Globalization;
using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.Converters
{
	public class BoolToColorConverter : IValueConverter
	{
		// Correção: Adicionei '?' para aceitar nulos (object?) e evitar os avisos CS8767
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is bool isIncome)
			{
				// True = Entrada (Verde), False = Saída (Vermelho)
				return isIncome ? Colors.Green : Colors.Red;
			}
			// Retorno padrão seguro se falhar
			return Colors.Black;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return null;
		}
	}
}