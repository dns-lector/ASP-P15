using ASP_P15.Data;
using ASP_P15.Data.Entities;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P15.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;

        public AuthController(DataContext dataContext, IKdfService kdfService)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
        }

        [HttpGet]
        public object DoGet(String email, String password)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
            {
                return new
                {
                    status = "Error",
                    code = 400,
                    message = "Email and password must not be empty"
                };
            }
            // Розшифрувати DK неможливо, тому повторюємо розрахунок DK з сіллю, що
            // зберігається у користувача, та паролем, який передано у параметрі
            
            var user = _dataContext.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && _kdfService.DerivedKey(password, user.Salt) == user.Dk)
            {
                // генеруємо токен
                Token token = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ExpiresAt = DateTime.Now.AddMinutes(2),
                };
                _dataContext.Tokens.Add(token);
                _dataContext.SaveChanges();
                // зберігаємо токен у сесії
                HttpContext.Session.SetString("token", token.Id.ToString());
                return new
                {
                    status = "Ok",
                    code = 200,
                    message = token.Id  // передаємо токен клієнту
                };
            }
            else
            {
                return new
                {
                    status = "Reject",
                    code = 401,
                    message = "Credentials rejected"
                };
            }            
        }
    }
}
/*
 * Контролери розрізняють: MVC та API
 * MVC - різні адреси ведуть на різні дії (actions)
 *    /Home/Index -> Index()
 *    /Home/Db    -> Db()
 *    
 * API - різні методи запиту ведуть на різні дії
 *   GET  /api/auth  -> DoGet()
 *   POST /api/auth  -> DoPost()
 *   PUT  /api/auth  -> DoPut()
 *   
 *   
 * Токени авторизації  
 * Токен - "жетон", "перепустка" - дані, що видаються як результат
 * автентифікації і далі використовуються для "підтвердження особи" -
 * авторизації.
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   
 *   Д.З. Реалізувати перевірку токена при автентифікації
 *   - перед тим як згенерувати новий токен слід перевірити чи є
 *      для даного користувача активний токен (не протермінований)
 *   - якщо є, то видати цей токен
 *   - якщо немає, то генерувати новий
 */
