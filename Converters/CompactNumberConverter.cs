using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Converters
{
	public class CompactNumberConverter : IValueConverter
	{
		// Correção: object? para aceitar nulos e satisfazer a interface
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is int num)
			{
				if (num >= 1_000_000_000)
					return (num / 1_000_000_000D).ToString("0.#") + "b";

				if (num >= 1_000_000)
					return (num / 1_000_000D).ToString("0.#") + "m";

				if (num >= 1_000)
					return (num / 1_000D).ToString("0.#") + "k";

				return num.ToString();
			}
			return "0";
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}