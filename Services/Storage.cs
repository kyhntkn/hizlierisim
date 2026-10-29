using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HizliErisim;

public static class Storage
{
    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HizliErisim");

    private static string FilePath => Path.Combine(Dir, "items.json");
    public static string IconDir => Path.Combine(Dir, "icons");

    private static readonly JsonSerializerOptions Opt = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    public static List<LaunchItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new();
            return JsonSerializer.Deserialize<List<LaunchItem>>(File.ReadAllText(FilePath), Opt) ?? new();
        }
        catch
        {
            // Corrupt file: keep a backup and start fresh
            try { File.Copy(FilePath, FilePath + ".bak", true); } catch { }
            return new();
        }
    }

    public static void Save(IEnumerable<LaunchItem> items)
    {
        Directory.CreateDirectory(Dir);
        var tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(items, Opt));
        File.Move(tmp, FilePath, true);
    }
}
