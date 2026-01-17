using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public interface IAuthService
	{
		Task<bool> LoginAsync(string email, string password);
		Task<bool> RegisterAsync(string email, string password, string name);

		Task SaveSessionAsync(string token, string userId);
		Task<string> GetSessionTokenAsync();

		// MUDANÇA AQUI: Agora é uma Task (Assíncrono)
		Task<string> GetUserIdAsync();

		void Logout();
	}
}