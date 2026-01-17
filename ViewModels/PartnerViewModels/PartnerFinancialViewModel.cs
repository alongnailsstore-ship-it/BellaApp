using BellaLink.App.Models;
using BellaLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BellaLink.App.ViewModels.PartnerViewModels
{
	public partial class PartnerFinancialViewModel : ObservableObject
	{
		private readonly DatabaseService _databaseService;
		private readonly IAuthService _authService;

		public ObservableCollection<FinancialTransaction> Transactions { get; set; } = new ObservableCollection<FinancialTransaction>();

		[ObservableProperty] private decimal totalBalance;
		[ObservableProperty] private decimal totalIncome;
		[ObservableProperty] private decimal totalExpense;
		[ObservableProperty] private bool isBusy;

		// CORREÇÃO CS8618: Inicializando com valores padrão
		[ObservableProperty] private double incomeProgress = 0;
		[ObservableProperty] private double expenseProgress = 0;
		[ObservableProperty] private string incomePercentText = "0%";
		[ObservableProperty] private string expensePercentText = "0%";

		public PartnerFinancialViewModel(DatabaseService db, IAuthService auth)
		{
			_databaseService = db;
			_authService = auth;
		}

		public async Task LoadFinancials()
		{
			if (IsBusy) return;
			IsBusy = true;

			try
			{
				var userId = await _authService.GetUserIdAsync();
				var list = await _databaseService.GetTransactionsAsync(userId);

				Transactions.Clear();
				if (list != null)
				{
					foreach (var item in list) Transactions.Add(item);
				}

				CalculateTotals();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"ERRO FINANCEIRO: {ex.Message}");
			}
			finally
			{
				IsBusy = false;
			}
		}

		private void CalculateTotals()
		{
			TotalIncome = Transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
			TotalExpense = Transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
			TotalBalance = TotalIncome - TotalExpense;

			decimal totalMovimentado = TotalIncome + TotalExpense;

			if (totalMovimentado > 0)
			{
				IncomeProgress = (double)(TotalIncome / totalMovimentado);
				ExpenseProgress = (double)(TotalExpense / totalMovimentado);

				IncomePercentText = $"{IncomeProgress:P0}";
				ExpensePercentText = $"{ExpenseProgress:P0}";
			}
			else
			{
				IncomeProgress = 0;
				ExpenseProgress = 0;
				IncomePercentText = "0%";
				ExpensePercentText = "0%";
			}
		}

		[RelayCommand]
		private async Task AddTransaction()
		{
			try
			{
				string action = await Shell.Current.DisplayActionSheetAsync("Nova Transação", "Cancelar", null, "Entrada (Receita)", "Saída (Despesa)");
				if (action == "Cancelar" || string.IsNullOrEmpty(action)) return;

				bool isIncome = action == "Entrada (Receita)";

				string desc = await Shell.Current.DisplayPromptAsync("Descrição", "Ex: Corte de Cabelo");
				if (string.IsNullOrWhiteSpace(desc)) return;

				string valueStr = await Shell.Current.DisplayPromptAsync("Valor", "Ex: 50,00", keyboard: Keyboard.Text);

				if (string.IsNullOrWhiteSpace(valueStr)) return;

				if (!TryParseCurrency(valueStr, out decimal amount))
				{
					await Shell.Current.DisplayAlertAsync("Erro", "Valor inválido.", "OK");
					return;
				}

				var trans = new FinancialTransaction
				{
					PartnerId = await _authService.GetUserIdAsync(),
					Description = desc,
					Amount = amount,
					IsIncome = isIncome,
					Category = "Geral",
					Date = DateTime.Now
				};

				await _databaseService.AddTransactionAsync(trans);
				await LoadFinancials();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
			}
		}

		private bool TryParseCurrency(string input, out decimal result)
		{
			result = 0;
			if (string.IsNullOrWhiteSpace(input)) return false;
			string cleanInput = Regex.Replace(input, @"[^\d.,]", "");
			if (decimal.TryParse(cleanInput, NumberStyles.Any, new CultureInfo("pt-BR"), out result)) return true;
			if (decimal.TryParse(cleanInput, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return true;
			string forcedDot = cleanInput.Replace(",", ".");
			if (decimal.TryParse(forcedDot, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return true;
			return false;
		}

		[RelayCommand]
		private async Task DeleteTransaction(FinancialTransaction trans)
		{
			if (trans == null) return;

			// CORREÇÃO CS8604: Verificação de Nulo antes de chamar o banco
			if (string.IsNullOrEmpty(trans.PartnerId) || string.IsNullOrEmpty(trans.Id))
			{
				await Shell.Current.DisplayAlertAsync("Erro", "Item inválido.", "OK");
				return;
			}

			bool confirm = await Shell.Current.DisplayAlertAsync("Excluir", "Apagar registro?", "Sim", "Não");
			if (confirm)
			{
				await _databaseService.DeleteTransactionAsync(trans.PartnerId, trans.Id);
				await LoadFinancials();
			}
		}

		[RelayCommand]
		private async Task GoBack() => await Shell.Current.GoToAsync("..");
	}
}