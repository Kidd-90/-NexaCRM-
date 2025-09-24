using NexaCRM.WebClient.Models.Db;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class DbAdminService : IDbAdminService
{
    public Task DeleteEntryAsync(int id) =>
        Task.CompletedTask;

    public Task<byte[]> ExportToExcelAsync(DbExportSettings settings) =>
        Task.FromResult(Array.Empty<byte>());

    public Task<IEnumerable<DbCustomer>> SearchAsync(DbSearchCriteria criteria) =>
        Task.FromResult<IEnumerable<DbCustomer>>(new List<DbCustomer>());
}

