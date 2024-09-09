using ASP_P15.Data;
using ASP_P15.Models;
using ASP_P15.Models.Home;
using ASP_P15.Models.Shop;
using ASP_P15.Services.Hash;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P15.Controllers
{
    public class HomeController : Controller
    {
        // ������� �������� - _logger
        private readonly ILogger<HomeController> _logger;
        // ��������� ��� (���-) �����
        private readonly IHashService _hashService;
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;

        private String fileErrorKey = "file-error";
        private String fileNameKey  = "file-name";

        public HomeController(ILogger<HomeController> logger, IHashService hashService, DataContext dataContext, IKdfService kdfService)
        {
            _logger = logger;
            _hashService = hashService;
            _dataContext = dataContext;
            _kdfService = kdfService;
            /* �������� ����� ����������� - ������� �������������� ������. 
��������� ����� (��������) ������ ��������� ������������ � ��� 
��������� �� ����� �������� ��'���� (��������) �����
*/
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return RedirectToAction("Index", "Shop");
        }

        public IActionResult Profile()
        {
            // if (HttpContext.User.Identity?.IsAuthenticated == true)
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String sid = HttpContext
                    .User
                    .Claims
                    .First(c => c.Type == ClaimTypes.Sid)
                    .Value;

                return View(new ProfilePageModel()
                {
                    User = _dataContext
                        .Users
                        .Include(u => u.Feedbacks)
                            .ThenInclude(f => f.Product)
                        .First(u => u.Id.ToString() == sid),
                });
            }
            return RedirectToAction(nameof(this.Index));
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

            // �� ������� ���������� �� � ��������� ���� 
            if(HttpContext.Session.Keys.Contains("signup-data"))
            {
                // � ��� - �� ��������, ���������� ���
                var formModel = JsonSerializer.Deserialize<SignUpFormModel>(
                    HttpContext.Session.GetString("signup-data")!)!;

                model.FormModel = formModel;
                model.ValidationErrors = _Validate(formModel);

                if(model.ValidationErrors.Where(p => p.Value != null).Count() == 0)
                {
                    // ���� ������� �������� - �������� � ��
                    String salt = _hashService.Digest(Guid.NewGuid().ToString())[..20];
                    _dataContext.Users.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel.UserName,
                        Email = formModel.UserEmail,
                        Salt = salt,
                        Dk = _kdfService.DerivedKey(formModel.UserPassword, salt),
                        Registered = DateTime.Now,
                        Avatar = HttpContext.Session.GetString(fileNameKey)
                    });
                    _dataContext.SaveChanges();
                }


                ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";

                // ��'� ������������� ����� (��������) ����� ���������� � ���
                if (HttpContext.Session.Keys.Contains(fileNameKey))
                {
                    ViewData["avatar"] = HttpContext.Session.GetString(fileNameKey);
                    HttpContext.Session.Remove(fileNameKey);
                }

                // ��������� ��� � ���, ��� �������� ���������� ����������
                HttpContext.Session.Remove("signup-data");
            }
            return View(model);
        }

        public IActionResult Demo([FromQuery(Name="user-email")] String userEmail, [FromQuery(Name = "user-name")] String userName)
        {
            /* ������ ����� �� �����, ������ 1: ����� ��������� action 
             * ��'�������� ����������� ���������� �� ����� ����
             * <input name="userName"/> ------ Demo(String userName)
             * ���� � HTML ���������������� �����, �� �������� � C#
             * (user-name), �� �������� ������� [From...] �� ����������� ����
             * ����� �������� ����������
             * 
             * ������ 1 ��������������� ���� ������� ��������� �������� (1-2)
             * ����� �������������� ����� - ������������ �������
             */
            ViewData["data"] = $"email: {userEmail}, name: {userName}";
            return View();
        }

        public IActionResult RegUser(SignUpFormModel formModel)
        {
            HttpContext.Session.SetString("signup-data", 
                JsonSerializer.Serialize(formModel));
            
            if (formModel.UserAvatar != null)
            {
                // 1. ³��������� ���������� �����
                int dotPosition = formModel.UserAvatar.FileName.IndexOf('.');
                if (dotPosition == -1)  // ���� ���������� ����
                {
                    HttpContext.Session.SetString(fileErrorKey, 
                        "����� ��� ���������� �� �����������");
                }
                else
                {
                    String ext = formModel.UserAvatar.FileName[dotPosition..];
                    // 2. ��������� ���������� �� ������ ����������
                    if (ext == ".jpg" || ext == ".png" || ext == ".bmp")
                    {
                        // 3. ���������� ��'� �����, ������������, �� �� ������������� ������� ����
                        String filename;
                        String path = "./Uploads/"; //  "./wwwroot/img/upload/";
                        do
                        {
                            filename = Guid.NewGuid().ToString() + ext;
                        } while (System.IO.File.Exists(path + filename));
                        // 4. �������� ����, �������� � �� ��'� �����.
                        using Stream writer = new StreamWriter(path + filename).BaseStream;
                        formModel.UserAvatar.CopyTo(writer);
                        HttpContext.Session.SetString(fileNameKey, filename);
                    }
                    else
                    {
                        HttpContext.Session.SetString(fileErrorKey,
                        "���� �� ����������� ����������");
                    }
                }
                
                
            }
            
            return RedirectToAction(nameof(SignUp));

            // ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";
            // return View("Demo");
            /* ��������: ���� ������� ���������� ����� �������� �����, ��
             * �� ��������� � �������
             * �) ���� �����������, �� ��� �� �� ��������
             * �) �������� ������ ��� �����, �� ���� ��������� �� 
             *     ���������� ����� � ��, �����, ����
             * г�����: "�������� �����" - ������������� ������ �� 
             *  �����'����������� �����
             *  
             *  Client(Browser)                    Server(ASP)
             *  [form]--------- POST RegUser --------->  [form]---Session
             *  <-------------- 302 SignUp -----------             |
             *  --------------- GET SignUp ----------->            |
             *   <-----------------HTML----------------------- ����������
             *   
             * ��������� �� ������������ ���� - ���. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state  
             */
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Download([FromRoute] String id)
        {
            // id - ��������� � ������������� �����, ���� - ��'� �����
            id = id.Replace('_', '/');
            String filename = $"./Uploads/{id}";
            if(System.IO.File.Exists(filename))
            {
                var stream = new StreamReader(filename).BaseStream;
                return File(stream, "image/png");
            }
            return NotFound();
        }


        private Dictionary<String, String?> _Validate(SignUpFormModel model)
        {
            /* �������� - �������� ����� �� ���������� ������ ��������/��������
             * ��������� �������� - {
             *   "UserEmail": null,          null - �� ������ ������ ��������
             *   "UserName": "Too short"     �������� - ����������� ��� �������
             * }
             */
            Dictionary<String, String?> res = new();

            var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            res[nameof(model.UserEmail)] =
                String.IsNullOrEmpty(model.UserEmail)
                ? "�� ����������� ������ ����"
                : emailRegex.IsMatch(model.UserEmail)
                    ? null
                    : "������ �������� ������";
            
            var nameRegex = new Regex(@"^\w{2,}(\s+\w{2,})*$");
            res[nameof(model.UserName)] =
                String.IsNullOrEmpty(model.UserName)
                ? "�� ����������� ������ ����"
                : nameRegex.IsMatch(model.UserName)
                    ? null
                    : "������ �������� ��'�";

            if (String.IsNullOrEmpty(model.UserPassword))
            {
                res[nameof(model.UserPassword)] = "�� ����������� ������ ����";
            }
            else if(model.UserPassword.Length < 3)
            {
                res[nameof(model.UserPassword)] = "������ �� ���� �� �������� �� 8 �������";
            }
            else 
            {
                List<String> parts = [];
                if (!Regex.IsMatch(model.UserPassword, @"\d"))
                {
                    parts.Add(" ���� �����");
                }
                if (!Regex.IsMatch(model.UserPassword, @"\D"))
                {
                    parts.Add(" ���� �����");
                }
                if (!Regex.IsMatch(model.UserPassword, @"\W"))
                {
                    parts.Add(" ���� ����������");
                }
                if (parts.Count > 0)
                {
                    res[nameof(model.UserPassword)] = 
                        "������ ������� ������ ����������" + String.Join(',', parts);
                }
                else
                {
                    res[nameof(model.UserPassword)] = null;
                }                
            }


            res[nameof(model.UserRepeat)] = model.UserPassword == model.UserRepeat
                ? null
                : "����� �� ���������";


            res[nameof(model.IsAgree)] = model.IsAgree 
                ? null 
                : "��������� �������� ������� �����";

            // ���������� �������� ����� �������� � ���
            if (HttpContext.Session.Keys.Contains(fileErrorKey))
            {
                res[nameof(model.UserAvatar)] = 
                    HttpContext.Session.GetString(fileErrorKey);
                HttpContext.Session.Remove(fileErrorKey);
            }

            return res;
        }
    }
}
