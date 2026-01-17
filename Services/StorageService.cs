using Firebase.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public class StorageService
	{
		private readonly FirebaseStorage _storage;

		public StorageService()
		{
			// Usa a constante definida no arquivo Constants.cs
			_storage = new FirebaseStorage(Constants.FirebaseStorageBucket);
		}

		public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
		{
			try
			{
				var imageUrl = await _storage
					.Child("images")
					.Child(fileName)
					.PutAsync(fileStream);

				return imageUrl;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Upload: {ex.Message}");
				return string.Empty;
			}
		}

		public async Task<string> UploadVideoAsync(Stream fileStream, string fileName)
		{
			try
			{
				var videoUrl = await _storage
					.Child("videos")
					.Child(fileName)
					.PutAsync(fileStream);

				return videoUrl;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Upload Vídeo: {ex.Message}");
				return string.Empty;
			}
		}
	}
}