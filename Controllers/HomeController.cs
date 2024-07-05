using ASP_P15.Models;
using ASP_P15.Models.Home;
using ASP_P15.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

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
            ViewData["hashCode"] = _hashService.GetHashCode();
            return View();
        }

        public IActionResult SignUp()
        {
            SignUpPageModel model = new();

            // На початку перевіряємо чи є збережена сесія 
            if(HttpContext.Session.Keys.Contains("signup-data"))
            {
                // є дані - це редирект, обробляємо дані
                var formModel = JsonSerializer.Deserialize<SignUpFormModel>(
                    HttpContext.Session.GetString("signup-data")!)!;
                model.FormModel = formModel;

                ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";
                
                // Видаляємо дані з сесії, щоб уникнути повторного оброблення
                HttpContext.Session.Remove("signup-data");
            }
            return View(model);
        }

        public IActionResult Demo([FromQuery(Name="user-email")] String userEmail, [FromQuery(Name = "user-name")] String userName)
        {
            /* Прийом даних від форми, варіант 1: через параметри action 
             * Зв'язування автоматично відбувається за збігом імен
             * <input name="userName"/> ------ Demo(String userName)
             * якщо в HTML використовуються імена, які неможливі у C#
             * (user-name), то додається атрибут [From...] із зазначенням імені
             * перед потрібним параметром
             * 
             * Варіант 1 використовується коли кількість параметрів невелика (1-2)
             * Більш рекомендований спосіб - використання моделей
             */
            ViewData["data"] = $"email: {userEmail}, name: {userName}";
            return View();
        }

        public IActionResult RegUser(SignUpFormModel formModel)
        {
            HttpContext.Session.SetString("signup-data", 
                JsonSerializer.Serialize(formModel));
            
            return RedirectToAction(nameof(SignUp));

            // ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";
            // return View("Demo");
            /* Проблема: якщо сторінка побудована через передачу форми, то
             * її оновлення у браузері
             * а) видає повідомлення, на яке ми не впливаємо
             * б) повторно передає дані форми, що може призвести до 
             *     дублювання даних у БД, файлів, тощо
             * Рішення: "скидання даних" - переадресація відповіді із 
             *  запам'ятовуванням даних
             *  
             *  Client(Browser)                    Server(ASP)
             *  [form]--------- POST RegUser --------->  [form]---Session
             *  <-------------- 302 SignUp -----------             |
             *  --------------- GET SignUp ----------->            |
             *   <-----------------HTML----------------------- оброблення
             *   
             * Включення та налаштування сесій - див. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state  
             */
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
