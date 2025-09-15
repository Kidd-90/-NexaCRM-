using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Pages;

public partial class SmsHistoryPage
{
    [Inject] private ISmsService SmsService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IEnumerable<SmsHistoryItem>? history;
    private DateTime? startDate;
    private DateTime? endDate;
    private string recipient = string.Empty;
    private string status = string.Empty;

    private int _currentPage = 1;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        history = await SmsService.GetHistoryAsync();
    }

    private IEnumerable<SmsHistoryItem> FilteredHistory =>
        history?.Where(item =>
            (!startDate.HasValue || item.SentAt.Date >= startDate.Value.Date) &&
            (!endDate.HasValue || item.SentAt.Date <= endDate.Value.Date) &&
            (string.IsNullOrWhiteSpace(recipient) || item.Recipient.Contains(recipient, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(status) || string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase)))
        ?? Enumerable.Empty<SmsHistoryItem>();

    private IEnumerable<SmsHistoryItem> PagedHistory =>
        FilteredHistory.Skip((_currentPage - 1) * PageSize).Take(PageSize);

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredHistory.Count() / (double)PageSize));

    private void PrevPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
        }
    }

    private void NextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
        }
    }

    private void ResetPage()
    {
        _currentPage = 1;
    }

    private async Task ExportCsv()
    {
        var lines = new List<string> { "Recipient,Message,SentAt,Status" };
        foreach (var item in FilteredHistory)
        {
            var line = string.Join(',', Escape(item.Recipient), Escape(item.Message), item.SentAt.ToString("u"), Escape(item.Status));
            lines.Add(line);
        }
        var csv = string.Join("\n", lines);
        await JS.InvokeVoidAsync("downloadCsv", "sms_history.csv", csv);
    }

    private static string Escape(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "\"\"";
        return $"\"{input.Replace("\"", "\"\"")}\"";
    }
}
