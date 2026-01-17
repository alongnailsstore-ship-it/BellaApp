using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using System.Linq;
using Microsoft.Maui.Controls;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	public partial class MapSearchViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		public ObservableCollection<Pin> MapPins { get; set; } = new ObservableCollection<Pin>();

		[ObservableProperty] Location? userLocation;
		[ObservableProperty] bool isBusy;

		public MapSearchViewModel(DatabaseService db)
		{
			_databaseService = db;
		}

		public async Task LoadMapData()
		{
			if (IsBusy) return;
			IsBusy = true;

			try
			{
				// 1. Tenta pegar localização
				var location = await Geolocation.GetLastKnownLocationAsync();
				if (location == null)
				{
					location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
				}

				if (location != null)
				{
					UserLocation = location;
				}

				// 2. Busca parceiros
				var partners = await _databaseService.GetAllPartnersAsync();
				MapPins.Clear();

				foreach (var p in partners)
				{
					// Se tiver endereço, tenta geocodificar
					if (!string.IsNullOrEmpty(p.Address))
					{
						try
						{
							var locations = await Geocoding.GetLocationsAsync(p.Address);
							var pinLocation = locations?.FirstOrDefault();

							if (pinLocation != null)
							{
								var pin = new Pin
								{
									Label = p.Name,
									Address = p.Address,
									Type = PinType.Place,
									Location = new Location(pinLocation.Latitude, pinLocation.Longitude)
								};

								pin.MarkerClicked += async (s, args) =>
								{
									await Shell.Current.GoToAsync($"PartnerDetailsPage?PartnerId={p.Id}");
								};

								MapPins.Add(pin);
							}
						}
						catch { /* Ignora erro de geocoding individual */ }
					}
				}
			}
			catch
			{
				// Erro geral de mapa
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}