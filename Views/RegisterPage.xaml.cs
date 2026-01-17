using BellaLink.App.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.Views
{
	public partial class RegisterPage : ContentPage
	{
		private bool _isUpdating;

		public RegisterPage(RegisterViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		private void OnPhoneChanged(object sender, TextChangedEventArgs e)
		{
			if (_isUpdating) return;
			var text = e.NewTextValue ?? "";
			if (string.IsNullOrEmpty(text)) return;

			var entry = sender as Entry;
			if (entry == null) return;

			_isUpdating = true;
			Dispatcher.Dispatch(() =>
			{
				try
				{
					var digits = Regex.Replace(text, "[^0-9]", "");
					string formatted = "";
					if (digits.Length > 11) digits = digits.Substring(0, 11);

					if (digits.Length == 0) formatted = "";
					else if (digits.Length <= 2) formatted = $"({digits}";
					else if (digits.Length <= 7) formatted = $"({digits.Substring(0, 2)}) {digits.Substring(2)}";
					else formatted = $"({digits.Substring(0, 2)}) {digits.Substring(2, 5)}-{digits.Substring(7)}";

					if (entry.Text != formatted)
					{
						entry.Text = formatted;
						try { if (!string.IsNullOrEmpty(entry.Text)) entry.CursorPosition = entry.Text.Length; } catch { }
					}
				}
				catch { }
				finally { _isUpdating = false; }
			});
		}

		// --- MÁSCARA DE CPF ---
		private void OnCpfChanged(object sender, TextChangedEventArgs e)
		{
			if (_isUpdating) return;
			var text = e.NewTextValue ?? "";
			if (string.IsNullOrEmpty(text)) return;

			var entry = sender as Entry;
			if (entry == null) return;

			_isUpdating = true;
			Dispatcher.Dispatch(() =>
			{
				try
				{
					// Remove tudo que não é número
					var digits = Regex.Replace(text, "[^0-9]", "");
					string formatted = "";

					// Limita a 11 dígitos
					if (digits.Length > 11) digits = digits.Substring(0, 11);

					// Aplica a máscara 000.000.000-00 progressivamente
					if (digits.Length <= 3)
						formatted = digits;
					else if (digits.Length <= 6)
						formatted = $"{digits.Substring(0, 3)}.{digits.Substring(3)}";
					else if (digits.Length <= 9)
						formatted = $"{digits.Substring(0, 3)}.{digits.Substring(3, 3)}.{digits.Substring(6)}";
					else
						formatted = $"{digits.Substring(0, 3)}.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}-{digits.Substring(9)}";

					if (entry.Text != formatted)
					{
						entry.Text = formatted;
						try { if (!string.IsNullOrEmpty(entry.Text)) entry.CursorPosition = entry.Text.Length; } catch { }
					}
				}
				catch { }
				finally { _isUpdating = false; }
			});
		}

		private void OnCnpjChanged(object sender, TextChangedEventArgs e)
		{
			if (_isUpdating) return;
			var text = e.NewTextValue ?? "";
			if (string.IsNullOrEmpty(text)) return;

			var entry = sender as Entry;
			if (entry == null) return;

			_isUpdating = true;
			Dispatcher.Dispatch(() =>
			{
				try
				{
					var digits = Regex.Replace(text, "[^0-9]", "");
					string formatted = "";
					if (digits.Length > 14) digits = digits.Substring(0, 14);

					if (digits.Length <= 2) formatted = digits;
					else if (digits.Length <= 5) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2)}";
					else if (digits.Length <= 8) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5)}";
					else if (digits.Length <= 12) formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5, 3)}/{digits.Substring(8)}";
					else formatted = $"{digits.Substring(0, 2)}.{digits.Substring(2, 3)}.{digits.Substring(5, 3)}/{digits.Substring(8, 4)}-{digits.Substring(12)}";

					if (entry.Text != formatted)
					{
						entry.Text = formatted;
						try { if (!string.IsNullOrEmpty(entry.Text)) entry.CursorPosition = entry.Text.Length; } catch { }
					}
				}
				catch { }
				finally { _isUpdating = false; }
			});
		}
	}
}