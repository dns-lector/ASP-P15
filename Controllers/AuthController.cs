using ASP_P15.Data;
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
                return new
                {
                    status = "Ok",
                    code = 200,
                    message = "Authenticated"
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
 *   Д.З. Реалізувати можливість входу у систему за допомогою 
 *   паролю та одного з інших параметрів: дата народження, ім'я, e-mail.
 *   Визначати тип параметра автоматично.
 */
