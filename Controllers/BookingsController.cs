using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Bookings
    public async Task<IActionResult> Index(string? searchString)
    {
        ViewData["CurrentFilter"] = searchString;

        var bookings = _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            bookings = bookings.Where(b =>
                EF.Functions.Like(b.BookingReference, $"%{searchString}%") ||
                (b.Event != null && EF.Functions.Like(b.Event.Name, $"%{searchString}%")) ||
                (b.Venue != null && EF.Functions.Like(b.Venue.Name, $"%{searchString}%")));
        }

        return View(await bookings.OrderByDescending(b => b.StartDate).ToListAsync());
    }

    // GET: Bookings/Create
    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("VenueId,EventId,StartDate,EndDate,BookingReference")] Booking booking)
    {
        if (booking.StartDate >= booking.EndDate)
        {
            ModelState.AddModelError("", "Start date must be before end date.");
        }

        bool conflict = await _context.Bookings.AnyAsync(b =>
            b.VenueId == booking.VenueId &&
            b.StartDate < booking.EndDate &&
            booking.StartDate < b.EndDate);

        if (conflict)
        {
            ModelState.AddModelError("", "This venue is already booked during the selected time.");
        }

        if (ModelState.IsValid)
        {
            booking.BookingReference = $"BKG-{DateTime.UtcNow.Ticks.ToString()[^6..]}";
            _context.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking created successfully.";
            return RedirectToAction(nameof(Index));
        }

        PopulateDropdowns(booking.VenueId, booking.EventId);
        return View(booking);
    }

    // GET: Bookings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        PopulateDropdowns(booking.VenueId, booking.EventId);
        return View(booking);
    }

    // POST: Bookings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,EventId,StartDate,EndDate,BookingReference")] Booking booking)
    {
        if (id != booking.Id) return NotFound();

        if (booking.StartDate >= booking.EndDate)
        {
            ModelState.AddModelError("", "Start date must be before end date.");
        }

        bool conflict = await _context.Bookings.AnyAsync(b =>
            b.Id != booking.Id &&
            b.VenueId == booking.VenueId &&
            b.StartDate < booking.EndDate &&
            booking.StartDate < b.EndDate);

        if (conflict)
        {
            ModelState.AddModelError("", "This venue is already booked during the selected time.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bookings.Any(e => e.Id == booking.Id))
                    return NotFound();

                throw;
            }
        }

        PopulateDropdowns(booking.VenueId, booking.EventId);
        return View(booking);
    }

    // GET: Bookings/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

    // POST: Bookings/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking deleted.";
        }
        else
        {
            TempData["Error"] = "Booking not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Bookings/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Event)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

    // Reusable dropdown population
    private void PopulateDropdowns(int? selectedVenueId = null, int? selectedEventId = null)
    {
        ViewData["VenueId"] = new SelectList(_context.Venues.OrderBy(v => v.Name), "Id", "Name", selectedVenueId);

        ViewData["EventId"] = new SelectList(_context.Events
            .OrderBy(e => e.StartDate)
            .Select(e => new
            {
                e.Id,
                Name = $"{e.Name} ({e.StartDate:yyyy-MM-dd} - {e.EndDate:yyyy-MM-dd})"
            }), "Id", "Name", selectedEventId);
    }
}
