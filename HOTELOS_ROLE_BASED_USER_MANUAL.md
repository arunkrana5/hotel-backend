# HotelOS – Role-Based Enterprise User Manual & Operational Reference

Welcome to the **HotelOS Universal Operating Manual**. This document covers every module, menu, user role, and operational workflow across the HotelOS SaaS PMS/ERP platform, complete with step-by-step procedures, concrete real-world examples, inputs, expected outputs, and troubleshooting tips.

---

## Quick Navigation: Roles & Responsibilities

1. [🛡️ IT Administrator & SuperAdmin](#1-it-administrator--superadmin) *(Crucial System Engine)*
2. [🛎️ Front Office & Reception](#2-front-office--reception)
3. [📅 Reservations & Revenue Management](#3-reservations--revenue-management)
4. [👥 Guest Relations & VIP CRM](#4-guest-relations--vip-crm)
5. [🧹 Housekeeping & Laundry Management](#5-housekeeping--laundry-management)
6. [🍽️ Restaurant POS & Food and Beverage](#6-restaurant-pos--food-and-beverage)
7. [👨‍🍳 Kitchen Display System (KDS)](#7-kitchen-display-system-kds)
8. [🎪 Banquets, Conferences & Event Sales](#8-banquets-conferences--event-sales)
9. [🛒 Purchase & Vendor Procurement](#9-purchase--vendor-procurement)
10. [📦 Inventory & Central Stores](#10-inventory--central-stores)
11. [💰 Finance, Accounting & Night Audit](#11-finance-accounting--night-audit)
12. [👔 Human Resources & Staff Shift Rostering](#12-human-resources--staff-shift-rostering)
13. [🔧 Engineering & Facility Maintenance](#13-engineering--facility-maintenance)
14. [🚨 Security, Gate Control & Incident Reporting](#14-security-gate-control--incident-reporting)
15. [📊 Executive Reporting, Analytics & Intelligence](#15-executive-reporting-analytics--intelligence)

---

## Accessing the In-UI User Manual
HotelOS provides an interactive User Manual accessible directly within the application:
- **Top Header**: Click the 📖 **BookOpen icon** in the top navigation bar.
- **Direct Route**: Navigate to **`http://localhost:3000/manual`** (or `/help` / `/docs`).
- **Command Palette**: Press `Ctrl + K` and search for *"User Manual"* or *"IT Guide"*.
- **Print / PDF**: Click `[Print Manual]` in the header of `/manual` to generate an official PDF training handbook.

---

## 1. IT Administrator & SuperAdmin
> **Role Importance:** Highest privilege level. Responsible for global configuration, master taxonomy, dynamic forms, approval workflows, document numbering architecture, role-permission matrices, and database maintenance.

---

### Task 1.1: Master Categories & Dynamic Dropdown Configuration
- **Menu Path:** `IT Department > Master Management` (`/it/masters` or `/it/dropdowns`)
- **Key Objective:** Manage global hotel reference data (room types, departments, tax slabs, rate codes, bed types) without touching source code.

#### Operating Steps:
1. Navigate to **IT Department > Master Management**.
2. Select a **Master Category** from the left-hand category tree:
   - `ORGANIZATION` (Departments, Designations)
   - `ROOMS` (Room Types, Room Statuses, Bed Types)
   - `RESERVATIONS` (Booking Channels, Market Segments, Cancellation Policies)
   - `BILLING` (Payment Modes, Currency Slabs, Tax Slabs)
   - `OPERATIONS` (Maintenance Priorities, Task Types)
3. Click a **Master Definition** (e.g., `ROOM_TYPE`).
4. Click **[+ Add Value]**.
5. Input **Code** (uppercase, e.g. `CHALET`), **Display Name**, **Sort Order**, and **Description**.
6. Ensure **IsActive** is toggled ON and click **Save**.

#### Concrete Real-World Example:
* **Scenario:** The resort opens a new wing of 4 private villas and needs the new room type available across Front Desk and Booking Engine.
* **Entered Inputs:**
  - *Category:* `ROOMS`
  - *Definition:* `ROOM_TYPE`
  - *Code:* `VILLA`
  - *Display Name:* `Private Plunge Pool Villa`
  - *Sort Order:* `9`
  - *Description:* `120sqm luxury villa with private infinity plunge pool and dedicated butler.`
* **Expected Outcome:** The `VILLA` option immediately appears in Room Grid, Room Allocation, and New Reservation dropdowns across all user sessions.
* **Pro Tip:** Never delete an existing master value that has historical reservation or folio records attached; toggle `IsActive = False` instead to preserve database foreign key integrity.

---

### Task 1.2: Document Numbering System Architecture
- **Menu Path:** `IT Department > Document Numbering` (`/it/numbering`)
- **Key Objective:** Configure automated, legally compliant, consecutive document numbering sequences across all 16 business modules.

#### Supported Format Components:
- **Prefix:** Text prefix (e.g. `RES-`, `INV-`, `FOL-`, `BNQ-`, `PO-`).
- **Date Format:** `yyyyMMdd` (e.g., `20261119`), `yyyyMM` (e.g., `202611`), `yyyy` (e.g., `2026`), or `none`.
- **Reset Frequency:** `Daily (Midnight)`, `Monthly (1st of month)`, `Yearly (Jan 1)`, or `Never (Continuous)`.
- **Sequence Length:** Padded zero digits (e.g., `4` digits yields `0001`).
- **Starting Number:** First integer counter (default: `1`).

#### Concrete Real-World Example:
* **Scenario:** Configure Banquet Event Bookings to generate monthly numbers like `BNQ-202611-0001`.
* **Entered Inputs:**
  - *Document Type:* `BANQUET_BOOKING`
  - *Prefix:* `BNQ-`
  - *Date Format:* `yyyyMM`
  - *Reset Frequency:* `Monthly`
  - *Sequence Digits:* `4`
  - *Starting Number:* `1`
* **Expected Outcome:** The real-time preview displays `BNQ-202611-0001`. The next booking will be numbered `BNQ-202611-0001`, rolling automatically on December 1st to `BNQ-202612-0001`.

---

### Task 1.3: Dynamic Form Builder & Validation Rules
- **Menu Path:** `IT Department > Dynamic Forms` (`/it/forms`)
- **Key Objective:** Build custom operational forms (VIP guest intake, health declaration, incident report, facility audit) with dynamic client validation without deploying new frontend code.

#### Operating Steps:
1. Open **Dynamic Forms**. Select the target form schema from the left panel.
2. Review fields: field types include `TEXT`, `NUMBER`, `DATE`, `DROPDOWN`, `CHECKBOX`, `TEXTAREA`.
3. Configure validation rules: `Required`, `Min/Max Length`, `Regex Pattern` (e.g. email regex or serial number format).
4. Use the **Live Form Preview** on the right side to test field layout and trigger validation errors in real-time.

---

### Task 1.4: Dynamic Workflow Engine & Multi-Tier Approvals
- **Menu Path:** `IT Department > Workflows` (`/it/workflows`)
- **Key Objective:** Define state machine transitions and authorized approver roles for high-value transactions.

#### Concrete Real-World Example:
* **Scenario:** Purchase Requisitions exceeding $1,000 require Department Head approval followed by Financial Controller sign-off.
* **Workflow States:** `DRAFT` ➔ `SUBMITTED` ➔ `DEPT_HEAD_APPROVED` ➔ `FINANCE_APPROVED` ➔ `PO_ISSUED`.
* **Transition Guard:** If rejected at any step, state shifts to `REVISION_REQUIRED` with mandatory audit comments returned to the author.

---

### Task 1.5: Database Maintenance, Reset & Multi-System Cloning
- **Command Path:** Server Terminal / `hotel-backend` folder
- **Key Objective:** Wipe test data, apply migrations, or deploy HotelOS on a new computer.

#### Fast Turnkey Procedures:
1. **Reset Database to Pristine Seed State:**
   - Run Windows Batch: `hotel-backend/reset_db.bat -y`
   - Or PowerShell: `.\reset_db.ps1 -Force`
   - Or CLI: `dotnet run --project src/HotelOS.Api -- --reset-db`
2. **Deploying on a New Laptop/Machine:**
   - Clone: `git clone https://github.com/arunkrana5/hotel-backend.git` & `hotal-frontend.git`
   - In PostgreSQL, run once: `CREATE DATABASE hotelos_db;`
   - In `appsettings.json`, ensure your PostgreSQL password is set.
   - Run backend: `dotnet run --project src/HotelOS.Api`. *(All 18 schemas and 64 tables are migrated and seeded automatically).*
   - Run frontend: `npm install && npm run dev`.
   - Log in: `admin` / `Admin@HotelOS2026!`.

---

## 2. Front Office & Reception
> **Role Importance:** Primary guest-facing touchpoint. Responsible for room assignments, guest registration, keycards, folios, and departures.

---

### Task 2.1: Room Allocation Grid
- **Menu Path:** `Front Office > Room Allocation Grid` (`/front-office/room-grid`)
- **Procedure:**
  1. Filter grid by floor (Floor 1, Floor 2, Floor 3) or status (Clean, Dirty, Occupied, Maintenance).
  2. Click any room card to open the **Room Detail Drawer**.
  3. Inspect rate per night, current occupant, housekeeping state, and quick actions:
     - Assign Room to Arriving Guest
     - Flag Out of Order / Request Maintenance
     - View Guest Folio

### Task 2.2: Guest Check-In Workflow
- **Menu Path:** `Front Office > Arrivals & Check-In` (`/front-office/arrivals`)
- **Procedure:**
  1. Locate booking by Confirmation Number (e.g., `RES-202611-0001`) or Guest Surname.
  2. Verify government ID/passport and record contact details.
  3. Select clean room matching room type.
  4. Click **[Complete Check-In]**. Keycard is issued, Room turns Blue (Occupied), and Folio opens.

### Task 2.3: Guest Departures & Bill Settlement
- **Menu Path:** `Front Office > Departures & Checkout` (`/front-office/departures`)
- **Procedure:**
  1. Select checkout guest. Open active folio.
  2. Verify all restaurant, minibar, and laundry charges are posted.
  3. Click **[Receive Payment]** (Credit Card, Cash, Corporate City Ledger).
  4. Once balance is `$0.00`, click **[Complete Checkout]**. Sequential Invoice (e.g., `INV-0001`) is generated and room is flagged Pink (Dirty) for housekeeping turnover.

---

## 3. Reservations & Revenue Management
> **Role Importance:** Maximize occupancy and Average Daily Rate (ADR).

- **Menu Path:** `Reservations > New Reservation` (`/reservations/new`)
- **Procedure:**
  1. Set Stay Dates (Check-In, Check-Out) and Occupancy (Adults, Children).
  2. System queries real-time availability across all 8 room categories.
  3. Select Room Type (`DLX`, `EXEC`, `OCN`, `PRES`) and Rate Plan (Best Available Rate, Corporate, MAP).
  4. Input guest contact details, corporate billing codes, and special requests (e.g. Late Checkout).
  5. Click **[Create Reservation]**. Auto-numbers with sequence (e.g., `RES-202611-0003`).

---

## 4. Guest Relations & VIP CRM
> **Role Importance:** Maintain VIP guest profiles, stay history, dietary preferences, and loyalty status.

- **Menu Path:** `Guest Profiles & CRM > Guest Directory` (`/guests/list`)
- **Procedure:**
  1. Search guest by name, mobile number, or email.
  2. Inspect lifetime spend, total stays, and loyalty tier (Silver, Gold, Platinum).
  3. Record guest preferences (e.g., "Non-smoking room, high floor, extra hypoallergenic pillows, almond milk").

---

## 5. Housekeeping & Laundry Management
> **Role Importance:** Room cleanliness, hygiene standards, turnover timing, and linen par ratios.

- **Menu Path:** `Housekeeping > Room Status Board` (`/housekeeping/board`)
- **Turnover Cycle:**
  1. **Vacated / Dirty (`DRT`):** Attendant cleans room, changes linen, disinfects bathroom.
  2. **Attendant Complete (`CLN`):** Marks task finished.
  3. **Supervisor Inspection (`INS`):** Supervisor verifies standards against checklist. Once certified, room unlocks for front desk check-in.
- **Linen Par Levels (`/housekeeping/linen`):**
  - Monitor Clean Stock vs. Par Level for King Sheets, Pillow Cases, Bath Towels.
  - Dispatch soiled batches to commercial laundry with tracked Laundry Orders (`LND-0001`).

---

## 6. Restaurant POS & Food and Beverage
> **Role Importance:** Dining table service, in-room dining, bar orders, and guest billing.

- **Menu Path:** `Restaurant & POS > Live POS Terminal` (`/restaurant/pos`)
- **Procedure:**
  1. Click an available dining table (e.g., Table 4).
  2. Browse menu categories (Starters, Mains, Beverages, Desserts). Click items to add to cart.
  3. Add prep modifiers (e.g. "Medium Rare", "No Garlic").
  4. Click **[Send to Kitchen]** ➔ Kitchen Order Ticket (KOT) sends instantly to Kitchen KDS.
  5. When guest requests the bill, click **[Settle Bill]** ➔ Settle via Card/Cash or click **[Post to Room]** to charge guest folio directly.

---

## 7. Kitchen Display System (KDS)
> **Role Importance:** Order preparation queue, station routing, and kitchen turnaround timing.

- **Menu Path:** `Kitchen (KDS) > Kitchen Queue` (`/kitchen/queue`)
- **Procedure:**
  1. Incoming KOTs appear with table number, quantities, and live elapsed timers.
  2. Color alerts: Green (<10 min), Amber (10–20 min), Red (>20 min overdue).
  3. Chef clicks **[Start Prep]** ➔ Notifies floor servers food is cooking.
  4. Chef plates dish and clicks **[Bump / Mark Ready]** ➔ Runner alert sounds for table delivery.

---

## 8. Banquets, Conferences & Event Sales
> **Role Importance:** Venue hall bookings, wedding galas, corporate conferences, and catering packages.

- **Menu Path:** `Banquets & Events > Event Bookings` (`/banquet/events`)
- **Procedure:**
  1. Select Hall/Space (e.g., Grand Crystal Ballroom, Royal Pavilion).
  2. Select Event Type, start/end time, and guaranteed guest count.
  3. Choose catering menu package (e.g., Gala Buffet @ $85/head).
  4. Record advance deposit. Auto-generates booking number (`BNQ-202611-0001`) and dispatches function sheet to Executive Chef.

---

## 9. Purchase & Vendor Procurement
> **Role Importance:** Vendor selection, purchase requisitions, and purchase orders.

- **Menu Path:** `Purchase & Vendor > Purchase Requisitions / Orders` (`/purchase/requisitions`)
- **Procedure:**
  1. Department submits Purchase Requisition (`PR-2026-0001`) for supplies.
  2. Purchase Manager selects approved vendor from Vendor Directory (`/purchase/vendors`).
  3. Quotes unit rates and generates Purchase Order (`PO-2026-0001`).
  4. Upon delivery, storekeeper records Goods Received Note (GRN) to update inventory stores.

---

## 10. Inventory & Central Stores
> **Role Importance:** Par levels, stock issuance, stocktakes, and shrinkage prevention.

- **Menu Path:** `Inventory & Stores > Inventory Items` (`/inventory/items`)
- **Procedure:**
  1. Track stock on hand against minimum reorder points.
  2. Record Department Stock Issues (e.g. issuing 10 boxes of guest soaps to Housekeeping). Central stores stock reduces; department expense is booked in General Ledger.
  3. Conduct monthly physical stocktakes and log variance adjustments.

---

## 11. Finance, Accounting & Night Audit
> **Role Importance:** Double-entry ledger, accounts receivable/payable, tax compliance, and daily night audit.

- **Menu Path:** `Finance & Accounts > General Ledger / Invoices` (`/finance/ledger`)
- **Procedure:**
  1. **General Ledger:** Create balanced Journal Vouchers (`JV-202611-0001`). Total Debits must equal Total Credits.
  2. **Night Audit Execution:**
     - Verify all outlet cash registers have closed.
     - Post room revenue and room tax charges to all in-house folios.
     - Roll business date.
     - Generate Trial Balance, Daily Revenue Report, and RevPAR metrics.

---

## 12. Human Resources & Staff Shift Rostering
> **Role Importance:** Staff profiles, employee departments, and shift attendance.

- **Menu Path:** `HR & Personnel > Staff Directory / Attendance` (`/hr/staff`)
- **Procedure:**
  1. Maintain employee records (department, designation, emergency contacts).
  2. Log daily shift attendance (Morning, Evening, Night shift clock-in/out).
  3. Audit attendance summaries for monthly payroll processing.

---

## 13. Engineering & Facility Maintenance
> **Role Importance:** Asset uptime, HVAC, plumbing, electrical, and room repairs.

- **Menu Path:** `Engineering & Maintenance > Work Orders` (`/maintenance/orders`)
- **Procedure:**
  1. Front Desk or Housekeeping logs defect (e.g. Room 105 water heater).
  2. Chief Engineer assigns duty technician with Priority: High/Urgent.
  3. Technician resolves fault, records spare parts used, and signs off. Room returns to Housekeeping for sanitation.

---

## 14. Security, Gate Control & Incident Reporting
> **Role Importance:** Physical safety, visitor badges, keycard access audits, and loss prevention.

- **Menu Path:** `Security & Surveillance > Visitor Log / Incident Reports` (`/security/visitors`)
- **Procedure:**
  1. Log visitor/contractor name, vehicle number, host department, and issue visitor badge.
  2. Log any property damage or guest lost & found in Incident Reports (`INC-2026-0001`).
  3. Audit gate log at end of shift to verify all badges are surrendered.

---

## 15. Executive Reporting, Analytics & Intelligence
> **Menu Path:** `Reports Hub > Executive Center` (`/reports/center`)

Key real-time reports available for instant viewing and CSV / PDF export:
- **Daily Manager's Flash Report** (Rooms Sold, Occupancy %, ADR, RevPAR).
- **Night Audit Revenue Summary** (Department breakdown: Rooms, F&B, Banquets, Spa).
- **Guest Folio Aging & Accounts Receivable Ledger**.
- **Housekeeping Turnover Efficiency & Average Turnaround Minutes**.
- **F&B Outlet Sales & Item Velocity Report**.
- **Inventory Stock Reorder & Consumption Analysis**.
