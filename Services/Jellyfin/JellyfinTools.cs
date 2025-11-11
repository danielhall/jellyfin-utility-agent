using System.ComponentModel;

/// <summary>
/// AI Agent tools for interacting with Jellyfin media server
/// </summary>
internal static class JellyfinTools
{
    private static JellyfinClient? _client;

    internal static void Initialize(JellyfinClient client)
    {
        _client = client;
    }

    private static JellyfinClient GetClient()
        => _client ?? throw new InvalidOperationException("JellyfinClient not initialized. Call Initialize() first.");

    [Description("Get a complete list of all movies available on the Jellyfin server.")]
    internal static async Task<List<string>> GetAllMoviesAsync()
    {
        var client = GetClient();
        return await client.GetAllMoviesAsync().Select(m => m.Name).ToListAsync();
    }

    [Description("Search for movies, TV shows, or other media by name. Optionally filter by media type (Movie, Series, Episode) and genre.")]
    internal static async Task<string> SearchLibraryAsync(
        [Description("The search query to find media items")] string query,
        [Description("Optional media type filter: Movie, Series, Episode, etc.")] string? mediaType = null,
        [Description("Optional genre filter")] string? genre = null)
    {
        var client = GetClient();
        var results = await client.SearchLibraryAsync(query, mediaType, genre);
        
        if (results.Count == 0)
            return "No results found.";

        var output = new System.Text.StringBuilder();
        output.AppendLine($"Found {results.Count} result(s):\n");

        foreach (var item in results)
        {
            output.AppendLine($"📺 {item.Name}");
            if (item.ProductionYear.HasValue)
                output.AppendLine($"   Year: {item.ProductionYear}");
            if (item.Genres?.Any() == true)
                output.AppendLine($"   Genres: {string.Join(", ", item.Genres)}");
            if (item.CommunityRating.HasValue)
                output.AppendLine($"   Rating: {item.CommunityRating:F1}/10");
            if (!string.IsNullOrEmpty(item.Overview))
                output.AppendLine($"   Overview: {item.Overview[..Math.Min(150, item.Overview.Length)]}...");
            output.AppendLine();
        }

        return output.ToString();
    }

    [Description("Get all available genres in the Jellyfin library. Optionally filter by media type (Movie, Series, etc.).")]
    internal static async Task<List<string>> GetAllGenresAsync(
        [Description("Optional media type to filter genres: Movie, Series, etc.")] string? mediaType = null)
    {
        var client = GetClient();
        var genres = await client.GetAllGenresAsync(mediaType);
        return genres.Select(g => g.Name).OrderBy(n => n).ToList();
    }

    [Description("Get movies filtered by a specific genre, sorted by rating.")]
    internal static async Task<string> GetMoviesByGenreAsync(
        [Description("The genre to filter by (e.g., Horror, Comedy, Action)")] string genre,
        [Description("Maximum number of results to return")] int limit = 20)
    {
        var client = GetClient();
        var movies = await client.GetMoviesByGenreAsync(genre, limit);
        
        if (movies.Count == 0)
            return $"No movies found in genre: {genre}";

        var output = new System.Text.StringBuilder();
        output.AppendLine($"🎬 Top {movies.Count} {genre} movies:\n");

        foreach (var movie in movies)
        {
            output.AppendLine($"• {movie.Name}");
            if (movie.ProductionYear.HasValue)
                output.Append($" ({movie.ProductionYear})");
            if (movie.CommunityRating.HasValue)
                output.Append($" - {movie.CommunityRating:F1}");
            output.AppendLine();
            
            if (!string.IsNullOrEmpty(movie.Overview))
                output.AppendLine($"  {movie.Overview[..Math.Min(120, movie.Overview.Length)]}...\n");
        }

        return output.ToString();
    }

