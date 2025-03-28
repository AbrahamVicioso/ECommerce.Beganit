namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class UploadProductImageViewModel
    {
        public int ProductId { get; set; }

        public IFormFile Image { get; set; }

        public int ImageId { get; set; }

        public string ImageUrl { get; set; }

        public bool IsPrimary {  get; set; }

        public int DisplayOrder { get; set; }

        public string AltText { get; set; }
    }
}
