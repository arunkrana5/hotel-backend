# HotelOS — Enterprise Hotel Management & Operations Platform

HotelOS is a complete, paperless, enterprise hotel PMS + ERP + operations management system designed for large hotels, luxury resorts, and multi-property chains.

## 1. Project Structure

```text
D:\HotelOS
│
├── hotel-backend       # ASP.NET Core 10 Web API, Application, Domain, Infrastructure, Workers
│   ├── src
│   │   ├── HotelOS.Api             # REST Web APIs, SignalR Hubs & Middlewares
│   │   ├── HotelOS.Application     # CQRS Services, Interfaces & Business Logics
│   │   ├── HotelOS.Domain          # Domain Entities, Value Objects & Enums
│   │   ├── HotelOS.Infrastructure  # PostgreSQL EF Core 10, Dapper, Seeders & Notifications
│   │   ├── HotelOS.Contracts       # Data Transfer Objects, Requests & API Responses
│   │   └── HotelOS.Workers         # Background Services & Scheduled Workflows
│   └── tests
│       ├── HotelOS.UnitTests       # 25 Unit Tests (Domain, Services, Calculators)
│       ├── HotelOS.IntegrationTests# 4 Integration Tests (Auth, Identity, Token Rotation)
│       └── HotelOS.ArchitectureTests# 2 Architecture Boundary Tests (Clean Architecture)
│
├── hotel-frontend      # React 18, Vite, TypeScript, Tailwind CSS, Recharts, Lucide Icons
│   ├── src
│   │   ├── features
│   │   │   ├── authentication      # 11 Identity & Account Management Screens
│   │   │   ├── dashboard           # Executive Command Center with Recharts
│   │   │   ├── front-office        # Room Grid, Arrivals, Departures, Folios & Billing
│   │   │   ├── reservations        # Reservation Lifecycle & Multi-Step Booking
│   │   │   ├── guests              # Guest CRM, Loyalty & Documents
│   │   │   ├── housekeeping        # Status Board, Task Allocations & Linen Registry
│   │   │   ├── restaurant          # POS Terminal, Table Floor Plan & Menu Pricing
│   │   │   ├── kitchen             # Kitchen Display System (KDS) Live Order Queue
│   │   │   ├── banquet             # Event Halls, Spaces & Banquet Bookings
│   │   │   ├── purchase            # Requisitions, Vendor Directory & Purchase Orders
│   │   │   ├── inventory           # Central Stores, Stock Ledger & Reorder Alerts
│   │   │   ├── finance             # General Ledger, Chart of Accounts & Invoicing
│   │   │   ├── hr                  # Employee Directory & Shift Attendance Tracker
│   │   │   ├── maintenance         # Engineering Work Orders & Asset Management
│   │   │   ├── security            # Visitor Access Logs & Incident Reporting
│   │   │   ├── reports             # Night Audit, GL Trial Balance & Yield Forecast
│   │   │   └── it-administration   # Dynamic Menus, Masters, Dropdowns & Workflows
│   │   ├── services                # Typed API Client, SignalR Hub & Reporting Services
│   │   └── layouts                 # App Layout with Real-time Notification Bell & Drawer
│   └── package.json
│
└── README.md
```

## 2. Architectural Principles

1. **Absolute Separation**: Backend and Frontend source codes reside in isolated subdirectories and communicate solely via documented APIs and real-time SignalR WebSockets.
2. **Zero Hardcoded Business Configuration**: Every department, status, role, workflow, and dropdown is database-driven and managed via the IT Department suite.
3. **Universal Server-Side Data Operations**: Tables and dropdowns query database-level pagination, sorting, and filtering (max 5 initial dropdown items with server-side "Show More").
4. **End-to-End Enterprise Security**: Multi-property tenant isolation, adaptive password hashing, audit history, and RBAC/PBAC.
5. **Real-time WebSockets & Push Notifications**: ASP.NET Core SignalR hub `/hubs/notifications` delivering live updates to connected staff and management.

## 3. Implemented Modules (Phases 1 — 7)

| Phase | Module | Status | Highlights |
|---|---|---|---|
| **Phase 1** | Enterprise Foundation & Architecture | Completed | .NET 10, EF Core 10, PostgreSQL, React 18, Vite, Clean Architecture |
| **Phase 2** | Production Auth & Account Security | Completed | 11 UI Screens, JWT rotation, Rate Limiting, Audit History |
| **Phase 3** | Central IT & Dynamic Engines | Completed | Dynamic Dropdown Engine (max 5 items), Dynamic Forms, State Workflows |
| **Phase 4** | Core PMS Engine | Completed | Interactive Room Grid, Check-in/Checkout, Folio Split Billing, Guest CRM |
| **Phase 5** | Hotel Operations Management | Completed | Housekeeping Board, Linen, Restaurant POS, KDS, Banquets, Guest Concierge |
| **Phase 6** | Enterprise ERP Suite | Completed | Purchase & Vendors, Stock Stores, Double-entry GL, HR Shifts, Engineering, Security |
| **Phase 7** | Reporting, Analytics & Real-Time | Completed | Executive Command Center (Recharts), Night Audit, GL Trial Balance, 14-Day Yield Forecast, CSV Streaming, SignalR Notifications |

## 4. Verification & Testing

- **Backend Automated Tests**: 31/31 Passing (`dotnet test D:\HotelOS\hotel-backend\HotelOS.Backend.sln`)
  - Unit Tests: 25 Passed
  - Architecture Boundary Tests: 2 Passed
  - Integration Tests: 4 Passed
- **Frontend Production Build**: 2,098 modules compiled cleanly with 0 TypeScript errors (`npm run build`)
- **Live E2E Verification Suite**: 9/9 Automated checks passing against live API and PostgreSQL (`run_phase7_tests_against_live.ps1`)
