#pragma warning disable CS0618
using BellaLink.App.Models;
using BellaLink.App.Services; // Necessário
using BellaLink.App.Views.ConsumerViews;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel; // Necessário
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	[QueryProperty(nameof(Partner), "Partner")]
	[QueryProperty(nameof(TotalAmount), "TotalAmount")]
	public partial class BookingDateTimeViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;

		[ObservableProperty]
		private Partner partner = new Partner();

		[ObservableProperty]
		private decimal totalAmount;

		// Ao mudar a data, recarrega os horários
		[ObservableProperty]
		private DateTime selectedDate;

		partial void OnSelectedDateChanged(DateTime value)
		{
			// Recarrega slots quando o usuário troca a data no calendário
			LoadTimeSlots();
		}

		[ObservableProperty]
		private TimeSlot? selectedTime;

		[ObservableProperty]
		private bool isLoading;

		public ObservableCollection<TimeSlot> AvailableTimes { get; set; }

		public BookingDateTimeViewModel(DatabaseService databaseService)
		{
			_databaseService = databaseService;
			SelectedDate = DateTime.Today; // Começa hoje
			AvailableTimes = new ObservableCollection<TimeSlot>();

			// Carrega inicial
			LoadTimeSlots();
		}

		// LÓGICA INTELIGENTE DE HORÁRIOS
		public async void LoadTimeSlots()
		{
			IsLoading = true;
			try
			{
				// 1. Busca no banco o que já está ocupado para esse dia/parceiro
				var existingAppointments = new List<Appointment>();
				if (!string.IsNullOrEmpty(Partner.Id))
				{
					existingAppointments = await _databaseService.GetPartnerAppointmentsByDateAsync(Partner.Id, SelectedDate);
				}

				MainThread.BeginInvokeOnMainThread(() =>
				{
					AvailableTimes.Clear();

					// Define o horário de funcionamento (Ex: 08:00 as 19:00)
					TimeSpan start = TimeSpan.FromHours(8);
					TimeSpan end = TimeSpan.FromHours(19);
					TimeSpan interval = TimeSpan.FromMinutes(60); // Intervalo de 1h

					var current = start;
					var now = DateTime.Now;

					while (current < end)
					{
						bool isAvailable = true;

						// REGRA 1: PASSADO
						// Se a data selecionada for HOJE, verifica se a hora já passou
						if (SelectedDate.Date == DateTime.Today)
						{
							if (current <= now.TimeOfDay)
							{
								isAvailable = false;
							}
						}

						// REGRA 2: DUPLICIDADE (Banco de Dados)
						// Verifica se já existe agendamento neste horário exato
						if (isAvailable)
						{
							bool slotTaken = existingAppointments.Any(app => app.Date.TimeOfDay == current);
							if (slotTaken) isAvailable = false;
						}

						// Só adiciona na lista visual se estiver livre
						// (Ou adiciona bloqueado/cinza se preferir mostrar que está ocupado)
						// Aqui vamos REMOVER da lista para limpar a tela
						if (isAvailable)
						{
							AvailableTimes.Add(new TimeSlot
							{
								Time = current.ToString(@"hh\:mm"),
								IsAvailable = true
							});
						}

						current = current.Add(interval);
					}

					// Se não sobrou horário nenhum
					if (AvailableTimes.Count == 0)
					{
						// Opcional: Adicionar um item de aviso visual
						// Mas deixar vazio já indica indisponibilidade
					}
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Slots: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		[RelayCommand]
		private async Task ConfirmBooking()
		{
			if (SelectedTime == null)
			{
				await Shell.Current.DisplayAlert("Atenção", "Selecione um horário disponível.", "OK");
				return;
			}

			var navigationParameter = new Dictionary<string, object>
			{
				{ "Partner", Partner },
				{ "TotalAmount", TotalAmount },
				{ "Date", SelectedDate },
				{ "Time", SelectedTime.Time ?? "00:00" }
			};

			await Shell.Current.GoToAsync(nameof(BookingConfirmationPage), navigationParameter);
		}

		[RelayCommand]
		private async Task GoBack()
		{
			await Shell.Current.GoToAsync("..");
		}
	}
}