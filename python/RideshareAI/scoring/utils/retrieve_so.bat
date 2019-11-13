@echo off
echo | set /p="Preparing container... "
for /f "usebackq" %%A in (`docker run -d --rm -it mcr.microsoft.com/azure-functions/python /bin/bash`) do set CID=%%A
docker cp %~dp0 %CID%:/tmp/ > nul 2>&1
if errorlevel 1 goto stop
echo done
echo | set /p="Installing necessary libraries into container... "
docker exec %CID% sh -c "mkdir -p /tmp/lib && apt update && apt install $(cat /tmp/utils/additional_packages.txt | grep -v \"^#\" | tr \"\n\" \" \") -y && cp -L $(ldconfig -p | grep -E \"($(cat /tmp/utils/necessary_libraries.txt ^| grep -v '^#' ^| tr '\n' '^|' ^| sed -E 's/\^|{1,}$//'))\" | sed -E 's/.* ([^ ]+$)/\1/') /tmp/lib" > nul 2>&1
if errorlevel 1 goto stop
echo done
echo | set /p="Copy libraries from container... "
docker cp %CID%:/tmp/lib/ . > nul 2>&1
if errorlevel 1 goto stop
echo done
echo | set /p="Shutting down container... "
docker kill %CID% > nul 2>&1
if errorlevel 1 goto stop
echo done
echo Finished.
exit /b 0

:stop
echo error occurred
echo code: %errorlevel%
exit /b 1