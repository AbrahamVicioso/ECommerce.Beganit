namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class CartViewModel
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public string SessionId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
    }
}
