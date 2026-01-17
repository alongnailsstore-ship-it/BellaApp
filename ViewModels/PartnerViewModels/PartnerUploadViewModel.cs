using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public partial class PartnerUploadViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;
		private readonly StorageService _storageService;

		[ObservableProperty] private string title = string.Empty;
		[ObservableProperty] private string description = string.Empty;
		[ObservableProperty] private string videoUrlInput = string.Empty;

		public ObservableCollection<ShortVideo> MyPublishedVideos { get; set; } = new ObservableCollection<ShortVideo>();

		[ObservableProperty] private bool isBusy;
		[ObservableProperty] private string uploadStatus = string.Empty;

		public PartnerUploadViewModel(DatabaseService databaseService, IAuthService authService, StorageService storageService)
		{
			_databaseService = databaseService;
			_authService = authService;
			_storageService = storageService;

			// Carrega os vídeos assim que a tela abre
			LoadMyVideosCommand.Execute(null);
		}

		// --- COMANDO DE NAVEGAÇÃO SEGURO ---
		[RelayCommand]
		private async Task GoBack()
		{
			try
			{
				await Shell.Current.GoToAsync("..");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro ao voltar: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task UploadVideoFromFile()
		{
			try
			{
				var result = await FilePicker.Default.PickAsync(new PickOptions
				{
					PickerTitle = "Selecione um vídeo",
					FileTypes = FilePickerFileType.Videos
				});

				if (result != null)
				{
					IsBusy = true;
					UploadStatus = "Enviando vídeo... aguarde (pode demorar)";

					using var stream = await result.OpenReadAsync();

					var downloadLink = await _storageService.UploadVideoAsync(stream, result.FileName);

					if (!string.IsNullOrEmpty(downloadLink))
					{
						VideoUrlInput = downloadLink;
						UploadStatus = "Upload concluído! Clique em PUBLICAR.";
					}
					else
					{
						UploadStatus = "Erro no upload.";
						await Shell.Current.DisplayAlert("Erro", "Falha ao obter link do vídeo.", "OK");
					}
				}
			}
			catch (Exception ex)
			{
				UploadStatus = "Erro ao selecionar arquivo.";
				await Shell.Current.DisplayAlert("Erro", $"Falha: {ex.Message}", "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task LoadMyVideos()
		{
			if (IsBusy) return;
			IsBusy = true;
			try
			{
				var userId = await _authService.GetUserIdAsync();
				var videos = await _databaseService.GetVideosByPartnerAsync(userId);

				MyPublishedVideos.Clear();
				foreach (var v in videos)
				{
					MyPublishedVideos.Insert(0, v); // Insere no topo para o mais recente aparecer primeiro
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro ao carregar vídeos: {ex.Message}");
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task PublishByUrl()
		{
			if (string.IsNullOrWhiteSpace(Title))
			{
				await Shell.Current.DisplayAlert("Atenção", "Digite um título.", "OK");
				return;
			}

			if (string.IsNullOrWhiteSpace(VideoUrlInput))
			{
				await Shell.Current.DisplayAlert("Atenção", "Cole a URL ou faça Upload.", "OK");
				return;
			}

			IsBusy = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();
				var user = await _databaseService.GetUserAsync(userId);

				var newShort = new ShortVideo
				{
					Id = Guid.NewGuid().ToString(),
					PartnerId = userId,
					PartnerName = user?.StoreName ?? user?.Name ?? "Profissional",
					PartnerPhoto = user?.PartnerPhoto,

					Title = this.Title,
					Description = $"{this.Title}\n{this.Description}",

					RealVideoUrl = this.VideoUrlInput,
					CurrentVideoUrl = this.VideoUrlInput,

					Likes = 0,
					Views = 0,
					IsPlaylist = false,
					CreatedAt = DateTime.Now
				};

				await _databaseService.SaveShortAsync(newShort);

				MyPublishedVideos.Insert(0, newShort);

				Title = string.Empty;
				Description = string.Empty;
				VideoUrlInput = string.Empty;
				UploadStatus = string.Empty;

				await Shell.Current.DisplayAlert("Sucesso", "Vídeo publicado!", "OK");

				// Opcional: Voltar automaticamente após publicar
				// await GoBack(); 
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", $"Falha: {ex.Message}", "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task DeleteVideo(ShortVideo video)
		{
			if (video == null) return;

			bool confirm = await Shell.Current.DisplayAlert("Excluir", "Apagar este vídeo?", "Sim", "Não");
			if (!confirm) return;

			try
			{
				MyPublishedVideos.Remove(video);

				if (!string.IsNullOrEmpty(video.Id))
				{
					await _databaseService.DeleteShortVideoAsync(video.Id);
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", $"Falha ao apagar: {ex.Message}", "OK");
			}
		}
	}
}