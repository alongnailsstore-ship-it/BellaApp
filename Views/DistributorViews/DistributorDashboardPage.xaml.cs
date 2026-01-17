using BellaLink.App.ViewModels.DistributorViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.DistributorViews
{
	public partial class DistributorDashboardPage : ContentPage
	{
		private readonly DistributorDashboardViewModel _viewModel;

		public DistributorDashboardPage(DistributorDashboardViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel; // <--- ISSO LIGA OS BOTÕES
			_viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (_viewModel != null)
			{
				await _viewModel.Initialize();
			}
		}
	}
}