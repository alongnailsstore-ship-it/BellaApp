#pragma warning disable CS0618
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BellaLink.App.Views;
using BellaLink.App.Views.PartnerViews;
using Microsoft.Maui.ApplicationModel;
using BellaLink.App.Models;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public class Supplier
	{
		public string? Name { get; set; }
		public string? Category { get; set; }
		public double Rating { get; set; }
		public string? Distance { get; set; }
	}

	public partial class PartnerDashboardViewModel : ObservableObject
	{
		public ObservableCollection<PromoBanner> Banners { get; set; }
		public ObservableCollection<Supplier> TopSuppliers { get; set; }

		[ObservableProperty]
		private string partnerName = "Minha Loja";

		public PartnerDashboardViewModel()
		{
			Banners = new ObservableCollection<PromoBanner>();
			TopSuppliers = new ObservableCollection<Supplier>();
			LoadData();
		}

		private void LoadData()
		{
			// Dados Mockados para teste visual
			Banners.Add(new PromoBanner
			{
				Title = "Aumente seus agendamentos",
				ImageUrl = "https://images.unsplash.com/photo-1600607686527-6fb886090705"
			});
			Banners.Add(new PromoBanner
			{
				Title = "Publique seu primeiro Short!",
				ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32"
			});

			TopSuppliers.Add(new Supplier { Name = "Beleza Distribuidora", Category = "Cosméticos", Rating = 4.9, Distance = "2.0 km" });
			TopSuppliers.Add(new Supplier { Name = "Descartáveis Express", Category = "Higiene", Rating = 4.7, Distance = "1.0 km" });
		}

		// --- NAVEGAÇÃO ---

		[RelayCommand]
		private async Task GoToPromo()
		{
			await Shell.Current.GoToAsync(nameof(PartnerPromoPage));
		}

		[RelayCommand]
		private async Task GoToUpload()
		{
			await Shell.Current.GoToAsync(nameof(PartnerUploadPage));
		}

		// NOVO: Navegação para o Financeiro
		[RelayCommand]
		private async Task GoToFinancial()
		{
			await Shell.Current.GoToAsync(nameof(PartnerFinancialPage));
		}

		// NOVO: Navegação para Compras
		[RelayCommand]
		private async Task GoToShopping()
		{
			await Shell.Current.GoToAsync(nameof(PartnerShoppingPage));
		}

		[RelayCommand]
		private async Task GoToProfile()
		{
			await Shell.Current.GoToAsync(nameof(UserProfilePage));
		}

		[RelayCommand]
		private void ExitToProfiles()
		{
			if (Application.Current != null && Application.Current.Windows.Count > 0)
			{
				Shell.Current.GoToAsync($"//{nameof(SelectProfilePage)}");
			}
		}

		[RelayCommand]
		private async Task OpenBanner(PromoBanner banner)
		{
			if (banner == null) return;
			await Shell.Current.DisplayAlertAsync("Novidade", $"Abrindo: {banner.Title}", "OK");
		}
	}
}