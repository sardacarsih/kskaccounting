# Auto Update ("Update Tersedia") dengan migrasi database otomatis

Accounting memeriksa manifest `latest.json` di server HTTPS. Jika versi di server lebih baru dari versi aplikasi, status bar menampilkan tombol hijau **Update Tersedia (vX)**. Pengguna mengunduh paket, aplikasi memverifikasi SHA-256, lalu `Accounting.Updater.exe` mengganti file setelah Accounting ditutup, **menjalankan GLMigrator** bila ada migrasi baru, dan membuka Accounting kembali. Pola ini diporting dari aplikasi Finance.

## Alur

1. Setelah login (`MainView_Load`) dan setiap `CheckIntervalMinutes`, aplikasi mengambil `ManifestUrl`.
2. Ada versi baru -> tombol "Update Tersedia" di status bar. Update wajib (`mandatory` atau di bawah `minimumVersion`) langsung membuka dialog.
3. **Update Sekarang** -> paket diunduh ke `%LOCALAPPDATA%\Accounting\updates\<versi>\`, ukuran dan SHA-256 dicek.
4. `Accounting.Updater.*` disalin ke `%TEMP%\AccountingUpdater\<guid>\` lalu dijalankan; Accounting menutup diri.
5. Updater menunggu Accounting keluar (maks 2 menit), mengekstrak paket ke `_update_staging`, mencadangkan file lama ke `_update_backup\<versi-lama>-<waktu>` (2 backup terakhir disimpan), lalu menyalin file baru.
6. **Migrasi database:** `GLMigrator.exe --mode up`, lalu `--mode verify`, memakai `Utilities\config.json` situs itu (server aktif = `ActiveServerKey`). Riwayat di `GL_MIGRATION_HISTORY` membuat run berulang aman: klien pertama yang update memigrasi database, klien berikutnya tidak menemukan migrasi tertunda.
7. Jika instalasi atau migrasi (up atau verify) gagal, file aplikasi dikembalikan dari backup. **Perubahan database yang sudah diterapkan tidak ikut dibatalkan** (migrasi bersifat additive; hubungi IT sebelum memakai modul terdampak). Accounting dijalankan ulang dalam kedua kasus.
8. Log updater: `<folder aplikasi>\logs\updater-yyyyMMdd.log`; log migrator: `<folder aplikasi>\logs\migrator-update-<waktu>\`.

File yang **tidak pernah ditimpa**: `Utilities\config.json`, `config.json`, folder `logs\`, `_update_staging\`, `_update_backup\`.

Jika folder aplikasi tidak bisa ditulis (mis. di `Program Files`), updater meminta hak administrator (UAC).

Menu **About -> Cek Update** memeriksa secara manual.

## Syarat migrasi otomatis

- `sqlplus` ada di `PATH` komputer yang menjalankan update (syarat GLMigrator).
- Akun database di `config.json` boleh menjalankan DDL migrasi.
- `--mode verify` harus lolos. Jika database sudah punya drift/checksum bentrok lama, update akan gagal dan file dikembalikan; perbaiki dulu dengan GLMigrator (`--mode rebaselinechecksum`, dll.).
- Migrasi khusus PKS (`20260729_001/002`) otomatis dilewati (`[N/A]`) di database yang tidak punya data PKS, jadi tidak lagi menggagalkan update di database kebun.
- `verify` mengompilasi ulang objek INVALID lebih dulu, supaya paket yang hanya "basi" akibat DDL migrasi tidak membuat verifikasi gagal.
- Komputer yang **tidak** boleh/bisa memigrasi (mis. tanpa sqlplus) set `"RunMigrator": false` pada section `Update`; migrasi dilewati dan harus dijalankan oleh komputer lain/IT.

## Konfigurasi klien

Default dibaca dari `App.config` (ikut paket): key `Update:ManifestUrl`. Untuk menimpa, tambahkan section `Update` di `Utilities\config.json`:

```json
"Update": {
  "Enabled": true,
  "ManifestUrl": "https://update.kskgroup.web.id/accounting/latest.json",
  "CheckIntervalMinutes": 240,
  "TimeoutSeconds": 30,
  "RunMigrator": true
}
```

| Key | Keterangan |
| --- | --- |
| `Enabled` | `false` -> fitur nonaktif tanpa error. Section yang ada selalu menang atas `App.config`. |
| `ManifestUrl` | Wajib `https://` (http hanya untuk `localhost`/127.0.0.1 saat pengujian). |
| `CheckIntervalMinutes` | Interval cek otomatis, minimum 15. |
| `TimeoutSeconds` | Timeout request manifest. |
| `RunMigrator` | `false` -> updater tidak menjalankan GLMigrator. Default `true`. |

