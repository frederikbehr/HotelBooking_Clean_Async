using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace HotelBooking.UnitTests;

public class BookingManagerTests
{
    private readonly IBookingManager bookingManager;
    readonly IRepository<Booking> bookingRepository;
    private readonly Mock<IRepository<Booking>> mockBookingRepo;
    private readonly Mock<IRepository<Room>> mockRoomRepo;
    private readonly BookingManager mockedBookingManager;
    private readonly ITestOutputHelper output;
        
    public BookingManagerTests(ITestOutputHelper output) {
        DateTime start = DateTime.Today.AddDays(10);
        DateTime end = DateTime.Today.AddDays(20);
        bookingRepository = new FakeBookingRepository(start, end);
        IRepository<Room> roomRepository = new FakeRoomRepository();
        bookingManager = new BookingManager(bookingRepository, roomRepository);
        this.output = output;
        // Create mock repositories
        mockBookingRepo = new Mock<IRepository<Booking>>();
        mockRoomRepo = new Mock<IRepository<Room>>();
            
        // Inject mocks into BookingManager
        mockedBookingManager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
    }

    // Same day booking test
    [Fact] 
    public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
    {
        // Arrange
        DateTime date = DateTime.Today;

        // Act
        Task result() => bookingManager.FindAvailableRoom(date, date);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
        
    // Test #1 - Booking in the past
    [Fact] 
    public async Task FindAvailableRoom_BookingInThePast_ThrowsArgumentException()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(-4);
        DateTime endDate = DateTime.Today.AddDays(-2);

        // Act
        Task result() => bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
        
    // Test #2 - Booking starts in the past and ends in the future
    [Fact] 
    public async Task FindAvailableRoom_BookedInPastEndsInFuture_ThrowsArgumentException()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(-2);
        DateTime endDate = DateTime.Today.AddDays(2);

        // Act
        Task result() => bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }

    // Find booking for tomorrow only
    [Fact]
    public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
    {
        // Arrange
        DateTime date = DateTime.Today.AddDays(1);
        // Act
        int roomId = await bookingManager.FindAvailableRoom(date, date);
        // Assert
        Assert.NotEqual(-1, roomId);
    }
        
    // Test #3 - from future date till even more future date
    [Fact]
    public async Task FindAvailableRoom_BookedInFuture_ReturnsAvailableRoom()
    {
        // This test was added to satisfy the following test design
        // principle: "Tests should have strong assertions".

        // Arrange
        DateTime startDate = DateTime.Today.AddDays(2);
        DateTime endDate = DateTime.Today.AddDays(4);
            
        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
            
        var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
            Where(b => b.RoomId == roomId
                       && b.StartDate <= startDate
                       && b.EndDate >= endDate
                       && b.IsActive);

        // bookings are checked to see if any bookings exist for the room id we get.
        // We assert, that there are no bookings for the given room and time.
        // Assert
        Assert.Empty(bookingForReturnedRoomId);
    }

    [Fact]
    public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
    {
        // This test was added to satisfy the following test design
        // principle: "Tests should have strong assertions".

        // Arrange
        DateTime date = DateTime.Today.AddDays(1);
            
        // Act
        int roomId = await bookingManager.FindAvailableRoom(date, date);
            
        var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
            Where(b => b.RoomId == roomId
                       && b.StartDate <= date
                       && b.EndDate >= date
                       && b.IsActive);

        // bookings are checked to see if any bookings exist for the room id we get.
        // We assert, that there are no bookings for the given room and time.
        // Assert
        Assert.Empty(bookingForReturnedRoomId);
    }
        
    // Test #4 - Booking starts before a booked period and ends after
    [Fact] 
    public async Task FindAvailableRoom_BookingThroughWholeBookedPeriod_ReturnsRoomIdMinusOne()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(8);
        DateTime endDate = DateTime.Today.AddDays(22);

        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(-1, roomId);
    }
        
    // Test #5 - Booking starts after a booked period and ends after too
    [Fact] 
    public async Task FindAvailableRoom_BookingAfterBookedPeriod_ReturnsRoomIdMinusOne()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(22);
        DateTime endDate = DateTime.Today.AddDays(24);
        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.NotEqual(-1, roomId);
    }
        
    // Test #6 - Booking overlaps the beginning of a booked period
    [Fact] 
    public async Task FindAvailableRoom_BookingOverlapsBeginningBookedPeriod_ReturnsRoomIdMinusOne()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(8);
        DateTime endDate = DateTime.Today.AddDays(12);

        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(-1, roomId);
    }
        
    // Test #7 - Booking starts in the middle of a booked period and ends the same
    [Fact] 
    public async Task FindAvailableRoom_BookingInsideBookedPeriod_ReturnsRoomIdMinusOne()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(12);
        DateTime endDate = DateTime.Today.AddDays(14);

        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(-1, roomId);
    }
        
    // Test #8 - Booking starts in the middle of a booked period and ends after
    [Fact] 
    public async Task FindAvailableRoom_BookingCrossesEndOfBookedPeriod_ReturnsRoomIdMinusOne()
    {
        // Arrange
        DateTime startDate = DateTime.Today.AddDays(18);
        DateTime endDate = DateTime.Today.AddDays(22);

        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(-1, roomId);
    }
        
    [Fact]
    public async Task FindAvailableRoom_ShouldReturnRoomId_WhenRoomIsAvailable()
    {
        // Defining rooms for setup
        var rooms = new List<Room>
        {
            new Room { Id = 1 }, 
            new Room { Id = 2 },
        };
        
        // Defining bookings for setup
        var bookings = new List<Booking>
        {
            new Booking { RoomId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true }
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);
        
        // Act (Call method)
        var roomId = await mockedBookingManager.FindAvailableRoom(DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));
        
        // Assert (Check result)
        Assert.True(roomId != -1, "Expected an available room ID");
    }
    
    [Fact]
    public async Task FindAvailableRoom_ShouldReturnMinusOne_WhenNoRoomIsAvailable()
    {
        // Defining rooms for setup
        var rooms = new List<Room>
        {
            new Room { Id = 1 }, 
        };
        
        // Defining bookings for setup
        var bookings = new List<Booking>
        {
            new Booking { RoomId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true }
        };

        mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act (Call method)
        var roomId = await mockedBookingManager.FindAvailableRoom(DateTime.Today.AddDays(2), DateTime.Today.AddDays(4));
        
        // Assert (Check result)
        Assert.True(roomId == -1, "Expected an available room ID");
    }
        
    // XXXXXXXXXXXXXXXXX
        
        

}