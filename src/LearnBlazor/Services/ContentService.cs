using System.Net.Http.Json;
using LearnBlazor.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace LearnBlazor.Services;

/// <summary>Loads the static study content from wwwroot/content.</summary>
public sealed class ContentService(HttpClient http)
{
    private List<TopicSummary>? _topics;
    private readonly Dictionary<string, (Lesson Lesson, string Theory)> _lessons = [];

    public async Task<IReadOnlyList<TopicSummary>> GetTopicsAsync()
    {
        _topics ??= await GetJsonAsync<List<TopicSummary>>("content/topics.json") ?? [];
        return _topics;
    }

    /// <summary>Returns null when the topic does not exist or is not available yet.</summary>
    public async Task<(Lesson Lesson, string Theory)?> GetLessonAsync(string id)
    {
        if (_lessons.TryGetValue(id, out var cached))
            return cached;

        var topics = await GetTopicsAsync();
        if (!topics.Any(t => t.Id == id && t.Available))
            return null;

        var lesson = await GetJsonAsync<Lesson>($"content/{id}/lesson.json");
        if (lesson is null)
            return null;

        var theory = await SendAsync($"content/{id}/theory.md", r => r.Content.ReadAsStringAsync());
        _lessons[id] = (lesson, theory);
        return _lessons[id];
    }

    private Task<T?> GetJsonAsync<T>(string path) =>
        SendAsync(path, r => r.Content.ReadFromJsonAsync<T>());

    // Skip the browser cache so a new deploy shows new content right away.
    private async Task<T> SendAsync<T>(string path, Func<HttpResponseMessage, Task<T>> read)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.SetBrowserRequestCache(BrowserRequestCache.NoCache);
        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await read(response);
    }
}
