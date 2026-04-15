@echo off
cd /d "%~dp0"
echo ========================================================
echo Starting ECommerce Product Management System Cluster...
echo ========================================================

echo Starting Identity Service (Port 5010/5011)...
start "Identity Service" cmd /k "cd src\services\IdentityService && dotnet run"

echo Starting Catalog Service (Port 5020/5021)...
start "Catalog Service" cmd /k "cd src\services\CatalogService && dotnet run"

echo Starting Workflow Service (Port 5030/5031)...
start "Workflow Service" cmd /k "cd src\services\ProductWorkflowService && dotnet run"

echo Starting Admin/Reporting Service (Port 5040/5041)...
start "Admin/Reporting Service" cmd /k "cd src\services\AdminReportingService && dotnet run"

echo Starting Ocelot API Gateway (Port 5000/5001)...
start "Ocelot Gateway" cmd /k "cd src\gateway\OcelotGateway && dotnet run"

echo Starting Search Service (Port 5050/5051)...
start "Search Service" cmd /k "cd src\services\SearchService && dotnet run"

echo Starting Angular Frontend (Port 4200)...
start "Angular Frontend" cmd /k "cd src\frontend\angular-app && npm start"

echo.
echo All services have been launched in separate terminal windows!
echo Gateway is listening on: http://localhost:5000
echo Frontend is running on:  http://localhost:4200
echo.
pause
