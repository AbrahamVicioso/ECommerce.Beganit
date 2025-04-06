using System.ComponentModel.DataAnnotations;

namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ProductVariantViewModel
    {

        public int Id { get; set; }

        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [StringLength(50, ErrorMessage = "SKU must be at most 50 characters.")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
        public int Quantity { get; set; }

        public bool IsActive { get; set; }

        public List<ProductVariantAttributeViewModel>? ProductVariantAttributes { get; set; }
    }
}
