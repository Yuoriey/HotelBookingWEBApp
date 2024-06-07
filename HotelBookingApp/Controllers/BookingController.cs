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
using Microsoft.AspNetCore.Identity;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingServiceFactory _bookingServiceFactory;
    private readonly IBookingService _bookingService;
    private readonly IHotelService _hotelService;
    private readonly IConverter _pdfConverter;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(UserManager<ApplicationUser> userManager, IBookingServiceFactory bookingServiceFactory, IHotelService hotelService, IConverter pdfConverter, IBookingService bookingService)
    {
        _bookingServiceFactory = bookingServiceFactory;
        _bookingService = bookingService;
        _hotelService = hotelService;
        _pdfConverter = pdfConverter;
        _userManager = userManager;
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
            //Rooms = hotel.Rooms.Where(r => r.IsAvailable && r.AvailableRooms > 0).ToList(),
            Rooms = hotel.Rooms.ToList(),
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

            if (model.PaymentProof != null && model.PaymentProof.Length > 0)
            {
                using var stream = new MemoryStream();
                await model.PaymentProof.CopyToAsync(stream);
                ViewData["PaymentProofBase64"] = Convert.ToBase64String(stream.ToArray());
            }

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

                var user = await _userManager.GetUserAsync(User);
                var booking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    HotelId = model.Hotel.Id,
                    Hotel = model.Hotel,
                    UserId = user.Id,
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
                    PhoneNumber = model.PhoneNumber,
                };

                if (model.PaymentProof != null && model.PaymentProof.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await model.PaymentProof.CopyToAsync(stream);
                    booking.PaymentProof = stream.ToArray();
                }

                foreach (var room in model.SelectedRooms)
                {
                    var hotelRoom = model.Hotel.Rooms.FirstOrDefault(r => r.Type == room.Type);
                    if (hotelRoom != null && hotelRoom.AvailableRooms > 0)
                    {
                        hotelRoom.AvailableRooms -= 1;
                        if (hotelRoom.AvailableRooms == 0)
                        {
                            hotelRoom.IsAvailable = false;
                        }
                    }
                }

                await _hotelService.UpdateHotelAsync(model.Hotel);

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

        var booking = await _bookingService.GetBookingByIdAsync(id);
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
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            .container {{ width: 100%; max-width: 1200px; margin: 0 auto; }}
            .header, .section {{ margin-bottom: 20px; }}
            .header h1 {{ text-align: center; }}
            .section h5 {{ margin-bottom: 10px; }}
            .section p {{ margin: 5px 0; }}
            .section .label {{ font-weight: bold; }}
            .row {{ display: flex; flex-wrap: wrap; justify-content: space-between; }}
            .column {{ flex: 1; padding: 15px; box-sizing: border-box; }}
            .info-container {{ border: 1px solid #ddd; border-radius: 5px; margin-bottom: 20px; box-shadow: 0 0 5px rgba(0,0,0,0.1); }}
            ul {{ padding-left: 20px; }}
            img.img-fluid {{ max-width: 100%; height: auto; }}
            table {{ width: 100%; border-collapse: collapse; }}
            table, th, td {{ border: 1px solid black; }}
            th, td {{ padding: 10px; text-align: left; }}
            .left-column {{ width: 33%; }}
            .middle-column {{ width: 34%; }}
            .right-column {{ width: 33%; }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>Review Booking</h1>
            </div>
            <table>
                <tr>
                    <td class='left-column'>
                        <!-- Hotel Information -->
                        <div class='info-container'>
                            <h5>Hotel Information</h5>
                            <p><strong>Hotel Name:</strong> {booking.Hotel.Name}</p>
                            <p><strong>Location:</strong> {booking.Hotel.Location}</p>
                            <p><strong>Rating:</strong> {booking.Hotel.Rating}</p>
                        </div>
                        <!-- Selected Rooms -->
                        <div class='info-container'>
                            <h5>Selected Rooms</h5>
                            <ul>
                                {string.Join("", booking.Rooms.Select(r => $"<li>{r.Type} - ${r.Price}</li><li>Guests - {r.NumberOfGuests}</li>"))}
                            </ul>
                            <p><strong>Total Amount:</strong> ${booking.TotalAmount}</p>
                        </div>
                    </td>
                    <td class='middle-column'>
                        <!-- User Information -->
                        <div class='info-container'>
                            <h5>User Information</h5>
                            <p><strong>First Name:</strong> {booking.FirstName}</p>
                            <p><strong>Last Name:</strong> {booking.LastName}</p>
                            <p><strong>Email:</strong> {booking.Email}</p>
                            <p><strong>Phone Number:</strong> {booking.PhoneNumber}</p>
                            <p><strong>Address:</strong> {booking.Address}</p>
                            <p><strong>City:</strong> {booking.City}</p>
                            <p><strong>Zip Code:</strong> {booking.ZipCode}</p>
                            <p><strong>Country:</strong> {booking.Country}</p>
                        </div>
                        <!-- Booking Details -->
                        <div class='info-container'>
                            <h5>Booking Details</h5>
                            <p><strong>Check-in Date:</strong> {booking.CheckInDate.ToString("MM/dd/yyyy")}</p>
                            <p><strong>Check-out Date:</strong> {booking.CheckOutDate.ToString("MM/dd/yyyy")}</p>
                        </div>
                    </td>
                    <td class='right-column'>
                        <!-- Proof of Payment -->
                        <div class='info-container'>
                            <h5>Proof Of Payment</h5>
                            {(booking.PaymentProof != null ? $"<img src='data:image/jpeg;base64,{Convert.ToBase64String(booking.PaymentProof)}' alt='Payment Proof' class='img-fluid' />" : "<p>No proof of payment provided</p>")}
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </body>
</html>";

        var pdfDoc = new HtmlToPdfDocument
        {
            GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Landscape,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
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




    [Authorize]
    public async Task<IActionResult> UserBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        var bookings = await _bookingService.GetBookingsByUserAsync(user.Id);
        return View(bookings);
    }
    [Authorize]
    public async Task<IActionResult> BookingDetails(string id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }
        return View(booking);
    }

}
