using System;
using System.Collections.Generic;

namespace QuickMail.Theming;

/// <summary>
/// The single source of truth for semantic theme token names.
///
/// Each color token exists in two spellings:
///   - the resource key published into Application.Current.Resources
///     (e.g. "Theme.WindowBackground"), consumed by XAML via DynamicResource
///     and by C# via SetResourceReference / TryFindResource; and
///   - the camelCase JSON name used in .quickmailtheme files
///     (e.g. "windowBackground").
///
/// <see cref="ColorTokens"/> maps camelCase JSON name → resource key and is the
/// authoritative list of the 45 color tokens. Typography and vision-assist keys
/// are resource-only (they come from the theme's typography block and user
/// settings, not the colors map).
/// </summary>
public static class ThemeKeys
{
    private const string Prefix = "Theme.";

    // ── Surfaces (7) ──────────────────────────────────────────────────────────
    public const string WindowBackground  = Prefix + "WindowBackground";
    public const string SurfaceBackground = Prefix + "SurfaceBackground";
    public const string ChromeBackground  = Prefix + "ChromeBackground";
    public const string InputBackground   = Prefix + "InputBackground";
    public const string Border            = Prefix + "Border";
    public const string BorderSubtle      = Prefix + "BorderSubtle";
    public const string InputBorder       = Prefix + "InputBorder";

    // ── Text (4) ──────────────────────────────────────────────────────────────
    public const string TextPrimary   = Prefix + "TextPrimary";
    public const string TextSecondary = Prefix + "TextSecondary";
    public const string TextDisabled  = Prefix + "TextDisabled";
    public const string TextOnAccent  = Prefix + "TextOnAccent";

    // ── Accent / interaction (7) ──────────────────────────────────────────────
    public const string Accent              = Prefix + "Accent";
    public const string AccentSubtle        = Prefix + "AccentSubtle";
    public const string Hyperlink           = Prefix + "Hyperlink";
    public const string SelectionBackground = Prefix + "SelectionBackground";
    public const string SelectionText       = Prefix + "SelectionText";
    public const string SelectionInactive   = Prefix + "SelectionInactive";
    public const string FocusIndicator      = Prefix + "FocusIndicator";

    // ── Status (8) ────────────────────────────────────────────────────────────
    public const string Error             = Prefix + "Error";
    public const string ErrorBackground   = Prefix + "ErrorBackground";
    public const string Warning           = Prefix + "Warning";
    public const string WarningBackground = Prefix + "WarningBackground";
    public const string Success           = Prefix + "Success";
    public const string SuccessBackground = Prefix + "SuccessBackground";
    public const string Info              = Prefix + "Info";
    public const string InfoBackground    = Prefix + "InfoBackground";

    // ── MailBoXX surfaces (30) ────────────────────────────────────────────────
    // The Windows 10 Mail-style shell: blue sidebar, tinted message rows with
    // envelope indicators, and the translucent reading surface over the scenic
    // main area. Alpha (#AARRGGBB) is meaningful on the tint/wash/surface tokens.
    public const string SidebarBackground    = Prefix + "SidebarBackground";
    public const string SidebarTint          = Prefix + "SidebarTint";
    public const string SidebarText          = Prefix + "SidebarText";
    public const string SidebarTextSecondary = Prefix + "SidebarTextSecondary";
    public const string SidebarSelection     = Prefix + "SidebarSelection";
    public const string SidebarHover         = Prefix + "SidebarHover";
    public const string SidebarFocus         = Prefix + "SidebarFocus";
    public const string RowUnreadBackground  = Prefix + "RowUnreadBackground";
    public const string RowReadBackground    = Prefix + "RowReadBackground";
    public const string RowHoverWash         = Prefix + "RowHoverWash";
    public const string RowSelectedWash      = Prefix + "RowSelectedWash";
    public const string RowSelectedMarker    = Prefix + "RowSelectedMarker";
    public const string EnvelopeUnread       = Prefix + "EnvelopeUnread";
    public const string EnvelopeUnreadGlyph  = Prefix + "EnvelopeUnreadGlyph";
    public const string EnvelopeRead         = Prefix + "EnvelopeRead";
    public const string AvatarBackground     = Prefix + "AvatarBackground";
    public const string AvatarText           = Prefix + "AvatarText";
    /// <summary>Sender circles take AvatarBackground or one of these, chosen per sender.</summary>
    public const string AvatarAlt1           = Prefix + "AvatarAlt1";
    public const string AvatarAlt2           = Prefix + "AvatarAlt2";
    public const string AvatarAlt3           = Prefix + "AvatarAlt3";
    public const string AvatarAlt4           = Prefix + "AvatarAlt4";
    public const string AvatarAlt5           = Prefix + "AvatarAlt5";
    public const string ReadingSurface       = Prefix + "ReadingSurface";
    /// <summary>The message list body (~90% opaque) over the window-wide picture.</summary>
    public const string ListSurface          = Prefix + "ListSurface";
    public const string MainAreaBackground   = Prefix + "MainAreaBackground";
    /// <summary>Account badges in the sidebar: MailBoXX's own coloured letters per provider.</summary>
    public const string ProviderOutlook      = Prefix + "ProviderOutlook";
    public const string ProviderGmail        = Prefix + "ProviderGmail";
    public const string ProviderHotmail      = Prefix + "ProviderHotmail";
    public const string ProviderOther        = Prefix + "ProviderOther";
    public const string ProviderGlyph        = Prefix + "ProviderGlyph";

