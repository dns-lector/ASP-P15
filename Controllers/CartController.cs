using ASP_P15.Data;
using ASP_P15.Models.Api;
using ASP_P15.Models.Cart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P15.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpPost]
        public async Task<RestResponse<String>> DoPost(
                            [FromBody] CartFormModel formModel)
        {
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Cart",
                },
            };
            if(formModel.UserId == default)
            {
                response.Data = "Error 401: Unauthorized";
                return response;
            }
            if (formModel.ProductId == default)
            {
                response.Data = "Error 422: Missing Product Id";
                return response;
            }
            if (formModel.Cnt <= 0)
            {
                response.Data = "Error 422: Positive Cnt expected";
                return response;
            }
            // Чи є у користувача відкритий кошик? Якщо є, то додаємо
            // товари до нього, якщо немає, то створюємо і додаємо до нового
            var cart = _dataContext
                .Carts
                .FirstOrDefault(c =>
                    c.UserId == formModel.UserId &&
                    c.CloseDt == null &&
                    c.DeleteDt == null);

            if (cart == null)   // немає відкритого кошику, треба створювати
            {
                Guid cartId = Guid.NewGuid();
                _dataContext.Carts.Add(new()
                {
                    Id = cartId,
                    UserId = formModel.UserId,
                    CreateDt = DateTime.Now,
                });
                _dataContext.CartProducts.Add(new()
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    ProductId = formModel.ProductId,
                    Cnt = formModel.Cnt,
                });
            }
            else   // є відкритий кошик, треба додавати до нього
            {
                // треба перевірити, чи є вже такий товар у кошику,
                // якщо є, то збільшити кількість, якщо немає, то додати
                var cartProduct = _dataContext
                    .CartProducts
                    .FirstOrDefault(cp =>
                        cp.CartId == cart.Id &&
                        cp.ProductId == formModel.ProductId);

                if (cartProduct == null)   // такого товару немає в кошику
                {
                    _dataContext.CartProducts.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = formModel.ProductId,
                        Cnt = formModel.Cnt,
                    });
                }
                else  // такий товар є в кошику
                {
                    cartProduct.Cnt += formModel.Cnt;
                }
            }
            await _dataContext.SaveChangesAsync();
            response.Data = "Added";
            return response;
        }
    }
}
/* Реалізувати виведення повідомлень щодо успішності додавання
 * товару до кошику (додано успішно / помилка додавання).
 * ** Також виводити кількість товару у кошику:
 *     Додано успішно, у кошику 3 шт обраних вами товарів (всього - 10)
 */
