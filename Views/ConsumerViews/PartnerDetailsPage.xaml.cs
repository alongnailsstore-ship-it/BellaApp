using BellaLink.App.ViewModels.ConsumerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class PartnerDetailsPage : ContentPage
	{
		public PartnerDetailsPage(PartnerDetailsViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			// Garante que os dados (Incluindo Seguidores e Comentários do Shorts)
			// sejam carregados toda vez que a tela aparecer.
			if (BindingContext is PartnerDetailsViewModel vm)
			{
				await vm.LoadPartnerData();
			}
		}
	}
}