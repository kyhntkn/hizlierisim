using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HizliErisim;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<LaunchItem> _items = new();
    private ICollectionView? _view;
    private ItemType? _filterType;

    public MainWindow()
    {
        InitializeComponent();

        foreach (var it in Storage.Load())
        {
            if (it.Type != ItemType.Url) it.Icon = IconService.GetFileIcon(it.Target);
            _items.Add(it);
        }

        _view = CollectionViewSource.GetDefaultView(_items);
        _view.Filter = Filter;
        ItemsHost.ItemsSource = _view;
        UpdateEmpty();

        // Favicons load in the background (cached after first download)
        Loaded += async (_, _) =>
        {
            var urlItems = _items.Where(i => i.Type == ItemType.Url).ToList();
            await Task.WhenAll(urlItems.Select(IconService.LoadIconAsync));
        };
    }

    // ---------- Filtering ----------

    private bool Filter(object o)
    {
        if (o is not LaunchItem i) return false;
        if (_filterType.HasValue && i.Type != _filterType.Value) return false;

        var q = SearchBox.Text?.Trim();
        if (string.IsNullOrEmpty(q)) return true;
        return Has(i.Title, q) || Has(i.Description, q) || Has(i.Target, q);
    }

    private static bool Has(string s, string q) =>
        s.Contains(q, StringComparison.CurrentCultureIgnoreCase);

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchHint.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        _view?.Refresh();
        UpdateEmpty();
    }

    private void Chip_Checked(object sender, RoutedEventArgs e)
    {
        var tag = (sender as RadioButton)?.Tag as string;
        _filterType = tag switch
        {
            "File" => ItemType.File,
            "Url" => ItemType.Url,
            "App" => ItemType.App,
            _ => null
        };
        _view?.Refresh();
        UpdateEmpty();
    }

    private void UpdateEmpty()
    {
        if (_view == null) return;
        if (_items.Count == 0)
        {
            EmptyTitle.Text = "Henüz bir şey eklemediniz";
            EmptyText.Text = "Yukarıdaki düğmelerle ekleyin ya da\ndosya, klasör, uygulama veya link sürükleyip buraya bırakın.";
            EmptyState.Visibility = Visibility.Visible;
        }
        else if (_view.IsEmpty)
        {
            EmptyTitle.Text = "Eşleşen kayıt yok";
            EmptyText.Text = "Aramayı veya filtreyi değiştirin.";
            EmptyState.Visibility = Visibility.Visible;
        }
        else
        {
            EmptyState.Visibility = Visibility.Collapsed;
        }
    }

    // ---------- Templates ----------

    private void Templates_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new TemplatesDialog(_items.ToList()) { Owner = this };
        if (dlg.ShowDialog() != true || dlg.ItemsToApply is not { } list) return;

        if (dlg.Replace) _items.Clear();

        var added = new List<LaunchItem>();
        foreach (var it in list)
        {
            if (!dlg.Replace && _items.Any(x => x.Type == it.Type &&
                    string.Equals(x.Target, it.Target, StringComparison.OrdinalIgnoreCase)))
                continue;
            _items.Add(it);
            added.Add(it);
        }

        UpdateEmpty();
        Save();
        foreach (var it in added) _ = IconService.LoadIconAsync(it);
    }

    // ---------- Add / Edit / Delete ----------

    private void AddFile_Click(object sender, RoutedEventArgs e) => AddWithDialog(ItemType.File);
    private void AddUrl_Click(object sender, RoutedEventArgs e) => AddWithDialog(ItemType.Url);
    private void AddApp_Click(object sender, RoutedEventArgs e) => AddWithDialog(ItemType.App);

    private void AddWithDialog(ItemType type)
    {
        var dlg = new ItemDialog(new LaunchItem { Type = type }, true) { Owner = this };
        if (dlg.ShowDialog() == true) AddItem(dlg.Result);
    }

    private async void AddItem(LaunchItem item)
    {
        _items.Add(item);
        UpdateEmpty();
        Save();
        await IconService.LoadIconAsync(item);
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        var item = ItemFrom(sender);
        if (item == null) return;

        var dlg = new ItemDialog(item.Clone(), false) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        var r = dlg.Result;
        var targetChanged = item.Target != r.Target;
        item.Title = r.Title;
        item.Description = r.Description;
        item.Target = r.Target;
        Save();
        if (targetChanged) _ = IconService.LoadIconAsync(item);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        var item = ItemFrom(sender);
        if (item == null) return;

        var ok = MessageBox.Show(this, $"\"{item.Title}\" listeden silinsin mi?\n(Gerçek dosya silinmez.)",
            "Hızlı Erişim", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (ok != MessageBoxResult.Yes) return;

        _items.Remove(item);
        UpdateEmpty();
        Save();
    }

    private void Card_Click(object sender, RoutedEventArgs e)
    {
        if (ItemFrom(sender) is { } item) Launcher.Open(item);
    }

    private void Open_Click(object sender, RoutedEventArgs e) => Card_Click(sender, e);

    private static LaunchItem? ItemFrom(object sender)
    {
        if (sender is not FrameworkElement fe) return null;
        if (fe.DataContext is LaunchItem i) return i;
        if (fe is MenuItem mi && mi.Parent is ContextMenu cm
            && cm.PlacementTarget is FrameworkElement t && t.DataContext is LaunchItem j) return j;
        return null;
    }

    private void Save()
    {
        try { Storage.Save(_items); }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Kaydedilemedi: " + ex.Message, "Hızlı Erişim",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- Drag & drop ----------

    private static string? GetUrlText(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.UnicodeText)) return null;
        var text = (data.GetData(DataFormats.UnicodeText) as string)?
            .Split('\r', '\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        if (text != null && Uri.TryCreate(text, UriKind.Absolute, out var u)
            && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
            return text;
        return null;
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        var ok = e.Data.GetDataPresent(DataFormats.FileDrop) || GetUrlText(e.Data) != null;
        e.Effects = ok ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            foreach (var p in paths) AddFromPath(p);
        }
        else if (GetUrlText(e.Data) is { } url)
        {
            AddFromUrl(url);
        }
        e.Handled = true;
    }

    private void AddFromPath(string path)
    {
        if (_items.Any(i => i.Type != ItemType.Url &&
                            string.Equals(i.Target, path, StringComparison.OrdinalIgnoreCase))) return;

        var isDir = Directory.Exists(path);
        var ext = Path.GetExtension(path).ToLowerInvariant();
        var isApp = !isDir && (ext == ".exe" || ext == ".lnk" || ext == ".bat" || ext == ".cmd");
        var type = isApp ? ItemType.App : ItemType.File;

        var item = new LaunchItem { Type = type, Title = Helpers.NameFromPath(path), Target = path };
        item.Icon = IconService.GetFileIcon(path);
        _items.Add(item);
        UpdateEmpty();
        Save();
    }

    private void AddFromUrl(string url)
    {
        if (_items.Any(i => i.Type == ItemType.Url &&
                            string.Equals(i.Target, url, StringComparison.OrdinalIgnoreCase))) return;
        AddItem(new LaunchItem { Type = ItemType.Url, Title = Helpers.HostOf(url), Target = url });
    }
}
