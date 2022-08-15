using System.Runtime.Serialization;

namespace Microsoft.Playwright.TestAdapter
{
    public enum AssetMode
    {
        [EnumMember(Value = "on")]
        On,
        [EnumMember(Value = "off")]
        Off,
        [EnumMember(Value = "on-first-retry")]
        OnFirstRetry,
        [EnumMember(Value = "retain-on-failure")]
        RetainOnFailure,
    }
}
