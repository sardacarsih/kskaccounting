#!/usr/bin/env bash
# Menambahkan /accounting/ ke vhost Apache update.kskgroup.web.id yang sudah ada (server grahafajar).
# Cloudflare Tunnel sudah mengarahkan update.kskgroup.web.id -> Apache 127.0.0.1:80, jadi
# cloudflared tidak disentuh. Jalankan sebagai root dari folder ini:
#   sudo ./setup-update-server.sh [deploy-user]
# Idempotent; config Apache divalidasi dan dikembalikan otomatis bila gagal.

set -euo pipefail

DEPLOY_USER="${1:-${SUDO_USER:-dharyadi}}"
HOSTNAME_UPDATE="update.kskgroup.web.id"
WEB_ROOT="/srv/accounting-update"
INCLUDE_DIR="/etc/httpd/conf.d/update-vhost.d"
INCLUDE_FILE="$INCLUDE_DIR/accounting.conf"
INCLUDE_LINE="    IncludeOptional $INCLUDE_DIR/*.conf"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ $EUID -ne 0 ]]; then
    echo "Jalankan sebagai root (sudo)." >&2
    exit 1
fi

if ! id "$DEPLOY_USER" >/dev/null 2>&1; then
    echo "User upload '$DEPLOY_USER' tidak ada." >&2
    exit 1
fi

command -v httpd >/dev/null 2>&1 || { echo "Apache httpd tidak ditemukan." >&2; exit 1; }

# 1. Cari vhost update yang sudah ada
VHOST_FILE=$(grep -lE "^\s*ServerName\s+$HOSTNAME_UPDATE\s*$" /etc/httpd/conf.d/*.conf | head -1 || true)
if [[ -z "$VHOST_FILE" ]]; then
    echo "Vhost $HOSTNAME_UPDATE tidak ditemukan di /etc/httpd/conf.d." >&2
    exit 1
fi
echo "==> Vhost: $VHOST_FILE"

# 2. Folder file update: ditulis user upload, dibaca Apache
echo "==> Menyiapkan $WEB_ROOT/accounting (pemilik $DEPLOY_USER)"
install -d -m 755 "$WEB_ROOT"
install -d -m 755 -o "$DEPLOY_USER" -g "$DEPLOY_USER" "$WEB_ROOT/accounting"

if command -v selinuxenabled >/dev/null 2>&1 && selinuxenabled; then
    semanage fcontext -a -t httpd_sys_content_t "$WEB_ROOT(/.*)?" 2>/dev/null \
        || semanage fcontext -m -t httpd_sys_content_t "$WEB_ROOT(/.*)?"
    restorecon -R "$WEB_ROOT"
fi

# 3. Config Apache (dengan backup untuk rollback)
BACKUP="$VHOST_FILE.before-accounting-$(date +%Y%m%d-%H%M%S).bak"
cp -p "$VHOST_FILE" "$BACKUP"
HAD_INCLUDE_FILE=0
[[ -f "$INCLUDE_FILE" ]] && HAD_INCLUDE_FILE=1 && cp -p "$INCLUDE_FILE" "$INCLUDE_FILE.bak"

rollback() {
    echo "!! Config Apache tidak valid, mengembalikan konfigurasi lama." >&2
    cp -p "$BACKUP" "$VHOST_FILE"
    if [[ $HAD_INCLUDE_FILE -eq 1 ]]; then
        mv -f "$INCLUDE_FILE.bak" "$INCLUDE_FILE"
    else
        rm -f "$INCLUDE_FILE"
    fi
    exit 1
}

install -d -m 755 "$INCLUDE_DIR"
install -m 644 "$SCRIPT_DIR/accounting-update.apache.conf" "$INCLUDE_FILE"
command -v restorecon >/dev/null 2>&1 && restorecon -R "$INCLUDE_DIR" || true

if grep -qF "IncludeOptional $INCLUDE_DIR/" "$VHOST_FILE"; then
    echo "==> IncludeOptional sudah ada di vhost"
else
    echo "==> Menambahkan IncludeOptional ke vhost (backup: $BACKUP)"
    # Insert before the closing tag of the vhost block that holds the update ServerName.
    awk -v host="$HOSTNAME_UPDATE" -v line="$INCLUDE_LINE" '
        /<VirtualHost/ { invhost = 1; match_host = 0 }
        invhost && $1 == "ServerName" && $2 == host { match_host = 1 }
        /<\/VirtualHost>/ && invhost { if (match_host && !done) { print line; done = 1 } invhost = 0 }
        { print }
        END { if (!done) exit 2 }
    ' "$BACKUP" > "$VHOST_FILE.tmp" || { rm -f "$VHOST_FILE.tmp"; rollback; }
    cat "$VHOST_FILE.tmp" > "$VHOST_FILE"
    rm -f "$VHOST_FILE.tmp"
fi

apachectl configtest || rollback
rm -f "$INCLUDE_FILE.bak"

# 4. Reload graceful: koneksi berjalan tidak diputus
systemctl reload httpd
echo "==> Apache di-reload"

# 5. Tes lokal lewat Host header (sama seperti yang dikirim cloudflared)
probe() {
    curl -s -o /dev/null -w '%{http_code}' -H "Host: $HOSTNAME_UPDATE" "http://127.0.0.1$1"
}
echo "==> Tes lokal:"
echo "    /accounting/version.json                   -> $(probe /accounting/version.json)  (404 wajar sebelum rilis pertama)"
echo "    /accounting/rahasia.txt                   -> $(probe /accounting/rahasia.txt)  (harus 403/404)"
echo "    /smartmillscale/releases.stable.json   -> $(probe /smartmillscale/releases.stable.json)  (SmartMillScale tetap 200)"

cat <<EOF

Selesai. Rilis dari mesin Windows (root repo Accounting):
  .\\tools\\publish-update.ps1 -NotesFile .\\catatan.txt -UploadTarget $DEPLOY_USER@ssh.kskgroup.web.id
Cek dari luar:
  curl -I https://$HOSTNAME_UPDATE/accounting/version.json
Rollback config: cp -p "$BACKUP" "$VHOST_FILE" && rm -f "$INCLUDE_FILE" && apachectl configtest && systemctl reload httpd
EOF
