using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodWeb.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; } // Khóa chính cho Database

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        public string? UserId { get; set; } // Lưu ID của người dùng (để phân biệt giỏ hàng ai nấy giữ)

        [NotMapped] // Không lưu cột này vào DB, chỉ để tính toán hiển thị
        public decimal Total => Price * Quantity;
    }
}