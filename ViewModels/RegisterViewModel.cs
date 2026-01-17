#pragma warning disable CS0618
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BellaLink.App.Services;
using BellaLink.App.Models;
using BellaLink.App.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Json;

namespace BellaLink.App.ViewModels
{
	public partial class RegisterViewModel : ObservableObject
	{
		private readonly IAuthService _authService;
		private readonly DatabaseService _databaseService;
		private readonly HttpClient _httpClient; // Para buscar o CEP

		[ObservableProperty] private string name = string.Empty;
		[ObservableProperty] private string cpf = string.Empty;
		[ObservableProperty] private string email = string.Empty;
		[ObservableProperty] private string password = string.Empty;
		[ObservableProperty] private string confirmPassword = string.Empty;
		[ObservableProperty] private string whatsApp = string.Empty;

		public ObservableCollection<Address> AddedAddresses { get; set; } = new ObservableCollection<Address>();

		[ObservableProperty] private string addressLabel = string.Empty;

		// Quando o CEP mudar, vamos tentar buscar o endereço
		[ObservableProperty] private string zipCode = string.Empty;

		async partial void OnZipCodeChanged(string value)
		{
			// Remove traços e pontos
			var cleanCep = Regex.Replace(value ?? "", "[^0-9]", "");

			// Se tiver 8 dígitos, busca no ViaCEP
			if (cleanCep.Length == 8)
			{
				await BuscarEnderecoPorCep(cleanCep);
			}
		}

		[ObservableProperty] private string street = string.Empty;
		[ObservableProperty] private string number = string.Empty;
		[ObservableProperty] private string neighborhood = string.Empty;
		[ObservableProperty] private string city = string.Empty;
		[ObservableProperty] private string state = string.Empty;

		[ObservableProperty] private bool isConsumer = true;
		[ObservableProperty] private bool isPartner;
		[ObservableProperty] private bool isSupplier;

		[ObservableProperty] private string storeName = string.Empty;
		[ObservableProperty] private bool isCorporate;
		[ObservableProperty] private string cnpj = string.Empty;

		[ObservableProperty] private bool isLoading;

		public RegisterViewModel(IAuthService authService, DatabaseService databaseService)
		{
			_authService = authService;
			_databaseService = databaseService;
			_httpClient = new HttpClient(); // Cliente HTTP simples
		}

