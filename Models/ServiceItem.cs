namespace BellaLink.App.Models
{
	public class ServiceItem
	{
		public string? Id { get; set; }
		public string? Name { get; set; }

		// NOVO CAMPO
		public string? Category { get; set; }

		public decimal Price { get; set; }
		public int DurationMinutes { get; set; }
		public string? Description { get; set; }

		// Helpers de exibição
		public string DurationDisplay => $"{DurationMinutes} min";
		public string PriceDisplay => $"R$ {Price:F2}";

		// Helper para mostrar categoria na lista (opcional)
		public string CategoryDisplay => string.IsNullOrEmpty(Category) ? "Geral" : Category;
	}
}