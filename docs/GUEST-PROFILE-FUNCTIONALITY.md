# HOTEL OS — GUEST PROFILE FUNCTIONALITY & CRM INTELLIGENCE

**Module:** Guest Profiles & CRM -> Complete Guest Profile  
**Route:** `/guests/:id` (and `/guests/profile/:id`)  
**Status:** Implemented & Verified  

---

## 1. Executive Profile Header

- **Avatar & Initials:** 64px rounded square with gradient gold rim and initials.
- **Identity & Tier:** Full Name, VIP status badge (`Platinum VIP`, `Gold VIP`, `Silver`, `Standard`), and Active status badge.
- **Contact Bar:** Direct email, phone, and nationality badges.
- **Header Actions:**
  - `Edit Profile`: Opens drawer to update contact, address, KYC, and preferences.
  - `Create Reservation`: Pre-fills guest ID and navigates to `/reservations/new?guestId=:id`.
  - `Activate / Deactivate`: Safe toggle with confirmation dialog.
  - `Back to Directory`: Returns directly to `/guests/list`.

---

## 2. Real-Time Summary Metrics (CRM KPI Bar)

Computed directly from backend database relations:
1. **Total Stays:** Completed visits.
2. **Upcoming:** Confirmed or provisional reservations starting today or later.
3. **Total Revenue:** Aggregated folio charges and confirmed reservation payments.
4. **Outstanding:** Unsettled folio balances.
5. **Avg Duration:** Average stay length in nights.
6. **Last Stay:** Date of most recent completed stay.

---

## 3. CRM Detail Tabs

### Tab 1: 360° Overview
- Full authoritative contact details, legal name, residential address, KYC government document ID.
- Special stay preferences and dietary requirements.
- Upcoming stay preview with room type, check-in/out dates, and status.
- VIP Concierge Protocol badge.

### Tab 2: Reservations
- Table listing all linked reservations with confirmation number, room type, assigned room, check-in, check-out, rate per night, total estimated amount, status, and booking source.

### Tab 3: Stay History
- Chronological timeline of all completed stays showing room number, nights stayed, total spend, and stay status.

### Tab 4: Folios & Billing
- Detailed list of guest folios with status, total charges, total payments, and outstanding balance.
- Expandable line items displaying item description, department code (`FO`, `FB`, `SPA`), amount, transaction reference, and posted timestamp.

### Tab 5: Preferences
- Segmented preference cards covering room location, bedding type, food & beverage preferences, and communication channels.

### Tab 6: Identity & KYC
- Document verification status (`Verified • On File`), document type, document number, and issuing country.

### Tab 7: Feedback & Notes
- Satisfaction ratings and sentiment history with operational guest notes.

### Tab 8: Activity & Audit
- Immutable chronological audit trail recording profile creation, data updates, and linked reservations with actor names and timestamps.
