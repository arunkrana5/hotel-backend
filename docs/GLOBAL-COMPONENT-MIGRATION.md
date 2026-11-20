# HOTEL OS — GLOBAL COMPONENT MIGRATION & DESIGN SYSTEM STANDARDS

**Architecture Version:** 2.5 Enterprise  
**Design Standard:** Coherent SaaS ERP/PMS Component Framework  
**Scope:** Universal Components, Modals, Drawers, Toolbars, Dropdowns & DataTables  
**Status:** Completed & Globally Integrated

---

## 1. Executive Summary & Transformation Goals

HotelOS 2.5 establishes a unified component design language across all modules. Prior to this release, disparate screens used varying button heights, misaligned search bars, native select elements, uncoordinated modals, and scattered action buttons.

This migration delivers:
1. **Universal Dropdown Standard:** A single searchable, portal-mounted `UniversalSelect` replacing all native `<select>` tags and disparate autocompletes.
2. **Unified List Toolbar:** A 36px one-row listing toolbar integrating search, quick filters, column customization, multi-format export, and secondary drawers.
3. **Structured Detail & Form Drawers:** Standardized `DetailDrawer` (viewing) and `FormDrawer` (editing) sliding from the right edge with uniform action footers.
4. **Stacked Modal System:** Z-index tiered modals and confirmation dialogs with guaranteed toast visibility (`z-index: 9999`).
5. **Universal Button & Input System:** Consistent sizing tokens (`xs`, `sm`, `md`, `lg`), focus rings, dark/light theme support, and ref forwarding.

---

## 2. Reusable Component Catalog

| Component | File Path | Primary Function | Z-Index |
|---|---|---|---|
| `UniversalSelect` | `@/components/common/UniversalSelect` | Unified dropdown for Mode A (DB 5-batch) & Mode B (Static) | 2000 |
| `UniversalMultiSelect` | `@/components/common/UniversalSelect` | Tag-based multi-selection with individual removal chips | 2000 |
| `UniversalDependentSelect` | `@/components/common/UniversalSelect` | Cascading dropdown linking parent & child entities | 2000 |
| `OverlayPortal` | `@/components/common/OverlayPortal` | Top-level DOM portal avoiding container clipping | 2000 |
| `ListToolbar` | `@/components/enterprise/ListToolbar` | One-row desktop listing toolbar (34–38px) | 30 |
| `ColumnVisibilityPopover` | `@/components/enterprise/ColumnVisibilityPopover` | Compact column toggle popover with localStorage memory | 2000 |
| `ExportMenu` | `@/components/enterprise/ExportMenu` | Multi-format export trigger (CSV, Excel, PDF) | 2000 |
| `AdvancedFilterDrawer` | `@/components/enterprise/AdvancedFilterDrawer` | Secondary slide-over filter drawer | 1050 |
| `Modal` | `@/components/common/Modal` | Centered dialog container with backdrop | 1050 |
| `ConfirmDialog` | `@/components/enterprise/ConfirmDialog` | High-priority confirmation alert dialog | 1150 |
| `FormDrawer` | `@/components/enterprise/FormDrawer` | Slide-over drawer for entity creation/editing | 1050 |
| `DetailDrawer` | `@/components/enterprise/DetailDrawer` | Slide-over drawer for entity inspection | 1050 |
| `DataTable` | `@/components/data-table/DataTable` | TanStack-powered table with sorting, pagination & rows selector | Base |
| `Button` | `@/components/common/Button` | ForwardRef button with variants (`primary`, `outline`, `danger`, etc.) | Base |
| `Input` / `Textarea` | `@/components/common/Input` | Floating label or top label input with error states | Base |

---

## 3. Module-by-Module Migration Matrix

