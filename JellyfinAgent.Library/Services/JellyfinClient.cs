using RestSharp;
using JellyfinAgent.Library.Models;

public class JellyfinClient : IJellyfinClient
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
                                 string? deviceName = null,
                                 string? deviceId = null,
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

    public async Task<BaseItemDtoQueryResult> GetItemsPageAsync(string itemType = "Movie", int startIndex = 0, int pageSize = 100)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)               // required if you’re not using an API key
            .AddQueryParameter("includeItemTypes", itemType)
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
            var page = await GetItemsPageAsync("Movie", start, pageSize);
            if (page.Items == null || page.Items.Count == 0) yield break;
            foreach (var item in page.Items) yield return item;
            start += page.Items.Count;
            if (!page.TotalRecordCount.HasValue || start >= page.TotalRecordCount.Value) yield break;
        }
    }

    /// <summary>
    /// Search library items by query string, with optional filtering by media type and genre
    /// </summary>
    /// <param name="searchTerm">Search query to match against item names</param>
    /// <param name="mediaType">Optional media type filter (e.g., "Movie", "Series", "Episode")</param>
    /// <param name="genre">Optional genre filter</param>
    /// <param name="pageSize">Number of results to return (default 100)</param>
    public async Task<List<BaseItemDto>> SearchLibraryAsync(string searchTerm, string? mediaType = null, string? genre = null, int pageSize = 100)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)
            .AddQueryParameter("searchTerm", searchTerm)
            .AddQueryParameter("recursive", "true")
            .AddQueryParameter("limit", pageSize.ToString())
            .AddQueryParameter("fields", "Overview,Genres,CommunityRating,OfficialRating,ProductionYear,RunTimeTicks");

        if (!string.IsNullOrEmpty(mediaType))
            req.AddQueryParameter("includeItemTypes", mediaType);

        if (!string.IsNullOrEmpty(genre))
            req.AddQueryParameter("genres", genre);

        var result = await AuthedExecuteAsync<BaseItemDtoQueryResult>(req);
        return result.Items ?? new List<BaseItemDto>();
    }

    /// <summary>
    /// Get all available genres in the library
    /// </summary>
    /// <param name="mediaType">Optional media type to filter genres by (e.g., "Movie", "Series")</param>
    public async Task<List<GenreDto>> GetAllGenresAsync(string? mediaType = null)
    {
        var req = new RestRequest("/Genres", Method.Get)
            .AddQueryParameter("userId", _userId)
            .AddQueryParameter("enableTotalRecordCount", "false");

        if (!string.IsNullOrEmpty(mediaType))
            req.AddQueryParameter("includeItemTypes", mediaType);

        var result = await AuthedExecuteAsync<GenreDtoQueryResult>(req);
        return result.Items ?? new List<GenreDto>();
    }

    /// <summary>
    /// Get movies by genre
    /// </summary>
    /// <param name="genre">Genre name to filter by</param>
    /// <param name="limit">Maximum number of results</param>
    public async Task<List<BaseItemDto>> GetMoviesByGenreAsync(string genre, int limit = 50)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)
            .AddQueryParameter("includeItemTypes", "Movie")
            .AddQueryParameter("genres", genre)
            .AddQueryParameter("recursive", "true")
            .AddQueryParameter("limit", limit.ToString())
            .AddQueryParameter("fields", "Overview,Genres,CommunityRating,OfficialRating,ProductionYear,RunTimeTicks")
            .AddQueryParameter("sortBy", "CommunityRating,SortName")
            .AddQueryParameter("sortOrder", "Descending");

        var result = await AuthedExecuteAsync<BaseItemDtoQueryResult>(req);
        return result.Items ?? new List<BaseItemDto>();
    }

    /// <summary>
    /// Get recently added items
    /// </summary>
    /// <param name="mediaType">Optional media type filter (e.g., "Movie", "Series")</param>
    /// <param name="limit">Maximum number of results</param>
    public async Task<List<BaseItemDto>> GetRecentlyAddedAsync(string? mediaType = null, int limit = 20)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)
            .AddQueryParameter("recursive", "true")
            .AddQueryParameter("limit", limit.ToString())
            .AddQueryParameter("fields", "Overview,Genres,CommunityRating,OfficialRating,ProductionYear,RunTimeTicks")
            .AddQueryParameter("sortBy", "DateCreated")
            .AddQueryParameter("sortOrder", "Descending");

        if (!string.IsNullOrEmpty(mediaType))
            req.AddQueryParameter("includeItemTypes", mediaType);

        var result = await AuthedExecuteAsync<BaseItemDtoQueryResult>(req);
        return result.Items ?? new List<BaseItemDto>();
    }

    /// <summary>
    /// Get details about a specific item by ID
    /// </summary>
    /// <param name="itemId">The item ID</param>
    public async Task<BaseItemDto?> GetItemDetailsAsync(string itemId)
    {
        return await AuthedGetAsync<BaseItemDto>($"/Users/{_userId}/Items/{itemId}");
    }

    /// <summary>
    /// Get favorite items
    /// </summary>
    /// <param name="mediaType">Optional media type filter</param>
    public async Task<List<BaseItemDto>> GetFavoritesAsync(string? mediaType = null)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)
            .AddQueryParameter("recursive", "true")
            .AddQueryParameter("isFavorite", "true")
            .AddQueryParameter("fields", "Overview,Genres,CommunityRating,OfficialRating,ProductionYear,RunTimeTicks");

        if (!string.IsNullOrEmpty(mediaType))
            req.AddQueryParameter("includeItemTypes", mediaType);

        var result = await AuthedExecuteAsync<BaseItemDtoQueryResult>(req);
        return result.Items ?? new List<BaseItemDto>();
    }

    /// <summary>
    /// Get items by year
    /// </summary>
    /// <param name="year">Production year</param>
    /// <param name="mediaType">Optional media type filter</param>
    public async Task<List<BaseItemDto>> GetItemsByYearAsync(int year, string? mediaType = null)
    {
        var req = new RestRequest("/Items", Method.Get)
            .AddQueryParameter("userId", _userId)
            .AddQueryParameter("recursive", "true")
            .AddQueryParameter("years", year.ToString())
            .AddQueryParameter("fields", "Overview,Genres,CommunityRating,OfficialRating,ProductionYear,RunTimeTicks");

        if (!string.IsNullOrEmpty(mediaType))
            req.AddQueryParameter("includeItemTypes", mediaType);

        var result = await AuthedExecuteAsync<BaseItemDtoQueryResult>(req);
        return result.Items ?? new List<BaseItemDto>();
    }

    private async Task<T> AuthedGetAsync<T>(string path)
        => await AuthedExecuteAsync<T>(new RestRequest(path, Method.Get));

    private async Task<T> AuthedExecuteAsync<T>(RestRequest req)
    {
        req.AddHeader("Authorization", $"MediaBrowser Token=\"{_token}\"");
        var response = await _client.ExecuteAsync<T>(req);
        return response.Data ?? throw new Exception($"Request to {req.Resource} returned null data");
    }
}
