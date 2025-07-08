using System.Diagnostics;
using FoxholeMap.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoxholeMap.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Main()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            return View();
        }
    }
}