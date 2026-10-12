using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuickMail.Models;
using QuickMail.Services;
using QuickMail.Theming;
using Xunit;

namespace QuickMail.Tests;

/// <summary>
/// WCAG policy for the MailBoXX surface tokens, in every built-in theme. Several of these
/// tokens are translucent (sidebar tint over a blurred picture, row washes, the reading
/// surface over the scenic background), so each check composites the token over its real
/// underlay first — and where the underlay is a photograph of unknown brightness, over
/// both a white and a black worst case.
/// </summary>
public class MailBoXXThemeTests
{
    private static readonly (byte R, byte G, byte B) White = (255, 255, 255);
    private static readonly (byte R, byte G, byte B) Black = (0, 0, 0);

    private static readonly string[] Ids = { "mailboxx-light", "mailboxx-dark", "parchment", "dark", "ember", "fjord", "heather" };

    public static IEnumerable<object[]> BuiltInIds() => Ids.Select(id => new object[] { id });

    private static ThemeDefinition Resolved(string id)
    {
        var store = new ThemeStore(new ProfileContext(Path.Combine(Path.GetTempPath(), $"MBX-ThemeTests-{Guid.NewGuid():N}")));
        var builtIns = store.LoadBuiltIns();
        var light = builtIns.First(t => t.Id == "parchment");
        var dark = builtIns.First(t => t.Id == "dark");
        var theme = builtIns.First(t => t.Id == id);
        return theme.ResolveAgainst(theme.Base == "dark" ? dark : light);
    }

