namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ProductBundleItemViewModel
    {
        public int BundleId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public ProductViewModel Product{ get; set; }
    }
}
