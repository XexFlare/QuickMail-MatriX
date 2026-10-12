using System;
using System.Collections.Generic;
using System.Linq;
using QuickMail.Models;
using QuickMail.ViewModels;
using QuickMail.Views;
using Xunit;

namespace QuickMail.Tests;

/// <summary>
/// MailBoXX sidebar (step b): the account's main folders, the provider badges and the folder
/// names and icons.
/// </summary>
public class SidebarTests
{
    private static readonly Guid Outlook = Guid.Parse("de300a1a-0000-4000-8000-000000000001");
    private static readonly Guid Gmail = Guid.Parse("de300a1a-0000-4000-8000-000000000002");

    private static FolderTreeNode Folder(Guid account, string name, SpecialFolderKind kind, int unread = 0) => new()
    {
        Label = name,
        Folder = new MailFolderModel { AccountId = account, FullName = name, DisplayName = name, Kind = kind, UnreadCount = unread },
    };

    private static FolderTreeNode Account(Guid id, string label, params FolderTreeNode[] children)
    {
        var node = new FolderTreeNode { Label = label, IsHeader = true, AccountId = id };
        foreach (var c in children) node.Children.Add(c);
        return node;
    }

    /// <summary>Calendar and the aggregate views first, as in QuickMail's tree; then two accounts.</summary>
    private static List<FolderTreeNode> Tree()
    {
        var allMail = new FolderTreeNode { Label = "All Mail", IsHeader = true };
        allMail.Children.Add(Folder(Guid.Empty, "All Inboxes", SpecialFolderKind.Inbox));
        var nested = Folder(Outlook, "Projects", SpecialFolderKind.None);
        nested.Children.Add(Folder(Outlook, "Projects/Archive", SpecialFolderKind.None));
        return
        [
            new FolderTreeNode { Label = "Calendar", IsCalendarNode = true },
            allMail,
            Account(Outlook, "Outlook",
                Folder(Outlook, "Junk", SpecialFolderKind.Junk, 1),
                Folder(Outlook, "INBOX", SpecialFolderKind.Inbox, 5),
                nested,
                Folder(Outlook, "Sent", SpecialFolderKind.Sent),
                Folder(Outlook, "Trash", SpecialFolderKind.Trash),
                Folder(Outlook, "Drafts", SpecialFolderKind.Drafts),
                Folder(Outlook, "Archive", SpecialFolderKind.Archive)),
            Account(Gmail, "Gmail",
                Folder(Gmail, "INBOX", SpecialFolderKind.Inbox),
                Folder(Gmail, "[Gmail]/All Mail", SpecialFolderKind.AllMail)),
        ];
    }

    [Fact]
    public void MainFolders_AreTheAccountsOwn_InWindowsMailOrder()
    {
        var picked = MainViewModel.PickMainFolders(Tree(), Outlook);

        Assert.Equal(new[] { "Inbox", "Drafts", "Sent Items", "Archive", "Deleted Items", "Junk Email" },
                     picked.Select(MainFolderLabelConverter.LabelFor));
        Assert.All(picked, n => Assert.Equal(Outlook, n.Folder!.AccountId));
    }

    [Fact]
    public void MainFolders_SkipKindsTheAccountDoesNotHave()
    {
        var picked = MainViewModel.PickMainFolders(Tree(), Gmail);

        Assert.Equal("INBOX", Assert.Single(picked).Folder!.FullName);   // never the aggregate "All Inboxes"
    }

    [Fact]
    public void MainFolders_WithNoAccountSelected_UseTheFirstAccount()
        => Assert.Equal(6, MainViewModel.PickMainFolders(Tree(), null).Count);

    [Fact]
    public void MainFolders_AreTheTreesOwnNodes_SoCountsStayLive()
    {
        var tree = Tree();
        var inbox = MainViewModel.PickMainFolders(tree, Outlook)[0];

        Assert.Same(tree[2].Children[1], inbox);
        Assert.Equal("5", inbox.UnreadCountText);
        Assert.Equal("Inbox, 5 unread", MainFolderLabelConverter.AccessibleNameFor(inbox));
    }

