# HotelOS – Database Reset & Multi-System Cloning Guide

This comprehensive guide explains how to:
1. **Reset and re-initialize the database from scratch** (truncate/purge and seed clean enterprise baseline data).
2. **Clone and set up HotelOS on another machine** with the exact database configuration steps.

---

## Part 1: How to Reset & Cleanly Re-Initialize Database Data

HotelOS provides automated and manual options to purge existing test data and restart from ground zero.

### Option A: 1-Click Reset (Recommended)

#### Using Windows Command Prompt / Double-Click:
In folder `D:\HotelOS\hotel-backend` (or inside the cloned `hotel-backend` folder):
```cmd
reset_db.bat
```
*(Or pass `-y` / `--yes` to skip the confirmation prompt: `reset_db.bat -y`)*

#### Using PowerShell:
```powershell
.\reset_db.ps1
```
*(Or with force: `.\reset_db.ps1 -Force`)*

#### Using .NET CLI directly:
```bash
dotnet run --project src/HotelOS.Api -- --reset-db
```

#### What happens automatically:
1. All 18 custom application schemas (`identity`, `pms`, `finance`, `masters`, `housekeeping`, `restaurant`, `banquet`, `purchase`, `inventory`, etc.) are dropped with `CASCADE`.
2. EF Core runs all 6 database migrations (`MigrateAsync`), creating all 64 tables and indexes cleanly.
3. `DatabaseSeeder` automatically populates:
   - **SuperAdmin Account**: Username `admin`, Password `Admin@HotelOS2026!`.
   - **System Roles & Permissions**: SuperAdmin, IT Administrator, Front Desk, Housekeeping, etc.
   - **Hotel Masters**: 10 Departments, 8 Room Types, Room Statuses, Rate Categories, Tax Slabs.
   - **PMS Inventory**: Rooms 101 to 305 across 3 floors with rates and statuses.
   - **Dynamic Navigation**: Database-driven sidebar navigation tree.
   - **Document Numbering**: 16 document sequence formats (e.g., `RES-`, `FOL-`, `INV-`, `BNQ-`).
   - **ERP Baseline**: Vendors, Chart of Accounts, Linen Stock, Restaurant Menu, Banquet Halls.

---

### Option B: SQL-Only Truncate (Preserving Schemas)

If you prefer using **pgAdmin**, **DBeaver**, or **psql**:

Execute the provided script:
`hotel-backend/scripts/truncate_all_tables.sql`

```bash
psql -U postgres -d hotelos_db -f scripts/truncate_all_tables.sql
```

This runs:
```sql
TRUNCATE TABLE 
    audit.audit_logs, banquet.banquet_bookings, banquet.banquet_venues,
    configuration.document_number_audits, configuration.document_number_configurations,
    configuration.dynamic_form_fields, configuration.dynamic_form_definitions,
    configuration.navigation_menus, finance.journal_lines, finance.journal_entries,
    finance.invoices, finance.accounts, housekeeping.laundry_order_items,
    housekeeping.laundry_orders, housekeeping.housekeeping_tasks, housekeeping.linen_items,
    hr.attendance_records, hr.employees, identity.email_verification_tokens,
    identity.login_attempts, identity.login_history, identity.password_history,
    identity.password_reset_tokens, identity.refresh_tokens,
    identity.user_property_assignments, identity.user_roles, identity.role_permissions,
    identity.user_sessions, identity.user_credentials, identity.users, identity.roles,
    identity.permissions, identity.security_events, inventory.stock_transactions,
    inventory.inventory_items, maintenance.work_orders, maintenance.assets,
    masters.master_values, masters.master_definitions, masters.master_categories,
    operations.guest_service_requests, platform.notifications, pms.folio_items,
    pms.folios, pms.reservations, pms.guests, pms.rooms, purchase.purchase_order_items,
    purchase.purchase_orders, purchase.purchase_requisition_items, purchase.purchase_requisitions,
    purchase.vendors, restaurant.restaurant_order_items, restaurant.restaurant_orders,
    restaurant.menu_items, restaurant.dining_tables, security.incident_reports,
    security.visitor_logs, workflow.workflow_histories, workflow.workflow_instances,
    workflow.workflow_transitions, workflow.workflow_steps, workflow.workflow_definitions
RESTART IDENTITY CASCADE;
```

