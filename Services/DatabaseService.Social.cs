using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using BellaLink.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public partial class DatabaseService
	{
		// --- SHORTS & VÍDEOS ---
		public async Task<List<ShortVideo>> GetShortVideosAsync() { try { var items = await _client.Child(ShortsNode).OnceAsync<ShortVideo>(); return items != null ? items.Select(x => x.Object).ToList() : new List<ShortVideo>(); } catch { return new List<ShortVideo>(); } }
		public async Task<List<ShortVideo>> GetVideosByPartnerAsync(string partnerId) { var all = await GetShortVideosAsync(); return all.Where(v => v.PartnerId == partnerId).ToList(); }
		public async Task SaveShortAsync(ShortVideo video) { if (string.IsNullOrEmpty(video.Id)) video.Id = Guid.NewGuid().ToString(); await _client.Child(ShortsNode).Child(video.Id).PutAsync(video); }
		public async Task DeleteShortVideoAsync(string videoId) { if (!string.IsNullOrEmpty(videoId)) await _client.Child(ShortsNode).Child(videoId).DeleteAsync(); }
		public async Task ReportVideoAsync(Report report) { await _client.Child(ReportsNode).PostAsync(report); }
		public async Task IncrementVideoCommentCountAsync(string videoId) { if (string.IsNullOrEmpty(videoId)) return; try { var video = await _client.Child(ShortsNode).Child(videoId).OnceSingleAsync<ShortVideo>(); if (video != null) { int newCount = video.CommentsCount + 1; await _client.Child(ShortsNode).Child(videoId).Child("CommentsCount").PutAsync(newCount); } } catch { } }

		// --- BANNERS ---
		public async Task SaveBannerAsync(PromoBanner banner) { if (string.IsNullOrEmpty(banner.Id)) banner.Id = Guid.NewGuid().ToString(); await _client.Child(BannersNode).Child(banner.Id).PutAsync(banner); }
		public async Task<List<PromoBanner>> GetActiveBannersAsync() { try { var all = await _client.Child(BannersNode).OnceAsync<PromoBanner>(); return all.Select(x => x.Object).ToList(); } catch { return new List<PromoBanner>(); } } // Removed .Where(IsActive) filter inside query to match original logic, logic handled in VM or here if property exists

		// --- SEGUIDORES ---
		public async Task<int> GetFollowerCountAsync(string partnerId) { if (string.IsNullOrEmpty(partnerId)) return 0; try { var items = await _client.Child(FollowersNode).Child(partnerId).OnceAsync<object>(); return items.Count; } catch { return 0; } }
		public async Task FollowPartnerAsync(string userId, string partnerId) { if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(partnerId)) await _client.Child(FollowersNode).Child(partnerId).Child(userId).PutAsync(true); }
		public async Task UnfollowPartnerAsync(string userId, string partnerId) { if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(partnerId)) await _client.Child(FollowersNode).Child(partnerId).Child(userId).DeleteAsync(); }
		public async Task<bool> IsUserFollowingAsync(string userId, string partnerId) { if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(partnerId)) return false; try { return await _client.Child(FollowersNode).Child(partnerId).Child(userId).OnceSingleAsync<bool>(); } catch { return false; } }

		// --- COMENTÁRIOS ---
		public async Task AddCommentAsync(Comment comment) { if (!string.IsNullOrEmpty(comment.VideoId)) await _client.Child(CommentsNode).Child(comment.VideoId).PostAsync(comment); }
		public async Task<List<Comment>> GetCommentsAsync(string videoId) { if (string.IsNullOrEmpty(videoId)) return new List<Comment>(); try { var items = await _client.Child(CommentsNode).Child(videoId).OnceAsync<Comment>(); return items.Select(x => x.Object).OrderBy(c => c.CreatedAt).ToList(); } catch { return new List<Comment>(); } }
		public async Task<List<Comment>> GetCommentsByVideoIdAsync(string videoId) { return await GetCommentsAsync(videoId); }
		public async Task ReportCommentAsync(string commentId, string commentText, string reportedUserId, string reason, string reporterId) { var report = new { Id = Guid.NewGuid().ToString(), CommentId = commentId, CommentText = commentText, Reason = reason, ReporterId = reporterId }; await _client.Child(CommentReportsNode).PostAsync(report); }

		// --- AVALIAÇÕES (REVIEWS) ---
		public async Task AddReviewAsync(Review review) { if (string.IsNullOrEmpty(review.Id)) review.Id = Guid.NewGuid().ToString(); if (!string.IsNullOrEmpty(review.PartnerId)) { await _client.Child(ReviewsNode).Child(review.PartnerId).Child(review.Id).PutAsync(review); await RecalculatePartnerRating(review.PartnerId); } }
		public async Task<List<Review>> GetReviewsForPartnerAsync(string partnerId) { if (string.IsNullOrEmpty(partnerId)) return new List<Review>(); try { var items = await _client.Child(ReviewsNode).Child(partnerId).OnceAsync<Review>(); return items.Select(x => x.Object).OrderByDescending(r => r.Date).ToList(); } catch { return new List<Review>(); } }
		private async Task RecalculatePartnerRating(string partnerId) { try { var reviews = await _client.Child(ReviewsNode).Child(partnerId).OnceAsync<Review>(); if (reviews.Count > 0) { double avg = reviews.Average(r => r.Object.Rating); await _client.Child(PartnersNode).Child(partnerId).Child("Rating").PutAsync(avg); } } catch { } }

		// --- CHAT (LÓGICA COMPLETA RESTAURADA) ---
		public string GetChatRoomId(string userA, string userB) { return string.Compare(userA, userB) < 0 ? $"{userA}_{userB}" : $"{userB}_{userA}"; }
		public async Task<List<ChatMessage>> GetChatHistoryAsync(string roomId) { try { var items = await _client.Child(MessagesNode).Child(roomId).OnceAsync<ChatMessage>(); return items.Select(x => x.Object).OrderBy(m => m.Timestamp).ToList(); } catch { return new List<ChatMessage>(); } }
		public async Task MarkMessageAsReadAsync(string roomId, string messageId) { await _client.Child(MessagesNode).Child(roomId).Child(messageId).Child("Status").PutAsync((int)MessageStatus.Read); }

		public async Task SendMessageAsync(ChatMessage msg, string roomId, string myName, string otherUserId, string otherUserName, string otherUserPhoto, string myPhotoUrl)
		{
			if (string.IsNullOrEmpty(msg.Id)) msg.Id = Guid.NewGuid().ToString();

			// 1. Salva a mensagem no histórico
			await _client.Child(MessagesNode).Child(roomId).Child(msg.Id).PutAsync(msg);

			// 2. Atualiza a lista de contatos do REMETENTE (Eu)
			string finalOtherPhoto = !string.IsNullOrEmpty(otherUserPhoto) ? otherUserPhoto : "https://ui-avatars.com/api/?background=CCCCCC&color=fff&name=" + (otherUserName ?? "U");
			var mySummary = new ChatContact
			{
				ChatRoomId = roomId,
				ContactId = otherUserId,
				Name = otherUserName,
				Photo = finalOtherPhoto,
				LastMessage = string.IsNullOrEmpty(msg.ImageUrl) ? msg.Text : "📷 Imagem",
				LastUpdate = DateTime.Now,
				UnreadCount = 0
			};
			await _client.Child(ChatsNode).Child(msg.SenderId).Child(otherUserId).PutAsync(mySummary);

			// 3. Atualiza a lista de contatos do DESTINATÁRIO (O outro)
			string finalMyPhoto = !string.IsNullOrEmpty(myPhotoUrl) ? myPhotoUrl : "https://ui-avatars.com/api/?background=FF4081&color=fff&name=" + (myName ?? "E");
			var otherSummary = new ChatContact
			{
				ChatRoomId = roomId,
				ContactId = msg.SenderId,
				Name = myName,
				Photo = finalMyPhoto,
				LastMessage = string.IsNullOrEmpty(msg.ImageUrl) ? msg.Text : "📷 Imagem",
				LastUpdate = DateTime.Now,
				UnreadCount = 1
			};
			await _client.Child(ChatsNode).Child(otherUserId).Child(msg.SenderId).PutAsync(otherSummary);
		}

		public async Task<List<ChatContact>> GetMyChatsAsync(string userId) { try { var list = await _client.Child(ChatsNode).Child(userId).OnceAsync<ChatContact>(); return list.Select(x => x.Object).OrderByDescending(c => c.LastUpdate).ToList(); } catch { return new List<ChatContact>(); } }
		public IObservable<FirebaseEvent<ChatMessage>> ListenToMessages(string roomId) { return _client.Child(MessagesNode).Child(roomId).AsObservable<ChatMessage>(); }
	}
}