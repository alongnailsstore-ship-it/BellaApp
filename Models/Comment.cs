using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BellaLink.App.Models
{
	public partial class Comment : ObservableObject
	{
		public string? Id { get; set; }
		public string? VideoId { get; set; }
		public string? UserId { get; set; }
		public string? UserName { get; set; }
		public string? UserPhoto { get; set; }
		public string? Text { get; set; }
		public DateTime CreatedAt { get; set; }

		// ID do Pai (para saber se é resposta)
		public string? ParentId { get; set; }

		// --- LÓGICA DE "VER MAIS" ---

		// 1. Guarda TODAS as respostas na memória (privado)
		private List<Comment> _allReplies = new List<Comment>();

		// 2. Lista que a TELA vê (começa com apenas 3)
		[ObservableProperty]
		private ObservableCollection<Comment> visibleReplies = new ObservableCollection<Comment>();

		// 3. Controla a visibilidade do botão "Ver mais..."
		[ObservableProperty]
		private bool hasMoreReplies;

		// 4. Texto do botão (ex: "Ver mais 5 respostas")
		[ObservableProperty]
		private string? showMoreText;

		// --- MÉTODOS AUXILIARES ---

		// Inicializa a lista (chamado ao carregar do banco)
		public void SetReplies(List<Comment> replies)
		{
			_allReplies = replies ?? new List<Comment>();
			UpdateVisibility(false); // false = mostra só as primeiras (modo compacto)
		}

		// Adiciona uma nova resposta (chamado quando VOCÊ comenta agora)
		public void AddReply(Comment reply)
		{
			_allReplies.Add(reply);

			// Se eu acabei de responder, quero ver minha resposta imediatamente
			if (!VisibleReplies.Contains(reply))
			{
				VisibleReplies.Add(reply);
			}
		}

		// Exibe o restante (chamado pelo botão "Ver Mais")
		public void ShowAll()
		{
			UpdateVisibility(true);
		}

		private void UpdateVisibility(bool showAll)
		{
			VisibleReplies.Clear();

			if (showAll)
			{
				// Mostra tudo
				foreach (var r in _allReplies) VisibleReplies.Add(r);
				HasMoreReplies = false; // Esconde o botão pois já mostrou tudo
			}
			else
			{
				// Mostra só 3
				var toShow = _allReplies.Take(3).ToList();
				foreach (var r in toShow) VisibleReplies.Add(r);

				int remaining = _allReplies.Count - 3;
				HasMoreReplies = remaining > 0;

				if (HasMoreReplies)
					ShowMoreText = $"──── Ver mais {remaining} respostas...";
			}
		}
	}
}