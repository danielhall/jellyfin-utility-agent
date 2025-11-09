using RestSharp;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class JellyfinClient
{
    private readonly RestClient _client;
    private string _token = "";
    private string _userId = "";

    public JellyfinClient(string baseUrl)
    {
        _client = new RestClient(new RestClientOptions(baseUrl)
        {
            ThrowOnAnyError = true
        });
    }

    private static string BuildClientHeader(string appName, string deviceName, string deviceId, string version)
        => $"MediaBrowser Client=\"{appName}\", Device=\"{deviceName}\", DeviceId=\"{deviceId}\", Version=\"{version}\"";

    public async Task LoginAsync(string username, string password,
                                 string appName = "ai.danieljh.uk",
                                 string deviceName = null,
                                 string deviceId = null,
                                 string appVersion = "1.0.0")
    {
        deviceName ??= Environment.MachineName;
        deviceId ??= Guid.NewGuid().ToString("N");

        var clientHeader = BuildClientHeader(appName, deviceName, deviceId, appVersion);

        var req = new RestRequest("/Users/AuthenticateByName", Method.Post)
            .AddHeader("X-Emby-Authorization", clientHeader)
            // Many servers also accept it under 'Authorization'; adding both is harmless.
            .AddHeader("Authorization", clientHeader)
            .AddJsonBody(new { Username = username, Pw = password });

        var resp = await _client.ExecuteAsync<AuthenticationResult>(req);
        _token = resp.Data?.AccessToken ?? throw new Exception("No token returned from Jellyfin.");

        // Resolve current user (needed by several endpoints).
        var me = await AuthedGetAsync<UserDto>("/Users/Me");
        _userId = me.Id ?? throw new Exception("Could not resolve user id.");
    }

    public async Task<BaseItemDtoQueryResult> GetMoviesPageAsync(int startIndex = 0, int pageSize = 100)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)               // required if you’re not using an API key
            .AddQueryParameter("includeItemTypes", "Movie")
            .AddQueryParameter("recursive", "true")
            .AddQueryParameter("startIndex", startIndex.ToString())
            .AddQueryParameter("limit", pageSize.ToString());

        return await AuthedExecuteAsync<BaseItemDtoQueryResult>(req);
    }

    public async IAsyncEnumerable<BaseItemDto> GetAllMoviesAsync(int pageSize = 200)
    {
        int start = 0;
        while (true)
        {
            var page = await GetMoviesPageAsync(start, pageSize);
            if (page.Items == null || page.Items.Count == 0) yield break;
            foreach (var item in page.Items) yield return item;
            start += page.Items.Count;
            if (!page.TotalRecordCount.HasValue || start >= page.TotalRecordCount.Value) yield break;
        }
    }

    private async Task<T> AuthedGetAsync<T>(string path)
        => await AuthedExecuteAsync<T>(new RestRequest(path, Method.Get));

    private async Task<T> AuthedExecuteAsync<T>(RestRequest req)
    {
        req.AddHeader("Authorization", $"MediaBrowser Token=\"{_token}\"");
        return (await _client.ExecuteAsync<T>(req)).Data;
    }

    public record AuthenticationResult(string AccessToken, UserDto User);
    public record UserDto(string Id, string Name);
    public record BaseItemDto(string Id, string Name);
    public record BaseItemDtoQueryResult(List<BaseItemDto> Items, int? TotalRecordCount);
}
