using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace MailBoXX.ArtGen;

/// <summary>
/// Row-shaped (wide, short) illustrations for the contextual-image categories. Each is a soft
/// gradient backdrop with a simple motif drawn on the right two-thirds — the row fades the left
/// side into its text, so nothing important lives there. Two colour variants per category.
/// The category names must match MailBoXX's ContextImageCategories.
/// </summary>
internal static class ContextArt
{
    private sealed record Palette(string Sky, string Ground, string Main, string Accent, string Soft);

    private static readonly (string Category, Palette A, Palette B, Action<Canvas, double, double, Palette> Motif)[] Catalog =
    {
        ("travel",        new("#8EC5F2", "#E8F3FC", "#2F6FC4", "#F2B33D", "#FFFFFF"), new("#F6C98A", "#FFF1DE", "#C2572B", "#2F6FC4", "#FFFFFF"), Travel),
        ("shopping",      new("#F7C6D6", "#FFF0F5", "#C23B6E", "#F2B33D", "#FFFFFF"), new("#C9E7D9", "#F0FAF5", "#2E8B67", "#E86F4E", "#FFFFFF"), Shopping),
        ("finance",       new("#BFE3D3", "#EFF8F3", "#2E7D5B", "#E0B13A", "#FFFFFF"), new("#C8D6EF", "#EEF3FB", "#2F4F8F", "#3FA37C", "#FFFFFF"), Finance),
        ("work",          new("#C9D7E8", "#F1F5FA", "#3C5A80", "#6FA8DC", "#FFFFFF"), new("#E6DCCB", "#FAF6EF", "#6B5440", "#4E8FC7", "#FFFFFF"), Work),
        ("events",        new("#D8CCF2", "#F5F1FC", "#5B3FA6", "#F2B33D", "#FFFFFF"), new("#F8D3B0", "#FFF4EA", "#C0602A", "#5B3FA6", "#FFFFFF"), Events),
        ("tech",          new("#CFE3F7", "#EEF5FC", "#2F6FC4", "#3FA3C4", "#FFFFFF"), new("#E2DAF7", "#F5F2FC", "#5B3FA6", "#3FA37C", "#FFFFFF"), Tech),
        ("food",          new("#FAD9B5", "#FFF5EA", "#D9622B", "#5FA84A", "#FFFFFF"), new("#F4E3B1", "#FFFAEB", "#B8451E", "#E2A72E", "#FFFFFF"), Food),
        ("fitness",       new("#BDE7C7", "#EEF9F1", "#2D8A4B", "#E2553F", "#FFFFFF"), new("#BFDDF5", "#EEF6FD", "#2C6CB0", "#F08A24", "#FFFFFF"), Fitness),
        ("photography",   new("#A9C9E8", "#E9F2FB", "#2B2F36", "#6D7784", "#FFFFFF"), new("#E8C9A9", "#FBF2E9", "#2B2F36", "#B5784A", "#FFFFFF"), Photography),
        ("news",          new("#DCDCDC", "#F7F7F7", "#3A3F47", "#A8B0BA", "#FFFFFF"), new("#D7E3F0", "#F3F7FB", "#2E4057", "#9BB0C8", "#FFFFFF"), News),
        ("security",      new("#C3D8F0", "#EEF4FB", "#1F4E8C", "#3FA37C", "#FFFFFF"), new("#CFE8DC", "#F0F9F4", "#22704F", "#2F6FC4", "#FFFFFF"), Security),
        ("delivery",      new("#F1D9A8", "#FFF7E6", "#A86A22", "#2F6FC4", "#FFFFFF"), new("#C9DDE8", "#EFF6FA", "#3C6E8C", "#E09A2E", "#FFFFFF"), Delivery),
        ("social",        new("#BFE0F5", "#EEF7FD", "#2C82C9", "#F06292", "#FFFFFF"), new("#F5D0C0", "#FFF2EC", "#D65A3A", "#3FA37C", "#FFFFFF"), Social),
        ("entertainment", new("#F3D9E6", "#FCF1F6", "#8E3A6B", "#F2B33D", "#FFFFFF"), new("#F7E0C8", "#FFF5EA", "#B5542C", "#5B3FA6", "#FFFFFF"), Entertainment),
    };

