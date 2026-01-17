#pragma warning disable CS0618 // Silencia avisos
using BellaLink.App.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BellaLink.App.Views
{
	public partial class SelectProfilePage : ContentPage
	{
		private readonly SelectProfileViewModel _viewModel;

		public SelectProfilePage(SelectProfileViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
			_viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (_viewModel != null)
			{
				// Pequeno delay para garantir que a UI esteja pronta
				await Task.Delay(100);
				await _viewModel.CheckUserRoles();
			}
		}
	}
}