using BellaLink.App.ViewModels.DistributorViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.DistributorViews
{
	public partial class DistributorProductsPage : ContentPage
	{
		private readonly DistributorProductsViewModel _viewModel;

		public DistributorProductsPage(DistributorProductsViewModel viewModel)
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
				await _viewModel.LoadProducts();
			}
		}
	}
}