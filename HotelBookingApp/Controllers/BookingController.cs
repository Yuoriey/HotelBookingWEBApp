using HotelBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using HotelBookingApp.Services;
using Newtonsoft.Json;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingServiceFactory _bookingServiceFactory;
    private readonly IBookingService _bookingService;
    private readonly IHotelService _hotelService;
    private readonly IConverter _pdfConverter;

    public BookingController(IBookingServiceFactory bookingServiceFactory, IHotelService hotelService, IConverter pdfConverter, IBookingService bookingService)
    {
        _bookingServiceFactory = bookingServiceFactory;
        _bookingService = bookingService;
        _hotelService = hotelService;
        _pdfConverter = pdfConverter;
    }

    public async Task<IActionResult> Create(string hotelId)
    {
        if (string.IsNullOrEmpty(hotelId))
        {
            return BadRequest("Hotel ID is required.");
        }
        var bookingService = _bookingServiceFactory.Create("default");
        var hotel = await _hotelService.GetHotelByIdAsync(hotelId);
        if (hotel == null)
        {
            return NotFound();
        }
        var model = new BookingViewModel
        {
            Hotel = hotel,
            HotelId = hotel.Id,
            Rooms = hotel.Rooms.Where(r => r.IsAvailable).ToList(),
            SelectedRooms = new List<Room>()
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult UserInfo(BookingViewModel model)
    {
        //if (ModelState.IsValid)
        {
            return PartialView("_UserInfoModal", model);
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        Console.WriteLine("ModelState errors: " + string.Join(", ", errors));

        return BadRequest(ModelState); // Return bad request if model state is invalid
    }

    [HttpPost]
    public async Task<IActionResult> ReviewBooking(BookingViewModel model, string hotelJson, List<string> selectedRoomTypes)
    {
        // Ensure the hotelJson is not null or empty
        if (string.IsNullOrEmpty(hotelJson))
        {
            ModelState.AddModelError("Hotel", "Hotel information is missing.");
            return BadRequest(ModelState);
        }

        // Deserialize hotelJson to Hotel object    
        model.Hotel = JsonConvert.DeserializeObject<Hotel>(hotelJson);

        // Check if model.Hotel is null after deserialization
        if (model.Hotel == null)
        {
            ModelState.AddModelError("Hotel", "Failed to deserialize hotel information.");
            return BadRequest(ModelState);
        }

        if (ModelState.IsValid)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(model.HotelId);
            if (hotel == null)
            {
                ModelState.AddModelError("", "Hotel not found.");
                return BadRequest(ModelState);
            }

            model.SelectedRooms = hotel.Rooms.Where(r => selectedRoomTypes.Contains(r.Type)).ToList();

            return PartialView("_ReviewBookingModal", model);
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return BadRequest(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmBooking(BookingViewModel model, List<string> selectedRoomTypes, string hotelJson)
    {
        try
        {
            // Ensure the hotelJson is not null or empty
            if (string.IsNullOrEmpty(hotelJson))
            {
                ModelState.AddModelError("Hotel", "Hotel information is missing.");
                return BadRequest(ModelState);
            }

            // Deserialize hotelJson to Hotel object
            model.Hotel = JsonConvert.DeserializeObject<Hotel>(hotelJson);

            // Check if model.Hotel is null after deserialization
            if (model.Hotel == null)
            {
                ModelState.AddModelError("Hotel", "Failed to deserialize hotel information.");
                return BadRequest(ModelState);
            }

            // Ensure that selectedRoomTypes is not null or empty
            if (selectedRoomTypes == null || !selectedRoomTypes.Any())
            {
                ModelState.AddModelError("Rooms", "No rooms selected.");
                return BadRequest(ModelState);
            }

            if (ModelState.IsValid)
            {
                model.SelectedRooms = model.Hotel.Rooms.Where(r => selectedRoomTypes.Contains(r.Type)).ToList();

                var booking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    HotelId = model.Hotel.Id,
                    Hotel = model.Hotel,
                    UserId = User.Identity.Name,
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    TotalAmount = model.TotalAmount,
                    Rooms = model.SelectedRooms,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Address = model.Address,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    PhoneNumber = model.PhoneNumber
                };

                var bookingService = _bookingServiceFactory.Create("default");
                if (bookingService == null)
                {
                    ModelState.AddModelError("", "Booking service could not be resolved.");
                    return BadRequest(ModelState);
                }

                await bookingService.AddBookingAsync(booking);

                // Generate a unique reservation number
                var reservationNumber = GenerateReservationNumber();
                // Send confirmation email (implementation needed)

                return RedirectToAction("Index", new { success = true });
            }

            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            // Log the exception (add a logger to your controller for better logging)
            Console.WriteLine("Error in ConfirmBooking: " + ex.Message);
            ModelState.AddModelError("", "An unexpected error occurred.");
            return BadRequest(ModelState);
        }
    }




    private string GenerateReservationNumber()
    {
        return DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
    }

    public async Task<IActionResult> Index()
    {
        var bookingService = _bookingServiceFactory.Create("default");
        var bookings = await bookingService.GetAllBookingsAsync();
        return View(bookings);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Booking ID is required.");
        }

        var bookingService = _bookingServiceFactory.Create("default");
        var booking = await bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }
        return View(booking);
    }

    public async Task<IActionResult> Download(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Booking ID is required.");
        }

        var bookingService = _bookingServiceFactory.Create("default");
        var booking = await bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        // Generate PDF logic here
        var pdfContent = GeneratePdf(booking);
        var fileName = $"Booking_{booking.Id}.pdf";
        return File(pdfContent, "application/pdf", fileName);
    }

    private byte[] GeneratePdf(Booking booking)
    {
        var html = $@"
        <html>
            <head>
                <title>Booking Details</title>
            </head>
            <body>
                <h1>Booking Details</h1>
                <p><strong>Reservation Number:</strong> {booking.Id}</p>
                <p><strong>Hotel Name:</strong> {booking.HotelId}</p>
                <p><strong>Check-in Date:</strong> {booking.CheckInDate.ToString("MM/dd/yyyy")}</p>
                <p><strong>Check-out Date:</strong> {booking.CheckOutDate.ToString("MM/dd/yyyy")}</p>
                <p><strong>Total Amount:</strong> {booking.TotalAmount.ToString("C")}</p>
                <h2>User Information</h2>
                <p><strong>First Name:</strong> {booking.FirstName}</p>
                <p><strong>Last Name:</strong> {booking.LastName}</p>
                <p><strong>Email:</strong> {booking.Email}</p>
                <p><strong>Address:</strong> {booking.Address}</p>
                <p><strong>City:</strong> {booking.City}</p>
                <p><strong>Zip Code:</strong> {booking.ZipCode}</p>
                <p><strong>Country:</strong> {booking.Country}</p>
                <p><strong>Phone Number:</strong> {booking.PhoneNumber}</p>
                <h2>Selected Rooms</h2>
                <ul>
                    {string.Join("", booking.Rooms.Select(r => $"<li>{r.Type} - ${r.Price}</li>"))}
                </ul>
            </body>
        </html>";

        var pdfDoc = new HtmlToPdfDocument
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true }
                }
            }
        };

        return _pdfConverter.Convert(pdfDoc);
    }
}
