using BellaLink.App.ViewModels.PartnerViewModels;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerDashboardPage : ContentPage
	{
		public PartnerDashboardPage(PartnerDashboardViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}