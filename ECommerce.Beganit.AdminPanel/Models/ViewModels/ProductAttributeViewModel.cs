namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class ProductAttributeViewmodel
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }

        public string AttributeName { get; set; }

        public string AttributeValue { get; set; }
    }
}
