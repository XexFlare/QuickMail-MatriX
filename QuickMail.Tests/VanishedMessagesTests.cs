using System;
using System.Collections.Generic;
using System.Linq;
using QuickMail.Models;
using QuickMail.ViewModels;
using Xunit;

namespace QuickMail.Tests;

/// <summary>
/// MailBoXX regression: All Mail removed most of a folder's messages when an incremental
/// ("UIDs after the last seen") fetch returned only the highest-UID message and that message was
/// older than the others (mail appended, imported or migrated out of date order).
/// </summary>
public class VanishedMessagesTests
{
    private static readonly Guid Account = Guid.Parse("de300a1a-0000-4000-8000-000000000001");
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    private static MailMessageSummary Msg(string uid, int daysAgo, string folder = "INBOX") => new()
    {
        AccountId = Account,
        FolderName = folder,
        MessageId = uid,
        InternetMessageId = $"<{folder}-{uid}@demo.test>",
        Date = Now.AddDays(-daysAgo),
        Subject = $"message {uid}",
    };

    // UIDs 1..5 appended newest-first: UID 5 is the OLDEST message.
    private static List<MailMessageSummary> Shown() =>
        [Msg("1", 1), Msg("2", 3), Msg("3", 6), Msg("4", 10), Msg("5", 20), Msg("9", 2, "Archive")];

    [Fact]
    public void IncrementalFetch_ReturningOnlyTheHighestUid_RemovesNothing()
    {
        // "UID 6:*" with nothing new still returns UID 5 (IMAP semantics), the oldest message.
        var fetched = new List<MailMessageSummary> { Msg("5", 20) };
        var incremental = new[] { (Account, "INBOX") };

        var vanished = MainViewModel.FindVanishedMessages(Shown(), fetched, incremental);

        Assert.Empty(vanished);
    }

    [Fact]
    public void WithoutTheIncrementalHint_TheOldBehaviourWouldHaveRemovedNewerMail()
    {
        // Documents the defect: treated as a snapshot, the one-message result "proves" 4 messages gone.
        var fetched = new List<MailMessageSummary> { Msg("5", 20) };

        var vanished = MainViewModel.FindVanishedMessages(Shown(), fetched);

        Assert.Equal(new[] { "1", "2", "3", "4" }, vanished.Select(m => m.MessageId).OrderBy(x => x));
    }

    [Fact]
    public void CompleteFetch_StillDetectsMailDeletedOnTheServer()
    {
        // A full date-range fetch that no longer contains UID 2 means it was deleted elsewhere.
        var fetched = new List<MailMessageSummary> { Msg("1", 1), Msg("3", 6), Msg("4", 10), Msg("5", 20) };

        var vanished = MainViewModel.FindVanishedMessages(Shown(), fetched);

        Assert.Equal("2", Assert.Single(vanished).MessageId);
    }

    [Fact]
    public void OtherFolders_AreNeverJudgedByThisFolder()
    {
        var fetched = new List<MailMessageSummary> { Msg("1", 1), Msg("2", 3), Msg("3", 6), Msg("4", 10), Msg("5", 20) };

        var vanished = MainViewModel.FindVanishedMessages(Shown(), fetched);

        Assert.Empty(vanished);   // the Archive message was not fetched, but its folder wasn't either
    }
}
