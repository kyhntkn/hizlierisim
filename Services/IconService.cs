using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HizliErisim;

public static class IconService
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string path, uint attrs, ref SHFILEINFO info, uint cb, uint flags);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static async Task LoadIconAsync(LaunchItem item)
    {
        if (item.Type == ItemType.Url)
            item.Icon = await GetFaviconAsync(item.Target);
        else
            item.Icon = GetFileIcon(item.Target);
    }

    public static ImageSource? GetFileIcon(string path)
    {
        try
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return null;
            var info = new SHFILEINFO();
            SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI_ICON | SHGFI_LARGEICON);
            if (info.hIcon == IntPtr.Zero) return null;
            try
            {
                var src = Imaging.CreateBitmapSourceFromHIcon(
                    info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                src.Freeze();
                return src;
            }
            finally { DestroyIcon(info.hIcon); }
        }
        catch { return null; }
    }

    public static async Task<ImageSource?> GetFaviconAsync(string url)
    {
        try
        {
            var host = new Uri(Helpers.NormalizeUrl(url)).Host;
            Directory.CreateDirectory(Storage.IconDir);
            var cache = Path.Combine(Storage.IconDir, host + ".png");

            if (!File.Exists(cache))
            {
                var bytes = await Http.GetByteArrayAsync(
                    $"https://www.google.com/s2/favicons?domain={host}&sz=64");
                await File.WriteAllBytesAsync(cache, bytes);
            }

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(cache);
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
        catch { return null; }
    }
}
