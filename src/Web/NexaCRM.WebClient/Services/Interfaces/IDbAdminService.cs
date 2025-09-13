using NexaCRM.WebClient.Models.Db;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IDbAdminService
{
    Task DeleteEntryAsync(int id);
    Task<byte[]> ExportToExcelAsync(DbExportSettings settings);
    Task<IEnumerable<DbCustomer>> SearchAsync(DbSearchCriteria criteria);
}