		private async Task BuscarEnderecoPorCep(string cep)
		{
			IsLoading = true;
			try
			{
				var response = await _httpClient.GetFromJsonAsync<ViaCepResult>($"https://viacep.com.br/ws/{cep}/json/");

				if (response != null && !response.Erro)
				{
					// Preenche os campos automaticamente
					Street = response.Logradouro ?? "";
					Neighborhood = response.Bairro ?? "";
					City = response.Localidade ?? "";
					State = response.Uf ?? "";

					// Foca no campo número (opcional, mas visualmente não temos como focar via VM fácil, 
					// o usuário vai perceber os campos preenchidos)
				}
			}
			catch
			{
				// Se der erro (sem internet, etc), apenas ignoramos e deixamos o usuário digitar
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void AddAddress()
		{
			if (string.IsNullOrWhiteSpace(Street) || string.IsNullOrWhiteSpace(City))
			{
				Shell.Current.DisplayAlert("Atenção", "Preencha Rua e Cidade.", "OK");
				return;
			}

			var newAddr = new Address
			{
				Label = string.IsNullOrWhiteSpace(AddressLabel) ? "Principal" : AddressLabel,
				ZipCode = ZipCode,
				Street = Street,
				Number = Number ?? "S/N",
				Neighborhood = Neighborhood,
				City = City,
				State = State
			};

			AddedAddresses.Add(newAddr);

			// Limpa os campos para adicionar outro se quiser
			AddressLabel = ""; ZipCode = ""; Street = ""; Number = "";
			Neighborhood = ""; City = ""; State = "";
		}

		[RelayCommand]
		private void RemoveAddress(Address address)
		{
			if (AddedAddresses.Contains(address)) AddedAddresses.Remove(address);
		}

		[RelayCommand]
		private async Task Register()
		{
			if (IsLoading) return;

			// Validações Básicas
			if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
				string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(WhatsApp))
			{
				await Shell.Current.DisplayAlertAsync("Atenção", "Preencha todos os dados pessoais.", "OK");
				return;
			}

			if (!IsValidCpf(Cpf))
			{
				await Shell.Current.DisplayAlertAsync("Erro", "CPF inválido. Use um CPF real.", "OK");
				return;
			}

			if (Password != ConfirmPassword)
			{
				await Shell.Current.DisplayAlertAsync("Erro", "As senhas não coincidem.", "OK");
				return;
			}

			if (AddedAddresses.Count == 0)
			{
				// Se o usuário preencheu os campos mas esqueceu de clicar em "+ Adicionar Endereço", adicionamos agora
				if (!string.IsNullOrWhiteSpace(Street))
				{
					AddAddress();
				}
				else
				{
					await Shell.Current.DisplayAlertAsync("Atenção", "Adicione um endereço.", "OK");
					return;
				}
			}

			if ((IsPartner || IsSupplier) && string.IsNullOrWhiteSpace(StoreName))
			{
				await Shell.Current.DisplayAlertAsync("Atenção", "Informe o nome da loja.", "OK");
				return;
			}

			IsLoading = true;

			try
			{
				bool authSuccess = await _authService.RegisterAsync(Email, Password, Name);

				if (authSuccess)
				{
					var userId = await _authService.GetUserIdAsync();

					if (string.IsNullOrEmpty(userId))
					{
						await Shell.Current.DisplayAlertAsync("Erro", "Erro de ID.", "OK");
						return;
					}

					var newUser = new User
					{
						Id = userId,
						Name = Name,
						Cpf = Regex.Replace(Cpf, "[^0-9]", ""),
						Email = Email,
						WhatsApp = WhatsApp,
						Addresses = AddedAddresses.ToList(),
						IsConsumer = IsConsumer,
						IsPartner = IsPartner,
						IsSupplier = IsSupplier,
						StoreName = (IsPartner || IsSupplier) ? StoreName : null,
						IsCorporate = IsCorporate,
						Cnpj = (IsPartner && IsCorporate) ? Cnpj : null,
						AcceptsCash = true
					};

					await _databaseService.SaveUserAsync(newUser);
					await Shell.Current.DisplayAlertAsync("Sucesso", "Cadastro realizado!", "OK");
					await Shell.Current.GoToAsync(nameof(SelectProfilePage));
				}
				else
				{
					await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível criar a conta (Email já existe?).", "OK");
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", $"Falha: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		private bool IsValidCpf(string cpf)
		{
			if (string.IsNullOrWhiteSpace(cpf)) return false;
			string cleanCpf = Regex.Replace(cpf, "[^0-9]", "");
			if (cleanCpf.Length != 11) return false;
			if (cleanCpf == "99999999999") return true; // Backdoor de teste

			bool allSame = true;
			for (int i = 1; i < 11; i++) { if (cleanCpf[i] != cleanCpf[0]) { allSame = false; break; } }
			if (allSame) return false;

			int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			string tempCpf = cleanCpf.Substring(0, 9);
			int soma = 0;

			for (int i = 0; i < 9; i++) soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
			int resto = soma % 11;
			if (resto < 2) resto = 0; else resto = 11 - resto;
			string digito = resto.ToString();
			tempCpf = tempCpf + digito;

			int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			soma = 0;
			for (int i = 0; i < 10; i++) soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
			resto = soma % 11;
			if (resto < 2) resto = 0; else resto = 11 - resto;
			digito = digito + resto.ToString();

			return cleanCpf.EndsWith(digito);
		}

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}