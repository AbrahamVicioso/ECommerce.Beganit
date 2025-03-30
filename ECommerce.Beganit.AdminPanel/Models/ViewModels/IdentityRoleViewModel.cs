using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class IdentityRoleViewModel
    {
        public string Id { get; set; } = default!;

        public string? Name { get; set; }

        public string? NormalizedName { get; set; }

        public string? ConcurrencyStamp { get; set; }
    }
}