### 3.1 Front Office & PMS
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `ArrivalsScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`) | Complete |
| `DeparturesScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`) | Complete |
| `RoomGridScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |
| `FolioManagementScreen.tsx` | Yes (`ListToolbar`) | Yes (`DynamicDropdown` -> `UniversalSelect`) | Yes (`Modal`) | Complete |
| `GuestRequestsScreen.tsx` | Yes (`ListToolbar`) | Yes (`Select` -> `UniversalSelect`) | Yes (`FormDrawer`) | Complete |

### 3.2 Guest Profiles & CRM
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `GuestDirectoryScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`, `FormDrawer`) | Complete |

### 3.3 Housekeeping & Facilities
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `HousekeepingTasksScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`) | Complete |
| `LinenLaundryScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |
| `RoomStatusBoardScreen.tsx` | Yes (`ListToolbar`) | Yes (`Select` -> `UniversalSelect`) | Yes (`Modal`) | Complete |

### 3.4 Food & Beverage / Restaurant
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `MenuItemsScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`) | Complete |
| `TableLayoutScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |
| `KitchenOrdersScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`) | Complete |

### 3.5 Banquets & Events
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `BanquetEventsScreen.tsx` | Yes (`ListToolbar`) | Yes (`Select` -> `UniversalSelect`) | Yes (`FormDrawer`) | Complete |
| `VenueCatalogScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |

### 3.6 Purchase & Procurement
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `PurchaseOrdersScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`, `FormDrawer`) | Complete |
| `PurchaseRequisitionsScreen.tsx`| Yes (`ListToolbar`) | Yes (`DynamicDropdown` / `Select` -> `UniversalSelect`) | Yes (`FormDrawer`) | Complete |
| `VendorDirectoryScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`) | Complete |

### 3.7 Inventory & Stores
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `InventoryItemsScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`, `Modal`) | Complete |
| `StockLedgerScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`) | Complete |

### 3.8 Finance & General Ledger
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `InvoicesScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`DetailDrawer`) | Complete |
| `GeneralLedgerScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`) | Complete |
| `ChartOfAccountsScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |

### 3.9 Human Resources
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `StaffDirectoryScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`, `DetailDrawer`) | Complete |
| `AttendanceScreen.tsx` | Yes (`ListToolbar`) | Yes (`Select` -> `UniversalSelect`) | Yes (`Modal`) | Complete |

### 3.10 Engineering & Maintenance
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `WorkOrdersScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`, `DetailDrawer`) | Complete |
| `AssetRegisterScreen.tsx` | Yes (`ListToolbar`) | Yes (`Select` -> `UniversalSelect`) | Yes (`Modal`) | Complete |

### 3.11 Security & Risk
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `VisitorLogScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |
| `IncidentReportsScreen.tsx` | Yes (`ListToolbar`) | Yes (`Select` -> `UniversalSelect`) | Yes (`FormDrawer`) | Complete |

### 3.12 IT Administration & Configuration
| Screen | Toolbar Migrated | Dropdowns Migrated | Drawers / Modals Migrated | Status |
|---|---|---|---|---|
| `UserManagementScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |
| `DocumentNumberingScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`Modal`) | Complete |
| `MasterManagementScreen.tsx` | Yes (`ListToolbar`) | Yes (`DynamicDropdown` -> `UniversalSelect`) | Yes (`Modal`) | Complete |
| `DynamicFormsScreen.tsx` | Yes (`ListToolbar`) | Yes (`UniversalSelect`) | Yes (`FormDrawer`) | Complete |

---

## 4. Design System Before/After Comparison

| Dimension | Before Migration | After Migration |
|---|---|---|
| **Filter Row Height** | 120px–180px across 2–3 rows | Strictly 36px in 1 horizontal row |
| **Dropdown Appearance** | Inconsistent HTML selects & custom autocompletes | Unified `UniversalSelect` with search, icons & portal |
| **Dropdown Batch Size** | Unbounded (all records) or missing pagination | Strictly 5 records initial batch + server pagination |
| **Overlay Clipping** | Popovers clipped by parent modals and tables | Rendered into `OverlayPortal` (`z-index: 2000`) |
| **Toast Notifications** | Hidden behind open modals and dark backdrops | Elevated to `z-index: 9999`, always visible |
| **Column Customization** | Missing or non-persistent | Stored per-module in `localStorage` |
| **Export Formats** | Raw CSV or hardcoded export | Unified CSV, XLSX, and PDF under single `ExportMenu` |
| **Theme Cohesion** | Active tab white-out and dark mode borders | Fully synchronized light & dark theme tokens |
