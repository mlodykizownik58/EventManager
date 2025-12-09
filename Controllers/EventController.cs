using System.Security.Claims;
using EventManagement.Data;
using EventManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EventManagement.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EventController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Participants List for an Event
        public async Task<IActionResult> Participants(int eventId)
        {
            var eventItem = await _context.Events
                .Include(e => e.EventSignups)
                .ThenInclude(es => es.User)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Sprawdzamy, czy użytkownik jest organizatorem wydarzenia
            if (eventItem.OrganizerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            var participants = eventItem.EventSignups.Select(es => es.User).ToList();

            return View(participants);
        }

        // GET: Create Event Form
        public IActionResult Create() => View();

        // POST: Create Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel, IFormFile? imageUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);

                    // Sprawdzanie, czy rola "Organizer" istnieje
                    var roleExists = await _roleManager.RoleExistsAsync("Organizer");
                    if (!roleExists)
                    {
                        var role = new IdentityRole("Organizer");
                        await _roleManager.CreateAsync(role);
                    }

                    if (user != null && !await _userManager.IsInRoleAsync(user, "Organizer"))
                    {
                        await _userManager.AddToRoleAsync(user, "Organizer");
                    }

                    eventModel.OrganizerId = user?.Id;
                    eventModel.EventSignups = new List<EventSignup>();

                    // Obsługuje przesyłanie obrazu (opcjonalnie)
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(imageUrl.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            TempData["ErrorMessage"] = "Invalid image format. Allowed formats: .jpg, .jpeg, .png, .gif.";
                            return View(eventModel);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageUrl.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageUrl.CopyToAsync(stream);
                        }

                        eventModel.ImageUrl = "/images/" + uniqueFileName;
                    }

                    _context.Add(eventModel);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "There was an error creating the event. Please try again.";
                    Console.WriteLine(ex.Message);
                }
            }
            return View(eventModel);
        }

        // GET: List of events
        [AllowAnonymous]
        public IActionResult Index()
        {
            var events = _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.EventSignups)
                .ToList();
            return View(events);
        }

        // GET: Edit Event Form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var eventToEdit = await _context.Events.FindAsync(id);
            if (eventToEdit == null)
            {
                return NotFound();
            }

            // Check if current user is the organizer
            if (eventToEdit.OrganizerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return View(eventToEdit);
        }

        // POST: Update Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event model, IFormFile? imageUrl)
        {
            var eventToEdit = await _context.Events.FindAsync(id);
            if (eventToEdit == null)
            {
                return NotFound();
            }

            // Check if current user is the organizer
            if (eventToEdit.OrganizerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            // Update fields
            eventToEdit.Name = model.Name;
            eventToEdit.Description = model.Description;
            eventToEdit.Date = model.Date;
            eventToEdit.Location = model.Location;

            // Obsługuje przesyłanie nowego obrazu, jeśli został dostarczony
            if (imageUrl != null && imageUrl.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageUrl.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Invalid image format. Allowed formats: .jpg, .jpeg, .png, .gif.";
                    return View(eventToEdit);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageUrl.CopyToAsync(stream);
                }

                eventToEdit.ImageUrl = "/images/" + uniqueFileName; // Aktualizacja ścieżki obrazu
            }

            _context.Update(eventToEdit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete == null)
            {
                return NotFound();
            }

            // Check if current user is the organizer
            if (eventToDelete.OrganizerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            var eventSignups = await _context.EventSignups
                .Where(es => es.EventId == id)
                .ToListAsync();

            _context.EventSignups.RemoveRange(eventSignups);

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Sign up for an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to sign up for an event.";
                return RedirectToAction("Index");
            }

            var eventItem = await _context.Events
                .Include(e => e.EventSignups)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("Index");
            }

            if (eventItem.EventSignups.Any(es => es.UserId == userId))
            {
                TempData["ErrorMessage"] = "You are already signed up for this event.";
                return RedirectToAction("Index");
            }

            var signup = new EventSignup
            {
                EventId = eventId,
                UserId = userId
            };
            _context.EventSignups.Add(signup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "You have successfully signed up for the event.";
            return RedirectToAction("Index");
        }

        // POST: Cancel signup for an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSignup(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);
            var eventSignup = await _context.EventSignups
                .FirstOrDefaultAsync(es => es.EventId == eventId && es.UserId == user.Id);

            if (eventSignup == null)
            {
                TempData["ErrorMessage"] = "You are not signed up for this event.";
                return RedirectToAction(nameof(Index));
            }

            _context.EventSignups.Remove(eventSignup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "You have canceled your signup for the event.";
            return RedirectToAction(nameof(Index));
        }
    }
}
