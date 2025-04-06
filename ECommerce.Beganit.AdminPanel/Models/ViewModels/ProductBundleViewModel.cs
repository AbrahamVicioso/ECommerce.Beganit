namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ProductBundleViewModel
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal? DiscountPercentage { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<ProductBundleItemViewModel>? Items { get; set; }
    }
}
