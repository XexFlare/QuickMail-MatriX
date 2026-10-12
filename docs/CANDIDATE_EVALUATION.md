# MailBox MatriX: Base-Project Candidate Evaluation

**Date:** 2026-10-11 · **Status:** For review, no base project selected · **Scope:** read-only research; no SDKs installed, nothing built or executed

**Evidence labels used below**

- **[src]** I verified it by reading the candidate's source at the commit listed.
- **[repo]** It comes from the candidate's own README or docs and was not confirmed in code.
- **[api]** GitHub API metadata, fetched 2026-10-11.
- **[est]** My estimate or judgement.

---

## 1. Original shortlist and recovered decisions

### What I could and could not recover

| Searched | Result |
|---|---|
| `X:\Code\MailboXX` (all files) | Only `Product_Requirements.md`, the design reference image, and my own `docs/ARCHITECTURE_AUDIT.md`. **No prior candidate research.** |
| Other folders under `X:\Code` (`*.md`, `*.txt`, `*.json`) | No mention of QuickMail, Skim, Pelton or MailAggregator |
| Claude Code memory for this project | Empty |
| Your claude.ai artifacts (13 listed) | None relates to MailBox MatriX or email clients |
| Earlier claude.ai chats | **Not accessible** from this environment |

**Nothing from the original evaluation could be recovered.** I have not reviewed the original reasoning, scoring or any other candidates it may have covered. The only record of earlier decisions is the summary you gave me:

| Project | Technology | Original note |
|---|---|---|
| QuickMail | C# / WPF | Preferred candidate: a genuine Windows desktop app |
| Skim | Rust / Svelte / Tauri | Lean and offline-first, but web-based UI |
| Pelton | Go / Wails | Cross-platform, web-based UI |
| MailAggregator | .NET / WPF | PolyForm Noncommercial licence raised distribution concerns |

### Repositories identified

The names were not given as URLs, so I matched them by GitHub search:

| Candidate | Repository | Commit inspected | Match confidence |
|---|---|---|---|
| QuickMail | `kellylford/QuickMail` | `f51602e` (2026-10-08) | High: C#/WPF desktop email client, the only substantial match |
| *(QuickMail fork)* | `DinamicaTech/QuickMail` ("Eudora QuickMail") | `c1b54e9` (2026-09-20) | A downstream fork of the above: shares 728 files and the MIT notice "Copyright (c) 2026 Kelly Ford" **[src]**, created 2026-09-19 **[api]**. Evaluated as part of QuickMail, not separately. **Confirm which one the original research meant (Q1).** |
| Skim | `nikserg/skim` | `9501088` (2026-10-10) | High |
| Pelton | `peltonapp/Pelton` | `b294aaf` (2026-10-03) | High |
| MailAggregator | `wzxklm/MailAggregator` | `b8380af` (2026-05-14) | High: .NET 8/WPF and PolyForm Noncommercial, both matching your note |
| Wino Mail *(comparison only)* | `bkaankose/Wino-Mail` | `c238bc2` (2026-10-11) | Audited earlier in `docs/ARCHITECTURE_AUDIT.md` |

---

## 2. Comparison against MailBox MatriX requirements

### 2.1 Summary matrix

