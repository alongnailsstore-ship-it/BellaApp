using BellaLink.App.Views; // Para UserProfilePage
using Microsoft.Maui.Controls;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerShell : Shell
	{
		public PartnerShell()
		{
			InitializeComponent();

			// Rota para o Perfil
			Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
		}
	}
}