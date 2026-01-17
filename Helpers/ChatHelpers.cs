using BellaLink.App.Models;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BellaLink.App.Helpers
{
	public class ChatTemplateSelector : DataTemplateSelector
	{
		public DataTemplate? MyMessageTemplate { get; set; }
		public DataTemplate? TheirMessageTemplate { get; set; }
		public DataTemplate? DateHeaderTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is ChatMessage msg)
			{
				if (msg.IsHeader) return DateHeaderTemplate ?? new DataTemplate();
				return msg.IsMine ? (MyMessageTemplate ?? new DataTemplate()) : (TheirMessageTemplate ?? new DataTemplate());
			}
			return new DataTemplate();
		}
	}

	public class MessageStatusConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is MessageStatus status)
			{
				return status switch
				{
					MessageStatus.Read => Colors.DeepSkyBlue,
					_ => Colors.Gray
				};
			}
			return Colors.Gray;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null!;
	}

	public class StatusIconConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is MessageStatus status)
			{
				return status switch
				{
					MessageStatus.Sending => "🕒",
					MessageStatus.Sent => "✓",
					_ => "✓✓"
				};
			}
			return "";
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null!;
	}
}