using System;
using System.Collections.Generic;

namespace NexaCRM.Services.Admin.Models.SupabaseOperations;

public sealed record SupabaseAuditSyncReport(
    bool IsConsistent,
    int AuditLogCount,
    int IntegrationEventCount,
    IReadOnlyList<string> Issues,
    DateTimeOffset CheckedAt
);

