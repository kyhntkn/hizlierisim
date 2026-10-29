# Hızlı Erişim

Dosya, klasör, link ve uygulamaları logolarıyla kartlar halinde gösteren hızlı erişim paneli (WPF, .NET 8).

## GitHub'da derleme (önerilen)
1. Bu klasörün İÇERİĞİNİ (HizliErisim.csproj dosyası repo kökünde olacak şekilde) yeni bir GitHub reposuna yükleyin.
2. Repo -> **Actions** sekmesi -> "Build EXE" iş akışı otomatik çalışır (çalışmazsa "Run workflow").
3. Bitince çalıştırma sayfasının altındaki **Artifacts** bölümünden `HizliErisim.zip` indirin; içinde .exe dosyaları vardır.
4. Kalıcı indirme linki için etiket atın: `git tag v1.0.0 && git push origin v1.0.0` -> **Releases** sayfasına .exe'ler eklenir.

## EXE üretme (bilgisayarda)
1. .NET 8 SDK kurun: https://dotnet.microsoft.com/download/dotnet/8.0
2. Bu klasörde `build.bat` dosyasını çalıştırın.
   - `publish\portable\HizliErisim.exe` -> tek dosya, ek kurulum gerekmez (büyük)
   - `publish\small\HizliErisim.exe`    -> küçük, .NET 8 Desktop Runtime gerekir

Geliştirme için: `dotnet run` veya Visual Studio 2022 ile `HizliErisim.csproj` dosyasını açın.

## Kullanım
- Üstteki düğmelerle Dosya/Klasör, Link ya da Uygulama ekleyin.
- Dosya, klasör, .exe/.lnk veya tarayıcıdan link sürükleyip pencereye bırakarak da ekleyebilirsiniz.
- Karta tıklayınca açılır. Sağ tık: Düzenle / Sil.
- Veriler: %LocalAppData%\HizliErisim\items.json

## Microsoft Store (MSIX)
1. Partner Center'da geliştirici hesabı açın ve uygulama adını ayırtın.
2. Visual Studio 2022 -> çözüme "Windows Application Packaging Project" ekleyin,
   Applications bölümüne HizliErisim projesini referans verin.
3. Package.appxmanifest içinde `runFullTrust` yeteneği otomatik gelir (diğer uygulamaları/dosyaları açmak için gerekli).
4. Görseller: `Assets\Store\` klasöründeki hazır boyutlar (StoreLogo, Square44x44, Square71x71,
   Square150x150, Square310x310, Wide310x150) manifest'e atanır.
5. Proje sağ tık -> Publish -> Create App Packages -> Microsoft Store -> yükleme dosyasını (.msixupload) Partner Center'a yükleyin.
