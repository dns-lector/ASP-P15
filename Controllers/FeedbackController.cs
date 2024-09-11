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
                UserId = model.UserId!.Value,
                ProductId = model.ProductId!.Value,
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

        [HttpPut]
        public async Task<RestResponse<String>> DoPut([FromBody] FeedbackFormModel model)
        {
            // готуємо відповідь
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Feedback",
                },
            };
            // шукаємо відгук за editId
            var feedback = _dataContext.Feedbacks.Find(model.EditId!.Value);
            if(feedback == null)               // Якщо не знаходимо
            {
                response.Data = "Not Found";   // то повертаємо відповідне повідомлення
            }
            else                               // Якщо знаходимо
            {
                feedback.Text = model.Text;    // то вносимо відповідні зміни 
                feedback.Rate = model.Rate;    // та зберігаємо
                await _dataContext.SaveChangesAsync();
                response.Data = "Updated";
            }
            return response;
        }

        [HttpDelete]
        public async Task<RestResponse<String>> DoDelete([FromQuery] String id)
        {
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Feedback",
                },
            };
            if (String.IsNullOrEmpty(id))
            {
                response.Data = "Error 400: id parameter is null or empty";
                return response;
            }
            Guid guid;
            try { guid = Guid.Parse(id); }
            catch 
            {
                response.Data = "Error 422: id parameter is not valid UUID";
                return response;
            }
            var feedback = _dataContext.Feedbacks.Find(guid);
            if(feedback == null)
            {
                response.Data = "Error 404: id parameter does not identify feedback";
                return response;
            }
            if (feedback.DeleteDt != null)
            {
                response.Data = "Error 409: id parameter identifies already deleted feedback";
                return response;
            }
            feedback.DeleteDt = DateTime.Now;
            response.Data = "Deleted";
            await _dataContext.SaveChangesAsync();
            return response;
        }

        public async Task<RestResponse<String>> DoOther()
        {
            if(Request.Method == "RESTORE")
            {
                return await DoRestore();
            }
            throw new NotImplementedException();
        }

        private async Task<RestResponse<String>> DoRestore()
        {
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Feedback"
                }
            };
            String id = Request.Query["id"].ToString();
            if (String.IsNullOrEmpty(id))
            {
                response.Data = "Error 400: id parameter is null or empty";
                return response;
            }
            Guid guid;
            try { guid = Guid.Parse(id); }
            catch
            {
                response.Data = "Error 422: id parameter is not valid UUID";
                return response;
            }
            var feedback = _dataContext.Feedbacks.Find(guid);
            if (feedback == null)
            {
                response.Data = "Error 404: id parameter does not identify feedback";
                return response;
            }
            if (feedback.DeleteDt == null)
            {
                response.Data = "Error 409: id parameter identifies not deleted feedback";
                return response;
            }
            feedback.DeleteDt = null;
            response.Data = "Restored";
            await _dataContext.SaveChangesAsync();
            return response;
        }
    }
}
/* Д.З. Додати до відгуків, які відображаються у профілі користувача
 * - дату/час створення (* в інтелектуальній формі: якщо сьогодні, 
 *     то відображати тільки час, якщо вчора, то так і писати "вчора",
 *     для решти - N днів тому)
 * - дату/час видалення - для видалених відгуків
 * - рейтинг відгуку у вигляді "зірок"
 */
