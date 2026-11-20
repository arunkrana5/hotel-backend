# HOTEL OS — COMPLETE UI/UX & FUNCTIONAL REDESIGN AUDIT INVENTORY

## 1. Executive Summary & Audit Scope

This document provides a comprehensive audit of all **48 navigation routes**, screens, forms, actions, ledgers, and components across the HotelOS enterprise hospitality platform. The audit assesses action availability, tab categorization, filter layout, theme compatibility, financial accounting adherence, and user journey complexity.

---

## 2. Global UI Defect & Inconsistency Findings

| Category | Observed Issue | Impact | Redesign Mandate |
|---|---|---|---|
| **Action Availability** | Inconsistent Add/Edit/Delete actions across modules. Some rows only show text or a single edit button; "View Details", "Activate/Deactivate", and contextual "More Actions" are missing. | Operators cannot inspect full records or perform life-cycle actions without editing. | Standardize a unified `RowActions` component on every table row: View Details (eye), Edit (pencil), and More Actions (three dots) with Activate/Deactivate and Delete. |
| **Status Tabs** | Several listing screens lack status-based tab navigation or have hardcoded filter buttons scattered in different places. | Users cannot quickly view records by operational state (e.g., Clean/Dirty, Draft/Posted, Confirmed/Checked-In). | Introduce universal `PageTabs` component with real database-backed count badges, keyboard accessibility, and high contrast active states. |
| **Filter Row Layout** | Filters and search boxes are scattered across multiple rows or placed haphazardly above or inside data tables. | Wastes vertical screen space, causes layout jumps, and confuses operators. | Implement ONE horizontal `ListToolbar` containing: compact search (debounced), status/property dropdowns, date picker, reset action, and advanced filter drawer trigger. |
| **Active Tab Color Contrast** | Under certain theme combinations, active tab text was rendering with poor contrast or white-on-white. | Unreadable navigation states. | Define explicit active/hover/focus design tokens (`text-brand-primary` / `bg-brand-primary/10` or solid primary pill) with WCAG AAA contrast ratio in both Light and Dark themes. |
| **Financial Ledgers** | General Ledger and Stores Ledger screens looked like generic master-data CRUD tables rather than enterprise accounting software. | Lacks running balances, debit/credit column separation, voucher drilldown, and audit integrity. | Re-architect into full SAP/Dynamics-grade double-entry ledger interfaces with KPI metric cards (Opening, Debit, Credit, Closing), running balances, and voucher source document modal. |
| **Forms & Creation Flows** | Create/Edit forms were full page transitions that lost page context, or inconsistent modal sizes. | Disrupts operator workflows during high-frequency front-desk or restaurant tasks. | Standardize on responsive `FormDrawer` (slide-over) for high-frequency CRUD operations, preserving table state and context. |
| **Document Numbering** | Transactions previously lacked read-only display of backend-generated document sequence numbers. | Risk of duplicate or conflicting transaction identifiers. | Display the authoritative backend document number as the first read-only field using the central sequence engine. |

---

## 3. Comprehensive Module-by-Module Route Inventory

