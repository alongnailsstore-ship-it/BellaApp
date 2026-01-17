using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using BellaLink.App.Services;
using BellaLink.App.ViewModels;
using BellaLink.App.ViewModels.ConsumerViewModels;
using BellaLink.App.ViewModels.PartnerViewModels;
using BellaLink.App.ViewModels.DistributorViewModels;
using BellaLink.App.Views;
using BellaLink.App.Views.ConsumerViews;
using BellaLink.App.Views.PartnerViews;
using BellaLink.App.Views.DistributorViews;
using BellaLink.App.Converters;

namespace BellaLink.App
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseMauiCommunityToolkitMediaElement()
				.UseMauiMaps() // Essencial para o Mapa funcionar
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			// ==========================================================
			// 1. SERVIÇOS E CONVERSORES (Singletons)
			// ==========================================================
			// Lógica: Serviços que existem durante toda a vida do App
			builder.Services.AddSingleton<IAuthService, AuthService>();
			builder.Services.AddSingleton<DatabaseService>();

			// CORREÇÃO CRÍTICA: Adicionado StorageService (faltava no original)
			builder.Services.AddSingleton<StorageService>();

			// ==========================================================
			// 2. VIEWMODELS & PAGES - AUTENTICAÇÃO (Transients)
			// ==========================================================
			// Lógica: Criados sob demanda a cada navegação
			builder.Services.AddTransient<LoginViewModel>();
			builder.Services.AddTransient<LoginPage>();

			builder.Services.AddTransient<RegisterViewModel>();
			builder.Services.AddTransient<RegisterPage>();

			builder.Services.AddTransient<SelectProfileViewModel>();
			builder.Services.AddTransient<SelectProfilePage>();

			builder.Services.AddTransient<UserProfileViewModel>();
			builder.Services.AddTransient<UserProfilePage>();

			// ==========================================================
			// 3. SOCIAL & CHAT
			// ==========================================================
			builder.Services.AddTransient<ChatListViewModel>();
			builder.Services.AddTransient<ChatListPage>();

			builder.Services.AddTransient<ChatViewModel>();
			builder.Services.AddTransient<ChatPage>();

			// ==========================================================
			// 4. CONSUMIDOR
			// ==========================================================
			builder.Services.AddTransient<ConsumerHomeViewModel>();
			builder.Services.AddTransient<ConsumerHomePage>();

			builder.Services.AddTransient<ShortsViewModel>();
			builder.Services.AddTransient<ShortsPage>();

			builder.Services.AddTransient<ShortsCommentsViewModel>();
			builder.Services.AddTransient<ShortsCommentsPage>();

			builder.Services.AddTransient<MyAppointmentsViewModel>();
			builder.Services.AddTransient<MyAppointmentsPage>();

			builder.Services.AddTransient<PartnerDetailsViewModel>();
			builder.Services.AddTransient<PartnerDetailsPage>();

			builder.Services.AddTransient<BookingDateTimeViewModel>();
			builder.Services.AddTransient<BookingDateTimePage>();

			builder.Services.AddTransient<BookingConfirmationViewModel>();
			builder.Services.AddTransient<BookingConfirmationPage>();

			builder.Services.AddTransient<RateServiceViewModel>();
			builder.Services.AddTransient<RateServicePage>();

			builder.Services.AddTransient<MapSearchViewModel>();
			builder.Services.AddTransient<MapSearchPage>();

			// ==========================================================
			// 5. PARCEIRO (SALÃO/PROFISSIONAL)
			// ==========================================================
			builder.Services.AddTransient<PartnerDashboardViewModel>();
			builder.Services.AddTransient<PartnerDashboardPage>();

			builder.Services.AddTransient<PartnerServicesViewModel>();
			builder.Services.AddTransient<PartnerServicesPage>();

			builder.Services.AddTransient<PartnerAppointmentsViewModel>();
			builder.Services.AddTransient<PartnerAppointmentsPage>();

			builder.Services.AddTransient<PartnerUploadViewModel>();
			builder.Services.AddTransient<PartnerUploadPage>();

			builder.Services.AddTransient<PartnerPromoViewModel>();
			builder.Services.AddTransient<PartnerPromoPage>();

			builder.Services.AddTransient<PartnerFinancialViewModel>();
			builder.Services.AddTransient<PartnerFinancialPage>();

			builder.Services.AddTransient<PartnerShoppingViewModel>();
			builder.Services.AddTransient<PartnerShoppingPage>();

			// Detalhes do Fornecedor (Visto pelo Parceiro)
			builder.Services.AddTransient<SupplierDetailsViewModel>();
			builder.Services.AddTransient<SupplierDetailsPage>();

			// ==========================================================
			// 6. DISTRIBUIDOR (FORNECEDOR)
			// ==========================================================
			builder.Services.AddTransient<DistributorDashboardViewModel>();
			builder.Services.AddTransient<DistributorDashboardPage>();

			builder.Services.AddTransient<DistributorProductsViewModel>();
			builder.Services.AddTransient<DistributorProductsPage>();

			builder.Services.AddTransient<DistributorOrdersViewModel>();
			builder.Services.AddTransient<DistributorOrdersPage>();

			return builder.Build();
		}
	}
}