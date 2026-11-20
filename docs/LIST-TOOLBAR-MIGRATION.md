# HOTEL OS — UNIFIED LIST TOOLBAR MIGRATION SPECIFICATION

**Architecture Version:** 2.5 Enterprise  
**Design Standard:** Linear / Ramp High-Density Enterprise Layout  
**Component Standard:** `ListToolbar` (`@/components/enterprise/ListToolbar`)  
**Companion Components:**
- `ColumnVisibilityPopover` (`@/components/enterprise/ColumnVisibilityPopover`)
- `ExportMenu` (`@/components/enterprise/ExportMenu`)
- `AdvancedFilterDrawer` (`@/components/enterprise/AdvancedFilterDrawer`)

---

## 1. Executive Summary & Design System Problem

In earlier iterations of HotelOS:
- Search bars, filter dropdowns, export buttons, and refresh controls were scattered in multiple rows, uneven card headers, or misaligned flex wraps.
- Controls had inconsistent heights (ranging from 28px to 46px), inconsistent padding, and clashing button styles.
- On standard 1080p and 1440p displays, filters consumed 120px to 180px of vertical space before any data rows were visible.
- Export options were hardcoded or scattered as individual buttons.
- Column visibility toggles were non-persistent or missing entirely.

### The Unified One-Row Toolbar Solution (`ListToolbar`)
The `ListToolbar` unifies all listing actions into **exactly one horizontal row** on desktop displays (34–38px height):
```
┌────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ [🔍 Search...] [Filter 1 ▼] [Filter 2 ▼] [Date Range]  [More Filters (2)] [Columns ▼] [Export ▼] [Action]│
└────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

On tablet and mobile screens (<1024px), secondary filters gracefully condense into the **`AdvancedFilterDrawer`**, while retaining instant access to global search and the primary action.

---

## 2. Component Specifications

### 2.1 One-Row Toolbar Metrics
| Metric | Specification | Visual Class / Rule |
|---|---|---|
| Desktop Row Height | 36px standard (34–38px acceptable range) | `h-9` (`36px`) |
| Control Spacing | 8px horizontal gap | `gap-2` |
| Search Input Width | 240px to 320px | `w-60 md:w-72` |
| Primary Filter Width | 140px to 190px | `w-36 md:w-48` |
| Dropdown Height | Compact 34px | `size="sm"` on `UniversalSelect` |
| Button Density | Compact enterprise with icon & label | `size="sm"` (34px height) |

### 2.2 Column Visibility Popover (`ColumnVisibilityPopover`)
- Embedded directly into the toolbar with a clean columns icon and active column count badge (`Columns (8/10)`).
- **LocalStorage Persistence:** Remembers user column visibility preferences per module table (e.g., `hotelos_columns_front-office-arrivals`).
- "Show All", "Hide All", and "Reset Defaults" actions.
- Drag/toggle reordering support.

### 2.3 Universal Export Menu (`ExportMenu`)
- Consolidated single trigger with downward arrow indicator: `[Export ▼]`.
- Three enterprise formats supported:
  1. **CSV:** Comma-separated UTF-8 with automatic BOM for international Excel compatibility.
  2. **Excel (.xlsx):** Formatted tabular spreadsheet.
  3. **PDF:** Print-ready executive landscape table with timestamp and property header.
- Displays non-blocking notification during generation.

### 2.4 Advanced Filters Drawer (`AdvancedFilterDrawer`)
- Slide-over drawer mounting from right edge (`z-index: 1050`).
- Activates when module requires 4+ filter dimensions (e.g., date ranges, numeric thresholds, multi-select tags).
- Displays active filter count badge on toolbar button (`More Filters (3)`).
- Provides "Apply Filters" and "Clear All" with instant reactive updates.

---

## 3. Screen Migration Audit Matrix

| Module | Route / Screen | Search Field | Primary Dropdown Filters | Columns Toggle | Export Menu | Status |
|---|---|---|---|---|---|---|
| **Front Office** | `/front-office/arrivals` | Guest Name, Confirmation | Room Type, Cleanliness | Persistent | CSV, XLSX, PDF | Complete |
| **Front Office** | `/front-office/departures` | Guest Name, Room # | Payment Status, Folio Balance | Persistent | CSV, XLSX, PDF | Complete |
| **Front Office** | `/front-office/room-grid` | Room Number | Room Category, Clean Status | Persistent | CSV, XLSX, PDF | Complete |
| **Front Office** | `/front-office/guest-requests` | Guest Name, Room # | Request Status, Priority | Persistent | CSV, XLSX, PDF | Complete |
| **Guests** | `/guests/directory` | Name, Email, Phone | VIP Tier, ID Document Type | Persistent | CSV, XLSX, PDF | Complete |
| **Housekeeping** | `/housekeeping/tasks` | Room #, Attendant | Clean Status, Priority Level | Persistent | CSV, XLSX, PDF | Complete |
| **Housekeeping** | `/housekeeping/linen` | Item Name, SKU | Service Category, Stock State | Persistent | CSV, XLSX, PDF | Complete |
| **Restaurant** | `/restaurant/menu-items` | Dish Name, Item Code | Menu Category, Dietary | Persistent | CSV, XLSX, PDF | Complete |
| **Purchase** | `/purchase/orders` | PO Number, Vendor | Order Status, Approver | Persistent | CSV, XLSX, PDF | Complete |
| **Purchase** | `/purchase/requisitions` | Req #, Department | Urgency, Approval Stage | Persistent | CSV, XLSX, PDF | Complete |
| **Inventory** | `/inventory/items` | Item Name, SKU, Barcode | Item Category, Low Stock Flag | Persistent | CSV, XLSX, PDF | Complete |
| **Finance** | `/finance/invoices` | Invoice #, Guest Name | Status (Paid/Pending/Overdue) | Persistent | CSV, XLSX, PDF | Complete |
| **Finance** | `/finance/general-ledger` | Voucher #, Account Code | Account Classification, Period | Persistent | CSV, XLSX, PDF | Complete |
| **HR** | `/hr/staff-directory` | Employee Name, ID | Department, Employment Type | Persistent | CSV, XLSX, PDF | Complete |
| **Maintenance** | `/maintenance/work-orders` | Asset, Issue Summary | Priority (P1-P4), Status | Persistent | CSV, XLSX, PDF | Complete |
| **Security** | `/security/visitor-log` | Visitor Name, Host | Department Visited, Status | Persistent | CSV, XLSX, PDF | Complete |
| **Security** | `/security/incident-reports` | Incident Title, Location | Severity Level, Status | Persistent | CSV, XLSX, PDF | Complete |
| **IT Admin** | `/it-admin/users` | Username, Full Name | Assigned Role, Account State | Persistent | CSV, XLSX, PDF | Complete |
| **IT Admin** | `/it-admin/document-numbering` | Document Code, Module | Reset Period, Status | Persistent | CSV, XLSX, PDF | Complete |

---

## 4. Verification and Visual Consistency

- **Theme Synchronization:** All toolbar elements, inputs, and popovers dynamically switch between Light Mode (`bg-white border-brand-border text-brand-text`) and Dark Mode (`bg-brand-darkSurface border-brand-darkBorder text-slate-100`).
- **Focus Rings:** Unified subtle ring focus styling (`focus:ring-2 focus:ring-blue-brand/20 dark:focus:ring-blue-brand/30`).
- **Zero Layout Shifts:** Toolbar elements use fixed height tokens preventing jumpy layout shifts during loading or state changes.
