# ==============================================================================
# HotelOS - PowerShell Database Reset Utility
# ==============================================================================
param (
    [switch]$Force
)

Write-Host "==============================================================================" -ForegroundColor Cyan
Write-Host "                     HotelOS - DATABASE RESET UTILITY                         " -ForegroundColor Cyan
Write-Host "==============================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "WARNING: This operation will:" -ForegroundColor Yellow
Write-Host "  1. Purge all existing data across all 18 PostgreSQL schemas."
Write-Host "  2. Re-apply all EF Core migrations from scratch."
Write-Host "  3. Initialize fresh enterprise seed data (Rooms 101-305, Rates, SuperAdmin, etc.)"
Write-Host ""

if (-not $Force) {
    $confirm = Read-Host "Are you sure you want to completely reset and reseed the database? (Y/N)"
    if ($confirm -ne 'Y' -and $confirm -ne 'y') {
        Write-Host "Operation cancelled by user." -ForegroundColor Gray
        exit 0
    }
}

Write-Host ""
Write-Host "[1/3] Terminating any existing HotelOS API background processes..." -ForegroundColor Yellow
Get-Process -Name "HotelOS.Api" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host "[2/3] Resetting schemas and executing database migrations & seeding..." -ForegroundColor Yellow
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

dotnet run --project src/HotelOS.Api -- --reset-db

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] Database reset encountered an error. Please verify PostgreSQL is running." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "==============================================================================" -ForegroundColor Green
Write-Host "[SUCCESS] HotelOS Database successfully reset and initialized from scratch!" -ForegroundColor Green
Write-Host ""
Write-Host "Default System SuperAdmin Credentials:" -ForegroundColor Cyan
Write-Host "  Username: admin" -ForegroundColor White
Write-Host "  Password: Admin@HotelOS2026!" -ForegroundColor White
Write-Host ""
Write-Host "Web Application: http://localhost:3000" -ForegroundColor Gray
Write-Host "Backend API:     http://localhost:5000" -ForegroundColor Gray
Write-Host "==============================================================================" -ForegroundColor Green
