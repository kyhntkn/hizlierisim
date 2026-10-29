using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace HizliErisim;

public enum ItemType { File, Url, App }

public class LaunchItem : INotifyPropertyChanged
{
    private string _title = "";
    private string _description = "";
    private string _target = "";
    private ImageSource? _icon;

    public Guid Id { get; set; } = Guid.NewGuid();
    public ItemType Type { get; set; }

    public string Title
    {
        get => _title;
        set { _title = value; OnChanged(nameof(Title)); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnChanged(nameof(Description)); }
    }

    /// <summary>File/folder path, app path, or URL.</summary>
    public string Target
    {
        get => _target;
        set { _target = value; OnChanged(nameof(Target)); }
    }

    [JsonIgnore]
    public ImageSource? Icon
    {
        get => _icon;
        set { _icon = value; OnChanged(nameof(Icon)); OnChanged(nameof(HasIcon)); }
    }

    [JsonIgnore] public bool HasIcon => _icon != null;

    [JsonIgnore]
    public string Glyph => Type switch
    {
        ItemType.Url => "\uE774",   // globe
        ItemType.App => "\uE71D",   // apps
        _ => "\uE8A5"               // document
    };

    [JsonIgnore]
    public string TypeLabel => Type switch
    {
        ItemType.Url => "LİNK",
        ItemType.App => "UYGULAMA",
        _ => "DOSYA"
    };

    public LaunchItem Clone() => new()
    {
        Id = Id, Type = Type, Title = Title, Description = Description, Target = Target
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
