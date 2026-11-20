# HOTEL OS — UNIFIED LIST TOOLBAR & TABLE INTEGRATION SPECIFICATION

**Architecture Version:** 2.5 Enterprise  
**Design Standard:** Linear / Ramp High-Density Single-Row Enterprise Grid  
**Component Standard:** `DataTable` (`@/components/data-table/DataTable`) & `ListToolbar` (`@/components/enterprise/ListToolbar`)  
**Companion Components:**
- `ColumnVisibilityPopover` (`@/components/enterprise/ColumnVisibilityPopover`)
- `ExportMenu` (`@/components/enterprise/ExportMenu`)
- `UniversalSelect` (`@/components/common/UniversalSelect`)

---

## 1. Executive Summary & Problem Resolution

### The "Split Space" Problem
In previous iterations, listing screens exhibited a confusing, disjointed layout:
1. **Row 1 (Above Card):** An external toolbar or search box rendered on the left, with completely blank, unused space on the right.
2. **Row 2 (Inside Table Card Header):** The table's internal card header had an empty left side, and on the right side sat the Customize (`[ ☲ ]`) and Export (`[ ⤓ Export ]`) buttons.
3. This gave the impression that the search box and export/customize buttons were taking "different spaces" and behaving as two uncoordinated components.

### The Unified Single-Row Solution
To eliminate this defect globally, `DataTable` now integrates the **Unified One-Row Enterprise Toolbar** directly into its card top border:
```
┌────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ [🔍 Search records...] [Filter 1 ▼] [Filter 2 ▼]     [☲ Columns (8/10) ▼] [⤓ Export ▼] [🔄 Refresh] [Action] │
├────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ TX NUMBER   │ DATE & TIME │ INVENTORY SKU │ DEPARTMENT │ TYPE │ MOVEMENT QTY │ TOTAL VALUE ($)         │
├─────────────┼─────────────┼───────────────┼────────────┼──────┼──────────────┼─────────────────────────┤
│ ...         │ ...         │ ...           │ ...        │ ...  │ ...          │ ...                     │
├─────────────┴─────────────┴───────────────┴────────────┴──────┴──────────────┴─────────────────────────┤
│ Showing 1 to 10 of 45 records          Rows: [10 ▼]                        << < Page 1 of 5 > >>       │
└────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

- **Left Side:** Search input (`searchQuery`, `onSearchChange`, clear icon button) + Inline `UniversalSelect` filters + Title badge.
- **Right Side:** `ColumnVisibilityPopover` (powered directly by TanStack Table's column definitions) + `ExportMenu` (CSV, Excel `.xls`, and print-ready PDF) + Refresh button + Actions.
- **Zero Space Waste:** Both search and actions occupy the **exact same 36px (`h-9`) row** at the top of the table card.
- **Zero Duplicate Rows:** The floating, disconnected external toolbar is eliminated from standard listing screens.

---

## 2. Component Specifications

### 2.1 One-Row Toolbar Metrics
| Metric | Specification | Visual Class / Rule |
|---|---|---|
| Desktop Row Height | 36px standard (34–38px acceptable range) | `h-8 sm:h-8.5` (`34-36px`) |
| Control Spacing | 8px horizontal gap | `gap-2` |
| Search Input Width | 200px to 280px | `min-w-[200px] max-w-[280px]` |
| Primary Filter Width | 140px to 190px | `w-36 sm:w-44` |
| Dropdown Height | Compact 34px | `size="sm"` on `UniversalSelect` |
| Button Density | Compact enterprise with icon & label | `size="sm"` (34px height) |

### 2.2 Column Visibility Popover (`ColumnVisibilityPopover`)
- Embedded directly into the toolbar with a clean columns icon and active column count badge (`Columns (8/10)`).
- **LocalStorage Persistence:** Remembers user column visibility preferences per module table (e.g., `hotelos_cols_stores_ledger`).
- Automatically syncs with TanStack Table's `getVisibleLeafColumns()`.
- "Show All", "Hide All", and "Reset Defaults" actions.

### 2.3 Universal Export Menu (`ExportMenu`)
- Consolidated single trigger with downward arrow indicator: `[Export ▼]`.
- Three enterprise formats supported:
  1. **CSV:** Comma-separated UTF-8 with automatic BOM for international Excel compatibility.
  2. **Excel (.xls):** Formatted tabular spreadsheet with tab delimiters and UTF-8 BOM.
  3. **PDF:** Print-ready executive landscape table with timestamp and property header.
- Displays non-blocking notification during generation.

---

## 3. Screen Migration Audit Matrix

All 23 screens now feature the unified single-row toolbar layout:

| Module | Route / Screen | Search Field | Primary Dropdown Filters | Columns Toggle | Export Menu | Status |
|---|---|---|---|---|---|---|
| **Inventory** | `/inventory/stores` | Tx #, Item Code, Dept | Active Tabs | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Front Office** | `/front-office/arrivals` | Guest Name, Booking # | Room Type, Cleanliness | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Front Office** | `/front-office/departures` | Guest Name, Room # | Payment Status, Folio Balance | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Front Office** | `/front-office/guest-requests` | Request Title, Guest | Status, Department | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Guests** | `/guests/directory` | Name, Email, Phone | VIP Tier, ID Document Type | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Housekeeping** | `/housekeeping/tasks` | Room #, Attendant | Clean Status, Priority Level | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Housekeeping** | `/housekeeping/linen` | Item Name, SKU | Service Category, Stock State | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Restaurant** | `/restaurant/menu-items` | Dish Name, Item Code | Menu Category, Dietary | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Purchase** | `/purchase/orders` | PO Number, Vendor | Order Status, Approver | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Purchase** | `/purchase/requisitions` | Req #, Department | Urgency, Approval Stage | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Purchase** | `/purchase/vendors` | Vendor Name, Code | Vendor Category, Status | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Inventory** | `/inventory/items` | Item Name, SKU, Barcode | Item Category, Low Stock Flag | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Finance** | `/finance/invoices` | Invoice #, Guest Name | Status (Paid/Pending/Overdue) | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Finance** | `/finance/general-ledger` | Voucher #, Account Code | Account Classification, Period | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **HR** | `/hr/staff-directory` | Employee Name, ID | Department, Employment Type | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **HR** | `/hr/attendance` | Employee Name, Code | Shift Type, Duty Status | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Maintenance** | `/maintenance/work-orders` | Asset, Issue Summary | Priority (P1-P4), Status | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Maintenance** | `/maintenance/assets` | Asset Tag, Serial, Name | Asset Category | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Security** | `/security/visitor-log` | Visitor Name, Host | Department Visited, Status | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Security** | `/security/incident-reports` | Incident Title, Location | Severity Level, Status | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Banquets** | `/banquet/events` | Event Name, Booking # | Event Status | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **Reservations** | `/reservations/list` | Confirmation #, Guest | Booking Source | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |
| **IT Admin** | `/it-admin/users` | Username, Full Name | Assigned Role, Account State | Persistent | CSV, XLS, PDF | **UNIFIED (1 Row)** |

---

## 4. Verification and Visual Consistency

- **Theme Synchronization:** All toolbar elements, inputs, and popovers dynamically switch between Light Mode (`bg-brand-surface border-brand-border text-brand-text`) and Dark Mode (`bg-brand-darkSurface border-brand-darkBorder text-slate-100`).
- **Focus Rings:** Unified subtle ring focus styling (`focus:ring-1 focus:ring-brand-primary`).
- **Zero Layout Shifts:** Search input and actions are bound inside the table card header, preventing any layout jumping or misaligned flex wraps.
