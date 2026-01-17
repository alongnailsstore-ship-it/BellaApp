using System;

namespace BellaLink.App.Models
{
	public class Report
	{
		public string? Id { get; set; }
		public string? VideoId { get; set; }
		public string? VideoUrl { get; set; }
		public string? ReporterId { get; set; }
		public string? Reason { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public string Status { get; set; } = "Pendente";
	}
}