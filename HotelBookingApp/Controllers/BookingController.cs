using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelBookingApp.Models;
using HotelBookingApp.Services;
using Stripe;
using System.Threading.Tasks;
using System.Linq;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingServiceFactory _bookingServiceFactory;
    private readonly IHotelService _hotelService;

    public BookingController(IBookingServiceFactory bookingServiceFactory, IHotelService hotelService)
    {
        _bookingServiceFactory = bookingServiceFactory;
        _hotelService = hotelService;
    }

    public async Task<IActionResult> Create(string hotelId)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(hotelId);
        if (hotel == null)
        {
            return NotFound();
        }
        var model = new BookingViewModel
        {
            Hotel = hotel,
            Rooms = hotel.Rooms.Where(r => r.IsAvailable).ToList()
        };

        ViewBag.TotalAmount = 0;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookingViewModel model, List<string> selectedRooms)
    {
        if (ModelState.IsValid)
        {
            var bookingService = _bookingServiceFactory.Create("default");
            var totalAmount = model.Rooms
                .Where(r => selectedRooms.Contains(r.Type))
                .Sum(r => r.Price);

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                HotelId = model.Hotel.Id,
                UserId = User.Identity.Name,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                TotalAmount = (decimal)totalAmount
            };
            await bookingService.AddBookingAsync(booking);

            var options = new ChargeCreateOptions
            {
                Amount = (long)(totalAmount * 100),
                Currency = "usd",
                Source = model.StripeToken,
                Description = $"Booking for {model.Hotel.Name}"
            };

            var service = new ChargeService();
            Charge charge = await service.CreateAsync(options);

            if (charge.Status == "succeeded")
            {
                // Send confirmation email logic here...
                return RedirectToAction("Confirmation");
            }
            ModelState.AddModelError("", "Payment failed.");
        }

        ViewBag.TotalAmount = model.TotalAmount;
        return View(model);
    }

    public IActionResult Confirmation()
    {
        return View();
    }
}
