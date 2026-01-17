using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public partial class PartnerPromoViewModel : ObservableObject
	{
		private readonly DatabaseService _db;
		private readonly IAuthService _auth;

		[ObservableProperty] string title;
		[ObservableProperty] string durationInput;
		[ObservableProperty] bool isBusy;

		public PartnerPromoViewModel(DatabaseService db, IAuthService auth)
		{
			_db = db;
			_auth = auth;
		}

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");

		[RelayCommand]
		private async Task PublishBanner()
		{
			if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(DurationInput))
			{
				await Shell.Current.DisplayAlert("Atenção", "Preencha todos os campos.", "OK");
				return;
			}

			IsBusy = true;
			try
			{
				// Lógica de salvar (simulada ou real)
				await Task.Delay(1000);
				await Shell.Current.DisplayAlert("Sucesso", "Promoção publicada!", "OK");
				await GoBack();
			}
			catch (System.Exception ex)
			{
				await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}