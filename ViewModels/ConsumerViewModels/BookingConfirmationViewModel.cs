#pragma warning disable CS0618
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BellaLink.App.Models;
using BellaLink.App.Services;
using BellaLink.App.Views.ConsumerViews; // Necessário para achar ConsumerHomePage
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	[QueryProperty(nameof(Partner), "Partner")]
	[QueryProperty(nameof(TotalAmount), "TotalAmount")]
	[QueryProperty(nameof(Date), "Date")]
	[QueryProperty(nameof(TimeStr), "Time")]
	public partial class BookingConfirmationViewModel : ObservableObject
	{
		// --- VARIÁVEIS E SERVIÇOS (Que estavam faltando) ---
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		[ObservableProperty] private Partner partner = new Partner();
		[ObservableProperty] private decimal totalAmount;
		[ObservableProperty] private DateTime date;
		[ObservableProperty] private string timeStr = string.Empty;

		[ObservableProperty]
		private bool isLoading; // A variável que estava dando erro CS0103

		[ObservableProperty]
		private string paymentMethodDisplay = "Dinheiro (Pagar no local)";

		// --- CONSTRUTOR ---
		public BookingConfirmationViewModel(DatabaseService databaseService, IAuthService authService)
		{
			_databaseService = databaseService;
			_authService = authService;
		}

		// --- COMANDO DE CONFIRMAR ---
		[RelayCommand]
		private async Task ConfirmBooking()
		{
			if (IsLoading) return;
			IsLoading = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();
				var user = await _databaseService.GetUserAsync(userId);

				var appointment = new Appointment
				{
					ClientId = userId,
					ClientName = user?.Name ?? "Cliente",
					ClientEmail = user?.Email,
					PartnerId = Partner.Id,
					PartnerName = Partner.Name,
					ServiceName = "Serviços Agendados",
					Date = Date.Date + TimeSpan.Parse(TimeStr),
					Status = "Confirmado",
					Price = TotalAmount,
					PaymentMethod = "Dinheiro"
				};

				// Salva no banco
				await _databaseService.CreateAppointmentAsync(appointment);

				await Shell.Current.DisplayAlertAsync("Sucesso!", "Agendamento confirmado. Te esperamos lá! 💅", "OK");

				// --- CORREÇÃO DE NAVEGAÇÃO ---
				// Reseta a pilha e volta para a Home do Consumidor
				await Shell.Current.GoToAsync($"//{nameof(ConsumerHomePage)}");
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

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}