using BellaLink.App.ViewModels.ConsumerViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class ShortsPage : ContentPage
	{
		private readonly ShortsViewModel? _viewModel;

		public ShortsPage(ShortsViewModel viewModel)
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
				await _viewModel.LoadVideos();

				// Pequeno delay para garantir que o layout renderizou antes de tocar
				Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
				{
					_viewModel.SetActiveVideo(0);
				});
			}
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_viewModel?.StopAll();
		}

		private void OnPositionChanged(object sender, PositionChangedEventArgs e)
		{
			_viewModel?.SetActiveVideo(e.CurrentPosition);
		}

		public void PauseVideo()
		{
			_viewModel?.StopAll();
		}
	}
}