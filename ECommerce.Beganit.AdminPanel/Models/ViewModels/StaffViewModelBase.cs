namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class StaffViewModelBase
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
