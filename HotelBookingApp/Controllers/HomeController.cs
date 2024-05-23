using HotelBookingApp.Models;
using HotelBookingApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly IHotelService _hotelService;

    public HomeController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    public async Task<IActionResult> Index(string searchQuery)
    {
        IEnumerable<Hotel> hotels;

        if (string.IsNullOrEmpty(searchQuery))
        {
            hotels = await _hotelService.GetAllHotelsAsync();
        }
        else
        {
            hotels = await _hotelService.SearchHotelsAsync(searchQuery);
        }

        // Filter active hotels
        hotels = hotels.Where(h => h.IsActive);

        return View(hotels);
    }

    [HttpPost]
    public async Task<IActionResult> Search(string searchQuery)
    {
        var hotels = await _hotelService.SearchHotelsAsync(searchQuery);

        // Filter active hotels
        hotels = hotels.Where(h => h.IsActive);

        return PartialView("_HotelListPartial", hotels);
    }
}
