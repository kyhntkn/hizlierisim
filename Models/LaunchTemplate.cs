using System.Text.Json.Serialization;

namespace HizliErisim;

public class LaunchTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public List<LaunchItem> Items { get; set; } = new();

    [JsonIgnore]
    public string Display => $"{Name}   ·   {Items.Count} öğe";
}
