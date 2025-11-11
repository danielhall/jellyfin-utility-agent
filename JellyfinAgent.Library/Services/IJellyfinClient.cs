using JellyfinAgent.Library.Models;

/// <summary>
/// Interface for Jellyfin client operations
/// </summary>
public interface IJellyfinClient
{
    /// <summary>
    /// Authenticate with the Jellyfin server
    /// </summary>
    Task LoginAsync(string username, string password,
                   string appName = "ai.danieljh.uk",
                   string? deviceName = null,
                   string? deviceId = null,
                   string appVersion = "1.0.0");

    /// <summary>
    /// Get a page of items from the Jellyfin library
    /// </summary>
    Task<BaseItemDtoQueryResult> GetItemsPageAsync(string itemType = "Movie", int startIndex = 0, int pageSize = 100);

    /// <summary>
    /// Get all movies as an async enumerable
    /// </summary>
    IAsyncEnumerable<BaseItemDto> GetAllMoviesAsync(int pageSize = 200);

    /// <summary>
    /// Search library items by query string, with optional filtering
    /// </summary>
    Task<List<BaseItemDto>> SearchLibraryAsync(string searchTerm, string? mediaType = null, string? genre = null, int pageSize = 100);

    /// <summary>
    /// Get all available genres in the library
    /// </summary>
    Task<List<GenreDto>> GetAllGenresAsync(string? mediaType = null);

    /// <summary>
    /// Get movies by genre
    /// </summary>
    Task<List<BaseItemDto>> GetMoviesByGenreAsync(string genre, int limit = 50);

    /// <summary>
    /// Get recently added items
    /// </summary>
    Task<List<BaseItemDto>> GetRecentlyAddedAsync(string? mediaType = null, int limit = 20);

    /// <summary>
    /// Get details about a specific item by ID
    /// </summary>
    Task<BaseItemDto?> GetItemDetailsAsync(string itemId);

    /// <summary>
    /// Get favorite items
    /// </summary>
    Task<List<BaseItemDto>> GetFavoritesAsync(string? mediaType = null);

    /// <summary>
    /// Get items by year
    /// </summary>
    Task<List<BaseItemDto>> GetItemsByYearAsync(int year, string? mediaType = null);
}
