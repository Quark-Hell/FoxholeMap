using System.Diagnostics;
using FoxholeMap.DataBase;
using FoxholeMap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoxholeMap.Controllers
{
    public class LoginController : Controller
    {
        private readonly UsersDbContext _db;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger, UsersDbContext db)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                
                bool passwordValid = 
                    (user != null) && 
                    (BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash));
                
                if (passwordValid)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    return RedirectToAction("Main", "Main");
                }
                
                ModelState.AddModelError("", "Неверный логин или пароль");
                return RedirectToAction("Index", "Login");
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