    /// <summary>Every picture in both sets: "light" for light themes, "dark" for dark themes.</summary>
    public static IEnumerable<(string Set, string Category, int Variant, BitmapSource Bitmap)> DrawAll(int w, int h)
    {
        foreach (var dark in new[] { false, true })
            foreach (var (category, a, b, motif) in Catalog)
            {
                var set = dark ? "dark" : "light";
                yield return (set, category, 1, Draw(w, h, dark ? Darken(a) : a, motif, dark));
                yield return (set, category, 2, Draw(w, h, dark ? Darken(b) : b, motif, dark));
            }
    }

    /// <summary>Dark-theme twin: deep backdrop, muted highlights, same motif colours.</summary>
    private static Palette Darken(Palette p) => p with
    {
        Sky = Blend(p.Sky, "#0E1726", 0.84),
        Ground = Blend(p.Ground, "#151C27", 0.88),
        Soft = "#C9D3DF",
    };

    private static string Blend(string a, string b, double t)
    {
        var x = Hex(a); var y = Hex(b);
        byte Mix(byte p, byte q) => (byte)Math.Round(p + (q - p) * t);
        return $"#{Mix(x.R, y.R):X2}{Mix(x.G, y.G):X2}{Mix(x.B, y.B):X2}";
    }

    private static BitmapSource Draw(int w, int h, Palette p, Action<Canvas, double, double, Palette> motif, bool dark)
    {
        var canvas = new Canvas { Width = w, Height = h, ClipToBounds = true };
        Add(canvas, new Rectangle
        {
            Width = w, Height = h,
            Fill = new LinearGradientBrush(Hex(p.Sky), Hex(p.Ground), new Point(0, 0), new Point(0.3, 1)),
        }, 0, 0);
        // Two soft light blooms for depth.
        var blooms = new Canvas { Width = w, Height = h, Effect = new BlurEffect { Radius = 60 } };
        Add(blooms, new Ellipse { Width = w * 0.45, Height = h * 1.2, Fill = Brush(p.Soft), Opacity = dark ? 0.12 : 0.45 }, w * 0.42, -h * 0.4);
        Add(blooms, new Ellipse { Width = w * 0.3, Height = h * 0.9, Fill = Brush(p.Accent), Opacity = 0.18 }, w * 0.78, h * 0.35);
        canvas.Children.Add(blooms);

        // The motif is drawn on its own layer, pulled left and slightly softened: the row puts its
        // time and envelope over the right edge, so that corner is kept calm.
        var layer = new Canvas
        {
            Width = w, Height = h,
            Opacity = dark ? 0.72 : 0.88,
            Effect = new BlurEffect { Radius = 1.2 },
            RenderTransform = new TransformGroup
            {
                Children = { new ScaleTransform(0.82, 0.82, w, h * 0.5), new TranslateTransform(-w * 0.16, 0) },
            },
        };
        motif(layer, w, h, p);
        canvas.Children.Add(layer);
        return Program.Render(canvas, w, h);
    }

    // ── motifs (drawn in the right two-thirds) ──────────────────────────────

    private static void Travel(Canvas c, double w, double h, Palette p)
    {
        Add(c, new Ellipse { Width = 90, Height = 90, Fill = Brush(p.Accent), Opacity = 0.9 }, w * 0.82, h * 0.12);
        Add(c, Poly(p.Main, 0.35, (w * 0.40, h), (w * 0.58, h * 0.45), (w * 0.72, h * 0.75), (w * 0.86, h * 0.38), (w * 1.02, h)), 0, 0);
        Add(c, Poly(p.Main, 0.6, (w * 0.50, h), (w * 0.66, h * 0.62), (w * 0.80, h * 0.82), (w * 0.94, h * 0.58), (w * 1.05, h)), 0, 0);
        // Airplane: fuselage, wings, tail.
        var x = w * 0.60; var y = h * 0.26;
        Add(c, Poly(p.Main, 1, (x, y + 12), (x + 150, y), (x + 162, y + 6), (x + 150, y + 12), (x, y + 18)), 0, 0);
        Add(c, Poly(p.Main, 1, (x + 60, y + 9), (x + 100, y - 30), (x + 112, y - 30), (x + 92, y + 9)), 0, 0);
        Add(c, Poly(p.Main, 1, (x + 6, y + 12), (x - 6, y - 12), (x + 4, y - 12), (x + 24, y + 12)), 0, 0);
    }

