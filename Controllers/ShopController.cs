using ASP_P15.Data;
using ASP_P15.Models.Shop;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P15.Controllers
{
    public class ShopController(DataContext dataContext) : Controller
    {
        private readonly DataContext _dataContext = dataContext;

        public IActionResult Index()
        {
            ShopPageModel model = new()
            {
                ProductGroups = _dataContext
                    .Groups
                    .Where(g => g.DeleteDt == null)
            };
            return View(model);
        }

        public IActionResult Group(String id)
        {
            // Розглядаємо можливість, що id - це або slug, або id
            ViewData["id"] = id;
            return View();
        }

    }
}

/* Slug - ідентифікатор ресурсу (сторінки), сформульований, як правило,
 * зрозумілою для людини мовою
 */
