# HOTEL OS — NOTIFICATION & CONFIRMATION SYSTEMS IMPLEMENTATION

**Core Modules:**
1. Global Toast Notification Service: `src/services/notify.ts`
2. Global Confirmation Dialog Provider: `src/context/ConfirmContext.tsx`
3. SignalR Real-Time Bridge: `src/services/signalr.ts`

---

## 1. GLOBAL TOAST NOTIFICATION SERVICE (`notify.ts`)

HotelOS eliminated arbitrary inline alerts, disparate console messages, and window `alert()` dialogs in favor of an enterprise-grade toast notification service built atop `sonner`.

### 1.1 API Specification

```typescript
import { notify } from '@/services/notify';

// Standard operational feedback
notify.success('Reservation #RES-2026-089 checked in successfully.');
notify.info('Night audit process has commenced.');
notify.warning('Room 304 marked Out of Order due to plumbing issue.');
notify.error('Failed to post payment to folio #FOL-4091.');

// Specialized systemic alerts
notify.networkError();            // Automatic offline / network error handling
notify.sessionExpired();          // Prompts re-authentication
notify.permissionDenied();        // Explains authorization restriction

// Async Promise Handling
await notify.promise(api.post('/pms/reservations', data), {
  loading: 'Generating confirmation and allocating room...',
  success: 'Reservation confirmed!',
  error: (err) => `Failed to create reservation: ${err.message}`,
});
```

### 1.2 Deduplication Architecture
To prevent toast spam when users double-click buttons or when network retry loops fire, `notify.ts` maintains an in-memory cache of recent message fingerprints. If an identical message is emitted within 1500ms, the subsequent call is debounced automatically.

### 1.3 Technical Exception Sanitization
Raw database errors (such as Npgsql unique constraint violations or internal connection timeouts) are intercepted by `sanitizeErrorMessage()`:
- Technical keywords like `23505: duplicate key value`, `Npgsql.PostgresException`, or `column not found` are translated into user-friendly guidance:
  *"A record with this identifier or code already exists in the system."*
- Stack traces are logged to client diagnostics but never leaked onto the visual UI.

---

## 2. GLOBAL CONFIRMATION DIALOG SYSTEM (`ConfirmContext.tsx`)

Destructive and irreversible operations in hospitality management (cancelling reservations, reversing journal vouchers, deactivating employees, or purging master records) require deliberate, auditable confirmation.

### 2.1 The `useConfirm()` Hook

```typescript
import { useConfirm } from '@/context/ConfirmContext';

const confirm = useConfirm();

const handleDeleteRecord = async (id: number) => {
  const confirmed = await confirm({
    title: 'Delete Master Rate Plan?',
    message: 'This will permanently remove the corporate seasonal tariff. Active reservations will preserve their locked rates.',
    variant: 'delete',
    confirmText: 'Permanently Delete',
    requireReason: true,            // Requires user to type an operational reason
    requireExactMatch: 'DELETE',    // Requires user to explicitly type 'DELETE'
  });

  if (confirmed) {
    await api.delete(`/masters/tariffs/${id}`);
    notify.success('Rate plan deleted successfully.');
  }
};
```

### 2.2 Supported Variants & Semantics

| Variant | Accent Color | Intended Use Case |
|---|---|---|
| `delete` | Crimson Red (`#DC2626`) | Permanent deletion of entities (Users, Rooms, Vendors) |
| `irreversible` | Dark Crimson (`#991B1B`) | Financial ledger purge, database truncate, master reset |
| `deactivate` | Amber (`#D97706`) | Suspending employee access, archiving item SKUs |
| `activate` | Emerald Green (`#059669`) | Re-enabling accounts, publishing draft menus |
| `cancel` | Slate Gray (`#4B5563`) | Cancelling pending reservations or purchase orders |
| `reject` | Rose (`#E11D48`) | Rejecting approval workflow requests or leave requests |
| `reverse` | Indigo (`#4F46E5`) | Reversing journal voucher transactions in GL |
| `logout` | Blue (`#2563EB`) | Terminating user session and clearing refresh tokens |
| `unsaved_changes`| Amber (`#D97706`) | Navigating away from uncommitted form drafts |
| `default` | Primary (`#354FC7`) | Standard administrative verifications |

---

## 3. SIGNALR REAL-TIME NOTIFICATION BROADCASTS

HotelOS integrates Microsoft SignalR WebSockets via `signalr.ts`:
- **Live Notifications:** Staff members receive real-time push alerts for VIP arrivals, emergency housekeeping tickets, and maintenance work orders.
- **Auto-Invalidation:** Incoming WebSocket events automatically trigger TanStack Query cache invalidation, ensuring that dashboards, room matrix tiles, and operational queues update without requiring a manual page refresh.
