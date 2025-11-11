using Moq;
using JellyfinAgent.Library.Models;

namespace JellyfinAgent.Tests;

[TestClass]
public class JellyfinToolsTests
{
    private Mock<IJellyfinClient> _mockClient = null!;
    private JellyfinTools _tools = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockClient = new Mock<IJellyfinClient>();
        _tools = new JellyfinTools(_mockClient.Object);
    }

    [TestMethod]
    public async Task GetAllMoviesAsync_ReturnsMovieNames()
    {
        // Arrange
        var movies = new List<BaseItemDto>
        {
            new("1", "The Shawshank Redemption"),
            new("2", "The Godfather"),
            new("3", "The Dark Knight")
        };

        var asyncEnumerable = ToAsyncEnumerable(movies);
        _mockClient.Setup(c => c.GetAllMoviesAsync(It.IsAny<int>()))
            .Returns(asyncEnumerable);

        // Act
        var result = await _tools.GetAllMoviesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount<string>(3, result);
        Assert.AreEqual("The Shawshank Redemption", result[0]);
        Assert.AreEqual("The Godfather", result[1]);
        Assert.AreEqual("The Dark Knight", result[2]);
    }

    [TestMethod]
    public async Task SearchLibraryAsync_WithResults_ReturnsFormattedString()
    {
        // Arrange
        var searchResults = new List<BaseItemDto>
        {
            new("1", "Inception", 
                Overview: "A thief who steals corporate secrets through dream-sharing technology",
                Genres: new List<string> { "Action", "Sci-Fi" },
                CommunityRating: 8.8f,
                ProductionYear: 2010)
        };

        _mockClient.Setup(c => c.SearchLibraryAsync("Inception", null, null, 100))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _tools.SearchLibraryAsync("Inception");

        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "Found 1 result(s)");
        StringAssert.Contains(result, "Inception");
        StringAssert.Contains(result, "2010");
        StringAssert.Contains(result, "Action, Sci-Fi");
        StringAssert.Contains(result, "8.8");
    }

    [TestMethod]
    public async Task SearchLibraryAsync_NoResults_ReturnsNoResultsMessage()
    {
        // Arrange
        _mockClient.Setup(c => c.SearchLibraryAsync("NonExistentMovie", null, null, 100))
            .ReturnsAsync(new List<BaseItemDto>());

        // Act
        var result = await _tools.SearchLibraryAsync("NonExistentMovie");

        // Assert
        Assert.AreEqual("No results found.", result);
    }

    [TestMethod]
    public async Task GetAllGenresAsync_ReturnsOrderedGenreNames()
    {
        // Arrange
        var genres = new List<GenreDto>
        {
            new("Horror", "1"),
            new("Action", "2"),
            new("Comedy", "3")
        };

        _mockClient.Setup(c => c.GetAllGenresAsync(null))
            .ReturnsAsync(genres);

        // Act
        var result = await _tools.GetAllGenresAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount<string>(3, result);
        Assert.AreEqual("Action", result[0]);
        Assert.AreEqual("Comedy", result[1]);
        Assert.AreEqual("Horror", result[2]);
    }

    [TestMethod]
    public async Task GetMoviesByGenreAsync_WithResults_ReturnsFormattedList()
    {
        // Arrange
        var horrorMovies = new List<BaseItemDto>
        {
            new("1", "The Shining", 
                ProductionYear: 1980,
                CommunityRating: 8.4f,
                Overview: "A family heads to an isolated hotel for the winter where a sinister presence influences the father into violence"),
            new("2", "A Nightmare on Elm Street",
                ProductionYear: 1984,
                CommunityRating: 7.5f,
                Overview: "Teenagers in a small town are hunted by a supernatural killer in their dreams")
        };

        _mockClient.Setup(c => c.GetMoviesByGenreAsync("Horror", 20))
            .ReturnsAsync(horrorMovies);

        // Act
        var result = await _tools.GetMoviesByGenreAsync("Horror");

        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "Horror movies");
        StringAssert.Contains(result, "The Shining");
        StringAssert.Contains(result, "1980");
        StringAssert.Contains(result, "8.4");
        StringAssert.Contains(result, "A Nightmare on Elm Street");
    }

    [TestMethod]
    public async Task GetMoviesByGenreAsync_NoResults_ReturnsNoMoviesMessage()
    {
        // Arrange
        _mockClient.Setup(c => c.GetMoviesByGenreAsync("Documentary", 20))
            .ReturnsAsync(new List<BaseItemDto>());

        // Act
        var result = await _tools.GetMoviesByGenreAsync("Documentary");

        // Assert
        Assert.AreEqual("No movies found in genre: Documentary", result);
    }

    [TestMethod]
    public async Task GetRecentlyAddedAsync_ReturnsFormattedList()
    {
        // Arrange
        var recentItems = new List<BaseItemDto>
        {
            new("1", "Dune: Part Two",
                ProductionYear: 2024,
                Genres: new List<string> { "Sci-Fi", "Adventure" }),
            new("2", "Oppenheimer",
                ProductionYear: 2023,
                Genres: new List<string> { "Biography", "Drama" })
        };

        _mockClient.Setup(c => c.GetRecentlyAddedAsync(null, 10))
            .ReturnsAsync(recentItems);

        // Act
        var result = await _tools.GetRecentlyAddedAsync();

        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "Recently added");
        StringAssert.Contains(result, "Dune: Part Two");
        StringAssert.Contains(result, "2024");
        StringAssert.Contains(result, "Oppenheimer");
    }

    [TestMethod]
    public async Task GetItemDetailsAsync_WithValidItem_ReturnsDetailedInfo()
    {
        // Arrange
        var searchResults = new List<BaseItemDto>
        {
            new("1", "Pulp Fiction",
                Overview: "The lives of two mob hitmen, a boxer, a gangster and his wife intertwine in four tales of violence and redemption.",
                Genres: new List<string> { "Crime", "Drama" },
                CommunityRating: 8.9f,
                OfficialRating: "R",
                ProductionYear: 1994,
                RunTimeTicks: 92400000000L) // 154 minutes = 2h 34m (154 * 60 * 10,000,000 ticks)
        };

        _mockClient.Setup(c => c.SearchLibraryAsync("Pulp Fiction", null, null, 1))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _tools.GetItemDetailsAsync("Pulp Fiction");

        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "Pulp Fiction");
        StringAssert.Contains(result, "1994");
        StringAssert.Contains(result, "Crime, Drama");
        StringAssert.Contains(result, "8.9");
        StringAssert.Contains(result, "R");
        StringAssert.Contains(result, "2h 34m");
        StringAssert.Contains(result, "The lives of two mob hitmen");
    }

    [TestMethod]
    public async Task GetItemDetailsAsync_ItemNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        _mockClient.Setup(c => c.SearchLibraryAsync("NonExistent", null, null, 1))
            .ReturnsAsync(new List<BaseItemDto>());

        // Act
        var result = await _tools.GetItemDetailsAsync("NonExistent");

        // Assert
        Assert.AreEqual("Could not find item: NonExistent", result);
    }

    [TestMethod]
    public async Task GetFavoritesAsync_ReturnsFavoritesList()
    {
        // Arrange
        var favorites = new List<BaseItemDto>
        {
            new("1", "The Matrix",
                ProductionYear: 1999,
                CommunityRating: 8.7f),
            new("2", "Interstellar",
                ProductionYear: 2014,
                CommunityRating: 8.6f)
        };

        _mockClient.Setup(c => c.GetFavoritesAsync(null))
            .ReturnsAsync(favorites);

        // Act
        var result = await _tools.GetFavoritesAsync();

        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "Favorites");
        StringAssert.Contains(result, "The Matrix");
        StringAssert.Contains(result, "Interstellar");
        StringAssert.Contains(result, "8.7");
    }

    [TestMethod]
    public async Task GetFavoritesAsync_NoFavorites_ReturnsNoFavoritesMessage()
    {
        // Arrange
        _mockClient.Setup(c => c.GetFavoritesAsync(null))
            .ReturnsAsync(new List<BaseItemDto>());

        // Act
        var result = await _tools.GetFavoritesAsync();

        // Assert
        Assert.AreEqual("No favorites found.", result);
    }

    [TestMethod]
    public async Task GetItemsByYearAsync_ReturnsItemsFromYear()
    {
        // Arrange
        var items = new List<BaseItemDto>
        {
            new("1", "The Shawshank Redemption",
                Genres: new List<string> { "Drama" },
                CommunityRating: 9.3f),
            new("2", "Pulp Fiction",
                Genres: new List<string> { "Crime", "Drama" },
                CommunityRating: 8.9f),
            new("3", "Forrest Gump",
                Genres: new List<string> { "Drama", "Romance" },
                CommunityRating: 8.8f)
        };

        _mockClient.Setup(c => c.GetItemsByYearAsync(1994, null))
            .ReturnsAsync(items);

        // Act
        var result = await _tools.GetItemsByYearAsync(1994);

        // Assert
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "Items from 1994");
        StringAssert.Contains(result, "The Shawshank Redemption");
        StringAssert.Contains(result, "Pulp Fiction");
        StringAssert.Contains(result, "Forrest Gump");
    }

    [TestMethod]
    public async Task GetItemsByYearAsync_NoItemsFound_ReturnsNoItemsMessage()
    {
        // Arrange
        _mockClient.Setup(c => c.GetItemsByYearAsync(1920, null))
            .ReturnsAsync(new List<BaseItemDto>());

        // Act
        var result = await _tools.GetItemsByYearAsync(1920);

        // Assert
        Assert.AreEqual("No items found from 1920.", result);
    }

    [TestMethod]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new JellyfinTools(null!));
        Assert.IsNotNull(exception);
    }

    // Helper method to convert List to IAsyncEnumerable
    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
