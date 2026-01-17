using BellaLink.App.ViewModels.PartnerViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerAppointmentsPage : ContentPage
	{
		private readonly PartnerAppointmentsViewModel _viewModel;
		// Removido o campo _isUpdating não usado

		public PartnerAppointmentsPage(PartnerAppointmentsViewModel viewModel)
		{
			try
			{
				InitializeComponent();
				BindingContext = viewModel;
				_viewModel = viewModel;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"💥 CRASH NA AGENDA (CONSTRUTOR): {ex.Message}");

				MainThread.BeginInvokeOnMainThread(async () =>
				{
					if (App.Current?.Windows != null && App.Current.Windows.Count > 0)
					{
						var page = App.Current.Windows[0].Page;
						if (page != null)
							// CORRIGIDO: DisplayAlertAsync
							await page.DisplayAlertAsync("ERRO FATAL",
								$"Não foi possível desenhar a Agenda.\nErro: {ex.Message}", "OK");
					}
				});
				throw;
			}
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_viewModel != null)
			{
				try
				{
					await _viewModel.LoadData();
				}
				catch (Exception ex)
				{
					// CORRIGIDO: DisplayAlertAsync
					await DisplayAlertAsync("Erro de Dados", $"Falha ao buscar agendamentos: {ex.Message}", "OK");
				}
			}
		}

		private void OnPhoneChanged(object sender, TextChangedEventArgs e)
		{
			// Vazio intencionalmente
		}
	}
}