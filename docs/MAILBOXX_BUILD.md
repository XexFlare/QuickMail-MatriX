# MailBox MatriX: Build, test and package

Every command here was run on the development machine on 2026-10-11 (Windows 11, x64), unless it is
marked *not yet run*.

## 1. Prerequisites

| Tool | Needed for | Notes |
|---|---|---|
| .NET SDK 10.0.x | everything | `global.json` pins the 10.0 band (`rollForward: latestMajor`). On the dev machine it was installed per-user with Microsoft's `dotnet-install.ps1 -Channel 10.0` into `%LOCALAPPDATA%\Microsoft\dotnet`. |
| WebView2 Runtime | running the app | Ships with Windows 11 |
| Java 11+ and PowerShell 7 | integration tests only | Portable copies were used: Microsoft OpenJDK 17 and PowerShell 7.6.6 under `%LOCALAPPDATA%\MailBoXX-devtools` |
| Python 3 + pip | integration tests only (Radicale) | The test-server script runs `pip install --user radicale==3.7.7` |
| Velopack CLI (`vpk`) | installer only | *Not installed*, see §6 |

A clean clone builds with no secrets. The gitignored Google OAuth and bug-report credential files
fall back to the placeholder templates in `docs/*.example`. See `docs/AUTHENTICATION.md` for what
real-provider sign-in needs.

## 2. Build and unit tests

```powershell
dotnet build QuickMail.sln -c Debug
dotnet test QuickMail.Tests/QuickMail.Tests.csproj -c Debug --no-build
```

The last full run passed 4,796 tests, with 0 failures and 16 skipped (the 16 are upstream skips).
`DateTimeFieldEntryTests.SteppingWithAnArrow_LeavesTheValueUnselected` failed once under full-suite
load and passed 3 of 3 times on its own. It is a timing-sensitive upstream WPF test that MailBox MatriX has
not changed.

Guard tests to know about:
- `ThemeRegressionGuardTests` (upstream): view XAML may not hard-code colours or numeric font sizes.
  Use `Theme.*` tokens.
- `BuiltInThemeTests` and `MailBoXXThemeTests`: WCAG contrast for every built-in theme. The MailBox MatriX
  tests composite translucent tokens over white and black worst cases.
- `ContextImageTests`: classifier, persistence, privacy (no message text in the store), and that the
  bundled pictures exist.

## 3. Integration tests (local mail servers, no real accounts)

```powershell
$env:JAVA_HOME = "$env:LOCALAPPDATA\MailBoXX-devtools\jdk"
& "$env:LOCALAPPDATA\MailBoXX-devtools\pwsh\pwsh.exe" -File .\scripts\start-test-servers.ps1 -JavaExe "$env:JAVA_HOME\bin\java.exe"
dotnet test QuickMail.IntegrationTests/QuickMail.IntegrationTests.csproj -c Debug
.\scripts\start-test-servers.ps1 -Stop      # when done
```

GreenMail provides IMAP on 127.0.0.1:3143, SMTP on 3025 and POP3 on 3110, with authentication
disabled. Radicale provides CalDAV/CardDAV on 5232.

The last run had 38–39 passed, 1–2 failed and 9 skipped. The failures are always in `CalDavTests`:
Radicale drops one HTTP response per run ("The response ended prematurely"), and which test fails
varies. This happened identically before any MailBox MatriX UI change. All IMAP, SMTP and POP3 tests pass.
The skips need live internet, real-provider accounts (`LIVE_*` variables), or hit a documented
GreenMail limitation.

## 4. Offline UI captures (no network, no accounts)

```powershell
dotnet run --project Tools/QuickMail.Fixtures -c Debug -- --out $env:TEMP\mbx-fixture
.\QuickMail\bin\Debug\MailBoxMatriX.exe --ui-probe inbox --theme mailboxx-light --profileDir $env:TEMP\mbx-fixture --capture-dir $env:TEMP\mbx-shots
```

Surfaces include `inbox`, `reading-pane`, `menu-bar` (added by MailBox MatriX), `compose` and
`settings-appearance`. Use `--theme mailboxx-dark` and `--text-scale 150` for the other cases.
Probe mode turns networking off. The capture uses `PrintWindow`, and falls back to copying from the
screen only if that comes back blank, so **run captures when the desktop is not in use**.

Known probe limitation (upstream): the `reading-pane` surface opens a fixture message whose details
are not in the offline store, so it shows an empty body.

## 5. Artwork

```powershell
dotnet run --project Tools/MailBoXX.ArtGen -c Release -- QuickMail/Assets
```

This regenerates `Assets/ContextImages/{light,dark}/*.jpg` (14 categories × 2 variants) and
`Assets/App/mailboxx.ico`. It is deterministic and uses no third-party images. Add
`--placeholder-scenic` to also draw the offline fallback landscape. The shipped background,
`Assets/Scenic/serengeti-kopjes.jpg`, is a CC BY 2.0 photograph (see `THIRD-PARTY-NOTICES.md`).

## 6. Packaging

**Development build (portable, done):**

```powershell
dotnet publish QuickMail/QuickMail.csproj -c Release -o publish
```

This produces `publish\MailBoxMatriX.exe`: single-file, self-contained, ReadyToRun, win-x64, 306 MB. It
runs on Windows 10/11 x64 without .NET installed. It was smoke-tested against the local GreenMail
server (it synced 4 messages and showed contextual pictures). `build.bat publish` does the same.
The size could roughly halve with `EnableCompressionInSingleFile`, at some startup cost. That has not
been evaluated.

**Installer (*not yet run*):** `build.bat installer` packs a Velopack Setup.exe and MSI with package
ID `MailBox MatriX`. It needs:
1. The Velopack CLI at the version in `.github/vpk-version`: `dotnet tool install -g vpk --version <that>`.
2. A **code-signing** setup. Upstream signs through Azure Artifact Signing in CI, which belongs to
   upstream's account. MailBox MatriX needs its own (decision D9).
3. A **release feed** for updates. `AppIdentity.UpdateRepoUrl` is empty, so the app makes no update
   checks until MailBox MatriX has a releases repository.
4. A licence-collection step (see `THIRD-PARTY-NOTICES.md`).

**CI:** the workflows in `.github/workflows` are upstream QuickMail's: they build `QuickMail.exe`
and use upstream's signing and relay secrets. They have **not** been adapted and will not work for
MailBox MatriX as they stand.

## 7. Data locations (MailBox MatriX identity)

| What | Where |
|---|---|
| Settings, cache, contacts, rules | `%AppData%\MailBoxMatriX\` (`mail.db`, `config.ini`, …) |
| Contextual-picture choices | `%AppData%\MailBoxMatriX\context-images.db` |
| Log (only when enabled) | `%AppData%\MailBoxMatriX\mailboxmatrix.log` |
| WebView2 data | `%LocalAppData%\MailBoxMatriX\WebView2*` |
| Passwords | Windows Credential Manager, targets `MailBoxMatriX:<account id>` |

`--profileDir <folder>` moves the settings, cache, picture choices and log into that folder. WebView2
data and Credential Manager entries stay where they are, except in `--ui-probe` mode, which keeps
WebView2 data inside its capture folder.
