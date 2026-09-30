$date = Get-Date -Format "yyyy-MM-dd"
$backupName = "backup_$date.sql"

Write-Host "Taking database backup..."
# Database ka exact case-sensitive naam "SupportPulse_db" use kiya hai
cmd.exe /c "docker exec -i support-pulse-db-1 pg_dump -U postgres -d `"SupportPulse_db`" > $backupName"

Write-Host "Backup successfully saved as $backupName"