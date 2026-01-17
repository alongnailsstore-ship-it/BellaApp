using BellaLink.App.Models;
using BellaLink.App.Services;
using BellaLink.App.Views.ConsumerViews;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	[QueryProperty(nameof(PartnerId), "PartnerId")]
	[QueryProperty(nameof(PartnerName), "PartnerName")]
	public partial class PartnerDetailsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		[ObservableProperty] string partnerId = "";
		[ObservableProperty] string partnerName = "";
		[ObservableProperty] Partner currentPartner = new Partner();
		[ObservableProperty] bool isBusy;
		[ObservableProperty] int followersCount;

		// --- CONTROLE DE EDIÇÃO ---
		[ObservableProperty] bool canEditAbout;

		// Avaliações
		private List<Review> _allReviews = new List<Review>();
		public ObservableCollection<Review> VisibleReviews { get; set; } = new ObservableCollection<Review>();
		[ObservableProperty] private bool hasMoreReviews;
		[ObservableProperty] private int totalReviewsCount;
		[ObservableProperty] private bool areReviewsExpanded;

		public PartnerDetailsViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadPartnerData()
		{
			if (IsBusy || string.IsNullOrEmpty(PartnerId)) return;
			IsBusy = true;

			try
			{
				// 1. Carrega o Parceiro
				var partner = await _databaseService.GetPartnerAsync(PartnerId);
				if (partner != null) CurrentPartner = partner;

				// 2. Verifica Permissões de Edição
				var myId = await _authService.GetUserIdAsync();

				// Dono do perfil pode editar
				bool isOwner = (!string.IsNullOrEmpty(myId) && myId == PartnerId);

				CanEditAbout = isOwner;

				// 3. Carrega Reviews e Seguidores
				await LoadReviews();
				FollowersCount = await _databaseService.GetFollowerCountAsync(PartnerId);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro ao carregar perfil: {ex.Message}");
			}
			finally
			{
				IsBusy = false;
			}
		}

		// --- COMANDO DE EDITAR (CORRIGIDO) ---
		[RelayCommand]
		private async Task EditAbout()
		{
			if (!CanEditAbout) return;

			string result = await Shell.Current.DisplayPromptAsync(
				"Editar Sobre",
				"Descreva seu negócio e diferenciais:",
				initialValue: CurrentPartner.Description,
				maxLength: 500,
				keyboard: Keyboard.Text);

			// Se o usuário cancelou (null), não faz nada. 
			// Se limpou o texto (vazio), permitimos salvar como vazio.
			if (result != null)
			{
				try
				{
					IsBusy = true;

					// 1. Atualiza Visualmente Imediato
					CurrentPartner.Description = result;

					// 2. Salva no Banco (com tratamento de erro detalhado)
					if (string.IsNullOrEmpty(PartnerId)) throw new Exception("ID do parceiro inválido");

					await _databaseService.UpdatePartnerDescriptionAsync(PartnerId, result);

					await Shell.Current.DisplayAlert("Sucesso", "Descrição atualizada!", "OK");
				}
				catch (Exception ex)
				{
					// AQUI: Mostra o erro real para sabermos o motivo (ex: Permissão negada)
					await Shell.Current.DisplayAlert("Erro ao Salvar", ex.Message, "OK");

					// Reverte visualmente se der erro (opcional)
					// await LoadPartnerData(); 
				}
				finally
				{
					IsBusy = false;
				}
			}
		}

		private async Task LoadReviews()
		{
			_allReviews = await _databaseService.GetReviewsForPartnerAsync(PartnerId);
			TotalReviewsCount = _allReviews.Count;
			AreReviewsExpanded = false;
			UpdateVisibleReviews();
		}

		private void UpdateVisibleReviews()
		{
			VisibleReviews.Clear();
			var itemsToShow = AreReviewsExpanded ? _allReviews : _allReviews.Take(3).ToList();
			foreach (var item in itemsToShow) VisibleReviews.Add(item);
			HasMoreReviews = !AreReviewsExpanded && _allReviews.Count > 3;
		}

		[RelayCommand]
		private void ToggleReviews()
		{
			AreReviewsExpanded = !AreReviewsExpanded;
			UpdateVisibleReviews();
		}

		[RelayCommand]
		private async Task OpenNavigation()
		{
			if (CurrentPartner == null || string.IsNullOrEmpty(CurrentPartner.Address)) return;
			try
			{
				var locations = await Geocoding.GetLocationsAsync(CurrentPartner.Address);
				var location = locations?.FirstOrDefault();
				if (location != null) await Map.OpenAsync(location, new MapLaunchOptions { Name = CurrentPartner.Name });
			}
			catch { }
		}

		[RelayCommand]
		private async Task GoToBooking() => await Shell.Current.GoToAsync($"{nameof(BookingDateTimePage)}?PartnerId={PartnerId}&PartnerName={PartnerName}");

		[RelayCommand]
		private async Task GoToReview() => await Shell.Current.GoToAsync($"{nameof(RateServicePage)}?PartnerId={PartnerId}&PartnerName={PartnerName}");

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}