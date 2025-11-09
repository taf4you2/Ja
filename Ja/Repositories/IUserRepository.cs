using System.Threading.Tasks;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Interfejs repozytorium użytkowników
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetDefaultUserAsync();
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
