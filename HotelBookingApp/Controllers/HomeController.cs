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

    public async Task<IActionResult> Index(string searchQuery, int page = 1, int pageSize = 9)
    {
        var model = await GetPaginatedHotels(searchQuery, page, pageSize);
        return View(model);
    }

    public async Task<IActionResult> LoadHotels(string searchQuery, int page = 1, int pageSize = 9)
    {
        var model = await GetPaginatedHotels(searchQuery, page, pageSize);
        return PartialView("_HotelListPartial", model);
    }

    private async Task<HotelListViewModel> GetPaginatedHotels(string searchQuery, int page, int pageSize)
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

        // Pagination
        var paginatedHotels = hotels.Skip((page - 1) * pageSize).Take(pageSize);
        var totalHotels = hotels.Count();
        var totalPages = (int)Math.Ceiling(totalHotels / (double)pageSize);

        return new HotelListViewModel
        {
            Hotels = paginatedHotels,
            CurrentPage = page,
            TotalPages = totalPages,
            SearchQuery = searchQuery
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
