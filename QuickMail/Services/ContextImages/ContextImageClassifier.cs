using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuickMail.Services.ContextImages;

/// <summary>
/// MailBox MatriX contextual images, step 1: decide which picture category suits a message, from the
/// sender's display name (or address) and the subject line ONLY — never the body, attachments or
/// anything else. Pure, deterministic and local: no network, no AI model. Unmatched mail gets no
/// category, which means the neutral row.
/// </summary>
public static class ContextImageClassifier
{
    /// <summary>Bumped whenever the rules change, so stored choices can be recomputed once.</summary>
    public const int Version = 1;

    /// <summary>Picture categories; the bundled art is Assets/ContextImages/&lt;category&gt;-&lt;n&gt;.jpg.</summary>
    public static readonly IReadOnlyList<string> Categories = new[]
    {
        "travel", "shopping", "finance", "work", "events", "tech", "food", "fitness",
        "photography", "news", "security", "delivery", "social", "entertainment",
    };

    // Ordered: the first category whose rule matches wins, so the more specific ones come first
    // (a "security alert from your bank" is about security, a "flight receipt" about travel).
    private static readonly (string Category, Regex Rule)[] Rules =
    {
        ("security",      Words("password", "sign-in", "sign in", "login", "log in", "verification code", "verify your", "security alert",
                                "two-factor", "2fa", "suspicious", "account locked", "reset your")),
        ("delivery",      Words("shipped", "shipping", "delivery", "delivered", "out for delivery", "tracking", "parcel", "package",
                                "courier", "dispatch", "dispatched", "your order is on its way")),
        ("travel",        Words("flight", "flights", "boarding", "airline", "airport", "hotel", "lodge", "booking", "itinerary",
                                "trip", "travel", "vacation", "holiday", "safari", "check-in", "rail", "train", "cruise")),
        ("finance",       Words("invoice", "receipt", "payment", "paid", "bank", "statement", "balance", "transfer", "refund",
                                "tax", "salary", "payroll", "billing", "bill", "credit card", "finance", "ledger", "budget")),
        ("events",        Words("invitation", "invite", "rsvp", "event", "webinar", "conference", "meetup", "ticket", "tickets",
                                "agenda", "meeting", "appointment", "reminder", "calendar", "party", "wedding", "birthday")),
        ("photography",   Words("photo", "photos", "photography", "camera", "lens", "picture", "pictures", "album", "gallery",
                                "photo walk", "print")),
        ("fitness",       Words("workout", "fitness", "gym", "run", "running", "training", "coach", "steps", "health",
                                "yoga", "marathon", "cycling", "wellness")),
        ("food",          Words("recipe", "recipes", "restaurant", "dinner", "lunch", "breakfast", "menu", "delivery order",
                                "takeaway", "food", "grocery", "groceries", "kitchen", "coffee", "bakery")),
        ("entertainment", Words("movie", "movies", "film", "series", "episode", "season", "stream", "streaming", "watch",
                                "music", "album release", "concert", "playlist", "game", "games", "premiere", "trailer")),
        ("shopping",      Words("sale", "deal", "deals", "offer", "discount", "coupon", "order", "cart",
                                "shop", "shopping", "store", "new arrivals", "black friday", "voucher")),
        ("tech",          Words("software", "update", "release", "build", "deploy", "server", "api", "developer", "github",
                                "app", "device", "laptop", "beta", "cloud", "outage", "status")),
        ("social",        Words("friend", "friends", "follower", "followers", "mentioned you", "commented", "liked", "message from",
                                "connection", "network", "community", "group", "chat")),
        ("news",          Words("newsletter", "digest", "news", "weekly", "daily", "bulletin", "update from", "roundup",
                                "headlines", "briefing", "edition")),
        ("work",          Words("project", "report", "review", "proposal", "deadline", "team", "planning", "quarterly",
                                "q1", "q2", "q3", "q4", "contract", "office", "hiring", "interview", "notes")),
    };

    /// <summary>
    /// The category for a message, and the rule that chose it — or (null, null) for the neutral row.
    /// </summary>
    public static (string? Category, string? Rule) Classify(string? from, string? subject)
    {
        var text = $"{Normalize(from)} {Normalize(DomainWords(from))} {Normalize(subject)}";
        if (string.IsNullOrWhiteSpace(text)) return (null, null);
        foreach (var (category, rule) in Rules)
        {
            var m = rule.Match(text);
            if (m.Success) return (category, m.Value.Trim().ToLowerInvariant());
        }
        return (null, null);
    }

    /// <summary>Which of a category's bundled pictures a message gets: stable for the same key.</summary>
    public static int Variant(string messageKey, int variants)
    {
        // FNV-1a, so the choice never changes between runs (string.GetHashCode is randomized).
        uint hash = 2166136261;
        foreach (var c in messageKey)
        {
            hash ^= c;
            hash *= 16777619;
        }
        return (int)(hash % (uint)Math.Max(1, variants)) + 1;
    }

    private static string Normalize(string? s) => (s ?? string.Empty).ToLowerInvariant();

    /// <summary>"news@cameraclub.test" → "cameraclub" (the domain's leading labels), so a sender
    /// shown only by address still says something about itself.</summary>
    private static string DomainWords(string? from)
    {
        if (string.IsNullOrEmpty(from)) return string.Empty;
        var at = from.LastIndexOf('@');
        if (at < 0) return string.Empty;
        var domain = from[(at + 1)..].Trim(' ', '>', '"');
        var labels = domain.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return labels.Length <= 1 ? domain : string.Join(' ', labels.Take(labels.Length - 1));
    }

    private static Regex Words(params string[] phrases) =>
        new(@"(?<![\p{L}\p{N}])(" + string.Join("|", phrases.Select(Regex.Escape)) + @")(?![\p{L}\p{N}])",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
