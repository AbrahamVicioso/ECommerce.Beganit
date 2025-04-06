using Microsoft.AspNetCore.Identity;

namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class UserRolesViewModel
    {
        public IdentityUser User { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
