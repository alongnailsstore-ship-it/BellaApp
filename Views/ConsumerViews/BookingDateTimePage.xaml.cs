using BellaLink.App.ViewModels.ConsumerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class BookingDateTimePage : ContentPage
	{
		public BookingDateTimePage(BookingDateTimeViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}