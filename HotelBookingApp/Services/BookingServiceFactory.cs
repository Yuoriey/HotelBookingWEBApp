using HotelBookingApp.Services;
using Microsoft.Extensions.Logging;
using System;

public class BookingServiceFactory : IBookingServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingServiceFactory> _logger;

    public BookingServiceFactory(IServiceProvider serviceProvider, ILogger<BookingServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IBookingService Create(string key)
    {
        _logger.LogInformation($"Attempting to create booking service with key: {key}");

        var service = key switch
        {
            "default" => _serviceProvider.GetService<IBookingService>(),
            _ => throw new ArgumentException($"No booking service found for key: {key}")
        };

        if (service == null)
        {
            _logger.LogError($"Failed to resolve booking service for key: {key}");
            throw new InvalidOperationException($"Failed to resolve booking service for key: {key}");
        }

        return service;
    }
}
