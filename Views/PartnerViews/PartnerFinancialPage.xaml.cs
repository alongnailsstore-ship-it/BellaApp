using BellaLink.App.ViewModels.PartnerViewModels;
using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.Views.PartnerViews
{
	public partial class PartnerFinancialPage : ContentPage
	{
		public PartnerFinancialPage(PartnerFinancialViewModel viewModel)
		{
			try
			{
				InitializeComponent();
				BindingContext = viewModel;
			}
			catch (Exception ex)
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					// CORREÇÃO: Chamando o DisplayAlert da Page de forma correta
					await DisplayAlert("Erro XAML", ex.Message, "OK");
				});
			}
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			try
			{
				if (BindingContext is PartnerFinancialViewModel vm)
				{
					await vm.LoadFinancials();
				}
			}
			catch (Exception ex)
			{
				await DisplayAlert("Erro Load", ex.Message, "OK");
			}
		}
	}
}