using System.Diagnostics;
using FoxholeMap.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoxholeMap.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            //TODO: Make auth
            try
            {
                HttpContext.Session.SetString("Username", model.Username);
                return RedirectToAction("Main", "Main");
            }
            catch (Exception ex)
            {
                return Content($"Ошибка: {ex.Message}");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}