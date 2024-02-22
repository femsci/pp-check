namespace Nyanbyte.PPCheck.Lib.Models;

public record SearchResponse
{
    public int Total { get; set; }
    public bool HasError { get; set; }

    public ICollection<OffenderInformation> Data { get; set; } = new List<OffenderInformation>();
}
