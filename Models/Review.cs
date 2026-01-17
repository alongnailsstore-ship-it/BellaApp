using System;

namespace BellaLink.App.Models
{
	public class Review
	{
		public string? Id { get; set; }
		public string? PartnerId { get; set; } // O profissional avaliado
		public string? ConsumerId { get; set; } // Quem avaliou
		public string? ConsumerName { get; set; }
		public int Rating { get; set; } // Nota de 1 a 5
		public string? Comment { get; set; } // Comentário opcional
		public DateTime Date { get; set; } = DateTime.Now;
	}
}