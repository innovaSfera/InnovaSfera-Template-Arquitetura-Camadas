using System.Text.Json.Serialization;

namespace InnovaSfera.Template.Domain.Entities;

public class Character
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("alternate_names")]
    public List<string> AlternateNames { get; init; } = new();

    [JsonPropertyName("species")]
    public string Species { get; init; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; init; } = string.Empty;

    [JsonPropertyName("house")]
    public string House { get; init; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; init; }

    [JsonPropertyName("yearOfBirth")]
    public int? YearOfBirth { get; init; }

    [JsonPropertyName("wizard")]
    public bool Wizard { get; init; }

    [JsonPropertyName("ancestry")]
    public string Ancestry { get; init; } = string.Empty;

    [JsonPropertyName("eyeColour")]
    public string EyeColour { get; init; } = string.Empty;

    [JsonPropertyName("hairColour")]
    public string HairColour { get; init; } = string.Empty;

    [JsonPropertyName("wand")]
    public Wand Wand { get; init; } = new();

    [JsonPropertyName("patronus")]
    public string Patronus { get; init; } = string.Empty;

    [JsonPropertyName("hogwartsStudent")]
    public bool HogwartsStudent { get; init; }

    [JsonPropertyName("hogwartsStaff")]
    public bool HogwartsStaff { get; init; }

    [JsonPropertyName("actor")]
    public string Actor { get; init; } = string.Empty;

    [JsonPropertyName("alternate_actors")]
    public List<string> AlternateActors { get; init; } = new();

    [JsonPropertyName("alive")]
    public bool Alive { get; init; }

    [JsonPropertyName("image")]
    public string Image { get; init; } = string.Empty;
}

public sealed record Wand
{
    [JsonPropertyName("wood")]
    public string Wood { get; init; } = string.Empty;

    [JsonPropertyName("core")]
    public string Core { get; init; } = string.Empty;

    [JsonPropertyName("length")]
    public decimal? Length { get; init; }
}