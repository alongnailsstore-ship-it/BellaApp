#pragma warning disable CS0618 // Silencia avisos de MainPage obsoleto
using BellaLink.App.Views;
using BellaLink.App.Views.ConsumerViews;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using System.Linq;

namespace BellaLink.App
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			return new Window(new AppShell());
		}

		protected override async void OnStart()
		{
			base.OnStart();
			bool jaPediu = Preferences.Default.Get("JaPediuPermissoes", false);
			if (!jaPediu)
			{
				await Task.Delay(1500);
				await RequestAllPermissions();
				Preferences.Default.Set("JaPediuPermissoes", true);
			}
		}

		// --- Lógica para parar vídeos ao minimizar ---
		protected override void OnSleep()
		{
			base.OnSleep();

			// Tenta encontrar se estamos na tela de Shorts
			if (Application.Current?.Windows.Count > 0)
			{
				var page = Application.Current.Windows[0].Page;

				// Se for Shell
				if (page is Shell shell && shell.CurrentPage is ShortsPage shortsPage)
				{
					shortsPage.PauseVideo();
				}
				// Se for navegação direta (fallback)
				else if (page?.Navigation?.NavigationStack?.LastOrDefault() is ShortsPage directPage)
				{
					directPage.PauseVideo();
				}
			}
		}

		private async Task RequestAllPermissions()
		{
			try
			{
				// 1. Câmera
				await CheckAndRequest<Permissions.Camera>();

				// 2. Microfone
				await CheckAndRequest<Permissions.Microphone>();

				// 3. Localização
				await CheckAndRequest<Permissions.LocationWhenInUse>();

				// 4. Arquivos/Mídia
				await CheckAndRequest<Permissions.StorageRead>();

				// 5. Contatos
				await CheckAndRequest<Permissions.ContactsRead>();

				// 6. Calendário
				await CheckAndRequest<Permissions.CalendarRead>();

				// 7. Notificações (Android 13+)
#if ANDROID
				if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
				{
					await CheckAndRequest<Permissions.PostNotifications>();
				}
#endif
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro silencioso nas permissões: {ex.Message}");
			}
		}

		private async Task CheckAndRequest<TPermission>() where TPermission : Permissions.BasePermission, new()
		{
			var status = await Permissions.CheckStatusAsync<TPermission>();
			if (status != PermissionStatus.Granted)
			{
				await Permissions.RequestAsync<TPermission>();
			}
		}
	}
}