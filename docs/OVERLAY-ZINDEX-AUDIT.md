# HOTEL OS — GLOBAL OVERLAY STACKING & Z-INDEX AUDIT REPORT

**Architecture Version:** 2.5 Enterprise  
**Design Standard:** Strict Multi-Layer Portal Stacking Hierarchy  
**Token Standard:** `Z_INDEX` (`@/constants/zIndex`)  
**Portal Standard:** `OverlayPortal` (`@/components/common/OverlayPortal`)  
**Status:** Verified & Fully Resolved

---

## 1. Problem Statement & Root Cause Analysis

### The Critical Bug
In previous builds of HotelOS, users performing operations inside open modals or drawers (e.g., submitting a "Create Reservation" dialog or saving a "Check-in Payment") reported that success/error toast notifications appeared **behind** the modal backdrop or dialogue window. This caused:
1. The user was unaware whether the action succeeded or failed.
2. The user repeatedly clicked the submit button, triggering duplicate requests or rate-limit warnings.
3. Dropdown menus opened inside modals were clipped by `overflow: hidden` parent containers or hid behind dialog footers.

### The Root Cause
1. **Ad-Hoc Z-Index Values:** Individual components assigned arbitrary Tailwind classes (`z-10`, `z-50`, `z-[999]`), leading to stacking race conditions.
2. **Missing Portal Mounts:** Several dropdowns and popovers rendered inline within their component hierarchy rather than at the top-level document body.
3. **Toaster Z-Index Conflict:** The Sonner toast container (`<Toaster />`) lacked an explicit high-order z-index rule in the application stylesheet, allowing modal backdrops with `z-50` or higher to cover the toasts.

---

## 2. Centralized Stacking Hierarchy Token System

To eliminate all stacking ambiguities, a centralized token dictionary was established in `src/constants/zIndex.ts`:

```typescript
export const Z_INDEX = {
  BASE: 0,
  STICKY_HEADER: 20,
  SIDEBAR: 25,
  DROPDOWN_TRIGGER: 30,
  STICKY_ACTION_BAR: 40,
  
  // Layer 1: Modals and Slide-Over Drawers
  MODAL_BACKDROP: 1000,
  MODAL_CONTENT: 1050,
  DRAWER_BACKDROP: 1000,
  DRAWER_CONTENT: 1050,

  // Layer 2: Nested Confirmation Dialogs
  CONFIRM_BACKDROP: 1100,
  CONFIRM_CONTENT: 1150,

  // Layer 3: Dropdowns, Popovers & Floating Tooltips
  DROPDOWN_POPUP: 2000,
  TOOLTIP: 2100,

  // Layer 4: Global Notifications & System Alerts
  TOAST_NOTIFICATION: 9999,
  CRITICAL_ALERT: 10000,
} as const;
```

### Stacking Rules Enforced:
1. **Rule 1 (Toasts Above Everything):** Toast notifications (`z-index: 9999`) **ALWAYS** render on top of all modals, dialogs, backdrops, and drawers.
2. **Rule 2 (Dropdowns Float Above Modals):** Dropdown menus opened inside modals or drawers (`z-index: 2000`) **ALWAYS** render above modal content (`z-index: 1050`) without being clipped.
3. **Rule 3 (Confirm Above Modals):** Confirmation alerts (`z-index: 1150`) spawned from inside an open modal render above that modal (`z-index: 1050`) with an independent nested backdrop (`z-index: 1100`).

---

## 3. Global Portal Infrastructure (`OverlayPortal.tsx`)

To guarantee zero clipping by `overflow: hidden` containers, `OverlayPortal` mounts directly into `#hotelos-overlay-root` (or falls back to `document.body`):

```tsx
export const OverlayPortal: React.FC<OverlayPortalProps> = ({ children, targetId = 'hotelos-overlay-root' }) => {
  const [mounted, setMounted] = useState(false);
  const portalRef = useRef<HTMLElement | null>(null);

  useEffect(() => {
    let container = document.getElementById(targetId);
    if (!container) {
      container = document.createElement('div');
      container.id = targetId;
      container.style.position = 'relative';
      container.style.zIndex = `${Z_INDEX.DROPDOWN_POPUP}`;
      document.body.appendChild(container);
    }
    portalRef.current = container;
    setMounted(true);
  }, [targetId]);

  if (!mounted || !portalRef.current) return null;
  return createPortal(children, portalRef.current);
};
```

---

## 4. Stacking Scenario Verification Matrix

| # | Scenario Tested | Component Layers Active | Expected Stacking Behavior | Audit Result |
|---|---|---|---|---|
| **1** | Toast fired from within an open Modal | `Modal (1050)` + `Toaster (9999)` | Toast renders cleanly above modal window and backdrop | **VERIFIED** |
| **2** | Toast fired from within a Slide-Over Drawer | `FormDrawer (1050)` + `Toaster (9999)` | Toast appears above drawer header and overlay | **VERIFIED** |
| **3** | Dropdown opened inside a scrollable Modal form | `Modal (1050)` + `UniversalSelect (2000)` | Dropdown popover floats outside modal boundary, zero clipping | **VERIFIED** |
| **4** | Dropdown opened inside a Slide-Over Drawer | `DetailDrawer (1050)` + `UniversalSelect (2000)` | Popover renders above drawer edge | **VERIFIED** |
| **5** | Confirmation Dialog opened over an open Modal | `Modal (1050)` + `ConfirmDialog (1150)` | Confirm dialog cleanly overlays the underlying modal | **VERIFIED** |
| **6** | Toast fired upon confirming deletion in ConfirmDialog | `ConfirmDialog (1150)` + `Toaster (9999)` | Toast renders on top of confirm alert and backdrop | **VERIFIED** |
| **7** | Table Header Sticky vs Table Dropdowns | `StickyHeader (20)` + `ListToolbar Dropdown (2000)` | Dropdown list renders over sticky table headers | **VERIFIED** |
| **8** | Sidebar Collapse vs Sticky Header | `Sidebar (25)` + `StickyHeader (20)` | Sidebar toggles over content without z-index flicker | **VERIFIED** |
| **9** | Multiple stacked Toasts | `Toaster (9999)` | Toasts stack vertically with highest elevation above modals | **VERIFIED** |
| **10**| Tooltip over active Modal Action Button | `Modal (1050)` + `Tooltip (2100)` | Tooltip appears directly above button, above modal body | **VERIFIED** |

---

## 5. CSS & Style Implementation Verification

In `src/styles/globals.css`:
```css
/* Universal Overlay Root */
#hotelos-overlay-root {
  position: relative;
  z-index: 2000;
}

/* Toast container override - guarantees toaster is never occluded by modals */
[data-sonner-toaster] {
  z-index: 9999 !important;
}

[data-sonner-toaster] [data-sonner-toast] {
  z-index: 9999 !important;
}
```

In `src/app/App.tsx`:
```tsx
<Toaster
  position="top-right"
  expand={false}
  richColors
  closeButton
  theme={theme}
  style={{ zIndex: 9999 }}
  className="!z-[9999]"
/>
```

**Conclusion:** The overlay stacking hierarchy is mathematically bounded, systematically enforced, and verified across all viewport sizes and composite interactions.
