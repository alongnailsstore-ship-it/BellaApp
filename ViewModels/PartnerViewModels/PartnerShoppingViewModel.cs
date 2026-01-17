using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public partial class PartnerShoppingViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<ShoppingItem> Items { get; set; } = new ObservableCollection<ShoppingItem>();

		[ObservableProperty]
		private string newItemName = "";

		[ObservableProperty]
		private bool isBusy;

		public PartnerShoppingViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadList()
		{
			if (IsBusy) return;
			IsBusy = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();

				// Se não tiver usuário logado, para aqui
				if (string.IsNullOrEmpty(userId)) return;

				var list = await _databaseService.GetShoppingListAsync(userId);

				Items.Clear();
				if (list != null)
				{
					foreach (var i in list) Items.Add(i);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"ERRO SHOPPING: {ex.Message}");
				await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível carregar a lista.", "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task AddItem()
		{
			if (string.IsNullOrWhiteSpace(NewItemName)) return;

			try
			{
				var userId = await _authService.GetUserIdAsync();

				// Garante que temos um ID de usuário antes de criar o item
				if (string.IsNullOrEmpty(userId))
				{
					await Shell.Current.DisplayAlertAsync("Erro", "Usuário não identificado.", "OK");
					return;
				}

				var item = new ShoppingItem
				{
					PartnerId = userId,
					Name = NewItemName,
					IsBought = false
				};

				await _databaseService.AddShoppingItemAsync(item);
				Items.Add(item);
				NewItemName = ""; // Limpa o campo
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao salvar: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task ToggleItem(ShoppingItem item)
		{
			try
			{
				// Só atualiza se o item e seus IDs forem válidos
				if (item != null && !string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.PartnerId))
				{
					await _databaseService.UpdateShoppingItemAsync(item);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro ao atualizar item: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task DeleteItem(ShoppingItem item)
		{
			if (item == null) return;

			// CORREÇÃO CRÍTICA CS8604: Verifica se as strings são nulas antes de passar para o DatabaseService
			if (string.IsNullOrEmpty(item.PartnerId) || string.IsNullOrEmpty(item.Id))
			{
				// Se faltar ID, remove apenas visualmente da lista (pois não existe no banco ou está corrompido)
				Items.Remove(item);
				return;
			}

			try
			{
				await _databaseService.DeleteShoppingItemAsync(item.PartnerId, item.Id);
				Items.Remove(item);
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao excluir: {ex.Message}", "OK");
			}
		}

		[RelayCommand]
		private async Task GoBack()
		{
			await Shell.Current.GoToAsync("..");
		}
	}
}