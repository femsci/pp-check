namespace Nyanbyte.PPCheck.Lib.Models;

public record SearchRequest
{
    public bool Advanced { get; set; } = false;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Nationality { get; set; }
    public string? CityOfBirth { get; set; }
    public string? CityOfResidence { get; set; }

    public int Page { get; set; } = 0;
    public bool ShowYearCal => false;
    public string SortColumn => "LastName";
    public string SortOrder => "asc";
    public int Take => 5;
}
