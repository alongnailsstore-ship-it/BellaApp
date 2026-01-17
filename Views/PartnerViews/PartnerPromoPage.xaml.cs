// Verifique se o namespace está assim no PartnerPromoPage.xaml.cs:
namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerPromoPage : ContentPage
	{
		public PartnerPromoPage(BellaLink.App.ViewModels.PartnerViewModels.PartnerPromoViewModel viewModel)
		{
			InitializeComponent(); // Agora vai funcionar
			BindingContext = viewModel;
		}
	}
}