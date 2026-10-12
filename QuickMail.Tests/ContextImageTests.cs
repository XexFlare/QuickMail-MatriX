using System;
using System.IO;
using System.Linq;
using QuickMail.Models;
using QuickMail.Services.ContextImages;
using Xunit;

namespace QuickMail.Tests;

/// <summary>MailBoXX contextual images: local, deterministic, sender + subject only, fail-safe.</summary>
public class ContextImageTests
{
    [Theory]
    [InlineData("Savanna Travel", "Your safari weekend: flights and lodges", "travel")]
    [InlineData("Finance Desk", "Invoice 2026-118 attached", "finance")]
    [InlineData("Camera Club", "Photo walk this Saturday", "photography")]
    [InlineData("Amina Okafor", "Agenda for Thursday", "events")]
    [InlineData("Courier Co", "Your parcel is out for delivery", "delivery")]
    [InlineData("Big Bank", "Security alert: new sign-in to your account", "security")]   // security beats finance
    [InlineData("info@travel-desk.test", "Hello", "travel")]                               // domain words count
    [InlineData("Weekly Digest", "This week in review", "news")]
    public void Classify_PicksTheExpectedCategory(string from, string subject, string expected)
    {
        var (category, rule) = ContextImageClassifier.Classify(from, subject);
        Assert.Equal(expected, category);
        Assert.False(string.IsNullOrEmpty(rule));
    }

    [Theory]
    [InlineData("Sam", "Hi")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void Classify_NoMatch_IsTheNeutralRow(string? from, string? subject)
    {
        Assert.Equal((null, null), ContextImageClassifier.Classify(from, subject));
    }

    [Fact]
    public void Classify_MatchesWholeWordsOnly()
    {
        // "travelling" is not the word "travel"; "billow" is not "bill".
        Assert.Null(ContextImageClassifier.Classify("Alex", "Billowing clouds").Category);
    }

    [Fact]
    public void Variant_IsStableAndInRange()
    {
        for (var i = 0; i < 200; i++)
        {
            var key = $"acct|msg-{i}@example.test";
            var v = ContextImageClassifier.Variant(key, ContextImageService.VariantsPerCategory);
            Assert.InRange(v, 1, ContextImageService.VariantsPerCategory);
            Assert.Equal(v, ContextImageClassifier.Variant(key, ContextImageService.VariantsPerCategory));
        }
    }

    [Fact]
    public void KeyFor_UsesTheMessageIdAcrossFolders_AndFallsBackToTheUid()
    {
        var account = Guid.NewGuid();
        var inInbox = new MailMessageSummary { AccountId = account, FolderName = "INBOX", MessageId = "7", InternetMessageId = "<ABC@x.test>" };
        var inLabel = new MailMessageSummary { AccountId = account, FolderName = "Work", MessageId = "99", InternetMessageId = "abc@x.test" };
        Assert.Equal(ContextImageService.KeyFor(inInbox), ContextImageService.KeyFor(inLabel));

        var noId = new MailMessageSummary { AccountId = account, FolderName = "INBOX", MessageId = "7" };
        Assert.Contains("|INBOX|7", ContextImageService.KeyFor(noId));
    }

    [Fact]
    public void Resolve_GivesTheBundledPicture_OrNullForNeutral_OrNullWhenTurnedOff()
    {
        using var service = new ContextImageService(store: null, assemblyName: "MailBoXX");
        var travel = Message("Savanna Travel", "Flights to Nairobi");
        var plain = Message("Sam", "Hi");

        var uri = service.ResolveImageUri(travel);
        Assert.NotNull(uri);
        Assert.StartsWith("pack://application:,,,/MailBoXX;component/Assets/ContextImages/light/travel-", uri);
        Assert.Contains("/ContextImages/dark/travel-", service.ResolveImageUri(travel, "dark"));
        Assert.EndsWith(".jpg", uri);
        Assert.Null(service.ResolveImageUri(plain));

        service.Enabled = false;
        Assert.Null(service.ResolveImageUri(travel));
    }

    [Fact]
    public void Choices_ArePersisted_AndReusedByANewSession()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"MBX-ContextImages-{Guid.NewGuid():N}");
        try
        {
            var message = Message("Finance Desk", "Invoice 77", "<inv-77@ledger.test>");
            string? first;
            using (var service = new ContextImageService(new ContextImageStore(dir), "MailBoXX"))
            {
                first = service.ResolveImageUri(message);
                service.FlushPendingSaves();
            }

            var stored = new ContextImageStore(dir).LoadAll();
            var choice = Assert.Single(stored.Values);
            Assert.Equal("finance", choice.Category);
            Assert.Equal("finance", choice.Rule);   // the sender name matches before the subject's "invoice"
            Assert.Equal(ContextImageService.BundledSource, choice.Source);
            Assert.Equal(ContextImageClassifier.Version, choice.SelectorVersion);

            using var again = new ContextImageService(new ContextImageStore(dir), "MailBoXX");
            Assert.Equal(first, again.ResolveImageUri(message));
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    public void Store_KeepsOnlyTheMessageKeyCategoryAndRule_NoMessageContent()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"MBX-ContextImages-{Guid.NewGuid():N}");
        try
        {
            using (var service = new ContextImageService(new ContextImageStore(dir), "MailBoXX"))
            {
                service.ResolveImageUri(Message("Savanna Travel", "Secret trip plans to Zanzibar", "<t1@x.test>"));
                service.FlushPendingSaves();
            }
            using var file = new FileStream(Path.Combine(dir, "context-images.db"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(file, System.Text.Encoding.UTF8);
            var text = reader.ReadToEnd();
            Assert.DoesNotContain("Zanzibar", text);
            Assert.DoesNotContain("Savanna Travel", text);
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    public void EveryCategory_HasItsBundledPictures()
    {
        var root = RepoRoot();
        foreach (var set in new[] { "light", "dark" })
            foreach (var category in ContextImageClassifier.Categories)
                for (var v = 1; v <= ContextImageService.VariantsPerCategory; v++)
                    Assert.True(File.Exists(Path.Combine(root, "QuickMail", "Assets", "ContextImages", set, $"{category}-{v}.jpg")),
                        $"missing Assets/ContextImages/{set}/{category}-{v}.jpg");
    }

    private static MailMessageSummary Message(string from, string subject, string? messageId = null) => new()
    {
        AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        FolderName = "INBOX",
        MessageId = "1",
        InternetMessageId = messageId ?? $"<{Guid.NewGuid():N}@x.test>",
        From = from,
        Subject = subject,
    };

    private static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "QuickMail", "Views")))
                return dir.FullName;
        throw new InvalidOperationException("repository root not found");
    }
}
