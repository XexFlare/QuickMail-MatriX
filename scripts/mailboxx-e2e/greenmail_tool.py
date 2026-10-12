"""E2E helper for the local GreenMail test server (127.0.0.1, auth disabled, no TLS).
Usage:
  python greenmail_tool.py seed <address>     deliver the test messages over SMTP :3025
  python greenmail_tool.py inspect <address>  list folders, then each INBOX message's flags/subject over IMAP :3143
Only ever talks to 127.0.0.1. No real accounts, no credentials beyond GreenMail's dummy login."""
import imaplib, smtplib, sys
from email.message import EmailMessage
from email.utils import formatdate, make_msgid

HOST = "127.0.0.1"

def build(to):
    msgs = []

    m = EmailMessage()
    m["From"] = "Savanna Travel <offers@savanna-travel.test>"
    m["To"] = to
    m["Subject"] = "Your safari weekend: flights and lodges"
    m.set_content("Plain-text part: three nights in the Serengeti from 499.")
    m.add_alternative("""<html><body style="font-family:Segoe UI,sans-serif">
<h2 style="color:#245FA9">Safari weekend</h2>
<p>Three nights in the <b>Serengeti</b>, flights included.</p>
<ul><li>Lodge with lake view</li><li>Two guided drives</li></ul>
<p><a href="https://example.com/offer">See the offer</a></p>
<img src="https://tracker.example.com/pixel.gif?id=e2e" width="1" height="1" alt="">
<script>document.body.innerHTML='SCRIPT RAN'</script>
</body></html>""", subtype="html")
    msgs.append(m)

    m = EmailMessage()
    m["From"] = "Amina Okafor <amina@team.test>"
    m["To"] = to
    m["Subject"] = "Agenda for Thursday"
    m.set_content("Hi,\n\nHere is the agenda for Thursday's planning meeting.\n\n1. Budget\n2. Hiring\n3. Office move\n\nAmina")
    msgs.append(m)

    m = EmailMessage()
    m["From"] = "Finance Desk <finance@ledger.test>"
    m["To"] = to
    m["Subject"] = "Invoice 2026-118 attached"
    m.set_content("Please find invoice 2026-118 attached.")
    m.add_attachment(b"Invoice 2026-118\nAmount due: 1,250.00\n", maintype="text", subtype="plain",
                     filename="invoice-2026-118.txt")
    msgs.append(m)

    m = EmailMessage()
    m["From"] = "Camera Club <news@cameraclub.test>"
    m["To"] = to
    m["Subject"] = "Photo walk this Saturday"
    m.set_content("Join us for a photo walk along the river this Saturday morning. Bring a wide lens!")
    msgs.append(m)

    import time
    now = time.time()
    for i, msg in enumerate(msgs):
        msg["Date"] = formatdate(now - 60 * (len(msgs) - i), localtime=True)   # a minute apart
        msg["Message-ID"] = make_msgid(domain="e2e.mailboxx.test")
    return msgs

def seed(addr):
    with smtplib.SMTP(HOST, 3025, timeout=10) as s:
        for msg in build(addr):
            s.send_message(msg)
            print("sent:", msg["Subject"])

def inspect(addr):
    imap = imaplib.IMAP4(HOST, 3143)
    imap.login(addr, "greenmail-any-password")
    typ, folders = imap.list()
    print("folders:", [f.decode().split(' "/" ')[-1] for f in folders])
    for folder in ["INBOX"]:
        imap.select(folder, readonly=True)
        typ, data = imap.search(None, "ALL")
        for num in data[0].split():
            typ, d = imap.fetch(num, "(FLAGS BODY.PEEK[HEADER.FIELDS (SUBJECT)])")
            flags = d[0][0].decode()
            subj = d[0][1].decode().strip()
            print(f"{folder} #{num.decode()}: {flags[flags.find('FLAGS'):]}  {subj}")
    imap.logout()

if __name__ == "__main__":
    {"seed": seed, "inspect": inspect}[sys.argv[1]](sys.argv[2])
