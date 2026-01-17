using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace BellaLink.App.ViewModels.DistributorViewModels
{
	public partial class DistributorProductsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

		[ObservableProperty] bool isBusy;

		public DistributorProductsViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadProducts()
		{
			if (IsBusy) return;
			IsBusy = true;
			try
			{
				var userId = await _authService.GetUserIdAsync();
				var list = await _databaseService.GetProductsByDistributorAsync(userId);
				Products.Clear();
				foreach (var p in list) Products.Add(p);
			}
			catch (System.Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
			}
			finally { IsBusy = false; }
		}

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");

		[RelayCommand]
		private async Task ToggleAddNew()
		{
			string result = await Shell.Current.DisplayPromptAsync("Novo Produto", "Digite o nome do produto:");
			if (string.IsNullOrWhiteSpace(result)) return;

			string priceStr = await Shell.Current.DisplayPromptAsync("Preço", "Digite o valor (ex: 29.90):", keyboard: Keyboard.Numeric);
			if (!decimal.TryParse(priceStr, out decimal price)) return;

			string stockStr = await Shell.Current.DisplayPromptAsync("Estoque", "Quantidade inicial:", keyboard: Keyboard.Numeric);
			if (!int.TryParse(stockStr, out int stock)) return;

			var userId = await _authService.GetUserIdAsync();

			var newProd = new Product
			{
				Name = result,
				Price = price,
				StockQuantity = stock,
				DistributorId = userId
			};

			await _databaseService.SaveProductAsync(newProd);
			await LoadProducts();
		}

		[RelayCommand]
		private async Task DeleteProduct(Product product)
		{
			if (product == null) return;
			bool confirm = await Shell.Current.DisplayAlert("Excluir", $"Apagar {product.Name}?", "Sim", "Não");
			if (!confirm) return;

			// CORREÇÃO: Passando os dois argumentos necessários
			var userId = await _authService.GetUserIdAsync();
			await _databaseService.DeleteProductAsync(userId, product.Id);

			Products.Remove(product);
		}
	}
}