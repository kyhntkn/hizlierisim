using System.Diagnostics;
using System.Windows;

namespace HizliErisim;

public static class Launcher
{
    public static void Open(LaunchItem item)
    {
        try
        {
            if (item.Type == ItemType.Url)
            {
                Process.Start(new ProcessStartInfo(Helpers.NormalizeUrl(item.Target)) { UseShellExecute = true });
                return;
            }

            if (!File.Exists(item.Target) && !Directory.Exists(item.Target))
            {
                MessageBox.Show($"Yol bulunamadı:\n{item.Target}\n\nTaşınmış ya da silinmiş olabilir. Kartı sağ tıklayıp düzenleyebilirsiniz.",
                    "Hızlı Erişim", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var psi = new ProcessStartInfo(item.Target) { UseShellExecute = true };
            if (item.Type == ItemType.App && File.Exists(item.Target))
            {
                var dir = Path.GetDirectoryName(item.Target);
                if (!string.IsNullOrEmpty(dir)) psi.WorkingDirectory = dir;
            }
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Açılamadı: " + ex.Message, "Hızlı Erişim", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
