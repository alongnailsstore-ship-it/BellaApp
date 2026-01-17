using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;
using System.Reactive.Linq;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
using System.Linq;

namespace BellaLink.App.ViewModels
{
	[QueryProperty(nameof(ContactId), "ContactId")]
	[QueryProperty(nameof(ContactName), "ContactName")]
	[QueryProperty(nameof(ContactPhoto), "ContactPhoto")]
	public partial class ChatViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;
		private readonly StorageService _storageService;

		// Propriedades da Navegação
		[ObservableProperty] private string contactId = "";
		[ObservableProperty] private string contactName = "";
		[ObservableProperty] private string contactPhoto = "";

		// Campo de Texto
		[ObservableProperty] private string messageText = "";

		// Lista de Mensagens
		public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

		// Variáveis Privadas
		private string _myUserId = "";
		private string _myUserName = "";
		private string _myUserPhoto = ""; // Guarda minha foto para enviar no chat
		private string _roomId = "";
		private IDisposable? _subscription;

		public ChatViewModel(DatabaseService db, IAuthService auth, StorageService storage)
		{
			_databaseService = db;
			_authService = auth;
			_storageService = storage;
		}

		public async Task Initialize()
		{
			_myUserId = await _authService.GetUserIdAsync();
			var user = await _databaseService.GetUserAsync(_myUserId);

			_myUserName = user?.Name ?? "Eu";

			// 1. GARANTE MINHA FOTO (Para enviar nas mensagens)
			_myUserPhoto = !string.IsNullOrEmpty(user?.PartnerPhoto)
						   ? user.PartnerPhoto
						   : (user?.ConsumerPhoto ?? "");

			// 2. GARANTE FOTO DO CONTATO (Se veio vazia da navegação, busca no banco)
			if (!string.IsNullOrEmpty(ContactId))
			{
				var contactUser = await _databaseService.GetUserAsync(ContactId);
				if (contactUser != null)
				{
					string realPhoto = !string.IsNullOrEmpty(contactUser.PartnerPhoto)
						? contactUser.PartnerPhoto
						: (contactUser.ConsumerPhoto ?? "");

					if (!string.IsNullOrEmpty(realPhoto))
					{
						ContactPhoto = realPhoto;
					}
				}
			}

			// 3. DEFINE A SALA (RoomId)
			_roomId = _databaseService.GetChatRoomId(_myUserId, ContactId);

			// 4. CARREGA HISTÓRICO
			var history = await _databaseService.GetChatHistoryAsync(_roomId);
			if (history != null)
			{
				Messages.Clear();
				foreach (var msg in history) ProcessMessage(msg);
			}

			// 5. ESCUTA NOVAS MENSAGENS (Realtime)
			_subscription = _databaseService.ListenToMessages(_roomId)
				.Subscribe(d =>
				{
					if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
					{
						MainThread.BeginInvokeOnMainThread(() =>
						{
							// Só processa se for nova ou atualização de status
							var incomingMsg = d.Object;
							var existing = Messages.FirstOrDefault(m => m.Id == incomingMsg.Id);

							if (existing == null)
							{
								ProcessMessage(incomingMsg);
							}
							else if (existing.Status != incomingMsg.Status)
							{
								// Atualiza tick azul
								existing.Status = incomingMsg.Status;
								int idx = Messages.IndexOf(existing);
								Messages[idx] = existing; // Força refresh visual
							}
						});
					}
				});
		}

		private void ProcessMessage(ChatMessage msg)
		{
			msg.IsMine = msg.SenderId == _myUserId;

			// Se a mensagem é do outro e eu estou vendo agora -> Marcar como Lida
			if (!msg.IsMine && msg.Status != MessageStatus.Read)
			{
				_ = _databaseService.MarkMessageAsReadAsync(_roomId, msg.Id ?? "");
				msg.Status = MessageStatus.Read;
			}

			// Adiciona na lista visual se ainda não estiver lá
			if (Messages.All(m => m.Id != msg.Id))
			{
				AddMessageWithDateLogic(msg);
			}
		}

