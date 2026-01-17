using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace BellaLink.App.ViewModels.ConsumerViewModels
{
	[QueryProperty(nameof(PartnerId), "PartnerId")]
	[QueryProperty(nameof(PartnerName), "PartnerName")]
	public partial class RateServiceViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		[ObservableProperty] string partnerId = "";
		[ObservableProperty] string partnerName = "";
		[ObservableProperty] int rating = 5;
		[ObservableProperty] string comment = "";

		[ObservableProperty] Color color1 = Colors.Gold;
		[ObservableProperty] Color color2 = Colors.Gold;
		[ObservableProperty] Color color3 = Colors.Gold;
		[ObservableProperty] Color color4 = Colors.Gold;
		[ObservableProperty] Color color5 = Colors.Gold;

		public RateServiceViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		[RelayCommand]
		void SetRating(string ratingStr)
		{
			if (int.TryParse(ratingStr, out int r))
			{
				Rating = r;
				UpdateStars();
			}
		}

		void UpdateStars()
		{
			Color1 = Rating >= 1 ? Colors.Gold : Colors.Gray;
			Color2 = Rating >= 2 ? Colors.Gold : Colors.Gray;
			Color3 = Rating >= 3 ? Colors.Gold : Colors.Gray;
			Color4 = Rating >= 4 ? Colors.Gold : Colors.Gray;
			Color5 = Rating >= 5 ? Colors.Gold : Colors.Gray;
		}

		[RelayCommand]
		async Task SubmitReview()
		{
			var userId = await _authService.GetUserIdAsync();
			var user = await _databaseService.GetUserAsync(userId);

			var review = new Review
			{
				PartnerId = PartnerId,
				ConsumerId = userId,
				ConsumerName = user?.Name ?? "Anônimo",
				Rating = Rating,
				Comment = Comment
			};

			await _databaseService.AddReviewAsync(review);

			// CORREÇÃO: Usando Shell.Current.DisplayAlert (Método correto do Shell)
			await Shell.Current.DisplayAlert("Sucesso", "Obrigado pela sua avaliação!", "OK");
			await Shell.Current.GoToAsync("..");
		}

		[RelayCommand]
		async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}