using ASP_P15.Data;
using ASP_P15.Data.Entities;
using ASP_P15.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_P15.Controllers
{
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        [HttpGet]
        public async Task<RestResponse<List<Feedback>>> DoGet()
        {
            List<Feedback> feedbacks = await _dataContext.Feedbacks.ToListAsync();

            return new()
            {
                Meta = new()
                {
                    Service = "Feedback",
                    Count = feedbacks.Count,
                },
                Data = feedbacks
            };
        }

        [HttpPost]
        public async Task<RestResponse<String>> DoPost([FromBody] FeedbackFormModel model)
        {
            _dataContext.Feedbacks.Add(new()
            {
                UserId = model.UserId,
                ProductId = model.ProductId,
                Text = model.Text,
                Rate = model.Rate,
            });
            await _dataContext.SaveChangesAsync();

            return new()
            {
                Meta = new()
                {
                    Service = "Feedback",
                },
                Data = "Created",
            };
        }
    }
}
/* Д.З. Додати до відгуків (Feedback) мітку часу:
 * - розширити Entity
 * - зробити міграцію, застосувати її
 * - розшити форму прийому даних (FormModel)
 * - передати дані від JS
 * - переконатись у внесенні даних до БД
 */
