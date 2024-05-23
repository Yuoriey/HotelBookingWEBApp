using HotelBookingApp.Services;
using System;

public class BookingServiceFactory : IBookingServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BookingServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBookingService Create(string key)
    {
        return key switch
        {
            "default" => _serviceProvider.GetService<BookingService>(),
            _ => throw new ArgumentException($"No booking service found for key: {key}")
        };
    }
}
