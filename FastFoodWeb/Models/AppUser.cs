using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FastFoodWeb.Models
{
    public class AppUser : IdentityUser
    {
        [StringLength(100)]
        [MaxLength(100)]
        [Required]
        public string? FullName { get; set; } // Thêm cột Họ và Tên

        [StringLength(200)]
        public string? Address { get; set; }  // Thêm cột Địa chỉ (tiện cho việc tự điền khi đặt hàng)
    }
}