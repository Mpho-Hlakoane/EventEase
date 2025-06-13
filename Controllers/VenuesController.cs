using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using EventEase.Data;
using Microsoft.EntityFrameworkCore;
using EventEase.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class VenuesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BlobService _blobService;

    public VenuesController(ApplicationDbContext context, BlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    // GET: Venues
    public async Task<IActionResult> Index(string? searchString)
    {
        var venuesQuery = _context.Venues.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            string search = searchString.Trim();

            venuesQuery = venuesQuery.Where(v =>
                EF.Functions.Like(v.Name, $"%{search}%") ||
                EF.Functions.Like(v.Location, $"%{search}%"));
        }

        var venues = await venuesQuery.ToListAsync();
        return View(venues);
    }

    // GET: Venues/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    // GET: Venues/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Venues/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Location,Capacity")] Venue venue, IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var stream = ImageFile.OpenReadStream();
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                venue.ImageUrl = await _blobService.UploadFileAsync(stream, fileName, ImageFile.ContentType);
            }
            else
            {
                venue.ImageUrl = "https://via.placeholder.com/150";
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(venue);
    }

    // GET: Venues/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    // POST: Venues/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("VenueId,Name,Location,Capacity,ImageUrl")] Venue venue, IFormFile? ImageFile)
    {
        if (venue == null) return BadRequest();
        if (id != venue.VenueId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Fetch existing venue from DB (no tracking to avoid EF conflicts)
                var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                if (existingVenue == null) return NotFound();

                // Handle new image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Delete old image from blob storage if applicable
                    if (!string.IsNullOrEmpty(existingVenue.ImageUrl) && existingVenue.ImageUrl.Contains("blob.core.windows.net"))
                    {
                        string oldFileName = Path.GetFileName(new Uri(existingVenue.ImageUrl).LocalPath);
                        await _blobService.DeleteFileAsync(oldFileName);
                    }

                    using var stream = ImageFile.OpenReadStream();
                    string newFileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    venue.ImageUrl = await _blobService.UploadFileAsync(stream, newFileName, ImageFile.ContentType);
                }
                else
                {
                    // Preserve the old image URL if no new file uploaded
                    venue.ImageUrl = existingVenue.ImageUrl;
                }

                _context.Update(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId))
                    return NotFound();

                TempData["Error"] = "Error updating venue due to concurrency conflict.";
                throw;
            }
            catch (Exception)
            {
                TempData["Error"] = "Unexpected error while updating venue.";
            }
        }

        return View(venue);
    }

    // GET: Venues/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return NotFound();

        bool hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == venue.VenueId);
        if (hasBookings)
        {
            TempData["Error"] = "Cannot delete venue with active bookings.";
            return RedirectToAction(nameof(Index));
        }

        return View(venue);
    }

    // POST: Venues/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue != null)
        {
            try
            {
                // Delete image from blob if applicable
                if (!string.IsNullOrEmpty(venue.ImageUrl) && venue.ImageUrl.Contains("blob.core.windows.net"))
                {
                    string fileName = Path.GetFileName(new Uri(venue.ImageUrl).LocalPath);
                    await _blobService.DeleteFileAsync(fileName);
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error deleting venue.";
            }
        }
        else
        {
            TempData["Error"] = "Venue not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool VenueExists(int id)
    {
        return _context.Venues.Any(v => v.VenueId == id);
    }
}
