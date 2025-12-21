using System.ComponentModel.DataAnnotations;

namespace FastFoodWeb.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên khách hàng")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng cung cấp số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string Address { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalPrice { get; set; }

        public string? Note { get; set; } // Ghi chú thêm (ví dụ: không lấy tương ớt)
    }
}