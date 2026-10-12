namespace QuickMail;

/// <summary>
/// MailBox MatriX: the one place the application's identity is defined. Everything that must not
/// collide with an installed QuickMail — data folder, Credential Manager targets, single-instance
/// names, sign-in-at-startup entry, shortcut — derives from here. (Namespaces and type names stay
/// "QuickMail" so upstream changes still merge.)
/// </summary>
internal static class AppIdentity
{
    /// <summary>The product name shown to people.</summary>
    public const string Name = "MailBox MatriX";

    /// <summary>
    /// The name without a space, for everything technical: the exe, data folder, Credential Manager
    /// prefix, single-instance names, the sign-in-at-startup entry, HTTP User-Agent tokens and mail
    /// header names — places where a space is invalid or awkward.
    /// </summary>
    public const string TechnicalName = "MailBoxMatriX";

    /// <summary>Folder under %AppData% / %LocalAppData% for settings, cache, logs and WebView2 data.</summary>
    public const string DataFolderName = TechnicalName;

    /// <summary>Prefix of every Windows Credential Manager target the app creates.</summary>
    public const string CredentialPrefix = TechnicalName;

    /// <summary>
    /// GitHub repository that publishes MailBox MatriX releases (Velopack feed and the release check).
    /// Empty until MailBox MatriX has one: the app then makes no update requests at all — it must never
    /// offer QuickMail's releases. A local feed can still be tested with --updateFeed.
    /// </summary>
    public const string UpdateRepoUrl = "";

    /// <summary>
    /// GitHub owner/repository that takes bug reports, or empty while MailBox MatriX has none — the
    /// report is then copied to the clipboard and no browser page is opened.
    /// </summary>
    public const string IssuesRepoOwner = "";
    public const string IssuesRepoName = "";
}