		private void AddMessageWithDateLogic(ChatMessage msg)
		{
			// Verifica data da última mensagem REAL (ignora headers)
			var lastRealMsg = Messages.LastOrDefault(m => !m.IsHeader);

			if (lastRealMsg == null || lastRealMsg.Timestamp.Date != msg.Timestamp.Date)
			{
				string headerText = GetDateText(msg.Timestamp);

				// Evita header duplicado
				var lastMsg = Messages.LastOrDefault();
				if (lastMsg == null || !lastMsg.IsHeader || lastMsg.HeaderText != headerText)
				{
					Messages.Add(new ChatMessage { IsHeader = true, HeaderText = headerText, Id = Guid.NewGuid().ToString() });
				}
			}
			Messages.Add(msg);
		}

		private string GetDateText(DateTime date)
		{
			if (date.Date == DateTime.Today) return "Hoje";
			if (date.Date == DateTime.Today.AddDays(-1)) return "Ontem";
			return date.ToString("dd/MM/yyyy");
		}

		[RelayCommand]
		private async Task SendText()
		{
			if (string.IsNullOrWhiteSpace(MessageText)) return;

			string textToSend = MessageText;
			MessageText = ""; // Limpa input na hora

			// Cria msg local (Otimista)
			var msg = new ChatMessage
			{
				Id = Guid.NewGuid().ToString(),
				SenderId = _myUserId,
				ReceiverId = ContactId,
				Text = textToSend,
				Timestamp = DateTime.Now,
				Status = MessageStatus.Sending // Reloginho
			};

			// Mostra na tela imediatamente
			ProcessMessage(msg);

			try
			{
				// Envia para o banco
				await _databaseService.SendMessageAsync(msg, _roomId, _myUserName, ContactId, ContactName, ContactPhoto, _myUserPhoto);

				// Sucesso -> Vira 1 Tick
				msg.Status = MessageStatus.Sent;
				int idx = Messages.IndexOf(msg);
				if (idx >= 0) Messages[idx] = msg;
			}
			catch
			{
				// Erro silencioso ou alert
				await Shell.Current.DisplayAlertAsync("Erro", "Falha ao enviar.", "OK");
			}
		}

		[RelayCommand]
		private async Task SendImage()
		{
			try
			{
				var result = await MediaPicker.Default.PickPhotosAsync();
				var photo = result?.FirstOrDefault();

				if (photo != null)
				{
					// 1. VISUAL INSTANTÂNEO (Otimista)
					// Mostra a foto local na tela enquanto sobe
					var tempMsg = new ChatMessage
					{
						Id = Guid.NewGuid().ToString(),
						SenderId = _myUserId,
						ReceiverId = ContactId,
						Text = "",
						ImageUrl = photo.FullPath, // Caminho local do celular
						Timestamp = DateTime.Now,
						Status = MessageStatus.Sending // Reloginho
					};

					ProcessMessage(tempMsg);

					// 2. UPLOAD (Pode demorar)
					using var stream = await photo.OpenReadAsync();
					var urlOnline = await _storageService.UploadVideoAsync(stream, photo.FileName);

					if (!string.IsNullOrEmpty(urlOnline))
					{
						// 3. SUCESSO: Atualiza com link real e status Enviado
						tempMsg.ImageUrl = urlOnline;
						tempMsg.Status = MessageStatus.Sent;

						// 4. ENVIA PARA O BANCO (Agora sim!)
						await _databaseService.SendMessageAsync(tempMsg, _roomId, _myUserName, ContactId, ContactName, ContactPhoto, _myUserPhoto);
					}
					else
					{
						await Shell.Current.DisplayAlertAsync("Erro", "Falha ao gerar link da imagem.", "OK");
					}
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", $"Falha no envio: {ex.Message}", "OK");
			}
		}

		public void Dispose() => _subscription?.Dispose();

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}