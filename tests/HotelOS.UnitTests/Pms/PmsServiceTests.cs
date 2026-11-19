using HotelOS.Contracts.Pms;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelOS.UnitTests.Pms;

public class PmsServiceTests
{
    private HotelOSDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HotelOSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HotelOSDbContext(options);
    }

    [Fact]
    public async Task CreateReservation_WithValidDates_CalculatesTotalAndCreatesFolio()
    {
        using var context = CreateInMemoryContext();
        var service = new PmsService(context);

        var request = new CreateReservationRequest(
            GuestId: null,
            NewGuest: new CreateGuestRequest(
                FirstName: "John",
                LastName: "Doe",
                Email: "john.doe@test.com",
                Phone: "+1 555 1234"
            ),
            RoomTypeCode: "DLX",
            CheckInDate: "2026-12-01",
            CheckOutDate: "2026-12-04",
            AdultsCount: 2,
            CustomRate: 200m
        );

        var result = await service.CreateReservationAsync(request, propertyId: 1, createdBy: "TEST");

        Assert.NotNull(result);
        Assert.StartsWith("RES-", result.ConfirmationNumber);
        Assert.Equal("CONF", result.ReservationStatus);
        Assert.Equal(600m, result.TotalEstimatedAmount); // 3 nights @ $200

        var savedFolio = await context.Folios.FirstOrDefaultAsync(f => f.ReservationId == result.Id);
        Assert.NotNull(savedFolio);
        Assert.Equal("Open", savedFolio.Status);
    }

    [Fact]
    public async Task CheckInGuest_AllocatesRoom_SetsRoomStatusToOCC_AndPostsRoomCharge()
    {
        using var context = CreateInMemoryContext();
        var service = new PmsService(context);

        var room = new Room
        {
            PropertyId = 1,
            RoomNumber = "101",
            FloorNumber = 1,
            RoomTypeCode = "DLX",
            RoomStatusCode = "CLN",
            BaseRatePerNight = 180m,
            CreatedBy = "TEST"
        };
        context.Rooms.Add(room);

        var guest = new Guest
        {
            PropertyId = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@test.com",
            Phone = "+1 555 8888",
            CreatedBy = "TEST"
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();

        var res = await service.CreateReservationAsync(new CreateReservationRequest(
            GuestId: guest.Id,
            NewGuest: null,
            RoomTypeCode: "DLX",
            CheckInDate: "2026-12-10",
            CheckOutDate: "2026-12-12", // 2 nights
            CustomRate: 180m
        ), 1, "TEST");

        var checkInResult = await service.CheckInGuestAsync(new CheckInRequest(
            ReservationId: res.Id,
            RoomId: room.Id,
            KeyCardUid: "KEY-101-ABC",
            AdvanceDeposit: 100m,
            PaymentModeCode: "CC"
        ), "FRONTDESK");

        Assert.Equal("CHKI", checkInResult.ReservationStatus);
        Assert.Equal("101", checkInResult.AssignedRoomNumber);

        var updatedRoom = await context.Rooms.FindAsync(room.Id);
        Assert.Equal("OCC", updatedRoom!.RoomStatusCode);
        Assert.Equal("KEY-101-ABC", updatedRoom.KeyCardUid);

        var folio = await context.Folios.Include(f => f.Items).FirstOrDefaultAsync(f => f.ReservationId == res.Id);
        Assert.NotNull(folio);
        Assert.Equal(360m, folio.TotalCharges); // 2 nights @ $180
        Assert.Equal(100m, folio.TotalPayments); // $100 deposit
        Assert.Equal(260m, folio.BalanceAmount); // $360 - $100 = $260
    }

    [Fact]
    public async Task CheckOutGuest_SetsRoomStatusToDRT_AndClosesFolio()
    {
        using var context = CreateInMemoryContext();
        var service = new PmsService(context);

        var room = new Room
        {
            PropertyId = 1,
            RoomNumber = "201",
            FloorNumber = 2,
            RoomTypeCode = "EXEC",
            RoomStatusCode = "CLN",
            BaseRatePerNight = 250m,
            CreatedBy = "TEST"
        };
        context.Rooms.Add(room);

        var guest = new Guest
        {
            PropertyId = 1,
            FirstName = "Robert",
            LastName = "Taylor",
            Email = "robert@test.com",
            Phone = "+1 555 9999",
            CreatedBy = "TEST"
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();

        var res = await service.CreateReservationAsync(new CreateReservationRequest(
            GuestId: guest.Id,
            NewGuest: null,
            RoomTypeCode: "EXEC",
            CheckInDate: "2026-12-05",
            CheckOutDate: "2026-12-06", // 1 night @ $250
            CustomRate: 250m
        ), 1, "TEST");

        await service.CheckInGuestAsync(new CheckInRequest(
            ReservationId: res.Id,
            RoomId: room.Id
        ), "FRONTDESK");

        // Now checkout with remaining balance settlement payment of $250
        var checkOutResult = await service.CheckOutGuestAsync(new CheckOutRequest(
            ReservationId: res.Id,
            SettlementPayment: 250m,
            PaymentModeCode: "CC"
        ), "FRONTDESK");

        Assert.Equal("CHKO", checkOutResult.ReservationStatus);

        // Verify Automatic Transition: Room is now DRT (Dirty) for Housekeeping!
        var updatedRoom = await context.Rooms.FindAsync(room.Id);
        Assert.Equal("DRT", updatedRoom!.RoomStatusCode);

        var folio = await context.Folios.FirstOrDefaultAsync(f => f.ReservationId == res.Id);
        Assert.Equal("Settled", folio!.Status);
        Assert.Equal(0m, folio.BalanceAmount);
    }
}
