using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Interfejs repozytorium historii FTP
    /// </summary>
    public interface IFTPHistoryRepository
    {
        Task<List<FTPHistory>> GetAllForUserAsync(int userId);
        Task<FTPHistory?> GetLatestForUserAsync(int userId);
        Task<FTPHistory> AddAsync(FTPHistory ftpHistory);
        Task UpdateAsync(FTPHistory ftpHistory);
        Task DeleteAsync(int id);
        Task<FTPHistory?> GetByIdAsync(int id);
    }
}
