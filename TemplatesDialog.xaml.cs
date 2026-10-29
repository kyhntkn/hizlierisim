using System.Windows;
using System.Windows.Controls;

namespace HizliErisim;

public partial class TemplatesDialog : Window
{
    private readonly List<LaunchItem> _current;
    private readonly List<LaunchTemplate> _templates;

    /// <summary>Items the main window should apply when the dialog returns true.</summary>
    public List<LaunchItem>? ItemsToApply { get; private set; }
    public bool Replace { get; private set; }

    public TemplatesDialog(List<LaunchItem> current)
    {
        InitializeComponent();
        _current = current;
        _templates = Storage.LoadTemplates();
        Refresh(null);
    }

    private LaunchTemplate? Selected => TemplateList.SelectedItem as LaunchTemplate;

    private void Refresh(Guid? select)
    {
        TemplateList.ItemsSource = null;
        TemplateList.ItemsSource = _templates;
        if (select != null)
            TemplateList.SelectedItem = _templates.FirstOrDefault(t => t.Id == select);
    }

    private void TemplateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Selected != null) NameBox.Text = Selected.Name;
    }

    private List<LaunchItem> Snapshot() => _current.Select(i => i.Clone()).ToList();

    private static LaunchItem Fresh(LaunchItem i)
    {
        var c = i.Clone();
        c.Id = Guid.NewGuid();
        return c;
    }

    private bool Persist()
    {
        try { Storage.SaveTemplates(_templates); return true; }
        catch (Exception ex) { Warn("Kaydedilemedi: " + ex.Message); return false; }
    }

    private void SaveNew_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (name.Length == 0) { Warn("Lütfen bir şablon adı yazın."); return; }
        if (_current.Count == 0) { Warn("Kaydedilecek kart yok. Önce kart ekleyin."); return; }

        var existing = _templates.FirstOrDefault(t =>
            string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (existing != null)
        {
            if (!Ask($"\"{existing.Name}\" adlı şablon zaten var. Üzerine yazılsın mı?")) return;
            existing.Items = Snapshot();
            if (Persist()) Refresh(existing.Id);
            return;
        }

        var t = new LaunchTemplate { Name = name, Items = Snapshot() };
        _templates.Add(t);
        if (Persist()) Refresh(t.Id);
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        var sel = Selected;
        if (sel == null) { Warn("Önce listeden bir şablon seçin."); return; }

        var name = NameBox.Text.Trim();
        if (name.Length == 0) { Warn("Lütfen yeni adı kutuya yazın."); return; }
        if (_templates.Any(t => t.Id != sel.Id &&
                string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase)))
        { Warn("Bu adda başka bir şablon var."); return; }

        sel.Name = name;
        if (Persist()) Refresh(sel.Id);
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        var sel = Selected;
        if (sel == null) { Warn("Önce listeden bir şablon seçin."); return; }
        if (_current.Count == 0) { Warn("Şu an hiç kartınız yok."); return; }
        if (!Ask($"\"{sel.Name}\" şablonunun içeriği şu anki kartlarınızla değiştirilsin mi?")) return;

        sel.Items = Snapshot();
        if (Persist()) Refresh(sel.Id);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        var sel = Selected;
        if (sel == null) { Warn("Önce listeden bir şablon seçin."); return; }
        if (!Ask($"\"{sel.Name}\" şablonu silinsin mi?")) return;

        _templates.Remove(sel);
        NameBox.Text = "";
        if (Persist()) Refresh(null);
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        var sel = Selected;
        if (sel == null) { Warn("Önce listeden bir şablon seçin."); return; }
        if (_current.Count > 0 &&
            !Ask("Mevcut kartlar kaldırılıp şablondakilerle değiştirilecek.\n(İsterseniz önce mevcut listeyi şablon olarak kaydedin.)\n\nDevam edilsin mi?"))
            return;

        ItemsToApply = sel.Items.Select(Fresh).ToList();
        Replace = true;
        DialogResult = true;
    }

    private void Merge_Click(object sender, RoutedEventArgs e)
    {
        var sel = Selected;
        if (sel == null) { Warn("Önce listeden bir şablon seçin."); return; }

        ItemsToApply = sel.Items.Select(Fresh).ToList();
        Replace = false;
        DialogResult = true;
    }

    private void Warn(string msg) =>
        MessageBox.Show(this, msg, "Hızlı Erişim", MessageBoxButton.OK, MessageBoxImage.Warning);

    private bool Ask(string msg) =>
        MessageBox.Show(this, msg, "Hızlı Erişim", MessageBoxButton.YesNo, MessageBoxImage.Question)
        == MessageBoxResult.Yes;
}