After running this, restart the backend:
```bash
dotnet run --project src/HotelOS.Api
```
The seeder will detect empty tables and re-initialize the baseline dataset.

---

## Part 2: Cloning HotelOS to Another System – Step-by-Step Guide

When cloning HotelOS to a new laptop, desktop, or server, **you do NOT need to manually create tables, write DDL, or run SQL scripts**. The backend handles migrations and seeding automatically.

### System Prerequisites
Ensure the target machine has:
1. **PostgreSQL** (version 14, 15, 16, or 17).
2. **.NET 10 SDK** (or runtime).
3. **Node.js** (v18.x, v20.x, or v22.x) and `npm`.
4. **Git**.

---

### Step 1: Clone the Repositories
```bash
git clone https://github.com/arunkrana5/hotel-backend.git
git clone https://github.com/arunkrana5/hotal-frontend.git
```

---

### Step 2: Create Empty PostgreSQL Database (Only 1 Command)
On the target machine, open PostgreSQL (via `psql` or `pgAdmin`):
```sql
CREATE DATABASE hotelos_db;
```
*(No need to create schemas or tables — EF Core will create all 18 schemas and 64 tables automatically).*

---

### Step 3: Configure Database Connection
Open `hotel-backend/src/HotelOS.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=hotelos_db;Username=postgres;Password=postgres"
}
```

#### What you need to manually change (if applicable):
- **Password**: If the PostgreSQL password on the target machine is NOT `postgres`, change `Password=YOUR_PASSWORD`.
- **Port**: If PostgreSQL runs on port `5433` (e.g., if multiple PostgreSQL versions are installed), change `Port=5433`.
- **Username**: If using a custom PostgreSQL superuser instead of `postgres`, change `Username=YOUR_USER`.

*Note: If PostgreSQL uses default `postgres` / `postgres` on `5432`, **no file edits are needed at all!***

---

### Step 4: Start Backend (Automatic Migration & Auto-Seeding)
Navigate to `hotel-backend` and run:
```bash
cd hotel-backend
dotnet run --project src/HotelOS.Api
```

#### What happens automatically on first launch:
1. EF Core applies all 6 migration files.
2. Creates all 18 schemas (`identity`, `pms`, `finance`, `masters`, etc.) and 64 tables.
3. Automatically seeds the SuperAdmin user (`admin`), system roles, room inventory, navigation menus, and document numbering sequences.
4. Backend starts listening on `http://localhost:5000`.

---

### Step 5: Start Frontend
In a new terminal window:
```bash
cd hotal-frontend
npm install
npm run dev
```
The frontend starts on `http://localhost:3000`.

---

### Step 6: Log In
Open your browser to:
**`http://localhost:3000`**

Login with the default enterprise administrator account:
- **Username / Email**: `admin`
- **Password**: `Admin@HotelOS2026!`

*(You can also click the "Admin Demo" quick-fill button on the login screen).*

---

## Summary Checklist for New Machine Setup

| Step | Action | Required File / Command |
|---|---|---|
| 1 | Clone code | `git clone ...` (frontend & backend) |
| 2 | Create empty DB | `CREATE DATABASE hotelos_db;` in PostgreSQL |
| 3 | Check DB credentials | `hotel-backend/src/HotelOS.Api/appsettings.json` (match your PG password) |
| 4 | Run Backend | `dotnet run --project src/HotelOS.Api` (auto migrates & auto seeds) |
| 5 | Run Frontend | `npm install && npm run dev` |
| 6 | Log In | `admin` / `Admin@HotelOS2026!` |
