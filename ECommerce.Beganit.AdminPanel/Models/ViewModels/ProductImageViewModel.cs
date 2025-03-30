namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ProductImageViewModel
    {
        public string ImageUrl { get; set; }

        public string AltText { get; set; }

        public bool? IsPrimary { get; set; }

        public int? DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
