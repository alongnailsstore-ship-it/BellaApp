using BellaLink.App.ViewModels.PartnerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerShoppingPage : ContentPage
	{
		public PartnerShoppingPage(PartnerShoppingViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (BindingContext is PartnerShoppingViewModel vm)
			{
				await vm.LoadList();
			}
		}
	}
}