# HOTEL OS — GUEST PROFILES & CRM ARCHITECTURE AUDIT

**Date:** November 21, 2026  
**Auditor:** Principal Enterprise SaaS & Hospitality CRM Architect  
**Scope:** Existing guest entities, APIs, routes, and business rules  

---

## 1. Existing Infrastructure Findings

### Database Entities
- **`Guest` Entity (`HotelOS.Domain.Pms.Entities.Guest`)**:
  - `Id`, `PropertyId`, `FirstName`, `LastName`, `Email`, `Phone`
  - `IdDocumentType`, `IdDocumentNumber`, `Nationality`
  - `VipStatus` (`Standard`, `Silver`, `Gold`, `Platinum`)
  - `TotalStays`, `LifetimeSpend`, `SpecialPreferences`, `Address`, `City`, `Country`
  - Navigation collections: `Reservations` (1:N), `Folios` (1:N)
  - Inherits from `BaseEntity` (`IsActive`, `IsDeleted`, `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`, `DeletedAt`, `DeletedBy`).

- **`Reservation` Entity (`HotelOS.Domain.Pms.Entities.Reservation`)**:
  - Foreign key `GuestId` -> `Guest`
  - `ConfirmationNumber`, `RoomTypeCode`, `AssignedRoomId`, `CheckInDate`, `CheckOutDate`
  - `AdultsCount`, `ChildrenCount`, `ReservationStatus`, `RatePerNight`, `TotalEstimatedAmount`, `PaidAmount`, `SpecialRequests`

- **`Folio` Entity (`HotelOS.Domain.Pms.Entities.Folio`)**:
  - Foreign key `GuestId` -> `Guest`
  - `FolioNumber`, `Status`, `TotalCharges`, `TotalPayments`, `BalanceAmount`
  - Navigation collection: `Items` (`FolioItem`)

### API Limitations (Prior to Redesign)
- Only basic listing (`GET /api/v1/pms/guests`), single lookup by ID (`GET /api/v1/pms/guests/{id}`), and create (`POST /api/v1/pms/guests`) were implemented.
- **Missing Capabilities:**
  - Update guest profile (`PUT /api/v1/pms/guests/{id}`)
  - Status toggle (`PATCH /api/v1/pms/guests/{id}/status`)
  - Safe soft-delete (`DELETE /api/v1/pms/guests/{id}`)
  - Complete 360-degree CRM profile with aggregates (`GET /api/v1/pms/guests/{id}/profile`)
  - CRM tab stats (`GET /api/v1/pms/guests/stats`)
  - Real-time duplicate detection (`GET /api/v1/pms/guests/check-duplicate`)

### Frontend Limitations (Prior to Redesign)
- Single screen `GuestDirectoryScreen.tsx` without navigation to individual guest profiles.
- Clicking a guest opened a basic summary drawer instead of an authoritative CRM profile.
- Missing active/inactive tab counts and duplicate prevention alerts.

---

## 2. Information Architecture Realignment

```
Guest Profiles & CRM (/guests)
 ├── Guest Directory (/guests/list)
 │    ├── Filter Tabs: [All Guests] [Active Profiles] [VIP & Loyalty] [Inactive / Archived]
 │    ├── One-Row Toolbar: [Search] [VIP Tier] [Columns] [Export]
 │    ├── Add/Edit Guest Drawer (6 Structured Sections + Real-time Duplicate Detection)
 │    └── Actions: [View Profile] [Edit Profile] [Create Booking] [Toggle Status] [Archive]
 └── Complete Guest CRM Profile (/guests/:id)
      ├── Executive Header (Avatar, VIP Badge, Status, Contact Pills, Actions)
      ├── Key Metrics Summary Cards (Total Stays, Upcoming, Revenue, Outstanding, Avg Duration, Last Stay)
      └── 8 CRM Tabs:
           ├── 1. 360° Overview (Identity, Address, KYC, Stay Preferences, Next Stay)
           ├── 2. Reservations (Linked active/upcoming bookings)
           ├── 3. Stay History (Chronological completed visits timeline)
           ├── 4. Folios & Billing (Financial statements, charges, payments, line items)
           ├── 5. Preferences (Room, F&B, bedding, communications)
           ├── 6. Identity & KYC (Government document verification status)
           ├── 7. Feedback & Notes (Sentiment ratings, operational notes)
           └── 8. Activity & Audit (Chronological change log with actors and timestamps)
```
