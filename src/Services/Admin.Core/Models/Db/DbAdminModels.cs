using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Db;

public record DbSearchCriteria(
    bool CheckDuplicates,
    DateTime? From,
    DateTime? To
);

public record DbExportSettings(IList<string> Fields)
{
    public DbExportSettings() : this(new List<string>()) { }
}

