using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace BellaLink.App.Models
{
	public partial class Order : ObservableObject
	{
		public string? Id { get; set; }

		// Quem comprou (O Parceiro/Salão)
		public string? PartnerId { get; set; }
		public string? PartnerName { get; set; }
		public string? PartnerWhatsApp { get; set; }

		// Quem vendeu (O Distribuidor)
		public string? DistributorId { get; set; }
		public string? DistributorName { get; set; }

		// Detalhes
		public List<OrderItem> Items { get; set; } = new List<OrderItem>();
		public decimal TotalAmount { get; set; }
		public DateTime OrderDate { get; set; } = DateTime.Now;

		// Endereço de Entrega (Snapshot do momento da compra)
		public string? DeliveryAddress { get; set; }

		// Máquina de Estados: "Pendente", "Pago", "Em Separação", "Enviado", "Entregue", "Cancelado"
		[ObservableProperty]
		private string status = "Pendente";

		[ObservableProperty]
		private string paymentStatus = "Aguardando"; // "Pago", "Na Entrega"

		// Propriedades Visuais
		public string ItemsSummary => $"{Items?.Count ?? 0} itens - R$ {TotalAmount:F2}";
		public bool IsActive => Status != "Entregue" && Status != "Cancelado";
	}
}