using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Threading.Tasks;
using Moq;

namespace HotelBooking.UnitTests;

public class BookingManagerTests
{
    private readonly BookingManager bookingManager;

    public BookingManagerTests()
    {
        var today = DateTime.Today;
        var bookedPeriodStart = today.AddDays(10);
        var bookedPeriodEnd = today.AddDays(20);

        // Create mocks
        var bookingRepoMock1 = new Mock<IRepository<Booking>>();
        
        bookingRepoMock1.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking>
        {
            new Booking { Id=1, StartDate=DateTime.Today.AddDays(1), EndDate=DateTime.Today.AddDays(1), IsActive=true, CustomerId=1, RoomId=1 },
            new Booking { Id=1, StartDate=bookedPeriodStart, EndDate=bookedPeriodEnd, IsActive=true, CustomerId=1, RoomId=1 },
            new Booking { Id=2, StartDate=bookedPeriodStart, EndDate=bookedPeriodEnd, IsActive=true, CustomerId=2, RoomId=2 },
        });
        
        IRepository<Room> roomRepository = new FakeRoomRepository();
        bookingManager = new BookingManager(bookingRepoMock1.Object, roomRepository);
    }
    
    public static IEnumerable<object[]> GetBookingTestCasesFindingAvailableRoom()
    {
        var today = DateTime.Today;
        
        // Test: 3 - later than today, earlier than booked period
        yield return [today.AddDays(3), today.AddDays(4), -1, false];
        yield return [today.AddDays(2), today.AddDays(2), -1, false];
        yield return [today.AddDays(2), today.AddDays(9), -1, false];
        
        // Test: 4 - from before booked period till after booked period
        yield return [today.AddDays(7), today.AddDays(23), -1, true];
        yield return [today.AddDays(9), today.AddDays(21), -1, true];
        yield return [today.AddDays(9), today.AddDays(21), -1, true];
        yield return [today.AddDays(2), today.AddDays(50), -1, true];
        
        // Test: 5 - period after booked period
        yield return [today.AddDays(21), today.AddDays(30), -1, false];
        yield return [today.AddDays(30), today.AddDays(40), -1, false];
        
        // Test: 6 - from before booked period till middle of booked period
        yield return [today.AddDays(8), today.AddDays(15), -1, true];
        yield return [today.AddDays(9), today.AddDays(11), -1, true];
        yield return [today.AddDays(5), today.AddDays(19), -1, true];
        
        // Test: 7 - between start and end of booked period
        yield return [today.AddDays(11), today.AddDays(15), -1, true];
        yield return [today.AddDays(11), today.AddDays(11), -1, true];
        yield return [today.AddDays(18), today.AddDays(19), -1, true];
        
        // Test: 8 - between start and end of booked period
        yield return [today.AddDays(19), today.AddDays(21), -1, true];
        yield return [today.AddDays(19), today.AddDays(22), -1, true];
        yield return [today.AddDays(15), today.AddDays(23), -1, true];
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
    
    // Test #3-8 - Booking starts after today where there is a booked period from +10 days to +20 days
    [Theory]
    [MemberData(nameof(GetBookingTestCasesFindingAvailableRoom))]
    public async Task FindAvailableRoom_BookingLaterThanTodayWithBookedPeriod_ReturnsExpectedRoomId(
        DateTime startDate, 
        DateTime endDate, 
        int expectedResult, 
        bool equals)
    {
        // Act
        int roomId = await bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        if (equals)
        {
            Assert.Equal(expectedResult, roomId);   
        }
        else
        {
            Assert.NotEqual(expectedResult, roomId);
        }
    }
        
        

}