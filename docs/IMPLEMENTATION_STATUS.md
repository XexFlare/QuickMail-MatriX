# MailBox MatriX: Implementation status

**Date:** 2026-10-11 · **Base:** QuickMail `f51602e` (kellylford/QuickMail, MIT), remote `upstream`
· **Branch:** `mailboxx` (all changes **uncommitted**; nothing pushed or published)

## 1. Summary

MailBox MatriX is a usable Windows email client in the Windows 10 Mail-inspired design. It reuses all of
QuickMail's mail, sync, storage, search, security and accessibility code. The work changed the
presentation layer, the app identity and one authentication dependency, and added an isolated
contextual-picture feature.

| Check | Result (final run) |
|---|---|
| Build (`QuickMail.sln`, Debug) | 0 errors. 3,726 warnings: upstream's 3,712 plus 14 test-name style warnings (CA1707) from new tests that follow upstream's naming |
| Unit tests | **4,796 passed, 0 failed, 16 skipped** (upstream baseline: 4,741/0/16; 55 tests added) |
| Integration tests (local GreenMail/Radicale) | **38 passed, 2 failed, 9 skipped**. Both failures are a Radicale CalDAV flake that predates the UI work; all IMAP/SMTP/POP3 tests pass |
| Live end-to-end against local GreenMail | Debug and published builds connected, synced and listed 4 seeded messages with the correct pictures |
| Development package | `publish\MailBoxMatriX.exe` (self-contained single file, win-x64) |

## 2. Decisions applied

| Decision | Implementation |
|---|---|
| **D4 reading view** | An opened message shows in the scenic main area on a ~90%-opaque surface (`Theme.ReadingSurface`: `#E6FFFFFF` light, `#E61F2023` dark), with the command bar and message tabs above it. QuickMail's reader and HTML pipeline are reused unchanged. **Trade-off:** WebView2 is a native window and cannot be blended by WPF, so the HTML body is opaque. Its colour matches the surface; the panel, header and margins are translucent. |
| **D12 Microsoft sign-in** | `Microsoft.Identity.Client.Desktop` was removed, taking `.Broker` and `.NativeInterop` with it (restrictive redistribution terms confirmed). Sign-in uses the system browser and the token cache is unchanged. See `docs/AUTHENTICATION.md`. |
| **D13 testing** | Java 17 and PowerShell 7 (portable, per-user) were installed and upstream's integration suite ran before the UI changes and at the end. No personal accounts were used. Real-provider testing awaits MailBox MatriX Entra and Google registrations and dedicated test accounts. |
| **D14 menu and toolbar** | The menu bar has **zero height until Alt** (or Alt+letter, F10), so all of QuickMail's access keys, the Alt-from-message-body handling and screen-reader paths still work. The toolbar became the **message command bar** (Reply, Reply all, Forward, Archive, Delete) in the reading area. New mail is in the sidebar, Sync and View mode are in the list header, and Sync range sits where the design shows "All". Every other command stays in the hidden menu, its shortcut and the command palette (Ctrl+Shift+P). |
| **Focused/Other** | Not implemented. **Three placeholder tabs** ("Tab 1/2/3") sit in that position, as requested: selectable and keyboard-accessible, with no filtering yet. Date-group headers are not implemented. |
| **Licence** | MailBox MatriX is MIT, with Kelly Ford's notice retained (`LICENSE`). Dependency licences were verified from package metadata (`THIRD-PARTY-NOTICES.md`). |

## 3. What changed, by phase

