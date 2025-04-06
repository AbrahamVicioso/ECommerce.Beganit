namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }

        public int? CartId { get; set; }

        public int? ProductId { get; set; }

        public int? VariantId { get; set; }

        public int Quantity { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ProductViewModel? Product { get; set; }

        public ProductVariantViewModel? Variant { get; set; }
    }
}
