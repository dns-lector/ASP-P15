﻿namespace ASP_P15.Models.Api
{
    public class FeedbackFormModel
    {
        public Guid? ProductId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? EditId { get; set; }
        public String Text { get; set; }
        public int Rate { get; set; } = 5;
    }
}
