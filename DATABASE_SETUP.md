# HotelOS – Backend Database Setup, Reset & Cloning Guide

This guide provides instructions for database management, automated reset, and setting up the backend on a new machine.

---

## 1. Automated Database Reset & Clean Re-initialization

To reset all data in `hotelos_db` back to a clean state with baseline enterprise seed data:

### Windows Batch (1-Click)
```cmd
reset_db.bat
```
*(Or `reset_db.bat -y` to bypass confirmation).*

### PowerShell
```powershell
.\reset_db.ps1 -Force
```

### Direct .NET CLI Command
```bash
dotnet run --project src/HotelOS.Api -- --reset-db
```

### SQL Truncate Only (Preserving Schema)
```bash
psql -U postgres -d hotelos_db -f scripts/truncate_all_tables.sql
```

---

## 2. Setting Up on Another System (Cloning)

### Prerequisites
- PostgreSQL 14+ running on port `5432`
- .NET 10 SDK

### Setup Steps
1. **Clone repository:**
   ```bash
   git clone https://github.com/arunkrana5/hotel-backend.git
   ```
2. **Create empty database:**
   In PostgreSQL (`psql` or `pgAdmin`):
   ```sql
   CREATE DATABASE hotelos_db;
   ```
3. **Check connection string:**
   Inspect `src/HotelOS.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=hotelos_db;Username=postgres;Password=postgres"
   }
   ```
   If your PostgreSQL password is not `postgres`, change `Password=YOUR_PASSWORD`.
4. **Run Backend:**
   ```bash
   dotnet run --project src/HotelOS.Api
   ```
   *No manual SQL scripts are required!* EF Core automatically creates all 18 schemas, runs all migrations, and seeds initial data (rooms, superadmin, navigation, etc.).

---

## 3. Default System Credentials
- **SuperAdmin Username**: `admin`
- **SuperAdmin Password**: `Admin@HotelOS2026!`
- **API URL**: `http://localhost:5000`
