using System;
using System.Drawing;
using System.IO;

namespace _5CRXmod;

internal static class PathHelper
{
    private const int MaxParentTries = 5;

    public static string StartupPath => AppDomain.CurrentDomain.BaseDirectory;

    public static string GetImgDir()
    {
        string dir = Path.Combine(StartupPath, "files", "img");
        if (Directory.Exists(dir)) return dir;

        string probe = StartupPath;
        for (int i = 0; i < MaxParentTries; i++)
        {
            dir = Path.Combine(probe, "files", "img");
            if (Directory.Exists(dir)) return dir;
            var parent = Directory.GetParent(probe);
            if (parent == null) break;
            probe = parent.FullName;
        }
        return Path.Combine(StartupPath, "files", "img");
    }

    public static string GetM3uDir()
    {
        string dir = Path.Combine(StartupPath, "files", "m3u");
        if (Directory.Exists(dir)) return dir;

        string probe = StartupPath;
        for (int i = 0; i < MaxParentTries; i++)
        {
            dir = Path.Combine(probe, "files", "m3u");
            if (Directory.Exists(dir)) return dir;
            var parent = Directory.GetParent(probe);
            if (parent == null) break;
            probe = parent.FullName;
        }
        return Path.Combine(StartupPath, "files", "m3u");
    }

    public static string GetFilesDir()
    {
        string dir = Path.Combine(StartupPath, "files");
        if (Directory.Exists(dir)) return dir;

        string probe = StartupPath;
        for (int i = 0; i < MaxParentTries; i++)
        {
            dir = Path.Combine(probe, "files");
            if (Directory.Exists(dir)) return dir;
            var parent = Directory.GetParent(probe);
            if (parent == null) break;
            probe = parent.FullName;
        }
        return Path.Combine(StartupPath, "files");
    }

    public static string ResolveImg(string fileName)
    {
        return Path.Combine(GetImgDir(), fileName);
    }

    /// <summary>Loads an image. For static images (PNG), copies to memory so the file isn't locked.
    /// For GIFs, loads directly to preserve animation (file may remain open).</summary>
    public static Image LoadImage(string path)
    {
        if (path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            return Image.FromFile(path);
        using var img = Image.FromFile(path);
        return new Bitmap(img);
    }
}
