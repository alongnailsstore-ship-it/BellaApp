using Microsoft.Maui.Controls;
using BellaLink.App.Views; // Para UserProfilePage
using System;

namespace BellaLink.App.Views.ConsumerViews
{
	public partial class ConsumerShell : Shell
	{
		public ConsumerShell()
		{
			try
			{
				InitializeComponent();

				// --- REGISTRO DE ROTAS ---
				// Se houver conflito de rotas, o erro acontece aqui

				Routing.RegisterRoute(nameof(PartnerDetailsPage), typeof(PartnerDetailsPage));
				Routing.RegisterRoute(nameof(BookingDateTimePage), typeof(BookingDateTimePage));
				Routing.RegisterRoute(nameof(BookingConfirmationPage), typeof(BookingConfirmationPage));

				// IMPORTANTE: Rota do Perfil
				Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"XXX CRASH NO CONSTRUTOR DO CONSUMERSHELL: {ex.Message}");
				// Relança para ser pego pelo ViewModel
				throw new Exception($"Erro no ConsumerShell: {ex.Message}");
			}
		}
	}
}