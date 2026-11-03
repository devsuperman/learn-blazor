using System.Text.Json;
using System.Text.Json.Serialization;
using LearnBlazor.Models;
using Microsoft.JSInterop;

namespace LearnBlazor.Services;

/// <summary>Saves each level's progress in the browser's localStorage.
/// Nothing leaves the browser: no server, no login.</summary>
public sealed class ProgressService(IJSRuntime js)
{
    private const string KeyPrefix = "learn-blazor:progress:";
    private static readonly JsonSerializerOptions Json = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly Dictionary<string, LessonProgress> _cache = [];

    /// <summary>Raised after any save, so the menu can refresh its status badges.</summary>
    public event Action? Changed;

    public async Task<LessonProgress> GetAsync(string topicId)
    {
        if (_cache.TryGetValue(topicId, out var cached))
            return cached;

        LessonProgress progress;
        try
        {
            var raw = await js.InvokeAsync<string?>("localStorage.getItem", KeyPrefix + topicId);
            progress = string.IsNullOrEmpty(raw)
                ? new LessonProgress()
                : JsonSerializer.Deserialize<LessonProgress>(raw, Json) ?? new LessonProgress();
        }
        catch (Exception ex) when (ex is JSException or JsonException)
        {
            // Storage blocked (private mode) or corrupted data: start fresh.
            progress = new LessonProgress();
        }

        _cache[topicId] = progress;
        return progress;
    }

    public async Task SaveAsync(string topicId, LessonProgress progress)
    {
        _cache[topicId] = progress;
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", KeyPrefix + topicId,
                JsonSerializer.Serialize(progress, Json));
        }
        catch (JSException)
        {
            // Storage unavailable: keep working in memory for this visit.
        }
        Changed?.Invoke();
    }

    public async Task ResetAsync(string topicId)
    {
        _cache.Remove(topicId);
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", KeyPrefix + topicId);
        }
        catch (JSException)
        {
        }
        Changed?.Invoke();
    }
}
