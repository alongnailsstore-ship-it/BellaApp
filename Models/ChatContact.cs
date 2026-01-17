using System;

namespace BellaLink.App.Models
{
	public class ChatContact
	{
		public string? ChatRoomId { get; set; } // ID único da conversa (Ex: UserA_UserB)
		public string? ContactId { get; set; }  // ID da outra pessoa
		public string? Name { get; set; }
		public string? Photo { get; set; }
		public string? LastMessage { get; set; }
		public DateTime LastUpdate { get; set; }
		public int UnreadCount { get; set; }
	}
}