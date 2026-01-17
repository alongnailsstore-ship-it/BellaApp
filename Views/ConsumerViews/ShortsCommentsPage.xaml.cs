using BellaLink.App.ViewModels.ConsumerViewModels;
namespace BellaLink.App.Views.ConsumerViews
{
	public partial class ShortsCommentsPage : ContentPage
	{
		public ShortsCommentsPage(ShortsCommentsViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (BindingContext is ShortsCommentsViewModel vm) await vm.LoadComments();
		}
	}
}