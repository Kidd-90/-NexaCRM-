using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;

namespace NexaCRM.Services.Admin;

public sealed class DbAdminService : IDbAdminService
{
    public Task DeleteEntryAsync(int id) => Task.CompletedTask;

    public Task<byte[]> ExportToExcelAsync(DbExportSettings settings) =>
        Task.FromResult(Array.Empty<byte>());

    public Task<IEnumerable<DbCustomer>> SearchAsync(DbSearchCriteria criteria) =>
        Task.FromResult<IEnumerable<DbCustomer>>(Array.Empty<DbCustomer>());
}
