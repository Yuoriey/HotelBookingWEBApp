using HotelBookingApp.Models;
using HotelBookingApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class HotelController : Controller
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelController> _logger;

    public HotelController(IHotelService hotelService, ILogger<HotelController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var hotels = await _hotelService.GetAllHotelsAsync();
        return View(hotels);
    }

    public async Task<IActionResult> Details(string id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return View(hotel);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Hotel hotel, IFormFile hotelPicture, List<IFormFile> roomPictures, List<string> roomTypes, List<double> roomPrices, List<int> availableRooms)
    {
        hotel.IsActive = true;
        hotel.Id = Guid.NewGuid().ToString();

        if (hotelPicture != null && hotelPicture.Length > 0)
        {
            using var stream = new MemoryStream();
            await hotelPicture.CopyToAsync(stream);
            hotel.Picture = Convert.ToBase64String(stream.ToArray());
        }

        for (int i = 0; i < roomTypes.Count; i++)
        {
            var room = new Room
            {
                Type = roomTypes[i],
                Price = roomPrices[i],
                AvailableRooms = availableRooms[i],
                IsAvailable = true
            };

            if (i < roomPictures.Count && roomPictures[i] != null && roomPictures[i].Length > 0)
            {
                using var stream = new MemoryStream();
                await roomPictures[i].CopyToAsync(stream);
                room.Picture = Convert.ToBase64String(stream.ToArray());
            }

            hotel.Rooms.Add(room);
        }

        await _hotelService.AddHotelAsync(hotel);
        _logger.LogInformation("Hotel created successfully.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return View(hotel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Hotel hotel, IFormFile hotelPicture, List<IFormFile> roomPictures, List<string> roomTypes, List<double> roomPrices, List<int> availableRooms, List<bool> roomAvailabilities)
    {
        if (ModelState.IsValid)
        {
            if (hotelPicture != null && hotelPicture.Length > 0)
            {
                using var stream = new MemoryStream();
                await hotelPicture.CopyToAsync(stream);
                hotel.Picture = Convert.ToBase64String(stream.ToArray());
            }

            hotel.Rooms.Clear();

            for (int i = 0; i < roomTypes.Count; i++)
            {
                var room = new Room
                {
                    Type = roomTypes[i],
                    Price = roomPrices[i],
                    AvailableRooms = availableRooms[i],
                    IsAvailable = roomAvailabilities[i]
                };

                if (i < roomPictures.Count && roomPictures[i] != null && roomPictures[i].Length > 0)
                {
                    using var stream = new MemoryStream();
                    await roomPictures[i].CopyToAsync(stream);
                    room.Picture = Convert.ToBase64String(stream.ToArray());
                }

                hotel.Rooms.Add(room);
            }

            await _hotelService.UpdateHotelAsync(hotel);
            return RedirectToAction(nameof(Index));
        }
        return View(hotel);
    }

    public async Task<IActionResult> Delete(string id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return View(hotel);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _hotelService.DeleteHotelAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetStatus(string id, bool isActive)
    {
        await _hotelService.SetHotelStatusAsync(id, isActive);
        return RedirectToAction(nameof(Index));
    }
}
