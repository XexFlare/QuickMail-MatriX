using System;
using Velopack;
using Velopack.Sources;

namespace QuickMail.Helpers;

/// <summary>
/// True when this process runs from a Velopack install (%LocalAppData%\QuickMail\current\),
/// as opposed to the portable exe or a dev build. Gates install-only behavior such as the
/// first-run desktop shortcut offer.
/// </summary>
public static class VelopackRuntime
{
    public static bool IsInstalled { get; } = Compute();

    private static bool Compute()
    {
        try
        {
            // The source is never contacted here; construction only inspects the local
            // install layout via IsInstalled.
            // MailBox MatriX: with no release repository yet, point the (never contacted) source at the
            // app folder — IsInstalled only inspects the local install layout.
            return (Services.UpdateCheckService.RepoUrl.Length == 0
                    ? new UpdateManager(AppContext.BaseDirectory)
                    : new UpdateManager(new GithubSource(Services.UpdateCheckService.RepoUrl, accessToken: null, prerelease: false)))
                .IsInstalled;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
