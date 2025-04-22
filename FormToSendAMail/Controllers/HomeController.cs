using Microsoft.AspNetCore.Mvc;
using FormToSendAMail.Services;
using FormToSendAMail.Models;

namespace FormToSendAMail.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;

        public HomeController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var user = new UserInfo
            {
                Email = "email@example.com", // Please put here an email of an user
                Username = "Kamila",
                CustomMessage = "Cze�� Kamila, oto Twoja spersonalizowana wiadomo��!"
            };
            var user1 = new UserInfo
            {
                Email = "", // Please put here an email of an user
                Username = "Kuba",
                CustomMessage = "Cze�� Kuba, oto Twoja spersonalizowana wiadomo��!"
            };

            await _emailService.SendCustomEmailAsync(user); //choose an user to send a message
            return View();
        }

    }
}
