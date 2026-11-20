# HOTEL OS — ENTERPRISE DESIGN SYSTEM STANDARDS

**System Name:** HotelOS LuxeDesign Tokens & Component Specifications  
**Version:** 2.0.0-PROD  
**Target Environment:** Web & Tablet Responsive Hospitality Applications  

---

## 1. COLOR TOKENS & SURFACE MATRIX

HotelOS enforces a strict dual-mode color token hierarchy designed to evoke luxury hospitality aesthetics while maintaining high information density and eye comfort during long operational shifts.

### 1.1 Brand & Neutral Palettes

```css
/* LIGHT THEME TOKENS */
--brand-primary: #354FC7;          /* Royal Sapphire Blue */
--brand-primary-hover: #293E9E;
--brand-secondary: #4F65D6;
--brand-surface: #FFFFFF;          /* Pure White Container */
--brand-secondarySurface: #F8FAFC; /* Muted Slate Background */
--brand-background: #F1F5FB;       /* Ambient Canvas */
--brand-border: #E2E8F0;          /* Crisp Hairline Border */
--brand-text: #172033;            /* Deep Midnight Charcoal */
--brand-muted: #64748B;           /* Subdued Steel */

/* DARK THEME TOKENS */
--brand-darkSurface: #171F30;      /* Deep Obsidian Container */
--brand-darkSecondary: #1E283E;    /* Slate-Indigo Secondary */
--brand-darkBackground: #111622;   /* Midnight Slate Canvas */
--brand-darkBorder: #303B4F;       /* Subtle Border */
--brand-darkText: #F1F5FB;         /* High-Contrast Off-White */
--brand-darkMuted: #8B9BB4;        /* Soft Periwinkle Muted */
```

### 1.2 Hospitality Status Semantics

| Status Code | Semantics | Light Mode Style | Dark Mode Style |
|---|---|---|---|
| **CLN / INS** | Clean / Inspected / Available | `bg-[#16845B]/10 text-[#16845B] border-[#16845B]/30` | `bg-[#45C79A]/15 text-[#45C79A] border-[#45C79A]/30` |
| **OCC** | Occupied / Checked In | `bg-[#354FC7]/10 text-[#354FC7] border-[#354FC7]/30` | `bg-[#8495FF]/15 text-[#8495FF] border-[#8495FF]/30` |
| **DRT** | Dirty / Turndown Pending | `bg-[#D44848]/10 text-[#D44848] border-[#D44848]/30` | `bg-[#F07878]/15 text-[#F07878] border-[#F07878]/30` |
| **MNT / OOD** | Maintenance / Out of Order | `bg-[#C78B24]/10 text-[#C78B24] border-[#C78B24]/30` | `bg-[#E7B65A]/15 text-[#E7B65A] border-[#E7B65A]/30` |

---

## 2. TYPOGRAPHY & INFORMATION HIERARCHY

* **Primary Font Family:** Inter, system-ui, -apple-system, sans-serif
* **Monospace / Financial Font Family:** JetBrains Mono, Fira Code, monospace

### Scale & Weight Rules:
- **Display 1 (Dashboard KPI Values):** `text-2xl sm:text-3xl font-black tracking-tight`
- **Section Headers:** `text-sm sm:text-base font-bold text-brand-text`
- **Sub-headers / Descriptions:** `text-[11px] sm:text-xs text-brand-muted`
- **Table Data Cells:** `text-xs font-medium`
- **Monetary & Ledger Identifiers:** `font-mono font-bold text-xs`
- **Pill / Badge Micro-labels:** `text-[10px] uppercase font-bold tracking-wider`

---

## 3. ELEVATION & SHADOW SCALE

```css
--shadow-soft-xs: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
--shadow-soft-sm: 0 1px 3px 0 rgba(0, 0, 0, 0.08), 0 1px 2px -1px rgba(0, 0, 0, 0.08);
--shadow-soft-md: 0 4px 6px -1px rgba(0, 0, 0, 0.08), 0 2px 4px -2px rgba(0, 0, 0, 0.08);
--shadow-soft-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1);
--shadow-soft-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1);
```

---

## 4. COMPONENT SPACING & STRUCTURE

1. **Page Container:** `max-w-7xl mx-auto p-4 sm:p-6 space-y-4 sm:space-y-6`
2. **Page Header:** Minimalist single-line height (`h-14` or `h-16`) containing breadcrumb, title, and aligned `PageActions`.
3. **Table Density:**
   - Header Row: `h-10 text-[11px] font-bold uppercase tracking-wider bg-slate-50/80 dark:bg-brand-darkCard`
   - Data Rows: `h-12 text-xs divide-y divide-brand-border dark:divide-brand-darkBorder/60`
   - Cell Padding: `px-3.5 py-2.5`
4. **Form Drawers:** Fixed widths:
   - `sm`: 400px (Quick filters, status updates)
   - `md`: 560px (Standard entities: User, Room, Item)
   - `lg`: 720px (Complex documents: Reservations, Invoices, Work Orders)
   - `xl`: 960px (Ledger batches, Night Audit logs)

---

## 5. BUTTON & INTERACTION STANDARDS

* **Primary Action:** `bg-brand-primary hover:bg-brand-primary-hover text-white shadow-soft-xs font-semibold text-xs h-8 sm:h-9 px-3.5 rounded-lg transition-all`
* **Secondary / Outline Action:** `border border-brand-border dark:border-brand-darkBorder bg-white dark:bg-brand-darkSurface text-brand-text hover:bg-slate-50 dark:hover:bg-brand-darkCard text-xs h-8 sm:h-9 px-3 rounded-lg`
* **Destructive Action:** `bg-rose-600 hover:bg-rose-700 text-white shadow-soft-xs font-semibold text-xs h-8 sm:h-9 px-3.5 rounded-lg`
* **Ghost / Icon Button:** `text-brand-muted hover:text-brand-text hover:bg-slate-100 dark:hover:bg-brand-darkCard p-1.5 rounded-lg transition-colors`

---

## 6. FORM & INPUT CONVENTIONS

- Inputs must always feature a visible `<label>` with font size `text-xs font-bold text-brand-text`.
- Mandatory fields must feature an indicator `text-rose-500 *`.
- Border on focus: `focus:ring-2 focus:ring-brand-primary/20 focus:border-brand-primary`.
- Validation errors must render directly below the field in `text-[11px] text-rose-500 font-medium`.
