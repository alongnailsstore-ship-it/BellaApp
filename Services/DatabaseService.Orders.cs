using Firebase.Database.Query;
using BellaLink.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public partial class DatabaseService
	{
		private const string OrdersNode = "orders";

		// Criar um novo pedido (Chamado pelo Parceiro ao comprar)
		public async Task<string> CreateOrderAsync(Order order)
		{
			if (string.IsNullOrEmpty(order.Id)) order.Id = Guid.NewGuid().ToString();

			// Salva no nó global de pedidos
			await _client.Child(OrdersNode).Child(order.Id).PutAsync(order);

			// Aqui deveríamos debitar o estoque (podemos fazer isso depois ou agora)
			// Para simplificar o MVP, assumimos que o estoque foi verificado na tela de carrinho

			return order.Id;
		}

		// Buscar pedidos do Distribuidor (Para ele gerenciar)
		public async Task<List<Order>> GetOrdersForDistributorAsync(string distributorId)
		{
			try
			{
				var allOrders = await _client.Child(OrdersNode).OnceAsync<Order>();
				return allOrders
					.Select(x => x.Object)
					.Where(o => o.DistributorId == distributorId)
					.OrderByDescending(o => o.OrderDate)
					.ToList();
			}
			catch
			{
				return new List<Order>();
			}
		}

		// Buscar pedidos do Parceiro (Histórico de compras)
		public async Task<List<Order>> GetOrdersForPartnerAsync(string partnerId)
		{
			try
			{
				var allOrders = await _client.Child(OrdersNode).OnceAsync<Order>();
				return allOrders
					.Select(x => x.Object)
					.Where(o => o.PartnerId == partnerId)
					.OrderByDescending(o => o.OrderDate)
					.ToList();
			}
			catch
			{
				return new List<Order>();
			}
		}

		// Atualizar Status (Avançar etapa: Pendente -> Enviado -> etc)
		public async Task UpdateOrderStatusAsync(string orderId, string newStatus, string paymentStatus = null)
		{
			if (string.IsNullOrEmpty(orderId)) return;

			await _client.Child(OrdersNode).Child(orderId).Child("Status").PutAsync(newStatus);

			if (!string.IsNullOrEmpty(paymentStatus))
			{
				await _client.Child(OrdersNode).Child(orderId).Child("PaymentStatus").PutAsync(paymentStatus);
			}
		}
	}
}