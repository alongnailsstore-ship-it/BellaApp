using BellaLink.App.Views;
using BellaLink.App.Views.ConsumerViews;
using BellaLink.App.Views.PartnerViews;
using BellaLink.App.Views.DistributorViews;
using Microsoft.Maui.Controls;

namespace BellaLink.App
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			RegisterRoutes();
		}

		private void RegisterRoutes()
		{
			// --- LOGIN & CORE ---
			// Permite navegar de volta para a seleção de perfil sem crashar
			Routing.RegisterRoute(nameof(SelectProfilePage), typeof(SelectProfilePage));

			Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
			Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
			Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));

			// --- CONSUMIDOR ---
			Routing.RegisterRoute(nameof(PartnerDetailsPage), typeof(PartnerDetailsPage));
			Routing.RegisterRoute(nameof(ShortsCommentsPage), typeof(ShortsCommentsPage));
			Routing.RegisterRoute(nameof(BookingDateTimePage), typeof(BookingDateTimePage));
			Routing.RegisterRoute(nameof(BookingConfirmationPage), typeof(BookingConfirmationPage));
			Routing.RegisterRoute(nameof(RateServicePage), typeof(RateServicePage));
			Routing.RegisterRoute(nameof(MapSearchPage), typeof(MapSearchPage));

			// --- PARCEIRO ---
			Routing.RegisterRoute(nameof(PartnerUploadPage), typeof(PartnerUploadPage));
			Routing.RegisterRoute(nameof(PartnerPromoPage), typeof(PartnerPromoPage));
			Routing.RegisterRoute(nameof(PartnerFinancialPage), typeof(PartnerFinancialPage));
			Routing.RegisterRoute(nameof(PartnerShoppingPage), typeof(PartnerShoppingPage));
			Routing.RegisterRoute(nameof(SupplierDetailsPage), typeof(SupplierDetailsPage));

			// --- SOCIAL (Chat) ---
			Routing.RegisterRoute(nameof(ChatListPage), typeof(ChatListPage));
			Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));

			// --- DISTRIBUIDOR ---
			Routing.RegisterRoute(nameof(DistributorDashboardPage), typeof(DistributorDashboardPage));
			Routing.RegisterRoute(nameof(DistributorProductsPage), typeof(DistributorProductsPage));
			// --- NOVO: Rota de Pedidos ---
			Routing.RegisterRoute(nameof(DistributorOrdersPage), typeof(DistributorOrdersPage));
		}
	}
}