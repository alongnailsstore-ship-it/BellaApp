#pragma warning disable CS0618
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BellaLink.App.Views.ConsumerViews;
using BellaLink.App.Views.PartnerViews;
using BellaLink.App.Views.DistributorViews;
using BellaLink.App.Services;
using BellaLink.App.Views;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using System;
using BellaLink.App.Models;

namespace BellaLink.App.ViewModels
{
	public partial class SelectProfileViewModel : ObservableObject
	{
		private readonly IAuthService _authService;
		private readonly DatabaseService _databaseService;

		// --- Propriedades de Controle de Acesso ---
		private bool _canAccessConsumer;
		public bool CanAccessConsumer
		{
			get => _canAccessConsumer;
			set => SetProperty(ref _canAccessConsumer, value);
		}

		private bool _canAccessPartner;
		public bool CanAccessPartner
		{
			get => _canAccessPartner;
			set => SetProperty(ref _canAccessPartner, value);
		}

		private bool _canAccessSupplier;
		public bool CanAccessSupplier
		{
			get => _canAccessSupplier;
			set => SetProperty(ref _canAccessSupplier, value);
		}

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		private string _userName = "Sistema";
		public string UserName
		{
			get => _userName;
			set => SetProperty(ref _userName, value);
		}

		private string _statusMessage = "Aguardando...";
		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		public SelectProfileViewModel(IAuthService authService, DatabaseService databaseService)
		{
			_authService = authService;
			_databaseService = databaseService;
		}

		public async Task CheckUserRoles()
		{
			IsLoading = true;
			try
			{
				StatusMessage = "1. Verificando Login...";
				var userId = await _authService.GetUserIdAsync();

				if (string.IsNullOrEmpty(userId))
				{
					StatusMessage = "Erro: Usuário não logado.";
					CanAccessConsumer = true;
					return;
				}

				StatusMessage = "2. Conectando ao Banco...";
				var user = await _databaseService.GetUserAsync(userId);

				StatusMessage = "3. Processando Permissões...";
				if (user != null)
				{
					MainThread.BeginInvokeOnMainThread(() =>
					{
						UserName = user.Name ?? "Visitante";
						CanAccessConsumer = user.IsConsumer;
						CanAccessPartner = user.IsPartner;
						CanAccessSupplier = user.IsSupplier;

						StatusMessage = "Quem é você hoje?";

						if (!CanAccessConsumer && !CanAccessPartner && !CanAccessSupplier)
						{
							StatusMessage = "Aviso: Nenhum perfil ativo.";
							CanAccessConsumer = true;
						}
					});
				}
				else
				{
					StatusMessage = "Erro: Cadastro não encontrado.";
					CanAccessConsumer = true;
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Falha Crítica: {ex.Message}";
				CanAccessConsumer = true;
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task SelectConsumer()
		{
			try
			{
				if (Application.Current != null && Application.Current.Windows.Count > 0)
				{
					// ROTA 1: Carrega o Shell do Consumidor
					Application.Current.Windows[0].Page = new ConsumerShell();
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
			}
		}

		[RelayCommand]
		private async Task SelectPartner()
		{
			if (!CanAccessPartner)
			{
				await Shell.Current.DisplayAlert("Acesso", "Perfil Profissional desativado.", "OK");
				return;
			}

			try
			{
				if (Application.Current != null && Application.Current.Windows.Count > 0)
				{
					// ROTA 2: Carrega o Shell do Parceiro (Agenda, Serviços)
					Application.Current.Windows[0].Page = new PartnerShell();
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
			}
		}

		[RelayCommand]
		private async Task SelectSupplier()
		{
			IsLoading = true;
			try
			{
				// Auto-promoção para testes (se não for fornecedor, vira um agora)
				var userId = await _authService.GetUserIdAsync();
				if (!string.IsNullOrEmpty(userId))
				{
					var user = await _databaseService.GetUserAsync(userId);
					if (user != null && !user.IsSupplier)
					{
						user.IsSupplier = true;
						if (string.IsNullOrEmpty(user.StoreName))
							user.StoreName = (user.Name ?? "Usuario") + " Distribuidora";

						await _databaseService.SaveUserAsync(user);
						CanAccessSupplier = true;
					}
				}

				// ROTA 3: Navega para o Dashboard do Distribuidor (Produtos)
				await Shell.Current.GoToAsync(nameof(DistributorDashboardPage));
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", $"Falha ao entrar como distribuidor: {ex.Message}", "OK");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private void Logout()
		{
			_authService.Logout();

			if (Application.Current != null && Application.Current.Windows.Count > 0)
			{
				// Reseta para o AppShell padrão (Login)
				Application.Current.Windows[0].Page = new AppShell();
			}
		}
	}
}