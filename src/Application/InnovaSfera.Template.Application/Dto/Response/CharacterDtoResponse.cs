namespace InnovaSfera.Template.Application.Dto.Response;

public class CharacterDtoResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> AlternateNames { get; set; } = new();
    public string Species { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public int? YearOfBirth { get; set; }
    public bool Wizard { get; set; }
    public string Ancestry { get; set; } = string.Empty;
    public string EyeColour { get; set; } = string.Empty;
    public string HairColour { get; set; } = string.Empty;
    public WandDtoResponse Wand { get; set; } = new();
    public string Patronus { get; set; } = string.Empty;
    public bool HogwartsStudent { get; set; }
    public bool HogwartsStaff { get; set; }
    public string Actor { get; set; } = string.Empty;
    public List<string> AlternateActors { get; set; } = new();
    public bool Alive { get; set; }
    public string Image { get; set; } = string.Empty;
}

public class WandDtoResponse
{
    public string Wood { get; set; } = string.Empty;
    public string Core { get; set; } = string.Empty;
    public decimal? Length { get; set; }
}
