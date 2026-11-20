# HOTEL OS — UI/UX COMPONENT STANDARDS & DESIGN SYSTEM SPECIFICATION

## 1. Design Direction & Visual Philosophy

HotelOS is designed to match the refined aesthetic and operational precision of world-class platforms like **Linear, Stripe, Ramp, and modern SAP Fiori**. It combines high information density with exceptional visual clarity, eliminating arbitrary spacing, cramped controls, redundant toolbars, and unreadable theme states.

---

## 2. Global Semantic Design Tokens

### Light Theme
- **Application Background:** `#F4F6FA` (Soft neutral canvas)
- **Main Surface / Card:** `#FFFFFF` (Pure white)
- **Secondary Surface:** `#F8FAFD` (Subtle off-white for headers and sidebars)
- **Primary Brand:** `#354FC7` (Deep cobalt enterprise blue)
- **Primary Hover:** `#293FA8`
- **Main Text:** `#172033` (High-contrast slate black)
- **Secondary / Muted Text:** `#64748B` (Refined slate)
- **Borders & Dividers:** `#E2E8F0`
- **Semantic Success:** `#16845B` (Forest green)
- **Semantic Warning:** `#C78B24` (Amber gold)
- **Semantic Danger:** `#D44848` (Crimson red)
- **Semantic Info:** `#2876C7` (Sky blue)

### Dark Theme
- **Application Background:** `#101522` (Deep midnight navy)
- **Main Surface / Card:** `#171F30` (Rich dark slate)
- **Secondary Surface:** `#1D283A`
- **Primary Brand:** `#8495FF` (Vibrant periwinkle)
- **Primary Hover:** `#9AA8FF`
- **Main Text:** `#F1F5FB` (Crisp off-white)
- **Secondary / Muted Text:** `#A0AEC0`
- **Borders & Dividers:** `#303B4F`
- **Semantic Success:** `#45C79A`
- **Semantic Warning:** `#E7B65A`
- **Semantic Danger:** `#F07878`
- **Semantic Info:** `#70B5F5`

---

## 3. Typography Standards

| Hierarchy | Size | Weight | Line Height | Usage |
|---|---|---|---|---|
| **Page Title** | 18–20px (`text-lg`) | Bold (700) | 1.25 | Universal page header on left |
| **Section Header** | 13–14px (`text-sm`) | Semi-Bold (600) | 1.3 | Drawer headers, table groupings |
| **Body / Table Cell** | 12–13px (`text-xs`) | Medium (500) | 1.4 | Data table cells, form labels |
| **Supporting / Muted** | 11px (`text-[11px]`) | Normal (400) | 1.3 | Timestamp, sub-labels, hints |
| **Badge / Tag** | 10–11px (`text-[10px]`) | Bold (700) | 1.0 | Status pills, count badges |
| **Currency / Financial** | 13–14px tabular | Semi-Bold (600) | 1.2 | Ledger balances, voucher amounts |

---

## 4. Universal Component Specifications

### A. Compact Page Header
- Page title strictly on the left.
- Single-row action buttons strictly on the right (Primary, Secondary, Export).
- No breadcrumbs, subtitle banners, or duplicate search inputs.
- Minimum height: 44px–48px.

### B. Segmented Page Tabs (`PageTabs`)
- Horizontal tab list with status/category pills.
- Pill counter badges showing live counts (`All (148)`, `Active (120)`, `Pending (12)`).
- **Active state:** Solid brand color (`bg-brand-primary text-white shadow-soft-xs`) or explicit tinted background with contrasting border.
- **Never render white text on a light background.** Inactive tabs remain readable in muted slate.
- Triggers server-side filter on change without full page reloads.

### C. Unified Single-Row Filter Toolbar (`ListToolbar` / `FilterBar`)
- **Structure:** Arranged strictly in ONE horizontal bar:
  1. Compact search box with search icon and clear (X) button.
  2. Dropdown filters with subtle borders (Property, Status, Category).
  3. Date range picker (quick range or custom).
  4. Reset filters button (returns to default state).
  5. Advanced filter drawer trigger (for complex criteria).

### D. Consistent CRUD Actions (`RowActions`)
- **View Details:** Direct eye icon button opens the record's detail drawer.
- **Edit:** Direct pencil icon button opens the edit form drawer.
- **More Actions:** Three-dots dropdown containing contextual operations:
  - Activate / Deactivate (with immediate status toggle)
  - Delete (with confirmation dialog)
  - Duplicate / Print / Custom module actions.

### E. Financial Ledger Architecture (`LedgerSummary` & `TransactionTable`)
- **Summary KPI Cards:**
  - Opening Balance
  - Total Debits (Period)
  - Total Credits (Period)
  - Closing Balance
  - Net Movement
- **Double-Entry Table Rules:**
  - Date, Voucher Number, Particulars / Account, Reference, Debit, Credit, Running Balance.
  - Numbers right-aligned with standard decimal places (`$ 1,250.00`).
  - Running balance calculated authoritatively.
  - Clicking any voucher number opens the full voucher transaction drilldown.

### F. Form Drawer & Dynamic Fields (`FormDrawer` & `FormSection`)
- Slide-over right drawer (380px–540px width).
- Sticky header with title and close button.
- Scrollable form body with two-column responsive layout.
- Sticky footer with Cancel, Save, and Save & Add Another buttons.
- First field in transaction forms is always the read-only backend document sequence number.
