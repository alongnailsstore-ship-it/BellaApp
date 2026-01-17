using BellaLink.App.ViewModels.PartnerViewModels;
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerUploadPage : ContentPage
	{
		public PartnerUploadPage(PartnerUploadViewModel viewModel)
		{
			InitializeComponent(); // Agora vai encontrar
			BindingContext = viewModel;
		}
	}
}