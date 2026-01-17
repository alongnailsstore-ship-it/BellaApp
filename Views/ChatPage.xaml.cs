using BellaLink.App.ViewModels;
using Microsoft.Maui.Controls;
using System.Linq;

namespace BellaLink.App.Views
{
	public partial class ChatPage : ContentPage
	{
		public ChatPage(ChatViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;

			// Inscreve no evento de coleção alterada para rolar automaticamente
			viewModel.Messages.CollectionChanged += (s, e) =>
			{
				if (viewModel.Messages.Count > 0)
				{
					// Pequeno delay para garantir que a UI renderizou o item
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						await Task.Delay(100);
						// Rola para o último item
						MessagesList.ScrollTo(viewModel.Messages.Count - 1, position: ScrollToPosition.End, animate: true);
					});
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (BindingContext is ChatViewModel vm)
			{
				await vm.Initialize();
				// Rola para o fim ao abrir
				if (vm.Messages.Count > 0)
				{
					MessagesList.ScrollTo(vm.Messages.Count - 1, position: ScrollToPosition.End, animate: false);
				}
			}
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			if (BindingContext is ChatViewModel vm)
			{
				vm.Dispose();
			}
		}
	}
}