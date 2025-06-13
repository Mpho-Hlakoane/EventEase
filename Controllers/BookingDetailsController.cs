using EventEase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace EventEase.Controllers
{
    public class BookingDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BookingDetails
        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var query = _context.BookingDetailsViews.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.EventName.Contains(searchString));
            }

            var bookingDetails = await query.ToListAsync();

            return View(bookingDetails);
        }
    }
}


