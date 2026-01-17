using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace BellaLink.App.ViewModels.DistributorViewModels
{
	public partial class DistributorOrdersViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<Order> Orders { get; set; } = new ObservableCollection<Order>();

		[ObservableProperty] bool isBusy;

		public DistributorOrdersViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadOrders()
		{
			if (IsBusy) return;
			IsBusy = true;
			try
			{
				var userId = await _authService.GetUserIdAsync();
				var list = await _databaseService.GetOrdersForDistributorAsync(userId);

				Orders.Clear();
				foreach (var o in list) Orders.Add(o);
			}
			catch (System.Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", $"Falha: {ex.Message}", "OK");
			}
			finally { IsBusy = false; }
		}

		[RelayCommand]
		private async Task GoBack()
		{
			await Shell.Current.GoToAsync("..");
		}

		[RelayCommand]
		private async Task AdvanceStatus(Order order)
		{
			if (order == null) return;

			string action = await Shell.Current.DisplayActionSheet(
				$"Pedido {order.Id?.Substring(0, 4)}...",
				"Voltar",
				null,
				"Confirmar Pagamento",
				"Marcar como Enviado",
				"Confirmar Entrega",
				"Cancelar Pedido");

			if (action == "Voltar" || action == null) return;

			string newStatus = order.Status;
			string? paymentStatus = null;

			if (action == "Confirmar Pagamento") paymentStatus = "Pago";
			else if (action == "Marcar como Enviado") newStatus = "Enviado";
			else if (action == "Confirmar Entrega") newStatus = "Entregue";
			else if (action == "Cancelar Pedido") newStatus = "Cancelado";

			try
			{
				// CORREÇÃO CS8604: Enviando string vazia se for nulo
				await _databaseService.UpdateOrderStatusAsync(order.Id ?? "", newStatus, paymentStatus ?? "");

				order.Status = newStatus;
				if (paymentStatus != null) order.PaymentStatus = paymentStatus;

				await Shell.Current.DisplayAlert("Sucesso", "Status atualizado!", "OK");
			}
			catch (System.Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
			}
		}
	}
}