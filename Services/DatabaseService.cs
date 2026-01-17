#pragma warning disable CS0618
using Firebase.Database;
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
		private readonly FirebaseClient _client;
		private readonly IAuthService _authService;

		// --- NÓS DO BANCO ---
		private const string PartnersNode = "partners";
		private const string ServicesNode = "services";
		private const string AppointmentsNode = "appointments";
		private const string UsersNode = "users";
		private const string CommentsNode = "comments";
		private const string ShortsNode = "shorts";
		private const string BannersNode = "banners";
		private const string ChatsNode = "chats";
		private const string MessagesNode = "messages";
		private const string ReportsNode = "reports";
		private const string FinancialNode = "financials";
		private const string ShoppingNode = "shopping_lists";
		private const string ReviewsNode = "reviews";
		private const string CommentReportsNode = "reports_comments";
		private const string FollowersNode = "followers";
		private const string ProductsNode = "products";

		public DatabaseService(IAuthService authService)
		{
			_authService = authService;
			_client = new FirebaseClient(Constants.FirebaseDatabaseUrl, new FirebaseOptions
			{
				AuthTokenAsyncFactory = () => _authService.GetSessionTokenAsync()
			});
		}

		// =================================================================================
		// 1. CORE (USUÁRIOS E PARCEIROS)
		// =================================================================================

		public async Task SaveUserAsync(User user)
		{
			if (string.IsNullOrEmpty(user.Id)) throw new Exception("ID inválido.");
			await _client.Child(UsersNode).Child(user.Id).PutAsync(user);

			if (user.IsPartner || user.IsSupplier)
			{
				string fullAddress = "";
				if (user.Addresses != null && user.Addresses.Any())
				{
					var addr = user.Addresses[0];
					fullAddress = $"{addr.Street}, {addr.Number}, {addr.City} - {addr.State}";
				}

				var publicPartner = new Partner
				{
					Id = user.Id,
					Name = string.IsNullOrWhiteSpace(user.StoreName) ? user.Name : user.StoreName,
					Category = user.IsSupplier ? "Distribuidor" : "Geral",
					ImageUrl = !string.IsNullOrEmpty(user.PartnerPhoto) ? user.PartnerPhoto : user.ConsumerPhoto,
					Rating = 5.0,
					Distance = "Ver no Mapa",
					IsOpen = true,
					Address = fullAddress,
					Description = user.StoreDescription
				};

				try
				{
					var oldPartner = await GetPartnerAsync(user.Id);
					if (oldPartner != null)
					{
						publicPartner.Rating = oldPartner.Rating;
						if (!string.IsNullOrEmpty(oldPartner.Category) && !user.IsSupplier)
							publicPartner.Category = oldPartner.Category;
						if (!string.IsNullOrEmpty(oldPartner.Description))
							publicPartner.Description = oldPartner.Description;
					}
				}
				catch { }

				await _client.Child(PartnersNode).Child(user.Id).PutAsync(publicPartner);
			}
			else
			{
				await _client.Child(PartnersNode).Child(user.Id).DeleteAsync();
			}
		}

		public async Task<User?> GetUserAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId)) return null;
			try { return await _client.Child(UsersNode).Child(userId).OnceSingleAsync<User>(); }
			catch { return null; }
		}

		public async Task<Partner?> GetPartnerAsync(string partnerId)
		{
			if (string.IsNullOrEmpty(partnerId)) return null;
			try { return await _client.Child(PartnersNode).Child(partnerId).OnceSingleAsync<Partner>(); }
			catch { return null; }
		}

		public async Task<List<Partner>> GetAllPartnersAsync()
		{
			try
			{
				var items = await _client.Child(PartnersNode).OnceAsync<Partner>();
				return items.Select(x => { var p = x.Object; p.Id = x.Key; return p; }).ToList();
			}
			catch { return new List<Partner>(); }
		}

		public async Task UpdatePartnerDescriptionAsync(string partnerId, string newDescription)
		{
			if (string.IsNullOrEmpty(partnerId)) return;
			var textToSave = newDescription ?? "";
			await _client.Child(PartnersNode).Child(partnerId).Child("Description").PutAsync<string>(textToSave);
		}

		public async Task<User?> FindUserByContactInfoAsync(string searchTerm)
		{
			if (string.IsNullOrWhiteSpace(searchTerm)) return null;
			try
			{
				var users = await _client.Child(UsersNode).OnceAsync<User>();
				foreach (var u in users)
				{
					var user = u.Object;
					if (user != null)
					{
						bool matchWhatsapp = !string.IsNullOrEmpty(user.WhatsApp) && user.WhatsApp.Contains(searchTerm);
						bool matchEmail = !string.IsNullOrEmpty(user.Email) && user.Email.Contains(searchTerm);

						if (matchWhatsapp || matchEmail) return user;
					}
				}
				return null;
			}
			catch { return null; }
		}
	}
}