| # | Module | Route | Screen Component | Current State | Required Redesign & Enhancements | Status |
|---|---|---|---|---|---|---|
| 1 | **Dashboard** | `/dashboard` | `DashboardScreen` | Operations Command Center with KPIs | Refine KPI typography, live Room Status Board, Department revenue velocity | Ready |
| 2 | **Front Office** | `/front-office/room-grid` | `RoomGridScreen` | Interactive room matrix by floor | Add floor filter tabs, instant status change drawer, reservation binding | In Progress |
| 3 | **Front Office** | `/front-office/arrivals` | `ArrivalsScreen` | Today's arrivals feed | Add VIP status badges, One-click Check-in modal, room assigner | In Progress |
| 4 | **Front Office** | `/front-office/departures` | `DeparturesScreen` | Today's departures feed | Add Folio settlement balance check, One-click Checkout, key return | In Progress |
| 5 | **Front Office** | `/front-office/folios` | `FolioManagementScreen` | Guest billing and charges | Multi-folio split, charge posting, payment recording, invoice print | In Progress |
| 6 | **Front Office** | `/front-office/requests` | `GuestRequestsScreen` | Concierge & guest tickets | Department assignment, priority tags, status progression tabs | In Progress |
| 7 | **Reservations** | `/reservations/list` | `ReservationListScreen` | Reservation directory table | Add status tabs (All, Confirmed, In-House, Completed, Cancelled), View Details drawer | In Progress |
| 8 | **Reservations** | `/reservations/new` | `NewReservationScreen` | Multi-step booking form | Real-time rate calculation, room type selection, guest auto-complete | In Progress |
| 9 | **Reservations** | `/reservations/matrix` | `AvailabilityMatrixScreen` | Room availability grid | 14/30 day calendar view, room type yields, direct booking click | In Progress |
| 10 | **Guests** | `/guests/list` | `GuestDirectoryScreen` | Guest CRM profiles | Tabs: All Guests, In-House, VIP, Corporate; Guest profile drawer | In Progress |
| 11 | **Guests** | `/guests/loyalty` | `GuestDirectoryScreen` | Loyalty tier members | Tier tier badges (Gold/Platinum), points balance, discount rules | In Progress |
| 12 | **Housekeeping** | `/housekeeping/board` | `RoomStatusBoardScreen` | Visual room cleanliness board | Quick status change (Clean/Dirty/Inspected/OOO), attendant assignment | In Progress |
| 13 | **Housekeeping** | `/housekeeping/tasks` | `HousekeepingTasksScreen` | Cleaning task list | Priority badges, floor filters, task time tracking | In Progress |
| 14 | **Housekeeping** | `/housekeeping/linen` | `LinenLaundryScreen` | Linen inventory and laundry | Soiled/Clean counts, vendor dispatch, par level alerts | In Progress |
| 15 | **Restaurant** | `/restaurant/pos` | `LivePosTerminalScreen` | Interactive POS terminal | Table selector, menu category pills, KOT submission, room charge | In Progress |
| 16 | **Restaurant** | `/restaurant/tables` | `TableManagementScreen` | Dining tables directory | Status tabs (All, Available, Occupied, Reserved, Billed), quick action | In Progress |
| 17 | **Restaurant** | `/restaurant/items` | `MenuItemsScreen` | F&B menu catalog | Category tabs, veg/non-veg tags, quick price/availability edit | In Progress |
| 18 | **Kitchen** | `/kitchen/queue` | `KitchenKdsScreen` | Live KDS order monitor | Order elapsed timers, prep status buttons (Ready/Delivered) | In Progress |
| 19 | **Banquet** | `/banquet/events` | `BanquetEventsScreen` | Event booking directory | Status tabs (Inquiry, Confirmed, Completed), function sheets | In Progress |
| 20 | **Banquet** | `/banquet/spaces` | `BanquetSpacesScreen` | Event venues catalog | Seating capacity, day/hourly tariffs, venue photos, amenities | In Progress |
| 21 | **Purchase** | `/purchase/requisitions` | `PurchaseRequisitionsScreen` | Department requisitions | Multi-level approval status tabs, itemized line drawer | In Progress |
| 22 | **Purchase** | `/purchase/orders` | `PurchaseOrdersScreen` | Purchase orders | Status tabs (Draft, Issued, Partially Received, Closed), print PO | In Progress |
| 23 | **Purchase** | `/purchase/vendors` | `VendorDirectoryScreen` | Approved supplier list | Contact drawer, payment terms, tax IDs, active/inactive toggle | In Progress |
| 24 | **Inventory** | `/inventory/items` | `InventoryItemsScreen` | SKU registry & stock levels | Category tabs, reorder level alerts, stock adjustment drawer | In Progress |
| 25 | **Inventory** | `/inventory/stores` | `StoresLedgerScreen` | Stores stock movements | Running balances, Issue/GRN filters, batch numbers | In Progress |
| 26 | **Finance** | `/finance/ledger` | `GeneralLedgerScreen` | Accounting General Ledger | Chart of accounts tree, running balance, debit/credit totals, voucher drilldown | In Progress |
| 27 | **Finance** | `/finance/invoices` | `InvoicesScreen` | Invoicing & settlements | Status tabs (All, Unpaid, Partially Paid, Paid, Overdue), payment drawer | In Progress |
| 28 | **HR** | `/hr/staff` | `StaffDirectoryScreen` | Employee directory | Department tabs, designation, salary grade, active status | In Progress |
| 29 | **HR** | `/hr/attendance` | `AttendanceScreen` | Daily attendance log | Shift filters, punch-in/out records, overtime tracker | In Progress |
| 30 | **Maintenance** | `/maintenance/orders` | `WorkOrdersScreen` | Engineering work orders | Priority tabs (Urgent, High, Medium, Low), technician assigner | In Progress |
| 31 | **Maintenance** | `/maintenance/assets` | `AssetRegisterScreen` | Fixed asset directory | Depreciation status, warranty dates, service logs | In Progress |
| 32 | **Security** | `/security/visitors` | `VisitorLogScreen` | Visitor access book | Check-in/out stamps, pass badge number, destination host | In Progress |
| 33 | **Security** | `/security/incidents` | `IncidentReportsScreen` | Security incident reports | Severity badges, investigation notes, resolution signoff | In Progress |
| 34 | **Reports** | `/reports/center` | `ReportsCenterScreen` | Analytical reports hub | Categorized report cards, export to Excel/PDF, date selectors | In Progress |
| 35 | **Reports** | `/reports/night-audit` | `ReportsCenterScreen` | Night audit settlement ledger | Daily room revenue posting, trial balance verification, roll date | In Progress |
| 36 | **Reports** | `/reports/forecast` | `ReportsCenterScreen` | 30-day occupancy forecast | Predicted ADR, RevPAR, and room pickup trends | In Progress |
| 37 | **Reports** | `/reports/trial-balance`| `ReportsCenterScreen` | Accounting trial balance | Debit/Credit balance equality verification | In Progress |
| 38 | **IT Department** | `/it/dashboard` | `ITDashboardScreen` | System health & operations | Real-time DB connectivity, memory footprint, active users | In Progress |
| 39 | **IT Department** | `/it/users` | `UserManagementScreen` | Staff operator accounts | Add operator drawer, password reset modal, lock/unlock toggle | In Progress |
| 40 | **IT Department** | `/it/roles` | `UserManagementScreen` | Roles & permissions | Role matrix, permission checkboxes, user count | In Progress |
| 41 | **IT Department** | `/it/masters` | `MasterManagementScreen` | Master definitions | Searchable categories, add/edit value drawer, sort order | In Progress |
| 42 | **IT Department** | `/it/dropdowns` | `MasterManagementScreen` | Dynamic dropdown values | Dropdown value editor with active/inactive flags | In Progress |
| 43 | **IT Department** | `/it/menus` | `ITSystemControlScreen` | Navigation menu config | Order manager, icon picker, role permission assignment | In Progress |
| 44 | **IT Department** | `/it/workflows` | `WorkflowManagementScreen` | State transitions & approvals | Step visualization, condition rules, role approvals | In Progress |
| 45 | **IT Department** | `/it/forms` | `DynamicFormViewerScreen` | Dynamic forms engine | Form schema renderer, field validations, submission preview | In Progress |
| 46 | **IT Department** | `/it/security` | `ITSystemControlScreen` | Security events log | Failed logins, token revocation, IP security feed | In Progress |
| 47 | **IT Department** | `/it/audit` | `ITSystemControlScreen` | Enterprise audit trail | Entity modification logs, before/after values, user stamps | In Progress |
| 48 | **IT Department** | `/it/numbering` | `DocumentNumberingScreen` | Document sequence engine | Real-time format preview, prefix/suffix, reset frequencies | In Progress |

---
