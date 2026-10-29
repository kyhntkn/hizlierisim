using System.Windows;
using Microsoft.Win32;

namespace HizliErisim;

public partial class ItemDialog : Window
{
    private readonly LaunchItem _item;
    public LaunchItem Result { get; private set; }

    public ItemDialog(LaunchItem item, bool isNew)
    {
        InitializeComponent();
        _item = item;
        Result = item;

        var typeName = item.Type switch
        {
            ItemType.Url => "Link",
            ItemType.App => "Uygulama",
            _ => "Dosya / Klasör"
        };
        Title = isNew ? $"Yeni {typeName}" : $"{typeName} düzenle";
        Heading.Text = Title;

        switch (item.Type)
        {
            case ItemType.Url:
                TargetLabel.Text = "Web adresi";
                BrowsePanel.Visibility = Visibility.Collapsed;
                break;
            case ItemType.App:
                TargetLabel.Text = "Uygulama (.exe veya kısayol)";
                BrowseFileBtn.Content = "Gözat…";
                BrowseFolderBtn.Visibility = Visibility.Collapsed;
                break;
            default:
                TargetLabel.Text = "Dosya veya klasör yolu";
                break;
        }

        TargetBox.Text = item.Target;
        TitleBox.Text = item.Title;
        DescBox.Text = item.Description;

        Loaded += (_, _) => (item.Type == ItemType.Url || !isNew ? TargetBox : TitleBox).Focus();
        if (isNew && item.Type != ItemType.Url)
            Loaded += (_, _) => BrowseFile_Click(this, new RoutedEventArgs());
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { CheckFileExists = true };
        if (_item.Type == ItemType.App)
        {
            dlg.Title = "Uygulama seç";
            dlg.Filter = "Uygulamalar (*.exe;*.lnk)|*.exe;*.lnk|Tüm dosyalar (*.*)|*.*";
            dlg.DereferenceLinks = false;
            var start = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
            if (Directory.Exists(start)) dlg.InitialDirectory = start;
        }
        else
        {
            dlg.Title = "Dosya seç";
        }

        if (dlg.ShowDialog(this) == true) SetTarget(dlg.FileName);
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Klasör seç" };
        if (dlg.ShowDialog(this) == true) SetTarget(dlg.FolderName);
    }

    private void SetTarget(string path)
    {
        TargetBox.Text = path;
        TitleBox.Text = Helpers.NameFromPath(path);
    }

    private void TargetBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_item.Type == ItemType.Url && string.IsNullOrWhiteSpace(TitleBox.Text)
            && !string.IsNullOrWhiteSpace(TargetBox.Text))
            TitleBox.Text = Helpers.HostOf(TargetBox.Text);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var target = TargetBox.Text.Trim().Trim('"');
        var title = TitleBox.Text.Trim();

        if (target.Length == 0) { Warn("Lütfen bir yol veya adres girin."); return; }

        if (_item.Type == ItemType.Url)
        {
            target = Helpers.NormalizeUrl(target);
            if (!Uri.TryCreate(target, UriKind.Absolute, out var u) ||
                (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps))
            { Warn("Geçerli bir web adresi girin (örn. https://ornek.com)."); return; }
            if (title.Length == 0) title = Helpers.HostOf(target);
        }
        else
        {
            if (!File.Exists(target) && !Directory.Exists(target)) { Warn("Bu yol bulunamadı."); return; }
            if (title.Length == 0) title = Helpers.NameFromPath(target);
        }

        _item.Target = target;
        _item.Title = title;
        _item.Description = DescBox.Text.Trim();
        Result = _item;
        DialogResult = true;
    }

    private void Warn(string msg) =>
        MessageBox.Show(this, msg, "Hızlı Erişim", MessageBoxButton.OK, MessageBoxImage.Warning);
}
