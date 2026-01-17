using BellaLink.App.ViewModels.DistributorViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.DistributorViews
{
	public partial class DistributorOrdersPage : ContentPage
	{
		private readonly DistributorOrdersViewModel _viewModel;

		public DistributorOrdersPage(DistributorOrdersViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
			_viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _viewModel.LoadOrders();
		}
	}
}