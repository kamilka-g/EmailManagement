using Microsoft.AspNetCore.Mvc;
using FormToSendAMail.Services;
using FormToSendAMail.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FormToSendAMail.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<HomeController> _logger;

        // Kolekcja u�ytkownik�w
        private static HashSet<User> _users = new HashSet<User>
        {
            new User
            {
                 Email = "kubap1402@gmail.com",
                FirstName = "Kuba",
                Username = "KochamKamileMatusik420",
                HasAgreed = false,
                CustomMessage = "Cze�� Kuba, oto Twoja spersonalizowana wiadomo��: Kamila Matusik przeprasza za to �e jest agresywna i niewychowana :))"

            },
            new User
            {
                Email = "kamilav7312@gmail.com",
                FirstName = "Kamila",
                Username = "kamila123",
                HasAgreed = false,
                CustomMessage = "Cze�� Kamila, oto Twoja spersonalizowana wiadomo��!"

            }
        };

        public HomeController(IEmailService emailService, ILogger<HomeController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Aplikacja zosta�a uruchomiona.");

            // Pobieramy u�ytkownika, kt�ry jeszcze nie wyrazi� zgody
            User user = _users.FirstOrDefault(u => u.HasAgreed == false);

            // Wys�anie spersonalizowanego e-maila, je�li u�ytkownik zosta� znaleziony
            if (user != null)
            {
                await _emailService.SendCustomEmailAsync(user);
            }

            return View();
        }

        // Akcja odpowiedzialna za zapisanie zgody u�ytkownika
        public IActionResult Agree(string username, bool agreement)
        {
            // Zmieniamy stan zgody u�ytkownika
            User user = _users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                user.HasAgreed = agreement; 
                _logger.LogInformation($"U�ytkownik: {user.Username}, Wyrazi� zgod�: {user.HasAgreed}");

            }
            _logger.LogInformation($"U�ytkownik {username} wyrazi� zgod�: {agreement}");

            // Mo�esz doda� jak�� informacj� zwrotn� u�ytkownikowi
            return Content(agreement ? "Zgoda zosta�a wyra�ona." : "Zgoda zosta�a odm�wiona.");
        }
    }
}
