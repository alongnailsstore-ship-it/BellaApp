using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Net.Http; // Necessário
using System.Net.Http.Json; // Necessário
using System.Text.RegularExpressions; // Necessário

namespace BellaLink.App.ViewModels
{
	public partial class UserProfileViewModel : ObservableObject
	{
		private readonly IAuthService _authService;
		private readonly DatabaseService _databaseService;
		private readonly StorageService _storageService;
		private readonly HttpClient _httpClient; // Para o ViaCEP

		[ObservableProperty]
		private User currentUser;

		[ObservableProperty]
		private bool isLoading;

		[ObservableProperty]
		private bool isPartner;

		// --- CAMPOS DE ENDEREÇO ---

		[ObservableProperty] string zipCode = ""; // Campo do CEP

		// Gatilho Automático ao digitar o CEP
		async partial void OnZipCodeChanged(string value)
		{
			var cleanCep = Regex.Replace(value ?? "", "[^0-9]", "");
			if (cleanCep.Length == 8)
			{
				await BuscarEnderecoPorCep(cleanCep);
			}
		}

		[ObservableProperty] string street = "";
		[ObservableProperty] string number = "";
		[ObservableProperty] string neighborhood = ""; // Bairro (Novo aqui)
		[ObservableProperty] string city = "";
		[ObservableProperty] string state = "";

		public UserProfileViewModel(IAuthService auth, DatabaseService db, StorageService storage)
		{
			_authService = auth;
			_databaseService = db;
			_storageService = storage;
			_httpClient = new HttpClient(); // Inicializa o cliente

			CurrentUser = new User();
			LoadProfile(); // Carrega os dados ao abrir (sem async/await no construtor para evitar warning)
		}

		private async void LoadProfile()
		{
			IsLoading = true;
			try
			{
				var userId = await _authService.GetUserIdAsync();
				if (string.IsNullOrEmpty(userId)) return;

				var user = await _databaseService.GetUserAsync(userId);

				if (user != null)
				{
					CurrentUser = user;
					IsPartner = user.IsPartner;

					// Carrega o endereço existente nos campos da tela
					if (user.Addresses != null && user.Addresses.Count > 0)
					{
						var addr = user.Addresses[0];
						ZipCode = addr.ZipCode ?? ""; // Carrega o CEP
						Street = addr.Street ?? "";
						Number = addr.Number ?? "";
						Neighborhood = addr.Neighborhood ?? "";
						City = addr.City ?? "";
						State = addr.State ?? "";
					}
				}
				else
				{
					CurrentUser = new User { Id = userId };
				}
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task BuscarEnderecoPorCep(string cep)
		{
			IsLoading = true;
			try
			{
				// Reutiliza a classe ViaCepResult que criamos para o cadastro
				var response = await _httpClient.GetFromJsonAsync<ViaCepResult>($"https://viacep.com.br/ws/{cep}/json/");

				if (response != null && !response.Erro)
				{
					Street = response.Logradouro ?? "";
					Neighborhood = response.Bairro ?? "";
					City = response.Localidade ?? "";
					State = response.Uf ?? "";
				}
			}
			catch
			{
				// Falha silenciosa (deixa o usuário digitar)
			}
			finally
			{
				IsLoading = false;
			}
		}

		// --- UPLOAD DE FOTOS (Mantido igual) ---

		[RelayCommand]
		private async Task UploadConsumerPhoto()
		{
			var url = await PickAndUploadPhoto("perfil");
			if (!string.IsNullOrEmpty(url)) CurrentUser.ConsumerPhoto = url;
		}

		[RelayCommand]
		private async Task UploadPartnerPhoto()
		{
			var url = await PickAndUploadPhoto("salao");
			if (!string.IsNullOrEmpty(url)) CurrentUser.PartnerPhoto = url;
		}

		[RelayCommand]
		private async Task UploadSupplierPhoto()
		{
			var url = await PickAndUploadPhoto("fornecedor");
			if (!string.IsNullOrEmpty(url)) CurrentUser.SupplierPhoto = url;
		}

		private async Task<string?> PickAndUploadPhoto(string type)
		{
			try
			{
				var result = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
				{
					SelectionLimit = 1,
					Title = "Selecione uma foto"
				});

				var photo = result?.FirstOrDefault();
				if (photo == null) return null;

				IsLoading = true;
				using var stream = await photo.OpenReadAsync();
				var url = await _storageService.UploadFileAsync(stream, $"{CurrentUser.Id}_{type}.jpg");
				return url;
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", "Falha no upload: " + ex.Message, "OK");
				return null;
			}
			finally { IsLoading = false; }
		}

		// --- SALVAR (Atualizado com CEP) ---

		[RelayCommand]
		private async Task SaveChanges()
		{
			if (CurrentUser == null) return;
			IsLoading = true;

			try
			{
				CurrentUser.IsPartner = IsPartner;

				// Atualiza o objeto de endereço
				var address = new Address
				{
					Label = "Principal",
					ZipCode = ZipCode ?? "",
					Street = Street ?? "",
					Number = Number ?? "",
					Neighborhood = Neighborhood ?? "",
					City = City ?? "",
					State = State ?? ""
				};

				// Substitui a lista de endereços (regra simplificada: 1 endereço por usuário)
				CurrentUser.Addresses = new List<Address> { address };

				await _databaseService.SaveUserAsync(CurrentUser);
				await Shell.Current.DisplayAlertAsync("Sucesso", "Perfil atualizado!", "OK");
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao salvar: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");

		[RelayCommand]
		private async Task Logout()
		{
			_authService.Logout();
			await Shell.Current.GoToAsync($"//{nameof(Views.LoginPage)}");
		}
	}
}