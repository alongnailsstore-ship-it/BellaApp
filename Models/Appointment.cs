using System;

namespace BellaLink.App.Models
{
	public class Appointment
	{
		public string? Id { get; set; }

		// --- DADOS DO CLIENTE (Resolve CS0117 e CS1061) ---
		public string? ClientId { get; set; }
		public string? ClientName { get; set; }  // <--- ESSENCIAL
		public string? ClientEmail { get; set; } // <--- ESSENCIAL

		// DADOS DO PARCEIRO
		public string? PartnerId { get; set; }
		public string? PartnerName { get; set; }

		public string? ServiceName { get; set; }

		public DateTime Date { get; set; }
		public string? Status { get; set; } // "Pendente", "Confirmado", "Cancelado"
		public decimal Price { get; set; }
		public string? PaymentMethod { get; set; }
	}
}