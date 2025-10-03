using System;
using System.Collections.Generic;

namespace NexaCRM.Services.Admin.Models.Db;

public sealed class DbSearchCriteria
{
    public bool CheckDuplicates { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public string? SearchTerm { get; set; }

    public DbStatus? Status { get; set; }

    public bool IncludeArchived { get; set; }
}

public record DbExportSettings(IList<string> Fields)
{
    public DbExportSettings() : this(new List<string>()) { }
}

