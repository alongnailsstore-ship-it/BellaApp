using BellaLink.App.ViewModels;
using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.Views
{
	public partial class UserProfilePage : ContentPage
	{
		public UserProfilePage(UserProfileViewModel viewModel)
		{
			try
			{
				InitializeComponent();
				BindingContext = viewModel;
			}
			catch (Exception ex)
			{
				// Proteção contra erros de desenho da tela (XAML)
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await DisplayAlert("Erro XAML", $"Falha ao abrir perfil: {ex.Message}", "OK");
				});
			}
		}

		// O método OnAppearing foi removido pois o ViewModel 
		// já carrega os dados automaticamente no construtor.
		// Isso resolve o erro CS1061 (LoadUserData não encontrado).
	}
}