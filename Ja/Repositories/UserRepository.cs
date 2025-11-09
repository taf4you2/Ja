using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ja.Database;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Repozytorium dostępu do danych użytkowników
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly JaDbContext _context;

        public UserRepository(JaDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.FTPHistory)
                .Include(u => u.WeightHistory)
                .Include(u => u.PowerZones)
                .Include(u => u.HeartRateZones)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetDefaultUserAsync()
        {
            // Zwraca pierwszego użytkownika lub tworzy domyślnego
            var user = await _context.Users.FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Name = "User",
                    Surname = "Default",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Inicjalizuj domyślne strefy
                JaDbContext.InitializeDefaultPowerZones(_context, user.Id);
                JaDbContext.InitializeDefaultHeartRateZones(_context, user.Id);
            }

            return user;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Inicjalizuj domyślne strefy dla nowego użytkownika
            JaDbContext.InitializeDefaultPowerZones(_context, user.Id);
            JaDbContext.InitializeDefaultHeartRateZones(_context, user.Id);

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
