using System.Text;
using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Reporting;
using HotelOS.Domain.Finance.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelOS.UnitTests.Reporting;

public class ReportingServiceTests
{
    private HotelOSDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HotelOSDbContext(options);
    }

    private class MockNotificationSender : IRealtimeNotificationSender
    {
        public List<NotificationDto> SentNotifications { get; } = new();
        public List<(string EventType, object Payload)> LiveEvents { get; } = new();

        public Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            SentNotifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task BroadcastLiveEventAsync(string eventType, object payload, CancellationToken cancellationToken = default)
        {
            LiveEvents.Add((eventType, payload));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecutiveDashboard_Calculates_Adr_RevPar_And_OccupancyRate_Correctly()
    {
        using var context = CreateInMemoryContext();
        var reportingService = new ReportingService(context);

        long propertyId = 1;

        // Seed 4 Rooms (2 occupied, 1 clean vacant, 1 dirty vacant)
        var r1 = new Room { PropertyId = propertyId, RoomNumber = "101", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "OCC", CreatedBy = "SYSTEM" };
        var r2 = new Room { PropertyId = propertyId, RoomNumber = "102", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "OCC", CreatedBy = "SYSTEM" };
        var r3 = new Room { PropertyId = propertyId, RoomNumber = "103", FloorNumber = 1, RoomTypeCode = "STD", RoomStatusCode = "CLN", CreatedBy = "SYSTEM" };
        var r4 = new Room { PropertyId = propertyId, RoomNumber = "104", FloorNumber = 1, RoomTypeCode = "STD", RoomStatusCode = "DRT", CreatedBy = "SYSTEM" };
        context.Rooms.AddRange(r1, r2, r3, r4);

        await context.SaveChangesAsync();

        // Act
        var dashboard = await reportingService.GetExecutiveDashboardAsync(propertyId);

        // Assert
        Assert.NotNull(dashboard);
        Assert.Equal(4, dashboard.OperationalHealth.TotalRooms);
        Assert.Equal(2, dashboard.OperationalHealth.OccupiedRooms);
        Assert.Equal(50.0, dashboard.OccupancyRate); // 2 occupied / 4 total = 50%
        Assert.True(dashboard.Adr > 0);
        Assert.True(dashboard.RevPar > 0);
        Assert.Equal(1, dashboard.OperationalHealth.CleanRooms);
        Assert.Equal(1, dashboard.OperationalHealth.DirtyRooms);
    }

    [Fact]
    public async Task RevenueTrialBalance_IsBalanced_DebitsEqualCredits()
    {
        using var context = CreateInMemoryContext();
        var reportingService = new ReportingService(context);

        long propertyId = 1;

        // Seed accounts
        var accCash = new Account { PropertyId = propertyId, AccountCode = "1010", AccountName = "Cash & Bank", AccountType = "ASSET", Balance = 10000m, CreatedBy = "SYSTEM" };
        var accRev = new Account { PropertyId = propertyId, AccountCode = "4010", AccountName = "Room Revenue", AccountType = "REVENUE", Balance = 0m, CreatedBy = "SYSTEM" };
        context.Accounts.AddRange(accCash, accRev);
        await context.SaveChangesAsync();

        // Seed balanced journal entry
        var entry = new JournalEntry
        {
            PropertyId = propertyId,
            EntryNumber = "JE-2026-001",
            EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Front Office Nightly Postings",
            ReferenceNumber = "NIGHT_AUDIT",
            Status = "POSTED",
            TotalDebit = 1500m,
            TotalCredit = 1500m,
            CreatedBy = "SYSTEM",
            Lines = new List<JournalLine>
            {
                new() { AccountId = accCash.Id, DebitAmount = 1500m, CreditAmount = 0m, Description = "Guest Payments" },
                new() { AccountId = accRev.Id, DebitAmount = 0m, CreditAmount = 1500m, Description = "Lodging Charges" }
            }
        };

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        var trialBalance = await reportingService.GetRevenueTrialBalanceReportAsync(propertyId);

        // Assert
        Assert.NotNull(trialBalance);
        Assert.True(trialBalance.IsBalanced);
        Assert.Equal(1500m, trialBalance.TotalDebits);
        Assert.Equal(1500m, trialBalance.TotalCredits);
        Assert.Equal(2, trialBalance.Accounts.Count);
    }

    [Fact]
    public async Task OccupancyForecast_Generates_Exact_Requested_Days()
    {
        using var context = CreateInMemoryContext();
        var reportingService = new ReportingService(context);

        long propertyId = 1;
        context.Rooms.Add(new Room { PropertyId = propertyId, RoomNumber = "101", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "CLN", CreatedBy = "SYSTEM" });
        await context.SaveChangesAsync();

        // Act
        var forecast = await reportingService.GetOccupancyForecastReportAsync(propertyId, days: 14);

        // Assert
        Assert.NotNull(forecast);
        Assert.Equal(14, forecast.ForecastDays.Count);
    }

    [Fact]
    public async Task ExportReportCsv_Produces_Valid_Csv_Payload()
    {
        using var context = CreateInMemoryContext();
        var reportingService = new ReportingService(context);

        long propertyId = 1;
        context.Rooms.Add(new Room { PropertyId = propertyId, RoomNumber = "201", FloorNumber = 2, RoomTypeCode = "DLX", RoomStatusCode = "OCC", CreatedBy = "SYSTEM" });
        await context.SaveChangesAsync();

        // Act
        var csvBytes = await reportingService.ExportReportCsvAsync("DASHBOARD", propertyId);
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        Assert.NotEmpty(csvBytes);
        Assert.Contains("Metric,Value", csvContent);
        Assert.Contains("Total Rooms,1", csvContent);
        Assert.Contains("Occupied Rooms,1", csvContent);
    }

    [Fact]
    public async Task NotificationService_Create_And_Read_Lifecycle_Works()
    {
        using var context = CreateInMemoryContext();
        var mockSender = new MockNotificationSender();
        var notificationService = new NotificationService(context, mockSender);

        long propertyId = 1;
        long userId = 42;

        // 1. Create Notification
        var notif = await notificationService.CreateNotificationAsync(
            "VIP Arrival Alert",
            "Guest Ambassador arriving for Presidential Suite check-in.",
            "ALERT",
            "/front-office/arrivals",
            userId,
            propertyId
        );

        Assert.NotNull(notif);
        Assert.False(notif.IsRead);
        Assert.Equal("VIP Arrival Alert", notif.Title);
        Assert.Single(mockSender.SentNotifications);

        // 2. Fetch Notifications
        var list = await notificationService.GetNotificationsAsync(userId, propertyId);
        Assert.Single(list);
        Assert.Equal(notif.Id, list[0].Id);

        // 3. Mark As Read
        var readSuccess = await notificationService.MarkAsReadAsync(notif.Id);
        Assert.True(readSuccess);

        var listAfterRead = await notificationService.GetNotificationsAsync(userId, propertyId);
        Assert.True(listAfterRead[0].IsRead);

        // 4. Create another and test MarkAllAsRead
        await notificationService.CreateNotificationAsync("Daily Turnover", "Turnover completed.", "INFO", null, userId, propertyId);
        var markedCount = await notificationService.MarkAllAsReadAsync(userId, propertyId);
        Assert.Equal(1, markedCount);
    }
}
