# HOTEL OS — GUEST PROFILES & CRM API & DATABASE CHANGES

**Date:** November 21, 2026  
**Backend:** .NET 10 Web API, Entity Framework Core, PostgreSQL 19  

---

## 1. Contracts & DTOs Added (`HotelOS.Contracts.Pms`)

- **`GuestDto`:** Added `bool IsActive = true, string? Address = null` to support status and residence.
- **`UpdateGuestRequest`:** Full profile payload for updates.
- **`SetGuestStatusRequest`:** `bool IsActive` payload for status toggling.
- **`GuestCrmProfileDto`:** Comprehensive CRM profile DTO containing guest details, summary metrics, linked reservations, stay history, folios with line items, and audit trail.
- **`GuestCrmSummaryDto`:** Aggregated counts, total revenue, outstanding balance, last visit, and average duration.
- **`GuestStayHistoryDto`:** Detailed stay timeline model.
- **`GuestFolioSummaryDto` & `GuestFolioItemDto`:** Financial folios and itemized transactions.
- **`GuestAuditEventDto`:** System event audit log entry.
- **`GuestCrmStatsDto`:** Tab counts (`totalCount`, `activeCount`, `vipCount`, `inactiveCount`) and aggregate spend.
- **`DuplicateCheckResult`:** `bool IsDuplicate, GuestDto? MatchedGuest`.

---

## 2. API Endpoints (`GuestController.cs`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/pms/guests` | Paged guest directory with `search`, `vipStatus`, and `isActive` filters |
| `GET` | `/api/v1/pms/guests/stats` | Real-time counts for directory tabs and spend aggregates |
| `GET` | `/api/v1/pms/guests/check-duplicate` | Real-time email and phone duplicate detection |
| `GET` | `/api/v1/pms/guests/{id}` | Single guest profile lookup |
| `GET` | `/api/v1/pms/guests/{id}/profile` | Complete 360-degree CRM profile with linked data |
| `POST` | `/api/v1/pms/guests` | Create new guest profile |
| `PUT` | `/api/v1/pms/guests/{id}` | Update existing guest profile |
| `PATCH` | `/api/v1/pms/guests/{id}/status` | Activate or deactivate guest profile |
| `DELETE` | `/api/v1/pms/guests/{id}` | Safe soft-delete (with active booking & folio balance checks) |

---

## 3. Business Logic & Security Safeguards (`PmsService.cs`)

1. **Deletion Guardrails:**
   - Rejects deletion if the guest has active/upcoming reservations (`CONF`, `PROV`, `CHKI`).
   - Rejects deletion if the guest has unsettled folio balances (`BalanceAmount != 0`).
   - Applies soft-deletion (`IsDeleted = true`, `DeletedAt = UtcNow`, `DeletedBy = user`) when validated.
2. **Normalized Duplicate Matching:**
   - Trims and normalizes emails to lowercase.
   - Compares phone strings.
   - Excludes the current editing guest ID to permit in-place updates.
3. **Tenant & Property Scoping:**
   - All queries filter strictly by `PropertyId == propertyId && !g.IsDeleted`.
