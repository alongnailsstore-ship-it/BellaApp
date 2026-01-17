using BellaLink.App.ViewModels.ConsumerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class BookingConfirmationPage : ContentPage
	{
		public BookingConfirmationPage(BookingConfirmationViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}