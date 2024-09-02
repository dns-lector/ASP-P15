namespace ASP_P15.Models.Api
{
    public class FeedbackFormModel
    {
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public String Text { get; set; }
    }
}
