@echo off
setlocal

set SERVER=root@YOUR_SERVER_IP
set REMOTE_DIR=~/schoolinfo

:MENU
cls
echo ==================================================
echo          SCHOOLINFO AUTOMATIC DEPLOYMENT
echo ==================================================
echo [1] Tumu (API ve Web)
echo [2] Sadece Backend (API)
echo [3] Sadece Frontend (Web)
echo [4] Iptal / Cikis
echo ==================================================
set /p SECIM="Seciminiz (1-4): "

if "%SECIM%"=="1" goto DEPLOY_ALL
if "%SECIM%"=="2" goto DEPLOY_API
if "%SECIM%"=="3" goto DEPLOY_WEB
if "%SECIM%"=="4" exit /b 0
goto MENU

:DEPLOY_ALL
echo.
echo === [1/5] API Derleniyor (Release) ===
dotnet publish src/SchoolInfo.API/SchoolInfo.API.csproj -c Release -o schoolinfo-api-publish
if %ERRORLEVEL% neq 0 goto ERR_API

echo.
echo === [2/5] Web Derleniyor (Release) ===
dotnet publish src/SchoolInfo.Web/SchoolInfo.Web.csproj -c Release -o schoolinfo-web-publish
if %ERRORLEVEL% neq 0 goto ERR_WEB_BUILD

echo.
echo === [3/5] Dosyalar Sunucuya Yukleniyor ===
scp -r schoolinfo-api-publish %SERVER%:%REMOTE_DIR%/
if %ERRORLEVEL% neq 0 goto ERR_UPLOAD
scp -r schoolinfo-web-publish %SERVER%:%REMOTE_DIR%/
if %ERRORLEVEL% neq 0 goto ERR_UPLOAD
scp docker-compose.yml %SERVER%:%REMOTE_DIR%/docker-compose.yml
if %ERRORLEVEL% neq 0 goto ERR_UPLOAD

echo.
echo === [4/5] Dockerfile'lar Yukleniyor ===
scp Dockerfile.schoolinfo-api %SERVER%:%REMOTE_DIR%/Dockerfile.schoolinfo-api
scp Dockerfile.schoolinfo-web %SERVER%:%REMOTE_DIR%/Dockerfile.schoolinfo-web

echo.
echo === [5/5] Docker Yeniden Baslatiliyor ===
ssh %SERVER% "chmod -R 755 %REMOTE_DIR%/schoolinfo-api-publish %REMOTE_DIR%/schoolinfo-web-publish && cd %REMOTE_DIR% && docker compose up --build -d"
if %ERRORLEVEL% neq 0 goto ERR_DOCKER
goto END

:DEPLOY_API
echo.
echo === [1/4] API Derleniyor (Release) ===
dotnet publish src/SchoolInfo.API/SchoolInfo.API.csproj -c Release -o schoolinfo-api-publish
if %ERRORLEVEL% neq 0 goto ERR_API

echo.
echo === [2/4] API Sunucuya Yukleniyor ===
scp -r schoolinfo-api-publish %SERVER%:%REMOTE_DIR%/
if %ERRORLEVEL% neq 0 goto ERR_UPLOAD
scp docker-compose.yml %SERVER%:%REMOTE_DIR%/docker-compose.yml
scp Dockerfile.schoolinfo-api %SERVER%:%REMOTE_DIR%/Dockerfile.schoolinfo-api

echo.
echo === [3/4] Izinler Ayarlaniyor ===
ssh %SERVER% "chmod -R 755 %REMOTE_DIR%/schoolinfo-api-publish"

echo.
echo === [4/4] Docker (API) Yeniden Baslatiliyor ===
ssh %SERVER% "cd %REMOTE_DIR% && docker compose up --build -d schoolinfo-api"
if %ERRORLEVEL% neq 0 goto ERR_DOCKER
goto END

:DEPLOY_WEB
echo.
echo === [1/4] Web Derleniyor (Release) ===
dotnet publish src/SchoolInfo.Web/SchoolInfo.Web.csproj -c Release -o schoolinfo-web-publish
if %ERRORLEVEL% neq 0 goto ERR_WEB_BUILD

echo.
echo === [2/4] Web Sunucuya Yukleniyor ===
scp -r schoolinfo-web-publish %SERVER%:%REMOTE_DIR%/
if %ERRORLEVEL% neq 0 goto ERR_UPLOAD
scp docker-compose.yml %SERVER%:%REMOTE_DIR%/docker-compose.yml
scp Dockerfile.schoolinfo-web %SERVER%:%REMOTE_DIR%/Dockerfile.schoolinfo-web

echo.
echo === [3/4] Izinler Ayarlaniyor ===
ssh %SERVER% "chmod -R 755 %REMOTE_DIR%/schoolinfo-web-publish"

echo.
echo === [4/4] Docker (Web) Yeniden Baslatiliyor ===
ssh %SERVER% "cd %REMOTE_DIR% && docker compose up --build -d schoolinfo-web"
if %ERRORLEVEL% neq 0 goto ERR_DOCKER
goto END

:ERR_API
echo === HATA: API Derlenemedi ===
goto END

:ERR_WEB_BUILD
echo === HATA: Web Derlenemedi ===
goto END

:ERR_UPLOAD
echo === HATA: Yukleme Yapilamadi ===
goto END

:ERR_DOCKER
echo === HATA: Docker Baslatilamadi ===
goto END

:END
pause
