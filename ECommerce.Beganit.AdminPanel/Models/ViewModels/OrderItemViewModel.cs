namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class OrderItemViewModel
    {
        public int Id { get; set; }

        public int? OrderId { get; set; }

        public int? ProductId { get; set; }

        public int? VariantId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