    [Fact]
    public void MainFolders_UnknownAccount_IsEmpty()
        => Assert.Empty(MainViewModel.PickMainFolders(Tree(), Guid.NewGuid()));

    [Theory]
    [InlineData("xexin@outlook.test", "127.0.0.1", "outlook")]
    [InlineData("xexin.plays@gmail.test", "127.0.0.1", "gmail")]
    [InlineData("xexin@hotmail.test", "127.0.0.1", "hotmail")]
    [InlineData("someone@live.com", "outlook.office365.com", "hotmail")]
    [InlineData("me@company.example", "imap.gmail.com", "gmail")]
    [InlineData("me@company.example", "mail.company.example", "other")]
    public void ProviderBadge_ComesFromTheAddressAndServers(string user, string host, string expected)
        => Assert.Equal(expected, AccountProviderConverter.Classify(new AccountModel { Username = user, ImapHost = host }));

    [Theory]
    [InlineData(SpecialFolderKind.Inbox, "")]
    [InlineData(SpecialFolderKind.Drafts, "")]
    [InlineData(SpecialFolderKind.Sent, "")]
    [InlineData(SpecialFolderKind.Archive, "")]
    [InlineData(SpecialFolderKind.Trash, "")]
    [InlineData(SpecialFolderKind.Junk, "")]
    [InlineData(SpecialFolderKind.None, "")]
    public void FolderIcons(SpecialFolderKind kind, string glyph)
        => Assert.Equal(glyph, FolderGlyphConverter.GlyphFor(Folder(Outlook, "x", kind)));

    [Fact]
    public async System.Threading.Tasks.Task ActivatingAnAccount_OpensItsInbox()
    {
        var mail = new FoldersMail();
        var vm = new MainViewModel(
            mail, new StubAccountService(), new StubCredentialService(),
            new StubLocalStoreService(), new StubOAuthService(), new StubSyncService(), new StubConfigService(),
            new StubCommandRegistry(), new StubViewService(), new StubRuleService(), new StubSmtpService());
        var gmail = new AccountModel { Id = Gmail, AccountName = "Gmail", AuthType = AuthType.OAuth2Microsoft };

        await vm.OpenAccountAsync(gmail);

        Assert.Same(gmail, vm.SelectedAccount);
        Assert.Equal(Gmail, vm.SelectedFolder?.AccountId);
        Assert.Equal("INBOX", vm.SelectedFolder?.FullName);
    }

    [Fact]
    public async System.Threading.Tasks.Task AllMailButton_OpensTheCombinedView_AndShowsAsSelected()
    {
        var vm = new MainViewModel(
            new FoldersMail(), new StubAccountService(), new StubCredentialService(),
            new StubLocalStoreService(), new StubOAuthService(), new StubSyncService(), new StubConfigService(),
            new StubCommandRegistry(), new StubViewService(), new StubRuleService(), new StubSmtpService());
        await vm.OpenAccountAsync(new AccountModel { Id = Gmail, AccountName = "Gmail", AuthType = AuthType.OAuth2Microsoft });
        Assert.False(vm.IsAllMailOpen);

        await vm.OpenAllMailCommand.ExecuteAsync(null);

        Assert.Equal(MainViewModel.AllMailFolder.FullName, vm.SelectedFolder?.FullName);
        Assert.True(vm.IsAllMailOpen);
    }

    private sealed class FoldersMail : StubImapMailServiceBase
    {
        public override System.Threading.Tasks.Task<List<MailFolderModel>> GetFoldersAsync(
            Guid accountId, System.Threading.CancellationToken ct = default)
            => System.Threading.Tasks.Task.FromResult(new List<MailFolderModel>
            {
                new() { AccountId = accountId, FullName = "Sent", DisplayName = "Sent", Kind = SpecialFolderKind.Sent },
                new() { AccountId = accountId, FullName = "INBOX", DisplayName = "Inbox", Kind = SpecialFolderKind.Inbox },
            });
    }
}
