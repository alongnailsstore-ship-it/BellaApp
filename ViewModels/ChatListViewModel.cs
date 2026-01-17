using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace BellaLink.App.ViewModels
{
	public partial class ChatListViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<ChatContact> Chats { get; set; } = new ObservableCollection<ChatContact>();

		[ObservableProperty] private bool isLoading;

		public ChatListViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadChats()
		{
			if (IsLoading) return;
			IsLoading = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();
				var list = await _databaseService.GetMyChatsAsync(userId);

				Chats.Clear();

				if (list.Count == 0)
				{
					// Contato de Exemplo
					Chats.Add(new ChatContact
					{
						Name = "Suporte BellaLink",
						LastMessage = "Olá! Arraste o botão rosa para testar.",
						Photo = "https://ui-avatars.com/api/?name=Suporte+Bella&background=FF4081&color=fff",
						UnreadCount = 1,
						ContactId = "suporte_fake_id",
						LastUpdate = DateTime.Now
					});
				}

				foreach (var c in list) Chats.Add(c);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Chat: {ex.Message}");
			}
			finally { IsLoading = false; }
		}

		[RelayCommand]
		private async Task AddContact()
		{
			string phone = await Shell.Current.DisplayPromptAsync("Novo Chat", "Digite o telefone:", "Buscar", "Cancelar", keyboard: Keyboard.Telephone);
			if (string.IsNullOrWhiteSpace(phone)) return;

			IsLoading = true;
			try
			{
				var userFound = await _databaseService.FindUserByContactInfoAsync(phone);

				if (userFound != null)
				{
					// Verifica se não é o próprio usuário
					var myId = await _authService.GetUserIdAsync();
					if (userFound.Id == myId)
					{
						// CORREÇÃO CS0618: DisplayAlertAsync
						await Shell.Current.DisplayAlertAsync("Ops", "Você não pode conversar consigo mesmo.", "OK");
						return;
					}

					var navParam = new Dictionary<string, object>
					{
						{ "ContactId", userFound.Id ?? "" },
						{ "ContactName", userFound.Name ?? "Usuário" },
						{ "ContactPhoto", userFound.ConsumerPhoto ?? "" }
					};
					await Shell.Current.GoToAsync("ChatPage", navParam);
				}
				else
				{
					// CORREÇÃO CS0618: DisplayAlertAsync
					bool invite = await Shell.Current.DisplayAlertAsync("Não encontrado", "Convidar para o App?", "Sim", "Não");
					if (invite)
					{
						await Share.Default.RequestAsync(new ShareTextRequest { Title = "Convite", Text = "Baixe o BellaLink!" });
					}
				}
			}
			finally { IsLoading = false; }
		}

		[RelayCommand]
		private async Task OpenChat(ChatContact contact)
		{
			if (contact == null) return;

			string photoUrl = !string.IsNullOrEmpty(contact.Photo)
				? contact.Photo
				: "https://ui-avatars.com/api/?name=" + (contact.Name ?? "U") + "&background=FF4081&color=fff";

			var navParam = new Dictionary<string, object>
			{
				{ "ContactId", contact.ContactId ?? "" },
				{ "ContactName", contact.Name ?? "Usuário" },
				{ "ContactPhoto", photoUrl }
			};

			await Shell.Current.GoToAsync("ChatPage", navParam);
		}

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}