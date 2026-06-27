#!/bin/bash
# =============================================================
# CV Agora — Deploy local → VPS
# Uso: ./deploy.sh usuario@IP_DO_VPS
# =============================================================
set -e

SERVER=${1:-"root@IP_DO_VPS"}
PUBLISH_DIR="./publish"

echo "========================================"
echo "  CV Agora — Deploy para ${SERVER}"
echo "========================================"

echo "▶ A compilar e publicar..."
dotnet publish src/CvAgora.Web/CvAgora.Web.csproj \
    -c Release \
    -o "${PUBLISH_DIR}" \
    --self-contained false \
    -r linux-x64

echo "▶ A enviar ficheiros para o servidor..."
rsync -avz --progress \
    --exclude '*.pdb' \
    --exclude 'appsettings.Development.json' \
    "${PUBLISH_DIR}/" \
    "${SERVER}:/var/www/cvagora/"

echo "▶ A reiniciar o serviço..."
ssh "${SERVER}" "
    chown -R cvagora:cvagora /var/www/cvagora && \
    systemctl restart cvagora && \
    sleep 2 && \
    systemctl status cvagora --no-pager
"

rm -rf "${PUBLISH_DIR}"

echo ""
echo "✅ Deploy concluído!"
echo "   Logs: ssh ${SERVER} 'journalctl -u cvagora -f'"
