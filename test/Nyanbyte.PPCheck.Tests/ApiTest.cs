using Nyanbyte.PPCheck.Lib;
using Nyanbyte.PPCheck.Lib.Models;

namespace Nyanbyte.PPCheck.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Meow()
    {
        ApiClient cli = new();

        SearchRequest req = new()
        {
            FirstName = "dariusz",
            LastName = "adamski"
        };

        var results = await cli.Search(req);
        Assert.Multiple(() =>
        {
            Assert.That(results.HasError, Is.False);
            Assert.That(results.Total, Is.GreaterThan(0));
        });

        var person = results.Data[0].Personas[0];
        Assert.Multiple(() =>
        {
            Assert.That(person.FirstName, Is.EqualTo("DARIUSZ"));
            Assert.That(person.LastName, Is.EqualTo("ADAMSKI"));
        });
    }
}
