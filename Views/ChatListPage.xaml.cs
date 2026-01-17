using BellaLink.App.ViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views // <--- O Namespace TEM que ser este para bater com o XAML
{
	public partial class ChatListPage : ContentPage
	{
		public ChatListPage(ChatListViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			// Força o carregamento ao abrir a página
			if (BindingContext is ChatListViewModel vm)
			{
				await vm.LoadChats();
			}
		}
	}
}