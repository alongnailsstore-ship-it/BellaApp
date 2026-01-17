using System;

namespace BellaLink.App.Models
{
	public class FinancialTransaction
	{
		public string? Id { get; set; }
		public string? PartnerId { get; set; }
		public string? Description { get; set; } // Ex: "Corte João", "Compra Shampoo"
		public decimal Amount { get; set; }
		public bool IsIncome { get; set; } // True = Entrada, False = Saída
		public DateTime Date { get; set; } = DateTime.Now;
		public string? Category { get; set; } // Serviço, Produto, Aluguel, etc.
	}
}