# Server Update Accounting (Apache di grahafajar + Cloudflare Tunnel)

Menyajikan `https://update.kskgroup.web.id/accounting/version.json` yang dibaca `UpdateCheckService`.
Pola sama dengan Finance: menumpang di vhost `update.kskgroup.web.id` yang sudah ada
(ingress cloudflared tidak diubah, Apache hanya reload graceful).

```
Klien ──https──> Cloudflare ──tunnel──> Apache 127.0.0.1:80 (vhost update.kskgroup.web.id)
                                         ├── /smartmillscale/ → /srv/smartmillscale-updates/public
                                         ├── /finance/        → /srv/finance-update/finance
                                         └── /accounting/     → /srv/accounting-update/accounting  (baru)
```

## Setup (sekali)
```powershell
scp -r .\deploy\update-server ssh.kskgroup.web.id:/tmp/accounting-update-server
ssh ssh.kskgroup.web.id "sudo bash /tmp/accounting-update-server/setup-update-server.sh"
```
Script idempotent: membuat folder (label SELinux `httpd_sys_content_t`), memasang `accounting.conf` ke
`/etc/httpd/conf.d/update-vhost.d/`, `apachectl configtest` (rollback otomatis bila gagal), lalu reload.

## Rilis
1. Naikkan `FileVersion` di `Accounting/Accounting.csproj`, publish, buat `GL_Setup.zip`.
2. Unggah paket **dulu**, lalu `version.json` (contoh: `Accounting/Utilities/Update/version.json`):
```powershell
scp GL_Setup.zip   dharyadi@ssh.kskgroup.web.id:/srv/accounting-update/accounting/
scp version.json   dharyadi@ssh.kskgroup.web.id:/srv/accounting-update/accounting/
```
3. Cek: `curl -I https://update.kskgroup.web.id/accounting/version.json` → 200, `application/json`, `no-cache`.
   File lain di `/accounting/` selain `version.json` dan `GL_Setup*.zip|exe` → 403.

## Rollback
Lihat baris terakhir output setup (perintah `cp -p ...before-accounting-*.bak`), lalu
`sudo rm -f /etc/httpd/conf.d/update-vhost.d/accounting.conf && sudo apachectl configtest && sudo systemctl reload httpd`.
