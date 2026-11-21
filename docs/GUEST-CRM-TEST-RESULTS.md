# HOTEL OS — GUEST PROFILES & CRM E2E TEST RESULTS

**Test Suite:** `e2e_crm_test.py`  
**Execution Timestamp:** November 21, 2026 09:18:25 UTC  
**Environment:** Live Backend (.NET 10, Port 5000), PostgreSQL 19, Frontend (Port 3000)  

---

## 1. Test Execution Summary

| Test Step | Description | Expected Result | Actual Result | Status |
|---|---|---|---|---|
| **Step 1** | Operator Authentication | Token returned with HTTP 200 | Token received for `admin` | **PASSED** |
| **Step 2** | Guest Directory & CRM Stats | Active counts and totals returned | `totalCount: 4, activeCount: 4, vipCount: 2` | **PASSED** |
| **Step 3** | Duplicate Detection | Flag true for existing, false for new | Positive match on `sarah.jenkins`, negative for unique | **PASSED** |
| **Step 4** | Create Guest Profile | Created with ID and DTO returned | Created ID #5 (`Lord Julian Vane`) | **PASSED** |
| **Step 5** | Complete CRM Profile Detail | Full aggregates, tabs data returned | Profile loaded with 0 upcoming, 1 audit event | **PASSED** |
| **Step 6** | Update Guest Profile | Data updated and returned | Updated preferences with Champagne note | **PASSED** |
| **Step 7** | Toggle Active/Inactive Status | Status updated to false, then back to true | Deactivated and re-activated with HTTP 200 | **PASSED** |
| **Step 8** | Safe Soft-Delete / Archive | Profile soft-deleted | Archived successfully | **PASSED** |

**Overall Result:** 8 of 8 Functional Tests Passed (100% Success Rate).

---

## 2. Frontend Production Verification
- **Command:** `npm run build`
- **Modules Transformed:** 2,480 modules.
- **TypeScript Errors:** 0 errors.
- **Build Duration:** 27.71 seconds.
- **Assets Generated:**
  - `dist/index.html` (1.58 kB)
  - `dist/assets/index-CU0iDHrR.css` (100.21 kB)
  - `dist/assets/index-Dvmk27qw.js` (1,660.56 kB)
