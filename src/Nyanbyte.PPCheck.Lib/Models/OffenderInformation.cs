using System.Text.Json.Serialization;

namespace Nyanbyte.PPCheck.Lib.Models;

public record OffenderInformation
{
    public Guid PersonIdentityId { get; set; }
    public int AnnotationCount { get; set; }

    [JsonPropertyName("persones")]
    public ICollection<OffenderPersona> Personas { get; set; } = new List<OffenderPersona>();
}

public record OffenderPersona
{
    public Guid Id { get; set; }
    public Guid PersonIdentityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string LastName { get; set; }
    public string CityOfBirth { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string FamilyName { get; set; }
    public string? FathersName { get; set; }
    public string? MothersName { get; set; }
    public Sex Sex { get; set; }
    public string CountryOfBirth { get; set; }
    public string Nationalities { get; set; }
    public string DwellingPlace { get; set; }
}
