$ErrorActionPreference = "Stop"
$SERVER = "root@85.235.74.24"
$REMOTE_DIR = "~/schoolinfo"

Write-Host "=== [1/5] API Derleniyor (Release) ==="
dotnet publish src/SchoolInfo.API/SchoolInfo.API.csproj -c Release -o schoolinfo-api-publish

Write-Host "=== [2/5] Web Derleniyor (Release) ==="
dotnet publish src/SchoolInfo.Web/SchoolInfo.Web.csproj -c Release -o schoolinfo-web-publish

Write-Host "=== [3/5] Dosyalar Sunucuya Yukleniyor ==="
scp -r schoolinfo-api-publish ${SERVER}:${REMOTE_DIR}/
scp -r schoolinfo-web-publish ${SERVER}:${REMOTE_DIR}/
scp docker-compose.yml ${SERVER}:${REMOTE_DIR}/docker-compose.yml
scp Dockerfile.schoolinfo-api ${SERVER}:${REMOTE_DIR}/Dockerfile.schoolinfo-api
scp Dockerfile.schoolinfo-web ${SERVER}:${REMOTE_DIR}/Dockerfile.schoolinfo-web

# .env might not exist locally, so suppress error if it fails
try { scp .env ${SERVER}:${REMOTE_DIR}/.env } catch {}

Write-Host "=== [5/5] Docker Yeniden Baslatiliyor ==="
ssh $SERVER "chmod -R 755 ${REMOTE_DIR}/schoolinfo-api-publish ${REMOTE_DIR}/schoolinfo-web-publish && cd ${REMOTE_DIR} && docker compose up --build -d"
Write-Host "DEPLOYMENT SUCCESSFUL"
