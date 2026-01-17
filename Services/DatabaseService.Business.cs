using Firebase.Database.Query;
using BellaLink.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public partial class DatabaseService
	{
		// --- SERVIÇOS DO SALÃO ---
		public async Task<List<ServiceItem>> GetServicesForPartnerAsync(string partnerId) { try { var items = await _client.Child(ServicesNode).Child(partnerId).OnceAsync<ServiceItem>(); return items.Select(x => { var s = x.Object; s.Id = x.Key; return s; }).ToList(); } catch { return new List<ServiceItem>(); } }
		public async Task AddServiceAsync(string partnerId, ServiceItem service) { if (string.IsNullOrEmpty(partnerId)) return; var result = await _client.Child(ServicesNode).Child(partnerId).PostAsync(service); if (result != null) service.Id = result.Key; }
		public async Task RemoveServiceAsync(string partnerId, string serviceId) { if (string.IsNullOrEmpty(partnerId) || string.IsNullOrEmpty(serviceId)) return; await _client.Child(ServicesNode).Child(partnerId).Child(serviceId).DeleteAsync(); }

		// --- AGENDAMENTOS ---
		public async Task CreateAppointmentAsync(Appointment appointment) { await _client.Child(AppointmentsNode).PostAsync(appointment); }
		public async Task UpdateAppointmentAsync(Appointment appointment) { if (!string.IsNullOrEmpty(appointment.Id)) await _client.Child(AppointmentsNode).Child(appointment.Id).PutAsync(appointment); }
		public async Task DeleteAppointmentAsync(string appointmentId) { if (!string.IsNullOrEmpty(appointmentId)) await _client.Child(AppointmentsNode).Child(appointmentId).DeleteAsync(); }
		public async Task<List<Appointment>> GetAppointmentsForClientAsync(string clientId) { try { var all = await _client.Child(AppointmentsNode).OnceAsync<Appointment>(); return all.Select(x => { var a = x.Object; a.Id = x.Key; return a; }).Where(a => a.ClientId == clientId).OrderByDescending(a => a.Date).ToList(); } catch { return new List<Appointment>(); } }
		public async Task<List<Appointment>> GetAppointmentsForPartnerAsync(string partnerId) { try { var all = await _client.Child(AppointmentsNode).OnceAsync<Appointment>(); return all.Select(x => { var a = x.Object; a.Id = x.Key; return a; }).Where(a => a.PartnerId == partnerId).OrderBy(a => a.Date).ToList(); } catch { return new List<Appointment>(); } }
		public async Task<List<Appointment>> GetPartnerAppointmentsByDateAsync(string partnerId, DateTime date) { try { var all = await _client.Child(AppointmentsNode).OnceAsync<Appointment>(); return all.Select(x => x.Object).Where(a => a.PartnerId == partnerId && a.Date.Date == date.Date && a.Status != "Cancelado").ToList(); } catch { return new List<Appointment>(); } }
		public async Task UpdateAppointmentStatusAsync(Appointment appointment, string newStatus) { if (string.IsNullOrEmpty(appointment.Id)) return; appointment.Status = newStatus; await _client.Child(AppointmentsNode).Child(appointment.Id).Child("Status").PutAsync<string>(newStatus); }

		// --- FINANCEIRO ---
		public async Task AddTransactionAsync(FinancialTransaction transaction) { if (string.IsNullOrEmpty(transaction.Id)) transaction.Id = Guid.NewGuid().ToString(); await _client.Child(FinancialNode).Child(transaction.PartnerId).Child(transaction.Id).PutAsync(transaction); }
		public async Task<List<FinancialTransaction>> GetTransactionsAsync(string partnerId) { try { var items = await _client.Child(FinancialNode).Child(partnerId).OnceAsync<FinancialTransaction>(); return items.Select(x => x.Object).OrderByDescending(t => t.Date).ToList(); } catch { return new List<FinancialTransaction>(); } }
		public async Task DeleteTransactionAsync(string partnerId, string transactionId) { await _client.Child(FinancialNode).Child(partnerId).Child(transactionId).DeleteAsync(); }

		// --- LISTA DE COMPRAS ---
		public async Task AddShoppingItemAsync(ShoppingItem item) { if (string.IsNullOrEmpty(item.Id)) item.Id = Guid.NewGuid().ToString(); await _client.Child(ShoppingNode).Child(item.PartnerId).Child(item.Id).PutAsync(item); }
		public async Task UpdateShoppingItemAsync(ShoppingItem item) { if (!string.IsNullOrEmpty(item.Id)) await _client.Child(ShoppingNode).Child(item.PartnerId).Child(item.Id).PutAsync(item); }
		public async Task<List<ShoppingItem>> GetShoppingListAsync(string partnerId) { try { var items = await _client.Child(ShoppingNode).Child(partnerId).OnceAsync<ShoppingItem>(); return items.Select(x => x.Object).ToList(); } catch { return new List<ShoppingItem>(); } }
		public async Task DeleteShoppingItemAsync(string partnerId, string itemId) { await _client.Child(ShoppingNode).Child(partnerId).Child(itemId).DeleteAsync(); }
	}
}