using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        // Konstruktor kontrolera z wstrzyknięciem UserManager
        public AccountController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // Akcja do rejestracji użytkownika
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            // Sprawdź, czy podano dane
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Email and password are required.");
            }

            // Utwórz nowego użytkownika
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            // Sprawdź, czy rejestracja się powiodła
            if (result.Succeeded)
            {
                // Przypisz użytkownika do roli 'Participant'
                await _userManager.AddToRoleAsync(user, "Participant");

                return Ok("User registered successfully.");
            }

            // Zwróć błędy, jeśli rejestracja się nie powiodła
            return BadRequest(result.Errors);
        }

        // GET: Formularz przypisywania roli organizatora
        [HttpGet]
        public IActionResult AssignOrganizerRoleForm()
        {
            return View();
        }

        // POST: Przypisanie roli organizatora
        [HttpPost]
        public async Task<IActionResult> AssignOrganizerRoleForm(string email)
        {
            // Znajdź użytkownika na podstawie emaila
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found.";
                return View();
            }

            // Sprawdź, czy użytkownik już ma rolę 'Organizer'
            if (await _userManager.IsInRoleAsync(user, "Organizer"))
            {
                ViewBag.ErrorMessage = "User is already an Organizer.";
                return View();
            }

            // Przypisz użytkownika do roli 'Organizer'
            await _userManager.AddToRoleAsync(user, "Organizer");

            ViewBag.SuccessMessage = $"User {email} was successfully assigned to the Organizer role.";
            return View();
        }
    }
}
