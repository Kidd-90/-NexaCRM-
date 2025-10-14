using NexaCRM.Services.Admin.Models.Db;

namespace NexaCRM.Pages.Pages.Db.Shared;

internal static class DbStatusCss
{
    public static string GetStatusTextClass(DbStatus status) => status switch
    {
        DbStatus.New => "font-semibold text-blue-500",
        DbStatus.InProgress => "font-semibold text-amber-500",
        DbStatus.NoAnswer => "font-semibold text-rose-500",
        DbStatus.Completed => "font-semibold text-emerald-500",
        DbStatus.OnHold => "font-semibold text-slate-500",
        _ => "font-semibold text-slate-500"
    };

    public static string GetStatusBadgeClass(DbStatus status) => status switch
    {
        DbStatus.New => "bg-blue-50 text-blue-600 border border-blue-100",
        DbStatus.InProgress => "bg-amber-50 text-amber-600 border border-amber-100",
        DbStatus.NoAnswer => "bg-rose-50 text-rose-600 border border-rose-100",
        DbStatus.Completed => "bg-emerald-50 text-emerald-600 border border-emerald-100",
        DbStatus.OnHold => "bg-slate-50 text-slate-600 border border-slate-100",
        _ => "bg-slate-50 text-slate-600 border border-slate-100"
    };
}
