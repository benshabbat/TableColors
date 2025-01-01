using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ColorsTableNew.Models
{
    public class ColorModel
    {
        [Key]
        public int ColorID { get; set; }

        [DisplayName("Color Name")]
        [Required]
        public string ColorName { get; set; } = "";

        [DisplayName("Price")]
        [Required]
        public decimal Price { get; set; } = 0.0m;


        [DisplayName("Order")]
        [Required]
        public int DisplayOrder { get; set; } = 1;

        [DisplayName("In Stock?")]
        public bool IsInStock { get; set; } = false;
    }
}
