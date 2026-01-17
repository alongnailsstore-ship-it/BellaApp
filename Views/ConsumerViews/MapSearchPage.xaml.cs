using BellaLink.App.ViewModels.ConsumerViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class MapSearchPage : ContentPage
	{
		public MapSearchPage(MapSearchViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (BindingContext is MapSearchViewModel vm)
			{
				await vm.LoadMapData();

				// Popula os pinos manualmente para garantir que apareçam
				PartnersMap.Pins.Clear();
				foreach (var pin in vm.MapPins)
				{
					PartnersMap.Pins.Add(pin);
				}

				// Centraliza no usuário se tiver localização
				if (vm.UserLocation != null)
				{
					PartnersMap.MoveToRegion(MapSpan.FromCenterAndRadius(vm.UserLocation, Distance.FromKilometers(5)));
				}
			}
		}
	}
}