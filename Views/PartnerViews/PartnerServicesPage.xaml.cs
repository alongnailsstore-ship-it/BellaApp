using BellaLink.App.ViewModels.PartnerViewModels;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerServicesPage : ContentPage
	{
		private readonly PartnerServicesViewModel _viewModel;

		public PartnerServicesPage(PartnerServicesViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
			_viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			// Espera um pouco para a animação da tela terminar
			await Task.Delay(300);

			// Joga o carregamento para a thread principal de forma segura
			Dispatcher.Dispatch(async () =>
			{
				if (_viewModel != null)
				{
					await _viewModel.LoadServices();
				}
			});
		}
	}
}