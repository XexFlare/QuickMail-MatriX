using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickMail.Models;

namespace QuickMail.ViewModels;

/// <summary>
/// MailBoXX sidebar state: the selected account's main folders (Windows Mail's short folder list,
/// with "More" leading to QuickMail's full folder tree), and which sidebar parts are collapsed.
/// </summary>
public partial class MainViewModel
{
    /// <summary>The folders the short list shows, in Windows Mail's order.</summary>
    internal static readonly SpecialFolderKind[] MainFolderKinds =
    [
        SpecialFolderKind.Inbox, SpecialFolderKind.Drafts, SpecialFolderKind.Sent,
        SpecialFolderKind.Archive, SpecialFolderKind.Trash, SpecialFolderKind.Junk,
    ];

    /// <summary>
    /// The selected account's main folders. The same node objects as in <see cref="FolderTree"/>,
    /// so unread counts refreshed there show here too.
    /// </summary>
    public ObservableCollection<FolderTreeNode> MainFolders { get; } = [];

    /// <summary>True: the sidebar shows the full folder tree instead of the short list.</summary>
    [ObservableProperty]
    private bool _showAllFolders;

    [ObservableProperty]
    private bool _isAccountListExpanded = true;

    /// <summary>The hamburger: the sidebar folds to a narrow rail.</summary>
    [ObservableProperty]
    private bool _isSidebarCompact;

    /// <summary>True while the combined All Mail view is open: the sidebar's All Mail button shows as selected.</summary>
    public bool IsAllMailOpen =>
        string.Equals(SelectedFolder?.FullName, AllMailFolder.FullName, StringComparison.Ordinal);

    /// <summary>The sidebar's All Mail button: every account's mail in one list.</summary>
    [RelayCommand]
    private Task OpenAllMailAsync() => SelectFolderCommand.ExecuteAsync(AllMailFolder);

    [RelayCommand]
    private void ToggleAllFolders() => ShowAllFolders = !ShowAllFolders;

    [RelayCommand]
    private void ToggleAccountList() => IsAccountListExpanded = !IsAccountListExpanded;

    [RelayCommand]
    private void ToggleSidebarCompact() => IsSidebarCompact = !IsSidebarCompact;

    /// <summary>
    /// Picks the account's main folders out of the folder tree: the first folder of each kind in
    /// <see cref="MainFolderKinds"/> under that account's own branch (never the aggregate views).
    /// With no account, the first account in the tree.
    /// </summary>
    internal static List<FolderTreeNode> PickMainFolders(IEnumerable<FolderTreeNode> roots, Guid? accountId)
    {
        var branch = roots.FirstOrDefault(r => r.AccountId is { } id && (accountId is null || id == accountId));
        if (branch is null) return [];

        var nodes = Flatten(branch.Children)
            .Where(n => !n.IsHeader && n.Folder is { IsHeader: false } f && f.AccountId == branch.AccountId)
            .ToList();
        var picked = new List<FolderTreeNode>();
        foreach (var kind in MainFolderKinds)
            if (nodes.FirstOrDefault(n => n.Folder!.Kind == kind) is { } node)
                picked.Add(node);
        return picked;

        static IEnumerable<FolderTreeNode> Flatten(IEnumerable<FolderTreeNode> level)
        {
            foreach (var n in level)
            {
                yield return n;
                foreach (var c in Flatten(n.Children)) yield return c;
            }
        }
    }

    /// <summary>
    /// True while Mailboxes (All Mail) or one of the combined folders is open: the sidebar's folder
    /// list then shows the combined folders, with totals across accounts.
    /// </summary>
    public bool IsCombinedFolderView =>
        SelectedFolder is { } f && (IsAllMailOpen || IsFolderScopedAggregate(f.FullName));

    /// <summary>The combined folders, in the short list's order, and the kind each one gathers.</summary>
    private static readonly (SpecialFolderKind Kind, MailFolderModel Sentinel)[] CombinedFolderKinds =
    [
        (SpecialFolderKind.Inbox,   AllInboxesFolder), (SpecialFolderKind.Drafts, AllDraftsFolder),
        (SpecialFolderKind.Sent,    AllSentFolder),    (SpecialFolderKind.Archive, AllArchiveFolder),
        (SpecialFolderKind.Trash,   AllTrashFolder),   (SpecialFolderKind.Junk,   AllJunkFolder),
    ];

