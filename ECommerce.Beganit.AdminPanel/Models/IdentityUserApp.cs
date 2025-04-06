using Microsoft.AspNetCore.Identity;

namespace ECommerce.Beganit.AdminPanel.Models
{
    public class IdentityUserApp : IdentityUser
    {
        public string UrlProfileImage;
    }
}
