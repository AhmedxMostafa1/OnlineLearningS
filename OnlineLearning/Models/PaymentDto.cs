namespace OnlineLearning.Models.DTOs
{
    public class PaymentDto
    {
        public int CourseId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Credit Card";
        public string CardNumber { get; set; } = "";
        public string ExpiryDate { get; set; } = "";
        public string CVV { get; set; } = "";
        public string CardHolderName { get; set; } = "";
    }
}
