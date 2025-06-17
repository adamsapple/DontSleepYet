using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DontSleepYet.Contracts.Services;
using Microsoft.Extensions.Options;


namespace DontSleepYet.Services;

public class GithubUpdateCheckService : IUpdateCheckService
{
    public DateTime LastCheckedAt 
    {
        get => throw new NotImplementedException();
        private set => throw new NotImplementedException();
    }

    public DateTime NextCheckAt
    {
        get => throw new NotImplementedException();
        private set => throw new NotImplementedException();
    }
    public TimeSpan CheckPeriod
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    private GithubBaseOption baseOption;

    public GithubUpdateCheckService(IOptions<GithubBaseOption> baseOption)
    {
        this.baseOption = baseOption?.Value ?? throw new ArgumentNullException(nameof(baseOption));
    }

    public async Task<UpdateCheckData> CheckUpdateAsync()
    {
        var url     = new Uri($"https://api.github.com/repos/{baseOption.User}/{baseOption.Repository}/releases/latest");
        
        var client  = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        //request.Headers.Add("Content-Type", "application/json");
        request.Headers.Add("Accept", "application/vnd.github.v3+json");
        request.Headers.Add("User-Agent", baseOption.Repository);

        var response = await client.SendAsync(request);
        
        var isCheckSuccess = response.IsSuccessStatusCode;
        var isUpdateAvailable = false;
        var latestVersion = string.Empty;
        var description = string.Empty;
        var publishedAt = DateTime.MinValue;
        Uri infoUrl = null;

        {
            var release = JsonSerializer.Deserialize<GithubReleaseData>(
                                await response.Content.ReadAsStringAsync(),
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
           　                );

            if (!string.IsNullOrEmpty(release?.LatestVersion))
            {
                latestVersion = release.LatestVersion;
                description 　= release.Body ?? string.Empty;
                infoUrl 　　　= new Uri(release.HtmlUrl ?? string.Empty);
                publishedAt 　= release.PublishedAt.LocalDateTime;
                isUpdateAvailable = true;
            }
        }

        var result = new UpdateCheckData(isCheckSuccess: isCheckSuccess,
                            isUpdateAvailable: isUpdateAvailable,
                            latestVersion: latestVersion,
                            publishedAt: publishedAt,
                            description: description,
                            infoUrl: infoUrl);        

        return result;
    }
}

public class GithubBaseOption
{
    public string User { get; set; }
    public string Repository { get; set; }
}

class GithubReleaseData
{
    [JsonPropertyName("tag_name")]
    public string? LatestVersion { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("published_at")]
    public DateTimeOffset PublishedAt { get; set; }
}