using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using NexaCRM.WebClient.Services.Supabase;
using Supabase.Gotrue;
using Xunit;

namespace NexaCRM.WebClient.UnitTests.Supabase;

public class BrowserSupabaseSessionPersistenceTests
{
    [Fact]
    public void SaveAndLoadSession_RoundTripsPayload()
    {
        var runtime = new FakeInProcessJSRuntime();
        var persistence = new BrowserSupabaseSessionPersistence(runtime, NullLogger<BrowserSupabaseSessionPersistence>.Instance);
        var session = new Session
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        persistence.SaveSession(session);
        var loaded = persistence.LoadSession();

        Assert.NotNull(loaded);
        Assert.Equal("access-token", loaded!.AccessToken);
        Assert.Equal("refresh-token", loaded.RefreshToken);
    }

    [Fact]
    public void LoadSession_InvalidJson_ReturnsNullAndClearsStorage()
    {
        var runtime = new FakeInProcessJSRuntime();
        runtime.SetItem("supabase.auth.session", "{ invalid json");
        var persistence = new BrowserSupabaseSessionPersistence(runtime, NullLogger<BrowserSupabaseSessionPersistence>.Instance);

        var session = persistence.LoadSession();

        Assert.Null(session);
        Assert.Null(runtime.GetItem("supabase.auth.session"));
    }

    [Fact]
    public void SaveSession_NoInProcessRuntime_DoesNotThrow()
    {
        var runtime = new FakeAsyncJSRuntime();
        var persistence = new BrowserSupabaseSessionPersistence(runtime, NullLogger<BrowserSupabaseSessionPersistence>.Instance);
        var session = new Session { AccessToken = "token" };

        var exception = Record.Exception(() => persistence.SaveSession(session));

        Assert.Null(exception);
    }

    private sealed class FakeInProcessJSRuntime : IJSInProcessRuntime
    {
        private readonly Dictionary<string, string?> _storage = new(StringComparer.Ordinal);

        public TValue Invoke<TValue>(string identifier, params object?[]? args)
        {
            return (TValue?)InvokeCore(identifier, args) ?? default!;
        }

        public void InvokeVoid(string identifier, params object?[]? args)
        {
            InvokeCore(identifier, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return ValueTask.FromResult(Invoke<TValue>(identifier, args));
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return ValueTask.FromResult(Invoke<TValue>(identifier, args));
        }

        public void SetItem(string key, string value)
        {
            _storage[key] = value;
        }

        public string? GetItem(string key)
        {
            return _storage.TryGetValue(key, out var value) ? value : null;
        }

        private object? InvokeCore(string identifier, object?[]? args)
        {
            return identifier switch
            {
                "localStorage.setItem" => SetItem(args!),
                "localStorage.getItem" => GetItemInternal(args!),
                "localStorage.removeItem" => RemoveItem(args!),
                _ => throw new NotSupportedException($"Unsupported identifier '{identifier}'.")
            };
        }

        private object? SetItem(object?[] args)
        {
            var key = args.Length > 0 ? args[0]?.ToString() : null;
            var value = args.Length > 1 ? args[1]?.ToString() : null;
            if (!string.IsNullOrEmpty(key))
            {
                _storage[key] = value;
            }

            return null;
        }

        private string? GetItemInternal(object?[] args)
        {
            var key = args.Length > 0 ? args[0]?.ToString() : null;
            return key is not null && _storage.TryGetValue(key, out var value) ? value : null;
        }

        private object? RemoveItem(object?[] args)
        {
            var key = args.Length > 0 ? args[0]?.ToString() : null;
            if (!string.IsNullOrEmpty(key))
            {
                _storage.Remove(key);
            }

            return null;
        }
    }

    private sealed class FakeAsyncJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return ValueTask.FromResult(default(TValue)!);
        }
    }
}
