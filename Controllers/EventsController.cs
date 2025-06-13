using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BlobService _blobService;

    public EventsController(ApplicationDbContext context, BlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    // GET: Events
    public async Task<IActionResult> Index(
        int? eventTypeId,
        int? venueId,
        DateTime? startDate,
        DateTime? endDate,
        bool onlyAvailable = false)
    {
        var query = _context.Events
            .Include(e => e.EventType)
            .Include(e => e.Bookings)
            .AsQueryable();

        if (eventTypeId.HasValue)
            query = query.Where(e => e.EventTypeId == eventTypeId.Value);

        if (venueId.HasValue)
            query = query.Where(e => e.Bookings.Any(b => b.VenueId == venueId.Value));

        if (startDate.HasValue)
            query = query.Where(e => e.StartDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EndDate <= endDate.Value);

        if (onlyAvailable)
        {
            // Filter events that have no bookings overlapping with the date range
            query = query.Where(e => !e.Bookings.Any()
                || !e.Bookings.Any(b =>
                    (startDate == null || b.EndDate >= startDate) &&
                    (endDate == null || b.StartDate <= endDate)));
        }

        var events = await query.ToListAsync();

        ViewBag.EventTypes = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", eventTypeId);
        ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "Name", venueId);
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.OnlyAvailable = onlyAvailable;

        return View(events);
    }

    // GET: Events/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var @event = await _context.Events.FindAsync(id);
        if (@event == null) return NotFound();

        return View(@event);
    }

    // GET: Events/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Events/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event @event, IFormFile? ImageFile)
    {
        if (ImageFile != null && ImageFile.Length > 0)
        {
            using var stream = ImageFile.OpenReadStream();
            var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            @event.ImageUrl = await _blobService.UploadFileAsync(stream, fileName, ImageFile.ContentType);
        }

        if (ModelState.IsValid)
        {
            _context.Add(@event);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(@event);
    }

    // GET: Events/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var @event = await _context.Events.FindAsync(id);
        if (@event == null) return NotFound();

        return View(@event);
    }

    // POST: Events/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event @event, IFormFile? ImageFile)
    {
        if (id != @event.Id) return NotFound();

        if (ImageFile != null && ImageFile.Length > 0)
        {
            // Delete old image from Blob if it exists
            if (!string.IsNullOrEmpty(@event.ImageUrl) && @event.ImageUrl.Contains("blob.core.windows.net"))
            {
                var oldFileName = Path.GetFileName(new Uri(@event.ImageUrl).LocalPath);
                await _blobService.DeleteFileAsync(oldFileName);
            }

            using var stream = ImageFile.OpenReadStream();
            var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            @event.ImageUrl = await _blobService.UploadFileAsync(stream, fileName, ImageFile.ContentType);
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        return View(@event);
    }

    // GET: Events/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var @event = await _context.Events.FindAsync(id);
        if (@event == null) return NotFound();

        bool hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == @event.Id);
        if (hasBookings)
        {
            TempData["Error"] = "Cannot delete event with active bookings.";
            return RedirectToAction(nameof(Index));
        }

        return View(@event);
    }

    // POST: Events/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event != null)
        {
            // Delete image from Blob Storage
            if (!string.IsNullOrEmpty(@event.ImageUrl) && @event.ImageUrl.Contains("blob.core.windows.net"))
            {
                var fileName = Path.GetFileName(new Uri(@event.ImageUrl).LocalPath);
                await _blobService.DeleteFileAsync(fileName);
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Event not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool EventExists(int id)
    {
        return _context.Events.Any(e => e.Id == id);
    }
}
