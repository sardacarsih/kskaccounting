# Server Update Accounting (Apache di grahafajar + Cloudflare Tunnel)

Menyajikan `https://update.kskgroup.web.id/accounting/latest.json` yang dibaca `UpdateService` (lihat [docs/auto-update.md](../../docs/auto-update.md)).
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
Otomatis lewat skrip (membuat ZIP + `latest.json`, upload ZIP dulu, cek SHA-256 di server, baru `latest.json`):
```powershell
.\tools\publish-update.ps1 -NotesFile .\catatan.txt -UploadTarget dharyadi@ssh.kskgroup.web.id
```
Detail alur dan format manifest: [docs/auto-update.md](../../docs/auto-update.md).

Cek: `curl -I https://update.kskgroup.web.id/accounting/latest.json` -> 200, `application/json`, `no-cache`.
File lain di `/accounting/` selain `latest.json` dan `Accounting-<versi>-win-x64|x86.zip` -> 403.

## Rollback
Lihat baris terakhir output setup (perintah `cp -p ...before-accounting-*.bak`), lalu
`sudo rm -f /etc/httpd/conf.d/update-vhost.d/accounting.conf && sudo apachectl configtest && sudo systemctl reload httpd`.
