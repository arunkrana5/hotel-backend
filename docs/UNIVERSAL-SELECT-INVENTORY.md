# HOTEL OS — UNIVERSAL SELECT DROPDOWN INVENTORY & SPECIFICATION

**Architecture Version:** 2.5 Enterprise  
**Design Standard:** Universal Form Controls & Overlay Architecture  
**Component Standard:** `UniversalSelect` (`@/components/common/UniversalSelect`)  
**Status:** Completed & Globally Integrated (100% Native `<select>` Eradicated)

---

## 1. Executive Summary & Architecture Overview

HotelOS previously suffered from fragmented, disjointed dropdown components:
1. Native HTML `<select>` elements with inconsistent styling between operating systems and browsers.
2. Incomplete autocomplete textboxes lacking keyboard controls and portal-safe overlays.
3. Dropdowns with hardcoded styles that clipped behind open modals, drawers, or table boundaries.
4. Unbounded data fetching causing performance bottlenecks on high-volume masters.

To solve this fundamentally across the enterprise, the **`UniversalSelect`** component was built and rolled out as the universal dropdown engine for HotelOS.

### Core Architectural Capabilities
- **Mode A (Database-Driven Masters):** Queries `/api/v1/masters/{masterType}/options`. Strictly retrieves an initial batch of **5 items** (`pageSize=5`). Supports debounced remote search, server-side pagination ("Show More" / incremental fetching), and preserves previously selected items even when they do not appear in the current search page.
- **Mode B (Configured / Static Options):** Handles static domain lookups, priority levels, document types, and status filters with client-side instant search, icon decorators, badge chips, and descriptions.
- **Cascading Dependencies:** Native support for `parentValueId` (e.g., Property -> Floor -> Room, Country -> State -> City). Automatically resets child selection and re-fetches relevant records when the parent value changes.
- **Keyboard Navigation:** Full ARIA compliance (Up/Down arrow navigation, Enter to select, Escape to close, automatic focus on internal search bar).
- **Zero-Clipping Portal Rendering:** Renders dropdown popovers into the `OverlayPortal` (`#hotelos-overlay-root` / `document.body`) at `z-index: 2000`, positioning dynamically relative to trigger bounds with smart viewport-boundary collision detection.
- **Multi-Select & Single-Select:** Supports tag-based multi-selection with individual removal chips, alongside single-value selection with optional one-click clearing.

---

## 2. Universal API Contract (`MasterService.cs`)

All dynamic selections route through the unified backend endpoint:
```http
GET /api/v1/masters/{masterType}/options?page=1&pageSize=5&search=&parentValueId=
```

### Supported Dynamic Master Types in Backend
| Master Type Code | Data Source | Search Fields | Batch Size | Cascading Key |
|---|---|---|---|---|
| `PROPERTY` | `pms.properties` | Name, Code | 5 | N/A |
| `ROOM` | `pms.rooms` | RoomNumber, Floor | 5 | `parentValueId` (PropertyId) |
| `ROOM_TYPE` | `pms.room_types` | Name, Code | 5 | N/A |
| `GUEST` | `pms.guests` | FirstName, LastName, Email, Phone | 5 | N/A |
| `VENDOR` | `procurement.vendors` | Name, Code, Email | 5 | N/A |
| `CUSTOMER` | `identity.users` / Guests | FullName, Email | 5 | N/A |
| `ACCOUNT` | `finance.accounts` | AccountNumber, Name | 5 | N/A |
| `EMPLOYEE` | `hr.employees` | FirstName, LastName, EmployeeId | 5 | `parentValueId` (DepartmentId) |
| `DEPARTMENT` | `hr.departments` | Name, Code | 5 | N/A |
| `PAYMENT_METHOD`| `pms.payment_methods` | Name, Code | 5 | N/A |
| `GENERIC_MASTER`| `system.master_values` | Name, Code | 5 | `parentValueId` (ParentMasterValueId) |

---

## 3. Global Dropdown Inventory Across All Modules

