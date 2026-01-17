#pragma warning disable CS0618
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using BellaLink.App.Models;
using BellaLink.App.Services;
using System.Linq;
using System;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public partial class PartnerAppointmentsViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<Appointment> PendingAppointments { get; set; } = new ObservableCollection<Appointment>();
		public ObservableCollection<Appointment> ConfirmedAppointments { get; set; } = new ObservableCollection<Appointment>();
		public ObservableCollection<ServiceItem> MyServices { get; set; } = new ObservableCollection<ServiceItem>();

		[ObservableProperty] private bool isLoading;
		[ObservableProperty] private string workStart = "09:00";
		[ObservableProperty] private string workEnd = "18:00";

		// Dias
		[ObservableProperty] private bool workSeg; [ObservableProperty] private bool workTer;
		[ObservableProperty] private bool workQua; [ObservableProperty] private bool workQui;
		[ObservableProperty] private bool workSex; [ObservableProperty] private bool workSab;
		[ObservableProperty] private bool workDom; [ObservableProperty] private bool workHoliday;

		// Popup
		[ObservableProperty] private bool isBookingModalVisible;
		[ObservableProperty] private string manualPhoneInput = string.Empty;
		[ObservableProperty] private ServiceItem? selectedManualService;

		[ObservableProperty] private DateTime manualDate = DateTime.Now;
		[ObservableProperty] private TimeSpan manualTime = DateTime.Now.TimeOfDay;

		[ObservableProperty] private bool isEditing;
		[ObservableProperty] private string modalTitle = "Novo Agendamento";
		private Appointment? _currentEditingAppointment;

		public PartnerAppointmentsViewModel(DatabaseService databaseService, IAuthService authService)
		{
			_databaseService = databaseService;
			_authService = authService;
		}

		public async Task LoadData()
		{
			if (IsLoading) return;
			IsLoading = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();
				if (!string.IsNullOrEmpty(userId))
				{
					var user = await _databaseService.GetUserAsync(userId);
					if (user != null)
					{
						MainThread.BeginInvokeOnMainThread(() =>
						{
							WorkStart = user.WorkStart; WorkEnd = user.WorkEnd;
							WorkSeg = user.WorkSeg; WorkTer = user.WorkTer; WorkQua = user.WorkQua;
							WorkQui = user.WorkQui; WorkSex = user.WorkSex; WorkSab = user.WorkSab;
							WorkDom = user.WorkDom; WorkHoliday = user.WorkHoliday;
						});
					}

					var myServicesDb = await _databaseService.GetServicesForPartnerAsync(userId);
					var appointments = await _databaseService.GetAppointmentsForPartnerAsync(userId);

					foreach (var app in appointments)
					{
						if (string.IsNullOrEmpty(app.ClientName) && !string.IsNullOrEmpty(app.ClientId))
						{
							var clientUser = await _databaseService.GetUserAsync(app.ClientId);
							app.ClientName = clientUser?.Name ?? "Cliente";
						}
					}

					MainThread.BeginInvokeOnMainThread(() =>
					{
						MyServices.Clear();
						foreach (var s in myServicesDb) MyServices.Add(s);

						PendingAppointments.Clear();
						ConfirmedAppointments.Clear();

						foreach (var app in appointments)
						{
							if (app.Status == "Pendente" || app.Status == "Agendado") PendingAppointments.Add(app);
							else if (app.Status == "Confirmado") ConfirmedAppointments.Add(app);
						}
					});
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Agenda: {ex.Message}");
			}
			finally { IsLoading = false; }
		}

		[RelayCommand]
		private void OpenBookingModal()
		{
			IsEditing = false;
			ModalTitle = "Novo Agendamento";
			ManualPhoneInput = "";
			SelectedManualService = null;
			ManualDate = DateTime.Today;
			ManualTime = new TimeSpan(9, 0, 0);
			IsBookingModalVisible = true;
		}

		[RelayCommand]
		private void Reschedule(Appointment app)
		{
			if (app == null) return;
			IsEditing = true;
			ModalTitle = "Remarcar";
			_currentEditingAppointment = app;
			ManualPhoneInput = "Registrado";
			ManualDate = app.Date.Date;
			ManualTime = app.Date.TimeOfDay;
			SelectedManualService = MyServices.FirstOrDefault(s => s.Name == app.ServiceName);
			IsBookingModalVisible = true;
		}

		[RelayCommand]
		private void CloseBookingModal()
		{
			IsBookingModalVisible = false;
		}

		// --- CORREÇÃO OTIMISTA AQUI ---
		[RelayCommand]
		private async Task ConfirmManualAppointment()
		{
			if (!IsEditing && string.IsNullOrWhiteSpace(ManualPhoneInput))
			{
				await Shell.Current.DisplayAlert("Atenção", "Digite o telefone.", "OK");
				return;
			}

			DateTime finalDateTime = ManualDate.Date + ManualTime;
			IsLoading = true;
			try
			{
				if (IsEditing && _currentEditingAppointment != null)
				{
					// --- EDIÇÃO OTIMISTA ---
					_currentEditingAppointment.Date = finalDateTime;
					if (SelectedManualService != null)
					{
						_currentEditingAppointment.ServiceName = SelectedManualService.Name;
						_currentEditingAppointment.Price = SelectedManualService.Price;
					}

					// Salva no banco (mas a tela já atualizou pelo Binding automático do objeto)
					await _databaseService.UpdateAppointmentAsync(_currentEditingAppointment);

					// Força a lista a se "mexer" para reordenar se a data mudou (opcional, mas bom)
					MainThread.BeginInvokeOnMainThread(() =>
					{
						var item = _currentEditingAppointment;
						ConfirmedAppointments.Remove(item);
						ConfirmedAppointments.Add(item);
						// Idealmente ordenaríamos aqui, mas para MVP adicionar no fim serve
					});

					await Shell.Current.DisplayAlert("Sucesso", "Remarcado!", "OK");
				}
				else
				{
					// --- CRIAÇÃO OTIMISTA ---
					var client = await _databaseService.FindUserByContactInfoAsync(ManualPhoneInput);
					if (client == null)
					{
						await Shell.Current.DisplayAlert("Erro", "Cliente não encontrado.", "OK");
						return;
					}
					if (SelectedManualService == null)
					{
						await Shell.Current.DisplayAlert("Atenção", "Selecione o serviço.", "OK");
						return;
					}

					var partnerId = await _authService.GetUserIdAsync();
					var partnerUser = await _databaseService.GetUserAsync(partnerId);

					var newApp = new Appointment
					{
						Id = Guid.NewGuid().ToString(),
						ClientId = client.Id,
						ClientName = client.Name,
						ClientEmail = client.Email,
						PartnerId = partnerId,
						PartnerName = partnerUser?.StoreName ?? "Minha Loja",
						ServiceName = SelectedManualService.Name,
						Price = SelectedManualService.Price,
						Date = finalDateTime,
						Status = "Confirmado",
						PaymentMethod = "A Definir"
					};

					// Salva no Banco
					await _databaseService.CreateAppointmentAsync(newApp);

					// ATUALIZAÇÃO IMEDIATA NA TELA (Sem LoadData)
					MainThread.BeginInvokeOnMainThread(() =>
					{
						ConfirmedAppointments.Add(newApp);
					});

					await Shell.Current.DisplayAlert("Sucesso", $"Agendado!", "OK");
				}

				IsBookingModalVisible = false;
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
			}
			finally { IsLoading = false; }
		}

		[RelayCommand]
		private async Task CancelConfirmed(Appointment app)
		{
			if (app == null) return;
			bool confirm = await Shell.Current.DisplayAlert("Excluir", $"Cancelar agendamento de {app.ClientName}?", "Sim", "Não");
			if (confirm) await UpdateStatus(app, "Cancelado");
		}

		[RelayCommand]
		private async Task Accept(Appointment app)
		{
			if (app == null) return;
			await UpdateStatus(app, "Confirmado");
		}

		[RelayCommand]
		private async Task Deny(Appointment app)
		{
			if (app == null) return;
			bool confirm = await Shell.Current.DisplayAlert("Recusar", $"Recusar agendamento?", "Sim", "Não");
			if (confirm) await UpdateStatus(app, "Cancelado");
		}

		private async Task UpdateStatus(Appointment app, string status)
		{
			if (string.IsNullOrEmpty(app.Id)) return;
			IsLoading = true;
			try
			{
				await _databaseService.UpdateAppointmentStatusAsync(app, status);

				// ATUALIZAÇÃO OTIMISTA (Já estava aqui, mantive)
				MainThread.BeginInvokeOnMainThread(() =>
				{
					if (status == "Cancelado")
					{
						if (PendingAppointments.Contains(app)) PendingAppointments.Remove(app);
						if (ConfirmedAppointments.Contains(app)) ConfirmedAppointments.Remove(app);
					}
					else if (status == "Confirmado")
					{
						if (PendingAppointments.Contains(app)) PendingAppointments.Remove(app);
						app.Status = "Confirmado";
						if (!ConfirmedAppointments.Contains(app)) ConfirmedAppointments.Add(app);
					}
				});
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", $"Falha: {ex.Message}", "OK");
			}
			finally { IsLoading = false; }
		}

		[RelayCommand]
		private async Task SaveSettings()
		{
			IsLoading = true;
			try
			{
				var userId = await _authService.GetUserIdAsync();
				var user = await _databaseService.GetUserAsync(userId);
				if (user != null)
				{
					user.WorkStart = WorkStart; user.WorkEnd = WorkEnd;
					user.WorkSeg = WorkSeg; user.WorkTer = WorkTer; user.WorkQua = WorkQua;
					user.WorkQui = WorkQui; user.WorkSex = WorkSex; user.WorkSab = WorkSab;
					user.WorkDom = WorkDom; user.WorkHoliday = WorkHoliday;
					await _databaseService.SaveUserAsync(user);
					await Shell.Current.DisplayAlert("Sucesso", "Configurações salvas!", "OK");
				}
			}
			catch { }
			finally { IsLoading = false; }
		}
	}
}