    /// <summary>Opacity (double) for decorative imagery — the scenic background and
    /// contextual row images. 1 normally; 0 under High Contrast, where QuickMail has
    /// no visual opinion and only the system palette is drawn.</summary>
    public const string ImageryOpacity = Prefix + "ImageryOpacity";

    /// <summary>Which set of contextual row pictures suits the theme (string): "light" or "dark",
    /// from the theme's base. Dark themes get dark-toned pictures so light text stays readable.</summary>
    public const string ImagerySet = Prefix + "ImagerySet";

    // ── Typography (resource-only) ────────────────────────────────────────────
    public const string FontFamily     = Prefix + "FontFamily";
    public const string FontFamilyMono = Prefix + "FontFamilyMono";
    public const string FontSizeBase   = Prefix + "FontSizeBase";
    public const string FontSizeSmall  = Prefix + "FontSizeSmall";
    public const string FontSizeLarge  = Prefix + "FontSizeLarge";
    public const string FontSizeHeader = Prefix + "FontSizeHeader";

    // ── Vision assist (resource-only) ─────────────────────────────────────────
    public const string FocusThickness = Prefix + "FocusThickness";
    /// <summary>Always FocusThickness + 2: the contrasting halo drawn under the
    /// focus ring extends 1px beyond it on each side, guaranteeing the ring is
    /// visible on any fill (including accent-strength selection).</summary>
    public const string FocusHaloThickness = Prefix + "FocusHaloThickness";

    /// <summary>Message-list row padding (Thickness), from the density setting:
    /// comfortable 4,3 / compact 2,1. Padding only — never anything a screen
    /// reader can observe.</summary>
    public const string ListRowPadding = Prefix + "ListRowPadding";

    /// <summary>camelCase JSON name → application resource key, for all 45 color tokens
    /// (QuickMail's 26 plus the 19 MailBoXX surface tokens).</summary>
    public static readonly IReadOnlyDictionary<string, string> ColorTokens =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["windowBackground"]    = WindowBackground,
            ["surfaceBackground"]   = SurfaceBackground,
            ["chromeBackground"]    = ChromeBackground,
            ["inputBackground"]     = InputBackground,
            ["border"]              = Border,
            ["borderSubtle"]        = BorderSubtle,
            ["inputBorder"]         = InputBorder,
            ["textPrimary"]         = TextPrimary,
            ["textSecondary"]       = TextSecondary,
            ["textDisabled"]        = TextDisabled,
            ["textOnAccent"]        = TextOnAccent,
            ["accent"]              = Accent,
            ["accentSubtle"]        = AccentSubtle,
            ["hyperlink"]           = Hyperlink,
            ["selectionBackground"] = SelectionBackground,
            ["selectionText"]       = SelectionText,
            ["selectionInactive"]   = SelectionInactive,
            ["focusIndicator"]      = FocusIndicator,
            ["error"]               = Error,
            ["errorBackground"]     = ErrorBackground,
            ["warning"]             = Warning,
            ["warningBackground"]   = WarningBackground,
            ["success"]             = Success,
            ["successBackground"]   = SuccessBackground,
            ["info"]                = Info,
            ["infoBackground"]      = InfoBackground,
            ["sidebarBackground"]    = SidebarBackground,
            ["sidebarTint"]          = SidebarTint,
            ["sidebarText"]          = SidebarText,
            ["sidebarTextSecondary"] = SidebarTextSecondary,
            ["sidebarSelection"]     = SidebarSelection,
            ["sidebarHover"]         = SidebarHover,
            ["sidebarFocus"]         = SidebarFocus,
            ["rowUnreadBackground"]  = RowUnreadBackground,
            ["rowReadBackground"]    = RowReadBackground,
            ["rowHoverWash"]         = RowHoverWash,
            ["rowSelectedWash"]      = RowSelectedWash,
            ["rowSelectedMarker"]    = RowSelectedMarker,
            ["envelopeUnread"]       = EnvelopeUnread,
            ["envelopeUnreadGlyph"]  = EnvelopeUnreadGlyph,
            ["envelopeRead"]         = EnvelopeRead,
            ["avatarBackground"]     = AvatarBackground,
            ["avatarText"]           = AvatarText,
            ["avatarAlt1"]           = AvatarAlt1,
            ["avatarAlt2"]           = AvatarAlt2,
            ["avatarAlt3"]           = AvatarAlt3,
            ["avatarAlt4"]           = AvatarAlt4,
            ["avatarAlt5"]           = AvatarAlt5,
            ["readingSurface"]       = ReadingSurface,
            ["listSurface"]          = ListSurface,
            ["mainAreaBackground"]   = MainAreaBackground,
            ["providerOutlook"]      = ProviderOutlook,
            ["providerGmail"]        = ProviderGmail,
            ["providerHotmail"]      = ProviderHotmail,
            ["providerOther"]        = ProviderOther,
            ["providerGlyph"]        = ProviderGlyph,
        };
}
