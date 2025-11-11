/// <summary>
/// Data transfer objects for Jellyfin API responses
/// </summary>
namespace JellyfinAgent.Library.Models;

public record AuthenticationResult(string AccessToken, UserDto User);

public record UserDto(string Id, string Name);

public record BaseItemDto(
    string Id, 
    string Name, 
    string? Overview = null,
    List<string>? Genres = null,
    float? CommunityRating = null,
    string? OfficialRating = null,
    int? ProductionYear = null,
    long? RunTimeTicks = null,
    string? Type = null
);

public record GenreDto(string Name, string Id);

public record BaseItemDtoQueryResult(List<BaseItemDto> Items, int? TotalRecordCount);

public record GenreDtoQueryResult(List<GenreDto> Items, int? TotalRecordCount);
