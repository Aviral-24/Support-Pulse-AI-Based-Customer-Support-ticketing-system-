#!/bin/bash
# SupportPulse Automated Restore Script

# Check karna ki user ne backup file ka naam diya hai ya nahi
if [ -z "$1" ]; then
  echo "Error: Backup file ka path dena zaroori hai!"
  echo "Example: bash scripts/restore.sh ./backups/supportpulse_db_20260915.sql"
  exit 1
fi

BACKUP_FILE=$1
echo "Restoring database from $BACKUP_FILE..."

# Backup file (SQL) ko read karke chalte hue PostgreSQL docker container me inject karna
cat $BACKUP_FILE | docker-compose -f docker-compose.prod.yml exec -T db psql -U postgres -d SupportPulse_db

echo "✅ Database restore successfully completed!"