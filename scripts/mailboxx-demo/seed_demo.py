"""Load the MailBox MatriX demo mailboxes (demo_data.py) into the local GreenMail test server.

    python -I seed_demo.py            # seed any demo account that is still empty
    python -I seed_demo.py --reset    # wipe and re-seed all demo accounts

Talks only to 127.0.0.1 (GreenMail: IMAP 3143, authentication disabled). Messages are appended with
their flags (read/unread, flagged, draft) and dates, into the folder names MailBox MatriX recognises:
INBOX, Sent, Drafts, Archive, Junk, Trash.
"""
import imaplib, os, sys, time
from email.message import EmailMessage
from email.utils import formatdate, make_msgid

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import demo_data as D

HOST, PORT = "127.0.0.1", 3143
MARKER = "X-MailBoxMatriX-Demo"
FOLDERS = ["Sent", "Drafts", "Archive", "Junk", "Trash"]

NEWSLETTER = """<!doctype html><html><body style="margin:0;font-family:'Segoe UI',Arial,sans-serif;background:#f3f5f8">
<table width="100%" cellpadding="0" cellspacing="0"><tr><td align="center" style="padding:24px">
<table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff">
<tr><td style="background:{color};color:#ffffff;padding:18px 24px;font-size:20px;font-weight:600">{brand}</td></tr>
<tr><td style="padding:24px;color:#1b1b1b;font-size:15px;line-height:1.5">
<h2 style="margin:0 0 12px 0;font-size:20px">{subject}</h2><p style="margin:0 0 16px 0">{body}</p>
<p style="margin:0"><a href="https://{domain}/" style="background:{color};color:#ffffff;padding:10px 18px;text-decoration:none">Read more</a></p>
</td></tr>
<tr><td style="padding:12px 24px;color:#6b7280;font-size:12px">You're receiving this because you subscribed. This is demo mail for MailBox MatriX.</td></tr>
</table></td></tr></table>
<img src="https://{domain}/open.gif" width="1" height="1" alt="">
</body></html>"""
COLORS = ["#2F6FC4", "#5B3FA6", "#2E7D5B", "#C2572B", "#8E3A6B", "#1F4E8C"]

