using System.Net;
using System.Net.Http.Json;
using Activity.Application;
using Activity.Application.Abstractions;

namespace Activity.Infrastructure.Catalog;

/// <summary>
/// HTTP-клиент к сервису Catalog. Вызов метода превратился в сетевой вызов,
/// который может упасть, — это нормальная ситуация в микросервисах.
/// </summary>
public class CatalogClient(HttpClient http) : ICatalogClient
{
    public async Task<bool> MangaExistsAsync(Guid mangaId, CancellationToken ct = default)
    {
        using var response = await SendAsync($"api/manga/{mangaId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<CatalogChapter?> GetChapterAsync(Guid chapterId, CancellationToken ct = default)
    {
        using var response = await SendAsync($"api/chapters/{chapterId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CatalogChapter>(ct);
    }

    private async Task<HttpResponseMessage> SendAsync(string url, CancellationToken ct)
    {
        try
        {
            var response = await http.GetAsync(url, ct);

            if ((int)response.StatusCode >= 500)
            {
                response.Dispose();
                throw new CatalogUnavailableException();
            }

            return response;
        }
        catch (HttpRequestException)
        {
            throw new CatalogUnavailableException(); // Catalog выключен или недоступен по сети
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new CatalogUnavailableException(); // таймаут
        }
    }
}
