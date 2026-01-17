using BellaLink.App.Models;
using BellaLink.App.Services;
using BellaLink.App.Views.ConsumerViews;
using BellaLink.App.Views.PartnerViews;
using BellaLink.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Maui.ApplicationModel;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	public partial class ConsumerHomeViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		// --- COLEÇÕES ---
		public ObservableCollection<PromoBanner> Banners { get; set; } = new ObservableCollection<PromoBanner>();
		public ObservableCollection<Partner> Partners { get; set; } = new ObservableCollection<Partner>();

		// NOVO: Coleção para Fornecedores/Distribuidores
		public ObservableCollection<Partner> Suppliers { get; set; } = new ObservableCollection<Partner>();

		private List<Partner> _allPartners = new List<Partner>();

		// --- PROPRIEDADES ---
		[ObservableProperty] private string userName = "Visitante";
		[ObservableProperty] private string userPhoto = "";
		[ObservableProperty] private string searchText = "";
		[ObservableProperty] private bool isBusy;

		// Controle do Carrossel
		[ObservableProperty] private int currentBannerPosition;

		public ConsumerHomeViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		// --- COMANDOS DO CARROSSEL ---
		[RelayCommand]
		private void NextBanner()
		{
			if (Banners.Count <= 1) return;

			if (CurrentBannerPosition < Banners.Count - 1)
				CurrentBannerPosition++;
			else
				CurrentBannerPosition = 0; // Loop
		}

		[RelayCommand]
		private void PreviousBanner()
		{
			if (Banners.Count <= 1) return;

			if (CurrentBannerPosition > 0)
				CurrentBannerPosition--;
			else
				CurrentBannerPosition = Banners.Count - 1; // Loop
		}
		// -----------------------------

		public async Task Initialize()
		{
			if (IsBusy) return;
			IsBusy = true;
			try
			{
				// 1. Carregar Dados do Usuário
				var userId = await _authService.GetUserIdAsync();
				if (!string.IsNullOrEmpty(userId))
				{
					var user = await _databaseService.GetUserAsync(userId);
					if (user != null)
					{
						UserName = user.Name ?? "Visitante";

						// Tratamento robusto para foto (evita CS8601)
						string photo = user.ConsumerPhoto;
						if (string.IsNullOrEmpty(photo)) photo = user.PartnerPhoto;
						UserPhoto = photo ?? "";
					}
				}

				// 2. Carregar Listas em Paralelo (opcional, mas rápido)
				await Task.WhenAll(
					LoadBanners(),
					LoadPartners(),
					LoadSuppliers() // NOVO
				);
			}
			finally { IsBusy = false; }
		}

		private async Task LoadBanners()
		{
			try
			{
				var list = await _databaseService.GetActiveBannersAsync();

				MainThread.BeginInvokeOnMainThread(() =>
				{
					Banners.Clear();

					// Adiciona os reais do banco
					foreach (var b in list) if (b.IsActive) Banners.Add(b);

					// --- MOCKUP SE VAZIO ---
					if (Banners.Count == 0)
					{
						Banners.Add(new PromoBanner
						{
							Title = "Bem-vindo ao BellaLink",
							ImageUrl = "https://images.pexels.com/photos/3993449/pexels-photo-3993449.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
							PartnerName = "Sistema"
						});

						Banners.Add(new PromoBanner
						{
							Title = "Unhas Perfeitas",
							ImageUrl = "https://images.pexels.com/photos/3997391/pexels-photo-3997391.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
							PartnerName = "Sistema"
						});

						Banners.Add(new PromoBanner
						{
							Title = "Ofertas Especiais",
							ImageUrl = "https://images.pexels.com/photos/2113855/pexels-photo-2113855.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
							PartnerName = "Sistema"
						});
					}
					CurrentBannerPosition = 0;
				});
			}
			catch { }
		}

		private async Task LoadPartners()
		{
			try
			{
				var list = await _databaseService.GetAllPartnersAsync();
				_allPartners = list;
				MainThread.BeginInvokeOnMainThread(() =>
				{
					Partners.Clear();
					foreach (var p in list) Partners.Add(p);
				});
			}
			catch { }
		}

		// NOVO MÉTOD0: Carregar Distribuidores
		private async Task LoadSuppliers()
		{
			try
			{
				var list = await _databaseService.GetSuppliersAsync();
				MainThread.BeginInvokeOnMainThread(() =>
				{
					Suppliers.Clear();
					foreach (var s in list) Suppliers.Add(s);
				});
			}
			catch { }
		}

		// --- NAVEGAÇÃO ---

		[RelayCommand]
		private async Task GoToProfile() => await Shell.Current.GoToAsync(nameof(UserProfilePage));

		[RelayCommand]
		private async Task SelectPartner(Partner partner)
		{
			if (partner == null) return;
			await Shell.Current.GoToAsync($"{nameof(PartnerDetailsPage)}?PartnerId={partner.Id}&PartnerName={partner.Name}");
		}

		[RelayCommand]
		private async Task Refresh() => await Initialize();

		[RelayCommand]
		private async Task GoToSearch() => await Shell.Current.GoToAsync(nameof(MapSearchPage));

		// --- BUSCA EM TEMPO REAL ---
		async partial void OnSearchTextChanged(string value)
		{
			await Task.Run(() =>
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					MainThread.BeginInvokeOnMainThread(() => { Partners.Clear(); foreach (var p in _allPartners) Partners.Add(p); });
				}
				else
				{
					var filtered = _allPartners.Where(p => p.Name != null && p.Name.ToLower().Contains(value.ToLower())).ToList();
					MainThread.BeginInvokeOnMainThread(() => { Partners.Clear(); foreach (var p in filtered) Partners.Add(p); });
				}
			});
		}
	}
}