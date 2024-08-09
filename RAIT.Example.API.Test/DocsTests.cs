using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class DocsTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _defaultClient = null!;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _defaultClient = _application.CreateDefaultClient();
    }

    private void PrepareEnv(IWebHostBuilder _)
    {
        _.UseEnvironment("Test");
    }

    [Test]
    public async Task GenerateDocForIncludeParameters()
    {
        var secret = "sec";
        var secret2 = "sec2";
        var request = new Model
        {
            ModelIncluded = new ModelIncluded
            {
                Secret = secret,
                Uri = new Uri("https://google.com/news")
            },
            ModelIncludeInLists = new List<ModelIncludeInList>
            {
                new()
                {
                    XSecret2 = secret2
                }
            }
        };
        await _defaultClient.Rait<RaitGetModelController>().Call(n => n.PingPost(request));

        var member = RaitDocumentationState.DocRootModelState.Members.Member.First(n=>n.Name.EndsWith("Secret"));
        Assert.That(member.Example, Is.EqualTo(secret));
        var member2 = RaitDocumentationState.DocRootModelState.Members.Member.First(n=>n.Name.EndsWith("XSecret2"));
        Assert.That(member2.Example, Is.EqualTo(secret2));
    }
}