| Criterion (PRD ref) | **QuickMail** | Skim | Pelton | MailAggregator | Wino Mail *(comparison)* |
|---|---|---|---|---|---|
| Genuine Windows UI (§1, §5.1) | ✅ WPF on .NET 10 **[src]**. WebView2 is used only for message bodies. | ❌ Svelte 5 UI inside Tauri 2/WebView2 **[src]** | ❌ Svelte 5 UI inside Wails v2 webview **[src]** | ✅ WPF + ModernWpfUI **[src]** | ✅ WinUI 3 **[src]** |
| Licence | **MIT** **[src]** | MIT **[src]** | GPL-3.0 **[src]** | ❌ **PolyForm Noncommercial 1.0.0** **[src]** | GPL-3.0 **[src]** |
| Multiple accounts | ✅ unlimited **[repo]** | ✅ **[repo]** | ✅ **[repo]** | ✅ **[repo]** | ⚠️ 3 free, then a paid add-on **[src]** |
| Microsoft / Outlook / Hotmail | ✅ MSAL. IMAP by default, Graph opt-in **[src]** | ✅ OAuth over IMAP **[repo]** | ⚠️ OAuth code mentions Microsoft endpoints **[src]**; README advertises Gmail OAuth only **[repo]** | ✅ OAuth PKCE **[repo]** | ✅ Graph API **[src]** |
| Gmail | ⚠️ App password by default; Google sign-in behind a feature flag because "Google OAuth sign-in is blocked for new accounts (#369)" **[src]** | ✅ OAuth or app password **[repo]** | ✅ OAuth **[repo]** | ✅ OAuth **[repo]** | ✅ Gmail API OAuth **[src]** |
| Offline cache and reading | ✅ SQLite (WAL), offline bodies, outbox queue with backoff **[src]** | ✅ SQLite, durable offline queue **[repo]** | ✅ local storage and outbox **[src]** | ✅ EF Core SQLite **[repo]** | ✅ SQLite and MIME cache **[src]** |
| Search | ✅ **FTS5** full-text index **[src]** | ✅ FTS5 **[repo]** | ✅ Bleve index **[src]** | ? | ⚠️ SQL `LIKE` only **[src]** |
| Attachments, reply/forward, archive/delete | ✅ **[repo]** | ✅ **[repo]** | ✅ **[repo]** | ✅ (archive unverified) **[repo]** | ✅ **[src]** |
| Credential storage | ✅ Windows Credential Manager; MSAL cache helper **[src]** | ✅ OS keyring **[src]** | ✅ OS keyring **[src]** | ✅ AES-GCM + DPAPI **[repo]** | ❌ IMAP passwords and Gmail tokens in **plaintext** on Windows **[src]** |
| Safe HTML | ✅ CSP `default-src 'none'`, `script-src`/`img-src 'none'`, `connect-src 'none'`, plus stripping **[src]** | ✅ `ammonia` sanitiser **[src]** | ✅ `bluemonday` sanitiser **[src]** | ? | ✅ nonce CSP and request filter **[src]**; remote images on by default |
| Hosted services or subscriptions to remove | Minor: optional Cloudflare bug-report relay, GitHub update check **[src]** | AI features use the user's own key (optional) **[repo]** | **MCP server** exposing mail to AI agents; VirusTotal integration **[src]** (defaults not checked) | AI translate/summarise (user's own key) **[repo]** | ❌ Major: Wino account, billing, cloud "Mail Intelligence" that uploads bodies, Sentry **[src]** |
| Notifications | ✅ toasts and tray **[src]** | ✅ **[repo]** | ✅ **[src]** | ✅ IDLE push **[repo]** | ✅ **[src]** |
| Fit to Win10 Mail design (§6) | ⚠️ Classic three-pane, accessibility-first UI; layout must be rebuilt **[est]** | ⚠️ Easiest to style (HTML/CSS) but excluded by §1 | ⚠️ Same as Skim | ⚠️ Small UI, rebuild needed | ✅ Closest: Win10-Mail lineage, background themes, Focused/Other, date headers **[src]** |
| Packaging | ✅ Velopack setup.exe/MSI, auto-update from GitHub Releases, winget **[src]** | ✅ NSIS-style setup and updater **[src]** | ✅ multi-OS **[repo]** | ❌ none (build from source) **[repo]** | ✅ MSIX, Store, App Installer **[src]** |
| Tests and CI | ✅ about 3,856 test attributes; 8 workflows including CodeQL **[src]** | 322 Rust tests, CI **[src]** | 127 Go test files **[src]** | 237 tests **[repo]**, no CI seen | about 1,015 tests, no CI in repo **[src]** |
| Code size | about 92k C# lines + 11k XAML, in one app project **[src]** | 25k Rust + 14k Svelte/TS **[src]** | 40k Go (excluding tests) **[src]** | 150 files total **[src]** | about 189k lines, 30+ projects **[src]** |
| Maturity **[api]** | Created 2026-05-09. 34★, 6 forks, 88 open issues. Pushed 2026-10-08, v0.8.x. | Created 2026-07-12. 60★. Pushed 2026-10-10. | Created 2026-06-26. 160★. Pushed 2026-10-09. | Created 2026-03-08. 24★. Last push 2026-05-14. | Created 2021-09-18. 1,600★, 97 forks. Pushed 2026-10-10. |
| Android reuse (§5.3) | ⚠️ Services sit inside the WPF project, but only 14/123 service files and 1/59 model files reference WPF/Windows APIs **[src]**. Extracting a `net10.0` core is realistic **[est]**. | ✅ Tauri 2 targets Android; Rust core portable **[est]** | ⚠️ Go core; Android through gomobile is unusual **[est]** | ✅ Already split into `Core`/`Desktop` **[src]** | ✅ `net10.0` core already shared with a macOS head **[src]** |
| Build requirements | .NET 10 SDK (`rollForward: latestMajor`). Velopack CLI only for installers. Two gitignored credential files must be created from the `docs/*.example` files (CI generates them) **[src]**. | Rust + Node + Tauri CLI | Go 1.26 + Node + Wails CLI | .NET 8 SDK | .NET SDK pinned to 10.0.401, Windows App SDK, MSIX tooling, Native AOT |

