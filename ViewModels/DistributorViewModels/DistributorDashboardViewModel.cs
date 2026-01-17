using BellaLink.App.Services;
using BellaLink.App.Views.DistributorViews;
using BellaLink.App.Views.PartnerViews;
using BellaLink.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.DistributorViewModels
{
	public partial class DistributorDashboardViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		[ObservableProperty] private string storeName = "Carregando...";
		[ObservableProperty] private string ownerName = "";
		[ObservableProperty] private string logoUrl = "https://via.placeholder.com/150";
		[ObservableProperty] private bool isBusy;

		[ObservableProperty] private int activeProductsCount;
		[ObservableProperty] private int pendingOrdersCount;
		[ObservableProperty] private decimal monthRevenue;

		public DistributorDashboardViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task Initialize()
		{
			if (IsBusy) return;
			IsBusy = true;
			try
			{
				var userId = await _authService.GetUserIdAsync();
				var user = await _databaseService.GetUserAsync(userId);
				if (user != null)
				{
					// Tratamento seguro de Nulos (CS8601)
					string nameSafe = user.Name ?? "Usuário";
					StoreName = !string.IsNullOrEmpty(user.StoreName) ? user.StoreName : nameSafe;
					OwnerName = nameSafe;

					if (!string.IsNullOrEmpty(user.SupplierPhoto)) LogoUrl = user.SupplierPhoto;
					else if (!string.IsNullOrEmpty(user.PartnerPhoto)) LogoUrl = user.PartnerPhoto;
					else if (!string.IsNullOrEmpty(user.ConsumerPhoto)) LogoUrl = user.ConsumerPhoto;

					ActiveProductsCount = 12; // Simulação
					MonthRevenue = 1500.00m;  // Simulação
				}
			}
			finally { IsBusy = false; }
		}

		[RelayCommand]
		private async Task GoBack()
		{
			// ✅ CORREÇÃO: Remove o uso obsoleto de Application.MainPage
			await Shell.Current.GoToAsync("//LoginPage");
		}

		[RelayCommand]
		private async Task GoToProfile() => await Shell.Current.GoToAsync(nameof(UserProfilePage));

		[RelayCommand]
		private async Task GoToProducts() => await Shell.Current.GoToAsync(nameof(DistributorProductsPage));

		[RelayCommand]
		private async Task GoToOrders() => await Shell.Current.GoToAsync(nameof(DistributorOrdersPage));

		[RelayCommand]
		private async Task CreatePromoBanner() => await Shell.Current.GoToAsync(nameof(PartnerPromoPage));

		[RelayCommand]
		private async Task GoToShortsUpload() => await Shell.Current.GoToAsync(nameof(PartnerUploadPage));
	}
}