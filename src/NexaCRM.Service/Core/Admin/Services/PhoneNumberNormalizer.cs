using System;

namespace NexaCRM.Services.Admin;

internal static class PhoneNumberNormalizer
{
    private const int StackAllocThreshold = 256;

    public static string ExtractDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var length = value.Length;
        Span<char> buffer = length <= StackAllocThreshold ? stackalloc char[length] : new char[length];
        var index = 0;

        foreach (var ch in value)
        {
            if (char.IsDigit(ch))
            {
                buffer[index++] = ch;
            }
        }

        return index == 0 ? string.Empty : new string(buffer[..index]);
    }
}
