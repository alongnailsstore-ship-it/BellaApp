using CommunityToolkit.Mvvm.ComponentModel;

namespace BellaLink.App.Models
{
	// Usamos ObservableObject para o CheckBox atualizar a UI em tempo real
	public partial class ShoppingItem : ObservableObject
	{
		public string? Id { get; set; }
		public string? PartnerId { get; set; }

		[ObservableProperty] private string name = string.Empty;
		[ObservableProperty] private bool isBought;
	}
}