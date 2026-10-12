using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuickMail.Models;
using QuickMail.ViewModels;
using QuickMail.Views;
using Xunit;

namespace QuickMail.Tests;

/// <summary>
/// MailBox MatriX: Mark as Unread (Ctrl+U, context menu, command bar), the row envelope that toggles
/// read state, and clicking the open message again to close it.
/// </summary>
public class MarkUnreadTests
{
    private static readonly Guid Account = Guid.NewGuid();

    private static MailMessageSummary Msg(string id, bool read, string folder = "INBOX") => new()
    {
        MessageId = id, AccountId = Account, FolderName = folder, IsRead = read,
    };

    private static (MainViewModel vm, RecordingMail mail, RecordingStore store) MakeVm()
    {
        var mail = new RecordingMail();
        var store = new RecordingStore();
        var vm = new MainViewModel(
            mail, new StubAccountService(), new StubCredentialService(),
            store, new StubOAuthService(), new StubSyncService(), new StubConfigService(),
            new StubCommandRegistry(), new StubViewService(), new StubRuleService(), new StubSmtpService());
        return (vm, mail, store);
    }

    [Fact]
    public async Task MarkUnread_ClearsReadState_InTheList_TheStore_AndOnTheServer()
    {
        var (vm, mail, store) = MakeVm();
        var a = Msg("1", read: true);
        var b = Msg("2", read: true, folder: "Archive");

        await vm.MarkMessagesUnreadAsync([a, b]);

        Assert.False(a.IsRead);
        Assert.False(b.IsRead);
        Assert.Contains(store.Calls, c => !c.IsRead && c.Ids.SequenceEqual(new[] { "1", "2" }));
        Assert.Contains(("INBOX", "1"), mail.Unread);
        Assert.Contains(("Archive", "2"), mail.Unread);
        Assert.Equal("Marked 2 messages as unread.", vm.StatusText);
    }

    [Fact]
    public async Task MarkUnread_OnAlreadyUnreadMail_DoesNothing()
    {
        var (vm, mail, store) = MakeVm();

        await vm.MarkMessagesUnreadAsync([Msg("1", read: false)]);

        Assert.Empty(mail.Unread);
        Assert.Empty(store.Calls);
    }

    [Theory]
    [InlineData(true, false)]    // read → envelope click → unread
    [InlineData(false, true)]    // unread → envelope click → read
    public async Task Envelope_TogglesReadState(bool before, bool after)
    {
        var (vm, _, _) = MakeVm();
        var m = Msg("1", read: before);

        await vm.ToggleMessagesReadAsync([m]);

        Assert.Equal(after, m.IsRead);
    }

    [Fact]
    public async Task Envelope_OnAMixedSelection_MarksAllRead()
    {
        var (vm, _, _) = MakeVm();
        var read = Msg("1", read: true);
        var unread = Msg("2", read: false);

        await vm.ToggleMessagesReadAsync([read, unread]);

        Assert.True(read.IsRead);
        Assert.True(unread.IsRead);
    }

    [Theory]
    [InlineData(MessageOpenMode.ReadingPane, true,  true,  true,  true)]   // the open message, clicked again: close
    [InlineData(MessageOpenMode.ReadingPane, true,  true,  false, false)]  // a different message: switch to it
    [InlineData(MessageOpenMode.ReadingPane, false, false, true,  false)]  // nothing open: open it
    [InlineData(MessageOpenMode.ReadingPane, true,  false, true,  false)]  // still loading: let it finish
    [InlineData(MessageOpenMode.Tab,         true,  true,  true,  false)]  // other modes keep their behaviour
    [InlineData(MessageOpenMode.Window,      true,  true,  true,  false)]
    public void ClickingTheOpenMessageAgain_ClosesIt_OnlyInTheReadingPane(
        MessageOpenMode mode, bool isOpen, bool isLoaded, bool same, bool expectClose)
        => Assert.Equal(expectClose, MainWindow.ShouldCloseOnReclick(mode, isOpen, isLoaded, same));

    private sealed class RecordingMail : StubImapMailServiceBase
    {
        public List<(string Folder, string Id)> Unread { get; } = [];

        public override Task MarkUnreadBatchAsync(Guid accountId, string folderName, IList<string> messageIds, CancellationToken ct = default)
        {
            foreach (var id in messageIds) Unread.Add((folderName, id));
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingStore : StubLocalStoreService
    {
        public List<(bool IsRead, List<string> Ids)> Calls { get; } = [];

        public override Task UpdateIsReadBatchAsync(IEnumerable<(Guid AccountId, string FolderName, string MessageId)> items, bool isRead)
        {
            Calls.Add((isRead, items.Select(i => i.MessageId).ToList()));
            return Task.CompletedTask;
        }
    }
}
