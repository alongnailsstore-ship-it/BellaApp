using BellaLink.App.Models;
using BellaLink.App.Services;
using BellaLink.App.Views.ConsumerViews;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	public partial class MyAppointmentsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		[ObservableProperty]
		private ObservableCollection<Appointment> myAppointmentsList = new ObservableCollection<Appointment>();

		[ObservableProperty]
		private bool isBusy;

		public MyAppointmentsViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		[RelayCommand]
		public async Task LoadAppointments()
		{
			// Se já estiver carregando, não faz nada (Evita duplo spinner)
			if (IsBusy) return;

			try
			{
				IsBusy = true; // Liga o spinner

				var userId = await _authService.GetUserIdAsync();
				if (string.IsNullOrEmpty(userId))
				{
					IsBusy = false;
					return;
				}

				// Busca dados (Sem delay artificial para ser mais rápido)
				var list = await _databaseService.GetAppointmentsForClientAsync(userId);

				MainThread.BeginInvokeOnMainThread(() =>
				{
					MyAppointmentsList.Clear();
					if (list != null)
					{
						var ordenados = list.OrderByDescending(a => a.Date).ToList();
						foreach (var appt in ordenados)
						{
							MyAppointmentsList.Add(appt);
						}
					}
				});
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao carregar: {ex.Message}", "OK");
			}
			finally
			{
				// DESLIGAMENTO FORÇADO DO SPINNER
				MainThread.BeginInvokeOnMainThread(() =>
				{
					IsBusy = false;
				});
			}
		}

		[RelayCommand]
		private async Task CancelAppointment(Appointment appt)
		{
			if (appt == null) return;
			bool confirm = await Shell.Current.DisplayAlertAsync("Cancelar", "Deseja cancelar este agendamento?", "Sim", "Não");
			if (!confirm) return;

			try
			{
				IsBusy = true; // Mostra carregamento rápido
				await _databaseService.UpdateAppointmentStatusAsync(appt, "Cancelado");
				await LoadAppointments(); // Recarrega a lista
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
				IsBusy = false;
			}
		}

		[RelayCommand]
		private async Task RescheduleAppointment(Appointment appt)
		{
			if (appt == null) return;
			// Vai para a tela de escolha de horário com os dados do parceiro
			await Shell.Current.GoToAsync($"{nameof(BookingDateTimePage)}?PartnerId={appt.PartnerId}&PartnerName={appt.PartnerName}");
		}

		[RelayCommand]
		private async Task DeleteAppointment(Appointment appt)
		{
			if (appt == null) return;

			// REGRA: Só exclui se estiver Cancelado
			if (appt.Status != "Cancelado")
			{
				await Shell.Current.DisplayAlertAsync("Ação Negada", "Você só pode excluir agendamentos cancelados.", "OK");
				return;
			}

			bool confirm = await Shell.Current.DisplayAlertAsync("Excluir", "Apagar do histórico?", "Sim", "Não");
			if (!confirm) return;

			try
			{
				if (!string.IsNullOrEmpty(appt.Id))
				{
					await _databaseService.DeleteAppointmentAsync(appt.Id);

					// Remove da lista visualmente (Instantâneo)
					MainThread.BeginInvokeOnMainThread(() => MyAppointmentsList.Remove(appt));
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
			}
		}
	}
}