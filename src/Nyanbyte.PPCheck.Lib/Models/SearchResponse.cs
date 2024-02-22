namespace Nyanbyte.PPCheck.Lib.Models;

public record SearchResponse
{
    public int Total { get; set; }
    public bool HasError { get; set; }

    public IList<OffenderInformation> Data { get; set; } = new List<OffenderInformation>();
}
