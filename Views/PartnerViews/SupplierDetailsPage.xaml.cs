using BellaLink.App.ViewModels.PartnerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class SupplierDetailsPage : ContentPage
	{
		public SupplierDetailsPage(SupplierDetailsViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (BindingContext is SupplierDetailsViewModel vm)
			{
				await vm.LoadData();
			}
		}
	}
}