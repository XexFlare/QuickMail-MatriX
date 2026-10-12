using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using QuickMail.Models;
using QuickMail.Services;
using QuickMail.ViewModels;
using QuickMail.Services.ContextImages;

namespace QuickMail.Views;

/// <summary>
/// MailBox MatriX shell behaviour for the main window: the self-drawn caption (move, minimize,
/// maximize, close), the menu bar that stays out of sight until Alt, and the always-visible
/// search box. Kept apart from MainWindow.xaml.cs so upstream QuickMail code stays recognisable.
/// </summary>
public partial class MainWindow
{
    /// <summary>Called once from the constructor, after InitializeComponent.</summary>
    private void InitializeMailBoXX()
    {
        // The menu bar has zero height until its menus have keyboard focus (Alt, Alt+letter,
        // F10): every access key and screen-reader path QuickMail provides keeps working, but the
        // bar does not occupy the window the rest of the time.
        MainMenuBar.IsKeyboardFocusWithinChanged += (_, e) =>
            MainMenuBar.Height = (bool)e.NewValue ? double.NaN : 0;

        FitToWorkArea();
        UpdateMaximizeState();
        InitializeContextImages();
        InitializeSidebar();
    }

    /// <summary>
    /// Opens the window centred inside the work area. Without a system title bar, Windows' default
    /// cascade position could put the bottom of the window (status strip, last rows) under the
    /// taskbar on smaller or scaled screens.
    /// </summary>
    private void FitToWorkArea()
    {
        const double margin = 24;
        var area = SystemParameters.WorkArea;   // primary monitor, device-independent pixels
        Width = Math.Max(MinWidth, Math.Min(Width, area.Width - 2 * margin));
        Height = Math.Max(MinHeight, Math.Min(Height, area.Height - 2 * margin));
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = area.Left + Math.Max(0, (area.Width - Width) / 2);
        Top = area.Top + Math.Max(0, (area.Height - Height) / 2);
    }

    // ── Sidebar ─────────────────────────────────────────────────────────────

