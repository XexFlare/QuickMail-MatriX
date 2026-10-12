using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuickMail.Models;
using QuickMail.Services;
using QuickMail.Services.ContextImages;

namespace QuickMail.Views;

/// <summary>
/// The first letter or digit of a display name, for the round sender and profile pictures.
/// "?" when the name has none.
/// </summary>
public sealed class InitialConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? string.Empty;
        foreach (var c in text)
        {
            if (char.IsLetterOrDigit(c))
                return char.ToUpper(c, culture).ToString();
        }
        return "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// A message row → its contextual picture as a right-anchored, fill-the-row brush; null (the
/// neutral row) when there is none or it cannot be decoded. Bound to the row's message itself, so
/// the picture is resolved lazily — only for rows actually on screen (the list virtualizes). An
/// explicit <see cref="MailMessageSummary.ContextImagePath"/> wins; otherwise
/// <see cref="ContextImageService"/> picks one from the sender and subject. Images are decoded once at
/// row size, frozen, and kept in a small bounded cache so a recycled row never re-decodes and a
/// large mailbox never holds more than <see cref="Capacity"/> pictures in memory.
/// </summary>
public sealed class ContextImageBrushConverter : IValueConverter
{
    internal const int Capacity = 128;
    private const int DecodeWidth = 640;   // ~row width at 125% scaling; rows are drawn UniformToFill

    private static readonly Dictionary<string, LinkedListNode<(string Path, ImageBrush Brush)>> Index =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly LinkedList<(string Path, ImageBrush Brush)> Recent = new();

    /// <summary>Set once at startup (MainWindow); null leaves every row neutral.</summary>
    public static ContextImageService? Service { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = value switch
        {
            MailMessageSummary m when !string.IsNullOrEmpty(m.ContextImagePath) => m.ContextImagePath,
            MailMessageSummary m => Service?.ResolveImageUri(m,
                System.Windows.Application.Current?.TryFindResource(Theming.ThemeKeys.ImagerySet) as string ?? "light"),
            string s => s,
            _ => null,
        };
        if (string.IsNullOrEmpty(path))
            return null;

        if (Index.TryGetValue(path, out var node))
        {
            Recent.Remove(node);
            Recent.AddFirst(node);
            return node.Value.Brush;
        }

        var brush = Load(path);
        if (brush is null)
            return null;

        Index[path] = Recent.AddFirst((path, brush));
        while (Recent.Count > Capacity)
        {
            var last = Recent.Last!;
            Recent.RemoveLast();
            Index.Remove(last.Value.Path);
        }
        return brush;
    }

    private static ImageBrush? Load(string path)
    {
        try
        {
            var uri = path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                ? new Uri(path, UriKind.Absolute)
                : File.Exists(path) ? new Uri(path, UriKind.Absolute) : null;
            if (uri is null)
                return null;
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;          // read now, release the file
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.DecodePixelWidth = DecodeWidth;
            image.UriSource = uri;
            image.EndInit();
            image.Freeze();
            var brush = new ImageBrush(image)
            {
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Right,
                AlignmentY = AlignmentY.Center,
            };
            brush.Freeze();
            return brush;
        }
        catch (Exception ex)
        {
            // A broken or vanished picture only costs this row its image.
            LogService.Debug($"Context image could not be loaded: {ex.GetType().Name}");
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Which mail provider an account belongs to, for its sidebar badge: "outlook", "gmail",
/// "hotmail" or "other". Read from the provider id, the address and the server names only.
/// The badges are MailBoXX's own coloured letters, not the providers' logos.
/// </summary>
public sealed class AccountProviderConverter : IValueConverter
{
    public static string Classify(AccountModel? account)
    {
        if (account is null) return "other";
        var hay = string.Join(" ", account.ProviderId, account.Username, account.ImapHost, account.SmtpHost)
                        .ToLowerInvariant();
        if (hay.Contains("hotmail", StringComparison.Ordinal) || hay.Contains("live.", StringComparison.Ordinal)
            || hay.Contains("msn.", StringComparison.Ordinal)) return "hotmail";
        if (hay.Contains("gmail", StringComparison.Ordinal) || hay.Contains("google", StringComparison.Ordinal)
            || hay.Contains("googlemail", StringComparison.Ordinal)) return "gmail";
        if (hay.Contains("outlook", StringComparison.Ordinal) || hay.Contains("office365", StringComparison.Ordinal)
            || hay.Contains("microsoft", StringComparison.Ordinal)) return "outlook";
        return "other";
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Classify(value as AccountModel);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>A folder node → its Segoe MDL2 Assets glyph in the sidebar.</summary>
public sealed class FolderGlyphConverter : IValueConverter
{
    public static string GlyphFor(FolderTreeNode? node)
    {
        if (node is null) return "";
        if (node.IsCalendarNode) return "";
        return node.Folder?.Kind switch
        {
            SpecialFolderKind.Inbox   => "",   // Mail
            SpecialFolderKind.Drafts  => "",   // Document
            SpecialFolderKind.Sent    => "",   // Send
            SpecialFolderKind.Archive => "",   // Package (archive box)
            SpecialFolderKind.Trash   => "",   // Delete
            SpecialFolderKind.Junk    => "",   // Shield
            SpecialFolderKind.Outbox  => "",   // Outbox-like tray
            _                         => "",   // Folder
        };
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => GlyphFor(value as FolderTreeNode);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// A main-folder node → its Windows Mail name ("Sent Items", "Deleted Items", "Junk Email"). As a
/// multi-converter over (node, ItemStatusLabel) it gives the accessible name with the unread count
/// ("Inbox, 5 unread"), re-evaluated whenever the count changes. Other folders keep their own name.
/// </summary>
public sealed class MainFolderLabelConverter : IValueConverter, IMultiValueConverter
{
    public static string LabelFor(FolderTreeNode? node) => node?.Folder?.Kind switch
    {
        SpecialFolderKind.Inbox  => "Inbox",
        SpecialFolderKind.Drafts => "Drafts",
        SpecialFolderKind.Sent   => "Sent Items",
        SpecialFolderKind.Archive => "Archive",
        SpecialFolderKind.Trash  => "Deleted Items",
        SpecialFolderKind.Junk   => "Junk Email",
        _                        => node?.Label ?? string.Empty,
    };

    public static string AccessibleNameFor(FolderTreeNode? node)
    {
        var label = LabelFor(node);
        var status = node?.ItemStatusLabel;
        return string.IsNullOrEmpty(status) ? label : $"{label}, {status}";
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => LabelFor(value as FolderTreeNode);

    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        => AccessibleNameFor(values.Length > 0 ? values[0] as FolderTreeNode : null);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>true → Collapsed, false → Visible (the inverse of BooleanToVisibilityConverter).</summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// A sender → which of the six circle colours it gets (0 = Theme.AvatarBackground, 1–5 = AvatarAlt1–5).
/// Stable across runs and machines (FNV-1a over the lower-cased name; string.GetHashCode is randomised).
/// </summary>
public sealed class AvatarColorIndexConverter : IValueConverter
{
    public static int IndexFor(string? sender)
    {
        if (string.IsNullOrWhiteSpace(sender)) return 0;
        uint hash = 2166136261;
        foreach (var c in sender.Trim().ToLowerInvariant())
        {
            hash ^= c;
            hash *= 16777619;
        }
        return (int)(hash % 6);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => IndexFor(value as string).ToString(CultureInfo.InvariantCulture);   // matched by DataTrigger Value="1"…

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
