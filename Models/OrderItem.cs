namespace BellaLink.App.Models
{
	public class OrderItem
	{
		public string? ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? ProductImageUrl { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal Total => Quantity * UnitPrice;
	}
}