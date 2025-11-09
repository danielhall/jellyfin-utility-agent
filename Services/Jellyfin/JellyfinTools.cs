using System.ComponentModel;

internal static class JellyfinTools
{
    [Description("Provides a full list of movies available on the Jellyfin server.")]
    internal static async Task<List<string>> GetAllMovies()
    {
        Console.WriteLine($"Retrieving all movies from Jellyfin server: {Environment.GetEnvironmentVariable("JELLYFIN_URL")}");

        var jf = new JellyfinClient(Environment.GetEnvironmentVariable("JELLYFIN_URL") ?? throw new InvalidOperationException("JELLYFIN_URL is not set."));
        await jf.LoginAsync(Environment.GetEnvironmentVariable("JELLYFIN_USERNAME") ?? throw new InvalidOperationException("JELLYFIN_USERNAME is not set."),
                            Environment.GetEnvironmentVariable("JELLYFIN_PASSWORD") ?? throw new InvalidOperationException("JELLYFIN_PASSWORD is not set."));

        return await jf.GetAllMoviesAsync().Select(m => m.Name).ToListAsync();
    }
}