    [Fact]
    public void SystemTheme_FollowsTheMailBoXXBuiltIns()
    {
        var ids = new ThemeStore(new ProfileContext(Path.Combine(Path.GetTempPath(), $"MBX-ThemeTests-{Guid.NewGuid():N}")))
            .LoadBuiltIns().Select(t => t.Id).ToList();
        Assert.Contains(ThemeService.SystemLightThemeId, ids);
        Assert.Contains(ThemeService.SystemDarkThemeId, ids);
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void Sidebar_TextStaysReadable_OverAnyPictureBrightness(string id)
    {
        var t = Resolved(id);
        foreach (var picture in new[] { White, Black })
        {
            // The picture covers sidebarBackground completely; only the tint stands between it and the text.
            var sidebar = Over(t.ColorOf("sidebarTint"), picture);
            AssertRatio(t, "sidebarText on sidebar", Rgb(t.ColorOf("sidebarText")), sidebar, 4.5);
            AssertRatio(t, "sidebarTextSecondary on sidebar", Rgb(t.ColorOf("sidebarTextSecondary")), sidebar, 4.5);
            AssertRatio(t, "sidebarFocus on sidebar", Rgb(t.ColorOf("sidebarFocus")), sidebar, 3.0);

            var selected = Over(t.ColorOf("sidebarSelection"), sidebar);
            AssertRatio(t, "sidebarText on selection", Rgb(t.ColorOf("sidebarText")), selected, 4.5);
            var hovered = Over(t.ColorOf("sidebarHover"), sidebar);
            AssertRatio(t, "sidebarText on hover", Rgb(t.ColorOf("sidebarText")), hovered, 4.5);
        }
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void MessageRows_TextAndIndicatorsMeetPolicy(string id)
    {
        var t = Resolved(id);
        // Rows are glass too: the row tint over the ~90% list surface over the picture.
        foreach (var picture in new[] { White, Black })
        {
            var list = Over(t.ColorOf("listSurface"), picture);
            foreach (var rowKey in new[] { "rowUnreadBackground", "rowReadBackground" })
            {
                var row = Over(t.ColorOf(rowKey), list);
                foreach (var wash in new[] { (string?)null, "rowHoverWash", "rowSelectedWash" })
                {
                    var bg = wash is null ? row : Over(t.ColorOf(wash), row);
                    var where = wash is null ? rowKey : $"{wash} over {rowKey}";
                    AssertRatio(t, $"textPrimary on {where}", Rgb(t.ColorOf("textPrimary")), bg, 4.5);
                    AssertRatio(t, $"textSecondary on {where}", Rgb(t.ColorOf("textSecondary")), bg, 4.5);
                }
                // The selected-row marker is the selection's non-colour-only cue: 3:1 non-text.
                AssertRatio(t, $"rowSelectedMarker on {rowKey}", Rgb(t.ColorOf("rowSelectedMarker")), row, 3.0);
            }

            // Envelopes are state indicators (non-text, 3:1) on their own row state.
            AssertRatio(t, "envelopeUnread on rowUnreadBackground",
                Rgb(t.ColorOf("envelopeUnread")), Over(t.ColorOf("rowUnreadBackground"), list), 3.0);
            AssertRatio(t, "envelopeRead on rowReadBackground",
                Rgb(t.ColorOf("envelopeRead")), Over(t.ColorOf("rowReadBackground"), list), 3.0);
        }
        AssertRatio(t, "avatarText on avatarBackground",
            Rgb(t.ColorOf("avatarText")), Rgb(t.ColorOf("avatarBackground")), 4.5);
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void ReadingSurface_IsAboutNinetyPercentOpaque_AndReadableOverAnyPicture(string id)
    {
        var t = Resolved(id);
        var (a, _, _, _) = ThemeDefinition.HexToArgb(t.ColorOf("readingSurface"));
        Assert.InRange(a / 255.0, 0.85, 0.95);

        foreach (var picture in new[] { White, Black })
        {
            var surface = Over(t.ColorOf("readingSurface"), picture);
            AssertRatio(t, "textPrimary on reading surface", Rgb(t.ColorOf("textPrimary")), surface, 4.5);
            AssertRatio(t, "textSecondary on reading surface", Rgb(t.ColorOf("textSecondary")), surface, 4.5);
        }
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void AccountBadgeLetters_AreReadable(string id)
    {
        var t = Resolved(id);
        foreach (var key in new[] { "providerOutlook", "providerGmail", "providerHotmail", "providerOther" })
            AssertRatio(t, $"providerGlyph on {key}", Rgb(t.ColorOf("providerGlyph")), Rgb(t.ColorOf(key)), 4.5);
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void SenderCircleLetters_AreReadableOnEveryColour(string id)
    {
        var t = Resolved(id);
        foreach (var key in new[] { "avatarBackground", "avatarAlt1", "avatarAlt2", "avatarAlt3", "avatarAlt4", "avatarAlt5" })
            AssertRatio(t, $"avatarText on {key}", Rgb(t.ColorOf("avatarText")), Rgb(t.ColorOf(key)), 4.5);
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void GlassLayers_HaveTheDesignedOpacity(string id)
    {
        var t = Resolved(id);
        // Message list body: ~90%, so the picture shows slightly where there are few rows.
        var (list, _, _, _) = ThemeDefinition.HexToArgb(t.ColorOf("listSurface"));
        Assert.InRange(list / 255.0, 0.85, 0.95);
        // Sidebar: blue glass over the blurred picture. Lower alphas let more picture through but
        // cannot keep white text at 4.5:1 over a bright picture (checked above).
        var (tint, _, _, _) = ThemeDefinition.HexToArgb(t.ColorOf("sidebarTint"));
        Assert.InRange(tint / 255.0, 0.5, 0.8);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static (byte R, byte G, byte B) Rgb(string hex)
    {
        var (_, r, g, b) = ThemeDefinition.HexToArgb(hex);
        return (r, g, b);
    }

    /// <summary>Source-over compositing of a (possibly translucent) hex color onto an opaque one.</summary>
    private static (byte R, byte G, byte B) Over(string hex, (byte R, byte G, byte B) under)
    {
        var (a, r, g, b) = ThemeDefinition.HexToArgb(hex);
        var alpha = a / 255.0;
        static byte Mix(double top, double bottom, double alpha) => (byte)Math.Round(top * alpha + bottom * (1 - alpha));
        return (Mix(r, under.R, alpha), Mix(g, under.G, alpha), Mix(b, under.B, alpha));
    }

    private static void AssertRatio(ThemeDefinition theme, string what,
        (byte R, byte G, byte B) fg, (byte R, byte G, byte B) bg, double minimum)
    {
        var ratio = Ratio(fg, bg);
        Assert.True(ratio >= minimum,
            $"{theme.Id}: {what} is {ratio:F2}:1 (fg #{fg.R:X2}{fg.G:X2}{fg.B:X2}, bg #{bg.R:X2}{bg.G:X2}{bg.B:X2}), needs {minimum}:1");
    }

    private static double Ratio((byte R, byte G, byte B) a, (byte R, byte G, byte B) b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        var (hi, lo) = la >= lb ? (la, lb) : (lb, la);
        return (hi + 0.05) / (lo + 0.05);
    }

    private static double Luminance((byte R, byte G, byte B) c) =>
        0.2126 * Linear(c.R) + 0.7152 * Linear(c.G) + 0.0722 * Linear(c.B);

    private static double Linear(byte channel)
    {
        var c = channel / 255.0;
        return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}
