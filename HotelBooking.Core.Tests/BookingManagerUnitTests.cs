using Moq;
using HotelBooking.Core;

public class BookingManagerTests
{
    private readonly Mock<IRepository<Booking>> mockBookingRepo;
    private readonly Mock<IRepository<Room>> mockRoomRepo;
    private readonly BookingManager bookingManager;

    public BookingManagerTests()
    {
        // Create mock repositories
        mockBookingRepo = new Mock<IRepository<Booking>>();
        mockRoomRepo = new Mock<IRepository<Room>>();

        // Inject mocks into BookingManager
        bookingManager = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
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
        var roomId = await bookingManager.FindAvailableRoom(DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));
        
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
        var roomId = await bookingManager.FindAvailableRoom(DateTime.Today.AddDays(2), DateTime.Today.AddDays(4));
        
        // Assert (Check result)
        Assert.True(roomId == -1, "Expected an available room ID");
    }
}