# Hotel Booking Clean Async
Hotel booking site, where people can book hotel rooms for a give period of time if it hasn't already been booked.

## Application requirements
- Customer can book a room for a duration
- Room is marked as occupied after check-in
- After check-out the room is marked as free
- Customer can cancel or change a reservation before check-in.
- Website should show which dates a room is available.
- Website needs a calendar showing this month's availability

# Part 1
To make the calendar, a
method which finds and returns the fully occupied dates for a given period, is required.

### Already developed:
- Hotel rooms can be booked for a period in the future - if not taken in that period.
- It is easy for the user to find out if rooms are available during a given
  period.

### Task
Implement different types of testing, covering the two features above.

### Requirements
- Write unit tests for all business logic inside the HotelBooking.Core project. 
  - Demonstrate use of data-driven unit testing and of a mocking framework (e.g. Moq). 
  - You should use the basic naming conventions introduced in the course.
####
- Improve the design of the minimal implementation to maximize its testability (this may be hard to do).

### Presentation
Each team demonstrates their solution to this part during Week 9, and will be announced the week before.

#### Demonstration
- Explain how you have implemented unit testing of the core business logic.
- Min. 1 example of data-driven unit testing 
- Min. 1 example of the use of a mocking framework. 
- Each presentation can last up to 10 minutes.