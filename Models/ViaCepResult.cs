using System.Text.Json.Serialization;

namespace BellaLink.App.Models
{
	public class ViaCepResult
	{
		[JsonPropertyName("cep")]
		public string? Cep { get; set; }

		[JsonPropertyName("logradouro")]
		public string? Logradouro { get; set; } // Rua

		[JsonPropertyName("complemento")]
		public string? Complemento { get; set; }

		[JsonPropertyName("bairro")]
		public string? Bairro { get; set; }

		[JsonPropertyName("localidade")]
		public string? Localidade { get; set; } // Cidade

		[JsonPropertyName("uf")]
		public string? Uf { get; set; } // Estado

		[JsonPropertyName("erro")]
		public bool Erro { get; set; }
	}
}