### 2.2 Per-candidate notes

**QuickMail (evaluated first)**
- **Strengths [src]:**
  - Built on MailKit and MSAL, with an IMAP connection pool split into foreground and background leases.
  - A thorough connectivity model: debounced offline detection, reconnect with backoff, and transport failures told apart from server rejections (`ConnectionFailure`).
  - An offline outbox, an FTS5 search index, and credentials in Credential Manager.
  - The strictest HTML policy of all candidates: remote images are blocked outright.
  - Account autodiscovery that respects privacy: a local catalogue first, online tiers can be turned off, and plaintext authentication is refused.
  - No telemetry SDKs. The only outbound non-mail calls are the GitHub update check, Mozilla autoconfig, DNS-over-HTTPS (Cloudflare) for autodiscovery, and the optional bug-report relay.
  - A strong accessibility record, which serves PRD §11.
  - The MIT licence leaves MailBox MatriX free to choose its own licence, provided the MIT notice is kept.
- **Material problems:**
  1. **The UI layer is the most monolithic part of the code, and it is exactly what MailBox MatriX replaces.** `Views/MainWindow.xaml.cs` has 8,322 lines and `ViewModels/MainViewModel.cs` has 10,984 **[src]**. The redesign will mean careful surgery, not just restyling.
  2. **The UI conventions are screen-reader- and keyboard-first:** classic three panes, tab strip, tree view, command palette **[src]**. They are good for accessibility but visually far from Windows 10 Mail. The shell and message row need rebuilding **[est]**. WPF can do everything the design needs: image-filled rows with gradients, square corners (the WPF default), custom window chrome. The acrylic-style blurred sidebar is the hardest part in WPF, so a pre-blurred image is the practical route **[est]**.
  3. **Gmail OAuth is effectively unavailable upstream** (#369), and Microsoft accounts default to IMAP rather than Graph **[src]**. Any fork needs its own verified Google OAuth client regardless of base, so this is not unique to QuickMail. But QuickMail's Gmail OAuth path has had less real-world use.
  4. **Young project with one maintainer:** five months old, released as v0.8.x **[api]**. The rapid change makes upstream merges costly, though the MIT licence makes diverging easy.
  5. **More scope than MailBox MatriX needs:** calendar, CalDAV/CardDAV, contacts, rules, templates, Markdown compose, saved views **[src]**. It can be hidden; there is no backend to strip.
- **Verdict [est]:** still suitable. No disqualifying problem found. Its main cost is rebuilding the UI.

**Skim:** a high-quality, privacy-respecting core (Rust, keyring, ammonia, FTS5) with a lean scope. **Rejected on PRD §1/§5.1:** the entire UI is a web app in WebView2 **[src]**. Its AI assistant and co-writer conflict with PRD §3 but are optional and removable. It is the strongest option *if* you ever relaxed the native-UI requirement, both for styling ease and for Android (Tauri 2 mobile).

**Pelton:** cross-platform with a broad feature set. **Rejected on PRD §1/§5.1** (Svelte in a Wails webview) **[src]**. It also includes an **MCP server that exposes mailbox data to AI agents** and a VirusTotal integration **[src]**, both of which would need review or removal under PRD §8.2/§9. It is GPL-3.0 and its core is Go, which no other part of the MailBox MatriX direction uses.

**MailAggregator:** native WPF with a clean `Core`/`Desktop` split **[src]**. **Rejected on licence:** PolyForm Noncommercial 1.0.0 is not an open-source licence and prohibits commercial use **[src]**. Publishing a fork would be restricted, and paid distribution (including a paid Store listing) would be ruled out. It is also small, uses the unmaintained-looking ModernWpfUI 0.9.6, has no installer, and saw no commits after 2026-05-14 **[api]**.

**Wino Mail (comparison only):** see `docs/ARCHITECTURE_AUDIT.md`. It has the closest existing UI to the design, the most mature codebase and a modular core. Against it: GPL-3.0; **a commercial backend woven into its core** (Wino account, billing, cloud AI that uploads message bodies, Sentry), with about 90 files plus package references in the synchronizers to remove; plaintext credentials on Windows; and the heaviest build (Native AOT, MSIX, pinned SDK). The upstream project is heading toward a paid, account-based model that diverges from MailBox MatriX's goals **[est]**.

### 2.3 Is the shortlist sufficient?

Yes, with one note. **Wino Mail was not on the original shortlist and is the only plausible addition** I identified. It is the one candidate whose *existing UI* already resembles Windows 10 Mail. I am not proposing to swap it in. It is listed as an alternative because it trades lower UI effort for higher removal and security work and a copyleft licence (§3). I did no further broad searching: the well-known alternatives are web-based (Mailspring, Electron), very large and non-.NET (Thunderbird), or proprietary (eM Client), so none fits better **[est, not researched in depth here]**.

---

## 3. Strongest candidate and evidence

**Recommendation: QuickMail (`kellylford/QuickMail`) remains the strongest base**, ahead of Wino Mail, subject to the validation spike below.

| Factor | QuickMail | Wino Mail |
|---|---|---|
| Work to *remove* conflicting functionality | Low: hide calendar, contacts and rules; repoint or remove the bug relay | **High**: account, billing, cloud AI and telemetry are entangled with domain and synchronizer code |
| Security baseline | Already meets PRD §9 (Credential Manager, strict CSP, no telemetry) | Needs fixes before release (plaintext secrets, remote images on by default) |
| Licence flexibility | MIT, permissive | GPL-3.0, copyleft for the whole fork |
| Build and maintainability for a small team (PRD goal 6) | Plain `dotnet build` and Velopack; one app project | Native AOT, MSIX and 30+ projects; strict AOT binding rules |
| Work to reach the design | **High**: rebuild shell and row, inside monolithic files | **Lower**: themes, pivot and date headers exist |
| Maturity | Young, one maintainer | Five years, many contributors |

In short, QuickMail has **the right foundations (local-first, secure, permissively licensed, simple to build) and the wrong look**. Wino Mail has **the right look and the wrong foundations** for MailBox MatriX's privacy and no-backend goals. Changing the look is work MailBox MatriX has to do anyway. Removing a commercial backend from a sync core is work it should not have to do.

**What would change this recommendation:** the spike shows the WPF shell cannot reasonably be separated from `MainWindow`/`MainViewModel`, or that row images with virtualisation perform poorly. In that case, Wino Mail becomes the stronger choice despite its removal work.

### Proposed validation spike (after approval, before committing)
1. Install the .NET 10 SDK (needs your approval). Create the two credential stub files from `docs/*.example`. Build and run the tests unmodified, and record the results.
2. Connect test accounts: an Outlook.com account (via both IMAP and Graph) and Gmail with an app password. Check offline reading and the outbox.
3. Build a throwaway prototype of the MailBox MatriX sidebar and an image-backed row on a branch. Measure how much of `MainWindow.xaml(.cs)` it touches, and check scrolling on more than 5,000 messages.

---

## 4. Reasons to reject each candidate

| Candidate | Reasons |
|---|---|
| QuickMail | Monolithic main window and view model. Accessibility-first UI far from the design. Young, single-maintainer project with fast churn. Gmail OAuth blocked upstream. Scope wider than needed. |
| Skim | Web UI (PRD §1). AI assistant and co-writer (PRD §3). IMAP only, no Graph. Rust + Svelte stack. |
| Pelton | Web UI (PRD §1). MCP server and VirusTotal integrations (PRD §8.2/§9). Go stack. GPL-3.0. |
| MailAggregator | **PolyForm Noncommercial licence** (not open source). Small, inactive since May 2026. No installer. AI features. |
| Wino Mail | Commercial backend and cloud AI entangled in the core. Plaintext secrets on Windows. GPL-3.0. Heavy AOT/MSIX build. Upstream heading toward a paid model. |

---

## 5. Questions that need your decision

1. **Q1: Which QuickMail?** Confirm the original research meant `kellylford/QuickMail` (the upstream project) and not the `DinamicaTech/QuickMail` "Eudora QuickMail" fork. I recommend the upstream: it has more history and contributors, and the fork adds a third-party brand that refers to the Eudora name.
2. **Q2: Run the validation spike?** It needs approval to install the .NET 10 SDK (and later the Velopack CLI). No application code would change outside a throwaway branch.
3. **Q3: Fork licence.** QuickMail's MIT licence lets MailBox MatriX stay MIT or adopt another licence. Which do you prefer? This affects whether GPL code (for example Wino Mail snippets) could ever be borrowed.
4. **Q4: Wino Mail as the formal fallback?** Should it be recorded as the fallback if the spike fails, given it was not on the original shortlist?
5. **Q5: Existing audit.** `docs/ARCHITECTURE_AUDIT.md` was written against Wino Mail. If QuickMail is confirmed, should I rewrite it against QuickMail after the spike?

*No base project has been selected. No environment setup or implementation will start without your approval.*
