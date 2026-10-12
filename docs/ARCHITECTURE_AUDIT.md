# MailBox MatriX: Architecture Audit and Validation (QuickMail base)

> **Update (2026-10-11):** QuickMail was approved as the base and the implementation phases have been
> carried out. This audit is kept as the pre-implementation record. For what was actually built,
> verified and deferred, see `docs/IMPLEMENTATION_STATUS.md`.

**Date:** 2026-10-11 · **Status:** Validation complete, awaiting approval before implementation
**Proposed base:** QuickMail, `github.com/kellylford/QuickMail`, commit `f51602efff83826ef57e7aaa57a71e4cce779536` (2026-10-08, v0.8.55)
**Comparison candidate:** Wino Mail (Appendix B). It is not the assumed base.
**Proposed MailBox MatriX licence:** MIT, subject to the obligations in §4.4

**Evidence labels:**
- **[verified]**: established by reading source, building, testing or running it in this validation.
- **[repo]**: taken from upstream documentation and not independently exercised.
- **[est]**: my estimate or judgement.

> This document replaces the earlier Wino-based draft of this file. The candidate comparison is in `docs/CANDIDATE_EVALUATION.md`.

---

## 0. What was done

### 0.1 Environment

**Before** this validation **[verified]**:

| Item | State |
|---|---|
| OS | Windows 11 Pro 10.0.26200, x64 |
| Present | Git 2.55, Node.js, Python 3.14, Windows SDK 10.0.26100, WebView2 Runtime 154.0.4258.62, winget |
| Absent | .NET SDK (any version), Visual Studio, Velopack CLI (`vpk`), PowerShell 7, Java |

**Added:** **.NET SDK 10.0.401** (runtime 10.0.12). It was installed per-user by Microsoft's `dotnet-install.ps1` (Authenticode signature checked as valid, signed by Microsoft Corporation) into `%LOCALAPPDATA%\Microsoft\dotnet`.
- No admin rights were used, and the system `PATH` was not changed.
- It can be removed by deleting that folder.
- Nothing else was installed. Velopack is only needed to build installers; Java is only needed for the integration-test servers.

### 0.2 Working copies (outside the MailBox MatriX docs folder)

