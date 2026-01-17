using BellaLink.App.ViewModels;

namespace BellaLink.App.Views
{
	public partial class LoginPage : ContentPage
	{
		// Injeção de Dependência: O MauiProgram entrega o ViewModel aqui
		public LoginPage(LoginViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}