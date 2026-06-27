#!/bin/bash
# Backup automático do MySQL — adicionar ao cron:
# 0 3 * * * /usr/local/bin/backup-cvagora.sh

BACKUP_DIR="/var/backups/cvagora"
DB_NAME="cvagora"
DB_USER="cvagora_user"
KEEP_DAYS=14

mkdir -p "${BACKUP_DIR}"
FILENAME="${BACKUP_DIR}/cvagora_$(date +%Y%m%d_%H%M%S).sql.gz"

mysqldump -u "${DB_USER}" -p"${DB_PASSWORD}" "${DB_NAME}" | gzip > "${FILENAME}"

# Apagar backups com mais de KEEP_DAYS dias
find "${BACKUP_DIR}" -name "cvagora_*.sql.gz" -mtime +${KEEP_DAYS} -delete

echo "Backup guardado: ${FILENAME}"
