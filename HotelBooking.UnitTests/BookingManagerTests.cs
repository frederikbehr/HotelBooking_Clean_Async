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
    
    public BookingManagerTests()
    {
        DateTime today = DateTime.Today;
        DateTime start = today.AddDays(10);
        DateTime end = today.AddDays(20);
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
    
    public static IEnumerable<object[]> GetBookingTestCases()
    {
        var today = DateTime.Today;
        
        // Test: 3 - later than today, earlier than booked period
        yield return new object[] {today.AddDays(3), today.AddDays(4), -1, false};
        yield return new object[] {today.AddDays(2), today.AddDays(2), -1, false};
        yield return new object[] {today.AddDays(2), today.AddDays(9), -1, false};
        
        // Test: 4 - from before booked period till after booked period
        yield return new object[] {today.AddDays(7), today.AddDays(23), -1, true};
        yield return new object[] {today.AddDays(9), today.AddDays(21), -1, true};
        yield return new object[] {today.AddDays(9), today.AddDays(21), -1, true};
        yield return new object[] {today.AddDays(2), today.AddDays(50), -1, true};
        
        // Test: 5 - period after booked period
        yield return new object[] {today.AddDays(21), today.AddDays(30), -1, false};
        yield return new object[] {today.AddDays(30), today.AddDays(40), -1, false};
        
        // Test: 6 - from before booked period till middle of booked period
        yield return new object[] {today.AddDays(8), today.AddDays(15), -1, true};
        yield return new object[] {today.AddDays(9), today.AddDays(11), -1, true};
        yield return new object[] {today.AddDays(5), today.AddDays(19), -1, true};
        
        // Test: 7 - between start and end of booked period
        yield return new object[] {today.AddDays(11), today.AddDays(15), -1, true};
        yield return new object[] {today.AddDays(11), today.AddDays(11), -1, true};
        yield return new object[] {today.AddDays(18), today.AddDays(19), -1, true};
        
        // Test: 8 - between start and end of booked period
        yield return new object[] {today.AddDays(19), today.AddDays(21), -1, true};
        yield return new object[] {today.AddDays(19), today.AddDays(22), -1, true};
        yield return new object[] {today.AddDays(15), today.AddDays(23), -1, true};
        
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
    [MemberData(nameof(GetBookingTestCases))]
    public async Task FindAvailableRoom_BookingLaterThanTodayWithBookedPeriod_ReturnsExpectedRoomId(DateTime startDate, DateTime endDate, int expectedResult, bool equals)
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