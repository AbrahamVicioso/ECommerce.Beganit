namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public int Rating { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public string Sku { get; set; }

        public decimal RegularPrice { get; set; }

        public decimal? DiscountPrice { get; set; }

        public DateTime? DiscountStartDate { get; set; }

        public DateTime? DiscountEndDate { get; set; }

        public int? Quantity { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public int? BrandId { get; set; }

        public bool IsActive { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public ICollection<string> Categories { get; set; }

        public ICollection<ProductImageViewModel> Images { get; set; }
    }
}
