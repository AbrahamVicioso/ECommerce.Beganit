using ECommerce.Beganit.AdminPanel.Models;
using ECommerce.Beganit.AdminPanel.Models.ViewModels;
using Riok.Mapperly.Abstractions;

namespace ECommerce.Beganit.AdminPanel.Mapper
{
    [Mapper]
    public partial class CategoryMapper
    {
        public partial Category CategoryToCategoryViewModel(CategoryViewModel model);   
    }
}
