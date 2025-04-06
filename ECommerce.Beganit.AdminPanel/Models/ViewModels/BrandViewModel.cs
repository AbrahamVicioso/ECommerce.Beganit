using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class BrandViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand Name is required")]
        [StringLength(100, ErrorMessage = "Brand Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; }

        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; }

        [Display(Name = "Created At")]
        [DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}