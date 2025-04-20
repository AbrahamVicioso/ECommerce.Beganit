namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string UserType { get; set; }

        public string Action { get; set; }

        public string EntityName { get; set; }

        public int? EntityId { get; set; }

        public string OldValues { get; set; }

        public string NewValues { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
