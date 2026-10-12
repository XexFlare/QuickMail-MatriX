# MailBoXX end-to-end checks (local GreenMail, no real accounts)

These scripts exercise the real `MailBoXX.exe` against the local GreenMail server started by
`scripts\start-test-servers.ps1` (IMAP 127.0.0.1:3143, SMTP :3025, authentication disabled).

| Script | What it does | Touches the desktop? |
|---|---|---|
| `greenmail_tool.py seed <address>` | Delivers four test messages (HTML with a script and a remote tracking pixel, plain text, an attachment, a newsletter), one minute apart | No |
| `greenmail_tool.py inspect <address>` | Lists the server-side folders and each message's flags | No |
| `inspect.ps1 -Exe … -Profile … -OutPng … [-SelectFolder Inbox]` | Starts MailBoXX, reads the message list through UI Automation, saves a **window-only** `PrintWindow` image, closes the app | Opens a window (behind your work); no keystrokes or mouse |
| `drive-interactive.ps1 -Exe … -Profile … -OutDir …` | Opens messages with Enter, deletes one with Delete, captures the **screen** area of the window | **Yes: sends keystrokes and copies from the screen. Run only when nobody is using the desktop.** |

## Profile setup

1. Create a folder with an `accounts.json` for one IMAP account: host `127.0.0.1`, IMAP port 3143,
   SMTP port 3025, SSL off, the GreenMail address as `Username`. Add a `config.ini` with
   `TutorialCompleted = on`.
2. Store a **dummy** password where MailBoXX expects it:
   `cmdkey /generic:"MailBoXX:<account id>" /user:<address> /pass:anything`
3. `python -I greenmail_tool.py seed <address>`, then run `inspect.ps1`.
4. Afterwards, remove the dummy credential with `cmdkey /delete:"MailBoXX:<account id>"`.

Use a fresh GreenMail address per run. The tool sends messages a minute apart because upstream
collapses messages with identical timestamps in the list (see `docs/IMPLEMENTATION_STATUS.md` §6).
