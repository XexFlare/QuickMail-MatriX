# MailBoXX: Authentication

This is an **interim** setup. It reuses QuickMail's authentication code and makes only the change
that licensing required. A separate authentication stack is planned for later, so nothing here is
meant to be permanent.

## What changed from QuickMail (decision D12)

| | QuickMail | MailBoXX |
|---|---|---|
| Microsoft interactive sign-in | In-app **embedded WebView2** window (`Microsoft.Identity.Client.Desktop`, `.WithUseEmbeddedWebView(true)`) | **System browser** (`.WithUseEmbeddedWebView(false)`): MSAL opens the default browser, receives the code on a random `http://localhost` loopback port, then the browser shows "You can close this tab and return to MailBoXX". |
| Packages | `Microsoft.Identity.Client`, `.Desktop`, `.Extensions.Msal`; `.Desktop` pulls in `.Broker` and `.NativeInterop` (`msalruntime`) | `Microsoft.Identity.Client` and `.Extensions.Msal` only |
| Token cache | `MsalCacheHelper` file `msal.cache` in the profile folder, protected with DPAPI (current user) | **Unchanged** |

**Why.** `Microsoft.Identity.Client.NativeInterop` 0.20.6 is licensed under Microsoft Software License
Terms whose §3(e) says the licensee will not "share, publish, distribute, or lease the software". §2(a)
also permits data collection by Microsoft. QuickMail never called `WithBroker()`: the package arrived
only as a transitive dependency of `.Desktop`, which existed for the embedded sign-in window. Dropping
`.Desktop` removes Broker and NativeInterop entirely, which is verified with
`dotnet list package --include-transitive` and in the build output.

**User-visible impact.**
- Microsoft sign-in now happens in a browser tab rather than a window inside the app. The user
  returns to MailBoXX by hand after signing in.
- Windows account broker (WAM) single sign-on is not used. It wasn't used by QuickMail either.
- Microsoft 365 admin consent (`AdminConsentWindow`) is unaffected; it uses the app's own WebView2
  directly, not MSAL.

## Supported sign-in methods (as inherited from QuickMail)

| Provider | Default method | Also available | Notes |
|---|---|---|---|
| Outlook.com, Hotmail, Microsoft 365 | OAuth (MSAL, system browser), IMAP/SMTP | Microsoft Graph backend (Advanced → Connection method) | Needs an Entra app registration (see below) |
| Gmail | **App password** over IMAP/SMTP (requires Google 2-step verification) | "Sign in with Google" OAuth, behind the `GoogleAuth` feature flag (off by default) | OAuth needs a MailBoXX Google Cloud client. Without real credentials the build compiles the placeholder template and Google sign-in is unavailable. Upstream disabled it because Google blocked new sign-ins for QuickMail's client (#369). |
| iCloud, Yahoo | App password | | Upstream behaviour |
| Other IMAP/SMTP, POP3 | Password | Autodiscovery | |

Passwords are stored in **Windows Credential Manager** (`CredentialService`), never in files or the
database. OAuth tokens are stored only in the DPAPI-protected MSAL cache (Microsoft) or Google's token
store (Google). Neither is logged.

## Before real-provider testing or distribution (decision D13)

1. **Microsoft.** The built-in client ID is still **QuickMail's** registration (`UpstreamClientId` in
   `QuickMail/Services/OAuthService.cs`). This is acceptable only for local development builds. Create a
   **MailBoXX** registration as described in `docs/ENTRA-APP-REGISTRATION.md`:
   - multi-tenant plus personal accounts;
   - `http://localhost` under "Mobile and desktop applications";
   - delegated `IMAP.AccessAsUser.All`, `SMTP.Send`, `offline_access`, plus the Graph mail scopes.

   Then:
   - for testing, set the environment variable `MAILBOXX_MS_CLIENT_ID=<application id>` (no rebuild needed);
   - before distribution, replace `UpstreamClientId` with the MailBoXX ID.
2. **Google.** For OAuth, create a MailBoXX Google Cloud project with an OAuth "Desktop app" client.
   Put its ID and secret in the gitignored `QuickMail/Services/GoogleOAuthService.Credentials.cs` (copy
   `docs/GoogleOAuthService.Credentials.example`). Keep the client in "Testing" status with only
   the dedicated test account listed. Public release needs Google's verification for Gmail scopes. Until
   then, an app password is the supported path.
3. **Test accounts.** Use only dedicated Outlook.com and Gmail test accounts. Never use personal
   mailboxes. Credentials go into the app (Credential Manager) or environment variables for
   upstream's `LiveSmokeTests` (`LIVE_*`, see `docs/TESTING-INTEGRATION.md`). They never go into files in the repository.

No real-provider sign-in has been performed for MailBoXX yet. The protocol paths have been exercised
against local GreenMail servers (`QuickMail.IntegrationTests`).
