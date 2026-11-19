using HotelOS.Contracts.Banquet;
using HotelOS.Contracts.Housekeeping;
using HotelOS.Contracts.Operations;
using HotelOS.Contracts.Restaurant;
using HotelOS.Domain.Banquet.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Domain.Restaurant.Entities;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelOS.UnitTests.Operations;

public class OperationsServiceTests
{
    private HotelOSDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HotelOSDbContext(options);
    }

    [Fact]
    public async Task HousekeepingTask_Completed_TransitionsRoomToClean()
    {
        using var context = CreateInMemoryContext();
        var room = new Room
        {
            PropertyId = 1,
            RoomNumber = "101",
            FloorNumber = 1,
            RoomTypeCode = "DLX",
            RoomStatusCode = "DRT" // Room is dirty after checkout
        };
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        var service = new HousekeepingService(context);

        var task = await service.CreateTaskAsync(
            new CreateHousekeepingTaskRequest(room.Id, "TURNOVER", "HIGH", null, "Guest checked out"),
            propertyId: 1,
            createdBy: "TEST");

        Assert.NotNull(task);
        Assert.Equal("PENDING", task.Status);

        // Attendant cleans room
        var updated = await service.UpdateTaskStatusAsync(
            task.Id,
            new UpdateHousekeepingTaskStatusRequest("COMPLETED", "Thoroughly cleaned"),
            currentUserId: null,
            updatedBy: "attendant1");

        Assert.NotNull(updated);
        Assert.Equal("COMPLETED", updated.Status);

        // Verify Room status transitioned to CLN
        var refreshedRoom = await context.Rooms.FindAsync(room.Id);
        Assert.NotNull(refreshedRoom);
        Assert.Equal("CLN", refreshedRoom.RoomStatusCode);
    }

    [Fact]
    public async Task GuestLaundryOrder_OccupiedRoom_PostsChargeToFolio()
    {
        using var context = CreateInMemoryContext();
        var room = new Room { PropertyId = 1, RoomNumber = "102", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "OCC" };
        var guest = new Guest { PropertyId = 1, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" };
        context.Rooms.Add(room);
        context.Guests.Add(guest);
        await context.SaveChangesAsync();

        var res = new Reservation
        {
            PropertyId = 1,
            ConfirmationNumber = "RES-TEST-01",
            GuestId = guest.Id,
            AssignedRoomId = room.Id,
            ReservationStatus = "CHKI"
        };
        context.Reservations.Add(res);
        await context.SaveChangesAsync();

        var folio = new Folio
        {
            PropertyId = 1,
            ReservationId = res.Id,
            GuestId = guest.Id,
            FolioNumber = "FOL-TEST-01",
            Status = "Open",
            TotalCharges = 0,
            BalanceAmount = 0
        };
        context.Folios.Add(folio);
        await context.SaveChangesAsync();

        var service = new HousekeepingService(context);

        var laundryOrder = await service.CreateLaundryOrderAsync(
            new CreateLaundryOrderRequest(
                OrderType: "GUEST",
                RoomId: room.Id,
                GuestId: guest.Id,
                IsExpress: false,
                SpecialInstructions: "Extra starch on dress shirts",
                Items: new List<CreateLaundryOrderItemRequest>
                {
                    new("DRY_CLEAN", "Silk Evening Blouse", 2, 15m, null),
                    new("WASH_AND_FOLD", "Cotton T-Shirt", 3, 5m, null)
                }
            ),
            propertyId: 1,
            createdBy: "TEST");

        Assert.NotNull(laundryOrder);
        Assert.Equal(5, laundryOrder.TotalPieces);
        Assert.True(laundryOrder.TotalAmount > 0);

        // Verify folio charge posted automatically
        var refreshedFolio = await context.Folios.FindAsync(folio.Id);
        Assert.NotNull(refreshedFolio);
        Assert.True(refreshedFolio.TotalCharges > 0);
        Assert.Equal(laundryOrder.TotalAmount, refreshedFolio.BalanceAmount);

        var folioItem = await context.FolioItems.FirstOrDefaultAsync(f => f.FolioId == folio.Id);
        Assert.NotNull(folioItem);
        Assert.Equal("HK", folioItem.DepartmentCode);
        Assert.Equal(laundryOrder.TotalAmount, folioItem.Amount);
    }

    [Fact]
    public async Task RestaurantOrder_SettleWithRoomCharge_UpdatesTableAndFolio()
    {
        using var context = CreateInMemoryContext();
        var table = new DiningTable { PropertyId = 1, TableNumber = "T-01", Section = "MAIN_DINING", Capacity = 4, Status = "AVAILABLE" };
        var room = new Room { PropertyId = 1, RoomNumber = "104", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "OCC" };
        var guest = new Guest { PropertyId = 1, FirstName = "Robert", LastName = "Vance", Email = "bob@vance.com" };
        var menuItem = new MenuItem { PropertyId = 1, ItemCode = "MNU-BURGER", ItemName = "Wagyu Burger", Category = "MAINS", Price = 30m, IsAvailable = true };
        context.DiningTables.Add(table);
        context.Rooms.Add(room);
        context.Guests.Add(guest);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();

        var res = new Reservation
        {
            PropertyId = 1,
            ConfirmationNumber = "RES-TEST-02",
            GuestId = guest.Id,
            AssignedRoomId = room.Id,
            ReservationStatus = "CHKI"
        };
        context.Reservations.Add(res);
        await context.SaveChangesAsync();

        var folio = new Folio
        {
            PropertyId = 1,
            ReservationId = res.Id,
            GuestId = guest.Id,
            FolioNumber = "FOL-TEST-02",
            Status = "Open",
            TotalCharges = 0,
            BalanceAmount = 0
        };
        context.Folios.Add(folio);
        await context.SaveChangesAsync();

        var restaurantService = new RestaurantService(context);

        // 1. Create Order
        var order = await restaurantService.CreateOrderAsync(
            new CreateRestaurantOrderRequest(
                OrderType: "DINE_IN",
                TableId: table.Id,
                RoomId: room.Id,
                GuestId: guest.Id,
                SpecialInstructions: "Medium rare",
                Items: new List<CreateOrderItemRequest>
                {
                    new(menuItem.Id, 2, "Extra pickles")
                }
            ),
            propertyId: 1,
            serverUserId: null,
            createdBy: "TEST");

        Assert.NotNull(order);
        Assert.Equal("PLACED", order.Status);

        // Verify table marked OCCUPIED
        var refreshedTable = await context.DiningTables.FindAsync(table.Id);
        Assert.NotNull(refreshedTable);
        Assert.Equal("OCCUPIED", refreshedTable.Status);

        // 2. Kitchen starts cooking
        var cookingOrder = await restaurantService.UpdateOrderStatusAsync(
            order.Id,
            new UpdateOrderStatusRequest("COOKING"),
            updatedBy: "chef1");

        Assert.NotNull(cookingOrder);
        Assert.Equal("COOKING", cookingOrder.Status);

        // 3. Settle with direct Room Charge
        var settled = await restaurantService.SettleOrderAsync(
            order.Id,
            new SettleRestaurantOrderRequest("CHARGED_TO_ROOM", "ROOM_CHARGE"),
            settledBy: "waiter1");

        Assert.NotNull(settled);
        Assert.Equal("BILLED", settled.Status);
        Assert.Equal("CHARGED_TO_ROOM", settled.PaymentStatus);

        // Verify table is released to AVAILABLE
        refreshedTable = await context.DiningTables.FindAsync(table.Id);
        Assert.NotNull(refreshedTable);
        Assert.Equal("AVAILABLE", refreshedTable.Status);

        // Verify Folio received F&B charge
        var refreshedFolio = await context.Folios.FindAsync(folio.Id);
        Assert.NotNull(refreshedFolio);
        Assert.True(refreshedFolio.TotalCharges > 0);

        var folioItem = await context.FolioItems.FirstOrDefaultAsync(f => f.FolioId == folio.Id && f.DepartmentCode == "FB");
        Assert.NotNull(folioItem);
        Assert.Equal(settled.TotalAmount, folioItem.Amount);
    }

    [Fact]
    public async Task BanquetBooking_CalculatesBalance_AndGuestRequestLifecycle()
    {
        using var context = CreateInMemoryContext();
        var venue = new BanquetVenue
        {
            PropertyId = 1,
            VenueCode = "VNU-01",
            VenueName = "Grand Ballroom",
            Location = "Level 2",
            MaxCapacity = 400,
            HourlyRate = 300m,
            DayRate = 3000m,
            Status = "AVAILABLE"
        };
        var room = new Room { PropertyId = 1, RoomNumber = "201", FloorNumber = 2, RoomTypeCode = "EXEC", RoomStatusCode = "OCC" };
        context.BanquetVenues.Add(venue);
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        var banquetService = new BanquetService(context);
        var opsService = new OperationsService(context);

        // 1. Banquet Booking
        var booking = await banquetService.CreateBookingAsync(
            new CreateBanquetBookingRequest(
                VenueId: venue.Id,
                EventName: "Annual Tech Gala",
                EventType: "GALA",
                ClientName: "TechCorp",
                ClientPhone: "+1 555 9999",
                ClientEmail: "gala@techcorp.com",
                GuestId: null,
                StartDateTime: DateTimeOffset.UtcNow.AddDays(10),
                EndDateTime: DateTimeOffset.UtcNow.AddDays(10).AddHours(6),
                ExpectedGuests: 250,
                TotalAmount: 6000m,
                AdvancePaid: 2500m,
                CateringPackage: "Premium 4-Course",
                SetupStyle: "ROUND_TABLE",
                SpecialRequests: "Stage and LED wall"
            ),
            propertyId: 1,
            createdBy: "TEST");

        Assert.NotNull(booking);
        Assert.Equal("CONFIRMED", booking.Status);
        Assert.Equal(3500m, booking.BalanceAmount);

        // 2. Guest Service Request
        var guestReq = await opsService.CreateGuestRequestAsync(
            new CreateGuestServiceRequest(
                RoomId: room.Id,
                GuestId: null,
                RequestType: "EXTRA_TOWELS",
                Department: "HOUSEKEEPING",
                Priority: "HIGH",
                Description: "Four extra pool towels",
                AssignedToUserId: null
            ),
            propertyId: 1,
            createdBy: "TEST");

        Assert.NotNull(guestReq);
        Assert.Equal("PENDING", guestReq.Status);

        var resolved = await opsService.UpdateGuestRequestStatusAsync(
            guestReq.Id,
            new UpdateGuestServiceRequestStatus("RESOLVED", "Delivered by runner", null),
            updatedBy: "runner1");

        Assert.NotNull(resolved);
        Assert.Equal("RESOLVED", resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);
        Assert.Equal("Delivered by runner", resolved.ResolutionNotes);
    }
}
