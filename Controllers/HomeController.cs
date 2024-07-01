using ASP_P15.Models;
using ASP_P15.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_P15.Controllers
{
    public class HomeController : Controller
    {
        // приклад інжекції - _logger
        private readonly ILogger<HomeController> _logger;
        // інжектуємо наш (хеш-) сервіс
        private readonly IHashService _hashService;

        public HomeController(ILogger<HomeController> logger, IHashService hashService)
        {
            _logger = logger;
            _hashService = hashService;
            /* Інжекція через конструктор - найбільш рекомендований варіант. 
               Контейнер служб (інжектор) аналізує параметри конструктора і сам 
               підставляє до нього необхідні об'єкти (інстанси) служб
            */
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult Intro()
        {
            return View();
        }
        
        public IActionResult Razor()
        {
            return View();
        }
        public IActionResult Ioc()
        {
            ViewData["hash"] = _hashService.Digest("123");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
