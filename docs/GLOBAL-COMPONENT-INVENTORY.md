# HOTEL OS — GLOBAL COMPONENT INVENTORY & SPECIFICATIONS

**Package Scope:** `@/components/enterprise/*`, `@/components/common/*`, `@/components/layout/*`  
**Application:** HotelOS Enterprise  
**Status:** 100% Implemented & Validated  

---

## 1. ENTERPRISE REUSABLE COMPONENTS

| Component | Path | Purpose | Key Props & Features |
|---|---|---|---|
| **WelcomeCarousel** | `src/components/enterprise/WelcomeCarousel.tsx` | Luxury hospitality promotional & operations announcement banner | `slides: BannerSlide[]`, `autoPlayInterval: number`, swipe touch controls, pause-on-hover, dot & arrow navigation |
| **PageTabs** | `src/components/enterprise/PageTabs.tsx` | Standardized top-level module filtering | `tabs: TabItem[]`, `activeTab: string`, `onChange: (id) => void`, count pills |
| **StatusBadge** | `src/components/enterprise/StatusBadge.tsx` | Consistent status visualization | `status: string`, semantic colors (Clean, Dirty, Confirmed, Cancelled, Draft, Active) |
| **ListToolbar** | `src/components/enterprise/ListToolbar.tsx` | Search, filter dropdowns, and batch operations bar | `searchQuery`, `onSearchChange`, `filters: DropdownFilter[]`, `onResetFilters`, `onExport` |
| **RowActions** | `src/components/enterprise/RowActions.tsx` | Unified action dropdown for all data tables | `onView`, `onEdit`, `onDelete`, `onToggleActive`, `customActions: ActionItem[]` |
| **ConfirmDialog** | `src/components/enterprise/ConfirmDialog.tsx` | Global confirmation dialog for sensitive actions | 10 variants (`delete`, `deactivate`, `activate`, `cancel`, `reverse`, `logout`, etc.), reason capture, keyword match |
| **FormDrawer** | `src/components/enterprise/FormDrawer.tsx` | Side slide-over for creating and editing records | `isOpen`, `onClose`, `onSubmit`, `title`, `subtitle`, `size: sm|md|lg|xl`, keyboard Escape handling |
| **DetailDrawer** | `src/components/enterprise/DetailDrawer.tsx` | Read-only sliding inspection panel | `isOpen`, `onClose`, `title`, `tabs: DetailTab[]`, action toolbar, metadata timestamps |
| **LedgerSummary** | `src/components/enterprise/LedgerSummary.tsx` | Financial debit/credit balance header | `totalDebit: number`, `totalCredit: number`, `variance: number`, status indicators |
| **TransactionTable**| `src/components/enterprise/TransactionTable.tsx` | High-density financial posting grid | Double-entry journal rows, account mapping, Dr/Cr formatting |
| **FinancialSummary**| `src/components/enterprise/FinancialSummary.tsx` | Multi-currency outlet and tax totals | Subtotal, discounts, service charge, CGST/SGST/VAT, net payable |

---

## 2. COMMON CORE COMPONENTS

| Component | Path | Description |
|---|---|---|
| **Button** | `src/components/common/Button.tsx` | Accessible button with loading spinner, variants (`primary`, `secondary`, `outline`, `ghost`, `danger`), sizes (`xs`, `sm`, `md`, `lg`) |
| **Card / StatCard** | `src/components/common/Card.tsx` | Surface cards with responsive padding, icon container, and trendline indicators |
| **Modal** | `src/components/common/Modal.tsx` | Accessible dialog with overlay, keyboard trap, and customizable footer |
| **Badge** | `src/components/common/Badge.tsx` | Text pill badge with dot indicator and theme support |
| **DataTable** | `src/components/common/DataTable.tsx` | TanStack Table wrapper with column sorting, pagination, and empty state handling |

---

## 3. LAYOUT & SYSTEM CONTROLS

| Component | Path | Description |
|---|---|---|
| **PageContainer** | `src/components/layout/PageContainer.tsx` | Central responsive bounds wrapper (`max-w-7xl mx-auto p-4 sm:p-6`) |
| **PageHeader** | `src/components/layout/PageHeader.tsx` | Universal minimalist title header with live operational timestamps |
| **PageToolbar** | `src/components/layout/PageToolbar.tsx` | Action container aligning primary CTAs, filters, and export buttons |
| **Sidebar** | `src/components/layout/Sidebar.tsx` | Collapsible navigation rail with hover-flyout submenus, search, and user profile drawer |
| **Header** | `src/components/layout/Header.tsx` | App top bar with search trigger, notification bell, theme toggle, and branch switcher |

---

## 4. GLOBAL SERVICE APIS & CONTEXTS

| Service / Hook | Path | Purpose |
|---|---|---|
| **notify** | `src/services/notify.ts` | Centralized toast notification engine wrapping Sonner with deduplication & sanitization |
| **useConfirm** | `src/context/ConfirmContext.tsx` | Hook triggering global confirmation dialogs with async promise resolution |
| **useAuth** | `src/features/authentication/context/AuthContext.tsx` | Session profile, token lifecycle, and role-based permissions (`hasRole`, `hasPermission`) |
| **useTheme** | `src/hooks/useTheme.ts` | Reactive system, light, and dark theme synchronization |
| **signalRService** | `src/services/signalr.ts` | Real-time WebSocket subscriptions for notifications and live PMS events |
