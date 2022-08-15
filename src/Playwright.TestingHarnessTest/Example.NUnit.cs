using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

using TestAttribute = Microsoft.Playwright.NUnit.TestAttribute;

namespace Playwright.TestingHarnessTest.NUnit;

public class ExampleNUnit : PageTest
{
    [Test]
    public async Task TestTitle()
    {
        await Page.PauseAsync();
        await Page.GotoAsync("about:blank");
        Assert.AreEqual(122, await Page.EvaluateAsync<int>("() => 11 * 11"));
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions()
        {
        };
    }
}
