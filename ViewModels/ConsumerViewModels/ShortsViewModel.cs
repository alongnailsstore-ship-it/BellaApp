using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BellaLink.App.Models;
using BellaLink.App.Services;
using System.Linq;
using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using BellaLink.App.Views.ConsumerViews;
using BellaLink.App.Helpers;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	public partial class ShortsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<ShortVideo> Videos { get; set; } = new ObservableCollection<ShortVideo>();

		[ObservableProperty] private bool isLoading;
		[ObservableProperty] private int currentPosition;

		private IDispatcherTimer? _viewTimer;
		private ShortVideo? _activeVideo;
		private bool _viewCounted;

		public ShortsViewModel(DatabaseService databaseService, IAuthService authService)
		{
			_databaseService = databaseService;
			_authService = authService;

			// Escuta mensagens de atualização de comentários
			WeakReferenceMessenger.Default.Register<CommentAddedMessage>(this, (r, m) =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					var videoToUpdate = Videos.FirstOrDefault(v => v.Id == m.Value);
					if (videoToUpdate != null)
					{
						videoToUpdate.CommentsCount++;
					}
				});
			});
		}

		public async Task LoadVideos()
		{
			if (IsLoading) return;
			IsLoading = true;
			try
			{
				var dbVideos = await _databaseService.GetShortVideosAsync();
				if (dbVideos == null) dbVideos = new List<ShortVideo>();

				// Embaralha para dar efeito de feed
				var shuffled = dbVideos.OrderBy(a => Guid.NewGuid()).ToList();

				// Pega meu ID para checar quem eu sigo
				var myId = await _authService.GetUserIdAsync();

				MainThread.BeginInvokeOnMainThread(async () =>
				{
					Videos.Clear();
					foreach (var v in shuffled)
					{
						v.CurrentVideoUrl = null;
						v.IsPlaying = false;
						if (string.IsNullOrEmpty(v.RealVideoUrl))
						{
							v.RealVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
						}

						// CORREÇÃO: Verifica se já sigo esse parceiro no banco
						// (Nota: Em um app gigante, faríamos isso em lote, mas aqui funciona bem)
						if (!string.IsNullOrEmpty(myId) && !string.IsNullOrEmpty(v.PartnerId))
						{
							bool isFollowing = await _databaseService.IsUserFollowingAsync(myId, v.PartnerId);
							v.IsFollowing = isFollowing;
						}

						Videos.Add(v);
					}
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro ao carregar Shorts: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		public void SetActiveVideo(int index)
		{
			if (_activeVideo != null)
			{
				_activeVideo.IsPlaying = false;
				_activeVideo.CurrentVideoUrl = null;
			}
			StopTimer();

			if (index < 0 || index >= Videos.Count) return;

			_activeVideo = Videos[index];
			_activeVideo.CurrentVideoUrl = _activeVideo.RealVideoUrl;

			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Task.Delay(50);
				if (_activeVideo == Videos[index])
				{
					_activeVideo.IsPlaying = true;
				}
			});

			_viewCounted = false;
			StartTimerForActiveVideo();
		}

		public void StopAll()
		{
			foreach (var v in Videos)
			{
				v.IsPlaying = false;
				v.CurrentVideoUrl = null;
			}
			_activeVideo = null;
			StopTimer();
		}

		private void StartTimerForActiveVideo()
		{
			if (_activeVideo == null) return;
			if (Application.Current?.Dispatcher == null) return;

			double durationSeconds = 60;
			if (_activeVideo.PlaylistItems != null && _activeVideo.PlaylistItems.Any())
				durationSeconds = _activeVideo.PlaylistItems.First().DurationSeconds;

			double targetTime = durationSeconds * 0.30;
			if (targetTime < 3) targetTime = 3;

			_viewTimer = Application.Current.Dispatcher.CreateTimer();
			if (_viewTimer != null)
			{
				_viewTimer.Interval = TimeSpan.FromSeconds(targetTime);
				_viewTimer.Tick += (s, e) =>
				{
					if (_activeVideo != null && !_viewCounted)
					{
						_viewCounted = true;
						_activeVideo.Views++;
					}
					StopTimer();
				};
				_viewTimer.Start();
			}
		}

		private void StopTimer()
		{
			if (_viewTimer != null)
			{
				_viewTimer.Stop();
				_viewTimer = null;
			}
		}

		[RelayCommand]
		private async Task ShareVideo(ShortVideo video)
		{
			if (video?.RealVideoUrl == null) return;
			await Share.Default.RequestAsync(new ShareTextRequest
			{
				Title = "Veja no BellaLink",
				Uri = video.RealVideoUrl
			});
		}

		[RelayCommand]
		private void ToggleLike(ShortVideo video)
		{
			if (video == null) return;
			video.IsLikedByMe = !video.IsLikedByMe;
			if (video.IsLikedByMe) video.Likes++; else video.Likes--;
		}

		// --- CORREÇÃO: SEGUIR PARCEIRO E ATUALIZAR TODOS OS VÍDEOS DELE ---
		[RelayCommand]
		private async Task ToggleFollow(ShortVideo video)
		{
			if (video == null || string.IsNullOrEmpty(video.PartnerId)) return;

			// 1. Inverte o status atual (Só visual por enquanto)
			bool newState = !video.IsFollowing;

			// 2. Aplica essa mudança para TODOS os vídeos desse mesmo parceiro na lista
			// Isso resolve o problema de "seguir um e o outro não atualizar"
			foreach (var v in Videos.Where(x => x.PartnerId == video.PartnerId))
			{
				v.IsFollowing = newState;
			}

			// 3. Salva no Banco de Dados
			try
			{
				var myId = await _authService.GetUserIdAsync();
				if (string.IsNullOrEmpty(myId)) return;

				if (newState)
				{
					await _databaseService.FollowPartnerAsync(myId, video.PartnerId);
				}
				else
				{
					await _databaseService.UnfollowPartnerAsync(myId, video.PartnerId);
				}
			}
			catch
			{
				// Se der erro, reverte visualmente (opcional, mas boa prática)
				foreach (var v in Videos.Where(x => x.PartnerId == video.PartnerId))
				{
					v.IsFollowing = !newState;
				}
			}
		}

		[RelayCommand]
		private async Task OpenComments(ShortVideo video)
		{
			if (video == null) return;
			await Shell.Current.GoToAsync($"{nameof(ShortsCommentsPage)}?VideoId={video.Id}");
		}

		[RelayCommand]
		private async Task ReportVideo(ShortVideo video)
		{
			if (video == null) return;

			string reason = await Shell.Current.DisplayActionSheetAsync(
				"Denunciar este vídeo?",
				"Cancelar",
				null,
				"Conteúdo Impróprio",
				"Spam ou Golpe",
				"Violência",
				"Direitos Autorais");

			if (string.IsNullOrEmpty(reason) || reason == "Cancelar") return;

			var report = new Report
			{
				VideoId = video.Id,
				VideoUrl = video.RealVideoUrl,
				Reason = reason,
				Status = "Pendente"
			};

			_ = Task.Run(async () =>
			{
				try
				{
					report.ReporterId = await _authService.GetUserIdAsync();
					await _databaseService.ReportVideoAsync(report);
				}
				catch { }
			});

			if (Application.Current?.MainPage != null)
				await Application.Current.MainPage.DisplayAlert("Recebido", "Denúncia enviada. Obrigado.", "OK");
		}
	}
}