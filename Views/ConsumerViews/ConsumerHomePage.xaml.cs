using BellaLink.App.ViewModels.ConsumerViewModels;

namespace BellaLink.App.Views.ConsumerViews;

public partial class ConsumerHomePage : ContentPage
{
	private readonly ConsumerHomeViewModel _viewModel;

	public ConsumerHomePage(ConsumerHomeViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		// Garante que carregue os banners ao abrir a tela
		await _viewModel.Initialize();
	}
}