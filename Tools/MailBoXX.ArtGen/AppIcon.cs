using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace MailBoXX.ArtGen;

/// <summary>
/// The MailBoXX application icon: a square (no rounded corners, per the design) in the sidebar
/// blue with a white envelope. Written as a multi-size .ico whose entries are PNG images.
/// </summary>
internal static class AppIcon
{
    private static readonly int[] Sizes = { 16, 24, 32, 48, 64, 128, 256 };

    public static void Write(string icoPath, string pngPath)
    {
        var pngs = Sizes.Select(s => (Size: s, Bytes: Png(Draw(s)))).ToList();
        File.WriteAllBytes(pngPath, pngs.Last().Bytes);

        using var ico = new BinaryWriter(File.Create(icoPath));
        ico.Write((ushort)0);            // reserved
        ico.Write((ushort)1);            // type: icon
        ico.Write((ushort)pngs.Count);
        var offset = 6 + 16 * pngs.Count;
        foreach (var (size, bytes) in pngs)
        {
            ico.Write((byte)(size >= 256 ? 0 : size));   // 0 means 256
            ico.Write((byte)(size >= 256 ? 0 : size));
            ico.Write((byte)0);          // palette
            ico.Write((byte)0);          // reserved
            ico.Write((ushort)1);        // colour planes
            ico.Write((ushort)32);       // bits per pixel
            ico.Write(bytes.Length);
            ico.Write(offset);
            offset += bytes.Length;
        }
        foreach (var (_, bytes) in pngs)
            ico.Write(bytes);
    }

    private static BitmapSource Draw(int s)
    {
        var canvas = new System.Windows.Controls.Canvas { Width = s, Height = s };
        var back = new Rectangle
        {
            Width = s, Height = s,
            Fill = new LinearGradientBrush(Program.Hex("#2B6CC0"), Program.Hex("#174C93"), new Point(0, 0), new Point(1, 1)),
        };
        canvas.Children.Add(back);
        // Envelope: body and flap, white, stroke scaled to the icon size.
        double m = s * 0.18, top = s * 0.28, w = s - 2 * m, h = s * 0.44;
        var stroke = Math.Max(1.2, s / 16.0);
        var envelope = new Path
        {
            Data = Geometry.Parse(
                $"M {m},{top} L {m + w},{top} L {m + w},{top + h} L {m},{top + h} Z " +
                $"M {m},{top} L {m + w / 2},{top + h * 0.58} L {m + w},{top}"),
            Stroke = Brushes.White,
            StrokeThickness = stroke,
            StrokeLineJoin = PenLineJoin.Miter,
        };
        canvas.Children.Add(envelope);
        return Program.Render(canvas, s, s);
    }

    private static byte[] Png(BitmapSource bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }
}
