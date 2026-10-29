using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HizliErisim;

public static class Storage
{
    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HizliErisim");

    private static string ItemsPath => Path.Combine(Dir, "items.json");
    private static string TemplatesPath => Path.Combine(Dir, "templates.json");
    public static string IconDir => Path.Combine(Dir, "icons");

    private static readonly JsonSerializerOptions Opt = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static List<T> LoadList<T>(string path)
    {
        try
        {
            if (!File.Exists(path)) return new();
            return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), Opt) ?? new();
        }
        catch
        {
            // Corrupt file: keep a backup and start fresh
            try { File.Copy(path, path + ".bak", true); } catch { }
            return new();
        }
    }

    private static void SaveList<T>(string path, IEnumerable<T> items)
    {
        Directory.CreateDirectory(Dir);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(items, Opt));
        File.Move(tmp, path, true);
    }

    public static List<LaunchItem> Load() => LoadList<LaunchItem>(ItemsPath);
    public static void Save(IEnumerable<LaunchItem> items) => SaveList(ItemsPath, items);

    public static List<LaunchTemplate> LoadTemplates() => LoadList<LaunchTemplate>(TemplatesPath);
    public static void SaveTemplates(IEnumerable<LaunchTemplate> templates) => SaveList(TemplatesPath, templates);
}
