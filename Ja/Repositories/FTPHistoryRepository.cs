using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ja.Database;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Repozytorium dostępu do danych historii FTP
    /// </summary>
    public class FTPHistoryRepository : IFTPHistoryRepository
    {
        private readonly JaDbContext _context;

        public FTPHistoryRepository(JaDbContext context)
        {
            _context = context;
        }

        public async Task<List<FTPHistory>> GetAllForUserAsync(int userId)
        {
            return await _context.FTPHistory
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.TestDate)
                .ToListAsync();
        }

        public async Task<FTPHistory?> GetLatestForUserAsync(int userId)
        {
            return await _context.FTPHistory
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.TestDate)
                .FirstOrDefaultAsync();
        }

        public async Task<FTPHistory> AddAsync(FTPHistory ftpHistory)
        {
            ftpHistory.CreatedAt = DateTime.UtcNow;
            await _context.FTPHistory.AddAsync(ftpHistory);
            await _context.SaveChangesAsync();
            return ftpHistory;
        }

        public async Task UpdateAsync(FTPHistory ftpHistory)
        {
            _context.FTPHistory.Update(ftpHistory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ftpHistory = await _context.FTPHistory.FindAsync(id);
            if (ftpHistory != null)
            {
                _context.FTPHistory.Remove(ftpHistory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FTPHistory?> GetByIdAsync(int id)
        {
            return await _context.FTPHistory.FindAsync(id);
        }
    }
}
