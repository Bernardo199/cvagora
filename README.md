# CV Agora — Site dinâmico de Cabo Verde

Stack: **ASP.NET Core 8** + **MySQL 8** + **EF Core** + **DigitalOcean VPS**

## Estrutura do projecto

```
CvAgora/
├── src/
│   ├── CvAgora.Core/           # Entidades e interfaces (domínio puro)
│   ├── CvAgora.Infrastructure/ # EF Core, repositórios, migrações
│   └── CvAgora.Web/            # Controllers, Views, CSS, JS
├── deploy/
│   ├── setup-vps.sh            # Setup inicial do servidor (correr uma vez)
│   └── deploy.sh               # Deploy local → VPS (rsync)
└── scripts/
    └── backup-mysql.sh         # Backup diário da BD
```

## Configuração local (desenvolvimento)

### Pré-requisitos
- .NET 8 SDK
- MySQL 8 (local ou Docker)
- VS Code ou Visual Studio

### 1. Clonar e configurar

```bash
git clone https://github.com/SEU_UTILIZADOR/cvagora.git
cd cvagora
```

Editar `src/CvAgora.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=cvagora;User=root;Password=SUA_PASSWORD;CharSet=utf8mb4;"
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "password_de_teste"
  }
}
```

### 2. Aplicar migrations e correr

```bash
cd src/CvAgora.Infrastructure
dotnet ef database update --startup-project ../CvAgora.Web

cd ../CvAgora.Web
dotnet run
```

Site disponível em: http://localhost:5000
Painel admin: http://localhost:5000/admin

### 3. Adicionar migration (quando mudar entidades)

```bash
cd src/CvAgora.Infrastructure
dotnet ef migrations add NomeDaMigration --startup-project ../CvAgora.Web
dotnet ef database update --startup-project ../CvAgora.Web
```

---

## Deploy no DigitalOcean

### 1. Criar Droplet
- Ubuntu 24.04 LTS
- Plano mínimo recomendado: **2 GB RAM / 1 vCPU / 50 GB SSD** (~$12/mês)
- Adicionar chave SSH

### 2. Setup inicial do servidor (correr apenas uma vez)

```bash
scp deploy/setup-vps.sh root@IP_DO_VPS:/tmp/
ssh root@IP_DO_VPS "bash /tmp/setup-vps.sh"
```

O script instala e configura automaticamente:
- Nginx (reverse proxy)
- .NET 8 Runtime
- MySQL 8
- Certbot / Let's Encrypt (SSL)
- Serviço systemd
- Firewall (ufw)

### 3. Primeiro deploy

```bash
chmod +x deploy/deploy.sh
./deploy/deploy.sh root@IP_DO_VPS
```

### 4. Deploys seguintes

```bash
./deploy/deploy.sh root@IP_DO_VPS
```

---

## Configuração do Google AdSense

1. Entrar no painel admin: `https://seudominio.cv/admin`
2. Ir a **Configurações**
3. Preencher `adsense_client` com o Publisher ID (ex: `ca-pub-1234567890`)
4. Preencher `adsense_slot_1` e `adsense_slot_2` com os Slot IDs

---

## Gerir conteúdo

### Painel Admin → `/admin`
- **Dashboard**: visão geral de artigos, visualizações e subscritores
- **Artigos**: criar, editar, publicar/despublicar, marcar como destaque
- **Categorias**: gerir categorias com cores personalizadas
- **Configurações**: editar textos do hero, IDs do AdSense e Analytics
- **Newsletter**: ver e exportar subscritores

### Cores de categoria disponíveis
- `c-azul` — azul atlântico
- `c-sol` — amarelo sol
- `c-hibisco` — vermelho hibisco
- `c-verde` — verde morabeza

---

## Monitorização no servidor

```bash
# Ver logs em tempo real
journalctl -u cvagora -f

# Reiniciar a aplicação
systemctl restart cvagora

# Estado do serviço
systemctl status cvagora

# Logs do Nginx
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log

# Restaurar backup
gunzip -c /var/backups/cvagora/cvagora_YYYYMMDD_HHMMSS.sql.gz | mysql -u cvagora_user -p cvagora
```

---

## Variáveis de ambiente no servidor

Definidas no ficheiro `/etc/systemd/system/cvagora.service`:
- `DB_PASSWORD` — password do MySQL
- `ADMIN_USER` — utilizador do painel admin
- `ADMIN_PASSWORD` — password do painel admin
- `ASPNETCORE_ENVIRONMENT=Production`

Para alterar: editar o ficheiro e correr `systemctl daemon-reload && systemctl restart cvagora`
