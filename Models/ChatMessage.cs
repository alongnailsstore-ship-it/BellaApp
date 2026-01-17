using System;

namespace BellaLink.App.Models
{
	// Enum para os Ticks (Enviado, Recebido, Lido)
	public enum MessageStatus { Sending, Sent, Delivered, Read }

	public class ChatMessage
	{
		public string? Id { get; set; }
		public string? SenderId { get; set; }
		public string? ReceiverId { get; set; }
		public string? Text { get; set; }
		public string? ImageUrl { get; set; }
		public DateTime Timestamp { get; set; } = DateTime.Now;

		public bool IsMine { get; set; }
		public MessageStatus Status { get; set; } = MessageStatus.Sent;

		// --- Propriedades para Controle Visual (Não salvas no banco) ---
		public bool IsHeader { get; set; } // Se true, é um separador de data (Ex: "Hoje")
		public string? HeaderText { get; set; } // O texto do separador
	}
}