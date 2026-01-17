#pragma warning disable CS0618
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using BellaLink.App.Models;
using BellaLink.App.Services;
using BellaLink.App.Views;
using System;
using System.Globalization;
using BellaLink.App.ViewModels;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public partial class PartnerServicesViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<ServiceItem> MyServices { get; set; }

		// LISTA DE CATEGORIAS (Predefinidas igual ao Consumidor)
		public ObservableCollection<string> Categories { get; set; }

		[ObservableProperty] private bool isLoading;
		[ObservableProperty] private bool isFormVisible;

		[ObservableProperty] private string newServiceName = string.Empty;
		[ObservableProperty] private string newServicePrice = string.Empty;
		[ObservableProperty] private string newServiceDuration = string.Empty;

		// CATEGORIA SELECIONADA NO PICKER
		[ObservableProperty] private string? selectedCategory;

		public PartnerServicesViewModel(DatabaseService databaseService, IAuthService authService)
		{
			_databaseService = databaseService;
			_authService = authService;
			MyServices = new ObservableCollection<ServiceItem>();

			// Inicializa as categorias padrão
			Categories = new ObservableCollection<string>
			{
				"Cabelo",
				"Unhas",
				"Make",
				"Estética",
				"Sobrancelhas",
				"Spa",
				"Barbearia",
				"Depilação"
			};
		}

		public async Task LoadServices()
		{
			if (IsLoading) return;
			IsLoading = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();

				if (!string.IsNullOrEmpty(userId))
				{
					var servicesFromDb = await _databaseService.GetServicesForPartnerAsync(userId);

					MainThread.BeginInvokeOnMainThread(() =>
					{
						MyServices.Clear();
						foreach (var service in servicesFromDb)
						{
							MyServices.Add(service);
						}
					});
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro ao carregar: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void ShowForm()
		{
			NewServiceName = string.Empty;
			NewServicePrice = string.Empty;
			NewServiceDuration = string.Empty;
			SelectedCategory = null; // Limpa a seleção anterior
			IsFormVisible = true;
		}

		[RelayCommand]
		private void CancelForm()
		{
			IsFormVisible = false;
		}

		[RelayCommand]
		private async Task SaveService()
		{
			// Validações
			if (string.IsNullOrWhiteSpace(NewServiceName) || string.IsNullOrWhiteSpace(NewServicePrice))
			{
				await Shell.Current.DisplayAlert("Atenção", "Preencha nome e preço.", "OK");
				return;
			}

			if (string.IsNullOrEmpty(SelectedCategory))
			{
				await Shell.Current.DisplayAlert("Atenção", "Selecione uma categoria.", "OK");
				return;
			}

			string priceClean = NewServicePrice.Replace(".", ",");

			if (!decimal.TryParse(priceClean, NumberStyles.Currency, new CultureInfo("pt-BR"), out decimal price))
			{
				await Shell.Current.DisplayAlert("Erro", "Valor inválido.", "OK");
				return;
			}

			if (!int.TryParse(NewServiceDuration, out int duration)) duration = 30;

			// 1. CRIA O OBJETO LOCALMENTE
			var newService = new ServiceItem
			{
				Id = Guid.NewGuid().ToString(),
				Name = NewServiceName,
				Category = SelectedCategory, // SALVA A CATEGORIA
				Price = price,
				DurationMinutes = duration,
				Description = "Serviço Profissional"
			};

			// 2. ATUALIZAÇÃO OTIMISTA
			MyServices.Add(newService);
			IsFormVisible = false;

			// 3. ENVIA PARA O BANCO (Background)
			try
			{
				var userId = await _authService.GetUserIdAsync();
				if (!string.IsNullOrEmpty(userId))
				{
					await _databaseService.AddServiceAsync(userId, newService);
				}
				else
				{
					MyServices.Remove(newService);
					await Shell.Current.DisplayAlert("Erro", "Sessão expirada.", "OK");
				}
			}
			catch
			{
				MyServices.Remove(newService);
				IsFormVisible = true;
				await Shell.Current.DisplayAlert("Erro", "Falha na conexão. Tente novamente.", "OK");
			}
		}

		[RelayCommand]
		private async Task DeleteService(ServiceItem service)
		{
			if (service == null) return;
			bool confirm = await Shell.Current.DisplayAlert("Excluir", $"Apagar {service.Name}?", "Sim", "Não");

			if (confirm)
			{
				// 1. OTIMISTA
				MyServices.Remove(service);

				// 2. REAL
				try
				{
					var userId = await _authService.GetUserIdAsync();
					if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(service.Id))
					{
						await _databaseService.RemoveServiceAsync(userId, service.Id);
					}
				}
				catch
				{
					MyServices.Add(service);
					await Shell.Current.DisplayAlert("Erro", "Não foi possível excluir.", "OK");
				}
			}
		}

		[RelayCommand]
		private void ExitToProfiles()
		{
			if (Application.Current != null && Application.Current.Windows.Count > 0)
			{
				Application.Current.Windows[0].Page = new SelectProfilePage(new SelectProfileViewModel(_authService, _databaseService));
			}
		}
	}
}