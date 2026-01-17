using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	[QueryProperty(nameof(SupplierId), "SupplierId")]
	[QueryProperty(nameof(SupplierName), "SupplierName")]
	public partial class SupplierDetailsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;

		[ObservableProperty] private string supplierId = "";
		[ObservableProperty] private string supplierName = "";
		[ObservableProperty] private User currentSupplier = new User();
		[ObservableProperty] private bool isBusy;

		// Catálogo
		public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

		public SupplierDetailsViewModel(DatabaseService db)
		{
			_databaseService = db;
		}

		public async Task LoadData()
		{
			if (string.IsNullOrEmpty(SupplierId)) return;
			IsBusy = true;

			try
			{
				// 1. Carrega Perfil do Fornecedor
				var user = await _databaseService.GetUserAsync(SupplierId);
				if (user != null)
				{
					CurrentSupplier = user;
				}

				// 2. Carrega Produtos
				var products = await _databaseService.GetProductsByDistributorAsync(SupplierId);
				Products.Clear();
				foreach (var p in products)
				{
					Products.Add(p);
				}
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task GoBack()
		{
			await Shell.Current.GoToAsync("..");
		}

		[RelayCommand]
		private async Task ContactSupplier()
		{
			if (CurrentSupplier == null || string.IsNullOrEmpty(CurrentSupplier.WhatsApp))
			{
				await Shell.Current.DisplayAlert("Ops", "Este fornecedor não cadastrou WhatsApp.", "OK");
				return;
			}

			// Abre WhatsApp
			string url = $"https://wa.me/55{CurrentSupplier.WhatsApp}?text=Olá, vi seus produtos no BellaLink!";
			try { await Launcher.OpenAsync(url); } catch { }
		}

		[RelayCommand]
		private async Task AddToCart(Product product)
		{
			// Futuramente implementar carrinho real.
			// Por enquanto, avisa que o pedido deve ser feito via WhatsApp
			bool confirm = await Shell.Current.DisplayAlert("Interesse",
				$"Deseja pedir '{product.Name}' via WhatsApp?", "Sim", "Não");

			if (confirm && CurrentSupplier != null)
			{
				string url = $"https://wa.me/55{CurrentSupplier.WhatsApp}?text=Olá, tenho interesse no produto: {product.Name} (R$ {product.Price:F2})";
				try { await Launcher.OpenAsync(url); } catch { }
			}
		}
	}
}