#!/bin/bash
# =============================================================
# CV Agora — Setup inicial do VPS DigitalOcean (Ubuntu 24.04)
# Correr como root: bash setup-vps.sh
# =============================================================
set -e

echo "========================================"
echo "  CV Agora — Setup VPS DigitalOcean"
echo "========================================"

# Pedir dados ao utilizador
read -p "Domínio (ex: cvagora.cv): " DOMAIN
read -p "Email para Let's Encrypt: " EMAIL
read -s -p "Password para o MySQL (cvagora_user): " DB_PASSWORD
echo
read -s -p "Password para o painel admin: " ADMIN_PASSWORD
echo

# ── 1. Actualizar sistema ─────────────────────────────────
echo ""
echo "▶ 1/8 Actualizando sistema..."
apt-get update -qq
apt-get upgrade -y -qq
apt-get install -y -qq curl wget git unzip ufw nginx certbot python3-certbot-nginx

# ── 2. Instalar .NET 8 ───────────────────────────────────
echo "▶ 2/8 Instalando .NET 8..."
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
dpkg -i /tmp/packages-microsoft-prod.deb
apt-get update -qq
apt-get install -y -qq dotnet-sdk-8.0

# ── 3. Instalar MySQL ────────────────────────────────────
echo "▶ 3/8 Instalando MySQL 8..."
apt-get install -y -qq mysql-server

# Configurar MySQL
mysql -e "CREATE DATABASE IF NOT EXISTS cvagora CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -e "CREATE USER IF NOT EXISTS 'cvagora_user'@'localhost' IDENTIFIED BY '${DB_PASSWORD}';"
mysql -e "GRANT ALL PRIVILEGES ON cvagora.* TO 'cvagora_user'@'localhost';"
mysql -e "FLUSH PRIVILEGES;"

echo "  ✓ MySQL configurado"

# ── 4. Criar utilizador de sistema ───────────────────────
echo "▶ 4/8 Criando utilizador de sistema..."
id -u cvagora &>/dev/null || useradd -m -s /bin/bash cvagora
mkdir -p /var/www/cvagora
chown cvagora:cvagora /var/www/cvagora

# ── 5. Configurar firewall ───────────────────────────────
echo "▶ 5/8 Configurando firewall..."
ufw --force enable
ufw allow OpenSSH
ufw allow 'Nginx Full'

# ── 6. Configurar Nginx ──────────────────────────────────
echo "▶ 6/8 Configurando Nginx..."
cat > /etc/nginx/sites-available/cvagora << NGINX
server {
    listen 80;
    server_name ${DOMAIN} www.${DOMAIN};

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_read_timeout 90s;
    }

    location /static {
        alias /var/www/cvagora/wwwroot;
        expires 30d;
        add_header Cache-Control "public, immutable";
    }

    # Proteger o painel admin com rate limiting
    location /admin {
        limit_req zone=admin_limit burst=10 nodelay;
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }

    # Gzip
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml;
    gzip_min_length 1000;
}
NGINX

# Rate limiting para o painel admin
cat >> /etc/nginx/nginx.conf << 'NGINXCONF'

# Rate limiting (adicionar dentro do bloco http{})
# limit_req_zone $binary_remote_addr zone=admin_limit:10m rate=5r/m;
NGINXCONF

ln -sf /etc/nginx/sites-available/cvagora /etc/nginx/sites-enabled/cvagora
rm -f /etc/nginx/sites-enabled/default
nginx -t && systemctl reload nginx

# ── 7. Criar serviço systemd ─────────────────────────────
echo "▶ 7/8 Criando serviço systemd..."
cat > /etc/systemd/system/cvagora.service << SERVICE
[Unit]
Description=CV Agora — ASP.NET Core Web App
After=network.target mysql.service

[Service]
Type=simple
User=cvagora
WorkingDirectory=/var/www/cvagora
ExecStart=/usr/bin/dotnet /var/www/cvagora/CvAgora.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=cvagora
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=DB_PASSWORD=${DB_PASSWORD}
Environment=ADMIN_USER=admin
Environment=ADMIN_PASSWORD=${ADMIN_PASSWORD}

[Install]
WantedBy=multi-user.target
SERVICE

systemctl daemon-reload
systemctl enable cvagora

# ── 8. SSL com Certbot ───────────────────────────────────
echo "▶ 8/8 Configurando SSL..."
certbot --nginx -d "${DOMAIN}" -d "www.${DOMAIN}" --non-interactive --agree-tos -m "${EMAIL}" || \
    echo "  ⚠ Certbot falhou. Configurar SSL manualmente depois de apontar o DNS."

# Renovação automática
echo "0 12 * * * root certbot renew --quiet" >> /etc/cron.d/certbot-renew

# ── Criar script de deploy ────────────────────────────────
cat > /usr/local/bin/deploy-cvagora << 'DEPLOY'
#!/bin/bash
set -e
echo "▶ A fazer deploy de CV Agora..."
cd /tmp
git clone https://github.com/SEU_UTILIZADOR/cvagora.git cvagora-deploy 2>/dev/null || \
    (cd cvagora-deploy && git pull)
cd cvagora-deploy
dotnet publish src/CvAgora.Web/CvAgora.Web.csproj -c Release -o /tmp/cvagora-publish
systemctl stop cvagora || true
cp -r /tmp/cvagora-publish/* /var/www/cvagora/
chown -R cvagora:cvagora /var/www/cvagora
systemctl start cvagora
rm -rf /tmp/cvagora-deploy /tmp/cvagora-publish
echo "✓ Deploy concluído!"
DEPLOY
chmod +x /usr/local/bin/deploy-cvagora

echo ""
echo "========================================"
echo "  ✅ Setup concluído!"
echo "========================================"
echo ""
echo "  Próximos passos:"
echo "  1. Apontar DNS do domínio ${DOMAIN} para este IP"
echo "  2. Fazer upload do código: scp -r ./publish/* root@IP:/var/www/cvagora/"
echo "  3. Iniciar serviço: systemctl start cvagora"
echo "  4. Ver logs: journalctl -u cvagora -f"
echo "  5. Painel admin: https://${DOMAIN}/admin"
echo ""
