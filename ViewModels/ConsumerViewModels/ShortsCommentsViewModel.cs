using BellaLink.App.Helpers;
using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	[QueryProperty(nameof(VideoId), "VideoId")]
	public partial class ShortsCommentsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		[ObservableProperty] string? videoId;
		[ObservableProperty] string? newCommentText;
		[ObservableProperty] bool isBusy;

		[ObservableProperty] bool isReplying;
		[ObservableProperty] string? replyingToName;
		private string? _replyingToCommentId;

		public ObservableCollection<Comment> Comments { get; set; } = new ObservableCollection<Comment>();

		public ShortsCommentsViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadComments()
		{
			if (string.IsNullOrEmpty(VideoId)) return;
			IsBusy = true;
			try
			{
				var comments = await _databaseService.GetCommentsAsync(VideoId);
				Comments.Clear();
				foreach (var c in comments) Comments.Add(c);
			}
			finally { IsBusy = false; }
		}

		[RelayCommand]
		private async Task SendComment()
		{
			if (string.IsNullOrWhiteSpace(NewCommentText)) return;

			try
			{
				var userId = await _authService.GetUserIdAsync();
				var user = await _databaseService.GetUserAsync(userId);

				var comment = new Comment
				{
					VideoId = VideoId,
					UserId = userId,
					UserName = user?.Name ?? "Usuário",
					UserPhoto = user?.ConsumerPhoto ?? "https://via.placeholder.com/50",
					Text = NewCommentText,
					CreatedAt = DateTime.Now,
					ParentId = _replyingToCommentId
				};

				await _databaseService.AddCommentAsync(comment);
				await _databaseService.IncrementVideoCommentCountAsync(VideoId ?? "");

				WeakReferenceMessenger.Default.Send(new CommentAddedMessage(VideoId ?? ""));

				if (string.IsNullOrEmpty(comment.ParentId))
				{
					Comments.Insert(0, comment);
				}
				else
				{
					var parent = Comments.FirstOrDefault(c => c.Id == comment.ParentId);
					if (parent != null)
					{
						parent.AddReply(comment);
					}
				}

				CancelReply();
			}
			catch
			{
				// CORREÇÃO: Shell.Current
				await Shell.Current.DisplayAlert("Erro", "Falha ao enviar.", "OK");
			}
		}

		[RelayCommand]
		private void ReplyToComment(Comment comment)
		{
			if (comment == null) return;
			_replyingToCommentId = string.IsNullOrEmpty(comment.ParentId) ? comment.Id : comment.ParentId;
			ReplyingToName = $"Respondendo {comment.UserName}";
			IsReplying = true;
		}

		[RelayCommand]
		private void CancelReply()
		{
			_replyingToCommentId = null;
			IsReplying = false;
			ReplyingToName = "";
			NewCommentText = "";
		}

		[RelayCommand]
		private async Task ReportComment(Comment comment)
		{
			if (comment == null) return;

			// CORREÇÃO: Shell.Current
			string reason = await Shell.Current.DisplayActionSheet(
				"Motivo da denúncia:", "Cancelar", null, "Ofensivo", "Spam", "Golpe", "Assédio");

			if (string.IsNullOrEmpty(reason) || reason == "Cancelar") return;

			await Shell.Current.DisplayAlert("Recebido", "Denúncia enviada para análise.", "OK");
		}
	}
}