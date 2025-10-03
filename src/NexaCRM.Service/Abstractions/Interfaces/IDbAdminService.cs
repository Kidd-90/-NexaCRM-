using NexaCRM.Services.Admin.Models.Db;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces;

public interface IDbAdminService
{
    Task DeleteEntryAsync(int id);
    Task<byte[]> ExportToExcelAsync(DbExportSettings settings, DbSearchCriteria? criteria = null);
    Task<IEnumerable<DbCustomer>> SearchAsync(DbSearchCriteria criteria);
}