    private void InitializeSidebar()
    {
        _vm.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.IsSidebarCompact):
                    ApplySidebarCompact();
                    break;
                case nameof(MainViewModel.SelectedFolder):
                    Dispatcher.InvokeAsync(SyncMainFolderSelection, DispatcherPriority.Background);
                    break;
                case nameof(MainViewModel.ShowAllFolders):
                    // Keep keyboard focus in the folder area when it switches between the two lists.
                    var hadFocus = MainFolderList.IsKeyboardFocusWithin || FolderList.IsKeyboardFocusWithin
                                   || SidebarMoreFoldersButton.IsKeyboardFocused;
                    if (hadFocus) Dispatcher.InvokeAsync(FocusFolderTree, DispatcherPriority.Input);
                    break;
            }
        };
        _vm.MainFolders.CollectionChanged += (_, _) =>
            Dispatcher.InvokeAsync(SyncMainFolderSelection, DispatcherPriority.Background);
    }

    /// <summary>The hamburger: the sidebar folds to a rail of icons, and unfolds again.</summary>
    private void ApplySidebarCompact()
    {
        if (_vm.IsSidebarCompact)
        {
            SidebarColumn.MinWidth = 0;
            SidebarColumn.Width = new GridLength(52);
        }
        else
        {
            SidebarColumn.Width = new GridLength(17, GridUnitType.Star);
            SidebarColumn.MinWidth = 220;
        }
    }

    /// <summary>
    /// Keyboard paths into the sidebar (F6, Ctrl+1 / Ctrl+2) must never land on something folded
    /// away: unfold the rail and the account list first.
    /// </summary>
    private void RevealSidebarPane(int pane)
    {
        if (pane is not (1 or 2)) return;
        if (_vm.IsSidebarCompact) _vm.IsSidebarCompact = false;
        if (pane == 1 && !_vm.IsAccountListExpanded) _vm.IsAccountListExpanded = true;
        UpdateLayout();
    }

    /// <summary>
    /// With the short folder list showing, the folder pane is that list. Returns false when the
    /// full tree is showing, so the caller focuses the tree as before.
    /// </summary>
    private bool FocusMainFolderListInstead()
    {
        if (_vm.ShowAllFolders) return false;
        RevealSidebarPane(2);
        if (MainFolderList.Items.Count == 0)
        {
            // An account whose folders have not loaded yet: the full tree is the only folder pane.
            _vm.ShowAllFolders = true;
            return false;
        }
        var item = MainFolderList.SelectedItem ?? MainFolderList.Items[0];
        MainFolderList.SelectedItem = item;
        MainFolderList.UpdateLayout();
        if (MainFolderList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem container)
            container.Focus();
        else
            MainFolderList.Focus();
        return true;
    }

    /// <summary>Highlights the open folder in the short list, when it is one of them.</summary>
    private void SyncMainFolderSelection()
    {
        var open = _vm.SelectedFolder;
        var match = open is null ? null : _vm.MainFolders.FirstOrDefault(n =>
            n.Folder is { } f && f.AccountId == open.AccountId &&
            string.Equals(f.FullName, open.FullName, StringComparison.OrdinalIgnoreCase));
        if (!MainFolderList.IsKeyboardFocusWithin || match is not null)
            MainFolderList.SelectedItem = match;
    }

    private async void MainFolderList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (MouseActivation.RowFromClick(e.OriginalSource) is FolderTreeNode node)
            await _vm.OpenMainFolderAsync(node);
    }

    // Enter opens the folder and moves to its messages, as in the folder tree.
    private async void MainFolderList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None) return;
        if (MainFolderList.SelectedItem is FolderTreeNode { Folder: { IsHeader: false } } node)
        {
            e.Handled = true;
            await _vm.OpenMainFolderAsync(node);
            FocusActiveMessagePanel();
        }
    }

    private void SidebarProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (SidebarProfileButton.ContextMenu is not { } menu) return;
        menu.PlacementTarget = SidebarProfileButton;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
        menu.IsOpen = true;
    }

    // ── Contextual images ───────────────────────────────────────────────────

    /// <summary>
    /// Starts the contextual-image picker (local, sender + subject only). Any failure leaves every
    /// row neutral — pictures are never allowed to get in the way of mail.
    /// </summary>
    private void InitializeContextImages()
    {
        try
        {
            var profileDir = (Application.Current as App)?.Profile?.ProfileDir;
            var store = profileDir is null ? null : new ContextImageStore(profileDir);
            // Owned by the row converter for the app's lifetime; disposed (flushing pending
            // choices) when this window closes.
            var service = new ContextImageService(store, typeof(MainWindow).Assembly.GetName().Name ?? AppIdentity.TechnicalName)
            {
                Enabled = _configService.Load().ContextImages,
            };
            ContextImageBrushConverter.Service = service;
            ContextImagesMenuItem.IsChecked = service.Enabled;
            Closed += (_, _) =>
            {
                ContextImageBrushConverter.Service = null;
                service.Dispose();
            };
            // Light and dark themes use different picture sets: redraw the rows on a theme change.
            if (_themeService is not null)
                _themeService.ThemeChanged += (_, _) => Dispatcher.InvokeAsync(() => MessageList.Items.Refresh());
        }
        catch (Exception ex)
        {
            LogService.Log($"Contextual images unavailable: {ex.GetType().Name}");
            ContextImagesMenuItem.IsChecked = false;
            ContextImagesMenuItem.IsEnabled = false;
        }
    }

    private void MenuContextImages_Click(object sender, RoutedEventArgs e)
    {
        if (ContextImageBrushConverter.Service is not { } service) return;
        var on = ContextImagesMenuItem.IsChecked;
        service.Enabled = on;
        var cfg = _configService.Load();
        cfg.ContextImages = on;
        _configService.Save(cfg);
        MessageList.Items.Refresh();   // re-run the row converters
    }

    /// <summary>ui-probe hook: enter menu mode exactly as a bare Alt does.</summary>
    internal void EnterMenuModeForProbe()
    {
        MenuBarAccess.EnterMenuMode(MainMenuBar);
        LogService.Debug($"ui-probe menu-bar: active={IsActive} focusWithin={MainMenuBar.IsKeyboardFocusWithin} " +
                         $"focused={Keyboard.FocusedElement?.GetType().Name ?? "null"} height={MainMenuBar.Height}");
    }

    // ── Caption ─────────────────────────────────────────────────────────────

    private void CaptionDragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        try { DragMove(); }
        catch (InvalidOperationException) { /* mouse already released */ }
    }

    private void CaptionMinimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void CaptionMaximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void CaptionClose_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
        else SystemCommands.MaximizeWindow(this);
    }

    private void MbxWindow_StateChanged(object? sender, EventArgs e) => UpdateMaximizeState();

    /// <summary>
    /// A maximized borderless window extends past the screen by the resize border; pad the
    /// content back inside it, and show the right caption glyph and name.
    /// </summary>
    private void UpdateMaximizeState()
    {
        var maximized = WindowState == WindowState.Maximized;
        var border = SystemParameters.WindowResizeBorderThickness;
        MbxWindowFrame.Padding = maximized
            ? new Thickness(border.Left + 4, border.Top + 4, border.Right + 4, border.Bottom + 4)
            : new Thickness(0);
        CaptionMaximizeButton.Content = maximized ? "" : "";
        var name = maximized ? "Restore" : "Maximize";
        System.Windows.Automation.AutomationProperties.SetName(CaptionMaximizeButton, name);
        CaptionMaximizeButton.ToolTip = name;
    }

    // ── Reading pane ────────────────────────────────────────────────────────

    private void ToolbarCloseMessage_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.IsMessageOpen) CloseReadingPane();
    }

    /// <summary>
    /// Clicking the message that is already open in the reading pane closes it (click mail 1: it
    /// opens; click mail 2: it switches; click mail 2 again: it closes). Mouse only — Enter on the
    /// open message still opens it, and Escape / Ctrl+W close it from the keyboard.
    /// </summary>
    private bool CloseIfReclicked(MailMessageSummary summary)
    {
        if (!ShouldCloseOnReclick(_vm.MessageOpenMode, _vm.IsMessageOpen,
                                  _vm.MessageDetail is not null, ReferenceEquals(_vm.SelectedMessage, summary)))
            return false;
        CloseReadingPane();
        return true;
    }

    // ── Read / unread ───────────────────────────────────────────────────────

    /// <summary>The row envelope: toggles that row (or, when it is part of a multi-selection, the selection).</summary>
    private async void RowEnvelope_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: MailMessageSummary row }) return;
        e.Handled = true;   // a click on the envelope is not a click that opens the message
        var selected = MessageList.SelectedItems.OfType<MailMessageSummary>().ToList();
        await _vm.ToggleMessagesReadAsync(selected.Count > 1 && selected.Contains(row) ? selected : [row]);
    }

    private async void MessageContextMenu_MarkRead_Click(object sender, RoutedEventArgs e)
    {
        if (ContextMenuTargets() is { } targets) await _vm.MarkMessagesReadAsync(targets);
    }

    private async void MessageContextMenu_MarkUnread_Click(object sender, RoutedEventArgs e)
    {
        if (ContextMenuTargets() is { } targets) await _vm.MarkMessagesUnreadAsync(targets);
    }

    private async void ToolbarMarkUnread_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedMessage is { } open) await _vm.MarkMessagesUnreadAsync([open]);
    }

    // The flat list acts on its whole (extended) selection; anything else uses the Ctrl+Q targets.
    private IReadOnlyList<MailMessageSummary>? ContextMenuTargets()
    {
        if (_vm.IsMessagesView && MessageList.SelectedItems.Count > 0)
            return MessageList.SelectedItems.OfType<MailMessageSummary>().ToList();
        return ReadStateTargets();
    }

    internal static bool ShouldCloseOnReclick(MessageOpenMode mode, bool isOpen, bool isLoaded, bool isSameMessage)
        => mode == MessageOpenMode.ReadingPane && isOpen && isLoaded && isSameMessage;

    // ── Search ──────────────────────────────────────────────────────────────

    /// <summary>The search box is always on screen; focusing it starts a search, as Ctrl+Shift+S does.</summary>
    private void SearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!_vm.IsCalendarView && !_vm.IsSearchActive)
            _vm.IsSearchActive = true;
    }
}