## Kontrak server

Cukup file statis. Contoh `https://update.kskgroup.web.id/accounting/latest.json` (dibuat otomatis oleh `tools/publish-update.ps1`):

```json
{
  "version": "2.1.0.0",
  "mandatory": false,
  "minimumVersion": "2.0.0.0",
  "packageUrl": "Accounting-2.1.0.0-win-x64.zip",
  "sha256": "<64 hex>",
  "size": 105309106,
  "releaseDate": "2026-10-08",
  "notes": "Perbaikan jurnal dan laporan"
}
```

| Field | Wajib | Keterangan |
| --- | --- | --- |
| `version` | ya | Format `a.b.c.d`, dibandingkan dengan `AssemblyVersion` Accounting. |
| `packageUrl` | ya | Absolut atau relatif terhadap URL manifest. Wajib https. |
| `sha256` | ya | Hex 64 karakter dari file ZIP. Paket ditolak bila tidak cocok. |
| `size` | tidak | Byte; jika diisi, ukuran unduhan harus sama persis. |
| `mandatory` | tidak | `true` -> dialog tidak bisa ditunda. |
| `minimumVersion` | tidak | Klien di bawah versi ini diperlakukan sebagai update wajib. |
| `releaseDate`, `notes` | tidak | Ditampilkan di dialog. |

Isi ZIP = isi folder publish Accounting (Accounting.exe di root) + `Accounting.Updater.*` + `GLMigrator.exe`; tanpa `Utilities\config.json` dan `.pdb`.

Konfigurasi Apache/Cloudflare ada di [deploy/update-server](../deploy/update-server/README.md).

## Langkah rilis

1. Naikkan `<Version>`, `<AssemblyVersion>`, `<FileVersion>` di `Accounting/Accounting.csproj` (ketiganya sama, mis. `2.1.1.0`).
2. Jalankan (PowerShell, dari root repo):
   ```powershell
   .\tools\publish-update.ps1 -NotesFile .\catatan-rilis.txt -UploadTarget dharyadi@ssh.kskgroup.web.id
   ```
   Opsi: `-Mandatory`, `-MinimumVersion 2.0.0.0`, `-Runtime win-x64`, `-OutputDir`. Tanpa `-UploadTarget`, hasilnya hanya dibuat di `publish\update\` (`Accounting-<versi>-win-x64.zip` dan `latest.json`).
   Skrip mem-publish ulang GLMigrator sehingga migrasi SQL terbaru ikut tertanam.
3. Upload ZIP lebih dulu, **baru** `latest.json` (skrip melakukannya dan mencocokkan SHA-256 di server).

## Instalasi pertama

Klien yang belum punya `Accounting.Updater.exe` (versi sebelum fitur ini) harus dipasang manual sekali dari ZIP rilis. Setelah itu update berikutnya otomatis.

## Troubleshooting

| Gejala | Penyebab / tindakan |
| --- | --- |
| Tombol tidak muncul | `Update.Enabled` false, URL salah, atau server tidak terjangkau. Cek log aplikasi ("Cek update gagal"). Gunakan *Cek Update* untuk melihat pesan error. |
| "URL manifest update harus menggunakan https" | Ganti `ManifestUrl`/`packageUrl` ke https. |
| "Checksum SHA-256 ... tidak cocok" | ZIP di server berbeda dengan manifest (upload ulang / cache CDN). |
| "Migrasi database gagal, aplikasi dikembalikan" | Lihat `logs\updater-*.log` dan `logs\migrator-update-*`. Umum: `sqlplus` tidak ada di PATH, kredensial tanpa hak DDL, atau `verify` gagal karena drift. Set `RunMigrator=false` hanya bila migrasi dijalankan pihak lain. |
| "Update dibatalkan karena aplikasi Accounting masih berjalan" | Accounting tidak tertutup dalam 2 menit. Tutup lalu ulangi. |
| "Accounting.Updater.exe tidak ditemukan" | Instalasi lama; pasang manual versi terbaru. |
