using HotelBookingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingApp.Services
{
    public interface IBookingService
    {
        Task AddBookingAsync(Booking booking);
        Task<Booking> GetBookingByIdAsync(string id);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task UpdateBookingAsync(Booking booking);
        Task DeleteBookingAsync(string id);
        Task<IEnumerable<Booking>> GetBookingsByUserAsync(string userId);
    }
}
