namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }

        public int? CustomerId { get; set; }

        public int Rating { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool? IsVerifiedPurchase { get; set; }

        public bool? IsApproved { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
