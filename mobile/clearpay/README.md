# ClearPay mobile (Flutter)

JWT istemci. Para kuralı C# sitede. Bakiye Hive/SQLite’da tutulmaz. 8 ekran: giriş, kayıt, özet, havale, yükle/çek, hareket, dekont, admin (rol).

Site ayakta olmalı: `dotnet run --project src/ClearPay.Web --launch-profile http` → [http://localhost:5153](http://localhost:5153).

Windows’ta Flutter **cmd** ile (PowerShell değil):

```bat
cd /d C:\Users\clt\Projects\clearpay\mobile\clearpay
flutter doctor
flutter build windows
flutter run -d windows
```

- Windows / iOS simülatör: `http://localhost:5153`
- Android emülatör: `http://10.0.2.2:5153`
- Kayıt ve havale uygulamada. Footer: **Demo — sahte banka gateway.**
- Mağaza / HTTPS: TASK-16 (sen tıklarsın).

`ClearPay.slnx` bu klasörü içermez. CI `dotnet test` kalır.