    private static void Shopping(Canvas c, double w, double h, Palette p)
    {
        Bag(c, w * 0.55, h * 0.38, 120, 150, p.Main);
        Bag(c, w * 0.70, h * 0.30, 140, 175, p.Accent);
        Bag(c, w * 0.85, h * 0.45, 100, 125, p.Main);
        static void Bag(Canvas c, double x, double y, double bw, double bh, string color)
        {
            Add(c, new Rectangle { Width = bw, Height = bh, Fill = Brush(color), RadiusX = 6, RadiusY = 6 }, x, y);
            Add(c, new Path { Data = Geometry.Parse($"M {x + bw * 0.28},{y} C {x + bw * 0.28},{y - bh * 0.32} {x + bw * 0.72},{y - bh * 0.32} {x + bw * 0.72},{y}"),
                              Stroke = Brush(color), StrokeThickness = 7 }, 0, 0);
        }
    }

    private static void Finance(Canvas c, double w, double h, Palette p)
    {
        double[] bars = { 0.35, 0.55, 0.45, 0.72, 0.88 };
        for (int i = 0; i < bars.Length; i++)
            Add(c, new Rectangle { Width = 44, Height = h * bars[i], Fill = Brush(p.Main), Opacity = 0.8 }, w * 0.52 + i * 62, h - h * bars[i]);
        for (int i = 0; i < 5; i++)
            Add(c, new Ellipse { Width = 110, Height = 34, Fill = Brush(p.Accent), Stroke = Brush(p.Main), StrokeThickness = 2 }, w * 0.84, h * 0.78 - i * 22);
    }

    private static void Work(Canvas c, double w, double h, Palette p)
    {
        Add(c, new Rectangle { Width = 260, Height = 160, Fill = Brush(p.Main), RadiusX = 10, RadiusY = 10 }, w * 0.56, h * 0.18);
        Add(c, new Rectangle { Width = 236, Height = 136, Fill = Brush(p.Accent), Opacity = 0.9 }, w * 0.56 + 12, h * 0.18 + 12);
        Add(c, Poly(p.Main, 1, (w * 0.53, h * 0.18 + 172), (w * 0.56 + 290, h * 0.18 + 172), (w * 0.56 + 270, h * 0.18 + 160), (w * 0.56 - 10, h * 0.18 + 160)), 0, 0);
        Add(c, new Rectangle { Width = 60, Height = 74, Fill = Brush(p.Soft), Stroke = Brush(p.Main), StrokeThickness = 4, RadiusX = 8, RadiusY = 8 }, w * 0.88, h * 0.5);
    }

    private static void Events(Canvas c, double w, double h, Palette p)
    {
        var x = w * 0.6; var y = h * 0.12;
        Add(c, new Rectangle { Width = 230, Height = 220, Fill = Brush(p.Soft), RadiusX = 12, RadiusY = 12 }, x, y);
        Add(c, new Rectangle { Width = 230, Height = 52, Fill = Brush(p.Main), RadiusX = 12, RadiusY = 12 }, x, y);
        for (int r = 0; r < 3; r++) for (int k = 0; k < 4; k++)
            Add(c, new Rectangle { Width = 36, Height = 30, Fill = Brush(r == 1 && k == 2 ? p.Accent : p.Main), Opacity = r == 1 && k == 2 ? 1 : 0.25 }, x + 18 + k * 52, y + 70 + r * 46);
        var rng = new Random(7);
        for (int i = 0; i < 26; i++)
            Add(c, new Ellipse { Width = 10, Height = 10, Fill = Brush(i % 2 == 0 ? p.Accent : p.Main) }, w * (0.48 + rng.NextDouble() * 0.5), h * rng.NextDouble());
    }

    private static void Tech(Canvas c, double w, double h, Palette p)
    {
        var rng = new Random(11);
        for (int i = 0; i < 16; i++)
        {
            double x = w * (0.45 + rng.NextDouble() * 0.5), y = h * (0.1 + rng.NextDouble() * 0.8);
            double len = 60 + rng.NextDouble() * 140;
            Add(c, new Path { Data = Geometry.Parse($"M {x},{y} L {x + len},{y} L {x + len + 30},{y + (rng.Next(2) == 0 ? 30 : -30)}"),
                              Stroke = Brush(p.Main), StrokeThickness = 3, Opacity = 0.8 }, 0, 0);
            Add(c, new Ellipse { Width = 14, Height = 14, Fill = Brush(p.Accent) }, x - 7, y - 7);
        }
        Add(c, new Rectangle { Width = 110, Height = 110, Fill = Brush(p.Main), Opacity = 0.25, RadiusX = 10, RadiusY = 10 }, w * 0.74, h * 0.3);
    }

