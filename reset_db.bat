@echo off
setlocal enabledelayedexpansion

echo ==============================================================================
echo                      HotelOS - DATABASE RESET UTILITY                          
echo ==============================================================================
echo.
echo WARNING: This operation will:
echo   1. Purge all existing data across all 18 PostgreSQL schemas.
echo   2. Re-apply all EF Core migrations from scratch.
echo   3. Initialize fresh enterprise seed data (Rooms 101-305, Rates, SuperAdmin, etc.)
echo.

if "%1"=="--yes" goto execute
if "%1"=="-y" goto execute

set /p CONFIRM="Are you sure you want to completely reset and reseed the database? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo Operation cancelled by user.
    exit /b 0
)

:execute
echo.
echo [1/3] Terminating any existing HotelOS API background processes...
taskkill /F /IM HotelOS.Api.exe >nul 2>&1

echo [2/3] Resetting schemas and executing database migrations & seeding...
cd /d "%~dp0"
dotnet run --project src/HotelOS.Api -- --reset-db

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Database reset encountered an error. Please verify PostgreSQL is running.
    exit /b %ERRORLEVEL%
)

echo.
echo ==============================================================================
echo [SUCCESS] HotelOS Database successfully reset and initialized from scratch!
echo.
echo Default System SuperAdmin Credentials:
echo   Username: admin
echo   Password: Admin@HotelOS2026!
echo.
echo Web Application: http://localhost:3000
echo Backend API:     http://localhost:5000
echo ==============================================================================
echo.
