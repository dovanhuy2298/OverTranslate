using System.Globalization;
using OverTranslate.Services;
using Xunit;

namespace OverTranslate.Tests;

/// <summary>
/// The two decisions LocalizationService makes on its own: which language a first run starts in,
/// and which stored values it is willing to hand out afterwards.
/// </summary>
public class LocalizationServiceTests
{
    /// <summary>
    /// A first run starts in Vietnamese whatever the machine is set to.
    /// </summary>
    /// <remarks>
    /// The cultures are still enumerated, and that is the point: this used to follow the display
    /// language, so the regression to guard against is someone reinstating one of these mappings
    /// and leaving a Vietnamese build that opens in Chinese on a Chinese machine. The Chinese tags
    /// are over-represented because they are the ones that carried the most logic — a
    /// script-versus-region split — and so are the likeliest to come back by accident.
    /// </remarks>
    [Theory]
    [InlineData("zh-TW")]
    [InlineData("zh-HK")]
    [InlineData("zh-MO")]
    [InlineData("zh-Hant")]
    [InlineData("zh-Hant-TW")]
    [InlineData("zh-CN")]
    [InlineData("zh-SG")]
    [InlineData("zh-Hans")]
    [InlineData("zh")]
    [InlineData("ja-JP")]
    [InlineData("ko-KR")]
    [InlineData("vi-VN")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("")]
    public void ResolveSystemDefault_IsAlwaysVietnamese(string culture)
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            Assert.Equal(LocalizationService.Vietnamese, LocalizationService.ResolveSystemDefault());
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Theory]
    [InlineData(LocalizationService.TraditionalChinese)]
    [InlineData(LocalizationService.SimplifiedChinese)]
    [InlineData(LocalizationService.English)]
    [InlineData(LocalizationService.Japanese)]
    [InlineData(LocalizationService.Korean)]
    [InlineData(LocalizationService.Vietnamese)]
    public void IsSupported_AcceptsEveryLanguageOnOffer(string language) =>
        Assert.True(LocalizationService.IsSupported(language));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("kl")]
    [InlineData("zh-Hans-CN")]
    public void IsSupported_RejectsAnythingElse(string? language) =>
        Assert.False(LocalizationService.IsSupported(language));

    /// <summary>
    /// A language offered in settings that has no dictionary behind it lands the user on Traditional
    /// Chinese with no explanation — Apply falls back rather than throwing, which is right for a
    /// hand-edited settings file and useless as a way of noticing this.
    /// </summary>
    [Fact]
    public void EveryLanguageOnOfferHasADictionaryAndIsSupported()
    {
        foreach (var option in LocalizationService.Options)
        {
            Assert.True(
                LocalizationService.IsSupported(option.Code),
                $"{option.Code} is offered in settings but has no dictionary");

            var file = Path.Combine(
                StringsParityTests.ProjectDirectory(), "Resources", $"Strings.{option.Code}.xaml");

            Assert.True(File.Exists(file), $"{option.Code} is offered in settings but {file} is missing");
        }
    }

    /// <summary>
    /// The name in the picker is the one string never translated — someone who has landed in a
    /// language they cannot read finds their way out by recognising their own language's name.
    /// </summary>
    [Fact]
    public void EveryLanguageIsNamedInItself()
    {
        Assert.Equal(
            new[] { "繁體中文", "简体中文", "English", "日本語", "한국어", "Tiếng Việt" },
            LocalizationService.Options.Select(o => o.Display));
    }
}
