using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;
using IOPath = System.IO.Path;

namespace MailBoXX.ArtGen;

/// <summary>
/// Draws MailBox MatriX's placeholder artwork from code (no third-party images), deterministically.
/// Usage: MailBoXX.ArtGen &lt;outputAssetsDir&gt;
/// </summary>
public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var outDir = args.Length > 0 ? args[0] : IOPath.Combine("QuickMail", "Assets");
        // The app ships a photograph as its scenic background (see THIRD-PARTY-NOTICES.md); the
        // drawn savanna is only produced on request, as an offline fallback.
        if (args.Contains("--placeholder-scenic"))
        {
            Directory.CreateDirectory(IOPath.Combine(outDir, "Scenic"));
            var scenic = IOPath.Combine(outDir, "Scenic", "savanna-midday.jpg");
            Save(Scenic.Draw(2560, 1600), scenic, 90);
            Console.WriteLine($"wrote {scenic}");
        }

        var appDir = IOPath.Combine(outDir, "App");
        Directory.CreateDirectory(appDir);
        AppIcon.Write(IOPath.Combine(appDir, "mailboxx.ico"), IOPath.Combine(appDir, "mailboxx-256.png"));
        Console.WriteLine($"wrote {IOPath.Combine(appDir, "mailboxx.ico")}");

        // The row pictures are photographs too now (credited in THIRD-PARTY-NOTICES.md); the drawn
        // category art is only written on request, so a plain run never overwrites the photos.
        if (!args.Contains("--placeholder-context-art"))
            return 0;

        var contextDir = IOPath.Combine(outDir, "ContextImages");
        foreach (var (set, category, variant, bitmap) in ContextArt.DrawAll(960, 300))
        {
            Directory.CreateDirectory(IOPath.Combine(contextDir, set));
            var file = IOPath.Combine(contextDir, set, $"{category}-{variant}.jpg");
            Save(bitmap, file, 85);
            Console.WriteLine($"wrote {file}");
        }
        return 0;
    }

    internal static void Save(BitmapSource bitmap, string path, int quality)
    {
        var encoder = new JpegBitmapEncoder { QualityLevel = quality };
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    /// <summary>Renders a visual tree (effects included) at 96 dpi.</summary>
    internal static BitmapSource Render(FrameworkElement root, int width, int height)
    {
        root.Measure(new Size(width, height));
        root.Arrange(new Rect(0, 0, width, height));
        root.UpdateLayout();
        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(root);
        rtb.Freeze();
        return rtb;
    }

    internal static Color Hex(string hex) => (Color)ColorConverter.ConvertFromString(hex);
    internal static SolidColorBrush Brush(string hex) => new(Hex(hex));
}