    private static void Food(Canvas c, double w, double h, Palette p)
    {
        Add(c, new Ellipse { Width = 230, Height = 230, Fill = Brush(p.Soft) }, w * 0.6, h * 0.12);
        Add(c, new Ellipse { Width = 170, Height = 170, Fill = Brush(p.Ground), Stroke = Brush(p.Accent), StrokeThickness = 3 }, w * 0.6 + 30, h * 0.12 + 30);
        Add(c, new Ellipse { Width = 64, Height = 64, Fill = Brush(p.Main) }, w * 0.6 + 70, h * 0.12 + 70);
        Add(c, new Ellipse { Width = 50, Height = 50, Fill = Brush(p.Accent) }, w * 0.6 + 120, h * 0.12 + 96);
        Add(c, new Ellipse { Width = 80, Height = 30, Fill = Brush(p.Accent), RenderTransform = new RotateTransform(-30) }, w * 0.86, h * 0.2);
    }

    private static void Fitness(Canvas c, double w, double h, Palette p)
    {
        for (int i = 0; i < 4; i++)
            Add(c, new Path { Data = Geometry.Parse($"M {w * 0.4},{h + 20} C {w * 0.6},{h * (0.4 + i * 0.12)} {w * 0.85},{h * (0.4 + i * 0.12)} {w * 1.05},{h * (0.1 + i * 0.12)}"),
                              Stroke = Brush(p.Main), StrokeThickness = 6, Opacity = 0.35 + i * 0.15 }, 0, 0);
        Add(c, new Path { Data = Geometry.Parse($"M {w * 0.55},{h * 0.3} L {w * 0.65},{h * 0.3} L {w * 0.69},{h * 0.12} L {w * 0.74},{h * 0.48} L {w * 0.78},{h * 0.3} L {w * 0.92},{h * 0.3}"),
                          Stroke = Brush(p.Accent), StrokeThickness = 7, StrokeLineJoin = PenLineJoin.Round }, 0, 0);
    }

    private static void Photography(Canvas c, double w, double h, Palette p)
    {
        var x = w * 0.6; var y = h * 0.2;
        Add(c, new Rectangle { Width = 260, Height = 170, Fill = Brush(p.Main), RadiusX = 18, RadiusY = 18 }, x, y);
        Add(c, new Rectangle { Width = 80, Height = 34, Fill = Brush(p.Main), RadiusX = 8, RadiusY = 8 }, x + 30, y - 22);
        Add(c, new Ellipse { Width = 130, Height = 130, Fill = Brush(p.Accent) }, x + 65, y + 20);
        Add(c, new Ellipse { Width = 84, Height = 84, Fill = Brush(p.Sky), Stroke = Brush(p.Soft), StrokeThickness = 4 }, x + 88, y + 43);
        Add(c, new Ellipse { Width = 22, Height = 22, Fill = Brush(p.Soft), Opacity = 0.8 }, x + 108, y + 58);
    }

    private static void News(Canvas c, double w, double h, Palette p)
    {
        var x = w * 0.58; var y = h * 0.1;
        Add(c, new Rectangle { Width = 300, Height = 250, Fill = Brush(p.Soft), RadiusX = 6, RadiusY = 6 }, x, y);
        Add(c, new Rectangle { Width = 260, Height = 30, Fill = Brush(p.Main) }, x + 20, y + 18);
        for (int col = 0; col < 2; col++) for (int r = 0; r < 7; r++)
            Add(c, new Rectangle { Width = 118, Height = 8, Fill = Brush(p.Accent) }, x + 20 + col * 142, y + 66 + r * 22);
    }

