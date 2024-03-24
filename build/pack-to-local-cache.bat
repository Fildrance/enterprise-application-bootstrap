@echo off
set /p buildNumber=EnterBuildNumber:
cd ..
dotnet pack src\Enterprise.ApplicationBootstrap.Logging.Serilog\Enterprise.ApplicationBootstrap.Logging.Serilog.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget
dotnet pack src\Enterprise.ApplicationBootstrap.Core\Enterprise.ApplicationBootstrap.Core.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget
dotnet pack src\Entierprise.ApplicationBootstrap.WebApi\Enterprise.ApplicationBootstrap.WebApi.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget
dotnet pack src\Enterprise.ApplicationBootstrap.Telemetry\Enterprise.ApplicationBootstrap.Telemetry.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget
dotnet pack src\Enterprise.ApplicationBootstrap.Core.Api\Enterprise.ApplicationBootstrap.Core.Api.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget
dotnet pack src\Enterprise.ApplicationBootstrap.DataAccessLayer\Enterprise.ApplicationBootstrap.DataAccessLayer.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget
dotnet pack src\Enterprise.ApplicationBootstrap.DataAccessLayer.PgSql\Enterprise.ApplicationBootstrap.DataAccessLayer.PgSql.csproj -c Release /p:Version=0.0.%buildNumber% -o G:\Fildrance\doc\local-nuget