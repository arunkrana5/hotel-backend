# HOTEL OS — LOGIN ANIMATION & MOTION DESIGN STANDARDS

**Document Version:** 2.0.0  
**Status:** Approved & Implemented  
**Date:** November 21, 2026  
**Motion Library:** `motion` (`motion/react` v11.18.2)  

---

## 1. Initial Page Entrance Choreography

The login screen enters as an integrated, luxury digital product reveal:

| Step | Element | Direction | Distance | Duration | Delay | Easing |
|---|---|---|---|---|---|---|
| **1** | Left Background Image | Scale / Fade In | `scale: 1.05 -> 1.0` | `1.4s` | `0.0s` | `cubic-bezier(0.25, 1, 0.5, 1)` |
| **2** | Left Brand Mark & Crest | Top to Bottom | `y: -20px -> 0` | `0.8s` | `0.0s` | `easeOut` |
| **3** | Left Headline & Subtitle | Bottom to Top | `y: 20px -> 0` | `0.8s` | `0.2s` | `easeOut` |
| **4** | Right Login Console | Right to Left | `x: 25px -> 0` | `0.7s` | `0.15s` | `easeOut` |
| **5** | Username Field | Bottom to Top | `y: 10px -> 0` | `0.5s` | `0.25s` | `easeOut` |
| **6** | Password Field | Bottom to Top | `y: 10px -> 0` | `0.5s` | `0.30s` | `easeOut` |
| **7** | Remember Me & Demo | Bottom to Top | `y: 10px -> 0` | `0.5s` | `0.35s` | `easeOut` |
| **8** | Sign In Action Button | Bottom to Top | `y: 10px -> 0` | `0.5s` | `0.40s` | `easeOut` |

---

## 2. Uninterrupted Carousel Motion Standards

### Horizontal Slide Mechanics
The carousel avoids abrupt image replacement by using `AnimatePresence custom={direction}` with directional slide transforms:
```typescript
const slideVariants = {
  enter: (direction: number) => ({
    x: direction > 0 ? '100%' : '-100%',
    opacity: 1,
  }),
  center: {
    zIndex: 1,
    x: 0,
    opacity: 1,
  },
  exit: (direction: number) => ({
    zIndex: 0,
    x: direction < 0 ? '100%' : '-100%',
    opacity: 0.85,
  }),
};
```

### Motion Timing & Continuity
- **Transition Duration:** Exactly `1.4 seconds`.
- **Autoplay Interval:** Exactly `7.0 seconds`.
- **Slide Easing:** `cubic-bezier(0.25, 1, 0.5, 1)` (luxury deceleration curve).
- **Zero-Flicker Underlay:** A permanent `#050811` background prevents white or black flashes.
- **Image Preloading:** All slide images are prefetched into the browser cache on mount via `new Image().src`.
- **Page Visibility Coordination:** Listens to `document.addEventListener('visibilitychange')` to halt animations when the tab is inactive.
- **Accessibility:** Fully supports `prefers-reduced-motion` to tone down translational movement for sensitive users.
