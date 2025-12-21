namespace FastFoodWeb.Models
{
    using System.ComponentModel.DataAnnotations;
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