| Screen / Feature | Field / Filter | Mode | Master Type / Static Source | Batching / Search | Status |
|---|---|---|---|---|---|
| **Dashboard** | Period / Status Filter | Mode B | Static (Day, Week, Month, Quarter) | Local Search | Migrated |
| **Front Office - Arrivals** | Assign Room | Mode A | `ROOM` | 5/batch + Remote Search | Migrated |
| **Front Office - Departures** | Settlement Payment Method | Mode A | `PAYMENT_METHOD` | 5/batch + Remote Search | Migrated |
| **Front Office - Folios** | Charge Account / Transaction Code | Mode A | `ACCOUNT` | 5/batch + Remote Search | Migrated |
| **Front Office - Guest Requests** | Request Category & Priority | Mode B | Static Options | Local Search | Migrated |
| **Guest Directory** | VIP Status Level | Mode B | Static (Standard, Silver, Gold, Platinum) | Local Search | Migrated |
| **Guest Directory** | ID Document Type | Mode B | Static (Passport, Driving License, National ID) | Local Search | Migrated |
| **Housekeeping - Tasks** | Room Selector | Mode A | `ROOM` | 5/batch + Remote Search | Migrated |
| **Housekeeping - Tasks** | Task Type | Mode B | Static (Check-out Clean, Daily Tidy, Deep Clean) | Local Search | Migrated |
| **Housekeeping - Tasks** | Priority Level | Mode B | Static (Low, Medium, High, Urgent) | Local Search | Migrated |
| **Housekeeping - Linen** | Room Selector | Mode A | `ROOM` | 5/batch + Remote Search | Migrated |
| **Housekeeping - Linen** | Linen Service Category | Mode B | Static (Bedding, Bath Towels, Pool, Uniforms) | Local Search | Migrated |
| **Housekeeping - Status Board** | Room Cleanliness Filter | Mode B | Static (Clean, Dirty, Inspected, Out of Order) | Local Search | Migrated |
| **Restaurant - Menu Items** | Course / Category | Mode B | Static (Starters, Mains, Desserts, Beverages) | Local Search | Migrated |
| **Restaurant - Menu Items** | Dietary Restrictions | Mode B | Static Multi-Select (Veg, Vegan, GF, Nut-Free) | Local Search | Migrated |
| **Purchase - Orders** | Vendor Selector | Mode A | `VENDOR` | 5/batch + Remote Search | Migrated |
| **Purchase - Requisitions** | Requesting Department | Mode A | `DEPARTMENT` | 5/batch + Remote Search | Migrated |
| **Purchase - Requisitions** | Urgency / Priority | Mode B | Static (Normal, Urgent, Emergency) | Local Search | Migrated |
| **Inventory - Items** | Item Category | Mode B | Static (F&B, Cleaning, Linens, Amenities, Engineering) | Local Search | Migrated |
| **Inventory - Items** | Movement Transaction Type | Mode B | Static (Stock In, Stock Out, Transfer, Write-Off) | Local Search | Migrated |
| **Finance - Invoices** | Invoice Category | Mode B | Static (Room Stay, Banquet, Outlet POS, Corporate) | Local Search | Migrated |
| **Finance - Invoices** | Payment Mode | Mode A | `PAYMENT_METHOD` | 5/batch + Remote Search | Migrated |
| **Finance - General Ledger** | Chart of Accounts Selector | Mode A | `ACCOUNT` | 5/batch + Remote Search | Migrated |
| **HR - Staff Directory** | Department Selector | Mode A | `DEPARTMENT` | 5/batch + Remote Search | Migrated |
| **HR - Staff Directory** | Employment Type | Mode B | Static (Full-Time, Part-Time, Contractor, Seasonal) | Local Search | Migrated |
| **HR - Attendance** | Shift Type & Status | Mode B | Static (Morning, Evening, Night, Present, Absent) | Local Search | Migrated |
| **Maintenance - Work Orders** | Asset Selector | Mode B | Static / Domain Master | Local Search | Migrated |
| **Maintenance - Work Orders** | Severity / Priority | Mode B | Static (P1-Critical, P2-High, P3-Normal, P4-Low) | Local Search | Migrated |
| **Security - Visitor Log** | Department Visited | Mode A | `DEPARTMENT` | 5/batch + Remote Search | Migrated |
| **Security - Incidents** | Incident Severity & Status | Mode B | Static (Minor, Moderate, Major, Investigating, Closed) | Local Search | Migrated |
| **IT Admin - User Management** | Role Assignment | Mode B | Static / Roles (Admin, Manager, Front Desk, Cashier) | Local Search | Migrated |
| **IT Admin - Document Numbering** | Date Format Segment | Mode B | Static (YYYY, YYYYMM, YYYYMMDD, None) | Local Search | Migrated |
| **IT Admin - Document Numbering** | Reset Period | Mode B | Static (Never, Annually, Monthly, Daily) | Local Search | Migrated |
| **IT Admin - Master Values** | Parent Master Value | Mode A | `GENERIC_MASTER` | 5/batch + Cascading | Migrated |
| **Reservations - New Booking** | Room Type & Meal Plan | Mode A/B | `ROOM_TYPE` & Static Options | 5/batch + Remote Search | Migrated |
| **Common - DataTable** | Rows Per Page | Mode B | Static (10, 25, 50, 100) | Compact Inline Trigger | Migrated |
| **Common - DynamicForm** | Dynamic Field Dropdowns | Mode A/B | Dynamic Definition or Static | 5/batch + Remote Search | Migrated |

---

## 4. Elimination of Native `<select>` Tags

All occurrences of `<select>` in the frontend source tree have been replaced:
1. `src/components/common/Input.tsx`: Upgraded the legacy `Select` component to an adapter wrapping `UniversalSelect`. Existing screens importing `Select` automatically inherit `UniversalSelect` styling, search, and portal overlay behavior without breaking API signatures.
2. `src/components/dynamic-dropdown/DynamicDropdown.tsx`: Refactored to delegate directly to `UniversalSelect` Mode A.
3. `src/components/data-table/DataTable.tsx`: Replaced native rows-per-page `<select>` with compact `UniversalSelect`.
4. `src/components/dynamic-form/DynamicForm.tsx`: Replaced fallback select with `UniversalSelect`.

**Result:** Zero raw HTML `<select>` elements remain in runtime user interfaces.
