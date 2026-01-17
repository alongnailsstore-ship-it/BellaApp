using BellaLink.App.ViewModels.ConsumerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class MyAppointmentsPage : ContentPage
	{
		private readonly MyAppointmentsViewModel _viewModel;

		public MyAppointmentsPage(MyAppointmentsViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
			_viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (_viewModel != null)
			{
				// Carrega a lista toda vez que a aba abre
				await _viewModel.LoadAppointments();
			}
		}
	}
}