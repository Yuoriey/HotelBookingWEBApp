using HotelBookingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBookingApp.Services
{
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string query);
        Task<Hotel> GetHotelByIdAsync(string id);
        Task AddHotelAsync(Hotel hotel);
        Task UpdateHotelAsync(Hotel hotel);
        Task DeleteHotelAsync(string id);
        Task SetHotelStatusAsync(string id, bool isActive);
        Task<Room> GetRoomByTypeAsync(string hotelId, string roomType);

    }
}
