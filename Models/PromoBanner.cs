using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace BellaLink.App.Models
{
	public partial class PromoBanner : ObservableObject
	{
		public PromoBanner()
		{
			// Garante um ID único ao criar
			Id = Guid.NewGuid().ToString();
		}

		public string? Id { get; set; }
		public string? PartnerId { get; set; }
		public string? PartnerName { get; set; }
		public string? ImageUrl { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; } // Adicionado para compatibilidade

		// Controle de Tempo
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public int DurationDays { get; set; }
		public DateTime ExpiresAt => CreatedAt.AddDays(DurationDays);

		// Engajamento
		[ObservableProperty] private int likes;
		[ObservableProperty] private int dislikes;
		[ObservableProperty] private int views;

		// Controle Local
		[ObservableProperty] private bool isLikedByMe;
		[ObservableProperty] private bool isDislikedByMe;

		// Propriedade calculada para saber se o banner ainda vale
		public bool IsActive => DateTime.Now <= ExpiresAt;
	}
}