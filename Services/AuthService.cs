using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public class AuthService : IAuthService
	{
		// ✅ MUDANÇA: Usamos FirebaseAuthClient (Novo) em vez de Provider (Velho)
		private readonly FirebaseAuthClient _authClient;

		private string? _cachedToken;
		private string? _userId;

		public AuthService()
		{
			// ✅ CONFIGURAÇÃO: Jeito certo para a biblioteca nova (4.1.0)
			var config = new FirebaseAuthConfig
			{
				ApiKey = Constants.FirebaseApiKey,
				AuthDomain = "bellalink-dev.firebaseapp.com",
				Providers = new FirebaseAuthProvider[]
				{
					new EmailProvider()
				}
			};

			_authClient = new FirebaseAuthClient(config);
		}

		public async Task<bool> LoginAsync(string email, string password)
		{
			try
			{
				// ✅ COMANDO NOVO
				var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

				// Obtendo token e ID
				var token = await userCredential.User.GetIdTokenAsync();
				var uid = userCredential.User.Uid;

				await SaveSessionAsync(token, uid);
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Login: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> RegisterAsync(string email, string password, string name)
		{
			try
			{
				// ✅ COMANDO NOVO
				var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password, name);

				var token = await userCredential.User.GetIdTokenAsync();
				var uid = userCredential.User.Uid;

				await SaveSessionAsync(token, uid);
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Erro Registro: {ex.Message}");
				return false;
			}
		}

		public async Task SaveSessionAsync(string token, string userId)
		{
			_cachedToken = token;
			_userId = userId;
			await SecureStorage.SetAsync("auth_token", token);
			await SecureStorage.SetAsync("user_id", userId);
		}

		public async Task<string> GetSessionTokenAsync()
		{
			if (!string.IsNullOrEmpty(_cachedToken)) return _cachedToken;
			var token = await SecureStorage.GetAsync("auth_token");
			_cachedToken = token;
			return _cachedToken ?? string.Empty;
		}

		public async Task<string> GetUserIdAsync()
		{
			if (!string.IsNullOrEmpty(_userId)) return _userId;
			var uid = await SecureStorage.GetAsync("user_id");
			_userId = uid;
			return _userId ?? string.Empty;
		}

		public void Logout()
		{
			try { _authClient.SignOut(); } catch { }
			_cachedToken = null;
			_userId = null;
			SecureStorage.Remove("auth_token");
			SecureStorage.Remove("user_id");
		}
	}
}