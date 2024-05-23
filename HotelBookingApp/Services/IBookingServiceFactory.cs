using HotelBookingApp.Services;

public interface IBookingServiceFactory
{
    IBookingService Create(string key);
}
