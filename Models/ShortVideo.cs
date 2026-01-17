using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace BellaLink.App.Models
{
	public class ShortVideoItem
	{
		public string? VideoUrl { get; set; }
		public int DurationSeconds { get; set; }
	}

	public partial class ShortVideo : ObservableObject
	{
		public string? Id { get; set; }
		public string? PartnerId { get; set; }
		public string? PartnerName { get; set; }
		public string? PartnerPhoto { get; set; }

		public string? Title { get; set; }
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public string? RealVideoUrl { get; set; }

		[ObservableProperty]
		private bool isPlaying;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(HasVideoUrl))]
		private string? currentVideoUrl;

		public bool HasVideoUrl => !string.IsNullOrEmpty(CurrentVideoUrl);

		public bool IsPlaylist { get; set; }
		public List<ShortVideoItem> PlaylistItems { get; set; } = new List<ShortVideoItem>();

		// Métricas
		[ObservableProperty][NotifyPropertyChangedFor(nameof(RankingScore))] private int likes;
		[ObservableProperty][NotifyPropertyChangedFor(nameof(RankingScore))] private int dislikes;
		[ObservableProperty][NotifyPropertyChangedFor(nameof(RankingScore))] private int views;
		[ObservableProperty] private int commentsCount;

		[ObservableProperty] private bool isLikedByMe;
		[ObservableProperty] private bool isDislikedByMe;
		[ObservableProperty] private bool isFollowing;

		public double RankingScore
		{
			get
			{
				double engagementScore = (Likes * 2.0) + (Views * 0.5) - (Dislikes * 2.0);
				double hoursOld = (DateTime.Now - CreatedAt).TotalHours;
				double noveltyBonus = hoursOld < 24 ? 500 : (hoursOld < 48 ? 250 : 0);
				return engagementScore + noveltyBonus;
			}
		}
	}
}