def tiny_pdf(title):
    text = f"(MailBox MatriX demo: {title})".encode("latin-1", "replace")
    stream = b"BT /F1 14 Tf 72 720 Td " + text + b" Tj ET"
    objs = [b"<< /Type /Catalog /Pages 2 0 R >>", b"<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            b"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            b"<< /Length %d >>\nstream\n" % len(stream) + stream + b"\nendstream",
            b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"]
    out, offsets = b"%PDF-1.4\n", []
    for i, o in enumerate(objs, 1):
        offsets.append(len(out)); out += b"%d 0 obj\n" % i + o + b"\nendobj\n"
    xref = len(out)
    out += b"xref\n0 %d\n0000000000 65535 f \n" % (len(objs) + 1) + b"".join(b"%010d 00000 n \n" % o for o in offsets)
    out += b"trailer\n<< /Size %d /Root 1 0 R >>\nstartxref\n%d\n%%%%EOF\n" % (len(objs) + 1, xref)
    return out

def invite(subject, start):
    s = time.strftime("%Y%m%dT%H%M%SZ", time.gmtime(start))
    e = time.strftime("%Y%m%dT%H%M%SZ", time.gmtime(start + 7200))
    return ("BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//MailBox MatriX demo//EN\r\nMETHOD:REQUEST\r\nBEGIN:VEVENT\r\n"
            f"UID:{make_msgid(domain='demo.mailboxx.test')[1:-1]}\r\nDTSTAMP:{s}\r\nDTSTART:{s}\r\nDTEND:{e}\r\n"
            f"SUMMARY:{subject}\r\nEND:VEVENT\r\nEND:VCALENDAR\r\n")

def build(acct_name, acct_addr, frm, to, subject, body, opts, when, n):
    m = EmailMessage()
    m["From"], m["To"], m["Subject"] = frm, to, subject
    m["Date"] = formatdate(when, localtime=True)
    m["Message-ID"] = make_msgid(domain="demo.mailboxx.test")
    m[MARKER] = "1"
    m.set_content(body + "\n")
    if "h" in opts:
        name, addr = frm.split(" <", 1)
        domain = addr.rstrip(">").split("@", 1)[1]
        m.add_alternative(NEWSLETTER.format(color=COLORS[n % len(COLORS)], brand=name, subject=subject,
                                            body=body, domain=domain), subtype="html")
    if "a" in opts:
        m.add_attachment(tiny_pdf(subject), maintype="application", subtype="pdf",
                         filename=subject.split(":")[0].replace("/", "-")[:40].strip() + ".pdf")
    if "i" in opts:
        m.add_attachment(invite(subject, when + 7 * 86400).encode(), maintype="text", subtype="calendar",
                         filename="invite.ics", params={"method": "REQUEST"})
    return m

def flags(opts, folder):
    f = []
    if folder in ("Sent", "Archive", "Trash") or "u" not in opts:
        f.append("\\Seen")
    if "f" in opts:
        f.append("\\Flagged")
    if folder == "Drafts":
        f = ["\\Draft", "\\Seen"]
    return "(" + " ".join(f) + ")"

def wipe(imap):
    for folder in ["INBOX"] + FOLDERS:
        if imap.select(folder)[0] != "OK":
            continue
        typ, data = imap.search(None, "ALL")
        if data and data[0]:
            imap.store(data[0].decode().replace(" ", ","), "+FLAGS", "\\Deleted")
            imap.expunge()

def seed_account(key, name, addr, now, reset):
    imap = imaplib.IMAP4(HOST, PORT)
    imap.login(addr, "demo")
    for folder in FOLDERS:
        imap.create(folder)                       # harmless if it exists
    imap.select("INBOX")
    typ, data = imap.search(None, "HEADER", MARKER, "1")
    if data and data[0] and not reset:
        print(f"{name}: already seeded ({len(data[0].split())} demo messages in Inbox)")
        imap.logout()
        return
    if reset:
        wipe(imap)
    me = f"{D.OWNER} <{addr}>"
    count = 0
    plan = [("INBOX", D.INBOX[key]),
            ("Archive", [(h, n, a, s, b, "") for h, n, a, s, b in D.ARCHIVE[key]]),
            ("Junk", [(h, n, a, s, b, "u") for h, n, a, s, b in D.JUNK[key]]),
            ("Trash", [(h, n, a, s, b, "") for h, n, a, s, b in D.TRASH[key]])]
    for folder, rows in plan:
        for hours, fname, faddr, subject, body, opts in rows:
            when = now - hours * 3600 - count * 7      # distinct timestamps, always
            msg = build(name, addr, f"{fname} <{faddr}>", me, subject, body, opts, when, count)
            imap.append(folder, flags(opts, folder), imaplib.Time2Internaldate(when), msg.as_bytes())
            count += 1
    for hours, tname, taddr, subject, body in D.SENT[key]:
        when = now - hours * 3600 - count * 7
        msg = build(name, addr, me, f"{tname} <{taddr}>", subject, body, "", when, count)
        imap.append("Sent", flags("", "Sent"), imaplib.Time2Internaldate(when), msg.as_bytes())
        count += 1
    for tname, taddr, subject, body in D.DRAFTS[key]:
        when = now - 1800 - count * 7
        msg = build(name, addr, me, f"{tname} <{taddr}>", subject, body, "", when, count)
        imap.append("Drafts", flags("", "Drafts"), imaplib.Time2Internaldate(when), msg.as_bytes())
        count += 1
    imap.logout()
    print(f"{name}: seeded {count} messages")

if __name__ == "__main__":
    reset = "--reset" in sys.argv
    now = time.time()
    for key, name, addr in D.ACCOUNTS:
        seed_account(key, name, addr, now, reset)
