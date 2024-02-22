using System.Text.Json.Serialization;

namespace Nyanbyte.PPCheck.Lib.Models;

public record OffenderInformation
{
    public Guid PersonIdentityId { get; set; }
    public int AnnotationCount { get; set; }

    [JsonPropertyName("persones")]
    public IList<OffenderPersona> Personas { get; set; } = [];
}

public record OffenderPersona
{
    public Guid Id { get; set; }
    public Guid PersonIdentityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string CityOfBirth { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string? FathersName { get; set; }
    public string? MothersName { get; set; }
    public string CountryOfBirth { get; set; } = string.Empty;
    public string Nationalities { get; set; } = string.Empty;
    public string DwellingPlace { get; set; } = string.Empty;

    [JsonIgnore]
    public Sex OffenderSex => string.IsNullOrWhiteSpace(Sex) ? Models.Sex.Unknown : (Sex)Sex[0];
}
