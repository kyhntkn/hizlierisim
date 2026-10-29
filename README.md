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
- **Şablonlar** düğmesi: mevcut kartları adlandırılmış şablon olarak kaydet, geri yükle (değiştir veya ekle), adını değiştir, güncelle, sil. Dosya: %LocalAppData%\HizliErisim\templates.json
- Veriler: %LocalAppData%\HizliErisim\items.json

## Microsoft Store (MSIX)
Paket kimliği `Package/AppxManifest.xml` içinde Partner Center bilgilerinizle hazır
(Name, Publisher, PublisherDisplayName). GitHub Actions her çalışmada `HizliErisim.msix` üretir.

1. Actions -> Artifacts -> `HizliErisim` içindeki `HizliErisim.msix` dosyasını indirin.
2. Partner Center -> Hızlı Erişim -> Submission -> Packages bölümüne bu .msix dosyasını yükleyin
   (imzalamanız gerekmez, Store imzalar).
3. `runFullTrust` yetkisi için "restricted capability" gerekçesi sorulur. Örnek:
   "Uygulama, kullanıcının seçtiği dosya, klasör ve programları başlatmak için tam güven gerektirir."
4. Mağaza sayfası: açıklama, ekran görüntüleri, yaş derecelendirmesi ve (gerekirse) gizlilik politikası girilir.

Not: Her yüklemede sürüm numarası artmalıdır; sürüm otomatik olarak `1.0.<çalıştırma no>.0` olur.
