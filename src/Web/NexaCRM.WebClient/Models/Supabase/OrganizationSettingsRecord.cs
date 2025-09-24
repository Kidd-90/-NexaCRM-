using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("organization_settings")]
public sealed class OrganizationSettingsRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("organization_id")]
    public Guid OrganizationId { get; set; }

    [Column("locale")]
    public string Locale { get; set; } = "ko-KR";

    [Column("timezone")]
    public string Timezone { get; set; } = "Asia/Seoul";

    [Column("theme")]
    public string Theme { get; set; } = "light";

    [Column("feature_flags_json")]
    public string? FeatureFlagsJson { get; set; }
}