/// <summary>A bright midday savanna: sky, sun glow, clouds, a rocky kopje, a lake and acacias.</summary>
internal static class Scenic
{
    public static BitmapSource Draw(int w, int h)
    {
        var rng = new Random(20261011);
        var canvas = new System.Windows.Controls.Canvas { Width = w, Height = h, ClipToBounds = true };
        double horizon = h * 0.56;

        // Sky.
        Add(canvas, new Rectangle
        {
            Width = w, Height = h,
            Fill = new LinearGradientBrush(new GradientStopCollection
            {
                new(Program.Hex("#2F6FC4"), 0.0),
                new(Program.Hex("#5C97DA"), 0.35),
                new(Program.Hex("#A9CFEF"), 0.62),
                new(Program.Hex("#DCEBF5"), 0.70),
            }, 90),
        }, 0, 0);

        // Sun glow, upper right.
        Add(canvas, new Ellipse
        {
            Width = w * 0.7, Height = w * 0.7,
            Fill = new RadialGradientBrush(Program.Hex("#80FFF6D8"), Program.Hex("#00FFF6D8")),
        }, w * 0.48, -w * 0.32);

        // Clouds: clusters of soft white puffs.
        var clouds = new System.Windows.Controls.Canvas { Width = w, Height = h, Effect = new BlurEffect { Radius = 18 } };
        for (int c = 0; c < 14; c++)
        {
            double cx = rng.NextDouble() * w, cy = h * (0.05 + rng.NextDouble() * 0.33);
            double scale = 0.6 + rng.NextDouble() * 1.2;
            for (int p = 0; p < 9; p++)
            {
                double pw = (80 + rng.NextDouble() * 140) * scale;
                Add(clouds, new Ellipse
                {
                    Width = pw, Height = pw * (0.55 + rng.NextDouble() * 0.3),
                    Fill = Program.Brush(p % 3 == 0 ? "#EEF4FA" : "#FFFFFF"),
                    Opacity = 0.75 + rng.NextDouble() * 0.25,
                }, cx + (rng.NextDouble() - 0.5) * 260 * scale, cy + (rng.NextDouble() - 0.5) * 50 * scale);
            }
        }
        canvas.Children.Add(clouds);

        // Distant blue ridge.
        Add(canvas, Ridge(rng, w, horizon - 40, 70, 18, "#8FA9C4", "#A9BED3"), 0, 0);

        // The kopje: a rocky flat-topped hill, centre-left.
        var kopje = new Path
        {
            Data = Geometry.Parse(
                $"M {w * 0.18},{horizon} C {w * 0.24},{horizon - 120} {w * 0.27},{horizon - 330} {w * 0.33},{horizon - 360} " +
                $"L {w * 0.42},{horizon - 380} C {w * 0.47},{horizon - 370} {w * 0.50},{horizon - 300} {w * 0.53},{horizon - 200} " +
                $"C {w * 0.56},{horizon - 120} {w * 0.60},{horizon - 60} {w * 0.66},{horizon} Z"),
            Fill = new LinearGradientBrush(Program.Hex("#B48A66"), Program.Hex("#7A5A44"), new Point(0, 0), new Point(1, 1)),
        };
        canvas.Children.Add(kopje);
        // Shadowed rock faces.
        for (int i = 0; i < 9; i++)
        {
            double x = w * (0.30 + rng.NextDouble() * 0.2), y = horizon - 340 + rng.NextDouble() * 260;
            Add(canvas, new Rectangle
            {
                Width = 8 + rng.NextDouble() * 16, Height = 60 + rng.NextDouble() * 140,
                Fill = Program.Brush("#5E4535"), Opacity = 0.35,
                RenderTransform = new SkewTransform(-8 + rng.NextDouble() * 16, 0),
            }, x, y);
        }

        // Lake.
        Add(canvas, new Rectangle
        {
            Width = w, Height = h * 0.09,
            Fill = new LinearGradientBrush(Program.Hex("#5E9AD0"), Program.Hex("#3E78B0"), 90),
        }, 0, horizon);
        for (int i = 0; i < 40; i++)
            Add(canvas, new Rectangle { Width = 40 + rng.NextDouble() * 160, Height = 3, Fill = Program.Brush("#A8D0F0"), Opacity = 0.5 },
                rng.NextDouble() * w, horizon + 6 + rng.NextDouble() * h * 0.08);

        // Savanna hills, back to front.
        Add(canvas, Ridge(rng, w, horizon + h * 0.10, 40, 14, "#9AA64A", "#8E9A3E", h), 0, 0);
        Add(canvas, Ridge(rng, w, horizon + h * 0.17, 55, 10, "#B79A48", "#9C7F36", h), 0, 0);

        // Acacias on the middle distance and one large tree at the right.
        for (int i = 0; i < 26; i++)
        {
            double s = 0.35 + rng.NextDouble() * 0.5;
            Acacia(canvas, rng.NextDouble() * w, horizon + h * (0.10 + rng.NextDouble() * 0.06), s);
        }
        Acacia(canvas, w * 0.86, h * 0.86, 3.2);

        // Foreground rocks and grass.
        for (int i = 0; i < 12; i++)
        {
            double rw = 120 + rng.NextDouble() * 260;
            Add(canvas, new Ellipse
            {
                Width = rw, Height = rw * 0.55,
                Fill = new LinearGradientBrush(Program.Hex("#9C948A"), Program.Hex("#5F5850"), 90),
            }, rng.NextDouble() * w, h * (0.84 + rng.NextDouble() * 0.12));
        }

        return Program.Render(canvas, w, h);
    }

    private static Path Ridge(Random rng, double w, double baseY, double amplitude, int steps, string top, string bottom, double? floor = null)
    {
        var fig = new PathFigure { StartPoint = new Point(0, floor ?? baseY + amplitude * 2), IsClosed = true };
        for (int i = 0; i <= steps; i++)
            fig.Segments.Add(new LineSegment(new Point(w * i / steps, baseY - rng.NextDouble() * amplitude), true));
        fig.Segments.Add(new LineSegment(new Point(w, floor ?? baseY + amplitude * 2), true));
        return new Path
        {
            Data = new PathGeometry(new[] { fig }),
            Fill = new LinearGradientBrush(Program.Hex(top), Program.Hex(bottom), 90),
        };
    }

    private static void Acacia(System.Windows.Controls.Canvas canvas, double x, double groundY, double s)
    {
        var trunk = new Path
        {
            Data = Geometry.Parse($"M {x},{groundY} L {x + 6 * s},{groundY - 70 * s} M {x + 6 * s},{groundY - 70 * s} L {x - 30 * s},{groundY - 95 * s} M {x + 6 * s},{groundY - 70 * s} L {x + 40 * s},{groundY - 98 * s}"),
            Stroke = Program.Brush("#3B2B1E"), StrokeThickness = Math.Max(2, 5 * s),
        };
        canvas.Children.Add(trunk);
        Add(canvas, new Ellipse { Width = 150 * s, Height = 38 * s, Fill = Program.Brush("#3F5A22") }, x - 70 * s, groundY - 120 * s);
        Add(canvas, new Ellipse { Width = 110 * s, Height = 26 * s, Fill = Program.Brush("#567A2E") }, x - 50 * s, groundY - 124 * s);
    }

    private static void Add(System.Windows.Controls.Canvas canvas, UIElement element, double left, double top)
    {
        System.Windows.Controls.Canvas.SetLeft(element, left);
        System.Windows.Controls.Canvas.SetTop(element, top);
        canvas.Children.Add(element);
    }
}