    /// <summary>
    /// One node per combined folder that at least one account has, its count the total of that
    /// folder's unread mail across accounts (Inbox: 4 + 3 + 4 = 11). The node's folder is a copy
    /// of the sentinel carrying the kind (for the label and icon) and the total; activating it
    /// opens the sentinel itself (<see cref="OpenMainFolderAsync"/>).
    /// </summary>
    internal List<FolderTreeNode> BuildCombinedFolders()
    {
        var nodes = new List<FolderTreeNode>();
        foreach (var (kind, sentinel) in CombinedFolderKinds)
        {
            var sources = FolderScopedAggregateSources(sentinel.FullName).ToList();
            if (sources.Count == 0) continue;
            nodes.Add(new FolderTreeNode
            {
                Label = sentinel.DisplayName,
                Folder = new MailFolderModel
                {
                    FullName = sentinel.FullName, DisplayName = sentinel.DisplayName, Kind = kind,
                    UnreadCount = sources.Where(s => !s.Folder.SuppressUnreadCount).Sum(s => s.Folder.UnreadCount),
                },
            });
        }
        return nodes;
    }

    /// <summary>
    /// Rebuilds <see cref="MainFolders"/> when the tree, the selected account or the open folder
    /// changes: the account's own folders, or the combined ones while Mailboxes is open. Counts
    /// alone changing updates the existing nodes, so the list (and keyboard focus in it) stays put.
    /// </summary>
    internal void RefreshMainFolders()
    {
        if (IsCombinedFolderView)
        {
            var fresh = BuildCombinedFolders();
            var sameShape = fresh.Count == MainFolders.Count && fresh.Zip(MainFolders).All(p =>
                p.Second.Folder is { } f && f.AccountId == Guid.Empty &&
                string.Equals(f.FullName, p.First.Folder!.FullName, StringComparison.Ordinal));
            if (sameShape)
            {
                foreach (var (f, live) in fresh.Zip(MainFolders))
                    if (live.Folder!.UnreadCount != f.Folder!.UnreadCount)
                    {
                        live.Folder.UnreadCount = f.Folder.UnreadCount;
                        live.NotifyUnreadChanged();
                    }
                return;
            }
            MainFolders.Clear();
            foreach (var n in fresh) MainFolders.Add(n);
            return;
        }

        var picked = PickMainFolders(FolderTree ?? [], SelectedAccount?.Id);
        if (picked.SequenceEqual(MainFolders)) return;
        MainFolders.Clear();
        foreach (var n in picked) MainFolders.Add(n);
    }

    /// <summary>Opens a short-list folder: an account folder as is, a combined one as its sentinel.</summary>
    public Task OpenMainFolderAsync(FolderTreeNode node)
    {
        if (node.Folder is not { IsHeader: false } folder) return Task.CompletedTask;
        var target = folder.AccountId == Guid.Empty
            ? AllVirtualFolders.FirstOrDefault(v => string.Equals(v.FullName, folder.FullName, StringComparison.Ordinal)) ?? folder
            : folder;
        return SelectFolderCommand.ExecuteAsync(target);
    }

    partial void OnFolderTreeChanged(ObservableCollection<FolderTreeNode> value) => RefreshMainFolders();

    /// <summary>The account's Inbox from its cached folder list; null before its folders are known.</summary>
    internal MailFolderModel? InboxOf(Guid accountId) =>
        _cachedFolders.TryGetValue(accountId, out var folders)
            ? folders.FirstOrDefault(f => f.Kind == SpecialFolderKind.Inbox && !f.IsHeader)
            : null;

    /// <summary>
    /// Where the app opens when no startup folder has been chosen: the default account's Inbox
    /// (Windows Mail), so the sidebar's account and folder agree with the list from the first frame.
    /// Null — and so All Mail, QuickMail's default — when that Inbox is not cached yet.
    /// </summary>
    private MailFolderModel? DefaultStartupInbox()
    {
        var account = Accounts.FirstOrDefault(a => a.IsDefault) ?? Accounts.FirstOrDefault();
        if (account is null || InboxOf(account.Id) is not { } inbox) return null;
        SelectedAccount = account;
        return inbox;
    }

    /// <summary>
    /// Activating an account (click or Enter): connect it as before, then open its Inbox, so the
    /// list shows that account's mail. The combined views stay available under More. Arrowing
    /// through the accounts still loads nothing.
    /// </summary>
    public async Task OpenAccountAsync(AccountModel? account)
    {
        if (account is null) return;
        await SelectAccountCommand.ExecuteAsync(account);
        if (SelectedAccount?.Id == account.Id && InboxOf(account.Id) is { } inbox)
            await SelectFolderCommand.ExecuteAsync(inbox);
    }
}
