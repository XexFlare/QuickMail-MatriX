# MailBox MatriX

A local-first desktop email client for Windows, inspired by the layout and calm of the
Windows 10 Mail app. It has a blue sidebar, a compact message list with a picture behind each row,
and messages that open on a translucent surface over a scenic background.

MailBox MatriX is a fork of **[QuickMail](https://github.com/kellylford/QuickMail)** by Kelly Ford (MIT).
QuickMail supplies the proven parts: mail protocols, sync, offline cache, search, security and
accessibility. MailBox MatriX replaces the look and removes what a small, local-first client does not need.

> **Status:** development build. It is not yet released or signed. See
> `docs/IMPLEMENTATION_STATUS.md` for what is done, what is verified, and what is pending.

## Highlights

- **Accounts:** Outlook.com, Hotmail and Microsoft 365, Gmail, iCloud and any IMAP/SMTP or POP3
  server, as many as you like.
- **Offline:** local SQLite cache, offline reading, an outbox that sends when you are back online,
  and full-text search.
- **Safe HTML:** scripts and remote content in messages are blocked by default.
- **Credentials:** passwords are kept in Windows Credential Manager. Microsoft sign-in tokens are kept
  in a DPAPI-protected cache.
- **Contextual pictures** behind message rows. They are chosen **on your computer** from the sender
  and subject only. No message body is read and nothing is sent anywhere. Turn them off under
  View → Pictures Behind Messages.
- **Keyboard:** the classic menu bar is hidden until you press **Alt**. Every QuickMail shortcut and
  the command palette (**Ctrl+Shift+P**) still work.
- **Themes:** MailBox MatriX Light and Dark follow Windows. Windows High Contrast is honoured, and decorative
  pictures are turned off under it.

## Build and run

Requirements: Windows 10/11 and the .NET 10 SDK.

```powershell
dotnet build QuickMail.sln -c Debug
dotnet run --project QuickMail -c Debug
dotnet test QuickMail.Tests/QuickMail.Tests.csproj -c Debug
dotnet publish QuickMail/QuickMail.csproj -c Release -o publish   # -> publish\MailBoxMatriX.exe
```

See `docs/MAILBOXX_BUILD.md` for integration tests, offline UI captures, packaging and the artwork
generator. See `docs/AUTHENTICATION.md` before signing in to real accounts.

## Repository layout

The C# projects keep QuickMail's names (`QuickMail/`, `QuickMail.Tests/`, namespace `QuickMail.*`)
so upstream fixes can still be merged. The shipped program is `MailBoxMatriX.exe`.

| Path | Contents |
|---|---|
| `QuickMail/` | The WPF application |
| `QuickMail/Services/ContextImages/` | MailBox MatriX's contextual-picture feature (isolated) |
| `QuickMail/Themes/BuiltIn/mailboxx-*.json` | MailBox MatriX themes |
| `Tools/MailBoXX.ArtGen/` | Generates MailBox MatriX's original artwork and icon |
| `Product_Requirements.md`, `Design Reference/` | MailBox MatriX product definition |
| `docs/` | MailBox MatriX docs (`ARCHITECTURE_AUDIT.md`, `IMPLEMENTATION_STATUS.md`, …) plus QuickMail's own docs |

## Licence

MIT. See `LICENSE` and `THIRD-PARTY-NOTICES.md`. QuickMail's original README is kept at
`docs/upstream-QuickMail-README.md`.
