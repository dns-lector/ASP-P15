using ASP_P15.Data;
using ASP_P15.Data.Entities;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ASP_P15.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(DataContext dataContext, IKdfService kdfService, ILogger<AuthController> logger)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
            _logger = logger;
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
                    ExpiresAt = DateTime.Now.AddHours(3),
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

        [HttpDelete]
        public object DoDelete()
        {
            HttpContext.Session.Remove("token");
            return "Ok";
        }

        [HttpPut]
        public async Task<object> DoPutAsync()
        {
            // Дані, що передаються в тілі запиту доступні через Request.Body
            String body = await new StreamReader(Request.Body).ReadToEndAsync();

            _logger.LogWarning(body);

            JsonNode json = JsonSerializer.Deserialize<JsonNode>(body)
                ?? throw new Exception("JSON in body is invalid");

            String? email = json["email"]?.GetValue<String>();
            String? name = json["name"]?.GetValue<String>();
            String? birthdate = json["birthdate"]?.GetValue<String>();

            if (email == null && name == null && birthdate == null)
            {
                return new { code = 400, status = "Error", message = "No data" };
            }
            if (email != null) 
            {
                var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                if ( ! emailRegex.IsMatch(email))
                {
                    return new { code = 422, status = "Error", message = "Email match no pattern" };
                }
            }
            DateTime? birthDateTime = null;
            if (birthdate != null) 
            {
                try
                {
                    birthDateTime = DateTime.Parse(birthdate);
                }
                catch 
                {
                    return new { code = 422, status = "Error", message = "Birthdate unparseable" };
                }
            }

            Guid userId = Guid.Parse(
                HttpContext
                .User
                .Claims
                .First(c => c.Type == ClaimTypes.Sid)
                .Value);

            var user = _dataContext.Users.Find(userId);
            if (user == null)
            {
                return new { code = 403, status = "Error", message = "Forbidden" };
            }

            if (email != null)
            {
                user.Email = email;
            }
            if (name != null)
            {
                user.Name = name;
            }
            if (birthDateTime != null) 
            {
                user.Birthdate = birthDateTime;
            }

            await _dataContext.SaveChangesAsync();

            return new { code = 200, status = "OK", message = "Updated" };
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
 */
/* CRUD: Delete
 * Особливості видалення даних
 * ! видалення створює проблеми за наявності зв'язків між даними
 * - замість видалення вводиться мітка "видалено" (у вигляді дати-часу видалення)
 * ! Art. 17 GDPR "Право бути забутим" - необхідність видалення персональних
 *   даних на вимогу користувача
 * - Класифікувати дані на персональні / не персональні, одні - видаляти, інші
 *   залишати.
 *   
 * = розглядається два варіанти видалень
 *  soft-delete - помітка видалення і у випадку людини стирання персональних даних
 *  hard-delete - повне видалення - допускається лише за відсутності зв'язків
 */
