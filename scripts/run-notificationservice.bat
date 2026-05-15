@echo off

set LOGFILE=NotificationServiceLog.txt

powershell -Command  "dapr run --app-id notificationservice --app-port 5235 --dapr-http-port 3513 --resources-path ./components  -- dotnet run --project .\NotificationServiceApp\NotificationServiceApp.csproj  | Tee-Object -FilePath %LOGFILE%"

pause

