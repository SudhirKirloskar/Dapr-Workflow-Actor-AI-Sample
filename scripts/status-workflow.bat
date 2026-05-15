@echo off
set /p workflowId="Enter Workflow GUID: "
set URL=http://localhost:5300/status/%workflowId%

echo Checking workflow status...
powershell -Command "Invoke-WebRequest -Uri %URL% -Method GET -UseBasicParsing | Select-Object -ExpandProperty Content"
pause
