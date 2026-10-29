namespace HizliErisim;

public static class Helpers
{
    public static string NormalizeUrl(string s)
    {
        s = s.Trim();
        return s.Contains("://") ? s : "https://" + s;
    }

    public static string HostOf(string url)
    {
        try
        {
            var h = new Uri(NormalizeUrl(url)).Host;
            return h.StartsWith("www.") ? h[4..] : h;
        }
        catch { return url; }
    }

    public static string NameFromPath(string path)
    {
        var trimmed = path.TrimEnd('\\', '/');
        if (Directory.Exists(path))
        {
            var n = Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(n) ? path : n;
        }
        return Path.GetFileNameWithoutExtension(trimmed);
    }
}
