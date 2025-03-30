using Microsoft.AspNetCore.Identity;

namespace ECommerce.Beganit.AdminPanel.Models
{
    public class IdentityUserCustom : IdentityUser
    {
        public string UserName;
        public string UrlProfileImage;
    }
}
