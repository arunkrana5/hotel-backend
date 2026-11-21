# HOTEL OS — GUEST DIRECTORY DESIGN SPECIFICATION

**Module:** Guest Profiles & CRM -> Guest Directory  
**Route:** `/guests/list` (and `/guests/loyalty`)  
**Status:** Implemented & Verified  

---

## 1. Page Header & Actions

- **Page Title:** `Guest Profiles & CRM`
- **Badge:** `Authoritative CRM`
- **Page Actions:**
  - `Add Guest` (`UserPlus` icon, primary button): Opens the multi-section guest registration drawer.
  - `Export Directory` (CSV, Excel, PDF via `DataTable`).

---

## 2. Professional Status Tabs

Tabs load real counts dynamically from the backend `GET /api/v1/pms/guests/stats` endpoint:
- **All Guests** (`ALL`): Full property guest count.
- **Active Profiles** (`ACTIVE`): Verified active guest records.
- **VIP & Loyalty** (`VIP`): Filtered to VIP, Platinum, and Gold tier guests.
- **Inactive / Archived** (`INACTIVE`): Deactivated or archived guest profiles.

---

## 3. One-Row Unified Listing Toolbar

The toolbar stays on a single horizontal row with consistent height (`min-h-[44px]`):
- **Search Field:** `Search guests by name, email, phone, passport...` with debounced search query.
- **VIP Tier Filter:** `UniversalSelect` with options: *All VIP Tiers*, *Standard Guest*, *Silver Tier*, *Gold VIP*, *Platinum VIP*.
- **Column Customization:** Compact `ColumnVisibilityPopover`.
- **Multi-Format Export:** `ExportMenu` with CSV, Excel, and PDF options.
- **Reset Filters:** Resets search, tier filter, and tab to default.

---

## 4. High-Density Guest Table Columns

| Column | Header | Data Display |
|---|---|---|
| **1** | Guest Profile | Initials avatar circle + Full Name + VIP Tier Pill + Guest ID (`GST-00X`) |
| **2** | Contact Details | Email (`Mail` icon) + Phone (`Phone` icon) |
| **3** | Origin & Location | Nationality (`Globe` icon) + City |
| **4** | Identification | Document Type (Passport, National ID) + Document Number |
| **5** | Visits | Completed stay count (`Bed` icon) |
| **6** | Lifetime Spend | Formatted revenue (`$XX,XXX.XX`) |
| **7** | Status | Active (Emerald) / Inactive (Slate) |
| **8** | Actions | View Profile, Edit Profile, Create Booking, Deactivate/Activate, Archive |

---

## 5. Add / Edit Guest Structured Drawer

Divided into 5 logical form sections:
1. **Basic Information:** First Name, Last Name, VIP Classification (`UniversalSelect`).
2. **Contact Information:** Email, Mobile Phone.
3. **Address & Origin:** Nationality, City, Street Address.
4. **Identity & KYC:** ID Document Type (`Passport`, `National ID`, `Driving License`), Document Number.
5. **Guest Preferences & Notes:** Stay preferences, dietary requirements, room location.
- **Real-Time Duplicate Warning:** Debounced query to `/pms/guests/check-duplicate` warns staff immediately if a guest with matching email or phone already exists.
