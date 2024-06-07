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
    public async Task<IActionResult> Create(Hotel hotel, IFormFile hotelPicture, List<IFormFile> roomPictures, List<string> roomTypes, List<double> roomPrices, List<int> availableRooms, List<int> numberOfGuests)
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
                NumberOfGuests = numberOfGuests[i],
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

    [HttpGet]
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
    public async Task<IActionResult> Edit(
        Hotel hotel,
        List<string> roomTypes,
        List<double> roomPrices,
        List<int> availableRooms,
        List<int> numberOfGuests,
        List<IFormFile> newRoomPictures,
        List<string> newRoomTypes,
        List<double> newRoomPrices,
        List<int> newAvailableRooms,
        List<int> newNumberOfGuests)
    {
        if (ModelState.IsValid)
        {
            var existingHotel = await _hotelService.GetHotelByIdAsync(hotel.Id);
            if (existingHotel == null)
            {
                return NotFound();
            }

            // Update hotel details
            existingHotel.Name = hotel.Name;
            existingHotel.Location = hotel.Location;
            existingHotel.Rating = hotel.Rating;

            // Update existing rooms
            for (int i = 0; i < existingHotel.Rooms.Count; i++)
            {
                existingHotel.Rooms[i].Type = roomTypes[i];
                existingHotel.Rooms[i].Price = roomPrices[i];
                existingHotel.Rooms[i].AvailableRooms = availableRooms[i];
                existingHotel.Rooms[i].NumberOfGuests = numberOfGuests[i];
            }

            // Add new rooms
            for (int i = 0; i < newRoomTypes.Count; i++)
            {
                var room = new Room
                {
                    Type = newRoomTypes[i],
                    Price = newRoomPrices[i],
                    AvailableRooms = newAvailableRooms[i],
                    NumberOfGuests = newNumberOfGuests[i],
                    IsAvailable = true
                };

                if (i < newRoomPictures.Count && newRoomPictures[i] != null && newRoomPictures[i].Length > 0)
                {
                    using var stream = new MemoryStream();
                    await newRoomPictures[i].CopyToAsync(stream);
                    room.Picture = Convert.ToBase64String(stream.ToArray());
                }

                existingHotel.Rooms.Add(room);
            }

            await _hotelService.UpdateHotelAsync(existingHotel);
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