    private static void Security(Canvas c, double w, double h, Palette p)
    {
        var x = w * 0.66; var y = h * 0.08;
        Add(c, new Path { Data = Geometry.Parse($"M {x},{y + 30} L {x + 110},{y} L {x + 220},{y + 30} C {x + 220},{y + 170} {x + 140},{y + 240} {x + 110},{y + 260} C {x + 80},{y + 240} {x},{y + 170} {x},{y + 30} Z"),
                          Fill = Brush(p.Main) }, 0, 0);
        Add(c, new Rectangle { Width = 90, Height = 70, Fill = Brush(p.Soft), RadiusX = 8, RadiusY = 8 }, x + 65, y + 115);
        Add(c, new Path { Data = Geometry.Parse($"M {x + 80},{y + 115} L {x + 80},{y + 92} C {x + 80},{y + 58} {x + 140},{y + 58} {x + 140},{y + 92} L {x + 140},{y + 115}"),
                          Stroke = Brush(p.Soft), StrokeThickness = 10 }, 0, 0);
        Add(c, new Ellipse { Width = 16, Height = 16, Fill = Brush(p.Accent) }, x + 102, y + 140);
    }

    private static void Delivery(Canvas c, double w, double h, Palette p)
    {
        var x = w * 0.56; var y = h * 0.3;
        Add(c, new Rectangle { Width = 200, Height = 120, Fill = Brush(p.Main), RadiusX = 6, RadiusY = 6 }, x, y);
        Add(c, Poly(p.Main, 0.85, (x + 205, y + 40), (x + 270, y + 40), (x + 300, y + 80), (x + 300, y + 120), (x + 205, y + 120)), 0, 0);
        Add(c, new Ellipse { Width = 50, Height = 50, Fill = Brush(p.Accent), Stroke = Brush(p.Soft), StrokeThickness = 6 }, x + 30, y + 100);
        Add(c, new Ellipse { Width = 50, Height = 50, Fill = Brush(p.Accent), Stroke = Brush(p.Soft), StrokeThickness = 6 }, x + 220, y + 100);
        Add(c, new Rectangle { Width = 80, Height = 70, Fill = Brush(p.Accent), Opacity = 0.9 }, w * 0.88, h * 0.5);
    }

    private static void Social(Canvas c, double w, double h, Palette p)
    {
        Bubble(c, w * 0.56, h * 0.14, 200, 110, p.Main);
        Bubble(c, w * 0.74, h * 0.42, 190, 100, p.Accent);
        static void Bubble(Canvas c, double x, double y, double bw, double bh, string color)
        {
            Add(c, new Rectangle { Width = bw, Height = bh, Fill = Brush(color), RadiusX = 26, RadiusY = 26 }, x, y);
            Add(c, Poly(color, 1, (x + 30, y + bh - 2), (x + 20, y + bh + 30), (x + 64, y + bh - 2)), 0, 0);
            for (int i = 0; i < 3; i++)
                Add(c, new Ellipse { Width = 16, Height = 16, Fill = Brushes.White }, x + bw / 2 - 40 + i * 32, y + bh / 2 - 8);
        }
    }

    private static void Entertainment(Canvas c, double w, double h, Palette p)
    {
        var x = w * 0.55; var y = h * 0.24;
        Add(c, new Rectangle { Width = 320, Height = 140, Fill = Brush(p.Main), Opacity = 0.9 }, x, y);
        for (int i = 0; i < 8; i++)
        {
            Add(c, new Rectangle { Width = 22, Height = 14, Fill = Brush(p.Sky) }, x + 14 + i * 38, y + 10);
            Add(c, new Rectangle { Width = 22, Height = 14, Fill = Brush(p.Sky) }, x + 14 + i * 38, y + 116);
        }
        Add(c, Poly(p.Soft, 1, (x + 135, y + 42), (x + 135, y + 98), (x + 185, y + 70)), 0, 0);
        Add(c, new Ellipse { Width = 34, Height = 26, Fill = Brush(p.Accent) }, w * 0.9, h * 0.62);
        Add(c, new Rectangle { Width = 5, Height = 80, Fill = Brush(p.Accent) }, w * 0.9 + 29, h * 0.62 - 68);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static Polygon Poly(string color, double opacity, params (double X, double Y)[] pts)
    {
        var poly = new Polygon { Fill = Brush(color), Opacity = opacity };
        foreach (var (x, y) in pts) poly.Points.Add(new Point(x, y));
        return poly;
    }

    private static void Add(Canvas canvas, UIElement element, double left, double top)
    {
        Canvas.SetLeft(element, left);
        Canvas.SetTop(element, top);
        canvas.Children.Add(element);
    }

    private static Color Hex(string hex) => Program.Hex(hex);
    private static SolidColorBrush Brush(string hex) => Program.Brush(hex);
}