    [Description("Get recently added movies or TV shows.")]
    internal static async Task<string> GetRecentlyAddedAsync(
        [Description("Optional media type filter: Movie, Series, etc.")] string? mediaType = null,
        [Description("Maximum number of results")] int limit = 10)
    {
        var client = GetClient();
        var items = await client.GetRecentlyAddedAsync(mediaType, limit);
        
        if (items.Count == 0)
            return "No recently added items found.";

        var output = new System.Text.StringBuilder();
        output.AppendLine($"Recently added ({items.Count}):\n");

        foreach (var item in items)
        {
            output.AppendLine($"• {item.Name}");
            if (item.ProductionYear.HasValue)
                output.Append($" ({item.ProductionYear})");
            if (item.Genres?.Any() == true)
                output.Append($" - {string.Join(", ", item.Genres.Take(3))}");
            output.AppendLine();
        }

        return output.ToString();
    }

    [Description("Get detailed information about a specific movie or show by searching for it.")]
    internal static async Task<string> GetItemDetailsAsync(
        [Description("Name of the movie or show to get details for")] string itemName)
    {
        var client = GetClient();
        var searchResults = await client.SearchLibraryAsync(itemName, pageSize: 1);
        
        if (searchResults.Count == 0)
            return $"Could not find item: {itemName}";

        var item = searchResults[0];
        var output = new System.Text.StringBuilder();
        
        output.AppendLine($"{item.Name}");
        output.AppendLine();
        
        if (item.ProductionYear.HasValue)
            output.AppendLine($"Year: {item.ProductionYear}");
        
        if (item.Genres?.Any() == true)
            output.AppendLine($"Genres: {string.Join(", ", item.Genres)}");
        
        if (item.CommunityRating.HasValue)
            output.AppendLine($"Community Rating: {item.CommunityRating:F1}/10");
        
        if (!string.IsNullOrEmpty(item.OfficialRating))
            output.AppendLine($"Rating: {item.OfficialRating}");
        
        if (item.RunTimeTicks.HasValue)
        {
            var runtime = TimeSpan.FromTicks(item.RunTimeTicks.Value);
            output.AppendLine($"Runtime: {runtime.Hours}h {runtime.Minutes}m");
        }
        
        if (!string.IsNullOrEmpty(item.Overview))
        {
            output.AppendLine();
            output.AppendLine("Overview:");
            output.AppendLine(item.Overview);
        }

        return output.ToString();
    }

    [Description("Get the user's favorite movies and shows.")]
    internal static async Task<string> GetFavoritesAsync(
        [Description("Optional media type filter: Movie, Series, etc.")] string? mediaType = null)
    {
        var client = GetClient();
        var favorites = await client.GetFavoritesAsync(mediaType);
        
        if (favorites.Count == 0)
            return "No favorites found.";

        var output = new System.Text.StringBuilder();
        output.AppendLine($"Favorites ({favorites.Count}):\n");

        foreach (var item in favorites)
        {
            output.AppendLine($"• {item.Name}");
            if (item.ProductionYear.HasValue)
                output.Append($" ({item.ProductionYear})");
            if (item.CommunityRating.HasValue)
                output.Append($" - {item.CommunityRating:F1}");
            output.AppendLine();
        }

        return output.ToString();
    }

    [Description("Get movies or shows from a specific year.")]
    internal static async Task<string> GetItemsByYearAsync(
        [Description("The production year")] int year,
        [Description("Optional media type filter: Movie, Series, etc.")] string? mediaType = null)
    {
        var client = GetClient();
        var items = await client.GetItemsByYearAsync(year, mediaType);
        
        if (items.Count == 0)
            return $"No items found from {year}.";

        var output = new System.Text.StringBuilder();
        output.AppendLine($"Items from {year} ({items.Count}):\n");

        foreach (var item in items.Take(20))
        {
            output.AppendLine($"• {item.Name}");
            if (item.Genres?.Any() == true)
                output.Append($"  {string.Join(", ", item.Genres.Take(3))}");
            if (item.CommunityRating.HasValue)
                output.Append($" - {item.CommunityRating:F1}");
            output.AppendLine();
        }

        if (items.Count > 20)
            output.AppendLine($"\n... and {items.Count - 20} more");

        return output.ToString();
    }
}