| Path | Purpose |
|---|---|
| `X:\Code\MailboXX-spike\QuickMail` | Clone of upstream on the disposable branch **`spike/mailboxx-sidebar-row`**. Changes are **uncommitted**; nothing was pushed. |
| `X:\Code\MailboXX-spike\QuickMail-baseline` | `git worktree` of unmodified `main`, used for before/after comparison |
| `X:\Code\MailboXX-spike\evidence\` | Screenshots, `spike.patch`, test result files (`baseline.trx`, `spike.trx`) |

The only tracked-file changes are on the spike branch. Two gitignored credential stubs (§0.3) exist in both working copies.

### 0.3 Build and test results (unmodified upstream)

| Step | Command | Result |
|---|---|---|
| Clean-clone build | `dotnet build QuickMail.sln -c Debug` | **FAILED: 4 × CS0103.** `RelayUrl`/`RelayKey` (BugReportService) and `ClientId`/`ClientSecret` (GoogleOAuthService) are missing because their partial-class files are gitignored. This contradicts the template's comment that "the build still compiles" without them. Restore took 4.3 min, including one transient NuGet timeout (WireMock.Net.MimePart) that recovered on retry. |
| Add stubs | copy `docs/GoogleOAuthService.Credentials.example` and `docs/BugReportService.Credentials.example` into `QuickMail/Services/*.Credentials.cs` (placeholder values, no secrets) | No tracked files changed |
| Build | `dotnet build QuickMail.sln -c Debug` | **Succeeded: 0 errors, 3,712 warnings.** The warnings are analyser output, overwhelmingly CA1707 naming warnings in the test project. Took 3 min 20 s. |
| Unit tests | `dotnet test QuickMail.Tests/QuickMail.Tests.csproj -c Debug --no-build` | **4,741 passed, 0 failed, 16 skipped (4,757 total), 5 min 29 s** |
| Integration tests | `QuickMail.IntegrationTests` | **Not run.** They need local GreenMail (IMAP/SMTP) and Radicale (CalDAV/CardDAV) servers, which need Java 11+ (not installed) **[repo]**. |
| Release / installer build | `build.bat publish` / `installer` | **Not run.** This needs the Velopack CLI and, for real releases, Azure Artifact Signing **[repo]**. |
| Offline UI capture | `QuickMail.exe --ui-probe inbox --profileDir <fixture>` using the upstream fixture generator (`Tools/QuickMail.Fixtures`) | **Works.** Upstream's probe mode turns off networking at the DI root, loads a synthetic profile ("Test User <test@example.com>", 7 inbox + 2 sent) and saves a PNG. **No email account was used anywhere in this validation.** |

---

## 1. Existing architecture (QuickMail)

| Area | Finding |
|---|---|
| Framework | **WPF on .NET 10** (`net10.0-windows10.0.17763.0`, `UseWPF`). `UseWindowsForms` is also on, for the tray icon and an uninstall prompt. WebView2 1.0.4191.47 is used only for message bodies, compose preview and the admin-consent page **[verified]**. |
| Structure | One app project, `QuickMail/`, with `Views`, `ViewModels`, `Services`, `Models`, `Helpers`, `Styles`, `Themes`, `Theming`. Tests are in `QuickMail.Tests` (xUnit), plus `QuickMail.IntegrationTests` and `Tools/QuickMail.Fixtures`. In total about 92k lines of C# and 11k lines of XAML **[verified]**. |
| Composition | Manual dependency injection in `App.xaml.cs`, one interface per service (`Services/I*.cs`) **[verified]** |
| UI concentration | `Views/MainWindow.xaml` has 2,335 lines, `Views/MainWindow.xaml.cs` 8,322 lines and `ViewModels/MainViewModel.cs` 10,984 lines (plus partial files). Value converters also live inside `MainWindow.xaml.cs` **[verified]**. |
| Mail protocols | MailKit/MimeKit 4.17 for IMAP, POP3 and SMTP, with a pooled IMAP connection model split into foreground and background leases. Microsoft Graph backend for Microsoft 365 / Outlook.com (opt-in "Connection method"). IMAP IDLE push, and Graph delta polling, behind `IChangeNotifier`/`ChangeNotifierRouter` **[verified]**. |
| Authentication | MSAL 4.89 for Microsoft, with a token cache via `MsalCacheHelper`. Google.Apis.Auth 1.76 for Gmail, **feature-flagged off by default** because "Google OAuth sign-in is blocked for new accounts" (#369); Gmail defaults to an app password. The Microsoft client ID is a constant in `Services/OAuthService.cs` **[verified]**. |
| Credentials | Windows Credential Manager via AdysTech.CredentialManager (`Services/CredentialService.cs`) **[verified]** |
| Storage | `%AppData%\QuickMail\`: `mail.db` (SQLite, WAL, `PRAGMA user_version` migrations), `config.ini`, and JSON files for contacts, groups, rules, views, templates and flags. The location can be overridden with `--profileDir` **[verified]**. |
| Offline | The cache-first UI fills from SQLite and sync then fills gaps. Offline bodies are stored (`LocalStoreService.OfflineBodies`, `SyncService.OfflineBodies`). An **outbox** queues drafts and sends when offline, with doubling backoff. `ConnectivityService` debounces offline detection and reconnects automatically **[verified]**. |
| Search | **SQLite FTS5** contentless index over subject, sender, recipients and bodies (`LocalStoreService.Search.cs`), falling back to loaded rows **[verified]** |
| HTML safety | Message HTML is stripped and shown with a strict meta-CSP: `default-src 'none'`, with `script-src`, `img-src`, `connect-src`, `frame-src` and `form-action` all `'none'`. Remote images are blocked; embedded `cid:` images are served from a local host origin (`EmbeddedPictureHost`). DevTools and the status bar are off **[verified]**. |
| Transport security | `MailSecurity` refuses STARTTLS downgrades. Autodiscovery rejects settings that would send plaintext authentication **[verified]**. |
| Logging | Plain file log at `%AppData%\QuickMail\quickmail.log` with no redaction layer. A name-based scan of all 601 log calls found none interpolating password, token or body variables **[verified, heuristic]**. |
| Notifications | Toasts (`WindowsToastNotificationService`, Microsoft.Toolkit.Uwp.Notifications) and a tray icon **[verified]** |
| Outbound non-mail traffic | GitHub Releases update check (`api.github.com/repos/kellylford/QuickMail`). Autodiscovery tiers: Mozilla ISPDB, domain Autodiscover, Cloudflare DNS-over-HTTPS; these can be turned off with `AutoDiscoverOnline`. Optional bug-report relay (Cloudflare Worker). **No telemetry SDK** **[verified]**. |
| Theming | JSON themes (`Themes/BuiltIn/*.json`: light, dark, ember, fjord, heather), applied as `Theme.*` brush and size tokens by `ThemeService`, with a High Contrast passthrough and a text-scale setting. **`ThemeRegressionGuardTests` fail the build's test suite if view XAML hard-codes colours or numeric font sizes** **[verified]**. |
| Accessibility | First-class: `RowSpeech`/`RowLayoutService` (user-configurable spoken row fields), `AriaHiddenTextBlock`, a command palette and remappable shortcuts **[verified]** |
| Packaging | Velopack 1.2.161: Setup.exe and MSI, delta updates from GitHub Releases, winget. CI signs through Azure Artifact Signing over GitHub OIDC (`.github/workflows/build-installer.yml`) **[verified from workflow files; not run]**. |
| CI | 8 workflows, including CodeQL and a winget install matrix **[verified]** |

---

## 2. Requirements coverage (QuickMail and the PRD)

Key: **R** = reusable as-is · **M** = needs modification · **N** = missing · **X** = unnecessary (hide or remove) · **D** = defer

| PRD requirement | Status | Notes |
|---|---|---|
| Genuine Windows UI (§1, §5.1) | **R** | WPF |
| Blue sidebar with accounts and folders (§6.1, §6.3) | **M** | **Prototyped** (Appendix A). Reuses the existing `AccountList` and `FolderList` unchanged. |
| Profile area bottom left (§6.3) | **M/N** | Prototyped. Needs a new view-model property for the primary account (§A.3, item 3). |
| Compact message list and row fields (§6.4) | **M** | **Prototyped**: avatar, sender, subject, preview, time, envelope |
| Row-wide contextual image with gradient (§6.4) | **M** (rendering) / **N** (selection) | Rendering technique prototyped. Selection and persistence are not built (out of scope). |
| Unread blue tint and closed envelope; read neutral and open transparent envelope (§6.5) | **M** | Prototyped. State is shown by envelope shape and bold weight, so it doesn't depend on colour alone. |
| Scenic main-area background (§6.6) | **N** | Not prototyped. Needs the right pane restructured (the reading pane is currently docked under the list). |
| Message open view (§6.1) | **M** | Today the reading pane is docked below the list. MailBox MatriX needs it in the main area (decision D4). |
| Square, flat styling (§6.2) | **R/M** | WPF default chrome is already square. The work is new tokens and templates. |
| Focused / Other tabs, "All" filter, date group headers (design reference) | **N** | No Focused/Other or date grouping exists **[verified by search]**. Graph exposes `inferenceClassification`; IMAP has no equivalent. |
| Multiple accounts (§7) | **R** | Unlimited |
| Outlook / Hotmail / Microsoft 365 (§7) | **R/M** | IMAP+OAuth by default, Graph opt-in. Needs a **MailBox MatriX Entra app registration**. |
| Gmail (§7) | **M** | App password works. OAuth needs MailBox MatriX's own Google Cloud client and **Google verification** for public release. |
| IMAP/SMTP generic, POP3 (§7) | **R** | |
| Read, navigate, compose, send, reply, reply-all, forward (§7) | **R** [repo] | Not exercised against a live server in this validation |
| Attachments (§7) | **R** [repo] | |
| Search (§7) | **R** | FTS5 |
| Read/unread, archive, delete (§7) | **R** | |
| Sync, offline cache, outbox, error status, reconnect (§7, §10) | **R** | The strongest part of the codebase |
| Notifications (§7) | **R** | Rebrand the toast identity |
| Contextual image selection, persistence, cache, privacy (§8) | **N** | Design is unchanged from the earlier plan (§5.3) |
| Secure credentials, OAuth, safe HTML, no secret logging (§9) | **R** | Already meets the bar. Fix the Microsoft broker licence issue (§4.4). |
| No hosted backend or subscription (§2.4) | **R/M** | No backend. Repoint or remove the bug relay and the update source. |
| Packaging (§12) | **M** | Velopack pipeline reusable. Needs MailBox MatriX identity, a signing account, and notices. |
| Calendar, contacts, rules, templates, saved views, Markdown compose | **X** | More than PRD §4 needs. Hide entry points in v1; decide removal later (D6). |
| Menu bar, toolbar, status bar | **X/M** | Not in the design. Removing them affects access keys and discoverability (§A.3, item 8). |

---

## 3. Reusable components and required changes

**Reuse unchanged:** all of `Services/` (mail, sync, store, outbox, connectivity, search, credentials, OAuth, autodiscover, notifications), `Models/`, the HTML pipeline (`Helpers/MessageBodyHtmlBuilder.cs`), the theme engine, `RowSpeech`, the test suites and the fixture/probe tooling.

**Modify:**

| Component | Change |
|---|---|
| `QuickMail/Services/*.Credentials.cs` handling | Make a clean clone build: commit placeholder partials, or use an MSBuild fallback, with real values injected by CI |
| `Services/OAuthService.cs` (Microsoft client ID), Google client | MailBox MatriX registrations |
| `Services/UpdateCheckService.cs`, `Helpers/VelopackRuntime.cs` | Point updates at the MailBox MatriX repository |
| `Services/BugReportService.cs` + `relay/` | Remove, or replace with a MailBox MatriX-owned relay or a plain "open GitHub issue" link (D11) |
| `Theming/ThemeKeys.cs`, `Themes/BuiltIn/*.json`, `Services/ThemeService.cs` | Add a MailBox MatriX theme and new tokens (sidebar gradient, row tints, envelope colours, image fade) and make it the default |
| `Views/MainWindow.xaml` | Shell layout: sidebar, message row, main area, reading view placement |
| `ViewModels/MainViewModel.cs` | Small additions: primary account for the profile area, and later the context-image path per row |
| `QuickMail.csproj`, assembly metadata, icons, installer config | MailBox MatriX name, version, publisher |
| `CLAUDE.md`, `CONTRIBUTING.md`, docs | Rewrite for the fork. Upstream's `CLAUDE.md` contains build guidance only; no unsafe agent instructions were found **[verified by keyword scan]**. |

**Add:** context-image service and store (`Services/ContextImages/*`), a MailBox MatriX theme JSON, image assets with a licence manifest, `THIRD-PARTY-NOTICES.md`, `FORK_NOTES.md`.

**Hide in v1 (not delete):** calendar and contacts entry points (the folder tree's "Calendar" node, menus), rules, templates, saved views.

---

## 4. Risks

### 4.1 Technical

1. **UI concentration.** `MainWindow.xaml.cs` (8.3k lines) and `MainViewModel.cs` (11k lines) are where the remaining shell work lands: main area, reading view placement, menu and toolbar removal. The prototype avoided touching them, but Phase 4 will not be able to **[est]**. Mitigation: put new behaviour in new files and controls, and don't refactor upstream code unless needed.
2. **Reading view relocation.** The reading pane (`SubjectField`, `FromField`, …, the `MessageBody` WebView2) is docked inside the right pane and referenced by name from code-behind. Moving it into a third "main area" column is a layout change with many name dependencies. **It was not prototyped and is the biggest unknown.**
3. **Focused/Other and date grouping.** These are new features, not restyling. Focused/Other is Graph-only. Date headers need a grouped `CollectionView` alongside the existing view modes (Messages, Conversations, From, To).
4. **WebView2 airspace.** WPF cannot draw over or blend with a WebView2 surface. A scenic background behind the message body only works around it, not through it **[est, standard WPF limitation]**.
5. **Acrylic sidebar.** WPF has no acrylic brush. A gradient (prototyped) or a pre-blurred image are the practical options.
6. **Upstream velocity and maintainer.** The project is 5 months old, has one main maintainer and is at v0.8.x **[api]**. Merging upstream changes into a heavily restyled `MainWindow.xaml` will conflict. The MIT licence makes permanent divergence acceptable.
7. **Build noise.** 3,712 analyser warnings hide new ones. Consider a warnings baseline for the fork.

### 4.2 Security and privacy

- Existing protections are strong and must be kept: strict CSP, remote images blocked, Credential Manager, TLS-downgrade refusal, no telemetry.
- Microsoft accounts: the WAM broker path brings in `Microsoft.Identity.Client.NativeInterop`, whose terms (§2a) allow data collection by Microsoft. This is a privacy disclosure item as well as a licence issue (§4.4).
- Autodiscovery sends the address's **domain** to Mozilla and Cloudflare, and the **full address** to the domain's own Autodiscover endpoint. Document this, and consider making online autodiscovery opt-in.
- The file log has no redaction layer. Keep the "never log secrets or bodies" rule and add a test or analyser for it.

### 4.3 Upstream build hygiene

- A clean clone doesn't compile (§0.3).
- Real Google and relay secrets are injected by CI and "baked into" upstream's release binaries **[repo]**. MailBox MatriX must use its own secrets and never reuse upstream's.

### 4.4 Licensing (proposed MailBox MatriX licence: MIT)

**Upstream:** `LICENSE` is MIT, "Copyright (c) 2026 Kelly Ford" **[verified]**. A MIT-licensed fork is permitted. Obligation: keep this copyright and permission notice in all copies, and add a MailBox MatriX copyright line.

**Shipped dependencies.** The app's full transitive package graph (`dotnet list package --include-transitive`), with licences read from each restored `.nuspec` **[verified]**:

| Licence | Packages | Obligation |
|---|---|---|
| MIT | MailKit, MimeKit, BouncyCastle.Cryptography, Microsoft.Identity.Client (+Desktop, Broker, Extensions.Msal), Microsoft.IdentityModel.Abstractions, CommunityToolkit.Mvvm, Microsoft.Data.Sqlite(+Core), AdysTech.CredentialManager, Microsoft.Toolkit.Uwp.Notifications, Velopack, Newtonsoft.Json 13.0.4, System.Management | Include each copyright and licence text in notices |
| Apache-2.0 | Google.Apis, Google.Apis.Auth, Google.Apis.Core, SQLitePCLRaw.* | Include the licence, and any NOTICE file contents |
| BSD-2-Clause | Markdig | Include the copyright and licence |
| Microsoft BSD-3-style | Microsoft.Web.WebView2 (SDK) | Reproduce the notice; no endorsement |
| **Microsoft Software License Terms (proprietary)** | **Microsoft.Identity.Client.NativeInterop 0.20.6** (transitive through MSAL.Desktop → Broker) | **§3e: "you will not … share, publish, distribute, or lease the software".** §2a allows Microsoft data collection. **Read literally, this conflicts with redistributing it in a MailBox MatriX installer.** Clarify with Microsoft, or drop the broker (WAM) dependency and use MSAL's system-browser sign-in (D12). |
| Build-only (not shipped) | Roslynator.Analyzers (Apache-2.0), Microsoft.NET.ILLink.Tasks | None for distribution |

**Other components:**
- The WebView2 **Runtime** is installed separately under Microsoft's terms (the installer bootstraps it).
- SQLite (inside e_sqlite3) is public domain.
- **The sample row image in the spike is cropped from the MailBox MatriX design reference. It is for local prototyping only and must not be committed or shipped.**

**Trademarks:** "QuickMail" is not claimed as a trademark in the repository, but rebrand fully anyway. The `DinamicaTech` fork uses the "Eudora" name; MailBox MatriX should not.

**Conclusion:** MIT is viable for MailBox MatriX. The one open item is the MSAL NativeInterop terms.

---

## 5. Recommended implementation architecture

```
QuickMail (WPF, single app project)
├─ Services/ … unchanged mail, sync, store, outbox, search, auth   ← reuse
├─ Services/ContextImages/ (new)  IContextImageService, classifier, SQLite table, bounded cache
├─ Themes/BuiltIn/mailboxx.json (new) + ThemeKeys additions        ← all colours and sizes as tokens
├─ Views/MainWindow.xaml
│   ├─ Sidebar: blue tokens, scoped over existing AccountList/FolderList (prototyped)
│   ├─ MessageList: stacked row template, ImageBrush layer (prototyped)
│   └─ MainArea (new column): scenic ImageBrush background; reading view hosted here
└─ Views/Mbx*.cs (new files) — new converters and controls kept out of MainWindow.xaml.cs
```

Principles proven or learned in the spike:
- **Restyle through tokens, not template rewrites.** Override `Theme.*` brushes at the scope; set the scope's own `TextElement.Foreground` because inherited values don't re-resolve.
- **Images are paint, not layout.** Use an `ImageBrush` in a `Border`, so text scaling decides row height.
- **Every colour and font size is a token.** The guard tests enforce this and keep High Contrast and text scaling working.
- **Keep control names, types and handlers.** The code-behind relies on them by name.

**Contextual images:** the v1 design is unchanged from the earlier plan. A bundled, licensed image library and a deterministic local classifier (sender, domain, subject → category) send nothing off the device. The result goes in a new `mail.db` table keyed by message, with a neutral fallback, and an opt-in online provider comes later. **Android:** later, extract `Services/` and `Models/` into a `net10.0` class library. Only 14/123 service files and 1/59 model files reference WPF or Windows APIs **[verified]**.

---

## 6. Phased roadmap (QuickMail base)

| Phase | Outcome | Main files | Preserve | Acceptance |
|---|---|---|---|---|
| **0. Fork baseline** | MailBox MatriX repo from the pinned upstream commit; clean clone builds; CI green | `.csproj`, credential partials, `.github/workflows`, `FORK_NOTES.md`, `LICENSE` (+ MailBox MatriX line) | Everything | Clean clone builds with no manual steps; 4,741+ tests pass; `--ui-probe` captures work |
| **1. Identity, privacy, licence** | Own Entra and Google clients; updates from the MailBox MatriX repo; bug relay removed or replaced; MSAL broker decision applied; notices file | `OAuthService.cs`, `GoogleOAuthService*`, `UpdateCheckService.cs`, `VelopackRuntime.cs`, `BugReportService.cs`, `THIRD-PARTY-NOTICES.md` | Auth flows, Credential Manager, CSP | No traffic to upstream endpoints (proxy capture); sign-in works with test accounts (§7 D13) |
| **2. MailBox MatriX theme and sidebar** | Tokenised version of the prototyped sidebar; profile area from a new VM property; focus ring visible on blue; calendar and other extras hidden | `Theming/ThemeKeys.cs`, `Themes/BuiltIn/mailboxx.json`, `MainWindow.xaml`, `MainViewModel` (small) | Account and folder behaviour, keyboard navigation, `RowSpeech` | `ThemeRegressionGuardTests` pass; High Contrast and 150% text checked in probe; Narrator reads accounts and folders as before |
| **3. Message list** | Tokenised version of the prototyped row; envelope icons; date group headers; header search, sync and select | `MainWindow.xaml`, new `Views/Mbx*` controls, `MainViewModel.Search` | Selection, multi-select, context menu, flags, view modes, virtualisation | Guard tests pass; 5,000-message scroll stays smooth; read/unread distinguishable in greyscale |
| **4. Main area and reading view** | Scenic background in the main column; message opens there; menu and toolbar removed or collapsed behind the command palette | `MainWindow.xaml(.cs)`, reading-pane code | Reader security, keyboard access to every command | Every menu command still reachable (command palette or shortcut); reading and compose work in the probe and against GreenMail |
| **5. Contextual images v1** | Local deterministic selection, persistence, neutral fallback | `Services/ContextImages/*`, `mail.db` migration, row template | Sync never waits on images | Zero outbound requests from image selection; unit tests for the classifier and persistence |
| **v0.1 gate** | Signed Velopack installer under the MailBox MatriX identity | installer config, signing | | Install, update and uninstall verified on a clean VM |
| Later | Focused/Other (Graph), online image provider, extracting a core library for Android | | | |

Phase 0 depends on approval. Phases 2–5 depend on Phase 1's identity work only for live testing; the probe and fixture tooling lets UI work proceed offline.

---

## 7. Decisions requiring your input

| # | Decision | Recommendation |
|---|---|---|
| D4 | Where opened mail appears: in the main (scenic) area, replacing the docked reading pane, or in a separate window? | Main area |
| D6 | Calendar, contacts, rules, templates, saved views: hide in v1, or delete? | Hide |
| D11 | Bug reporting: remove the relay, run a MailBox MatriX relay, or link to GitHub issues? | GitHub link (no hosted service) |
| D12 | MSAL WAM broker (NativeInterop terms): drop it for browser sign-in, or seek clarification from Microsoft? | Drop it; browser sign-in is fully supported by MSAL |
| D13 | **Test accounts for Phase 1.** No accounts were used in this validation. Live testing needs: (a) a MailBox MatriX **Microsoft Entra** app registration, free, multi-tenant plus personal accounts, `IMAP.AccessAsUser.All`/`SMTP.Send` and Graph `Mail.*` scopes per `docs/ENTRA-APP-REGISTRATION.md`; (b) a **dedicated** Outlook.com test mailbox; (c) a dedicated Gmail test account with 2-step verification and an **app password**, or (d) a MailBox MatriX **Google Cloud** OAuth client in "testing" mode, limited to listed test users. Alternatively, (e) install Java 11+ and use upstream's local GreenMail servers, which need no real accounts. | Start with (e), then add (a)+(b)+(c) |
| D14 | Remove the menu bar and toolbar (as in the design), keeping everything reachable through the command palette and shortcuts? | Yes, in Phase 4 |
| D15 | Focused/Other in v0.1 (Microsoft accounts only), or defer? | Defer |
| D8 | Licensed scenic background and row-image library: who sources them? | Commission or CC0 |
| D9 | Publisher name and code-signing account for the installer | Needed before the v0.1 gate |

---

## Appendix A: Prototype validation

### A.1 What was prototyped

Exactly the two items requested: the **MailBox MatriX sidebar** and **one image-backed message row**. No features were removed. Contextual image selection, the scenic main area and the reading view were not touched.

### A.2 Files changed (branch `spike/mailboxx-sidebar-row`, uncommitted)

| File | Change |
|---|---|
| `QuickMail/Views/MainWindow.xaml` | +200 / −122 lines across three regions: window resources (+9: converter, sidebar gradient, sample `ImageBrush`); left pane (scoped token overrides, header with "New mail" bound to the existing `NewMessageCommand`, profile footer whose settings gear uses `ManageAccountsCommand`, row indices shifted); `MessageList` (removed the `GridView` columns and rewrote the item container template) |
| `QuickMail/QuickMail.csproj` | +5: `<Resource Include="Assets\Prototype\*.jpg"/>` |
| `QuickMail/Views/MailBox MatriXSpikeConverters.cs` | New, 27 lines: `InitialConverter` (avatar letter) |
| `QuickMail/Assets/Prototype/row-sample.jpg` | New: 343×238 crop from the design reference (prototype only) |
| **C# code-behind, view models, services** | **No changes** |

The full diff is in `X:\Code\MailboXX-spike\evidence\spike.patch`.

### A.3 Coupling discovered

1. **The message list is cleanly replaceable.** `MainWindow.xaml.cs` refers to `MessageList` 175 times, by name and as a `ListView`, but **never to its `GridView` columns**. Nothing sorts or saves column widths. `RowSpeech` and `RowLayoutService` control only spoken output, independent of visuals. Removing the columns broke no behaviour or test.
2. **The sidebar can be re-themed without editing its templates.** `AccountList`, `FolderList`, their handlers, context menus and automation names are untouched; scoped `Theme.*` overrides restyle them. *Caveat:* inherited properties (`Foreground` from the `Window`) do not re-resolve, so the scope root must set `TextElement.Foreground` itself. This was found and fixed in the spike.
3. **There is no "current user" in the view model.** `SelectedAccount` is null in unified views such as "All Mail", so the profile footer rendered empty. The spike bound it to `Accounts[0]`; production needs a proper property.
4. **The focus ring is outside the scope.** `FocusVisualStyle` renders in the adorner layer, so sidebar token overrides don't reach it, and its colour on blue is unverified. Probe screenshots don't show keyboard focus. A sidebar-specific focus style is likely needed.
5. **Images must not drive layout.** An `Image` element in an auto-height row made the image row grow at 150% text scale. Painting through an `ImageBrush` fixed it (compare `07-spike-150-fixed.png` with `09-spike-150.png`).
6. **Styling guard tests.** `ThemeRegressionGuardTests` failed on the spike's hard-coded colours and font sizes, as intended. Production MailBox MatriX styling must be tokens in the theme JSON. This also gives dark, High Contrast and text-scale support for free.
7. **Converters live in the 8.3k-line `MainWindow.xaml.cs`.** New code should go in separate files, as the spike did.
8. **Menu and toolbar.** These remain in the spike. Removing them for the design affects access keys and discoverability; the command palette (`CommandRegistry`) is the replacement path. Not tested.
9. **Reading pane and main area.** These were not prototyped. The reading pane is docked in the same right-hand `Grid` as the list and referenced by name (`MessageBody`, `SubjectField`, …), so relocating it is the main remaining coupling risk.

### A.4 Problems encountered

| Problem | Cause | Resolution |
|---|---|---|
| Clean clone doesn't compile | Gitignored credential partials | Copied upstream's example templates (placeholders) |
| NuGet timeout | Transient network | Retry succeeded automatically |
| XAML error MC3072 | My use of `AutomationProperties.AccessibilityView` (WinUI-only) | Used upstream's `AriaHiddenTextBlock` |
| Dark section labels on blue | Inherited foreground (A.3, item 2) | `TextElement.Foreground` on the sidebar root |
| Empty profile footer | No current-user concept (A.3, item 3) | Bound to `Accounts[0]` (spike only) |
| Text clipped at 150%, then image row ballooned | Fixed row height, then image-driven measure | `MinHeight` + `Theme.FontSizeLarge` + `ImageBrush` |
| Reading pane empty, date "January 1, 0001" in the probe | **Also present in unmodified upstream** (`06-baseline-reading-pane.png`): fixture/probe limitation | Not a regression |
| Dark theme shows light rows | Hard-coded spike colours | Expected; tokens in production |
| 2 guard tests fail | Hard-coded spike colours and sizes | Expected; tokens in production |

### A.5 Test results after the prototype

`dotnet test` (Debug): **4,739 passed, 2 failed, 16 skipped (4,757 total).** The two failures are `ThemeRegressionGuardTests.ViewXaml_HasNoHardcodedBrushColors` and `ViewXaml_HasNoNumericFontSizes`, both about the spike's styling shortcuts. **Every functional test still passes.**

### A.6 Evidence (in `X:\Code\MailboXX-spike\evidence\`)

| File | Shows |
|---|---|
| `00-baseline-inbox.png` | Unmodified QuickMail |
| `08-spike-100.png` | Prototype at 100%: blue sidebar, profile footer, image row, unread/read rows, flag bar |
| `09-spike-150.png` | Prototype at 150% text scale |
| `03-spike-dark.png` | Dark theme (shows the hard-coded colour limitation) |
| `05-spike-reading.png` + `06-baseline-reading-pane.png` | Reading pane before and after (same probe limitation) |
| `01`, `02`, `04`, `07` | Intermediate iterations showing the problems in A.4 |

### A.7 Not verified

- Live sync, send, OAuth sign-in or attachments (no accounts used).
- Integration tests (Java/GreenMail).
- Release or single-file publish and the installer.
- High Contrast mode, keyboard focus visuals, Narrator.
- Scroll performance with images on many rows.

---

## Appendix B: Wino Mail (comparison only)

Audited earlier at commit `c238bc2` (2026-10-11).
- **Strengths:** the closest existing Windows 10 Mail look (background themes, Focused/Other pivot, date headers, reading frame), a modular `net10.0` core already shared with macOS, and five years of maturity.
- **Weaknesses for MailBoxMatriX:**
  - GPL-3.0 copyleft.
  - A commercial backend woven into its core (Wino account, billing, a 3-account limit, cloud "Mail Intelligence" that uploads message bodies to `api.winomail.app`, Sentry telemetry with a hard-coded DSN), with about 90 files and package references inside the synchronizers to remove.
  - **Plaintext IMAP/SMTP passwords and Gmail refresh tokens** on Windows.
  - Remote images on by default.
  - A heavy Native AOT/MSIX build.
  - An upstream `AGENTS.md` telling agents to publish packages "without asking for approval".
- **Verdict:** it remains the fallback if QuickMail's shell restructuring (Phase 4) proves unworkable. Its look is closer, but its foundations conflict with the PRD's privacy and no-backend goals.
