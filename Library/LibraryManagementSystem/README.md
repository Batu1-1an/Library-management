# Kütüphane Yönetim Sistemi

Bu proje, ASP.NET Core kullanılarak geliştirilmiş bir kütüphane yönetim sistemidir.

## Gereksinimler

- .NET 6.0 SDK veya daha yeni bir sürüm
- Visual Studio 2022 veya Visual Studio Code
- MySQL Server (8.0 veya üzeri)

## Visual Studio Code Eklentileri

Visual Studio Code kullanıyorsanız, aşağıdaki eklentileri yüklemeniz önerilir:

1. C# (ms-dotnettools.csharp) - C# dil desteği
2. C# Dev Kit (ms-dotnettools.csdevkit) - C# geliştirme araçları
3. .NET Core Tools (ms-dotnettools.vscode-dotnet-runtime) - .NET Core araçları
4. NuGet Package Manager (jmrog.vscode-nuget-package-manager) - NuGet paket yönetimi
5. SQL Server (ms-mssql.mssql) - SQL Server bağlantısı ve sorgu çalıştırma


Bu eklentileri yüklemek için:
1. VS Code'da Extensions sekmesini açın (Ctrl+Shift+X)
2. Yukarıdaki eklentileri aratıp "Install" butonuna tıklayın
3. Yükleme tamamlandıktan sonra VS Code'u yeniden başlatın

## Kurulum Adımları

1. Projeyi bilgisayarınıza klonlayın veya ZIP olarak indirin.

2. Visual Studio ile açmak için:
   - LibraryManagementSystem.sln dosyasına çift tıklayın
   - NuGet paketlerinin yüklenmesini bekleyin
   - F5 tuşuna basarak projeyi çalıştırın

3. Visual Studio Code ile açmak için:
   - Terminal'i açın
   - Proje klasörüne gidin: `cd LibraryManagementSystem`
   - Aşağıdaki komutları sırasıyla çalıştırın:
     ```
     dotnet restore
     dotnet build
     dotnet run
     ```

4. Veritabanını oluşturmak için:
   - Package Manager Console'da veya terminal'de:
     ```
     dotnet ef database update
     ```

5. Tarayıcınızda `https://localhost:5001` veya `http://localhost:5000` adresine giderek uygulamayı görüntüleyebilirsiniz.

## Veritabanı Ayarları

Projeyi çalıştırmadan önce MySQL veritabanı ayarlarını yapmanız gerekmektedir:

1. MySQL Server'ı yükleyin ve çalıştığından emin olun
2. `appsettings.json` dosyasındaki bağlantı dizesini kendi MySQL ayarlarınıza göre güncelleyin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=LibraryManagementSystem;User=root;Password=YOUR_PASSWORD;"
     }
   }
   ```
   - Server: MySQL sunucu adresi (genelde localhost)
   - Database: Veritabanı adı (değiştirmenize gerek yok)
   - User: MySQL kullanıcı adınız
   - Password: MySQL şifreniz

## Önemli Notlar

- MySQL Server'ın yüklü ve çalışır durumda olduğundan emin olun
- appsettings.json dosyasındaki MySQL bağlantı bilgilerini kendi ortamınıza göre güncellemeyi unutmayın
- Sisteme ilk kayıt olan kullanıcı otomatik olarak admin yetkisine sahip olacaktır
- Şifre gereksinimleri:
  - En az 8 karakter
  - En az 1 büyük harf
  - En az 1 küçük harf
  - En az 1 rakam
  - En az 1 özel karakter

## Sorun Giderme

1. "The term 'dotnet' is not recognized" hatası alırsanız:
   - .NET SDK'nın doğru şekilde yüklendiğinden emin olun
   - Bilgisayarınızı yeniden başlatın
   - Terminal/PowerShell'i yeniden açın

2. MySQL bağlantı hatası alırsanız:
   - MySQL Server'ın çalıştığından emin olun
   - MySQL kullanıcı adı ve şifrenizin doğru olduğundan emin olun
   - appsettings.json dosyasındaki bağlantı dizesini kontrol edin
   - MySQL'in 3306 portunda çalıştığından emin olun

3. "Unable to create database" hatası alırsanız:
   - MySQL kullanıcınızın veritabanı oluşturma yetkisine sahip olduğundan emin olun
   - Belirtilen veritabanı adının kullanılabilir olduğunu kontrol edin 