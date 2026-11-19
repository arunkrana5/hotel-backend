using HotelOS.Contracts.Finance;
using HotelOS.Contracts.Hr;
using HotelOS.Contracts.Inventory;
using HotelOS.Contracts.Maintenance;
using HotelOS.Contracts.Purchase;
using HotelOS.Contracts.Security;
using HotelOS.Domain.Finance.Entities;
using HotelOS.Domain.Inventory.Entities;
using HotelOS.Domain.Maintenance.Entities;
using HotelOS.Domain.Purchase.Entities;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelOS.UnitTests.Erp;

public class ErpServiceTests
{
    private HotelOSDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HotelOSDbContext(options);
    }

    [Fact]
    public async Task PurchaseService_CreateVendorAndRequisition_Succeeds()
    {
        using var context = CreateInMemoryContext();
        var purchaseService = new PurchaseService(context);

        // 1. Create Vendor
        var vendorReq = new CreateVendorRequest(
            "VND-TEST-01",
            "Test Linens Supply",
            "Jane Doe",
            "orders@testlinens.com",
            "+1 555 0101",
            "123 Mill Rd",
            "TX-998811",
            30
        );
        var vendor = await purchaseService.CreateVendorAsync(vendorReq, 1, "TEST_USER");
        Assert.NotNull(vendor);
        Assert.Equal("VND-TEST-01", vendor.VendorCode);

        // 2. Create Requisition
        var req = new CreatePurchaseRequisitionRequest(
            "HK",
            "2026-12-01",
            "HIGH",
            "Restock luxury towels for holiday season",
            new List<CreateRequisitionItemRequest>
            {
                new("Bath Sheet White 800gsm", "Egyptian cotton", 50, "PCS", 25.00m)
            }
        );

        var requisition = await purchaseService.CreateRequisitionAsync(req, 1, 1, "TEST_USER");
        Assert.NotNull(requisition);
        Assert.Equal("PENDING_APPROVAL", requisition.Status);
        Assert.Equal(1250.00m, requisition.TotalEstimatedAmount);
    }

    [Fact]
    public async Task InventoryService_RecordTransaction_UpdatesCurrentStockCorrectly()
    {
        using var context = CreateInMemoryContext();
        var inventoryService = new InventoryService(context);

        var itemReq = new CreateInventoryItemRequest(
            "INV-TST-01",
            "Artisan Coffee Beans 1kg",
            "FOOD_BEVERAGE",
            "BAG",
            18.50m,
            20,
            10,
            5,
            100,
            "SHELF-FB-01"
        );
        var item = await inventoryService.CreateItemAsync(itemReq, 1, "TEST_USER");
        Assert.Equal(20, item.CurrentStock);

        // Stock Issue: -5 bags
        var txReq = new RecordStockTransactionRequest(
            item.Id,
            "ISSUE",
            5,
            18.50m,
            "REST",
            "REQ-REST-001",
            "Issued to main restaurant barista station"
        );
        var tx = await inventoryService.RecordTransactionAsync(txReq, 1, "TEST_USER");
        Assert.Equal("ISSUE", tx.TransactionType);

        var updatedItem = await context.InventoryItems.FindAsync(item.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(15, updatedItem.CurrentStock);
    }

    [Fact]
    public async Task FinanceService_CreateJournalEntry_EnforcesBalancedDebitCredit()
    {
        using var context = CreateInMemoryContext();
        var financeService = new FinanceService(context);

        var acc1 = new Account { PropertyId = 1, AccountCode = "1010", AccountName = "Cash", AccountType = "ASSET", Balance = 1000m };
        var acc2 = new Account { PropertyId = 1, AccountCode = "4010", AccountName = "Revenue", AccountType = "REVENUE", Balance = 0m };
        context.Accounts.AddRange(acc1, acc2);
        await context.SaveChangesAsync();

        // 1. Unbalanced should throw
        var unbalancedReq = new CreateJournalEntryRequest(
            "2026-11-19",
            "Unbalanced entry",
            null,
            new List<CreateJournalLineRequest>
            {
                new(acc1.Id, 100m, 0m, "Debit Cash"),
                new(acc2.Id, 0m, 80m, "Credit Revenue") // Mismatch!
            }
        );
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            financeService.CreateJournalEntryAsync(unbalancedReq, 1, "TEST_USER"));

        // 2. Balanced should succeed
        var balancedReq = new CreateJournalEntryRequest(
            "2026-11-19",
            "Balanced entry",
            "REF-001",
            new List<CreateJournalLineRequest>
            {
                new(acc1.Id, 100m, 0m, "Debit Cash"),
                new(acc2.Id, 0m, 100m, "Credit Revenue")
            }
        );
        var entry = await financeService.CreateJournalEntryAsync(balancedReq, 1, "TEST_USER");
        Assert.NotNull(entry);
        Assert.Equal(100m, entry.TotalDebit);
        Assert.Equal(100m, entry.TotalCredit);
    }

    [Fact]
    public async Task FinanceService_RecordInvoicePayment_CalculatesBalanceAndStatus()
    {
        using var context = CreateInMemoryContext();
        var financeService = new FinanceService(context);

        var invReq = new CreateInvoiceRequest(
            "GUEST",
            "RES-1001",
            "Dr. Richard Sterling",
            1000m,
            100m,
            "2026-11-19",
            "2026-11-26",
            "Corporate conference folio checkout"
        );
        var invoice = await financeService.CreateInvoiceAsync(invReq, 1, "TEST_USER");
        Assert.Equal(1100m, invoice.TotalAmount);
        Assert.Equal(1100m, invoice.BalanceAmount);
        Assert.Equal("ISSUED", invoice.Status);

        // Partial payment $600
        var payment1 = await financeService.RecordPaymentAsync(
            invoice.Id,
            new RecordInvoicePaymentRequest(600m, "CREDIT_CARD"),
            "TEST_USER"
        );
        Assert.NotNull(payment1);
        Assert.Equal(500m, payment1.BalanceAmount);
        Assert.Equal("PARTIALLY_PAID", payment1.Status);

        // Full settlement remaining $500
        var payment2 = await financeService.RecordPaymentAsync(
            invoice.Id,
            new RecordInvoicePaymentRequest(500m, "BANK_TRANSFER"),
            "TEST_USER"
        );
        Assert.NotNull(payment2);
        Assert.Equal(0m, payment2.BalanceAmount);
        Assert.Equal("PAID", payment2.Status);
    }

    [Fact]
    public async Task HrService_RecordAttendance_CreatesOrUpdatesAttendance()
    {
        using var context = CreateInMemoryContext();
        var hrService = new HrService(context);

        var emp = await hrService.CreateEmployeeAsync(new CreateEmployeeRequest(
            "EMP-TEST-99",
            null,
            "James",
            "Wilson",
            "ENG",
            "HVAC Technician",
            "2025-05-10",
            "FULL_TIME",
            4000m,
            "j.wilson@hotelos.com",
            "+1 555 9988",
            null
        ), 1, "TEST_USER");

        // Record Check-in
        var att = await hrService.RecordAttendanceAsync(new RecordAttendanceRequest(
            emp.Id,
            "2026-11-19",
            "08:05",
            null,
            "MORNING",
            "PRESENT",
            0,
            "On time morning shift"
        ), 1, "TEST_USER");

        Assert.NotNull(att);
        Assert.Equal("PRESENT", att.Status);
        Assert.Equal("08:05", att.CheckInTime);

        // Record Check-out later in the day
        var attOut = await hrService.RecordAttendanceAsync(new RecordAttendanceRequest(
            emp.Id,
            "2026-11-19",
            "08:05",
            "17:30",
            "MORNING",
            "PRESENT",
            1.5m,
            "Shift completed with 1.5h overtime"
        ), 1, "TEST_USER");

        Assert.Equal("17:30", attOut.CheckOutTime);
        Assert.Equal(1.5m, attOut.OvertimeHours);
    }

    [Fact]
    public async Task MaintenanceService_CreateAndCompleteWorkOrder_ResolvesAssetStatus()
    {
        using var context = CreateInMemoryContext();
        var maintService = new MaintenanceService(context);

        var asset = await maintService.CreateAssetAsync(new CreateAssetRequest(
            "AST-TST-CHL",
            "Rooftop Cooling Tower Unit 3",
            "HVAC",
            "Roof Plant Deck",
            "2024-06-01",
            35000m,
            "2029-06-01",
            "SN-TRANE-900",
            "Trane"
        ), 1, "TEST_USER");

        var wo = await maintService.CreateWorkOrderAsync(new CreateWorkOrderRequest(
            asset.Id,
            null,
            "ENG",
            "Compressor belt vibration detected",
            "HIGH",
            null
        ), 1, "TEST_USER");

        Assert.Equal("OPEN", wo.Status);

        // Progress work order
        await maintService.UpdateWorkOrderStatusAsync(wo.Id, new UpdateWorkOrderStatusRequest(
            "IN_PROGRESS",
            "Technician diagnosing belt tension",
            0
        ), "TEST_USER");

        var assetUnderMaint = await context.Assets.FindAsync(asset.Id);
        Assert.Equal("UNDER_MAINTENANCE", assetUnderMaint?.Status);

        // Complete work order
        var completedWo = await maintService.UpdateWorkOrderStatusAsync(wo.Id, new UpdateWorkOrderStatusRequest(
            "COMPLETED",
            "Replaced tensioner pulley and belt. Vibration eliminated.",
            150m
        ), "TEST_USER");

        Assert.Equal("COMPLETED", completedWo?.Status);
        Assert.NotNull(completedWo?.CompletedAt);
        Assert.Equal(150m, completedWo?.TotalCost);

        var assetRestored = await context.Assets.FindAsync(asset.Id);
        Assert.Equal("OPERATIONAL", assetRestored?.Status);
    }

    [Fact]
    public async Task SecurityService_VisitorCheckInAndCheckOut_TracksStatus()
    {
        using var context = CreateInMemoryContext();
        var secService = new SecurityService(context);

        var checkInReq = new CheckInVisitorRequest(
            "Alexander Hamilton",
            "DRIVERS_LICENSE",
            "DL-NY-9912",
            "+1 555 1776",
            "Auditing Financial Ledgers",
            "FINANCE",
            "Accounts Office 204",
            "BADGE-09"
        );
        var visitor = await secService.CheckInVisitorAsync(checkInReq, 1, "TEST_USER");
        Assert.NotNull(visitor);
        Assert.Equal("IN_PREMISES", visitor.Status);
        Assert.StartsWith("PASS-", visitor.PassNumber);

        var checkedOut = await secService.CheckOutVisitorAsync(visitor.Id, new CheckOutVisitorRequest("Badge returned safely"), "TEST_USER");
        Assert.NotNull(checkedOut);
        Assert.Equal("CHECKED_OUT", checkedOut.Status);
        Assert.NotNull(checkedOut.CheckOutTime);
    }
}