| Phase | Outcome | Main files |
|---|---|---|
| 0 Baseline | MailBox MatriX repository from upstream. A clean clone builds (credential-template fallback) | `QuickMail.csproj` |
| 1 Theme resources | 20 MailBox MatriX colour tokens (`listSurface` added in step (a)), `ImageryOpacity`, `ImagerySet`. MailBox MatriX Light and Dark themes (System follows them). High Contrast mappings. WCAG tests over worst-case backgrounds | `Theming/ThemeKeys.cs`, `Services/ThemeService.cs`, `Themes/BuiltIn/*.json`, `MailBoXXThemeTests.cs` |
| 2 Layout | Borderless window with its own caption buttons. Full-height blue sidebar over a blurred scenic image. List header. Scenic main area. Alt-revealed menu. Command bar. Status strip | `Views/MainWindow.xaml`, `Views/MainWindow.MailBoXX.cs` |
| 3 Message rows | Avatar, sender, subject, preview, time, attachment mark. Unread tint with a closed envelope; read row neutral with an open outlined envelope. Selection marker. Flag bar kept | `Views/MainWindow.xaml`, `Views/MailBoXXConverters.cs` |
| 4 Reading view | Header styling on the translucent surface | `Views/MainWindow.xaml` |
| 5 Authentication | MSAL Desktop/Broker/NativeInterop removed; system-browser sign-in | `Services/OAuthService.cs`, `docs/AUTHENTICATION.md` |
| Identity | `MailBoxMatriX.exe`. Data in `%AppData%\MailBoxMatriX`. Credential targets `MailBoxMatriX:*`. Single-instance, sign-in-at-startup and shortcut renamed. No update checks or bug reports go to QuickMail. About 130 visible strings rebranded | `AppIdentity.cs` and the identity sites listed in it |
| 6 Contextual images | Local, deterministic classifier (sender and subject only) with 14 categories. Choices persisted in a separate, bounded `context-images.db`. Light and dark picture sets. Neutral fallback, off switch (View → Pictures Behind Messages), off under High Contrast | `Services/ContextImages/*`, `Assets/ContextImages/*`, `ContextImageTests.cs` |
| 7 Packaging and docs | App icon, published exe, licence notices, README, build guide | `Assets/App/*`, `THIRD-PARTY-NOTICES.md`, `README.md`, `docs/MAILBOXX_BUILD.md` |
| Demo | Xexin's demo profile: three accounts (Outlook, Gmail, Hotmail-style; `.test` addresses) with 24 inbox messages each plus Sent, Drafts, Archive, Junk and Trash, seeded into local GreenMail. MailBox MatriX reads them through its normal sync and database. `preview\Run MailBox MatriX Demo.cmd` opens it | `scripts/mailboxx-demo/*` |
| Fix: All Mail | All Mail and per-account All Mail showed only one message per folder after the first refresh. An incremental "UIDs since" fetch (which, by IMAP rules, returns the newest-UID message even when nothing is new) was treated as a complete snapshot, so newer mail looked deleted on the server. Incrementally fetched folders are now skipped by the vanished-message check. Upstream bug | `ViewModels/MainViewModel.cs` (`FindVanishedMessages`), `VanishedMessagesTests.cs` |
| Step (b) sidebar | Hamburger (folds the sidebar to a rail), Accounts header with chevron, one-line account rows with a provider badge (MailBox MatriX's own coloured letters, not logos; `Theme.Provider*` tokens) and right-aligned unread count. **Main folders** list for the selected account — Inbox, Drafts, Sent Items, Archive, Deleted Items, Junk Email, each with an icon and right-aligned count — and **More**, which switches the same pane to QuickMail's full folder tree (All Mail, Calendar, saved views, every folder). F6 / Ctrl+1 / Ctrl+2 unfold whatever is folded. Profile: picture, name (the accounts' display name, "Xexin" in the demo), ▾ menu (Manage Accounts, Settings) and gear | `Views/MainWindow.xaml`, `Views/MainWindow.MailBoXX.cs`, `ViewModels/MainViewModel.MailBoXX.cs`, `Views/MailBoXXConverters.cs`, `SidebarTests.cs` |
| Step (c) rows | Time as Windows Mail showed it ("8:48 AM", "Mon 2:02 PM", "Fri 10/27"; visual only — screen readers keep the upstream date). Only the sender is bold on unread mail. Sender circles in six colours, stable per sender (`Theme.AvatarAlt1–5`) | `Models/MailMessageSummary.cs` (`FriendlyDate`), `Views/MainWindow.xaml`, `MessageRowFormatTests.cs` |
| Step (d) photos | The 28 row pictures are now photographs from Wikimedia Commons (CC0, public domain or CC BY; no share-alike or non-commercial), credited in `THIRD-PARTY-NOTICES.md`. No identifiable people; no prominent logos. Light set blended 50% toward white and dark set 65% toward the dark surface, so the time and envelope drawn over them stay readable. ArtGen no longer overwrites them (drawn art only with `--placeholder-context-art`) | `Assets/ContextImages/*`, `THIRD-PARTY-NOTICES.md`, `Tools/MailBoXX.ArtGen/Program.cs` |
| Accounts and All Mail | Clicking an account (or Enter on it) connects it and opens its Inbox; the app opens on the default account's Inbox when no startup folder is chosen (a chosen one still wins; All Mail if no Inbox is known yet). A **Mailboxes** button above Accounts opens the combined All Mail view; while any combined view is open the short folder list shows combined folders whose counts are the totals across accounts (Inbox 4 + 3 + 4 = 11), and choosing one opens it for every account. New combined **All Junk** view (QuickMail had none). Test fix: `LinkContextMenuTests.Open_ReportsEmptyOnSuccess` really opened the default browser on every run; it is now opt-in (`QUICKMAIL_RUN_PROCESS_TESTS=1`), like the other tests that launch programs. More space between the accounts and Folders | `ViewModels/MainViewModel.MailBoXX.cs` (`OpenAccountAsync`, `DefaultStartupInbox`, `OpenAllMail`), `Views/MainWindow.xaml`, `MainViewModelStartupTests.cs`, `SidebarTests.cs` |
| Feedback batch 1 | Open message fills the whole right pane (surface from the top, caption buttons over it). Close button on the command bar, and clicking the open message again closes it (Escape / Ctrl+W already did). **Mark as Unread**: new `IMailService.MarkUnreadBatchAsync` (IMAP clears \Seen, Graph `isRead=false`, POP local) with Ctrl+U (also from inside the message body), context menu (with Mark as Read), command bar, and the row envelope as a read/unread toggle. Flag strip takes space only on flagged rows. Sidebar blur 40 → 27 | `Views/MainWindow.xaml`, `Views/MainWindow.MailBoXX.cs`, `Services/*MailService.cs`, `ViewModels/MainViewModel.cs`, `MarkUnreadTests.cs` |
| Step (a) | One picture behind the whole window. Sidebar is blue glass (75% tint) over a blurred, aligned copy. List header opaque; list body (`Theme.ListSurface`) and reading surface ~90%. Window opens centred inside the work area. Caption glyphs sit on the picture with a halo (no patch). Invisible splitter. Columns 17 / 39 / 44 % | `Views/MainWindow.xaml`, `Views/MainWindow.MailBoXX.cs`, theme JSONs, `MailBoXXThemeTests.cs` |

The scenic background is a CC BY 2.0 photograph of the Serengeti kopjes by Güldem Üstün (credited in
`THIRD-PARTY-NOTICES.md`). The 56 row pictures and the icon are original and generated by
`Tools/MailBoXX.ArtGen`.

## 4. Verified, and how

- **Automated:** the full unit and integration suites (above). These include upstream's guards for
  hard-coded colours and font sizes, row accessibility (`RowSpeech`), menu access-key uniqueness and
  XAML parsing.
- **Offline UI captures** (`--ui-probe`, fixture data, no network): light, dark and 150% text,
  reading surface, contextual pictures in both themes.
- **Live sync** against local GreenMail with a dedicated dummy account: the Debug and the published
  exe synced, listed and pictured four messages. Verification used UI Automation and window-only
  `PrintWindow` captures, with no keyboard or mouse injection.
- **Hidden menu:** the app log shows the bar appearing when menu mode is entered and hiding when it is
  left.

## 5. Not yet verified (manual checks needed)

These need interactive keyboard or mouse use. I did not inject input on a desktop that was in use.

1. Opening a real HTML message in the reading view, including that remote images and scripts stay
   blocked. The probe's fixture has no offline body (an upstream limitation). Upstream unit tests cover
   the HTML sanitiser and CSP.
2. Alt / Alt+F / F10 revealing the menu during normal use.
3. Delete, Archive, Reply and Compose through the new command bar against GreenMail.
4. Keyboard focus visibility on the blue sidebar, Narrator, and Windows High Contrast.
5. Window drag, double-click to maximize, Aero Snap, and maximized padding on multiple monitors and
   DPIs.

The scripts used for the live checks are in `scripts/mailboxx-e2e/`. Its `drive-interactive.ps1` can
automate items 1 and 3, but only when nobody is using the desktop.

## 6. Known issues

| Issue | Origin | Notes |
|---|---|---|
| ~~Messages with an identical send timestamp collapse into one row~~ | Upstream | **Fixed.** The real cause was the All Mail vanished-message bug (see §3, "Fix: All Mail"). Worth reporting upstream |
| Sidebar glass is 75% tint, not the ~50% asked for | WCAG | At 50–70%, white sidebar text drops below 4.5:1 when the picture behind is bright. Tunable via `sidebarTint` |
| Attachment clip only appears once a message's details are fetched | Upstream | The list summary has no attachment flag before then |
| Radicale CalDAV integration test drops 1–2 responses per run | Test environment | IMAP/SMTP/POP3 unaffected |
| `DateTimeFieldEntryTests` intermittent under full-suite load | Upstream UI test | Passes in isolation |
| Conversations / From / To view modes still use upstream's tree rows | Not restyled | |
| Compose, Settings and other dialogs use the MailBox MatriX colours but keep QuickMail's layout | Not restyled | |
| The short folder list has no right-click menu | MailBox MatriX | QuickMail's folder actions resolve the folder from a tree item; they stay available in the full tree (More) and the Folder menu |
| A tab-order test (`AccountManagerTabOrderTests`) failed once under full-suite load | Test environment | Passed alone and in the next full run; focus timing |
| Contextual pictures are per category, not specific to each message | By design for v1 (privacy) | See `docs/ARCHITECTURE_AUDIT.md` §5 |
| Published exe is 306 MB | Upstream publish settings (self-contained + ReadyToRun) | Compression not evaluated |

## 7. Pending decisions and next steps

1. **Commit strategy.** Commit per phase on `mailboxx`, and optionally push to a private GitHub
   repository. Not done without your approval.
2. **MailBox MatriX app registrations:** Microsoft Entra (replace `UpstreamClientId`) and Google OAuth,
   plus dedicated Outlook.com and Gmail test accounts (D13).
3. **Row pictures:** optionally replace the drawn category art with free-licence photos.
4. **D9:** publisher name, code-signing account and a release feed. Then the Velopack installer,
   licence-collection step and CI rewrite.
5. Run the manual checks in §5, then decide whether the reading view needs the WebView2
   composition control for true translucency.
6. Wire the placeholder tabs to real filters, or relabel them.
7. **Custom folders in the short sidebar list** (e.g. a user's own "Explosions" folder): decided to leave them under
   More for now. If users ask, options are showing top-level custom folders below Junk Email, or
   right-click → "Add to sidebar" favourites (as Windows Mail did).

## 8. Test resources on this machine

- **Credential Manager:** the earlier dummy entries were removed. The demo adds three dummy entries
  (`MailBoxMatriX:de300a1a-…-0001` to `-0003`, password `demo`) for the local server only. Remove them with
  `cmdkey /delete:MailBoxMatriX:<id>`.
- **Demo profile:** `%LOCALAPPDATA%\MailBoxMatriX-demo` (separate from real data in `%AppData%\MailBoxMatriX`).
- **GreenMail:** running for the demo (in-memory: the demo mail is re-seeded automatically after a
  restart by `Run-Demo.ps1`). Radicale is stopped.
- **Portable dev tools (kept):** `%LOCALAPPDATA%\Microsoft\dotnet` (.NET SDK 10.0.401) and
  `%LOCALAPPDATA%\MailBoXX-devtools` (OpenJDK 17, PowerShell 7.6.6). Radicale 3.7.7 is installed in the user Python site-packages.
- **Prototype reference (unchanged):** `X:\Code\MailboXX-spike`.
