namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public string OrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public int? ShippingAddressId { get; set; }

        public int? BillingAddressId { get; set; }

        public int? OrderStatusId { get; set; }

        public int? PaymentStatusId { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public string TrackingNumber { get; set; }

        public string Notes { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
