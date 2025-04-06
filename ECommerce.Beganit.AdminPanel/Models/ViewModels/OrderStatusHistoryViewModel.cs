namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class OrderStatusHistoryViewModel
    {
        public int Id { get; set; }

        public int? OrderId { get; set; }

        public int? OrderStatusId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? CreatedBy { get; set; }
    }
}
