namespace Nyanbyte.PPCheck.Lib.Models;

public record Response
{
    public int Total { get; set; }
    public bool HasError { get; set; }

    public ICollection<OffenderInformation> Data { get; set; } = new List<OffenderInformation>();
}
