using CommunityToolkit.Mvvm.ComponentModel; // Necessário para edição em tempo real
using System;

namespace BellaLink.App.Models
{
	public class Category
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Icon { get; set; }
		public string? Color { get; set; }
	}

	// A classe Partner agora é 'partial' e herda de ObservableObject
	// Isso corrige os avisos de nulo e permite a edição do perfil
	public partial class Partner : ObservableObject
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
		public string? Category { get; set; }
		public string? ImageUrl { get; set; }
		public double Rating { get; set; }
		public string? Distance { get; set; }
		public bool IsOpen { get; set; }
		public string? Address { get; set; }

		// Campo 'Sobre' (Descrição) gerenciado pelo ObservableProperty
		[ObservableProperty]
		private string? description;

		// Horários para exibição futura
		public string? WorkStart { get; set; }
		public string? WorkEnd { get; set; }
	}
}