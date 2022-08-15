using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestMethodAttribute = Microsoft.Playwright.MSTest.TestMethodAttribute;

namespace Playwright.TestingHarnessTest.MSTest;

[TestClass]
public class ExampleMSTest : PageTest
{
    [TestMethod]
    public async Task Test()
    {
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
