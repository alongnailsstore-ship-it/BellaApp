#pragma warning disable CS0618
using BellaLink.App.Services;
using BellaLink.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels
{
	public partial class LoginViewModel : ObservableObject
	{
		private readonly IAuthService _authService;

		public LoginViewModel(IAuthService authService)
		{
			_authService = authService;
			LoadSavedCredentials();
		}

		[ObservableProperty] private string email = string.Empty;
		[ObservableProperty] private string password = string.Empty;
		[ObservableProperty] private string errorMessage = string.Empty;

		[ObservableProperty] private bool hasError;

		// Inicia oculto (Senha protegida) -> Cadeado Fechado
		[ObservableProperty] private bool isPasswordHidden = true;
		[ObservableProperty] private string passwordIcon = "🔒";

		[ObservableProperty] private bool rememberMe = true;
		[ObservableProperty] private bool isBusy;

		private async void LoadSavedCredentials()
		{
			try
			{
				var savedEmail = await SecureStorage.GetAsync("saved_email");
				var savedPassword = await SecureStorage.GetAsync("saved_password");

				if (!string.IsNullOrEmpty(savedEmail))
				{
					Email = savedEmail;
					Password = savedPassword ?? string.Empty;
					RememberMe = true;
				}
			}
			catch { }
		}

		[RelayCommand]
		private void TogglePassword()
		{
			IsPasswordHidden = !IsPasswordHidden;

			// Se está Oculto -> Mostra Cadeado Fechado (Seguro)
			// Se está Visível -> Mostra Cadeado Aberto (Exposto)
			PasswordIcon = IsPasswordHidden ? "🔒" : "🔓";
		}

		[RelayCommand]
		private void ToggleRememberMe()
		{
			RememberMe = !RememberMe;
		}

		[RelayCommand]
		private async Task Login()
		{
			if (IsBusy) return;

			HasError = false;
			ErrorMessage = string.Empty;

			if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
			{
				ErrorMessage = "Preencha E-mail e Senha.";
				HasError = true;
				return;
			}

			IsBusy = true;

			try
			{
				bool success = await _authService.LoginAsync(Email, Password);

				if (success)
				{
					if (RememberMe)
					{
						await SecureStorage.SetAsync("saved_email", Email);
						await SecureStorage.SetAsync("saved_password", Password);
					}
					else
					{
						SecureStorage.Remove("saved_email");
						SecureStorage.Remove("saved_password");
					}

					MainThread.BeginInvokeOnMainThread(async () =>
					{
						await Shell.Current.GoToAsync(nameof(SelectProfilePage));
					});
				}
				else
				{
					ErrorMessage = "E-mail ou senha inválidos.";
					HasError = true;
				}
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task NavigateToRegister()
		{
			await Shell.Current.GoToAsync(nameof(RegisterPage));
		}

		[RelayCommand]
		private async Task ForgotPassword()
		{
			await Shell.Current.DisplayAlert("Recuperar", "Em breve: Recuperação de Senha", "OK");
		}

		[RelayCommand]
		private async Task GoogleLogin()
		{
			// Lógica futura de Google
			await Shell.Current.DisplayAlert("Google", "Complete seu cadastro para continuar.", "OK");
			await Shell.Current.GoToAsync(nameof(RegisterPage));
		}
	}
}