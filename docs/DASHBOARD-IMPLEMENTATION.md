# HOTEL OS — DASHBOARD IMPLEMENTATION & OPERATIONS COMMAND CENTER

**Module:** Executive & Operations Dashboard  
**Screen Path:** `src/features/dashboard/screens/DashboardScreen.tsx`  
**API Endpoints:** `/reports/dashboard`, `/reports/night-audit`, `/pms/front-office/arrivals`, `/pms/front-office/departures`, `/pms/rooms`, `/system/banners`  

---

## 1. ARCHITECTURE & SECTION OVERVIEW

The HotelOS Dashboard was designed to function as an active **Operations Command Center** rather than a passive analytics screen. It consolidates hospitality yield analytics, real-time room inventories, operational queues, and promotional announcements into a cohesive single-pane interface.

```
+-----------------------------------------------------------------------------------+
| PAGE HEADER: Operations Command Center | Live Indicator | Date Range | Refresh | Customize |
+-----------------------------------------------------------------------------------+
| SECTION A: Luxury Welcome & Announcement Carousel (WelcomeCarousel.tsx)           |
+-----------------------------------------------------------------------------------+
| SECTION B: Tier 1 Executive Yield KPIs (Occupancy Rate, ADR, RevPAR, Invoiced)    |
+-----------------------------------------------------------------------------------+
| SECTION C: Operational Pulse Counters (Arrivals, Departures, In-House, Dirty)    |
+-----------------------------------------------------------------------------------+
| SECTION D: Secondary Inventory Pulse (Available Rooms, OOD, Work Orders, Ledger)  |
+-----------------------------------------------------------------------------------+
| SECTION E: Live Interactive Room Status Matrix (Floor Tabs, Status Filter, Search)|
+-----------------------------------------------------------------------------------+
| SECTION F: Revenue Velocity Area Chart & Department Revenue Share Donut           |
+-----------------------------------------------------------------------------------+
| SECTION G: Operations Center Tabular Roster (Today's Arrivals & Departures)       |
+-----------------------------------------------------------------------------------+
```

---

## 2. KEY CAPABILITIES & SUBSYSTEMS

### 2.1 Luxury Welcome Carousel Engine
* **Source:** Wired to `GET /api/v1/system/banners` (powered by `BannerController.cs`).
* **Interactive Controls:**
  - Auto-play with configurable interval (6.5s) and pause-on-hover.
  - Touch/swipe gestures on mobile and tablet devices.
  - Keyboard arrow navigation (`ArrowLeft` / `ArrowRight`).
  - Active slide indicators and luxury vignette gradients.
  - Direct deep-links to operational modules or special packages.
  - Dedicated "Manage Banners" overlay button for authorized administrators.

### 2.2 Tier 1 Financial & Yield KPIs
* **Occupancy Rate:** Real-time percentage calculated as `(occupiedRooms / totalRooms) * 100`, deep-linked to `/front-office/room-grid`.
* **Average Daily Rate (ADR):** Total room revenue divided by occupied room nights, deep-linked to `/reports/trial-balance`.
* **RevPAR:** Normalized yield per total available room inventory, deep-linked to `/reports/forecast`.
* **Gross Invoiced:** Cumulative daily gross turnover across Outlets, PMS, and Banquets, deep-linked to `/finance/invoices`.

### 2.3 Live Room Status Matrix
* Displays all property rooms categorized by color:
  - **Emerald Green:** Clean / Inspected / Available (`CLN` / `INS`)
  - **Royal Indigo:** Occupied (`OCC`)
  - **Crimson Red:** Dirty / Needs Turndown (`DRT`)
  - **Amber Gold:** Out of Order / Under Maintenance (`MNT` / `OOD`)
* **Filtering Controls:** Search room number or category, filter by specific floor (1, 2, 3...), or filter by operational status.
* **Instant Action Modal:** Clicking any room opens a detail dialog displaying current PMS status, room category, occupancy limits, and keycard RFID pairing, with one-click status transition buttons:
  - `Mark Clean (CLN)`
  - `Mark Inspected (INS)`
  - `Mark Dirty (DRT)`
  - `Out of Order (OOD)`

### 2.4 Revenue Analytics & Department Share
* **Revenue Velocity Area Chart:** Dynamic Recharts area graph tracking 7-day, 14-day, or 30-day revenue trajectory.
* **Department Revenue Share:** Donut chart breakdown illustrating gross share across Front Office, F&B, Banquets, Spa, and Minibar.

### 2.5 Tabular Operations Queues
* Live switches between **Today's Scheduled Arrivals** and **Today's Scheduled Departures**.
* Displays confirmation number, guest profile, VIP status, assigned room, and dates.
* Quick-action button opens guest folio directly (`/front-office/folios?reservationId={id}`).

### 2.6 Dashboard Layout Customizer
* Users can toggle visibility of any dashboard section via the "Customize" button.
* Preferences persist in browser `localStorage` under `hotelos_dashboard_widgets`.
* "Reset Defaults" restores standard executive configuration in one click.
